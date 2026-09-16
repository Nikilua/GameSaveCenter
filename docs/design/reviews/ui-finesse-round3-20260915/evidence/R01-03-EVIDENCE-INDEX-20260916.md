# R01-03 每项证据直达证据

日期：2026-09-16  
分支：`codex/ui-finesse-round2`  
实现提交：`3875f88`、`2eb4c46`、`591deae`、`f6a3209`、`9d5146d`  
最终审计身份：`9d5146d4a4d604b2779f933f3dee00e730c68b71`

## 对照缺口

质量审查要求 R01-03 不再用同一句自动通过覆盖几十项，而要能从共享聚合报告直达具体控件/状态、代码身份、样本和未验边界；抽查 20 项应能在 30 秒内找到对应执行结果。已有 `UI_MANIFEST`、`LAYOUT_REPORT` 和 `UI_FIDELITY_MATRIX` 各自提供部分信息，但没有共同索引。

## 实现

- `UiReportWriter.WriteAll` 现在生成 `EVIDENCE_INDEX.md`，并在 `AUDIT_SUMMARY.md` 与 `README.md` 暴露入口。
- `UiEvidenceIndexBuilder` 从实际 `UiAuditRunResult` 的 manifest、运行时布局和快照集合生成索引：优先抽取生产页面的 DataGrid，再按 route 分散抽取交互控件；每行包含页面/Tab、具体控件、状态、结果文件和检索键、40 位提交身份、样本字段及未验边界。
- 运行时 DataGrid 条目直达 `LAYOUT_REPORT.md` 的 route/Tab/size/DataGrid；静态-only 条目直达 `UI_MANIFEST.md` 的 source file/line，并明确没有本轮运行时几何样本。交互条目直达 `UI_FIDELITY_MATRIX.md` 和 manifest source。
- `scripts/validate-ui-evidence-index.ps1` 校验 E01～E20、报告与源码入口、完整 commit、样本和边界字段；`UiAuditSourceTests` 增加接入契约测试。`9d5146d` 同时修正新脚本的 Windows PowerShell BOM、R01-01 身份命令参数的源码分隔符兼容，以及当前 `ButtonChrome=0.72` 的既有源码守卫。

## 实际验证

1. 在最终提交运行完整受控审计：

   ```powershell
   $env:GSC_SOURCE_ROOT = (Get-Location).Path
   $env:GSC_UI_AUDIT_COMMIT = (git rev-parse HEAD).Trim()
   dotnet run --project tests\GameSaveCenter.RenderHarness\GameSaveCenter.RenderHarness.csproj -c Release --no-restore -- audit .tmp\r01-03-audit-proof3
   .\scripts\validate-ui-evidence-index.ps1 -AuditRoot .tmp\r01-03-audit-proof3
   ```

   审计命令 exit `0`；报告使用完整身份 `9d5146d4a4d604b2779f933f3dee00e730c68b71`。校验器输出：`rows=20, references=20/20, identities=20/20, samples=20/20, boundaries=20/20`。

