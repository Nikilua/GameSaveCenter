# T00-08 失败后成功回归

## 本阶段范围

新增 `scripts/tests/Test-BuildFailureRecovery.ps1`，通过受控 `dotnet.cmd` shim 驱动真实 `scripts/build.ps1`：先运行显式 `-SkipTests` 的基线，再在核心测试进程注入 exit 23，最后不带 `-SkipTests` 重跑。断言三次构建各自留下独立诊断，失败运行保留真实非零码，重跑恢复 Core、Worker、Playnite discovery 与一个 WPF 隔离测试进程；还核对 `PATH`、临时目录和构建身份变量在每次运行后恢复。

`Test-SdkBootstrapRecovery.ps1` 可将下载中断、安装器损坏和后续成功结果归档到调用方指定的仓库目录，且仅在归档成功后清除本测试创建的 `.tmp` fixture。`Test-InstallTransaction.ps1` 将锁文件失败、改名回滚失败和成功重试的安装尝试 JSON 脱敏后分别保存，保留每次失败原因及成功暂存清理事实。

## 候选回放

提交前在 PowerShell Core 7.6.2 与 Windows PowerShell 5.1 各运行 SDK、安装和构建恢复夹具，六个脚本进程均退出 0。SDK组3个情形、安装组3条尝试均通过。构建夹具记录第一次测试 exit 23，并在下一次独立诊断运行中恢复全部4个测试组；`testsRequested=true`，TRX案例数为0，因为 `dotnet` 是控制流shim，没有编译程序集或执行任何真实测试案例。

上述是提交前回放，不能替代该提交上的干净回放或实际构建。提交后将归档两种PowerShell的摘要/日志，再运行默认 `GameSaveCenter-Run.cmd` 完成真实还原、Release构建、测试、打包、安装身份校验及启动。真实下载中断仍由隔离传输模拟，另一台电脑的失败根因仍需其同次 `artifacts/one-click-install.log` 和 `artifacts/build-diagnostics/<run-id>/summary.json` 才能确认。
