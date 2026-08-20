# Demo-first 页面迁移证据矩阵

> 依据 `C:\Users\lopmatu\.codex\attachments\40558e57-e0ac-4d93-b138-75a31d611b90\goal-objective.md` 建立。视觉基准只取 `GameSaveCenter.AcrylicFork/src/GameSaveCenter.Playnite/Design/`；生产 `Views` 只用于核对真实数据、命令、绑定和生命周期。此表是当前事实，不代表总目标已完成。

## 横向验收项

| 类别 | 当前状态 | 生产证据/保留边界 | 仍需收口 |
| --- | --- | --- | --- |
| Tokens / Colors | 部分收口 | `Themes/DesignTokens.xaml`、`AdaptiveThemePaletteFactory.ApplyDemoCoreResources` 已接入字体和 Demo 核心浅/深色关系；高对比度仍走系统路径。 | 逐项核对 Demo `DesignTokens`、`DesignColors*`、`DesignControls`，补齐未覆盖的图表、按钮和辅助界面色阶。 |
| Shell / 页面框架 | 部分收口 | `Views/AcrylicProductionShellView.xaml` 保留当前游戏选框和项目 Tab chrome；标题已接入 Display 字阶。 | 对照 `DesignShellView.xaml` 核对侧栏、顶栏、页头、页脚、主工作区边界和不同尺寸命中区域。 |
| 字体 / 密度 | 部分收口 | UI、Display、Cascadia Mono 和中文回退链已集中到生产令牌；七页已使用共享生产资源。 | 逐页核对字号、字重、行高、单行截断和 CJK 回退的真实宿主结果。 |
| 表格 | 部分收口 | Save、Media、Maintenance、Task 基于共享 `GscRedesignWorkspaceDataGrid`，保留排序、列宽调整、选中行、Item scrolling 和 Recycling 虚拟化。 | 对照 Demo `LabGrid` 逐页检查表头高度、排序箭头、行高、长消息截断、按钮命中和被遮挡/隐藏可点击项。 |
| Disclosure / segmented / controls | 部分收口 | 生产已有共享 `Redesign.xaml` 控件资源；Settings 左侧分类栏使用 `LabSegmented`，Trainer/Save/Media/Maintenance 外层继续使用项目 Tab chrome，业务折叠区保留真实状态。 | 对照 `LabDisclosure`、按钮、输入框、ComboBox、开关和展开动画；不要把项目工作区 Tab 再替换成 Demo `LabSegmented`。 |
| 滚动 / 选框 / 性能 | 保留并需持续回归 | 生产 ScrollBar、ScrollViewer、当前游戏选框、列表/表格虚拟化和媒体缩略图异步加载未替换；UI-267 几何审计已通过。 | 每次页面改动都要验证父子滚动、横向表格滚动、DPI、resize、键盘焦点和大列表性能。 |
| 真实功能 | 保留并需持续回归 | ViewModel、Command、Worker/Playnite 生命周期、异步/错误/取消/安全语义仍在生产页面；Save 立即备份/全部备份、Media Inspector 和真实操作入口已有源码/渲染证据。 | 在可识别 Playnite 宿主执行真实备份、媒体预览/归类/忽略/保留副本、任务取消和设置保存等安全操作。 |
| Toast / Dialog / Inspector / 状态 | 部分收口 | 生产通知、确认/选择对话框和 Inspector 继续由真实请求与选中项驱动；页面状态控件和语义色已接入 Demo 核心色板。 | 对照 Demo 空/加载/错误/提示文案布局，验证层级、Escape、焦点、取消和错误详情不被遮挡。 |
| 证据与交付 | 未完成 | UI-272 的 Release 构建、测试、source/WPF 静态门禁、RenderHarness 双主题多尺寸持续通过；真实宿主审计已取得 Settings `EmbeddedPlaynite` 截图，但 Dashboard 仍未嵌入捕获。 | 真实 Playnite Dashboard 七页逐页像素、DPI、主题、键盘、命中区域和真实操作证据仍未完全收口；`ControlledDashboardCaptured=true` 不能替代 `EmbeddedDashboardCaptured=true`。 |

## 七页结构矩阵

