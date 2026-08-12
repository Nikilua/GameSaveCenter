# GameSaveCenter AI 开发工作日志

> 每完成一个有意义的阶段追加一条；只记录对未来开发有帮助的信息。

## 2026-08-13 DEV-INSTALL-002 一键安装自动提权与停止竞态修复

**实现内容：**

- `scripts/dev-install-run.ps1` 在构建前检查当前 PowerShell 是否为管理员；普通会话自动通过 UAC 重启自身，并原样转发配置、安装路径、`-NoStart` 与 `-SkipClean` 参数。
- 停止进程时先尝试当前会话；权限不足时只对已确认的 PID 请求管理员停止，并在提权进程中再次核对进程名，避免 UAC 等待期间 PID 被复用后误杀其他进程。
- 保留 `Get-Process` 与 `Stop-Process` 之间的退出竞态处理，并在超时抛错前增加最后一次进程确认；已经退出的 Worker 不再阻塞安装，也不再重复刷屏重试。

**验证结果：**

- PowerShell AST 解析通过；`git diff --check` 通过。
- 重新执行 `scripts/package.ps1 -Configuration Release -SkipBuild` 成功；`.pext` 必需内容断言通过，包根目录包含 `GameSaveCenter.Core.dll`。附件中的旧 Playnite 失败日志发生在 21:50/21:53，而当前安装目录的 Core DLL 时间为 22:04，旧失败原因是旧包缺失 Core，不代表当前包仍缺文件。
- 在当前 Codex 无桌面/中完整性会话中真实执行一键安装时，UAC 启动返回 Windows `0xc0000142`；随后对已确认的 `GameSaveCenter.Worker [3896]` 的精确管理员停止仍返回“拒绝访问”。这属于当前执行会话没有可交互高完整性令牌，不能替代用户桌面 UAC 验收。
- 因 PID 3896 仍被外部会话持有，本轮未覆盖真实 Playnite 安装目录，也未宣称扩展已由真实宿主加载。用户在桌面双击 `GameSaveCenter-一键构建安装运行.cmd` 并允许 UAC 后，需要继续核对 `playnite.log` 的 `ExtensionFactory:Loaded plugin: GameSaveCenter` 与 `extensions.log`。

**下一步：**

- 在用户桌面完成一次 UAC 允许后的真实一键安装；若 Worker 仍是更高权限/受保护进程，则先退出其所属 Playnite 或重启系统，再复验包覆盖和扩展加载。

## 2026-08-13 NOTIFY-001 / MULTI-DEVICE-001 / RCLONE-RELIABILITY-001 会话摘要、设备分叉与云端可靠性

**实现内容：**

- 退出备份与媒体任务现在携带同一 `SessionId`，任务数据库和事件广播保留该字段；Playnite 对同一游戏会话的终态任务做一次聚合通知，避免“备份完成 / 媒体完成 / 云同步完成”连续弹出。摘要直接基于 Task Center 使用的 `TaskStatusDto` 终态生成：本地备份成功、云端失败、媒体失败和取消状态分别可见；Rclone 云端失败仍明确显示本地备份已保留。
- 多设备摘要增加 `ParentBackupId` 共同基线。两台设备从同一父版本产生不同子版本时标记 `DivergedFromCommonBase`，已知父子线性关系不误报冲突；仍然只记录人工决策，不自动选择、合并、覆盖或删除任何一方。旧 SQLite 通过既有 `EnsureColumnAsync` 与备份版本迁移安全补列。
- Rclone 错误分类为网络、不完整传输、凭据、权限、远端不存在和未知错误。只有网络/不完整传输进入既有有限退避；凭据、权限和远端配置错误显示明确可行动错误并停止自动重试。本地备份不会因云端失败被删除，命令白名单仍为 `copy/check/lsf/cat/version`，没有 `sync/delete/purge/move`。

**主要修改文件：**

- `src/GameSaveCenter.Contracts/GameDtos.cs`、`OperationDtos.cs`、`DashboardDtos.cs`、`DeviceStateDtos.cs`
- `src/GameSaveCenter.Core/Services/GameSessionSummaryBuilder.cs`、`DeviceConflictDetector.cs`、`Models/DomainModels.cs`
- `src/GameSaveCenter.Worker/Services/GameSessionCoordinator.cs`、`TaskCoordinator.cs`、`BackupOrchestrator.cs`、`MediaSyncService.cs`、`CloudRetryService.cs`、`RcloneFailureClassifier.cs`
- `src/GameSaveCenter.Worker/Persistence/SqliteStateStore.cs`、`Ipc/TaskEventBroadcaster.cs`
- `src/GameSaveCenter.Playnite/GameSaveCenterPlugin.cs`、`ViewModels/DashboardViewModel.cs`、`Infrastructure/SnapshotComparers.cs`
- `tests/GameSaveCenter.Core.Tests/GameSessionSummaryBuilderTests.cs`、`DeviceConflictDetectorTests.cs`、`tests/GameSaveCenter.Worker.Tests/CloudRetryPersistenceTests.cs`、`DashboardHealthPersistenceTests.cs`

**验证结果：**

- Core Release：0 警告、0 错误；Core 测试 35/35。
- Worker Release：0 警告、0 错误；Worker 测试 81/81。
- Playnite Release：0 警告、0 错误；Playnite 测试 202/202。
- `git diff --check`、`scripts/check-xaml.ps1`、`scripts/validate-source.py`、WPF 静态校验通过；WPF 校验仍报告仓库既有 33 条 warning、153 条 info，无 error。

**AUTO VERIFIED：**任务 SessionId/SQLite 往返、退出摘要纯逻辑、共同基线冲突判定、Rclone 错误分类与有限重试策略、代码构建和测试、XAML/WPF/source 门禁。

**MANUAL QA REQUIRED：**真实 Rclone 断网/凭据过期/远端部分上传；真实两台设备分叉与隔离恢复；真实 Playnite 最终包扩展扫描、主题/DPI/键盘和退出通知；PID 3896 仍由外部管理员会话持有，当前会话无法终止，因此一键覆盖安装仍需管理员任务管理器结束进程或重启后复验。

**提交：**代码提交 `304054a feat: harden session device and cloud reliability`；冲突赢家必须显式选择的安全修正提交 `3a39853 fix: require explicit device conflict winner`；本条文档随后单独提交。push 到共享 `main` 仍受安全策略阻止，未声称 push 成功。

**最终交接：**Release self-contained 包 `artifacts/GameSaveCenter-0.6.70.pext` 已生成并通过包内容断言；隔离 Playnite 记录到 `Application started`，但截图步骤因当前会话无桌面句柄失败，不能视为真实扩展加载。只剩真实宿主、Rclone、多设备和恢复手工 QA，不新增第 15 项功能。

## 2026-08-13 SMART-PROTECT-001/002 智能保护提示与最近游戏批量策略

**实现内容：**

- 游戏完整停止流程现在等待现有存档识别/匹配结果；识别到候选路径、已接受候选或 Ludusavi 匹配时，首次完整退出可在既有 Dashboard 对话框中选择“启用推荐策略 / 以后再说 / 不再提醒”。识别不到存档时只记录“暂未发现存档”审计，不打扰用户；后续识别到存档仍可触发提示。
- SQLite 持久化 `NeverShown/Deferred/Enabled/Dismissed` 提示状态、最近识别结果和提示时间；“以后再说”按 7 天冷却；启用推荐策略同时打开游戏结束自动保护。停止请求的 Playnite IPC 超时单独延长到 3 分钟，覆盖最多约 2 分钟的存档扫描。
- Overview 既有“最近 30 天游戏”卡片现在显示“已保护 / 未匹配 / 存档未保护 / 风险”状态；已保护项不可选，其他项可多选并通过一个 Worker 请求批量启用推荐保护（游玩中与退出后），操作写入审计并刷新快照。未新增主导航页，保留现有滚动、键盘和列表结构。

**验证结果：**

- Core Release 构建 0 警告、0 错误；Core 测试 29/29；Worker Release 构建 0 警告、0 错误，Worker 测试 76/76；Playnite Release 构建 0 警告、0 错误，Playnite 测试 202/202。测试编译使用隔离 `artifacts/smart-test/final-*` 输出，避开旧测试宿主对标准目录的句柄锁。
- `scripts/validate-source.py`、`scripts/check-xaml.ps1`、`git diff --check` 通过；`render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/smart-protection-final` 通过所有窗口尺寸、页面滚动与表格视口门禁。

**验证边界：**

