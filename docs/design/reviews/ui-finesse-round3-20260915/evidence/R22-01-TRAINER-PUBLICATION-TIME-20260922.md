# R22-01 修改器版本发布时间证据

日期：2026-09-22  
代码提交：`c0885757`（统一修改器版本时间显示）

## 本批范围

本批核对 `TrainerReleaseDto.PublishedUtc` 与 `TrainerCenterView` 的实际版本列表/详情绑定。该入口确实仍直接展示 `PublishedDisplay` 的本地日期，因此复用已有时间格式化器：

- 版本列表和详情正文使用 `PublishedRelativeDisplay`，保留“日期未知”负例。
- Tooltip/Automation HelpText 使用 `PublishedFullDisplay`，包含完整本地时区时间和 UTC 原值。
- `PublishedRawUtcDisplay` 保留可复制的原始 UTC 投影；旧 `PublishedDisplay` 保留为兼容属性，但生产 XAML 不再绑定它。
- 下载、选择版本、虚拟化 ListBox、隔离解压、命令、取消/错误语义和 Playnite/net462 兼容未改。

## 行为证据

`R22TimeDisplayBehaviorTests.TrainerReleasePublicationUsesSharedRelativeFullAndRawContract` 使用合成版本 DTO 和固定 UTC 时间，覆盖：

- 已知时间的相对、完整本地/UTC 和 Raw UTC 投影；
- `PublishedUtc` 缺失时三种投影均不虚构 1970 年时间；
- 生产列表/详情绑定完整时间 Tooltip/Automation HelpText；
- 旧 `PublishedDisplay` 不再出现在生产 XAML 的发布时间绑定中。

本批定向结果：`R22TimeDisplayBehaviorTests 27/27`。

## 验证结果与边界

- 隔离 Release 构建：XAML `24/24`，最终命令 `0 error / 0 warning`；同工作区此前完整编译的既有 `MediaCenterView.xaml.cs:699 CS8602` 基线未被本批代码触碰。
- `python scripts/validate-source.py`：通过；`scripts/check-xaml.ps1`：`24/24`；`git diff --check`：通过。
- 当前仓库不存在 `scripts/validate_wpf_ui.py`，本批未新增 WPF 静态审查通过声明；本批是绑定/DTO 微调，未运行页面级 `render-qa`。
- 仅使用合成 DTO、隔离构建和测试目录；未启动真实 Playnite/package-host，未验真实 UIA/读屏、OS 输入/IME、DPI/跨屏、最终呈现帧、ETW 或宿主性能。Demo 原目录不可用，继续参考已恢复生产基线。

下一可执行任务：继续按实际绑定核对 `DashboardViewModel`/Contracts 其余 stale/缓存时间入口；报告、复制列和日志的稳定完整时间语义不由本批代签。
