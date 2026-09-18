# R08-08 变换所有权

## 结论

R08-08 已满足当前可控实现条件。提交 `194a16fe588a2f102047792a94252e25ba5e7714` 为每个使用 motion helper 的控件建立自己的 RenderTransform 所有权：外部或样式提供的变换首次进入 helper 时按当前值 `CloneCurrentValue()`，同一控件后续调用复用登记的变换树；冻结变换、直接变换、已有 `TransformGroup` 和嵌套组合均保留原几何效果。

这样不依赖 WPF 是否能枚举某个可变 Freezable 的其他依赖属性所有者：两个控件即使在调用前引用同一个未冻结 `TranslateTransform` 或 `TransformGroup`，进入动画 helper 后也会分别拥有独立副本。R00-02 已有的组合缩放复用仍保持同一控件连续 1000 次调用的节点数、深度和实例稳定。

## 实现与行为验证

- `GscMotion.MotionState` 记录控件拥有的根变换；`GetMutableTranslateTransform` 与 `GetMutableScaleTransform` 共用所有权入口，不改命令、Binding、游戏选框或滚动条系统。
- 新增 `ExternalMutableTransformsAreOwnedPerElement`：两个 Border 共享未冻结平移变换时，改变第一个的 X 不改变第二个或原始变换；两个控件共享带旋转子节点的 TransformGroup 时，缩放第一个不改变第二个，旋转角仍为 `7`。
- R00-02 `ScaleTransformIsReusedInAStableCompositeTree` 连续 `1000` 次调用通过；冻结的平移/旋转组合几何仍为 `12/-4/7`，独立实例互不串扰。
- 当前提交干净隔离工作树的 Release/XAML 构建：XAML `24/24`，解决方案 `0 warning / 0 error`，包含 Playnite `net462`。
- 当前提交串行定向回归：Foundation `9/9`、R08-01 反向 `2/2`、R08-02 热关闭 `1/1`、R08-03 离屏 `1/1`、R08-05 页面切换 `2/2`、R08-06 数字变化 `3/3`、R08-07 对话框 `3/3`，均无失败/跳过；源码校验与 `git diff --check` 通过。

## 边界

- 验证使用合成控件、STA WPF、fake/隔离输出和逻辑 DIP；没有把离屏结果写成真实 Playnite 呈现、物理 DPI/跨屏、presented frame、UIA/读屏、ETW 或宿主性能通过。
- Demo 原目录不可用；本轮没有替换视觉体系，继续以恢复的生产资源基线为视觉参照。
- `CloneCurrentValue()` 保留进入 helper 时的当前几何值；外部绑定后续是否需要重新推送到 motion-owned 副本不属于本项契约，未宣称动态资源绑定语义已在真实宿主验证。
- 未修改真实存档、媒体、云端或用户 Playnite；package-host 身份仍为 `not-provided`。

## 下一步

R08 动效生命周期小批量已收口；下一可执行任务为 R09-01“主题转换闪白”，先核对现有主题切换资源更新顺序和 Popup/占位图能力，再补最小行为与负例证据。
