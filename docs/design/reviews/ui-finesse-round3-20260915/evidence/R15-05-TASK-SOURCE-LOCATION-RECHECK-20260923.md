# R15-05 任务来源定位定向复核

日期：2026-09-23

复核提交：`ba4d624b`（`codex/ui-finesse-round2`，D 盘工作区）

## 结论

R15-05 的受控实现条件已满足，账本状态校正为“已满足，待环境验证”。本批没有新增生产代码，复用了 `0d1ff346` 已提供的任务来源引用、精确导航解析和 Task Center 来源卡片，仅补当前提交上的实际行为复测证据。

## 实际复测

- Playnite `R15TaskSourceNavigationTests`：`3/3` 通过。
  - 已删除游戏与当前同名游戏同时存在时，稳定 ID 不匹配，解析结果为空，不跳到同名游戏。
  - 已删除版本与前后相邻版本同时存在时，精确版本 ID 不匹配，解析结果为空，不选择邻近版本。
  - 来源 clone 保留稳定来源 ID、关联游戏 ID 和诊断详情，避免广播/快照复制时丢失定位依据。
- Worker 来源持久化、广播和失败路径：`TaskQueryPersistenceTests`、`TaskEventBroadcasterTests`、`TaskCoordinatorFailureTests` 合计 `21/21` 通过。来源引用继续经过任务查询、事件 clone 和失败状态路径，未另建历史或导航服务。
- Playnite 定向构建实际生成 `net462` 产物；保留 `MediaCenterView.xaml.cs:706` 两条既有 `CS8602` warning，无新增编译错误。

## 保留能力与边界

- 继续复用 `TaskStatusDto`、`TaskSourceReferenceDto`、现有 SQLite 任务查询、Worker 广播、游戏选框、详情滚动容器、命令绑定、取消/错误/恢复保护和 Playnite/net462 路径；没有用同名或邻近对象兜底。
- 测试使用合成 DTO、fake/内存任务和隔离 SQLite/测试宿主，没有读写真实存档、真实媒体、云端、用户云端或对外诊断。
- 未启动正常的真实 Playnite/package-host，未验删除/重命名后的来源卡片最终呈现、UI Automation/读屏、IME、物理 DPI/跨屏、最终 presented frame、ETW 或宿主性能；这些不计入本阶段通过。
- Demo 原目录不可用，沿用已恢复生产基线，没有引入新的设计体系。离屏或测试宿主结果不冒充真实宿主呈现。

下一可执行任务：`R15-06 耗时与吞吐`。先核对已有可靠采样、未知总量和停顿语义；R15-05 继续保留真实 Playnite 来源卡片与 UIA/焦点边界待验。
