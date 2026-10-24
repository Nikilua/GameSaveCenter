# T00-08 失败后成功回归

## 本阶段范围

新增 `scripts/tests/Test-BuildFailureRecovery.ps1`，通过受控 `dotnet.cmd` shim 驱动真实 `scripts/build.ps1`：先运行显式 `-SkipTests` 的基线，再在核心测试进程注入 exit 23，最后不带 `-SkipTests` 重跑。断言三次构建各自留下独立诊断，失败运行保留真实非零码，重跑恢复 Core、Worker、Playnite discovery 与一个 WPF 隔离测试进程；还核对 `PATH`、临时目录和构建身份变量在每次运行后恢复。

`Test-SdkBootstrapRecovery.ps1` 可将下载中断、安装器损坏和后续成功结果归档到调用方指定的仓库目录，且仅在归档成功后清除本测试创建的 `.tmp` fixture。`Test-InstallTransaction.ps1` 将锁文件失败、改名回滚失败和成功重试的安装尝试 JSON 脱敏后分别保存，保留每次失败原因及成功暂存清理事实。

## 候选回放

提交前在 PowerShell Core 7.6.2 与 Windows PowerShell 5.1 各运行 SDK、安装和构建恢复夹具，六个脚本进程均退出 0。SDK组3个情形、安装组3条尝试均通过。构建夹具记录第一次测试 exit 23，并在下一次独立诊断运行中恢复全部4个测试组；`testsRequested=true`，TRX案例数为0，因为 `dotnet` 是控制流shim，没有编译程序集或执行任何真实测试案例。

上述是候选回放，受控 `dotnet` shim 本身不执行测试案例。之后在 `fbd8a8828b2fac8e19dc1ef714b9bbd97ccbff85` 上通过默认 `GameSaveCenter-Run.cmd` 完成真实还原、Release构建、测试、Worker发布、ZIP/PEXT打包、安装身份校验及Playnite启动：构建run `20261024T154742005Z-e15429e1`、安装run `20261024T154739936Z-bc3e83dc`，SDK `8.0.423`，Release 0警告/0错误，1967项测试1910通过、57跳过、0失败，148份TRX计数一致，145个WPF隔离类全部结束；根入口退出0。57项跳过均为同一条已撤销“今日工作台 UI 架构”断言。安装的六个关键程序集版本/SHA与构建输出一致；未声称该次验证了进程内插件加载路径或真实页面交互。原始诊断在本机 `artifacts/build-diagnostics/20261024T154742005Z-e15429e1`，详细见 [T01-08完整链证据](../T01-08/standard-fbd8a882/README.md)。

外机输出进一步确认 `git status` 为 `main...origin/main [behind 2]`、HEAD `ea926c5e`，工作树包含多项跟踪修改和未跟踪证据。其 `GamePickerViewModel.cs` diff 可见 `<<<<<<< Updated upstream` / `=======` / `>>>>>>> Stashed changes`，直接对应构建日志中的9个CS8300编译错误；这是该文件未解决的 stash 合并冲突。当前主线构建已在 `fbd8a882` 成功，之后文档记录提交为 `3d2fbeed`。外机在收到该推送后应先用 `git stash push --include-untracked` 保存所有本地修改和新文件，确认工作树干净，再 `git pull --ff-only origin main`，然后运行默认 `GameSaveCenter-Run.cmd`。该旧HEAD与当前远端相差8个提交。不得reset/覆盖，也不要在干净主线构建通过前pop stash；之后再审阅stash并逐项移植自己的改动。外机同步后的实际构建结果仍未收到。真实下载中断仍由隔离传输模拟。
