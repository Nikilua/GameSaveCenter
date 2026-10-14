# GameSaveCenter 当前事实入口

更新：2026-10-14；版本 0.6.73-development-preview；分支 main；本轮R10-05测试身份为 `GscBuildCommit=0b4e64a72e2ec6d282fd935745accec5544551f4`，更改源由复核包SHA固定。R00/R01 freshness 14/14；Q24-08 15/15；Q25-01 13/13；R08-05 44/44、R07-08 1/1。R09外机焦点失败当前main SDK9定向2/2未复现；外机HEAD/DLL/PDB未知。R10-02 SDK9 5/5、相邻25/25；#990 `38553608412` SDK8/SDK10 jobs失败，API logs 403。R10-03 12/12；R10-04 17/17。#991 `38556833749`已终态失败：SDK10 job `115827968247`、SDK8默认job `115827968644`均“编译与测试”exit1；job logs/artifact API 403，匿名下载404；artifact ID/哈希见交接文档。R10-05 SDK9.0.302定向5/5，响应式/工具栏相关合计14/14，validate-source与diff check通过；当前改动尚无远端CI结果。SDK9不替代SDK8.0.423/CI SDK10.0.401。用户至2026-10-15 09:00无法回复；完成当前阶段推送并追踪CI后继续R10-06。
本页只保留当前结论与下一步，不再累加阶段日志。9 月 30 日及以前完整原文移至 [历史快照](CURRENT_STATE_HISTORY_THROUGH_20260930.md)，旧记录中的“当前”“下一项”只代表当时状态。

## 当前判断

核心功能和六个工作区已实现，当前属于功能较完整、尚未完成发布验收的开发预览版。不能用测试数量、Round3 的“已满足”或旧评估的 95%/98% 推导正式稳定版完成度。详见 [完成度评估](../FEATURE_COMPLETION_ASSESSMENT.md)。

