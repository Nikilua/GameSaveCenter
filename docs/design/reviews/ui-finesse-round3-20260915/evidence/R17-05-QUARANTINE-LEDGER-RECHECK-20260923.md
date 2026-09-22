# R17-05 隔离账本入口定向复核

日期：2026-09-23  
复核提交：`d01ab7ea`（`codex/ui-finesse-round2`，D 盘工作区 `D:\workplace\github\GameSaveCenter`）  
结论：现有实现满足受控代码与行为门槛，继续保持“已满足，待环境验证”；本批没有新增生产代码，复用实现提交 `3002a8dc`。

## 实际复测

- Worker `RetentionQuarantineRecoveryTests`：`5/5`。启动恢复只清理索引已移除且身份已知的隔离文件；移动中断恢复原路径且不删除；分页总量与页大小受控；恢复只处理指定 `EntryId`；隔离占用摘要可持续读取。
- Playnite R17 合并定向测试：`12/12`，包含 R17-01 `5/5`、R17-02 `2/2`、R17-03 `3/3` 和 `R17QuarantineLedgerSourceTests 2/2`。后者确认隔离项显示原路径/隔离路径和完整 Tooltip，并沿用现有逐条确认、`Confirmed=true`、`EntryId` 定向恢复入口。
- 从当前 checkout 生成的隔离 Release solution：`0 errors`，`2` 条既有 warning，均为 `MediaCenterView.xaml.cs:706` 的 `CS8602`。
- `validate-source.py`、XAML `24/24`、`git diff --check` 通过；WPF 静态检查 `0 errors / 28 warnings / 162 info`。warning/info 属于现有共享布局、主题与颜色提示，不作为最终呈现通过的依据。

## 保留能力与边界

账本持久化原路径、隔离路径、状态、文件大小、时间和最后错误；分页只返回未删除条目，受控恢复遇到不安全路径、原路径冲突、文件身份/大小不一致时标记 `RecoveryRequired`，保留文件并停止，不猜测覆盖或默认删除残留。保留当前游戏选框、滚动条、命令/Binding、错误/取消/恢复保护、有限列表性能和 Playnite/net462 兼容。测试只使用合成 DTO、fake IPC、隔离 SQLite 和临时目录，没有读取或修改真实存档、媒体、用户云端或真实生产账本。

本批尚未证明真实 Playnite/package-host、最终浅深主题、DPI/UIA/IME、焦点/滚动、Explorer/权限、真实重启恢复时序、RenderHarness presented frame、ETW、物理跨屏或宿主性能。Demo 原目录不可用，继续以已恢复生产基线为参考；没有绕过系统跟踪权限，也没有把离屏结果写成真实呈现结论。

下一可执行任务：`R17-06 存储分析导航`，先核对已有存储统计、来源记录和维护页跳转能力，再决定是否需要代码。
