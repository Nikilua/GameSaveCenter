# R11-06 备份前变更摘要证据

日期：2026-09-19

分支：`codex/ui-finesse-round2`
范围：当前游戏备份前范围预览、已识别路径展示、空数据/失败解释和执行时重新校验

## 结论

R11-06 已满足当前可控验收条件。

先核对最新实现：`LudusaviClient.BackupAsync` 已支持 preview 参数，但原生产入口只调用真实执行模式，没有把本次扫描范围传回 SaveCenter。本阶段复用既有 Ludusavi JSON 解析和 `BackupRequestDto`，新增只读 `backup.preview` IPC；预览调用 `--preview`，不创建备份根目录、不创建任务、不写 SQLite 历史、不上传云端。真实“立即备份”仍调用原 `backup.game` 链路，始终重新扫描，不把旧预览当作安全保证。

## 实际变更

- Contracts 增加 `BackupPreviewDto`/`BackupPreviewPathDto`：区分 Loading、Ready、NoData、Unavailable、Error；显示扫描时间、路径数、预计大小和有限路径窗口。路径返回最多 120 条，避免大清单把 UI 变成无限列表。
- Worker 增加 `backup.preview` 只读路由。`BackupOrchestrator.PreviewAsync` 只读取当前游戏和匹配结果，调用 Ludusavi preview，复用 `LudusaviResultParser.ParseOperationSnapshot` 读取路径/大小；空扫描、工具不可用、未匹配和工具失败分别返回可解释状态。
- `LudusaviClient` 在 preview 模式跳过备份目录创建；真实备份仍保留目录创建和原有任务、校验、历史、云端链路。
- SaveCenter 历史版本摘要卡增加“预览备份”入口、范围摘要、已识别路径和非破坏提示；执行完成或切换游戏后清除旧摘要。现有游戏选框、表格滚动、候选路径、立即备份命令和取消/错误语义保持不变。

## 实际验证

- Worker `BackupPreviewBehaviorTests`：`2/2`。
  - 合成 Ludusavi preview JSON 解析为 2 个路径、384 B，生成只读摘要且不暴露/创建 BackupId。
  - `someGamesFailed` 与 NoData 状态保持区分，失败不会伪装成“没有可纳入路径”。
- `R11BackupPreviewBehaviorTests`：`1/1`；真实 `SaveCenterView`/隔离 STA Window 验证 Ready 摘要卡可见、路径文本进入视觉树，XAML 绑定预览命令和非破坏提示。
- R11-01/R11-02/R11-03/R11-04/R11-05/R11-06 SaveCenter WPF 夹具串行组合：`12/12`。
- 存档页 R06 空态、排序、列宽相邻回归：`11/11`。
- D 盘隔离 Release solution 构建：`0 warning / 0 error`；Playnite `net462`、Worker、测试程序集均从同一隔离输出生成。
- `scripts/check-xaml.ps1`：`24/24`；`python scripts/validate-source.py`：通过；`git diff --check`：通过。

## 边界

验证使用合成 Ludusavi JSON、隔离 STA WPF 和项目测试宿主；没有读取或修改真实存档、媒体、云端或对外诊断数据，也没有运行真实 Playnite/package-host。测试证明 preview 分支的 DTO、解析和生产视图绑定，不等价真实 Ludusavi 版本输出、真实归档文件系统变化、最终 presented frame、物理 DPI/跨屏、UIA/读屏、真实键盘/IME、ETW 或宿主性能。Demo 原目录不可用，继续沿用恢复生产基线。

真实 Worker 进程、Ludusavi IPC 和安装呈现仍未验；本阶段没有绕过 ETW/系统跟踪权限。main 的合并后安装事实继续独立保留：编译 `0 warning / 0 error`，Core `83/83`，Worker `311/311`，Playnite `73 failed / 588 passed / 57 skipped`，安装器退出 1；本阶段未触碰 main，也未在 dirty main 上安装或覆盖。

## 下一步

下一可执行任务：R11-07 备份结果分层。R11-06 之外的真实 Ludusavi/Worker IPC、Playnite 宿主安装呈现和上述系统级边界仍未验。