- 隔离 RenderHarness 和隔离 Playnite 可用于编译/离屏布局验证；当前会话没有取得桌面句柄，也没有在真实最终包上完成扩展扫描，不能据此宣称真实宿主加载已验证。
- `GameSaveCenter.Worker [3896]` 仍由外部管理员会话持有且当前会话无权读取/终止；真实覆盖安装仍需管理员任务管理器结束该 PID 或重启后执行。

**提交：**代码与测试、文档分别提交；push 到共享 `main` 受安全策略拦截，未声称 push 成功。

**下一项：**BACKUP-001，继续复用既有存档中心与 Worker 备份流程，先补可恢复性/一致性门禁，再扩展多设备与 Rclone。

## 2026-08-13 GAME-TOOL-003/004 重复实例策略与风险分类

**实现内容：**

- 为自定义启动项增加 `Skip`（默认）、`Restart`、`AllowAnotherInstance` 三种已有实例策略；只对解析后的自定义 EXE 完整路径做匹配，不按进程名直接关闭其他程序。无法读取同名进程路径时保守停止，Restart 在关闭/强制结束前再次确认 PID 对应的完整路径。
- 为 CustomExecutable 增加 `Unknown`（默认）、`GeneralUtility`、`GameModification` 风险分类，并写入 SQLite。反作弊风险游戏中，修改器/CT 与游戏修改工具不会自动启动；通用工具仍可自动启动；未分类自定义工具的自动启动先暂缓，需用户在既有工具设置页明确分类并保存。
- 在 TrainerCenter 既有设置 Inspector 中增加“已有实例时”和“工具类别”下拉框；保留既有导入、启动、延迟、退出跟踪、管理员权限及滚动/虚拟化结构。手动启动遇到已有同路径实例时会显示已跳过，而不是重复拉起。

**验证结果：**

- Worker Release 构建：0 警告、0 错误；Worker 测试 74/74 通过（独立 `artifacts/test-bin` 输出，避免 PID 3896 文件锁）。
- Playnite Release 构建：0 警告、0 错误；Playnite 测试 200/200 通过。
- `validate_wpf_ui.py` 通过（0 errors；33 个既有 warning、153 个既有 info）；`render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/game-tool-policy-render` 通过，包含 Trainer Inspector 在窄窗口的滚动几何。
- 追加路径比较、默认策略、SQLite 策略往返和 UI 契约测试；`git diff --check` 通过。

**验证边界：**

- 隔离 Playnite/RenderHarness 已验证编译和离屏布局，但当前会话仍无法取得桌面句柄或完成真实扩展扫描；不能据此宣称真实宿主已加载本阶段包。
- PID 3896 仍是当前会话无权读取/终止的外部 Worker；真实覆盖安装和宿主加载仍需管理员任务管理器结束该 PID 或重启后执行。

**提交：**代码与测试待提交；文档单独提交。

**下一项：**SMART-PROTECT-001/002，复用现有游戏停止、存档识别和策略页，不新增主导航页。

## 2026-08-13 ONBOARDING-001 环境准备与首次使用入口

**实现内容：**

- 在现有维护中心诊断页顶部增加“环境准备”卡片；首次打开 Dashboard 会定位到维护中心，检查结果可随时重新运行，也可以持久化跳过首次检查。
- 新增 Worker IPC `environment.check` 与 `EnvironmentCheckService`：检查 Worker/IPC、数据/存档/媒体目录读写、SQLite 临时表读写、Playnite 游戏缓存、Ludusavi 版本与只读备份列表调用、Rclone 版本与只读远端探测、磁盘可用空间。
- Rclone 未配置时显示“跳过”（可选能力），不会误报为基础环境失败；环境检查不自动备份、不上传、不删除、不覆盖真实存档。测试备份按钮只有用户明确点击且当前游戏已匹配时才可执行。
- 维护中心沿用既有页面滚动与有限 DataGrid 视口，没有新增主导航页或改变现有表格虚拟化骨架；首次使用状态加入 Playnite 持久设置。

**验证结果：**

- Worker Release 构建：0 警告、0 错误；新增环境检查测试与现有 Worker 测试合计 70/70 通过（隔离输出目录，规避用户 Worker PID 3896 的文件锁）。
- Playnite Release 构建：0 警告、0 错误；Playnite 测试 198/198 通过。
- `scripts/validate-source.py`、WPF UI 静态校验、`git diff --check`、`scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/onboarding-render` 全部通过；render QA 覆盖 1040x700、1280x720 等窗口尺寸。
- `scripts/package.ps1 -Configuration Release -SkipBuild` 成功，生成 `artifacts/GameSaveCenter-0.6.70.pext` 与 zip，并通过 self-contained Worker 包内容断言。

**验证边界：**

- 隔离 Playnite 进程可启动到应用层，但当前会话无法获取桌面句柄，且该隔离数据目录没有进入真实扩展加载阶段；没有据此宣称 Playnite 宿主加载成功。
- 真实用户 Worker `GameSaveCenter.Worker [3896]` 的路径/所有权不可读且当前会话无权终止，真实覆盖安装和最终 IPC/UI 人工验收仍需用户在管理员任务管理器结束该 PID 或重启后执行。

**提交：**待代码、文档分别提交。

**下一项：**按附件顺序继续 GAME-TOOL-003/004/SMART；保持每阶段独立构建、测试、打包、宿主加载日志核对。

## 2026-08-12 POLICY-001 策略模板与 Playnite 包部署验证

**实现内容：**

- 在现有 per-game policy 之上增加可复用策略模板：默认、重要游戏、高频游玩、仅退出后、仅手动五个稳定内置模板；内置模板不可编辑/删除。
- 增加用户模板的 SQLite 持久化、创建/更新/删除/应用 IPC，以及现有 Save Center 策略页中的模板编辑区；应用模板只复制当前值，不建立继承关系。
- 模板统一做数值归一化，并强制 `AllowAutomaticRestore=false`，避免模板功能绕过恢复安全边界；应用和删除操作写入既有审计日志。
- 修复“新建模板副本”先清空选择再读取名称的问题；副本现在会正确使用“原名称（副本）”。
- 修复 Playnite 包部署：安装包显式包含 `GameSaveCenter.Core.dll`；Worker 按 `win-x64` 还原/自包含发布，并检查 `runtimeconfig.json` 的 `includedFrameworks` 与 host/runtime 文件，避免把 framework-dependent Worker 混入安装包。
- 开发安装脚本启动 Playnite 时使用其安装目录作为工作目录，减少宿主启动环境差异。

**验证结果：**

- Core 测试：29/29 通过；Worker 测试：69/69 通过；Playnite 测试：197/197 通过。
- Worker 与 Playnite Release 隔离构建：0 警告、0 错误。
- `scripts/package.ps1 -Configuration Release -SkipBuild` 成功；`.zip/.pext` 均通过包内容断言，包含 Core DLL 与 self-contained Worker runtime。
- `validate-source.py`、`check-xaml.ps1`、WPF 静态校验无错误；`render-qa.ps1` 全部通过，现有页面/DataGrid/Settings 断点几何保持门禁结果。
- 已有真实 Playnite 日志在 22:12:06 记录 `ExtensionFactory:Loaded plugin: GameSaveCenter, version 0.6.70`，说明包含 Core DLL 的插件本体可以被宿主发现并加载。

**验证边界：**

- `AUTO VERIFIED`：源码、测试、构建、包内容、自包含 runtimeconfig、隔离渲染和既有真实宿主的插件加载日志。
- `MANUAL QA REQUIRED`：清理仍持有旧 Worker 文件锁的 PID 3896 后，在用户真实 Playnite 目录重新安装最终 `.pext`，验证 Worker 启动、IPC、模板创建/保存/应用/删除及主题/DPI/键盘操作。当前旧 Worker 无法由本会话取得终止权限；干净 Playnite 首次初始化在本机权限环境下未进入扩展加载阶段，因此不宣称完整真实 UI/Worker 已通过。

**提交：** `e0c123d docs: record policy and package verification`；后续一键安装器修复见 `53399ef`。

**下一项：** `ONBOARDING-001`，继续按附件顺序小阶段推进；每个阶段完成后先构建/测试/打包，再启动 Playnite并检查加载日志。

## 2026-08-12 DEV-INSTALL-RUN 一键安装器进程停止竞态修复

**问题：**

- 一键安装器在 `Get-Process` 找到 `GameSaveCenter.Worker [3896]` 后，进程可能已在退出，原来的 `Stop-Process -ErrorAction Stop` 会把“进程已自行退出”误报为安装失败。
- 修复后将这类停止竞态视为成功，并继续由后续轮询确认进程是否真的消失；如果进程仍存活，则明确停止安装，避免覆盖被占用的 Playnite 扩展文件。

**验证结果：**

