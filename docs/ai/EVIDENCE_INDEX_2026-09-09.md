# 2026-09-09 证据索引与验收边界

当前基线：`main`，窗口级滚动探针提交为 `862742a`；候选包内生产代码提交为 `8d729ab`。本索引只汇总已有证据，不把离线夹具或源码断言扩大为真实宿主结论。

## 当前阶段

| 范围 | 权威证据 | 当前结果 | 边界 |
| --- | --- | --- | --- |
| 表格诊断 | `src/GameSaveCenter.Playnite/Infrastructure/DataGridScrollDiagnostics.cs`、`src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml.cs` | 记录实际表格 `ScrollViewer`、`DataGridRowsPresenter`、Presenter 矩形、offset/extent、首末行、单元格内容/裁剪、锚点执行状态 | 仍需真实 Playnite/FusionX 日志 |
| 任务/媒体离线滚动 | `.tmp/l32-scrollprobe/scaleprobe-report.txt` | 200/2000/10000 条、20 次往返、滚轮、PageUp/PageDown、Ctrl+End；`scaleprobe OK`，未发现空正文/选框分离；末尾滑块路径最后行完整 | 新增隐藏 WPF `Window` 对照：插件模板从顶部 `ScrollIntoView(最后一项)` 到 `1992/1992` 且最后行完整；标准模板的 deferred `ScrollIntoView` 在该夹具仍为基线不确定，但直接滑块/Ctrl+End 末行完整；两者都不能替代 FusionX |
| 契约与全量回归 | `MediaWindowAnchorContractTests`、Playnite 全量 | 锚点定向 `8/8`；Playnite `421/484`，`0` 失败、`63` 条件 skip | skip 详见 [`SKIP_LEDGER_2026-09-09.md`](SKIP_LEDGER_2026-09-09.md) |
| 候选包 | [`L30_PACKAGE_CHECKLIST_2026-09-09.md`](L30_PACKAGE_CHECKLIST_2026-09-09.md)、[`RELEASE_NOTES.md`](../RELEASE_NOTES.md) | `0.6.73+8d729abd...`；`.pext`/`.zip` 各 `43,831,491` 字节，SHA-256 `680E5007...A75B2B7`；迁移定向 `14/14` | 未安装到真实 Playnite |
| 真实宿主 | [`L31_REAL_HOST_BLOCKER_2026-09-09.md`](L31_REAL_HOST_BLOCKER_2026-09-09.md) | PowerShell 可见 `D:\software\Playnite\Playnite.DesktopApp.exe`，Windows Computer Use 返回 `apps: []` | 无真实截图、录屏、FusionX、DPI 或宿主滚动日志 |

## 可复核命令

- 源码门禁：`python scripts/validate-source.py`
- XAML 门禁：`powershell -NoProfile -ExecutionPolicy Bypass -File scripts/check-xaml.ps1`
- 诊断契约：`dotnet test tests/GameSaveCenter.Playnite.Tests/GameSaveCenter.Playnite.Tests.csproj -c Release --no-restore --filter "FullyQualifiedName~MediaWindowAnchorContractTests"`
- 离线规模探针：使用 RenderHarness `scaleprobe`，输出应保留在 `.tmp/l32-scrollprobe/scaleprobe-report.txt`；报告新增 `L32-Plugin-DataGrid` 与 `L32-Standard-DataGrid` 的隐藏窗口对照。
- 候选包：`scripts/package.ps1 -Configuration Release -BuildOutputRoot <repo>/.tmp/<isolated-build>`；完成后删除该隔离构建目录，保留当前包和被文档引用的诊断报告。

## 尚未完成的验收

以下项目仍不能标记为完成：真实 Playnite/FusionX 模板链、原视频操作复测、100/150/200% DPI、浅/深/用户主题、键盘焦点、Worker 回收、真实加载更多锚点日志，以及真实宿主中最后一条记录的完整可见性。

恢复条件：提供可绑定的 Playnite UI 控制入口，并确认可以停止/重启当前 Playnite；随后使用 L30 候选包在隔离扩展目录执行 L31 矩阵。此前不修改用户 FusionX、不停止当前宿主、不把候选包或离线报告写成“已解决”。
