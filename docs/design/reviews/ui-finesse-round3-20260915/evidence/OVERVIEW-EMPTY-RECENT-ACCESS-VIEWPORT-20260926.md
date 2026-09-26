# Overview 空最近访问列表可读视口复核

日期：2026-09-26
代码基线：`daa6ecb26c6cc089279373a24189724ee4baabb0`（修复前 `main`）
任务账本：R `192/192`，状态 `106/83/1/1/1`；不新增或改变任务行

## 根因与修复

当前 RenderHarness 的真实空状态夹具绑定 `RecentAccessItems.Count == 0`。`OverviewRecentAccessList` 原来显式 `MinHeight="0"`，所在 Grid 行又按内容 Auto 测量，因此 WPF 将 ListBox 收缩到 `2 DIP`；`MaxHeight="280"` 只限制最大尺寸，不能为无项列表提供最小阅读/空态区域。自动审计在多个常用尺寸和主题持续报告 viewport `<236 DIP`。

将该 ListBox 的最小视口设为 `236 DIP`，保留 `280 DIP` 上限、真实空集合绑定、空态文案、内部滚动和 Recycling virtualization。没有添加占位数据、改动命令/状态模型或改变 Demo 的首页信息架构。

## 验证

- 修复前的当前 main 离屏 RenderHarness：11 个窗口样本中空集合 ListBox 均为 `2 DIP`；全项目 Render QA 共 33 项 PROBLEM，其中 Overview 多尺寸/主题占 20 项。
- 修复后 Release RenderHarness build：`0 warning / 0 error`；首页 RenderHarness 全部 11 个尺寸样本列表高度为 `236 DIP`、`items=0`，双主题页面样本无 Overview PROBLEM。
- `RestoredAcrylicForkBaselineTests`：`16/16` 通过；该类增加来源契约，要求保留 OverviewRecentAccessList 与 236 DIP 最小视口。
- 项目级报告从 33 项降至 13 项，Overview PROBLEM `0`。余项仅属 Task（视口/紧凑 Inspector）和 Settings（矮窗类别/主体视口）；完整项目 gate 仍失败，不能记为全绿。
- `scripts/validate-source.py`、XAML structural `24/24`、WPF static `0 errors/28 warnings/177 info`、`git diff --check` 均通过。
- `scripts/render-qa.ps1` 的包装构建先因当前身份无权读取用户 NuGet.Config 而在渲染前退出。随后使用现有已还原资产执行 Release `--no-restore` build，并运行同一 RenderHarness，获得上述完整尺寸/双主题报告。

修复后报告的 HEAD 字段仍是起点 `daa6ecb2`，`WorkingTreeClean=False` 是因为本次 Overview source/test 正在工作树中；报告明确枚举并验证了该工作树，不能当作 clean-commit 构建记录。

只验证离屏 WPF 逻辑 DIP，不代表 Playnite 实际父容器、物理 DPI 或最终屏幕呈现。本轮没有安装或启动 Playnite。R 基线和状态数不变。
