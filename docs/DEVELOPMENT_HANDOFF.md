# GameSaveCenter 当前交接

更新：2026-10-13；R00/R01 freshness `14 fresh / 0 stale` 校正已完成。R05-01/02 已推送到 main：`1c39e909856faa736b32224ee196ef0bac6ca460`，定向 `11/11`；R05-03 定向 `7/7`，几何通过但无闪屏未验、状态部分满足；R05-04 当前工作树复核 `3/3`，清空负例现验证实际模式和跨模式保留 ID。#941 与 #942 已终态失败，日志/artifact API 403 rate limit，根因未知。当前事实见 [CURRENT_STATE](ai/CURRENT_STATE.md)，持久规则见 [PROJECT_MEMORY](ai/PROJECT_MEMORY.md)，阶段记录见 [WORKLOG](ai/WORKLOG.md)。下一项复核 R05-05 当前 main 三态复选框消费点。版本0.6.73。旧记录完整保留在 [历史交接](DEVELOPMENT_HANDOFF_HISTORY_THROUGH_20260930.md)。

## 2026-10-13 R05-04 当前 main 多选摘要复核

- 在 main 父基线 `903525becdb679e9a4aae7bb04e73c98c1cb25dc` 上复用已有生产多选摘要，没有改生产 UI/VM/命令/DTO。原清空测试没有实际挂接 `MediaInboxMode`，本次补强 fixture：生产清空处理器清理当前“已忽略”可见与隐藏 ID、保留“待归类”集合，ViewModel 与 ComboBox 模式保持原值。
- 固定本机 SDK `8.0.423` 下完整 Release solution build `0/0`，定向 `R05MultiSelectionSummaryBehaviorTests 3/3`；源校验与 diff check 通过。确切 TRX、源码与程序集身份见[R05-04 当前复核包](design/reviews/ui-finesse-round3-20260915/evidence/R05-04-CURRENT-MAIN-RECHECK-20261013/README.md)。没有生产视觉变化，未重跑 RenderHarness。
- #941 (`38279606113`, `1c39e909`) failure，8m31s；#942 (`38281199307`, `903525be`) failure，9m00s。job 页面显示登录后可看 logs；匿名 logs/artifacts API 403 rate limit。根因、失败步骤、实际 CI SDK 未知；不得按 exit code 猜修。本机 SDK8不替代用户要求单独跟踪的CI SDK10.0.401。新测试 fixture 与上述 CI 根因无关，未据其失败状态更改生产实现。
- 未运行真实 Playnite，也未访问真实媒体/存档/云端。OS UIA/Narrator/IME、物理DPI/多屏、呈现帧、ETW/宿主性能仍未验证。下一项 R05-05 当前 main 适用性核验。

## 2026-10-13 R05-01/02 当前 main 提交后复核与 Actions 检查点

- 推送的 main commit 为 `1c39e909856faa736b32224ee196ef0bac6ca460`。隔离 SDK `8.0.423` 下 Release 测试构建插件 `net462` 与测试 `net472`；`R05FocusBoundaryBehaviorTests`、`R05OptionVirtualizationBehaviorTests`、`R21FocusVisualRegressionBehaviorTests` 合计 `11/11`，VSTest exit0。TRX SHA-256 `DE45C3F9462F44665BDCD2F068E00630377C413D8669B54F4944F3E988312554`；测试 DLL SHA/MVID `9A509F12BBA20BD826ECC3D900DFD21752F0AAA498B9E10A7BE1CF683C52F063` / `8c8895de-7c96-4fe3-bdcb-fc0455c400e4`，插件 DLL SHA/MVID `8EB2BCE95E6959F3F4F794B26E1CE565CD59ADEF86F4A5B4EEB29133615E7F33` / `0477b734-38ce-4897-8ea4-6bb904b4d2ec`；测试宿主 .NET Framework `4.8.9181.0`。
- Dashboard 行为证据由最小 STA WPF fixture 调用生产确认处理器并通过 routed Click 走生产取消处理器；覆盖焦点进入、Tab/Shift+Tab 环绕、减少动效即时关闭、240ms 动画关闭期间焦点仍留在 overlay、取消后回到合成触发器。XML 检查另核对生产 XAML 的 Cancel/Confirm Click 接线。fixture 没有加载完整 Dashboard XAML 或 Playnite 宿主。
- `python scripts/validate-source.py` 通过，`git diff --check` 通过（仍打印测试源码 CRLF 提示）。进程退出时输出三条 WPF TextServicesHost/TextStore `InvalidComObjectException` 清理异常；测试为 `11/11`、exit0，异常根因未知。用 PowerShell 清理本轮试验 TRX 的命令被执行策略拒绝，相关文件保留在 evidence 本地目录，未尝试其他删除路径。
- Actions #940 (`38274288024`, commit `746eaf6a`) 终态 failure，job `114895863386` 耗时10m30s；日志 API 返回403 `Must have admin rights to Repository.`，根因未知。#941 和 #942 当前都已失败，具体状态见本交接顶部 R05-04 段；当时实际 runner SDK均未由可读日志证实。用户跟踪 CI SDK `10.0.401`；本机 SDK `8.0.423` 分开记录。真实 Playnite、完整 Dashboard XAML 加载、IME/UIA/Narrator、物理 DPI/跨屏、翻转呈现帧与宿主性能仍待验。
- 用户补充的旧检查点 `77de7f450431d267026ce2169aa30525b18bf528` 对应 Actions `36677346317`（#920），现已终态 failure，7m04s 后“编译、测试与打包”exit1；job 页要求登录，完整日志下载 HTTP403，根因未知。此状态属于早于当前 main `746eaf6a` 的历史提交，不替代 #940 或本轮新 run。
- 用户要求优先取得的旧 run `36675309812`（#919，commit `a6b6dfc`）也已终态 failure，总耗时13m02s，“编译与测试”步骤exit1。公开 job 展开仅有加载失败/登录提示，完整日志下载 HTTP403；根因未知，没有日志前不基于 exit code 猜测修复。

