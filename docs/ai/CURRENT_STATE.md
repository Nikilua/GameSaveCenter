# GameSaveCenter 当前事实入口

更新时间：2026-10-21。产品版本 `0.6.73`。旧记录已归档，本文仅保留当前身份、活动事项、硬性边界与证据索引。

Round4 S05-03 至 S05-07 已验收；下一步按依赖审计 S05-08 情景性能验证。SDK8.0.423 未安装，本轮 SDK9.0.302 Release build/定向测试通过；保留用户原有未跟踪 TRX/TestResults。

## 2026-10-20 用户反馈：页面滚动区域回归修复

存档历史/路径候选、任务队列与详情区域曾以旧布局测量值固定整页高度，造成内容被裁切而内层滚动仍可操作。现在整页根布局自然测量，PageHost 外层承担额外滚动，表格和详情保留有限内部视口；SDK8 Release solution `0 warning / 0 error`，几何回归 `5/5`，静态契约 `4 pass / 1 historical skip`。证据：[页面滚动修复](../design/reviews/UI-SCROLL-LAYOUT-REPAIR-20261020/README.md)。测试为 STA/离屏 WPF；真实 Playnite 窗口和物理 DPI 未验证。后续已完成 S05-03 至 S05-05。

## 2026-10-21 Round4 S05-03 至 S05-05 已验收

S05-03 实现提交 `e6279b7d` 将历史逻辑增长表达为样本覆盖内的观察范围或未知，分开显示用户假设、本地卷容量与未知远端配额，不声称置信区间或耗尽日期；SDK8 Release solution `0/0`、Worker `5/5`、生产维护页 Light/Dark `2/2`。证据：[S05-03](../design/reviews/ui-finesse-round4-20261018/evidence/S05-03/README.md)。

S05-04 实现提交 `cf50643c` 为本地存档、第二镜像、恢复 staging、媒体目录提供独立只读预算分项，区分目录文件长度和 SQLite 逻辑量，空目录/未知/未启用分别显示，不生成总计并警告重叠路径。精确提交 SDK8.0.423 Release solution `0/0`、Worker `8/8`、生产维护页双主题与导航 `4/4`。证据：[S05-04](../design/reviews/ui-finesse-round4-20261018/evidence/S05-04/README.md)。目录文件长度不表示卷簇实占。

S05-05 实现提交 `2fd0aa04` 将策略总量差异下钻到逐游戏和变化版本，并展示各保护原因；只对通过既有路径安全检查的变化候选做只读归档存在性检查，不创建清理句柄。精确提交 SDK8.0.423 Release solution `0/0`、Worker `2/2`、生产维护页 Light/Dark 双层滚动 `2/2`，RenderHarness 无问题。证据：[S05-05](../design/reviews/ui-finesse-round4-20261018/evidence/S05-05/README.md)。真实 Playnite、完整 WPF 隔离循环、物理 DPI 和宿主滚动未验。随后 S05-06 与 S05-07 已验收，当前下一步审计 S05-08。

## 2026-10-21 Round4 S05-07 转交策略编辑已受控验收

实现提交 `6807ae84` 在维护中心保留策略卡新增“转交策略编辑”。该命令复用现有 `CreatePolicyTemplate`，将选中且未修改的命名情景转入未保存模板草稿，只覆盖四项保留期限；增长范围仍是比较假设，自动恢复安全约束和其他策略字段沿用模板克隆逻辑。遇到未保存情景、模板或游戏策略修改时拒绝覆盖。转交清空旧比较、切到存档中心备份策略页，不保存、不应用、不清理。后续策略应用沿用现有差异/确认，清理需要新的 Worker 预览并实时复核。

SDK9.0.302 Release solution `0/0`、生产 MaintenanceView Light/Dark 与策略映射测试 `4/4`、XAML `25/25`、source validation 通过。证据：[S05-07](../design/reviews/ui-finesse-round4-20261018/evidence/S05-07/README.md)。当前机器缺少 global.json 锁定的 SDK8.0.423；真实 Playnite、完整 WPF 隔离循环、物理 DPI/CEF 未验。测试数据为合成，未触发 CI；用户旧 Round3 TRX/TestResults 保留。
## 2026-10-21 Round4 S05-06 保存情景草稿已验收

实现提交 `313fabc1` 为保留情景增加有界、版本化的标量设置持久化，以及选择载入、保存/更新、复制、重命名、删除和最近快照时间。草稿与生效策略分离；比较结果不缓存，后续会重新读取当前版本快照。SDK8.0.423 Release solution `0/0`、XAML `25/25`、定向测试 `13/13`、Light/Dark RenderHarness `0 PROBLEM`、source validation 通过。证据：[S05-06](../design/reviews/ui-finesse-round4-20261018/evidence/S05-06/README.md)。真实 Playnite、完整 WPF 隔离循环、物理 DPI/CEF 未验；原 Round3 TRX/TestResults 保留。随后 S05-07 已验收，下一项审计 S05-08。

## Round4 前序小批：S05-02 已验收

