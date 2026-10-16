# GameSaveCenter 长期项目记忆

更新：2026-10-16；分支 `main`，推送 HEAD `a81e7abd219e136cf784359ac48174e154f157be` 与 origin 一致，代码父级为 `ccdbacb683afe6b0a745640d545cc9876332391c`。R13-04 本机 SDK8 定向 Worker/Core/Playnite 为 `4/4`、`1/1`、`24/24`，同输出 R09 `2/2`；当前候选 Actions #1039 双 SDK（8.0.423 / 10.0.401）均终态成功，Playnite source `108` 类、WPF `124` 类全部通过；SDK8 Render QA、静态审查和扩展包生成成功。R00/R01 freshness `14/14` 无需重跑、0 登记源路径命中，package identity 未提供。证据见[R13-04当前main复核](../design/reviews/ui-finesse-round3-20260915/evidence/R13-04-CURRENT-MAIN-RECHECK-20261016/README.md)。

外机新附 R09 输出显示 WPF `116` 类，line 151 断言 Chrome `2 DIP`；当前 main 同行检查错误色，line 152 是 Chrome `1 DIP`，line 154 才是独立 `ValidationStroke 2 DIP`。旧测试形态见 `3f7d0b30`，`f13b9e05` 已拆分。`gsc-b/fd7672ef` 为随机构建目录，不是提交标识；缺同次 sourceCommit、dirty paths、测试 DLL/PDB SHA 与加载插件 DLL 身份。可确认测试输入版本不同，具体来源未归因；#1039 当前候选通过不覆盖外机失败。完整本机 WPF 扫描仍因受限桌面的 `GetCursorPos` error 5 中止。

CLOSE-WRAP-01、CLOSE-OPTICAL-01、CLOSE-FOCUS-01 已由 #1039 双 SDK 完整自动化门禁收口。下一步按 ROUND3_PROGRESS 中的条件选择依赖已满足且仍未完成的 Q/R 项。真实 Playnite、物理 DPI/跨屏、UIA/IME、最终呈现、ETW/宿主性能仍待验。

## AI/Codex 启动协议

开始开发前依次阅读：

1. `docs/ai/CURRENT_STATE.md`。
2. `docs/ai/PROJECT_MEMORY.md`（本文件）。
3. `docs/ai/WORKLOG.md` 最新相关阶段，历史按需查找。
4. `docs/DEVELOPMENT_HANDOFF.md`。
5. 最近 15–30 条 Git log、`git status`；联网后 fetch 当前远端分支，保留未知本地变更。
6. `docs/AUTONOMOUS_BACKLOG.md` 活动队列和目标任务的证据。
7. UI 任务另读仓库 `.codex/skills/wpf-apple-desktop-ui/SKILL.md`、相关 references、`docs/design/APPLE_WPF_IMPLEMENTATION_PROMPT.md` 和 `docs/design/UI_CHANGE_GATE.md`。

事实来源为当前代码、证据、Git 和用户指令。默认每阶段构建、测试、同步文档、单独 commit 并 push 当前远端分支（默认 main）；不要被历史“不得 push main”覆盖。禁止提交 artifacts、.tmp、bin/obj、用户配置、凭据和可再生大包。

## 项目与模块

- 当前版本由 `Directory.Build.props` 与 `src/GameSaveCenter.Playnite/extension.yaml` 决定：0.6.73。版本号不能替代 commit、SHA-256、MVID 的包身份核对。
- Playnite 是唯一主要 UI（net462 WPF GenericPlugin），扩展 ID `66e9f2d7-67bb-43ef-b62a-b8e60734fcec`；Worker 是独立 net8 Windows 进程。
- Core/Contracts 为 netstandard2.0；Named Pipe 连接 Playnite 与 Worker；SQLite、存档归档、媒体和 GameTools 文件由 Worker 管理。
- Worker `BackupOrchestrator`、`RestoreOrchestrator`、`RestoreReadinessService`、`RemoteBackupStagingService` 管理备份/安全恢复；`TaskCoordinator` 管理任务；`MediaSyncService` 管理媒体；`GameToolService` 管理工具。
- `DashboardViewModel` 已拆出 16 个 partial，主文件仍约 6480 行。只能基于实际修改痛点或测量结果继续提取职责，不重建已有服务/DTO/命令。
- 六工作区与 Settings 共用生产局部资源；不要把插件资源写进 Playnite 全局 Application.Resources。

## 安全与业务语义

- 恢复前置检查、PreRestore、失败回滚、UndoRestore、运行中拒绝恢复和冲突确认不可因 UI 简化丢失。多设备分叉由用户决策，禁止自动合并/覆盖。
- 远端内容先隔离 staging、归档/路径/清单/大小/hash 校验，再进入既有恢复链。Manifest 非法、缺失文件、路径越界或无法确认时不能标 Ready。
- Rclone 使用既有 copy/check/lsf/cat/version 白名单，不引入 sync/delete/purge。恢复、删除、保留清理和外部进程验证只使用隔离夹具。
- 写 IPC 断连/超时可意味着已提交；重放保留 RequestId 和 ledger 语义，取消不等于回滚。不可把未知结果包装为失败后可盲重试。
- GameTool 启动走 Worker。CloseOnGameExit 只处理本 Session 启动、PID 与 StartTime 均确认的进程；未解析链接、脚本/默认程序不假装可追踪。
- 自定义工具已有实例按完整 EXE 路径匹配；反作弊场景按既有分类门禁执行，不自动运行 Unknown/GameModification，不提供绕过保护。
- 初次环境检查只读或操作自身临时探针，不自动备份/同步/上传/删除真实数据。`OnboardingCompleted` 的跳过和再次检查语义保留。
- Playnite 退出回收自身 Worker，防止 shutdown 竞态重启。不得按名称杀任意 Worker；安装时路径/归属未知即停止，不默认提权。

## UI 与性能规则

