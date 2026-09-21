# R22-01 Save 与 Maintenance 时间入口（2026-09-21）

## 本子批次结论

提交 `2be8627db39304a2a3f929a797799c5c366b28c0`、`c211a04f5c28b400bac976cfc2223cae2a709299`、`0db4abe7`、`058ca8ff`、`44023ad6`、`0df6c69e` 和 `6a27ef42` 完成 Save 历史/详情、Maintenance 审计表、恢复可用性检查时间、Local Mirror 最近同步时间、恢复巡检时间、首次环境检查时间及云端队列选中详情证据时间的显示接入。R22-01 整项仍为“部分满足，待继续”，保留其他时间入口和真实宿主边界，未修改真实存档、诊断写入或用户目录。

## 复用与实现

- 复用 `TimeDisplayFormatter`，为已有 `BackupVersionDto`、`AuditLogEntryDto` 增加相对时间、带本地时区偏移的完整时间和原始 UTC 文本；没有复制第二套格式化逻辑。
- Save 历史表和选中版本详情改用相对时间，既有列宽、选择/分页、DataGrid 复制、恢复/校验/路径命令和滚动系统保持；时间单元格与详情提供完整时间 Tooltip/UIA HelpText。
- Maintenance 审计表改用相对时间，完整时间进入共享时间单元格的 Tooltip/UIA HelpText；诊断表、复制诊断、健康报告和其他维护命令未改动。保留预览中的 `CreatedDisplay` 留作下一边界，不提前扩大本批范围。
- `c211a04f` 仅校正一个随绑定迁移而过时的 Save 源码断言：从旧 `CreatedLocal` 改为 `CreatedRelativeDisplay`，并保留完整时间存在性校验。
- `BackupVersionDto.RestoreReadinessCheckedDisplay` 保留旧的本地时间、结果较旧提示和“尚未检查”语义；新增相对/完整显示，SaveCenter 所选版本详情改用相对时间，完整时间进入 Tooltip/UIA HelpText。恢复校验命令、选中版本、恢复保护和滚动系统未改。
- `LocalMirrorStatusDto.LastSyncDisplay` 保留旧的本地时间和“尚未同步”语义；新增相对/完整/原始 UTC 显示，Maintenance 镜像状态卡片改用相对时间，完整时间进入 Tooltip/UIA HelpText。镜像刷新/同步命令和“绝不删除镜像中的多余文件”语义未改。
- `HealthInspectionStateDto` 保留 `LastSuccessfulLocalDisplay`、`LastCompletedLocalDisplay`、`NextDueLocalDisplay` 与 `NextPlanDisplay` 兼容属性；新增相对/完整/原始 UTC 投影，Maintenance 恢复巡检卡片改用相对时间，完整计划与最近时间进入 Tooltip/UIA HelpText。停用、尚未成功、尚未完成、待安排、取消、单次预算和已有维护命令保持；`LastAttemptDisplay` 旧动作摘要路径未迁移。
- `EnvironmentCheckReportDto` 保留 `CheckedLocalDisplay` 与“尚未检查”语义；新增相对/完整/原始 UTC 投影，首次环境检查卡片改用相对时间，完整值进入 Tooltip/UIA HelpText。`EnvironmentCheckService` 的非破坏性检查、完成/跳过条件、手动测试备份和运行检查命令保持；本批未扩大真实目录或存档操作。
- `CloudTransferStatusDto` 保留 `LastAttemptDisplay`、`LastSuccessfulVerificationDisplay` 和云端状态/重试合同；新增最后尝试与最后成功校验的相对/完整/原始 UTC 投影，选中队列详情改用相对时间，完整值进入 Tooltip/UIA HelpText。`RetryTimingDisplay`、维护动作摘要和云端分页筛选仍未在本子批次扩大范围。

## 实际验证

- `R22TimeDisplayBehaviorTests` `17/17`：共享 formatter、任务/活动、BackupVersion/AuditLog DTO、恢复校验、Local Mirror、HealthInspection、EnvironmentCheck 和 CloudTransfer 详情时间的完整/原始 UTC 合同，以及 Save/Maintenance 绑定入口；云端/Health/Environment/时间/维护定向 `39/39`；Worker `EnvironmentCheckServiceTests 1/1`；`MaintenanceReportSourceTests | R17FindingTriageBehaviorTests` `7/7`。
- `R06SortingBehaviorTests` `4/4`、`R11HistoryTimeNavigationBehaviorTests` `3/3`；该定向批次合计 `27/27`，恢复校验与 Local Mirror 的旧 stale/unknown 负例和新绑定均实际覆盖。
- 精确代码提交身份 `44023ad6` 的隔离 Release 构建：XAML `24/24`，Playnite `net462` 与 Playnite Tests `net472` `0 errors`；`validate-source.py`、`git diff --check` 和 WPF 静态检查 `0 errors / 27 warnings / 177 info` 通过。联合回归为 `32 passed / 3 failed / 0 skipped`（总计 35），3 条失败仍是既有分类证据、分类选择和 Sidebar 版本文本断言漂移，未改写为通过。
- 全套 `WpfUiResourceDictionaryTests` 记录为 `133 passed / 39 skipped / 4 failed`；本批相关 Save 旧绑定断言已由 `c211a04f` 校正。剩余 3 条失败分别是既有 Settings 响应字段断言、媒体空数据滚动断言和 Inbox 下拉共享模板断言，与本批时间字段无关，未改写为通过。

## 未验边界

- Maintenance 保留预览和其他维护日期入口，以及其他 Save/恢复入口尚未全部接入；真实剪贴板、系统时钟跳变/跨系统启动周期、Playnite/package-host、Windows UIA/读屏、OS 输入/IME、DPI/跨屏、最终呈现、ETW 和宿主性能未验。
- 代理/离屏 WPF 结果不替代真实宿主呈现或物理跨屏证据；Demo 原目录不可用，继续使用恢复生产基线。业务验证使用合成 DTO、fake/隔离测试宿主，未写真实存档、媒体、云端或诊断；main 用户改动未碰、未合并。

## 下一步

继续 R22-01：先核对 `RetryTimingDisplay` 与 Maintenance 动作摘要中剩余 CloudTransfer 旧本地时间，再盘点 Storage，复用已有 formatter 后补未知/兼容负例；再处理系统时钟和宿主边界。
