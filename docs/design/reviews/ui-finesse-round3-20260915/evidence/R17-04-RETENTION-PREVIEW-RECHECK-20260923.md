# R17-04 保留预览对比定向复核

日期：2026-09-23  
复核提交：`503c2119`（`codex/ui-finesse-round2`，D 盘工作区 `D:\workplace\github\GameSaveCenter`）  
结论：现有实现满足受控代码与行为门槛，继续保持“已满足，待环境验证”；本批没有新增生产代码，复用实现提交 `3c73b498`。

## 实际复测

- Worker `RetentionSimulationServiceTests`：`12/12`。覆盖候选与用户锁定/PreRestore/健康保护、二次确认、成功隔离与实际释放、共享操作锁忙碌、十分钟过期、候选/策略/归档指纹变化、取消、重复提交，以及 SQLite 索引删除失败后的归档恢复负例。
- 索引删除失败负例实际证明：归档从隔离区恢复原路径，失败计数增加，`MovedBytes=0`、`FreedBytes=0`，恢复账本保留；隔离删除成功才计入实际释放。
- Playnite R17 合并回归：`11/11`，含 R17-01 `5/5`、R17-02 `2/2`、R17-03 `3/3` 和保留预览源码/绑定 `1/1`；布局回归 `20 passed / 11 skipped`，跳过项是已撤销的旧今日工作台架构夹具。
- 从当前 checkout 生成的隔离 Release solution：`0 errors`，`2` 条既有 warning，均为 `MediaCenterView.xaml.cs:706` 的 `CS8602`。
- `validate-source.py`、XAML `24/24`、`git diff --check` 通过；WPF 静态检查 `0 errors / 28 warnings / 162 info`。warning/info 属于现有共享布局、主题与颜色提示，不作为最终呈现通过的依据。

## 保留能力与边界

预览仍是只读的候选/保护项/预计释放/隔离占用对比；Apply 继续要求二次确认、预览句柄与十分钟时效，并在游戏操作锁内重读 live 状态。保留当前游戏选框、滚动条、命令/Binding、错误/取消/恢复保护、有限列表性能和 Playnite/net462 兼容。行为验证使用合成策略、fake/隔离 SQLite trigger、隔离存档目录和临时文件，没有删除真实媒体、存档、数据库或写用户云端。

本批尚未证明真实 Playnite/package-host、Explorer/权限、真实进程锁、文件故障、重启恢复时序、最终浅深主题、DPI/UIA/IME、焦点/滚动、RenderHarness presented frame、ETW、物理跨屏或宿主性能。Demo 原目录不可用，继续以已恢复生产基线为参考；没有绕过系统跟踪权限，也没有把离屏结果写成真实呈现结论。

下一可执行任务：`R17-05 隔离账本入口`，先核对分页隔离列表、原/隔离路径、状态和受控恢复入口，不默认删除残留。
