# R11-08 测试宿主隔离与当前 main 复核

更新：2026-10-16。针对 GitHub Actions #1043 / run `38889453677` 的 SDK8 job 失败进行复核与夹具隔离修复。

## 失败原因与修复

CI 的 `R11HistoryTimeNavigationBehaviorTests.ProductionSaveHistoryRangeAndNavigationControlsFilterRestoreAndSelectVersions` 在 WPF 布局消息泵中抛出 `NullReferenceException`：合成测试使用 `FormatterServices.GetUninitializedObject` 创建的 Playnite 插件；真实 `SaveCenterView` 加载后连接了 `DataGridColumnLayoutController`，其 400 ms 宽度持久化计时器调用 `DashboardViewModel.PersistUiPreference()`，继而进入 `Plugin.SavePluginSettings()` / `GetPluginUserDataPath()`。这条测试只验证历史筛选、跳转和选择行为，不应触碰 Playnite 设置宿主。

`SaveCenterView` 增加 internal 行为测试回调入口，默认仍使用现有 `viewModel.PersistUiPreference`；R11 合成测试传入空回调。生产页面的持久化路径没有改变。列宽控制器仍由既有 `R06ColumnWidthPersistenceBehaviorTests` 与 `Q13ColumnResizeBehaviorTests` 独立覆盖，含用户宽度保存/恢复、重置、最小宽度与双击恢复 Auto。

## 本机复核

从 `D:\` 启动以选择本机安装的 SDK `9.0.302`；VSTest `17.14.1`，testhost `.NET Framework 4.8.9181.0`。使用新的 `.tmp/r11-fixture-isolation-20261016` 隔离输出，对 Playnite 插件 `net462`、测试 `net472` 与项目引用重新还原并 Release 构建。`GSC_BUILD_COMMIT` 指向构建前 HEAD `2341ca59e8bca692c9e778c2e31e62ba59016768`；源码是该 HEAD 加本次已记录的工作树变更，文件 SHA 见 `hashes-current-main-sdk9302.txt`。

- `R11HistoryTimeNavigationBehaviorTests`：`6/6` 通过。
- `R09FocusOutlineBehaviorTests`：`2/2` 通过；结果与外机日志的旧断言差异见 [R09 身份复核](../CI-R09-OTHER-MACHINE-FAILURE-RECHECK-20261014/README.md)。
- `R06ColumnWidthPersistenceBehaviorTests`：`6/6` 通过。
- `Q13ColumnResizeBehaviorTests`：单独测试进程 `1/1` 通过。

TRX 已随证据归档。一次把 R06 和 Q13 放入同一测试进程的探索性运行遇到 WPF `Application.ShutdownMode` Dispatcher 线程冲突；按仓库 WPF 类隔离方式单独运行后均通过，该混合进程失败不作为代码验证结果。

## 门禁与边界

本机没有仓库要求的 SDK `8.0.423`，本地 SDK9 定向 build/test 不能替代 SDK8 或 SDK10.0.401 CI，也不是完整 solution build。修改推送后必须检查新的两个 SDK Actions job 到终态。#1043 中 SDK10.0.401 已成功，SDK8 在此修复前因上述夹具异常失败；该旧 run 不代表本次修复已获 SDK8 验证。

没有改 XAML、日期过滤/命令/选择业务逻辑、真实插件设置、真实存档或云端；没有运行真实 Playnite host、宿主 UIA/IME、物理 DPI/跨屏或最终呈现帧。
