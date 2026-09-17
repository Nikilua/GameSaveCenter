# R05-07 Tooltip 时序证据

日期：2026-09-17（Asia/Shanghai）  
任务：`R05-07 | Tooltip 时序`  
实现提交：`2fc8c0d67c1d6cf585c85b984607efefb4b129a3`（`收紧提示换行与Esc关闭行为`）

## 结论

当前生产 Tooltip 已收口到可控范围：快速指针移动继续使用 `InitialShowDelay=350 ms`、`BetweenShowDelay=100 ms`；两套实际会覆盖生产页面的隐式 Tooltip 样式都明确不可聚焦、Mouse 附近放置、`MaxWidth=420 DIP`，字符串说明使用换行且不省略；壳层和独立设置页按 Esc 关闭当前插件范围内的 Tooltip，关闭时不把焦点移入浮层。

本轮先核对了 Q15-03/Q15-07 已有时序、Popup 边界和资源隔离能力，再发现 `AcrylicReferenceControls.xaml` 会覆盖 `DesignTokens.xaml` 的隐式 Tooltip 样式，之前的 `MaxWidth=420` 不能覆盖所有实际生产资源路径。本轮只修共享样式和局部行为入口，没有引入新的设计体系，也没有改变游戏选框、滚动条、命令绑定、取消/错误、安全或 net462 契约。

## 实际生产行为

`R05TooltipTimingBehaviorTests.ProductionTooltipWrapsLongPathsAndEscClosesWithoutMovingFocus` 在真实 `AcrylicProductionShellView`、生产 `GscWpfUiPathDetailTextBox` 样式、生产 Tooltip 样式和 STA WPF `Window` 中使用合成路径验证：

1. Shell 的实际附加属性为 `InitialShowDelay=350`、`BetweenShowDelay=100`；Tooltip 的有效 `Focusable=False`、`Placement=Mouse`、`MaxWidth=420`。
2. 超长合成路径完整保留在 Tooltip 的字符串内容中，生成的 `TextBlock` 为 `TextWrapping=Wrap`、`TextTrimming=None`；实际 Tooltip 与文本宽度均未超过 420 DIP，文本高度大于单行字体高度，证明发生了受约束换行而不是横向撑宽或省略。
3. Tooltip 打开前后焦点都留在原 `TextBox`；对原输入控件发送 `Key.Escape` 后实际 Tooltip 关闭、事件被处理，`Keyboard.FocusedElement` 保持原输入控件。

`GscToolTipBehavior` 只挂在 `AcrylicProductionShellView` 和 `GameSaveCenterSettingsView`，按 `PlacementTarget` 限定在当前插件界面范围，避免影响其他 Playnite 扩展的 Tooltip。

## 构建与回归

- `scripts/build.ps1 -Configuration Release -OutputRoot .tmp/r05-07-build`：XAML structural validation `24/24`；Release 编译 `0 warning / 0 error`；Playnite 目标仍为 `net462`；Core `83/83`。
- 同一标准构建产物：`R05TooltipTimingBehaviorTests 1/1`、`UiFinesseRound2ControlSourceTests 24/24`、`R05PopupBoundaryBehaviorTests 2/2`、`R05FocusBoundaryBehaviorTests 3/3`、`R05TogglePersistenceBehaviorTests 1/1`。
- Worker 全集为 `309/311` 通过、`2` 失败：既有 `IntegrityCheckServiceTests` 期望 `Healthy`/`Skipped`，当前隔离环境实际返回 `Warning`；失败发生在标准脚本的 Worker 步骤，未进入 Playnite 全量步骤，不是 R05-07 失败。直接无身份注入的 `dotnet test` 源测试曾出现 `GscBuildCommit` 缺失，随后已按标准构建产物复跑通过。
- `git diff --check`：通过。

## 视觉证据

执行：

```text
scripts/render-qa.ps1 -Configuration Release -Output .tmp/r05-07-render-clean
```

报告 `.tmp/r05-07-render-clean/render-qa-report.txt` 绑定完整实现提交 `2fc8c0d6...`，记录 `WorkingTreeClean=True`、Light/Dark、297 张 PNG、`DpiScale=1.00`（仅离屏 logical DIP）和 `render-qa OK`。报告中的 Settings Popup/Tooltip 开面探针、各 workspace、多尺寸和滚动探针均通过；人工抽查 `Settings-1040x700-tab2.png`、`Settings-1040x700-tab3.png` 与 `Overview-1040x700.png`，未见本轮共享 Tooltip 模板修改引起的页面裁切、横向溢出或层级破坏。

## 边界

行为测试使用合成路径、隔离 STA WPF Window 和生产资源，Tooltip 是显式打开的生产样式实例；没有把它写成真实 Playnite 鼠标悬停录像。快速划过不产生气泡雨的真实鼠标轨迹、ShowDuration 与宿主输入消息队列、真实屏幕边缘翻转/遮挡、物理 DPI/跨屏、宿主字体、OS 键盘/IME、屏幕阅读器/UIA、presented frame、ETW 和宿主性能仍未验证。离屏 `DpiScale=1.00` 只代表 logical DIP；未写真实存档、媒体、云端或诊断数据。

下一项：`R05-08` 弹层资源热切换。
