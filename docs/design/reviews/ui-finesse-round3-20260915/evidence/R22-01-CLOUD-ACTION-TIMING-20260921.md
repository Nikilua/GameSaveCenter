# R22-01 CloudTransfer 维护动作摘要时间证据

日期：2026-09-21（Asia/Shanghai）  
代码提交：`47bc1d00`（基于已推送的 `f1e568d0`）  
范围：维护页行动项中 CloudTransfer 的上次尝试、下次尝试及摘要条目。

## 现有能力与本批改动

- 先复用 `CloudTransferStatusDto` 已有的 `LastAttemptRelativeDisplay`、`LastAttemptFullDisplay`、`RetryTimingRelativeDisplay` 和 `RetryTimingFullDisplay`；没有新增重试服务、云端请求、分页或队列状态源。
- `MaintenanceActionItem.TimingDisplay` 对 CloudTransfer 使用相对时间；`TimingFullDisplay` 为 Tooltip 和 `AutomationProperties.HelpText` 提供完整本地时间与 round-trip UTC 证据。HealthInspection 和 RetentionQuarantine 的原有摘要格式保留，并通过缺省回退避免未迁移项目出现空提示。
- `CloudTransferSummaryDto` 增加维护摘要需要的相对/完整/原始下次计划投影。行动项对未来计划显示“约……后”，过去计划显示“可立即重试”，无计划摘要显示“按队列状态”；认证/失败/校验失败等没有计划的记录仍显示“需处理后再试”，不伪造下一时间。
- `LastAttemptDisplay`、`NextAttemptDisplay` 等旧字段仍存在；队列分页、筛选、重试/校验、脱敏、有限预览、行动分组和 `OpenCloudQueue` 命令参数均未改变。

## 受控验证

- `MaintenanceCloudTransferResolverTests | MaintenanceReportSourceTests`：`11/11` 通过。行为测试实际断言 CloudTransfer 摘要只显示尝试/重试，不误显示远端校验；完整提示含 UTC 原值；摘要未知状态和过去时间分别保持“按队列状态 / 可立即重试”。源码门禁只用于确认 ViewModel/XAML 接线，未单独作为交互签收。
- `R22TimeDisplayBehaviorTests`：`18/18`；`UiDisplayMappingTests`：`40/40`。
- Release 隔离构建：XAML `24/24`，Playnite/Tests `0 errors`；仅保留既有 `MediaCenterView.xaml.cs:671` 的 2 条 CS8602 warning。`validate-source.py`、`git diff --check` 和 WPF 静态检查 `0 errors / 27 warnings / 177 info` 通过。

## 边界与下一步

证据来自合成 DTO、fake/隔离测试宿主和隔离构建目录；没有写真实存档、媒体、云端或外发诊断。没有把静态绑定、离屏行为或代理结果写成真实 Playnite/package-host、最终呈现、UIA/读屏、OS 输入/IME、DPI/跨屏、ETW 或宿主性能通过。Demo 原目录不可用，main 工作区用户改动未触碰、未覆盖、未合并。

R22-01 仍为“部分满足，待继续”。下一可执行小批量是盘点 Storage 及其他仍直显旧本地时间的 Save/恢复入口，优先复用已有 DTO/绑定并补未知/兼容负例。
