# GameSaveCenter 当前事实入口

> 更新时间：2026-09-06。本文是新一轮开发的短入口；历史细节仍保留在 [`PROJECT_MEMORY.md`](PROJECT_MEMORY.md)、[`WORKLOG.md`](WORKLOG.md) 和 [`DEVELOPMENT_HANDOFF.md`](../DEVELOPMENT_HANDOFF.md)，但与本文冲突时以本文和最新代码为准。

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

## 2026-09-06 复查后待办（V2-01～V2-05 已完成，余项尚未修复）

- 最新审阅为 [FOLLOWUP_REVIEW_2026-09-06.md](FOLLOWUP_REVIEW_2026-09-06.md)，基线 `8018cee`；上一轮功能已有实现，本轮只更新文档，新增 V2-01～07 和 X2-01～03，不重新执行旧路线图。
- 当前优先源码风险转为云队列摘要容量和媒体多页累积成本。均需对应行为/性能测试，不代表已经真机复现。
- 覆盖旧 F01/F03 交接中的保证：已实现存储账本不等于所有崩溃窗口闭环；V2-01～V2-05 已补齐对应的服务级提交、调度、候选恢复、IPC 身份和云端校验代际行为，但仍不能把自动测试写成断电或真实宿主证据。
- 本阶段 Release 无警告/错误，Core 65/65，Worker 293 通过/1 跳过，Playnite 339 通过/62 跳过，XAML 19/19。Worker 硬重启及部分 Named Pipe 行为测试按既有环境规则跳过；真实宿主仍为 MANUAL QA REQUIRED。

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

- 自动基线：V2-05 Release 构建 0 warning/0 error；Core `65/65`、Worker `293/294`（1 跳过）、Playnite `339/401`（62 跳过）、XAML `19/19`；E01 历史分组矩阵业务 `44/44`、IPC `22/22`、WPF/STA `45/45`、故障/Soak `4/4` 全部退出码为 0；规模基线 full 为 `2000/20000/10000/5000/500`（游戏/备份/任务/媒体/工具），stress 为 `10000/20000/10000/50000/500`，两档均通过资源增长与残留断言；`scripts/validate-source.py` 和 `git diff --check` 通过。
- 离屏 RenderHarness、静态源码检查和沙箱测试不等同真实 Playnite 宿主证据。已对当前用户 Worker 完成一次只读 `system.ping` 的真实 Named Pipe 连通性验证；E01 还在随机管道、独立 Mutex 和临时 SQLite 中完成了真实 Worker 硬中断后重启恢复验证。真实 Playnite 逐页像素、主题/高对比度、DPI、键盘焦点、媒体大库、真实云端凭据/断网和长时多进程并发仍标记为 `MANUAL QA REQUIRED`。
- 用户可操作的验收应使用隔离 Playnite 安装、独立数据目录和明确进程边界；在这些条件未提供前，继续做安全的源码/Worker/离屏验证，但不要安装插件、写入用户数据或伪造宿主通过结论。

## 新任务启动顺序

1. 先读本文，再读 `PROJECT_MEMORY.md`、`WORKLOG.md`、`DEVELOPMENT_HANDOFF.md` 和任务相关设计门禁。
2. 先确认代码事实与本文一致；若历史条目冲突，在本文补充当前覆盖关系，不删除历史证据。
3. 每个独立阶段只改一个功能边界，补行为测试，运行 Release/门禁/渲染验证，同步三份交接文档后用中文提交并推送。
