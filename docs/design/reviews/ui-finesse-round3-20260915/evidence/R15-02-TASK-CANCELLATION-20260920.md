# R15-02 取消过程展示证据

日期：2026-09-20
代码提交：`9c8241fb 完善任务取消过程`
分支：`codex/ui-finesse-round2`

## 实际完成

- 先核对现有 `TaskCoordinator.Cancel`、`TaskStatusDto`、取消 IPC 和 Task Center 取消提示；保留原有 `TaskState`、命令绑定、取消入口、滚动系统和 `net462` 路径，没有重建任务服务。
- Contracts 增加共享取消阶段：可取消为空、`Requested`（正在取消）、`Finalizing`（无法立即中断，正在安全收尾）、`Cancelled`（已取消）和 `NotInterruptible`（无法中断，任务已结束）。Task Coordinator 以任务运行时闸门串行化终态与取消请求；重复取消只接受第一次令牌取消，成功/取消竞争按已接受取消收敛为 `Cancelled`，取消后真实失败收敛为 `NotInterruptible`，终态不保留取消中状态。
- SQLite 任务表通过既有迁移入口增加 `cancellation_state`，新增/最近/活动/分页查询和旧库默认值保持一致。Task Center 详情增加取消状态卡，快照比较器和任务复制列同步阶段字段，避免刷新时漏掉取消阶段或阶段文本。

## 证据

- `python scripts/validate-source.py`：通过。
- `scripts/check-xaml.ps1 -ProjectRoot (Get-Location)`：24/24 通过。
- `git diff --check`：通过。
- Worker Release 隔离项目构建产出后，使用 `GscBuildCommit=9c8241fb` 执行 `TaskCoordinatorFailureTests|TaskQueryPersistenceTests`：14/14 通过，退出码 0。覆盖连点取消只触发一次、`Requested → Finalizing → Cancelled` 发布顺序和完成后晚到取消拒绝。
- Playnite Release 构建目标为 `net462`，使用 `GscBuildCommit=9c8241fb` 执行 R06 取消回归、R15-01 阶段和 R15-02 取消夹具：8/8 通过，退出码 0。构建保留 `MediaCenterView.xaml.cs:664` 的 2 条既有 nullable warning，无错误。
- WPF 静态质量检查：0 errors / 28 warnings / 162 info；没有新增 XAML 结构错误。四个本批隔离输出目录已清理。

## 边界与未验项

- 标准完整 solution 脚本在 linked worktree 生成 Playnite WPF 临时项目时遇到 `Access denied`，因此不把完整 solution 写成通过；Worker 与 Playnite 项目已分别实际构建，定向测试结果可复现且已绑定本提交。
- 只使用合成 DTO、fake 状态存储、隔离 SQLite/输出目录；未写真实存档、媒体、云端或诊断。Demo 原目录不可用，沿用恢复生产基线。
- 未验真实 Playnite 宿主/RenderHarness、用户主题和最终 presented frame、物理 DPI/跨屏、UIA/IME、ETW、宿主性能及不响应取消令牌的真实长任务；也未把静态契约或离屏结果写成这些证据。
- main 分支的用户改动 `DashboardView.xaml.cs`、对话框基础设施和 `src.zip` 未触碰、未合并。

下一可执行任务：`R15-03 任务详情时间线`，先查已有任务事件缓存、阶段字段和详情滚动容器，再补最小时间线读模型与行为负例。
