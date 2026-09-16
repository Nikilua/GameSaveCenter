# R00-05 上下文按钮禁用透明度证据

日期：2026-09-16  
代码提交：`aebcefc`（`校准上下文按钮禁用透明度`）  
分支：`codex/ui-finesse-round2`

## 结论

R00-05 的共享样式修正与受控行为验证已完成，账本保持“代码完成待验收”。`GscWpfUiContextButton` 不再给控件整体设置 `Opacity=0.48`，禁用态只由共享模板中的 `ButtonChrome.Opacity=0.72` 负责。这样标签、图标和解释文字不会再经过 `0.48 × 0.72` 的二次淡化，按钮仍留在布局中。

## 验证结果

- 当前隔离 worktree 的 Playnite WPF Release 构建：`0` warning / `0` error。
- 双主题定向测试：`2/2` 通过（Light、Dark）。
  - `WpfUiResourceDictionaryTests.ContextActionsUseSingleDisabledChromeOpacityAcrossDerivedStyles`
- 测试从生产 `DesignTokens.xaml` 与 `WpfUiProduction.xaml` 资源字典加载实际模板，并在真实 WPF `Window`/Dispatcher 中覆盖三类派生样式：
  - `GscWpfUiContextButton`：存档“撤销最近恢复”及媒体/维护上下文动作。
  - `GscWpfUiRemoteRestoreButton`：维护页远端“下载并校验”动作。
  - `GscWpfUiMediaBatchButton`：媒体页“收藏所选”批量动作。
- 三类按钮禁用时均观测到控件 `Opacity=1`、模板 `ButtonChrome.Opacity=0.72`；启用/禁用前后 `ActualHeight` 差异小于 `0.01 DIP`，不会因状态切换跳行。
- 复合内容使用实际 WPF `StackPanel`（图标字形 + 标签）并带解释文字；双主题下解析到的标签/解释前景均非透明。以运行时渐变玻璃表面和真实主题背景做受控合成时，禁用上下文标签最低对比度为 `3.0`，达到本任务对禁用文本的可读目标。
- 生产引用校验确认 `SaveCenterView`、`MediaCenterView`、`MaintenanceView` 仍使用对应共享/派生样式；命令、绑定、布局占位和安全语义未改动。

## 范围与边界

测试窗口使用隔离资源、合成内容和不可见的低不透明度窗口来完成 Dispatcher/模板行为检查，没有写入真实存档、媒体或云端，也没有生成可冒充真实屏幕呈现的截图。该证据不替代 Playnite 嵌入宿主、用户主题、物理 DPI、真实鼠标按压或屏幕像素复核；“双主题”指当前生产主题资源在受控 WPF 实例中的 Light/Dark 运行时合成。后续 R01-02 继续处理数字单元格裁切，R00-06 继续处理媒体四行门禁。
