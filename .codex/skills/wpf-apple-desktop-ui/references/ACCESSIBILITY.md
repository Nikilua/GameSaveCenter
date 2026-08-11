# 可访问性

每个功能支持鼠标、键盘、UI Automation/Narrator、高对比度、DPI 和不依赖颜色的状态表达。

## 键盘

Tab 顺序与视觉顺序一致；Enter/Space 触发；Esc 关闭 Dialog/Popup 或按上下文清除搜索；方向键操作菜单、列表、Tab、ComboBox。

## Focus Visual

不能只写 `FocusVisualStyle="{x:Null}"`。必须有替代焦点环：与控件圆角一致、1–2 DIP、对比足够、键盘 Tab 时明显。

## Automation

自定义控件设置 `AutomationProperties.Name`、`HelpText`、`LabeledBy`，并保留正确 AutomationPeer/Pattern。看起来像按钮的卡片不要用无语义 Border + MouseDown。

## 对比度与状态

普通文本 4.5:1，大文本/图形 3:1；Success/Warning/Error 有文本或图标；Selected、Focus、Hover 不靠微弱色差。

## 点击目标

建议普通按钮高 40–42，图标按钮 36–40，最小可点击区域约 32×32。

## 测试

Windows 真机使用 Accessibility Insights、Inspect.exe、Narrator、Keyboard-only、High Contrast 和 200% DPI。
