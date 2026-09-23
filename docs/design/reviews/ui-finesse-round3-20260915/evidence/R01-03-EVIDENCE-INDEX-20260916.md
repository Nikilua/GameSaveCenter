# R01-03 每项证据直达证据

> 当前 main 复核已由 R00/R01 主复核记录取代旧审计摘要：源码身份 f55dce61adba84fec96c3e5434e5a8c1e3fa7132，E01–E20 的引用/身份/样本/边界校验均为 20/20。后文 c2399d7b、b5c7a6d4 等提交只作为历史实现与历史审计记录；当前实际风险保留 7 HIGH/4 MEDIUM，详见 [当前 main 复核](R00-R01-CURRENT-RECHECK-20260923.md) 和 [R01-06 归档](R01-06-controlled-audit-20260923/README.md)。
日期：2026-09-16；当前审计复核：2026-09-18
分支：`codex/ui-finesse-round2`  
实现提交：`3875f88`、`2eb4c46`、`591deae`、`f6a3209`、`9d5146d`  
历史审计身份：`9d5146d4a4d604b2779f933f3dee00e730c68b71`
当前审计身份：`c2399d7be9f723e77226619172be16778fe3646f`

## 对照缺口

质量审查要求 R01-03 不再用同一句自动通过覆盖几十项，而要能从共享聚合报告直达具体控件/状态、代码身份、样本和未验边界；抽查 20 项应能在 30 秒内找到对应执行结果。已有 `UI_MANIFEST`、`LAYOUT_REPORT` 和 `UI_FIDELITY_MATRIX` 各自提供部分信息，但没有共同索引。

## 实现

- `UiReportWriter.WriteAll` 现在生成 `EVIDENCE_INDEX.md`，并在 `AUDIT_SUMMARY.md` 与 `README.md` 暴露入口。
- `UiEvidenceIndexBuilder` 从实际 `UiAuditRunResult` 的 manifest、运行时布局和快照集合生成索引：优先抽取生产页面的 DataGrid，再按 route 分散抽取交互控件；每行包含页面/Tab、具体控件、状态、结果文件和检索键、40 位提交身份、样本字段及未验边界。
- 运行时 DataGrid 条目直达 `LAYOUT_REPORT.md` 的 route/Tab/size/DataGrid；静态-only 条目直达 `UI_MANIFEST.md` 的 source file/line，并明确没有本轮运行时几何样本。交互条目直达 `UI_FIDELITY_MATRIX.md` 和 manifest source。
- `scripts/validate-ui-evidence-index.ps1` 校验 E01～E20、报告与源码入口、完整 commit、样本和边界字段；`UiAuditSourceTests` 增加接入契约测试。`9d5146d` 同时修正新脚本的 Windows PowerShell BOM、R01-01 身份命令参数的源码分隔符兼容，以及当前 `ButtonChrome=0.72` 的既有源码守卫。

## 当前提交审计复核

- 从当前 `c2399d7b` 新建 `.tmp\r01-03-build-c2399d7b`，solution 与 RenderHarness Release 构建均 `0 warning / 0 error`，XAML 结构校验 `24/24`。RenderHarness 使用当前程序集和源码根运行，没有复用旧默认 bin。
- 完整受控审计输出为 `.tmp\r01-03-audit-c2399d7b`；`AUDIT_SUMMARY.md` 记录静态 View `10`、Tab `32`、DataGrid `14`、运行时快照 `161`、INFO `80`、Fidelity 警告 `0`、失败路由 `0`、HIGH/MEDIUM `0`。
- `scripts/validate-ui-evidence-index.ps1 -AuditRoot .tmp\r01-03-audit-c2399d7b` 实际输出：`rows=20, references=20/20, identities=20/20, samples=20/20, boundaries=20/20`；E01～E20 的代码身份均为完整 `c2399d7b...`，运行时条目直达 `LAYOUT_REPORT.md`，静态条目明确标注无本轮运行时几何样本。
- 当前 freshness 扫描仍会按版本化基线把 R01-03 标为“需要重跑”，原因是 `Program.cs` 自 `9d5146d` 后有变更；本节的当前审计正是完成该重跑后的新证据，不能把旧 JSON 的状态当作当前审计结论。R01-01/R01-02 同批也已用新提交重跑，R01-07 的基线扫描后续再单独收口。

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

R01-07 freshness baseline 已在 main 按 f55dce61 审计身份更新，14 条记录 14 fresh/0 stale；详见 R01-07 main 当前复核。R01-08 也已有独立 skip 说明，下一项以第三轮账本的未完成任务为准。

## 2026-09-23 当前身份复核

- 当前提交 `b5c7a6d423a4bf23004c3b080e133b3b0b065fa5` 的隔离审计记录为 `168` 个运行时快照、`0` Fidelity、`0` 失败路由；`validate-ui-evidence-index.ps1` 输出 `rows=20, references=20/20, identities=20/20, samples=20/20, boundaries=20/20`。
- 当前审计摘要实际包含 `7` 条 TRUE_PARENT_CHILD_SCROLL_CONFLICT HIGH 和 `4` 条 TOOLBAR_VERTICAL_EXPANSION MEDIUM；它们是摘要中的真实发现，不能再沿用历史 `0 HIGH/0 MEDIUM` 说法。索引可追溯性通过不等于这些页面风险已清零。
- 审计运行显式设置 `GSC_SOURCE_ROOT`、`GSC_BUILD_COMMIT`、`GSC_UI_AUDIT_COMMIT`，结果身份为完整 SHA，`WorkingTreeClean=True`；未启动真实 Playnite，不把索引校验升级为宿主呈现通过。

## 2026-09-23 clean-tree 当前审计

- 在 clean commit `5fbfc869ecddec852440ac82b3b0cc94343f3d60` 的隔离 Release RenderHarness 上生成审计；结果完整 SHA 与 `audit-metadata.json`、20 项索引一致。XAML `24/24`，solution `0 errors / 2` 条既有 nullable warning。
- 当前摘要为 `168` 个运行时快照、`110` 个运行时警告、`0` Fidelity、`0` 失败路由；明确有 `7 HIGH / 4 MEDIUM`。索引校验 `rows=20, references=20/20, identities=20/20, samples=20/20, boundaries=20/20`。
- 探针报告中一个纯图标按钮此前标成 `<composite>: missing`。核对屏幕图与可视树后确认控件存在且没有文本节点；RenderHarness 已改为“icon-only / no text label / text contrast not applicable”，不将它混入文字对比样本，也不误报缺控件。
- 该审计、索引和当前 38d5b7b2 freshness 记录之间只有之后的测试断言与文档变化；R01-03 的 `sourcePaths` 未被这些变化命中。完整归档链接见 [R01-06 当前审计](R01-06-controlled-audit-20260923/README.md)。
