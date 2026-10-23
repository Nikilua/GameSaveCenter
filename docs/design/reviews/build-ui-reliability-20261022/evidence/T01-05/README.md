# T01-05 活跃运行保护

`Test-ArtifactActiveRunProtection.ps1` 在 PowerShell 7.6.2/Core 与 Windows PowerShell 5.1.22000.2538 各运行一次，结果见 [Core](results-Core.json) 和 [Desktop](results-Desktop.json)。测试会启动实际的同版本子 PowerShell 进程，将 PID、进程启动时间和唯一 run ID 写入隔离清单，并调用生产只读预览验证进程身份。活跃所有者期间预览返回 `OwnerState=active`、`OccupancyState=owner-active`、`CanDelete=false`；为避免干扰活跃构建/测试，身份仍活跃时不尝试独占打开其文件。

子进程退出后，测试持有隔离哨兵文件的 `FileShare.None` 句柄。预览识别 `OwnerState=exited` 与 `OccupancyState=in-use`，报告占用文件名并保持哨兵哈希不变；释放句柄后状态变为 `clear`，预览仍不授权删除。另以当前 PID 配旧启动时间验证返回 `different-process`，并用缺少 owner 的清单验证返回 `unknown/owner-unknown`，不会把 PID 重用或身份缺失误认为安全候选。

测试还持有当前 checkout 的构建互斥锁，并启动真正的 `capture-ui-audit.ps1`。审计入口快速以非零码报告已有活动运行，指定输出目录中的哨兵保持原样，说明第二个审计不会先清空前一轮输出。构建/安装和 UI 审计共用 checkout 锁，避免二者并行清理或写入同一 checkout 的产物。

这次变更后也重新运行了 [T01-04 路径边界矩阵](../T01-04/README.md)，两宿主各13项有效断言通过，路径及链接保护行为保持有效。文件符号链接创建仍分别受系统权限限制而各跳过1项；结果保留在 T01-04 的双宿主 JSON 中。

生产预览和 UI 审计脚本不提供通用删除授权；所有结果仍为 `CanDelete=false`。占用探测是只读、瞬时的人工复核信号，不是后续删除的原子保证；任何清理仍须重新验证运行身份、路径、引用和占用。测试仅创建并清理仓库内唯一命名的 `.tmp/t01-05-*` 与 `artifacts/t01-05-*` 隔离夹具。

标准根入口在提交 `b55d8d1d` 完整退出0，见[完整链证据](standard-verification.json)。1955项测试1898通过、57既有跳过、0失败，发布、安装验证与Playnite启动完成。
