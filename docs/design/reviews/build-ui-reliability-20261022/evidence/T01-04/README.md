# T01-04 路径边界负例

[Test-ArtifactPathBoundaries.ps1](../../../../../../scripts/tests/Test-ArtifactPathBoundaries.ps1) 调用生产 `Get-GscArtifactPreview`，只在每轮新建的 `.tmp/t01-04-*` 隔离目录中创建哨兵、junction 和可选文件符号链接。PowerShell 7.6.2/Core 与 Windows PowerShell 5.1.22000.2538 各运行 13 项有效断言，均通过：空路径、相对路径、`..` 越界、相似前缀、允许根本身、其他盘符、盘符相对越界、盘符/路径大小写、未声明所有权、junction 本身及含 junction 的目录。所有预览都返回 `CanDelete=false`，哨兵 SHA-256 不变，隔离目录已清理。

测试脚本与双宿主结果提交于 `d2429890`。

文件符号链接创建在两个宿主均受操作系统权限限制：PowerShell 7 报告需要管理员权限，Windows PowerShell 的 `mklink` 返回权限不足。因此该用例记为 1 项跳过，不能声称符号链接本身已运行；junction 和通用 ReparsePoint 边界已覆盖。预览接口没有删除动作，测试验证其不能授权删除且不改变隔离数据。逐版本明细：[Core](results-Core.json)、[Desktop](results-Desktop.json)。

2026-10-25 在提升测试上下文再次运行Core夹具，仍以“Administrator privilege required”拒绝创建文件符号链接：13项通过、1项跳过、0失败，`CanDelete=false`，哨兵SHA不变，`.tmp/t01-04-*`本次夹具已清理。结果见[提升上下文回归](results-Core-elevated-20261025.json)。当前宿主没有获得创建该链接的权限，不重复尝试或更改Windows安全设置；T01-04保持代码完成待验收。

这批只扩充路径预览负例，没有改变构建、安装或生产 UI。T01-04 保持待完成状态，直到符号链接行为可以实际验证，或验收边界经证据审查确认可由 junction 充分代表。