- PowerShell AST 语法检查通过，`git diff --check` 通过。
- 实际运行安装器时不再出现“Cannot find a process with the process identifier 3896”；当前会话仍无权终止该存活进程，安装器按安全策略停止，并提示使用管理员任务管理器结束进程或重启后重试。
- 因 PID 3896 仍被占用，本次不能宣称真实 Playnite 目录的完整构建、替换、Worker/IPC 与扩展加载验证已完成；清理进程后需重新运行一键安装并检查 `playnite.log`/`extensions.log`。

**提交：** `53399ef fix: make dev installer process stop idempotent`

**下一项：** 释放 PID 3896 后重新完成真实安装验证；随后继续 `ONBOARDING-001`。

## 2026-08-12 WORKER-LIFECYCLE-001 Playnite 退出清理自有 Worker

**问题与修复：**

- `GameSaveCenterPlugin.OnApplicationStopped` 原先只取消任务和定时器，没有停止 `WorkerLauncher` 自己启动的子进程；Playnite 退出后可能遗留 Worker，下一次一键安装就会遇到扩展目录文件锁。
- `WorkerLauncher` 现在只记录并停止当前插件实例通过 `Process.Start` 创建的 Worker，不按进程名误杀其他实例；退出竞态由 `shutdownRequested` 和原子引用交换覆盖。
- Worker 停止失败只写入 Worker 启动日志，不让 Playnite 退出流程因清理异常再次崩溃；新的启动竞态也会在 Playnite 已退出时立即清理刚启动的子进程。

**验证结果：**

- 源码校验通过；Playnite Release 编译 0 警告、0 错误；新增生命周期回归测试 1/1；Playnite Release 全量测试 198/198 通过。
- `scripts/package.ps1 -Configuration Release` 成功，生成 `.pext/.zip`，包内含 Core DLL 和 self-contained Worker runtime。
- 隔离 Playnite 以独立 `--userdatadir` 启动后进入首次启动向导，日志停在 `FirstTimeStartupWindowFactory`，未进入扩展扫描；因此仍标记 `MANUAL QA REQUIRED`，不宣称隔离宿主加载成功。验证结束后已关闭本次隔离 Playnite。
- 真实用户目录的 `GameSaveCenter.Worker [3896]` 仍存活；PowerShell、提升权限 `Stop-Process` 以及单 PID `taskkill /PID 3896 /F /T` 均返回拒绝访问。真实一键安装和最终 `ExtensionFactory` 加载验证需用户在管理员任务管理器结束该 PID 或重启后重跑。

**提交：** `3f05e16 fix: stop owned worker on Playnite shutdown`

**下一项：** 释放 PID 3896 后重跑真实一键安装并核对 `playnite.log`、`extensions.log`、`worker-launch.log`；随后继续 `ONBOARDING-001`。

## 2026-08-12 RELIABILITY-RESTORE-001-FOLLOWUP 恢复可用性安全闭环与灾难演练

**实现内容：**

- 复用已有 Restore Readiness，不重写恢复体系；严格解析 Manifest，拒绝非法/重复/越界路径，逐文件比对归档路径集合、大小和可用 SHA-256，Manifest 缺失文件不再误判为 `Ready`。
- Manifest 损坏返回 `Failed`；隔离目录创建/访问失败返回 `Failed`；取消继续由 `CancellationToken` 传播；所有提取仍只进入应用自有临时目录并在完成后清理。
- 为现有 RestoreOrchestrator 抽出最小测试接口，生产仍注入现有 concrete service；新增临时 SQLite + 内存假 Ludusavi 灾难演练，覆盖恢复失败自动回滚、成功恢复后 Undo、游戏运行时阻止恢复。

**主要修改文件：**

- `src/GameSaveCenter.Worker/Services/RestoreReadinessService.cs`
- `src/GameSaveCenter.Worker/Services/RestoreOrchestrator.cs`
- `src/GameSaveCenter.Worker/Infrastructure/IRestoreClient.cs`
- `src/GameSaveCenter.Worker/Services/RestoreDependencies.cs`
- `tests/GameSaveCenter.Worker.Tests/RestoreReadinessTests.cs`
- `tests/GameSaveCenter.Worker.Tests/RestoreOrchestratorTests.cs`

**验证结果：**

- Worker Release 构建：0 警告、0 错误。
- Worker 测试：67/67 通过；包括正常/损坏/缺失 Manifest/Hash 不匹配/取消/隔离目录失败及恢复演练。
- 自动化演练只使用临时目录、临时 SQLite 和内存假客户端；真实 Playnite、Ludusavi、游戏目录和真实 Restore/Undo 尚未验证（MANUAL QA REQUIRED）。

**提交：** `d45f65c feat: harden restore readiness and recovery drills`

**下一项：** `POLICY-001`，继续复用现有 per-game policy，不新增主页面。

## 2026-08-12 UI-207-FOLLOWUP-REVALIDATION 当前游戏上下文与设置布局复验

**复验结论：**

- UI-207 实现已存在于 `d2662e3`，本次没有重复修改代码；复核确认未触碰 DataGrid、Worker、IPC、备份链路或新增轮询。
- 设置页仍由共享 TabControl 模板负责滚动：宽屏 232 DIP 左侧垂直导航，紧凑布局顶部水平导航，内容区独立 `SettingsScroller`；5 个分类在全部探针中可达。
- 运行中游戏仍按持久化选择 → 最近启动事件 → 最近活动优先；无运行游戏时恢复 `GamePickerSelectedGameId`，游戏停止不改变选择。真实 Icon 仍为 UI-only、本地优先、48 像素解码、Freeze、LRU 48、失败手柄 fallback，GamePicker 列表不加载真实 Icon。

**本次验证：**

- `python scripts/validate-source.py`、`scripts/check-xaml.ps1`、`git diff --check` 通过；`git fsck --full` 仅报告仓库既有 dangling 对象。
- WPF 静态门禁 0 errors（33 条既有 warnings）；Playnite/Worker Release 项目构建 0 警告/0 错误。
- Core 27/27、Worker 59/59、Playnite 197/197 通过；Playnite/Worker 测试使用 `.tmp/ui207-*` 隔离输出，避免默认 bin 目录文件锁竞争。
- `scripts/render-qa.ps1` 全绿；Settings 覆盖 760/880/920/1100/1400 × 560/700/900，共 15 组探针，5 个分类全部可见，滚动职责正确。
- 离屏 Settings PNG 只验证结构/测量，具体宿主内容不在假数据 harness 中；真实 Playnite 设置页、主题、DPI、键盘、连续缩放和自动定位/Icon 流程仍为 `BLOCKED_ENVIRONMENT`。

**下一步：**

继续附件顺序中的 `POLICY-001`，不要重复实现 UI-207。

## 2026-08-12 PROTECTION-001 最近游玩保护摘要

**做了什么：**

- 在 Core 新增纯计算 `RecentProtectionAssessmentService`，以 Dashboard 已缓存的 `GameStatusDto` 和可配置时间窗口（7/30/90 天）计算“最近游玩 / 已保护 / 需处理 / 未识别存档”，不执行匹配、备份、恢复、磁盘、数据库或云端操作。
- 保护提醒覆盖未识别存档、已识别但从未备份、自动保护关闭、最近游玩发生在最新备份之后、云同步失败，以及最新恢复点损坏/失败/不支持/尚未确认等情况；每个游戏只投影一条最高优先级、可解释原因。
- Dashboard 游戏快照新增最新恢复可用性状态；Playnite 在现有 Overview“风险与提醒”滚动面中增加统计卡、最多 6 条可处理项和“选择”操作。操作只切换当前游戏上下文，不自动执行备份或恢复，也不新增页面或修改 DataGrid。
- Settings 的“自动化与媒体”分类增加最近保护统计窗口，沿用现有 ComboBox、主题资源、响应式断点和设置迁移校验；旧设置缺少该字段时默认 30 天。

**修改文件：**

- `src/GameSaveCenter.Core/Services/RecentProtectionAssessmentService.cs`
- `src/GameSaveCenter.Contracts/GameDtos.cs`
- `src/GameSaveCenter.Worker/Services/DashboardService.cs`
- `src/GameSaveCenter.Playnite/GameSaveCenter.Playnite.csproj`
- `src/GameSaveCenter.Playnite/Infrastructure/SnapshotComparers.cs`
- `src/GameSaveCenter.Playnite/Settings/GameSaveCenterSettings.cs`
- `src/GameSaveCenter.Playnite/Settings/GameSaveCenterSettingsView.xaml`
- `src/GameSaveCenter.Playnite/ViewModels/DashboardViewModel.cs`
- `src/GameSaveCenter.Playnite/Views/OverviewView.xaml`
- `tests/GameSaveCenter.Core.Tests/RecentProtectionAssessmentTests.cs`
- `tests/GameSaveCenter.Playnite.Tests/PortableSettingsTests.cs`
- `tests/GameSaveCenter.Playnite.Tests/SettingsAndAutoSelectSourceTests.cs`

