# R01-06 宿主证据保全证据

> 当前 main 审计与归档已在 f55dce61adba84fec96c3e5434e5a8c1e3fa7132 重新生成并校验。旧 c2399d7b、b5c7a6d4、5fbfc869 章节仅为历史记录；当前摘要为 168 snapshots、110 warnings、7 HIGH/4 MEDIUM，E01–E20 五类索引校验为 20/20。完整可移植材料见 [R01-06 main 归档](R01-06-controlled-audit-20260923/README.md)。
## 结论

R01-06 已满足。当前受控审计的关键身份、摘要、manifest、路由/交互矩阵、运行时布局和具体证据索引已归档到 [R01-06-host-evidence-20260916/](R01-06-host-evidence-20260916/)。六张精选图随仓库保存；完整截图集仍由归档说明中的固定 checkout 和审计命令重现，不依赖某台机器的绝对临时路径。

## 实际结果

| 项目 | 结果 |
| --- | --- |
| 代码身份 | c2399d7be9f723e77226619172be16778fe3646f |
| 运行时审计 | 161 个快照；0 Fidelity 警告；0 失败路由 |
| 风险摘要 | 0 HIGH；0 MEDIUM；80 条 INFO 均为已分类的滚动/可见性信息 |
| 静态关键计数 | View 10；Tab 32；Button/Toggle 246；DataGrid 14；ScrollViewer 34；条件 UI 247 |
| 具体索引 | 20 / 20 行，包含结果入口、40 位身份、样本和未验边界 |
| 精选图 | 壳层、首页标准/窄窗口、维护诊断、存档候选、任务中心，共 6 张 |

## 验证

- 当前 R01-03 隔离 RenderHarness Release 构建：0 warning / 0 error；审计命令成功返回 UI audit complete。`validate-ui-evidence-index.ps1` 对归档的 `EVIDENCE_INDEX.md` 输出 `rows=20, references=20/20, identities=20/20, samples=20/20, boundaries=20/20`。
- 当前审计生成 362 张图；归档保留壳层、首页标准/窄窗口、维护诊断、存档候选、任务中心 6 张精选图，完整图集仍按 README 在隔离 `.tmp` 中重现，不复制到证据目录。
- 已人工查看当前标准尺寸壳层、首页、维护诊断代表图；图片是受控 WPF 离屏暗色主题，不写成真实宿主呈现。`audit-metadata.json` 的 OutputRoot/ZipPath 已改为仓库相对/可再生临时路径，没有保留机器绝对路径。

## 边界

- 当前归档不宣称真实 Playnite Dashboard 嵌入、用户主题、物理 DPI、OS 输入/IME、物理滚轮、presented frame、ETW 或宿主性能。
- 本项没有执行备份、恢复、删除、迁移、下载、真实媒体/云端写入或诊断外发，也没有修改生产命令、绑定、picker、滚动条、取消/错误、恢复保护、有限列表或 net462 契约。
- UI_MANIFEST.json、UI_ROUTE_MAP.json、视觉树 JSON 与其余全量截图未纳入 Git；它们是由相同审计入口按 README 重现的临时输出。

## 下一步

R01-06 当前身份、摘要、manifest、索引和精选图已满足；下一可执行小批量为 R01-07 freshness baseline 增量更新，把本次 `c2399d7b` 归档身份写入后再推进 R01-08 跳过测试说明。

## 2026-09-23 当前身份复核

- 当前 `b5c7a6d423a4bf23004c3b080e133b3b0b065fa5` 的受控审计生成 `168` 个快照；`EVIDENCE_INDEX` 校验为 `20/20` 行、引用/身份/样本/边界均 `20/20`，`0` Fidelity、`0` 失败路由。
- 当前 summary 的真实风险为 `7` 条 HIGH（父子滚动冲突）和 `4` 条 MEDIUM（待归类动作栏纵向扩展）；精选图/manifest 的持久归档规则不变，但不再声称本次审计 `0 HIGH/0 MEDIUM`。
- 这仍是受控 WPF 离屏 logical DIP 证据；没有真实 Playnite Dashboard/UIA/物理 DPI/呈现帧/ETW/宿主性能身份，也没有执行真实存档、媒体、云端或诊断写入。

## 2026-09-23 clean-tree 当前归档

- 新的持久归档为 [R01-06-controlled-audit-20260923](R01-06-controlled-audit-20260923/README.md)，源码审计身份 `5fbfc869ecddec852440ac82b3b0cc94343f3d60`。它保存 summary、portable metadata、manifest、route map、fidelity matrix、layout report、20 项 evidence index 和六张人工查看过的代表图。
- 六张图分别为概览 standard/narrow、媒体待归类、维护诊断、存档历史、任务中心。全量 screenshot/JSON 集未入库，按归档 README 在隔离 `.tmp` 中重建。Metadata 不留下机器绝对输出目录或 ZIP 路径。
- 当前审计 `168` 快照、0 Fidelity、0 路由失败，真实保留 `7 HIGH / 4 MEDIUM`；索引五类校验都是 `20/20`。R01-06 freshness sourceCommit 绑定到该实际 audit commit，不把测试-only `38d5b7b2` 误作审计构建身份。
- 图像与所有结果仍是 synthetic DTO + WPF offscreen logical DIP；不宣称真实 Playnite 呈现、物理 DPI/跨屏、OS 输入/IME、presented frame、ETW 或宿主性能。