2. 20 项具体索引的抽样构成为：

   | ID | 页面 / Tab | 控件 | 结果类型 |
   | --- | --- | --- | --- |
   | E01 | maintenance / 云端队列 | `DataGrid CloudTransferGrid` | 运行时 `LAYOUT_REPORT`，standard，8 items，virtualization=Recycling |
   | E02 | maintenance / 发现的问题 | `DataGrid MaintenanceAuditFindingsGrid` | 静态 `UI_MANIFEST`，4 列，明确无运行时几何样本 |
   | E03 | maintenance / 审计记录 | `DataGrid MaintenanceAuditLogGrid` | 静态 `UI_MANIFEST`，3 列，明确无运行时几何样本 |
   | E04 | maintenance / 异常与审计 | `DataGrid MaintenanceAuditFindingsGrid` | 静态 `UI_MANIFEST`，4 列，明确无运行时几何样本 |
   | E05 | maintenance / 异常与审计 | `DataGrid MaintenanceAuditLogGrid` | 静态 `UI_MANIFEST`，3 列，明确无运行时几何样本 |
   | E06 | maintenance / 设备状态 | `DataGrid MaintenanceDeviceGrid` | 运行时 `LAYOUT_REPORT`，standard，8 items，virtualization=Recycling |
   | E07 | maintenance / 诊断 | `DataGrid FindingsGrid` | 静态 `UI_MANIFEST`，3 列，明确无运行时几何样本 |
   | E08 | maintenance / 进程映射 | `DataGrid MaintenanceProcessGrid` | 运行时 `LAYOUT_REPORT`，standard，8 items，virtualization=Recycling |
   | E09 | maintenance / 问题列表 | `DataGrid FindingsGrid` | 静态 `UI_MANIFEST`，3 列，明确无运行时几何样本 |
   | E10 | media-center / 待归类 | `DataGrid MediaInboxGrid` | 运行时 `LAYOUT_REPORT`，standard，6 items，virtualization=Standard |
   | E11 | save-center / 历史版本 | `DataGrid SaveHistoryGrid` | 运行时 `LAYOUT_REPORT`，standard，8 items，virtualization=Recycling |
   | E12 | save-center / 路径与校验 | `DataGrid SaveCandidateGrid` | 运行时 `LAYOUT_REPORT`，standard，8 items，virtualization=Recycling |
   | E13 | task-center / 页面 | `DataGrid TaskGrid` | 运行时 `LAYOUT_REPORT`，standard，8 items，virtualization=Recycling |
   | E14 | maintenance / 云端队列 | `Button CloudTransferCompactDetailsButton` | `UI_FIDELITY_MATRIX` + manifest source `MaintenanceView.xaml:495` |
   | E15 | media-center / 当前游戏媒体 | `Button MediaCompactDetailsButton` | `UI_FIDELITY_MATRIX` + manifest source `MediaCenterView.xaml:589` |
   | E16 | save-center / 历史版本 | `Button SaveHistoryCompactDetailsButton` | `UI_FIDELITY_MATRIX` + manifest source `SaveCenterView.xaml:184` |
   | E17 | task-center / 页面 | `Button TaskClearFiltersButton` | 条件状态，`UI_FIDELITY_MATRIX` + source `TaskCenterView.xaml:200` |
   | E18 | trainer-center / FLiNG 在线库 | unnamed `Button` | 条件状态，`UI_FIDELITY_MATRIX` + source `TrainerCenterView.xaml:135` |
   | E19 | settings / 页面 | `Button SettingsValidationLocateButton` | `UI_FIDELITY_MATRIX` + source `GameSaveCenterSettingsView.xaml:67` |
   | E20 | overview / 页面 | unnamed `Button` | `UI_FIDELITY_MATRIX` + source `OverviewView.xaml:312` |

3. UI 审计 summary 为 Fidelity 警告 `0`、失败路由 `0`；完整隔离 Release 为构建 `0 warning / 0 error`，Core `83/83`、Worker `311/311`、Playnite `521` 通过、`57` 跳过、`0` 失败；新增源码契约定向测试 `6/6`，`scripts/validate-source.py` 通过。

## 边界

- 索引验证的是受控 WPF 离屏审计报告的可追溯性，不是把 20 项自动升级成视觉/交互全量签收。静态-only 条目只证明 manifest 结构和源码位置；条件、禁用、错误、加载分支仍需各自行为证据。
- 审计使用合成数据、实际 WPF 视图、隔离窗口和 offscreen logical DIP；不等价真实 Playnite 嵌入 Dashboard、物理 DPI、OS 输入/IME、presented frame、ETW 或宿主性能。没有执行备份、恢复、删除、迁移、下载、设置保存或真实媒体写入。
- 临时审计目录和 ZIP 只用于本次复跑，文档完成后清理；持久事实是源代码、测试和本证据文档。索引未来若出现少于 20 项、unknown commit、缺 source 入口或缺 boundary，校验器应阻断。

## 下一步

R01-03 的索引实现与完整性校验已完成，下一可执行小批量为 R01-04“动效行为替代字符串”：复用现有 `GscMotion` 行为测试，证明删掉关键终态处理会失败，同时保留源码门禁的结构用途。
