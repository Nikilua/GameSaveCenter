# U03-01/02/03 按钮状态与图标命中区

## 去重结论与本轮增量

生产共享按钮已实现 Normal/Hover/Pressed/Focus/Disabled 五态，主/次/危险/图标按钮共用状态层；忙碌指示是延迟显示的非命中测试覆盖层，不替换文案、不改按钮宽度。图标按钮使用 34×34 DIP 主样式、36×36 DIP 工具栏样式，保持 6 DIP 间距，窄空间由 WrapPanel 换行。实际使用中的 17 个生产图标操作都有 AutomationProperties.Name 和 Command/Click；图标样式继承 GscWpfUiButton 的共享键盘焦点反馈。

没有生产 XAML/资源增量。本批只新增一项回归保护：`AccessibilitySourceTests.ProductionIconOnlyActionsExposeAutomationNamesAndSharedKeyboardFocus` 扫描生产 XAML，确保每个图标操作具有无障碍名称、命令或 Click，并继承共享焦点视觉。该测试源提交为 `8c927a93cff92b923a8efbedbdf28aadbc10d00b`；生产按钮模板基线仍是 `f59fe4c86c78cb2fc11414a431aceb49821910ee`。

## 行为与状态图验证

SDK 8.0.423 Release，精确提交 `8c927a93` 定向运行 `Q06ButtonStateSequenceBehaviorTests`、`Q06ContinuousButtonBehaviorTests`、`R02BusyStateTests`、`R02HitAreaSpacingTests` 与 `AccessibilitySourceTests`：13/13 passed，0 failed，0 skipped。TRX：[U03-01-03-8c927a93-focused.trx](tests/U03-01-03-8c927a93-focused.trx)，SHA-256 `249014D5A7DA4D7152FBD5E7D52533B3316EA1A508CD0E462047603119461739`。

专用五态探针在浅/深主题分别截取 normal、hover、pressed、focus、disabled，共10张图；逐图SHA在 [screenshots.json](screenshots.json)，代表图可直接查看：[浅色正常态](button-states/light-normal.png)、[浅色悬停态](button-states/light-hover.png)、[浅色禁用态](button-states/light-disabled.png)、[深色禁用态](button-states/dark-disabled.png)。测试覆盖尺寸与内容在忙碌周期保持、键盘焦点保留、快速操作不闪忙碌提示、重复执行门禁，以及失败/取消后恢复命令状态。图标命中测试量测按钮边界并通过 WPF VisualTree hit-test 检查邻接目标和空隙；34 DIP 按钮不重叠，窄宽下三项操作按行折叠。

Q06 探针的悬停由测试设置 WPF IsMouseOver 只读依赖属性，不是物理鼠标；按压通过合成 Space，焦点通过 Keyboard.Focus。图片为 96-DPI 逻辑 WPF 截图。命中检查与 AutomationProperties 名称验证没有启动 Playnite、Narrator 或真实 UIA 客户端。未改生产资源，所以不另跑整页 Render QA；按钮模板的五态图像矩阵和 U02-05/06 双主题 334 图 render-qa 留存供对照。

首轮回归扫描曾把 `Views/Development/UiFrameworkProbeView.xaml` 的开发探针按钮当作产品操作，并因其没有命令触发失败；修正为只扫描生产 XAML 后 13/13 通过。提交后第一次本地复验又把 `GSC_BUILD_COMMIT` 参数写错，触发测试已有的 checkout/程序集身份保护；从 `git rev-parse HEAD` 取准确 SHA 后完整重跑通过。两份失败 TRX 均保留在 [tests](tests/) 并在 [manifest](screenshots.json) 记录理由与SHA；它们不是产品失败。

## GitHub 与验收边界

提交 `8c927a93` 的 `.github/workflows/windows-build.yml` run `40561836233`（baseline job `122392618285`、SDK10.0.401 job `122392618705`）截至 2026-10-27 18:43 UTC 仍在各自编译/测试阶段。更早 U02 源码与文档提交的 run、job 与 SDK10 artifact 身份见 [ci-runs.json](../U02-05-06/ci-runs.json)；required-checks 接口此前403，仍记“远端待验”。CI 结果不能用预提交本地通过替代。

当前状态为“代码完成待验收”：真实 Playnite 宿主、实际 UIA 树/屏幕阅读器、实体鼠标、物理 DPI 未验证。没有触碰存档恢复、媒体删除或云端写入；保持真实命令、Binding、错误/取消行为、滚动条和游戏选框不变。
