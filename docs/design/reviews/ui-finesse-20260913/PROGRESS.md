# UI 精修任务账本

计划来源：`docs/design/UI_FINESSE_IMPLEMENTATION_PROMPTS_2026-09-13.md`。状态只使用：待开始、实施中、代码完成待验收、已验收、已满足无需修改、外部阻塞。

| ID | 状态 | 缺陷/实际变化 | commit | 自动证据 | 真实宿主证据 | 剩余动作 |
| --- | --- | --- | --- | --- | --- | --- |
| P00-01 | 已验收 | 已建立生产资源合并与最终生效入口映射，双来源风险入账 | fd573e4 | BASELINE.md；静态门禁通过 | 受控证据；嵌入待验收 | 后续变更更新映射 |
| P00-02 | 已验收 | 开发夹具改用生产资源链，覆盖固定混排、技术文本、按钮状态、输入/选择、状态徽章和有限表格 | 本阶段 | 双主题 1120×980 `finesse-fixture OK`；4 行表格、22 个按钮、CaptionOpacity=1 | 受控证据；不等同嵌入 | 后续阶段复用 `.tmp/ui-finesse-probe-20260913-{dark,light}` |
| P00-03 | 外部阻塞 | 已有安装身份核对、导航/详情/搜索/表格探针；本轮真实窗口仍不可枚举 | 本阶段 | 夹具与现有探针入口可执行 | `MainWindowHandle=0`，无 `summary.json` | 宿主可枚举后按人工脚本重跑；不循环重装 |
| P00-04 | 已验收 | 已建立 52 项续跑账本与问题分级 | fd573e4 | 本表、BASELINE.md | — | 每阶段独立更新 |
| P01-01 | 代码完成待验收 | 共享字体链按本机实际名称修正为 `Noto Sans SC`，夹具记录 CJK/Latin/数字/箭头命中；Inter 未安装、𠮷 在显式链未命中 | 本阶段 | 双主题夹具报告含 `FontGlyph` 逐字符结果 | 真实多机字体分发/系统 fallback 待验收 | 评估是否随包提供字体；不卸载用户字体伪造缺字环境 |
| P01-02 | 代码完成待验收 | 22/16/14/12 DIP 层级保留；共享语义样式统一 Fixed hinting、像素对齐和布局取整 | 本阶段 | 双主题同尺寸 PNG；RenderHarness 全量 `render-qa OK` | 100/150% 物理 DPI 待验收 | 真实 DPI 下复核字形裁剪与基线 |
| P01-03 | 已验收 | 新增共享 `GscTypographyCode`，路径/诊断使用 Code；数字继续使用 Numeric，单位/空值/变化样本固定 | 本阶段 | 夹具含 9→100%、KB/GB、时间、路径和错误码样本；PNG 可读 | 复制/屏幕阅读器真实路径待验收 | 继续复用生产页面已有 Tooltip/复制出口 |
| P01-04 | 已验收 | `GscTypographyCaption` 从 0.65 改为 1，层级由语义色表达，避免祖先透明度叠加 | 本阶段 | Dark/Light 报告 `CaptionOpacity: 1`；截图中辅助文字保持原尺寸 | 复杂宿主背景待验收 | 继续检查页面局部祖先 Opacity |
| P02-01 | 已验收 | Adaptive palette 与 Demo core 的浅/深语义层次保留，浅色状态色改为合格深色值 | 本阶段 | 双主题夹具 + 全量 Theme QA/PNG `render-qa OK` | Follow/用户主题真实窗口待验收 | 热切换证据归入 P02-04 |
| P02-02 | 已验收 | ContrastGuard 覆盖 Secondary/Muted 4.5、OnAccent 4.5、四类状态色 3.0、表面/轮廓合成检查 | 本阶段 | 每主题 11 项实际值；Light 最小 Muted=4.854、Dark 最小 ControlFill luminance diff=0.007，均通过 | 复杂真实背景仍待验收 | 新增 `Measure` 输出原始配对值 |
| P02-03 | 已满足无需修改 | 共享 ambient、阅读面、浮层和现有低成本表面已由 U12 建立；未向列表添加 Blur/阴影 | 本阶段 | 全量 Render QA 双主题/多尺寸通过；WPF 静态 0 error | 真实显卡/宿主成本待验收 | 保持单一共享表面入口 |
| P02-04 | 代码完成待验收 | 运行时 palette 明暗状态色、强调色、禁用色均走 DynamicResource；主题夹具支持 Light/Dark | 本阶段 | 11 项 ContrastGuard 与双主题 PNG 通过 | Light→Dark→Follow 20 次真实往返待验收 | 后续在受控窗口或宿主可枚举后重跑 |
| P03-01 | 代码完成待验收 | 共享按钮/图标按钮继续复用 WPF-UI 生产模板，覆盖 Normal/Hover/Pressed/Disabled/Focus 语义 | 6c3c238 | `finesseprobe` 22 个按钮、双主题；静态门禁与 Render QA 通过 | 真实 Pressed/Focus、Tooltip 和命令次数仍待宿主 | 宿主可枚举后做物理点击与键盘复核 |
| P03-02 | 代码完成待验收 | 搜索、TextBox、校验、长路径继续使用共享 Typography/输入资源，未新增局部输入体系 | 6c3c238 | `finesseprobe` 含中英、路径、错误码、输入/选择；源测试通过 | IME、复制、焦点和宿主字体仍待验收 | 真实窗口执行 IME/长路径回归 |
| P03-03 | 代码完成待验收 | ComboBox、Tab、Toggle 复用现有模板和动态主题资源，保留真实绑定/命令 | 6c3c238 | `render-qa` 双主题、多页、筛选控件可见文本通过；状态夹具通过 | Popup 边缘、键盘循环和物理开关仍待验收 | 宿主可枚举后补交互矩阵 |
| P03-04 | 代码完成待验收 | DataGrid、图标、分隔线走共享 Redesign/IconPack 入口，表格保留 Item/Recycling/列宽契约 | 6c3c238 | `gridprobe` 端点/滑块/PageUp/PageDown/Ctrl+End；4 个数据量通过 | 物理拖拽、Popup 裁切和真实宿主滚轮仍待验收 | 取得宿主后重放并记录原始字段 |
| P04-01 | 已验收 | `GscMotion` 读取 `MotionTokens.xaml`，缺宿主资源时回退 120/100/220/300ms；保留现有入口与 EaseOut | 本阶段 | 定向测试 8/8；夹具报告与 token 值一致；无新增散落毫秒值 | 真实系统动效设置待验收 | 继续从共享入口消费 |
| P04-02 | 代码完成待验收 | 现有 C# 动画从实例当前 Transform 接管，壳层 rapid-toggle 代理探针可收敛到最后状态 | 本阶段 | Render QA rapid-toggle `settled=True`；现有 Transform 回归通过 | 真实连续 20 次录屏/焦点待验收 | 补可重复行为采样 |
| P04-03 | 代码完成待验收 | 共享 Transform 克隆与动画入口保持实例级；未引入列表逐项动画或布局属性动画 | 本阶段 | 冻结 Transform 定向测试通过；Render QA rapid-toggle 状态收敛 | 100 次窗口/卸载生命周期待验收 | 真实宿主可运行后采集活动时钟/订阅 |
| P04-04 | 代码完成待验收 | `IsEnabled` 同时检查用户设置、系统动画和 High Contrast；现有立即终态路径保持 | 本阶段 | 现有无障碍/动效源码测试与静态门禁通过 | 运行中切换 High Contrast/动画设置待验收 | 宿主条件具备后执行组合矩阵 |
| P05-01 | 代码完成待验收 | 导航/页签复用既有状态持久化与页面切换实现，未引入第二套过渡 | 6c3c238 | `render-qa` 覆盖 8 页面、Tab、resize 和 rapid-toggle 收敛 | 真实 30 次切页及宿主焦点仍待验收 | 宿主可见后补 30 次切页 |
| P05-02 | 代码完成待验收 | Inspector、展开区、侧栏和页尾继续使用 U12 的有限滚动/可达性结构 | 6c3c238 | shellqa 覆盖 Media/Maintenance/Task inspector 开关和尾部几何 | 物理滚轮、Popup 裁切仍待验收 | 宿主可枚举后检查尾部操作 |
| P05-03 | 代码完成待验收 | Loading/Refreshing/进度由真实状态字段与 Presenter 驱动，未替换为静态 Demo | 6c3c238 | statefixtures 覆盖 Ready/Empty/Loading/Error/Stale/Offline；actionItems 随状态变化 | 真实 Worker 进度节奏仍待验收 | 真实宿主执行刷新/取消/恢复 |
| P05-04 | 代码完成待验收 | Toast/Banner/Dialog 继续保留真实通知、错误详情、取消和 Escape 语义 | 6c3c238 | 状态夹具保留 stale banner；已有反馈/可访问性源测试通过 | 多通知堆叠、模态焦点和 Escape 物理回归仍待验收 | 宿主可见后补通知矩阵 |
| P06-01 | 代码完成待验收 | 页面仍使用有限视口、Item/Recycling 和共享资源，未增加列表行布局动画 | 6c3c238 | `render-qa` 记录 layout/render 字段；静态审查无新增热点 | offscreen timing 不是真实呈现帧，宿主负载采样待验收 | 用同条件真实宿主采 p50/p95/max |
| P06-02 | 代码完成待验收 | 刷新/通知沿用现有批处理与去抖入口，未改业务事件契约 | 6c3c238 | `BatchObservableCollectionTests`、`DebouncedRefreshTests`、`LatestRequestCoordinatorTests` 通过 | 真实高频 Worker 通知延迟仍待验收 | 真实宿主采集事件量/延迟 |
| P06-03 | 已验收 | 缩略图加载支持并发窗口、取消、坏图回退、缓存和陈旧路径保护 | 6c3c238 | `thumbnailprobe OK`：120 项、100 次窗口、peak=3、active=0、cache=96/96；AsyncThumbnail 测试通过 | 不把 synthetic PNG/隐藏 STA 窗口当作真实 Playnite 视频/DPI 证据 | 保持现有缓存边界 |
| P06-04 | 已验收 | 大表保留虚拟化、Item 滚动、端点完整性与分页锚点；夹具升级为 1000/5000/20000 | 6c3c238 | `scaleprobe OK` + `gridprobe OK`；50/400/2000/4468 行端点/滑块/分页通过 | L32 `ScrollIntoView` 离屏基线不具结论，物理滚轮/真实宿主待验收 | 宿主可见后做大库回放 |
| P07-01 | 代码完成待验收 | Shell 全局上下文、选中游戏背景和 footer 继续由单一壳层资源承载 | 6c3c238 | shellqa 检查 720–1040 壳层及完整 ambient bounds | 真实 Playnite 游戏背景/嵌入 Dashboard 待验收 | 宿主可枚举后逐页打开 |
| P07-02 | 代码完成待验收 | 首页保留风险卡、当前游戏和主操作真实命令/Binding | 6c3c238 | `render-qa` Overview 多尺寸；活动/保护列表和根滚动字段通过 | 真实当前游戏数据和物理操作仍待验收 | 宿主复核风险与主操作 |
| P07-03 | 代码完成待验收 | 存档中心保留长路径、候选/历史/比较和页签命令 | 6c3c238 | `render-qa` Save 四 Tab、有限表格和 4/4 可读行通过 | 真实备份/加载/复制安全操作待验收 | 仅在宿主可见后执行真实操作 |
| P07-04 | 代码完成待验收 | 媒体中心保留收件箱、详情、来源和页尾批量命令 | 6c3c238 | statefixtures + shellqa + `render-qa`：媒体短窗与尾部几何通过 | 真实媒体预览、归类/忽略操作待验收 | 宿主可见后做非破坏性操作 |
| P07-05 | 代码完成待验收 | 任务中心保留状态、详情、筛选与有限队列滚动 | 6c3c238 | shellqa 覆盖 1040/1100/1366；TaskGrid 端点证据通过 | 真实 Worker 任务流和物理分页待验收 | 宿主可见后回放 |
| P07-06 | 代码完成待验收 | 修改器中心保留长英文、来源错误和导入候选语义 | 6c3c238 | `render-qa` Trainer 多 Tab、长文本滚动和候选 ComboBox 通过 | 真实工具目录/导入操作待验收 | 宿主可见后检查错误详情 |
| P07-07 | 代码完成待验收 | 维护中心保留风险优先、诊断/进程/日志多表格和折叠 Inspector | 6c3c238 | statefixtures + shellqa + `render-qa` 多表格/compact inspector 通过 | 真实诊断命令和长日志滚动待验收 | 宿主可见后非破坏性复核 |
| P07-08 | 代码完成待验收 | 设置页保留五分类、保存/取消、导入导出和错误反馈 | 6c3c238 | `render-qa` Settings 双主题、多尺寸、resize 通过；设置源码测试通过 | 真实设置窗口宿主、保存/导入安全操作待验收 | 宿主可见后执行保存前检查 |
| P08-01 | 代码完成待验收 | 共享文本使用 Fixed hinting、像素对齐和布局取整，未改业务布局 | 6c3c238 | Typography/字体夹具与双主题 PNG 通过 | 100–200% 物理 DPI 和多机字体仍外部阻塞 | 真实 DPI 宿主可用后复核 |
| P08-02 | 已验收 | 逻辑尺寸矩阵覆盖常用、紧凑和高分辨率布局 | 6c3c238 | `render-qa` 覆盖 1040/1100/1280/1366/1536/1600/1707/1920/2048/2560/3840，并有 shell 720/960/980/1040 | 这些是离屏逻辑尺寸，不等同物理窗口 | 保持尺寸矩阵回归 |
| P08-03 | 外部阻塞 | 无需新增页面代码；多屏、窗口恢复和宿主窗口状态依赖真实可见宿主 | — | 离屏 resize 只覆盖尺寸恢复 | 当前宿主不可枚举，无真实多屏/恢复证据 | 获得宿主后补矩阵 |
| P08-04 | 代码完成待验收 | 壳层、表格、页尾和 Inspector 几何保持可达，Popup/端点未另起布局 | 6c3c238 | shellqa/gridprobe 检查表格端点、footer、inspector reachability | 物理命中、Popup clipping 和拖拽仍待验收 | 宿主可见后补 hit-test |
| P09-01 | 代码完成待验收 | WorkspaceStatePresenter 与各页真实状态字段覆盖完整状态矩阵 | 6c3c238 | statefixtures：6 状态 × Media/Details/Maintenance/NextSteps，action/banners/presenters 随状态变化 | 真实六页宿主状态切换仍待验收 | 宿主可见后补状态切换 |
| P09-02 | 已验收 | 术语和混排样本固定于共享夹具，路径/单位/状态字形语义已统一 | 6c3c238 | `finesseprobe` 与状态夹具覆盖中英、数字、路径、错误码；U12 术语文档同步 | 屏幕阅读器实际朗读仍单列于 P09-03 | 保持术语入口单一 |
| P09-03 | 代码完成待验收 | AutomationProperties、键盘焦点和共享控件语义保持，未牺牲真实绑定 | 6c3c238 | AccessibilitySourceTests、KeyboardFocusSourceTests、静态门禁和控件命名通过 | 真实键盘/屏幕阅读器仍待宿主 | 宿主可见后做 Tab/读屏回归 |
| P09-04 | 代码完成待验收 | Light/Dark、动画关闭/高对比回退走共享资源和立即终态路径 | 6c3c238 | 双主题 Render QA、ContrastGuard、动效源码测试通过 | 系统高对比度/禁用动画现场仍待验收 | 宿主可见后补组合矩阵 |
| P10-01 | 外部阻塞 | 当前仅有离屏 layout/render 和 rapid-toggle 代理，未宣称屏幕帧预算 | — | `render-qa` 记录 timing；代理 maxFrameGap 不作为 presented frame | 无真实呈现帧采样，无法完成 p50/p95/max | 取得宿主后按提示词采样 |
| P10-02 | 外部阻塞 | 未执行 30 分钟真实窗口耐久测试 | — | 缩略图 100 次窗口探针仅为受控短测 | 无真实 Playnite 长时会话/释放时间序列 | 获得宿主后执行并保留原始数据 |
| P10-03 | 代码完成待验收 | 玻璃、动效和缩略图已有共享回退/取消路径，未新增强制 Blur | 6c3c238 | 静态审查、双主题、thumbnailprobe 和动效测试通过 | 低性能 Tier/高负载现场成本待验收 | 宿主可见后做降级采样 |
| P10-04 | 外部阻塞 | 真实路径走查依赖可操作的 Playnite 主窗口和真实数据 | — | 受控 fixture 不包含真实文件路径/业务写入 | `MainWindowHandle=0`、无 `summary.json` | 宿主条件具备后按清单走查 |
| P11-01 | 代码完成待验收 | 八入口以共享 Typography/Palette/Controls/Motion/State 入口收敛 | 6c3c238 | 八页面、多尺寸、双主题、状态/壳层报告均通过 | 真实宿主并排视觉评分仍待验收 | 宿主可见后按八维评分 |
| P11-02 | 已验收 | 回归与跳过原因已记录，门禁可重复执行 | 6c3c238 | Release 0 warning/0 error；Core/Worker/Playnite 定向与全量无失败；source/XAML/WPF/render QA 通过 | 外部宿主项按本表单列，不伪造通过 | 每次代码变更复跑相关门禁 |
| P11-03 | 外部阻塞 | 包流程需 clean working tree，当前用户未跟踪 `src.zip` 阻止安全打包；宿主加载另受窗口不可枚举影响 | — | 一键链已完成构建/测试阶段；包装阶段明确因 `?? src.zip` 停止 | 无法在不删除/移动用户文件前生成可签收新包，且无嵌入 Dashboard | 用户处理 src.zip 或提供干净工作树后重试 |
| P11-04 | 已验收 | 本账本、基线、工作日志、项目记忆和交接文档已同步当前证据与边界 | 6c3c238 | 当前探针报告、最终 Render QA、静态门禁和一键构建测试结果已登记 | 真实宿主缺口保留为外部阻塞 | 后续代码变更按本表续跑 |

## 当前执行顺序

P00-02 → P01-01～04 → P02-01～04 → P03-01～04 → P04-01～04；随后复用已存在的 U12 结构证据关闭可满足项，再处理 P05～P11 的新增证据和剩余缺陷。每个独立阶段都必须更新本表、`WORKLOG.md`、`PROJECT_MEMORY.md`，通过验证后中文提交并推送。
