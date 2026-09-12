# U12-09 动效、焦点与可访问性审计

## 自动证据

- `AccessibilitySourceTests`、`KeyboardFocusSourceTests`、`ProductionShellChromeSourceTests` 组合定向测试：13/13 通过。
- `SharedMotionPrimitivesCloneFrozenTransformsBeforeTheyAreAnimated`、减动效侧栏终态和共享列表焦点契约：3/3 通过。
- `check-xaml.ps1`：24 个 XAML 通过；`validate-source.py` 通过；WPF 静态审查：0 errors、22 warnings、175 info。警告均为既有有限视口/参考控件提示，未新增 error。

## 当前契约

- 页面与共享模板继续使用 `MotionTokens.xaml`、`GscMotion`，系统关闭动画或高对比度时直接进入终态，不把 Width/Margin/DataGrid 行作为常规动画目标。
- 搜索框、紧凑 Inspector、图标按钮、状态 Presenter 和列表项保留 `AutomationProperties.Name`、共享焦点视觉和键盘回退；禁用按钮继续显示 Tooltip。
- 状态 Presenter 的 Loading 会阻挡底层输入，Error/Offline 的重试按钮保持可命中并可由 Enter/Space 激活；筛选无结果显示独立“无结果”语义。

## 未覆盖边界

上述证据来自 STA/WPF 离屏夹具和源码门禁，不等同真实 Playnite/FusionX 宿主。Windows“关闭动画”、高对比度、物理 DPI、实际键盘导航和宿主主题仍需在隔离 Playnite 环境人工复核。
