# GameSaveCenter 持续维护交接与开发入口

> 这是 GameSaveCenter 的跨电脑、跨模型持续维护入口。任何新的 agent、模型或开发者接手前，先完整读取本文件，再读取项目记忆、开发进度和 UI 规则。不要只依赖聊天记录。

## 2026-09-05 U02 侧栏动画成本与快速操作终态

- 侧栏仍使用现有 Demo 认可的 210ms 宽度过渡与 190ms 内容淡入；动画期间再次点击会从当前实际宽度切换到最新目标，不再静默丢弃操作。旧动画 `Completed` 通过代际令牌失效，禁用动画和卸载会清理所有时钟并立即落到最终 72/270 DIP 状态。
- RenderHarness 新增 2000×1100 离屏布局探针，记录 Measure/Arrange、布局更新和帧间隔，并与无动画原子切换对比。独立 shell 结果为单次 Arrange 约 `9.9ms`、快速往返最终宽度 `270`、原子切换约 `4.0ms`；成本可控，未删除已有过渡。完整多尺寸/双主题/resize RenderHarness 为 `render-qa OK`。
- 真实 Playnite 长列表帧率、2000 游戏/5000 媒体压力、DPI、高对比度和卸载时序仍需人工复核；离屏探针不能替代宿主证据。

## 2026-09-05 U01 任务页视口与状态试点

- 第一阶段只落地 TaskCenter：任务页新增加载中、无数据、筛选无结果、无旧数据失败、保留旧数据失败/刷新中等明确状态。已有任务在失败或刷新时继续显示，并展示更新时间；可直接重试，筛选无结果可清除筛选。
- 低高度布局将次级任务摘要从 `84` DIP 收紧到 `64` DIP，任务列表保持 `236` DIP 最小高度；宽屏列表+Inspector、窄屏紧凑详情入口、列表内部滚动和 Recycling 虚拟化保持不变，未改真实业务命令和绑定。
- 阶段验证：Release 0 warning/0 error；Core `65/65`、Worker `260/260`、Playnite `337/399`（62 跳过）、XAML `19/19`、源码门禁通过；RenderHarness 在浅/深主题、1040×700、1366×768、2560×1440 与 resize transition 均为 `render-qa OK`。尚未取得真实 Playnite 重载后的像素/键盘证据，后续仍需人工复核高对比度、100/125/150/200% DPI、长错误详情和大数据量。

## 2026-09-05 U03 游戏目录来源与新鲜度诊断

- 已完成按 Playnite ID 的只读诊断链：Playnite 插件叠加当前来源存在性，Worker 返回脱敏的描述是否存在、原始安装标志、安装来源、描述同步时间、匹配/最后尝试、备份数量/最近备份及安装目录存在信号；完整本地路径不进入诊断 DTO。
- 新增 `games.diagnostic.get`、`games.descriptor.sync`、`games.match.retry`。同步描述只写一个当前 descriptor，不触发 Ludusavi；重试只处理一个已同步的 Worker 游戏。来源缺失仅显示“来源缺失”，备份和历史保持不变。
- `GamePickerViewModel` 的诊断原因与过滤谓词共用判断；维护页和选择器空状态均可清除搜索/状态/平台筛选，排序不变。普通 `Refresh` 仍走既有全库同步门控，不会因诊断入口扩大全库昂贵工作。
- 阶段验证：Release 0 warning/0 error；Core `65/65`、Worker `260/260`、Playnite `335/397`（62 跳过）、XAML `19/19`、源码校验通过。已跳过项是既有 UI 环境/Named Pipe 能力边界，不代表失败；真实 Playnite 目标机需人工复核来源缺失、Worker 离线、无效路径和安装状态变化。

## 2026-09-05 R07 IPC 取消与写请求结果追踪

- 客户端请求入口增加调用者/宿主生命周期令牌，连接、写入、读取可及时停止等待；取消区分用户取消与宿主退出，超时/管道断开单独报告。详情和媒体分页已接入页面请求取消，旧响应继续由 generation/选中 ID 丢弃。
- 破坏性 IPC 使用持久化 RequestId 账本：同 ID 的已完成请求重放原响应，处理中不二次执行，Worker 重启将未完成记录标为 Interrupted；备份/恢复任务写入 RequestId，任务页可按 RequestId 查 TaskId。账本保留 7 天。
- 阶段验证：Release 0 warning/0 error；Core `65/65`、Worker `258/258`、Playnite `333/395`（62 跳过）、XAML `19/19`、源码校验通过。真实 Named Pipe 5 项行为测试因当前沙箱禁止客户端连接而能力探测跳过；完整 Windows/Playnite 环境应复核取消和重放。

## 后续执行顺序

- R01～R07、U03、U01 任务页试点、U02 侧栏动画试点已完成。下一阶段按实施方案推进 F01/F02/F03 或 E01/E02；真实宿主、Worker 重启、长任务断线和媒体大数据量仍是人工验收项。

## 2026-09-05 R06 媒体按需分页与稳定查询

- 新增 `MediaQueryDto`/`MediaPageDto` 和三类媒体分页 IPC；Worker 采用 `(captured_utc, media_id)` 稳定游标，服务端支持类型、收藏、关键词过滤，`TotalCount` 独立于当前游标，旧列表消息继续兼容。
- 当前游戏、待归类、已忽略列表首批均为 200 条；ViewModel 分别保存模式游标/总数，搜索和筛选重取首屏，提供加载更多，保留选中 ID、批量操作、Item 滚动和 Recycling 虚拟化。SQLite 复合索引有查询计划测试。
- 阶段验证：Release 构建 0 warning/0 error；Core `65/65`、Worker `255/255`、Playnite `333/390`（57 跳过）、XAML `19/19`、源码校验和 `git diff --check` 通过。真实 Playnite 宿主、50,000 项压力、4 MiB 消息边界和长时 UI 性能仍待执行。

## 2026-09-05 R05 游戏筛选下拉框 STA 行为验证

- 新增真实 WPF STA 行为回归：程序化修改关闭状态下的游戏筛选 ComboBox 不会写回共享状态；窗口托管下打开→选择→关闭会通过 `DropDownClosed` 提交最终值。当前无需修改生产提交策略。
- 生产 Shell 与兼容 Dashboard 继续使用 OneWay 显示绑定、`UiFilterSelection.Synchronize` 和 `DropDownClosed` 唯一写回；真实 Playnite UI Automation、字符搜索、Esc、双实例、主题/DPI 仍需人工复核。

## 2026-09-05 R04 任务统计口径与完整历史查询

- Worker 新增全量任务查询契约与 `tasks.page` IPC：状态、游戏、类型、关键词、创建时间半开区间均可服务端过滤，使用 `(created_utc, task_id)` 游标分页；独立摘要聚合不再被最近任务窗口截断。
- Dashboard 云端待处理数来自全量摘要，今日完成按本地日换算 UTC 半开区间统计；任务页增加最近/全部历史、时间范围、加载更多和已加载/总数提示，保留现有任务命令、详情 Inspector、虚拟化和滚动。
- `finished_utc,state` 索引有 `EXPLAIN QUERY PLAN` 自动证据；Release 0 warning/0 error，Core `65/65`、Worker `251/251`、Playnite `331/388`（57 跳过）、XAML `19/19`、源码门禁均通过。真实 Playnite 宿主、长历史性能和跨午夜人工验收仍待执行。

## 2026-09-05 下一轮开发任务包

- 新增 [项目完善实施方案](ai/IMPROVEMENT_ROADMAP_2026-09-05.md)，按 R01～R07、U01～U03、F01～F03、E01～E02 提供 15 个可交接任务。优先处理任务锁释放、全局清理确认/互斥、清理隔离账本，再推进统计、分页、交互与功能增强。
- 该节记录最初的源码审阅交接；R01～R07 已在其后完成，应用版本继续为 0.6.73。现有自动化通过不等于真实宿主行为已经验收。
- Release 0 warning/0 error；Core 65/65、Worker 258/258、Playnite 333 通过/62 跳过；未新增真实 Playnite 或渲染验收。新 AI 应先检查后续提交，避免重复实施。

## 2026-09-04 Worker 描述缓存安装状态陈旧

- 0.6.72 只修复了 WPF 选择器的程序化筛选抢写，用户实测仍有“全部/已匹配/有备份可见、已安装不可见”。
- 0.6.73 已修复真正的数据链：`GameCatalogService` 不再把匹配输入缓存当成完整描述缓存；`IsInstalled`、`InstallDirectory`、Actions 等非匹配字段变化会独立写入 SQLite，不会使已有 Ludusavi 匹配失效。
- 本阶段已通过 Release 0 warning/0 error、Core `65/65`、Worker `235/235`、Playnite `331/388`（57 跳过）、XAML `19/19`、源码/WPF 门禁；0.6.73 已安装到本机 Playnite，日志确认 `GameSaveCenter 0.6.73.0 loaded`。
- 目标机器复核步骤：完全退出 Playnite 和 Worker，安装 [GameSaveCenter-0.6.73.pext](../artifacts/GameSaveCenter-0.6.73.pext) 或对应 Release 包，重新打开后执行一次刷新，再在“已安装”搜索“死亡空间”。本机样本只有 3 个游戏，没有目标条目。

## 2026-09-03 游戏选择器用户选择写回竞态补强

- 用户确认目标游戏行信息已显示“已安装”，但“全部”可以搜索、“已安装”无法搜索。0.6.71 的 `SelectionChanged` 写回仍会把程序化初始化/绑定刷新当成用户选择。
- 0.6.72 已将生产 Shell 和兼容 Dashboard 的筛选写回入口改为 `DropDownClosed`；程序化 `SelectedItem` 对齐不会再反向修改共享筛选状态，并新增“已安装 + 死亡空间搜索”回归测试。
- 交付门槛已完成：Release 0 warning/0 error；Core `65/65`、Worker `234/234`、Playnite `331/388`（57 跳过）；XAML `19/19`、源码/WPF 门禁和 Render QA 通过；本机 Playnite 日志已确认 `GameSaveCenter 0.6.72.0 loaded`。目标游戏当前不在本机三条样本数据中，真实目标机器仍需用户安装 0.6.72 后复核。

## 2026-09-03 游戏选择器双向绑定残留与安装判定补强

- 用户在真实使用中复核到上一轮后仍有“全部/已安装找不到、已匹配/有备份/需处理可见”。这说明仅在加载后同步 ComboBox 仍不足以覆盖 WPF 两套选择器副本的 ItemsSource 重建竞态。
- 生产 Shell 与兼容 Dashboard 的状态、平台、排序 ComboBox 现在只从 `GamePickerViewModel` 单向显示；用户实际选择通过 `SelectionChanged` 明确写回，所有静态/动态首项都不再隐式写回共享状态。
- Playnite 安装状态增加有效本地 Play action/working directory 兜底，Steam URI 不会被当成本地文件；版本升为 `0.6.71`，安装后应在 `extensions.log` 确认 `GameSaveCenter 0.6.71.0 loaded`，并重启 Playnite 使旧 0.6.70 DLL 卸载。
- 本轮交付门槛已完成：Release 编译 0 warning/0 error，全量自动化为 Core `65/65`、Worker `234/234`、Playnite `330/387`（57 跳过），XAML `19/19`、源码/WPF 门禁和 Render QA 通过；本机安装目录与打包暂存 DLL 哈希一致，`extensions.log` 已确认 `GameSaveCenter 0.6.71.0 loaded`。真实嵌入 Playnite Dashboard 和目标游戏仍需用户复核，不能把 ControlledAuditWindow 当成宿主真机证据。

## 2026-09-03 游戏选择器合法过期选中值收口

- 在上一轮移除状态/排序静态 `SelectedIndex="0"` 后，仍需防御 WPF 两套共享选择器各自留下的合法旧选中值；否则控件可能显示“全部”，但 `GamePicker.StatusFilter` 仍是“已安装”。
- 已在 `UiFilterSelection` 增加 `Synchronize`，生产 Shell 与兼容 Dashboard 的加载和动态平台列表恢复均以共享 ViewModel 值同步状态、平台和排序，用户改变选项后仍由 ViewModel 作为唯一状态源。新增 STA 回归测试。
- 自动验证：Core `65/65`、Worker `234/234`、Playnite `329/386`（57 跳过）、Release 0 warning/0 error、源码/XAML/WPF 门禁和 Render QA 全部通过；0.6.70 包已安装到本机 Playnite且 DLL 哈希与隔离构建一致。真实宿主审计未能通过 UI Automation 进入 Dashboard，不能替代用户第二台机器复核。

## 2026-09-03 设置页持久化选择状态收口

- 设置页的备份格式、压缩方式和主题模式此前同时设置静态 `SelectedIndex="0"` 与持久化 `SelectedValue` 双向绑定；WPF 初始化时可能把已保存的 `Simple`、`Deflate` 或深色主题抢回第一项。
- 已移除三处局部静态首项，显式使用 `Mode=TwoWay` 与 `UpdateSourceTrigger=PropertyChanged`，并补充 ToolTip 与 UI Automation 名称；设置模型原有保存/导入契约不变。
- 自动证据：Release 0 warning/0 error、Core `65/65`、Worker `234/234`、Playnite `328/385`（57 跳过）和源码校验通过。当前阶段没有改变页面布局，因此未重复执行渲染截图；真实 Playnite 重新打开设置、切换主题并重启后的配置恢复仍需人工复核。

## 2026-09-03 设备冲突状态与媒体筛选绑定收口

- 设备冲突详情的人工决策和媒体中心的类型筛选不再同时使用静态 `SelectedIndex="0"` 与双向 ViewModel 绑定，已保存的“以本机为准/以远端为准”和上次媒体筛选不会在加载时被重置。
- 设备详情新增远端备份隔离状态卡，持续显示“尚未下载”或已校验的游戏、设备、备份 ID 及有效时间；决策、下载、恢复控件补充 ToolTip 和 UI Automation 名称，真实恢复/覆盖安全语义不变。
- 自动证据：Release 0 warning/0 error、Core `65/65`、Worker `234/234`、Playnite `327/384`（57 跳过）、源码校验和 WPF 静态审计通过。真实多设备、媒体筛选恢复和宿主无障碍树仍需人工复核。

## 2026-09-03 任务中心批量安全重试

- TaskCenter 新增“重试可恢复”按钮；从当前最近任务中筛选失败/取消且已有安全重试路径的任务，同一游戏与任务类型只取最新一条，`BackupAll`/`MediaInbox` 各只执行一次。
- 批量操作先二次确认，逐项调用既有 Backup、MediaSync、CloudUpload 或 MediaInbox 重试分流；单项失败会继续处理其他项，最后汇总成功/失败并刷新任务列表，不扩大 Worker/IPC 协议。
- 自动证据：Release 0 warning/0 error、Core `65/65`、Worker `234/234`、Playnite `326/383`（57 跳过）、源码校验和 WPF 静态审计通过。真实 Playnite 任务中心命中、批量确认和长任务行为仍需人工复核。

## 2026-09-03 Playnite 游戏菜单补充媒体同步

- Playnite 游戏右键菜单新增“同步媒体”，单选和多选均可用；执行前会同步当前游戏描述，复用现有 Worker `media.sync`、媒体开关和云端上传设置。
- 媒体归档关闭时只提示用户，不提交请求；该入口没有新增业务实现或 IPC 协议，仍沿用现有任务和通知链路。
- 自动证据：Playnite `325/382`（57 跳过）和源码校验通过；Release 构建与 Core/Worker 全量回归需在本阶段收口，真实 Playnite 菜单命中仍需人工复核。

## 2026-09-03 游戏级云端状态汇总媒体上传

- Dashboard 的 `GetDashboardGameRecordsAsync` 现在在一次 SQLite 媒体聚合中计算已归类媒体的 `Failed`、`RetryScheduled`、`Pending` 和 `Synced/Uploaded` 数量，并与存档上传状态合并。
- 游戏级状态优先级为失败、等待重试、待上传、已上传；因此存档已上传但媒体失败时，游戏选择器、Overview 和详情页会显示“上传失败”。Inbox/Ignore 媒体不参与游戏级状态。
- 自动证据：新增 Worker 持久化回归后 Worker `234/234`；Release 构建、Core/Playnite 自动化、源码校验和 WPF 静态审计通过。真实 Rclone 远端断网恢复、媒体长传输和宿主显示仍需人工复核。

## 2026-09-02 游戏选择器“全部”状态被抢写

- 用户复核上一轮跨机器修复时发现：某款游戏在“已匹配/有备份”能看到，但切换到“全部”或“已安装”后消失。
- 已确认数据层不是按状态分别查询：Worker 的 Dashboard 聚合 SQL 从 `games g` 全量返回，ViewModel 的“全部”分支也不会排除未安装条目。实际问题是生产 Shell 与隐藏兼容 Dashboard 各有一套绑定同一 `GamePicker` 的 ComboBox；状态和排序静态列表上的 `SelectedIndex="0"` 与双向 `SelectedItem` 绑定在初始化时存在抢写，界面显示值和 ViewModel 实际筛选值可能不一致。
- 已修复：两套 XAML 的状态、排序 ComboBox 删除强制 `SelectedIndex="0"`，以持久化/用户当前的共享 ViewModel 值为唯一来源；动态平台列表仍保留索引 0 和 Loaded 恢复。新增 ViewModel 和 XAML 源码回归测试。
- 自动验证已通过：Release 0 warning/0 error；Core `65/65`、Worker `233/233`、Playnite `325/382`（57 跳过）；源码门禁和 WPF 静态审计通过。需要在真实第二台 Playnite 中安装新包，确认首次打开、选择“全部/已安装”及重新打开面板后目标游戏均可见。

## 2026-09-02 跨机器 Steam 游戏搜索不到

- 根因已确认：GameSaveCenter 的搜索数据来自 Playnite `Database.Games` 和 Worker SQLite 快照，不会直接枚举 Steam 客户端；旧逻辑对 500+ 游戏库在 Dashboard 打开时跳过自动目录同步，第二台机器的空/旧 Worker 缓存因此不会出现新游戏；同时默认“已安装”筛选只看 Playnite `IsInstalled`。
- 已修复：Dashboard 首屏仍缓存优先，但打开后立即同步 Playnite 目录描述；Worker 先持久化整批描述，昂贵的 Ludusavi 匹配继续由节流后台队列处理；适配器在 Playnite 标志为 false 但 `InstallDirectory` 实际存在时按已安装处理。大库回调在 Dashboard 打开后也不再被超大库门禁吞掉。
- 自动证据：Core `65/65`、Worker `233/233`、Playnite `323/380`（57 跳过），Release 0 warning/0 error，源码校验通过，WPF 静态审计 0 error/18 warning/172 info。真实 Playnite 宿主尚未在本机运行，安装新包后需在第二台电脑重启 Playnite/插件并打开一次 Dashboard 复核。
- 边界：若游戏没有被 Playnite 的 Steam 集成导入到 `Database.Games`，本插件仍没有可同步的来源；应先确认 Playnite 本身能看到该游戏，再检查插件的“全部”筛选或重新打开 Dashboard。

## 2026-09-02 媒体收件箱旧加载继续分页

- `DashboardViewModel` 的媒体收件箱加载现在把 `mediaInboxLoadGeneration` 传入分页读取；每一页 IPC 发起前和返回后都会检查代际。
- 页面卸载或切换“待归类/已忽略”后，旧请求最多完成当前已发出的单页，不再继续请求剩余分页，也不会再进入收件箱集合和 UI 状态回写；公共 IPC 的既有客户端超时语义不变。
- 自动证据：Release 0 warning/0 error，Core `65/65`、Worker `232/232`、Playnite `321/378`（57 跳过），源码校验和 WPF 静态审计通过。真实 Playnite 快速切换、卸载和 Worker 重启仍需人工观察。

## 2026-09-01 IPC 长连接读取器的取消等待对象累积

- `BoundedIpcLineReader` 原先用 `Task.Delay(Timeout.Infinite, token)` 与每次底层读取竞争；读取先完成时，该 Delay 和取消注册会一直保留到整个事件监听器取消。
- 现在用 `TaskCompletionSource` 配合一次性 `CancellationToken.Register`，在底层读取完成或取消竞争结束后释放注册；IPC 协议、4 MiB 行上限和超大消息丢弃语义不变。
- 自动证据：阻塞读取取消与可释放注册回归通过；Release 0 warning/0 error，Core `65/65`、Worker `232/232`、Playnite `321/378`（57 跳过），源码门禁通过。真实宿主长时间任务通知连接仍需人工观察。

## 2026-09-01 初始同步取消的令牌源释放竞态

- `DashboardViewModel.CancelInitialSynchronization` 取消页面初始同步时，现在容忍后台同步任务已在并发窗口中先行释放 `CancellationTokenSource` 的情况；`ObjectDisposedException` 只表示任务已经结束，不再影响页面卸载。
- 仍由初始同步任务的 `finally` 负责释放令牌源，避免卸载线程与 `Task.Delay` 注册竞争；代际号失效和正常取消语义保持不变。
- 自动证据：定向 Playnite 源码回归通过；Release 0 warning/0 error，Core `65/65`、Worker `230/230`、Playnite `320/377`（57 跳过），源码门禁通过。真实宿主快速打开/关闭与 Worker 启动竞态仍需人工复核。

## 2026-09-01 原生确认框 UI 线程边界

- `GameSaveCenterPlugin.ConfirmAsync` 的 Playnite 原生对话兜底现在也通过 `TryInvokeUi` 在宿主 UI Dispatcher 内执行；游戏停止、快捷操作等后台续体不会直接从线程池调用 `PlayniteApi.Dialogs.ShowMessage`。
- UI Dispatcher 已关闭或调用失败时按未确认处理，危险动作保持拒绝；嵌入式确认事件和正常确认语义不变。
- 自动证据：定向 Playnite 源码回归通过；Release 0 warning/0 error，Core `65/65`、Worker `230/230`、Playnite `319/376`（57 跳过），源码门禁通过。真实宿主后台事件触发确认框与关闭竞态仍需人工复核。

## 2026-09-01 自动修改器审计异常隔离

- `GameToolService.LaunchAfterDelayAsync` 现在通过 `TryAppendAutoStartAuditAsync` 统一写入跳过/成功/失败审计；审计失败只记录 Debug，不会从脱离任务继续抛出。
- Worker 停止令牌已取消时，自动启动失败按停机取消记录 Debug；正常业务启动失败仍按 Error 记录，审计写入是 best-effort，不影响任务主流程。
- 自动证据：Release 0 warning/0 error，Core `65/65`、Worker `230/230`、Playnite `319/376`（57 跳过），源码门禁通过；真实自动修改器启动、进程权限和 Worker 关闭竞态仍需人工复核。

## 2026-09-01 自动修改器审计写入退出期取消

- `GameToolService.LaunchAfterDelayAsync` 的跳过、成功和失败审计写入现在使用延迟启动任务的取消令牌，不再使用 `CancellationToken.None`；停机取消会被现有取消分支观察。
- 这补齐了 STAB-015 对自动修改器启动链的最后一段生命周期边界；真正的启动失败仍记录 Error 并尝试写审计，正常 Worker 停止不再补写。
- 自动证据：Release 0 warning/0 error，Core `65/65`、Worker `230/230`、Playnite `319/376`（57 跳过），源码门禁通过；真实自动修改器与 Worker 停机竞态仍需人工复核。

## 2026-09-01 游戏会话自动化退出期取消

- `GameSessionCoordinator` 的脱离请求后台任务现在统一使用 `ApplicationStopping`：自动修改器延迟启动、退出备份、退出媒体同步、游玩中定时备份和定时媒体同步均会随 Worker 停止取消。
- `RunSafeAsync` 将 Worker 停止期间的 `OperationCanceledException` 记为 Debug，不再把正常停机写成业务失败；真正的异常仍按原有错误日志处理。会话停止请求本身仍使用调用方令牌完成必要的状态收口。
- 自动证据：Release 0 warning/0 error，Core `65/65`、Worker `229/229`、Playnite `319/376`（57 跳过），源码门禁通过；真实 Worker 忙碌时退出与外部进程取消仍需人工复核。

## 2026-09-01 会话快照退出期取消

- `SavePathDetectionService.BeginSessionCapture` 仍然不等待短暂 IPC 请求，但现在使用 Worker 的 `IHostApplicationLifetime.ApplicationStopping` 作为后台扫描令牌；Worker 停止时会取消存档路径快照，避免 SQLite/文件存储释放后继续回写。
- 取消异常由既有完成回调观察，正常停止不会升级为未观察任务异常；真实 Worker 忙碌时重启、文件扫描取消时机仍需人工复核。
- 自动证据：Release 0 warning/0 error，Core `65/65`、Worker `228/228`、Playnite `319/376`（57 跳过），源码门禁通过。

## 2026-09-01 FLiNG 下载进度写入收口

- 下载进度回调由 `IProgress` 改为可等待异步回调；GameToolService 按百分比变化节流任务进度，避免大文件产生大量并发 SQLite 写入和未观察异常。
- 只有进度通知机制改变，FLiNG 域名校验、下载大小限制、临时文件清理、归档安全解包和工具绑定流程保持不变。
- 自动证据：Release 0 warning/0 error，Core `65/65`、Worker `227/227`、Playnite `319/376`（57 跳过），源码门禁通过；真实 FLiNG 下载速度、网络断开和取消场景仍需人工复核。

## 2026-09-01 公共 IPC 入口的退出期保护

- 插件公共 `RequestAsync` 现在在 `lifetimeCancellation` 取消后返回取消任务；页面操作、右键快捷操作和其他异步续体即使跨过多个 await，也不会在 Playnite 关闭阶段创建新的 Worker 请求。
- 这是 STAB-011 的第二道边界：启动/游戏事件/任务轮询/目录同步有自己的退出检查，公共 IPC 入口负责拦截未被这些专用路径覆盖的调用方。已发出的单个 IPC 不强行中断，晚返回结果仍由各自流程处理或取消。
- 自动证据：Release 0 warning/0 error，Core `65/65`、Worker `226/226`、Playnite `319/376`（57 跳过），源码门禁通过；真实宿主关闭中操作、Worker 重启和 Dispatcher 关闭需人工复核。

## 2026-09-01 Playnite 退出阶段后台生命周期收口

- 应用停止时先取消插件生命周期并停止任务通知计时器，再停止插件自己启动的 Worker；退出后的 Playnite 回调、Fire-and-forget、游戏会话写入、任务长轮询和目录同步不会再创建新 IPC 或 UI 回写。
- 目录同步的信号等待、任务通知的 IPC 返回和终态任务逐项处理均有生命周期检查；正在进行的单个 IPC 请求不能被现有客户端强行中断，但其晚返回结果不会继续驱动退出期状态。
- 自动证据：Release 0 warning/0 error，Core `65/65`、Worker `226/226`、Playnite `319/376`（57 跳过），源码门禁通过。需要人工验证：真实 Playnite 退出/重启、Worker 正在忙时关闭宿主、Dispatcher 关闭竞态。

## 2026-09-01 异步操作返回上下文保护

- 媒体/存档元数据保存会捕获原游戏、原条目和请求时编辑值；返回期间切换游戏或条目时，不会更新当前新列表、清理新编辑器的 dirty 状态或覆盖媒体摘要。
- 重新归类、备份、校验、恢复准备、策略模板、比较/保留预览和进程映射等异步流程的提示与回写均绑定原始上下文；新增的请求只会更新原请求仍可见的页面。
- 自动证据：Release 0 warning/0 error，Core `65/65`、Worker `226/226`、Playnite `318/375`（57 跳过），源码门禁通过。真实宿主快速切换和输入保持仍需人工回归。

## 2026-09-01 详情编辑草稿与游戏摘要刷新保护

- 存档备注/锁定、媒体备注/收藏现在有独立 dirty 状态；详情刷新重新绑定同一条目时保留用户未保存的字段，不会被旧 DTO 覆盖，其他未编辑字段仍会同步最新服务端值。
- 新条目、清空选择和成功保存按预期重置 dirty 状态；媒体批量收藏/备注只清除本次实际更新的对应字段。游戏策略未保存编辑也会在快照刷新时保留。
- 游戏选择器同一条目更新会向 Dashboard/生产壳层转发 `SelectedGame` 通知，标题和平台摘要可随快照更新。
- 策略保存请求会捕获原游戏的 ID、名称和策略副本；保存期间切换游戏不会把基线或成功提示写到新选中的游戏。
- 需人工验证：真实 Playnite 中打开存档/媒体详情或备份策略，编辑输入后触发自动刷新、任务完成刷新和页面切换，确认输入保持且保存后能回读服务端值。

