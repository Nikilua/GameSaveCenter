# Q11-06 工作区页签溢出与可达性（2026-09-29）

## 结果

当前四个生产页面的顶层页签数量由 XAML 读取并核对：Save Center 4、Media Center 4、Maintenance 6、Trainer Center 4。共享 `GscRedesignWorkspaceTabControl` 已使用独立的水平 `ScrollViewer`、`HorizontalScrollBarVisibility=Auto` 和生产页签模板，本阶段没有更改生产 XAML 或导航结构。

新增 `WorkspaceTabOverflowKeepsProductionHeadersMouseAndKeyboardReachableInBothThemes`，在 STA WPF 窗口中实际加载生产资源和共享 TabControl/TabItem 样式，以生产页面的 Header 字串建立页签。对每个页面分别使用浅色、深色主题，在 `320×240 DIP` 窄窗口中验证：

- 水平溢出量大于 `24 DIP`，可见水平滚动条高度至少 `10 DIP`；
- 从前置按钮执行 WPF `MoveFocus(Next)` 能进入首个页签；方向键 Right/Left 逐项选择并将焦点页签完整滚入 Header 视口；
- 切换页签时 `PART_SelectedContentHost.ActualWidth` 相对基线变化不超过 `0.25 DIP`；
- 调用实际 ScrollBar Track 的 PageRight/PageLeft 按钮处理后，末页和首页面板可完整进入视口，页签鼠标路由事件能选择对应项；
- `1280×280 DIP` 窗口的 Maintenance 六个页签不需要滚动，水平滚动条折叠。

键盘方向通过 WPF routed key event 驱动，滚动按钮调用生产 `RepeatButton.OnClick`，页签选择通过 WPF routed mouse event 驱动；这些是受控 WPF 行为，不是物理键盘/鼠标输入。子页面内容使用测试占位内容，未加载各生产页面的业务 ViewModel。

## 运行身份与环境

- 代码提交：`1c4c21d537e78ffb9595f5d278410a7d97ca504f`。
- 行为测试源码 SHA-256：`858F7CD9F383CE290A27EF9BE55FC68B18F701370C9B800B6CA9BC6949DFD4EF`。
- 生产共享模板 `src/GameSaveCenter.Playnite/Themes/Redesign.xaml` SHA-256：`0E9B2FA1EB88C33A0DFCDFB0B0FFEDE1293CD96C6A73CED61D5F7889772B8A43`。
- 主窗口由测试调用 `CreateWindow`：`WindowStyle=None`、请求尺寸 `320×240 DIP`（宽屏负例 `1280×280 DIP`）、`Opacity=0.01`、不显示任务栏。资源在浅/深主题下强制应用，玻璃开关开、强度 `78%`、动效开。
- 执行环境：Windows `10.0.22000`，VSTest 的 .NET Framework `4.8.9181.0` 测试宿主。当前用户注册表 `AppliedDPI=144`；没有在测试窗口上测得有效 Per-Monitor DPI，不能将此注册表值当作窗口实测 DPI。字体沿用测试宿主的 WPF/系统默认值，没有锁定字体版本。
- 测试没有保存 `Window.ActualWidth/ActualHeight`、PageHost 实际尺寸或每页签的具体 viewport 数值；通过断言检查视口非空、溢出量和可见性。窗口隐藏透明度为 `0.01`，本次没有取得截图或屏幕呈现帧。

## 构建与测试

- 精确提交身份的完整 Release 流程退出码 `0`：XAML `24/24`；解决方案 `0 warning / 0 error`；Core `125/125`；Worker `356 passed / 1 existing skip / 0 failed`；Playnite source classes `111` 与 WPF isolated classes `108` 全部通过。`R08MotionReverseBehaviorTests` 在 `[54/108]` 通过，先前用户报告的失败没有在本轮完整隔离执行中重现。
- `R23ProductionResourceStateBehaviorTests`：`7/7` 通过，0 失败、0 跳过，VSTest exit `0`。TRX：[Q11-06-WORKSPACE-TAB-OVERFLOW-1c4c21d5.trx](Q11-06-WORKSPACE-TAB-OVERFLOW-1c4c21d5.trx)。
- 完整 Release 日志：[full-release-build-1c4c21d5.log](../../../../../artifacts/q11-06-release-20260929/full-release-build-1c4c21d5.log)。

本证据确认共享模板在受控 STA WPF 中的窄窗溢出及键盘/鼠标路由可达行为，不代表真实 Playnite 页面呈现、OS 物理输入、FollowPlaynite、物理 125%/150% DPI 或截图验收。因宿主和有效 per-monitor DPI 未实测，Q11-06 自动行为为通过，视觉与宿主门禁仍待验，Round2 最终状态不签收。
