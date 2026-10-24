# T00-03 SDK 恢复控制流复验

复验基线为提交 `20c02b5d` 加本次工作区中的测试脚本版本，脚本 SHA-256：`AEA80E4C034CF91BC88D0164CC94E0993FA1922C318AA6C4B6261A71DCFC20C3`。PowerShell Core `7.6.2` 与 Windows PowerShell `5.1.22000.2538` 均运行 `scripts/tests/Test-SdkBootstrapRecovery.ps1`，各退出码 `0`、4/4 场景通过；每个运行的 JSON 采用 UTF-8，SHA-256 分别见下方。

| 场景 | 验证结果 |
| --- | --- |
| 下载中断 | 先写入部分安装脚本再注入传输中断；错误给出网络/代理和手动安装建议，恢复进程环境，清理本次 bootstrap 临时目录 |
| 损坏临时安装脚本 | 写入未闭合的 PowerShell 函数，子进程解析失败并返回非零；失败诊断保留安装器退出码 |
| 安装器退出 0 但 SDK 仍不匹配 | 安装器模拟退出 0，SDK 观察仍只见 `9.0.302`；拒绝视为成功并报告缺少精确 `8.0.423`，未写入成功标记 |
| 重试 | 前三次失败后再次尝试，只有精确观察到 `8.0.423` 才返回成功；总下载/安装尝试为 4 |

两种宿主都执行了真实 `Initialize-GscSdkBaseline` 控制流和子 PowerShell 调用；网络传输、SDK 版本观察及最终安装器成功载荷由隔离夹具模拟。因此这验证了故障诊断、环境恢复、临时文件清理、错误退出处理和精确 SDK 后验，不代表真实网络中断或真实 SDK 压缩包损坏恢复。本机缺 SDK 引导、冷 NuGet 还原和当前候选标准完整链分别见 [实际冷环境验证](../clean-machine-bootstrap.md) 与 [标准链证据](../../T01-08/standard-fbd8a882/README.md)。

| 宿主 | 结果文件 | SHA-256 |
| --- | --- | --- |
| PowerShell Core 7.6.2 | [sdk-bootstrap-recovery.json](../recovery-20261025-verified-pwsh/sdk-bootstrap-recovery.json) | `94101810F5C681C4E7787302584CA45E367BE6857A7D28BAB0DAFF31ED5AE0D6` |
| Windows PowerShell 5.1.22000.2538 | [sdk-bootstrap-recovery.json](../recovery-20261025-verified-desktop/sdk-bootstrap-recovery.json) | `EFAC8DF1A4CFCBAA2F125F18253ABFA4FA780CF15C2E1375BCA9F99C935D26C0` |
