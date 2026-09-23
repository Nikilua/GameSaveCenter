# Q06-08 按钮五态受控序列（2026-09-23）

## 结果

从提交 `75769579c46d998cc3b931b8c0011762e7d8cfe4` 构建 Release 测试后，新增 `Q06ButtonStateSequenceBehaviorTests.ProductionButtonCapturesAndChecksFiveStatesInLightAndDark` 定向结果 `1/1`、exit `0`。Playnite 目标 `net462`、测试 `net472`，构建 `0 errors`；保留既有 `MediaCenterView.xaml.cs:703 CS8602`，NuGet 漏洞数据源不可用产生 `NU1900`，依赖包恢复成功。测试加载生产 `DesignTokens`、`WpfUiProduction`、`Redesign` 和 `AcrylicProductionResources`，使用 STA WPF Window 与合成假命令，Light/Dark 各捕获 normal、hover、pressed、focus、disabled，共 10 张图。测试逐项核对模板状态和实际属性，失败时会使测试失败。

| 状态 | 激活方式 | 行为检查 | 截图 |
| --- | --- | --- | --- |
| Normal | 默认可用、无输入 | 启用且未 hover、pressed、focus；三个状态 overlay 为 0 | [Light](light-normal.png) · [Dark](dark-normal.png) |
| Hover | WPF `MouseDevice.ChangeMouseOver(button)` 状态探针 | `IsMouseOver=true` 且 Hover overlay 为 1 | [Light](light-hover.png) · [Dark](dark-hover.png) |
| Pressed | 合成 `Keyboard.KeyDown(Space)` | `IsPressed=true`、Pressed/Focus overlay 为 1、缩放和 chrome opacity 进入按下值 | [Light](light-pressed.png) · [Dark](dark-pressed.png) |
| Focus | `Keyboard.Focus(button)` | `IsKeyboardFocusWithin=true` 且 Focus overlay 为 1 | [Light](light-focus.png) · [Dark](dark-focus.png) |
| Disabled | 假 `RelayCommand.CanExecute=false` | 按钮禁用、Disabled chrome opacity 为 0.72；框架点击派发后命令执行次数仍为 0 | [Light](light-disabled.png) · [Dark](dark-disabled.png) |

原始状态/属性报告：[button-state-probe-report.txt](button-state-probe-report.txt)；VSTest 结果：[q06-08-state-sequence-final3.trx](q06-08-state-sequence-final3.trx)。

## 边界

图像由离屏 `RenderTargetBitmap` 以 96-DPI logical 尺寸生成，已检查两主题背景、文字、激活标签和按钮状态轮廓。Hover 调用 WPF 内部状态转换，没有移动 OS 鼠标；Space 为合成路由键盘事件，focus 为程序化 WPF 焦点。此结果不代表物理鼠标、Playnite 宿主焦点/输入、UI Automation/读屏、DPI 缩放、真实屏幕呈现帧或动画像素序列通过。因此 Q06-08 的受控自动/视觉列通过，宿主仍外部待验，最终状态保持未完成。
