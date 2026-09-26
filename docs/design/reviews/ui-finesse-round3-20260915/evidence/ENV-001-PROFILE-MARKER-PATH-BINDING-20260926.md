# ENV-001 隔离 profile 标记路径绑定复核（2026-09-26）

## 发现与修复

独立复核 `scripts/PlayniteHostIsolation.ps1` 时发现，旧 profile marker 仅验证仓库根路径和 GUID。若 marker 被复制到同仓库 `.tmp` 下的另一个已有目录，那个目录会被当作已登记 profile 接受，削弱了“拒绝接管未知非空目录”的守卫。

marker schema 升至 `2`，现在同时记录并校验规范化后的绝对 `ProfilePath`、仓库根路径和 GUID。旧 schema 或路径不匹配均 fail-closed，不自动迁移、不删除、不接管原目录；需使用新的隔离 profile 路径。测试将有效 marker 复制到另一个含哨兵文件的目录，确认初始化拒绝且哨兵保持原样；原目录仍能通过 marker ID 稳定复用。

## 验证

- `scripts/tests/Test-PlayniteHostIsolation.ps1`：通过，包含 marker 正常复用、复制到另一目录的拒绝/哨兵保留、数据库越界、output 防覆盖和 reparse 负例。
- `DiagnosticsEvidenceSourceTests`：Release `8/8`，0 失败/跳过。编译保留两条既有 `MediaCenterView.xaml.cs:703 CS8602` warning。
- 首次脚本回归暴露测试清理在 Windows PowerShell 5.1 上对 junction 使用 `Remove-Item` 会抛 `NullReferenceException`；测试夹具改用 `[System.IO.Directory]::Delete(path, $false)` 只移除 link 本身，之后整组通过。一次 C# 测试启动因传入的 build identity 与 checkout HEAD 不一致而按身份门拒绝；用当前完整 HEAD 重建重跑为 `8/8`。
- PowerShell helper 测试临时目录和 output 均由测试 `finally` 清理；本批未安装/启动 Playnite，未访问用户 profile、存档、媒体、云端或用户剪贴板。

## 未改变的验收边界

这只加固 ENV-001 的本地路径身份守卫，不证明 Playnite 实例隔离或宿主行为。当前 WMI 命令行查询权限拒绝、既有 CEF `platform_channel 0x5` 启动失败证据没有变化；不重试相同启动、不绕过权限，ENV-001 继续 `BLOCKED_ENVIRONMENT`。Round3 R 台账仍是 192 个唯一 ID、状态计数 `106/83/1/1/1`，没有新增、重分类或签收 R 项。
