# 576b689b 标准根入口完整链

2026-10-25，提交 `576b689b88f0f141c75def40b4fb601157045fcb` 运行默认 `cmd.exe /d /c "GameSaveCenter-Run.cmd < nul"` 退出0。构建run `20261025T063456418Z-21cff7b5`，安装run `20261025T063454422Z-7a7fb984`。

SDK8.0.423、Release0警告/0错误；Core157/157、Worker400/400。1968项测试1911通过、57跳过、0失败，149份TRX计数一致，146个WPF隔离类全部结束，155个步骤无非零退出。57项均为已撤销今日工作台UI架构的既有断言，未新增skip。

Worker win-x64发布、ZIP/PEXT、安装身份校验及Playnite启动完成。ZIP和PEXT SHA256相同；安装目录六个程序集与暂存文件逐项SHA一致，ProductVersion均包含完整576b689b提交身份。新Playnite PID47304与Worker PID47656已出现。本次只确认磁盘身份和进程启动，没有宣称进程内插件实际加载路径或完整真实页面矩阵通过。

证据见 [verification.json](verification.json)、[安装身份](install-identities.txt) 和对应修复类TRX。原始149份TRX及完整诊断仍在 `artifacts/build-diagnostics/20261025T063456418Z-21cff7b5`。后续源码或测试变更不能沿用此页宣称新候选完整链已通过。

归档后清理本轮11个自有失败候选/定向目录，共3788文件、860,738,000逻辑字节；D盘可用空间由10,168,287,232增至11,038,715,904字节。逐路径清单见 [cleanup.json](cleanup.json)。最新成功输出、包、全部原失败诊断及用户Round3 TRX/TestResults保留。归档文本使用LF换行，SHA清单按持久化内容计算。
