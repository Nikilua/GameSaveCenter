# R17-01 健康结果分层定向复核

日期：2026-09-23  
复核提交：`3f9d8e2b`（`codex/ui-finesse-round2`，D 盘工作区 `D:\workplace\github\GameSaveCenter`）  
结论：现有实现满足受控代码与行为门槛，账本状态校正为“已满足，待环境验证”；本批没有新增生产代码，复用实现提交 `eb033251`。

## 实际复测

- Playnite `R17FindingTriageBehaviorTests` 与 `R17FindingTriageSourceTests`：`5/5`。三档影响分组、错误优先、同游戏同稳定代码同问题标题的跨来源去重、不同健康检查备份保留、证据时间以及维护页保留原 Findings Grid 均通过。
- Worker `R17FindingPersistenceTests`：`2/2`。隔离 SQLite 返回记录的证据时间；解决健康 finding 后 `resolved=0` 开放队列为空。
- R17 定向合计：`7/7`。
- 从当前 checkout 生成的隔离 Release solution：`0 errors`，`2` 条既有 warning，均为 `MediaCenterView.xaml.cs:706` 的 `CS8602`。
- `validate-source.py`、XAML `24/24`、`git diff --check` 通过；WPF 静态检查 `0 errors / 28 warnings / 162 info`。warning/info 属于现有共享布局、主题与颜色提示，不作为最终呈现通过的依据。

## 保留能力与边界

复用 SQLite `resolved=0` 开放队列和健康巡检 resolve 入口，保留原问题表、选择、详情、复制、导航、滚动系统、命令/Binding、错误/取消/恢复保护、有限列表性能和 Playnite/net462 兼容；没有把摘要卡冒充修复入口。测试使用合成 DTO、fake/隔离 Worker 和临时 SQLite 目录，没有读取或修改真实存档、媒体、用户配置、云端或诊断上传目标。

本批尚未证明真实 Playnite/package-host、多来源生产标题规范的长期稳定性、最终浅深主题、DPI/UIA/IME、真实焦点/鼠标滚动、RenderHarness presented frame、ETW、物理跨屏或宿主性能。Demo 原目录不可用，继续以已恢复生产基线为参考；没有绕过系统跟踪权限，也没有把离屏结果写成真实呈现结论。

下一可执行任务：`R17-02 诊断包预览`，先核对类别预览、脱敏范围和生成后大小/位置。