| 页面 | Demo 结构锚点 | 当前生产结构锚点 | 当前判断 | 下一项高置信度检查 |
| --- | --- | --- | --- | --- |
| Overview | `Design/Pages/OverviewPage.xaml`：Hero + 当前游戏摘要、连续指标带、最近任务、活动记录、风险/保护、图表与进度。 | `Views/OverviewView.xaml`：`OverviewHeroColumn`、`OverviewCurrentGameColumn`、`OverviewStatStrip`、任务/活动表面及真实 Snapshot/Task 状态。 | 结构已大体对齐；需确认无额外生产堆叠、卡片间距和活动区在真实宿主可达。 | 逐段对照 Demo 顺序与隐藏/叠放命中关系，重点检查 Hero、当前游戏卡和底部风险/保护区域。 |
| Save | `SavesPage.xaml`：分段导航、规则摘要、历史表 + Inspector、路径校验、策略、比较与保留。 | `Views/SaveCenterView.xaml`：项目当前 `TabControl` chrome（明确例外），四个真实面板、共享 DataGrid、Inspector、真实扫描/校验/详情/备份入口。 | 内容和功能基本对应；导航外观按用户明确例外保留当前项目 Tab chrome。 | 核对“立即备份当前游戏”和“全部备份”在 Demo 结构中的可见位置、表格排序/列宽/状态列和比较保留布局。 |
| Media | `MediaPage.xaml`：连续指标带、分段导航、待归类表/详情、当前媒体卡片、来源规则双栏。 | `Views/MediaCenterView.xaml`：Tab chrome 例外、`MediaSummaryPanel`、待归类 DataGrid、当前媒体虚拟化卡片、Inspector、来源规则 `UniformGrid`。 | 结构和真实功能已对应；待归类截图/视频详情与批量操作必须继续可达。 | 抽查 Inbox 选中行、图片/视频预览、归类/忽略/保留副本按钮命中及窄宽 Inspector 切换。 |
| Maintenance | `MaintenancePage.xaml`：诊断概览、诊断/清理/审计/设备分段、指标、表格、标题完整的折叠栏。 | `Views/MaintenanceView.xaml`：Tab chrome 例外、健康卡、环境检查、诊断操作、设备/保留/异常/进程面板及内部诊断分段。 | 诊断概览顺序和主要指标已迁移；需持续防止无标题折叠栏和多表格密度回归。 | 核对每个 `LabDisclosure` 等价区域的标题/箭头/内边距、五类表格表头和诊断嵌套分段的可见性。 |
| Tasks | `TasksPage.xaml`：四项指标、筛选/搜索、`LabDisclosure` 更多筛选、任务表 + 进度、右侧详情。 | `Views/TaskCenterView.xaml`：`TaskSummaryPanel`、`TaskFilterBar`、`TaskMoreFiltersExpander`、真实任务 DataGrid、进度和 Inspector。 | 信息顺序已对应，真实搜索/过滤/取消/重试保留。 | 核对表格长消息单行截断、进度与状态对比、详情滚动和更多筛选在窄宽下的命中区域。 |
| Trainer | `TrainerPage.xaml`：四项分段导航、工具列表/详情、导入确认、在线目录、发行版本。 | `Views/TrainerCenterView.xaml`：项目 `TrainerTabControl` / `TrainerTabItem` 四个 `TabItem`，真实工具编辑和目录/发行命令。 | 按用户 Tab 例外恢复项目 Tab chrome；页面内容、真实绑定、虚拟化和 Inspector 仍对应 Demo 结构。 | 核对四个 Tab 的项目样式可见性、工具编辑复选框/按钮、导入确认与发行下载状态的可达性，不再要求 `LabSegmented`。 |
| Settings | `SettingsPage.xaml`：左侧分类栏、右侧单一内容区、设置卡片和控件。 | `Settings/GameSaveCenterSettingsView.xaml`：`SettingsSectionTabs`、五分类面板、`SettingsScroller`、真实验证/保存/导入导出。 | 信息架构已对应，设置主题资源已与 Shell 共用核心色板；`artifacts/ui-host-audit-ui271/settings/embedded-current/` 已取得真实宿主截图。 | 核对分类选中态、输入/开关/下拉/按钮样式、校验错误和低高度滚动；Dashboard 七页仍需同一宿主证据路径。 |

## 明确不能回退的例外

- 生产项目的工作区 Tab chrome 不使用 Demo 的外层 Tab UI；Save、Media、Maintenance 的真实 Tab 结构和嵌套内容保持当前项目实现。
- 当前游戏选框的外观、尺寸、位置、交互、绑定和切换逻辑不替换；只调整周围布局。
- 生产 ScrollBar 外观、ScrollViewer 交互、虚拟化和媒体异步缩略图处理不替换为 Demo 实现。
- Demo 的 Mock 数据、演示按钮和右上角样板色板不进入生产；所有页面继续绑定真实 ViewModel/Command/Worker 状态。

## 当前交付边界

最近代码阶段：UI-272（回滚 Trainer Demo 分段栏，已提交 `f6f915d`）；构建证据：`artifacts/gsc-b/ui-272-trainer-tab-rollback-v2`；离屏证据：`artifacts/ui-qa/ui272-trainer-tab-rollback-v1/render-qa-report.txt`；最近宿主证据阶段：`artifacts/ui-host-audit-ui271`（Settings 嵌入证据，Dashboard 未捕获）。

现有离屏 RenderHarness 与静态审计只证明生产页面可构建、可渲染且在覆盖尺寸下没有已知高/中等级几何问题；在可识别 Playnite 生产宿主中完成逐页像素、DPI、主题、键盘焦点、命中区域和真实操作前，不得宣布总目标完成。
