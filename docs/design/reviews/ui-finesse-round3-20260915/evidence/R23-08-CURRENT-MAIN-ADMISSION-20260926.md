# R23-08 当前 main 下一轮准入复核

## 2026-10-18 #1077 终态与实时环境复核

文档 HEAD `e8edbfc073d6bf18e07980a60c778577f643c6be` 对应 Actions [#1077/run `39050532641`](https://github.com/Nikilua/GameSaveCenter/actions/runs/39050532641) 已由 SDK8.0.423 和 SDK10.0.401 两 lane 终态成功。Core `125/125`、Worker `362/362`、Playnite WPF 隔离 `125/125` 通过；SDK8 Render QA、静态审查和扩展包上传成功。Artifact 身份见 `docs/ai/WORKLOG.md`。这是文档提交 CI，不改变代码基线 `8fc980f`。

实时只读快照：`\\.\DISPLAY1` 单块活动显示器（2560×1440，工作区 2560×1368），没有 Playnite/Worker 进程。此前 CEF `platform_channel 0x5` 未发生可观察变化，不重试；不绕过 ETW/WPR/xperf 权限拒绝。Round2 `208/208`、Round3 `192/192` 当前没有新的依赖已满足代码缺口；R00-01 至 R01-08 受控条件仍已满足。

本轮重复的外机 R09 文本 SHA-256 `F3DDDC6445E0BB1A5E4CD44EEEEC2C2C98834FC8C81482A67E456C5DB5864514` 与既有附件相同，目录仍指向 `20261016T052146719Z-d9d99ac4`，不是新的运行。外机 `1/2` 失败有效；其 line151 `Thickness(2/1)` 与当前 main line151 Brush 断言不匹配，具体外机 checkout/test DLL/PDB/plugin DLL 身份仍缺。#1077 CI 结果不覆盖该失败。

当前没有可据以实施的 Q/R 产品代码小批。下一准入条件为该次身份材料、新的可复现问题/明确范围或宿主/设备/权限前置发生可观察变化。外机材料到手先比对，不要求重跑。真实 Playnite/CEF、OS UIA/IME/读屏、物理 DPI/跨屏、最终呈现和 ETW/宿主性能仍未验。

## 2026-10-18 当前准入复核补记

文档 HEAD `32a62d8413f49d7f02a5909321377c9cdc09da22` 对应 Actions [#1076/run `39047011333`](https://github.com/Nikilua/GameSaveCenter/actions/runs/39047011333) 已 SDK8.0.423/SDK10.0.401 双 lane 成功；详情及 artifact SHA 见 `docs/ai/WORKLOG.md`。这是文档提交 CI，不改变产品源码基线 `8fc980f`。

重新检查任务行：Round2 的 208 项实现列均已代码完成/已复核，203 项最终未完成仍主要因宿主验收；Round3 192 项中没有待开始或实施中代码项；R00-01 至 R01-08 受控条件已满足。当前桌面只有一块活动显示器，当前用户无 Playnite/Worker 进程。此前 CEF `platform_channel 0x5` 未观察到变化，不重试；ETW/WPR/xperf 权限拒绝不绕过。外机 R09 `1/2` 仍缺失败当次 source/checkout、summary/TRX、test/plugin DLL/PDB 身份。

准入结论仍是当前无可依据实施的代码小批；总体验收未完成。恢复条件是新的可复现缺陷/明确功能范围、外机同次身份材料或宿主/屏幕/权限前置发生可观察变化。外机材料到手后先比对身份，不要求重跑。真实 Playnite/CEF、OS UIA/IME/读屏、物理 DPI/跨屏、最终呈现和 ETW/宿主性能仍待验。

日期：2026-09-26
代码基线：`4f778e9bd7e305cc878e83671f372b6b954b32e8`
文档收口：`6ac276d1`；当前复核不修改产品代码或 192 项任务状态

## 2026-10-17 当前 main 复核补记

当前文档 HEAD `6e308527e5040b94da1f07199f93adddf8978e4a` 对应 Actions [#1075/run `39043054832`](https://github.com/Nikilua/GameSaveCenter/actions/runs/39043054832) 已双 SDK 终态成功（SDK8.0.423 与 SDK10.0.401）；这只是文档提交 runner 结果，不改变产品代码身份。当前代码基线仍是 `8fc980fce5262411fbb16f0299583c34074f7f61`。R00-01—R01-08 当前账本行均为已满足，R00-08 的真实宿主/OS 输入保持未验；Round2/3 复扫未发现新增且依赖满足的代码缺口。

只读检查当前桌面拓扑得到一块活动显示器 `\\.\DISPLAY1`（2560×1440，primary），当前用户没有 Playnite 或 GameSaveCenter Worker 进程。故 Q24-03 的第二物理屏条件不具备；R23-04 不重试既有 CEF `platform_channel` `0x5` 同态；R23-05 不绕过 ETW/WPR/xperf 权限拒绝。未启动宿主、未访问真实用户数据。外机 R09 `1/2` 仍缺失败当次 checkout、summary/TRX 与 test/plugin DLL/PDB 身份；#1075 CI 通过不覆盖该机器失败。

R23-08 当前准入结论不变：无可据以实施产品改动的待开始/实施中 Q/R 代码项。下一可执行条件是出现新的可复现缺陷/明确功能范围，或上述宿主、屏幕、跟踪权限前置条件发生可观测变化；外机身份材料到手后先比对，不要求重跑。

## 结论

按当前 R23-08 清单和 main 证据，当前没有可在本机直接领取的新增产品代码项。R 任务表仍是 `192/192` 个唯一 ID：`106` 项已满足/当前 main 受控复核、`83` 项已满足但待明确环境门禁、`1` 项外部阻塞（R02-06）、`1` 项部分满足且待宿主性能（R23-05）、`1` 项不适用（R05-05）；没有待开始、实施中或代码完成待验收的 R 行。本复核不改这些计数。

### 2026-09-26 后续 UI gate 复核补记

上述结论是 R 行准入快照，不代表 RenderHarness 能发现的问题均不存在。随后以当前 main 起始身份 `0e340c08` 执行的 UI gate 复核发现并修复了 Save 历史窄窗可读行数问题：1040×700 DIP 下 4 行完整可读。此为已有 UI gate 证据的实现跟进，不新增 R ID，也不改变 `192 / 106/83/1/1/1` 统计。

该完整 Render QA gate 仍有 33 个 PROBLEM：Overview 最近访问列表仅 2 DIP、Task 某些尺寸可读行数不足且 compact Inspector 越界、Settings 矮窗主体/类别 viewport 不足。后续应把这三组作为新的逐页 UI 实现阶段；项目级 Render QA 当前仍未通过。详见 [Save compact 与完整 gate 复核](SAVE-HISTORY-COMPACT-VIEWPORT-RECHECK-20260926.md)。

### Overview 阶段完成补记

在上述 gate 快照后，已在 `OverviewRecentAccessList` 上补 `MinHeight=236`，仅修复真实空集合下 ListBox 内容自动高度测量造成的 2 DIP 视口；保留 280 DIP cap、ItemsSource、空态提示、内部滚动和虚拟化。完整 RenderHarness 的 Overview 11 个常用尺寸现在均为 236 DIP，Overview PROBLEM `0`；项目总问题减至 `13` 个，剩余仅 Task 与 Settings。证据：[Overview 空列表 viewport](OVERVIEW-EMPTY-RECENT-ACCESS-VIEWPORT-20260926.md)。全局 gate 仍未通过。

- R23-06 当前身份的包/合成安装/文件级回退已在 [当前 main 证据](R23-06-CURRENT-MAIN-ROLLBACK-20260926.md) 收口，不应重复。
- R22-01 残余时间入口已逐项核对：没有生产 `Views/*.xaml` 绑定旧本地时间投影的证据；若没有新增具体入口，不应继续改时间展示。
- 本机 `System.Windows.Forms.Screen.AllScreens` 只枚举一个活动桌面显示器 `\\.\DISPLAY21`（Bounds `2352×1470`，WorkArea `2352×1446`），因此 Q24-03 真实跨屏条件仍不具备。本轮没有把逻辑 DPI 模拟当成第二块物理屏幕。
- 当前没有 Playnite 或 GameSaveCenter Worker 进程。R23-04 最近记录的 CEF `platform_channel` 拒绝访问 `0x5` 没有新的系统状态/日志证据；遵守“不在相同状态重试”，本轮没有启动宿主。
- R23-05 的真实 ETW/WPR/xperf 采集仍依据既有系统权限拒绝证据保持阻塞；没有尝试绕过权限。

## 后续准入条件

| 条目 | 可继续条件 | 当前处理 |
| --- | --- | --- |
| R23-04 Playnite UIA/Controlled host | CEF/正常隔离宿主启动条件发生可观察变化，并且有安全隔离 profile | 保留阻塞；不要重复相同启动或绕过权限 |
| Q24-03 物理跨屏 | Windows 枚举到第二个活动显示器，并可在隔离宿主中移动窗口/Popup | 当前单显示器，不执行跨屏模拟冒充实测 |
| R23-05 presented frame/宿主性能 | 获得合规 ETW/系统跟踪权限及可复现场景 | 保留阻塞；不推导真实掉帧/呈现结论 |
| 新代码项 | 用户报告新的可复现缺陷/明确功能范围，或任务账本出现有证据的实现缺口 | 当前 R23-08 无可据以修改产品行为的开放项，不自行发明功能或视觉改动 |

本复核使用只读任务证据与当前桌面拓扑，不访问真实存档、媒体、云端或用户扩展目录，也没有启动 Playnite。后续环境条件或用户新需求到来后，再按启动协议选择下一独立阶段。

### 2026-10-17 当前 main Render QA 复核补记

在源码未变的代码基线 `5c81a07056d64482767d546efe043012abc1e6fc` 上，以文档提交 `01efb189488e6d6323d9386f7ff68057c0030cbe` 执行当前 RenderHarness：仓库隔离 SDK `8.0.423`，Release build 与 Harness 均成功，完整报告 `render-qa OK` 且 `0 PROBLEM`。这一轮实测没有复现本准入记录中遗留的 Task/Settings 短窗布局问题，不能把 2026-09-26 的 13 项描述继续当作当前缺陷；此前 2026-09-30 全量报告已为 clean，本次是最新 main 的重新确认。

SettingsLayout probe 覆盖 760/880/920/1100/1400 DIP 宽度与 560/700/900 DIP 高度，共 15 个尺寸案例、45 行头部/分类/body 几何读数；5 个分类均可测，末分类可滚动到达，宽度不超过 920 DIP 时最低主体 viewport `181.33 DIP`，高于 Harness `160 DIP` 门槛。完整 Render QA 另覆盖 Light/Dark。Task Center 的 1040×700 DIP shell geometry 为 `4/4` 可读行、`200 DIP` 表格 viewport、Inspector 可见。R00/R01 freshness 为 `14 fresh / 0 stale / 0 matched source paths / 2,919 compared paths`，package identity 未提供。详细摘要见 [R00/R01 current-main Render QA 证据](R00-R01-CURRENT-MAIN-RENDERQA-RECHECK-20261017/README.md)。

这仅是本机离屏逻辑 DIP 和合成数据验证，未运行 Playnite、物理 DPI/跨屏或最终呈现帧。2026-10-15 静态 audit 的 `13 MEDIUM TOOLBAR_VERTICAL_EXPANSION` 仍单独保留为布局风险；本次 Render QA 不会自动清除这些警告。当前没有由这次复核支持的新增产品代码改动；R23-08 的后续代码准入仍要求新发现可复现问题或明确用户需求。

### 2026-10-17 当前 main 准入与外部边界复查

- Worker 取消回调测试夹具修正后的提交 `4a98148caf2b0909998824acc73119272fc0cb37` 对应 #1056/run `38962119739` 已由 SDK8.0.423 与 SDK10.0.401 两 lane 完整验证成功；本阶段未改生产 UI 或服务。此 runner 结果不能覆盖用户另一台电脑的 R09 实际失败。
- 重新扫描 Round2 208 项与 Round3 192 项：任务表中没有新增未实现代码项；Q24-03 仍要求第二物理屏，R23-04 仍依赖可用的隔离 Playnite/CEF 状态，R23-05/Q25-02/Q25-03 仍依赖合规系统呈现跟踪证据，其它当前差异主要是宿主/OS 输入/读屏/真实呈现验收。没有据此创建新的产品改动。
- 本机只读环境快照：当前没有对当前用户可见的 Playnite 或 GameSaveCenter Worker 进程；`Screen.AllScreens` 仅有 `\\.\DISPLAY1` 一块活动屏，2560×1440，工作区 2560×1368。故 Q24-03 本轮不具备真跨屏条件；没有启动宿主或模拟第二屏。
- 用户最新粘贴的 R09 文本 SHA-256 `F3DDDC6445E0BB1A5E4CD44EEEEC2C2C98834FC8C81482A67E456C5DB5864514` 与已有附件相同；日志仍是 R09 `1/2`、总体 exit `1`，诊断目录不在当前工作区。缺同次 `summary.json`、TRX、test DLL/PDB 与生产插件 DLL 身份，根因仍未定位；不要求用户重跑，也不凭当前本机/CI 通过覆盖该失败。

准入结论：当前没有可安全独立实施的新 Q/R 代码项。下一可执行动作是在出现新的故障身份 artifact、可观察到环境前置变化或任务表产生具体缺口时，再依启动协议选一项；外机诊断 artifact 若之后可用，先比对该次 sourceCommit/dirty paths 与 test DLL/PDB/plugin DLL SHA。未验边界仍包括真实 Playnite UI、物理跨屏、OS UIA/IME/读屏、最终呈现帧、ETW/宿主性能。
