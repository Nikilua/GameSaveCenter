# T00-04 真实路径构建

隔离本地共享对象克隆，源码提交 `05072195cc46e14856e31dc6d008888a5c0843ca`（产品源码与最后完整验收190bbaea相同）。原工作区、真实用户数据及全局Git配置未修改。克隆根含中文和空格，138字符；最长已检出跟踪路径324字符。只对隔离克隆设置core.longpaths=true。

- [直接PowerShell构建](path-build-direct.log.txt)：真实build.ps1 -SkipTests，SDK8.0.423，Release0警告/0错误，退出0；[原摘要](direct-summary.json)。
- [正常Desktop环境的CMD入口](path-build-cmd-clean.log.txt)：Windows PowerShell启动真实scripts/build.cmd -SkipTests，退出0，Release0/0；[原摘要](cmd-summary.json)。第二次构建为增量，首次直接构建包含实际还原/编译。
- [首次与重试失败](path-build-retry.log.txt)：由PowerShell7直接经CMD启动Desktop时，诊断初始化Get-FileHash不可用，退出1；未进入编译。原仓库同样复现，不能归因于中文或长路径。

只读对照：直接powershell的PSModulePath仅包含WindowsPowerShell目录；经cmd的子进程保留PowerShell7模块目录并优先解析。按名称导入Utility后仍无Get-FileHash；正常Desktop环境调用成功。尚未修改入口，污染环境的兼容性仍待修复和回归，因此本项保留待验收。

本次跳过测试，没有打包、安装或启动；不能替代190bbaea的1967项完整标准链。日志中dotnet --info的部分中文路径显示问号，实际构建和JSON摘要成功；不宣称所有原生控制台编码一致。隔离克隆暂留于本仓库.tmp下供入口修复复核，不是交付安装。旧启动器27/27仅证明退出码传递。
## 入口修复及复验（2026-10-24）

build.cmd增加setlocal并清空仅子进程继承的PSModulePath，让Desktop自行初始化其默认模块目录；调用者环境不变，脚本退出码原样返回。没有更改系统模块、全局配置或构建断言。

[双宿主入口Core](launcher-Core.json)、[Desktop](launcher-Desktop.json)各9/9：三入口0/7/23透传、失败不报成功；build.cmd真实调用Get-FileHash，父PSModulePath逐项不变。测试脚本明确exit0，避免最后一个预期的原生23污染成功测试的进程退出码。

[修复后真实CMD构建](path-build-fixed.log.txt)在同一138字符中文空格根、原PS7父环境中退出0，SDK8.0.423，完整新输出Release0警告/0错误。[原摘要](fixed-summary.json)明确HEAD05072195加唯一dirty scripts/build.cmd，不伪称提交已包含修复；入口SHA256 E08E77DD9B64079B5024DA458F80B6D9896E3954B6F1D0D2F8CA2A7AA1941872。SkipTests不替代最终完整链，后者将在修复提交后冻结运行。

[清理探测](cleanup-probe.json)：三个已归档启动器夹具共45文件删除；隔离克隆6204文件约546MB有17个编译器分析器占用，保留至释放后复查，不强杀共享编译器。进程CIM读取在沙箱被拒，未把缺失观察当成无活动进程。原始失败与成功日志/摘要均已归档。

最终CMD保持仓库CRLF，SHA256 873C6AECBC86876BADC7ED6458F9CD735EF9E0FA1A7FB4756C0476E97291A552；[同路径增量复核](path-build-fixed-crlf.log.txt)退出0、Release0/0，[摘要](fixed-crlf-summary.json)。双宿主9/9记录已用最终CRLF入口重跑替换；JSON显式UTF8，解决Desktop默认ANSI证据编码。两份额外夹具30文件已归档后清理。

最终JSON无BOM UTF8写入后双宿主再各9/9、退出0；记录已替换为该次结果。另30文件经归档、无链接/独占检查后移除。累计本阶段7个启动器夹具105文件已移除；克隆保留原因不变。
