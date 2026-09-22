# R17-05 隔离账本入口（2026-09-20）

## 结论

本项已满足，受控验证完成；真实 Playnite 宿主和最终呈现仍待验。当前提交为 `3002a8dc`（`补充隔离账本路径展示`）。

## 现有能力核对

- `RetentionQuarantineEntryDto` 已持久化 `OriginalPath`、`QuarantinePath`、状态、文件大小、时间和最后错误；分页接口只返回未进入 `Deleted` 的账本项，并保留总数与有限页大小。
- Worker 已有定向恢复入口和确认 DTO。现有隔离 SQLite 夹具覆盖：索引已移除时只清理已知隔离文件、移动中断时恢复原路径且不删除、只处理指定 `EntryId`、分页总量受控。
- 恢复服务遇到路径不安全、原路径冲突或文件身份不一致时保留文件并标记 `RecoveryRequired`，不会猜测、覆盖或默认删除残留。

## 本阶段改动

- 维护行动项为隔离账本单独携带原路径和隔离路径；仅对 `RetentionQuarantine` 项显示两行带完整 Tooltip 的路径信息，有限宽度下换行，不改变现有列表/滚动和分页。
- 将操作文案明确为“受控恢复”，仍复用现有逐条确认、`EntryId` 定向 IPC、`Confirmed = true` 和安全冲突停止语义；没有新增批量删除或替换入口。
- 新增 `R17QuarantineLedgerSourceTests`，只验证路径字段/模板触发和现有确认恢复入口的接线；恢复安全性由 Worker 隔离 SQLite 行为测试提供，不以 `Assert.Contains` 作为唯一交互证据。

## 验证

- Worker `RetentionQuarantineRecoveryTests`：`5/5` 通过。
- Playnite 完整 R17 源码门禁：`12/12` 通过；其中本阶段新增隔离账本路径与入口测试 `2/2`，相关维护报告门禁 `3/3`。
- 当前分支隔离 Release solution：`0 errors`；保留 `MediaCenterView.xaml.cs:664` 的既有 `2` 条 nullable warning；Playnite 目标为 `net462`。
- `python scripts/validate-source.py` 通过；XAML 结构 `24/24`；`git diff --check` 通过；WPF 静态检查 `0 errors / 27 warnings / 162 info`。警告和信息为现有布局/主题提示，本阶段没有新增错误。
- 构建使用 `D:\workplace\github\GameSaveCenter\.tmp\r17-05-solution` 独立输出，验证后已清理；没有把生成物加入 Git。

## 安全与边界

验证只使用合成 DTO、fake IPC、隔离 SQLite 和临时目录，没有读取或修改真实存档、媒体、用户云端或真实生产账本；没有运行真实 Playnite/package-host，因此未宣称最终浅深主题、DPI/UIA/IME、焦点/滚动、Explorer/权限、presented frame、ETW 或宿主性能已验证。Demo 原目录不可用，继续沿用恢复生产基线；main 的用户改动、`src.zip` 和未跟踪对话框文件均未碰、未合并。

下一可执行任务：`R17-06 存储分析导航`，先核对已有存储统计、来源记录和维护页跳转能力，再决定是否需要代码。
