# GameSaveCenter 当前事实入口

> 更新时间：2026-09-10。本文是新一轮开发的短入口；历史细节仍保留在 [`PROJECT_MEMORY.md`](PROJECT_MEMORY.md)、[`WORKLOG.md`](WORKLOG.md) 和 [`DEVELOPMENT_HANDOFF.md`](../DEVELOPMENT_HANDOFF.md)，但与本文冲突时以本文和最新代码为准。

## 2026-09-10 当前候选包已按最新生产源提交重新生成

- `scripts/package.ps1` 已从生产源提交 `248d28e` 在隔离目录完成 Release 构建、测试和 Worker 发布；Core `76/76`、Worker `310/311`（1 skip）、Playnite `427/490`（63 skip），0 失败，构建 0 warning/0 error。
- 六份程序集身份统一为 `0.6.73+248d28eff8c595516a803f8db356952cef54c166`；[`.pext`](../../artifacts/GameSaveCenter-0.6.73.pext) 和 [`.zip`](../../artifacts/GameSaveCenter-0.6.73-playnite.zip) 均为 `43,837,966` 字节，SHA-256 均为 `628F34B01C478CD30A26260650703C8B77F558561E103D78B24C8A390949DDD3`。
- `248d28e` 为活动游戏选择器共享按钮样式显式清空 `ContentTemplate`，并补充回归断言，防止宿主文本模板把复合 `Grid` 显示成 `System.Windows.Controls.Grid`；此前首页活动行、云端队列整卡及 Dashboard 顶部复合按钮修复仍保留。
- 当前干净离屏 RenderHarness 报告为 [`.tmp/render-qa-gamecontext-clean-20260910/render-qa-report.txt`](../../.tmp/render-qa-gamecontext-clean-20260910/render-qa-report.txt)，报告提交 `248d28e`、`WorkingTreeClean: True`、`render-qa OK`；双主题/多尺寸/resize、云端队列筛选文字、完整壳层背景层和 Media 页尾可达性均通过。该报告不替代真实 Playnite/FusionX 宿主验收。
- 临时构建目录和打包 staging 目录已清理；包未安装真实 Playnite。真实宿主/FusionX、用户主题、DPI、物理点击和视频复测仍待 L31 条件恢复。
- 包后独立门禁：`validate-source.py` 通过，XAML `19/19` 通过，`git diff --check` 通过。
- 本轮 WPF 静态审查为 `0 errors / 22 warnings / 172 info`；warnings/info 为既有 Canvas、滚动容器和颜色令牌提示，未新增 error。
- 2026-09-10 在新增 `GameContextButtonKeepsCompositeGridContentThroughItsRuntimeTemplate` 运行时测试后重新执行 `dotnet test GameSaveCenter.sln -c Release --no-restore -m:1`：Core `76/76`、Worker `310/311`（1 skip）、Playnite `428/491`（63 skip），失败 `0`；此前 `AsyncThumbnailLoader` 的 `120`/`122` 并发污染本次未再复现。该测试实际解析并套用了选择器模板，确认 `ContentPresenter` 保留原始 `Grid` 内容。
- L32 链接审计：扫描本地 Markdown 链接 `122` 条，缺失 `0`；当前候选 `.pext/.zip` 均存在。已移除旧工作日志中指向已清理一次性截图的失效链接。

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
