# fbd8a882 标准根入口完整链

- 源码提交：`fbd8a8828b2fac8e19dc1ef714b9bbd97ccbff85`
- 标准入口：`GameSaveCenter-Run.cmd`，通过 `cmd.exe /d /c "GameSaveCenter-Run.cmd < nul"` 执行，退出码 0
- 构建诊断 run：`20261024T154742005Z-e15429e1`
- 安装运行 ID：`20261024T154739936Z-bc3e83dc`
- SDK：8.0.423；Release solution build：0 warnings / 0 errors
- 测试：1967 total，1910 passed、57 skipped、0 failed；148 TRX 计数一致，145 个 WPF 隔离类全部结束
- Core：157/157；Worker：400/400；Playnite 源码及隔离 WPF 测试均无失败
- Worker win-x64 发布、ZIP/PEXT、安装事务与 Playnite 启动均成功
- 安装磁盘程序集版本、完整源码身份和 SHA 已核对；没有核验进程内插件加载路径或新增真实页面交互

57 项跳过沿用已撤销“今日工作台 UI 架构”断言的既有跳过原因。

## 包身份

ZIP 与 PEXT SHA-256 相同：`C205F550B1C03F45B90E26625B4D1538CFCA68510FFAE22E104C60A14DE932FC`。六个安装程序集的版本和 SHA 见 [`install-identities.txt`](install-identities.txt)。

## 原始诊断

本机原始诊断保留于 `artifacts/build-diagnostics/20261024T154742005Z-e15429e1`，隔离 Release 输出 `artifacts/gsc-b/20cc996b`。安装报告与 attempt 原件保留在本机 `artifacts/last-dev-install.txt` 和 `artifacts/install-attempts/`；这些可再生输出未提交 Git。机器可读的脱敏摘要见 [`verification.json`](verification.json)。

Playnite 在安装后启动；这里仅确认启动成功和安装目录磁盘文件身份，不代表确认该进程实际加载了这些程序集。
