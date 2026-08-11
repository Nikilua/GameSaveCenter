# 颜色、材质与主题

## 主题

所有可变颜色使用 `DynamicResource`。支持 Light、Dark、Follow Playnite/System、High Contrast、Transparency disabled。

## 建议资源

```text
WindowBackgroundBrush
SidebarMaterialBrush
CardBackgroundBrush
SecondarySurfaceBrush
FloatingSurfaceBrush
OverlayBrush
PrimaryTextBrush
SecondaryTextBrush
DisabledTextBrush
DividerBrush
DividerStrongBrush
AccentBrush
AccentHoverBrush
AccentPressedBrush
AccentSoftBrush
SuccessBrush
WarningBrush
ErrorBrush
InformationBrush
FocusRingBrush
```

## 强调色

用于主按钮、当前导航/页签、焦点环、选中指示、少量品牌元素。不要用于大段正文、所有图标和每张卡片。

## 对比度

目标：普通文本至少 4.5:1，大文本/主要图形至少 3:1。状态还必须包含文本或图标。

## 材质

适合：侧栏、顶部浮层、菜单、Popup、Inspector、Dialog、Toast。

不适合：DataGrid 每行、游戏列表每行、大面积滚动内容、所有卡片。

透明/模糊不可用时切换为不透明着色表面，保持可读性。

## 阴影

只用于浮层、Dialog、Toast 和少数主卡片。避免每行/每按钮 DropShadowEffect。深色主题更多依赖表面亮度差和低对比边框。
