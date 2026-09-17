# R06-07 空表保留结构证据

日期：2026-09-18  
任务：`R06-07 | 空表保留结构`  
结论：在当前可控范围内已满足；代码与行为测试已提交并推送到 `codex/ui-finesse-round2`。

## 1. 现有能力核对

- Task、Media、Maintenance 已有真实状态机和 presenter：首次空、筛选/已处理完为空、加载中、读取失败以及已有数据降级分别由现有状态模型表达，恢复命令分别复用清除筛选或刷新/重新加载命令。
- Save 原先只有 `IsBusy + Count == 0` 的加载判断，首次空、候选处理完成、读取失败会落入同一类空文本，已有数据读取失败也没有降级提示；候选表在加载期间还可能提前显示空态。这是本阶段唯一需要补的生产缺口。
- 当前 checkout 仍没有原始 Demo `DesignShellView.xaml`/`Pages` 目录；沿用恢复生产基线，没有把 main 的旧实现覆盖到当前分支。

## 2. 实现

生产代码提交：`5046bf8f30920065660d38ea03613edb1ea7aaa4`（`补强存档表格空态与失败提示`）。

- `DashboardViewModel` 为 Save 详情请求增加 `WorkspaceDataState` 生命周期：Loading、Empty、Ready、Error、已有数据失败时的 Stale；取消恢复既有取消语义，异常继续向原有 `RunAsync` 错误路径传播。
- Save 历史和候选表继续保留真实 `DataGrid`、表头、列宽、排序、滚动和 `LoadDetailsCommand`；加载中不显示“无数据”，首次成功为空与候选处理完成/本次扫描无新结果使用不同说明，失败显示可重试 presenter，已有数据读取失败保留旧行并显示降级横幅。
- `R06EmptyStateBehaviorTests` 使用实际 `SaveCenterView`、生产状态绑定和合成 `SavePageState`，验证历史 7 列、候选 4 列始终存在；Loading/Empty/Ready/Error 的 presenter、空文案、重试命令及错误优先级均按状态变化工作。
- RenderHarness fake 同步新增 Save 状态绑定，避免离屏夹具绕过生产状态逻辑；未新增服务契约，也未改变游戏选框、滚动条、命令/绑定、取消/错误、恢复保护、有限列表或 Playnite/net462 兼容。

## 3. 自动验证

- `R06EmptyStateBehaviorTests`：`2/2`。
  - 首次加载中历史/候选空文案折叠，成功首次空保留表头并显示历史空态；候选 Ready 状态显示“候选处理完成”语义。
  - 读取失败显示错误 presenter、隐藏空文案，重试入口实际绑定并可执行 `LoadDetailsCommand`；已有数据失败路径由生产状态模型保留旧行并进入降级状态。
- 相邻回归：`WorkspaceStateSourceTests 9 passed / 1 intentional legacy skip`、`TaskCenterViewResponsiveTests 7/7`、`R06TaskProgressBehaviorTests 4/4`。
- `scripts/check-xaml.ps1`：`24/24`；`python scripts/validate-source.py`、`git diff --check` 通过。
- 标准 Release 构建：`0 warning / 0 error`；RenderHarness Release 构建：`0 warning / 0 error`。

## 4. 视觉与布局证据

渲染证据：`.tmp/r06-07-emptytables/emptytables-report.txt`。

- 报告绑定生产代码提交 `5046bf8f30920065660d38ea03613edb1ea7aaa4`，`WorkingTreeClean=True`，结果为 `emptytables OK`。
- Light/Dark、1040×700 与 1600×900 DIP 均通过；Save 历史/候选、Task、Media、Maintenance 的空表/空列表仍保留必要表头或列表结构，空态文案与 presenter 没有覆盖失败状态。报告记录了 Save History/Candidates 的实际列尺寸、TaskGrid、MediaGrid/InboxGrid 和 Maintenance Findings/Device/Process/Audit 表结构。
- 已人工查看 `save-empty-light-1040x700-tab0.png` 与 `save-empty-dark-1040x700-tab1.png`：浅色历史页保留表头和空态说明，深色路径核验页保留表头、规则卡片与候选空态，操作按钮和原滚动结构仍可达。
- `DpiScale=1.00` 仅代表离屏 logical DIP；PNG 与报告不证明真实 Playnite presented frame、物理 DPI、跨屏或系统字体替换。

## 5. 未验边界与下一步

本阶段使用合成 Save/Task/Media/Maintenance 数据、fake 服务、隔离 STA WPF 和离屏渲染，没有启动真实 Playnite/Worker，也没有读取或修改真实存档、媒体、云端或诊断数据。未验真实宿主 UIA/读屏、OS 输入/IME、物理 DPI/跨屏、presented frame、ETW、宿主性能、真实服务失败时序和用户实际重试点击；空态 presenter 的可达性已通过生产命令绑定和隔离 WPF 行为覆盖，但不扩大为宿主验证。

`.tmp/r06-07-emptytables` 是本阶段仍被引用的证据目录，保留报告与代表 PNG；没有新增未被证据引用的 artifacts/.tmp 产物。

下一可执行任务：`R06-08 详情与行高预算`，先核对 Task/Media/Maintenance 现有详情区、选中对象同步和四行门禁，避免把长诊断文本塞进列表行。
