# R04-05 数字输入边界（2026-09-17）

## 结论

R04-05 在当前生产字段范围内已满足：数字编辑器现在统一复用 `IntegerRangeValidationRule` 做空值、整数范围、上下界校验，并由共享 `GscNumericTextBox` 接入滚轮步进。非法当前值、溢出值和越界候选都不会被静默 clamp；越界滚轮事件保留为未处理，使页面现有滚动系统继续接管。

## 现状核对与实现

- 最新代码原本已有 `GscNumericTextBox`、设置页整数规则和服务侧安全边界；本阶段没有从 `main` 带入旧实现，也没有新增不存在的重试次数、端口或容量 DTO/字段。
- 设置页已有的整数规则扩展为共享 `ValidateValue`：空白输入、非整数和 `Int32` 溢出分别返回可理解的错误；最小值和最大值继续由字段绑定声明。
- 存档策略页的游玩中间隔与保留周期模板，以及 Trainer 启动延迟，接入了同一套验证规则。当前范围为：间隔 `1–1440` 分钟、启动延迟 `0–300` 秒、保留周期为 `0–2147483647`；绑定的 `ValidatesOnExceptions`/`NotifyOnValidationError` 保持错误状态可见。
- `NumericInput` attached behavior 只对共享数字 TextBox 生效。滚轮先用该 TextBox 的绑定规则验证当前文本，再以 ±1 计算候选；当前值非法、候选越界或更新源出现验证错误时保持原文本/源值不变，不做静默 clamp。合法步进才更新文本与源，并将滚轮标记为已处理。
- 普通键盘编辑和粘贴仍由同一个 WPF binding validation 链处理；滚轮路径额外强制 `UpdateSource`，因此三种入口共享同一范围规则。没有改动命令绑定、取消/错误语义、游戏选框或滚动条系统。

## 验证证据

- 代码提交：`8d56eb5a050a5c2db2718a42cf928b7d43d6f897`（`补齐数字输入边界校验`），提交后工作树 clean。
- clean commit 定向测试：`NumericInputTests`、`SettingsValidationSourceTests`、`PasteNormalizationTests` 与 `PasteNormalizationSourceTests` 合计 `20/20` 通过；其中实际 STA WPF Window 测试覆盖合法滚轮更新、最大值越界不改值/不吞事件、溢出当前值和非法文本负例。
- Release 结构校验覆盖 `24/24` 个 XAML，构建 `0 warnings / 0 errors`，`python scripts/validate-source.py` 通过，`git diff --check` 通过。
- clean RenderHarness 报告绑定完整提交，双主题、工作树 clean、合成 fixture 与 `50/400/2000/4468` 滚动探针共生成 `297` 张 PNG，最终为 `render-qa OK`。`Save-1040x700-tab2.png` 人工抽查显示间隔值、`1–1440 分钟`帮助文案和现有滚动条均可读，未见横向溢出。

## 证据边界与后续

测试使用合成设置/fake 服务、隔离目录、STA WPF 和 offscreen logical DIP；没有写入真实存档、媒体或云端。尚未在真实 Playnite 嵌入宿主中验证物理键盘/系统剪贴板/系统滚轮、IME、屏幕阅读器、物理 DPI/跨屏、presented frame、ETW 或宿主性能，因此不将离屏结果表述为这些环境已验证。当前生产模型没有独立可编辑端口、排除模式、重试次数或容量字段，未虚构对应 UI。下一可执行任务为 R04-06 清空与撤销。
