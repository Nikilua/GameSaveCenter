# R20-04 零结果恢复证据

日期：2026-09-20  
实现提交：`cf09f3f6`（`补齐零结果恢复`）

## 目标

R20-04 要求筛选无结果时提供当前条件摘要和清除条件的恢复动作；恢复动作只能改变筛选，不得重置用户数据；离线或读取错误不能伪装成零结果。

## 实现事实

- 游戏选框复用 `GamePickerViewModel` 的搜索、状态、平台筛选和已有清除命令，新增 `ActiveFiltersSummary`；空结果显示当前条件、清除搜索和清除筛选，保留现有选框虚拟化与滚动实现。
- 媒体中心复用 `MediaHasActiveFilters`、`ClearMediaFiltersCommand` 和 `WorkspaceDataState`；媒体空态绑定 `MediaDetailsStateMessage`，有条件时说明当前搜索/类型范围，清除只重置筛选字段并重新查询，不删除或重置媒体数据。
- 任务中心已有 `TaskActiveFiltersSummary`、`ClearTaskFiltersCommand` 和 `FilterEmpty` 分支，本项未重建任务筛选系统。
- 云端队列新增 `CloudTransferActiveFiltersSummary`、`ClearCloudTransferFiltersCommand` 和明确的 `CloudTransferEmptyStateMessage`。读取异常设置 `CloudTransferLoadFailed`，空态会提示“这不是零结果”并引导刷新；清除动作只清空状态、类型、游戏、来源设备和时间筛选后重新加载队列。
- `FilterConditionSummaryTests` 覆盖游戏选框实际清除命令、媒体/云端条件摘要和云端读取失败与零结果的负例；没有用单一 `Assert.Contains` 代替行为验证。

## 受控验证

- `FilterConditionSummaryTests` + `GamePickerViewModelTests`：`26 passed / 0 failed / 0 skipped / 26 total`。
- 受影响 UI/源码套件：`171 passed / 3 failed / 50 skipped / 224 total`。3 条失败均为未修改的既有基线：`SettingsUsesSharedResponsiveFieldGroupsWithoutShrinkingNumericInputs`、`EmptyDataSurfacesExplainNextStepsWithoutBreakingLocalScrolling`、`FiniteWidthComboBoxesUseTheSharedLongTextTemplate`；没有把它们计作本项行为失败。
- Playnite `net462` / Tests `net472` 隔离 source-copy Release 构建：`0 errors / 2 warnings`，两条均为既有 `MediaCenterView.xaml.cs:671` 的 `CS8602`。
- `python scripts/validate-source.py`、`scripts/check-xaml.ps1`（`24/24`）和 `git diff --check` 通过。

## 边界与未验证项

本阶段只使用合成/fake/隔离 testhost 和隔离构建目录，没有写真实存档、媒体、用户云端或诊断；没有宣称真实 Playnite/package-host 呈现、物理 DPI/跨屏、UIA/IME、ETW 或宿主性能已验证。Demo 原目录不可用，继续参考已恢复生产基线；main 的用户改动未触碰、未合并。

下一可执行任务为 `R20-05 Stale 可理解`：先核对任务、媒体、云端和维护页 stale 状态的最后成功时间、失败原因、重试入口，以及已有可用只读数据是否保持可见。
