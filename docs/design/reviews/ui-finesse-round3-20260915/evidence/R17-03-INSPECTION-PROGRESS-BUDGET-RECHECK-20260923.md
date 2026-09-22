# R17-03 检查进度预算定向复核

日期：2026-09-23  
复核提交：`e0b5cd15`（`codex/ui-finesse-round2`，D 盘工作区 `D:\workplace\github\GameSaveCenter`）  
结论：现有实现满足受控代码与行为门槛，账本状态校正为“已满足，待环境验证”；本批没有新增生产代码，复用实现提交 `87473bc3`。

## 实际复测

- Worker `HealthInspectionServiceTests`：`12/12`。隔离 SQLite 游标跨重启恢复、真实隔离 ZIP 成功/损坏、运行中延后且不读归档、操作锁与手动巡检互斥、候选身份先持久化、状态写入失败分类、延后候选不饿死其他候选、取消未完成索引读取、有效备份短路、in-flight 游标优先和并发计划变更保护均通过。
- Playnite R17 合并定向回归：`10/10`，包含 R17-01 `5/5`、R17-02 `2/2` 和 R17-03 `R17HealthInspectionBudgetTests 3/3`；运行中候选、单次预算、取消与最近完成/成功区分、下轮计划和维护页 Automation HelpText 均有断言。
- 从当前 checkout 生成的隔离 Release solution：`0 errors`，`2` 条既有 warning，均为 `MediaCenterView.xaml.cs:706` 的 `CS8602`。
- `validate-source.py`、XAML `24/24`、`git diff --check` 通过；WPF 静态检查 `0 errors / 28 warnings / 162 info`。warning/info 属于现有共享布局、主题与颜色提示，不作为最终呈现通过的依据。

## 保留能力与边界

复用既有持久化游标、时间预算、运行中会话门禁、`GameOperationLock`、延后候选表和取消/失败终态；摘要记录索引总数、需检查数、延后数、候选数和未读取归档，取消/预算/异常不伪装整库完成。保留当前游戏选框、滚动条、命令/Binding、错误/取消/恢复保护、有限列表性能和 Playnite/net462 兼容。测试只使用合成会话/fake 锁、隔离 SQLite、隔离归档和临时目录，没有修改真实存档、媒体、用户配置或云端数据。

本批尚未证明真实 Playnite/package-host、真实游戏运行进程/锁竞争/超时归档/用户目录时序、最终浅深主题、DPI/UIA/IME、焦点/滚动、RenderHarness presented frame、ETW、物理跨屏或宿主性能。Demo 原目录不可用，继续以已恢复生产基线为参考；没有绕过系统跟踪权限，也没有把代理性能或离屏结果写成真实呈现结论。

下一可执行任务：`R17-04 保留预览对比`，先核对保留候选、保护项、隔离账本和执行前预览过期边界。
