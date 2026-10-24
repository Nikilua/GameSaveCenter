# T01-01 产物清单验收：f577fd19

本阶段给失败诊断增加显式 `failedStageId`；非零命令/启动失败记录其 step ID，XAML/SDK/其他编排异常记录 `orchestration`。安装运行生成独立 Run ID， attempt JSON 包含 attempt ID、进程 PID/启动时刻和 artifact entries。暂存目录与旧安装回滚目录分别标记阶段、本次创建归属、结束时是否存在、Active 状态及保留理由。外部 Playnite Extensions 路径只用于诊断，明确不能传给仓库清理器。

## 双宿主回归

SDK `8.0.423`，源码提交 `f577fd192709d5a312c7bc72fd92249b6b5e31ab`。

- PowerShell Core `7.6.2`：受控失败后恢复回归通过；Core 测试注入 exit `23` 时 summary 的 `failedStageId=core`。新一轮恢复了 Core、Worker、Playnite discovery 和单个 WPF 隔离进程。控制流 shim 不执行真实案例，TRX案例数为0。
- Windows PowerShell `5.1.22000.2538`：相同失败阶段与成功重试断言通过。
- 两个宿主的安装夹具均 `3/3`：锁占用失败、安装改名失败并回滚、成功重试。每个 attempt 均关联同一安装 Run ID 和唯一 Attempt ID，staging artifact lifecycle 完整；回滚备份有记录且最终不存在。
- 两个 BuildFailureRecovery 摘要中的源提交均为 `f577fd19`；dirty paths 只含既有用户未跟踪 TRX/TestResults，夹具未碰它们。

夹具摘要、受控阶段日志、安装事务结果和脱敏 attempt records 位于本目录各宿主子目录。

## 提交级标准完整链

在同一源码提交上，默认 `GameSaveCenter-Run.cmd` run `20261024T135653993Z-90e04544` 完整退出0：Release 0 warning/0 error；1967项1910通过、57跳过、0失败，148 TRX一致，145个WPF隔离类；Worker发布、ZIP/PEXT、安装及Playnite启动成功。安装 run `20261024T135651674Z-ade787f3` 的生产 attempt 确认 `install-staging` 与 `install-backup` 均由本次创建、事务结束后不存在且标记为 inactive。完整构建/安装证据见 [T00-01标准全链](../../T00-01/standard-f577fd19/README.md)。

该任务不授予清理外部安装目录的权限；真实下载中断仍未实测。