- Demo-first 优先于历史生产结构和通用 Apple 风格。Demo 路径当前缺失时沿用已恢复生产资源并明确限制。当前游戏选框与滚动条为明确保留例外。
- 真实 Binding、命令、错误/取消、键盘与 UI Automation、主题/DPI 和可扩展列表必须保持。当前授权允许整页重构，旧历史“不能替换/不能迁移”不冻结页面。
- 焦点/Popup夹具必须先断言控件实际可见、Focus()成功及Keyboard.FocusedElement；紧凑布局隐藏筛选行时不能程序化打开隐藏ComboBox再当成真实Tab路径。MoveFocus只证明受控遍历，OS键盘/IME与Playnite宿主仍单列。
- 页面内容有限测量，主列表/表格内部虚拟化；最小可读视口与短窗回退按当前生产几何门禁验证，不机械沿用历史固定高度。
- DataGrid Item 滚动与稳定行样式已有真实回归经验，勿盲改 Pixel 或关闭虚拟化。Media Inbox 宿主空白需同进程几何诊断，不能由离屏通过推断修复。
- 资源走动态主题令牌；普通计数/装饰用 accent/neutral，Info/Success/Warning/Error 表达真实语义，不能只靠颜色表达状态。
- 动态 ComboBox 重排/集合刷新可能清空或反写选择；保持来源唯一、逻辑默认值和草稿上下文。无关刷新不能抢走用户选择。
- 大库先显示 SQLite 缓存；不在 UI 线程全量匹配、图片解码、文件/网络/SQLite IO；无数据变化避免 Reset/CollectionView.Refresh。
- 缩略图后台限并发、取消、LRU、Freeze 后回 UI；动画优先 render 属性，卸载释放时钟/订阅。真实性能必须测量，Rendering 回调不是 presented frame。

## 证据规则和当前风险

- 活动动画测试需显式控制应用与系统偏好两个前提；生产默认必须保留HighContrast/ClientAreaAnimation，测试用实例内internal输入，不修改OS/全局开关。CI无动画偏好不能与“动画应启动”混淆；默认真实偏好路径必须单独验证。
- build-diagnostics保存native实际退出码、分步console/TRX/SDK/dirty源码hash/DLL身份。PS5.1会将stderr包装成ErrorRecord，采集过程需继续至LASTEXITCODE；缺失exe必须有launchError而不能继承0。摘要标注testsRequested；诊断异常也不能阻止恢复环境。
- SDK声明以 `global.json` 精确 `8.0.423/disable/不允许preview` 为准；CI 每个 run 按日志记录实际选中版本。当前 main #947–#950 选中 SDK `8.0.423`；用户指定旧 run #919/#920 选中 SDK `10.0.401`，版本结果不可互相替代。缺失版本须明确失败，不能用其他预装 SDK 替代；升级另做完整编译/测试/包验证。2026-10-12 将 SDK8 安装在隔离路径 `D:\GameSaveCenterTools\dotnet-sdk-8.0.423`，仅临时调整进程 PATH，不改系统 PATH；C#12.0 而非 latest。
- 带预期非零native负例的自测入口须在finally之后显式exit0；Actions pwsh外层会继承LASTEXITCODE，出现断言全部通过但步骤失败。不能在采集器清零实际负例；异常路径仍非零。自测诊断也纳入artifact目录，上传只JSON/TXT/TRX，不能以always步骤success当作artifact存在。
- TRX脱敏除TestRun.name/computerName/路径，还须覆盖runUser和runDeploymentRoot；保留GUID、方法/断言/堆栈/计数和DLL身份。合成元数据负例同时检验异常路径仍非零。
- run37565955357/37566483862已实际失败上传并下载：SDK10侧栏22/22通过，native偏好记录ClientAreaAnimation=false；仍在Q14错误态36→37 DIP失败。STA干预确认2 DIP错误边框改变测量；独立私有几何夹具复现Light/Dark654 DIP单行20 DIP margin，620/660通过。探针不是生产修复或宿主验收。
- SDK阶段历史：2026-10-07本机侧栏22/22、source470/18、WPF前23类通过；Q14独立及全量先因错误态TextBox高度36→37 DIP失败，未到654 DIP，原边距问题继续开放。GscMotion/build变更还命中R00-03/R01-01/R01-04，补证范围增加，baseline不能静默转绿。
- 受控自动行为、离屏截图、真实宿主、物理 DPI/跨屏、包安装身份分别记账。历史宿主成功不签收当前候选。
- IPC取消通过dispose解除IO时，任务可先以IOException或EOF结束；仅在linked token确已取消的transport路径按host→caller→timeout分类，并保留RequestId/未知提交，不覆盖服务拒绝。测试服务用握手、每场景独立管道和失败路径异步清理，不用固定延时猜取消时机。
- 当前活动缺口：唯一执行入口是 `docs/AUTONOMOUS_BACKLOG.md`。本地 main `ad2e6716` 比 origin ahead 1，R13-03 当前 SDK8 行为补证已完成；同一新隔离 DLL 上 R09 类 `2/2`、R13 类 `14/14`。完整本机 Playnite 扫描因 R05 fixture 读取系统指针返回 Win32 error 5 停在 `45/124`，不能记全量成功；无权限绕过，也未尝试移动指针。旧 Actions #1037 的两个 SDK jobs 成功但只验证 `09173448`，本地提交推送后必须查新 Actions 到终态。用户外机 R09 日志没有 source/DLL/PDB identity，WPF 类数 `116` 与当前 `124` 不同、line151映射不一致，根因仍未确认；下次如重现，需获取同次 diagnostics summary、HEAD/工作树、源片段、DLL/PDB SHA。R00/R01 freshness `14 fresh / 0 stale / 0 source matches`，package identity 未提供。下一项 R13-04。真实 Playnite/OS UIA/IME、物理 DPI/跨屏、presented frame、系统时钟事件即时刷新、ETW/宿主性能仍分别记账。
- `599d2fd` 的 `14 fresh / 0 stale` 基线已对受影响行为和负例复验；`0243c108` 的四项 stale 是更新前中间状态。`4078f23d` 当前源路径复核仍为 `14 fresh / 0 stale`。Package identity 与真实宿主边界仍独立。
- ENV-001 历史 WMI 命令行读取拒绝、CEF 0x5 与单实例边界尚无解除证据；条件变化后先做 fail-closed 预检，不关闭用户实例，不反复撞同一边界。
- Q06/R08 本机不复现不能关闭用户问题；需要失败方法、断言、堆栈、DLL 身份。COM 清理日志不自动解释测试失败。
- 当前任务状态和完成定义只维护在 [活动积压清单](../AUTONOMOUS_BACKLOG.md)；Q/R 账本是证据矩阵，不能合并为产品完成率。

- 2026-10-07共享UI阶段：错误描边不得改变原生内容测量；行距按Arrange后的layout slot判断实际多行并恢复作者margin，Unload释放LayoutUpdated。Q14已通过定向及首轮全量入口；随后R02失败由旧clean DLL同样复现。共享按钮须区分文本的有限Stretch测量与视觉树的HorizontalContentAlignment，避免拉伸固定/Auto列产生额外DIP。原4项已过，联合完整门禁待验；当前受影响证据为10项，旧渲染不能签收新增按钮模板。

