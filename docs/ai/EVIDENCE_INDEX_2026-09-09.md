# 2026-09-09 证据索引与验收边界

当前基线：`main`，窗口级滚动探针提交为 `8775809`；候选包内生产代码提交为 `8d729ab`。本索引只汇总已有证据，不把离线夹具或源码断言扩大为真实宿主结论。

## 当前阶段

| 范围 | 权威证据 | 当前结果 | 边界 |
| --- | --- | --- | --- |
| 表格诊断 | `src/GameSaveCenter.Playnite/Infrastructure/DataGridScrollDiagnostics.cs`、`src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml.cs` | 记录实际表格 `ScrollViewer`、`DataGridRowsPresenter`、Presenter 矩形、offset/extent、首末行、单元格内容/裁剪、锚点执行状态；`f31711c` 让锚点捕获/恢复采用同一实际滚动器选择规则，定向 `9/9` | 仍需真实 Playnite/FusionX 日志；此前候选包 `8d729ab` 未包含该修复 |
| 任务/媒体离线滚动 | `.tmp/l32-scrollprobe/scaleprobe-report.txt` | 200/2000/10000 条、20 次往返、滚轮、PageUp/PageDown、Ctrl+End；`scaleprobe OK`，报告提交为 `ea18b11` 且 `WorkingTreeClean: True`，未发现空正文/选框分离；末尾滑块路径最后行完整 | 隐藏 WPF `Window` 同时对照插件、标准 WPF 和本机已安装 FusionX `2.1.1` 的 `DefaultControls/DataGrid.xaml`；FusionX 普通视口和水平条显示视口都能直接滑到末尾且末行完整，deferred `ScrollIntoView` 仍基线不确定；均不能替代真实 Playnite |
| 契约与全量回归 | `MediaWindowAnchorContractTests`、Playnite 全量 | 锚点定向 `9/9`；Playnite `422/485`，`0` 失败、`63` 条件 skip | skip 详见 [`SKIP_LEDGER_2026-09-09.md`](SKIP_LEDGER_2026-09-09.md) |
| 候选包 | [`L30_PACKAGE_CHECKLIST_2026-09-09.md`](L30_PACKAGE_CHECKLIST_2026-09-09.md)、[`RELEASE_NOTES.md`](../RELEASE_NOTES.md) | `0.6.73+8d729abd...`；`.pext`/`.zip` 各 `43,831,491` 字节，SHA-256 `680E5007...A75B2B7`；迁移定向 `14/14` | 未安装到真实 Playnite |
| 真实宿主 | [`L31_REAL_HOST_BLOCKER_2026-09-09.md`](L31_REAL_HOST_BLOCKER_2026-09-09.md) | PowerShell 确认 `D:\software\Playnite\Playnite.DesktopApp.exe` 路径；本次复核时进程已退出，Windows Computer Use 仍返回 `apps: []` | 未启动宿主；无真实截图、录屏、FusionX、DPI 或宿主滚动日志 |

## 可复核命令

- 源码门禁：`python scripts/validate-source.py`
- XAML 门禁：`powershell -NoProfile -ExecutionPolicy Bypass -File scripts/check-xaml.ps1`
- 诊断契约：`dotnet test tests/GameSaveCenter.Playnite.Tests/GameSaveCenter.Playnite.Tests.csproj -c Release --no-restore --filter "FullyQualifiedName~MediaWindowAnchorContractTests"`
- 离线规模探针：使用 RenderHarness `scaleprobe`，输出应保留在 `.tmp/l32-scrollprobe/scaleprobe-report.txt`；报告包含 `L32-Plugin-DataGrid`、`L32-Standard-DataGrid` 和只读 `L32-FusionX-DataGrid` 的隐藏窗口对照。
- 候选包：`scripts/package.ps1 -Configuration Release -BuildOutputRoot <repo>/.tmp/<isolated-build>`；完成后删除该隔离构建目录，保留当前包和被文档引用的诊断报告。

## 尚未完成的验收

以下项目仍不能标记为完成：真实 Playnite/FusionX 模板链（当前仅只读加载了安装资源）、原视频操作复测、100/150/200% DPI、浅/深/用户主题、键盘焦点、Worker 回收、真实加载更多锚点日志，以及真实宿主中最后一条记录的完整可见性。

恢复条件：提供可绑定的 Playnite UI 控制入口，并确认可以停止/重启当前 Playnite；随后使用 L30 候选包在隔离扩展目录执行 L31 矩阵。此前不修改用户 FusionX、不停止当前宿主、不把候选包或离线报告写成“已解决”。
