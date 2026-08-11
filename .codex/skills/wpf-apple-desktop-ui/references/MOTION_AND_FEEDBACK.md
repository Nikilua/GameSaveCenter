# 动画与反馈

## 动画目的

动画用于确认输入、解释状态变化、表现层级和方向。不要为了“高级感”让所有控件漂浮、弹跳或模糊。

## 推荐属性

优先：Opacity、TranslateTransform、轻微 ScaleTransform、Color/Brush、图标旋转、选中指示。

避免：Width、Height、Margin、GridLength、大面积 BlurEffect、每帧回调、大范围布局动画。

## 时长

- Hover 100–140ms
- Pressed 80–100ms
- Focus/Selection 120–180ms
- Tab/Inspector 160–220ms
- Dialog 180–240ms
- Toast 200–260ms
- Drawer 220–280ms

## WPF 注意

不要动画已 Freeze 的 Freezable；可共享但不动画的 Brush/Geometry 可 Freeze；动画结束清理 Storyboard；窗口关闭停止长期动画；禁止每行 DispatcherTimer。

## Reduce Motion

尊重系统动画关闭、高对比度和用户设置。降级为淡入淡出，关闭弹性/缩放，但保留必要状态变化。

## Feedback 模型

- Inline：表单错误、路径不可用、单项状态
- Banner：页面级、可恢复、非阻塞问题
- Toast：自动备份、媒体同步、设置保存、后台完成；不抢焦点
- Dialog：不可逆操作、选择、重要警告、详细错误
- Progress：显示阶段、进度、可取消性和失败下一步
