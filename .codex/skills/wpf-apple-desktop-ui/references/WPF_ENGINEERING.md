# WPF 工程实现规范

## ResourceDictionary

建议拆分：DesignTokens、Light/Dark Colors、Typography、LayoutTokens、Buttons、Inputs、ScrollBars、DataGrid、Tabs、Dialogs、Animations。

合并顺序：基础颜色/度量 → Typography → Primitive controls → Complex controls → Page overrides。避免循环引用。

## DynamicResource

会随主题变化的 Brush、Foreground、BorderBrush、Focus、Surface 使用 DynamicResource；稳定 Geometry/Thickness 可 StaticResource。

## ControlTemplate 契约

完整保留 PART、ContentPresenter、Popup、Track、ItemsPresenter、ScrollViewer、Validation、Focus、Keyboard 和 UI Automation。不要只复制半套宿主模板。

## VisualStateManager

复杂公共控件优先 VSM：Common、Focus、Check、Selection、Validation。简单样式可 Trigger，但项目内统一。

## Behaviors

适合自适应布局、自动 Tooltip、搜索清除、列宽持久化、焦点管理。行为不包含业务逻辑。

## Converter

只做轻量纯转换。禁止文件 I/O、网络、日志读取和重型集合处理。

## Virtualization

```xml
VirtualizingPanel.IsVirtualizing="True"
VirtualizingPanel.VirtualizationMode="Recycling"
ScrollViewer.CanContentScroll="True"
```

自定义 ItemsPanel 时检查虚拟化是否失效。

## Binding

用 FallbackValue/TargetNullValue；只读属性 OneWay；大型集合避免频繁 Reset；收集 Binding Error。

## 高风险模式

StackPanel + DataGrid、ClipToBounds、固定 Height/MaxHeight、外层 ScrollViewer、负 Margin、ContentAlignment 属性继承、Popup 资源作用域、宿主默认模板泄漏、Freeze 后动画、DPI 半像素。

## 构建

按真实工具链执行 restore/build/test/验证脚本/git diff。旧 .NET Framework WPF 可能需要 Visual Studio MSBuild，不能把跨平台 dotnet build 失败误判为代码失败。

## 打包

不在构建失败后安装旧 DLL；安装后核对实际版本；保持插件 ID；不替换用户配置。