- 2026-10-12 CLOSE-PICKER 复验：在 STA WPF ViewModel 测试夹具中没有 `SynchronizationContext` 时，`Task.Delay(...).ConfigureAwait(false)` 的延续可能运行在线程池；刷新 WPF `CollectionView` 必须回到创建它的 Dispatcher。保留 20 ms 输入合并，取消的旧 Dispatcher 回调还要在执行时检查 cancellation token。新负例先在旧实现失败、修复后 R05 全量类通过。生产 TextBox 错误态两 DIP 描边采用独立 `ValidationStroke` overlay，因此行为测试应检查 overlay 的 Opacity/Thickness，并断言布局 Chrome 仍为 1 DIP；不能要求 Chrome 为 2 DIP。

### 2026-10-14 Q24-07 当前行为证据

高对比行为应通过生产资源字典与真实 WPF 控件模板状态验证，不以资源键或 XAML 字符串存在作为完成依据。当前测试覆盖 Follow/Light/Dark、宿主明暗、玻璃/动效开关、禁用按钮和选项、选中态、焦点框、Popup、活动动画时钟移除和恢复。WPF 合成 KeyDown 可证明模板 pressed 状态；直接构造 KeyUp 不更新 `KeyboardDevice`，当前释放使用明示的受控 `IsPressed` 状态探针，不得记为物理键盘输入。证据链接：[Q24-07 高对比复核包](../design/reviews/ui-finesse-round3-20260915/evidence/Q24-07-CURRENT-MAIN-HIGH-CONTRAST-RECHECK-20261014/README.md)。本机 Release SDK8 不是 SDK10 CI 或真实 Playnite 视觉证据。当前 freshness stale 的四项按计划下一批重跑，不提前把旧基线签为 fresh。

## 文档维护约定

CURRENT_STATE 保持短摘要，PROJECT_MEMORY 保持稳定规则，HANDOFF 保持下一步；WORKLOG 保留阶段记录。不要向三份入口反复复制完整 TRX 数字与过期下一项。证据全文独立归档，入口链接到同一来源。

历史快照与旧 ledger 状态原文保留；任何状态升级必须有对应证据。临时文件删除前确认绝对路径在仓库 artifacts/.tmp 内，使用 PowerShell `Remove-Item -LiteralPath -Recurse -Force`，只清理已确认不用的本轮产物。

### 2026-10-13 R06-07 当前 main 证据规则

`87d27592` 相对 `73d0f6e6` 只有文档变化，故可复用同源 SDK8 Release/net472 二进制运行不读源码根的实际行为用例；遇到 `TestRepositoryContext` 时必须新建匹配当前 HEAD 的程序集或跳过，不得将旧程序集用于源码型测试。当前可重用行为证据为 16/16；旧 `.tmp/r06-07-emptytables` 图报告绑定 `5046bf8f` 且目录已失，不能声称是当前 main 截图。用户已提供 #962/#963 诊断包：CI SDK均为 `8.0.423`，构建通过，随后同一个 `R08NumericChangeBehaviorTests` 的反馈计数断言失败（0/期望1）；这不是 R06-07/08 生产行为失败。证据包和 ZIP SHA 见 R08-06 当前 main README。R06-08 当前行为复核为 14/14；#964 对应源码 `83fbee97` 失败于“编译与测试”，因当时仍含上述旧测试前提；目前有本机修正，等待新 Actions 终态。

### R06-08 当前行为门禁

旧长详情测试只放两行且断言两行可见，不能证明四行门禁。当前通过的补强是在 STA WPF 窗口加载 10 条合成任务，打开长失败详情并实测至少 4 行完整可见；还核对选中详情对象、错误码、独立详情滚动与行高上限。R06-08 当前复核为 14/14（SDK8.0.423，Release 0/0，XAML24/24）。在新 Actions 对 R08-06 门控修正给出终态后先复核 R00/R01，再推进依赖已满足的 Q/R 小批。

### R08-06 系统动效输入

`NumericChangeFeedback` 默认尊重 `GscMotion.IsEnabled(true)`（High Contrast / Windows ClientAreaAnimation）。R08-06 正例必须用按 TextBlock 状态隔离的 `SystemMotionEnabledForAudit=true` 固定输入，确保在关闭系统动画的 runner 上仍实际采样 WPF 动画时钟；另测 override=false 无动画负例，并覆盖 override 为 null 时跟随真实系统输入。不要为通过 CI 改生产动效策略。用户提供 #962/#963 诊断包显示旧夹具反馈计数0/期望1，CI SDK为8.0.423；工作流SDK与旧 #919/#920 的10.0.401不混写。当前本机 SDK8 solution `0/0`、行为 `5/5`，新 Actions 仍须终态确认；门禁后按当前续跑顺序先复核 R00/R01，再推进依赖已满足的 Q/R。

### SDK 版本复核边界

仓库 `global.json` 固定 SDK `8.0.423`；记录 CI 结果时必须写明实际 `dotnet --version`，不能把 workflow 中的 `setup-dotnet` 版本当作已选版本。若要求 SDK `10.0.401` 而仓库仍锁 SDK8，应使用单独 CI job，在临时 detached worktree 中仅改该 worktree 的 `global.json` 并运行同一 Release build/test 脚本；保留 SDK8 基线 job。SDK9 本机完整构建只算交叉检查，不能替代 SDK10 CI。CI 失败先取完整日志与失败诊断包，不凭摘要猜测。隔离 WPF TRX 中旧架构断言可能标为 `NotExecuted`，应读取其具体 reason 并与通过数分开记录。

### 2026-10-13 当前 CI 与 R00/R01 检查点

`4aa1ec7` 对应 #968/run `38366158855` 已终态成功；SDK8 `8.0.423` 和 SDK10 `10.0.401` 是独立 job。#969/run `38377673615` 是随后的文档同步并成功。#967 的 SDK8 源码门禁因 `verify-sdk-build.ps1` 缺 UTF-8 BOM 失败；同一 `validate-source.py` guard 在本机复现后补 BOM。用户 ZIP 对应绿色 SDK10 job `115178885908`，SHA 与 artifact `11795654027` 一致，SDK `10.0.401`，117 TRX、1566 passed/0 failed。R00/R01 freshness 为 14/14，package identity 未提供；不代表 Playnite 宿主、IME、物理 DPI/跨屏或最终呈现已验证。证据：[R00/R01 当前 main 与 CI 复核](../design/reviews/ui-finesse-round3-20260915/evidence/R00-R01-CURRENT-MAIN-RECHECK-20261013/README.md)。