## 2026-09-01 媒体收件箱与详情刷新一致性

- 媒体收件箱切换现在保留最后一次模式请求；若切换发生在其他操作进行期间，会在 `RunAsync` 结束后补加载。请求响应通过模式代际号校验，旧响应不会覆盖当前收件箱集合。
- 收件箱媒体、目标游戏、存档版本和当前游戏媒体在异步刷新后保留当前 ID 选择，找不到原条目时才回退第一项；“已忽略”模式的刷新会实际读取忽略列表。
- Media Inbox 新增 Worker 离线状态面板，离线时不再显示与状态冲突的空列表文案。自动证据：Release 构建 0 warning/0 error，Core `65/65`、Worker `226/226`、Playnite `318/375`（57 跳过），源码/WPF 门禁和 Render QA 均通过；真实 Playnite 快速切换、Worker 重启和 DPI 仍需人工回归。

## 2026-09-01 修改器目录加载状态反馈交接

- 修改器中心的 FLiNG 搜索结果、目录同步和可下载版本读取现在分别有真实加载状态；结果为空时只有在请求完成后才显示空状态，加载期间显示共享加载面板。
- `DashboardViewModel` 的命令、自动读取版本、下载绑定和错误传播未改变；`TrainerCenterView.xaml` 只补充状态呈现与空态条件，未改变页面导航、列表虚拟化或数据契约。
- 自动证据：Release 构建 `0 warning/0 error`，Core `65/65`、Worker `226/226`、Playnite `313/370`（57 跳过），源码校验通过，WPF `0 error/18 warning/172 info`，RenderHarness 双主题/多尺寸/Resize/Shell QA `render-qa OK`。用户已确认当前版本能编译并正常运行于 Playnite；本轮新增状态尚未由 Agent 在真实宿主重新操作，仍需随发布包做一次人工观察。

## 2026-09-01 修改器版本读取竞态与逐项操作交接

- FLiNG 目录结果行的“读取版本”按钮现在带有当前行作为参数；非选中行也会正确切换选择并读取对应版本。
- 版本请求增加代际号、目录 ID 校验和忙碌期间最新选择排队；旧的慢响应只结束自身加载状态，不再写入新选择的 `TrainerReleases`。全局命令保护、下载校验、安全解压和绑定行为未变。
- 自动证据：Release 构建 `0 warning/0 error`，Core `65/65`、Worker `226/226`、Playnite `314/371`（57 跳过），源码校验通过，WPF `0 error/18 warning/172 info`，RenderHarness 双主题/多尺寸/Resize/Shell QA `render-qa OK`。用户已确认当前版本可在 Playnite 正常运行；本轮新增竞态场景仍需在实际宿主中快速切换目录项观察一次。

## 2026-09-01 IPC 媒体边界与任务通知缓存优化

- 当前游戏媒体 `ListMedia` 在 Worker 分发层新增 1–1000 条页大小夹限，避免未来异常请求绕过 IPC 响应边界；现有 UI 的 1000 条读取上限不变，Inbox/Ignored 的 500 条分页规则不变。
- 任务通知去重使用容量 4096 的线程安全 FIFO 集合，替代插件生命周期无界的任务 ID 字典；淘汰旧 ID 只影响极少数历史 feed 重放的重复提示风险，不影响任务、备份或媒体数据。
- 已完成 Release 构建（`0 warning/0 error`）、全量测试：Core `65/65`、Worker `226/226`、Playnite `312/369`（57 跳过）；源码校验通过，WPF 静态审计为 `0 error/18 warning/172 info`。真实 Playnite/DPI/宿主重启和 Rclone 仍是人工验证边界。

## 2026-09-01 FLiNG 目录解析收口

- FLiNG 在线目录和详情页已使用独立纯解析方法，支持绝对、相对和协议相对链接；统一 HTML 解码、HTTPS/FLiNG 主域边界、预期路径校验和规范 URL 去重。
- 目录链接移除追踪查询参数，下载链接保留查询参数但去除 fragment；详情页解析以实际页面 URL 为相对链接基准。外站链接不会进入缓存或下载列表。
- Worker `222/222`、Core `65/65`、Playnite `310/367`（57 跳过）及源码门禁通过。真实 FLiNG 站点、下载、安全软件和 Playnite 宿主仍需人工回归。

## 2026-08-26 UI-335 完整圆角交接

- 标题栏 `GscRedesignHeaderSurface` 已从仅顶部圆角改为四角 18 DIP 圆角、完整 1 DIP 描边和 `ClipToBounds=True`；普通页面共享卡片 `GscRedesignSectionCard` 也统一启用裁剪，避免内部背景造成方形角泄漏。
- 变更只影响共享视觉容器，不改变标题/副标题、顶部按钮命令、Binding、页面数据、滚动或虚拟化；对应源码回归断言已更新。
- 自动证据：源码校验、XAML 19/19、WPF 静态审计、Release 0 warning/0 error，Core `59/59`、Worker `210/210`、Playnite `310/367`（57 跳过），`.tmp/ui-qa-rounded-surfaces-v1/render-qa-report.txt` 为 `render-qa OK`。
- RenderHarness 只覆盖页面内容，不覆盖外层 Shell 标题栏；真实 Playnite、DPI 和宿主截图仍未运行，Phase 4 按用户要求跳过。

## 2026-08-26 UI-334 按钮状态层覆盖交接

- 生产共享 `GscWpfUiButton` 已改为整按钮状态覆盖：`ButtonChrome` 不再承载内容 Padding，Hover/Pressed 层覆盖整个圆角表面，内容间距由 `ContentPresenter Margin={TemplateBinding Padding}` 保持。
- 键盘焦点新增整按钮 `FocusOverlay`，Primary 使用 on-accent 覆盖资源，并保留共享 `GscSharedFocusVisual` 焦点环；命令、Binding、按钮尺寸、文字省略、滚动、虚拟化和主题契约未改。
- 自动证据：`validate-source.py`、XAML 检查、WPF 静态审计、Release 构建/全量测试通过；Core `59/59`、Worker `210/210`、Playnite `310/367`（57 跳过）、`.tmp/ui-qa-button-focus-v1/render-qa-report.txt` 为 `render-qa OK`。
- RenderHarness 不模拟真实 Playnite 外壳或所有键盘交互状态；真实 Playnite、DPI 和焦点实机验收未运行。Phase 4 仍按用户要求跳过。

## 2026-08-26 UI-333 生产标题栏圆角交接

- `AcrylicProductionShellView.HeaderSurface` 已接入共享 `GscRedesignHeaderSurface`；顶部两角为 16 DIP 圆角并裁剪内部材质，底边保持直线，避免标题栏与 PageHost 之间出现尖锐矩形边界或额外布局缝隙。
- 标题、副标题、游戏选择器、刷新/同步/备份命令和响应式布局未变；变更只涉及 `Redesign.xaml`、`AcrylicProductionShellView.xaml` 与 `ProductionShellChromeSourceTests.cs`。
- 自动证据：Release 0 warning/0 error，Core `59/59`、Worker `210/210`、Playnite `310/367`（57 跳过），源码/XAML/WPF 门禁通过，`.tmp/ui-qa-header-corner-v1/render-qa-report.txt` 为双主题、多尺寸、Resize `render-qa OK`。
- 页面 RenderHarness 不包含外层 Shell 标题栏，因此未将页面 PNG 误称为标题栏实机截图；真实 Playnite 未运行，Phase 4 仍为用户明确跳过的人工验证项。

## 2026-08-26 UI-332 按钮组与页面环境材质交接

- 生产页面已收口本轮用户指出的三类视觉问题：远端恢复按钮使用 `GscWpfUiRemoteRestoreButton`，媒体中心当前游戏媒体两处批量按钮使用 `GscWpfUiMediaBatchButton`，修改器下载版本提示使用中性 `GscDiagnosticHintBubble` 与 `GscDiagnosticHintText`。
- `AmbientMaterialLayer` 的页面环境材质现在由共享圆角 `MaterialChrome` 裁剪，默认页面圆角 16 DIP；`AcrylicProductionShellView` 的 Shell 环境层显式为 0 DIP，避免在完整 Shell 内形成额外内凹边界。
- 自动证据：Release 0 warning/0 error，Core `59/59`、Worker `210/210`、Playnite `309/366`（57 跳过），源码/XAML/WPF 门禁通过，`.tmp/ui-qa-button-material-v1/render-qa-report.txt` 为双主题、多尺寸、Resize `render-qa OK`。
- 本轮只改共享样式、控件材质容器和对应页面样式引用，未改命令、Binding、数据契约、滚动、虚拟化或动画。真实 Playnite 未运行，Phase 4 仍为用户明确跳过的人工验证项。

## 2026-08-26 STAB-007 性能测量交接

- Worker 全量数据规模已测完：2,000 游戏、20,000 备份、10,000 任务、30,000 媒体、500 工具，约 3m03s，managedGrowth 0 MiB、handles +0、threads +0；2,000 游戏合成集合基准为 55/2/15ms SetItems、215/196ms 搜索、1/0ms ReplaceAll。
- Blur 20/78/100 DIP 已加入回归；没有改默认 Blur、动画、滚动模型、虚拟化或增加效果。离屏 Render QA 253 样本平均 219.01ms，报告 `render-qa OK`。
- 这些是 Worker/SQLite、合成集合或离屏数据。真实 Playnite 初次打开/切页/侧栏/主题/背景内存/DPI/大库滚动仍是 `MANUAL QA REQUIRED`，Phase 4 依用户要求跳过，禁止用离屏证据替代。

## 2026-08-26 STAB-006 页面树与响应式协调交接

- Phase 5 已完成静态双树治理：可见业务实例来自 `ProductionShellView.PageHost`；旧 Dashboard 页面树保留为兼容面，详情见 `docs/ai/WORKSPACE_TREE_INVENTORY.md`。业务导航、搜索焦点、维护定位、任务动画和生产页面布局不得重新引用旧实例。
- Phase 6 已将现有响应式宽高状态集中到 `ResponsiveLayoutCoordinator`，保持所有既有断点和尺寸；`AcrylicProductionShellView` 的导航、侧栏切换、Resize 延迟回调都统一调用 `ApplyResponsiveLayout`。不可在未测量前修改 Blur、动画、滚动模型或虚拟化。
- 自动证据：Release 0 warning/0 error，Core 59/59、Worker 210/210、Playnite 309/366（57 跳过），源校验/XAML/WPF 门禁通过，`.tmp/phase6-responsive-coordinator-render/render-qa-report.txt` 为双主题与连续 Resize `render-qa OK`。
- Phase 4 由用户明确跳过；不要把上述离屏证据称为真实 Playnite 验收。宿主安装、DPI、键盘焦点、Worker 重启/日志、性能和旧树删除安全性仍是 `MANUAL QA REQUIRED`。

## 2026-08-26 STAB-005 媒体 Inbox 响应超限交接

- 用户实机两端均为 `0.6.70.0`；Worker 在 `11:04:21` 报告 Named Pipe 响应超过 `4194304` 字节，Playnite 随后把 `MESSAGE_TOO_LARGE` 显示成“操作失败”。已确认根因是媒体 Inbox 列表旧路径一次请求 5000 条，用户库有 4615 条 Inbox 媒体。
- 当前修复在 Worker 端把未分配/已忽略媒体限制为 500 条并支持 `Offset`，在 Playnite 端分页读取最多 5000 条；数据库查询按 `captured_utc DESC, media_id DESC` 稳定排序。服务端超限日志新增 `RequestId`、`Type`、`ResponseBytes`、`PayloadBytes`，不能简单调高 4 MiB 上限。
- 代码没有改 XAML、布局、动画或页面数据应用路径；自动验证为 Core `59/59`、Worker `210/210`、Playnite `302/359`（57 跳过）、Release 0 warning/0 error、RenderHarness `render-qa OK`。WPF 静态审计仍为 0 error/18 warning/172 info。
- 用户明确允许本修复跳过 Phase 4。当前未覆盖真实 Playnite 安装后的补丁 Worker 验收：只读探针对现有 Worker 的 500 条请求测得约 745990 字节，隔离补丁 Worker 因全局互斥未启动成功；没有停止用户 Worker 或修改其数据。
- 下一步：用本次构建产物安装到用户实际扩展目录后，打开 Media Inbox/Ignore 页面，确认没有 IPC 超限；若仍有超限，按新增 Worker 日志中的 `Type`、请求 ID和字节数继续定位其他接口。

## 2026-08-26 STAB-003A IPC 真实管道烟测补充

- 隔离 Release Worker 真实启动后，`4194778` 字节超限请求返回 `MESSAGE_TOO_LARGE`；同一 Named Pipe 连接的后续 `system.ping` 成功，证明超限行消费和连接复用路径可工作。
- 受限执行上下文的 Named Pipe 连接会 `Access is denied`，但提升权限后的同一隔离烟测通过；报告为 `.tmp/phase3-ipc-runtime-escalated/runtime-smoke-report.txt`。
- 仅测试结束时强制停止临时 Worker，退出码 `-1` 属于清理动作；没有修改 Playnite 用户数据。Phase 4 真实 Playnite 矩阵仍被缺少宿主环境阻塞。

## 2026-08-26 STAB-004 真实 Playnite 验证阻塞

- 当前环境没有可执行的 Playnite、运行中的宿主或注册表安装入口；`D:\software\Playnite\Playnite\Playnite.DesktopApp.exe` 等历史候选路径也不存在。Phase 4 状态为 `BLOCKED_ENVIRONMENT`。
- 不要用离屏 RenderHarness、静态 WPF 审计或历史截图宣称真实宿主通过；也不要未经隔离证明运行 `real-host-audit.ps1`/`dev-install-run.ps1`，因为它们可能安装扩展、关闭宿主或写入用户目录。
- 下一步如果要完成真实宿主验收，需要用户提供隔离 Playnite 安装/便携目录、独立数据根、唯一进程边界和扩展目录；在用户已明确跳过 Phase 4 的前提下，Phase 5/6 的静态治理可以继续，但不能补写真实宿主通过结论。
- Phase 0–3 的自动验证已提交推送，最新 IPC commit 为 `9d53b1d`；Phase 4 没有源码改动。

## 2026-08-26 STAB-003 IPC 健壮性交接

- 三个 Named Pipe 方向共用 Contracts 的有状态 `BoundedIpcLineReader`；按 4 MiB 字节上限消费消息，超限返回/发送 `MESSAGE_TOO_LARGE`，服务端响应和事件写出同样先检查，不能恢复 `ReadLineAsync` 后再检查。
- 读取器必须按连接实例化以保存 4 KiB 缓冲剩余数据；否则一块内多条消息会被错误丢弃。请求服务端并行槽位 32、事件服务端槽位 8，`CurrentUserOnly`、管道名、协议版本和消息类型保持原值。
- Playnite 请求超时仍向上暴露 `TimeoutException("Worker response timed out.")`；事件取消、JSON 错误和客户端断开继续走原有重连/忽略路径。
- 本轮验证：IPC 定向 `3/3`，全量 Core `59/59`、Worker `201/201`、Playnite `302/359`（57 跳过）、Release 0 warning/0 error，WPF 0 error/18 warning/172 info，最终 RenderHarness `.tmp/phase3-ipc-boundary-render-final/render-qa-report.txt` 为 `render-qa OK`。
- 下一阶段是 Phase 4 真实 Playnite 隔离宿主验证；若环境无法完成，必须明确写 `MANUAL QA REQUIRED` 或 `BLOCKED_ENVIRONMENT`，不能把离屏 RenderHarness 当作宿主证据。

## 2026-08-26 STAB-002 外部进程输出交接

- `ExternalProcessRunner` 每个 stdout/stderr 流最多保留 4 MiB；超限后继续消费但丢弃，返回 `PROCESS_OUTPUT_LIMIT_EXCEEDED`，不能恢复无界 `ReadToEndAsync`。
- `ProcessResult` 保留退出码、标准输出/错误和稳定 `ErrorCode`；`PROCESS_TIMED_OUT` 仍返回超时结果，调用者取消仍抛出取消异常。普通进程失败仍使用原退出码和输出。
- `RcloneClient.RunSafeAsync` 不再有 `workingDirectory` 参数，始终传 null standardInput 给 Runner；不要改变 Runner 既有的可执行文件目录或 Rclone 参数顺序。
- 本轮验证：定向 Worker `6/6`、全量 Core `59/59`、Worker `201/201`、Playnite `302/359`（57 跳过）、Release 0 warning/0 error，WPF 0 error/18 warning/172 info，RenderHarness `render-qa OK`。
- 下一阶段处理 Named Pipe 统一消息上限和边界读取；真实 Rclone/Ludusavi 大输出、Worker 重启和 Playnite 宿主日志仍需人工复核。

## 2026-08-26 STAB-001 Dashboard 事件生命周期交接

- `DashboardViewModel` 的 `PlayniteGameStarted` 订阅由 `PlayniteGameStartedSubscription` 持有，必须通过 `StartPlayniteGameStartedSubscription` / `StopPlayniteGameStartedSubscription` 管理；生产 View 的 Loaded/Unloaded 已成对调用，禁止重新放回构造函数永久订阅。
- handler 在排队 UI 调度前后都检查订阅状态，卸载后的旧回调不能写入页面；pending auto-select 的原有时序和游戏列表未到达时的保留行为不变。
- 本轮没有改 XAML 或页面视觉；验证为生命周期定向 `6/6`、Release Core `59/59`、Worker `201/201`、Playnite `302/359`（57 跳过）、0 warning/0 error，RenderHarness `render-qa OK`。
- 下一阶段应处理 `ExternalProcessRunner` 的 stdout/stderr 有限累积及 `RcloneClient` 参数语义；完成后继续同步本文件、`docs/ai/WORKLOG.md` 与 `docs/ai/PROJECT_MEMORY.md`。
- 真实 Playnite 重载、事件触发、DPI、主题和焦点仍是 `MANUAL QA REQUIRED`，不能用离屏渲染报告代替。

## 2026-08-25 UI-331 共享控件与流畅度交接

- 共享生产 TextBox 模板现在把样式 Padding 应用到外层 Chrome；ContentHost 必须保持零 Margin/零 Padding，避免输入文字高度和左右内边距因模板重复计算而漂移。TextBox/ComboBox 保留 `SnapsToDevicePixels` 与 `UseLayoutRounding`。
- ComboBox 选中内容和下拉项统一传递字体族、字号、字重，并使用明确的左对齐；相邻筛选下拉框的 Items、Binding、默认值和命令未改动。
- 侧栏边界控制仍是无文字、32×32 的底部集成控件。宽度 270↔72 DIP 由 210ms `GridLengthAnimation` 驱动；内容层的 190ms 淡入/4 DIP 位移只用于平滑视觉过渡。完成、非动画布局和卸载必须停止/清理动画。
- 游戏背景仍只有 Shell 的一个静态图片层，现增加 `BitmapCache`；不要为了追求“玻璃感”给卡片、文字、表格、列表和 ScrollViewer 挂 BlurEffect。
- 设置路径输入校验已改为 Background Dispatcher 合并通知，避免 `File.Exists` 在每个键盘字符同步执行；验证语义不变。
- 证据：定向 124/39、全量 Playnite 297/57、Release 0/0、WPF 0 error/18 warning/172 info，`.tmp/ui-qa-polish-v1/render-qa-report.txt` 为 `render-qa OK`。真实 Playnite 宿主仍需复核实际点击动画、DPI、帧率与焦点。

## 2026-08-25 UI-330 毛玻璃强度交接

- `GlassStrengthSlider` 仍是 20–100；`AdaptiveThemePalette.BlurRadiusForStrength` 现在直接把百分比映射为 Blur DIP：20→20、默认 78→78、100→100。后续不要再使用 12–34 或 16–34 的压缩范围，否则滑块会再次出现“100 像 20”的观感。
- 真实 Blur 只允许出现在游戏背景图片层和设置页环境层；卡片、文字、表格、列表和滚动区域继续使用共享半透明材质，不挂 BlurEffect。
- 保留关闭玻璃、高对比度、无图/关闭游戏背景跟随时的 null/透明/不透明回退，并且强度变化不能启动新的背景解码。
- 验证证据：WPF 资源定向 119 通过/39 跳过，全量 Playnite 297 通过/57 跳过，Release 0 警告/0 错误，`.tmp/ui-qa-glass-strength-v1/render-qa-report.txt` 为 `render-qa OK`。真实 Playnite 的 100% 帧率、DPI 和视觉观感仍需安装后确认。

## 2026-08-25 UI-329 刷新与布局流畅度交接

- Dashboard 自动刷新不得在选中游戏未变化时重复刷新 Icon 或 Background；`EnsureSelectedGameBackgroundLoaded` 只在页面重新显示且当前背景确实缺失时做一次恢复。不要把背景解码放回普通快照轮询。
- 背景取色只读取五个 1×1 像素，不能恢复整张位图的临时 `byte[]` 复制；如果未来增加采样点，应先评估切换游戏时的分配、GC 和取消语义。
- 生产壳连续 `SizeChanged` 通过 `DispatcherPriority.Render` 合并，筛选平台集合变化通过单个 `Loaded` 调度恢复默认值；不要在拖拽窗口时同步执行全页面响应式重排，也不要重复排队 `DataBind + Loaded` 两套同一恢复工作。
- 本轮保留真实命令、绑定、滚动/虚拟化、侧栏宽度动画和主题材质；卸载时必须清理 pending 标记。`render-qa` 通过不等于 Playnite 宿主已完成帧率、内存、DPI 和实际缩放验收。
- 验证证据：定向 Playnite 26/26，全量 297 通过/57 跳过，Release 0 警告/0 错误，WPF 0 error/18 warning/172 info，`.tmp/ui-qa-performance-v1/render-qa-report.txt` 为 `render-qa OK`。

## 2026-08-25 UI-328 游戏背景跟随开关交接

- 设置新增 `FollowSelectedGameBackground`，默认开启，位于“外观与动态效果”。关闭后不是只隐藏图片：ViewModel 会取消封面加载、清理已解码图像和采样 Brush，重新开启后只加载当前选中游戏。
- 游戏背景资源入口必须保留共享回退：只有开关开启、采样材质存在、毛玻璃可用且非高对比度时才显示底层图片、tint 和 BlurEffect；其他情况恢复主题/Demo 中性材质。
- `AmbientMaterialLayer` 必须以 `UseSelectedGameBackground && HasSelectedGameBackgroundAmbientMaterial` 决定隐藏主题洗色，避免关闭开关后主题环境光消失。设置保存通过既有 `EndEdit -> NotifyVisualSettingsChanged` 链路通知已打开的 Dashboard。
- 验证证据：`validate-source.py`、`check-xaml.ps1`、WPF 静态审查 0 error/18 warning/172 info、Release 0 warning/0 error、Playnite 296 通过/57 跳过、`.tmp/ui-qa-game-background-v1/render-qa-report.txt` 为 `render-qa OK`。真实 Playnite 重载后仍需确认开关即时生效、浅色/深色/Follow、高对比度、DPI 和性能。

## 2026-08-25 UI-327 字体清晰度与维护提示交接

- 共享生产文字排版已从 `Display` 改为 `Ideal`，由生产壳、首页、设置页和 DataGrid 样式统一传递；继续保留 `ClearType`、`Fixed` hinting、`UseLayoutRounding` 和 `SnapsToDevicePixels`。这是针对用户反馈的大号中文笔画像素感，不是把文字做模糊。
- 修改器确认导入页的“主程序”标签和 ComboBox 已放在同一 Grid 行；按钮仍绑定原确认/取消命令。后续若再做窄宽度适配，应保持该标签/输入行的基线关系，不要恢复纵向 StackPanel。
- 维护中心摘要采用 `GscDiagnosticHintBubble` / `GscDiagnosticHintText`，用于诊断、存储、保留策略、镜像和任务摘要；气泡使用信息色低透明度，正文常规次级文字，Severity pill 的状态语义不变。
- 验证证据：`.tmp/ui-qa-font-bubbles-v1/render-qa-report.txt` 为 `render-qa OK`；Release 0 警告/0 错误，Core 59、Worker 199、Playnite 295 通过/57 跳过。未在真实 Playnite 宿主执行本轮人工 DPI/字体像素检查。

## 2026-08-25 UI-326 设置窗口与侧栏折叠交互交接

- 设置页根控件已移除 `MinWidth=1180/MinHeight=760`，避免窗口缩小时被 UserControl 强行撑大。`GameSaveCenterSettingsView.OnLoaded` 会通过 `Window.GetWindow(this)` 对真实宿主执行一次 `EnsureHostWindowSize`：优先约 1280×840，受当前工作区上限约束，设置 `SizeToContent=Manual` 和 Stretch 对齐；后续缩放不再强制回弹，继续由 `ApplyResponsiveLayout` 处理紧凑模式。没有 owner Window 的 RenderHarness 不会触发该逻辑。
- 生产壳侧栏控件位于 `SidebarLayout` 底部行 `SidebarCollapseArea`，共享样式为 `AcrylicSidebarBoundaryButton`。它是透明 32×32 边界控制，不带“收起侧栏”等文字、不覆盖导航内容；`‹` 表示收起，`›` 表示展开，悬停/按下只显示轻量 tint。
- `GridLengthAnimation` 位于 `src/GameSaveCenter.Playnite/Controls/GridLengthAnimation.cs`，210ms、`CubicEase.EaseOut`，驱动 `SidebarColumn.Width` 从 270 到 72 DIP 或反向变化。动画关闭时保持同步直接切换；页面通过 Grid 列变化自动跟随，不使用 Canvas。
- `GameSaveCenterSettings.SidebarCollapsed` 由 `DashboardView` 注入生产壳读写并立即保存，首次加载恢复用户选择。命令、导航、设置入口、主题切换、滚动与虚拟化未改动。
- 本轮验证：Debug 隔离构建 0 warning/0 error；Core 59/59、Worker 199/199、Playnite 295 通过/57 跳过；`validate-source.py`、`check-xaml.ps1`、WPF 静态审查（0 error/18 warning/172 info）、`git diff --check` 和 `.tmp/ui-qa-sidebar-boundary-v1/render-qa-report.txt`（`render-qa OK`）均通过。真实 Playnite 宿主尺寸、动画点击、DPI/焦点仍需重载扩展后复核。

## 2026-08-25 UI-319 Dune 浅色主题 FollowPlaynite 修复

- 用户当前 Playnite Desktop 主题是 Dune。该主题通过 `ThemeDarkStyle=False` 表示浅色，并同时发布 `WindowBackgroundBrush`/`DarkWindowBackgroundBrush`；Follow 不能只依赖默认主题的历史 `WindowBackgourndBrush`。
- `AdaptiveThemePaletteFactory` 现在先读取 `ThemeDarkStyle`，按主题模式选择浅色或深色窗口资源；无该标志的主题才使用背景与 `TextBrush`/`TextBrushDark` 的一致性推断。正确拼写的 `WindowBackgroundBrush` 排在历史拼写之前。
- 本轮没有改变强制浅色/深色、设置绑定、保存按钮、命令、虚拟化或 Playnite 兼容性；新增回归测试锁定 Dune 两套资源同时存在时的 Follow 结果。
- UI-319 验证：定向 2/2，Playnite 290 通过/57 跳过/0 失败，Release 0 警告/0 错误，`validate-source.py` 和 `check-xaml.ps1` 通过。真实 Playnite 重载后的 Follow 浅色设置窗口仍需人工确认，不能把离屏测试写成宿主像素验收。

## 2026-08-25 UI-318 Follow Playnite 主题读取修复

- Playnite Desktop 的实际背景资源键是 `WindowBackgourndBrush`；Follow 解析已加入该键，并兼容其他常见背景键。设置窗口还会显式检查 owner Window 和 `Application.Current` 资源。
- 当主题只提供文本资源时，使用 `TextBrush`/`TextBrushDark` 明暗推断安全背景；强制浅色/深色行为保持不变。
- 设置页主题 ComboBox 会先显式写回 `CurrentSettings.ThemeMode` 再刷新材质，避免从深色切换 Follow 时使用旧枚举值。
- UI-318 已通过定向/全量测试、Release 构建、源码/XAML/WPF 门禁和 `.tmp/ui-qa-settings-follow-v8/render-qa-report.txt`；真实 Playnite 重启后需确认实际浅色宿主窗口。

## 2026-08-25 UI-317 壳体圆角玻璃与 Follow Playnite 主题

