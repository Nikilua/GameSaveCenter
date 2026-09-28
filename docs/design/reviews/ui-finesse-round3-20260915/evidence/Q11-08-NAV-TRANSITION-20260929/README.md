# Q11-08 快速工作区导航与焦点保持（2026-09-29）

## 结果

现有生产导航已同步更新当前工作区页面、页标题和 RadioButton 选中项；内容通过缓存的生产 UserControl 立即替换，没有整页入场动画。本阶段只补真实 WPF 事件回归，没有修改生产导航或动画。

新增 `R08PageSwitchBehaviorTests.RapidNavigationUpdatesPageTitleSelectionAndKeepsFocusOnTheChosenNavigationItem`，在 STA Window 中对生产 `NavTasks`/`NavOverview` 设置实际 `RadioButton.IsChecked`，由 XAML `Checked="OnNavChecked"` 路由进入生产导航代码，快速交替七次。每次检查 ViewModel workspace、`PageHost.Content` 的缓存页面引用、同步后的标题、唯一选中项、焦点仍在刚选中的导航项，以及内容宿主没有 opacity/effect/transform 入场状态。布局完成后再次检查终点仍为任务页。测试记录：`routes=7; final=Tasks; title=任务中心; focus=NavTasks; pageEffects=none`。

只使用 Overview 与 Tasks 路由，避免隔离测试启动存档/媒体/维护的游戏范围异步加载。页面本身是真实生产视图，但 DataContext 是状态夹具；未连接 Worker、仓库、Playnite profile 或业务查询服务。

## 精确身份验证

- 代码提交：`8e3cc914`。
- Release solution build 成功：XAML `24/24`，solution `0 warning / 0 error`。Playnite.Tests 在最终代码提交身份独立 Release build `0 warning / 0 error`。
- 壳层/页面切换类：`R08PageSwitchBehaviorTests 4/4`，含新增快速导航行为。[TRX](Q11-08-R08PageSwitch-8e3cc914.trx)
- 路由来源与历史：`PurposeNavigationSourceTests 4/4` + `R10NavigationBehaviorTests 2/2`，合计 `6/6`。[TRX](Q11-08-Navigation-R10-8e3cc914.trx)
- 用户前一条报告的动画反向类另在该最终身份 `R08MotionReverseBehaviorTests 2/2`，exit `0`。[TRX](R08-MotionReverse-8e3cc914.trx)。退出 console 有一条 `TextServicesHost.OnUnregisterTextStore InvalidComObjectException` 清理噪声，xUnit 为 `2/2`、VSTest exit `0`；根因未知。
- 三组定向行为测试共 `12/12` passed、0 failed、0 skipped；这不是全量 Playnite 测试套件结果。

## 证据边界

WPF 窗口请求 `1366×900 DIP`，`WindowStyle=None`、`ShowActivated=false`、`Opacity=0.01`。测试使用 WPF Keyboard focus API 和真实 RadioButton Checked 路由，不是 OS 物理键盘/鼠标或 Playnite 宿主输入；没有捕获屏幕或 DWM presented frame，不能据同步状态断言用户屏幕上绝无一帧旧内容。没有强制主题或读取有效物理 DPI。Q11-08 自动导航行为已通过，真实 Playnite/视觉帧、OS 输入和物理 DPI 仍待验，Round2 最终状态保持未完成。
