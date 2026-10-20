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
