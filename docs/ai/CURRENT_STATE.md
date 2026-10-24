# GameSaveCenter 当前事实

更新：2026-10-24。历史按当时提交解释，不能覆盖本页当前结论。

## 验证身份

- 当前完整验收代码：`190bbaea`，包含安装身份报告、诊断加载路径、命名测试窗口和Desktop超时进程树收尾。
- 最后完整标准入口成功/实际安装身份：`190bbaeac1c2b8e0cb871215c76f5cf2f5fa1ec3`。
- 命令：默认 `GameSaveCenter-Run.cmd`，退出0；run `20261024T031835306Z-29399c0f`。
- SDK8.0.423 / Release，XAML25/25、0警告/0错误；1967项1910通过、57既有跳过、0失败，148TRX一致、145WPF隔离类结束。
- 发布、ZIP/PEXT、安装六程序集SHA与构建身份一致；Playnite/Worker新进程及插件加载路径已确认。
- 最后定向验证：命名窗口独立三次5/5、双PowerShell进程树各5步骤；均已由当前完整标准链结合专项证据复核。
- [完整结果/交付身份/边界](../design/reviews/build-ui-reliability-20261022/evidence/T03-04/standard-190bbaea/README.md)。后续文档提交不是新产品构建身份。

- 前次aefbddfa的第101类崩溃保留；当前同类5/5、完整链0失败，不关闭UIA或扩大skip。真实诊断已显示190bbaea插件/Worker身份及实际加载路径。

## 当前范围

- 当前190bbaea真实首页与维护诊断已显示，插件/Worker构建身份及插件实际加载路径与安装报告一致；Ctrl+End可达诊断摘要，未触发业务操作。
- b8eedc47的存档列表/详情交叉滚动证据保留；不将它替代190bbaea的完整双主题/窄短窗矩阵。
- 完整双主题/正常窄短窗、命令面板宿主验收、物理键盘/IME及物理DPI仍待验。设置对话框坐标偏差后已停止主题切换，没有改配置。
- T02-02～07受控回归与当前标准链通过；T02-08实施中。T03已登记共享能力、安装报告缺口、回退候选及证据链接修复，未宣布32项整体完成。
- [32项专项账本](../design/reviews/build-ui-reliability-20261022/PROGRESS.md)、[第四轮机器账本](../design/reviews/ui-finesse-round4-20261018/ROUND4_STATUS.json)、[待办](../AUTONOMOUS_BACKLOG.md)是任务入口。
- SDK缺失为历史；CEF/ETW与S07-03 branch-protection读取权限边界独立保留，按[能力缓存](ENVIRONMENT_CAPABILITY_CACHE.json)重试。

## 保留、清理与后续

- 前次f25标准链测试通过，但运行中改文档触发打包保护退出1；[失败保留](../design/reviews/build-ui-reliability-20261022/evidence/T03-01/README.md)。先提交再冻结完整重跑已成功，未绕过保护。
- 25个本阶段测试结果文件先归档校验，再移除六个原临时目录；原失败、当前包/构建/诊断/宿主图与用户旧Round3 TRX/TestResults保留。详情见T02-08清理记录。
- 后续源码/测试/脚本/资源改动按影响运行定向检查与标准链；完整链期间不修改任何跟踪文件。纯文档按引用/源码校验，无需为其CI通过再追加记录提交。
- 继续T02-08证据矩阵、路径/SDK故障恢复与回退隔离验证，按32项账本逐项审计剩余条件；不要以受控或启动结果自动升级宿主验收。
- 旧事实原文见[截至2026-10-24历史](CURRENT_STATE_HISTORY_THROUGH_20261024.md)，更早记录仍可在Git和既有历史页追查。

## 持续有效的实现基线

- 产品版本0.6.73；共享入口为 `Themes/AcrylicProductionResources.xaml`、`DesignTokens.xaml`、`WpfUiProduction.xaml` 与 `Redesign.xaml`。
- Demo-first优先级保留；`GameSaveCenter.AcrylicFork/src/GameSaveCenter.Playnite/Design/` 当前工作区不存在，缺源时沿用生产共享样式，不伪造Demo对照。
- 保留业务命令、Binding、安全/取消/错误语义、虚拟化、UIA、键盘与Playnite兼容；游戏平台筛选以 `DropDownClosed` 提交。
- 行为矩阵入口 `scripts/e01-behavior-matrix.ps1`；`MANUAL QA REQUIRED` 项仍需对应人工证据，自动测试不替代。
- 开发启动继续阅读[PROJECT_MEMORY.md](PROJECT_MEMORY.md)、[WORKLOG.md](WORKLOG.md)、[DEVELOPMENT_HANDOFF.md](../DEVELOPMENT_HANDOFF.md)，按记忆协议推进。