## 2026-10-12 R04-03 本机复核与最新已推送 CI

- 最近已推送基线为 main `e952c414a3941491543ecf558f6f6ab2a68bbf6e`。R04-03 测试工作树基于该源码/测试基线并含本阶段关闭确认 test seam 与行为测试；插件 net462、测试 net472 定向构建及 `SettingsDraftLifecycleBehaviorTests + PortableSettingsTests + SettingsValidationSourceTests` 为 `12/12`。详细 TRX、程序集身份和范围见[R04-03 当前 main 补充证据](design/reviews/ui-finesse-round3-20260915/evidence/R04-03-CURRENT-MAIN-RECHECK-20261012/README.md)。未跑完整 Release/RenderHarness，也未修改 XAML/布局。
- 最新已推送 run `38270923668`（#939，commit `e952c414`，job `114884678634`）终态 failure：7m11s 后“编译与测试”exit 1；Node.js 20→24 弃用警告；artifact `GameSaveCenter-build-diagnostics` 478 KB、SHA-256 `660a09c2217195c1e1db44d96887d4f3c674c32f0c02bb3a0385e03dcafc29dc`。GitHub job 页面要求登录，run/job logs 与 artifact API 返回 HTTP403，未取得完整日志或根因，不凭退出码盲修。
- 本机隔离 SDK `8.0.423` 与用户任务补充要求跟踪的 CI SDK `10.0.401` 分开记录；#939 实际 SDK 尚未通过日志核实。仓库 workflow 通过 global.json 选择 `8.0.423`，与用户补充信息有待核差，本机结果不替代 CI。
- R04-03 的“否”路径通过实际 STA WPF `Window.Closing` 事件保留草稿并把键盘焦点恢复到编辑框；“是”路径才调用现有 `CancelEdit` 并关闭。test seam 默认仍走原生 MessageBox。真实 Playnite 原生确认框、保存/Worker 应用与宿主关闭时序仍待验。R04-04 至 R04-08 已有账本证据，下一项 R05-01 当前 main 复核。
- 使用合成设置、fake Worker 和隔离目录；未碰真实存档/媒体/云端。Narrator/UIA、物理 DPI/跨屏、最终 presented frame、ETW 与宿主性能仍未验证。

## 2026-10-12 早期检查点（前序记录）

