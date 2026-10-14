# R10-06 筛选来源提示当前 main 复核

更新：2026-10-14；分支 `main`；测试构建元数据基线 `GscBuildCommit=a2fdb4f647552d31def33aeee641917a049a77d2`。本阶段仅补行为验证，没有改生产 ViewModel、XAML、服务、DTO 或视觉样式。

## 已有能力与本轮验证

筛选来源提示与专用清除命令已由 `5403d797` 引入。本轮沿用现有 `DashboardViewModel`、`ClearTaskNavigationContextCommand`、`FilterTask`、`LatestRequestCoordinator` 与 `TaskCenterView`，没有新增导航、查询服务或状态 DTO。

检查原测试后发现：真实 `TaskCenterView` 的按钮绑定只执行了替身命令；生产清除逻辑的唯一负例是源码字符串检查。本轮加入 `ProductionClearButtonRemovesOnlyNavigationFilterAndCancelsActivePageRequest`，在隔离 STA 中实例化生产页面和未初始化但显式设置合成状态的生产 ViewModel，运行真实导航命令初始化方法，再从真实页面按钮执行生产 `ClearTaskNavigationContextCommand`。为隔离真实 Playnite 设置持久化，测试移除该页面只在真实插件环境使用的 Loaded 集成回调；XAML 页面、数据绑定、按钮和生产命令仍实际运行。

用两条合成失败任务验证：

- 清除前导航游戏条件只显示目标游戏任务；提示可见、按钮命令可执行。
- 点击生产按钮后活动 `LatestRequestCoordinator` token 已取消，来源提示折叠且命令不可执行；第二条原先被导航条件隐藏、但仍匹配其他筛选的任务重新出现。
- 搜索、状态、游戏、类型、任务历史范围和时间区间保持原值；没有请求真实 Worker 或任务服务。

本轮 `R10FilterSourceBehaviorTests` 为 `3/3`，包括原有真实页面绑定正/负可见性测试与源码结构检查；本机 Release 项目构建无编译警告/错误。R09 外机问题也在同一当前 main 构建身份下重新运行：`R09FocusOutlineBehaviorTests` `2/2`，当前测试仍分别要求主 `Chrome` 为 `1 DIP`、独立错误 `ValidationStroke` 为 `2 DIP`。

## 本机验证

- 从仓库外目录选择本机 SDK `9.0.302`；Release `dotnet test --no-restore -m:1` 构建 Playnite `net462`、测试宿主 `net472` 及 Contracts/Core/HostAuditSeeder 引用项目，VSTest `17.14.1`。使用已有 `project.assets.json`，未改 SDK pin、依赖或用户配置。
- `R10FilterSourceBehaviorTests`：`3/3`，0 失败、0 跳过。
- `R09FocusOutlineBehaviorTests`：`2/2`，0 失败、0 跳过；与 R10 分开串行执行，避免 STA WPF 窗口争焦。
- `scripts/validate-source.py` 与 `git diff --check` 通过。无 XAML/视觉改动，因此没有制作截图，也没有宣称 Playnite 宿主呈现已验。
- 脱敏 TRX：[R10-06 behavior](tests/R10FilterSourceBehaviorTests-current-main.trx)、[R09 focus recheck](tests/R09FocusOutlineBehaviorTests-current-main.trx)。本机账户、主机名与绝对路径已脱敏。

## 构建身份

测试程序集 `ProductVersion=0.6.73+a2fdb4f647552d31def33aeee641917a049a77d2`。`GscBuildCommit` 是构建时的父提交身份；以下源 SHA 固定了未提交阶段变更及被验证的生产接线。