实现提交 `5f1431f2` 在维护中心“保留策略”页加入当前策略与会话候选策略的只读对比。审计时确认旧 `PreviewRetentionSimulation` 仅规划当前生效策略，`StorageAnalysisService` 的历史趋势也不接收候选规则。现在 Worker 单条 SQLite 查询读取版本及逐游戏策略，两侧共用该快照与 UTC 截止时点，并复用 `RetentionPlanner`；候选规则统一应用于全部游戏。增长范围作为用户假设回显，不参与保留计数。该结果没有清理预览 ID 或 Apply 通路。

SDK `8.0.423` 精确提交 Release 一键隔离构建/Worker 发布/no-start 安装 `0 warning / 0 error`，Worker 保留模拟器对账 `14/14`，生产 WPF Light/Dark 页面行为 `2/2`，双主题 RenderHarness `render-qa OK / 0 PROBLEM`。受控证据见 [S05-02](../design/reviews/ui-finesse-round4-20261018/evidence/S05-02/README.md)，提交 DLL SHA 与隔离安装副本一致。真实 Playnite 页面、完整 WPF 隔离循环和物理 DPI 未验。随后 S05-03、S05-04 已完成；旧 Round3 TRX/TestResults 保留。

## 当前最高优先级：默认一键构建、测试、安装与启动已全链通过

用户报告的 `DEV-INSTALL-008` 在另一台只装 SDK `9.0.302` 的机器上因 `global.json` 精确锁定 `8.0.423` 而在编译前中止。当前 `DEV-INSTALL-009` 保留精确版本要求，缺少时先通过微软官方安装脚本准备当前用户 SDK；之前也在仅暴露 SDK9 的环境中验证了自动准备/选择 SDK8。2026-10-20 本机从标准入口 `GameSaveCenter-Run.cmd` 完成默认全流程，退出码 `0`：SDK `8.0.423`、Release solution `0 warning / 0 error`、Core `157/157`、Worker `383/383`、Playnite 总计 `1812` 项（`1754` 通过、`58` 跳过、0 失败/错误/超时/中断），WPF 隔离类 `137/137` 全部完成。TRX 计数校验一致，`140` 份结果无解析或计数差异；R09 `2/2`、R10 `5/5`。

根因与修复：逐类 WPF 测试的 shadow-copy 临时目录此前不断累积并耗尽 D 盘；现在每个成功类只清理由本次测试在隔离 TEMP 中新增的副本。另修复帮助 Popup 未关闭导致的 WPF 退出挂起，并加入 2 分钟 hang 诊断。输入路由不可用的桌面会跳过依赖系统指针的单项用例，不再误报为产品断言失败。

Worker 自包含 win-x64 发布、`0.6.73` 包/PEXT、Playnite 扩展安装与启动均完成；安装清单 `0.6.73`、插件 DLL `0.6.73.0`，构建与安装 DLL SHA-256 相同（`AC87FDC16F4FF7211CC00C6B0B8135711D32D20F6C338A36F09B086972535A31`）。Playnite PID `11068` 已启动且响应。运行提交 `f70b806e21277f257f5b7b0f3eb7ae572cc66768`，诊断 `20261019T184725162Z-607fe456`，详细记录见 [一键全链验收证据](../design/reviews/DEV-INSTALL-009-20261020/README.md)。首次在新机器准备 SDK 仍需访问微软下载源；未检查真实 Playnite 页面视觉呈现。

## Round4 当前小批已验收：S04-06 至 S04-08

S00-01 已满足；S00-02 的 WPF/安装门禁仍停在 R05 系统指针权限读取。S04-03 复用 `f51e5c9e` 同步缩放/平移能力。S04-04 `9e0af12b` 为媒体比较增加擦除分隔线、A 左/B 右、键盘调整和透明棋盘背景，明确仅用于人工视觉比较；复用现有异步图像源。SDK8.0.423 Release solution `0/0`，定向生产 WPF 行为 `2/2`，源码与 XAML 校验通过。受控图、TRX、程序集身份和边界见 [S04-04 证据](../design/reviews/ui-finesse-round4-20261018/evidence/S04-04/README.md)。

S04-05 提交 `4d0bfd3c` 锁定左侧 A，仅从当前已加载的 `MediaView` 搜索/筛选结果中切换右侧 B；坏图跳过，沿用缩略图取消与代际保护。S04-06 `792b1a2f` 加入会话内可撤销意见；S04-07 `14925e5e` 将稳定 ID 快照转交既有预览并复核；S04-08 `c9da7c76` 验证 GIF、非支持格式和卸载时迟到回调，复用现有 2048 px/64 MiB/3 路解码资源预算。S04-08 SDK8.0.423 Release solution `0/0`、缩略图与生产比较页行为 `19/19`。各项证据见 [S04-05](../design/reviews/ui-finesse-round4-20261018/evidence/S04-05/README.md)、[S04-06](../design/reviews/ui-finesse-round4-20261018/evidence/S04-06/README.md)、[S04-07](../design/reviews/ui-finesse-round4-20261018/evidence/S04-07/README.md)、[S04-08](../design/reviews/ui-finesse-round4-20261018/evidence/S04-08/README.md)。真实 Playnite、真实媒体、物理 DPI 和完整 WPF 隔离循环未验。

