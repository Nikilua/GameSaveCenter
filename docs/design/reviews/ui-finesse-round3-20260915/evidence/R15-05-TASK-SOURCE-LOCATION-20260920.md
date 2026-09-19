# R15-05 任务来源定位证据

日期：2026-09-20

代码提交：`0d1ff346`（`codex/ui-finesse-round2`）

## 结论

R15-05 已实现，当前阶段状态为“已实现，待环境验证”。任务详情现在保留可验证的来源引用，并按稳定身份返回游戏、存档版本、媒体批次或云队列；来源对象不存在时保留任务诊断并明确提示，不按显示名称跳转到同名对象。

## 已有能力核对与实现

- 复用现有 `TaskStatusDto`、`TaskCoordinator`、`TaskEventBroadcaster`、SQLite 任务分页/最近任务查询、`OpenCloudQueue`、媒体归类历史的精确 `BatchId` 恢复和已有任务返回游戏入口，没有另建任务历史或替换游戏选框/滚动系统。
- Contracts 新增 `TaskSourceReferenceDto` 与 `TaskSourceReferenceKind`。`StableId` 是导航身份，`DisplayName`/`Detail` 仅用于诊断；TaskCoordinator 只保留有稳定身份的来源，并为有 `GameId` 的任务补充稳定游戏来源。
- tasks 表通过现有迁移入口增加 `source_references_json`，新增、更新、最近任务、活动任务和分页读取保持一致；事件广播 clone 保留来源引用。旧库空列读取为空，不影响历史任务。
- 恢复/远端暂存任务记录精确 `BackupVersion`；备份云重试和媒体云重试记录由现有 `Backup:<PlayniteId>` / `Media:<PlayniteId>` 组成的 `CloudTransfer`。媒体批次路由复用现有历史页 `BatchId` 分页恢复；当前代码核对未发现现有分类操作会创建独立 TaskCenter 任务，因此没有把普通媒体同步任务冒充媒体批次任务。
- Task Center 新增“任务来源”卡片：游戏继续使用原“查看关联游戏”按钮，其他来源使用共享上下文按钮。版本查找只按 `BackupId`，媒体历史只按 `BatchId`，云队列只接受稳定队列键前缀；缺失时清除待选目标而不保留旧选择。

## 行为验证

本阶段使用合成 DTO、fake/内存任务和隔离 SQLite 目录，没有写真实存档、真实媒体、真实云端、用户云端或对外诊断。

- `R15TaskSourceNavigationTests`：`3/3` 通过。
  - 已删除游戏 + 同名当前游戏：返回空，不改用同名游戏。
  - 已删除版本 + 相邻版本：返回空，不选择邻近版本。
  - 来源 clone 保留稳定 ID、游戏 ID 和诊断详情。
- `TaskQueryPersistenceTests`、`TaskEventBroadcasterTests`、`TaskCoordinatorFailureTests`：`18/18` 通过；来源引用经过 SQLite 最近/分页读取与 Worker 广播路径。
- Playnite R15 回归定向测试：`10/10` 通过；Worker 定向测试：`18/18` 通过。测试 DLL 由注入 `GSC_BUILD_COMMIT=0d1ff346` 的当前分支外部副本生成，未使用旧默认输出。
- `python scripts/validate-source.py`：通过；XAML 结构：`24/24`；`git diff --check`：通过。
- 当前分支源码外部副本的 `dotnet build GameSaveCenter.sln -c Release --no-restore -m:1 -nodeReuse:false -p:NuGetAudit=false -p:MSBuildEnableWorkloadResolver=false -p:GscBuildOutputRoot=...`：`0 errors`，保留 `MediaCenterView.xaml.cs:664` 的既有 `2` 条 nullable warning。Playnite 目标仍为 `net462`。
- WPF 技能静态检查：`0 errors / 28 warnings / 162 info`；本阶段没有新增共享模板级 warning。

## 视觉、宿主与边界

- 这是任务来源行为与可追溯性改动；共享 `GscWpfUiContextButton`、现有详情滚动容器、游戏选框、命令绑定、取消/错误/恢复保护、有限列表和 Playnite/net462 兼容均保留。没有将离屏截图或代理性能写成真实呈现/物理跨屏证据。
- 未启动真实 Playnite/package-host，未验真实游戏库重命名/删除后的宿主呈现、UI Automation/读屏、IME、物理 DPI/跨屏、最终屏幕帧、ETW 或宿主性能；这些不计入本阶段通过。
- Demo 原目录不可用；沿用已恢复的生产基线和当前共享资源，没有自行引入新的设计体系。

下一可执行任务：`R15-06 耗时与吞吐`。先核对已有可靠耗时/进度采样字段和未知总量语义，再决定是否只补证据或补最小实现。R15-05 仍待真实 Playnite 宿主验证来源卡片布局、键盘/UIA 焦点和删除对象提示。