### 2026-10-13 R05-03 隔离宿主保护点

既有 `scripts/real-host-audit.ps1` 在独立 `--userdatadir` 下未于关闭请求后 20 秒退出，准确错误为 `Playnite isolated profile bootstrap did not exit gracefully; refusing to force-stop it and leave a safe-start marker.` Runner 在扩展安装前退出；配置文件已创建，无 safe-start marker，无插件安装或合成库，真实 Popup 交互/呈现帧没有运行。PID `34512` 是本次 exe/profile；当前观察到 4 个 CefSharp 子进程仍绑定隔离 profile，host 本身仍 responding、HWND 为 0。不要强制终止或删除 profile；等整个树自行退出后再判断是否可重试。证据摘要在 `evidence/R05-03-CURRENT-MAIN-RECHECK-20261013/host-preflight-20261013.json`，runner 原始输出由对应 README 引用。

### 2026-10-13 #967 R08-01 自动证据

用户给的 SDK10 ZIP 内独立 `R08MotionReverseBehaviorTests` TRX 为 `2/2`，已复制该 5.5 KB 精确 TRX 到 `evidence/R08-01-CURRENT-MAIN-RECHECK-20261013/`；包 SDK `10.0.401`。测试 source SHA 与当前相同。此自动证据不解释用户机器 `1/2`；未取得具体失败方法/断言/堆栈/DLL 身份，不能说该故障已修复。

### 2026-10-14 R08-01 按钮反向与 R09 外机失败对照

当前 main 基线 066b9a6c01057c044ddc0d7c63ff70ce3dd7a6b3 仅新增 R08 测试夹具，实际生产按钮模板 hover/press 动画现覆盖切换瞬间、方向推进与最后目标；R08 类 3/3、Q06 按钮状态 1/1。本机 SDK 为 9.0.302，不等价 global.json SDK 8.0.423 或 CI SDK 10.0.401。用户机器 R08 1/2 的原始方法/堆栈/DLL 仍缺，不能声称已修复。

外机 R09 日志的 Expected 2 / Actual 1 与历史 3f7d0b30 的 Chrome 2 DIP 断言相符；f13b9e05 已根据生产 ValidationStroke 分层将测试改为 Chrome 1 DIP + 错误层 2 DIP。当前 main R09 本机 SDK9 2/2；未取得外机 HEAD 与 DLL/PDB 身份，故旧分支、旧构建产物或源码/PDB错配仍只是最可能解释。证据分别见 R08-01 与 CI-R09 外机复核包。

