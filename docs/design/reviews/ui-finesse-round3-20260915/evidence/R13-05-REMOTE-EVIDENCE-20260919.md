# R13-05 远端证据详情（2026-09-19）

## 结论

R13-05 在当前可控范围已满足。实现提交为 `e280cf1c`（`补充云端详情证据展示`），已推送到 `origin/codex/ui-finesse-round2`。

本批复用了已有云端队列、远端布局和维护页详情入口，仅增加安全的 display-only 投影：

- `CloudTransferStatusDto` 现在提供远端对象、来源设备、最后尝试和最后成功校验的显示字段。
- `CloudTransferStateService` 按现有备份/媒体远端相对路径生成远端对象显示值，不把凭据传入 UI。
- `CloudRemoteDisplay` 脱敏 URI 用户信息、`token/password/secret/api_key/authorization` 等 key-value 以及 Bearer 值。
- 维护页沿用 `GscInspectorScrollViewer`、`GscRedesignSectionCard` 和现有 DataGrid 滚动系统展示四项证据。
- 既有 `CopyDiagnosticsCommand` 继续经过 `ClipboardValueSanitizer`；没有增加真实云端写入或新的复制通道。

成功校验时间的边界是刻意保守的：现有 durable 队列没有历史成功校验时间列，只有当前 `RemoteVerified` 行的 `UpdatedUtc` 能被证明为本次成功校验时间；其他状态显示“未知”，不把上传时间或状态迁移时间冒充历史校验事实。

## 行为与负例

实际测试覆盖：

- 空字段：远端对象、最后尝试、最后成功校验均为“未知”，来源设备为“未知设备”。
- URI 用户密码、query token、Bearer token：显示文本不包含原认证参数，只保留 `[已隐藏]`。
- Worker 状态映射：`RemoteVerified` 显示当前可证实的成功时间；转为 `Uploaded` 后没有历史持久化证据，最后成功校验回到“未知”。
- 维护页绑定了四项证据；复制诊断的既有 sanitizer 对 URI、query token 和 Bearer 负例均移除原值。

## 验证结果

- Release 隔离 solution：`0 warning / 0 error`；包含 Contracts、Core、Worker、Playnite `net462` 和测试程序集。
- Core 定向：`CloudRemoteEvidenceKeepsUnknownFieldsUnknownAndRedactsCredentials`、`CloudRemoteDisplayCombinesRelativePathWithoutExposingQueryCredentials`，`2/2`。
- Worker 定向：`CloudStatusProjectsRemoteEvidenceAndKeepsHistoricalVerificationUnknown`，`1/1`。
- Playnite 定向：`R13CloudTransferStageBehaviorTests`，`10/10`。
- XAML 结构：`24/24`；`python scripts/validate-source.py`、`git diff --check` 通过。

构建使用当前分支代码的外部隔离源码/输出目录和 linked worktree 的 Git 身份绑定；本批 `.tmp/r13-05-source`、`.tmp/r13-05-build`、`.tmp/r13-05-build-linked` 已清理。直接在 linked worktree 生成 WPF 临时项目仍遇 Access denied，因此没有把它误写成宿主失败或代码失败。

## 未验边界与下一步

没有运行真实 Playnite/package-host、真实 rclone 远端、RenderHarness 视觉截图、物理 DPI/跨屏、UIA/IME、presented frame、ETW 或宿主性能；没有写真实存档、媒体、云端或对外诊断。Demo 原目录不可用，继续沿用恢复生产基线。main 的用户改动和 `src.zip` 未触碰、未合并；旧 `.tmp/r12-07-build-final` 仍因 Access denied 暂留，未扩大清理范围。

下一可执行任务是 `R13-06 离线恢复反馈`：先查现有 Worker/维护页离线状态、恢复入口和错误分类，再补网络恢复前后状态变化与负例。
