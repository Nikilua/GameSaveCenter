# GameSaveCenter AI 开发工作日志

> 每完成一个有意义的阶段追加一条；只记录对未来开发有帮助的信息。

## 2026-08-16 UI-205-REAL-HOST-MIGRATION-FIX AcrylicFork 生产迁移收口

**实现内容：**

- 将 AcrylicFork 的共享视觉层级继续收口到生产 Dashboard/Settings/Media/Trainer/Save 页面；演示专用右上角颜色按钮、演示数据和样例滚动条没有迁移。
- 修复 Playnite BAML 在私有扩展加载上下文中偶发找不到 `GameSaveCenter.Contracts` 的问题：生产 XAML 不再直接引用外部 Contracts 程序集，新增 `XamlEnumValues` 本地包装值，保留原始枚举对象以维持绑定和 DataTrigger 语义。
- 修复 Dashboard 选中游戏标题栏在首次测量/窗口缩放时保留旧 DesiredSize 的问题：标题、身份区和操作行按最终可用宽度约束，操作按钮窄屏换行，ApplicationIdle 再做一次收敛布局；Save/Media 的表格 viewport 使用短窗口可读性下限。
- Real Host 审计等待 ApplicationIdle 后再截图，并在后续干净轮次清理旧 `CHILD_LAYOUT_OVERFLOW` 门禁，避免把瞬时布局或上一轮证据当成最终失败。

**验证结果：**

- `validate-source.py`、`check-xaml.ps1`、Release 构建通过，0 warning / 0 error；Core 59/59、Worker 191/191、Playnite 303/303。
- `DEV-INSTALL-008` 重新打包安装成功，安装验证为 `extension.yaml 0.6.70`、DLL `0.6.70.0`；Playnite 扩展日志确认生产插件加载，未出现新的 `XamlParseException` 或 Contracts 缺失记录，外来 Preview Worker 保持运行。
- Real Host 受控矩阵覆盖 maximized、1600/1366/1280/1024 与 Light/Dark、工作区和内层 Tab；最终 `gates/overflow-classification.json` 的 `RealFixedLayoutOverflow` 为空，旧 `CHILD_LAYOUT_OVERFLOW.json` 已清除。

**验证边界：**

- `AUTO VERIFIED`：源码/XAML、构建、三组测试、受控截图/滚动证据、安装包和生产扩展加载日志。
- `MANUAL QA REQUIRED`：本机自动 UIAutomation 仍未找到 Playnite 左侧 GameSaveCenter 项，故审计诚实保留 `EmbeddedDashboardCaptured=false`；受控窗口证据不能冒充真实嵌入像素。前一轮人工观察到的真实嵌：首页、存档中心和任务中心在 1280/1024 尺寸下无明显文字重叠或右侧裁切。

## 2026-08-16 UI-205-ACRYLIC-PARITY AcrylicFork 视觉与页面结构迁移

**实现内容：**

- 纠正参考源：以 `D:\workplace\github\GameSaveCenter.AcrylicFork` 的 `b09cba6` 为权威视觉样板；迁移深色/浅色表面层级、Shell/卡片/控件圆角尺度、按钮与标题尺寸、状态色、页面标题和 Settings 分类栏节奏。
- Dashboard 顶部改为 AcrylicFork 式开放页面头部，保留生产中的真实游戏选择器、刷新/备份/同步命令和绑定；Overview 普通状态不再重复显示全局命令卡，仅在真实 onboarding 状态保留操作带。
- 调整 Dashboard 的游戏选择器/操作区响应式排布，Settings 在常用内容宽度使用左侧分类栏，低于 700 DIP 转为顶部横向分类，低于 620 DIP 进一步收紧标题与输入布局。
- 修正共享 DataGrid 表头、Button、Card、状态胶囊和页面标题的圆角/尺寸，使迁移后的控件不再依赖样板中的布局问题；滚动条继续使用生产模板和完整 Track 绑定。

**明确不迁移：**

- AcrylicFork 的演示数据、演示专用右上角颜色/主题按钮、样例滚动条均未引入；顶部七组色值仅作为生产主题参考，生产命令、数据绑定、虚拟化、页面滚动骨架和现有滚动条保留。

**验证结果：**

- `python scripts/validate-source.py`、XAML 检查通过；Release 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 302/302。
- `scripts/render-qa.ps1 -Output artifacts/ui-qa/render-acrylic-correction` 全部通过，覆盖 Light/Dark、7 个工作区、多个窗口尺寸及 resize transition；修正渲染夹具的 Settings 入口动画时钟后，设置页主题截图不再为空，Overview/Save/Maintenance/Settings 无文字重叠。
- `DEV-INSTALL-008` 一键安装成功；安装器只处理当前扩展 Worker，外来 AcrylicFork Worker 保持运行，Playnite 与生产 Worker 均成功启动。

**验证边界：**

- `AUTO VERIFIED`：源码/XAML、Release 构建、三组测试、离屏截图和布局探针。
- `MANUAL QA REQUIRED`：本阶段未重新启动真实 Playnite 宿主；真实 Playnite 主题、DPI、键盘和连续缩放仍需在宿主内确认。

**下一步：**用户在真实 Playnite 中确认嵌入式页面；如无阻塞反馈，不再修 AcrylicFork 演示样本本身。

## 2026-08-16 UI-REAL-HOST-AUDIT-BLOCKERS-FIX 实施完成

- 来源：`GameSaveCenter_RealHost_Audit_Blockers_Prompt.zip`；只修 Audit 可信度。
- CommitSha：`real-host-audit.ps1` 启动前写 `GSC_UI_AUDIT_COMMIT`；metadata 优先级 env → AssemblyInformationalVersion → unknown；unknown 写 `AUDIT_SOURCE_REVISION_MISSING` HIGH。
- Embedded 身份：`IsGenuinelyEmbeddedDashboard`（Loaded + PresentationSource + Window 非 fallback），不再 static null 猜；headless 无人点击时诚实 `EmbeddedDashboardCaptured=false` + `REAL_EMBEDDED_DASHBOARD_NOT_CAPTURED` HIGH。
- SafeFileName 根因是 `string.Join("-", chars)` 每字符插 `-`；改为仅替换非法字符、折叠连续 `-`、保留中文。
- Overflow 分类：fixed/scroll/decorative/false-positive；ScrollViewer 内容与装饰性越界不再误报 HIGH，真实 fixed overflow 仍写 gate；分类统计入 `gates/overflow-classification.json`。
- Resize 稳定：最多 3 pass，等待 DataBind/Loaded/Render/Idle，连续两次几何差 ≤0.5 DIP 才截图，记录 responsive-stable.json。
- Scroll：排除 DG_ScrollViewer/PART_ContentHost/TextBox/ComboBox 内部 scroller；manifest 写真实尺寸/状态/Reason；`Scope=Dashboard|Settings` 隔离。
- 证据：summary CommitSha=d7948c9、Settings embedded=true、Controlled=true；ZIP `artifacts/GameSaveCenter-ui-host-audit.zip`。
- 测试：Playnite 302/302、Worker 191/191、Core 59/59；报告 `docs/ai/REAL_HOST_AUDIT_BLOCKERS_FIX_REPORT.md`。

## 2026-08-16 UI-HOST-AUDIT-TRUTHFULNESS-FIX 实施完成

- 来源：`GameSaveCenter_HostAudit_Truthfulness_Fix_Prompt.zip`；目标是让 Real Host Audit 的 origin、scroll semantics、manifest 与 success state 全部可信。
- Sidebar View 语义：删除 `SidebarItem.Activated` 调用（View 由 Playnite 在用户点击时走 `Opened`），审计就绪时发通知等待真实 Dashboard OnLoaded；fallback 专用窗口显式标记 `AuditHostKind.ControlledAuditWindow`，Dashboard 默认 `EmbeddedPlaynite`。
- Origin 显式化：不再用 `auditDashboardWindow == null` 猜 origin；`CompletedRoots` 按 `kind-dashboard` / `settings` 拆分，Controlled fallback 不阻断 Embedded capture。
- Manifest session 化：`AuditCaptureSession` 独立保存 EmbeddedDashboard/ControlledDashboard/Settings entries；settings manifest 只含 Settings，重复运行不串数据。
- Scroll 语义：`DG_ScrollViewer`/DataGrid 逻辑 item 滚动器不再生成像素长图，标记 `SkippedVirtualized`；`PART_ContentHost` 排除；`CaptureStatus/Reason/OutputWidthPx/OutputHeightPx/ViewportHeight/ExtentHeight/SegmentCount` 真实写入；CompletenessValidated 只在真实校验后为 true。
- 新增 `summary.json` 硬门禁与 `CHILD_LAYOUT_OVERFLOW` gate；`real-host-audit.ps1` 在 Embedded 未捕获时输出 PARTIAL 并以退出码 2 结束。
- 证据：summary `EmbeddedDashboardCaptured=false`（headless 无人工点击，诚实标注）、Settings embedded 10 条、Controlled 363 条、滚动面 86 条 validated；zip `artifacts/GameSaveCenter-ui-host-audit.zip`。
- 回归：Playnite 294/294、Worker 191/191、Core 59/59；报告 `docs/ai/HOST_AUDIT_TRUTHFULNESS_FIX_REPORT.md`。

## 2026-08-16 UI-REAL-HOST-CAPTURE-COMPLETENESS-FIX 实施完成

- 来源：`GameSaveCenter_RealHost_Capture_Completeness_Fix_Prompt.zip`；根因是截图语义模型错误，不是 UI 本身。
- 已确认两个 P0 根因并修复：
  - Controlled Host 过去同时把 Window.Width/Height 与 Dashboard.Width/Height 设成 profile，WPF Window 是 outer size，client 更小，导致右/底被 clip；真实 embedded 场景也不能改 Dashboard 尺寸。现改为无边框审计窗口（client == outer == profile），Dashboard `ClearValue(Width/Height)` + Stretch，并加 `CAPTURE_PROFILE_SIZE_MISMATCH` 断言。
  - `SaveFullPage` 过去只挑 ExtentHeight 最大的一个 ScrollViewer 当作整页，导致 sidebar/header/其他 scroller 缺失。现拆分为三种契约：`embedded-current/viewport`、`controlled/<profile>/<theme>/viewport`、`scroll-surfaces/<route>__<name>.png`，并输出 `capture-manifest.json`。
- 新增完整性断言：viewport 输出尺寸必须等于 `Actual*DpiScale`（`CAPTURE_VIEWPORT_CLIPPED` gate）、边界 sentinel 几何、Controlled client size 断言、所有 meaningful ScrollViewer 枚举。
- 元数据改为真实值：`Mode=embedded-current|controlled-host-window`、`CaptureOrigin`、`DedicatedAuditWindowUsed`、`ProfileSizeApplied`、`ThemeOverrideApplied`；PlayniteDesktopVersion 不再回退成假精确的 1.0.0.0。
- 测试：新增 `UiAuditCaptureContractTests`（6 项：controlled 不改 Dashboard 尺寸、embedded 不 resize/不改主题、DPI 像素契约、多 ScrollViewer 枚举、右下 sentinel 几何、manifest 字段）；Playnite 287/287、Worker 191/191、Core 59/59。
- 最终端到端运行：603 个文件，无 CAPTURE gates；controlled 尺寸 1024/1280/1366 精确命中，1600 受工作区限制为 1600x912；scroll-surfaces 197 张；Settings 由 Playnite 托管时如实标记 `embedded-current-settings`；zip `artifacts/GameSaveCenter-ui-host-audit.zip`。
- 报告：`docs/ai/REAL_HOST_CAPTURE_COMPLETENESS_FIX_REPORT.md`。

## 2026-08-15 LUDUSAVI-DIAGNOSTICS-FIX 实施完成

- 用户诊断：备份失败直接原因是 Ludusavi 联网更新 `raw.githubusercontent.com/.../manifest.yaml` 超时（22:08/22:12 失败，网络恢复后重试成功），不是存档路径或 ZIP 写入问题。
- 三个放大问题的修复：
  - `ExternalProcessRunner` 为 stdout/stderr 显式设置 `Encoding.UTF8`，避免中文错误按系统代码页解码成乱码。
  - `LudusaviClient.ExecuteJsonAsync` 失败分支现在把原始 stdout（stdout 为空时用 stderr）写入 `RawOutput`，任务详情不再显示空 RawOutput。
  - `DashboardViewModel` 新增 `CopyTextWithRetry`：复制详情/健康报告/诊断信息时对剪贴板占用（`CLIPBRD_E_CANT_OPEN`/COMException）重试 3 次，最终失败显示明确提示而不是弹出异常。
- 测试：新增 UTF-8 解码回归测试；Worker 191/191、Playnite 281/281；Release 0 warning/0 error；`validate-source.py`、`check-xaml.ps1` 通过。
- 备注：Ludusavi manifest 网络问题属外部环境，插件侧不重试 manifest 下载；失败时保留原始输出便于用户按 `raw.githubusercontent.com` 超时定位。

## 2026-08-15 UI-REAL-HOST-AUDIT-NESTED-TABS-THEMES 实施完成

- 用户复核继续反馈：Tab 下的嵌套 Tab（如“异常与审计”→“审计记录”）缺失；需要浅色/深色两套截图；截图仍只显示页面局部。
- 修复：
  - `CaptureAllInnerTabs` 改为递归捕获嵌套 TabControl（选择父 Tab 后等待 ApplicationIdle，再从视觉树与 Content 双路径找子 TabControl），已产出 `tab-maintenance-异-常-与-审-计-审-计-记-录.png` 等嵌套页。
  - 真机审计按 `Light`/`Dark` 双主题各跑一遍；Dashboard/Settings 新增 `ApplyThemeForAudit`，设置页兜底窗口现在注入真实 `GameSaveCenterSettings` 作为 DataContext。
  - 整页截图改为始终渲染 Dashboard/Settings 根元素（含侧栏/外壳）并按内容最大滚动高度撑高后输出，不再只截内部滚动器或窗口可视区；maximized 工作区截图现为 1707×1232（Overview 1707×1717）。
- 产物：`artifacts/ui-host-audit/screenshots/<size>/<light|dark>/`，嵌套 Tab、双主题、整页长图齐全；最终 zip `artifacts/GameSaveCenter-ui-host-audit.zip`（约 86MB）。
- 验证：Release 0 warning/0 error；`validate-source.py`、`check-xaml.ps1` 通过。第三方主题/真实 Playnite 窗口内观感仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-REAL-HOST-AUDIT-MULTI-SIZE 实施完成

- 用户复核后补充要求：真机截图要覆盖最大化窗口与多种窗口化尺寸，且项目协作规则要加入“及时清理无用文件”。
- 多尺寸捕获：`RealHostUiAuditService` 现按 `maximized`（实际 WorkArea 1707×912 DIP）+ `1600x1000`、`1366x768`、`1280x720`、`1024x768` 五档循环，每档重新截取 Dashboard、6 个工作区、全部内层 Tab、窗口截图与 Settings 5 个分类。
- 关键修复：
  - Playnite 进程为 DPI-unaware，`GetSystemMetrics` 返回虚拟化小尺寸，导致窗口被内容吸成 640×480；改用 `SystemParameters.WorkArea` 并 `SizeToContent.Manual` + 显式给 Dashboard/Settings 设置宽高，窗口尺寸才与档位一致。
  - 内容截图按逻辑分辨率输出，避免 32 位进程整窗口 1.5× 渲染 OOM；真实 DPI/尺寸记录在 `metadata-<size>.json`。
  - 移除了多尺寸扫描中的全页滚动拼接（单张表格拼接可卡十几分钟），保留页面/Tab/窗口截图；滚动证据仍可由单尺寸离屏审计提供。
- 产物：`artifacts/ui-host-audit/`（5 个尺寸目录 × 6 workspace + 内层 Tab + window + Settings 5 分类）与 `artifacts/GameSaveCenter-ui-host-audit.zip`（约 33MB，379 个文件）。
- 清理：AGENTS.md 与 DEVELOPMENT_HANDOFF 新增“文件清理规则”；删除 `artifacts/` 旧 `dev-build`（7.4GB）、`ui-audit-build`、`phase*`、`audit*`、旧 zip 与 `.tmp/` 全部旧审计目录，仅保留当前安装目录、`.pext`、当前审计输出与日志。
- 验证：Release 0 warning/0 error；`validate-source.py`、`check-xaml.ps1` 通过。真实第三方主题/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-REAL-HOST-AUDIT-FULL-COVERAGE 实施完成

- 用户复核真实宿主截图后指出：脚本运行到 Settings 时停住、只截页面左上部分、未覆盖全部页面；另反馈审计兜底窗口部分超出屏幕且无法关闭。
- 根因与修复：
  - Playnite 主窗口在无交互桌面会话中不可见，UIA 侧栏点击不可靠；插件启动时若存在 `GSC_REAL_HOST_AUDIT`/sentinel，延迟自动激活侧栏项。
  - 侧栏激活仍不落地时，`RealHostUiAuditService.EnsureDashboardCaptured` 在专用窗口（1440×900、左上锚定、ToolWindow 可关闭）内创建真实 `DashboardView` 并触发同一套 Tier B 捕获；Settings 通过 UI 线程专用窗口兜底捕获。
  - Settings 兜底此前失败的两个原因：Dashboard 完成后先删 sentinel，兜底再读 sentinel 得到空路径；以及兜底窗口创建曾调度到线程池 Dispatcher。现改为 `RequestSettingsCapture` 时缓存输出根并直接传入 Dashboard 的 UI Dispatcher。
  - 设置页分类 Header 是复杂 Grid，`Header.ToString()` 全部同名导致去重只截第 1 类；改为从 Header 视觉树提取中文标题，并按索引去重，5 个分类全部截图。
  - `CreateZip` 遇到默认 zip 被占用时改写入带时间戳的独立 zip，不再中断后续 Settings 捕获。
- 产物（`artifacts/ui-host-audit/` + `GameSaveCenter-ui-host-audit.zip`）：Dashboard 6 个 workspace 截图/完整窗口截图、全部内层 Tab、`full-scroll/` 整页拼接（Overview 1099×2579 等）、真实 DPI 1.5 metadata、resource/style/visual-tree；Settings 5 个分类截图 + 完整窗口截图 + SettingsScroller 整页拼接 + metadata。
- 验证：Release 0 warning/0 error；`validate-source.py`、`check-xaml.ps1`、WPF UI 校验 0 errors；Core 59/59、Worker 190/190、Playnite 281/281（dev-install-run 已跑）。
- 说明：无交互桌面下捕获的是“真实 DashboardView + 真实 adaptive palette + 真实 DPI”的专用窗口证据；有可见 Playnite 窗口时会走真实窗口截图路径。真实第三方主题/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-REAL-HOST-PARITY-CLOSURE 实施完成

- 来源：`GameSaveCenter_RealHost_UI_Parity_Audit_Prompt.zip`；计划 `docs/ai/REAL_HOST_UI_PARITY_CLOSURE_PLAN.md`；报告 `docs/ai/REAL_HOST_UI_PARITY_CLOSURE_REPORT.md`。
- 建立 Tier A（Offscreen Regression Audit）与 Tier B（Real Playnite Host Fidelity Audit）双审计链路；README 已明确离屏截图不是 production pixel truth。
- 插件新增 `RealHostUiAuditService`：`GSC_REAL_HOST_AUDIT`/sentinel 触发，从真实 Dashboard 捕获截图、visual tree、resource snapshot、style fingerprint、DPI、bounds，并自动打开 Settings 捕获设置页；不触发业务命令。
- 新增 `UiDiagnosticsExporters` 与 `AdaptiveThemePaletteContrastGuard` 及单测。
- 本机真实宿主捕获成功：DPI 1.5、Dashboard 1264×868、6 个 workspace 证据；发现离屏“更漂亮”主因是 fallback palette vs runtime adaptive palette + 真实 DPI/host bounds/data。
- 新增 `docs/HOST_STYLE_DEPENDENCY_REPORT.md` 与 `docs/HOST_VISUAL_PARITY_REPORT.md`。
- AGENTS.md / DEVELOPMENT_HANDOFF 新增“每轮完成后由 Agent 自己 commit 并 push”项目协定。
- 验证：Release 0 warning/0 error；Core 59/59、Worker 190/190、Playnite 281/281；render-qa 全绿；Offscreen UI Audit 0 HIGH/0 MEDIUM/0 fidelity/0 failed routes。
- 剩余：真实 125-200% DPI、第三方主题、连续缩放与 Settings paired evidence 仍需人工/下次脚本运行确认。