截至 2026-10-14，公开 run 38393317198 (#971) 仍 In progress；#978/run 38509928015 Failure，完整日志需登录且 runner SDK/根因未知。推送本批后继续跟踪当前 SDK8/SDK10 lanes；CI 未终态前不记门禁完成。下一项 R08-02。真实 Playnite、OS 输入/读屏、物理 DPI/跨屏、呈现帧、ETW 与宿主性能仍待验。

### 2026-10-14 R08-02 应用与系统动效热关闭复核

当前 main 的应用设置开关与系统策略变更均复用 `GscMotion.NormalizeAll` 清除活动时钟，不重建动画服务。SDK `9.0.302` Release `0/0`；应用 ToggleSwitch `1/1`、调用真实 `OnSystemParametersChanged` 的实例级系统策略模拟 `1/1`、生产侧栏 reduced-motion `2/2`、Dashboard/Settings 订阅与卸载保护门禁 `1/1`。两种热关闭均验证活动入场时钟清理、稳定值 `Opacity/Y=1/0`，重新开启后不重播旧动画。系统策略通过实例级内部测试 provider 输入；未改 Windows 设置或触发真实静态系统事件。证据见 `evidence/R08-02-CURRENT-MAIN-RECHECK-20261014/README.md`。WPF 测试退出仍有 TextServices COM 清理异常噪声，根因未知、退出码成功。本机 SDK9 不替代 SDK8.0.423/CI SDK10.0.401；CI 推送后须分别追踪终态。R08-03 隐藏/最小化 spinner 停机已按当前 main 刷新复核，实际 Windows 动效通知、Playnite、UIA/读屏、物理 DPI/呈现帧/ETW/宿主性能仍未验。下一项 R08-04。

### 2026-10-14 R08-03 当前 main 隐藏与最小化停机复核

R08-03 生产服务、模板与 storyboard 已存在，未改生产代码。R08-02 代码阶段在提交前对最终 `31d64e42` 源内容完成 SDK `9.0.302` 隔离 Release build `0/0`；构建 ProductVersion 是父提交 `d3cb0b2d`，源码/程序集 SHA 与 MVID 独立记录。提交后对同一 hash-identified 测试程序集执行 `IndeterminateProgressPausesForHiddenTabAndMinimizedWindowThenResumes`，实际运动、隐藏暂停、忙态保留、恢复运动和最小化暂停均通过 `1/1`。新隔离 build 尝试在 MSBuild ProjectReference target 以 exit 1/0 warning/0 error 失败，根因日志无 MSB/NU 信息；不记为 build pass。证据见 `evidence/R08-03-CURRENT-MAIN-RECHECK-20261014/README.md`。当前 Actions 公开页仍只列到 #979，没显示 `31d64e42` run；#971 仍 In Progress、#978 Failure 且日志需登录，须继续查 SDK8/SDK10.0.401 到终态。下一项 R08-04 业务完成节奏；真实 Playnite/呈现/物理最小化/ETW/宿主性能仍未验。

### 2026-10-14 R08-04 当前 main 业务完成节奏复核

先复用 `BusyOperationCoordinator`、共享按钮忙态/延时 spinner、`FeedbackToast` AutomationPeer、`DashboardView.ApplyToastAutomation` 和 `TaskStatusDto`；生产代码、命令绑定和安全语义未改。新增一条成功慢任务行为：未完成的 action 保持忙态且不触发完成，放行后回调一次、恢复忙态，不触发失败/取消反馈；成功、失败（错误码与详情优先）、取消 DTO 的状态和详情使用精确值断言。隔离 SDK `9.0.302` Release solution `0 warning / 0 error`，`R02BusyStateTests 5/5` + `R08BusinessFeedbackBehaviorTests 4/4`，共 `9/9`；本机缺 global.json 的 `8.0.423`，该结果不替代 SDK8/CI SDK `10.0.401`。源校验和 diff check 通过；WPF 静态检查 `0 errors/29 warnings/177 info`（扫描包含无关 `.tmp/r12-04-stage` 历史布局 warning）。完整测试/程序集身份和限制见 `evidence/R08-04-CURRENT-MAIN-RECHECK-20261014/README.md`。第一次没有传 `GSC_BUILD_COMMIT` 的源契约用例拒绝未知身份程序集；按仓库构建约定重建后完整定向类通过。公共 Actions 页仍停到 #979 queued；#971 In progress，#978 Failure 且需登录获取日志；当前 R08 提交的 SDK8/SDK10 CI 尚未可见。真实 Playnite/Worker/Narrator、外部 UIA、宿主通知、呈现帧/物理 DPI、ETW 与宿主性能仍待验。下一项 R08-05。

## 2026-10-14 R09-05 外机失败当前 main 复核

- 当前 main 552b8168 的隔离 SDK 9.0.302 Release solution build 为 0 warning/0 error，R09FocusOutlineBehaviorTests 2/2。用户所附 SDK10.0.401 ZIP（commit 983fee5）与 SDK8.0.423 ZIP（commit c9f9179）内对应 R09 结果也为 2/2；commit 5a6647c 的另一 ZIP 不含 R09 TRX。
- 当前 test source SHA 为 A5AAB9E…B2CBC98；插件/测试 DLL SHA、MVID、ProductVersion 以及脱敏 TRX 已记录在 CI-R09-OTHER-MACHINE-FAILURE-RECHECK-20261014。没有更改生产模板或降低断言。
- 外机日志第 151 行、Expected 2/Actual 1 对应历史 Chrome 2 DIP 断言；当前第 151 行是错误色、第 154 行读取 ValidationStroke 2 DIP。旧测试程序集/分支或 PDB 与源码错配是推断，缺少外机 HEAD/DLL/PDB 身份，不能确认。libpng iCCP 与该断言无关；TextServices COM 异常发生在失败后的 WPF 收尾。
- #971/run 38393317198 公开页仍为 In progress，日志需登录；#978/run 38509928015 为 Failure，完整日志需登录。Actions 列表仅到 #979（ae3c565），未显示当前 HEAD run；SDK9 本机结果不替代 SDK8/SDK10.0.401 CI。
- 用户暂时不可回复，所需的外机提交、工作树、测试 DLL SHA/ProductVersion 与源码 145–160 行记录为待验，不阻塞当前 R08-05。
## 2026-10-14 R08-05 页面切换当前 main 复核

- 现有生产页缓存与 PageHost 行为已满足任务方向，没有重建页面服务或动画体系。本次没有生产代码/XAML 改动，只复核真实 shell/page 行为并刷新当前 main 证据。
- 隔离 SDK 9.0.302 Release solution build 0 warning/0 error；R08PageSwitch 4/4、ProductionShellChrome 22/22、UiFinesseFoundation 9/9、R02 Busy 5/5、R08 business feedback 4/4，共 44/44。source validation 通过，XAML 结构 24/24，文档同步后的 diff check 另行校验。详细 SHA/MVID/TRX 在 R08-05-CURRENT-MAIN-RECHECK-20261014。
- 真实 WPF 页面切换保留 DataGrid offset 10→10、任务引用、选中项与 DataContext；两次不同页面切换各 Measure/Arrange 1/1，同页重入 0/0；96 项集合和筛选/页签状态恢复，refresh command 0 次。七次快速路由后导航标题/焦点一致；PageHost 无 Blur/整页入场动画。
- ShellChrome 22 项均通过、TRX exit 0；testhost 收尾出现 TextServices COM 异常输出，根因未知。误用类名的零测试筛选器未计入 44/44。
- 本机 SDK9 不替代仓库 SDK8.0.423 或 CI SDK10.0.401。公开 Actions 仍只显示 #979（ae3c565），#971 In progress，#978 Failure 且日志需登录；推送的 R09 文档提交 8dda9995 尚未显示 run。真实 Playnite、物理 DPI/跨屏、presented frame、UIA/读屏、ETW/宿主性能仍待验。
- R08-05 当前 main 受控条件已满足，视觉不适用，不生成截图。下一可执行任务 R07-08 当前 main resize 复核；先查现有实现/测试及 #960/#968 的提交边界，不凭旧失败摘要猜修。
### 2026-10-14 R07-08 当前主线复核

R07 resize 回归应按已加载 PageHost/DataGrid 的实际窗口与内容区尺寸验收；compact 详情关闭后须路由生产 open button 才能证明详情可在 resize 中保持。复核五阶段的 PageHost 实际几何、picker/焦点/任务表可达、详情/picker MaxHeight 清理和恢复误差。SDK9 单机行为通过不能解释或覆盖 #960 未知 CI 失败，也不等同真实拖动帧时间/Playnite。当前证据与程序集身份见 evidence/R07-08-CURRENT-MAIN-RECHECK-20261014。

## 2026-10-14 R10-02 当前游戏定位行为复核

- 复用 `TaskSourceNavigationResolver`、`TaskStatusDto`、`GamePickerViewModel` 和 Shell 当前游戏选框。代码提交 `13704e75` 仅增加默认关闭的内部工作区加载审计回调与行为用例；正常生产 Worker 调度路径不变。
- 本机 SDK `9.0.302`，Playnite `net462`、测试 `net472`，Release 编译成功；R10 context `5/5`，含同名重名选框正例、缺失 ID 不回退负例、生产任务命令准确路由与缺失目标保护。相邻导航/来源组 `25/25`，无失败/跳过。source validator、diff check 通过。
- 源码/DLL/MVID/TRX 哈希、命令与行为结果在 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R10-02-CURRENT-MAIN-RECHECK-20261014/README.md`。构建 ProductVersion 带前置 hash `a94819b`，因此以源码/DLL SHA/MVID 标识实际构建，不冒称 SDK10 CI。
- Actions 列表最新可读 #972（`8546aca`）仍 In progress；本轮尚未推送。用户指定旧 run #920/`36677346317`（`77de7f4`）现 Failure、7m04s、无 artifact，日志登录限制，实际 SDK/根因未知。真实 Playnite/媒体服务、presented frame、物理 DPI/跨屏、OS UIA/读屏、ETW/宿主性能未验。CI 终态后下一项 R10-03 搜索快捷键。

## 2026-10-15 R10-02 当前 main 复核

- R10-02 延用任务稳定 ID 路由与全局 GamePicker。本机 SDK `8.0.423` / VSTest `17.11.1`，Playnite `net462` 与测试 `net472` 的 Release 项目/引用构建 `0 warning / 0 error`；R10 及相邻导航行为 `25/25`，新增生产 Shell 同名选框 STA 交互后合计 `32/32`。XAML `24/24`，source validation/diff check 通过。
- 新交互测试从生产顶部按钮打开当前游戏选框，在相同显示名中选择具体 ListBox 项，验证选框关闭、焦点归还、对象引用和 `PlayniteId` 均准确，且当前游戏名称绑定显示。任务命令已有按 `GameId` 导航正例和目标缺失时保持原状态的负例；未增加生产功能代码。
- SDK8 本机不替代 SDK10.0.401 CI；`6eac7b90` 与证据文档提交的 Actions run ID/终态尚未读取。当前证据包保存六份脱敏 TRX、程序集身份与边界。真实 Playnite、媒体后端、IME/物理输入、UIA/读屏、物理 DPI/跨屏和呈现帧未验。

## 2026-10-14 R10-07 / R10-08 当前 main 复核

- R10-07 复用现有侧栏实现，Release STA 行为 22/22；首次未设置 GSC_BUILD_COMMIT 的身份检查失败不是产品回归。有效测试设置仓库约定的构建身份、重建后执行，源/DLL/MVID/TRX 见 evidence/R10-07-CURRENT-MAIN-RECHECK-20261014/README.md。
- R10-08 复核发现 GSC-058 把首页最近访问卡片永久折叠，历史测试手动改可见性后才点命令，无法证明入口可达。复用已有卡片、DTO、共享按钮模板，增加同一概览区域内最近任务/最近访问切换；通过 STA Window 验证焦点、Automation 名称、双方可见性与列表分离，以及合成访问行命令。R10RecentAccessBehaviorTests 2/2，没有新增服务/DTO/选框/滚动条或用户设置访问。
- 本轮本机 SDK 9.0.302、VSTest 17.14.1；Playnite net462 与测试 net472 Release build 成功；R10-08 2/2、R09 2/2、R10-07 22/22；source validation、XAML 24/24、RenderHarness Light/Dark render-qa OK/0 PROBLEM、diff check 通过。WPF 静态检查 0 errors/29 warnings/177 info，Overview 的 scroll/StackPanel warning 是静态审查命中；本轮未改滚动模型。离屏截图仅覆盖默认最近任务模式。
- 外机 R09 日志的旧 Chrome 2 DIP 期望与当前 Chrome 1 DIP、独立 ValidationStroke 2 DIP 区分；外机 HEAD、源、DLL/PDB 身份仍未知，错配只是推测。libpng warning 不在失败调用栈；TextServices COM 异常在 WPF 失败后的收尾阶段。
- #993/run 38570078343 对 e0a72c9 终态失败：SDK10 job 115872658151、默认 build job 115872658675 均 exit1；日志 API 403、公开 job 页面要求登录，失败根因未知。SDK9 不替代仓库 SDK8.0.423 或 CI SDK10.0.401。当前批次待提交推送后跟踪新 Actions；CI 日志受限时记录限制并推进 R11-01。真实 Playnite/host、物理 DPI/跨屏、最终呈现、外部 UIA/读屏/IME、ETW/宿主性能仍未验。

## 2026-10-14 R11-01 当前 main 复核

- R10-07 侧栏 ProductionShellChromeSourceTests 22/22，R10-08 最近访问入口 2/2。R11-01 复用已有实现，Release 当前 main 行为及 Save 邻接类 17/17；本轮不重建 DTO、DataGrid 或命令。
- SDK9.0.302/VSTest17.14.1 重建 Playnite net462 与测试 net472，0 warning/0 error；source validator、XAML 24/24、git diff --check 通过。
- 真实 SaveCenterView/隔离 STA Window+合成 DTO 验证七列、未知来源/恢复状态、Ready 文案和锁定不显示安全绿；邻接类覆盖空态区分、排序正/负路径、列宽最小值/保存恢复/选择性重置。脱敏 TRX、源/DLL/MVID/PDB SHA 在 evidence/R11-01-CURRENT-MAIN-RECHECK-20261014/README.md。
- 无视觉源码改动，不生成截图；真实 Playnite/package-host、UIA/读屏/IME、物理 DPI/跨屏、最终呈现帧、ETW/宿主性能未验。
- 58090d99 已推送；匿名 GitHub Actions API 查询返回 HTTP 403 rate limit。最新公开 Actions 页面是旧缓存快照：显示 972 workflow results，最新可见 #972（head 8546aca）In progress；较早抓取又曾显示 #979。两个快照均不含 58090d99，不能确认该提交的 run 是否创建、也不知道终态。继续间隔查询；在结果明确前不报 CI 完成。 SDK9 不替代 SDK8.0.423/SDK10.0.401。下一项 R11-02 当前 main 复核。

### 2026-10-14 R11-04 版本备注与外机焦点日志

R11 版本说明已有 Dashboard 编辑/取消、Ludusavi 元数据更新与 SQLite 按 `(PlayniteId, BackupId)` upsert。取消必须由生产 `CancelBackupMetadataEdit` 恢复备注/锁定值并保持 DTO 和 BackupId；页面夹具若用测试 ICommand 适配器，应明确只证明真实 View/Binding 到生产方法的路径，不称为真实生产 RelayCommand 实例。重复备注只能由稳定 BackupId 选中；重启读取测试应从真实生产 parser 解析合成 Ludusavi JSON、在隔离数据库 upsert 后由新 Store 实例读回，而非仅手工构造 DTO。

外机 R09 `Expected Thickness(2)` / `Actual Thickness(1)`、行151与旧 commit `3f7d0b30` 的 Chrome 两 DIP 断言吻合。当前应检查 Chrome 1 DIP 和独立 ValidationStroke 2 DIP；仅本机复验无法得知外机 source/DLL/PDB。将旧源/程序集或 PDB 错配保留为未证实推断。`libpng iCCP` warning 和失败后的 TextServices COM shutdown 异常不能解释厚度断言。SDK8 当前 main 复跑用于本机对照，不替代 SDK10 CI。

### 2026-10-15 R10-03 搜索快捷键当前 main

R10-03 复用现有 `SearchShortcutPolicy`、PreviewKeyDown、Shell 搜索命令和工作区 TextBox 映射；当前 main `95109c2` 的生产路由行为补为真实 WPF routed-event，Media 页选中时实际把键盘焦点移到 `MediaSearchTextBox`。弹层占用、Ctrl+Z/C、普通 F 负例确认事件未消费且焦点不变。SDK8.0.423/VSTest17.11.1 定向 Release build `0/0`，相关 R10 组 `13/13`；R09 同身份对照 `2/2`。脱敏 TRX、二进制身份与边界见 [R10-03 证据包](../design/reviews/ui-finesse-round3-20260915/evidence/R10-03-CURRENT-MAIN-RECHECK-20261015/README.md)。真实 Playnite、全局热键协作、物理键盘/IME、UIA、呈现、DPI/跨屏/ETW/宿主性能未验。

`95109c2` 对应 run `38645600070` 上次为 In progress；本次 API 连接被拒绝，浏览器读取超时，本机无 `gh`，所以终态未确认。推送新的 R10-03 阶段后另查新 Actions run；SDK8 结果不得替代 SDK10.0.401。下一项 R10-04 快捷键帮助。外机 R09 仍需要相同失败运行的 `summary.json`、完整 console/TRX 与实际测试 DLL/PDB 身份；截至用户可回复前继续其他任务。
## 2026-10-15 R10-04 当前 main 快捷键帮助复核

R10-04 已复用现有 KeyboardShortcutHelpCatalog、FocusWorkspaceSearchCommand 和 CanExecute(null)，没有重复实现。基线 main 6cf279238d55fe3d139fa2cb0eb7b09229a7478a；本机只有 SDK9.0.302/VSTest17.14.1，仓库固定 SDK8.0.423 不可用。本轮隔离全解方案 Release build 0 warning / 0 error，R10帮助和相邻行为 18/18，R09 2/2，XAML 24/24、source validation/diff check 通过。实际 Shell Popup 开闭、当前媒体页 Ctrl+F 条目、生产命令禁用后的空列表均通过；没有生产代码或 XAML 改动。CI run ID/终态未知，SDK9 不替代 SDK8/SDK10.0.401。外机 R09 根因仍未知，需外机实际 HEAD/source/DLL/PDB 身份。证据：docs/design/reviews/ui-finesse-round3-20260915/evidence/R10-04-CURRENT-MAIN-RECHECK-20261015/README.md。下一项 R10-05 筛选预设；先检查已有模型和持久化，不重建服务。
## 2026-10-15 R10-05 当前 main 筛选预设行为补证

R10-05 沿用现有 FilterPresetDefinition、设置规范化、任务/媒体 preset 命令及确认流程。原工具栏按钮用测试命令，生产快照/应用方法此前只有源码接线断言，因此新增一个 STA 行为用例调用生产 CreateTaskFilterPreset 和 ApplyTaskFilterPreset，验证标量筛选状态恢复，以及状态/游戏/类型动态项过期后回退“全部”。非法/旧配置回退、JSON scalar roundtrip、真实重命名/删除取消保护继续通过。只改测试，无生产代码/XAML变更。

基线 e2353aa38e8c125d49ab1a1a37d67d37e1cbca8c，本机 SDK9.0.302/VSTest17.14.1（SDK8.0.423未安装）；全解方案 Release build 0/0，R10 preset 6/6、TaskCenterViewResponsive 8/8、Q14 toolbar 1/1，合计15/15，XAML24/24、source validation/diff check通过。Q14通过后出现testhost COM teardown诊断但exit0。SDK9不替代SDK8/CI SDK10.0.401。用户设置和真实Playnite未触碰。CI run待确认；下一项 R10-06 来源条件清除行为。证据：docs/design/reviews/ui-finesse-round3-20260915/evidence/R10-05-CURRENT-MAIN-RECHECK-20261015/README.md。


### 2026-10-15 CLOSE-WRAP SDK8 全量门禁与外部输入边界

当前 main b04660e0 的 WorkspaceTreeGovernanceTests 旧 Trainer/Media typed getter 断言改为 generic registry 路由；实际 PageHost/导航/焦点行为由 WPF 测试覆盖，定向 8/8。只改测试，不改生产 UI。隔离 SDK8.0.423/VSTest17.11.1 完整 Release solution build 0/0、XAML24/24、Core125/125、Worker356 passed/1 skipped、Playnite source455 passed/27 skipped。122 个 WPF 类已全部执行，648 passed/39 skipped/1 failed；Q14 654 DIP 行距1/1。

唯一失败 R05TooltipTimingBehaviorTests.QuickMousePassesCancelTooltipAndAStationaryDwellOpensIt 在窗口显示前 GetCursorPos 返回 false；该类2/3，余下78个类另行单类隔离均通过。总计1584 passed/67 skipped/1 failed，不把系统指针输入未用计为产品缺陷，也不将其改成跳过。证据 docs/design/reviews/ui-finesse-round3-20260915/evidence/CLOSE-WRAP-CURRENT-MAIN-RECHECK-20261015/README.md。推送后追踪SDK8/SDK10 CI；下一本机任务重查R00/R01 freshness，之后CLOSE-OPTICAL-01。

### 2026-10-15 R08 hosted motion test fixture

Actions #1024 (`38677622085`) 的 SDK8.0.423 与 SDK10.0.401 job logs 都证明 `R08MotionHotChangeBehaviorTests` 没启动 opacity entrance（`opacity=1; loaded=True`）。测试在 `Window.Show` 前设置时长，但初次 `ApplyAdaptiveTheme` 会基于真实 Windows 动效策略重写资源。夹具应在初次主题应用后注入测试时长；应用开关和系统策略仍分开由各自测试覆盖。此轮本机 SDK8.0.423 build `0/0`、两类行为 `1/1`、source validation/diff check 通过；不要把它当成新提交的 SDK10 CI 结果。证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/CI-R08-MOTION-POLICY-FIX-20261015/README.md`。

### 2026-10-15 R08 system-motion fixture follow-up

Actions #1025 (`38687483121`) 的 SDK10.0.401 日志指向 R08 system-motion 测试第56行没有检测到 opacity 动画时钟。仅等 `Show/UpdateLayout` 不保证 Loaded 队列已完成；测试现在先 drain dispatcher 并重新布局，再注入测试时长并触发被测入场动画。SDK10 artifact `11951470215` SHA-256 `7c328cf7b16c0d641dc0bdc61e6095d24e33b48ae69d0c9b4f1f5ddb812e4893`。同 run SDK8 job `116265550259` 报 R09HighContrast 第195行 Assert.True（Expected True / Actual False）；同身份本机 SDK8 3/3 未复现，先保留为未定位的独立 CI 失败，不将它归到 R08。artifact `11951095969` SHA-256 `8d928e1b58657bb058b243aeb96de0cf8e5906b5bed46a9ffc90606181dd34cd`。本机锁定 SDK8.0.423/VSTest17.11.1 build `0/0`，R08 system/app `1/1`、R09 high-contrast `3/3`；两个完整 CI log 已取得。新补丁推送后跟踪双 SDK Actions 至终态；其后按用户顺序重验 R00/R01。

### 2026-10-15 TaskCoordinator 取消 Gate 生命周期竞态

`TaskCoordinator.RunAsync` 从 `_taskRuntimes` 移除 runtime 时，另一个 `CancelAsync` 可能已读出同一个 runtime 并仍在等待/使用 `SemaphoreSlim Gate`。不可在移除时 Dispose 这个 Gate；让 semaphore 在并发调用释放引用后由 GC 回收。`CancellationRequested` 必须在终态判断前检查，使已接受取消的重复请求继续返回成功。CI #1026 SDK10.0.401 真实日志确认旧实现的 `ObjectDisposedException` 正是在 `CancelAsync` 的 `Release` 上抛出。修复后的本机 SDK8.0.423 目标回归 `1/1`、Worker `356 pass / 1 环境 skip`；新 SDK10 CI 仍待结果。

### 2026-10-15 R00/R01 freshness on 5862856e

`check-ui-evidence-freshness.ps1` 对 `5862856e508fdb0abf2478fbbc54c34b34075692` 的 14 条 R00/R01 记录给出 `14 fresh / 0 stale`，没有命中登记源路径，package identity 为 `not-provided`。该脚本只证明相关源路径未失效，不能证明重新构建、安装包或 Playnite 宿主。R00/R01 行为和负例证据此前已经满足；Worker `TaskCoordinator` 修复不触及登记范围，因此不重建功能。

### 2026-10-15 R09 high-contrast press fixture

Actions #1027 (`38698959702`) 与 #1028 (`38701408503`) 的 SDK8.0.423/SDK10.0.401 完整 job logs 都在 R09 high-contrast matrix 的合成 `RaiseSpaceKey` 路径后报告 bare `Assert.True` Expected True/Actual False；后续 `TextServicesContext` COM 异常也会在通过的本机测试退出时出现，不能当成该 xUnit 断言的根因。现用 `HighContrastStateProbeButton.SetPressedStateForProbe(true/false)` 驱动按钮模板状态，再检查 overlay、动画时钟与释放后的负例。生产样式不变。SDK8.0.423 本机 Release 测试项目 build 成功，R09 `3/3`、Q06 routed Space 状态类 `1/1`；物理键盘仍未验。证据与 CI artifact 元信息见 `evidence/CI-R09-PRESSED-STATE-RECHECK-20261015/README.md`。修正提交推送后必须确认 SDK8 与 SDK10.0.401 Actions 终态。

### 2026-10-15 R21 UIA selection CI follow-up

Actions #1029 两条 SDK lane 在 R21 DataGrid `ISelectionProvider.GetSelection()` 查询时都得到空集合；CI 没有记录 WPF SelectedItems 或行容器，故实际根因尚未证实。本机 SDK8.0.423 / .NET Framework 4.8.9181.0 对照通过。当前测试夹具在查询前显式滚动、布局并检查真实选中项和生成的 DataGridRow，同时保留 UIA 正向项数/名称断言。修改后 R21 `24/24`；尚待新提交 SDK8/SDK10.0.401 CI 终态。只改测试，不改生产 UI/绑定/命令。

`.tmp` 盘点约 1.09 GiB；约 714 MiB 为后续 SDK8 构建工具，约 374 MiB 为过期隔离输出。批量清理被自动审查拒绝，因为尚有未归档 TRX/日志且 dotnet PID `32408` 无法识别/确认占用；没有通过其他方式删除。D 盘当时剩余 `8.93 GiB`。旧输出只有在逐份确认/归档并消除进程占用风险后再清理；此事不阻断其他任务。

### 2026-10-16 R21 Selection peer 子树复核

Actions #1030/run `38712426496` 的 SDK8 与 SDK10.0.401 job 均在 R21 父级 `ISelectionProvider.GetSelection()` 返回空集合处失败；CI 已确认 SelectedItem、唯一 SelectedItems 和已实现 DataGridRow 前置条件通过，根因仍未知。新夹具先断言生产 `SelectionUnit.FullRow`，再枚举 grid peer 子树并用 `ISelectionItemProvider.IsSelected` 检查首行、旧行取消和新行选中。只改测试；本机隔离 SDK8.0.423、.NET Framework 4.8.9181.0 下 Release build `0/0`、R21 `24/24`。源码 SHA `E13552DA73AF1FE8D72715CE0D9A0C6D5FC1B36DB0564C359701C58226D77D5C`，脱敏 TRX SHA `E33A440A00897E7DC830CA35232B67B0441170F03D81643980E63242EC330550`。新修订待 push 后追踪 SDK8/SDK10.0.401 Actions 终态，证据见 `evidence/CI-R21-SELECTION-PEER-ENUMERATION-20261015/README.md`。真实 Playnite/Windows UIA、OS 输入、DPI、呈现与宿主性能未验。

先前的清理请求曾被自动审查拒绝；用户随后明确授权五个精确目标，已按授权范围清理并释放 `373.7 MiB`。当前 `.tmp` 约 `744.9 MiB`，保留 SDK8 工具链和 R21 复现目录；PID `32408` 仍无法识别。

### 2026-10-16 临时目录清理

收到用户对五个旧阶段目录的明确授权后，清理了 `.tmp/q24-07-official-postcommit-0243c108`、`.tmp/r11-06-40af39c8-20261015`、`.tmp/r09-current-main-retry-20261015`、`.tmp/r00-08-55ed3a17-20261015` 和 `.tmp/r08-01-production-button-sdk9-20261014/build`，共释放 `373.7 MiB`。没有删除 SDK、当前 R21 复现输出或其他任务文件；剩余 `.tmp` 约 `744.9 MiB`，D 盘可用 `9.26 GiB`。PID `32408` 仍无法识别，但删除范围严格限于用户明确指定授权的路径。
