# R20-05 Stale 可理解证据

日期：2026-09-21  
实现提交：`c492bbc2`（`补齐云端过期状态提示`）

## 目标

R20-05 要求旧数据状态展示最后成功时间、刷新失败原因和重试入口；用户必须能区分旧缓存与当前真值，且已经可用的安全只读操作不能被错误遮挡。

## 已有能力与本阶段补口

- 任务中心已有 `TaskPageStatusSummary`、`TaskPageLastUpdatedDisplay`、旧数据保留提示和刷新命令；存档、媒体和维护工作区已有 `WorkspaceDataState.Stale`、最后成功时间、失败详情及重试按钮，本阶段复用这些路径。
- 云端队列此前在有旧记录时会保留表格但没有独立 stale 提示。本阶段为云端队列记录最近成功读取时间和本次错误原因，新增 `CloudTransferStaleVisible`、`CloudTransferStateDetail`，在旧记录上方显示“云端队列显示已过期”、最后成功时间、失败原因和“重试”入口。
- 云端读取失败时不清除旧记录、不改变队列数据和分页契约；加载摘要明确“读取失败，已保留旧数据”。无旧记录时仍走 R20-04 的失败空态，明确这不是零结果。
- `FilterConditionSummary.StaleStateDetail` 统一 stale 详情格式，覆盖“上次成功读取”和“本次刷新失败”两部分；不改变当前游戏选框、滚动条、命令绑定、取消/恢复和有限列表性能路径。

## 受控验证

- R20-04 条件摘要/选框行为、R20-05 stale 详情以及既有媒体/任务状态回归：`37 passed / 1 failed / 0 skipped / 38 total`。
- 唯一失败为未修改的 `TaskCenterViewResponsiveTests.FailedTaskDetailsPutUserReasonBeforeCollapsedTechnicalDetails`，仍是既有 `TaskCenterView.xaml` 断言漂移；未把它计作本项云端 stale 行为失败。
- Playnite `net462` / Tests `net472` 隔离 source-copy Release 构建：`0 errors / 2 warnings`，两条均为既有 `MediaCenterView.xaml.cs:671` 的 `CS8602`。
- `python scripts/validate-source.py`、`scripts/check-xaml.ps1`（`24/24`）和 `git diff --check` 通过。

## 边界与未验证项

只使用合成/fake/隔离 testhost 和隔离构建目录，没有写真实存档、媒体、云端或诊断；未宣称真实 Playnite/package-host 呈现、物理 DPI/跨屏、UIA/IME、ETW 或宿主性能已验证。Demo 原目录不可用，main 的用户改动未触碰、未合并。

下一可执行任务为 `Q/R` 依赖清单中下一项：先从最新账本筛选依赖已满足且未被既有基线阻塞的小批量，继续逐项补行为证据，不把未修改的 UI 断言漂移扩大成无关重构。
