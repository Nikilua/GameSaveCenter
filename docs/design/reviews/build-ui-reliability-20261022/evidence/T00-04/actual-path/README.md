# T00-04 真实路径构建

隔离本地共享对象克隆，源码提交 `05072195cc46e14856e31dc6d008888a5c0843ca`（产品源码与最后完整验收190bbaea相同）。原工作区、真实用户数据及全局Git配置未修改。克隆根含中文和空格，138字符；最长已检出跟踪路径324字符。只对隔离克隆设置core.longpaths=true。

- [直接PowerShell构建](path-build-direct.log.txt)：真实build.ps1 -SkipTests，SDK8.0.423，Release0警告/0错误，退出0；[原摘要](direct-summary.json)。
- [正常Desktop环境的CMD入口](path-build-cmd-clean.log.txt)：Windows PowerShell启动真实scripts/build.cmd -SkipTests，退出0，Release0/0；[原摘要](cmd-summary.json)。第二次构建为增量，首次直接构建包含实际还原/编译。
- [首次与重试失败](path-build-retry.log.txt)：由PowerShell7直接经CMD启动Desktop时，诊断初始化Get-FileHash不可用，退出1；未进入编译。原仓库同样复现，不能归因于中文或长路径。

只读对照：直接powershell的PSModulePath仅包含WindowsPowerShell目录；经cmd的子进程保留PowerShell7模块目录并优先解析。按名称导入Utility后仍无Get-FileHash；正常Desktop环境调用成功。尚未修改入口，污染环境的兼容性仍待修复和回归，因此本项保留待验收。

本次跳过测试，没有打包、安装或启动；不能替代190bbaea的1967项完整标准链。日志中dotnet --info的部分中文路径显示问号，实际构建和JSON摘要成功；不宣称所有原生控制台编码一致。隔离克隆暂留于本仓库.tmp下供入口修复复核，不是交付安装。旧启动器27/27仅证明退出码传递。