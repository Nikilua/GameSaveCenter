# R19-06 分页快照变化证据

日期：2026-09-20  
分支：`codex/ui-finesse-round2`  
结论：已满足，待环境验证

## 1. 任务条件

R19-06 要求翻页期间新增、删除或排序变化使用稳定游标或明确重载策略，避免重复/漏项假象，末页变化不能造成无限加载，选择对象必须按稳定 ID 保持。

本项先核对最新生产代码和已有夹具，确认能力已经存在；本阶段没有重建分页服务、增加第二套快照或修改媒体/任务 UI。

## 2. 已有实现核对

- Worker 任务页在 `SqliteStateStore.TaskQueries` 中按 `created_utc DESC, task_id DESC` 排序，以 `(created_utc, task_id)` 编码游标；下一页使用严格的小于谓词，不使用 offset。查询限制为 `1..500`，多取一条判断 `HasMore`，仅把真实最后一项编码为 `NextCursor`。
- Worker 媒体页在 `SqliteStateStore.MediaQueries` 中按 `captured_utc DESC, media_id DESC` 排序，以 `(captured_utc, media_id)` 编码游标；分类、游戏、搜索和收件箱状态在游标之前进入同一 WHERE 条件。限制为最多 `200`，多取一条判定末页；无额外行时 `HasMore=false` 且 `NextCursor` 为空，不会因空页继续请求。
- Playnite `DashboardViewModel.Media` 将筛选/搜索/刷新作为 reset：推进 `mediaPageGeneration`、取消旧请求、清空游标并替换第一页；同一上下文翻页只带当前 `NextCursor`，工作区、游戏 ID 和 generation 不匹配的迟到页丢弃。待归类/已忽略分别保存游标、总数、`HasMore` 和累加器。
- `MediaPageAccumulator` 按 `MediaId` 归一化并去重；重叠页更新同 ID 的对象而不追加重复项，窗口上限为 `2000`，可在裁剪时保留当前选择。`SelectionAnchorResolver` 按稳定媒体 ID 恢复选择，只有目标已消失时才使用原索引作为可理解的邻近回退。任务列表同样按稳定 `TaskId` 合并和恢复。
- “加载更多”命令只在对应 `HasMore` 为真时可执行；生产分页响应的 `HasMore` 来自多取一条，因此末页返回、筛选变化或对象变化后不会因为重复使用旧 cursor 无限加载。新增或排序变化落在当前游标之前时，用户可通过刷新窗口取得新的第一页，而不会把新快照伪装成当前页的连续项。

## 3. 实际证据

- Worker `TaskQueryPersistenceTests` 与 `MediaQueryPersistenceTests` 合计 `12/12` 通过：同创建时间任务分页、同捕获时间媒体分页、过滤/收件箱总数、首屏之外搜索、稳定索引和大页查询均通过；同时间项目按稳定 ID 排序，第二页不包含第一页 ID。
- Playnite `MediaPageAccumulatorTests` 与 `TaskIndexedCollectionTests` 合计 `10/10` 通过：250 页媒体仍保持 `2000` 条窗口、重叠页按 ID 更新不重复、窗口裁剪和选中项保留、大库窗口上限均通过。
- Playnite `R07SelectionAnchorBehaviorTests` `4/4` 通过，证明按 ID 的跨刷新选择锚点与目标删除后的邻近回退行为。
- 合并执行的 Playnite 分页/索引/选择筛选为 `15 passed / 1 failed / 16 total`；唯一失败是相邻旧 R06 测试在旧 net472 产物中缺少 `GscBuildCommit`，由 `TestRepositoryContext` 身份门退出，与本项分页行为无关，已拆出，不写成全量通过。
- 本阶段无生产代码变更，沿用最近 `6d1a401b` 的 clean-tree Release 结果 `0 errors / 2 existing MediaCenter nullable warnings`；本阶段重新运行 `validate-source.py`、XAML 结构检查 `24/24` 和 `git diff --check`，均通过。

## 4. 负例与环境边界

- 以上变化夹具使用合成 DTO、隔离 SQLite、fake/testhost 和受控集合；没有读取或修改真实存档、媒体文件、用户云端或外部诊断。
- 稳定游标给出的是明确的分页边界：翻页期间新对象若排在游标之前，要通过刷新重载才进入第一页；对象被删除或排序字段变化时不保证旧页继续代表实时全量快照。真实生产并发变更时的可见时序、自动刷新提示和 Playnite 宿主重绘尚未运行验证。
- 未验真实 Playnite/package-host、最终 presented frame、物理 DPI/跨屏、UIA/读屏/IME、ETW、宿主性能和真实大库滚动；没有绕过 Named Pipe/系统跟踪权限。Demo 原目录不可用，沿用恢复生产基线。

## 5. 下一步

下一可执行任务为 `R19-07 外部文件变化`：先核对媒体/备份详情打开期间文件被移动、占用或损坏的已有回退、诊断上下文和重新定位入口，再补隔离文件系统负例。

## 2026-09-24 main 当前身份复核

- 当前 checkout 为 `main`，Release/test identity `9b3dd2f19f1bfce8c51ec12ec1b8b654c3c15bc1`。与 `6d1a401b` 比较后，任务/媒体的稳定游标键与谓词、MediaPageAccumulator、稳定选择解析没有发生语义变更；后续任务查询改动补存单调耗时字段，媒体视图增加了筛选摘要/详情通知。故重新构建并按类复测，不把旧程序集失败沿用为当前失败。
- Release solution/XAML build：XAML `24/24`、0 errors，保留 `MediaCenterView.xaml.cs:703` 的两条既有 `CS8602` warning。`python scripts/validate-source.py` 和 `git diff --check` 通过。
- Worker `TaskQueryPersistenceTests + MediaQueryPersistenceTests` `13/13`；Playnite `MediaPageAccumulatorTests + TaskIndexedCollectionTests` `10/10`；`R07SelectionAnchorBehaviorTests` `4/4`，合计 R19-06 关联行为 `27/27`。新增的单调耗时恢复用例也通过。
- 旧合并运行中的 R06 `GscBuildCommit` 身份门失败已在当前构建中复测：相邻 `R06SortingBehaviorTests 5/5` 通过，其中 `ProductionTablesAttachStableProfilesAndKeepSharedArrowContract` 当前源/程序集身份相符。该失败只保留为旧 net472 产物问题，不计当前行为失败。
- 原始结果分别见 [Worker 查询 13/13](R19-06-WORKER-QUERIES-20260924-9B3DD2F1.trx)、[Playnite 分页/任务索引 10/10](R19-06-PLAYNITE-PAGES-20260924-9B3DD2F1.trx)、[稳定选择 4/4](R19-06-SELECTION-ANCHOR-20260924-9B3DD2F1.trx) 和[相邻排序 5/5](R19-06-ADJACENT-SORTING-20260924-9B3DD2F1.trx)。
- 本轮只使用隔离 SQLite、fake、合成 DTO 与测试集合；生产并发新增/删除/重排时的呈现时序、正常 Playnite/package-host、最终呈现帧、DPI/UIA/IME、ETW 和宿主性能仍未验。任务维持“已满足，待环境验证”。
