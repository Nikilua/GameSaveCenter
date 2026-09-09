# 2026-09-09 证据索引与验收边界

当前基线：`main`，当前 HEAD 为 `8657654`（生产视觉修复源提交为 `248d28e`，首页交互测试为 `4001d9d`，生产首页修复为 `1fdd15e`，缩略图测试隔离为 `c0197e5`，Media 离屏门禁收口为 `d777e65`）。当前源码回归为 Playnite `428/491`（63 skip、0 fail）；新增活动游戏选择器共享按钮的视觉内容模板防护、静态断言与 STA WPF 运行时模板测试，并保留首页活动行/云端整卡和 Dashboard 顶部复合按钮的 STA/静态验证。当前候选包构建提交为 `8657654`，程序集身份为 `0.6.73+8657654310d99e349c120f3bb0484b65ac9f4dc2`，两个包均 `43,837,912` 字节、SHA-256 `E136B5C6465A5A8933C72B0F9707CFF64D91A02D35115B9A007BF6A749B62AB0`。当前干净 RenderHarness 报告为 [`.tmp/render-qa-gamecontext-clean-20260910/render-qa-report.txt`](../../.tmp/render-qa-gamecontext-clean-20260910/render-qa-report.txt)，报告提交元数据为 `248d28e`，不是宿主安装包；`8657654` 只改测试文件，因此该生产 XAML 报告仍适用。L42 运行基线及构建身份为 `398a6f0` / `0.6.73+398a6f095718e2827c3e8bd2a19bbbb5525c1f16`。L41/L42 均未捕获嵌入 Dashboard，因此当前有效的真实嵌入表格回放仍是 L39 的 `0.6.73+a9b8bcec0c05f7d548d8160119b2b29e9698b1ab`。窗口级滚动探针 canonical 报告仍为 `ea18b11`。本索引只汇总已有证据，不把程序化回放扩大为物理滑块/视频结论。

## 当前阶段

### 2026-09-10 游戏选择器视觉内容模板防护与发布证据

`248d28e` 为 `GscRedesignGameContextButton` 显式清空 `ContentTemplate`，覆盖生产壳层 `GameContextButton` 与 Dashboard 紧凑选择器两个复合 `Grid` 内容实例；回归断言验证共享样式、两个实例和内容呈现器均存在。新增 STA WPF 运行时测试实际套用模板并确认 `ContentPresenter` 保留原始 `Grid` 内容。静态定向测试与运行时定向测试均为 `1/1`，当前完整 Playnite `428/491`（63 skip、0 fail），Release 构建 `0 warning/0 error`。最新候选包已从 HEAD `8657654` 生成，身份为 `0.6.73+8657654310d99e349c120f3bb0484b65ac9f4dc2`，两个包均 `43,837,912` 字节、SHA-256 为 `E136B5C6465A5A8933C72B0F9707CFF64D91A02D35115B9A007BF6A749B62AB0`；不是真实宿主安装包。

### 2026-09-10 顶部复合按钮修复后的离屏回归

报告： [`.tmp/render-qa-headerbuttons-20260910/render-qa-report.txt`](../../.tmp/render-qa-headerbuttons-20260910/render-qa-report.txt)。报告提交元数据为 `82e064c`，离屏 RenderHarness `render-qa OK`；双主题、多尺寸、resize、云端筛选文字、完整壳层背景和 Media 页尾几何均通过。该结果不替代真实 Playnite/FusionX、DPI、物理拖动或录屏。

### 2026-09-10 顶部复合按钮视觉树修复证据

`63f4b2d` 为 Dashboard 顶部 7 个图标+文字复合按钮引入视觉内容专用样式，显式清空共享文本 `ContentTemplate`；定向工具栏测试 `1/1`，完整 Playnite `427/490`（63 skip、0 fail），Release 构建 `0 warning/0 error`。候选包身份为 `0.6.73+63f4b2d54d0ff69f824a167ac3d62c070c32bf37`。这仍是插件层和离线证据，不替代真实 Playnite/FusionX 主题、DPI、物理点击或录屏。

### 2026-09-09 首页活动和云端队列整卡行为证据

`tests/GameSaveCenter.Playnite.Tests/OverviewInteractionTests.cs` 在实际 `OverviewView` WPF 视觉树布局后验证：全局活动按钮内容是 `Border`，不是文本模板产生的类型名；云端队列整卡内容是 `StackPanel`，`ContentTemplate` 为空，命令绑定到 `OpenCloudQueueCommand`，实际点击路径只执行一次。定向测试 `1/1`，全量 Playnite `427/490`（63 skip、0 fail）。这是插件层 WPF 行为证据，不替代真实 Playnite/FusionX 主题、DPI、物理拖动或录屏。

