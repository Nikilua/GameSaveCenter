# GameSaveCenter 长期项目记忆

更新：2026-10-07。本文件保留稳定规则、架构和已知陷阱；当前结果以 [CURRENT_STATE.md](CURRENT_STATE.md) 为准，逐阶段历史见 [WORKLOG.md](WORKLOG.md)。原 6066 行完整原文已保留于 [历史记忆快照](PROJECT_MEMORY_HISTORY_THROUGH_20260930.md)，其中旧版本、优先级与下一项不是当前指令。

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
- SDK声明以global.json精确8.0.423/disable/不允许preview为准，CI通过global-json-file安装；C#12.0而非latest。缺失版本须明确失败，不能以另一预装SDK替代。SDK升级另做完整编译/测试/包验证；本阶段不新增兼容通道。原生Get-Command可能返回多个PATH匹配，必须选首个Application；PS5.1/7用两个不同原生程序的同名夹具验证优先级与环境恢复。
- 带预期非零native负例的自测入口须在finally之后显式exit0；Actions pwsh外层会继承LASTEXITCODE，出现断言全部通过但步骤失败。不能在采集器清零实际负例；异常路径仍非零。自测诊断也纳入artifact目录，上传只JSON/TXT/TRX，不能以always步骤success当作artifact存在。
- TRX脱敏除TestRun.name/computerName/路径，还须覆盖runUser和runDeploymentRoot；保留GUID、方法/断言/堆栈/计数和DLL身份。合成元数据负例同时检验异常路径仍非零。
- run37565955357/37566483862已实际失败上传并下载：SDK10侧栏22/22通过，native偏好记录ClientAreaAnimation=false；仍在Q14错误态36→37 DIP失败。STA干预确认2 DIP错误边框改变测量；独立私有几何夹具复现Light/Dark654 DIP单行20 DIP margin，620/660通过。探针不是生产修复或宿主验收。
- SDK阶段历史：2026-10-07本机侧栏22/22、source470/18、WPF前23类通过；Q14独立及全量先因错误态TextBox高度36→37 DIP失败，未到654 DIP，原边距问题继续开放。GscMotion/build变更还命中R00-03/R01-01/R01-04，补证范围增加，baseline不能静默转绿。
- 受控自动行为、离屏截图、真实宿主、物理 DPI/跨屏、包安装身份分别记账。历史宿主成功不签收当前候选。
- IPC取消通过dispose解除IO时，任务可先以IOException或EOF结束；仅在linked token确已取消的transport路径按host→caller→timeout分类，并保留RequestId/未知提交，不覆盖服务拒绝。测试服务用握手、每场景独立管道和失败路径异步清理，不用固定延时猜取消时机。
- 当前活动缺口：联合UI全量WPF/完整CI与包门禁、共享按钮对齐和输入/行距的联合签收、10项evidence stale、Media Inbox宿主滚动、Q06/R08原用户失败、发布恢复矩阵。IPC已实现且200专项/source470通过，Core125/Worker357通过；完整门禁仍未通过。最新覆盖与身份见CURRENT_STATE，不沿用历史绿灯。
- R00/R01在10月5日基线是9 fresh/5 stale；10月7日另命中R00-03/R01-01/R01-04，提交后重新计算。只改baseline身份无效，必须补行为/负例。d57a613f本地包已有独立hash/程序集身份，未安装；包结构通过不代替整体WPF/CI/宿主验收。
- ENV-001 历史 WMI 命令行读取拒绝、CEF 0x5 与单实例边界尚无解除证据；条件变化后先做 fail-closed 预检，不关闭用户实例，不反复撞同一边界。
- Q06/R08 本机不复现不能关闭用户问题；需要失败方法、断言、堆栈、DLL 身份。COM 清理日志不自动解释测试失败。
- 当前任务状态和完成定义只维护在 [活动积压清单](../AUTONOMOUS_BACKLOG.md)；Q/R 账本是证据矩阵，不能合并为产品完成率。

- 2026-10-07共享UI阶段：错误描边不得改变原生内容测量；行距按Arrange后的layout slot判断实际多行并恢复作者margin，Unload释放LayoutUpdated。Q14已通过定向及首轮全量入口；随后R02失败由旧clean DLL同样复现。共享按钮须区分文本的有限Stretch测量与视觉树的HorizontalContentAlignment，避免拉伸固定/Auto列产生额外DIP。原4项已过，联合完整门禁待验；当前受影响证据为10项，旧渲染不能签收新增按钮模板。

## 文档维护约定

CURRENT_STATE 保持短摘要，PROJECT_MEMORY 保持稳定规则，HANDOFF 保持下一步；WORKLOG 保留阶段记录。不要向三份入口反复复制完整 TRX 数字与过期下一项。证据全文独立归档，入口链接到同一来源。

历史快照与旧 ledger 状态原文保留；任何状态升级必须有对应证据。临时文件删除前确认绝对路径在仓库 artifacts/.tmp 内，使用 PowerShell `Remove-Item -LiteralPath -Recurse -Force`，只清理已确认不用的本轮产物。