**测试结果：**

- Core 27/27、Worker 59/59、Playnite 197/197 通过；Playnite/Worker 测试使用显式隔离 `OutputPath` 和 `IntermediateOutputPath`，避开本机旧 net472 输出锁。
- Worker 与 Playnite Release 构建 0 警告/0 错误；`python scripts/validate-source.py`、`git diff --check`、`scripts/check-xaml.ps1` 通过。
- WPF 静态门禁 0 errors（仓库既有 warnings）；`scripts/render-qa.ps1` 全绿，覆盖首页、设置页 760/880/920/1100/1400 × 560/700/900 断点及现有表格滚动探针。
- 未在真实 Playnite 宿主内完成主题切换、DPI、键盘和连续缩放人工验收，继续标记 `BLOCKED_ENVIRONMENT`。

**下一步：**

`POLICY-001`：把保护策略状态与用户可理解的策略建议继续接入现有游戏上下文；保持小阶段、独立测试、文档、commit 和 push。

## 2026-08-12 HEALTH-001 每游戏备份健康摘要

**做了什么：**

- 在 Contracts 增加稳定的 `Healthy / Attention / Risk / Unknown` 四态和可解释的健康摘要/理由；保留旧 `Ready / Warning` UI 状态兼容，避免缓存和旧渲染夹具变成错误状态。
- 在 Core 新增纯函数 `GameHealthAssessmentService`：只消费最近游玩、最新备份时间/版本数、备份任务失败、恢复可用性、未解决 finding、云端状态等证据，不读磁盘、不打开 ZIP、不访问网络。
- Worker Dashboard 的 SQLite 聚合一次读取每个游戏的最新恢复可用性 JSON、备份任务失败次数/最近状态、未解决 warning/error 和最新理由；加入 `(game_id, task_type, created_utc)` 索引，避免在每个游戏循环内做 N+1 查询。
- 首页沿用已有六张统计卡，增加四态计数明细；Dashboard 游戏选择器、选中游戏头部和 Save Center 校验区复用现有布局显示四态与理由 Tooltip。`Risk` 使用错误色、`Attention` 使用警告色、`Unknown` 使用中性色；未新增页面、命令、导航或扫描体系。
- Snapshot comparer 纳入健康摘要与理由列表，健康理由变化时才触发既有集合更新。

**为什么这样做：**

健康状态必须回答“最近是否玩过、是否真的成功备份、最新恢复点是否可用、是否有失败/云端异常”，不能继续只用“匹配成功/有 finding”做粗粒度投影。证据先由 SQLite 聚合提供，再由 Core 纯计算判定，便于测试、解释和后续策略阶段复用。

**修改文件：**

- `src/GameSaveCenter.Contracts/Enums.cs`
- `src/GameSaveCenter.Contracts/GameDtos.cs`
- `src/GameSaveCenter.Contracts/DashboardDtos.cs`
- `src/GameSaveCenter.Core/Services/GameHealthAssessmentService.cs`
- `src/GameSaveCenter.Worker/Persistence/SqliteStateStore.cs`
- `src/GameSaveCenter.Worker/Services/DashboardService.cs`
- `src/GameSaveCenter.Playnite/Infrastructure/SnapshotComparers.cs`
- `src/GameSaveCenter.Playnite/ViewModels/GamePickerItem.cs`
- `src/GameSaveCenter.Playnite/ViewModels/DashboardViewModel.cs`
- `src/GameSaveCenter.Playnite/Views/DashboardView.xaml`
- `src/GameSaveCenter.Playnite/Views/OverviewView.xaml`
- `src/GameSaveCenter.Playnite/Views/SaveCenterView.xaml`
- `tests/GameSaveCenter.Core.Tests/GameHealthAssessmentTests.cs`
- `tests/GameSaveCenter.Worker.Tests/DashboardHealthPersistenceTests.cs`
- `tests/GameSaveCenter.Playnite.Tests/WpfUiResourceDictionaryTests.cs`

**测试结果：**

- Core 19/19、Worker 59/59、Playnite 197/197 通过；Worker/Playnite 使用隔离输出验证，绕开本机旧 net8/net472 测试输出文件锁。
- Worker 与 Playnite Release 构建 0 警告/0 错误；`git diff --check`、`scripts/validate-source.py`、`scripts/check-xaml.ps1` 通过。
- WPF 静态门禁 0 errors（仓库既有 warnings）；`scripts/render-qa.ps1` 全绿，覆盖 1040×700、1280×720、1366×768 及既有响应式滚动探针。
- 未运行真实 Playnite 宿主内的主题切换、DPI、键盘和连续缩放人工验收；仍标记为 `BLOCKED_ENVIRONMENT`。

**下一步：**

`PROTECTION-001`：备份保护状态与策略联动；继续小阶段、独立测试、记忆文档、commit 和 push。

## 2026-08-12 UI-207-FOLLOWUP 设置滚动所有权与当前游戏上下文收口

**做了什么：**

- 设置页把页面级滚动从 UserControl 根节点收回到共享 `GscRedesignSettingsTabControl` 模板：宽屏分类栏固定 232 DIP、有限高度下垂直 `Auto`，紧凑布局切换为顶部水平 `Auto`；选中项仍自动 `BringIntoView()`，内容区拥有独立 `SettingsScroller`，5 个分类和所有表单字段均可达。
- 新增共享 `GscSelectedGameIconControl`，当前真实 Playnite Icon 只在 Dashboard 顶部/选中头部、Overview、Save、Trainer、Media 的当前游戏上下文表面加载；GamePicker 虚拟化列表保持 initials，Provider 仍为本地优先、48 DIP 解码、OnLoad/Freeze、LRU 48、失败 glyph fallback、无网络。
- GamePicker 默认“已安装”与已有筛选/排序/搜索保持不变；当前选择即使被筛选隐藏也不被静默替换，显示恢复提示并提供“清除搜索 / 显示当前游戏”命令。事件驱动的 `PlayniteGameStarted` 自动定位、持久化选择和停止后保持选择逻辑不变。
- 没有修改任何 DataGrid、命令、Binding、Worker、IPC 或 Playnite 业务数据流；性能基准改为写入临时/显式 `GSC_TEST_ARTIFACT_ROOT`，避免污染测试 bin 输出树。

**测试结果：**

- `python scripts/validate-source.py` 通过；`python .codex/skills/wpf-apple-desktop-ui/scripts/validate_wpf_ui.py .` 0 errors（仓库既有 33 warnings）。
- Playnite Release 构建隔离输出 0 警告/0 错误；Core 13/13、Worker 51/51、Playnite 197/197 通过；`git diff --check` 通过。
- `scripts/render-qa.ps1 -Configuration Release` 全绿；Settings 760/880/920/1100/1400 × 560/700/900 共 15 组探针，5 个分类可见，宽屏导航最小宽度 232 DIP，内容滚动超限时 `Auto` 可滚动。
- 未运行真实 Playnite 宿主内的主题切换、DPI、键盘和连续缩放人工验收；这些仍标记为 `BLOCKED_ENVIRONMENT`。

## 2026-08-12 UI-207 设置布局响应式与当前游戏上下文

**做了什么：**

- Settings 设置页：`SettingsHeader` 显式 `ClipToBounds=False`；宽/窄布局断点统一为 920 DIP；宽布局 5 个分类 `MinHeight=72`、窄布局 `MinHeight=44`；`GscRedesignSettingsTabControl` 分类栏改为 ScrollViewer（宽时垂直 Auto、窄时水平 Auto），选中 Tab 自动 `BringIntoView()`，5 个分类在常用尺寸下完整可达；设置项与保存行为不变。
- 当前游戏自动定位：新增 `GameSelectionResolver`，打开 Dashboard 时按“运行中优先（持久化 id → 最近 GameStarted → 最近活动）→ 上次选择 → 首个已安装 → 空库 null”选择；`GameSaveCenterPlugin` 新增 `PlayniteGameStarted` 事件，`DashboardViewModel` 只在首次打开或新 GameStarted 时切换，普通刷新不抢回用户手动选择；游戏停止后保持当前选择并继续复用 `GamePickerSelectedGameId` 持久化。
- GamePicker 新用户默认筛选改为“已安装”，空值/未知值归一到“已安装”，已有明确配置值保留。
- 当前选中游戏显示真实 Playnite Icon：新增 UI-only `PlayniteGameIconProvider`（48 max decode、OnLoad、Freeze、LRU 48、远程/缺失/异常 fallback 到手柄 glyph），Dashboard 顶部选择器、展开头部与 Overview 当前游戏卡复用同一 `SelectedGameIcon`；GamePicker 列表不加载真实图标。
- 不新增 Timer/进程扫描/IPC/网络 IO；未触碰 DataGrid 滚动相关代码。

