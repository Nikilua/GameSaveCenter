# GameSaveCenter 长期项目记忆

## 2026-10-18 Round4 S01-01 代码完成待验收

新增 `ViewConfigurationDefinition` v1，持久化筛选预设 ID、工作区键、排序、可见稳定列键、列宽及密度；只包含整数/字符串/字符串列表/列宽映射，不持有 DTO 或文件内容。旧设置缺失 `ViewConfigurations` 时使用空列表，未知 JSON 成员由 Json.NET 忽略，未知 schema 条目不应用。`GameSaveCenterSettings.CopyFrom`/JSON 编辑快照与“全部重置”已接入。提交 `25bbe005`。Source validation 和 diff check 通过；新 C# 测试尚未运行，因为机器缺精确 SDK8.0.423。未改 XAML、未验证 Playnite host；见 [S01-01 证据](../design/reviews/ui-finesse-round4-20261018/evidence/S01-01/README.md)。

## 2026-10-18 Round4 S07-03 外部阻塞

`windows-build.yml` 当前不按路径筛选；每次 push/pr 都跑双 SDK 全流程。GitHub `main` branch protection read 返回 403，rulesets 列表为空但不能推断 classic branch protection；浏览器设置页未显示，`gh` CLI 不可用。required check 名称尚未确认，故没有改 workflow。取得 branch protection 只读权限或用户提供 required-check 配置后，才能验证文档、混合与未知路径是否正确保留必需检查。记录见 [S07-03](../design/reviews/ui-finesse-round4-20261018/evidence/S07-03/README.md) 与[环境能力缓存](ENVIRONMENT_CAPABILITY_CACHE.json)。

## 2026-10-18 Round4 S07-08 已验收

Round4 的[交付与退出协议](../design/reviews/ui-finesse-round4-20261018/DELIVERY_AND_EXIT.md)要求每个小批集中同步实现、证据、未验边界和下一项；总计划只有 64 个任务均达到终态且必需的宿主/设备/外部证据齐备时才能结束。完整计划已获用户授权，`PROPOSED` 表示依赖/能力审查未完成，不是要求重复授权。无当前可执行项时记录阻塞和解锁条件，不重试拒绝探测或自行扩展计划外需求。此规则不改变 S00-02 门禁，Round4 当前仍进行中。证据见 [S07-08](../design/reviews/ui-finesse-round4-20261018/evidence/S07-08/README.md)。

## 2026-10-18 Round4 S07-07 已验收

`docs/ai/ENVIRONMENT_CAPABILITY_CACHE.json` 记录 SDK 8.0.423 缺失、Playnite 真实宿主未检查及历史 CEF/ETW 权限阻塞，保留已知检查日期、证据和允许重试条件；未知日期保持空值。`scripts/validate-environment-capability-cache.py` 校验状态、唯一 ID、日期、证据路径与重试条件，并已接入 `validate-source.py`。没有重复启动被 SDK 门禁挡下的宿主或重试被拒权限探测。S00-02 门禁仍待补齐 SDK 后按指定流程验证。证据见[Round4 S07-07](../design/reviews/ui-finesse-round4-20261018/evidence/S07-07/README.md)。

## 2026-10-18 Round4 S07-05 已验收

沿用 `build-diagnostics.ps1` 的 collector，不增加第二套 DLL/TRX 采集器。新的 `summary.json.environment` 记录 OS version/bitness、PowerShell version/edition、process bitness 和 .NET runtime；不记录用户名或用户目录。`scripts/preview-build-diagnostics.ps1` 在本地只读显示 checkout、环境、SDK、steps、DLL/PDB、summary/TRX 计数与缺失解释。旧格式可从本地 raw TRX 重算测试总数；环境未记录时不会推断历史环境。此入口不检查安装包或运行中 Playnite。证据见[Round4 S07-05](../design/reviews/ui-finesse-round4-20261018/evidence/S07-05/README.md)。

## 2026-10-18 Round4 S07-02 已验收

Round4 的机器数据源是 `docs/design/reviews/ui-finesse-round4-20261018/ROUND4_STATUS.json`；它把整体状态、实现/commit、受控验证、宿主验证、证据路径和未验边界分列。`scripts/render-round4-progress.py --write` 更新账本生成块，`--check` 验证计划 ID/标题、唯一性、状态值和证据链接；`scripts/validate-source.py` 已包含该检查。手写说明留在生成区域外。此索引只包含 Round4 的 64 项，不自动改动已有 Q/R 项或外部阻塞。证据见[Round4 S07-02](../design/reviews/ui-finesse-round4-20261018/evidence/S07-02/README.md)。

## 2026-10-18 Round4 S07-06 已验收

诊断 `summary.json` 现从 TRX `<UnitTestResult>` 原始条目统计用例数，分类记录 passed/failed/error/timeout/aborted/skipped 等结果，并逐文件比对 `ResultSummary/Counters/@total`。报告分组为 Core、Worker、Playnite source 与 Playnite WPF isolated，另记录实际类数与 WPF 隔离类 TRX 运行数；结果文件相对路径允许重新计数。`Test-BuildDiagnostics.ps1` 的合成结果与旧 SDK8 整套 TRX 快照验证通过，旧快照中 Q14 的 1 个失败仍保留为失败。证据见[Round4 S07-06](../design/reviews/ui-finesse-round4-20261018/evidence/S07-06/README.md)。

## 2026-10-18 Round4 S07-04 已验收

CI 结果应保留其实际 source commit、SDK、job 与 artifact 身份。只有新失败/修复、发布或影响验收状态时新增结论；不要为纯文档/CI 结果记录触发出的通过结果再追加一笔记录提交。该约束已提升到根 `AGENTS.md`。现有 Windows workflow 对 `main` 的每次 push 都运行完整任务，本阶段没有改触发器或 required checks；S07-03 需单独审查其兼容性。证据见[Round4 S07-04](../design/reviews/ui-finesse-round4-20261018/evidence/S07-04/README.md)。

## 2026-10-18 Round4 S07-01 已验收

`CURRENT_STATE.md` 已从 254 行精简到 32 行，保留当前代码身份、活动事项、不可绕过的门禁边界及验证入口；压缩前的完整正文存入 `CURRENT_STATE_HISTORY_THROUGH_20261018.md`，并已与 `4ea5e694` 的父提交版本核对一致。source validation、diff check 通过。该文档项不改变 S00-02 的代码完成待 UI 门禁状态，也不构成 Playnite 验证。证据见[Round4 S07-01](../design/reviews/ui-finesse-round4-20261018/evidence/S07-01/README.md)。

## 2026-10-18 Round4 S00-01 已验收，S00-02 代码完成待验收

`dd38f8caec8cc6c63080b2d7e1eb587e14e9cde8` 新增只读命令目录，稳定登记工作区导航、插件设置、游戏选择、当前页搜索与任务关联游戏。每项有稳定 ID、中文名、英文别名、分类/作用域和动态不可用原因，执行时复用真实 Shell/ViewModel 命令并重新校验 CanExecute；当前游戏或任务缺失时返回原因并阻断执行。现有快捷键帮助只有当前页 Ctrl+F，不等价于统一目录。

SDK9.0.302 / VSTest17.14.1 定向 Release build 成功；新目录与既有 R10 task-navigation 行为共 `7/7`，源码校验与 diff check 通过。SDK8.0.423 未安装；没有改 XAML、运行 Playnite 或声称宿主视觉通过。TRX / test DLL 哈希和具体边界见[证据页](../design/reviews/ui-finesse-round4-20261018/evidence/S00-01/README.md)。

S00-02 在本地提交 `29dc09b2` 与 `a58dd582` 实现命令搜索 Shell overlay：中文/英文名称、别名、分类过滤，空输入显示最多八项，结果显示分类、作用范围、可用状态和不可用原因，启用列表虚拟化。按最终提交 Release 构建成功，21/21 定向用例通过；200 项搜索耗时 0.554 ms。`GameSaveCenter-Run.cmd` 因机器只装 SDK9.0.302 而仓库要求精确 SDK8.0.423，在安装与版本核验前退出。UI 门禁因此未完成，S00-02 保持代码完成待验收，不推送也不准入依赖 S00-03。证据见[Round4 S00-02](../design/reviews/ui-finesse-round4-20261018/evidence/S00-02/README.md)。

## 2026-10-18 新明确范围：第四轮 64 项增量计划

用户要求继续补充有价值的功能/UI，并直接落地本地文档。本轮已核对 `4d55a7dd` 的源码与第三轮主账本：192 个主条目无精确“待开始/实施中”，但宿主/IME/跨屏/呈现等边界仍未全验。本轮只做规划，不新增产品代码，不重新声称 CI 或宿主通过。

新增[第四轮 64 项计划](../design/UI_FINESSE_ROUND4_64_TASKS_2026-10-18.md)及[账本](../design/reviews/ui-finesse-round4-20261018/ROUND4_PROGRESS.md)：命令搜索、工作视图、通知收件箱、备份来源/恢复方案、媒体比较、容量情景、人工待办、开发入口维护。按依赖推进；S00-01 已验收，S00-02 等待 UI 门禁后再准入下一项。旧 Q/R 环境边界仍保留。


## 2026-10-18 最新：#1086 双 SDK 成功，R00-03 runner 复核继续

`main` 文档提交 `5eb0d1fd9a06acedaa2bbdb52e3b810a37eb9341` 的 Actions #1086/run `39077533150` 已整体 success：SDK8.0.423 job `117527685827`、SDK10.0.401 job `117527686008` 各自 Release build/test 成功，Core `125/125`、Worker `362/362`、Playnite WPF `125/125`；目标 ProductionShellChromeSourceTests 在 `17/125`，两 lane 跑完余下全部类。SDK8 Render QA、静态审查、扩展包通过。日志与 artifacts 在 CURRENT_STATE / WORKLOG。

旧 #919/#920 SDK10.0.401 失败根因是活动侧栏过渡夹具只控制应用动效开关，未控制合法关闭动画的 Windows 系统偏好。`48b78b3b` 为夹具增加实例级系统输入、策略矩阵与系统默认值测试；#1056、#1084 至 #1086 双 SDK 全量 WPF 都成功。R00-03 受控要求可记已满足；历史失败保留。Runner success 不证明外机 source/DLL/Playnite runtime。

外机 R09 文本 SHA `F3DDDC6445E0BB1A5E4CD44EEEEC2C2C98834FC8C81482A67E456C5DB5864514` 仍与旧输入一致，真实 `1/2` 失败未解决；第151行 Thickness 断言与当前源的 Brush 断言不一致，没有同次 summary/TRX/test/plugin DLL/PDB identity，具体根因未知。用户表示暂时无诊断包；按其要求不等该材料，继续主线。

R00/R01 freshness 继续绑定 `be314f59`（14 fresh、0 stale、0 source matches）。Round2/3 / R23-08 重筛无新的依赖已满足且有明确实现缺口的 Q/R 项；总目标仍 active。外机 R09 同一失败日志哈希仍未得到同次身份包，用户已明确继续主线、不阻塞任务。下一步在新外机身份材料、新复现/明确范围或宿主/设备/权限前置变化时重新准入。用户未跟踪的 TRX/TestResults 未动，未产生临时文件。

## 2026-10-18 历史：#1084 双 SDK 通过，R00-03 历史失败已归因

当前 `main` HEAD `4cde34ce465a99787ba3ef969a57bacf9e606550` 的 Actions #1084/run `39070369704` 已终态成功：SDK8.0.423 job `117506536858`、SDK10.0.401 job `117506537093` 均 Release build/test 成功，Core 125/125、Worker 362/362、Playnite WPF 125/125；ProductionShellChromeSourceTests 在 17/125，两 lane 全部完成。该结果验证 runner 上精确 main，不代表另一台机器加载了相同源码/程序集或 Playnite 呈现。

#919 与 #920 的 SDK10.0.401 完整日志都显示旧动画正例夹具只固定应用开关，没有固定 Windows 系统动效偏好，因而系统合法关闭动画时“活动时钟”断言失败。`48b78b3b` 为测试加入实例级系统输入和组合策略矩阵；#1056 和当前 main #1084 两 SDK 全量 WPF 通过，R00-03 受控条件可维持已满足。日志和证据路径见 CURRENT_STATE 与 R00-03 evidence README。

用户本轮附加的外机文本 SHA-256 `F3DDDC6445E0BB1A5E4CD44EEEEC2C2C98834FC8C81482A67E456C5DB5864514` 已确认与旧文本相同，诊断 ID 仍为 `20261016T052146719Z-d9d99ac4`、R09 `1/2`。第151行 Thickness 2/1 与当前源第151行 Brush 断言不一致；无同次 summary/TRX/test/plugin DLL/PDB identity，具体根因仍未知。始终分开陈述本机、GitHub runner、外机结果，不以任何一方覆盖另一方。

