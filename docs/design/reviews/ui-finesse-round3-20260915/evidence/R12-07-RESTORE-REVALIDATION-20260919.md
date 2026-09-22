# R12-07 预览失效重验证据

日期：2026-09-19  
分支：`codex/ui-finesse-round2`  
代码提交：`00724e62`（已推送 `origin/codex/ui-finesse-round2`）

## 完成事实

- 先复用现有 `DashboardViewModel`、`RestoreOrchestrator`、`RestoreReadinessService`、版本/映射 DTO 和恢复保护链路，没有重建预览服务或恢复命令。
- 确认对话框返回后，提交恢复请求前重新比较确认时的游戏 ID/版本 ID 与当前选中对象；游戏或版本在确认期间变化时清空当前恢复流程，显示“确认期间游戏或版本已变化”，不提交旧确认。
- Worker 执行时仍按最新游戏映射和精确 `BackupId` 重新解析目标，并在实际写入前再次执行目标预览；写入后保留已有结果校验。行为测试记录调用顺序为 `("B", true)`、`("B", false)`、`("B", true)`。
- 恢复就绪重验使用归档清单中的 SHA-256。隔离夹具先验证 4 字节 `save`，再用同样 4 字节的 `data` 替换归档；第二次验证返回 `Corrupted`、实际大小仍为 4、`HashValidation=Failed`，没有把同大小文件当成未变化。

## 验证

- Playnite 定向 `R12Restore`：`13/13` 通过（恢复冲突说明 `4/4`、确认守卫/重验 `2/2`、既有恢复流程 `7/7`）。
- Worker 定向 `RestoreReadinessTests|RestoreOrchestratorTests`：`27/27` 通过。
- 隔离 Debug 构建：XAML `24/24`；solution `0 warning / 0 error`；Playnite 目标保持 `net462`。
- `python scripts/validate-source.py` 通过；`git diff --check` 通过。
- 本批只改恢复确认守卫和 Worker 行为夹具，没有新增 XAML 或视觉资源；保留现有游戏选框、滚动条、命令绑定、取消/错误语义、PreRestore/回滚保护和有限列表策略。

## 边界与下一步

- 证据来自合成版本/Manifest、fake Worker、隔离目录和隔离构建输出；没有读写真实存档、删除真实媒体、写用户云端或外发诊断。
- 未运行真实 Playnite/package-host、全量 WPF、物理 DPI/跨屏、UIA/读屏、IME、presented frame、ETW 或宿主性能；不能把代理性能或离屏结果写成真实呈现结论。Demo 原目录不可用，本阶段沿用恢复生产基线。
- main 仍有用户 R08 改动和 `src.zip`，本阶段未触碰、未覆盖、未合并。
- 本阶段已先关闭 MSBuild 并按精确路径清理 `.tmp/r12-07-build-final`；VB/C# 编译器服务器 shutdown 被系统拒绝，目录仍有文件锁，未强杀未知进程。下一次启动应先重试该目录清理，避免把临时产物当作证据提交。
- 下一可执行任务为 `R12-08 恢复结果报告`：先核对已有任务结果、PreRestore、恢复文件范围和脱敏复制能力，再补部分完成/全部完成与任务 ID 追溯的实际行为负例。
