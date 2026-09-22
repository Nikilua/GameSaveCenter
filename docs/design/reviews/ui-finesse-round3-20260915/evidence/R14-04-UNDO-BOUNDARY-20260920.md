# R14-04 撤销边界说明证据

日期：2026-09-20  
任务：R14-04 撤销边界说明  
分支：`codex/ui-finesse-round2`  
既有实现来源：`1c0c5a37`、`a7c39922`  
本阶段补充行为夹具：`03521991`

## 任务结论

R14-04 的实现条件已满足：媒体归类历史已有可撤销入口，撤销前后均以批次和媒体稳定 ID 为边界；无法安全撤销的项目会明确显示冲突并保留当前状态，不覆盖应用后的人工修改。这里没有重建媒体归类服务或引入新的移动/删除语义。

## 实现与行为证据

- `MediaCenterView.xaml` 复用现有归类历史和底部操作区，提供“撤销上次建议批次”和“撤销所选可回退批次”；Tooltip 明确只有未被后续修改且仍处于安全状态的项目可回退，冲突项目不会被覆盖。
- `DashboardViewModel.MediaClassification.cs` 只允许 `Applied`/`AppliedWithConflicts` 且存在已应用项的批次进入撤销命令；选中历史批次优先，否则使用最近可撤销批次。
- `DashboardViewModel.Media.cs` 在确认文案中说明“应用后未被再次修改”的范围，并明确冲突项目保留当前状态；结果状态和逐项结果刷新回历史与媒体列表。
- `MediaSyncService.UndoClassificationBatchAsync` 只处理批次项 `Applied` 记录，撤销前重新读取当前媒体并执行 `MatchesAppliedClassification`；目标、归档路径、分类状态和原始元数据任一发生变化都会生成 Conflict，不进入恢复移动。
- `SqliteStateStore.MediaClassification.cs` 的撤销提交使用目标 Playnite ID、当前 `Assigned` 状态、应用后归档路径和批次项 `Applied` 条件更新；条件不成立时不会覆盖后来状态。移动后提交竞态失败也会进入恢复/冲突分支。
- 既有隔离行为夹具 `ClassificationApplyAndUndoMovesOnlyArchiveCopyAndRestoresInboxState` 覆盖正常应用、重启 Store 后撤销、归档副本回到 Inbox、原始文件保留。
- 本阶段新增 `ClassificationUndoLeavesLaterManualDecisionAndArchiveUntouched`：应用后通过隔离 SQLite 修改收藏和备注，再请求撤销；断言结果为 `UndoneWithConflicts`，媒体仍为 `Assigned`，人工备注和收藏保留，应用后的归档文件仍存在，Inbox 副本不会被错误重建。

## 门禁与边界

- `python scripts/validate-source.py`：通过。
- `scripts/check-xaml.ps1 -ProjectRoot ...`：24/24 通过。
- `git diff --check`：通过。
- 已提交补充夹具并推送：`03521991`（`补充媒体撤销冲突负例`）。
- 定向 Worker `dotnet test` 已尝试执行，但当前主机只有已知受限 SDK/Workload 状态；命令长时间无输出，未获得可签收的 testhost 汇总，不能写成运行时通过。Release/net462、Playnite、RenderHarness 和真实宿主本轮未执行。
- 仅使用合成数据、fake 服务和隔离目录；未读取或写入真实存档、媒体、用户云端或对外诊断。没有绕过 ETW/系统跟踪权限。
- Demo 原目录不可用，视觉检查沿用已恢复的生产基线。静态/XAML 和受控夹具不等价真实 Playnite 呈现、物理 DPI/跨屏、UIA/读屏/IME、presented frame、ETW 或宿主性能；main 的用户改动和 `src.zip` 未触碰、未合并。

## 下一步

下一可执行小批量为 `R14-05 重复媒体识别视图`：先核对现有 hash/元数据重复检测与安全只读能力，复用既有服务和 DTO，再补“疑似/确定”分组及不删除真实媒体的行为证据。R14-04 的 Worker/Playnite 运行时复跑仍需在可用 SDK/Workload 环境完成。
