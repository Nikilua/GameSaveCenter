# Q11-05 页签内容 Stretch 行为复核（2026-09-28）

## 结果

复核 `GscRedesignWorkspaceTabControl`：共享 TabControl 默认设置 `HorizontalContentAlignment` 和 `VerticalContentAlignment` 为 `Stretch`；模板把 `PART_SelectedContentHost` 放在独立 `*` 行，并通过 `TemplateBinding` 保持内容对齐与 Header 面板解耦。没有生产 XAML 修改。

新增 STA WPF 行为用例在 `380×240 DIP` 窄窗口里实际实例化共享模板，以短中文、长英文和计数 Header 逐项切页，分别在浅/深主题测量当前页内容根 Border 与 `PART_SelectedContentHost`：内容视口左缘相对 TabControl 为 `0 DIP`；页面根元素在视口内 x/y 偏移不超过 `0.25 DIP`，宽高与内容视口差不超过 `0.25 DIP`。在这三种 Header 下都没有整页居中或留下空白偏移。

## 构建与测试

- 源码/测试身份：`e467c3b2fe080965c3946f1f3101f707c731472b`。
- Release Playnite 测试项目构建：`0 warning / 0 error`。
- `R23ProductionResourceStateBehaviorTests`：`6/6` passed、0 failed/skipped，VSTest exit `0`；TRX：`artifacts/q11-05-tab-content-stretch-20260928/q11-05-tab-content-stretch-e467c3b2.trx`。
- 本结果是隔离 STA WPF 合成内容与逻辑 DIP 布局，不代表真实业务页、Playnite 宿主、FollowPlaynite、物理 DPI/屏幕呈现或 OS 输入已验证。Q11-05 的 Stretch 行为证据补齐，整体最终状态仍受真实宿主门禁限制。

下一独立项为 Q11-06：按当前生产页签数量核实窄窗下已有水平滚动策略的鼠标与键盘可达性，不新增不必要的导航系统。Media Inbox 用户滚动缺陷仍待同次真实 Playnite 几何日志或隔离宿主前置条件恢复。
