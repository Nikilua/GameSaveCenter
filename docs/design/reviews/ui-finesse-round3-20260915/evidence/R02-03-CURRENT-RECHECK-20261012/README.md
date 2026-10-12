# R02-03 当前 main 复验

日期：2026-10-12。复核对象为已实现的 R02-03“禁用原因可达”。本阶段未修改生产 UI、ViewModel 或命令；只加强 WPF 行为测试并在当前 main 构建上补测。R02-03 仍记为“已满足”，本记录不扩展为真实宿主验收。

## 当前行为证据

- 当前提交基线为 `da6388e814309825141fb530ecda702cb7e11eab`。隔离 Release solution 构建使用本机 SDK `8.0.423`，XAML `24/24`，`0 warning / 0 error`。构建时只有 `tests/GameSaveCenter.Playnite.Tests/R02ActionAvailabilityHintTests.cs` 未提交，SHA-256 为 `9AE4C53C0705D1D60EE9ABAA10BBB289F4B3BFE4142C595CB4D2D6B73565D0F7`；因此程序集身份按 commit 与该测试文件分别记录，不能称为 clean-tree 构建。
- R02 定向 WPF/source 组合 `5/5`：原四项状态文案/安全边界和提示样式测试，加上生产 `SaveCenterView` 紧凑布局把恢复提示移入可达检查器的行为用例。加强后的 STA 用例实际调用 `Focus()` 并核对 `Keyboard.FocusedElement`，从 UIA peer 读取 `Name`、`HelpText` 并与屏幕提示文本比对；不是仅检查 XAML 字符串或 peer 是否存在。TRX：[R02-03-current.trx](tests/R02-03-current.trx)。
- 保存提示说明在宽态与紧凑态的真实生产 `SaveCenterView` 中分别可见/折叠；紧凑态检查器与入口可见，且提示不会占用历史列表行。多状态测试仍保留 Restore、Media Inbox、Cloud Transfer 和 Remote Restore 的缺项、离线、配置、暂存/校验和失败/重试语义；未改变任何危险命令门禁。

## 共享 UI 质量检查

- 当前身份 RenderHarness Release 构建 `0 warning / 0 error`，全量离屏 `render-qa OK`，输出 `372` 张 Light/Dark 样例，logical DPI `1.00`。报告记 `Commit=da6388e...`、`WorkingTreeClean=False`（上述测试文件未提交）。这是一轮通用主题/布局回归；默认样例没有强制所有禁用原因状态，因此没有把其截图当作本项的禁用态视觉证明，也没有将离屏渲染写成 Playnite 或物理 DPI 结果。
- `scripts/validate-source.py` 通过。WPF 静态审查扫描 `36` 个 XAML，`0 errors / 29 warnings / 177 info`；本阶段没有 XAML 或生产样式改动，扫描提示未归因于本次测试改动。完整 `render-qa` 图像位于可再生 `.tmp`，本包只保留与本项直接相关的 TRX、身份摘要和结果文本。
- 关键程序集 SHA-256、MVID、源码/测试 dirty 状态见 [build-identity.json](build-identity.json)。

## 最新 CI 与未验边界

- 推送 `da6388e8` 的 Actions run `38226955461`（run 931）终态为 failure；Jobs API 仅确认“编译与测试”步骤失败，SDK 为 CI `10.0.401`。完整日志 API 返回 HTTP 403 `Must have admin rights to Repository.`；唯一诊断 artifact ID `11735869371`（488,867 bytes）可列出但下载返回 HTTP 401。失败命令、断言、堆栈与 SDK 诊断不可读，CI 根因未知；本阶段没有凭退出码盲改。
- 未生成或安装扩展包；没有运行真实 Playnite，也未验证 Narrator/OS UIA、OS 输入/IME、物理 DPI/跨屏、Worker 真实时序、ETW 或最终呈现帧。Demo 原目录不可用；本阶段没有改变视觉体系，沿用当前生产基线。
- R02-03 的受控键盘/UIA 行为与紧凑布局条件有当前 main 证据，真实 Playnite、屏幕阅读器与包身份仍单列待验。下一可执行项：R02-04 当前 main 图文光学对齐复验；先核对现有 `R02OpticalAlignmentTests` 与共享按钮当前实现。
