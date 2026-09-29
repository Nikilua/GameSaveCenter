# R01-06 当前 main 受控审计归档

源码身份：`9c906cc0772aad06143bdf3237255effd417e2de`。本目录保存当前源构建的可查阅审计索引、布局报告、manifest、route map、fidelity matrix、summary 和五张精选页面图，可在只 clone 仓库的机器上读取。

## 当前结果

- 静态 View `10`、Tab `33`、Button/ToggleButton `297`、DataGrid `16`、ScrollViewer `38`、条件 UI `293`；运行时快照 `168`。
- `EVIDENCE_INDEX.md` 包含 20 项具体控件/状态；`scripts/validate-ui-evidence-index.ps1` 校验 `20/20` 行，报告/source 引用、完整 commit、样本与未验边界均 `20/20`。
- Audit 检出 `7 HIGH / 4 MEDIUM`、`0` Fidelity 警告、`0` 失败路由。HIGH/MEDIUM 是实际审计发现，不因审计成功或索引可追溯而清零。
- 代表图经查看：概览、存档历史、媒体待归类、任务队列、维护问题列表。所有图片均来自合成数据和 offscreen logical DIP。

## 文件

- `AUDIT_SUMMARY.md`、`EVIDENCE_INDEX.md`、`LAYOUT_REPORT.md`：summary、20 项证据索引和运行时布局。
- `UI_MANIFEST.md` / `.json`、`UI_ROUTE_MAP.md` / `.json`、`UI_FIDELITY_MATRIX.md`：静态控件清单、页面路由及审计 fidelity 结果。
- `audit-metadata.json`：完整代码 commit、版本及逻辑窗口尺寸；机器临时目录和 zip 路径已替换为可复现说明。
- [`screenshots/`](screenshots/)：5 张精选 2K logical DIP 页面图。

全量 visual-tree JSON 与 312 张全页面 RenderHarness 图没有提交；请用同身份源码构建，并把输出写入被忽略的 `.tmp` 后重新生成。命令示例：

```powershell
$env:GSC_SOURCE_ROOT = (Get-Location).Path
$env:GSC_BUILD_COMMIT = (git rev-parse HEAD).Trim()
$env:GSC_UI_AUDIT_COMMIT = (git rev-parse HEAD).Trim()
$buildRoot = Join-Path (Get-Location).Path '.tmp/r01-06-rebuild'
$auditRoot = Join-Path (Get-Location).Path '.tmp/r01-06-audit'
dotnet build tests/GameSaveCenter.RenderHarness/GameSaveCenter.RenderHarness.csproj -c Release "-p:GscBuildOutputRoot=$buildRoot"
$exe = Join-Path $buildRoot 'bin/GameSaveCenter.RenderHarness/Release/net472/GameSaveCenter.RenderHarness.exe'
& $exe audit $auditRoot
./scripts/validate-ui-evidence-index.ps1 -AuditRoot $auditRoot
```

## 边界

没有启动真实 Playnite 宿主，没有验证用户主题、有效物理 DPI、OS 输入/IME、UIA/读屏、presented frame、ETW 或真实宿主性能。此次 audit 不读写真实存档、媒体、云端或诊断数据。若宿主隔离启动条件（CEF `platform_channel 0x5` / 访问检查）没有变化，不重试相同受阻流程；保留真实宿主滚动、跨屏和性能边界。
