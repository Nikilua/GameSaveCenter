# GameSaveCenter 当前事实入口

## 当前第三轮 R20-02 指标统计范围（已满足，待环境验证）

- 复用 `DashboardSnapshotDto` 的 `GeneratedUtc`、全库计数、云端/媒体计数和现有当前游戏 DTO；`OverviewSnapshotDisplay` 与 `DashboardViewModel` 投影为概览指标补充全库/当前游戏范围、更新时间和未加载表达。快照未成功返回时数字为 `—` 并明确“不代表 0”，已加载的合法零仍为 `0`。
- 概览统计条显示 Playnite 全库来源与生成时间，当前游戏卡片显示同一快照时间；未加载时隐藏依赖快照的状态胶囊/比例条，保留现有零分母规则、选框、滚动条、命令绑定和错误/取消/恢复语义。
- R20-02 及 R20-01 核心定向 `40/40`；概览相关布局/源码断言 `0 passed / 0 failed / 4 skipped`。更宽筛选 `194 passed / 3 failed / 50 skipped / 247 total`，3 条失败为未改动的设置响应式字段、空态覆盖层和受限下拉模板既有基线。
- Release 隔离 source-copy 编译 Playnite `net462` / Tests `net472` `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671` nullable warning；`validate-source.py`、XAML `24/24`、`git diff --check` 通过。链接工作树仍因 obj/WPF 临时项目 `Access denied` 未能直接写入，未绕过。
- 仅用合成 DTO、fake、隔离 testhost/目录；真实 Playnite/Worker/外部工具/存档/云端写入、呈现、物理 DPI/跨屏、UIA/IME、ETW、宿主性能未验。Demo 原目录不可用，main 用户改动未碰且未合并。证据见 `evidence/R20-02-METRIC-SCOPE-20260920.md`。
- 下一项：`R20-03` 首次配置引导，先查现有环境检查、设置入口、已完成标记和可返回/不自动改配置边界。

## 当前第三轮 R20-01 概览下一步（已满足，待环境验证）

- `OverviewPriorityResolver` 已按失败任务、空库、未匹配、可备份再到既有媒体/通用提醒/健康状态形成稳定优先级；失败任务进入真实任务中心“失败”筛选，未匹配和可备份进入现有游戏选框，后者使用“可备份”筛选，不直接触发全库写入。
- `OverviewPriorityResolverTests` + `GamePickerViewModelTests` `32/32`，Overview 真实命令交互 + 游戏选框 Shell 源码 `5/5`；覆盖未匹配优先于可备份、可备份负例、失败入口及无关快照变化不跳状态。Release 隔离 source-copy 编译 Playnite `net462` / Tests `net472` 通过，仅保留既有 `MediaCenterView.xaml.cs:671` nullable warning。
- `validate-source.py`、XAML `24/24`、`git diff --check` 通过。链接工作树直接构建受 `obj/...AssemblyInfoInputs.cache`/WPF 临时项目 `Access denied` 阻塞，未绕过；未运行真实 Playnite/Worker/外部工具、呈现、DPI/UIA/IME、ETW 或宿主性能。Demo 原目录不可用，未写真实存档/媒体/云端，main 用户改动未碰、未合并。
- 证据见 `evidence/R20-01-OVERVIEW-NEXT-ACTION-20260920.md`。下一项：`R20-02` 指标统计范围，先核对概览数字来源、当前游戏/全库范围与更新时间。

## 当前第三轮 R19-08 慢调用可取消（已满足，受控回归完成；真实管道与外部工具待验）

- `ExternalProcessRunner` 为 Ludusavi/Rclone 使用显式超时、取消 token、进程树终止和有界 stdout/stderr；Playnite IPC 取消区分调用者/宿主退出，写请求可能已接收时保留同一 RequestId 复核，不以新 ID 盲目重试；Busy finally 释放，云端只有命令明确成功才记 `Uploaded`。
- Worker 取消/超时/云队列/ledger 定向 `36/36`，Rclone 安全调用源码门 `1/1`；Playnite 可执行子集 `7 passed / 6 skipped / 0 failed / 13 total`。Named Pipe 6 条真实时序测试因当前系统权限跳过；另一组 2 条旧 net472 源码测试因产物缺 `GscBuildCommit` 身份门退出，未写成行为失败。
- 证据见 `evidence/R19-08-CANCELLABLE-SLOW-CALLS-20260920.md`。未运行真实 Playnite/Worker 管道、真实 Ludusavi/Rclone/远端写入、网络延迟、presented frame、DPI/UIA/IME、ETW 或宿主性能；Demo 原目录不可用。本阶段无生产代码变更。下一项：`R20-01` 概览下一步。

## 当前第三轮 R19-07 外部文件变化（已满足，受控回归完成；真实宿主待验）

- 媒体预览已有 `Missing/Failed/Loading/Ready` 占位、录像 MediaFailed fallback、generation/取消/卸载迟到保护；打开媒体失败只写状态/通知，打开目录在文件消失时回退到现存父目录，刷新命令按当前上下文重载。
- 备份详情通过 `ValidateRestoreReadinessCommand` 在 GameSaveCenter 隔离目录重新读 ZIP、Manifest、大小和哈希；缺失/损坏/权限/不一致有明确状态，Worker 对最近 `Corrupted/Failed` 阻止真实恢复，详情重新加载保留 BackupId/诊断。归档路径不静默改写，重新定位走显式检测/设置。
- Worker `RestoreReadinessTests 14/14`，Playnite 纯媒体/恢复说明 `7/7`；组合 Playnite `17 passed / 4 failed / 21 total` 的 4 条均是旧 net472 产物缺 `GscBuildCommit` 身份门。`validate-source.py`、XAML `24/24`、diff 通过；本阶段无生产代码变更。
- 证据见 `evidence/R19-07-EXTERNAL-FILE-CHANGE-20260920.md`。真实文件移动/占用/损坏的 Playnite/package-host 时序、Explorer/播放器呈现、presented frame、DPI/UIA/IME、ETW、宿主性能待验；Demo 原目录不可用。下一项：`R19-08` 慢调用可取消。

## 当前第三轮 R19-06 分页快照变化（已满足，受控回归完成；真实宿主待验）

- 已有 Worker 任务/媒体稳定游标：任务按 `(created_utc, task_id)`，媒体按 `(captured_utc, media_id)` 排序和严格游标过滤；多取一条决定 `HasMore`，末页不继续请求。Playnite 筛选/刷新重置游标与 generation，同一上下文翻页只使用当前 cursor。
- `MediaPageAccumulator` 按稳定 `MediaId` 去重/更新，窗口上限 `2,000`，可保留选中 ID；任务按 `TaskId` 合并，选择锚点按 ID 恢复。Worker 分页 `12/12`，Playnite 媒体累加器/任务索引 `10/10`，选择锚点 `4/4`。
- 合并 Playnite 筛选 `15 passed / 1 failed / 16 total` 中唯一失败是旧 R06 产物缺 `GscBuildCommit` 的身份门，不归入本项；`validate-source.py`、XAML `24/24`、`git diff --check` 通过。本阶段无生产代码变更，沿用 `6d1a401b` Release `0 errors/2 existing warnings`。
- 证据见 `evidence/R19-06-PAGED-SNAPSHOT-20260920.md`。只覆盖合成/fake/隔离 SQLite/testhost；真实并发变更、Playnite/package-host、presented frame、DPI/UIA/IME、ETW、宿主性能待验；Demo 原目录不可用。下一项：`R19-07` 外部文件变化。

## 当前第三轮 R19-05 Worker 重启恢复（已满足，受控回归完成；真实进程待验）

- 已有 Worker 启动 reconcile、整库备份 `ResumePendingAsync`、云传输持久队列和事件 pipe 单连接订阅；事件流是 best-effort，`GetTaskChanges`/SQLite 快照负责断线修复，重连不会无限累积订阅。
- Worker durable/event/cloud 回归 `18/18`；Playnite `TaskEventUiBatcher` `3/3`；XAML `24/24`、`validate-source.py`、`git diff --check` 通过。
- 独立 Worker 硬重启因当前环境禁止创建本地 Named Pipe 跳过；一条相邻 subscription 测试因旧 net472 产物缺 `GscBuildCommit` 被身份校验拦截，未写成真实重启或完整 Playnite 通过。证据见 `evidence/R19-05-WORKER-RESTART-RECOVERY-20260920.md`。
- 未验真实 Worker 进程/管道断线、Playnite 重开、presented frame、DPI/UIA/读屏/IME、ETW、宿主性能；Demo 原目录不可用。下一项：`R19-06` 分页快照变化。

## 当前第三轮 R19-04 取消关闭顺序（已满足，受控回归完成；真实宿主待验）

- 已有 Dashboard 卸载顺序会停止刷新/事件订阅、取消所有延迟请求和 generation，插件退出会先取消 lifetime、停止通知轮询，再停止本插件拥有的 Worker；Worker 启动会把旧 Queued/Running 任务按真实持久化状态标成可见失败，任务页重开读取 SQLite 快照。
- `LatestRequestCoordinator` + Busy 原子门相邻行为 `5/5`；`TaskReconcileService` `1/1`；Worker 取消/重启相邻组合 `13 passed / 1 skipped / 14 total`。取消、迟到回写、永久 busy 和协调重复执行边界均有实际夹具，不只依赖源码断言。
- 独立 Worker 硬重启因当前环境禁止本地 Named Pipe 跳过；一个 WPF shutdown 源测试因复用 net472 产物缺 `GscBuildCommit` 在身份校验处退出，需在可注入构建身份的干净构建中复跑。证据见 `evidence/R19-04-CLOSE-CANCEL-ORDER-20260920.md`。
- 未验真实 Playnite 关闭/重开、Worker 同时断管、presented frame、DPI/UIA/读屏/IME、ETW、宿主性能；Demo 原目录不可用。下一项：`R19-05` Worker 重启恢复。

## 当前第三轮 R19-03 重复执行幂等（已满足，受控回归完成；真实管道待验）

- 先查到现有 `IpcEnvelope.RequestId`、`IpcRequestSemantics` replay-protected 分类、Worker 持久化 request ledger、`WorkerIpcClient` 同 ID 复核和 `BusyOperationCoordinator` 已覆盖写请求单次执行、响应丢失复核、重复 UI 触发拒绝以及未知结果提示，没有重建写服务。
- Worker ledger/消息边界定向回归 `16/16`；生产 Busy 原子门与按钮行为 `2/2`；XAML `24/24`、`validate-source.py`、`git diff --check` 通过。
- 真实 Named Pipe 客户端组因当前环境禁止创建本地 Named Pipe 而跳过；完整 WPF linked 构建受 `_wpftmp.csproj` Access denied 阻塞，外部源码副本 restore 受 `NU1301` 网络策略阻塞，均未绕过或写成通过。证据见 `evidence/R19-03-IDEMPOTENCY-20260920.md`。
- 仅覆盖合成请求、fake/隔离 testhost 和本地已有产物；真实 Playnite/Worker 断连、presented frame、DPI/UIA/读屏/IME、ETW、宿主性能未验。Demo 原目录不可用。下一项：`R19-04` 取消关闭顺序。

## 当前第三轮 R19-02 刷新失败保留草稿（已满足，受控回归完成；真实宿主待验）

- 先查到生产 VM 已有 `backupCommentDirty`/`backupLockDirty`/`mediaCommentDirty`/`mediaFavoriteDirty`，稳定 ID 刷新分别走 `SyncBackupEditor(..., preserveDirtyFields)`、`SyncMediaEditor(..., preserveDirtyFields)`；失败状态路径不替换集合或编辑值，因此没有重建第二套草稿模型。
- 新增 `R19DraftRefreshBehaviorTests` 直接回放生产 VM 的存档/媒体失败、同 ID 成功和干净字段更新，`2/2` 通过；叠加 R11 WPF 备注取消和工作区状态回归为 `14 passed / 1 skipped / 15 total`。
- `7b2d3afa` clean-tree 隔离 Release `0 errors/2 existing MediaCenter nullable warnings`；`validate-source.py`、XAML `24/24`、`git diff --check` 通过。证据见 `evidence/R19-02-DRAFT-REFRESH-20260920.md`。
- 只覆盖合成 DTO、隔离 VM/WPF testhost；未把它写成真实 Playnite/Worker 断连与宿主时序、presented frame、DPI/UIA/读屏、ETW 或宿主性能证据。Demo 原目录不可用。下一项：`R19-03` 重复执行幂等。

## 当前第三轮 R19-01 旧请求晚返回（已满足，受控回归完成；真实宿主待验）

- `ed107c50` 收紧详情读请求代际：工作区切换立即调用 `CancelDetailsLoad`，`LoadDetailsAsync` 捕获启动时 workspace/generation/game ID，并在开始、成功、取消、失败的 UI 回写边界统一检查；媒体旧异常也不能写入新游戏/筛选上下文。
- 新增 A 慢成功/B 新失败负例，证明标题、数据、选择 ID、更新时间和失败信息仍属于 B；已有媒体状态缓存负例继续证明 A 的晚到成功不恢复 B 的旧成功时间。相关工作区状态、媒体分页锚点、页面重访回归最终 `30 passed / 1 skipped / 31 total`。
- clean-tree 隔离 Release solution `0 errors/2 existing MediaCenter nullable warnings`；`validate-source.py`、XAML `24/24`、`git diff --check` 通过。证据见 `evidence/R19-01-LATE-REQUEST-CONTEXT-20260920.md`。
- 只使用合成/fake/隔离 testhost，不触碰真实存档、媒体、用户云端或外部诊断；未把源契约/离屏测试写成真实 Playnite IPC 延迟、presented frame、物理 DPI/跨屏、UIA/读屏、ETW 或宿主性能证据。Demo 原目录不可用。下一项：`R19-02` 刷新失败保留草稿。

## 当前第三轮 R18-08 低性能降级触发（已满足，明确模拟完成；真实 Render Tier 待验）

- `3bfe3d3c` 先复用现有 `AdaptiveThemePaletteFactory` 的 null Effect/不透明表面/环境层关闭、`GscMotion.IsEnabled` 与 `NormalizeAll`；lowcostprobe 明确传入 `glassEnabled=false, motionEnabled=false`，没有引入新的主题体系或硬件检测假象。
- clean-tree Release RenderHarness 在六工作区、Light/Dark、1040×700/1600×900 共 24 组合通过：资源 Effects 全为 null、PopupTransparency=False、PopupAnimation=None、环境层 opacity=0，`visibleEffects=0`，可见文本 `4–147`，非输入框 `unexpectedOverflow=0`，最终 `lowcostprobe OK`。代表 Media/Trainer Light 图已检查。
- 首轮诊断发现 Media Inspector 的 `MediaClassificationPreviewItems` 在横向 Auto 下造成真实可见水平溢出；生产 XAML 同时为 preview/history ListBox 明确 `HorizontalScrollBarVisibility=Disabled`，保留垂直有限列表、Recycling 和路径 TextBox 的合法内容滚动。证据见 `evidence/R18-08-LOW-PERFORMANCE-FALLBACK-20260920.md`，原始输出在 `.tmp/r18-08-lowcost-final-clean/`。
- 直接相关回归 `37/37`、Release `0 errors/2 existing MediaCenter nullable warnings`、source/diff 通过。联合 R14 筛选 `40 passed/2 failed/42` 的两条是既有旧源码断言，不归入本项通过。
- 这只覆盖明确模拟的无玻璃/无动画回退，不是硬件 `RenderCapability.Tier` 或真实低 Tier GPU 证据；真实 Playnite/package-host、物理 DPI/跨屏、UIA/读屏、presented frame、ETW 和宿主性能仍未验。Demo 原目录不可用。下一项：`R19-01` 异步竞态与故障恢复入口。

## 当前第三轮 R18-07 长时资源曲线（已满足，受控验证完成；真实宿主待验）

- `56d8e1b7` 扩展既有 `RunEnduranceProbe`，复用原有合成六工作区/Light-Dark/Media 预览/详情动作，保留每次原始样本，并增加 managed/private/working set、threads/handles、探针可见 timers、反射可见托管事件委托、`HasAnimatedProperties` 代理和 `AsyncThumbnailLoader` 缓存诊断；无强制 GC。
- 受控 WPF STA Window 运行 `1800s` 操作 + `30s` 停止输入静置，实际 `1830.2s/1830`，`177` 样本、`2769` 周期、`8537` 动作、`0` 失败；静置四点周期保持 `2769`、`timers=0`，private `316,137,472→268,775,424`、working set `345,202,688→298,184,704` 后尾点稳定，最终 `enduranceprobe OK`。
- 相关源测试 `31/31`，Release 隔离构建 `0 errors/2 existing MediaCenter nullable warnings`，`validate-source.py`、diff 通过。证据见 `evidence/R18-07-LONG-RUN-RESOURCE-CURVE-20260920.md`，原始序列保留在 `.tmp/r18-07-endurance-final/enduranceprobe-report.txt`。
- `WorkingTreeClean=False` 是报告生成时静置 patch 尚未提交的事实；随后由 `c5c33e18` 固化，未伪造 clean-tree 运行。subscriptions/animated owners/thumbnail cache 是探针限定的代理或本场景未触发值，不能扩大解释为全局 WPF 订阅、精确动画时钟或缩略图解码证据。
- 只使用合成/fake/隔离目录，未写真实存档、媒体、云端或诊断；未绕过 ETW/系统跟踪权限。Demo 原目录不可用，真实 Playnite/package-host、物理 DPI/跨屏、UIA/读屏、presented frame、ETW 和宿主性能仍未验。下一项：`R18-08 低性能降级触发`。

## 当前第三轮 R18-06 页面重访成本（已满足，受控验证完成；真实宿主待验）

- `1b19fd4c` 保留生产壳层一次创建六个页面并复用 `PageHost.Content` 实例；新增 `[PERF] WorkspacePages`/`WorkspaceActivation` 的首次、重访、attach/reuse 和同步 layout 记录，以及 `[PERF] WorkspaceLoad` 的读取/跳过/成功/取消/失败/晚返回丢弃记录。
- `RequestWorkspaceLoad` 继续只走既有页面级读取，不触发整库 `RefreshDashboard`/`Synchronize`；15 秒热态门按 workspace、游戏 ID、媒体筛选/搜索/收件箱模式隔离。失败、取消、上下文变化、卸载失效和晚返回不产生新鲜缓存；显式 `LoadDetailsCommand` 保持强制读取。
- `WorkspaceRevisitLoadGate`/source `7/7`，页面生命周期/状态 presenter/请求协调/忙状态相邻回归 `31/31`；`validate-source.py`、XAML `24/24`、`git diff --check` 通过；Release 隔离 solution `0 errors/2 existing MediaCenter nullable warnings`；本阶段 `.tmp/r18-06-*` 已清理。原始证据见 `evidence/R18-06-WORKSPACE-REVISIT-20260920.md`。
- 只用合成时间/context、fake/source 测试和隔离 testhost；没有真实数据写入。未验真实 Playnite 首次打开/重访采样、最终 presented frame、DPI/UIA/读屏、ETW 或宿主性能；Demo 原目录不可用。下一项：`R18-07 长时资源曲线`。

## 当前第三轮 R18-05 后台事件合并（已满足，受控验证完成；真实宿主待验）

- `91947336` 复用现有 `TaskEventBroadcaster`、durable `TaskChangeFeed`、`TaskIndexedCollection`、`BatchObservableCollection` 和 Dashboard 卸载取消路径；Playnite 端按 TaskId 合并进度，最多保留 `128` 个待处理任务、每批最多 `32` 条，完成/失败/取消以 `DataBind` 优先级立即投递，卸载时清空并使已排队回调失效。
- Worker 每个订阅仍为固定 `128` 容量；满载手动淘汰非终态进度，完成/失败/取消优先保留。实际“先失败、再 200 条进度”仍保留失败终态，队列为 `128`；durable change feed/快照继续承担断线或极端终态压力下的恢复来源。
- `TaskEventUiBatcherTests 3/3`、`TaskEventBroadcasterTests 5/5`、任务进度/时间线/索引/通知相关 Playnite 回归 `19/19`；`validate-source.py`、XAML `24/24`、`git diff --check` 通过；Release 隔离 solution `0 errors/2 existing MediaCenter nullable warnings`，`.tmp/r18-05-solution` 和 TestResults 已清理。原始样本见 `evidence/R18-05-BACKGROUND-EVENT-BATCHING-20260920.md`。
- 未改命令绑定、取消/错误/恢复语义、游戏选框或滚动条系统；只用合成 DTO、fake 调度器、隔离 testhost/SQLite。Demo 原目录不可用；未验真实 Playnite 卸载/重载时序、最终呈现、DPI/UIA/读屏、ETW 或宿主性能。下一项：`R18-06 页面重访成本`。

## 当前第三轮 R18-04 表格容器预算（已满足，受控验证完成；真实宿主待验）

- `64843642` 复用生产 `TaskCenterView`、`MediaCenterView`、`MediaPageAccumulator` 和现有 DataGrid 模板；正常高度下给 Media Inbox 显式有限 `Height/MaxHeight`，外层页级纵向滚动只在短页或 stale fallback 开启，保留 Media `Standard/Item/EnableColumnVirtualization=False` 例外、游戏选框、滚动条、绑定和选择/锚点语义。
- 实际 STA WPF 2k/10k/20k 合成规模：Task 最大已实现容器均为 `9`、视口 `7` 行；Media Inbox 最大均为 `9`、视口 `9` 行，媒体 UI 窗口由生产分页上限保持 `2,000`。滚动 8 次 p95/最大：Task `52.846/52.846`、`28.365/28.365`、`34.807/34.807ms`；Media `0.132/0.132`、`0.289/0.289`、`0.153/0.153ms`。原始数组见 `evidence/R18-04-TABLE-CONTAINER-BUDGET-20260920.md`。
- R18-04 `1/1`；媒体分页、锚点、四行几何、细滚动和滚动归属相关回归 `23/23`；`validate-source.py`、XAML `24/24`、`git diff --check` 通过；Release 隔离 solution `0 errors/2 existing MediaCenter nullable warnings`，`.tmp/r18-04-solution` 已清理。
- 本阶段还修正 c17 引入的媒体详情 `Style.BasedOn` 动态资源解析错误，两个此前失败的媒体锚点 STA 测试恢复通过。首轮夹具曾捕获“先 Show 后布局会全量生成 2,000 行”的真实负例，最终在首次 measure 前应用生产响应式布局；真实 Playnite 首次 Loaded/宿主布局时序仍待验。Demo 原目录不可用；不宣称物理呈现、DPI/UIA/读屏、ETW 或宿主性能。下一项：`R18-05 后台事件合并`。

## 当前第三轮 R18-03 缩略图滚动预算（已满足，受控验证完成；真实宿主待验）

- `e54d514e`/`18c5073f` 复用现有 `AsyncThumbnailLoader` 的 3 路后台解码、96 项 LRU、取消和 `AsyncThumbnailImage` generation 保护；新增隔离合成滚动预算与迟到失败负例，没有重建加载器或修改生产 UI。
- 120 个合成 PNG 按 10 个 12 项窗口回放：请求/解码开始/成功 `120/120/120`，峰值活动解码 `3`，每轮结束活动 `0`，缓存序列 `12,24,36,48,60,72,84,96,96,96`，托管堆增量代理最大 `90,072 bytes`，预取消计数 `1`；旧缺失结果未把替换后的新图片行改回 `Missing/Failed`。原始样本见 `evidence/R18-03-THUMBNAIL-BUDGET-20260920.md`。
- R18-03 `1/1`；AsyncThumbnailLoader/Image 相关回归 `9/9`；`validate-source.py`、XAML `24/24`、`git diff --check` 通过；Release 隔离 solution `0 errors/2 existing MediaCenter nullable warnings`，WPF 静态沿用 `0/27/162`。
- 本阶段校正 c17 引入的 `PreviewDimensions` 旧断言：当前 `PreviewWidth=96` 的实际解码显示为 `96×96 px`，未修改生产加载器。托管堆值是 `GC.GetTotalMemory(false)` 代理，不是 ETW/显存/物理帧证据；Demo 原目录不可用，`.tmp/r18-03-solution` 已清理。
- 未验真实 Playnite/package-host 快速滚动、真实媒体分布、DPI/UIA/读屏、presented frame、ETW 或宿主性能。下一项：`R18-04 表格容器预算`，在 2k/10k/20k 数据下记录虚拟化容器上限与滚动更新成本。

## 当前第三轮 R18-02 真实 Dispatcher 基准（已满足，受控验证完成；真实宿主待验）

- `59468b37`/`5b28b0c3` 在真实 STA WPF `Window` 中区分本地 VM 刷新完成和可见列表容器反馈；VM 端用 `GamePickerPerformanceDiagnostics.RefreshCount + LastSearchText`，窗口端用实际 `ListBox.ItemContainerGenerator` 首容器的 `IsVisible`、`ActualWidth/ActualHeight` 和 `UpdateLayout`，没有用 `FilteredCount` 代替画面延迟。
- 20 次样本：SearchText→VM 完成 p95/最大 `52.272/63.581ms`；VM 完成→可见容器增量 p95/最大 `28.343/49.942ms`；每次可见计数 `1`，容器实测 `476×19.24 DIP`。原始数组见 `evidence/R18-02-DISPATCHER-VISIBILITY-20260920.md`。
- R18-02 `1/1`；同一实现逻辑的 R18/游戏选框/键盘/IME/防抖合并回归 `47/47`；`validate-source.py`、XAML `24/24`、`git diff --check` 通过；Release 隔离 solution `0 errors/2 existing MediaCenter nullable warnings`。WPF 静态基线沿用 `0/27/162`，无 XAML/主题变更。
- 受控窗口显式安装 `DispatcherSynchronizationContext`；首轮未安装时夹具真实捕获了 Dispatcher 投递不收敛的负例，修正后才记为通过。该测量是布局/可见容器证据，不是 presented frame、物理 DPI、60fps、UIA/读屏、真实 Playnite 或 ETW 性能证据。Demo 原目录不可用，`.tmp/r18-02-solution` 已清理。
- 下一项：`R18-03 缩略图滚动预算`，先查 `AsyncThumbnailLoader` 的活动请求、取消、缓存上限和迟到结果保护。

## 当前第三轮 R18-01 连续输入基准（已满足，受控验证完成；真实宿主待验）

- `10bc5789` 复用现有 `GamePickerViewModel` 本地缓存/同步过滤、取消入口和 20ms 防抖路径，仅增加内部诊断快照与 2,000/10,000 项测试夹具；未改游戏选框、滚动条、命令绑定、取消/错误/恢复保护、虚拟化或 Playnite `net462` 契约。
- 连续英文输入各 30 次：2,000 项 p95/最大值 `4.908/6.121ms`，10,000 项 `12.115/13.813ms`；每次分别评估 2,000/10,000 项。粘贴/删除/已提交中文 IME 查询及最终单次防抖刷新均在两档通过。原始每次延迟、过滤次数和托管堆增量代理值见 `evidence/R18-01-CONTINUOUS-INPUT-20260920.md`。
- R18 基准 `1/1`；游戏选框、键盘/IME、防抖相邻回归 `46/46`；`validate-source.py`、XAML `24/24`、`git diff --check` 通过；Release 隔离 solution `0 errors/2 existing MediaCenter nullable warnings`。无 XAML/主题变更，WPF 静态质量基线沿用 `0/27/162`。
- 托管堆值使用 net472 可用的 `GC.GetTotalMemory(false)` 前后非负差值，仅是可复算代理，不是 ETW/真实分配栈或物理呈现性能；未绕过系统跟踪权限。只用合成/fake/隔离 testhost，Demo 原目录不可用，main 用户改动、`src.zip` 和未跟踪对话框文件未碰、未合并；`.tmp/r18-01-solution` 已清理。
- 未验真实 Playnite/package-host、Windows IME 候选 UI、最终 presented frame、DPI/UIA/读屏、ETW 或宿主性能。下一项：`R18-02 真实 Dispatcher 基准`，区分 VM 数据完成与受控窗口可见反馈的两段时间戳。

## 当前第三轮 R17-08 维护报告可读性（已满足，受控验证完成；真实宿主待验）

- `59d4190b` 复用原 `MaintenanceReportService`、DTO、IPC 和维护页复制/导出命令；报告按软件身份、摘要、待处理、已验证、未知组织，摘要与分段使用同一生成时间和条目计数。
- Playnite 将插件/构建/Playnite 身份传给 Worker；报告末端统一脱敏 URL 查询/片段参数和 Windows 用户路径，保留安全主体，不改复制、导出、取消和错误语义。
- Worker R17-08 `2/2`，Playnite R17-08 `4/4`；合并相关回归 Worker `5/5`、Playnite `19/19`；隔离 Release solution `0 errors/2 条既有 MediaCenter nullable warning`；source、XAML `24/24`、diff、WPF `0/27/162` 通过。证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R17-08-MAINTENANCE-REPORT-20260920.md`。
- 未验真实 Playnite/package-host、实际文件夹/剪贴板、最终主题/DPI/UIA/IME/读屏/物理跨屏、presented frame、ETW 或宿主性能；只用合成/fake/隔离 SQLite/临时目录。Demo 原目录不可用，main 用户改动、`src.zip` 和未跟踪对话框文件未碰、未合并；`.tmp/r17-08-solution` 已清理。
- 下一项：`R18-01 连续输入基准`，先核对游戏选框搜索、`DebouncedRefresh`、IME 和大库合成夹具。

## 当前第三轮 R17-07 检查项一键定位（已满足，受控验证完成；真实宿主待验）

- `e8d581c6` 复用既有 Finding/Health/Task 导航与 `WorkspaceNavigationStack`；存档路径、任务、云队列继续走原稳定入口，健康巡检问题新增 `PlayniteId + BackupId` 精确版本路由。
- `ValidationFindingDto` 与 SQLite `findings` 增加 `BackupId` 兼容迁移；历史标题前缀仍可解析。加载只选择精确版本，目标不存在保留诊断并显示未选择其他版本；缺少版本身份不回落到失败任务。维护选择键纳入版本 ID，返回维护后沿用原筛选、选中项和滚动恢复。
- Worker 迁移/健康/Finding `18/18`，Playnite R17 `15/15`；隔离 Release solution `0 errors/2 条既有 MediaCenter nullable warning`；source、XAML `24/24`、diff、WPF `0/27/162` 通过。证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R17-07-FINDING-NAVIGATION-20260920.md`。
- 未验真实 Playnite/package-host、最终主题/DPI/UIA/IME/焦点滚动、Explorer/权限、presented frame、ETW 或宿主性能；只用合成/fake/隔离 SQLite/临时目录。Demo 原目录不可用，main 用户改动、`src.zip` 和未跟踪对话框文件未碰、未合并；`.tmp/r17-07-solution` 已清理。
- 下一项：`R17-08 维护报告可读性`，先核对报告 DTO/导出和脱敏路径。

## 当前第三轮 R17-06 存储分析导航（已满足，受控验证完成；真实宿主待验）

- `51cae6b9` 复用现有 `StorageAnalysisService`、逻辑索引/目录实测统计、TopGames 和稳定 ID 解析；维护页 Demo 卡片明确区分 SQLite 逻辑大小、备份目录文件实测和卷剩余空间。
- 失联或空归档路径单独统计逻辑体积，显示“未计入磁盘实测，不代表占用为 0”；备份目录不可用时显示路径状态未知，不把失联路径当作零占用。TopGames 补充最新 `BackupId`，“查看游戏/查看版本”只按稳定 `PlayniteId`/`BackupId` 精确定位，缺失时不回退到同名或其他版本。
- Worker 存储分析 `4/4`、Playnite R17-06 `4/4`、Playnite R17 `15/15`；隔离 Release solution `0 errors/2 条既有 MediaCenter nullable warning`；source、XAML `24/24`、diff、WPF `0/27/162` 通过。证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R17-06-STORAGE-ANALYSIS-NAVIGATION-20260920.md`。
- 未验真实 Playnite/package-host、最终主题/DPI/UIA/IME/焦点滚动、Explorer/实际权限、真实文件系统占用时序、presented frame、ETW 或宿主性能；只用合成/fake/隔离 SQLite/目录。Demo 原目录不可用，main 用户改动和 `src.zip` 未碰、未合并；`.tmp/r17-06-solution` 已清理。
- 下一项：`R17-07 检查项一键定位`，先核对已有 Finding/Health/Task 稳定来源、返回目标和跨工作区入口。

## 当前第三轮 R17-05 隔离账本入口（已满足，受控验证完成；真实宿主待验）

- `3002a8dc` 在维护行动项中补充隔离账本原路径、隔离路径和状态对应的受控恢复入口；路径行只对隔离账本显示，支持有限宽度换行和完整 Tooltip，不改变现有分页、滚动、命令绑定或 Worker 恢复算法。
- 复用既有 `RetentionQuarantineEntryDto`、分页 IPC、逐条 `EntryId` 恢复和确认语义；现有 Worker 隔离 SQLite 行为测试继续证明冲突/身份不一致时保留残留、不默认删除。
- Worker 隔离账本 `5/5`、Playnite 完整 R17 `12/12`（本项新增 `2/2`，维护报告 `3/3`）；隔离 Release solution `0 errors/2 条既有 MediaCenter nullable warning`；source、XAML `24/24`、diff、WPF `0/27/162` 通过。证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R17-05-QUARANTINE-LEDGER-20260920.md`。
- 未验真实 Playnite/package-host、最终主题/DPI/UIA/IME/焦点滚动、Explorer/权限、真实账本重启恢复时序、presented frame、ETW 或宿主性能；只用合成/fake/隔离 SQLite/临时目录。Demo 原目录不可用，main 用户改动和 `src.zip` 未碰、未合并。
- 下一项：`R17-06 存储分析导航`，先核对已有存储统计、来源记录和维护页跳转能力，再决定是否需要代码。

## 当前第三轮 R17-04 保留预览对比（已满足，受控验证完成；真实宿主待验）

- 复核确认既有 `RetentionSimulationService` 已展示候选、用户锁定/PreRestore/健康保护、预计释放和隔离占用；Apply 已验证预览句柄/十分钟时效、策略/候选/归档指纹并在执行前重读 live 状态。
- `3c73b498` 只补隔离 SQLite 删除失败负例：归档入隔离后索引删除失败会恢复原路径、保留恢复账本，`MovedBytes/FreedBytes` 均不计入真实释放；没有重建既有服务或修改真实数据。
- 最终 Worker 保留策略 `12/12`、Playnite R17 `10/10`、维护源码门禁 `3/3`；完整 Release solution `0 errors/2 条既有 MediaCenter nullable warning`；source、XAML `24/24`、diff、WPF `0/28/162` 通过。证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R17-04-RETENTION-PREVIEW-20260920.md`。
- 未验真实 Playnite/package-host、最终主题/DPI/UIA/IME/焦点滚动、Explorer/权限、真实锁/文件故障/重启恢复时序、presented frame、ETW 或宿主性能；布局回归另有 `20 passed/11 skipped`，未写成全绿。Demo 原目录不可用，main 用户改动和 `src.zip` 未碰、未合并。
- 下一项：`R17-05 隔离账本入口`，先核对已有分页隔离列表、原路径/隔离路径/状态和受控恢复入口。

## 当前第三轮 R17-03 检查进度预算（代码已提交，受控验证完成；真实宿主待验）

- `87473bc3` 复用既有健康巡检游标、单次时间预算、会话/操作锁、延后表和取消/失败终态；在现有 `LastSummary` 持久化通道补充本轮索引范围、需检查/延后/候选数量和未读取归档边界。
- 游戏运行、操作锁占用和全候选延后均显示具体暂停原因；取消、时间预算和异常结束写出准确结束状态，不把延后/取消伪装成整库已检查。维护页健康卡与行动项显示当前/最近候选、最近完成、最近成功和下轮计划/预算。
- 最终提交 Worker 健康巡检 `12/12`、Playnite R17 `10/10`；完整 Release solution `0 errors/2 条既有 MediaCenter nullable warning`；source、XAML `24/24`、diff、WPF `0/28/162` 通过。证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R17-03-INSPECTION-PROGRESS-BUDGET-20260920.md`。
- 未验真实 Playnite/package-host、最终主题/DPI/UIA/IME/焦点滚动、presented frame、ETW、宿主性能、真实游戏/锁/超时进程时序；只用合成/fake/隔离 SQLite 和目录。Demo 原目录不可用，main 用户改动和 `src.zip` 未碰、未合并。
- 下一项：`R17-04 保留预览对比`，先核对现有 RetentionSimulation/保护项/隔离账本和执行前预览过期条件。

## 当前第三轮 R17-02 诊断包预览（代码已提交，受控验证完成；真实宿主待验）

- `2b6e9051` 复用既有 `DiagnosticsPackageService.CreateAsync`、2 MiB/日志上限和 `DiagnosticRedactor`，新增只读预览 IPC，列出将包含的摘要类别、可选日志、脱敏范围、上限和明确排除项。
- 预览区分 `database.json` 的 schema/大小/完整性探针摘要与真实 SQLite 文件/表内容；明确不含存档、媒体、数据库文件/内容、Rclone 凭据或自动上传。Playnite 先用既有确认语义显示预览，取消不生成；确认后显示完整路径和大小并沿用原打开动作。
- 最终提交定向 Playnite R17 `7/7`、Worker `3/3`；完整 Release solution `0 errors/2 条既有 MediaCenter nullable warning`；source、XAML `24/24`、diff、WPF `0/28/162` 通过。证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R17-02-DIAGNOSTICS-PACKAGE-PREVIEW-20260920.md`。
- 未验真实 Playnite/package-host 确认框、最终主题、DPI/UIA/IME、Explorer/权限、真实日志并发、呈现/ETW/宿主性能；只用合成请求、fake/隔离 SQLite/临时目录。Demo 原目录不可用，main 用户改动和 `src.zip` 未碰、未合并。
- 下一项：`R17-03 检查进度预算`，先核对巡检范围、延后原因、最近成功和下轮计划。

## 当前第三轮 R17-01 健康结果分层（代码已提交，受控验证完成；真实宿主待验）

- `eb033251` 复用既有 `findings.resolved=0` 开放队列、健康巡检稳定 finding 和解决入口；Worker 将 `created_utc` 带入 `ValidationFindingDto`，维护详情保留本地化证据时间。
- Playnite 展示边界新增真实影响三档：需立即处理、建议处理、信息项；按游戏、稳定代码和问题标题合并跨来源重复，错误/严重优先，同游戏不同健康备份仍分别保留。原问题表、选中详情、命令绑定和滚动系统保留。
- 最终提交定向 Playnite `5/5`、Worker `2/2`；完整 Release solution `0 errors/2 条既有 MediaCenter nullable warning`；source、XAML `24/24`、diff、WPF `0/28/162` 通过。证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R17-01-HEALTH-RESULT-LAYERS-20260920.md`。
- 未验真实 Playnite/package-host、最终主题、DPI/UIA/IME、真实呈现/焦点/滚动、ETW 或宿主性能；未验证生产多来源标题规范。Demo 原目录不可用，main 用户改动和 `src.zip` 未碰、未合并。
- 下一项：`R17-02 诊断包预览`，先核对现有 DiagnosticsPackage 生成入口、类别清单和脱敏范围。

## 当前第三轮 R16-08 保存冲突处理（代码已提交，受控验证完成；真实宿主待验）

- `ee6b37c9` 在既有 Playnite 编辑基线/fingerprint 上增加 `SettingsConflictResolver` 三方合并：仅后台变化的字段并入草稿；用户和最新持久化同时改动且值不同的字段进入冲突列表，不部分覆盖。
- `EndEdit` 保存前读取最新 settings；冲突触发 `SettingsConflictDetected`、设置页字段级提示和 `SettingsConflictException`，当前草稿不写入，取消基线移到最新持久化值；无冲突继续原保存/视觉通知/Worker 应用链。
- 最终 HEAD 定向设置/PortableSettings 回归 `17/17`；隔离 Release solution `0 errors/2 existing MediaCenter nullable warnings`；source、XAML `24/24`、diff、WPF `0/28/162` 通过。证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R16-08-SETTINGS-CONFLICT-20260920.md`。
- 未验真实 Playnite 双设置窗口、后台保存竞态、宿主错误呈现、最终主题、DPI/UIA/IME、RenderHarness、ETW 或宿主性能；共享 settings 对象的真实线程时序仍待验。Demo 原目录不可用，main 用户改动和 `src.zip` 未碰、未合并。
- 下一项：`R17-01 健康结果分层`，先查健康检查结果、已解决项和跨来源去重时间证据。

## 当前第三轮 R16-07 配置导入预览（代码已提交，受控验证完成；真实宿主待验）

- `451195ad` 复用既有 `ExportPortableJson`、`ImportPortableJson` 和缺失路径报告，新增 detached `PreviewPortableJson` 与确认后的 `ApplyPortableJson`；预览显示架构版本、兼容性、变化字段和未知字段，未知字段忽略且不破坏当前配置。
- 导出继续清空设备身份，当前 DTO 无凭据字段；预览明确凭据不进入可分享导出。UI 使用原生 Yes/No 预览确认，取消、旧架构、坏值不修改草稿；应用前快照保证复制/报告异常可恢复原配置。
- 最终 HEAD 定向 R16-07/PortableSettings `15/15`；隔离 Release solution `0 errors/2 existing MediaCenter nullable warnings`；source、XAML `24/24`、diff、WPF `0/28/162` 通过。证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R16-07-SETTINGS-IMPORT-PREVIEW-20260920.md`。
- 未验真实 Playnite/package-host 文件选择器、MessageBox、保存取消和最终呈现、DPI/UIA/IME、RenderHarness、ETW 或宿主性能；Demo 原目录不可用，main 用户改动和 `src.zip` 未碰、未合并。只用合成 JSON/detached settings/隔离目录。
- 下一项：`R16-07 配置导入预览`，先核对现有导入报告、版本、未知字段、凭据和失败回退。

## 当前第三轮 R16-06 生效条件说明（代码已提交，受控验证完成；真实宿主待验）

- `83e7c745` 追踪并标注现有 `EndEdit → NotifyVisualSettingsChanged → ApplySettingsAsync → settings.update → WorkerOptions.Apply/SyncPlan` 链路：外观保存后即时重建，工具/目录/备份从下一任务读取，轮询/队列/健康计划按下一轮边界读取，启动类开关只影响下一次 Playnite 启动。
- 设置页四个分类标题旁新增生效条件说明，明确当前页预览与保存后的范围；没有把普通设置笼统写成需要重启 Playnite，继续保留云端时段和安全模式的已有边界说明。
- 当前提交重新编译后 R16-06 链路/负例 + R16-05 路径回归 `8/8`；外部隔离 Release solution `0 errors/2 warnings`（既有 MediaCenter nullable）；source、XAML `24/24`、diff、WPF `0/28/162` 通过。
- 未验真实 Playnite/package-host 保存后时序、Worker 重启/轮询呈现、最终主题、DPI/UIA/IME、RenderHarness、ETW 或宿主性能；Demo 原目录不可用，main 用户改动和 `src.zip` 未碰、未合并。证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R16-06-SETTINGS-EFFECT-CONDITIONS-20260920.md`。
- 下一项：`R16-07 配置导入预览`，先核对现有导入报告、版本、未知字段、凭据和失败回退。

## 当前第三轮 R16-05 路径编辑一致（代码已提交，受控验证完成；真实宿主待验）

- `955dc52e` 在核对既有全量异步路径校验、粘贴标准化和导入/导出后，新增设置页统一“路径编辑”卡片，复用六个本地工具/目录 TextBox Binding；浏览按文件/目录类型选择，Rclone 云端目标不进入本地打开流程。
- `SettingsPathEditorService` 用只读 `File.GetAttributes` 与目录枚举区分有效、缺失、网络/磁盘不可达、类型错误和无权限；打开只处理当前字段已存在且可读的路径，不回退父目录；复制复用脱敏与 `ClipboardRetry`，浏览取消不改草稿。
- 外部隔离 Release solution `0 errors/2 warnings`（既有 `MediaCenterView.xaml.cs:664` nullable）；R16-05 定向行为/源码/路径回归 `6/6`；`validate-source.py`、XAML `24/24`、diff、WPF `0/28/162` 通过。源码测试显式绑定当前 commit，未把真实 UI 行为写成通过。
- 未验真实 Playnite/package-host、文件夹对话框归属、Explorer 动作、最终浅深主题、DPI/UIA/IME、RenderHarness、ETW 或宿主性能；未使用真实网络共享/用户 ACL/剪贴板。Demo 原目录不可用，main 用户改动和 `src.zip` 未碰、未合并；证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R16-05-PATH-EDITOR-20260920.md`。
- 下一项：`R16-06 生效条件说明`，先核对设置字段实际消费点、保存/应用/重启边界。

## 当前第三轮 R16-04 恢复默认粒度（代码已提交，受控验证完成；真实宿主待验）

- `2b194461` 新增设置恢复默认目录，提供单字段、单分类、全部默认三个范围；确认文案列出影响。全部默认只重置安全标量和本地 UI 偏好，明确保留 Worker/Ludusavi/Rclone、存档/媒体/镜像路径、云端目标和设备身份。
- 重置直接修改现有 Playnite 草稿并重绑同一 settings 对象刷新界面，不结束编辑、不保存、不启动 Worker；Playnite 取消仍恢复重置前草稿。测试实际覆盖连接字段保留、默认值和取消恢复。
- 外部隔离 Release solution `0 errors/2 warnings`（既有 `MediaCenterView.xaml.cs:664` nullable）；R16-04 行为 `2/2`、源码接线 `1/1`；`validate-source.py`、XAML `24/24`、diff、WPF `0/28/177` 通过。使用隔离副本现有 `obj` 和 `--no-restore`，未宣称 fresh restore 通过。
- 未验真实 Playnite/package-host、最终浅深主题、DPI/UIA/IME、RenderHarness、ETW 或宿主性能。Demo 原目录不可用，main 用户改动和 `src.zip` 未碰、未合并；证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R16-04-RESET-GRANULARITY-20260920.md`。
- 下一项：`R16-05 路径编辑一致`，先核对路径浏览、校验、打开、复制和权限/网络/不存在负例。

## 当前第三轮 R16-03 模板应用范围（代码已提交，受控验证完成；真实宿主待验）

- `52fbf5de` 复用现有模板/策略 DTO、归一化、单游戏操作锁、持久化和审计路径，新增有界批量模板应用：客户端只发送明确勾选的稳定 Playnite ID，最多 100 个；空选择、超限和筛选隐藏项不会误用全部游戏。
- Save 页面显示目标、排除、预计变更字段数，筛选保留稳定 ID 选择；Worker 逐项处理并返回逐项成功/失败结果，非取消异常继续后续目标，失败项可单独重试，取消仍传播。模板继续是一次性复制。
- 外部隔离 Release solution `0 errors/2 warnings`（均为既有 `MediaCenterView.xaml.cs:664` nullable）；Core `3/3`、Playnite 源契约 `1/1`、Worker `2/2`；`validate-source.py`、XAML `24/24`、diff、WPF `0/28/177` 通过。fresh restore 无诊断退出，构建复核使用隔离副本现有 `obj` 和 `--no-restore`，未宣称全新还原通过。
- 保留游戏选框、滚动条、命令/Binding、取消/错误/恢复保护、有限列表性能和 net462；未验真实 Playnite/package-host、最终浅深主题、DPI/UIA/IME、RenderHarness、ETW 或宿主性能。Demo 原目录不可用，main 用户改动和 `src.zip` 未碰、未合并；证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R16-03-POLICY-TEMPLATE-BATCH-20260920.md`。
- 下一项：`R16-04 恢复默认粒度`，先核对设置恢复入口、敏感连接字段保护和取消草稿行为。

## 当前第三轮 R16-02 策略差异预览（代码已提交，受控验证完成；真实宿主待验）

- `b327d5ef` 复用现有策略/模板 DTO、模板目录归一化、Worker IPC 和保存基线，新增 13 字段有界差异服务与 `BackupPolicyDto` 字段通知；Save 页面分开展示当前已保存基线、显式游戏草稿和模板将覆盖值，模板明确为一次性复制而非实时继承。
- “取消未保存修改”只把保存基线复制回当前游戏草稿，不调用 `RequestAsync`；保存仍走原 `SavePolicyAsync`。存在未保存游戏策略草稿时，应用模板命令保持禁用，避免覆盖本地草稿。
- 外部隔离 Release solution `0 errors/7 warnings`（均为离线 NuGet `NU1900`）；策略差异/复制/通知核心 `5/5`，Playnite 源契约 `1/1`；source、XAML `24/24`、diff 和 WPF `0/28/177` 通过。未把源契约测试写成宿主交互或视觉通过。
- 保留游戏选框、滚动条、命令/Binding、取消/错误/恢复保护、有限列表和 net462；未验真实 Playnite/package-host、最终浅深主题呈现、DPI/UIA/IME、ETW、宿主性能或 RenderHarness presented frame。Demo 原目录不可用，linked `obj` 仍 `Access denied`，只用合成/fake/隔离目录；main 用户改动和 `src.zip` 未碰、未合并。证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R16-02-POLICY-DIFF-20260920.md`。
- 下一项：`R16-03 模板应用范围`，先核对现有模板 DTO、应用命令和一次性复制边界，再补目标范围/取消与负例证据。

## 当前第三轮 R16-01 设置搜索定位（代码已提交，受控验证完成；真实宿主待验）

- `a4e35578` 在现有设置分类上增加轻量搜索框、匹配字段索引和结果摘要；不复制 DTO/服务，不改原控件 Binding。搜索输入只改变匹配字段与分类的可见性，首次搜索记录原分类，清空恢复；验证错误定位先清空搜索后复用已有分类/滚动/焦点路径。
- 外部隔离 Release solution 单节点构建 `0 errors/10 warnings`（离线 `NU1900` 与既有 `MediaCenterView.xaml.cs:664` nullable warning）；R16 搜索行为 `1/1`、源契约 `1/1`，验证导航/草稿分别独立 `1/1`；source、XAML `24/24`、diff 通过；WPF `0/28/177`。
- 受控行为覆盖匹配字段可见且可编辑、未命中字段隐藏、清空回原分类、配置路径不变和无 pending edit。联合 WPF 筛选受既有 AppDomain Application 多实例夹具冲突影响，不能作为门禁通过。
- 未验真实 Playnite/package-host、最终浅深主题呈现、RenderHarness presented frame、DPI/跨屏、UIA/IME、ETW 或宿主性能；Demo 原目录不可用，linked `obj` 仍 `Access denied`，只用合成/fake/隔离目录；main 用户改动未碰、未合并。证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R16-01-SETTINGS-SEARCH-20260920.md`。
- 下一项：`R16-02 策略差异预览`，先核对策略/模板 DTO 的继承值和显式覆盖值，再限定只读差异与取消无写入。

## 当前第三轮 R15-08 清理历史范围（既有实现已满足，隔离证据完成；真实宿主待验）

- 既有 `88bde5de`、`a841e42c`、`77d5f346` 已提供全局 Retention Simulation、日期/原因/影响预览、共享游戏锁和持久化隔离账本；`a07f0518` 补齐当前提交下的行为/维护页证据并修正一条 R15-07 后陈旧源码断言。
- 预览保留 `PreviewId`/生成时间、现有/保留/候选数量、预计释放、锁定/健康恢复点/PreRestore 影响和隔离账本占用；候选行绑定日期、原因、路径，列表最多 200 条且有限高 240。应用要求明确二次确认和 Worker 预览句柄，过期/状态/文件身份变化拒绝；运行中的备份、恢复、媒体操作按共享 `GameOperationKind.Retention` 锁跳过，保护版本与恢复账本不被清理。
- 当前外部隔离副本验证：`RetentionSimulationServiceTests` + `RetentionQuarantineRecoveryTests` `16/16`；Playnite `net462` `MaintenanceReportSourceTests` `3/3`；`validate-source.py`、XAML `24/24`、diff check 通过。生产 XAML 未改，WPF 静态沿用上一批 `0/28/162`。
- 未验真实 Playnite/package-host、RenderHarness presented frame、DPI/跨屏、UIA/IME、ETW/宿主性能；Demo 原目录不可用，linked `obj` 的 `Access denied` 仍存在。只用合成/fake/隔离目录，未写真实存档、媒体、云端、诊断或系统剪贴板；main 用户改动和 `src.zip` 未碰、未合并。证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R15-08-HISTORY-CLEANUP-SCOPE-20260920.md`。
- 下一项：`R16-01 设置搜索定位`，先核对已有设置页定位能力与绑定，再做小批量实现或“已满足”证据。

## 当前第三轮 R15-07 失败结果复制（代码已提交，隔离验证完成；真实宿主待验）

- `37dd4a03` 复用已有 `CopyTaskErrorCommand`、任务错误字段、恢复报告脱敏文本和剪贴板重试入口；新增 Contracts 共享 `ClipboardTextSanitizer`、短摘要 `FailureSummary`、安全详情 `SafeDetailMessage`、可测试的复制格式化器和最多 4 次 COM/`InvalidOperationException` 瞬时失败重试。完整复制继续保留 `ErrorMessage`、`ErrorCode`、`DetailMessage` 和任务 ID。
- Task Center 失败卡先显示脱敏首行摘要与错误码；技术详情默认收起，使用生产 `GscWpfUiTextBox` 的只读可选择有限高控件，避免长堆栈撑开任务页。游戏选框、滚动条、命令绑定、取消/错误/恢复保护和 net462 保持。
- `R15TaskFailureCopyTests` `6/6`；R06/R12/R15 相邻回归 `14/14`；外部源码副本完整 Release solution `7 warnings/0 errors`（均为离线 NuGet `NU1900`）；Playnite `net462` 定向构建保留 `MediaCenterView.xaml.cs:664` 的 2 条既有 warning；source/XAML/diff 和 WPF 静态 `0/28/162` 通过。
- 未验真实 Playnite/package-host、RenderHarness、最终呈现、DPI/UIA/IME、presented frame、ETW 或宿主性能；linked `obj` 仍 `Access denied`，使用外部源码副本。只用合成 DTO、fake 剪贴板 setter、隔离 STA WPF/构建，Demo 原目录不可用；main 用户改动和 `src.zip` 未碰、未合并。R15-06 的 Worker 全量 `342/1 skipped/1 failed/344` 既有失败未改写。
- 证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R15-07-TASK-FAILURE-COPY-20260920.md`。下一可执行任务：`R15-08 清理历史范围`，先核对清理命令、运行中任务保护、恢复账本和日期/状态预览。

## 当前第三轮 R15-06 耗时与吞吐（代码已提交，隔离验证完成；真实宿主待验）

- `6f65638e` 复用现有 `TaskProgress`、`TaskStatusDto`、SQLite 任务查询和 Task Center 详情，新增可选可靠工作量采样。明确总量的整库游戏数、媒体专属候选文件数和已知下载总字节才展示速率/ETA；普通阶段、未知总量和恢复/远端无实际进度的任务不推算。
- 采样器最多保留 5 个推进样本，至少两个推进样本才给速率；15 秒无推进重置窗口，10 秒无新推进隐藏速率和 ETA。SQLite 追加列并自动迁移旧库，广播/快照比较器/最近、活动和分页查询均保留采样字段；普通 `ReportAsync` 清除旧采样。
- Task Center 详情增加按可靠性折叠的“可靠进度采样”卡片；已有开始/结束耗时、游戏选框、滚动条、命令绑定、取消/错误/恢复保护和 net462 保持。
- 证据：最终外部隔离 Release solution 到达 Playnite `net462`，`0 errors/2` 条既有 warning；Core `106/106`；Worker 定向 `20/20`；Playnite R15 `11/11`；XAML `24/24`、source/diff 通过；WPF 静态 `0/28/162`。Worker 全量门禁为 `342 passed/1 skipped/1 failed/344 total`，失败是既有 `tests/GameSaveCenter.Worker.Tests/MediaSyncServiceTests.cs:570`，未写成全量通过。
- 未验真实 Playnite/package-host、RenderHarness、最终呈现、DPI/UIA/IME、presented frame、ETW 或宿主性能；linked worktree 的 `obj` 直接写入仍 `Access denied`，最终使用外部源码副本。只用合成/fake/隔离 SQLite/目录，Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未碰、未合并。证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R15-06-TASK-THROUGHPUT-20260920.md`。
- 下一可执行任务：`R15-07 失败结果复制`，先核对现有任务摘要、错误码、脱敏详情和剪贴板失败语义；同时保留 Worker 全量既有失败和真实宿主呈现边界。

## 当前第三轮 R15-05 任务来源定位（代码已提交，隔离验证完成；真实宿主待验）

- `0d1ff346` 复用现有 `TaskStatusDto`、`TaskCoordinator`、Worker 广播、SQLite 任务查询、`OpenCloudQueue` 和媒体历史 `BatchId` 恢复；新增稳定来源引用 DTO/持久化列。恢复/远端暂存记录 `BackupVersion`，备份/媒体云重试记录 `CloudTransfer`，有游戏 ID 的任务保留 `Game`；没有把普通媒体同步任务冒充媒体批次任务。
- Task Center 详情新增来源卡片，游戏继续使用原关联游戏入口；版本、媒体批次和云队列按 `BackupId`/`BatchId`/稳定队列键精确恢复。来源对象不存在时清除待选目标并保留诊断，不跳同名游戏或邻近版本。
- 已验证：源码门禁、XAML `24/24`、`git diff --check`；当前分支外部源码副本 solution Release `0 errors/2 条既有 nullable warning`；Playnite R15 `10/10`、Worker 来源/任务回归 `18/18`；WPF 静态 `0/28/162`。
- 未验真实 Playnite/package-host、删除/重命名后的来源卡片呈现、UIA/读屏、IME、物理 DPI/跨屏、presented frame、ETW 或宿主性能。只用合成/fake/隔离 SQLite；Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未碰、未合并。证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R15-05-TASK-SOURCE-LOCATION-20260920.md`。
- 下一可执行任务：`R15-06 耗时与吞吐`，先核对可靠耗时/进度采样和未知总量语义。

## 当前第三轮 R15-04 重复通知归并（代码已提交，隔离验证完成；真实宿主待验）

- `a67d371e` 复用既有 `BoundedTaskIdSet`、`SessionNotificationAccumulator`、`NotificationLevelPolicy`、Dashboard Toast 和 Task Center 历史，新增按任务/终态/失败证据归并的 `TaskNotificationDeduper`。进度事件不领取通知键；相同失败证据只通知一次，不同失败保留；摘要后的新失败/取消不静音；完整错误仍可从 Task Center 历史读取。
- 已验证：`validate-source.py`、XAML `24/24`、`git diff --check`；Playnite Release `net462` 外部源码副本项目构建 0 errors、2 条既有 nullable warning；通知/会话/R15 时间线/R13 相邻定向夹具 `28/28`；WPF 静态 `0/28/177`。
- linked WPF 临时项目直接构建仍 `Access denied`，未写成完整 solution/RenderHarness/真实宿主通过；未验 Toast/OS 通知、真实 Playnite、最终呈现、DPI/UIA/IME、ETW/性能。Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未碰、未合并。证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R15-04-TASK-NOTIFICATION-DEDUPE-20260920.md`。
- 下一可执行任务：`R15-05 任务来源定位`，先核对稳定对象身份和删除对象负例。

## 当前第三轮 R15-03 任务详情时间线（代码已提交，隔离验证完成；真实宿主待验）

- `fe0c05a9` 复用现有 `TaskChangeEventDto`、`TaskCoordinator`、Worker 事件广播和 `TaskCenterView`；变更事件增加 Worker 观察到的 `OccurredUtc`，广播 clone 保留 `StageMessage` 与 `CancellationState`。
- `TaskTimelineBuilder` 仅整理已知创建、开始、阶段、取消和结束事件，按 UTC/序号稳定排序并同时显示本地时间与 UTC；缺少事件或时间显示“时间未知”，没有关联依据不推断重试。Dashboard 运行期事件窗口最多 64 条/任务、200 个任务，详情时间线为有限高度卡片。
- 已验证：`validate-source.py`、XAML `24/24`、`git diff --check`；Worker Release 隔离定向 `11/11`；Playnite Release `net462` 构建 0 错误、R06 取消回归 + R15-01/R15-02/R15-03 定向 `11/11`；WPF 静态检查 `0/28/162`。Playnite 保留 `MediaCenterView.xaml.cs:664` 的 2 条既有 nullable warning。
- 未验真实 Worker 重启后历史事件的持久化时间线；本批未宣称完整 solution、RenderHarness、真实 Playnite/最终呈现、DPI/UIA/IME、ETW 或宿主性能。只用合成/fake/隔离数据，Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未碰、未合并。
- 证据见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R15-03-TASK-TIMELINE-20260920.md`。下一可执行任务：`R15-04 重复通知归并`，先核对现有通知、会话摘要和失败历史入口。

## 当前第三轮 R15-02 取消过程展示（代码已提交，隔离验证完成；真实宿主待验）

- `9c8241fb` 先复用现有 `TaskCoordinator`、取消 IPC、任务 DTO、SQLite 查询和 `TaskCenterView`，新增共享取消阶段 `Requested`、`Finalizing`、`Cancelled`、`NotInterruptible`。任务运行时闸门保证连点取消只调用一次令牌取消；成功/取消竞争、取消后失败和终态晚到取消分别收敛，不永久停在取消中。
- `TaskStatusDto` 提供取消显示和可取消/取消中判定；Task Center 详情新增取消状态卡。`tasks.cancellation_state` 通过现有迁移入口接入新增、最近、活动和分页查询，快照比较和任务复制列同步阶段字段，旧库默认空值兼容。
- 已验证：`validate-source.py`、XAML `24/24`、`git diff --check`；Worker Release 隔离项目定向测试 `14/14`；Playnite Release `net462` 构建 0 错误、R06 取消回归与 R15-01/R15-02 定向测试 `8/8`；WPF 静态检查 `0/28/162`。构建保留 `MediaCenterView.xaml.cs:664` 的 2 条既有 nullable warning。
- 完整 solution 脚本在 linked worktree 生成 WPF 临时项目时遇到 `Access denied`，没有写成完整 solution 通过；Worker 与 Playnite 已分别实际构建。只用合成/fake/隔离目录，未写真实存档、媒体、云端或诊断。Demo 原目录不可用，沿用恢复生产基线；main 用户改动与 `src.zip` 未碰、未合并。
- 证据见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R15-02-TASK-CANCELLATION-20260920.md`。下一可执行任务：`R15-03 任务详情时间线`，先核对已有任务事件缓存、阶段字段和详情滚动容器；真实 Playnite/RenderHarness、最终呈现、DPI/UIA/IME、presented frame、ETW 和宿主性能仍待验。

## 当前第三轮 R15-01 任务阶段可读（代码已提交，隔离验证完成；真实宿主待验）

- `4e7ac33a` 复用现有 `TaskCoordinator`、任务 DTO、SQLite 查询和 `TaskCenterView`，新增 Contracts 层 `TaskStageResolver` 与 `TaskStatusDto.StageMessage`。已有 Worker 阶段事件可显示为扫描、校验、索引、上传、下载、恢复、清理等可读阶段；失败/取消保留最后真实阶段，错误和技术详情独立显示。
- 运行中无可靠总量时进度显示 `—`，不把 0% 当成真实进度；`tasks.stage_message` 通过现有迁移入口补列，旧库空值可继续读取。Task Center 增加阶段列和详情，不改变任务命令、取消入口、滚动或有限列表边界。
- 已验证：`validate-source.py`、XAML `24/24`、`git diff --check`；Worker Release 隔离定向测试 `12/12`；Playnite Release `net462` 定向测试 `2/2`，构建有既有 `MediaCenterView.xaml.cs:664` nullable warning 2 条；WPF 静态检查 `0/28/177`。
- 未验：真实 Playnite/RenderHarness、真实宿主各类阶段事件全覆盖、最终呈现、DPI/跨屏、UIA/IME、presented frame、ETW、宿主性能。Demo 原目录不可用，沿用恢复生产基线；main 用户改动与 `src.zip` 未碰、未合并。
- 证据见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R15-01-TASK-STAGES-20260920.md`。下一可执行任务：`R15-02 取消过程展示`，先核对取消请求、取消中状态和成功/取消竞争的终态收敛。

## 当前第三轮 R14-08 来源规则试运行（代码已提交，隔离验证完成；真实宿主待验）

- `89141528` 复用现有来源规则 DTO、媒体扩展名和文件模式匹配，增加来源规则草稿的只读试运行 IPC/Worker/UI 链。每个样本显示命中/排除、文件名、大小、路径和确定原因；试运行不保存规则、不写媒体记录、不移动文件。
- Worker 的 linked cancellation token 同时受时间预算、扫描数量预算和样本数量预算保护；请求值分别收敛到 `100–5000ms`、`1–5000`、`1–200`。UI 默认 `120/2000/1500ms`，部分扫描显示 `Partial`/预算原因。来源规则列表保持有限高度、Recycling 和当前滚动/命令绑定。
- 已验证：`validate-source.py`、XAML `24/24`、`git diff --check`；Worker Release 隔离构建 `0 warning / 0 error`、新增行为测试 `1/1`；Playnite Release `net462` 构建与 R14-08 契约测试 `1/1`，构建有既有 `MediaCenterView.xaml.cs:664` nullable warning 2 条；WPF 静态检查 `0 errors / 28 warnings / 177 info`。
- 未验：真实 Playnite 来源页和 RenderHarness/最终呈现、权限拒绝/超大目录、DPI/跨屏、UIA/IME、presented frame、ETW、宿主性能。render-qa 的 linked `obj` 权限阻塞没有被写成呈现通过。Demo 原目录不可用，沿用恢复生产基线；main 用户改动与 `src.zip` 未碰、未合并。
- 证据见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R14-08-SOURCE-RULE-PREVIEW-20260920.md`。下一可执行任务：`R15-01 任务阶段可读`，先核对现有 TaskCoordinator/任务事件阶段 DTO。

## 当前第三轮 R14-07 媒体详情浏览（代码完成，环境待验）

- 本批在现有 `Media` 分页窗口、`SelectedMedia` 和 `SelectionAnchorResolver` 上增加上一项/下一项；导航只跨当前已加载窗口，并按稳定 `MediaId` 将选中项滚回 `MediaGrid` 可见行。
- 详情复用 `MediaItemDto` 显示类型、来源、大小和采集时间；`AsyncThumbnailImage` 显示实际解码的 `PixelWidth × PixelHeight`。视频缺失、不支持格式或 `MediaFailed` 显示统一回退，截图加载继续保留 generation、取消和失败状态。
- 已验证：`validate-source.py`、XAML 24/24、`git diff --check`；新增尺寸行为断言和 R14-07 源码契约夹具。Playnite Tests Release 构建退出 1，仅有 0 警告/0 错误且无诊断，未写成构建或 testhost 通过。
- 未验跨页导航、真实视频编解码、Playnite/RenderHarness、Release/net462、宿主呈现、DPI/跨屏、UIA/IME、presented frame、ETW 和宿主性能。Demo 原目录不可用，沿用恢复生产基线；main 用户改动、src.zip 未碰、未合并。
- 证据见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R14-07-MEDIA-DETAIL-20260920.md`。下一可执行任务：在可用 SDK/Workload 环境补跑 R14-04/R14-05/R14-06/R14-07 定向验证，再核对 R14-08。

## 当前第三轮 R14-06 批量目标防误选（代码完成，环境待验）

- cfbb1279 已推送。复用现有 GameDescriptorDto/GameStatusDto、Playnite 适配和 Games/SelectedItem 目标绑定，沿描述快照增加只读本地 IconPath 与 IdentityDisplay；归类目标下拉及全局选框现在显示图标（无图标回退平台占位/首字母）、平台和稳定 PlayniteId。
- 全局 GamePickerViewModel 原有搜索/平台/状态筛选、被筛选隐藏目标保留和“显示当前游戏”恢复未改；目标控件没有改成 SelectedIndex，重名游戏按对象绑定和稳定 ID 区分。媒体批量归类、预览目标覆盖、单项重新归类共用目标展示模板。
- 已验证：python scripts/validate-source.py、XAML 24/24、git diff --check；新增选择展示行为/源码契约夹具。定向 Playnite testhost 长时间无输出后仅终止当前会话，未产出可签收构建或测试结果，未写成通过。
- IconPath 只从 Playnite 现有本地图标引用解析，不下载、不写入、不读取真实存档或媒体；Demo 原目录不可用，沿用恢复生产基线。main 用户改动、src.zip 未碰、未合并；没有新增 artifacts/.tmp。
- 证据见 docs/design/reviews/ui-finesse-round3-20260915/evidence/R14-06-TARGET-GUARD-20260920.md。下一可执行任务：在可用 SDK/Workload 环境补跑 R14-04/R14-05/R14-06 定向验证，再推进 R14-07 媒体详情浏览。

## 当前第三轮 R14-05 重复媒体识别视图（代码完成，环境待验）

- `136285d5` 已推送。先复用既有 SHA-256 入库去重、`MediaItemDto` 和当前游戏媒体查询，新增当前游戏范围的确定/疑似重复组只读视图；确定组为相同非空哈希，疑似组为同类型、文件名和大小一致且排除确定组。
- Worker 扫描上限 5000、最多 100 组、每组展示 24 项；Media 新 Tab 的组和组内列表使用有限高度、FiniteViewport、Recycling，只有选择查看和重新识别，没有删除/移动/重新归类命令。请求带取消和 generation，失败不阻塞主媒体详情。
- 已验证：`validate-source.py`、XAML `24/24`、`git diff --check`；Worker/Playnite 夹具已加入但当前 linked `obj` Access denied/SDK-Workload 环境未产出可签收构建或运行时测试。现有 `media.sha256` 唯一约束意味着确定组主要兼容历史/异常数据，不把空结果写成全库绝对无重复。
- 只用合成/fake/隔离数据；Demo 原目录不可用，沿用恢复生产基线；main 用户改动、`src.zip` 未碰、未合并，没有新增 artifacts/.tmp。证据见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R14-05-DUPLICATE-INSPECTION-20260920.md`。
- 下一可执行任务：在可用 SDK/Workload 环境补跑 R14-04/R14-05 定向验证，再推进 `R14-06 批量目标防误选`。

## 当前第三轮 R14-04 撤销边界说明（实现已满足，运行时环境待验）

- 既有 `1c0c5a37`/`a7c39922` 已提供媒体归类历史、最近批次和所选可回退批次的撤销入口；`03521991` 补充应用后人工修改再撤销的隔离负例夹具。
- 撤销前重新核对批次项和当前媒体快照，持久层以目标、Assigned、应用后归档路径和 Applied 批次项做条件提交；冲突项目保留后来人工决定，不覆盖其状态或归档文件。
- 已验证：`validate-source.py`、XAML `24/24`、`git diff --check`；正常撤销和人工修改负例已写入 Worker 行为夹具，但当前定向 dotnet testhost 长时间无输出，未产出可签收运行时结果。
- 只用合成/fake/隔离目录；Demo 原目录不可用，沿用恢复生产基线；main 用户改动、`src.zip` 未碰、未合并，没有新增需要保留的 artifacts/.tmp。证据见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R14-04-UNDO-BOUNDARY-20260920.md`。
- 下一可执行任务：`R14-05 重复媒体识别视图`，先核对现有 hash/元数据重复检测，再做只读分组和不删除真实媒体的行为证据；R14-04 运行时复跑仍待可用 SDK/Workload。

## 当前第三轮 R14-03 部分成功处理（代码已提交，环境待验）

- `aef251b1` 已推送到 `codex/ui-finesse-round2`。复用现有媒体批量归类/忽略/恢复的逐项 best-effort 结果，UI 保存失败稳定 MediaId 和逐项原因，并提供只重试失败集合的命令。
- 失败列表使用有限高度和 Recycling；重试沿用原批次操作与归类目标，不读取新的列表选择，成功项不会再次提交。Worker 成功项/失败项和媒体归档副本语义保持。
- 已验证：`validate-source.py`、XAML `24/24`、`git diff --check`；Worker 隔离行为夹具和 Playnite 源契约夹具已加入但未执行。未验证 Worker/Playnite 运行时、Release/net462、RenderHarness、真实宿主/呈现/DPI/UIA/IME/ETW/性能。
- 只用合成/fake/隔离数据；没有新的 artifacts/.tmp 产物。Demo 原目录不可用，沿用恢复生产基线；main 用户改动、`src.zip` 未碰、未合并。
- 下一可执行任务：在可用 SDK/Workload 环境补跑 R13-07/R13-08/R14-01/R14-02/R14-03 定向测试与相关回归，再推进 `R14-04 撤销边界说明`。

## 当前第三轮 R14-02 预览选择编辑（代码已提交，环境待验）

- `69cf2a72` 已推送到 `codex/ui-finesse-round2`。复用现有媒体归类预览批次、稳定 `MediaId`、Worker 重验和应用/撤销链；预览项现在可排除，且高置信建议可在当前游戏目录中调整目标。
- 应用请求只提交当前纳入项的稳定 ID；`TargetPlayniteId` 覆盖通过 `MediaClassificationTargetOverrideDto` 传递。Worker 校验当前游戏目录和批次 `Pending` 状态，合法目标写回批次原因后重新进入既有冲突、取消、恢复保护语义，非法目标跳过并保持未归类。
- 已验证：`validate-source.py`、XAML `24/24`、`git diff --check`、Contracts/Core Release 隔离构建 `0/0`。未验证：当前 Core testhost 可运行结果、Worker/Playnite 测试、Release/net462、RenderHarness、真实 Playnite/呈现/DPI/UIA/IME/ETW/性能。
- 只用合成/fake/隔离数据；`.tmp/r14-02-build` 已清理。Demo 原目录不可用，沿用恢复生产基线；main 用户改动、`src.zip` 未碰、未合并。
- 下一可执行任务：在可用 SDK/Workload 环境补跑 R13-07/R13-08/R14-01/R14-02 定向测试与相关回归，再推进 `R14-03 部分成功处理`。

## 当前第三轮 R14-01 归类建议解释（代码已提交，环境待验）

- `7735cd7c` 已推送到 `codex/ui-finesse-round2`。复用现有 `MediaSyncService` 建议算法与来源规则、游戏会话、进程映射、文件名匹配，新增 `MediaClassificationEvidenceDto` 结构化依据；多候选保留各候选依据，无依据显示“待判断”，不伪造目标或置信事实。
- Media 预览卡继续使用既有有限高度/Recycling/Inspector 滚动与命令链；本批只展示合成证据，不新增服务、IPC、移动、删除或真实数据写入。Worker/Core/Playnite/RenderHarness 夹具已补齐。
- 已验证：`validate-source.py`、XAML `24/24`、`git diff --check`、Contracts/Core Release 隔离构建 `0/0`。未验证：Core testhost（项目引用目标框架评估退出 `1`）、Worker restore/测试、Playnite `net462`、RenderHarness、真实宿主/呈现/DPI/UIA/IME/ETW/性能。
- R14 新隔离构建目录已清理；此前 `.tmp/r13-verify-source` 曾短暂被占用，阶段末已精确删除，未强杀未知进程。main 用户改动、`src.zip` 未碰、未合并；Demo 原目录不可用，沿用恢复生产基线。
- 下一可执行任务：先在可用 SDK/Workload 环境补跑 R13-07/R13-08/R14-01 定向测试与相关回归，再推进 `R14-02 预览选择编辑`。

## 当前第三轮 R13-08 失败分类帮助（代码已提交，环境待验）

- `96a4c6a9` 已推送到 `codex/ui-finesse-round2`。复用稳定 Rclone 错误码，新增无空间 `RCLONE_NO_SPACE`、限流 `RCLONE_RATE_LIMITED`；认证、空间、远端不存在、校验差异、限流各有 display-only 下一步，未知错误不生成建议。
- 维护详情沿用共享样式，已识别帮助与默认折叠的“原始诊断”分开；原始错误码/详情保留，可访问名称保留。限流进入既有有界退避，不改上传、取消、恢复保护和本地副本语义。已加入 Core/Worker/Playnite 定向夹具但未执行。
- 已验证：源码校验、XAML `24/24`、diff check。未验证：Core/Worker/Playnite 测试、Release/net462、RenderHarness、真实 rclone/远端配额和宿主。主机只有 SDK `9.0.302`，`global.json` 的 `8.0.100` 向上滚动命中缺失 Workload resolver 目录，Worker restore 退出 `1`；不写成 build/test 通过。
- 只用合成/fake/隔离数据，Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未碰、未合并。R13-07 隔离源目录首次清理时短暂被占用，阶段末已精确删除，未强杀未知进程。
- 下一可执行任务：在可用 SDK/Workload 环境同时重跑 R13-07/R13-08 定向测试和相关回归，通过后推进 `R14-01 归类建议解释`。

## 当前第三轮 R13-07 队列筛选与汇总（代码已提交，环境待验）

- `d6c2af90` 已推送到 `codex/ui-finesse-round2`。复用既有状态/类型筛选、查询一致性 token、分页追加、`existingKeys` 去重和选中项恢复；新增游戏/Playnite ID 片段、来源设备、24 小时/7 天/30 天时间窗口以及未筛选 `GlobalTotalCount`。维护页摘要区分当前筛选与全局计数，筛选栏使用可收缩列；RenderHarness 合成 ViewModel 同步新绑定。
- 已加入 Worker 合成 SQLite 行为夹具和 Playnite 源行为夹具，覆盖游戏/设备/时间正例、错误设备负例、全局计数、筛选绑定、分页去重和摘要。但新增 Worker/Playnite 定向测试尚未执行，不把夹具写成通过。
- 已验证：`python scripts/validate-source.py`、XAML `24/24`、`git diff --check`。未验证：Worker/Playnite 编译测试、Release/net462、RenderHarness、真实 Playnite/package-host。当前主机只有 SDK `9.0.302`，`global.json` 的 `8.0.100` 向上滚动命中缺失 Workload resolver 目录，Worker restore 退出 `1` 且没有 `project.assets.json`；linked `obj` 另有 Access denied。
- 只用合成/fake/隔离数据，未写真实网络、云端、存档、媒体或诊断；Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未碰、未合并。`r13-07-build` 已清理；`r13-07-source` 首次清理时短暂被占用，阶段末已精确删除，未强杀未知进程。
- 下一可执行任务：在可用 SDK/Workload 的隔离目录同时重跑 R13-07/R13-08 定向夹具和相关回归；通过后推进 `R14-01 归类建议解释`。

## 当前第三轮 R13-06 离线恢复反馈

- `e4244329` 已推送到 `codex/ui-finesse-round2`。复用现有 `CloudRetryPolicy`/`CloudRetryService`，只新增 `CloudTransferStatusDto.NetworkRecoveryDisplay` 和维护页绑定，没有改变上传、校验、取消、错误、恢复保护或队列调度语义。
- 网络失败的 `RetryScheduled` 显示等待网络恢复、退避次数和本轮最多 10 项；进入 `Transferring` 显示网络已恢复、按批次上传中；认证失败不套用网络恢复文案。现有 Worker 30 秒轮询、每轮最多 10 项、顺序处理、最多 6 次退避重试且无逐条旧失败通知。
- 验证：Release 外部隔离 solution `0 warning / 0 error`、Playnite `net462`；Core `1/1`、Worker `CloudRetryPersistenceTests 10/10`、Playnite R13 `11/11`、XAML `24/24`、source validation/diff check 通过。证据见 [R13-06 离线恢复反馈](../design/reviews/ui-finesse-round3-20260915/evidence/R13-06-OFFLINE-RECOVERY-20260919.md)。
- 只用合成 DTO、fake/隔离 SQLite 和外部构建；未运行真实网络/rclone、真实 Playnite/package-host、RenderHarness、最终呈现、物理 DPI/跨屏、UIA/IME、presented frame、ETW 或宿主性能。Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未碰、未合并；`.tmp/r13-06-*` 已清理，旧 `.tmp/r12-07-build-final` 因 Access denied 暂留。
- 下一可执行任务：`R13-07 队列筛选与汇总`，先查现有状态筛选、全局计数、分页和选中项联动，再补筛选负例与汇总证据。

## 当前第三轮 R13-05 远端证据详情

- `e280cf1c` 已推送到 `codex/ui-finesse-round2`。复用现有云端队列、远端路径布局、`CloudTransferStatusDto`、`CloudTransferStateService` 和维护页详情入口，新增 display-only 远端对象/来源设备/最后尝试/最后成功校验字段；没有改变上传、校验、取消、错误或恢复语义。
- `CloudRemoteDisplay` 对 URI 用户信息、query/key-value secret 和 Bearer 值脱敏；既有 `CopyDiagnosticsCommand` 继续经 `ClipboardValueSanitizer`。当前行 `RemoteVerified` 的 `UpdatedUtc` 才作为可证实成功校验时间，其他状态显示未知，因为队列没有历史成功校验持久化列。
- 验证：Release 外部隔离 solution `0 warning / 0 error`、Playnite `net462`；Core `2/2`、Worker `1/1`、Playnite R13 `10/10`、XAML `24/24`、source validation/diff check 通过。只用合成 DTO、fake/隔离 SQLite 和脱敏负例；未运行 RenderHarness/真实 Playnite/package-host/真实远端或最终呈现。
- Demo 原目录不可用，沿用恢复生产基线；游戏选框、滚动条、命令绑定、取消/错误/恢复保护、有限列表和 net462 保持。main 用户改动与 `src.zip` 未碰、未合并；本批 `.tmp/r13-05-*` 已清理，旧 `.tmp/r12-07-build-final` 仍因 Access denied 暂留。
- 下一可执行任务：`R13-06 离线恢复反馈`，先查现有 Worker/维护页离线状态、恢复入口和错误分类，再补网络恢复前后状态变化与负例。

## 当前第三轮 R13-04 暂停与允许时段

- `cca3f052` 已推送到 `codex/ui-finesse-round2`。复用现有 `CloudUploadQueuePaused`、允许时段持久化、`CloudRetryService` 和设置页入口；`QueueControlDisplay` 现在实际区分用户暂停、允许时段外、队列空闲和运行中。
- 设置页明确暂停只影响后台自动重试，恢复后继续处理已保存队列；允许时段修改在下一轮 Worker 检查生效，已开始的上传不会被取消。源代码复核确认暂停跳过 sweep、时段外持久化 defer，未把静态复核写成真实云端运行时证明。
- 证据：Core 定向 `1/1`；Worker 允许时段/暂停持久化 `3/3`；Playnite `R13CloudTransferStageBehaviorTests 9/9`、既有 `PortableSettingsTests 10/10`；隔离 Debug `0 warning / 0 error`、XAML `24/24`、source validation/diff check 通过。证据见 [R13-04 暂停与允许时段](../design/reviews/ui-finesse-round3-20260915/evidence/R13-04-PAUSE-WINDOW-20260919.md)。
- 只使用合成 DTO、fake/隔离设置和外部构建，未触碰真实云端、存档、媒体或诊断；未验真实 Playnite/package-host、进行中上传时序、物理 DPI/跨屏、UIA/IME、presented frame、ETW、宿主性能。Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未碰、未合并。
- 下一可执行任务：`R13-05 远端证据详情`，先核对远端验证结果、脱敏和“已上传不等于已验证”的详情入口。

## 当前第三轮 R13-03 手动重试范围

- `680ea83a` 已推送到 `codex/ui-finesse-round2`。先复用现有单项/媒体重试入口、任务中心批量入口和 IPC ledger；新增 `CloudTransferStatusDto.CanManuallyRetry` 与 `ManualRetryScopeDisplay`，维护页明确仅重试当前选中的失败/排队项，只复制已保留的云端源，不重新执行本地备份。
- 实际状态行为验证：失败/排队可重试，传输中/已上传/已校验不可重试；真实 `RelayCommand.CanExecute` 忙态门控下第二次点击提交数保持 `1`。任务中心既有批量入口仍只处理当前筛选结果并按任务类型/游戏去重；`RetryCloudUpload`/`RetryMediaCloudUpload` 保持同一 RequestId 的 replay protection。
- 证据：Playnite `8/8`、Core `29/29`、Worker 云状态/部分成功 `19/19`、Worker IPC ledger `6/6`，隔离 Debug `0 warning / 0 error`，XAML `24/24`，source validation/diff check 通过。Playnite named-pipe 类为 `1 passed / 6 skipped / 0 failed`，跳过未计为真实 IPC 通过。证据见 [R13-03 手动重试范围](../design/reviews/ui-finesse-round3-20260915/evidence/R13-03-MANUAL-RETRY-SCOPE-20260919.md)。
- 只使用合成 DTO、fake/隔离 SQLite、外部隔离构建，未触碰真实云端、存档、媒体或诊断；未验真实 Playnite/package-host、物理 DPI/跨屏、UIA/IME、presented frame、ETW、宿主性能。Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未碰、未合并。
- 下一可执行任务：`R13-04 暂停与允许时段`，先核对 `CloudUploadQueuePaused`、允许时段、持久化队列状态和进行中上传边界。

## 当前第三轮 R13-02 下次重试时间

- `6fd22892` 已推送到 `codex/ui-finesse-round2`。复用 `NextAttemptUtc/NextAttemptLocal`，维护页详情显示 `RetryTimingDisplay`：未来给绝对时间和约 N 分钟/小时/天后，到期给“可立即重试”，无时间给“无自动重试”；没有新增每行常驻计时器。
- 证据：Core `29/29`、Playnite `2/2`、Worker `CloudTransferStateTests 11/11`，隔离 Debug `0 warning / 0 error`，XAML `24/24`，source validation/diff check 通过。证据见 [R13-02 下次重试时间](../design/reviews/ui-finesse-round3-20260915/evidence/R13-02-RETRY-TIMING-20260919.md)。
- 构建和测试只使用当前分支外部隔离副本、fake/合成数据；未验真实 Playnite/package-host、真实远端、物理 DPI/跨屏、UIA/IME、presented frame、ETW、宿主性能。Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未碰。
- 下一可执行任务：`R13-03 手动重试范围`，先核对单项/媒体重试入口、幂等 requestId 和部分成功语义。

## 当前第三轮 R13-01 队列阶段展示

- `803470b8` 已推送到 `codex/ui-finesse-round2`。复用现有云端状态机、队列摘要和维护页，新增 `CloudTransferStatusDto.QueuePhaseDisplay`，区分等待队列、等待网络、等待重试、上传中、验证中、等待验证和已验证；认证失败负例仍是等待重试。
- 维护页表格与详情使用同一阶段显示，同时保留 `GuaranteeLevelDisplay`，上传成功不冒充远端校验；`QueueControlDisplay` 继续表达暂停、允许时段外和运行中。
- 证据：Core `27/27`、Playnite `7/7`、Worker `CloudTransferStateTests 11/11`，隔离 Debug `0 warning / 0 error`，XAML `24/24`，source validation/diff check 通过。证据见 [R13-01 队列阶段展示](../design/reviews/ui-finesse-round3-20260915/evidence/R13-01-CLOUD-QUEUE-STAGES-20260919.md)。
- 构建使用当前 linked worktree 身份的外部隔离副本，已清理外部源码/输出；未验真实 Playnite/package-host、真实远端、物理 DPI/跨屏、UIA/IME、presented frame、ETW、宿主性能。Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未碰。
- 下一可执行任务：`R13-02 下次重试时间`，先核对既有 `NextAttemptUtc/NextAttemptLocal`、系统时钟变化和页面生命周期。

## 当前第三轮 R12-08 恢复结果报告

- `fd4756ea` 已推送到 `codex/ui-finesse-round2`。复用既有 `RestoreOrchestrator`、`TaskCoordinator`、`TaskStatusDto`、PreRestore 和任务详情滚动容器，新增持久化的无凭据 `RestoreReportDto`；报告展示目标版本、预览文件范围、保护备份、失败阶段、完成/回滚/人工介入/取消/失败结果和任务 ID。
- TaskCenter 结果卡沿用现有 ScrollViewer，复制命令在有报告时只复制脱敏文本；SQLite 最近、活动和分页任务查询均回读报告。成功与部分完成/回滚/人工介入/失败分开投影，未改变游戏选框、滚动条、命令绑定、取消/错误、恢复保护、有限列表性能或 net462。
- 证据：Playnite R12 `15/15`，Worker `RestoreReadinessTests|RestoreOrchestratorTests|TaskQueryPersistenceTests 34/34`，隔离 Debug `0 warning / 0 error`，XAML `24/24`，source validation/diff check 通过。证据见 [R12-08 恢复结果报告](../design/reviews/ui-finesse-round3-20260915/evidence/R12-08-RESTORE-RESULT-REPORT-20260919.md)。
- 仍未验真实 Playnite/package-host、全量 WPF、物理 DPI/跨屏、UIA/IME、presented frame、ETW、宿主性能；Demo 原目录不可用，沿用恢复生产基线。当前 linked worktree WPF 临时项目仍 Access denied，最终构建使用当前分支外部隔离源码副本；main 用户改动和 `src.zip` 未碰。关闭 MSBuild/VB/C# 编译器服务器后，`.tmp/r12-07-build-final` 仍逐项 Access denied，待下一启动重试精确清理。
- 下一可执行任务：`R13-01 队列阶段展示`，先核对 `CloudTransferCoordinator`/维护页现有阶段投影和 Q22 依赖。

## 当前第三轮 R12-07 预览失效重验

- `00724e62` 复用既有恢复确认、映射解析、readiness 校验和 PreRestore；`DashboardViewModel.RestoreAsync` 在确认返回后重新比较游戏 ID/版本 ID，确认期间切换对象不会提交旧确认。
- Worker 真实编排保持最新映射/精确 `BackupId`，在写入前重新预览目标，写入后继续结果校验；新增 fake 行为证据固定为 `true → false → true`。同大小归档内容替换在 Manifest SHA-256 下返回 `Corrupted/Failed`。
- 隔离 Debug solution `0 warning / 0 error`、XAML `24/24`、Playnite R12 `13/13`、Worker `27/27`；源码校验与 diff check 通过。没有新增 XAML/视觉资源，选框、滚动条、命令绑定、取消/错误、恢复保护和有限列表保持。
- 证据见 [R12-07 预览失效重验证据](../design/reviews/ui-finesse-round3-20260915/evidence/R12-07-RESTORE-REVALIDATION-20260919.md)。未验真实 Playnite/package-host、全量 WPF、物理 DPI/跨屏、UIA/IME、presented frame、ETW、宿主性能；Demo 原目录不可用，沿用恢复生产基线。main 用户改动和 `src.zip` 未碰。`.tmp/r12-07-build-final` 已尝试精确清理但受 VB/C# 编译器服务器锁定而暂留，未强杀未知进程；下一次启动先重试清理，再执行 R12-08 恢复结果报告。

## 当前第三轮 R12-06 恢复冲突说明

- `423856b2` 复用已有恢复 readiness/task/error 结果，在 `RestoreWorkflowProgress` 中按游戏运行、操作锁、磁盘空间、目标权限和未知原因给出处理步骤；泛化写入失败不冒充权限问题，所有说明均保留安全检查，不建议无依据关闭安全机制。
- SaveCenter 只在原四步流程卡片新增 `ResolutionDisplay` 和 Automation HelpText；命令/绑定、取消/错误传播、PreRestore/回滚、游戏选框、滚动条、有限列表与 net462 保持。
- 隔离 Debug solution `0 warning / 0 error`、XAML `24/24`；Playnite R12 `11/11`，Worker `RestoreOrchestratorTests 12/12`；源码门禁与 diff check 通过。新增冲突说明行为 `4/4`，覆盖四类原因和泛化写入失败负例。
- 证据见 [R12-06 恢复冲突说明](../design/reviews/ui-finesse-round3-20260915/evidence/R12-06-RESTORE-CONFLICT-EXPLANATION-20260919.md)。未验真实 Playnite/package-host、物理 DPI/跨屏、UIA/IME、presented frame、ETW、宿主性能；Demo 原目录不可用，沿用恢复生产基线。main 用户改动和 `src.zip` 未碰。下一可执行任务：R12-07 预览失效重验。

## 当前第三轮 R12-05 远端下载进度

- `23e5b9d4` 已推送到 `codex/ui-finesse-round2`。远端下载复用现有 `RemoteBackupStagingService`、`TaskCoordinator`、任务事件流和 Maintenance 绑定，阶段区分准备、下载到隔离区、校验、版本确认、清单写入和等待恢复确认。
- 取消/失败只清理本次隔离目录；取消消息保留“已清理/清理失败仍有残留”，清理失败不再静默。成功状态写明当前存档尚未恢复；非 RemoteStage 事件不会改变远端进度。
- 最终 Debug 隔离构建 `0 warning / 0 error`、XAML `24/24`；R12-05 Playnite `3/3`、Worker `22/22`；source/XAML/diff check 通过。全量隔离 WPF 在既有 R07 resize/focus 用例失败处停止，未改写为通过。
- 证据见 [R12-05 远端下载进度](../design/reviews/ui-finesse-round3-20260915/evidence/R12-05-REMOTE-STAGE-PROGRESS-20260919.md)。Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未碰。下一可执行任务：R12-06 恢复冲突说明。

## 当前第三轮 R12-04 恢复保护备份

- `e4e42f40` 复用已有 `RestoreOrchestrator`、`TaskCoordinator`、`GameOperationLock` 和 `RestoreWorkflowProgress`，将 PreRestore 失败统一为 `RESTORE_PRERESTORE_FAILED`；保护备份未成功、未锁定或本地保护索引未保存时，危险恢复在写入前中止。
- 执行阶段明确显示保护备份子阶段；成功任务会记录保护快照已创建并锁定，失败任务保留错误码、阶段和中止原因。重试仍重新获取同游戏锁、读取最新索引，并用重试时的当前状态创建新保护快照。
- 隔离 `scripts/build.ps1` 全流程：XAML `24/24`；Release solution `0 warning / 0 error`；Core `85/85`、Worker `326/326`；Playnite source `68` 类与 WPF `84` 类隔离进程全部返回 0；资源字典类定向 `137/39/0`。
- `RestoreOrchestratorTests` `12/12`、`R12RestoreWorkflowBehaviorTests` `7/7`；`validate-source.py`、XAML、diff check 通过。证据见 [R12-04 恢复保护备份](../design/reviews/ui-finesse-round3-20260915/evidence/R12-04-RESTORE-PROTECTION-20260919.md)。本轮只使用 fake、隔离目录和 STA/WPF 测试，不代表真实 Playnite/package-host、物理 DPI/跨屏、presented frame、UIA/IME、ETW 或宿主性能；Demo 原目录不可用，沿用恢复生产基线。
- 最新 main 一键命令 `GameSaveCenter-一键构建安装运行.cmd` 的 `DEV-INSTALL-008` 仍在安装前失败：源码组 `273 passed / 18 skipped / 1 failed / 292 total`，失败为 `DangerousConfirmationKeepsCancelAsTheInitialFocusTarget`。main dirty R08 实现与跟踪测试源断言尚未安全集成；本分支已推送，main 用户文件未触碰。下一可执行小批量为 R12-05 远端下载进度。

## 当前第三轮 R12-01 恢复流程分步摘要

- `e1a8da0c` 先复用 `BackupVersionDto.RestoreReadiness`、`TaskStatusDto` 与现有恢复命令，把 Save 页面恢复过程分成选择版本、可恢复性检查、目标核对、执行结果四个固定阶段；没有新增恢复服务/DTO/IPC，也没有改变 `PreRestore` 保护、确认、取消、错误传播或回滚语义。
- `R12RestoreWorkflowBehaviorTests` `6/6`：覆盖 warning、目标核对阻断、执行失败保留回滚详情、成功四阶段收口和原命令保持可达；相邻 R11/R06 合计 `17/17`。资源字典类 `137 passed / 39 skipped / 0 failed`，隔离 Release solution `0 warning / 0 error`，XAML `24/24`，源校验/XAML/diff check 通过。
- WPF 静态审查为 `0 error / 24 warning / 177 info`，warning 是既有外层滚动/布局提醒；当前提交绑定的 render-qa 完成双主题、多尺寸和滚动探针但真实退出 `1`，只命中既有 Overview 空列表 2 DIP、Task/Save 可读行、Settings 状态、Shell header/Media 几何基线。离屏结果不代表 Playnite 实机、物理 DPI、跨屏或安装通过。
- 证据使用合成 DTO/fake、隔离 STA WPF、隔离 `.tmp` 源副本，没有读写真实存档、媒体、云端或外发诊断。Demo 原目录不可用，沿用恢复生产基线；main 用户改动未触碰。main DEV-INSTALL-008 的 `73 failed / 588 passed / 57 skipped`、安装器退出 `1` 事实保持独立。下一可执行小批量为 R12-02 校验结果解释。

## 当前第三轮 R08-06 数字变化动效

- 先核对已有能力：概览页原有六个摘要计数、进度条和技术文本均已存在；本阶段只为六个摘要计数增加 `NumericChangeFeedback`，不改变服务/DTO/命令、游戏选框、滚动条、取消/错误/恢复保护或有限列表策略。`OverviewTaskProgressBar` 与技术时间文本明确排除。
- `acbfe7e0` 使用现有 `GscMotion` 的 render-only `ScaleTransform` pulse，峰值 `1.04`、最短反馈间隔 `420ms`；`e264acff` 仅修正 STA 测试取样，在渲染队列后检查真实动画时钟。六个指标固定 `96 DIP` 槽位，减动效即时落字。
- 最终隔离身份 `r08-06-source-e264acff` / `r08-06-build-e264acff`：solution Release `0 warning / 0 error`、Playnite `net462`、XAML `24/24`；R08Numeric `3/3`、R08PageSwitch `2/2`、R08BusinessFeedback `4/4`；源校验、XAML、diff check 通过；WPF 静态审查 `0 error / 21 warning / 177 info`。
- 最终 RenderHarness 绑定 `e264acff` 生成 `357` 张 PNG，双尺寸 Overview 已人工抽查，未见本项新增挤压；完整脚本真实退出 `1`，仅报告既有 Save/Task resize 的 `3/4` 行可读问题，不写成 `render-qa OK`。证据使用合成数据/隔离 STA/offscreen logical DIP；Demo 原始目录不可用，沿用恢复生产基线。
- 未验真实 Playnite/Worker 高频刷新、宿主呈现帧、物理 DPI/跨屏、UIA/读屏、ETW 或宿主性能。下一可执行任务为 R08-07 对话框遮罩同步。

## 当前第三轮 R08-05 页面切换轻量化

- 先核对已有能力：`AcrylicProductionShellView.Attach` 一次创建六个真实工作区页面，`NavigateTo` 通过页面缓存复用实例；本阶段只增加同页 `PageHost.Content` 引用保护，避免刷新期间重复导航触发无意义内容替换。没有重建页面、列表或导航体系。
- `ccd71c27` 增加真实生产 shell/page 的 STA 行为夹具和仅供审计的 shell `MeasureOverride`/`ArrangeOverride` 计数。任务→媒体为 `2/2`，媒体→任务为 `1/1`；DataGrid offset `10→10`，选中项、DataContext、96 项 ItemsSource 保持引用；同页再次导航为 `0/0`。PageHost 无 Effect、无 `AnimateEntrance(PageHost)`。
- 当前身份隔离 `r08-05-source-ccd71c27` / `r08-05-build-ccd71c27`：solution Release `0 warning / 0 error`、Playnite `net462`、XAML `24/24`；R08-05 `2/2`，相邻 R02/R08/壳层回归 `28/28`；source/XAML/diff check 通过；WPF 静态检查 `0 error / 21 warning / 177 info`。
- 证据仍为合成 DTO/fake、真实生产 WPF、隔离 STA/offscreen logical DIP；未验真实 Playnite/Worker、物理 DPI/跨屏、presented frame、UIA/读屏、ETW/宿主性能。Demo 原始目录不可用，沿用恢复生产基线。本项无页面级 XAML/响应式布局改动，未把 render-qa 结果冒充本项证据。下一项为 R08-06 数字变化动效。

## 当前第三轮 R08-04 业务完成节奏

- 先核对已有能力：`BusyOperationCoordinator` 等待真实 prepare/action 完成后才复位 `IsBusy`；Dashboard 任务通知只对 `Succeeded/Failed/Cancelled` 终态发出，任务列表和通知共同使用 `TaskStatusDto.State`，没有固定延迟伪造成功。`3bf90a00` 只在共享按钮补视觉忙态延迟，`43141399` 校正 net462 AutomationPeer 说明，不改变业务命令/DTO/安全语义。
- `Button.IsBusy` 仍即时作为命令门禁；只读 `IsBusyIndicatorVisible` 在持续 `120ms` 后才显示共享 spinner，快速完成会停止计时器并保持折叠，卸载会清理。实际反馈 Toast 使用兼容 net462 的 Name/HelpText 与 Name 属性变更信号，不写不存在的 `LiveSetting/LiveRegionChanged` API。
- 最终隔离身份 `GscBuildCommit=43141399` 的 solution Release `0 error`、Playnite `net462`，R02 忙态 `4/4`、R08-04 反馈 `4/4`、XAML `24/24`、源校验通过；8 条 `NU1900` 是漏洞索引网络警告。边界仍是合成 DTO/fake、STA WPF/offscreen logical DIP；未验真实 Playnite/Worker 长请求、Narrator/真实 UIA、物理呈现、ETW/宿主性能。Demo 原始目录不可用，沿用恢复生产基线。下一可执行任务为 R08-05 页面切换轻量化。

## 当前第三轮 R08-03 离屏与隐藏停机

- 先核对现有实现：不确定进度条原本只跟随 `IsIndeterminate` 的模板触发，真实入口是生产壳层忙碌按钮和 Dashboard 后台刷新提示。`4a18fe0c` 新增 `IndeterminateProgressBehavior`，按有效可见性、Loaded/Unloaded 和宿主 `WindowState` 控制共享 storyboard 的暂停/恢复，不改变业务 `IsIndeterminate`、命令绑定、服务 DTO、游戏选框、滚动条或安全语义。
- 当前身份隔离源码根 `r08-03-source-hiddenstop` 的 solution Release 构建为 `0 warning / 0 error`，Playnite `net462`，XAML `24/24`；焦点串行 `22/22`：Offscreen `1/1`、HotChange `1/1`、Reverse `2/2`、ProductionShell `10/10`、Foundation `8/8`；源校验通过。
- 实际 WPF 取样确认可见 spinner 移动超过 `0.5 DIP`；隐藏 Tab 和最小化窗口各在 `360ms` 等待中保持位置到小数后三位，恢复后继续运动。相邻资源字典类 `136/176`、`39` skipped、1 条既有 Save 时间列源码期望失败未归入本项。边界仍是合成控件、隔离 STA/offscreen logical DIP；未验真实 Playnite/物理呈现/UIA/ETW/宿主性能。Demo 原始目录不可用，沿用恢复生产基线。下一可执行任务为 R08-04 业务完成节奏。

## 当前第三轮 R08-02 热关闭动画

- `459de0de` 先复用已有入口：Dashboard 已监听 `plugin.VisualSettingsChanged` 与 `SystemParameters.StaticPropertyChanged`，侧栏已有关闭动效归一化；Settings 已监听系统参数和动画开关事件。本次补按 Dispatcher 隔离的弱引用动效登记、generation 失效和 `NormalizeAll`，并让 Settings 自身入场时钟在关闭动画后立即落到 opacity `1`/Y `0`；没有改变服务/DTO、命令绑定、选框、滚动条或安全语义。
- 当前身份输出 `r08-02-build-459de0de`、`r08-02-solution-build-459de0de` 均为 `0 warning / 0 error`，Playnite `net462`，XAML `24/24`；串行焦点 `21/21`：HotChange `1/1`、Reverse `2/2`、ProductionShell `10/10`、Foundation `8/8`。源校验和 diff check 通过。
- 实际 Settings WPF 行为确认关闭开关会释放 opacity/Y 时钟并保持稳定可见；重新开启等待原时长不重放旧入场。并行合跑曾出现 3 条 Foundation STA 时序抖动，未作为通过依据；WPF 退出 COM 诊断仍记录，vstest 退出码为 0。未验真实 Windows 偏好切换、真实 Playnite、物理输入/DPI/呈现/UIA/ETW/宿主性能。Demo 原始目录不可用，沿用恢复生产基线。下一可执行任务为 R08-03 离屏与隐藏停机。

## 当前第三轮 R08-01 中途反向连续

- 先核对已有实现：`AcrylicProductionShellView` 的侧栏开合已有 `sidebarTransitionGeneration`；发现通用 `GscMotion.AnimateTranslate` 的旧 `Completed` 回调可能覆盖最新目标。`a7aaa89e` 只补按元素 generation guard，`d62757e9` 稳定真实 STA WPF 取样，没有改变命令/绑定、游戏选框、滚动条、取消/错误/恢复语义或服务 DTO。
- 当前身份隔离 Release 输出 `r08-01-build-d62757e9` 与完整 solution 输出 `r08-01-solution-build-d62757e9` 均为 `0 warning / 0 error`，Playnite `net462`；XAML `24/24`；R08-01 `2/2`，相邻 `ProductionShellChromeSourceTests 10/10`、`UiFinesseFoundationTests 8/8`，源校验和 diff check 通过。Translate 实际取样 `8.403→8.403→-8`；侧栏 `169.333→169.333→221.333→270`，最终 opacity `1` 且时钟释放。
- 夹具检查了中点、有效值连续性、最新目标、最终状态和时钟释放，不是 Assert.Contains-only。边界仍是合成/fake、隔离 STA WPF/offscreen logical DIP；未验真实 Playnite、物理输入/DPI/跨屏、呈现/UIA/读屏、ETW、宿主性能或 OS reduced-motion。Demo 原始目录不可用，沿用恢复生产基线。下一可执行任务为 R08-02 热关闭动画。

## 当前第三轮 R07-08 resize 压力序列

- `9f478cac` 补真实生产 `AcrylicProductionShellView` + `TaskCenterView` 的隔离 STA WPF resize 夹具；复用 shell Render 优先级合并、独立详情 `ScrollViewer`、游戏选择器 overlay 和现有命令/绑定，没有换布局体系。序列为 `1366×900→960×700→960×560→1440×900→1366×900`，任务详情、游戏选择器、搜索焦点全程保持。
- 当前身份 shadow 构建 `r07-08-build-9f478cac`：XAML `24/24`、solution Release `0 warning / 0 error`、Playnite `net462`；`R07ResizeStressBehaviorTests 1/1`，R07 过滤回归 `14/14`，源校验和 diff check 通过。表格 ActualHeight `525.333/180/180/525.333/525.333`，Task MaxHeight 全为 `∞`；详情 MaxHeight 宽态 `∞`、紧凑/短窗 `160` 后恢复 `∞`；选择器 ActualHeight `122.667`，MaxHeight `401.333–774.667`。
- 测试只覆盖合成 DTO/fake、隔离 STA WPF/offscreen logical DIP；WPF 退出时的 `TextServicesContext.InvalidComObjectException` 已记录，vstest 退出码为 0。未验真实 Playnite 拖拽 resize、物理 DPI/跨屏、presented frame、UIA/读屏、ETW 或宿主性能。Demo 原始目录仍缺失；下一可执行任务为 R08-01 中途反向连续。

## 当前第三轮 R07-07 触控板小增量

- `98e8b244` 修正共享 `ScrollBoundaryRoutingBehavior` 的最终边界：内层/外层都不可滚动时收口无效滚轮事件，避免每个末端 no-op 触发一次布局；有可移动外层时仍按 delta 转发，正常小增量仍交给 WPF 原生处理。没有改游戏选框、滚动条、虚拟化、命令/绑定或安全语义。
- 当前身份隔离 shadow 构建 `r07-07-build-98e8b244`：XAML `24/24`、solution Release `0 warning / 0 error`、Playnite `net462`；`R07FineScrollBehaviorTests 2/2`，R07 过滤回归 `13/13`，源校验和 diff check 通过。小增量 offset `0→36`、反向 `33→27`、末端 `154/154`；可见行 `6–9`；20 个事件布局 `17`，末端 5 个 no-op 布局增量 `0`；加速边界外层 `32→80/432`、内层 `628`、单事件布局 `1`。
- 证据仅为合成数据、隔离 STA WPF/offscreen logical DIP；`-30/+120/-360` 是 routed wheel 夹具，不等价真实触控板/Playnite、物理 DPI/跨屏、UIA/读屏、presented frame、ETW 或宿主性能。Demo 原始目录仍缺失，沿用恢复生产基线；下一可执行任务为 R07-08 resize 压力序列。

## 当前第三轮 R07-06 状态横幅预算

- 当前测试提交 `3551de81` 已生成隔离身份一致输出 `.tmp\\r07-06-build-3551de81`：XAML `24/24`、solution Release `0 warning / 0 error`、Playnite `net462`；没有修改生产状态/服务/DTO、游戏选框或滚动条。
- `R07StatusBannerBudgetBehaviorTests 4/4` 实际覆盖 Task 失败保留旧数据、无数据失败、Save Stale、Maintenance Stale/安全模式；相邻空态 `2/2`、Task 响应 `7/7`、R06 详情 `2/2`、R07 滚动 `2/2`，提交身份合并 `17/17`。横幅重试/恢复命令可达，表格最小高度与实际 viewport 不归零，真实失败仍由错误 presenter 展示。
- 更宽相邻合跑仍有一条旧 `WorkspaceStateSourceTests` Media 源字符串契约失败和一条 intentional skip，未改写为通过；它们不属于本项。证据仅是合成 DTO/fake、隔离 STA WPF/offscreen logical DIP；Demo 原始目录缺失，下一可执行任务为 R07-07 触控板小增量。

## 当前第三轮 R07-05 详情断点稳定

- 当前提交 `0daca6f0` 已推送到 `codex/ui-finesse-round2`。四个生产页面复用 `ResponsiveDetailBreakpointLatch`：980 DIP 为内容预算基线，进入紧凑需 `<972`，回宽需 `>=988`，临界测量不重复切换侧栏/下方详情；详情控件、选中对象、焦点和原 `ScrollViewer` 保持。
- 隔离 `.tmp\\r07-05-build-0daca6f0`（构建身份与提交一致）：XAML `24/24`、solution Release `0 warning / 0 error`；断点协调器 `5/5`，Task 真实生产视图 STA 行为 `1/1`；滚动归属 `2/2`、Task 响应 `7/7`、详情预算 `2/2`，合并运行 `17/17`；源码校验和 diff check 通过。
- 证据使用合成 DTO/fake、隔离 STA WPF/offscreen logical DIP；未启动真实 Playnite，不宣称物理 DPI/跨屏、设备拖拽、UIA/读屏、presented frame、ETW 或宿主性能。Demo 原始目录仍缺失，沿用恢复生产基线；下一可执行小批量为 R07-06 状态横幅预算。

## 当前第三轮 R01-08 跳过测试说明复验

- 当前 HEAD 72be494d 的隔离输出 .tmp\r01-08-current-72be494d 已产出全部测试程序集；XAML 结构检查 24/24。Core 当前全量为 83/0/0，Playnite WorkerIpcClientBehaviorTests 为 7/0/0（其中 6 条 NamedPipe gated），Worker WorkerProcessRestartTests 为 1/0/0，源码清单确认 57 条 LegacyProductionUiBaselineFact。
- R01-08 只签收通过/失败/跳过分类与可执行 gated 补测，不把当前直接 Playnite 全量中观察到的非 R01 WPF 资源树、动画、布局和 R02/R06/R07 行为失败改成 skip 或绿色；这些边界留在各自任务证据中继续处理。
- 本项未修改生产 UI、服务/DTO、命令绑定、游戏选框、滚动条或安全语义。R02-01～R02-05 已有独立证据，R02-06 仍受宿主菜单 visual tree 阻塞；下一可执行小批量为 R07-05 详情断点稳定。真实 Playnite、物理 DPI、OS 输入/IME、presented frame、UIA/读屏、ETW 与宿主性能仍未验。

## 当前第三轮 R01-06 宿主证据保全

- 复用当前 R01-03 审计输出，在 `c2399d7be9f723e77226619172be16778fe3646f` 身份下更新 R01-06 归档：161 快照、80 条预期 INFO、0 Fidelity、0 失败路由、0 HIGH/0 MEDIUM；E01～E20 索引校验 `20/20`。
- 归档报告、metadata、manifest、route/fidelity/layout、EVIDENCE_INDEX 和 6 张精选图已更新；完整 362 张截图不入库，README 给出独立 clone 的 build/restore/harness/audit 重现命令。metadata 不再含机器绝对输出路径，ZIP 只记为可再生临时输出且未保留。
- 本项没有修改生产 UI、服务/DTO、命令绑定、游戏选框、滚动条或安全语义。下一小步是 R01-07 把本次 `c2399d7b` 归档身份写入 freshness baseline，再推进 R01-08；真实 Playnite、物理 DPI、OS 输入/IME、presented frame、UIA/读屏、ETW 与宿主性能仍未验。

## 当前第三轮 R01-05 负例注册表复核

- 当前提交 `810114e2` 新建 `.tmp\r01-05-build-810114e2`，XAML `24/24`、solution Release `0 warning / 0 error`；`UiNegativeFixtureRegistryTests` 在构建绑定源码根下 `1/1` 通过，`python scripts/validate-source.py` 通过。
- N01–N05 不是字符串登记：当前 detector 逐项返回 `detected=True`。N01 对比度 violation=1；N02 长负数横向失败/纵向通过且不可读；N03 无可见候选 Enter 不处理并保留旧选择；N04 隔离 gate 生成 `CHILD_LAYOUT_OVERFLOW`；N05 Loading presenter 屏蔽底层命中。
- 本批只重跑测试侧既有注册表，没有修改生产 UI、服务/DTO、命令绑定、游戏选框、滚动条或安全语义。R01-05 账本恢复“已满足”；R01-06 因 freshness 命中 RenderHarness Program 仍待重跑。
- 证据仍是合成夹具、隔离 STA WPF/offscreen logical DIP；真实 Playnite/OS 输入/IME/物理 DPI/presented frame/UIA/ETW/宿主性能未验。下一项先做 R01-07 baseline 增量更新，再处理 R01-06。

## 当前第三轮 R01-07 freshness 基线复核

- 版本化 `UI_EVIDENCE_BASELINE.json` 已把本批实际重跑的 R00-01-02、R00-03、R00-04、R00-05、R00-06、R00-07、R00-08、R01-01、R01-02、R01-03 绑定到各自当前隔离证据身份；没有把未重跑的旧证据强行标 fresh。
- `test-ui-evidence-freshness.ps1` 的文档-only、共享 `Redesign.xaml`、合成 package identity 三类 smoke 均通过。扫描时源码 `c3e67cb4` 的 14 条记录为 `14 fresh / 0 stale`；R01-06 当前归档身份已绑定 `c2399d7b`。
- 当前包身份仍为 `not-provided`，不宣称 package-host 安装或真实 Playnite；全 fresh 只表示 baseline/sourcePaths 未命中新的非文档变更，不扩大视觉/交互/性能结论。
- 下一可执行任务为 R01-08 跳过测试说明；本阶段继续保留 Demo 原始目录缺失、offscreen logical DIP 和真实宿主/UIA/物理呈现/ETW/性能未验边界。

## 当前第三轮 R01-03 每项证据直达

- 当前提交 `c2399d7b` 已用新隔离构建 `.tmp\r01-03-build-c2399d7b` 复核：solution/RenderHarness Release `0 warning / 0 error`、XAML `24/24`；完整受控审计 `.tmp\r01-03-audit-c2399d7b` 的身份为完整 `c2399d7be9f723e77226619172be16778fe3646f`。
- 审计实际为 161 个运行时快照、80 条预期 INFO、0 Fidelity、0 失败路由、0 HIGH/0 MEDIUM；`validate-ui-evidence-index.ps1` 对 E01～E20 输出 `rows=20, references=20/20, identities=20/20, samples=20/20, boundaries=20/20`。索引同时区分运行时 DataGrid 与仅静态 manifest 的条目，不把索引完整性写成视觉/交互全量签收。
- freshness 工具按旧版本化 baseline 仍将 R01-03 标为需重跑，因为 `Program.cs` 自历史身份 `9d5146d` 后变化；本批已经用 `c2399d7b` 完成该重跑，R01-01/R01-02 也完成当前提交复核。R01-07 后续需单独更新 baseline 扫描事实，不能静默改旧 JSON。
- Demo 原始目录仍缺失，当前审计沿用恢复生产基线；证据来自合成数据、实际 WPF 视图、隔离窗口和 offscreen logical DIP。未启动真实 Playnite，不宣称物理 DPI、OS 输入/IME、presented frame、UIA/读屏、ETW 或宿主性能；未执行真实业务写入。下一执行项为 R01-07 freshness 基线复核，再视结果推进 R01-08。

## 当前第三轮 R01-01 / R01-02 收口

- 当前代码提交 `a5219c09` 已推送到 `codex/ui-finesse-round2`。先用旧默认 `bin\Release` 复跑时，R01-01 身份门禁正确拒绝了程序集 `447ac07e` 与当前源码不一致；在当前隔离构建中又定位到 `R06EmptyStateBehaviorTests.cs` 的残留 `Environment.CurrentDirectory` 回溯，现已改为 `TestRepositoryContext.Root`。这只修测试源码根绑定，没有把 main 的旧实现带入当前分支，也没有改生产空表逻辑。
- 新建 `.tmp\r01-01-02-build-clean-a5219c09` 后，Release solution/RenderHarness 均 `0 warning / 0 error`、XAML `24/24`；`RepositoryIdentityTests + UiFinesseFoundationTests + UiAuditSourceTests` 为 `16/16`，`NumericCellReadabilityTests` 为 `2/2`。
- Light/Dark 当前 `finesseprobe` 报告分别保留在 `.tmp\r01-02-finesse-clean-a5219c09-light` 与 `...-dark`：均 `WorkingTreeClean=True`、`expected=4 realized=4 horizontalFit=4 verticalFit=4 allReadable=True`、contrast violations `0`；窄列负例保持 `HorizontalFit=False / VerticalFit=True / must-fail=passed`。生产数值列能力复用 `acfe1ea`，本批没有重建业务实现。
- R01-01/R01-02 账本已更新为“已满足”。证据仍是合成/fake、隔离 STA WPF/offscreen logical DIP；未启动真实 Playnite，不把离屏结果写成物理 DPI、presented frame、OS 输入/IME、ETW 或宿主性能验证。Demo 原始目录仍缺失，沿用恢复生产基线；游戏选框、滚动条、命令/Binding、取消/错误、恢复保护、有限列表和 net462 未改。
- 下一可执行小批量为 R01-03“每项证据直达”：复用现有索引/校验脚本，先验证当前分支的 20 行索引、报告入口、源码身份、样本和边界，并补断链/错链负例；本批新建的隔离输出中未被文档引用的旧目录仍待清理。

## 当前第三轮 R00-07 / R00-08 证据校正

- 当前 HEAD `4f585024` 用隔离 `.tmp/r00-07-08-build-clean` 重建后复核：R00-07 审计源/响应式契约 `30/30`，RenderHarness `0/0`；R00-08 生产选框键盘/焦点/搜索测试 `31/31`。
- clean `toolbarprobe` 三场景通过：正常长表单按用途排除并记录理由，同祖先超宽动作栏命中 `TOOLBAR_HORIZONTAL_OVERFLOW`，同祖先不可达动作栏命中 `TOOLBAR_UNREACHABLE`；没有按字符串断言签收。
- R00-08 实际生产 `AcrylicProductionShellView`/WPF Window 路由覆盖无结果 Enter、活动 composition Enter、IME/方向键、可见候选 Enter、Esc/清除焦点回返；当前可控条件已满足。真实 OS IME、Playnite、物理输入/呈现/性能未验。
- Demo 原始目录仍缺失，沿用恢复生产基线；游戏选框、滚动条、命令/Binding、取消/错误、恢复保护、有限列表和 net462 未改。下一实际批次转入 R01-01/R01-03 证据与身份校正。

## 当前第三轮 R00-05 / R00-06 证据校正与夹具修正

- 旧默认 `bin\Release` 的一次 R00-05/R00-06 复跑被 R01-01 身份门禁正确阻止（程序集 `447ac07e`、源码根为当前提交）；按源码根绑定协议改用 `.tmp/r00-05-06-build-clean` 后定向集合 `6/6` 通过，未绕过旧根。
- `e8fe1aab` 只修正 RenderHarness 的备用密度夹具：列级 HeaderStyle 与行 Style 同时覆盖，Light/Dark `mediageometryprobe` 五场景均 `OK`，alternate-density 实测 `36/44`；生产 `42 DIP MinHeight`、滚动条和页面结构未改。
- clean-tree 证据：`shellqa OK`（Media 1040/1100/1366 DIP）；完整审计 161 快照、80 分类 INFO、0 Fidelity、0 失败路由、0 HIGH、0 MEDIUM。R00-05/R00-06 账本已改为已满足。
- 证据仍限于合成媒体 DTO/fake、隔离 STA WPF/offscreen logical DIP；INFO 是可解释滚动上下文，不是缺陷清零，亦不等价真实 Playnite、物理 DPI、设备输入、presented frame、ETW 或宿主性能。下一小批量为 R00-07/R00-08 或对应 R01 校正。

## 当前第三轮 R00-03 / R00-04 证据校正

- 当前 HEAD `e5a12ff` 复核既有动效与搜索基准实现：R00-03 的实际 WPF Dispatcher 行为 `3/3`，R00-04 的 2,000 项搜索基准与有限时间负例 `2/2`；当前隔离 Release RenderHarness 构建 `0 warning/0 error`。
- `motionreentryprobe` Light/Dark 均从当前渲染宽度接管到 `270 DIP`，最终 `X=0/finalAnimated=False`；`motionhotprobe` 两主题均捕获活动中间态，关闭动效后归一到 `72/Opacity=1`，禁用重入立即完成。
- R00-04 当前 `.tmp/r00-03-04-test-artifacts/ui-qa/benchmarks/large-library.txt` 保留 `30` 个不同查询、`changed_result_sets=30`、原始 `p50/p95/max=46/47/48ms` 和 `75ms` 不可能结果负例；账本已改为已满足。R18-01 仍覆盖连续输入、IME、20ms debounce 的过滤次数/分配。
- 证据仍是合成数据、隔离 STA WPF/offscreen logical DIP，不等价真实 Playnite 输入、Windows 偏好通知、物理 DPI/呈现帧、ETW 或宿主性能；Demo 原始目录仍缺失，沿用恢复生产基线。下一小批量转入 R00-05/R00-06 或其证据校正。

## 当前第三轮 R00-01 / R00-02 证据校正

- 当前 HEAD `89aa27e1` 复核既有按压合成和组合缩放实现，没有重建生产功能；定向 Playnite WPF 行为测试 `4/4`，隔离 Release XAML `24/24`、解决方案 `0 warning/0 error`、RenderHarness `0/0`。
- Light/Dark 当前 HEAD `finesseprobe` 均 `finesse-fixture OK`：88 个 normal/hover/focus/pressed 组合 0 violation；非等距 stop 与黑底灰背景负例通过；1000 次组合变换树节点/深度稳定、控件实例隔离；NumericReadability 4/4 可读，窄列负例按预期失败。
- 证据报告为 `.tmp/r00-01-02-current-built/ui-finesse-fixture-report.txt`（当前隔离构建生成，`WorkingTreeClean=True`、offscreen `DpiScale=1.00`）；R00-01/R00-02 账本已改为已满足。真实宿主按压/屏幕帧、物理 DPI、可变 Freezable 跨控件共享仍未验，后者留给 R08-08。
- Demo 原始目录仍缺失，继续沿用恢复生产基线；游戏选框、滚动条、命令/Binding、取消/错误、恢复保护、有限列表和 net462 未改。下一小批量继续处理 R00-03/R00-04 或其现有证据校正。

## 当前第三轮 R07-04 横向滚动端点

- `8f682fcb814a648de497728b17ebc870a13187aa` 先核对现有生产 DataGrid 模板和横向滚动系统，只新增隔离 RenderHarness 的 `horizontalprobe`；没有改生产 XAML、滚动条、游戏选框、命令绑定或服务。
- Light/Dark×Save/Task/Media/Maintenance 共 8 组合均通过：负/超最大水平 offset 夹断、末列/末单元格完整落入 viewport、横向条不遮挡第一行、回到左端无漂移。clean 报告 `.tmp/r07-04-horizontal-clean/horizontalprobe-report.txt` 绑定 `WorkingTreeClean=True`、`DpiScale=1.00`；Task/Media 实际右端图已抽查。
- Media 仍为 `Standard/Item/EnableColumnVirtualization=False`，只使用合成 60/6/8 条数据；XAML `24/24`、Release `0 warning/0 error`、RenderHarness `0/0`。Demo 原始目录仍缺失，继续以恢复生产基线为准。
- 边界仍是合成 DTO/fake、隔离 STA WPF/offscreen logical DIP；未验真实 Playnite/设备输入、Ctrl/Shift/IME、UIA/读屏、物理 DPI/跨屏、presented frame、ETW 或宿主性能，未写真实用户数据。下一实际批次按用户要求转入 R00/R01 问题复核与证据校正；账本下一布局项为 R07-05。

## 当前第三轮 R07-03 短窗底栏可达

- `d19e848b`、`447ac07e` 先核对最新生产实现：`AcrylicProductionShellView` 的 `FooterSurface` 已在固定 36 DIP 底栏行，`PageHost` 在主内容行；本阶段没有重建底栏、页面 ScrollViewer、DataGrid/详情滚动、游戏选框或滚动条系统，只新增隔离 RenderHarness 的实际 WPF 短窗探针和合成 `MediaInboxPageHasMore` 状态。
- `shortwindowprobe` 在 Light/Dark、1040×700/1040×560 共 16 个组合中检查实际 shell/page/inner surface 几何和滚动状态：Media 末端 `LoadMore`、Save `保存策略`、Task 详情 `取消任务`、Maintenance 云端 `加载更多` 均可达，提示条与固定 footer 不互相覆盖；报告末尾为 `shortwindowprobe OK`。Save 探针还保留了“最大滚动值不等于按钮可达”的首次负例修正，改用真实按钮 `BringIntoView`。
- 相关回归 `20/20`、XAML `24/24`、Release `0 warning/0 error`、`validate-source.py`、`git diff --check` 通过；clean `.tmp/r07-03-short-window/shortwindowprobe-report.txt` 绑定 `447ac07e...`、`WorkingTreeClean=True`。全套 Playnite 本次观测 `578/695 passed`、`60 failed`、`57 skipped`，失败未用于 R07-03 绿灯结论。
- 证据只覆盖合成数据、fake、隔离 STA WPF/offscreen logical DIP；Demo 原始目录仍缺失，沿用恢复生产基线。真实 Playnite/Worker、设备输入、UIA/读屏、物理 DPI/跨屏、presented frame、ETW、宿主性能和真实服务失败时序仍未验，未写真实用户数据。下一项为 R07-04 横向滚动端点。

## 当前第三轮 R07-02 锚点删除回退

- `7acb61a5976196c7e5add347920762912e1149ea` 新增共享 `SelectionAnchorResolver`，并接入 Task 全量/分页、Media 主库/Inbox、Findings、进程映射、云端队列、Save 历史/候选。刷新前记录稳定键与旧索引；稳定键不存在时按旧索引夹到邻近项，不跳首行；任务导航目标仍优先，云端一致性分页仍保留 pending key，Save 候选仍保留既有 Pending/首项初始化。
- `ef53a7486d0c5cc5111d7f302d184fbe1a1f201d` 补 Save 候选稳定路径删除后的邻近回退负例。`R07SelectionAnchorBehaviorTests 4/4`，最终相邻回归 `20/20`、0 skipped；清理 Release 输出后标准构建 XAML `24/24`、编译 `0 warning/0 error`、源码校验/diff check 通过。
- clean `.tmp/r07-02-anchor-final/render-qa-report.txt` 绑定 ef53a748、`WorkingTreeClean=True`、Light/Dark、多尺寸/滚动/resize、50/400/2000/4468 合成数据量 `render-qa OK`；1040×700 Save/Media/Maintenance/Task 为 `4/4`、`6/4`、`5/4`、`5/4`，1366 Task `4/4`。已查看 Task/Media/Maintenance 代表图。
- 当前仍仅证明合成 DTO、生产共享恢复器、实际 STA WPF DataGrid 与 offscreen logical DIP；Demo 原始 `DesignShellView.xaml`/`Pages` 不存在，沿用恢复生产基线。真实 Playnite/Worker、后端刷新竞态、设备输入、UIA/读屏、物理 DPI/跨屏、presented frame、ETW、宿主性能未验；未写真实存档、媒体、云端或诊断数据。下一项为 R07-03 短窗底栏可达。

## 当前第三轮 R07-01 滚动所有权

- `9c878b9cb9a8cd07075ede332c9534e1d957f6b3` 先核对既有页面主滚动、DataGrid 模板 `DG_ScrollViewer` 和 Inspector 详情滚动；保留 `CanContentScroll`、虚拟化、水平/垂直滚动条和游戏选框。当前 checkout 没有原始 Demo `DesignShellView.xaml`/`Pages`，沿用恢复生产基线。
- 新增共享 `ScrollBoundaryRoutingBehavior`，接入 `GscPageScrollViewer`/`GscInspectorScrollViewer` 和 `GscRedesignWorkspaceDataGrid`。内层还能滚时不抢事件；到方向边界才按滚轮增量转给最近可滚动外层，DataGrid 从实际模板内部 ScrollViewer 判定，不在外层与表格间同步跳动。移除未接线且固定三行的旧 Dashboard 处理器；源码门禁迁移为检查共享行为与两个样式的真实接线。
- `R07ScrollOwnershipBehaviorTests 2/2`：实际 STA WPF 嵌套 ScrollViewer 上/下边界和实际 DataGrid → 页面边界转发；最终提交重建后相邻 Task 响应式、Media 四行、详情 disclosure、R06 详情合计 `14/14`。XAML `24/24`、Release 编译 `0 warning/0 error`、源码校验/diff check 通过。
- clean `.tmp/r07-01-scroll-final/render-qa-report.txt` 绑定该 SHA，`WorkingTreeClean=True`、Light/Dark、多尺寸与 resize `render-qa OK`；1040×700 Save/Media/Maintenance/Task 至少四行，1366 Task `4/4`；Task/Media/Maintenance 代表图已查看。
- 边界：真实 Playnite/Worker、设备滚轮/触控板轨迹、UIA/读屏、OS 输入/IME、物理 DPI/跨屏、presented frame、ETW、宿主性能和主题额外 routed event 未验；未写真实存档、媒体、云端或诊断数据。下一项为 R07-02 锚点删除回退，先检查刷新/删除/筛选/加载更多的稳定 ID 视口恢复。

## 当前第三轮 R06-08 详情与行高预算

- `07376adb1d52a394137853896efe1b4589f983d1` 先核对已有 Task/Media/Save/Maintenance 详情区、选中绑定、独立 ScrollViewer 和紧凑行高预算；Task 长诊断已在详情区/折叠技术详情中展开，未扩张所有列表行。唯一发现的行为缺口是 Save 刷新后候选总是优先回到第一个 Pending 项，因此新增按 `PlayniteId + Path` 恢复原候选，找不到时才回退 Pending/首项。
- `R06DetailsBudgetBehaviorTests 2/2` 实际 STA WPF 生产 `TaskCenterView` 验证第一项切换到带长诊断的第二项后详情对象正确、技术详情可展开、详情滚动可用、列表仍保留行高预算；同一测试也验证 Save Accepted/Rejected 候选刷新后的稳定选择恢复。相邻 `TaskCenterViewResponsiveTests` + `MediaInboxGeometryTests` + `DetailsDisclosureSourceTests` `10/10`。
- Release XAML `24/24`、编译 `0 warning/0 error`、源码校验与 diff check 通过。clean `.tmp/r06-08-render-final/render-qa-report.txt` 绑定该 SHA，`WorkingTreeClean=True`，Light/Dark、1040/1100/1366/2560 DIP 与 resize 均 `render-qa OK`；Task 最窄窗口 `4/4` 可读行，紧凑详情 `160 DIP`，宽布局独立 `360×516` 详情侧栏；Media/Maintenance/Save 也保留至少四行门禁。
- 已查看 `Task-1040x700.png`、`Task-1366x768.png`、`Save-1040x700-tab1.png`；Demo 原始 `DesignShellView.xaml`/`Pages` 当前 checkout 仍不存在，继续沿用恢复生产基线，保留游戏选框、滚动条、命令/Binding、取消/错误/恢复保护、有限列表和 net462。
- 边界：证据为合成 DTO、fake/probe、隔离 STA WPF 和 offscreen logical DIP；未验真实 Playnite/Worker 时序、UIA/读屏、OS 输入/IME、物理 DPI/跨屏、presented frame、ETW、宿主性能或真实服务失败时序，未写真实存档、媒体、云端或诊断数据。下一项为 R07-01 滚动所有权，先盘点多层 ScrollViewer 的所有权与事件边界。

## 当前第三轮 R06-07 空表保留结构

- `5046bf8f30920065660d38ea03613edb1ea7aaa4` 先核对状态覆盖：Task、Media、Maintenance 已有首次空、筛选/已处理完、加载中、失败和旧数据降级 presenter；Save 原先只有 `IsBusy + Count == 0`，读取失败会落入空文案，因此只补 Save 的状态边界，没有重建已有页面。
- Save 详情请求现在区分 Loading、Empty、Ready、Error 和已有数据失败时的 Stale；历史/候选 DataGrid 表头、列宽、排序、滚动和 `LoadDetailsCommand` 保持。加载中隐藏空文案，首次历史为空与候选处理完成/本次扫描无新结果分开表达，失败提供可重试 presenter，旧数据失败保留行并显示降级提示。
- `R06EmptyStateBehaviorTests 2/2` 使用真实 `SaveCenterView`、生产绑定和合成状态验证 7 个历史表头、4 个候选表头、Loading/Empty/Ready/Error、错误优先级及重试命令；RenderHarness fake 已同步状态绑定。
- Release XAML `24/24`、构建 `0 warning/0 error`、`WorkspaceStateSourceTests 9 passed / 1 intentional legacy skip`、`TaskCenterViewResponsiveTests 7/7`、`R06TaskProgressBehaviorTests 4/4`、源校验和 diff check 通过。
- clean `.tmp/r06-07-emptytables/emptytables-report.txt` 绑定 `5046bf8f...`，`WorkingTreeClean=True`，Light/Dark、1040×700/1600×900 `emptytables OK`；Save/Task/Media/Maintenance 结构保留，Save 明暗代表图已查看。
- 边界：合成数据、fake 服务、隔离 STA WPF 和 offscreen logical DIP；未验真实 Playnite/Worker 时序、UIA/读屏、OS 输入/IME、物理 DPI/跨屏、presented frame、ETW 或宿主性能；未写真实存档、媒体、云端或诊断数据。下一可执行任务为 R06-08 详情与行高预算，先核对现有详情区、选中对象同步和四行门禁。

## 当前第三轮 R06-06 行内进度稳定

- `72fd6d9aeb74890b86ec11b2ef8e4d3954546803` 先复核既有 `TaskStatusDto.ProgressPercent/ProgressValue/ProgressDisplay`、`SnapshotComparers.Task`、`TaskIndexedCollection.Merge` 和任务分页 `TaskSummary`；实时事件更新已有行只走索引器 `Replace`，不是全表 `Reset`，分页总数仍来自服务端汇总，不由当前可见页倒推。
- `TaskCenterView.xaml` 新增局部 `TaskCancellationStatusText`：绑定既有 `IsCancellingTask`，取消请求等待期间显示“正在取消…”，另有兼容 net462 的 Automation HelpText；没有改任务协议、取消确认、Worker 请求、终态或滚动模型。游戏选框、滚动条、命令/Binding、错误/恢复语义、有限列表和 Playnite/net462 保持。
- `fd9326d7` 补齐 `R06TaskProgressBehaviorTests` 的真实生产 TaskCenter 取消提示可见性行为；当前定向 `4/4`，另有 TaskIndexed `4/4`、Batch `3/3`、R03 数值 `10/10`、R06 选中焦点 `2/2`、排序 `4/4`。
- Release XAML `24/24`、编译 `0 warning/0 error`、`validate-source.py`、`git diff --check` 通过。clean RenderHarness `.tmp/r06-06-render-final/render-qa-report.txt` 绑定生产代码 `72fd6d9a`，双主题 357 PNG、任务页多尺寸/滚动/虚拟化/resize、`WorkingTreeClean=True`、`render-qa OK`；Light/Dark Task 1040×700 已抽查。
- 边界：合成任务、隔离 STA WPF 和 offscreen logical DIP，不等价真实 Playnite 长任务/取消竞争、UIA/读屏、物理 DPI/跨屏、presented frame、ETW 或宿主性能；未写真实存档、媒体、云端或诊断数据。下一可执行任务为 R06-07 空表保留结构。

## 当前第三轮 R06-05 列头说明

- `7527e6381e531331d658166f0917487c03a0e7` 先核对已有共享 `GscDataGridHeaderTextTemplate`、22 DIP 排序箭头槽、透明列宽拖拽 Thumb 和容量格式化路径；新增 `DataGridColumnHeaderHelpBehavior`，说明挂在 `DataGridColumn` 上，生成 header 将说明同步到 Tooltip 与 `AutomationProperties.HelpText`。
- Save History/Candidates、Task Queue、Media Inbox、Maintenance Findings 的时间、数量/百分比、容量、路径、类型/来源、状态/等级和详情摘要列均有短说明；Header 保持原始字符串，不插入按钮或独立点击路由，因此排序、键盘导航、列宽拖拽和 `DataGridStableSortController` 保持。
- `B/KiB/MiB/GiB` 1024 进制单位复用既有 DTO 格式化逻辑；共享表头继续 `Wrap + TextTrimming=None`，长表头可换行而不遮挡排序箭头。原始 Demo `DesignShellView.xaml`/`Pages` 当前 checkout 仍不存在，继续沿用恢复生产基线。
- `R06ColumnHeaderHelpTests 2/2`；R06-04 复制 `3/3`；R06-03 选中焦点 `2/2`；R06-02 排序 + R06-01 列宽 `9/9`；Release XAML `24/24`、编译 `0 warning/0 error`、源校验与 diff check 通过。
- clean RenderHarness `.tmp/r06-05-render-final/render-qa-report.txt` 绑定该代码 SHA，Light/Dark 357 PNG、生产主要表格 header contract `resize=true/sort-arrow=visible`、50/400/2000/4468 数据量、滚动/虚拟化和 2560×1440 → 1100×720 → 2560×1440 resize，`WorkingTreeClean=True`、`render-qa OK`；Save/Task/Maintenance 代表图已抽查。
- 边界：证据使用合成 DTO、隔离 STA WPF 和 offscreen logical DIP，不等价真实 Playnite 悬停/排序/拖拽、UIA/读屏、物理 DPI/跨屏、presented frame、ETW 或宿主性能；未写真实存档、媒体、云端或诊断数据。下一可执行任务为 R06-06 行内进度稳定。

## 当前第三轮 R06-04 复制单元格与整行

- `afe4aa55a24e306bbe219da1ce76d8675325c35e` 先核对并复用既有 `CopyPathCommand`、诊断/错误/维护报告复制入口、`CopyTextWithRetryAsync` 和五类生产 DTO；新增共享 `DataGridClipboardBehavior`，为 Save History/Candidates、Task Queue、Media Inbox、Maintenance Findings 提供显式复制 profile。
- `Ctrl+C` 复制当前显示顺序的 Extended 选中行，`Ctrl+Shift+C` 复制当前单元格；稳定 ID 去重，TSV 使用 TAB/CRLF 和明确转义，技术字段保留完整原值；密码、token、secret、API key、Authorization Bearer 等凭据统一输出 `[已隐藏]`。公共文本写入点同步脱敏，未复制隐藏凭据。
- 保留 FullRow/滚动/虚拟化、游戏选框、命令/Binding、取消/错误/恢复保护、有限列表和 net462；没有从 main 带入旧实现。原始 Demo `DesignShellView.xaml`/`Pages` 当前 checkout 仍不存在，继续沿用恢复生产基线。
- `R06ClipboardBehaviorTests 3/3`；R06-03 选中焦点 `2/2`；R06-02 排序 + R06-01 列宽 `9/9`；Release XAML `24/24`、编译 `0 warning/0 error`、源校验与 diff check 通过。全量 Playnite 合跑观察到 `573/679` 通过、`57` 跳过、`49` 既有 WPF 环境性失败，未计为绿色。
- clean RenderHarness `.tmp/r06-04-render-final/render-qa-report.txt` 绑定该代码 SHA，Light/Dark 357 PNG、50/400/2000/4468 数据量、滚动/虚拟化和 2560×1440 → 1100×720 → 2560×1440 resize，`WorkingTreeClean=True`、`render-qa OK`；Task/Media/Maintenance 代表图已抽查。
- 边界：证据使用合成 DTO、隔离 STA WPF、验证 seam 和 offscreen logical DIP，不等价真实 Playnite 选择、OS 剪贴板、UIA/读屏、IME、物理 DPI/跨屏、presented frame、ETW 或宿主性能；未写真实存档、媒体、云端或诊断数据。下一可执行任务为 R06-05 列头说明，先盘点表头单位、Tooltip/Automation 和可复用资源。

## 当前第三轮 R06-03 选中焦点区分

- `d83c73785c15b72af11c6b8897f359129ba66c26` 收口共享 `ListBoxItem` 与 `DataGridRow` 状态：活动选中、键盘当前、悬停、失焦选中和 `TaskState.Failed` 错误行各有独立 surface/outline；键盘当前使用 2 DIP accent outline，失焦选中使用 `GscSelectionInactiveBrush`/muted border，失败行复用 `GscErrorTintBrush`/`GscErrorBrush`。DataGridCell 选中内容面保持透明，Media Inbox 删除会覆盖共享状态的本地 hover/selected trigger。
- 先核对已有能力后只修共享生产资源和真实行为门禁，没有带入 main 旧实现；游戏选框、滚动条、命令/Binding、取消/错误/安全恢复、有限列表和 net462 未改。当前 checkout 没有原始 Demo `DesignShellView.xaml`/`Pages` 目录，本阶段记录事实并沿用恢复的 `DesignTokens.xaml`/`WpfUiProduction.xaml` 生产基线。
- `R06SelectionStateBehaviorTests 2/2`；`WpfUiResourceDictionaryTests 137/137`（39 skip、0 fail）；R06-02 排序 `4/4`、R06-01 列宽 `5/5`；Release XAML `24/24`、编译 `0/0`、源校验和 diff check 通过。最终 RenderHarness `.tmp/r06-03-render-final/render-qa-report.txt` 绑定该 SHA，双主题 357 PNG、`WorkingTreeClean=True`、`render-qa OK`；Task/Media/Save 代表图已抽查，覆盖滚动、多数据量和 2560×1440 → 1100×720 → 2560×1440 resize。
- 证据见 [`R06-03-SELECTION-FOCUS-20260918.md`](../design/reviews/ui-finesse-round3-20260915/evidence/R06-03-SELECTION-FOCUS-20260918.md)。边界：行为与视觉均为合成 DTO、隔离 STA WPF 和 offscreen logical DIP，不等价真实 Playnite 鼠标/键盘/UIA/读屏、物理 DPI/跨屏、IME、presented frame、ETW 或宿主性能；未写真实存档、媒体、云端或诊断数据。下一可执行任务为 R06-04 复制单元格与整行。

## 当前第三轮 R06-02 排序提示与稳定性

- `c577afc587a63acd7c8fbe73183812f574008f7d` 新增共享 `DataGridStableSortController`，复用现有 DataGrid、DTO、绑定和 `WpfUiProduction.xaml` 的 SortDirection 箭头。Save History/Candidates、Task Queue、Media Inbox 绑定原始时间/数字/枚举/文本值，稳定次键分别为 BackupId、Path+PlayniteId、TaskId、MediaId；默认时间/置信度降序，其余按 profile 默认方向。
- 未知值由统一比较器在升、降序均放末尾：缺失时间/文本、非法数值、Task 负进度和排队中 0、Media 未知枚举均有固定规则。真实列头点击路径与行为夹具共用切换逻辑，同列升降序会同步 `DataGridColumn.SortDirection`；刷新清空后以乱序重加仍保持稳定次键顺序。游戏选框、滚动条、命令/Binding、取消/错误、安全恢复、有限列表和 net462 保持，Maintenance 表格本批不新增排序 profile。
- `R06SortingBehaviorTests 4/4`、最终 Release XAML `24/24`、编译 `0/0`、源码校验和 diff check 通过；R06-01 独立回归 `5/5`。组合 WPF vstest 产生的 7 项失败是同一 AppDomain/Application、隐藏 Window/布局生命周期环境性问题，未计为绿色。
- clean RenderHarness `.tmp/r06-02-render-final3/render-qa-report.txt` 绑定 `c577afc5...`，双主题、357 PNG、`WorkingTreeClean=True`、`render-qa OK`；列头 contract 报告 Save 7、Task 6、Media 5 的 `sort-arrow=visible`，Save/Task/Media `1040×700` 图已抽查，覆盖滚动、多数据量和窄窗恢复。
- 边界：证据使用合成 DTO、隔离 CollectionView、STA WPF、隔离构建目录和 offscreen logical DIP；未验真实 Playnite 点击/多线程刷新录像、UIA/读屏、OS 输入/IME、物理 DPI/跨屏、presented frame、ETW、宿主性能；未写真实存档、媒体、云端或诊断数据。下一可执行任务为 R06-03 选中焦点区分。

## 当前第三轮 R06-01 列宽用户记忆

- `75a6e6d826e39fe5a7f06438d7aefd1985ad1692` 在不覆盖 main 旧实现的前提下，复用现有 Playnite 设置保存链新增 `DataGridColumnLayoutController`。Save History/Candidates、Task Queue、Media Inbox 分别使用稳定 `v1/{view}/{column}` 键保存有效 Pixel 列宽；初始化/响应式默认布局不误写，Star 宽度不被当作用户偏好，卸载 flush，四处均有“重置列宽”。游戏选框、滚动条、命令/绑定、取消/错误、安全恢复、有限列表和 net462 保持。
- `R06ColumnWidthPersistenceBehaviorTests` 实际 `5/5`：控制器重建恢复且视图区隔离、`v0`/未知键忽略、最小宽度约束、重置删除当前键并持久化、窄 WPF Window 主列与横向滚动条负例/接线均通过。clean Release XAML `24/24`、编译 `0/0`，源码校验和 diff check 通过；R05 独立回归 Popup `1/1`、Tooltip `1/1`、边界 `2/2`、焦点 `3/3`、开关 `1/1`、选项虚拟化 `3/3`、源契约 `24/24`。
- clean RenderHarness 绑定完整 SHA，Light/Dark、357 PNG、`WorkingTreeClean=True`、`render-qa OK`；报告覆盖 50/400/2000/4468 数据量、纵横向滚动和 2560×1440 ↔ 1100×720 resize 恢复。Save/Task/Media `1040×700` 图已抽查。证据见 [`R06-01-COLUMN-WIDTH-PERSISTENCE-20260918.md`](../design/reviews/ui-finesse-round3-20260915/evidence/R06-01-COLUMN-WIDTH-PERSISTENCE-20260918.md)。
- 边界：恢复证据是隔离设置对象上的控制器销毁/重建，不等价真实 Playnite 进程重启、用户拖拽录像或宿主配置迁移；未验真实宿主、UIA/读屏、OS 输入/IME、物理 DPI/跨屏、presented frame、ETW、宿主性能。Maintenance DataGrid 本批仅参与既有回归，不纳入列宽记忆。未写真实存档、媒体、云端或诊断数据。下一可执行任务为 R06-02 排序提示与稳定性。

## 当前第三轮 R05-08 弹层资源热切换

- `599a8fd96f2808f5879f7eee7dd1d564e054db8a` + `bebde2fed18f3ad891d6fe4123c6b5a6757b96fd` 收口实际生产 Settings Popup/Tooltip 的主题与生命周期：Light→Dark 时两个附属层保持打开并使用活动资源，父窗 `Hide()` 后视图不可见且两层关闭；`GscToolTipBehavior` 仅做局部范围同步，卸载时解除显式 Tooltip 处理器，不改全局静态资源订阅。
- `AdaptiveThemePalette` 为生产 `AcrylicReferenceControls.xaml` 仍消费的旧 Acrylic 键补齐活动 Demo 主题别名，修复脱离页面视觉树的 Tooltip 在浅色主题回退静态深色资源的真实缺口；命令/Binding、游戏选框、滚动条、取消/错误、安全/恢复和 net462 保持。
- 最终标准产物 XAML `24/24`、编译 `0/0`；隔离进程定向 R05-08 `1/1`、Tooltip `1/1`、Popup `2/2`、焦点 `3/3`、开关 `1/1`、选项虚拟化 `3/3`、源契约 `24/24`。实现提交上的完整记录为 Core `83/83`、Worker `311/311`、Playnite `573/665`、57 跳过、35 条并行 WPF 环境性失败；最终夹具提交后没有把全量并行结果改写成绿色。
- clean RenderHarness 绑定最终 SHA，Light/Dark、357 PNG、`WorkingTreeClean=True`、`render-qa OK`；Settings 1040×700 开面、Popup/Tooltip 双主题图已抽查。报告 `.tmp/r05-08-render-clean/render-qa-report.txt`，证据见 [`R05-08-POPUP-LIFECYCLE-20260918.md`](../design/reviews/ui-finesse-round3-20260915/evidence/R05-08-POPUP-LIFECYCLE-20260918.md)。
- 边界仍是隔离合成数据/隐藏 WPF Window/offscreen logical DIP：未验真实 Playnite 关闭/重建、泄漏 profiler、物理 DPI/跨屏、OS 输入/IME、读屏/UIA、presented frame、ETW 或宿主性能；未写真实存档、媒体、云端或诊断数据。下一可执行任务为 R06-01 列宽用户记忆。

## 当前第三轮 R05-07 Tooltip 时序

- `2fc8c0d67c1d6cf585c85b984607efefb4b129a3` 修复实际生产资源覆盖缺口：`AcrylicReferenceControls.xaml` 不再丢失 Tooltip 的 `MaxWidth=420`，并与 `DesignTokens.xaml` 一起明确 `Focusable=False`、`Placement=Mouse`、字符串换行/不省略；壳层和独立设置页接入局部 `GscToolTipBehavior`，Esc 关闭当前插件范围 Tooltip，不移动输入焦点。
- `R05TooltipTimingBehaviorTests 1/1` 使用真实生产 Shell、生产路径 TextBox/Tooltip 样式和 STA WPF Window 验证 350/100ms 时序、420 DIP 宽度、长合成路径完整换行、Escape 关闭和焦点保持；相邻 R05 Popup `2/2`、焦点 `3/3`、开关 `1/1`，源契约 `24/24`。
- clean RenderHarness 绑定完整 SHA：Light/Dark、297 PNG、`WorkingTreeClean=True`、`render-qa OK`；Settings Popup/Tooltip 开面探针、Overview/Settings 代表图已抽查。标准构建 XAML `24/24`、编译 `0/0`、Core `83/83`；Worker 全集 `309/311`，2 条既有健康状态断言在隔离环境返回 Warning 而失败。
- 受控边界仍未包含真实 Playnite 悬停轨迹/气泡雨、ShowDuration 消失时序、屏幕边缘翻转与遮挡、物理 DPI/跨屏、宿主字体、OS 输入/IME、读屏/UIA、presented frame、ETW 或宿主性能；未写真实存档、媒体、云端或诊断数据。证据见 [`R05-07-TOOLTIP-TIMING-20260917.md`](../design/reviews/ui-finesse-round3-20260915/evidence/R05-07-TOOLTIP-TIMING-20260917.md)。下一可执行任务为 R05-08 弹层资源热切换。

## 当前第三轮 R05-06 开关保存语义

- `cf9a250f5020de056d070c785fc00f92a81cdc2a` 修复生产 `GameSaveCenterSettings` 布尔属性在 `CopyFrom`/`CancelEdit`/导入时不发 `PropertyChanged` 的绑定缺口：所有布尔设置改用字段和去重 `SetBoolean`，真实变化通知 WPF，同值不重复通知；没有改 Playnite 持久化协议、Worker live apply、命令、取消/错误或安全语义。
- 实际生产 `GameSaveCenterSettingsView` STA WPF 行为 `R05TogglePersistenceBehaviorTests 1/1`：外观开关切换后模型值、实际 `ToggleSwitch.IsChecked`、模板 Track 颜色、脏保存提示和隔离 `ExportPortableJson` 快照一致；`CancelEdit` 恢复模型/显示/依赖面板。自动化页媒体来源和恢复巡检参数的 `IsEnabled` 随开关关闭/回滚正确变化。
- 提交后 clean Release：XAML `24/24`、构建 `0/0`；`SettingsSaveFeedbackTests 2/2`、`SettingsDraftLifecycleBehaviorTests 1/1`、`UiFinesseRound2ControlSourceTests 24/24`，`git diff --check` 通过。RenderHarness 绑定完整 SHA，Light/Dark、297 PNG、`WorkingTreeClean=True`、`render-qa OK`，Settings 外观/自动化代表图已抽查。
- R04-08 的保存中、Playnite 已写入/Worker 应用失败、本地保存失败稳定提示继续复用；当前字段没有仅重启生效项。证据见 [`R05-06-TOGGLE-SAVE-20260917.md`](../design/reviews/ui-finesse-round3-20260915/evidence/R05-06-TOGGLE-SAVE-20260917.md)。边界仍是合成设置/隔离目录/STA WPF/offscreen logical DIP；未在真实 Playnite 注入保存或 Worker 失败，未验宿主保存取消关闭、OS 输入/IME、读屏/UIA、物理 DPI/跨屏、presented frame、ETW 或宿主性能，未写真实存档/媒体/云端。下一可执行任务为 R05-07 Tooltip 时序。

## 当前第三轮 R05-05 复选框三态边界

- R05-05 核对结论为“不适用”：当前生产媒体批量入口是 `MediaCenterView` 的 Extended DataGrid，批量命令消费 `SelectedItems`，没有代表“当前页/全部结果”的全选或半选 CheckBox。R05-04 已覆盖真实模型的总选择、当前窗口可操作数、暂不可见例外和当前模式清空语义。
- 当前生产 CheckBox 只有设置项/锁定等标量布尔绑定；`UiFrameworkProbeView` 的显式三态 CheckBox 是开发校对夹具。共享 `GscCheckBox` 与 `GscDataGridCheckBox` 已定义 `IndeterminateMark`，Q10-02 既有 `indeterminate=True mark=visible` 只证明共享视觉，不代表批量集合语义。
- 以文档提交后的当前 HEAD `a3c4a67` 重新执行标准 Release：XAML `24/24`、构建 `0/0`；`UiFinesseRound2ControlSourceTests 24/24`。之前直接复跑的 24 个失败是程序集仍绑定 `52900815` 而源码 HEAD 已变的身份不一致，已通过标准构建消除。
- 不为凑三态新增不存在的全选模型；未来若出现当前页/全部结果 CheckBox，应先固定集合来源，再补 Space、UIA、筛选/分页/部分失败行为。边界仍是不启动真实 Playnite、不验证真实 OS 输入/读屏/UIA/物理 DPI/跨屏/presented frame/ETW/宿主性能，不写真实存档/媒体/云端。证据见 [`R05-05-CHECKBOX-THREESTATE-20260917.md`](../design/reviews/ui-finesse-round3-20260915/evidence/R05-05-CHECKBOX-THREESTATE-20260917.md)。下一可执行任务为 R05-06 开关保存语义。

## 当前第三轮 R05-04 多选摘要

- `52900815c1fb8a16550446b9ee8d5318b8238a25` 先复用媒体收件箱已有 Extended DataGrid、按媒体 ID 保留当前模式的选择集合、加载更多恢复和批量提交前去重/无效项统计；只补齐摘要的总数/当前窗口范围/不可见例外表达，并增加独立“清空选择”入口。
- 实际生产 `MediaCenterView` + STA WPF 行为 `R05MultiSelectionSummaryBehaviorTests 3/3`：0 项显示未选择，1 项和多项只显示数量；保留一个当前窗口不可见 ID 时显示总选择与当前可操作数量；点击清空后选择集合归零，`已忽略` 收件箱模式保持不变。R05-01/02/03 回归合计 `11/11`。
- clean Release `scripts/build.ps1`：XAML `24/24`、构建 `0/0`；clean RenderHarness 绑定完整 SHA、Light/Dark、357 PNG、`WorkingTreeClean=True`、`render-qa OK`，Media `1040×700` 双主题图已抽查。未改游戏选框/滚动条、命令绑定、取消/错误/恢复保护、有限列表、Playnite/net462 或真实业务写入。
- 边界：跨窗口例外在隔离生产视觉树中以保留 ID 夹具和真实 DataGrid 选择验证，不等价真实 Playnite 分页、后端删除竞态、OS 输入/IME、读屏/UIA、物理 DPI/跨屏、presented frame、ETW 或宿主性能；渲染 `DpiScale=1.00` 仅 logical DIP。未写真实存档、媒体或云端。下一可执行任务为 R05-05 复选框三态。

## 当前第三轮 R05-03 弹层边缘适配

- `cbfacd2064d6bb5400e3e203ec4f5f14493d44ad` 核对确认游戏选框是壳层内 Grid，共享 ComboBox 才是 WPF Popup；修复生产 `PickerPanel` 固定 460/500 在短壳层越界的问题，为其绑定 `PickerOverlay.ActualWidth`/`ActualHeight` 最大约束，保留现有筛选、滚动和命令链。
- `R05PopupBoundaryBehaviorTests` 实际生产资源/STA WPF `2/2`：720×360 短壳层面板不越界、Auto 列表滚动和选定行可见；共享 ComboBox 在当前桌面右下工作区打开时 Popup 翻转回工作区，最大高度、滚动条和选定项可见均通过。420×220 极端壳层只有约 `92×92 DIP`，面板虽不越界但列表视口为 0，完整交互保留为最小尺寸/紧凑布局待定义边界。
- clean Release XAML `24/24`、构建 `0/0`；clean artifact R05-03 `2/2`、R05-02 回归 `3/3`。clean RenderHarness 绑定完整 SHA，Light/Dark、297 PNG、`WorkingTreeClean=True`、`render-qa OK`，Shell/Settings 图已抽查。证据见 [`R05-03-POPUP-EDGE-20260917.md`](../design/reviews/ui-finesse-round3-20260915/evidence/R05-03-POPUP-EDGE-20260917.md)。
- 边界：Popup 几何是在当前桌面工作区的设备坐标观察，不代表多物理屏/真实 Playnite；未把它写成 presented frame 无闪屏，真实 OS 输入/IME、读屏、UIA、物理 DPI/跨屏、ETW、宿主性能仍未验；未写真实存档、媒体或云端。下一可执行任务为 R05-04 多选摘要。

## 当前第三轮 R05-02 选项虚拟化焦点

- `7a4ede84667d916ad61d382302b742cca8fd4704` 先复现生产 Shell 游戏选框第一次 `Down` 会因 `SelectionChanged` 误关闭弹层，再以最小范围补键盘导航保护：方向键、PageUp/PageDown、Home/End 更新活动项时不关闭；鼠标预览点击清除保护，保留原有点击选择提交和关闭语义；卸载清理代际状态。
- 实际生产 Shell + 隔离 STA WPF 的 2000 项行为 `R05OptionVirtualizationBehaviorTests 3/3`：有限窗口保持虚拟化，PageDown/End/Home 后活动项均在实际 DIP 可见范围；过滤到无结果时保留有效隐藏选择并可用已有恢复命令，删除当前项后回退首个有效项。未改 `GamePickerViewModel`、过滤契约、滚动条、命令/Binding、取消错误/恢复保护、有限列表或 net462。
- clean Release XAML `24/24`、构建 `0/0`；clean artifact R05-02 `3/3`、R05-01 回归 `3/3`、既有选框键盘 `6/6`。clean RenderHarness 绑定完整 SHA，Light/Dark、297 PNG、`WorkingTreeClean=True`、`render-qa OK`，Shell/Settings/Task 代表图已抽查。证据见 [`R05-02-OPTION-VIRTUALIZATION-20260917.md`](../design/reviews/ui-finesse-round3-20260915/evidence/R05-02-OPTION-VIRTUALIZATION-20260917.md)。
- 边界：键盘是生产视觉树隔离 STA WPF 的合成路由，不等价真实 Playnite、物理键盘/IME、读屏或 UIA；渲染的 `DpiScale=1.00` 仅 logical DIP，真实物理 DPI/跨屏、presented frame、ETW、宿主性能仍未验；未写真实存档、媒体或云端。下一可执行任务为 R05-03 弹层边缘适配。

## 当前第三轮 R05-01 弹层焦点范围

- `0004999507d0cf27f483ea1234a7a60b4ad7f9b7` 在最新生产 Shell 上收口弹层焦点边界：`PickerOverlay` 和 Dashboard `DialogOverlay` 声明本地焦点范围、Tab 循环和方向键包含；游戏选框打开后进入 `GameSearchTextBox`，关闭仍回焦 `GameContextButton`。共享 `ComboBoxItem` 明确 `IsTabStop=False`，选项导航仍由原生方向键/Enter/Escape 负责。
- 实际生产 Shell STA WPF 行为 `R05FocusBoundaryBehaviorTests 3/3`：此前已复现第一次 Tab 落到遮挡层后的 `RadioButton`（`Focus=RadioButton, inside=False`），修复后前进/后退焦点均留在选框、Shift+Tab 回到最后访问选项、Escape 关闭并回焦上下文按钮；ComboBox 选项不成为表单 Tab 停靠点。未改 `GamePickerViewModel`、滚动条、命令/Binding、取消错误/恢复保护、有限列表或 net462。
- clean Release XAML `24/24`、构建 `0/0`；clean RenderHarness 绑定完整 SHA，Light/Dark、297 PNG、`WorkingTreeClean=True`、`render-qa OK`，Settings normal/dirty 和 Overview 图已抽查。证据见 [`R05-01-FOCUS-BOUNDARY-20260917.md`](../design/reviews/ui-finesse-round3-20260915/evidence/R05-01-FOCUS-BOUNDARY-20260917.md)。
- 边界：Dashboard 真实宿主模态事件顺序未启动 Playnite，仅有生产代码契约/源审查；ComboBox 与渲染均为隔离 STA WPF/offscreen logical DIP（`DpiScale=1.00`），真实 OS 输入/IME、读屏、物理 DPI/跨屏、presented frame、ETW、宿主性能仍未验；未写真实存档、媒体或云端。下一可执行任务为 R05-02 选项虚拟化焦点。

## 当前第三轮 R04-08 保存反馈闭环

- `c735905aa1092f409bc7ddf9de1272f1d77dc084` 在最新设置保存链上补齐稳定反馈：`GameSaveCenterSettings` 发出保存开始、应用开始、应用完成和保存失败事件；`GameSaveCenterPlugin` 把既有 Worker 异步应用完成/异常回传设置页；并发 `EndEdit` 被 `Interlocked.CompareExchange` 抑制。
- 设置页统一提示面现在区分正在保存、已写入 Playnite/正在应用 Worker、Worker 应用失败、本地保存失败、校验中、校验错误、脏草稿和已保存。Worker 失败不伪报成功；本地写入失败保留编辑克隆。当前生产字段均 live apply，没有真实的仅重启生效项，因此不生成虚构重启态。
- R04-08 相关选择集 `20/20`；clean Release XAML `24/24`、构建 `0/0`、源码校验与 diff check 通过。RenderHarness 绑定完整 SHA、双主题、297 PNG、`WorkingTreeClean=True`、`render-qa OK`，Settings normal/dirty/invalid 图已抽查。证据见 [`R04-08-SAVE-FEEDBACK-20260917.md`](../design/reviews/ui-finesse-round3-20260915/evidence/R04-08-SAVE-FEEDBACK-20260917.md)。
- 边界：尚未在真实 Playnite 宿主注入本地保存或 Worker 失败，也未验真实保存/取消/关闭时序；渲染和行为证据仍是合成设置/fake 服务/隔离目录/STA WPF/offscreen logical DIP。真实 Playnite 嵌入、系统输入/IME/剪贴板、读屏、物理 DPI/跨屏、presented frame、ETW、宿主性能仍未验，未写真实存档、媒体或云端。下一可执行任务为 R05-01 弹层焦点范围。

## 当前第三轮 R04-07 异步校验竞态

- `b1d5b68f` 将设置页编辑期路径可用性检查拆为不可变 `SettingsPathValidationSnapshot`、后台 `SettingsPathValidationService` 和递增版本的 `LatestAsyncValidationCoordinator`；复用 `GameSaveCenterSettings.VerifySettings` 的原有文件/目录规则，完整同步校验仍保留用于保存状态/最终安全路径。
- `GameSaveCenterSettingsView` 在字段事件合并后读取最新快照，旧请求取消或晚返回均不能覆盖新版本；DataContext 替换、提交、回滚和 Unloaded 使旧结果失效。校验中保存提示明确等待，后台异常变成可修复提示，不同步阻塞 UI 线程。`df6f8083` 修复内部验证摘要重载与 RenderHarness 无参数反射入口冲突。
- clean Release XAML `24/24`、构建 `0/0`、源码校验通过；R04-07 定向 `7/7`。RenderHarness clean `df6f8083` 双主题、297 PNG、`WorkingTreeClean=True`、`render-qa OK`，Settings normal/dirty/invalid 图已抽查。
- 边界：测试使用合成设置/隔离目录/任务完成源/STA WPF 与 offscreen logical DIP；真实 Playnite 宿主输入/关闭时序、慢网络共享、IME/剪贴板、读屏、物理 DPI/跨屏、presented frame、ETW、宿主线程/帧率性能仍未验；不可中断的单次文件系统调用只能取消结果应用，未写真实存档、媒体或云端。下一可执行任务为 R04-08 保存反馈闭环。

## 当前第三轮 R04-06 清空与撤销

- `0d3f71af27f00c34cbfc5d012f5f7dde39a4625c` 复用 Shell、Dashboard、Trainer、Task、Media 现有搜索清空处理和 WPF 原生编辑命令；Dashboard 空状态“清除搜索”现在保留 `GamePicker.ClearSearchCommand`，并通过目标 TextBox Tag/Click 复用清空与焦点回返，事件已处理。
- 游戏选框 Esc 仍只执行关闭和上下文按钮焦点恢复；普通 TextBox 通过 WPF `ApplicationCommands.SelectAll/Undo/Redo` 与标准粘贴事件保持选择区和重做，不接触保存/恢复/删除等危险命令。
- clean Release 构建 XAML `24/24`、构建 `0/0`，源码校验/diff check 通过；R04-06 实际 STA WPF 行为 `3/3`。clean RenderHarness 绑定完整 SHA、双主题、297 PNG、`WorkingTreeClean=True`、`render-qa OK`，Task/Shell `1040×700` 已抽查。
- 边界：合成搜索数据、隔离 STA WPF、offscreen logical DIP；真实 Playnite 系统输入/剪贴板/IME/读屏/物理 DPI/跨屏/presented frame/ETW/宿主性能仍未验，未写真实存档、媒体或云端。下一可执行任务为 R04-07 异步校验竞态。

## 当前第三轮 R04-05 数字输入边界

- `8d56eb5a050a5c2db2718a42cf928b7d43d6f897` 先复用现有 `GscNumericTextBox`、`IntegerRangeValidationRule` 和服务侧安全边界；没有把 main 旧实现带入当前分支，也没有为当前不存在的重试/端口/容量字段扩展 DTO 或 UI。
- 共享验证现在明确拒绝空值、非整数、`Int32` 溢出和字段上下界之外的输入。存档策略页间隔/保留周期模板与 Trainer 启动延迟已接入同一绑定规则：分别为 `1–1440`、`0–2147483647`、`0–300`。
- 共享 `NumericInput` attached behavior 让滚轮按同一规则以 ±1 步进；非法当前值、越界候选或更新源验证错误保持文本/源值不变，不静默 clamp，越界滚轮不吞掉以便现有页面滚动继续工作。键盘/粘贴仍沿用 WPF binding validation，未改命令、取消/错误、安全、游戏选框或滚动条。
- clean commit 定向 `20/20`；Release XAML `24/24`、构建 `0/0`、源码校验和 diff check 通过。RenderHarness 绑定完整 SHA、双主题、297 PNG、`WorkingTreeClean=True`、`render-qa OK`，Save `1040×700` 图已抽查。
- 边界：证据来自合成设置/fake 服务、隔离目录、STA WPF、offscreen logical DIP；真实 Playnite 系统输入/IME/读屏/物理 DPI/跨屏/presented frame/ETW/宿主性能仍未验，未写真实存档、媒体或云端。下一可执行任务为 R04-06 清空与撤销。

## 当前第三轮 R04-04 粘贴标准化

- `d7e92f1` 先核对当前生产字段：设置页有本地路径和 Rclone 云端目标，MediaCenter 有自定义媒体目录与文件模式；当前没有可编辑端口字段，也没有独立 Exclude 设置字段，没有从 main 带入旧实现。
- 新增共享 `PasteNormalization` attached behavior，按 `Path`、`RemoteTarget`、`Port`、`ExcludePattern` 类型保留语义边界。已接入设置 6 个路径、云端目标、媒体目录和媒体文件模式；Port 类型只作为未来真实字段的可复用合同，不新增业务 UI。
- 单值粘贴去除外层空白/成对引号及尾随 shell 换行；内部仍有换行的多行粘贴被拒绝，字段保持原值，剪贴板不改写。标准化结果通过 Tooltip/Automation HelpText 解释，并用 `TextBox.SelectedText` 保留原生 Ctrl+Z 恢复粘贴前字段值。
- `PasteNormalizationTests`、`PasteNormalizationSourceTests`、`SettingsValidationSourceTests` 定向 `8/8`；clean Release XAML `24/24`、构建 `0/0`、源码校验通过。clean RenderHarness 绑定完整 `d7e92f1467648f045ff94fb30da5ac9712cd402b`，`WorkingTreeClean=True`、双主题、297 PNG、`render-qa OK`；人工抽查 Settings/Media `1040×700`。
- 边界：合成文本/设置/fake Worker、隔离目录、STA WPF、offscreen logical DIP；未验真实 Playnite 剪贴板/输入链、IME、读屏、物理 DPI/跨屏、presented frame、ETW、宿主性能，也没有真实端口/排除字段可做宿主验证。未写真实存档、媒体或云端。下一可执行任务为 R04-05 数字输入边界。

## 当前第三轮 R04-03 未保存离开保护

- `58e1734` 先核对并复用现有 Playnite `ISettings` 编辑克隆、三态保存提示、设置分类和滚动入口；模型新增 `HasPendingEdit`/`GetEditBaselineFingerprint()`，`CancelEdit`/`EndEdit` 明确清空编辑缓冲，未新增保存服务，也没有从 main 带入旧实现。
- 设置页在宿主 `Window.Closing` 只对当前编辑会话的有效脏草稿进入现有原生消息框：选择“是”调用既有 `CancelEdit` 后关闭；选择“否”取消关闭，保留草稿并恢复离开前分类、字段焦点；确认异常时保守保留窗口和草稿。临时卸载/重挂同一视图会继续使用 Playnite 编辑基线并恢复原焦点。
- clean Release XAML `24/24`、构建 `0 warning / 0 error`、源码校验通过；R04-03 定向 `12/12`。真实 STA WPF Window 测试验证草稿分离/重挂、分类/字段焦点恢复和显式 CancelEdit 回滚；模型测试验证编辑基线与单次撤销。
- clean RenderHarness 绑定完整 SHA `58e1734f0bf84b82d0fd95077327122e9b99cbf3`，`WorkingTreeClean=True`，Light/Dark、多尺寸、设置 normal/dirty/invalid、滚动/虚拟化、Shell/resize `render-qa OK`，357 PNG；人工抽查设置状态图。离屏证据不代表真实关闭确认 UI。
- 边界：合成设置/fake Worker、隔离目录、STA WPF 与 offscreen logical DIP；没有真实 Playnite 原生关闭按钮/确认框 UIA 操作，真实宿主保存/取消/关闭时序、屏幕阅读器、物理 DPI/跨屏、presented frame、ETW、宿主性能仍未验；未写真实存档、媒体或云端。证据见 [`R04-03-DRAFT-LIFECYCLE-20260917.md`](../design/reviews/ui-finesse-round3-20260915/evidence/R04-03-DRAFT-LIFECYCLE-20260917.md)。下一可执行任务为 R04-04 粘贴标准化。

## 当前第三轮 R04-02 错误摘要导航

- `6524f94` 先核对并复用现有 `GameSaveCenterSettings.VerifySettings`、字段 Validation 模板、页头摘要/详情、设置分类 ListBox 与 `SettingsScroller`；没有从 main 带入旧实现，也没有替换游戏选框或滚动条。设置校验错误现在按真实文案映射到已命名字段，详情中的可定位消息使用 Hyperlink。
- 点击错误链接或“定位首个错误”会选择对应分类，并对目标字段执行 `BringIntoView`、`Focus` 与 `Keyboard.Focus`；目标字段的 Automation HelpText 含具体错误原因，链接的 Automation Name/HelpText 含定位说明。`HealthInspectionEnabled=false` 时，禁用的巡检间隔/有效期旧值不进入 `VerifySettings`，保存仍保持原有阻断与提交语义。
- clean commit 隔离 Release XAML `24/24`、构建 `0/0`、源码校验通过；R04-02 定向 `5/5`，其中真实 STA WPF Window 验证跨 Tab、字段焦点、滚动和读出原因，模型负例验证隐藏巡检字段不阻止保存。
- clean RenderHarness 绑定完整 SHA `6524f944f05921f02f0b32b7f5aa3dfa702ac165`，双主题、多尺寸、滚动/虚拟化、Shell/resize `render-qa OK`，357 PNG，探针记录 `links=1`、分类切换、字段可见性和 HelpText；人工抽查设置页明暗代表图。全量 Playnite testhost 同代码两次复跑出现 18/23 个既有 WPF 环境性失败，未改写为通过；后续需在稳定宿主复测。
- 边界：合成设置/fake Worker、隔离目录、STA WPF 与 offscreen logical DIP；RenderHarness 未连接 PresentationSource，不代表真实桌面焦点/滚动或 presented frame。真实屏幕阅读器、Playnite 嵌入、物理 DPI/跨屏、OS 输入、ETW、宿主性能仍未验；未写真实存档、媒体或云端。证据见 [`R04-02-VALIDATION-NAVIGATION-20260917.md`](../design/reviews/ui-finesse-round3-20260915/evidence/R04-02-VALIDATION-NAVIGATION-20260917.md)。下一可执行任务为 R04-03 未保存离开保护。

## 当前第三轮 R04-01 组合输入状态

- `40c7f9a` 先复核 R00-08 的可见生产游戏选框、`ItemsView` 候选确认、无结果/`ImeProcessed`/方向键/Escape 焦点行为和现有本地搜索；没有从 main 带入旧实现，也没有替换 picker/滚动条/设计体系。`AcrylicProductionShellView` 为 `GameSearchTextBox` 接入 WPF `TextComposition` start/update/预览与冒泡 commit 事件，卸载时清除状态。
- 组合期间 `SelectionChanged` 不进入 `SelectedGame`/关闭流程，`PreviewKeyDown` 不处理 Enter；提交事件到达后恢复原有可见 `ItemsView` 候选确认与焦点回返。`GamePickerKeyboardBehaviorTests` 实际 `4/4`，包含 start/update + Enter 负例、commit + Enter 正例，以及既有无结果/IMEProcessed/方向键/焦点回归。英文即时反馈仍由 `GamePickerViewModel` 本地缓存/现有测试承担，未转发 Worker。
- clean commit 隔离 Release XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `569/626`（57 跳过、0 失败），源码校验通过。首次未提交工作树全量曾有 2 条一次性失败；同一隔离输出复跑及 clean commit 全量均 0 失败，未放宽门禁。
- RenderHarness clean `40c7f9a` 双主题、多尺寸、滚动/虚拟化、Shell/resize 为 `render-qa OK`，`WorkingTreeClean=True`，357 PNG；人工抽查 Light/Dark Task 与 Shell。证据只代表合成 DTO、隔离 STA WPF/offscreen logical DIP；真实 Windows IME 候选 UI/物理键盘/Playnite 嵌入输入链、屏幕阅读器、物理 DPI/跨屏、presented frame、ETW、宿主性能仍未验。下一可执行项为 R04-02 错误摘要导航。

## 当前第三轮 R03-08 用户文本缩放

- `981b600` 先核对现有 `GscBodyFontSize`/`GscCaptionFontSize`、按钮/输入框模板和表头样式，未引入新字体体系，也没有从 main 带入旧实现。共享 TextBox、远程恢复/媒体批处理按钮、只读完整路径框和 Overview 两个保护按钮去掉硬 `Height`，保留 `MinHeight`、现有模板与命令/安全语义。
- 共享 DataGrid 去掉 `ColumnHeaderHeight` 覆盖；生产 `DataGridColumnHeader` 通过 `GscTableHeaderHeight` 最小高度和 `GscDataGridHeaderTextTemplate` 的 `Wrap + Trimming=None` 允许较大文本自然增长。Task、Media、Dashboard、Redesign 和开发探针的重复覆盖同步移除。
- `R03TextScaleTests` 实际 STA WPF `2/2`：窗口资源将正文/说明字号覆盖为 `24/20 DIP`，输入框、文本按钮、表头均按需求测量且内容宿主不裁切；既有相关断言已改为检查最小高度/可见性，不放宽负例。隔离 Release XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `568/625`（57 跳过、0 失败），源码校验通过。
- RenderHarness clean commit `981b600` 双主题、多尺寸、滚动/虚拟化、Shell/resize 为 `render-qa OK`，`WorkingTreeClean=True`，357 PNG；人工抽查 Light/Dark Task、Light Media、Dark Save。证据来自合成文本、隔离 WPF/offscreen logical DIP；真实用户文本设置/宿主字体替换、Playnite presented frame、物理 DPI/跨屏、OS 输入/IME、读屏、ETW、宿主性能仍未验。下一可执行项为 R04-01 组合输入状态。

## 当前第三轮 R03-07 文案标点统一

- `5a07eda` 先盘点当前生产可见文案，确认半角冒号集中在 Task 状态/类型/范围/时间四个标签，失败 `DetailMessage` 仍生成 `ErrorCode: ErrorMessage`；没有从 main 带入旧实现，也没有重建设计体系。四个真实 Task TextBlock 已改为全角冒号；失败详情和整库聚合使用“错误码：…；…”/“游戏：…；…”展示分隔。
- `TaskStatusDto` 只在展示层格式化失败详情：`ErrorCode`、`ErrorMessage` 保持原值；错误码为空时直接返回错误正文，避免孤立标签。R03-05 的 `CopyPathCommand`/路径转换/完整路径绑定未改，路径原值不经过本轮标点格式化。
- `R03CopySafePunctuationTests` 实际 STA WPF 定向 `3/3`，覆盖四个生产标签、含 `C:\Saves\A:1` 的错误正文原样保留、空错误码和非失败原消息负例。全量隔离 Release XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `566/623`（57 跳过、0 失败），源码校验通过。
- RenderHarness 绑定 clean commit `5a07eda`，Light/Dark 多尺寸页面、滚动/虚拟化、Shell/resize 均 `render-qa OK`，`WorkingTreeClean=True`，357 PNG；人工抽查 Task `1040×700`、`1366×768` 明暗图。证据使用合成 DTO、受控 STA WPF、offscreen logical DIP；真实 Playnite presented frame、宿主字体/物理 DPI/跨屏、OS 输入/IME、读屏、ETW、宿主性能和真实剪贴板仍未验。下一可执行项为 R03-08 用户文本缩放。

## 当前第三轮阶段：R03-06 双语长度压力

- `52527b6` 先核对已有标题 `CharacterEllipsis + Tooltip`、长文本表格样式和 RenderHarness `LongTitle` profile；确认真正缺口是共享文字动作模板的 `NoWrap + CharacterEllipsis`，没有把 main 旧实现覆盖到当前分支。
- 共享 `GscWpfUiButtonTextTemplate` 改为 `TextWrapping=Wrap`、`TextTrimming=None`；新增 `BilingualLengthStress` 合成 profile，把英文长句和中文长游戏名放入实际 Overview 任务/活动/关注项数据。`R03BilingualLengthTests` `2/2`，154 DIP 窄槽实际测量两个动作均保留完整文案、Tooltip 和可增长高度。
- clean commit 的 `overviewedges` 覆盖 Light/Dark、820/1040/1600 DIP：双语样本 `surfaces=6/6`、标题省略且 `titleTooltip=True`、英文长句可见、当前游戏动作 2 个、无横向溢出；统一 `render-qa` 绑定 `52527b6`、`WorkingTreeClean=True`、297 PNG、`render-qa OK`。全量 Release XAML `24/24`、构建 `0/0`、Core `83/83`、Playnite `563/620`（57 跳过、0 失败），源码校验通过。
- 证据来自合成数据、受控 STA WPF 和 offscreen logical DIP；未验真实 Playnite presented frame、宿主字体替换、物理 DPI/跨屏、OS 输入/IME、屏幕阅读器、ETW 或宿主性能。Worker 本阶段未改，沿用同分支最近完整基线 `311/311`；未写真实存档/媒体/云端。下一可执行项为 R03-07 文案标点统一。

## 当前第三轮阶段：R03-05 长路径分层

- `2b8612f` 先核对现有 `GscPathText`、Tooltip 和复制重试：原有表格路径只有尾部省略，详情没有统一的可选择完整值/路径复制命令；没有把 main 旧实现覆盖到当前分支。
- 新增 `PathDisplayConverter`/`GscPathDisplayConverter`，长预览保留盘符/根路径和文件名尾部；新增共享 `GscWpfUiPathDetailTextBox`（只读、NoWrap、隐藏横向滚动、原生选择/Ctrl+C）；`CopyPathCommand` 直接复用现有 `CopyTextWithRetryAsync`，复制值不经过预览转换。
- Save 候选、Media 待归类/媒体详情、Trainer 选中版本均接入预览、完整可选框和复制按钮。`R03LongPathTests` `3/3`：超过 260 字符的合成路径在 340 DIP 内不撑宽，SelectedText 与原始路径完全一致；首轮过期的 Trainer 源码断言已校准，不改 skip。
- 最终隔离 Release：XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `561/618` 通过、`57` 跳过、`0` 失败；源码校验通过。RenderHarness 绑定 `2b8612f` 且 `WorkingTreeClean=True`，Light/Dark 多尺寸 `render-qa OK`，共 297 张 PNG；人工查看 Save/Media/Trainer 1366×768 选中详情。
- 证据来自合成路径、受控 STA WPF、offscreen logical DIP；未验真实 Playnite presented frame、真实 OS 剪贴板/键盘/IME、读屏、宿主字体、物理 DPI/跨屏、ETW 或宿主性能。Maintenance 摘要路径仍只保留 Tooltip/只读摘要，未宣称有独立复制详情；未写真实存档/媒体/云端。下一可执行项为 R03-06 双语长度压力。

## 当前第三轮阶段：R03-04 数字列对齐

- `9fa68ef` 先核对现有 `GscTypographyNumeric`、Save 数字样式和各页时间/百分比列；保留已有 Tabular 数字、游戏选框、滚动条和生产业务契约，没有把 main 旧实现覆盖到当前分支。
- 新增共享数字/时间/百分比单元格样式，并让 Save、Media、Maintenance、Task 的对应列统一右锚点；`TaskStatusDto.ProgressValue/ProgressDisplay` 只提供安全的显示层钳制与未知占位，原始 `ProgressPercent` 保留。
- `R03NumericAlignmentTests` `10/10`：真实 STA WPF 120 DIP 列中 `9/10/99/100` 右边界均在 `119.5～120.5 DIP`；排队 0/负数显示 `—`，正常百分比与超界钳制通过。最终隔离 Release：XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `558/615` 通过、`57` 跳过、`0` 失败；源码校验通过。
- RenderHarness 绑定 clean commit，Light/Dark 生产 1040×700 页面 `render-qa OK`，人工抽查 Task/Save/Media。证据来自合成 DTO、受控 WPF/offscreen logical DIP，不等价真实 Playnite presented frame、宿主字体替换、物理 DPI/跨屏、OS 输入/IME、读屏、ETW 或宿主性能；未写真实数据。下一可执行项为 R03-05 长路径分层。

## 当前第三轮阶段：R03-03 双语混排基线

- `466c2f5` 先核对当前生产 Save Center/存档中心、日期容量和中文标点入口，确认现有 Typography/数字样式已经统一承载混排，没有生产 XAML 需要通过行级位移修复；没有从 main 带入旧实现或重建设计体系。
- 新增 `MixedBaselineEvidence`/`CaptureMixedBaseline`，在 STA WPF `TextFormatter.GetIndexedGlyphRuns()` 中读取实际 run 的 baseline，`MixedChineseLatinDateAndCapacityRunsShareOneBaseline` 独立 `1/1`；Light/Dark 四组样本均有实际 glyph、`spread=0`、`stable=True`。
- 最终隔离 Release：XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `548/605` 通过、`57` 跳过、`0` 失败；`TypographyDiagnosticsTests` `11/11`；源码校验通过。RenderHarness Light/Dark `finesse-fixture OK`，生产 1040×700 双主题 `render-qa OK`，人工抽查 Overview/Media。
- 保留游戏选框、滚动条、命令/绑定、取消/错误、恢复保护、有限列表性能与 net462 契约，未写真实数据。证据仅代表受控 WPF/offscreen logical DIP，不等价真实 Playnite presented frame、宿主字体替换、物理 DPI/跨屏、OS 输入/IME、读屏、ETW 或宿主性能。下一可执行项为 R03-04 数字列对齐。

## 当前第三轮阶段：R03-02 阅读层级校准

- `e889b04` 先核对现有 `Typography.xaml`/`Redesign.xaml` 共享层级，再清理生产 Views/Settings 中 5 处 `12.5`、9 处 `10.5`、2 处 `9.5` 微字号：壳层品牌、媒体文件名、Overview 主标题归入 `GscBodyFontSize=14`；版本/状态/时间/详情/比较徽标归入 `GscCaptionFontSize=12`。路径继续复用 `GscTypographyCode`/`GscPathText`，未引入新设计体系。
- 新增 `ProductionTypographyUsesSharedHierarchyWithoutMicroSizeDrift`，逐行拒绝生产微字号并校对关键页面语义映射；最终 clean 隔离程序集定向 `10/10`。生产 Views/Settings 直接扫描没有 TextBlock 局部低透明度，剩余透明度只在 ambient 装饰层。
- 最终隔离 Release：XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `547/604` 通过、`57` 跳过、`0` 失败；源码校验通过。RenderHarness 绑定 `e889b04...` 且 `WorkingTreeClean=True`，Light/Dark 1040×700 生产窄窗均 `render-qa OK`，人工查看 Overview、Media、Save、Maintenance 代表图。
- 本项保留游戏选框、滚动条、命令/绑定、取消/错误、恢复保护、有限列表性能与 net462 契约，未写真实数据。证据只代表受控 WPF/offscreen logical DIP，不等价真实 Playnite presented frame、宿主字体替换、物理 DPI/跨屏、OS 输入/IME、读屏、ETW 或宿主性能。下一可执行项为 R03-03 双语基线。

## 当前第三轮阶段：R03-01 真实落字证据

- `855e727` 在不改生产字体链和页面布局的前提下，为现有 `TypographyDiagnostics` 增加 WPF `TextFormatter.GetIndexedGlyphRuns()` 诊断；候选 `FontCandidate` 与最终 `GlyphRunEvidence` 分开记录，未捕获或 `.notdef` 不会被推定为命中。
- clean commit 的 Light/Dark `finesseprobe` 对中文 U+5B58、英文 U+0053、数字 U+0039、`𠮷` U+20BB7、组合重音 U+0301 均输出 `GlyphRunCaptured`；`𠮷` 候选为 unresolved，但最终 Typeface 为 `MingLiU-ExtB`。真实 STA 定向 `1/1`，`TypographyDiagnosticsTests` 类 `9/9`。
- 最终隔离 Release：XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `546/603` 通过、`57` 跳过、`0` 失败；源码校验通过；RenderHarness Light/Dark 均 `WorkingTreeClean=True`、`finesse-fixture OK`，人工查看两张 `1120×980` 逻辑 DIP 夹具图。
- 证据只代表受控 WPF 排版/离屏 logical DIP，不等价真实 Playnite 最终呈现、宿主字体替换、物理 DPI、OS 输入/IME、屏幕阅读器、presented frame、ETW 或宿主性能。下一可执行项为 R03-02 阅读层级校准。

## 当前第三轮阶段：R02-08 动作文案动词化

- `43ea843` 先盘点实际 Binding 后收口动作标签：旧 Dashboard/Overview/Save Center 的 `LoadDetailsCommand` 入口统一为“重新加载详情”，旧 `ValidateCommand` 为“重新校验”；错误状态保留“重试”，不把重试冒充普通刷新。
- 远端恢复第一步明确为“下载到隔离区并校验”，第二步为“快照并恢复”；云端详情区明确区分“校验远端内容”和“重试上传”。CloudTransfer DTO、Playnite 提示和 Worker 失败消息统一使用“远端校验”，协议内部 `Check*` 状态名和 rclone 参数未改。
- `R02ActionCopyTests` `4/4`，连同相关既有可用性/焦点测试定向 `13/13`；最终干净 Release 为 XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `545/602` 通过、`57` 跳过、`0` 失败；源码校验通过。R03-01 已继续补齐真实 WPF GlyphRun 证据。
- RenderHarness 报告绑定 `43ea843613daf3eb3b1378d31181a712d3a10c05` 且工作树干净，Light/Dark 均 `render-qa OK`；人工查看 Save 与 Maintenance CloudQueue 1040×700 双主题代表图。未启动真实 Playnite、未执行外部云端/备份操作；宿主字体/输入、物理 DPI、IME、读屏、presented frame、ETW 和性能仍未验。下一可执行项为 R03-01 真实落字证据。

## 当前第三轮阶段：R02-07 异步菜单上下文

## 当前第三轮阶段：R02-07 异步菜单上下文

- `4d4bb93` 在 `GameMenuItem` 生成时保存不可变目标 Guid 快照；立即备份、同步媒体、查看备份历史、验证最新恢复点和游戏工具在异步执行前都按当前 `PlayniteApi.Database.Games` 重新解析，不跟随已刷新的旧 `Game` 引用，也不从当前选中行猜测目标。
- 列表刷新正例解析到替换后的对象并保持菜单原顺序；删除负例返回缺失 ID 和空结果。失效或无法确认时走既有提示/日志路径，在 Worker/Upsert 前明确提示“上下文菜单目标已失效……未执行操作”，然后中止。
- `R02MenuActionContextTests` 与 `R02MenuHostContractTests` 合计定向 `4/4`；最终干净 Release 为 XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `541/598` 通过、`57` 跳过、`0` 失败；源码校验通过。首轮只发现并校正一条过期源码断言，独立提交为 `4d4bb93`，干净复跑通过。
- 本阶段无生产 XAML/样式改动，不把已有 RenderHarness 结果冒充菜单宿主呈现证据；未启动真实 Playnite、未执行菜单 Action、未写真实存档/媒体/云端。宿主打开后事件时序、菜单视觉/输入、OS 键盘/IME、屏幕阅读器、物理 DPI、presented frame、ETW 和宿主性能仍未验。R02-08 已收口动作标签，下一可执行项为 R03-01 真实落字证据。

## 当前第三轮阶段：R02-06 菜单状态完整（外部宿主阻塞）

- 当前提交 `8e48ac8` 核对确认生产代码没有 WPF `ContextMenu/MenuItem`；唯一菜单入口是交给 Playnite 渲染的 `GameMenuItem`。本阶段没有凭空新增菜单体系，也没有把 main 的旧实现覆盖到当前分支。
- `R02MenuHostContractTests` 定向 `2/2`：空选择不产生菜单；两项合成游戏按固定顺序生成六个 `GameSaveCenter` 分组动作，均有非空 Action，但测试不执行 Action，因此不启动 Worker、不写真实数据。
- 完整隔离 Release：XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `539/596` 通过/`57` 跳过/`0` 失败；源码校验通过。
- 禁用/勾选/危险/子菜单/快捷键列、键盘上下左右/Enter/Esc、边缘翻转、点外关闭和焦点返回由 Playnite 宿主控制，当前工作区没有可测 visual tree，不能签收这些条件；没有启动真实宿主或触碰真实库。R02-07 已收口插件可控的异步目标身份，下一可执行项为 R02-08 动作文案动词化。

## 当前第三轮阶段：R02-05 命中区与间距

- 当前提交 `f03b4dd` 核对确认生产共享图标按钮链已满足本项实现方向：`GscIconOnlyButtonBase` 为 `34×34 DIP`、最小宽高 `34`、相邻 `6 DIP`；工具条变体为 `36×36 DIP`，文字动作继续使用 `GscButtonHeight=36`/`GscCompactButtonHeight=30`。本阶段只增加行为门禁，没有覆盖 main 旧实现或重建业务控件。
- `R02HitAreaSpacingTests` 定向 `2/2`：真实 STA WPF Window 中复制/删除中心分别命中各自 Button，6 DIP 间隔不命中任一按钮；宽 `82 DIP` 的 WrapPanel 排布为 `34+6+34` 同行，第三动作换到下一行，三者无正面积重叠且中心命中身份正确。
- 最终隔离 Release：XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `537/594` 通过/`57` 跳过/`0` 失败；源码校验通过。RenderHarness 绑定 `f03b4dd` 且工作树干净，Light/Dark 56 场景均 `OK`；人工对照 Media `1040×700` 双主题图，批量动作组和列表滚动完整。
- 证据来自真实生产 WPF 资源、合成图标、隔离 STA Window、`VisualTreeHelper.HitTest` 和 offscreen logical DIP；未验真实 Playnite、OS 鼠标/键盘输入、屏幕阅读器、物理 DPI/跨屏、IME、presented frame、ETW 或宿主性能。未写真实存档/媒体/云端。下一可执行项为 R02-06 菜单状态完整。

## 当前第三轮阶段：R02-04 图文光学居中

- 当前提交 `d296ce0` 未改生产按钮体系；既有 `GscWpfUiButtonTextTemplate`、共享内容居中属性和 `ContentTemplate={x:Null}` 复合图标路径已满足实现方向，本阶段新增 `R02OpticalAlignmentTests` 作为实际几何门禁。
- R02 定向 Release `3/3`，同样式中文四字/英文双词基线差 `<0.5 DIP`；16/20 DIP 图标与数字间距 `8 DIP`、中心差 `≤1.5 DIP`。Light/Dark RenderHarness 56 个视图/尺寸场景均 `OK`，报告绑定 `d296ce0` 且工作树干净。
- 最终完整隔离 Release：XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `535/592` 通过/`57` 跳过/`0` 失败；一次既有 IPC 取消时序用例先失败，立即单项 `1/1` 复跑并随后完整脚本通过，未改写 skip/失败事实。
- 证据来自真实生产 WPF 资源、合成 Geometry、隔离 STA Window 和 offscreen logical DIP；未验真实 Playnite、物理 DPI/跨屏、屏幕阅读器、OS 输入/IME、presented frame、ETW 或宿主性能。下一可执行项为 R02-05 命中区与间距。

## 当前第三轮阶段：R02-03 禁用原因可达

- `627f864` 在核对既有 Restore、Media Inbox、Cloud Transfer 和 Remote Restore 命令门禁后，只增加共享 `ActionAvailabilityHints` 状态说明、生产样式和相邻维护路径；没有放宽危险命令，没有新增服务/DTO，也没有覆盖 main 旧实现。
- Save、Media、Maintenance 的说明使用可聚焦 TextBlock，Name/HelpText 与可见文案一致；需要 Worker/配置时复用既有 `OpenMaintenanceCommand`。命令 Binding、恢复保护、取消/错误、游戏选框、滚动条、有限列表和 net462 继续保留。
- 最终隔离 Release：XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `532/589` 通过/`57` 跳过/`0` 失败；R02 定向 `4/4`；RenderHarness 双主题 56 个视图/尺寸场景均 `OK`，源码校验通过。
- 证据来自合成状态、fake 服务、真实生产 WPF 样式、STA Window 和 offscreen logical DIP；没有真实 Playnite、用户云端或诊断写入。未验真实屏幕阅读器、物理 DPI/跨屏、OS 键盘/IME、presented frame、ETW、Worker 时序和宿主性能。下一可执行项为 R02-04 图文光学居中。

## 当前第三轮阶段：R02-02 忙碌宽度稳定

- `473cf3a` 在核对前序 `4947539` 忙态模板后，将共享 `GscWpfUiButton` 的 `IsBusy` 绑定接到最近生产页面 `UserControl.DataContext.IsBusy`，所以 Acrylic 壳层与 Overview/Save/Media 工作区的派生按钮承接真实 Dashboard 命令状态；无该属性的设置/校对夹具回到 `false`，旧 Dashboard 本地绑定继续优先。
- `BusyOperationCoordinator` 已接入 `DashboardViewModel.RunAsync`，以原子单槽拒绝重复执行，并保持真实 Worker 准备、取消文案、错误通知、Trainer 清理和 queued refresh 的原有顺序。R02 定向行为测试 `3/3`：重复/异常/取消负例和实际 STA WPF 模板状态均通过。
- 最终隔离 Release 为 XAML `24/24`、构建 `0 warning/0 error`、Core `83/83`、Worker `311/311`、Playnite `528/585` 通过、`57` 跳过、`0` 失败；Light/Dark `buttonbusyprobe` 均 `180→180`、指示器可见且 indeterminate、文本稳定。证据见 [`R02-02-BUSY-WIDTH-20260916.md`](../design/reviews/ui-finesse-round3-20260915/evidence/R02-02-BUSY-WIDTH-20260916.md)。
- 以上是合成 fake 生命周期、真实生产 WPF 样式和 offscreen logical DIP 证据；未验真实 Playnite/Worker 耗时与宿主输入、物理 DPI/跨屏、OS 键盘/IME、presented frame、屏幕阅读器、ETW 或宿主性能。游戏选框、滚动条、命令绑定、取消/错误、恢复保护和 net462 兼容未改。下一可执行项为 R02-03 禁用原因可达。

## 当前第三轮阶段：R02-01 动作优先级

- `89c9cc3` 在当前 `codex/ui-finesse-round2` 分支复用现有按钮族，新增 `GscWpfUiDangerActionButton` 与 `GscWpfUiContextDangerButton`，并将 Save 恢复、Media 来源移除接入共享危险外观；Overview 主页工具栏、当前游戏卡片和 Media Inbox 批量栏各保留一个 Primary 角色。
- Media Inbox 的待归类/已忽略批量主动作通过真实 XAML 条件互斥：已忽略隐藏 `ApplyMediaClassification`，只显示 `RestoreIgnoredMediaBatch`；列表和检查器两处均覆盖。命令、Binding、恢复保护、撤销、picker、滚动条、有限列表和 net462 未改。
- 当前提交 Release 全流程为 Core `83/83/0`、Worker `311/311/0`、Playnite `525/582` 通过、`57` 跳过、`0` 失败；XAML `24/24`，构建 `0/0`；R02 定向行为/样式测试 `2/2`。RenderHarness 绑定 `89c9cc3` 为 `161` 快照、`0` Fidelity、`0` 失败路由、`0 HIGH/0 MEDIUM`。
- 证据见 [`R02-01-ACTION-PRIORITY-20260916.md`](../design/reviews/ui-finesse-round3-20260915/evidence/R02-01-ACTION-PRIORITY-20260916.md)。范围是合成数据、真实生产 WPF 样式和 offscreen logical DIP；未验真实 Playnite 嵌入、用户主题、物理 DPI/跨屏、OS 输入/IME、presented frame、ETW、屏幕阅读器或宿主性能。下一可执行项为 R02-02 忙碌宽度稳定。

## 当前第三轮阶段：R01-08 跳过测试说明

- 当前隔离 Release 全流程为 Core 83/83/0、Worker 311/311/0、Playnite 523/580 通过/57 跳过/0 失败，合计 917 通过、0 失败、57 跳过；XAML 24/24，构建 0 warning/0 error。
- Playnite 计划中常说的 63 项由 57 条 LegacyProductionUiBaselineFact（已撤销今日工作台架构）+ 6 条 NamedPipeFact（IPC/取消/恢复）组成；当前 Named Pipe 可用，6 条实际通过，当前 skip 只有 57。Worker 的 1 条 WorkerProcessFact 也实际通过。
- 证据已明确成功/失败/跳过分开、gated 能力和补测命令；不能为消除 skip 修改业务断言。下一可执行项为 R02-01 动作优先级。

## 当前第三轮阶段：R01-07 基线失效规则

- e1324fe 新增 check-ui-evidence-freshness.ps1、test-ui-evidence-freshness.ps1 和版本化 UI_EVIDENCE_BASELINE.json；按证据源码提交与关联 sourcePaths/scopes 扫描 Git 变更，当前 14 条记录为 7 stale / 7 fresh。
- 真实扫描标记 R00-01-02、R00-04、R00-06、R00-07、R00-08、R01-01、R01-02 需重跑；R01-03～R01-07 当前无匹配源码变更。共享 Redesign.xaml 合成变更只命中 shared-controls/all-pages 的 R00-01-02/R00-05；纯文档变更为 0 重跑、0 重装。
- 输出单独展示 currentSourceCommit 和 currentPackageCommit；当前包身份为 not-provided，合成包 mismatch 只证明分支会要求重装，不是实际 package-host 证据。源码校验和三分支 smoke 通过；下一可执行项为 R01-08 跳过测试说明。

## 当前第三轮阶段：R01-06 宿主证据保全

- 受控 RenderHarness 审计绑定 3929ed73e1056a964d7ceacc54a6046abcc9983e：运行时快照 161、Fidelity 警告 0、失败路由 0、HIGH 0、MEDIUM 0；关键静态计数为 View 10、Tab 32、Button/Toggle 234、DataGrid 14、ScrollViewer 34、条件 UI 235。
- R01-06 已将 AUDIT_SUMMARY、audit-metadata、UI_MANIFEST、UI_ROUTE_MAP、UI_FIDELITY_MATRIX、LAYOUT_REPORT、20 行 EVIDENCE_INDEX 和 6 张精选截图归档到 R01-06-host-evidence-20260916。metadata 的输出路径已改为仓库相对路径，README 给出固定 commit 的全量截图/JSON 重现命令。
- 本项只保全证据，没有修改生产 UI；完整截图与视觉树 JSON 仍为可再生临时输出。证据是受控 WPF 离屏 logical DIP，不等价真实 Playnite 嵌入、用户主题、物理 DPI、OS 输入/IME、presented frame、ETW 或宿主性能。下一可执行项为 R01-07 基线失效规则。

> 更新时间：2026-09-16。本文是新一轮开发的短入口；历史细节仍保留在 [`PROJECT_MEMORY.md`](PROJECT_MEMORY.md)、[`WORKLOG.md`](WORKLOG.md) 和 [`DEVELOPMENT_HANDOFF.md`](../DEVELOPMENT_HANDOFF.md)，但与本文冲突时以本文和最新代码为准。

## 当前第三轮阶段：R01-05 负例注册表

- `5e6d64a` 在测试项目新增 `UiNegativeFixtureRegistryTests`，统一登记 N01～N05 五类有意不合格夹具：对比度 `violations=1`、数值裁切 `horizontalFit=False/verticalFit=True/isReadable=False`、无结果 Enter `handled=False/overlay=Visible/selectionPreserved=True`、子级溢出写 gate、Loading 隐藏重试并阻断底层命中。
- 注册表每项调用实际检测器或受控 WPF Window probe，要求 `detected=True`；夹具只在测试项目和随机隔离临时目录使用，未新增生产入口。它证明检测器能抓住指定错误，不把“检测到问题”误写成产品测试失败。
- 以 `5e6d64aea3a64bc3fcde85627a84ce7cff5759e2` 为身份的 Release 全流程：XAML `24/24`，构建 `0/0`，Core `83/83`，Worker `311/311`，Playnite `523/580`（`57` 跳过、`0` 失败）；注册表定向 `1/1`，源码校验通过。证据见 [`R01-05-NEGATIVE-REGISTRY-20260916.md`](../design/reviews/ui-finesse-round3-20260915/evidence/R01-05-NEGATIVE-REGISTRY-20260916.md)。
- 范围是合成数据、实际 WPF 控件、隔离 Window 和 offscreen logical DIP；不等价真实 Playnite、物理 DPI、OS 输入/IME、presented frame、ETW 或宿主性能。下一可执行项为 R01-06 宿主证据保全。

## 当前第三轮阶段：R01-04 动效行为替代字符串

- `74abb10` 在现有真实 WPF 动效覆盖上新增 `EntranceMotionReentryKeepsTheRenderedBaseWhenLatestClockIsCancelled`：实际 STA `Window` 中启动入场、Dispatcher 采样、重入并清除最新时钟，Y/Opacity 必须保持采样时的有效值；既有完成终态、重入连续性和壳层清理测试继续保留。
- `EntranceMotionTakesOverFromTheCurrentEffectiveValue` 已收窄为结构门禁，只检查 `AnimateEntrance`、活动动画分支、清钟、动画对象、HoldEnd 和 Completed，不再用 `currentY/currentOpacity` 局部源码字符串签收行为。对隔离突变分别删除 Y/Opacity 基值写回后，新增行为测试按预期失败，偏差约 `8.33 DIP` / `0.745`。
- 以 `74abb10096df77cb77ef59128f1898e621475a51` 为身份的 Release 全流程：XAML `24/24`，构建 `0/0`，Core `83/83`，Worker `311/311`，Playnite `522/579`（`57` 跳过、`0` 失败）；动效过滤 `4/4`。证据见 [`R01-04-MOTION-BEHAVIOR-20260916.md`](../design/reviews/ui-finesse-round3-20260915/evidence/R01-04-MOTION-BEHAVIOR-20260916.md)。
- 这是合成 WPF 控件、隔离 Window 与 offscreen logical DIP；不等价真实 Playnite 嵌入、物理 DPI、输入、presented frame、ETW 或宿主性能。下一可执行项为 R01-05 负例注册表。

## 当前第三轮阶段：R01-03 每项证据直达

- `3875f88` 在共享 `UiReportWriter` 接入 `EVIDENCE_INDEX.md`；`UiEvidenceIndexBuilder` 从实际 `UI_MANIFEST`、运行时 `LAYOUT_REPORT` 和 `UI_FIDELITY_MATRIX` 数据抽取具体控件/状态，不复用一条聚合 `passed` 文案覆盖整页。索引固定至少 20 项，优先覆盖生产 DataGrid，再按 route 分散抽取交互控件；`2eb4c46`、`591deae` 校准了生产页面优先、分散抽样、运行时边界方向和源码文件回退。
- `f6a3209` 增加 `scripts/validate-ui-evidence-index.ps1` 及源码契约测试，`9d5146d` 校准 Windows PowerShell BOM、源码身份命令参数和当前 `ButtonChrome=0.72` 守卫。最新提交完整审计生成 20 行：13 个生产 DataGrid + 7 个来自 maintenance/media/save/task/trainer/settings/overview 的控件；每行均有具体结果文件与 `source=src\...` 检索入口、40 位 commit、样本字段和未验边界，校验输出为 `references=20/20, identities=20/20, samples=20/20, boundaries=20/20`。
- 最新受控审计的 `EVIDENCE_INDEX.md`、`AUDIT_SUMMARY.md` 使用完整身份 `9d5146d4a4d604b2779f933f3dee00e730c68b71`；审计 `Fidelity=0`、失败路由 `0`，并明确静态-only 条目没有运行时几何样本。全流程 Release 为构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `521` 通过/`57` 跳过/`0` 失败。
- 证据见 [`R01-03-EVIDENCE-INDEX-20260916.md`](../design/reviews/ui-finesse-round3-20260915/evidence/R01-03-EVIDENCE-INDEX-20260916.md)。这是报告/测试基础设施，不改变生产命令、绑定、picker、滚动条、取消/错误、恢复保护、有限列表或 net462；不等价真实 Playnite 嵌入、物理 DPI、IME、presented frame、ETW 或宿主性能。下一可执行项为 R01-04 动效行为替代字符串。

## 当前第三轮阶段：R01-02 数字单元格裁切

- `acfe1ea` 新增 `NumericCellReadability`，在实际 WPF `DataGridCell` 上分别测量非约束文本宽度、扣除 padding 后的可用内容宽度和行内文本高度；`HorizontalFit`、`VerticalFit` 与 `IsReadable` 不再把仅有行高通过误当成数字完整可读。Demo 校对表将数值列设为 `160 DIP`，固定覆盖 `1,024 / 99,999`、长负数、容量和 TiB 样本，并显式 `NoWrap`/无裁切。
- 新增实际 STA WPF 行为测试 `2/2`：生产校对夹具四个数值全部横向/纵向通过；`56 DIP` 窄列中的长负数保持纵向通过但横向失败，`IsReadable=False`，作为明确负例。Release 全流程为构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `520` 通过/`57` 跳过/`0` 失败。
- clean-tree `acfe1eab48572e538c9d17cbb45b7bdc116942ed` 的 Light/Dark `finesseprobe` 均为 `expected=4 realized=4 horizontalFit=4 verticalFit=4 allReadable=True`，长负数负例 `textWidth=115.26 / available=56 / textHeight=15.33 / cellHeight=52` 且 `horizontalFit=False, verticalFit=True`；对照度违规为 `0`，离屏图像已人工检查。旧 `110 DIP` 基线曾出现首行末位裁切，但原报告没有水平完整性门禁。
- 证据见 [`R01-02-NUMERIC-CELL-READABILITY-20260916.md`](../design/reviews/ui-finesse-round3-20260915/evidence/R01-02-NUMERIC-CELL-READABILITY-20260916.md)。范围是合成 DTO、真实 WPF 控件、隔离窗口与 offscreen logical DIP；不宣称真实 Playnite 嵌入、物理 DPI、IME、呈现帧、ETW 或宿主大库性能。下一可执行项为 R01-03 每项证据直达。

## 当前第三轮阶段：R01-01 测试源码根绑定

- `abb5589` 为 Playnite 源码型测试引入 `GscSourceRoot`/`GscBuildCommit` 程序集元数据和 `TestRepositoryContext`；37 个重复根 helper 与 6 个直接回溯 reader 均改为使用构建绑定根，未知、无效或 checkout/程序集 commit 不一致时清晰失败。
- `scripts/build.ps1` 现在显式传播并恢复 `GSC_SOURCE_ROOT`。当前 worktree 隔离 OutputRoot 全流程为构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `518` 通过/`57` 跳过/`0` 失败；把同一输出放到 main checkout 的 `.tmp` 复跑仍相同，main 源码与 `src.zip` 未改。证据见 [`R01-01-REPOSITORY-IDENTITY-20260916.md`](../design/reviews/ui-finesse-round3-20260915/evidence/R01-01-REPOSITORY-IDENTITY-20260916.md)。
- 代表性身份/源码测试 `14/14`，定向初跑曾发现 7 位短 SHA 与完整 HEAD 的比较校准，改为安全前缀匹配后通过；不是放宽到任意身份。下一可执行项为 R01-02 数字单元格裁切。

## 当前第三轮阶段：R00-08 搜索框 Enter/IME

- `8435d80` 修正生产 `AcrylicProductionShellView.OnPickerPreviewKeyDown`：IME 处理事件先放行；Enter 只确认 `PickerList.SelectedItem` 且该项仍在共享 `GamePicker.ItemsView` 中的候选，不再因旧 `SelectedGame` 存在而关闭；Escape 保持焦点返回，方向键不拦截。
- 当前提交的 Release Playnite `net462` 构建为 `0 warning / 0 error`，R00-08 相关 STA WPF 测试 `28/28`；覆盖无结果 Enter 负例、`Key.ImeProcessed`、四向键、可见候选 Enter、Esc 关闭和 `GameContextButton` 焦点返回。证据见 [`R00-08-PICKER-ENTER-IME-20260916.md`](../design/reviews/ui-finesse-round3-20260915/evidence/R00-08-PICKER-ENTER-IME-20260916.md)。
- 测试使用真实生产 Shell、WPF Window/路由和合成 DTO；最小测试承载未启动 Worker/Playnite，也不等价真实 Windows 中文输入法候选、物理键盘、物理 DPI、presented frame 或 ETW。下一可执行项为 R01-01 测试源码根绑定。

## 当前第三轮阶段：R00-07 审计排除项收窄

- `42f9dca` 删除按 `TrainerToolsSettingsScrollViewer` 整棵子树静默排除的逻辑，按动作按钮/输入控件/显式 Toolbar 语义分类，并在报告保留排除理由、布局/需求宽度、可见交集、溢出和滚动祖先；隐藏状态父级单独记录为状态隐藏。
- 当前 clean-tree `42f9dca61eb23b92e1cf80a615b764e844d5a1d7`：RenderHarness Release 构建 `0/0`，审计源与既有精修定向 `29/29`；`toolbarprobe` 的正常长表单、同祖先超宽动作栏、同祖先不可达动作栏三场景通过，输出 `toolbarprobe OK`。全量审计 `161` 快照、`0` Fidelity、`0` 失败路由、`0 HIGH / 0 MEDIUM`。
- 首轮审计曾把页面滚动位置外的合法动作栏的 `Rect.Empty.Width=-∞` 误作横向溢出；最终只按有效需求宽度与可用宽度判定，并以 clean-tree 全量审计校准通过。真实生产页面未改，现有命令/绑定、滚动与安全语义保持。
- 证据见 [`R00-07-TOOLBAR-EXCLUSION-20260916.md`](../design/reviews/ui-finesse-round3-20260915/evidence/R00-07-TOOLBAR-EXCLUSION-20260916.md)。这是合成/离屏逻辑 DIP 与受控生产视图审计，不等价真实 Playnite 嵌入、物理 DPI、输入、presented frame、ETW 或宿主帧率；下一可执行项为 R00-08 搜索框 Enter/IME。

## 当前第三轮阶段：R00-06 媒体四行门禁

- `7d57575` 将媒体主表的 `212 DIP` 固定门禁改为按实际表头、行高、水平滚动条、边框和 padding 计算的运行时 floor；`UiLayoutAnalyzer` 按真实 `DataGridColumnHeadersPresenter`、`DataGridRow` 与有效裁剪交集计数完整行，另有短窗回退和父级不可达的明确诊断。`db5d483` 让几何探针报告记录完整提交号与工作树状态。
- 当前 clean-tree `db5d483d8ac6460ac7c3a07fe64cec5c7fe417d3`：Playnite 定向构建 `0/0`，R00-06 几何/源契约定向 `4/4`，RenderHarness 构建 `0/0`；双主题 `mediageometryprobe` 的正常、水平条、不同密度、短窗回退和父级裁剪负例共 10 场景通过，输出 `mediageometryprobe OK`。完整审计 `161` 快照、`0` Fidelity、`0` 失败路由、`0 HIGH / 0 MEDIUM`；`shellqa` exit 0。
- 默认密度有水平条时所需主表为 `262 DIP`（`42 + 4×52 + 12`），表格框为 `288 DIP`（另加 `24` padding 与 `2` border）；不同密度实际测到 `36/44 DIP` 并按公式得到 `212 DIP`，不是恢复旧固定值。短窗保留页级可达性并记录 INFO；阻断父级的 `pageScroll=False` 记录 HIGH 负例。
- 证据见 [`R00-06-MEDIA-FOUR-ROWS-20260916.md`](../design/reviews/ui-finesse-round3-20260915/evidence/R00-06-MEDIA-FOUR-ROWS-20260916.md)。这些是合成数据、真实生产 WPF 视图和离屏 logical DIP 证据，不等价真实 Playnite 嵌入 Dashboard、物理 DPI、鼠标滚轮/键盘输入、presented frame、ETW 或宿主帧率；下一可执行项为 R00-07 审计排除项收窄。

## 当前第三轮阶段：R00-04 搜索基准真实性

- `df884b0` 修正搜索基准：正式 30 次输入使用不同的 `SearchText`，等待条件核对真实可见 `PlayniteId` 集合而不是只核对计数；每次等待拥有独立运行的超时计时器，并保留不可能结果的有限超时负例。
- 当前隔离 worktree 的 Release 构建为 `0 warning / 0 error`，R00-04 定向测试 `2/2`；合成 2,000 项结果集合 `30/30` 发生变化，原始查询和时延样本已记录，p50/p95/max=`45/60/60ms`。
- 这是 fake 服务/合成数据下的受控 WPF 逻辑证据，不替代真实 Playnite 输入、连续打字、IME、物理 DPI、屏幕呈现帧或 ETW；R18-01 继续补 debounce/分配和 IME，下一执行点为 R00-05 上下文禁用透明度。

证据：[`R00-04-SEARCH-BENCHMARK-20260916.md`](../design/reviews/ui-finesse-round3-20260915/evidence/R00-04-SEARCH-BENCHMARK-20260916.md)。

## 当前第三轮阶段：R00-05 上下文按钮禁用透明度

- `aebcefc` 移除 `GscWpfUiContextButton` 的控件级 `Opacity=0.48`，保留共享模板唯一的 `ButtonChrome.Opacity=0.72`；因此 Context/RemoteRestore/MediaBatch 派生动作的禁用标签、图标和解释文字不再二次淡化，仍保留布局位置。
- Light/Dark 真实 WPF Window/Dispatcher 定向 `2/2`；三类生产派生样式启用/禁用高度差 `<0.01 DIP`，复合内容前景非透明，受控运行时渐变合成的最低禁用文本对比度为 `3.0`。SaveCenter/MediaCenter/Maintenance 引用校验通过。
- 这是受控模板/逻辑与语义合成证据，不是 Playnite 宿主屏幕像素、物理 DPI 或鼠标交互证据；命令、绑定、恢复保护和布局契约未改。下一执行点为 R00-06 媒体四行门禁。

证据：[`R00-05-CONTEXT-DISABLED-20260916.md`](../design/reviews/ui-finesse-round3-20260915/evidence/R00-05-CONTEXT-DISABLED-20260916.md)。

## 当前第三轮阶段：R00-01/R00-02 代码收口

- 当前工作区为 `codex/ui-finesse-round2`，最新实现提交 `a95e900`，非等距渐变负例测试补充为 `e216e9b`；没有合并 `main` 的旧实现。R00-01 按整组 chrome opacity 与真实父背景计算，保留 `GradientStop.Offset` 并覆盖 focus/hover/pressed 组合；R00-02 对已有组合变换递归复用 ScaleTransform，冻结树只在第一次接入时克隆。
- 当前 SHA `e216e9bf0d2ed18adced62936d6897b8d84f0d58` 的 Release 定向 Playnite WPF 测试为 `5/5`，RenderHarness 双主题 `finesseprobe` 均 `WorkingTreeClean=True`、`SemanticButtonContrast samples=88`、`violations=0`、`finesse-fixture OK`。证据见 [`R00-01-02-CONTRAST-SCALE-20260916.md`](../design/reviews/ui-finesse-round3-20260915/evidence/R00-01-02-CONTRAST-SCALE-20260916.md)。
- 这些是受控 STA/offscreen logical DIP 证据，不替代真实 Playnite 鼠标按压、IME、物理 DPI、屏幕呈现帧或可变 Freezable 跨实例所有权；R00-03 已补完成/取消/卸载/重入 Dispatcher 复核，下一项为 R00-04 搜索基准。

## 当前第三轮阶段：R00-03 动效生命周期行为收口

- `4414f05` 的生产终态实现通过 `cda168c` 新增行为测试：真实 WPF Window/Dispatcher 上完成后清钟并保持 `Opacity=1/Y=0`，中途重入从当前有效值接续，活动侧栏切换 reduced motion 后立即归一，等待旧时长不会晚写覆盖；完成与卸载回归继续覆盖。
- 当前 clean-tree `cda168ca410bee0a4b0416664452010c9246db96` 的 R00-03 定向测试为 `5/5`；`motionreentryprobe` 与 `motionhotprobe` 双主题均 exit 0，终态/取消态分别为 `270/X=0` 与 `72/Opacity=1/X=0`，证据见 [`R00-03-MOTION-LIFECYCLE-20260916.md`](../design/reviews/ui-finesse-round3-20260915/evidence/R00-03-MOTION-LIFECYCLE-20260916.md)。
- 仍未宣称真实 Playnite 输入、Windows 偏好通知、ETW、物理 DPI/呈现帧；下一可执行任务是 R00-04。

## 当前最近阶段：Q18-03 动效当前值接管受控证据

- `bdb99b9` 的 clean-tree `motionreentryprobe` 使用真实生产 `AcrylicProductionShellView` 在 Light/Dark、900×640 DIP 中先中断收起动画，再立即触发展开意图；即时宽度与中断宽度差异仅 `0.02/0.16 DIP`，重入中间态继续向 `270` DIP 展开，终态 X=0 且无活动动画。
- 八张截图和原始报告见 [`Q18-03-MOTION-REENTRY-20260915.md`](../design/reviews/ui-finesse-round2-20260913/evidence/q13-q25/Q18-03-MOTION-REENTRY-20260915.md)。Q18-03 受控视觉列已通过；真实 Playnite 快速输入、宿主时序、物理 DPI 和屏幕帧仍未签收。

## 上一阶段：Q18-07 动效 Loaded/Unloaded 生命周期受控证据

- `0815871` 的 clean-tree `motioncycleprobe` 使用同一真实生产 `AcrylicProductionShellView` 在 Light/Dark、900×640 DIP 各执行 100 次 Loaded/Unloaded 循环；每次在侧栏动画中间态卸载并重载，事件计数均为 `101/101`，最终 `finalLoaded=False`、`transitionRunning=False`、Opacity `1`，没有活动侧栏时钟。
- 六张截图和原始报告见 [`Q18-07-MOTION-CYCLE-20260915.md`](../design/reviews/ui-finesse-round2-20260913/evidence/q13-q25/Q18-07-MOTION-CYCLE-20260915.md)。Q18-07 受控视觉列已通过；夹具未构造真实 Dashboard VM/Worker 订阅，真实 Playnite 宿主关闭、物理 DPI/屏幕帧和 ETW 生命周期仍未签收。

## 上一阶段：Q18-05 系统动画热变更受控证据

- `dc1dd67` 的 clean-tree `motionhotprobe` 使用真实生产 `AcrylicProductionShellView` 在 Light/Dark、900×640 DIP 中启动侧栏动画，再在活动中间态切换 MotionEnabled 为关闭；两主题均确认中间态存在活动动画，归一化后宽度 `72`、Opacity `1`、X `0` 且无活动时钟，禁用重入立即恢复到 `270` DIP。
- 报告绑定完整 SHA、`WorkingTreeClean=True`、DPI `1.00`，六张截图和原始报告见 [`Q18-05-MOTION-HOT-CHANGE-20260915.md`](../design/reviews/ui-finesse-round2-20260913/evidence/q13-q25/Q18-05-MOTION-HOT-CHANGE-20260915.md)。Q18-05 受控视觉列已通过；真实 Windows 偏好通知、Playnite 宿主、物理 DPI/屏幕帧和性能边界仍未签收。

## 上一阶段：Q18-04 生产壳层动效视觉序列

- `af9b1dd` clean tree 的 `motionprobe` 以真实生产 `AcrylicProductionShellView` 运行 Light/Dark、900×640 DIP，报告绑定完整 SHA 与 `WorkingTreeClean=True`；两主题均保留中间态、终态、快速重入终态及卸载后检查，代表 PNG 已人工查看。
- 审计报告记录中间态宽度约 `100/153 DIP`、Opacity `0.858/0.592`，终态/卸载均为 `72/1/0` 且无活动动画；700ms 只覆盖审计资源，不改变生产动画 token。Q18-04 视觉列已通过；真实宿主 Loaded/Unloaded、ETW 和物理屏幕帧，以及 Q18-07 的宿主边界仍未完成。

## 上一阶段：Q17-04 受控状态夹具视觉复核

- `Q17-04-STATE-FIXTURES-20260915.md` 的 RenderHarness Release 报告包含双主题、四种 DIP 尺寸下 `160` 张截图/`160` 条 fixture 记录；已人工查看浅色 Loading、深色 Stale、深色 Offline 和浅色 Stale 下一步运维四张代表图，状态覆盖层、横幅、说明、按钮和正文可读。
- 因此 Q17-04 视觉列已升级为通过；真实 Worker 进度节奏、动画停止、悬停/卸载时序和 Playnite 宿主像素仍是外部阻塞，结论仍未完成。真实宿主事实继续以下方 `37f92f7` 成功 Embedded 证据为准。

## 上一阶段：当前提交真实宿主嵌入复核

- `37f92f7` 已在全新隔离 UserData、扩展目录和 Worker IPC 下完成 Release 真实宿主审计；当前程序集身份统一为 `0.6.73+37f92f7f107880bb5a33f61c82eee11fe1344874`。
- `artifacts/ui-host-audit-round2-fp-final6-20260915/summary.json` 记录 Dashboard/Settings 均为 `EmbeddedPlaynite`，`ProductionVisualSourceOfTruthAvailable=true`、`HighGateCount=0`；捕获 29 个 Dashboard 视口、2 个滚动面、1 个 Settings 视口，150% DPI，代表 PNG 已人工复核。
- 本次宿主使用隔离 Worker，结束后调用 Playnite 官方 `--shutdown --userdatadir` 清理；用户扩展目录旧 Worker PID `23304` 未触碰。Q24-03 仍因仅有 `DISPLAY1` 外部阻塞，短窗、IME/读屏、真实组合输入和 ETW 边界继续保留。

## 当前补充：非空隔离库宿主边界复核

- 在文档 HEAD `0fb597e` 上以只写入 `.tmp` 的 3 个游戏库重跑真实宿主审计；清理旧运行态后仍在 Playnite 主窗口前触发 CEF `mojo platform_channel` `Access denied (0x5)`。
- `artifacts/ui-host-audit-round2-fp-data2-20260915/host-startup-blocker.json` 明确无视觉证据、无 `summary.json`，因此 Q20-01～Q20-08 不升级；这次失败不覆盖 `37f92f7` 的空库 Embedded 成功证据。用户 Worker `23304` 未结束。

## 上一阶段：真实宿主启动阻断结构化

- 在 `9654c05` 之后增强 `scripts/real-host-audit.ps1`：保留隔离 Playnite 启动进程句柄，检测“主窗口前退出 + CEF 启动日志”并写出 `host-startup-blocker.json`；报告明确 `VisualEvidenceCaptured=false`、`CountsAsVisualPass=false`，不写入 `gates/`。
- 2026-09-15 受控诊断额外使用 `--no-sandbox --disable-gpu` 仍复现 CEF `mojo platform_channel` `Access denied (0x5)`；该参数只用于根因隔离，没有进入生产配置。PowerShell AST 解析和新增宿主证据源契约测试通过。
- 该阶段改善了早退证据可追溯性；`cc63523` 的 CEF 早退记录仍保留为失败边界，但已被 `37f92f7` 的成功隔离宿主捕获 supersede，不把失败目录写成当前视觉证据。

## 当前最近阶段：生产按钮忙态反馈

- `4947539` 已推送 `codex/ui-finesse-round2`：生产 `ui:Button` 新增 `IsBusy` 依赖属性和共享 indeterminate `BusyIndicatorHost`，Dashboard 顶部刷新、全部备份、媒体同步复用真实 `DashboardViewModel.IsBusy`；原 ContentPresenter 与按钮测量槽保持不变。
- `buttonbusyprobe` 在 STA、96 DPI 下完成 Light/Dark 双主题运行，两个主题均报告宽度 `180→180`、指示层可见、indeterminate、文字保持“全部备份”；源码定向测试 `17/17`，RenderHarness Release `0 warning/0 error`，截图与报告见 Q04–Q12 证据索引。
- 该阶段只升级 Q05-05/Q05-06 共享模板/受控视觉列；真实 Playnite 命令耗时、宿主输入、真实 DPI 和其它 Q06 状态序列仍不宣称完成。当前账本统计需以 `ROUND2_PROGRESS.md` 的逐列解析为准。

## 当前最近阶段：按钮卸载状态清理

## 2026-09-15 当前提交真实宿主审计边界

- `c5a2997` 的隔离 `real-host-audit.ps1` 已捕获当前提交 EmbeddedPlaynite Dashboard/Settings：Dashboard 29 个视口、2 个滚动面、Settings 1 个视口，150% DPI；构建 0/0，Core `83/83`、Worker `311/311`、Playnite `499/556`（57 skip），提交 SHA 已写入 metadata。
- 本轮隔离启动复用了用户扩展目录内仍运行的旧 Worker PID `23304`，路径为 `C:\Users\lopmatu\AppData\Roaming\Playnite\Extensions\GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec\Worker\GameSaveCenter.Worker.exe`，身份 `0.6.73+6450f6...`，与当前插件 `0.6.73+c5a2997...` 不一致；截图出现真实 Worker 失败 Toast，`HighGateCount=1`。
- 不把这组带旧 Worker 的截图写成当前提交全链路通过；只有在用户明确允许停止该精确旧 Worker 后，才重跑审计并要求 `HighGateCount=0`。持久边界证据见 `docs/design/reviews/ui-finesse-round2-20260913/evidence/q13-q25/REAL_HOST_AUDIT-CURRENT-20260915.md`。

- `7804431` 已推送：生产自定义 Button 订阅 `Unloaded`，清除 `ButtonChrome` 的活动 Opacity/Scale 动画，并将 Hover/Pressed/Focus 覆盖层动画与 Opacity 归零；随后 `132e6d5` 增加冻结 `ScaleTransform` 保护，避免无活动动画的模板实例在卸载清理时抛异常。
- `132e6d5` 已推送：完整 clean-tree RenderHarness `render-qa OK`，解决方案 Release 构建 `0/0`，`UiFinesseRound2ControlSourceTests=19/19`，源码/XAML 门禁通过。
- 这是 Q06-02 的生命周期实现修复，不等价真实鼠标按下/移出/失焦/禁用/卸载序列；当前视觉待验与宿主外部边界继续按账本记录。

## 当前最近阶段：设置主题打开态运行时夹具

- `a1f3cae` 已推送 `codex/ui-finesse-round2`：新增 `settingsthemeprobe` 与完整 RenderHarness 调用，使用真实 Settings view 在 STA WPF 隐藏宿主窗口中打开 ComboBox Popup 和 ToolTip，在保持打开时执行 Light→Dark 切换。
- 聚焦输出与 clean-tree 完整 `render-qa` 均通过；探针报告 `popupOpen=True`、`tooltipOpen=True`、主文字 `#F21B1F27→#FFF2F4F8`，并人工复核 6 张截图。Q03-07/Q23-07 视觉列已升级为通过。
- 这是受控 WPF 视觉证据，不等价真实 Playnite Dialog/Inspector、屏幕闪白帧、宿主主题跟随、保存/取消提交、物理 DPI 或跨屏 Popup；Q24-03 单屏阻塞和其余宿主边界保持不变。

## 当前最近阶段：真实宿主最终复核已完成

- 当前交付基线为 `69e1f84`，已推送 `codex/ui-finesse-round2`。clean-tree Release real-host audit 通过：XAML `24/24`、Core `83/83`、Worker `311/311`、Playnite `494/551`（57 skip）、0 fail。
- 最终证据目录为 `artifacts/ui-host-audit-round2-final-20260915`：真实 `EmbeddedPlaynite` Dashboard/Settings 均捕获，29 个 Dashboard 视口、2 个完整滚动面、1 个 Settings 视口；runner metadata、Settings metadata、构建身份均绑定完整 SHA `69e1f844f8b20b1fcf1667d2b8a6af2772eb0ed4`。
- Settings 150% DPI `1278×762 DIP` 截图已人工复核为稳定可读；Q24-03 仍只有 `\\.\DISPLAY1`，因此物理跨屏 Popup、完整键盘/IME/读屏、ETW 呈现帧和真实耐久边界不能升级为完成。

## 当前最近阶段：真实宿主审计身份

- 已发现并修复 `real-host-audit.ps1` 在 PowerShell pipeline 后误用 `$LASTEXITCODE` 的证据问题；下一轮审计必须将 Git SHA 写入 runner metadata、插件 Settings metadata 和构建身份。
- 当前未提交改动仅为该脚本、证据源回归测试和 AI 记忆更新；Release Playnite 编译、定向 `10/10`、源码/XAML/diff 门禁均已通过。
- 上一轮 `ui-host-audit-round2-settled-20260915` 的 Settings 图像视觉稳定可读，但因 metadata SHA 为 `unknown` 只能作为时序修复复核，不作为最终可追溯证据；提交后会重跑。

## 当前最近阶段：真实 Settings 宿主截图时序

- 本阶段修复真实 Playnite 审计过早截图的问题：Settings 的入场动画从 `Opacity=0` 开始，审计现在等待实际 Slow motion token 完成后才捕获稳定终态；正常用户动画和 Settings 绑定未改变。
- Release 串行编译 0 警告/0 错误；`UiAuditBlockerTests` `8/8`；审计构建阶段 Core `83/83`、Worker `311/311`、Playnite `494/551`（57 skip）、0 失败。提交后还需重新安装 clean tree 并复核 Settings PNG。
- 旧真实审计的 Settings 暗图是本次问题复现对照；不能把旧图作为最终视觉签收。Q24-03 当前机器仍只有 `\\.\DISPLAY1`，物理跨屏继续阻塞。

## 2026-09-15 Q24-03 物理跨屏前置复核

- 当前分支生产代码基线为 `39e37b1`，本次前置复核时文档 HEAD 为 `7609c4a`；`System.Windows.Forms.Screen.AllScreens` 仍只发现 `\\.\DISPLAY1`，边界 2560×1440、工作区 2560×1368。
- `DiagnosticsEvidenceSourceTests` + `UiFinesseRound2ControlSourceTests` 共 `17/17` 通过，`real-host-audit.ps1` PowerShell 语法解析通过；没有执行或伪造第二屏宿主迁移、打开态 Popup 和截图。
- Q24-03 仍是条件阻塞：需要第二个物理显示器和可见 Playnite 宿主；证据见 [`Q24-03-PHYSICAL-CROSS-SCREEN-20260915.md`](../design/reviews/ui-finesse-round2-20260913/evidence/q13-q25/Q24-03-PHYSICAL-CROSS-SCREEN-20260915.md)。

## 2026-09-15 UI 精修 Q18 生产侧栏卸载清理回归

- 当前交付提交为 `39e37b1`，已推送 `codex/ui-finesse-round2`。`AcrylicProductionShellView.OnUnloaded` 现在取消侧栏位移动画后写回 `TranslateTransform.X = 0`；新增 `ProductionShellChromeSourceTests.SidebarTransitionReleasesClocksOnCompletionAndUnloadInAnActualWpfWindow`，在真实 STA WPF `Window` 中覆盖侧栏动画完成和窗口关闭卸载清理。
- Debug 构建 0 warning/0 error；定向 Playnite 门禁为 `165` 通过、`39` 跳过、0 失败（204 总计）；最终 Release 全量为 Core `83/83`、Worker `310/311`（1 skip）、Playnite `487/550`（63 skip），失败 `0`；侧栏回归 Release 独立回放 `5/5`，源码、XAML 和 diff 门禁通过。
- 该证据仍是受控 WPF Window，不等价真实 Playnite 100 次 Loaded/Unloaded、宿主窗口关闭、Rendering/ETW 或物理屏幕帧；Q18-04/Q18-07 的真实宿主/视觉列继续待验。证据见 [`Q18-04-07-MOTION-CLEANUP-20260915.md`](../design/reviews/ui-finesse-round2-20260913/evidence/q13-q25/Q18-04-07-MOTION-CLEANUP-20260915.md)。

## 2026-09-15 UI 精修 Q18 动效终态运行时门禁

- 当前交付提交为 `68b49a1`，已推送 `codex/ui-finesse-round2`。新增 `UiFinesseFoundationTests.MotionAnimationsReleaseClocksAtTheirFinalValues`，在 STA WPF `Window`/PresentationSource 宿主中实际推进 `AnimateTranslate` 与 `AnimateEntrance`，完成后确认 X/Y/Opacity 为终值且不再有活动动画时钟；Release 连续回放 `5/5` 通过。
- 定向 Playnite 门禁为 `164` 通过、`39` 跳过、0 失败（203 总计）；最终 Release 全量为 Core `83/83`、Worker `310/311`（1 skip）、Playnite `486/549`（63 skip），失败 `0`；源码和 XAML 门禁通过。
- Q18-04/Q18-07 因新增受控 WPF 运行时证据得到加强，但真实 Playnite Loaded/Unloaded 循环、窗口关闭、Rendering/ETW 与物理屏幕帧仍保持待验。证据见 [`Q18-04-07-MOTION-CLEANUP-20260915.md`](../design/reviews/ui-finesse-round2-20260913/evidence/q13-q25/Q18-04-07-MOTION-CLEANUP-20260915.md)。

## 2026-09-15 UI 精修 Q16-08/Q17-07/Q18-04/Q18-07 动效生命周期收口

- 当前生产实现提交为 `4414f05`，运行时回归提交为 `68b49a1`，均已推送 `codex/ui-finesse-round2`。`GscMotion`、Dashboard 和生产壳层动效在完成回调中清理时钟并写回终态；对话框关闭/重开使用代际失效，Dashboard 卸载归一化页面动效，Toast 卸载逐卡释放 Timer/动画。
- 最终 Release 全量为 Core `83/83`、Worker `310/311`（1 skip）、Playnite `486/549`（63 skip），失败 `0`；定向动效/WPF 源码与受控运行时测试 `164` 通过、`39` 跳过、0 失败，源码和 XAML 门禁通过。
- Q16-08/Q17-07/Q18-04/Q18-07 仅实现列升级为代码完成；真实 Playnite Loaded/Unloaded 循环、窗口 close、宿主动画时序、Rendering/ETW 与物理屏幕帧仍保持待验。证据见 [`Q18-04-07-MOTION-CLEANUP-20260915.md`](../design/reviews/ui-finesse-round2-20260913/evidence/q13-q25/Q18-04-07-MOTION-CLEANUP-20260915.md)。

## 2026-09-15 UI 精修 Q17-04/Q18-03 状态夹具与动画重入修复

- 当前交付提交为 `c76ce62`，已推送 `codex/ui-finesse-round2`。`GscMotion.AnimateEntrance` 现在只在没有活动动画时初始化起点；重入时先捕获有效 Transform/Opacity、移除旧时钟，再从当前值继续，避免快速切换闪回。
- `ProgressBar` 的不确定进度模板补齐 `StopStoryboard` 回归门禁；定向 `UiFinesseFoundationTests` + `UiFinesseRound2ControlSourceTests` 为 `18/18` 通过。
- RenderHarness Release 重建为 `0 warning / 0 error`，当前代码重新执行 `statefixtures` 得到 `statefixtures OK`：160 条记录/160 张截图，覆盖四个生产状态页、适用的六态、双主题和四个尺寸；代表证据见 [`Q17-04-STATE-FIXTURES-20260915.md`](../design/reviews/ui-finesse-round2-20260913/evidence/q13-q25/Q17-04-STATE-FIXTURES-20260915.md)。真实进度节奏、宿主动画时序和 Playnite 像素仍保持待验。
- 随后在当前修复代码上执行串行 Release 全量测试：Core `83/83`、Worker `310/311`（1 skip）、Playnite `484/547`（63 skip），失败 `0`；新增两项回归门禁使 Playnite 总数从历史 `545` 增至 `547`。

## 2026-09-15 UI 精修 Q18-01 资源宿主解析收口

- 当前交付提交为 `8dfe7fa`，生产动画入口不再把 `GscMotion.Fast/Normal/Slow/Press` 静态时长直接传入 UI；`AnimateTranslate`、`AnimateEntrance`、侧栏、状态胶囊、对话框和 Toast 均按实际视觉宿主读取局部 motion token。
- 定向测试覆盖 `202/202`（其中 39 项既有宿主布局边界按规则跳过），Release 全量为 Core `83/83`、Worker `310/311`（1 skip）、Playnite `485/548`（63 skip），失败 `0`；源代码和 XAML 门禁均通过。
- 证据见 [`Q18-01-MOTION-HOST-20260915.md`](../design/reviews/ui-finesse-round2-20260913/evidence/q13-q25/Q18-01-MOTION-HOST-20260915.md)。真实热切换、系统动画偏好和宿主像素仍保持待验。

## 2026-09-15 UI 精修 Q12-08 双主题业务空表复核

- 当前 clean-tree 提交 `77f4dc5` 新增 RenderHarness 的 `emptytables` 夹具入口、全量空表数据清理和空态断言；Fake 数据使用 `WorkspaceFixtureState.Empty`，补齐 Trainer 的三个 loading 绑定，避免测试夹具制造假空白。
- 受本机仅有 .NET 9 SDK、项目要求 .NET 8 SDK 且 ProjectReference workload 解析失败的环境限制，本轮用临时 direct-reference WPF runner 重建 Fake 数据并加载 production views/XAML；Light/Dark、1040×700 与 1600×900 均运行 `emptytables OK`。
- Q12-08 视觉列已更新为通过；证据见 [`Q12-08-EMPTY-TABLES-20260915.md`](../design/reviews/ui-finesse-round2-20260913/evidence/q04-q12/Q12-08-EMPTY-TABLES-20260915.md)。真实 Playnite/Worker 空结果、Popup、物理 DPI、键盘和读屏仍保持宿主边界。

## 2026-09-15 UI 精修 Q02-06 生产路径视觉复核

- 当前 clean-tree 提交 `82cf066` 的 `RenderHarness audit` 在 1440×900 与 1040×700 DIP 下复核 SaveCenterView“路径与校验”页；候选路径列和右侧详情均能读到完整技术路径，路径 TextBlock 为 260 DIP，详情路径为 321.33 DIP。
- Q02-06 视觉列已更新为通过；真实 Playnite Tooltip、复制原值、中文长路径最终截断、物理 DPI 和宿主字体差异仍保持外部边界。证据见 [`Q02-PATH-RENDER-20260915.md`](../design/reviews/ui-finesse-round2-20260913/evidence/q02/Q02-PATH-RENDER-20260915.md)。

## 2026-09-15 UI 精修 Q02-07 窄窗正文尺寸视觉复核

- 当前 clean-tree 提交 `3811673` 的 `RenderHarness audit` 复核了 1040×700 DIP 下 Overview、Save、Media、Maintenance、Trainer、Task 六个生产工作区；重要标题、正文、状态、数字和操作保持可读，审计 `HIGH=0 / MEDIUM=0 / Fidelity=0`。
- Q02-07 视觉列已更新为通过；Settings 专页本次审计路由未加载有效内容，因此不扩大结论。真实宿主小窗口、物理 DPI、字形和 IME 仍保持外部边界。证据见 [`Q02-TEXT-SIZE-RENDER-20260915.md`](../design/reviews/ui-finesse-round2-20260913/evidence/q02/Q02-TEXT-SIZE-RENDER-20260915.md)。

## 2026-09-15 UI 精修 Q00 深色设置前景回归与 Q21/Q23 视觉证据

- 当前交付基线为 `f193423`，已推送 `codex/ui-finesse-round2`。设置页错误详情 Expander 标题已显式使用 `GscPrimaryTextBrush`，修复了页面根前景存在但标题模板仍可能落回暗色默认黑字的真实漏检；随后补齐了精修夹具的提交身份元数据、排序箭头双状态证据和零值/未知值语义证据。
- Release 全量验证为 XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `310/311`（1 skip）、Playnite `481/544`（63 skip、0 fail）。当前 clean-tree RenderHarness 双主题、多窗口尺寸和设置 normal/dirty/invalid 夹具 `render-qa OK`，DPI 仅为离屏 DIP `1.00`。
- 证据见 [`Q00-INDEX.md`](../design/reviews/ui-finesse-round2-20260913/evidence/Q00-INDEX.md) 和 [`Q21-Q23-RENDER-20260915.md`](../design/reviews/ui-finesse-round2-20260913/evidence/q13-q25/Q21-Q23-RENDER-20260915.md)。Q21-03/07/08、Q23-01/04 的视觉列已更新；真实 Playnite、物理 DPI、键盘完整路径、IME、读屏、保存中/失败过渡与主题 Owner 生命周期仍不能由离屏证据替代。

## 2026-09-15 UI 精修 Q01/Q02 当前字体与数字夹具身份复核

- 当前代码基线为 `f193423`。RenderHarness `finesseprobe` 报告现在自带 `Scenario`、`Commit`、`WorkingTreeClean`、`DpiScale`、主题和数据范围，避免把未绑定 SHA 的历史夹具当作当前证据。
- Dark/Light 当前 clean-tree 夹具均 `finesse-fixture OK`：有效文本对比 12 个样本 0 violation，黑字负例 1 violation，行完整性 4/4、压缩负例 3/4，`PunctuationSamples`、`NumericMetrics`、混排路径和状态样本均重新记录。
- Q01-04、Q02-03、Q02-08 的视觉列已据此升级；物理 DPI、真实 GlyphRun、IME、Tooltip 和八入口宿主列观感仍保持边界。
- 随后全量 Release 门禁仍为 XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `310/311`（1 skip）、Playnite `481/544`（63 skip、0 fail）；报告已脱离 `.tmp` 临时截图路径并随证据目录保存。
- 当前生产 `SaveHistoryGrid` audit 又确认文件数/大小 16 个数值 TextBlock 全部右对齐，大小样本无裁切；Q02-02 视觉列已更新，真实宿主字体/DPI 与排序点击仍待验。
- `edgevalues` 双主题夹具又覆盖 `0 B`、`未知大小`、`尚未检查` 与 `文件 0/0 · 大小 0 B/0 B`；Q02-04 视觉列已更新，八入口业务盘点和宿主状态切换仍待验。

## 2026-09-15 UI 精修 Q12-07 表头排序箭头双状态夹具

- 当前代码基线为 `2f3d17b`。RenderHarness `finesseprobe ... sorted` 只在开发夹具中设置第一列升序、第二列降序，不改变生产排序逻辑；共享生产表头保留 22 DIP 排序槽，实际箭头宽度 14 DIP，降序旋转 `180°`。
- Dark/Light clean-tree 报告均为 `finesse-fixture OK`、`WorkingTreeClean=True`，截图可见 `名称` 上箭头与 `数值` 下箭头，未压缩表头文字。
- Q12-07 视觉列已更新；真实业务排序点击、排序键/结果和宿主输入序列仍保持待验。

## 2026-09-15 UI 精修 Q15 Tooltip、Popup 与浮层主题自动门禁

- 当前代码基线为 `86ac336`。Dashboard、生产壳层和独立设置页统一声明 Tooltip 初次延迟 `350 ms`、快速切换间隔 `100 ms`；Combo Popup 两套生产模板显式 `StaysOpen=False`，并保留 Bottom 定位、有限高度、自动滚动、键盘方向导航隔离和动态主题资源。
- 新增 Tooltip/Popup 源码契约与 WPF STA 主题隔离测试：浅色 Dashboard、深色 Settings 使用独立资源字典，Popup 资源随 Owner 变化，设置材质不写入 Dashboard 或 Owner 外部字典。Playnite 全量 `481/544`（63 skip，0 fail），Release `0/0`，XAML `24/24`。
- 证据见 [`Q15-TOOLTIP-POPUP-THEME-20260915.md`](../design/reviews/ui-finesse-round2-20260913/evidence/q13-q25/Q15-TOOLTIP-POPUP-THEME-20260915.md)。Q15-03/Q15-07/Q15-08 自动验证已更新为通过；真实悬停/边缘定位/子菜单/打开态主题热切换/跨屏宿主仍待验，不能由 STA 或源码测试替代。

## 2026-09-15 UI 精修 Q25-03 UI 动作热点边界专项

- 当前代码基线为 `ed97d2c`。`RenderHarness enduranceprobe` 现在记录每个动作周期耗时、p95、最大值和 `>100 ms` 计数，并在超阈值动作完成后最多保留 8 条当前线程栈。
- 干净 120 秒受控 WPF 运行完成 `120.4s/120s`、`254` 循环、`886` 动作、`13` 样本、0 动作异常；动作 p95/max=`249.94/483.44 ms`，`>100ms=254/254`。原始报告为 [`.tmp/q25-03-hotspot-clean-20260915/enduranceprobe-report.txt`](../../.tmp/q25-03-hotspot-clean-20260915/enduranceprobe-report.txt)，专项说明见 [`Q25-03-UI-HOTSPOT-20260915.md`](../design/reviews/ui-finesse-round2-20260913/evidence/q13-q25/Q25-03-UI-HOTSPOT-20260915.md)。
- 栈在动作返回后的 `finally` 中读取，报告明确写作 `captured after action completion`；它不是 ETW/PerfView 停顿期间采样调用栈。Q25-03 的受控动作边界已补齐，但真实 Playnite 宿主热点、前后同环境优化回归仍受 ETW/等价性能工具条件阻塞。

## 2026-09-15 UI 精修 Q25-02 呈现回调代理专项

- 当前代码基线为 `a1462ee`。生产壳层 `shellqa` 现在记录 `CompositionTarget.Rendering` 相邻回调的 p95、最大间隔和 60Hz 慢帧比例；干净报告的单次侧栏为 `66.1/193.2/0.147`，快速二次切换为 `15.3/28.5/0.038`，无动画原子终态为 `36.1/36.1/0.250`。
- 该数据明确标记为 WPF Rendering 回调代理，不是屏幕呈现帧；当前主机 xperf DWM 会话仍被 `0x5 / Access denied` 拒绝，Q25-02 真实呈现 p95/最大值/慢帧比例和 Q25-03 调用栈仍待性能工具条件。

## 2026-09-15 UI 精修 Q25-04 30 分钟耐久专项

- 当前代码基线为 `7d7e2cb`。新增 `enduranceprobe`，在真实 WPF STA Window 中循环六个生产工作区、Light/Dark 主题、Media 预览分段、列表/表格选择和详情入口；默认 1800 秒运行完成 `1800.4s`、`3847` 循环、`13462` 动作、`176` 样本、0 动作异常，耐久探针本身不调用强制 GC。
- 干净报告为 [`.tmp/endurance-clean-20260915/enduranceprobe-report.txt`](../../.tmp/endurance-clean-20260915/enduranceprobe-report.txt)，私有字节趋势约 `348,564.72 bytes/min`，预热后线程 `22–25`、句柄 `1170–1177`，中后段资源为有界波动。该证据是受控 WPF Window，不等于 Playnite 嵌入宿主、真实输入/图片/网络任务或目标机合成；Q25-04 自动验证通过，宿主仍外部阻塞。

## 2026-09-15 UI 精修 Q25-05 低性能回退专项

- 当前代码基线为 `97dd0cd`。新增 `lowcostprobe`，在 `glass=false`、`motion=false` 下覆盖 Overview、Save、Trainer、Media、Maintenance、Task 六页，浅/深色各覆盖 `1040×700` 与 `1600×900`，24/24 输出通过。
- 干净报告为 [`.tmp/lowcostprobe-20260915/lowcostprobe-report.txt`](../../.tmp/lowcostprobe-20260915/lowcostprobe-report.txt)，所有可见效果为 0，Popup 透明/动画关闭，环境光和游戏背景透明度为 0；`DG_ScrollViewer` 的有界表格水平滚动被保留，非表格溢出为 0。真实低 Tier/宿主合成仍待验。

## 2026-09-15 UI 精修 Q25-01 热态响应专项复核

- 当前交付基线为 `1a58e4b`。在干净提交上重跑 `LargeLibraryPerformanceTests` 专项，2000 项 GamePicker 先预热 5 次、再采样 30 次，`4/4` 通过；`p50=45ms`、`p95=46ms`、`max=60ms`，`p95≤100ms` 回归门禁通过。
- 原始结果为 [`.tmp/q25-01-clean-20260915/ui-qa/benchmarks/large-library.txt`](../../.tmp/q25-01-clean-20260915/ui-qa/benchmarks/large-library.txt)，证据见 `Q25-01-HOT-RESPONSE-20260915.md`。这只是 ViewModel 输入到 `FilteredCount` 的受控热路径，不替代页面导航、真实输入、呈现帧、30 分钟耐久或低 Tier 实测。

## 2026-09-15 UI 精修 Q20-08 首页边界状态

- 当前交付基线为 `5a2ed09`。Overview 全局活动空态新增 `OverviewActivityEmptyState` 和 `MinHeight="120"`，修复空活动时 `Auto` 行把状态压成不可见区域的真实布局缺口。
- RenderHarness 新增 `overviewedges`：空活动、多风险、96 字符超长标题、Worker 离线四种夹具，浅/深色各覆盖 `1040×700` 与 `1600×900`；干净报告为 [`.tmp/overviewedges-clean-20260915/overviewedges-report.txt`](../../.tmp/overviewedges-clean-20260915/overviewedges-report.txt)，`WorkingTreeClean=True`、16/16 首屏输出通过，空态高度 160 DIP，另有页面尾部空态截图。
- 标准干净 RenderHarness [`.tmp/ui-qa-overview-empty-clean-20260915/render-qa-report.txt`](../../.tmp/ui-qa-overview-empty-clean-20260915/render-qa-report.txt) 为 `render-qa OK`、无 `PROBLEM`；Core `83/83`、Playnite `474/537`（63 skip、0 fail），定向 Overview/边界静态契约 `8/8`。真实 Playnite 状态切换、Hover/Focus、物理 DPI/多屏、IME/读屏仍待宿主条件。

## 2026-09-15 UI 精修游戏选框键盘与自动化名称

- 当前交付基线为 `dea74f7`。生产壳层游戏选框现在在打开时聚焦搜索框，Esc/已选游戏 Enter、点外部和选中游戏都会关闭选框并把焦点返回当前游戏按钮；导航、Header、筛选器、列表、footer 和 Overview 主要动作已补充稳定 UI Automation 名称。
- 真实 WPF STA 行为测试已验证 Esc 关闭、事件处理和焦点返回；RenderHarness 双主题、多尺寸回归仍为 `render-qa OK`。完整六页 Tab/Shift+Tab、IME、读屏、物理跨屏与真实 Playnite UIA Pattern 仍待宿主条件。

## 2026-09-15 UI 精修云端上传与远端校验语义

- 当前交付基线为 `f01dfe9`。Overview 云端队列整卡在保留自动队列运行/暂停状态的同时，明确显示 `已上传 N · 已校验 M`；`RemoteVerified` 不再与普通上传共用一个含义。
- Core `UiDisplayMappingTests`、真实 WPF `OverviewInteractionTests` 和 Playnite 源码契约均覆盖该显示/命令边界；Core 全量 `83/83`、Playnite 全量 `473/536`（63 skip，0 fail），干净双主题多尺寸 RenderHarness 为 `render-qa OK` 且无 `PROBLEM`。
- 真实 Playnite Hover/Focus/单次导航及多状态宿主像素仍待可操作宿主条件；当前证据见 `Q20-07-CLOUD-GUARANTEE-20260915.md`。

## 2026-09-15 UI 精修 150% 宿主截图渲染修复

- 当前交付基线为 `4f1dbb4`。已修正真实宿主 150% 捕获中 RenderTargetBitmap 的宿主 DPI 与显式缩放叠加问题，并用回归测试锁定 1.5 倍输出不再被错误绘制为 2.25 倍。
- 非空隔离 Playnite 库保留原库数据并观察到 3 个游戏；真实嵌入 Dashboard 取得 27 个视口、2 个完整滚动面和 1 个 Settings 视口，右侧内容完整。全量门禁为 XAML 24/24、Release 0/0、Core 82/82、Worker 311/311、Playnite 475/532（57 skip，0 fail）。
- 仍未签收的边界包括 Q20 真实 Hover/Focus/导航与窄窗状态、Q24 物理多屏/DPI/IME/读屏，以及 Q25-02/Q25-03 的 ETW 呈现帧与 >100ms 调用栈、Q25-05 的真实低 Tier/宿主合成和 Q25-04 的 Playnite 宿主耐久；受控 WPF 30 分钟时间序列已单独记录，不能由它替代真实宿主。

## 2026-09-14 UI 精修独立复核与第二轮 208 项

- 最新复核入口为 [第一轮独立复核](../design/UI_FINESSE_REVIEW_2026-09-13.md)，审阅 `fd573e4..2fc40ce`。旧 52 项实际为 14 已验收、1 已满足、31 代码完成待验收、6 外部阻塞，不能以“待开始=0”认定全部完成；部分正向签收也因夹具/证据缺口需要重开。
- 已确认暗色校对 PNG 的 Numeric、部分按钮/开关/徽章存在黑字，而 `finesse-fixture OK` 不检查最终控件前景；状态色 3.0 门槛仍被用于 11 DIP 文本；字体候选可用性不是实际 WPF 落字，数据 4 行不是完整可读 4 行。下一轮优先修这些门禁与可读性问题，不能把旧泛化验收描述覆盖本次复核。
- 新增 [208 项控件与细节任务](../design/UI_FINESSE_ROUND2_208_TASKS_2026-09-13.md) 和 [分维度账本](../design/reviews/ui-finesse-round2-20260913/ROUND2_PROGRESS.md)，26 组、前轮任务数量 4 倍；全部新任务待开始，由用户要求的新 5.6 Luna/max 任务持续实施。
- 本轮仅审计/文档；独立 Release 构建 0 warning/error，Core 76/76、Worker 310/311（1 skip）、Playnite 449/512（63 skip），0 fail。主工作区 `src.zip` 保留；后续干净 worktree 可解决打包源码边界，不削弱 clean-tree 保护，真实宿主取证仍独立验收。

## 2026-09-13 UI 精修提示词计划（P00、P01、P02、P04 共享底座与受控收口已完成）

- 新增 [UI 精修实施提示词包](../design/UI_FINESSE_IMPLEMENTATION_PROMPTS_2026-09-13.md)，包含 12 阶段、52 项任务、可复制总提示词、混排样本、色彩/动效/性能目标、逐页精修和续跑账本。它补充 U12 后的精修与体感验收，不将已完成工作重新列为待重建。
- P00-01/P00-04 已建立资源映射、问题台账与 52 项账本；P00-02 已新增仅开发入口可见的生产资源校对夹具，P00-03 的真实宿主入口仍受窗口不可枚举限制。
- P01 已按本机实际字体命中校准共享链、移除 Caption 二次透明并统一文本像素属性；P02 已补齐最终合成对比度检查与浅色状态色；P04 已让 C# 动效消费 XAML 令牌并保留无资源回退。具体数值和未完成的多机/真实宿主边界见 `BASELINE.md` 与 `PROJECT_MEMORY.md`。
- 当前阶段隔离 Release 构建、Playnite 测试、双主题夹具、状态/网格/缩略图/壳层探针和完整 RenderHarness 均通过；P03、P05～P09 的共享实现与受控证据已登记，真实宿主视觉/物理交互、DPI、读屏及屏幕帧时间仍沿用下方待验收边界，具体验证记录见 WORKLOG。

## 2026-09-13 UI 精修 P03 / P05～P11 当前事实

- 当前证据基线是 `6c3c238b8ba0c7ce3a010e4234e18cfc34f10ee1`。最终 RenderHarness [`ui-finesse-qa-final-20260913`](../../.tmp/ui-finesse-qa-final-20260913/render-qa-report.txt) 为 `render-qa OK`，覆盖双主题、多尺寸、八入口、状态、表格、resize 和生产壳层几何；离屏 `DpiScale=1.00` 仅代表逻辑 DIP。
- `statefixtures`、`gridprobe`、`scaleprobe`、`thumbnailprobe`、`shellqa` 均已在当前提交执行并通过。大数据规模为 backend 1000/5000/20000、Media UI window 2000；缩略图窗口 peak=3、active=0、cache=96/96。Grid 的 `ScrollIntoView` 离屏基线仍为 inconclusive。
- 已同步 52 项账本：P06-03/P06-04、P08-02、P09-02、P11-02 标为已验收；共享代码和受控证据已具备但依赖真实宿主的项目标为代码完成待验收；多屏/物理呈现/耐久/真实路径/打包等标为外部阻塞。
- 当前宿主边界不可扩大：`MainWindowHandle=0`、无 `summary.json`，PNG 是 `DedicatedAuditWindow` 且 `DashboardWasAlreadyHostedByPlaynite=false`；不能把专用窗口、代理帧时间、离屏 DPI 或 synthetic thumbnail 写成真实嵌入 Dashboard、物理 DPI、屏幕帧预算或 Playnite 视频验收。
- 工作区唯一变化外的用户文件是根目录未跟踪 `src.zip`；打包脚本按安全策略拒绝 dirty tree，本轮不删除、移动或提交它。最终一键链结果必须如实记录。
- 最终一键链已通过 XAML `24/24`、Release `0 warning/0 error`、Core `76/76`、Worker `311/311`、Playnite `455/512`（57 skip、0 fail）；在包装阶段按 clean-tree 保护停止，日志为 `artifacts/one-click-install.log`，因此不宣称本轮新包已生成、安装或运行。
- 文档提交后已重试一键链与真实宿主审计：当前工作树仅剩 `?? src.zip`，两者均在安装/采集前按同一 clean-tree 保护停止；最终 Render QA 已在提交 `cacdaff` 重跑为 `render-qa OK`。宿主重试仅生成 runner metadata，无 `summary.json`，不能宣称嵌入 Dashboard。

## 2026-09-13 开发提示词包全量门禁复核

- 用户提供的完整开发提示词包已按 Phase 0～12 和最终验收逐项映射；实现仍是 `net462` WPF `GenericPlugin`，生产入口为 `DashboardView` / `AcrylicProductionShellView`，业务层与插件 ID 未改。
- 最新 RenderHarness 报告为 [`ui-prompt-qa-final2-20260913`](../../.tmp/ui-prompt-qa-final2-20260913/render-qa-report.txt) 的 `render-qa OK`，对应当前 HEAD `108ae0f`，覆盖双主题、多尺寸、状态夹具、表格滚动、Resize 恢复和性能探针；静态审查 0 error，22 个 warning 为既有 Canvas/有限滚动兼容提示。
- 报告的 `WorkingTreeClean=False` 只对应工作区现有未跟踪根目录 `src.zip`；本阶段不删除用户文件。已在 Playnite 退出后通过安全安装器完成最新插件安装，安装验证报告保存在 `artifacts/last-dev-install-current-20260913.txt`。
- Playnite 日志已确认实际加载 `GameSaveCenter 0.6.73`；真实宿主审计生成了 [`ui-host-audit-current-20260913`](../../artifacts/ui-host-audit-current-20260913/)，但当前自动化会话仍枚举不到 Playnite 主窗口（`MainWindowHandle=0`），未生成 `summary.json`，所以嵌入 Dashboard、用户主题、物理 DPI、键盘/滚轮和大库帧率仍未宣称通过。
- 当前 `artifacts/GameSaveCenter-0.6.73.pext` 与 `GameSaveCenter-0.6.73-playnite.zip` 均为 `43,809,213` 字节，SHA-256 为 `20C8FA5D19A61D4378FFDE9E93BBAFAD889F40C9C0F93FEBA3E92E62F376D4C3`；已安装 DLL 构建身份为 `0.6.73+a81403478539ec1036aa135198c88e752e7d41d8`。

## 2026-09-13 GPT UI 契约与发布包复核完成

- 已将用户补充的 Phase 1～5、11 设计契约对齐到现有 Demo-first 共享系统：`DesignTokens.xaml` 补齐 `Spacing4`～`Spacing40`、状态字形转换器和 52/42 DIP 表格行/表头密度；`Typography.xaml` 补齐 Display/Page/Section/Body/Caption/Numeric 语义别名；`Redesign.xaml` 补齐 `GscGlassSurface`、`GscGlassCard`、`GscGlassPanel`；`ButtonStyles.xaml` 补齐 Secondary/Danger/Icon 语义别名。
- `StatusGlyphConverter` 只为已知状态文本添加 `✓/×/⚠`，未知值原样保留；任务、存档和维护状态胶囊已接入，命令、绑定、业务状态与可访问性不变。动效令牌已明确为 Normal 220ms、Slow 300ms，继续使用共享 `GscMotion`/EaseOut；维护页表格最小高度同步到 260 DIP，任务页紧凑 236 DIP 视口保持既有契约。
- Release 解决方案构建 `0 warning / 0 error`；Core `76/76`、Worker `310/311`（1 skip）、Playnite `447/510`（63 skip、0 fail）；`validate-source.py`、XAML 结构检查和全量 RenderHarness 均通过，最新报告为 `render-qa OK`（双主题、多尺寸、缩放恢复、表格滚动和性能探针）。
- 从干净 HEAD `b470bf9` 重新运行 `scripts/package.ps1 -Configuration Release`，`.pext/.zip` 均为 `43,809,091` 字节，SHA-256 为 `9E6F4BB11B812DB824D8E3EA4EBFCD45A2490FEA5C907996301ABD9F21A55F8A`，六份程序集构建身份统一为 `0.6.73+b470bf97f53ec0012da165cb4eb954d6b2e18e6f`。
- 本项仍不宣称真实 Playnite 嵌入 Dashboard、物理 DPI/键盘/滚轮已验收；真实宿主边界沿用下方 2026-09-13 审计记录。

## 2026-09-13 真实宿主审计复跑（受控证据边界）

- 在当前 HEAD `d59a6a6` 使用 `scripts/real-host-audit.ps1 -Configuration Release -Output artifacts/ui-host-audit-live-20260913 -PlayniteExecutable D:\software\Playnite\Playnite.DesktopApp.exe` 完成构建、安装和宿主启动。Release 构建为 `0 warning / 0 error`；Core `76/76`、Worker `311/311`、Playnite `444 passed / 57 skipped / 0 failed`。
- Playnite 扩展目录已安装并加载 `GameSaveCenter 0.6.73`；清单与六份程序集身份为当前提交 `0.6.73+d59a6a6...`。审计输出覆盖 1366×768、1600×1000 和最大化的浅/深主题，记录 150% DPI、真实视口、表格诊断及 `RealFixedLayoutOverflow=[]`。
- 自动 UIAutomation 未找到 Playnite 的 GameSaveCenter 侧栏项，宿主进程 `MainWindowHandle=0`；输出没有 `summary.json`，所有 PNG 的 `CaptureOrigin=DedicatedAuditWindow`、`DashboardWasAlreadyHostedByPlaynite=false`。因此本轮只能作为当前生产程序集的受控窗口证据，不能宣称真实嵌入 Dashboard、物理键盘/滚轮或宿主主题验收已完成。

## 2026-09-12 UI 任务已合并

- 新增 [`UI_CONSOLIDATED_BACKLOG_2026-09-12.md`](UI_CONSOLIDATED_BACKLOG_2026-09-12.md)，合并本项目的 D12 显示复核任务与用户提供的 GPT UI 建议。后续 UI 工作从该队列执行；它明确了已有共享系统只审计不重建、Demo-first 优先及真实业务边界。
- 近期优先项为共享审计/Demo 基准恢复、媒体短窗、设置错误摘要和 RenderHarness/真实宿主证据入口；其他页面、状态、表格与动效任务按 U12-03～U12-09 顺序推进。

## 2026-09-12 U12-00 共享资源审计完成

- 审计结论和资源入口已固化在 [`UI_SHARED_AUDIT_2026-09-12.md`](UI_SHARED_AUDIT_2026-09-12.md)：现有 `DesignTokens`、Typography、WPF-UI/Redesign、图标和 Motion 系统均为后续唯一共享入口，不新增平行控件或玻璃体系。
- 当前工作树不存在原始 `GameSaveCenter.AcrylicFork/.../Design/` 文件；Demo-first 结构的可追溯锚点为 `3c12b2c` 和 `RestoredAcrylicForkBaselineTests.cs`。原始素材找回前不将其作为构建依赖，也不以其他视觉体系替代。
- 页面/设置层仅发现媒体预览图片遮罩的局部透明黑硬编码；它不承担主题职责，保持不变。下一项按队列进入 U12-02 设置错误摘要与首屏。

## 2026-09-12 U12-02 设置错误摘要与首屏完成

- 设置页将页头中拼接的长校验错误改为“有 N 项设置需要修正”的单行摘要；完整错误改为可展开详情，且 Tooltip 保留完整信息。
- 常规与目录分类中的错误会在字段区前显示，页头“定位首个错误”仍可键盘聚焦并跳转到对应分类；保存阻断、路径校验和原有字段验证保持不变。
- 短高度隐藏装饰性 SETTINGS 眉题，保留标题、保存状态、错误摘要与定位入口。Release 构建、XAML/源码检查与静态 WPF 审查通过；真实 Playnite、物理 DPI 与键盘路径仍待宿主复核。

## 2026-09-12 U12-01 待归类媒体短窗布局完成

- 待归类页在紧凑高度（低于 800 DIP）隐藏与上方指标重复的标题/数量信息带，将空间交还给批量工具栏和有限 DataGrid；正常高度保持原信息带。
- 已用 RenderHarness 发现主题化 1366×768 的直接内容宿主实际仅 528 DIP，若不启动页面级滚动，表格只能完整显示 3 行。fallback 阈值据此校准为 560 DIP；离线/过期横幅仍强制安全降级，批量操作、Inspector、选择、虚拟化和末页入口未改。
- Release RenderHarness 构建 0 warning / 0 error，完整 `render-qa OK`；真实宿主的物理滚动末端、DPI 与主题仍待验收。

## 2026-09-12 U12-03 任务统计与筛选紧凑化完成

- 720 DIP 以下将任务统计卡压缩为数字与标签，隐藏“运行中/可重试”的次级摘要；四项统计、搜索、筛选、清除与详情入口保持不变。
- 释放的首屏高度直接留给有限且虚拟化的 TaskGrid。源码检查通过；真实宿主的行数对比、详情开关和物理末端滚动仍待验收。

## 2026-09-12 U12-04 首页首屏工作内容优先完成

- 内容区低于 560 DIP 时，首页 Hero 保留优先事项与真实操作，但隐藏装饰眉题/说明并降至 132 DIP 最小高度，让“今日工作台”更早可见。
- 当前游戏与“全部备份”作用域未变；页面滚动、风险列表有限视口和真实命令保持。源码检查通过，真实 Playnite/DPI 验收待补。

## 2026-09-12 U12-05 表格阅读与排版审计完成

- Media、Task、Maintenance 已统一使用省略号、Tooltip、稳定列宽、横向滚动与共享虚拟化契约；审计补齐了媒体收件箱“来源”列遗漏的 `MediaLongText` 样式。
- 不改变表格字体、排序、拖拽、行/列虚拟化或 DataGrid 模板。源码与 XAML 门禁通过；200% DPI 与真实宿主的横向两端阅读仍待人工复核。

## 2026-09-12 U12-06 与 U12-10 诊断出口及离屏门禁

- 任务失败诊断复制不再要求 `DetailMessage` 存在：失败原因、错误码或技术详情任一可用时，用户即可复制包含三者及任务 ID 的完整载荷；任务详情顺序/折叠定向测试 6/6 通过。
- 失败卡片位于技术详情之前，`TaskTechnicalDetailsExpander` 默认收起。完整 RenderHarness 已以干净临时目录重跑并得到 `render-qa OK`，覆盖双主题、多尺寸、表格完整可读行、尺寸恢复、壳层几何和侧栏状态。该结果是离屏 WPF 证据，不能替代真实 Playnite 的 DPI、键盘和宿主主题验收。

## 2026-09-12 U12-08 筛选无结果状态

- 共享 `GscWorkspaceStatePresenter` 为 `FilterEmpty` 增加独立“无结果”状态标签；任务页继续提供“清除筛选”下一步，真实数据状态、选择与滚动保持不变。

## 2026-09-12 U12-09 动效、焦点与可访问性审计

- 无障碍/键盘/壳层动效组合定向测试 13/13，动效原语、减动效终态与列表焦点契约 3/3；源码/XAML 门禁通过，WPF 静态审查 0 error。
- 共享 `GscMotion`/`MotionTokens`、高对比度/关闭动画终态、AutomationProperties、Tooltip 和键盘路径保持现有实现；详细证据见 [`UI_ACCESSIBILITY_AUDIT_2026-09-12.md`](UI_ACCESSIBILITY_AUDIT_2026-09-12.md)。真实 Playnite/FusionX、物理 DPI 和高对比度宿主仍待人工验收。

## 2026-09-12 一键构建回归已恢复

- `scripts/build.cmd` 的完整运行曾在 Playnite 测试阶段失败 4 项；原因不是编译或运行时故障，而是近期图标按钮、共享动效 token 的生产迁移完成后，四个源码回归测试仍断言旧的文字按钮样式与硬编码时长。
- 测试现改为验证图标按钮的 `ToolTip`、自动化名称和主题图标、Dashboard 刷新按钮的无标签图标语义，以及 `GscMotion`/`MotionTokens.xaml` 的统一时长和 easing 契约，未回退生产 UI。
- 完整一键构建已通过：Release `0 warning / 0 error`；Core `76/76`、Worker `310/311`（1 skip）、Playnite `435/498`（63 skip），失败均为 0。

## 2026-09-12 图标按钮语义边界收紧

- 图标按钮仅用于刷新、重试、删除、复制、取消、折叠等低歧义且高频的上下文操作；Tooltip、`AutomationProperties.Name` 和禁用态提示仍为必需。
- 会触发业务判断或存在多个明确目标的操作保留文字：存档页“立即扫描 / 重新校验”、媒体“打开媒体 / 打开所在目录”、维护“数据/存档/媒体目录”均已恢复文字按钮。命令、绑定、窄屏 Wrap 和真实数据流不变。
- 本轮 `validate-source.py`、XAML 结构检查、WPF 静态审查（`0 error / 22 warnings / 175 info`）和 `render-qa OK` 均通过；完整一键构建为 Release `0 warning / 0 error`、Core `76/76`、Worker `310/311`（1 skip）、Playnite `435/498`（63 skip），失败为 0。

## 2026-09-10 实际 Playnite 扩展已刷新

- 复核用户反馈后确认，之前的源码改动没有进入 Playnite 正在使用的扩展目录：源码 Release DLL 与安装目录 DLL 哈希不同，安装目录 DLL 时间为 15:45，`icon.png` 仍为 8 月 2 日旧资产。因此“按钮没有变化”首先是旧 DLL 被宿主加载，不是用户误判。
- 已使用 `scripts/dev-install-run.ps1 -Configuration Release -NoStart` 从当前 HEAD `c757ffe` 重新构建、运行 Core `76/76`、Worker `311/311`、Playnite `440/497`（57 skip，0 fail），并原子替换实际目录 `C:\Users\lopmatu\AppData\Roaming\Playnite\Extensions\GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec`。
- 安装后包 staging 与宿主目录的 `extension.yaml`、插件 DLL、Contracts/Core、Worker DLL、`icon.png` 哈希全部一致；插件构建身份为 `0.6.73+c757ffe5ff8b678d69cdca1386cf89be10f7a831`。本次使用 `-NoStart`，当前没有运行中的 Playnite；需要完全启动/重启 Playnite 后观察按钮和侧栏图标。

## 2026-09-10 主题感知线性图标包

- 当前生产 UI 使用 `Controls/ThemeAwareIcon.cs` 和 `Themes/GscIconPack.xaml` 的共享 Geometry/Path 线稿资源，来源为用户提供的 `GameSaveCenter_IconPack_v2_round-flat.zip`；不依赖 SVG 渲染器，图标透明底并从主题/父控件继承 `Foreground`。
- 生产壳导航、设置分组（含常规与目录、设置迁移）、首页状态、媒体/维护/修改器/任务局部图标以及兼容 Dashboard 对应图标已接入；状态颜色、选中前景、真实命令/绑定/虚拟化和自动化语义保持。
- `src/GameSaveCenter.Playnite/icon.png` 已替换为 ZIP 中 `plugin-main-256.png` 的透明线稿；运行时侧栏则由 `CreateThemeAwareSidebarIcon()` 使用 `plugin-main.svg` 几何，并将 Stroke 绑定 Playnite `GlyphBrush`，跟随宿主主题切换黑/白色。
- 最新安装验证：Playnite 扩展目录已与当前包逐项哈希一致；此前的离屏报告仍不代表真实 Playnite 的用户主题、Follow、DPI 和物理侧栏截图验收。

## 2026-09-10 统一 Glass 按钮组件语义

- 生产按钮继续使用已有的 `ui:Button` 兼容控件和 `GscWpfUiButton` 共享模板；`Themes/ButtonStyles.xaml` 提供 `GlassButton`、`PrimaryGlassButton`、`DangerGlassButton`、`IconButton`、`SegmentedButton` 语义别名，避免重复的独立控件实现。
- 共享模板具备 Primary/Secondary/Danger/Disabled 状态、主题动态颜色、Hover/Focus/Pressed 状态和 `0.97` 按压缩放；不改变业务命令、Binding、布局或虚拟化，也不使用大面积逐控件 Blur。
- 最新验证：共享模板的源码修改已安装到真实 Playnite 扩展目录；安装前用户看到的旧按钮来自旧 DLL。离屏报告 [`.tmp/icon-button-qa-clean-20260910/render-qa-report.txt`](../../.tmp/icon-button-qa-clean-20260910/render-qa-report.txt) 仍只用于双主题结构检查，按钮最终视觉需要在 Playnite 完全重启后复核。

## 2026-09-10 表格内容视口与末行可见性继续修正

- 共享 `GscRedesignDataGridTemplate` 现在把 `DG_ScrollViewer` 内的 `PART_ScrollContentPresenter` 与真实 `ItemsPresenter` 都固定为 `HorizontalAlignment=Stretch`、`VerticalAlignment=Top`；不再依赖宿主模板在短窗口或拖动滑块后的间接对齐值，已实现行从列头下方开始排列。
- 该修复覆盖 Saves、Media、Tasks、Maintenance 等共用模板的表格；Media 的 `Standard` 行虚拟化、`Item` 滚动、关闭列虚拟化和 `DataGridStarFill.Enabled=False` 例外保持不变。
- 定向 `MediaWindowAnchorContractTests` 为 `10/10`，最新离屏报告 [`.tmp/qa-table-scroll-20260910-final/render-qa-report.txt`](../../.tmp/qa-table-scroll-20260910-final/render-qa-report.txt) 为 `render-qa OK`，覆盖短高度、滚动端点、页尾几何与双主题。真实 Playnite 宿主、物理 DPI 缩放和用户现场滑块仍需验收。

## 2026-09-10 设置主题资源与导航对齐已修正

- `AdaptiveThemePaletteFactory` 解析 FollowPlaynite 资源时现在先读取实际承载页面的 Window/Owner，再读取控件局部资源和 Application 资源，避免设置窗口先命中遗留深色默认值；显式“浅色/深色”覆盖行为不变。
- 生产壳层导航样式补齐 `VerticalContentAlignment=Center`，全部图标、文字组和子元素显式垂直居中，保持收起导航栏、键盘和自动化名称不变。
- 新增设置窗口优先级与七项导航内容对齐回归断言；Playnite 定向 WPF 测试 `130 passed / 39 skipped / 0 failed`。真实 Playnite 设置窗口和用户主题的物理截图仍需宿主验收。

## 2026-09-10 待归类媒体表格视口对齐已修正

- 共享 `GscRedesignDataGridTemplate` 的外层 `DG_ScrollViewer` 与内层 `PART_ScrollContentPresenter` 现在都显式绑定 `HorizontalContentAlignment` / `VerticalContentAlignment`；`MediaInboxGrid` 的 `Top` 对齐贯穿到真实内容视口，避免大数据虚拟化表格滚到底后上方出现大块空白。
- Media 的 `Standard` 行虚拟化、`Item` 滚动、关闭列虚拟化和 `DataGridStarFill.Enabled=False` 仍是已验证的大数据例外，没有恢复成其他工作区的共享 Recycling 配置。
- 最终全量 Release 回归为 Core `76/76`、Worker `310/311`（1 skip）、Playnite `429/492`（63 skip），0 失败；当前离屏报告 [`.tmp/qa-table-shell-20260910/render-qa-report.txt`](../../.tmp/qa-table-shell-20260910/render-qa-report.txt) 为 `render-qa OK`。真实 Playnite 的物理滚轮/滑块和视频式回归仍保持待验收边界。

## 2026-09-10 页面外围重复材质已移除

- 各生产页面的 `AmbientMaterialLayer` 仍保留，但共享 `GscAmbientPageOpacity` 在运行时固定为 `0`；这样页面不会再各自绘制一个带圆角的整面渐变矩形。
- `AcrylicProductionShellView` 的唯一可见 `ShellAmbientMaterialLayer` 通过 `IsShellLayer="True"` 使用 `GscShellAmbientOpacity`，继续覆盖侧栏、右侧页面和 footer；选中游戏背景 `ImageBrush` 与可读性遮罩不变。
- 资源测试、Release 构建、全量测试、源码/XAML 门禁、WPF 静态审计和双主题 RenderHarness 均已通过；干净报告 [`.tmp/qa-table-ambient-20260910/render-qa-report.txt`](../../.tmp/qa-table-ambient-20260910/render-qa-report.txt) 为 `fc3c7d5`、`WorkingTreeClean: True`、`render-qa OK`。真实 Playnite/FusionX 和宿主 DPI 仍需人工截图确认。

## 2026-09-10 生产壳层背景已覆盖整页并去除边界缝

- 图二中的长方形是选中游戏背景 `ImageBrush`，现在继续覆盖整个生产壳层（侧栏 + 右侧页面 + footer），同时由跨壳层 ambient 层和页面表面负责可读性。
- `DemoShell` 去掉 4 DIP 外边距和外框描边；侧栏去掉右侧 6 DIP 缝；footer 去掉独立边框/外边距，避免图片在侧栏和页面之间被切出丑的竖向边界线。功能绑定、命令和导航未改。
- 离屏报告 [`.tmp/qa-table-shell-20260910/render-qa-report.txt`](../../.tmp/qa-table-shell-20260910/render-qa-report.txt) 的双主题完整壳层背景和 Shell Media 多尺寸几何门禁通过；真实宿主图片资源与用户主题仍待验收。

## 2026-09-10 当前候选包已按最新生产源提交重新生成

- `scripts/package.ps1` 已从当前 HEAD `8657654` 在隔离目录完成 Release 构建、测试和 Worker 发布；Core `76/76`、Worker `310/311`（1 skip）、Playnite `428/491`（63 skip），0 失败，构建 0 warning/0 error。
- 六份程序集身份统一为 `0.6.73+8657654310d99e349c120f3bb0484b65ac9f4dc2`；[`.pext`](../../artifacts/GameSaveCenter-0.6.73.pext) 和 [`.zip`](../../artifacts/GameSaveCenter-0.6.73-playnite.zip) 均为 `43,837,912` 字节，SHA-256 均为 `E136B5C6465A5A8933C72B0F9707CFF64D91A02D35115B9A007BF6A749B62AB0`。
- 生产视觉修复来自 `248d28e`，`8657654` 只补充游戏选择器运行时模板测试；活动游戏选择器共享按钮继续显式清空 `ContentTemplate`，防止宿主文本模板把复合 `Grid` 显示成 `System.Windows.Controls.Grid`，此前首页活动行、云端队列整卡及 Dashboard 顶部复合按钮修复仍保留。
- 当前干净离屏 RenderHarness 报告为 [`.tmp/render-qa-head-clean-20260910/render-qa-report.txt`](../../.tmp/render-qa-head-clean-20260910/render-qa-report.txt)，报告提交 `5c53e59`、`WorkingTreeClean: True`、`render-qa OK`；`5c53e59` 只改文档，生产 XAML 与当前候选包一致。双主题/多尺寸/resize、云端队列筛选文字、完整壳层背景层和 Media 页尾可达性均通过。该报告不替代真实 Playnite/FusionX 宿主验收。
- 临时构建目录和打包 staging 目录已清理；包未安装真实 Playnite。真实宿主/FusionX、用户主题、DPI、物理点击和视频复测仍待 L31 条件恢复。
- 包后独立门禁：`validate-source.py` 通过，XAML `19/19` 通过，`git diff --check` 通过。
- 本轮 WPF 静态审查为 `0 errors / 22 warnings / 172 info`；warnings/info 为既有 Canvas、滚动容器和颜色令牌提示，未新增 error。
- 2026-09-10 在新增 `GameContextButtonKeepsCompositeGridContentThroughItsRuntimeTemplate` 运行时测试后重新执行 `dotnet test GameSaveCenter.sln -c Release --no-restore -m:1`：Core `76/76`、Worker `310/311`（1 skip）、Playnite `428/491`（63 skip），失败 `0`；此前 `AsyncThumbnailLoader` 的 `120`/`122` 并发污染本次未再复现。该测试实际解析并套用了选择器模板，确认 `ContentPresenter` 保留原始 `Grid` 内容。
- L32 链接审计：扫描本地 Markdown 链接 `123` 条，缺失 `0`；当前候选 `.pext/.zip` 均存在。已移除旧工作日志中指向已清理一次性截图的失效链接。

## 2026-09-09 首页交互行为测试已补齐

- `4001d9d` 新增 STA WPF 行为测试，实际布局 `OverviewView` 后确认全局活动行不会把 `Border/Grid` 字符串化为 `System.Windows.Controls.Border`，云端队列卡片保留 `StackPanel` 内容并通过整卡点击执行 `OpenCloudQueueCommand` 一次。
- 当前回归：Release 解决方案构建 `0 warning / 0 error`；Playnite `427/490`（63 skip、0 fail）；定向首页交互/工具栏测试均通过。当前候选包已从生产源提交 `63f4b2d` 重新生成。
- 真实 Playnite/FusionX 没有可绑定窗口；离屏/STA 测试不等价真实宿主浅色/深色主题、DPI、物理点击和视频复测。

## 2026-09-09 首页全局活动与云端队列卡片修复

- `OverviewActivityRowButton` 已清空共享文本按钮模板的 `ContentTemplate`，避免活动行的 `Border/Grid` 可视树被字符串化为 `System.Windows.Controls.Border`。
- 云端队列指标已改成整卡 `OverviewCloudQueueCardButton`，直接执行 `OpenCloudQueueCommand`；“查看明细”子按钮已删除，整卡保留可点击、键盘焦点和自动化名称。
- 当前验证：Playnite `426/489`（63 skip、0 fail）、Release 构建 `0/0`、RenderHarness `render-qa OK`、源校验通过、XAML `19/19`。RenderHarness 和离屏截图不替代真实 Playnite/FusionX 宿主验收。
- 当前候选包为提交 `1fdd15e`：六份程序集身份 `0.6.73+1fdd15eedfa64bb34292b85cb0e4d14bbfa9dd81`，两个包均 `43,837,799` 字节，SHA-256 为 `FD91FB0E0B12ABA2A73F29F76F1E3F90255D6FA4D30798EBA2FEFB53FAD120F9`；未安装真实 Playnite。

## 2026-09-09 回归失败修复与离屏门禁更新

- `c0197e5` 将 `AsyncThumbnailLoaderTests` 和 `AsyncThumbnailImageTests` 放入禁并行集合。此前两个测试共享进程级诊断/缓存状态，完整套件并发时会把一次应为 `120` 的请求计数污染为 `122`；定向 `1/1`、Playnite 全量 `425/488`（63 skip、0 fail）已通过。
- `559d64f` 修正 RenderHarness 的 Settings 临时目录夹具、Sidebar 快速切换完成计时器和 Media 主题探针高度。干净工作树报告为 [`.tmp/render-qa-harness-clean-20260909/render-qa-report.txt`](../../.tmp/render-qa-harness-clean-20260909/render-qa-report.txt)，RenderHarness 构建 `0 warning/0 error`；Settings 三态和 Sidebar 第二次点击门禁已恢复正常，生产壳层 Media 1040/1100 DIP 表格视口均为 `300`，页尾 footer/history/secondary 可到达。
- `d777e65` 将 Media 门禁改为按实际 DataGrid 行几何统计完整可读行数，并让 resize 探针传入 Media 的实际 `contentH`；嵌套 Inspector 预览/历史列表不再套用主表四行门禁。最新完整报告为 [`.tmp/render-qa-cloud-filter-probe-20260909/render-qa-report.txt`](../../.tmp/render-qa-cloud-filter-probe-20260909/render-qa-report.txt)，对应 `9114092`、`WorkingTreeClean: True`、`render-qa OK`；Media resize 为 `300 DIP`、`readableRows=6/4`，原 `230 DIP` 场景为 `readableRows=4/4`。
- 当前顺序 Release 复验通过：`dotnet build GameSaveCenter.sln -c Release --no-restore -m:1` 为 `0 warning / 0 error`；Core `76/76`、Worker `310/311`（1 skip）、Playnite `425/488`（63 skip）均为 `0` 失败。源码校验、XAML `19/19` 和 WPF 静态审计 `0 errors / 22 warnings / 172 info` 通过。
- RenderHarness 新增云端队列主题控件探针：在真实 WPF `MaintenanceView` 的“云端队列”页中找到两个带 Automation Name 的 ComboBox，浅色选中文本实际为 `#F21B1F27`、深色为 `#FFF2F4F8`，均有可见文字；截图和报告保留在 `.tmp/render-qa-cloud-filter-probe-20260909/`。该证据仍是离屏宿主，不替代真实 FusionX 截图。
- 同一 RenderHarness 还检查生产壳层的 `ShellAmbientMaterialLayer`：浅深主题都必须跨两列、跨 footer 两行、覆盖完整壳层，并保持 `UseSelectedGameBackground=False`，用于防止图片方框/接缝回归。结果写入同一报告，仍不替代真实 Playnite 图片资源宿主截图。
- 该报告仍是离屏 WPF 证据，不替代真实 Playnite/FusionX、DPI、用户主题和视频式拖动；真实宿主仍待验收。

## 2026-09-09 浅色主题视觉问题修复（离屏验证完成，真实宿主待验收）

- 对应代码提交：`2365a5c`（已推送到 `origin/main`）。
- 用户截图中的两类问题已按共享层修复：生产壳层不再在内容列重复绘制选中游戏背景；维护中心云端队列及同类有限宽度 ComboBox 的文本显式使用 `GscPrimaryTextBrush`，浅色主题不再继承白色宿主文字。
- 相关契约测试 `2/2`、源校验、XAML `19/19` 和 WPF 静态审计 `0 errors` 通过；RenderHarness 已成功构建并产出双主题、多尺寸输出，Settings/Sidebar/Media 离屏门禁均已通过。真实 Playnite/FusionX 视觉确认仍待宿主验收。
- 目前没有可绑定的真实 Playnite 窗口，未完成 FusionX 宿主截图或浅色主题真实回归；交付时应把真实宿主视觉确认标为待验收。前述表格滚动视频问题的物理操作与录屏边界保持不变。

## 2026-09-09 L42/L41/L39 表格滚动证据边界（当前最新）

- L42 运行基线为 `398a6f0`；其 Release 构建身份为 `0.6.73+398a6f095718e2827c3e8bd2a19bbbb5525c1f16`。L42 Core `76/76`、Worker `311/311`、Playnite `431/488`（57 skip），源码和 XAML `19/19` 门禁通过。
- L41 在受限环境中因 Playnite/CEF `拒绝访问` 后退出；L42 提升权限后 Playnite 进程仍无可绑定主窗口（`MainWindowHandle=0`），UI Automation 找不到 GameSaveCenter 侧栏，最终没有 `summary.json` 或 replay JSON。详细边界见 [`L42_REAL_HOST_SCROLL_REPLAY_2026-09-09.md`](L42_REAL_HOST_SCROLL_REPLAY_2026-09-09.md)。
- L40 使用隔离 Playnite 启动并修正了复制配置中遗留的旧 Worker 绝对路径；Worker 身份确认正确。但最终 `EmbeddedDashboardCaptured=false`、`ControlledDashboardCaptured=true`、`ProductionVisualSourceOfTruthAvailable=false`，没有新的真实嵌入表格 replay JSON，不能作为滚动通过证据。详情见 [`L40_REAL_HOST_SCROLL_REPLAY_2026-09-09.md`](L40_REAL_HOST_SCROLL_REPLAY_2026-09-09.md)。
- 当前有效的真实嵌入端点证据仍是 L39：`MediaInboxGrid` 400 条、`TaskGrid` 50 条，各 47 个样本，底部各 21 个，20 次上下端点往返；Media `394/394`、Task `40/40`，两表末项均在实际 Presenter 内完整可见，空正文、大块间隙、水平条覆盖、选中内容缺失和尾项不完整均为 0。该证据不覆盖物理滑块/视频。
- b100913 的开发审计 RangeValue provider 曾抛出 WPF `NullReferenceException`；c84107b 只在开发审计中隔离该异常，不改变生产 DataGrid 模板、滚动单位、绑定、选择或虚拟化。当前未确认视频异常根因，也没有宣称问题 B 已解决。
- Computer Use 当前仍返回 `apps=[]`；物理滑块、滚轮、PageUp/PageDown、Ctrl+End、尾部选择、主题/DPI 矩阵、加载更多现场锚点和真实录屏继续标为 `MANUAL QA REQUIRED`。完整索引见 [`EVIDENCE_INDEX_2026-09-09.md`](EVIDENCE_INDEX_2026-09-09.md)。

## 2026-09-09 L36 真实 Playnite 嵌入采集结果与未完成项

- 当前提交 `99bc976473d13a92c12d6992dff11dbf807e42b5` 已在隔离数据目录中启动真实 Playnite，并由生产扩展自身完成 EmbeddedPlaynite Dashboard/设置页采集；证据入口为 [`artifacts/ui-host-audit-isolated-l36/summary.json`](../../artifacts/ui-host-audit-isolated-l36/summary.json)。本次没有修改用户 FusionX 文件、Playnite 全局样式或用户数据目录。
- `MediaInboxGrid` 和 `TaskGrid` 的真实诊断均定位到 `ScrollViewer` + `ScrollContentPresenter|DataGridRowsPresenter`，`CanContentScroll=True`、`ScrollUnit=Item`；水平条显示时 Presenter 矩形实际避开水平条。真实日志出现短暂 `visual>0,text=0` 呈现过渡，约 32ms 后恢复；未出现持续 `blank=True`、`gap=True` 或 `hOverlap=True`，所以视频根因仍不能定案。
- 这次采集的媒体表为分页 `Items.Count=200`，不是底层 4645 条一次性装载；滚动面清单的 `CapturedAndValidated` 也不能替代“人工拖到已加载末尾、最后行完整可见”的验收。原视频的滑块 20 次往返、滚轮/PageUp/PageDown/Ctrl+End、选择尾部、水平条显隐、尺寸/DPI矩阵和真实录屏仍为 `MANUAL QA REQUIRED`。
- 真实宿主启动前置修复：`TrainerDownloadProgress` 只读绑定显式 `Mode=OneWay`，见提交 `e98eba2`；`99bc976` 修正隔离配置 UTF-8 读取。不要将该启动修复写成滚动根因。

## 2026-09-09 L30 候选安装包与升级/回退说明已完成（未安装真实宿主）

- 候选包沿用公共版本 `0.6.73`，插件/Worker/Core/Contracts 六份程序集构建身份一致：`0.6.73+1fdd15eedfa64bb34292b85cb0e4d14bbfa9dd81`。Worker 为 `win-x64` self-contained，manifest、必需文件和包内容校验通过；本包包含滚动锚点、浅色主题背景、云端筛选文字和首页活动/云端卡片修复。
- [`.pext`](../../artifacts/GameSaveCenter-0.6.73.pext) 与 [`.zip`](../../artifacts/GameSaveCenter-0.6.73-playnite.zip) 均 `43,837,799` 字节，SHA-256 均为 `FD91FB0E0B12ABA2A73F29F76F1E3F90255D6FA4D30798EBA2FEFB53FAD120F9`。`scripts/package.ps1` 在隔离输出中完成 Release 构建、测试、Worker 发布和包校验；候选包未安装到真实 Playnite。
- 数据库升级/重复初始化定向 `14/14`；当前迁移为幂等增量，回退必须恢复完整升级前隔离配置/状态库副本，不承诺旧包直接读取新 schema。具体步骤见 [`L30_PACKAGE_CHECKLIST_2026-09-09.md`](L30_PACKAGE_CHECKLIST_2026-09-09.md)。
- 真实 Playnite 加载、FusionX/用户主题、DPI、Worker 进程回收和原视频复测仍待 L31 宿主矩阵。

## 2026-09-09 L31 真实宿主矩阵外部阻塞

- PowerShell 只读检查确认宿主程序路径为 `D:\software\Playnite\Playnite.DesktopApp.exe`；本次继续核查时 Playnite 进程已不在，Windows Computer Use 仍返回 `apps: []`，没有可绑定的窗口。
- 本轮没有启动/重装宿主、安装候选包、执行真实 UI 操作或生成真实录屏/滚动诊断；现有扩展目录仍为已安装的 `0.6.73`，表格视频异常仍未宣称解决。
- 尝试调用既有 `scripts/real-host-audit.ps1` 时被安全门禁拦截：该流程会替换用户扩展目录并启动宿主，当前没有明确的替换授权；因此未写入用户 Playnite 目录、未启动 Playnite，外部状态没有改变。
- 恢复条件和待执行矩阵见 [`L31_REAL_HOST_BLOCKER_2026-09-09.md`](L31_REAL_HOST_BLOCKER_2026-09-09.md)。

## 2026-09-09 表格滚动诊断补强、窗口级离线复现与 FusionX 只读对照（不等同真实宿主）

- `DataGridScrollDiagnostics` 现在优先选择包含真实 `DataGridRowsPresenter` 的内部 `ScrollViewer`，并记录实际 `ScrollContentPresenter`、`IScrollInfo`、DIP 视口、逻辑 offset/extent、水平条占用、首末可见行、单元格内容/裁剪和异常分类。媒体锚点上下文补充 queued、executing、completed、stale-generation、retry 和失败原因。
- `scaleprobe` 使用任务表/媒体表相同离线模板，200/2000/10000 条数据分别执行 20 次顶部/底部/中间往返及滚轮、PageUp/PageDown、Ctrl+End；报告为 `scaleprobe OK`，没有稳定空白正文、选中框与文字分离或大块表头间隙，末行在直接滑到末尾时完整落入 Presenter 视口。
- 新增提交 `862742a` 的隐藏 WPF `Window` 对照：插件模板从顶部执行 `ScrollIntoView(最后一项)` 后为 `1992/1992`，最后行 `1999` 完整落在 `ScrollContentPresenter` 内；随后 20 次滑块往返和 `PageDown/PageUp/Ctrl+End` 仍保持可见、选择稳定。标准 WPF 模板的直接滑块/Ctrl+End 末行完整，但 deferred `ScrollIntoView` 在该夹具中仍记录为 `offscreen-baseline-inconclusive`，不把基线不确定性写成通过。
- 提交 `85b1aeb` 将本机已安装 FusionX `2.1.1` 的 `DefaultControls/DataGrid.xaml` 只读加载进同一隐藏 `Window` 对照；没有修改用户主题文件。FusionX 直接滑块和 `Ctrl+End` 均到 `1987/1987`，末行 `1999` 为 `@608/44`，Presenter 为 `0,36,1078.67x600`；水平条为 `Collapsed/0`。FusionX 的 deferred `ScrollIntoView` 仍标为 `offscreen-baseline-inconclusive`，所以这只证明当前安装模板在离线直接拖动路径下未复现空白/行框分离，不能替代真实宿主。
- 提交 `5198c6c` 又对 FusionX 执行 700×640 DIP 窄视口、1100 DIP 列宽的水平条场景；水平条为 `Visible/17.33`，Presenter 实际缩为 `678.67x582.67`，滑到 `1987/1987` 后末行仍为 `1999@608/44`。20 次往返和 `PageDown/PageUp/Ctrl+End` 均通过，说明离线模板在水平条占用内容视口时也没有把最后一行盖住；仍不等同真实 Playnite。
- 提交 `8775809` 修正探针元数据：空的 `git status --porcelain` 不再被误报为 `unknown`。最新 canonical 报告的提交为 `ea18b11`，`WorkingTreeClean: True`，并保留上述两组 FusionX 对照结果。
- 提交 `f31711c` 修正生产锚点路径：`MediaCenterView` 的捕获和恢复不再取第一个后代 `ScrollViewer`，而是优先选择拥有 `DataGridRowsPresenter` 的实际表格滚动器，再按实际视口尺寸兜底；提交 `1477a37` 增加真实 STA 隐藏 Window 行为测试，`MediaWindowAnchorContractTests` 定向结果为 `10/10`。当前 `1fdd15e` 候选包已包含该修复和后续首页/浅色主题修复，真实宿主仍需用该包复测。
- 证据保留在 `.tmp/l32-scrollprobe/scaleprobe-report.txt`；真实 Playnite/FusionX、视频、DPI、加载更多现场锚点仍未验证。本阶段验证：Release RenderHarness 构建 `0 warning/0 error`、`scaleprobe OK`，锚点定向 `10/10`，Playnite 全量 `423/486`（63 skip、0 fail），源码校验通过。
- L32 证据索引见 [`EVIDENCE_INDEX_2026-09-09.md`](EVIDENCE_INDEX_2026-09-09.md)，其中明确当前候选包、离线滚动报告、skip 账本和真实宿主缺口。

## 2026-09-09 L28 持续更新分页与选择恢复已完成离线收口（真实宿主待验收）

- 云端队列和媒体归类历史继续使用 Worker 的 revision/一致性令牌，不把变化中的 offset 页静默拼接。新增/状态更新导致令牌变化时，旧页请求返回 `PageResetRequired`；没有取消一致性检查。
- 两条分页 VM 在重置前保存当前稳定 `TransferKey`/`BatchId`。第一页重建后会在 `HasMore` 允许时继续加载到该对象所在页，按稳定 ID 恢复选择；对象删除或确实到达末页才清掉选择。
- 连续变化导致第二次重置时不再自动递归请求下一页，保留待恢复 ID，并通过列表摘要和全局状态明确提示点击已有“刷新队列/刷新”命令。没有 `Items.Refresh()`、定时 `UpdateLayout()`、强制回顶或关闭虚拟化。
- 验证：Release 构建 `0 warning/0 error`；Core `76/76`；Worker `310/311`（1 项真实 Worker 重启测试在沙箱跳过）；Playnite `420/483`（63 项 UI/宿主条件跳过）；分页定向 `26/26`，新增 Playnite 分页契约 `1/1`；源校验、XAML `19/19`、差异检查通过。真实 Playnite/FusionX、用户主题/DPI 和原视频操作仍待宿主验收。

## 2026-09-09 L29 全量回归与 skip 账本已完成（真实宿主待验收）

- 全量 Release：Core `76/76`；Worker `310/311`，唯一跳过是当前沙箱无法创建本地 Named Pipe 的真实 Worker 重启测试；Playnite `420/483`，拆分为 `57` 项旧“今日工作台”架构断言和 `6` 项 Named Pipe 行为测试。没有失败，跳过原因和替代证据见 [`SKIP_LEDGER_2026-09-09.md`](SKIP_LEDGER_2026-09-09.md)。
- 修复 E01 行为矩阵脚本的 `-SkipBuild` 验证缺陷：跳过构建时使用已有 Release 输出，不再注入未构建的 `GscBuildOutputRoot`。修复后实际矩阵 `151` 项，`144` 通过、`7` 跳过，日志和分类汇总均有真实测试输出；`.tmp` 证据已清理。
- `validate-source.py`、XAML `19/19`、`git diff --check` 通过。真实 Playnite/FusionX、6 项 Named Pipe IPC 时序、Worker 硬重启、用户主题/DPI 和原视频操作仍待宿主，不能用离线计数替代。

## 2026-09-09 L27 配置、路径与外部文件变化已完成离线收口（真实宿主待验收）

- 设置页目录校验现在区分可由 Worker 创建的缺失叶目录、指向文件的路径，以及磁盘/网络共享不可访问；校验只读，不为每次输入创建目录。便携设置的无效值仍在复制前拒绝，保留当前编辑值不变。
- 媒体同步对用户配置的媒体来源做可读性确认；来源消失、访问被拒或扫描期间不可达会产生 `MEDIA_SOURCE_UNAVAILABLE`，占用文件会产生 `MEDIA_FILE_UNAVAILABLE`，源文件不会被删除。默认系统来源仍按可选缺失处理。
- 存档路径探测对用户明确传入的 `AdditionalRoots` 使用严格错误语义，产生 `SAVE_PATH_ROOT_UNAVAILABLE`，不再把不可访问目录当作空候选；默认系统目录继续跳过不可用路径。补充 Unicode/长文件名、占用文件和缺失来源夹具。
- 验证：Release 构建 `0 warning/0 error`；Core `76/76`；Worker `308/309`（1 项真实进程重启测试在沙箱跳过）；Playnite `419/482`（63 项 UI/宿主条件跳过）；设置/便携导入定向 `11/11`，媒体/存档路径定向 `16/16`；`validate-source.py`、XAML `19/19`、`git diff --check` 通过。没有真实 Playnite/FusionX、网络共享 ACL、DPI 或用户视频复测，仍待宿主验收。

## 2026-09-09 L22 缩略图加载、取消与缓存边界已完成（真实宿主待验收）

- `AsyncThumbnailImage` 现在只在已加载且可见时发起加载；不可见/卸载会取消并清空旧图，路径或尺寸变化通过代际号阻止旧结果回写。`AsyncThumbnailLoader` 保留 `OnLoad` 冻结位图、3 路并发和 96 项 LRU，破损/缺失文件返回空占位而不抛出到列表。
- 新增内部计数探针记录请求、缓存命中、解码开始/成功、活动/峰值并发、取消、失败和缓存数量；Playnite STA 回归覆盖“不可见不启动”和“旧路径不串图”，RenderHarness `thumbnailprobe` 覆盖 120 项、失败文件、预取消、100 次 12 项窗口往返和文件删除边界。
- `.tmp/l22-thumbnailprobe-final/thumbnailprobe-report.txt` 实测：120/120 解码成功，峰值并发 `3`，缓存 `96/96`；保留窗口命中 `16` 次且未新增解码；破损/缺失均为空且失败 `2`；预取消 `1` 次；100 次往返请求增量 `1200`、缓存命中增量 `1089`、解码增量 `111`，活动解码归零；800×800→64×64 路径替换最终为新图，解码宽度 `96`，合成像素标记 `231`。
- 这证明插件加载器、可见性生命周期和缓存边界在合成 STA 夹具中成立，不证明真实 Playnite/FusionX 模板、DPI、用户目录文件系统或视频帧级表现；真实宿主按原视频操作矩阵待验收。

## 2026-09-09 L21 列表虚拟化与滚动规模实测已完成（真实宿主待验收）

- 根因证据不是任务/收件箱 DataGrid 集合或页面整体移动：规模探针发现当前游戏 `MediaGrid` 的 ItemsPanel 是普通 `WrapPanel`，200/2000/10000 后端夹具分别生成 200/2000/2000 个卡片容器。`VirtualizingPanel.IsVirtualizing` 对普通 WrapPanel 不会产生虚拟化。
- 生产修复仅将当前游戏 `MediaGrid` 的 ItemsPanel 接回现有 `ui:VirtualizingWrapPanel`，固定卡片几何仍为 164×154、0 间距，ListBox 的选择、绑定、滚动条和模板保持不变。修复后 200/2000/10000 后端场景的当前媒体 UI 窗口为 200/2000/2000，顶部/底部/回顶部均约 20 个容器；任务表和收件箱表继续使用原有 Item 滚动、行列虚拟化与 Recycling。
- 任务表 200/2000/10000、收件箱 200/2000/10000 的离屏滑块往返、滚轮、PageUp/PageDown、Ctrl+End、resize、选择保持均通过；诊断记录实际 `ScrollViewer`、`ScrollContentPresenter`、offset/viewport/extent、首末稳定 ID/Y/height、单元格内容与水平条状态，未报空正文或选中行内容缺失。直接 `ScrollIntoView(最后一项)` 在插件模板与标准 WPF 对照模板中都未在离屏探针即时定位，保持 `offscreen-inconclusive`。
- 完成证据：`.tmp/l21-scaleprobe-final/scaleprobe-report.txt`、`.tmp/l21-render-qa-final/render-qa-report.txt`。全量 Render QA 的当前媒体卡片回滚已通过；剩余失败是既有 Media 小视口/预览列表门禁和 Sidebar rapid-toggle，不能写成全量通过。无真实 Playnite/FusionX、DPI 或用户视频回归环境，因此视频中的 DataGrid 空白/文字分离仍需宿主按原验收矩阵复测。

## 2026-09-09 L20 修改器导入与下载结果反馈已完成（真实宿主待验收）

- 修改器导入检测和下载提交都在异步操作开始时捕获稳定的游戏/目录/版本 ID。多候选导入在检测返回或确认期间切换游戏会被丢弃或取消；成功返回后只有仍在同一游戏时才刷新详情，避免把晚到结果写入错误上下文。
- FLiNG 下载结果区分排队、真实任务进度、已下载绑定、重复绑定、站点拒绝、格式拒绝、版本解析失败、取消和通用失败，并给出刷新/重试/手动导入下一步。取消复用 Worker `CancelTask`，不自动运行下载内容，不绕过 Worker 的来源、签名、解压和临时文件清理安全链。
- 本轮只改 Trainer 状态反馈与导入代际保护，没有改页面布局路线、虚拟化或新增下载协议；RenderHarness Trainer 双主题、多尺寸/resize 未报新增问题。真实 Playnite/FusionX、在线 403/离线、真实取消时序、DPI 和用户视频复测仍待宿主验收。
- 验证：Release `0 warning/0 error`；Core `76/76`、Worker `303/304`（1 跳过）、Playnite `405/467`（62 跳过）；RenderHarness Release 构建、源校验、XAML `19/19`、WPF 静态审查 `0 errors/21 warnings/172 info`、差异检查通过。完整 render-qa 的已知失败仍为 Media 小视口/预览列表和 Sidebar rapid-toggle。

## 2026-09-09 L19 存档版本识别、比较与恢复信息已完成（真实宿主待验收）

- 存档版本详情补齐来源设备、操作系统、锁定状态、恢复就绪状态与最近检查时间；来源/系统缺失明确显示“未知”，未根据时间或文件数量推断健康。恢复确认前捕获游戏 ID、版本 ID 和只读事实摘要，确认后请求不再读取可能已变化的选择；PreRestore、锁定当前快照、游戏关闭确认和撤销语义保持不变，Worker 仍是最终安全校验方。
- 比较页明确当前选中版本与上一版本的比较范围，补充未变化文件数、总大小增量和新增/变化/删除分组标题；选择版本变化或无可比较上一版本时清理旧结果，响应返回前同时校验游戏、当前版本和上一版本 ID，避免旧比较结果留在新上下文。
- 验证：Release 构建 `0 warning/0 error`；Core `76/76`、Worker `303/304`（1 跳过）、Playnite `404/466`（62 跳过）；`validate-source.py`、XAML `19/19`、WPF 静态审查 `0 errors/21 warnings/172 info`、`git diff --check` 通过。离屏 Save 历史/比较页截图与双主题、多尺寸探针未报本轮新增问题；完整 render-qa 仍以既有 Media 小视口/预览列表与 Sidebar rapid-toggle 失败退出。当前无可用真实 Playnite/FusionX 宿主，确认框、不同恢复状态、DPI、真实滚动与录屏仍待宿主验收。

## 2026-09-09 L18 批量动作提交前摘要已完成（真实宿主待验收）

- 媒体批量归类、忽略、恢复和归类预览现在在提交前捕获去重后的稳定媒体 ID、目标游戏/批次、原始选择数、重复项和无稳定 ID 项；确认后只执行捕获快照，确认期间列表选择变化不会改写提交范围。只读预览不新增确认层，归类复用现有确认流程。
- 任务批量重试继续只处理当前 `TasksView` 结果，按游戏/任务类型去重；现在排除缺少稳定 `TaskId` 的记录，并用确认时的任务快照执行。媒体收件箱和归类建议结果分别显示计划、成功、失败、冲突、跳过/未返回，部分结果不再提示为全量成功。
- 验证：Release 构建 `0 warning/0 error`；全量 Core `72/72`、Worker `303/304`（1 跳过）、Playnite `404/466`（62 跳过）；新增批量动作契约测试包含在 Playnite 全量中；`validate-source.py`、XAML `19/19`、WPF 静态审查 `0 errors/21 warnings/172 info`、`git diff --check` 通过。本轮未修改 XAML，未重复 render-qa；真实 Playnite/FusionX 确认框、选择变化、部分失败和录屏仍待宿主验收。

## 2026-09-09 L17 常用筛选与工作区状态记忆已完成（真实宿主待验收）

- 任务状态、游戏、类型、搜索、历史范围和时间范围现在都属于同一套查询条件持久化；动态游戏/类型筛选会等任务选项重建后恢复，当前 Playnite 游戏库已删除的游戏会回到“全部”，仍在游戏库但暂时不在最近任务页的游戏会保留为可选项。保存的历史范围/时间范围重启后会重新激活服务端历史分页。旧配置/非法值会归一化，不恢复瞬时选择、任务状态或自动执行批量命令。
- 媒体当前游戏页新增 `ClearMediaFiltersCommand` 和按状态显示的“清除”入口，一次清理媒体搜索与类型条件，取消旧的防抖查询并沿用真实媒体分页/代际保护；不触发媒体编辑、归类、批量操作或强制刷新。
- 验证：Release 构建 `0 warning/0 error`；全量 Core `72/72`、Worker `303/304`（1 跳过）、Playnite `403/465`（62 跳过）；迁移/持久化定向测试 `10/10`；`validate-source.py`、XAML `19/19`、WPF 静态审查 `0 errors/21 warnings/172 info`、`git diff --check` 通过。完整 render-qa 仍有既有 Media 小视口/预览列表与 Sidebar rapid-toggle 失败，本轮未新增筛选工具栏或主题/resize 失败；真实 Playnite/FusionX 重启恢复、删除游戏、DPI、键盘和宿主录屏仍待验收。

## 2026-09-09 L16 媒体收件箱操作可达性已完成（真实宿主待验收）

- 收紧收件箱批量操作栏的选择摘要、模式、目标游戏和预览入口宽度，1040 DIP 内容宽度下仍保持批量处理、归类和预览入口同一行；没有改变多选、目标校验、预览、历史或撤销命令。
- 生产壳层 1040×700/1100×720 的 PageHost 实际高度约为 577/597 DIP。媒体页在 `<620` DIP 时使用明确的页面纵向滚动通道，DataGrid 仍保持有限高度、Item 滚动、行列虚拟化和 Recycling；不添加固定底部补偿。离屏壳层表格分别为 300 DIP，可见 6 行，页尾滚到底后 footer、批次历史和次级操作均完整落入视口。
- RenderHarness 新增真实“滚到页面末尾”可达性探针；源门禁定向 `5/5`。Release 构建 `0 warning/0 error`；全量 Core `72/72`、Worker `303/304`（1 跳过）、Playnite `402/464`（62 跳过）；`validate-source.py`、XAML `19/19`、`git diff --check` 通过。完整 render-qa 的剩余失败仍是媒体直接视图的小视口/预览列表门禁和侧栏 rapid-toggle，不把它们写成全量通过。真实 Playnite/FusionX、DPI、视频式拖动回归和真实录屏仍待宿主验收。

## 2026-09-09 L15 任务页查错与范围说明已完成（真实宿主待验收）

- 任务摘要现在同时说明运行中、排队/等待确认、当前已加载筛选结果中的可重试数，以及失败/已取消数；不把“当前结果可重试”误写成全历史或整库操作。批量重试按钮的提示和自动化名称明确它只处理当前已加载且符合筛选的记录，现有按游戏/任务类型去重、确认和安全重试链路不变。
- 活跃筛选摘要在宽屏任务队列标题区直接可见，仍保留“更多筛选”中的清除入口；已加载条数、服务端查询总数、最近任务/全部历史和时间范围继续分开显示。失败行的完整详情、错误码、任务 ID、复制详情和安全重试入口未被截断摘要替代。
- Release 构建 `0 warning/0 error`；任务定向测试 `36/42`（6 跳过），全量 Core `72/72`、Worker `303/304`（1 跳过）、Playnite `402/464`（62 跳过）；`validate-source.py`、XAML `19/19`、`git diff --check` 通过。离屏 Task 在 1040×700 保留 6 行、1100×720/1366×768 保留更高首屏，双主题和 resize 通过；完整 render-qa 稳定失败仍是既有媒体小视口/媒体壳高度，首轮另有侧栏 rapid-toggle 未稳定。真实 Playnite/FusionX、DPI、键盘和长历史人工操作仍待验收。

## 2026-09-09 L14 运维总览按处理顺序组织已完成（真实宿主待验收）

- 维护诊断概览不再把所有动作渲染成同等权重的长列表：真实动作按“需要人工处理 / 等待自动重试 / 例行巡检”分组，每组默认只展示前 3 条；其余记录通过明确的“显示其余 N 项”展开器保留在同一上下文。没有新增批量自动修复，云端 `TransferKey`、隔离账本 `EntryId` 和巡检命令参数继续沿用原动作对象。
- 空分组不生成空卡片；分组计数显示该组总量，默认列表与溢出列表分别绑定 `PreviewItems` / `OverflowItems`。长标题仍有省略提示，详情、时间、风险说明和逐条动作入口均保留。RenderHarness Fake 同步使用同一分组形状。
- 新增分组与 0/1/20 条边界契约（含长标题）并更新维护页静态门禁。Release 构建 `0 warning/0 error`；Core `72/72`、Worker `303/304`（1 跳过）、Playnite `402/464`（62 跳过）；`validate-source.py`、XAML `19/19`、`git diff --check` 通过。离屏维护页双主题/多尺寸/resize 通过；完整 render-qa 的稳定失败仍是既有媒体小视口/媒体壳表格高度，首轮另有一次侧栏 rapid-toggle 未稳定，单独 shellqa 重跑已稳定。真实 Playnite/FusionX、DPI、键盘和宿主模板仍待验收。

## 2026-09-09 L13 首页优先级与活动上下文已完成（真实宿主待验收）

- Hero 继续只有一套 `OverviewPriorityResolver` 优先级入口：Worker 离线、首次准备、云端待处理、媒体待归类、空库、游戏告警、健康刷新按明确顺序决策；空库不再误显示“整体状态安全”，云端失败继续使用快照 `AttentionCount` 统一口径。
- 全局活动保留真实对象名和本地时间，并携带稳定 `PlayniteId`；活动行现在是可聚焦/可键盘触发的单一命令入口，按备份/恢复、媒体、工具、云端或维护路由到对应工作区，游戏存在时先恢复同一游戏上下文。没有新增第二套 Hero、重复集合刷新或隐藏全局入口。
- Release 构建 `0 warning/0 error`；Core `72/72`、Worker `303/304`（1 跳过）、Playnite `401/463`（62 跳过）；`validate-source.py`、XAML `19/19`、`git diff --check` 通过。完整 render-qa 的 Overview 双主题/四尺寸与 resize 探针通过，仍只报告既有媒体小视口和媒体壳表格高度问题；真实 Playnite/FusionX 点击路由、主题/DPI 与截图仍待宿主验收。

## 2026-09-09 L11 通知、长错误与复制详情已完成（真实宿主待验收）

- 通知事件现在同时携带短摘要和完整 `DetailMessage`：普通成功/信息摘要限制为 180 字符，错误/警告保留 320 字符摘要；任务详情仍保留完整错误码、任务 ID 和原始详情，不再把截断摘要当作唯一证据。
- Dashboard 错误 Toast 的“查看详情”打开可滚动的完整详情，并提供“复制详情”；复制绑定点击时捕获的详情快照，复制重试完成后会检查当前对话框仍是同一详情，不会改写新错误。长成功/警告消息若有独立详情也可进入同一详情面板，常规反馈不再堆成长卡片。
- 取消任务使用 `UiNotificationKind.Warning`；未改变任务重试、取消确认、诊断包或 Playnite 宿主保存语义，也没有把所有错误改成模态弹窗。
- 验证：Release 构建 `0 warning/0 error`；Core `72/72`、Worker `303/304`（1 跳过）、Playnite `398/460`（62 跳过）；`validate-source.py`、XAML `19/19`、`git diff --check` 通过。完整离屏 `render-qa` 仍只报已有媒体小视口/侧栏快速切换问题，未报通知相关问题；真实 Playnite/FusionX 多任务连续完成、长错误复制和宿主通知回退仍待验收。

## 2026-09-09 L12 详情展开与选中上下文已完成（真实宿主待验收）

- 媒体收件箱现在把新行选择视为新的详情上下文：选中变化会关闭旧的紧凑预览，切换“待归类/已忽略”同时关闭预览和批次历史，必须由用户显式重新打开当前对象详情；媒体当前列表、任务、存档、维护各选择处理器继续在选中对象变化时关闭旧 Inspector。
- 维护云端表格的选择事件改为只保留 XAML 声明的一条路由，删除构造函数重复挂接，避免同一次选择触发两次布局；同一 Inspector 控件、真实 Binding、虚拟化与宽/窄布局滚动策略均未改变。
- 新增详情展开源契约，覆盖任务/媒体/存档/维护选择清理和云端事件单路由。全量 Release 构建 `0 warning/0 error`；Core `72/72`、Worker `303/304`（1 跳过）、Playnite `399/461`（62 跳过）；`validate-source.py`、XAML `19/19`、`git diff --check` 通过。未重复运行完整 render-qa；真实 Playnite 中对象删除、快速换选、宽窄来回和 FusionX 模板仍待宿主验收。

## 2026-09-08 L10 设置修改、错误定位与取消体验已完成（真实宿主待验收）

- 校验摘要现在有键盘/鼠标均可触发的“定位首个错误”入口，按现有验证消息切换到常规、备份、外观或自动化分类，并把焦点送回分类导航；不新增插件保存命令，Playnite 保存/取消生命周期保持原样。
- 设置脏状态覆盖 TextBox、ComboBox、CheckBox、ToggleSwitch 和毛玻璃 Slider；此前 ToggleSwitch/Slider 只刷新主题或没有统一刷新摘要，跨分类编辑可能看不到“未保存更改”，现已统一进入校验状态更新队列。
- 导入继续以原编辑基线比较：导入成功后重绑同一设置对象不会把导入内容伪装成已提交；`DeviceId` 仍排除在用户编辑指纹之外，已有 CancelEdit/导入无变异/无效包测试继续有效。
- RenderHarness 实测校验备份字段时 `SettingsValidationNavigation selectedCategory=1`，normal/dirty/invalid 三态仍通过；全量 Release 构建 `0 warning/0 error`，Core `72/72`、Worker `303/304`（1 跳过）、Playnite `395/457`（62 跳过）。Playnite 宿主保存失败提示、实际取消按钮和 FusionX/DPI 键盘轨迹仍待真实宿主验收。

## 2026-09-08 L09 设置首屏与保存状态已完成（真实宿主待验收）

- 设置页首屏移除重复的全宽说明，Hero 副标题压缩为工具路径/存档策略/外观与自动化；分类卡片内的就地说明和 Playnite 保存/取消语义保留，核心 Worker/Ludusavi 字段在 1040×700 首屏更早可见。
- 修正校验状态语义：验证摘要出现时，右上角不再误显示“已保存”，而是统一显示“存在校验错误 · 保存前请修正”；正常、未保存、失败三种状态仍只保留一个主要保存状态入口。
- RenderHarness 增加 1040×700 设置三态夹具和重复说明布局门禁，实测 normal=`已保存`、dirty=`有未保存更改`、invalid=`存在校验错误`；760–1400 DIP/560–900 DIP 分类与正文滚动探针通过。完整 `render-qa` 仍只报告已有媒体小视口/侧栏动效问题，没有新增 Settings 问题。
- Release 构建 `0 warning/0 error`；全量测试 Core `72/72`、Worker `303/304`（1 跳过）、Playnite `395/457`（62 跳过），源码/XAML/差异校验通过。离屏夹具不等同真实 Playnite/FusionX 设置窗口，主题/DPI/Playnite 保存取消仍待宿主验收。

## 2026-09-08 L08 可重复诊断与性能采样入口已完成（真实宿主待验收）

- RenderHarness 的 `render-qa`、`gridprobe`、`shellqa` 报告现在带有场景、证据来源、Git 提交/工作树状态、窗口 DIP、主题、数据量和明确的时序字段；布局与渲染时长分开记录，离线 Harness 明确不伪造请求耗时与真实 DPI。
- `scripts/real-host-audit.ps1` 增加 `runner-metadata.json`，记录真实宿主审计的提交、配置、窗口 DIP、实际 DPI、主题、生产数据量和采集清单状态；已有真实宿主捕获元数据继续作为实际来源，未修改用户 FusionX 文件。
- 诊断包 `system.json` 增加场景、证据来源、窗口 DIP、已加载条目数、数据量和 Worker 查询耗时；布局耗时在没有真实布局采样时明确为空，不写入媒体内容、文件内容或敏感路径，不新增业务写入。
- Release 构建 `0 warning/0 error`；全量测试 Core `72/72`、Worker `303/304`（1 跳过）、Playnite `395/457`（62 跳过），`gridprobe OK`、源码/XAML/差异校验通过。以上是代码与离线证据，不等同真实 Playnite/FusionX 录屏、性能结论或视频异常已解决；真实宿主仍待验收。

## 2026-09-08 L07 键盘、焦点与可访问名称已完成（真实宿主待验收）

- 媒体、任务、存档、维护和工具页的紧凑详情抽屉现在显式接收 Esc：打开时焦点进入真实 inspector ScrollViewer，关闭后回到仍可见的原入口；媒体批次历史回到“批次历史”按钮，宽屏常驻右侧详情不会被 Esc 误关。
- 抽屉 ScrollViewer 明确 `Focusable=True`、`KeyboardNavigation.IsTabStop=False`，避免隐藏控件残留焦点和 Tab 陷阱；已有紧凑入口按钮继续带 `AutomationProperties.Name`，没有劫持宿主快捷键或改变业务命令。
- 新增 `KeyboardFocusSourceTests`，包含实际 STA WPF 控件实例检查和五页键盘/自动化契约检查。Playnite 全量 `394/456`，其中 `62` 项历史/离屏项跳过，0 失败；Release 构建、源码、XAML 和差异校验通过。
- 代码级 STA/契约证据不等于 FusionX/真实 Playnite 人工键盘验收；搜索→选中→开关详情、Enter/Space、Esc、DPI/高对比度和真实宿主焦点轨迹仍待宿主复测。

## 2026-09-08 L06 目的导航与返回上下文已完成（真实宿主待验收）

- 诊断到存档路径的入口现在必须在当前快照中找到同一 `PlayniteId` 才切换 `SelectedGame`；目标消失时停留原工作区并明确提示，不再悄悄沿用当前游戏。
- 诊断到失败任务使用一次性的目标游戏上下文，按稳定 ID 或诊断名称筛选并在返回后选中匹配任务；保留原搜索、类型、时间和范围条件，连续跳转由现有任务请求代际与取消链路收敛，不依赖固定延时或并发刷新。
- 维护页动作继续通过真实 `TransferKey` 分页恢复指定云端记录；普通侧栏切换复用已创建的生产工作区实例，媒体/存档/维护标签使用双向绑定保留会话内位置。新增目标解析与导航源契约测试；Playnite 全量 `392/454`，其中 `62` 项历史/离屏项跳过。
- 本阶段只证明插件代码与离线测试；真实 Playnite/FusionX 中的诊断入口、筛选隐藏目标和快速连续点击仍待宿主按手工清单复测。

## 2026-09-08 L05 六态与运维页夹具已完成（真实宿主待验收）

- RenderHarness Fake 新增 `Ready/Empty/Loading/Error/Stale/Offline` 六态、媒体详情/收件箱/维护状态绑定、任务页 Loading/Error 映射，以及恢复巡检、云端重试、隔离账本三条真实形状的 `MaintenanceActionItem`。关键绑定缺失会在 `statefixtures` 报告中直接失败。
- 新增 `statefixtures` 入口，使用生产 `MediaCenterView`、`MaintenanceView`，覆盖浅色/深色和 `1040×700 / 1100×720 / 1366×768 / 2560×1440`；报告检查页面截图、数据表面几何、状态覆盖层、Stale 提示和动作项数量。
- 夹具首次复现 `Stale` 提示条把媒体 `DataGrid` 测量为 `693×0`：短但非 fallback 高度关闭外层页面滚动，提示条、工具栏和页脚耗尽表卡空间。`MediaCenterView` 现让可见 Stale 提示启用外层内容滚动，恢复有限的 `MediaInboxGrid` 视口；未添加固定底部补偿。
- L05 离屏结果：`statefixtures OK`；定向 `WorkspaceStateSourceTests` 为 `8 通过 / 1 跳过`；RenderHarness Release 构建 `0 warning / 0 error`。截图只证明插件生产页离屏夹具，不等同真实 Playnite/FusionX 录屏；真实宿主仍待验收。

## 2026-09-08 Q6-04 构建身份闭环已完成（隔离包验收）

- `WorkerLauncher` 现在只在两个构建身份都已知且不一致时拒绝复用；旧 Worker 缺少 `BuildIdentity`、或任一侧为 `+unknown` 时，先完成版本/协议校验并以“身份未验证”状态兼容，不把未知身份冒充为同源。
- 未知身份仍会写入健康探测详情，便于诊断区分“可用但未证明同源”和“已知身份冲突”；同版本不同已知提交继续标记为不可复用。
- 定向 `BuildIdentityTests` 已覆盖已知冲突、旧 Worker 空字段和 unknown 组合（`3/3`）。隔离正例包六个程序集同源；`-SkipBuild` 混合 DLL、`+unknown`、脏工作树、无 Git 和环境变量恢复负例均按预期失败或恢复，未触碰真实 Playnite 安装目录。
- 文档提交后的最终 Git HEAD 已重新完成 Release 打包，包内身份与六个程序集一致；真实安装目录和宿主握手仍不纳入本阶段结论。

## 2026-09-08 两项截图问题：代码修复与真实安装验收状态

- Worker 使用命名互斥锁时，重复启动实例会以退出码 0 正常结束。`WorkerLauncher` 现在会在发现该退出码后限时探测现有 Worker；确认同版本且健康时复用现有实例并结束本次启动，不再把正常的重复实例退出误报为“Worker 启动后立即退出”。现有实例仍不健康时继续保留真实失败日志与错误提示。
- 媒体中心“待归类”页的 DataGrid 原先被外层页面滚动内容的 `*` 行安排到表格卡片底部，宽屏下因此出现异常高的空白区域。这不是设计意图；表格卡片、内部布局和 DataGrid 已改为顶部对齐，仍保留 DataGrid 的有限高度、内部滚动和虚拟化。Production Shell 几何门禁新增表卡到表格的顶部间距检查，1366×768 已从 `477` DIP 降至 `63` DIP。
- Worker 身份修复已完成真实安装链路：`scripts/package.ps1` 在包内读取插件、Worker、Core/Contracts DLL 的实际 InformationalVersion，并要求都等于最终打包时的 Git HEAD。安装目录为 `C:\Users\lopmatu\AppData\Roaming\Playnite\Extensions\GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec`，运行中 Worker 为该目录下的唯一进程；最终包的完整身份以最后一次打包输出和握手记录为准。
- 真实命名管道 `system.handshake` 返回成功、协议 `1`、Worker `0.6.73.0` 和包内同一构建身份；真实只读 `media.inbox.page` 返回成功（当前数据 `totalCount=4615`）。受控停止并恢复唯一已核实路径的 Worker 时，停止后管道确实不可达，恢复后握手再次成功；恢复日志没有重复实例或退出码 0 循环。
- 媒体待归类页已改为有限 PageHost、左侧工具栏/表格/底部操作独立 Grid 行、左右独立滚动、右侧有限详情和离线非零假状态；源码/XAML/离屏几何门禁已通过。最新隔离 Release 构建与全量测试为 `0 warning/0 error`，Core `72/72`、Worker `303/304`（1 跳过）、Playnite `376/438`（62 跳过）。
- 真实 Playnite 的窗口内视觉操作、宽→窄→宽切换、实际截图及 PageHost/滚动范围采集仍未完成：本会话没有可用的 CUA 端点，不能把 RenderHarness 截图冒充宿主截图。Q6-01 已完成代码与离屏行为收口，真实宿主仍待验收；32 项扩展计划继续按依赖推进。

## 2026-09-08 Q6-01 状态面板重试输入已收口（代码/离屏）

- 根因已由真实 WPF 模板行为测试确认：三个失败态 `WorkspaceStatePresenter` 使用点把整个面板设为 `IsHitTestVisible="False"`；同时共享 `Redesign.xaml` 用 `DataTrigger` 判断 `RetryCommand` 空值时，Failure/Offline 按钮仍被模板触发器保持 `Collapsed`。命令本身没有丢失，不能把现象归因于 ViewModel 或计数。
- 修复范围仅限插件：移除 MediaInbox、MediaDetails、Maintenance Audit 三个带重试命令面板的局部禁止命中；共享模板改用针对 `RetryCommand` 依赖属性的 `Trigger`，Loading 明确隐藏重试按钮但保留阻挡层，并补齐重试按钮自动化名称。未修改 FusionX、Playnite 全局样式或业务命令。
- 真实模板 STA 行为测试 `WorkspaceStatePresenterBehaviorTests` 为 `5/5`：Error/Offline 按钮可见、绑定命令且命中目标；Loading 不穿透底层；Enter/Space 各执行一次。双主题 Error/Offline/Loading 状态探针 `stateprobe OK`，截图保存在 `docs/design/reviews/2026-09-08-quality/`。
- Release 隔离构建/全量测试为 0 warning/error：Core `72/72`、Worker `303/304`（1 跳过）、Playnite `376/438`（62 跳过）。`validate-source.py`、XAML `19/19`、WPF 静态检查均无 error。完整 `render-qa` 本次报告仍有 25 个已有媒体小视口/侧栏快速切换门禁问题，未将其误报为 Q6-01 通过；真实 Playnite 仍待宿主验收。

## 2026-09-08 Q6-02 媒体状态按上下文隔离（代码/测试已完成）

- 根因已确认：媒体详情和收件箱原先各自只有一份全局成功时间/错误；游戏、筛选或收件箱模式切换后，任意旧成功都能把新上下文的首次失败误判为 Stale，并显示旧上下文时间。
- 修复：新增按上下文键工作的 `MediaWorkspaceStateCache`。媒体详情键包含游戏 ID、媒体筛选和搜索词；收件箱键包含模式。同上下文刷新仍保留成功时间并进入 Stale，新上下文先清空旧缓存语义；筛选/搜索/选中游戏变化立即推进媒体请求代际并取消旧请求，晚到响应不能完成当前上下文。离线标题/消息也统一从有效状态派生。
- 新增 `MediaWorkspaceStateCacheTests`：同上下文失败保留 Stale 时间、A 成功/B 首失败为 Error、旧响应不能覆盖 B、待归类/已忽略时间隔离、无缓存取消不伪造 Ready，共 `4` 项。
- Release 隔离构建与全量测试：0 warning/error；Core `72/72`、Worker `303/304`（1 跳过）、Playnite `380/442`（62 跳过）。XAML `19/19` 通过；本项无 XAML 改动，未重复运行完整 `render-qa`，此前 25 个独立小视口/侧栏问题仍按原边界保留。真实 Playnite 状态切换、故障注入与录屏仍待宿主验收，不能据此宣称视频问题已完全解决。

## 2026-09-08 Q6-03 运维云端告警归并与时间语义（代码/测试已完成）

- 根因已确认：维护页先对快照和分页明细分别筛选需关注状态，再按 `TransferKey` 取最后一条；旧快照 Failed 会把新明细 Uploaded 在去重前过滤掉。云端 `LastAttemptUtc` 也被标成“上次验证”，把上传尝试和远端验证混为一谈。
- 修复：新增 `MaintenanceCloudTransferResolver`，先按 `UpdatedUtc` 合并全部来源，同时间由分页明细优先，再筛需关注状态；摘要计数同步扣除被明细解决的快照告警并计入明细新增告警。云端显示“上次尝试”，隔离账本显示“账本更新”，恢复巡检保留“上次验证”。
- 新增 5 项归并/时间语义测试，覆盖新成功覆盖旧失败、新失败覆盖旧成功、同时间来源优先、摘要计数修正和时间标签。Release 全量：0 warning/error；Core `72/72`、Worker `303/304`（1 跳过）、Playnite `385/447`（62 跳过），源码校验通过。
- 本项仅改 ViewModel 和测试，无 XAML 改动，未重复运行完整 `render-qa`；此前独立小视口/侧栏门禁仍按原记录保留。真实 Playnite 告警刷新、分页变化和故障注入仍待宿主验收。

## 2026-09-08 连续开发队列（32 项）

- 用户要求增加任务计划，减少逐项来回确认。实施入口改为 [32 项 / 8 阶段连续开发计划](CONTINUOUS_DEVELOPMENT_PLAN_2026-09-08.md)，L01～04 承接 Q6，后续覆盖验收底座、设置/共享交互、四个主要页面、常用流程、大库性能、稳定性与发布。
- 接手后核对最新实现，从首个依赖满足的未完成项持续推进；每项完成验证/文档/提交/push 后继续，不逐项询问。外部阻塞记明并继续独立项；已满足的功能给证据后跳过，不重复实现。
- 计划创建阶段仅扩展规划；本轮已按用户截图反馈修复 Worker 重复启动误报与媒体待归类表格布局。新计划区分缺陷、增强与先测后改，不额外授权真实数据写入或安装；质量依据仍见下方独立复核。

## 2026-09-08 Q4/Q5/X2 独立质量复核（仅文档）

- 基线 `97131f0`，新增 [完成质量与后续计划](QUALITY_REVIEW_2026-09-08.md)。Q4/Q5 主体已落地，维护/任务 Shell 紧凑可见行数改善；X2 仍需收口，不能只凭实现记录判全部验收。
- 优先 Q6-01 状态面板重试命中、Q6-02 游戏/模式状态隔离、Q6-03 运维告警最新状态归并、Q6-04 SkipBuild/unknown 构建身份；之后完善六态/运维夹具、设置首屏与真实发布验收。
- 本轮独立实跑 Release 0 warning/error；Core 72 通过、Worker 303 通过/1 跳过、Playnite 368 通过/62 跳过；XAML 19/19、源码门禁、静态 UI 0 errors、render-qa OK。证据 `docs/design/reviews/2026-09-08-quality/`；未安装或验证真实 Playnite。

## 2026-09-08 X2-03 构建身份核验底座已落地

- `Directory.Build.props` 在不改变 `0.6.73` 公共版本的前提下，把 `GSC_BUILD_COMMIT` 嵌入程序集 InformationalVersion；打包脚本从当前 Git HEAD 提供该值，未能取得提交时明确为 `unknown`。
- Worker 的 `system.handshake` 和兼容旧客户端的 `system.ping` 返回构建身份；Dashboard、维护页和脱敏诊断包同时记录 Worker 构建身份，诊断包也携带插件构建身份。插件启动时对已知身份执行同版本不同构建检测，旧 Worker 缺字段时仍按原版本兼容路径工作。
- 当前验证：Release 构建 0 警告/0 错误；Core `72/72`、Worker `303/304`（1 跳过）、Playnite `368/430`（62 跳过）；源码/XAML 门禁、WPF `0 errors/21 warnings/172 info`、双主题/多尺寸/resize/Production Shell `render-qa OK`。本阶段未改版本、未安装插件；下一次实际发布仍需核对安装包、程序集、握手和 Playnite 已安装 DLL 的身份一致性。

## 2026-09-07 X2-02 运维总览与失败下一步已落地

- 维护页“诊断概览”新增真实的“下一步运维”列表，统一展示恢复巡检、云端失败/认证/重试记录和清理隔离账本：每项都带上次验证、下次尝试、当前状态、错误详情和明确动作；云端项可定位到原队列记录，未加载完的队列明确提示继续分页，不把加载窗口当全集。
- 新增隔离账本只读 IPC 与按 `EntryId` 定位的再次协调 IPC。再次协调必须经过 Playnite 明确确认，Worker 仍复用原有路径安全、文件身份和索引状态机；冲突或不安全路径保留文件并继续标为需人工确认，不提供无目标的“全部自动修复”。
- 当前验证：Release 构建 0 警告/0 错误；Core `72/72`、Worker `303/304`（1 跳过）、Playnite `366/428`（62 跳过）；XAML `19/19`、源码校验、WPF 静态审查 `0 errors/21 warnings/172 info`、双主题/多尺寸/resize/Production Shell `render-qa OK`。真实 Playnite、DPI/高对比度和完整键盘仍需人工验收。

## 2026-09-07 X2-01 工作区状态体验已落地

- 媒体当前游戏列表、媒体收件箱和维护诊断现在共享 `WorkspaceDataState`：Loading、Ready、Empty、Stale、Error、Offline。状态来自真实 IPC 请求、分页代际和 Worker 快照，不使用延时模拟；旧数据在刷新失败时保留，并显示上次成功读取时间、失败详情和重试命令。
- `WorkspaceStatePresenter` 已接入媒体当前列表、收件箱和维护诊断表；正常空结果仍显示下一步说明，加载/首失败显示阻塞状态，已有数据刷新失败显示降级提示，不再把旧列表误标为最新结果。媒体编辑草稿、选择、批量命令、虚拟化和有限滚动视口保持不变。
- 当前验证：Release 构建 0 警告/0 错误；Core `72/72`、Worker `302/303`（1 跳过）、Playnite `365/427`（62 跳过）；XAML `19/19`、源码校验、WPF 静态审查 `0 errors/21 warnings/172 info`、双主题/多尺寸/resize/Production Shell `render-qa OK`。真实 Playnite、DPI/高对比度和完整键盘仍需人工验收。

## 2026-09-07 媒体待归类间距与任务统计边界已收口

- `MediaCenterView` 的待归类页签增加了外边距、按钮内边距和页签间距；批量处理卡片使用 `14,12,14,0` 内边距，底部操作区使用 `16,12,16,0` 与 `0,8,0,0` 间距，解决按钮贴边问题，同时保留真实命令、选中语义、DataGrid 虚拟化和有限滚动视口。
- 任务首页现在独立加载全部活动任务，不会因最近 50 条历史记录窗口而隐藏旧的运行中/等待用户任务；云端等待统计包含 `WaitingForUser`。任务历史摘要明确区分“全部历史”和“当前筛选”，批量安全重试只针对当前结果，并说明去重和跳过原因。
- 当前验证：Release 构建 0 警告/0 错误；Core `72/72`、Worker `302/303`（1 跳过）、Playnite `364/426`（62 跳过）；XAML `19/19`、源码校验、WPF 静态审查 `0 errors/20 warnings/172 info`、双主题/多尺寸/resize/Production Shell `render-qa OK`。真实 Playnite、DPI/高对比度和完整键盘仍需人工验收。

## 2026-09-07 任务页紧凑空间门禁已补齐

- `TaskCenterView` 在紧凑详情展开时不再让队列表底部的“收起任务详情”按钮覆盖 DataGrid 行：按钮移入详情卡片，队列保持有限表格视口；无有效筛选时，更多筛选标题中的“清除”按钮自动隐藏，不再占用首屏空间。搜索、状态、类型、游戏、范围和时间筛选的真实绑定与清除命令保持不变。
- `DashboardViewModel.TaskHasActiveFilters` 与筛选属性通知保持一致；RenderHarness 的 Fake 数据同步该绑定。Production Shell 任务探针覆盖 1040×700、1100×720、1366×768，详情展开后分别保留 3、4 行完整任务记录，宽屏仍保持右侧 Inspector。
- 新增紧凑详情 STA 回归，验证表格最小高度、Inspector 有限高度以及展开/收起按钮可达；最新验证为 Release 构建 0 错误（保留 1 个 `NU1900` 网络审计警告）、Core `72/72`、Worker `300/301`（1 跳过）、Playnite `364/426`（62 跳过）、XAML `19/19`、源码校验、WPF 静态审查 `0 errors/20 warnings/172 info`、双主题/多尺寸/resize/Production Shell `render-qa OK`。真实 Playnite、DPI/高对比度、完整键盘和 60fps 仍需人工验收。

## 2026-09-07 质量计划状态已校正

- `QUALITY_REVIEW_2026-09-07.md` 的活动表和 Q4-01～Q4-03 正文已同步为已完成：媒体重试独立 IPC 与结果语义、媒体/存档目标标签导航、维护页紧凑详情显式展开及 Production Shell 离屏探针均已有实现和回归证据。
- 仍保留真实 Playnite、真实 Rclone/远端、用户数据、DPI/高对比度和完整键盘流程等人工验收边界；这些不是代码未完成项，也不应作为后续自动化开发计划重复实现。

## 2026-09-07 动效门控与即时终态收口已完成

- 关闭界面动画或系统动画时，`AcrylicProductionShellView` 会立即取消正在运行的侧栏宽度/透明度/位移动画，恢复到当前展开或收起终态；不需要等待下一次点击，也不会留下半透明或中间宽度。
- Dashboard 在主题/系统视觉设置刷新时同步清理已知页面过渡（入口、游戏筛选、详情页、状态胶囊、对话框和任务详情），同时继续把动画开关传递给生产 Shell 与 Overview 工作区。共享 Popup 动画仍按资源门控为 Fade/None。
- 新增真实 STA WPF `Window` 回归，关闭动画后点击侧栏立即得到 72 DIP 收起宽度、无运行中的过渡；动效门控定向测试 `7/7` 通过。
- 验证：Release 构建 0 错误（1 个因 nuget.org 漏洞服务不可达产生的 `NU1900` 警告）；全量 Core `72/72`、Worker `300/301`（1 跳过）、Playnite `363/425`（62 跳过）；WPF 静态审查 `0 errors/20 warnings/172 info`、源码校验、XAML `19/19`、RenderHarness `render-qa OK`、`git diff --check` 通过。已重新生成并校验 `artifacts/GameSaveCenter-0.6.73.pext`，未安装到 Playnite。真实 Playnite、DPI/高对比度、完整键盘和 60fps 流畅度仍需外部验收。

## 2026-09-07 Q4-00 媒体分页锚点行为收口已完成

- `MediaCenterView` 的延迟锚点恢复现在携带上下文代际；切换游戏、切换收件箱模式、切换 ViewModel、离开页面或发生新的选择/集合变化时，旧回调会统一失效并清理待恢复状态。收件箱模式还会校验回调携带的模式，避免旧请求覆盖当前窗口。
- `selectionRestoreQueued` 只在恢复成功、显示“当前位置不可恢复”提示或统一失效清理时释放，不会因一次延迟重试提前解锁；锚点过期计时器、待恢复选择和待恢复锚点由同一失效路径清理。
- 新增真实 STA WPF `Window` 行为回归：旧回调在上下文失效后不会显示过期提示；锚点被窗口裁掉时会显示提示并释放恢复锁。媒体锚点相关定向测试 `5/5` 通过，不再只依赖源码字符串断言。
- 验证：Release 构建 0 错误（保留 1 个因 nuget.org 漏洞服务不可达产生的 `NU1900` 警告）；全量 Core `72/72`、Worker `300/301`（1 跳过）、Playnite `362/424`（62 跳过）；源码校验、XAML `19/19`、`git diff --check` 通过。本阶段无 XAML/布局改动，因此未重复生成渲染截图。真实 Playnite、DPI/高对比度、完整键盘和大库连续滚动仍需外部验收。

## 2026-09-07 Q5-01 设置页操作反馈已完成

- 设置页头部现在始终保留一行可读状态：无错误时显示“已保存 · 由 Playnite 保存按钮提交”，修改后显示“有未保存更改 · 使用 Playnite 保存”，验证失败时显示“存在校验错误 · 保存前请修正”；状态带 Tooltip，未增加自定义保存按钮，Playnite 原有保存/取消边界不变。
- `GameSaveCenterSettings` 提供不包含 `DeviceId` 的稳定编辑指纹，并在 `EndEdit`/`CancelEdit` 后通知设置页重置基线；导入设置仍视为待提交修改，不会被 DataContext 重绑误判为已保存。设备身份不会因内部变化触发脏状态。
- 矮窗口不再隐藏保存状态；紧凑头部仍折叠长说明，页面滚动与分类导航、实际绑定和校验逻辑保持不变。RenderHarness 已验证浅色/深色及 1040×700、1100×720、1366×768 等尺寸，`render-qa OK`。
- 验证：Release 全量 Core `72/72`、Worker `300/301`（1 跳过）、Playnite `360/422`（62 跳过）；构建 0 错误、NuGet 漏洞审计保留既有 `NU1900` 网络警告；XAML `19/19`、源码校验、设置定向测试 `153/203`、`git diff --check` 通过。真实 Playnite、DPI/高对比度和完整键盘仍需外部验收。

## 2026-09-07 Q4-04 动态分页一致性已完成

- 云端队列与媒体归类历史新增持久化 `query_revisions` 修订表和 SQLite 触发器；覆盖队列/重试记录、游戏/媒体影响字段、归类批次和批次条目，已有数据库初始化时会自动补齐。
- 两套分页请求/响应增加不透明 `ConsistencyToken`。Worker 在查询前后校验修订号，翻页携带旧 token 或查询期间发生变化时返回 `PageResetRequired` 与中文原因，不把不一致结果标记为“已加载全部”。
- Playnite 翻页携带 token；收到 reset 后清空旧窗口、保留稳定 ID 语义、显示状态提示并自动从第一页重载一次，第二次仍变化时保留手动刷新入口。
- 新增云端队列、归类历史 stale-token 回归及旧库迁移断言。验证：Release 构建 0 错误；Core `72/72`、Worker `300/301`（1 跳过）、Playnite `358/420`（62 跳过）；XAML `19/19`、源码校验、WPF 静态检查 0 errors、`git diff --check` 通过。NuGet 漏洞审计因当前网络不可达出现既有 `NU1900` 警告；真实 Playnite、真实数据并发和人工键盘流程未运行。

## 2026-09-07 Q4-03 紧凑维护页详情布局已完成

- `MaintenanceView` 的诊断/进程映射页在 PageHost 宽度小于 980 DIP 时默认折叠选中详情，保留主列表的有限星号空间；新增明确的“查看详情 › / 收起详情 ›”操作，打开后详情仍由可滚动 Inspector 承载。选择变化会关闭旧详情，Esc 可收起，Tab/Shift+Tab 继续使用 WPF 默认键盘导航；宽屏维持并排 Inspector。
- 修正进程映射紧凑布局的第三行高度未应用问题，并把进程详情改为单一滚动容器；诊断与进程列表都按视觉树中实际可见行做审计，不再只看 `DataGrid.ActualHeight`。
- RenderHarness 新增真实 `AcrylicProductionShellView.PageHost` 维护页探针，覆盖 1040×700、1100×720、1366×768。紧凑 PageHost 715×577/775×597 时，诊断与进程列表默认分别可见 10/7、10/8 行；宽屏 PageHost 1041×645 保持并排详情。最终 `render-qa OK`，截图在本地 `.tmp/q4-03-render-final` 生成后已按规则清理。
- 验证：Release 构建 0 warning/0 error；新增维护紧凑详情 STA 回归与审计入口回归通过，XAML 19/19、源码校验通过，WPF 静态检查 0 errors/20 warnings/172 info；尚未运行真实 Playnite 宿主、真实 DPI/高对比度和完整键盘人工流程。

## 2026-09-07 Q4-01/Q4-02 媒体重试与目标标签导航已完成

- 媒体云端重试现在使用独立的 `media.cloud.upload.retry` IPC，Worker 接到 `MediaSyncService.RetryCloudUploadForUserAsync`；只重传已有媒体归档，不重新扫描来源、不复用备份专用 `cloud.upload.retry`。返回 `Submitted`、`PausedByPolicy`、`CannotSubmit` 及可选任务详情。
- 安全模式、全局云端关闭、Rclone 未配置、游戏策略未允许上传、后台失败/取消均不会在 UI 显示“已提交”；策略暂停会提示到存档策略设置。备份云端重试路径保持不变。
- `MediaTabIndex`/`SaveTabIndex` 通过 TabControl 双向绑定保留普通标签状态；首页待归类动作先选媒体“待归类”，诊断“进入存档路径确认”先选“路径与校验”，不模拟点击或延时跳转。
- 验证：Release 构建 0 warning/0 error；媒体重试 Worker 定向 10/10、媒体锚点/重试契约 Playnite 定向 4/4。尚未运行真实 Playnite、真实 Rclone 远端和用户数据上传。

## 2026-09-07 质量复核收尾补充

- UI3-07 已在 `9e93909` 提交；本轮补审锚点逻辑并独立运行 MediaPageAccumulator/MediaWindowAnchorContract 定向测试 **5/5**。已更新 [Q4-00～04 收口计划](QUALITY_REVIEW_2026-09-07.md)，后续重点是实际滚动行为、操作语义和可见布局。
- 完整构建/渲染证据仍属于 `b0aa85a`。UI3-07 的源码字符串契约测试不能代替 WPF 滚动/延迟回调验收；先补该项，随后落实 Q4-01～04，再进行设置和动效优化。

## 2026-09-07 UI3-07 缓存窗口翻页与滚动锚点已完成

- `MediaCenterView` 在当前游戏媒体和媒体收件箱点击“加载更多”前捕获可见首项、滚动偏移和当前多选 ID；集合 Reset 后按 `VirtualizingWrapPanel`/DataGrid 的滚动模型恢复位置与可见选择，编辑中的媒体对象仍由 ViewModel 保持。
- 三个缓存窗口继续使用既有 2000 项上限。窗口裁剪掉锚点时不伪造位置恢复：页面显示“列表窗口已前移，当前位置不可恢复”和“返回最新”，分别通过 `ReloadMediaWindowCommand`/`ReloadMediaInboxCommand` 重新载入较新的首批内容。
- 多选语义明确为“仅当前保留窗口参与批量操作”；跨模式分别保存选中 ID，窗口外 ID 不会被批量命令静默覆盖。RenderHarness Fake 已补齐新增命令和加载统计绑定。
- 验证：Release 构建 0 warning/0 error；Core `72/72`、Worker `296/297`（1 跳过）、Playnite `355/417`（62 跳过）；XAML `19/19`、源码校验、WPF 静态审查 `0 error`、双主题/多尺寸/resize `render-qa OK`、`git diff --check` 均通过。该目录的证据实际来自 b0aa85a 冻结审计，不作为 UI3-07 行为验证证据。未运行真实 Playnite 宿主；真实大媒体库连续滚动、DPI、高对比度和用户数据仍需人工复核。

## 2026-09-07 UI3 完成质量复核（文档交付）

- 新增 [完成质量复核与后续计划](QUALITY_REVIEW_2026-09-07.md)，冻结基线 `b0aa85a`。UI3-00～06 主体完成；审阅时 UI3-07 仍有并发未提交改动，不纳入验收结果。
- 隔离源码 Release：0 warning/error；Core 72 通过，Worker 296 通过/1 跳过，Playnite 352 通过/62 跳过；XAML、源码门禁、静态 UI 检查、RenderHarness 通过。未验证真实 Playnite。
- 下一步先验收 UI3-07，再处理媒体上传重试语义、目标标签导航、紧凑维护列表空间和动态分页一致性；之后优化任务密度、设置首屏与动效。离屏证据位于 `docs/design/reviews/2026-09-07-quality/`，其中独立页面画布与生产 Shell 几何应明确区分。

## 2026-09-07 UI3-06 存档与维护详情可读性已完成

- 存档历史表将时间收敛为 `MM-dd HH:mm`，完整 `yyyy-MM-dd HH:mm:ss` 仍在单元格和 Inspector Tooltip；窄宽度下给“类型/状态”保留语义宽度，备注继续弹性占用。Inspector 把恢复可用性、隔离校验风险和验证入口置于备注编辑之前，恢复仍沿用预检、确认和 PreRestore 保护流程。
- 维护诊断主表只保留“等级/游戏/问题”三列，详情和建议处理在可滚动 Inspector 展开；新增按诊断身份解析的受控入口，可进入对应游戏的存档路径确认、失败任务筛选或云端队列，不执行自动修复/重试/覆盖。无身份或未知类型不显示误导性导航。
- 云端摘要区分“校验失败”“上传失败”和混合失败；未知云端状态、媒体归类批次状态和批次条目状态统一回退为中文“未知状态”；移除设备页固定“2 台设备”文案，避免显示与真实数据不符的数量。
- 验证：Release 全量 Core `72/72`、Worker `296/297`（1 跳过）、Playnite `352/414`（62 跳过）；XAML `19/19`、源码校验、WPF 静态审查 `0 error/20 warnings/172 info`、双主题/多尺寸/resize `render-qa OK`、`git diff --check` 均通过。未运行真实 Playnite 宿主，真实宿主/DPI/高对比度仍需人工复核；下一阶段进入 UI3-07 缓存窗口与滚动锚点。

## 2026-09-07 UI3-05 首页优先级与下一步动作已完成

- 首页 Hero 不再使用固定“存在需要处理的项目”，由 `OverviewPriorityResolver` 按 Worker、首次环境准备、云端异常队列、待归类媒体、游戏关注项、健康刷新顺序选择一个最高优先级状态；标题、计数说明、按钮文案和命令都来自真实快照/设置。
- 新增 `OpenMediaWorkspaceCommand`，待归类媒体可从 Hero 直接进入媒体工作区；云端优先级进入已有队列明细，Worker/首次准备进入维护中心，关注项保持风险卡中的一个上下文入口，健康状态使用刷新概览，不再把正常状态渲染成警告大标题。
- 首页工具栏和当前游戏卡移除重复关注入口；工具栏文案明确全局批量操作与当前游戏操作边界。RenderHarness Fake 同步真实云端摘要，新增优先级解析与首页绑定回归。
- 验证：Release 全量 Core `65/65`、Worker `296/297`（1 跳过）、Playnite `348/410`（62 跳过）；构建无 warning/error，XAML `19/19`、源码校验、WPF 静态审查和双主题/多尺寸/resize `render-qa OK` 均通过。未运行真实 Playnite 宿主，真实宿主/DPI/高对比度仍需人工复核。

## 2026-09-07 UI3-03 媒体归类批次历史与可找回撤销已完成

- 媒体归类批次历史现在由 Worker 从 SQLite 聚合读取，提供状态筛选、分页、总量/已加载量和每批次的已应用/冲突/已撤销计数；重启后不依赖客户端内存恢复上下文。
- 预览批次不再占用“上次可撤销批次”语义；只有应用完成后才成为可撤销批次。历史列表选择可撤销批次，撤销后刷新存储状态；`UndoneWithConflicts`/`AppliedWithConflicts` 等状态和冲突计数保留，不覆盖后续用户修改。
- 媒体收件箱检查器新增批次历史有限视口、状态筛选、刷新、加载更多和“撤销所选可回退批次”；请求带代际/取消，离开媒体页不会让旧历史页回写当前界面。RenderHarness 已加入预览、已应用冲突、已撤销冲突和待确认夹具。
- 验证：历史持久化/分页/状态筛选回归已通过；Release 构建、全量测试、XAML 19/19、源码校验、WPF 静态审查和 `render-qa OK` 均通过。未运行真实 Playnite 宿主，真实用户媒体目录、DPI/高对比度和人工跨重启验收仍需后续复核。

## 2026-09-07 UI3-04 任务中心紧凑筛选与主表密度已完成

- 紧凑任务页保留搜索、状态和刷新在同一行；类型、游戏、任务范围和时间范围移动到同一组可展开筛选中，使用原有控件实例和真实 Binding，不复制一套会失效的筛选状态。收起时显示当前生效条件并提供清除入口，宽屏调整会把控件恢复到桌面工具栏。
- 任务主表时间改为 `MM-dd HH:mm`，完整时间保留 Tooltip；时间/状态/进度列设置语义最小宽度，详情列不再挤压关键状态。堆叠模式使用专用 36 DIP 行样式，宽屏仍使用共享 44 DIP 行样式；紧凑详情按钮和虚拟化/分页命令保持可达。
- `FakeDashboardData` 补齐任务统计、历史范围、分页状态和任务操作命令，避免离屏审计因夹具缺属性产生空控件；新增响应式回归覆盖窄宽度重排与宽度恢复。
- 验证：Release 全量测试 Core `65/65`、Worker `296/297`（1 跳过）、Playnite `343/405`（62 跳过）；构建 0 warning/0 error，XAML `19/19`、源码校验、WPF 静态审查和 `render-qa OK` 均通过。最终紧凑截图显示三行完整任务记录，宽屏首屏五行；未运行真实 Playnite 宿主，仍需人工复核宿主/DPI/高对比度。
- 下一阶段进入 UI3-05 首页层级和下一步动作收口；不要把离屏夹具结果当作真实 Playnite 宿主验收。

## 2026-09-07 UI3-02 云端队列明细分页与用户操作入口已完成

- `MaintenanceView` 新增“云端队列”页，概览卡可直接进入；页面使用 Worker 已有的 `cloud.transfer.status` 分页契约，提供状态/类型筛选、100 条一页的“加载更多”、总量独立摘要和按 `TransferKey` 保留选择的详情面板。
- 详情明确区分“已上传”和“远端已校验”，展示错误码、错误详情、下次尝试和保证级别；“执行远端 check”是只读比对，不上传、不覆盖本地内容。备份重试走 `RetryCloudUpload`，媒体重试走媒体同步并上传，认证失败不会被转成自动上传重试。
- 维护页在窄宽度下保留有限虚拟化表格，详情折叠为可访问的“查看详情”抽屉；切换筛选、刷新和离开维护页会取消过期请求，防止旧页覆盖新筛选结果。
- 验证：Release RenderHarness `render-qa OK`，XAML `19/19`，源码校验通过；Core `65/65`、Worker `295/296`（1 跳过）、Playnite `342/404`（62 跳过）。WPF 静态审查 0 error，保留既有 warning/info。未运行真实 Playnite 宿主、真实云端凭据或目标机 DPI/高对比度验收。

## 2026-09-07 UI3-00/01 媒体收件箱可见性与紧凑布局已完成

- `MediaCenterView` 的“待归类”页现在使用共享页面滚动样式承载有限的 `MediaInboxGrid` 视口；批量主操作、目标游戏和查看详情保持一行，忽略/预览/应用/恢复/撤销等次级动作移到表格后的页内滚动区。窄宽度下详情面板默认折叠为可访问的“查看预览与归类”按钮，真实命令、Binding、选择和虚拟化未改。
- `ApplyResponsiveLayout` 使用实际页面可用高度计算表格高度；`RenderHarness` 不再把外层窗口高度冒充页面高度，并新增 1040×700、1100×720、1366×768 生产壳层 `PageHost` 几何夹具。布局审计新增祖先裁剪交集、首屏表格高度和动作控件命中区检查，当前媒体页无 HIGH/Fidelity/失败路由。
- 证据保存在 [`docs/design/reviews/2026-09-07-ui3/`](../design/reviews/2026-09-07-ui3/)，包含同一 1040×700 夹具的改造前后截图。Release RenderHarness 构建 0 warning/0 error，`render-qa OK`；Playnite 测试 `342/404` 通过、62 跳过，`validate-source.py` 通过。WPF 静态审查仍有既有 warning/info，未新增 error。
- 以上是离屏/WPF 自动证据，不等同实际 Playnite 宿主渲染；真实宿主、DPI、高对比度和用户大媒体库仍需人工复核。下一阶段继续处理 UI3-03 的可找回归类批次。

## 2026-09-06 修复 FLiNG 后台下载 403

- FLiNG 详情页可访问而 `/downloads/` 文件链接对非浏览器抓取返回 403；Worker 原先只发送自定义 User-Agent，未复用详情页 Cookie，也未携带下载来源页 Referer。
- `FlingTrainerCatalogSource` 现在使用 CookieContainer、浏览器风格请求头，并在下载前读取对应目录项、预热官方详情页会话，再以该详情页作为 Referer 发起下载；最终重定向仍必须通过 FLiNG HTTPS 域名白名单。
- 403 现在落为稳定的 `FLING_DOWNLOAD_FORBIDDEN` 业务错误；`GameToolService` 将下载临时文件清理范围扩展到下载/解压/绑定全流程，失败不会留下 `.download` 残留。
- 新增 Worker 回归验证详情页预热、Referer、浏览器请求头和文件落盘。Release 全量结果为构建 0 warning/0 error、Core `65/65`、Worker `295/296`（1 跳过）、Playnite `341/403`（62 跳过）、XAML `19/19`；源码校验、XAML 检查和 `git diff --check` 通过。
- 本机沙箱无法对 FLiNG 进行真实 Worker 在线下载；若站点后续启用 JavaScript/验证码挑战，仍需人工确认或改用浏览器下载，不会绕过安全验证。

## 2026-09-06 V2 完成后 UI 复查（仅文档，UI3 尚未实施）

- 当前审阅基线 `0ac0e39`；入口为 [UI_REVIEW_V3_2026-09-06.md](UI_REVIEW_V3_2026-09-06.md)。V2 主体有对应代码/测试，抽查未确认阻断 UI 工作的新 P0；不代表完整无缺陷验收。
- 下一阶段优先 UI3-00/01 修渲染夹具与媒体紧凑布局，UI3-02/03 接入云队列明细与可找回归类批次，再改善任务密度、首页层级、存档/维护文本和翻页锚点。当前已有后端分页不等于生产页已提供明细翻页入口。
- 本轮离屏 `render-qa OK`，但媒体 1040×700 外层夹具（744×460 工作区）存在按钮横向溢出、表格可见高度不足；任务夹具缺 TaskTotalCount/历史范围等新属性，空显示不能直接认定为生产绑定故障。截图/报告保留在 `docs/design/reviews/2026-09-06-v3/`；不是 Playnite 真机证据。
- Release 0 warning/0 error，Core 65/65、Worker 294 通过/1 跳过、Playnite 341 通过/62 跳过、XAML 19/19、源码校验通过。本轮只改文档，没有改应用版本或安装插件；真实宿主仍为 MANUAL QA REQUIRED。

## 2026-09-06 V2-07 媒体多页累积成本与有界窗口已完成

- 媒体主列表和“未归类/已忽略”收件箱首屏使用首页替换，后续游标页按媒体 ID 增量更新/追加，不再每页遍历并重建整个 `ObservableCollection`；同 ID 更新不会产生重复项。
- 三个视图缓存各自维护 2000 条有界窗口；继续加载仍沿用服务端游标和总数语义，当前选中媒体在窗口裁剪时保留，页头明确显示“当前保留/总数/窗口上限”，未归类和已忽略缓存互不污染。
- `BatchObservableCollection` 为每个变化页合并为一次 Reset 通知；新增 250 页、5 万条输入的回归验证窗口保持 2000 条，且覆盖重叠 ID 更新和选中项保留。
- 隔离 Release 全量结果：构建 0 warning/0 error，Core `65/65`、Worker `294/295`（1 跳过）、Playnite `341/403`（62 跳过）、XAML `19/19`；源码校验、XAML 检查与 `git diff --check` 通过。
- 真实 Playnite 大库首屏、滚动/回收帧率、DPI 和目标机长时性能仍为 `MANUAL QA REQUIRED`；本阶段没有写入用户媒体或宿主数据。

## 2026-09-06 V2-06 云队列全量摘要与独立分页已完成

- 云端状态查询先在 SQLite CTE 中合并新 `cloud_transfer_queue`、旧 `cloud_retry_queue` 和游戏/媒体基础状态，按稳定键去重后再计算完整总数、各状态计数和最早 `RetryScheduled` 时间；新队列优先于旧表，旧表不会和新表重复计数。
- 明细改为独立的 `Page/PageSize` 查询，默认每页 100 项、服务端最多 100 项；支持按状态和 `Backup/Media` 类型过滤，并返回 `LoadedCount/HasMore`，分页窗口不会影响摘要统计。
- 保留既有稳定优先级排序：认证、校验失败、上传失败、待重试、运行中、待上传和暂停依次展示，重试项按最早下次时间优先；老重试记录继续映射为可操作的认证/重试状态。
- 新增超过 1000 条、旧新队列同键、最早重试、末页和失败状态过滤回归。隔离 Release 全量结果为 Core `65/65`、Worker `294/295`（1 跳过）、Playnite `339/401`（62 跳过）、XAML `19/19`，源码校验与 `git diff --check` 通过。
- 真实 Playnite、大库 UI 首屏和跨进程并发仍为 `MANUAL QA REQUIRED`；本阶段仅使用临时 SQLite 夹具。

## 2026-09-06 V2-05 云端校验终态与代际保护已完成

- 云端队列为上传和只读远端校验分别保存 `Upload`/`Verify` 操作类型与操作代际；校验进入闸门等待前先持久化 `Verifying`，取消、进程异常和工具失败都会恢复原有上传状态或落为明确的校验失败，不再把校验误留成 `Transferring`。
- 校验完成使用操作 ID CAS 收尾；较新的上传代际一旦接管队列，迟到的校验结果只返回 `CLOUD_CHECK_SUPERSEDED`，不会覆盖新状态，也不会触发上传或删除任何本地/远端内容。
- Worker 启动恢复仅重新排队上传型 `Pending/Transferring`；纯校验型 `Verifying` 恢复到校验前快照或 `CheckFailed`，不因重启自动发起上传。基础游戏云端投影采用尽力写入，不会反向污染已落盘的队列终态。
- 新增取消、工具异常、check 失败、较新上传代际、Worker 重启和旧库迁移回归。隔离 Release 全量结果为 Core `65/65`、Worker `293/294`（1 跳过）、Playnite `339/401`（62 跳过）、XAML `19/19`，源码校验与 `git diff --check` 通过。
- 真实 Playnite、真实云端凭据/断网、硬杀窗口和跨进程并发仍为 `MANUAL QA REQUIRED`；本阶段没有自动上传、删除或写入用户云端数据。

## 2026-09-06 V2-01 媒体归类提交与恢复协调已完成

- 媒体归类应用/撤销现在先在 `media_classification_operations` 持久化文件意图，再移动归档副本；媒体行、批次条目和操作账本在同一个 SQLite 事务中提交，避免“数据库已改、批次未改”窗口。
- Worker 启动在常规任务恢复前协调 `Planned`/`Moved`/`RecoveryRequired` 操作：能确认已提交的标记为 `Committed`，未提交且文件完整的恢复为原布局并标记 `Aborted`，状态无法唯一对应则保留 `RecoveryRequired`，不静默覆盖。
- 审计写入位于业务提交之后且使用独立尽力路径；审计失败不会回滚已经提交的媒体归类。取消和补偿使用 `CancellationToken.None`，归档源来自原始文件时只删除新建副本，不触碰用户原始媒体。
- V2-01 行为测试覆盖批次 SQLite 更新失败后的启动恢复、审计写入失败不回滚、取消后的独立令牌补偿，以及既有正常应用/撤销/冲突路径。Release 构建 0 warning/0 error；Core `65/65`、Worker `278/279`（1 跳过）、Playnite `339/401`（62 跳过）、XAML `19/19`，源码校验与 `git diff --check` 通过。
- 真实 Playnite、用户媒体目录、断电/硬杀时序和跨进程并发仍是 `MANUAL QA REQUIRED`；本阶段只使用临时隔离目录与 SQLite 故障触发器，不写入用户数据。

## 2026-09-06 V2-02 健康巡检调度与计划写入已完成

- `HealthInspectionService` 的后台循环在手动巡检持有执行锁时使用 250ms 有界退避，不再无等待地读取 SQLite/争抢锁；外围存储或调度异常使用 1s 退避并继续运行，停止令牌仍可及时退出。
- 巡检计划字段与执行字段分开写入：`UpdateHealthInspectionPlanAsync` 只更新启用/间隔/过期/预算/下次时间，`SaveHealthInspectionExecutionStateAsync` 只更新运行结果和游标；完成时读取最新计划后计算下次时间，避免旧整行状态覆盖设置变更。
- 新增可控闸门回归验证手动巡检持锁 700ms 内后台争锁次数有上限，以及执行状态写入不会覆盖并发计划修改。隔离 Release 全量结果为 Core `65/65`、Worker `280/281`（1 跳过）、Playnite `339/401`（62 跳过）、XAML `19/19`，源码校验与 `git diff --check` 通过。
- 真实宿主和长时调度压力仍为 `MANUAL QA REQUIRED`。

## 2026-09-06 V2-03 健康巡检候选公平与恢复游标已完成

- 巡检候选按 `PlayniteId`、`CreatedUtc`、`BackupId` 的同一稳定顺序轮转，游标使用完整的游戏/备份复合身份；持久化的 in-flight 候选在重启后优先恢复，即使它后来已变新鲜也不静默跳过。
- 选定候选后，在检查游戏会话、申请操作锁和读取归档前先落盘身份；候选状态写入失败时不会开始归档校验，也不会把基础设施故障污染成归档失败或健康 finding。
- 游戏运行或同游戏操作繁忙会为具体候选保存下次尝试时间，本轮继续寻找其他可检查候选；全量新鲜时返回 `UpToDate`，不重复读取归档，并补齐“校验仍在有效期内”状态文案。
- 新增 5 项健康巡检行为回归，定向测试 `11/11`；隔离 Release 全量结果为 Core `65/65`、Worker `285/286`（1 跳过）、Playnite `339/401`（62 跳过）、XAML `19/19`，源码校验与 `git diff --check` 通过。
- 真实 Playnite、硬杀/断电窗口、跨进程并发和长时调度压力仍为 `MANUAL QA REQUIRED`；V2-05～V2-07 尚未处理。

## 2026-09-06 V2-04 IPC 请求身份与重放指纹已完成

- IPC 请求账本新增协议版本和规范化 JSON 负载 SHA-256 指纹；同一 `RequestId` 只有在类型、协议版本和负载语义一致时才允许重放，JSON 属性顺序变化不会制造冲突。
- 同一 `RequestId` 携带不同类型、协议或负载会稳定返回 `REQUEST_ID_CONFLICT`，不执行也不重放旧响应；受保护写请求缺少 `RequestId` 返回 `REQUEST_ID_REQUIRED`。
- 旧账本缺少负载指纹时按不可安全重放处理；启动迁移补列，完成项按 7 天有界清理，中断证据保留 30 天；Named Pipe 服务额外以低频后台维护清理账本，不触碰当前 Worker 的 in-flight 行。
- 新增账本冲突、属性顺序、旧记录、迁移和保留策略回归；隔离 Release 全量结果为 Core `65/65`、Worker `288/289`（1 跳过）、Playnite `339/401`（62 跳过）、XAML `19/19`，源码校验与 `git diff --check` 通过。
- 真实 Playnite、跨版本旧 Worker/插件组合和硬杀后的端到端重放仍为 `MANUAL QA REQUIRED`；V2-05～V2-07 尚未处理。

## 2026-09-06 复查后待办（V2-01～V2-07 已完成，余项尚未修复）

- 最新审阅为 [FOLLOWUP_REVIEW_2026-09-06.md](FOLLOWUP_REVIEW_2026-09-06.md)，基线 `8018cee`；上一轮功能已有实现，本轮只更新文档，新增 V2-01～07 和 X2-01～03，不重新执行旧路线图。
- V2-01～V2-07 已补齐对应的服务级提交、调度、候选恢复、IPC 身份、云端校验代际、完整摘要和媒体多页增量累积行为；X2-01/X2-02 与 X2-03 的构建身份底座已落地，X2-03 的实际发布安装核验仍需发布窗口。
- 覆盖旧 F01/F03 交接中的保证：已实现存储账本不等于所有崩溃窗口闭环，自动测试也不能替代断电或真实宿主证据。
- 本阶段 Release 无警告/错误，Core 65/65，Worker 294 通过/1 跳过，Playnite 341 通过/62 跳过，XAML 19/19；Worker 硬重启及部分 Named Pipe 行为测试按既有环境规则跳过，真实宿主仍为 MANUAL QA REQUIRED。

## 版本与生产入口

- 当前分支：`main`；当前版本：`0.6.73`，版本来源为 `Directory.Build.props`、`src/GameSaveCenter.Playnite/extension.yaml` 和生产侧栏版本文本。
- 生产可见宿主是 `src/GameSaveCenter.Playnite/Views/DashboardView.xaml` 中承载的 `AcrylicProductionShellView`；页面工作区位于 `Views/OverviewView.xaml`、`SaveCenterView.xaml`、`TrainerCenterView.xaml`、`TaskCenterView.xaml`、`MediaCenterView.xaml` 和 `MaintenanceView.xaml`。`DashboardView` 的兼容壳仍保留真实 Binding/命令，不应把兼容路径误当成第二套业务实现。
- Worker 入口为 `src/GameSaveCenter.Worker/Program.cs`；Playnite 与 Worker 的契约集中在 `src/GameSaveCenter.Contracts`，请求分派入口为 `src/GameSaveCenter.Worker/Ipc/IpcRequestDispatcher.cs`，持久化入口为 `Persistence/SqliteStateStore*.cs`。

## 当前视觉来源与有效例外

- 页面迁移遵循 Demo-first 规则；计划引用的 `GameSaveCenter.AcrylicFork/src/GameSaveCenter.Playnite/Design/` 在当前工作区不存在，不能作为本机测试输入，也不能声称完成与该目录的逐像素比对。当前可追溯的生产资源入口是 `src/GameSaveCenter.Playnite/Themes/AcrylicProductionResources.xaml`，其核心资源为 `DesignTokens.xaml`、`WpfUiProduction.xaml` 和 `Redesign.xaml`，并按兼容需要合并 AcrylicReference 资源。
- Demo 的 Mock 数据、演示行为和窗口按钮不进入生产。当前游戏选择器、项目现有滚动条系统、真实运行时数据以及目标文件明确要求的安全/确认语义是有效例外；页面可以重构信息架构，但必须保留命令、Binding、数据契约、错误/取消语义、虚拟化、键盘/UI Automation 和 Playnite 兼容性。
- 游戏筛选 ComboBox 使用 OneWay 显示绑定、`UiFilterSelection.Synchronize` 恢复共享状态，并以 `DropDownClosed` 作为用户写回入口；不要恢复静态 `SelectedIndex` 与双向写回竞态。工作区列表应保留有限视口、内部滚动和 Recycling 虚拟化。
- 共享生产 TextBox 的 `PART_ContentHost` 必须保持零 Margin/零 Padding/零 BorderThickness，外层 Chrome 不得重复应用 `TextBox.Padding`；任务中心搜索框保持 `30,7,38,7` 和 `GscButtonHeight=36`，输入可视区域由原生内容视口承载。

## 当前已完成的功能阶段

- R01～R07：危险清理隔离/账本、任务统计历史、游戏筛选行为、媒体分页、IPC 取消/重放。
- U01～U03：任务状态视口、侧栏动画终态、游戏目录来源诊断。
- F01～F03：备份健康巡检与隔离恢复、云端队列与传输策略、媒体归类建议与可撤销批次；E02：当前事实入口与交接边界治理。
- E01 的自动证据已分层：`scripts/e01-behavior-matrix.ps1` 可在隔离 `.tmp` 输出中分开记录业务、IPC、WPF/STA、故障/Soak 和可选 RenderHarness 结果；`scripts/e01-scale-baseline.ps1` 提供 full/stress 两档隔离 Worker/SQLite 规模基线，记录查询首查/热查、搜索、分页、分配量和资源残留，并生成结构化 `worker-scale.json`；矩阵已包含 `WorkerProcessRestartTests` 的独立 Worker 硬中断/重启用例，真实宿主项仍不会被脚本伪造为通过。
- F03 的媒体归类只基于来源规则、会话、进程映射和文件名等本地证据；低置信度不自动归类，批次快照冲突不覆盖用户修改，原始媒体和真实存档不删除。应用/撤销只移动可恢复的归档副本。

## 验证边界

- 自动基线：V2-07 Release 构建 0 warning/0 error；Core `65/65`、Worker `294/295`（1 跳过）、Playnite `341/403`（62 跳过）、XAML `19/19`；E01 历史分组矩阵业务 `44/44`、IPC `22/22`、WPF/STA `45/45`、故障/Soak `4/4` 全部退出码为 0；规模基线 full 为 `2000/20000/10000/5000/500`（游戏/备份/任务/媒体/工具），stress 为 `10000/20000/10000/50000/500`，两档均通过资源增长与残留断言；`scripts/validate-source.py` 和 `git diff --check` 通过。
- 离屏 RenderHarness、静态源码检查和沙箱测试不等同真实 Playnite 宿主证据。已对当前用户 Worker 完成一次只读 `system.ping` 的真实 Named Pipe 连通性验证；E01 还在随机管道、独立 Mutex 和临时 SQLite 中完成了真实 Worker 硬中断后重启恢复验证。真实 Playnite 逐页像素、主题/高对比度、DPI、键盘焦点、媒体大库、真实云端凭据/断网和长时多进程并发仍标记为 `MANUAL QA REQUIRED`。
- 用户可操作的验收应使用隔离 Playnite 安装、独立数据目录和明确进程边界；在这些条件未提供前，继续做安全的源码/Worker/离屏验证，但不要安装插件、写入用户数据或伪造宿主通过结论。

## 新任务启动顺序

1. 先读本文，再读 `PROJECT_MEMORY.md`、`WORKLOG.md`、`DEVELOPMENT_HANDOFF.md` 和任务相关设计门禁。
2. 先确认代码事实与本文一致；若历史条目冲突，在本文补充当前覆盖关系，不删除历史证据。
3. 每个独立阶段只改一个功能边界，补行为测试，运行 Release/门禁/渲染验证，同步三份交接文档后用中文提交并推送。

## 2026-09-15 Round2 Q24-03 物理跨屏前置采集

- 提交 `70935fe` 首次加入 Q24-03 拓扑采集，`3851228` 修正最终 JSON 深度写入，当前代码基线为 `3851228`：`scripts/real-host-audit.ps1` 在启动前采集 `System.Windows.Forms.Screen.AllScreens`，将 `DisplayCount`、边界/工作区和 `Q24_03PhysicalCrossScreen.Status` 写入 `runner-metadata.json`；单屏状态明确为 `blocked-single-display`，双屏才进入宿主回放前置。
- `DashboardView` 的游戏选框补充了宿主内浮层契约：`GameBrowserPanel`/`GameBrowserScrim` 不创建独立 Window 或 WPF Popup；共享 ComboBox Popup 继续使用 Bottom 定位、StaysOpen=False、动态主题资源和有限内部滚动。新增 `DiagnosticsEvidenceSourceTests` 与 `UiFinesseRound2ControlSourceTests` 源码契约。
- 当前机器前置枚举只有 `DISPLAY1`（2560×1440，工作区 2560×1368），因此真实跨屏迁移和打开态 Popup 仍外部阻塞；证据见 `docs/design/reviews/ui-finesse-round2-20260913/evidence/q13-q25/Q24-03-PHYSICAL-CROSS-SCREEN-20260915.md`。未将源码门禁或离屏结果写成物理跨屏通过。
- `python scripts/validate-source.py`、PowerShell 脚本语法解析和 `git diff --check` 通过；定向 `dotnet test` 在当前 SDK/工程解析阶段长时间无输出，未得到测试结果。
- 随后使用 `dotnet test GameSaveCenter.sln --no-restore -c Release -m:1 --logger "console;verbosity=minimal"` 在 `d8fad48` 通过 Release 全量测试：Core `83/83`、Worker `310/311`（1 skip）、Playnite `482/545`（63 skip），失败 0；定向 Q24-03 源码测试为 `16/16`，替换此前未收敛的并发节点尝试。

## 2026-09-15 Round2 Q06-05 选中悬停状态优先级

- `AcrylicProductionResources.xaml` 的生产 `AcrylicNavItem` 以前让普通 `IsMouseOver` 覆盖 `IsChecked` 强选中背景；提交 `513ac5f` 增加最后执行的 `MultiTrigger(IsChecked=True, IsMouseOver=True)`，恢复强背景、强边框和选中前景。
- 新增 `SelectedAcrylicNavigationKeepsItsStrongStateWhenHovered` 源码契约；定向 `UiFinesseRound2ControlSourceTests` 为 `20/20`，`check-xaml.ps1` 为 `24/24`，`python scripts/validate-source.py` 通过。
- clean-tree RenderHarness 使用提交 `513ac5f2528e0706fde194d977f03fd8bd798e5d`，浅/深主题全量报告 `render-qa OK`、0 warning/0 error、`WorkingTreeClean=True`；证据见 `evidence/q04-q12/nav-priority-20260915.md`。
- Q06-05 账本仍保留真实宿主输入序列待验：当前修复解决共享资源优先级，不把离屏状态或本轮 Playnite 导航观察冒充按压/失焦/禁用序列的最终像素验收。

## 2026-09-15 Round2 WPF 测试调度隔离

- 当前工作树新增 `tests/GameSaveCenter.Playnite.Tests/AssemblyInfo.cs`，关闭 Playnite 测试程序集并行化；原因是多个真实 STA WPF Window/Dispatcher 测试在完整套件中偶发争用，单独重跑同一 Release 二进制均可通过。
- 修改后 `scripts/build.ps1 -Configuration Release` 通过：XAML `24/24`、Release `0 warning/0 error`、Core `83/83`、Worker `310/311`（1 skip）、Playnite `495/558`（63 skip），失败 `0`。
- 该变更只收紧测试运行隔离，不改变生产 UI 行为；下一步宿主证据必须使用包含该测试门禁修复的提交身份重新建立。

## 2026-09-15 Round2 Q00 审计门禁假阳性修复复核

- `f1ea52a` 修正真实宿主 `HighGateCount` 统计：`overflow-classification.json` 仅是分类诊断，不再算阻断门禁；当前 Release truthfulness 定向回归 `1/1`，已有隔离产物按新规则为 `BlockingGateFiles=0`，但旧摘要原文仍是 `HighGateCount=1`。
- `cc63523` 自动门禁与打包安装通过：XAML `24/24`、Core `83/83`、Worker `310/311`（1 skip）、Playnite `495/558`（63 skip），失败 `0`。
- `cc63523` 的早期隔离 Playnite 曾在 CEF `mojo platform_channel` 访问拒绝后退出；该失败边界已由 `37f92f7` 的成功隔离重跑补充。当前成功产物绑定完整 SHA、`HighGateCount=0`，但 Q24-03 仍受单屏限制，用户 Worker PID `23304` 未终止。

## 2026-09-18 Round3 R08-07 当前事实

- 当前续作分支 `codex/ui-finesse-round2` 已推送 `afa845a6`。R08-07 的 `DialogLifecycleStateMachine`、`DialogOverlayMotion` 与行为测试已提交；main 工作树的用户改动未合并、未覆盖。
- 受控验证：隔离 D: 源目录 Release 构建 XAML `24/24`、`0 warning / 0 error`；R08 按类 `16/16`，Core `83/83`，Worker `310/311`（1 skip），源码校验和 diff check 通过。C: worktree 直接 WPF 构建因 `_wpftmp.csproj` `Access denied` 未通过，不能把隔离路径结果改写成 C: 路径成功。
- 未验边界：真实 Playnite 呈现、物理 DPI/跨屏、真实键盘/IME、UIA/读屏、presented frame、ETW 和宿主性能仍未验证；Demo 原目录仍不可用，当前以恢复生产资源基线为视觉依据。
- 下一可执行任务：R08-08 变换所有权；先检查 `GscMotion` 的共享可变/冻结 `Freezable`、其他变换实例和 R00-02 已有证据，再做最小行为/负例补充。

## 2026-09-19 Round3 R00/R01 当前提交复核

- `3354fd82` 修正了 R00/R01 仍指向旧对话框动画、旧忙碌触发器和旧焦点保护的源码断言；同时让 RenderHarness 与 UiAuditRunner 从 `GscSourceRoot/GscBuildCommit` 元数据解析隔离源码身份，避免 `.tmp` 输出目录向上误认 main。
- 干净提交验证：XAML `24/24`、Release `0 warning / 0 error`；受影响源码/行为测试均通过，浅/深合成 finesse、motion hot/re-entry、media geometry、审计和 evidence index 均通过；审计 `161` snapshots、`0` Fidelity、`0` failed routes、无 HIGH/MEDIUM，EVIDENCE_INDEX `20/20`。
- freshness 当前为 `14/14 fresh`，负例测试通过；包身份保持 `not-provided`。R00-07、R01-01、R01-07 原本已 fresh，未伪造为同一批构建产物。
- main 工作树仍保留用户未提交文件，续作分支只提交自身实现和本阶段文档；真实 Playnite、物理屏幕/DPI/跨屏、IME/UIA、presented frame、ETW、宿主性能和 package-host 仍未验证。
- 下一可执行任务：R08-08，检查 `GscMotion` 的共享可变/冻结 `Freezable` 和变换实例归属，再做最小行为/负例补充。

## 2026-09-19 Round3 R08-08 变换所有权

- `194a16fe` 已推送。`GscMotion` 对外部/样式 RenderTransform 在 motion helper 首次使用时按当前值克隆并登记到控件状态；同一控件复用自有树，两个控件共享未冻结直接变换或组合变换时不会互相移动，原有旋转/平移几何保留。
- 新增真实 STA 行为测试覆盖直接 `TranslateTransform` 与带旋转子节点的 `TransformGroup`；R00-02 1000 次组合缩放稳定性仍通过。当前提交隔离 Release/XAML `24/24`、解决方案 `0/0`，Foundation `9/9`，R08 相关串行回归通过。
- 证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R08-08-TRANSFORM-OWNERSHIP-20260919.md`。未把 synthetic/offscreen logical DIP 写成真实 Playnite、物理屏幕、presented frame、UIA/读屏、ETW 或宿主性能通过。
- Demo 原目录不可用，沿用恢复生产基线；命令/绑定、游戏选框、滚动条、取消/错误语义和恢复保护未改。下一可执行任务：R09-01 主题转换闪白。

## 2026-09-19 Round3 R09-01 主题转换闪白

- 最新生产实现已具备局部主题切换能力：Dashboard/Settings/ProductionShell/workspace 各自应用完整 palette，游戏选择器继续是宿主内 scrim，脱离式 ToolTip 通过既有刷新入口同步；未写 `Application.Current.Resources`。
- `fe048952` 新增真实 STA WPF 行为夹具，Light→Dark 后在下一 Render 优先级取样 Popup surface、Path icon、thumbnail placeholder 和文本，均完成资源更新且宿主字典哨兵未变；定向 `1/1`，干净 Release/XAML `24/24`、solution `0/0`。
- 这只是 Dispatcher/local-resource 的可控证据，不是物理呈现帧零闪截图；Demo 原目录不可用，真实 Playnite、系统主题切换、物理 DPI/跨屏、UIA/读屏、ETW、宿主性能和 package-host 仍未验。下一任务：R09-02 图标语义统一。

## 2026-09-19 Round3 R09-02 图标语义统一

- `c7c7ae0d` 已推送到 `codex/ui-finesse-round2`：在现有 `ThemeAwareIcon + Geometry/Path` 体系内补齐备份、恢复、上传、校验、归类、忽略六类动作资源，并新增共享 `GscActionIcon` 尺寸 `16x16`；生产 Dashboard/Overview/Save/Maintenance/Media 按钮复用图标，命令、绑定、参数、恢复保护和禁用语义保持不变。
- 精确提交隔离验证：XAML `24/24`、Release solution `0 warning / 0 error`、Playnite `net462`；`R09IconSemanticBehaviorTests=2/2`，相邻图标/动作回归 `3/3`；源码校验与 diff check 通过。夹具覆盖非空 Geometry、真实生产 XAML 语义映射和禁用 Button 的 IconData/可见性/16x16 布局保留。
- 证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R09-02-ICON-SEMANTICS-20260919.md`；Demo 原目录不可用，沿用恢复生产资源基线。用户提供的 DEV-INSTALL-008 main 全量测试失败已记录为合入后需单一 checkout 重跑的发布边界，不改写本阶段精确隔离通过结论。
- 未验真实 Playnite 呈现、物理 DPI/跨屏、presented frame、UIA/读屏、IME、ETW、宿主性能和 package-host；main 的用户未提交文件仍未触碰。下一可执行任务：R09-03 一像素描边。

## 2026-09-19 Round3 R09-03 一像素描边

- `083b7a22` 已在 `codex/ui-finesse-round2` 完成并通过实现验证：共享工作区 Tab、Dashboard 同构 Tab、Acrylic 壳层导航和 Dashboard 导航的圆角 Chrome 明确启用 `SnapsToDevicePixels=True`，保留 `UseLayoutRounding=True` 与 1 DIP 边框；没有改变游戏选框、滚动条、命令绑定、取消/错误语义、恢复保护或列表性能策略。
- 新增 `R09PixelStrokeBehaviorTests`：实际 STA WPF 生产 `TabItem`/`RadioButton` 进入选中状态后检查模板 Chrome，再用离屏 `RenderTargetBitmap` 对 1 DIP 分隔线和圆角边框执行 `1.00/1.25/1.50/1.75/2.00` 明确缩放模拟。R09-03 为 `2/2`，相邻图标/共享资源回归合计 `6/6`。
- 以 `083b7a22` 重建：XAML `24/24`、Release solution `0 warning / 0 error`、Playnite `net462`；`validate-source.py`、XAML 检查和 `git diff --check` 通过。证据见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R09-03-PIXEL-STROKE-20260919.md`。
- 五档是 96-DPI 基线上的明确缩放模拟，不是物理显示器 DPI/跨屏/真实 Playnite 呈现或 presented frame；UIA/读屏、IME、ETW、宿主性能和 package-host 仍未验。Demo 原目录不可用，沿用恢复生产基线；main 用户文件未触碰。用户 DEV-INSTALL-008 的 main 全量失败仍需合入后单一 checkout 重跑。
- 下一可执行任务：R09-04 阴影层次预算，先查共享 Effect、低 Tier 与高对比回退，再决定证据或最小修复。

## 2026-09-19 Round3 R09-04 阴影层次预算

- 现有 `AdaptiveThemePaletteFactory.ApplyMaterialResources` 已经有有限角色层级：surface `14/2`、primary button `18/0`、popup `20/5`、sidebar `24/3`、dialog `34/8`、slider thumb `6/1`（Blur/Depth）；`GscSurface` 默认无 Effect，`GscElevatedSurface` 只给主卡片使用，没有重建阴影系统。
- `3ad61099` 新增 `R09ShadowBudgetBehaviorTests`：浅/深 palette 六个冻结 Effect 参数、`glassEnabled=false` 的真实 null/透明 wash 回退、三个真实带阴影 Border 的 ScrollViewer ExtentHeight 保持均通过，R09-04 为 `3/3`。
- 精确提交重建：XAML `24/24`、Release solution `0 warning / 0 error`、Playnite `net462`；R09-02/R09-03/共享资源相邻回归合计 `9/9`，源码校验/XAML/diff check 通过。证据见 `evidence/R09-04-SHADOW-BUDGET-20260919.md`。
- 本阶段没有切换真实 Windows High Contrast；高对比系统语义归 R09-06，不能由 `glassEnabled=false` 代替。未验真实 Playnite 呈现、物理 DPI/跨屏、UIA/读屏/IME、ETW、宿主性能和 package-host；Demo 原目录不可用，main 用户文件未触碰。
- 下一可执行任务：R09-05 焦点轮廓合成，重点验证焦点与 hover/selected/error 的实际模板叠加及圆角 Clip 边界。

## 2026-09-19 Round3 R09-05 焦点轮廓合成

- 复核最新生产资源后确认没有需要重建的焦点体系：`GscSharedFocusVisual` 实际为 `2 DIP`、`CornerRadius=13` 的 accent Border；`GscWpfUiButton` 用完整圆角 `FocusOverlay` 叠加键盘焦点，Workspace Tab 的 Chrome 不裁切共享焦点描边，TextBox 的错误触发器在焦点触发器之后覆盖错误底色/边框。
- `3f7d0b30` 新增 `R09FocusOutlineBehaviorTests`：真实生产 Button 聚焦/失焦负例、共享焦点模板实例化、选中 Tab、TextBox `1..3` 校验错误与 `9→2` 恢复均通过，R09-05 `2/2`。测试 collection 串行 WPF 窗口，并避免跨 STA 共享 `Application.Current`；这只是 testhost 隔离，不改变生产代码。
- 当前 Release 精确验证：XAML `24/24`、solution `0 warning / 0 error`、Playnite `net462`；R09-02/R09-03/R09-04/共享焦点资源相邻回归合计 `11/11`；`validate-source.py`、XAML、`git diff --check` 通过。证据见 `evidence/R09-05-FOCUS-OUTLINE-20260919.md`。
- 受控 STA 未提供系统键盘输入源，未把 WPF Focus Adorner 自动挂载写成通过；真实 Playnite、物理 DPI/跨屏、presented frame、UIA/读屏、IME、High Contrast、ETW、宿主性能和 package-host 仍未验。Demo 原目录不可用，沿用恢复生产基线；main 用户文件未触碰。下一可执行任务：R09-06 高对比真实配色。

## 2026-09-19 Round3 R09-06 高对比真实配色

- `1f2eac4a` 在现有 `AdaptiveThemePalette`/局部 ResourceDictionary 管线内补齐 `IsHighContrast` 状态：高对比使用 Window/WindowText/Control/ControlDark/Highlight/HighlightText/GrayText/HotTrack 语义资源；进度、图标、禁用文字、选中/悬停、按钮 opaque stop、Popup/阴影/背景模糊回退均由同一状态驱动。未修改 Windows 全局设置。
- `R09HighContrastBehaviorTests` 使用实际 DynamicResource 绑定的 ProgressBar、Path、TextBlock，并在同一 ResourceDictionary 中验证高对比→普通 palette 的 material 恢复。正式 Release/XAML `24/24`、solution `0/0`；R09-06、相邻 R09 和高对比源码门禁 `13/13`；source/XAML/diff check 通过。
- 证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R09-06-HIGH-CONTRAST-20260919.md`。未切换真实 Windows High Contrast，不能写成 OS 方案或真实 Playnite presented frame 通过；DEV-INSTALL-008 的 main 合并后安装失败继续单列。Demo 原目录不可用，沿用恢复生产基线。
- 下一可执行任务：R09-07 缩略图占位一致，先盘点加载/失败/无图/视频/损坏文件现有入口及行高约束。

## 2026-09-19 Round3 R09-07 缩略图占位一致

- `72a1a07b` 复用 `AsyncThumbnailImage` 的后台解码、取消/过期请求保护、冻结图像、缓存和并发限制，新增 `MediaThumbnailPreview` 固定媒体槽位；列表卡片保持 `164 x 154`、预览 `96 x 96`、操作区第二行 `58 DIP`，不改 DTO、命令绑定、游戏选框、滚动条或虚拟化策略。
- 真实 STA WPF 夹具覆盖无图、录像、缺失文件、损坏文件和有效 PNG：R09-07 `1/1`；AsyncThumbnailImage `2/2`、AsyncThumbnailLoader `6/6`、MediaThumbnailConverter `1/1`，R09 定向 `12/12`。正式构建 XAML `24/24`、solution `0/0`，source/XAML/diff check 通过。
- 详情截图状态文字补齐，录像继续由已有 MediaElement 承载并隐藏截图占位文字；成功缩略图迟到只替换固定槽位，不改变行高或挤占操作区。证据：`evidence/R09-07-THUMBNAIL-PLACEHOLDER-20260919.md`。
- 使用隔离临时目录与合成媒体；未验真实 Playnite presented frame、物理 DPI/跨屏、UIA/读屏、IME、ETW、宿主性能和 package-host。Demo 原目录不可用，沿用恢复生产基线；DEV-INSTALL-008 main 全量安装失败仍单列，main 用户改动未触碰。下一可执行任务：R09-08 主题背景压力。
## 2026-09-19 Round3 R09-08 主题背景压力

- `7de5c3de` 复用现有 `AdaptiveThemePaletteContrastGuard` 的真实 alpha 合成路径，新增 `R09BackgroundPressureBehaviorTests`：浅/深/暖/蓝四种合成宿主背景读取实际运行时 backdrop、ambient、glass 资源，透明 stop 保持可见，正文合成对比度均达到 `4.5`；故意失败负例 `1/1` 被拒绝。
- 同阶段修复 R09-06 的主题工厂兼容性回归：恢复四参数 `AdaptiveThemePaletteFactory.Create`，高对比隔离 override 使用独立方法；更新过时结构断言后，主题/材质 `8/8`、高对比/主题/阴影 `5/5`、R09 `14/14`。正式 Release/XAML `24/24`、solution `0/0`，source/XAML/diff check 通过。
- 证据：`evidence/R09-08-BACKGROUND-PRESSURE-20260919.md`。证据是隔离 STA/逻辑 DIP 资源合成，不是真实 Playnite presented frame、物理 DPI/跨屏、UIA/读屏、IME、ETW、宿主性能或 package-host；Demo 原目录不可用，沿用恢复生产基线。DEV-INSTALL-008 main 全量失败仍单列，main 用户文件未触碰。下一可执行任务：R10-01 上下文返回。

## 2026-09-19 Round3 R10-01 上下文返回

- `97770ed4` 复用既有 `FindingNavigationResolver`、稳定 `PlayniteId` 和任务 DTO，新增不落盘的 `WorkspaceNavigationSnapshot`/LIFO 栈；告警→存档/失败任务、任务→关联游戏均有真实命令与返回入口。恢复前先切工作区，再恢复游戏，避免选中游戏监听器在错误页启动详情加载。
- 返回快照覆盖 Save/Media/Maintenance 页签、任务筛选/历史范围/导航目标、任务/诊断选择和实际 DataGrid 内部 ScrollViewer 偏移。`TaskCenterView`/`MaintenanceView` 通过 `Loaded/Unloaded/ScrollChanged` 真实捕获与恢复有限列表滚动；对象消失时不替换其他游戏/任务并给出状态说明。
- 当前隔离验证：source validation 通过、XAML `24/24`、Release solution `0 warning / 0 error`、R10 与相邻告警路由 `13/13`、diff check 通过。证据：`evidence/R10-01-CONTEXT-RETURN-20260919.md`。
- 证据仅覆盖 fake/合成状态、隔离 STA WPF 和逻辑 DIP；未验真实 Playnite 返回序列、presented frame、物理 DPI/跨屏、UIA/读屏、IME、ETW、宿主性能和 package-host。Demo 原目录不可用，沿用恢复生产基线。用户 DEV-INSTALL-008 main 全量日志的 `73 failed / 588 passed / 57 skipped` 作为独立发布边界记录，未在 dirty main 上覆盖或重跑。
- 下一可执行任务：R10-02；真实宿主与 main 合并后的单 checkout 安装器复验仍未完成。

## 2026-09-19 Round3 R10-02 定位当前游戏

- 盘点确认无需另建选择器：任务详情沿用 R10-01 的 `SelectedTask.GameId` 精确入口；媒体页沿用当前游戏 Shell 选框、`SelectedGame.Name` 和 `SelectedGame.PlayniteId` 请求身份。`GamePickerViewModel` 已按 ID 选取，不按重名显示名猜测。
- `3c258873` 新增 R10-02 行为夹具：两个同名 synthetic 游戏中指定第二个稳定 ID，实际 `GamePickerViewModel.SelectGame` 选中第二个；同时核对任务/媒体/Shell 生产接线。R10-02 `2/2`，R10-01/告警/页面相邻合计 `15/15`。
- 当前隔离验证：Release solution `0 warning / 0 error`、XAML `24/24`、source/XAML/diff check 通过。证据：`evidence/R10-02-CURRENT-GAME-20260919.md`；账本 R10-02 已改为“已满足”。
- 仅证明 synthetic DTO、真实选框 ViewModel、生产接线和隔离 net462 testhost；未验真实 Playnite 操作、presented frame、物理 DPI/跨屏、UIA/读屏、IME、ETW、宿主性能和 package-host。DEV-INSTALL-008 main 全量 `73 failed / 588 passed / 57 skipped` 仍为独立发布边界，main 未触碰。
- 下一可执行任务：R10-03；真实宿主定位与呈现仍未完成。

## 2026-09-19 Round3 R10-03 搜索快捷键

- `563e6862` 复用现有 `FocusWorkspaceSearch` 和各页搜索框，新增最小 `SearchShortcutPolicy`；Ctrl+F 仅在没有对话框、Shell 游戏选框或紧凑游戏浏览器占用输入时聚焦当前页搜索。未改动 Playnite 全局键、IME、方向键、Enter 或 Esc 选框语义。
- 行为与相邻接线定向测试 `9/9`；正式 Release/XAML `24/24`、solution `0 warning / 0 error`，source/XAML/diff check 通过。证据：`evidence/R10-03-SEARCH-SHORTCUT-20260919.md`；账本 R10-03 已改为“已满足”。
- 证据只覆盖隔离 net472 testhost、生产源码接线和策略正/负例；未验真实 Playnite 全局快捷键协作、IME/物理输入、UIA/读屏、presented frame、物理 DPI/跨屏、ETW、宿主性能和 package-host。Demo 原目录不可用，沿用恢复生产基线；DEV-INSTALL-008 main 全量失败仍单列，main 未触碰。
- `.tmp/r10-03-build` 清理已尝试，部分文件因 Access denied 仍被现有 dotnet/testhost 占用，未强杀未知进程，临时输出未提交。下一可执行任务：R10-04 快捷键帮助；真实 Playnite 输入与呈现仍未完成。

## 2026-09-19 Round3 R10-04 快捷键帮助

- `19be9f12` 复用现有 `RelayCommand`/`CanExecute` 和 R10-03 的搜索路由，新增 Shell 页头键盘操作入口及 Popup；帮助目录按当前工作区生成 Ctrl+F 说明，禁用命令不显示，未增加未接线的全局快捷键。
- 真实 STA WPF 点击行为与目录负例、当前页文案及生产接线定向回归 `13/13`；正式 Release/XAML `24/24`、solution `0 warning / 0 error`，source/XAML/diff check 通过。证据：`evidence/R10-04-KEYBOARD-HELP-20260919.md`；账本 R10-04 已改为“已满足”。
- 证据覆盖隔离 STA Window、真实 Shell/XAML 和合成工作区状态；未验真实 Playnite Popup 呈现、物理 DPI/跨屏、UIA/读屏、真实键盘/IME、全局键协作、ETW、宿主性能和 package-host。Demo 原目录不可用，沿用恢复生产基线；DEV-INSTALL-008 main 全量失败仍单列，main 未触碰。
- 一次 C: 盘 `0.21 GB` 空间不足的隔离构建已改用 D: 仓库 `.tmp` 成功完成并清理；旧 VBCSCompiler 锁定事实保留，未强杀未知进程。下一可执行任务：R10-05 筛选预设。
## 2026-09-19 Round3 R10-05 筛选预设

- 当前续作分支实现提交为 `005dc2c5`，已推送 `origin/codex/ui-finesse-round2`。先核对发现既有 `PolicyTemplates` 只服务备份策略，不把它误记成筛选预设。
- 新增 `FilterPresetDefinition` 与 `GameSaveCenterSettings.FilterPresets`：仅保存稳定预设 ID、名称、工作区及任务/媒体字符串筛选值；配置最多 32 条，非法旧配置在 setter/clone/JSON 往返时回退或丢弃。任务游戏值仍沿用现有查询字符串名契约，不是 DTO 引用。
- 任务/媒体页新增保存、应用、重命名、确认删除；应用复用原筛选属性和分页/刷新路径。任务紧凑布局重排不再把预设第二行设为 0 高度。未改游戏选框、滚动条、命令绑定、取消/错误语义、恢复保护和有限列表性能。
- 证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R10-05-FILTER-PRESETS-20260919.md`；账本 R10-05 已改为“已满足”。R10-05 `4/4`，R10 `14/14`，直接相关回归 `25/25`；Release/net462 `0/0`、XAML `24/24`、source/XAML/diff check 通过。
- 全量门禁事实必须分开写：Core `83/83`、Worker `311/311` 通过；同一当前 testhost 的 Playnite 为 `84 failed / 610 passed / 57 skipped`，主要是已有 PresentationSource/STA/资源/动画/DataGrid/源码契约问题，不作为本项通过依据。用户 main DEV-INSTALL-008 仍为 `73 failed / 588 passed / 57 skipped` 且安装器退出 1，main 未覆盖。
- 未验真实 Playnite 的保存/确认 Popup、最终呈现、物理 DPI/跨屏、UIA/读屏、IME、ETW、宿主性能和 package-host；Demo 原目录不可用，继续使用恢复生产基线。D: 5 个临时构建目录已清理 4 个，`continuation-r10-05-build-20260919` 的部分 VBCSCompiler analyzer DLL 仍锁定。下一可执行任务：R10-06 筛选来源提示。

## 2026-09-19 Round3 R10-06 筛选来源提示

- `5403d797` 复用 R10-01 的临时任务导航游戏条件、现有任务查询和 `RelayCommand`；任务页新增真实来源提示、无来源折叠状态和“清除带入条件”入口。专用清除只清 `taskNavigationGameId/taskNavigationGameName`，保留搜索、状态、游戏、类型、历史范围草稿；普通全量清除也会清掉临时来源条件。
- `R10FilterSourceBehaviorTests` `2/2`，完整 R10 `16/16`；最后隔离 Release/net462 build `0 warning / 0 error`、XAML `24/24`、source/XAML/diff check 通过。测试使用真实 TaskCenterView/STA Window、正负可见性和实际绑定命令，不把源码字符串断言当作唯一交互证据。
- 按协议执行的完整脚本因 C: 磁盘空间耗尽，在复制既有 Worker/XAML 中间文件阶段停止，未进入全量测试；不把它写成产品编译错误。main DEV-INSTALL-008 仍是 `0/0`、Core `83/83`、Worker `311/311`、Playnite `73/588/57`、安装器退出 1，main 用户改动未触碰。
- `.tmp/r10-06-full-20260919` 已清理；`.tmp/r10-06-build-20260919` 仅余被长驻 VBCSCompiler 锁定的 `test-temp` analyzer DLL 父目录，未强杀未知进程。真实 Playnite 清除交互、presented frame、DPI/跨屏、UIA/读屏、IME、ETW、宿主性能和 package-host 未验；Demo 原目录不可用，沿用恢复生产基线。下一可执行任务：R10-07 侧栏信息密度。

## 2026-09-19 Round3 R10-07 侧栏信息密度

- 先核对最新生产 Shell：`SidebarColumn` 已有 `270/72 DIP` 收展、固定侧栏 + 主区星号列、`ClipToBounds=True`；每个入口已有独立图标、选中模板、Tooltip、Automation 名称和 RadioButton Tab 键入口。折叠只隐藏标签并把图标槽位居中，品牌版本徽标收起时隐藏，不存在覆盖图标的导航计数徽标，因此没有重建控件。
- `ProductionShellChromeSourceTests` 新增两个真实 STA WPF Window 行为测试：折叠/展开后任务入口选中态保持，7 个入口逐项保留图标/Tooltip/Automation/Tab 入口；长导航名称下展开侧栏仍为 `270 DIP`，品牌图标与版本徽标不相交，`MainPageHost` 仍有可用宽度。该类 `12/12`，R10 组合 `28/28`。
- 当前身份绑定的 Release/net462 组合构建 `0 warning / 0 error`，XAML `24/24`，`validate-source.py`、XAML 检查和 `git diff --check` 通过。只改测试与本阶段文档，没有改生产 XAML、服务/DTO、命令绑定、游戏选框、滚动条、取消/错误、恢复保护或有限列表性能。
- 证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R10-07-SIDEBAR-DENSITY-20260919.md`。证据为隔离 STA WPF、合成长名称和逻辑 DIP；真实 Playnite/package-host、物理 DPI/跨屏、presented frame、UIA/读屏、真实键盘/IME、ETW、宿主性能仍未验；Demo 原目录不可用，沿用恢复生产基线。旧 `.tmp` 清理仍受 Access denied/锁定句柄影响，未强杀未知进程；main 用户文件未触碰。
- 下一可执行任务：R10-08 最近操作续接；继续保持用户文件、现有游戏选框/滚动条系统和 net462 兼容边界。

## 2026-09-19 Round3 R10-08 最近操作续接

- 先复用现有 `OverviewTasks`、`Activities`、`OpenActivityCommand`、`GamePickerViewModel` 和 `Games` 快照，不把任务历史误当最近访问，也没有新增服务或 DTO。
- 新增 `RecentAccessRecord`/`RecentAccessItem` 与 `DashboardViewModel.RecentAccess.cs`：设置只保存稳定 `PlayniteId`、工作区、TabIndex、UTC 时间，最多 8 条，按 ID 去重；快照后按当前游戏库清理已移除对象，标题由当前快照解析，不含本地绝对路径。
- Overview 增加独立“最近访问”有限 `ListBox`，`280 DIP` 最大高度、Recycling 虚拟化、独立 `OpenRecentAccessCommand`；点击恢复游戏、工作区和适用页签，缺失对象只清理并提示。游戏选框、滚动条、命令/绑定、取消/错误、恢复保护和 net462 保持。
- `R10RecentAccessBehaviorTests` `2/2`；R10 组合 `18/18`；定向 Release 编译成功（Contracts/Core/Playnite `net462`/Tests `net472`）；XAML `24/24`；source/XAML/diff check 通过。证据：`evidence/R10-08-RECENT-ACCESS-20260919.md`。
- 证据仅覆盖 synthetic 设置/DTO、隔离 STA WPF 和逻辑 DIP；未验真实 Playnite/package-host、最终 presented frame、物理 DPI/跨屏、UIA/读屏、真实键盘/IME、ETW、宿主性能。Demo 原目录不可用，沿用恢复生产基线；用户 main 的 DEV-INSTALL-008 `73/588/57` 与安装器退出 1 未覆盖或重跑。
- 下一可执行任务：R11-01 版本信息摘要；停止前真实未验边界仍为宿主安装/呈现和上述系统级能力。

## 2026-09-19 Round3 R11-01 版本信息摘要

- 先查现有 `BackupVersionDto`/`RestoreReadinessDto` 和 SaveCenter 版本表：时间、文件数、大小、备注、锁定、设备、恢复可用性详情均已存在；没有新增服务、DTO 请求或恢复命令。
- `SaveHistoryDeviceColumn` 改用已有 `SourceDisplay`，空来源显示“未知设备”；新增 `ProtectionAndReadinessDisplay` 组合锁定与恢复状态。状态列改为 `Ready` 才成功色，Warning/Corrupted/Failed 分别提示，Unknown/Checking/未提供校验保持中性，移除锁定即绿色的误导触发。
- 右侧版本详情、恢复校验按钮、固定列宽和现有 DataGrid 滚动保持；长摘要进入 ToolTip/详情，不推宽历史表格。游戏选框、命令绑定、取消/错误、恢复保护和有限列表性能未改。
- `R11VersionSummaryBehaviorTests` `2/2`；Save 页面相邻回归 `13/13`；定向 Release 编译成功（Playnite `net462`/Tests `net472`）；XAML `24/24`；source/XAML/diff check 通过。证据：`evidence/R11-01-VERSION-SUMMARY-20260919.md`。
- 证据使用合成 DTO、真实 SaveCenterView/STA Window 和逻辑 DIP；未验真实 Playnite/package-host、最终呈现、物理 DPI/跨屏、UIA/读屏、IME、ETW、宿主性能。Demo 原目录不可用，沿用恢复生产基线；main DEV-INSTALL-008 `73/588/57` 与安装器退出 1 未覆盖或重跑。
- 下一可执行任务：R11-02 双版本对比选择。

## 2026-09-19 Round3 R11-02 双版本对比选择

- 先复用现有 `BackupCompareRequestDto`、`MessageTypes.CompareBackups`、`BackupDiffDto` 和 Worker `FileManifestDiffService`；既有差异算法已按 `Left → Right` 计算，本阶段未新增比较服务、恢复命令或第二套数据契约。
- 存档比较页新增真实 A/B 选择框：`CompareLeftBackup` 为 A 基准、`CompareRightBackup` 为 B 对照，初始仍沿用上一版本→当前版本；`SwapCompareBackupCommand` 交换选择后重新提交现有比较请求。摘要明确“新增属于 B，删除属于 A”，历史详情按钮改为同一 A/B 语义。
- 同一版本、缺少稳定 ID或不完整选择均不会执行比较；同版本提示明确“不发起比较或恢复”。比较命令只走 `CompareBackups`，恢复保护/取消/错误/游戏选框/滚动系统和 Playnite net462 保持。
- `R11VersionComparisonBehaviorTests` `2/2`：真实 SaveCenterView/STA 绑定和同版本禁用，加上 Core manifest 反向方向反例；R11-02 与 R11-01、Save R06 相邻回归 `15/15`。定向 Release 编译无 warning/error，XAML `24/24`、source/XAML/diff check 通过。证据：`evidence/R11-02-VERSION-COMPARISON-20260919.md`。
- 证据仍只覆盖 synthetic manifest、隔离 STA WPF、真实生产视图和逻辑 DIP；未证明真实 Worker IPC/归档读取、Playnite/package-host 安装呈现、presented frame、物理 DPI/跨屏、UIA/读屏、IME、ETW 或宿主性能。Demo 原目录不可用，继续沿用恢复生产基线。main DEV-INSTALL-008 `73/588/57`、安装器退出 1 和 main 用户文件仍独立未覆盖。
- 下一可执行任务：R11-03 差异列表搜索；保持有限加载、完整路径复制和零变化/未知差异的既有边界。

## 2026-09-19 Round3 R11-03 差异列表搜索

- 先复用 `BackupDiffDto`、既有 `CopyPathCommand` 和 `GscInspectorScrollViewer`；新增 `BackupDiffPathFilter` 只对当前比较结果做内存投影，不新增存储、IPC 或恢复请求。
- 比较页新增全部/新增/修改/删除类型筛选、路径片段搜索、清除入口和匹配摘要。每类初始最多显示 120 条，`LoadMoreDiffPathsCommand` 每次再增加有限窗口；匹配计数不被显示窗口截断。
- 每条新增/修改/删除路径改为只读原始 TextBox，保持完整相对路径可选，并绑定现有 `CopyPathCommand`；未变化数量和非 Exact 的未知差异质量独立显示。同步修正 R11-02 新增行位造成的比较卡片摘要/列表重叠。
- `R11DiffListSearchBehaviorTests` `2/2`；R11-01/R11-02/R11-03 WPF 夹具串行 `6/6`；R06 存档相邻回归 `11/11`。定向 Release 编译无 warning/error，XAML `24/24`、source/XAML/diff check 通过。专用 `R11SaveWpf` 集合禁并行，避免 STA 全局资源竞争污染结果。证据：`evidence/R11-03-DIFF-LIST-20260919.md`。
- 未验真实 Worker IPC/归档读取、大型真实清单呈现、Playnite/package-host 安装呈现、presented frame、物理 DPI/跨屏、UIA/读屏、IME、ETW 或宿主性能。Demo 原目录不可用，继续沿用恢复生产基线；main DEV-INSTALL-008 `73/588/57`/安装器退出 1 和 main 用户文件仍独立未覆盖。
- 下一可执行任务：R11-04 版本说明编辑；继续保持稳定 BackupId 不受显示备注影响。

## 2026-09-19 Round3 R11-04 版本说明编辑

- 先核对既有元数据链路：`BackupMetadataUpdateDto`、`LudusaviClient.EditBackupAsync`、Worker `RefreshBackupHistoryAsync` 和 SQLite `backup_versions` upsert 已支持备注/锁定持久化；请求不含 `ArchivePath`，没有重建服务或 DTO。
- 补齐明确交互缺口：`DashboardViewModel` 增加本地 `CancelBackupMetadataCommand`/`HasBackupMetadataChanges`，复用 `SyncBackupEditor` 回滚当前版本原值，不发 IPC；SaveCenter 版本备注编辑区增加可访问的“取消修改”入口。
- `R11VersionNoteBehaviorTests 3/3`：真实 SaveCenterView/STA ICommand 绑定探针验证草稿取消、重复说明仍按 `BackupId` 选择；Worker 隔离 SQLite 重建夹具 `1/1` 验证备注/锁定/BackupId/归档路径；R11 串行 `9/9`、R06 相邻 `11/11`。
- D 盘隔离 Release solution `0 warning / 0 error`，Playnite `net462`、Worker 和测试从同一输出生成；XAML `24/24`、source validation、diff check 通过。C 盘构建曾因可用空间 `0` 失败，未强杀未知 dotnet/VBCSCompiler。
- 证据：`evidence/R11-04-VERSION-NOTE-20260919.md`；未验真实 Ludusavi IPC/归档读取和真实 Worker 进程重启、Playnite/package-host、最终呈现、DPI/跨屏、UIA/IME、ETW、宿主性能；Demo 原目录不可用，沿用恢复生产基线。main DEV-INSTALL-008 `73/588/57`/安装器退出 1 和 main 用户文件仍独立未覆盖。
- 下一可执行任务：R11-05 保护操作解释；继续保持真实存档/媒体/云端/诊断隔离边界。

## 2026-09-19 Round3 R11-05 保护操作解释

- 先核对既有能力：Core `RetentionPlanner` 与 Worker `RetentionSimulationService` 已对锁定、PreRestore、健康恢复点执行保留预览/应用跳过和应用前重检；本阶段复用该链路，没有重建服务或引入真实数据写入。
- `BackupVersionDto` 新增 `IsRetentionProtected`、保护 glyph、保护类型和解除条件说明；健康保护判断与 Worker 的严重异常边界对齐为 `Ready` 且 `FileCount > 0`、`TotalBytes > 0`。`SaveCenterView` 历史行绑定 `✓`/`⚠` 与解释 ToolTip，详情锁定区说明取消锁定并保存后下一次预览才重新评估；锁定草稿说明由 `DashboardViewModel` 动态通知。
- `R11ProtectionBehaviorTests 2/2`、Core `RetentionPlannerTests 3/3`、Worker 保护夹具 `2/2`；R11-01/02/03/04/05 串行 `11/11`；R06 相邻 `11/11`。Release solution `0/0`，XAML `24/24`，source validation/diff check 通过。
- 证据：`evidence/R11-05-PROTECTION-EXPLANATION-20260919.md`；真实 SaveCenterView 夹具验证行绑定状态/解释契约，不宣称最终像素呈现。WPF 非提升临时 `wpftmp` 构建曾 Access denied，改用 D 盘 `GscBuildOutputRoot` 完成同一验证；未绕过 ETW/系统跟踪权限。
- 未验真实 Ludusavi/Worker IPC/归档读取、Playnite/package-host 安装呈现、presented frame、物理 DPI/跨屏、UIA/读屏、真实键盘/IME、ETW、宿主性能；Demo 原目录不可用，沿用恢复生产基线。main DEV-INSTALL-008 `73/588/57`、安装器退出 1 和 main 用户文件仍独立未覆盖。
- 下一可执行任务：R11-06 备份前变更摘要；继续保持真实存档、媒体、云端和诊断隔离边界。

## 2026-09-19 Round3 R11-06 备份前变更摘要

- 先核对既有能力：`LudusaviClient.BackupAsync` 已支持 `preview` 参数，原生产入口只走真实执行；本阶段复用 `BackupRequestDto` 和 `LudusaviResultParser`，新增只读 `backup.preview` IPC，不重建备份服务。
- `BackupPreviewDto` 区分 Loading/Ready/NoData/Unavailable/Error，展示扫描时间、路径数、预计大小和最多 120 条已识别路径。Worker preview 不创建备份目录、不创建任务、不写历史/SQLite、不上传云端；`LudusaviClient` 仅在真实执行模式创建备份目录。
- SaveCenter 历史摘要增加“预览备份”按钮、非破坏提示和已识别路径；立即备份执行前仍由原 Worker 真实链路重新扫描，不使用旧预览保证安全；切换游戏或执行结束清理旧摘要。
- `BackupPreviewBehaviorTests 2/2`、`R11BackupPreviewBehaviorTests 1/1`；R11 串行 `12/12`；R06 相邻 `11/11`。Release solution `0/0`，XAML `24/24`，source validation/diff check 通过。
- 证据：`evidence/R11-06-BACKUP-PREVIEW-20260919.md`。合成 Ludusavi JSON/隔离 STA 只证明解析、状态区分和生产视图绑定；未验真实 Ludusavi 输出、Worker IPC、归档文件系统变化、Playnite/package-host、最终 presented frame、DPI/跨屏、UIA/IME、ETW、宿主性能；Demo 原目录不可用，沿用恢复生产基线。
- main DEV-INSTALL-008 `73/588/57`、安装器退出 1 和 main 用户文件仍独立未覆盖；下一可执行任务：R11-07 备份结果分层。

## 2026-09-19 Round3 R11-07 备份结果分层

- 先复用已有 `CloudTransferStatusDto`、云端状态服务、`RetryCloudUpload` IPC 和重试队列；新增 `BackupResultDto` 只表达本地成功与云端后续状态，不新增第二套上传服务。
- 本地历史版本已索引并持久化后发布本地成功结果；云端排队、镜像失败、认证待处理、传输中、已上传待远端校验、远端已校验分别显示。云端失败仍保留失败任务语义，但 `HasPartialSuccess` 使 Playnite 不隐藏已成功本地历史；单独重试只执行云端复制，不重新创建本地版本。
- `TaskCoordinator`、实时 `TaskEventBroadcaster` 和终态复制均保留 DTO。SaveCenter 结果卡片和补救按钮使用真实生产视图绑定；`Uploaded` 负例不会显示“单独重试云端上传”，也不会显示“远端已校验”。
- 验证：Worker 分层/终态/实时事件 `9/9`，云状态相邻组合 `20/20`；R11 Playnite 定向 `14/14`；D 盘隔离 Release `0 warning / 0 error`；XAML `24/24`；source validation、XAML、diff check 通过。
- 同步校正 main 日志中确认的两条过期 XAML 连续字符串门禁，改为元素/属性关系检查；校正后相关 Playnite `6/6`。这不等于 dirty main 已修复或全量门禁已绿。
- 真实边界：只用合成 DTO、fake TaskCoordinator、隔离 STA WPF 和隔离输出；未验真实 Ludusavi/rclone/Worker IPC、云端/存档、Playnite package-host/安装呈现、presented frame、物理 DPI/跨屏、UIA/IME、ETW、宿主性能。Demo 原目录不可用，沿用恢复生产基线；WPF C 盘 worktree 的 `wpftmp` 写入权限阻塞，改用 D 盘可写副本验证，未绕过系统权限。
- main DEV-INSTALL-008 仍独立记录为 Release `0/0`、Core `83/83`、Worker `311/311`、Playnite `73 failed / 588 passed / 57 skipped`、安装器退出 `1`，没有进入打包/安装；main 的 `DashboardView.xaml.cs`、`src.zip`、Dialog/R08 用户文件未触碰。
- 提交并推送：`02860571`，证据为 `evidence/R11-07-BACKUP-RESULT-LAYERS-20260919.md`。
- 下一可执行任务：按用户顺序先处理 R00/R01 小批量问题修复与证据校正，再推进依赖已满足的 R11-08。

## 2026-09-19 Round3 R00/R01 合并后门禁纠偏

- 用户 main 的 DEV-INSTALL-008 失败事实独立保留：Release `0/0`、Core `83/83`、Worker `311/311`、Playnite `73 failed / 588 passed / 57 skipped`、安装器退出 `1`，未打包/安装。首个 SaveWorkspace 失败是 XAML 列帮助属性插入后，旧连续字符串断言漂移，不是命令不可达的充分证据。
- continuation 分支已提交 `c975e16d` 并推送 `codex/ui-finesse-round2`。本批修正 XAML 关系断言、最新状态/字体/布局测试契约、R08 行为的有界 Dispatcher 等待；增加 WPF 类级 testhost 隔离和输出目录 TEMP/TMP；生产代码仅增加导航返回按钮早期空引用保护。
- D 盘隔离 `build3`：XAML `24/24`、Release solution `0 warning / 0 error`；Playnite source `65` 类组 + WPF `84` 类进程全通过；资源字典 `137 passed / 39 skipped / 0 failed`；Core `84/84`，Worker `322 passed / 1 skipped / 0 failed`。当前 worktree 的 source/XAML/diff check 均通过。
- 这只是当前分支的测试与边界校正，不反写 dirty main 为已修复或已安装。未验真实 Playnite/package-host、物理 DPI/跨屏、UIA/IME、presented frame、ETW、宿主性能；未触碰真实存档/媒体/云端/诊断。Demo 原目录不可用，继续以恢复生产基线为视觉依据。
- 证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R00-R01-TEST-GATE-CORRECTION-20260919.md`。下一可执行任务：R11-08 历史时间导航，先核对已有能力与依赖。

## 2026-09-19 Round3 R11-08 历史时间导航

- 复用 `BackupVersionDto.CreatedUtc`、`CreatedLocal`、`BackupId` 和现有历史排序；`BackupHistoryDateRange` 按本地日历日期提供全部/今天/昨天/近 7 天/近 30 天范围，活动范围排除未知时间，全部范围恢复未知与已知版本。
- `DashboardViewModel`/`SaveCenterView` 增加范围摘要、清除范围、最近/更早跳转和摘要；历史表使用独立 `CollectionViewSource`，`Backups` 仍是 A/B 选择和既有绑定的来源。未改变游戏选框、滚动条、命令绑定、取消/错误、恢复保护和有限列表约束。
- 同秒排序固定为 `CreatedUtc` 后 `BackupId`；`R11HistoryTimeNavigationBehaviorTests 3/3` 覆盖本地日历、未知时间、清除和稳定顺序。R06 `4/4`、R11 版本摘要/保护 `4/4`、资源字典 `137/39/0`；组合 `148/39/0`；Release `0/0`、XAML `24/24`。
- 实现提交 `8cc329e4` 已推送；证据为 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R11-08-HISTORY-TIME-NAVIGATION-20260919.md`。隔离合成验证不等价真实 Playnite/package-host、呈现帧、物理 DPI/跨屏、UIA/IME、ETW 或宿主性能；Demo 原目录不可用。main DEV-INSTALL-008 仍为 Playnite `73/588/57`、安装器退出 `1`，未被改写。
- 下一可执行任务：R12-01 恢复分步摘要；先核对已有恢复 DTO、任务状态及取消/错误语义。
