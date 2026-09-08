# GameSaveCenter 当前事实入口

> 更新时间：2026-09-08。本文是新一轮开发的短入口；历史细节仍保留在 [`PROJECT_MEMORY.md`](PROJECT_MEMORY.md)、[`WORKLOG.md`](WORKLOG.md) 和 [`DEVELOPMENT_HANDOFF.md`](../DEVELOPMENT_HANDOFF.md)，但与本文冲突时以本文和最新代码为准。

## 2026-09-08 Q6-04 构建身份兼容边界已修正（包验收进行中）

- `WorkerLauncher` 现在只在两个构建身份都已知且不一致时拒绝复用；旧 Worker 缺少 `BuildIdentity`、或任一侧为 `+unknown` 时，先完成版本/协议校验并以“身份未验证”状态兼容，不把未知身份冒充为同源。
- 未知身份仍会写入健康探测详情，便于诊断区分“可用但未证明同源”和“已知身份冲突”；同版本不同已知提交继续标记为不可复用。
- 定向 `BuildIdentityTests` 已覆盖已知冲突、旧 Worker 空字段和 unknown 组合（`3/3`）。本阶段的隔离正常打包、`-SkipBuild` 混合产物、脏工作树和环境变量恢复验证尚待完成，未触碰真实 Playnite 安装目录。

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