- R02-08 当前 main 行为测试 `4/4`，source TRX、Light/Dark Save 动作文案截图及 CloudQueue 页面上下文图已归档；RenderHarness `372` 图 `render-qa OK`。插件文案行为满足，真实 Playnite 与云端/恢复操作未执行。[证据包](design/reviews/ui-finesse-round3-20260915/evidence/R02-08-CURRENT-MAIN-RECHECK-20261012/README.md)
- 本轮 build diagnostics 及 `D:\GameSaveCenterTools` 下 build/render 输出已请求按精确路径清理，但执行器拒绝 PowerShell `Remove-Item`；它们仍留在本机且未纳入提交，没有尝试其他删除路径。
- picker Dispatcher 修复提交 `f13b9e05de0cf502ac61e97b0fbad8433b339b3f` 已推送 `main`。无 `SynchronizationContext` 的 STA 旧实现线程负例失败；修复后的 R05 picker 类 `5/5`。前一候选完整 Release/WPF 脚本在本机 SDK `8.0.423` 下 exit `0`（XAML24/24、0 warning/error、合计1550 passed/67 skipped/0 failed），其构建诊断来自 e12119b 加三项 dirty 源码/测试；不能称为 clean post-commit 全量身份。
- 本次对确切提交 f13b9e05 的 R00/R01 定向复核 `40/40`，RenderHarness 0 warning/error；媒体几何与工具栏负例探针均通过，证据身份、SHA/MVID、原始30搜索样本见[R00/R01复核包](design/reviews/ui-finesse-round3-20260915/evidence/R00-R01-CURRENT-RECHECK-20261012/README.md)。freshness 已校正为14 fresh/0 stale；包未构建，package identity 未提供。
- 当前 Actions run `38222818374`（run930）已终态 failure，失败步骤“编译与测试”。CI workflow SDK10.0.401与本机 SDK8.0.423分开记录。下载完整日志API返回403 `Must have admin rights to Repository.`；artifact列表可读但下载返回401，根因未知。不得按退出码猜测或盲改；需取得有权限的完整日志后复现。
- 文档证据提交 `da6388e814309825141fb530ecda702cb7e11eab` 对应 run `38226955461`（run931）已终态 failure，失败步骤仍为“编译与测试”，SDK10.0.401。完整日志 HTTP403；diagnostic artifact ID `11735869371`（488,867 bytes）下载 HTTP401。根因未知，尚无失败命令/断言/堆栈，不凭 exit code 盲改。
- R02-03 复验已提交 `a2144083`；随后当前 main `a21440834865551cdb6894830548af51ad1d8e78` 的 Release 构建 SDK8.0.423 `0/0`、XAML24/24，R02-04 STA 几何测试 `7/7`，16/20 DIP 图标间距各8 DIP、中心差0；双主题 RenderHarness `372` 图 `render-qa OK`，Save 1040×700 两张截图已核对并归档。`validate-source.py` 通过，WPF静态审查36 XAML `0 errors/29 warnings/177 info`。该复验未改生产 UI。
- `a2144083` 对应 Actions run `38244963620` 已失败于“编译与测试”，页面仅显示 `Process completed with exit code 1`。CI SDK10.0.401；未认证 API rate limit HTTP403，GitHub job 页完整日志要求登录，diagnostic artifact `11745496487`（478 KB，SHA-256 `11de81935473ce3da60739d481c768eba2af589d2c76cc819cf3977fd4daea1e`）下载路由404。根因未知，不按退出码修复。
- `90099a09` 对应最新 Actions run `38247748056` 也终态 failure 于“编译与测试”，页面只有 `Process completed with exit code 1`。CI SDK10.0.401；未认证 Jobs/log API rate-limit HTTP403；diagnostic artifact `11747310690`（479 KB，SHA-256 `8586bcace066ca779c72dbf9fbea1843c19a49133d619b9a6000e6e8dc6e79c5`）下载路由404。失败命令、断言、堆栈未知，不按退出码猜修。
- docs/evidence commit `3df542af` 的 Actions run `38249983091`（#934）已终态 failure 于“编译与测试”；SDK安装、结构、诊断自测及SDK基线步骤成功，渲染/静态审查/打包跳过。CI SDK10.0.401；日志endpoint HTTP403；artifact `11747469284`（488,428 bytes，SHA-256 `662aba958e53e7a6d8ccd837a319fd93e8df91d2309f7977b0af1a84935f79fa`）下载HTTP401，根因未知。[run #934](https://github.com/Nikilua/GameSaveCenter/actions/runs/38249983091)
- main `4d166f017dcba8fbb07980ec21b690267b9ff377` 对应 run `38253621689`（#935，job `114825925174`）已终态 failure，耗时9m39s，失败步骤“编译与测试”exit1。CI SDK10.0.401；job logs HTTP403，artifact `11750360097`（478 KB，SHA-256 `805b8eff2797816cf4b8f0fded68de024da80f3dc585993c02373be29e8ef46c`）下载HTTP401。失败命令/断言/堆栈不可读，根因未知，不盲改。[run #935](https://github.com/Nikilua/GameSaveCenter/actions/runs/38253621689)
- 当前文档/evidence提交 `8e71feb41aeb20738aa80f0cbb5d05d0c6aeb0af` 对应 run `38262296773`（#936，job `114855606547`）终态 failure，耗时6m19s，“编译与测试”exit1。CI SDK10.0.401；logs API403 rate limit，job页要求登录；artifact `11752913387`、476 KB、SHA-256 `d78fb86cee5d81520fa81cebe65434cded562c18bd6fda46af0a91b8081bb021`，匿名suite路由404。根因未知，不能盲改。[run #936](https://github.com/Nikilua/GameSaveCenter/actions/runs/38262296773)
- 安装前本机只发现SDK9.0.302；仓库要求SDK8.0.423且 `rollForward=disable`。对 `3df542af` 的本机构建在sdk-baseline预检停止，未编译/测试；`validate-source.py`通过。2026-10-12后由官方 `dotnet-install.ps1` 将 SDK8.0.423 安装到隔离目录，临时前置进程PATH，系统PATH未改；clean main `4d166f01` Release 全量 `1560 passed/57 skipped/0 failed`。这不替代CI SDK10。
- R02-05 当前 main `90099a09104677708a5829cf7e6a5d22b6b414ff` 已重建 Release `0/0`、XAML24/24；生产资源 STA 几何命中测试 `2/2`，包含6 DIP间距负例、中心命中独立按钮及82 DIP换行；双主题RenderHarness `372` 图 `render-qa OK`，已归档Media 1040×700浅/深截图。源码门禁通过，WPF静态审查36 XAML `0 errors/29 warnings/177 info`。没有生产UI变更。
- R02-08 当前 main `4d166f01` 文案行为 `4/4`，全量 Release `1560/57/0`、双主题 RenderHarness `372` 图通过，TRX/截图/身份见当前复验包。R03-01 当前 main `8e71feb4` 的 GlyphRun 行为证据和 `TypographyDiagnosticsTests 11/11` 已补；探针/测试程序集身份及旧 DLL 负例见复核包。下一项 R04-01 当前 main 行为类 `6/6` 已复跑，证据待同步。R02-06 仅在 ENV-001 前置变化后重跑 fail-closed 宿主预检；真实 Windows IME 候选 UI/Playnite 输入链仍未验；Q06/R08用户原始失败日志与DLL identity仍缺。
- `.tmp/r02-03-current-recheck-20261012`、`.tmp/r02-04-current-main-20261012`、`.tmp/r02-05-current-main-20261012` 和 `artifacts/build-diagnostics` 下本轮诊断目录均已确认是仓库临时输出，所需证据已复制至 docs；递归清理请求被执行器拒绝，暂留本机。最新SDK预检诊断目录为 `artifacts/build-diagnostics/20261012T123814433Z-853a4720`，未提交。SDK下载中断后留下的空工具目标 `D:\GameSaveCenterTools\dotnet-sdk-8.0.423`，其精确非递归清理也被执行器拒绝。没有改走其他删除路径。
- Actions 权限受限期间继续不依赖该日志的独立 Q/R 小批次，不把 CI 标绿。CLOSE-HOST-01 仍受 ENV-001 限制；Q06/R08 用户原始失败日志与 DLL identity 仍缺。真实 Playnite、物理 DPI、IME/UIA、ETW 和最终呈现帧均未验。