- 生产壳导航当前由 `SidebarSurface` 的真实圆角 Border 裁切，材质使用动态 `GscSidebarMaterialBrush`；页脚由 `FooterSurface` 提供四边圆角玻璃面。不要恢复让内部 Grid 直接绘制导航背景的结构。
- 设置页不跟随游戏图片，但使用 `SettingsAmbientLayer` 的环境渐变/BlurEffect 和低 alpha 的外壳、分类栏、卡片、内容材质；BlurEffect 不得提升到文字、表格、列表或滚动区域。
- Follow Playnite 解析优先级已调整为宿主发布的背景资源，再回退视觉树背景，避免插件自身深色回退资源抢先把 Playnite 浅色主题识别成深色。
- RenderHarness 设置页 Light/Dark 审计必须经过 `ApplyThemeForAudit`；最新证据为 `.tmp/ui-qa-settings-glass-v7/render-qa-report.txt`，真实宿主逐像素、DPI、高对比度仍待人工确认。

## 2026-08-25 UI-314 当前交接：游戏背景真实模糊

- 当前游戏背景仍由 Shell 的唯一跨壳 `ImageBrush` 绘制；UI-314 只在背景实际加载时给这个矩形挂 `GscGameBackgroundEffect`，不要把 BlurEffect 加到卡片、文字、页面根、列表或滚动器。
- `GscGameBackgroundEffect` 由 `AdaptiveThemePalette` 按毛玻璃强度直接生成 20–100 DIP 的冻结 `BlurEffect`，默认设置为 78 DIP，`RenderingBias=Performance`。图片仍保持 `UniformToFill` 居中、不平铺和原有透明度/tint。
- 没有背景图、关闭毛玻璃、高对比度时必须返回 null；XAML 的 `HasSelectedGameBackgroundAmbientMaterial` DataTrigger 负责避免无图时保留大面积效果视觉。
- UI-314 已通过源码门禁、WPF 静态审查、Release 全量测试和多主题多尺寸 `render-qa OK`。真实 Playnite 没有可控窗口，安装后需人工确认模糊强度和帧率。

## 2026-08-24 UI-313 当前交接：背景图单层居中与底部接缝

- 截图中的矩形接缝来自重复材质层：生产 `ShellAmbientMaterialLayer` 与各工作区内部的 `AmbientMaterialLayer` 不能同时绘制 `SelectedGameBackgroundAmbientBrush`。共享控件的 `UseSelectedGameBackground` 默认关闭，只有 Shell 实例设置为 `True`。
- Shell 真实背景使用一个跨两行两列的 `ImageBrush`，显式 `Stretch="UniformToFill"`、`AlignmentX/Y="Center"`、`TileMode="None"`；图片和 tint 均覆盖页脚行。不要恢复多个 Image/ImageBrush 或页面局部游戏背景层。
- 页面局部环境层在有游戏背景时保持透明，因此游戏颜色仍能从 Shell 单一环境层透出，同时不再叠加固定绿色宽域渐变。无背景、关闭毛玻璃和高对比度的主题回退保持不变。
- UI-313 已通过源码门禁、WPF 静态审查、Release 全量测试和多主题多尺寸 `render-qa OK`；当前 Playnite 没有可控窗口，真实宿主仍需安装后切换不同宽高比游戏背景人工确认。

## 2026-08-24 UI-312 当前交接：卡片表面与游戏背景自适应

- Today 卡必须直接使用共享 `GscRedesignSectionCard`，不要在卡片内部放整面背景 Border；此前的方形内层来自 WPF 圆角卡片里嵌套第二层材质。
- 背景链路现在由 `PlayniteGameBackgroundProvider.LoadVisualAsync` 同时返回真实图片和图片采样环境色。`AmbientMaterialLayer` 有真实背景时隐藏固定主题宽域洗色，使用采样色；无背景时才使用主题默认宽域材质，因此不能重新把 success 绿色固定叠到所有游戏上。
- 背景图仍位于 Shell 底层，卡片/导航/文字保持功能层级；深色/浅色图片透明度为 0.48/0.40，主题 tint alpha 为 0x52/0x66。远程下载 5 秒、12 MB、后台解码、缓存、取消和 generation 约束必须保留。
- UI-312 已通过源码门禁、WPF 静态审查、Release 构建、背景提供器 5 项定向测试和多主题多尺寸 `render-qa OK`。当前 Playnite 进程没有可控窗口，因此下一次安装后需要人工切换两款有不同背景图的游戏确认真实宿主颜色跟随。

## 当前 UI 重构方向（2026-08-17）

用户已明确要求对页面进行完全大改。现在允许重做页面布局、信息架构、导航容器、Tab/Segmented 结构、控件类型、共享模板、滚动实现和视觉层级。下方 UI-221 及更早条目中的“不要恢复/不要替换/明确不迁移”是上一轮样板迁移的历史交接信息，不再构成页面结构或控件实现禁令。

新实现仍需以真实业务行为为底线：命令、Binding、数据、安全确认、错误/取消、可访问性、可扩展性能和 Playnite 兼容性不能被无意删除。如果新方案有意改变这些能力，应在当前阶段明确说明并配套测试；不得以“旧实现必须保留”为理由阻止整页重构。

## 2026-08-24 UI-311 当前交接：Today 圆角与游戏背景切换

- Today 卡内部宽域材质使用带 12 DIP 圆角的 Border；不要改回整面 Rectangle。外层 `ClipToBounds` 不能模拟 WPF 的圆角裁切。
- 背景切换链路仍是 `GamePickerViewModel.SelectedItem` → `DashboardViewModel.RefreshSelectedGameBackground` → `AcrylicProductionShellView` 的 `SelectedGameBackground` Binding。提供器优先读 Playnite 本地缓存/数据库文件，也支持当前选中游戏的 HTTP/HTTPS 背景直链；远程路径必须保持 5 秒超时、12 MB 上限、后台解码、取消和 generation 防串图。
- 毛玻璃本轮只做小幅增强，不增加 Shell/页面/表格/滚动器的 BlurEffect：背景图透明度和宽域渐变稍微提高，主题 tint 稍微变透明；关闭毛玻璃和高对比度仍必须回退到透明背景图层。
- 验证：`scripts/build.ps1 -Configuration Release` 完整通过（Core 59、Worker 199、Playnite 288 通过，57 跳过），XAML/source/WPF 门禁通过，`.tmp/ui-qa-current/render-qa-report.txt` 为 `render-qa OK`。本轮未在可识别 Playnite 宿主中实际切换游戏截图，不能把真实宿主背景切换宣称为已人工复核。

## 最新视觉优先级：Demo-first（2026-08-20）

后续迁移统一以 `GameSaveCenter.AcrylicFork/src/GameSaveCenter.Playnite/Design/` 下的 `DesignShellView.xaml`、`Pages/*.xaml`、`DesignTokens.xaml`、`DesignColorsLight.xaml`、`DesignColorsDark.xaml` 和 `DesignControls.xaml` 为主要且唯一视觉基准。Demo 与旧生产页面、UiLab、历史计划或 `wpf-apple-desktop-ui` 的通用 Apple-inspired 设计建议冲突时，以 Demo 的整体页面结构和视觉层级为准。

`wpf-apple-desktop-ui` 仅用于质量检查，不再限制 Demo-first 的页面选择；必须继续保护真实数据、命令、Binding、错误/取消/安全语义、虚拟化、可访问性和 Playnite 兼容性。当前游戏选框与现有滚动条系统按总目标保留，Demo 的 Mock 数据和演示行为不迁移。

## 2026-08-24 UI-310 当前游戏背景图环境材质

- 生产 Shell 通过 `DashboardViewModel.SelectedGameBackground` 使用 Playnite `Game.BackgroundImage` 的本地缓存/数据库文件；UI-311 已补齐当前选中游戏的受限 HTTP/HTTPS 异步下载，其他无法解析的引用仍回退主题默认背景。
- 背景图片低透明度绘制在 Shell 底层，之上仍是主题 tint、宽域多色材质、导航和页面卡片；它不是对整页加 BlurEffect。无背景图、关闭毛玻璃或高对比度时，`GscGameBackgroundOpacity` 必须为 0。
- 默认背景确实跟随当前主题：`AdaptiveThemePalette` 根据 Playnite 主题/用户的浅深色模式生成 `GscBackdropBrush`、背景 tint 和宽域材质。不要让某个游戏背景图替换主题文字/控件对比度。
- `PlayniteGameBackgroundProvider` 使用后台解码、1920 宽度上限、6 张缓存和取消/generation 保护；保持这些性能边界。
- UI-310 已完成构建、全量测试、WPF 静态检查和多主题多尺寸 render QA；本轮未强制关闭正在运行的 Playnite，因此真实宿主背景图显示需更新安装后复核。

## 2026-08-24 UI-309 全局宽域多色玻璃材质

- 主页 Today 卡片、Settings 环境层、兼容 Dashboard 和共享 `AmbientMaterialLayer` 统一使用 `GscAmbientWideWashBrush` 的整面矩形；不再恢复装饰性径向渐变、椭圆光斑或 `GscAmbientBlurEffect`。
- 宽域材质由动态 accent/info/teal/success/中性表面组成六段线性渐变，颜色在大范围内缓慢过渡；透明度受 `GlassStrength` 限制，关闭毛玻璃/高对比度必须透明。状态小圆点和图标语义色仍保留。
- 不要给 Shell、导航栏、页面根、表格、列表或滚动器挂大面积 `BlurEffect`；Playnite 嵌入视图没有安全的宿主桌面像素 backdrop blur，这里采用低成本整面渐变模拟。
- 本轮不改变真实命令、Binding、数据契约、虚拟化、滚动条或 Playnite 兼容性；源码/XAML/WPF 门禁、Release 构建、全量测试和多主题多尺寸 `render-qa` 均已通过，当前用户 Playnite 扩展已核对为 `0.6.70.0`。由于 Computer Use 当前只返回 `EmptyWindowAutomationPeer`，不要把本轮描述为真实宿主页面点击/截图已复核。

## 2026-08-24 UI-308 导航栏与宽域材质交接

- `AcrylicProductionShellView.xaml` 的 `SidebarLayout` 统一使用 `GscSidebarMaterialBrush`；标题/导航两个子 Border 必须保持透明，不能恢复成各自绘制材质，否则会造成渐变断层。
- 导航栏材质使用全区域对角线性渐变，不做透明边缘渐隐，不新增右侧硬边或光柱。它是 Playnite 嵌入视图下的低成本玻璃模拟，不是对宿主桌面像素的真正 backdrop blur。
- `AmbientMaterialLayer.xaml` 第一层为 `GscAmbientWideWashBrush` 的 `Rectangle`，用于大范围非圆形环境洗色；旧的固定椭圆实现已由 UI-309 移除。不要给 `SidebarLayout`、页面根、表格、列表或滚动器增加 BlurEffect。
- `AdaptiveThemePaletteFactory.ApplyMaterialResources` 必须在关闭毛玻璃/高对比度时提供透明宽域渐变；资源测试已锁定这一降级契约。
- UI-308 已完成 Release 构建、全量测试、多主题/多尺寸 render QA，并在真实 Playnite `0.6.70.0` 中查看了导航栏和首页；详情见 `docs/ai/WORKLOG.md`。

## 2026-08-24 UI-307 首页圆角与导航材质交接

- Today 卡片的装饰椭圆已全部移入卡片内部。不要依赖 `ClipToBounds` 裁切圆角，也不要恢复负边距光源；WPF 这里按矩形裁切，负边距会在圆角外产生直角色块。
- `AmbientMaterialLayer` 的 `ShowLeftGlow` 默认开启，生产 Shell 必须关闭它；页面局部环境光仍可保留。这样主内容列起点不会出现竖向光柱。
- 导航栏必须使用完整宽度的中性 `GscSidebarMaterialBrush`，当前不使用 `SidebarSeamMaterial`、`GscSidebarSeamBrush`、透明渐隐边缘或右侧 1 DIP 硬边。导航玻璃感由整栏低对比度材质提供，不能通过边缘高亮增强。
- 本轮没有改变命令、Binding、页面结构、DataGrid/ListBox 虚拟化、滚动条或安全语义。真实 Playnite `0.6.70.0` 已安装并截图复核：Today 左上角圆角正常、导航分界无光柱。
- 证据：Release 构建 0 warning/0 error；Core 59/59、Worker 199/199、Playnite 283/283（57 跳过）；多主题、多尺寸、滚动及 resize transition `render-qa OK`。

## 2026-08-24 UI-306 历史记录（已由 UI-307 覆盖）：Shell 毛玻璃与导航栏硬边

- `AcrylicProductionShellView.xaml` 的 `ShellAmbientMaterialLayer` 使用正向 Z 层级覆盖整个 Shell 两列，位于导航/页面表面下方；不要放回负 Z 层，也不要把 Blur 挂到根 Shell 或页面内容。
- 导航栏两个表面不再绘制右侧 1 DIP 边框；`SidebarSeamMaterial` 是 42 DIP、不可交互的渐隐过渡，`GscSidebarSeamBrush` 和 `GscShellAmbientOpacity` 由运行时主题/毛玻璃强度提供。不要恢复硬直线或新建布局列。
- 本轮未改动真实导航命令、页面 Binding、DataGrid/ListBox 虚拟化、滚动条或页面布局契约。共享固定装饰 Blur 仍为性能模式，关闭毛玻璃/高对比度必须保持真实 null/0 降级。
- 证据：`artifacts/ui-qa/shell-glass-final2/render-qa-report.txt` 为 `render-qa OK`；Release 构建 0 warning/0 error，Core 59、Worker 199、Playnite 283 通过，57 跳过；生产扩展 `0.6.70.0` 已安装并在真实 Playnite 查看首页/存档/修改器页面。若沙箱中重现安装 Access denied，应使用与 Playnite 相同用户权限的安装流程，不要据此判断旧 DLL 的视觉结果。

## 2026-08-24 UI-305 任务表格底部滚动交接

- 用户截图中的任务表最后一行被水平滚动条覆盖。生产 `TaskDataGrid` 当前仅增加 `Padding="0,0,0,12"` 底部安全区，不能关闭横向滚动、不能关闭共享 `DataGridStarFill`，也不要改动真实列宽、Binding、命令、Item scrolling 或 Recycling。
- RenderHarness 使用 500 条任务数据执行到底→回顶→再到底→中段→回顶；底部检查比较最后一行底部和水平滚动条顶部，同时检查回拉后无空白/无效行。修改滚动链路时必须保留该方向性探针。
- 当前离屏证据在 `artifacts/ui-qa/task-bottom-final/render-qa-report.txt`，结果为 `render-qa OK`；这不等同真实 Playnite 宿主逐像素验收，用户仍需在本机滚到任务列表底部确认最后一行完整。

## 2026-08-24 UI-303 任务中心输入文字垂直裁切修复交接

- 根因已确认：TextBox 模板把 `TextBox.Padding` 同时用于 `PART_ContentHost.Margin`，WPF TextBox/Playnite 宿主又对内容宿主应用 Padding，任务搜索框上下 `7 DIP` 重复扣除，`PART_ContentHost.ViewportHeight` 仅约 `5 DIP`。不要恢复 `Margin="{TemplateBinding Padding}"`，也不要通过增加 TextBox 高度修复。
- `WpfUiProduction.xaml`、`DesignTokens.xaml` 的 `PART_ContentHost` 当前使用 `Margin="0"`、`Padding="0"`、`BorderThickness="0"`；TextBox 的 Padding 单独负责左右输入边距。内容宿主显式继承 TextBox 的 Foreground、FontFamily、FontSize、FontWeight，避免宿主主题重新改变文字度量或颜色。
- 当前验证：源码门禁、XAML 19/19、WPF 静态审查 0 error/20 warnings/165 info、Release 0 warning/0 error、Core 59/59、Worker 199/199、Playnite 282/282（57 跳过）通过；输入态离屏 viewport `5→19 DIP` 且文字完整显示，render QA 通过；生产安装已核验清单 `0.6.70`、DLL `0.6.70.0`。真实 Playnite 逐像素截图仍需用户复核。

## 2026-08-24 UI-302 任务中心搜索框可见性与图标间距修复交接

- TaskCenter 的“搜索任务…”和 Dashboard 游戏库搜索框属于带图标输入，当前输入 Padding 为 `30,7,38,7`，对应占位提示从 `30` DIP 起始；右侧 `38` DIP 继续为清除按钮保留空间。不要把这两处重新收紧到 `20`，否则放大镜会贴住提示文字。
- `WpfUiProduction.xaml` 和 `DesignTokens.xaml` 的 TextBox 模板必须把 `TextElement.Foreground` 绑定到 `Foreground`，并保留 `PART_ContentHost` 的现有对齐绑定；这是 Playnite 宿主下真实输入文字可见性的关键。数值输入的专用对齐、搜索清除按钮、Binding、命令、焦点和无障碍语义保持。
- 当前验证：`validate-source.py`、XAML 19/19、WPF 静态审查 0 error/20 warnings/165 info、Release 0 warning/0 error、Core 59/59、Worker 199/199、Playnite 282/282（57 跳过），`artifacts/ui-qa/ui302-task-search-v1/render-qa-report.txt` 为 `render-qa OK`；受控生产安装已核验扩展清单 `0.6.70`、DLL `0.6.70.0`，用户宿主逐像素截图仍需用户复核。

## 2026-08-24 UI-301 表格右侧安全边距与搜索输入起点微调交接

- 共享及 Dashboard 本地选中行模板的 `RowChrome` Margin 已统一为 `4,2,12,2`；这只是为了避开 Playnite 宿主垂直滚动轨道，不要改动 `SelectiveScrollingGrid`、滚动方式、排序或 Recycling。
- UI-301 曾将 TaskCenter 和 Dashboard 游戏库的带搜索图标输入框左侧 Padding/提示 Margin 收紧为 `20`；UI-302 已修正为 `30`，右侧清除按钮预留 `38` 保持；普通 TextBox、数值输入、清除按钮、Binding 和键盘焦点语义不变。
- 当前验证：`validate-source.py`、XAML 19/19、WPF 静态审查 0 error/20 warnings/165 info、Release 0 warning/0 error、Core 59/59、Worker 199/199、Playnite 282/282（57 跳过），`artifacts/ui-qa/ui301-spacing-v1/render-qa-report.txt` 为 `render-qa OK`；受控 `scripts/dev-install-run.ps1 -Configuration Release -NoStart` 已安装生产扩展 `0.6.70.0`。真实 Playnite 宿主逐像素边距仍由用户复核。

## 2026-08-24 UI-300 / FUNC-004 表格、输入框与 FLiNG 归档修复交接

- 共享 `WpfUiProduction.xaml`、`DesignTokens.xaml` 的普通 TextBox 现在显式使用左对齐内容和文本；模板把 `HorizontalContentAlignment` 传给 `PART_ContentHost`，因此插入光标从输入框左侧内边距开始。数值输入仍由专用样式覆盖为右对齐或居中，不改变原有编辑语义。
- 游戏、媒体、任务和 FLiNG 搜索框均增加共享清除按钮：有内容时显示 `×`，清空后保留键盘焦点；Dashboard 原有游戏搜索行为保持不变。按钮使用 AutomationProperties 名称，右侧输入内边距为清除按钮预留命中区。
- 共享 `GscRoundedDataGridRowTemplate` 与 Dashboard 本地兼容行模板把选中描边收进右侧安全区，避免 Playnite 宿主垂直滚动轨道覆盖右边圆角；Save/Task/Media/Maintenance 仍复用同一选中态、滚动、排序和 Recycling 契约。
- Overview 风险区两个真实命令按钮统一使用工具栏按钮模板、固定共享高度和垂直居中，命令、Automation 名称和安全行为不变。
- FLiNG 归档按实际站点链路处理：归档站点的字母/子目录列表中的 `.zip`、`.rar`、`.7z` 直链进入现有目录搜索；下载后识别 RAR/7z 签名，由 SharpCompress 流式解包，并复用越界路径、条目数、单文件和总展开体积限制；不执行压缩包中的 EXE。官方归档临时不可用时仍不阻塞在线目录刷新。
- 当前阶段验证：`validate-source.py`、XAML 19/19、WPF 静态审查 0 error/20 warnings/165 info、Release 0 warning/0 error、Core 59/59、Worker 199/199、Playnite 282/282（57 项按既有环境规则跳过），`artifacts/ui-qa/ui300-input-fling-v1/render-qa-report.txt` 为 `render-qa OK`。`scripts/dev-install-run.ps1 -Configuration Release -NoStart` 已在受控 Windows 权限下完成生产扩展安装，清单 `0.6.70`、DLL `0.6.70.0`；尚未从官方归档实际下载并运行修改器，安全软件拦截仍需用户环境复核。

## 2026-08-23 FUNC-002 媒体收件箱忽略恢复交接

- 本轮继续按用户要求只补功能块，不重排媒体页面。`MediaInboxBatchActionRow` 内增加“待归类/已忽略”视图切换；已忽略列表按需加载，默认待归类流程仍只请求原有消息。Inspector 在已忽略模式只显示恢复动作，原有归类、忽略、批量选择和 DataGrid 滚动/虚拟化保持不变。
- Worker/SQLite 新增 `ListIgnoredMedia`、`RestoreIgnoredMediaBatch`、`GetIgnoredMediaAsync` 和 `RestoreMediaToInboxAsync`。恢复最多 500 个去重 ID，文件先做现有归档/原始来源选择和目标冲突哈希校验，再移动回 `_Inbox\\Pending`；原始文件不删除，数据库回写 `Inbox`/`NotApplicable`/“用户撤销忽略，待重新归类”，每项追加审计，部分失败返回明细。
- 恢复完成后 Playnite 刷新两个缓存，防止切回待归类时显示旧数据。当前模式不持久化，重新打开页面默认待归类；不要在旧 Worker 兼容路径中预加载已忽略查询。
- 当前证据：`validate-source.py`、XAML 19/19、WPF 静态审查 0 error；Release 0 warning/0 error；Core 59/59、Worker 196/196、Playnite 280 通过/57 跳过；`artifacts/ui-qa/media-inbox-restore-v1/render-qa-report.txt` 为 `render-qa OK`，覆盖双主题、多尺寸、滚动和 resize transition。真实 Playnite 未执行会改变用户媒体数据的恢复操作；宿主 DPI、高对比度、键盘焦点和真实点击命中仍需人工验收。

## 2026-08-23 FUNC-001 媒体收件箱批量操作交接

- 本轮按用户要求只做功能块，不重排页面。`MediaCenterView.xaml` 在待归类表格上方增加轻量 `MediaInboxBatchActionRow`：多选收件箱行、选择目标游戏、批量归类或忽略；原 Inspector、单条操作和表格滚动/虚拟化不变。
- Worker 新增 `media.reassign.batch` / `media.inbox.ignore.batch`，每个请求最多 500 个去重 ID；Playnite 更大选择自动分批。服务端复用现有文件移动、归档副本、SQLite 索引和审计逻辑，返回成功项与失败明细；忽略仍保留归档副本，取消继续传播。
- `AcrylicProductionShellView.xaml` 的 Worker/Ludusavi 状态改为真实 DataTrigger 文案和颜色，禁止恢复原始布尔文本。
- 当前证据：源码校验、XAML 19/19、Release 构建 0 warning/0 error、Core 59/59、Worker 194/194、Playnite 280 通过/57 跳过，`artifacts/ui-qa/media-inbox-batch-v1/render-qa-report.txt` 为 `render-qa OK`，WPF 审查 0 error。没有在真实 Playnite 执行会改动用户数据的批量操作；宿主 DPI、高对比度和真实点击命中仍需人工验收。

## 2026-08-22 UI-296 任务/媒体摘要条实际宿主交接

- 用户提供的 Playnite 截图证明实际宿主仍在显示旧的“四列全等宽 + Rectangle 占用第 2/3/4 个统计列”布局：第一块后缺线、最后一块后多线。此前源码的七列修复没有同步到 00:07 安装的 DLL，不能只看离屏 RenderHarness 就认为宿主已经更新。
- `TaskCenterView.xaml` 与 `MediaCenterView.xaml` 现在统一使用 `* / Auto / * / Auto / * / Auto / *`；四个统计块为 `0/2/4/6`，三条竖线为 `1/3/5`，没有尾线。测试锁定精确列序列、3 条分隔线及仅允许奇数列放置 Rectangle。
- UI-296 已通过 `validate-source.py`、WPF 静态审查、Release 构建/测试和 `artifacts/ui-qa/summary-divider-layout-fix/render-qa-report.txt` 双主题多尺寸回归；构建为 0 warning/0 error，Core 59、Worker 194、Playnite 277/57/0。
- 已运行 `scripts/dev-install-run.ps1 -Configuration Release -NoStart`，标准 Playnite 扩展目录验证为 `0.6.70.0`。本轮没有启动 Playnite 也没有声称真实嵌入像素已通过；用户启动后应重新查看 Task/Media 两页确认宿主截图。

## 2026-08-22 UI-297 页面激活运行中游戏同步交接

- 旧自动定位依赖首次 Worker Dashboard 快照中的 `IsRunning` 和页面存活期间的 `PlayniteGameStarted`；Worker 在游戏已运行后才启动时，进程检测首轮基线不会创建会话，因而选框可能保持上次游戏。
- `GameSaveCenterPlugin.TryGetCurrentlyRunningPlayniteGameIds()` 只读 Playnite SDK `Game.IsRunning`。`DashboardView` 在 `Loaded`、重新可见和快照应用后调用 `DashboardViewModel.SelectCurrentlyRunningGameOnViewActivation()`；它只覆盖 UI DTO 状态并按现有 resolver 规则切换，不发送 Worker IPC、不创建会话、不触发备份，也不改变停止后保留选择的语义。
- `GamePickerItem` 的 `INotifyPropertyChanged` 仅用于不替换缓存对象时刷新运行状态；大库筛选/排序/虚拟化和真实命令/Binding 不变。
- 验证：`validate-source.py` 通过；隔离 Release XAML 19/19、0 warning/0 error；专项测试 1/1 通过；WPF 审查 0 error、21 warnings、164 info。完整 Playnite 测试当前分支 240 通过/62 跳过/19 既有 Demo/布局断言失败；真实 Playnite 反复打开页面、多游戏同时运行、主题/DPI 尚未验证。

## 2026-08-22 UI-295 媒体摘要分隔线交接（已由 UI-296 统一为 Auto 分隔槽）

- `MediaCenterView.xaml` 的 `MediaSummaryPanel` 使用 `* / Auto / * / Auto / * / Auto / *` 七列；四个统计块位于 `0/2/4/6`，三条分隔线位于 `1/3/5`，最后一块右侧不能再增加 Rectangle。
- 本轮只修正摘要条几何，真实 MediaSummary/Snapshot OneWay Binding、媒体 Tab、DataGrid/ListBox 虚拟化、Inspector、命令和滚动模型均未改变。
- UI-295 已通过源码校验、WPF 静态审查、Release 构建/测试和 `artifacts/ui-qa/media-summary-divider-fix/render-qa-report.txt` 双主题多尺寸回归；真实 Playnite 宿主 DPI/连续缩放仍需人工验收。

## 2026-08-21 BUILD-002 跨电脑构建与测试交接

- `9b19dbd` 的 Release 编译本身通过；此前另一台机器的 5 个失败均是视觉对照测试读取 `D:\workplace\Github\GameSaveCenter.AcrylicFork` 导致的 `DirectoryNotFoundException`。
- 已删除 `AcrylicForkDesignSource.cs`、`AcrylicForkDesignFactAttribute.cs` 及五个测试中的外部 Demo 读取；这些测试现在用普通 `[Fact]` 验证仓库内生产资源契约。不要重新引入 Demo 目录解析、绝对路径或“缺失时跳过”的默认逻辑。
- 普通跨电脑构建不需要 `GSC_ACRYLICFORK_ROOT`、`GSC_REQUIRE_ACRYLICFORK_BASELINE` 或 `GameSaveCenter.AcrylicFork` 兄弟目录。Demo 是视觉设计来源，不是生产编译/测试输入。
- Worker 默认 Soak 规模已设为慢盘可接受的边界值；完整压力测试使用 `GSC_SOAK_DATA_SCALE=1`，稳定性周期可用 `GSC_SOAK_ITERATIONS` 覆盖。验证脚本继续使用 `-m:1 -nodeReuse:false`，避免 SDK 多节点恢复阶段长时间无输出。
- 本阶段只修复测试环境可移植性，未回退或重写此前 Demo-first UI、功能、绑定、命令和业务行为。

## 2026-08-21 UI-287 表格表头与列宽交互交接

