# dcbaab13 标准根入口完整链

- 源码提交：`dcbaab13b9464523ffed6d88ca2748b6fb6dd647`
- 标准入口：`GameSaveCenter-Run.cmd`，经 `cmd.exe /d /c "GameSaveCenter-Run.cmd < nul"` 执行，退出码0
- 构建诊断 run：`20261024T192816167Z-276884c0`
- 安装 run：`20261024T192814223Z-736788e1`
- SDK：8.0.423；Windows 10.0.22000；PowerShell Core 7.6.2；Release solution build：0 warnings / 0 errors
- 测试：1967 total，1910 passed、57 skipped、0 failed；148份TRX计数一致、0 mismatch、145个WPF隔离类全部结束
- Core：157/157；Worker：400/400；Playnite源码：544/562；WPF隔离：809/848；所有组0失败
- Worker win-x64发布、ZIP/PEXT打包、安装验证及Playnite启动全部成功
- ZIP与PEXT SHA-256相同：`A085FFC7DEDF2CF799A0F65410E984F22D2E68B565DE3C80FB3D650B17AC1A2B`
- 六个安装目录程序集身份均为 `0.6.73+dcbaab13b9464523ffed6d88ca2748b6fb6dd647`，文件SHA与独立复算一致；没有核验进程内插件加载路径

57项skip均是已撤销“今日工作台 UI 架构”断言的既有原因。安装摘要和程序集哈希见 [`verification.json`](verification.json) 与 [`install-identities.txt`](install-identities.txt)。

## 原始诊断

完整诊断仍保留在本机 `artifacts/build-diagnostics/20261024T192816167Z-276884c0`，隔离Release输出为 `artifacts/gsc-b/ae74f424`，安装原报告位于本机 `artifacts/last-dev-install.txt`。这些可再生原始产物不提交Git；本目录保存脱敏的验证摘要和身份哈希。

运行摘要标记 `workingTreeClean=false`，原因是运行前已存在的未跟踪Round3 TRX及 `tests/GameSaveCenter.Playnite.Tests/TestResults/`；提交跟踪文件干净，用户证据没有暂存或删除。
