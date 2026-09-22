# R22-01 恢复确认框时间

日期：2026-09-21  
代码提交：`c5dc47fc`（`统一恢复确认时间显示`）

## 本批范围

核对 `DashboardViewModel.RestoreAsync` 的实际原生确认消息，确认历史版本创建时间不是仅以固定本地时间直显。复用 `BackupVersionDto` 已有的 `CreatedRelativeDisplay` 与 `CreatedFullDisplay`，将确认正文的版本行改为“相对时间（完整本地时间与 round-trip UTC）”，不改变确认、二次选中项核对、PreRestore 保护、取消/错误和恢复命令提交语义。

## 行为证据

`R22RestoreConfirmationTimeBehaviorTests` 直接调用生产 `DashboardViewModel.BuildRestoreConfirmation`，覆盖：

- 合成历史版本的相对时间与完整本地/UTC 时间同时进入确认正文。
- 原有来源、系统、可恢复性摘要以及“恢复前创建并锁定 PreRestore、确认游戏关闭”的安全上下文仍在正文中。
- `CreatedUtc` 未知时正文显示“时间未知（时间未知）”，不伪造日期。
- 旧的“完整本地时间后直接接备份类型”格式不再生成。

## 验证结果

- `R22RestoreConfirmationTimeBehaviorTests`、`R12RestoreRevalidationBehaviorTests`、`R22TimeDisplayBehaviorTests`：`28/28`
- Release 隔离构建：XAML `24/24`，Contracts/Playnite net462、Tests net472、Worker `0 errors`；构建仍报告既有 `MediaCenterView.xaml.cs:671 CS8602`，未修改。
- `python scripts/validate-source.py`：通过。
- WPF 静态审查：`0 errors / 27 warnings / 177 info`。
- `git diff --check`：通过。

## 未验边界

确认消息通过生产 `ConfirmAsync` 进入原生/宿主确认路径，但本批未启动真实 Playnite/package-host，也未把隔离 testhost 作为最终呈现、Windows UIA/读屏、DPI/跨屏或输入法证据。没有真实存档、媒体、云端、恢复写入或外发诊断；Demo 原目录不可用，参考已恢复生产基线。完整本地/UTC 显示的最终字体和跨时区呈现仍待真实宿主回放。
