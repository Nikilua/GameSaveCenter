# 190bbaea 标准完整链与安装身份核验

源码 `190bbaeac1c2b8e0cb871215c76f5cf2f5fa1ec3`，默认 `GameSaveCenter-Run.cmd` 完整退出0；run `20261024T031835306Z-29399c0f`，SDK8.0.423、XAML25/25、Release0警告/0错误。1967项、1910通过、57既有跳过、0失败/error/timeout/abort，148TRX计数一致、145个WPF隔离类结束。未使用SkipTests、NoStart等替代选项。[交付核验](verification.json)、[跳过逐名对照](skip-comparison.json)。

前次aefbddfa在第101类testhost崩溃；本次同类5/5，原业务断言及正常WindowAutomationPeer名称断言均保留，[本轮TRX](R12RemoteStageProgressBehaviorTests.trx)。原失败在相邻uia-title-failure目录保留，不被本次成功覆盖。

Worker发布、包内部身份与ZIP/PEXT均通过。独立复算六个stage/安装DLL的SHA与ProductVersion，逐项核对[新安装报告](last-dev-install.txt)也一致；全部为0.6.73+190bbaea完整提交。报告明确磁盘身份不等于进程已加载。

11:44日志显示应用启动并从目标扩展目录加载插件，见[当时加载日志](plugin-load-log.txt)。14:13查询未见进程，不将历史启动当作一直存活。随后通过computer-use重新打开既有安装：真实维护诊断页同时显示插件和Worker完整190bbaea身份，并显示插件实际加载路径；Ctrl+End到达底部摘要。路径整行长于当前文本视口，不声称整行同时可见。当前Worker运行缓存DLL与安装DLL另做SHA对照一致，进程与截图SHA在verification.json的interactiveRecheck。截图只在本地artifacts/host-audit-190bbaea保留，未加入Git；没有执行巡检、备份、恢复或删除业务动作。

[清理](cleanup.json)：三个被本次完整构建替代的定向构建目录已检查归属、路径/链接和独占文件探测后移除，保留关键程序集身份。原定向TRX已归档；原始失败DLL26e5d1bd、全部失败/成功诊断、当前eb609571构建、包和宿主图保留。用户旧Round3 TRX/TestResults未动。此前三个结果目录及一次性IL脚本先归档再移除，详细原记录保留在artifacts/t03-prebuild-cleanup-190bbaea.json；归档目录artifacts/t03-local-evidence-190bbaea仍在用。

本批证明T00-06、T03-03/04的当前完整链与身份路径；不代表32项整体完成，也不代表双主题/窄短窗口矩阵、物理DPI、真实IME或全部旧宿主任务已验收。纯文档提交不产生新的产品安装身份。
