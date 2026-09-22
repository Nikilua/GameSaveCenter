# R17-08 维护报告可读性

日期：2026-09-20  
代码提交：`59d4190b`（`完善维护报告结构与脱敏`）  
分支：`codex/ui-finesse-round2`

## 本阶段事实

先复用现有 `MaintenanceReportService` 的状态采集、`MaintenanceReportDto`、维护页复制/导出命令和 `maintenance.report.get` IPC，没有另建报告通道。原平铺文本现在按固定顺序组织为：

1. 软件身份：GameSaveCenter 插件版本/构建身份、Worker 版本/构建身份、Playnite 版本、IPC 协议、Windows 和 .NET 运行时。
2. 摘要：统一使用本次生成的本地时间，并重复报告正文的待处理、已验证、未知三类计数。
3. 待处理、已验证、未知：每个标题带与摘要相同的条目数；无条目时明确写出当前采集范围内没有项目。

Playnite 请求现在把插件和 Playnite 身份通过现有 IPC 传给 Worker；复制与导出仍使用同一个 `ReportText`，保存对话框、命令绑定和取消语义不变。所有最终报告文本统一经过 `MaintenanceReportRedactor`：保留 URL 主体但移除查询/片段参数，Windows `C:\Users\用户名\...` 只保留脱敏后的根路径，不把凭据、用户名称或 URL 参数写入报告。

## 行为与负例证据

- Worker 隔离报告生成/分组/时间一致性/身份透传与脱敏 `2/2`：实际调用 `MaintenanceReportService.GetAsync`，检查 `GeneratedUtc`、Summary、正文计数/时间、软件身份和四段标题。
- 脱敏负例实际覆盖 URL `token/user/fragment` 参数和 Windows 用户路径；断言秘密值与用户名称不再出现在输出，同时保留安全主体和脱敏占位。
- Playnite R17-08 报告 IPC/复制/导出接线 `4/4`；当前提交的 R17 与报告合并回归 `19/19`，Worker 对应合并回归 `5/5`。
- `validate-source.py` 通过；XAML 结构检查 `24/24`；`git diff --check` 通过。

## 构建与质量门禁

隔离 Release solution 构建成功：`0 errors`，仅有既存 `src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml.cs:664` 的 2 条 `CS8602` warning。WPF 质量扫描保持 `0 errors / 27 warnings / 162 info`；本阶段没有修改 Demo-first 视觉体系、游戏选框或滚动条系统。

## 边界与未验项

- 未运行真实 Playnite/package-host，因此未把真实导出文件夹、剪贴板、最终呈现、浅深主题、DPI/UIA/IME、屏幕阅读器、物理跨屏或宿主性能写成通过。
- 业务状态只使用合成/fake/隔离 SQLite 和临时目录；未读写真实存档、媒体、用户云端或外发诊断。
- 未绕过 ETW/系统跟踪权限；没有把离屏或代理结果当作 presented frame 或真实路径/权限证明。
- Demo 原目录不可用，沿用恢复生产基线。`main` 用户改动、`src.zip` 和未跟踪对话框文件未触碰、未合并。

`.tmp/r17-08-solution` 已在文档同步前清理。下一可执行任务：进入 `R18-01 连续输入基准`，先盘点现有 `GamePickerViewModel`、`DebouncedRefresh`、搜索/IME 夹具和可重复的大库合成基准。