S04-06 已核对现有收藏/备注与正式忽略/归类行为，均不等价于比较会话中的人工意见标记；已加入 READY。建议以会话内可撤销方式实现，不跨重启保存。

## 2026-10-20 用户反馈：页面与表格滚动区

存档历史、候选路径、任务队列和当前游戏媒体页使用 PageHost 实际 `ViewportHeight` 分配表格/列表空间；页面不足时由外层滚动，表格保留内部虚拟化滚动，详情仍可到达。用户反馈后的二次布局调整修复了大表格把滚动条挤出详情区域、下方内容被遮挡的问题。提交 `7242fb6b`；SDK8 Release solution `0/0`，最终几何回归 `6/6`，证据和合成截图见[滚动区修复记录](../design/reviews/UI-SCROLL-VIEWPORT-20261020/README.md)。测试为 STA/离屏 WPF，不代表真实 Playnite 或物理 DPI；媒体收件箱布局保持既有算法。

## 当前构建优先项：缺 SDK 时自动准备准确版本

`global.json` 仍精确锁定 .NET SDK `8.0.423`。`DEV-INSTALL-009` 在 Playnite 关闭前检查/准备此版本；若缺失，通过官方安装脚本写入当前用户 LocalAppData，SDK9 不会替代；下载失败则保留 Playnite 运行状态。补上 Windows PowerShell 5.1 对 `dotnet` 预期 stderr 退出的捕获后，只有系统 SDK `9.0.302` 的本机已自动安装并选中 `8.0.423`，SDK 基线自测通过（诊断 `20261019T014732824Z-823abe5a`）。干净提交的 `-SkipTests` 一键编译、Worker 发布、真实用户扩展目录安装和 Playnite/Worker 启动通过；默认全测试一键流程仍在复核中。先前工作树 SDK8 Release build `0/0`，Core `157/157`、Worker `382/1 skip`、Playnite 非 WPF `487/27 skip`；R09 `2/2`，S04-01 `4/4`。此前全量 WPF 133 类循环只到 5 类通过后被新用户输入控制中断（诊断 `20261019T010804009Z-d25e5350`），不代表失败；输入含未提交 S04-01 源码。

## 2026-10-19 用户反馈：窗口布局与弹出样式

按本轮五项反馈调整：壳层头部在 `1200 DIP` 起并为一行；显示列菜单使用共享玻璃色、圆角和遵循动效偏好的淡入位移动画；存档详情展开时保留四行表格，媒体详情展开时至少保留一整行卡片；通知操作按钮等高，空状态只在通知表格列中水平居中并复用玻璃空状态面板。SDK `8.0.423` Release solution build、15 项定向 WPF 测试、全尺寸双主题 RenderHarness 和最终 shell QA 均通过。构建/渲染工作树还包含用户未提交的 S04-01 媒体比较更改，本轮提交仅选择 UI 修复文件；真实 Playnite 中的新版本渲染尚未复验，完整 WPF 隔离循环未运行。

## 2026-10-19 Round4 S04-01 比较对象集合

S04-01 实现提交 `f911cba4`，已完成受控验收：最多两张有效截图按稳定 `MediaId` 保存在筛选无关集合；视频/缺失文件有说明，只有被选中的截图归档路径会接受 `File.Exists` 检查。SDK8.0.423 Release solution build `0 warnings / 0 errors`，生产页面按钮/命令行为 `5/5`，双主题 11 尺寸 RenderHarness `render-qa OK / 0 PROBLEM`。源、构建 DLL、PDB、TRX 和合成渲染截图身份见 [S04-01 证据](../design/reviews/ui-finesse-round4-20261018/evidence/S04-01/README.md)。没有运行完整 WPF 隔离循环或真实 Playnite；offscreen DPI 为逻辑 1.00。S04-02 已验收，下一项 S04-03 同步缩放移动。

## 2026-10-19 Round4 S04-02 双图工作区

实现提交 `825e58e9867ae3303c100eef31693f768f1772cf`：媒体比较区等比例并排显示两张截图及原图像素尺寸、大小、来源和游戏 ID；宽度低于 `560 DIP` 时上下排列，高度低于 `680 DIP` 时自动收起以保留列表空间，仍可手动展开；列表最小可读高度 `154 DIP`。SDK `8.0.423` Release solution build `0/0`，两个隔离 WPF 类共 `7/7`，双主题 11 尺寸 RenderHarness `render-qa OK / 0 PROBLEM`，源码校验通过。证据与 SHA：[S04-02](../design/reviews/ui-finesse-round4-20261018/evidence/S04-02/README.md)。未运行完整 WPF 隔离循环或真实 Playnite；渲染使用合成数据、未读取真实媒体。下一项 S04-03 同步缩放移动。

## 2026-10-19 Round4 S03-08 来源与方案验收

