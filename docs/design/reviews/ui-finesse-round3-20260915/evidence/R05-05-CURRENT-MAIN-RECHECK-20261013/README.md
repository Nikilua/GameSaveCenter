# R05-05 当前 main 三态复选框消费点复核

日期：2026-10-13（Asia/Shanghai）
代码基线：`fbf504d2a7aabcd322c3c735959a31c7e1679fc9`（main）。本次没有生产代码、UI、ViewModel、服务或 DTO 变更。

## 适用性结论

R05-05 继续记为“不适用”。该项讨论的是用于媒体批量结果的全选、半选和未选集合，以及筛选/追加/部分失败时的集合范围。当前 `MediaCenterView` 的媒体收件箱通过 `DataGrid.SelectionMode=Extended`、`SelectionUnit=FullRow` 和 `MediaInboxGrid.SelectedItems` 命令参数提供多选；当前生产 XAML 没有对应的批量三态 CheckBox。

当前唯一显式 `IsThreeState="True"` 的生产仓库 XAML 命中位于 `Views/Development/UiFrameworkProbeView.xaml` 的“校对半选框”，属于开发校对夹具。共享 `GscCheckBox`/`GscDataGridCheckBox` 模板会在 `IsChecked=null` 时显示 `IndeterminateMark`，但这只提供控件视觉状态；共享模板本身没有媒体批量集合、页/全部结果作用域、筛选和追加状态。

## 当前 main 源码核对

| 文件 | SHA-256 | 核对结果 |
| --- | --- | --- |
| `src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml` | `31E8EC467E5FF4BC5C79617C4406C60D0E4DD4E81BC466D7A89A9448DE791C08` | 媒体收件箱 Extended 行选择；批量命令接收 `SelectedItems` |
| `src/GameSaveCenter.Playnite/Views/Development/UiFrameworkProbeView.xaml` | `5A67C2195C663DB99A675E2CD029CE6ED8D0E4DA90CC1DB9C8ED050140358E3D` | 唯一显式三态样例是开发校对页的静态“半选”控件 |
| `src/GameSaveCenter.Playnite/Themes/DesignTokens.xaml` | `2FB7931D4F5C7E741484F5F7D59A68785CCD9F7F69C85BC9D2C8AE26CC4EE31B` | 共享模板提供 null 对应的短横标记 |
| `tests/GameSaveCenter.Playnite.Tests/UiFinesseRound2ControlSourceTests.cs` | `B2717D805C550ADB789F77D4E47BF130ABB0458FBCB41A977DEF3637D6FB557C` | `SharedCheckboxExposesAnExplicitIndeterminateMark` 只检查资源字符串/Trigger 接线 |

代码后置扫描没有找到生产 `IsThreeState` / null `IsChecked` 集合消费逻辑。其他生产 CheckBox 用于标量布尔设置、版本锁定、建议项纳入等用途，不表示“当前页”或“全部结果”的批量选择集合。

## 本机验证

- 本机隔离 SDK `8.0.423`；使用 `scripts/build.ps1 -Configuration Release -SkipTests`，XAML structural validation `24/24`，Release solution build `0 warning / 0 error`。生产插件目标 `net462`，Playnite 测试目标 `net472`。
- `UiFinesseRound2ControlSourceTests` `24 passed / 0 failed / 0 skipped`。脱敏 TRX：[R05-05-current-main.trx](tests/R05-05-current-main.trx)，SHA-256 `E6BD5F4FDF559CA1589ED45385538908D04052F9314EBFD6C2AB32D0AB7D9BAE`。
- 首次直接 `dotnet build` 未注入 `GscBuildCommit`，因此源契约测试按仓库身份规则拒绝读取源码（`0/24` 执行通过）；随后按官方脚本重建带当前 HEAD 身份的程序集并重跑，最终结果如上。首次运行不是产品断言失败。
- `python scripts/validate-source.py` 通过。仅为适用性审查，没有改生产视觉，因此没有运行 RenderHarness。

该测试组的半选用例确实是静态字符串断言，不能证明 Space 输入、UIA/Narrator 状态或媒体集合行为；本项不以它签收这些交互。媒体批量集合、筛选和追加保留由 R05-04 的 STA WPF DataGrid 行为证据覆盖。

## Actions 与未验证边界

- main commit `fbf504d2` 对应 Windows 构建验证 #943：run `38284177732`、job `114928442567`，状态 `failure`，总时长 `6m13s`；公开摘要显示“编译与测试”exit code 1。artifact `11762189156`，477 KB，SHA-256 `bb2ffc75a4f111adffa061f46440af303131d14bfd6b52f2d6642b6b9a76176d`。
- job 页面要求登录查看完整日志；匿名 logs 与 artifacts API 请求均为 HTTP 403。本机没有取得完整失败日志，故失败步骤内部错误、CI 实际 SDK 和根因仍未知；用户要求跟踪的 CI SDK `10.0.401` 与本机 SDK `8.0.423` 分开记录，不凭 exit code 盲改。
- 本次没有启动真实 Playnite或发送真实 OS Space 输入；未测 UIA/Narrator、物理 DPI/多屏、最终呈现帧、ETW 或宿主性能。没有访问/改写真实存档、媒体、云端或用户配置。

若后续新增面向批量结果的三态复选框，应重新打开本项，先规定选择作用域和集合来源，再补全选/半选/空选、筛选、分页追加、部分失败、Space 与 UIA 的行为和负例。

下一项：复核 R05-06 当前 main 的开关模型/视觉/持久化/回滚行为；已有实现优先复用，不重建设置保存流程。
