# T01-08 中断后的临时进程收尾

当前仍为“实施中”。受控中断和代码提交后的标准构建链已通过；真实桌面 Ctrl+C、另一台电脑同步后的构建仍待核验。

## 双宿主受控回归

- [PowerShell Core](bounded-Core.json) 与 [Windows PowerShell](bounded-Desktop.json) 的 `Test-BoundedProcess.ps1` 均退出 0。已有超时树清理、TRX/管道、UTF-8 与无关进程保护回归继续通过。
- 新增受控 managed-thread 中断：在 `Process.WaitForExit` 等待期间中断当前托管线程，确认本次启动的子进程按 PID 和启动时刻退出，无关进程仍在，并且后续命令重试退出 0。
- [PowerShell Core 临时目录序列](cleanup-Core.json) 与 [Windows PowerShell 临时目录序列](cleanup-Desktop.json) 均通过：退出码 7/124/0/0/0、成功重试后文件/字节不再增长、未知哨兵 SHA 不变、文件占用导致的清理失败可见且释放后可重试。
- Windows PowerShell 的进程树回归在默认沙箱里受 `taskkill /T` 的系统访问限制；相同受控脚本在提升的本机测试上下文退出 0。该限制不被记作功能通过证据。

`bounded-*.json` 明确记录 `CancellationSignal=managed-thread wait interruption; physical Ctrl+C not exercised`。测试覆盖取消/中断时的 `finally` 收尾，不等同于真实键盘取消。

## 提交后标准根入口完整链

代码提交 `fbd8a8828b2fac8e19dc1ef714b9bbd97ccbff85` 后，默认 `GameSaveCenter-Run.cmd` 完整退出 0。SDK 8.0.423 的 Release solution build 为 0 warnings / 0 errors；1967 项测试中 1910 passed、57 skipped、0 failed，148 份 TRX 计数一致，145 个 WPF 隔离类全部结束。Worker win-x64 发布、ZIP/PEXT、安装事务及 Playnite 启动成功。

构建诊断 run 为 `20261024T154742005Z-e15429e1`，安装 run 为 `20261024T154739936Z-bc3e83dc`；细节见[完整验证摘要](standard-fbd8a882/README.md)和[机器可读结果](standard-fbd8a882/verification.json)。原始本机诊断保留在 `artifacts/build-diagnostics/20261024T154742005Z-e15429e1`，当前构建输出在 `artifacts/gsc-b/20cc996b`。安装报告核验磁盘上的六个程序集版本、源码身份和 SHA；Playnite 已启动，但没有核验其进程内是否加载了这些程序集。

第一次候选根入口在脚本尚未提交时按打包身份保护退出 1；该次没有发布或安装，不能代替也不否定提交后的完整链结果。其诊断 `20261024T151721547Z-fc1457c1` 与构建目录 `cd302376` 保留。

## 尚未覆盖

真实 PowerShell/桌面 Ctrl+C 物理按键路径，以及用户另一台电脑先保留本地改动、快进到当前 `main` 后的构建仍待验证。当前不将 T01-08 提升为已验收。