验收测试提交 `37f42cc2e9920574d43d13cf8018ad6dcf114158` 完成受控验收，没有生产代码变化。合成两设备夹具验证明确父 ID、同指纹和时钟偏差；41 天前 readiness 只显示该次检查年龄，不自行判过期；只读展示未改写合成存档文件；草稿重验与原 RestoreConfirmationGuard 相关行为通过。SDK9 Release solution build `0/0`，Core `10/10`，Playnite `15/15`（含 R09 `2/2`），source validation 通过。当前机器未安装 global.json 锁定的 SDK8.0.423，完整 WPF 循环和真实 Playnite未运行。证据：[S03-08](../design/reviews/ui-finesse-round4-20261018/evidence/S03-08/README.md)。下一项按计划核对 S04-01。

## 2026-10-19 Round4 S03-07 脱敏方案导出

实现提交 `eeedd003bb91a0e562529b45b15d4cde67f18db8` 已完成受控验收：SDK8 Release solution build 成功（17 条 NuGet audit 数据源不可达警告、0 errors），导出/草稿/R09 定向测试 `18/18`、source validation、XAML `25/25`、WPF 静态审查 0 errors、RenderHarness 双主题和精确提交隔离 no-start 安装通过。完整 WPF 循环、真实 Playnite和文件对话框交互仍未验；Demo 目录缺失。其后 S03-08 已验收；当前按计划核对 S04-01。证据：[S03-07](../design/reviews/ui-finesse-round4-20261018/evidence/S03-07/README.md)。

## 2026-10-19 Round4 S03-06 方案失效规则

实现提交 `7366d8cfe76702350a4c397f8de8aeed8e482b38` 已完成受控验收：SDK8 Release solution build 成功（17 条 NuGet audit 数据源不可达警告、0 errors），定向 Playnite `14/14`（含 R09 `2/2`）、source validation、XAML `25/25`、RenderHarness 双主题和精确提交隔离 no-start 安装通过。完整 WPF 逐类循环、真实 Playnite仍未验。旧 R09 诊断文件缺失，旧运行输入身份未知。后续 S03-07 已完成，S03-08 已验收；当前按计划核对 S04-01。证据：[S03-06](../design/reviews/ui-finesse-round4-20261018/evidence/S03-06/README.md)。

## 当前身份与活动事项

