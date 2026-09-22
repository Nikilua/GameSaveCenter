# R14-04 撤销边界定向复核

日期：2026-09-23  
工作区：`D:\workplace\github\GameSaveCenter`  
分支：`codex/ui-finesse-round2`  
复核代码身份：`30de5783`（本阶段无生产代码变更）  
实现/夹具提交：`1c0c5a37`、`a7c39922`、`03521991`

## 结论

R14-04 已按当前实现和受控运行结果收口为“已满足，待环境验证”。本批没有重建撤销服务、DTO 或页面命令；现有撤销链已经以批次、稳定 `MediaId`、应用后归档路径和当前媒体状态做安全边界判断。

## 受控验证

- Worker `MediaSyncServiceTests 20/20` 已在当前隔离 Release 输出运行通过。
- `ClassificationApplyAndUndoMovesOnlyArchiveCopyAndRestoresInboxState` 覆盖应用、Store 重启后的撤销、归档副本回到 Inbox 和原始文件保留。
- `ClassificationUndoLeavesLaterManualDecisionAndArchiveUntouched` 覆盖应用后通过隔离 SQLite 修改收藏/备注再撤销：结果为 `UndoneWithConflicts`，媒体保持 `Assigned`，人工决定保留，应用后的归档文件仍存在，Inbox 副本不被错误重建。
- 当前主分支合并复核的 Release 构建为 XAML `24/24`、solution `0 error/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning，Playnite 目标为 `net462`；`python scripts/validate-source.py`、XAML 结构检查和 diff check 通过。

## 语义与边界

- 撤销前重新读取当前媒体；目标、归档路径、分类状态或原始元数据变化会进入冲突，不执行恢复移动。持久层条件更新失败也保留当前决定，不覆盖后来状态。
- 测试只使用合成数据、fake 服务、隔离 SQLite 和隔离目录，未读取或写入真实存档、媒体、用户云端或对外诊断。
- 本批未运行真实 Playnite/package-host、Windows UIA/读屏/IME、物理 DPI/跨屏、最终 presented frame、ETW、宿主性能或新的 RenderHarness 呈现；Playnite 编译不等于真实宿主行为。Demo 原目录不可用，视觉基准沿用恢复生产基线。

下一可执行小批量：复核 `R14-05 重复媒体识别视图` 的确定/疑似分组与只读门禁；禁止把重复查看扩展成删除或移动真实媒体。
