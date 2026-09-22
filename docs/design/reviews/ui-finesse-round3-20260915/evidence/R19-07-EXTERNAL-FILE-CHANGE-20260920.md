# R19-07 外部文件变化证据

日期：2026-09-20  
分支：`codex/ui-finesse-round2`  
结论：已满足，待环境验证

## 1. 任务条件

R19-07 要求媒体/备份详情打开期间文件被移动、占用或损坏时有可理解回退：不崩溃、不写错误路径，并能重新定位或刷新，保留诊断上下文。

本项先核对现有媒体预览、路径动作、存档详情和恢复可用性链路。已有能力覆盖了回退和重新验证边界，本阶段没有重建文件服务，也没有静默改写用户路径。

## 2. 已有实现核对

- 截图缩略图由 `AsyncThumbnailImage`/`AsyncThumbnailLoader` 在后台读取，路径不存在时进入 `Missing`，解码失败时进入 `Failed`；generation、取消和卸载保护会丢弃被替换路径的迟到结果。`MediaThumbnailPreview` 把 `Loading/Missing/Failed/Ready` 映射为固定预览槽中的可读占位，不会让坏文件拆掉媒体列表。
- 录像详情在切换选择时先检查归档路径存在性和受支持格式；路径消失、格式不支持或 `MediaElement.MediaFailed` 时隐藏播放器并显示“录像文件不存在、格式不受支持或无法打开”。截图/录像均保留媒体 ID、文件名、完整归档路径、来源和备注上下文。
- “打开媒体”在执行前检查归档文件存在，失败由 `RunLocal` 统一写入 `StatusMessage` 并显示错误通知；“打开所在目录”使用 `OpenPath`，文件消失但父目录仍在时只打开父目录，父目录也消失则报告明确路径错误，不猜测或写入新路径。`ReloadMediaWindowCommand` 会按当前游戏/筛选重新加载窗口。
- 备份详情的 `ValidateRestoreReadinessCommand` 按稳定 `PlayniteId + BackupId` 重新验证当前归档：ZIP 不存在、ZIP 损坏、条目缺失、路径不安全、Manifest 无效、大小/哈希不一致和隔离区不可访问分别落为 `Corrupted/Failed/Warning/Unsupported` 等状态，校验只写 GameSaveCenter 自有隔离目录并在结束清理。
- 真实恢复在 Worker 入口再次读取持久化 `RestoreReadiness`；最近一次 `Corrupted/Failed` 会阻止恢复并保留 BackupId、错误码和诊断说明。验证成功后 Playnite 重新加载存档详情；详情加载失败仍沿用已有 workspace state/retry，`DetectPathsCommand` 可重新发现存档路径。归档路径不会因文件消失被静默替换，重新定位必须经过显式检测/设置或重新验证。

## 3. 实际证据

- Worker `RestoreReadinessTests` `14/14` 通过，覆盖损坏 ZIP、缺失归档、危险条目、Manifest 缺失/无效、文件缺失、同大小内容变化哈希失败、隔离目录不可访问、取消清理和结果持久化；测试均使用隔离临时目录，不触碰真实存档路径。
- Playnite 纯媒体/预览/恢复说明回归 `7/7` 通过：`MediaThumbnailConverterTests`、`AsyncThumbnailImageTests`、`R12RestoreConflictExplanationBehaviorTests`。覆盖有界冻结缩略图、路径替换时旧图不回显、游戏运行/操作锁/磁盘空间/权限与未知写失败的用户可理解说明。
- 相关 Playnite 组合筛选为 `17 passed / 4 failed / 21 total`；4 条失败均是旧 net472 产物缺少 `GscBuildCommit`，在 `TestRepositoryContext` 身份门退出，分布于 R11/R12 的 WPF 源契约夹具，不是外部文件行为断言失败。没有把这组写成完整 Playnite 通过。
- 本阶段无生产代码变更；沿用最近 `6ab0600c`（代码基线 `6d1a401b`）的 clean-tree Release `0 errors / 2 existing MediaCenter nullable warnings`，并在本阶段完成 `validate-source.py`、XAML `24/24`、`git diff --check` 门禁。

## 4. 负例与环境边界

- 合成隔离 ZIP、Manifest、临时目录、fake/testhost 和本地生产 DTO 仅验证了可复现回退/阻止边界；没有删除、移动或覆盖真实存档、媒体、用户云端或外部诊断，也没有为了模拟锁占用而改动真实文件。
- 当前没有在真实 Playnite/package-host 中执行“打开详情后外部移动/占用/损坏”的窗口时序，也没有验证 Explorer/播放器对物理锁、网络盘断开和第三方解码器异常的最终呈现；这些仍需在可控隔离宿主复跑。
- 未验真实 presented frame、物理 DPI/跨屏、UIA/读屏/IME、ETW、宿主性能和实际网络/权限策略；不绕过 Named Pipe 或系统跟踪权限。Demo 原目录不可用，沿用恢复生产基线。

## 5. 下一步

下一可执行任务为 `R19-08 慢调用可取消`：先核对网络、外部工具等待、可观察超时、取消后 UI 解锁和未知写结果语义，再补 fake 慢服务与取消负例。