## 2026-10-07共享UI阶段检查点（历史记录）

输入错误覆盖描边、实际Arrange行距和共享复合按钮对齐已修复；联合专项64/64、焦点可见获焦/紧凑负例4/4、完整render/source/XAML通过，Core125/Worker357/source470 passed/18原有skip。前两轮全量R02/R05失败、旧DLL/同candidate对照及干预均保留，未放宽原断言。

第三轮完整入口前36类WPF含Q14/R02/R05焦点通过，第37类R05OptionVirtualizationBehaviorTests删除回退期望Game 0000、实际Game 0020，2 passed/1 failed，后77类未执行。旧clean c35与同candidate独立均3/3；新增CLOSE-PICKER-01，先受控核对排序/20ms防抖刷新线程及集合重建，尚不归因具体修改或改断言。UI阶段源码/证据单独commit/push，完整门禁和任务仍IN_PROGRESS；下一阶段构建必须使用提交后的真实HEAD，不能把c35 dirty DLL冒充新提交。

c35 runner两项自测/SDK8.0.423已实际验证，失败artifact已取得。独立render-qa未注入env时MSBuild身份unknown已实际查询，当前render显式注入c35并核对DLL；后续修脚本注入/恢复。共享模板另命中R00-01-02/R00-05，共10项补证，baseline不改。证据：[共享UI阶段](ai/evidence/close-wrap-20261007/README.md)。
## 本轮完成

