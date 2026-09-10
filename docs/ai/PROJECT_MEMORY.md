# GameSaveCenter AI/Codex 长期项目记忆

> 维护时间：2026-09-10

## 2026-09-10 主题感知线性图标包

- 用户提供的 `GameSaveCenter_IconPack_v2_round-flat.zip` 的 SVG 是 24×24、透明底、`currentColor`、低线密度线稿；WPF 生产端通过 `Controls/ThemeAwareIcon.cs` + `Themes/GscIconPack.xaml` 的 Geometry/Path 渲染，不能改回依赖 Segoe MDL2 字形或引入只支持单色/固定底图的 PNG。
- `GscLineIcon` 是共享样式，Path 的 Stroke/可选 Fill 绑定控件 `Foreground`；导航图标从 RadioButton 前景继承，状态图标继续使用 `Gsc*Brush` 语义色。资源由 `WpfUiProduction.xaml` 统一合并，页面不应逐个复制图标 Geometry。
- 当前接入范围包括生产导航与侧栏品牌、设置标题/分组（含常规与目录、设置迁移）、首页活动状态、媒体来源、维护目录、修改器列表、任务搜索、共享游戏图标 fallback、Disclosure chevron 及隐藏兼容 Dashboard 的对应操作；真实命令、Binding、分页/虚拟化、焦点和自动化名称未改。
- 不把压缩包中的 plugin PNG 直接覆盖 `src/GameSaveCenter.Playnite/icon.png`：它是透明线稿导出，不适合作为 Playnite 清单图标。扩展清单图标仍保留现有可见 PNG，界面内 plugin-main fallback 使用矢量 Geometry。
- 本轮又补齐 ZIP 中 `section-general-directory` 与 `section-migration` 到设置页对应标题；Release Playnite 构建 `0 warning/0 error`，Playnite `432/495`（432 通过、63 跳过、0 失败），源码/XAML 门禁通过；`.tmp/icon-qa-settings-icons-20260910/render-qa-report.txt` 为双主题、多尺寸 `render-qa OK`。这是离屏 WPF 证据，不能写成真实 Playnite 用户主题/DPI/清单图标已验收。

## 2026-09-10 共享表格内容视口固定到顶部

- 用户现场仍能看到待归类表格列头下方的大块空白，因此仅靠 `DataGrid` 的 `VerticalContentAlignment=Top` 和宿主属性绑定不够；Playnite/FusionX 的短窗口测量或滑块端点可能重新安排真实 `ItemsPresenter`。
- `Redesign.xaml` 的共享 DataGrid 模板现将外层 `PART_ScrollContentPresenter` 和外层 ScrollViewer 的 `ItemsPresenter` 显式设为横向拉伸、纵向顶部对齐，保证虚拟化行在有限内容视口的首端布局；不会改变命令、Binding、Selection、Item 滚动或 Media 大数据例外。
- 证据：`MediaWindowAnchorContractTests` `10/10`；`.tmp/qa-table-scroll-20260910-final/render-qa-report.txt` `render-qa OK`。离屏证据使用 1.00 DIP，不能替代真实 Playnite 窗口化/2K 与非 100% DPI 复验。

## 2026-09-10 设置窗口主题优先级与生产导航对齐

- 设置页是 Playnite 的 `UserControl`，实际可能位于独立设置 `Window`。`AdaptiveThemePaletteFactory` 的 FollowPlaynite 资源解析必须先检查当前 Window 及 Owner 链，再检查控件局部视觉资源和 `Application.Current`；不能让 `TryFindResource` 先穿过应用范围拿到旧深色值。
- 这一优先级只影响 FollowPlaynite；`GameSaveCenterThemeMode.Light/Dark` 仍是用户明确选择的覆盖模式。窗口资源不存在时，控件本地资源和应用资源仍提供兼容回退。
- `AcrylicNavItem` 与生产壳层七个导航内容组均显式 `VerticalAlignment=Center`；图标和文字保持同一水平行，收起状态只改变可见性和内容对齐，不改变导航命令。

## 2026-09-10 Media 待归类表格内容视口对齐

- 用户反馈的“滚到底后中下部有数据、上部空白”不是 Media 专属数据分页或 `Standard` 虚拟化本身造成；共享 `GscRedesignDataGridTemplate` 的 `ScrollContentPresenter` 原先未绑定 `HorizontalContentAlignment` / `VerticalContentAlignment`，导致 `VerticalContentAlignment=Top` 没有落到真实内容视口。
- `Redesign.xaml` 已为外层 `DG_ScrollViewer` 和内层 `PART_ScrollContentPresenter` 补上两项 `TemplateBinding`，使 DataGrid 的 `Top` 对齐贯穿真实内容视口。Media 的大数据保护策略仍原样保留：`VirtualizationMode=Standard`、`ScrollUnit=Item`、`EnableColumnVirtualization=False`、`DataGridStarFill.Enabled=False`。
- `MediaWindowAnchorContractTests` 增加源契约断言；定向契约测试 `18/18`，最终全量 Release 回归为 Core `76/76`、Worker `310/311`（1 skip）、Playnite `429/492`（63 skip），0 失败，`render-qa` 报告为 `OK`。这只证明离屏 WPF 端点和模板结构，真实 Playnite/FusionX 的物理拖动仍需人工验收。

## 2026-09-10 页面外围重复环境材质矩形

- 第二张图中的页面外围矩形来自页面级 `AmbientMaterialLayer` 的圆角宽域渐变，不是需要再次铺开的图片层；Shell 已经拥有跨侧栏、页面和 footer 的单一环境坐标系。
- `AmbientMaterialLayer` 新增 `IsShellLayer`。页面实例保持挂载但通过 `GscAmbientPageOpacity=0` 透明；Shell 实例标记为 `True`，内部洗色保持可见并由外层 `GscShellAmbientOpacity` 控制。不要重新让页面层和 Shell 层同时绘制宽域洗色。
- 本轮资源/壳层定向测试通过，Release 全量为 Core `76/76`、Worker `310/311`（1 skip）、Playnite `429/492`（63 skip），构建 0 warning/0 error；干净 RenderHarness 报告 [`.tmp/qa-table-ambient-20260910/render-qa-report.txt`](../../.tmp/qa-table-ambient-20260910/render-qa-report.txt) 为 `fc3c7d5`、`WorkingTreeClean: True`、`render-qa OK`，真实宿主截图和 DPI 仍是验收边界。

## 2026-09-10 生产壳层整页游戏背景与边界缝

- 图二右侧页面/侧栏后方的长方形确实来自选中游戏背景 `ImageBrush`。现有图片层和 `ShellAmbientMaterialLayer` 均为 `Grid.Row=0`、跨两列、跨两行，背景已经是整壳层层级；问题在于外壳 `Margin=4`、侧栏右 `Margin=6`、footer 独立描边/边距形成了视觉缝。
- `AcrylicProductionShellView.xaml` 现在移除这些外层缝隙和独立描边：`DemoShell` 零边距/零边框，`SidebarSurface` 零边距并保留左圆角，`FooterSurface` 跨完整宽度且无独立边框。背景仍被半透明 sidebar/page 表面覆盖，避免原图干扰文字。
- `ProductionShellChromeSourceTests` 增加整壳层背景契约；RenderHarness 双主题及 1040/1100/1366 Shell Media 几何回归通过。离屏没有真实 Playnite 图片资源，不能替代宿主截图。

## 2026-09-10 游戏选择器视觉树运行时回归补强

- 新增 `WpfUiResourceDictionaryTests.GameContextButtonKeepsCompositeGridContentThroughItsRuntimeTemplate`，在 STA WPF 线程中解析生产资源并实际套用 `GscRedesignGameContextButton`，测量/排列后验证 `ContentTemplate={x:Null}`、`Chrome` 模板内的 `ContentPresenter` 仍持有原始 `Grid`，从运行时层面覆盖 `System.Windows.Controls.Grid` 字符串化风险。
- 定向测试 `1/1`；完整 Release 回归为 Core `76/76`、Worker `310/311`（1 skip）、Playnite `428/491`（63 skip），失败 `0`。这是测试覆盖增强，未改变生产程序集，`248d28e` 候选包仍为当前生产候选。

## 2026-09-10 当前生产源候选包与发布边界

- 当前候选包从 HEAD `8657654` 重新生成；六份程序集统一为 `0.6.73+8657654310d99e349c120f3bb0484b65ac9f4dc2`，两个候选包均为 `43,837,912` 字节，SHA-256 为 `E136B5C6465A5A8933C72B0F9707CFF64D91A02D35115B9A007BF6A749B62AB0`。
- 发布链验证为 Core `76/76`、Worker `310/311`（1 skip）、Playnite `428/491`（63 skip），构建无警告/错误；生产视觉修复来自 `248d28e`，`8657654` 只增加游戏选择器运行时模板测试，候选包仍包含首页活动行、云端整卡、顶部复合按钮和活动游戏选择器模板防护。
- staging/隔离构建目录已清理；包仍未安装真实 Playnite，不能把最新包身份校验写成 FusionX、DPI、物理点击或视频验收。
- 包后又执行 `python scripts/validate-source.py`、`scripts/check-xaml.ps1` 和 `git diff --check`，均通过；这些是源码/交付门禁，不替代 L31 真实宿主证据。
- 2026-09-10 在当前 `main` 重新执行 `dotnet test GameSaveCenter.sln -c Release --no-restore -m:1`，Core `76/76`、Worker `310/311`（1 skip）、Playnite `428/491`（63 skip），失败 `0`；新增运行时模板测试确认游戏选择器复合 `Grid` 没有被文本模板转换为类型名；此前 `AsyncThumbnailLoader` 的 `120`/`122` 并发污染本次未再复现。
- 最新 `render-qa-head-clean-20260910/render-qa-report.txt` 的报告提交元数据为 `5c53e59`，`WorkingTreeClean: True`、`render-qa OK`；`5c53e59` 只改文档，生产 XAML 与 `8657654` 候选包一致；覆盖双主题、多尺寸、resize、云端筛选前景、完整壳层背景和 Media 页尾几何，仍属于离屏证据。
- L32 文档链接审计共检查 123 个本地 Markdown 链接，缺失 0；旧临时截图不恢复，工作日志改指向当前证据索引。

## 2026-09-09 首页活动和云端队列整卡交互验证

- `4001d9d` 增加真实 WPF STA 行为测试，不再只依靠 XAML 字符串断言：`OverviewView` 完成测量/排列后，活动按钮内容保持 `Border`，云端卡片内容保持 `StackPanel`，共享按钮的文本模板不会把视觉树显示成类型名。
- 测试通过反射调用 WPF `ButtonBase.OnClick` 的框架点击路径，确认 `OpenCloudQueueCommand` 恰好执行一次；因此“查看明细”被移除后整卡入口仍有行为覆盖。
- 本地验证为 Release 构建 `0 warning / 0 error`、Playnite `427/490`（63 skip、0 fail）。测试夹具不改变生产包；真实 Playnite/FusionX 主题、DPI、鼠标点击和录屏仍不可由该测试代替。

## 2026-09-09 首页活动行可视树与云端队列整卡导航

- 共享 `GscWpfUiButton` 默认通过 `GscWpfUiButtonTextTemplate` 将 `Content` 绑定到 `TextBlock.Text`；当首页全局活动行把 `Border/Grid` 作为按钮内容时，会出现 `System.Windows.Controls.Border`。页面局部的 `OverviewActivityRowButton` 必须设置 `ContentTemplate={x:Null}`，不能修改为关闭全局按钮模板或删除活动命令。
- 云端队列指标采用 `OverviewCloudQueueCardButton` 作为唯一交互面，直接绑定 `OpenCloudQueueCommand`，不再在卡片内部嵌套“查看明细”按钮。保留 `AutomationProperties.Name`、焦点样式、悬停动效和提示。
- 这轮离线证据为 Playnite `426/489`（63 skip、0 fail）、Release `0 warning/0 error`、RenderHarness `render-qa OK`、XAML `19/19`、WPF 静态审计 `0 errors / 22 warnings / 157 info`；真实 Playnite/FusionX、用户主题/DPI 和人工点击仍须宿主验收。

## 2026-09-09 Media 离屏视口门禁按实际行几何收口

- `d777e65` 修正了离屏 RenderHarness 的 Media 误报：DataGrid 不再用固定 `236 DIP` 作为主表通过条件，而是统计行容器相对 DataGrid 边界的完整可见数量；有至少 4 条数据时要求 `4/4` 行完整可读。`230 DIP` 的独立 Media 表格因此得到 `readableRows=4/4`，没有修改生产表格高度或关闭虚拟化。
- `MediaClassificationPreviewItems`/`MediaClassificationHistoryList` 属于媒体 Inspector 内的小型预览/历史列表，分别只有 2/3 条夹具内容，并受 Inspector 自己的滚动容器管理；它们不适用主工作区四行门禁，但报告仍记录尺寸和条目数。
- Resize 探针改为向 Media 传入 `ContentSize` 的 `contentH`，与生产 PageHost 测量一致；原先 `86 DIP` 的错误结果恢复为 `300 DIP`、`6/4` 完整可读行。提交后的报告为 [`.tmp/render-qa-media-gate-clean-20260909/render-qa-report.txt`](../../.tmp/render-qa-media-gate-clean-20260909/render-qa-report.txt)，`WorkingTreeClean: True`、`render-qa OK`。
- 这只完成了离屏质量门禁，不证明 Playnite/FusionX 真实模板链、DPI、用户主题或视频式拖动问题已解决；宿主证据边界保持不变。

## 2026-09-09 当前回归基线与宿主边界同步

- 当前 `main` 交接基线为 `580a70f`。此前缩略图并发测试的 `120/122` 失败已由 `c0197e5` 通过测试集合隔离修复；Playnite 全量为 `425/488`（63 skip、0 fail）。
- `580a70f` 只同步回归证据与离屏审计边界，没有继续修改生产表格模板、滚动单位或虚拟化。真实 Playnite/FusionX 仍没有可绑定窗口，不能把 `render-qa OK` 写成视频问题已解决。

## 2026-09-09 回归失败与离屏审计夹具修正

- `AsyncThumbnailLoader` 的诊断、缓存和并发闸门是进程级静态状态；`AsyncThumbnailLoaderTests` 在 `ResetDiagnostics()` 后若与 `AsyncThumbnailImageTests` 并行，观察到的 `RequestCount=122` 不代表生产代码多发请求。`c0197e5` 将两个测试类放进同一个禁并行集合，保留原有 `120` 精确断言和生产 3 路/96 项边界；定向 `1/1`，Playnite 全量 `425/488`（63 skip、0 fail）。
- `559d64f` 修正 RenderHarness 的三类误报来源：Settings 使用可访问的临时目录、Sidebar 完成探针改为不会被 Render 优先级 tick 饿死、Media 主题响应式探针传入实际 PageHost 高度。干净报告为 [`.tmp/render-qa-harness-clean-20260909/render-qa-report.txt`](../../.tmp/render-qa-harness-clean-20260909/render-qa-report.txt)，构建 `0 warning/0 error`。
- 修正后 Settings normal/dirty/invalid 和 Sidebar rapid-toggle 通过；生产壳层 Media 1040/1100 表格为 `300 DIP`，页尾 footer/history/secondary 可达。render-qa 仍有嵌套归类预览 `126 DIP`、独立 Media 表格 `230 DIP` 和 resize `86 DIP`，这些离屏组合尚未证明生产问题，也不能被直接删除门禁；真实宿主/FusionX 与视频式操作仍待验收。

## 2026-09-09 浅色主题背景与云端队列文字对比度修复

- 生产壳层同时存在全壳层选中游戏 `ImageBrush` 与内容列 `UseSelectedGameBackground=True` 的 `AmbientMaterialLayer`；后者会形成截图中侧栏右边及最右侧的矩形图片边界。修复只保留全壳层图片，Shell ambient wash 跨两列且 `UseSelectedGameBackground=False`，页面局部材质仍可继续提供页面光晕。
- `MaintenanceView.xaml`/`DashboardView.xaml` 的 `GscComboBoxLongText` 显式设置 `Foreground` 和 `TextElement.Foreground` 为 `GscPrimaryTextBrush`。这是对 `BaseTextBlockStyle` 覆盖 ComboBox 模板文字绑定的收口，不改变 ItemsSource、SelectedItem、Popup、选择或截断语义。
- 验证边界：定向契约测试 `2/2`，源校验和 XAML `19/19` 通过，WPF 静态审计 `0 errors`；RenderHarness 构建成功但完整 render-qa 仍报告既有失败。没有真实 Playnite/FusionX 窗口，因此不得把离屏图片或静态契约写成宿主视觉通过。

## 2026-09-09 L42/L41 真实宿主启动边界

- L42 运行基线为 `398a6f0`。L41 受限启动的 `cef.log` 记录 CEF Mojo channel `拒绝访问 (0x5)`，Playnite 随后退出；L42 在提升权限下完成 Core `76/76`、Worker `311/311`、Playnite `431/488`（57 skip），但 Playnite 进程保持 `MainWindowHandle=0`，UI Automation 无法找到侧栏，最终没有 `summary.json` 或表格 replay。
- L42 只证明当前宿主入口没有形成可绑定的真实窗口，不是视频表格异常的复现，也不是修复通过证据。由审计启动的 Playnite 进程已停止，用户 FusionX、全局 Playnite 样式和用户数据目录未修改。
- 当前真实嵌入表格证据仍是 L39：实际 `ScrollViewer`/`DataGridRowsPresenter`、`CanContentScroll=True`、`ScrollUnit=Item`，两表程序化端点回放的尾项完整、异常计数为 0。物理视频操作和问题 B 仍待宿主人工验收；没有诊断日志前不选择某个模板、偏移或锚点原因。
- 详见 [`L42_REAL_HOST_SCROLL_REPLAY_2026-09-09.md`](L42_REAL_HOST_SCROLL_REPLAY_2026-09-09.md)。

## 2026-09-09 L40 真实宿主捕获未形成新的嵌入滚动证据

- L40 运行代码基线为 `c84107b`，构建身份为 `0.6.73+c84107b2f79870133707ed01abd22c521e87e070`。L40 隔离 Playnite 最终 `EmbeddedDashboardCaptured=false`、`ControlledDashboardCaptured=true`、`ProductionVisualSourceOfTruthAvailable=false`，所以不能覆盖或升级 L39 的真实嵌入证据。
- L40 启动前修正了隔离配置残留的旧 Worker 绝对路径；这只修复审计环境身份问题。L40 没有新的真实嵌入 `scroll-replay` JSON，受控窗口截图不能当作生产表格滚动通过。
- b100913 的 RangeValue 滑块等效路径曾在 WPF provider 内抛出 `NullReferenceException`；c84107b 对开发审计调用做异常隔离并继续执行。该路径不是生产滚动逻辑，也不能替代真实鼠标拖拽。
- 最新回归为 Core `76/76`、Worker `310/311`（1 skip）、Playnite `425/488`（63 skip），构建无警告/错误。L39 的真实嵌入端点结果仍有效：媒体 400 条、任务 50 条、各 47 样本、底部尾项完整、异常计数为 0。
- 真实物理滑块、滚轮/PageUp/PageDown/Ctrl+End、主题/DPI 矩阵、加载更多后的锚点和视频录屏仍必须标记为宿主人工待验收；不能因为程序化端点回放或源码契约测试而写成“已解决”。

## 2026-09-09 L39 真实宿主回放未复现视频 B 类异常

- L39 运行代码基线为 `a9b8bce`；L39 包身份为 `0.6.73+a9b8bcec0c05f7d548d8160119b2b29e9698b1ab`。隔离真实 Playnite/FusionX 嵌入采集成功，Dashboard `1365.33×868 DIP`、DPI `1.5`。
- `MediaInboxGrid` 400 条和 `TaskGrid` 50 条各完成 47 个程序化回放样本，包含顶部/底部、20 次上下端点往返、水平端点和尾项选择。两表底部尾项均完整：Media `offset=394/scrollable=394`、Presenter `0,36,604×279.33`、尾行 `399@256..300`；Task `40/40`、Presenter `0,36,637.33×456`、尾行 `49@432..476`。尾项 cell/visual/text 为 `5/5/5`、`6/6/6`。
- 回放 JSON 的空正文、大间隙、末端水平条覆盖、选中内容缺失和尾项不完整均为 0。该证据证明当前模板端点范围和末行视口避让在真实宿主中通过，但不证明物理滑块拖动期间的 B 类视频问题已解决。
- 任务审计必须保留 50 条页面，不调用加载更多；当前任务分页命令会替换页面并保留 `HasMore=true`，重复调用会造成无限分页回放，不能把它误记为滚动异常。
- CUA 原生应用仍为 `apps: []`，真实鼠标滑块、滚轮/键盘矩阵、DPI/主题矩阵和录屏保持宿主人工待验收。后续若人工复现，先按 `Items.Count/CollectionChanged/代际/滚动器/Presenter DIP/首末行/cell visual-text-clip/锚点` 分流，禁止先改参数。

## 2026-09-09 L36 真实 Playnite 嵌入采集已获得，但视频式人工复测仍待宿主

- 当前最新宿主证据为 [`artifacts/ui-host-audit-isolated-l36/summary.json`](../../artifacts/ui-host-audit-isolated-l36/summary.json)、[`metadata.json`](../../artifacts/ui-host-audit-isolated-l36/metadata.json) 和 [`diagnostics-summary.md`](../../artifacts/ui-host-audit-isolated-l36/diagnostics-summary.md)，提交为 `99bc976473d13a92c12d6992dff11dbf807e42b5`。真实 Playnite 生产扩展嵌入 Dashboard/设置页均已采集；没有使用受控专用窗口作为生产宿主证据。
- 隔离只读数据库为 `games=3`、`media=4645`、`tasks=4551`、`game_tools=4`；MediaInbox 页面只加载 `200` 条分页数据。宿主 DPI `1.5`、Dashboard `1365.33×868 DIP`、`FollowPlaynite`。当前最新包 `.pext/.zip` 均为 `43,831,773` 字节，SHA-256 为 `099BDC1B69E1BF44E25B7536A15B03342385C116857AB9CE1DBAE202A5B15C06`。
- 真实日志确认任务表/媒体表行滚动器为 `ScrollViewer`，其 `IScrollInfo` 链包含 `ScrollContentPresenter|DataGridRowsPresenter`，`CanContentScroll=True`、`ScrollUnit=Item`；水平条出现时 Presenter 的实际可见矩形已缩小，当前证据支持现有插件局部模板的“表头 Auto / 行内容 * / 水平条 Auto”方向，不支持继续向页面 Margin 或固定底部补偿扩散。
- 重要诊断边界：真实宿主在初始尺寸/滚动过渡帧会出现 `visual>0,text=0`，约 32ms 后恢复为 `text=visual`；该帧没有 `blank/gap/hOverlap`，没有行漂移记录。本次没有持续 `blank=True` 或 `gap=True`，因此不能把视频根因提前归结为锚点恢复、集合 Reset 或单位错误。
- `capture-manifest.json` 对媒体 Inspector/媒体列表滚动面给出 `CapturedAndValidated`，但这不等价于已人工拖到 `MediaInboxGrid` 已加载末尾并确认最后行完整。由于 CUA 当前无可识别原生窗口，必须保留“真实宿主人工 20 次拖动/键盘矩阵/录屏待验收”。后续若人工复现持续空白，优先对照本摘要的 `items/offset/viewport/extent/presenter/rows/text/clip/anchorGen`，按集合是否 Reset、偏移是否越界、容器几何是否错误、单元格是否仅视觉缺失分流。

## 2026-09-09 L36 之前的真实宿主阻塞记录（历史）

## 2026-09-09 L30 候选安装包与升级/回退

- 候选公共版本固定为 `0.6.73`。`scripts/package.ps1` 必须从当前源码生成插件、Worker、Core、Contracts 的同源构建身份；最新身份为 `0.6.73+1fdd15eedfa64bb34292b85cb0e4d14bbfa9dd81`。Worker 发布必须是 `win-x64` self-contained，并验证 `runtimeconfig` 的 `includedFrameworks`。
- 包内最低必需集合包括 manifest、icon、插件 DLL、Contracts/Core、Worker EXE/DLL、Worker runtimeconfig、hostfxr/hostpolicy/coreclr；`.pext` 与 zip 应保持同字节内容。最新候选两个包均为 `43,837,799` 字节，SHA-256 为 `FD91FB0E0B12ABA2A73F29F76F1E3F90255D6FA4D30798EBA2FEFB53FAD120F9`。
- 数据库迁移采用幂等增量列/表初始化，必要时在事务内重建 `backup_versions` 并保留数据；没有通用 down-migration。升级前必须复制完整隔离配置/状态库，回退先停宿主、保留失败副本、恢复升级前副本再安装旧包，不能让旧版直接打开未知新 schema。
- L30 只验证候选包与隔离迁移，不等价真实 Playnite 安装；包未安装，宿主加载/FusionX/DPI/用户主题/视频继续由 L31 验收。

## 2026-09-09 L31 真实宿主矩阵阻塞

- `scripts/real-host-audit.ps1` 会停止现有 Playnite并通过 `dev-install-run.ps1` 重装/启动开发扩展；本轮没有执行该有副作用流程。
- PowerShell 只读发现 Playnite 位于 `D:\software\Playnite\Playnite.DesktopApp.exe`，但 Windows Computer Use 返回空应用清单，不能绑定窗口，因此没有真实滚动、DPI、键盘、FusionX 或录屏证据。
- 本轮尝试启动既有审计流程时被安全门禁拒绝，因为流程会替换用户扩展目录；未绕过门禁，也未写入 Playnite 用户目录或启动宿主。继续执行需要用户明确授权该替换范围。
- 后续不得把 RenderHarness、源码检查或包内验证当作真实宿主通过；恢复条件和矩阵见 `docs/ai/L31_REAL_HOST_BLOCKER_2026-09-09.md`。

## 2026-09-09 表格滚动诊断补强与离线复现

- `DataGridScrollDiagnostics` 选择实际拥有 `DataGridRowsPresenter` 的内部滚动器，避免宿主模板出现多个 `ScrollViewer` 时把外层页面滚动器误当作表格滚动器；日志仍只记录稳定 ID、数量、尺寸、偏移、代际和状态，不记录文件内容。
- `MediaCenterView` 的锚点记录不改变恢复语义，只增加 `queued/executing/completed/skipped/retry/failed` 状态与原因；`MediaWindowAnchorContractTests` 锁定这些诊断出口。
- 提交 `862742a` 新增隐藏 WPF `Window` 的同数据对照：插件模板从顶部执行 `ScrollIntoView(最后一项)` 后偏移为 `1992/1992`，最后行完整；随后 20 次往返及 `PageDown/PageUp/Ctrl+End` 保持可见和选择。标准 WPF 模板的滑块/Ctrl+End 末行完整，但 deferred `ScrollIntoView` 在该离线夹具仍为 `offscreen-baseline-inconclusive`，不能拿来推断 FusionX。
- 提交 `85b1aeb` 只读加载本机 FusionX `2.1.1` 的 `DefaultControls/DataGrid.xaml` 做同窗体对照；没有写入用户主题。FusionX 直接滑块/Ctrl+End 到 `1987/1987`，末行 `1999@608/44` 完整，Presenter `0,36,1078.67x600`，水平条 `Collapsed/0`；deferred `ScrollIntoView` 仍是不确定基线。该结果不能替代真实 Playnite 内的 FusionX 模板链、DPI 或视频操作。
- 提交 `5198c6c` 增加 FusionX 水平条显示场景：700×640 DIP 视口、1100 DIP 列宽时水平条为 `Visible/17.33`，Presenter 为 `678.67x582.67`，末尾仍到 `1987/1987` 且最后行 `1999@608/44` 完整；20 次往返和语义滚动没有空正文或末行裁剪。该结果仍是隐藏窗口离线夹具，不是宿主录屏。
- 提交 `8775809` 修正 RenderHarness 对空 `git status --porcelain` 的解释；最新 `.tmp/l32-scrollprobe/scaleprobe-report.txt`（canonical 提交 `ea18b11`）记录 `WorkingTreeClean: True`，不再把干净工作树写成 `False`。
- 提交 `f31711c` 让 `CaptureAnchor`/`RestoreAnchor` 使用与诊断器一致的实际表格 `ScrollViewer` 选择规则：优先含 `DataGridRowsPresenter`，再按 DIP 视口高度/宽度排序；提交 `1477a37` 用 STA 隐藏 Window 行为测试锁定多滚动器选择。定向锚点契约为 `10/10`；最新 `1fdd15e` 候选包已包含生产修复、浅色主题修复和首页修复，但仍不能把离线报告写成真实宿主已验证。
- 离线 `scaleprobe` 的 20 次滑块往返在任务表/媒体表 200、2000、10000 条规模均保持可见行和文字；末尾滑块路径的最后行完整。真实 Playnite/FusionX、DPI、视频和加载更多现场锚点仍必须复测。
- 本轮没有真实宿主窗口；后续仍须用 Playnite/FusionX 重复视频动作，不能用该离线报告替代宿主验收。锚点修复和 STA 行为测试后的 Playnite 全量离线回归为 `423/486`（63 skip、0 fail），锚点定向 `10/10`。

## 2026-09-09 L28 持续更新分页与选择恢复

- `CloudTransferStateService.GetStatusAsync` 和 `MediaSyncService.GetClassificationHistoryAsync` 的 revision/一致性令牌是 offset 分页的正确性边界：请求期间或请求前令牌变化必须返回 `PageResetRequired`，不能继续拼接旧页。当前触发器覆盖云端队列/重试队列、游戏/媒体云状态以及归类批次/批次项的增删改。
- `DashboardViewModel.CloudTransfers` 和 `DashboardViewModel.MediaClassification` 在分页重置前捕获稳定选择 ID。重置后使用第一页结果恢复；首屏没有该 ID 且 `HasMore` 时，设置 pending ID 继续下一页，恢复后清理 pending；到达末页仍未找到则按对象删除/不再可见处理并清理选择。
- `allowConsistencyRetry` 只允许一次从第一页自动重试。第二次仍然 `PageResetRequired` 时必须禁止 pending selection 的后续自动翻页；保留 pending ID，设置 `*NeedsManualRefresh`，摘要和 `StatusMessage` 引导用户点击现有刷新命令。手动刷新会重新开启一次有界自动重试。
- L28 Worker 回归覆盖新增记录已有场景，以及云端传输状态更新、归类批次状态更新；Playnite 源契约覆盖选择保留、后页恢复和重复重置停止。不要把离线源契约当成真实宿主行为证据。

## 2026-09-09 L29 全量回归与 skip 账本

- 当前全量计数必须按项目和原因拆开记录：Core `76/76`；Worker `310/311`，1 项 `[WorkerProcessFact]` 跳过；Playnite `420/483`，57 项 `[LegacyProductionUiBaselineFact]` 旧架构断言 + 6 项 `[NamedPipeFact]` IPC 行为测试。跳过不计入通过，详细文件/数量/替代证据在 `docs/ai/SKIP_LEDGER_2026-09-09.md`。
- `LegacyProductionUiBaselineFactAttribute` 不是当前功能失败，而是旧“今日工作台”布局断言与现行 AcrylicFork/Demo-first 生产结构不一致；恢复它们前必须按当前页面契约重写。Named Pipe 和 Worker 进程测试则是环境前置条件缺失，不能用源字符串或普通单元测试宣称行为已通过。
- `scripts/e01-behavior-matrix.ps1 -SkipBuild` 必须使用既有默认 Release 输出；只有实际构建隔离输出时才传 `GscBuildOutputRoot`。否则会出现空日志/`0/0` 且退出码为 0，污染回归证据。修复后 E01 为 `144/151` 通过、`7/151` 跳过，并明确保留真实 Playnite `MANUAL QA REQUIRED`。
- L29 的回归只证明当前代码在可用离线/STA 条件下通过；真实 Playnite、FusionX、DPI、Named Pipe 进程间时序、Worker 重启和视频操作仍是宿主验收项。

## 2026-09-09 L27 配置、路径与外部文件变化

- `GameSaveCenterSettings.VerifySettings` 对存档目录、媒体目录和启用的本地镜像执行只读路径形状/目标类型检查：缺失的叶目录只要所在驱动器或共享可达就保留为可创建状态；指向文件、无效路径、磁盘/共享不可达或 ACL 访问异常会关联到具体目录字段。不要在文本框校验中调用 `Directory.CreateDirectory` 或写探针。
- `MediaSyncService` 的内置系统来源仍允许缺失；用户在媒体来源规则中配置的目录必须先通过一次可枚举性确认，并在扫描期间异常时抛出 `MEDIA_SOURCE_UNAVAILABLE`。单个媒体文件在稳定性/哈希/复制阶段遇到占用或访问异常时抛出 `MEDIA_FILE_UNAVAILABLE`，保持源文件不删除。`MEDIA_SOURCE_UNAVAILABLE`、`MEDIA_FILE_UNAVAILABLE` 的 diagnostic detail 只带路径/异常摘要，不写入媒体内容。
- `SavePathDetectionService` 将 `DetectionRequestDto.AdditionalRoots` 置于默认根之前并标为 required：根目录消失、无法枚举或子目录/文件枚举被拒时抛出 `SAVE_PATH_ROOT_UNAVAILABLE`；用户未明确传入的系统根仍按可选扫描处理，避免普通用户目录权限差导致整个探测失败。
- L27 证据：设置/便携导入定向 `11/11`；媒体与存档路径定向 `16/16`，包含缺失配置来源失败、附加根失败、Unicode/长文件名归档、占用文件失败且源文件仍在。全量 Core `76/76`、Worker `308/309`（1 项隔离 Worker 重启测试在沙箱跳过）、Playnite `419/482`（63 项 UI/宿主条件跳过）；Release `0 warning/0 error`，`validate-source.py`、XAML `19/19`、`git diff --check` 通过。
- 边界：本轮未改用户 FusionX/全局主题，也未关闭任何虚拟化；没有真实 Playnite 保存失败、网络共享 ACL、外置盘断开后的宿主页面、DPI 或视频录屏证据，不能把离线路径夹具写成真实宿主已验收。

## 2026-09-09 L26 Worker 启动、断连与恢复边界

- `WorkerLauncher.EnsureStartedAsync` 的生命周期边界：健康握手先分协议/版本/构建身份；同路径已有进程的 transient probe 最多宽限 45 秒，启动新 Worker 的真实就绪截止 30 秒；不能因一次短 Ping 超时杀掉大库 Worker。`StopOwnedWorker` 只回收本插件保存的 `runningWorker`，Playnite 退出不扫描/停止其他实例。
- Worker SQLite 初始化先 `RecoverIpcRequestLedgerAsync`，把上一个进程遗留的 `InProgress` 写请求变为 `Interrupted`，客户端收到后只能核对状态，不能把旧请求当成可安全重放。任务协调的硬重启恢复会将未完成 durable task 标为 `WORKER_RESTARTED_RETRYABLE`。
- 构建身份规则：已知 actual/expected 两个身份不一致才是 `BuildIdentityIncompatible`；unknown/空身份在协议/版本兼容时可继续工作，但日志必须明确“构建身份未验证”，不能冒充同源。旧 Worker 没有 handshake 时按受限 legacy Ping 兼容。
- L26 证据：独立临时 Worker 硬停止/重启测试 `1/1`；完整 Worker `305/305`、0 skip；Playnite 完整 `423/480`、57 个宿主条件 skip。真实 Playnite UI 的启动失败、运行中断连、恢复刷新和多实例安装仍需宿主矩阵。

## 2026-09-09 L25 IPC 取消、超时与同 ID 复核

- `WorkerIpcClient` 的破坏性请求在超时/断管后只能用原 `IpcEnvelope.RequestId` 复核；不能生成新 ID 直接重发。Worker `ipc_request_ledger` 按请求类型、协议版本和 canonical payload fingerprint 冲突保护，Completed 回放原响应，InProgress 返回“仍在执行”，Interrupted 返回“此前 Worker 已退出，先核对状态”。
- 复核阶段必须继续使用调用方 Token：连接、读取和 `REQUEST_IN_PROGRESS` 间隔等待都可被调用方取消；但因为原写请求可能已送达，取消异常必须保留 `MayHaveBeenAccepted=true`。宿主退出优先报告 `WorkerIpcCancellationReason.HostShutdown`，只读请求响应读取取消保持 `false`。
- 既有 VM 请求作用域/代际保护仍是 UI 提交门，不要在旧 IPC 响应到达时弹过期错误或恢复旧选择。写请求回执丢失是“结果未知”，不应自动显示成功，也不应把普通取消当服务器拒绝。
- L25 证据：本地 Named Pipe 行为测试 `6/6`，Worker 账本测试 `6/6`；测试矩阵包括连接前、只读读响应、宿主关闭、写回执丢失、复核阶段取消和写入中取消。真实 Playnite Worker 启停、页面关闭和录屏仍是宿主验收项。

## 2026-09-09 L24 隔离账本分页与动效采样

- 隔离账本的 UI 查询入口现在是分页契约，不再把所有未删除行一次性跨 IPC 传到维护页：`RetentionQuarantinePageRequestDto` 限制 `Offset/Limit`，Worker 在 SQLite 侧先给 durable `TotalCount`，再按 `updated_utc DESC, entry_id DESC` 返回最多 100 条并给 `HasMore`。现有 Worker 内部恢复/预览仍可使用完整持久化读取，不把 UI 分页误用于安全协调。
- `DashboardViewModel` 首次只装载一页；`LoadMoreRetentionQuarantineCommand` 追加下一页并按 `EntryId` 去重，动作请求仍以稳定 EntryId 为准。每次刷新或恢复后从第一页重建，摘要区分“隔离账本总量”和“已加载/全部”，按钮状态必须进入 `RaiseCommandStatesCore`，否则新页到达后可能保持不可用。
- 维护溢出 `ListBox` 必须保留有限视口（当前 `Tag=FiniteViewport`、`MaxHeight=360`）及 `CanContentScroll=True`、`VirtualizingPanel.IsVirtualizing=True`、`VirtualizationMode=Recycling`；不能用扩大页面、隐藏滚动条或关闭虚拟化掩盖列表规模。`scripts/validate-source.py` 对此有限视口有显式门禁。
- L24 证据：205 条夹具返回 `100/100/5` 三页、durable total `205`、稳定 ID 去重 `205`；shellqa 的单次/快速/关闭动画布局计数为 `46/58/3`，最大帧间隔为 `53.2/31.1/17.1ms`，均 settled。离屏报告不代表 FusionX、真实 DPI 或真实帧率；新增/删除/状态变化仍须在真实宿主通过刷新/恢复操作复测。

## 2026-09-09 L23 搜索、刷新与重复 IPC 合并

- `DashboardViewModel` 的任务历史分页与 Dashboard 快照读取必须遵循最新请求提交规则：开始请求时创建稳定 generation/Token，IPC 返回后和 `ApplyOnUi` 回写前再次确认当前作用域；旧响应只允许结束自己的取消/清理，不得清空新请求状态、覆盖新筛选或写入旧页面。`LatestRequestCoordinator.RequestScope.Token` 在取消后仍可安全读取，不能让释放 CTS 反过来制造 `ObjectDisposedException`。
- 任务搜索/状态/游戏/类型变化先失效当前分页请求，再通过 debounce 合并；历史范围/时间范围和清除筛选是立即查询。debounce 回调必须投递到 UI，若 `IsBusy` 则保留 `taskHistoryQueryQueued`，在 `RunAsync` 释放忙状态后启动最新一次；不可恢复为“直接 `Run`、忙时静默丢失”。
- Dashboard 刷新代际要覆盖同步、`GetDashboard` IPC、快照应用和后续任务/媒体/维护加载；`CancelDeferredUiWork` 同时取消任务页与快照协调器。后台只读刷新不改变写请求的取消/重试语义，也不能将不同筛选条件复用成同一结果。日志只输出请求代际、请求号、数量/尺寸和 hasMore 等诊断字段，不输出搜索文本或文件内容。
- L23 证据：debounce 的连续 `a→ab→abc` 夹具为 `1` 次回调；协调器取消、替换、CTS 释放后 Token 和 `20` 次快速替换测试通过。独立 Playnite 测试 `417` 通过、`62` 跳过、`0` 失败；Core `76/76`、Worker `303/304`（1 跳过），Release 构建无警告/错误。真实 Playnite/FusionX、DPI、视频和真实 IPC 日志仍待宿主验收，不能把离线契约测试写成视频问题已解决。

## 2026-09-09 L22 缩略图加载、取消与缓存边界

- `AsyncThumbnailImage` 的加载入口必须同时满足 `IsLoaded`、`IsVisible` 和非空路径；不可见/卸载先推进 generation、取消并释放旧 `CancellationTokenSource`，同时清空 `Source`。路径或预览宽度变化不能让旧任务回写新卡片。
- `AsyncThumbnailLoader` 的稳定边界是 `BitmapCacheOption.OnLoad`、冻结 `BitmapSource`、最大 3 路解码和最多 96 项 LRU；文件元数据、缓存检查和解码均不在调用方 UI 线程执行。宽度先夹到 `48..480` 后参与缓存键，破损/缺失/权限等预期读取错误返回空值，取消继续向上抛出。
- L22 探针只记录 ID/尺寸/计数：120 项初次窗口 `120` 请求、`120` 成功、峰值并发 `3`、缓存 `96/96`；16 项保留窗口全命中；破损与缺失各返回空；预取消可观测；100 次 12 项窗口往返结束时活动解码为 `0`，实际缓存仍为 `96/96`；旧 800×800 路径替换为新 64×64 路径后最终像素标记为新图且输出宽度 `96`。
- `tests/GameSaveCenter.Playnite.Tests` 的 WPF 控件回归与 `tests/GameSaveCenter.RenderHarness thumbnailprobe` 只证明合成 STA/文件夹夹具；不把它们写成真实 Playnite/FusionX、DPI、用户目录锁定或视频回放验收。临时报告只保留最新 `.tmp/l22-thumbnailprobe-final`，不进 Git。

## 2026-09-09 L21 列表虚拟化与滚动规模实测

- 当前游戏媒体卡片列表必须使用项目已有的 `VirtualizingWrapPanel`；普通 `WrapPanel` 即使配了 `VirtualizingPanel.IsVirtualizing=True` 也会在 200/2000/10000 夹具中生成 200/2000/2000 个卡片，不能作为大库实现。当前 XAML 的 `MediaGrid` 保留 `ItemWidth=164`、`ItemHeight=154`、水平/垂直间距 0，选择和 ListBox 滚动契约不变。
- L21 离屏规模证据：当前媒体窗口上限 2000；200/2000/10000 后端场景顶部、底部、回顶部均约 20 个卡片容器，200 条往返的 extent/viewport/最大偏移为 `6160/345.33/5814.67`。任务 DataGrid 200/2000/10000 保持个位数行容器，收件箱 10000 后端仍保留 2000 条 UI 窗口；选择 ID、末项稳定 ID 和 resize 后容器均受探针检查。
- `DataGridScrollDiagnostics` 必须继续被动记录真实内部 `ScrollViewer`/`ScrollContentPresenter`、单位、offset/viewport/extent、首末行 ID/Y/height、行/单元格内容、选择和水平条，不以 `DataGrid.ActualHeight` 代替内部证据。离屏 `ScrollIntoView` 在插件模板和标准 WPF 模板中同样 deferred，不能写成 FusionX 已定位或已修复。
- 当前阶段没有修改宿主 FusionX、全局样式或关闭 DataGrid 虚拟化；无真实 Playnite/FusionX、DPI 和视频回放，相关验收保持宿主待验收。临时探针证据只保留最新目录，不纳入 Git。

## 2026-09-09 L20 修改器导入与下载结果反馈

- `DashboardViewModel` 的工具导入必须在检测开始时保存目标游戏 ID/名称；多候选 `ImportEntryCandidates` 的确认不能在返回后重新从 `SelectedGame` 推导目标。游戏切换会清理待确认项并要求重新导入；导入请求完成后只有同一游戏仍被选中时才刷新当前工具详情。
- FLiNG 下载请求必须捕获 `PlayniteId`、`CatalogId`、`ReleaseId` 后再进行 IPC。页面反馈只消费 Worker 任务事件和终态 `TaskStatusDto`，按状态与 `FLING_DOWNLOAD_FORBIDDEN`、`FLING_DOWNLOAD_INVALID`、`FLING_RELEASE_PARSE_FAILED` 等稳定错误码给出下一步；不要在 VM 重新实现下载、解压、来源校验或启动可执行文件。
- 取消下载复用现有 `MessageTypes.CancelTask`/`TaskCoordinator.Cancel`。Worker 的取消边界、临时文件清理和不自动运行语义是安全事实；页面只能显示“已发送取消请求/已取消”，不能在取消按钮点击时假定文件已删除或绑定已回滚。
- `TrainerCenterView` 的新增状态卡只承载进度、结果和下一步，不能以固定高度、隐藏滚动条、关闭虚拟化或改全局主题来掩盖问题。离屏 RenderHarness 只证明 XAML/夹具状态可渲染，不证明 Playnite/FusionX 模板链、在线 403/离线和真实视频操作。
- L20 验证：Release 构建 `0 warning/0 error`；Core `76/76`、Worker `303/304`（1 跳过）、Playnite `405/467`（62 跳过）；RenderHarness Release 构建、源校验、XAML `19/19`、WPF 静态审查 `0 errors/21 warnings/172 info`、差异检查通过。完整 render-qa 仍有已知 Media 小视口/预览列表与 Sidebar rapid-toggle 失败。

## 2026-09-09 L19 存档版本识别、比较与恢复信息

- `BackupVersionDto` 的来源、系统和恢复检查时间必须使用显式展示字段；空值显示未知/尚未检查，不能从创建时间、文件数量或锁定状态推导“健康”或“可恢复”。`BackupDiffDto` 的大小变化使用带符号的人类可读值，比较质量继续沿用 Worker 的 Exact/Estimated/InvalidManifest 事实。
- `DashboardViewModel.SelectedBackup` 切换到不同稳定 ID 时清掉旧 `LastBackupDiff`；比较响应提交前重新确认当前游戏、选中版本和上一版本 ID 都未变。比较命令的现有语义是“当前选中版本 vs 上一版本”，UI 必须这样表述，不要暗示任意两版本选择。
- 恢复确认前先复制游戏/版本稳定 ID、创建时间、来源、系统、锁定和就绪摘要；确认后 `RestoreExecute` 只能使用这些快照值。不得因为未知、警告或失败状态在 UI 层猜测健康或替代 Worker 安全阻断；PreRestore 快照与撤销请求保持原协议。
- L19 验证：Release 0 警告/错误；Core `76/76`、Worker `303/304`（1 跳过）、Playnite `404/466`（62 跳过）；源校验、XAML `19/19`、WPF 静态审查 `0/21/172`、差异检查通过。RenderHarness 的 Save 截图与多尺寸/双主题探针未报本轮新增问题；全量 render-qa 的 Media 小视口/预览列表和 Sidebar rapid-toggle 仍是已知失败。真实 Playnite/FusionX、DPI、不同恢复就绪状态和录屏必须继续标记为宿主待验收。

## 2026-09-09 L18 批量动作提交前摘要

- 媒体批量命令必须在命令入口捕获 `MediaId`，先去重并统计原始选择、重复项、无稳定 ID 项，再将捕获的 ID 列表交给确认后的 IPC；不得在确认返回后重新读取 `SelectedItems`。目标游戏 ID/名称、归类预览 `BatchId` 和高置信媒体 ID 同样要在确认前保存。
- 只读媒体归类预览不额外弹确认；批量归类、忽略、恢复以及应用归类建议复用现有确认框。Worker 返回后要区分成功、失败、冲突、跳过和未返回，不能用“成功 N 项”覆盖部分结果；已保留的归档副本和安全移动语义不变。
- 任务批量重试只从当前 `TasksView` 结果计算，先按现有游戏/任务类型去重，再排除没有稳定 `TaskId` 的候选；确认后用任务类型、游戏 ID、错误码和显示信息的不可变快照执行。Worker 协议当前按游戏/任务类型重试，不要为了摘要新增任务重试协议。
- L18 验证：Release 0 警告/错误；Core `72/72`、Worker `303/304`（1 跳过）、Playnite `404/466`（62 跳过）；源校验、XAML `19/19`、WPF 静态审查 `0/21/172`、差异检查通过。本轮未改页面布局，不把未运行的真实 Playnite/FusionX 选择变化、部分失败和录屏写成已验收。

## 2026-09-09 L17 常用筛选与工作区状态记忆

- `GameSaveCenterSettings` 的任务查询状态必须一起处理：状态、动态游戏/类型、搜索、历史范围和时间范围。动态游戏/类型不能在构造函数中直接写入 ComboBox 还未拥有的选项；使用 pending 值，等 `TaskFilterOptionsSync` 完成后再恢复。
- 已删除游戏的旧任务记录不能单独证明筛选仍有效。恢复动态游戏筛选时同时检查当前 Playnite `Games`；仍存在但不在最近任务窗口的游戏可补入动态选项，缺失游戏回退“全部”。历史范围只接受现有选项，旧格式/非法值归一化为“最近任务/全部时间”。
- 如果保存的历史范围或时间范围不是默认值，构造 VM 时必须重新激活 `taskHistoryActive`，让启动后的第一次刷新继续走服务端历史分页；不能只恢复 ComboBox 文本而继续展示最近任务快照。
- 媒体“清除”只重置查询条件：取消媒体搜索和分页防抖、失效当前媒体详情代际、刷新现有 `MediaView` 并重新调度受工作区/游戏保护的分页请求；不得把筛选重置绑定到归类、编辑、刷新全库或批量命令。
- L17 验证：持久化/迁移定向 `10/10`，全量 Core `72/72`、Worker `303/304`（1 跳过）、Playnite `403/465`（62 跳过）；Release 构建 0 警告/错误，源码/XAML/WPF/差异检查通过。完整 render-qa 的既有直接 Media 小视口/预览列表和 Sidebar rapid-toggle 失败继续独立记录，真实 Playnite/FusionX 仍待验收。

## 2026-09-09 L16 媒体收件箱操作可达性

- `MediaCenterView` 的收件箱批量栏只做局部压缩：选择摘要宽度 `112`、模式 `104`、目标游戏 `160`，预览入口与批量动作保持同一操作层级；不得为首屏压缩删除真实归类、预览、批次历史或撤销入口。
- 生产壳层在 1040/1100 窗口下给媒体页的 PageHost 约 577/597 DIP。`MediaCenterView.ApplyResponsiveLayout` 在 `<620` DIP 或 Stale 时打开 `MediaInboxPageScrollViewer`，让页面内容承接工具栏/有限 DataGrid/footer 的总高度；DataGrid 仍是 `Tag=FiniteViewport`、Item 滚动、Recycling 和行列虚拟化。不要改成无限测量、关闭虚拟化或用固定底部像素补偿。
- 离屏生产壳层探针的 1040×700/1100×720 网格为 `300 DIP`、顶部间距 `63 DIP`；将页面滚到 `offset==scrollable` 后，`MediaInboxFooter`、`MediaInboxHistoryButton` 和 `MediaInboxSecondaryActions` 均完整处于 PageHost 视口。真实 FusionX 模板和用户录屏仍未验证，不能把该探针称为宿主通过。
- 完整 render-qa 当前仍会报告直接 Media 场景的预览列表/小视口门禁以及偶发侧栏快速切换；这些与生产壳层底部可达性分开记录，后续不要通过降低门禁或扩大 DataGrid 来掩盖。

## 2026-09-09 L15 任务页查错与范围说明

- 顶部任务摘要保留 `RunningTaskCount`、`RetryableTaskCount` 和今日完成，同时新增 `TaskWaitingSummary`（排队 + 等待确认）与 `TaskRetrySummary`（失败 + 已取消）。快照刷新和历史分页完成时必须一起触发这些派生属性的通知。
- `TaskQueueFilterSummary` 只在 `TaskHasActiveFilters` 时显示在队列标题区，避免宽屏用户必须展开“更多筛选”才能知道当前查询；`TaskLoadedSummary` 继续表达已加载窗口、服务端总数、最近/全部历史和时间范围。不要把已加载页数写成全历史已处理数。
- 批量重试按钮的作用域是当前已加载并通过 `TasksView` 的结果，现有 `GetRetryGroupKey` 去重和逐组安全重试保留；文案必须明确这一点。不要因为增加范围说明而改 Worker 协议、游标分页或 `TaskIndexedCollection` 的 200 行窗口。
- L15 验证：任务定向 `36/42`（6 跳过），全量 Core `72/72`、Worker `303/304`（1 跳过）、Playnite `402/464`（62 跳过）；Release 构建无警告/错误，源码/XAML/差异检查通过。离屏 Task 场景在 1040×700 仍有 6 行首屏，双主题/resize 通过；完整 render-qa 的稳定失败属于既有媒体小视口/媒体壳高度，真实 Playnite/FusionX 仍待验收。

## 2026-09-09 L14 运维总览按处理顺序组织

- `MaintenanceActionItem.Group` 只按现有动作语义分为 `NeedsManualHandling`、`WaitingForRetry` 和 `Routine`：隔离账本、认证/失败云端记录需要人工处理，`RetryScheduled` 进入等待重试，恢复巡检进入例行巡检。不要依据显示文案另造分类，也不要把云端摘要计数当成已加载明细。
- `MaintenanceActionSection` 固定保留完整 `Items`，`PreviewItems` 只取前 3 条，`OverflowItems` 显式取其余记录。概览只展示有记录的分组；每次重建都替换 section 列表，保持集合代际和具体 `TransferKey`/`EntryId` 动作参数不变。
- `MaintenanceView.xaml` 的单条动作模板集中保留状态、详情、时间和真实 `RunMaintenanceActionCommand`；溢出使用基于 `GscDisclosureCard` 的展开器。不要改成批量自动修复、定时刷新或关闭虚拟化。长文件名依靠省略提示和详情文本可达。
- L14 验证：分组边界契约覆盖空组、单条、20 条及长标题；Release 构建 0 警告/错误；Core `72/72`、Worker `303/304`（1 跳过）、Playnite `402/464`（62 跳过）；源码/XAML/差异检查通过。离屏维护页通过；完整 render-qa 的稳定失败属于既有媒体小视口/媒体壳高度，首轮另有一次侧栏 rapid-toggle 未稳定，单独 shellqa 重跑已稳定。真实 Playnite/FusionX 仍待验收。

## 2026-09-09 L13 首页优先级与活动上下文

- `OverviewPriorityResolver` 保持单一 Hero 决策，顺序为 Worker 离线、首次准备、云端待处理、媒体待归类、空库、游戏告警、健康刷新。空库使用 `ManagedGames <= 0` 明确显示“还没有可管理的游戏”，不再把无游戏快照当作健康状态；云端失败/认证/校验/重试仍统一来自 `CloudTransferSummaryDto.AttentionCount`。
- `ActivityEntryDto` 新增稳定 `PlayniteId`，由 `ActivityTimelineMapper` 从审计详情提取；`DashboardViewModel.OpenActivityCommand` 按活动类型路由到存档、媒体、工具、云端或维护工作区，能命中已加载游戏时先恢复同一 `SelectedGame`。Overview 活动行使用透明但真实的按钮模板，保留虚拟化、时间和对象显示，并支持键盘焦点。
- 新增空库/云端失败优先级测试，并扩展活动映射测试。Release 构建无警告/错误；Core `72/72`、Worker `303/304`（1 跳过）、Playnite `401/463`（62 跳过）；源码/XAML/差异检查通过。离屏 render-qa 的 Overview 场景通过，媒体小视口/媒体壳表格高度仍是既有失败；没有真实 Playnite/FusionX 交互证据。

## 2026-09-09 L11 通知、长错误与复制详情

- `UiNotificationEventArgs` 的 `Message` 只用于短摘要，`DetailMessage` 保留完整文本。`GameSaveCenterPlugin.RaiseUiNotification` 按成功/信息与错误/警告使用不同摘要上限；任务终态详情额外包含状态、错误码和任务 ID。
- Dashboard 只在 Toast 位置显示摘要；错误或存在独立详情的长消息通过“查看详情”打开共享结果对话框。对话框消息区使用有限 `ScrollViewer`，复制按钮使用详情快照并在异步重试后确认当前对话框仍对应同一文本，避免旧反馈覆盖新反馈。
- 取消任务从 `ShowInfo` 改为 `ShowWarning`，宿主没有 `NotificationType.Warning` 时仍回退到现有 Info 通知；没有新增弹窗式错误流程、没有改任务集合/重试/取消业务语义。
- 新增 `UiFeedbackTests` 与 `NotificationFeedbackSourceTests`。Release 构建 0 警告/错误；全量 Core `72/72`、Worker `303/304`（1 跳过）、Playnite `398/460`（62 跳过）；源码/XAML/差异检查通过。离屏 `render-qa` 的既有媒体小视口/侧栏快速切换问题继续独立记录，真实宿主通知回退、DPI 和录屏未完成。

## 2026-09-09 L12 详情展开与选中上下文

- 收件箱紧凑 Inspector 的展开状态不再跨对象泄漏：`OnMediaInboxSelectionChanged` 清除 `mediaInboxInspectorOpen`，`OnMediaInboxModeSelectionChanged` 还会清除 `mediaInboxHistoryOpen`；切换模式/换选后需要显式打开当前对象。其他任务、当前媒体、存档历史/候选和维护诊断/进程/设备/云端路径保留各自已有的选择即关闭旧详情语义。
- `MaintenanceView` 的 `CloudTransferGrid` 已移除构造函数的重复 `SelectionChanged` 订阅，XAML handler 作为唯一路由；不能再把一次选择触发两次 `ApplyResponsiveLayout` 当成正常行为。
- `DetailsDisclosureSourceTests` 守护四类 CenterView 的选择清理和云端事件单路由。没有新增通用状态框架、定时器、强制布局或业务命令；真实宿主对象删除、快速换选、窄/宽来回和 FusionX 视觉/键盘轨迹仍待验收。

## 2026-09-08 L10 设置修改、错误定位与取消体验

- `SettingsValidationSummary` 旁新增 `SettingsValidationLocateButton`。`FindValidationCategoryIndex` 根据 `VerifySettings` 的现有错误文本把压缩/保留量送到备份分类，毛玻璃送到外观，进程/刷新/巡检/通知送到自动化，Worker/Ludusavi/Rclone/镜像送到常规；点击后只改变 `SettingsSectionTabs.SelectedIndex` 并聚焦分类导航。
- `GameSaveCenterSettingsView` 在构造时监听 `TextBox.TextChanged`、`ComboBox.SelectionChanged`、`CheckBox.Click` 以及 `ToggleButton.Checked/Unchecked`；`OnVisualSettingChanged` 和 `OnGlassStrengthChanged` 也调用 `QueueValidationSummaryUpdate`，所以切换 ToggleSwitch 或拖动 Slider 会更新指纹/校验状态。所有更新仍经 Dispatcher 合并，不加入定时器或保存命令。
- 导入流程的 `settingsTransferInProgress` 保护和 `settingsBaselineInitialized` 逻辑保持不变：`DataContext` 重绑不会重置旧保存指纹，导入后的可编辑差异继续显示未保存；`CreateSettingsFingerprint` 忽略安装级 `DeviceId`。取消由现有 `CancelEdit` 恢复克隆并触发 `SettingsReverted`。
- `SettingsValidationSourceTests` 守护定位入口和控件事件；RenderHarness 的设置三态/隐藏分类导航探针实测 `selectedCategory=1`。全量测试为 Playnite `395/457`（62 跳过），构建 0 警告/错误。Playnite 真正的 `SavePluginSettings` 失败提示和宿主取消按钮没有在离屏环境中伪造，仍需宿主验收。

## 2026-09-08 L09 设置首屏与保存状态

- `GameSaveCenterSettingsView.xaml` 保留 `SettingsIntroDescription` 作为兼容命名但默认/响应式布局均设为 `Collapsed`；Hero 副标题改成短的“工具路径 · 存档策略 · 外观与自动化”。重复说明不再占用标题与正文之间的首屏高度，完整解释留在对应分类卡片附近。
- `RefreshValidationSummary` 的错误分支必须调用 `RefreshSaveState(errors.Count == 0)`。旧代码传入 `errors.Count != 0`，会在摘要显示校验错误时把保存胶囊误写成“已保存”；这个布尔语义由三态夹具守护。
- RenderHarness 的 `RunSettingsLayoutProbes` 检查重复说明不可见、5 个分类项和正文视口；`RunSettingsStateProbes` 在同一 `1040×700` 画布生成 normal/dirty/invalid 状态，分别核验保存文案、错误摘要可见性和截图。
- L09 全量 Release 构建无警告/错误；Core `72/72`、Worker `303/304`（1 跳过）、Playnite `395/457`（62 跳过）。完整 `render-qa` 的已有媒体小视口/侧栏快速切换门禁仍独立记录，不能归因于设置页；真实 Playnite/FusionX、DPI 和 Playnite 保存/取消仍待验收。

## 2026-09-08 L08 可重复诊断与性能采样入口

- `RenderHarness` 的 `render-qa`、`gridprobe` 和 `shellqa` 统一写入 `Scenario`、`EvidenceSource`、Git 提交/工作树状态、离线逻辑 DIP、主题、数据量和时序字段；`RenderTabs`/`RenderView` 将 `layout_ms` 与 `render_ms` 分开，离线报告把 `request_ms` 标为不适用，避免把离屏测量冒充真实宿主性能。
- `scripts/real-host-audit.ps1` 在真实宿主输出旁写 `runner-metadata.json`，包含场景、提交、配置、窗口 DIP、WPF 实际 DPI、主题、生产数据量和采集清单状态。真实宿主元数据仍由插件审计服务捕获，脚本只补充运行器来源，不修改 FusionX 或 Playnite 全局文件。
- `DiagnosticsPackageService` 的 `system.json` 记录场景、证据来源、窗口 DIP、已加载条目数、数据量和 Worker 查询耗时；没有真实布局采样时 `layoutDurationMs` 保持空值。诊断包不增加媒体/文件内容或敏感路径输出，也没有新增业务写入。
- `gridprobe` 重建后报告为 `gridprobe OK`，包含 50/400/2000/4468 数据量声明和滚动语义探针；这只能证明离线夹具的可重复性。真实 Playnite/FusionX 的同尺寸操作、实际 DPI、请求/布局采样和视频式回归仍是宿主验收项。

## 2026-09-08 L07 键盘、焦点与可访问名称

- `MediaCenterView`、`TaskCenterView`、`SaveCenterView`、`MaintenanceView` 和 `TrainerCenterView` 的紧凑 inspector 统一采用“入口打开 → inspector 获焦 → Esc 关闭 → 原入口恢复焦点”的路径；媒体收件箱批次历史单独回到 `MediaInboxHistoryButton`。
- Inspector ScrollViewer 设置 `Focusable="True"` 与 `KeyboardNavigation.IsTabStop="False"`，PreviewKeyDown 只在对应紧凑状态实际打开时处理 Esc；宽屏常驻详情不会因同一事件处理器而隐藏或把焦点送到折叠按钮。
- 点击处理器不写入非必要的 `RoutedEventArgs.Handled`，兼容现有直接反射调用布局测试；键盘事件仍在真实 RoutedEvent 上标记已处理。没有加入计时器、强制 UpdateLayout、滚动重置或宿主级快捷键劫持。
- `KeyboardFocusSourceTests` 的 STA 用例验证实际 `TaskCenterView` inspector 可获焦但不进入 Tab 顺序；源契约覆盖五页 Esc、Keyboard.Focus、焦点入口和自动化名称。真实 Playnite/FusionX 的人工键盘轨迹仍需验收。

## 2026-09-08 L06 目的导航与返回上下文

- `FindingNavigationTargetResolver` 将诊断目标分为精确游戏、任务名称兜底和不可用三类。存档路径导航只接受当前 `Games` 中存在的稳定 ID；目标消失时不切换当前游戏，直接写入可见状态提示。
- 失败任务导航在 `DashboardViewModel` 中保存短生命周期的诊断目标 ID/名称，不覆盖用户已有的任务搜索、类型、时间和范围筛选。服务端查询使用目标名称，内存过滤优先稳定 ID，加载完成后按目标选择任务；目标没有记录时明确提示。用户修改任一任务筛选后，诊断目标自动清除。
- 目的导航会取消输入防抖和旧任务查询，再启动一次代际保护的读取；没有加入 `Task.Delay`、`ScrollIntoView` 或新的全局导航框架。生产 Shell 的工作区页只创建一次，标签 `SelectedIndex` 继续由 VM 双向保留。
- `FindingNavigationResolverTests` 新增目标缺失、精确 ID、名称兜底和无身份场景；`PurposeNavigationSourceTests` 守护单次显式加载、选中恢复和工作区页/标签上下文。真实宿主仍需验证原用户主题、目标隐藏和快速连续入口。

## 2026-09-08 L05 六态与运维夹具覆盖

- `FakeDashboardData` 的默认构造保持 Ready 兼容；带 `WorkspaceFixtureState` 的构造可生成六态，非 Ready/Stale 会清空对应媒体、诊断和运维动作集合，Stale 保留旧数据并显示过期提示。Fake 不记录媒体文件内容。
- `Program statefixtures` 必须渲染真实 `MediaCenterView`/`MaintenanceView`，不能只渲染裸 `WorkspaceStatePresenter`。它检查关键公共绑定反射存在、状态覆盖层可见性、Stale banner、数据表面 DIP 尺寸和 Ready 运维动作数；缺字段直接进入失败报告。
- 代表性画面已目视复核：Stale 收件箱显示旧行、过期提示和选择框仍在同一行；Error 画面显示失败覆盖层；维护“下一步运维”显示恢复巡检、云端重试和隔离账本三项。错误/离线底层表面仍保留，但状态覆盖层承担不可用语义。
- 夹具暴露的布局根因是 Stale banner 出现后 `MediaInboxGrid` 被压成零高，不是数据集合丢失。`MediaCenterView.ApplyResponsiveLayout` 现在在 Stale banner 可见时启用外层页面滚动，同时保留内部 DataGrid 有限视口、虚拟化和底部操作区。
- 证据目录 `.tmp/l05-statefixtures` 为当前可再生输出；完成交付前只保留当前报告/必要证据，禁止将整批 PNG 或临时构建物提交 Git。真实 Playnite/FusionX 仍需用户环境按视频操作复测。

## 2026-09-08 Q6-04 构建身份闭环（隔离包验收）

- `WorkerLauncher` 仅在实际身份与期望身份都已知且不一致时判定不可复用。旧 Worker 没有 `BuildIdentity`，或任一身份包含 `+unknown`，仍按公共版本/协议继续工作，但健康结果明确标记为“构建身份未验证”，不伪造同源证明。
- `IsBuildIdentityCompatible` 将空/unknown 视为不可验证而非冲突；同版本两个已知提交不同仍返回不兼容。定向 `BuildIdentityTests` `3/3` 通过。
- `scripts/package.ps1` 的隔离正例已读取六个实际 PE 程序集并确认同源；同版本旧插件、`+unknown` 插件、脏工作树、无 Git 夹具均在生成成功提示前失败，失败后 `GSC_BUILD_COMMIT` 恢复为调用方值；最终文档提交后的 HEAD 也已重新打包校验。

## 2026-09-08 Q6-03 运维云端告警归并

- `DashboardViewModel.MaintenanceActions.cs` 的云端来源必须先合并再筛告警：`Snapshot.CloudTransfers.Items` 和分页 `CloudTransferItems` 按 `TransferKey` 聚合，`UpdatedUtc` 较新者胜出，同时间分页明细优先。不能先 `Where(IsAttention)` 再 `GroupBy`，否则旧 Failed 会遮住已 Uploaded/RemoteVerified 的新状态。
- `MaintenanceCloudTransferMergeResult` 同时给出被新明细解决的快照告警数和明细新增告警数，用于修正“未全部加载”的剩余数量与维护摘要；这避免旧摘要计数在已解决记录消失后继续生成伪造占位。
- 时间语义固定：恢复巡检使用 `LastVerifiedDisplay`，云端使用 `LastAttemptDisplay`，隔离账本使用 `LedgerUpdatedDisplay`；上传尝试不再显示为远端验证成功。
- `MaintenanceCloudTransferResolverTests` 5 项通过。真实 Playnite 分页刷新、状态更新、故障注入和维护页录屏仍未完成，代码测试不能替代宿主验收。

## 2026-09-08 Q6-02 媒体状态按上下文隔离

- `DashboardViewModel.WorkspaceStates.cs` 现在用 `MediaWorkspaceStateCache` 保存媒体详情/收件箱状态，成功时间和错误只属于当前上下文；详情上下文由游戏 ID、媒体筛选、搜索词组成，收件箱上下文由待归类/已忽略模式组成。
- 同上下文刷新失败仍保留旧数据并显示 Stale；新游戏、新筛选或新模式没有自己的成功缓存时显示 Error。媒体筛选/搜索/选中游戏改变时会推进 `mediaPageGeneration`、取消旧请求并重置分页状态，避免旧响应在防抖新请求前写入。
- Q6-02 测试覆盖同上下文 Stale、A 成功/B 首失败、旧请求晚回、收件箱模式隔离和取消状态，共 4 项。真实 Playnite 故障注入、状态切换录屏和用户主题仍未完成；后续文档只能写“代码/测试完成，宿主待验收”。

## 2026-09-08 Q6-01 状态面板重试命中修复（代码/离屏已完成）

- 复核确认三处带 `RetryCommand` 的失败状态面板（媒体收件箱、媒体详情、维护审计）不应设置 `IsHitTestVisible="False"`；该父级值会让模板内按钮永远无法鼠标命中。`MaintenanceView` 中没有重试命令的降级提示仍保持非阻塞，不要误删其语义。
- `Themes/Redesign.xaml` 的重试按钮空命令判断改为针对 `RetryCommand` 依赖属性的模板 `Trigger`；Loading 明确隐藏按钮但仍由可见面板阻挡底层操作。按钮保留共享样式、绑定和键盘行为，并补 `AutomationProperties.Name`。
- 新增 `WorkspaceStatePresenterBehaviorTests`：实际加载生产 `GscWorkspaceStatePresenter` 模板，在 STA WPF Window 中验证 Error/Offline 的可见性、绑定命令和视觉树命中，Loading 的底层阻挡，以及 Enter/Space 各执行一次；当前 `5/5` 通过。`WorkspaceStateSourceTests` 额外守护三处使用点不重新加父级禁止命中。
- 新增 RenderHarness `stateprobe`，生成双主题 Error/Offline/Loading 状态图和报告：`docs/design/reviews/2026-09-08-quality/state-*.png`、`stateprobe-report.txt`。这是插件模板的离屏证据，不是真实 Playnite 截图。
- 本阶段 Release 隔离构建与全量测试为 Core `72/72`、Worker `303/304`（1 跳过）、Playnite `376/438`（62 跳过）；源码/XAML/WPF 静态检查无 error。完整 `render-qa` 本次仍报告 25 个媒体小视口/侧栏快速切换问题，不能写成 render-qa 全部通过；真实宿主复测仍待完成。

## 2026-09-08 表格滚动诊断与局部模板修复（当前验收边界）

- 针对用户视频中的任务表/媒体待归类表正文空白、行内容漂移、选中框与文字分离及末行截断，本轮只处理表格滚动正确性，没有扩展功能或继续做页面美化。新增 `DataGridScrollDiagnostics`，只记录稳定 ID、计数、滚动范围、实际 `ScrollViewer`/`IScrollInfo`、`ScrollContentPresenter` 矩形、首末行坐标/高度、单元格内容可见性、滚动条占用和分页/锚点代际，不记录文件内容。
- 先在离屏相同数据探针中复现布局级坏路径：媒体 4468 条、短高度 287 DIP 时，未限制外层无限测量的版本出现 `items=4468`、`ScrollViewer viewport=4468x1963.33`、`ScrollableHeight=0`、`ScrollContentPresenter height=196592`，所有行被一次性测量，滚动动作不再改变可见窗口；这不是正常的中间滚动。去掉短窗口条件后又出现 `gridH=0`/内容视口高度 0，确认不能靠简单关闭页面 fallback 解决。
- 静态检查了用户当前 FusionX 的 DataGrid 模板：未修改 FusionX 或 Playnite 全局样式。插件在 `Themes/Redesign.xaml` 增加局部 `GscRedesignDataGridTemplate`，保留 `PART_ColumnHeadersPresenter`、`PART_ScrollContentPresenter`、ItemsPresenter、双向滚动条、列宽/排序/选择/键盘和虚拟化绑定；内部网格为表头 `Auto`、内容 `*`、水平滚动条 `Auto`，垂直滚动条只占内容行区域。
- `MediaCenterView` 的锚点恢复不再用 `CanContentScroll` 猜单位：捕获/判断可见性使用真实 `ScrollContentPresenter` 的 DIP 矩形；只有实际发现 `VirtualizingStackPanel`、`ScrollUnit=Item` 且双方 `CanContentScroll=true` 时才按逻辑项恢复，否则使用 DIP 像素差；请求/上下文代际保护保留。短窗口最终布局保留有限 `MaxHeight`，避免重新进入无限测量。
- 当前离屏证据保存在 `.tmp/gridprobe-final-20/gridprobe-report.txt`：任务/媒体执行 20 次顶部、底部和中间往返拖动，并执行滚轮、PageUp/PageDown、Ctrl+End、最后项定位；任务/媒体 50、400、2000 和媒体 4468 条、287/311/337/353/419/640/840 高度、600 DIP 窄宽水平滚动均为 `gridprobe OK`，报告诊断异常数为 0。窄宽媒体底部末两行 `y=80..124`，水平条 `y=159.33..171.33`；普通宽度媒体末行完整落在内容视口内。
- Release 构建 0 警告/错误；Playnite 全量测试 `371 通过 / 62 跳过 / 0 失败`，源码校验和 XAML 结构校验通过。真实 Playnite FusionX 窗口未在本会话中重播用户视频，也没有真实宿主前后录屏，因此本轮只能写“离屏验证通过，待宿主验收”，不能宣称用户视频问题已在真实宿主解决。

## 2026-09-08 两项截图问题的当前验收边界

- Worker 身份修复已经过真实打包和安装：包内六个实际程序集、Playnite 扩展目录中的插件/Worker/共享 DLL，以及运行中命名管道握手均为同一个最终打包 HEAD 身份。`scripts/package.ps1` 会在打包前从 PE 元数据读取实际身份，同源不一致或当前工作树不干净时停止，不生成混合包。
- 实际 Playnite 安装目录为 `C:\Users\lopmatu\AppData\Roaming\Playnite\Extensions\GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec`。运行中唯一 Worker 的 `system.handshake` 和 `media.inbox.page` 均成功；后者返回真实数据 `totalCount=4615`。受控停止唯一已核实路径后，管道连接失败，宿主恢复后再次握手成功。
- 媒体页代码已收口为有限 PageHost 与左右独立滚动；离线状态不再把读取失败显示成真实 0。RenderHarness/源码门禁通过，但真实 Playnite 窗口内的导航、尺寸切换、选择/历史滚动和截图尚未完成。
- 32 项扩展计划保持暂停。没有 CUA 宿主操作证据时，后续交接必须写“代码修复已完成，真实视觉宿主验收待完成”，不得写“用户截图问题已解决”。

## 2026-09-08 用户截图反馈修复约束

- Worker 的单实例互斥语义是：重复进程可以以退出码 0 结束，但 Launcher 不能据此直接报告启动失败。启动子进程发现退出码 0 时，必须在有限等待内用期望版本/构建身份探测现有实例；健康则复用并清理本次子进程引用，不健康才保留真实错误。
- 媒体待归类页的 DataGrid 必须在表格卡片顶部正常出现，不能因外层页面 ScrollViewer 的无限测量和父级 `*` 行被排列到卡片底部。表格卡片/内部布局/DataGrid 使用顶部对齐；DataGrid 仍保持固定的可读最小高度、内部滚动和既有虚拟化契约。
- Production Shell 媒体探针必须同时检查 PageHost、页面滚动方向、DataGrid 最小高度以及表卡到 DataGrid 的顶部间距；只检查 DataGrid `ActualHeight` 不足以发现“表格被推到首屏之外”的布局回归。
- 本轮验证：Release 全量 Core `72/72`、Worker `303/304`（1 跳过）、Playnite `369/431`（62 跳过）；XAML `19/19`、源码校验、WPF `0 errors/21 warnings/172 info`、双主题/多尺寸/resize/Production Shell `render-qa OK`。真实 Playnite、DPI/高对比度和完整键盘仍属外部验收边界。

## 2026-09-08 连续开发执行约定

- 用户希望后续无需完成一点就重新要计划，新增 [32 项连续开发队列](CONTINUOUS_DEVELOPMENT_PLAN_2026-09-08.md)。L01～04 对应已确认 Q6，后续为有依赖/验收条件的增强或验证任务，不能都当作既有缺陷。
- 默认完成一项验证、同步文档并提交 push 后继续下一项。仅在必要产品决策、外部条件或授权边界出现时询问；阻塞项不阻断无依赖任务。已满足部分核对证据后跳过。
- 本轮只规划；不代表启动后台开发，不新增真实存档/云端写入或安装授权。后续用任务状态/commit/证据/阻塞表保持可续跑。

## 2026-09-08 Q6 收口计划与证据边界

- 下一轮依据 [97131f0 独立质量复核](QUALITY_REVIEW_2026-09-08.md)，不重复重做 Q4/Q5。新状态面板的重试不能继承 `IsHitTestVisible=false`；成功读取时间和错误必须按游戏/模式隔离；运维状态先按身份与新鲜度归并再筛告警。
- 构建身份底座不等于发布链已验收：SkipBuild 必须核对旧插件与新 Worker 的实际身份，`+unknown` 必须显式处理。报告将脚本推导问题与真实安装验收区分。
- 本轮全量构建/测试、静态和离屏检查通过；Fake 尚未覆盖完整新增状态/运维绑定面。优先补行为测试和六态夹具，再优化设置首屏、运维密度和真实宿主性能。

## 2026-09-08 X2-03 构建身份实现约束

- 公共扩展版本仍由 `extension.yaml`/`VersionPrefix` 控制；构建身份是独立诊断字段，来自 `AssemblyInformationalVersion`，格式为 `版本+提交号`，没有提交号时必须显示 `unknown`，不得用它替代协议版本或擅自升级插件版本。
- `scripts/package.ps1` 必须让插件和 self-contained Worker 使用同一个 Git HEAD 构建身份；`WorkerHandshakeDto`/`WorkerPingDto`、Dashboard 快照和诊断包都要暴露该身份。旧 Worker 返回空身份时保持兼容，两个新构建身份已知且不一致时必须标记为不可复用并重新启动/提示。
- 当前发布窗口尚未执行安装替换；不能把本地打包或离屏检查写成已安装 DLL 验收。实际发布时需核对 `extension.yaml`、程序集版本、包内 Worker、握手身份和 Playnite 扩展目录中的文件来源。
- 当前自动验证：Release 构建 0 警告/0 错误；Core `72/72`、Worker `303/304`（1 跳过）、Playnite `368/430`（62 跳过）；WPF 静态 `0 errors/21 warnings/172 info`，`render-qa OK`。

## 2026-09-07 X2-02 运维总览实现约束

- “下一步运维”必须从真实 `HealthInspectionStateDto`、`CloudTransferStatusDto`/摘要和 `RetentionQuarantineEntryDto` 生成；每个动作都保留真实记录 ID 或明确导航目标，不新增一个没有对象边界的全局自动修复按钮。
- 云端项只在当前已加载的真实记录上显示详情；若摘要中还有未加载的关注项，必须显示“未全部加载”并把动作导向现有云端队列分页。重试、远端 check、认证处理继续使用原有命令与权限语义。
- 隔离账本再次协调只接受 `EntryId`，IPC 请求必须 `Confirmed=true`；Worker 仍检查原/隔离路径安全、文件大小/身份和账本状态，遇到冲突不得覆盖或删除未知文件。启动恢复与用户针对单条记录的协调共用同一状态机。
- 当前验证：Release 构建 0 警告/0 错误；Core `72/72`、Worker `303/304`（1 跳过）、Playnite `366/428`（62 跳过）；源码/XAML 门禁、WPF `0 errors/21 warnings/172 info`、`render-qa OK` 均通过。真实宿主、DPI/高对比度、完整键盘和真实故障注入仍是外部边界。

## 2026-09-07 X2-01 工作区状态实现约束

- 媒体当前列表、收件箱和维护诊断统一使用 `WorkspaceDataState` 的 Loading/Ready/Empty/Stale/Error/Offline 语义，并通过现有 `WorkspaceStatePresenter` 呈现；不要回退为只看集合 Count 或只显示全局 `StatusMessage`。
- Loading/首失败时可以覆盖内容区域；已有成功数据刷新失败必须保留旧集合、选择和编辑草稿，使用降级提示显示上次成功读取时间与错误详情，并把 RetryCommand 接回真实的媒体/诊断刷新命令。Worker 离线优先于普通错误状态。
- 状态变更必须受媒体分页/收件箱代际保护，旧请求不得把新游戏、新模式或新列表覆盖为 Stale/Ready；`RefreshCoreAsync` 的工作区级失败也要结束 Loading 状态。不要用 `Task.Delay` 制造成功或加载效果。
- 当前验证：Release 构建 0 警告/0 错误；Core `72/72`、Worker `302/303`（1 跳过）、Playnite `365/427`（62 跳过）；XAML `19/19`、源码校验、WPF 静态审查 `0 errors/21 warnings/172 info`、`render-qa OK`。离屏证据不能替代真实 Playnite、DPI/高对比度和完整键盘验收。

## 2026-09-07 媒体间距与任务范围实现约束

- 待归类页签的按钮不能贴住工作区边界：`MediaTabControl` 保留 `8,0,8,0` 外边距，页签保留 `16,8` 内边距和 `4` 间距；批量处理卡片保留 `14,12,14,0` 内边距，底部操作区保留 `16,12,16,0` 与 `0,8,0,0` 间距。后续只能在共享布局契约内调整，不要为压缩高度移除这些呼吸空间。
- Dashboard 的最近历史窗口与活动任务集合必须分开查询；活动任务包括 Queued、Running、WaitingForUser，并按稳定创建时间/任务 ID 排序合并。任务摘要 SQL 的云端等待数必须覆盖等待用户状态。
- 批量安全重试必须基于当前 `TasksView` 结果计算，按游戏和任务类型去重，并向用户说明当前结果总数、实际计划、去重数和未纳入数；不能回退为对首页最近任务集合盲目重试。
- 当前验证：Release 构建 0 警告/0 错误；Core `72/72`、Worker `302/303`（1 跳过）、Playnite `364/426`（62 跳过）；XAML `19/19`、源码校验、WPF 静态审查 `0 errors/20 warnings/172 info`、`render-qa OK`。离屏渲染不等于真实 Playnite、DPI/高对比度或完整键盘验收。

## 2026-09-07 任务页紧凑空间实现约束

- `TaskCenterView` 的紧凑详情按钮不能与 `TaskGrid` 共享一个会发生溢出的有限行：详情关闭时使用队列底部按钮，详情打开后必须隐藏该按钮并在 `TaskDetailCard` 内提供“收起详情”，避免按钮覆盖第三行。
- 紧凑详情仍保留 `TaskGrid.MinHeight=180`、36 DIP 数据行和 `TaskDetailScrollViewer.MaxHeight=160` 的可读性底线；Production Shell 探针必须按 `PageHostForAudit` 的实际 DataGrid 视口统计展开后的完整行，1040×700 至少 3 行，1100×720 至少 3 行。
- `TaskHasActiveFilters` 必须覆盖搜索、状态、游戏、类型、历史范围和时间范围，并在每个对应 setter 及清除路径通知；无有效筛选时只隐藏清除按钮，不能隐藏“更多筛选”或断开真实 `ClearTaskFiltersCommand`。
- 当前验证：紧凑详情定向 `4/4`；Core `72/72`、Worker `300/301`（1 跳过）、Playnite `364/426`（62 跳过）；XAML `19/19`、源码校验、WPF 静态审查 `0 errors/20 warnings/172 info`、`render-qa OK`。真实 Playnite、DPI/高对比度、完整键盘和大库连续滚动仍是外部验收边界。

## 2026-09-07 质量计划状态同步

- 当前质量计划 `QUALITY_REVIEW_2026-09-07.md` 已将 Q4-01～Q4-03 的历史待办改为已完成状态：媒体云端重试独立 IPC/结构化结果、目标标签导航、维护页紧凑详情折叠与 Production Shell 离屏探针均已落地并有回归证据。
- 后续阅读计划时，只把真实 Playnite、Rclone/远端、用户数据、DPI/高对比度和完整键盘流程视为外部人工验收边界，不要重新实现上述已完成代码。

## 2026-09-07 动效门控实现约束

- `AcrylicProductionShellView.NormalizeMotionIfDisabled()` 是侧栏动效的统一终态入口：当 Dashboard 的设置或 Windows 动画偏好变为关闭时，必须取消 `ColumnDefinition.Width`、内容层 `Opacity`/`TranslateTransform.X` 的活动时钟，恢复内容不透明、当前侧栏宽度和 `sidebarTransitionRunning=false`。
- Dashboard 的 `ApplyAdaptiveTheme()` 在传播 `MotionEnabled` 后调用生产 Shell 的终态清理，并清理自身已知的入口、游戏筛选、详情页、状态胶囊、对话框和任务详情过渡。动画关闭只跳过视觉过渡，不改变页面可见性、命令、Binding 或业务状态。
- 不要把动画时长改成 `DynamicResource` 直接塞入 Storyboard；WPF 会尝试冻结跨线程时间线并在测试/宿主中抛出冻结异常。当前安全路线是代码级门控、即时终态清理和资源级 PopupAnimation `Fade/None`。
- 行为测试必须在真实 STA `Window` 中验证关闭动画后的最终几何和无活动过渡；离屏渲染不能证明 60fps。当前动效门控定向 `7/7`，全量 Playnite `363/425`（62 跳过），WPF 静态审查 `0 errors/20 warnings/172 info`，RenderHarness `render-qa OK`。最新 `artifacts/GameSaveCenter-0.6.73.pext` 已重新打包并完成包内版本/必需文件校验，未安装到 Playnite；真实 Playnite/DPI/高对比度/键盘和流畅度仍是外部验收边界。

## 2026-09-07 Q4-00 媒体分页锚点行为实现约束

- `MediaCenterView` 的延迟 `RestoreAnchor` 回调必须携带 `anchorRestoreGeneration`；切换游戏、收件箱模式、ViewModel、页面生命周期或产生新的集合/选择上下文时递增代际并统一清除待恢复锚点、选择和计时器。旧回调只能静默退出，不能改动当前列表。
- 收件箱恢复除代际外还要校验 `MediaInboxMode`；`PropertyChanged` 订阅必须随 ViewModel Attach/Detach 成对管理，避免离开页面后旧 ViewModel 继续触发失效逻辑。
- `selectionRestoreQueued` 在整个恢复/重试链路中保持占用，只有恢复成功、无法恢复并显示提示、或上下文失效时释放。不要在单次 `RestoreSelection` 的 `finally` 中提前清零，否则集合 Reset 的后续 `SelectionChanged` 会覆盖待恢复选择。
- 行为测试必须在真实 STA `Window` 中驱动 View、Dispatcher 和私有恢复链路，至少验证旧上下文回调不显示过期提示，以及锚点被裁掉时提示可见且恢复锁释放。源码契约断言只能作为补充，不能替代该行为证据。
- 当前证据：媒体锚点定向 `5/5`；全量 Core `72/72`、Worker `300/301`（1 跳过）、Playnite `362/424`（62 跳过）；构建 0 错误，存在 1 个 `NU1900` 网络审计警告。真实 Playnite、DPI/高对比度、完整键盘和大库连续滚动仍是外部验收边界。

## 2026-09-07 Q5-01 设置页操作反馈实现约束

- 设置页状态必须由当前设置指纹、`VerifySettings` 结果和 Playnite 编辑生命周期共同决定：验证错误优先于脏状态；无错误且指纹与提交基线不同显示未保存；相同显示已保存。不要添加绕过 Playnite 的自定义保存按钮。
- `GameSaveCenterSettings.CreateSettingsFingerprint()` 必须排除 `DeviceId`，因为它是安装身份而非用户可编辑设置；`SettingsCommitted`/`SettingsReverted` 只在 Playnite 的 `EndEdit`/`CancelEdit` 边界更新基线。导入 Portable JSON 仍是当前编辑缓冲区的修改，DataContext 重绑不得清掉未保存状态。
- `SettingsSaveHintText` 在所有高度保持可见，矮窗口只隐藏冗长副标题/说明，不隐藏保存语义；状态文字需有 AutomationProperties.Name 与 Tooltip，并保持双主题资源可用。真实宿主仍需检查 Playnite 保存/取消后的回写、DPI 和键盘焦点。
- 当前验证：RenderHarness 双主题、多尺寸、resize `render-qa OK`；全量 Core `72/72`、Worker `300/301`（1 跳过）、Playnite `360/422`（62 跳过）；XAML 19/19、源码校验和差异检查通过。离屏结果不能当作真实 Playnite 宿主验收。

## 2026-09-07 Q4-04 动态分页一致性实现约束

- `cloud_transfers` 与 `classification_history` 使用 SQLite 持久化修订号，不使用动态查询结果里的 `strftime('now')` 或 `MAX(updated_utc)` 充当快照标识；旧库启动时必须创建 `query_revisions`、种子行和幂等触发器。
- 云端修订由 `cloud_transfer_queue`、`cloud_retry_queue`、游戏名称/云端状态、媒体归属/云端/归类状态变化触发；归类历史修订由批次和批次条目的增删改触发。筛选共用全局修订号，宁可要求刷新也不能静默漏项。
- 两套分页响应必须同时带 `ConsistencyToken`、`PageResetRequired` 和 `PageResetReason`。Worker 在读前拒绝旧 token，在读后发现修订变化也返回 reset；页面不能用空结果宣称“已加载全部”。
- Playnite 继续按稳定 `TransferKey`/`BatchId` 恢复选择。reset 时清空已加载窗口、提示用户列表已变化并自动从第一页重试一次；不要改成拉取全量列表，也不要把可变排序键游标当成一致性快照。
- 当前只验证 Worker/离屏客户端链路；真实 Playnite 宿主、跨进程持续写入、DPI/高对比度和人工键盘仍是外部验收边界。

## 2026-09-07 Q4-03 紧凑维护页详情布局实现约束

- 诊断和进程映射的紧凑断点为 PageHost 宽度 `< 980` DIP。选中行不能自动让 Inspector 进入 Auto 行；默认必须保留列表，详情只能通过命名的紧凑按钮展开，详情打开后才占用有限的第三行空间。
- `FindingsGrid`/`MaintenanceProcessGrid` 的选择变化必须关闭旧详情，`Esc` 关闭当前紧凑 Inspector；按钮提供 AutomationProperties.Name，Tab/Shift+Tab 交给 WPF 键盘导航。宽度恢复到 980 以上时回到并排详情，不能保留紧凑抽屉状态造成空列。
- 进程详情的唯一滚动所有者是 `MaintenanceProcessInspectorScrollViewer`，诊断继续由 `MaintenanceDiagnosticsInspector` 承担；不要再把详情拆成多个竞争滚动条。列表行数回归必须统计可视树中实际可见且有高度的 `DataGridRow`，不能只断言 `ActualHeight`。
- RenderHarness 的 `RunProductionShellMaintenanceProbe` 必须使用 `AcrylicProductionShellView.PageHostForAudit`，覆盖 1040×700、1100×720、1366×768，并同时验证紧凑默认关闭、按钮可见、打开后可见及至少 3 个完整行；离屏结果仍不等同真实 Playnite。

## 2026-09-07 Q4-01/Q4-02 媒体重试与目标标签导航实现约束

- 媒体云端重试必须走 `MessageTypes.RetryMediaCloudUpload` 和 `MediaCloudRetryRequestDto`，Worker 只调用 `MediaSyncService.RetryCloudUploadForUserAsync`；`MessageTypes.RetryCloudUpload` 继续只服务备份，禁止用 `SyncMedia` 冒充“重试上传”。该入口不能扫描来源或归类新文件。
- `MediaCloudRetryResultDto.Outcome` 是 UI 的事实来源：`Submitted` 才能显示提交/完成；`PausedByPolicy` 必须说明游戏策略未允许上传；`CannotSubmit` 必须展示安全模式、全局开关、Rclone、失败或取消原因，不能无条件写成功提示。
- `MediaTabIndex` 初始保留当前游戏媒体页，`SaveTabIndex` 初始保留历史页；首页待归类动作设置媒体索引 0，诊断存档路径动作设置存档索引 1，且必须在切换 `CurrentWorkspace` 前设置，让生产 Shell 的页面绑定不会先落到默认标签。
- 本阶段定向测试覆盖 Worker 策略暂停/云端不可用不创建任务，以及 Playnite IPC/Tab 绑定契约；仍需真实 Playnite 宿主、真实 Rclone、DPI/高对比度和用户数据验收。

## 2026-09-07 质量复核补审更新

- UI3-07 已提交 9e93909；独立补审定向测试 5/5，但新增的 View 契约测试仅查源码字符串。下一步以 [质量报告 Q4-00～04](QUALITY_REVIEW_2026-09-07.md) 为收口计划，先补锚点真实行为与上下文切换验收。
- `docs/design/reviews/2026-09-07-quality/` 属于 b0aa85a 的完整离屏审计，不能归于 UI3-07；后续真实宿主证据另记版本与画布。

## 2026-09-07 UI3-07 缓存窗口与滚动锚点实现约束

- `MediaCenterView` 的“加载更多”不是纯粹追加：点击事件必须先捕获当前列表可见首项、相对位置/逻辑偏移和多选 ID；`MediaPageAccumulator` 发出 Reset 后，由视图按 `VirtualizingWrapPanel` 的像素偏移或 DataGrid 的逻辑 item 偏移恢复。不要用一个像素公式同时处理两种滚动模型。
- 当前游戏媒体、未归类收件箱、已忽略收件箱各自保持既有 2000 项窗口。窗口裁掉锚点时必须显示“返回最新/重新载入较新内容”路径；`ReloadMediaWindowCommand` 和 `ReloadMediaInboxCommand` 只重新请求首批，不自动修复、删除或修改媒体。
- 多选语义是“仅当前保留窗口”：按收件箱模式分开保存 ID，恢复只将仍在 `Items` 中的项目加入控件选择；批量操作继续从当前 `SelectedItems` 取值，不能让被裁掉的 ID 被静默执行。SelectedMedia、SelectedInboxMedia 和未保存备注/收藏草稿仍归 ViewModel 管理。
- 集合 Reset 后的 WPF SelectionChanged 不能覆盖待恢复 ID 集合；`selectionRestoreQueued` 必须在恢复或 15 秒无变化超时后释放。离开页面需解除集合事件，避免旧 ViewModel 接收事件。
- RenderHarness Fake 必须暴露新增恢复命令、分页状态和摘要；RenderHarness 只证明合成壳层的布局/夹具，不等同真实 Playnite。真实大库第 2/11 页滚动、多选、快速切换、编辑中加载、DPI 和高对比度仍属人工验收边界。

## 2026-09-07 UI3 质量复核与后续边界

- [质量报告](QUALITY_REVIEW_2026-09-07.md) 以 `b0aa85a` 为冻结基线；UI3-07 并发改动不属于该次验证。报告只新增文档和原始证据，不改变生产行为。
- 后续优先补媒体重试的独立 IPC/真实结果反馈，以及媒体收件箱、存档路径页的明确 Tab 路由；现有 workspace 跳转不等于到达按钮承诺的位置。
- 紧凑维护 Inspector 挤压列表在独立小画布中复现；须先扩展生产 Shell 几何检查，不能把独立页面场景名当成真实宿主尺寸。动态 offset 分页漏项是静态推导边界，后续先补变更回归再改一致性契约。
- 全量构建/现有测试通过不代表真实宿主、DPI、动画流畅度或 UI3-07 已验收；下一轮遵循报告的分阶段计划，不重复重做 UI3-00～06。

## 2026-09-07 UI3-06 存档与维护详情实现约束

- `SaveCenterView` 的历史时间展示使用 `MM-dd HH:mm`，完整时间只通过 Tooltip 提供；`SaveHistoryTimeColumn` 在窄宽度仍不得低于能读出日期/时间的宽度，类型和状态列优先于备注，备注继续使用星号列。Inspector 中恢复可用性必须先于备注编辑，但安全恢复命令、隔离校验、确认和 PreRestore 保护不能被 UI 重排绕过。
- `MaintenanceView` 的主诊断表只显示等级/游戏/问题三列；`Detail` 与 `SuggestedAction` 只能在选中 Inspector 中完整换行。`ApplyFindingsColumnLayout` 的主表分支按三列处理，审计表仍保持自己的四列契约，不要把两者混用。
- 诊断跳转统一经过 `FindingNavigationResolver`：带云端/Rclone/远端身份进入云队列，任务/Worker/健康或明确任务提示进入失败任务筛选，有游戏身份的其他诊断进入存档工作区并选中同一游戏；未知或无身份返回 `None`。动态按钮只隐藏/显示，不复制多套命令或自动执行修复。
- `CloudTransferStatusDto`、`CloudTransferSummaryDto` 和媒体归类 DTO 只改变显示映射，不改变状态存储/IPC 值；校验失败与上传失败必须可区分，未知未来状态不得把内部英文枚举直接泄漏到界面。设备说明不得写死设备数量。
- UI3-06 当前证据：全量 Core `72/72`、Worker `296/297`（1 跳过）、Playnite `352/414`（62 跳过），XAML `19/19`，WPF 静态审查 0 error，RenderHarness 双主题/多尺寸/resize `render-qa OK`。离屏渲染不等同真实 Playnite，下一阶段继续 UI3-07 缓存窗口与滚动锚点。

## 2026-09-07 UI3-05 首页优先级与下一步动作实现约束

- Hero 的优先级唯一来源是 `OverviewPriorityResolver.Resolve(snapshot, isOnboardingPending)`；不要在 XAML 重新按多个计数拼接互相竞争的主标题或命令。顺序固定为 Worker、Onboarding、Cloud attention、Unassigned media、Warning games、Healthy refresh。
- `DashboardViewModel` 暴露 `OverviewPriorityKind/Title/Description/ActionText/ActionToolTip/ActionCommand`，快照刷新和 onboarding 完成/跳过都必须通知这些派生属性。`OpenMediaWorkspaceCommand` 只切换到真实 `WorkspaceKind.Media` 并调用现有工作区加载路径。
- 首页 Hero 只保留一个动态主操作；全局工具栏处理全部备份/媒体同步，当前游戏卡处理当前游戏备份/详情刷新，关注项保留在风险卡上下文入口。不要恢复被移除的工具栏、当前游戏卡关注按钮或首次环境检查重复横幅。
- 离屏夹具必须把 `CloudTransferViewSummary` 回填到 `Snapshot.CloudTransfers`，否则首页 Hero 会错误地落入媒体/游戏分支。RenderHarness 只是合成壳层证据，真实 Playnite、DPI、高对比度和用户快照仍需人工复核。

## 2026-09-07 UI3-03 媒体归类批次历史与可找回撤销

- `media_classification_batches` 已有的 durable 记录现在通过 `GetMediaClassificationBatchHistoryAsync` 聚合成分页摘要；查询不加载全部项目明细，按批次更新时间和批次号稳定排序，状态筛选不会改变聚合语义。
- `MediaClassificationBatchSummaryDto.IsUndoable` 只对 `Applied`/`AppliedWithConflicts` 且仍有 `Applied` 条目的批次为真。撤销服务仍只接受这两个状态；撤销后 `UndoneWithConflicts` 与冲突计数保留为不可覆盖的历史事实。
- Dashboard 的预览、应用、撤销分别更新“预览对象”“最后应用批次”和历史选中项；不要把预览 ID 当作可撤销 ID，也不要在撤销有冲突时无条件清除批次上下文。
- UI 历史列表使用有限 `ListBox`（最小 236 DIP，最大 260 DIP）并保留内部滚动；筛选变化通过 code-behind 触发刷新，历史请求拥有独立 generation/CTS，离开 Media workspace 必须取消。

## 2026-09-07 UI3-04 任务中心筛选与密度实现约束

- `TaskCenterView` 在 `<760 DIP` 时只把搜索、状态和刷新留在主工具栏；类型、历史范围、时间范围和游戏筛选放入 `TaskMoreFiltersHost`。不要复制 ComboBox 以实现响应式布局：`SetCompactFilterPlacement` 会将同一批真实控件从 `TaskFiltersPanel` 移到 StackPanel，宽屏再移回，Binding/选择状态因此连续。
- `TaskActiveFiltersSummary` 必须随搜索、状态、游戏、类型、范围和时间变化通知；收起的 `TaskMoreFiltersExpander` 仍需显示生效条件并提供 `ClearTaskFiltersCommand`，不能因为隐藏控件而隐藏已生效条件。清除按钮位于 Expander header，点击要阻止误触发折叠切换。
- 任务时间列使用 `TaskTimeCell` 显示 `MM-dd HH:mm`，Tooltip 保留 `yyyy-MM-dd HH:mm:ss`；时间/状态/进度的最小宽度优先于详情，详情可以省略但不能改变真实数据。紧凑模式使用 `TaskCompactDataGridRow`（36 DIP），宽屏必须恢复 `GscStableDataGridRow`，不要修改共享表格行高来迁就单页。
- `FakeDashboardData` 需要覆盖 `TaskTotalCount`、`TaskHistoryScopeOptions`、`TaskHistoryRangeOptions`、`TaskLoadedSummary`、`TaskHistoryHasMore` 和任务详情/批量命令；夹具字段为空不应被误判为生产 Binding 缺陷。最终离屏只能说明合成壳层几何，不能代替真实 Playnite 宿主、DPI 或高对比度验收。

## 2026-09-07 UI3-02 云端队列明细分页与用户操作入口

- 生产维护中心现在有独立的云端队列 Tab。`DashboardViewModel.CloudTransfers` 以 `CloudTransferStatusRequestDto` 调用 Worker 的 `GetCloudTransferStatus`，页大小固定请求 100，服务端聚合总量，客户端按 `TransferKey` 去重追加并在刷新时恢复当前选择。
- `CloudTransferStatusDto` 增加面向 UI 的备份/媒体类型显示；列表和 Inspector 同时显示状态详情、错误码、下次尝试和保证级别，`Uploaded` 不得写成“已验证”。
- `VerifyCloudTransferCommand` 使用 `VerifyCloudTransfer` 只读 check；`RetryCloudUploadCommand` 对 Backup 使用既有云上传重试，对 Media 使用 `SyncMedia` + `UploadAfterSync`，认证需处理状态不被当作自动可重试上传。请求有独立 CTS 和代际，离开维护页会取消云队列请求。
- `MaintenanceView` 宽屏为表格 + Inspector，低于 980 DIP 时表格保持有限高度，详情通过 `CloudTransferCompactDetailsButton` 打开并堆叠；RenderHarness 已加入真实状态夹具和维护页 Tab 索引回归。验证基线为 Core `65/65`、Worker `295/296`（1 跳过）、Playnite `342/404`（62 跳过）、XAML `19/19`、RenderHarness `render-qa OK`。
- 未取得真实 Playnite 宿主、真实 Rclone 凭据/远端、DPI/高对比度证据；下一阶段按 UI3-03 处理可找回的媒体归类批次历史。

## 2026-09-07 UI3-00/01 媒体收件箱可见性与紧凑布局

- 媒体收件箱拆分为“选择/视图、目标/主操作、次级动作”三层：主操作栏在窄屏保持单行，次级操作和加载统计位于表格之后；详情面板在不足以同时容纳列表与 Inspector 时默认折叠，由 `MediaInboxCompactDetailsButton` 打开并堆叠到表格下方。
- `MediaInboxPageScrollViewer` 使用 `GscPageScrollViewer`，`MediaInboxGrid` 仍是 `Tag=FiniteViewport` 的共享 `MediaDataGrid`，代码按实际页面高度赋予有限 Height/MaxHeight。不要把整个 DataGrid 放回无限测量的外层 ScrollViewer，也不要删除页内滚动入口来追求假性首屏填满。
- RenderHarness 现将 measured PageHost/content 高度传给页面协调器，并在真实 `AcrylicProductionShellView` 内验证 1040×700、1100×720、1366×768 的 PageHost、滚动 extent、表格高度；`UiLayoutAnalyzer` 通过祖先裁剪交集检查表格和动作控件是否真的可见。`validate-source.py` 也已认识新的页面滚动契约。
- 当前证据：`render-qa OK`，双主题/多尺寸/resize/壳层几何通过；UI audit 154 个运行时快照，Fidelity 0、失败路由 0、媒体页无 HIGH；Playnite `342/404` 通过、62 跳过。截图见 [`docs/design/reviews/2026-09-07-ui3/`](../design/reviews/2026-09-07-ui3/)。未运行真实 Playnite 宿主，不能把离屏结果写成宿主验收。

## 2026-09-06 FLiNG 后台下载 403 修复

- 复查确认详情页正常、下载文件链接 403 与 Worker 请求特征不完整相符：原实现没有 Cookie 会话和详情页 Referer，且只发送 `GameSaveCenter/0.5` User-Agent。
- 下载源现在显式维护 `CookieContainer`，所有页面/文件请求使用浏览器风格 User-Agent、Accept 和 Accept-Language；下载前从 `release.CatalogId` 找到官方 `/trainer/` 页面并预热，会话 Cookie 与 Referer 一起用于文件请求。
- 仍保留 `EnsureFlingUri` 的 HTTPS/主域及子域约束、重定向后的最终地址校验和 2 GiB 大小限制；403 映射为 `FLING_DOWNLOAD_FORBIDDEN`，便于任务页区分站点拒绝与网络错误。
- 同时修复 `GameToolService` 下载失败时临时 `.download` 文件未进入 finally 的边界；现由下载到安全解压/绑定的完整流程统一清理。
- `FlingTrainerCatalogSourceTests.Download_UsesTrainerPageSessionAndReferer` 覆盖请求顺序、来源头、User-Agent 和文件落盘；定向 `12/12`，Release 全量 Worker `295/296`（1 跳过）。未取得真实 Worker 在线下载或 Cloudflare/验证码挑战证据。

## 2026-09-06 Luna 实现后 UI 复查（仅审阅）

- 基线 `0ac0e39`：抽查 V2 已实现代码与测试，新增 [UI3 可见优化任务包](UI_REVIEW_V3_2026-09-06.md)，不重新执行已完成的 V2 修复。
- 本轮实际离屏审图发现媒体紧凑按钮/表格视口不足；云队列明细后端未接到生产分页 UI；归类“上次批次”内存入口会被新预览替换，需要可找回历史。任务夹具空数值/范围是 FakeDashboardData 缺属性，不直接认定生产缺陷。
- RenderHarness OK 不等于用户可见区域验收，UI3-00 要补祖先裁剪交集与实际生产 Shell 几何；六张原始截图与报告纳入 `docs/design/reviews/2026-09-06-v3/`。
- 本轮仅文档，Release 0 warning/0 error、Core 65/65、Worker 294 通过/1 跳过、Playnite 341 通过/62 跳过、XAML 19/19、源码门禁通过；未安装/操作用户数据，真实宿主仍待验收。
> 本文件面向新的 AI/Codex 会话，目标是在几分钟内恢复项目状态，避免重复实现已完成的工作。

> 当前事实入口：先读 [`CURRENT_STATE.md`](CURRENT_STATE.md)。本文保留按阶段的历史约束和证据；若与当前事实入口或最新代码冲突，以 `CURRENT_STATE.md` 的覆盖说明为准，不要按旧条目恢复已撤销布局或外部 Demo 路径。

## 2026-09-06 V2-07 媒体多页累积成本与有界窗口

- `MediaPageAccumulator` 为媒体主列表、未归类收件箱和已忽略收件箱分别维护 ID 索引；首个游标页替换，后续页只对相同 ID 更新并追加新项，不再按已加载集合执行全量合并/替换。
- 每个缓存默认保留最近 2000 条。裁剪时保留当前选中媒体，`MediaLoadedSummary`/`MediaInboxLoadedSummary` 明确区分当前保留数、服务端总数和窗口上限；游标和总数继续由服务端分页状态驱动。
- `BatchObservableCollection.ApplyBatch` 将一页内的更新、追加和裁剪收敛成一次 Reset 通知，避免逐项触发 UI 重排；索引更新保持与集合位置一致。
- 回归覆盖 250 页 × 200 项的 5 万条输入、2000 条窗口、选中项保留，以及重叠 ID 更新不重复。隔离 Release 全量为 Core `65/65`、Worker `294/295`（1 跳过）、Playnite `341/403`（62 跳过）、XAML `19/19`，构建无警告/错误，源码/XAML/差异门禁通过。
- 未取得真实 Playnite 大库滚动回收、DPI、目标机帧率或长时内存证据；不得将有界集合回归写成真实宿主性能验收。

## 2026-09-06 V2-06 云队列全量摘要与独立分页

- `SqliteStateStore.CloudTransfers` 使用统一 CTE 合并 durable 新队列、legacy 重试表和游戏/媒体基础云状态；按 `transfer_key`（不区分大小写）以新队列优先去重，再由 SQL 聚合全量状态计数和最早重试时间。
- `CloudTransferStateService.GetStatusAsync` 保留无参数调用作为首页兼容入口，同时接受 `CloudTransferStatusRequestDto` 的页码、页大小、状态和类型过滤；摘要总数与分页明细查询相互独立，页大小服务端限制为 100，并返回已加载量和是否还有下一页。
- 旧备份重试表仍按原有错误分类显示认证需处理或下次尝试；基础游戏/媒体状态只在无更高优先级 durable 行时补入，避免新旧来源重复计数。
- 新增 1005 条记录、旧新同键、摘要最早重试、分页末页和失败过滤回归；隔离 Release 全量验证为 Core `65/65`、Worker `294/295`（1 跳过）、Playnite `339/401`（62 跳过）、XAML `19/19`，源码校验与差异检查通过。
- 未取得真实 Playnite 大库 UI 首屏、跨进程并发或真实宿主证据；后续处理 V2-07。

## 2026-09-06 V2-05 云端校验终态与代际保护

- `cloud_transfer_queue` 新增操作类型、操作 ID 和校验前快照字段。上传使用 `Upload` 代际，远端只读校验使用独立 `Verify` 代际；校验在等待全局闸门前就持久化为 `Verifying`，避免 durable 状态与实际操作不一致。
- 校验取消、工具启动/执行异常和失败结果均有明确收尾：可恢复时恢复校验前的 Uploaded/RemoteVerified/RetryScheduled 等快照，失败结果记录 `CheckFailed` 或 `AuthenticationRequired`；恢复失败也不会提升云端保证。投影到游戏行的状态使用尽力路径。
- 校验结果以操作 ID CAS 写回；更晚的上传代际接管后，旧校验不会覆盖队列。Worker 重启只恢复 Upload 型 Pending/Transferring；Verify 型 Verifying 恢复快照或 `CheckFailed`，绝不因校验恢复而排入上传。
- 新增五项云端校验行为回归及旧队列迁移覆盖；隔离 Release 全量构建 0 warning/0 error，Core `65/65`，Worker `293/294`（1 跳过），Playnite `339/401`（62 跳过），XAML `19/19`，源码校验与差异检查通过。
- 真实 Playnite、真实云端凭据/断网、硬杀和跨进程并发证据仍未取得；不要把这些 Worker/SQLite 回归写成真实宿主验收。

## 2026-09-06 V2-01 媒体归类提交与恢复协调

- `media_classification_operations` 记录归类/撤销的操作意图、源/目标路径、哈希、原始文件标记和目标预存状态；媒体行、批次条目和账本状态在 SQLite 同一事务内从 `Moved` 提交为 `Committed`。
- `WorkerInitializationService` 在任务/云队列恢复前调用 `MediaSyncService.RecoverPendingClassificationOperationsAsync`。启动时能确认业务已提交则补齐账本，未提交且状态仍匹配则按账本恢复文件并标记 `Aborted`，无法唯一判断则保留 `RecoveryRequired`。
- 审计是提交后的尽力写入，失败只产生结果告警和 Worker 日志，不回滚已提交业务；取消回滚和文件协调使用独立非取消令牌。原始媒体作为源时只清理本次新建的归档副本。
- 新增 `MediaSyncServiceTests` 的 SQLite 触发器/取消回归：批次条目提交失败后启动恢复可恢复 Inbox 副本，审计失败仍保留 Assigned 副本，取消后文件和数据库回到 Inbox。V2-01 验证为 Release 0 warning/0 error、Core `65/65`、Worker `278/279`（1 跳过）、Playnite `339/401`（62 跳过）、XAML `19/19`、源码门禁与差异检查通过。
- 这仍不是真实 Playnite、用户媒体、断电或跨进程并发证据；后续 V2-02 起按复查包顺序逐项实施。

## 2026-09-06 V2-02 健康巡检调度与计划写入

- `HealthInspectionService` 后台循环在 `_runGate` 争用失败时等待 250ms 后重试，外围 `Get/SyncPlan/RunOne` 异常按 1s 退避隔离，应用停止时不进行无界重试。
- 计划和执行状态采用不同 SQLite 写入契约：`UpdateHealthInspectionPlanAsync` 只更新计划字段，`SaveHealthInspectionExecutionStateAsync` 只更新结果/游标字段；`CompleteAsync` 读取最新计划后设置下一次时间，避免并发设置修改被旧 DTO 整行覆盖。
- 定向回归用可控手动巡检闸门证明 700ms 内后台争锁次数受限，并证明执行写入不会覆盖已更新的启用/间隔/过期/预算；隔离 Release 全量验证为 Core `65/65`、Worker `280/281`（1 跳过）、Playnite `339/401`（62 跳过）、XAML `19/19`。
- 真实宿主长时调度与目标机资源压力仍需人工复核。

## 2026-09-06 V2-03 健康巡检候选公平与恢复游标

- `HealthInspectionService` 先按稳定的 `PlayniteId`、`CreatedUtc`、`BackupId` 顺序建立候选集，再按完整游戏/备份复合游标轮转；启动时若存在持久化 in-flight 游标，优先恢复该确切候选。
- 候选身份在检查会话、游戏锁和归档前通过执行状态契约落盘。状态写失败会停止本轮且不读取归档、不写入 Failed readiness/finding；审计和完成状态仍走尽力记录路径。
- `health_inspection_deferred_candidates` 为被运行中游戏或操作锁推迟的版本记录独立下次尝试时间，允许本轮转向其他游戏；无过期项时返回 `UpToDate`，不重复解包。
- 新增候选提前落盘、状态写失败、公平推迟、新鲜集合和 in-flight 恢复回归；定向健康巡检 `11/11`，隔离 Release 全量结果为 Core `65/65`、Worker `285/286`（1 跳过）、Playnite `339/401`（62 跳过）、XAML `19/19`。
- 本阶段仍未取得真实 Playnite、硬杀/断电、跨进程并发和长时调度证据；后续按复查包处理 V2-04～V2-07。

## 2026-09-06 V2-04 IPC 请求身份与重放指纹

- `ipc_request_ledger` 现在持久化 `protocol_version` 和规范化 JSON 负载 SHA-256 指纹。对象属性按序规范化、数组顺序保持语义；相同请求属性顺序变化可以安全重放。
- `ClaimIpcRequestAsync` 对已有 ID 同时核对 type、协议版本和 payload 指纹；不一致或旧行缺少指纹时返回冲突标记，`NamedPipeServerService` 返回 `REQUEST_ID_CONFLICT`，不会执行或重放旧结果。受保护写请求没有 ID 时返回 `REQUEST_ID_REQUIRED`。
- 旧表通过 `EnsureColumnAsync` 补齐新列；完成行 7 天后按每轮最多 256 行有界清理，中断行保留 30 天以便核对。Named Pipe 服务每小时执行一次只清理终态的维护任务，启动恢复仍单独负责将本进程外的 in-flight 标记为 Interrupted。
- 新增 RequestId 内容冲突、属性顺序、旧账本、迁移和保留策略回归；隔离 Release 全量结果为 Core `65/65`、Worker `288/289`（1 跳过）、Playnite `339/401`（62 跳过）、XAML `19/19`。
- 未取得真实 Playnite、跨版本旧 Worker/插件组合和硬杀端到端重放证据；后续处理 V2-05～V2-07。

## 2026-09-06 完成后复查（仅文档）

- 新增 [FOLLOWUP_REVIEW_2026-09-06.md](FOLLOWUP_REVIEW_2026-09-06.md)，基线 `8018cee`。确认上一轮主体已实现，另列 7 项源码边界和 3 项扩展，未修改生产代码。
- V2-01 媒体归类/撤销数据库提交后异常可能只补偿文件；V2-02/03 巡检忙等、外围异常隔离、游标持久化与公平性；V2-04 RequestId 内容指纹；V2-05/06 云端校验终态与完整摘要；V2-07 媒体多页累积成本。后续应先写行为复现测试，不能将审阅认定为已修复。
- 本轮 Release 0 warning/0 error，Core 65/65、Worker 275 通过/1 跳过、Playnite 339 通过/62 跳过，XAML 19/19、源码门禁通过。没有复跑真实宿主、规模矩阵或新增故障注入；Worker 重启和 5 项 IPC 行为测试本次跳过。

## 2026-09-05 UI-134 任务中心搜索文字垂直裁切修复

- `GscWpfUiTextBoxTemplate` 的 TextBox Padding 由原生 `PART_ContentHost` 消费，外层 `Chrome` 必须保持无 Padding；重复应用 `30,7,38,7` 会把 36 DIP 搜索框的文字 viewport 压到约 5 DIP。
- 本次只移除生产模板外层重复 Padding，任务搜索框尺寸、左右输入区、Foreground、Binding、清除按钮和键盘语义均未变；新增 STA 回归锁定高度 `36 DIP`、viewport 不小于文字 extent 和非透明前景。
- 自动证据：Playnite 定向 Release 测试 `126/165` 通过、`39` 跳过，源码/XAML/WPF 门禁通过；RenderHarness 双主题、多尺寸及 resize `render-qa OK`，修复后 viewport `19/18 DIP`。真实 Playnite 仍需人工复核。

## 2026-09-05 E01 规模性能基线

- `scripts/e01-scale-baseline.ps1` 是显式触发的隔离规模入口：默认不扩大普通测试夹具，`-Profile full` 使用 2,000 游戏/20,000 备份/10,000 任务/5,000 媒体/500 工具，`-Profile stress` 使用 10,000 游戏/20,000 备份/10,000 任务/50,000 媒体/500 工具；两档均写出 `worker-scale.json` 和 `baseline.md`。
- 2026-09-05 两档 Release 基线均通过：full seed/模拟 `105999/427 ms`，游戏首查/热查 `17/14 ms`、任务页首查/热查/下一页 `92/3/3 ms`、媒体页 `3/1/0 ms`；stress seed/模拟 `242832/1529 ms`，游戏首查/热查 `83/84 ms`、任务页 `93/3/2 ms`、媒体页 `4/0/1 ms`。两档还记录了任务/媒体搜索和查询/模拟分配量，托管内存保留增长、句柄/线程增长、订阅者和临时文件残留均受断言约束。
- 这是隔离 Worker/SQLite 数据量、查询首查/热查、搜索/分页、事件/原子写/操作锁模拟和资源增长的基线，不是 Playnite 宿主的冷/热首屏、UI 分配或帧间隔证据；真实宿主大库、DPI 和目标机目录仍按 `MANUAL QA REQUIRED` 处理。

## 2026-09-05 E01 行为证据矩阵与独立 Worker 重启验证

- `scripts/e01-behavior-matrix.ps1` 是自动证据分层入口：输出根默认在 `.tmp/e01-behavior`，业务、IPC、WPF/STA、故障/Soak 每个测试类单独记录；`-IncludeRender` 才运行 RenderHarness，真实 Playnite 始终写入 `MANUAL QA REQUIRED`。故障/Soak 组包含 `WorkerProcessRestartTests`。
- 受控真实 Windows 矩阵结果为业务 `44/44`、IPC `22/22`、WPF/STA `45/45`、故障/Soak `4/4`，整体进程退出码为 0；完整 Release 套件为 Core `65/65`、Worker `276/276`、Playnite `343/400`（57 项跳过）。
- `WorkerProcessRestartTests` 已证明真实 Worker 在随机管道和独立 Mutex 下硬停止后，第二个进程能用同一临时 SQLite 启动，并将未完成 Backup 标记为 `WORKER_RESTARTED_RETRYABLE`。为使该证据成立，Worker 补注册 `ITaskStatusStore`，管道名支持受校验的隔离覆盖而生产默认常量不变；客户端只对破坏性请求报告“可能已提交”。
- 已对当前用户 Worker 执行一次只读 `system.ping` 并收到成功响应，证明真实 Named Pipe 连通；没有停止或写入用户 Worker。E01 仍不等于 Playnite 宿主验收，双选择器、主题/DPI、睡眠唤醒与退出重启等真实宿主项仍需隔离环境复核。

## 2026-09-05 E02 当前事实入口与模块边界文档治理

- `docs/ai/CURRENT_STATE.md` 是新会话的短事实入口：当前版本 `0.6.73`，生产可见路径为 `DashboardView` 承载的 `AcrylicProductionShellView`，工作区和 Worker/Contracts/SQLite 入口均指向仓库实际文件。`GameSaveCenter.AcrylicFork/src/GameSaveCenter.Playnite/Design/` 当前缺失，不能作为编译、测试或逐像素证据输入；生产主题入口是 `Themes/AcrylicProductionResources.xaml` 及其 `DesignTokens`/`WpfUiProduction`/`Redesign` 资源。
- 当前有效的 UI/行为例外和保护边界在短入口集中说明：游戏选择器、既有滚动条、真实数据与安全确认语义保留；筛选使用 OneWay 显示 + `UiFilterSelection.Synchronize` + `DropDownClosed` 写回；历史记忆只作阶段证据，冲突时短入口与最新代码覆盖。
- `AGENTS.md` 已将 CURRENT_STATE 放在启动协议第一项；根 `docs/PROJECT_MEMORY.md` 与 `DEVELOPMENT_HANDOFF.md` 通过链接声明自身为历史归档。源码门禁会检查短入口中的版本、资源、缺失 Demo 和 `MANUAL QA REQUIRED` 标记。
- E02 阶段验证：Release 0 warning/0 error；Core `65/65`、Worker `275/275`、Playnite `338/400`（62 跳过）、XAML `19/19`，源码校验和 `git diff --check` 通过。真实宿主和大库证据仍未被文档治理替代。

## 2026-09-05 F03 媒体归类建议与可撤销批次

- `MediaSyncService` 的归类预览读取 Inbox 媒体、启用来源规则、会话区间和进程映射；规则目录/进程映射可给 High，时间范围或唯一文件名候选给 Medium，多个候选、未知时间或冲突给 Low 且不提供可应用目标。预览默认最多 200 项，批次有效期 30 分钟。
- `media_classification_batches` / `media_classification_batch_items` 保存原始媒体快照（状态、PlayniteId、版本、路径、云端状态、不可变元数据）以及建议/应用后快照。应用默认 `HighConfidenceOnly=true`，只处理仍匹配原快照的条目；手工覆盖或并发修改返回 Conflict，不覆盖用户选择。
- 归类移动的是归档目录中的副本，使用路径安全校验和同盘移动/复制回退；原始媒体和真实存档不删除、不移动。撤销从 SQLite 批次恢复原路径与元数据，只接受当前应用快照和 `Pending` 云端状态，批次可在 Worker 重启或预览过期后撤销；部分失败会保留逐项结果和审计。
- MediaCenter Inspector 的三个命令是预览、应用高置信建议、撤销上次批次；预览为空时列表折叠，不挤占空 Inspector 的首屏高度，非空列表保留 `FiniteViewport`、Recycling 虚拟化和内部滚动。手工 Assign/Ignore/Restore 入口不变。
- F03 阶段验证：Release 0 warning/0 error；Core `65/65`、Worker `275/275`、Playnite `338/400`（62 跳过）、XAML `19/19`、源码校验和 WPF 质量审计通过，RenderHarness 为 `render-qa OK`。未取得真实 Playnite 宿主、真实媒体目录、50,000 媒体压力和多进程并发证据。

## 2026-09-05 F02 云端队列可见与传输策略

- 备份与媒体复制统一写入 SQLite `cloud_transfer_queue`，键为 `Backup/Media + PlayniteId`；旧 `cloud_retry_queue` 继续兼容。Worker 启动恢复未完成的 `Pending/Transferring`，自动扫描同时覆盖旧备份队列和新媒体/备份队列，同一游戏同一类型只保留一条状态。
- `CloudTransferStateService` 负责状态、次数、原因、下次时间及远端保证等级；`Uploaded` 仅表示 copy 命令成功，`RemoteVerified` 才表示 `rclone check` 成功。认证失败为 `AuthenticationRequired` 且不进入自动到期扫描，校验失败只记录状态，不删改本地内容。
- 设置 `CloudUploadQueuePaused`、`CloudUploadAllowedStartMinute/EndMinute` 默认关闭暂停、全天允许；允许时段用本地分钟并支持跨午夜，后台队列以外时段延后，手动入口不受该策略限制。没有实现带宽上限/运行中游戏限速，因为当前没有锁定并验证的 Rclone 参数；安全命令边界仍是 `copy/check/lsf/cat/version`。
- F02 阶段验证：Release 0 warning/0 error；Core `65/65`、Worker `272/272`、Playnite `338/400`（62 跳过）、XAML `19/19`、源码门禁和 RenderHarness `render-qa OK`。目标机真实凭据、断网、Worker 硬重启和云端 check 仍需人工复核。

## 2026-09-05 F01 备份健康巡检与隔离恢复演练

- `HealthInspectionService` 是 Worker HostedService，计划/游标/最近结果位于 SQLite `health_inspection_state` 单例行；每轮只选一个新或超过有效期的备份，按 PlayniteId/创建时间/BackupId 稳定排序，游戏运行或 `GameOperationLock` 忙时只记录 `Deferred`，不读取归档。
- `LastStartedUtc` 在候选读取前落盘，`LastCompletedUtc` 只在终态落盘；Worker 非正常退出后下次启动识别 in-flight 状态并从同一游标重试。成功验证时间只在 `Ready` 更新，`Warning`/`Corrupted`/`Failed`/`Unsupported` 写入稳定的 `HEALTH_INSPECTION_FAILED` 关注项。
- `RestoreOrchestrator` 在真实恢复开始前读取目标版本的最近校验结果；已知 `Corrupted`/`Failed` 直接以 `RESTORE_READINESS_FAILED` 拦截，预览和未建立校验记录的旧/非 ZIP 版本保持兼容，不会因巡检入口自动执行真实恢复。
- 健康巡检复用 `RestoreReadinessService` 的 ZIP/Manifest/哈希/安全路径逻辑，隔离目录位于 `WorkerOptions.RestoreReadinessDirectory`，只在实际检查时创建；空间不足在解包前失败，清理状态明确为 `Pending`、`Cleaned` 或 `Retained`。这不代表真实进度或真实恢复已经演练。
- 设置 DTO/页面新增启用、间隔（15–10080 分钟）和重新验证有效期（1–3650 天）；最大单次预算由 Worker 固定校验范围（30–1800 秒）控制。维护页和报告展示“状态/最近成功/下次计划”，手动入口只执行隔离校验。
- F01 自动证据：Release 0 warning/0 error；Core `65/65`、Worker `264/264`、Playnite `338/400`（62 跳过）、XAML `19/19`、源码门禁和 RenderHarness `render-qa OK`。真实 Playnite、断电/硬杀时序、磁盘耗尽、分卷归档和长时资源压力仍待人工复核。

## 2026-09-05 U02 侧栏动画成本与快速操作终态

- `AcrylicProductionShellView.OnSidebarCollapseClick` 不再拒绝动画期间的后续点击；每次从当前 `SidebarColumn.ActualWidth` 接续到最新 72/270 DIP 目标。`sidebarTransitionGeneration` 使取消或卸载后的旧 `Completed` 回调失效，避免旧动画把新目标覆盖；无动画分支会清除宽度/透明度/位移动画后一次性落终态。
- `tests/GameSaveCenter.RenderHarness/Program.cs` 的侧栏探针统计 Measure/Arrange 次数与耗时、LayoutUpdated、Rendering 帧间隔，并对比 `MotionEnabledProvider=false` 的原子切换。独立 2000×1100 夹具单次 Arrange 约 `9.9ms`、快速往返最终宽度 `270`，原子切换 Arrange 约 `4.0ms`；因此保留当前过渡，暂不做大范围视觉简化。
- 阶段验证：Release 0 warning/0 error；Core `65/65`、Worker `260/260`、Playnite `338/400`（62 跳过）、XAML `19/19`，源码门禁和完整 RenderHarness `render-qa OK` 通过。探针不是真实 Playnite 帧率证据，长列表真机压力、DPI、高对比度和宿主卸载仍需人工复核。

## 2026-09-05 U01 任务页视口与状态试点

- `DashboardViewModel.TaskPageState.cs` 将任务页请求生命周期和展示状态独立出来：`Loading`、`Empty`、`FilterEmpty`、`Error`/`ErrorWithData`、`Ready`；刷新失败或刷新中的已有数据不被清空，并通过 `TaskPageLastUpdatedDisplay` 与 `TaskPageStatusSummary` 暴露旧数据时间和恢复入口。
- TaskCenter 在 1040×700、1366×768 等尺寸下保持主列表+Inspector 和窄屏紧凑详情路径；短高度将摘要 `MinHeight`/Padding 收紧，任务表最小高度保持 `236` DIP，列表内部滚动与 Recycling 虚拟化保留。状态层提供清除筛选、重试和无旧数据错误详情。
- 阶段验证：Release 0 warning/0 error；Core `65/65`、Worker `260/260`、Playnite `337/399`（62 跳过）、XAML `19/19`，源码门禁和 RenderHarness `render-qa OK` 通过。离屏夹具不是真实 Playnite 宿主证据；高对比度、实际 DPI、长历史性能和故障时序仍需人工复核。

## 2026-09-05 U03 游戏目录来源与新鲜度诊断

- `GameDescriptorDto` 新增 `PlayniteIsInstalled` 与 `InstallStateSource`；`PlayniteGameAdapter` 按 Playnite 原始标志、有效安装目录、有效本地 Play action 的顺序记录来源，`GameMatchInput` 不包含安装状态，安装状态变化不会使已有匹配失效。
- `GameCatalogService` 新增 descriptor-only 持久化、按 ID 诊断和单项匹配重试；描述同步不调用 Ludusavi，重试只读取并处理该 Worker 游戏。`SqliteStateStore.games.descriptor_synced_utc` 独立于匹配尝试时间，旧库通过 `EnsureColumnAsync` 升级。
- `GameDiscoveryDiagnosticDto` 只返回存在性、安装信号、匹配/备份摘要、时间和当前筛选条件，不包含完整私人路径；Playnite 插件在 Worker 不可用时仍返回本地来源事实。来源缺失只展示诊断状态，不能触发删除历史或备份。
- `GamePickerViewModel.GetFilterExclusionReasons` 与真实 `FilterItem` 共用判断逻辑；维护页提供按 ID 诊断、清除筛选、单项描述同步、单项匹配重试，普通刷新不改变现有全库同步策略。
- 阶段验证：Release 0 warning/0 error；Core `65/65`、Worker `260/260`、Playnite `335/397`（62 跳过）、XAML `19/19`、源码校验和 `git diff --check` 通过。真实宿主目标游戏、Worker 离线/重启及来源移除后的 UI 仍需人工复核。

## 2026-09-05 R07 IPC 取消与请求结果追踪

- `WorkerIpcClient`/插件请求入口现在接受调用者 `CancellationToken`，并联结插件生命周期；连接、写入和读取均使用可取消的 `Task.WhenAny` 竞态。取消回调只负责发出完成信号并把管道关闭排到线程池，避免在 `CancellationTokenSource.Cancel()` 中同步 Dispose 原生管道。
- `DashboardViewModel` 的选中详情、当前媒体分页、媒体收件箱分页会取消旧请求；generation/选中 ID 仍是响应回写的第二道边界。后台失败汇报忽略 `OperationCanceledException`，用户取消与 Playnite 退出不会弹失败提示。
- 破坏性 IPC 类型由 Contracts 集中分类；客户端超时/断管且请求可能已接收时用同一 RequestId 做有限重放，遇到 `RequestInProgress` 会短暂轮询。Worker `ipc_request_ledger` 保留 7 天，Completed 可重放，InProgress 重启恢复为 Interrupted；任务表新增 RequestId，Backup/Restore 的 TaskStatusDto 与请求关联，`TaskQueryDto.RequestId` 可精确查询。
- net462 继续使用 `NamedPipeClientStream.Connect(int)` 在线程池执行，不依赖现代 Stream 取消重载。真实 Named Pipe 行为套件有能力探测：当前沙箱客户端连接被系统拒绝时 5 项跳过；完整 Windows/Playnite 运行时应执行，不得把跳过当作真实行为验收。
- 阶段验证：Release 构建 0 warning/0 error；Core `65/65`、Worker `258/258`、Playnite `333/395`（62 跳过）、XAML `19/19`、源码校验通过。真实宿主、Worker 重启和长任务断线恢复仍需人工复核。

## 2026-09-05 R06 媒体按需分页

- Contracts 新增 `MediaQueryDto`/`MediaPageDto`；Worker 新增三类分页 IPC，按固定 `classification_state` 查询，支持 `PlayniteId`、`Kind`、`FavoriteOnly`、关键词和稳定 `(captured_utc, media_id)` 游标。`TotalCount` 不受当前游标影响，旧列表消息仍保留给兼容客户端。
- SQLite 新增 `ix_media_game_state_capture` 与 `ix_media_state_capture`，分页数据按 `captured_utc DESC, media_id DESC` 返回；查询计划测试证明当前游戏/收件箱路径命中新索引。搜索匹配路径、备注、媒体 ID 及可识别来源显示名，LIKE 通配符已转义。
- Dashboard 当前游戏首批 200 条，搜索/类型/收藏变化服务端重取首屏；待归类与已忽略集合各自保存游标、总数和 HasMore，模式切换、刷新、批量归类期间的旧页按代际丢弃。`MediaLoadedSummary`/`MediaInboxLoadedSummary` 显示已加载/总数，“加载更多”不改变现有多选、批量操作和 Item/Recycling 虚拟化。
- R06 验证：Release 0 warning/0 error；Core `65/65`、Worker `255/255`、Playnite `333/390`（57 跳过）、XAML `19/19`、源码校验和 `git diff --check` 通过。未运行真实 Playnite、50,000 项性能实验、4 MiB 消息边界和长时 UI 帧率测量。

## 2026-09-05 R05 游戏筛选下拉框 STA 行为验证

- `GamePickerFilterBehaviorTests` 在真实 WPF STA 线程中把筛选 ComboBox 挂到窗口，验证程序化关闭状态选中不会触发提交，以及打开→改选→关闭会触发 `DropDownClosed` 并得到最终选项。
- R05 未改生产行为：两套选择器仍使用 OneWay 显示绑定、`UiFilterSelection.Synchronize` 恢复程序化状态、`DropDownClosed` 作为唯一用户写回入口。脱离可视宿主的 ComboBox 不会自然触发关闭事件，测试夹具不可省略。
- 目前自动证据仅覆盖 WPF 控件基本提交时序；真实宿主 UI Automation Selection、字符搜索、Esc、双实例竞态、主题/DPI 仍待人工验证。

## 2026-09-05 R04 任务统计口径与完整历史查询

- Worker 新增 `TaskQueryDto`/`TaskPageDto`/`TaskSummaryDto` 查询契约和 `tasks.page` IPC；查询支持状态（含组合状态）、游戏、类型、关键词、创建时间半开区间，按 `created_utc DESC, task_id DESC` 加不透明游标分页，稳定覆盖同一创建时间的任务。
- `GetTaskSummaryAsync` 独立聚合全量匹配结果，`DashboardService` 不再用最近 50 条推导云端等待数；今日成功使用本地日边界转换后的 UTC 半开区间。`finished_utc,state` 索引已由查询计划测试证明用于完成数统计。
- TaskCenter 的任务总数、运行中和今日完成不再混用加载窗口；支持最近/全部历史、时间范围筛选和“加载更多”，列表底部明确已加载/总数。选择历史或时间范围后采用服务端分页，重试计数明确为当前已加载结果；原有命令、绑定、虚拟化和任务滚动保留。
- 阶段验证：Release 0 warning/0 error；Core `65/65`、Worker `251/251`、Playnite `331/388`（57 跳过）、XAML `19/19`、源码校验和 `git diff --check` 通过。未运行真实 Playnite 宿主、10,000 条任务性能实验和跨午夜人工验收。

## 2026-09-05 R03 保留清理隔离账本与启动恢复

- 新增 `retention_quarantine_batches` / `retention_quarantine_entries` SQLite 表和 `RetentionQuarantineState` 状态机；每个条目持久化批次、游戏/备份 ID、原路径、隔离路径、文件字节和最后错误，应用流程依次记录 `Planned` → `Moved` → `IndexRemoved` → `Deleted`，不确定时进入 `RecoveryRequired`。
- `WorkerInitializationService` 在常规任务恢复前调用 `RecoverPendingQuarantineAsync`。仅 `IndexRemoved` 且精确 `.pending` 文件仍在当前备份根、大小与账本一致、原路径不存在时自动删除；`Planned`/`Moved` 优先把文件恢复回原路径；原路径冲突、路径越界、大小变化和未知隔离文件均保持不动。
- Retention 预览、应用结果和维护健康报告分别展示 pending 条目数、实际隔离占用、人工恢复数、移入隔离字节和真实释放字节；`FreedBytes` 只在物理删除成功后增加。维护页沿用现有 `RetentionSimulation.Summary` 显示这些状态，因此本阶段未改 XAML。
- 新增 `RetentionQuarantineRecoveryTests` 并补充成功清理的 `Deleted` 账本断言。验证：Worker Debug/Release `247/247`、Core `65/65`、Playnite `331/388`（57 跳过），Release 构建 0 warning/0 error，`validate-source.py` 和 `git diff --check` 通过；未运行真实 Worker 崩溃时序或 Playnite 宿主，阶段提交待完成。

## 2026-09-05 R02 全局保留清理候选身份与互斥

- `RetentionSimulationPreviewDto` 新增 Worker 生成的 `PreviewId`；Worker 仅在服务端保存的预览快照中查找完整候选列表，执行开始即消费句柄，重复提交、重启后旧句柄和过期句柄都会被拒绝。旧的候选数量/体积字段保留为兼容显示字段，不再作为执行授权依据。
- 预览快照指纹包含游戏/备份 ID、规范化归档路径、索引大小、文件数、创建时间、锁定/PreRestore/健康保护状态、策略指纹，以及归档文件长度和最后写入时间；执行前先做全局校验，逐游戏取得共享 `GameOperationLock` 后再次重读并校验，状态变化的游戏只跳过并要求刷新预览，不尝试替代候选。
- 保留清理、备份元数据/锁定状态、游戏策略更新、恢复可恢复性写入和仓库索引重建统一进入现有按游戏互斥通道；归档根路径、ZIP 后缀和 reparse point/junction 边界均在移动前复核。结果新增忙碌/状态变化计数，真实释放字节继续只在隔离文件删除后累计。
- 新增/补强替换候选、同路径同大小文件替换、策略变化、重复提交和共享锁占用回归。验证：Worker `244/244`、Release 0 warning/0 error、`validate-source.py`、`git diff --check` 通过；未运行真实 Playnite/Rclone，后续按 R03 继续。

## 2026-09-05 R01 任务终态写入失败不再锁死同一游戏

- `TaskCoordinator` 将任务状态写入依赖收口为 `ITaskStatusStore`；终态持久化放在独立保护块中，失败时记录 TaskId、GameId、TaskType、原始业务终态和异常，但不阻断 `_taskTokens` 清理或同游戏信号量释放。
- `PersistAndPublishAsync` 仍先成功写入 SQLite 再发布终态事件，因此未成功落盘的 `Succeeded` 不会进入 UI 任务事件流；业务返回值保留原始成功/失败/取消结果，便于区分业务结果与持久化故障。
- 新增 `TaskCoordinatorFailureTests`，对成功、业务失败、取消三条终态路径注入最后一次写入失败，验证后续同游戏任务和其他游戏任务均能在有界时间内完成，取消令牌已清理且没有发布未落盘终态事件。
- 验证：Worker Debug 构建 0 warning/0 error；Worker `238/238` 通过；`git diff --check` 通过。未运行真实 Worker 故障注入或 Playnite 宿主，后续按 R02 继续。

## 2026-09-05 全项目完善方案（仅审阅，尚未实施）

- 用户要求审阅功能、UI、可扩展性、健壮性、稳定性与性能，并提供便于其他 AI 实施的方案；已交付 [IMPROVEMENT_ROADMAP_2026-09-05.md](IMPROVEMENT_ROADMAP_2026-09-05.md)，含 15 个任务的代码依据、步骤、依赖和验收条件，基线 `d5fd494` / 0.6.73。
- 优先候选：TaskCoordinator 终态持久化异常会跳过锁释放；全局保留清理仅比较数量/体积且未参与游戏操作锁；隔离区缺持久化恢复映射；任务“今日完成”没有日期过滤；媒体传输分页仍一次聚合最多 5000 条才显示。它们是本轮源码审阅发现，不是已实施修复，也不是全部已真机复现。
- 游戏筛选 DropDownClosed 的键盘/Automation 行为需要 STA 复现；侧栏 GridLengthAnimation 成本需要实测，不能直接当成已测性能故障。当前工作区缺规则引用的 AcrylicFork Design 目录，未声称比对 Demo 像素。
- 文档交付基线验证：Release 0 warning/0 error，Core 65/65、Worker 235/235、Playnite 331 通过/57 跳过，XAML 19/19、源码验证通过。未运行本轮真实宿主/渲染/故障注入；没有改应用代码或升级版本。

## 2026-09-04 Worker 描述缓存安装状态陈旧

- 用户实测 0.6.72 后仍确认：“全部、已匹配、有备份等都有死亡空间，但已安装没有”；这排除了只修 WPF ComboBox 写回竞态的解释。
- 真正根因在 `GameCatalogService.UpsertAndMatchAsync`：`GameMatchInput.CreateHash` 有意排除 `IsInstalled`，但旧逻辑把“匹配输入未变化”错误当成“描述无需持久化”，导致 SQLite `descriptor_json` 一直保留旧的 `IsInstalled=false`。Dashboard 的“已安装”筛选读取这个持久化描述，因此目标条目被过滤。
- 0.6.73 将 `descriptorsToPersist` 与 `pending` 匹配队列分离；描述全字段变化（安装状态、安装目录、启动动作、标签、进程名、最近游玩等）会更新 SQLite，但只有匹配输入变化或到期重试才进入 Ludusavi。这样保持已有匹配，又让安装筛选使用最新状态。
- 新增 `GameCatalogPersistenceTests.InstallStateChangePersistsWithoutInvalidatingExistingMatch`，锁定“死亡空间已匹配后 false→true”场景；`GameMatchInput` 注释明确记录“匹配输入与描述持久化是两个契约”。
- 验证：Release 0 warning/0 error；Core `65/65`、Worker `235/235`、Playnite `331/388`（57 跳过）；XAML `19/19`、源码验证、WPF 静态检查通过；0.6.73 已打包、安装并由 Playnite 日志确认 `GameSaveCenter 0.6.73.0 loaded`。本机 Playnite 仍只有 3 个样本游戏，不能代替用户目标机器复核。

## 2026-09-03 游戏选择器用户选择写回竞态补强

- 用户进一步确认目标游戏行信息显示“已安装”，但“已安装”筛选搜索不到；这排除了单纯安装状态缺失，问题仍是筛选框显示值与共享 `GamePickerViewModel.StatusFilter` 在 WPF 初始化/集合刷新期间发生抢写。
- 0.6.72 将两套选择器的筛选写回从 `SelectionChanged` 改为 `DropDownClosed`，程序化选中、绑定刷新和 `ItemsSource` 重建不再被当成用户输入；保留 OneWay 显示绑定和 `UiFilterSelection.Synchronize`。
- 新增精确回归：`IsInstalled=true` 的“死亡空间”在搜索词为“死亡空间”、状态为“已安装”时 `FilteredCount=1`，且显示状态为“已安装”。
- 本阶段验证完成：Release 0 warning/0 error；Core `65/65`、Worker `234/234`、Playnite `331/388`（57 跳过）；XAML `19/19`、源码门禁、WPF 静态审查和 Render QA 通过；0.6.72 包已安装到本机 Playnite，`extensions.log` 已记录 `GameSaveCenter 0.6.72.0 loaded`。本机 Playnite 只有 3 条样本数据，不包含用户目标游戏，不能替代目标机器复核。

## 2026-09-03 游戏选择器双向绑定残留与安装判定补强

- GSC-130 的“加载后同步”仍不能阻止 WPF 在两个选择器副本的 ItemsSource 重建期间把旧值写回共享状态；本轮将生产 Shell 与兼容 Dashboard 的状态、平台、排序 ComboBox 改为 `Mode=OneWay`，只在 `SelectionChanged` 收到实际字符串选项时写入 `GamePickerViewModel`，并移除平台的静态 `SelectedIndex="0"`。
- `PlayniteGameAdapter` 现在除 `IsInstalled` 和安装目录外，还把存在的本地 Play action/working directory 作为只读安装信号；Steam URI 等非文件路径不会被误判，安装目录枚举也使用展开后的路径。
- 版本提升到 `0.6.71`，用于强制 Playnite 替换之前复用相同程序集版本号的旧 DLL。交付前必须核对 Playnite 日志为 `0.6.71.0`，不能只看 zip 文件时间。
- 本阶段已完成 Release 编译/回归、XAML/源码/WPF 门禁、Render QA、打包和本机安装；Playnite 日志已记录 `GameSaveCenter 0.6.71.0 loaded`，安装 DLL 与打包暂存 DLL 哈希一致。当前环境的 Playnite 样本只有 3 个游戏，未包含用户截图中的“死亡空间”，因此不能伪称已经在真实目标游戏上完成宿主 UI 复核。

## 2026-09-03 游戏选择器合法过期选中值收口

- 仅移除状态/排序 ComboBox 的静态 `SelectedIndex="0"` 仍不足以覆盖 WPF 初始化顺序：生产 Shell 与隐藏兼容 Dashboard 可能各自保留一个“合法但过期”的选中项，造成界面显示“全部”而 `GamePicker.StatusFilter` 仍为“已安装”。
- `UiFilterSelection.Synchronize` 现在在 Loaded 和平台选项重建后的恢复点，以共享 ViewModel 值为准同步状态、平台和排序三个 ComboBox；用户选择仍先写入共享 ViewModel，因此不会被恢复逻辑改回默认项。`RestoreDefault` 保留给只应修复空选择的其他场景。
- 回归覆盖新增有效过期选中值修复测试，并保留“全部包含未安装但已匹配/有备份”测试。GSC-130 自动证据：Core `65/65`、Worker `234/234`、Playnite `329/386`（57 跳过）、Release 0 warning/0 error、源码/XAML/WPF 门禁和 Render QA 通过。
- 本轮重新安装了当前 Release 到本机 Playnite，安装 DLL 与隔离构建 SHA256 一致；真实宿主审计已启动 Playnite，但 UI Automation 未找到侧栏入口，只产生受控证据，不能替代第二台机器的实际筛选复核。

## 2026-09-03 设置页持久化选择状态收口

- `GameSaveCenterSettingsView.xaml` 的备份格式、压缩方式和主题模式不再设置局部静态 `SelectedIndex="0"`；三项均由持久化 `SelectedValue` 的 `Mode=TwoWay`、`UpdateSourceTrigger=PropertyChanged` 绑定驱动，避免初始化时覆盖 `GameSaveCenterSettings` 已恢复的值。
- 这三项补充了中文 ToolTip 和 `AutomationProperties.Name`；不要为了视觉默认值把静态首项加回设置型 ComboBox。动态集合重建型筛选器可按各自契约保留恢复逻辑。
- GSC-129 自动证据：Release 0 warning/0 error、Core `65/65`、Worker `234/234`、Playnite `328/385`（57 跳过）和源码校验通过；真实 Playnite 重启/主题/设置导入仍需人工复核。

## 2026-09-03 设备冲突状态与媒体筛选绑定收口

- `MaintenanceView` 的设备决策 ComboBox 和 `MediaCenterView` 的媒体类型 ComboBox 移除局部静态 `SelectedIndex="0"`；设备决策显式使用双向 `DeviceDecision` 绑定，媒体筛选继续恢复 `MediaFilterState`，避免初始化阶段把持久化状态抢回第一项。
- 设备冲突 Inspector 显示 `StagedRemoteBackupStatus`，默认提示隔离区保护，完成下载校验后显示游戏、远端设备、备份 ID 和有效期；决策备注、保存决策、下载校验、已校验恢复均补充可访问名称和安全 ToolTip。
- GSC-128 自动证据：Release 0 warning/0 error、Core `65/65`、Worker `234/234`、Playnite `327/384`（57 跳过）、源码/WPF 门禁通过，真实宿主和 Rclone/多设备仍需人工复核。

## 2026-09-03 任务中心批量安全重试

- `DashboardViewModel` 新增 `RetryAllTasksCommand` 与 `RetryAllTasksAsync`；从最近任务中选取 `CanRetryTask` 项，按游戏/任务类型去重并保留最新记录，`BackupAll`/`MediaInbox` 使用全局单例键。
- 批量操作沿用 `RetryTaskCoreAsync` 的 Backup、MediaSync、CloudUpload、MediaInbox 分流；先二次确认，单项异常收集后继续，最终刷新快照并显示汇总，不新增 Worker/IPC 消息。
- GSC-127 自动证据：Release 0 warning/0 error、Core `65/65`、Worker `234/234`、Playnite `326/383`（57 跳过），源码校验和 WPF 静态审计通过；真实 Playnite 长任务和关闭竞态仍需观察。

## 2026-09-03 Playnite 游戏菜单补充媒体同步

- `GameSaveCenterPlugin.GetGameMenuItems` 新增“同步媒体”，对 Playnite 当前选中的一个或多个游戏调用现有 `MessageTypes.SyncMedia`；先写入最新游戏描述，`UploadAfterSync` 跟随 `Settings.EnableCloudUpload`。
- 入口受 `Settings.EnableMediaSync` 保护，媒体关闭时只显示提示；没有新增 Worker/IPC 业务协议，保留既有任务、错误和通知语义。
- GSC-126 自动证据：Playnite `325/382`（57 跳过）和源码校验通过；真实 Playnite 右键菜单、多选和任务通知仍需人工观察。

## 2026-09-03 游戏级云端状态汇总媒体上传

- `SqliteStateStore.GetDashboardGameRecordsAsync` 的现有媒体统计子查询现在同时聚合已归类媒体的云端状态；Dashboard 以失败、等待重试、待上传、已上传的顺序合并存档和媒体状态。
- 这样 `GameStatusDto.CloudState` 在存档已上传但媒体上传失败/排队时仍能显示真实的游戏级风险；`Inbox`/`Ignored` 媒体不计入游戏状态，没有云端内容时保留“未启用”。没有新增 IPC 字段，也没有 N+1 查询。
- GSC-125 自动证据：Release 0 warning/0 error，Core `65/65`、Worker `234/234`、Playnite `326/383`（57 跳过），源码校验和 WPF 静态审计通过；真实 Rclone 和宿主显示仍需人工观察。

## 2026-09-02 游戏选择器状态筛选抢写

- GamePicker 的状态/排序选项是静态列表，不应在 XAML 同时设置 `SelectedIndex="0"` 与共享 ViewModel 的双向 `SelectedItem`。生产 Shell 和隐藏兼容 Dashboard 各有一套选择器，两个初始化序列会互相抢写 `GamePicker.StatusFilter`，表现为用户选“全部”后仍沿用“已安装”过滤。
- 当前两套 XAML 已移除状态、排序的强制索引；动态 `PlatformFilterOptions` 仍保留索引 0 和 Loaded 恢复，因为它会异步重建。不要把状态/排序的静态索引加回来。
- 回归覆盖：`GamePickerViewModelTests.AllFilterIncludesUninstalledGameThatHasMatchOrBackup` 和 `GamePickerShellSourceTests.GamePickerStatusAndSortSelectionsComeFromSharedViewModel`。
- 自动证据：Core `65/65`、Worker `233/233`、Playnite `325/382`（57 跳过）、Release 0 warning/0 error、源码门禁通过，WPF 静态审计 0 error/18 warning/172 info；真实宿主仍需第二台 Playnite 复核。

## 2026-09-02 跨机器 Steam 游戏目录同步与安装状态识别

- 已确认插件不直接读取 Steam 客户端，而是读取 Playnite `Database.Games`，再把目录描述同步到 Worker 的 SQLite；此前 500+ 游戏库在 Dashboard 打开时仍跳过自动目录同步，第二台机器的本地 Worker 缓存为空/过期时，游戏会永久不出现在搜索结果，除非手动刷新。
- `GameSaveCenterPlugin` 现在在 Dashboard 打开后允许大库/超大库同步目录描述；`DashboardViewModel` 仍先绘制缓存，但随后立即在后台触发同步。Worker 先持久化全部描述，再把昂贵的 Ludusavi 匹配放入已有的节流队列，保持首屏不阻塞。
- `PlayniteGameAdapter` 将存在的 `InstallDirectory` 作为 `IsInstalled` 的只读兜底，避免 Steam/Playnite 短暂错误上报未安装时被默认“已安装”筛选隐藏。若游戏根本没有进入 Playnite Steam 库，插件仍无法仅凭 Steam 客户端发现它，必须先让 Playnite 导入该游戏。
- 自动证据：Core `65/65`、Worker `233/233`、Playnite `323/380`（57 跳过），Release 构建 0 warning/0 error，源码校验通过，WPF 静态审计 0 error/18 warning/172 info；真实第二台 Playnite 宿主仍需安装新包后复核。

## 2026-09-02 媒体收件箱旧代际分页取消

- `LoadMediaInboxPagesAsync` 现在接收 `requestGeneration`，并在每页 IPC 前后与 `mediaInboxLoadGeneration` 比较；旧代际直接返回 `null`，调用方不再进入集合或 UI 回写。
- 这收口了页面卸载、模式切换和忙碌期间最新请求排队时的无效工作：当前已发出的单个 IPC 请求仍由客户端既有超时完成，但旧加载不会继续请求最多 5000 条收件箱的剩余页面。
- STAB-021 自动证据：Release 0 warning/0 error，Core `65/65`、Worker `232/232`、Playnite `321/378`（57 跳过），源码校验和 WPF 静态审计通过；真实 Playnite 快速切换/卸载仍需人工观察。

## 2026-09-01 IPC 长连接读取器的取消等待对象累积

- `BoundedIpcLineReader` 不再为每次底层读取创建会一直等待监听器令牌取消的无限 `Task.Delay`；改用一次性 `CancellationToken.Register` 唤醒竞争任务，读取完成后立即释放注册。
- 任务事件长连接的取消、JSON 行边界、4 MiB 上限和超大消息丢弃语义保持不变；这只收口长期运行时的对象/令牌回调积累。
- STAB-020 自动证据：阻塞读取取消与源码契约回归通过；Release 0 warning/0 error，Core `65/65`、Worker `232/232`、Playnite `321/378`（57 跳过），源码门禁通过。真实宿主长时间任务通知仍需人工观察。

## 2026-09-01 初始同步取消的令牌源释放竞态

- `DashboardViewModel.CancelInitialSynchronization` 不再假设字段交换后令牌源一定仍未释放；如果后台同步任务已在并发窗口内完成并释放对象，卸载取消会吞掉该次 `ObjectDisposedException`。
- 令牌源释放权仍归初始同步/缓存重试任务的 `finally`，避免主动 Dispose 与 `Task.Delay` 取消注册竞争；正常取消、代际失效和后台同步行为不变。
- STAB-019 自动证据：定向 Playnite 回归通过；Release 0 warning/0 error，Core `65/65`、Worker `230/230`、Playnite `320/377`（57 跳过），源码门禁通过。真实宿主快速打开/关闭仍需人工观察。

## 2026-09-01 原生确认框 UI 线程边界

- `GameSaveCenterPlugin.ConfirmAsync` 的原生 Playnite 对话兜底不再直接在调用线程执行；结果变量在 `TryInvokeUi` 的 Dispatcher 边界内赋值，后台游戏事件和快捷操作不会从线程池触碰宿主对话 API。
- Dispatcher 已关闭或调用失败时返回 `false`，因此危险操作保持未确认状态；Dashboard 嵌入式确认流程、按钮命令和文案不变。
- STAB-018 自动证据：定向 Playnite 源码回归通过；Release 0 warning/0 error，Core `65/65`、Worker `230/230`、Playnite `319/376`（57 跳过），源码门禁通过。真实宿主后台确认与关闭竞态仍需人工观察。

## 2026-09-01 自动修改器审计异常隔离

- `LaunchAfterDelayAsync` 的自动启动审计统一调用 `TryAppendAutoStartAuditAsync`；该 helper 会观察取消并吞掉审计存储异常，避免 detached task 在 Worker 关闭期间留下未观察异常。
- 原始启动异常在停机 token 已取消时降为 Debug；正常启动失败仍保留 Error。自动启动成功后即使审计失败，也不会影响已启动进程或让后台任务冒泡。
- STAB-017 自动证据：Release 0 warning/0 error，Core `65/65`、Worker `230/230`、Playnite `319/376`（57 跳过），源码门禁通过。真实 Worker 关闭竞态仍需人工观察。

## 2026-09-01 自动修改器审计写入退出期取消

- `GameToolService.LaunchAfterDelayAsync` 的三个审计分支（已有实例跳过、成功启动、启动失败）均使用传入的延迟任务 token，不再以 `CancellationToken.None` 回写 SQLite。
- 延迟任务已经绑定 `GameSessionCoordinator.ApplicationStopping`；Worker 停止时，延迟、启动后的审计和取消异常均不会继续形成停机期存储访问。
- STAB-016 自动证据：Release 0 warning/0 error，Core `65/65`、Worker `230/230`、Playnite `319/376`（57 跳过），源码门禁通过。真实启动/停机时序仍需人工观察。

## 2026-09-01 游戏会话自动化退出期取消

- `GameSessionCoordinator` 注入可选 `IHostApplicationLifetime`，为所有脱离 IPC 请求的会话自动化操作提供统一 `ApplicationStopping` 令牌：自动修改器、退出备份/媒体同步、游玩中定时备份/媒体同步均不再使用 `CancellationToken.None`。
- `RunSafeAsync` 只在 Worker 停止取消时记录 Debug；非取消异常仍记录 Error。这样不会让停机中的任务继续访问已开始释放的 SQLite、归档目录或外部工具。
- STAB-015 自动证据：Release 0 warning/0 error，Core `65/65`、Worker `229/229`、Playnite `319/376`（57 跳过），源码门禁通过。真实 Worker 退出竞态仍需人工观察。

## 2026-09-01 会话存档路径快照退出期取消

- `SavePathDetectionService` 注入可选 `IHostApplicationLifetime`；会话开始时的非阻塞存档路径快照使用 `ApplicationStopping`，不再永久使用 `CancellationToken.None`。
- 这样保留了“不阻塞游戏启动 IPC”的行为，同时保证 Worker 停止时扫描会取消，避免 SQLite 已释放后继续写入快照/审计；完成回调仍观察取消或失败结果。
- STAB-014 自动证据：Release 0 warning/0 error，Core `65/65`、Worker `228/228`、Playnite `319/376`（57 跳过），源码门禁通过。真实 Worker 重启取消时机仍需人工观察。

## 2026-09-01 FLiNG 下载进度写入收口

- FLiNG 下载接口使用可等待的 `Func<long, long?, Task>` 进度回调；下载循环等待回调完成，不再把任务进度写入丢到未观察的后台任务。
- GameToolService 只在百分比变化时报告 5–80 的下载进度，避免 80 KiB 分块级别的 SQLite/任务事件写入和进度倒序；下载、取消、解压和 2 GiB 安全上限未改变。
- STAB-013 自动证据：Release 0 warning/0 error，Core `65/65`、Worker `227/227`、Playnite `319/376`（57 跳过）和源码门禁通过。真实网络下载与取消时机仍需人工回归。

## 2026-09-01 公共 IPC 入口的退出期保护

- `GameSaveCenterPlugin.RequestAsync<T>` 现在检查 `lifetimeCancellation`；退出后返回 `Task.FromCanceled<T>`，统一阻止页面命令、快捷操作和异步续体在多个 await 后绕过生命周期守卫发起新 Worker 请求。
- 该保护只拦截退出后尚未创建的请求，不强行中断已经在管道中的请求；现有调用方的 `OperationCanceledException` 由页面命令的取消路径观察，不显示退出期错误通知。
- STAB-012 自动证据：Release 0 warning/0 error，Core `65/65`、Worker `226/226`、Playnite `319/376`（57 跳过），源码门禁通过。真实宿主关闭中操作仍需人工回归。

## 2026-09-01 Playnite 退出阶段后台生命周期收口

- `GameSaveCenterPlugin.OnApplicationStopped` 现在先取消 `lifetimeCancellation`，停止任务通知计时器后再停止本插件持有的 Worker；计时器已经排队的回调会在轮询入口、IPC 返回后和通知逐项处理前退出。
- `FireAndForget`、`EnsureWorkerAsync`、库回调、游戏启动/停止事件、目录同步和任务通知均拒绝退出后的新工作；同步闸门等待使用 `lifetimeCancellation.Token`，避免 Playnite 关闭期间继续触碰 Worker、Dispatcher 或 UI 通知。
- Release 构建 0 warning/0 error，Core `65/65`、Worker `226/226`、Playnite `319/376`（57 跳过）和源码门禁通过。真实宿主退出竞态、Worker 重启与 Dispatcher 关闭仍属于人工验证边界。

## 2026-09-01 异步操作返回上下文保护

- 保存媒体单项/批量元数据、存档元数据时，先捕获原游戏、条目和编辑值；Worker 返回后只有原游戏仍在媒体/存档工作区、且编辑器仍对应原条目时才更新集合、摘要或清理 dirty 标记，避免切换期间丢失新输入。
- 媒体重新归类、备份完成提示、校验/恢复准备、策略模板应用、备份比较/保留预览和进程映射输入清理也不再读取异步期间变化的当前对象；旧结果只服务于原请求。
- 本阶段未改 XAML；Release 构建 0 warning/0 error，Core `65/65`、Worker `226/226`、Playnite `318/375`（57 跳过）和源码门禁通过。真实 Playnite 快速切换仍需人工验证。

## 2026-09-01 详情编辑草稿与游戏摘要刷新保护

- `SelectedBackup`/`SelectedMedia` 重新绑定同一 ID 时，不再无条件覆盖编辑器字段。`backupCommentDirty`、`backupLockDirty`、`mediaCommentDirty`、`mediaFavoriteDirty` 分别记录用户是否改过对应值，详情刷新只同步未修改字段。
- 切换到新条目或清空选择会完整同步/清空编辑器；存档单项保存、媒体单项保存以及媒体批量修改成功后清理相应 dirty 标记，使后续服务端回读可以校准值。
- 游戏策略编辑以选中游戏的基线副本识别本地未保存 `Policy`，快照刷新时只给展示集合保留该草稿；保存请求捕获原游戏 ID、名称和策略副本，返回期间切换游戏时只更新原游戏的基线，成功提示不会串到新选中的游戏。`GamePickerViewModel.SetItems` 在当前 item 未变但 DTO 更新时补发 `SelectedGame`，Dashboard 再转发通知给壳层绑定。
- 这项保护只涉及 Playnite ViewModel 的 UI 编辑草稿，不改变 DTO、IPC 或 Worker 数据；需在真实 Playnite 中边输入边触发任务刷新/切换页面观察绑定体验。

## 2026-09-01 媒体收件箱与详情刷新一致性

- `MediaInboxMode` 切换现在用 `mediaInboxLoadGeneration` 丢弃过期可见响应；`pendingMediaInboxLoadMode` 保留忙碌期间最后一次模式，`RunAsync` 结束后自动补加载。旧响应仍可更新对应缓存，但不会把旧集合应用到当前视图。
- 媒体收件箱刷新按当前 `MediaId` 和目标游戏 ID 保留用户选择；存档版本和当前游戏媒体详情刷新按 `BackupId`/`MediaId` 保留选择，只有条目不存在时才回退第一项。媒体工作区刷新在“已忽略”模式会同时更新忽略缓存。
- 收件箱补齐共享 Worker 离线态，离线时隐藏“空列表”误导文案；健康状态下的 Demo-first 布局、命令、Binding、滚动和虚拟化不变。新增源契约回归。
- Release 构建 `0 warning/0 error`；Core `65/65`、Worker `226/226`、Playnite `315/372`（57 跳过）；源码/WPF 门禁和双主题、多尺寸、连续 Resize、Shell `render-qa OK`。真实宿主快速切换、Worker 重启和 DPI 仍需人工观察。

## 2026-09-01 修改器目录加载状态反馈

- `DashboardViewModel` 新增 `IsTrainerCatalogLoading` 与 `IsTrainerReleasesLoading`，覆盖 FLiNG 目录同步/搜索及版本查询的真实请求生命周期，并在 `finally` 中清理状态。
- `TrainerCenterView` 的目录结果和版本结果空状态现在要求对应请求已结束；请求期间使用共享 `WorkspaceStatePresenter` 展示加载信息，避免空集合短暂显示为“没有匹配”或“请选择版本”。
- Release 构建 `0 warning/0 error`；Core `65/65`、Worker `226/226`、Playnite `313/370`（57 跳过），源校验和 WPF 静态审计通过；RenderHarness 双主题、多尺寸、连续 Resize 和 Shell 紧凑标题区均 `render-qa OK`。不要把离屏结果写成新增 UI 在真实 Playnite 全矩阵中已验收。

## 2026-09-01 修改器版本读取竞态与逐项操作优化

- `LoadTrainerReleasesCommand` 接收 FLiNG 目录行的 `CommandParameter`；点击某一行的“读取版本”会先将该行设为当前目录项，再读取它的版本，而不是复用此前的选择。
- 版本请求用 `trainerReleaseLoadGeneration` 和当前 `CatalogId` 双重确认响应是否仍然有效；`pendingTrainerReleaseCatalogId` 记录忙碌期间最新选择，`RunAsync` 完成后自动补发一次，旧响应不会覆盖新选择。
- 当前验证：Release 构建 `0 warning/0 error`；Core `65/65`、Worker `226/226`、Playnite `314/371`（57 跳过），源码/WPF 门禁通过，RenderHarness 多尺寸、双主题、连续 Resize 和 Shell QA 均 `render-qa OK`。该请求队列仍需随新包在真实 Playnite 中人工观察一次。

## 2026-09-01 IPC 媒体边界与任务通知缓存优化

- `IpcRequestDispatcher` 对 `ListMedia` 的请求统一夹限到 1–1000 条；数据库仍保留自身 5000 条防线，分发层新增的 1000 条上限用于避免异常 IPC 请求产生过大的单次响应，当前 Dashboard 请求行为不变。
- `GameSaveCenterPlugin` 的任务通知去重从无界 `ConcurrentDictionary` 改为 `BoundedTaskIdSet`，默认保留最近 4096 个任务 ID，操作带锁且大小写不敏感；这只是通知去重缓存，未改变任务执行、状态持久化和通知策略。
- Release 构建 `0 warning/0 error`；Core `65/65`、Worker `226/226`、Playnite `312/369`（57 跳过），`validate-source.py` 通过，WPF 静态审计 `0 error/18 warning/172 info`。真实 Playnite、DPI、历史任务重放和用户环境命名管道响应仍需人工验收。

## 2026-09-01 FLiNG 目录解析收口

- 将在线目录和详情页解析从网络流程中抽成纯解析方法，保留目录最小数量保护；支持绝对、相对和协议相对链接，统一 HTML 解码与 URI 规范化，只接受 FLiNG HTTPS 主域/子域及预期路径。
- 目录页去除追踪查询参数并按规范 URL 去重；下载链接保留查询参数、去除 fragment，非法外站链接不会进入本地缓存或下载列表。新增 HTML 实体、相对链接、重复项和外站链接回归。
- Worker `222/222`、Core `65/65`、Playnite `310/367`（57 跳过）和 `validate-source.py` 通过。真实 FLiNG 页面、实际下载、杀毒软件拦截和 Playnite 宿主仍需人工回归。

## 2026-09-01 媒体云端重试收口

- 修复媒体本地归档成功、云端复制失败后再次同步可能不上传的问题：单游戏同步不再只看本轮 `copied > 0`，还会识别已归类且处于 `Pending`/`Failed`/`RetryScheduled` 的媒体；公共 Inbox 同样会补入这些游戏。
- 新增 `GetMediaGamesNeedingCloudUploadAsync` 及 SQLite 索引；媒体复制前回写 `Pending`，成功为 `Synced`，同时补齐媒体和游戏云端状态的用户文案。Worker 测试 `220/220` 通过。
- 该修复只保证任务重试路径不丢上传，不等同真实 Rclone 远端验证；真实网络失败、远端权限和 Playnite 任务通知仍需人工回归。

## 2026-08-31 UI/可靠性审计收口

- 本轮按用户授权完成所有不依赖真实 Playnite 宿主的修复：`BackupAll` 改为 SQLite 持久化主任务，提交立即返回，逐游戏进度通过既有任务事件/SQLite 状态反馈；Worker 启动时恢复 `Queued/Running` 整库任务，任务中心支持安全重试。真实 Worker 重启后的连续恢复仍需在隔离宿主手工验证。
- 大库匹配保留描述缓存和后台分批执行，未匹配条目按 6 小时重试；后台任务使用 Worker 生命周期取消，并且所有待匹配项都会入队，避免只处理首批后永久遗漏。媒体签名加入 4 KiB 多点采样校验、30 天/10 万条清理，样本只是快速变更探测，完整 SHA-256 仍是去重依据。
- 生产 Shell 紧凑标题区在 `<980 DIP` 使用自动高度 + `WrapPanel`，操作区按可用宽度换行；RenderHarness 新增 `shellqa` 入口，720/960/980/1040×640/700 均通过，720 截图确认“立即备份/全部备份”不再被右侧裁切。剪贴板重试改为异步等待，不阻塞 Playnite Dispatcher。
- 自动证据：Debug 解决方案构建 0 警告/0 错误；Core `59/59`、Worker `219/219`、Playnite `310/367`（57 跳过）；WPF 静态审计 `0 error/18 warning/172 info`；Shell QA `shell-qa OK`。静态审计 warning 主要是既有 StackPanel/ScrollViewer 启发式提示和参考主题 Canvas，不等同于运行时缺陷。
- 当前仍不能宣称真实 Playnite、DPI、宿主主题/高对比度、窗口连续缩放、Worker 实例重启日志和真实大库滚动已验收；环境缺少可证明隔离的 Playnite 安装，必须由用户手工完成这一阶段。

## 2026-08-26 UI-335 标题栏与页面卡片完整圆角

- 用户反馈截图中的标题栏和页面卡片仍有尖角。根因是 `GscRedesignHeaderCorner` 只设置了 `16,16,0,0`，普通 `GscRedesignSectionCard` 也没有统一裁剪内部子元素。
- 标题栏改为 18 DIP 四角圆角、完整 1 DIP 描边和裁剪；共享页面卡片启用 `ClipToBounds=True`，避免内部背景/内容把圆角视觉填回直角。命令、Binding、页面数据、滚动和虚拟化未改。
- Release 0 warning/0 error；Core `59/59`、Worker `210/210`、Playnite `310/367`（57 跳过）；源码/XAML/WPF 门禁通过，`.tmp/ui-qa-rounded-surfaces-v1/render-qa-report.txt` 为 `render-qa OK`。RenderHarness 不包含外层 Shell 标题栏，真实 Playnite 未运行，Phase 4 按用户要求跳过。

## 2026-08-26 UI-334 按钮状态层覆盖范围

- 生产 `GscWpfUiButton` 的 Hover/Pressed 状态层现在覆盖整个圆角按钮；`ButtonChrome` 保持 0 padding，内容间距改由 `ContentPresenter` 的 `TemplateBinding Padding` Margin 保留。
- 新增全按钮 `FocusOverlay`：键盘聚焦时覆盖整个按钮，并继续保留共享焦点环；Primary 按钮使用对应的 on-accent 覆盖色。命令、Binding、按钮尺寸、文字省略、滚动、虚拟化和主题资源契约未改。
- TextBox 的 Padding 回归断言收窄到 TextBox 模板范围，避免与按钮模板的内容 Margin 契约冲突。Release 0 warning/0 error；Core `59/59`、Worker `210/210`、Playnite `310/367`（57 跳过）；源码/XAML/WPF 门禁通过，`.tmp/ui-qa-button-focus-v1/render-qa-report.txt` 为 `render-qa OK`。真实 Playnite 未运行，Phase 4 按用户要求跳过。

## 2026-08-26 UI-333 生产标题栏顶部圆角

- `AcrylicProductionShellView.HeaderSurface` 现在使用共享 `GscRedesignHeaderSurface`，顶部两角使用 `GscRedesignHeaderCorner=16,16,0,0` 并启用 `ClipToBounds`；底部保留直线边界，标题栏与页面内容仍然连续。
- 本轮只改共享 Redesign 资源、生产 Shell 样式引用和源码契约测试；页面导航、标题/副标题绑定、顶部按钮命令、响应式行高和内容滚动保持不变。
- Release 构建 0 warning/0 error；Core `59/59`、Worker `210/210`、Playnite `310/367`（57 跳过）；源码/XAML/WPF 静态门禁通过，`.tmp/ui-qa-header-corner-v1/render-qa-report.txt` 为双主题、多尺寸和 Resize `render-qa OK`。RenderHarness PNG 不包含外层 Shell 标题栏，真实 Playnite 仍未运行，Phase 4 按用户要求跳过。

## 2026-08-26 UI-332 按钮组、提示气泡与环境材质边界

- 按用户当前视觉反馈，维护中心“远端恢复”按钮统一使用共享 `GscWpfUiRemoteRestoreButton`（148×36 DIP），媒体中心当前游戏媒体的两处批量操作统一使用 `GscWpfUiMediaBatchButton`（120×36 DIP）；命令、Binding、间距语义和两处页面实例保持不变。
- 修改器中心“可下载版本”提示改用 `GscDiagnosticHintBubble`/`GscDiagnosticHintText`，取消醒目的蓝色粗体文本，保留低干扰提示气泡语义。
- `AmbientMaterialLayer` 增加共享 `CornerRadius` 依赖属性和 `MaterialChrome` 裁剪容器；页面层默认 16 DIP 圆角，生产 Shell 层显式使用 0 DIP 以覆盖完整内容窗格，消除页面底部环境材质的尖锐矩形边界。
- Release 构建 0 warning/0 error；Core `59/59`、Worker `210/210`、Playnite `309/366`（57 跳过）；源码/XAML/WPF 静态门禁通过，`.tmp/ui-qa-button-material-v1/render-qa-report.txt` 为双主题、多尺寸和 Resize `render-qa OK`。本轮未运行真实 Playnite，Phase 4 仍按用户要求跳过，实机视觉/DPI/宿主日志仍需人工复核。

## 2026-08-26 STAB-007 性能测量事实

- Phase 7 只完成测量，没有性能猜测式改动：默认 Blur/动画/滚动/虚拟化保持不变；Blur 20/78/100 的半径回归为 20/78/100 DIP。
- 全量 Worker 数据规模通过：2,000 游戏、20,000 备份、10,000 任务、30,000 媒体、500 工具，约 3m03s，managed memory 增长 0 MiB、句柄 +0、线程 +0。
- 2,000 游戏合成 UI 集合测试：首次/未变化/单项变化 55/2/15ms，搜索/清空 215/196ms，任务 ReplaceAll 1/0ms；离屏 Render QA 253 样本 20–3032ms，平均 219.01ms，双主题和 Resize 通过。
- 真实 Playnite 首屏、切页、侧栏、主题/背景内存、DPI 和大库滚动仍未验证；Phase 4 是用户明确跳过的宿主阶段，不能把这些数字写成实机结论。

## 2026-08-26 STAB-006 页面树与响应式协调事实

- 生产页面路径现在由 `AcrylicProductionShellView.PageHost` 和 `GetWorkspaceView<T>` 统一承载；Dashboard 中的旧页面树仍存在，但只作为兼容资源/审计面，`GetLegacyCompatibilityWorkspaceViews()` 是显式边界。不要删除旧树，除非后续取得 Playnite 初始化、资源查找和外部引用的真实证明。
- `ResponsiveLayoutCoordinator.Calculate(width, height)` 是宽高状态的唯一数值来源，保留原有断点和尺寸。Dashboard 外壳与生产 Shell 共用状态，Shell 的导航、侧栏切换和延迟 Resize 都通过 `ApplyResponsiveLayout`，回调在 `IsLoaded` 之后才执行。
- 结构测试覆盖双树职责、生产注册表和宽高临界值；Release Core `59/59`、Worker `210/210`、Playnite `309/366`（57 跳过），WPF `0 error/18 warning/172 info`，Render QA 双主题/多尺寸/Resize `render-qa OK`。
- Phase 4 按用户明确指示跳过。离屏渲染、静态审计和测试不等于真实 Playnite；实机视觉、DPI、宿主日志、Worker 重启、性能和 `GscTableViewportHeight` 外部引用仍需人工/环境证据。

## 2026-08-26 STAB-005 媒体 Inbox IPC 分页事实

- 超限接口已确认是 `media.inbox.list` / `ListUnassignedMedia`，不是 Dashboard 快照：旧 Playnite 请求 `Limit=5000` 会把 4615 条 Inbox 媒体拼成超过 4 MiB 的响应，随后 UI 只看到通用“操作失败”。
- `GameQueryDto.Offset` 是兼容性增量字段；Worker 强制将未分配/已忽略媒体单页限制为 500，并使用稳定时间+ID排序和 SQL `OFFSET`。Playnite 连续请求 offset `0..4999`，短页或空页停止，最终最多应用 5000 条，保留原集合、选择、Binding 和虚拟化路径。
- `NamedPipeServerService` 超限日志必须包含请求 ID、消息类型、完整响应字节数和 payload 字节数；4 MiB 限制与 `MESSAGE_TOO_LARGE` 语义不能放宽。该诊断也覆盖未来其他超限接口。
- 自动验证：Core `59/59`、Worker `210/210`、Playnite `302/359`（57 跳过），Release `0 warning/0 error`，RenderHarness `render-qa OK`，WPF `0 error/18 warning/172 info`。现有用户 Worker 的只读 500 条请求为 `745990` 字节；补丁 Worker 未在用户实例中替换运行。
- 本次按用户要求跳过 Phase 4，不得把离屏 RenderHarness或旧 Worker 只读探针写成真实宿主验收；发布/安装新包后应复查日志中 `RequestId=... Type=media.inbox.list ResponseBytes=...`，并确认 Inbox/Ignored 页面无超限错误。

## 2026-08-26 STAB-003A 真实 Named Pipe 烟测事实

- 当前 Release Worker 在隔离 `.tmp/phase3-ipc-runtime-escalated/data` 中真实启动成功；超限请求 `4194778` UTF-8 字节得到 `MESSAGE_TOO_LARGE`，同一连接后续 `system.ping` 成功。
- 受限上下文的 Named Pipe `Access is denied` 已与 Worker 启动问题区分：最小同用户探针同样失败，提升权限后真实 Worker 烟测通过。报告位于 `.tmp/phase3-ipc-runtime-escalated/runtime-smoke-report.txt`。
- 烟测结束已停止临时进程，未触碰 Playnite 用户数据；真实宿主、多客户端、重启和日志矩阵仍属于 Phase 4/人工验收范围。

## 2026-08-26 STAB-004 真实 Playnite 环境阻塞事实

- Phase 4 真实宿主矩阵尚未执行，状态必须保持 `BLOCKED_ENVIRONMENT`：当前机没有 `Playnite.DesktopApp.exe`、运行中的 Playnite 进程、App Paths 或卸载注册表入口；历史候选 `D:\software\Playnite\Playnite\Playnite.DesktopApp.exe` 也不存在。
- 不要运行 `scripts/real-host-audit.ps1` 或 `scripts/dev-install-run.ps1` 作为替代。它们的安装/启动路径可能写入用户扩展目录、关闭当前宿主或复用全局单实例；在没有隔离安装、独立数据根、唯一 PID 和扩展路径证明前，不能触碰用户实例。
- 必须验证的真实条件仍包括四种窗口尺寸、100%/125%/150% DPI、Light/Dark/Follow/High Contrast、七个 Dashboard 页面与 Settings、侧栏/Tab/表格/搜索/键盘、Dashboard 重载、游戏启动事件、Worker 启停/握手/日志和旧 DLL 复用。
- Phase 0–3 自动验证已完成，但不能解除 Phase 4 阻塞，也不能作为 Phase 5 双页面树治理的“前置全部通过”证据。解除条件是用户提供可审计的隔离 Playnite 环境。

## 2026-08-26 STAB-003 IPC 边界事实

- `BoundedIpcLineReader` 是共享 Contracts 类型，必须按连接实例化；它有 4 KiB 内部字符缓冲并保存块内偏移，不能改回静态一次性块读取，否则遇到同块多条消息会丢掉下一条。超限行只保留上限内内容并继续消费到换行。
- 请求、响应和事件都使用 `ProtocolConstants.MaximumMessageBytes`（4 MiB）；Worker 服务端对请求/响应、事件服务端对事件写出、Playnite 客户端对请求/响应/事件读取均有边界。超限稳定码为 `MESSAGE_TOO_LARGE`，事件端收到后忽略并继续重连/监听。
- `NamedPipeServerService` 的业务连接槽位为 32，`TaskEventPipeServerService` 为 8；这是过载保护，不改变单请求、任务事件广播或 SQLite 回退路径。管道名、版本、消息类型和 `PipeOptions.CurrentUserOnly` 不变。
- Playnite 请求响应读取取消后继续抛出原 `TimeoutException("Worker response timed out.")`；事件监听 token 取消仍走外层 `OperationCanceledException` catch；服务端仍分别处理 JSON 错误、IO 断开和停止取消。
- STAB-003 验证：IPC 定向 `3/3`，隔离 Release Core `59/59`、Worker `201/201`、Playnite `302/359`（57 跳过），构建 `0 warning/0 error`，WPF `0 error/18 warning/172 info`，`.tmp/phase3-ipc-boundary-render-final/render-qa-report.txt` 为 `render-qa OK`。
- 本阶段没有 UI 改动；真实宿主的多连接/重启/权限和 Worker 日志仍需人工验证。

## 2026-08-26 STAB-002 外部进程执行事实

- `ProcessExecutionLimits.MaximumOutputBytes` 集中定义为每个 stdout/stderr `4 * 1024 * 1024`；`ExternalProcessRunner` 必须使用有界读取，不能恢复 `ReadToEndAsync` 或把超限内容继续追加到内存。
- `ProcessResult.ErrorCode` 是稳定的低层错误码：超限为 `PROCESS_OUTPUT_LIMIT_EXCEEDED`，超时为 `PROCESS_TIMED_OUT`，可执行文件缺失为 `EXECUTABLE_NOT_FOUND`。正常输出、非零退出码、取消异常和既有标准错误文本都要继续保留。
- 超限读取器达到上限后仍消费管道但丢弃后续内容，以避免子进程阻塞；只记录流是否受限和上限，不记录被截断文本。不要把本地化错误文本当作机器判断条件。
- `RcloneClient.RunSafeAsync` 的签名不再含 `workingDirectory`；它向 Runner 显式传 `null` standardInput，Runner 继续将可执行文件所在目录作为实际 WorkingDirectory。不要为修复命名而改变 Rclone 参数、allowlist 或工作目录。
- STAB-002 验证：定向 Worker `6/6`，隔离 Release Core `59/59`、Worker `201/201`、Playnite `302/359`（57 跳过），构建 `0 warning/0 error`，WPF `0 error/18 warning/172 info`，`.tmp/phase2-process-stability-render/render-qa-report.txt` 为 `render-qa OK`。
- 本阶段未修改 UI；真实 Worker/Rclone/Ludusavi 实机行为和 Playnite 宿主日志仍需人工验证。

## 2026-08-26 STAB-001 Dashboard 生命周期订阅事实

- `DashboardViewModel` 不再在构造函数永久订阅 `PlayniteGameStarted`；`PlayniteGameStartedSubscription` 以幂等 `Start/Stop` 保存唯一 handler，`DashboardView.OnLoaded`/`OnUnloaded` 分别调用启动/停止。
- `OnPlayniteGameStarted` 在进入 `ApplyOnUi` 前后都检查 `IsSubscribed`，因此卸载后已经排队的回调不会继续写入 Dashboard；`pendingAutoSelectPlayniteId`、`TryApplyPendingAutoSelection` 和“游戏尚未到达时保留 pending”语义不变。
- 生命周期回归事实覆盖首次加载、卸载、重载、重复生命周期和 pending selection；源码门禁现在要求上述 View 生命周期契约，不能恢复为构造函数直接订阅。
- STAB-001 验证：定向 `6/6`，隔离 Release Core `59/59`、Worker `201/201`、Playnite `302/359`（57 跳过），构建 `0 warning/0 error`，WPF `0 error/18 warning/172 info`，`.tmp/phase1-dashboard-lifecycle-render/render-qa-report.txt` 为 `render-qa OK`。
- 本阶段没有修改 XAML；真实 Playnite 仍需重启扩展后确认事件订阅、DPI、主题、焦点和重载行为。

## 2026-08-26 STAB-000 当前稳定性修复基线

- 当前基线提交为 `28bccfe4ad7f55a9ea95083d5a686d7d2837e96b`，`main` 与 `origin/main` 同步，工作树干净。
- Phase 0 已完成只读基线：源码校验通过、XAML 19/19、WPF 0 error/18 warning/172 info、Release Core 59/59、Worker 201/201、Playnite 297/354（57 跳过），RenderHarness `render-qa OK`，2000 游戏合成耗时已记录在 WORKLOG。
- 生产壳通过 `AcrylicProductionShellView.PageHost` 动态承载 Overview、Save、Trainer、Media、Task、Maintenance；Settings 是独立 `GameSaveCenterSettingsView`。`DashboardView.xaml` 旧/兼容页面树仍保留，禁止在稳定性阶段删除。
- Phase 1 的首个已确认风险是 Dashboard 生命周期：`DashboardViewModel` 构造函数订阅 `PlayniteGameStarted`，而 View 的 Loaded/Unloaded 没有对应的显式启动/停止 API。后续应增加幂等 Start/Stop，由 `DashboardView.OnLoaded`/`OnUnloaded` 驱动，并锁定卸载后不调度 UI、重新加载可恢复、pending selection 不变。
- Phase 2/3 的首个边界风险也已确认：`ExternalProcessRunner.ReadToEndAsync` 无输出上限；Named Pipe 服务端/客户端读取整行后才检查大小；`RcloneClient.RunSafeAsync` 的 workingDirectory/standardInput 参数语义错位。正常输出、退出码、超时、取消、JSON 错误和协议契约必须保持不变。
- 本轮未重新启动真实 Playnite；RenderHarness 和静态审计均不等于宿主视觉、DPI、键盘焦点或生命周期验收。真实验证继续标记为 `MANUAL QA REQUIRED`。

## 2026-08-25 UI-331 当前事实：共享控件、侧栏动画与输入校验性能

- 历史实现曾通过外层 `Chrome.Padding={TemplateBinding Padding}` 应用输入框内边距；当前有效实现由原生 `PART_ContentHost` 消费 TextBox Padding，外层 Chrome 保持无 Padding。ContentHost 必须继续保持 `Margin=0`、`Padding=0`，不要恢复外层重复 Padding 或把 Padding 绑定到 ContentHost Margin 的旧写法。
- ComboBox 选中值和 `ComboBoxItem` 都显式继承 `FontFamily`、`FontSize`、`FontWeight`；共享 ComboBox 固定 `HorizontalContentAlignment=Left`、像素对齐和布局取整。不要在单个页面给相邻筛选框另设字体或基线补丁。
- `AcrylicProductionShellView` 的侧栏仍由 `GridLengthAnimation` 驱动 270↔72 DIP、210ms、CubicEase EaseOut；内容层额外使用 190ms、4 DIP 的淡入/位移过渡。切换完成、非动画布局恢复和 `OnUnloaded` 都要清理 `BeginAnimation`，避免重载后残留透明/偏移。
- Shell 的游戏背景仍是唯一静态图片层，`CacheMode=BitmapCache` 只用于该层；禁止把缓存或 BlurEffect 扩散到卡片、文字、表格、列表或滚动器，以免内存/虚拟化成本反弹。
- `GameSaveCenterSettingsView.OnSettingsFieldChanged` 通过 `DispatcherPriority.Background` 合并校验通知；`VerifySettings` 含文件存在性检查，不能改回每个字符同步执行，否则长路径输入会卡顿。
- UI-331 验证：WPF 0 error/18 warning/172 info、Playnite 297/354（57 跳过）、Release 0 warning/0 error、`.tmp/ui-qa-polish-v1/render-qa-report.txt` 为 `render-qa OK`。真实 Playnite 帧率、DPI、键盘焦点与动画仍需人工复核。

## 2026-08-25 UI-330 当前事实：毛玻璃强度直接比例映射

- `AdaptiveThemePalette.BlurRadiusForStrength` 将设置滑块 20–100 直接映射为 Blur 半径 20–100 DIP：默认 `GlassEffectStrength=78` 就是 78 DIP，100 才是完整 100 DIP。不要恢复此前 12–34 或 16–34 DIP 的压缩范围。
- 主界面 `GscGameBackgroundEffect` 只挂在 Shell 的静态游戏背景图片层；设置页 `GscSettingsAmbientEffect` 只挂在设置页环境层。卡片、文字、DataGrid、ListBox 和 ScrollViewer 禁止使用 BlurEffect。
- `EnableGlassEffects=false`、`SystemParameters.HighContrast`、无游戏背景/关闭跟随后，仍必须返回真实 `null` 或透明/不透明回退资源；强度调整不能重新触发背景解码。
- UI-330 验证：WPF 资源定向 119/158（39 跳过）、Playnite 全量 297/354（57 跳过）、Release 0 warning/0 error、`.tmp/ui-qa-glass-strength-v1/render-qa-report.txt` 为 `render-qa OK`。真实 Playnite 的 100% 帧率、DPI 和宿主视觉仍需人工复核。

## 2026-08-25 UI-329 当前事实：刷新与背景取色性能

- `DashboardViewModel.RefreshDashboardAsync` 只有在 `SelectedGame.PlayniteId` 变化时才允许刷新当前游戏 Icon/Background；普通自动快照轮询不能重复读取 Playnite 图标或重启同一背景解码。`DashboardView.OnLoaded` 的 `EnsureSelectedGameBackgroundLoaded` 只负责恢复卸载期间被取消且当前仍缺失的加载。
- `PlayniteGameBackgroundProvider.CreateAmbientBrush` 已将整帧 `CopyPixels` 改为五次 1×1 `BitmapSource.CopyPixels`，因为材质只需要五个采样点。不要为了“方便取色”恢复多 MB 的全图临时 `byte[]`，也不要改变已验证的五个采样坐标。
- `AcrylicProductionShellView.OnShellSizeChanged` 使用 `DispatcherPriority.Render` 合并连续窗口尺寸事件；`QueueGamePickerFilterDefaults` 使用单个 `Loaded` 调度和 pending 标记。待处理标记必须在卸载和 Dispatcher 关闭路径可安全复位，避免窗口重开后丢布局或重复恢复筛选。
- 这些优化不改变页面资源、Binding、Command、导航、列表虚拟化和侧栏动画；如果后续需要进一步提速，优先复用快照/材质缓存并保持 UI 线程只做轻量状态更新，不要增加新的轮询器或每帧动画计时器。
- UI-329 验证：定向 26/26、Playnite 297/354（57 跳过）、Release 0 warning/0 error、WPF 0 error/18 warning/172 info、`.tmp/ui-qa-performance-v1/render-qa-report.txt` 为 `render-qa OK`；真实 Playnite 帧率/内存/DPI 仍需人工复核。

## 2026-08-25 UI-328 当前事实：游戏背景跟随开关与材质回退

- `GameSaveCenterSettings.FollowSelectedGameBackground` 默认 `true`，位于设置页“外观与动态效果”中的“跟随当前游戏背景”开关；旧 JSON/便携设置没有该字段时必须继续默认跟随。
- `DashboardViewModel.ApplySelectedGameBackgroundPreference()` 只在偏好状态发生变化时刷新；关闭时取消 `PlayniteGameBackgroundProvider` 当前任务并清空 `SelectedGameBackground`、采样 Brush 和材质标记，开启时按当前选择重新加载。不要把它改成每次调节毛玻璃强度都解码图片。
- `DashboardView.ApplySelectedGameGlassResources` 和 `AdaptiveThemePaletteFactory.ApplyGameBackgroundGlassResources` 必须共同检查开关、采样材质、毛玻璃和高对比度；关闭/无图时 `GscGameBackgroundOpacity=0`、tint 透明、BlurEffect 为 null，并恢复 Demo 中性表面资源。
- `AmbientMaterialLayer` 的 `ThemeAmbientWash` 只能在实际使用游戏材质时隐藏：判断应使用 `UseSelectedGameBackground && hasGameMaterial`，不能只看 `hasGameMaterial`，否则关闭游戏背景后会丢失主题环境光。
- UI-328 验证：源码/XAML 门禁、WPF 0 error、Release 0 warning/0 error、Playnite 296/353（57 跳过）和 `.tmp/ui-qa-game-background-v1/render-qa-report.txt`（`render-qa OK`）通过；真实 Playnite 的开关即时效果、DPI、Follow/高对比度和性能仍需人工复核。

## 2026-08-25 UI-327 当前事实：文字渲染、修改器对齐与诊断气泡

- 生产壳、Dashboard、Settings、共享 `DataGrid` 和开发探针现在统一使用 `TextFormattingMode=Ideal`、`TextRenderingMode=ClearType`、`TextHintingMode=Fixed`，并保留像素对齐；不要为修复锯齿把 BlurEffect 加到文字或滚动内容。
- `TrainerCenterView` 的“确认导入”表单现在用 Grid 将“主程序”标签与 `TrainerImportEntryComboBox` 放在同一行，按钮和辅助说明在下一行；真实 `ImportEntryCandidates`、`SelectedImportEntryCandidate` 和确认/取消命令未改变。
- `Redesign.xaml` 的 `GscDiagnosticHintBubble` / `GscDiagnosticHintText` 是维护中心摘要提示的共享样式：低饱和信息底、细信息描边、常规次级文字。严重程度 pill 仍保留语义色，不能用诊断气泡样式覆盖 Warning/Error/Critical 标识。
- UI-327 已通过 Release 构建、Core 59、Worker 199、Playnite 295/57、源码/XAML/WPF 门禁和 `.tmp/ui-qa-font-bubbles-v1` 多主题多尺寸 render QA；真实 Playnite 宿主字体清晰度、DPI 和 Follow 仍需人工回归。

## 2026-08-25 UI-326 当前事实：设置宿主尺寸与侧栏边界折叠

- `GameSaveCenterSettingsView` 根 `UserControl` 不再设置 `MinWidth/MinHeight`；这些属性会阻止页面缩到紧凑断点。真实宿主第一次加载时由 `EnsureHostWindowSize` 将 Window 设为工作区允许范围内的约 1280×840、`SizeToContent=Manual`、水平/垂直 Stretch；RenderHarness 无 owner Window，不会被改尺寸。
- 设置页后续缩放仍走 `ApplyResponsiveLayout`，所以宿主可在首次打开后缩小，分类栏会堆叠到表单上方，表单内容在 `SettingsScroller` 内滚动。不要把 UserControl 的最小尺寸重新加回去。
- 生产侧栏的 `SidebarCollapseArea` 是 `SidebarLayout` 的底部 Grid 行，控制器使用 `AcrylicSidebarBoundaryButton`，只显示 `‹`/`›`，没有文字或独立书签形状。展开宽度为 270 DIP，折叠宽度为 72 DIP，按钮固定 32×32、16 圆角；静止透明、悬停/按下只叠加低 alpha tint。
- 侧栏宽度必须通过 `Controls/GridLengthAnimation.cs` 的 210ms `GridLengthAnimation` 动画变化，不能回到瞬时 `GridLength` 赋值或 Canvas 绝对定位。动画使用 `CubicEase.EaseOut`，内容列由 Grid 自动同步重排。
- `GameSaveCenterSettings.SidebarCollapsed` 是持久化 UI 偏好；`DashboardView` 通过 `SidebarCollapsedProvider/SidebarCollapsedChanged` 注入生产壳，切换后立即保存，启动时恢复。不要把插件实例直接耦合到 `AcrylicProductionShellView`。
- UI-326 已通过 Debug 隔离构建、Core 59、Worker 199、Playnite 295/57、源码/XAML/WPF 门禁和 `.tmp/ui-qa-sidebar-boundary-v1` render QA；真实 Playnite 宿主仍需人工确认初始 Window 尺寸、实际动画、DPI 和键盘焦点。

## 2026-08-25 UI-319 当前事实：Dune 浅色 FollowPlaynite 判断

- 当前用户 Playnite 配置 `config.json` 的 Desktop 主题是 Dune；Dune 用资源 `ThemeDarkStyle=False` 表示浅色，同时保留 `WindowBackgroundBrush` 和 `DarkWindowBackgroundBrush` 两套颜色。
- `AdaptiveThemePaletteFactory.Create` 在 FollowPlaynite 下必须优先读取 `ThemeDarkStyle`：False 使用浅色窗口资源/浅色回退，True 使用 `DarkWindowBackgroundBrush`/深色回退。不能只读取通用 `WindowBackgourndBrush`，因为 Playnite 默认主题的历史拼写可能作为遗留资源继续可见。
- 没有 `ThemeDarkStyle` 的第三方主题才走背景资源 + `TextBrush`/`TextBrushDark` 的一致性推断；若二者明暗冲突，应优先选择文字资源推断，避免设置独立窗口出现黑底黑字。
- UI-319 已通过定向 2/2、Playnite 290/57/0、Release 0 警告/0 错误、源码校验和 19 个 XAML 结构检查。真实宿主重载后的设置窗口 Follow 浅色像素仍需人工确认。

## 2026-08-25 UI-318 当前事实：Follow Playnite 资源解析

- Playnite Desktop 默认主题的窗口背景资源键是历史拼写 `WindowBackgourndBrush`，由 `MainWindowStyle`/`StandardWindowStyle` 使用；Follow 解析必须保留该键，同时兼容 `WindowBackgroundBrush` 等第三方主题键。
- 设置页可能被 Playnite 放在独立设置窗口中，不能只依赖插件 UserControl 的视觉树背景。`AdaptiveThemePalette` 现在还显式检查 owner Window 与 `Application.Current` 资源；背景仍不可用时用宿主 `TextBrush`/`TextBrushDark` 推断浅深。
- `GameSaveCenterSettingsView.OnThemeModeChanged` 先把 ComboBox 的 enum 写回 `CurrentSettings.ThemeMode`，再排队 `ApplyAdaptiveTheme`，确保从强制深色切换 Follow 不会继续使用旧值。
- UI-318 验证：真实资源键回归测试、Release 全量构建/测试和 v8 多主题多尺寸 render QA 均通过。离屏 RenderHarness 的默认 Follow 没有 Playnite 宿主资源，仍会按中性深色回退；真实宿主需安装后复核。

## 2026-08-25 UI-317 当前事实：壳体圆角玻璃与 Follow Playnite 浅色主题

- 生产 `AcrylicProductionShellView` 的导航由 `SidebarSurface` 真实 Border 承载，使用 `GscRedesignSidebarSurface` 与动态 `GscSidebarMaterialBrush`；不要再让内部 Grid 直接承担整块导航背景，否则右侧上下角不会被圆角裁切。
- 生产页脚由 `FooterSurface` 提供独立四边圆角玻璃面；文字、状态 Binding 和底部布局不变。共享导航样式也已改为同一动态 sidebar 材质。
- 设置页继续不使用游戏图片，但 `SettingsAmbientLayer` 负责固定环境渐变与唯一 BlurEffect，外壳、分类栏、卡片和内容层使用较低 alpha 的主题材质，使环境模糊可见。不要把 BlurEffect 加到文字、表格、列表或滚动面。
- `AdaptiveThemePaletteFactory.Create` 在 `FollowPlaynite` 下必须优先检查宿主发布的 Window/Main/Control/Background 资源，再检查视觉树背景；插件控件自身的深色 `GscBackdropBrush` 只能作为回退，不能遮蔽 Playnite 浅色主题。
- `RenderHarness` 的设置页 Light/Dark 渲染必须调用 `GameSaveCenterSettingsView.ApplyThemeForAudit(mode)`；否则会误测静态深色 DesignTokens，而不是运行时主题链路。
- UI-317 验证：源码/XAML 门禁、Release 全量构建测试、WPF 静态审查和多主题多尺寸 `render-qa OK` 均通过。真实 Playnite 宿主、DPI、Follow、高对比度和关闭玻璃回退仍需安装后人工复核。

## 2026-08-25 UI-314 当前事实：游戏背景增加真实模糊

- UI-313 解决了背景图重复绘制和横向矩形接缝，但原链路只有低透明度图片、主题 tint 和采样渐变，没有任何 `BlurEffect`，因此用户看到的仍接近清晰原图。
- `AcrylicProductionShellView` 的单一游戏背景矩形现在仅在 `HasSelectedGameBackgroundAmbientMaterial=True` 时挂 `GscGameBackgroundEffect`；卡片、文字、页面、列表和滚动内容不挂 BlurEffect。
- `AdaptiveThemePalette.ApplyMaterialResources` 按 `GlassEffectStrength` 生成冻结的 `BlurEffect`；UI-330 已改为直接比例半径 20–100 DIP，默认 78% 为 78 DIP，使用 `RenderingBias.Performance`。关闭毛玻璃、无背景图或高对比度时资源为真正的 null，不保留无效效果视觉。
- UI-314 验证：源码门禁、WPF 静态检查（0 error、18 条既有 warning）、Release 全量构建/测试和多主题多尺寸 `render-qa OK`。真实 Playnite 没有可控窗口，安装后需确认实际模糊观感与性能。

## 2026-08-24 UI-313 当前事实：背景图单层居中与底部接缝修复

- 截图中的横向矩形不是游戏原图被拼接，而是 Shell 和各页面内部的 `AmbientMaterialLayer` 同时绘制同一份游戏采样渐变；不同控件尺寸让相对坐标不同，形成可见接缝。
- `AmbientMaterialLayer.UseSelectedGameBackground` 默认关闭，只有生产 Shell 的 `ShellAmbientMaterialLayer` 开启游戏采样环境色；页面局部层在有游戏背景时仅保持透明，不再重复绘制图片材质或固定绿色洗色。
- `AcrylicProductionShellView` 现在用一个跨越 Shell 两行/两列的 `ImageBrush` 绘制真实背景，`UniformToFill`、`AlignmentX/Y=Center`、`TileMode=None` 明确保持比例、对称裁剪并禁止平铺；主题 tint 也覆盖同一完整区域，页脚不再切换到另一块背景。
- UI-313 验证：源码门禁、WPF 静态检查（0 error、18 条既有 warning）、Release 构建与测试通过；Core 59/59、Worker 199/199、Playnite 289/346（57 跳过）；多主题多尺寸 `render-qa OK`。真实 Playnite 没有可控窗口，仍需安装后用带不同宽高比背景的游戏人工确认裁剪中心。

## 2026-08-24 UI-312 当前事实：卡片表面与游戏背景自适应

- `OverviewTodayHeroCard` 不再嵌套整面背景 Border；它只使用共享 `GscRedesignSectionCard`，因此卡片本身是一层连续的阅读/玻璃表面，不能恢复“卡片里面再放一张全尺寸背景”的结构。
- `PlayniteGameBackgroundProvider.LoadVisualAsync` 返回真实 `ImageSource` 和从同一张位图采样出的冻结 `AmbientBrush`；采样只生成低 alpha 线性渐变，不替换真实图片。缓存仍为最多 6 张、后台最多 1920 宽度解码、远程 5 秒/12 MB 限制、取消和 generation 防串图。
- `AmbientMaterialLayer` 在 `DashboardViewModel.HasSelectedGameBackgroundAmbientMaterial` 为 true 时隐藏固定 `GscAmbientWideWashBrush`，显示 `SelectedGameBackgroundAmbientBrush`；没有背景图时才使用主题 accent/info/success 宽域洗色。这样游戏背景颜色跟随图片，不再固定 success 绿色。
- 当前背景图层透明度为深色/浅色 0.48/0.40，主题 tint alpha 为 0x52/0x66；关闭毛玻璃或高对比度仍让图片层和环境材质透明。不要把图片层改成内容卡片或对整页、表格、列表加 BlurEffect。
- UI-312 验证：源码门禁、WPF 静态检查和 render QA 通过；背景提供器定向测试 5/5；Release 全量测试为 Core 59/59、Worker 199/199、Playnite 289/346（57 跳过、0 失败）。当前 Playnite 无可控窗口，真实宿主背景切换仍需安装后人工切换两款有不同背景图的游戏确认。

## 2026-08-24 UI-311 当前事实：背景切换、Today 圆角与材质强度

- `OverviewTodayHeroCard` 内的宽域洗色必须使用带 `CornerRadius="12"` 的 Border；不要把整面 Rectangle 直接放进圆角卡片并依赖 `ClipToBounds`，WPF 不会按 CornerRadius 裁切子元素。
- `PlayniteGameBackgroundProvider` 优先解析 `Game.BackgroundImage` 的本地路径/数据库文件；如果 Playnite 返回 HTTP/HTTPS 直链，只为当前选中游戏异步下载，5 秒超时、12 MB 上限、取消令牌和后台 1920 宽度解码仍必须保留。快速切换由 generation 和 6 张缓存保护。
- `GscGameBackgroundOpacity` 当前深色/浅色为 0.36/0.28，背景 tint alpha 当前为 0x98/0xA8；`GscAmbientWideWashBrush` 的受限 alpha 也略有提升。若继续调强，优先保持大范围均匀和文字对比度，不要恢复圆形光斑或大面积 BlurEffect。
- UI-311 已完成源码/XAML/WPF 门禁、Release 全量构建测试和多主题多尺寸 `render-qa OK`；Playnite 测试为 288/345（57 跳过），真实 Playnite 背景切换仍需用户安装新包后切换两款有不同背景图的游戏确认。

## 2026-08-24 UI-310 当前游戏背景图环境材质

- `DashboardViewModel.SelectedGameBackground` 由 UI-only `PlayniteGameBackgroundProvider` 从 Playnite `Game.BackgroundImage` 读取；优先解析本地缓存/数据库文件，UI-311 已补齐受限的 HTTP/HTTPS 异步下载。
- 背景图在后台线程按最多 1920 宽度解码，缓存最多 6 张；远程请求仅限当前选中游戏并带 5 秒超时、12 MB 上限，选中游戏切换时使用取消令牌和 generation 丢弃旧结果。不要在 Playnite UI 线程同步解码或无限制下载。
- `AcrylicProductionShellView` 在 Shell 底层绘制低透明度背景图和主题 tint；无图、关闭毛玻璃、高对比度时图片层透明，继续使用 `GscBackdropBrush` 和 `GscAmbientWideWashBrush`。
- 默认背景与主题挂钩：`AdaptiveThemePalette` 依据 Playnite 主题/固定浅深色模式生成默认背景、tint 和宽域材质。背景图只作为环境素材，不覆盖卡片/导航的功能层级。
- UI-310 已完成源码/XAML/WPF 门禁、Release 构建/全量测试和多主题多尺寸 `render-qa OK`；当前用户 Playnite 未被强制重启，真实宿主背景图显示仍需用户在更新后复核。

## 2026-08-24 UI-309 当前事实：全局宽域多色玻璃材质

- `AmbientMaterialLayer.xaml`、Overview Today 卡片、Settings 环境层和兼容 `DashboardView.xaml` 都使用 `GscAmbientWideWashBrush` 的整面 `Rectangle`；生产 UI 不再使用装饰性 `RadialGradientBrush`、椭圆光源或 `GscAmbientBlurEffect`。
- `GscAmbientWideWashBrush` 是按当前主题动态生成的六段对角线性渐变，颜色从 accent/info 过渡到 teal/success 和中性表面，覆盖大范围但保持低饱和、低透明度；`GlassStrength` 只提升受限 alpha。
- `GscAmbientAccentBrush`、`GscAmbientInfoBrush`、`GscAmbientSuccessBrush` 及旧的环境阴影色 token 已删除。状态圆点、图标填充和语义状态色不属于装饰光斑，继续保留。
- 关闭毛玻璃或高对比度时必须返回透明渐变；不要把真实 `BlurEffect` 提升到导航栏、页面根、表格、列表或滚动面。此处是嵌入式 WPF 下的低成本整面渐变模拟，不是宿主桌面像素级 backdrop blur。
- UI-309 验证：源码/XAML/WPF 静态门禁通过，Release 0 warning/0 error，Core 59/59、Worker 199/199、Playnite 283/283（57 跳过），多主题多尺寸 `render-qa OK`；已打包并核对当前用户 Playnite 扩展 `0.6.70.0`。本轮 Computer Use 仅返回 `EmptyWindowAutomationPeer`，未宣称真实宿主页面点击/截图已复核。

## 2026-08-24 UI-308 当前事实：导航栏连续材质与宽域环境洗色

- 生产 Shell 的 `SidebarLayout` 现在统一承载 `GscSidebarMaterialBrush`；标题区和导航内容区使用透明背景，保证整个 236 DIP 导航栏只绘制一层连续对角材质。
- `GscSidebarMaterialBrush` 是动态的低成本半透明线性渐变，不能改回右侧透明渐隐、硬分割线或单独的 `SidebarSeamMaterial`；边界保持正常表面，避免再次出现亮柱。
- `AmbientMaterialLayer` 的第一层是 `GscAmbientWideWashBrush` 线性渐变矩形，负责大范围非圆形洗色；本条记录的旧版本曾保留固定椭圆，当前以 UI-309 为准。
- 宽域洗色运行时按 accent/info/success 和 `GlassStrength` 生成；关闭毛玻璃或高对比度时必须返回透明渐变，不能留下大面积遮罩。
- UI-308 验证：Release 0 warning/0 error；Core 59/59、Worker 199/199、Playnite 283/283（57 跳过）；多主题多尺寸 `render-qa OK`；真实 Playnite `0.6.70.0` 已安装并截图复核导航材质和宽域洗色。

## 2026-08-24 UI-307 当前事实：首页圆角外溢与导航材质均匀化

- `OverviewTodayHeroCard` 的装饰椭圆必须保持在卡片内部；不要恢复 `Margin="-112..."` 等负边距。WPF `ClipToBounds` 是矩形裁切，不会按 `CornerRadius` 裁切，负边距会在圆角外留下直角色块。
- `AmbientMaterialLayer.ShowLeftGlow` 默认是 `true`，页面局部层继续显示左侧环境光；生产 `ShellAmbientMaterialLayer` 必须设置 `ShowLeftGlow="False"`，否则主内容列起点会再次出现竖向亮带。
- 导航栏使用贯穿整栏的中性 `GscSidebarMaterialBrush`，不得恢复透明渐隐到右边缘，也不要重新加入 `SidebarSeamMaterial`、`GscSidebarSeamBrush` 或右侧硬边框。当前材质通过低对比度的整栏渐变体现玻璃感，边界保持均匀。
- Blur 仍只挂在固定尺寸环境光椭圆上；真实导航命令、页面 Binding、滚动/虚拟化、关闭毛玻璃/高对比度降级语义不变。
- UI-307 验证：源码/XAML/WPF 静态门禁通过，Release 0 warning/0 error，Core 59/59、Worker 199/199、Playnite 283/283（57 跳过），多主题多尺寸 `render-qa OK`；真实 Playnite 已安装并截图复核 `0.6.70.0`。

## 2026-08-24 UI-306 历史记录（当前以 UI-307 为准）：Shell 毛玻璃覆盖与导航栏过渡

- 生产 Shell 的 `ShellAmbientMaterialLayer` 当前必须位于 Shell 背景之上、`SidebarLayout`/主内容 Grid 之下，覆盖两列；不要恢复为 `Panel.ZIndex=-1`，否则会被 Shell 背景压住，用户看不到环境光。
- 导航栏右侧硬分割线已移除；`SidebarSeamMaterial` 只负责低成本不可交互的渐隐过渡，不能恢复 `BorderThickness="0,0,1,0"`。导航栏仍使用半透明 `GscSidebarBrush`，导航项、命令、页面切换、列宽和滚动均未改变。
- `GscShellAmbientOpacity` 按 `GlassStrength` 计算，`GscSidebarSeamBrush` 由当前主题和 accent 动态生成；Blur 仍只挂在固定 `AmbientMaterialLayer` 装饰椭圆上，不能对整个 Shell、页面、表格或滚动器加 Blur。
- UI-306 验证：源码门禁、XAML 结构、WPF 静态审查、Release 构建/测试和 `artifacts/ui-qa/shell-glass-final2/render-qa-report.txt` 均通过；真实 Playnite 已安装 `0.6.70.0` 并实际查看首页、存档中心、修改器中心。沙箱直接安装失败是外部 Roaming 目录 ACL 限制，受控当前用户权限安装成功。

## 2026-08-24 UI-305 当前事实：任务表格底部滚动安全区

- `TaskCenterView.xaml` 的 `TaskDataGrid` 使用 `Padding="0,0,0,12"`，为 Playnite 宿主可能覆盖最后一行的水平滚动条保留内部底部安全区；不要通过关闭横向滚动或关闭 `DataGridStarFill` 修复，否则会破坏列可见性和旧的回拉滚动契约。
- 任务表仍保持横向 `Auto`、`ScrollViewer.CanContentScroll=True`、`VirtualizingPanel.ScrollUnit=Item`、Recycling 和真实列宽/Binding/命令。
- RenderHarness 的任务探针使用 500 条数据，明确测试到底→回顶→再到底→中段→回顶，并比较最后一行底部与水平滚动条顶部；任务表无效行、底部覆盖或回拉异常都必须失败。
- UI-305 验证：源码门禁、XAML/WPF 静态检查、Release 0 warning/0 error、Core 59/59、Worker 199/199、Playnite 283/283（57 跳过）及 `artifacts/ui-qa/task-bottom-final/render-qa-report.txt` 均通过。真实 Playnite 逐像素宿主表现仍需用户复核。

## 2026-08-24 UI-304 当前事实：毛玻璃强度联动与固定环境光增强

- `AdaptiveThemePalette` 现在保存规范化 `GlassStrength`；`ApplyAccentResources` 使用它增加固定 accent/info/success 环境光及中心柔光的 alpha，`ApplyMaterialResources` 使用它计算 `GscAmbientPageOpacity`。100% 不再只是让卡片更不透明。
- `AmbientMaterialLayer.xaml` 目前有四个固定尺寸环境光椭圆，新增 `GscAmbientCenterShadowColor`。Blur 仍只附着到这四个装饰椭圆，半径为 24、`RenderingBias.Performance`；严禁把 Blur 提升到页面根、文字、表格、列表或滚动表面。
- 高强度玻璃表面的 alpha 采用受控透光曲线（强度越高越能透出固定环境光），低强度保持稳定阅读底色；`EnableGlassEffects=false`、高对比度和真实 null 效果降级语义不变。
- UI-304 验证：源码门禁、XAML 结构、WPF 静态审查和临时开启 100% 的主题离屏 QA 通过；RenderHarness 已恢复为无毛玻璃布局基线，避免测试夹具污染正式代码。Release 构建 0 警告/0 错误，Core 59/59、Worker 199/199、Playnite 282/282（57 跳过），标准 `render-qa` 为 `render-qa OK`；生产安装已核验 `0.6.70.0`。真实 Playnite 逐像素强度仍需用户本机复核。

## 2026-08-24 UI-303 当前事实：TextBox 内容宿主重复 Padding 修复

- 任务中心输入文字裁切的根因是模板将 `TextBox.Padding` 绑定到 `PART_ContentHost.Margin`，而 WPF TextBox/宿主又会对内容宿主应用自身 Padding；Task 搜索框上下 `7 DIP` 被重复计算，导致 `PART_ContentHost` viewport 只有 `5 DIP`。不要恢复 `Margin="{TemplateBinding Padding}"`，也不要靠增加 TextBox 高度规避。
- `WpfUiProduction.xaml` 与 `DesignTokens.xaml` 的 TextBox `PART_ContentHost` 当前固定 `Margin="0"`、`Padding="0"`、`BorderThickness="0"`，输入起点由 TextBox 的 Padding 单独负责；字体、字号、字重和前景色均从 TextBox 显式传入内容宿主。
- 当前共享 TextBox 样式使用 `GscUiFontFamily`、`GscBodyFontSize` 和 `FontWeight=Normal`。Task/Dashboard 图标搜索框的 `30 DIP` 左侧输入区、右侧清除按钮区、Binding、命令和焦点语义保持。
- UI-303 输入态验证：离屏 RenderHarness 注入“存档”后文字完整显示，`PART_ContentHost` viewport 从 `5` 恢复为 `19 DIP`；空态/输入态 render QA 均通过。Release 0 warning/0 error、Core 59/59、Worker 199/199、Playnite 282/282（57 跳过），生产安装核验 DLL `0.6.70.0`；真实 Playnite 逐像素截图仍需用户复核。

## 2026-08-24 UI-302 当前事实：任务中心搜索框可见性与图标间距

- TaskCenter 的“搜索任务…”和 Dashboard 的游戏库搜索框是带放大镜的特殊输入框，当前 `TextBox.Padding` 与占位提示 `Margin` 均为 `30,7,38,7` / `30,0,38,0`（Task 提示右侧为 `12`）；`30` 是图标与实际文字之间的独立安全间距。普通无图标输入框仍按共享样式的常规左侧内边距处理。
- `WpfUiProduction.xaml` 与 `DesignTokens.xaml` 的 TextBox 模板均要求 `PART_ContentHost` 显式设置 `TextElement.Foreground="{TemplateBinding Foreground}"`，确保输入文字继承 Gsc 前景色；不要仅靠宿主默认 TextBox 主题推断文字颜色。
- 右侧清除按钮、现有 Binding、搜索过滤、命令、键盘焦点和无障碍名称未改变。UI-302 只修复输入可见性与图标间距。
- UI-302 验证：源码验证、XAML 19/19、WPF 静态审查 0 error/20 warnings/165 info、Release 0 warning/0 error、Core 59/59、Worker 199/199、Playnite 282/282（57 跳过）、`artifacts/ui-qa/ui302-task-search-v1/render-qa-report.txt` 为 `render-qa OK`；真实 Playnite 逐像素输入截图仍需用户复核。

## 2026-08-24 UI-301 当前事实：表格右侧安全边距与搜索输入起点

- 共享 `WpfUiProduction.xaml` 的 `GscRoundedDataGridRowTemplate` 和 `DashboardView.xaml` 的本地兼容行模板，选中 `RowChrome` 使用 `4,2,12,2`；`12` 是为 Playnite 宿主垂直滚动轨道保留的右侧安全区。不要恢复到 `4,2,8,2` 或旧的 `4,2`。
- UI-301 曾将 TaskCenter 与 Dashboard 游戏库搜索框的图标字段收紧到 `20` DIP；UI-302 已将当前值修正为 `30` DIP，提示文本同步从 `30` 起始，右侧 `38` DIP 仍为清除按钮预留区。共享普通 TextBox 的左对齐模板、数值输入的专用对齐和清除按钮行为不变。
- UI-301 验证：`validate-source.py`、XAML 19/19、WPF 静态审查 0 error/20 warnings/165 info、Release 0 warning/0 error、Core 59/59、Worker 199/199、Playnite 282/282（57 跳过）、`artifacts/ui-qa/ui301-spacing-v1/render-qa-report.txt` 的 `render-qa OK`；受控一键安装已核验生产扩展 `0.6.70.0`。真实 Playnite 宿主逐像素边距仍需用户复核。

## 2026-08-24 UI-300 / FUNC-004 当前事实：表格、输入框与 FLiNG 归档

- `WpfUiProduction.xaml` 的共享 `GscRoundedDataGridRowTemplate` 选中描边使用 `4,2,12,2` 安全边距，`DashboardView.xaml` 的本地兼容行模板同步；不要把右边距恢复为 `4,2,8,2` 或 `4,2`，否则宿主垂直滚动轨道可能覆盖右侧圆角。共享行的排序、SelectiveScrollingGrid、Item 滚动和 Recycling 不变。
- `GscWpfUiTextBox` 与 `GscTextBox` 的普通文本默认 `HorizontalContentAlignment=Left`、`TextAlignment=Left`，模板把对齐属性传给 `PART_ContentHost`；数值输入专用样式仍可覆盖对齐方式。普通搜索框通过 `GscSearchClearButton` 在非空时显示清除动作并清空后恢复焦点。
- `AcrylicProductionShellView`、`MediaCenterView`、`TaskCenterView`、`TrainerCenterView` 的搜索清除按钮均只影响输入值和焦点，不改绑定/命令；Trainer 的响应式宽度现在作用于 `TrainerSearchBoxHost`，避免按钮被宽度赋值挤出输入框。
- `FlingTrainerCatalogSource` 从 `https://archive.flingtrainer.com/` 有界 BFS 解析 `.zip`、`.rar`、`.7z`、`.exe` 归档直链，并保留同主机 HTTPS、目录/文件上限和可降级在线目录刷新。`GameToolService` 先保留 ZIP/direct EXE 路径，RAR/7z 使用 SharpCompress reader 流式写入临时版本目录，通过 `ArchivePathGuard` 与 1 GiB 单文件/4 GiB 总展开限制后再选择修改器 EXE；不执行下载内容。
- `SharpCompress` 版本由 `Directory.Packages.props` 统一锁定为 `0.50.4`。本阶段验证：源码门禁通过，XAML 19/19，WPF 0 error/20 warnings/165 info，Release 0 warning/0 error，Core 59/59、Worker 199/199、Playnite 282/282（57 跳过），`artifacts/ui-qa/ui300-input-fling-v1/render-qa-report.txt` 为 `render-qa OK`。受控运行 `scripts/dev-install-run.ps1 -Configuration Release -NoStart` 已安装生产扩展并核验 `0.6.70.0`；沙箱内直接运行时的 Access denied 只来自受限环境写不了 Roaming Playnite 扩展目录。真实 FLiNG 归档下载/运行和安全软件拦截仍不由自动验证声称覆盖。

## 2026-08-24 UI-299 当前事实：表格字体与行表面可读性

- `DesignTokens.xaml` 新增 `GscTableRowBrush`；`AdaptiveThemePalette.ApplyRuntimeThemeResources` 和 `ApplyDemoCoreResources` 为浅/深主题提供低透明度行面/交替行面，高对比度保持透明降级。
- 共享 `WpfUiProduction.xaml` 的隐式 `DataGrid` 与 `Redesign.xaml` 的 `GscRedesignWorkspaceDataGrid` 使用 `TextOptions.TextFormattingMode=Display`；`DataGridCell` 显式使用 `GscUiFontFamily`/`GscBodyFontSize`。不要把行底色改成不透明大色块，也不要把 Display 文本格式化扩展到大范围页面滚动器。
- `RowBackground`、隐式/稳定 `DataGridRow` 的正常背景仍来自动态 `GscTableRowBrush`，`AlternatingRowBackground`、Hover、选中态、表头排序箭头和列宽拖拽保持现有共享模板；不要移除 `SelectiveScrollingGrid`、Recycling、Item scrolling 或媒体收件箱的 Standard 虚拟化例外。
- UI-299 当前验证：源码验证、XAML 19/19、WPF 静态审查、Release 0 警告/0 错误、Core 59、Worker 198、Playnite 281 通过/57 跳过；`artifacts/ui-qa/table-readability-v1/render-qa-report.txt` 为 `render-qa OK`。
- 真实 Playnite 日志已确认隔离 Preview 后加载 `GameSaveCenter` 0.6.70；同一用户配置同时放置旧 Preview 时，Playnite 会先加载其 0.6.71 的同名 `GameSaveCenter.Contracts`，导致标准插件的 `MediaInboxBatchResultDto` 类型加载冲突。该冲突属于外部 Preview 安装状态，不要把 Preview 目录禁用动作当成源码修复；本阶段未在 Computer Use 中点击宿主页面，因为返回窗口无真实 HWND，避免把 Codex 截图误当 Playnite。
- 2026-08-24 复核：改由 Windows 正常启动 Playnite 后，启动页短暂出现真实 HWND，但主窗口显示后 `MainWindowHandle` 又回到 0；Computer Use 重新选窗、激活、Raise 后仍返回 Codex 截图和 `EmptyWindowAutomationPeer`。未发送点击或滚动输入；这属于当前宿主/Computer Use 窗口映射限制，不是 UI 代码通过后的失败。Preview 已用 238 个文件恢复原路径，Playnite 与标准 Worker 均已停止。
- 同轮新增真实 WPF 操控证据：临时宿主直接加载生产 `SaveCenterView`、当前主题资源和 160 行 `DataGrid`；Computer Use 实际切换“历史版本”标签、选择第二行、向下滚动再滚回，选中行高亮、右侧详情和表格字体/行面均保持正确。临时宿主及构建输出已清理，正式源码未被测试夹具污染。

## 2026-08-23 UI-298 当前事实：安全毛玻璃高光与环境光 Blur

- `DesignTokens.xaml` 的 `GscAmbientBlurEffect` 默认必须是 `x:Null`；`AdaptiveThemePaletteFactory.ApplyMaterialResources` 仅在 `glassEnabled` 时创建半径 18、`RenderingBias.Performance` 的冻结 `BlurEffect`，关闭毛玻璃或高对比度时返回真实 null。
- `AmbientMaterialLayer.xaml` 只把该效果挂到三个固定尺寸的装饰椭圆；禁止把它提升到根 Grid、页面内容、TextBlock、DataGrid、ListBox 或任何大范围滚动容器，否则会破坏性能和可读性。
- `AcrylicProductionShellView.xaml` 的玻璃高光是 1 DIP、`IsHitTestVisible=False` 的装饰 Border，不参与布局输入；`ApplyRuntimeThemeResources` 在 `glassEnabled=false` 时将 `GscGlassHighlightBrush` 置为透明。
- 现有 `EnableGlassEffects`、高对比度、页面环境光隐藏和不透明主题表面降级语义不变；不要引入 Windows 宿主级 Backdrop、修改 Playnite WindowChrome 或对整个插件做 Blur。
- UI-298 验证：WPF 资源定向测试 2/2、源码验证通过；WPF 静态审查 0 error/21 warnings/164 info；`artifacts/ui-qa/glass-blur-v1/render-qa-report.txt` 为 `render-qa OK`；Release 解决方案 0 warning/0 error。尚未在真实 Playnite 宿主逐像素验证。

## 2026-08-23 FUNC-003 当前事实：FLiNG 历史归档递归解析

- `FlingTrainerCatalogSource.SyncCatalogAsync` 先解析在线目录，再通过 `GetArchiveCatalogAsync` 从 `https://archive.flingtrainer.com/` 以有界 BFS 递归读取归档目录；每个目录只允许继续访问同一归档主机的 HTTPS 子目录。
- 归档文件只接受 `.zip` 和 `.exe`，每次扫描最多 2048 个目录、10000 个文件，结果按 `PageUrl` 去重后写入现有 Trainer catalog；FLiNG 的旧归档文件仍通过现有单文件 release 路径下载。
- `ParseArchiveDirectoryListing` 同时返回文件和子目录，必须保留相对路径解析、外部主机过滤和目录去重；不要把归档递归改成无界网络爬取，也不要放开非 FLiNG 主机。
- 同步日志格式包含总数、在线目录数和归档目录数，例如 `Synchronized ... FLiNG catalog entries (... online, ... archive)`，便于确认 2019 年以前条目是否实际进入本地目录。
- FUNC-003 当前验证：Release 解决方案构建 0 warning/0 error；Core 59/59、Worker 198/198 通过，FLiNG 解析定向测试 3/3 通过。尚未在真实 FLiNG 归档站点和真实 Playnite 宿主中做网络/宿主验收；本阶段没有 UI/XAML 改动。

## 2026-08-23 PERF-001 当前事实：大型库 Worker 预热与 Dashboard 版本探测解耦

- `GameSaveCenterPlugin.OnApplicationStarted` 对 100+ 游戏库会后台调用 `StartWorkerAndScheduleSynchronizationAsync` 预热 Worker；`StartWorkerAndScheduleSynchronizationAsync` 在大型库仍会在 `interactiveSurfaceOpened == false` 时直接返回，不得提交全库 `UpsertGames`/Ludusavi 匹配。
- `WaitForLibraryReadyAndStartWorkerAsync` 在库稳定为大型库后同样预热 Worker；这只提前完成 Worker/SQLite 初始化，不改变 500+ 大库的缓存优先和显式刷新门禁。
- `DashboardService.GetAsync` 不再等待 `ludusavi --version`；首次快照读取内存缓存并通过 `RefreshLudusaviVersionAsync` 后台探测，版本结果在后续快照显示。不要把版本探测重新放回首个 Dashboard IPC 请求。
- `WorkerInitializationService` 记录 storage、stale-task reconciliation、snapshot cleanup 和总耗时；这些日志用于继续定位用户机器上的慢启动阶段。
- PERF-001 验证：Release 解决方案构建 0 warning/0 error；新增定向 Playnite 源码测试通过。真实 Playnite 宿主中的预热时序仍待用户机器实测。

## 2026-08-23 FUNC-002 当前事实：媒体收件箱忽略恢复

- `MediaCenterView.xaml` 的待归类页在既有 `MediaInboxBatchActionRow` 内增加 `MediaInboxMode` 轻量视图切换，选项为“待归类”和“已忽略”。默认仍只加载 `ListUnassignedMedia`；切换到“已忽略”后才按需请求 `ListIgnoredMedia`，避免旧 Worker 在正常启动路径上因未知新消息而受影响。
- `MediaInboxItems` 是 DataGrid 的当前显示集合，`UnassignedMedia` 和 `IgnoredMedia` 分别保留两种缓存。已忽略模式隐藏目标游戏、归类和忽略命令，只显示 `RestoreIgnoredMediaBatchCommand`；切回待归类时恢复原有单条/批量归类操作。不要把已忽略项目重新接回 `AssignInboxMediaCommand` 或 `IgnoreInboxMediaCommand`。
- Worker 新增 `media.inbox.ignored.list` 与 `media.inbox.ignored.restore.batch`。恢复批次最多 500 个去重 ID，逐项复用安全文件移动：优先使用现有归档副本，否则从原始文件重建副本；目标已存在时必须做 SHA-256 相同校验，禁止覆盖不同内容。数据库状态变为 `Inbox`，PlayniteId 清空，CloudState 为 `NotApplicable`，原因固定为“用户撤销忽略，待重新归类”，并追加审计。
- 恢复完成后 Playnite 同时刷新待归类和已忽略列表，避免用户切换视图看到过期缓存。当前视图切换不持久化，页面重新打开默认进入待归类。
- RenderHarness 的 `FakeDashboardData` 通过 `MediaInboxItems => UnassignedMedia` 投影兼容生产绑定，保留 4468 行收件箱虚拟化/回顶探针，不要因为新增显示集合把夹具改回空列表。
- FUNC-002 验证：`validate-source.py`、XAML 19/19、Release 0 warning/0 error；Core 59、Worker 196、Playnite 280 通过/57 跳过；`artifacts/ui-qa/media-inbox-restore-v1/render-qa-report.txt` 为 `render-qa OK`；WPF 审查 0 error、21 warnings、164 info。没有在真实 Playnite 中执行恢复操作，宿主 DPI、高对比度与真实点击仍待人工验收。

## 2026-08-23 FUNC-001 当前事实：媒体收件箱批量处理与侧栏真实状态

- `MediaCenterView.xaml` 的 `MediaInboxGrid` 现在允许 `Extended + FullRow` 多选；表格上方新增 `MediaInboxBatchActionRow`，仅提供目标游戏 ComboBox、`归类所选` 和 `忽略所选`，原有 Inspector 与单条归类/忽略命令保持不变。`MediaInboxGrid` 仍保留既有 `Standard` 行虚拟化、关闭列虚拟化和 `GscDataGridStarFill` 例外，不要为批量操作恢复 star-fill 或改掉滚动模型。
- 新 IPC 类型为 `media.reassign.batch` 与 `media.inbox.ignore.batch`，请求在 Worker 侧最多 500 个去重媒体 ID；Playnite 对更大选择自动分批，每批使用较长请求超时。Worker 逐项复用现有归类/忽略逻辑，保留归档副本和审计记录，失败项通过 `MediaInboxBatchResultDto.Failures` 返回，取消不吞掉。
- 批量归类复用 `InboxTargetGame`，忽略仍需一次安全确认；批量完成后刷新 Dashboard 和 Inbox，部分失败在状态栏和错误提示中显示首条错误。不要把单批上限扩大到无界，也不要改成前端逐项发起数千个 IPC 请求。
- `AcrylicProductionShellView.xaml` 的 Worker/Ludusavi 指示灯和状态文字由 `Snapshot.WorkerHealthy`/`Snapshot.LudusaviAvailable` 的 DataTrigger 驱动，显示“正常/不可用”“可用/不可用”，不再显示原始布尔值。
- FUNC-001 验证：`validate-source.py` 与 XAML 19/19 通过；Release 0 warning/0 error；Core 59、Worker 194、Playnite 280 通过/57 跳过；`artifacts/ui-qa/media-inbox-batch-v1/render-qa-report.txt` 为 `render-qa OK`；WPF 审查 0 error、21 warnings、164 info。未执行真实 Playnite 批量数据变更、宿主 DPI 或高对比度人工验收。

## 2026-08-22 UI-297 当前事实：页面激活时同步运行中游戏

- 旧行为是：Dashboard 首次快照按 `GameSelectionResolver` 选择运行中游戏，页面已打开时由 `PlayniteGameStarted` 事件切换；普通刷新保留用户手动选择。Worker 进程检测首轮是基线扫描，因此 Worker 在游戏已经运行后才启动时，快照可能暂时没有该会话。
- `GameSaveCenterPlugin.TryGetCurrentlyRunningPlayniteGameIds()` 只读 Playnite SDK 的 `Game.IsRunning`，不启动 Worker 会话、不发 `GameSessionStarted`、不扫描进程、不触发备份或其他自动化。
- `DashboardView` 在 `Loaded` 和重新变为可见时调用 `DashboardViewModel.SelectCurrentlyRunningGameOnViewActivation()`；快照异步晚到时也会调用一次。该方法覆盖当前 DTO 的运行状态、刷新 GamePicker 缓存行并按既有 resolver 规则选择运行中游戏；没有运行中游戏时继续保留用户上次选择。
- `GamePickerItem` 实现 `INotifyPropertyChanged` 以支持不替换缓存对象时刷新运行状态；GamePicker 仍然虚拟化/本地筛选，未新增定时器、Worker IPC 轮询或网络请求。
- UI-297 验证：`scripts/validate-source.py` 通过；隔离 Release 构建 XAML 19/19、0 warning/0 error；新增页面激活自动定位源码测试通过；WPF 静态审查 0 error、21 warnings、164 info。完整 Playnite 测试当前分支仍有 19 个既有 Demo/布局断言失败、240 通过、62 跳过，未归因于本改动；真实 Playnite 宿主需启动后验证页面反复打开/切换时的实际选框。

## 2026-08-22 UI-296 当前事实：任务/媒体摘要条实际宿主布局

- `TaskCenterView.xaml` 与 `MediaCenterView.xaml` 的摘要条当前统一使用 `* / Auto / * / Auto / * / Auto / *` 七列；四个统计块位于 `0/2/4/6`，三条竖线位于 `1/3/5`，最后一个统计块右侧不能有 Rectangle。`Auto` 分隔列与首页的 `OverviewStatStrip` 保持同一布局契约，避免恢复旧的四列重叠结构。
- 用户截图中的错位来自实际宿主仍加载旧的四列布局；本轮已通过 `scripts/dev-install-run.ps1 -Configuration Release -NoStart` 将当前 `0.6.70.0` 安装到标准 Playnite 扩展目录。该安装过程没有读取或假定 Demo 文件夹，也不会停止 `GameSaveCenterPreview` 的其他 Worker。
- UI-296 验证：源码门禁通过；Release 为 0 warning/0 error；Core 59、Worker 194、Playnite 277/57/0；`artifacts/ui-qa/summary-divider-layout-fix/render-qa-report.txt` 为 `render-qa OK`，双主题和多尺寸回归通过。真实 Playnite 已安装新 DLL，但本轮未自动捕获嵌入页面像素，需用户启动后复核截图。

## 2026-08-22 UI-295 当前事实：媒体摘要块后的竖线契约（已由 UI-296 统一为 Auto 分隔槽）

- `MediaCenterView.xaml` 的 `MediaSummaryPanel` 使用 7 列 `* / Auto / * / Auto / * / Auto / *`；四个真实统计块在 `0/2/4/6`，三条竖线在 `1/3/5`。不要把 Rectangle 和统计 StackPanel 放进同一列，也不要在最后一个统计块右侧增加竖线。
- `MediaSummary.TotalCount`、截图/录像数量、`TotalSizeDisplay`、`FavoriteCount` 和 `Snapshot.UnassignedMediaCount` 的真实 OneWay Binding 与摘要文案保持不变；这只是几何布局修复。
- UI-295 验证：源码门禁通过；Release 为 0 warning/0 error；Core 59、Worker 194、Playnite 277/57/0；`artifacts/ui-qa/media-summary-divider-fix/render-qa-report.txt` 为 `render-qa OK`，亮/暗主题媒体摘要已抽查。RenderHarness 是离屏证据，不等同真实 Playnite 宿主逐像素验收。

## 2026-08-21 UI-294 当前事实：任务统计分隔线与全工作区环境光（摘要列已由 UI-296 统一为 Auto 分隔槽）

- `TaskCenterView.xaml` 的任务统计条固定为 7 列：四个 `*` 统计列与三个 `Auto` 分隔列交替；分隔线位于分隔列并居中，不能恢复为与统计项共用列。
- `Controls/AmbientMaterialLayer.xaml` 是共享的、无 BlurEffect 的环境光层，已放在 Overview、Save、Trainer、Media、Task、Maintenance 六个页面的真实内容之后；Settings 页使用相同的自适应环境光颜色。它不改变页面尺寸、滚动器、命令、Binding 或虚拟化列表。
- `AdaptiveThemePaletteFactory.ApplyMaterialResources` 提供 `GscAccentShadowColor`、`GscInfoShadowColor`、`GscSuccessShadowColor` 和 `GscAmbientPageOpacity`；透明效果关闭或高对比度时环境光层隐藏。该方案是安全的渐变材质，不应改成对整页或 DataGrid 使用 BlurEffect。
- UI-294 验证：源码门禁通过；Release 为 0 warning/0 error；Core 59、Worker 194、Playnite 276/58/0；`artifacts/ui-qa/task-ambient-final5/render-qa-report.txt` 为 `render-qa OK`，覆盖双主题、多尺寸、Tab、滚动和 resize。RenderHarness 是离屏证据，不等同真实 Playnite 宿主逐像素验收；Demo 文件夹不是运行时或测试依赖。

## 2026-08-21 UI-293 当前事实：首页关注事项与比较质量状态

- `OverviewView.xaml` 的 `AttentionFindings` 行使用 `26` DIP 图标列、`*` 标题列和 `220` DIP 建议列；建议列不能恢复为无约束 `Auto`，否则长 `SuggestedAction` 会把标题/游戏名挤成单字宽。真实 `SuggestedAction` 仍右对齐并用省略号，完整内容通过 ToolTip 可读。
- `SaveCenterView.xaml` 的“版本比较”标题行使用明确的标题/气泡两列布局；标题、`GscRedesignContextPill` 和内部文本都设为垂直居中。`LastBackupDiff.ComparisonQualityDisplay` 同时使用 `TargetNullValue=等待比较` 和 `FallbackValue=等待比较`，比较前不应出现空色块；有实际 DTO 时仍显示 Worker 返回的真实质量。
- `DashboardViewModel.diffSummary` 初始值为“选择两个版本后，比较结果会显示在这里。”，比较前的结果区域不能恢复为空字符串；执行比较后仍由 `diff.Summary` 覆盖。
- UI-293 验证：Release 构建为 XAML 18/18、0 warning/0 error；Core 59、Worker 194、Playnite 276/58/0；`artifacts/ui-qa/attention-pill-fix/render-qa-report.txt` 为 `render-qa OK`，覆盖双主题、多尺寸、Tab、滚动和 resize。RenderHarness 是离屏证据，不等同真实 Playnite 宿主逐像素验收；Demo 文件夹不是运行时或测试依赖。

## 2026-08-21 UI-292 当前事实：设置路径输入框与生产壳顶部

- `WpfUiProduction.xaml` 的 `GscWpfUiTextBoxTemplate` 使用拉伸的 `PART_ContentHost`，正文通过 `VerticalContentAlignment=Center` 垂直居中；`GscWpfUiTextBox` 默认 `HorizontalScrollBarVisibility=Hidden`，避免长路径自动水平滚动条占用输入框底部。
- `DesignTokens.xaml` 的 `GscTextBox` 与 WPF-UI 适配器使用适合 36 DIP 控件的垂直内边距；不要恢复过大的上下 Padding，也不要给设置页路径字段重新加 `HorizontalScrollBarVisibility=Auto`，否则长路径会把文字视口压缩并裁切。
- `GameSaveCenterSettingsView.xaml` 的 Worker、Ludusavi、存档目录、Rclone、云端目标、媒体目录和镜像目录仍绑定真实设置属性，编辑/保存语义不变；横向滚动条只是隐藏，获得焦点后仍可编辑长路径。
- `AcrylicProductionShellView.xaml` 不再显示“主题 / 跟随 Playnite / 浅色 / 深色”顶部 utility surface，Header 直接使用右侧区域第一行；`AcrylicProductionShellView.xaml.cs` 与 `DashboardView.xaml.cs` 不再有壳主题按钮回调。主题选择仍由 Playnite 设置页的 `ThemeMode` 下拉框提供，动态调色板链未删除。
- UI-292 验证：长路径 STA 测量断言通过；Release 为 XAML 18/18、0 warning/0 error、Core 59、Worker 194、Playnite 276/58/0；最终 `artifacts/ui-qa/settings-theme-fix-final/render-qa-report.txt` 为 `render-qa OK`。RenderHarness 仍是离屏证据，不等同真实 Playnite 宿主逐像素验收。

## 2026-08-21 BUILD-003 当前事实：测试根目录不受外部 Demo 工作目录影响

- `WpfUiResourceDictionaryTests`、`RestoredAcrylicForkBaselineTests`、`NumericInputTests` 的仓库根目录探测只从 `AppContext.BaseDirectory` 向上查找 `GameSaveCenter.sln`；找不到时明确失败，不再信任进程当前工作目录。
- 因此即使一键安装器从 `D:\workplace\Github\GameSaveCenter.AcrylicFork` 启动，测试也会读取当前隔离构建输出对应的 `GameSaveCenter` 仓库，不会假定外部 Demo 目录存在。
- BUILD-003 验证：外部 Demo 目录作为当前工作目录时，当前仓库 Playnite 测试 276/58/0；完整 Release 构建为 XAML 18/18、Core 59、Worker 194、Playnite 276/58/0；没有新增 Demo 路径、环境变量或生产运行时依赖。

## 2026-08-21 UI-291 当前事实：媒体来源规则宽窄布局

- `MediaCenterView.xaml` 的来源规则页使用 `MediaSourceLayout`：宽屏为 1.1* 表单、14 DIP 间距、* 规则列表；窄屏由 `ApplyResponsiveLayout` 改为表单在上、规则列表在下，字段从两列收为一列。
- 表单默认可见，真实 `CustomMediaSourcePath`/`CustomMediaPattern`/`CustomMediaShared` 绑定及 `AddMediaSourceCommand` 保留；规则列表继续使用 `MediaSources`、更新/移除命令、内部 Auto 滚动、行虚拟化和空状态。
- 媒体待归类页的 Inspector、目标游戏、归类/忽略/批量操作和大数据表格虚拟化契约未改动；窄屏只给来源表单和规则列表各自有限视口。
- UI-291 证据：当前最终审计 Release 0 warning/0 error；Core 59、Worker 194、Playnite 276/58/0；`artifacts/ui-qa/ui-final-audit/render-qa-report.txt` 为 `render-qa OK`，双主题、多尺寸和 resize 通过。未宣称真实 Playnite 生产宿主逐像素验收，Demo 文件夹不是运行时或测试依赖。

## 2026-08-21 UI-290 当前事实：首页信息层级

- `OverviewPrimaryFlow` 的固定行契约是：`OverviewHeroAndGameRow` 行 0、`OverviewStatStrip` 行 1、`OverviewHomeToolbar` 行 2、`OverviewActivityColumn` 行 3/4；`OverviewSecondaryScrollViewer` 仍由响应式代码在宽屏并列、窄屏后置。
- 首页辅助 toolbar 仍保留 `RefreshCommand`、`BackupAllCommand`、`SyncMediaCommand`、`OpenAttentionCenterCommand` 和环境检查 `OpenMaintenanceCommand`；只是视觉层级后置，不得删除这些真实入口。
- UI-290 验证：源码门禁通过；Release 0 warning/0 error；Core 59/59、Worker 194/194、Playnite 276/58/0；`artifacts/ui-qa/overview-order-ui288/render-qa-report.txt` 为 `render-qa OK`。
- 该 RenderHarness 报告只能作为离屏多尺寸/双主题回归，不能替代 Playnite 宿主像素截图或高 DPI/高对比度人工验收。

## 2026-08-21 UI-289 当前事实：共享控件尺寸

- 普通按钮/输入/ComboBox 的共享基准是 `GscButtonHeight=36`；紧凑按钮使用独立 `GscCompactButtonHeight=30`，不能在页面上重新写回 38 DIP。
- `DesignTokens.xaml` 的 `GscTextBox`、`GscNumericFieldInput`、`GscComboBox`，以及 `WpfUiProduction.xaml` 的 WPF-UI TextBox/ComboBox/按钮适配器都引用普通动态令牌；`GscWpfUiCompactButton` 只引用紧凑令牌。
- 首页保护动作仍保留显式普通高度，因为它是高风险操作例外；普通/紧凑工具栏和存档操作不再通过页面 `MinHeight` 覆盖共享资源。
- UI-289 验证：源码门禁通过；Release 0 warning/0 error；Core 59/59、Worker 194/194、Playnite 275/59/0；WPF validator 0 error、20 warnings、164 info。

## 2026-08-21 UI-288 当前事实：生产壳主题操作与设置入口

- `AcrylicProductionShellView` 的 44 DIP 顶部 utility band 现在是可操作的主题条，不是空白占位；三个 RadioButton 分别对应 `GameSaveCenterThemeMode.FollowPlaynite/Light/Dark`，样式位于 `AcrylicProductionResources.xaml` 的 `AcrylicThemeModeItem`。
- `DashboardView` 给生产壳注入两个真实回调：设置项调用 `plugin.PlayniteApi.MainView.OpenPluginSettings(plugin.Id)`；主题项写入 `plugin.Settings.ThemeMode`、保存设置、触发 `NotifyVisualSettingsChanged()`，并沿用 `AdaptiveThemePaletteFactory` 的动态资源刷新。
- 侧栏 `NavSettings` 不是工作区，不会改写 `DashboardViewModel.CurrentWorkspace`；点击时恢复当前工作区 RadioButton 后打开 Playnite 设置，设置页内部分类 Tab、真实字段和保存语义保持不变。
- UI-288 验证：源码门禁通过；Release 0 warning/0 error；Core 59/59、Worker 194/194、Playnite 274/60/0。该阶段仍未宣称真实 Playnite 嵌入像素验收；Demo 文件夹不是运行时或测试依赖。

## 2026-08-21 BUILD-002 当前事实：跨电脑测试不再依赖 Demo 目录

- `9b19dbd` 之后的跨电脑失败来自五个 Playnite 视觉对照测试仍读取开发者机器的 `D:\workplace\Github\GameSaveCenter.AcrylicFork`；不是生产代码编译或 UI 实现回退。
- 已删除 `tests/GameSaveCenter.Playnite.Tests/AcrylicForkDesignSource.cs`、`AcrylicForkDesignFactAttribute.cs` 及五个测试中的外部 Demo 读取。相关测试现在是普通 `[Fact]`，只验证当前仓库内生产资源契约；Demo 不再是测试运行时输入，也不需要环境变量或兄弟目录。
- 验证：设置 `GSC_ACRYLICFORK_ROOT` 指向不存在目录时，完整 Playnite 为 274 通过、60 跳过、0 失败，共 334 项；隔离 Release 构建成功。没有修改生产 UI、命令、Binding 或业务逻辑。

## 2026-08-21 UI-287 当前事实：共享表格表头排序箭头与列宽调整

- 生产共享 `DataGridColumnHeader` 模板之前只有排序 `Path`，没有 WPF 约定的 `PART_LeftHeaderGripper`/`PART_RightHeaderGripper`，因此虽然 `CanUserResizeColumns=True`，实际表头没有可拖拽列宽的命中区域。
- 排序箭头之前放在固定 14 DIP 列中，但路径本身宽约 13 DIP 还带右侧 inset，窄列排序时会被裁掉；现在预留 22 DIP 独立箭头列，并保留 64 DIP 的共享最小列宽。
- `WpfUiProduction.xaml` 的共享表格和 `DashboardView.xaml` 的兼容表格均保留透明 resize Thumb、完整排序箭头、现有选中态、滚动和虚拟化；不改列绑定、命令或业务排序逻辑。
- RenderHarness 现在对 Save/Task/Media/Maintenance 探测表格检查真实表头模板部件，并强制验证排序状态下箭头有非零布局宽度；第一列左 Thumb 被 WPF 自动折叠是正常边界行为，至少一个边界命中区必须有效。
- UI-287 证据：`artifacts/gsc-b/ui287-table-header-v2` 构建/测试通过，XAML 18/18、0 warning/0 error、Core 59、Worker 194、Playnite 274 通过/60 跳过；`artifacts/ui-qa/ui287-table-header-v2/render-qa-report.txt` 为 `render-qa OK`，覆盖双主题、1040/1100/1366/2560、多 Tab、滚动和 resize transition。真实 Playnite 生产宿主逐页拖拽验证仍未完成。

## 2026-08-21 UI-286 当前事实：修改器导入线程与 FLiNG 历史归档

- `GameToolService.InspectImportAsync` 不得把文件名包含 `Update` 的所有 EXE 排除；显式选中的单文件必须按扩展名保留。目录/ZIP 候选只排除明确的 `unins*`、`uninstall`、`update`、`updater`、`setup` 辅助入口，`Outlast 2 v1.0-Update 2 Plus 4 Trainer.exe` 是有效修改器入口。
- `DashboardViewModel.PrepareGameToolImportAsync`、`ClearPendingGameToolImport` 和导入后选中工具更新涉及绑定集合/属性，必须通过 `ApplyOnUi`；`ImportEntryCandidates` 的 `CollectionView` 不能从 IPC/Worker continuation 修改。
- `FlingTrainerCatalogSource` 现在可选同步 `https://archive.flingtrainer.com/` 的目录链接；仅登记 ZIP/EXE，归档条目的 `PageUrl` 是安全校验后的直接下载地址，`GetReleasesAsync` 返回单一归档版本。下载继续复用现有 HTTPS host 校验、2 GiB 限制、ZIP 路径/大小/文件数校验和入口筛选；RAR/7z 尚未支持。
- UI-286 证据：`artifacts/gsc-b/ui286-trainer-import-v2` 构建/测试通过，Worker 194、Playnite 273/60；`artifacts/ui-qa/ui286-trainer-import-v1/render-qa-report.txt` 为 `render-qa OK`，覆盖双主题、多尺寸、Tab、滚动和 resize。新测试覆盖含 `Update` 的直接 EXE、ZIP 入口和归档目录解析。当前共享环境没有用户的 `D:\Download\Brave` 文件，未进行其真实签名/哈希检查，也未执行它；真实 Playnite 生产宿主逐页验证仍未完成。

## 2026-08-21 UI-285 当前事实：媒体待归类反向滚动禁用共享星号重分配

- `MediaInboxGrid` 处于有限 `Grid` 视口时必须设置 `infra:DataGridStarFill.Enabled=False`。共享 star-fill 只适合无限测量宿主；在媒体 4468 条收件箱中把星号列重算为像素列并 `InvalidateMeasure`，会和 Standard 虚拟行呈现器的底部→顶部回退竞争，产生滚动条仍在但行全部消失的偶发状态。
- 媒体收件箱仍固定 `VirtualizingPanel.ScrollUnit=Item`、`VirtualizationMode=Standard`、行虚拟化开启、列虚拟化关闭；不要为了修复空白而关闭整个虚拟化，也不要把该例外扩散到其他工作区。
- `tests/GameSaveCenter.RenderHarness/Program.cs` 的媒体探针必须覆盖多次底部→顶部→中段回退，并确认 `DataGridStarFill.GetEnabled(MediaInboxGrid)==false`、每一步有实现行且无无效 DataContext/表头 gap。
- UI-285 证据：`artifacts/gsc-b/ui285-media-scroll-v1` 构建/测试通过；`artifacts/ui-qa/ui285-media-scroll-v3/render-qa-report.txt` 为 `render-qa OK`，多尺寸、双主题和反向滚动通过。真实 Playnite 生产宿主逐页截图仍未补齐，Demo-first 总迁移仍未完成。

## 2026-08-21 UI-284 当前事实：共享按钮和主题侧栏已统一

- `Themes/WpfUiProduction.xaml` 的 `GscWpfUiButtonTextTemplate` 必须把宿主 Button 的动态 `Foreground`、字体族、字阶和字重传给内部 `TextBlock`；否则 Primary 按钮文字会回落为黑色，深色主题不可读。页面不得复制按钮模板绕过共享修复。
- `OverviewView.xaml` 的风险操作按钮使用固定 `GscButtonHeight` 的水平布局；`AttentionFindings` 继续是 Demo 风格的真实分隔列表，右侧显示 `SuggestedAction` 文本，空集合仅保留标题、说明和真实维护入口；维护入口必须是有背景/描边的 Secondary 按钮。
- `GameSaveCenterSettingsView.xaml` 的 `SettingsSectionTabs` 使用 `GscSettingsSectionTabs`，不要恢复 `LabSegmented` 灰色整块；选中态、Hover、字体和前景色都从当前主题动态资源读取。
- `SaveCenterView.xaml` 的比较质量气泡固定 28 DIP 高度、内边距和最大宽度，并与“版本比较”标题垂直居中；不要让长文本重新参与标题行高度测量。
- UI-284 证据：`artifacts/gsc-b/ui284-theme-v1` 构建/测试通过；`artifacts/ui-qa/ui284-theme-v1/render-qa-report.txt` 为 `render-qa OK`，双主题、多尺寸和 resize 通过。真实 Playnite 生产宿主逐页截图仍未补齐，Demo-first 总迁移仍未完成。

## 2026-08-21 UI-283 当前事实：媒体待归类使用稳定的大数据虚拟化契约

- `MediaCenterView.xaml` 的 `MediaDataGrid` 继续 `Item` 滚动、行虚拟化、固定共享行高、顶部对齐、排序/列宽调整和圆角选中态；针对真实最多 5000 条的 `UnassignedMedia`，必须局部使用 `VirtualizationMode=Standard` 与 `EnableColumnVirtualization=False`，不能恢复共享的 `Recycling` + 列虚拟化组合。
- 这只是媒体收件箱的性能/呈现例外，不得扩散到 Save/Task/Maintenance 等工作区；其它共享表格仍由 `GscRedesignWorkspaceDataGrid` 使用 `Recycling` 和列虚拟化。当前媒体预览 Inspector、目标游戏 ComboBox、归类/忽略命令、`SelectedInboxMedia` 与安全语义必须保持在 DataGrid 滚动面之外。
- `tests/GameSaveCenter.RenderHarness/Program.cs` 的 `CreateMediaInboxProbeData` 使用 4468 条数据，媒体滚动探针显式期望 Standard/关闭列虚拟化；不要把大数据探针降回 60 条，也不要把所有表格的 Recycling 断言重新写成无例外的全局门禁。
- UI-283 证据：`artifacts/gsc-b/ui283-media-inbox-v1` 构建/测试通过；`artifacts/ui-qa/ui283-media-inbox-v5/render-qa-report.txt` 为 `render-qa OK`，4468 条数据在 0/25/50/75/100% 位置均无正向 gap，双主题、多尺寸和 resize 均通过。真实 Playnite 生产宿主逐页截图仍未补齐，Demo-first 总迁移仍未完成。

## 2026-08-21 UI-282 当前事实：首页小屏与 Demo 风险/比较结构已收口

- 当前 `OverviewView.xaml` 的紧凑堆叠布局中，`OverviewActivityColumn` 使用内层 Flow 第 3、4 行；`OverviewSecondaryScrollViewer` 必须在专用第 5 行，不能回到第 4 行，否则风险卡会与全局活动发生空间叠加。首页根 ScrollViewer 仍是唯一页面级纵向滚动面。
- `OverviewProtectionPreviewItems` 是 Demo 风格的多选 `ListBox`，卡片只显示状态点、游戏、状态 Chip 和原因，不得恢复 Checkbox、逐项“查看”按钮、第二个保护明细卡或安全提示子卡。卡片选中通过 `OnProtectionSelectionChanged` 转发到真实 `OpenProtectionItemCommand`；底部 `OpenProtectionGamesCommand`/`ApplyRecommendedProtectionCommand`、确认和当前快照安全语义不变。
- `AttentionFindings` 必须使用 Demo 的紧凑分隔行，右侧绑定真实 `SuggestedAction` 文字；不要恢复“查看原因”按钮，也不要添加不在 Demo 中的自定义空状态段落。空集合时保留标题和“打开维护中心”真实入口即可。
- `SaveCenterView.xaml` 的“比较与保留”页必须由 `SaveComparePageScrollViewer` 承载 `MinWidth=880` 的横向画布，左右两张 `GscReadingCardStyle` 对等卡片不在窄宽改为上下堆叠；左侧绑定 `LastBackupDiff` 的三类计数/文件清单，右侧绑定 `RetentionSummary`、`LastRetentionPreview.KeepBackupIds` 和 `DeleteCandidateIds`，并保留二次确认安全说明。
- UI-282 证据：`artifacts/gsc-b/ui282-demo-structure-v4` 构建与测试通过；`artifacts/ui-qa/ui282-demo-structure-v2/render-qa-report.txt` 为 `render-qa OK`，首页 1040/1100 小屏的活动与风险坐标不重叠，比较页横向滚动指标正常；`validate-source.py` 0 error，WPF UI 0 error、20 warnings、164 info。真实 Playnite Dashboard 逐页宿主截图仍未补齐，Demo-first 总迁移仍未完成。

## 2026-08-21 UI-281 当前事实：首页风险/关注区与共享按钮几何已修复

- `Themes/WpfUiProduction.xaml` 的 `GscWpfUiButton` 必须让状态层和 `ContentPresenter` 位于带 `Padding` 的同一个按钮外壳内；之前并列的空白 Border 不参与内容测量，是全局中文按钮贴边/溢出的根因。不要在页面里复制按钮模板来绕过它。
- `OverviewView.xaml` 的重复摘要只能隐藏摘要图标、标题和说明，不能折叠包住 `OverviewProtectionDetails`；真实 `RecentProtection.Items`、选择框、状态点/气泡、逐项查看和批量保护命令必须保持可见。首页最近任务/全局活动不使用 `IsMouseOver` 视觉覆盖，选中态和键盘焦点语义保留。
- `AttentionFindings` 仍是 Dashboard 的真实 Finding 集合；需关注事项使用紧凑 Demo 行，有数据时显示真实标题/游戏/动作，无数据时仅显示绑定驱动的空状态，不得添加 Mock 条目。
- `SaveCenterView.xaml` 的比较与保留页使用两个 `*` 对等卡片，动作按钮放在各自标题行；`ApplyResponsiveLayout` 只在窄宽时折叠到单列。备份策略、云端上传设置中的标签/Chip/Toggle/输入框/ComboBox 使用显式 `*` + `Auto` 列，避免“启用备份策略”和“重要游戏 · 严格”错位。
- UI-281 证据：`artifacts/gsc-b/ui281-button-risk-v4` 构建/测试通过；`artifacts/ui-qa/ui281-button-risk-v3/render-qa-report.txt` 为 `render-qa OK`，风险保护列表与关注列表的 ScrollViewer 均恢复非零视口，比较与保留/备份策略截图已抽查。真实 Playnite Dashboard 宿主证据仍未补齐，Demo-first 总迁移仍未完成。

## 2026-08-21 UI-280 当前事实：首页已修正 Demo 密度与选中态

- `OverviewView.xaml` 的 `OverviewStatStrip` 使用 11 列交替布局，统计卡和分隔线不再占用同一列；六个真实 `Snapshot` 指标、比例进度条和统计卡悬停动效保持。
- 首页最近任务、全局活动、风险标题/正文和需关注事项使用本地 Demo 字体别名；状态气泡使用 `GscRedesignTableStatusPill`，共享 ToolTip 使用 UI 字体链、12 DIP 字阶、`Padding=10,7`、`VerticalOffset=4` 和受控最大宽度。
- 首页风险区不再使用额外的 `Expander` 或嵌套生产子卡，`OverviewProtectionDetails` 直接承载真实 `RecentProtection.Items`；复选框的 `IsSelected`、逐项 `OpenProtectionItemCommand`、批量 `OpenProtectionGamesCommand`/`ApplyRecommendedProtectionCommand`、确认和当前快照安全语义必须继续保留。保护项为两行紧凑卡，列表仍在 `OverviewProtectionItemsScrollViewer` 内滚动。
- 需关注事项使用 18 DIP 小图标标记和透明紧凑操作；不要恢复 34 DIP 大图标块。共享 `GscRoundedDataGridRowTemplate` 对隐式 `DataGridRow`、`GscStableDataGridRow` 以及 Media 行提供 Trainer 风格的 Accent 填充、Accent 描边和 14 DIP 圆角选中态；`SelectiveScrollingGrid`、DetailsPresenter、Recycling 和列宽/排序行为保持。
- Dashboard 兼容表格的本地 DataGridRow 模板也补齐相同的选中圆角；当前游戏选框、生产滚动条系统、真实命令/绑定和项目 Tab chrome 未改动。
- UI-280 证据：`artifacts/gsc-b/ui-overview-home-v4` 构建/测试通过（Playnite 272 通过、60 跳过）；`artifacts/ui-qa/ui-overview-home-v2/render-qa-report.txt` 为 `render-qa OK`；WPF 静态校验 0 error、19 warnings、164 info。真实 Playnite 逐页嵌入截图仍未补齐，不能把本阶段离屏证据写成总迁移完成。

## 2026-08-21 UI-279 当前事实：Trainer 窄宽导入工具栏改为可用重排

- `TrainerCenterView.xaml` 的“当前游戏工具”标题区现在把导入工具栏、拖放提示和标题分成可重排的独立行；`ApplyResponsiveLayout` 在 `<980 DIP` 时把四个真实导入按钮移到标题下方，避免约 744 DIP 工作区裁掉最右侧按钮。
- 不得通过隐藏“导入修改器”“导入目录”“导入 CT”“+ 添加启动项”中的任何入口来解决窄宽问题；四个 Command、导入确认、工具列表/Inspector、ScrollViewer 和 Recycling 虚拟化均保持。
- UI-279 证据：`artifacts/gsc-b/ui279-trainer-toolbar-v1` 构建/测试通过；`artifacts/ui-qa/ui279-trainer-toolbar-v1/render-qa-report.txt` 为 `render-qa OK`，浅/深色 Trainer 1040×700 截图按钮均可见；`artifacts/ui-audit-ui279-trainer-toolbar-v1/AUDIT_SUMMARY.md` 为 Fidelity 0、HIGH 0、失败路由 0。真实 Playnite Dashboard 宿主证据仍未补齐。

## 2026-08-21 UI-278 当前事实：RenderHarness 使用主题化宿主画布

- RenderHarness 之前在页面宿主 Grid 中写死 `#181E2B`，导致强制浅色 QA 将 Demo 浅色色板页面错误放在深色画布上；这不是生产 Trainer 页面本身的真实主题关系。
- `tests/GameSaveCenter.RenderHarness/Program.cs` 的页面宿主现在统一通过 `CreateHarnessBackground(view)` 读取当前页面的 `GscBackdropBrush`；没有应用主题的历史探针保留原深色回退，不能删掉回退以改变既有探针语义。
- UI-278 证据：`artifacts/ui-qa/ui278-themed-host-v1/render-qa-report.txt` 为 `render-qa OK`，Light/Dark 七页、多尺寸、滚动和 resize 均通过；浅色 Trainer 1040×700 已确认顶部导入区可读。真实 Playnite Dashboard 宿主证据仍未补齐。

## 2026-08-21 UI-277 当前事实：共享折叠栏 Header 具有主题表面

- `DesignTokens.xaml` 的 `GscDisclosureCardExpander` 现在以 `GscControlFillBrush` 为默认背景，并把 `Background`/`BorderBrush` 传到 Header ToggleButton 的 `HeaderChrome`；Task 窄宽“更多筛选”不再在浅色主题深色画布上隐去标题。
- 不要把折叠栏改回透明 Header 或在 Task 页复制局部模板；整行命中、`TaskMoreFiltersExpander` 的 Visibility/响应式切换、Chevron 150ms 动效和真实 `TaskGameFilter` 绑定由共享资源继续负责。
- UI-277 证据：`artifacts/gsc-b/ui-277-disclosure-surface-v1` Release 0 warning/0 error、Core 59/59、Worker 191/191、Playnite 266 通过/62 跳过/0 失败；`artifacts/ui-qa/ui277-disclosure-surface-v1/render-qa-report.txt` 为 `render-qa OK`，Task 1040×700 双主题截图确认标题可见。真实 Playnite Dashboard 宿主证据仍未补齐。

## 2026-08-21 UI-276 当前事实：媒体当前页窄宽操作区已拆分

- `MediaCenterView.xaml` 的 `MediaCurrentActionRow` 现在是共享的响应式操作容器：宽屏提示和三个批量操作同一行；窄屏提示独占第一行，批量操作与 `MediaCompactDetailsButton` 分列第二行，不能把两组按钮重新放回同一个 Grid 单元格。
- `MediaCenterView.xaml.cs` 只在 `ApplyResponsiveLayout` 中切换操作行的行列位置；媒体 `ListBox` 的 SelectionMode、Recycling、异步缩略图、Inspector 抽屉和 `MediaCompactDetailsButton` 的展开/收起状态保持不变。
- UI-276 证据：`artifacts/gsc-b/ui-276-media-actions-v1` Release 0 warning/0 error、Core 59/59、Worker 191/191、Playnite 266 通过/62 跳过/0 失败；`artifacts/ui-qa/ui276-media-actions-v1/render-qa-report.txt` 为 `render-qa OK`，媒体 1040×700 双主题截图确认操作区不再重叠。真实 Playnite Dashboard 宿主证据仍未补齐。

## 2026-08-21 UI-273 当前事实：共享按钮与开关状态对齐 Demo

- `Themes/WpfUiProduction.xaml` 的共享 `GscWpfUiButton` 现在有 Demo `LabBtn` 对应的 `HoverOverlay`/`PressedOverlay` 状态层和透明度动效；主按钮继续使用生产 Demo 核心色板，覆盖层 `IsHitTestVisible=False`，不得在页面局部复制按钮状态模板。
- 共享 `GscWpfUiToggleSwitch` 使用 40×23 DIP 轨道、17 DIP 滑块、46 DIP 内容起始列和 140ms `RenderTransform.(TranslateTransform.X)` 位移动效；真实 `Content`、设置绑定、键盘焦点、禁用态和项目页面滚动保持不变。
- UI-273 证据：`artifacts/gsc-b/ui-273-shared-button-toggle-v1` 构建通过；`artifacts/ui-qa/ui273-shared-button-toggle-v1/render-qa-report.txt` 为 `render-qa OK`，七页双主题、多尺寸、滚动和 resize 均通过。真实 Playnite Dashboard 宿主证据仍未补齐。

## 2026-08-21 UI-274 当前事实：共享输入框与下拉选项对齐 Demo

- 生产 `GscWpfUiTextBoxTemplate` 的键盘焦点现在同时切换 `GscControlFocusFillBrush` 与 Accent 边框；普通主题由 `ApplyDemoCoreResources` 使用 Demo `FieldFocusFillBrush` 注入，高对比度由 `ApplyWpfUiResources` 提供自适应 fallback，验证错误仍保持错误填充/边框。
- 隐式 `ComboBoxItem` 现在使用 UI 字体链、`GscBodyFontSize` 和 Hand 光标；悬停/选中使用 `GscAccentTintBrush`，选中项 Medium 字重。不要修改 Popup 的真实滚动、键盘导航、最大高度或选择绑定来追求视觉一致。
- UI-274 证据：`artifacts/gsc-b/ui-274-input-combo-v1` 构建通过；`artifacts/ui-qa/ui274-input-combo-v1/render-qa-report.txt` 为 `render-qa OK`，七页双主题、多尺寸、滚动和 resize 均通过。真实 Playnite Dashboard 宿主证据仍未补齐。

## 2026-08-21 UI-275 当前事实：共享滑杆几何对齐 Demo

- 唯一使用点 `Settings/GameSaveCenterSettingsView.xaml` 的 `GlassStrengthSlider` 继续使用共享 `GscSlider`；模板现在为 22 DIP 高、4 DIP 轨道、18 DIP 滑块，生产主题自适应的 Thumb 阴影/悬停/拖动状态和真实 `GlassEffectStrength` 双向绑定不变。
- 不要为 Settings 另写滑杆模板，也不要因为收紧控件几何而移除页面 ScrollViewer、键盘焦点或值变化事件；低高度视口继续通过现有页面滚动到达下方控件。
- UI-275 证据：`artifacts/gsc-b/ui-275-slider-v1` 构建通过；`artifacts/ui-qa/ui275-slider-v1/render-qa-report.txt` 为 `render-qa OK`，七页双主题、多尺寸、滚动和 resize 均通过。真实 Playnite Dashboard 宿主证据仍未补齐。

## 2026-08-20 当前总规则：Demo-first 覆盖旧视觉优先级

- 后续所有页面迁移以 `GameSaveCenter.AcrylicFork/src/GameSaveCenter.Playnite/Design/DesignShellView.xaml`、`Pages/*.xaml`、`DesignTokens.xaml`、`DesignColorsLight.xaml`、`DesignColorsDark.xaml` 和 `DesignControls.xaml` 为唯一主要视觉基准；Demo 与旧生产页面、UiLab、历史计划或通用 Apple-inspired 建议冲突时，以 Demo 的整体结构、层级、空间、字体、颜色和控件为准。
- `wpf-apple-desktop-ui` 不再是视觉与实现路线的优先约束，只作为 WPF 质量检查依据，继续检查真实 Binding/Command、异步错误/取消/安全语义、虚拟化、键盘/UI Automation、可访问性、主题/DPI 和 Playnite 兼容性。
- 当前游戏选框、生产滚动条系统、真实运行时数据和 Demo 未覆盖但目标文件明确要求保留的功能继续保留；Demo Mock 数据、演示色板、窗口按钮和演示行为不得接入生产。
- 本段覆盖早期“当前生产 main > Demo”或“技能优先”的视觉排序；旧条目只用于历史追溯，不得阻止 Demo-first 的新页面迁移。

## 2026-08-21 UI-272 当前事实：修改器中心恢复项目 Tab chrome

- `TrainerCenterView.xaml` 已回滚 `a03accf` 引入的 `TrainerSegmentTabs` + `LabSegmented` 外层分段栏，恢复项目原有 `TrainerTabControl` / `TrainerTabItem`，其样式基于 `GscRedesignWorkspaceTabControl` / `GscRedesignWorkspaceTabItem`。
- 四个真实页面面板重新由 `TabItem` 承载；不要再把修改器中心外层 Tab 改成 Demo 的 `LabSegmented`。Settings 左侧 `SettingsSectionTabs` 仍保留，因为它是目标 Demo 要求的分类栏信息架构而非项目工作区 Tab chrome。
- `ImportTrainerCommand`、工具目录/CT/启动项导入、FLiNG 搜索与版本下载、工具/发行 Inspector、回收虚拟化、ScrollViewer 和 `ApplyResponsiveLayout` 均保持；仅删除 Demo 分段切换的可见性代码。
- 当前阶段证据：`artifacts/gsc-b/ui-272-trainer-tab-rollback-v2` 的 XAML 18/18、Release 0 warning/0 error、Core 59/59、Worker 191/191、Playnite 262 通过/62 跳过/0 失败；source/WPF/diff 门禁通过；`artifacts/ui-qa/ui272-trainer-tab-rollback-v1/render-qa-report.txt` 为 `render-qa OK`，覆盖七页双主题、多尺寸、Tab、滚动和 resize。该离屏结果不替代真实 Playnite 宿主证据。

## 2026-08-21 UI-271 真实 Playnite 宿主审计边界复核

- Release `0.6.70+c6cb235ef446fbe6e0c12566a7920c92e2135af8` 已通过 `scripts/real-host-audit.ps1` 安装并启动 Playnite；`artifacts/ui-host-audit-ui271/summary.json` 明确记录 `EmbeddedSettingsCaptured=true`、`ControlledDashboardCaptured=true`、`EmbeddedDashboardCaptured=false`、`ProductionVisualSourceOfTruthAvailable=false`。
- Settings 的 `settings/embedded-current/viewport/settings.png`、视觉树和资源快照来自真实 `EmbeddedPlaynite` 宿主，可作为 Settings 嵌入证据。Dashboard 自动 UI Automation 仍未定位左侧 GameSaveCenter 入口，`gates/REAL_EMBEDDED_DASHBOARD_NOT_CAPTURED.json` 的 HIGH 门禁有效；受控 Dashboard 图像只能作为布局辅助，不能冒充生产视觉真值。
- Computer Use 观察到 Playnite 主窗口为 `EmptyWindowAutomationPeer`；未绕过该限制，也未停止另一个扩展目录中的旧 Worker。后续必须在用户可见、可交互的 Playnite 窗口中打开 GameSaveCenter 后重跑审计，才可补齐七页 Dashboard 的像素、DPI、键盘焦点、命中区域和真实操作证据。
- 审计内 Release 基线为 XAML 18/18、Release 0 warning/0 error、Core 59/59、Worker 191/191、Playnite 262 通过/62 跳过/0 失败；这不改变总 Demo-first 目标未完成的判断。

## 2026-08-21 UI-271 当前事实：共享表格使用 Demo 正文与表头字阶

- `Themes/DesignTokens.xaml` 当前提供 `GscBodyFontSize=13.5`、`GscCaptionFontSize=12`，分别对应 Demo `SizeBody` 和 `SizeCaption`；生产隐式 `DataGrid` 使用 UI 字体链和正文令牌，`DataGridColumnHeader` 使用 UI 字体链、表头令牌和 Medium 字重。
- 这只统一表格文本密度，不改变 `GscTableRowHeight=44`、`GscTableHeaderHeight=36`、排序箭头、列宽调整、选中态、内部滚动、Recycling 虚拟化或真实表格绑定。
- 当前证据：`artifacts/gsc-b/ui-271-table-typography-v1` Release 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 262 通过/62 跳过；source/WPF/diff 门禁通过；`artifacts/ui-qa/ui271-table-typography-v1/render-qa-report.txt` 双主题、多尺寸、滚动和 resize 均为 `OK`。真实 Playnite 宿主字号、DPI、键盘和列宽拖动验收仍未收口。

## 2026-08-21 UI-270 当前事实：共享折叠栏箭头动效对齐 Demo

- `Themes/DesignTokens.xaml` 的 `GscDisclosureCardExpander` 现在在 `IsChecked` 进入/离开时以 150ms 将 Chevron 在 `-90°` 与 `0°` 间旋转，匹配 Demo `LabDisclosure`；`GscDisclosureCard` 继续作为统一别名。
- 这只改变共享控件的视觉状态过渡，保留整行点击、键盘焦点、内容显隐、真实 Expander 绑定和页面滚动；没有改变业务命令、数据、虚拟化或项目 ScrollBar。
- 当前证据：`artifacts/gsc-b/ui-270-disclosure-animation-v1` Release 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 261 通过/62 跳过；source/WPF/diff 门禁通过；`artifacts/ui-qa/ui270-disclosure-animation-v1/render-qa-report.txt` 双主题、多尺寸、滚动和 resize 均为 `OK`。真实 Playnite 宿主的动效时间、键盘焦点和逐页视觉验收仍未收口。

## 2026-08-21 UI-269 当前事实：Demo 核心主题色不再被宿主中性刷覆盖

- `AdaptiveThemePaletteFactory.ApplyDemoCoreResources` 是生产 Shell 与 Settings 共用的核心色板入口，固定 Demo 的浅色/深色画布渐变、卡片、侧栏、顶栏、输入框、文字层级、表格、分段控件、滚动条、遮罩和语义状态色；宿主 Accent/focus 仍保留给非核心交互。
- 高对比度通过提前返回继续使用系统自适应路径；普通主题不再由 Playnite 背景/正文中性刷重写已迁移页面的核心表面。生产 Tab chrome、当前游戏选框、滚动条行为、虚拟化、命令/Binding 和真实业务数据没有改变。
- 当前证据：`artifacts/gsc-b/ui-269-demo-palette-v2` Release 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 260 通过/62 跳过；source/WPF/diff 门禁通过；`artifacts/ui-qa/ui269-demo-palette-v1/render-qa-report.txt` 双主题、多尺寸、滚动和 resize 均为 `OK`。截图仍不能替代可识别 Playnite 宿主的逐页像素、DPI、键盘、主题和真实操作验收。

## 2026-08-21 UI-268 当前事实：标题字体接入独立 Display 字阶

- `src/GameSaveCenter.Playnite/Themes/DesignTokens.xaml` 当前同时提供 `GscUiFontFamily`（`Segoe UI Variable Text, Segoe UI, Microsoft YaHei UI`）、`GscDisplayFontFamily`（`Segoe UI Variable Display, Segoe UI, Microsoft YaHei UI`）和 `GscCodeFontFamily`（`Cascadia Mono, Consolas, Microsoft YaHei UI`）。
- `GscRedesignHeroTitle`、`GscRedesignFeedbackDialogTitle`、`GscPageTitleStyle`、`AcrylicProductionShellView` 的 `PageTitleText` 和 `DashboardView` 的回退标题使用 Display 字阶；`GscRedesignSectionTitle` 继续使用继承的正文族，保持 Demo `LabTitle`/`LabSection` 的层级关系。
- 标题字体改动没有触及生产 Tab chrome、当前游戏选框、滚动条系统、虚拟化、真实命令/Binding 或业务数据；回归测试保护共享令牌和两个生产标题入口。
- 当前证据：Release 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 259 通过/62 跳过；source/WPF/diff 门禁通过；`artifacts/ui-qa/ui268-display-font-v1/render-qa-report.txt` 双主题、多尺寸、滚动和 resize 均为 `OK`。总目标仍需继续完成 Demo 七页结构/视觉逐项核对及可识别 Playnite 宿主的像素、DPI、键盘和真实操作证据。

## 2026-08-21 UI-267 当前事实：工作区表格测量与几何审计已收口

- `MediaCenterView.xaml` 的当前游戏媒体搜索操作区保持至少 `300 DIP`，搜索输入列保持至少 `160 DIP`；真实 `MediaSearchText`、媒体类型筛选、媒体卡片、预览 Inspector 和批量操作没有改变。
- `SaveCenterView.xaml.cs` 在工作区宽度低于 `1240 DIP` 时启用历史表紧凑列宽；这是为了在标准宿主的 Inspector 并列布局中保持状态列可达，不是删除列或隐藏操作。DataGrid 的 `Auto` 横向滚动、列宽拖动、排序和 Recycling 虚拟化继续由共享生产样式负责。
- 回归断言 `SharedWorkspaceBreakpointsKeepSearchAndHistoryEssentialsReadable` 保护上述两个空间契约；生产 Tab chrome、当前游戏选框、页面滚动、真实命令/绑定和异步安全语义均未改动。
- UI 审计已按主控件直接所属 Grid 行计算纵向填充，允许 Overview 的有限本地虚拟视口，排除列表内部媒体卡片误判工具栏；列宽超过视口但 `DG_ScrollViewer` 有真实 Auto 横向滚动时记为 `EXPECTED_HORIZONTAL_SCROLL`。最新 `artifacts/ui-audit-ui267-fix3` 为 Fidelity 0、HIGH 0、MEDIUM 0、失败路由 0。
- 当前验证证据：`artifacts/gsc-b/ui-audit-layout-fix-v1` Release 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 259 通过/62 跳过；`artifacts/ui-qa/ui267-layout-audit-fix-v1` 为 `render-qa OK`。WPF 静态检查保留 0 error、19 warnings、161 info。真实 Playnite 宿主的逐页像素、DPI、键盘焦点与真实操作仍是总目标的未收口边界。

## 2026-08-20 UI-266 当前事实：存档维护指标统一数值优先阅读

- 存档页“比较与保留”中的新增文件、修改文件、删除文件指标现在统一为“数值 → 标签”；真实 `LastBackupDiff` 绑定、差异文件列表、比较命令和只读保留预览均保持不变。
- 维护页的保留、容量、趋势、保留模拟、保护状态和本地镜像指标统一为“数值 → 标签 → 补充说明”，绑定仍来自真实运行时状态，不得用 Demo 示例数字替换。
- 本阶段没有改变生产 Tab chrome、页面滚动、DataGrid/列表虚拟化、Inspector、命令或安全语义；后续新增指标卡继续优先检查 Demo 的数值优先阅读节奏。
- 当前证据：`artifacts/gsc-b/metrics-rhythm-v1` Release 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 258 通过/62 跳过；XAML/source/WPF/diff 门禁通过；`artifacts/ui-qa/metrics-rhythm-v1` 双主题、多尺寸、滚动和 resize `render-qa OK`。截图仍属于离屏证据，不能替代可识别 Playnite 宿主验收。

## 2026-08-20 UI-265 当前事实：维护诊断概览先显示环境健康

- `MaintenanceView.xaml` 的诊断概览现在按 Demo 顺序先显示 `DiagnosticHealthCard`/`DiagnosticHealthPanel`，再显示 `EnvironmentCheckCard` 与 `MaintenanceDiagnosticsActionCard`；不要把六项健康状态重新藏回“更多维护操作”展开区。
- 健康卡仍来自真实运行时绑定：Worker/Ludusavi/Rclone 状态、数据与媒体目录、待归类媒体数和设备比较数；环境检查、诊断复制/导出、自检、索引重建、任务协调、元数据灾备、路径迁移与安全模式命令没有改变。
- `DiagnosticHealthPanel` 的响应式列数仍由 `ApplyResponsiveLayout` 控制为宽屏 4 列、中等 2 列、窄屏 1 列；生产 Tab chrome 是用户明确例外，不迁移为 Demo 的 segmented UI。
- 当前证据：Release 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 258 通过/62 跳过；XAML/source/WPF/diff 门禁通过；`artifacts/ui-qa/maintenance-health-order-v1` 的双主题、多尺寸、滚动和 resize `render-qa OK`。截图仍属于离屏证据，不能替代可识别 Playnite 宿主的逐页像素验收。

## 2026-08-20 UI-264 当前事实：首页统计条已恢复 Demo 连续结构

- `OverviewView.xaml` 的 `OverviewStatStrip` 当前是一个 `GscRedesignSectionCard` 连续统计条，六个等宽指标使用五条 `GscTableDividerBrush` 分隔；数字在上、标签在下，不要恢复六张带间隙的独立 metric card 或旧的 `UniformGrid.Columns` 响应式换列。
- 六项显示继续绑定真实 `Snapshot.ManagedGames`、`Snapshot.MatchedGames`、`Snapshot.RunningGames`、`Snapshot.WarningGames`、`Snapshot.PendingCloudTasks`、`Snapshot.UnassignedMediaCount`；匹配/风险进度条与 `ManagedGames == 0` 时隐藏的防护仍有效，健康/注意/风险/未知明细也继续来自 Snapshot。
- `OverviewStatStrip` 的命名 XAML 元素已从 `UniformGrid` 调整为 `Border`，`ApplyResponsiveWidth` 不再调整统计列数；今日工作台、当前游戏选框、立即备份/全部备份、活动列表虚拟化、页面滚动和 hover render-only gate 均未迁移。
- 当前证据：Release 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 258 通过/62 跳过；XAML/source/WPF/diff 门禁通过；`artifacts/ui-qa/overview-summary-strip-v1` 的双主题、多尺寸、滚动和 resize `render-qa OK`。截图仍属于离屏证据，不能替代可识别 Playnite 宿主的逐页像素验收。

## 2026-08-20 UI-263 当前事实：任务统计条已恢复 Demo 连续结构

- `TaskCenterView.xaml` 顶部 `TaskSummaryPanel` 当前是一个 `GscRedesignSectionCard` 连续统计条，四个等宽指标之间使用 `GscTableDividerBrush` 分隔；不要恢复旧的可变列数 `UniformGrid` 或独立 metric card。
- 四项真实统计继续绑定 `Tasks.Count`、`RunningTaskCount`、`RetryableTaskCount`、`CompletedTaskCount`；运行中使用 `GscAccentBrush`、需要重试使用 `GscWarningBrush`、今日完成使用 `GscSuccessBrush`，与 Demo 的状态层级一致。
- `TaskSummaryPanelElement` 已从 `UniformGrid` 调整为 `Border`；`ApplyResponsiveLayout` 不再调整摘要列数，但仍负责任务表 236 DIP 最小视口、筛选重排、Inspector 堆叠与详情高度。
- 当前证据：Release 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 258 通过/62 跳过；XAML/source/WPF/diff 门禁通过；`artifacts/ui-qa/task-summary-strip-v1` 的双主题、多尺寸、滚动和 resize `render-qa OK`。截图仍属于离屏证据，不能替代可识别 Playnite 宿主的逐页像素验收。

## 2026-08-20 UI-262 当前事实：媒体统计条已恢复 Demo 连续结构

- `MediaCenterView.xaml` 顶部 `MediaSummaryPanel` 当前是一个 `GscRedesignSectionCard` 连续统计条，四个等宽指标之间使用 `GscTableDividerBrush` 分隔；不要恢复为四张带间隙的 `GscRedesignMetricBorder` 独立卡片。
- 四组显示继续绑定真实值：总媒体/截图/录像来自 `MediaSummary`，占用来自 `MediaSummary.TotalSizeDisplay`，收藏来自 `MediaSummary.FavoriteCount`，待归类来自 `Snapshot.UnassignedMediaCount`；Demo 示例数字没有进入生产。
- `MediaSummaryPanelElement` 已从 `UniformGrid` 调整为 `Border`，`ApplyResponsiveLayout` 不再设置不存在的 `Columns`；媒体来源字段仍独立使用 `UniformGrid` 的响应式列数。
- 当前证据：Release 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 258 通过/62 跳过；XAML/source/WPF/diff 门禁通过；`artifacts/ui-qa/media-summary-strip-v1` 的双主题、多尺寸、滚动和 resize `render-qa OK`。截图仍属于离屏证据，不能替代可识别 Playnite 宿主的逐页像素验收。

## 2026-08-20 UI-253 当前事实：修改器中心已切换为 Demo 分段面板

- `TrainerCenterView.xaml` 当前使用 `TrainerSegmentTabs` + `LabSegmented`，通过 `PanelTools`、`PanelImport`、`PanelCatalog`、`PanelReleases` 四个命名面板承载 Demo 页面结构；不要恢复旧 `TabControl/TabItem` 外壳作为主要导航。
- 分段切换只控制 `Visibility`，真实入口继续存在：`ImportTrainerCommand`、`ImportToolFolderCommand`、`ImportCheatTableCommand`、`ImportCustomLaunchItemCommand`、`ConfirmGameToolImportCommand`、`SearchTrainerCatalogCommand`、`LoadTrainerReleasesCommand` 和 `DownloadTrainerCommand`。
- `TrainerToolsList`、目录结果和发行版本列表继续使用项目现有回收虚拟化和 ScrollViewer 交互；工具设置 Inspector、窄宽详情抽屉和响应式布局仍由 `ApplyResponsiveLayout` 管理。`LabSegmented` 四项导航属于有限标签列表，源码门禁不得要求它承担大列表虚拟化契约。
- XAML 构造期分段事件必须保留面板字段空保护；WPF 页面初始化时 `SelectedIndex` 可能早于后续命名面板生成。
- 当前证据：Release 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 252/252 通过、61 跳过；XAML/source/diff/WPF 静态门禁通过；`artifacts/ui-qa/trainer-segmented-final` 的 RenderHarness 双主题、多尺寸、resize 和四分段探针通过。仍不能把离屏 PNG 作为 Playnite 宿主逐页像素验收。

## 2026-08-20 UI-251 当前事实：存档规则卡与诊断概览按 Demo 第三轮收口

- 存档中心当前规则卡在常见工作区宽度下横向排列“当前存档规则 / 游戏名 / 状态 / 立即扫描 / 重新校验 / 刷新详情”；低于 700 DIP 才堆叠操作，避免正常宿主中规则信息和按钮被拉成多行。
- 维护中心诊断概览以 AcrylicFork 生产页面为结构基线：环境检查卡在前，诊断操作卡在后，健康指标位于“更多维护操作”内；真实检查、修复、导出、取消命令与 Binding 未移除。
- 共享生产表格排序箭头采用 Demo 的 14 DIP 表头保留列和完整路径几何；列宽拖拽、排序、固定行高和虚拟化契约继续有效。
- 设置页与生产壳体/工作区统一继承 `GscUiFontFamily`；`DashboardView` 是安全回退页，已有同一字体入口。
- 当前证据：Playnite 测试 251 通过、61 跳过、0 失败；`validate-source.py`、WPF UI 静态校验（0 error）、`git diff --check` 和 RenderHarness Release `render-qa OK` 均通过。仍无新的可识别 Playnite 生产宿主像素证据，不能把离屏结果写成真实宿主 1:1 验收。
- 已清理两条旧 AcrylicFork 基线门禁：它们曾要求首页没有“今日工作台”、媒体页必须存在已废弃的 `MediaSummaryTabStrip`/`MediaTabStrip`，与当前 Demo 结构和用户要求相反；现改为保护当前工作台、完整 TabControl、媒体 Inspector 和可预览入口。

## 2026-08-20 UI-252 当前事实：存档历史页补回 Demo 操作卡

- 存档中心默认“历史版本”页现在在表格上方显示 `SaveHistorySummaryCard`，包含真实 `Backups.Count` 版本数、当前规则/健康状态摘要以及“立即扫描 / 重新校验 / 刷新详情”三个真实命令入口；不再只有表格和选中后才出现的详情按钮。
- 摘要卡在 700 DIP 以下把操作区移到第二行，正常工作区保持标题、版本数与操作横向排列；历史表仍保持 DataGrid 的列宽拖动、排序、Item scrolling、行/列虚拟化和项目现有滚动条。
- 当前证据：XAML structural validation 18 个文件通过；Release 构建 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 251 通过/61 跳过/0 失败；RenderHarness Release `render-qa OK`，双主题、多尺寸和 resize transition 均通过。仍无新增可识别 Playnite 生产宿主像素证据，不能把离屏结果写成真机 1:1 验收。

## 2026-08-20 UI-250 当前事实：媒体页已恢复 AcrylicFork Tab 结构

- 媒体中心以 AcrylicFork 实际 `MediaCenterView.xaml` 为结构基线：摘要四卡在顶部，工作区 `TabControl` 负责自己的标题栏和内容，不能再恢复成独立 Tab 标题 + `MediaTabContentHost` 的拼接结构。
- 待归类 DataGrid 外部保留 `MediaInboxInspectorScrollViewer`，绑定 `SelectedInboxMedia`，侧栏包含截图/录像预览、来源/原因、目标游戏、`AssignInboxMediaCommand` 与 `IgnoreInboxMediaCommand`。常见 744 DIP 内容宽度下 Inspector 保持侧栏，低于 700 DIP 才堆叠。
- 生产壳体及 Overview/Save/Media/Maintenance/Task/Trainer 根节点显式继承 `GscUiFontFamily`；图标仍使用明确的 Segoe MDL2 Assets，不要用页面根字体覆盖图标。
- UI-248 的“独立 MediaTabContentHost”只保留作历史记录，当前实现以 UI-250 为准。

## 2026-08-20 UI-249 当前事实：AcrylicFork 视觉迁移已重新启动

- 本轮最终视觉参考以 `D:\workplace\github\GameSaveCenter.AcrylicFork` 为准。旧的“不要恢复/不要替换”页面迁移限制已经失效；页面级结构、共享模板、Tab/分段导航和滚动模型可以按参考实现重构，但真实命令、Binding、数据契约、错误/取消/安全语义、虚拟化和 Playnite 兼容性仍必须保留。
- 首页已补回独立“今日工作台”操作区；首页最近任务项固定为 54 DIP、消息单行省略并提供 Tooltip，不能再让多行 DetailMessage 撑高单条记录。
- 存档中心生产壳体现在同时显示“立即备份”（`BackupSelectedCommand`）和“全部备份”（`BackupAllCommand`），前者只作用于当前游戏。
- 共享 `GscRedesignTableFrame`、DataGrid 行/表头样式和 `GscDisclosureCard` 已按 AcrylicFork Demo 的紧凑密度首轮收口；后续页面阶段必须继续检查有限 Grid 行、Inspector 和操作区是否发生隐藏堆叠。
- 当前证据：RenderHarness Release `render-qa OK`、source/WPF 校验和 `git diff --check` 通过；尚未获得可识别的 Playnite 生产宿主像素证据。

## 2026-08-19 UI-247 当前事实：任务搜索区必须是单一输入面

- 任务中心的“搜索任务…”必须作为 `TaskSearchTextBox` 内部提示，搜索图标也在同一输入区内；禁止恢复独立标签列，否则真实宿主会把输入框测量成窄条。
- 当前响应式下限为桌面 `420 DIP`、中等 `300 DIP`、紧凑 `180 DIP`；该逻辑不改变 `TaskSearchText` Binding、状态/类型筛选或刷新命令。
- 如果宿主仍出现“外部搜索任务… + 旁边空框”，优先判断为旧 DLL/旧安装目录/未重启旧 Playnite 进程，不能据此判断当前源码已经回退。

## 2026-08-19 UI-248 当前事实：媒体摘要卡片与 Tab 导航已分区

- `MediaSummaryTabStrip` 现在只承载四张独立等宽统计卡片；统计卡片不再与 Tab 导航共用一个外框。
- `MediaTabStrip` 单独承载“待归类 / 当前游戏媒体 / 来源规则”导航，`MediaTabContentHost` 位于下一行并占据剩余空间。
- 该结构保留真实媒体 Binding、详情 Inspector、项目自身 ScrollBar 和虚拟化，不迁移 Demo 顶部彩色主题按钮；离屏回归通过，仍需可识别的 Playnite 宿主像素截图完成最终验收。

## 2026-08-19 UI-246 历史事实：媒体标签材质基线

- 该阶段曾将媒体标签栏尝试调整为紫色强调材质；后续 UI-248 已按页面信息架构将统计卡片与 Tab 导航拆为两个独立区域，当前实现以 UI-248 为准。

## 2026-08-19 UI-245 当前事实：任务搜索输入区宽度

- 任务中心的搜索提示必须属于 `TaskSearchTextBox` 内部内容，不能再用独立标签占用筛选栏列。
- `TaskSearchBoxHost` 的响应式最小宽度为桌面 `420 DIP`、中等 `300 DIP`、紧凑 `180 DIP`；这是为真实 Playnite 宿主的测量差异设置的布局下限，不改变搜索 Binding 或筛选命令。
- Release 包已重新安装并由 Playnite 日志确认加载生产扩展 DLL；若 Playnite 未出现可识别窗口，后续仍需重新取得宿主截图后再判断视觉结果。

## 当前有效 UI 方向（2026-08-17）

- 用户已明确授权页面级、整页 UI 重构。页面布局、信息架构、导航结构、Tab/Segmented 方案、控件类型、ControlTemplate、共享样式和滚动实现都可以重新设计；不再把上一轮 UiLab/AcrylicFork 迁移中的“不要恢复/不要替换/明确不迁移”当作当前硬性禁令。
- 旧阶段条目继续保留用于追溯当时的迁移决策，但它们不应阻止新的页面方案。新设计默认继续保护真实命令、Binding、数据契约、错误/取消/安全语义、可访问性、列表性能和 Playnite 兼容性；如果设计需要改变这些内容，必须在当前任务中明确并验证，而不是因为历史实现而回避布局或控件重构。
- 本段是当前方向声明，优先于下方 2026-08-17 之前的页面迁移偏好；后续每个独立 UI 阶段都要更新本段对应的当前事实和 `docs/ai/WORKLOG.md`。

## 2026-08-19 UI-244 当前事实：首页风险明细与最近任务有限视口

- 首页最近任务外层和列表不再设置固定最小高度，数据较少时按实际行数收缩；虚拟化、真实任务 Binding 和页面滚动仍保留。
- 首页风险内容只保留一套可见摘要/明细结构；重复预览和重复操作行隐藏，底部操作按钮位于风险视口之外。
- `OverviewRiskViewport` 在紧凑堆叠布局上限为 520 DIP，在侧栏布局上限为 420 DIP；`OverviewProtectionItemsScrollViewer` 在紧凑布局上限为 420 DIP，在侧栏布局上限为 340 DIP。两者都使用项目现有 `GscPageScrollViewer`，超出内容只在对应内部区域滚动。
- 2026-08-19 在可识别的 Playnite 生产宿主中重新展开首页风险明细，确认明细卡片和“查看”操作完整可见，底部保护操作未被内部滚动区域截断。本事实只覆盖首页紧凑布局，不能替代其他页面的宿主对照验收。

## 2026-08-19 UI-240 当前事实：风险栏有限视口与维护 Inspector 按选中状态出现

- 首页“风险与提醒”使用外层 `OverviewRiskViewport` 的 `330 DIP` 有限视口；列表超出后在自身区域使用项目 `GscPageScrollViewer` 滚动，不能让 Dashboard 根页面无限增高。展开的保护明细仍是独立 `190 DIP` 视口。
- 维护中心发现问题/审计列表无选中项时释放右侧 Inspector，表格跨满可用宽度；只有用户点击行后才显示详情 Inspector。`DashboardViewModel` 刷新时仅恢复原先仍存在的选中项，不自动选择第一条。
- 2026-08-19 已在实际 Playnite 生产版重新安装并核对：首页风险栏保持有限高度；维护中心先显示全宽表格，点击问题后显示右侧详情。该次证据来自可识别的 Playnite 宿主窗口，不能与 Preview 入口或 RenderHarness 混淆。
- 同次一键 Release 验证：Core 59/59、Worker 191/191、Playnite 249 通过/61 跳过/0 失败；source/WPF 校验和安装版本核对均通过。

## 2026-08-19 UI-227 当前事实：媒体 Tab 条与生产按钮文本由共享样式控制

- `GscRedesignWorkspaceTabControl` 的外层 Tab 条必须使用 `TemplateBinding Background`，不能把 `GscControlFillBrush` 写死在模板中；媒体中心通过 `MediaTabControl.Background=GscGlassFillBrush` 使用玻璃材质，其他工作区继续沿用各自样式。
- `GscWpfUiButtonTextTemplate` 是生产文本按钮的统一边界：`TextAlignment=Center`、`NoWrap`、`CharacterEllipsis`。新增生产操作按钮优先使用已有 `GscWpfUi*Button` 样式，不要重新创建不受边界约束的局部 Button 模板。
- 这只证明源码与 RenderHarness 的布局契约；必须在 Playnite 生产 `GameSaveCenter` 入口重新安装并人工查看媒体中心，不能把 Preview 入口或离屏截图当成宿主验收。
- 2026-08-19 已完成生产扩展落盘安装验证（目标目录中的 `extension.yaml` 与 DLL 为 `0.6.70`），但本机屏幕控制两次返回了 Codex/其他窗口而非 Playnite 内容，自动化树为 `EmptyWindowAutomationPeer`；后续不得把该次结果写成 Playnite 视觉通过，需改用能确认窗口内容的宿主截图路径。

## 2026-08-19 UI-226 当前事实：维护表格需按工作区宽度压缩固定列

- 维护诊断和异常审计页面在右侧 Inspector 可见时，1040 DIP 工作区的主表格只有约 660 DIP，不能直接沿用宽屏的游戏/标题/详情/动作最小宽度。
- `MaintenanceView.ApplyFindingsColumnLayout` 现在在 1180 DIP 以下压缩非必要固定列，保留详情和建议处理的弹性列；1366 及以上恢复更宽的 Demo 比例。长文本单元格统一字符省略并提供 Tooltip。
- 这只是视口布局策略，不改变真实 Findings、SelectedFinding、Inspector 或命令；维护表格仍使用项目现有 DataGrid、虚拟化和滚动条。
- 当前验证：RenderHarness Release `render-qa OK`，source/WPF 校验和 `git diff --check` 通过。直接 `dotnet build --no-restore` 的 MSB4276 是本机 SDK 9.0.302 缺少 Workload locator 的环境限制，后续优先使用仓库脚本的禁用 Workload resolver 构建路径或补齐 SDK 环境。

## 2026-08-18 当前构建事实：可选目录未配置不应阻断健康检查

- `IntegrityCheckService.CheckDirectory` 对空的可选目录按未启用处理；只有用户配置了路径后，该路径不存在或不可写才报告 Warning。关键目录的空路径仍按原有关键错误语义处理。
- 一键安装遇到 `Healthy` 变 `Warning` 或 `Skipped` 变 `Warning` 时，先检查是否把可选 `GameToolsDirectory`/`DownloadDirectory` 当成缺陷；不要通过修改测试期望值掩盖默认安装语义。
- `scripts/build.ps1 -OutputRoot` 的隔离目录适合编译/Core/Worker 验证，但当前 Playnite 源码测试会从测试运行目录向上定位仓库；Playnite 测试必须在仓库原目录运行，不能把隔离目录失败写成代码回归。

## 2026-08-18 UI-223 当前事实：首页状态语义与真实宿主复核

- 生产首页最近任务的进度条不是所有任务的通用装饰：只有 `执行中`、`等待中`、`等待确认` 显示进度；成功、失败、已取消项只显示状态和时间，避免偏离 Demo 语义。
- 首页最近任务状态徽标、全局活动类型/结果徽标、风险提醒徽标统一使用固定边界内的水平/垂直居中和不换行；风险徽标保持足够最小宽度，不使用省略号截断“风险”等状态文字。
- 生产首页 v0.6.70.0 已重新安装到 Playnite 并实际打开复核；确认侧栏“设置”、最近任务状态布局、全局活动“维护/信息”徽标、风险徽标均来自真实生产宿主。电脑控制已在复核后释放。
- 媒体虚拟化缩略图使用 96px 有界解码宽度；这是性能约束，不要为了视觉放大恢复到 240px 的逐卡解码。
- 当前验证基线：source 校验通过，隔离 Release 构建通过，包生成/安装通过，WPF 校验 0 error；Worker 仍有 2 个集成测试失败，Render QA 有 2 个媒体 resize 恢复用例待修，不能写成全量验证通过。
- 真实宿主本阶段只对首页重点区域做了截图复核；其他页面仍需按页面逐一进入 Playnite 对照 Demo，最终交付必须区分“实际宿主复核”和“RenderHarness/源码验证”。

## 2026-08-18 UI-225 当前事实：首页风险列表必须使用有限视口

- 首页“风险与提醒”中，`AttentionFindings` 和展开的 `RecentProtection.Items` 都不能直接让右栏 `StackPanel` 无限测量；页面使用两个独立的 190 DIP `ScrollViewer`，分别命名为 `OverviewAttentionScrollViewer` 和 `OverviewProtectionItemsScrollViewer`。
- 两个内部滚动面均引用生产 `GscPageScrollViewer`，竖向 `Auto`、横向 `Disabled`；风险卡标题、恢复提示、摘要和“打开维护中心”等操作不在内部列表视口内，始终可见。
- `DashboardViewModel` 仍把首页风险原因限制为前 4 项，`RecentProtectionSummary` 仍把保护明细限制为前 6 项；XAML 的有限视口是防止未来数据契约放宽或展开内容增长时撑高首页的第二道布局保护，不改变业务计数和完整详情入口。
- 本阶段 RenderHarness 构建、双主题多尺寸及 resize 通过；定向 Playnite 测试运行器再次无输出挂起，不能把该次测试写成通过。真实 Playnite 嵌入像素仍需单独人工验收。

## 2026-08-18 UI-224 当前事实：首页徽标模板与 Demo 行密度

- `ChipBase` 模板必须把 `HorizontalContentAlignment`/`VerticalContentAlignment` 传给内部 `ContentPresenter`；仅给外层控件设置对齐属性不够。
- 首页最近任务的进度条只属于执行中、等待中和等待确认项；成功/失败/取消项使用独立状态和时间列。全局活动的类型/结果徽标与风险徽标使用共享 Chip，分别保持 64 DIP 最小宽度与 11 DIP 风险字号，避免“信息/成功”偏移和“风险”裁切。
- 本轮真实 Playnite 只复核首页上述区域；0.6.70 已安装成功且控制已释放。Media resize 两项 RenderHarness 失败、旧 Playnite UI 结构测试失败仍是未清理的验证债务，不得宣称全量 UI 完成。

## 2026-08-17 UI-222 当前事实：生产入口与 Preview 必须区分

- Playnite 当前可能同时加载两个独立扩展：`GameSaveCenter Preview`（0.6.71，Demo/预览）和生产 `GameSaveCenter`（0.6.70，`AcrylicProductionShellView`）。验证生产 UI 时必须从侧栏的 `GameSaveCenter` 进入，不能把 Preview 入口的画面当作生产页面。
- 生产 `DashboardView` 的可见层是 `AcrylicProductionShellView`，其 `PageHost` 承载真实 Overview/Save/Trainer/Media/Task/Maintenance 页面；旧 `DashboardDemoShell` 保留字段但必须保持 `Collapsed`。
- 生产头部标识使用“生产版”，不要恢复 Demo 的“外观预览”文字；Demo 的主题色只进入生产令牌，不迁移 Demo 色板控件。
- Real Host 审计在 150% DPI 下必须把 `TransformToAncestor` 的设备像素坐标规范化到根元素 DIP，并按 `GetRenderScale(window)` 保存受控窗口截图；否则会产生右侧按钮假溢出和截图被裁剪的假象。
- 当前验证基线：source/WPF 校验无 error、Release 无 warning/error、Playnite 303/303、RenderHarness `render-qa OK`；嵌入式 Dashboard 仍需用户真实点击 Playnite 侧栏才能取得像素真值。

## 2026-08-17 UI-221 AcrylicFork 整页视觉迁移收口

- 权威参考仍为 `D:\workplace\github\GameSaveCenter.AcrylicFork` @ `b09cba6`；本轮确认生产 7 页骨架已与其一致，并补齐 Media/Trainer/Save/Maintenance 分段导航右侧说明与真实计数/状态：待归类数、已绑定工具数、当前规则校验状态、安全模式/云端状态。
- 评估过独立 `AcrylicParity.xaml` 别名层，但独立字典的 `BasedOn` 无法解析父级合并字典里的 Gsc 样式，最终未引入；页面继续直接引用生产 Gsc 共享样式，避免解析风险。不要为了“Lab 键”重新创建该字典。
- 继续保留生产滚动条、圆角表头、DataGrid/ListBox 虚拟化和真实绑定；demo 右上角色板、演示数据和样例滚动条未迁移。
- 验证基线：source/XAML 通过；WPF UI 校验 0 errors（16 条既有 warning）；Release 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 304/304；render-qa OK（7 页 × Light/Dark × 1040/1100/1366/2560 + resize）。
- 真机：0.6.70 安装成功并加载，`extensions.log` 无新增 XamlParseException；real-host-audit 捕获 EmbeddedSettings/Controlled，`EmbeddedDashboardCaptured=false`（自动化无法点击侧栏），不得冒充完整宿主像素验收。

## 2026-08-16 UI-220 UiLab 几何对齐与虚拟媒体回滚修复

- 上一轮迁移留下的关键差异已收口：`GscRedesignWorkspaceTabItem` 的页面内容对齐改为 `Stretch`，模板只把分段标题居中；否则 WPF 会把真实表格/卡片内容按标题对齐方式压缩到中间，造成用户截图中的大面积空白和错位。
- 维护中心的诊断二级导航已按 UiLab 改为 `GscRedesignSegmented` + 命名面板宿主；“问题列表”和“诊断概览”都可见，原有诊断表格、详情 Inspector、绑定和命令保留。媒体、存档、维护页的本地 TabItem 样式同步改为 Stretch 内容。
- `VirtualizingWrapPanel` 不再把首次无限高度测量解释为零视口；现在优先使用 ScrollViewer/上一次视口高度，并在生成器刷新越界时排队重新测量，覆盖媒体列表切换、滚动到底后回顶的 WPF 时序。
- 首页 Hero/当前游戏列恢复 UiLab 的 `1.35*:1` 比例，744 DIP 紧凑视口仍自动堆叠并保持三枚操作按钮可用。RenderHarness 新增媒体 WrapPanel 滚动回顶探针，并支持 segmented 页面和维护二级 segmented 导航。
- 当前验证：source 校验通过；WPF UI 校验 0 errors（仅既有布局/主题资源提示）；Playnite Release 构建 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 303/303；`render-qa OK`，双主题、多尺寸、resize、维护二级导航和媒体滚动回顶均通过。离屏截图不能替代 Playnite 宿主像素验收；当前 Computer Use 仍无法稳定激活 Playnite，不能把本轮写成完整真机视觉验收。

## 2026-08-16 UI-219 UiLab 分段页面骨架直迁当前事实

- 当前媒体、存档、修改器、维护四个生产页已按 UiLab 的顶层页面骨架运行：`Grid` 的 Auto 导航行 + `GscRedesignSegmented` + `Grid` 面板宿主；不要再把“共用配色/卡片”当成完成迁移，也不要恢复外层 `TabControl` 作为这四页的主导航。维护诊断和审计内部嵌套页签是 UiLab 本身存在的层级，继续保留。
- 生产面板名称：Media=`MediaInboxScrollSurface`/`MediaCurrentScrollSurface`/`MediaSourcesPanel`；Save=`SaveHistoryPanel`/`SaveCandidatePanel`/`SavePolicyPanel`/`SaveComparePanel`；Trainer=`InstalledToolsLayout`/`TrainerImportPanel`/`TrainerCatalogPanel`/`TrainerReleasesPanel`；Maintenance=`MaintenanceDiagnosticsPanel`/`MaintenanceDevicePanel`/`MaintenanceRetentionPanel`/`MaintenanceAuditPanel`/`MaintenanceProcessPanel`。分段 `SelectionChanged` 在 `InitializeComponent` 未完成时必须容忍空字段。
- `Redesign.xaml` 的 `GscRedesignSegmented` 对齐 UiLab `LabSegmented` 的 `SegmentFillBrush`、选中项填充/描边、RadiusM=10 和 item 内边距；运行时浅色/深色主题在 `AdaptiveThemePalette` 同步这些资源。演示色板、窗口按钮、样例数据、样例滚动条不迁移。
- `VirtualizingWrapPanel` 现在对生成器插入位置做边界裁剪，遇到 WPF 生成器时序越界会清空当前生成子项并重新测量；不要改回无边界 `VisualCollection.Insert`，也不要以普通 `WrapPanel` 替换它。
- 当前自动基线：source/XAML 校验通过，Release 0 warning / 0 error，Core 59/59、Worker 191/191、Playnite 303/303；真实扩展版本为 `0.6.70.0`。启动日志未出现本轮新增崩溃，但 Computer Use 无法激活 Playnite（黑色捕获 + `EmptyWindowAutomationPeer`），所以真实页面像素和逐页切换仍是 `MANUAL QA REQUIRED`，不能写成已验收。

## 2026-08-16 UI-218 UiLab 页面骨架迁入生产

- 当前生产页面已按 `D:\workplace\github\GameSaveCenter.UiLab` 的工作区骨架收敛：Dashboard 只保留一套页面头部游戏上下文；`SelectedGameHeader`、`GameHeaderActions`、`RestoreSafetyBanner` 不得在页面外重新显示成重复的第二层。安全说明应放在备份策略/比较表面内。
- `SaveCenterView.xaml` 的策略页使用三栏 Demo 表面并保留真实 `SelectedGame.Policy`、模板、云端和命令绑定；比较指标使用可换行的固定最小宽度，兼顾用户指定的按钮/指标重叠例外。
- `TrainerCenterView.xaml` 的当前游戏工具栏和拖入导入区是两行；`TaskCenterView.xaml` 的搜索、状态、类型与游戏筛选是两行，`TaskGameFilterHost` 必须整体移动，不能只移动 ComboBox。
- 不迁移 UiLab 的演示色板、窗口控制、演示数据和样例滚动条；生产滚动条、DataGrid 虚拟化、键盘/绑定/命令和 Playnite 兼容性优先。窄宿主视口出现策略卡纵向堆叠属于响应式行为，不等于未迁移。
- 当前验证基线：source/XAML 校验通过，Release 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 303/303；真实 Playnite 人工检查首页、备份中心、修改器中心、任务中心已加载，无重复全局安全横幅。自动审计仍保持 `EmbeddedDashboardCaptured=false`，人工截图不能冒充自动嵌入证据。

## 2026-08-16 UI-217 真实 Playnite 人工视觉复核

- `81fde54` Release 包在真实 Playnite 内加载成功；人工进入 GameSaveCenter 后确认首页的开放式头部、真实游戏上下文、单一六项指标带、工作区卡片和生产页脚均可见，演示色板/窗口按钮/样例滚动条没有进入生产。
- 真实任务中心 1303×673 宿主视口可见四项统计带、筛选区、圆角表头 DataGrid、真实状态胶囊和进度条；未选中任务时 Inspector 按 `SelectedTask` 为空隐藏，属于生产交互状态，不是页面布局缺失。
- 自动 `real-host-audit` 仍因 UIAutomation 找不到 Playnite 侧栏而记录 `EmbeddedDashboardCaptured=false`；人工截图只作为本轮观察，不得改写自动审计摘要或冒充完整宿主矩阵。

## 2026-08-16 UI-215 Task 统计栏与窄窗筛选迁移

- `TaskCenterView.xaml` 已把生产任务中心顶部四个独立指标卡改为 UiLab 同款的单一 `TaskSummaryBand`；`Tasks.Count`、`RunningTaskCount`、`RetryableTaskCount`、`CompletedTaskCount` 四个真实 OneWay 绑定保留，内部使用三条 `GscDividerBrush` 分隔线，不迁移 UiLab 的演示数据或滚动条。
- 指标按 UiLab 的“数值在上、标签在下”阅读节奏收敛到 26 DIP，保留任务队列、详情、筛选、DataGrid 表头/滚动条和虚拟化；任务摘要不再通过 `UniformGrid.Columns` 产生卡片换列。
- 修复窄宽度筛选迁移的残留标签错位：`TaskGameFilterHost` 将“游戏:”与真实下拉框作为一个控件组一起移入/移出“更多筛选”，避免 1040-DIP 工作区出现“游戏:”孤立在“类型:”前的拥挤布局。
- 当前自动验证：`validate-source.py`、XAML 检查、Release 0 warning/0 error、Playnite 303/303、RenderHarness 双主题/1040/1100/1366/1600/2560 及 resize 全部 `render-qa OK`；新证据在 `artifacts/ui-qa/task-summary-band-v2`。真实 Playnite Dashboard 仍未重新捕获，不能把离屏截图写成宿主视觉真值。
- 隔离安装构建曾暴露旧的响应式单元测试仍检查 `TaskGameFilterComboBox` 直接挂在筛选容器；测试已同步到新的 `TaskGameFilterHost` 结构，当前 Playnite 测试恢复 303/303。后续移动筛选控件时必须同时更新父容器契约测试。

## 2026-08-16 UI-214 Overview 统计栏与首页光晕收口

- `OverviewView.xaml` 已把原先六个独立圆角统计卡改为 UiLab 同款的单一 `OverviewStatBand`：六个真实 `Snapshot` 指标保留在同一表面内，使用五条 `GscDividerBrush` 分隔线；两个真实比例条、空库折叠保护和 OneWay 绑定均未改变。
- 删除统计卡专用的悬停位移动画，避免把非交互指标做成堆叠/错位卡片；Dashboard 的 `UiAnimationsEnabled` 合约仍保留给其它工作区。
- `DashboardView.xaml` 的根层环境光从三色椭圆收敛为单一 `GscAmbientAccentBrush` 磨玻璃晕影，保留首页喜欢的背景氛围；生产 ScrollBar、页面滚动、虚拟化和真实命令/绑定未迁移 UiLab 演示实现。
- 当前自动验证：`validate-source.py`、XAML 检查、WPF UI 校验（0 errors）、Release 0 warning/0 error、Playnite 303/303、RenderHarness 双主题/1040/1100/1366/1600/2560 及 resize 全部 `render-qa OK`。新证据在 `artifacts/ui-qa/overview-single-band-v1`。
- 本阶段未把离屏截图写成真实 Playnite 嵌入真值；真实宿主 Dashboard 仍受 UIAutomation 无法定位侧栏入口限制，必须继续保留 `EmbeddedDashboardCaptured=false` 的诚实边界。

## 2026-08-16 UI-213 真实宿主审计当前事实

- 当前提交 `420483f` 的 Release 包已通过 `scripts/real-host-audit.ps1` 安装并启动 Playnite；人工通过 Computer Use 进入真实 Playnite，确认 GameSaveCenter 实例和 Settings 宿主窗口可见，未出现立即的 XAML 解析崩溃。
- `artifacts/ui-host-audit/summary.json` 明确记录：`EmbeddedSettingsCaptured=true`、`EmbeddedDashboardCaptured=false`、`ControlledDashboardCaptured=true`、`ProductionVisualSourceOfTruthAvailable=false`。Settings 的真实宿主截图/滚动证据已生成，但 Controlled Dashboard 证据不是生产嵌入像素。
- 因自动 UIAutomation 仍找不到 Playnite 左侧 GameSaveCenter 入口，本轮不能把 Media 缩略图网格写成“真实宿主已验收”；Media 仍以 `artifacts/ui-qa/media-grid-migration-v3` 的离屏多尺寸/主题结果作为自动证据，真实大媒体库、DPI、键盘和连续缩放继续是 `MANUAL QA REQUIRED`。

## 2026-08-16 UI-212 Media 网格当前事实

- `MediaCenterView.xaml` 的当前媒体 `MediaGrid` 已按 UiLab 的 164×142 DIP 卡片节奏改为 `ui:VirtualizingWrapPanel`，缩略图高度 96 DIP；卡片包含真实 `ArchivePath` 异步缩略图、录像/收藏标识、文件名、拍摄时间和云端状态。
- `VirtualizingWrapPanel` 位于 `src/GameSaveCenter.Playnite/Controls/VirtualizingWrapPanel.cs`，实现 `IScrollInfo`，只生成当前视口附近的容器，兼容 Recycling generator；不要替换成普通 `WrapPanel`，也不要迁移 UiLab 的滚动条模板。
- `MediaGrid` 仍是生产 `ListBox`，保留 `ItemsSource={Binding MediaView}`、Extended selection、`ScrollViewer.CanContentScroll=True`、生产滚动条、Inspector 抽屉和真实批量/编辑命令；窄窗仍使用 `MediaCompactDetailsButton`。
- 首次挂载时生成器可能尚未就绪，面板已对该 WPF 测量时序做空保护；Reset、窄宽切换、Light/Dark 离屏渲染均已通过。
- UI-212 自动验证：`validate-source.py`、XAML 检查、Release 0 warning/0 error、Playnite 303/303、RenderHarness 全量 `render-qa OK`；真实 Playnite 大媒体库/DPI/主题/键盘/连续缩放仍需人工验收。

## 2026-08-16 UI-208 Overview 全局活动当前事实

- `OverviewView.xaml` 的全局活动已按 UiLab 业务列表迁移：类型胶囊 → 对象/事件两行 → 结果胶囊 → 时间，不再额外显示 DataGrid 式表头，也不使用图标列。
- 生产仍绑定真实 `Activities`，保留 `ItemsControl` Recycling、`KindDisplay`/`ResultDisplay`、结果语义色和 `OverviewStackScrollSurface` 页面滚动；demo 的滚动条、演示数据和右上角色板没有迁移。
- 窄窗口只缩小 `ActivityKindColumn`、`ActivityTimeColumn` 并降低摘要最小宽度；不要重新添加内部滚动或把时间/结果挤进摘要列。
- UI-208 自动验证：Playnite 303/303，生产 Release 0 warning/0 error，v6/v6.2 Overview 宽/窄截图通过；真实 Playnite 宿主主题/DPI/键盘/连续缩放仍需人工验收。

## 2026-08-16 UI-209 表头共享模板当前事实

- 生产 `DataGridColumnHeader` 已有独立圆角模板；`DataGridColumnHeadersPresenter` 现在必须保持 `Background=Transparent`，否则连续底色会吞掉列头之间的圆角间隙并恢复成硬矩形表头。
- `GscTableHeaderBrush`、`GscTableDividerBrush`、表头高度/内边距、排序 glyph 和页内 DataGrid 虚拟化仍由生产共享模板控制；不要为对齐 UiLab 而迁移 UiLab 滚动条或关闭 `VirtualizingPanel.ScrollUnit=Item`。
- UI-209 自动验证：Playnite 303/303，生产 Release 0 warning/0 error，v6/v6.2 表格与 Overview 宽/窄离屏截图通过；真实 Playnite 宿主主题/DPI 仍需人工验收。

## 2026-08-16 UI-210 Media 当前事实

- `MediaCenterView.xaml` 顶部统计现在是一个 `GscRedesignSectionCard` 内的四段统计带，`MediaSummaryPanel` 仍是 `UniformGrid`，真实统计绑定未改变；默认 `TabControl.SelectedIndex=1` 展示当前游戏媒体。
- Media 当前媒体仍使用生产 `ListBox + VirtualizingStackPanel`，不是 UiLab 的非虚拟化 `WrapPanel` 缩略图网格；这是为大媒体库和现有滚动性能保留的生产适配边界，不要为了像素复制而关闭虚拟化或迁移 demo 滚动条。
- `ApplyResponsiveLayout` 在 700-720 DIP 常规窗口保持 236 DIP 表格下限；宽屏恢复会把已选择媒体的 Inspector 重新设为 Visible，窄屏继续由 `MediaCompactDetailsButton` 控制 Inspector。
- UI-210 自动验证：Playnite 303/303，生产 Release 0 warning/0 error，Media Light/Dark 多尺寸和 resize transition 通过；全量 render-qa 仅剩 Save 候选表历史窄视口门禁。

## 2026-08-16 UI-211 Save 当前事实

- `SaveCenterView.ApplyResponsiveLayout` 的表格高度公式为 `Math.Max(180d, Math.Min(252d, height - 464d))`；常规 700-720 DIP 窗口的 SaveCandidateGrid/SaveHistoryGrid 不得回到 236 DIP 以下。
- Save 历史/候选 DataGrid 仍使用生产共享表头、Item scrolling、行/列虚拟化和现有 Inspector 抽屉；只修复视口下限，没有替换滚动条或绑定。
- UI-211 自动验证：Playnite 303/303，生产 Release 0 warning/0 error，全量 RenderHarness `render-qa OK`（Light/Dark、7 页面、多尺寸、resize transition）。

## 2026-08-16 UI-205-ACRYLIC-PARITY 当前事实

- 权威视觉来源是 `D:\workplace\github\GameSaveCenter.AcrylicFork`（当前参考提交 `b09cba6`），不是 `GameSaveCenter.UiLab`。生产已迁移其页面层级、颜色/表面层级、圆角尺度、按钮/标题比例、Dashboard 开放式页面头部和 Settings 分类栏；不要回头修复 AcrylicFork 样板自身的布局 bug。
- 明确排除 AcrylicFork 演示数据、右上角颜色/主题按钮和样例滚动条。生产继续使用真实数据/命令/绑定/虚拟化和现有带 Track 绑定的滚动条。
- AcrylicFork 顶部色板作为主题参考：靛蓝 `#7C8CF8`、天蓝 `#4FA3F0`、青碧 `#35B8C9`、薄荷 `#4CC08A`、紫罗兰 `#A07BF5`、琥珀 `#E8973C`、玫瑰 `#E56E8C`。生产默认使用靛蓝基线，同时继续尊重 Playnite/强制主题设置，不增加演示色板按钮。
- 共享几何：Shell 20、Card 16、Control 10；Settings 实际内容宽度 ≥700 DIP 使用左侧分类栏，<700 DIP 顶部横向分类，<620 DIP 收紧窄屏标题和字段；这三个断点与 `GameSaveCenterSettingsView.xaml.cs` 保持一致。
- Overview 的全局命令只在 Dashboard 页面头部显示；`OverviewHomeToolbar` 只有 `IsOnboardingPending=True` 时显示，不能重新改成普通状态下的重复命令卡。
- DataGrid 表头与 Button/Card 已按生产共享模板收敛圆角；不要把 AcrylicFork 的滚动条模板覆盖到生产，也不要给可选 `DataGridColumnHeader.Tag` 增加 `CornerRadius` 绑定，Playnite 生成 filler header 可能得到 `UnsetValue`。
- 安装器修订为 `DEV-INSTALL-008`：只停止当前生产扩展目录下的 Worker；其他扩展的 Worker 留在原处，路径不可读取时仍 fail-closed。一次点击安装已成功，生产 Worker 与 AcrylicFork 外来 Worker 可并存。
- 本阶段验证基线：Release 构建 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 302/302；`validate-source.py`、XAML 检查、render-qa（Light/Dark、7 页面、多个尺寸与 resize transition）全绿。
- 离屏截图不是真实 Playnite 嵌入式像素真值；本阶段已真实安装并启动 Playnite/生产 Worker，但没有把宿主窗口像素冒充为自动视觉证据，主题/DPI/键盘/连续缩放仍需人工确认。

## 2026-08-16 UI-205-REAL-HOST-MIGRATION-FIX 当前事实

- Playnite BAML 不应在生产 XAML 中直接写 `clr-namespace:GameSaveCenter.Contracts;assembly=GameSaveCenter.Contracts`：扩展程序集位于 Playnite 私有目录时，BAML resolver 可能从默认 AppDomain 解析失败，即使安装目录实际包含 Contracts.dll。需要使用生产程序集内的 `GameSaveCenter.Playnite.XamlValues` 包装属性；属性返回真实 Contracts enum object，因此不改变绑定/DataTrigger 语义。
- Dashboard 选中游戏标题栏必须在 Grid 重排后再次按 `TransformToAncestor` 的实际 X 坐标限制宽度；首次 measure 的旧 DesiredSize 会让 `SelectedGameHeaderLayout` 和按钮短暂超过页面右边界。`ApplicationIdle` 二次布局与审计的 ApplicationIdle 等待是配套约束，不能只修截图审计。
- `RealHostUiAuditService.CheckChildLayoutOverflow` 复用输出目录时，干净轮次必须删除旧 `CHILD_LAYOUT_OVERFLOW.json`；最终状态以当前 `overflow-classification.json` 为准，不能用旧门禁文件判断本轮。
- 本轮安装验证：`extension.yaml 0.6.70`、生产 DLL `0.6.70.0`，日志确认 `GameSaveCenter 0.6.70.0 loaded`，没有新的 `XamlParseException`/Contracts 缺失；外来 `GameSaveCenterPreview` Worker 不得结束。
- 本轮自动基线：Core 59/59、Worker 191/191、Playnite 303/303，Release 0 warning/0 error；受控矩阵的最终 `RealFixedLayoutOverflow=[]`。由于 UIAutomation 未定位到 Playnite 侧栏，`EmbeddedDashboardCaptured=false` 仍是诚实门禁，受控窗口不能替代真实嵌入像素。

## 2026-08-16 UI-REAL-HOST-AUDIT-BLOCKERS-FIX 当前事实

- Audit CommitSha 必须可追踪：脚本设置 `GSC_UI_AUDIT_COMMIT`，unknown 触发 `AUDIT_SOURCE_REVISION_MISSING` HIGH。
- Embedded 判定用 `IsGenuinelyEmbeddedDashboard`（IsLoaded + PresentationSource + Window 非 fallback）；headless 无人点击时必须诚实 false + HIGH gate。
- `SafeFileName` 不能再用 `string.Join("-", chars)`；已修复为仅替换非法字符并折叠连续 `-`。
- Overflow gate 必须分类：fixed/scroll/decorative；ScrollViewer 内容与装饰性越界不误报。
- Resize 后必须等待 DataBind/Loaded/Render/Idle 且连续两次几何差 ≤0.5 DIP 才截图（最多 3 pass）。
- Manifest 按 `Scope=Dashboard|Settings` 隔离；内部滚动器（DG_ScrollViewer/PART_ContentHost/TextBox/ComboBox）默认排除。
- 基线：Playnite 302/302、Worker 191/191、Core 59/59；Release 0 warning/0 error。

## 2026-08-16 UI-HOST-AUDIT-TRUTHFULNESS-FIX 当前事实

- Real Host Audit 的 origin 必须显式：`DashboardView.AuditHostKind`（默认 EmbeddedPlaynite，fallback 专用窗口设 ControlledAuditWindow）；不要用 `auditDashboardWindow==null` 推断。
- Sidebar View 不能调用 `Activated`；真实 embedded Dashboard 只能由用户在 Playnite 点击侧栏后经 `Opened` 加载，`OnLoaded` 触发捕获。无人点击时输出必须写 `EmbeddedDashboardCaptured=false`。
- `AuditCaptureSession` 隔离三类 manifest：EmbeddedDashboard / ControlledDashboard / Settings；settings manifest 不允许混入 Dashboard entries。
- DataGrid（`CanContentScroll=true`/`ScrollUnit=Item`）是逻辑 item 单位，禁止复用像素 stitch；`DG_ScrollViewer`/`PART_ContentHost` 默认排除。
- `summary.json` 是硬门禁：`EmbeddedDashboardCaptured` / `EmbeddedSettingsCaptured` / `ControlledDashboardCaptured` / `VisualSourceOfTruthAvailable`。
- 基线：Playnite 294/294、Worker 191/191、Core 59/59；Release 0 warning/0 error。

## 2026-08-16 UI-REAL-HOST-CAPTURE-COMPLETENESS-FIX 当前事实

- Real Host Audit 输出语义已重构为三类，不再用“最大 ScrollViewer”冒充整页：
  - `embedded-current/viewport/`：真实 Playnite 嵌入 Dashboard 的当前视口（production visual truth；本次 headless 会话不可用时会如实标注）。
  - `controlled/<profile>/<theme>/viewport/`：无边框审计窗口，profile 即 client size，Dashboard Stretch。
  - `scroll-surfaces/<route>__<name>.png`：每个 meaningful ScrollViewer 的完整 extent。
- 关键实现：无边框窗口（client == outer）、Dashboard `ClearValue(Width/Height)` + Stretch、`SaveViewport` 校验 `Actual*DpiScale` 输出尺寸、`CAPTURE_VIEWPORT_CLIPPED`/`CAPTURE_PROFILE_SIZE_MISMATCH` gates、`capture-manifest.json`。
- 元数据：`Mode` 只能是 `embedded-current` 或 `controlled-host-window`；`CaptureOrigin`、`DedicatedAuditWindowUsed`、`ProfileSizeApplied`、`ThemeOverrideApplied` 必填；PlayniteDesktopVersion 取宿主 exe 文件版本，失败写 `unknown`。
- 基线：Playnite 287/287、Worker 191/191、Core 59/59；Release 0 warning/0 error。

## 2026-08-15 LUDUSAVI-DIAGNOSTICS-FIX 当前事实

- 备份失败常见根因之一是 Ludusavi 联网更新 manifest 超时（`raw.githubusercontent.com/.../manifest.yaml`）；这不是存档路径/ZIP 写入问题，网络恢复后重试即成功。
- 插件侧已修复三项放大问题：外部进程 stdout/stderr 按 UTF-8 解码；`LudusaviCommandResult.RawOutput` 在失败时保留原始输出；剪贴板复制带重试与失败降级（`CopyTextWithRetry`）。
- 相关代码：`ExternalProcessRunner.cs`、`LudusaviClient.cs`、`DashboardViewModel.CopyTextWithRetry`。
- 基线：Worker 191/191、Playnite 281/281；Release 0 warning/0 error。

## 2026-08-15 UI-REAL-HOST-AUDIT-NESTED-TABS-THEMES 当前事实

- 真机审计现在按 5 档窗口尺寸 × `Light`/`Dark` 双主题捕获；每个尺寸/主题目录含 Dashboard、6 个工作区、全部顶层 Tab 与嵌套 Tab（如“异常与审计”→“审计记录”）。
- 嵌套 Tab 捕获要点：选择父 Tab 后等待 `ApplicationIdle`，分别从 TabItem 视觉树和 `tab.Content` 两个路径找子 TabControl，再递归；单纯视觉树遍历会漏掉部分嵌套页。
- 整页截图统一渲染 Dashboard/Settings 根元素（含侧栏和外壳），高度按内容最大滚动范围撑高（maximized 下工作区 1707×1232、Overview 1707×1717）；不要改成只渲染内部滚动器内容，否则会缺左右外壳。
- 设置页兜底窗口必须注入 `GameSaveCenterSettings` 作为 DataContext，否则主题/玻璃设置是默认值。
- 产物：`artifacts/ui-host-audit/screenshots/<size>/<light|dark>/`，zip `artifacts/GameSaveCenter-ui-host-audit.zip`。
- 基线：Release 0 warning/0 error；`validate-source.py`、`check-xaml.ps1` 通过。真实第三方主题/Playnite 窗口内观感仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-REAL-HOST-AUDIT-MULTI-SIZE 当前事实

- 真机审计按 5 档窗口尺寸捕获：`maximized`（WorkArea 1707×912 DIP）、`1600x1000`、`1366x768`、`1280x720`、`1024x768`；每档包含 Dashboard、6 个工作区、全部内层 Tab、窗口截图与 Settings 5 分类。
- Playnite 进程 DPI-unaware：窗口尺寸必须用 `SystemParameters.WorkArea`，不能用 `GetSystemMetrics`（会返回 39×24 虚拟值导致窗口 640×480）；窗口需 `SizeToContent.Manual` 并显式设置视图宽高。
- 多尺寸扫描不启用全页滚动拼接（避免大表格十几分钟卡死）；内容按逻辑分辨率输出防止 OOM。
- 产物：`artifacts/ui-host-audit/screenshots/<size>/` 与 `metadata-<size>.json`；zip 为 `artifacts/GameSaveCenter-ui-host-audit.zip`。
- 清理规则已写入 AGENTS.md「文件清理规则」与 DEVELOPMENT_HANDOFF：每轮完成后删除旧 `dev-build`、`ui-audit-build`、`phase*`、`audit*`、旧 zip 与 `.tmp` 旧目录，只保留当前安装目录与审计证据。
- 基线：Release 0 warning/0 error；`validate-source.py`、`check-xaml.ps1` 通过。真实第三方主题/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-REAL-HOST-AUDIT-FULL-COVERAGE 当前事实

- 真实宿主审计已覆盖 Dashboard 全部页面/Tab 与 Settings 全部 5 个分类；`artifacts/ui-host-audit/` 是最新证据，zip 为 `artifacts/GameSaveCenter-ui-host-audit.zip`。
- 无交互桌面（Playnite 主窗口不可见）时，审计通过专用窗口兜底：Dashboard 1440×900、Settings 同样 1440×900、左上锚定、ToolWindow 可关闭；不再出现越界不可关窗口。
- 设置页兜底的关键约定：输出根在 Dashboard 完成前缓存并传给 Settings 兜底，且窗口创建必须使用 Dashboard 的 UI Dispatcher（线程池 Dispatcher 不会显示窗口）。
- 设置分类 Header 为复杂 Grid，文件命名须从 Header 视觉树提取中文文本，不能用 `Header.ToString()`（会全部同名）。
- 默认 zip 被其他进程占用时审计会写 `GameSaveCenter-ui-host-audit-<时间戳>.zip`，不会中断 Settings 捕获。
- 基线：Release 0 warning/0 error；Playnite 281/281；`validate-source.py`、`check-xaml.ps1`、WPF UI 校验 0 errors。
- 真实第三方主题、连续缩放、用户实际 Playnite 窗口尺寸仍为 `MANUAL QA REQUIRED`；无交互桌面证据不能冒充真实窗口像素。

## 2026-08-15 UI-REAL-HOST-PARITY-CLOSURE 当前事实

- 来源：`GameSaveCenter_RealHost_UI_Parity_Audit_Prompt.zip`，计划 `docs/ai/REAL_HOST_UI_PARITY_CLOSURE_PLAN.md`，报告 `docs/ai/REAL_HOST_UI_PARITY_CLOSURE_REPORT.md`。
- Audit 定位：Tier A `capture-ui-audit.ps1` 是 Offscreen Regression Audit（几何/滚动/虚拟化/fidelity 门禁，不是视觉真值）；Tier B `real-host-audit.ps1` 才是真实 Playnite 视觉事实来源。
- 插件内 `RealHostUiAuditService`：`GSC_REAL_HOST_AUDIT` 或 `%LOCALAPPDATA%\GameSaveCenter\real-host-audit.request` 触发；从真实 Dashboard/Settings 捕获截图、visual tree、resource snapshot、style fingerprint、真实 DPI/bounds；不触发备份/恢复/删除等业务命令。
- `UiDiagnosticsExporters`：resource snapshot / style fingerprint / visual tree / PNG 导出；`AdaptiveThemePaletteContrastGuard`：palette 对比守卫。
- 本机证据：`artifacts/ui-host-audit/` + `artifacts/GameSaveCenter-ui-host-audit.zip`；DPI 1.5，Dashboard 1264×868，runtime palette（accent #0379FF、Glass alpha 0.78-0.94 等）。
- 离屏更漂亮的根因：离屏用 DesignTokens fallback palette；真实宿主用 AdaptiveThemePaletteFactory runtime palette + 真实 DPI/host bounds/data；当前无证据显示 surface hierarchy 被压平，故未改 palette。
- 协定：AGENTS.md / DEVELOPMENT_HANDOFF 已写明每轮完成后 Agent 自己 commit 并 push。
- 基线：Playnite `281/281`；render-qa 全绿；Offscreen UI Audit 0 HIGH/0 MEDIUM/0 fidelity/0 failed routes。
- 真实 125-200% DPI、第三方主题、连续缩放与 Settings paired evidence 仍需人工/下次脚本运行确认。

## 2026-08-15 UI-AUDIT11-RESIDUAL-CLOSURE 当前事实

- 来源：`GameSaveCenter_Audit11_Residual_UI_Closure_Prompt.zip`，计划 `docs/ai/UI_AUDIT11_RESIDUAL_UI_CLOSURE_PLAN.md`，报告 `docs/ai/UI_AUDIT11_RESIDUAL_UI_CLOSURE_REPORT.md`。
- SaveHistory 大小列使用 `SaveSizeValue`（`TextTrimming=None`，`Tag=SaveHistorySize`），列宽 116 DIP；narrow 状态列保留。
- Maintenance Device Inspector 在 Compact/Narrow 默认收起，独立“查看设备详情 ›”按钮，展开 viewport >= 180 DIP，表格 MinHeight 150（header + 2 行）。
- Audit fidelity 新增 `SHORT_SEMANTIC_VALUE_TRIMMING` 与 `INTERACTIVE_INSPECTOR_USABILITY`，均为 MEDIUM 且触发即失败。
- Settings 分类滚动目标整数取整；`ACTIVE_TAB_VISIBILITY` 保持 0。
- 基线：Playnite `276/276`；render-qa 11 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/0 fidelity/0 failed routes。
- 审计 ZIP：`artifacts/audit11-final/GameSaveCenter-ui-audit.zip`（标准路径被外部进程锁定）。
- 真实 Playnite 宿主主题/DPI 125%/150%/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-FIDELITY-CLOSURE-AUDIT10 当前事实

- 来源：`GameSaveCenter_UI_Fidelity_Closure_Audit10_Prompt.zip`，计划 `docs/ai/UI_FIDELITY_CLOSURE_AUDIT10_PLAN.md`，报告 `docs/ai/UI_FIDELITY_CLOSURE_AUDIT10_REPORT.md`。
- Maintenance 不再有局部 implicit `DataGridColumnHeader` style；真实列统一走 `GscDataGridColumnHeaderStyle`，中间表头全部渲染。
- Media 搜索框为 `Auto/*(MinWidth=160)/Auto/150` Grid；narrow 内容宽约 390 DIP。
- Settings 选中分类在 SelectionChanged/ApplyResponsiveLayout 后同步 scroll-into-view（BringIntoView + 增量 delta 收敛）。
- Save History narrow 收起备注列保留状态列；完整备注在版本详情 Inspector。
- Audit fidelity 门禁：`HEADER_CONTENT_FIDELITY` / `ACTIVE_TAB_VISIBILITY` / `CONTROL_USABILITY_GEOMETRY` / `ESSENTIAL_COLUMN_VISIBILITY` 均为 MEDIUM 且触发即失败。
- 基线：Playnite `273/273`；render-qa 11 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/0 failed routes/0 fidelity。
- 真实 Playnite 宿主主题/DPI 125%/150%/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-POST-TYPOGRAPHY-GEOMETRY-CLOSURE 当前事实

- 来源：`GameSaveCenter_PostTypography_Geometry_Audit_Fix_Prompt.zip`，计划 `docs/ai/UI_POST_TYPOGRAPHY_GEOMETRY_CLOSURE_PLAN.md`，报告 `docs/ai/UI_POST_TYPOGRAPHY_GEOMETRY_CLOSURE_REPORT.md`。
- Maintenance 诊断与异常审计的“等级”列统一使用 `GscSeverityColumnWidth`（DataGridLength 92 DIP），不再有 72 DIP 挤压。
- UI Audit Text-Fit：`UiLayoutAnalyzer` 用 `FormattedText` 无约束宽度对比 `ActualWidth`；`TEXT_FIT`=MEDIUM 且 `UiAuditRunner` 遇任何 TEXT_FIT 返回失败码；wrap/ellipsis 文本不误报。
- visual-tree：exporter 不能用 `IsVisible`（离屏 host 无 PresentationSource 恒 false），改用 `Visibility == Visible`；当前 175 个 JSON 非空。
- 基线：Playnite `268/268`；render-qa 11 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/0 failed routes/0 TEXT-FIT。
- 真实 Playnite 宿主主题/DPI 125%/150%/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-FINAL-TYPOGRAPHY-RESPONSIVE-CLOSURE 当前事实

- 来源：`GameSaveCenter_Final_UI_Typography_Prompt.zip`，计划 `docs/ai/UI_TYPOGRAPHY_RESPONSIVE_CLOSURE_PLAN.md`，报告 `docs/ai/UI_TYPOGRAPHY_RESPONSIVE_CLOSURE_REPORT.md`。
- 字体 token：`GscUiFontFamily = Segoe UI Variable Text, Segoe UI, Microsoft YaHei UI`，`GscCodeFontFamily = Consolas, Microsoft YaHei UI`；普通 UI 无硬编码 UI 字体；图标字体 `Segoe MDL2 Assets` 与代码字体 `Consolas` 保留；通用按钮默认 Medium，Primary 保留 SemiBold。
- Settings Compact/Narrow：长说明/副标题/保存提示按断点隐藏，header 最小高度 56-76 DIP；render-qa 760×560 正文 viewport 300 DIP、880×560 285 DIP。
- Save Compare Narrow：主比较区 MinHeight 240、MaxHeight `max(300, height*0.52)`；1040×700 主比较 viewport 234 DIP，保留策略 246 DIP。
- Compact Inspector：Save/Trainer/Media/Task 五个详情按钮均为表格下方独立 `Grid.Row=1` 操作行，无 overlay。
- Media 待归类底栏与表格内容左边缘统一 12 DIP padding。
- 基线：Playnite `266/266`；render-qa 11 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/0 失败路由。
- 真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-FINAL-POLISH-V7.1 当前事实

- 来源：`GameSaveCenter_UI_Final_Polish_Pack_v7_1.zip`，计划 `docs/ai/UI_FINAL_POLISH_PLAN_V7_1.md`，报告 `docs/ai/UI_FINAL_POLISH_REPORT_V7_1.md`。
- 首页活动行五列：Icon/Scope/Message(*)/MetaChip/Time；chip 独立横向列组，Time 右留白 20 DIP。
- `POSSIBLE_CLIPPING=0`；Audit 消息含元素名/父元素/文本，且按 Margin 修正误报。
- 基线：Playnite `263/263`；render-qa 11 档 + 56 主题 + 7 Resize 全绿；最终 Audit 0 HIGH/0 MEDIUM/0 失败路由。
- 最终 Audit ZIP：`artifacts/GameSaveCenter-ui-audit.zip`（Commit `f6f17a8`）；提交 `702b0d5`、`f6f17a8`。

## 2026-08-15 UI-FINAL-CLOSURE-V7 当前事实

- 来源：`GameSaveCenter_UI_Final_Closure_Pack_v7.zip`，计划 `docs/ai/UI_FINAL_CLOSURE_PLAN_V7.md`，报告 `docs/ai/UI_FINAL_CLOSURE_REPORT_V7.md`。
- Audit 工具已支持嵌套子路由与 expected/actual 主表断言；Settings 标题解析为真实分类名。
- 共享 `DataGridStarFill` 附加行为修复宽屏星号列：2K 六张表 ColumnFillRatio=1.00，MaintenanceProcess 目标游戏 1549 DIP；横向滚动 Disabled，Save/Task <1200 DIP 时 Inspector 收起。
- Task 根改为有限 Grid；Media Inbox/Current 取消 460 上限；Task/Media 主行 VerticalFillRatio=1.00。
- Maintenance 表头白块清零；Progress 模板补 `PART_Track` 并新增专用 track/fill token；单行 TextBox `PART_ContentHost` Stretch + Padding 收口。
- 基线：Playnite `263/263`；render-qa 11 档 + 56 主题 + 7 Resize 全绿；最终 Audit 0 HIGH/0 MEDIUM/0 失败路由。
- 最终 Audit ZIP：`artifacts/GameSaveCenter-ui-audit.zip`（Commit `90738b7`）；Progress probe：`artifacts/ui-qa/v7-progress/`。
- 提交：`5cd0226`、`58191d5`、`494b402`、`87d0553`、`7eaaacd`。真实 Playnite 主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-FEEDBACK-GLOBAL-ACTIVITY-CHIP-CENTER

- 首页“全局活动”的 Kind/Result chip 文字已强制水平/垂直/文本三向居中（`OverviewView.xaml` 宽窄两套共 4 个 TextBlock），并有 `UiLayoutRegressionTests` 回归断言锁定。
- 该修正属于 v6.2 之后的用户反馈补丁，提交 `d962b4d`；Playnite `263/263`、XAML/source 门禁与截图均通过。

## 2026-08-15 UI-TABLE-AND-CHIP-CLOSURE-V6.2 当前事实

- 来源：`GameSaveCenter_UI_Table_and_Chip_Fix_Pack_v6_2.zip`，计划 `docs/ai/UI_TABLE_AND_CHIP_CLOSURE_PLAN_V6_2.md`，报告 `docs/ai/UI_TABLE_AND_CHIP_CLOSURE_REPORT_V6_2.md`。
- Chip 已统一为圆角矩形：`GscRedesignContextPill`（CornerRadius 7、MinHeight 26）、`GscRedesignTableStatusPill`（CornerRadius 7）。
- 共享 `DataGridCell` Padding `12,8,20,8`；Overview 时间列 `Margin=12,0,20,0`，六列 `40|150|*|96|84|112`。
- SaveCandidate 可信度列是 ProgressBar（Height 8、Maximum 1、`Value={Binding Score}`）+ `P0` 文本；Task/Overview 已有真实进度条，Settings 数值不是业务进度。
- Maintenance 四个主表 `MaxHeight=PositiveInfinity`；`MaintenanceDeviceLayout` / `MaintenanceProcessLayout` 为 `VerticalAlignment=Stretch`。2K/4K fill ratio：Diagnostics 0.89/0.93、Device 0.82/0.88、Audit 0.88/0.92、Process 0.90/0.93。
- 当前基线：Release 0 warning/0 error；Playnite `263/263`；render-qa 11 档（含 3840×2160）+ 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/8 EXPECTED INFO/0 失败路由。
- v6.2 截图：`artifacts/ui-qa/v6-2-shots/`，命令 `scripts/capture-v6-2-shots.ps1`。
- 提交：`c58b359`、`6a68a59`。真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-OVERNIGHT-CLOSURE-V6 当前事实

- 页面历史已改为 Playnite 会话级：`GameSaveCenterPlugin.SessionLastWorkspace`；首次打开 Overview、同会话恢复、重启回 Overview；`Settings.LastWorkspace` 保留但不再作为启动依据。
- `GscNumericFieldInput` 根模板已修：`PART_ContentHost` 绑定垂直内容对齐；数字 1/5/30/120/1440 完整居中。
- 全局活动为轻量六列表格（40/150/*/88/76/112）+ header；Overview 主列/次列 disabled ScrollViewer 已改为 Grid；Maintenance Device/Process 与 Media Current 外层 ScrollViewer 已改为有限 Grid。
- Task/Media 筛选带语义前缀；Device/Process 主表最小视口 252 DIP。
- 基线：Release 0 warning/0 error；Playnite `261/261`；render-qa 10 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/8 INFO/0 TRUE_PARENT_CHILD_SCROLL_CONFLICT。
- v6 截图：`artifacts/ui-qa/v6-shots/`；命令 `scripts/capture-v6-shots.ps1`。
- 提交：`baa8f72` 计划及后续实施/文档提交见 `git log`。真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-OVERNIGHT-FIX-V4 当前事实

- 来源：`GameSaveCenter_UI_Overnight_Fix_Pack_v4.zip`，计划在 `docs/ai/UI_OVERNIGHT_FIX_PLAN_V4.md`，报告在 `docs/ai/UI_OVERNIGHT_FIX_REPORT_V4.md`。
- `GscDisclosureCard` 已升级：独立 chevron 图标区、垂直居中、无尾部 `>`；所有页面 Expander 统一引用且折叠体内不再内滚。
- 维护中心诊断页已拆成二级 Tab：默认 `问题列表`（FindingsGrid 独占），次项 `诊断概览`（环境/操作/摘要共用页面滚动）。旧内部 ScrollViewer 已删除。
- 存档备份自动化与策略模板的数值输入全部补齐 label/unit/helper；共享样式 `GscFormFieldLabel`、`GscFormFieldHelper`、`GscNumericFieldInput`。
- 首页全局活动行高 60 DIP、图标居中、列 `40/*/Auto(180)/112`。
- 当前基线：Release 0 warning/0 error；Core `59/59`、Worker `190/190`、Playnite `255/255`；render-qa 10 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/39 INFO/0 失败路由。
- v4 截图：`artifacts/ui-qa/v4-shots/`；命令 `scripts/capture-v4-shots.ps1`。
- 用户后续反馈已修复：折叠 header 文字与图标垂直居中；`GscNumericFieldInput` 数字水平/垂直居中显示，框尺寸不变。
- 提交：`3015182`、`5131e4d`、`0201615`、`5196f4a`、`fc86ecc`。真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-14 UI-VISUAL-REWORK-V3 当前事实

- 来源：`GameSaveCenter_UI_Design_and_Prompt_Pack_v3.zip`，计划在 `docs/ai/UI_VISUAL_REWORK_PLAN_V3.md`。
- Overview：当前游戏卡三按钮同排同几何；最近 30 天动作/统计/折叠三层分离；全局活动为轻量表四列，Time 固定 112 DIP，窄窗 chips 下移。
- Save：当前存档规则状态一行 badge 并按 `SelectedGame.HealthState` 着色，三按钮统一紧凑几何，卡片压高。
- Maintenance：环境卡摘要化，首次环境检查/更多维护操作统一 `GscDisclosureCard`（去尾部 `>`），FindingsGrid 五列最小宽度收敛为 72/120/160/*180/140；两个主 Disclosure 内容使用内部有限滚动，展开不挤压主表。
- Disclosure 统一入口：`GscDisclosureCard`（别名 `GscDisclosureCardExpander` 保留），Chevron 独立图标区、整行可点、Hover/Expanded 主题态；Media/Save/Task 的旧 `GscExpander` 引用已全部替换，页面不再引用旧样式。
- 颜色分层全部使用 DynamicResource/Design Token：正常绿、信息蓝、警告橙、错误红、中性灰蓝；未写死前景/背景。
- 功能保真：REMOVE=0；命令、绑定、DataGrid 5 列、EnvironmentCheckItems、虚拟化和 GamePicker HARD LOCK 均未改。
- Overview Hero/当前游戏列保持 1:1，确保 1536×864 等常用窗口下三个操作按钮同一行；render-qa 会检查 Overview/Save 三按钮的 Y 坐标与高度差。
- 当前自动化基线：Release 0 warning/0 error；Core `59/59`、Worker `190/190`、Playnite `253/253`；render-qa 10 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/32 INFO/0 失败路由。
- 截图证据：`artifacts/ui-qa/v3-shots/` 10 张（当前游戏卡、保护折叠/展开、活动宽/窄、Save 标准/窄、Maintenance 初始/两个展开态），生成命令 `scripts/capture-v3-shots.ps1`。
- 提交：`5c3bdae`（v3 计划）、`9ee3660`（Overview）、`e8b8c31`（Save/Maintenance），最终补强与文档提交见 `git log`。真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-14 UI-REFACTOR-V1（实施包 v1）当前事实

- 本轮是严格受控 WPF UI 重构，不是业务重写。事实来源优先级（当时记录，已由 2026-08-20 Demo-first 总规则覆盖）：当前生产 main > UI Audit（commit `4ab44fe`）> 实施包 v1 锁定/范围 > WPF Demo v6.1 > 旧布局。
- 完整功能保真计划在 `docs/ai/UI_REFACTOR_FIDELITY_PLAN.md`：覆盖 92 条命令、43 个 DataGrid 列、30 个 ScrollViewer、143 个条件 UI；默认禁止 `REMOVE`，只允许 `KEEP/MOVE/RESTYLE/COLLAPSE/RESPONSIVE_MOVE`。
- Dashboard 顶部全局 GamePicker 绝对锁定，必须是 Dashboard 单实例共享控件，在六个工作区永久常驻；首页“今日工作台 / TODAY / 当前游戏”只做布局、间距和响应式修正。
- 已知必须修复的 Audit 症状：SaveCandidateGrid 约 3.7 行、MaintenanceAuditLogGrid 约 1.6～1.9 行、MaintenanceDeviceGrid/ProcessGrid narrow 约 3.7 行、诊断 13 工具 narrow 138 DIP 按钮墙、多处 Page Scroll + DataGrid/List Scroll 嵌套。
- Phase 0 基线：Release 构建 0 警告/0 错误，Core `59/59`、Worker `190/190`、Playnite `238/238`，source/XAML/WPF/render-qa 全绿。后续按 Phase 1～8 分阶段独立提交并 push。
- Phase 1（共享布局基础）已交付：`Redesign.xaml` 新增 `GscInternalTabControl`、`GscInternalTabItem`、`GscToolbarActionRow`、`GscToolbarOverflowButton`，并由 `WpfUiResourceDictionaryTests` 锁定；未改任何 View 页面与业务。
- Phase 2（首页 Overview）已交付：六项 Snapshot 指标改为响应式紧凑 Summary Strip（6/3/2 列），最近 30 天保护明细默认折叠到共享 Expander，全局活动改为稳定四列；`OverviewStatStrip` 响应式列数、保护明细可达性、全局活动四列由新回归测试锁定。GamePicker 与首页锁定结构未改。
- Phase 3（存档中心）已交付：历史/候选窄窗 Inspector 默认收起为“查看详情”按钮，主表高度在 1040×700 分别提升到约 385/254 DIP；候选页头部压成单行；策略模板区默认折叠但全部命令可达。新增窄窗 Inspector 切换回归测试。
- Phase 4a（修改器中心 Trainer）已交付：已绑定工具页窄窗默认收起工具设置 Inspector 为详情按钮，1040×700 工具列表视口 236 DIP；新增 Trainer 窄窗切换回归测试。FLiNG/可下载版本/导入流程未改。
- Phase 4b（媒体中心 Media）已交付：当前媒体窄窗 Inspector 默认收起为详情按钮，来源规则添加表单默认折叠但字段可达；新增 Media 窄窗切换与来源表单折叠回归测试。待归类 DataGrid 与媒体异步缩略图未改。
- Phase 5（任务中心 Task）已交付：游戏筛选在 compact 进入“更多筛选”Expander、wide 回到主行；任务详情 Inspector 窄窗默认收起为详情按钮；操作行保持横向；任务表 1040×700 视口 252 DIP。新增 Task 窄窗切换与更多筛选移动回归测试。
- Phase 6（维护中心 Maintenance）已交付：诊断常用按钮收敛为主行 5 个，低频命令进入共享 Expander；审计日志表视口提升到 280 DIP（约 6 行）；设备/进程/Findings 主表保持 350 DIP。保留策略与全部维护命令未改。
- Phase 7（设置轻量统一）已交付：设置字段标签列宽 token 化为 `GscSettingsFieldColumnWidth`；五个设置分区与保存语义未改。
- Phase 8（最终回归）已交付：Audit HIGH 从 10 清零、MEDIUM 从 4 降到 0，失败路由 0；最终测试基线 Core 59/Worker 190/Playnite 250；真实宿主主题/DPI/连续缩放仍为 MANUAL QA REQUIRED。
- Phase 8 收口：`OverviewView.xaml` 把“当前游戏”卡片 3 个操作按钮底部边距从 8 收到 4 DIP，消除最后一个 Audit MEDIUM（未命名 WrapPanel 92 DIP）；顶部工作台工具栏使用 `Padding="14,10"`，1040×700 下由 91 降到 79 DIP。无 REMOVE，GamePicker 与 Dashboard 锁定区域未改。
- 扩档验证：render-qa 覆盖 10 档逻辑尺寸（1040×700 / 1100×720 / 1280×720 / 1366×768 / 1536×864 / 1600×900 / 1707×960 / 1920×1080 / 2048×1152 / 2560×1440）；UI Audit 新增 2K 与 1100×720 尺寸，快照 161，HIGH/MEDIUM 均 0，运行时警告 39。Audit 工作区高度已改为窗口高度，与生产 Dashboard 和 render-qa 一致。
- 主题 QA：RenderHarness 对 7 个工作区 × 4 尺寸 × Light/Dark 共 56 个离屏场景渲染并校验调色板与视口，全部通过；像素采样确认 Light/Dark 背景确实切换。真实 Playnite 宿主主题仍为 MANUAL QA REQUIRED。
- 页面级横向溢出门禁：render-qa 与主题 QA 要求 `*ScrollSurface` / `SettingsScroller` 的 `hbar=Disabled` 且无横向溢出；DataGrid 内部列滚动允许。10 档尺寸与 56 主题场景均通过。
- Resize 恢复：render-qa 新增 2560×1440 → 1100×720 → 2560×1440 同实例布局恢复探针，7 个工作区全部恢复；修复 Save/Task/Trainer Inspector 宽窗不恢复的缺陷，新增 3 条回归测试。
- 验收审计：`docs/ai/UI_REFACTOR_ACCEPTANCE_AUDIT.md` 已落盘，逐项映射实施包验收清单；真实 Playnite 宿主主题/DPI/连续缩放与大数据滚动仍为 MANUAL QA REQUIRED。
- 真实宿主 reload 已验证：`dev-install-run.ps1 -Configuration Release` 成功安装并启动 Playnite；`playnite.log` 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`，扩展日志记录 `0.6.70.0 loaded`，Worker 从当前扩展目录运行，`18:10` 后无 ERROR/Exception/crash。
- Visual Correction v2 已完成：Overview 单滚动、风险卡去内滚、Disclosure、活动行响应式、Save 卡片、Diagnostics 去父子双滚动、Audit 二级切换；新增 OV/SAVE/MAINT 断言，最终 Audit HIGH 0、MEDIUM 0、运行时警告 33。
- Visual Correction v2 真实宿主 reload 已验证：Playnite 加载 `GameSaveCenter 0.6.70`，扩展日志确认 `0.6.70.0 loaded`，Worker 从当前扩展目录运行，`20:39` 后无 ERROR/Exception/crash。

## 当前事实覆盖（2026-08-14 Layer A 收口、Layer B 13 项与 Layer C 11 项）

- `UI-AUDIT-001` 已交付（提交见 `git log -1`）：开发专用 UI 自动审计工具由 `scripts/capture-ui-audit.ps1` / `GameSaveCenter-UI-Audit.cmd` 启动，复用 RenderHarness 渲染真实生产视图；自动扫描 XAML 生成路由/Manifest/保真矩阵，输出视觉树与布局 JSON；页面级滚动容器直接渲染完整内容，DataGrid/ListBox 逐段滚动拼接 `-scroll-*.png`；覆盖 maximized/2k/wide/standard/compact/narrow-1100/narrow，最终 ZIP 在 `artifacts/GameSaveCenter-ui-audit.zip`。后续新增页面只要放入 Dashboard 或 `Views` 目录并保持无参构造，静态盘点与运行时路由会自动纳入。
- 用户日志中的“编译解决方案”失败根因是旧 `dotnet/testhost` 或 Worker 锁住标准 `bin\Release` 输出，随后测试项目无法覆盖 DLL/PDB/XML；不是 `GameSaveCenter.Contracts` 编译失败。
- 一键开发安装器现在默认不请求管理员权限。`scripts/build.ps1`、`scripts/package.ps1` 和 `scripts/dev-install-run.ps1` 支持按运行生成 `artifacts\dev-build\<Configuration>\<guid>` 隔离的 bin/obj、Worker 发布和安装暂存目录，入口修订号为 `DEV-INSTALL-007`。Playnite 发现增加运行中进程、常见目录、卸载信息、App Paths 和 PATH；未发现 Playnite 且没有运行中的 Playnite 时允许继续构建/安装并提示无法自动启动。Playnite 正常退出超时后，仅当进程属于当前会话、可执行文件路径与本次发现结果完全一致且已经没有主窗口时，才结束该无窗口残留；路径不可确认、跨会话或仍有主窗口时继续停止安装。
- 真实宿主已验证：安装报告为 0.6.70 / DLL 0.6.70.0；Playnite `playnite.log` 记录插件加载，插件日志记录 0.6.70.0，`worker-launch.log` 记录存储初始化、过期任务整理和 `Application started`。不要再用 2026-08-12 的 PID 3896 历史日志判断当前安装器行为。
- 当前自动化基线为 Core `59/59`、Worker `190/190`、Playnite `250/250`，Release 构建 0 warnings / 0 errors；source、XAML、WPF 静态门禁与 10 档 `render-qa` 通过；扩档 UI Audit 161 快照、0 HIGH/0 MEDIUM/0 失败路由。真实开发安装已成功，Playnite 与 Worker 启动日志正常。
- `ATOMIC-IO-001` 已交付：新增共享 `AtomicFileWriter`，Worker 设置持久化与媒体复制统一使用“目标同目录临时文件 + 原子 Move”，失败自动清理 `.tmp/.partial` 后再抛出；`WorkerOptions.Persist()` 与 `MediaSyncService` 私有复制逻辑已委托给共享实现。
- `SOAK-001` 已交付：`SoakStabilityHarness` 加速压测任务协调、事件扇出、单游戏锁、原子写入和 SQLite 探针；`TaskEventBroadcaster.SubscriberCount` 与 `GameOperationLock.TrackedGameCount` 提供只读稳定性计数，`scripts/soak-test.ps1` 支持用 `GSC_SOAK_ITERATIONS` 扩展到最多 5000 轮长跑。
- `FAULT-INJECTION-001` 已交付：`FaultInjectionHarness` 注入原子写、外部进程、任务协调、事件广播、操作锁、损坏 ZIP 与损坏 SQLite 共 15 类边界故障，断言无残留、稳定终态、原始文件不被失败注入删除，且锁/订阅全部回收；`scripts/fault-injection-test.ps1` 可独立运行。
- `A-HARDEN-001` 通知级别主体已收口：`NotificationLevel` 持久化默认 `Summary`，`NotificationLevelPolicy` 控制仅重要事件/退出摘要/详细任务；`SessionNotificationAccumulator` 已抽出并覆盖同 Session 单次 final、期望任务数、重复投递等测试。非任务型重要事件（健康风险/冲突/完整性严重）仍由 Dashboard Findings 承载，未单独 toast。
- `A-HARDEN-002` 已交付：未分类 `CustomExecutable` 在普通游戏下按 AutoStart 正常启动；反作弊游戏下必须持久化 `AllowUnknownToolWithAntiCheat` 授权后才允许，Trainer/CT/GameModification 继续禁止；`game_tools` 新增授权列并纳入旧库升级测试。
- `A-HARDEN-003` 已审计收口：首次使用“测试备份”按钮复用真实 `MessageTypes.BackupGame` 生产链路，无独立假服务；无可用测试游戏时显示“可稍后在存档中心手动执行备份”，并有回归测试锁定命令链路。
- `DIAGNOSTICS-001` 已升级：诊断包包含 `system/worker/dependencies/database/recent-tasks/health/settings` JSON、审计与受限日志；`DiagnosticRedactor` 集中脱敏密码、Token、API Key、Authorization、URL query、UNC 凭据、邮箱和用户路径。
- `SAFE-MODE-001` 已升级：Worker 连续 3 次启动失败后请求安全模式，Playnite 询问确认；设置页支持“下次以安全模式启动”，维护中心安全模式提示条提供“恢复正常模式”。
- `INTEGRITY-001` 已补齐：自检覆盖孤儿归档、Manifest 无效/重复路径、磁盘剩余空间和未配置依赖状态；结果使用 `Healthy/Warning/Error/Skipped`，仍只读不自动修复。
- `DB-MIGRATION-001` 已补齐：两代旧库 Fixture 覆盖策略、模板、会话、设备决策、GameTool 与备份历史，并使用 `ReadScalar` 验证真实数据值而非仅检查表存在。
- `METADATA-BACKUP-001` 已补齐恢复流程：预览校验 manifest/哈希/路径越界，确认后备份当前元数据、原子替换数据库与设置、完整性校验并在失败时回滚；维护中心提供“恢复元数据灾备”入口。
- `REPOSITORY-REBUILD-001` 已补齐：只读扫描预览统计已确认/未归属/部分缺失/损坏归档，执行重建必须用户确认，未确认不写库。
- `PATH-REMAP-001` 已补齐：只读预览按类型列出受影响路径和目标存在状态；目标缺失默认跳过，可显式授权仍应用；执行前自动创建元数据灾备。
- `TASK-RECONCILE-001` 已补齐：任务持久化 `WorkerSessionId`，启动协调只处理旧 Worker 会话遗留任务；Backup/Media/Cloud 标记可重试中断，Integrity 标记普通中断，Restore 标记人工介入且不自动重试。
- `GAME-OP-LOCK-001` 已补齐：`GameOperationKind` 与显式兼容矩阵写入代码，备份/恢复/媒体/云端使用类型化锁；Restore 不与其他操作并发，同游戏双 Backup 禁止。
- `IPC-COMPAT-001` 已补齐：握手返回 `AppVersion` 与能力列表，协议版本独立于应用版本，能力包括 RestoreReadiness/MetadataBackup/RepositoryRebuild/PathRemap/TaskReconcile/GameOperationLock/AtomicIo。
- `ATOMIC-IO-001` 已审计补齐：共享原子写入覆盖设置/媒体/元数据恢复/启动失败计数，取消写入或替换失败时旧文件保持完整且无残留。
- `SOAK-001` 已补齐：DataScale Soak 默认小规模、`GSC_SOAK_DATA_SCALE=1` 全量规模；监控 Managed Memory/句柄/线程/订阅/临时文件并断言有界增长。
- `STORAGE-001` 已交付：维护中心“保留策略”页新增只读备份存储分析卡；显示卷剩余/总容量、目录实测与索引体积、版本数、7/30/90 天增长趋势、Top 5 游戏占用排行，并给出标注“估算”的简单容量耗尽预测；新增 IPC `storage.analysis`、Worker 服务与取消支持。
- `RETENTION-SIM-001` 已交付：维护中心“保留策略”页新增全局保留策略模拟器；按每游戏策略复用 `RetentionPlanner` 计算现有/保留/候选清理/预计释放、用户锁定/健康保护/PreRestore 计数与候选明细；`retention.simulation.apply` 要求二次确认，只删除备份根目录下的 ZIP 候选并同步移除 SQLite 索引，锁定/PreRestore/健康恢复点永不进入候选。
- `LOCAL-MIRROR-001` 已交付：设置页新增“启用第二本地镜像”与镜像目录；维护中心“保留策略”页新增镜像状态与“同步镜像”入口。Worker `LocalMirrorService` 只复制和按大小校验，绝不删除镜像中多余文件；外置硬盘未连接时状态为 `Unavailable` 而不是系统错误；同步完成后写入镜像标记文件。
- `ACTIVITY-001` 已交付：首页新增“全局活动”时间线，由 `ActivityTimelineMapper` 把最近 100 条审计记录映射为备份/恢复/云端/媒体/工具/健康/冲突/完整性/仓库修复等业务事件；只展示时间、游戏、分类、结果与摘要，不暴露原始日志，UI 最多显示 12 条并保持有限视口与虚拟化。
- `PLAYNITE-QUICK-001` 已交付：`GetGameMenuItems` 为游戏右键菜单提供“立即备份 / 查看备份历史 / 验证最新恢复点 / 游戏工具 / 打开设置”五个快捷操作，全部绑定当前所选游戏 ID，并复用 Worker 生产 IPC 链路。
- `DRAGDROP-001` 已交付：修改器中心支持单文件/目录拖拽导入，`.ct` 自动按 CheatTable，`.lnk/.bat/.cmd/.ps1` 按自定义启动项，`.exe` 弹出“修改器/普通启动项”二选一，`.zip`/目录进入既有主程序选择流程；未选择游戏时拒绝导入并提示。
- `UI-STATE-001` 已交付：设置持久化上次 Workspace、任务状态/游戏/类型筛选、任务搜索、媒体筛选与媒体搜索；VM 启动时恢复，变更经 500ms 防抖保存；运行中游戏优先与上次选择恢复继续复用既有 GamePicker 持久化，不保存 Loading/Busy/Error 等瞬态。
- `ACCESSIBILITY-001` 已交付：`Ctrl+F` 按当前 Workspace 聚焦游戏/任务/媒体/FLiNG/进程映射搜索框并全选；任务、媒体、FLiNG 与游戏搜索框补充 `AutomationProperties.Name`；共享 `GscSharedFocusVisual` 与高对比度降级继续生效。
- `UI-STATES-001` 已交付：新增共享 `WorkspaceStatePresenter`，统一 Loading/Empty/Error/Degraded/Offline/Disabled 六种状态的图标、标题、说明与可选重试按钮；Overview 全局活动与 Task 空状态已接入共享控件，其余页面继续复用 `GscEmptyStateText`。
- `SETTINGS-VALIDATION-001` 已交付：设置页在标题区显示即时验证摘要，文本框、下拉框与复选框变化时复用 `VerifySettings` 校验并内联展示最多 4 条错误；验证错误不再只等 Playnite 保存时出现。
- `MAINTENANCE-REPORT-001` 已交付：新增 IPC `maintenance.report.get` 与 Worker `MaintenanceReportService`，从 SQLite 计数、完整性自检、存储分析与本地镜像状态聚合用户可读健康报告；维护中心诊断操作带新增“复制健康报告/导出健康报告”，支持 TXT/Markdown；报告不含日志、原始数据库或凭据，与开发者诊断 ZIP 明确区分。
- 最终代码缺口已闭合：`RepositoryRebuildService` 现在可从空/新 SQLite 按磁盘 ZIP 与 Manifest 重建历史，按 Ludusavi 目录名创建 `recovered-*` 占位游戏，不猜 Parent，二次重建幂等；`MetadataBackupService` 灾备包新增 `settings/plugin-settings.json`，恢复后由 Playnite 侧导入插件设置并回滚；`WorkspaceStatePresenter` 已覆盖存档历史 Loading、修改器工具 Loading/Empty、媒体 Worker Offline、维护云端 Degraded；`LocalMirrorService` 同步改为 SHA256 内容校验，同大小但内容不同会重新复制。
- 崩溃修复：`GscWorkspaceStatePresenter` 模板内重试按钮从普通 `Button` 改为 `ui:Button`，修复真实 Playnite 切换存档页时 `“Button”TargetType 与元素“Button”的类型不匹配` 的 XamlParseException；已增加源码回归断言并在真实宿主复测。
- Metadata 原子回滚：恢复前用 `VACUUM INTO` 生成一致性 DB 快照（不再直接复制可能缺 WAL 的活库）；Worker 新增 `metadata.restore.rollback`，Playnite 侧新增 `MetadataRestoreCoordinator`，Plugin 设置导入/保存/应用任一步失败时先恢复旧插件设置，再调用 Worker 从 PreRestorePath 回滚 DB 与 Worker 设置，失败才进入人工介入。
- 本轮已修复 Layer A 审计缺口：多设备只有 Manifest 内容指纹相同才可判定等价；仅文件数/总大小相同改为保守的未知分歧；Restore Readiness 使用可取消的流式解压与增量 Hash；环境检查分别验证数据、存档和媒体所在磁盘；Manifest 重复路径不会抛异常或产生强指纹。
- `DIAGNOSTICS-001` 已完成：维护中心可导出有上限、只读、脱敏的 ZIP 诊断包；包含环境/任务/审计/Worker 日志摘要，不包含数据库、存档、媒体或凭据；新增 IPC 请求和 Worker 测试覆盖敏感字段与大小边界。
- Layer A 14 项、本轮审计补缺、A-HARDEN-001/002/003、Layer B 13 项（DIAGNOSTICS/SAFE-MODE/INTEGRITY/DB-MIGRATION/METADATA-BACKUP/REPOSITORY-REBUILD/PATH-REMAP/TASK-RECONCILE/GAME-OP-LOCK/IPC-COMPAT/ATOMIC-IO/SOAK/FAULT-INJECTION）与 Layer C 11 项已交付；逐项验收见 `docs/ai/PRODUCT_HARDENING_LAYER_B_AUDIT.md` 与 `docs/ai/PRODUCT_HARDENING_LAYER_C_AUDIT.md`，最终逐项审计见 `docs/ai/PRODUCT_HARDENING_EPIC_FINAL_AUDIT.md`，人工验收清单见 `docs/ai/FINAL_MANUAL_QA_CHECKLIST.md`。由于真实场景人工验收未全部完成，整体 Epic 状态为 `PARTIALLY COMPLETED / MANUAL QA REQUIRED`，不能宣称全部任务完成。
- 通知级别已收口：`ImportantOnly` 只显示失败/取消任务与警告/失败摘要，`Summary` 保持一次退出摘要，`Verbose` 在最终摘要外逐任务显示；设置页新增通知级别选择，旧设置缺省归一为 `Summary`。
- 安全模式已交付：全局开关持久化到插件与 Worker 设置；开启后暂停自动退出/定时备份、自动媒体同步、自动工具启动、会话存档快照与保护提示、云端自动上传与自动重试，手动操作和恢复仍可用。维护中心诊断页与诊断摘要会显示当前状态。
- 完整性自检已交付：维护中心“完整性自检”通过 IPC 检查 SQLite 完整性/外键/表结构、目录可写性、配置程序存在性和索引文件引用；只报告不修复，数据库问题为 Critical，文件缺失为 Warning。
- 数据库迁移 Harness 已交付：`DatabaseMigrationHarness` 在临时目录创建旧版 Fixture 后执行当前 `SqliteStateStore.InitializeAsync`，覆盖旧库升级、全新库创建、重复初始化和失败报告；只操作临时数据库，不触碰用户数据。
- 元数据灾备已交付：维护中心“导出元数据灾备”生成 SQLite `VACUUM INTO` 一致性快照、脱敏 Worker 设置和版本清单 ZIP；不包含存档、媒体或凭据，超过 512 MiB 安全上限时失败并清理。
- 备份索引重建已交付：维护中心“重建备份索引”按 Ludusavi 磁盘列表重建 SQLite 版本索引，单游戏失败不中断，只读归档并保留失败游戏原索引。
- 批量路径迁移已交付：维护中心“批量路径迁移”按旧根/新根前缀批量改写 SQLite 与 Worker 设置中的已索引路径；只改引用不移动文件，服务端强制确认。
- 中断任务协调已交付：维护中心“协调中断任务”把 Worker 重启遗留的排队/运行中任务幂等标记为 `WORKER_RESTARTED`，启动时仍自动执行同一逻辑。
- 单游戏操作锁已交付：同一游戏的备份、云端重试、媒体同步、恢复预览/执行互斥，超时返回 `GAME_OPERATION_BUSY`；不同游戏并行不受影响。
- IPC handshake 已交付：`system.handshake` 返回协议版本、最低支持版本与 Worker 版本；客户端握手不兼容即拒绝，旧 Worker 回退 Ping 探测。
- Restore 在实际写入开始后的失败、异常或后校验失败必须尝试恢复锁定的 PreRestore；回滚本身失败才进入 `ManualInterventionRequired`。灾难演练现覆盖 A/B/Undo、部分写入、写后异常、权限、只读、目录缺失和回滚失败。
- 多设备云目录使用持久化 32 位不透明 `DeviceId`，机器名只用于显示与旧 sidecar 兼容；便携设置导入不得复制设备身份。远端恢复继续要求隔离下载、Rclone check、Ludusavi 版本确认和既有 PreRestore 恢复链。
- 每游戏策略新增 `BackupAnomalyProtectionLevel`（Off/Normal/Strict）；重要游戏模板默认 Strict。Manifest 大量删除参与异常检测，最后健康恢复点与用户 Lock 都不能成为 retention 候选。
- Rclone 每次执行都经过命令白名单 `copy/check/lsf/cat/version`，禁止 `sync/move/delete/purge`；外部进程日志不再记录完整参数。Worker 重启会把未完成任务转为 `WORKER_RESTARTED`，取消会终止子进程。
- 真实 Rclone 断网、真实两台设备、真实游戏 Restore/Undo、真实 EXE/LNK/BAT/PS1、1000+ 游戏库和完整主题/DPI 连续缩放仍为 `MANUAL QA REQUIRED`，不得由自动化结果冒充。

## UI-QA-REAL-006 设置分类 Tab 实际裁切修复（2026-08-13）

- 上一轮仅在 `TabPanel` 外增加底部留白没有解决用户截图中的直线底边。实际根因是 `GscRedesignSettingsTabItem` 让圆角 Border 直接充满 `TabItem` 模板布局槽，并开启 `ClipToBounds=True`；`TabPanel`/宿主布局取整后会把 Chrome 的底部圆角贴槽裁平。
- 当前共享模板使用不裁切的 `TabItemRoot` 包裹独立 Chrome；Chrome `VerticalAlignment=Top`、`Margin=0,0,0,2`，因此始终保留底部安全距离并移除 Chrome 的 `ClipToBounds=True`。
- 分类滚动内容使用真实 `SettingsHeaderBottomSafetyZone` 元素放在 `TabPanel` 后面形成内容 extent；顶部横向模式折叠该元素。RenderHarness 同时检查最后一项 `TabItem`、Chrome 的底部位置和 `chromeSafety >= 1`。
- 当前验证：5 种窗口渲染图通过，设置几何探针和 Playnite `210/210` 通过；真实 Playnite 主机的 DPI/主题/连续缩放依旧只能由人工验收确认。

## 2026-08-13 UI-QA-REAL-005 首页顶端对齐、当前游戏空间与设置圆角回归

- 首页宽屏 `OverviewSecondaryScrollViewer` 与其内容面显式使用 `VerticalAlignment/VerticalContentAlignment=Top`，并在响应式代码中重复设定，避免 Playnite 宿主模板刷新后“今日概览”落到工作区中部。
- 首页 Hero/当前游戏宽屏列由 `1.25* + 0.75*` 调整为 `1.1* + 0.9*`；离屏报告中的当前游戏/Hero 宽度比约 `0.82`，原约 `0.60`，没有改变 Hero/当前游戏的堆叠断点、命令或绑定。
- 设置共享分类栏模板在 `TabPanel` 外增加命名的底部安全 host，并设置顶部内容对齐、像素对齐和布局取整；滚动到末端时最后一个分类的底部仍落在 viewport 内，避免圆角被横向直线裁掉。
- RenderHarness 现在在截图前显式解除设置页入口动画的 `Opacity=0`，并检查 Overview 右栏 top delta、当前游戏宽度比和 Settings 最后一张 Tab 的底部几何，避免“空白 PNG/只测到布局没有测到可见性”。
- 验证：`python scripts/validate-source.py`、WPF 静态门禁、`git diff --check`、五种窗口尺寸 `render-qa` 全绿；Core `42/42`、Worker `117/117`、Playnite `210/210` 通过。真实 Playnite 主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## AI/Codex 启动协议

开始 GameSaveCenter 开发前，请依次阅读：

1. `docs/ai/CURRENT_STATE.md`（当前事实入口）
2. `docs/ai/PROJECT_MEMORY.md`（本文件）
3. `docs/ai/WORKLOG.md`
4. `docs/DEVELOPMENT_HANDOFF.md`
5. `git log` 最近 15～30 个 commit 与 `git status`
6. `docs/design/APPLE_WPF_IMPLEMENTATION_PROMPT.md`（UI 任务）
7. `docs/design/UI_CHANGE_GATE.md`（UI 任务）

然后才开始修改代码。不要仅凭历史对话假设当前项目状态；代码、文档和 Git 历史是唯一事实来源。

## 项目定位

- GameSaveCenter 是 Playnite 的 GenericPlugin，提供存档备份/恢复/校验、媒体同步、任务中心、维护中心、修改器与 CT 管理，以及新增的自定义游戏启动项能力。
- Playnite 是唯一主要 UI（WPF），后台 Worker 是独立 .NET 8 进程，两者通过 Named Pipe IPC 通信。
- `GameSaveCenter.Contracts`：Playnite/Worker 共享的 DTO、枚举、消息类型，netstandard2.0。
- `GameSaveCenter.Core`：Playnite 侧可复用逻辑（目前主要是启动/包装与少量辅助）。
- `GameSaveCenter.Worker`：持久化、Ludusavi、Rclone、媒体索引、任务编排、游戏 Session、GameTool 导入/启动/追踪。
- `GameSaveCenter.Playnite`：WPF Dashboard 外壳 + 六个 Workspace 页面 + 设置页。
- 数据持久化：SQLite（`SqliteStateStore`）+ 文件系统（存档、媒体归档、GameTools 目录）。
- 模块关系：Ludusavi 负责存档底层；Rclone 只允许 copy/check，不使用 sync/delete/purge；媒体为增量同步；GameTool 绑定在游戏级。

## 当前主要架构

### 程序集与入口
- Solution：`GameSaveCenter.sln`，版本 `0.6.70-development-preview`（`Directory.Build.props` 0.6.70）。
- 插件入口：`src/GameSaveCenter.Playnite/GameSaveCenterPlugin.cs`，扩展 ID `66e9f2d7-67bb-43ef-b62a-b8e60734fcec`。
- Worker 入口：`src/GameSaveCenter.Worker`，IPC dispatcher 为 `IpcRequestDispatcher`。
- 测试：Core 42、Worker 117、Playnite 203（2026-08-13 当前基线；优先使用 `scripts/build.ps1 -OutputRoot <目录>`，避免本机旧 Worker/测试宿主锁住标准输出）。
- ONBOARDING-001（2026-08-13）新增 `environment.check`：检查服务驻留 Worker，使用临时 SQLite 表和目录临时文件做可逆探针；Rclone 未配置为 `Skipped`，不把可选云端能力误计为基础失败。当前基线为 Worker 70、Playnite 198；UI 仍复用 Maintenance 诊断页的单一外层滚动与有限表格视口。
- GAME-TOOL-003/004（2026-08-13）新增 `GameToolIfAlreadyRunning` 与 `GameToolRiskCategory` 持久化列。CustomExecutable 的已有实例策略只允许按解析后的 EXE 完整路径匹配；Skip 为默认，Restart 只重启再次确认过的同路径 PID，路径读取不完整时必须保守停止。反作弊游戏仅允许已分类为 `GeneralUtility` 的自定义工具自动启动；Unknown 与 `GameModification` 自动启动必须阻止并写审计，用户需在 TrainerCenter Inspector 明确分类后保存。
- SMART-PROTECT-001/002（2026-08-13）：完整游戏停止请求等待存档识别并以持久化提示状态驱动三选一保护提示；只在识别到候选/匹配存档时提示，未识别时写审计并等待后续识别。`Deferred` 有 7 天冷却，`Enabled`/`Dismissed` 不再弹出；停止 IPC 使用 3 分钟专用超时。Overview 最近游戏列表显示已保护、未匹配、存档未保护和风险，已保护项不可选，其余项可批量启用游戏中/退出后推荐保护并写审计。不要新增主导航页或绕过既有恢复安全边界。
- NOTIFY-001 / MULTI-DEVICE-001 / RCLONE-RELIABILITY-001（2026-08-13）：退出备份与媒体任务使用同一 SessionId，Playnite 依据 Task Center 的终态任务聚合为一条退出摘要；本地备份成功但云端失败时必须同时显示本地成功和云端可重试失败。设备摘要携带 `ParentBackupId`，同一父版本分叉只标记冲突并要求人工决策，禁止自动合并/覆盖/删除；下载远端仍必须进入隔离 staging、校验、归档检查后才能走既有安全恢复链。Rclone 仅允许 copy/check/lsf/cat/version；网络或不完整传输有限重试，凭据/权限/远端不存在明确失败并停止自动重试。

### Dashboard / Workspace
- `DashboardViewModel` 是大型聚合 ViewModel（技术债，暂不拆分），持有所有 Workspace 数据与命令。
- 六个 Workspace：Overview（首页）、Saves（存档中心）、Trainers（修改器中心）、Media（媒体中心）、Tasks（任务中心）、Maintenance（维护中心）；另有 Settings 页面。
- 工作区页面位于 `Views/`：DashboardView + 各 CenterView；共享资源在 `Themes/DesignTokens.xaml`、`Themes/WpfUiProduction.xaml`、`Themes/Redesign.xaml`。
- Dashboard 视图有响应式 code-behind 协调（`DashboardView.xaml.cs`），页面级滚动面 + 主表/主列表有限视口 + 内部虚拟化滚动。

### UI-207 当前约束（2026-08-12）

- Settings 的 `SettingsScroller` 位于共享 `GscRedesignSettingsTabControl` 模板内容区；`SettingsHeaderScroller` 是分类导航区。宽屏分类栏为 232 DIP 左侧有限滚动，紧凑布局为顶部横向 `Auto`，不能把根 UserControl 再包回第二个页面滚动器。
- `GscSelectedGameIconControl` 只用于当前游戏上下文表面（Dashboard、Overview、Save、Trainer、Media），GamePicker 虚拟化列表不得加载真实 Icon。
- GamePicker 选择可被当前筛选隐藏但不能静默丢失；必须保留 `SelectedItem`、显示恢复语义并保持 `GamePickerSelectedGameId` 持久化。默认筛选只对新用户/未知值归一为“已安装”。
- 事件驱动的 `PlayniteGameStarted` 自动定位优先于普通刷新；页面每次 `Loaded/IsVisible=true` 允许一次只读 Playnite `Game.IsRunning` 同步，以补足 Worker 在既有游戏运行后启动时的基线缺口。该同步不得启动 Worker 会话、进程扫描、IPC 轮询或网络请求，也不得改动 DataGrid 滚动/虚拟化契约。
- 当前自动化结果：本阶段 Worker 相关 Release 构建 0 警告/0 错误，Worker 67/67 通过；上一阶段 Core 27/27、Playnite 197/197、render-qa 通过。真实 Playnite 宿主/DPI/主题人工验证仍待环境。

### 数据流
- Playnite → Worker：Named Pipe 请求（`GameSaveCenter.Playnite/Ipc`、`GameSaveCenter.Worker/Ipc`）。
- 任务状态：Worker `TaskCoordinator` 持久化 + `TaskEventBroadcaster` 事件流 + Dashboard 轮询兜底。
- 快照：`MessageTypes.GetDashboard` 返回 `DashboardSnapshotDto`；大库先渲染 SQLite 缓存，后台再同步。

### GameTool 模型
- `GameToolType`：Trainer / CheatTable / CustomExecutable（自定义启动项）。
- `GameToolDto` + `GameToolVersionDto`：DisplayName、Enabled、AutoStart、LaunchTiming、LaunchDelaySeconds、CloseOnGameExit、RequiresAdmin、ActiveVersionId、EntryPath、WorkingDirectory、Arguments、ResolvedTargetPath 等；`game_tool_versions` 已补 `resolved_target_path` 兼容列。
- Worker `GameToolService`：导入（Trainer/CT 复制进 GameTools 目录；自定义启动项默认保留外部路径引用）、更新、删除、启动、随游戏自动启动/延迟/关闭追踪。
- Session 追踪：`GameToolSessionTracker`（SessionId → PID + 实际 StartTime + CloseOnExit），关闭时要求 PID 与实际 StartTime 双向匹配，禁止按进程名杀。

### 任务系统
- `TaskCoordinator` 统一编排；`TaskStatusDto` 有 Progress/Message/ErrorCode/ErrorMessage/State/时间戳。
- Dashboard `TaskIndexedCollection` 按 TaskId 索引增量合并；`knownTaskStates` 去重通知。

### 媒体系统
- `MediaItemDto` 由 Worker 索引；列表与详情预览已改为 `AsyncThumbnailImage` 异步加载（`Task.Run` 强制后台、3 并发、LRU 96、Freeze 后回 UI、Unloaded 取消）；`MediaThumbnailConverter` 保留为兼容实现。
- Media 列表使用 ListBox + Recycling 虚拟化；页面滚动面与列表滚动分工明确。

### 缓存与性能机制
- `BatchObservableCollection<T>`：批量 Replace 只发一次 Reset（默认引用相等比较；PERF-005 起支持内容比较器跳过未变化）。
- GamePicker 有 180ms 搜索防抖、按 PlayniteId 缓存 `GamePickerItem`、平台指纹短路。
- Task 筛选指纹短路（`ComputeTaskFilterFingerprint`）、平台指纹短路（`ComputePlatformFingerprint`）。
- Dashboard 大库 cache-first + 非阻塞后台目录描述同步；昂贵的 Ludusavi 匹配仍由 Worker 节流队列处理；`[PERF]` 日志设施见 `docs/ai/PERFORMANCE_BASELINE.md`。

## UI 设计原则

- 目标是 Apple-inspired 的原生 WPF 桌面工具：清晰层级、克制毛玻璃、圆角、统一设计令牌、自然微动效、深浅色、跟随 Playnite、高对比度、DPI 适配、响应式布局、不使用突兀的原生控件视觉。
- 所有 UI 修改必须先读 `docs/design/APPLE_WPF_IMPLEMENTATION_PROMPT.md` 与 `docs/design/UI_CHANGE_GATE.md`，并遵循 `.codex/skills/wpf-apple-desktop-ui/SKILL.md`。
- 常用窗口下限 1040×700 DIP；1080p/2K/4K 必须按 DPI 换算后的逻辑 DIP 检查全屏、窗口化、最大化；不把 4K 通过当作 1080p 通过。
- 页面级滚动只承载有限测量内容；DataGrid/ListBox 保留 236 DIP 最小视口、内部滚动和虚拟化；堆叠 Inspector 下限 160 DIP。
- 动态下拉框必须显示逻辑默认值（如“全部”）；TaskCenter 游戏/类型筛选通过 `TaskFilterOptionsSync` 增量同步，`全部` 稳定保留在 index 0，不再 Clear/Replace 集合。
- GamePicker 新用户默认筛选为“已安装”，已有明确配置值必须保留；Dashboard 打开时运行中游戏优先，否则恢复上次选择，普通刷新不得抢回用户手动选择。

## 已完成的大型重构 / 优化

- UI-001～UI-205、SKILL-001、QA-001～005：页面 Workspace 化、响应式断点、滚动分工、Inspector 下限、筛选默认值、离屏渲染 QA。
- UI-207（2026-08-12）：设置页 Header 不裁剪与分类栏滚动（920 DIP 断点）、运行中游戏自动定位、上次选择持久化复用、GamePicker 新用户默认“已安装”、当前游戏真实 Playnite Icon（事件驱动，无轮询/无网络，LRU 48）。
- `scripts/render-qa.ps1` + `tests/GameSaveCenter.RenderHarness`：7 页面 × 5 常用窗口离屏渲染回归，含自动失败门禁。
- PERF-001：`BatchObservableCollection` 批量 Reset。
- PERF-002/003：Task 筛选与 GamePicker 平台指纹短路。
- PERF-004（旧编号）：GamePickerItem 缓存复用（新任务编号体系中 PERF-004 是性能基线设施，不要混淆）。
- PERF-004/005/006（新编号）：`[PERF]` 基线日志、Snapshot 无变化 0 Reset、Task/Media 搜索防抖。
- PERF-007：媒体缩略图异步化（`AsyncThumbnailLoader` Task.Run 后台解码 + 3 并发 + LRU + Freeze + `[PERF]` 埋点，`AsyncThumbnailImage` 占位加载并 Unloaded 取消）。
- PERF-009/010：任务事件合并 TaskId 索引 O(1) 更新；命令状态刷新 Dispatcher 合帧。
- GAME-TOOL-001/002：自定义启动项正式支持 EXE/LNK/BAT/CMD/PS1，外部路径引用不复制文件；Session 级 PID 追踪与 CloseOnGameExit 安全关闭。
- UI-204/205：TaskCenter 与 GamePicker 下拉框默认值恢复（含真实 Playnite 异步物化重试）。
- UI-206（含回滚）：DataGrid 滚动几何修复。初版 `Pixel ScrollUnit` 经真实 Playnite A/B 验证会严重恶化空白，已撤回；最终采用 `Item` + `GscStableDataGridRow` 稳定行样式 + geometry probe（60 行 × 非整行高度，gap ≤4 DIP、末行完整、无跳变、Recycling 保持）；诊断摘要取消外层裁剪并由页面滚动负责可达性。

## 当前技术债

- `DashboardViewModel` 仍很大，包含命令、筛选、导入、诊断、设备状态等职责；只有性能实现被严重阻碍或 GAME-TOOL 无法接入时才拆（独立 `ARCH-xxx` 任务）。
- `DashboardView.xaml.cs` 仍承担部分响应式协调。
- 媒体列表/详情缩略图已异步化；真实大量截图滚动下的帧率仍需真机验证。
- 真实 Playnite 宿主、主题切换、DPI 真机、连续缩放流畅性尚未完整人工验收（UI-QA-REAL-001 仅完成冒烟）。

## 当前开发优先级

- P0：性能基础设施与真实热点优化（PERF-004～007、009/010 已完成）。
- P0：自定义游戏启动项（已完成，GAME-TOOL-001/002）。
- P1：媒体性能（PERF-007 异步缩略图，已完成）。
- P1：真实 Playnite / DPI / 大型游戏库 QA（UI-QA-REAL-001 冒烟已完成，完整人工验收待用户）。
- P2：架构进一步拆分（不主动做）。
- PERF-008：已评估收口，维持现状。详情已按激活 Workspace 分支加载，全量快照仅用于全局摘要且后台有 1 分钟 TTL；2000 规模合成 profiling 无 O(n^2)，待真实大库渲染 profiling 证明瓶颈后再评估。

## 2026-08-12 可靠性阶段补充

- `RELIABILITY-RESTORE-001` 已实现：备份历史版本支持非破坏性的恢复可用性检查，结果持久化在 `backup_versions.restore_readiness_json`，检查过程只在应用数据目录隔离提取，不接触真实存档目录。
- `d45f65c` 已补齐恢复校验安全闭环：Manifest 非法、重复/越界路径、Manifest 缺失文件现在不能得到 `Ready`；逐文件路径集合、大小、可用 Hash 和提取结果均纳入判定，验证目录创建失败返回 `Failed`，取消仍由调用方观察。
- `d45f65c` 为恢复编排增加窄接口测试边界，并用临时 SQLite + 内存假 Ludusavi 完成 A→PreRestore→B→失败回滚、成功恢复→Undo、运行中拒绝恢复等灾难演练；未启动 Playnite、未调用真实 Ludusavi、未接触真实存档。
- Ludusavi 备份版本的 `backupPath + backup ID` 已持久化为 `backup_versions.archive_path`；Simple 归档、缺失/损坏 ZIP、路径穿越、超大展开量、不一致统计与不支持压缩方式必须返回明确状态。
- 恢复可用性入口位于现有 Save Center 历史 Inspector，不能新建页面或改变 `SaveHistoryGrid` 的滚动/虚拟化骨架；新增内容必须留在 `SaveHistoryActionsScrollViewer` 内，并继续通过 `render-qa` 验证 1040×700 等窗口。
- 初始实现的历史基线为 Core 13、Worker 58、Playnite 197；当前阶段增量基线为 Worker 67/67，生产 Worker Release 构建 0 警告/0 错误。真实 Playnite 宿主、主题/DPI 人工验收仍待用户环境确认。
- 该恢复可用性阶段的下一项已在后续 `HEALTH-001` 完成；历史记录保留原阶段编号，当前开发顺序见下方 HEALTH-001 补充。

## 2026-08-12 HEALTH-001 阶段补充

- 每游戏健康状态已统一为四态：`Healthy`（健康）、`Attention`（注意）、`Risk`（风险）、`Unknown`（未知）。旧 `Ready / Warning / LudusaviUnavailable` 仅作为 UI/历史缓存兼容输入保留；新 Dashboard 快照输出四态。
- `GameHealthAssessmentService` 是 Core 纯计算服务，证据包括最近游玩、备份版本/时间、最近 30 天失败任务数、最近任务状态、最新 `RestoreReadinessStatus`、未解决 finding 严重度、按游戏策略启用的云端状态；不做磁盘、ZIP、网络或数据库访问。
- Worker 的 `GetDashboardGameRecordsAsync` 一次聚合最新备份可用性、任务失败、finding 和媒体/策略数据；`DashboardService` 只在内存中计算四态和理由，并把 `WarningGames = AttentionGames + RiskGames`，`UnknownGames` 不误计入需处理数。
- UI 改动只复用首页统计卡、Dashboard 游戏列表/选中头部和 Save Center 校验区；四态在有限宽度下不新增列或固定宽度，理由使用已有 Tooltip，旧 `Ready` 夹具继续显示绿色。Snapshot comparer 已比较健康摘要与理由列表。
- 当前测试基线为 Core 19、Worker 59、Playnite 197；源码门禁、XAML 门禁、WPF 静态门禁、隔离 Release 构建和 render-qa 已通过。真实 Playnite 宿主、主题/DPI 人工验收仍待用户环境确认。
- 当前已完成 Restore Readiness、Health、Protection 三项；下一项按附件顺序为 `POLICY-001`，不要重做上述功能，不新增主页面，继续采用小阶段、独立 commit、文档和 push。

## 2026-08-12 PROTECTION-001 阶段补充

- `RecentProtectionAssessmentService` 已在 Core 实现为无副作用纯计算：以 `GameStatusDto.LastPlayedUtc` 过滤最近 7/30/90 天，按未识别存档、从未备份、恢复点不可用、自动保护关闭、云同步异常、游玩后备份过旧和备份健康异常分类；每个游戏只显示一条最高优先级原因。
- `GameStatusDto` 现在带有 `LatestRestoreReadinessStatus`，由 Worker Dashboard 从已有聚合记录投影；Playnite 不增加 IPC、扫描或数据库查询，Overview 只从现有快照计算摘要。
- UI 复用现有 Overview 风险滚动面与 Settings 自动化分类。保护摘要最多展示 6 条；选择条目只改变当前游戏选择并提示用户确认，绝不因筛选/选择自动备份或恢复；没有新增页面，也没有修改 DataGrid、虚拟化或滚动骨架。
- 最近保护窗口设置默认 30 天，接受 7/30/90，便携设置导入会校验非法值，旧 JSON 缺少字段时保持默认值。
- 本阶段验证基线为 Core 27、Worker 59、Playnite 197；Worker/Playnite Release 构建、源码/XAML/WPF 门禁和 render-qa 均通过。真实 Playnite 主题/DPI/键盘/连续缩放验收仍待用户环境。
- 下一项按附件顺序为 `POLICY-001`；不要重做 `HEALTH-001` 或本阶段保护摘要。

## 2026-08-12 POLICY-001 阶段补充

- 策略模板复用 `BackupPolicyDto`，内置模板 ID 固定为 `default`、`important`、`high-frequency`、`exit-only`、`manual-only`；用户模板 ID 必须以 `custom-` 开头。模板应用是一次性复制，不建立继承关系。
- `BackupPolicyTemplateCatalog.ClonePolicy` 是模板的安全边界：周期间隔限制在 1–1440 分钟，保留值不小于 0，所有模板都强制关闭自动恢复。内置模板由 Worker 初始化幂等播种，禁止通过 IPC 修改/删除。
- Save Center 的模板区位于既有策略页滚动内容内，未新增页面、未改变 DataGrid/虚拟化骨架；创建副本时先保存当前选择再清空选择，避免名称丢失。
- Playnite 包必须同时包含 `GameSaveCenter.Core.dll` 与 Worker 的 self-contained Windows runtime；`scripts/package.ps1` 会验证 Core、hostfxr/hostpolicy/coreclr/System.Private.CoreLib 和 `includedFrameworks`，Worker 项目保持 `RuntimeIdentifiers=win-x64`，发布使用单节点/无 node reuse 参数。
- 当前自动验证：Core 29/29、Worker 69/69、Playnite 197/197；Worker/Playnite Release 隔离构建 0 警告/0 错误；source/XAML/WPF 门禁与 render-qa 通过；最终 `.pext` 打包成功。真实 Playnite 日志曾确认插件加载，但旧 Worker PID 3896 仍锁住用户安装目录，完整 Worker/IPC/UI 仍标记为 `MANUAL QA REQUIRED`，不能以隔离首启未进入扩展阶段冒充真实宿主通过。
- 以后每个代码阶段的验收顺序固定为：`dotnet test/build` → 源码/XAML/WPF/render-qa → `scripts/package.ps1` → 安装包内容断言 → 启动 Playnite 并检查 `ExtensionFactory`/扩展日志；若宿主被单实例或权限环境阻断，必须记录为人工验收，不得宣称加载成功。
- 本阶段完成后的下一项为 `ONBOARDING-001`；不要重做 Restore Readiness、Health、Protection 或本阶段策略模板。

## 一键安装器进程停止与权限约束

- `DEV-INSTALL-007` 允许可信 Playnite 候选为空，避免 PowerShell 将空数组绑定到停止函数时直接失败。没有运行中的 Playnite 时，安装器仍使用 `%APPDATA%\Playnite\Extensions`（或显式 `-PlayniteExtensionsPath`）完成安装；以后需要自动启动时应通过 `-PlayniteExecutable` 指定便携版/自定义目录中的 `Playnite.DesktopApp.exe`。
- `scripts/dev-install-run.ps1` 的 `Stop-PlayniteAndOwnedWorkerReliably` 必须先允许 Playnite 正常退出并等待插件回收 Worker，再处理残留；不能把 `Get-Process` 与停止之间的退出竞态误报为失败。
- 安装器不应默认请求管理员权限，也不应按进程名广泛终止 Worker。`DEV-INSTALL-004` 先调用 Playnite 的正常窗口关闭，让插件既有 `OnApplicationStopped`/`WorkerLauncher.StopOwnedWorker()` 回收自己创建的 Worker；只有 Playnite 已退出后仍存在、且路径明确属于当前扩展目录的残留 Worker 才可处理。
- 路径不可读取或残留 Worker 属于其他扩展时必须停止安装并要求用户手动处理，不能为了自动化验证提权或误杀其他用户进程。根目录入口同步检查 `DEV-INSTALL-004`，避免旧副本继续运行已经废弃的提权逻辑。
- `DEV-INSTALL-006` 补齐 Playnite 自身的无窗口残留：先等待 20 秒正常退出；仅对当前会话、精确可信路径且 `MainWindowHandle=0` 的实例执行强制结束，并把 `Refresh` 与停止之间的自然退出视为成功。不得退化为按进程名批量终止。

## 2026-08-12 Worker 生命周期清理补充

- Playnite 插件退出必须调用 `WorkerLauncher.StopOwnedWorker()`；Launcher 只允许停止当前实例记录的 `runningWorker`，不能按名称终止任意 `GameSaveCenter.Worker`。`shutdownRequested` 防止退出竞态重新启动子进程。
- 本阶段 `3f05e16 fix: stop owned worker on Playnite shutdown` 已通过 Playnite Release 全量 198/198、源码校验、Release 编译和 Release self-contained 包验证。
- 隔离 Playnite 的 `--userdatadir` 首次启动会停在 `FirstTimeStartupWindowFactory`，不能据此宣称扩展加载；真实宿主验证仍必须看 `ExtensionFactory:Loaded plugin: GameSaveCenter` 与扩展日志，并记录 `MANUAL QA REQUIRED` 直到用户环境实际通过。

已完成：见 WORKLOG.md 与 Git log；不要重复实现已完成的 UI/性能工作。

## 已知坑

- WPF `ICollectionView.Refresh()` 昂贵；不要在每个按键或每次快照都调用。
- `ObservableCollection` Reset 仍会触发 CollectionView 重建；数据没变时应跳过（PERF-005）。
- 动态 ComboBox Items 重建会清空 SelectedItem；要显式恢复逻辑默认值。
- 大库启动不要同步全量匹配/扫描；先渲染 SQLite 缓存。
- Worker 是独立进程：Playnite 启动早期 IPC 可能超时，要用失败快速降级 + 后台重试。
- 修改器/CT/自定义工具启动一律走 Worker；禁止在 Playnite UI 进程直接 Process.Start 外部程序。
- CloseOnGameExit 只能关闭本 Session 由 GameSaveCenter 启动且能确认 PID/StartTime 的进程；脚本（BAT/CMD/PS1）与系统默认程序打开的文件不可靠，UI 对这类入口禁用开关。
- 自定义启动项支持 EXE/LNK/BAT/CMD/PS1/普通文件：EXE 与导入/重定位时已解析并持久化的 LNK→EXE 目标可跟踪；未解析的 LNK、脚本和系统默认程序启动时 Trackable=false。
- 磁盘 IO、图片解码不要放 UI 线程；图片解码要限制并发并 freeze。
- 表格/列表虚拟化很容易被外层 ScrollViewer 或 DataGrid 嵌套破坏，改 XAML 后必须跑 render-qa。
- DataGrid 不要写死运行时 `Height`，用 `MinHeight/MaxHeight` 保持有限 viewport；`Pixel ScrollUnit` 已在真实 Playnite 验证会回归（轻微滚动即大空白），当前必须保持 `Item` + 稳定行样式，禁止重新改回 Pixel。
- `git push` 前确认没有 bin/obj、用户本地配置、密钥、测试临时文件和大压缩包（如 `GameSaveCenter.7z` 不要提交）。

### ONBOARDING-001 不可丢失约束

- 首次使用状态由 Playnite `GameSaveCenterSettings.OnboardingCompleted` 持久化；未完成时 Dashboard 首次打开定位 Maintenance，用户可“跳过首次检查”，之后仍可手动重新运行环境检查。
- 环境检查只允许读取、创建/删除自身临时探针和只读远端列举；禁止自动备份、上传、同步、删除或覆盖真实存档。测试备份必须由用户明确点击，并且要求当前游戏已匹配且 Ludusavi 可用。
- 真实宿主验收必须看到 `ExtensionFactory:Loaded plugin: GameSaveCenter` 与扩展日志；隔离 Playnite 只能证明进程启动，不可替代真实安装验证。当前安装器不再默认请求 UAC；用户桌面应确认普通双击入口即可完成“关闭 Playnite → 回收 Worker → 构建安装 → 启动 Playnite”链路。

## 文档导航

- `docs/DEVELOPMENT_HANDOFF.md`：跨电脑/跨模型交接入口，包含每轮 UI 基线。
- `docs/PROJECT_MEMORY.md`：长期不可丢失约束与 UI 决策历史（大文件，按章节检索）。
- `docs/DEVELOPMENT_PROGRESS.md`：按 UI 编号的实施历史与下一步线索。
- `docs/design/APPLE_WPF_IMPLEMENTATION_PROMPT.md`、`docs/design/UI_CHANGE_GATE.md`：UI 方向与门禁。
- `.codex/skills/wpf-apple-desktop-ui/SKILL.md`：WPF/Playnite UI 专项技能。
- `docs/ai/WORKLOG.md`：每阶段开发流水记录。
- `docs/ai/PERFORMANCE_BASELINE.md`：性能基线与测量方法。

## 2026-08-13 UI-QA-REAL-002 当前事实

- 首页“今日工作台”状态区已改为标题下方全宽第二行，避免 2K 最大化时原窄列 `WrapPanel` 将状态胶囊挤成不可见/竖向圆点；`OverviewView.xaml.cs` 会根据实际或估算宽度调整英雄卡内边距。
- 维护中心首次使用环境检查复用现有检查项，采用响应式 `UniformGrid`：可用宽度 ≥ 900 DIP 为 3 列，620–899 DIP 为 2 列，更窄为 1 列；检查卡统一拉伸并设置最小高度，避免胶囊乱序和不规则空洞。
- 设置页共享文本框内容宿主已垂直居中，默认高度 42 DIP；设置分类卡片的共享模板增加底部安全间距并使用一致圆角，避免窗口底部裁切。
- 本阶段没有新增主导航页面，没有改动业务命令、绑定、DataGrid 虚拟化或 Worker/恢复体系；渲染夹具仅补充首次使用卡片的演示数据。
- 当前自动化基线为 Core 42/42、Worker 117/117、Playnite 206/206；Release 构建无警告/错误，五种窗口尺寸的 `render-qa` 通过。
- 2026-08-13 真实开发安装完成，Playnite 扩展日志确认 `GameSaveCenter 0.6.70.0` 加载，Worker 正常运行。真实宿主日志/进程证明已记录；用户实际 2K 最大化、主题/DPI、Settings 连续缩放仍标记 `MANUAL QA REQUIRED`。
- 本阶段只完成用户反馈的三类 UI 修复，不引入第 15 个功能；下一步等待人工视觉反馈。

## 2026-08-13 UI-QA-REAL-003 当前事实

- 首页“最近 30 天玩过的游戏”风险卡片的两个操作按钮已经移到摘要下方的独立响应式行，避免 2K 或右侧窄栏中按钮与标题、统计文本挤压。
- 修改器中心启动延迟编辑器现在明确显示“启动延迟”和“秒”，继续绑定 `SelectedGameTool.LaunchDelaySeconds`，输入框高度收敛为 34 DIP。
- 媒体中心 `MediaGrid` 显式使用顶部对齐的虚拟化面板和内容对齐，已修复表头/筛选区下方到首条媒体记录之间的大段空白；未修改列表的虚拟化滚动模型。
- 当前自动化基线为 Core 42/42、Worker 117/117、Playnite 209/209；Release 构建无警告/错误，五种窗口尺寸的 `render-qa` 通过。
- 2026-08-13 一键开发安装完成，Playnite 扩展日志确认 `GameSaveCenter 0.6.70.0` 加载，Worker 进程从当前扩展目录运行。
- `AUTO VERIFIED` 仅覆盖自动化、渲染、安装和真实宿主日志；用户实际 2K 最大化、主题/DPI、连续缩放及真实媒体数据滚动仍为 `MANUAL QA REQUIRED`。
- 本阶段只补充用户反馈的三个布局问题，不新增主导航页面，不改变业务绑定或 Worker/恢复体系；下一步等待人工反馈。

## 2026-08-13 UI-QA-REAL-004 当前事实

- 设置页左侧分类卡的“底部/边缘圆角被削掉”根因已确认是 `SettingsHeaderScroller` 的滚动条占用内容宽度，固定 232 DIP 的 `TabItem` 被 viewport 裁切，不是 CornerRadius 数值失效。
- `SettingsHeaderScroller` 已扩展到 248 DIP，分类 `TabItem` 仍保持 232 DIP 内容宽度；滚动条出现时为卡片边缘预留安全区，分类卡继续使用 14 DIP 圆角并开启自身边界裁剪。
- 设置页在可用高度低于 760 DIP 时使用更紧凑的 60 DIP 分类卡和 8 DIP 间距；左右设置滚动面保留底部安全留白，避免最后一项在宿主 viewport 边缘被直接截断。
- 当前自动化基线为 Core 42/42、Worker 117/117、Playnite 210/210；Release 构建无警告/错误，五种窗口尺寸的 `render-qa` 通过。
- 2026-08-13 一键开发安装完成，真实 Playnite 扩展日志确认 `GameSaveCenter 0.6.70.0` 加载，Worker 进程从当前扩展目录运行。
- `AUTO VERIFIED` 仅覆盖自动化、渲染、安装和真实宿主日志；用户实际 2K/DPI 设置页最终视觉仍为 `MANUAL QA REQUIRED`。
- 本阶段只修复设置页现有分类卡和滚动 viewport 的裁切，不新增页面、不改变设置字段、绑定、保存语义或 Worker/恢复体系；下一步等待人工反馈。

## 2026-08-16 UI-206 Overview 页面级迁移事实

- UiLab 的关键骨架不是“右侧摘要从页面顶部开始”，而是顶部 Hero/当前游戏与六项指标占满整行，最近活动开始后才分成左主区与右侧风险/关注栏；生产 Overview 已按此层级重排。
- 生产右栏使用 330 DIP 固定宽度，宽屏与最近活动卡同一行起始（离屏探针偏移 0 DIP）；窄窗口由现有页面级 ScrollViewer 承载并把右栏下移。不要恢复成 1.2*/0.8* 的整页比例栏，也不要把 UiLab 演示滚动条迁入生产。
- 生产数据和行为保持真实：`Snapshot`、`RecentProtection`、`AttentionFindings`、OpenProtection/OpenAttention 命令、选择状态、虚拟化列表和页面滚动都未替换为 demo 假数据；UiLab 右上角颜色按钮没有迁移。
- 共享表头现在使用低对比度表头填充、8 DIP 圆角和 1/2 DIP 安全边距；普通活动行使用较弱 Divider，避免 DataGrid/活动表头看起来像尖锐矩形。
- 本阶段自动验证：源码/XAML 门禁通过，Playnite 303/303，生产 Release 0 warning/0 error，Overview 多尺寸与 Light/Dark 离屏渲染通过。全量 render harness 仍有 Save/Media 窄尺寸主表 `<236 DIP` 的历史门禁项，不能写成全量 render-qa 通过。
- 本阶段尚未完成真实 Playnite 2K/DPI/Follow/高对比度人工验收；后续优先在真实宿主检查页面级滚动、侧栏下移、键盘焦点和长中文文案，再继续迁移其他页面。

## 2026-08-17 AcrylicFork 全量页面重构事实

- 本轮确认没有任何提示词保护；先前外观不变的根因是生产页面与 AcrylicFork Demo 使用两套不同的页面树，同时旧 Dashboard 外壳重复渲染页面上下文和局部表格样式。
- 生产 Shell 现在只负责侧栏、Header/GameSwitcher、全局操作、PageHost 和 Footer；首页、媒体、任务、存档、修改器、维护页面继续持有真实 ViewModel/Command/Binding，Demo 顶部颜色按钮只作为主题令牌参考，生产滚动条保持项目实现。
- 首页与维护中心已经按 Demo 信息架构迁移；维护中心默认诊断概览显示六项健康卡、环境检查、诊断操作和完整摘要，发现问题表格通过独立问题列表 Tab 保留并验证。
- DataGrid 共享样式采用透明表头、稳定底部分隔线和明确文本对比度；不使用负 Margin、Canvas、透明占位或隐藏溢出来修复布局。首页零分母进度条折叠，关注入口提供可访问说明。
- 2026-08-17 自动事实：源码校验通过；WPF UI 静态校验 0 error；Playnite 测试 303/303；RenderHarness 全量 render-qa OK。真实 Playnite 宿主、DPI、Follow/高对比度、键盘和大库滚动仍需人工复核。
- `scripts/package.ps1` 已修复空 `dotnet` 参数问题；当前无 `BuildOutputRoot` 的标准打包流程可正常生成并校验安装包，且会保留 `GameSaveCenter.Contracts.dll`。
- 一键安装的隔离构建还必须把 `TEMP/TMP` 指向隔离输出盘；否则完整性测试会读取系统临时目录所在磁盘的真实剩余空间，在低于 512 MiB 时把健康夹具判为 `Warning`。
- 一键安装的隔离构建根目录使用短路径 `artifacts/gsc-b/<guid>`；过深的 `artifacts/dev-build/Release/<guid>` 会让 .NET Framework Playnite 测试适配器加载失败。当前完整 `dev-install-run.ps1 -NoStart` 已通过。

## 2026-08-18 UI-214 首页状态徽标事实

- 首页最近任务与全局活动徽标的文本必须显式设置 `HorizontalAlignment=Center` 和 `TextAlignment=Center`，不能依赖旧 Chip/Border 模板的默认测量。
- 风险与提醒徽标使用独立 `Border + TextBlock`，最小宽度 52 DIP、内边距 8/2，并通过 DataTemplate triggers 同时切换背景、边框和文字颜色；这样“风险/需关注/未知/已就绪”不会被裁切或错误显示为同一种颜色。
- 任务图标应引用 `GscAccentBrush` 等生产资源键，不能直接引用 Acrylic Demo 的裸 `AccentBrush`。
- 2026-08-18 Release 构建 0 warning / 0 error；Overview 离屏浅深色多尺寸探针通过。全量 RenderHarness 仍有既有 Media resize recovery 失败，不能标记全量通过。
- 本轮未重新进行 Playnite 宿主截图；屏幕控制此前已由用户物理 Escape 停止，离屏渲染结果不得替代真实宿主人工验收。
- 本轮重复的 Playnite 测试命令长时间无输出并被停止，不能据此宣称新增测试通过；保留上一阶段已记录的回归基线。

## 2026-08-18 UI-225 首页状态徽标实际宿主事实

- 首页纯文本状态徽标使用页面局部 `DataTemplate` 居中，不修改共享 Chip 的复杂内容承载方式；否则 Worker/Ludusavi 的 StackPanel 内容会被错误显示为控件类型字符串。
- 风险与提醒徽标位于独立 Grid `Auto` 列，游戏名称允许省略并提供 Tooltip，风险文本保留 72 DIP 最小宽度；这解决了名称挤压徽标和“风险”中文裁切。
- 最新 v28 生产包已经重新安装并在真实 Playnite 中启动。宿主截图明确显示：最近任务的“成功”徽标文本居中，风险徽标文本完整，侧栏设置项可见。
- 本阶段的 RenderHarness 与宿主验证只覆盖首页徽标修复；全量 RenderHarness 的 Media resize recovery 失败和 Playnite 迁移前结构测试失败仍是公开的后续工作项。
-
## 2026-08-18 UI-224 首页徽标与真实宿主截图核验事实

- 首页活动/任务状态徽标不能依赖 `LabSubCard` 的内边距和默认测量；固定徽标必须使用零内边距、固定宽度、子 `TextBlock` 显式 `HorizontalAlignment=Center` 与 `TextAlignment=Center`。当前生产首页的任务/活动徽标宽度为 58 DIP，风险徽标为 70 DIP。
- 风险徽标必须保留 Tooltip，中文状态不能用省略号替代；背景、边框和文字颜色继续由真实健康状态触发器驱动。
- 真实 Playnite 验收的证据要求提高：必须同时有 Playnite 日志中的 `ExtensionFactory:Loaded plugin: GameSaveCenter`、可识别的 Playnite 页面截图和必要时的交互结果。若 Computer Use 返回 `EmptyWindowAutomationPeer`、`MainWindowHandle=0` 或截图是其他桌面窗口，只能记录为“宿主已加载，视觉验证阻塞”，不得写成视觉通过。
- 2026-08-18 本轮 Release 编译/打包/安装通过，日志确认生产 DLL `0.6.70.0` 加载；但 Computer Use 截图不属于 Playnite 页面，真实宿主视觉验收仍为 `MANUAL QA REQUIRED`。Worker 两项环境状态测试和 Media resize recovery 两项 RenderHarness 失败继续保持公开记录。

## 2026-08-18 UI-226 首页短主题别名事实

- 生产 Acrylic 共享控件与 AcrylicFork Demo 共用一组短资源键：`AccentBrush`、`AccentHoverBrush`、`AccentPressedBrush`、`AccentStrokeBrush`、`AccentTintBrush`、`AccentTintStrongBrush`、`AccentWashBrush` 和 `TextOnAccentBrush`。
- 生产主题适配必须同时写入这些短键和 `Gsc*` 键；只写 `Gsc*` 会让 Playnite 宿主的未解析短键回退为黑色/透明，表现为任务图标、进度条、活动气泡和主按钮与 Demo 色彩不一致。
- 短键只能写入当前页面的局部 `ResourceDictionary`，不能修改 Playnite 全局资源；这样既能复现 Demo 的强调色层级，又不会污染宿主主题。
- 2026-08-18 已确认生产页面的实际资源注入缺口：`DashboardView` 之前只更新旧隐藏页面树，现已同时更新 `AcrylicProductionShellView` 及其 `PageHost` 页面实例；离屏深色 RenderHarness PNG 已确认首页最近任务图标/进度条、`全部` 链接、风险主按钮、全局活动分类和信息气泡恢复紫色或对应语义色。
- Playnite 日志确认新 DLL 已加载，但 Computer Use 返回 `EmptyWindowAutomationPeer`、`MainWindowHandle=0` 且截图不是 Playnite 页面，因此真实宿主视觉验收仍为 `MANUAL QA REQUIRED`，不得把离屏 PNG 写成真实宿主截图通过。全量 RenderHarness 仍有 Media resize recovery 两项失败。

## 2026-08-18 UI-227 媒体中心模式栏事实

- 媒体中心顶部模式栏已从灰色透明条切换为生产深色 `MediaModeStrip`，外层使用 `GscGlassStrongBrush` 与 `GscControlStrokeBrush`；选中 RadioButton 使用 `GscAccentTintStrongBrush`、`GscAccentBrush` 和 `GscSelectionTextBrush`。
- RadioButton 的三种模式、真实数据绑定、命令和项目自身滚动条没有改变；本次只修正页面级 Tab 承载样式，并确保悬停不会覆盖选中态。
- Release 构建与静态检查通过；RenderHarness 编译和主题/尺寸探针完成，但 Media resize recovery 仍有两项失败：回弹后 `MediaGrid` 尺寸不一致、`MediaInspectorScrollViewer` 从可见变为折叠。真实 Playnite 截图仍待可识别宿主窗口后复核。

## 2026-08-18 UI-228 页面基线回退修复事实

- `be5707d` 是一次页面基线回退：它把生产页面覆盖成“今日工作台 / 最近活动”架构，导致两台同步仓库的电脑同时显示数个版本前的页面。
- 当前恢复目标是 `be5707d^` 的 AcrylicFork 生产基线，首页必须包含“最近任务 / 全局活动 / 风险与提醒”；顶部 Demo 彩色按钮和 Demo 滚动条仍不迁移。
- 针对已撤销架构的 61 条 Playnite 源码契约断言必须保持显式跳过，不能用无业务意义的兼容控件让它们假通过；当前基线由 `RestoredAcrylicForkBaselineTests` 覆盖首页、存档、媒体和任务入口。
- 本轮验证：Release 0 warning / 0 error，Core 59/59，Worker 191/191，Playnite 246 通过、61 跳过、0 失败。真实 Playnite 宿主视觉仍需可识别窗口截图确认。

## 2026-08-18 UI-229 媒体模式栏交付验证边界

- 一键 Release 构建、Worker 发布和 Playnite 安装已重新通过，安装目录为 `%APPDATA%\Playnite\Extensions\GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec`，`extension.yaml` 为 `0.6.70`，生产 DLL 为 `0.6.70.0`。
- RenderHarness 不是 Playnite 真机截图；本轮工具上下文没有可用的 Playnite 鼠标/键盘控制，因此没有把离屏结果冒充真实宿主视觉验收。
- 当前提交只交付媒体模式栏颜色修正和对应基线断言，不代表首页、存档、修改器、任务、维护等页面已经完成 1:1 视觉迁移；Media resize recovery 两项失败仍是后续阻塞项。

## 2026-08-18 UI-230 首页风险列表滚动边界事实

- 首页风险卡片的两个可能增长列表必须使用独立的生产 `GscPageScrollViewer`：需关注列表与展开后的最近游戏保护明细均限制为 `MaxHeight=190`，垂直滚动 `Auto`，水平滚动 `Disabled`。
- 页面根滚动与风险列表内部滚动职责分离：普通首页内容由页面滚动承载，风险项超过视口后只在自身区域滚动，不能通过追加列表项把主页面高度无限撑大。
- 当前 Dashboard ViewModel 的需关注数据按真实严重度筛选后完整绑定，首页不再用前 4 条静默截断；保护明细继续使用真实 `RecentProtection.Items`。190 DIP 是 UI 层防护边界，不是业务截断替代品。
- RenderHarness 默认 fixture 没有使需关注列表溢出，因此其 `scrollable=false` 只说明当前 fixture 未超过 190 DIP；静态契约测试已强制检查滚动边界。不得据此宣称已完成真实 Playnite 滚动条交互验收。
- 首页清理了已撤销的“今日工作台”旧工具栏及其代码后置响应式引用；媒体中心模式栏继续使用生产控件底色和紫色选中色，不迁移 Demo 顶部彩色按钮或 Demo 滚动条。

## 2026-08-19 UI-231 首页风险项数量边界事实

- 首页“风险与提醒”及其关联明细必须采用有限视口：最近游戏保护明细、需关注事项列表分别使用生产 `GscPageScrollViewer`，`MaxHeight=190`、垂直滚动 `Auto`、水平滚动 `Disabled`。
- 数量很多时，风险明细在卡片内部滚动，不能把 Dashboard 根内容高度无限撑长；风险卡片本身不再包一层整卡滚动，避免双滚动条。
- 该视口不替代业务数据：真实绑定仍保留，当前展示条数限制只由现有 ViewModel 业务规则和列表视口共同决定。
- 真实 Playnite 截图验收仍未完成：本轮插件已由日志确认加载，但控制接口返回 `EmptyWindowAutomationPeer` 且截图错指 Codex 窗口；后续不得将 RenderHarness 或错误窗口截图称为宿主视觉通过。

## 2026-08-19 UI-232 首页风险区域回归事实

- 首页风险列表的正确边界是列表级有限视口，而不是整张风险卡片或首页根容器无限增长：`OverviewAttentionScrollViewer` 与 `OverviewProtectionItemsScrollViewer` 均使用 `GscPageScrollViewer`、`MaxHeight=190`、垂直 `Auto`、水平 `Disabled`。
- 首页宽布局的右侧风险栏必须与 `OverviewRecentActivityCard` 同行并跨越最近任务/全局活动两行；RenderHarness 现在直接按该卡片比较，避免使用整页滚动面导致错误告警。
- 2026-08-19 RenderHarness 已确认高数量风险探针在 190 DIP 视口内滚动，宽布局右侧栏偏移为 `0 DIP`；这只是离屏渲染验证，不等于 Playnite 真机视觉验收。
- 本轮 Playnite 测试命令因长时间无输出和低 CPU 子进程空转被停止，不能写成测试通过；后续需使用可完成的测试入口重新验证。

## 2026-08-19 UI-233 首页风险列表与 Demo 行结构事实

- 首页风险区域的稳定方案是列表级有限视口：`OverviewAttentionScrollViewer` 和 `OverviewProtectionItemsScrollViewer` 使用项目 `GscPageScrollViewer`，`MaxHeight=190`、垂直 `Auto`、水平 `Disabled`。风险数量很多时只滚动列表内部，不能让风险项把首页根高度无限推长；不要再给整张风险卡片叠加一层滚动。
- RenderHarness 的溢出探针已经确认 190 DIP 视口在 1040×700、1100×720 下保持固定且 `scrollable=True`；普通 fixture 未溢出时的 `scrollable=False` 只能表示当时数据不足，不表示没有滚动配置。
- 首页最近任务和全局活动已按 Demo 行结构重排：任务的类型/游戏名、详情/进度、结果/时间分别分层；活动不再保留额外表头和图标列，分类徽标文本使用紫色主题令牌并显式居中。
- 媒体模式栏的外层表面使用 `GscAccentTintBrush`，内部选中状态使用更强的紫色令牌；顶部 Demo 彩色按钮、Demo 滚动条仍不迁移。
- 仅安装裸 .NET SDK 的机器可能缺少 Workload Resolver 目录；`scripts/build.ps1` 和 `scripts/render-qa.ps1` 通过 `MSBuildEnableWorkloadResolver=false` 兼容本项目的 .NET Framework/WPF 构建，不代表项目依赖任何 SDK Workload。
- 2026-08-19 完成一次可复现验证：Playnite UI 测试 248 通过、61 跳过、0 失败；Playnite 与 RenderHarness Release 编译 0 警告/0 错误；RenderHarness 全量 `render-qa OK`。真实 Playnite 宿主视觉截图仍未完成，离屏证据不能替代宿主验收。

## 2026-08-19 UI-236 风险视口和紫色状态验证事实

- 首页“风险与提醒”必须限制列表视口，而不是限制业务集合：`OverviewAttentionScrollViewer` 与 `OverviewProtectionItemsScrollViewer` 使用 `GscPageScrollViewer`、`MaxHeight=190`、垂直滚动 `Auto`、水平滚动 `Disabled`；数量增加时主页面高度保持稳定，列表内部出现项目现有滚动条。
- 首页最近任务、媒体来源和可下载版本的长文本使用有限 Grid 测量、`CharacterEllipsis` 和 Tooltip，防止标题挤压状态徽标、按钮或 Inspector。
- 媒体中心模式栏使用更明确的 `GscAccentTintStrongBrush` 紫色生产资源；该资源变更已同步源码契约测试，Demo 顶部颜色按钮和 Demo 滚动条仍未迁移。
- 重新编译测试后 Playnite 测试为 248 通过、61 跳过、0 失败；之前 38 项失败来自旧测试二进制，不是当前源码结果。
- RenderHarness `render-current3` 全量 `render-qa OK`，但仍属于离屏证据；如果屏幕控制返回 `EmptyWindowAutomationPeer` 或错误窗口，必须记录为宿主视觉阻塞，不能写成 Playnite 真机验收通过。

## 2026-08-19 UI-237 Release 安装事实和宿主视觉边界

- 当前 `main` 的 `37ab9a6` 已完成 Release 一键安装；安装目录为 `%APPDATA%\Playnite\Extensions\GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec`，`extension.yaml` 为 `0.6.70`，DLL 为 `0.6.70.0`。
- 本轮 Release 验证结果为 Core 59/59、Worker 191/191、Playnite 248 通过/61 跳过/0 失败；安装报告保存在 `artifacts/last-dev-install.txt`。
- 首页“风险与提醒”按列表级有限视口实现：两个风险列表使用项目 `GscPageScrollViewer`、`MaxHeight=190`、垂直 `Auto`、水平 `Disabled`。风险数量增加时只在列表内部滚动，不会无限增加首页高度。
- 真实 Playnite 截图验证仍未通过：Computer Use 唯一返回的窗口标题是 Playnite，但截图内容是其他桌面窗口。此类结果只能记为宿主视觉阻塞，绝不能宣称页面已在 Playnite 中 1:1 验收。

## 2026-08-19 UI-238 首页风险与提醒视口事实

- 首页“风险与提醒”现在有独立的 `OverviewRiskViewport`，使用生产 `GscPageScrollViewer`，最大高度为 `330 DIP`，垂直滚动 `Auto`、水平滚动 `Disabled`。风险数量增加时，首页主内容高度不再被风险条目无限撑大。
- 风险区标题、说明和底部“打开维护中心”按钮位于外层视口之外；展开的最近游戏保护明细仍保留 `OverviewProtectionItemsScrollViewer` 的 `190 DIP` 内部视口。前者限制整个风险提醒栏，后者限制展开明细列表，不是无意义地叠加两个相同滚动条。
- `OverviewRiskScrollViewer` 仍然是兼容 `Panel` 节点，真实 `AttentionFindings` 与 `RecentProtection.Items` 绑定不变；不要把兼容节点直接改成同名 `ScrollViewer`，否则会破坏响应式代码和源码契约测试。
- 2026-08-19 已用单节点测试入口完成 Playnite 249/61、Core 59/59、Worker 191/191；RenderHarness 全量 `render-qa OK`，但这些结果仍不能替代可识别 Playnite 窗口的真实宿主截图。

## 2026-08-19 UI-239 媒体中心结构基线事实

- 媒体中心 Demo 的顶部结构是四张独立指标卡，下面是共享紫色分段 Tab；不能把统计数字、模式 RadioButton 和 Tab 再混合到一个横向条带中。
- 当前生产 `MediaCenterView` 使用 `UniformGrid MediaSummaryPanel` 承载四张 `GscRedesignMetricBorder` 卡片，宽度按 4/2/1 列响应式重排；`MediaTabControl` 基于 `GscRedesignWorkspaceTabControl`，其选中项使用生产紫色强调令牌。
- `MediaModeStrip`、`MediaModeRadio`、`MediaContentTabs` 和 `OnMediaModeChecked` 已从生产媒体页移除；真实 Tab 内容、Binding、Command、虚拟化列表和项目滚动条保持不变。
- 2026-08-19 的 Release RenderHarness 已确认媒体三 Tab 在浅色/深色、多尺寸和缩放过渡下可渲染；离屏 PNG 只能证明结构和布局探针通过，不能替代 Playnite 真机截图。

## 2026-08-19 UI-240 首页风险展开态验证事实

- 首页风险侧栏固定为 410 DIP 宽；`OverviewRiskViewport` 根据窗口高度在 500–720 DIP 之间限制，风险总列表使用生产滚动条，避免风险数量把首页无限撑高。
- 展开“最近游戏保护明细”时，隐藏同一数据源的只读预览列表；明细列表使用独立 300–420 DIP 视口。明细卡片按游戏名、状态、换行说明、查看操作纵向布局，原有选择和命令 Binding 不变。
- 2026-08-19 在新安装的真实 Playnite 窗口 `10621340` 中，展开后重新滚动并获取新截图，确认重复预览已隐藏，完整明细卡片和“查看”操作可见且无重叠。该证据仅覆盖首页风险侧栏，不代表其他页面完成宿主视觉验收。

## 2026-08-19 UI-241 任务中心搜索栏事实

- 任务中心搜索输入区不再使用独立的“搜索任务…”标签列；提示文字与搜索图标在输入框内部，`TaskSearchTextBox` 仍绑定真实 `TaskSearchText`。
- 桌面布局让搜索区占据筛选栏剩余宽度；紧凑布局时搜索区独占第一行，状态、类型、刷新在第二行，避免再次出现输入框被压成窄条或控件重叠。
- 2026-08-19 Release 构建、Core 59/59、Worker 191/191、Playnite 250/61/0 通过；安装已成功。但最终宿主截图验证被用户物理 Escape 中止，不能把本轮写成 Playnite 视觉验收完成。

## 2026-08-19 UI-242 真实宿主搜索栏复核事实

- 已重新启动并绑定真实 Playnite 生产窗口，确认截图目标为生产 `GameSaveCenter`，不是 AcrylicFork Preview。
- 任务中心真实宿主截图确认：搜索提示和搜索图标位于 `TaskSearchTextBox` 内部，输入框占据筛选栏剩余宽度；状态、类型和刷新控件各自保持独立边界。
- `scripts/validate-source.py` 已按当前 `OverviewActivityList` 的真实 Grid/页面滚动宿主结构修正有限视口判断，避免静态门禁把合法布局误报为无限测量。
- 本次真实宿主证据只覆盖首页入口和任务中心搜索区；媒体、存档、修改器、维护和首页风险展开态仍必须逐页截图复核，不能把离屏 RenderHarness 或单页截图写成全量 1:1 完成。

## 2026-08-19 UI-243 当前视觉验收边界

- 任务搜索提示已经和输入框合并；媒体页局部 Tab 已恢复共享紫色分段样式；首页风险侧栏和保护明细使用有限视口，保护明细采用纵向可读卡片。
- 本轮 Release 安装和 Core/Worker/Playnite 测试通过，但 Computer Use 未取得可识别 Playnite 窗口；离屏渲染、安装清单和测试不能替代宿主视觉验收。
- 后续逐页截图必须在同一 Playnite 宿主同时打开生产扩展和 AcrylicFork Preview，分别记录窗口、页面、分辨率、主题和滚动位置；若截图目标不是 Playnite 页面，立即记为阻塞并释放控制。

## 2026-08-20 UI-244 表头前景与媒体摘要卡事实

- 最近任务和任务中心表头不能只设置 `Foreground`：WPF 的 `DataGridColumnHeader` 内容还可能通过 `TextElement.Foreground` 继承宿主默认黑色。共享表头、表头呈现器和任务局部表头现在同时显式绑定生产主题文本令牌，首页任务模板的标题、游戏名、详情和结果也有明确前景色。
- 媒体中心四张摘要卡使用共享 `GscRedesignMetricBorder`，统一采用紧凑内边距、72 DIP 最小高度、14 DIP 圆角和 24 号数字；卡片仍由 `UniformGrid` 等宽承载，不混入 Tab 或来源规则布局。
- 2026-08-20 RenderHarness 最终报告 `artifacts/ui-qa/phase-home-media-cards-final/render-qa-report.txt` 为 `render-qa OK`，覆盖浅色/深色、多窗口尺寸和回弹过渡；Core 59/59，Playnite 251 通过、61 跳过。该证据属于离屏渲染，不能替代可识别 Playnite 宿主的逐页截图。

## 2026-08-20 UI-254 设置页分类栏与任务页 Demo 骨架事实

- 生产设置入口文件是 `src/GameSaveCenter.Playnite/Settings/GameSaveCenterSettingsView.xaml`，不是 `Views/SettingsView.xaml`。当前结构必须保持 `SettingsWorkspace` 的 190 DIP 分类栏、16 DIP 间距和右侧 `SettingsScroller`；分类 ListBox 名称是 `SettingsSectionTabs`，事件是 `OnSettingsTabSelectionChanged`。
- 设置页五个可见面板分别是 `SettingsGeneralPanel`、`SettingsBackupPanel`、`SettingsAppearancePanel`、`SettingsAutomationPanel`、`SettingsMigrationPanel`。切换只改变 `Visibility`，不得把真实字段 Binding、Validation、Playnite 保存按钮语义或导入/导出命令移入 Mock 数据。
- 设置页常见 1040px 逻辑窗口仍使用左侧分类栏；`ApplyResponsiveLayout` 的极窄分支为 `layoutWidth < 560`，窄标题阈值为 `layoutWidth < 520`。常见窗口必须让右侧 `GscPageScrollViewer` 获得有限视口，不能让五项分类栏占满第一屏。
- RenderHarness 的 ListBox 分段入口发现规则同时接受名称以 `SegmentTabs` 结尾的迁移页和生产设置的 `SettingsSectionTabs`；设置布局探针验证五个 `ListBoxItem` 可见可测和右侧内容视口，不再查找旧 `SettingsHeaderScroller`/TabControl。
- 当前 `artifacts/ui-qa/task-settings-final/render-qa-report.txt` 为 `render-qa OK`；这是离屏证据。Playnite 生产宿主 Light/Dark、Follow、DPI、键盘焦点与逐页真实截图仍需单独人工验收。

## 2026-08-20 UI-255 共享工作区表格事实

- `Themes/Redesign.xaml` 的 `GscRedesignWorkspaceDataGrid` 是 Save/Media/Maintenance/Task 四个提取页的显式 LabGrid-like 行为基类，集中保护 `RowHeight`/`ColumnHeaderHeight`、FullRow 单选、列宽调整、排序、`VirtualizingPanel.ScrollUnit=Item`、Recycling、行/列虚拟化和 Auto 内部滚动。
- 页面样式可以继续覆盖 `RowStyle`、`ColumnHeaderStyle`、Background 和媒体专用表头，但不能恢复各页复制一整套表格行为 setter 的分叉模式；新增工作区表格应优先基于该 key，并补充源契约测试。
- `GscCodeFontFamily` 当前为 `Cascadia Mono, Consolas, Microsoft YaHei UI`；维护诊断摘要已使用该 token。业务 Expander 已统一采用 `GscDisclosureCard`，当前没有引入 Demo 滚动条。
- 新自动证据：`artifacts/ui-qa/shared-grid-contract-final/render-qa-report.txt` 为 `render-qa OK`；Release 0 warning/0 error；Core 59/59；Worker 190/190（排除 Soak）；Playnite 251 通过/61 跳过/0 失败；WPF validator 0 error/20 warnings/161 info。
- 本阶段仍不能声称真实 Playnite 宿主逐页验收完成；宿主截图、Follow/高对比度、DPI、键盘/UI Automation、真实长文案和大数据量滚动仍是后续人工边界。

## 2026-08-20 UI-256 共享按钮与反馈资源事实

- Dashboard 的 Toast/Dialog 视觉资源现在位于 `Themes/Redesign.xaml`：`GscRedesignFeedbackToastCard`、`GscRedesignFeedbackDialogCard`、对应遮罩和文字样式；页面代码只负责真实事件、状态、动画、计时器和完成结果。
- Dashboard 不再声明与 `DesignTokens.xaml` 重复的原生 `GscButtonBase`/`GscPrimaryButton` 模板；Toast 关闭/详情按钮和确认 Dialog 按钮复用全局按钮契约。页面级 `ui:Button` 继续使用 `GscWpfUiToolbarButton`、`GscWpfUiActionButton`、`GscWpfUiContextButton` 等共享语义样式，不能新增局部按钮模板解决单页问题。
- `UiNotificationRequested`、`UiConfirmationRequested`、`UiChoiceRequested` 的事件与安全完成逻辑保持不变；设置页导入报告/错误仍使用原生 `MessageBox`，这是为避免 Playnite 共享 Window 中 Window-wide WPF-UI host 冲突的有意边界。
- UI-256 自动证据：XAML 18 文件通过，源码门禁通过，Release 0 warning/0 error，Core 59/59，Worker 190/190（排除 Soak），Playnite 252/61/0，`artifacts/ui-qa/feedback-surfaces-final/render-qa-report.txt` 为 `render-qa OK`，WPF validator 0 error/20 warnings/161 info。
- 仍未完成真实 Playnite 宿主逐页视觉验收；不要把 RenderHarness 的反馈资源加载或 PNG 结果写成 Playnite Light/Dark/Follow、DPI、高对比度和键盘/UI Automation 已验收。

## 2026-08-20 UI-257 首页有限视口事实

- `OverviewStackScrollSurface` 保持现有生产页面滚动条和 `HorizontalScrollBarVisibility=Disabled`；`OverviewLayoutGrid` 必须绑定 `ViewportWidth` 并使用有限宽度，否则 WPF 的无限横向测量会让星号列按内容期望宽度增长，裁切当前游戏卡片和真实按钮。
- 当前首页响应式证据：`artifacts/ui-qa/overview-responsive-ui257/render-qa-report.txt`。1366×768 的 workspace 为 1042 DIP，Hero 为 506 DIP、当前游戏卡片为 x=520..1026，操作按钮高度均为 38 DIP；1600×900 同样无横向溢出。RenderHarness 仅是受控 WPF 证据，不等同 Playnite 嵌入视觉验收。
- UI-257 最终门禁：XAML 18/18；Release 0 警告/0 错误；Core 59/59；Worker 191/191；Playnite 256/318（62 跳过）；`validate-source.py` 通过；WPF 静态审查 0 error、20 warnings、146 info。
- 三次真实宿主审计均确认生产扩展 0.6.70.0 可加载并读取真实数据，最新受控证据位于 `artifacts/ui-host-audit-ui257-final`；但 Playnite 返回 `EmptyWindowAutomationPeer`，未能取得可识别的嵌入页面像素截图。不得把受控窗口截图写成 Playnite 1:1 完成，七页 Demo-first 总目标仍处于进行中。

## 2026-08-20 UI-258 生产宿主七页人工嵌入事实

- 本轮已在真实 Playnite 生产扩展 `GameSaveCenter 0.6.70.0` 中打开七个目标页面；生产壳标题为 `GameSaveCenter 生产版`，当前游戏为 `Bongo Cat`。这是真实嵌入窗口的人工 Computer Use 复核，不是离屏或受控窗口截图。
- 首页、存档、媒体、任务、修改器、维护均从生产壳左侧导航实际进入。首页的当前游戏卡片和操作按钮完整可见；存档的立即备份/全部备份与四个标签可见；媒体显示 30 项、5.76 MiB、待归类 4468 项；任务显示 50 条任务、0 运行中、16 需关注、34 今日完成；修改器显示 Wo Long 与 Yakuza 3 工具及右侧工具设置；维护显示进程映射和诊断页。
- Media Inbox 已实际进入并选中截图，独立 Inspector 滚动后可见预览、归类游戏 ComboBox、“确认归类”和“忽略并保留副本”。本轮不执行这些动作，因此没有改变真实数据。
- 设置通过 Playnite 游戏右键菜单的 `GameSaveCenter → 打开设置` 实际打开，显示 `GameSaveCenter 设置` 的“常规与目录”页面及 Worker、Ludusavi、存档目录字段；关闭时未保存更改。
- 自动审计事实仍不变：Playnite 主窗口的 UIAutomation 树是 `EmptyWindowAutomationPeer`，脚本没有 `summary.json` 的嵌入逐页像素证据；人工截图可证明真实页面能进入和关键控件可达，但不能替代自动门禁，也不能外推到其他 DPI、主题/Follow、高对比度或完整操作回归。

## 2026-08-20 UI-259 媒体收件箱共享虚拟化事实

- `MediaInboxGrid` 现在只保留页面需要的 `ScrollUnit=Item` 与顶部对齐，行/列虚拟化和 `VirtualizationMode=Recycling` 统一从 `GscRedesignWorkspaceDataGrid` 继承；禁止在媒体实例上恢复 `Standard` 或关闭列虚拟化。
- `tests/GameSaveCenter.RenderHarness/Program.cs` 的 `Media-Inbox` 探针使用 60 项真实形状的 `MediaItemDto` 夹具，覆盖 287/311/337/353/419 DIP 视口与 0/25/50/75/100% 滚动位置，检查 Recycling、列虚拟化、首行无 phantom gap 与末行可达。
- UI-259 证据：`artifacts/ui-qa/media-virtualization-fix/render-qa-report.txt` 为 `render-qa OK`；Release 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 256/318（62 跳过）；WPF validator 0 error、19 warnings、146 info。
- 该阶段未改变真实媒体绑定、Inspector 或归类/忽略/保留副本命令；真实宿主七页人工证据沿用 UI-258，不能把本轮离屏探针写成新的 Playnite 视觉截图。

## 2026-08-20 UI-260 存档页标题必须跟随真实当前游戏

- 生产壳 `AcrylicProductionShellView.xaml.cs` 不得保留 Demo 游戏名；存档页副标题必须由 `SelectedGame.Name` 生成，空选择使用“未选择游戏”。
- `UpdatePageHeader` 同时由工作区切换和 `DashboardViewModel.SelectedGame` 属性变更调用，保证当前游戏选择器改变后标题副文案不会滞后。
- UI-260 安装验证：XAML 18/18、Release 0 warning/0 error、Core 59/59、Worker 191/191、Playnite 257/319（62 跳过）；定向契约 14/14。
- 真实 Playnite 修复前复核已捕获 `Bongo Cat` 选择器与 `Elden Ring` 存档副文案不一致；修复后安装已完成，但重启后的 Computer Use 窗口暂时不可捕获，因此不把修复后截图写成宿主像素证据。
- GSC-086 常规宿主滚动复核已完成（4468 条媒体收件箱数据，顶部/中部/底部/快速滚轮/返回顶部无白色空视口）；DPI、窗口缩放、Follow/高对比度、键盘焦点和真实业务操作仍是人工边界。

## 2026-08-20 UI-261 工作区 Tab 栏视觉例外

- Demo-first 视觉基准不覆盖生产页 Tab 栏：用户明确要求继续使用项目当前 Tab chrome，因为它比 Demo 的外层连续分段胶囊更合适；后续迁移不能把该页签视觉重新替换为 Demo 样式。
- `GscRedesignWorkspaceTabControl`/`GscRedesignWorkspaceTabItem` 已在共享 `Themes/Redesign.xaml` 中恢复项目原有的透明 header 带、横向 HeaderScrollViewer、11 DIP 独立圆角页签、选中强调色、焦点视觉和内部 8 DIP 防裁切槽。页面仍保留 Demo 的周边布局以及真实 TabControl/TabItem、内容 Stretch、绑定和命令。
- Save、Media、Maintenance 的顶层页签和维护页内部页签均通过共享契约；不要在单页 XAML 复制一套 TabControl 模板来绕开该例外。
- RenderHarness 的 `SnapshotLayoutMetrics` 对重复模板部件名按出现顺序添加 `#2` 等稳定后缀，解决维护页嵌套 TabControl 的合法同名 `HeaderScrollViewer` 导致 `ToDictionary` 重复键的问题。
- UI-261 证据：源码/XAML/差异检查通过，定向契约 15/15，`artifacts/ui-qa/project-tab-chrome-rollback/render-qa-report.txt` 为 `render-qa OK`；代表离屏截图已确认 Save/Media/Maintenance 的项目 Tab chrome。Tab 回滚后的完整安装也通过：Release 0 warning/0 error、Core 59/59、Worker 191/191、Playnite 258 通过/62 跳过、安装 0.6.70/DLL 0.6.70.0；WPF validator 0 error、19 warnings、161 info。
- 真实宿主重装后的稳定前台截图仍缺失；不要将离屏证据扩写为 Playnite 1:1 验收。DPI、窗口缩放、Follow/高对比度、键盘焦点和真实备份/媒体操作仍是总目标边界。

## 2026-08-25 UI-315 共享自适应毛玻璃材质

- 游戏背景的真实图片、壳体底层模糊和卡片共享材质是三层职责：图片只绘制一次并居中裁剪；BlurEffect 只放在壳体图片层；卡片/表格/浮层通过共享 DynamicResource 使用采样色渐变。
- `AdaptiveThemePaletteFactory.ApplyGameBackgroundGlassResources` 是共享表面入口。当前覆盖 `GscGlassFillBrush`、`GscGlassStrongBrush`、`GscTableHeaderBrush`、`GscPopupBrush` 和 `CardBackgroundFillColorDefaultBrush`，从而覆盖 Redesign 的 SectionCard、TableFrame、Hero、Metric、FloatingPicker 及 WPF-UI Card。
- `DashboardView.ApplySelectedGameGlassResources` 必须在主题刷新和 `SelectedGameBackgroundAmbientBrush`/`HasSelectedGameBackgroundAmbientMaterial` 变化时同步 Dashboard、生产壳及所有 workspace 的本地 ResourceDictionary；切换到无图游戏时必须恢复 `ApplyDemoCoreResources` 的中性资源。
- 不要给每张卡片增加 BlurEffect，也不要把游戏采样色直接作为完全透明背景；前者会模糊文字并增加视觉树成本，后者会让表格在亮色图片上失去可读性。共享表面应保持受控 alpha，真实 Blur 继续留在底层图片。
- 输入框、ComboBox 等交互控件暂不跟随图片大幅变色；如果未来扩大范围，先验证文本对比度、焦点边框、禁用态和高对比度回退。

## 2026-08-25 UI-316 设置页独立毛玻璃材质

- 设置窗口是独立页，不跟随当前游戏图片取色；它通过 `AdaptiveThemePaletteFactory.ApplySettingsMaterialResources` 使用主题 Accent/Info/Success 生成自己的环境渐变和透明材质层。
- `GameSaveCenterSettingsView.xaml` 的外壳、分类栏、右侧 `SettingsScroller`、设置 `Card` 和表单输入继续使用分层 DynamicResource；只有 `SettingsAmbientLayer` 承载整页 BlurEffect，不能把模糊效果挂在卡片或输入控件上。
- `GscSettingsShellBrush`/`PanelBrush`/`CardBrush`/`ContentBrush` 的 alpha 需要保持层次：底层环境渐变可见，表单文字和输入值仍清晰；禁用玻璃和高对比度必须恢复不透明回退。
- RenderHarness 的设置页渲染应显式调用 `ApplyThemeForAudit`，否则只会捕获 `DesignTokens` 初始回退，无法验证设置页运行时玻璃资源是否真正生效。
- UI-316 已通过源码/XAML 校验、Release 全量构建测试和浅色/深色/多尺寸 render-qa；WPF validator 为 0 error、18 warnings、172 info。真实 Playnite 重启后的逐页像素证据仍不具备，不得扩写为宿主验收。

## 2026-08-25 UI-320 游戏选框圆角与筛选默认值事实

- 生产壳游戏选框 `AcrylicProductionShellView.xaml` 的 `PickerList` 必须基于共享隐式 `ListBoxItem` 样式；局部样式只能覆盖间距、对齐和文本前景，不能让 Playnite 默认模板接管选中/预选状态，否则会重新出现矩形高亮。
- 生产壳三个游戏筛选框必须同时保留 `SelectedIndex="0"` 与真实 `SelectedItem` 双向绑定的 `TargetNullValue`/`FallbackValue`。平台选项集合会异步重建，代码需要订阅 `PlatformFilterOptions.CollectionChanged` 并调用 `UiFilterSelection.RestoreDefault`，不要只依赖 XAML 初始 `SelectedIndex`。
- 首页游戏选框自定义 Row 若使用 `CornerRadius`，必须同时使用 `ClipToBounds="True"`；否则圆角背景下的内容/状态层可能露出矩形。
- 真实游戏选择绑定、`SelectedGame` 更新、列表虚拟化和关闭弹层逻辑保持不变。当前 `.tmp/ui-qa-game-picker-rounded-defaults/render-qa-report.txt` 仅是离屏证据，不能写成真实 Playnite 弹层视觉验收。

## 2026-08-25 UI-321 平台筛选默认值时序事实

- 生产 `PickerOverlay` 初始为 `Collapsed`，不能只在 `Attach` 或点击事件的同步代码中调用 `UiFilterSelection.RestoreDefault`；这些时刻可能还没有生成 ComboBox Items。
- 平台筛选默认值恢复必须覆盖弹层打开、平台选项集合变化和 `GamePickerPlatformComboBox.Loaded`，并至少排队到 `DispatcherPriority.DataBind`、`DispatcherPriority.Loaded` 两个阶段。
- `UiFilterSelection.RestoreDefault` 只应修复空选中或不再属于当前 Items 的无效选中；有效的用户平台选择不能被“全部”覆盖。
- UI-321 已通过源码/XAML 门禁、Release 构建和 Playnite 定向测试；真实宿主需重载扩展后确认中间框显示“全部”。

## 2026-08-25 UI-322 底部状态栏与侧栏折叠事实

- 生产壳 `FooterSurface` 现在位于根 Grid 的第 0 列并跨两列，Worker/Ludusavi 状态灯由 `FooterStatusPanel` 承载；侧栏不再放状态卡。新增状态展示必须继续使用 `Snapshot.WorkerHealthy` 与 `Snapshot.LudusaviAvailable`，不能复制静态健康状态。
- 侧栏默认宽度是 236 DIP；`SidebarCollapseButton` 是品牌区内 26×26 的小型共享 `GscWpfUiButton`，点击通过 `ApplySidebarLayout` 切换 78 DIP 图标态，再调用既有页头/页面响应式布局。不要把折叠入口做成导航项，也不要默认启动为折叠态。
- 折叠态只隐藏品牌文字、生产版标签和导航文字，并保留 ToolTip/AutomationProperties.Name；导航 RadioButton 仍是同一组真实工作区入口，绑定、命令、滚动和虚拟化不变。
- UI-322 的源码/XAML/Release/Playnite/RenderHarness 门禁已通过；RenderHarness 只证明页面主题和响应式回归，不等同真实 Playnite 侧栏折叠像素或键盘验收。

## 2026-08-25 UI-323 当前事实：状态栏右对齐、版本气泡与设置尺寸

- 生产壳底部 `FooterStatusPanel` 位于 Footer 的右侧 Auto 列；底部不再显示产品名和“生产版 · 真实数据由 Worker 提供”，状态文字仍必须绑定 `Snapshot.WorkerHealthy`/`Snapshot.LudusaviAvailable`。
- `SidebarProductionVersionText` 在壳体 Loaded 时从 `AcrylicProductionShellView` 程序集读取三段版本号，XAML 的 `v0.6.70` 只是安全初始值；不要将它改回“生产版”静态标签。折叠按钮在 `SidebarUtilityStrip`（标题下方独立工具条），品牌行只负责图标、名称和版本气泡。
- 设置入口 `GameSaveCenterSettingsView` 现在请求 `MinWidth=1180`、`MinHeight=760`；内部 `SettingsShell` 仍受 1360 DIP 上限和原有响应式断点控制，不能为了放大窗口移除滚动或改变保存语义。
- UI-323 的源码/XAML/Release/Playnite/RenderHarness 门禁已通过；RenderHarness 的设置尺寸证据不等同 Playnite 宿主最终窗口尺寸，需重载扩展后人工确认。

## 2026-08-25 UI-324 当前事实：侧栏折叠书签与过渡动画

- `AcrylicProductionShellView.xaml` 的 `SidebarCollapseButton` 不再位于品牌标题下方工具条，而是覆盖在侧栏右侧靠近底部的位置；它使用 `AcrylicSidebarBookmarkButton` 共享 ControlTemplate，呈垂直书签轮廓，默认展开态仍为 236 DIP，折叠态仍为 78 DIP。
- 书签是独立于 `SidebarContentLayer` 的交互层，因此不会挤压 `GameSaveCenter`、版本气泡或真实 `Nav*` 项；导航内容继续由同一组 RadioButton、绑定、滚动和虚拟化承载。
- `OnSidebarCollapseClick` 在动画启用时对 `SidebarContentLayer` 做短暂淡出、4 DIP 横向位移再淡入；`MotionEnabledProvider` 从 `DashboardView` 提供持久化动画设置，系统高对比度/禁用动画时走同步切换。不要把动画改成循环计时器，也不要给页面列表内容加 BlurEffect。
- 书签 ControlTemplate 只使用共享动态材质和状态触发器，包含悬停、按下、键盘焦点、禁用和 Tooltip/AutomationProperties；不要将折叠按钮恢复为普通导航项或重新放回品牌行。
- UI-324 已通过源码/XAML、WPF 0 error、Release、Playnite 295/352 和 RenderHarness `render-qa OK`；真实 Playnite 宿主点击/键盘/DPI 像素仍是人工边界。

## 2026-08-25 UI-325 当前事实：侧栏底部一体式折叠控件

- UI-325 覆盖 UI-324 中“字面垂直书签”的视觉指导。当前不得恢复 `AcrylicSidebarBookmarkButton`、Path 丝带轮廓或贴在侧栏右边的书签形状；“书签”只表示用户提供的底部控制位置概念。
- 当前共享样式是 `AcrylicSidebarCollapseButton`，基于 `GscWpfUiButton` 的普通圆角按钮。展开态在侧栏底部显示图标、“收起侧栏”和右箭头，约 168×34 DIP；折叠态为 40×34 DIP 的小圆角按钮，只保留居中的展开图标。
- 折叠态必须同时设置 `SidebarCollapseButton` 与 `SidebarCollapseButtonContent` 的居中；所有 `Nav*Content` 在折叠时也必须显式 `HorizontalAlignment=Center`。不能只隐藏文字后依赖默认 ContentPresenter 推断位置。
- `OnSidebarCollapseClick` 的动画、MotionEnabledProvider、系统动画/高对比度降级以及 `ApplyHeaderLayout`/`ApplyPageLayout` 重算继续保持；不能为了换控件改变真实导航绑定、滚动、虚拟化或版本气泡。
- UI-325 已完成源码/XAML、Release、定向折叠契约和 RenderHarness `render-qa OK`；完整测试与真实 Playnite 点击/键盘/DPI 像素复核是本阶段提交前/宿主边界。

## 2026-08-25 UI-326 当前事实：折叠图标中心线与首页右侧卡片密度

- 折叠态导航的对齐基准是侧栏内部 26 DIP 图标槽：`SidebarHeaderLayout` 去掉展开态不对称边距，`SidebarBrandContent` 与所有 `Nav*Content` 在折叠态固定宽度并居中；图标 `TextBlock` 必须保持 `TextAlignment="Center"`，不能只依赖 StackPanel 的默认测量。
- `AcrylicProductionShellView.xaml.cs` 的 `ApplySidebarLayout` 仍是展开/折叠唯一布局入口，需保留 `ApplyHeaderLayout`、`ApplyPageLayout` 和导航 RadioButton 的真实绑定/滚动/虚拟化。
- 首页 `OverviewProtectionPreviewCard` 的圆点列为 14 DIP，以便状态点和游戏标题之间保留轻微间距；`OverviewAttentionScrollViewer` 的有限视口为 220 DIP，页面根滚动继续负责更长内容。
- 这些调整只改变共享布局密度，不改变风险状态语义、关注项真实数据、操作命令或主题资源。
- UI-326 已通过 source/XAML 门禁、WPF 0 error、Release 构建、Core 59/59、Worker 199/199、Playnite 295/57 skipped/0 failed，以及双主题多尺寸 `render-qa OK`。
- 真实 Playnite 重启后的折叠像素、Follow/浅色/深色、DPI 和键盘焦点仍未由本轮重新确认；不得把 RenderHarness 截图写成真实宿主逐像素验收。

## 2026-08-26 CORE-327 保留策略清理安全闭环

- `RetentionSimulationApplyRequestDto` 必须包含 `PreviewGeneratedUtc`、`ExpectedCandidateCount` 和 `ExpectedReleaseBytes`；Worker 在加载当前索引后重新计算候选数量/体积，不匹配或超过 10 分钟就抛出 `RETENTION_PREVIEW_STALE`，缺少预览则抛出 `RETENTION_PREVIEW_REQUIRED`。
- `RetentionSimulationService` 处理候选 ZIP 时先移动到备份根下的 `.gsc-retention-quarantine/<batch>/<backupId>.pending`，再删除索引；索引删除失败要尝试原路恢复。不能恢复时必须保留审计明细，不能直接把原文件静默删除。
- 隔离目录中的文件不使用 `.zip` 后缀，以免被后续 Ludusavi 归档扫描误识别；清理失败通过 `PendingQuarantineCount`/`PendingQuarantineBytes` 返回并记录审计。
- 预览 UI 最多展示 200 条候选明细，摘要必须明确“前 N 条/全部候选”，避免产生完整列表的错误认知。
- CORE-327 已完成 Worker 定向测试 6/6；本阶段跳过真实 Playnite 宿主验收，不能把离线测试写成主题、DPI 或宿主行为证据。

## 2026-08-26 CORE-328 诊断环境信息与首次检查性能

- 诊断包展示 DPI 必须来自 `GetDpiForSystem`，不能用 `SystemParameters.PrimaryScreenWidth / WorkArea.Width`；屏幕数量来自 `GetSystemMetrics(80)`，API 失败时回退 1。
- `EnvironmentCheckRequestDto.IncludeBackupProbe` 控制是否调用 Ludusavi 的全库只读列表；首次自动检查传 false，手动“重新检查”传 true。`IncludeRemoteProbe` 同样在首次自动检查关闭，避免启动时网络探测。
- IPC 默认请求仍保持完整探测（两个开关默认 true），只有 Playnite 首次启动路径显式使用快速模式，避免改变其他调用方语义。
- CORE-328 的真实逐窗口 DPI、Playnite 多屏位置和远端探测宿主行为仍属于跳过的人工验收边界。

## 2026-08-26 PERF-329 大库更新回归门槛

- `LargeLibraryPerformanceTests.GamePicker2000_Benchmark_WritesMeasuredTimings` 不仅写 profiling，还必须保持 2000 条首次/单项变化更新和任务首次替换低于 5 秒、未变化替换低于 1 秒；阈值刻意宽松，只拦截数量级退化。
- 详细基准仍写入 `large-library.txt`，不得把这些离线集合耗时扩写为真实 Playnite 渲染帧率。