## 2026-08-15 UI-AUDIT11-RESIDUAL-CLOSURE 实施完成

- 来源：`GameSaveCenter_Audit11_Residual_UI_Closure_Prompt.zip`；计划 `docs/ai/UI_AUDIT11_RESIDUAL_UI_CLOSURE_PLAN.md`；报告 `docs/ai/UI_AUDIT11_RESIDUAL_UI_CLOSURE_REPORT.md`。
- SaveHistory 大小列：90 → 116 DIP，`SaveSizeValue` 使用 `TextTrimming=None` + `Tag=SaveHistorySize`；narrow 状态列仍完整。
- Maintenance Device：Compact/Narrow 改为详情切换（查看设备详情 ›），展开 viewport >= 180 DIP，表格保留 header + 2 行。
- Audit 新增 `SHORT_SEMANTIC_VALUE_TRIMMING` 与 `INTERACTIVE_INSPECTOR_USABILITY` 失败门禁；Settings 滚动目标整数取整。
- 验证：Release 0 warning/0 error；Core 59/59、Worker 190/190、Playnite 276/276；render-qa 11 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/0 fidelity/0 failed routes。
- 审计 ZIP 因标准路径被其他进程锁定，输出到 `artifacts/audit11-final/GameSaveCenter-ui-audit.zip`。
- 真实 Playnite 宿主/DPI 125%/150%/主题/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-FIDELITY-CLOSURE-AUDIT10 实施完成

- 来源：`GameSaveCenter_UI_Fidelity_Closure_Audit10_Prompt.zip`；计划 `docs/ai/UI_FIDELITY_CLOSURE_AUDIT10_PLAN.md`；报告 `docs/ai/UI_FIDELITY_CLOSURE_AUDIT10_REPORT.md`。
- Maintenance：移除局部 implicit `DataGridColumnHeader` style，中间列表头恢复渲染（Diagnostics/Device/Audit Findings/Audit Log/Process 共 11 个 header）；filler header 继续由共享模板主题化。
- Media：搜索框改为 `Auto/*(MinWidth=160)/Auto/150` Grid，不再 54.67 DIP；narrow 内容宽约 390 DIP。
- Settings：选中分类在 SelectionChanged/ApplyResponsiveLayout 后同步 scroll-into-view，保留 BringIntoView 回退并用增量 delta 收敛；最后分类 HorizontalOffset=359.33。
- Save History：narrow 收起备注 summary 列，状态列留在视口内；完整备注仍在 Inspector。
- Audit：新增 `HEADER_CONTENT_FIDELITY`、`ACTIVE_TAB_VISIBILITY`、`CONTROL_USABILITY_GEOMETRY`、`ESSENTIAL_COLUMN_VISIBILITY`，全部 MEDIUM + 失败门禁；修复前可复现 92 个失败，修复后 0。
- 验证：Release 0 warning/0 error；Core 59/59、Worker 190/190、Playnite 273/273；render-qa 11 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/0 failed routes/0 fidelity。
- 提交：见 `git log`。真实 Playnite 宿主/DPI 125%/150%/主题/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-POST-TYPOGRAPHY-GEOMETRY-CLOSURE 实施完成

- 来源：`GameSaveCenter_PostTypography_Geometry_Audit_Fix_Prompt.zip`；计划 `docs/ai/UI_POST_TYPOGRAPHY_GEOMETRY_CLOSURE_PLAN.md`；报告 `docs/ai/UI_POST_TYPOGRAPHY_GEOMETRY_CLOSURE_REPORT.md`。
- 修复 Maintenance 诊断“等级”列 72 → 共享 `GscSeverityColumnWidth`（92 DIP），异常审计同列统一引用；`GscRedesignTableStatusPill` 保持内容自适应。
- 全仓固定几何扫描：仅修改确认挤压的等级列；Task/Save/Media/Audit/Process 短列与按钮 MinWidth 核对后保持不变，未全局增高。
- UI Audit 新增 Text-Fit 检测：`FormattedText` 无约束宽度 vs `ActualWidth`，排除 wrap/ellipsis；`TEXT_FIT` 为 MEDIUM 且 audit 直接失败。
- visual-tree exporter 修复：离屏 host `IsVisible` 恒 false → 改用 `Visibility == Visible`；175 个 JSON 全部非空。
- 验证：Release 0 warning/0 error；Core 59/59、Worker 190/190、Playnite 268/268；render-qa 11 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/0 failed routes/0 TEXT-FIT。
- 提交：见 `git log`。真实 Playnite 宿主/DPI 125%/150%/主题/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-FINAL-TYPOGRAPHY-RESPONSIVE-CLOSURE 实施完成

- 来源：`GameSaveCenter_Final_UI_Typography_Prompt.zip`；计划 `docs/ai/UI_TYPOGRAPHY_RESPONSIVE_CLOSURE_PLAN.md`；报告 `docs/ai/UI_TYPOGRAPHY_RESPONSIVE_CLOSURE_REPORT.md`。
- 字体：新增 `GscUiFontFamily`（含 Microsoft YaHei UI fallback）与 `GscCodeFontFamily`；普通 UI 不再硬编码 `Segoe UI Variable Text, Segoe UI`；`Segoe MDL2 Assets` 与 `Consolas` 保留；通用按钮默认 SemiBold → Medium，Primary 保留 SemiBold。
- Settings：Compact/Narrow header 收紧（隐藏长说明/副标题/保存提示、缩小 icon 与 padding）；760×560 正文 viewport 300 DIP。
- Save Compare：窄窗主比较区 MinHeight 240、MaxHeight `max(300, height*0.52)`；1040×700 viewport 234 DIP。
- Compact Inspector：Save/Trainer/Media/Task 五个详情按钮移入表格下方独立操作行，不再覆盖状态文字。
- Media 待归类底栏：`Margin="12,10,12,0"`，与表格内容左边缘对齐并保留窄窗换行。
- 验证：Release 0 warning/0 error；Core 59/59、Worker 190/190、Playnite 266/266；render-qa 11 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/0 失败路由。
- 提交：见 `git log`。真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-FINAL-POLISH-V7.1 实施完成

- 计划：`docs/ai/UI_FINAL_POLISH_PLAN_V7_1.md`；报告：`docs/ai/UI_FINAL_POLISH_REPORT_V7_1.md`。
- 首页活动行改为五列：Icon / Scope / Message(*) / MetaChip / Time；chip 独立横向列组，不再与 scope 上下堆叠；Time 右留白 20 DIP。
- Maintenance 六个指定 clipping 与全量 `POSSIBLE_CLIPPING` 清零；Audit 消息带元素名/父元素/文本片段，并扣除 Margin 误报。
- 验证：Playnite `263/263`；render-qa 11 档 + 56 主题 + 7 Resize 全绿；最终 Audit 0 HIGH/0 MEDIUM/0 失败路由。
- 提交：`702b0d5`、`f6f17a8`。真实 Playnite 主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-FINAL-CLOSURE-V7 实施完成

- 计划：`docs/ai/UI_FINAL_CLOSURE_PLAN_V7.md`；报告：`docs/ai/UI_FINAL_CLOSURE_REPORT_V7.md`。
- Audit 6 的路由错误已根修：维护中心按 `OuterTab + InnerTab + ExpectedPrimaryElement` 生成独立子路由，截图前断言 expected==actual，失败路由 0。
- 宽屏列 Fill：新增共享 `DataGridStarFill`（记录星号权重并按剩余宽度分配），六张 2K 表 ColumnFillRatio 0.18~0.48 → 1.00；MaintenanceProcess 目标游戏 20 → 1549 DIP。
- 纵向 Stretch：Task 根 ScrollViewer 改有限 Grid，Media Inbox/Current 取消 460 上限，Task/Media 主行 fill 1.00。
- Maintenance 表头白块：`GscLastColumnHeader` 去掉 `OverridesDefaultStyle`，统一 `MaintenanceLastColumnHeader`，`HEADER_WHITE_BLOCK=0`。
- Progress：新增 `GscProgressTrackBrush/GscProgressFillBrush`，模板补 `PART_Track`；0/5/25/50/75/100 探针 fill 占比 0.00/0.04/0.24/0.49/0.74/0.99。
- 单行 TextBox：`PART_ContentHost` Stretch + Padding 收口，`SINGLE_LINE_CONTENTHOST_VERTICAL_SCROLL=0`。
- Task narrow：1040×700 outer scroll=0、4.04 行、父子滚动冲突 0。
- 验证：Playnite `263/263`；render-qa 11 档 + 56 主题 + 7 Resize 全绿；最终 Audit 0 HIGH/0 MEDIUM/0 失败路由。
- 提交：`5cd0226`、`58191d5`、`494b402`、`87d0553`、`7eaaacd`。真实 Playnite 主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-FEEDBACK-GLOBAL-ACTIVITY-CHIP-CENTER

- 用户反馈首页“全局活动”中“备份成功”等结果气泡文字不是居中。
- 根因：宽/窄两套 Kind/Result chip 的 TextBlock 未设置水平与文本对齐，Border 被固定列宽撑开后文字默认靠左。
- 修复：`OverviewView.xaml` 四个 chip TextBlock 统一加 `HorizontalAlignment=Center / VerticalAlignment=Center / TextAlignment=Center`；`UiLayoutRegressionTests` 新增居中回归断言。
- 验证：XAML/source 门禁通过；Playnite `263/263`；v6.2 截图刷新到 `artifacts/ui-qa/v6-2-shots/`。
- 提交：`d962b4d`。

## 2026-08-15 UI-TABLE-AND-CHIP-CLOSURE-V6.2 实施完成

- 计划：`docs/ai/UI_TABLE_AND_CHIP_CLOSURE_PLAN_V6_2.md`；报告：`docs/ai/UI_TABLE_AND_CHIP_CLOSURE_REPORT_V6_2.md`。
- Chip 从胶囊改圆角矩形：`GscRedesignContextPill` / `GscRedesignTableStatusPill` CornerRadius 统一 7，ContextPill MinHeight 26。
- 时间列不再贴边：共享 `DataGridCell` Padding `12,8,20,8`，Overview 时间列 `Margin=12,0,20,0`，六列 `40|150|*|96|84|112`。
- SaveCandidate 可信度列改为真实 ProgressBar（Height 8，Maximum 1）+ `P0` 文本；Task/Overview 原有进度条未重复改。
- Maintenance 四个主表取消 460 DIP `MaxHeight` 上限，Device/Process 布局 `VerticalAlignment=Stretch`；2K/4K fill ratio：Diagnostics 0.89/0.93、Device 0.82/0.88、Audit 0.88/0.92、Process 0.90/0.93。
- RenderHarness 新增 `v6-2shots` 模式与 3840×2160 档，脚本 `scripts/capture-v6-2-shots.ps1`，截图 `artifacts/ui-qa/v6-2-shots/`。
- 验证：Release 0 warning/0 error；Playnite `263/263`；render-qa 11 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/8 EXPECTED INFO；source/XAML/WPF 门禁通过。
- 提交：`c58b359`、`6a68a59`。真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-OVERNIGHT-CLOSURE-V6 实施完成

- 计划：`docs/ai/UI_OVERNIGHT_CLOSURE_PLAN_V6.md`；报告：`docs/ai/UI_OVERNIGHT_CLOSURE_REPORT_V6.md`。
- 页面历史改为 Playnite 会话级：首次打开 Overview，同会话恢复，Playnite 重启回 Overview；不再使用持久化 LastWorkspace；Onboarding 首页显示 CTA 显式进入 Maintenance。
- `GscNumericFieldInput` 根修：`PART_ContentHost` 绑定 `VerticalContentAlignment`，数字 1/5/30/120/1440 完整居中显示。
- 全局活动改为轻量六列表格并加 header；Overview 主列/次列 disabled ScrollViewer 改为 Grid；Maintenance Device/Process 与 Media Current 外层 ScrollViewer 改为有限 Grid。
- Task/Media 筛选补语义前缀；Device/Process 主表最小视口 252 DIP；UI Audit 0 HIGH/0 MEDIUM/8 INFO、TRUE_PARENT_CHILD_SCROLL_CONFLICT=0。
- 验证：Release 0 warning/0 error；Playnite `261/261`；render-qa 10 档 + 56 主题 + 7 Resize 全绿；source/XAML/WPF 静态门禁通过。
- v6 截图：`artifacts/ui-qa/v6-shots/`，命令 `scripts/capture-v6-shots.ps1`。
- 提交：`baa8f72`（计划）及实施/文档提交见 `git log`。真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-OVERNIGHT-FIX-V4 实施完成

- 计划：`docs/ai/UI_OVERNIGHT_FIX_PLAN_V4.md`；报告：`docs/ai/UI_OVERNIGHT_FIX_REPORT_V4.md`。
- `GscDisclosureCard` 模板升级为独立 chevron 图标区，全项目 Expander 统一且无内部滚动；维护中心诊断页拆成 `问题列表 / 诊断概览` 二级 Tab，FindingsGrid 独占主工作区。
- 首页全局活动行收敛为 60 DIP，图标垂直居中，时间列 112 DIP；存档备份自动化所有数值输入补齐 label/unit/helper。
- 新增 `OvernightV4SharedTests`、`OvernightV4SaveFormTests`、`OvernightV4MaintenanceTests`；Playnite `255/255`，render-qa 10 档 + 56 主题 + 7 Resize 全绿，UI Audit 0 HIGH/0 MEDIUM/39 INFO/0 失败路由。
- v4 截图：`artifacts/ui-qa/v4-shots/`，命令 `scripts/capture-v4-shots.ps1`。
- 用户后续反馈修复：折叠 header 文字改为与图标垂直居中；存档备份自动化数字输入改为水平/垂直居中显示，框尺寸不变；提交 `fc86ecc`。
- 提交：`3015182`（计划）、`5131e4d`（共享样式+首页）、`0201615`（存档表单）、`5196f4a`（维护中心）、`fc86ecc`（对齐修复）。真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-14 UI-VISUAL-REWORK-V3 实施完成