| 维度 | 当前可核查事实 |
| --- | --- |
| 最近完整 Release | clean source identity `4d166f017dcba8fbb07980ec21b690267b9ff377`、隔离 SDK `8.0.423`、`0 warning / 0 error`、XAML `24/24`；main `8e71feb4` 之后只有文档/证据变化，source/tests 不变 |
| 最近完整脚本测试 | clean source identity `4d166f0` 上 `1560 passed / 57 skipped / 0 failed`：Core `125/125`、Worker `357/357`、Playnite source `470 passed / 18 skipped`、114 个 WPF 隔离类 `608 passed / 39 skipped`。Named Pipe 环境 skip 与其余 `NotExecuted` 未计通过 |
| 当前实现阶段 | `CLOSE-PICKER-01` 本机代码和自动化门禁已实现；新增 STA Dispatcher 线程负例证明旧实现线程错误，删除回退/虚拟化全量通过。R09 测试已校正为检查独立 `ValidationStroke` 两 DIP 覆盖层，不错误断言 Chrome 改为两 DIP |
| R02-03 当前复验 | 生产实现未变；加强后的 STA 提示焦点/UIA peer行为与Save紧凑布局定向 `5/5`。本机 SDK `8.0.423` Release `0 warning / 0 error`、XAML `24/24`；WPF 静态审查 `0 errors/29 warnings/177 info`；RenderHarness 通用双主题 `render-qa OK`，未作为禁用提示截图证明 |
| R02-04 当前复验 | 当前 main Release `0/0`、XAML `24/24`；生产资源 STA 几何 `7/7`，16/20 DIP 图标间距各 `8 DIP`、中心差 `0`；Light/Dark RenderHarness `372` 图 `render-qa OK`，已人工对照并归档 Save 1040×700 两主题截图。未改生产 UI |
| R02-05 当前复验 | 当前 main Release `0/0`、XAML `24/24`；生产资源 STA 命中/间距/82 DIP 换行 `2/2`；Light/Dark RenderHarness `372` 图 `render-qa OK`，已检查并归档 Media 1040×700 两主题截图。未改生产 UI |
| R03-01 当前复核 | main `8e71feb4`；RenderHarness Release `0 warning / 0 error`、`TypographyDiagnosticsTests 11/11`、双主题 `finesseprobe` exit 0；中文/英文/数字/𠮷/组合重音均有实际 GlyphRun。报告 `WorkingTreeClean=False` 是证据目录已在工作树的真实记录；自 `4d166f0` 以来 src/tests 无变化。截图/报告与 identity negative 见[R03-01 当前复核包](../design/reviews/ui-finesse-round3-20260915/evidence/R03-01-CURRENT-MAIN-RECHECK-20261012/README.md) |
| R04-02 当前复核 | main 源码/测试身份 `32c0e1e2`；Playnite net472 定向行为 `5/5`，覆盖摘要链接跨 Tab 后字段可见、键盘焦点、HelpText/滚动，以及禁用巡检字段旧非法值和路径负例。结构检查仅作补充；没有执行真实 Playnite 设置保存/Worker 应用。详见[R04-02 当前复核包](../design/reviews/ui-finesse-round3-20260915/evidence/R04-02-CURRENT-MAIN-RECHECK-20261012/README.md) |
| R04-03 当前复核 | `e952c414` 工作树上的插件 net462 / 测试 net472 定向构建，隔离 SDK `8.0.423`，`SettingsDraftLifecycleBehaviorTests + PortableSettingsTests + SettingsValidationSourceTests` 为 `12/12`；关闭事件实际选择“否”保留草稿并恢复焦点、“是”后才关闭/回滚。真实 Playnite 原生确认框、宿主保存和 Worker 应用仍待验。详见[R04-03 当前 main 补充证据](../design/reviews/ui-finesse-round3-20260915/evidence/R04-03-CURRENT-MAIN-RECHECK-20261012/README.md) |
| R05-01/02 当前 main | commit `1c39e909856faa736b32224ee196ef0bac6ca460`，隔离 SDK `8.0.423`，Playnite 插件 `net462`、测试 `net472`；R05Focus/R05Option/R21 定向合计 `11/11`。TRX SHA/MVID、插件 SHA/MVID 已归档。Dashboard 受控 STA fixture 覆盖焦点进入、Tab 环绕、即时/动画关闭和取消回焦；XML 解析断言核对真实 XAML Click 接线。完整 Dashboard XAML/Playnite 宿主未加载；VSTest 收尾有三条 TextServicesHost `InvalidComObjectException` 消息但 exit0，原因未知 |
| R05-03 当前 main | 同一 commit 和隔离 SDK 下 `R05PopupBoundaryBehaviorTests + R05OptionVirtualizationBehaviorTests 7/7`。短窗 Picker 边界、当前桌面 ComboBox Popup 翻转后的工作区几何/MaxHeight/滚动/选定项可见通过。隔离 Playnite bootstrap 于插件安装前因 graceful shutdown 未完成而被保护策略阻止，未取得 popup/呈现证据；“翻转无闪屏”、420×220 负例、真实 Playnite/多屏仍未验 |
| R05-04 当前工作树 | 基于 main `903525be`，测试源 SHA `B6D804AD...`；本机 SDK `8.0.423` Release solution `0/0`，`R05MultiSelectionSummaryBehaviorTests 3/3`。验证 0/1/2 摘要、隐藏 ID 例外、清空“已忽略”选择同时保留“待归类”选择并保持模式；未改生产视觉。身份/边界见当前复核包 |
| R05-05 当前 main | `fbf504d2a7aabcd322c3c735959a31c7e1679fc9`；媒体批量选择用 Extended DataGrid + `SelectedItems`；唯一显式三态样例在开发校对页。XAML `24/24`、Release `0/0`、源契约 `24/24`；源测试为字符串断言，不覆盖 Space/UIA/集合行为。维持不适用，见[复核包](../design/reviews/ui-finesse-round3-20260915/evidence/R05-05-CURRENT-MAIN-RECHECK-20261013/README.md) |
| R05-06 当前 main | 生产基线 `0fcb44f8`，只补同值重复赋值负例；隔离 SDK `8.0.423`、官方 Release build `0/0`、XAML `24/24`，定向 `28/28`。当前行为、TRX 与源码/程序集身份见[复核包](../design/reviews/ui-finesse-round3-20260915/evidence/R05-06-CURRENT-MAIN-RECHECK-20261013/README.md) |
| R05-07 当前 main | 生产 Tooltip 行为未改；测试夹具在 main 4572118f 置顶/激活临时窗口，并先确认 WPF hover 再计驻留。本机 SDK 8.0.423 Release 0/0、XAML 24/24；Tooltip 2/2 两轮。#952 于“编译与测试”失败，12m07s；job 日志要求登录，artifact 11774606363（556 KB，SHA-256 9686cc633488c860c9dd8021690986abfff69129f1420ced7b433ff062490a3a）内容不可读，实际 runner SDK/根因未知；不凭摘要盲改。详情见[复核包](../design/reviews/ui-finesse-round3-20260915/evidence/R05-07-CURRENT-MAIN-RECHECK-20261013/README.md) |
| R06-01 当前 main | 同一 `afade3cd` 最新 main 重建 SDK `8.0.423` Release `0/0`、XAML `24/24`；`R06ColumnWidthPersistenceBehaviorTests 6/6`，含 Task 列键/生产列定义一致性；当前代码和程序集 SHA、TRX 见[复核包](../design/reviews/ui-finesse-round3-20260915/evidence/R06-01-CURRENT-MAIN-RECHECK-20261013/README.md)。旧双主题截图不绑定当前 commit |
| R06-02 当前 main | `90454586` 隔离 SDK `8.0.423` Release `0/0`、XAML `24/24`；`R06SortingBehaviorTests 7/7`。实际 WPF 列头双向点击/箭头、乱序重加稳定次键、未知值和失效视图负例见[复核包](../design/reviews/ui-finesse-round3-20260915/evidence/R06-02-CURRENT-MAIN-RECHECK-20261013/README.md)。历史 RenderHarness 图绑定 `c577afc5`，未当作当前截图 |
| R06-03 当前 main | 生产实现沿用 `d83c7378`；本轮以 `fdeeec8` 生产源加单项测试差异构建，SDK `8.0.423` Release `0/0`、XAML `24/24`；R06Selection `2/2`、四表双主题状态几何 `1/1`、Task 双主题受控 hover/错误徽章 InputHitTest/Failed 终态禁用与 Running 取消按钮 UIA Invoke `1/1`、共享资源契约 `2/2`。测试仅证明隔离 WPF 状态与逻辑 DIP，不是实际 OS 指针或 Playnite 呈现。详情见[复核包](../design/reviews/ui-finesse-round3-20260915/evidence/R06-03-CURRENT-MAIN-RECHECK-20261013/README.md) |
| R06-04 当前 main | 复用 afe4aa55 已有 DataGridClipboardBehavior 和五类 profile；生产行为复核 5/5，复制反馈 2/2。本机 SDK 8.0.423 Release 0/0、XAML 24/24，Q14 1/1、侧栏 22/22、R06Clipboard 5/5、R22 2/2。#952 完整日志显示其失败发生在旧 R07 resize 初始断言；本轮修正尚无 CI 结果，R06-04 不因本机定向通过签收。详情见[当前复核包](../design/reviews/ui-finesse-round3-20260915/evidence/R06-04-CURRENT-MAIN-RECHECK-20261013/README.md) |
| R06-05 当前 main | 复用现有共享列头说明、排序与单位格式；本机 SDK `8.0.423` Release `0/0`、XAML `24/24`、`R06ColumnHeaderHelpTests 2/2`。当前身份 RenderHarness `372 PNG`、`render-qa OK`；代表图和 header contract 见[复核包](../design/reviews/ui-finesse-round3-20260915/evidence/R06-05-CURRENT-MAIN-RECHECK-20261013/README.md)。本轮未改生产 UI；受控 STA 与离屏逻辑 DIP 不等价真实 Playnite/OS 输入 |
| R06-06 当前 main | 复用 `TaskStatusDto`、`TaskIndexedCollection`、现有事件合批与 `TaskSummary`；SDK `8.0.423` Release `0/0`、XAML `24/24`，Playnite 隔离 `34/34`、Worker 合成 SQLite `9/9`；真实 DataGrid 验证 80 行更新只发生 `Replace`、选择/行数/滚动偏移保持，未知/0/钳制进度、取消状态与服务端分页总数各有行为证据。TRX 哈希/程序集身份见[R06-06 当前 main 复核包](../design/reviews/ui-finesse-round3-20260915/evidence/R06-06-CURRENT-MAIN-RECHECK-20261013/README.md)。本轮无当前身份截图，旧图仍绑定 `72fd6d9a`。混合非隔离试次 `22/23` 的单一失败在滚动 viewer 查找，已披露且未混入通过数。#961 是旧 run；当前代码检查点 #968 的 SDK8/SDK10 全门禁已成功；真实 Playnite/取消竞争、UIA、物理跨屏、presented frame、ETW 和宿主性能未验 |
| R07-08 / R08 当前 main | #959 CI 的 R07 resize `1/1` 通过，R08 fixture 的系统动效前提已在 `d60e472d` 修正；本机 SDK `8.0.423` Release `0/0`、XAML `24/24`、R07 `1/1`、R08 `2/2`、native motion policy `22/22`。#960/run `38327521241` 已确认 terminal failure、job `115069541561` “编译与测试” exit 1；完整日志 endpoint 404/API 403，SDK/根因未知。#968 当前源码自动门禁成功；真实宿主仍待验。历史日志/证据见[证据包](../design/reviews/ui-finesse-round3-20260915/evidence/CI-R07-08-RESIZE-FAILURE-20261013/README.md) |
| Q23-08 当前 main | 复用现有 ResetCatalog、导入预览/确认、Playnite Save/Cancel；SDK `8.0.423` 本机 build `0/0`、行为 `12/12`、XAML `24/24`、source validation 通过；Light/Dark × `1040×700`/`720×640 DIP` 的四场景动作可达性通过。视觉栏通过，宿主外部阻塞、最终未完成。证据见[复核包](../design/reviews/ui-finesse-round3-20260915/evidence/Q23-08-CURRENT-MAIN-RECHECK-20261013/README.md) |
| Q24-01 当前 main | `3ca41ec9` 新增受控分数 DPI 行为验证；SDK `8.0.423`，Release 定向测试 `1/1`。生产资源字体/图标/1 DIP 描边在 RenderTargetBitmap 的 96/120/144/168/192 DPI 目标栅格可见，负例覆盖裁切和永久缩放；受控离屏视觉通过，物理 DPI/Playnite Host 仍待验，最终未完成。详见[证据包](../design/reviews/ui-finesse-round3-20260915/evidence/Q24-01-CURRENT-MAIN-LOGICAL-DPI-RECHECK-20261013/README.md) |
| Q24-02 当前 main | `5fcd6856` 记录生产 PageHost 实际宽高；`74815b2b` 修复 Tooltip/Popup roots 跨 Dispatcher 遍历并增加双 STA 回归。SDK `8.0.423` 定向 `9/9`，覆盖 PageHost 窄/短/宽恢复、2,000 条合成收件箱首尾可达/虚拟化及 Tooltip 行为；扩展 Popup/焦点组 `8/16`，失败边界未验收。真实 Playnite、物理跨屏和最终呈现未验。详见[证据包](../design/reviews/ui-finesse-round3-20260915/evidence/Q24-02-CURRENT-MAIN-PAGEHOST-RECHECK-20261014/README.md) |
| Q24-04 当前 main | `87070909` 加强 R21 键盘轨迹测试；SDK `8.0.423`、Playnite `net462`/tests `net472` 定向 `8/8`。八入口 90 个当前可见交互控件双向可达、反向顺序互逆，选框行为 6/6。没有完整业务行态或真实 Playnite/OS 键盘；视觉/宿主/最终仍未验。详见[证据包](../design/reviews/ui-finesse-round3-20260915/evidence/Q24-04-CURRENT-MAIN-KEYBOARD-RECHECK-20261014/README.md) |
| Q24-05 当前 main | 复用 b20f1a4b 及现有状态模板；隔离 SDK 8.0.423 Release 构建 0/0，焦点/选中/当前态与选框关闭回焦行为 7/7。选中与键盘焦点区分、双主题及禁用/失焦负例通过；真实 Playnite、OS UIA/读屏、物理焦点像素仍待验。详见[Q24-05 证据包](../design/reviews/ui-finesse-round3-20260915/evidence/Q24-05-CURRENT-MAIN-FOCUS-RECHECK-20261014/README.md) |
| Q24-06 当前 main | `af8ed430` 更新 11 个生产 DataGrid 的 Name/HelpText；进程内 peer 验证 Grid/Table/Selection pattern 与选中行变化，Task 图标按钮名称、进度 Off live setting/焦点负例通过。Release 0/0，R21Automation 24/24，R00-06 freshness 行为 13/13，freshness 14/14。实际 Narrator/Playnite/OS UIA 未验，因此代码完成待验收。详见[Q24-06 证据包](../design/reviews/ui-finesse-round3-20260915/evidence/Q24-06-CURRENT-MAIN-UI-AUTOMATION-20261014/README.md) |
| Q24-07 当前 main | `0243c108` 将高对比与有效动效状态沿视觉树继承；系统语义色、禁用/焦点/选中/Popup、活动动画时钟清理与恢复行为通过。SDK `8.0.423`，Release `0/0`，XAML `24/24`，WPF `13/13`。代码待 CI/外部验收。真实 Playnite、Windows 高对比切换、OS 键鼠/UIA/Narrator、物理 DPI/跨屏和最终呈现未验。R05 Tooltip 相邻类 `2/3` 的失败在 GetCursorPos 前置阶段，TRX 单列。详见[Q24-07 证据包](../design/reviews/ui-finesse-round3-20260915/evidence/Q24-07-CURRENT-MAIN-HIGH-CONTRAST-RECHECK-20261014/README.md) |
| Q24-08 当前 main | 复用现有长文本、TextComposition、键盘与复制实现；本机 SDK `8.0.423` Release `0/0`、XAML `24/24`、定向 `15/15`。双主题长标签/AutomationPeer/Space command、合成中文 composition 和 82 DIP 长值复制负例通过；无生产 UI 改动。真实 Windows IME、Playnite/Narrator、系统剪贴板及运行时 Tooltip 仍待验。详见[Q24-08 证据包](../design/reviews/ui-finesse-round3-20260915/evidence/Q24-08-CURRENT-MAIN-LONG-TEXT-ASSISTIVE-INPUT-RECHECK-20261014/README.md) |
| Q25-01 当前 main | 复用既有搜索/可见列表实现；SDK `8.0.423` Release `0/0`、XAML `24/24`、4 类 `13/13`。2000项5次预热+30个不同查询 p50/p95/max=`45/60/60ms`，受控 Window 20次输入到可见容器=`46.214/75.782/76.334ms`，GamePicker 键盘路径6/6。受控行为可满足；不等于真实 Playnite/物理输入/呈现帧。详见[Q25-01 证据包](../design/reviews/ui-finesse-round3-20260915/evidence/Q25-01-CURRENT-MAIN-HOT-RESPONSE-RECHECK-20261014/README.md) |
| 远端 CI | #970/run `38386326043` SDK10 `10.0.401` Worker cancellation test race failed；SDK8 `8.0.423` 两次 solution-build passed；测试夹具修正在 `599d2fd3`。#971/run `38393317198` 公开状态仍 In progress；用户所附 SDK8 diagnostics 在 `5a6647c` 记录 Q14 fixture failure，`599d2fd3` 已修，未提供 SDK10 lane 日志。#972/run `38421480496` 两 lane 成功但对应更早提交。#978/run `38509928015` 两命名 lane 均失败；完整日志需登录，实际 SDK/根因未知。#979（`ae3c565`）仍 queued；当前 main `31d64e42` 已推送；最新公开 Actions 检查仍只显示 #979 queued，未见该提交对应的新 run。SDK9 本机定向复核不替代 SDK8/CI SDK10.0.401。 |
| R00/R01 证据 | 最新已签 baseline `599d2fd` freshness 为 `14 fresh / 0 stale`；R00-01/02、R00-05 定向 `6/6`，双主题 finesseprobe 各 88 状态样本、0 违规；当时受影响完整类 `154 passed / 39 skipped / 0 failed`。当前 `0243c108` 因共享主题/动效资源改变，freshness 初检有 4 项待复验：R00-01-02、R00-03、R00-05、R01-05；未复验前不沿用旧基线。旧 audit 为 `168 snapshots / 116 warnings / 0 HIGH / 13 MEDIUM / 0 Fidelity / 0 failed routes`、E01–E20 `20/20`。物理 DPI/跨屏、OS IME/Playnite 输入、package identity仍待验。 |
| 真实宿主 | 未启动真实 Playnite；Media Inbox 同进程滚动、物理 DPI/跨屏、IME、Narrator/UIA、ETW 与最终呈现帧均未验，`MANUAL QA REQUIRED` |