- 共享 `WpfUiProduction.xaml` 的 DataGrid 表头现在必须包含 `PART_LeftHeaderGripper` 和 `PART_RightHeaderGripper`；不要为了改箭头而删掉这两个 WPF 模板部件。第一列左 Thumb 由 WPF 自动折叠属于正常行为，其他可调整边界必须有有效命中区。
- 共享 DataGrid 设置 `CanUserResizeColumns=True`、`MinColumnWidth=64`、`CanUserSortColumns=True`；排序箭头使用独立 22 DIP 列，避免窄列裁剪。`DashboardView.xaml` 的本地兼容表头也保持同样的 22 DIP 箭头列和两个 resize 部件。
- 不要为单个页面复制另一套表头模板；Save、Media、Task、Maintenance 使用共享表格契约，真实绑定、排序、选中态、虚拟化和现有滚动条系统不变。
- `RenderHarness` 的 `VerifyDataGridHeaderInteractionContract` 会检查真实模板部件和排序态箭头布局；运行 `scripts/render-qa.ps1` 时若表格回退到没有 Thumb 或箭头宽度为 0 的模板，必须视为失败。
- 当前证据：`artifacts/gsc-b/ui287-table-header-v2` 构建/测试通过；`artifacts/ui-qa/ui287-table-header-v2/render-qa-report.txt` 为 `render-qa OK`。真实 Playnite 宿主仍需在可见窗口中实际拖动至少一张 Save/Media/Task 表格列边界确认命中体验，不能把离屏证据写成宿主已验证。

## 2026-08-21 UI-286 修改器导入与历史归档交接

- `GameToolService` 已修复文件名误判：含版本文字 `Update` 的修改器 EXE 不再被过滤；只有明确的 `unins*`、`uninstall`、`update`、`updater`、`setup` 辅助入口在目录/ZIP候选列表中排除。显式单文件导入必须保留其入口。
- 拖放导入的候选集合、选中项、清理和导入后工具选择均通过 `DashboardViewModel.ApplyOnUi`，防止 Worker/IPC continuation 直接修改 WPF `CollectionView`。不要将 `Replace(ImportEntryCandidates,...)` 或 `ImportEntryCandidates.Clear()` 移回后台 continuation。
- FLiNG 目录刷新现在可选读取 `https://archive.flingtrainer.com/` 的静态目录；历史 ZIP/RAR/7z/EXE 作为“FLiNG 归档”搜索结果，读取版本后走现有下载/安全解压/入口选择流程。归档失败是可降级的，不得让当前在线目录刷新失败；RAR/7z 由 SharpCompress 解包并继续执行同一安全限制。
- 当前证据：`artifacts/gsc-b/ui286-trainer-import-v2` 为 XAML 18/18、Release 0 warning/0 error、Core 59/59、Worker 194/194、Playnite 273 通过/60 跳过/0 失败；`artifacts/ui-qa/ui286-trainer-import-v1/render-qa-report.txt` 为 `render-qa OK`；source 0 error，WPF UI 0 error、20 warnings、164 info。真实 Playnite 宿主和用户提供的 EXE 均未执行/未验证，不能把本阶段写成真实宿主验收或总迁移完成。

## 2026-08-21 UI-285 媒体待归类反向滚动空白交接

- `MediaInboxGrid` 已局部关闭 `infra:DataGridStarFill.Enabled`。它仍保留有限 Grid 中的原生星号列、Standard 行虚拟化、Item 滚动和列虚拟化关闭；不要恢复共享 star-fill，也不要关闭全部虚拟化。
- 根因是有限视口中的 star-fill 像素重分配会在大数据反向滚动时触发行/列呈现器重测量，出现滚动条有效但行容器不再显示。真实 `UnassignedMedia`、选中媒体、预览 Inspector、归类/忽略入口和安全语义均未改动。
- RenderHarness 已加入多次 `0→100→0→100→50→0→100→0` 的 4468 行收件箱回退探针，并把 star-fill 开关纳入门禁；最新 `render-qa-report.txt` 为 `render-qa OK`，构建/测试和源码校验通过。
- 这是代码级和离屏验证结果；真实 Playnite 宿主仍需用户在可见窗口拖动到底再回翻确认，不能把本阶段写成 Playnite 生产渲染已验证。

## 2026-08-21 UI-284 主题前景色与页面控件对齐交接

- 共享 `GscWpfUiButtonTextTemplate` 已把 Button 的动态前景色和字体属性传给内部文字；这是修复深色主题 Primary 按钮黑字的根因，后续页面不要复制 Button 模板。
- 首页风险操作区固定两个按钮的高度并水平对齐；需关注事项继续使用真实 `AttentionFindings` 的 Demo 分隔列表，`SuggestedAction` 仍是右侧文字，不得换回 checkbox、逐项按钮或空的自定义卡片；底部维护入口使用带背景/描边的 Secondary 样式。
- 设置分类栏已从 `LabSegmented` 切换到 `GscSettingsSectionTabs`，背景、选中态、文字和 Hover 均来自当前主题动态资源，浅色/深色主题都不能写死灰色填充。
- 比较页质量气泡使用固定高度与内边距，并保持标题行垂直居中；同时保留比较/保留页横向画布、真实差异/保留绑定和安全说明。
- 当前证据：`artifacts/ui-qa/ui284-theme-v1/render-qa-report.txt` 为 `render-qa OK`，构建和测试通过，WPF 静态校验 0 error；本阶段未运行真实 Playnite 宿主，后续仍需在可见宿主补验主题、DPI 和交互命中。

## 2026-08-21 UI-283 媒体待归类大数据滚动修复交接

- 用户反馈待归类收件箱在约 4468 条数据向下滚动时出现表头下空白表格。当前 `MediaDataGrid` 必须保留 `Item` 滚动和行虚拟化，但局部覆盖为 `VirtualizingPanel.VirtualizationMode=Standard`、`EnableColumnVirtualization=False`；这是针对星号文本列与大数据量的 WPF 呈现稳定性例外。
- 不要把该例外恢复为共享 `Recycling`/列虚拟化，也不要通过关闭整个 DataGrid 虚拟化、复制滚动条或删除 Inspector 来规避。`UnassignedMedia`、`SelectedInboxMedia`、预览、目标游戏选择、归类/忽略命令及安全语义均已保留；Inspector 继续位于表格滚动面之外。
- `RenderHarness` 媒体探针固定 4468 条数据，并显式验证媒体为 Standard/关闭列虚拟化，其余工作区仍验证共享 Recycling/列虚拟化。最新报告：`artifacts/ui-qa/ui283-media-inbox-v5/render-qa-report.txt` 为 `render-qa OK`；构建/测试：`artifacts/gsc-b/ui283-media-inbox-v1`。
- 本阶段完成了代码级回归修复和离屏验证，但没有新的可识别 Playnite 真实 Dashboard 逐页截图；后续仍需用户在可见宿主中确认实际拖动滚动条、预览/归类按钮和主题/DPI 行为，不能宣布总 Demo-first 迁移完成。

## 2026-08-21 UI-279 Trainer 导入工具栏窄宽交接

- `TrainerCenterView` 标题区新增 `TrainerToolsToolbar` 与 `TrainerToolsDropHint` 的独立布局行；`ApplyResponsiveLayout` 在 `<980 DIP` 时将四个真实导入命令置于标题下方，拖放提示再下一行，解决 1040×700 / 744 DIP 工作区最后一个按钮被边界裁切的问题。
- 继续保留项目 `TrainerTabControl` / `TrainerTabItem`，不得用隐藏按钮、横向溢出或 Demo 外层 segmented 解决；工具列表、导入确认、工具编辑 Inspector、真实绑定/命令、ScrollViewer 和回收虚拟化不变。
- 当前证据：`artifacts/ui-qa/ui279-trainer-toolbar-v1/render-qa-report.txt` 为 `render-qa OK`；Light/Dark Trainer 1040×700 已抽查，四个导入按钮均在可视区域；`artifacts/ui-audit-ui279-trainer-toolbar-v1/AUDIT_SUMMARY.md` 无 HIGH、无 Fidelity、无失败路由。静态审计的 MEDIUM `TOOLBAR_VERTICAL_EXPANSION` 只来自 Inspector 内必要的五项设置换行。

## 2026-08-21 UI-278 RenderHarness 主题背景交接

- RenderHarness 的页面宿主背景不能写死为深色；`CreateHarnessBackground(view)` 必须优先读取页面当前主题的 `GscBackdropBrush`，否则强制浅色截图会把正确的深色文字错误压到深色画布上，产生假低对比。
- 本轮覆盖主题 QA、Tab 页面、单页页面、滚动/布局探针和 resize 审计宿主；未主题化的旧探针保留原深色 fallback。生产 View、真实命令/绑定、滚动、虚拟化和项目 Tab chrome 均未改动。
- 当前证据：`artifacts/ui-qa/ui278-themed-host-v1/render-qa-report.txt` 为 `render-qa OK`，浅色 Trainer 1040×700、浅色 Save 1366×768、深色 Trainer 1040×700 已抽查。真实 Playnite Dashboard 主题与操作证据仍需用户在可见宿主中补验。

## 2026-08-21 UI-277 折叠栏表面交接

- 共享 `GscDisclosureCardExpander` 已为 Header 提供 `GscControlFillBrush` 表面，并把 Expander 的背景/边框绑定到 Header ToggleButton 与 `HeaderChrome`；这修复了 Task 1040×700 窄宽下“更多筛选”只剩箭头、标题无法识别的问题。
- 后续页面继续使用共享 `GscDisclosureCard`，不要通过局部颜色或隐藏 Header 解决对比度；`LabDisclosure` 对应的整行点击、Hover/Expanded tint、Chevron 动效、真实筛选命令和键盘焦点都必须保留。
- 当前证据：`artifacts/gsc-b/ui-277-disclosure-surface-v1` Release 0 warning/0 error、Core 59/59、Worker 191/191、Playnite 266 通过/62 跳过/0 失败；`artifacts/ui-qa/ui277-disclosure-surface-v1/render-qa-report.txt` 为 `render-qa OK`，已抽查 Task 浅/深主题 1040×700。真实 Playnite Dashboard 的折叠栏命中与键盘行为仍需用户在可见宿主中补验。

## 2026-08-21 UI-276 媒体当前页操作区交接

- `MediaCenterView` 当前游戏媒体页的批量操作与紧凑 Inspector 入口已拆成 `MediaCurrentActionRow`：宽屏同一行，窄屏提示独占首行、批量操作与“查看媒体详情”分列第二行，避免 Playnite 工作区约 744 DIP 时按钮互相覆盖。
- 真实媒体选择、收藏/取消收藏/应用备注命令、异步预览、Recycling ListBox、Inspector 滚动和紧凑详情抽屉均未迁移；后续不要通过隐藏批量操作或删除详情入口来修复窄宽布局。
- 当前证据：`artifacts/gsc-b/ui-276-media-actions-v1` Release 0 warning/0 error、Core 59/59、Worker 191/191、Playnite 266 通过/62 跳过/0 失败；`artifacts/ui-qa/ui276-media-actions-v1/render-qa-report.txt` 为 `render-qa OK`，已抽查媒体浅/深主题 1040×700 与浅色 1366×768。真实 Playnite Dashboard 的媒体命中、键盘焦点和 DPI 仍需用户在可见宿主中补验。

## 2026-08-21 UI-275 滑杆几何交接

- 共享 `GscSlider` 已对齐 Demo `LabSlider` 的 22 DIP 高度、4 DIP 轨道和 18 DIP 滑块；唯一 Settings 使用点、真实值绑定、`ValueChanged` 事件、键盘焦点和生产主题阴影继续保留。
- 不要在设置页局部复制滑杆模板，也不要用删除滚动内容来解决低高度布局；RenderHarness 已覆盖 Settings 双主题、多尺寸、滚动和 resize，但真实 Playnite 中的拖动/键盘调节仍需验收。
- 当前证据：`artifacts/gsc-b/ui-275-slider-v1` Release 0 warning/0 error、Core 59/59、Worker 191/191、Playnite 265 通过/62 跳过/0 失败；`artifacts/ui-qa/ui275-slider-v1/render-qa-report.txt` 为 `render-qa OK`。

## 2026-08-21 UI-274 输入框与下拉状态交接

- 共享 `GscWpfUiTextBoxTemplate` 已对齐 Demo 聚焦填充与 Accent 边框，`GscWpfUiComboBox` 的隐式选项已对齐 Demo 字体、悬停/选中 tint、Medium 字重和 Hand 光标；验证错误、Popup、键盘导航和真实选择绑定均保留。
- `GscControlFocusFillBrush` 有默认令牌、普通主题 Demo 核心覆盖和高对比度 WPF 适配路径。后续页面不要局部复制 TextBox/ComboBox 模板，也不要用隐藏下拉项或改变滚动模型来解决布局问题。
- 当前证据：`artifacts/gsc-b/ui-274-input-combo-v1` Release 0 warning/0 error、Core 59/59、Worker 191/191、Playnite 264 通过/62 跳过/0 失败；`artifacts/ui-qa/ui274-input-combo-v1/render-qa-report.txt` 为 `render-qa OK`。真实 Playnite Dashboard 仍需用户在可见宿主中打开 GameSaveCenter 后补证。

## 2026-08-21 UI-273 按钮与开关共享状态交接

- `Themes/WpfUiProduction.xaml` 的 `GscWpfUiButton` 已加入 Demo `LabBtn` 的悬停/按下覆盖层和过渡；`GscWpfUiToggleSwitch` 已对齐 Demo `LabToggle` 的 40×23 DIP 几何与 140ms 滑块位移动效。覆盖层不可命中，所有真实内容、命令、绑定和焦点语义保持。
- 不要为了局部页面效果复制按钮/开关模板，也不要把本阶段误扩展为项目工作区 Tab、当前游戏选框或生产滚动条迁移；Trainer/Save/Media/Maintenance 外层仍使用项目 Tab chrome。
- 当前证据：`artifacts/gsc-b/ui-273-shared-button-toggle-v1` Release 0 warning/0 error、Core 59/59、Worker 191/191、Playnite 263 通过/62 跳过/0 失败；`artifacts/ui-qa/ui273-shared-button-toggle-v1/render-qa-report.txt` 为 `render-qa OK`。真实 Playnite Dashboard 仍需用户在可见宿主中打开 GameSaveCenter 后补证。

## 2026-08-21 UI-272 修改器中心 Tab chrome 回滚交接

- 用户明确要求项目 Tab 栏优先于 Demo Tab UI；已回滚 `TrainerCenterView` 在 `a03accf` 引入的 `TrainerSegmentTabs` + `LabSegmented`，恢复 `TrainerTabControl` / `TrainerTabItem` 项目样式和四个真实 `TabItem`。
- 这只回滚外层导航容器，不回滚页面内容或业务：工具列表、导入确认、FLiNG 目录、发行版本、Inspector、拖拽导入、命令/绑定、回收虚拟化、ScrollViewer 和响应式布局均保留。Settings 左侧五项 `LabSegmented` 分类栏继续保留，这是 Demo 目标明确要求的 Settings 信息架构，不属于项目工作区 Tab chrome。
- 继续禁止把修改器中心外层导航恢复成 Demo `LabSegmented`；Save、Media、Maintenance 及 Dashboard 的项目 Tab chrome 也不得因 Demo-first 页面迁移被替换。
- 最新验证：`artifacts/gsc-b/ui-272-trainer-tab-rollback-v2` 为 XAML 18/18、Release 0 warning/0 error、Core 59/59、Worker 191/191、Playnite 262 通过/62 跳过/0 失败；source/WPF/diff 门禁通过；`artifacts/ui-qa/ui272-trainer-tab-rollback-v1/render-qa-report.txt` 为 `render-qa OK`，覆盖七页双主题、多尺寸、各 Tab、滚动和 resize。离屏截图已抽查 Trainer 浅/深主题；真实宿主证据仍未补齐。

## 2026-08-21 UI-271 真实 Playnite 宿主审计边界交接

- `scripts/real-host-audit.ps1 -Configuration Release -Output artifacts/ui-host-audit-ui271` 已完成 Release 构建、安装和 Playnite 启动；`summary.json` 为 `EmbeddedSettingsCaptured=true`、`ControlledDashboardCaptured=true`、`EmbeddedDashboardCaptured=false`、`ProductionVisualSourceOfTruthAvailable=false`。
- Settings 的 `settings/embedded-current/viewport/settings.png`、视觉树和资源快照确实来自 `EmbeddedPlaynite`，可用于 Settings 的宿主复核。Dashboard 自动 UI Automation 没有找到左侧 GameSaveCenter 入口，最终 90 秒超时并保留 `gates/REAL_EMBEDDED_DASHBOARD_NOT_CAPTURED.json`；Controlled Dashboard 只能作为受控布局证据。
- Computer Use 观察到 Playnite 主窗口返回 `EmptyWindowAutomationPeer`，没有把 Codex/其他窗口画面当作 Playnite Dashboard，也没有停止不属于本轮的旧 Worker。下次优先让用户在可见 Playnite 左侧手动打开 GameSaveCenter，再重跑审计。
- 本轮基线：XAML 18/18；Release 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 262 通过、62 跳过、0 失败。真实 Dashboard 七页的像素、DPI、键盘焦点、命中区域、主题切换和真实操作仍未收口，总 Demo-first 目标不能宣布完成。

## 2026-08-21 UI-271 共享表格字阶交接

- 生产 `DesignTokens.xaml` 的 `GscBodyFontSize=13.5`、`GscCaptionFontSize=12` 对齐 Demo `SizeBody`/`SizeCaption`；共享 `DataGrid` 和 `DataGridColumnHeader` 使用 UI 字体链、正文/表头字阶和 Medium 表头字重。
- 不要为解决表格文字大小在页面局部加另一套字号；`44 DIP` 行高、`36 DIP` 表头、排序箭头、列宽调整、选中态、内部滚动和 Recycling 虚拟化继续由共享生产样式负责。
- 最新验证：`artifacts/gsc-b/ui-271-table-typography-v1` Release 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 262 通过/62 跳过/0 失败；source/WPF/diff 门禁通过；`artifacts/ui-qa/ui271-table-typography-v1/render-qa-report.txt` 为 `render-qa OK`，覆盖七页双主题、多尺寸、滚动和 resize。已抽查 Save/Task/Maintenance 表格截图，但真实 Playnite 宿主的字号、DPI、键盘焦点和列宽拖动仍需验收。

## 2026-08-21 UI-270 共享折叠栏动效交接

- `Themes/DesignTokens.xaml` 的 `GscDisclosureCardExpander` 已按 Demo `LabDisclosure` 增加 Chevron 150ms 展开/收起旋转，`GscDisclosureCard` 仍是页面统一使用的共享入口；不要在单个页面复制另一套 Expander 模板。
- 本阶段保留整行点击、键盘焦点、真实 `IsExpanded` 绑定、内容显隐、页面滚动和生产 ScrollBar；没有改动业务命令、数据、虚拟化或当前游戏选框。
- 最新验证：`artifacts/gsc-b/ui-270-disclosure-animation-v1` Release 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 261 通过/62 跳过/0 失败；source/WPF/diff 门禁通过；`artifacts/ui-qa/ui270-disclosure-animation-v1/render-qa-report.txt` 为 `render-qa OK`，覆盖七页双主题、多尺寸、滚动和 resize。动效时间曲线仍需在可识别 Playnite 宿主中用鼠标/键盘验收。

## 2026-08-21 UI-269 Demo 核心主题配色交接

- `AdaptiveThemePaletteFactory.ApplyDemoCoreResources` 现在由生产 Shell 和 Settings 共用，普通浅/深色主题固定 Demo 的画布渐变、卡片/侧栏/顶栏、输入框、正文层级、表格、分段控件、滚动条、遮罩及成功/警告/错误/信息状态关系；宿主只继续影响非核心 Accent/focus 交互。
- 高对比度仍绕过 Demo 核心覆盖并使用系统自适应资源。不要把宿主 `Background`/`Foreground` 中性刷重新应用到迁移页面核心表面，也不要为配色修复替换生产 Tab chrome、当前游戏选框或滚动条交互。
- 当前验证：`artifacts/gsc-b/ui-269-demo-palette-v2` Release 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 260 通过/62 跳过/0 失败；source/WPF/diff 门禁通过；`artifacts/ui-qa/ui269-demo-palette-v1/render-qa-report.txt` 为 `render-qa OK`，覆盖七页双主题、多尺寸、滚动和 resize。已抽查浅/深色代表截图，但仍需可识别 Playnite 宿主的逐页像素、DPI、键盘焦点、主题切换和真实操作验收。

## 2026-08-21 UI-267 工作区表格测量与几何审计交接

- 媒体当前游戏媒体头部的操作区使用 `MinWidth=300`，搜索输入使用 `MinWidth=160`，所以标准及窄工作区不会把搜索框压成不可用的窄条；媒体搜索、筛选、卡片、预览 Inspector 和批量命令仍是真实绑定/命令。
- 存档历史页在工作区宽度 `<1240 DIP` 时采用紧凑历史列宽，以适配表格与 `360 DIP` Inspector 并列时的真实宿主内容宽度；状态列仍可通过 DataGrid Auto 横向滚动到达。不要通过隐藏状态列、移除 Inspector 或替换项目滚动条来解决此类宽度问题。
- `tests/GameSaveCenter.Playnite.Tests/OvernightV4SaveFormTests.cs` 的 `SharedWorkspaceBreakpointsKeepSearchAndHistoryEssentialsReadable` 是当前空间回归契约。生产 Tab chrome 是明确例外，当前游戏选框、共享 DataGrid 的排序/列宽拖动/Recycle 虚拟化、页面滚动和真实命令没有迁移。
- `tests/GameSaveCenter.RenderHarness/UiAudit/UiLayoutAnalyzer.cs` 现在按主控件直接 Grid 行计算填充，认可 Overview 有限活动视口和表格内部卡片布局，并把有实际横向滚动能力的列压缩记录为 `EXPECTED_HORIZONTAL_SCROLL`。最新审计 `artifacts/ui-audit-ui267-fix3/AUDIT_SUMMARY.md` 为 Fidelity 0、HIGH 0、MEDIUM 0、失败路由 0。
- 最新验证：`artifacts/gsc-b/ui-audit-layout-fix-v1` Release 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 259 通过/62 跳过/0 失败；source 门禁、WPF 校验（0 error、19 warnings、161 info）和 diff 门禁通过；`artifacts/ui-qa/ui267-layout-audit-fix-v1/render-qa-report.txt` 为 `render-qa OK`，覆盖双主题、多尺寸、滚动和 resize。
- 总 Demo-first 迁移仍未宣布完成：自动/离屏证据不能替代可识别 Playnite 生产宿主中的逐页像素、DPI、键盘焦点、主题和真实操作验收；下一阶段继续针对目标文件逐页复核并收集宿主证据。

## 2026-08-21 UI-268 标题字体阶关系交接

- 生产共享令牌现在有三条明确字体链：`GscUiFontFamily` 使用 `Segoe UI Variable Text`，`GscDisplayFontFamily` 使用 `Segoe UI Variable Display`，`GscCodeFontFamily` 使用 `Cascadia Mono`；三者均带 `Segoe UI`/`Microsoft YaHei UI` 回退。
- `GscRedesignHeroTitle`、`GscRedesignFeedbackDialogTitle`、`GscPageTitleStyle`、生产 Shell 标题和 Dashboard 回退标题使用 Display；分区标题仍使用正文族，避免把 Demo 的 `LabSection` 错误提升为 Display。
- 本阶段没有改变用户指定的生产 Tab chrome、当前游戏选框、生产滚动条、表格/列表虚拟化、真实命令或 Binding。验证已通过 Release 构建/测试、source/WPF 门禁、双主题多尺寸 RenderHarness；证据目录为 `artifacts/gsc-b/ui-268-display-font-v1` 与 `artifacts/ui-qa/ui268-display-font-v1`。
- 总目标继续未完成：仍需按目标文件逐项收口七页信息架构、Demo 颜色/控件/状态和可访问性证据，并在可识别 Playnite 生产宿主中完成逐页像素、DPI、键盘焦点、主题和真实操作验收。

## 2026-08-20 UI-266 存档维护指标阅读节奏交接

- 存档“比较与保留”页的新增/修改/删除差异指标，以及维护页的保留、容量、趋势、保留模拟、保护状态和本地镜像指标，已统一为 Demo 的“数值 → 标签 → 补充说明”节奏；真实绑定和只读/安全语义没有改变。
- `LastBackupDiff`、Snapshot、保留模拟、存储趋势和本地镜像等真实状态继续作为唯一数据来源；没有迁移 Demo Mock 数据，也没有改变任何命令、Inspector、DataGrid/列表虚拟化、滚动或生产 Tab chrome。
- 最新验证：XAML 18/18，Release 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 258 通过/62 跳过/0 失败；source/WPF/diff 门禁通过；`artifacts/ui-qa/metrics-rhythm-v1/render-qa-report.txt` 为 `render-qa OK`，覆盖双主题、多尺寸、滚动和 resize。仍需在可识别的 Playnite 生产宿主中完成逐页像素、DPI、键盘焦点及真实操作验收。

## 2026-08-20 UI-265 维护诊断概览环境健康区交接

- 维护页诊断概览已按 Demo 顺序把六项真实健康卡前置到 `DiagnosticHealthCard`，随后是 `EnvironmentCheckCard` 和 `MaintenanceDiagnosticsActionCard`；不要恢复到“更多维护操作”内部。
- `DiagnosticHealthPanel` 继续绑定 Worker/Ludusavi/Rclone、数据/媒体目录、待归类媒体和设备状态；环境检查展开项、诊断复制/导出、自检、目录日志、索引重建、任务协调、元数据灾备、路径迁移和安全模式入口均保留。宽屏 4 列、中等 2 列、窄屏 1 列由现有响应式代码管理。
- 最新验证：XAML 18/18，Release 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 258 通过/62 跳过/0 失败；source/WPF/diff 门禁通过；`artifacts/ui-qa/maintenance-health-order-v1/render-qa-report.txt` 为 `render-qa OK`，覆盖双主题、多尺寸、滚动和 resize。生产 Tab chrome 未动，仍需在可识别的 Playnite 生产宿主中完成逐页像素、DPI、键盘焦点及真实操作验收。

## 2026-08-20 UI-264 首页统计条连续结构交接

- `OverviewView` 顶部 `OverviewStatStrip` 已按 Demo 恢复为一个连续统计条：六个等宽指标、五条分隔线、26 DIP 数值且数字位于标签上方；不要恢复六张独立 metric card 或旧的列数响应式逻辑。
- 六项统计仍绑定真实 `Snapshot`，匹配率与风险率进度条、空游戏库隐藏保护和健康明细均保留；今日工作台、当前游戏选框、“立即备份/全部备份”、活动列表滚动/虚拟化及真实命令没有因视觉迁移被删除。
- 最新验证：XAML 18/18，Release 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 258 通过/62 跳过/0 失败；source/WPF/diff 门禁通过；`artifacts/ui-qa/overview-summary-strip-v1/render-qa-report.txt` 为 `render-qa OK`，覆盖双主题、多尺寸、滚动和 resize。生产 Tab chrome 仍保持当前项目实现，这是用户明确的例外；仍需在可识别的 Playnite 生产宿主中完成逐页像素、DPI、键盘焦点及真实操作验收。

## 2026-08-20 UI-263 任务统计条连续结构交接

- `TaskCenterView` 顶部 `TaskSummaryPanel` 已按 Demo 恢复为一个连续统计条：四个等宽指标、三条分隔线和 26 DIP 数值；生产 Tab/页签 chrome 例外规则不受本阶段影响。
- 计数仍来自真实 `Tasks.Count`、`RunningTaskCount`、`RetryableTaskCount`、`CompletedTaskCount`；任务筛选、更多筛选、任务队列 DataGrid、右侧详情 Inspector、复制错误/重试/取消命令没有迁移或删除。后台命名元素类型同步为 `Border`，摘要不再通过 `Columns` 响应式重排。
- 最新验证：XAML 18/18，Release 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 258 通过/62 跳过/0 失败；source/WPF/diff 门禁通过；`artifacts/ui-qa/task-summary-strip-v1/render-qa-report.txt` 为 `render-qa OK`，覆盖双主题、多尺寸、滚动和 resize。仍需在可识别的 Playnite 生产宿主中完成逐页像素、DPI、键盘焦点及真实操作验收。

## 2026-08-20 UI-262 媒体统计条连续结构交接

