# R08-05 当前 main 页面切换复核（2026-09-24）

## 结论

R08-05 当前 main 仍满足受控页面切换条件。生产 shell 缓存六个工作区页面；同页导航复用现有 `PageHost.Content`，跨页切换保留 DataGrid 的选中项、DataContext、96 项数据源和滚动位置。当前采样的任务页滚动 offset 为 `10→10`，任务→媒体的 shell Measure/Arrange 增量为 `1/1`，媒体→任务为 `1/1`；重复导航到当前任务页额外增量 `0/0`。PageHost 没有 Effect 或入口动画。

## 当前身份与构建

- 当前代码与 Release 测试程序集身份：`13c38754`（main HEAD；本阶段未改生产代码或测试代码）。
- Release solution 构建成功，XAML `24/24`、0 errors；保留两条既有 `MediaCenterView.xaml.cs:703 CS8602` warning。Playnite 目标为 `net462`，测试目标为 `net472`。
- `python scripts/validate-source.py`、`scripts/check-xaml.ps1`（24 files）和 `git diff --check` 通过。

## 行为门禁组成

按类串行执行、各 VSTest 进程 exit `0`，合计 `31/31`，0 failed、0 skipped：

| 测试类 | 通过数 | 记录 |
| --- | ---: | --- |
| `R08PageSwitchBehaviorTests` | 2/2 | 真实生产 shell、Task/Media 页、96 个合成任务和隔离 STA Window；正向跨页保留状态与滚动，另有无全页 Blur/入场动画的源码门禁 |
| `R02BusyStateTests` | 4/4 | 相邻业务忙态回归 |
| `R08BusinessFeedbackBehaviorTests` | 4/4 | 相邻成功/失败/取消反馈回归 |
| `ProductionShellChromeSourceTests` | 12/12 | 生产壳层结构与行为源码门禁 |
| `UiFinesseFoundationTests` | 9/9 | 共享 WPF 基础回归 |

当前 R08 页面切换测试的 TRX 实际输出为：`offset=10→10`、切往媒体 `Measure/Arrange=1/1`、返回任务 `1/1`、页面实例 `cached=True`、选择仍在。增量数字是本次观察值；该行为测试断言切换确实引起布局，而不把 `1/1` 写成跨环境固定上限。重复同页导航的 `0/0` 由测试单独精确断言。

`ProductionShellChromeSourceTests` TRX 在 testhost 收尾阶段包含一段 WPF `TextServicesHost.OnUnregisterTextStore InvalidComObjectException` 输出；VSTest 明确 `12/12` 且 exit `0`。其他四份 TRX 未检出该文本；根因未知，按清理噪声记录。

## 边界

证据使用合成 DTO、真实生产 WPF 页面、隔离 STA Window 和逻辑 DIP。未验证正常 Playnite 嵌入宿主、Worker 长任务、UIA/读屏、物理 DPI/跨屏、实际呈现帧、ETW 或宿主性能。用户报告的 Media、Task、Save、Settings 窗口问题不能由本项页面切换测试关闭；旧本机扩展身份与候选包对照仍以设置截图证据为准。Demo 原目录不可用，沿用恢复的生产基线；没有访问真实存档、媒体、云端或诊断数据。

## 原始结果

- [R08PageSwitchBehaviorTests](R08-05-CURRENT-MAIN-R08PageSwitchBehaviorTests.trx)
- [R02BusyStateTests](R08-05-CURRENT-MAIN-R02BusyStateTests.trx)
- [R08BusinessFeedbackBehaviorTests](R08-05-CURRENT-MAIN-R08BusinessFeedbackBehaviorTests.trx)
- [ProductionShellChromeSourceTests](R08-05-CURRENT-MAIN-ProductionShellChromeSourceTests.trx)
- [UiFinesseFoundationTests](R08-05-CURRENT-MAIN-UiFinesseFoundationTests.trx)

下一项按用户当前请求核对 R18-04 相关 `23` 项行为测试的精确组成；该项已完成的 R08-06/07/08 证据不重建。