| 文件 | SHA-256 | MVID |
| --- | --- | --- |
| `R10FilterSourceBehaviorTests.cs` | `5AB5AEBA3F8B7EF07AB9937FEA2463C938FF6EE0968C3DA583E6CF52C828AAD7` | — |
| `DashboardViewModel.Navigation.cs` | `C95DCC3A544320CAC91DF6688EEF42E0C0A4A8A02657621616CB3F7BAE5CCD62` | — |
| `TaskCenterView.xaml` | `00EBAC27AB45A858BBF9107DE60C64C28DA8F95C70CB3FB8425A6ACE5D65C0E2` | — |
| `R09FocusOutlineBehaviorTests.cs` | `A5AAB9E90C340F93301D8424FF1E30D98B160D39C1265172AEE7A6B01B2CBC98` | — |
| `WpfUiProduction.xaml` | `7CFCBEE2720F3C8960FA9B54D8B86562C87D17D0E44F679FCF846F05ACDCEB2B` | — |
| `GameSaveCenter.Playnite.dll` | `8DACAD7EBD415A9A7DD02D40DD2BE80FA9F2F548193032194DC3CC20C94C85C2` | `239c14b9-4b7b-40b9-bcb2-2e070d4e91c2` |
| `GameSaveCenter.Playnite.pdb` | `0ED07799C57071BF8E2A74AABC79397B10D4D2B1186D591C4CCEF04B8762AE28` | — |
| `GameSaveCenter.Playnite.Tests.dll` | `2990F7BD310999879237D65D1BAC687394C72E2E8F6C5B05EAC503E02F6D5E94` | `abbb878a-622d-4920-96a1-5f1bf66fb609` |
| `GameSaveCenter.Playnite.Tests.pdb` | `CAA2E89167E26ED87DC44D600C964E6911BCED8075A893703B8522A4DDEDB002` | — |

## CI 与外部边界

父提交 `a2fdb4f647552d31def33aeee641917a049a77d2` 对应 Actions [run #992](https://github.com/Nikilua/GameSaveCenter/actions/runs/38564273828)，已终态失败。公开页面确认：

- SDK10.0.401 lane，job `115852859775`，步骤“SDK 10.0.401 编译与测试复核”以 exit 1 结束。
- 默认 `build` lane，job `115852860137`，步骤“编译、测试与打包”以 exit 1 结束。仓库 SDK pin 为 8.0.423，但本次完整日志不可读，实际 runner SDK 未核实。

公开页面列出诊断包：

- `GameSaveCenter-build-diagnostics`，artifact ID `11895041720`，581 KB，SHA-256 `5c6edf73f2b02b2db6860537f26bce48ac9253ef6c49c7b8a3588cbc4cbd41f3`。
- `GameSaveCenter-sdk10-build-diagnostics`，artifact ID `11894167097`，507 KB，SHA-256 `d585c5b86571c403b8a7e24c58946bf38e5320f96f71ffa89bfff04625076147`。

两条 job 的完整日志 API 均返回 HTTP 403 rate limit；两个 artifact API 下载也返回 HTTP 403，匿名网页下载路由返回 HTTP 404。页面和 job annotation 只有步骤名与通用 exit code，失败命令、断言、堆栈和根因未知；没有据此猜测或修改 SDK/CI 问题。SDK9 本机通过不能替代 SDK8.0.423 或 SDK10.0.401 CI。

当前 R09 外机仍缺那台机器的 HEAD、测试 DLL/PDB 身份与实际源文件，因此旧测试源或 PDB 错配仍是推断，不是已确认根因。日志中的 libpng iCCP 警告不在厚度断言调用栈中；TextServicesContext COM 异常发生在断言失败后的 WPF 收尾。

没有访问或修改真实存档、媒体、云端、用户设置或 Playnite 安装。真实 Playnite 清除交互/服务返回、UIA/读屏、IME、物理 DPI/跨屏、最终呈现帧、ETW 与宿主性能仍未验。

下一可执行项：本阶段提交并推送后跟踪新 Actions 到成功或失败终态；失败先尝试取得完整日志。CI 日志受限时，继续 R10-07 侧栏信息密度的独立当前 main 核查，不阻塞主任务。

## #993 当前终态补记

2026-10-14 检查了提交 `e0a72c93774610110100d7ead0ab59108689d1eb` 对应的 Actions [run #993](https://github.com/Nikilua/GameSaveCenter/actions/runs/38570078343)，公开页面确认终态 Failure：

- SDK10 job `115872658151`，“SDK 10.0.401 编译与测试复核”步骤 exit 1。
- 默认 build job `115872658675`，“编译与测试”步骤 exit 1。
- Jobs/log API 均返回 HTTP 403 rate limit；公开 job 页面显示登录后查看日志，没有取得完整命令输出或可下载诊断包。因此失败根因仍未知，不据此调整代码或 SDK 配置。

该 run 不含后续 R10-08 入口切换修改。SDK9.0.302 本地通过不替代此 run 或后续新提交的 SDK8/SDK10.0.401 结果。