- `MediaCenterView` 顶部 `MediaSummaryPanel` 已按 Demo 恢复为一个连续统计条：四个等宽指标、三条分隔线和共享 `GscRedesignSectionCard`；生产 Tab 页签仍保留项目当前 Tab chrome，这是用户明确要求的例外。
- 统计值继续来自真实 `MediaSummary`/`Snapshot` 绑定；待归类 DataGrid、媒体预览、Inspector、目标游戏选择、归类/忽略/保留副本等入口没有迁移或删除。`MediaSummaryPanelElement` 后台类型同步为 `Border`，来源规则的 `MediaSourceFields` 仍是独立 `UniformGrid`。
- 最新验证：XAML 18/18，Release 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 258 通过/62 跳过/0 失败；source/WPF/diff 门禁通过；`artifacts/ui-qa/media-summary-strip-v1/render-qa-report.txt` 为 `render-qa OK`，覆盖双主题、多尺寸、滚动和 resize。仍需在可识别的 Playnite 生产宿主中完成逐页像素、DPI、键盘焦点及真实操作验收。

## 2026-08-20 UI-253 修改器中心分段结构交接

- 当前 `TrainerCenterView` 已按 Demo 恢复 `TrainerSegmentTabs` + `LabSegmented` 顶部分段导航，四个真实面板为 `PanelTools`、`PanelImport`、`PanelCatalog`、`PanelReleases`；旧 `TabControl/TabItem` 不再是页面主导航。
- 面板切换由 `OnTrainerSegmentChanged` 管理，只改可见性，不改业务数据或命令。必须保留工具导入、待确认 EXE 选择、FLiNG 搜索/刷新、版本加载/下载、工具编辑 Inspector 和紧凑详情入口。
- `TrainerToolsList`、目录结果、发行版本仍保留项目 ScrollBar、`CanContentScroll`、回收虚拟化和现有 `ApplyResponsiveLayout`；Demo segmented 导航仅包含 4 个标签，不应为了源码门禁给它添加大列表虚拟化要求。
- 构造期 `SelectionChanged` 的空保护是必要的：XAML 的 `SelectedIndex=0` 可能在四个面板字段完全生成前触发事件。
- 最新验证：Release 构建 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 252/252 通过、61 跳过；XAML/source/WPF 静态门禁通过；RenderHarness `render-qa OK`，证据为 `artifacts/ui-qa/trainer-segmented-final`，覆盖四个分段、双主题、多尺寸和 resize。尚无新的可识别 Playnite 宿主逐页像素证据。

## 2026-08-20 UI-252 存档历史页操作卡交接

- 默认“历史版本”页已补回 Demo 的 `SaveHistorySummaryCard`：真实版本数、当前规则/健康摘要，以及“立即扫描 / 重新校验 / 刷新详情”入口均位于历史表上方；对应命令仍是 `DetectPathsCommand`、`ValidateCommand`、`LoadDetailsCommand`。
- 700 DIP 以下摘要操作区转为第二行，正常宿主保持横向布局；历史 DataGrid 的列宽拖动、排序、虚拟化和现有滚动条未改变。
- 当前验证：XAML 18 个文件通过，Release 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 251 通过/61 跳过/0 失败，RenderHarness `render-qa OK`。仍需在可识别的 Playnite 生产宿主中逐页核对，不得以离屏 PNG 代替真机像素证据。

## 2026-08-17 UI-221 AcrylicFork 整页视觉迁移收口交接

- 本轮以 `GameSaveCenter.AcrylicFork` @ `b09cba6` 复核了 Overview/Save/Trainer/Media/Task/Maintenance/Settings 的生产骨架，并补齐 Media、Trainer、Save、Maintenance 分段导航右侧的样板说明与真实状态/计数。这些信息必须继续绑定真实数据，不能写死 demo 数字。
- 不要创建 `AcrylicParity.xaml` 这类独立 Lab 别名字典：独立 ResourceDictionary 的 `BasedOn` 无法引用父级合并字典的 Gsc 样式，页面构造会直接抛 `StaticResourceHolder` 异常。共享视觉键统一用生产 `Gsc*`。
- 生产滚动条、圆角表头、Item scrolling/Recycling 虚拟化、真实命令/绑定保持优先；右上角色板、预览徽标、Mock 数据和 demo 滚动条不迁移。
- 本轮验证：Core 59/59、Worker 191/191、Playnite 304/304；Release 0 warning/0 error；`render-qa OK` 证据在 `artifacts/ui-qa/acrylic-full-migration-v1`；真实宿主 0.6.70 安装加载成功，审计证据在 `artifacts/ui-host-audit-acrylic-v1`，但 `EmbeddedDashboardCaptured=false`，需要用户真实点击 Playnite 侧栏后再重跑 `real-host-audit.ps1` 才能补齐嵌入像素真值。

## 2026-08-16 UI-220 UiLab 几何对齐与媒体回滚修复交接

- `GscRedesignWorkspaceTabItem` 的 `HorizontalContentAlignment`/`VerticalContentAlignment` 必须保持 `Stretch`；标题的居中由模板内的 `HeaderContent` 完成。不要把标题对齐属性重新绑定给页面内容。
- `MaintenanceDiagnosticsSubTabs` 现在是 `ListBox` segmented control，不是 `TabControl`；内容由 `MaintenanceDiagnosticsFindingsPanel` 和 `MaintenanceDiagnosticsOverviewPanel` 切换。维护二级导航两项文字都必须保持可见。
- `VirtualizingWrapPanel.ResolveViewportHeight` 和 deferred recovery 是媒体网格滚动回顶的必要保护；继续保留生产虚拟化和滚动条，不要替换成 UiLab 的非虚拟化样例实现。
- 首页 `OverviewHeroColumn`/`OverviewCurrentGameColumn` 的宽度比例为 `1.35*:1`；744 DIP 左右的紧凑视口由 `ApplyResponsiveWidth` 堆叠，不能用旧的等宽比例恢复。
- 验证基线：Release 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 303/303；`render-qa OK`（双主题、多尺寸、resize、媒体滚动回顶）。WPF UI 校验无 error，但保留既有布局/主题资源 warnings。当前仍不能声称完成 Playnite 宿主逐页像素验收，因 Computer Use 无法稳定激活窗口。

## 2026-08-16 UI-219 UiLab 分段页面骨架直迁交接

- 本轮已把媒体、存档、修改器、维护四个生产页的主导航从旧 `TabControl/TabItem` 改成 UiLab 的 segmented navigation + named panel host；真实数据/命令/绑定、Inspector、DataGrid/ListBox 虚拟化和生产滚动条没有被 demo 样例替换。维护诊断/审计中的嵌套页签仍是页面内部层级。
- 新共享资源为 `GscRedesignSegmented`、`GscRedesignSegmentedItem`、`GscSegmentFillBrush`、`GscSegmentItemFillBrush`、`GscSegmentItemStrokeBrush`。继续修复时优先修改共享模板/令牌，不要对单个页面复制另一套圆角参数；UiLab 右上演示色板/窗口按钮/样例滚动条不属于生产迁移范围。
- 直接迁移后曾出现页面构造期 `SelectionChanged` 空引用，四个 `On*SegmentChanged` 已加入初始化期空保护；媒体虚拟网格的 `VirtualizingWrapPanel` 也已修复 generator 插入索引越界回退。若再次改动 XAML 面板顺序，必须先构造所有页面再运行 Playnite 测试。
- 证据：`validate-source.py`、WPF UI 校验、Release 构建通过；Core 59/59、Worker 191/191、Playnite 303/303；`DEV-INSTALL-008` 已安装 `0.6.70.0`，`extensions.log` 记录加载且无本轮新增崩溃。当前 Computer Use 只能捕获黑色 Playnite 窗口并返回 `EmptyWindowAutomationPeer`，激活失败；因此真实宿主逐页截图/像素对照仍需用户可交互窗口，不能把此次日志通过写成视觉真机验收。

## 2026-08-16 UI-218 UiLab 页面骨架迁入交接

- 本阶段把 `GameSaveCenter.UiLab` 的页面层级迁入生产工作区：页面头部保留唯一游戏上下文，取消 Dashboard 外层重复选中游戏卡、全局操作行和恢复安全横幅；安全提示归入策略/比较页面内部。
- 已收口的重点：备份策略三栏、比较指标窄宽可换行、修改器工具栏/拖入区分行、任务中心筛选分两行；真实数据、绑定、命令、Inspector、虚拟化和生产滚动条均保留。
- 明确不迁移 UiLab 右上演示色板、窗口按钮、样例数据和样例滚动条。用户指定的五类重叠只做防重叠修正，不能借机恢复生产页外壳或替换滚动模型。
- 验证：Release 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 303/303；真实 Playnite 约 1303×673 视口人工检查首页、备份、修改器、任务页通过，任务筛选与修改器拖入区不重叠。自动审计仍以 `EmbeddedDashboardCaptured=false` 为准，不能把 Computer Use 截图写成自动嵌入证据。

## 2026-08-16 UI-217 真实 Playnite 人工视觉复核

- `81fde54` Release 安装包已在真实 Playnite 中加载；人工进入 GameSaveCenter 后确认首页和任务中心的主要 UiLab 层级已出现在实际宿主，且保留真实数据、命令、绑定、虚拟化和生产滚动条。
- 任务中心在约 1303×673 宿主视口中显示四项统计带、筛选区、圆角表头 DataGrid、状态胶囊和进度条；Inspector 由真实 `SelectedTask` 驱动，默认无选择时隐藏，不能把样板默认选中的失败任务详情当成生产必显内容。
- 本轮手动观察不改变 `real-host-audit` 的事实边界：自动侧栏定位仍失败，`EmbeddedDashboardCaptured=false` 继续有效；后续如需可复现像素证据，应在用户实际打开侧栏后重跑审计。

## 2026-08-16 UI-215 Task 统计栏当前基线

- 任务中心顶部现在是单一 `TaskSummaryBand`，四个真实计数位于同一圆角阅读面，三条 `GscDividerBrush` 分隔；不要恢复四张独立指标卡或 `TaskSummaryPanel.Columns` 换列逻辑。
- 生产任务队列的真实筛选、详情 Inspector、DataGrid 表头/滚动条、Item scrolling 和虚拟化均保留。窄于 760 DIP 时，`TaskGameFilterHost` 整体（标签 + 下拉框）进入“更多筛选”，避免孤立标签错位。
- 当前证据：`artifacts/ui-qa/task-summary-band-v2`，双主题、多尺寸和 resize 均 `render-qa OK`；真实 Playnite Dashboard 仍未自动捕获，离屏证据不能替代宿主视觉验收。
- `TaskGameFilterHost` 的父子层级测试已同步更新；如果后续继续把筛选项成组移入“更多筛选”，应验证组容器移动和内部真实 ComboBox，而不是检查 ComboBox 的直接父级。

## 2026-08-16 UI-214 Overview 单卡统计栏当前基线

- 首页 Overview 的六项统计已收敛成 UiLab 风格的单一圆角 `OverviewStatBand`，内部是六个真实 `Snapshot` 指标和五条分隔线；不要恢复六个独立卡片或为指标恢复悬停位移动画。
- Dashboard 根层只保留一个 `GscAmbientAccentBrush` 磨玻璃晕影；右上角演示色板/窗口控制和 UiLab 滚动条仍明确不迁移，生产滚动条、页面滚动、虚拟化、命令与绑定继续作为事实来源。
- 当前证据：`artifacts/ui-qa/overview-single-band-v1`，覆盖双主题、多尺寸与 resize transition，`render-qa OK`。真实宿主 Dashboard 仍没有自动捕获到嵌入像素，不能把离屏图当作 Playnite 视觉真值。

## 2026-08-16 UI-213 真实宿主审计交接

- 当前 `420483f` 已执行 Release 真实宿主审计：构建、Core 59/59、Worker 191/191、Playnite 303/303、安装与 Playnite 启动通过；人工进入真实 Playnite 后确认 GameSaveCenter Settings 宿主窗口可见。
- `artifacts/ui-host-audit/summary.json`：`EmbeddedSettingsCaptured=true`、`EmbeddedDashboardCaptured=false`、`ControlledDashboardCaptured=true`、`ProductionVisualSourceOfTruthAvailable=false`。因此 Settings 的 `EmbeddedPlaynite` 截图有效，Controlled Dashboard 截图只能作为辅助布局证据，不能宣称为生产嵌入视觉真值。
- 自动 UIAutomation 仍未找到 Playnite 左侧 GameSaveCenter 入口；Media 缩略图网格尚未完成真实宿主视觉验收。后续人工 QA 优先进入真实 Media 页检查 164×142 卡片、实际缩略图、滚动和选中/Inspector 行为，再检查 DPI、主题、键盘焦点和连续缩放。

## 2026-08-16 UI-205-ACRYLIC-PARITY 当前开发基线

- 权威参考是 `D:\workplace\github\GameSaveCenter.AcrylicFork` @ `b09cba6`，不是 UiLab；其页面视觉与结构已迁入生产共享资源。AcrylicFork 演示数据、右上角颜色/主题按钮和样例滚动条明确不迁移，七组顶部色值仅用于主题参考。
- 生产保留真实命令、绑定、虚拟化和现有滚动条；Overview 普通状态不再显示重复全局命令卡，Settings 常用宽度使用左侧分类栏，低于 700/620 DIP 进入紧凑布局。
- `DEV-INSTALL-008` 已修复外来扩展 Worker 误杀：只处理当前生产目录，陌生但可读路径保留运行，无法读取路径仍 fail-closed。
- 自动验证已通过：Core 59/59、Worker 191/191、Playnite 302/302、Release 0 warning/0 error、source/XAML 门禁和 `render-acrylic-correction`；一键安装已成功启动 Playnite/生产 Worker。真实宿主最终主题/DPI/键盘/连续缩放仍需人工确认。

## 2026-08-16 UI-205-REAL-HOST-MIGRATION-FIX 当前开发基线

- 已修复生产 XAML 对外部 Contracts 程序集的 BAML 直接解析风险：Settings/Media/Trainer 使用 `GameSaveCenter.Playnite.XamlValues` 本地枚举包装，Dashboard 删除未使用的外部 namespace；不要恢复 `assembly=GameSaveCenter.Contracts` 的 XAML 直接引用。
- Dashboard 选中游戏头部现在以最终实际 viewport 宽度约束，窄屏操作行固定可用宽度并换行；WPF Grid 重排后在 ApplicationIdle 做二次响应式布局，避免 1600/1366/1280/1024 DIP 首次测量残留越界。
- Real Host 审计在 Render 后等待 ApplicationIdle，干净轮次清理旧 `CHILD_LAYOUT_OVERFLOW`；最终受控矩阵 `RealFixedLayoutOverflow=[]`。自动 UIAutomation 仍找不到真实侧栏，必须保留 `EmbeddedDashboardCaptured=false`，不要把 Controlled Host 截图宣称为生产视觉真值。
- 当前基线：Core 59/59、Worker 191/191、Playnite 303/303，Release 0 warning/0 error；安装验证 `0.6.70` / DLL `0.6.70.0`，生产日志无新的 XamlParseException。

## 2026-08-16 UI-REAL-HOST-AUDIT-BLOCKERS-FIX 实施完成

- Real Host Audit 阻塞项已收口：CommitSha 可追踪、SafeFileName 修复、overflow 分类 gate、resize 稳定截图、manifest Scope 隔离、内部滚动器过滤、Embedded 身份显式判定。
- headless 无人点击时 summary 诚实写 `EmbeddedDashboardCaptured=false` + HIGH gate；用户在 Playnite 点击 GameSaveCenter 后重跑 `scripts/real-host-audit.ps1` 即可得到真实 embedded 证据。
- 报告：`docs/ai/REAL_HOST_AUDIT_BLOCKERS_FIX_REPORT.md`。
- 基线：Playnite 302/302、Worker 191/191、Core 59/59；Release 0 warning/0 error。

## 2026-08-16 UI-HOST-AUDIT-TRUTHFULNESS-FIX 实施完成

- Real Host Audit 已可信化：Sidebar View 不再调用 `Activated`，等待用户真实打开；origin 显式（EmbeddedPlaynite/ControlledAuditWindow）；manifest 按 session/scope 隔离；DataGrid 逻辑滚动器不生成像素长图；`summary.json` 硬门禁 + `CHILD_LAYOUT_OVERFLOW` gate。
- 报告：`docs/ai/HOST_AUDIT_TRUTHFULNESS_FIX_REPORT.md`。
- 基线：Playnite 294/294、Worker 191/191、Core 59/59；Release 0 warning/0 error。

## 2026-08-16 UI-REAL-HOST-CAPTURE-COMPLETENESS-FIX 实施完成

- Real Host Audit 已按 Capture Contract 重构：embedded-current / controlled-host-window / ScrollSurfaceFull 三类输出，`capture-manifest.json` + gates + 完整性断言。
- Controlled host 用无边框窗口，profile 即 client size，Dashboard Stretch 且不写死 Width/Height；embedded 模式绝不 resize Dashboard 或覆盖主题。
- 关键文件：`RealHostUiAuditService.cs`、`UiDiagnosticsExporters.cs`、`UiAuditCaptureContractTests.cs`；报告 `docs/ai/REAL_HOST_CAPTURE_COMPLETENESS_FIX_REPORT.md`。
- 基线：Playnite 287/287、Worker 191/191、Core 59/59；Release 0 warning/0 error。

## 2026-08-15 LUDUSAVI-DIAGNOSTICS-FIX 实施完成

- 已修复：外部进程输出 UTF-8 解码；Ludusavi 失败时保留 `RawOutput`；剪贴板复制重试与失败降级（`CopyTextWithRetry`）。
- 备份失败根因常为 Ludusavi manifest 下载超时，重试即可；插件保留原始输出便于诊断。
- 基线：Worker 191/191、Playnite 281/281；Release 0 warning/0 error。

## 2026-08-15 UI-REAL-HOST-AUDIT-NESTED-TABS-THEMES 实施完成

- 真机审计已覆盖嵌套 Tab（如“异常与审计”→“审计记录”）、浅色/深色双主题、5 档窗口尺寸；整页截图渲染完整 Dashboard 外壳并按内容高度撑高。
- 关键实现：递归捕获嵌套 TabControl（ApplicationIdle 等待 + 视觉树/Content 双路径）；`DashboardView`/`GameSaveCenterSettingsView` 提供 `ApplyThemeForAudit`；设置兜底注入真实 Settings DataContext。
- 最新产物：`artifacts/ui-host-audit/screenshots/<size>/<light|dark>/` 与 `artifacts/GameSaveCenter-ui-host-audit.zip`。

## 2026-08-15 UI-REAL-HOST-AUDIT-MULTI-SIZE 实施完成

- 真机审计现覆盖 5 档窗口尺寸（maximized + 1600x1000/1366x768/1280x720/1024x768），每档包含 6 个工作区、全部内层 Tab、窗口截图与 Settings 5 分类；产物在 `artifacts/ui-host-audit/`。
- 修复要点：Playnite DPI-unaware，窗口尺寸用 `SystemParameters.WorkArea` + `SizeToContent.Manual`；内容按逻辑分辨率输出防 OOM；多尺寸扫描不做全页滚动拼接。
- 文件清理规则已加入 AGENTS.md 与本文档：每轮完成后清理旧构建/旧审计/旧 zip 与 `.tmp` 一次性目录，只保留当前安装目录和审计证据。

## 2026-08-15 UI-REAL-HOST-AUDIT-FULL-COVERAGE 实施完成

- 真实宿主审计修复收口：Dashboard 6 个 workspace + 全部内层 Tab + 完整窗口截图 + 整页拼接；Settings 5 个分类全部截图。
- 修复要点：无窗口会话使用专用 1440×900 兜底窗口；Settings 兜底使用缓存输出根 + Dashboard UI Dispatcher；设置分类名从 Header 视觉树提取；zip 占用时写唯一文件名。
- 最新证据：`artifacts/ui-host-audit/` 与 `artifacts/GameSaveCenter-ui-host-audit.zip`；DPI 1.5。
- 回归：Release 0 warning/0 error；`validate-source.py`、`check-xaml.ps1`、WPF UI 校验 0 errors；Core 59/59、Worker 190/190、Playnite 281/281。
- 剩余人工项：用户实际 Playnite 窗口、第三方主题、连续缩放下的最终视觉确认。

## 2026-08-15 UI-REAL-HOST-PARITY-CLOSURE 实施完成

- 计划：`docs/ai/REAL_HOST_UI_PARITY_CLOSURE_PLAN.md`；报告：`docs/ai/REAL_HOST_UI_PARITY_CLOSURE_REPORT.md`。
- Tier A 离屏审计与 Tier B 真实宿主审计分层；`scripts/real-host-audit.ps1` 从真实 Dashboard 捕获证据。
- `RealHostUiAuditService`、`UiDiagnosticsExporters`、`AdaptiveThemePaletteContrastGuard` 已加入。
- 本机真实宿主证据已生成；离屏更漂亮的根因是 fallback vs runtime adaptive palette 等环境差异。
- 协定：每轮开发完成后由 Agent 自己 commit 并 push（AGENTS.md 已同步）。
- 基线：Playnite `281/281`；render-qa 全绿；Offscreen UI Audit 0 HIGH/0 MEDIUM/0 fidelity/0 failed routes。

## 2026-08-15 UI-AUDIT11-RESIDUAL-CLOSURE 实施完成

- 计划：`docs/ai/UI_AUDIT11_RESIDUAL_UI_CLOSURE_PLAN.md`；报告：`docs/ai/UI_AUDIT11_RESIDUAL_UI_CLOSURE_REPORT.md`。
- SaveHistory 大小列不再 ellipsis（116 DIP + `SaveSizeValue`）；Device Inspector 改为 Compact/Narrow 详情切换，展开 viewport >= 180 DIP。
- Audit 新增 `SHORT_SEMANTIC_VALUE_TRIMMING` / `INTERACTIVE_INSPECTOR_USABILITY` 失败门禁。
- 基线：Playnite `276/276`；render-qa 11 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/0 fidelity/0 failed routes。
- 真实 Playnite 宿主主题/DPI 125%/150%/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-FIDELITY-CLOSURE-AUDIT10 实施完成

- 计划：`docs/ai/UI_FIDELITY_CLOSURE_AUDIT10_PLAN.md`；报告：`docs/ai/UI_FIDELITY_CLOSURE_AUDIT10_REPORT.md`。
- Maintenance 中间列表头恢复渲染；Media 搜索框可伸展；Settings 分类自动 scroll-into-view；Save History narrow 状态列保留。
- UI Audit 新增 4 类 fidelity 失败门禁，修复前 92 个失败 → 修复后 0。
- 基线：Playnite `273/273`；render-qa 11 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/0 failed routes/0 fidelity。
- 真实 Playnite 宿主主题/DPI 125%/150%/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-POST-TYPOGRAPHY-GEOMETRY-CLOSURE 实施完成

- 计划：`docs/ai/UI_POST_TYPOGRAPHY_GEOMETRY_CLOSURE_PLAN.md`；报告：`docs/ai/UI_POST_TYPOGRAPHY_GEOMETRY_CLOSURE_REPORT.md`。
- 诊断“等级”列 72 → `GscSeverityColumnWidth` 92 DIP；异常审计同列统一 token。
- UI Audit 新增 Text-Fit 检测并作为失败门禁；visual-tree exporter 修复后 175 个 JSON 非空。
- 基线：Playnite `268/268`；render-qa 11 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/0 failed routes/0 TEXT-FIT。
- 真实 Playnite 宿主主题/DPI 125%/150%/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-FINAL-TYPOGRAPHY-RESPONSIVE-CLOSURE 实施完成

- 计划：`docs/ai/UI_TYPOGRAPHY_RESPONSIVE_CLOSURE_PLAN.md`；报告：`docs/ai/UI_TYPOGRAPHY_RESPONSIVE_CLOSURE_REPORT.md`。
- 字体链统一：`GscUiFontFamily`（含 Microsoft YaHei UI fallback）；普通 UI 不再硬编码 Segoe 组合；图标/代码字体保留；按钮默认字重 Medium，Primary SemiBold。
- Settings Compact/Narrow header 收紧，正文 viewport 不再只剩几十 DIP；Save Compare Narrow 主比较区直接可见；Compact Inspector 详情按钮独立操作行；Media 待归类底栏对齐并保留换行。
- 基线：Playnite `266/266`；render-qa 11 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/0 失败路由。
- 真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-FINAL-POLISH-V7.1 实施完成

- v7.1 计划：`docs/ai/UI_FINAL_POLISH_PLAN_V7_1.md`；报告：`docs/ai/UI_FINAL_POLISH_REPORT_V7_1.md`。
- 首页活动行五列、chip 独立列组、Time 右留白；全量 `POSSIBLE_CLIPPING=0`。
- 基线：Playnite `263/263`；render-qa 11 档 + 56 主题 + 7 Resize 全绿；最终 Audit 0 HIGH/0 MEDIUM/0 失败路由。
- 提交：`702b0d5`、`f6f17a8`；Audit ZIP `artifacts/GameSaveCenter-ui-audit.zip`。

## 2026-08-15 UI-FINAL-CLOSURE-V7 实施完成

- v7 计划：`docs/ai/UI_FINAL_CLOSURE_PLAN_V7.md`；报告：`docs/ai/UI_FINAL_CLOSURE_REPORT_V7.md`。
- Audit 子路由可信、六张表 2K 列填率 1.00、Task/Media 纵向 fill 1.00、Maintenance 表头白块清零、Progress 对比与单行 TextBox 指标清零、Task 1040 outer scroll=0。
- 基线：Playnite `263/263`；render-qa 11 档 + 56 主题 + 7 Resize 全绿；最终 Audit 0 HIGH/0 MEDIUM/0 失败路由。
- 最终 Audit ZIP：`artifacts/GameSaveCenter-ui-audit.zip`；提交 `5cd0226`、`58191d5`、`494b402`、`87d0553`、`7eaaacd`。
- 真实 Playnite 主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-FEEDBACK-GLOBAL-ACTIVITY-CHIP-CENTER

- 首页“全局活动”的“备份成功”等结果气泡文字已居中；宽/窄两套 Kind/Result chip 的 TextBlock 统一三向居中，回归断言已锁定。
- 提交：`d962b4d`；Playnite `263/263`、XAML/source 门禁通过，截图 `artifacts/ui-qa/v6-2-shots/`。

## 2026-08-15 UI-TABLE-AND-CHIP-CLOSURE-V6.2 实施完成

- v6.2 计划：`docs/ai/UI_TABLE_AND_CHIP_CLOSURE_PLAN_V6_2.md`；报告：`docs/ai/UI_TABLE_AND_CHIP_CLOSURE_REPORT_V6_2.md`。
- Chip 改圆角矩形（CornerRadius 7）、时间列右留白 20 DIP、Overview 六列 `40|150|*|96|84|112`、SaveCandidate 可信度真实 ProgressBar。
- Maintenance 四个主表取消 460 DIP 上限，Device/Process 布局改 Stretch；2K/4K fill ratio 见报告。
- 基线：Playnite `263/263`；render-qa 11 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/8 EXPECTED INFO。
- v6.2 截图：`artifacts/ui-qa/v6-2-shots/`；命令 `scripts/capture-v6-2-shots.ps1`。
- 提交：`c58b359`、`6a68a59`。真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-OVERNIGHT-CLOSURE-V6 实施完成

- v6 计划：`docs/ai/UI_OVERNIGHT_CLOSURE_PLAN_V6.md`；报告：`docs/ai/UI_OVERNIGHT_CLOSURE_REPORT_V6.md`。
- 页面历史为 Playnite 会话级；Tasks/Maintenance 保持不显示 GamePicker；数字输入根模板修复；全局活动六列表格；Overview/Maintenance/Media 真实父子滚动清零。
- 基线：Playnite `261/261`；render-qa 全绿；UI Audit 0 HIGH/0 MEDIUM/8 INFO/0 TRUE_PARENT_CHILD_SCROLL_CONFLICT。
- 截图：`artifacts/ui-qa/v6-shots/`；命令 `scripts/capture-v6-shots.ps1`。
- 提交：`baa8f72` 及后续提交见 `git log`。真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-OVERNIGHT-FIX-V4 实施完成

