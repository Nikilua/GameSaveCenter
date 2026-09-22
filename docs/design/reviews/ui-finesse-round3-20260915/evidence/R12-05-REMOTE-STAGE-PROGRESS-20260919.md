# R12-05 远端下载进度证据

日期：2026-09-19。实现提交：`23e5b9d4`。分支：`codex/ui-finesse-round2`。

## 实现事实

- 先核对现有 `RemoteBackupStagingService`、`TaskCoordinator`、`TaskStatusDto`、任务事件流和 Maintenance 远端恢复入口，没有新增第二套任务状态或替换滚动模型。
- `StageRemote` 任务发布准备、下载到隔离区、完整性校验、Ludusavi 版本确认、隔离清单写入和“等待恢复确认”阶段。`RemoteBackupStageResultDto.StatusMessage` 明确表示已校验但尚未恢复当前存档；`TaskStatusDto.TaskTypeDisplay` 复用现有任务列表显示链。
- 下载取消或失败时只清理本次隔离目录；取消终态保留“隔离区已清理”或“清理失败仍有残留内容”的真实消息，清理失败会提升为稳定 Worker 错误，不静默报告成功。远端下载仍使用现有 rclone 只读 `copy/check` 允许列表，没有远端删除、移动或覆盖。
- Playnite 维护页只新增进度条、活动态取消按钮和状态投影；原远端恢复两步命令、游戏选框、现有 ScrollViewer、命令绑定、取消/错误传播、PreRestore 保护和 net462 目标保持不变。

## 行为与构建证据

- `R12RemoteStageProgressBehaviorTests`：`3/3`。实际验证下载中/一致性校验/待恢复是不同的活动状态和进度；成功不出现“恢复完成”；取消与失败都是终态并保留隔离区边界；Restore 等非远端任务不会污染远端投影。
- `RemoteBackupStagingSafetyTests`：`22/22`。覆盖现有 rclone 破坏性命令拒绝、设备名/暂存 ID 安全规则、`check --one-way` 参数，以及合成隔离目录的递归清理行为。
- 最终 D 盘隔离构建：`scripts/build.ps1 -Configuration Debug -SkipTests -OutputRoot .tmp/r12-05-build-final`，XAML `24/24`，solution `0 warning / 0 error`；构建后分别运行上述两个定向测试，均为 `0` 失败。
- `python scripts/validate-source.py`、`scripts/check-xaml.ps1`、`git diff --check` 均通过。
- 曾按公共门禁执行一次全量隔离 WPF 测试；脚本在既有 `R07ResizeStressBehaviorTests.ResizeSequenceKeepsOpenTaskDetailsAndPickerFocusReachable` 失败处停止，不能写成全量通过。该失败保留为独立边界，没有跳过、放宽断言或把它归因于 R12-05。

## 范围与未验边界

- 业务验证只使用合成 `TaskStatusDto`、fake/隔离目录和测试输出；没有调用真实 rclone/Ludusavi 远端，没有读写真实存档、删除真实媒体、写用户云端或发送诊断。
- 定向测试和隔离 net462 程序集不等价真实 Playnite/package-host 呈现、最终屏幕帧、物理 DPI/跨屏、UIA/读屏、IME、ETW、帧率或大库性能；没有绕过被拒绝的系统跟踪权限。
- Demo 原目录不可用，视觉核对沿用恢复生产基线。main 工作树仍有用户 R08 修改及 `src.zip`，本轮没有覆盖、清理或合并；下一项是 `R12-06` 恢复冲突说明。
