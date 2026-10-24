# T00-08 失败后成功回归

## 本阶段范围

新增 `scripts/tests/Test-BuildFailureRecovery.ps1`，通过受控 `dotnet.cmd` shim 驱动真实 `scripts/build.ps1`：先运行显式 `-SkipTests` 的基线，再在核心测试进程注入 exit 23，最后不带 `-SkipTests` 重跑。断言三次构建各自留下独立诊断，失败运行保留真实非零码，重跑恢复 Core、Worker、Playnite discovery 与一个 WPF 隔离测试进程；还核对 `PATH`、临时目录和构建身份变量在每次运行后恢复。

`Test-SdkBootstrapRecovery.ps1` 可将下载中断、安装器损坏和后续成功结果归档到调用方指定的仓库目录，且仅在归档成功后清除本测试创建的 `.tmp` fixture。`Test-InstallTransaction.ps1` 将锁文件失败、改名回滚失败和成功重试的安装尝试 JSON 脱敏后分别保存，保留每次失败原因及成功暂存清理事实。

## 候选回放

提交前在 PowerShell Core 7.6.2 与 Windows PowerShell 5.1 各运行 SDK、安装和构建恢复夹具，六个脚本进程均退出 0。SDK组3个情形、安装组3条尝试均通过。构建夹具记录第一次测试 exit 23，并在下一次独立诊断运行中恢复全部4个测试组；`testsRequested=true`，TRX案例数为0，因为 `dotnet` 是控制流shim，没有编译程序集或执行任何真实测试案例。

上述是候选回放，受控 `dotnet` shim 本身不执行测试案例。随后 `dcbaab13b9464523ffed6d88ca2748b6fb6dd647` 通过默认 `GameSaveCenter-Run.cmd` 完成真实还原、Release构建、测试、Worker发布、ZIP/PEXT打包、安装身份校验及Playnite启动：构建run `20261024T192816167Z-276884c0`、安装run `20261024T192814223Z-736788e1`，SDK `8.0.423`，Release 0警告/0错误，1967项测试1910通过、57跳过、0失败，148份TRX计数一致，145个WPF隔离类全部结束；根入口退出0。安装的六个关键程序集版本/SHA与构建输出一致；未声称该次验证了进程内插件加载路径或真实页面交互。最新完整链摘要见 [T03-08验证记录](../T03-08/verification.json)。

2026-10-24外机先前另有一份单类日志：`R09FocusOutlineBehaviorTests` 2项中1项失败，报告期望 `2,2,2,2`、实际 `1,1,1,1`。2026-10-25在当前 `c9cc5a99` 用全新的隔离输出目录重新还原、Release编译并运行完整 `scripts/build.ps1`：0警告/0错误，1967项测试1899通过、68跳过、0失败，145个WPF隔离类全部结束；该R09类也通过。旧失败在当前源码复验中没有重现，未据旧日志改动样式。此次只运行解决方案构建和测试；Playnite/Worker仍在运行，因此没有重复安装/启动步骤。源码、测试、脚本自 `dcbaab13` 后无变化，完整安装链仍由上段提交级证据覆盖。

另一台电脑随后回传的根入口日志显示：HEAD `ea926c5e`、`main...origin/main [behind 2]`，多项本地修改和未跟踪证据；`GamePickerViewModel.cs` 有 `<<<<<<< Updated upstream` / `=======` / `>>>>>>> Stashed changes`。因此出现9个 `CS8300`，测试数为0且没有开始测试；这次失败是未解决的 stash 合并冲突，不是 SDK/NuGet。先按以下步骤保留本地工作并在干净主线上验证：

```powershell
git stash push --include-untracked -m "保存本机 GamePicker 构建前改动"
git status --short --branch
git pull --ff-only origin main
git rev-parse --short HEAD
cmd.exe /d /c "GameSaveCenter-Run.cmd < nul"
```

确认 stash 后工作树干净、快进成功且 HEAD 与 `origin/main` 一致，再运行根入口。保持 stash 不动，直到完整链退出0；不要 `reset`、覆盖文件或 `stash pop`。构建通过后可逐项审阅stash并移植需要的改动。外机同步后的实际构建结果仍未收到，不能标为成功。真实下载中断仍由隔离传输模拟。
