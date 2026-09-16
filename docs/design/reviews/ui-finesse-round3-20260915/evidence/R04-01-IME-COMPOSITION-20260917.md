# R04-01 组合输入状态证据（2026-09-17）

## 结论

R04-01 在当前可控范围内已满足：可见生产 Shell 的游戏搜索框现在识别 WPF `TextComposition` 的 start/update/commit 路由；组合仍在进行时，选框不会因 Enter 误提交旧游戏，也不会因候选状态误关闭。组合提交后，现有 `ItemsView` 可见候选的 Enter 确认、关闭弹层和焦点回返继续生效。代码提交为 `40c7f9a8596dcdf1ba2d42740023a75f10427452`（`补充游戏选框组合输入保护`），已推送到 `codex/ui-finesse-round2`。

- 先复核 R00-08 的已有能力：无可见结果的 Enter、`Key.ImeProcessed`、方向键、Escape 焦点回返和可见候选 Enter 已在 `AcrylicProductionShellView` 的真实 WPF 路由上覆盖，本阶段没有替换游戏选框或滚动条系统。
- 在 `AcrylicProductionShellView` 构造阶段为 `GameSearchTextBox` 接入 `PreviewTextInputStart`、`PreviewTextInputUpdate`、预览/冒泡 `TextInput`；卸载时清除组合状态，避免 Shell 重载后残留。组合期间 `SelectionChanged` 不进入 `SelectedGame`/关闭流程，`PreviewKeyDown` 不处理 Enter；提交事件到达后才恢复原有候选确认路径。
- `GamePickerKeyboardBehaviorTests.ActiveTextCompositionKeepsPickerOpenUntilCompositionCommits` 使用隔离 Window、真实生产 Shell 和 STA WPF 事件路由，先回放 start/update + Enter 的负例，再回放 commit + Enter 的正例：前者 `Handled=False`、弹层保持可见、旧游戏保持；后者 `Handled=True`、弹层关闭并选择列表候选。该类最终 `4/4`，原有无结果/IMEProcessed/方向键/焦点行为未回归。
- 搜索业务仍复用 `GamePickerViewModel` 的本地缓存、`ItemsView` 和现有短 debounce；英文即时反馈路径未改，也没有把搜索输入转发到 Worker。现有 `GamePickerViewModelTests.SearchAndPlatformFilterUseLocalCacheWithoutWorkerDependency` 继续由全量套件覆盖。

## 验证结果

- clean commit 隔离 Release：XAML 结构 `24/24`；编译 `0` 警告、`0` 错误；Core `83/83`、Worker `311/311`、Playnite `569/626` 通过，`57` 跳过、`0` 失败。
- 首次在未提交工作树上的全量 Playnite 运行曾报告 `2` 个一次性失败；同一隔离输出复跑完整 Playnite 套件为 `569/569` 通过，随后 clean commit 全流程再次为 `0` 失败。该一次性抖动没有被写成通过，也没有放宽测试门禁。
- clean commit RenderHarness 报告：`Commit=40c7f9a8596dcdf1ba2d42740023a75f10427452`、`WorkingTreeClean=True`、`DpiScale=1.00`、`Themes=light,dark`、`357` 张 PNG；1040/1100/1366/2560 DIP 页面、滚动/虚拟化、Shell/resize probes 均通过，报告末尾为 `render-qa OK`。人工抽查 Light/Dark Task 与 Shell 代表图。
- 可复现命令：

```powershell
powershell.exe -ExecutionPolicy Bypass -File scripts/build.ps1 -Configuration Release -OutputRoot .tmp/r04-01-build-clean
powershell.exe -ExecutionPolicy Bypass -File scripts/render-qa.ps1 -Configuration Release -Output .tmp/r04-01-render-final
python scripts/validate-source.py
```

## 边界与保留项

证据使用合成游戏 DTO、隔离 Window、STA WPF 和 offscreen logical DIP；没有启动真实 Playnite UI、修改真实存档、删除真实媒体、写用户云端或发送诊断。`DpiScale=1.00` 不代表物理 DPI 或跨屏结果，RenderHarness 图片不等价最终 presented frame。真实 Windows IME 候选窗口、物理键盘输入法切换、候选翻页时序、Playnite 嵌入宿主输入链、屏幕阅读器、ETW 和宿主帧率仍未验；连续输入/IME/debounce 分配与时延留给 R18-01。命令/Binding、取消/错误语义、恢复保护、有限列表、当前游戏选框、滚动条和 Playnite/net462 目标均保持。

下一可执行任务：R04-02 错误摘要导航；继续先核对现有错误详情、导航和焦点能力，再以小批量真实行为证据推进。
