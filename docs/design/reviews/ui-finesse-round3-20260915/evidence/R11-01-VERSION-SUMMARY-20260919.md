# R11-01 版本信息摘要证据

日期：2026-09-19  
分支：`codex/ui-finesse-round2`  
范围：存档历史版本行的时间、大小、文件数、来源、保护和恢复校验摘要

## 结论

R11-01 已满足当前可控验收条件。

本阶段复用现有 `BackupVersionDto` 和 `RestoreReadinessDto`，没有新增服务、请求或恢复命令。历史表格继续使用固定列和现有 DataGrid 滚动/列宽系统：设备列改用已有 `SourceDisplay`，空来源显示“未知设备”；状态列改为已有锁定状态与恢复可用性摘要的组合。未知/未检查状态使用中性控件填充，只有 `RestoreReadinessStatus.Ready` 才触发成功色；长校验说明继续放在状态摘要 ToolTip 和右侧版本详情，不推宽表格。

## 现有能力与变更

- 时间、文件数、大小、备注和固定列宽已存在，本阶段未复制 DTO 或重建表格。
- `BackupVersionDto.SourceDisplay` 已提供本地/远端设备缺省文案，本阶段把历史行从原始 `SourceDevice` 切到该显示属性。
- 新增 `ProtectionAndReadinessDisplay`，只组合现有 `LockStateDisplay` 与 `RestoreReadinessStatusDisplay`，例如“已锁定 · 未验证”或“已锁定 · 可恢复”。
- 状态样式移除“`IsLocked=True` 即绿色”的旧触发；Ready 为成功色，Warning 为警告色，Corrupted/Failed 为错误色，Unknown/Checking/未提供校验保持中性。锁定不再冒充恢复安全。
- 右侧版本详情原有恢复可用性说明、指标、检查时间和隔离校验命令保持不变；没有执行真实恢复、扫描或用户文件写入。

## 实际验证

- `R11VersionSummaryBehaviorTests`：`2/2`。
  - DTO 正/负行为验证：空来源为“未知设备”；无 `RestoreReadiness` 为“未验证”，不标记健康保护；Ready 状态显示“可恢复”并保留摘要。
  - 真实 `SaveCenterView` 在 STA Window 中加载历史集合和 7 个生产 DataGrid 列，确认当前 DTO 集合绑定、列标题和状态/来源接线；同时检查未知状态不再由锁定触发成功色。
- R11-01 与 Save 页面相邻回归：`13/13`（R06 空态、排序、列宽和 R11-01）。
- 定向 Release 编译成功：Contracts/Core、`GameSaveCenter.Playnite`（`net462`）和测试（`net472`）；最终定向输出无 warning/error。
- `python scripts/validate-source.py`：通过。
- `scripts/check-xaml.ps1`：`24/24`。
- `git diff --check`：通过。

## 边界

证据使用合成 `BackupVersionDto`/`RestoreReadinessDto`、真实生产 `SaveCenterView`、隔离 STA Window 和逻辑 DIP；没有启动 Playnite/package-host，没有读取或修改真实存档、媒体、用户云端或诊断数据。未宣称最终 presented frame、物理 DPI/跨屏、UIA/读屏、真实键盘/IME、ETW 或宿主性能通过。Demo 原目录不可用，继续沿用恢复生产基线。

用户提供的 main 合并后安装事实继续独立保留：编译/Core/Worker 成功，但 Playnite 全量为 `73 failed / 588 passed / 57 skipped`，安装器退出 1；本阶段未覆盖 dirty main、未重跑安装器。

## 下一步

下一可执行任务：R11-02 双版本对比选择。R11-01 之外的真实 Playnite 安装呈现和上述系统级边界仍未验。
