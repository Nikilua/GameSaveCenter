# R22-01 游戏来源诊断时间证据

日期：2026-09-22  
代码提交：`814d3e7a`（统一游戏来源诊断时间显示）

## 本批范围

本批先核对 `DashboardViewModel.GameDiscovery` 的已有诊断 DTO、实际 `MaintenanceView.xaml` 绑定和共享 `TimeDisplayFormatter`，只处理“游戏来源诊断”卡片中的三个真实用户可见时间入口：Worker 描述同步、最后匹配尝试和最近备份。

- 正文 `GameDiscoveryDiagnosticSummary` 改用相对时间，例如“3 分钟前”，不再直接拼接 `ToLocalTime()`。
- `GameDiscoveryDiagnosticFullSummary` 复用 `TimeDisplayFormatter.Full`，通过同一只读 TextBox 的 Tooltip 和 `AutomationProperties.HelpText` 暴露完整本地时区时间及可复制的 UTC 原值。
- DTO 缺少时间时，正文和完整提示均保持“未知”，不从默认值推导 1970 年或其他虚假时间。
- 诊断命令、当前筛选、单游戏同步/匹配、错误/取消和 Playnite/net462 兼容未改变；未新增服务或 DTO，也未读写真实存档、媒体或云端。

## 行为证据

`R22TimeDisplayBehaviorTests` 新增两个行为/负例：

- 固定 UTC 参考时间验证三个时间分别显示“3 分钟前”“2 分钟前”“1 分钟前”；完整投影同时包含 `TimeDisplayFormatter.Full` 和 `RawUtc`。
- 三个时间均缺失时正文和完整提示都保持“未知”，并核对生产 XAML 同时绑定 `GameDiscoveryDiagnosticFullSummary` 的 Tooltip/Automation HelpText。

本批定向结果：`R22TimeDisplayBehaviorTests 26/26`。

## 验证结果

- 隔离 Release 构建：XAML `24/24`，Playnite `net462`、Tests `net472`、Worker/Core 成功，`0 error / 2` 条既有 `MediaCenterView.xaml.cs:699 CS8602` warning。
- `python scripts/validate-source.py`：通过。
- `git diff --check`：通过。
- `scripts/validate_wpf_ui.py` 在当前 D 盘仓库不存在，执行结果为“无法打开文件”；因此本批没有新增 WPF 静态审查通过声明，沿用账本此前的历史 WPF 证据并保留该工具缺失事实。

## 未验边界与下一步

证据只来自合成 DTO、固定 UTC 时间、隔离 Release 输出和现有测试入口；未验真实 Playnite/package-host、Windows UIA/读屏、OS 输入/IME、DPI/物理跨屏、最终呈现帧、ETW 或宿主性能。Demo 原目录不可用，继续参考已恢复生产基线。报告/复制列/日志等需要保留稳定完整时间语义的入口没有由本批代签。

下一可执行任务：继续按实际绑定核对 `DashboardViewModel`/Contracts 其余 stale 或缓存时间入口；发现真实用户可见不一致后再按小批量复用同一 formatter。