- global.json与CI固定SDK8.0.423/disable，C#12.0，构建和独立runner验证实际选择；PS5.1/7真实缺失旧SDK失败、原latestMajor对照返回8.0.423通过。Release0/0、Core125/Worker357/source470通过；完整WPF、runner与包门禁仍须按最新终态签收，SDK/CI任务不关闭。
- run37565955357/37566483862已取得终态与always上传产物；SDK10侧栏22/22，默认偏好ClientAreaAnimation=false；实际Q14错误态36→37 DIP失败。STA测量确认错误边框参与布局，独立双主题654 DIP边距失败和620/660正例均已取得。
- 补修诊断器多个PATH同名程序拼接导致启动失败；两个不同原生程序夹具在PS5.1/7验证首路径优先级，finally恢复PATH。前后诊断保留；无业务/UI修复或版本变化。

## 前阶段已记录事实（历史）

- CI诊断已接入实际native退出码、独立console/TRX、SDK/源码hash/DLL身份及always上传；PS5.1/7受控失败、stderr、缺失程序和脱敏自测通过。生产默认系统动效策略保留，实例内测试输入覆盖8组合；侧栏22/22，source470/18，Core125/125。
- 本轮WPF前23类通过，第24类Q14先在错误态高度36→37 DIP失败，后89类未执行；没有运行到原654 DIP边距断言。CI恢复仍未验，不关闭CLOSE-CI-01。新增受影响R00-03/R01-01/R01-04须跟原5项一并补证。
- CLOSE-IPC-01已完成：独立管道、握手与失败清理先排除夹具干扰，再受控复现生产取消dispose的IOException/EOF竞争；修复取消分类，保留RequestId、有限重放和未知提交语义。
- 同一最终DLL专项20轮200/200；source470通过/18原有跳过/零失败，Core125/Worker357通过，SDK8 Release 0 warning/error、XAML24/24。源码/DLL SHA、MVID、复现和负例见 [本轮证据](ai/evidence/close-ipc-20261005/README.md)。提交前dirty构建身份明确分账。
- 全量WPF在第24类Q14失败：654 DIP单行预设控件残留20 DIP bottom margin，同DLL独立复核再失败。前23类通过，后89类未执行；新增CLOSE-WRAP-01，整体门禁不通过。
- 取得最新CI run37329446647完整失败步骤：实际SDK10.0.401，两条ProductionShellChromeSourceTests活动动画断言失败，与IPC分开；旧run日志403仍是历史事实。

## 下一项如何开始

1. fetch main并核对工作树；活动任务以 [AUTONOMOUS_BACKLOG](AUTONOMOUS_BACKLOG.md) 为唯一入口。
   两条前阶段CI终态及诊断已经归档，不再重复查询；SDK阶段隔离build身份是c7f2a368加dirty源码hash，后续提交需重建。freshness6/8，baseline未改；不要复用旧DLL假称当前候选。
2. 继续CLOSE-CI-01/CLOSE-SDK-01：当前源码runner须实际选择8.0.423；Q14修复后完整CI编译/测试/渲染/包门禁通过才关闭。 SDK10侧栏实际通过，不再视为当前失败，不改OS偏好或skip。
   SDK源码91bfb6e1已推送并clean重建0/0、SDK自测通过；run37569191845在自测日志通过后的外层退出码失败，API产物0。已补显式自测exit0/自测日志上传及过滤脚手架；PS5.1/7同Actions包装入口退出0。先核对补修后的新CI，不重复查旧两条远端。
3. CLOSE-WRAP-01：先核对共享TextBox错误态BorderThickness的测量变化，同时解决原654 DIP单行残留20 DIP margin；保留620→660→620往返和Light/Dark，不放宽几何容差。修好后补原5项及R00-03/R01-01/R01-04。
4. ENV-001门禁满足后做Media Inbox同PID/DLL/MVID滚动几何；用户Q06/R08仍需原始日志与运行身份；最后执行同候选恢复/回滚/Undo与发布矩阵。

## 验证边界

本轮没有启动真实Playnite、改变Windows动画偏好、安装到用户Extensions或触及真实存档/媒体/云端；没有验证物理DPI/读屏/最终呈现帧。SDK8本机与SDK10 CI分别记录。测试期间只终止已核对命令行归属本轮隔离目录的测试进程；被终止的运行不记为通过。

每独立阶段编译、测试、同步文档并commit/push当前main。历史d57a613f包的clean构建/IPC/结构及六个程序集身份已记录，未安装；不能签收当前源码。当前freshness6/8，baseline未改，完整SDK/CI门禁仍受Q14阻塞。本地包不冒称稳定发布，当前阶段中间产物归档后按仓库规则清理。