### 2026-09-09 回归与离屏 RenderHarness 证据

证据报告： [`.tmp/render-qa-overview-cloud-20260909/render-qa-report.txt`](../../.tmp/render-qa-overview-cloud-20260909/render-qa-report.txt)。工作树干净且报告对应 `1fdd15e`；RenderHarness Release 构建 `0 warning/0 error`、`render-qa OK`。Settings normal/dirty/invalid 三态、Sidebar rapid-toggle 的第二次点击、双主题/多尺寸/resize、生产壳层 Media 1040/1100 表格 `300 DIP`/页尾可达均通过；Media 主表记录实际 `readableRows`，resize 为 `6/4`，原 `230 DIP` 场景为 `4/4`。Inspector 预览/历史列表仍按嵌套小列表处理。这些离屏结果不替代真实 Playnite/FusionX 视频验收。

新增的 `.tmp/render-qa-cloud-filter-probe-20260909/render-qa-report.txt` 在完整 render-qa 中额外验证了真实 `MaintenanceView` 云端队列页的两个 ComboBox：浅色 `#F21B1F27`、深色 `#FFF2F4F8`，选中文本均已实现并可见；同时保留浅/深主题截图。该报告和截图是离屏 WPF 证据，不替代真实 FusionX/Playnite 嵌入。

同一报告还记录生产壳层背景层的运行时几何：`ShellAmbientMaterialLayer` 在浅/深主题均跨完整两列和 footer 两行，并保持 `UseSelectedGameBackground=False`，但离屏夹具未加载真实 Playnite 游戏背景图片，不能据此宣称真实宿主截图已通过。

当前候选包：构建提交 `fa9af0a`，程序集身份 `0.6.73+fa9af0ad36db07551bd1c2985258d72eecb9c8e4`；`.pext/.zip` 均为 `43,837,868` 字节，SHA-256 为 `631615AB7695C46F943D9546A53369C69ADA49F347C4F7CE33D96A33C3831249`。包未安装真实 Playnite，L31 宿主矩阵仍未解除。

### L42/L41 真实宿主启动边界

证据目录：[`artifacts/ui-host-audit-isolated-l42`](../../artifacts/ui-host-audit-isolated-l42)，详细记录：[`L42_REAL_HOST_SCROLL_REPLAY_2026-09-09.md`](L42_REAL_HOST_SCROLL_REPLAY_2026-09-09.md)。L41 的受限启动记录 CEF `拒绝访问 (0x5)`；L42 提升权限后 Playnite 进程保持响应但 `MainWindowHandle=0`，UI Automation 无法定位侧栏，最终没有 `summary.json`、嵌入截图或 replay JSON。L42 只证明本轮宿主入口未建立，不是表格滚动失败或修复证据。

### L40 真实宿主捕获边界

证据目录：[`artifacts/ui-host-audit-isolated-l40`](../../artifacts/ui-host-audit-isolated-l40)，详细记录：[`L40_REAL_HOST_SCROLL_REPLAY_2026-09-09.md`](L40_REAL_HOST_SCROLL_REPLAY_2026-09-09.md)。隔离 Worker 路径已修正且启动身份正确，但最终 `EmbeddedDashboardCaptured=false`、`ControlledDashboardCaptured=true`，生成 `REAL_EMBEDDED_DASHBOARD_NOT_CAPTURED`；没有新的真实嵌入 replay JSON，因此不覆盖 L39 的真实宿主结论，也不计为视频式滚动通过。c84107b 对开发审计 RangeValue provider 异常做了隔离，生产滚动路径未改变。

### L39 真实 Playnite/FusionX 表格回放

证据目录：[`artifacts/ui-host-audit-isolated-l39`](../../artifacts/ui-host-audit-isolated-l39)，详细记录：[`L39_REAL_HOST_SCROLL_REPLAY_2026-09-09.md`](L39_REAL_HOST_SCROLL_REPLAY_2026-09-09.md)。真实嵌入 Dashboard/设置采集成功；媒体 400 条、任务 50 条各 47 个样本，底部 21 个，末项完整性通过；空正文、大间隙、末端水平条覆盖、选中内容缺失和尾项不完整均为 0。该证据覆盖实际宿主端点回放，不覆盖物理鼠标拖动、滚轮/键盘矩阵、DPI/主题矩阵或录屏，因此问题 B 仍为宿主人工待验收。