- 本地 `main` 已提交 S00-02、S01-01 至 S01-08、S02-01 至 S02-08、S03-01 至 S03-08 和维护记录。S00-02 的完整 WPF/安装/版本门禁仍在 R05 系统指针读取处受权限阻挡；相关宿主与安装事实按各任务证据分别记录，不把受控结果写成真实宿主验收。
- 前一轮 `BUILD-20261019` 诊断已修正设置页范围字段数量断言；该记录保留其当时的 `NumericInputTests 12/12`、R09 `2/2`，不代表当前最新整链。当前 main 全解方案和一键结果见 S03-04 证据：Release build `0/0`，Core `156/156`、Worker `380/1 skipped`、Playnite 非 WPF `475/27 skipped`；WPF 隔离在 `48/133` 类处因 R05 系统指针读取失败中断，R09 当前 main `2/2`。
- S03-02 提交 `fd59dcc1` 完成版本关系视图受控验证：Worker 用时间键+备份 ID 稳定分页，每页最多 100 项；Playnite 详情按时间显示父版本、分叉、同内容版本、设备和不确定关系。SDK `9.0.302` Release solution `0/0`、Playnite 定向 `12/12`（R09 `2/2`）、Worker `2/2`、RenderHarness `render-qa OK / 0 PROBLEM`。SDK8 正式基线与真实 Playnite 未验；旧 R09 运行身份文件缺失，不能从本轮通过反推旧输入。证据：[S03-02](../design/reviews/ui-finesse-round4-20261018/evidence/S03-02/README.md)。
- S03-03 提交 `3268565c` 已在 SDK `8.0.423` 完成冲突原因对照受控验证：设备冲突详情同时显示两侧备份/设备证据、共同父版本、指纹比较结果和 readiness 检查状态/时间；不按时间自动推荐覆盖。Release solution `0/0`，Playnite `4/4`、Worker `2/2`、Core `7/7`、R09 `2/2`，RenderHarness `render-qa OK`。证据：[S03-03](../design/reviews/ui-finesse-round4-20261018/evidence/S03-03/README.md)。
- S03-04 恢复方案草稿已实现并完成定向受控验证，源实现提交 `1c7083e2`；完整一键复跑验证源码 `d4068e65`。SDK `8.0.423` Release solution `0/0`；Core `156/156`、Worker `380 passed / 1 skipped`、Playnite 非 WPF `475 passed / 27 skipped`、当前 main R09 `2/2`、草稿 `4/4`；此前独立隔离安装和双主题 RenderHarness 复跑通过。最新全量 WPF 循环跑到 `48/133` 类时在 `R05TooltipTimingBehaviorTests` 因启动前系统指针读取失败中断（其余 85 类未运行）；未启动真实 Playnite。用户旧 R09 同次 summary/TRX/DLL/PDB 暂时没有，旧运行身份仍未知。证据：[S03-04](../design/reviews/ui-finesse-round4-20261018/evidence/S03-04/README.md)。S03-05 随后完成；下一项为 S03-06。
- S03-05 方案差异比较由 `b2c1e3de` 实现：按 A → B 比较草稿来源、映射、预计影响和未知项，使用 DPAPI 保护的 HMAC 映射指纹，不保存路径或密钥。SDK8 Release solution `0/0`，Playnite `7/7`、Worker `4/4`，RenderHarness 双主题通过，精确提交的隔离 no-start 安装通过；完整 WPF 循环与真实 Playnite未运行。证据：[S03-05](../design/reviews/ui-finesse-round4-20261018/evidence/S03-05/README.md)。下一项 S03-06 方案失效规则。
- S03-01：提交 `5e05aa73` 新增只读来源关系模型。基于真实父 ID 建父边，基于内容指纹标注等价版本；报告缺父/重复/歧义 ID、环、缺失身份和未知关系，不按时间戳推导线性历史。输入最多 500 条并用一条 lookahead 判断截断。SDK `9.0.302` Core Release 测试 `156/156`；global.json 要求 SDK8，本次没有全解方案、Worker/Playnite 接入或真实宿主验证。证据：[S03-01](../design/reviews/ui-finesse-round4-20261018/evidence/S03-01/README.md)。
- S02-08：复用通知收件箱/Worker 持久化等现有实现完成体验验收；SDK `9.0.302` Playnite 定向测试 `20/20`、Worker 持久化 `12/12`，插件 DLL/PDB 与测试加载副本哈希一致。完整 WPF 循环、SDK8 基线、真实 Playnite、Windows 系统高对比和读屏器未验；WPF 清理输出 `InvalidComObjectException`，VSTest 退出码为 0。详情：[S02-08 证据](../design/reviews/ui-finesse-round4-20261018/evidence/S02-08/README.md)。
- S02-07：提交 `9ed6b4e5`、`c4fb56a0` 为明确重试链路建立通知问题身份；同一故障重试归并并展示次数、首末时间、最后错误、首次/最近任务入口，成功重试关闭告警，后续失败重新打开；通知异步乱序落库仍保留最早时间/任务。SDK `9.0.302` Release solution build `0/0`；Core `147/147`、Worker `374` 通过/`1` 跳过、通知持久化 `12/12`、通知与重试及 R09 定向测试 `19/19`（R09 `2/2`）；source validation 通过，WPF 静态审查 `0 errors / 26 warnings / 177 info`。SDK8、完整 WPF 循环、安装、双主题渲染和真实 Playnite 宿主仍待验；旧 R09 输入身份未知。详见 [S02-07 证据](../design/reviews/ui-finesse-round4-20261018/evidence/S02-07/README.md)。
- S02-06：提交 `a6153219` 明确只有已知警告/错误通知突破临时静音和安静时段；未知级别按信息通知走静音策略。设置解释任务/会话警告与错误仍弹出并留在收件箱；任务状态/会话摘要仍驱动实际级别，备份校验问题仍由 Findings 持久保存。SDK `9.0.302` Release solution build `0/0`；策略测试 `22/22`、通知记录持久化 `7/7`、同次隔离输出 R09 `2/2`，source validation 通过。该目录内插件 DLL 与 R09 测试目录加载副本 SHA-256 一致；旧失败诊断 ID `20261018T012631286Z-e027df9d` 的 summary/TRX/DLL/PDB 未保留，旧输入仍无法定位。SDK8、完整 WPF 循环、安装和真实 Playnite 宿主仍待验。详见 [S02-06 证据](../design/reviews/ui-finesse-round4-20261018/evidence/S02-06/README.md)。
- S02-05：提交 `2aa3fe3c` 增加通知临时静音与每日跨午夜安静时段，记录仍写入本地收件箱；SDK `9.0.302` Release solution build `0/0`，策略测试 `20/20`、设置持久化/交互 `15/15`、独立 R09 `2/2`，双主题 2 种尺寸设置探针 `4/4`。SDK8、完整 WPF 循环、一键安装和真实 Playnite 宿主仍待验；机器此前保存的一键日志停在 R05 系统鼠标位置读取，旧 R09 输入身份缺失。详见 [S02-05 证据](../design/reviews/ui-finesse-round4-20261018/evidence/S02-05/README.md)。
- S02-04：提交 `1b17e4a0` 增加通知到关联游戏/任务的稳定 ID 跳转；SDK `9.0.302` Release solution build `0/0`，通知/R10、Worker 精确任务 ID、R09 定向测试 `20/20`，双主题 11 尺寸 Offscreen Render QA OK。正式 SDK8、完整 WPF 循环、安装和真实 Playnite 宿主仍待验；保存的一键日志在 R05 系统鼠标位置读取失败，未安装。旧 R09 输入身份缺失。详见 [S02-04 证据](../design/reviews/ui-finesse-round4-20261018/evidence/S02-04/README.md)。
- S02-03：提交 6d3a6f43 支持单条/当前筛选结果已读和确认清除已读记录；Release 构建 0/0，Playnite 定向 18/18（R09 2/2），Worker SQLite 7/7，双主题 11 尺寸 Render QA 22/22，source validation 通过，WPF 静态审查 0 errors / 26 warnings / 177 info。真实 Playnite 安装/宿主呈现和完整 WPF 隔离循环未运行；外机旧 R09 输入身份仍未知。详见 [S02-03 证据](../design/reviews/ui-finesse-round4-20261018/evidence/S02-03/README.md)。
- S02-02：通知记录生产者与通知收件箱已接入；SDK `8.0.423` Release solution build `0/0`，通知/页面切换/焦点/命令目录定向测试 `15/15`（R09 `2/2`），Worker 持久化 `3/3`，双主题 11 尺寸 Render QA `22/22`，source validation 通过。完整 WPF 独立进程循环与真实 Playnite 安装/宿主呈现未运行。详见 [S02-02 证据](../design/reviews/ui-finesse-round4-20261018/evidence/S02-02/README.md)。
- S00-01：已满足。稳定命令目录复用现有 ICommand；原受控行为 `7/7`，当前 SDK8 命令目录类复核 `2/2`。见 [S00-01 证据](../design/reviews/ui-finesse-round4-20261018/evidence/S00-01/README.md)。
- S00-02：代码完成待验收。Shell 内搜索 overlay，名称/别名/分类过滤，空输入最多 8 项，显示目标与可用状态；定向用例 `21/21`、200 项过滤 `0.554 ms`。SDK8 Release build 通过，但完整 WPF 隔离在 R05 系统指针 `GetCursorPos` Win32 error 5 处中止，尚未到安装/版本核验。见 [S00-02 证据](../design/reviews/ui-finesse-round4-20261018/evidence/S00-02/README.md)。
- S01-01：版本化视图配置模型已实现并接入设置复制/重置；SDK `8.0.423` Release 测试项目构建 `0/0`，配置测试与 R09 共 `7/7` 通过。Json.NET 集合替换问题已由 `ObjectCreationHandling.Replace` 修复。未改 XAML；Playnite host 未验。详见 [S01-01 证据](../design/reviews/ui-finesse-round4-20261018/evidence/S01-01/README.md)。
- S01-02：新增舒适/紧凑表格密度设置，行/表头/留白令牌为 `52/42/12` 与 `44/38/10 DIP`，正文字号不变；四行最小高度按活动令牌计算。SDK `8.0.423` Release 解决方案构建 `0/0`；密度/设置 `14/14`、R09 `2/2`、MediaInbox 几何 `3/3`、R23 双主题生产资源 `9/9`。提交 `ad7bd563`。真实 Playnite 设置保存与呈现未验证；完整 WPF/安装门禁仍受 R05 系统指针权限阻挡，详见 [S01-02 证据](../design/reviews/ui-finesse-round4-20261018/evidence/S01-02/README.md)。
- S01-03：提交 `8cdf5253` 为存档历史、候选路径与任务表格加入可选列菜单和稳定键持久化；状态/身份列固定可见，新列默认显示，媒体收件箱未改。SDK `8.0.423` Release 解决方案构建 `0/0`，列显隐/列宽/视图配置/R09 共 `17/17`，source validation 通过。用户外机 R09 日志第 151 行与当前测试源码不一致；当前提交 R09 `2/2`。真实 Playnite 设置保存及呈现未验证；详见 [S01-03 证据](../design/reviews/ui-finesse-round4-20261018/evidence/S01-03/README.md)。
- S01-04：提交 `c6eb8d62` 为任务、存档历史和候选路径表格加入命名视图保存、复制、重命名与删除。默认排列的“保存”创建命名视图；活动命名视图不覆盖全局默认列宽/显隐/任务筛选状态。SDK `8.0.423` Release 解决方案构建 `0/0`，视图配置、交互、列显隐/宽度、R09、R10、Q14 隔离测试 `29/29`；Render QA OK，Task `1040×700` 有 `4/4` 可读行。真实 Playnite 设置读写、双主题宿主呈现及安装门禁未验证；外机 R09 缺少同次 summary/TRX/DLL/PDB 身份，本机通过不能解释外机输入。详见 [S01-04 证据](../design/reviews/ui-finesse-round4-20261018/evidence/S01-04/README.md)。
- S01-05：提交 `dcfca235` 增加已保存视图的临时差异提示与恢复动作，观察筛选、列宽/显隐、排序和密度；筛选值无法由预设表示时阻止保存/另存。提交后 SDK `8.0.423` Release 解决方案构建 `0/0`，七个相关隔离测试类 `29/29`，Render QA OK，Task `1040×700` 为 `4/4` 行。真实 Playnite、完整 WPF/安装门禁未运行；既有 R05 `GetCursorPos` Win32 error 5 边界保留。证据与程序集身份见 [S01-05](../design/reviews/ui-finesse-round4-20261018/evidence/S01-05/README.md)。
- S01-06：提交 `9ec0096b` 增加表格布局重置范围预览。SDK `8.0.423` Release 解决方案构建 `0/0`；定向测试 `39 passed / 0 failed / 11 skipped`，Render QA OK，Task/候选路径 `1040×700` 均为 `4/4` 可读行。完整 WPF/安装门禁与真实 Playnite 设置保存/宿主双主题未验；R05 权限边界保留。外机 R09 缺少同次 summary/TRX 和 DLL/PDB 身份，当前 R09 `2/2` 不能解释其失败。详见 [S01-06 证据](../design/reviews/ui-finesse-round4-20261018/evidence/S01-06/README.md)。
- S01-07：提交 `87520a92` 在共享列宽控制器中加入窗口窄时临时压缩与放宽后恢复；临时宽度不写设置，Save 历史断点分支已移除。SDK `8.0.423` Release 解决方案构建 `0/0`；相关 WPF 测试 `20/20`，R09 `2/2`；Render QA OK，Task/候选路径 `1040×700` 均为 `4/4`。真实 Playnite、完整 WPF/安装门禁仍未验。外机 R09 的 `Thickness(2)→Thickness(1)` 与 `f13b9e05` 前旧断言一致，但因缺少其同次身份包不能确认实际测试输入。详见 [S01-07 证据](../design/reviews/ui-finesse-round4-20261018/evidence/S01-07/README.md)。
- S01-08：提交 `d1005e33` 完成视图配置迁移回退：无效视图字段可恢复，其他设置保留，损坏原文件先备份；SDK `8.0.423` Release solution build `0/0`，五个隔离测试类 `21/21`（含 R09 `2/2`），source validation 与 diff check 通过。真实 Playnite 设置保存/加载、双主题宿主呈现和安装门禁未运行；用户旧 R09 运行缺同次 summary/TRX/DLL/PDB，无法确认实际输入。详见 [S01-08 证据](../design/reviews/ui-finesse-round4-20261018/evidence/S01-08/README.md)。
- S02-01：本地通知记录 DTO 与 Worker SQLite 去重/保留 API 已实现；SDK `8.0.423` Release 解决方案构建 `0/0`，通知持久化测试 `3/3`、Worker `364` passed / `1` skipped。30 天、500 条上限；通知生产者未接入，未改 XAML/发送外部通知。详见 [S02-01 证据](../design/reviews/ui-finesse-round4-20261018/evidence/S02-01/README.md)。
- S00-02 的精确 SDK8 `8.0.423` 已在仓库隔离目录可用，SDK8 Release build 已通过；完整 WPF 测试因 R05 `GetCursorPos` 权限错误中断，安装与版本核验未运行。不得把受控构建或合成呈现写成 Playnite 真实呈现。
- S07-01 已验收：本入口由 254 行压缩至 32 行，完整快照保存在 [归档](CURRENT_STATE_HISTORY_THROUGH_20261018.md)；实现提交 `4ea5e694`，压缩前正文逐字核对相同。验证及边界见 [S07-01 证据](../design/reviews/ui-finesse-round4-20261018/evidence/S07-01/README.md)。后续准入见 [Round4 账本](../design/reviews/ui-finesse-round4-20261018/ROUND4_PROGRESS.md) 与 [自主积压](../AUTONOMOUS_BACKLOG.md)。
- S07-02 已验收：64 项 Round4 状态和证据索引现由 `ROUND4_STATUS.json` 驱动，账本摘要/表格自动生成，并接入 `validate-source.py` 检查；旧 Q/R 状态独立保留。见 [S07-02 证据](../design/reviews/ui-finesse-round4-20261018/evidence/S07-02/README.md)。
- S07-05 已验收：现有 diagnostics 增加 OS/PowerShell/位数身份，新增本机只读预览；旧摘要可从原始 TRX 只读复算。不检查运行中的 Playnite/安装目录。见 [S07-05 证据](../design/reviews/ui-finesse-round4-20261018/evidence/S07-05/README.md)。
- S07-04 已建立 CI 结果记录规则：只在新失败/修复、发布或验收状态变化时新增结论，不为纯记录提交的通过结果再造一笔提交；保留 run/job/artifact 身份。详见 [仓库规则](../../AGENTS.md) 与 [S07-04 证据](../design/reviews/ui-finesse-round4-20261018/evidence/S07-04/README.md)。未调整 workflow/required checks。
- S07-06 已在诊断 `summary.json` 加入原始 TRX 测试用例计数、结果状态、suite 和 WPF 隔离类运行汇总，并逐文件对照原 TRX Counters。定向诊断自测与历史原始 TRX 复算通过；历史用例失败原样保留。见 [S07-06 证据](../design/reviews/ui-finesse-round4-20261018/evidence/S07-06/README.md)。
- S07-07 已建立环境能力缓存与校验器，区分 SDK 缺失、真实宿主未检查、CEF/ETW 权限阻塞及 GitHub branch protection 读取权限阻塞，并逐项记录证据和重试条件；没有重试被拒探测。见 [S07-07 证据](../design/reviews/ui-finesse-round4-20261018/evidence/S07-07/README.md) 与[缓存](ENVIRONMENT_CAPABILITY_CACHE.json)。
- S07-03 审查发现 workflow 对纯文档和混合路径都运行完整双 SDK 流程；GitHub API 读取 `main` branch protection 返回 403，无法确认 required check 名称。workflow 未修改，待获得配置读权限或用户提供清单后再解锁。见 [S07-03 审查](../design/reviews/ui-finesse-round4-20261018/evidence/S07-03/README.md)。
- S07-08 已记录 64 项总体验收条件、每批快照内容与无可执行项时的边界处理。Round4 尚未完成；具体规则见[交付协议](../design/reviews/ui-finesse-round4-20261018/DELIVERY_AND_EXIT.md)。S01-08、S02-02 至 S03-04 受控验证已通过；完整隔离循环与真实宿主的剩余边界按任务记录；下一项处理 S03-06 方案失效规则。

