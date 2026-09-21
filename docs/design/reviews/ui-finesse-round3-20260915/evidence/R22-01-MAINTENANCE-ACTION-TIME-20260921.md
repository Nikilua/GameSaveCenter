# R22-01 维护摘要时间

日期：2026-09-21  
代码提交：`df689dc1`（`统一维护摘要时间显示`）

## 本批范围

核对 Maintenance 诊断概览的实际“下一步运维”行动摘要，发现恢复巡检动作和隔离账本动作仍把用户可见时间填成旧本地格式。复用 `HealthInspectionStateDto` 已有相对/完整投影，并让隔离账本更新时间在行动摘要正文使用相对时间、Tooltip/Automation HelpText 使用完整本地/UTC 证据；保留分页、清理账本、Worker 协调、人工确认和非破坏性巡检语义。

## 行为证据

`R22MaintenanceActionTimeBehaviorTests` 覆盖：

- 恢复巡检行动的最近完成、最近成功、下轮计划正文分别使用相对时间，完整字段进入 `TimingFullDisplay`。
- 隔离账本行动的 `UpdatedUtc` 正文使用相对时间，完整字段进入 `TimingFullDisplay`。
- 实际 STA WPF `MaintenanceView` 的“诊断概览”行动摘要 TextBlock：正文读取 `TimingDisplay`，Tooltip 与 `AutomationProperties.HelpText` 读取 `TimingFullDisplay`。
- 保留已有云端动作的相对/完整时间合同；更新 R17 旧源码断言，使其验证当前生产映射而非旧本地格式。

## 验证结果

- `R22MaintenanceActionTimeBehaviorTests`、`MaintenanceCloudTransferResolverTests`、`R17HealthInspectionBudgetTests`、`R22RestoreConfirmationTimeBehaviorTests`、`R22TimeDisplayBehaviorTests` 定向合计：`38/38`
- Release 隔离构建：XAML `24/24`、Contracts/Playnite net462、Tests net472、Worker `0 warnings / 0 errors`；既有 MediaCenter CS8602 未修改。
- `python scripts/validate-source.py`：通过。
- WPF 静态审查：`0 errors / 27 warnings / 177 info`。
- `git diff --check`：通过。

## 未验边界

证据来自合成维护动作、fake DataContext、隔离 STA WPF、隔离 Release 构建和测试目录；未启动真实 Playnite/package-host，也未宣称真实 UIA/读屏、最终呈现、系统时钟跳变、DPI/物理跨屏、ETW 或宿主性能。没有真实存档、媒体、云端、账本协调、恢复写入或外发诊断；Demo 原目录不可用，参考已恢复生产基线。