| 范围 | 权威证据 | 当前结果 | 边界 |
| --- | --- | --- | --- |
| 表格诊断 | `src/GameSaveCenter.Playnite/Infrastructure/DataGridScrollDiagnostics.cs`、`src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml.cs` | 记录实际表格 `ScrollViewer`、`DataGridRowsPresenter`、Presenter 矩形、offset/extent、首末行、单元格内容/裁剪、锚点执行状态；`81da090` 增加最后加载项完整性与末端判定，`a9b8bce` 补齐任务表真实宿主回放，实际 L39 末端两表通过；`c84107b` 隔离开发审计 RangeValue provider 异常 | 物理滑块/视频操作仍需人工宿主验收；不能把程序化端点回放扩大为问题 B 已解决 |
| 任务/媒体离线滚动 | `.tmp/l32-scrollprobe/scaleprobe-report.txt` | 200/2000/10000 条、20 次往返、滚轮、PageUp/PageDown、Ctrl+End；`scaleprobe OK`，报告提交为 `ea18b11` 且 `WorkingTreeClean: True`，未发现空正文/选框分离；末尾滑块路径最后行完整 | 隐藏 WPF `Window` 同时对照插件、标准 WPF 和本机已安装 FusionX `2.1.1` 的 `DefaultControls/DataGrid.xaml`；FusionX 普通视口和水平条显示视口都能直接滑到末尾且末行完整，deferred `ScrollIntoView` 仍基线不确定；均不能替代真实 Playnite |
| 契约与全量回归 | `MediaWindowAnchorContractTests`、Playnite 全量 | L42 回归：Core `76/76`；Worker `311/311`；Playnite `431/488`（57 skip） | skip 详见 [`SKIP_LEDGER_2026-09-09.md`](SKIP_LEDGER_2026-09-09.md) |
| 候选包 | [`L30_PACKAGE_CHECKLIST_2026-09-09.md`](L30_PACKAGE_CHECKLIST_2026-09-09.md)、[`RELEASE_NOTES.md`](../RELEASE_NOTES.md) | L42 Release 身份 `0.6.73+398a6f09...`，编译/发布/安装校验通过；Worker 启动路径已写入隔离目录 | L41/L42 没有嵌入 Dashboard replay，不等价用户环境视频验收 |
| 真实宿主 | [`artifacts/ui-host-audit-isolated-l39`](../../artifacts/ui-host-audit-isolated-l39)、[`L39_REAL_HOST_SCROLL_REPLAY_2026-09-09.md`](L39_REAL_HOST_SCROLL_REPLAY_2026-09-09.md) | `EmbeddedPlaynite=true`；媒体/任务端点回放各 `47` 样本、底部各 `21`，尾项完整，异常计数为 0 | 无物理鼠标拖动、滚轮/键盘矩阵、DPI/主题矩阵或录屏；问题 B 仍宿主人工待验收 |

## 可复核命令

- 源码门禁：`python scripts/validate-source.py`
- XAML 门禁：`powershell -NoProfile -ExecutionPolicy Bypass -File scripts/check-xaml.ps1`
- 诊断契约：`dotnet test tests/GameSaveCenter.Playnite.Tests/GameSaveCenter.Playnite.Tests.csproj -c Release --no-restore --filter "FullyQualifiedName~MediaWindowAnchorContractTests"`
- 离线规模探针：使用 RenderHarness `scaleprobe`，输出应保留在 `.tmp/l32-scrollprobe/scaleprobe-report.txt`；报告包含 `L32-Plugin-DataGrid`、`L32-Standard-DataGrid` 和只读 `L32-FusionX-DataGrid` 的隐藏窗口对照。
- 候选包：`scripts/package.ps1 -Configuration Release -BuildOutputRoot <repo>/.tmp/<isolated-build>`；完成后删除该隔离构建目录，保留当前包和被文档引用的诊断报告。

## 尚未完成的验收

以下项目仍不能标记为完成：真实 Playnite/FusionX 模板链（当前仅只读加载了安装资源）、原视频操作复测、100/150/200% DPI、浅/深/用户主题、键盘焦点、Worker 回收、真实加载更多锚点日志，以及真实宿主中最后一条记录的完整可见性。

恢复条件：提供可绑定的 Playnite UI 控制入口，并确认可以停止/重启当前 Playnite；随后使用 L30 候选包在隔离扩展目录执行 L31 矩阵。此前不修改用户 FusionX、不停止当前宿主、不把候选包或离线报告写成“已解决”。
