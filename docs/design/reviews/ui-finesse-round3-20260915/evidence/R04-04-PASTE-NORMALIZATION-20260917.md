# R04-04 粘贴标准化证据

## 结论

R04-04 在当前生产输入范围内已满足。实现提交为 `d7e92f1467648f045ff94fb30da5ac9712cd402b`，已推送到 `origin/codex/ui-finesse-round2`。

本阶段先核对最新代码，没有把 `main` 的旧实现覆盖到当前分支。当前没有可编辑端口字段，也没有独立命名为 Exclude 的设置字段；实际相关输入是设置页的本地路径/云端目标，以及 MediaCenter 的自定义媒体目录和文件模式。文件模式沿用现有 `CustomMediaPattern`，以 `ExcludePattern` 输入类型接入同一标准化合同，没有新增排除规则业务。

## 实现范围

- 新增共享 `PasteNormalization` WPF attached behavior，按 `Path`、`RemoteTarget`、`Port`、`ExcludePattern` 类型识别输入语义。当前生产只接入实际存在的 Path/RemoteTarget/媒体 Path/Pattern；Port 类型保留给未来真实端口字段，不虚构 UI。
- 对单值粘贴去除外层空白；外层带成对单/双引号时去除引号并再次去除外层空白；单值末尾的 shell 换行属于外层空白，会被去除。内部仍存在换行的多行内容直接拒绝，不将多个路径折叠成一个隐形无效路径。
- 标准化通过 `TextBox.SelectedText` 写入，保留 WPF 原生 Undo 单元；剪贴板从不被改写。attached behavior 在内存中保留本次原始粘贴文本和结果说明，并把“已标准化”或“已阻止多行粘贴”反馈到 Tooltip/Automation HelpText；提示明确说明可用 Ctrl+Z 恢复粘贴前字段值。
- 原有设置 Binding、Playnite 保存/取消、媒体来源命令、游戏选框、滚动条、错误/取消/恢复保护和有限列表语义未改；实现保持 `GameSaveCenter.Playnite` `net462`。

## 自动与行为验证

- `PasteNormalizationTests`、`PasteNormalizationSourceTests` 和 `SettingsValidationSourceTests` 定向合计 `8/8`：覆盖路径外层空白/引号/尾随 shell 换行、真实多行拒绝负例、端口/模式/云端目标类型、WPF Pasting/Undo、多行字段保护、Tooltip/Automation 反馈及生产 XAML 接入。
- clean Release 构建：XAML `24/24`，`0` warning / `0` error。
- `python scripts/validate-source.py`：通过。

## 双主题受控视觉证据

- 使用默认 `tests/GameSaveCenter.RenderHarness/bin/Release/net472` 入口运行单次 RenderHarness，避免隔离输出改变既有仓库根定位；Worker 仅作为合成设置夹具，未连接真实存档/媒体/云端。
- `.tmp/r04-04-render-clean-final/render-qa-report.txt` 绑定完整 SHA `d7e92f1467648f045ff94fb30da5ac9712cd402b`，`WorkingTreeClean=True`，`Themes=light,dark`，`DpiScale=1.00 (offscreen logical DIP)`，数据量包含 `50/400/2000/4468`，297 张 PNG，最终 `render-qa OK`。
- 设置 normal/dirty/invalid 状态探针分别为“已保存/有未保存更改/存在校验错误”，摘要可见性分别为 `False/False/True`。人工抽查 Settings 与 Media 来源 `1040×700` 图，标准化提示换行可读，输入区没有新增横向溢出；媒体来源表面继续使用既有滚动结构。

## 边界

证据来自合成文本、合成设置/fake Worker、隔离目录、STA WPF 和 offscreen logical DIP。没有自动操作真实 Playnite 剪贴板/输入链、Windows IME/物理键盘、屏幕阅读器、宿主字体替换、物理 DPI/跨屏、presented frame、ETW 或宿主性能；没有真实端口/排除设置可供宿主验证。真实 Playnite 嵌入中的系统粘贴路由和宿主 Tooltip/Automation 呈现仍需在隔离宿主复测。未写真实存档、删除真实媒体、写用户云端或发送诊断。

## 下一步

下一可执行任务为 R04-05 数字输入边界；R04-04 的 Port 类型可在出现真实端口字段时复用，不提前扩展业务模型。
