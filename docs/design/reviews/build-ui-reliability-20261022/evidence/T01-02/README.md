# T01-02 清理预览验收

当前代码身份 `f577fd19`；`scripts/artifact-preview.ps1`、`scripts/preview-artifacts.ps1` 与对应测试脚本自该提交以来未变。该任务只交付只读预览能力，不接入删除操作。

## 双 PowerShell 复验

- PowerShell Core `7.6.2`：运行 `pwsh.exe -NoProfile -ExecutionPolicy Bypass -File scripts/tests/Test-ArtifactPreview.ps1`，退出码 `0`，`9/9` 通过，哨兵 SHA 未变；结果见 [`preview-Core.json`](preview-Core.json)。
- Windows PowerShell `5.1.22000.2538`：运行 `powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/tests/Test-ArtifactPreview.ps1`，退出码 `0`，`9/9` 通过，哨兵 SHA 未变；结果见 [`preview-Desktop.json`](preview-Desktop.json)。

用例覆盖活动进程与启动时间匹配、JSON 时间往返、PID 重用、空路径/相对路径/越界与相似前缀/仓库临时根本身、junction 后代，以及保留理由。两份结果均包含路径、估算字节数、所有者/占用状态和原因；所有行的 `CanDelete` 都是 `false`。活动拥有者、未知/过期身份、保留项、链接边界及非法路径不会被授权；可审查候选仍要求人工检查证据引用和文件占用。

未知所有者与活动文件锁的补充双宿主样例见 [T01-05 活跃运行保护](../T01-05/README.md)。

证据中的本机根路径已归一为 `[repo]`，越界目标归一为 `[outside]`。不因预览结果声称已有自动清理器，也不删除未知产物。源码只读检查还确认只有本次明确创建且没有活跃/保留状态的产物才会进入“待复核候选”说明。
