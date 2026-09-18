# R10-01 上下文返回证据（2026-09-19）

## 结论

R10-01 在当前续作分支的可控验证范围内已满足。实现提交为 `97770ed4`（`补齐上下文返回与任务游戏导航`）。

## 实际实现

- 复用现有 `FindingNavigationResolver`、`TaskStatusDto.GameId`、工作区缓存和既有命令/绑定；新增短生命周期内存 `WorkspaceNavigationSnapshot` 与 LIFO `WorkspaceNavigationStack`，不落盘、不携带路径或写操作令牌。
- 告警导航到存档或失败任务前保存来源；任务详情新增“查看关联游戏”，只按稳定 `PlayniteId` 解析当前 `Games` 快照。缺少对象时保留当前选择，不替换成其他游戏，并给出可行动的刷新提示。
- 返回前先切换工作区，再恢复游戏选择，避免现有选中游戏监听器在错误页面启动详情加载。返回任务时恢复任务搜索、状态/游戏/类型筛选、历史范围、任务导航目标、任务 ID/索引；返回维护中心时恢复维护页签和诊断项稳定选择键。
- `TaskCenterView` 和 `MaintenanceView` 均从实际 DataGrid visual tree 找到内部 `ScrollViewer`，在 `Loaded/Unloaded/ScrollChanged` 捕获和恢复垂直偏移；返回后由 pending restore 属性等待有限列表完成布局，再将偏移限制到当前 `ScrollableHeight`。
- 任务原项在返回后的当前结果中消失时，不回退到其他任务；诊断项或原游戏消失时保留安全空选择并说明原因。

## 验证

- `python scripts/validate-source.py`：通过。
- `powershell -ExecutionPolicy Bypass -File scripts/check-xaml.ps1`：`24/24`。
- `scripts/build.ps1 -SkipTests -OutputRoot .tmp/r10-01-build`：Release solution `0 warning / 0 error`，包含 Playnite `net462` 和测试程序集。
- 定向 Playnite 回归：`R10NavigationBehaviorTests` `2/2`、`PurposeNavigationSourceTests` `3/3`、`FindingNavigationResolverTests` `8/8`，合计 `13/13`。
- `git diff --check`：通过。行为测试使用合成快照/稳定 ID；WPF 滚动证据来自实际生产视图的控件事件接线，不是 `Assert.Contains` 对交互结果的替代。

## 边界与未验事实

- 证据使用隔离构建目录、fake/合成状态和 WPF STA 控件；没有读取或修改真实存档、媒体、云端或用户诊断数据。
- 尚未在真实 Playnite 中运行该返回序列，未宣称物理 DPI/跨屏、最终 presented frame、UIA/屏幕阅读器、IME、ETW、宿主性能或 package-host 通过；Demo 原目录不可用，沿用恢复生产基线。
- 用户提供的 `DEV-INSTALL-008` main checkout 日志显示编译 `0/0`、Core `83/83`、Worker `311/311`，但 Playnite 全量为 `73 failed / 588 passed / 57 skipped`。失败混合了首个 DataGrid 行未生成、STA/AppDomain/PresentationSource 宿主问题、资源字典/布局断言与动效/焦点类错误；本阶段没有在 dirty main 上覆盖、重跑安装器或把该事实改写成 R10-01 失败。

下一可执行任务：R10-02；真实宿主返回序列、物理呈现和 main 合并后的单 checkout 安装器复验仍是未验边界。