R00/R01 对产品代码 `be314f59` 的 freshness 为 `14 fresh / 0 stale / 0 matched source paths`。Round2/3 与 R23-08 重扫没有新的依赖满足且有具体实现缺口的 Q/R 项；总目标仍 active。新输入到达时先核对身份；否则等新复现、明确范围或宿主/设备/权限前置有可观察变化，再重新准入。既有用户 TRX/TestResults 未动，没有新 artifacts/.tmp。

## 2026-10-18 历史检查点：#1083 双 SDK 成功，外机附件未更新

本轮更新前的 `main` 文档 HEAD `0bde982d49700465498bd0732651e21c10cebe43` 对应 Actions #1083/run `39066560072` 已终态成功。SDK8.0.423 job `117495353783` 与 SDK10.0.401 job `117495353928` 均通过；两边 Release build/test、Core `125/125`、Worker `362/362`、Playnite WPF `125/125` 通过。SDK8 Render QA、30-XAML 静态审查与扩展包上传成功。代码基线仍是 `be314f59c6a3a0b93d890cf39ee4c6c26ff2c523`。这些结果验证精确提交在对应 runner，不代表用户另一台机器成功。

用户本轮粘贴日志 SHA-256 `F3DDDC6445E0BB1A5E4CD44EEEEC2C2C98834FC8C81482A67E456C5DB5864514` 与此前文件逐字节相同，诊断目录仍为 `20261016T052146719Z-d9d99ac4`，不是新运行。其 R09 第 151 行厚度断言与当前测试源码第 151 行 Brush 断言不同，表明日志的行号/断言映射不一致；旧/混合源码、符号、程序集仍只是线索。没有同次 summary/TRX/test DLL/PDB/plugin DLL identity，不能定位根因，也不能称外机已解决。用户指出本机构建不能代表外机，这一点成立；本机 SDK9 和 CI runner 均只对各自输入作结论。

R00/R01 freshness 继续绑定 `be314f59`，为 `14 fresh / 0 stale / 0 matched source paths`，测试夹具改动未命中登记源码路径。质量审查、Round3 192 行及 R23-08 最新复核目前没有新的依赖已满足且有具体实现缺口的 Q/R 代码项。继续条件：外机同次身份材料、新的可复现问题/明确功能范围，或宿主、设备、权限前置的可观察变化。真实 Playnite/CEF、OS UIA/IME/读屏、物理 DPI/跨屏/呈现、ETW/宿主性能仍未验；整体目标未完成。

## 2026-10-18 截至 #1082：R08 夹具双 SDK 收口与 R09 外机版本差异

代码提交 be314f59c6a3a0b93d890cf39ee4c6c26ff2c523 的 #1081/run 39063179675 已双 SDK 成功；后续文档提交 b9eb6971 的 #1082/run 39063783824 也双 SDK 成功。两 run 的 SDK8.0.423、SDK10.0.401 都 Release 0/0、Core 125/125、Worker 362/362、Playnite WPF 125/125，含 R08MotionHotChangeBehaviorTests 与 UiFinesseFoundationTests。SDK8 Render QA OK、WPF 静态审查 30 XAML / 0 errors / 27 warnings / 177 info、扩展包生成上传成功；artifact IDs/digests 见 R08 evidence README。

#1080/run 39061487759 整体 failure：SDK10 R08 active-clock 正例失败，SDK8 全部通过；已用完整日志确认。be314f59 将 app-motion 夹具改用显式时长后，#1081 SDK8/SDK10 全量测试均通过。SDK9.0.302 / VSTest17.14.1 本机定向行为类 1/1 是补充，不替代 CI。

外机 R09 Expected Chrome thickness 2 / Actual 1 与当前源码不一致。历史测试 3f7d0b30 断言 Chrome=2 DIP；f13b9e05 起模板/测试拆成 Chrome=1、ValidationStroke=2。当前 main 的 #1079/#1081/#1082 干净 CI 都通过 WPF 125 类，但这不能证明外机加载程序集正确。缺同次 checkout、TRX、test/plugin DLL/PDB 身份，旧或混合产物只是待证线索，不能称外机已解决。

Round2/3 与 R23-08 最近复扫无新的依赖已满足且有明确代码缺口的 Q/R 项。若外机身份材料到达，先对照，不要求用户重跑；若没有新身份材料、新复现或宿主/设备/权限前置变化，当前无独立可实施的 Q/R 代码项。真实 Playnite、OS UIA/IME/读屏、物理 DPI/跨屏/呈现、ETW/宿主性能仍未验。总目标未完成。

## 2026-10-18 最新：#1079 R08 双 SDK 收口与 R00/R01 freshness

代码提交 `de8bdbb176bb7ae85c4ca3f488f0aeb37958bfec` 对应 Actions #1079/run `39057273073` 已整体终态成功：SDK8.0.423 job `117467706465` 与 SDK10.0.401 job `117467706337` 均 Release build `0/0`、Core `125/125`、Worker `362/362`、WPF 隔离 `125/125`，包含 `UiFinesseFoundationTests` 与 R08 system-motion 行为类。SDK8 另 Render QA、XAML 静态审查及扩展包生成/上传成功。完整 artifact IDs/SHA 见 Round3 的 `evidence/CI-R08-SYSTEM-MOTION-20261018/README.md`。

R00/R01 初扫命中 R00-03、R01-04 的 `GscMotion.cs`；已由该精确 SHA 的两 lane 全量 WPF 运行复核并更新证据基线，freshness `14/14`，package identity `not-provided`。本机仓库 global.json 要求 SDK8.0.423 而本机只有 SDK9.0.302，本轮本机 dotnet 命令未启动；SDK9 不是 CI 替代。外机 R09 `1/2` 仍是真实独立失败，当前同次 checkout/TRX/test/plugin DLL/PDB 身份缺失。下一步是在新复现、明确范围或宿主/设备/权限前置变化时重新扫描任务准入；取得外机身份材料时先比对，不要求重跑。总目标未完成。

## 2026-10-18 R08 system-motion 测试时长隔离

Actions #1078 的 SDK8.0.423 在 `R08SystemMotionHotChangeBehaviorTests` 启动动画的前置断言失败（Opacity=1、无动画时钟），SDK10.0.401 全量成功，本机 SDK9.0.302 定向 `1/1` 成功。不要从这些结果推断已证实的操作系统根因；针对该行为测试，用 `GscMotion.AnimateEntrance` 的显式时长重载控制要被取消的时钟，保证继续经过生产的注册、取消和归一化路径。默认双参数入口仍从当前控件资源读取时长。新提交必须追踪 SDK8 与 SDK10.0.401 Actions 到终态；SDK9 不能替代二者。

## 2026-10-18 最新续跑：#1077 双 SDK CI 与准入复核

`main` HEAD `e8edbfc073d6bf18e07980a60c778577f643c6be` 对应 Actions #1077/run `39050532641` 已双 SDK 终态成功。SDK8.0.423 job `117448022062`、SDK10.0.401 job `117448022272` 均 Release `0/0`、Core `125/125`、Worker `362/362`、Playnite WPF 隔离 `125/125`。SDK8 Render QA、30-XAML 静态审查和扩展包通过。Artifacts：package `12108658169` / SHA-256 `c290c2dbec6af9c044d5ec42ad1ae8bd21cb0d86f5094925ae82d66e9aff8937`；SDK8 diagnostics `12108792518` / `39a2d164e7224eeac7d7f7932a64d5267bbda78c3cabf6a82cd21952a5442969`；SDK10 diagnostics `12108435363` / `d9faa653ca34c1d8c1e00f2b430450f81384df9d665e0722d9dd5cfce31c63f5`。

Round2/3 与 R23-08 复扫没有新的依赖满足代码缺口；R00/R01 受控项状态不变。本机当前单屏且无 Playnite/Worker。外机 R09 附件 SHA-256 与既有输入相同，仍是真实失败；本机或 CI 通过不代表外机成功。下一准入点是该次身份材料、新复现/明确范围或宿主/设备/权限前置发生可观察变化。总目标保持未完成。

## 2026-10-18 最新续跑：#1076 双 SDK 收口与当前准入

文档提交 `32a62d8413f49d7f02a5909321377c9cdc09da22` 对应 Actions #1076/run `39047011333` 已双 SDK 终态成功：SDK8.0.423 job `117437640771`、SDK10.0.401 job `117437640925` 均 Release `0/0`、Core `125/125`、Worker `362/362`、Playnite WPF 隔离 `125/125`；SDK8 Render QA、XAML 静态审查和扩展包生成成功。Artifacts：package `12107422070` / SHA-256 `18310d2c4a94b85f6840006cc3ac9da77e896161ec07be066f3c58b32b54e9e0`；SDK8 diagnostics `12107750956` / `70db57dfdfd00cf41ac2a46cbc65985f41ad1ea3c989e9f9ebc28fce8377bd6b`；SDK10 diagnostics `12106670885` / `01203d80bde6bbc05a367951b966106e05b51016fb3b751a725af25f0b6a2f88`。

Round2/3 最新复核没有依赖已满足的代码缺口；R00/R01 受控项已完成，剩余主要是实际宿主、OS 输入、物理显示或合规系统跟踪门槛。本机当前单屏且无 Playnite/Worker，外机 R09 仍缺同次构建身份。CI runner 成功不能覆盖外机失败；总目标保持未完成，满足新证据/环境条件后继续准入。

## 2026-10-17 最新续跑：#1075 双 SDK 终态与准入边界

文档提交 `6e308527e5040b94da1f07199f93adddf8978e4a` 对应 Actions #1075/run `39043054832` 已终态成功。SDK8.0.423 (`117426249110`) 与 SDK10.0.401 (`117426248974`) 的 Release build、Core `125/125`、Worker `362/362`、Playnite WPF 隔离 `125/125` 均通过；SDK8 Render QA、30-XAML 静态审查及扩展包生成/上传成功。Artifacts：package `12106570037` / `e0068c287926768ee8ed13b5fb1d2eee01ac33581331d74068fa0a8195e63424`；SDK8 diagnostics `12106450704` / `5c8f78a322556c9c7e1c5201fce243f0d1f44070c3692fd250b5e5dfe4371c53`；SDK10 diagnostics `12104389589` / `f606db90d3323b3d9ed997b4cddab54681c4209ef50dc0cad08c55fdbd22b3f4`。

R00-01 至 R01-08 最新账本均为“已满足”；R00-08 仍只是受控合成行为已满足，真实宿主/OS 输入待验。Round2/3 与 R23-08 复扫未发现新的依赖满足代码缺口。本机当前单显示器、无 Playnite/Worker 进程，故 Q24-03 和 R23-04/05 门槛没有变化；R09 外机 `1/2` 仍缺同次 source/test/plugin 身份。当前代码基线仍为 `8fc980f`，本轮只改文档；不把 #1075 runner 通过当作外机成功。

## 2026-10-17 最新续跑：#1074 双 SDK CI 终态与外机边界

文档提交 `68c178968e535774d7acb68c8c01a67319c20804` 对应 Actions #1074/run `39039120707` 已终态成功：SDK8.0.423 job `117414822833` 与 SDK10.0.401 job `117414822607` 均 Release build `0/0`、Core `125/125`、Worker `362/362`、Playnite/WPF 隔离 `125/125` 通过；SDK8 Render QA、30-XAML 静态审查与包生成/上传成功。Artifacts：package `12104192973` / SHA-256 `0900b6fe7ec432716e9fc6da9a23c1c194c8dec34069c0d3b5f49848bf5b5098`，SDK8 diagnostics `12104078221` / `b7ad514628db695804530d5e49a2b02e95e91ccdd10128a5c57264aef79ec8d9`，SDK10 diagnostics `12104125808` / `7d6274eac8e4e096d13a3b074a018af3cd05c0e5e9fb3cd4c5bb3034b42b5bc6`。

这是文档提交的 CI 结果，不是产品源码变更，也不重算 R00/R01 freshness；代码基线保持 `8fc980f`。CI 成功不能推出外机 R09 成功：外机 `1/2` 失败缺同次 checkout、summary/TRX 和测试/插件 DLL/PDB 身份，根因未确认。Round2/3 复扫没有新且依赖满足的 Q/R 代码缺口；等同次身份材料、新复现或前置变化后再准入。真实 Playnite/CEF、OS 输入/UIA/IME/读屏、物理屏幕与呈现帧、ETW/宿主性能仍待验。

## 2026-10-17 最新续跑：#1073 双 SDK 终态与 Q/R 准入

`main` HEAD `199b49381598aeffa362a4154dced74f202180e9` 的 Actions #1073/run `39027064950` 已终态成功；SDK8.0.423 job `117380045515` 与 SDK10.0.401 job `117380045495` 均成功。SDK8 完成 Release build/test、Render QA、WPF 静态审查、扩展包与诊断上传；SDK10 完成 Release build/test 和 diagnostics 上传。该提交仅验证文档，R00/R01 freshness 继续绑定代码基线 `8fc980f`。