- v4 计划：`docs/ai/UI_OVERNIGHT_FIX_PLAN_V4.md`；报告：`docs/ai/UI_OVERNIGHT_FIX_REPORT_V4.md`。
- 折叠统一：`GscDisclosureCard` 独立 chevron 图标区，全项目无旧 `GscExpander`、无尾部 `>`、无折叠体内滚动。
- 维护中心：诊断页二级 Tab（默认 `问题列表`，次项 `诊断概览`），FindingsGrid 独占主工作区。
- 存档中心：备份自动化与模板数值输入全部带 label/unit/helper；首页活动行紧凑化。
- 基线：Core `59/59`、Worker `190/190`、Playnite `255/255`；render-qa 全绿；UI Audit 0 HIGH/0 MEDIUM。
- 截图：`artifacts/ui-qa/v4-shots/`；命令 `scripts/capture-v4-shots.ps1`。
- 提交：`3015182`、`5131e4d`、`0201615`、`5196f4a`、`fc86ecc`。真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-14 UI-VISUAL-REWORK-V3 实施完成

- v3 计划：`docs/ai/UI_VISUAL_REWORK_PLAN_V3.md`；五张用户截图对应问题已全部关闭（Overview 三按钮/保护三层/活动轻量表、Save 状态 Badge、Maintenance 折叠与五列表头）。
- 所有 Expander/Disclosure 统一到 `GscDisclosureCard`，Chevron 独立、整行可点、无尾部 `>`；Media/Save/Task 旧 `GscExpander` 引用已全部替换；Maintenance 主 Disclosure 使用内部有限滚动。
- 功能保真：REMOVE=0，命令/绑定/DataGrid 列/虚拟化/GamePicker HARD LOCK 未改。
- render-qa 新增按钮对齐探针；Overview Hero/当前游戏列按 1:1 布局，1536×864 下三按钮仍同排同高。
- 自动化基线：Release 0 warning/0 error；Core `59/59`、Worker `190/190`、Playnite `253/253`；render-qa 10 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/32 INFO/0 失败路由。
- 提交：`5c3bdae`（计划）、`9ee3660`（Overview）、`e8b8c31`（Save/Maintenance），最终补强提交见 `git log`；v3 截图 `artifacts/ui-qa/v3-shots/`（脚本 `scripts/capture-v3-shots.ps1`），Audit `artifacts/ui-audit/v3-final/AUDIT_SUMMARY.md`。
- 真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`，不得冒充已人工验收。

## 2026-08-14 UI-AUDIT-001 开发专用 UI 自动审计

- 一条命令：`.\scripts\capture-ui-audit.ps1` 或双击 `GameSaveCenter-UI-Audit.cmd`。
- 输出：`artifacts/ui-audit/`（README、UI_MANIFEST、UI_ROUTE_MAP、UI_FIDELITY_MATRIX、LAYOUT_REPORT、AUDIT_SUMMARY、visual-tree、layout、screenshots）与 `artifacts/GameSaveCenter-ui-audit.zip`。
- 静态盘点自动发现未来新增页面/Tab/控件；运行时整页截图覆盖页面级滚动容器，DataGrid/ListBox 内部滚动也从头到底拼接。
- 当前基线：Core `59/59`、Worker `190/190`、Playnite `250/250`；source/XAML/WPF 门禁与 10 档 render-qa 通过；扩档审计 161 快照、0 HIGH/0 MEDIUM/0 失败路由。
- Commit：见当前 `git log -1`。

## 2026-08-14 UI-REFACTOR-V1 扩档验证与 Audit 高度对齐

- `render-qa` 矩阵已覆盖 1040×700、1100×720、1280×720、1366×768、1536×864、1600×900、1707×960、1920×1080、2048×1152、2560×1440，10 档全部通过。
- UI Audit 新增 `2k` 与 `narrow-1100` 尺寸；工作区 `ApplyResponsiveLayout` 现在接收窗口高度（与生产 Dashboard 和 render-qa 一致），不再使用内容高度造成 1100×720 维护设备/进程表 240 DIP 的误报。
- 扩档后最终审计：HIGH `0`、MEDIUM `0`、运行时警告 39、失败路由 `0`。真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-14 UI-REFACTOR-V1 强制 Light/Dark 主题 QA

- RenderHarness 新增 `RunThemeQa`，复用生产 `AdaptiveThemePaletteFactory` 对 7 个工作区 × 1040×700 / 1100×720 / 1366×768 / 2560×1440 × Light/Dark 渲染，共 56 个离屏场景；调色板、主表视口和页面滚动面断言全部通过。
- `GameSaveCenter.Playnite.csproj` 仅向 RenderHarness 开放 `InternalsVisibleTo`，生产 UI 与业务未改。
- 像素采样确认主题切换有效：Light 背景约 `239,240,243`，Dark 约 `54,57,67`。真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。
- render-qa 与主题 QA 同时加入页面级横向溢出门禁：`*ScrollSurface` / `SettingsScroller` 必须 `hbar=Disabled` 且无横向溢出；DataGrid 内部列滚动允许。10 档尺寸与 56 主题场景全部通过。
- `render-qa` 新增 Resize 恢复探针（2560×1440 → 1100×720 → 2560×1440 同实例布局快照对比），修复 Save/Task/Trainer Inspector 宽窗不恢复缺陷，并新增 3 条回归测试。
- 实施包验收审计已落盘到 `docs/ai/UI_REFACTOR_ACCEPTANCE_AUDIT.md`；真实 Playnite 宿主主题/DPI/连续缩放与大数据滚动仍为 `MANUAL QA REQUIRED`。
- 真实宿主 reload 已验证：`dev-install-run.ps1 -Configuration Release` 成功，Playnite 加载 `GameSaveCenter 0.6.70`，扩展日志确认 `0.6.70.0 loaded`，Worker 从当前扩展目录运行，`18:10` 后无 ERROR/Exception/crash。
- Visual Correction v2 已实施：Overview 单滚动、风险卡去内滚、Disclosure、活动行响应式、Save 卡片、Diagnostics 单滚动、Audit 二级切换；新增 OV/SAVE/MAINT 断言，最终 Audit 0 HIGH/0 MEDIUM/33 运行时警告。
- Visual Correction v2 真实宿主 reload 已验证：`20:39:57` 插件加载、`20:39:59` 扩展初始化、Worker 从当前扩展目录运行，之后无 ERROR/Exception/crash。

## 2026-08-14 Final Code Gap Closure 与最终 Epic 状态

- 当前基线：Core `59/59`、Worker `190/190`、Playnite `235/235`；Release 构建 0 warnings / 0 errors；source、XAML、WPF 静态门禁和 `render-qa` 通过；fault-injection 与 1000 轮 soak 通过。
- 已交付：多设备强内容指纹/保守分歧判断、可取消的流式 Restore Readiness、按实际目录分盘检查、重复 Manifest 防护、结构化且集中脱敏的诊断包、`ImportantOnly/Summary/Verbose` 通知级别、带启动失败自动请求与恢复正常按钮的安全模式、覆盖孤儿归档/Manifest/磁盘空间/四态结果的完整性自检、旧版数据库 Fixture 升级 Harness、自身元数据灾备 ZIP 与安全恢复流程、带只读预览与确认的备份仓库索引重建、带预览/目标缺失策略/自动灾备的批量路径迁移、按 Worker 会话与任务类型分类的中断任务协调、带显式兼容矩阵的单游戏操作锁、带 AppVersion 与能力列表的 IPC protocol handshake、覆盖设置/媒体/元数据恢复/启动计数且旧文件完整保留的共享原子写入器、数据规模与资源监控 Soak Harness、覆盖 ZIP/数据库/原子写/外部进程/任务/广播/锁的 15 类故障注入 Harness、通知级别正式收口与同 Session 去重测试、未分类自定义启动项反作弊授权语义、Onboarding 测试备份真实生产链路审计、只读备份存储分析、全局保留策略模拟器、第二本地镜像、首页全局活动时间线、Playnite 游戏右键快捷操作、修改器中心拖拽导入、合理 UI 状态持久化、键盘快捷搜索与自动化名称、统一页面状态控件、设置即时验证摘要、用户可读维护健康报告（复制/导出 TXT/Markdown）、本轮四项缺口（Repository Rebuild 空库灾难恢复、元数据灾备含 Playnite 插件设置、Workspace 状态控件覆盖存档/工具/媒体/维护、Local Mirror SHA256 内容校验），以及 Metadata 跨进程原子回滚与状态控件 TargetType 崩溃修复。
- 真实开发安装已通过：Playnite 扩展 `0.6.70` 加载，Worker 从当前扩展目录启动并记录 `Application started`。安装器仍默认不请求管理员权限；沙箱无法写入 Playnite 扩展目录时，需在正常用户环境或获授权外部环境运行安装器。
- 当前阶段 commit：`cbd0cdb`、`c6bfda2`、通知级别/安全模式/完整性自检/迁移 Harness/元数据灾备/索引重建/路径迁移/任务协调/操作锁/握手提交见紧随其后的 commit；原子写入提交为 `f1eda41`，稳定性测试提交为 `8132419`，故障注入提交为 `f91992e`，A-HARDEN 提交为 `fbbdb90/8d6c36c/5666944`，B01+B02 升级提交为 `17e88b6`，B03 提交为 `a064108`，B04 提交为 `55f532f`，B05 提交为 `97b06b5`，B06 提交为 `dff0cf4`，B07 提交为 `bddcfdd`，B08 提交为 `47685bd`，B09 提交为 `7dba09c`，B10 提交为 `7ee738d`，B11 提交为 `eed946c`，B12 提交为 `c9e053a`，B13 提交为 `738f339`，Layer B Audit 提交为 `25b191f`，C01 提交为 `2ef8f13`，C02 提交为 `a27f430`，C03 提交为 `387149f`，C04 提交为 `7f38b15`，C05 提交为 `15ab1d7`，C06 提交为 `5d113c3`，C07 提交为 `e4513cb`，C08 提交为 `c907983`，C09 提交为 `f21cba4`，C10 提交为 `4614bb8`，C11 提交为 `9d465d2`；Final Code Gap Closure 提交为 `e58714c`、`0bcdce2`、`84f74fc`、`5a12d1e`；崩溃修复与 Metadata 原子回滚提交为 `a417b7c`、`13f21a5`。Layer B 13 项与 Layer C 11 项已全部交付；`PRODUCT_HARDENING_LAYER_C_AUDIT.md` 与 `PRODUCT_HARDENING_EPIC_FINAL_AUDIT.md` 已生成。
- 整体 Epic 状态：`PARTIALLY COMPLETED / MANUAL QA REQUIRED`；按 `docs/ai/FINAL_MANUAL_QA_CHECKLIST.md` 执行真实 Restore/Undo、Rclone、双设备、外置镜像、启动项、反作弊、主题/DPI/连续缩放与 1000+ 游戏库人工验收。
- `AUTO VERIFIED` 与 `MANUAL QA REQUIRED` 必须分开记录。真实 Restore/Undo、Rclone 断网、双设备、EXE/LNK/BAT/PS1、1000+ 游戏库、主题/DPI/连续缩放仍需人工验收。

## 2026-08-13 UI-QA-REAL-006 设置分类 Tab 实际裁切修复

- 上一轮分类栏底部安全区只改变了外层 extent，未改变 `TabItem` Chrome 贴住 `TabPanel` 布局槽的问题；真实截图中的每张分类卡底边仍像被一条水平边界切平。
- 共享 `GscRedesignSettingsTabItem` 现使用不裁切的 `TabItemRoot` + 独立圆角 Chrome，Chrome 顶部对齐并留出 2 DIP 底部安全距离，移除 Chrome 的 `ClipToBounds=True`。
- `GscRedesignSettingsTabControl` 现用 `SettingsHeaderBottomSafetyZone` 真实占位元素增加分类滚动内容 extent；顶部横向模式折叠占位元素。RenderHarness 会检查最后 Tab 与 Chrome 的底部几何及安全距离。
- 已验证：XAML/source 门禁、Playnite `210/210`、设置页五种窗口 `render-qa OK`；仍需真实 Playnite 宿主主题/DPI/连续缩放人工验收。

## 接手时的最短指令

以后可以直接对新的 agent 说：

```text
请先读取 GameSaveCenter 项目的 docs/DEVELOPMENT_HANDOFF.md，按照其中的读取顺序、不可丢失约束和验证要求恢复项目上下文。当前代码侧已收口：除非用户提供新的真实问题、日志或明确新需求，否则不要主动开启新的 UI 重构或性能优化；不要重置或覆盖已有改动，先检查 `git status`，完成后更新项目记忆与工作日志并提交 commit。
```

## 2026-08-13 DEV-INSTALL-007：兼容未发现 Playnite 路径

- 另一台机器的一键安装日志显示，失败发生在构建前：Playnite 使用自定义/便携安装路径时没有发现任何 `Playnite.DesktopApp.exe`，空候选数组绑定到 `TrustedPlayniteExecutables` 后被 PowerShell 拒绝；不是编译失败。
- 当前安装器增加 App Paths、PATH 和规范化路径发现，并允许可信 Playnite 候选为空。没有运行中的 Playnite 时继续构建、打包和安装；安装完成后只提示无法自动启动 Playnite。可使用 `-PlayniteExecutable "D:\\实际路径\\Playnite.DesktopApp.exe"` 支持自定义路径。
- 如果 Playnite 仍在运行但路径不可确认，安装器仍安全停止并要求手动退出或显式指定路径，不按进程名强杀未知进程。
- 根目录入口修订号为 `DEV-INSTALL-007`；真实另一台机器的便携版、自定义路径和完全未启动 Playnite 场景仍需用户手工验证。

## 2026-08-13 UI-QA-REAL-005：首页与设置页几何兼容性修复

- 首页宽屏右侧 `今日概览` 已在 XAML 与响应式代码双重设为顶端对齐；离屏几何探针在 1600/1920 DIP 宽屏下 `OverviewSecondaryTopDelta=0`。
- Hero/当前游戏宽屏比例调整为 `1.1* + 0.9*`，离屏报告当前游戏卡相对 Hero 约 `0.82`，改善 4K/大窗口下按钮与指标的拥挤；没有改变业务命令、Binding 或窄屏堆叠阈值。
- 设置分类共享模板增加 `SettingsHeaderItemsHost` 底部安全区，且分类 Chrome 使用像素对齐和布局取整；末项滚动到底部后仍完整落在 viewport 内，五个分类都保持可见/可滚动。
- RenderHarness 在捕获设置图前将 `SettingsShell.Opacity` 设为 1，避免入口动画未由真实宿主触发时生成空白图；新增三类几何门禁。
- 当前验证基线：Release 隔离构建 0 warnings/0 errors；Core `42/42`、Worker `117/117`、Playnite `210/210`；五种常用窗口的 `render-qa` 全绿。真实 Playnite 宿主 Light/Dark/Follow、高对比度、100%–200% DPI 和连续缩放仍需人工验收。

## 2026-08-13 DEV-INSTALL-006：无窗口 Playnite 残留回收

- 真实复现 PID 48188：Playnite 已无主窗口但进程残留，`CloseMainWindow()` 无法请求退出，旧安装器等待 20 秒后中止。这不是管理员权限问题。
- 当前安装器仍先请求正常退出；超时后仅当进程属于当前会话、路径与本次发现的 Playnite 可执行文件完全一致且 `MainWindowHandle=0` 时，才结束该残留实例。路径不可确认、跨会话或仍有主窗口时继续拒绝强制结束；PID 自然退出竞态不再误报。
- `DEV-INSTALL-006` 已在原失败场景完整通过：Release 0 警告/0 错误，Core 42/42、Worker 117/117、Playnite 203/203，普通用户安装并启动；当前安装 DLL 标识 `0.6.70+4125a5448b1d903c1122d6ba596b8ca31597a714`。Playnite 11:14:33 日志确认插件加载，Worker 11:14:38 日志确认初始化并启动。

## 2026-08-13 DEV-INSTALL-005：构建隔离与真实宿主验证

- 用户日志中的 Contracts 编译成功；失败发生在后续测试覆盖标准 `bin\Release` 时，原因是旧 `dotnet/testhost` 或 Worker 文件锁。当前一键入口 `GameSaveCenter-Run.cmd` 对应 `DEV-INSTALL-005`，每次构建使用唯一 `artifacts\dev-build\<Configuration>\<guid>`，不清理标准输出，也不默认请求 UAC。
- `scripts/dev-install-run.ps1 -Configuration Release -NoStart -SkipClean` 已在正常桌面文件权限下完整通过；当前受限 Codex 沙箱对 Playnite 用户扩展目录的写入失败不代表脚本失败。遇到旧残留时应先正常关闭 Playnite，让插件回收其所属 Worker；不要按进程名强杀，也不要把管理员提权作为默认修复。
- 真实启动证据：`C:\Users\lopmatu\AppData\Roaming\Playnite\playnite.log` 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`；插件自身日志记录 `GameSaveCenter.Playnite 0.6.70.0`；`C:\Users\lopmatu\AppData\Local\GameSaveCenter\Logs\worker-launch.log` 记录 Worker 存储初始化后进入 `Application started`。安装报告为 `artifacts\last-dev-install.txt`。
- 当前自动化基线：Core `37/37`、Worker `81/81`、Playnite `202/202`，Release 构建 0 warnings / 0 errors。真实主题/DPI/键盘、Rclone/多设备、游戏恢复/撤销、EXE/LNK/BAT/PS1 和 900+ 游戏库仍需人工 QA。

## 必须读取的资料

按以下顺序读取：

1. `AGENTS.md`
2. `docs/DEVELOPMENT_HANDOFF.md`（本文件）
3. `docs/PROJECT_MEMORY.md`：长期不可丢失约束、已完成 UI 决策和性能边界
4. `docs/DEVELOPMENT_PROGRESS.md`：按 UI 编号排列的实施历史和下一步线索
5. `docs/design/APPLE_WPF_IMPLEMENTATION_PROMPT.md`：总体设计方向
6. `docs/design/UI_CHANGE_GATE.md`：每次 UI 变更的门禁与验收标准
7. `.codex/skills/wpf-apple-desktop-ui/SKILL.md`：WPF/Playnite UI 专项技能，随仓库提交；本机同时安装于 `%USERPROFILE%\.codex\skills\wpf-apple-desktop-ui`。做任何 WPF/XAML 改动前先完整读取，并按任务需要读取 `references/` 中的对应文档
8. `C:\Users\lopmatuse\.codex\attachments\1b6b382f-30ed-44c7-a9ce-6c580fefbe83\pasted-text.txt`：用户提供的完整任务提示词附件；如果新电脑不存在该路径，以本文件和仓库内文档为准
9. `D:\workplace\Github\GameSaveCenter.WpfUiDemo.v3.1`：WPF Demo 模板，比较布局层级、节奏、控件尺寸和交互表面，不复制 Demo 假数据或业务实现；当前本机实际可用副本为 `D:\workplace\VSCode\GameSaveCenter.WpfUiDemo.v3.1`

如果附件路径发生变化，先在当前对话的附件中找到同一份完整提示词；不能因为附件不可用而跳过仓库内的规则和约束。

## Codex 2026-08-11/12 阶段补充（性能与自定义启动项）

## Codex 2026-08-13 阶段补充（环境准备与 GameTool 安全策略）

ONBOARDING-001 已在既有 Maintenance 诊断页加入首次环境准备入口；GAME-TOOL-003/004 已在既有 TrainerCenter Inspector 加入 CustomExecutable 的已有实例策略和风险分类。当前测试基线为 Worker 74/74、Playnite 200/200；Worker/Playnite Release 构建均为 0 警告、0 错误，WPF 静态校验与离屏 render QA 通过。

GameTool 的安全边界不可丢失：不能按进程名直接关闭程序；只在完整 EXE 路径一致且重新确认 PID 路径后执行 Restart；无法读取路径时保守拒绝。反作弊风险游戏中只允许 `GeneralUtility` 自定义工具自动启动，Unknown/Trainer/CT/GameModification 必须阻止并审计。真实 Playnite 覆盖安装、扩展扫描和 Worker/IPC 加载仍受 PID 3896 文件锁限制，清理 PID 后需要重新执行一键安装并检查 `playnite.log` 与 `extensions.log`。

## Codex 2026-08-13 阶段补充（SMART-PROTECT-001/002）

SMART-PROTECT 已完成第一版智能保护闭环：游戏停止请求等待现有存档识别，识别到候选/接受候选/Ludusavi 匹配后在既有 Dashboard 对话框显示“启用推荐策略 / 以后再说 / 不再提醒”三选一；没有识别到存档只写审计，不打扰用户。提示状态与最近识别结果保存在 SQLite，`Deferred` 冷却 7 天，`Enabled` 和 `Dismissed` 不再重复提示。因为识别最长约 2 分钟，Playnite 的 `session.stopped` 请求使用 3 分钟专用超时。

Overview 既有最近保护卡已扩展为最近窗口游戏状态列表：`已保护`、`未匹配`、`存档未保护`、`风险`。已保护项不可选，其余项可多选，通过 `protection.recommended.apply` 批量启用退出后与游玩中自动保护，并写入审计后刷新快照。不要把这个能力移到新主导航页，也不能把“启用推荐”实现成自动恢复或删除操作。

本阶段验证：Core 29/29、Worker 76/76、Playnite 202/202；Worker/Playnite Release 构建 0 警告、0 错误；`validate-source.py`、`check-xaml.ps1`、WPF UI 契约测试与 `render-qa.ps1` 全部通过。测试使用 `artifacts/smart-test/final-*` 隔离输出；标准测试目录仍被早先宿主锁住时，不要删除或强杀不属于本次任务的进程。

验证边界不变：当前会话无法完成真实最终包的 Playnite 扩展扫描；真实宿主日志仍需用户复核。开发安装器不默认请求管理员权限，而是先正常关闭 Playnite、等待插件回收其 Worker，再按扩展路径处理残留。隔离启动/离屏渲染只能证明应用层启动和布局，不得写成“真实宿主加载成功”。

## Codex 2026-08-13 阶段补充（NOTIFY-001 / MULTI-DEVICE-001 / RCLONE-RELIABILITY-001）

退出任务的 `TaskStatusDto.SessionId` 是通知聚合的唯一业务关联；备份与媒体任务必须使用退出会话的 SessionId，Playnite 只在该会话预期任务全部进入终态后发一条摘要。摘要不能另造成功判断，必须复用任务中心终态；云端失败时保留“本地备份完成”事实并给出重试提示。

多设备摘要的 `ParentBackupId` 用于识别共同基线：同父版本的两个不同子版本标记 `DivergedFromCommonBase`，线性父子关系不标记冲突。`PreferLocal`、`PreferRemote`、`KeepBoth` 只持久化用户决定；不得自动合并、自动选择、删除远端或绕过 PreRestore → Restore → Validate → Rollback。

Rclone 可靠性仍是单向安全适配器，只允许 `copy`、`check`、`lsf`、`cat`、`version`。`RcloneFailureClassifier` 将认证、权限、远端不存在、网络和不完整传输转换为稳定错误码；只有网络/不完整传输进入有限退避，凭据/权限类错误不能无限重试。本地历史不得因远端缺失而删除。

当前自动验证基线：Core 35/35、Worker 81/81、Playnite 202/202；三者 Release 构建均 0 警告/0 错误；source、XAML、WPF 静态门禁通过。真实 Rclone、两台设备、真实恢复和最终 Playnite 宿主加载仍为 `MANUAL QA REQUIRED`。开发安装的 Playnite/Worker 回收链路已改为普通权限正常关闭优先。

新的 AI/Codex 长期记忆入口已建立：先读 `docs/ai/PROJECT_MEMORY.md` 与 `docs/ai/WORKLOG.md`，再读本文件。

本轮已完成并推送：

- PERF-004：`[PERF]` 性能基线日志 + `docs/ai/PERFORMANCE_BASELINE.md`。
- PERF-005：Snapshot 无变化 0 CollectionChanged（`SnapshotComparers` + `BatchObservableCollection.ReplaceAll`）。
- PERF-006：Task/Media 搜索 180ms 防抖（`DebouncedRefresh`）。
- GAME-TOOL-001/002：自定义启动项 EXE/LNK/BAT/CMD/PS1，外部路径引用不复制文件；`GameToolLauncher` 按类型启动；`GameToolSessionTracker` 只按 Session/PID/StartTime 关闭本会话进程。
- PERF-007：媒体缩略图异步化（`AsyncThumbnailLoader` 3 并发 + LRU + Freeze，`AsyncThumbnailImage`）。
- UI-QA-REAL-001：隔离 Playnite 真机冒烟通过，截图在 `artifacts/ui-qa/real/playnite-real.png`；主题/DPI/键盘/缩放与自定义启动项真机流程仍待用户复核。
- UI-206 初始方案（SUPERSEDED）：提交 `962a6b0` 曾把共享与关键 DataGrid Style 改为 `VirtualizingPanel.ScrollUnit=Pixel`，并把 Maintenance/Task 表格由强制 `Height` 改为 `Height=double.NaN + MaxHeight`；该 Pixel 方案已由真实 Playnite A/B 验证为回归并撤回，仅保留历史记录，不作为当前方案。
- DataGrid 最终结论（`d9cd82f`/`0ce3388`/`4564c8f`）：Pixel ScrollUnit 经真实 Playnite A/B 验证会回归，已撤回；当前采用 `Item` + `GscStableDataGridRow` 稳定行样式 + geometry probe（gap ≤4 DIP、末行完整、Recycling 保持）。不要重新改回 Pixel。

本机最近两个本地提交（未推送）：`e86e461 docs: record UI-207 revalidation baseline`、`d45f65c feat: harden restore readiness and recovery drills`。UI-207 的设置布局、运行中游戏自动定位、上次选择恢复、GamePicker 新用户默认“已安装”和当前游戏真实 Icon 已由 `d2662e3` 收口；Restore Readiness 的严格 Manifest 校验与恢复灾难演练已由 `d45f65c` 收口。

当前测试基线：Core 27、Worker 67、Playnite 197；本阶段 Worker Release 构建 0 警告/0 错误，Restore Readiness/恢复演练 67/67；上一阶段 render-qa 全绿，源码验证与技能静态审查通过。下一阶段为 POLICY-001；不要重复打开已完成的 UI-207、RELIABILITY-RESTORE-001、HEALTH-001、PROTECTION-001、PERF-004～010 与 GAME-TOOL-001/002。

## 项目目标

这是已有的 Playnite 插件项目 GameSaveCenter 的持续 UI 重构，不是新建功能，也不是只改某一个 `Margin` 或颜色。

生产 UI 使用现有的 WPF/C# 技术栈，最终视觉和信息层级应接近 `GameSaveCenter.WpfUiDemo.v3.1`。Demo 只是 UI 模板；生产页面必须继续使用真实数据、真实命令和真实状态。

必须长期保持：

- 不改插件 ID、业务服务、Worker、IPC、数据库、持久化和任务协议，除非用户明确要求并单独批准。
- 保留现有 ViewModel、命令、Binding、`x:Name`、真实状态流、错误反馈和 Playnite 生命周期。
- 修复共享资源、样式、ControlTemplate 和页面结构，不用局部补丁掩盖共享控件问题。
- 保留大型列表的有限测量、内部滚动、键盘访问、UI Automation 和虚拟化。
- 不使用 HTML、WebView、Electron、Avalonia、WinUI 或截图替代原生 WPF。
- 不使用 Demo 假数据，不用 `Task.Delay` 模拟业务成功，不把视觉状态伪造成业务结果。

## 用户明确提出的 UI 细节

这些细节不是可选的视觉偏好，而是持续验收标准：

- 同类按钮的宽度/高度、文字方向、文字垂直和水平位置必须一致。
- 文本不能因为共享模板硬编码对齐而偏离调用方意图；需要修复共享模板的 `TemplateBinding`。
- 下拉框初始必须显示明确的默认值。例如初始筛选是“全部”，控件就必须显示“全部”。
- 对于依赖真实上下文的动态下拉框，空值可能代表“等待真实上下文”，不能为了视觉而强行选择第一项。
- 页面在 100%/125%/150%/175%/200% DPI、不同窗口尺寸、Light/Dark/Follow Playnite/高对比度下不能出现重叠、裁切、文字不可读或操作入口消失。

## 每次开发的固定流程