仓库锁定 SDK `8.0.423` 已由官方安装脚本安装到 `D:\GameSaveCenterTools\dotnet-sdk-8.0.423`；通过临时进程 PATH 前缀选择，系统 SDK/PATH 未改。历史 SDK/CI/IPC 诊断见[SDK阶段证据](evidence/close-sdk-20261007/README.md)、[CI证据](evidence/close-ci-20261007/README.md)和[IPC证据](evidence/close-ipc-20261005/README.md)。R05 新确定性负例与修复正例已记录；旧的第三轮失败保留为归因证据，不能用旧clean DLL独立通过覆盖。run930、run931、run932、#934、#935 与 #936 的失败原因均未取得完整日志；访问受限期间不盲修。

## 当前执行顺序

当前用户明确续跑 Round3 192 项任务，并在 2026-10-15 09:00 前无法回复；不因缺少补充 ZIP 停住独立工作。

1. #970/#971 新附件 SHA-256 与已审包相同；对应问题已在先前提交分别修正，#971 SDK10 原始诊断仍缺但非独立任务前置。见[诊断记录](../design/reviews/ui-finesse-round3-20260915/evidence/CI-RUN-970-971-TEST-RECHECK-20261013/README.md)。
2. R00/R01 当前受控证据已在 `ae8f3be9` 上复核，freshness `14 fresh / 0 stale`；本机 SDK `8.0.423` Release `0/0`、5 类 `163 passed / 39 skipped / 0 failed`。新证据包已归档。
3. #978/run `38509928015` 两 lane 终态失败。SDK8 与 SDK10.0.401 命名 job 都在编译/测试步骤失败；日志需登录，根因和实际 runner SDK 未核实。不凭通用 exit code 猜修；本机 SDK8 不替代 SDK10。该远端门禁仍开放，其他独立 Q/R 工作继续。
4. Q24-08 当前 main 受控行为 `15/15`；Q25-01 当前 main `13/13`，热搜索/受控可见容器数据已记账。R08-01 按钮反向 `3/3`，R09 外机失败日志第 151 行对应历史 Chrome 2 DIP 断言，当前 main 该行已是颜色检查、R09 `2/2`；外机仍缺 HEAD/DLL/PDB。R08-02 应用热关闭 `1/1`、系统策略回调受控模拟 `1/1`、侧栏 reduced-motion `2/2`、订阅/卸载门禁 `1/1`。R08-03 当前 main 隐藏 Tab/最小化停机实测 `1/1`；R08-04 本机 SDK9.0.302 Release `0/0`、忙态/反馈 `9/9`，成功慢任务保持忙态至 action 结束，三类终态详情精确匹配；下一项 R08-05。真实 Windows 设置通知、Playnite、ETW/宿主性能仍待验。CI 公开页暂未显示 R08 最新提交的 run；#971 In progress、#978 Failure 且完整日志需登录、#979 queued，推送后继续跟 SDK8/SDK10.0.401 到终态。Demo 原目录不可用，沿用恢复生产基线；Q24-03 第二显示器、真实高对比开关仍待验。
## 架构与生产 UI

