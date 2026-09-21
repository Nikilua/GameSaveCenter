# R21-03 验证错误播报：现有实现审计与证据

日期：2026-09-21  
分支：`codex/ui-finesse-round2`  
结论：已有实现满足受控 WPF/testhost 范围，阶段标记为“已满足，待环境验证”；本批没有生产代码变更。

## 验收对应

R21-03 的条件是：错误出现时只通知必要变化并关联字段；不把每次键入播报成长段错误；修复后旧错误状态消失。

本次先核对生产路径，确认不是把表格方向误写成代码缺口：

- `GameSaveCenterSettingsView.QueueValidationSummaryUpdate` 已合并编辑变更，避免每次输入同步访问文件系统或重复布局。
- `RefreshValidationSummaryCore` 只把模型范围错误、异步路径结果和当前 `Validation.GetErrors` 合并一次；同一目标和同一消息不重复加入。
- `UpdateValidationFieldHelp` 会把当前字段的错误摘要写入关联控件；没有错误时清空该字段的 `AutomationProperties.HelpText`。
- `entries.Count == 0` 会折叠摘要、定位按钮、详情和常规提示；`RebuildValidationDetails` 每次先清空 Inline 集合再重建错误链接，因此旧错误不会留在下一轮详情中。
- 详情链接使用 `定位错误：{message}` 的 Automation 名称，并把分类、字段和聚焦目标绑定到同一条错误项；这保持了现有命令、Binding、取消/保存和 Playnite 设置草稿语义。

## 实际行为证据

以下是仓库现有的行为测试，不是仅检查源码字符串：

1. `SettingsValidationNavigationBehaviorTests.ErrorLinkSelectsAutomationTabAndFocusesTheFixableField` 在 STA WPF `Window` 中创建真实生产 `GameSaveCenterSettingsView`，使用隔离临时目录和合成 Worker 文件；实际找到“恢复可用性巡检间隔”的 `Hyperlink`，触发 Click 后验证分类切换到自动化页、字段可见并取得键盘焦点、字段 HelpText 关联原错误、滚动容器已移动。
2. `R09FocusOutlineBehaviorTests.SelectedTabAndInvalidTextBoxKeepFocusRingOutsideStateChromeAndRecoverAfterError` 使用真实生产数值校验规则和控件模板：输入越界值 `9` 后 `Validation.GetHasError` 为真且错误边框生效；改为合法值 `2` 后错误消失，焦点填充、边框颜色和边框宽度恢复。该负例覆盖“修复后旧错误状态不继续显示”。
3. `SettingsAsyncValidationTests.SlowOlderRequestCannotOverwriteTheLatestRequest` 与 `CancelSuppressesACompletedRequestAfterThePageLeaves` 覆盖旧异步结果和离页取消，不把已过期的路径错误重新播报或覆盖当前编辑。
4. `SettingsValidationSourceTests.SettingsPageShowsInlineValidationSummary` 逐项核对生产 XAML、字段命名、错误事件合并、详情清空重建、字段聚焦和可取消异步路径校验接线；它只作为结构门禁，不替代上面的 WPF 行为证据。

## 验证结果

在 D 盘隔离 source-copy 上以 `GscBuildCommit=6e94195d` 构建，避免链接工作树 `_wpftmp.csproj` 的既有 `Access denied`：

- Release solution：Playnite `net462` / tests `net472`，`0 errors`；仅有既有 `MediaCenterView.xaml.cs:671` 两条 `CS8602` warning。
- 设置错误导航：`1/1` passed。
- 焦点/错误视觉恢复：`2/2` passed。
- 异步验证、源审计、数值边界：`16/16` passed。
- `scripts/validate-source.py`：passed。
- `scripts/check-xaml.ps1`：24/24 passed。
- WPF 静态质量检查：`0 errors / 27 warnings / 162 info`；warning/info 为既有模板、滚动容器和参考主题基线，本批无新增诊断。
- `git diff --check`：passed；本批 source-copy/build 已清理。

## 边界与下一项

本证据来自生产 WPF 视图、合成/fake 服务和隔离目录；未宣称真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、DPI/物理跨屏、最终呈现、ETW 或宿主性能已验证。Demo 原目录不可用，继续以已恢复生产基线和现有共享样式为参考；main 的用户改动未碰、未合并。

下一可执行任务：进入 R21-04，先盘点后台任务完成/失败/列表加载的现有通知与可再次读取入口，再补不抢焦点、去重进度和失败/成功负例。
