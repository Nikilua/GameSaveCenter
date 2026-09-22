# R22-01 残余时间入口复核（2026-09-23）

## 结论

本批只复核当前生产代码，没有新增生产实现。按真实绑定盘点了 `Views`、`ViewModels` 和 `Contracts` 中仍保留的旧本地时间投影：未发现生产 `Views/*.xaml` 绑定到旧的本地化直显属性。现有 `LastAccessDisplay`、`TaskPageLastUpdatedDisplay`、`LastAttemptDisplay` 以及 Contracts 中的 `ToLocalTime`/旧显示属性，分别属于兼容入口、报告/复制/日志字段或内部状态；页面实际使用相对时间、完整本地时间、原始 UTC 和 Automation/Tooltip 证据入口。

## 实际验证

- `R22TimeDisplayBehaviorTests`：`30/30` 通过。
  - 覆盖刚刚/分钟/昨天边界、完整时间与 UTC 提示、未知时间、任务与活动合同、最近访问、云端摘要/详情、维护动作、存储与恢复、比较项、游戏来源诊断、Trainer 发布时间、任务时间线和生产视图旧绑定负向审计。
  - `ProductionViewsDoNotBindLegacyLocalTimeProjections` 同时核对旧入口不在生产视图绑定中，并核对相对/完整入口存在；不是单独的 `Assert.Contains` 交互签收。
- `R10RecentAccessBehaviorTests`：`2/2` 通过。
  - 实际覆盖最近访问稳定 ID/有界列表/移除游戏清理，以及概览最近访问命令与任务历史隔离。
- 当前隔离 Release 构建：XAML `24/24`，Playnite 目标 `net462`，`0 error`；仅保留 `MediaCenterView.xaml.cs:706` 两条既有 `CS8602` warning。
- 当前工作区为 `D:\workplace\github\GameSaveCenter` 的 `codex/ui-finesse-round2`，复核身份为 `df6bee9d`；本批没有覆盖或重建 `main` 的旧实现。

## 边界

验证使用合成 DTO、fake/隔离 testhost 和隔离构建目录，没有读写真实存档、媒体、云端或外发诊断。尚未宣称真实 Playnite/package-host、Windows UIA/读屏、OS 输入/IME、DPI/物理跨屏、最终 presented frame、ETW 或宿主性能通过；Demo 原目录不可用，继续沿用已恢复的生产基线。当前 WPF 静态脚本 `scripts/validate_wpf_ui.py` 不存在，不以缺失脚本伪造检查结果。

## 下一项

R22-01 残余入口本地复核按“已满足”收口，不重复重建。下一可执行项保留 R23-04 正常可枚举 Playnite 主窗体/UIA 会话；若仍被 CEF/窗口暴露阻塞，则转依赖已满足的独立 Q/R 小批量，并保留真实宿主、呈现帧、ETW 和性能边界。
