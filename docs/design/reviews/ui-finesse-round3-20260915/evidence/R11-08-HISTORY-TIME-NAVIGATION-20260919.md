# R11-08 历史时间导航证据

日期：2026-09-19  
分支：`codex/ui-finesse-round2`  
实现提交：`8cc329e4`（补齐存档历史时间导航）

## 完成内容

- 先复用 `BackupVersionDto.CreatedUtc`、`CreatedLocal`、`BackupId` 和现有历史表排序，没有新增 DTO、存储、Worker IPC 或真实归档读取链路。
- `BackupHistoryDateRange` 提供全部时间、今天、昨天、近 7 天和近 30 天选项。活动范围按本地日历日期比较 `CreatedLocal`，因此不把夏令时切换误算成固定 24 小时窗口；未知时间在活动范围中排除，在“全部时间”中保留。
- 历史列表使用独立 `CollectionViewSource` 过滤视图，仍保留 `Backups` 作为真实来源，避免破坏 A/B 对比选择器和既有绑定契约。清除范围恢复全部历史；“最近/更早”命令只在当前可见范围内跳转并更新选中版本。
- 同秒版本按 `CreatedUtc` 后接稳定 `BackupId` 排序，未知时间始终置后，避免本地显示顺序因集合刷新而漂移。

## 验证

- D 盘隔离源码副本 `D:\workplace\github\GameSaveCenter\.tmp\r11-08-source\build`：Release solution `0 warning / 0 error`，Playnite `net462` 与测试 `net472` 均从同一输出生成；XAML 门禁 `24/24`。
- 定向组合：`R11HistoryTimeNavigationBehaviorTests 3/3`、`R06SortingBehaviorTests 4/4`、`R11VersionSummaryBehaviorTests + R11ProtectionBehaviorTests 4/4`、`WpfUiResourceDictionaryTests 137 passed / 39 skipped / 0 failed`；合计 `148 passed / 39 skipped / 0 failed`。
- `scripts/validate-source.py`、`scripts/check-xaml.ps1` 和 `git diff --check` 作为源码、XAML 与差异门禁执行通过。
- 行为夹具覆盖本地今天/昨天/7 天范围、未知时间正负例、清除恢复全部、同秒稳定排序、独立过滤视图和 UI 命令/绑定契约。

## 边界与下一项

- 以上使用合成 `BackupVersionDto`、fake/隔离 STA WPF 与隔离输出目录；未验真实 Playnite/package-host 安装及 presented frame、物理 DPI/跨屏、UIA/读屏、IME、ETW 或宿主性能，也未读取/写入真实存档、媒体、云端或外发诊断。
- Demo 原目录不可用，视觉判断沿用已恢复的生产基线；游戏选框、滚动条系统、命令绑定、取消/错误语义、恢复保护和有限列表约束保持不变。
- main 的 DEV-INSTALL-008 失败事实仍独立为 Release `0/0`、Core `83/83`、Worker `311/311`、Playnite `73 failed / 588 passed / 57 skipped`、安装器退出 `1`，不由本证据改写。
- 下一可执行小批量：R12-01 恢复分步摘要；先核对已有恢复 DTO、任务状态与取消/错误语义，再决定补证据还是做最小缺口修复。
