# R01-03 / R01-06 当前 main 受控审计归档

源码提交：`dc7f97cfa49724778c4987224c9f736c744b3631`。RenderHarness 在同一提交 clean tree 上生成审计；`audit-metadata.json` 保留了源码身份、逻辑窗口数据与运行环境字段，删除了仅本机有效的临时输出路径。

## 当前审计结果

- 静态 View `10`、Tab `33`、Button/ToggleButton `297`、DataGrid `16`、ScrollViewer `38`；运行时快照 `168`，运行时警告 `110`。
- E01–E20 证据索引校验为 `20/20` 引用、身份、样本与边界；`UiAuditSourceTests 6/6`。
- 当前发现为 `0 HIGH / 7 MEDIUM / 0 Fidelity / 0 failed routes`。七条 MEDIUM 均是窄布局时 `HeaderActionsPanel` 或 Media Inbox 批量动作工具栏发生自然换行/垂直扩展的审计提示；本次增加了行距、没有把布局扩展隐藏成“无风险”。
- 五张精选图取自当前提交的 RenderHarness 输出，窗口逻辑尺寸为 `720×640` 与 `1040×700 DIP`，Light/Dark 条件见完整 [`render-qa-report.txt`](../USER-REPORTED-COMPACT-SPACING-STACKED-HEADER-20260930/render-qa-report.txt)。图片属于离屏合成 WPF，不是 Playnite 最终宿主帧。

## 可复核文件

- `AUDIT_SUMMARY.md`、`EVIDENCE_INDEX.md`、`LAYOUT_REPORT.md`、`UI_FIDELITY_MATRIX.md`、`UI_MANIFEST.md`、`UI_ROUTE_MAP.md` 及其 JSON 机器数据。
- `audit-metadata.json` 记录完整源码 SHA 与测试逻辑尺寸。
- [`screenshots/`](screenshots/) 保存紧凑外壳、存档历史、媒体待归类和任务中心代表图。
- [`USER-REPORTED-COMPACT-SPACING-STACKED-HEADER-20260930`](../USER-REPORTED-COMPACT-SPACING-STACKED-HEADER-20260930/README.md) 记录实际布局几何、行为测试、程序集 SHA/MVID 和颜色原则来源。

## 边界

未启动真实 Playnite；未验证用户安装的 DLL、宿主主题、物理 DPI、OS 输入/IME、UIA/读屏、跨屏、presented frame、ETW 或真实宿主性能。没有读写真实存档、媒体或云端数据。Media Inbox 滚动后列头/行偏移仍待同一安全宿主进程的 `[GSC-GRID-DIAGNOSTIC]`，不能由此离屏审计签收。

如需重建，按当前 `main` 的完整提交身份设置 `GSC_SOURCE_ROOT` / `GSC_UI_AUDIT_COMMIT` 后运行：

```powershell
$env:GSC_SOURCE_ROOT = (Get-Location).Path
$env:GSC_UI_AUDIT_COMMIT = (git rev-parse HEAD).Trim()
dotnet run --project tests/GameSaveCenter.RenderHarness/GameSaveCenter.RenderHarness.csproj -c Release --no-build -- audit .tmp/r01-06-audit
./scripts/validate-ui-evidence-index.ps1 -AuditRoot .tmp/r01-06-audit
```
