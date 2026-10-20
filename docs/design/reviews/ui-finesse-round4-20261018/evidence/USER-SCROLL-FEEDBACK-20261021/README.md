# 用户截图滚动裁切修复

日期：2026-10-21
源码提交：`43b869d13e7b62a5af319aa9c3cfdaf33a81dfc9`

## 用户反馈与原因

三张宿主截图分别显示存档历史/路径候选表格下方内容被压住、任务队列滚动区域侵入下方详情，以及候选列表只露出部分数据。紧凑窗口下，页面工具行、次要筛选与视图操作一起占用首屏高度；候选表格的详情按钮还占着表格框架内的一行，因此内容行和滚动端点没有对应到用户看到的可用区域。

## 修正

- 存档候选列/视图操作收进可折叠区，紧凑模式将候选详情入口移到该区域标题；候选 DataGrid 独占表格框架，不再把详情按钮算进表格行。
- 任务中心紧凑模式把次要筛选、筛选预设、视图配置、导航提示和后台任务提示收进“概览与更多筛选”；搜索、状态和刷新仍保持直接可见。任务详情按钮移出队列表格框架。
- 按常见紧凑视口测量表格和页面详情，确认 4 行可读下限、表内滚动条边界和页面级滚动能到达下方详情。

## 验证

- SDK `9.0.302` Release 全解决方案构建通过，0 warning / 0 error；使用隔离输出目录、`--no-restore`、单 MSBuild worker 和 `MSBuildEnableWorkloadResolver=false`。本次构建日志未另存；构建成功结果在本轮执行时确认。
- 最终定向几何行为测试 `10/10` 通过，TRX：[compact-scroll-geometry-sdk9302.trx](tests/compact-scroll-geometry-sdk9302.trx)。用例覆盖截图尺寸紧凑布局、存档历史/候选表、任务队列、独立详情滚动、四行可见和缩小后再放大。
- `scripts/validate-source.py .` 通过；WPF 静态检查扫描 31 个 XAML 文件，0 error、27 warning、179 info。静态检查报告中的警告来自仓库其他页面/共享模板，不是本次改动文件。
- 变更后 `git diff --check` 通过。

详细身份见 [verification-identity.json](verification-identity.json)；源码校验与静态报告位于 [verification](verification/)。

## 边界

真实 Playnite 宿主未启动，离屏几何测试不作为真实屏幕证据。RenderHarness 因当前还原资产缺少 `net472/win-x86` 目标而未运行（`NETSDK1047`）；仓库锁定的 SDK `8.0.423` 本机不可用，本次可控构建使用 SDK `9.0.302`。没有重试或绕过 CEF/ETW 权限拒绝。用户原有 Round3 TRX 与 `tests/GameSaveCenter.Playnite.Tests/TestResults/` 均未修改或提交。
