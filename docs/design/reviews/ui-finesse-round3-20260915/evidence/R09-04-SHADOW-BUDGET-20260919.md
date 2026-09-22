# R09-04 阴影层次预算证据

日期：2026-09-19  
证据提交：`3ad61099`（`补充阴影层次预算行为夹具`）  
视觉实现沿用：`AdaptiveThemePaletteFactory.ApplyMaterialResources`、`DesignTokens.xaml`、`Redesign.xaml`

## 结论

R09-04 在当前可控条件下已满足；没有发现需要重建阴影系统的生产缺口。普通 `GscSurface` 默认无 Effect，`GscElevatedSurface` 只给主卡片使用，运行时资源为浮层、卡片、输入/按钮和滑块分配有限角色层级。低成本/玻璃关闭路径返回真实 `null`，不是保留一个 Opacity 为 0 的 Effect。

## 现有层级与行为证据

`ApplyMaterialResources` 在浅/深主题下均发布以下冻结 `DropShadowEffect`：

- surface：Blur `14`、Depth `2`；
- primary button：Blur `18`、Depth `0`；
- popup：Blur `20`、Depth `5`；
- sidebar：Blur `24`、Depth `3`；
- dialog：Blur `34`、Depth `8`；
- slider thumb：Blur `6`、Depth `1`。

这组资源不是所有控件统一浮起：列表/紧凑卡片仍使用无 Effect 的共享表面，浮层与对话框的模糊/深度高于普通 surface，所有实际 Effect 都是冻结对象。

`R09ShadowBudgetBehaviorTests` 使用真实工厂和资源字典覆盖：

- Light/Dark 两组 palette 的六个层级、模糊半径、深度、透明度和冻结状态；
- `motionEnabled=false` 时 PopupAnimation 为 None；
- `glassEnabled=false` 时六个阴影、游戏背景 Blur 都是 `null`，Popup 不允许透明，ambient wide wash 的各 GradientStop 全透明；
- 实际把 `GscElevatedSurface` 应用到三个 WPF Border 并放入 `ScrollViewer`，记录有阴影时的 `ExtentHeight`，移除 Effect 后刷新布局，滚动范围保持相同。

## 构建与门禁

- 以 `3ad61099` 重新执行 `scripts/build.ps1 -SkipTests`：XAML `24/24`，Release solution `0 warning / 0 error`，包含 Playnite `net462` 测试程序集。
- 同一提交身份运行 R09-04、R09-03、R09-02 和共享资源回归：`9/9` 通过、`0` skipped；其中 R09-04 为 `3/3`。
- `python scripts/validate-source.py`、`scripts/check-xaml.ps1`（`24/24`）和 `git diff --check` 通过。

## 验收边界

- 本阶段验证的是运行时资源对象、真实 WPF ScrollViewer 的布局范围和关闭玻璃的低成本回退；没有把离屏逻辑布局写成真实 Playnite 合成帧、物理 DPI/跨屏、UIA/读屏、IME、ETW 或宿主性能证据。
- 测试没有切换操作系统的真实 High Contrast 设置；高对比系统语义资源的独立验收仍归 R09-06，不能由本阶段的 `glassEnabled=false` 代替。
- 未运行真实 Playnite 安装器，也没有读取/修改真实存档、媒体、云端或用户 Playnite；package-host 仍为 `not-provided`。Demo 原始目录不可用，继续沿用恢复生产基线。
- 用户提供的 `DEV-INSTALL-008` main 全量 Playnite.Tests `73 failed / 588 passed / 57 skipped` 仍是合入后单一 checkout 的发布重跑边界，不能覆盖本阶段精确提交回归。

下一可执行任务：R09-05 焦点轮廓合成；先用真实模板状态检查焦点、选中、错误和圆角 Clip 的叠加关系。
