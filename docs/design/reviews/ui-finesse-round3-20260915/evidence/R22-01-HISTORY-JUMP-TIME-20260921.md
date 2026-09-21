# R22-01 历史跳转状态时间证据

日期：2026-09-21

## 本子批范围

本批只处理 SaveCenter 历史列表“跳到最近版本/跳到较早版本”后的 Dashboard 状态栏反馈。日期范围筛选仍按本地日历日期运行，导航仍按 UTC 时间和稳定 BackupId 排序；插件菜单“查看备份历史”通知、复制列和内部日志/导出字段不在本批范围。

## 已核对与实现

- `DashboardViewModel.BackupHistory` 的跳转正文复用 `BackupVersionDto.CreatedRelativeDisplay`，保留“最近/较早”、选中版本和无可跳转提示。
- 状态栏新增 `StatusMessageFullDisplay`：普通状态消息默认回退原文；历史跳转显式将完整本地时间和 round-trip UTC 放入 Tooltip/Automation HelpText。
- `CreatedUtc` 的本地日历筛选、导航排序、稳定 ID、游戏选框、历史列表滚动和命令门控保持；未知 `DateTime.MinValue` 不伪造时间。

## 证据

- `R11HistoryTimeNavigationBehaviorTests 4/4`：本地日历范围、同秒稳定排序/未知置后、生产绑定门禁，以及最近/较早/未知时间的相对正文与完整证据。
- 隔离 Release 构建：XAML `24/24`；Playnite `net462` 与 Tests `net472` `0 errors`；仅既有 `MediaCenterView.xaml.cs:671` 的 2 条 CS8602 warning。
- `scripts/validate-source.py`、`git diff --check` 通过；WPF 静态基线为 `0 errors / 27 warnings / 177 info`。

## 边界

验证仅使用合成 `BackupVersionDto`、隔离构建和源码/绑定契约，没有真实存档、媒体、云端或诊断写入。未声称真实 Playnite/package-host、Windows UIA/读屏、真实系统输入、DPI/跨屏、presented frame、ETW 或宿主性能已验证；Demo 原目录不可用。插件快速历史通知没有 Tooltip/Automation 容器，本批保留其独立设计和旧完整日期语义，下一子批再决定通知正文/证据呈现；复制列及内部日志/导出继续保持机器可读完整时间。
