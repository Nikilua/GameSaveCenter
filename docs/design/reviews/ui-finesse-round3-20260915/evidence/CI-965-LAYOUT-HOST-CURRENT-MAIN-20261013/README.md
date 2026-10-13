# Actions #965 布局测试宿主校正

日期：2026-10-13

## #965 原始失败

用户提供的诊断包与 Actions #965 / run `38342488530` 一致：包 SHA-256 `328C86FE4C908C469D84995F9B01114E9FF89BCD1692362FBDA7B9520E461D36`，artifact ID `11788460821`。源码提交是 `d0dbaea00dc620545524236c55444748d43d6540`，工作树 clean；包记录 CI SDK `8.0.423`、VSTest `17.11.1`。

Release solution build 成功，`0 warning / 0 error`。随后 `ReportedWorkspaceLayoutBehaviorTests` 共 `54` 项，`50` 通过、`4` 失败；R08 数字反馈类为 `5/5`。四个失败是两项测试的浅/深主题实例：

| 测试 | CI 实测 | 失败条件 |
| --- | --- | --- |
| `CompactInboxKeepsBatchButtonsCompactAndTheGridInsideItsFrameRow` | 收件箱按钮与全局目标中心差 `42/42/0 DIP`；按钮、模式和目标高度均 `36 DIP` | 跨行按钮仍与目标摘要比较中心位置 |
| `SettingsHeaderAndPathActionsStayAnchoredToTheirLabelsAndEachOther` | `wide shell/search=952/0 DIP` | 请求 `1880×1200` 后，壳宽被断言必须在 `1320–1360 DIP` |

诊断没有记录窗口实际尺寸或 runner 分辨率，因此不把失败断言成生产布局缺陷，也不把窗口限宽归因到某个未记录的系统设置。共同证据是测试用 `Window.Width` 请求尺寸推导布局宽度，而断言读到的 WPF 实际布局仍只有 `952 DIP`。R08 修正已在该 run 通过；整次 CI 失败来自上述布局测试假设。

## 校正与本机验证

两项测试改用明确尺寸的 `Grid` 逻辑宿主；隐藏 WPF Window 保留 `PresentationSource`，宿主分别控制媒体页 `1280×720 DIP` 与设置页 `1280/1880/1254/560 DIP` 布局。媒体用例还断言宿主实际宽度为 `1280 DIP`；设置用例断言宽宿主实际宽度为 `1880 DIP`，并保留原有中心、断点、行高、滚动与布局负例。生产 XAML、ViewModel、命令和 DTO 未改。

在本机隔离 SDK `8.0.423` 下重建 Release solution：`0 warning / 0 error`；完整 `ReportedWorkspaceLayoutBehaviorTests` 为 `54/54`，VSTest `17.11.1`，测试宿主 .NET Framework `4.8.9181.0`。该主机运行时与 CI 日志记录的 `4.8.9345.0` 不同。系统默认 SDK `9.0.302` 下也复跑同一类 `54/54`（VSTest `17.14.1`）；不以 SDK9 结果替代 SDK8 或新的 Actions。

| 证据 | SHA-256 |
| --- | --- |
| `tests/ReportedWorkspaceLayoutBehaviorTests-sdk8423-hosted.trx` | `6278114BF12A482C9EFA163EEC9D0CBFBA068462DD4C506A70113AA13A4DC6BA` |
| `tests/ReportedWorkspaceLayoutBehaviorTests-sdk9302-hosted.trx` | `AA6752E2066E9722AADB3ABED2227C833B1D06F3A74AAA1A296BFA300DD6B15A` |
| 测试源码 | `B525DAFADBD5DE4E05F9281E0144091DC1A1DBDAC9CABA6ECCCD316F0AAE101E` |
| SDK8 Release 测试程序集 | `7EA7242BFC7B4ECBBE9FB6CD83595DB9657CE1FB867CD52CF29D0C9CCDA3F0BC` |
| SDK8 Release 插件程序集 | `FC9D5D9676DFF780E8F0FB1DDBE82ACC7F43EE7999A69C297C52A1A332B56DEC` |

`python scripts/validate-source.py` 与 `git diff --check` 通过。源码改动只调整 WPF 测试的逻辑尺寸宿主。

## 验证边界与下一步

这些结果是隔离 STA WPF 的逻辑几何行为；窗口 opacity 为 `0.01`，测试没有证明屏幕上完整呈现，也没有验证真实 Playnite、物理 DPI 或多屏。没有生成或保留截图。

本次测试宿主修正提交并推送 main 后，检查新 Actions 到明确终态。只有新 CI 成功才收口当前门禁；失败时读取该 run 的完整诊断包，按实际错误处理。门禁收口后先做 R00/R01 问题修复与证据校正，再推进依赖已满足的 Q/R 任务。
