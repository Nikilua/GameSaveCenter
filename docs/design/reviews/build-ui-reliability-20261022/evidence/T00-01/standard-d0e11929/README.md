# d0e11929 标准根入口完整链

- 源码提交：`d0e1192935e7e161a1592d86eccbba3995419146`
- 标准入口：`GameSaveCenter-Run.cmd`，经 `cmd.exe /d /c "GameSaveCenter-Run.cmd < nul"` 执行，退出码0
- 构建诊断 run：`20261024T180031697Z-d82ddccb`
- 安装 run：`20261024T180029755Z-13c00e79`
- SDK：8.0.423；Windows 10.0.22000；Release solution build：0 warnings / 0 errors
- 测试：1967 total，1910 passed、57 skipped、0 failed；148份TRX计数一致、0 mismatch、145个WPF隔离类全部结束
- Core：157/157；Worker：400/400；Playnite源码与WPF测试零失败
- Worker win-x64发布、ZIP/PEXT打包、安装验证及Playnite启动全部成功
- 安装磁盘程序集版本、源码身份与SHA一致；没有核验进程内插件加载路径或新增真实页面交互

57项skip均是已撤销“今日工作台 UI 架构”断言的既有原因。安装摘要和包/程序集哈希见 [`verification.json`](verification.json) 与 [`install-identities.txt`](install-identities.txt)。

## 原始诊断

完整诊断仍保留在本机 `artifacts/build-diagnostics/20261024T180031697Z-d82ddccb`，隔离Release输出为 `artifacts/gsc-b/69170e28`，安装原报告位于本机 `artifacts/last-dev-install.txt`。这些可再生原始产物不提交Git；本目录保存脱敏的验证摘要和身份哈希。

该运行诊断记录 `workingTreeClean=false`，原因是此前已有的未跟踪TRX及 `tests/GameSaveCenter.Playnite.Tests/TestResults/`；提交跟踪文件干净，未跟踪用户证据没有暂存或清理。标准链仍以 `sourceCommit=d0e11929…` 构建、测试、打包并安装成功。
