# 用户报告的备份诊断乱码与复制失败：当前 main 复核（2026-09-26）

## 结论

此前用户诊断线程提出三处放大问题：Ludusavi 输出未按 UTF-8 解码、非零退出诊断缺少原始输出、任务详情复制遇到剪贴板占用时直接抛错。这三项实现已由 `3f42de436fdbd86029cdf61ae82392fe1c87afa7` 落入 main；本次测试运行时的产品代码基线为 `d1559fc94af34b0c66c4b565fc8278167575224c`，可达该修复提交。本次只重建并复测，没有新的产品代码修改。

## 当前实现与边界

- `ExternalProcessRunner` 对 stdout/stderr 显式设置 UTF-8；`Utf8OutputIsDecodedAsUtf8` 以中文输出验证解码。
- Ludusavi 非零退出时保留原始输出：有 stdout 时 `RawOutput` 使用 stdout，否则回退到 stderr；优先 stderr 的 `ErrorMessage` 单独作为任务错误消息，诊断详情包含退出码和 `RawOutput`。因此典型仅 stderr 报错不会再留下空 `RawOutput`。
- 任务详情复制使用 `ClipboardRetry`，最多四次尝试，针对 COM/InvalidOperation 剪贴板占用异常做短暂重试；持续失败时给出状态与通知提示，不把异常直接抛给用户。原有脱敏与完整详情内容保持。
- 原始备份失败的根因是当时 Ludusavi 下载 GitHub manifest 超时，后续网络恢复；本修复改善编码、错误证据与复制体验，不伪称修复外部网络，也不新增自动重试备份策略。

## 当前提交验证

- Release Worker `ExternalProcessRunnerTests`：`5/5` 通过，包括 UTF-8 中文解码、输出限额、非零退出输出、超时与取消。
- Playnite `R15TaskFailureCopyTests`、`R06ClipboardBehaviorTests`、`R22CopyFeedbackBehaviorTests`、`R12RestoreReportBehaviorTests`、`TaskCenterViewResponsiveTests`：`21/21` 通过；构建时使用该产品代码基线的 `GSC_BUILD_COMMIT` 与 `GSC_SOURCE_ROOT`，不是旧 test assembly。
- Release Playnite 构建有两条既有 `MediaCenterView.xaml.cs:703 CS8602` warning，无编译错误。定向结果只证明隔离测试中的行为，不替代真实 Playnite 宿主或真实系统剪贴板测试。
- 补充已有证据：[R15-07 任务失败复制复核](R15-07-TASK-FAILURE-COPY-RECHECK-20260923.md)、[R22-03 复制反馈复核](R22-03-COPY-FEEDBACK-20260921.md)；原始实现提交为 `3f42de43`。

本次未启动 Playnite、未操作真实系统剪贴板或用户数据。R 台账仍为 192 项，状态计数不变。
