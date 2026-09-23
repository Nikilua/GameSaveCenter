# R23-03 代表页面终审

日期：2026-09-23
状态：已满足受控页面终审，真实宿主环境待验
审计基线：`16f4dee6`（既有页面盘点）；当前代码与验证身份：`7a4ba2a94da870c832e59f3ee025f9e34325d175`（此后只追加文档）

## 终审方法

- 选取概览、存档、媒体、工具、任务、维护、设置、壳层八个代表入口，先核对当前生产 XAML 的状态承载、命令/Automation 入口和滚动/选择边界，再复用对应 Q/R 证据。
- “空、错、加载”按页面业务语义判断：数据工作区必须有空/错误/加载状态；设置页没有可为空的结果集，改查验证错误、冲突和导入预览；壳层没有独立数据页，改查选框无结果、加载副标题和 Worker/工具状态。
- 可读性证据来自当前生产资源、合成 DTO/隔离 WPF、Light/Dark fixture 和已有行为测试；旧宿主截图仅作为路由/历史参考，不作为当前提交的真实呈现签收。

## 八类页面终审索引

| 页面 | 当前实例与信息重点 | 空/错/加载证据 | 操作可达证据 | Demo 对应/例外与待验边界 |
| --- | --- | --- | --- | --- |
| 概览 | `OverviewView` 的下一步、健康状态、最近活动、快照/任务摘要；状态 pill 与相对/完整时间并存 | `R20-01`、`R20-04`～`R20-08`、`R22-01-OVERVIEW-TIME`；活动区有 `WorkspaceStatePresenter State="Empty"`，Worker/快照异常使用可读错误/部分可用提示 | `R10-01`～`R10-04`、`R21-01`、`R21-02-DASHBOARD-AUTOMATION`；复制、重新加载、当前游戏/选框入口保留 | 页面层级和资源链沿用 Demo-first 生产壳层；未把历史 overview PNG 当当前呈现证据，真实 Playnite、UIA/读屏、DPI/跨屏待验 |
| 存档 | `SaveCenterView` 的候选表、历史、恢复准备/保护、差异/策略模板；`SaveDataGrid` 与独立详情滚动 | `R11-01`～`R11-08`、`R12-01`～`R12-08`、`R16-02`～`R16-08`、`R22-02`/`R22-07`；候选/历史具备 Empty、Loading、Unavailable、Error 和重试，PreRestore/冲突/失败阶段保留 | `R04-02`/`R04-05`、`R06-03`/`R06-04`、`R21-02-SAVECENTER-AUTOMATION`、`R22-07`；复制、重试、恢复和撤销命令未被视觉收口替换 | Demo 的卡片/表格/危险确认结构在生产资源链中保留；真实恢复只允许合成/隔离验证，不能用真实存档验收 |
| 媒体 | `MediaCenterView` 的选择框、来源/分类、缩略图、详情和批量动作；`MediaDataGrid`/有限列表保持 | `R00-06`、`R14-01`～`R14-08`、`R21-02-MEDIA-BATCH-EMPTY-GUARD`、`R22-03`/`R22-05`；无媒体、来源错误、缩略图/视频回退、批量空选择和忙态均有单项证据 | `R05-02`～`R05-08`、`R06-01`～`R06-08`、`R07-01`～`R07-08`、`R21-02-MEDIA-SELECTORS-AUTOMATION`；列选择、复制、滚动、批量命令和详情导航均保留 | Demo 的四行表格/选择/滚动边界沿用恢复生产基线；真实媒体、鼠标滚动、编解码、Playnite 呈现和宿主性能待验 |
| 工具 | `TrainerCenterView` 的工具列表、导入、设置和诊断；列表使用 Recycling/有限滚动 | `R21-02-TRAINER-IMPORT-AUTOMATION`、`R21-02-TRAINER-SETTINGS-AUTOMATION`；工具列表显式 Empty/Loading，导入失败与诊断警告通过结果/帮助文本表达，没有伪造通用 Error presenter | `R05-01`、`R06-03`、`R07-01`/`R07-05`、`R18-03`/`R18-04`；导入、编辑、保存和详情入口有名称/状态 | 工具页不套用存档页空错卡片；保留自身导入/诊断语义和列表性能。真实 EXE/ZIP/CT、宿主输入、UIA/读屏、DPI/呈现待验 |
| 任务 | `TaskCenterView` 的筛选、任务表、详情、阶段/进度、取消/重试/复制 | `R15-01`～`R15-08`、`R21-02-TASK-PROGRESS-PEER`、`R22-01-TASK-PAGE-TIME`、`R22-06`；源码和状态绑定明确 Empty、FilterEmpty、Loading、Error，失败详情与技术详情可展开 | `R06-06`、`R21-02-PROGRESS-AUTOMATION`、`R21-03`/`R21-04`、`R22-06`/`R22-08`；离页不取消、返回可追踪、取消是显式命令 | Demo 的状态 pill/进度层级沿用生产模板；真实 Worker 长任务、离页返回、UIA/读屏、呈现帧和宿主性能待验 |
| 维护 | `MaintenanceView` 的诊断、云端队列、保留/隔离/存储和 Finding 详情；页面派生表格状态单独保留 | `R13-01`～`R13-08`、`R17-01`～`R17-08`、`R20-06`/`R20-08`、`R22-01-MAINTENANCE-ACTION-TIME`；诊断空态、云端 degraded、认证/失败/重试和加载更多均有证据 | `R06-03`/`R06-05`、`R07-01`/`R07-06`、`R21-02-MAINTENANCE-CLOUD-SELECTORS`、`R21-02-MAINTENANCE-PROCESS-MAPPING`；筛选、定位、重试、复制和分页入口可达 | Demo-first 的状态语气与生产资源一致，但云端菜单/宿主状态仍受真实 Playnite/网络外部条件限制；不写真实云端 |
| 设置 | `GameSaveCenterSettingsView` 的分组搜索、路径/数值编辑、主题/动态效果、安全和导入导出 | 页面没有可为空的结果集；以 `R04-02`/`R04-05`/`R04-07`、`R16-01`/`R16-05`～`R16-08` 覆盖校验错误、异步校验、冲突、导入预览和生效条件 | `R05-01`、`R10-03`、`R16-01`、`R21-02` 设置/选择器证据；搜索、焦点、路径复制、保存/取消和冲突处理保留 | 设置采用同一生产调色板和共享输入，不强行套用工作区 Empty 卡；真实 Playnite 设置宿主、读屏、DPI/IME 待验 |
| 壳层 | `AcrylicProductionShellView` 的导航、当前游戏选框、页头动作、状态指示和页面切换；保留现有滚动/选框系统 | `R00-08`、`R08-03`/`R08-05`、`R10-01`～`R10-04`、`R21-01`/`R21-07`/`R21-08`；无结果/IME/焦点回返、页切换、加载副标题和 Worker/工具指示有证据 | `R00-08` 实际 WPF 路由、`R21-01` 键盘追踪、`R21-07` 焦点视觉；导航、Ctrl+F、Esc/Enter、清除和页头命令保留 | Demo-first 以 `AcrylicProductionResources`/`Redesign` 生产链为准；Playnite 宿主菜单不在插件 visual tree（R02-06 外部阻塞），真实嵌入/跨屏/最终呈现待 R23-04 |

