# R04-01 当前 main 组合输入行为复核

日期：2026-10-12
状态：已满足（插件可控输入事件行为）；真实 Windows IME 候选窗与 Playnite 嵌入输入仍待验证。

## 当前身份与验证

- 本轮测试绑定 main HEAD 32c0e1e21d1fbfe0b8e46d765b4d2bd3161c46ed。源代码与测试目录相对完整源码基线 4d166f017dcba8fbb07980ec21b690267b9ff377 没有变化；此次只复核现有实现，没有新增服务、DTO、命令或断言。
- 使用隔离安装的 .NET SDK 8.0.423 对 Playnite net472 测试项目执行 Release 编译及定向测试。TRX 汇总为 37 passed / 0 failed / 0 skipped，包含 GamePickerKeyboardBehaviorTests 6 项、GamePickerViewModelTests 22 项、DebouncedRefreshTests 4 项和 R05OptionVirtualizationBehaviorTests 5 项。
- 当前身份测试程序集 SHA-256：062626771582C0657630AC573C16B0D9BBD5397FDA81FF27F78422F912CE0E61；MVID：e65c3013-b5c0-462f-8e5b-6431911a4a5c；ProductVersion：0.6.73+32c0e1e21d1fbfe0b8e46d765b4d2bd3161c46ed。
- 当前身份插件程序集 SHA-256：576C173A76880CE3B0C9D8922B64B843BE76A8A12C1EAABB3C3A9A8DC6D5937A；MVID：2ced2b54-667b-47c1-9478-e799309ec9d9；ProductVersion：0.6.73+32c0e1e21d1fbfe0b8e46d765b4d2bd3161c46ed。
- 脱敏 TRX：[PickerInputRefreshBehavior-current-main.trx](tests/PickerInputRefreshBehavior-current-main.trx)，SHA-256：F0A0CFB85D868BF8929699F1D70509C4C7106AD2880D9A3EAEF25425EA137107。

## 行为覆盖

- 生产 AcrylicProductionShellView 的游戏搜索 TextBox 在隔离 STA WPF Window 中接收组合开始、更新和提交路由。组合期间的 Enter 断言 Handled 为 false、弹层仍可见、旧 SelectedGame 不变；提交后 Enter 才确认当前可见候选并关闭弹层。
- 无可见结果时 Enter 保留旧游戏和打开状态；ImeProcessed 与上下左右方向键不会被选框 Enter 提交逻辑接管；有效候选确认、Escape 关闭以及清空按钮后的焦点回返均有行为断言。
- 配套 GamePickerViewModelTests 覆盖本地缓存过滤、状态/平台/搜索组合、选择保留和移除回退；DebouncedRefreshTests 覆盖非空查询合并、清空立即刷新、取消待执行刷新；R05OptionVirtualizationBehaviorTests 覆盖 Dispatcher 归属、立即刷新取消防抖以及 2,000 项筛选列表的虚拟化与键盘滚动。
- 搜索仍复用现有 GamePickerViewModel、ItemsView 和 debounce，没有把输入发送到 Worker，也没有改变英文查询路径。

## 证据边界

- TextCompositionEventArgs 由测试夹具构造并沿真实生产 Shell 的 WPF routed-event handlers 投递；它不是 Windows TSF/输入法产生的系统事件。夹具没有启动真实中文 IME，也没有断言操作系统候选窗翻页时的实际文本和 FilteredCount，因此不把拼音候选分页或真实输入顺序写成已验证。
- 本轮其它查询测试覆盖的是本地 ViewModel/Dispatcher 行为，不是物理键盘英文输入到画面反馈的端到端延迟测量。
- Window 为隔离的 900×640 WPF 宿主，Opacity 0.01、ShowInTaskbar false；不是 Playnite 嵌入窗口、物理 DPI、OS UI Automation/Narrator 或最终 presented frame。ETW/宿主性能不在本证据范围。
- 仅使用合成游戏 DTO 与隔离窗口；没有读取或写入真实存档、媒体、用户云端，也没有发送诊断。

原始实现证据仍见 [R04-01-IME-COMPOSITION-20260917.md](../R04-01-IME-COMPOSITION-20260917.md)。当前 main 复核没有扩大原结论：插件事件处理与候选提交顺序有行为证据，真实系统 IME/Playnite 输入链需在可用宿主中单独验证。
