# R19-01 旧请求晚返回证据

日期：2026-09-20  
任务：`R19-01` 旧请求晚返回  
代码：`ed107c50`  

## 结论

R19-01 在受控代码与回归范围内已满足：详情请求现在把启动时的请求代际、工作区和游戏 ID绑定到每个开始、成功、取消和失败回写边界；工作区切换立即失效旧详情代际，媒体详情失败也不会把旧游戏/筛选上下文的错误投影到新上下文。A 的慢成功不能覆盖 B 的新失败，B 的标题、数据、选择和更新时间保持同一上下文。

## 实现与负例

- `CurrentWorkspace` 变化调用 `CancelDetailsLoad()`，同时推进 `detailsLoadGeneration` 和 `mediaPageGeneration`，并取消当前媒体请求。
- `LoadDetailsAsync` 捕获 `requestWorkspace` 与有效 `requestGeneration`，在请求开始、`ApplyOnUi` 成功回写、取消和异常回写前都检查 `IsCurrentDetailsLoad`；判定同时要求取消令牌未取消、代际仍相同、当前工作区相同、当前选中游戏 ID 相同。
- Media 详情的旧异常回写增加同一请求上下文检查；筛选/搜索仍沿用 `InvalidateMediaDetailsContext` 的媒体代际失效。没有改变游戏选框、滚动条、命令绑定、取消/错误语义、恢复保护、有限列表性能或 Playnite `net462` 契约。
- `LatestRequestCoordinatorTests.SlowSuccessFromOldContextCannotReplaceANewContextFailure` 用合成 A/B surface 验证负例：新上下文写入 B 失败后，旧作用域已不再是提交候选，标题、数据、选中 ID、更新时间和失败信息均保持 B。
- 已有 `MediaWorkspaceStateCacheTests.NewContextFailureDoesNotReuseThePreviousContextCache` 继续验证 A 成功后切到 B、A 晚到完成被拒绝、B 首次失败显示 `Error` 且没有旧成功时间；工作区状态和请求源契约也覆盖媒体模式/筛选/页签边界。

## 验证

- clean-tree 隔离 Release solution：`0 errors`；仅有既有 `MediaCenterView.xaml.cs:671` 的 2 条 nullable warning。
- R19-01 及相邻回归：`30 passed / 1 skipped / 31 total`；跳过项是仓库既有环境标记的生产基线项，不是本批失败。
- `python scripts/validate-source.py`、`check-xaml.ps1`（24 files）和 `git diff --check` 通过。
- 只使用合成 surface、fake/request coordinator、隔离 net472/WPF testhost；没有读写真实存档、媒体、用户云端或外部诊断。

## 未验边界

这不是实际 Playnite package-host 下的 IPC 延迟注入、真实页面快速切换录屏或最终 presented frame 证据；真实 Worker/宿主的跨线程调度、物理 DPI/跨屏、UIA/读屏、ETW 和宿主性能仍待环境验证。Demo 原目录不可用，沿用恢复的生产基线。

下一可执行任务：`R19-02` 刷新失败保留草稿，先核对只读刷新与编辑对象的分离、成功回写是否无条件覆盖未保存字段，并补真实负例。