Round2 `208/208` 与 Round3 `192/192` 复扫未发现新且依赖已满足的 Q/R 产品代码缺口。现在未收口的主要条件是真实 Playnite/CEF、OS 输入/UIA/IME/读屏、物理跨屏与屏幕呈现、获准 ETW/系统跟踪。用户外机 R09 `1/2` 仍是独立失败；其 `Thickness(2/1)` 第151行与当前源/PDB 映射不匹配，但缺同次 summary/TRX/test DLL/PDB/plugin DLL 身份，故根因未确认。当前本机或 CI 通过不能证明外机成功。若有同次诊断材料，先直接比较身份；没有新材料时不要求用户重跑，按 R23-08 条件等待新复现或环境变化再准入。

## 2026-10-17 最新续跑：#1072 文档检查点终态与 R09 外机映射复核

当前文档检查点的前序 `main` HEAD 为 `6c8c85b0efae4d15996a2c7382255fcbaa8d2a2d`，代码基线为 `8fc980fce5262411fbb16f0299583c34074f7f61`。代码提交 `547efd4badaa462379532c2c0c96eea5c5a5c524` 只增强 `Q06ButtonStateSequenceBehaviorTests`，没有改生产按钮/XAML/命令。本机 SDK `8.0.423`、VSTest `17.11.1` 下 Playnite `net462`/测试 `net472` 定向 Release 与双主题 STA 测试 `1/1` 通过；该结果不是完整 solution build 或真实 Playnite/物理输入。源码/测试身份、脱敏 TRX 见 `docs/design/reviews/ui-finesse-round2-20260913/evidence/Q06-02-PRESSED-RESTORE-20261017.md`。

R00/R01 freshness 对当前 HEAD 为 `14` 项、`0` 需重跑、`0` 个匹配登记源码路径，`packageIdentity=not-provided`。#1069/run `39013848650` 与精确代码提交 #1070/run `39016699991` 均双 SDK 终态成功。#1070 SDK8 job `117350168944`（SDK `8.0.423`）和 SDK10 job `117350169177`（SDK `10.0.401`）完整 Release build/test、WPF `125/125` 类通过，均含 R09/Q06-02；SDK8 Render QA `render-qa OK`。本次当前 HEAD 证据见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R00-R01-CURRENT-HEAD-RECHECK-20261017-8FC980F.md`。旧13条 MEDIUM toolbar layout risk 不是 Render QA 检查结果，仍须保留。

用户另一台机器日志的 `Expected Thickness(2) / Actual Thickness(1)` 和行151对应历史 `e12119bf` 的旧 Chrome 边框断言；当前源在 `f13b9e05` 后改为 Chrome `1 DIP` 与独立 ValidationStroke `2 DIP`，当前行151是颜色比较。这意味着外机日志所用源或 PDB 行映射与当前 main 不一致；具体 checkout、测试 DLL/PDB、plugin DLL 尚未取得，不能把可能的旧 checkout/程序集错配写成确诊。CI 通过只证明各 runner 的精确输入，不覆盖外机 `1/2` 失败。Round2 Q06-02 仍因物理输入、取消时序和 Playnite 宿主未验而未完成。R23-08 复扫没有新的依赖满足代码缺口。

