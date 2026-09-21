# R22-01 快速历史通知时间证据

日期：2026-09-21

## 本子批范围

本批处理 Playnite 游戏菜单“查看备份历史”快速操作的通知行。该入口由 `ShowInfo` 直接提供正文，没有可绑定的 Tooltip/Automation 容器，因此沿用通知正文承载相对时间和完整时间；复制列与仅内部日志/导出字段保持独立。

## 已核对与实现

- `GameSaveCenterPlugin.ShowBackupHistoryQuickActionAsync` 复用 `BackupVersionDto` 的共享时间投影；每条通知行显示相对时间，并在同一行保留完整本地时间、时区偏移和 round-trip UTC。
- 备份数量、大小、恢复可用性、最多 20 行、ForceRefresh 查询、稳定 PlayniteId 和只读查询语义保持；没有新增 Worker 服务、存档写入、媒体删除、云端写入或通知体系。

## 证据

- `QuickActionSourceTests 2/2`：合成 `BackupVersionDto` 实际调用快速历史格式化方法，验证相对时间、完整时间、大小和恢复可用性同时保留；既有菜单/IPC 来源契约也通过。
- 隔离 Release 构建：XAML `24/24`；Playnite `net462` 与 Tests `net472` `0 errors`；仅既有 `MediaCenterView.xaml.cs:671` 的 2 条 CS8602 warning。
- `scripts/validate-source.py`、`git diff --check` 通过；WPF 静态基线为 `0 errors / 27 warnings / 177 info`。

## 边界

验证使用合成 DTO 和隔离构建，没有真实 Playnite 菜单宿主、真实通知呈现、存档、媒体、云端或诊断写入证据；Demo 原目录不可用。由于 `ShowInfo` 没有本批可用的 Tooltip/Automation 容器，完整证据以通知正文同行展示，不声称 Windows UIA/读屏、presented frame、DPI/跨屏、ETW 或宿主性能已验证。复制列和内部日志/导出字段下一子批继续核对其稳定机器可读语义。
