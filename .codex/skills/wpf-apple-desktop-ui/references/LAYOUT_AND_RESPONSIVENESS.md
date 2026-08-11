# 布局与响应式

## WPF 布局基础

页面根节点优先使用 `Grid`：标题和工具栏为 `Auto`，主内容为 `*`，底部动作区为 `Auto`。可滚动列表放在 `*` 行，由自身滚动。

不要把 `DataGrid`、`ListBox`、`ListView`、`TabControl` 或日志区域放在纵向 `StackPanel` 中；它会给子控件无限高度，导致剩余空间、滚动和缩放异常。

## 响应式模式

依据插件内容宿主的 `ActualWidth/ActualHeight`，不是屏幕分辨率。

- Wide：宽度 ≥ 1280
- Medium：980–1279
- Compact：< 980
- Comfortable Height：≥ 760
- Compact Height：< 760

### Wide

完整导航；左侧游戏/项目列表 320–360；详情区使用剩余宽度。

### Medium

导航可变成图标模式；左侧列表 270–300；隐藏重复说明；次级操作进入“更多”。

### Compact

隐藏左侧列表，改为顶部选择器或 Drawer；主内容占满；Inspector 改为覆盖抽屉；表格保留横向滚动。禁止硬挤三栏。

## 高度响应

高度不足时优先：隐藏重复副标题、压缩间距、折叠 Banner、把常驻设置移入 Inspector。不要把正文缩到不可读。

## 宿主安全区

Playnite 或自定义窗口右上角可能存在窗口控制按钮。标题栏用独立安全列：

```xml
<Grid.ColumnDefinitions>
    <ColumnDefinition Width="*"/>
    <ColumnDefinition Width="Auto"/>
    <ColumnDefinition Width="{DynamicResource PageTopSafeRight}"/>
</Grid.ColumnDefinitions>
```

不要用负 Margin 避让。

## 滚动策略

- 不给整页随意套 ScrollViewer
- 标题、导航、主要工具栏固定
- 列表、表格、日志各自滚动
- 超宽 DataGrid 自动横向滚动
- Tab Header 过多时使用横向滚动或溢出菜单

## GridSplitter

可见线低对比，拖动热区可更宽；设置合理 Min/Max，保存用户宽度，支持恢复默认。

## DPI

使用设备无关单位；`UseLayoutRounding=True`；细线可 `SnapsToDevicePixels=True`；图标用统一 ViewBox；测试 100%、125%、150%、200%。
