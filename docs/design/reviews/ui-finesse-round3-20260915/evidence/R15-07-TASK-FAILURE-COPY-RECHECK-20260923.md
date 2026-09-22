# R15-07 失败结果复制定向复核

日期：2026-09-23

复核提交：`d9dc4317`（`codex/ui-finesse-round2`，D 盘工作区）

## 结论

R15-07 的受控实现条件已满足，账本状态校正为“已满足，待环境验证”。本批没有新增生产代码，复用了 `37dd4a03` 已提供的任务复制命令、统一脱敏、有限高只读详情和剪贴板重试，仅复测当前提交上的行为与 net462 构建。

## 实际复测

- `R15TaskFailureCopyTests`、`R06ClipboardBehaviorTests`、`R22CopyFeedbackBehaviorTests`、`R12RestoreReportBehaviorTests`、`TaskCenterViewResponsiveTests` 合计 `21/21` 通过。
  - 失败短摘要按首行脱敏并限制为 240 字符；完整复制 payload 保留任务摘要、错误码、技术详情和任务 ID，但密码等凭据被替换为 `[已隐藏]`。
  - 复制失败后的瞬时占用最多重试 4 次；第三次成功保留原 payload，四次失败返回负例且不丢当前任务/详情选择。
  - 生产只读 TextBox 的完整脱敏文本可选择，详情使用有限高控件而非把未经处理的技术字段直接显示为 TextBlock。
  - 相邻 DataGrid/恢复报告复制反馈仍能区分成功与失败，不改变现有任务选择和滚动边界。
- Playnite `net462` 定向构建实际完成，无新增错误；保留 `MediaCenterView.xaml.cs:706` 两条既有 `CS8602` warning。

## 保留能力与边界

- 继续复用 `CopyTaskErrorCommand`、`TaskFailureClipboardFormatter`、`ClipboardTextSanitizer`、`ClipboardRetry`、现有任务选中状态、详情滚动、命令绑定、取消/错误/恢复保护和 Playnite/net462；没有新建第二套任务历史或剪贴板入口。
- 测试使用合成 DTO、fake 剪贴板 setter、隔离 STA WPF/testhost，没有写真实存档、媒体、云端、用户诊断或系统剪贴板。
- 未运行真实 Playnite/package-host，未验系统剪贴板时序、最终 presented frame、真实 UIA/读屏、IME、物理 DPI/跨屏、ETW 或宿主性能；隔离 TextBox 选择行为不冒充真实宿主呈现通过。
- Demo 原目录不可用，沿用已恢复生产基线，没有引入新的设计体系。R15-06 记录的 Worker 全量既有 `MediaSyncService.cs:570` 失败仍不改写为通过。

下一可执行任务：`R15-08 清理历史范围`。先核对日期/状态预览、运行中任务保护、恢复账本和真正执行集合的一致性。