- Playnite 插件 `net462`，Core/Contracts `netstandard2.0`，Worker `net8.0-windows10.0.19041.0`，Playnite 测试 `net472`。插件与 Worker 经 Named Pipe 协作，SQLite 由 Worker 持久化。
- 生产外壳为 `AcrylicProductionShellView`；局部共享入口 `AcrylicProductionResources.xaml`，配合 `DesignTokens.xaml`、`WpfUiProduction.xaml`、`Redesign.xaml` 等资源。六工作区：Overview、Save、Trainer、Media、Task、Maintenance；另有 Settings。
- Demo-first 唯一主要视觉基准仍为 `GameSaveCenter.AcrylicFork/src/GameSaveCenter.Playnite/Design/`；该目录当前工作区不存在。按既有授权沿用已恢复的生产基线，不伪造 Demo 对比结果。WPF 技能只负责质量检查。
- 游戏状态/排序选择沿用已修复的 `DropDownClosed` 提交路径；动态选项、用户选择、真实命令、取消/错误与滚动/虚拟化契约必须保留。页面结构可按当前 AGENTS 授权重构，不沿用旧历史记录中的页面冻结禁令。
- `DashboardViewModel` 主文件约 6480 行，已存在 16 个 partial 文件；这是维护风险，不是未实现功能或已测出的性能瓶颈。

