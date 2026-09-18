# R07-02 锚点删除回退验收证据

日期：2026-09-18  
分支：`codex/ui-finesse-round2`  
代码提交：`7acb61a5976196c7e5add347920762912e1149ea`  
测试补充提交：`ef53a7486d0c5cc5111d7f302d184fbe1a1f201d`

## 结论

R07-02 在当前可控范围内已满足。刷新、删除、筛选和加载更多路径先按稳定业务键恢复对象；稳定对象不存在时，按刷新前行位夹到邻近项，不再无条件跳首行，也不把新列表同索引的其他对象误当成原对象。任务导航目标仍优先于删除后的行位回退，云端队列在一致性分页尚未结束时继续保留稳定键并加载后续页。

## 现有路径核对与实现

- 新增共享 `SelectionAnchorResolver`：稳定键命中优先；未命中时以旧索引夹到 `[0, Count - 1]`；空集合返回空。它只处理选择恢复，不重建集合、不改变分页、虚拟化或滚动条。
- Task 全量快照与历史分页在 `Replace` 前记录 `TaskId + 旧索引`；任务导航目标仍在稳定键失败后优先解析，随后才使用邻近行。保留既有取消、错误、过滤和显式导航语义。
- Media 主库与 Media Inbox 的刷新/筛选/加载更多分别按 `MediaId + 旧索引` 恢复；Inbox 模式切换继续沿用当前模式集合和既有首项初始化，只在已有选择被删除时使用邻近项。
- Dashboard Findings 使用 `PlayniteId + Code + Title` 组合键；进程映射使用 `ExecutableName`；云端队列使用 `TransferKey`，一致性分页仍保留 pending key 直到目标页到达或确定不存在。
- Save 历史按 `BackupId` 恢复；候选按既有 `PlayniteId + Path` 恢复，找不到时若存在旧索引则选邻近项，否则继续保留 R06-08 的 Pending/首项初始化约定。
- 以上修改没有替换游戏选框、滚动条系统、DataGrid `CanContentScroll`/虚拟化、命令/Binding、取消/错误/恢复保护或 net462 兼容路径；没有从 main 覆盖旧实现。当前 checkout 仍不存在原始 Demo `DesignShellView.xaml`/`Pages`，视觉核验沿用恢复生产基线。

## 行为与回归证据

- `R07SelectionAnchorBehaviorTests 4/4`：稳定 ID 在位置变化时仍优先；稳定对象删除后选择邻近项而非首项；实际 STA WPF `DataGrid` 在删除行后应用恢复结果并保留邻行；生产 `RestoreSaveCandidateSelection` 覆盖稳定路径删除后的邻近回退。
- 最终相邻回归 `20/20`、0 skipped：包含 `R07ScrollOwnershipBehaviorTests`、`R06DetailsBudgetBehaviorTests`、`TaskCenterViewResponsiveTests`、`MediaInboxGeometryTests`、`DetailsDisclosureSourceTests`、`PurposeNavigationSourceTests` 与 R07-02 行为测试。源码测试程序集在清理 Release 输出后重新构建，提交身份与当前 HEAD 一致。
- Release 标准构建：XAML `24/24`，生产 net462 与测试 net472 `0 warning / 0 error`；`validate-source.py` 通过；`git diff --check` 通过。

## 离屏渲染与视觉核验

报告：`.tmp/r07-02-anchor-final/render-qa-report.txt`

- 报告绑定 `ef53a7486d0c5cc5111d7f302d184fbe1a1f201d`，`WorkingTreeClean: True`，Light/Dark、多尺寸（1040×700、1100×720、1366×768、2560×1440 等）及 `2560×1440 → 1100×720 → 2560×1440` resize 均 `render-qa OK`。
- 报告覆盖 50/400/2000/4468 合成数据量、页面/表格/详情滚动容器、云端筛选控件和生产 PageHost 几何；1040×700 Save/Media/Maintenance/Task 分别保留 `4/4`、`6/4`、`5/4`、`5/4` 可读行，1366×768 Task 为 `4/4`。
- 已查看 `Task-1040x700.png`、`Media-1040x700-tab0.png`、`Maintenance-1040x700-tab0.png`，未发现本阶段造成的结构、裁剪或主题异常。

## 边界与下一步

行为证据使用合成 DTO、隔离 STA WPF Window、生产共享恢复器和 offscreen logical DIP（报告 `DpiScale=1.00`）。没有启动真实 Playnite/Worker，也没有真实鼠标/触控板、键盘/IME、UIA/读屏、物理 DPI/跨屏、presented frame、ETW、宿主性能或真实服务失败/分页时序；未写真实存档、媒体、云端或诊断数据。渲染中的布局/帧时间不被解释为真实宿主性能或物理呈现。

下一可执行任务为 R07-03「短窗底栏可达」：在 1040×700 及更短内容区核对顶部提示条叠加后的保存、取消、加载更多可达性，继续复用现有滚动条和生产布局基线。
