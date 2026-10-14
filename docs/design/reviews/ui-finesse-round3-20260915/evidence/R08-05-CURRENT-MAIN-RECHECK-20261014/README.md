# R08-05 当前 main 页面切换复核

日期：2026-10-14
源码基线：`552b8168f5ace1051cce9ebe7d39ee971c2d97e0`；后续 `8dda9995` 只补诊断/记忆文档，生产与测试源码未变。
本机 SDK：`9.0.302`；MSBuild `17.14.13`；VSTest `17.14.1`；.NET Framework `4.8.9181.0`。仓库 `global.json` 固定 `8.0.423`，本机未安装；本机结果不替代 SDK8/CI SDK `10.0.401`。

## 已有实现与验收覆盖

生产 `AcrylicProductionShellView` 已缓存工作区页面，仅在 `PageHost.Content` 变化时替换内容；R08-05 原始实现/布局计数沿用 `ccd71c27`，快速导航焦点行为由 `8e3cc914` 以来的测试覆盖。本次盘点后未改生产页面、绑定、命令或动画。

- `CachedProductionPagesPreserveBindingSelectionAndScrollAcrossNavigation` 使用真实生产 shell/page、96 条合成 `TaskStatusDto` 与隔离 STA Window。Task→Media→Task 后，页面实例、DataContext、ItemsSource、选中项保持同一引用，DataGrid 垂直偏移 `10→10`；两个不同页面导航各产生 Measure/Arrange `1/1`，同页重入产生 `0/0`；PageHost Opacity 为 1、Effect 为 null、RenderTransform 为 identity。
- `CachedWorkspacePagesRestoreTabsAndTaskFiltersWithoutRefreshing` 检查 Media/Save/Maintenance 页签索引 `2/3/5`、任务筛选值、选中任务、页面与 ItemsSource 身份；任务列表偏移 `18→18`，合成 refresh command 执行 `0` 次，仍有 96 项。
- `RapidNavigationUpdatesPageTitleSelectionAndKeepsFocusOnTheChosenNavigationItem` 通过真实 RadioButton Checked 路由快速切换 Overview/Tasks 共七次，每次核对 workspace、标题、唯一选中导航项和键盘焦点；PageHost 无 Effect/非恒等变换。
- `ProductionPageHostDoesNotDeclareFullPageBlurOrEntranceAnimation` 检查生产导航仅在内容实际改变时替换 PageHost，且没有整页 Blur 或 AnimateEntrance(PageHost)。该源码检查只作辅助；STA 行为测试覆盖实际导航、焦点和状态。

## 构建与行为结果

- 完整 Release solution 使用隔离输出成功，`0 warning / 0 error`；Playnite `net462`、测试 `net472`。隔离 SDK `9.0.302` 由本轮 `.tmp/r09-cross-machine-recheck-20261014/sdk9-cwd/global.json` 选择，构建身份 `GSC_BUILD_COMMIT=552b8168`。
- `R08PageSwitchBehaviorTests 4/4`、`ProductionShellChromeSourceTests 22/22`、`UiFinesseFoundationTests 9/9`、`R02BusyStateTests 5/5`、`R08BusinessFeedbackBehaviorTests 4/4`，共 `44/44`；每类独立 VSTest 进程，0 failed、0 skipped。
- 一次错误的筛选器名没有匹配测试，未计入结果；改用实际类名 `UiFinesseFoundationTests` 后取得 `9/9`。ShellChrome 测试进程在 22 项均结束后输出 WPF TextServices `InvalidComObjectException` 清理异常，但 TRX 为 `22/22`、进程 exit 0；根因未知。
- `scripts/validate-source.py` 通过；XAML 结构校验 `24/24`；文档同步后的 `git diff --check` 通过，只有既有 CRLF/LF 提示。没有 XAML 修改，无视觉变化，不生成截图。

## 证据身份

插件/测试程序集 SHA、MVID、ProductVersion 见 `assemblies.json`；生产壳层与测试源 SHA、脱敏 TRX SHA 见 `hashes.txt`。本轮使用真实生产 WPF 页与壳层，但 DataContext/列表是合成的，Window 为隔离 STA 测试宿主。

## 边界、CI 与下一步

不代表正常 Playnite host、真实 Worker 查询、物理 DPI/跨屏、实际呈现帧、UIA/读屏、ETW 或宿主性能通过。Demo 原始目录当前不可用，继续以恢复的生产基线为视觉参考；本轮没有重选设计体系。

推送前公开 Actions 页仍只显示到 #979（提交 `ae3c565`），#971/run `38393317198` 页面 In progress、#978/run `38509928015` Failure 且完整日志需登录；页面没有显示源码基线 `552b8168`。R09 证据提交 `8dda9995` 已推送后，页面仍未更新，无法确认它的 SDK8/SDK10.0.401 run；继续查到终态，不能用 SDK9 本机结果替代。

R08-05 当前 main 的受控条件已满足，表格保留真实宿主与 SDK CI 边界。下一项按依赖清单复核 R07-08 当前 main 的 resize 行为与先前 CI 证据；不把旧提交 CI 失败原因猜成当前代码问题。
