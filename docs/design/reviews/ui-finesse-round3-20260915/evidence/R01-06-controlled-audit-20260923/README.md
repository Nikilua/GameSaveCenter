# R01-06 受控审计归档（2026-09-23）

## 身份与结果

- 审计源码提交：`5fbfc869ecddec852440ac82b3b0cc94343f3d60`（clean tree）
- 插件版本：`0.6.73.0`；Playnite SDK：`6.16.0.0`
- WPF 离屏逻辑 DPI：`1.0`；审计窗口按逻辑 DIP 建立，不是真实 Playnite 宿主或物理屏幕采样。
- 运行时快照：`168`；Fidelity 警告：`0`；失败路由：`0`；HIGH：`7`；MEDIUM：`4`。
- `EVIDENCE_INDEX.md` 的 20 行均可追到结果、完整身份、样本和边界（索引校验 `20/20`）。
- 审计的 7 条 HIGH 是媒体页父子滚动冲突；4 条 MEDIUM 是待归类工具栏纵向扩展。该归档保留实际发现，不代表问题已消除。

## 文件

- `AUDIT_SUMMARY.md`：静态/运行时计数、告警和真实发现。
- `audit-metadata.json`：完整 SHA、版本、尺寸、逻辑 DPI；路径改为可再生相对路径。
- `UI_MANIFEST.md`、`UI_ROUTE_MAP.md`、`UI_FIDELITY_MATRIX.md`：页面、路由和状态入口。
- `LAYOUT_REPORT.md`：运行时布局记录。
- `EVIDENCE_INDEX.md`：20 个控件/状态样本的结果入口、身份、样本和边界。
- `screenshots/`：六张人工查看过的代表图：概览标准/窄窗、媒体待归类、维护诊断、存档历史、任务中心。所有图来自 synthetic DTO 和 WPF 离屏 logical DIP。

全量截图和 JSON 树共数百个文件，未复制到仓库；在下方命令中可本机重建。图片不证明真实 Playnite 呈现、物理 DPI、屏幕帧、UIA/IME 或宿主性能。

## 重现

在独立 checkout 中使用归档源码提交 `5fbfc869ecddec852440ac82b3b0cc94343f3d60`：

```powershell
$buildRoot = '.tmp\r01-06-reproduce-build'
$auditRoot = '.tmp\r01-06-reproduce-audit'
$env:GSC_SOURCE_ROOT = (Get-Location).Path
$env:GSC_BUILD_COMMIT = (git rev-parse HEAD).Trim()
$env:GSC_UI_AUDIT_COMMIT = $env:GSC_BUILD_COMMIT
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\build.ps1 -Configuration Release -SkipTests -OutputRoot $buildRoot
dotnet restore tests\GameSaveCenter.RenderHarness\GameSaveCenter.RenderHarness.csproj -m:1 "-p:GscBuildOutputRoot=$buildRoot" -p:NuGetAudit=false -p:MSBuildEnableWorkloadResolver=false
dotnet build tests\GameSaveCenter.RenderHarness\GameSaveCenter.RenderHarness.csproj -c Release --no-restore -m:1 -nodeReuse:false "-p:GscBuildOutputRoot=$buildRoot" -p:NuGetAudit=false -p:MSBuildEnableWorkloadResolver=false
& "$buildRoot\bin\GameSaveCenter.RenderHarness\Release\net472\GameSaveCenter.RenderHarness.exe" audit $auditRoot
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\validate-ui-evidence-index.ps1 -AuditRoot $auditRoot
```

审计只展示合成数据、切换受控页面/Tab 和收集截图；没有执行备份、恢复、删除、迁移、下载、配置保存、真实媒体/云端写入或诊断外发。ZIP 和完整输出属于本机 `.tmp` 可再生文件，没有归档。