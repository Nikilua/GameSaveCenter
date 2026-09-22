# R13/R14 定向行为与视觉复核

日期：2026-09-22

工作区：`D:\workplace\github\GameSaveCenter`

分支：`codex/ui-finesse-round2`

代码身份：`8a9a052e68d88ac3cdbe514e141d8e946c9e1268`

## 结论

本批没有生产服务、DTO、命令或页面绑定变更；只补了 RenderHarness 的合成数据，使生产 XAML 已有的 R14 重复组和来源试运行列表获得正向样本，同时保留清空态复位。此前因 SDK/Workload 环境阻塞的 R13/R14 定向夹具现在可以在当前隔离 Release 输出中实际运行。

R13-07、R13-08、R14-01～R14-08 的当前受控行为、绑定数据和相关视觉样本已补证，账本不再沿用“定向夹具未执行”的过时描述；真实 Playnite/package-host、网络/外部工具、UIA/读屏、OS 输入/IME、物理 DPI/跨屏、最终 presented frame、ETW 和宿主性能仍是独立未验边界。

## 自动验证

当前隔离 Release 构建先通过 XAML `24/24`、解决方案 `0 error`，仅有既存 `MediaCenterView.xaml.cs:699` 两条 `CS8602` warning；RenderHarness 项目自身构建为 `0 error / 2` 条相同 warning。

定向测试结果：

- Core `UiDisplayMappingTests`：`40/40`，覆盖云端阶段/失败帮助、未知与零值、归类证据和高置信门禁。
- Worker `CloudRetryPersistenceTests` + `MediaSyncServiceTests`：`32/32`，覆盖队列筛选/分页选择、错误分类、来源试运行、重复组、稳定目标、部分成功、撤销冲突和取消恢复。
- Playnite `R13CloudTransferStageBehaviorTests`、`R14ClassificationEvidenceTests`、`R14ClassificationSelectionTests`、`R14SourceRulePreviewTests`：合计 `19/19`。

本批使用 `tests/GameSaveCenter.RenderHarness/FakeDashboardData.cs` 的合成 DTO：当前游戏重复识别为 `2` 组，确定组内 `4` 项；来源试运行 `4` 个样本（命中 `2`、排除 `2`）。这些样本不启动 Worker、不访问真实媒体，也不执行删除或移动。

## 视觉与布局核对

最终 clean-tree 报告目录为 [`artifacts/ui-qa-r13-r14-clean-20260922`](../../../../../artifacts/ui-qa-r13-r14-clean-20260922)，报告身份为 `Commit=8a9a052e`、`WorkingTreeClean=True`、Light/Dark、offscreen logical DIP `1.00`。

- Maintenance 诊断页在 `1040×700` 起始场景中保留 `FindingsGrid` `8` 行、`5/4` 可读行；本轮报告没有 Maintenance/R13 专属 `PROBLEM`。
- Media 重复识别页在 `1040×700` 中报告 `MediaDuplicateGroupsList=2`、`MediaDuplicateItemsList=4`、组内列表 `330 DIP`；来源规则页报告 `MediaSourcePreviewItems=4`、`240 DIP`。双主题代表图已人工抽查：确定/疑似标签、SHA-256/元数据原因、组内路径、命中/排除原因和只读说明均可见。
- RenderHarness 全报告仍以 `FAILED` 结束，但失败集中在既有 Overview 空活动列表、Settings 窄窗断点/校验状态、Task/Save 窄窗行数门禁；本轮没有 R13/R14 专属失败。不能把该报告写成全局 `render-qa OK`，也不能把 offscreen 结果写成真实宿主呈现。

## 未验边界与下一步

- R13 的队列/失败帮助仍未在真实网络、真实 rclone 配额或 Playnite 宿主中验证；R14 仍未在真实存档/媒体上执行任何移动、删除或云端写入。
- 真实 Playnite UIA/键盘入口仍受 R23-04 的宿主窗口不可枚举限制；本批不绕过系统权限，也不把 `DPI=1.00` 离屏数字当成物理 DPI。
- 下一可执行项：继续收敛 R23-05 的独立几何/性能边界，或取得能暴露 Playnite 主窗体的隔离桌面会话后重跑 R02-06/R23-04；当前 R13/R14 受控证据已补齐，真实宿主边界保持待验。
