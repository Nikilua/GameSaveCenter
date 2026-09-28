# Media Inbox 表头后空白：宿主取证准备（2026-09-28）

## 当前结论

用户报告的真实 Playnite 滚动后大块空白**尚未在本机复现，也未修复**。本阶段只增加生产日志的同坐标几何、运行中程序集身份/窗口/DPI/主题，以及使用生产视图和合成媒体的 WPF 滚动行为回归。没有改共享 DataGrid 模板、MediaDataGrid 行虚拟化、行高或滚动锚点算法；不能从隔离测试通过推断宿主原因。

实现提交 `4b7d1e99a55146daba02cfffe39f046e0c4708c4`。Release Playnite 测试项目构建 `0 warning / 0 error`，`scripts/validate-source.py` 通过。该提交的 `net472` 测试程序集在独立输出目录运行，`11 passed / 0 failed / 0 skipped`：`MediaInboxScrollBehaviorTests` 3、`KeyboardFocusSourceTests` 5、`MediaInboxGeometryTests` 3。原始 TRX 仅留本机 `artifacts/ui-media-inbox-scroll-synthetic-20260928/media-inbox-grid-4b7d1e99.trx`，其中的测试宿主复制路径含本机用户名，未提交。

## 生产日志字段

`[GSC-GRID-DIAGNOSTIC]` 对 `MediaInboxGrid` 记录：已加载插件程序集的 informational version、MVID 和路径，主题，窗口/表格逻辑尺寸，WPF DPI 缩放；`gridGeometryDip` 的列头底边、`PART_ScrollContentPresenter` 顶边、首个与内容视口相交的 `DataGridRow` 顶边均以 **DataGrid 本身为坐标原点**。另记录外层页面 ScrollViewer 的 offset/scrollable/viewport、表格相对该页面视口的顶边，以及内部滚动与首行索引。`theme` 在有真实 DashboardViewModel 时读取当前插件设置；下列无 VM 的夹具显示 `vm=none`，不能替代宿主主题值。

## 隔离 WPF 行为样本

测试将 2,000 条合成 `MediaItemDto` 绑定到生产 `MediaCenterView`，保持 `VirtualizationMode.Standard`、行虚拟化和逻辑项滚动。按顶部、中段、四分之三、滚动条 Thumb 到底、返回顶部往返三轮；首轮通过生产 `MediaPageAccumulator` 追加 50 项并裁掉旧 50 项，再调用生产 `CaptureAnchor`/`QueueRestore`；随后缩小窗口、滚动页面页尾并检查“忽略所选媒体”按钮、窄窗口再拖到底、放大窗口。每个缩放变量有 18 个检查点。检测到的最多已实现行容器为 22，始终少于测试的 40 上限；滚动到底时第 2,000 个缓存项完整可见。这里的 22 属于本夹具，不能与 R18-04 的不同窗口样本合并比较。

本次三个用例的实际 WPF 测试宿主 DPI 都是 `1.5×1.5`。参数 `1.0/1.25/1.5` 只是视图的 `RenderTransform` 输出缩放，**不是**三档物理显示器 DPI；物理 125% 和真实 Playnite 150% 仍待验。选取单个 `scale=1` 用例的日志值如下（单位 DIP；原始程序集路径已从摘录中省略）：

| 检查点 | 列头底 | 内容视口顶 | 首个可见行顶 | 外层页面偏移 | 首行索引 | 末缓存行完整 |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| 顶部初始 | 42 | 42 | 42 | 0 / 62 | 0 | 不适用 |
| 中段 | 42 | 42 | 42 | 0 / 62 | 957 | 不适用 |
| 追加分页并恢复锚点 | 42 | 42 | 42 | 0 / 62 | 907（同一媒体 ID） | 不适用 |
| 拖动到底 | 42 | 42 | 42 | 0 / 62 | 1995 | 是 |
| 窄窗口拖动到底 | 42 | 42 | 42 | 62 / 62 | 1995 | 是 |

同一测试日志中的程序集身份为 `GameSaveCenter.Playnite.dll:0.6.73+4b7d1e99a55146daba02cfffe39f046e0c4708c4`，MVID `1895253a-7e2a-40f7-a3b9-e4bcb3a4849b`。这些是隔离测试加载的 DLL，**不是**用户运行中 Playnite DLL。此前用户贴出的一键测试 `KeyboardFocusSourceTests` 3 失败，在本机用旧程序集与新源码身份错配可重现身份门失败；以本次同提交程序集重跑该类 `5/5`。远程机器上旧产物进入路径的具体原因未核实。

## 宿主待验步骤与限制

当前没有运行中的 Playnite/GameSaveCenter 进程；安全隔离启动前置的 `Win32_Process.CommandLine` 查询仍为 Access Denied。先前相同环境的 Playnite CEF bootstrap 报 `platform_channel 0x5`，见 [ENV-001](ENV-001-ISOLATED-RUNNER-20260926.md)。本阶段未重启相同受阻宿主，也未安装到真实扩展目录、读取真实用户库或修改真实媒体。运行中的 DLL、真实窗口逻辑尺寸/DPI/主题、滚动前后故障日志均**没有**取得。

环境恢复后，先用已授权的隔离 profile 和真实 Playnite：

1. 确认进程命令行属于隔离 profile，记录插件实际加载 DLL 的 path、ProductVersion/MVID、窗口 DIP、DPI 和主题；身份必须与候选提交一致才比较代码行为。
2. 在“待归类”顶部、中段、拖动到底、反复往返、缩放窗口时分别保存 `MediaInboxGrid` 的 `[GSC-GRID-DIAGNOSTIC]` 原行与截图，并核对页面外层 offset。
3. 若 `presenterTop - headerBottom` 增大，先检查共享 `Redesign.xaml` DataGrid 模板和页面有限高度布局；若该差近零而 `firstRowTop - presenterTop` 增大，检查 MediaDataGrid 虚拟化、集合刷新和锚点恢复。只在证据确定后改实现。
4. 在真实宿主重复顶部/中段/拖到底/往返/窗口缩放和物理 125%/150% DPI；核对首行紧贴有效视口、末行完整、页尾操作可达与有限行容器。没有上述结果前，用户缺陷及宿主验收保持打开。

R 台账 192 项与原状态未改；下一可执行步骤是恢复安全宿主取证条件后复现并修根因。若环境仍阻塞，继续已有准入清单中依赖满足的小批量任务，不用离屏结果代替真实呈现。
