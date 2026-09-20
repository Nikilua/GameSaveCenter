# R19-02 刷新失败保留草稿证据

日期：2026-09-20  
任务：`R19-02` 刷新失败保留草稿  
代码：`7b2d3afa`（补充行为回归；生产能力沿用已有实现）  

## 结论

R19-02 在受控生产 VM 边界内已满足：只读详情刷新失败只更新工作区状态，不替换存档/媒体集合或编辑字段；同一稳定版本/媒体 ID 的成功刷新通过 `preserveDirtyFields` 保留用户未保存的备注/锁定/收藏草稿，只有干净字段才接受服务端新值。用户明确取消编辑仍沿用原命令回滚路径。

## 已有实现与行为回归

- 存档 `BackupComment`、`LockSelectedBackup` 和媒体 `MediaComment`、`MediaFavorite` 有独立 dirty 标记；`applyingEditorSelection` 防止服务端同步把字段再次标记为用户编辑。
- `SelectedBackup`/`SelectedMedia` 按稳定 ID 判断同一对象；刷新替换集合后调用 `SyncBackupEditor(value, sameBackup)` / `SyncMediaEditor(value, sameMedia)`，保留 dirty 字段，并清理实际接受服务端值的 dirty 标记。
- `FailSaveDetailsLoad` / `FailMediaDetailsLoad` 只收口状态与错误详情，不调用编辑同步或清空集合；因此失败不会把草稿重置成旧服务端值。
- 新增 `R19DraftRefreshBehaviorTests`：直接在隔离的生产 `DashboardViewModel` 实例上回放存档/媒体刷新失败、同 ID 成功回写和干净字段更新，2/2 通过。该夹具没有连接 Playnite 或 Worker，也没有真实文件写入。
- 既有 `R11VersionNoteBehaviorTests` 的真实 WPF `SaveCenterView` 命令路径继续验证取消按钮恢复原值；`WorkspaceStateSourceTests` 保留编辑草稿/刷新契约。

## 验证与边界

- 7b2d3afa clean-tree 隔离 Release solution：`0 errors`；仅有既有 `MediaCenterView.xaml.cs:671` 的 2 条 nullable warning。
- R19-02、R11 版本备注和工作区状态定向回归：`14 passed / 1 skipped / 15 total`；跳过项是仓库既有环境标记的生产基线项，不是本批失败。
- `python scripts/validate-source.py`、`check-xaml.ps1`（24 files）和 `git diff --check` 通过。
- 只使用合成 DTO、隔离 VM/testhost 和 WPF 行为夹具；未读写真实存档、媒体、用户云端或外部诊断。没有替换 Demo 视觉体系，也未改变游戏选框、滚动条、命令绑定、取消/错误、恢复保护、有限列表或 Playnite `net462`。
- 未验真实 Playnite package-host 下 Worker 断连/慢刷新与可见草稿时序、最终 presented frame、物理 DPI/跨屏、UIA/读屏、ETW 和宿主性能；Demo 原目录不可用，沿用恢复生产基线。

下一可执行任务：`R19-03` 重复执行幂等，先核对已有 request ID、重试恢复和 UI 重复触发边界，不把未知写结果擅自重试或删除。
