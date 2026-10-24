# T00-07 checkout 并发锁复核

## 实现

锁名按规范化 checkout 路径的 SHA-256 派生。原 `Local\` 名称只在当前 Windows 登录会话可见；实现改用 `Global\`，让共享 checkout 的锁跨 Windows 会话可见。Windows 对 named kernel object 的 Global/Local 命名空间规则见[Microsoft 文档](https://learn.microsoft.com/en-us/windows/win32/termserv/kernel-object-namespaces)。同一会话的既有重入、并发拒绝、释放重试、异常退出恢复和不同 checkout 独立性均保留。

## 验证

- `scripts/tests/Test-CheckoutLock.ps1`：PowerShell Core 7.6.2 与 Windows PowerShell 5.1.22000.2538 各 5/5，覆盖重入、独立子进程并发拒绝、释放后重试、持锁进程异常退出后的 abandoned mutex 恢复、不同 checkout 独立。结果见 `lock-Core.json` 与 `lock-Desktop.json`。这些进程检查均在当前 Windows 会话运行。
- `scripts/tests/Test-CheckoutLockCrossSession.ps1` 尝试通过同一用户的 S4U 临时计划任务进入另一会话。本机账号无权访问 Task Scheduler WMI / 注册任务；运行记录为 `TaskCreated=false`、`TaskRemoved=true`、`FixtureRemoved=true`，没有留下计划任务或 `.tmp` 夹具。跨会话实际互斥仍待有权限的 Windows 会话验证，不以 Global 名称本身冒充运行时证据。
- 候选源码 `3c8a0854` 上，默认根入口完成 Release 编译（0 警告、0 错误）及 1967 项测试编排（1899 通过、68 跳过、0 失败；148 TRX 计数一致、145 个 WPF 类结束）。68 项跳过分别为 57 个已撤销架构断言、10 个本执行环境不允许的 Named Pipe 行为、1 个系统指针访问限制。打包器检测到尚未提交的锁源码/测试更改后按身份保护退出1，未发布、打包、安装或启动；run `20261024T185640550Z-8d576492` 的原始诊断保留于本地 `artifacts/build-diagnostics/`。这不构成该候选的完整链验收；锁源码提交后仍须重跑默认根入口全链。
- 提交 `dcbaab13b9464523ffed6d88ca2748b6fb6dd647` 后，在跟踪文件干净的源码身份上重跑默认根入口，退出码0；SDK8.0.423、Release0/0，1967项1910通过/57跳过/0失败，148份TRX计数一致、145个WPF类结束。Worker发布、ZIP/PEXT、安装和Playnite启动通过，六程序集安装身份及SHA一致。完整数据见[提交级全链证据](../T00-01/standard-dcbaab13/README.md)。本次未核验Playnite进程内程序集加载路径。

## 当前边界

T00-07 仍为代码完成待验收：全局锁实现和同会话测试有证据，跨会话任务受当前系统权限阻挡。跨用户账号的 Global mutex ACL 行为也没有在本项中验证；若访问权限不足，入口必须在写构建/安装产物前失败。
