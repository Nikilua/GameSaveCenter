# R22-01 Retention 预览时间入口（2026-09-21）

## 本子批次结论

提交 `abe9369e5bfa7055f971c8db0a39f95899e7d79a` 将 Maintenance 保留预览中的日期展示接入共享时间契约。R22-01 整项仍为“部分满足，待继续”，没有改变只读预览、二次确认、保护判断或真实存档操作。

## 复用与实现

- `RetentionSimulationItemDto` 保留旧 `CreatedDisplay` 供报告/兼容使用，同时增加相对时间、完整本地时区时间和原始 UTC 文本。
- Maintenance 保留预览列表改用 `CreatedRelativeDisplay`；Tooltip/UIA HelpText 提供 `CreatedFullDisplay`。列表结构、有限高度、滚动、候选保护状态、Apply 二次确认和 Worker 授权字段均未改动。

## 实际验证

- `R22TimeDisplayBehaviorTests` `8/8`：共享 formatter、任务/活动、Save/Maintenance DTO 以及 Retention 旧字段兼容和新完整/原始 UTC 合同。
- `MaintenanceReportSourceTests` `4/4`、`R17FindingTriageBehaviorTests` `3/3`；Playnite retention 相关定向合计 `15/15`。
- `RetentionSimulationServiceTests` `12/12`：候选计算、保护统计、只读预览、二次确认和应用边界继续通过。
- 精确提交身份 Release 构建：Playnite `net462`、Playnite Tests `net472`、Worker Tests `net8` 均 `0 errors`；Playnite 主项目保留 2 个既有 `MediaCenterView.xaml.cs:671 CS8602` warning。`validate-source.py`、XAML `24/24`、`git diff --check`、WPF 静态检查 `0 errors / 27 warnings / 177 info` 通过。

## 未验边界

- MediaCenter 的 `CapturedLocal`、ValidationFinding 的证据时间及其他未展示的 DTO 时间字段尚未全部迁移；真实剪贴板、系统时钟跳变/跨系统启动周期、Playnite/package-host、Windows UIA/读屏、OS 输入/IME、DPI/跨屏、最终呈现、ETW 和宿主性能未验。
- Demo 原目录不可用，继续使用恢复生产基线；业务验证使用合成 DTO、fake 服务和隔离目录/测试宿主，未写真实存档、删除媒体、写云端或外发诊断；main 用户改动未碰、未合并。

## 下一步

继续 R22-01：盘点 MediaCenter `CapturedLocal` 入口及其他仍显示绝对本地时间的页面，优先处理已有共享样式和复制行为边界。
