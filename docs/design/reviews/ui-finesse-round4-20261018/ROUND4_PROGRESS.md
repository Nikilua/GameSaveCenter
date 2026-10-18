# 第四轮执行账本

任务源：[64 项实施计划](../../UI_FINESSE_ROUND4_64_TASKS_2026-10-18.md)。S00-01 已按新范围实现并验收，S00-02 代码已完成、等待 SDK8/Playnite UI 门禁；S07-01/02/04/05/06/07/08 已完成。S07-03 因 GitHub 无权读取 main branch protection、required checks 不可见而外部阻塞，workflow 未改。旧 Q/R 工作与环境边界未撤销。交付快照与退出条件见[协议](DELIVERY_AND_EXIT.md)，后续准入以 `docs/AUTONOMOUS_BACKLOG.md` 为准。

状态：待开始、实施中、代码完成待验收、已验收、已满足、外部阻塞、不适用。已满足必须有当前等价能力与证据；受控通过不代表真实宿主通过。每项分别补实现 commit、验证与未验边界。

<!-- ROUND4-STATUS-GENERATED-START -->

状态摘要（由 `ROUND4_STATUS.json` 生成）：任务 64 项；状态 已验收11、已满足1、代码完成待验收15、外部阻塞1、待开始36。
实现状态：完成27、未开始37、不适用0；受控验证：通过27、未跑36、不适用0；宿主验证：通过0、待验9、未跑47、不适用8。

