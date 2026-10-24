# T00-01 标准完整链：f577fd19

- 源码提交：`f577fd192709d5a312c7bc72fd92249b6b5e31ab`
- 标准入口：`GameSaveCenter-Run.cmd`，退出码 0
- 构建诊断 run：`20261024T135653993Z-90e04544`
- 安装运行 ID：`20261024T135651674Z-ade787f3`
- SDK：8.0.423；Release solution build：0 warnings / 0 errors
- 测试：1967 total，1910 passed、57 skipped、0 failed；148 TRX 计数一致，145 WPF 隔离类全部结束
- Worker win-x64 发布、ZIP/PEXT、安装和 Playnite 启动均成功。Playnite PID 26372 于 2026-10-24 22:24:18 启动。
- 安装六程序集的产品版本、完整源码身份和SHA已核对；本次没有核实进程内插件加载路径，也没有新增真实页面交互结论。
- 57 项跳过是已撤销“今日工作台 UI 架构”断言的同一跳过原因。

## 包身份

- ZIP 与 PEXT SHA-256 相同：`19AF45FC199A9245320BE3216584465B0AB2A71E02D07EDA3A3C68FACC597340`
- 六程序集安装 SHA：见 [`install-identities.txt`](install-identities.txt)

## 原始诊断

本机完整诊断保留于 `artifacts/build-diagnostics/20261024T135653993Z-90e04544`，隔离 Release 输出 `artifacts/gsc-b/229ce3b1`。安装报告及运行 attempt 原件保留于本机 `artifacts/last-dev-install.txt` 与 `artifacts/install-attempts/`。大型可再生输出不提交 Git。

T01-01 的双 PowerShell 失败阶段与安装暂存清单回归见 [`artifact-manifest-f577fd19`](../../T01-01/artifact-manifest-f577fd19/README.md)。