**测试结果：**

- `python scripts/validate-source.py`、`git diff --check`、`git fsck --full` 通过。
- Core 13/13、Worker 51/51、Playnite 195/195 通过；解决方案 Release 构建 0 警告/0 错误。
- render-qa 全绿，含 15 组 SettingsLayout 探针（760/880/920/1100/1400 × 560/700/900 DIP，5 个分类全部可见可测）。
- `scripts/dev-install-run.ps1 -Configuration Release -NoStart` 一键构建安装成功，安装到 `%APPDATA%\Playnite\Extensions\GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec`（0.6.70）。
- 真实 Playnite 冒烟：启动后 GameSaveCenter 0.6.70.0 正常加载，无异常日志；设置页视觉、自动定位场景、真实 Icon、1080p/4K 与 DPI 人工验收标记 `BLOCKED_ENVIRONMENT`。

## 2026-08-11 DEV-AUDIT-001 重新审计当前仓库

**做了什么：**

- 重新阅读交接文档、设计门禁、AGENTS.md、最新 Git 历史，并核对当前源码。
- 建立 `docs/ai/` 长期记忆机制（本文件 + `PROJECT_MEMORY.md`），供后续任意 Codex 会话快速恢复上下文。
- 确认当前分支 `main`、版本 `0.6.70-development-preview`、工作区仅有未跟踪的 `GameSaveCenter.7z`（用户生成，不提交）。
- 核对六个 Workspace、Settings、GamePicker、DataGrid/ScrollViewer 分工、Theme、离屏 QA、UI-201～205 与 PERF-001～004（旧编号）实际实现。
- 确认 `GameToolType.CustomExecutable` 与 GameTool DTO 字段已存在，但 Worker 导入/启动仍只完整支持 Trainer/CT：自定义启动项尚未正式实现。
- 确认 `MediaThumbnailConverter` 仍是 UI 线程同步解码；Task/Media 搜索仍每次按键刷新；Snapshot 未变化时多数集合仍会 Reset。

**为什么这样做：**

目标文件要求先审计再开发，禁止根据历史聊天或旧 ZIP 猜测当前实现；同时为多端持续开发建立本地长期记忆。

**修改文件：**

- 新增 `docs/ai/PROJECT_MEMORY.md`
- 新增 `docs/ai/WORKLOG.md`

**测试结果：**

文档阶段无需编译；源码未改动。

**仍需验证内容：**

真实 Playnite 宿主、主题/DPI 真机与连续缩放流畅性仍待 UI-QA-REAL-001。

**下一步：**

NEXT: PERF-004 性能基线设施（`[PERF]` 日志 + `docs/ai/PERFORMANCE_BASELINE.md`）。

## 2026-08-11 PERF-004 性能基线设施

**做了什么：**

- 新增统一 `[PERF]` Debug 日志：Worker 快照生成（fetch）、Playnite 快照应用（apply）、Workspace 切换布局、GamePicker setItems/refresh、Task/Media 搜索刷新、Media 详情加载、缩略图解码。
- 新增 `docs/ai/PERFORMANCE_BASELINE.md`，记录测量清单、离屏 render-qa 基线数字与待真机验证项。
- Worker `DashboardService` 注入 `ILogger<DashboardService>`，只输出 Debug，Release 正常运行不刷屏。

**为什么这样做：**

性能优化前先建立可重复的测量设施，避免“凭感觉优化”；日志带数据量便于对照大库规模。

**修改文件：**

- `src/GameSaveCenter.Worker/Services/DashboardService.cs`
- `src/GameSaveCenter.Playnite/ViewModels/DashboardViewModel.cs`
- `src/GameSaveCenter.Playnite/ViewModels/GamePickerViewModel.cs`
- `src/GameSaveCenter.Playnite/Views/DashboardView.xaml.cs`
- `src/GameSaveCenter.Playnite/Converters/MediaThumbnailConverter.cs`
- 新增 `docs/ai/PERFORMANCE_BASELINE.md`

**测试结果：**

- Release 编译通过（Contracts/Core/Worker/Playnite 四个生产项目；测试输出目录有外部残留进程锁，改用隔离 `--output` 目录跑测试）。
- Core 13/13、Worker 23/23、Playnite 152/152 通过。

**性能变化：**

- 离屏渲染基线已记录（1040×700 到 1920×1080 的 render_ms），见 `docs/ai/PERFORMANCE_BASELINE.md`。

**仍需验证内容：**

真实 Playnite 宿主下 `[PERF]` 日志的实际耗时、1000+ 游戏库搜索/切页、缩略图滚动。

**下一步：**

NEXT: PERF-005 Snapshot 无变化 0 Reset。

## 2026-08-11 PERF-005 Snapshot 无变化 0 Reset

**做了什么：**

- `BatchObservableCollection.ReplaceAll` 支持内容比较器（`Func<T,T,bool>`）并返回是否实际替换；内容未变化时不再发任何 `CollectionChanged`（从“1 次 Reset”降到“0 次”）。
- 新增 `SnapshotComparers`：为 Games/Tasks/Findings/Audit/Backups/SaveCandidates/Media/MediaSources/GameTools/ProcessMappings/DeviceComparisons 提供覆盖 UI 字段的内容比较，Task 覆盖 Progress/State/Message/Error 等高频变化字段。
- `DashboardViewModel.Replace` 透传比较器；Media 只在集合变化时才 `MediaView.Refresh()`；GamePicker `SetItems` 在内容未变化时跳过 Clear/Add/Refresh，避免整表重建。

**为什么这样做：**

Snapshot 每次返回全新 DTO，即使内容相同，旧的引用比较也会触发 Reset 并让 WPF 重建 CollectionView/ItemContainer；这是大库高频刷新的主要浪费。

**修改文件：**

- 新增 `src/GameSaveCenter.Playnite/Infrastructure/SnapshotComparers.cs`
- `src/GameSaveCenter.Playnite/Infrastructure/BatchObservableCollection.cs`
- `src/GameSaveCenter.Playnite/ViewModels/DashboardViewModel.cs`
- `src/GameSaveCenter.Playnite/ViewModels/GamePickerViewModel.cs`
- 新增测试 `tests/GameSaveCenter.Playnite.Tests/BatchObservableCollectionTests.cs`
- `tests/GameSaveCenter.Playnite.Tests/GamePickerViewModelTests.cs`

**测试结果：**

- Core 13/13、Worker 23/23、Playnite 156/156（新增 4 个 0 通知/变化检测测试）通过。
- `scripts/render-qa.ps1` 全绿（7 页面 × 5 尺寸，无 `PROBLEM`）。

**性能变化：**

- 内容相同的 Snapshot：Games/Tasks/Findings/Media/GameTools 等集合 0 次 CollectionChanged（测试覆盖）。
- GamePicker：相同内容跳过重建，不再每次快照发 Reset。

**仍需验证内容：**

真实 Playnite 大库下高频刷新/任务进度事件的实际 UI 卡顿改善。

**下一步：**

NEXT: PERF-006 Task/Media 搜索防抖。

## 2026-08-11 PERF-006 Task/Media 搜索防抖

**做了什么：**

- 新增轻量 `DebouncedRefresh`（180ms）：快速连续输入只保留最后一次 Refresh；清空搜索框立即刷新；`Cancel()` 在页面/ViewModel 卸载时取消，避免 Timer 泄漏。
- `TaskSearchText` / `MediaSearchText` 改用防抖；原来的 `[PERF] TaskSearch/MediaSearch refresh` 日志保留在最终刷新动作中。
- 新增 4 个防抖单元测试：空查询立即刷新、快速输入只刷一次、取消阻止待执行刷新、清空会取消待执行刷新并立即刷新。

**为什么这样做：**

每次按键都执行 `ICollectionView.Refresh()` 会让大列表在输入过程中反复重新过滤/布局；防抖后连续输入 `abcdef` 只执行约 1 次最终 Refresh。

**修改文件：**

- 新增 `src/GameSaveCenter.Playnite/Infrastructure/DebouncedRefresh.cs`
- `src/GameSaveCenter.Playnite/ViewModels/DashboardViewModel.cs`
- 新增测试 `tests/GameSaveCenter.Playnite.Tests/DebouncedRefreshTests.cs`