1. 先执行 `git status`、`git branch --show-current` 和最近提交检查；不得 `reset --hard`、`checkout --` 或覆盖别的电脑留下的未提交改动。
2. 读取本文件、`PROJECT_MEMORY.md`、`DEVELOPMENT_PROGRESS.md` 中与目标页面相关的最新条目。
3. 搜索目标控件的全部共享样式、模板、资源和调用点，先判断根因属于信息架构、布局测量、模板状态、可读性、可访问性、性能还是宿主兼容性。
4. 按 UI Change Gate 实施小范围、可验证的 UI 改动；优先共享资源和结构修复，保持业务合同不变。
5. 至少运行适用的静态校验（含 `python .codex/skills/wpf-apple-desktop-ui/scripts/validate_wpf_ui.py .`）、`git diff --check`、源码验证、WPF 结构测试、Debug/Release 构建和相关单元测试。
6. 真实 Playnite、主题、DPI、键盘或宿主渲染没有实际运行时，必须明确写“尚未验证”，不能声称已经验证。
7. 完成一轮后同步更新：
   - `docs/PROJECT_MEMORY.md`：新增不可丢失的结构/行为约束
   - `docs/DEVELOPMENT_PROGRESS.md`：记录 UI 编号、修改文件、保留的命令/绑定、验证结果和未完成的宿主验证
   - 本文件的“当前交接基线”和“下一步方向”
8. 每次有实际开发改动都必须创建一个清晰的 Git commit。提交前确认工作区没有意外文件。

## 合并后当前交接基线（2026-08-11）

- 分支：`main`
- 当前 UI 交接基线：本轮本地提交 `界面：完善设置布局与当前游戏上下文`（hash 以 `git log -1` 为准；本任务不 push）；生产 UI 最近相关提交 `0ce3388`（DataGrid 稳定行样式 + geometry probe）与 `4564c8f`（诊断摘要非裁剪）。DataGrid 最终采用 `Item + GscStableDataGridRow`，Pixel 已真机验证并撤回。
- UI-207（本轮）：设置页 Header 不裁剪、分类栏宽/窄滚动与选中 BringIntoView；打开 Dashboard 自动定位运行中游戏（否则恢复上次选择/首个已安装），GameStarted 事件驱动、普通刷新不抢回；GamePicker 新用户默认“已安装”；当前游戏显示真实 Playnite Icon（UI-only provider，LRU 48，失败 fallback 手柄 glyph）。当前复验基线为 Core 27 / Worker 59 / Playnite 197、render-qa 全绿；真实设置页/自动定位/Icon 与 1080p/4K/DPI 人工验收标记 BLOCKED_ENVIRONMENT。
- 上一合并提交：`e87e2af`（`merge: reconcile local and cross-machine UI migration`）
- 合并共同基线：`9cdd975`；本机 UI-173～UI-181 与 `origin/main` 的 UI-181～UI-183、交接文档线均已保留，没有删除任一方共同基线后的提交。
- 本机额外 WIP 已先由 `e61d0fc` 固化后纳入合并；本机的长期约束已追加到 `docs/PROJECT_MEMORY.md` 的 `MERGE-001`，远端既有记忆条目保持原文。
- UI-184 已将 Overview 的 Demo 层级落地为“今日工作台动作卡 → Hero/当前游戏双列（受限宽度堆叠）→ 六项指标 → 最近活动”，具体约束见 `docs/PROJECT_MEMORY.md` 与 `docs/DEVELOPMENT_PROGRESS.md`。
- UI-185 已将 SaveCenter 的候选路径页补齐为“当前规则与校验 → 候选表/Inspector”，页签改为“路径与校验”；共享 Dashboard 游戏上下文不重复渲染，真实 `SelectedGame` 状态和扫描/校验/刷新命令保持绑定，具体约束见 `docs/PROJECT_MEMORY.md` 与 `docs/DEVELOPMENT_PROGRESS.md`。
- UI-186 已将 TaskCenter 摘要卡改为 Demo 的“任务总数 / 运行中 / 需要重试 / 已完成”四项真实任务状态计数，宽屏四列、窄屏按两列/单列收缩；任务筛选、全局视角、详情 Inspector 和恢复命令未改变。
- UI-187 已将 Maintenance 诊断页顶部摘要改为 Demo 的六项真实健康卡（Worker、Ludusavi、Rclone、数据与备份目录、媒体目录、设备状态），并将响应式列数收口为宽屏 3 列、中屏 2 列、窄屏 1 列；诊断操作、表格、Inspector、审计、完整摘要和原有命令/绑定均保留。
- UI-188 已将 TaskCenter 任务队列补齐 Demo 的搜索输入框，真实搜索任务 ID、类型、游戏、详情和错误，并与状态/游戏/类型筛选叠加；未新增 Worker/IPC 请求，任务计数、Inspector、恢复命令和虚拟化保持不变。
- UI-189 已将 TaskCenter 顶部四项任务摘要改为 Demo 的“标题 → 30px 数值 → 副文案”三行阅读卡，仍绑定真实任务计数并保持四列/两列/单列响应式逻辑；搜索、筛选、Inspector、命令和虚拟化未改变。
- UI-190 已修复常用窗口尺寸下 Overview 底部内容被截断、Media 表格只剩一行和 Maintenance 诊断下方内容不可达的问题：Overview 只滚动上方工作台内容并让最近活动保持有限 Grid/ListBox 视口；Media 与 Maintenance 使用明确命名的页面滚动面承载下方内容，主表/主列表由 code-behind 保持 236–460 DIP 有限高度和内部虚拟化滚动。真实命令、Binding、Inspector 和业务层未改变。
- UI-191 已修复 SaveCenter/TrainerCenter 窄宽度或低高度堆叠 Inspector 时主列表被挤成一行的问题：历史版本/候选路径 DataGrid、已安装工具/FLiNG 搜索结果/可下载版本 ListBox 区域保持 236 DIP 最小视口；Inspector 高度按实际布局剩余空间计算并继续使用自身滚动。真实命令、Binding、选中项、导入确认、虚拟化和 Recycling 未改变。
- UI-192 已修复 TaskCenter 窄宽度或低高度堆叠详情 Inspector 时任务主表被挤压的问题：任务队列保留 236 DIP 最小视口，详情高度按摘要区、筛选区和主表剩余空间计算，搜索/筛选、真实计数、取消/重试/复制命令和虚拟化未改变。
- UI-193 已修复 Maintenance 设备状态、异常审计和进程映射页窄宽度/低高度堆叠 Inspector 时主表被挤压的问题：诊断、设备、审计发现和进程映射表统一保留 236 DIP 最小视口；设备/审计 Inspector 按实际布局剩余空间限高并继续内部滚动，进程映射表接入显式结构标识和共享表头主题加载，命令、Binding、选中项、审计日志和虚拟化未改变。
- UI-194 已将共享页面级滚动契约与 Demo 对齐：`GscPageScrollViewer` 默认垂直 `Auto`，设置页、存档策略、维护保留策略、侧栏导航和 Overview 辅助页面不再用 `Hidden` 掩盖溢出；Overview 宽屏/堆叠的有限内部滚动仍由 code-behind 分工控制，表格/列表虚拟化未改变。
- UI-195 已补齐 Maintenance“设备状态 / 异常与审计 / 进程映射”三个 Tab 的命名页面滚动面；对应三张主表由 code-behind 使用 236–460 DIP 有限 Height，继续保留 DataGrid 内部滚动、虚拟化、Inspector 滚动、真实命令和 Binding，源码门禁同步识别该结构。
- UI-196 已补齐 TaskCenter 的命名页面滚动面 `TaskPageScrollSurface`；任务表由 code-behind 使用 236–460 DIP 有限 Height，堆叠 Inspector 按页面实际视口计算剩余高度，摘要、筛选、任务表、详情和底部恢复操作在短高度常用窗口下均可通过明确滚动访问，真实搜索/筛选/计数/Binding/命令/虚拟化未改变。
- UI-197 已修复截图暴露的常用窗口可见性问题：TaskCenter 动态游戏/类型筛选的 WPF 集合刷新空选中在 DataBind 优先级恢复为 `全部`，MediaGrid 通过共享 ListBox 顶部内容契约避免少量媒体卡沉到有限视口底部，Overview 工作台按钮在窄主列第二行横向自动换行，宽屏右侧摘要/风险列保留有限 Auto 滚动以确保“打开维护中心”可达；真实命令、Binding、Worker、IPC、数据库、持久化、虚拟化、键盘访问和 Automation 未改变。
- UI-198 已修复 Overview 在常用窗口和窄窗口下的主工作区被 sibling 行挤压问题：工作台、Hero/当前游戏、六项指标和最近活动统一进入 `OverviewPrimaryScrollSurface`，窄布局再由 `OverviewStackScrollSurface` 统一承载主列与右侧摘要，避免 980 DIP 下主列高度变成 0；宽布局仍保留主列/摘要列独立有限滚动。`OverviewActivityList` 继续使用有限高度、ListBox Recycling 和自身滚动，真实命令、Binding、SelectedTask、键盘访问和 Automation 未改变。已按 1600/1366/1280/1100/980 DIP 与 900/768/720/700/640 DIP 运行隔离生产离屏渲染，源码验证通过，生产插件 Release 构建 0 警告/0 错误，隔离测试 149/149 通过；未运行真实 Playnite 宿主、主题切换、DPI 真机和连续缩放流畅性验证。
- UI-199（代码提交 `5cbd512`）已修复工作区由程序化导航、恢复状态或离屏渲染直接切换时顶栏仍显示“首页”的语义不同步：`DashboardView.UpdateWorkspacePresentation()` 与侧栏点击共同调用 `UpdateWorkspaceHeader`，媒体/维护/任务等页面标题和副标题始终跟随当前可见工作区。MediaCenter 的摘要卡响应式断点改为逻辑 DIP 的 `>=760` 四列、`>=520` 两列、其余单列，使 Dashboard 在常用 1080p/2K/4K 窗口下保持 Demo 四卡横排并为主表保留可见行；表格有限视口、内部滚动、虚拟化、Inspector、真实命令和 Binding 未改变。源码验证通过；生产插件 Release 构建 0 警告/0 错误；隔离 WPF 测试 150/150 通过；生产离屏渲染覆盖 1600/1366/1280/1100/980 DIP 与 900/768/720/700/640 DIP 并返回 `render-prod OK`。Render harness 自身仍有 3 个 FakeApi 未使用事件警告；真实 Playnite 宿主、主题切换、DPI 真机和连续缩放流畅性尚未验证。
- UI-200（代码提交 `f11e9b7`）已将 Demo 的 `MinWidth=1040`、`MinHeight=700` DIP 固化为生产外壳的常用最小窗口：Dashboard `>=1040` 保留带文字侧栏和单行顶栏，低于该值才进入图标紧凑壳；同时按外壳扣除侧栏后的约 700 DIP 页面宽度校准 Media `>=700` 四列、Task `>=900` 四列/`>=680` 两列、Maintenance `>=980` 三列/`>=680` 两列。1040×700 离屏结果为 Media 四卡并显示两行表格、Task 2×2 摘要并显示队列、Maintenance 两列健康卡；1366×768 仍为完整多列，页面级滚动、表格/列表有限视口、内部滚动、虚拟化、真实命令/Binding 和业务层未改。源码验证通过；生产插件 Release 构建 0 警告/0 错误；隔离 WPF 测试 151/151 通过；生产离屏渲染覆盖 1600/1366/1280/1100/1040/980 DIP 与 900/768/720/700/640 DIP 并返回 `render-prod OK`。Render harness 自身仍有 3 个 FakeApi 未使用事件警告；真实 Playnite 宿主、主题切换、DPI 真机和连续缩放流畅性尚未验证。
- SKILL-001（本轮）：`wpf-apple-desktop-ui` 技能已随仓库提交到 `.codex/skills/wpf-apple-desktop-ui/`，并安装到本机 `%USERPROFILE%\.codex\skills\wpf-apple-desktop-ui`；AGENTS.md、DEVELOPMENT_HANDOFF.md、UI_CHANGE_GATE.md、PROJECT_MEMORY.md 与 DEVELOPMENT_PROGRESS.md 已同步仓库内技能路径，UI 门禁新增 `python .codex/skills/wpf-apple-desktop-ui/scripts/validate_wpf_ui.py .` 静态审查。
- QA-001（本轮）：新增可复用的离屏渲染 QA：`tests/GameSaveCenter.RenderHarness`（假数据，不启动 Worker/IPC）与 `scripts/render-qa.ps1`，覆盖 1040×700、1280×720、1366×768、1600×900、1920×1080 逻辑窗口，输出 PNG 与 `artifacts/ui-qa/render/render-qa-report.txt`（页面滚动面、DataGrid/ListBox 有限视口尺寸、可滚动性）。1040×700 复核结果：Media 待归类/当前游戏媒体主表 350 DIP 高、6 行；Task 队列 350 DIP 高、8 行；Maintenance 各主表 350 DIP 高、8 行；所有页面滚动面为 `Auto` 且内容超限时 `scrollable=True`；Overview 堆叠模式由页面滚动承载，风险区内容完整（496 DIP）。本机系统 SDK 9.0.302 在多节点构建时会因 SDK locator 目录缺失在 `GetTargetFrameworks` 静默失败，`render-qa.ps1` 已固化 `-m:1 -nodeReuse:false -p:NuGetAudit=false`。本机 C 盘空间耗尽时，需先把 `TEMP/TMP` 指到仓库 `.tmp/qa-temp` 再运行脚本。真实 Playnite 宿主、主题、DPI 和连续缩放流畅性仍未验证。
- UI-201（本轮）：TaskCenter 堆叠详情 Inspector 的最小高度从 96 提高到 160 DIP，解决 1040×700/1280×720/1366×768 常用窗口下详情条带过矮、基本无法阅读的问题；`TaskGrid` 的 236–460 DIP 有限视口、内部滚动、虚拟化、真实搜索/筛选/计数/命令/Binding 均未改变。离屏 QA 复核：1040×700/1280×720/1366×768 下 `TaskDetailScrollViewer` 均为 160 DIP 且内部 `Auto` 滚动，Task 队列仍为 350/360/384 DIP 高、8 行；1600×900/1920×1080 仍保持 Demo 式右栏 Inspector（360 宽）。源码验证通过；真实 Playnite 宿主、主题、DPI 和连续缩放流畅性仍未验证。
- UI-202（本轮）：Overview 首页按 Demo `HomeView` 的单列阅读流收口：Dashboard 在内容区 `<1200` DIP（1280×720、1366×768 等常用窗口化逻辑宽度）时把 Overview 切为堆叠单列页面流，右侧“今日概览/风险与提醒”下移到主内容之后，避免主工作区只剩 550–600 DIP 宽导致 Hero 与“当前游戏”被挤成上下堆叠；Hero/当前游戏同行的堆叠阈值同步从 760 降到 700，使 1040×700 最小窗口也保持 Demo 的“Hero + 当前游戏”并排。离屏 QA 复核：1040×700 Hero 444px、当前游戏 266px 同行；1280×720 Hero 576px、当前游戏 346px 同行；1366×768 Hero 630px、当前游戏 378px 同行；所有尺寸均无重叠，最近活动由 `OverviewStackScrollSurface` Auto 滚动可达；1600×900/1920×1080 继续使用宽屏双列。`OverviewActivityList` 有限视口、ListBox Recycling、真实命令/Binding 和页面滚动分工未改变；`render-qa.ps1` 的 Overview 堆叠阈值同步为 1200。源码验证通过；真实 Playnite 宿主、主题、DPI 和连续缩放流畅性仍未验证。
- UI-203（本轮）：存档中心与修改器中心的堆叠 Inspector 最小高度统一从 96 提高到 160 DIP（`SaveHistoryActionsScrollViewer`、`SaveCandidateInspectorScrollViewer`、`TrainerToolsSettingsScrollViewer`、`TrainerReleaseInfoScrollViewer`），与 UI-201 的 Task 规则一致；`SaveHistoryGrid`/`SaveCandidateGrid`、修改器主表与可下载版本面板仍保持 236 DIP 最小视口、内部滚动和虚拟化。离屏 QA 已扩展覆盖 SaveCenterView/TrainerCenterView（含假数据）：1040×700/1280×720/1366×768 下四个堆叠 Inspector 均为 160 DIP 且内部 Auto 滚动，主表 236 DIP 高、8 行；1600×900/1920×1080 保持右栏 360 宽。源码验证通过；真实 Playnite 宿主、主题、DPI 和连续缩放流畅性仍未验证。
- QA-002（本轮）：离屏渲染 QA 覆盖补齐全部工作区与设置页：Overview、Save、Trainer、Media、Maintenance、Task、Settings 均在 1040×700/1280×720/1366×768/1600×900/1920×1080 渲染并输出 PNG/报告。设置页依赖 Playnite 宿主 `BaseTextBlockStyle`，harness 在 Application.Resources 预置中性 fallback 后可在无宿主环境解析；1040×700 下 Settings 四个 Tab 的 `SettingsScroller` 均为 Auto 且内容超限时 `scrollable=True`。
- QA-003（本轮）：`render-qa.ps1` 增加自动失败门禁：任何命名工作区主表/主列表（排除 `MaintenanceAuditLogGrid` 审计条带与 `OverviewActivityList` 最近活动）在任一常用窗口下的有限视口 `<236` DIP，或命名页面滚动面（`*ScrollSurface`/`SettingsScroller`）内容超限却使用 Hidden 滚动条，render-qa 将以退出码 1 失败并在报告中列出 `PROBLEM`。当前 7 页面 × 5 尺寸全绿。
- QA-004（本轮）：在 C 盘恢复可用空间并重定向 `TEMP/TMP` 到 `.tmp/qa-temp`、测试输出隔离到 `artifacts/ui-qa/*-tests` 后，完整隔离测试重跑通过：Core 13/13、Worker 23/23、Playnite 151/151。Playnite 源码结构断言同步更新为 UI-201/202/203 的新阈值（`Math.Max(160, ...)`、`workspaceContentWidth < 1200`、`primaryWidth < 700`）。
- QA-005（本轮）：`scripts/dev-install-run.ps1 -Configuration Release -NoStart` 一键构建安装成功：解决方案 Release 构建 0 警告/0 错误，Core 13/13、Worker 23/23、Playnite 151/151 全部通过，已打包并安装到 `C:\Users\lopmatu\AppData\Roaming\Playnite\Extensions\GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec`（extension.yaml 0.6.70、DLL 0.6.70.0），未自动启动 Playnite。真实 Playnite 宿主内的 Light/Dark/Follow/高对比度、DPI、键盘与连续缩放仍需用户手工验收。
- UI-207-FOLLOWUP（2026-08-12）：Settings 的页面滚动所有权已收口到共享 TabControl 模板：宽屏 `SettingsHeaderScroller` 为 232 DIP 左侧导航，紧凑布局为顶部横向 Auto，`SettingsScroller` 仅承载当前内容；5 个分类在 760/880/920/1100/1400 × 560/700/900 探针中可见。当前游戏 Icon 统一由共享样式复用到 Dashboard/Overview/Save/Trainer/Media，GamePicker 列表不加载 Icon；筛选隐藏当前选择时保留选择并提供恢复操作。隔离 Release 构建 0 警告/0 错误，Core 13/13、Worker 51/51、Playnite 197/197，render-qa 全绿；真实 Playnite 宿主主题/DPI/键盘/连续缩放仍未运行。
- UI-204（本轮）：修复真实 Playnite 中任务中心筛选下拉框在异步集合重建后显示为空的问题。新增共享 `UiFilterSelection.RestoreDefault`（`src/GameSaveCenter.Playnite/Infrastructure/UiFilterSelection.cs`），TaskCenter 的 状态/游戏/类型 三个筛选下拉框在 `Loaded`、`DataContextChanged`、游戏/类型选项集合 `CollectionChanged` 时恢复逻辑默认值（全部）；Dashboard GamePicker 的 状态/平台/排序 三个下拉框在打开选择器与平台选项重建时同样恢复，且只在 `SelectedItem == null` 时恢复，不覆盖用户真实选择。离屏 QA 记录命名 ComboBox 的 `selected/index/items`，空选择会触发 `PROBLEM`；render-qa 全绿，Playnite 151/151 通过。
- UI-205（本轮）：针对真实 Playnite 中动态下拉框 Items 物化晚于 DataBind 导致游戏/类型筛选仍为空的问题，TaskCenter 增加 200ms `DispatcherTimer` 短周期重试（最多 25 次），在集合重建、加载、DataContext 变化后持续恢复默认选中直到三个下拉框都有值；GamePicker 平台选项重建与面板打开时增加 `DispatcherPriority.Loaded` 二次恢复。render-qa 与 Playnite 152/152 通过。
- PERF-001（本轮）：性能优化第一刀——大列表集合批量通知。新增共享 `BatchObservableCollection<T>`（`src/GameSaveCenter.Playnite/Infrastructure/BatchObservableCollection.cs`），`ReplaceAll` 只在内容真正变化时发一次 `Reset`，避免 `Clear()+Add()` 对 Games/Tasks/Media/Findings/Backups/GameTools/TrainerCatalogResults 等大集合逐条触发 WPF 布局；`DashboardViewModel.Replace` 自动路由到 `ReplaceAll`。render-qa 新增每页 `render_ms` 计时基线；render-qa 全绿，Playnite 152/152 通过。
- PERF-002（本轮）：任务中心每次完整快照都会重建 游戏/类型 筛选选项（O(n log n)）。新增 `ComputeTaskFilterFingerprint`，当 `Tasks` 的任务 ID 顺序与数量指纹未变化时直接跳过 `RebuildTaskFilters`，用户筛选仍由属性 setter 直接 `TasksView.Refresh()`；render-qa 全绿，Playnite 152/152 通过。
- PERF-003（本轮）：GamePicker 每次快照都会 `Distinct+OrderBy` 重建平台筛选选项。新增 `ComputePlatformFingerprint`（平台名顺序 + 数量），`Items` 未变化时跳过 `RebuildPlatformOptions`；render-qa 全绿，Playnite 152/152 通过。
- PERF-004（本轮）：GamePicker 每次快照都为整库新建 `GamePickerItem`，大库下分配多且选中引用会漂移。改为按 `PlayniteId` 缓存复用 `GamePickerItem`，快照时只 `UpdateGame` 更新内部 `GameStatusDto` 引用（缓存超过 `max(1024, 2*游戏数+100)` 才清空重建）；减少大库分配并让选中对象身份跨快照稳定。render-qa 全绿，Playnite 152/152 通过。
- 新增的响应式门禁要求：1080p、2K、4K 不能只按物理分辨率判断，必须按 DPI 换算后的逻辑 DIP 尺寸检查全屏、窗口化和最大化；常用窗口下首屏下方真实内容不得被页脚或工作区边界遮住，主表/主列表应保留约四行可读视口，页面滚动与列表内部虚拟化滚动必须分工明确。具体门禁见 `docs/design/UI_CHANGE_GATE.md`。
- 本轮工作区：`0c6f143` 提交后干净；后续 agent 仍须先运行 `git status`、`git log -5 --oneline --decorate` 和 `git branch --show-current`。
- 验证：`python scripts/validate-source.py` 通过；`scripts/render-qa.ps1` 覆盖 Overview/Save/Trainer/Media/Maintenance/Task/Settings × 1040×700/1280×720/1366×768/1600×900/1920×1080 全部通过（含自动失败门禁）；技能静态审查 0 error；`scripts/dev-install-run.ps1 -NoStart` 一键构建安装成功（解决方案 Release 0 警告/0 错误，Core 13/13、Worker 23/23、Playnite 151/151），最新扩展已安装到本机 Playnite Extensions，未自动启动。真实 Playnite 宿主、主题、DPI、窗口化截图和连续缩放运行时渲染仍需用户在 Playnite 内手工验收。

以下原有的远端交接基线保留为历史记录，便于追溯另一台机器的 UI-183 上下文：

## 原远端交接基线（2026-08-10）

- 分支：`main`
- 当前 UI 基线：`2db4336`（`重构：维护中心诊断操作区对齐 Demo`，UI-183）
- 版本：`0.6.70-development-preview`
- 当前工作区：本轮交接文档提交完成后应干净
- 相对 `origin/main`：请运行 `git rev-list --count origin/main..HEAD` 实时确认；UI-183 实现提交为 `2db4336`，后续交接文档提交不改变生产 UI
- 最近已完成的重点：
  - UI-177：修改器中心无选中工具时释放空 Inspector 和固定右栏
  - UI-178：媒体中心无选中媒体时释放空 Inspector 和堆叠行
  - UI-179：设置页窄屏标题提示换行，以及 ZIP/Zstandard/跟随 Playnite 默认值显示
  - UI-180：首页阅读顺序调整为“工作台/今日状态 → 当前游戏 → 六项指标 → 最近活动”
  - UI-181：维护中心五张 DataGrid 的首列显式使用 `MaintenanceFirstColumnHeader`，末列继续使用显式维护表头主题
  - UI-182：维护中心进程映射编辑器宽屏使用 EXE `*`、目标游戏 240 DIP、绑定按钮的 Demo 对齐 Grid，窄于 720 DIP 时目标和按钮换到第二行
  - UI-183：维护中心诊断页顶部改为 Demo 式“诊断操作”阅读卡，刷新诊断提升为主操作，其余五个只读入口保留在第二行操作带
- 当前已完成的自动化基线：Core 13、Worker 23、Playnite 142 测试通过；Release 构建 0 警告/0 错误；XAML 结构检查 13/13；WPF 源码验证通过。静态测试不等同于真实 Playnite 宿主、主题和 DPI 渲染验证。

## 下一步开发方向

继续以 Demo 对齐为目标，对生产页面做页面级收口和真实宿主验收，优先顺序如下：

1. 以 `scripts/render-qa.ps1`（7 页面 × 5 种常用逻辑窗口）为离屏回归基线；继续检查 Overview、SaveCenter、TrainerCenter、TaskCenter、Maintenance、MediaCenter、Settings 的层级、按钮尺寸、文字对齐、默认选择和空状态。
2. 检查共享 `Button`、`ComboBox`、`TextBox`、`ListBox`、`DataGrid`、Tab 和 Inspector 资源，发现同类问题时修共享模板。
3. 每次页面级改动后运行 `scripts/render-qa.ps1`（C 盘满时先设 `TEMP/TMP` 到仓库 `.tmp/qa-temp`）；按 980/1040/1100/1280/1366/1600 DIP 宽度、640/720/900/1080 DIP 高，以及 1080p/2K/4K 在 100%/125%/150%/175%/200% DPI 下的常用窗口化逻辑尺寸，复核窄屏堆叠、首屏内容可见性、有限滚动和长文本；不把 4K 通过当作 1080p 通过。
4. 在可用环境中运行 Playnite 宿主，验证 Light/Dark/Follow Playnite/高对比度、键盘焦点、真实数据加载和窗口关闭生命周期；若环境不可用，保留明确的手工验收清单。
5. 发现问题后继续使用新的 UI 编号记录，不要删除历史记录或把未验证事项标成完成。
6. 在可用的干净环境中重跑完整 Release 构建与隔离测试；本机曾因测试输出 DLL 残留句柄占用和 C 盘 0 可用空间而无法在本次重跑。

## 跨电脑、跨模型规则

代码和文档是交接的真实来源，模型记忆不是。切换电脑前应先把当前提交推送到远端：

每一轮开发完成后，由 Agent 自己 commit 并 push 到当前远端分支（默认 `main`），不要等用户提醒；这是项目长期协定，不需要用户每次重复。

每轮开发完成后，还要及时清理不再使用的本地中间产物：`artifacts/` 下的旧 `dev-build`、`ui-audit-build`、`phase*`、`audit*` 目录与旧 zip，以及 `.tmp/` 下的一次性审计目录。只保留当前安装包/打包目录与当前审计证据；删除前确认路径在仓库 `artifacts/` 或 `.tmp/` 内，并用 PowerShell `Remove-Item -LiteralPath` 执行。

```powershell
git push origin main
```

切换后应先拉取同一分支，再执行：

```powershell
git status
git log -5 --oneline --decorate
git branch --show-current
```

如果另一台电脑有未 push 的提交或未提交改动，先比较 `git status`、分支和提交历史，再合并；禁止直接覆盖。DeepSeek、Claude 或其他模型可以根据本文件、源码和测试继续工作，但不会自动继承原聊天中的隐含上下文、工具状态或审批状态，因此必须先读取本文件并按流程重新建立上下文。

## 2026-08-16 UI-206 Overview 页面级迁移交接