- 图一：Overview 当前游戏卡三个按钮统一 `GscWpfUiCompactButton` 几何（38/88 DIP），Primary 只改 `Appearance`，标准宽度同排、窄窗自然 2+1。
- 图二：最近 30 天动作按钮与统计 chips 分层；统计 chips 中性/警告配色；所有 Expander/Disclosure 统一到 `GscDisclosureCard`，去掉尾部 `>`，Chevron 独立图标区并居中。
- 图三：全局活动改为轻量表四列（Icon / Main / Chips / Time 112 DIP），行高 64 DIP、hover 分隔线；窄窗 chips 移到摘要下方。
- 图四：Save 当前存档规则状态合并为一行 badge，按 `SelectedGame.HealthState` 着色（风险=橙、正常=绿、待确认/中性=灰蓝、运行=蓝），三按钮统一 38/100 DIP 紧凑几何；卡片 padding 收敛为 12/10。
- 图五：Maintenance Diagnostics 环境卡压缩为摘要行，首次环境检查与低频维护进入 `GscDisclosureCard`；FindingsGrid 五列收敛为 72/120/160/*180/140，1040×700 下五列表头完整且首屏可见。
- 补强：Media 来源表单、Save 策略模板、Task 更多筛选也统一到 `GscDisclosureCard`，`GscExpander` 不再被页面引用；Maintenance 两个主 Disclosure 内容改为内部有限滚动（`EnvironmentCheckDisclosureScroller` / `MaintenanceActionsDisclosureScroller`），展开后不会挤压 FindingsGrid。
- 按钮对齐探针：render-qa 新增 Overview/Save 三按钮 Y 坐标与高度差检查；探针抓到 1536×864 下“查看需关注项”换行，已将 Hero/当前游戏列从 1.1:0.9 调整为 1:1，10 档尺寸全部同排同高。
- 功能保真：REMOVE=0；`DetectPathsCommand/ValidateCommand/LoadDetailsCommand`、全部诊断/维护命令、FindingsGrid 5 列与 EnvironmentCheckItems 均保留；GamePicker HARD LOCK 未改。
- 验证：Release 构建 0 warning/0 error；Core `59/59`、Worker `190/190`、Playnite `253/253`；`check-xaml.ps1`、`validate-source.py`、WPF 静态审查（0 error）通过；render-qa 10 档 + 56 主题场景 + 7 Resize 全绿；UI Audit 0 HIGH / 0 MEDIUM / 32 INFO / 0 失败路由。
- 截图证据：`artifacts/ui-qa/v3-shots/`（10 张标准/窄窗/展开态）、`artifacts/ui-qa/v3-final/`；Audit 汇总：`artifacts/ui-audit/v3-final/AUDIT_SUMMARY.md`。截图命令：`scripts/capture-v3-shots.ps1`。
- 提交：`5c3bdae`（计划）、`9ee3660`（Overview）、`e8b8c31`（Save/Maintenance），后续补强与文档将在最终提交中记录。真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-14 UI-VISUAL-REWORK-V3 计划

- 用户提供 `GameSaveCenter_UI_Design_and_Prompt_Pack_v3.zip`，要求按 5 张截图定向重构，不继续泛化优化。
- 已生成 `docs/ai/UI_VISUAL_REWORK_PLAN_V3.md`：逐图列出问题、受影响 x:Name、当前结构、拟改结构、KEEP/MOVE/RESTYLE、保留 Binding、预期标准/窄窗样式与验收方法。
- 按 v3 要求先提交计划，随后按 图一 → 图二/Disclosure → 图三 → 图四 → 图五 → 颜色/Audit → 文档 的顺序实施。

## 2026-08-14 UI-VISUAL-CORRECTION-V2 真实宿主 reload 验证

- `scripts/dev-install-run.ps1 -Configuration Release` 一键构建安装成功并启动 Playnite：Release 0 warning / 0 error，Core `59/59`、Worker `190/190`、Playnite `250/250`。
- 真实宿主证据：`playnite.log` `20:39:57` 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`；`extensions.log` `20:39:59` 记录 `GameSaveCenter 0.6.70.0 loaded`；Worker PID `36980` 从当前扩展目录运行；`20:39` 后无 ERROR/Exception/crash。
- 真实 Playnite 主题视觉、DPI 与连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-14 UI-VISUAL-CORRECTION-V2 实施完成

- Overview：根页成为唯一纵向滚动；主列/右列/风险卡取消滚动职责；风险卡去内滚与死空间；最近保护明细使用 `GscDisclosureCardExpander`；全局活动按 Tag 切换宽/紧凑行。
- Save：`SaveCurrentRuleCard` 压成紧凑四列，三按钮等高/等宽，窄窗动作移到第二行；`SaveCandidateGrid` 最小视口提到 252 DIP。
- Maintenance：Diagnostics 外层改 Grid，去掉 `Outer Scroll > FindingsGrid` 父子双滚动；环境明细、低频维护、运行状态收进 Disclosure；Audit 改为“发现的问题 / 审计记录”二级切换，同一时刻只显示一张主表。
- Audit 新增 v2 断言：OV-001/002/003/005、SAVE-001/002、MAINT-001/002/003；最终 UI Audit HIGH `0`、MEDIUM `0`、运行时警告 `33`、161 快照、失败路由 `0`。
- render-qa 10 档尺寸 + 56 主题场景 + 7 Resize 恢复全部通过；Core `59/59`、Worker `190/190`、Playnite `250/250`。

## 2026-08-14 UI-VISUAL-CORRECTION-V2 计划

- 用户提供 `GameSaveCenter_UI_Visual_Correction_Pack_v2.zip`，本轮不推翻上一轮重构，只关闭截图/Audit 仍暴露的问题。
- 已生成 `docs/ai/UI_VISUAL_CORRECTION_PLAN_V2.md`，覆盖 Overview 单滚动、风险卡去内滚/去空白、Disclosure 视觉、全局活动响应式行、Save 当前存档规则卡、Maintenance Diagnostics 去双滚动、Audit 二级切换与新增 Audit 断言。
- 按包要求先提交计划，随后按 P2 → P0/P1/P3 → P4 → P5 → P6 → P7 顺序实施。

## 2026-08-14 UI-REFACTOR-V1 真实宿主 reload 验证

- `scripts/dev-install-run.ps1 -Configuration Release` 一键构建安装成功：Release 0 warning / 0 error，Core `59/59`、Worker `190/190`、Playnite `250/250`，扩展安装到 `C:\Users\lopmatu\AppData\Roaming\Playnite\Extensions\GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec` 并启动 Playnite。
- 真实宿主证据：`playnite.log` `18:15:48` 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`；`extensions.log` `18:15:50` 记录 `GameSaveCenter 0.6.70.0 loaded`；Worker PID `11056` 从当前扩展目录运行；`18:10` 后 `playnite.log` 与 `extensions.log` 无 ERROR/Exception/crash。
- 真实 Playnite 主题视觉、DPI 与连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-14 UI-REFACTOR-V1 验收审计文档

- 新增 `docs/ai/UI_REFACTOR_ACCEPTANCE_AUDIT.md`，把实施包 `06_ACCEPTANCE_CHECKLIST.md` 逐项映射到仓库证据，并区分 `AUTO VERIFIED` 与 `MANUAL QA REQUIRED`。
- Inspector 恢复修复后重跑最终 UI Audit：HIGH `0`、MEDIUM `0`、运行时警告 `39`、161 快照、失败路由 `0`。

## 2026-08-14 UI-REFACTOR-V1 Resize 恢复探针与 Inspector 恢复修复

- `render-qa` 新增 Resize transition QA：同一视图实例按 2560×1440 → 1100×720 → 2560×1440 依次布局，快照 DataGrid/ListBox/命名 ScrollViewer 的尺寸、可见性与滚动条，断言回到大窗后几何恢复。
- 探针抓到并修复真实缺陷：Save/Task/Trainer 的 Inspector 在宽→窄→宽后停留在窄窗收起状态；现在非 compact 分支会按当前选择显式恢复 Inspector 可见性。
- 新增 3 条回归测试（Save/Task/Trainer `InspectorRestoresAfterCompactResize`），并让旧“列释放”测试在验证宽窗时设置真实选中项、在 compact 验证 Auto 行前走真实详情切换。
- `render-qa` 10 档尺寸 + 56 主题场景 + 7 视图 Resize 恢复全部通过；Core `59/59`、Worker `190/190`、Playnite `250/250`。

## 2026-08-14 UI-REFACTOR-V1 页面级横向溢出门禁

- render-qa 与主题 QA 现在都会检查页面级 `*ScrollSurface` 与 `SettingsScroller`：`hbar` 必须为 `Disabled` 且无横向溢出；DataGrid 内部 `hbar=Auto` 的列滚动仍被允许。
- 报告行新增 `hbar` 与 `hscrollable` 探针；10 档尺寸矩阵 + 56 个 Light/Dark 主题场景全部通过，页面级横向溢出为 0。
- Release 0 警告/0 错误；Core `59/59`、Worker `190/190`、Playnite `247/247`；source/XAML/WPF 门禁通过。
- 真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-14 UI-REFACTOR-V1 强制 Light/Dark 主题 QA

- RenderHarness 新增 `RunThemeQa`：对 7 个工作区 × 1040×700 / 1100×720 / 1366×768 / 2560×1440 × Light/Dark 强制主题渲染，共 56 个离屏场景。
- 通过 `AdaptiveThemePaletteFactory.Create` 与 `ApplyRuntimeThemeResources` 复用生产主题桥接；`InternalsVisibleTo` 仅开放给 RenderHarness。
- 门禁：56 个场景全部 OK；调色板断言（Light 主文字黑、Dark 主文字白）与主表/页面滚动面视口检查通过；像素采样 Light 背景约 `239,240,243`、Dark 约 `54,57,67`。
- `render-qa` 现包含 10 档尺寸矩阵 + 主题 QA；Release 0 警告/0 错误，Core `59/59`、Worker `190/190`、Playnite `247/247`。
- 真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`；本轮没有 REMOVE，GamePicker 与 Dashboard 锁定区域未改。

## 2026-08-14 UI-REFACTOR-V1 扩档验证（2K / 1100×720 / DPI 等效）

- `render-qa` 窗口矩阵从 5 档扩到 10 档：1040×700、1100×720、1280×720、1366×768、1536×864（1080p @125%）、1600×900、1707×960（1440p @150%）、1920×1080、2048×1152（1440p @125%）、2560×1440；10 档全部通过。
- UI Audit 新增 `2k`（2560×1440）与 `narrow-1100`（1100×720）尺寸，快照 115→161；修复 Audit 向工作区 `ApplyResponsiveLayout` 传内容高度而非窗口高度的夹具偏差，与生产 Dashboard 和 render-qa 对齐。
- 扩档后最终审计：HIGH `0`、MEDIUM `0`、运行时警告 39、失败路由 `0`；2560×1440 主表视口与页面滚动正常，1100×720 维护设备/进程表按窗口高度预算为 360 DIP，而不是错误的 240 DIP。
- 真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-14 UI-REFACTOR-V1 Phase 8 收口（Overview 92 DIP 清零）

- 定位到 After Audit 剩余 MEDIUM 的实际对象是“当前游戏”卡片内 3 个操作按钮的未命名 `WrapPanel`（按钮 38 DIP + 底部 8 DIP，换行两行后总高 92 DIP），不是顶部 `OverviewHomeToolbar` Border。
- `OverviewView.xaml` 将该行 3 个按钮底部边距从 8 收到 4 DIP（`RESTYLE`，命令/Binding/可达性不变）；同时保留顶部工作台工具栏 `Padding="14,10"` 收口，render-qa 实测工具栏 1040×700 91→79 DIP、1280×720 75→63 DIP。
- 最终 UI Audit：HIGH `0`、MEDIUM `0`、运行时警告 42→40、失败路由 `0`；render-qa 五种窗口全绿；Core `59/59`、Worker `190/190`、Playnite `247/247`；source/XAML/WPF 门禁 0 errors。
- 真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`；本轮没有 REMOVE，GamePicker 与 Dashboard 锁定区域未改。

## 2026-08-14 UI-REFACTOR-V1 Phase 8 最终回归

- 最终 Release 隔离构建 0 警告/0 错误；Core `59/59`、Worker `190/190`、Playnite `247/247`；source/XAML/WPF/render-qa 全绿。
- 重跑 `scripts/capture-ui-audit.ps1`：HIGH 从 10 项清零，MEDIUM 从 4 项降到 2 项（Overview 工具栏高度 92 DIP），运行时警告 62 → 42，失败路由 0。
- 最后补修 Maintenance 设备/进程 narrow 表最小视口到 280 DIP，使 Audit HIGH 归零。
- `UI_REFACTOR_FIDELITY_PLAN.md` 已回填 Phase 0～8 状态与 After Audit。
- 真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`；离屏 Audit 与 render-qa 不代表真实宿主人工验收。

## 2026-08-14 UI-REFACTOR-V1 Phase 7 设置轻量统一

- `DesignTokens.xaml` 新增共享 `GscSettingsFieldColumnWidth`（150 DIP），Settings 常规与目录页 8 个字段行全部改为引用该 token，消除页面级硬编码列宽。
- 未改设置分区、字段、绑定、保存语义、响应式断点或主题桥接；五个 Tab 与所有业务设置保持原状。
- 新增资源字典回归断言：`GscSettingsFieldColumnWidth` 必须可解析为 `GridLength`。
- Gate：Release 0 警告/0 错误，Core `59/59`、Worker `190/190`、Playnite `247/247`，source/XAML/WPF/render-qa 全绿。

## 2026-08-14 UI-REFACTOR-V1 Phase 6 维护中心 Maintenance

- 诊断操作卡主行收敛为 5 个常用按钮（复制/导出诊断、复制/导出健康报告、完整性自检）；目录与日志、完整性/自愈/协调、元数据灾备进入 3 个共享 Expander，安全模式、批量路径迁移、重建/协调等 16 个命令全部保留。
- 异常与审计页的审计日志表视口从 96/140 提升到 236/280 DIP；render-qa 实测 1040×700 审计日志表 280 DIP（6 行），不再只有 1.6～1.9 行。
- FindingsGrid、设备表、进程映射表保持 350 DIP 主视口与内部滚动；保留策略四块业务与设备/进程全部命令未改；无 REMOVE。
- 更新两条旧回归断言以匹配新结构：Inspector 内不允许 Expander；诊断主行 5 个按钮 + 卡片共 16 个命令。
- Gate：Release 0 警告/0 错误，Core `59/59`、Worker `190/190`、Playnite `247/247`，source/XAML/WPF/render-qa 全绿。

## 2026-08-14 UI-REFACTOR-V1 Phase 5 任务中心 Task

- 紧凑面板下“游戏筛选”响应式移入“更多筛选”Expander（宽屏自动回到主筛选行），搜索/状态/类型筛选保持主行；四筛选全部可达。
- 任务详情 Inspector 在 Compact/Narrow 默认收起为“查看任务详情 / 收起任务详情”按钮；复制详情、安全重试、取消任务仍可展开访问。
- 操作行不再在 1040×700 竖向膨胀：仅 <520 DIP 才竖向堆叠；常见窄窗保持横向。
- 任务摘要卡在 520–899 DIP 使用 4 列紧凑单行，任务表 1040×700 视口 252 DIP、1280×720 259 DIP；页面 extent 从约 867 降到 536（1040）。
- 未改 ViewModel、Worker、IPC、任务筛选语义、DataGrid 六列/Item ScrollUnit/虚拟化；无 REMOVE。
- 新增 2 条回归测试：Task 窄窗 Inspector 默认收起/可打开；游戏筛选在 compact 进入更多筛选、wide 回到主行。
- Gate：Release 0 警告/0 错误，Core `59/59`、Worker `190/190`、Playnite `247/247`，source/XAML/WPF/render-qa 全绿。

## 2026-08-14 UI-REFACTOR-V1 Phase 4b 媒体中心 Media

- 当前游戏媒体页在 Compact/Narrow 下默认收起媒体 Inspector，改为表内“查看媒体详情 / 收起媒体详情”按钮；预览、收藏、备注、保存、打开目录、重新归类与批量操作仍可通过按钮展开访问。
- 来源规则页“添加截图或录像来源”表单默认折叠到共享 Expander；目录、文件模式、共享开关与添加来源命令仍可展开访问，规则列表继续获得 Auto/* 主行。
- 待归类 DataGrid、媒体虚拟化/Recycling、缩略图异步加载、搜索/筛选绑定与全部命令未改。
- render-qa 实测：1040×700 当前媒体列表保持 350 DIP，Inspector 收起后页面 extent 从约 891 降至 489；1280×720 同样保持 360 DIP。
- 新增 2 条回归测试：Media 窄窗 Inspector 默认收起/可打开、来源表单折叠但字段可达。
- Gate：Release 0 警告/0 错误，Core `59/59`、Worker `190/190`、Playnite `245/245`，source/XAML/WPF/render-qa 全绿。

## 2026-08-14 UI-REFACTOR-V1 Phase 4a 修改器中心 Trainer

- 已绑定工具页在 Compact/Narrow 下默认收起工具设置 Inspector，改为表内“查看工具详情 / 收起工具详情”按钮；启动、保存、打开目录、重新定位、解除绑定与全部编辑字段仍可通过按钮展开访问。
- `TrainerToolsList` 增加命名与选择事件，代码补防重入协调；虚拟化、Recycling、拖拽导入、FLiNG 搜索、可下载版本与导入确认流程未改。
- render-qa 实测：1040×700 工具列表视口 236 DIP、1280×720 276 DIP（修复首轮 234 DIP 门禁失败后达标）。
- 新增 1 条回归测试：窄窗默认收起工具 Inspector、详情按钮可打开/收起。
- Gate：Release 0 警告/0 错误，Core `59/59`、Worker `190/190`、Playnite `243/243`，source/XAML/WPF/render-qa 全绿。

## 2026-08-14 UI-REFACTOR-V1 Phase 3 存档中心 Save Center

- 历史版本与路径候选在 Compact/Narrow 下默认收起 Inspector，改为表内“查看版本详情 / 查看候选详情”按钮；点击后展开同一套 Inspector（备注/锁定/恢复/比较/恢复就绪与接受/忽略/重新扫描），功能零损失。
- 路径与校验页头部从两行压成单行：当前规则/校验状态在左，扫描/校验/刷新在右，为主表让出高度。
- 备份策略页模板区放入共享 `GscExpander`（默认折叠），模板新建/保存/应用/删除命令与全部参数仍可展开访问。
- 比较与保留继续保留并排/堆叠双区，`CompareBackup`、`PreviewRetention` 与所有差异/保留信息未改。
- 未改 GamePicker、ViewModel、Worker、IPC、DataGrid 列/虚拟化/稳定滚动策略；无 REMOVE。
- render-qa 实测：1040×700 历史表高度 385 DIP（原 236）、候选表 254 DIP（原 236）；1280×720 历史 405 DIP、候选 274 DIP；Inspector 收起时主表内部滚动保持 Auto + Item ScrollUnit。
- 新增 1 条回归测试：窄窗默认收起 Inspector、详情按钮可打开/收起；Gate：Release 0 警告/0 错误，Core `59/59`、Worker `190/190`、Playnite `242/242`，source/XAML/WPF/render-qa 全绿。

## 2026-08-14 UI-REFACTOR-V1 Phase 2 首页 Overview

- 六项 Snapshot 指标由 3×2 等权大卡改为响应式紧凑 Summary Strip：宽工作区 6 列、中等 3 列、窄 2 列；卡内 Padding/圆角/数字字号收紧，六个真实计数、比例条与说明全部保留。
- 最近 30 天保护明细放入共享 `GscExpander`（默认折叠），汇总、未识别/需处理胶囊、查看与启用所选保护命令保持常驻；`RecentProtection.Items` 与选择语义不变。
- 全局活动时间线从 3 列 Auto 挤压改为稳定 4 列：图标 / 内容（MinWidth 220）/ 分类 / 结果 / 时间分离；`KindDisplay / ResultDisplay / CreatedDisplay` 全部保留。
- 未改 Dashboard GamePicker、首页“今日工作台 / TODAY / 当前游戏”结构、任何 ViewModel/命令/Binding、DataGrid/ListBox 虚拟化策略；无 REMOVE。
- 新增 3 条回归测试：摘要条响应式列数、保护明细可折叠且项不丢失、全局活动稳定四列。
- Gate：隔离 Release 构建 0 警告/0 错误；Core `59/59`、Worker `190/190`、Playnite `241/241`；`validate-source.py`、WPF UI validator 0 errors、五种窗口 `render-qa OK`；`git diff --check` 通过。

## 2026-08-14 UI-REFACTOR-V1 Phase 1 共享布局基础

- 在 `Themes/Redesign.xaml` 新增共享基础件：`GscInternalTabControl` / `GscInternalTabItem`（内部二级 Tab，供保留策略、异常审计、比较保留使用）、`GscToolbarActionRow`（工具栏动作行）、`GscToolbarOverflowButton`（溢出/更多按钮）。
- 在 `WpfUiResourceDictionaryTests` 增加四个新资源 key 的解析断言，确保后续页面使用这些共享样式时不会静默回退。
- 未修改任何 View 页面、GamePicker、ViewModel、Worker、IPC 或业务逻辑；本轮只建立后续 Phase 2～8 可复用的共享样式。
- Gate：隔离 Release 构建 0 警告/0 错误；Core `59/59`、Worker `190/190`、Playnite `238/238`；`validate-source.py`、WPF UI validator 0 errors、五种窗口 `render-qa OK`；`git diff --check` 通过。

## 2026-08-14 UI-REFACTOR-V1 Phase 0 基线、保真计划与 Gate

- 读取 `GameSaveCenter_UI_Refactor_Implementation_Pack_v1` 全部正文、UI Audit（commit `4ab44fe`）与 WPF Demo v6.1；确认事实来源优先级：生产 main > UI Audit > 实施包 > Demo > 旧布局。
- 生成 `docs/ai/UI_REFACTOR_FIDELITY_PLAN.md`：锁定 GamePicker、一级导航、首页“今日工作台 / TODAY / 当前游戏”、修改器导入流程、Settings 分区与 dev-probe；覆盖 92 条命令、43 个 DataGrid 列、30 个 ScrollViewer、143 个条件 UI 的保真策略。
- Phase 0 Gate：隔离 Release 构建 0 警告 / 0 错误；Core `59/59`、Worker `190/190`、Playnite `238/238`；`validate-source.py` 通过；WPF UI validator 0 errors（17 warnings 为现有 StackPanel/滚动上下文提示）；五种窗口 `render-qa OK`；`git diff --check` 通过。
- 后续 Phase 1～8 按计划逐阶段独立提交并 push，不跨阶段混改；出现 DataGrid 滚动/虚拟化回归先停后续阶段。

## 2026-08-14 UI-AUDIT-001 开发专用 UI 自动审计与整页滚动快照

- 新增 `scripts/capture-ui-audit.ps1` 与根目录 `GameSaveCenter-UI-Audit.cmd`，一条命令产出 `artifacts/ui-audit/` 与 `artifacts/GameSaveCenter-ui-audit.zip`。
- 静态盘点自动扫描 `Views/Settings` 下真实 XAML，生成 `UI_ROUTE_MAP.md/json`、`UI_MANIFEST.md/json`、`UI_FIDELITY_MATRIX.md`；Dashboard、Workspace、设置页、未来新增 UserControl/Tab 都会被自动发现。
- 运行时审计复用 `GameSaveCenter.RenderHarness` 的生产视图与假数据，输出每页每尺寸的 `visual-tree/*.json`、`layout/*.json`、`LAYOUT_REPORT.md`、`AUDIT_SUMMARY.md`。
- 整页滚动截图覆盖所有有滚动条的区域：页面级 `-full-*.png` 直接渲染完整内容，DataGrid/ListBox 按像素换算逐段滚动拼接成 `-scroll-*.png`，满足“从头到底都能截进去”的要求。
- 覆盖 `maximized`（实际 WorkArea）、`wide`、`standard`、`compact`、`narrow` 五种逻辑窗口；截图与文本报告统一做用户目录脱敏，并明确提示位图内容需自行复核。
- 验证：Core `59/59`、Worker `190/190`、Playnite `238/238`；`validate-source.py`、WPF UI validator 0 errors、`render-qa` 全绿；最终审计 `0` 失败路由、`0` 零字节 PNG/JSON；ZIP 约 16.2 MiB。
- Commit：见当前 `git log -1`。

## 2026-08-14 文档：README 与旧文档版本号同步到 0.6.70

- README 当前版本显式标注 `0.6.70-development-preview`（extension.yaml / DLL `0.6.70.0`）。
- 同步 `docs/design/APPLE_UI_GUIDE.md`、`docs/KNOWN_ISSUES.md`、`docs/CODEX_FULL_HANDOFF.md` 中仍停留在 0.6.22/0.6.46/0.6.55 的版本号。

## 2026-08-14 崩溃修复与 Metadata 原子回滚

- 真实 Playnite 09:58:04 崩溃根因：`GscWorkspaceStatePresenter` 模板中重试按钮使用普通 `Button`，却应用 `GscWpfUiActionButton`（TargetType 为 `ui:Button`），切换存档页渲染状态控件时抛 `XamlParseException: “Button”TargetType 与元素“Button”的类型不匹配`。已改为 `ui:Button` 并增加源码回归断言。
- `MetadataRestoreCoordinator`：Playnite 侧在 Worker 恢复前用 detached `GameSaveCenterSettings.ValidatePortableJson` 预校验插件设置；恢复后导入/保存/应用任一失败时，先恢复旧插件设置，再调用 `metadata.restore.rollback` 回滚 DB 与 Worker 设置，失败才进入 `MANUAL_INTERVENTION_REQUIRED`。
- Worker `MetadataBackupService`：恢复前副本改为 `VACUUM INTO` 一致性快照（不再直接复制可能缺 WAL 的活库）；恢复/回滚后调用 `WorkerOptions.ReloadPersistedSettings()`，避免旧内存选项覆盖刚恢复的 worker-settings.json。
- 故障注入覆盖：Worker 测试验证 B 恢复成功后调用 rollback 可回到 A（DB + Worker settings）；Playnite 协调器测试验证 Plugin 保存故意失败后旧 Plugin settings 被恢复、Worker rollback 被调用、失败时进入人工介入。
- 最终 Gate：Release 0 warnings / 0 errors；Core `59/59`、Worker `190/190`、Playnite `235/235`；source/XAML/WPF 静态门禁、五种窗口 `render-qa`、fault-injection 与 1000 轮 soak 全绿。
- 真实宿主验证：`dev-install-run.ps1` 后 `playnite.log` 10:18:34 记录插件加载，`extensions.log` 10:18:36 记录 `GameSaveCenter 0.6.70.0 loaded`，`worker-launch.log` 10:18:40 记录 `Application started`；10:18 后无新崩溃日志。
- Commit：`a417b7c`（崩溃修复）、`13f21a5`（Metadata 原子回滚）。

## 2026-08-14 Final Code Gap Closure 四项缺口闭合

- P0 `REPOSITORY-REBUILD-001` 升级：`RepositoryRebuildService` 改为从磁盘 ZIP/Manifest 扫描，空/新 SQLite 也能按 Ludusavi 目录重建 `recovered-*` 占位游戏与备份历史；不依赖原 Game Match，不猜 Parent，保留 Locked/PreRestore 旧索引，二次重建幂等。
- P1 `METADATA-BACKUP-001` 升级：灾备包新增 `settings/plugin-settings.json`（复用 `ExportPortableJson/ImportPortableJson`），预览返回插件设置哈希与 JSON，恢复后由 Playnite 侧导入、保存并同步 Worker；导入失败时回滚恢复前插件设置。
- P1/P2 `UI-STATES-001` 覆盖扩展：存档历史 Loading、修改器工具 Loading/Empty、媒体 Worker Offline、维护云端 Degraded 均接入共享 `WorkspaceStatePresenter`；状态来自真实快照与集合，不模拟业务成功。
- P1 `LOCAL-MIRROR-001` 升级：Local Mirror 从“同大小即跳过”改为 SHA256 内容校验；已存在文件同大小但内容不同会重新复制并在复制后再次校验。
- 最终 Gate：Release 0 warnings / 0 errors；Core `59/59`、Worker `190/190`、Playnite `235/235`；`validate-source.py`、XAML/WPF 静态门禁与五种窗口 `render-qa` 全绿；`fault-injection-test.ps1` 与 `soak-test.ps1 -Iterations 1000` 通过。
- 真实宿主验证：`dev-install-run.ps1 -Configuration Release` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 09:43:56 记录插件加载，`extensions.log` 09:43:58 记录 `GameSaveCenter 0.6.70.0 loaded`，`worker-launch.log` 09:44:03 记录 `Application started`。
- Commit：`e58714c`、`0bcdce2`、`84f74fc`、`5a12d1e`。
- 下一项：按 `docs/ai/FINAL_MANUAL_QA_CHECKLIST.md` 执行真实场景人工验收；整体 Epic 仍为 `PARTIALLY COMPLETED / MANUAL QA REQUIRED`。

## 2026-08-14 Layer C 与最终 Epic 审计收口

- 已生成 `docs/ai/PRODUCT_HARDENING_LAYER_C_AUDIT.md`：11 项逐项列出生产代码、测试、AUTO VERIFIED、MANUAL QA REQUIRED 与 Commit；结论为 Layer C 11/11 交付。
- 已生成 `docs/ai/PRODUCT_HARDENING_EPIC_FINAL_AUDIT.md`：Layer A 14 项、A-HARDEN-001/002/003、Layer B 13 项、Layer C 11 项逐项表格，包含状态、代码/测试/CI 证据、人工验收和 Commit。
- 最终 Gate：Release 0 warnings / 0 errors；Core `59/59`、Worker `183/183`、Playnite `231/231`；`validate-source.py`、XAML/WPF 静态门禁与五种窗口 `render-qa` 全绿。
- 独立复核：`scripts/fault-injection-test.ps1 -Configuration Release` 通过；`scripts/soak-test.ps1 -Configuration Release -Iterations 1000` 通过。
- 真实宿主验证：`dev-install-run.ps1 -Configuration Release` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 04:02:36 记录插件加载，`extensions.log` 04:02:37 记录 `GameSaveCenter 0.6.70.0 loaded`，`worker-launch.log` 04:02:42 记录 `Application started`。
- 整体状态：`PARTIALLY COMPLETED / MANUAL QA REQUIRED`；真实 Restore/Undo、Rclone、双设备、外置镜像、启动项、反作弊、主题/DPI/连续缩放与 1000+ 游戏库仍未人工验收。
- Commit：`039bc68`（Layer C/Epic 审计）；故障注入与 Soak 独立复核补记随后续文档提交。
- 下一项：等待真实场景人工验收；不再主动扩张新功能。

## 2026-08-14 MAINTENANCE-REPORT-001 用户可读维护健康报告

- Task ID：`MAINTENANCE-REPORT-001`。
- 实现内容：新增 IPC `maintenance.report.get` 与 Worker `MaintenanceReportService`，从 SQLite 计数、完整性自检、存储分析、本地镜像状态聚合用户可读健康文本；维护中心“诊断”操作带新增“复制健康报告/导出健康报告”，导出支持 TXT/Markdown。报告与开发者诊断 ZIP 明确区分，不包含日志、原始数据库或凭据。
- 主要修改文件：`MaintenanceReportDtos.cs`、`MaintenanceReportService.cs`、`MessageTypes.cs`、`WorkerDtos.cs`、`IpcRequestDispatcher.cs`、`Program.cs`、`DashboardViewModel.cs`、`MaintenanceView.xaml`、`MaintenanceReportServiceTests.cs`、`MaintenanceReportSourceTests.cs`、`ProtocolCompatibilityTests.cs`、`WpfUiResourceDictionaryTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `59/59`、Worker `183/183`、Playnite `231/231`；`validate-source.py`、XAML/WPF 静态门禁与五种窗口 `render-qa` 全绿。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 03:43:26 记录插件加载，`worker-launch.log` 03:43:32 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实维护中心点击复制/导出健康报告、长文本显示与内容完整性复核。
- Commit：`9d465d2`。
- 下一项：生成 Layer C Audit，随后生成最终 Epic Audit。

## 2026-08-14 SETTINGS-VALIDATION-001 设置即时验证摘要

- Task ID：`SETTINGS-VALIDATION-001`。
- 实现内容：设置页标题区新增 `SettingsValidationSummary`，在文本框、下拉框与复选框变化时调用既有 `VerifySettings` 并内联展示最多 4 条错误；错误清除后自动隐藏，补充 `AutomationProperties.Name="设置验证错误"`。验证结果仍然与 Playnite 保存流程共用同一套规则。
- 主要修改文件：`GameSaveCenterSettingsView.xaml`、`GameSaveCenterSettingsView.xaml.cs`、`SettingsValidationSourceTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `59/59`、Worker `182/182`、Playnite `230/230`；`validate-source.py`、XAML/WPF 静态门禁与五种窗口 `render-qa` 全绿。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 03:43:26 记录插件加载，`worker-launch.log` 03:43:32 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实修改路径/数值后立即查看内联错误、修正后消失，以及保存时仍能阻止严重错误。
- Commit：`4614bb8`。
- 下一项：Layer C `MAINTENANCE-REPORT-001` 用户可读健康报告。

## 2026-08-14 UI-STATES-001 统一页面状态控件

- Task ID：`UI-STATES-001`。
- 实现内容：新增共享 `WorkspaceStatePresenter`，统一 Loading/Empty/Error/Degraded/Offline/Disabled 六种状态；共享模板展示状态图标、状态文案、标题、说明和可选重试按钮（无命令时自动隐藏）。Overview 全局活动与 Task 空状态已接入共享控件，页面空状态回归测试同时接受 `GscEmptyStateText` 与 `GscWorkspaceStatePresenter`。
- 主要修改文件：`WorkspaceStatePresenter.cs`、`Redesign.xaml`、`OverviewView.xaml`、`TaskCenterView.xaml`、`WorkspaceStateSourceTests.cs`、`WpfUiResourceDictionaryTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `59/59`、Worker `182/182`、Playnite `229/229`；`validate-source.py`、XAML/WPF 静态门禁与五种窗口 `render-qa` 全绿。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 03:36:34 记录插件加载，`worker-launch.log` 03:36:40 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实 Loading/Error/Degraded/Offline/Disabled 状态人工切换与重试按钮复核。
- Commit：`f21cba4`。
- 下一项：Layer C `SETTINGS-VALIDATION-001` 设置即时验证。

## 2026-08-14 ACCESSIBILITY-001 键盘快捷搜索、焦点与自动化名称

- Task ID：`ACCESSIBILITY-001`。
- 实现内容：Dashboard 全局 `PreviewKeyDown` 增加 `Ctrl+F`：首页/存档聚焦游戏搜索框，修改器聚焦 FLiNG 搜索框，任务聚焦任务搜索框，媒体聚焦媒体搜索框，维护聚焦进程映射输入框，并自动全选当前内容。任务、媒体、FLiNG 与游戏搜索框补充 `AutomationProperties.Name`；既有共享 `GscSharedFocusVisual` 与 `SystemParameters.HighContrast` 玻璃降级保持生效。
- 主要修改文件：`DashboardView.xaml.cs`、`DashboardView.xaml`、`TaskCenterView.xaml`、`MediaCenterView.xaml`、`TrainerCenterView.xaml`、`AccessibilitySourceTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `59/59`、Worker `182/182`、Playnite `227/227`；`validate-source.py`、XAML/WPF 静态门禁与五种窗口 `render-qa` 全绿。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 03:25:12 记录插件加载，`worker-launch.log` 03:25:18 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实 Playnite 中键盘-only 使用 `Ctrl+F`、Tab/方向键、Esc，以及 Narrator/Accessibility Insights、高对比度与 200% DPI 复核。
- Commit：`c907983`。
- 下一项：Layer C `UI-STATES-001` 统一 Loading/Empty/Error/Degraded/Offline/Disabled 页面状态。

## 2026-08-14 UI-STATE-001 合理 UI 状态持久化

- Task ID：`UI-STATE-001`。
- 实现内容：设置新增 `LastWorkspace`、任务状态/游戏/类型筛选、任务搜索、媒体筛选与媒体搜索持久化字段；DashboardViewModel 启动时恢复上次 Workspace 与筛选，变更通过新增的 `DebouncedRefresh.Schedule()` 500ms 防抖保存。运行中游戏优先与上次选择恢复继续使用既有 GamePicker 持久化；不保存 Loading/Busy/Error 等瞬态。
- 主要修改文件：`GameSaveCenterSettings.cs`、`DashboardViewModel.cs`、`DebouncedRefresh.cs`、`PortableSettingsTests.cs`、`UiStatePersistenceSourceTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `59/59`、Worker `182/182`、Playnite `225/225`；`validate-source.py` 与源码门禁通过（本阶段无 XAML 改动，不重跑 render-qa）。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 03:16:07 记录插件加载，`worker-launch.log` 03:16:13 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实切换 Workspace、修改任务/媒体筛选与搜索后重启 Playnite，核对恢复结果；运行中游戏优先语义人工复核。
- Commit：`e4513cb`。
- 下一项：Layer C `ACCESSIBILITY-001` 键盘、高对比度与焦点统一。

## 2026-08-14 DRAGDROP-001 修改器中心拖拽导入

- Task ID：`DRAGDROP-001`。
- 实现内容：修改器中心“已绑定工具”页支持从资源管理器拖入单个文件或目录；`.ct` 自动进入 CheatTable 导入，`.lnk/.bat/.cmd/.ps1` 自动按自定义启动项（外部引用不复制），`.exe` 弹出“修改器 / 普通启动项”二选一，`.zip` 与目录复用既有主程序选择流程；未选择游戏时拒绝导入并提示。导入后 AutoStart 仍保持默认关闭。
- 主要修改文件：`DashboardViewModel.cs`、`TrainerCenterView.xaml`、`TrainerCenterView.xaml.cs`、`DragDropImportSourceTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `59/59`、Worker `182/182`、Playnite `224/224`；`validate-source.py`、XAML/WPF 静态门禁与五种窗口 `render-qa` 全绿。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 03:06:22 记录插件加载，`worker-launch.log` 03:06:28 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实拖入 EXE/CT/LNK/BAT/CMD/PS1/ZIP/目录，并人工确认 EXE 二选一、多 EXE 选择和 AutoStart 默认关闭。
- Commit：`5d113c3`。
- 下一项：Layer C `UI-STATE-001` 合理 UI 状态持久化。

## 2026-08-14 PLAYNITE-QUICK-001 Playnite 游戏右键快捷操作

- Task ID：`PLAYNITE-QUICK-001`。
- 实现内容：`GameSaveCenterPlugin.GetGameMenuItems` 为游戏右键菜单提供“立即备份 / 查看备份历史 / 验证最新恢复点 / 游戏工具 / 打开设置”五个快捷操作，全部使用当前所选游戏 ID，并复用 Worker 的 `backup.game`、`backup.list`、`restore.readiness.validate`、`tools.list` 生产 IPC 链路；菜单统一分组到 `GameSaveCenter`。
- 主要修改文件：`GameSaveCenterPlugin.cs`、`QuickActionSourceTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `59/59`、Worker `182/182`、Playnite `222/222`；`validate-source.py` 与源码门禁通过（本阶段无 XAML 改动，不重跑 render-qa）。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 02:50:33 记录插件加载，`worker-launch.log` 02:50:39 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实 Playnite 游戏库中右键逐项点击五个快捷操作，核对备份提交、历史/恢复点/工具摘要与设置页打开。
- Commit：`15ab1d7`。
- 下一项：Layer C `DRAGDROP-001` 修改器中心拖拽导入。

## 2026-08-14 ACTIVITY-001 首页全局活动时间线

- Task ID：`ACTIVITY-001`。
- 实现内容：首页新增“全局活动”卡片，通过 `ActivityTimelineMapper` 把最近 100 条审计记录映射为备份/恢复/云端/媒体/游戏工具/健康/冲突/完整性/仓库修复等业务事件；每条只保留时间、游戏、分类、结果与摘要，不暴露原始日志或堆栈。Dashboard 快照新增 `RecentActivities`，UI 最多显示 12 条，使用有限 240 DIP 视口、虚拟化与 Recycling。
- 主要修改文件：`DashboardDtos.cs`、`ActivityTimelineMapper.cs`、`DashboardService.cs`、`SnapshotComparers.cs`、`DashboardViewModel.cs`、`OverviewView.xaml`、`ActivityTimelineMapperTests.cs`、`UiLayoutRegressionTests.cs`、`FakeDashboardData.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `59/59`、Worker `182/182`、Playnite `221/221`；`validate-source.py`、XAML/WPF 静态门禁与五种窗口 `render-qa` 全绿。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 02:41:38 记录插件加载，`worker-launch.log` 02:41:44 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实备份/恢复/云端/媒体/工具操作后人工核对时间线分类、结果与摘要可读性。
- Commit：`7f38b15`。
- 下一项：Layer C `PLAYNITE-QUICK-001` Playnite 游戏右键快捷操作。

## 2026-08-14 LOCAL-MIRROR-001 第二本地镜像复制与校验

- Task ID：`LOCAL-MIRROR-001`。
- 实现内容：设置页新增“启用第二本地镜像”与镜像目录；维护中心“保留策略”页新增镜像状态卡与“刷新状态/同步镜像”入口。Worker `LocalMirrorService` 从存档目录按相对路径复制到镜像并校验文件大小，跳过大小相同的现有文件，从不删除镜像中多余文件；外置硬盘未连接时状态为 `Unavailable` 而不是系统错误，同步完成写入 `.gsc-mirror-sync.json` 标记。
- 主要修改文件：`LocalMirrorDtos.cs`、`LocalMirrorService.cs`、`WorkerOptions.cs`、`OperationDtos.cs`、`MessageTypes.cs`、`IpcRequestDispatcher.cs`、`GameSaveCenterSettings.cs`、`GameSaveCenterSettingsView.xaml`、`DashboardViewModel.cs`、`MaintenanceView.xaml`、`MaintenanceView.xaml.cs`、`LocalMirrorServiceTests.cs`、`PortableSettingsTests.cs`、`UiLayoutRegressionTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `55/55`、Worker `182/182`、Playnite `220/220`；`validate-source.py`、XAML/WPF 静态门禁与五种窗口 `render-qa` 全绿。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 02:24:22 记录插件加载，`worker-launch.log` 02:24:28 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实外置硬盘插入/拔出、镜像同步后人工核对文件数量与大小，以及主仓库删除后镜像历史不被删除。
- Commit：`387149f`。
- 下一项：Layer C `ACTIVITY-001` 全局活动时间线。

## 2026-08-14 RETENTION-SIM-001 全局保留策略模拟器与安全清理

- Task ID：`RETENTION-SIM-001`。
- 实现内容：维护中心“保留策略”页新增全局保留策略模拟器：按每游戏持久化策略复用 `RetentionPlanner`，汇总现有版本、建议保留、候选清理、预计释放，以及用户锁定/健康保护/PreRestore 计数；展示最多 200 条候选明细。`retention.simulation.apply` 要求二次确认，执行时重新计算并逐条复核，只删除备份根目录下的 ZIP 候选并同步移除 SQLite 索引；锁定、PreRestore、健康恢复点和根目录外/非 ZIP 路径自动跳过，失败明细写入审计。
- 主要修改文件：`RetentionSimulationDtos.cs`、`RetentionSimulationService.cs`、`SqliteStateStore.cs`、`MessageTypes.cs`、`IpcRequestDispatcher.cs`、`DashboardViewModel.cs`、`MaintenanceView.xaml`、`MaintenanceView.xaml.cs`、`RetentionSimulationServiceTests.cs`、`UiLayoutRegressionTests.cs`、`FakeDashboardData.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `55/55`、Worker `178/178`、Playnite `219/219`；`validate-source.py`、XAML/WPF 静态门禁与五种窗口 `render-qa` 全绿。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 02:10:44 记录插件加载，`worker-launch.log` 02:10:50 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实备份仓库下人工预览并确认清理，核对保护版本未被删除、审计记录完整。
- Commit：`a27f430`。
- 下一项：Layer C `LOCAL-MIRROR-001` 第二本地镜像。

## 2026-08-14 STORAGE-001 备份存储分析与增长趋势预估

- Task ID：`STORAGE-001`。
- 实现内容：维护中心“保留策略”页新增只读“备份存储分析”卡：显示备份目录所在卷总容量/剩余空间、目录实测大小、SQLite 索引体积与版本数、近 7/30/90 天新增索引体积、Top 5 游戏占用排行，并按近 30 天增速给出标注“估算”的容量耗尽预测。所有统计在 Worker 侧执行，支持取消，不删除或移动任何文件。
- 主要修改文件：`StorageAnalysisDtos.cs`、`StorageAnalysisService.cs`、`SqliteStateStore.cs`、`MessageTypes.cs`、`IpcRequestDispatcher.cs`、`DashboardViewModel.cs`、`MaintenanceView.xaml`、`MaintenanceView.xaml.cs`、`StorageAnalysisServiceTests.cs`、`UiLayoutRegressionTests.cs`、`FakeDashboardData.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `55/55`、Worker `174/174`、Playnite `218/218`；`validate-source.py`、XAML/WPF 静态门禁与五种窗口 `render-qa` 全绿。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 01:45:42 记录插件加载，`worker-launch.log` 01:45:48 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实备份仓库下人工核对磁盘数字、增长趋势与预测合理性。
- Commit：`2ef8f13`。
- 下一项：Layer C `RETENTION-SIM-001` 全局保留策略模拟器。

## 2026-08-14 FAULT-INJECTION-001 ZIP/数据库故障注入补齐

- Task ID：`FAULT-INJECTION-001`。
- 实现内容：`FaultInjectionHarness` 新增 `corrupted-zip` 与 `corrupted-database` 两类边界故障：损坏 ZIP 必须按 `InvalidDataException` 失败且测试临时归档被清理；损坏 SQLite 文件初始化必须失败且原始数据库文件不得被失败注入删除。总注入故障从 13 类提升到 15 类，仍断言 `Attempted == Recovered`、无意外错误、无残留锁/订阅/临时文件。
- 主要修改文件：`tests/GameSaveCenter.Worker.Tests/Infrastructure/FaultInjectionHarness.cs`、`FaultInjectionTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `55/55`、Worker `170/170`、Playnite `217/217`；`validate-source.py` 与 XAML 结构门禁通过（测试-only 改动，不重跑 render-qa）。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 01:28:48 记录插件加载，`worker-launch.log` 01:28:54 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实磁盘满/权限拒绝/进程强杀/断电时对备份、恢复与元数据操作的人工复核。
- Commit：`738f339`。
- 下一项：生成 Layer B Audit，随后进入 Layer C 11 项。

## 2026-08-14 SOAK-001 数据规模与资源监控 Soak

- Task ID：`SOAK-001`。
- 实现内容：新增 `SoakDataScaleHarness`：默认测试用小规模（200 游戏/2000 备份/1000 任务/3000 媒体/50 工具），设置 `GSC_SOAK_DATA_SCALE=1` 运行完整规模（2000/20000/10000/30000/500）；循环模拟 Dashboard 读取、任务/媒体/工具查询、事件扇出、原子写入和操作锁。
- 监控与断言：记录 Managed Memory、Handle Count、Thread Count、订阅残留、临时文件残留，并做有界增长断言（内存 < 256 MiB 增量、句柄 +256、线程 +32）；失败会记录错误而非只输出日志。
- 主要修改文件：`tests/GameSaveCenter.Worker.Tests/Infrastructure/SoakDataScaleHarness.cs`、`SoakDataScaleTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `55/55`、Worker `170/170`、Playnite `217/217`；`validate-source.py` 与 XAML 结构门禁通过（测试-only 改动，不重跑 render-qa）。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 01:20 记录插件加载，`worker-launch.log` 01:20:05 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实 24/48 小时宿主持续运行、真实大库与长时间挂机后的内存/句柄趋势观察。
- Commit：`c9e053a`。
- 下一项：继续 Layer B，从 `FAULT-INJECTION-001` ZIP/数据库故障注入补齐开始。

## 2026-08-14 ATOMIC-IO-001 原子写入旧文件完整保留审计

- Task ID：`ATOMIC-IO-001`。
- 实现内容：审计确认共享 `AtomicFileWriter` 已覆盖 Worker 设置持久化、媒体复制、元数据恢复替换和启动失败计数；本阶段补充“取消写入时旧文件完整保留”“替换失败时旧文件完整保留且无 `.replace` 残留”两项审计测试。
- 主要修改文件：`AtomicFileWriterTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `55/55`、Worker `169/169`、Playnite `217/217`；`validate-source.py` 与 XAML 结构门禁通过（测试-only 改动，不重跑 render-qa）。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 01:09 记录插件加载，`worker-launch.log` 01:09:28 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实进程在写入中途被终止/断电后确认目标文件不出现截断半成品且临时文件被清理。
- Commit：`eed946c`。
- 下一项：继续 Layer B，从 `SOAK-001` 数据规模与监控补齐开始。

## 2026-08-14 IPC-COMPAT-001 IPC 握手能力列表

- Task ID：`IPC-COMPAT-001`。
- 实现内容：`WorkerHandshakeDto` 增加 `AppVersion` 与 `Capabilities`，握手返回 `RestoreReadiness/MetadataBackup/RepositoryRebuild/PathRemap/TaskReconcile/GameOperationLock/AtomicIo` 能力列表；插件/Worker 仍使用独立 `ProtocolVersion`，不直接用 0.6.70 判断兼容。
- 主要修改文件：`WorkerDtos.cs`、`IpcRequestDispatcher.cs`、`ProtocolCompatibilityTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `55/55`、Worker `167/167`、Playnite `217/217`；`validate-source.py` 与 XAML 结构门禁通过（本阶段无 UI 改动，不重跑 render-qa）。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 01:01 记录插件加载，`worker-launch.log` 01:01:23 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：保留旧版 Worker 后启动新插件，确认握手拒绝并替换旧进程。
- Commit：`7ee738d`。
- 下一项：继续 Layer B，从 `ATOMIC-IO-001` 重要状态文件原子写入审计补齐开始。

## 2026-08-14 GAME-OP-LOCK-001 单游戏操作兼容矩阵

- Task ID：`GAME-OP-LOCK-001`。
- 实现内容：新增 `GameOperationKind`（Backup/Restore/Retention/CloudUpload/CloudDownload/Media/RestoreReadiness/RepositoryRepair）与 `GameOperationPolicy.IsCompatible` 显式兼容矩阵：Restore 与所有操作不兼容，同游戏两个 Backup 不兼容，Retention 与 Backup/Restore 不兼容；Backup+Media、CloudUpload+Media、RestoreReadiness+Backup 标记兼容。
- 锁入口：`GameOperationLock.AcquireAsync` 新增带操作类型的重载，备份、云端重试、媒体同步、恢复预览/执行均已接入类型化调用；底层仍按 GameId 串行化（安全超集），不同游戏可并行。
- 主要修改文件：`GameOperationLock.cs`、`BackupOrchestrator.cs`、`RestoreOrchestrator.cs`、`MediaSyncService.cs`、`GameOperationLockTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `54/54`、Worker `167/167`、Playnite `217/217`；`validate-source.py` 与 XAML 结构门禁通过（本阶段无 UI 改动，不重跑 render-qa）。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 00:52 记录插件加载，`worker-launch.log` 00:52:47 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实长时间备份期间对同一游戏发起恢复/媒体同步，确认 `GAME_OPERATION_BUSY` 且不同游戏仍并行。
- Commit：`7dba09c`。
- 下一项：继续 Layer B，从 `IPC-COMPAT-001` 能力列表补齐开始。

## 2026-08-14 TASK-RECONCILE-001 Worker 中断任务分类协调

- Task ID：`TASK-RECONCILE-001`。
- 实现内容：为每个 Worker 进程生命周期生成 `WorkerSessionId`，`TaskStatusDto` 与 `tasks.worker_session_id` 持久化任务所属 Worker。Worker 启动时只协调“不属于当前 Worker 会话”的遗留 `Queued/Running` 任务，并分类：`Backup/MediaSync/MediaInbox/CloudUpload` 标记 `WORKER_RESTARTED_RETRYABLE`，`Integrity` 标记 `WORKER_RESTARTED`，`Restore` 标记 `MANUAL_INTERVENTION_REQUIRED` 且错误信息提示检查 PreRestore 快照，绝不自动重试恢复。
- UI/语义：任务中心可见“Worker 意外退出，任务中断”的错误信息；手动“协调中断任务”复用同一分类逻辑。
- 主要修改文件：`OperationDtos.cs`、`WorkerOptions.cs`、`SqliteStateStore.cs`、`TaskCoordinator.cs`、`TaskReconcileService.cs`、`WorkerInitializationService.cs`、`TaskEventBroadcaster.cs`、`SnapshotComparers.cs` 及相关 Worker 测试。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `54/54`、Worker `158/158`、Playnite `217/217`；`validate-source.py` 与 XAML 结构门禁通过（本阶段无 XAML 改动，不重跑 render-qa）。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 00:44 记录插件加载，`worker-launch.log` 00:44:18 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实中断恢复任务后人工核对 `MANUAL_INTERVENTION_REQUIRED` 与 PreRestore 快照；以及中断上传/备份后从任务中心重试的真实流程。
- Commit：`47685bd`。
- 下一项：继续 Layer B，从 `GAME-OP-LOCK-001` 单游戏操作兼容矩阵补齐开始。

## 2026-08-14 PATH-REMAP-001 批量路径迁移预览与目标缺失策略

- Task ID：`PATH-REMAP-001`。
- 实现内容：路径迁移增加只读预览 `path.remap.preview`，按“备份归档/媒体归档/媒体原始路径/媒体来源/游戏工具入口/工作目录/解析目标/存档候选”列出受影响旧/新路径并标记目标是否存在；执行前自动调用 `MetadataBackupService.CreateAsync` 生成元数据灾备，再写库。
- 目标缺失策略：`PathRemapRequestDto.ApplyMissingTargets` 默认 `false`；存在缺失目标且未授权应用时返回 `PATH_REMAP_TARGET_MISSING` 并整体跳过（默认安全跳过）；用户确认后可以 `ApplyMissingTargets=true` 仍应用。UI 先预览并显示缺失数量，再二次确认。
- 主要修改文件：`PathRemapService.cs`、`PathRemapDtos.cs`、`SqliteStateStore.PathRemap.cs`、`MessageTypes.cs`、`IpcRequestDispatcher.cs`、`DashboardViewModel.cs`、`PathRemapServiceTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `54/54`、Worker `158/158`、Playnite `217/217`；`validate-source.py` 与 XAML 结构门禁通过（本阶段无 XAML 改动，不重跑 render-qa）。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 00:32 记录插件加载，`worker-launch.log` 00:32:16 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实移动目录后预览并执行迁移，核对数据库路径、Worker 设置与自动生成的元数据灾备包。
- Commit：`bddcfdd`。
- 下一项：继续 Layer B，从 `TASK-RECONCILE-001` Worker 中断任务分类恢复补齐开始。

## 2026-08-14 REPOSITORY-REBUILD-001 备份索引重建预览与确认

- Task ID：`REPOSITORY-REBUILD-001`。
- 实现内容：备份索引重建拆成“只读扫描预览 → 用户确认 → 写库”三个阶段。`repository.rebuild.preview` 扫描存档目录 ZIP，统计已确认归属/未归属/元数据部分缺失/损坏归档并返回摘要；`repository.rebuild` 现在要求 `Confirmed=true`，未确认返回 `REPOSITORY_REBUILD_NOT_CONFIRMED`。
- UI：维护中心“重建备份索引”先调用预览并显示摘要，再弹出确认框，确认后才执行真实重建；失败游戏仍保留原索引，重复重建不会产生额外版本（沿用既有幂等刷新路径）。
- 主要修改文件：`RepositoryRebuildService.cs`、`RepositoryRebuildDtos.cs`、`MessageTypes.cs`、`IpcRequestDispatcher.cs`、`DashboardViewModel.cs`、`RepositoryRebuildServiceTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `54/54`、Worker `157/157`、Playnite `217/217`；`validate-source.py` 与 XAML 结构门禁通过（本阶段无 XAML 改动，不重跑 render-qa）。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 00:23 记录插件加载，`worker-launch.log` 00:23:37 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实 Ludusavi 备份目录下预览后执行重建，并核对版本数量和归档路径。
- Commit：`dff0cf4`。
- 下一项：继续 Layer B，从 `PATH-REMAP-001` 批量路径迁移预览/目标缺失处理补齐开始。

## 2026-08-14 METADATA-BACKUP-001 元数据灾备恢复流程

- Task ID：`METADATA-BACKUP-001`。
- 实现内容：在既有元数据灾备导出基础上补齐安全恢复：`metadata.restore.preview` 校验 ZIP 内的 manifest、数据库/设置 SHA-256、条目路径越界；`metadata.restore.execute` 要求确认，执行前复制当前数据库与 Worker 设置到 `MetadataBackups/PreRestore-*`，临时暂停后台自动操作，使用 `AtomicFileWriter.ReplaceFileAsync` 原子替换，随后执行 SQLite `integrity_check`；失败时回滚恢复前副本，回滚失败返回 `METADATA_RESTORE_ROLLBACK_FAILED`。
- UI：维护中心新增“恢复元数据灾备”按钮，流程为选择 ZIP → 预览 → 危险操作确认 → 执行 → 显示恢复摘要与恢复前副本路径。
- 核心设计选择：恢复包不会被直接解压覆盖；替换前清理 WAL/SHM 侧车文件并确保文件属性可写；`SqliteConnection.ClearAllPools` 用于释放 SQLite 池锁。Worker.Tests 增加 `DisableTestParallelization`，避免全局池清理干扰并行测试。
- 主要修改文件：`MetadataBackupService.cs`、`MetadataBackupDtos.cs`、`MessageTypes.cs`、`IpcRequestDispatcher.cs`、`AtomicFileWriter.cs`、`WorkerOptions.cs`、`DashboardViewModel.cs`、`MaintenanceView.xaml`、`MetadataBackupServiceTests.cs`、`AssemblyInfo.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `54/54`、Worker `155/155`、Playnite `217/217`；`validate-source.py`、XAML 结构、WPF 静态门禁 0 errors / 33 warnings / 153 info、五种窗口 `render-qa OK`。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 00:15 记录插件加载，`worker-launch.log` 00:15:20 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实选择元数据包执行恢复、核对恢复前副本与恢复后数据库，以及恢复失败回滚的人工复核。
- Commit：`97b06b5`。
- 下一项：继续 Layer B，从 `REPOSITORY-REBUILD-001` 备份仓库索引重建预览/确认补齐开始。

## 2026-08-13 DB-MIGRATION-001 历史数据库 Fixture 补齐

- Task ID：`DB-MIGRATION-001`。
- 实现内容：`DatabaseMigrationHarness` 增加 `ReadScalar`，两代旧库 Fixture 覆盖 `game_policies`、`backup_policy_templates`、`sessions`、`device_conflict_decisions`、备份历史、任务、媒体来源、GameTool 与版本表；第二个 Fixture 模拟缺少 `session_id/archive_path/shared_directory/risk_category/resolved_target_path` 等新列的更老 schema，验证升级补列且数据不丢。
- 数据值断言：不再只断言“表能打开”，而是直接验证冲突决策、策略 JSON、模板名称、会话启动配置、任务类型、锁定状态、媒体来源匹配模式和工具路径等真实值；内置策略模板种子使用 `INSERT OR IGNORE`，旧 `important` 模板保留。
- 主要修改文件：`tests/GameSaveCenter.Worker.Tests/Infrastructure/DatabaseMigrationHarness.cs`、`DatabaseMigrationHarnessTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `54/54`、Worker `152/152`、Playnite `217/217`；`validate-source.py` 与 XAML 结构门禁通过（测试-only 改动，不重跑 render-qa）。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 23:42 记录插件加载，`worker-launch.log` 23:42:43 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实历史用户数据库升级后的功能回归，以及大量历史数据迁移时长的人工复核。
- Commit：`55f532f`。
- 下一项：继续 Layer B，从 `METADATA-BACKUP-001` 元数据灾备恢复流程审计开始。

## 2026-08-13 INTEGRITY-001 完整性自检补齐

- Task ID：`INTEGRITY-001`。
- 实现内容：在既有 SQLite/表结构/目录/已索引文件检查基础上补齐：磁盘上未被数据库索引的孤儿归档（`ORPHAN_ARCHIVE`）、备份 Manifest 无法解析或重复路径（`MANIFEST_INVALID`/`MANIFEST_DUPLICATE_PATH`）、数据/存档/媒体所在卷剩余空间不足（`LOW_DISK_SPACE`）、未配置依赖的状态收敛为 `Warning`（Ludusavi）或 `Skipped`（Rclone 可选）。
- 结果模型：`IntegrityCheckResultDto` 现在使用 `Healthy/Warning/Error/Skipped` 四态并返回 `SkippedCount`；自检仍完全只读，不自动删除或修复。
- 主要修改文件：`IntegrityCheckService.cs`、`SqliteStateStore.Integrity.cs`、`IntegrityDtos.cs`、`IntegrityCheckServiceTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `54/54`、Worker `151/151`、Playnite `217/217`；`validate-source.py` 与 XAML 结构门禁通过（本阶段无 UI 改动，不重跑 render-qa）。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 23:27 记录插件加载，`worker-launch.log` 23:27:07 记录 Worker 启动与 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实磁盘不足、孤儿 ZIP、损坏 Manifest 场景下人工复核自检结果；修复动作仍未实现自动应用，只提供建议。
- Commit：`a064108`。
- 下一项：继续 Layer B，从 `DB-MIGRATION-001` 历史数据库 Fixture 补齐开始。

## 2026-08-13 DIAGNOSTICS-001 一键诊断包结构化升级

- Task ID：`DIAGNOSTICS-001`。
- 实现内容：诊断包从纯文本摘要升级为结构化 JSON：`system.json`、`worker.json`、`dependencies.json`、`database.json`、`recent-tasks.json`、`health.json`、`settings.json`，外加 README、审计与受限日志尾部。新增集中式 `DiagnosticRedactor`，统一脱敏密码/Token/API Key/Authorization/Bearer、URL query secret、UNC 凭据、邮箱和 `C:\Users\<USER>` 用户路径。
- 核心设计选择：所有写入包内的字符串都先经过 `DiagnosticRedactor`；数据库只做只读探针，不打包 SQLite；日志/包大小保持上限；Playnite 侧传入插件版本、Playnite 版本、主题、工作区与 DPI 信息，Worker 补 PID/启动时间/Uptime/协议版本。
- 主要修改文件：`DiagnosticsPackageService.cs`、`DiagnosticRedactor.cs`、`DiagnosticsDtos.cs`、`SqliteStateStore.cs`、`DashboardViewModel.cs`、`DiagnosticsPackageServiceTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `54/54`、Worker `148/148`、Playnite `217/217`；`validate-source.py`、XAML 结构、WPF 静态门禁 0 errors / 33 warnings / 153 info、五种窗口 `render-qa OK`。禁止泄密测试覆盖 `abc123/token123/secret123/JohnDoe/querysecret`。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 23:18 记录插件加载，`worker-launch.log` 23:18:24 记录 Worker 启动与 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实 Playnite 中导出诊断包后人工检查 ZIP 内 JSON 字段与脱敏结果。
- Commit：`17e88b6`（与 SAFE-MODE-001 同组提交）。
- 下一项：`SAFE-MODE-001` 启动恢复（同一提交）后继续 `INTEGRITY-001` 补齐。

## 2026-08-13 SAFE-MODE-001 安全模式启动恢复

- Task ID：`SAFE-MODE-001`。
- 实现内容：在既有安全模式门禁基础上补齐启动恢复：`WorkerOptions` 新增持久化 `SafeModeRequested` 与启动失败计数文件；连续 3 次 Worker 初始化失败后自动请求安全模式，Playnite 打开维护中心时用 `ConfirmAsync` 询问“是否使用安全模式打开”，确认后启用并清除请求。设置页新增“下次以安全模式启动”开关，维护中心安全模式提示条新增“恢复正常模式”按钮。
- 核心设计选择：安全模式请求不自动永久启用，必须由用户确认；启动成功只清失败计数、保留待确认请求；手动退出安全模式走真实 `UpdateSettings` 管道并立即刷新诊断。
- 主要修改文件：`WorkerOptions.cs`、`WorkerInitializationService.cs`、`OperationDtos.cs`、`IpcRequestDispatcher.cs`、`GameSaveCenterSettings.cs`、`GameSaveCenterSettingsView.xaml`、`MaintenanceView.xaml`、`DashboardViewModel.cs`、`SafeModeStartupTests.cs`。
- 测试结果：与 DIAGNOSTICS-001 同批：Core `54/54`、Worker `148/148`、Playnite `217/217`；安全模式三次失败请求、成功清计数、请求清除、设置持久化与 UI 绑定均有测试。
- 真实宿主验证：与 DIAGNOSTICS-001 同批通过；插件与 Worker 正常加载。
- MANUAL QA REQUIRED：真实连续启动失败 3 次后人工确认提示与安全模式自动暂停效果；以及“恢复正常模式”按钮的真实行为。
- Commit：`17e88b6`。
- 下一项：继续 Layer B，从 `INTEGRITY-001` 全局完整性自检补齐开始。

## 2026-08-13 A-HARDEN-003 Onboarding 测试备份闭环审计

- Task ID：`A-HARDEN-003`。
- 实现内容：审计确认 ONBOARDING-001 已具备真实“测试备份”入口：维护中心首次使用卡片上的 `OnboardingTestBackupCommand` 直接调用 `BackupSelectedAsync`，而 `BackupSelectedAsync` 走真实 `MessageTypes.BackupGame` 生产备份管道（`Reason="Manual"`、`Force=true`），没有独立的 `TestBackupService` 或假服务。按钮仅在存在已匹配当前游戏且 Ludusavi 可用时可用。
- 本阶段补齐：无可用测试游戏时的明确文案改为“若当前没有可用于测试的已识别存档游戏，可稍后在存档中心手动执行备份。”，并新增回归测试锁定“OnboardingTestBackupCommand → BackupSelectedAsync → MessageTypes.BackupGame/Manual”链路。
- 主要修改文件：`MaintenanceView.xaml`、`tests/GameSaveCenter.Playnite.Tests/SettingsAndAutoSelectSourceTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `54/54`、Worker `145/145`、Playnite `216/216`；`validate-source.py`、XAML 结构、WPF 静态门禁 0 errors / 33 warnings / 153 info、五种窗口 `render-qa OK`。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 22:58 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`，插件日志记录 `0.6.70.0 loaded`，`worker-launch.log` 22:58:47 记录 Worker 启动、存储初始化和 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实首次使用流程中点击“对当前游戏做测试备份”，确认任务中心显示真实 Manual Backup 结果和“测试备份成功”反馈。
- Commit：`5666944`。
- 下一项：STEP 7 Layer A Hardening Gate（Release/Core/Worker/Playnite/source/XAML/render-qa 全量复跑），随后生成 Layer B 逐项审计。

## 2026-08-13 A-HARDEN-002 未分类自定义启动项反作弊自动启动语义

- Task ID：`A-HARDEN-002`。
- 实现内容：修正 `GameToolAutoStartPolicy`：`CustomExecutable + Unknown` 在普通游戏下按用户 AutoStart 正常启动，不再因未分类一律暂缓；遇到反作弊游戏时，只有该工具显式持久化授权 `AllowUnknownToolWithAntiCheat` 后才允许。`GeneralUtility` 在反作弊游戏下继续允许，`GameModification`/`Trainer`/`CheatTable` 在反作弊游戏下继续禁止。
- 数据模型：`GameToolDto` 与 `UpdateGameToolRequestDto` 新增 `AllowUnknownToolWithAntiCheat`；`game_tools` 新增 `allow_unknown_anticheat_autostart INTEGER NOT NULL DEFAULT 0`，`EnsureColumnAsync` 与 CREATE TABLE 同步，旧库升级由迁移 Harness 校验。
- UI：TrainerCenter 工具类别区域新增“反作弊授权”开关，仅当自定义启动项且类别为未分类时显示；未分类选项文案从“自动启动将暂缓”改为“未分类（反作弊游戏需授权）”。`SnapshotComparers` 现在同时比较 IfAlreadyRunning/RiskCategory/AllowUnknownToolWithAntiCheat。
- 主要修改文件：`GameToolAutoStartPolicy.cs`、`SqliteStateStore.cs`、`TrainerDtos.cs`、`TrainerCenterView.xaml`、`DashboardViewModel.cs`、`SnapshotComparers.cs`，以及 Worker/Playnite 相关测试。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `54/54`、Worker `145/145`、Playnite `215/215`；`validate-source.py`、XAML 结构、WPF 静态门禁 0 errors / 33 warnings / 153 info、五种窗口 `render-qa OK`。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 22:53 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`，插件日志记录 `0.6.70.0 loaded`，`worker-launch.log` 22:53:09 记录 Worker 启动、存储初始化和 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实反作弊游戏与未分类自定义工具组合下的人工复核，确认授权开关持久化且不会每次启动重复询问；普通游戏下未分类工具按 AutoStart 正常启动。
- Commit：`8d6c36c`。
- 下一项：`A-HARDEN-003` Onboarding 测试备份闭环审计。

## 2026-08-13 A-HARDEN-001 通知级别正式实现与回归补齐

- Task ID：`A-HARDEN-001`。
- 实现内容：在既有 `NOTIFY-LEVELS-001` 基础上正式收口通知级别：`NotificationLevel`（`ImportantOnly/Summary/Verbose`）持久化到 Settings，默认 `Summary`；`NotificationLevelPolicy` 纯策略控制会话摘要与任务事件；设置页提供“仅重要通知/退出摘要/详细任务通知”和说明文本。本阶段把 `SessionNotificationAccumulator` 从插件私有嵌套类抽出为 `Infrastructure` 内部类，并给 Playnite.Tests 开放 InternalsVisibleTo，补齐“同一 Session 只发一次最终摘要”“期望任务数未到不提前汇总”“重复投递只替换终态不重复”和“Verbose 子事件 + 最终摘要”等回归测试。
- 核心设计选择：通知状态继续复用 Task Center 的同一批终态任务，不另造成功判断；Summary 对 Session 任务只发一条退出摘要，Verbose 才额外逐任务显示；同一 Session 由 `TryMarkEmitted` 保证不重复 final。
- 主要修改文件：`src/GameSaveCenter.Playnite/Infrastructure/SessionNotificationAccumulator.cs`、`GameSaveCenterPlugin.cs`、`GameSaveCenter.Playnite.csproj`、`tests/GameSaveCenter.Playnite.Tests/SessionNotificationAccumulatorTests.cs`、`tests/GameSaveCenter.Core.Tests/NotificationLevelPolicyTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `54/54`、Worker `141/141`、Playnite `215/215`；`validate-source.py` 与 XAML 结构门禁通过（本阶段未改 XAML，不重跑 render-qa 与 WPF 静态门禁）。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 22:34 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`，插件日志记录 `0.6.70.0 loaded`，`worker-launch.log` 22:34:39 记录 Worker 启动、存储初始化和 `Application started`；安装后无新增插件异常。
- 验证边界：非任务型重要事件（Health Risk、Backup anomaly、Multi-device conflict、Integrity serious error）目前由维护中心 Findings/Dashboard 承载，尚未单独生成 Playnite toast；真实宿主下三种通知级别的实际观感仍需人工复核。
- Commit：`fbbdb90`。
- 下一项：`A-HARDEN-002` CustomExecutable Unknown AutoStart 语义修正。

## 2026-08-13 FAULT-INJECTION-001 关键 I/O 故障注入

- Task ID：`FAULT-INJECTION-001`。
- 实现内容：新增 `FaultInjectionHarness` 与 `FaultInjectionTests`，只服务自动测试，不在生产代码中加入 `TestMode` 分支。注入 13 类确定性边界故障：原子写源缺失、目标已占用、取消写入、外部进程缺失/超时/取消/非零退出、TaskCoordinator 业务异常/通用异常/取消、订阅释放后发布、单游戏锁争用超时和双重 Dispose。
- 核心设计选择：Harness 使用临时数据目录，故障后断言 `.tmp/.partial` 无残留、任务进入稳定终态、取消令牌被移除、锁可重新获取、订阅计数归零；故障注入不触碰真实存档、媒体、数据库或 Worker 设置。
- 主要修改文件：`tests/GameSaveCenter.Worker.Tests/Infrastructure/FaultInjectionHarness.cs`、`FaultInjectionTests.cs`、`scripts/fault-injection-test.ps1`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `53/53`、Worker `141/141`、Playnite `211/211`；`validate-source.py` 与 XAML 结构门禁通过（本阶段无 UI 改动，不重跑 render-qa 与 WPF 静态门禁）。`scripts/fault-injection-test.ps1` 独立跑通，0 失败。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 22:27 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`，插件日志记录 `0.6.70.0 loaded`，`worker-launch.log` 22:27:34 记录 Worker 启动、存储初始化和 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实磁盘满/权限移除/外部进程崩溃场景下的人工复核，确认任务中心显示稳定错误码且原数据完整。
- Commit：`f91992e`。
- 下一项：按最终总任务顺序回到 STEP 4，从 `A-HARDEN-001` 正式实现通知级别开始，随后 `A-HARDEN-002/003`、Layer A Hardening Gate，再对已交付的 Layer B/C 逐项审计补齐。

## 2026-08-13 SOAK-001 长时间稳定性测试

- Task ID：`SOAK-001`。
- 实现内容：新增可复用 `SoakStabilityHarness` 与 `SoakStabilityTests`，以加速轮次反复压测 Worker 最容易长期退化/泄漏的表面：`TaskCoordinator` 任务持久化与变更 Feed、`TaskEventBroadcaster` 订阅/发布/释放、`GameOperationLock` 单游戏互斥、`AtomicFileWriter` 原子写与失败清理、SQLite 临时表读写探针和审计写入。新增 `scripts/soak-test.ps1` 长跑入口，可通过 `GSC_SOAK_ITERATIONS` 把默认 100 轮扩展到最多 5000 轮。
- 核心设计选择：Harness 只使用临时数据目录，不触碰真实存档、媒体、数据库或 Worker 设置；为稳定性断言补充 `TaskEventBroadcaster.SubscriberCount` 与 `GameOperationLock.TrackedGameCount` 两个只读计数，不改任何并发行为。变更 Feed 的保留上限仍为 500 条，慢订阅者容量仍为 128 条 Drop-Oldest。
- 主要修改文件：`tests/GameSaveCenter.Worker.Tests/Infrastructure/SoakStabilityHarness.cs`、`SoakStabilityTests.cs`、`TaskEventBroadcasterTests.cs`、`GameOperationLockTests.cs`、`TaskEventBroadcaster.cs`、`GameOperationLock.cs`、`scripts/soak-test.ps1`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `53/53`、Worker `140/140`、Playnite `211/211`；`validate-source.py` 与 XAML 结构门禁通过（本阶段无 UI 改动，不重跑 render-qa 与 WPF 静态门禁）。`scripts/soak-test.ps1 -Iterations 60` 独立跑通，0 失败。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 22:18 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`，插件日志记录 `0.6.70.0 loaded`，`worker-launch.log` 22:18:53 记录 Worker 启动、存储初始化和 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实 24/48 小时宿主持续运行、内存/句柄趋势、大游戏库与真实备份/媒体任务混跑，以及长时间挂机后任务中心/通知仍可用的观察。
- Commit：`8132419`。
- 下一项：继续 Layer B，从 `FAULT-INJECTION-001` 故障注入开始。

## 2026-08-13 ATOMIC-IO-001 原子写入审计与共享原子写入器

- Task ID：`ATOMIC-IO-001`。
- 实现内容：新增 Worker 共享 `AtomicFileWriter`，统一设置持久化与媒体复制两处“临时文件 + 原子替换”逻辑。`WriteAllTextAsync` 在目标同目录写唯一 `.tmp` 后 `File.Move(..., true)`；`CopyAtomicallyAsync` 在目标同目录写唯一 `.partial` 后 `File.Move(..., false)`。任何失败都会先清理残留临时文件再重新抛出。
- 核心设计选择：临时文件与目标同盘同目录，避免跨卷 `Move` 退化成非原子复制；`WorkerOptions.Persist()` 与 `MediaSyncService` 的私有复制逻辑改为委托共享实现，保证以后所有 Worker 原子写入都走同一套清理与替换契约。
- 主要修改文件：`src/GameSaveCenter.Worker/Infrastructure/AtomicFileWriter.cs`、`WorkerOptions.cs`、`MediaSyncService.cs`，以及 `tests/GameSaveCenter.Worker.Tests/AtomicFileWriterTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `53/53`、Worker `136/136`、Playnite `211/211`；`validate-source.py`、XAML 结构、WPF 静态门禁 0 errors / 33 warnings / 153 info、五种窗口 `render-qa OK`。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 22:04 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`，插件日志记录 `0.6.70.0 loaded`，`worker-launch.log` 记录存储初始化与 `Application started`；安装后无新增插件异常，Playnite 与 Worker 均从当前扩展目录运行。
- MANUAL QA REQUIRED：真实进程在写入中途被终止/断电后确认目标文件不出现截断半成品且临时文件被清理；以及高并发读取方在覆盖替换瞬间始终看到完整旧值或完整新值。
- Commit：`f1eda41`。
- 下一项：继续 Layer B，从 `SOAK-001` 长时间稳定性测试开始。

## 2026-08-13 IPC-COMPAT-001 IPC protocol handshake

- Task ID：`IPC-COMPAT-001`。
- 实现内容：新增显式 `system.handshake` IPC 与 `WorkerHandshakeDto`（协议版本、最低支持版本、Worker 版本、UTC 时间）。`WorkerIpcClient.HandshakeAsync` 先校验 `ProtocolCompatibility.IsCompatible`，不兼容时抛出 `PROTOCOL_MISMATCH`；`WorkerLauncher.ProbeHealthAsync` 优先握手，旧 Worker 不支持握手时回退 Ping，版本不匹配仍标记 `Incompatible`。
- 核心设计选择：兼容规则放在 Contracts 纯静态类，客户端/服务端/测试共用；显式握手与既有逐请求协议版本检查共存，避免新插件静默复用旧 Worker。
- 主要修改文件：Contracts `WorkerDtos.cs`/`ProtocolCompatibility.cs`/`MessageTypes`、`IpcRequestDispatcher`、`WorkerIpcClient`、`WorkerLauncher`，以及 Core/Playnite 测试。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `53/53`、Worker `132/132`、Playnite `211/211`；`validate-source.py`、XAML 结构、WPF 静态门禁 0 errors / 33 warnings / 153 info、五种窗口 `render-qa OK`。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`，插件日志记录 `0.6.70.0 loaded`，`worker-launch.log` 记录存储初始化与 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：保留旧版 Worker 进程后启动新插件，确认握手拒绝并替换旧进程；以及协议版本变更时的真实兼容验证。
- Commit：`6454027`。
- 下一项：继续 Layer B，从 `ATOMIC-IO-001` 原子写入审计开始。

## 2026-08-13 GAME-OP-LOCK-001 单游戏操作兼容锁

- Task ID：`GAME-OP-LOCK-001`。
- 实现内容：新增 `GameOperationLock` 单游戏信号量锁，并接入备份、云端上传重试、媒体同步、恢复预览与恢复执行。同一游戏的备份/恢复/媒体操作在同一时间只允许一个执行；不同游戏仍可并行。锁获取超时返回 `GAME_OPERATION_BUSY`，不会排队积压或静默覆盖。
- 核心设计选择：锁以 PlayniteId 为粒度、大小写不敏感；恢复执行在进入任务前获取锁，预览也持锁，避免与真实写入交错；不清理字典以避免并发竞态，锁对象数量与游戏数有界。
- 主要修改文件：`GameOperationLock`、`BackupOrchestrator`、`MediaSyncService`、`RestoreOrchestrator`、`Program`，以及 Worker 测试。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `51/51`、Worker `132/132`、Playnite `211/211`；`validate-source.py`、XAML 结构、WPF 静态门禁 0 errors / 33 warnings / 153 info、五种窗口 `render-qa OK`。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`，插件日志记录 `0.6.70.0 loaded`，`worker-launch.log` 记录存储初始化与 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实长时间备份期间对同一游戏发起恢复/媒体同步，确认显示 `GAME_OPERATION_BUSY` 且不同游戏仍并行。
- Commit：`dc8a5e6`。
- 下一项：继续 Layer B，从 `IPC-COMPAT-001` IPC protocol handshake 开始。

## 2026-08-13 TASK-RECONCILE-001 Worker 中断任务恢复体系

- Task ID：`TASK-RECONCILE-001`。
- 实现内容：在既有启动自动协调基础上新增 `tasks.reconcile` IPC 与 `TaskReconcileService`。维护中心诊断页新增“协调中断任务”按钮和结果摘要；手动执行会把遗留的 `Queued/Running` 任务幂等标记为 `WORKER_RESTARTED`，保留任务中心和通知统一状态，并写审计。
- 核心设计选择：`MarkInterruptedTasksAsync` 改为返回受影响行数，让手动协调可报告数量；已成功/失败/取消/等待任务不受影响，重复协调不会重复改写。
- 主要修改文件：Contracts `TaskReconcileDtos.cs`/`MessageTypes`、`SqliteStateStore`、`TaskReconcileService`、`Program`、`IpcRequestDispatcher`、`DashboardViewModel`、`MaintenanceView`，以及 Worker/Playnite 测试。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `51/51`、Worker `131/131`、Playnite `211/211`；`validate-source.py`、XAML 结构、WPF 静态门禁 0 errors / 33 warnings / 153 info、五种窗口 `render-qa OK`。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`，插件日志记录 `0.6.70.0 loaded`，`worker-launch.log` 记录存储初始化与 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实中断上传/备份后点击协调，并在任务中心核对 `WORKER_RESTARTED` 状态与重试入口。
- Commit：`2724209`。
- 下一项：继续 Layer B，从 `GAME-OP-LOCK-001` 单游戏操作兼容锁开始。

## 2026-08-13 PATH-REMAP-001 批量路径迁移

- Task ID：`PATH-REMAP-001`。
- 实现内容：新增 `path.remap` IPC 与 `PathRemapService`。维护中心诊断页新增“批量路径迁移”卡片：输入旧根路径和新根路径并确认后，Worker 批量改写 SQLite 中 `backup_versions.archive_path`、`media`、`media_sources`、`game_tool_versions`、`save_candidates` 等已索引路径，并同步 Worker 设置的存档/媒体目录。只改路径引用，不移动或删除文件。
- 核心设计选择：SQLite 更新使用“前缀 + 目录边界”匹配，避免 `C:\Saves` 误改 `C:\Saves2`；服务端强制 `Confirmed=true`，Playnite 侧先弹危险操作确认框；失败/空/相同/相对路径均明确拒绝。
- 主要修改文件：Contracts `PathRemapDtos.cs`/`MessageTypes`、`SqliteStateStore.PathRemap.cs`、`PathRemapService`、`Program`、`IpcRequestDispatcher`、`DashboardViewModel`、`MaintenanceView`，以及 Worker/Playnite 测试。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `51/51`、Worker `130/130`、Playnite `211/211`；`validate-source.py`、XAML 结构、WPF 静态门禁 0 errors / 33 warnings / 153 info、五种窗口 `render-qa OK`。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`，插件日志记录 `0.6.70.0 loaded`，`worker-launch.log` 记录存储初始化与 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实移动备份/媒体目录后执行迁移，并核对 Worker 重启后路径与磁盘归档仍然一致。
- Commit：`a521281`。
- 下一项：继续 Layer B，从 `TASK-RECONCILE-001` Worker 中断任务恢复体系开始。

## 2026-08-13 REPOSITORY-REBUILD-001 备份仓库索引重建

- Task ID：`REPOSITORY-REBUILD-001`。
- 实现内容：新增 `repository.rebuild` IPC 与 `RepositoryRebuildService`。遍历已匹配游戏，通过既有 `RefreshBackupHistoryAsync` 从 Ludusavi 磁盘实际备份列表重建 SQLite `backup_versions` 索引；单个游戏失败不中断其余游戏，结果汇总成功/失败/索引版本数并写审计。维护中心诊断页新增“重建备份索引”按钮和结果摘要。
- 核心设计选择：抽出窄接口 `IBackupHistoryRebuilder`，让重建服务可单测且不依赖真实 Ludusavi；重建只读取磁盘并协调索引，不删除、不上传、不修改真实归档；失败游戏保留原索引。
- 主要修改文件：Contracts `RepositoryRebuildDtos.cs`/`MessageTypes`、`RestoreDependencies`、`BackupOrchestrator`、`RepositoryRebuildService`、`Program`、`IpcRequestDispatcher`、`DashboardViewModel`、`MaintenanceView`，以及 Worker/Playnite 测试。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `51/51`、Worker `128/128`、Playnite `211/211`；`validate-source.py`、XAML 结构、WPF 静态门禁 0 errors / 33 warnings / 153 info、五种窗口 `render-qa OK`。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`，插件日志记录 `0.6.70.0 loaded`，`worker-launch.log` 记录存储初始化与 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实 Ludusavi 备份目录下点击重建后，与磁盘文件逐一核对版本数量和归档路径。
- Commit：`7845a53`。
- 下一项：继续 Layer B，从 `PATH-REMAP-001` 批量路径迁移开始。

## 2026-08-13 METADATA-BACKUP-001 GameSaveCenter 自身元数据灾备

- Task ID：`METADATA-BACKUP-001`。
- 实现内容：新增 `metadata.backup.create` IPC 与 `MetadataBackupService`，在数据目录 `MetadataBackups` 生成有上限、自描述的 ZIP 灾备包。包含 SQLite 一致性快照（`VACUUM INTO`）、脱敏的 Worker 设置、版本与校验清单、README；明确不包含存档、媒体、Rclone 配置或凭据。维护中心诊断页新增“导出元数据灾备”按钮和结果摘要。
- 核心设计选择：数据库快照使用 SQLite `VACUUM INTO` 获取一致副本，避免直接复制 WAL 状态；设置文件先脱敏再入包；ZIP 超过 512 MiB 直接失败并清理临时文件，防止灾备目录膨胀。
- 主要修改文件：Contracts `MetadataBackupDtos.cs`/`MessageTypes`、`MetadataBackupService`、`Program`、`IpcRequestDispatcher`、`DashboardViewModel`、`MaintenanceView`，以及 Worker/Playnite 测试。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `51/51`、Worker `127/127`、Playnite `211/211`；`validate-source.py`、XAML 结构、WPF 静态门禁 0 errors / 33 warnings / 153 info、五种窗口 `render-qa OK`。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`，插件日志记录 `0.6.70.0 loaded`，`worker-launch.log` 记录存储初始化与 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实维护中心点击导出后的包内容人工复核，以及从灾备包恢复数据库的演练。
- Commit：`e7e8fb2`。
- 下一项：继续 Layer B，从 `REPOSITORY-REBUILD-001` 备份仓库索引重建开始。

## 2026-08-13 DB-MIGRATION-001 历史数据库 Fixture 升级 Harness

- Task ID：`DB-MIGRATION-001`。
- 实现内容：新增可复用的 Worker 测试 Harness `DatabaseMigrationHarness`，在临时目录创建旧版 SQLite Fixture，然后运行当前 `SqliteStateStore.InitializeAsync` 完整迁移，并报告首次/重复初始化、缺失表、缺失列、各表行数和明确错误。覆盖旧库升级保留数据、全新数据库创建、非法旧 schema 失败报告三个场景。
- 核心设计选择：Harness 只操作临时数据库，绝不接触用户数据；旧 Fixture 覆盖历史上后加的列（`match_input_hash`、`session_id`、`archive_path`、`classification_state`、`shared_directory`、`if_already_running`、`risk_category`、`resolved_target_path` 等）和旧版 `backup_versions` 主键，验证真实迁移路径而非只测 `CREATE TABLE`。
- 主要修改文件：`tests/GameSaveCenter.Worker.Tests/Infrastructure/DatabaseMigrationHarness.cs`、`tests/GameSaveCenter.Worker.Tests/DatabaseMigrationHarnessTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `51/51`、Worker `126/126`、Playnite `211/211`；`validate-source.py`、XAML 结构、WPF 静态门禁 0 errors / 33 warnings / 153 info、五种窗口 `render-qa OK`。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`，插件日志记录 `0.6.70.0 loaded`，`worker-launch.log` 记录存储初始化与 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实旧版本用户数据库升级后的功能回归，以及大量历史数据迁移时长的人工复核。
- Commit：`187744b`。
- 下一项：继续 Layer B，从 `METADATA-BACKUP-001` GameSaveCenter 自身元数据灾备开始。

## 2026-08-13 INTEGRITY-001 全局完整性自检

- Task ID：`INTEGRITY-001`。
- 实现内容：新增 Worker 全局完整性自检与 IPC `integrity.check`。检查 SQLite `integrity_check`、外键、必需表、关键目录可写性、已配置的 Ludusavi/Rclone 可执行文件，以及备份归档、游戏工具和媒体索引指向的文件是否存在。维护中心诊断页新增“完整性自检”按钮和结果摘要。
- 核心设计选择：自检只读，不自动删除或修复任何数据；数据库问题标记为 Critical，文件/可执行文件缺失标记为 Warning；缺失路径只展示最多 20 个示例，避免大量记录刷屏。
- 主要修改文件：Contracts `IntegrityDtos.cs`/`MessageTypes`、`SqliteStateStore.Integrity.cs`、`IntegrityCheckService`、`Program`、`IpcRequestDispatcher`、`DashboardViewModel`、`MaintenanceView`，以及 Worker/Playnite 测试。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `51/51`、Worker `123/123`、Playnite `211/211`；`validate-source.py`、XAML 结构、WPF 静态门禁 0 errors / 33 warnings / 153 info、五种窗口 `render-qa OK`。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`，插件日志记录 `0.6.70.0 loaded`，`worker-launch.log` 记录存储初始化与 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实维护中心点击完整性自检后的结果展示，以及缺失文件/数据库异常场景的人工复核。
- Commit：`85cb884`。
- 下一项：继续 Layer B，从 `DB-MIGRATION-001` 历史数据库 Fixture 升级 Harness 开始。

## 2026-08-13 SAFE-MODE-001 安全模式

- Task ID：`SAFE-MODE-001`。
- 实现内容：新增全局“安全模式”设置并持久化到插件与 Worker 设置。开启后暂停自动退出备份、游玩中定时备份、自动媒体同步、自动工具启动、会话存档快照与保护提示、云端自动上传和云端自动重试；手动备份、手动媒体同步、手动工具启动、诊断与恢复仍可用。设置页新增开关，维护中心诊断页在开启时显示醒目提示，诊断摘要包含安全模式状态。
- 核心设计选择：安全模式只关闭“自动产生副作用”的路径，不封锁用户明确点击的手动操作；Worker 侧在 `GameSessionCoordinator`、`GameToolService`、`BackupOrchestrator`、`MediaSyncService` 和 `CloudRetryService` 同时设门禁，避免绕过 Playnite 事件的进程检测路径重新触发自动化。旧设置 JSON 缺字段时保持关闭。
- 主要修改文件：Contracts `WorkerSettingsDto`/`WorkerSettingsSnapshotDto`/`DashboardSnapshotDto`、`WorkerOptions`、`GameSessionCoordinator`、`GameToolService`、`BackupOrchestrator`、`MediaSyncService`、`CloudRetryService`、`DashboardService`、`IpcRequestDispatcher`、Playnite 设置与维护视图、诊断摘要，以及 Worker/Playnite 测试。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `51/51`、Worker `120/120`、Playnite `211/211`；`validate-source.py`、XAML 结构、WPF 静态门禁 0 errors / 33 warnings / 153 info、五种窗口 `render-qa OK`。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`，插件日志记录 `0.6.70.0 loaded`，`worker-launch.log` 记录存储初始化与 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实游戏中开启/关闭安全模式后验证不再触发自动备份与工具启动，以及设置页/维护提示在主题与 DPI 下的人工观感。
- Commit：`a70d73f`。
- 下一项：继续 Layer B，从 `INTEGRITY-001` 全局完整性自检开始。

## 2026-08-13 NOTIFY-LEVELS-001 通知级别收口

- Task ID：`NOTIFY-LEVELS-001`。
- 实现内容：补齐 `ImportantOnly/Summary/Verbose` 三级通知语义：`ImportantOnly` 仅显示失败/取消任务及警告/失败退出摘要；`Summary` 保持当前每次游戏退出只发一条聚合摘要；`Verbose` 在最终摘要之外逐条显示终态任务。设置页新增“通知级别”ComboBox，旧设置 JSON 缺字段时归一为 `Summary`。
- 核心设计选择：把级别决策抽成 Core 纯逻辑 `NotificationLevelPolicy`，插件只消费该决策；Session 任务无论级别都继续进入统一聚合器，避免破坏一次 Session 一条摘要的去重契约。枚举值从 1 开始，避免旧 JSON 缺字段时误解析成 `ImportantOnly`。
- 主要修改文件：`Contracts/Enums.cs`、`Core/Services/NotificationLevelPolicy.cs`、`Playnite/GameSaveCenterPlugin.cs`、`Playnite/Settings/GameSaveCenterSettings.cs`、`GameSaveCenterSettingsView.xaml`、Core 与 Playnite 测试、项目记忆与交接文档。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `51/51`、Worker `118/118`、Playnite `210/210`；`validate-source.py`、XAML 结构、WPF 静态门禁 0 errors / 33 warnings / 153 info、五种窗口 `render-qa OK`。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`，插件日志记录 `0.6.70.0 loaded`，`worker-launch.log` 记录存储初始化与 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实游戏退出触发三种级别时的实际通知观感，以及设置页主题/DPI/连续缩放人工复核。
- Commit：`d99b8ef`。
- 下一项：继续 Layer B，从 `SAFE-MODE-001` 安全模式开始。

## 2026-08-13 LAYER-A-AUDIT-001 / DIAGNOSTICS-001

- Task ID：`LAYER-A-AUDIT-001`、`DIAGNOSTICS-001`。
- 实现内容：修复多设备等价判断、Restore Readiness 大文件取消/Hash、分磁盘环境检查和重复 Manifest 防护；新增维护中心脱敏诊断包导出入口与 IPC。
- 核心设计选择：只有完整 Manifest 内容指纹相同才判定跨设备内容等价；仅大小/文件数相同保守标记未知分歧；诊断包限制日志/任务/审计数量和总大小，拒绝打包数据库、存档、媒体与凭据。
- 主要修改文件：`BackupContentFingerprint`、`DeviceConflictDetector`、`RestoreReadinessService`、`EnvironmentCheckService`、`FileManifestDiffService`、`DiagnosticsPackageService`、Contracts IPC DTO、`DashboardViewModel`、`MaintenanceView` 及对应 Core/Worker/Playnite 测试。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `47/47`、Worker `118/118`、Playnite `210/210`；`validate-source.py` 通过；WPF 静态门禁 0 errors / 33 warnings / 153 info（均为既有布局提示）；五种窗口 `render-qa OK`；一键开发安装与 Playnite 启动成功，日志确认插件 `0.6.70` 加载、Worker 初始化并进入 `Application started`。
- AUTO VERIFIED：上限/脱敏/排除敏感文件测试、指纹与恢复校验单测、源码/XAML/WPF/render-qa、真实构建/安装/启动。
- MANUAL QA REQUIRED：真实游戏 Restore/Undo、真实 Rclone 断网与凭据失效、真实双设备、多种 EXE/LNK/BAT/PS1、1000+ 游戏库、主题/DPI/连续缩放；未把自动化结果冒充这些验收。
- Commit：`cbd0cdb`（Layer A 审计修复）、`c6bfda2`（诊断包）。
- 下一项：继续 Layer B/C 任务；完整 38 项 Epic 尚未完成，通知级别随后已由 `NOTIFY-LEVELS-001` 收口。

## 2026-08-13 UI-QA-REAL-006 设置分类 Tab 实际裁切修复

- 用户反馈上一轮“底部安全区域”没有视觉变化；复核截图后确认不是安全区数值太小，而是 `TabItem` 的圆角 `Border` 直接作为模板根并开启 `ClipToBounds=True`，圆角 Chrome 被 `TabPanel` 的布局槽贴底裁平。
- `GscRedesignSettingsTabControl` 改为 `StackPanel` 承载 `TabPanel` 与真实的 `SettingsHeaderBottomSafetyZone` 占位元素；紧凑顶部横向模式隐藏该占位元素，保留原有滚动方向、选中逻辑和键盘导航。
- `GscRedesignSettingsTabItem` 改为不裁切的 `TabItemRoot` + 独立 Chrome；Chrome 顶部对齐并保留 2 DIP 底部安全边距，移除会放大边界裁切的 `ClipToBounds=True`，不改变 Header、Binding、FocusVisualStyle 或业务行为。
- RenderHarness 增加 `chromeBottom` / `chromeSafety` 几何门禁；最终 5 种窗口尺寸渲染通过，短窗口/滚动探针全部通过，设置页视觉复核确认卡片底部圆角不再被直线切平。
- 验证：XAML/source 门禁、`git diff --check`、Playnite `210/210`、`render-qa OK`。真实 Playnite 宿主的主题、DPI 和连续缩放仍需人工复核。

## 2026-08-13 UI-QA-REAL-005 首页与设置页几何兼容性修复

- 用户在 4K 全屏反馈首页“今日概览”没有贴在内容顶端、当前游戏卡空间紧张，以及设置页分类卡底部圆角仍像被切掉。
- `OverviewView` 将宽屏右侧摘要滚动面和内容面显式锚定顶部；Hero/当前游戏宽屏列从 `1.25* + 0.75*` 调整为 `1.1* + 0.9*`，保留原有堆叠断点、命令和绑定。
- `GscRedesignSettingsTabControl` 在共享 `TabPanel` 外增加 `SettingsHeaderItemsHost` 底部安全区，设置顶部对齐、像素对齐和布局取整，避免 `ScrollViewer` viewport 在最后一张分类卡底部裁剪圆角。
- RenderHarness 修复设置页入口动画导致的空白截图，并加入 Overview secondary top delta、当前游戏宽度比、Settings 最后一张 Tab 底部几何门禁。
- 验证：`python scripts/validate-source.py`、WPF 静态门禁、`git diff --check`、`render-qa` 全绿；隔离 Release 全量测试 Core `42/42`、Worker `117/117`、Playnite `210/210`。真实 Playnite 宿主主题/DPI/连续缩放仍需用户复核。

## 2026-08-13 DEV-INSTALL-007 兼容未发现 Playnite 路径

- 用户反馈另一台机器在 Playnite 使用自定义/便携安装路径时，一键入口在构建前报 `TrustedPlayniteExecutables` 空数组绑定错误；实际失败点是安装前进程处理，不是项目编译失败。
- `scripts/dev-install-run.ps1` 现在增加 App Paths、PATH 和规范化路径发现，并允许可信 Playnite 候选为空；没有运行中的 Playnite 时继续构建、打包和安装，仅提示安装后无法自动启动。指定了不存在的 `-PlayniteExecutable` 时给出直接错误。
- 如果 Playnite 仍在运行但没有任何可确认的可信路径，仍然停止安装并要求用户退出 Playnite 或显式指定路径，保留防止误杀未知进程的安全边界。
- 根目录 `GameSaveCenter-Run.cmd` 与源码门禁同步到 `DEV-INSTALL-007`。
- 验证要求：PowerShell AST、`scripts/validate-source.py`、`git diff --check`；真实另一台机器的自定义 Playnite 路径和无 Playnite 进程场景仍需用户手工执行一键入口确认。

## 2026-08-13 DEV-INSTALL-006 无窗口 Playnite 残留回收

- 真实复现一键安装失败：Playnite PID 48188 收到正常退出请求后 20 秒仍未退出；该进程属于当前会话、路径为已发现的 `D:\software\Playnite\Playnite.DesktopApp.exe`，且 `MainWindowHandle=0`，因此没有窗口可供 `CloseMainWindow()` 再次关闭，并非管理员权限问题。
- 安装器继续优先正常退出；超时后仅结束当前会话、路径与本次 Playnite 候选完全一致、且已无主窗口的残留实例。路径不可读、路径不可信、跨会话或仍有主窗口时停止安装，禁止按名称广泛终止。
- 停止操作处理 PID 在检查与终止之间自然退出的竞态，不再把已退出进程误报为失败。根目录入口同步要求 `DEV-INSTALL-006`，源码门禁锁定上述安全条件。
- 验证：PowerShell 语法与 `scripts/validate-source.py` 通过；同一 PID 48188 场景下 `DEV-INSTALL-006` 自动识别并结束无窗口残留，随后完整 Release 构建 0 警告/0 错误，Core 42/42、Worker 117/117、Playnite 203/203，全量打包、普通用户安装和启动成功。
- 安装 DLL 产品版本为 `0.6.70+4125a5448b1d903c1122d6ba596b8ca31597a714`，证明安装的是执行本阶段前的当前 HEAD 而非旧 `c01e9d5` 包；11:14:33 的 Playnite 日志确认插件从用户扩展目录加载，11:14:38 的 Worker 日志确认存储初始化、过期任务整理与 `Application started`，Playnite/Worker 进程持续运行。

## 2026-08-13 DEV-INSTALL-005 构建输出隔离、安装暂存隔离与批量保护确认

**问题根因：**

- 用户提供的日志中 Contracts/Core/Worker/Playnite 编译均已成功；真正失败的是 Playnite.Tests 与 Worker.Tests 覆盖标准 `bin\Release` 输出时被拒绝访问。
- 当前机器仍有旧的 `dotnet/testhost` 或 Worker 持有仓库标准 `bin/obj` 文件。该类文件锁不应通过默认 UAC 或按进程名强杀解决。

**实现内容：**

- `scripts/build.ps1` 增加可选 `-OutputRoot`，通过 `GscBuildOutputRoot` 为每个项目生成独立的 `bin/obj`，并关闭 MSBuild 节点复用；普通 IDE/CI 未传参数时仍使用原路径。
- `scripts/dev-install-run.ps1` 每次使用 `artifacts\dev-build\<Configuration>\<guid>`，不再清理正在使用的标准输出；安装临时目录改为唯一 GUID 路径。安装器修订号为 `DEV-INSTALL-005`，默认不请求管理员权限。
- `scripts/package.ps1` 支持从隔离输出打包，并让 Worker 的运行时恢复/发布也使用同一隔离根目录。
- Overview 的批量“启用推荐自动保护”增加修改前预览确认；只提交已选且可选游戏，不执行备份/恢复、不覆盖其他策略设置。

**验证结果：**

- Release 隔离构建：0 warnings / 0 errors；Core `37/37`、Worker `81/81`、Playnite `202/202`。
- `validate-source.py`、XAML 结构检查、WPF 静态校验与 `git diff --check` 通过；WPF 静态校验为 0 errors，剩余 warnings 是既有布局审查提示。
- `scripts/package.ps1 -SkipBuild -BuildOutputRoot ...` 成功生成 `artifacts\GameSaveCenter-0.6.70.pext`。
- 使用正常桌面文件权限执行 `scripts/dev-install-run.ps1 -Configuration Release -NoStart -SkipClean` 成功，无 UAC；安装报告确认版本 `0.6.70`、插件 DLL `0.6.70.0`。
- 真实 Playnite 启动验证成功：`playnite.log` 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`，插件日志记录加载 `GameSaveCenter.Playnite 0.6.70.0`；`worker-launch.log` 记录 Worker 初始化完成并进入 `Application started`。

**未替代的人工验收：**

- 真实用户交互、主题/DPI/键盘焦点、连续缩放，以及真实 Rclone、多设备冲突、游戏恢复/撤销、EXE/LNK/BAT/PS1 启动项、900+ 游戏库仍需人工验收；本阶段不把静态测试或一次宿主启动写成全部产品验收完成。

## 2026-08-13 DEV-INSTALL-004 回退默认提权并按归属回收 Worker

**实现内容：**

- 移除一键安装器启动时的管理员检测、UAC 自重启和安装过程中的管理员停止逻辑；普通用户运行 `.cmd` 不再被强制要求授权。
- 安装前先对现有 Playnite 窗口调用 `CloseMainWindow()`，等待 Playnite 的 `OnApplicationStopped` 执行既有 `WorkerLauncher.StopOwnedWorker()`，再继续覆盖安装。
- Playnite 退出后若仍有 Worker，只允许处理路径明确位于当前扩展目录 `Worker\GameSaveCenter.Worker.exe` 的残留进程；路径不可读取或属于其他扩展时直接停止安装，避免按进程名误杀。
- 安装器修订号更新为 `DEV-INSTALL-004`，根目录入口同步检查该版本，防止旧入口继续运行已废弃的提权代码。

**验证结果：**

- PowerShell AST 解析、`git diff --check` 通过。
- 当前会话真实执行 `DEV-INSTALL-004` 时没有出现 UAC；日志确认先完成“Playnite 已退出，等待插件回收 Worker”，随后才进入 NuGet 恢复。
- 本次执行后 Playnite 与 Worker 均已退出；后续 NuGet 恢复因当前机器 SDK 缺失 `Microsoft.NET.SDK.Workload*Locator` 目录而退出码为 1，与进程处理无关。用户重启后的上一轮构建/测试/安装已成功。

**结论：**

- `GameSaveCenter.Worker` 不是用户手动启动的陌生进程；`worker-launch.log` 记录它由已安装的 GameSaveCenter 插件从扩展目录启动。残留通常来自 Playnite 异常退出或旧权限上下文，安装器现在先走正常退出回收链路，不再把整个安装提权。

## 2026-08-13 DEV-INSTALL-003 安装入口版本识别

**实现内容：**

- 一键安装 PowerShell 脚本输出自身的绝对路径和安装器修订号 `DEV-INSTALL-003`，避免旧日志/旧脚本错误地被当作当前修复结果。
- `GameSaveCenter-Run.cmd` 启动前检查脚本是否包含当前修订号；从旧副本或错误目录启动时立即停止并显示期望路径，不再执行旧的进程停止逻辑。
- 重新确认 PID 3896 是无父进程的孤儿 Worker；当前执行会话对 `Stop-Process`、`taskkill /F` 和命名管道请求均无权/无响应，不能把当前环境的强制结束结果伪造为成功。

**验证结果：**

- 当前仓库 `scripts/dev-install-run.ps1` 第 147 行是扩展路径筛选，不是用户日志中的旧权限异常；旧错误文本不再存在于当前脚本。
- PowerShell AST 解析与 `git diff --check` 通过；入口校验已加入。
- 该阶段已被 DEV-INSTALL-004 的正常退出方案取代；保留旧日志证据，不能作为当前安装器行为判断依据。

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
# 2026-08-13 可靠性闭环最终验收与缺口修复

**Task IDs：** RELIABILITY-RESTORE-001、HEALTH-001、PROTECTION-001、POLICY-001、ONBOARDING-001、GAME-TOOL-003/004、SMART-PROTECT-001/002、BACKUP-DIFF-001、BACKUP-GUARD-001、NOTIFY-001、MULTI-DEVICE-001、RCLONE-RELIABILITY-001

**实现与修复：**

- 恢复编排在部分写入、写后异常和 post-validation 失败时都恢复锁定 PreRestore；回滚失败输出稳定错误并进入人工介入状态。灾难演练增至 10 个临时目录用例。
- 多设备从机器名迁移到稳定不透明 DeviceId；保留旧 sidecar/旧决策兼容，便携设置不会复制设备身份，旧设置首次加载会生成并保存新身份。
- 备份异常保护加入 Off/Normal/Strict，重要游戏模板默认为 Strict；持久化 Manifest 的大量文件删除参与保护判定。
- GameTool 将已有实例决策抽成纯策略并补真实测试自有 EXE 演练；反作弊自动启动分类独立可测。
- Rclone 增加执行级白名单，完整命令行不再进入日志；补中断任务、取消子进程和禁止危险命令测试。
- 补健康云开关/异常、A3/B3 分叉、三种人工冲突决策、设置设备身份迁移等验收测试。
- 新增 `docs/ai/RELIABILITY_CLOSURE_AUDIT.md`，逐项列出 AUTO VERIFIED 与 MANUAL QA REQUIRED。

**核心设计选择：**

- 不新增第 15 项功能，不重写 Worker/恢复/GameTool；所有修补沿用现有 DTO、SQLite、Task Center 和恢复链。
- 云端目录身份与展示机器名分离；用户可迁移显示名，但同一安装保持稳定 ID。
- 异常备份允许保存，Retention 保护用户 Lock、PreRestore 和已验证健康点；仍不自动执行删除。
- UI 只在既有 Save Center 策略区加入共享样式 ComboBox，并保留 Automation 名称、键盘访问与响应式滚动结构。

**自动验证：**

- Core 42/42、Worker 117/117、Playnite 203/203。
- Release build 0 warnings / 0 errors；source、XAML、diff、WPF render-qa 全通过。
- 一键脚本普通权限完成构建、测试、打包、安装、版本校验并启动 Playnite；宿主日志确认 0.6.70 加载，Worker 运行并完成 IPC。

**MANUAL QA REQUIRED：**

真实游戏 Restore/Undo、真实 Rclone 断网/凭据/部分上传、真实两台设备、真实启动项、1000+ 游戏库、完整主题/DPI/键盘/连续缩放。详见最终审计文档。

**提交：** `dd39229`（可靠性安全缺口）；本审计文档提交见紧随其后的 docs commit。

**下一项：** 本轮无第 15 项；只接受人工 QA 反馈或明确的新任务。

## 2026-08-13 UI-QA-REAL-002 2K 首页状态区与设置/维护布局修复

**实现内容：**

- 修复首页“今日工作台”在 2K 最大化宽度下状态灯被挤到窄列、只剩竖向圆点的问题；状态胶囊改为标题下方的全宽第二行，并保留响应式内边距。
- 维护中心首次使用环境检查改为响应式 `UniformGrid`：宽窗口 3 列、中等窗口 2 列、窄窗口 1 列；统一卡片高度和横向拉伸，消除最后一行参差不齐的胶囊排列。
- 修复共享文本框内容宿主的垂直对齐和高度，统一为居中、42 DIP；设置页分类卡片增加底部安全间距并收敛圆角，避免窗口底部裁切和“被削掉”的视觉效果。
- 补充渲染夹具中的环境检查样例数据，并新增 UI 布局回归测试，确保后续修改不会复现这三类问题。

**核心设计选择：**

- 只修复现有共享布局和模板，不新增页面、不改变绑定/命令、滚动骨架或 DataGrid 虚拟化。
- 首页状态区使用显式 Grid 行而非依赖窄列 WrapPanel；维护卡片使用可随可用宽度变化的 UniformGrid；设置输入框继续复用现有共享模板。

**主要修改文件：**

- `src/GameSaveCenter.Playnite/Views/OverviewView.xaml`、`OverviewView.xaml.cs`
- `src/GameSaveCenter.Playnite/Views/MaintenanceView.xaml`、`MaintenanceView.xaml.cs`
- `src/GameSaveCenter.Playnite/Themes/WpfUiProduction.xaml`、`DesignTokens.xaml`、`Redesign.xaml`
- `tests/GameSaveCenter.RenderHarness/FakeDashboardData.cs`
- `tests/GameSaveCenter.Playnite.Tests/UiLayoutRegressionTests.cs`
- `docs/ai/PROJECT_MEMORY.md`、`docs/ai/WORKLOG.md`

**测试结果：**

- XAML 结构校验通过；Release 构建 0 warning / 0 error。
- Core 42/42、Worker 117/117、Playnite 206/206。
- `render-qa.ps1` 在 1040×700、1280×720、1366×768、1600×900、1920×1080 全部通过，包含首页与维护中心首次使用样例渲染。
- `dev-install-run.ps1 -Configuration Release` 构建、打包、安装校验成功；真实 Playnite 扩展日志确认 `GameSaveCenter 0.6.70.0` 加载，Worker 进程正常运行。

**验证边界：**

- `AUTO VERIFIED`：源码/XAML、Release 构建、Core/Worker/Playnite 测试、五种窗口尺寸渲染、开发安装、真实宿主插件日志和 Worker 启动。
- `MANUAL QA REQUIRED`：用户实际 2K 最大化窗口的视觉确认、Settings 全页滚动、主题/DPI 切换和连续缩放；本阶段没有伪造“已完成人工点击验收”。

**提交：** `a2c0f8d`（UI 修复、回归测试与长期记忆更新）。本条 hash 回填由随后独立的文档提交完成。

**下一项：**等待用户人工 QA 反馈；不新增本轮范围外功能。

## 2026-08-13 UI-QA-REAL-003 首页、工具设置与媒体列表布局补充

**实现内容：**

- 首页“最近 30 天玩过的游戏”卡片将两个操作按钮移到摘要下方的独立响应式行，避免右侧窄栏与标题/统计文字争抢空间。
- 修改器中心为启动延迟字段补充“启动延迟 / 秒”标签，保留原有 `LaunchDelaySeconds` 绑定，并使用 34 DIP 紧凑输入框，消除单独显示数字 `8` 的歧义和过大的输入框观感。
- 媒体中心当前媒体列表显式设置内容水平/垂直拉伸、顶部对齐，并使用顶部对齐的虚拟化面板，修复表头下方到第一行之间的大段空白。
- 补充三项 UI 布局回归测试，覆盖首页操作行、媒体列表首行定位和启动延迟单位/紧凑高度。

**核心设计选择：**

- 只调整既有卡片内部布局与共享控件尺寸，不新增页面，不改变命令、绑定、业务流程或媒体列表虚拟化骨架。
- 媒体列表仍使用 `ListBox` 与 `VirtualizingStackPanel`，只修正内容对齐，避免以固定高度或外层嵌套滚动器掩盖布局问题。

**主要修改文件：**

- `src/GameSaveCenter.Playnite/Views/OverviewView.xaml`
- `src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml`
- `src/GameSaveCenter.Playnite/Views/TrainerCenterView.xaml`
- `tests/GameSaveCenter.Playnite.Tests/UiLayoutRegressionTests.cs`

**测试结果：**

- `check-xaml.ps1` 通过（13 个 XAML 文件）。
- Release 构建 0 warning / 0 error；Core 42/42、Worker 117/117、Playnite 209/209。
- `render-qa.ps1` 在 1040×700、1280×720、1366×768、1600×900、1920×1080 全部通过；渲染证据确认首页操作区不再拥挤、媒体首行紧贴列表顶部、工具设置显示带单位的紧凑延迟字段。
- `dev-install-run.ps1 -Configuration Release` 普通权限完成构建、打包、安装校验并启动 Playnite；扩展日志确认 `GameSaveCenter 0.6.70.0` 加载，Worker 从当前安装目录正常运行。

**验证边界：**

- `AUTO VERIFIED`：源码/XAML、Release 构建、Core/Worker/Playnite 测试、五种窗口尺寸渲染、开发安装、真实宿主扩展日志和 Worker 进程。
- `MANUAL QA REQUIRED`：用户实际 2K 最大化窗口、主题/DPI 切换、Settings 全页连续缩放，以及真实媒体数据量下的滚动体验；未将自动渲染冒充为人工验收。

**提交：**`3657145`（代码、回归测试与长期记忆）；本条 hash 回填由随后独立的文档提交完成。

**下一步：**等待用户人工视觉反馈；不引入本轮范围外功能。

## 2026-08-13 UI-QA-REAL-004 设置页分类卡圆角裁切修复

**问题根因：**

- 设置页左侧分类卡并不是圆角值失效，而是固定 232 DIP 的 `SettingsHeaderScroller` 在启用纵向滚动条后，实际内容 viewport 变窄；`TabItem` 仍占满原宽度，卡片右侧圆角被滚动区域直接裁掉，视觉上像底部或边缘被削平。

**实现内容：**

- 左侧分类滚动槽从 232 DIP 扩展到 248 DIP，为滚动条和卡片圆角保留安全宽度；分类卡自身继续使用共享 14 DIP 圆角。
- 分类卡增加短高度响应式：可用高度低于 760 DIP 时收紧高度和底部间距，避免五个分类卡在设置宿主 viewport 中被挤压。
- 左右设置滚动面增加明确的底部安全留白，顶部布局和现有设置内容不变；卡片 Chrome 开启自身边界裁剪，避免内容越界破坏圆角。
- 新增设置页圆角/短高度布局回归测试，并更新已有设置布局断言。

**主要修改文件：**

- `src/GameSaveCenter.Playnite/Themes/Redesign.xaml`
- `src/GameSaveCenter.Playnite/Settings/GameSaveCenterSettingsView.xaml.cs`
- `tests/GameSaveCenter.Playnite.Tests/SettingsAndAutoSelectSourceTests.cs`
- `tests/GameSaveCenter.Playnite.Tests/UiLayoutRegressionTests.cs`

**测试结果：**

- `check-xaml.ps1` 通过（13 个 XAML 文件）。
- Release 构建 0 warning / 0 error；Core 42/42、Worker 117/117、Playnite 210/210。
- `render-qa.ps1` 在 1040×700、1280×720、1366×768、1600×900、1920×1080 全部通过；设置布局探针确认左侧滚动槽为 248 DIP，短高度分类卡进入 60 DIP 档位，页面滚动仍为 Auto。
- `dev-install-run.ps1 -Configuration Release` 构建、打包、安装校验成功并启动 Playnite；真实扩展日志确认 `GameSaveCenter 0.6.70.0` 加载，Worker 从当前安装目录正常运行。

**验证边界：**

- `AUTO VERIFIED`：源码/XAML、Release 构建、Core/Worker/Playnite 测试、五种窗口尺寸渲染、开发安装、真实宿主扩展日志和 Worker 进程。
- `MANUAL QA REQUIRED`：用户实际 Playnite 设置页在 2K/DPI 缩放下的最终视觉确认，以及不同主题下滚动条与圆角之间的观感；本阶段不把离屏探针冒充为人工点击验收。

**提交：**`a9d7e58`（代码、回归测试与长期记忆）；本条 hash 回填由随后独立的文档提交完成。

**下一项：**等待用户人工视觉反馈；不引入本轮范围外功能。

## 2026-08-16 UI-206 Overview UiLab 页面级迁移

**实现内容：**

- 将生产首页重排为 UiLab 的真实阅读层级：顶部 Hero/当前游戏横向区域与六项指标占满工作区，下方才进入“最近活动 + 全局活动 / 风险与提醒 + 需关注事项”双栏。
- 右侧下栏采用 330 DIP 固定关注栏，窄窗口自动下移；保留生产页真实 Snapshot、Protection、AttentionFindings、命令、绑定和页面级滚动，不引入 UiLab 演示用顶部颜色按钮或演示滚动条。
- 统一 Overview 阅读卡、活动容器、全局活动圆角表头和低对比度分隔线；共享 `DataGridColumnHeader` 增加圆角和安全内边距，降低宿主截图中的尖锐边框感。
- 修复新关注行模板的 DataTemplate 触发器层级，并补充页面骨架/固定侧栏的源码回归断言；渲染 harness 改为验证侧栏与下方活动行对齐。

**验证结果：**

- `validate-source.py`、`check-xaml.ps1`、WPF 静态审查：通过，无新增错误。
- Playnite Release 构建：0 warning / 0 error；Playnite 测试 303/303 通过。
- 离屏 Overview 在 1040×700 至 3840×2160、多主题 Light/Dark 与 resize transition 下完成渲染；最终报告确认宽屏侧栏相对最近活动行偏移 0 DIP、无 Overview 新问题。
- 全量 render harness 仍报告 Save/Media 窄尺寸表格视口低于 236 DIP 的既有门禁项；这些问题不属于本阶段 Overview 改动，未伪报为全量通过。

**验证边界：**

- `AUTO VERIFIED`：源码/XAML、生产编译、Playnite 测试、Overview 多尺寸/双主题离屏渲染、布局对齐探针。
- `MANUAL QA REQUIRED`：真实 Playnite 宿主中的最终 DPI、Follow/高对比度主题、键盘焦点和连续缩放；本阶段未将离屏渲染冒充为人工验收。

**提交：**本阶段代码、测试与文档需由 Agent 独立 commit 并 push 到 `main`。

## 2026-08-16 UI-208 Overview 全局活动业务列表对齐 UiLab

**问题根因：**

- UiLab 的“全局活动”是业务阅读列表：左侧类型胶囊，中间对象/事件两行，右侧结果和时间；生产首页之前仍保留了额外的 DataGrid 式表头和图标列，所以即使卡片外观接近，页面信息层级仍明显不同。

**实现内容：**

- 移除 Overview 全局活动专用表头，改为 UiLab 同款四列业务行；`Activities`、`KindDisplay`、`ResultDisplay`、虚拟化 ItemsControl 和页面级滚动保持不变。
- 类型与结果胶囊继续按真实 `Result` 使用成功、警告、失败语义色；窄窗口只收紧类型/时间列，不引入 demo 内部滚动条。
- 更新布局回归测试，确保四列、中文摘要截断、状态胶囊和时间列间距不会回退到旧表格式。

**验证结果：**

- `validate-source.py`、`check-xaml.ps1`、WPF 静态审查通过（无新增错误）。
- Playnite Release 构建 0 warning / 0 error；Playnite 测试 303/303 通过。
- RenderHarness v6/v6.2 首页宽/窄截图通过；全局活动业务行无文字重叠，页面滚动和真实绑定未改变。

**验证边界：**

- `AUTO VERIFIED`：源码/XAML、生产编译、Playnite 测试、Overview 宽/窄离屏截图。
- `MANUAL QA REQUIRED`：真实 Playnite 宿主中的 2K/DPI、Follow/高对比度主题、键盘焦点和连续缩放；本阶段未把离屏截图冒充宿主像素真值。

## 2026-08-16 UI-209 共享 DataGrid 表头对齐 UiLab

**问题根因：**

- UiLab 的 `LabGridColumnHeader` 以透明表头容器承载弱分隔线；生产虽然已有圆角列头模板，但 `DataGridColumnHeadersPresenter` 仍给整行连续铺设表头底色，列头之间的圆角间隙因此不可见，视觉上接近尖锐矩形条。

**实现内容：**

- 将生产共享 `DataGridColumnHeadersPresenter` 背景改为透明，保留每个 `DataGridColumnHeader` 自身的圆角、低对比度填充和分隔线。
- 不改变表头高度、列宽、排序 glyph、`VirtualizingPanel.ScrollUnit=Item`、行/列虚拟化、页面滚动条或页内真实绑定。

**验证结果：**

- `validate-source.py`、`check-xaml.ps1` 通过。
- Playnite Release 构建 0 warning / 0 error；Playnite 测试 303/303 通过。
- RenderHarness v6/v6.2 表格及首页宽/窄截图通过；Maintenance 表头圆角间隙可见，无白块或文字重叠。

**验证边界：**

- `AUTO VERIFIED`：源码/XAML、生产编译、Playnite 测试、离屏截图。
- `MANUAL QA REQUIRED`：真实 Playnite 宿主最终 DPI、Follow/高对比度主题、键盘焦点和连续缩放。

## 2026-08-16 UI-210 Media 统计带与响应式布局对齐 UiLab

**问题根因：**

- UiLab Media 顶部是一个连续圆角统计带，默认阅读当前游戏媒体；生产之前是四张分离统计卡且默认落在待归类页，首屏结构差异明显。
- Media 在短窗/缩放时把列表视口压到 180-200 DIP；从窄窗恢复宽窗后，Inspector 还可能保持折叠状态，造成宽屏右侧详情消失。

**实现内容：**

- 将 Media 四项统计改为单一 `GscRedesignSectionCard` 内的四段 value → caption → supporting line 结构，保留真实 `MediaSummary`/`Snapshot` 绑定。
- 默认选择“当前游戏媒体”标签，使首屏阅读顺序与 UiLab 一致；待归类表格和归类命令仍可直接切换。
- 常规 700-720 DIP 窗口将媒体/收件箱视口维持在 236 DIP；极短窗口才使用 180 DIP 紧凑下限。
- 宽屏恢复时重新显示已选媒体 Inspector；窄屏仍使用现有“查看媒体详情”折叠按钮，未引入 UiLab 滚动条或关闭 ListBox/DataGrid 虚拟化。

**验证结果：**

- `validate-source.py`、`check-xaml.ps1` 通过。
- Playnite Release 构建 0 warning / 0 error；Playnite 测试 303/303 通过。
- 重建 RenderHarness 后，Media Light/Dark、1040/1100/1366/2560 多尺寸和 2560→1100→2560 resize transition 通过；Media 不再产生视口不足或 Inspector 恢复失败门禁。
- 全量 render-qa 仍只剩 Save 候选表窄窗 `<236 DIP` 的既有门禁，未把它误记为本阶段已解决。

**验证边界：**

- `AUTO VERIFIED`：源码/XAML、生产编译、Playnite 测试、Media 多主题/多尺寸离屏渲染。
- `MANUAL QA REQUIRED`：真实 Playnite 宿主中的最终 DPI、Follow/高对比度主题、键盘焦点、连续缩放和大媒体库滚动。

## 2026-08-16 UI-211 Save 候选表窄窗视口收口

**问题根因：**

- Save 的响应式高度公式沿用了过大的 `height - 520` 扣减，常规 700-720 DIP 窗口把候选表压到 205-225 DIP，触发全量 render-qa 的 `<236 DIP` 门禁。

**实现内容：**

- 将 Save 历史/候选表的常规视口公式收敛到 `Math.Max(180d, Math.Min(252d, height - 464d))`；700-720 DIP 保持 236 DIP，真正短窗才降至 180 DIP。
- 保留 Save 的生产 DataGrid、Item scrolling、行/列虚拟化、Inspector 抽屉和页面命令，不迁移 UiLab 的滚动条模板。

**验证结果：**

- `validate-source.py`、`check-xaml.ps1` 通过。
- Playnite Release 构建 0 warning / 0 error；Playnite 测试 303/303 通过。
- 全量 RenderHarness `render-qa OK`：7 页面、Light/Dark、1040/1100/1366/2560、多尺寸和 2560→1100→2560 resize transition 全部通过。

**验证边界：**

- `AUTO VERIFIED`：源码/XAML、生产编译、Playnite 测试、全量离屏渲染。
- `MANUAL QA REQUIRED`：真实 Playnite 宿主最终 DPI、Follow/高对比度主题、键盘焦点和连续缩放。

## 2026-08-16 UI-212 Media 缩略图网格迁移并保留生产虚拟化

**问题根因：**

- UiLab 当前媒体页是固定 164 DIP 卡片、96 DIP 缩略图的网格；生产之前是横向三列信息行，因此即使统计带、Inspector 和页签已经接近 Demo，媒体主体仍明显不像样板。
- Demo 使用普通 `WrapPanel`，直接复制会一次性生成大媒体库的所有缩略图，也会把 Demo 的滚动条模板带入生产，和项目的大列表性能/宿主兼容约束冲突。

**实现内容：**

- 新增 `Controls/VirtualizingWrapPanel`：按固定卡片尺寸计算列/行，只生成可见区附近的 ListBoxItem，实现 `IScrollInfo` 的行、页、鼠标滚轮和 `MakeVisible`，并兼容 Recycling generator。
- Media 当前游戏列表改为该虚拟化网格；卡片按 Demo 迁移缩略图、底部渐变、录像标识、收藏标识、文件名、拍摄时间、云端状态和圆角选中描边。
- 继续使用生产 `ListBox`/`ScrollViewer`/`ScrollBar`，保留 `MediaView`、Extended selection、Inspector 抽屉、真实打开/收藏/备注/归类命令和窄窗详情按钮。
- 为首次挂载时生成器尚未就绪的 WPF 测量时序增加保护；空集合、Reset 和宽窄窗口切换不会抛异常。

**验证结果：**

- `validate-source.py`、`check-xaml.ps1`、WPF 静态审查通过（0 error；已有滚动测量提示和硬编码颜色 info 保持历史基线）。
- Playnite Release 构建 0 warning / 0 error；Playnite 测试 303/303 通过。
- RenderHarness 全量 `render-qa OK`：7 页面、Light/Dark、1040/1100/1366/2560、多尺寸和 2560→1100→2560 resize transition 全部通过；Media 网格截图已输出到 `artifacts/ui-qa/media-grid-migration-v3`。

**验证边界：**

- `AUTO VERIFIED`：源码/XAML、生产编译、Playnite 测试、离屏卡片网格、生产滚动条和响应式过渡。
- `MANUAL QA REQUIRED`：真实 Playnite 宿主中的实际缩略图解码、超大媒体库滚动手感、最终 DPI、Follow/高对比度主题、键盘焦点和连续缩放；本阶段没有把离屏假数据当作真实宿主验收。