| ID | 任务 | 状态 | 实现 commit | 验证与证据 | 边界/下一步 |
| --- | --- | --- | --- | --- | --- |
| S00-01 | 只读命令目录 | 已满足 | dd38f8ca | [证据](evidence/S00-01/README.md)：origin/main 已有稳定命令目录；原 7/7 SDK9 与当前 SDK8 CommandDirectoryBehaviorTests 2/2 复核通过 | 当前 SDK8.0.423 解决方案构建通过；真实 Playnite 未运行；S00-02 仍待 UI 门禁 |
| S00-02 | 命令搜索面板 | 代码完成待验收 | 29dc09b2、a58dd582 | [证据](evidence/S00-02/README.md)：Release 目标构建与 21/21 定向用例通过；SDK8 全量隔离在 R05 系统指针权限错误处中止 | SDK8 Release build 已通过；WPF 隔离在 R05 GetCursorPos Win32 error 5 处停止，安装/版本核验及真实宿主未运行；门禁通过后再推送并准入 S00-03 |
| S00-03 | 键盘与组合输入 | 待开始 | — | 待实现后按范围验证 | 先核对已有能力；依赖见计划 |
| S00-04 | 固定与最近命令 | 待开始 | — | 待实现后按范围验证 | 先核对已有能力；依赖见计划 |
| S00-05 | 上下文预览 | 待开始 | — | 待实现后按范围验证 | 先核对已有能力；依赖见计划 |
| S00-06 | 危险动作转交 | 待开始 | — | 待实现后按范围验证 | 先核对已有能力；依赖见计划 |
| S00-07 | 结果精细排版 | 待开始 | — | 待实现后按范围验证 | 先核对已有能力；依赖见计划 |
| S00-08 | 目录行为验收 | 待开始 | — | 待实现后按范围验证 | 先核对已有能力；依赖见计划 |
| S01-01 | 视图配置模型 | 已验收 | 25bbe005 | [证据](evidence/S01-01/README.md)：SDK8 Release 测试项目构建通过；ViewConfigurationDefinition 与 R09 合计 7/7 | 未改 XAML；真实 Playnite 设置加载/保存未验证；密度、列选择和视图保存交互留后续任务 |
| S01-02 | 舒适紧凑密度 | 代码完成待验收 | ad7bd563 | [证据](evidence/S01-02/README.md)：SDK8 Release 构建0/0；密度/设置14/14，R09 2/2，媒体几何3/3，生产资源双主题9/9 | 真实 Playnite 设置加载/保存及宿主呈现未验证；完整 WPF/安装门禁仍在 R05 GetCursorPos Win32 error 5 处停止；门禁满足后继续 S01-03 |
| S01-03 | 可选列管理 | 代码完成待验收 | 8cdf5253 | [证据](evidence/S01-03/README.md)：SDK8.0.423 Release 解决方案构建 0/0；列显隐 4/4、列宽 6/6、视图配置 5/5、R09 2/2，source validation 通过 | Playnite 宿主设置提交/双主题视觉未验；完整安装/UI 门禁仍受 R05 GetCursorPos Win32 error 5 阻塞 |
| S01-04 | 保存与复制视图 | 代码完成待验收 | c6eb8d62 | [证据](evidence/S01-04/README.md)：SDK8.0.423 Release 解决方案构建 0/0；视图配置/交互、列显隐/列宽、R09、R10、Q14 隔离测试共 29/29；Render QA OK，Task 1040×700 为 4/4 可读行 | 未运行完整 WPF/安装门禁及真实 Playnite；既有 R05 GetCursorPos Win32 error 5 权限边界未绕过，宿主设置保存与双主题呈现待验 |
| S01-05 | 临时覆盖标记 | 代码完成待验收 | dcfca235 | [证据](evidence/S01-05/README.md)：SDK8.0.423 Release 解决方案构建 0/0，视图/列/R09/R10/Q14 隔离测试 29/29，Render QA OK | 完整 WPF/安装门禁和真实 Playnite 设置保存、双主题宿主呈现未验；既有 R05 GetCursorPos Win32 error 5 权限边界保留 |
| S01-06 | 布局重置预览 | 代码完成待验收 | 9ec0096b | [证据](evidence/S01-06/README.md)：SDK8.0.423 Release 构建 0/0；定向测试 39 通过、11 跳过；Render QA OK，Task/候选表 1040×700 均为 4/4 可读行 | 完整 WPF/安装门禁和真实 Playnite 设置/双主题宿主呈现未验；R05 GetCursorPos Win32 error 5 权限边界保留；外机 R09 因缺少同次 summary/TRX/DLL/PDB 身份无法追溯 |
| S01-07 | 断点适配记忆 | 代码完成待验收 | 87520a92 | [证据](evidence/S01-07/README.md)：SDK8.0.423 Release 构建 0/0；六个隔离测试类 20/20，R09 2/2；Render QA OK，Task/候选表 1040×700 均为 4/4 可读行 | 完整 WPF/安装门禁和真实 Playnite 设置/双主题宿主呈现未验；R05 GetCursorPos Win32 error 5 权限边界保留；外机 R09 的 Thickness(2)→Thickness(1) 与 f13b9e05 前旧断言一致，但缺少同次 summary/TRX/DLL/PDB，无法确认其实际测试输入 |
| S01-08 | 视图迁移验收 | 代码完成待验收 | d1005e33 | [证据](evidence/S01-08/README.md)：SDK 8.0.423 Release solution build 0/0；五个隔离测试类 21/21（含 R09 2/2）；source validation 与 diff check 通过。 | 真实 Playnite 设置保存/加载、双主题宿主呈现及安装门禁未运行；用户旧 R09 运行缺同次 summary/TRX/DLL/PDB，无法确认实际测试输入。 |
| S02-01 | 通知记录契约 | 已验收 | 83194c50 | [证据](evidence/S02-01/README.md)：SDK8 Release 构建通过；NotificationRecordPersistenceTests 3/3，Worker 364 passed / 1 skipped | 通知生产者尚未接入；无 XAML、外发通知或 Playnite host 验证；S02-02 后续接入 |
| S02-02 | 收件箱入口 | 代码完成待验收 | 8810d9e6 | [证据](evidence/S02-02/README.md)：SDK8 Release solution 0/0；Playnite 定向测试 15/15（R09 2/2）、Worker 3/3；双主题 Render QA 22/22 | 完整 WPF 独立进程循环未完成；真实 Playnite 安装/宿主呈现未运行；旧 R09 诊断缺少 summary/TRX/DLL/PDB，不能确认旧输入；S02-03 已在后续批次完成。 |
| S02-03 | 已读语义 | 代码完成待验收 | 6d3a6f43 | [证据](evidence/S02-03/README.md)：SDK8.0.423 Release 构建 0/0；Playnite 18/18（R09 2/2）；Worker SQLite 7/7；双主题 11 尺寸 Render QA 22/22 | 完整 WPF 独立进程循环和真实 Playnite 安装/宿主呈现未运行；旧 R09 诊断缺同次 summary/TRX/DLL/PDB，实际输入身份无法确认。下一项按依赖审查 S02-04。 |
| S02-04 | 上下文跳转 | 代码完成待验收 | 1b17e4a0 | [证据](evidence/S02-04/README.md)：SDK 9.0.302 Release solution build 0/0；通知/R10、Worker 精确任务查询与 R09 定向测试 20/20；双主题 11 尺寸 Offscreen Render QA OK。SDK8 正式基线、完整 WPF 独立循环及真实宿主未验 | global.json 要求 SDK 8.0.423；本次受控构建使用 SDK 9.0.302。保存的一键日志在 R05 系统鼠标位置读取处中止安装；用户旧 R09 诊断缺少 summary/TRX/DLL/PDB，输入身份仍未知。 |
| S02-05 | 临时静音与时段 | 代码完成待验收 | 2aa3fe3c | [证据](evidence/S02-05/README.md)：SDK9 Release solution 0/0；通知策略 20/20，设置持久化/交互 15/15，独立 R09 2/2；浅/深主题 × 1040×700/720×640 设置探针 4/4 | 当前只安装 SDK 9.0.302，global.json 锁定 SDK 8.0.423；完整 WPF 隔离循环、安装和真实 Playnite 宿主未验证。已有一键日志在 R05 系统鼠标位置读取处停止；旧 R09 同次 summary/TRX/DLL/PDB 身份缺失。 |
| S02-06 | 重要事件例外 | 代码完成待验收 | a6153219 | [证据](evidence/S02-06/README.md)：SDK9 Release solution build 0/0；弹窗策略 22/22、通知记录持久化 7/7、当前匹配构建 R09 2/2；source validation 通过 | global.json 锁定 SDK 8.0.423，本机仅有 SDK 9.0.302；完整 WPF 隔离循环、安装和真实 Playnite 宿主未验。旧 R09 诊断身份缺失、根因未定位；本次匹配 DLL 复测通过。 |
| S02-07 | 重复归并摘要 | 代码完成待验收 | 9ed6b4e5、c4fb56a0 | [证据](evidence/S02-07/README.md)：SDK9 Release solution build 0/0；Core 147/147、Worker 374通过/1跳过、通知持久化 12/12、Playnite通知与R09定向 19/19；source validation 通过，WPF静态审查0错误 | global.json锁定SDK8.0.423，本机仅有SDK9.0.302；完整WPF隔离循环、一键安装和真实Playnite宿主未验。旧R09诊断文件和程序集身份未保留，旧失败根因仍未知。 |
| S02-08 | 通知体验验收 | 代码完成待验收 | 已有 S02 通知实现（83194c50 至 c4fb56a0） | [受控证据](evidence/S02-08/README.md)：SDK9 Playnite 通知体验 20/20，Worker 持久化 12/12；插件 DLL/PDB 与测试加载副本哈希匹配 | 本次使用 SDK9.0.302；SDK8 正式基线、完整 WPF 隔离循环、真实 Playnite、Windows 系统高对比和真实读屏器未验证。VSTest 退出码为0但 WPF 清理输出 InvalidComObjectException。 |
| S03-01 | 来源关系模型 | 已验收 | 5e05aa73 | [受控证据](evidence/S03-01/README.md)：SDK9 Core Release 构建成功；Core 全套测试 156/156，覆盖父关系、等价内容、异常关系和有界读取 | 仅新增 Core 模型/构建器与测试；SDK8 正式基线、全解方案、Worker IPC、Playnite 版本关系视图和真实恢复未验证。S03-02 负责视图接入。 |
| S03-02 | 版本关系视图 | 已验收 | fd59dcc1 | [受控证据](evidence/S03-02/README.md)：SDK9 Release solution 0/0；Playnite 12/12、Worker 2/2；RenderHarness render-qa OK，0 PROBLEM | 当前仅 SDK9.0.302；SDK8.0.423 正式基线和真实 Playnite 未运行。用户旧 R09 诊断缺同次 summary/TRX/DLL/PDB，当前 R09 通过不识别旧运行输入。 |
| S03-03 | 冲突原因对照 | 待开始 | — | 待实现后按范围验证 | 先核对已有能力；依赖见计划 |
| S03-04 | 恢复方案草稿 | 待开始 | — | 待实现后按范围验证 | 先核对已有能力；依赖见计划 |
| S03-05 | 方案差异比较 | 待开始 | — | 待实现后按范围验证 | 先核对已有能力；依赖见计划 |
| S03-06 | 方案失效规则 | 待开始 | — | 待实现后按范围验证 | 先核对已有能力；依赖见计划 |
| S03-07 | 脱敏方案导出 | 待开始 | — | 待实现后按范围验证 | 先核对已有能力；依赖见计划 |
| S03-08 | 来源与方案验收 | 待开始 | — | 待实现后按范围验证 | 先核对已有能力；依赖见计划 |
| S04-01 | 比较对象集合 | 待开始 | — | 待实现后按范围验证 | 先核对已有能力；依赖见计划 |
| S04-02 | 双图工作区 | 待开始 | — | 待实现后按范围验证 | 先核对已有能力；依赖见计划 |
| S04-03 | 同步缩放移动 | 待开始 | — | 待实现后按范围验证 | 先核对已有能力；依赖见计划 |
| S04-04 | 差异查看辅助 | 待开始 | — | 待实现后按范围验证 | 先核对已有能力；依赖见计划 |
| S04-05 | 比较候选切换 | 待开始 | — | 待实现后按范围验证 | 先核对已有能力；依赖见计划 |
| S04-06 | 保留意见标记 | 待开始 | — | 待实现后按范围验证 | 先核对已有能力；依赖见计划 |
| S04-07 | 比较结束转交 | 待开始 | — | 待实现后按范围验证 | 先核对已有能力；依赖见计划 |
| S04-08 | 大图资源验收 | 待开始 | — | 待实现后按范围验证 | 先核对已有能力；依赖见计划 |
| S05-01 | 只读情景输入 | 待开始 | — | 待实现后按范围验证 | 先核对已有能力；依赖见计划 |
| S05-02 | 双方案预览 | 待开始 | — | 待实现后按范围验证 | 先核对已有能力；依赖见计划 |
| S05-03 | 估算不确定性 | 待开始 | — | 待实现后按范围验证 | 先核对已有能力；依赖见计划 |
| S05-04 | 预算分解 | 待开始 | — | 待实现后按范围验证 | 先核对已有能力；依赖见计划 |
| S05-05 | 策略差异定位 | 待开始 | — | 待实现后按范围验证 | 先核对已有能力；依赖见计划 |
| S05-06 | 保存情景草稿 | 待开始 | — | 待实现后按范围验证 | 先核对已有能力；依赖见计划 |
| S05-07 | 转交策略编辑 | 待开始 | — | 待实现后按范围验证 | 先核对已有能力；依赖见计划 |
| S05-08 | 情景性能验证 | 待开始 | — | 待实现后按范围验证 | 先核对已有能力；依赖见计划 |
| S06-01 | 工作清单契约 | 待开始 | — | 待实现后按范围验证 | 先核对已有能力；依赖见计划 |
| S06-02 | 加入待办入口 | 待开始 | — | 待实现后按范围验证 | 先核对已有能力；依赖见计划 |
| S06-03 | 清单浏览整理 | 待开始 | — | 待实现后按范围验证 | 先核对已有能力；依赖见计划 |
| S06-04 | 续接上下文 | 待开始 | — | 待实现后按范围验证 | 先核对已有能力；依赖见计划 |
| S06-05 | 逐项处理转交 | 待开始 | — | 待实现后按范围验证 | 先核对已有能力；依赖见计划 |
| S06-06 | 失效刷新 | 待开始 | — | 待实现后按范围验证 | 先核对已有能力；依赖见计划 |
| S06-07 | 清单导出预览 | 待开始 | — | 待实现后按范围验证 | 先核对已有能力；依赖见计划 |
| S06-08 | 跨会话回归 | 待开始 | — | 待实现后按范围验证 | 先核对已有能力；依赖见计划 |
| S07-01 | 精简当前事实入口 | 已验收 | 4ea5e694 | [证据](evidence/S07-01/README.md)：254 行压缩至 32 行；归档正文与提交前 Git 版本一致；source validation、diff check 通过 | 仅更新文档；未运行产品构建；S00-02 UI 门禁仍待 SDK8 与真实 Playnite |
| S07-02 | 结构化任务状态 | 已验收 | 7a81e5f2 | [证据](evidence/S07-02/README.md)：64 个 ID 与计划对齐；生成表格/分项摘要；校验重复 ID、标题、状态值与证据链接 | 仅管理 Round4 状态；既有 Q/R 外部阻塞保持独立，不从本索引自动清除 |
| S07-03 | 文档变更 CI 分流 | 外部阻塞 | — | [审查证据](evidence/S07-03/README.md)：工作流路径行为已审；branch protection API 403，required check 名称不可读 | 无 required check 上下文前不改 paths 过滤；须获得只读设置权限或用户提供配置 |
| S07-04 | 避免 CI 自循环记录 | 已验收 | 33c8722c | [证据](evidence/S07-04/README.md)：仓库规则限定结果记录触发条件；保留原 run/job/artifact 查询身份 | 未调整所有 push 触发的 workflow 或 required checks；不为纯记录提交的通过结果追加记录提交 |
| S07-05 | 跨机器诊断入口 | 已验收 | 4226a8df | [证据](evidence/S07-05/README.md)：复用诊断 collector；记录环境/checkout/DLL/PDB/TRX 身份并提供离线只读预览；旧 TRX 可现场复算 | 不探测运行中 Playnite 或安装目录；不收集用户存档/凭据；安装包身份仍需单独提供 |
| S07-06 | 测试计数口径 | 已验收 | c698c161 | [证据](evidence/S07-06/README.md)：原始 TRX 可复算用例与状态总数；区分 suite、实际测试类及 WPF 隔离类运行 | 旧 SDK8 证据只用于核验解析器；历史 Q14 失败保持失败，不代表当前代码 |
| S07-07 | 环境能力准入缓存 | 已验收 | 473e67d8 | [证据](evidence/S07-07/README.md)：缓存 SDK/Playnite/CEF/ETW 状态、证据与重试条件；7 项校验测试及 source validation 通过 | 不探测或绕过权限；S00-02 真实宿主门禁未运行，旧 Q/R 阻塞独立保留 |
| S07-08 | 交付快照与退出条件 | 已验收 | 5ee1a0c9 | [证据](evidence/S07-08/README.md)：定义每批交付快照、64 项退出条件与无可执行项处理；账本及源码校验通过 | 仅定义管理流程；Round4 尚有未完成项，S00-02 UI 门禁及外部依赖保持独立 |

<!-- ROUND4-STATUS-GENERATED-END -->
