# R14-05 重复媒体识别定向复核

日期：2026-09-24  
工作区：`D:\workplace\github\GameSaveCenter`  
分支：`codex/ui-finesse-round2`  
复核代码身份：`3487288a`（本阶段无生产代码变更）  
实现提交：`136285d5`

## 结论

R14-05 已按当前实现和受控运行结果收口为“已满足，待环境验证”。现有重复识别服务、DTO、IPC 和页面已经满足只读查看要求，本批没有重建服务、DTO 或新的设计体系，只补运行/视觉证据并校正账本。

## 受控行为验证

- Worker `MediaSyncServiceTests 20/20` 通过，其中 `DuplicateInspectionSeparatesMetadataSuspectsFromHashEvidence` 使用隔离 SQLite 合成两个不同 SHA-256 但同类型/文件名/大小相同的媒体，返回一个 `Suspected` 组、组内 `2` 项、确定组 `0`，并确认条目仍为 `Assigned`。
- Playnite `R14ClassificationSelectionTests 4/4` 通过；其中重复识别契约覆盖当前游戏边界、`Certain`/`Suspected` 分组、Recycling、稳定 IPC/刷新命令，并确认重复页片段不存在删除或重新归类命令。
- 当前隔离 Release 构建：XAML `24/24`、解决方案 `0 error`；Playnite 目标 `net462`；保留既有 `src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml.cs:706` 两条 `CS8602` warning。
- `python scripts/validate-source.py`、`scripts/check-xaml.ps1 -ProjectRoot .` 和 `git diff --check` 在本阶段账本更新后通过。

## 受控视觉/几何证据

- 保留并引用 `artifacts/ui-qa-r13-r14-clean-20260922` 的 RenderHarness 报告和截图：报告身份为 `8a9a052e`、`WorkingTreeClean=True`、offscreen logical DIP `1.00`；`Media-1040x700-tab2` 记录重复组列表 `2` 组、组内列表 `4` 项，组内列表高度 `330 DIP`，重复页滚动容器在窄窗口可滚动。代表截图已人工检查，确定/疑似标签、依据、组内路径、时间和大小均可见。
- 该报告全局仍以 `render-qa FAILED` 结束，问题集中在既有 Overview/Settings/Task/Save 基线；本批不把它改写成全局通过，也不把离屏 logical DIP 当作真实 Playnite 呈现、物理 DPI 或性能。

## 语义与边界

- Worker 扫描当前选中游戏的已归档媒体，扫描上限 `5000`，组上限 `100`，每组最多显示 `24` 项；非空 SHA-256 完全一致为确定重复，同类型/文件名/大小一致且未被确定组占用为疑似重复。
- 仅使用合成 DTO、fake/隔离 SQLite、隔离 WPF testhost 和既有 RenderHarness；没有读取或写入真实存档、媒体、用户云端，也没有增加删除/移动入口。
- 真实 Playnite/package-host、Windows UIA/读屏/IME、物理 DPI/跨屏、最终 presented frame、ETW、宿主性能、超大真实媒体库和真实文件权限仍未验；Demo 原目录不可用，视觉基准沿用恢复生产基线。

下一可执行小批量：复核 `R14-06 批量目标防误选`，先核对现有游戏选框、平台/图标/稳定 ID 展示和过滤后选中目标保护，不改造现有选框系统。