文档提交 `6c8c85b0` 对应 Actions [#1072/run `39023752118`](https://github.com/Nikilua/GameSaveCenter/actions/runs/39023752118) 已整体终态成功。SDK8.0.423 job `117370558805` 与 SDK10.0.401 job `117370558697` 均 Release build `0 warning / 0 error`、Core `125/125`、Worker `362/362`、Playnite source `107` 类及 WPF 隔离 `125/125` 类通过。SDK8 另 `render-qa OK`、静态审查 `30 XAML / 0 errors / 27 warnings / 177 info`、`0.6.73` 扩展包生成并上传；SDK10 日志确认 SDK `10.0.401` 并完成诊断上传。Artifacts：SDK8 package `12098304583`，90,314,080 bytes，SHA-256 `3ac418287e75d012c6917d80abb10cde0e0b42005725797a88112991ae0a4ffa`；SDK8 diagnostics `12099205027`，1,630,261 bytes，SHA `9f8d8503738a9990159787a1d83996b490d0e6832547345a5acfb334c7a0756d`；SDK10 diagnostics `12098735372`，774,612 bytes，SHA `36d6c6766ae8f879794c708ce19f66e459e389717ab70c1f1a7e3dd7e9d443e1`。#1072 只验证文档提交；R00/R01 freshness 仍绑定代码基线 `8fc980f`，不把 `packageIdentity=not-provided` 写成包/宿主验收。

用户外机失败同次身份仍未提供；当前 main/CI 两 SDK 通过不覆盖外机 R09 `1/2`。若无该证据、新复现缺陷或环境前置变化，下一步按 R23-08 重新准入，不制造产品改动。真实 Playnite/package-host、物理 DPI/跨屏、OS UIA/IME、最终呈现帧、ETW/宿主性能仍未验。

## 2026-10-17 最新续跑：#1068 CI 与 R00/R01 freshness

文档提交 `b14e780daa11a074d63abc381d777c66a8d258a8` 对应 Actions #1068/run `39010428185` 已整体终态成功。SDK `8.0.423` job `117331623876` 和 SDK `10.0.401` job `117331623679` 均 Release `0 warning / 0 error`、Core `125/125`、Worker `362/362`、Playnite source `107` 类及 WPF 隔离 `125` 类通过（包含 R09）。SDK8 Render QA `0 PROBLEM`、WPF 静态审查 `30 XAML / 0 errors / 27 warnings / 177 info`、扩展包生成/上传成功。Artifacts：SDK10 diagnostics `12092898627`，SDK8 diagnostics `12093722541`，package `12093881767`；大小与 SHA-256 见 `docs/ai/CURRENT_STATE.md`。

R00/R01 freshness 对 `b14e780d` 为 `14` 项、`0` 需重跑、`0` 个匹配源码路径、`2,963` 比较路径，package identity `not-provided`。Round2/3 canonical ledger `192/192` 唯一；仍未收口行均要求宿主/系统输入/外机身份/获准系统跟踪，当前没有新的依赖已满足 Q/R 产品代码项。用户外机 R09 `1/2` 仍真实且未定位；缺同次 summary/TRX/test DLL/PDB/plugin DLL 身份。本机与 runner 通过不能覆盖外机失败。下一准入点是同次身份 artifact、新的复现缺陷或前置环境变化；不可等待该文件而阻塞独立工作。

## 2026-10-17 最新续跑：#1067 与用户外机 R09

提交 `26de12257748075e024c61972da16e87fd4ddfce` 对应 Actions #1067/run `39006921277` 已整体终态成功。SDK8.0.423 job `117321161387` 和 SDK10.0.401 job `117321161211` 均 Release `0/0`、Core `125/125`、Worker `362/362`、Playnite WPF 隔离 `125` 类通过；SDK8 Render QA、静态审查、扩展包和诊断上传成功。Artifacts：SDK8 diagnostics `12092696132`、SDK10 diagnostics `12091697203`、package `12092870374`。这证明精确提交在 GitHub runner 上通过，不等于本机或用户外机通过。

用户再次粘贴的外机 R09 文本 SHA-256 `F3DDDC6445E0BB1A5E4CD44EEEEC2C2C98834FC8C81482A67E456C5DB5864514` 与此前相同；其真实结果仍为 R09 `1/2`、整体 exit `1`。当前 main 源 line151 与该日志的旧厚度断言不匹配，但外机本次 `summary.json`、test DLL/PDB 和 plugin DLL identity 仍缺，具体输入错配未证实。不得以本机或 CI 通过覆盖该失败；不改焦点样式或放宽断言。

R00/R01 freshness 对 `26de1225` 为 `14 fresh / 0 stale / 0 matched source paths`、2,963 比较路径，package identity `not-provided`。Round3/R23-08 最新准入没有新的依赖已满足产品代码项；下一准入点是外机同次身份材料、新可复现问题或环境前置变化。Playnite/package-host、物理 DPI/跨屏、OS UIA/IME、最终呈现帧、ETW/宿主性能仍未验。

## 2026-10-17 R09 外机身份诊断补强

在 `scripts/build-diagnostics.ps1` 中，为每个已识别 DLL 增加同目录 PDB 的可选 identity 记录（文件名、脱敏路径、SHA-256、大小；缺失时为 `null`，被锁/不可读时记录错误类型而不阻断摘要），使失败栈行号可与实际符号文件做同次比较。`Test-BuildDiagnostics.ps1` 用合成 DLL/PDB 验证可读、锁定、缺失三态、JSON 序列化和路径脱敏；本机诊断自测、`scripts/validate-source.py`、`git diff --check` 通过。首版 `8a895807` 的 Actions #1066/run `39003320618` 已双 SDK 终态成功；后续 `26de1225` 的锁定/不可读保护已由 #1067/run `39006921277` 双 SDK 终态验证。该工作只补诊断能力，不诊断或覆盖用户外机真实 R09 失败；仍需其同次 summary/TRX/DLL/PDB/plugin 身份。CI runner 成功不证明外机成功。Round3 准入不因此新增或完成产品任务。

## 2026-10-17 最新续跑补记：#1064 与 R09 外机证据范围

文档检查点 5475ba7d9744ab107b159a89ef930d535f1f95cb 的 Actions #1064/run 38996324038 已双 SDK 整体成功。SDK10.0.401 job 117289578704 与 SDK8.0.423 job 117289578557 在 Windows Server 2025 干净检出上均 Release 0/0、Core 125/125、Worker 362/362、WPF 隔离 125 类（含 R09）；SDK8 Render QA 0 PROBLEM、静态审查 30 XAML / 0 errors / 27 warnings / 177 info、扩展包与诊断上传成功。Artifact IDs：SDK10 diagnostics 12088185573、SDK8 diagnostics 12088616683、package 12088477370。CI 是独立 runner 证据，仍不能覆盖用户外机的实际失败。

用户给的 GameSaveCenter-build-diagnostics (4).zip 与 GameSaveCenter-sdk10-build-diagnostics (1).zip 内部记录为 2026-10-13、sourceCommit c9f9179d9b8d38e506dbb1402ae7030eafebea6c；SDK8 ZIP 中 R09 2/2，SDK10 ZIP 中 Worker 取消测试 356/357 失败且没有 R09 子运行。这些文件不含 2026-10-14 外机失败目录 20261014T052440239Z-3fc9f5f1。外机 Thickness(2/1) line 151 匹配历史 3f7d0b30 测试断言，当前 main 已将 Chrome 1 DIP 与 ValidationStroke 2 DIP 分开；当前一键流程每次新建隔离输出目录，根因仍需该次 summary/TRX 与 test/plugin DLL/PDB 身份，不能按旧 bin 猜修。

当前仍没有新的依赖已满足 Q/R 产品代码项；下一准入点为外机同次身份 artifact、新的可复现问题或前置环境变化。未验真实 Playnite、物理 DPI/跨屏、最终呈现、OS UIA/IME、ETW/宿主性能。保持本机、GitHub runner 与外机验证分账。

## 2026-10-17 当前续跑：Q11-07 双主题工作区状态往返

Q11-07 在 main 基线 `57023ac849689dab0d2b999737e0213042a1eebc` 上只扩展 `R08PageSwitchBehaviorTests`，添加 opt-in 生产缓存页 Light/Dark 离屏输出；生产 UI/XAML/服务/DTO/命令/导航未改。Playnite `net462` / Tests `net472`，本机 SDK9.0.302/VSTest17.14.1 隔离 Release 类 `4/4`。实际测试源 SHA `6B46ADE6C82FE6DF26466BBB95406F0CAEA96423BEBC9AB862031CC062237222`，测试 DLL/PDB 与插件程序集身份见 [Q11-07 复核包](../design/reviews/ui-finesse-round3-20260915/evidence/Q11-07-CURRENT-MAIN-VISUAL-RECHECK-20261017/README.md)。SDK9结果只适用于本机该源码/运行环境，不证明 CI 或用户外机。

四个生产缓存页面在往返后 Light/Dark 共8张 `RenderTargetBitmap` 离屏图，96 DPI；不是 Playnite/桌面屏幕/有效物理 DPI/呈现帧。行为断言检查筛选、Tab `2/3/5`、任务 ID `q11-07-task-55`、滚动 `18 DIP`、缓存与引用保持、刷新命令0次。选中任务不在当前截图视口，截图不声称其可见。只用合成 DTO 和 state-only fixture，未触业务查询/真实存档/Worker/云端。

Actions #1061/run `38986851956` 已在旧提交 `57023ac` 上双 lane 终态成功，但不含本轮测试改动。Q11-07 精确代码提交 `6a909cb329d88af3da83a78446050a83d24ef246` 对应 #1062/run `38988908489` 双 lane 终态成功：SDK10.0.401 job `117266648758` 与 SDK8.0.423 job `117266649011` 均 Release `0/0`、Core `125/125`、Worker `362/362`、WPF 隔离 `125` 类全通过（R09 与 R08 page-switch 类包含在内）；SDK8 另 Render QA `0 PROBLEM`、静态审查 `30 XAML / 0 errors / 27 warnings / 177 info`、包与诊断 artifacts 上传成功。#1062 artifacts：SDK10 diagnostics `12084772763`、SDK8 diagnostics `12086060974`、package `12085647890`。文档收口提交 `ee8d6aa8d32da2861167d9d49b28e1f912063a8b` 对应 #1063/run `38992929963` 也双 lane 终态成功，SDK10 job `117279033715`，SDK8 job `117279033559`。CI runner 结果不覆盖用户另一台电脑的真实 R09 `1/2` 失败；其日志目录在当前工作区不存在，已向用户请求方便时附加同次目录 ZIP，不等待文件。Q11-07 的宿主、有效物理 DPI/最终屏幕呈现仍待验；R23-08 最新准入复扫无可独立实施 Q/R 代码项，下一可执行条件为取得外机身份 artifact、新的可复现问题或环境前置变化。

## 2026-10-17 最新续跑：Q11-06 双 SDK CI 收口与 R00/R01 freshness

代码提交 `1c60a2fc61fae6109a609fd14c4977dfb927a32c` 与证据脱敏提交 `d62e199fed5adf002fbf762f3a8ea0a770da46fd` 已推送至 `main`；Q11-06 复用生产页签模板补了可选离屏捕获，测试源 SHA `94347B788C770CD4D30FD3AFA161F751DD17EA724748C35CA37F028F89F56BA9`，生产 `Redesign.xaml` 未改。SDK9.0.302/VSTest17.14.1 定向 Release test `1/1`，18 张 Light/Dark 截图与几何报告已归档。#1059/run `38982569941` 与 #1060/run `38983018226` 双 SDK 均整体成功；SDK8.0.423 和 SDK10.0.401 build/test 通过，SDK8 另 Render QA、静态审查、扩展包生成与上传成功。SDK9 本机结果与 CI 结果分开记录。

Q11-06 视觉只记受控通过；host 因真实 Playnite/OS 输入/有效物理 DPI/屏幕呈现未验而保持外部阻塞，Final 未完成。外机 R09 文本是重复附件（SHA `F3DDDC...4514`），旧 `3f7d0b30` 第151行是 Chrome 2 DIP 断言；当前已拆 Chrome 1 DIP + ValidationStroke 2 DIP。外机 HEAD、源码、DLL/PDB 与插件身份仍缺，不能用本机或 CI 通过覆盖外机失败。最新 R00/R01 freshness `14/14 fresh / 0 stale / 0 matched source paths`、2,949 changed paths、package identity `not-provided`。下一步复扫 Q/R ledger，选依赖满足且证据缺口明确的一小批。

更新：2026-10-17；当前 `main` 代码基线为 `4a98148caf2b0909998824acc73119272fc0cb37`，最近已推送文档检查点为 `01e99b6e41c05519a78dfae42a4ad97c6fcf9f1b`；Worker 取消回调测试夹具时序修正已完成，生产代码未改。Q24-04 的本机 SDK9 行为和 R00/R01 freshness/Render QA 仍见[Q24-04 复核包](../design/reviews/ui-finesse-round3-20260915/evidence/Q24-04-LOAD-MORE-KEYBOARD-RECHECK-20261017/README.md)和[R00/R01 Render QA 复核](../design/reviews/ui-finesse-round3-20260915/evidence/R00-R01-CURRENT-MAIN-RENDERQA-RECHECK-20261017/README.md)。

新读取的 #919/#920 完整 SDK10 日志证实它们在 2026-09-30 分别对 `a6b6dfc9`、`77de7f45` 运行，并在 `ProductionShellChromeSourceTests` 的两个活动动效前提处失败（每次 11/13）。测试只覆盖应用动效设置，没有固定 runner 的系统动效偏好；2026-10-07 `48b78b3b` 加入实例级系统偏好测试 seam、偏好组合矩阵与默认真实系统偏好测试，生产策略仍尊重 Windows 状态。该提交是当前 main 祖先；#1056 两 SDK 的 125 个 WPF 类全部通过。历史失败保留为失败，修正后的 CI 通过不重写历史。证据见 [R00-03 历史 SDK10 失败复核](../design/reviews/ui-finesse-round3-20260915/evidence/R00-03-SIDEBAR-MOTION-CI-FAILURE-RECHECK-20261017/README.md)。

外机 R09 的本次文本附件 SHA 与旧件一致（`F3DDDC...4514`，诊断 ID `20261016T052146719Z-d9d99ac4`），不是当前 main 新复跑。其第151行数值断言属于旧 `3f7d0b30` 测试形态；`f13b9e05` 已把 Chrome 1 DIP 与独立错误描边 2 DIP 分开。不能据此断言用户当前 checkout 或程序集身份，不能用本机/CI 通过覆盖用户失败。新失败要优先取得同次 `summary.json`、R09 console/TRX 与 test DLL/PDB/plugin DLL SHA；在此之前不改生产模板、不放宽断言。

当前取消测试修正仅改 Worker 测试：本机 SDK8.0.423 官方 Release build `0/0`、Core `125/125`、Worker `361 Passed + 1 NotExecuted`，修正用例同一隔离 DLL 重复 `50/50`；同输出 R09 定向 `2/2`。完整 WPF 隔离在 R05 Tooltip `50/116` 因 `GetCursorPos` 无法取得初始系统光标位置停止，不能称全套通过。随后 #1056/run `38962119739` 在 SDK8.0.423 和 SDK10.0.401 两 lane 均 Release `0/0`、Core125、Worker362、source107类和WPF隔离125类通过；SDK8 Render QA、静态审查、扩展包也成功。GitHub runner 两 SDK 通过是独立于本机的证据，但仍不等于用户另一台电脑的构建输入成功。证据：[Worker 取消回调复核](../design/reviews/ui-finesse-round3-20260915/evidence/CI-WORKER-CANCEL-CALLBACK-RECHECK-20261017/README.md)。

R00/R01 完整 RenderHarness 在仓库隔离 SDK `8.0.423` 上 Release build/运行成功，`render-qa OK`、`0 PROBLEM`；旧准入表的 13 个 Task/Settings 问题未复现。窄 Settings 最小 body viewport `181.33 DIP`，Task 1040×700 `4/4` 行且 Inspector 可见。2026-10-15 静态审查的 13 MEDIUM `TOOLBAR_VERTICAL_EXPANSION` 警告仍作为独立风险保留。本次没有产品代码改动；没有运行 Playnite、物理 DPI/跨屏或屏幕最终呈现。

**本机成功不等于另一台机器或远端成功。** 外机一键流程真实 exit `1` 和 R09 `1/2` 必须保留；当前源码 line 151 是颜色断言，外机栈却报 `Thickness(2/1)`，与历史 `3f7d0b30` 的旧 Chrome 厚度断言相符。只能确认失败栈的源码/符号映射不对应当前源码，不能判定是旧 checkout、DLL、PDB 或脚本输入。外机附件重复提供的是同一日志。外机与本机隔离脚本均报116个WPF类，而 #1055 同代码提交 SDK8 CI报125个；发现计数依赖执行输入，不能单独定位错配。需失败同次 `summary.json`、TRX、HEAD/dirty paths 与 test DLL/PDB/plugin DLL identity；身份材料暂缺不阻塞其它任务。

Actions #1051 的 SDK10.0.401 Worker 取消回调测试失败为 `361/362`、callback count expected `1` actual `0`；SDK8 job 已完整成功。Q24-04 commit `5c81a070` 的 #1052/run `38932328340` 已整体成功。SDK10.0.401 lane build `0 warning / 0 error`，Core `125/125`、Worker `362/362`、125 个 WPF 类隔离运行成功（含 R21）；#1051 失败测试本次通过，未复现。SDK8.0.423 lane build `0 warning / 0 error`，Core/Worker 与 125 个 WPF 类通过，Render QA、WPF 静态审查、扩展包和上传完成。#1051 的失败仍是历史真实 CI 失败，本次未复现不等于修复。#1053/run `38937972835` 对文档提交 `01efb189` 已整体终态成功：SDK8.0.423 job `117098144434`、SDK10.0.401 job `117098144743` 均 Release `0/0`、Core125、Worker362、WPF隔离125类通过（含 R09/R21）；SDK8 Render QA `0 PROBLEM`、WPF 静态检查 `0 errors / 27 warnings / 177 info`、`0.6.73` 包与诊断 artifact 上传成功；SDK10 完整日志确认实际版本 `10.0.401`。这只验证当前 CI 构建输入，不覆盖外机 R09 的真实 `1/2` 失败。

Actions #1054/run `38944728502` 已在当前文档 HEAD `79748216` 终态成功：SDK8.0.423 job `117120854146` 与 SDK10.0.401 job `117120854512` 均 Release build `0 warning / 0 error`、Core `125/125`、Worker `362/362`、125 个 Playnite WPF 隔离类通过，含 R09；SDK8 另 Render QA `render-qa OK`、静态审查 `0 errors / 27 warnings / 177 info`、扩展包与 artifact 上传成功。它仅证明 GitHub runner 的该次构建输入通过，不覆盖外机 R09 `1/2`。

用户提供的外机日志仍待精确定位：外机报告 116 个 WPF 类，本机隔离也是116、#1055同提交SDK8 CI为125；类数不再作为根因判断。当前 R09 源 line151 是颜色断言，粘贴栈的 line151 却报 `Thickness(2/1)`；旧 Chrome 厚度断言在历史 `3f7d0b30`。重复附件 SHA-256 `F3DDDC6445E0BB1A5E4CD44EEEEC2C2C98834FC8C81482A67E456C5DB5864514`，没有新的运行身份；缺该次 `summary.json`、TRX 与 test DLL/PDB/plugin DLL 哈希，不能判断具体 checkout/程序集/符号错配。本机与 CI 通过均不覆盖外机真实失败。R12-04 四个源码/测试哈希已与现有证据一致。下一步按积压和 R23-08 准入复核选择依赖满足的小批；外机身份材料暂缺不阻塞独立工作。

目前物理第二屏、真实 Playnite/OS 输入、CEF 权限和 ETW 权限仍是外部边界；R05-03 fail-closed，不绕过系统跟踪拒绝。临时 `.tmp` 保留仍在使用的输出；其他任务的未跟踪 TRX/artifact 不擅自删除。
## AI/Codex 启动协议

开始开发前依次阅读：

1. `docs/ai/CURRENT_STATE.md`。
2. `docs/ai/PROJECT_MEMORY.md`（本文件）。
3. `docs/ai/WORKLOG.md` 最新相关阶段，历史按需查找。
4. `docs/DEVELOPMENT_HANDOFF.md`。
5. 最近 15–30 条 Git log、`git status`；联网后 fetch 当前远端分支，保留未知本地变更。
6. `docs/AUTONOMOUS_BACKLOG.md` 活动队列和目标任务的证据。
7. UI 任务另读仓库 `.codex/skills/wpf-apple-desktop-ui/SKILL.md`、相关 references、`docs/design/APPLE_WPF_IMPLEMENTATION_PROMPT.md` 和 `docs/design/UI_CHANGE_GATE.md`。

事实来源为当前代码、证据、Git 和用户指令。默认每阶段构建、测试、同步文档、单独 commit 并 push 当前远端分支（默认 main）；不要被历史“不得 push main”覆盖。禁止提交 artifacts、.tmp、bin/obj、用户配置、凭据和可再生大包。

## 项目与模块

- 当前版本由 `Directory.Build.props` 与 `src/GameSaveCenter.Playnite/extension.yaml` 决定：0.6.73。版本号不能替代 commit、SHA-256、MVID 的包身份核对。
- Playnite 是唯一主要 UI（net462 WPF GenericPlugin），扩展 ID `66e9f2d7-67bb-43ef-b62a-b8e60734fcec`；Worker 是独立 net8 Windows 进程。
- Core/Contracts 为 netstandard2.0；Named Pipe 连接 Playnite 与 Worker；SQLite、存档归档、媒体和 GameTools 文件由 Worker 管理。
- Worker `BackupOrchestrator`、`RestoreOrchestrator`、`RestoreReadinessService`、`RemoteBackupStagingService` 管理备份/安全恢复；`TaskCoordinator` 管理任务；`MediaSyncService` 管理媒体；`GameToolService` 管理工具。
- `DashboardViewModel` 已拆出 16 个 partial，主文件仍约 6480 行。只能基于实际修改痛点或测量结果继续提取职责，不重建已有服务/DTO/命令。
- 六工作区与 Settings 共用生产局部资源；不要把插件资源写进 Playnite 全局 Application.Resources。

## 安全与业务语义

- 恢复前置检查、PreRestore、失败回滚、UndoRestore、运行中拒绝恢复和冲突确认不可因 UI 简化丢失。多设备分叉由用户决策，禁止自动合并/覆盖。
- 远端内容先隔离 staging、归档/路径/清单/大小/hash 校验，再进入既有恢复链。Manifest 非法、缺失文件、路径越界或无法确认时不能标 Ready。
- Rclone 使用既有 copy/check/lsf/cat/version 白名单，不引入 sync/delete/purge。恢复、删除、保留清理和外部进程验证只使用隔离夹具。
- 写 IPC 断连/超时可意味着已提交；重放保留 RequestId 和 ledger 语义，取消不等于回滚。不可把未知结果包装为失败后可盲重试。
- GameTool 启动走 Worker。CloseOnGameExit 只处理本 Session 启动、PID 与 StartTime 均确认的进程；未解析链接、脚本/默认程序不假装可追踪。
- 自定义工具已有实例按完整 EXE 路径匹配；反作弊场景按既有分类门禁执行，不自动运行 Unknown/GameModification，不提供绕过保护。
- 初次环境检查只读或操作自身临时探针，不自动备份/同步/上传/删除真实数据。`OnboardingCompleted` 的跳过和再次检查语义保留。
- Playnite 退出回收自身 Worker，防止 shutdown 竞态重启。不得按名称杀任意 Worker；安装时路径/归属未知即停止，不默认提权。

## UI 与性能规则

- Demo-first 优先于历史生产结构和通用 Apple 风格。Demo 路径当前缺失时沿用已恢复生产资源并明确限制。当前游戏选框与滚动条为明确保留例外。
- 真实 Binding、命令、错误/取消、键盘与 UI Automation、主题/DPI 和可扩展列表必须保持。当前授权允许整页重构，旧历史“不能替换/不能迁移”不冻结页面。
- 焦点/Popup夹具必须先断言控件实际可见、Focus()成功及Keyboard.FocusedElement；紧凑布局隐藏筛选行时不能程序化打开隐藏ComboBox再当成真实Tab路径。MoveFocus只证明受控遍历，OS键盘/IME与Playnite宿主仍单列。
- 页面内容有限测量，主列表/表格内部虚拟化；最小可读视口与短窗回退按当前生产几何门禁验证，不机械沿用历史固定高度。
- DataGrid Item 滚动与稳定行样式已有真实回归经验，勿盲改 Pixel 或关闭虚拟化。Media Inbox 宿主空白需同进程几何诊断，不能由离屏通过推断修复。
- 资源走动态主题令牌；普通计数/装饰用 accent/neutral，Info/Success/Warning/Error 表达真实语义，不能只靠颜色表达状态。
- 动态 ComboBox 重排/集合刷新可能清空或反写选择；保持来源唯一、逻辑默认值和草稿上下文。无关刷新不能抢走用户选择。
- 大库先显示 SQLite 缓存；不在 UI 线程全量匹配、图片解码、文件/网络/SQLite IO；无数据变化避免 Reset/CollectionView.Refresh。
- 缩略图后台限并发、取消、LRU、Freeze 后回 UI；动画优先 render 属性，卸载释放时钟/订阅。真实性能必须测量，Rendering 回调不是 presented frame。

## 证据规则和当前风险

- 活动动画测试需显式控制应用与系统偏好两个前提；生产默认必须保留HighContrast/ClientAreaAnimation，测试用实例内internal输入，不修改OS/全局开关。CI无动画偏好不能与“动画应启动”混淆；默认真实偏好路径必须单独验证。
- build-diagnostics保存native实际退出码、分步console/TRX/SDK/dirty源码hash/DLL身份。PS5.1会将stderr包装成ErrorRecord，采集过程需继续至LASTEXITCODE；缺失exe必须有launchError而不能继承0。摘要标注testsRequested；诊断异常也不能阻止恢复环境。
- SDK声明以 `global.json` 精确 `8.0.423/disable/不允许preview` 为准；CI 每个 run 按日志记录实际选中版本。当前 main #947–#950 选中 SDK `8.0.423`；用户指定旧 run #919/#920 选中 SDK `10.0.401`，版本结果不可互相替代。缺失版本须明确失败，不能用其他预装 SDK 替代；升级另做完整编译/测试/包验证。2026-10-12 将 SDK8 安装在隔离路径 `D:\GameSaveCenterTools\dotnet-sdk-8.0.423`，仅临时调整进程 PATH，不改系统 PATH；C#12.0 而非 latest。
- 带预期非零native负例的自测入口须在finally之后显式exit0；Actions pwsh外层会继承LASTEXITCODE，出现断言全部通过但步骤失败。不能在采集器清零实际负例；异常路径仍非零。自测诊断也纳入artifact目录，上传只JSON/TXT/TRX，不能以always步骤success当作artifact存在。
- TRX脱敏除TestRun.name/computerName/路径，还须覆盖runUser和runDeploymentRoot；保留GUID、方法/断言/堆栈/计数和DLL身份。合成元数据负例同时检验异常路径仍非零。
- run37565955357/37566483862已实际失败上传并下载：SDK10侧栏22/22通过，native偏好记录ClientAreaAnimation=false；仍在Q14错误态36→37 DIP失败。STA干预确认2 DIP错误边框改变测量；独立私有几何夹具复现Light/Dark654 DIP单行20 DIP margin，620/660通过。探针不是生产修复或宿主验收。
- SDK阶段历史：2026-10-07本机侧栏22/22、source470/18、WPF前23类通过；Q14独立及全量先因错误态TextBox高度36→37 DIP失败，未到654 DIP，原边距问题继续开放。GscMotion/build变更还命中R00-03/R01-01/R01-04，补证范围增加，baseline不能静默转绿。
- 受控自动行为、离屏截图、真实宿主、物理 DPI/跨屏、包安装身份分别记账。历史宿主成功不签收当前候选。
- IPC取消通过dispose解除IO时，任务可先以IOException或EOF结束；仅在linked token确已取消的transport路径按host→caller→timeout分类，并保留RequestId/未知提交，不覆盖服务拒绝。测试服务用握手、每场景独立管道和失败路径异步清理，不用固定延时猜取消时机。
- 当前活动缺口：唯一执行入口是 `docs/AUTONOMOUS_BACKLOG.md`。Worker 取消回调测试修正与 #1056 双 SDK CI 已收口；R12-04 文件身份复核完成并维持“已满足”。本轮重新扫描 Round2/3 的 208/192 项及 R23-08：未发现新的可独立实施代码缺口；只读桌面只有一块活动屏且没有可见 Playnite/Worker 进程。下一执行点为取得外机 R09 同次诊断身份材料、环境前置发生变化或任务表出现具体证据缺口后再领取对应小批。外机 `1/2` 和 exit1 保留；line151/Thickness 映射不符，重复日志 SHA 相同，缺同次构建身份，根因仍未确认。R00/R01 freshness 与 Render QA 的当前事实见本文件上文。真实 Playnite/OS UIA/IME、物理 DPI/跨屏、presented frame、系统时钟事件即时刷新、ETW/宿主性能仍分别记账。
- `599d2fd` 的 `14 fresh / 0 stale` 基线已对受影响行为和负例复验；`0243c108` 的四项 stale 是更新前中间状态。`4078f23d` 当前源路径复核仍为 `14 fresh / 0 stale`。Package identity 与真实宿主边界仍独立。
- ENV-001 历史 WMI 命令行读取拒绝、CEF 0x5 与单实例边界尚无解除证据；条件变化后先做 fail-closed 预检，不关闭用户实例，不反复撞同一边界。
- Q06/R08 本机不复现不能关闭用户问题；需要失败方法、断言、堆栈、DLL 身份。COM 清理日志不自动解释测试失败。
- 当前任务状态和完成定义只维护在 [活动积压清单](../AUTONOMOUS_BACKLOG.md)；Q/R 账本是证据矩阵，不能合并为产品完成率。

- 2026-10-07共享UI阶段：错误描边不得改变原生内容测量；行距按Arrange后的layout slot判断实际多行并恢复作者margin，Unload释放LayoutUpdated。Q14已通过定向及首轮全量入口；随后R02失败由旧clean DLL同样复现。共享按钮须区分文本的有限Stretch测量与视觉树的HorizontalContentAlignment，避免拉伸固定/Auto列产生额外DIP。原4项已过，联合完整门禁待验；当前受影响证据为10项，旧渲染不能签收新增按钮模板。

- 2026-10-12 CLOSE-PICKER 复验：在 STA WPF ViewModel 测试夹具中没有 `SynchronizationContext` 时，`Task.Delay(...).ConfigureAwait(false)` 的延续可能运行在线程池；刷新 WPF `CollectionView` 必须回到创建它的 Dispatcher。保留 20 ms 输入合并，取消的旧 Dispatcher 回调还要在执行时检查 cancellation token。新负例先在旧实现失败、修复后 R05 全量类通过。生产 TextBox 错误态两 DIP 描边采用独立 `ValidationStroke` overlay，因此行为测试应检查 overlay 的 Opacity/Thickness，并断言布局 Chrome 仍为 1 DIP；不能要求 Chrome 为 2 DIP。

### 2026-10-14 Q24-07 当前行为证据

高对比行为应通过生产资源字典与真实 WPF 控件模板状态验证，不以资源键或 XAML 字符串存在作为完成依据。当前测试覆盖 Follow/Light/Dark、宿主明暗、玻璃/动效开关、禁用按钮和选项、选中态、焦点框、Popup、活动动画时钟移除和恢复。WPF 合成 KeyDown 可证明模板 pressed 状态；直接构造 KeyUp 不更新 `KeyboardDevice`，当前释放使用明示的受控 `IsPressed` 状态探针，不得记为物理键盘输入。证据链接：[Q24-07 高对比复核包](../design/reviews/ui-finesse-round3-20260915/evidence/Q24-07-CURRENT-MAIN-HIGH-CONTRAST-RECHECK-20261014/README.md)。本机 Release SDK8 不是 SDK10 CI 或真实 Playnite 视觉证据。当前 freshness stale 的四项按计划下一批重跑，不提前把旧基线签为 fresh。

## 文档维护约定

CURRENT_STATE 保持短摘要，PROJECT_MEMORY 保持稳定规则，HANDOFF 保持下一步；WORKLOG 保留阶段记录。不要向三份入口反复复制完整 TRX 数字与过期下一项。证据全文独立归档，入口链接到同一来源。

历史快照与旧 ledger 状态原文保留；任何状态升级必须有对应证据。临时文件删除前确认绝对路径在仓库 artifacts/.tmp 内，使用 PowerShell `Remove-Item -LiteralPath -Recurse -Force`，只清理已确认不用的本轮产物。

### 2026-10-13 R06-07 当前 main 证据规则

`87d27592` 相对 `73d0f6e6` 只有文档变化，故可复用同源 SDK8 Release/net472 二进制运行不读源码根的实际行为用例；遇到 `TestRepositoryContext` 时必须新建匹配当前 HEAD 的程序集或跳过，不得将旧程序集用于源码型测试。当前可重用行为证据为 16/16；旧 `.tmp/r06-07-emptytables` 图报告绑定 `5046bf8f` 且目录已失，不能声称是当前 main 截图。用户已提供 #962/#963 诊断包：CI SDK均为 `8.0.423`，构建通过，随后同一个 `R08NumericChangeBehaviorTests` 的反馈计数断言失败（0/期望1）；这不是 R06-07/08 生产行为失败。证据包和 ZIP SHA 见 R08-06 当前 main README。R06-08 当前行为复核为 14/14；#964 对应源码 `83fbee97` 失败于“编译与测试”，因当时仍含上述旧测试前提；目前有本机修正，等待新 Actions 终态。

### R06-08 当前行为门禁

旧长详情测试只放两行且断言两行可见，不能证明四行门禁。当前通过的补强是在 STA WPF 窗口加载 10 条合成任务，打开长失败详情并实测至少 4 行完整可见；还核对选中详情对象、错误码、独立详情滚动与行高上限。R06-08 当前复核为 14/14（SDK8.0.423，Release 0/0，XAML24/24）。在新 Actions 对 R08-06 门控修正给出终态后先复核 R00/R01，再推进依赖已满足的 Q/R 小批。

### R08-06 系统动效输入

`NumericChangeFeedback` 默认尊重 `GscMotion.IsEnabled(true)`（High Contrast / Windows ClientAreaAnimation）。R08-06 正例必须用按 TextBlock 状态隔离的 `SystemMotionEnabledForAudit=true` 固定输入，确保在关闭系统动画的 runner 上仍实际采样 WPF 动画时钟；另测 override=false 无动画负例，并覆盖 override 为 null 时跟随真实系统输入。不要为通过 CI 改生产动效策略。用户提供 #962/#963 诊断包显示旧夹具反馈计数0/期望1，CI SDK为8.0.423；工作流SDK与旧 #919/#920 的10.0.401不混写。当前本机 SDK8 solution `0/0`、行为 `5/5`，新 Actions 仍须终态确认；门禁后按当前续跑顺序先复核 R00/R01，再推进依赖已满足的 Q/R。

### SDK 版本复核边界

仓库 `global.json` 固定 SDK `8.0.423`；记录 CI 结果时必须写明实际 `dotnet --version`，不能把 workflow 中的 `setup-dotnet` 版本当作已选版本。若要求 SDK `10.0.401` 而仓库仍锁 SDK8，应使用单独 CI job，在临时 detached worktree 中仅改该 worktree 的 `global.json` 并运行同一 Release build/test 脚本；保留 SDK8 基线 job。SDK9 本机完整构建只算交叉检查，不能替代 SDK10 CI。CI 失败先取完整日志与失败诊断包，不凭摘要猜测。隔离 WPF TRX 中旧架构断言可能标为 `NotExecuted`，应读取其具体 reason 并与通过数分开记录。

### 2026-10-13 当前 CI 与 R00/R01 检查点

`4aa1ec7` 对应 #968/run `38366158855` 已终态成功；SDK8 `8.0.423` 和 SDK10 `10.0.401` 是独立 job。#969/run `38377673615` 是随后的文档同步并成功。#967 的 SDK8 源码门禁因 `verify-sdk-build.ps1` 缺 UTF-8 BOM 失败；同一 `validate-source.py` guard 在本机复现后补 BOM。用户 ZIP 对应绿色 SDK10 job `115178885908`，SHA 与 artifact `11795654027` 一致，SDK `10.0.401`，117 TRX、1566 passed/0 failed。R00/R01 freshness 为 14/14，package identity 未提供；不代表 Playnite 宿主、IME、物理 DPI/跨屏或最终呈现已验证。证据：[R00/R01 当前 main 与 CI 复核](../design/reviews/ui-finesse-round3-20260915/evidence/R00-R01-CURRENT-MAIN-RECHECK-20261013/README.md)。

### 2026-10-13 R05-03 隔离宿主保护点

既有 `scripts/real-host-audit.ps1` 在独立 `--userdatadir` 下未于关闭请求后 20 秒退出，准确错误为 `Playnite isolated profile bootstrap did not exit gracefully; refusing to force-stop it and leave a safe-start marker.` Runner 在扩展安装前退出；配置文件已创建，无 safe-start marker，无插件安装或合成库，真实 Popup 交互/呈现帧没有运行。PID `34512` 是本次 exe/profile；当前观察到 4 个 CefSharp 子进程仍绑定隔离 profile，host 本身仍 responding、HWND 为 0。不要强制终止或删除 profile；等整个树自行退出后再判断是否可重试。证据摘要在 `evidence/R05-03-CURRENT-MAIN-RECHECK-20261013/host-preflight-20261013.json`，runner 原始输出由对应 README 引用。

### 2026-10-13 #967 R08-01 自动证据

用户给的 SDK10 ZIP 内独立 `R08MotionReverseBehaviorTests` TRX 为 `2/2`，已复制该 5.5 KB 精确 TRX 到 `evidence/R08-01-CURRENT-MAIN-RECHECK-20261013/`；包 SDK `10.0.401`。测试 source SHA 与当前相同。此自动证据不解释用户机器 `1/2`；未取得具体失败方法/断言/堆栈/DLL 身份，不能说该故障已修复。

### 2026-10-14 R08-01 按钮反向与 R09 外机失败对照

当前 main 基线 066b9a6c01057c044ddc0d7c63ff70ce3dd7a6b3 仅新增 R08 测试夹具，实际生产按钮模板 hover/press 动画现覆盖切换瞬间、方向推进与最后目标；R08 类 3/3、Q06 按钮状态 1/1。本机 SDK 为 9.0.302，不等价 global.json SDK 8.0.423 或 CI SDK 10.0.401。用户机器 R08 1/2 的原始方法/堆栈/DLL 仍缺，不能声称已修复。

外机 R09 日志的 Expected 2 / Actual 1 与历史 3f7d0b30 的 Chrome 2 DIP 断言相符；f13b9e05 已根据生产 ValidationStroke 分层将测试改为 Chrome 1 DIP + 错误层 2 DIP。当前 main R09 本机 SDK9 2/2；未取得外机 HEAD 与 DLL/PDB 身份，故旧分支、旧构建产物或源码/PDB错配仍只是最可能解释。证据分别见 R08-01 与 CI-R09 外机复核包。

截至 2026-10-14，公开 run 38393317198 (#971) 仍 In progress；#978/run 38509928015 Failure，完整日志需登录且 runner SDK/根因未知。推送本批后继续跟踪当前 SDK8/SDK10 lanes；CI 未终态前不记门禁完成。下一项 R08-02。真实 Playnite、OS 输入/读屏、物理 DPI/跨屏、呈现帧、ETW 与宿主性能仍待验。

### 2026-10-14 R08-02 应用与系统动效热关闭复核

当前 main 的应用设置开关与系统策略变更均复用 `GscMotion.NormalizeAll` 清除活动时钟，不重建动画服务。SDK `9.0.302` Release `0/0`；应用 ToggleSwitch `1/1`、调用真实 `OnSystemParametersChanged` 的实例级系统策略模拟 `1/1`、生产侧栏 reduced-motion `2/2`、Dashboard/Settings 订阅与卸载保护门禁 `1/1`。两种热关闭均验证活动入场时钟清理、稳定值 `Opacity/Y=1/0`，重新开启后不重播旧动画。系统策略通过实例级内部测试 provider 输入；未改 Windows 设置或触发真实静态系统事件。证据见 `evidence/R08-02-CURRENT-MAIN-RECHECK-20261014/README.md`。WPF 测试退出仍有 TextServices COM 清理异常噪声，根因未知、退出码成功。本机 SDK9 不替代 SDK8.0.423/CI SDK10.0.401；CI 推送后须分别追踪终态。R08-03 隐藏/最小化 spinner 停机已按当前 main 刷新复核，实际 Windows 动效通知、Playnite、UIA/读屏、物理 DPI/呈现帧/ETW/宿主性能仍未验。下一项 R08-04。

### 2026-10-14 R08-03 当前 main 隐藏与最小化停机复核

R08-03 生产服务、模板与 storyboard 已存在，未改生产代码。R08-02 代码阶段在提交前对最终 `31d64e42` 源内容完成 SDK `9.0.302` 隔离 Release build `0/0`；构建 ProductVersion 是父提交 `d3cb0b2d`，源码/程序集 SHA 与 MVID 独立记录。提交后对同一 hash-identified 测试程序集执行 `IndeterminateProgressPausesForHiddenTabAndMinimizedWindowThenResumes`，实际运动、隐藏暂停、忙态保留、恢复运动和最小化暂停均通过 `1/1`。新隔离 build 尝试在 MSBuild ProjectReference target 以 exit 1/0 warning/0 error 失败，根因日志无 MSB/NU 信息；不记为 build pass。证据见 `evidence/R08-03-CURRENT-MAIN-RECHECK-20261014/README.md`。当前 Actions 公开页仍只列到 #979，没显示 `31d64e42` run；#971 仍 In Progress、#978 Failure 且日志需登录，须继续查 SDK8/SDK10.0.401 到终态。下一项 R08-04 业务完成节奏；真实 Playnite/呈现/物理最小化/ETW/宿主性能仍未验。

### 2026-10-14 R08-04 当前 main 业务完成节奏复核

先复用 `BusyOperationCoordinator`、共享按钮忙态/延时 spinner、`FeedbackToast` AutomationPeer、`DashboardView.ApplyToastAutomation` 和 `TaskStatusDto`；生产代码、命令绑定和安全语义未改。新增一条成功慢任务行为：未完成的 action 保持忙态且不触发完成，放行后回调一次、恢复忙态，不触发失败/取消反馈；成功、失败（错误码与详情优先）、取消 DTO 的状态和详情使用精确值断言。隔离 SDK `9.0.302` Release solution `0 warning / 0 error`，`R02BusyStateTests 5/5` + `R08BusinessFeedbackBehaviorTests 4/4`，共 `9/9`；本机缺 global.json 的 `8.0.423`，该结果不替代 SDK8/CI SDK `10.0.401`。源校验和 diff check 通过；WPF 静态检查 `0 errors/29 warnings/177 info`（扫描包含无关 `.tmp/r12-04-stage` 历史布局 warning）。完整测试/程序集身份和限制见 `evidence/R08-04-CURRENT-MAIN-RECHECK-20261014/README.md`。第一次没有传 `GSC_BUILD_COMMIT` 的源契约用例拒绝未知身份程序集；按仓库构建约定重建后完整定向类通过。公共 Actions 页仍停到 #979 queued；#971 In progress，#978 Failure 且需登录获取日志；当前 R08 提交的 SDK8/SDK10 CI 尚未可见。真实 Playnite/Worker/Narrator、外部 UIA、宿主通知、呈现帧/物理 DPI、ETW 与宿主性能仍待验。下一项 R08-05。

## 2026-10-14 R09-05 外机失败当前 main 复核

- 当前 main 552b8168 的隔离 SDK 9.0.302 Release solution build 为 0 warning/0 error，R09FocusOutlineBehaviorTests 2/2。用户所附 SDK10.0.401 ZIP（commit 983fee5）与 SDK8.0.423 ZIP（commit c9f9179）内对应 R09 结果也为 2/2；commit 5a6647c 的另一 ZIP 不含 R09 TRX。
- 当前 test source SHA 为 A5AAB9E…B2CBC98；插件/测试 DLL SHA、MVID、ProductVersion 以及脱敏 TRX 已记录在 CI-R09-OTHER-MACHINE-FAILURE-RECHECK-20261014。没有更改生产模板或降低断言。
- 外机日志第 151 行、Expected 2/Actual 1 对应历史 Chrome 2 DIP 断言；当前第 151 行是错误色、第 154 行读取 ValidationStroke 2 DIP。旧测试程序集/分支或 PDB 与源码错配是推断，缺少外机 HEAD/DLL/PDB 身份，不能确认。libpng iCCP 与该断言无关；TextServices COM 异常发生在失败后的 WPF 收尾。
- #971/run 38393317198 公开页仍为 In progress，日志需登录；#978/run 38509928015 为 Failure，完整日志需登录。Actions 列表仅到 #979（ae3c565），未显示当前 HEAD run；SDK9 本机结果不替代 SDK8/SDK10.0.401 CI。
- 用户暂时不可回复，所需的外机提交、工作树、测试 DLL SHA/ProductVersion 与源码 145–160 行记录为待验，不阻塞当前 R08-05。
## 2026-10-14 R08-05 页面切换当前 main 复核

- 现有生产页缓存与 PageHost 行为已满足任务方向，没有重建页面服务或动画体系。本次没有生产代码/XAML 改动，只复核真实 shell/page 行为并刷新当前 main 证据。
- 隔离 SDK 9.0.302 Release solution build 0 warning/0 error；R08PageSwitch 4/4、ProductionShellChrome 22/22、UiFinesseFoundation 9/9、R02 Busy 5/5、R08 business feedback 4/4，共 44/44。source validation 通过，XAML 结构 24/24，文档同步后的 diff check 另行校验。详细 SHA/MVID/TRX 在 R08-05-CURRENT-MAIN-RECHECK-20261014。
- 真实 WPF 页面切换保留 DataGrid offset 10→10、任务引用、选中项与 DataContext；两次不同页面切换各 Measure/Arrange 1/1，同页重入 0/0；96 项集合和筛选/页签状态恢复，refresh command 0 次。七次快速路由后导航标题/焦点一致；PageHost 无 Blur/整页入场动画。
- ShellChrome 22 项均通过、TRX exit 0；testhost 收尾出现 TextServices COM 异常输出，根因未知。误用类名的零测试筛选器未计入 44/44。
- 本机 SDK9 不替代仓库 SDK8.0.423 或 CI SDK10.0.401。公开 Actions 仍只显示 #979（ae3c565），#971 In progress，#978 Failure 且日志需登录；推送的 R09 文档提交 8dda9995 尚未显示 run。真实 Playnite、物理 DPI/跨屏、presented frame、UIA/读屏、ETW/宿主性能仍待验。
- R08-05 当前 main 受控条件已满足，视觉不适用，不生成截图。下一可执行任务 R07-08 当前 main resize 复核；先查现有实现/测试及 #960/#968 的提交边界，不凭旧失败摘要猜修。
### 2026-10-14 R07-08 当前主线复核

R07 resize 回归应按已加载 PageHost/DataGrid 的实际窗口与内容区尺寸验收；compact 详情关闭后须路由生产 open button 才能证明详情可在 resize 中保持。复核五阶段的 PageHost 实际几何、picker/焦点/任务表可达、详情/picker MaxHeight 清理和恢复误差。SDK9 单机行为通过不能解释或覆盖 #960 未知 CI 失败，也不等同真实拖动帧时间/Playnite。当前证据与程序集身份见 evidence/R07-08-CURRENT-MAIN-RECHECK-20261014。

## 2026-10-14 R10-02 当前游戏定位行为复核

- 复用 `TaskSourceNavigationResolver`、`TaskStatusDto`、`GamePickerViewModel` 和 Shell 当前游戏选框。代码提交 `13704e75` 仅增加默认关闭的内部工作区加载审计回调与行为用例；正常生产 Worker 调度路径不变。
- 本机 SDK `9.0.302`，Playnite `net462`、测试 `net472`，Release 编译成功；R10 context `5/5`，含同名重名选框正例、缺失 ID 不回退负例、生产任务命令准确路由与缺失目标保护。相邻导航/来源组 `25/25`，无失败/跳过。source validator、diff check 通过。
- 源码/DLL/MVID/TRX 哈希、命令与行为结果在 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R10-02-CURRENT-MAIN-RECHECK-20261014/README.md`。构建 ProductVersion 带前置 hash `a94819b`，因此以源码/DLL SHA/MVID 标识实际构建，不冒称 SDK10 CI。
- Actions 列表最新可读 #972（`8546aca`）仍 In progress；本轮尚未推送。用户指定旧 run #920/`36677346317`（`77de7f4`）现 Failure、7m04s、无 artifact，日志登录限制，实际 SDK/根因未知。真实 Playnite/媒体服务、presented frame、物理 DPI/跨屏、OS UIA/读屏、ETW/宿主性能未验。CI 终态后下一项 R10-03 搜索快捷键。

## 2026-10-15 R10-02 当前 main 复核

- R10-02 延用任务稳定 ID 路由与全局 GamePicker。本机 SDK `8.0.423` / VSTest `17.11.1`，Playnite `net462` 与测试 `net472` 的 Release 项目/引用构建 `0 warning / 0 error`；R10 及相邻导航行为 `25/25`，新增生产 Shell 同名选框 STA 交互后合计 `32/32`。XAML `24/24`，source validation/diff check 通过。
- 新交互测试从生产顶部按钮打开当前游戏选框，在相同显示名中选择具体 ListBox 项，验证选框关闭、焦点归还、对象引用和 `PlayniteId` 均准确，且当前游戏名称绑定显示。任务命令已有按 `GameId` 导航正例和目标缺失时保持原状态的负例；未增加生产功能代码。
- SDK8 本机不替代 SDK10.0.401 CI；`6eac7b90` 与证据文档提交的 Actions run ID/终态尚未读取。当前证据包保存六份脱敏 TRX、程序集身份与边界。真实 Playnite、媒体后端、IME/物理输入、UIA/读屏、物理 DPI/跨屏和呈现帧未验。

## 2026-10-14 R10-07 / R10-08 当前 main 复核

- R10-07 复用现有侧栏实现，Release STA 行为 22/22；首次未设置 GSC_BUILD_COMMIT 的身份检查失败不是产品回归。有效测试设置仓库约定的构建身份、重建后执行，源/DLL/MVID/TRX 见 evidence/R10-07-CURRENT-MAIN-RECHECK-20261014/README.md。
- R10-08 复核发现 GSC-058 把首页最近访问卡片永久折叠，历史测试手动改可见性后才点命令，无法证明入口可达。复用已有卡片、DTO、共享按钮模板，增加同一概览区域内最近任务/最近访问切换；通过 STA Window 验证焦点、Automation 名称、双方可见性与列表分离，以及合成访问行命令。R10RecentAccessBehaviorTests 2/2，没有新增服务/DTO/选框/滚动条或用户设置访问。
- 本轮本机 SDK 9.0.302、VSTest 17.14.1；Playnite net462 与测试 net472 Release build 成功；R10-08 2/2、R09 2/2、R10-07 22/22；source validation、XAML 24/24、RenderHarness Light/Dark render-qa OK/0 PROBLEM、diff check 通过。WPF 静态检查 0 errors/29 warnings/177 info，Overview 的 scroll/StackPanel warning 是静态审查命中；本轮未改滚动模型。离屏截图仅覆盖默认最近任务模式。
- 外机 R09 日志的旧 Chrome 2 DIP 期望与当前 Chrome 1 DIP、独立 ValidationStroke 2 DIP 区分；外机 HEAD、源、DLL/PDB 身份仍未知，错配只是推测。libpng warning 不在失败调用栈；TextServices COM 异常在 WPF 失败后的收尾阶段。
- #993/run 38570078343 对 e0a72c9 终态失败：SDK10 job 115872658151、默认 build job 115872658675 均 exit1；日志 API 403、公开 job 页面要求登录，失败根因未知。SDK9 不替代仓库 SDK8.0.423 或 CI SDK10.0.401。当前批次待提交推送后跟踪新 Actions；CI 日志受限时记录限制并推进 R11-01。真实 Playnite/host、物理 DPI/跨屏、最终呈现、外部 UIA/读屏/IME、ETW/宿主性能仍未验。

## 2026-10-14 R11-01 当前 main 复核

- R10-07 侧栏 ProductionShellChromeSourceTests 22/22，R10-08 最近访问入口 2/2。R11-01 复用已有实现，Release 当前 main 行为及 Save 邻接类 17/17；本轮不重建 DTO、DataGrid 或命令。
- SDK9.0.302/VSTest17.14.1 重建 Playnite net462 与测试 net472，0 warning/0 error；source validator、XAML 24/24、git diff --check 通过。
- 真实 SaveCenterView/隔离 STA Window+合成 DTO 验证七列、未知来源/恢复状态、Ready 文案和锁定不显示安全绿；邻接类覆盖空态区分、排序正/负路径、列宽最小值/保存恢复/选择性重置。脱敏 TRX、源/DLL/MVID/PDB SHA 在 evidence/R11-01-CURRENT-MAIN-RECHECK-20261014/README.md。
- 无视觉源码改动，不生成截图；真实 Playnite/package-host、UIA/读屏/IME、物理 DPI/跨屏、最终呈现帧、ETW/宿主性能未验。
- 58090d99 已推送；匿名 GitHub Actions API 查询返回 HTTP 403 rate limit。最新公开 Actions 页面是旧缓存快照：显示 972 workflow results，最新可见 #972（head 8546aca）In progress；较早抓取又曾显示 #979。两个快照均不含 58090d99，不能确认该提交的 run 是否创建、也不知道终态。继续间隔查询；在结果明确前不报 CI 完成。 SDK9 不替代 SDK8.0.423/SDK10.0.401。下一项 R11-02 当前 main 复核。

### 2026-10-14 R11-04 版本备注与外机焦点日志

R11 版本说明已有 Dashboard 编辑/取消、Ludusavi 元数据更新与 SQLite 按 `(PlayniteId, BackupId)` upsert。取消必须由生产 `CancelBackupMetadataEdit` 恢复备注/锁定值并保持 DTO 和 BackupId；页面夹具若用测试 ICommand 适配器，应明确只证明真实 View/Binding 到生产方法的路径，不称为真实生产 RelayCommand 实例。重复备注只能由稳定 BackupId 选中；重启读取测试应从真实生产 parser 解析合成 Ludusavi JSON、在隔离数据库 upsert 后由新 Store 实例读回，而非仅手工构造 DTO。

外机 R09 `Expected Thickness(2)` / `Actual Thickness(1)`、行151与旧 commit `3f7d0b30` 的 Chrome 两 DIP 断言吻合。当前应检查 Chrome 1 DIP 和独立 ValidationStroke 2 DIP；仅本机复验无法得知外机 source/DLL/PDB。将旧源/程序集或 PDB 错配保留为未证实推断。`libpng iCCP` warning 和失败后的 TextServices COM shutdown 异常不能解释厚度断言。SDK8 当前 main 复跑用于本机对照，不替代 SDK10 CI。

### 2026-10-15 R10-03 搜索快捷键当前 main

R10-03 复用现有 `SearchShortcutPolicy`、PreviewKeyDown、Shell 搜索命令和工作区 TextBox 映射；当前 main `95109c2` 的生产路由行为补为真实 WPF routed-event，Media 页选中时实际把键盘焦点移到 `MediaSearchTextBox`。弹层占用、Ctrl+Z/C、普通 F 负例确认事件未消费且焦点不变。SDK8.0.423/VSTest17.11.1 定向 Release build `0/0`，相关 R10 组 `13/13`；R09 同身份对照 `2/2`。脱敏 TRX、二进制身份与边界见 [R10-03 证据包](../design/reviews/ui-finesse-round3-20260915/evidence/R10-03-CURRENT-MAIN-RECHECK-20261015/README.md)。真实 Playnite、全局热键协作、物理键盘/IME、UIA、呈现、DPI/跨屏/ETW/宿主性能未验。

`95109c2` 对应 run `38645600070` 上次为 In progress；本次 API 连接被拒绝，浏览器读取超时，本机无 `gh`，所以终态未确认。推送新的 R10-03 阶段后另查新 Actions run；SDK8 结果不得替代 SDK10.0.401。下一项 R10-04 快捷键帮助。外机 R09 仍需要相同失败运行的 `summary.json`、完整 console/TRX 与实际测试 DLL/PDB 身份；截至用户可回复前继续其他任务。
## 2026-10-15 R10-04 当前 main 快捷键帮助复核

R10-04 已复用现有 KeyboardShortcutHelpCatalog、FocusWorkspaceSearchCommand 和 CanExecute(null)，没有重复实现。基线 main 6cf279238d55fe3d139fa2cb0eb7b09229a7478a；本机只有 SDK9.0.302/VSTest17.14.1，仓库固定 SDK8.0.423 不可用。本轮隔离全解方案 Release build 0 warning / 0 error，R10帮助和相邻行为 18/18，R09 2/2，XAML 24/24、source validation/diff check 通过。实际 Shell Popup 开闭、当前媒体页 Ctrl+F 条目、生产命令禁用后的空列表均通过；没有生产代码或 XAML 改动。CI run ID/终态未知，SDK9 不替代 SDK8/SDK10.0.401。外机 R09 根因仍未知，需外机实际 HEAD/source/DLL/PDB 身份。证据：docs/design/reviews/ui-finesse-round3-20260915/evidence/R10-04-CURRENT-MAIN-RECHECK-20261015/README.md。下一项 R10-05 筛选预设；先检查已有模型和持久化，不重建服务。
## 2026-10-15 R10-05 当前 main 筛选预设行为补证

R10-05 沿用现有 FilterPresetDefinition、设置规范化、任务/媒体 preset 命令及确认流程。原工具栏按钮用测试命令，生产快照/应用方法此前只有源码接线断言，因此新增一个 STA 行为用例调用生产 CreateTaskFilterPreset 和 ApplyTaskFilterPreset，验证标量筛选状态恢复，以及状态/游戏/类型动态项过期后回退“全部”。非法/旧配置回退、JSON scalar roundtrip、真实重命名/删除取消保护继续通过。只改测试，无生产代码/XAML变更。

基线 e2353aa38e8c125d49ab1a1a37d67d37e1cbca8c，本机 SDK9.0.302/VSTest17.14.1（SDK8.0.423未安装）；全解方案 Release build 0/0，R10 preset 6/6、TaskCenterViewResponsive 8/8、Q14 toolbar 1/1，合计15/15，XAML24/24、source validation/diff check通过。Q14通过后出现testhost COM teardown诊断但exit0。SDK9不替代SDK8/CI SDK10.0.401。用户设置和真实Playnite未触碰。CI run待确认；下一项 R10-06 来源条件清除行为。证据：docs/design/reviews/ui-finesse-round3-20260915/evidence/R10-05-CURRENT-MAIN-RECHECK-20261015/README.md。


### 2026-10-15 CLOSE-WRAP SDK8 全量门禁与外部输入边界

当前 main b04660e0 的 WorkspaceTreeGovernanceTests 旧 Trainer/Media typed getter 断言改为 generic registry 路由；实际 PageHost/导航/焦点行为由 WPF 测试覆盖，定向 8/8。只改测试，不改生产 UI。隔离 SDK8.0.423/VSTest17.11.1 完整 Release solution build 0/0、XAML24/24、Core125/125、Worker356 passed/1 skipped、Playnite source455 passed/27 skipped。122 个 WPF 类已全部执行，648 passed/39 skipped/1 failed；Q14 654 DIP 行距1/1。

唯一失败 R05TooltipTimingBehaviorTests.QuickMousePassesCancelTooltipAndAStationaryDwellOpensIt 在窗口显示前 GetCursorPos 返回 false；该类2/3，余下78个类另行单类隔离均通过。总计1584 passed/67 skipped/1 failed，不把系统指针输入未用计为产品缺陷，也不将其改成跳过。证据 docs/design/reviews/ui-finesse-round3-20260915/evidence/CLOSE-WRAP-CURRENT-MAIN-RECHECK-20261015/README.md。推送后追踪SDK8/SDK10 CI；下一本机任务重查R00/R01 freshness，之后CLOSE-OPTICAL-01。

### 2026-10-15 R08 hosted motion test fixture

Actions #1024 (`38677622085`) 的 SDK8.0.423 与 SDK10.0.401 job logs 都证明 `R08MotionHotChangeBehaviorTests` 没启动 opacity entrance（`opacity=1; loaded=True`）。测试在 `Window.Show` 前设置时长，但初次 `ApplyAdaptiveTheme` 会基于真实 Windows 动效策略重写资源。夹具应在初次主题应用后注入测试时长；应用开关和系统策略仍分开由各自测试覆盖。此轮本机 SDK8.0.423 build `0/0`、两类行为 `1/1`、source validation/diff check 通过；不要把它当成新提交的 SDK10 CI 结果。证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/CI-R08-MOTION-POLICY-FIX-20261015/README.md`。

### 2026-10-15 R08 system-motion fixture follow-up

Actions #1025 (`38687483121`) 的 SDK10.0.401 日志指向 R08 system-motion 测试第56行没有检测到 opacity 动画时钟。仅等 `Show/UpdateLayout` 不保证 Loaded 队列已完成；测试现在先 drain dispatcher 并重新布局，再注入测试时长并触发被测入场动画。SDK10 artifact `11951470215` SHA-256 `7c328cf7b16c0d641dc0bdc61e6095d24e33b48ae69d0c9b4f1f5ddb812e4893`。同 run SDK8 job `116265550259` 报 R09HighContrast 第195行 Assert.True（Expected True / Actual False）；同身份本机 SDK8 3/3 未复现，先保留为未定位的独立 CI 失败，不将它归到 R08。artifact `11951095969` SHA-256 `8d928e1b58657bb058b243aeb96de0cf8e5906b5bed46a9ffc90606181dd34cd`。本机锁定 SDK8.0.423/VSTest17.11.1 build `0/0`，R08 system/app `1/1`、R09 high-contrast `3/3`；两个完整 CI log 已取得。新补丁推送后跟踪双 SDK Actions 至终态；其后按用户顺序重验 R00/R01。

### 2026-10-15 TaskCoordinator 取消 Gate 生命周期竞态

`TaskCoordinator.RunAsync` 从 `_taskRuntimes` 移除 runtime 时，另一个 `CancelAsync` 可能已读出同一个 runtime 并仍在等待/使用 `SemaphoreSlim Gate`。不可在移除时 Dispose 这个 Gate；让 semaphore 在并发调用释放引用后由 GC 回收。`CancellationRequested` 必须在终态判断前检查，使已接受取消的重复请求继续返回成功。CI #1026 SDK10.0.401 真实日志确认旧实现的 `ObjectDisposedException` 正是在 `CancelAsync` 的 `Release` 上抛出。修复后的本机 SDK8.0.423 目标回归 `1/1`、Worker `356 pass / 1 环境 skip`；新 SDK10 CI 仍待结果。

### 2026-10-15 R00/R01 freshness on 5862856e

`check-ui-evidence-freshness.ps1` 对 `5862856e508fdb0abf2478fbbc54c34b34075692` 的 14 条 R00/R01 记录给出 `14 fresh / 0 stale`，没有命中登记源路径，package identity 为 `not-provided`。该脚本只证明相关源路径未失效，不能证明重新构建、安装包或 Playnite 宿主。R00/R01 行为和负例证据此前已经满足；Worker `TaskCoordinator` 修复不触及登记范围，因此不重建功能。

### 2026-10-15 R09 high-contrast press fixture

Actions #1027 (`38698959702`) 与 #1028 (`38701408503`) 的 SDK8.0.423/SDK10.0.401 完整 job logs 都在 R09 high-contrast matrix 的合成 `RaiseSpaceKey` 路径后报告 bare `Assert.True` Expected True/Actual False；后续 `TextServicesContext` COM 异常也会在通过的本机测试退出时出现，不能当成该 xUnit 断言的根因。现用 `HighContrastStateProbeButton.SetPressedStateForProbe(true/false)` 驱动按钮模板状态，再检查 overlay、动画时钟与释放后的负例。生产样式不变。SDK8.0.423 本机 Release 测试项目 build 成功，R09 `3/3`、Q06 routed Space 状态类 `1/1`；物理键盘仍未验。证据与 CI artifact 元信息见 `evidence/CI-R09-PRESSED-STATE-RECHECK-20261015/README.md`。修正提交推送后必须确认 SDK8 与 SDK10.0.401 Actions 终态。

### 2026-10-15 R21 UIA selection CI follow-up

Actions #1029 两条 SDK lane 在 R21 DataGrid `ISelectionProvider.GetSelection()` 查询时都得到空集合；CI 没有记录 WPF SelectedItems 或行容器，故实际根因尚未证实。本机 SDK8.0.423 / .NET Framework 4.8.9181.0 对照通过。当前测试夹具在查询前显式滚动、布局并检查真实选中项和生成的 DataGridRow，同时保留 UIA 正向项数/名称断言。修改后 R21 `24/24`；尚待新提交 SDK8/SDK10.0.401 CI 终态。只改测试，不改生产 UI/绑定/命令。

`.tmp` 盘点约 1.09 GiB；约 714 MiB 为后续 SDK8 构建工具，约 374 MiB 为过期隔离输出。批量清理被自动审查拒绝，因为尚有未归档 TRX/日志且 dotnet PID `32408` 无法识别/确认占用；没有通过其他方式删除。D 盘当时剩余 `8.93 GiB`。旧输出只有在逐份确认/归档并消除进程占用风险后再清理；此事不阻断其他任务。

### 2026-10-16 R21 Selection peer 子树复核

Actions #1030/run `38712426496` 的 SDK8 与 SDK10.0.401 job 均在 R21 父级 `ISelectionProvider.GetSelection()` 返回空集合处失败；CI 已确认 SelectedItem、唯一 SelectedItems 和已实现 DataGridRow 前置条件通过，根因仍未知。新夹具先断言生产 `SelectionUnit.FullRow`，再枚举 grid peer 子树并用 `ISelectionItemProvider.IsSelected` 检查首行、旧行取消和新行选中。只改测试；本机隔离 SDK8.0.423、.NET Framework 4.8.9181.0 下 Release build `0/0`、R21 `24/24`。源码 SHA `E13552DA73AF1FE8D72715CE0D9A0C6D5FC1B36DB0564C359701C58226D77D5C`，脱敏 TRX SHA `E33A440A00897E7DC830CA35232B67B0441170F03D81643980E63242EC330550`。新修订待 push 后追踪 SDK8/SDK10.0.401 Actions 终态，证据见 `evidence/CI-R21-SELECTION-PEER-ENUMERATION-20261015/README.md`。真实 Playnite/Windows UIA、OS 输入、DPI、呈现与宿主性能未验。

先前的清理请求曾被自动审查拒绝；用户随后明确授权五个精确目标，已按授权范围清理并释放 `373.7 MiB`。当前 `.tmp` 约 `744.9 MiB`，保留 SDK8 工具链和 R21 复现目录；PID `32408` 仍无法识别。

### 2026-10-16 临时目录清理

收到用户对五个旧阶段目录的明确授权后，清理了 `.tmp/q24-07-official-postcommit-0243c108`、`.tmp/r11-06-40af39c8-20261015`、`.tmp/r09-current-main-retry-20261015`、`.tmp/r00-08-55ed3a17-20261015` 和 `.tmp/r08-01-production-button-sdk9-20261014/build`，共释放 `373.7 MiB`。没有删除 SDK、当前 R21 复现输出或其他任务文件；剩余 `.tmp` 约 `744.9 MiB`，D 盘可用 `9.26 GiB`。PID `32408` 仍无法识别，但删除范围严格限于用户明确指定授权的路径。

### 2026-10-16 R13-06 当前 main 与外机 R09 证据校准

R13-06 的 `NetworkRecoveryDisplay`、维护页绑定、Worker 轮询/批次与任务通知去重已存在；本轮只扩展生产 `MaintenanceView` STA 行为检查，实际选择网络失败项→恢复传输项→认证失败项，断言显示文字变化、退避计数和认证负例。SDK9.0.302/VSTest17.14.1 新隔离输出：Playnite R13 `14/14`、Worker 重试 `12/12`、Core 映射 `1/1`、通知去重 `6/6`、R09 `2/2`；不是完整 solution，也不替代锁定 SDK8.0.423 或 CI SDK10.0.401。`R13-06-CURRENT-MAIN-BEHAVIOR-RECHECK-20261016` 保存 TRX、源/DLL/PDB SHA；无生产源码改动、无真实网络/通知/Playnite。

用户新附的外机 R09 日志对应诊断 ID `20261016T052146719Z-d9d99ac4`、随机隔离构建目录 `gsc-b/fd7672ef`、116 个 WPF 类。其第151行的 `Thickness(2)` 失败断言属于旧测试形态；当前 main（包括 `983fee5`）第151行检查错误色、第152行 Chrome 1 DIP、第154行独立 ValidationStroke 2 DIP。本机当前源干净隔离运行 R09 `2/2`。新日志不含外机 HEAD/dirty paths/test source hash/DLL/PDB/plugin DLL hash；外机根因仍未知，不能把本机成功说成同机复现。详细取证命令见 `evidence/CI-R09-OTHER-MACHINE-FAILURE-RECHECK-20261014/README.md`。

当前可执行下一步：提交并推送本阶段小批，再追踪本阶段新 Actions 两条 SDK lane 至终态；同时继续核对仍在运行的 #1041/#1042/#1043，不把它们的部分状态合并。随后 R13-07 队列筛选/汇总。真实通知监视器、联网/远端、Playnite host、物理 DPI/跨屏、独立 OS UIA/IME、最终呈现、ETW/宿主性能仍未验。

### WPF 合成宿主的设置保存边界（2026-10-16）

当 STA 行为测试将未初始化 Playnite `Plugin` 注入 `DashboardViewModel`，但加载真实页面会启用带 DispatcherTimer 的列宽持久化控制器时，必须让测试的偏好保存 callback 落在 fake host 内；不得让此行为测试异步调用 `Plugin.SavePluginSettings`。生产默认 callback 保持不变。列宽控制器保存/恢复本身用独立合成设置测试覆盖。若另一台机器的断言行号和当前源码不一致，先从同次诊断 `summary.json` 对照 sourceCommit、dirtyPaths、source SHA、DLL hash/MVID，再判断旧 checkout 或 stale assembly；本机不同 SDK 的定向通过不能覆盖对方故障。

### 2026-10-16 R13-07 队列筛选与分页行为复核

R13-07 既有 Worker 查询与维护页控件、全局/筛选计数、分页一致性 token、稳定选择恢复已存在；不要另建服务/DTO。当前 main 只将分页追加/选中恢复小段逻辑抽为生产路径实际调用的 helper，去掉全重复页的无效 Reset，并用生产汇总格式 helper 验证当前/全局计数。Playnite `MaintenanceView` STA 真实操作五个合成绑定控件；Worker `CloudTransferStateTests` 用隔离状态验证过滤、全局汇总和过期 token。最终 SDK9.0.302/VSTest17.14.1 Playnite R13 `16/16`、Worker `14/14`；不是完整 solution build，不代替 SDK8.0.423/CI SDK10.0.401。TRX、SHA、freshness 见 `evidence/R13-07-CURRENT-MAIN-BEHAVIOR-RECHECK-20261016/README.md`。

WPF 测试若新增窗口到另一个 STA thread，`Application.Current` 可能保留已关闭 Dispatcher，影响之后视图；优先将真实控件交互合并到已有的隔离 STA fixture 并完整运行该类，不能只跑新 Fact。外机旧断言来源仍需同次 `summary.json`/程序集 SHA 才能确认，但不阻断其他主线任务。当前先追踪 #1044 和 R13-07 新 Actions 两条 SDK lane 至终态，再做 R13-08；真实插件 IPC refresh、Playnite host、物理 DPI、独立 UIA/IME、最终呈现、ETW/宿主性能仍属未验证边界。
