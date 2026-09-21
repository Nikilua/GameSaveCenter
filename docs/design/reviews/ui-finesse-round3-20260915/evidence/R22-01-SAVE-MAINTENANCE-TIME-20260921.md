# R22-01 Save 与 Maintenance 时间入口（2026-09-21）

## 本子批次结论

提交 `2be8627db39304a2a3f929a797799c5c366b28c0`、`c211a04f5c28b400bac976cfc2223cae2a709299` 和 `0db4abe7` 完成 Save 历史/详情、Maintenance 审计表及恢复可用性检查时间的显示接入。R22-01 整项仍为“部分满足，待继续”，保留其他时间入口和真实宿主边界，未修改真实存档、诊断写入或用户目录。

## 复用与实现

- 复用 `TimeDisplayFormatter`，为已有 `BackupVersionDto`、`AuditLogEntryDto` 增加相对时间、带本地时区偏移的完整时间和原始 UTC 文本；没有复制第二套格式化逻辑。
- Save 历史表和选中版本详情改用相对时间，既有列宽、选择/分页、DataGrid 复制、恢复/校验/路径命令和滚动系统保持；时间单元格与详情提供完整时间 Tooltip/UIA HelpText。
- Maintenance 审计表改用相对时间，完整时间进入共享时间单元格的 Tooltip/UIA HelpText；诊断表、复制诊断、健康报告和其他维护命令未改动。保留预览中的 `CreatedDisplay` 留作下一边界，不提前扩大本批范围。
- `c211a04f` 仅校正一个随绑定迁移而过时的 Save 源码断言：从旧 `CreatedLocal` 改为 `CreatedRelativeDisplay`，并保留完整时间存在性校验。
- `BackupVersionDto.RestoreReadinessCheckedDisplay` 保留旧的本地时间、结果较旧提示和“尚未检查”语义；新增相对/完整显示，SaveCenter 所选版本详情改用相对时间，完整时间进入 Tooltip/UIA HelpText。恢复校验命令、选中版本、恢复保护和滚动系统未改。

## 实际验证

- `R22TimeDisplayBehaviorTests` `12/12`：共享 formatter、任务/活动、BackupVersion/AuditLog DTO、恢复校验时间的完整/原始 UTC 合同，以及 Save/Maintenance 绑定入口；`MaintenanceReportSourceTests | R17FindingTriageBehaviorTests` `7/7`。
- `R06SortingBehaviorTests` `4/4`、`R11HistoryTimeNavigationBehaviorTests` `3/3`；该定向批次合计 `26/26`，恢复校验的旧 stale/unknown 负例与新绑定均实际覆盖。
- 精确提交身份 `0db4abe7` Release 构建：Playnite `net462` `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning；Playnite Tests `net472` `0 errors`（构建阶段保留同一既有 warning）。`validate-source.py`、XAML `24/24`、`git diff --check` 和 WPF 静态检查 `0 errors / 27 warnings / 177 info` 通过。
- 全套 `WpfUiResourceDictionaryTests` 记录为 `133 passed / 39 skipped / 4 failed`；本批相关 Save 旧绑定断言已由 `c211a04f` 校正。剩余 3 条失败分别是既有 Settings 响应字段断言、媒体空数据滚动断言和 Inbox 下拉共享模板断言，与本批时间字段无关，未改写为通过。

## 未验边界

- Maintenance 保留预览、Local Mirror 最近同步和其他维护日期入口，以及其他 Save/恢复入口尚未全部接入；真实剪贴板、系统时钟跳变/跨系统启动周期、Playnite/package-host、Windows UIA/读屏、OS 输入/IME、DPI/跨屏、最终呈现、ETW 和宿主性能未验。
- 代理/离屏 WPF 结果不替代真实宿主呈现或物理跨屏证据；Demo 原目录不可用，继续使用恢复生产基线。业务验证使用合成 DTO、fake/隔离测试宿主，未写真实存档、媒体、云端或诊断；main 用户改动未碰、未合并。

## 下一步

继续 R22-01：先盘点 Local Mirror `LastSyncDisplay` 和其他仍直显旧本地时间的入口，复用已有 formatter 后补未知/兼容负例；再处理系统时钟和宿主边界。
