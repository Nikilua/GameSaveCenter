# Q11-01 导航图文节奏 WPF 行为证据（2026-09-28）

## 结论

生产侧栏已有共享 `AcrylicNavItem` 和 `AcrylicProductionShellView` 收起/展开布局；本阶段只补实际 WPF 几何回归，没有改导航模板、配色、命令或状态。受控窗口内展开、收起和恢复的图文中心与纵向节奏均符合当前布局预期。Playnite 宿主/显示帧未验，Q11-01 最终仍未完成。

## 当前提交和验证

- 测试代码提交：`cd631771f812fa1f3aed99b5d079f49043ae7f13`。
- Release Playnite 测试项目构建：`0 warning / 0 error`。
- `ProductionShellChromeSourceTests`：`13/13`，0 failed，VSTest exit `0`。其中新增 `ExpandedAndCollapsedNavigationKeepsEveryIconLabelAndSelectionFrameAligned` 在真实 STA WPF `Window` 中量测生产壳层；整个测试窗口为 `900×640 DIP`、Opacity `0.01`，不是 Playnite 宿主呈现。
- 精确身份 TRX 本机路径：`artifacts/q11-01-navigation-rhythm-20260928/q11-01-production-shell-exact-cd631771.trx`。早期整类启动使用错误的 `GSC_BUILD_COMMIT`，身份门禁拒绝 7 个源码读取用例；失败原始 TRX：`artifacts/q11-01-navigation-rhythm-20260928/q11-01-production-shell-cd631771.trx`。未绕过门禁；随后按 `git rev-parse HEAD` 修正身份并重建，整类 `13/13`。这次调用错误不是产品测试失败。
- 成功 TRX 有 1 条 WPF `TextServicesHost.OnUnregisterTextStore InvalidComObjectException` 清理输出；用例全部通过且进程 exit `0`，噪声根因未知。

## 几何断言

测试对首页、存档、修改器、媒体、任务、维护和设置七个入口逐项读取真实 `ActualWidth/Height` 与同一壳层坐标系中的图标/标签/导航边框边界：

- 展开和恢复时，每项图标/标签垂直中心差不超过 `1.5 DIP`，七项图标与标签左边界无个别漂移；主导航六项高度一致、相邻中心间距一致。Settings 前的分隔线间距保留为设计分组。
- 收起时，每个图标和 26-DIP 内容容器都在 48-DIP 导航按钮水平中心 `1.5 DIP` 内；标签确实折叠。被选任务的导航边框横向覆盖完整按钮宽度。
- 七项在展开与收起间的垂直中心变化不超过 `1.5 DIP`，任务选择在收起及恢复后仍保持。

观测摘要：展开导航边框 `x=10..256 DIP`、相邻中心间距 `54 DIP`、全部图标中心 `x=36.33 DIP`；收起边框 `x=10..58 DIP`、间距仍为 `54 DIP`、全部图标中心 `x=34.33 DIP`；恢复后展开几何回到相同值。

该证据证明 WPF 控件布局边界，不是像素截图或真实 Playnite 窗口验证；实际主题资源颜色、鼠标输入、物理 DPI 和最终显示帧仍待宿主检查。
