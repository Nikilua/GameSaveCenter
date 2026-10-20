# S04-07 比较结束转交

实现提交：`14925e5e`（`增加比较意见归类预览转交`）。

比较面板可把已标记对象整理后转交给现有归类/忽略预览。转交快照按稳定媒体 ID 排序并携带数量；预览再次核对当前收件箱对象，比较面板关闭不会执行操作。最终处理仍由原有用户预览与确认流程负责。

## 验证

- `MediaComparisonOpinionTrackerTests.HandoffSnapshotUsesStableIdsInDeterministicOrder` 和生产比较页交互等 Playnite 定向用例 `7/7` 通过：[Playnite TRX](tests/s04-07-playnite-final.trx)。
- Worker 收件箱复核转交对象状态 `1/1` 通过：[Worker TRX](tests/s04-07-worker-final.trx)。
- 同批 SDK `8.0.423` Release 全解决方案构建成功，`0 warning / 0 error`；当前滚动区修正的后续 Release 构建诊断 `20261020T071629590Z-146798ff` 也包含本提交代码，见[构建记录](../../../UI-SCROLL-VIEWPORT-20261020/ui-scroll-viewport-fix-build-console.txt)。
- 合成数据的转交预览截图：[media-comparison-handoff.png](media-comparison-handoff.png)。

首次 Playnite 定向运行 `6/7`，测试夹具在会话标记按钮处触发空引用；修正夹具后保留完整 `7/7` 终态结果。原始首次运行：[初次 TRX](tests/s04-07-playnite-initial.trx)。

截图和行为测试均为合成媒体及离屏 WPF，不是 Playnite 实际窗口或用户媒体。真实宿主呈现、物理 DPI 和完整 WPF 独立进程循环未验。