- 生产首页已按 `GameSaveCenter.UiLab/Pages/OverviewPage.xaml` 的阅读骨架重排：Hero/当前游戏与六项指标占满顶部，最近活动之后才分左主区与右侧风险/关注栏。
- 宽屏右栏固定 330 DIP，与 `OverviewRecentActivityCard` 同行并且离屏探针偏移 0 DIP；窄窗口由 `OverviewStackScrollSurface` 页面滚动承载，右栏下移。生产滚动条模板保持现有版本，未迁移 demo 滚动条或右上角颜色演示按钮。
- 真实功能保留：Snapshot、RecentProtection、AttentionFindings、OpenProtection/OpenAttention 命令、选择与虚拟化列表均仍绑定生产 ViewModel；共享活动表头已做 8 DIP 圆角、低对比度填充和安全边距。
- 本阶段验证：`validate-source.py`、`check-xaml.ps1`、WPF 静态审查通过；Playnite 测试 303/303；生产 Release 0 warning / 0 error；Overview 多尺寸、Light/Dark 与 resize transition 离屏审计通过。全量 render harness 仍仅剩 Save/Media 窄尺寸 `<236 DIP` 历史表格门禁，不要误称全量通过。
- 后续人工 QA：真实 Playnite 2K/DPI、Follow/高对比度、键盘焦点、长中文文案和连续缩放；优先检查 Overview 宽屏侧栏与窄屏下移，再继续其他页面的页面级迁移。

## 2026-08-16 UI-208 Overview 全局活动交接

- 首页“全局活动”已从生产旧的表头 + 图标列改为 UiLab 业务列表四列：类型、对象/事件、结果、时间；真实 `Activities` 绑定与虚拟化保持不变。
- `OverviewActivityTimelineList` 继续使用页面级滚动契约，窄窗通过 `ActivityKindColumn`/`ActivityTimeColumn` 的响应式收紧解决空间问题，不得引入 UiLab 的滚动条模板。
- 本阶段验证：`validate-source.py`、`check-xaml.ps1`、WPF 静态审查、Playnite 303/303、Release 0 warning/0 error、RenderHarness v6/v6.2 宽/窄首页截图均通过。
- 下一阶段继续对照 UiLab 的 Saves/Trainer/Media/Tasks/Maintenance/Settings 页面，优先收口共享按钮、Tab、DataGrid 表头和 Inspector 卡片；全量 render harness 仍需单独处理 Save/Media 窄表历史门禁。
- 真实 Playnite 2K/DPI、Follow/高对比度、键盘焦点与连续缩放仍为 `MANUAL QA REQUIRED`，不能用离屏截图替代。

## 2026-08-16 UI-209 共享表头交接

- `WpfUiProduction.xaml` 的 `DataGridColumnHeadersPresenter` 已改为透明；列头圆角由每个 `DataGridColumnHeader` 自己呈现，避免整行连续底色掩盖圆角。
- 生产滚动条、Item scrolling、行/列虚拟化、绑定和 Playnite 兼容性未改变；后续页面迁移不要直接复制 UiLab 的 `LabScrollViewer`。
- 验证已完成：`validate-source.py`、`check-xaml.ps1`、Release 构建、Playnite 303/303、RenderHarness v6/v6.2。
- 下一阶段继续对照 UiLab 的 Media/Save/Trainer/Task/Maintenance/Settings 页面，优先处理页面层级和窄窗列宽；真实宿主仍需人工 QA。

## 2026-08-16 UI-210 Media 交接

- Media 统计带已改成 UiLab 的单卡四段结构，默认当前媒体标签；真实媒体、待归类、来源规则绑定和命令保持不变。
- Media 保留生产 `VirtualizingStackPanel` 行列表与滚动条，不直接复制 UiLab 的 `WrapPanel` 缩略图网格；后续若要进一步做缩略图网格，必须先提供大媒体库虚拟化方案和滚动回归证据。
- 已修复 Media 700-720 DIP 视口过短以及窄→宽 Inspector 不恢复的问题。重建 RenderHarness 后 Media Light/Dark、多尺寸和 resize transition 通过。
- 全量 render-qa 的剩余门禁仅为 Save 候选表在部分窄尺寸低于 236 DIP；下一阶段优先处理 Save，而不是回退 Media 的布局修复。

## 2026-08-16 UI-211 Save 交接

- Save 表格视口高度公式已从 `height - 520` 调整为 `height - 464`，常规 700-720 DIP 窗口保持 236 DIP；短窗下限为 180 DIP。
- Save DataGrid、Inspector 抽屉、现有滚动条、虚拟化、命令与绑定均保持生产实现。
- 全量 RenderHarness 已通过：`render-qa OK`，包含 Light/Dark、7 页面、1040/1100/1366/2560 和 resize transition；真实 Playnite 宿主仍需人工 DPI/主题/键盘验收。

## 2026-08-16 UI-212 Media 网格交接

- Media 当前游戏主体已从生产横向信息行迁移为 UiLab 风格的固定 164×142 DIP 缩略图卡片；`VirtualizingWrapPanel` 负责可见项生成与 `IScrollInfo`，生产 `ListBox` 的 ScrollViewer/ScrollBar 继续负责实际滚动。
- 卡片只使用真实 MediaItemDto 字段和 `AsyncThumbnailImage`，Inspector、批量操作、Extended selection、窄屏详情抽屉及现有命令/绑定不变；不要直接复制 UiLab 的普通 `WrapPanel` 或演示滚动条。
- 自动验证完成：`validate-source.py`、`check-xaml.ps1`、WPF 静态审查、Release 构建、Playnite 303/303、全量 RenderHarness `render-qa OK`；证据目录为 `artifacts/ui-qa/media-grid-migration-v3`。
- 真实宿主中的实际缩略图文件、超大媒体库滚动、最终 DPI、Follow/高对比度、键盘焦点和连续缩放仍是 `MANUAL QA REQUIRED`。

## 用户原话（必须保留）

> 在继续之前你最好是能够搞一个文件，能够指引去哪里读取获得开发方向等等。这样我直接说你读取xx文件就可以了，他就知道后续怎么开发了。连我这段话你也要放进去，省得我每次都说了（这样每次开发他们都会维护这个项目）。

这句话代表长期维护要求：后续每次开发都必须继续维护本项目，并同步维护本交接文件、项目记忆、开发进度和 Git commit。

## 2026-08-20 UI-254 任务与设置页交接

- 设置页已经完成 Demo-first 页面壳迁移：`GameSaveCenterSettingsView.xaml` 使用左侧五项 `LabSegmented` 分类栏和右侧 `SettingsScroller` 单一内容滚动区；真实字段、校验、保存语义、导入/导出入口均保留。
- 设置页分类面板名称固定为 `SettingsGeneralPanel`、`SettingsBackupPanel`、`SettingsAppearancePanel`、`SettingsAutomationPanel`、`SettingsMigrationPanel`；代码通过 `OnSettingsTabSelectionChanged` 做可见性切换，不能恢复旧 TabControl 的横向 TabStrip 假设。
- 任务中心当前已符合 Demo 的“指标 → 筛选 → 更多筛选 → 队列/进度 → Inspector”阅读顺序；后续修改必须继续保留真实 `TasksView`、`SelectedTask`、`RetryTaskCommand`、`CancelTaskCommand`、DataGrid 虚拟化和项目滚动条。
- RenderHarness 设置审计已改为识别 `SettingsSectionTabs` ListBox；当前证据为 `artifacts/ui-qa/task-settings-final/render-qa-report.txt`，内容为 `render-qa OK`，涵盖 Light/Dark、1040/1100/1366/2560、多分类和 resize transition。
- 本阶段验证：XAML 18 文件通过；源码门禁通过；Release 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 253 通过/61 跳过/0 失败；WPF 静态审查 0 error、20 warnings、161 info。
- 真实 Playnite 宿主逐页截图仍是未完成的人工验收边界；不能把 RenderHarness PNG、测试或安装清单写成 Playnite 1:1 视觉通过。总 Demo-first 迁移目标尚未完成，下一阶段继续按目标文件对照未收口页面和宿主验证。

## 2026-08-20 UI-255 工作区表格共享契约交接

- `src/GameSaveCenter.Playnite/Themes/Redesign.xaml` 新增 `GscRedesignWorkspaceDataGrid`。Save、Media、Maintenance、Task 的页面 DataGrid 样式必须基于它；页面可以覆盖状态行、媒体表头和背景，但不要复制回虚拟化/滚动/排序/列宽 setter。
- 共享契约保留真实页面滚动条和有限 Grid 视口：`CanContentScroll=True`、`VirtualizingPanel.ScrollUnit=Item`、`VirtualizationMode=Recycling`、行/列虚拟化、列宽可调整、排序可用、FullRow 单选。当前离屏表格截图确认无表头、选中行、Inspector 或滚动条遮挡。
- 所有业务折叠区仍使用 `GscDisclosureCard`；Mono 文本应使用 `GscCodeFontFamily`，其首选链是 `Cascadia Mono, Consolas, Microsoft YaHei UI`。
- 新证据目录为 `artifacts/ui-qa/shared-grid-contract-final`，报告为 `render-qa OK`，覆盖七页、多尺寸、双主题和 resize transition。源码/XAML/Release/Playnite 门禁已通过。
- 下一阶段继续按目标文件检查按钮、辅助界面和真实操作路径，并优先在同一可识别 Playnite 宿主取得逐页截图；不要把上述离屏证据写成 Playnite 1:1 视觉通过。

## 2026-08-20 UI-256 反馈层与按钮契约交接

- `Themes/Redesign.xaml` 已新增共享 `GscRedesignFeedbackToastCard`、`GscRedesignFeedbackDialogCard`、Dialog 遮罩和反馈文字资源；Dashboard 的 Toast/Dialog 必须继续引用这些资源，不要把圆角、背景、阴影、标题字号重新写回 `DashboardView.xaml.cs` 或单页 XAML。
- Dashboard 的原生 `Button` 已回到 `DesignTokens.xaml` 的 `GscButtonBase`/`GscPrimaryButton`；页面内原有 `ui:Button` 语义样式仍是 WPF-UI 的共享入口。修改按钮时优先修共享资源，保留最小 38 DIP 高度、焦点视觉、禁用态和可读的文本截断。
- Toast 的真实通知来源仍是 `GameSaveCenterPlugin.UiNotificationRequested`，确认/选择仍由 `UiConfirmationRequested`/`UiChoiceRequested` 和 `TaskCompletionSource` 完成；不要改成只显示静态 Demo 提示或删除错误详情、Escape、计时器清理、重复 Dialog 取消语义。
- 设置页导出成功、导入报告和错误继续走原生 `MessageBox`，不要在 Playnite 共享 Window 中重新注册 WPF-UI `ContentDialogHost`；如果未来要改成页面内反馈，必须先补充 Window 宿主、焦点、模态、取消和真实操作验证。
- UI-256 已完成 XAML/source/Release/三组测试/离屏 render-qa；真实 Playnite 宿主 Light/Dark/Follow、高对比度、DPI、键盘焦点、Toast/Dialog 实际触发仍是人工验收边界，总 Demo-first 目标未完成。

## 2026-08-20 UI-257 首页当前游戏卡片响应式交接

- 首页根滚动面仍然是 `OverviewStackScrollSurface`，横向滚动必须保持禁用；`OverviewLayoutGrid` 已绑定 `{Binding ViewportWidth, ElementName=OverviewStackScrollSurface}`，并以 `HorizontalAlignment=Left` 避免无限横向测量造成卡片和按钮右侧裁切。
- 生产真实绑定、当前游戏选择器、`BackupSelectedCommand`、`LoadDetailsCommand`、`OpenAttentionCenterCommand`、首页全部备份/同步媒体/刷新入口未改变。RenderHarness 报告 `artifacts/ui-qa/overview-responsive-ui257/render-qa-report.txt` 已覆盖双主题、多逻辑尺寸和 resize transition；1366/1600 代表截图中的当前游戏卡片与三个按钮均完整可见。
- 最终验证为 XAML 18/18、Release 0 warning/0 error、Core 59/59、Worker 191/191、Playnite 256 通过/62 跳过、source validation 通过、WPF 0 error。新增源码回归 `OverviewFlowUsesTheFiniteViewportWhenHorizontalScrollingIsDisabled` 当前属于既有 `LegacyProductionUiBaselineFact` 门禁组，因此在默认测试运行中按基线跳过，但随 Playnite 测试程序集完成编译并锁定源码契约。
- 真实宿主验证边界：`real-host-audit.ps1` 三次安装并加载生产 0.6.70.0，最新受控证据为 `artifacts/ui-host-audit-ui257-final`；日志确认生产插件真实读取 3 games/50 tasks/100 findings/30 media。由于 Playnite 主窗口返回 `EmptyWindowAutomationPeer`，未能抓取嵌入式导航后的逐页像素截图。受控 `DashboardView` 截图只用于确认本次裁切修复，不得宣称七页已完成 Playnite 1:1 验收。

## 2026-08-20 UI-258 真实 Playnite 七页逐页复核交接

- 已在实际 Playnite 生产窗口 `GameSaveCenter 生产版` 中人工打开七页：Overview、Save、Media、Task、Trainer、Maintenance，以及从游戏右键 `GameSaveCenter → 打开设置` 打开的 Settings。
- 实际看到的关键数据/入口：当前游戏 `Bongo Cat`；Media 30 项、5.76 MiB、待归类 4468 项；Task 50 条、运行中 0、需关注 16、今日完成 34；Trainer 有 Wo Long 与 Yakuza 3；Maintenance 的诊断与进程映射均可进入；Settings 的“常规与目录”及 Worker/Ludusavi/存档目录字段可见。
- Media Inbox 已选中真实待处理截图，滚动右侧独立详情后确认预览、归类选择器、“确认归类”和“忽略并保留副本”可达。本次未点击归类/忽略，数据未改变。
- 这次是人工真实嵌入复核，补足了 UIAutomation 不能点击侧栏的缺口；`real-host-audit.ps1` 仍会因 `EmptyWindowAutomationPeer` 无法生成 `summary.json`，因此不要把人工截图写成自动审计通过。剩余边界是不同 DPI、Follow/高对比度、键盘焦点，以及备份/归类/忽略等真实操作回归。

### 后续启动协议补充

下一轮继续读取本文件、`docs/ai/PROJECT_MEMORY.md`、`docs/ai/WORKLOG.md` 和用户指定的目标文件；先检查 `git status` 与最近提交。页面继续以 `GameSaveCenter.AcrylicFork/src/GameSaveCenter.Playnite/Design/` Demo 为唯一视觉基准，保留当前游戏选择器、生产滚动条、真实命令/绑定、虚拟化和 Playnite 兼容性。优先补做真实备份/媒体归类操作的安全回归与不同 DPI/主题/键盘焦点检查；自动审计若仍返回 `EmptyWindowAutomationPeer`，如实记录边界，不能用 RenderHarness 替代。

## 2026-08-20 UI-259 媒体收件箱共享虚拟化交接

- `MediaCenterView.xaml` 的 `MediaInboxGrid` 已移除页面级 `Standard` 行虚拟化和列虚拟化关闭覆盖，统一继承 `GscRedesignWorkspaceDataGrid` 的 `Recycling`、Item scrolling、行/列虚拟化、排序和列宽调整；真实绑定与媒体 Inspector 操作不变。
- RenderHarness 已为 Media Inbox 提供 60 项夹具、五档高度和 0/25/50/75/100% 滚动位置探针，并将列虚拟化纳入 `ProbeGrid` 门禁；当前报告 `artifacts/ui-qa/media-virtualization-fix/render-qa-report.txt` 为 `render-qa OK`。
- UI-259 正式门禁：XAML 18/18；source validation 通过；Release 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 256 通过/62 跳过；WPF 0 error、19 warnings、146 info。
- 本轮没有重新安装真实 Playnite；UI-258 的真实七页人工嵌入复核仍是最近宿主事实。后续优先补做不同 DPI、Follow/高对比度、键盘焦点及真实备份/媒体归类操作的安全回归，若自动审计继续遇到 `EmptyWindowAutomationPeer` 必须如实记录。

## 2026-08-20 UI-260/261 存档页示例文案清理与工作区 Tab 样式回滚交接

- `AcrylicProductionShellView.xaml.cs` 的存档副标题已从硬编码 Demo 游戏名改为 `SelectedGame.Name`，空选择显示“未选择游戏”；页头在工作区切换和 `SelectedGame` 变化时都会刷新。对应源码契约已覆盖 Elden Ring 残留防回归。
- 生产页工作区 Tab 栏是当前项目视觉的明确例外：`Themes/Redesign.xaml` 中 `GscRedesignWorkspaceTabControl`/`GscRedesignWorkspaceTabItem` 已回滚为项目现有透明 header 带、独立圆角页签和横向滚动，不得恢复 Demo 的外层连续分段胶囊。Save/Media/Maintenance 的真实 Tab 结构、绑定、命令、内容 Stretch 和嵌套页签保持不变。
- RenderHarness 的重复模板部件名度量已修复；最新 `artifacts/ui-qa/project-tab-chrome-rollback/render-qa-report.txt` 为 `render-qa OK`，并已人工查看 Save/Media/Maintenance 代表截图。源码/XAML/差异检查及定向契约 15/15 均通过。
- UI-260/261 的 Release 安装验证已通过：XAML 18/18、Release 0 warning/0 error、Core 59/59、Worker 191/191、Playnite 258 通过/62 跳过、安装 0.6.70/DLL 0.6.70.0；WPF validator 0 error、19 warnings、161 info。现在可以进入提交/推送前的最终工作树检查。
- 真实 Playnite 重启后的 Computer Use 当前仍可能返回 `foreground window did not report a process id`；不能用离屏截图代替重装后宿主 Tab 像素证据。剩余人工边界仍为 125%/150% DPI、窗口缩放、Follow/高对比度、键盘焦点和真实备份/媒体归类操作。

## 2026-08-25 UI-315 共享卡片/表格自适应毛玻璃交接

- 用户已要求所有卡片、表格、容器尽量使用毛玻璃，并希望颜色跟随当前游戏背景图。实现已集中在 `AdaptiveThemePaletteFactory.ApplyGameBackgroundGlassResources`，不要回到逐页写固定背景色的方式。
- 生产卡片和表格通常通过 `GscGlassFillBrush` / `GscGlassStrongBrush`，表头通过 `GscTableHeaderBrush`，浮层通过 `GscPopupBrush`；修改材质优先改这些共享资源和 `Redesign.xaml`，不要逐个页面加局部 Brush。
- `DashboardView` 主题刷新后以及当前游戏背景采样属性变化时，会把材质同步到 Dashboard、`AcrylicProductionShellView` 和所有生产 workspace 的 ResourceDictionary。没有游戏背景、禁用玻璃或高对比度时必须走中性回退。
- 底层游戏图 BlurEffect 的职责与卡片颜色分离：不能把 BlurEffect 直接挂到卡片，否则会把卡片自己的文字/表格一起模糊；也不能把采样渐变设成完全不透明，否则失去玻璃效果。
- UI-315 验证：source validation 通过；Release 0 warning/0 error；Core 59/59、Worker 199/199、Playnite 289 通过/57 跳过；WPF validator 0 error/18 warnings/166 info；多主题、多尺寸和 resize transition 的 render-qa 为 OK。
- 本轮没有新增真实 Playnite 重启后的逐页像素证据；后续若继续调透明度，应在真实宿主中复核 Bongo Cat 等不同主色背景、Follow/高对比度和 125%/150% DPI，并继续保留真实备份/媒体归类操作边界。

## 2026-08-25 UI-316 设置页毛玻璃交接

- 设置页不使用游戏图片背景，但必须保留整页玻璃层。运行时由 `ApplySettingsMaterialResources` 生成主题驱动的环境渐变、外壳/分类栏/表单/内容四级材质，以及仅作用于 `SettingsAmbientLayer` 的 BlurEffect。
- 需要继续保持 `SettingsScroller` 的现有滚动模型、五个分类、字段绑定、保存/取消和设置导入导出语义；不要为了加玻璃再包裹一层改变响应式代码定位或滚动通道。
- `tests/GameSaveCenter.RenderHarness/Program.cs` 已在 Settings 离屏渲染前调用 `ApplyThemeForAudit(FollowPlaynite)`，后续截图回归要保留这一步，否则会误测静态 DesignTokens。
- UI-316 验证：source validation、XAML 结构检查通过；Release 0 warning/0 error；Core 59/59、Worker 199/199、Playnite 289 通过/57 跳过；render-qa OK；WPF validator 0 error、18 warnings、172 info。
- 当前没有新的真实 Playnite 重启后设置窗口截图；下一轮若继续调材质，优先用真实宿主复核 Follow/高对比度、不同 DPI、关闭玻璃回退和键盘焦点，不能将 `.tmp/ui-qa-settings-glass-v5` 当作 Playnite 1:1 证据。

## 2026-08-25 UI-320 游戏选择器交接

- `AcrylicProductionShellView.xaml` 的生产游戏弹层使用共享 `ListBoxItem` 圆角模板；不要重新添加未基于共享样式的局部 `ListBoxItem`，否则选中和预选会回到 Playnite 的矩形默认视觉。
- 生产壳状态/平台/排序筛选默认值是“已安装 / 全部 / 名称”。平台列表异步重建后必须通过 `UiFilterSelection.RestoreDefault` 恢复显示；真实 `GamePicker` 绑定和选中命令不可改成静态选项。
- `DashboardView.xaml` 游戏列表 Row 的 `ClipToBounds` 是圆角状态契约；如果继续调整游戏行，请同时检查选中、鼠标预选、键盘焦点和高对比度边框。
- UI-320 已完成源码/XAML/Release/定向 Playnite 测试与离屏 render-qa。真实 Playnite 游戏弹层尚未由本轮自动截图验证，后续如能取得可识别宿主窗口，应优先确认选中/预选圆角和中间“全部”显示。

## 2026-08-25 UI-321 平台筛选显示时序交接

- 仅设置 `SelectedIndex="0"` 不足以保证生产游戏弹层的中间框显示“全部”：弹层初始隐藏时 ItemsSource 可能尚未生成。
- 维护 `AcrylicProductionShellView` 的游戏弹层时，平台默认恢复必须保留 `Loaded` 事件、弹层打开时的 `DataBind/Loaded` 调度，以及 `PlatformFilterOptions.CollectionChanged` 监听。
- `UiFilterSelection.RestoreDefault` 的有效选中保护不能删除；用户已经选择具体平台时，集合仍包含该平台就必须保留它。
- UI-321 已完成源码/XAML/Release/定向测试；真实 Playnite 重载后的中间框“全部”像素仍待确认。

## 2026-08-25 UI-322 底部状态栏与侧栏折叠交接

- `AcrylicProductionShellView.xaml` 的 `FooterSurface` 已跨越根 Grid 两列，`FooterStatusPanel` 显示真实 Worker/Ludusavi 状态；不要把状态灯恢复到侧栏，或改成不绑定 `Snapshot` 的静态文案。
- 侧栏品牌区右上角的 `SidebarCollapseButton` 是 26×26 小按钮；生产壳默认仍为 236 DIP 展开态，代码中的 78 DIP 是折叠态。折叠只隐藏文字并将导航项收成图标，不改变 `Nav*` 的真实工作区事件与设置入口。
- `ApplySidebarLayout` 必须在切换后调用既有 `ApplyHeaderLayout`/`ApplyPageLayout`，以便页面按新可用宽度重新布局；不要为折叠状态复制一套页面布局或改变滚动/虚拟化。
- 本阶段 `validate-source.py`、`check-xaml.ps1`、Release 构建、Playnite 294/351 和 RenderHarness `render-qa OK` 均已通过。离屏渲染没有覆盖真实 Playnite 中点击折叠按钮的像素结果；后续应在可识别宿主中复核展开/折叠、Light/Dark/Follow、125%/150% DPI、键盘焦点和导航 Tooltip。

## 2026-08-25 UI-323 状态栏、版本气泡与设置尺寸交接

- `FooterSurface` 的两个状态灯现在靠右，产品名和生产版说明已移除；不要把版本说明重新塞回底部栏。`SidebarProductionBadge` 保留在品牌行并显示程序集版本，折叠按钮位于标题下方的 `SidebarUtilityStrip`，避免按钮覆盖或挤压版本气泡。
- 如果继续修复折叠按钮，优先检查 `SidebarCollapseButton` 的 Click 路径和 `ApplySidebarLayout`，不要再把它放回品牌行；默认展开宽度仍为 236 DIP，折叠为 78 DIP。
- `GameSaveCenterSettingsView.xaml` 根 UserControl 的 `MinWidth=1180`、`MinHeight=760` 用于让 Playnite 设置宿主默认打开更大；`SettingsShell` 的 MaxWidth、原有分类/滚动/保存按钮语义不变。若真实宿主仍忽略最小尺寸，再单独调查 Playnite 设置窗口宿主，不要用页面内部硬编码宽度强行撑破窗口。
- 本阶段源码/XAML/Release/Playnite 295/352 和 RenderHarness `render-qa OK` 均已通过；尚未取得重载后真实 Playnite 的按钮点击/窗口尺寸像素证据，后续需复核折叠、展开、设置窗口、Follow/浅色、DPI 和键盘焦点。

## 2026-08-25 UI-324 侧栏折叠书签与动画交接

- `AcrylicProductionShellView.xaml` 的折叠入口现在是贴在侧栏右边、靠近底部的 `SidebarCollapseButton`，使用 `AcrylicSidebarBookmarkButton` 书签模板；不要再把它放回品牌标题行或新增一个大号导航按钮。
- 品牌名称和 `SidebarProductionBadge` 保持原位置，版本气泡继续显示程序集版本；`SidebarContentLayer` 只负责标题和真实导航内容，书签位于独立覆盖层，不改变导航项的测量和事件。
- 展开/收起仍使用 `ApplySidebarLayout` 的 236/78 DIP 宽度和页面响应式重算；动画只淡出/淡入并做 4 DIP `TranslateTransform`，时长 110/170ms。`MotionEnabledProvider` 连接到 `DashboardView.MotionEnabled`，关闭动画、系统禁用动画和高对比度时必须同步切换。
- 本阶段 `validate-source.py`、`check-xaml.ps1`、WPF 静态审查（0 error）、Release、Playnite 295 通过/57 跳过和 RenderHarness `render-qa OK` 均已通过；真实 Playnite 重载后的书签点击、键盘焦点、浅色/深色/Follow 与 DPI 仍需人工复核。

## 2026-08-25 UI-325 侧栏折叠控件纠偏交接

- UI-324 的字面“书签”实现已被否定并替换：不要恢复 `AcrylicSidebarBookmarkButton` 或 Path 丝带。用户参考图要求的是侧栏底部的一体式普通圆角控制。
- 当前使用共享 `AcrylicSidebarCollapseButton`：展开态约 168×34 DIP，左侧图标、中间“收起侧栏”、右侧箭头；折叠态约 40×34 DIP，只显示居中的展开图标。控件位于侧栏底部，不挤压品牌名称和版本气泡。
- `ApplySidebarLayout` 显式设置折叠按钮尺寸/边距、按钮内容居中、`SidebarCollapseLabel`/箭头可见性，以及 `NavOverviewContent` 等导航内部 StackPanel 的折叠态居中；原有 236/78 DIP 侧栏宽度、真实导航、动画和页面重排保持不变。
- 质量边界：源码/XAML/Release/Playnite 契约及 `.tmp/ui-qa-sidebar-control-v1` 离屏 QA 需要保持通过；真实 Playnite 重载后仍需人工确认点击、键盘焦点、Light/Dark/Follow、125%/150% DPI，不能把 RenderHarness 当作宿主像素证据。

## 2026-08-25 UI-326 折叠态图标对齐与首页右侧卡片间距

- 用户最新反馈集中在三处：折叠后品牌/导航/设置图标不在同一中心线；风险卡圆点离标题太近；“需关注事项”卡高度偏紧。
- 当前实现已在 `AcrylicProductionShellView.xaml` 为七个导航图标设置 `TextAlignment="Center"`，并在 `ApplySidebarLayout` 中将折叠态品牌区和导航内容统一到 26 DIP 居中槽；不要仅修改某一个图标的 Margin。
- 首页 `OverviewView.xaml` 的风险卡首列为 14 DIP；关注事项滚动视口 `MaxHeight` 为 220 DIP，仍保留有限内部滚动、页面根滚动和真实 `OpenAttentionCenterCommand`。
- 本轮验证：source/XAML/差异门禁通过，WPF 静态审查 0 error、18 warnings、172 info，Release 0 warning/0 error，Core 59/59、Worker 199/199、Playnite 295 通过/57 跳过，`.tmp/ui-qa-sidebar-icons-v1/render-qa-report.txt` 为 `render-qa OK`。
- 交付前仍需保持真实宿主边界说明：本轮未重新取得 Playnite 重启后的逐像素折叠截图；不得把 RenderHarness 结果扩写为真实 Playnite 的 Light/Dark/Follow、DPI、键盘焦点或 Tooltip 已验收。