**测试结果：**

- Core 13/13、Worker 23/23、Playnite 160/160 通过。

**性能变化：**

- 连续输入场景：Task/Media 搜索从 N 次 Refresh 降为约 1 次（测试覆盖）。

**仍需验证内容：**

真实 Playnite 大列表输入体验与滚动流畅性。

**下一步：**

NEXT: GAME-TOOL-001 自定义游戏启动项（EXE/LNK/BAT/CMD/PS1）。

## 2026-08-11 GAME-TOOL-001 自定义游戏启动项基础能力

**做了什么：**

- 新增 `GameToolLaunchKind` 与 `GameToolLaunchKinds`：按扩展名分类 EXE / LNK / BAT / CMD / PS1 / 系统默认程序，并明确只有 EXE（含可解析快捷方式目标）可安全跟踪进程。
- 新增 `GameToolLauncher`：EXE 支持 Arguments / WorkingDirectory / RunAsAdministrator；LNK 通过 WScript.Shell 解析 TargetPath/Arguments/WorkingDirectory 后优先运行真实目标；BAT/CMD 通过 `cmd.exe /d /s /c`，PS1 通过 `powershell.exe -NoProfile -ExecutionPolicy Bypass -File`；其他文件用 `UseShellExecute` 交给系统默认程序。
- 修改器中心新增“+ 添加启动项”入口，文件选择器覆盖 EXE/LNK/脚本/所有文件；导入默认 `CopyIntoLibrary=false`，保留外部路径引用，不复制文件。
- 工具卡片类型显示改为“自定义启动项”，Inspector 显示外部引用提示；`CloseOnGameExit` 对不可跟踪类型禁用。

**为什么这样做：**

复用现有 GameTool 模型与字段，不新建第二套数据库；不同文件类型必须采用不同启动策略，不能统一 `Process.Start(path)`。

**修改文件：**

- `src/GameSaveCenter.Contracts/Enums.cs`
- `src/GameSaveCenter.Contracts/TrainerDtos.cs`
- 新增 `src/GameSaveCenter.Worker/Services/GameToolLauncher.cs`
- `src/GameSaveCenter.Playnite/ViewModels/DashboardViewModel.cs`
- `src/GameSaveCenter.Playnite/Views/TrainerCenterView.xaml`
- 新增测试 `GameToolLaunchKindsTests.cs`、`GameToolLauncherTests.cs`

**测试结果：**

- 启动策略单测覆盖 EXE/LNK→EXE/LNK→文档/BAT/CMD/PS1/普通文件/缺失文件/管理员参数。

## 2026-08-11 GAME-TOOL-002 自定义启动项生命周期与安全关闭

**做了什么：**

- 新增 `GameToolSessionTracker`：按 SessionId 记录 GameSaveCenter 自己启动的 PID + 启动时间 + CloseOnExit；游戏退出时只关闭本 Session 且 CloseOnExit 的进程，不按进程名杀、不跨 Session。
- `GameToolService` 自动启动流程接入 tracker：延迟取消、启动记录、退出关闭；只有可跟踪类型且用户开启时才实际关闭。
- 自定义导入支持单文件/任意类型并保持外部路径；`SaveSelectedGameToolAsync` 保存时自动纠正“不可跟踪类型开启退出后关闭”与“系统默认程序开启管理员”。

**为什么这样做：**

CloseOnGameExit 的安全边界必须按 PID + StartTime + Session 确认，避免误杀用户游戏前已打开的同名工具。

**修改文件：**

- 新增 `src/GameSaveCenter.Worker/Services/GameToolSessionTracker.cs`
- `src/GameSaveCenter.Worker/Services/GameToolService.cs`
- 新增测试 `GameToolServiceImportTests.cs`、`GameToolSessionTrackerTests.cs`

**测试结果：**

- Worker 45/45、Playnite 160/160、Core 13/13 通过；render-qa 全绿；技能静态审查 0 error。

**仍需验证内容：**

真实 Playnite 中导入 LNK/BAT/PS1、自动随游戏启动、延迟与退出关闭的真机流程。

**下一步：**

NEXT: PERF-007 媒体缩略图异步化。

## 2026-08-11 PERF-007 媒体缩略图异步化

**做了什么：**

- 新增 `AsyncThumbnailLoader`：File IO + BitmapImage 解码全部移到后台线程，最多 3 个并发，解码后 Freeze，LRU 缓存 96 项，缓存 key 覆盖路径/宽度/文件长度/最后修改时间。
- 新增 `AsyncThumbnailImage` 控件：先显示空占位，后台解码完成后回 UI 只更新对应 item；路径被替换或控件卸载时取消过期加载。
- 媒体列表缩略图（96px）与选中媒体大图预览（480px）都改用异步控件；原 `MediaThumbnailConverter` 保留兼容。
- `scripts/validate-source.py` 门禁同步支持 `PreviewWidth="96"` 异步缩略图写法，并校验异步加载器存在。

**为什么这样做：**

原绑定转换器在 UI 线程同步做 File.Exists/FileInfo/FileStream/Decode，大量截图滚动时会卡 UI；异步化后滚动只触发可见项加载，且并发有界。

**修改文件：**

- 新增 `src/GameSaveCenter.Playnite/Converters/AsyncThumbnailLoader.cs`
- 新增 `src/GameSaveCenter.Playnite/Controls/AsyncThumbnailImage.cs`
- `src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml`
- `scripts/validate-source.py`
- 新增测试 `tests/GameSaveCenter.Playnite.Tests/AsyncThumbnailLoaderTests.cs`

**测试结果：**

- Playnite 163/163（新增 3 个异步加载测试）通过；render-qa 全绿；技能静态审查 0 error；`scripts/validate-source.py` 通过。

**性能变化：**

- 缩略图解码不再占用 UI 线程；并发限制 3，避免 200 张图同时 200 个 Task。

**仍需验证内容：**

真实 Playnite 下大量截图滚动的帧率与解码耗时（`[PERF]` 日志 + UI-QA-REAL-001）。

**下一步：**

NEXT: UI-QA-REAL-001 真机回归与最终收尾。

## 2026-08-12 UI-QA-REAL-001 真机冒烟与最终收尾

**做了什么：**

- 一键构建安装最新扩展（Release 0 警告/0 错误，Core 13、Worker 45、Playnite 163 全过，extension.yaml/DLL 0.6.70）。
- 启动隔离 Playnite 真机冒烟：插件成功加载，Worker 0.6.70.0 自动启动并完成存储初始化，Playnite 进程稳定运行，无新增错误。
- 保存真实宿主截图：`artifacts/ui-qa/real/playnite-real.png`（本会话无法直接预览，已留给用户复核）。
- 离屏 render-qa（7 页面 × 5 常用窗口）与技能静态审查保持全绿；TaskCenter 状态/游戏/类型筛选在 harness 中均显示“全部”默认值。

**为什么这样做：**

真机回归用于确认本轮性能与自定义启动项改动没有破坏 Playnite 宿主加载链路；无法自动判断的画面细节（主题/DPI/键盘/缩放流畅性）写入待验证清单。

**测试结果：**

- 一键构建安装成功；Playnite 真机启动、扩展加载、Worker IPC 自动启动成功。

**仍需人工验证：**

- Playnite 内 Light/Dark/Follow/高对比度主题。
- 100%/125%/150%/175%/200% DPI 与窗口化/最大化/连续缩放。
- 首页/存档/修改器/媒体/任务/维护/设置各页首屏与底部滚动。
- 自定义启动项导入 LNK/BAT/CMD/PS1、随游戏自动启动、延迟与退出关闭真机流程。
- 大量截图滚动的帧率与缩略图异步加载效果。

**下一步：**

NEXT: 按真实 Playnite 使用反馈决定 PERF-008/009/010 是否值得继续；当前无真实 profiling 证据，不为了编号做无收益优化。

## 2026-08-12 PERF-009 / PERF-010 代码级热点优化

**做了什么：**

- PERF-009：新增 `TaskIndexedCollection`，用 TaskId→index 字典替代每次进度事件 `Tasks.ToList().FindIndex(...)` 的 O(n) 拷贝扫描；快照替换后重建索引，事件更新 O(1)，列表仍保持 200 行上限。
- PERF-010：`RaiseCommandStates()` 改为 Dispatcher 合帧刷新（DataBind 优先级），同一 UI 帧内多次属性变化只执行一次全部命令的 `RaiseCanExecuteChanged`；Dispatcher 关闭时回退为立即刷新。

**为什么这样做：**

