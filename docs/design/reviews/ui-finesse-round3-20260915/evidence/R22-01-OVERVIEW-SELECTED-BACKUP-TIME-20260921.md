# R22-01 Overview 选中游戏最近备份时间（2026-09-21）

## 本子批次结论

提交 `4e85d665` 收口 Overview 当前游戏卡片的“最新备份”时间入口。`R22-01` 整体仍为“部分满足，待继续”；本批只处理一个实际绑定到生产 XAML 的旧本地短日期，不代签其他 Save/恢复入口。

## 复用与实现

- `GameStatusDto.LastBackupUtc` 继续作为排序和业务判断原值，保留 `LastBackupLocal` 兼容投影；新增 `LastBackupRelativeDisplay`、`LastBackupFullDisplay`、`LastBackupRawUtcDisplay`，统一复用 `TimeDisplayFormatter`。
- `DashboardViewModel` 保留 `SelectedGameLastBackupDisplay`，新增选中项的相对、完整和原始 UTC 投影，并在快照加载、选中游戏变化时一起触发属性通知。未加载仍显示 `—`，未选中或没有备份仍显示 `暂无`；未知 DTO 时间保留“时间未知/未记录 UTC 时间”负例。
- Overview 当前游戏卡片正文改用相对时间；完整本地时区时间和 round-trip UTC 进入同一指标的 Tooltip 与 Automation HelpText。未改变游戏选框、`LastBackupUtc` 排序、备份/详情命令、页面滚动或 Playnite/net462 兼容。

## 实际验证

- `R22TimeDisplayBehaviorTests` `20/20`：已知时间保留旧本地投影并暴露相对/完整/原始 UTC，未知时间不伪造为当前时间；时间线 UTC 排序合同继续通过。
- `OverviewInteractionTests` `2/2`：隔离 STA WPF 实际读取 Overview 视觉树中的最新备份相对文本，并验证 Tooltip 与 Automation HelpText 为完整证据；既有云端卡片命令点击行为保持通过。定向命令合计 `22/22`。
- 精确提交身份 Release 隔离构建：XAML `24/24`；Playnite `net462`、Playnite Tests `net472` 均 `0 errors`，保留已有 `MediaCenterView.xaml.cs:671` 两条 CS8602 warning；`validate-source.py`、`git diff --check` 通过；WPF 静态检查 `0 errors / 27 warnings / 177 info`。

## 未验边界

- 业务验证仅使用合成 DTO、fake DataContext、隔离 STA WPF 和隔离构建目录；没有写真实存档、媒体、云端、用户目录或外发诊断。Demo 原目录不可用，继续以恢复生产基线为准。
- 未声称真实 Playnite/package-host、Windows UIA/读屏、真实剪贴板、系统时钟跳变/跨系统启动周期、DPI/跨屏、最终呈现、ETW 或宿主性能已验证；main 用户改动未碰、未合并。

## 下一步

继续 R22-01：盘点其他 Save/恢复入口仍直显的旧本地时间，先核对实际绑定、已有 DTO/复制入口和稳定排序，再按小批量补行为与未知负例。
