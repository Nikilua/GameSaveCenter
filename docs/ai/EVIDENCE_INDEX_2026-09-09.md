# 2026-09-09 证据索引与验收边界

当前基线：`main`，最新代码提交为 `a9b8bce`；真实宿主回放包身份为 `0.6.73+a9b8bcec0c05f7d548d8160119b2b29e9698b1ab`。窗口级滚动探针 canonical 报告仍为 `ea18b11`。本索引只汇总已有证据，不把程序化回放扩大为物理滑块/视频结论。

## 当前阶段

### L39 真实 Playnite/FusionX 表格回放

证据目录：[`artifacts/ui-host-audit-isolated-l39`](../../artifacts/ui-host-audit-isolated-l39)，详细记录：[`L39_REAL_HOST_SCROLL_REPLAY_2026-09-09.md`](L39_REAL_HOST_SCROLL_REPLAY_2026-09-09.md)。真实嵌入 Dashboard/设置采集成功；媒体 400 条、任务 50 条各 47 个样本，底部 21 个，末项完整性通过；空正文、大间隙、末端水平条覆盖、选中内容缺失和尾项不完整均为 0。该证据覆盖实际宿主端点回放，不覆盖物理鼠标拖动、滚轮/键盘矩阵、DPI/主题矩阵或录屏，因此问题 B 仍为宿主人工待验收。

| 范围 | 权威证据 | 当前结果 | 边界 |
| --- | --- | --- | --- |
| 表格诊断 | `src/GameSaveCenter.Playnite/Infrastructure/DataGridScrollDiagnostics.cs`、`src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml.cs` | 记录实际表格 `ScrollViewer`、`DataGridRowsPresenter`、Presenter 矩形、offset/extent、首末行、单元格内容/裁剪、锚点执行状态；`81da090` 增加最后加载项完整性与末端判定，`a9b8bce` 补齐任务表真实宿主回放，实际 l39 末端两表通过 | 物理滑块/视频操作仍需人工宿主验收；不能把程序化端点回放扩大为问题 B 已解决 |
| 任务/媒体离线滚动 | `.tmp/l32-scrollprobe/scaleprobe-report.txt` | 200/2000/10000 条、20 次往返、滚轮、PageUp/PageDown、Ctrl+End；`scaleprobe OK`，报告提交为 `ea18b11` 且 `WorkingTreeClean: True`，未发现空正文/选框分离；末尾滑块路径最后行完整 | 隐藏 WPF `Window` 同时对照插件、标准 WPF 和本机已安装 FusionX `2.1.1` 的 `DefaultControls/DataGrid.xaml`；FusionX 普通视口和水平条显示视口都能直接滑到末尾且末行完整，deferred `ScrollIntoView` 仍基线不确定；均不能替代真实 Playnite |
| 契约与全量回归 | `MediaWindowAnchorContractTests`、Playnite 全量 | 锚点定向 `10/10`；Playnite `423/486`，`0` 失败、`63` 条件 skip | skip 详见 [`SKIP_LEDGER_2026-09-09.md`](SKIP_LEDGER_2026-09-09.md) |
| 候选包 | [`L30_PACKAGE_CHECKLIST_2026-09-09.md`](L30_PACKAGE_CHECKLIST_2026-09-09.md)、[`RELEASE_NOTES.md`](../RELEASE_NOTES.md) | l39 包身份 `0.6.73+a9b8bcec...`，Release 编译/发布/安装校验通过；用于真实宿主隔离回放 | 当前包已在隔离真实 Playnite 运行，不等价用户环境视频验收 |
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
