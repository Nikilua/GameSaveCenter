# R05-03 当前 main 弹层边缘复核

日期：2026-10-13

状态：部分满足。当前 main 上生产 Picker/ComboBox 的边界、滚动和选定项可见行为通过；表格条件中的“翻转无闪屏”没有呈现帧证据，不能据几何稳定或离屏测试签收。

## 当前身份与验证

- 源码/测试 commit：`1c39e909856faa736b32224ee196ef0bac6ca460`。本轮仅复核现有实现，没有改生产 XAML、模板、Popup、滚动条或命令。
- 使用隔离 .NET SDK `8.0.423`，Release 构建插件 `net462` 和测试 `net472`；测试宿主 .NET Framework `4.8.9181.0`。
- 定向运行 `R05PopupBoundaryBehaviorTests` 与 `R05OptionVirtualizationBehaviorTests`，`7 passed / 0 failed / 0 skipped`。脱敏 TRX：[R05-03-boundary-postcommit.trx](tests/R05-03-boundary-postcommit.trx)，SHA-256 `B3BF918708BF18D6AA8B15A6DDE8962953D2E1084892CF8DA261198C93459427`。程序集身份见 [R05-01/02 当前 main 复核](../R05-01-CURRENT-MAIN-RECHECK-20261013/README.md)。

## 已验证行为

- 生产 Shell 在 720×360 DIP 短窗中加载 200 个合成游戏，选中最后一项后验证 PickerPanel 不越出 overlay 宽高、列表有 Auto 滚动、所选行位于实际列表视口；紧凑时筛选行隐藏、`Padding=8`；窗口扩到 900 DIP 后筛选行恢复，缩回 360 DIP 后再次隐藏，选中项保持可见。
- 使用生产 `GscWpfUiComboBox` 模板，在当前桌面工作区右下边缘打开 100 项 ComboBox，选中第 100 项。通过 `PointToScreen` 和当前窗口 `TransformToDevice` 换算验证 Popup 起止坐标在当前 `SystemParameters.WorkArea` 内；Popup 高度受 `MaxDropDownHeight=120` 约束，ScrollViewer 有可滚动范围且滚动条为 Auto，选定项在滚动视口可见。
- R05-02 2000 项选择/虚拟化回归一并运行 `5/5`。没有为本轮生产页面或共享模板生成新 RenderHarness 截图，因为没有布局或资源变更。

## 未验证边界

- 当前测试读取 Popup 打开后的几何，不采集呈现帧或眼见动画；“翻转无闪屏”仍未验。不能把稳定的 `PointToScreen` 坐标、RenderHarness 图片或逻辑 DPI 当作无闪烁证明。
- ComboBox 只覆盖当前桌面工作区，不覆盖真实 Playnite 嵌入、多物理屏、物理 DPI 跨屏、OS IME/UIA/Narrator、ETW 或宿主帧率/性能。720×360 测试没有重新验证历史 420×220 极端壳层；旧证据记录该尺寸约只有 92×92 DIP 的有效区，不能容纳完整选框内容。
- CI #941（commit 同为 `1c39e909`）仍在运行，当前本机 SDK `8.0.423` 测试不代表 CI SDK `10.0.401` 门禁通过。

原始实现记录：[R05-03 弹层边缘适配](../R05-03-POPUP-EDGE-20260917.md)。下一项依赖已满足的任务为 R05-04 多选摘要当前 main 复核；“翻转无闪屏”保留为待实际呈现帧验证的 R05-03 条件。
