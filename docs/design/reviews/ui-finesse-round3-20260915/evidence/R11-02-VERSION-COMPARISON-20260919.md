# R11-02 双版本对比选择证据

日期：2026-09-19
分支：`codex/ui-finesse-round2`
范围：存档版本比较页的 A/B 选择、方向交换和同版本负例

## 结论

R11-02 已满足当前可控验收条件。

现有 `BackupCompareRequestDto`、`MessageTypes.CompareBackups` 和 Worker 的 `FileManifestDiffService` 已经定义了 `Left → Right` 的差异语义，本阶段没有新增比较服务、恢复命令或第二套备份选择器。Playnite 端把原先隐含的“上一版本 → 当前版本”收口为显式 A/B 选择：初始选择仍保持原默认方向，用户可在比较页选择任意两个版本并交换方向。

## 实际变更

- `BackupVersionDto` 增加有限的 `ComparisonDisplay`，组合本地时间、类型和稳定 `BackupId`，不保存或展示本地归档路径。
- `DashboardViewModel` 增加 `CompareLeftBackup`、`CompareRightBackup`、`SwapCompareBackupCommand` 和方向摘要；历史详情按钮沿用同一 A/B 选择，不再声称只能比较上一版本。
- A 为基准、B 为对照；界面明确提示“新增属于 B，删除属于 A”。交换操作重新提交已有 `CompareBackups` 请求，因此新增/删除和大小增量由既有 Left/Right 算法随方向重算。
- A/B 为空、缺少稳定 ID或为同一版本时，比较命令不可用并显示明确的“不发起比较或恢复”提示；交换按钮只有已有比较结果且 A/B 有效时可用。
- 保留当前 DataGrid、页面滚动、游戏选框、命令运行器、取消/错误处理、恢复保护和 net462 兼容；比较仍然只发送 `CompareBackups`，没有触碰 `RestoreCommand`。

## 实际验证

- `R11VersionComparisonBehaviorTests`：`2/2`。
  - Core 真实 `FileManifestDiffService` 合成前后清单反向比较：新增、删除、修改和大小增量均随方向正确交换。
  - 真实生产 `SaveCenterView` 在隔离 STA Window 中加载 A/B ComboBox、比较按钮和交换按钮；确认默认 A/B 绑定、方向提示、交换命令入口以及同版本按钮禁用/命令不可执行。
- R11-02 与 R11-01、Save 页面 R06 空态/排序/列宽相邻回归：`15/15`。
- 定向 Release 编译成功：Contracts/Core、`GameSaveCenter.Playnite`（`net462`）和测试（`net472`）；最终输出无 warning/error。
- `python scripts/validate-source.py`：通过。
- `scripts/check-xaml.ps1`：`24/24`。
- `git diff --check`：通过。

## 边界

证据使用合成 manifest、真实生产 SaveCenterView、隔离 STA Window 和逻辑 DIP；没有启动真实 Playnite/package-host，没有读取或修改真实存档、媒体、用户云端或对外诊断数据。未宣称 Worker IPC 实际归档读取、最终 presented frame、物理 DPI/跨屏、UIA/读屏、真实键盘/IME、ETW 或宿主性能通过。Demo 原目录不可用，继续沿用恢复生产基线。

用户提供的 main 合并后安装事实继续独立保留：编译/Core/Worker 成功，但 Playnite 全量为 `73 failed / 588 passed / 57 skipped`，安装器退出 1；本阶段未覆盖 dirty main、未重跑安装器，也未覆盖 main 用户文件。

## 下一步

下一可执行任务：R11-03 差异列表搜索。R11-02 之外的真实 Playnite 安装呈现和上述系统级边界仍未验。
