# 工作区滚动区越界与内容遮挡修复

日期：2026-10-20

## 修改范围

存档历史、路径与校验、任务中心队列和当前游戏媒体页现在有两个独立滚动范围：页面滚动区受 PageHost 视口约束，表格/列表保留有限高度和内部虚拟化滚动。选中详情留在表格后方时可通过页面滚动区到达；表格滚动条不会越过自己的卡片边界。布局按 `ScrollViewer.ViewportHeight` 计算，避免把内容总高度当作可见高度。媒体收件箱继续使用原 PageHost 尺寸算法。

路径候选与存档历史的表格在 1040×700 合成宿主保持至少 4 行可读；任务队列保留其四行高度下限和 136 DIP 紧凑详情高度。保留数据虚拟化、命令、绑定、选择状态和原有详情滚动。

## 验证

- SDK `8.0.423`，Release 全解决方案构建成功：`0 warning / 0 error`。构建输入基于提交 `7f229b2ef741fb405f500e8ae5ddf6b5e34e12f5`，工作树含本页对应的布局、测试和校验器改动；该构建没有声称由 CI 执行。
- 定向 WPF 行为/回归测试 `9/9` 通过，TRX：[scroll-viewport-wpf.trx](scroll-viewport-wpf.trx)，SHA-256 `882EED424517C30ECBA929A74765F1BD0C76553EFE7DF7C74F25D5D24A8F9E6C`。覆盖存档历史、路径校验、媒体列表、任务表格的双滚动边界，以及短 PageHost、紧凑任务详情和相邻源码契约。
- Release RenderHarness 双主题、多尺寸全量几何检查：`render-qa OK`、`0 PROBLEM`。1040×700 任务页 PageHost 为 `726×573 DIP`，表格滚动区为 `693×385 DIP`；存档历史和候选路径外层页面分别是 `viewport 407 / extent 553 DIP`、`viewport 407 / extent 467 DIP`，两个表格都报告 `4/4` 行可读。完整报告：[render-qa-report.txt](render-qa-report.txt)。
- 代表性合成截图：[任务中心](task-center-1040x700.png)、[历史版本](save-history-1040x700.png)、[路径与校验](save-path-verification-1040x700.png)。截图中页面滚动条位于 PageHost 边界内，表格滚动条位于表格边框内。
- `scripts/validate-source.py`、XAML 结构校验（25 个文件）、`git diff --check` 通过。WPF 静态扫描 `0 errors / 26 warnings / 177 info`；其既有警告/提示不在本批 XAML 修改中。

## 边界

RenderHarness 使用合成数据与离屏逻辑 DPI `1.00`；这些结果核对控件几何，不代替真实 Playnite 窗口、真实媒体、物理 DPI 或显示器呈现验证。没有运行完整 CI，也没有把用户原有未跟踪 TRX/TestResults 纳入本批。

## 2026-10-20 二次调整

用户复测后，原先按页面视口比例给 DataGrid 设置高度的布局仍会在较大窗口把表格滚动区分配到详情卡片以下。存档历史、路径候选、任务队列与当前媒体列表现从 PageHost 的实际 `ViewportHeight` 扣除页面工具栏、表格卡片固定行和紧凑详情预算，再把剩余高度交给星号行；表格本身保持虚拟化的内部滚动。页面空间不足时页面滚动区承接完整内容。`ClipToBounds` 限制页面绘制越界。

- SDK `8.0.423` Release 全解决方案构建通过，`0 warning / 0 error`；最终联合构建诊断 ID `20261020T074347195Z-e0d59102`，控制台：[构建记录](ui-scroll-viewport-fix-build-console.txt)。构建输入以提交 `14925e5e` 为基线，并包含本次滚动区修正和 S04-08 的缩略图回归验证。
- 定向 WPF 几何与资源回归 `6/6` 通过：[原始 TRX](ui-scroll-viewport-regression-final.trx)，SHA-256 `EB332D41CE3ADFF3BC27CA5DE569BE80348DD13E2F62A5E8BFC99C83E162646D`。覆盖 `1400×900` 的宽工作区、`900×600` 存档/媒体页面及 `715×577` 任务页，检查页面是否在视口内、空间不足时页面是否可滚、表格边框内是否保留独立滚动范围。
- `scripts/validate-source.py`、25 个 XAML 结构检查和 `git diff --check` 通过。联合构建 DLL SHA-256：`098607BC5C7E16D0D269382A5B71BED71E2012F7086AEE6BDBC97DAAE726D3B9`；测试 DLL SHA-256：`CBD0229E663A9F935B99D98E02BC658041A6F06CDA7743A39664FBF794E9704D`。

这批证据来自生产 WPF 控件的 STA/离屏几何夹具，不是 Playnite 实际窗口或物理 DPI 证据。为滚到底的测试操作未纳入验收，因为未启动窗口的夹具不会推进 ScrollViewer 偏移；当前测试核对实际测量边界与内部滚动范围。真实宿主复测仍待用户环境确认。
