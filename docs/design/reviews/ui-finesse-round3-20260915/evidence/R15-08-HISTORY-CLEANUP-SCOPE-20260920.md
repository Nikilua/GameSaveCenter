# R15-08 清理历史范围证据

日期：2026-09-20  
既有实现：`88bde5de`、`a841e42c`、`77d5f346`  
本批证据夹具：`a07f0518`  
分支：`codex/ui-finesse-round2`

## 已满足

- 先核对现有 `RetentionSimulationService`、`RetentionSimulationPreviewDto`、SQLite 隔离账本和维护页，没有重建清理服务。全局只读预览返回 `PreviewId`、生成时间、现有/保留/候选数量、预计释放、锁定/健康恢复点/PreRestore 影响统计和隔离账本占用；候选明细保留 UTC 日期、用户本地日期显示、归档路径和候选原因。
- Maintenance 页面实际绑定候选的 `CreatedDisplay`、`Reason` 和完整影响摘要；候选列表有限为最多 200 条，容器 `MaxHeight=240`，应用入口明确要求二次确认。预览摘要明确“预览只读，清理不会自动执行”。
- 执行只接受明确 `Confirmed=true` 和 Worker 持有的预览句柄；预览超过 10 分钟、生成时间不一致、候选/策略/归档身份变化都会拒绝执行并要求刷新。执行前按游戏取得与备份、恢复、媒体共用的 `GameOperationKind.Retention` 锁，忙碌游戏计入 `SkippedBusyCount` 并跳过，不删除运行中的任务记录。
- 清理对象限定为配置备份根目录下的 ZIP 候选；锁定版本、PreRestore、健康恢复点、缺失/不支持/身份变化对象不会被误删。移动前先写入持久化隔离账本，索引删除和物理文件处理失败会留下恢复状态；Worker 重启后可按账本逐条恢复或人工确认。

## 实际证据

- 外部隔离源码副本绑定当前提交 `a07f0518d135226e1b7d1422b32f3f4cd5ffa1ff`：`RetentionSimulationServiceTests` 与 `RetentionQuarantineRecoveryTests` 合计 `16/16`；覆盖日期/原因/影响摘要、保护对象负例、二次确认、预览过期/状态变化、共享游戏锁忙碌跳过、归档身份校验、隔离账本恢复和人工确认边界。
- Playnite `net462` 构建并运行 `MaintenanceReportSourceTests`：`3/3`；覆盖维护页日期/原因/摘要绑定、有限滚动、应用命令和 Worker 忙碌/预览过期/恢复账本门禁。构建保留 `MediaCenterView.xaml.cs:664` 的 2 条既有 nullable warning，无新增错误。
- `python scripts/validate-source.py`、XAML `24/24`、`git diff --check` 通过；本批没有生产 XAML 改动，沿用上一批 WPF 静态质量结果 `0 errors / 28 warnings / 162 info`。
- 首次相邻源码测试发现一条因 R15-07 剪贴板重试入口更新而陈旧的 `Clipboard.SetText` 断言，已改为当前 `ClipboardRetry.TrySetTextAsync` 入口并重新通过；不是生产失败。

## 边界与清理

- 还原/构建的 `NU1900` 仅表示离线 NuGet 漏洞源不可访问；未宣称完整 solution、真实 Playnite/package-host、RenderHarness presented frame、物理 DPI/跨屏、UIA/读屏、IME、ETW 或宿主性能已验证。Demo 原目录不可用，继续沿用恢复的生产基线。
- 业务验证仅使用合成版本、fake/隔离 SQLite、临时归档和隔离锁；没有修改真实存档、媒体、用户云端、诊断或系统剪贴板，也没有删除真实历史。当前外部源码副本 `D:\workplace\github\GameSaveCenter\.tmp\r15-08-source` 已清理；其他既有 `artifacts/`、`.tmp/` 未碰。
- `main` 工作树的用户 `DashboardView.xaml.cs`、恢复基础设施、R08 测试和 `src.zip` 未触碰、未覆盖、未合并。

下一可执行任务：`R16-01 设置搜索定位`；先核对现有设置页筛选/分组/滚动与命令绑定，再补最小定位行为证据。R15-08 的真实宿主呈现和性能边界仍待可用 Playnite 流程验证。