## 可读性与状态结论

- 当前生产资源和页面状态入口没有发现需要另起设计体系的缺口；页面级派生表格、状态 pill、详情滚动容器和壳层选框均按实际 key/绑定单独核对。
- Light/Dark fixture 的代表控件已验证布局、文字、按钮/输入/选择/表格代理和共享状态层；`R23-02` 矩阵已明确哪些状态是基类继承、哪些是页面派生、哪些不适用。
- 本报告不把源码中有 `WorkspaceStatePresenter` 写成真实所有页面的屏幕可读性，也不把自动化名称写成 UIA/读屏通过；R23-04 需要固定当前构建身份，在隔离非空宿主复测并保留精选原图。

## 2026-09-23 当前 main 复核

- 本轮没有新增业务/页面代码。当前八个实际入口复核为 `src/GameSaveCenter.Playnite/Views/OverviewView.xaml`、`SaveCenterView.xaml`、`MediaCenterView.xaml`、`TrainerCenterView.xaml`、`TaskCenterView.xaml`、`MaintenanceView.xaml`、`src/GameSaveCenter.Playnite/Settings/GameSaveCenterSettingsView.xaml` 与 `AcrylicProductionShellView.xaml`；它们分别承载首页、存档、媒体、工具、任务、维护、设置、壳层。以当前文件/绑定和命令入口为准，沿用表中的 Demo 对应或业务语义例外。
- 当前 main 隔离 Release 重建：XAML `24/24`、solution `0 errors`；Playnite `net462`、Playnite.Tests `net472`，有两条既有 `MediaCenterView.xaml.cs:706 CS8602` warning。
- 当前抽样行为门禁：`WorkspaceStateSourceTests` `9 passed / 1 skipped`、`R21AutomationValueBehaviorTests 21/21`、`R21FocusVisualRegressionBehaviorTests 1/1`、`R21DisabledHiddenBehaviorTests 2/2`、`R10RecentAccessBehaviorTests 2/2`、`R23ProductionResourceStateBehaviorTests 3/3`；合计 `38 passed / 0 failed / 1 skipped`。唯一 skip 是 `WorkspaceStateSourceTests.SharedWorkspaceStatePresenterExistsAndIsUsedAcrossPages`，TRX 原因明确为断言针对已撤销的“今日工作台”UI 架构，本次没有把它计作通过。每类单独 testhost，均 exit `0`。
- 可达性证据包含 WPF AutomationPeer 上的名称/状态/动作、媒体空选择不写 metadata 的负例、Save 可用/禁用与安全原因、首页最近访问入口的命令路由；状态证据包含各页真实 presenter/绑定契约及源页面加载/空/错误语义。R23-02 的实际导航焦点/禁用和四种 DataGrid 派生样式也在当前代码基线上 `3/3` 重验。
- 以上属于生产 XAML、生产资源和隔离 STA WPF/合成数据的受控证据，不等于正常 Playnite package-host 的现场文字可读性、UIA/屏幕阅读器、用户鼠标/键盘、最终呈现像素或跨 DPI 验收。R23-04 最近仍遇到 CEF `platform_channel ... 拒绝访问 (0x5)`，窗口出现不能证明正常宿主；Demo 原目录不可用，继续以恢复生产基线按 Demo-first 核对。没有读写真实存档/媒体/云端或外发诊断。

## 下一步

下一步转 `R23-04` 正常可枚举隔离 Playnite 会话；若 CEF `0x5` 阻塞继续存在，则记录限制并推进依赖已满足的 Q/R 项。本报告不把旧宿主截图当当前通过，也不允许使用真实存档、媒体或用户云端验证。