## 验证与交付规则

- 行为矩阵入口 `scripts/e01-behavior-matrix.ps1`；构建 `scripts/build.ps1 -Configuration Release`，可用 `-OutputRoot` 隔离；XAML/source/freshness/RenderHarness 各自只证明其覆盖范围。
- 不把离屏 logical DIP、合成 WPF、进程内 UIA provider 当成 Playnite、物理 DPI、OS 输入或最终呈现帧。
- 每个独立阶段同步文档、记录构建/测试实际结果，单独 commit 并 push 当前远端分支；本次不安装到用户真实 Extensions，不操作真实存档、媒体或云端。
- 后续依次阅读 [PROJECT_MEMORY.md](PROJECT_MEMORY.md)、[WORKLOG.md](WORKLOG.md)、[DEVELOPMENT_HANDOFF.md](../DEVELOPMENT_HANDOFF.md) 与 Git 状态。只在追溯具体问题时读历史大文件。

## 历史：2026-10-13 SDK 10.0.401 CI 门禁实现（后续由 #968 收口）

- #965 用户诊断包确认 CI SDK `8.0.423`；布局类 54 项中 4 项失败，均已在 `5eaa12bf` 以显式逻辑 Grid 宿主修正。#966/run `38352437420` 对该提交已终态成功；其 SDK 仍为 `8.0.423`。
- 已新增 SDK `10.0.401` 独立 Actions job 和隔离 worktree 验证脚本。仓库 `global.json` 与原 SDK8 job 保持不变。本机 SDK `9.0.302` 全量 Release 编译成功；Core `125/125`、Worker `357/357`、Playnite `1566` 通过/0失败；另有 57 条旧 UI 架构断言以既有规则 NotExecuted。布局类 `54/54`。
- 该门禁后来在 #968 对 SDK8 `8.0.423` 与 SDK10 `10.0.401` 两个 job 验证成功；#967 SDK8 源码结构检查的 BOM 修正随 `4aa1ec7` 提交。此处 SDK9 本机记录只作历史交叉检查；当前执行顺序见本页上方。
- 保留其他任务 R04/R05/R07 未跟踪证据。真实 Playnite、OS UIA/IME、物理 DPI/跨屏、presented frame、ETW及宿主性能未验；Demo 原目录不可用，沿用恢复的生产基线。
