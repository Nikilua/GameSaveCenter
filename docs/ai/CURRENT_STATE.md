# GameSaveCenter 当前事实

更新：2026-10-24。历史按当时提交解释，不能覆盖本页当前结论。

## 验证身份

- 最后产品测试代码：`f25be527`；其后 `b8eedc47` 只变更审查文档、链接及失败证据。
- 最后完整标准入口成功/实际安装身份：`b8eedc4797ba39639a771456c86ed1911be0e153`。
- 命令：默认 `GameSaveCenter-Run.cmd`，退出0；run `20261024T014633742Z-7d44e664`。
- SDK8.0.423 / Release，XAML25/25、0警告/0错误；1967项1910通过、57既有跳过、0失败，148TRX一致、145WPF隔离类结束。
- 发布、ZIP/PEXT、安装六程序集SHA与构建身份一致；Playnite/Worker新进程及插件加载路径已确认。
- 最后定向验证：T02-07提交前最终详情夹具5/5、横竖图比较1/1；本次标准全链已覆盖当前实现。见[定向TRX](../design/reviews/build-ui-reliability-20261022/evidence/T02-07/t02-07-details-final.trx)。
- [完整结果/交付身份/边界](../design/reviews/build-ui-reliability-20261022/evidence/T02-08/README.md)。后续文档提交不是新产品构建身份。

- 新诊断改进：T03-03只读预览首屏已通过双PowerShell压力/旧TRX/脱敏回归；不改变上述产品安装身份，新最终候选完整链仍待统一执行。

## 当前范围

- 真实Playnite首页与存档页已显示；注入列表/详情滚动观察到各自位置独立，底部校验动作可见，未触发业务操作。
- 完整双主题/正常窄短窗、命令面板宿主验收、物理键盘/IME及物理DPI仍待验。设置对话框坐标偏差后已停止主题切换，没有改配置。
- T02-02～07受控回归与当前标准链通过；T02-08实施中。T03已登记共享能力、安装报告缺口、回退候选及证据链接修复，未宣布32项整体完成。
- [32项专项账本](../design/reviews/build-ui-reliability-20261022/PROGRESS.md)、[第四轮机器账本](../design/reviews/ui-finesse-round4-20261018/ROUND4_STATUS.json)、[待办](../AUTONOMOUS_BACKLOG.md)是任务入口。
- SDK缺失为历史；CEF/ETW与S07-03 branch-protection读取权限边界独立保留，按[能力缓存](ENVIRONMENT_CAPABILITY_CACHE.json)重试。

## 保留、清理与后续

- 前次f25标准链测试通过，但运行中改文档触发打包保护退出1；[失败保留](../design/reviews/build-ui-reliability-20261022/evidence/T03-01/README.md)。先提交再冻结完整重跑已成功，未绕过保护。
- 25个本阶段测试结果文件先归档校验，再移除六个原临时目录；原失败、当前包/构建/诊断/宿主图与用户旧Round3 TRX/TestResults保留。详情见T02-08清理记录。
- 后续源码/测试/脚本/资源改动按影响运行定向检查与标准链；完整链期间不修改任何跟踪文件。纯文档按引用/源码校验，无需为其CI通过再追加记录提交。
- 继续T02-08证据矩阵与T03-03/04现有诊断/安装身份改进，随后审计剩余T00/T01条件；不要以受控或启动结果自动升级宿主验收。
- 旧事实原文见[截至2026-10-24历史](CURRENT_STATE_HISTORY_THROUGH_20261024.md)，更早记录仍可在Git和既有历史页追查。

## 持续有效的实现基线

- 产品版本0.6.73；共享入口为 `Themes/AcrylicProductionResources.xaml`、`DesignTokens.xaml`、`WpfUiProduction.xaml` 与 `Redesign.xaml`。
- Demo-first优先级保留；`GameSaveCenter.AcrylicFork/src/GameSaveCenter.Playnite/Design/` 当前工作区不存在，缺源时沿用生产共享样式，不伪造Demo对照。
- 保留业务命令、Binding、安全/取消/错误语义、虚拟化、UIA、键盘与Playnite兼容；游戏平台筛选以 `DropDownClosed` 提交。
- 行为矩阵入口 `scripts/e01-behavior-matrix.ps1`；`MANUAL QA REQUIRED` 项仍需对应人工证据，自动测试不替代。
- 开发启动继续阅读[PROJECT_MEMORY.md](PROJECT_MEMORY.md)、[WORKLOG.md](WORKLOG.md)、[DEVELOPMENT_HANDOFF.md](../DEVELOPMENT_HANDOFF.md)，按记忆协议推进。
