# R10-08 最近访问入口当前 main 复核

日期：2026-10-14；分支 `main`；基线提交 `e0a72c93774610110100d7ead0ab59108689d1eb`。本阶段恢复已有最近访问能力的用户入口，没有重建数据服务或 DTO。

## 发现与修正

R10-08 的既有数据层已保存最多 8 条稳定 Playnite ID、工作区、TabIndex 与 UTC 时间，并能过滤已移除对象；序列化不保存游戏名、路径或归档路径。但后续 GSC-058 将首页“最近访问”卡片固定设为 `Visibility="Collapsed"`，历史测试又手动把它设为 `Visible` 后才点击，不能证明用户有入口。

现改为在同一概览栏切换“最近任务”和“最近访问”：默认仍显示最近任务，点击标题栏里的“最近访问”可展开原有访问列表；访问列表提供返回最近任务按钮。两种记录互不合并，同时只占用一个卡片位置。入口使用既有 `GscWpfUiCompactButton` 共享模板与动态主题资源，保留其 Hover、Pressed、Disabled、焦点状态；没有改选框、滚动条、业务命令、服务、DTO、取消/错误语义或列表虚拟化。

## 行为与视觉证据

- `R10RecentAccessBehaviorTests` `2/2`：一条验证记录上限/稳定 ID/未知工作区归一化/移除对象清理/不序列化名称或路径；另一条在真实 `OverviewView` 和 STA Window 中分别聚焦并点击“最近访问”和“最近任务”，检查可访问名称与焦点样式、两张列表保持独立 ItemsSource、隐藏列表不可见，以及真实视觉树中的最近访问行命令只触发一次。测试命令使用隔离 fake，不访问用户设置、Playnite 库或 Worker。
- 同一构建后 `R09FocusOutlineBehaviorTests` `2/2`，`ProductionShellChromeSourceTests` `22/22`；Release Playnite `net462`、测试 `net472` 和引用项目重建成功。SDK `9.0.302`、VSTest `17.14.1`；仓库 SDK pin `8.0.423` 本机未安装，不能替代 SDK8/CI SDK `10.0.401`。
- `scripts/validate-source.py` 通过；`scripts/check-xaml.ps1` 为 `24/24`；WPF 静态审查 `0 errors / 29 warnings / 177 info`。Warnings/Info 来自仓库既有检查项，不含本次 Overview 的 error。
- `scripts/render-qa.ps1 -Configuration Release` 完成 `render-qa OK`，报告中 `PROBLEM` 数为 `0`。报告覆盖 Light/Dark 与 11 个常用逻辑尺寸。人工检查 Overview `1040×700` 两主题图：默认最近任务标题栏内的切换按钮清晰、未挤压任务区。报告和截图是 offscreen RenderHarness 证据；报告中默认模式的 `OverviewRecentAccessList` 为 0×0。切换后的列表布局/可见状态由 STA 行为测试覆盖，没有把默认模式截图冒充为访问模式截图。
- WPF 静态检查和截图均未证明真实 Playnite 呈现、物理 DPI、跨屏或最终 presented frame。

| 证据 | SHA-256 |
| --- | --- |
| `tests/R10RecentAccessBehaviorTests-toggle-current-main.trx` | `0330FA4B55C791776658253C0271B7BCADAEDD84FAFE9BB84CB70298EE4E4F96` |
| `tests/R09FocusOutlineBehaviorTests-current-main.trx` | `E273EAABC8A58CDB04A899671FA662A9537B678DFDC9E3C4EBE6F2FDAF874E56` |
| `render/render-qa-report.txt` | `A2A9A0CED08FB80C3E2F73E4325A06CDE80643C37975AD74C0FE4804474E077A` |
| `render/theme/light/Overview-1040x700.png` | `37C3DA414934713DBD0DDBA4FEE16088A50DF4207F5BFCEBB70B6BA2710F485F` |
| `render/theme/dark/Overview-1040x700.png` | `F86EEAB506C83BF4C254EF8C827C432E55AEF9D5F973140FE10DCAEFB6D499E7` |

## 源码与程序集身份

| 文件 | SHA-256 | MVID / 版本 |
| --- | --- | --- |
| `OverviewView.xaml` | `4EBA02A3E15380DBBFC5C783968B41753759F3EA1EDA4E1E2945CCAE27E46036` | — |
| `OverviewView.xaml.cs` | `A1924BDFF4FF45E7DC82E8E8DB636CEFCFC1160F77D2C02CE9A9F89E9E8C9F6B` | — |
| `R10RecentAccessBehaviorTests.cs` | `8FA8A9CFD11E3E39812FA14146E62B801059198478E40EB234D1AA879E805E63` | — |
| `R09FocusOutlineBehaviorTests.cs` | `A5AAB9E90C340F93301D8424FF1E30D98B160D39C1265172AEE7A6B01B2CBC98` | — |
| `WpfUiProduction.xaml` | `7CFCBEE2720F3C8960FA9B54D8B86562C87D17D0E44F679FCF846F05ACDCEB2B` | — |
| `GameSaveCenter.Playnite.dll` | `D42A07426F001B5FE20F6168B00E6C51DCD1AF5838AC158CDB91B92E15FE61A4` | `40cdb240-f202-47c8-a438-60820e0e78c3`; ProductVersion `0.6.73+e0a72c93774610110100d7ead0ab59108689d1eb` |
| `GameSaveCenter.Playnite.Tests.dll` | `39DD3668B86A3782AA6C76DF01305D45108F39FDAE5D6AEAC685123034DFAF4B` | `eeee7482-78a5-4357-93b5-2ad1f85b1057`; same ProductVersion |
| `GameSaveCenter.Playnite.Tests.pdb` | `296F1E8F2932DA3AA4F9A972C8D5DD9A7BBEC0114010D55275B2CDD6AE561A80` | — |
| `GameSaveCenter.Playnite.pdb` | `9BD887A95DA0EA83336092D05F698DB877815CD71F4CA86AC0B35AA412197C65` | — |

## CI 与外部边界

前序提交 `e0a72c9` 的 Actions run #993 (`38570078343`) 已终态失败：SDK10 job `115872658151` 的“SDK 10.0.401 编译与测试复核”与默认 build job `115872658675` 的“编译与测试”均 exit 1。两份完整日志 API 返回 HTTP 403 rate limit；公开 job 页显示需要登录查看 logs。没有本次 run 的完整日志或可取回诊断 ZIP，失败根因未知；不以此推测本次 R10-08 的构建/测试根因，也不凭 SDK9 本机结果签收 CI。

本轮的构建产物与 TRX 使用合成数据、隔离 testhost；没有启动真实 Playnite、运行 Worker、写真实用户设置/存档/媒体或云端。下一可执行任务：本次提交推送后跟踪 SDK8 与 SDK10.0.401 Actions 至终态；CI 收口后复核依赖已满足的 R11 项。真实 Playnite/package-host、外部 UIA/读屏、OS 键盘/IME、物理 DPI/跨屏、最终呈现、ETW/宿主性能仍待验。
