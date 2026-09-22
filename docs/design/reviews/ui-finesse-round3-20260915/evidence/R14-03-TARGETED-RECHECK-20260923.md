# R14-03 部分成功处理定向复核

日期：2026-09-23  
工作区：`D:\workplace\github\GameSaveCenter`  
分支：`codex/ui-finesse-round2`  
复核代码身份：`9cef8849`（本阶段无生产代码变更）  
实现提交：`aef251b1`

## 本批结论

R14-03 已按当前实现和实际定向结果收口为“已满足，待环境验证”。本批没有重建服务、DTO、命令或页面；复用已有 `MediaSyncService` 逐项 best-effort 结果、`MediaInboxBatchFailures` 失败集合和 `RetryFailedMediaInboxBatchCommand`，只补运行证据并校正账本。

## 受控验证

- `scripts/build.ps1 -Configuration Release -SkipTests`：XAML `24/24`，解决方案 `0 error`；保留既有 `src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml.cs:706` 两条 `CS8602` warning，Playnite 目标仍为 `net462`。
- Worker `MediaSyncServiceTests`：`20/20`。其中 `InboxBatchKeepsPerItemFailureAndDoesNotRepeatSuccessfulItem` 验证一个逐项失败保留稳定媒体 ID/原因，成功项不重复执行；同组还覆盖归类、撤销、取消和恢复相关行为。
- Playnite `R14ClassificationSelectionTests`：`4/4`；源契约覆盖失败集合、仅重试失败项命令和“成功项不会再次执行”。`MediaWindowAnchorContractTests`：`10/10`，覆盖当前有限媒体窗口的滚动锚点/虚拟化与失效重载边界。
- `python scripts/validate-source.py`、`scripts/check-xaml.ps1 -ProjectRoot .` 和 `git diff --check` 在本阶段文档校正后通过。

## 语义与边界

- 失败提示列表继续使用 `MaxHeight=128`、`VirtualizingPanel.IsVirtualizing=True`、`VirtualizationMode=Recycling`；重试确认只提交上次失败的稳定 ID，不读取当前列表选择，成功项不会再次执行。
- 测试只使用合成数据、fake/隔离 SQLite 和隔离 WPF testhost；没有读取或写入真实存档、媒体、用户云端，也没有对外发送诊断。
- 本批没有运行真实 Playnite/package-host、RenderHarness 新失败样本、Windows UIA/读屏/IME、物理 DPI/跨屏、最终 presented frame、ETW 或宿主性能；离屏/源契约不等价于真实呈现。Demo 原目录不可用，视觉基准沿用恢复生产基线。

下一可执行小批量：复核 `R14-04 撤销边界说明` 的应用后人工变化冲突负例和归档副本恢复；真实宿主和物理/呈现边界保持待验。
