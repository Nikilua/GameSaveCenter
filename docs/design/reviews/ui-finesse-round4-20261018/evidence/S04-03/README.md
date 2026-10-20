# S04-03 同步缩放移动：当前实现已满足

日期：2026-10-20。实现提交：`f51e5c9e`（`实现比较图片同步缩放移动`）。本轮审查没有重复实现。

## 当前能力

- `MediaComparisonViewportState` 支持两图同步或独立缩放；缩放范围为 `12.5%–400%`，提供适应窗口和 100% 查看。
- 两张图片使用各自的视口尺寸和内容范围；同步平移按图像归一化中心坐标 `(0..1, 0..1)` 映射，因此不同原图尺寸和视口尺寸也会定位到相同的相对位置。
- 生产 Media 页面提供 Ctrl+滚轮缩放、拖动/滚动条/方向键平移、左右图各自的缩放按钮，并给视口和工具提供 UI Automation 名称与帮助说明。
- 比较预览沿用 `AsyncThumbnailImage` / `AsyncThumbnailLoader`：长边最多解码 `2048 px`，图片缓存最多 `64 MiB`；源图更大时保留原始像素尺寸说明，100% 指受界限的解码预览像素比例，不会无界解码大图。

实现位置：[`MediaComparisonViewportState.cs`](../../../../../../src/GameSaveCenter.Playnite/Controls/MediaComparisonViewportState.cs)、[`MediaCenterView.xaml`](../../../../../../src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml)、[`MediaCenterView.xaml.cs`](../../../../../../src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml.cs)、[`AsyncThumbnailLoader.cs`](../../../../../../src/GameSaveCenter.Playnite/Converters/AsyncThumbnailLoader.cs)。

## 行为证据

当前代码由 SDK `8.0.423` 的 `GameSaveCenter-Run.cmd` 全链运行覆盖。运行源码提交 `f70b806e`，当前 `9eae3841` 仅在其后追加验收文档；生产代码与本次审查时一致。全解 Release 构建 `0 warning / 0 error`，完整一键流程退出码 `0`，安装并启动 Playnite。完整结果和身份见[DEV-INSTALL-009 验收记录](../../../DEV-INSTALL-009-20261020/README.md)，诊断 ID `20261019T184725162Z-607fe456`。

与 S04-03 直接相关的现有行为用例全部通过：

- `MediaComparisonViewportStateTests`：`4/4`，覆盖同步/独立缩放、两图状态对齐、Fit/100% 重置、不同图像和视口尺寸下的归一化中心映射。
- `MediaComparisonViewportBehaviorTests`：真实生产 `MediaCenterView` WPF 窗口 `1/1`，使用两张不同尺寸合成 PNG，验证工具栏、适应视口、100% 原始像素尺寸、缩放和同步平移。
- `R18ThumbnailBudgetTests`：`3/3`，覆盖快速切换时的解码/缓存预算、比较预览的长边与缓存上限、原始尺寸与受限预览尺寸分离。

原始 TRX：[Playnite source suite](tests/playnite-source.trx)（SHA-256 `5282C9330BC843F98B873CA8DE67935EB3875FE649A986F58E4F814DCA61DE21`）、[WPF viewport behavior](tests/MediaComparisonViewportBehaviorTests.trx)（`BE6BA076F95E7603ADB059C8262B672FF2C0CB371E4520B8972A2D788B10CDFE`）、[thumbnail budget](tests/R18ThumbnailBudgetTests.trx)（`7F1B2A36AEBDDF08CF392E824FC0CC1FD3EE55A741D728621B58F2ADC2509157`）。WPF runner 在已通过的测试结束后输出 TextServices COM 清理异常，TRX 和一键退出结果仍均通过；没有将清理输出记成产品失败。

## 页面矩阵与边界

审查时在当前 HEAD 使用 SDK `8.0.423` 构建 RenderHarness：`0 warning / 0 error`；浅色、深色主题矩阵 `render-qa OK`，报告内 `PROBLEM` 数为 `0`。完整报告见[Render QA](render/render-qa-report.txt)（SHA-256 `A366A28996E9ECF3BADF47E127942C3ADC2AA81C717EBACA8E0CE807A98E7DA5`）。[1040×700 媒体页截图](render/Media-1040x700-tab1.png)来自合成离屏夹具，用来核对控件布局，不代表真实 Playnite 屏幕。

真实 Playnite 页面的视觉呈现、实际系统 DPI/物理屏幕和真实用户媒体未检查；RenderHarness 是逻辑 DPI `1.00` 的合成运行。仓库约定的 Demo 目录在当前 checkout 不存在，因此本次按生产 Media 页和共享主题资源复核，未声称核对 Demo。S00-01 起始候选已由现有账本与证据标为满足；S00-02 的真实 UI 门禁仍受 R05 系统指针读取权限边界限制。S04-04 已作为 S04-01/02/03 前置满足后的下一项准入。
