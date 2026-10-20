# S05-08 情景性能验证

日期：2026-10-21

实现基线：`f61ed534`，滚动修复后续提交：`48727c13`（该提交没有修改本项实现文件）

## 实现与行为

- 维护中心情景比较新增“取消比较”命令。运行时显示按钮，取消后经现有 Worker `CancellationToken` 结束只读比较，不进入清理流程；完成或取消后恢复非运行状态。
- Worker 情景比较硬上限为 10,000 个索引版本。SQLite 查询最多物化 10,001 个版本：超出上限时拒绝比较并给出明确错误，不先读取整库；超限不改变索引数据。
- 比较结束前重新读取同一快照，核对版本和生效保留策略；若期间有新增/变化则返回过期摘要并清空基线、两侧汇总和逐游戏明细。
- 增长假设仍只作为用户输入回显，不参与保留规划。未知磁盘/远端容量沿用 S05-03 现有独立呈现；比较 DTO 不产生清理预览 ID，后续实际清理仍需新的 Worker 预览及原有复核。

## 验证

- SDK 9.0.302 / VSTest 17.14.1 定向测试：Worker `29/29`，其中七个 `RetentionSimulationServiceTests.ScenarioComparison*` 用例覆盖 1,200 个版本库、零/极端增长假设、版本并发新增、有效策略并发变化、上限拒绝及取消；生产维护页 Light/Dark 行为 `2/2`，检查真实 XAML 中取消按钮的命令绑定、运行态可见性/可执行性和停止后的收起。
- 定向 TRX 运行于 2026-10-21 02:58–02:59（+08:00）的 S05 实现工作树；测试输出中的程序集位于 `.tmp/round4-s05-08-sdk9-output`。其后 `48727c13` 只提交了独立的页面滚动修复，没有变更 S05-08 的服务、ViewModel、XAML 或测试。TRX 保留原始运行时间和程序集身份；未将该测试误记为 `48727c13` 编译产物的重跑。
- S05 实现及后续滚动布局代码一并通过 SDK 9.0.302 Release 全解决方案构建，`0` error、`9` 个 `NU1900`（当前网络无法读取 NuGet 漏洞索引）；完整日志见[滚动修复构建记录](../../../UI-SCROLL-LAYOUT-REPAIR-20261020/build/follow-up-release-sdk9-console.txt)。与本项相关的生产维护页也在 Light/Dark RenderHarness 页面矩阵中通过；报告见[RenderHarness 结果](../../../UI-SCROLL-LAYOUT-REPAIR-20261020/render/render-qa-follow-up.txt)。这是离屏合成验证，不代表真实 Playnite 屏幕。

原始定向测试：[Playnite 控制台](tests/playnite-console.txt)、[Playnite TRX](tests/playnite/retention-scenario-wpf.trx)、[Worker 控制台](tests/worker-console.txt)、[Worker TRX](tests/worker/retention-scenario-worker.trx)。

## 边界

本机没有仓库 `global.json` 固定的 SDK 8.0.423，因此本项新代码用 SDK 9.0.302 验证；不将结果表述为 SDK8 通过。未启动真实 Playnite，也没有把离屏截图当成真实宿主证据；完整 WPF 安装/启动循环与物理 DPI/CEF 未验。用户原有 Round3 未跟踪 TRX/TestResults 保持未修改、未提交。

## 原始证据 SHA-256

| 文件 | SHA-256 |
| --- | --- |
| `tests/playnite-console.txt` | `24441E0A8BA9C85F6C473A4F246A1D5379C9E06B8C783ABB83C3BD6A62DBB27D` |
| `tests/playnite/retention-scenario-wpf.trx` | `62BDA83F6335D50F8F1B038805DD6BDC15F2E4AA243F784FCD39687906EBDC9A` |
| `tests/worker-console.txt` | `381D2278322675481EFFB319FE5C590F20656ACCAB2037C460C09D7510F83F6F` |
| `tests/worker/retention-scenario-worker.trx` | `17607F27334A3EFC0B4E5C3AFE4C326176FE032608C007DEC445294718EFF2CC` |
| 当前 Release solution build log | `2978B6F7FB25859382019071E04496410B2DDBB3D5EA6891125D7A9793CD854E` |
| 当前 RenderHarness 报告 | `18EEE0EDC4B4DA7077A56FDEAB5253DB998FE3FE1E856DF2A87C104E576C51BE` |