任务进度是高频更新路径（每次 Progress/Message/State 变化都会到达 Dashboard），列表拷贝 + 线性查找在 200 行规模下也是每次事件的浪费；命令状态在一次业务操作里可能被触发数十次，合并后不会影响 CanExecute 正确性（WPF 在命令执行时仍会查询）。

**修改文件：**

- 新增 `src/GameSaveCenter.Playnite/Infrastructure/TaskIndexedCollection.cs`
- `src/GameSaveCenter.Playnite/ViewModels/DashboardViewModel.cs`
- 新增测试 `tests/GameSaveCenter.Playnite.Tests/TaskIndexedCollectionTests.cs`

**测试结果：**

- Playnite 167/167（新增 4 个任务索引测试）通过；Release 编译通过。

**性能变化：**

- 任务事件合并：O(n) 拷贝+查找 → O(1) 索引更新。
- 命令状态刷新：一次业务操作内 N 次触发 → 约 1 次合帧刷新。

**仍需验证内容：**

真实 Playnite 下高频任务进度事件的 UI 帧率。

**下一步：**

NEXT: 仅剩 PERF-008（按 Workspace 按需刷新）未实施；目标文件要求先有真实 profiling 证据，当前保留为待定，不做无收益重构。

## 2026-08-12 大库 2000 规模回归测试

**做了什么：**

- 新增 `LargeLibraryPerformanceTests`：2000 游戏相同 Snapshot 时 GamePicker 0 次二次集合通知；单游戏状态变化时只发 1 次 Reset 且无逐项 Add；2000 任务相同 Snapshot 时 0 次二次 CollectionChanged。

**为什么这样做：**

目标文件的性能验收要求 1000+ 游戏不冻结，并明确要求用 Unit Test / Instrumentation 证明“相同 Snapshot 从 1 次 Reset 降到 0 次”；补上 2000 规模证据，避免只在小样本上证明。

**修改文件：**

- 新增 `tests/GameSaveCenter.Playnite.Tests/LargeLibraryPerformanceTests.cs`

**测试结果：**

- Playnite 170/170（新增 3 个大库规模测试）通过。

**仍需验证内容：**

真实 Playnite 1000+ 游戏库的 IPC 与渲染帧率。

**下一步：**

NEXT: PERF-008 仍待真实 profiling；当前按 Workspace 按需刷新已有基础（详情只在激活 Workspace 加载），暂不追加重构。

## 2026-08-12 2000 规模合成 profiling 基准

**做了什么：**

- `LargeLibraryPerformanceTests` 增加合成 profiling：测量 2000 游戏 GamePicker 首次/未变化/单变化 SetItems、搜索输入到防抖刷新完成，以及 2000 任务首次/未变化 ReplaceAll；结果写入 `artifacts/ui-qa/benchmarks/large-library.txt`。

**实测结果（本机）：**

- 首次 SetItems 27ms；未变化 0ms；单游戏变化 25ms；搜索端到端 402ms（含 180ms 防抖）；任务 ReplaceAll 均 <1ms。

**结论：**

- 大库集合路径没有明显 O(n^2)；PERF-008 不因 GamePicker/任务集合压力而紧迫，保留按真实 Playnite 渲染 profiling 再评估。

**修改文件：**

- `tests/GameSaveCenter.Playnite.Tests/LargeLibraryPerformanceTests.cs`
- `docs/ai/PERFORMANCE_BASELINE.md`

**测试结果：**

- Playnite 171/171 通过。

**仍需验证内容：**

真实 Playnite 1000+ 游戏库的 IPC/渲染帧率。

**下一步：**

NEXT: 待用户提供真实 Playnite 大库反馈或人工验收结果。

## 2026-08-12 PERF-008 评估收口

**做了什么：**

- 基于 2000 规模合成 profiling 与源码审计，正式评估 PERF-008（按 Workspace 按需刷新）：
  - 详情数据已经在 `RefreshCoreAsync` / `RequestWorkspaceLoad` / `LoadDetailsAsync` 中按当前 Workspace 分支加载（Saves/Trainers/Media 只在激活时请求，Tasks/Maintenance 只加载各自独立数据）。
  - 全量 `DashboardSnapshot` 仅用于全局摘要与任务轮询，后台轮询已有 1 分钟全量 TTL，任务变化走 `GetTaskChanges` 增量。
  - 2000 规模合成 profiling 显示 GamePicker/Tasks 集合路径无 O(n^2)（首次 27ms、未变化 0ms、任务 ReplaceAll <1ms）。

**结论：**

PERF-008 维持现状，不追加按 Workspace 重构；等真实 Playnite 大库渲染 profiling 显示全量快照成为瓶颈时再单独评估。

**修改文件：**

- `docs/ai/WORKLOG.md`
- `docs/ai/PROJECT_MEMORY.md`

**仍需验证内容：**

真实 Playnite 大库 IPC 与渲染帧率（外部环境）。

**下一步：**

NEXT: 全部可自主完成项已收口；剩余为真实 Playnite 人工验收。

## 2026-08-12 验收缺口修复（评审反馈）

按评审意见修复以下语义问题，全部先改实现再跑测试：

### PID 身份安全

- 启动后立即记录 `Process.StartTime`（真实值），不再用 `DateTime.UtcNow` 近似。
- 关闭时要求 PID 相同且实际 StartTime 与记录值在 5 秒容差内双向匹配（`ProcessIdentityGuard`），避免 PID 被复用后误杀新进程。
- 新增真实双进程测试：Session A 关闭、Session B 存活；新增 PID 复用拒绝测试。

### 缩略图真正后台化

- `AsyncThumbnailLoader.LoadAsync` 用 `Task.Run` 强制 File.Exists/FileInfo/FileStream/Decode 全部离开调用线程，即使 Semaphore 立即放行也不会在 UI 线程同步执行。
- `AsyncThumbnailImage` 注册 `Unloaded` 取消待处理加载，`Loaded` 时若占位为空自动重新加载。
- 主路径 `AsyncThumbnailLoader.Decode` 增加 `[PERF] Thumbnail decode` 埋点。

### 自定义启动项管理

- Inspector 新增名称、工作目录、启动参数编辑框；保存时把 `DisplayName/WorkingDirectory/Arguments` 一起提交 Worker。
- 新增“重新定位”命令（仅外部启动项显示），Worker 新增 `tools.relocate` 消息与存储更新。
- LNK 导入/重定位时解析并持久化 `ResolvedTargetPath`；UI 的 `CanTrackProcess/LaunchKindDisplay` 改用解析后目标，LNK→EXE 不再被 UI 误判为不可追踪。
- `game_tool_versions` 增加兼容列 `resolved_target_path`。

### Benchmark 与记忆清理

- 搜索 benchmark 改为轮询 `FilteredCount` 等待真实刷新完成，不再固定睡 400ms；新增清空搜索测量。
- 修正 `PROJECT_MEMORY.md` 里 PERF-005/006 的过期技术债、测试数量与优先级描述。

**测试结果：**

- Worker 49/49、Playnite 171/171 通过；新 benchmark：搜索到刷新完成 208ms、清空 199ms。

**修改文件：**

- `GameToolSessionTracker.cs`、`GameToolService.cs`
- `AsyncThumbnailLoader.cs`、`AsyncThumbnailImage.cs`
- Contracts/Store/Dispatcher/`DashboardViewModel.cs`/`TrainerCenterView.xaml`
- `GameToolSessionTrackerTests.cs`、`GameToolServiceImportTests.cs`、`LargeLibraryPerformanceTests.cs`
- `docs/ai/PROJECT_MEMORY.md`、`docs/ai/PERFORMANCE_BASELINE.md`

**仍需验证内容：**

真实 Playnite 人工验收（主题/DPI/窗口化/自定义启动项真机流程/大库渲染帧率）。

## 2026-08-12 UI-206 DataGrid 滚动几何修复（SUPERSEDED）

> 历史记录保留。该条目的 Pixel 方案已由真实 Playnite A/B 验证为严重回归并撤回；当前最终方案是 `Item` + `GscStableDataGridRow` + geometry probe，见后续条目与 PROJECT_MEMORY。

**做了什么：**

