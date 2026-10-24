# T01-08 中断后的临时进程收尾

当前仍为“实施中”。本证据覆盖受控等待中断与清理行为；没有声称已通过真实桌面 Ctrl+C 验收。

## 双宿主受控回归

- [PowerShell Core](bounded-Core.json) 与 [Windows PowerShell](bounded-Desktop.json) 的 `Test-BoundedProcess.ps1` 均退出 0。已有超时树清理、TRX/管道、UTF-8 与无关进程保护回归继续通过。
- 新增受控 managed-thread 中断：在 `Process.WaitForExit` 等待期间中断当前托管线程，确认本次启动的子进程按 PID 和启动时刻退出，无关进程仍在，并且后续命令重试退出 0。
- [PowerShell Core 临时目录序列](cleanup-Core.json) 与 [Windows PowerShell 临时目录序列](cleanup-Desktop.json) 均通过：退出码 7/124/0/0/0、成功重试后文件/字节不再增长、未知哨兵 SHA 不变、文件占用导致的清理失败可见且释放后可重试。
- Windows PowerShell 的进程树回归在默认沙箱里受 `taskkill /T` 的系统访问限制；相同受控脚本在提升的本机测试上下文退出 0。该限制不被记作功能通过证据。

`bounded-*.json` 明确记录 `CancellationSignal=managed-thread wait interruption; physical Ctrl+C not exercised`。测试覆盖取消/中断时的 `finally` 收尾，不等同于真实键盘取消。

## 标准根入口候选运行

候选源码在提交前执行标准 `GameSaveCenter-Run.cmd`：XAML 校验通过、Release 0 警告/0 错误，核心 157/157、Worker 399 通过/1 跳过、Playnite 145 个隔离类全部结束，总计 1,899 通过/68 跳过/0 失败。额外 11 个跳过项来自受限沙箱无法访问 Named Pipe 和系统鼠标。

根入口随后按设计在打包阶段拒绝未提交的 `scripts/bounded-process.ps1` 与 `scripts/tests/Test-BoundedProcess.ps1` 改动（退出 1）；因此该候选运行没有发布、打包、安装或启动 Playnite。诊断 run `20261024T151721547Z-fc1457c1`、构建目录 `cd302376` 保留。代码提交后仍须重新运行完整标准根入口；成功结果到时再补录。

## 尚未覆盖

真实 PowerShell/桌面 Ctrl+C 物理按键路径、用户电脑上的同步后构建，以及代码提交后的完整构建/发布/安装/启动仍待验证。当前不将 T01-08 提升为已验收。
