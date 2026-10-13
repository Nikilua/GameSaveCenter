# GameSaveCenter 长期项目记忆

更新：2026-10-13。稳定规则见本文件，当前结果以 CURRENT_STATE.md 为准。当前顺序是修复并收口 #967 的 CI 门禁，再复核 R00/R01，之后推进依赖满足的 Q/R。#965 布局测试主机尺寸修正已在 `5eaa12bf`，#966 SDK8 run 成功。新增 SDK10 job 在 #967 对 `983fee5` 用 SDK `10.0.401` 全步骤成功（用户 ZIP SHA 与 artifact `11795654027` 一致）；同一 run 的 SDK8 源码结构检查失败，`python scripts/validate-source.py` 本机复现为新建 `verify-sdk-build.ps1` 缺 UTF-8 BOM，添加 BOM 后通过。SDK8 原始日志仍未读到，新的 CI 必须验证该修正；SDK10 成功不得替代 SDK8 终态。R00/R01 上次 freshness 为旧源身份，后续需重跑并只记录新事实。

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
- 当前活动缺口：#960 状态现有记录与最新公开页冲突（历史记录为 failure、当前页为 In progress），#961（main `73d0f6e6`）仍 In progress；都需到终态。#919/#920 的日志 ZIP 已在仓库证据包，可确认实际 SDK `10.0.401` 与两项旧侧栏 transition 测试失败；此结果绑定旧源码。R06-05 不能在 #961 仍运行时签收；R06-06 本机复核与脱敏证据已归档，文档提交后的 Actions 仍需核查。下一项先复核已实现的 R06-07 当前 main 证据，再按账本继续。R00/R01 freshness `14/14`（source identity `5762c874`）；package identity 与真实宿主验收独立待验。R02-06 受 ENV-001 阻塞，Q06/R08 用户故障仍需原始失败方法、堆栈和 DLL 身份。
- R00/R01 当前 freshness 为14 fresh/0 stale：10条匹配源码变更的记录已按当前源码提交更新证据；4条无匹配源路径，继续引用原证据。package identity 未提供。source freshness 不表示包、真实宿主或全部任务验收。
- ENV-001 历史 WMI 命令行读取拒绝、CEF 0x5 与单实例边界尚无解除证据；条件变化后先做 fail-closed 预检，不关闭用户实例，不反复撞同一边界。
- Q06/R08 本机不复现不能关闭用户问题；需要失败方法、断言、堆栈、DLL 身份。COM 清理日志不自动解释测试失败。
- 当前任务状态和完成定义只维护在 [活动积压清单](../AUTONOMOUS_BACKLOG.md)；Q/R 账本是证据矩阵，不能合并为产品完成率。

- 2026-10-07共享UI阶段：错误描边不得改变原生内容测量；行距按Arrange后的layout slot判断实际多行并恢复作者margin，Unload释放LayoutUpdated。Q14已通过定向及首轮全量入口；随后R02失败由旧clean DLL同样复现。共享按钮须区分文本的有限Stretch测量与视觉树的HorizontalContentAlignment，避免拉伸固定/Auto列产生额外DIP。原4项已过，联合完整门禁待验；当前受影响证据为10项，旧渲染不能签收新增按钮模板。

- 2026-10-12 CLOSE-PICKER 复验：在 STA WPF ViewModel 测试夹具中没有 `SynchronizationContext` 时，`Task.Delay(...).ConfigureAwait(false)` 的延续可能运行在线程池；刷新 WPF `CollectionView` 必须回到创建它的 Dispatcher。保留 20 ms 输入合并，取消的旧 Dispatcher 回调还要在执行时检查 cancellation token。新负例先在旧实现失败、修复后 R05 全量类通过。生产 TextBox 错误态两 DIP 描边采用独立 `ValidationStroke` overlay，因此行为测试应检查 overlay 的 Opacity/Thickness，并断言布局 Chrome 仍为 1 DIP；不能要求 Chrome 为 2 DIP。

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
