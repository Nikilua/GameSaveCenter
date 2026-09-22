# R15-07 失败结果复制证据

日期：2026-09-20  
代码提交：`37dd4a03`（`补齐任务失败复制与详情`）  
分支：`codex/ui-finesse-round2`

## 完成事实

- 先核对并复用已有 `CopyTaskErrorCommand`、`CopyTextWithRetryAsync`、`ClipboardValueSanitizer`、`TaskStatusDto.ErrorCode/ErrorMessage/DetailMessage` 和恢复报告脱敏复制；没有新建第二套任务选择或剪贴板入口。
- `TaskStatusDto.FailureSummary` 只取脱敏后的首行并限制为 240 个字符；`SafeDetailMessage` 提供同一凭据规则处理后的完整技术详情。错误码仍单独可见，原有完整诊断复制契约继续包括 `ErrorMessage`、`ErrorCode`、`DetailMessage` 和任务 ID。
- `TaskFailureClipboardFormatter` 在返回 payload 前统一脱敏；`ClipboardRetry` 对 COM/`InvalidOperationException` 的瞬时占用最多尝试 4 次，成功前不修改任务对象、当前选中项或用户详情选择，最终失败保留可再次点击的失败反馈。
- Task Center 失败卡显示短摘要和错误码；技术详情继续默认收起，改为生产 `GscWpfUiTextBox` 的只读、可选择、有限高 `180 DIP` 控件，保留详情滚动和命令绑定。没有改游戏选框、既有滚动条、取消/错误/恢复保护或 net462 契约。
- Contracts 新增共享 `ClipboardTextSanitizer`，既用于任务显示也用于表格/复制入口，避免 UI Tooltip、只读详情和剪贴板出现不一致的凭据暴露规则。

## 验证结果

- `R15TaskFailureCopyTests`：`6/6`。实际覆盖短摘要边界、完整字段、密码负例、瞬时剪贴板失败重试、四次失败负例，以及生产 TextBox 样式下完整脱敏文本可选择。
- R06/R12/R15 相邻回归：`14/14`。其中先发现 R06 旧夹具仍按任务表无“阶段”列断言，校正到当前生产列契约后重跑通过；未改变复制实现。
- 外部源码副本完整 Release solution：`7 warnings / 0 errors`；警告均为当前离线环境无法访问 NuGet 漏洞源的 `NU1900`。Playnite `net462` 定向构建还保留 `MediaCenterView.xaml.cs:664` 的 2 条既有 nullable warning。
- `scripts/validate-source.py` 通过；XAML 结构校验 `24/24`；`git diff --check` 通过；`wpf-apple-desktop-ui` 静态审查 `0 errors / 28 warnings / 162 info`，无新增门禁错误。
- `.tmp/r15-07-source` 外部源码副本已删除并确认不存在；没有保留未被文档引用的本批 artifacts/tmp。

## 边界与未验项

- 未运行真实 Playnite/package-host、RenderHarness presented frame、物理 DPI/跨屏、真实 UIA/读屏、IME、ETW 或宿主性能；TextBox 选择验证是隔离 STA WPF 行为，不写成真实宿主呈现通过。
- 测试使用合成 DTO、fake 剪贴板 setter、隔离外部源码副本；没有读写真实存档、媒体、云端、用户诊断或系统剪贴板数据。Demo 原目录不可用，沿用已恢复的生产基线。
- linked worktree 的直接 WPF 临时工程仍受 `obj` `Access denied` 影响；本批用外部源码副本完成可执行构建。R15-06 已记录的 Worker 全量 `342 passed / 1 skipped / 1 failed / 344 total` 既有失败未在本批改写或冒充通过。
- main 未合并；main 仍保留用户的 R08 文件和 `src.zip`。

下一可执行任务：`R15-08 清理历史范围`，先核对现有任务清理命令、运行中任务保护、恢复账本和日期/状态预览边界。
