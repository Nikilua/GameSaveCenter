# R22-02 容量单位统一（2026-09-21）

## 结论

本批按“已满足，待环境验证”记录。实现提交为 `86b72e1e`（`统一容量单位显示口径`），已推送 `codex/ui-finesse-round2`。

## 实现事实

- 新增 `GameSaveCenter.Contracts.ByteSizeFormatter`，统一采用 1024 进制 `B / KiB / MiB / GiB`，整字节小于 1 KiB 时直接显示 `B`，单位换算最多保留两位小数，避免 `1` 字节等小值显示为 `0 KiB`。
- `FormatDelta` 保留比较页零值 `0 B`；`FormatSignedDelta` 保留 IPC 差异摘要原有的 `+0 B` 语义。
- Backup、恢复校验、Retention、Storage、Local Mirror、Media、媒体来源预览、Trainer、诊断包、元数据灾备包等 DTO/用户摘要，以及 Worker 备份预览、环境检查、完整性提示、维护报告、隔离/镜像/恢复摘要和 Playnite 恢复进度均复用该口径。
- Media source preview 从旧的 `KB/MB/GB` 统一为 `KiB/MiB/GiB`；Trainer 和 Dashboard 元数据灾备摘要保留“未知大小”语义并修正小值显示。命令、绑定、取消/错误、恢复保护、选框、滚动条和有限列表未改。
- 仍保留速率字段的独立 `B/秒` 格式化和固定安全上限文案；它们不是同一容量对象的显示投影，没有借本批扩大语义。

## 行为与构建证据

- `R22CapacityUnitBehaviorTests`：`13/13`。覆盖 `0/1/1023/1024/1536` 字节、MiB/GiB 边界、正负/零差值，以及 Backup 概览/列表/详情投影、Media summary、媒体预览、Trainer、诊断包和元数据灾备 DTO。
- `MaintenanceReportServiceTests`：`3/3`。在隔离 Worker 目录写入合成 1 字节镜像文件，实际报告输出包含 `共 1 B`，且不包含 `0 KiB`。
- 相关 Core 合跑：`53/53`；隔离 Release 构建：XAML `24/24`，Contracts/Playnite `net462`、Tests `net472`、Worker `0 errors / 2 warnings`。两条 warning 均为既有 `MediaCenterView.xaml.cs:671 CS8602`，未改写。
- `python scripts/validate-source.py`、`git diff --check` 通过；WPF 静态审查 `0 errors / 27 warnings / 177 info`，为既有基线。

## 验证边界

- 使用合成 DTO、fake/隔离 Worker 服务和隔离目录；没有读取或修改真实存档、媒体、用户云端或真实报告目录。
- 未启动真实 Playnite/package-host，未宣称真实窗口像素、Windows UIA/读屏、OS 输入/IME、DPI/物理跨屏、呈现帧、ETW 或宿主性能已验证；Demo 原目录不可用，视觉仍沿用已恢复生产基线。
- 临时构建/测试目录位于 `.tmp/r22-02-capacity-build-20260921`，完成后清理，不作为长期证据依赖。

## 下一步

下一可执行项为 `R22-03` 复制反馈轻量，先核对现有 DataGrid/路径/诊断复制入口及成功/失败状态，再以小批行为证据推进；保持当前容量格式、命令绑定、取消/错误和恢复保护边界。