## 已核验事实与外部边界

- 最近有完整双 SDK runner 记录的是 [Actions #1086](https://github.com/Nikilua/GameSaveCenter/actions/runs/39077533150)：SDK8.0.423/SDK10.0.401 的 Core、Worker、Playnite WPF、SDK8 Render QA 成功。它验证对应旧提交和 runner，不验证 S00-02 当前本机代码或用户 Playnite。
- 外机于 2026-10-18 报告 R09 `1/2`，`Thickness` 期望 `2`、实际 `1`，堆栈行号与当前测试源码中的断言行不一致。本机使用当前提交、新隔离输出完成 Release solution build 后，R09 `2/2` 通过；测试目录加载的插件 DLL 与该次插件构建 DLL SHA-256 一致。旧诊断 `20261018T012631286Z-e027df9d` 的 summary/TRX/source HEAD/DLL/PDB 未保留，无法区分旧 checkout、旧测试 DLL、资源 DLL 或 PDB 错配；“输入产物错配”只是线索，不是已证实根因。这次报告的是测试断言失败，不是编译器错误。详见 [S02-06 复核证据](../design/reviews/ui-finesse-round4-20261018/evidence/S02-06/README.md)。
- 历史 CEF `0x5` 与 ETW 系统跟踪权限拒绝保持原边界；不绕过、不重复探测，只有前置条件可观察变化后再复查。
- Playnite 未由本地门禁运行；OS UIA/读屏、实际 IME、物理 DPI/跨屏和最终呈现仍未验。离屏 WPF、源码测试和 CI 均只证明各自覆盖范围。

## 产品结构与有效约束

- Playnite 插件目标 `net462`，Core/Contracts 为 `netstandard2.0`，Worker 为 `net8.0-windows10.0.19041.0`，Playnite 测试为 `net472`。插件与 Worker 通过 Named Pipe 协作，SQLite 由 Worker 持久化。
- 生产外壳为 `AcrylicProductionShellView`；共享主题入口为 `Themes/AcrylicProductionResources.xaml`、`DesignTokens.xaml`、`WpfUiProduction.xaml` 与 `Redesign.xaml`。六个工作区为 Overview、Saves、Trainers、Media、Tasks、Maintenance。
- Demo-first 仍是视觉优先级，但 `GameSaveCenter.AcrylicFork/src/GameSaveCenter.Playnite/Design/` 当前工作区不存在。缺少 Demo 源时沿用生产共享样式，并明确记录未完成 Demo 对照；不要伪造宿主、屏幕或主题验证。
- 保留真实命令与安全/取消/错误语义、键盘可达性、UI Automation、IME 输入、虚拟化、Playnite 兼容性和现有存档/媒体/云端保护流程。游戏平台筛选以 `DropDownClosed` 提交，避免关闭前的临时选择写入用户偏好。UI 变更须读 `.codex/skills/wpf-apple-desktop-ui/SKILL.md`、`docs/design/APPLE_WPF_IMPLEMENTATION_PROMPT.md`、`docs/design/UI_CHANGE_GATE.md`，并遵循当前 Demo-first AGENTS。

## 验证、提交与启动入口

- 按影响范围运行 `python scripts/validate-source.py .`、WPF 静态审查以及适用的 Release 构建/行为测试；行为矩阵入口为 `scripts/e01-behavior-matrix.ps1`。修改 XAML 后按 UI_CHANGE_GATE 使用 `GameSaveCenter-Run.cmd`；不绕过仓库 SDK 版本检查。`.github`、截图与人工运行项中标记的 `MANUAL QA REQUIRED` 仍需真实人工证据，自动检查通过不能替代。
- 每个独立阶段记录实际输入、结果、未验边界和证据，使用中文提交并推送当前远端分支。纯文档或证据更新不为记录同一次变化反复触发完整 CI。
- 新开发会话依次读取 `CURRENT_STATE.md`、`PROJECT_MEMORY.md`、`WORKLOG.md`、`DEVELOPMENT_HANDOFF.md`，再查最近 Git log/status 与 `AUTONOMOUS_BACKLOG.md`。历史细节按链接追溯，不将归档中的旧“当前状态”当作最新事实。
- 历史入口：[2026-10-18 压缩前快照](CURRENT_STATE_HISTORY_THROUGH_20261018.md)、[2026-09-30 之前历史](CURRENT_STATE_HISTORY_THROUGH_20260930.md)、[项目记忆](PROJECT_MEMORY.md)、[工作日志](WORKLOG.md)、[交接](../DEVELOPMENT_HANDOFF.md)。Round4 实现与逐项状态见 [计划](../design/UI_FINESSE_ROUND4_64_TASKS_2026-10-18.md) 和 [账本](../design/reviews/ui-finesse-round4-20261018/ROUND4_PROGRESS.md)。
