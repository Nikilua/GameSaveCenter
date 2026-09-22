# R17-08 维护报告可读性定向复核

日期：2026-09-23  
当前复核提交：`fc8dffa5`（`校正R17-07检查项定位证据`）  
实现提交：`59d4190b`（`完善维护报告结构与脱敏`）  
分支：`codex/ui-finesse-round2`

## 结论

R17-08 在当前可控范围内继续记为“已满足，待环境验证”。本次没有新增生产代码，按当前检出版本重新构建并复用既有 `MaintenanceReportService`、`MaintenanceReportDto`、`maintenance.report.get` IPC 及维护页复制/导出命令；旧摘要中的测试口径按当前源码实际数量校正。

- Worker `MaintenanceReportServiceTests`：`3/3`。覆盖用户可读分组与统一生成时间/身份透传、1 B 镜像单位保持为 `1 B`、URL 参数和 Windows 用户路径脱敏负例。
- Playnite `MaintenanceReportSourceTests`：`4/4`；其中 R17-08 报告 IPC/复制/导出接线为 `2/2`，同类维护行动项/保留预览回归为 `2/2`。
- Playnite 完整 R17 筛选：`15/15`；若按报告源类合并计数为 `19/19`（R17 `15/15` + `MaintenanceReportSourceTests 4/4`）。

## 行为证据

- 报告固定输出软件身份、摘要、待处理、已验证、未知四段；摘要计数与正文段落数量来自同一批采集结果，无条目时写明当前采集范围内没有项目。
- `GeneratedUtc` 与报告正文的本地生成时间来自同一次生成；插件版本、插件构建身份和 Playnite 版本由现有 IPC 请求透传，Worker 身份、协议、Windows 和 .NET 运行时同时输出。
- 复制和导出继续使用同一个 `ReportText`，沿用现有剪贴板重试、保存对话框、命令绑定、取消/错误语义；本阶段没有另建报告通道。
- `MaintenanceReportRedactor` 保留 URL 主体但移除查询/片段参数，把 `C:\Users\用户名\...` 显示为 `C:\Users\[用户]...`；断言秘密 token、URL 用户名和 Windows 用户名不出现在报告中。
- 报告只采集 Worker 状态摘要，不写真实存档、媒体、用户云端或外发诊断；行为使用合成 DTO、fake 服务、隔离 SQLite 和临时目录。

## 构建与质量门禁

- 当前检出版本隔离 Release solution：`0 errors / 2 warnings`；两条均为既有 `src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml.cs:706` 的 `CS8602`，不是本阶段新增。
- 目标 Playnite 产物仍为 `net462`，测试程序集为 `net472`；没有覆盖 `main` 的旧实现或用户文件。
- `validate-source.py`：通过；XAML 结构校验：`24/24`；`git diff --check`：通过。
- `validate_wpf_ui.py src/GameSaveCenter.Playnite`：`0 errors / 28 warnings / 162 info`。警告/信息为已有 Canvas、StackPanel/ScrollViewer 和主题资源审查提示，不等价运行时视觉通过。

## 未验边界

Demo 原目录不可用，本阶段沿用已恢复的生产基线。未运行真实 Playnite/package-host、真实导出文件夹、剪贴板或用户选择对话框，因此未宣称最终浅深主题、DPI/UIA/IME、屏幕阅读器、焦点、物理跨屏、presented frame、真实权限和宿主性能通过；未绕过 ETW/系统跟踪限制。

## 下一步

下一可执行任务为 `R18-01 连续输入基准`：先盘点游戏选框搜索、`DebouncedRefresh`、IME 夹具和大库合成基准，区分 VM 查询性能与真实窗口呈现/宿主性能，不把代理堆测量当作 ETW 或 presented frame 证据。
