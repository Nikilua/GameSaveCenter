# 当前规则与工作台操作行窄窗测量

日期：2026-09-29。源码基线：`451164f2241f9a13a7d974c72410bcdd4528acb2`（仅归档测试和证据，不改生产 UI）。

## 测量范围

- 生产 `SaveCenterView` 的“路径与校验”页 `SaveCurrentRuleActions`，浅/深主题各测 `520、560、600、640、680、699、700、760、900 DIP`。
- 生产 `OverviewView` 的 `OverviewHomeToolbarActions`，浅/深主题各测 `520、560、600、640、680、699、700、720、760、900 DIP`。
- 每个尺寸都以 STA WPF Window 实际测量生产 WrapPanel 的子项坐标、行数和可见按钮尺寸；回归断言要求三个按钮都可见且大于零尺寸，并且操作组保持单行。

## 结果

- 两个操作组在扫描尺寸和两种主题下均保持单行；没有触发垂直换行，故无需人为添加行距。
- 当前规则操作组 520–699 DIP 时实际面板宽 `480–659.33 DIP`；700 DIP 起面板 `245.33 DIP`，仍能容纳全部三项操作。工作台操作组在 520–720 DIP 时实际面板宽 `472–672 DIP`，760 DIP 起切回右侧 `272 DIP` 栏，仍保持单行。
- 隔离 Release solution/XAML build：`0 warning / 0 error`，XAML `24/24`。精确新建 Release 测试 DLL 的 VSTest `2/2`，浅/深主题各一项，无失败/跳过。DLL SHA-256：`E6A388725AF39DDDD6109BF305B9B4595DCB4DC77565CCE057DA903B179535C0`。TRX：[`wrap-scan-final.trx`](wrap-scan-final.trx)。
- 旧 `DashboardView.GameHeaderActions` 位于 `DashboardDemoShell`（`Visibility=Collapsed`）内，不属于当前可见的生产页，因此不把它计作用户实际看到的换行控件，也不为隐藏兼容树改布局。

## 边界

- 结论只覆盖生产 WPF 控件、合成页面上下文、STA Window 的逻辑 DIP；未在真实 Playnite/package-host 或物理 125%/150% DPI、屏幕呈现中复核。
- 首次误用默认 `bin` 的 test invocation 被源码身份门禁拦截（旧程序集 `96bd2c81`、当前源码 `451164f2`），该结果已停止且不计入。随后以隔离 Release 输出中确切的测试 DLL 直接运行 VSTest，归档的 TRX 对应本节源码基线。
- 未改变生产布局、命令、Binding、数据契约或业务语义；Round3 192 项任务的分类与总数未改变。