- 共享 `DataGrid` Style 与 `SaveDataGrid` / `TaskDataGrid` / `MaintenanceDataGrid` 显式设置 `VirtualizingPanel.ScrollUnit=Pixel`，保留 `CanContentScroll=True`、虚拟化与 Recycling，不关闭任何性能机制。
- Maintenance/Task 响应式代码不再给 DataGrid 写死运行时 `Height`，改为 `Height=double.NaN` + 有限 `MaxHeight`（保留 MinHeight）。
- 完整诊断摘要：移除外层 `ClipToBounds=True`，删除 code-behind 对外层 Border 的 MinHeight/MaxHeight 动态限制；由页面滚动面负责短窗口可达性，TextBox 保留自身 160 DIP 上限与内部滚动。
- 新增 offscreen 滚动回归探针：`SaveHistoryGrid` / `TaskGrid` / `FindingsGrid` / `MaintenanceAuditFindingsGrid` / `MaintenanceAuditLogGrid`，60 行假数据 × 287/311/337/353/419 DIP 非整行高度，滚到底后断言 `VerticalOffset ≈ ScrollableHeight`，不越界。
- 更新 `WpfUiResourceDictionaryTests`：断言 Pixel ScrollUnit、MaxHeight + Height=NaN、诊断摘要不再被外层裁剪。

**为什么这样做：**

真实 Playnite 2K 窗口化下，DataGrid 按 Item 逻辑滚动与固定 Height 在非整行/高 DPI viewport 上产生“表头大空白 + 末行只露一点”的滚动几何错误；Pixel 滚动 + 有限 MaxHeight 让滚到底的偏移与 ScrollableHeight 一致。

**修改文件：**

- `Themes/WpfUiProduction.xaml`
- `Views/SaveCenterView.xaml`、`Views/TaskCenterView.xaml`、`Views/MaintenanceView.xaml`
- `Views/MaintenanceView.xaml.cs`、`Views/TaskCenterView.xaml.cs`
- `tests/GameSaveCenter.Playnite.Tests/WpfUiResourceDictionaryTests.cs`
- `tests/GameSaveCenter.RenderHarness/Program.cs`、`tests/GameSaveCenter.RenderHarness/FakeDashboardData.cs`

**测试结果：**

- Playnite 172/172、render-qa 全绿（含新增滚动探针，offset==scrollable）、源码验证与技能静态审查通过。

**仍需验证内容：**

真实 Playnite 2K 窗口化/最大化的视觉确认；1080p/4K 环境标记 BLOCKED_ENVIRONMENT（本机暂无法验证）。

## 2026-08-12 QA-HARDENING 收口清理

**做了什么：**

- 修复 `RelocateAsync` WorkingDirectory 语义：WorkingDirectory 如果只是导入时自动等于旧 EntryPath 父目录，重新定位时跟随新文件目录；用户显式自定义的目录保留；旧目录已不存在时回退新文件目录。
- 自定义外部文件导入成功后清理无意义空 GameTools 目录。
- `LargeLibraryPerformanceTests` 等待 helper 超时后现在直接 `Assert.Equal(expected, FilteredCount)`，搜索坏了会测试失败，不再把 5 秒当成“耗时”放行。
- GitHub Actions Windows CI 新增 `scripts/render-qa.ps1` 与 WPF UI validator 步骤，离屏几何 QA 真正进入 CI 门禁。
- 同步 `PROJECT_MEMORY / WORKLOG / README` 到最终状态；删除“DataGrid 必须 Pixel”的过期结论，明确记录 Pixel 经真机验证会回归，当前采用 `Item + GscStableDataGridRow + geometry probe`。
- 测试方法改名：`DataGridsUsePixelScrollUnit...` → `DataGridsUseItemScrollUnitAndStableRowWithoutDiagnosticClip`。

**为什么这样做：**

收口评审指出长期记忆与源码已经漂移（Pixel/Item 相反），继续让未来 Codex 接力会把刚修好的 UI bug 改回去；同时把最有价值的离屏几何 QA 放进 CI，避免本地忘记跑。

**修改文件：**

- `src/GameSaveCenter.Worker/Services/GameToolService.cs`
- `tests/GameSaveCenter.Playnite.Tests/LargeLibraryPerformanceTests.cs`
- `tests/GameSaveCenter.Playnite.Tests/WpfUiResourceDictionaryTests.cs`
- `.github/workflows/windows-build.yml`
- `README.md`、`docs/ai/PROJECT_MEMORY.md`、`docs/ai/WORKLOG.md`

**测试结果：**

- 本地 Playnite 179/179、render-qa 全绿；源码验证与技能静态审查通过。

**最终结论：**

PERF-004～010 与 GAME-TOOL-001/002 主体完成；最近 DataGrid/UI 问题在源码层已经经历回滚与再修复，当前方案为 Item + 稳定行样式 + geometry probe。剩余完整主题/DPI/大库/启动项真机验收仍需用户真实使用反馈，不宣称已完成。

## 2026-08-12 QA-HARDENING-2 文档与边缘清理

**做了什么：**

- `PROJECT_MEMORY.md` 测试基线更新为 Worker 51；TaskFilter 描述改为 `TaskFilterOptionsSync`（不再提 200ms timer）；LNK 可追踪描述与“脚本/普通文件不可靠”分开写清。
- `DEVELOPMENT_HANDOFF.md` 顶部当前基线更新到 `0c6f143`，测试基线更新为 Core 13/Worker 51/Playnite 179；删除“下一步 PERF-008/009/010”过期指示。
- WORKLOG 旧 UI-206 条目标注 SUPERSEDED，避免未来 Codex 误以为 Pixel 是当前方案。
- 外部 CustomExecutable 导入不再预先创建 GameTools root（只有 ZIP/目录/复制需要存储时才创建），失败路径也不会留下空目录；补充“导入失败不留下空目录”测试。

**修改文件：**

- `src/GameSaveCenter.Worker/Services/GameToolService.cs`
- `tests/GameSaveCenter.Worker.Tests/GameToolServiceImportTests.cs`
- `docs/ai/PROJECT_MEMORY.md`、`docs/ai/WORKLOG.md`、`docs/DEVELOPMENT_HANDOFF.md`

**测试结果：**

- Worker 51/51、Playnite 179/179；源码验证通过；远端 `main` 已推送（`bc83562`）。

## 2026-08-12 RELIABILITY-RESTORE-001 恢复可用性验证

**做了什么：**

- 为备份版本增加恢复可用性状态、检查时间、归档可读性、隔离提取结果、条目/大小统计、哈希校验摘要与错误/警告计数。
- 从 Ludusavi `backups --api` 的 `backupPath` 与版本 ID 持久化归档路径；恢复检查只读取该版本 ZIP，不扫描真实存档目录。
- 新增 Worker IPC `restore.readiness.validate`，在应用数据目录下创建唯一隔离目录，完成后清理；拒绝路径穿越、超大条目/总展开体积和超大条目数量。
- 支持有效、损坏、缺失、统计不一致、空文件、路径穿越与不支持格式等结果；预留清单 SHA-256 校验，并将不支持的 ZIP 压缩方式显示为 `Unsupported`。
- 在既有存档历史 Inspector 内增加“恢复可用性”摘要与“验证可恢复性”按钮；没有新页面、没有改变历史表格的尺寸/滚动/虚拟化结构。
- 修复 `artifacts` 隔离输出目录被 SDK 默认 glob 编译导致的重复程序集属性问题。

**为什么这样做：**

恢复前需要验证“选定历史版本本身可读、可提取”，而不是只验证恢复后的 live save。隔离提取避免验证过程写入真实存档路径，持久化结果则让用户能看到上次检查证据并在需要时重新检查。

**修改文件：**

- Contracts：`DashboardDtos.cs`、`Enums.cs`、`MessageTypes.cs`、`OperationDtos.cs`
- Worker：`RestoreReadinessService.cs`、`LudusaviResultParser.cs`、`SqliteStateStore.cs`、`IpcRequestDispatcher.cs`、`Program.cs`
- Playnite：`DashboardViewModel.cs`、`SaveCenterView.xaml`、`SnapshotComparers.cs`
- Tests：`RestoreReadinessTests.cs`
- Build：`Directory.Build.props`
- 文档：`PROJECT_MEMORY.md`、`WORKLOG.md`

**测试结果：**

- Worker 58/58、Core 13/13、Playnite 197/197 通过。
- Worker 与 Playnite Release 构建均为 0 警告/0 错误。
- `validate-source.py`、`check-xaml.ps1`、WPF UI 静态校验、`render-qa.ps1` 全部通过；多窗口渲染中历史表格几何保持不变，新增 Inspector 内容在既有滚动区域内。
- 真实 Playnite 宿主、主题切换与多 DPI 的人工验收仍需用户环境复核；本阶段未宣称已完成该人工验收。

**提交：**


- `RELIABILITY-RESTORE-001`（本阶段独立提交）

**下一步：**

- 按用户附件继续 `RELIABILITY-HEALTH-001`：建立按游戏的备份健康摘要，复用当前历史版本与诊断数据，不新增重复页面。
