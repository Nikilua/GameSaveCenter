# R12-06 恢复冲突说明证据

日期：2026-09-19  
分支：`codex/ui-finesse-round2`  
实现提交：`423856b2`（补充恢复冲突处理说明）

## 本阶段事实

先核对已有 `RestoreOrchestrator`、`RestoreReadinessDto`、`TaskStatusDto`、`RestoreWorkflowProgress` 和 SaveCenter 四阶段入口。本阶段没有新增恢复服务、DTO、IPC 或新的错误通道，而是在现有失败结果投影上增加原因分类和步骤级处理建议：

- 游戏运行/进程仍在运行：目标核对阶段保留原错误详情，提示退出游戏、启动器和相关 MOD 管理器，并明确不强制结束未知进程、不关闭安全检查。
- 操作锁：执行阶段复用已有任务锁/忙碌文本，提示等待任务中心完成，不并发重试、不绕过操作锁。
- 磁盘空间：恢复可用性检查读取已有失败摘要，识别隔离区或目标盘空间不足，提示清理或迁移后重新执行检查，不关闭空间检查。
- 目标无权限：写入失败详情中有明确权限证据时，提示核对 Playnite/Worker 账户 ACL 或受控文件夹权限；没有权限依据的泛化写入失败保持未知原因，不误导用户关闭安全机制。

处理建议只作为现有四步流程的展示状态，失败码、任务详情、取消/错误传播、PreRestore 保护、回滚和实际写入边界保持不变。SaveCenter 仍保留原游戏选框、滚动条系统、命令绑定、有限列表策略和 net462 目标。

## 自动验证

- 隔离 Debug 构建：`scripts/build.ps1 -Configuration Debug -SkipTests -OutputRoot .tmp/r12-06-build-final`；XAML `24/24`，solution `0 warning / 0 error`，Playnite 输出为 `net462`。
- Playnite 已构建 net472 测试程序集直接运行：R12 恢复流程与冲突说明 `11/11`，其中新增冲突说明 `4/4`，既有恢复流程 `7/7`。
- Worker 已构建测试程序集直接运行：`RestoreOrchestratorTests 12/12`，包含权限拒绝回滚、保护备份失败拦截、恢复后回滚、活动游戏会话拒绝等已有安全行为。
- `python scripts/validate-source.py` 通过；`git diff --check` 通过。
- 行为负例不是只检查字符串：测试同时验证阶段状态（目标失败或执行失败）、执行不会越过前置失败阶段、未知写入失败不会被错误归类为权限失败，以及 XAML 的 `ResolutionDisplay`/Automation HelpText 绑定实际存在。

## 视觉与宿主边界

本阶段只在现有恢复四步卡片中增加一行可换行的处理步骤文本，未重建页面、控件模板或滚动模型。按 Demo-first 使用已恢复的生产基线；原 Demo 目录事实不可用。未运行真实 Playnite/package-host，因此不能宣称最终嵌入呈现、屏幕阅读器/UIA、物理 DPI/跨屏、OS 输入/IME、presented frame、ETW 或宿主性能已验证。隔离数据使用合成 DTO/fake 和隔离输出目录，未写真实存档、媒体、云端或诊断数据。

main 仍有用户未提交的 R08 文件和 `src.zip`，本阶段没有覆盖或合并。

## 下一步

下一可执行任务为 `R12-07 预览失效重验`：先核对现有恢复预览/readiness 缓存失效和重验命令，覆盖过期结果、目标路径/版本变化、取消与失败负例；没有实际依据时不把缓存清理或重新读取写成真实恢复演练。
