# R13-07 队列筛选与汇总

日期：2026-09-19  
代码提交：`d6c2af90`（`补齐云端队列筛选汇总`）  
分支：`codex/ui-finesse-round2`

## 本阶段收口事实

- 先复用既有 `CloudTransferStatusRequestDto` 的状态/类型筛选、查询一致性 token、分页追加、`existingKeys` 去重和 `SelectionAnchorResolver.Restore` 选中项恢复，没有重建队列或改动命令、取消、错误、恢复保护和有限列表语义。
- 新增游戏名称/Playnite ID 片段、来源设备和最近 24 小时/7 天/30 天筛选。Worker 在同一候选集合上做状态、类型、游戏、设备和 `UpdatedUtc` 边界过滤；摘要的 `TotalCount` 是当前筛选结果，`GlobalTotalCount` 保持未筛选总数。
- 维护页把当前筛选、全局计数和已加载数量分开表达；文本输入使用既有 `DebouncedRefresh`，时间/状态/类型继续沿用选择变更入口。筛选栏改用可收缩列与输入最小宽度，保留现有队列表、滚动条、详情检查器和“刷新队列”命令。
- RenderHarness 的合成 ViewModel 同步暴露新绑定和筛选摘要，避免离屏夹具因新绑定缺失而产生假阴性；这不是呈现通过证明。

## 已执行的证据

- `python scripts/validate-source.py`：通过（退出码 `0`）。
- `scripts/check-xaml.ps1`：`24/24` 文件通过（退出码 `0`）。
- `git diff --check`：通过（退出码 `0`）。
- Worker 定向行为夹具 `CloudStatusFiltersGameDeviceAndTimeWhileKeepingGlobalCount` 已加入：合成三条备份/媒体记录，覆盖游戏、来源设备、时间窗口、全局计数和错误设备空结果；Playnite 源行为夹具覆盖筛选绑定、当前筛选摘要、分页去重和全局计数赋值。

## 尚未验与真实限制

- 本阶段 Worker/Playnite 定向测试尚未执行，不能把新增夹具写成通过；Release solution、Playnite `net462`、RenderHarness 和真实宿主也未宣称通过。
- 当前主机只有 .NET SDK `9.0.302`，`global.json` 要求 `8.0.100` 并允许向上滚动；SDK 下缺少 `Microsoft.NET.SDK.WorkloadAutoImportPropsLocator` 和 `Microsoft.NET.SDK.WorkloadManifestTargetsLocator` 目录。干净 Worker restore 退出 `1` 且没有生成 `project.assets.json`，属于本机 SDK/Workload 环境阻塞，不是代码测试结果。
- linked worktree 直接构建仍有 `obj` 的 Access denied。为隔离构建创建的 `D:\workplace\github\GameSaveCenter\.tmp\r13-07-build` 已清理；`r13-07-source` 首次清理时短暂被 Contracts 子目录占用，阶段末重试已精确删除，未强杀未知进程。
- Demo 原目录不可用，本阶段按已恢复的生产基线和 Demo-first 规则做共享资源/绑定质量检查；未运行真实 Playnite/package-host、真实网络/rclone、最终呈现、物理 DPI/跨屏、UIA/IME、ETW 或宿主性能。未写真实存档、媒体、云端或诊断，也未触碰 main 的用户改动和 `src.zip`。

## 下一步

1. 在具备可用 SDK/Workload 的隔离输出目录重新 restore/build，执行新增 Worker 与 Playnite 定向夹具及相关回归；通过后再把 R13-07 状态从“已实现，待环境验证”改为“已满足”。
2. R13-07 验证收口后，执行 R13-08 定向回归，再核对依赖并推进 `R14-01 归类建议解释`。
