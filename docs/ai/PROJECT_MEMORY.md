# GameSaveCenter AI/Codex 长期项目记忆

> 维护时间：2026-09-21

## 接续点校正与 R21-04（2026-09-21）

- 用户指出最近应进行到 R12-04；账本核对结果是 R12-04 已在 `e4e42f40` 满足，R12-05 至 R12-08 也已有后续独立记录。本轮不重建、不回滚这些事实，当前分支继续从真实未完成项推进。
- R21-04 复用现有 `TaskEventUiBatcher`、`TaskNotificationDeduper`、`SessionNotificationAccumulator`、`NotificationLevelPolicy`、Dashboard Toast 和 Task Center 状态 Binding。新增 `R21AsyncCompletionAnnouncementBehaviorTests 2/2`，验证终态辅助通知不改变键盘焦点、Automation 文本可回读，列表加载状态面从加载文案变为最近更新时间。
- 定向通知/批处理/会话/任务页行为 `22/22`、Core `5/5`、Worker `5/5`；隔离 Release `0/0`、XAML `24/24`、source/diff、WPF `0/27/162` 通过。相邻旧套件仍有 1 条旧源码断言失败，未改写为绿色。下一项 R21-05。
- 真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、presented frame、ETW、宿主性能和 Demo 原目录仍未验；main 用户改动未碰、未合并。证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R21-04-ASYNC-COMPLETION-ANNOUNCEMENT-20260921.md`。

## 第三轮 R21-03 验证错误播报（2026-09-21）

- 先查最新实现，确认设置页已有合并验证摘要：模型范围错误、可取消路径校验和字段 `Validation.GetErrors` 按目标去重；字段 `HelpText`、错误详情链接和聚焦目标同步清除/更新，空错误集合折叠旧摘要。
- 本批没有生产代码变更。现有实际 WPF 证据为设置错误链接切分类并聚焦字段 `1/1`，数值错误视觉在越界 `9` → 合法 `2` 后恢复 `2/2`；异步最新结果/离页取消、源审计和数值边界 `16/16`。这不是单纯 `Assert.Contains` 签收。
- 干净 D 盘 source-copy Release Playnite `net462` / Tests `net472` 构建 `0 errors`，仅 `MediaCenterView.xaml.cs:671` 两条既有 warning；source/XAML/diff 和 WPF `0/27/162` 通过，临时副本已清理。证据见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R21-03-VALIDATION-ANNOUNCEMENT-20260921.md`。
- 真实 Playnite/package-host、系统 UIA/读屏、OS 输入、IME、DPI/跨屏、最终呈现、ETW、宿主性能和 Demo 原目录仍是未验边界；main 用户改动未碰、未合并。下一项 R21-04：盘点异步完成/失败/列表加载通知、重复进度去重和可再次读取入口。

## 第三轮 R21-02 控件名称与值收口（2026-09-21）

- R21-02 按图标按钮、复合选择器、开关、进度条和负例完成收口，当前账本状态为“已满足，待环境验证”。`R21AutomationValueBehaviorTests 21/21`，相关筛选 `35/35`，最新 Playnite `net462` / Tests `net472` 隔离 Release `0 errors / 2` 条既有 warning，WPF `0/27/162`。
- 行为证据覆盖 UIA 名称、Invoke、Selection、Toggle、RangeValue、HelpText，以及无选中/未知/越界/忙碌/空选择/三态边界；没有替换真实命令、Binding、取消/错误、安全、选框、滚动条、虚拟化或列表性能语义。
- 真实 Playnite/package-host、系统 UIA/读屏、OS 输入、IME、DPI/跨屏、最终呈现、ETW 和宿主性能仍未验；Demo 原目录不可用，main 用户改动未碰未合并。证据见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R21-02-CLOSEOUT-20260921.md`。下一项进入 R21-03 错误播报。

## 第三轮 R21-02 选择器无选中与开关三态边界（2026-09-21，续作小批量）

- `efb42b7b` 复用生产 ComboBox、`ToggleSwitch` 和共享状态模板，新增实际 peer 证据：无选中 ComboBox 的 `GetSelection()` 为 `null`，选中后为单项；三态开关按 `Indeterminate → Off → On` 读取，没有修改生产 XAML 或业务 Binding。
- `R21AutomationValueBehaviorTests 21/21`；相关筛选 `35/35`。提交后 D 盘 source-copy Release 构建 Playnite `net462` / Tests `net472` 为 `0 errors / 2` 条既有 nullable warning；source/XAML/diff 与 WPF `0/27/162` 通过。
- 三态只证明共享控件的承载能力，不代表当前业务开关会产生 Indeterminate；真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、DPI/跨屏、呈现和性能未验。Demo 原目录不可用，main 用户改动未碰未合并。证据见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R21-02-SELECTOR-TOGGLE-NEGATIVE-20260921.md`。下一步继续 R21-02 剩余复合选择器/逐控件状态值负例，之后进入 R21-03。

## 第三轮 R21-02 TaskCenter 任务进度 UIA 值与状态边界（2026-09-21，续作小批量）

- `6b56a467` 复用既有 `TaskStatusDto.ProgressValue`、`ProgressDisplay` 和 TaskCenter 生产 Binding，只新增 `TaskProgressPeerExposesBoundValueAndUnknownStatus` 行为证据；实际 WPF `ProgressBar` peer 验证正常 `42`、未知 `-1`、越界 `120` 的 RangeValue、范围和 HelpText，未知保持 `—`，没有把未知伪装成有效百分比。
- `R21AutomationValueBehaviorTests 20/20`；本批相关筛选 `34/34`。提交后 D 盘 source-copy Release 构建 Playnite `net462` / Tests `net472` 为 `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning；source/XAML/diff 与 WPF `0/27/162` 通过。
- 本批无生产代码变更，不改 DTO、状态投影、命令、取消、恢复保护、选框、滚动条或列表性能。真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、DPI/跨屏、呈现和宿主性能未验；Demo 原目录不可用，main 用户改动未碰未合并。证据见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R21-02-TASK-PROGRESS-PEER-20260921.md`。下一步继续 R21-02 剩余复合选择器/逐控件状态值负例，之后进入 R21-03。

## 第三轮 R21-02 MediaCenter 媒体详情位置值与导航边界（2026-09-21，续作小批量）

- 复核确认生产 `MediaDetailNavigationDisplay` 已提供“未选择媒体”、`1 / 2`、`2 / 2` 值，导航属性按选中索引给出前后边界；`806d5a27` 只补行为证据，没有改变业务实现或媒体写入语义。
- `MediaDetailNavigationValueExposesSelectionBoundaries` 使用合成 DTO、隔离 ViewModel 字段和实际 WPF `TextBlock` peer 验证三态、前后导航负例、生产 Binding 与语义名；`R21AutomationValueBehaviorTests 19/19`，本批相关组合筛选 `22/22`。
- 显式提交身份 D 盘 source-copy Release 构建 Playnite `net462` / Tests `net472` 为 `0 errors / 2` 条既有 nullable warning；source/XAML/diff 与 WPF `0/27/162` 通过，未见本批新增静态诊断。
- 该批不代表真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、DPI/跨屏、呈现和性能；链接 `_wpftmp` `Access denied` 未绕过，Demo 原目录不可用，main 用户改动未碰未合并。source-copy/build 已清理。下一步继续 R21-02 其他状态值边界，公共门禁完成后进入 `R21-03`。证据见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R21-02-MEDIA-NAVIGATION-VALUE-20260921.md`。

## 第三轮 R21-02 MediaCenter 批量动作忙碌态刷新（2026-09-21，续作小批量）

- 复核确认三个 MediaCenter 批量命令的 `CanExecute` 均依赖 `!IsBusy`，但原 `RaiseCommandStatesCore` 漏列三项；`e0805624` 只把它们加入既有刷新列表，没有改变命令执行、选择集合、错误/取消或媒体写入语义。
- `MediaBatchCommandsRefreshBusyCanExecuteState` 先检查生产刷新列表，再用真实 WPF `Button.Command` 与 `RelayCommand` 探针验证启用、忙碌禁用和恢复；`R21AutomationValueBehaviorTests 17/17`，相关套件 `65/65`。
- 显式提交身份 D 盘 source-copy Release 构建 Playnite `net462` / Tests `net472` 为 `0 errors / 2` 条既有 nullable warning；source/XAML/diff 与 WPF `0/27/162` 通过，未见本批新增静态诊断。
- 该批不代表真实 Playnite/package-host、媒体 IPC/写入、UIA/读屏、OS 输入、IME、DPI/跨屏、呈现和性能；链接 `_wpftmp` `Access denied` 未绕过，Demo 原目录不可用，main 用户改动未碰未合并。下一步继续 R21-02 其他状态值边界，公共门禁完成后进入 `R21-03`。

## 第三轮 R21-02 MediaCenter 批量动作空选择保护（2026-09-21，续作小批量）

- 复核确认既有 `UpdateMediaMetadataBatchAsync` 先解析选择，再对 `null`/空 `IList` 抛出“请先在媒体列表中选择一个或多个项目。”；`f7b664f1` 只补该安全边界的行为证据，没有改变命令、Binding、错误/取消或写入语义。
- `MediaCenterBatchActionsRejectEmptySelectionBeforeMetadataWrite` 反射等待真实生产私有异步方法，覆盖 null 和空集合两条负例；`R21AutomationValueBehaviorTests 16/16`，相关套件 `64/64`。
- 显式提交身份 D 盘 source-copy Release 构建 Playnite `net462` / Tests `net472` 为 `0 errors / 2` 条既有 nullable warning；source/XAML/diff 与 WPF `0/27/177` 通过。该批不代表真实 CanExecute/UI Enabled、Playnite host 或媒体写入。
- 链接 `_wpftmp` `Access denied`、真实 Playnite/package-host、UIA/读屏、OS 输入、IME、DPI/跨屏、呈现和性能待验；Demo 原目录不可用，main 用户改动未碰未合并。下一步核对忙碌态/CanExecute 和状态值边界，后续 `R21-03`。

## 第三轮 R21-02 MediaCenter 批量动作名称与 Invoke（2026-09-21，续作小批量）

- 先复用现有三个批量命令和两处操作条；`9fd7223c` 仅为“收藏所选媒体”“取消收藏所选媒体”“为所选媒体应用当前备注”补稳定 Automation 名称，未改变 Content、Command、CommandParameter、样式或批量语义。
- `MediaCenterBatchActionsExposeStableNamesAndInvokeChannels` 用实际 WPF AutomationPeer 验证三类名称各出现两次，并验证三个隔离 `IInvokeProvider` 调用通道；`R21AutomationValueBehaviorTests 15/15`，相关套件 `63/63`。
- 显式提交身份 D 盘 source-copy Release 构建 Playnite `net462` / Tests `net472` 为 `0 errors / 2` 条既有 nullable warning；source/XAML/diff 与 WPF `0/27/177` 通过。Invoke 不代表真实 ICommand、批量选择集合或媒体写入。
- 链接 `_wpftmp` `Access denied`、真实 Playnite/package-host、UIA/读屏、OS 输入、IME、DPI/跨屏、呈现和性能待验；Demo 原目录不可用，main 用户改动未碰未合并。下一步先补禁用/空选择负例，再继续 R21-02，后续 `R21-03`。

## 第三轮 R21-02 MediaCenter 备注与元数据动作（2026-09-21，续作小批量）

- 先复用 MediaCenter 现有 `MediaComment`、`UpdateMediaMetadataCommand`、`ReassignMediaCommand` 和共享控件样式；`b20dac99` 仅补“当前媒体备注”“保存当前媒体元数据”“移动并归类当前媒体”三个 Automation 名称，没有改变业务命令、Binding 或媒体写入语义。
- `MediaCenterMetadataControlsExposeSemanticValueAndActions` 用实际 WPF AutomationPeer 验证名称、`IValueProvider` 的备注更新和两个 `IInvokeProvider` 调用通道；`R21AutomationValueBehaviorTests 14/14`，相关套件 `62/62`。
- 显式提交身份 D 盘 source-copy Release 构建 Playnite `net462` / Tests `net472` 为 `0 errors / 2` 条既有 nullable warning；source/XAML/diff 与 WPF `0/27/177` 通过。Invoke 夹具只代表隔离控件通道，不代表真实 ICommand/Playnite 写入。
- 链接 `_wpftmp` `Access denied`、真实 Playnite/package-host、UIA/读屏、OS 输入、IME、DPI/跨屏、呈现和性能待验；Demo 原目录不可用，main 用户改动未碰未合并。R21-02 继续盘点批量动作和状态/值负例，后续 `R21-03`。

## 第三轮 R21-02 MediaCenter 收藏开关名称与状态（2026-09-21，续作小批量）

- 先复用现有 MediaCenter 详情 `MediaFavorite` Binding、ToggleSwitch 和共享样式；实际缺口只有收藏开关缺稳定 Automation 名称。`42f5744d` 仅补 `AutomationProperties.Name="收藏当前媒体"`，没有改变 Binding、开/关内容、命令、服务、DTO 或媒体写入语义。
- `MediaCenterFavoriteToggleExposesSemanticState` 用实际 WPF AutomationPeer 验证名称及 `Off → On → Off` 状态往返，覆盖回切负例；`R21AutomationValueBehaviorTests 13/13`，相关套件 `61/61`。
- 显式提交身份 D 盘 source-copy Release 构建 Playnite `net462` / Tests `net472` 为 `0 errors / 2` 条既有 nullable warning；source/XAML/diff 与 WPF `0/27/177` 通过。未提交身份初跑被源码身份门拒绝，统一提交哈希后重跑通过。
- 仍只代表合成/fake/隔离 testhost；链接 `_wpftmp` `Access denied`、真实 Playnite/package-host、UIA/读屏、OS 输入、IME、DPI/跨屏、呈现和性能待验；Demo 原目录不可用，main 用户改动未碰未合并。R21-02 继续做其他逐控件状态/值负例，后续 `R21-03`。

## 第三轮 R21-02 SaveCenter 策略控件外置标签与状态（2026-09-21，续作小批量）

- `e1fadaab` 不改生产 XAML，复用 SaveCenter 既有策略 ToggleSwitch、异常保护/策略模板 ComboBox、锁定版本 CheckBox 的标签、选项来源、Binding 与 Automation 名称，补 WPF peer 名称和 Off/On、选值变化证据；没有新造服务、DTO、命令或存档写入语义。
- `R21AutomationValueBehaviorTests 12/12`，相关 R21 进度/焦点/键盘/无障碍/生产壳层回归 `60/60`；提交身份 D 盘源码副本 Release Playnite `net462` / Tests `net472` `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning，source/XAML/diff 和 WPF `0/27/177` 静态检查通过。证据：`design/reviews/ui-finesse-round3-20260915/evidence/R21-02-SAVECENTER-POLICY-PEER-20260921.md`。
- 链接工作树 WPF `_wpftmp.csproj` 写入仍是 `Access denied`，按既有流程用 D 盘源码副本验证并清理；未绕过权限。只用合成/fake/隔离 testhost，真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、呈现和宿主性能未验；Demo 原目录不可用，main 用户改动未碰、未合并。
- R21-02 仍未签收：其他复合选择器及逐控件状态/值负例待继续；之后进入 `R21-03` 错误播报。

## 第三轮 R21-02 Maintenance 云端队列筛选器名称与值（2026-09-21，续作小批量）

- `f3eecad0` 不改生产 XAML，复用 Maintenance 既有三个云端队列 ComboBox 的名称、选项来源和 `CloudTransfer*Filter` Binding，补 WPF peer 名称与“全部/全部时间”到“待处理/媒体/最近一天”的选值变化证据；没有新造服务、DTO、命令或云端写入语义。
- `R21AutomationValueBehaviorTests 11/11`，相关 R21 进度/焦点/键盘/无障碍/生产壳层回归 `59/59`；提交身份 D 盘源码副本 Release Playnite `net462` / Tests `net472` `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning，source/XAML/diff 和 WPF `0/27/177` 静态检查通过。证据：`design/reviews/ui-finesse-round3-20260915/evidence/R21-02-MAINTENANCE-CLOUD-SELECTORS-20260921.md`。
- 链接工作树 WPF `_wpftmp.csproj` 写入仍是 `Access denied`，按既有流程用 D 盘源码副本验证并清理；未绕过权限。只用合成/fake/隔离 testhost，真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、呈现和宿主性能未验；Demo 原目录不可用，main 用户改动未碰、未合并。
- R21-02 仍未签收：其他复合选择器及逐控件状态/值负例待继续；之后进入 `R21-03` 错误播报。

## 第三轮 R21-02 MediaCenter 额外选择器名称与值（2026-09-21，续作小批量）

- `eac4276f` 没有修改生产 XAML，复用 MediaCenter 已有四个选择器的名称、选项来源与 Binding，补 WPF peer 名称和选值变化证据；没有新造服务、DTO、命令或业务值。
- `R21AutomationValueBehaviorTests` 当前 `10/10`，相关 R21 进度/焦点/键盘/无障碍/生产壳层回归 `58/58`；新增 WPF peer 检查四个 MediaCenter ComboBox 名称与选项变化，既有 R06/R12 负例继续覆盖未知/排队/零值/越界/取消/成功和远端阶段边界。没有把源码断言冒充成生产行为。
- 隔离 source-copy Release Playnite `net462` / Tests `net472` `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning；source/XAML/diff 和 WPF `0/27/177` 静态检查通过。证据：`design/reviews/ui-finesse-round3-20260915/evidence/R21-02-MEDIA-EXTRA-SELECTORS-20260921.md`。真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、呈现和宿主性能未验；Demo 原目录不可用，main 用户改动未碰、未合并。
- R21-02 仍未签收：其他复合选择器及逐控件状态/值负例待继续；之后进入 `R21-03` 验证错误播报。

## 第三轮 R21-02 控件名称与值（2026-09-21，部分收口）

- 先查现有 UIA/Automation 接线；Shell、媒体历史、任务预设多数已有稳定名称。本批 `1ca2d01d` 只补 Dashboard/Overview/Maintenance/Trainer 的明确进度名与 TaskCenter 三个筛选器名，保持业务值、命令和 Binding。
- `R21AutomationValueBehaviorTests` 用真实 WPF AutomationPeer 验证 glyph 按钮动作名、ComboBox 名称、ToggleSwitch `Off/On` 和 ProgressBar `Value/Minimum/Maximum`，新增 `2/2`；与 R21-01 相关回归 `33/33`。隔离 Release `net462/net472` `0 errors / 2` 条既有 warning，source/XAML/diff 通过。
- 未签收整项：SaveCenter 外置标签开关、更多复合选择器、DataGrid 内重复进度条和状态/值负例还要继续。真实 Playnite/package-host、UIA/读屏、OS 输入、IME、物理 DPI/跨屏、呈现和宿主性能未验；Demo 原目录不可用，main 用户改动未碰、未合并。证据：`design/reviews/ui-finesse-round3-20260915/evidence/R21-02-AUTOMATION-VALUE-20260921.md`。
- 下一项仍是 `R21-02` 剩余控件；之后才进入 `R21-03` 验证错误播报。

## 第三轮 R21-01 八入口纯键盘（2026-09-21）

- 先查 Q24、`KeyboardFocusSourceTests`、`GamePickerKeyboardBehaviorTests` 和 R05 焦点边界能力；八个生产入口已经有导航/安全动作，没有重建服务、DTO、命令或导航模型。实际缺口是 TrainerCenter 默认工具页四个工具栏按钮缺少稳定 Automation 名称，`c3459ebd` 只补名称并保留原 Command/Binding。
- `R21KeyboardNavigationTraceTests` 用 STA WPF `Window`、独立焦点范围和实际 `TraversalRequest(Next/Previous)` 记录八入口前后向焦点轨迹；新增 `2/2`。相关回归在显式构建身份下 `31/31`，其中既有 R05/GamePicker 夹具覆盖方向键、Enter/Esc、弹层边界和焦点返回。
- 隔离 Release Playnite `net462` / Tests `net472` `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671` warning；source/XAML/diff 通过。Settings 独立构造只在 testhost 添加缺失的 `BaseTextBlockStyle`，不是生产资源改动。
- 真实 Playnite/package-host、OS 输入、UIA/读屏、IME、物理 DPI/跨屏、呈现、宿主性能未验；Demo 原目录不可用，main 用户改动未碰、未合并。证据：`design/reviews/ui-finesse-round3-20260915/evidence/R21-01-KEYBOARD-TRACE-20260921.md`。
- 下一项 `R21-02`：盘点图标按钮、复合选择器、开关和进度条的 UIA 名称/状态/值，先复用已有命名并补负例。

## 第三轮 R20-08 状态语气统一（2026-09-21）

- 先查已有能力：`WorkspaceStatePresenter` 已有 Loading/Empty/Error/Degraded/Offline 投影，`ActionAvailabilityHints` 已有恢复、媒体、云端、远端恢复动作前置解释，`OverviewPriorityResolver` 已有优先状态；本阶段没有新造服务、DTO 或状态模型。
- 实际缺口是 Shell 概览副标题静态声称“一切运行正常”，以及少数主状态把 Worker/Rclone 当作解释。`176183ec` 让副标题随优先状态刷新，统一主状态为“发生了什么 + 下一步”，并用实际 WPF presenter 与解析器正/负例验证；内部诊断标识仍只留在需要的技术区域。
- 定向 `30/30`；相关较宽套件 `25 passed / 1 skipped / 1 failed / 27 total`，失败是未修改的任务详情旧绑定源断言，skip 是 legacy 架构事实。隔离 Release `net462/net472` `0 errors / 2` 既有 warning，source/XAML/diff 通过。
- 真实 Playnite/package-host、Worker/工具/云端、呈现、DPI/UIA/IME、ETW、宿主性能未验；Demo 原目录不可用，main 用户改动未碰、未合并。证据：`design/reviews/ui-finesse-round3-20260915/evidence/R20-08-STATE-TONE-20260921.md`。
- 上一项已推进到 `R21-01`；当前事实见顶部记录。

## 第三轮 R20-07 最近活动密度（2026-09-21）

- 现有链路已满足本项：Worker Dashboard 将 active/recent 任务按 `TaskId` 去重，活动审计经 `ActivityTimelineMapper` 映射为有限摘要；Playnite `TaskEventUiBatcher` 按 TaskId 合并进度、限制 `128/32`，成功/失败/取消终态即时通过。
- Overview 只投影最近 8 条 `OverviewTasks`，使用 Recycling/本地滚动并绑定实际集合计数；TaskCenter 选中项提供失败摘要/错误码、技术详情 Expander、复制/重试和 220 DIP 时间线。已有 R18-05/R15 时间线实现复用，无生产代码新增。
- Core 活动映射 `4/4`；Playnite 定向 `11/11`。组合 WPF testhost 首次的 R06 选择时序失败，单独类进程 `2/2`，记录为环境时序边界而非业务修正。隔离 Release Playnite `net462` / Tests `net472` `0 errors / 2` 既有 warning，source/XAML/diff 通过。
- 真实 Playnite/Worker 实时流、外部工具、呈现、DPI/UIA/IME、ETW、宿主性能待验；Demo 原目录不可用，main 用户改动未碰、未合并。证据：`design/reviews/ui-finesse-round3-20260915/evidence/R20-07-RECENT-ACTIVITY-DENSITY-20260921.md`。
- 下一项 `R20-08`：盘点加载/失败/空/完成/需要操作的状态文案模板，再决定是否有小批量缺口。

## 第三轮 R20-06 部分可用状态（2026-09-21）

- 最新实现已经有可复用的 `ActionAvailabilityHints` 和 `WorkspaceStatePresenter`：恢复、媒体收件箱、云端和隔离远端恢复分别判断动作前置条件；页面分别保留 Loading/Empty/Error/Degraded/Offline 状态与恢复入口，不能用一个全局故障覆盖整页。
- R02/R07/Workspace 定向 `17 passed / 0 failed / 1 skipped / 18 total`。初跑的真实 WPF 测试捕获 `MaintenanceView.xaml` 云端过期横幅把 `DynamicResource` 用于 `Style.BasedOn` 的加载异常，已改用 `StaticResource`，提交 `e32ed599`；这次没有新造状态模型。
- 外部隔离 Release 构建显式绑定 `GscBuildCommit=36dbc3f9`：Playnite `net462` / Tests `net472`，`0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671` nullable warning；source/XAML/diff 通过。测试结束阶段的 TextServices COM 清理噪声记录为环境边界，不计为产品失败。
- 证据只使用合成/fake/隔离 source-copy/testhost；真实宿主、Worker/Named Pipe、Ludusavi/Rclone/云端、最终呈现、DPI/UIA/IME、ETW、宿主性能待验。Demo 原目录不可用，main 用户改动未碰、未合并。证据：`design/reviews/ui-finesse-round3-20260915/evidence/R20-06-PARTIAL-AVAILABILITY-20260921.md`。
- 下一项 `R20-07`：先盘点任务事件摘要、重复进度合并、完成/失败详情展开和计数来源，再决定是否需要小批量代码。

## 第三轮 R20-05 Stale 可理解（2026-09-21）

- 任务中心、存档、媒体和维护页已有 stale 状态路径；云端队列新增最近成功读取时间、错误原因、旧数据保留摘要和重试横幅。失败不清除旧队列记录，不覆盖可用只读详情；无旧记录继续区分失败与零结果。
- `FilterConditionSummary.StaleStateDetail` 统一“上次成功读取/本次刷新失败”表达；定向回归 `37 passed / 1 failed / 0 skipped / 38 total`，唯一失败是未修改的任务详情断言漂移。隔离 Playnite/Tests `0 errors / 2` 既有 warning，source/XAML/diff 通过。实现提交 `c492bbc2`。
- 只用合成/fake/隔离目录，真实 Playnite/package-host、云端/媒体、呈现、DPI/UIA/IME、ETW、宿主性能待验；Demo 原目录不可用，main 用户改动未碰、未合并。证据：`design/reviews/ui-finesse-round3-20260915/evidence/R20-05-STALE-UNDERSTANDABLE-20260921.md`。
- 下一项：从 Q/R 依赖账本选择依赖已满足的小批量，保留真实未验边界，不扩大为无关基线重构。

## 第三轮 R20-04 零结果恢复（2026-09-20）

- 任务中心已有条件摘要、`FilterEmpty` 状态和清除命令，继续复用；游戏选框、媒体中心和云端队列补齐用户可见的条件摘要与清除入口。清除只重置筛选并重新查询，不触碰真实存档、媒体或队列记录。
- 云端队列用 `CloudTransferLoadFailed` 区分读取异常和真实零结果；媒体仍由 `WorkspaceDataState` 区分 Empty/Offline/Error/Stale，游戏选框保留虚拟化列表和选中项恢复路径。`FilterConditionSummaryTests` 覆盖实际游戏选框清除命令及云端读取失败负例。
- 定向行为 `26/26`；受影响套件 `171 passed / 3 failed / 50 skipped / 224 total`，3 条失败是未改动的设置字段、空态覆盖层、下拉模板基线；隔离 Playnite/Tests `0 errors / 2` 既有 nullable warning，source/XAML/diff 通过。实现提交 `cf09f3f6`。
- 只用合成/fake/隔离目录，真实 Playnite/package-host、云端/媒体、呈现、DPI/UIA/IME、ETW、宿主性能待验；Demo 原目录不可用，main 用户改动未碰、未合并。证据：`design/reviews/ui-finesse-round3-20260915/evidence/R20-04-ZERO-RESULT-RECOVERY-20260920.md`。
- 下一项：从 Q/R 依赖账本选择依赖已满足且边界明确的小批量，先查已有实现和负例覆盖。

## 第三轮 R20-03 首次配置引导（2026-09-20）

- 复核确认 `1a90cd06` 已提供 `EnvironmentCheckCard`、`EnvironmentCheckService`、`OnboardingCompleted` 以及运行检查/完成/跳过/测试备份命令；完成只接受最近无失败检查，跳过明确可在维护中心重新运行，检查卡不强制导航。测试备份复用手动 `BackupSelectedAsync`，不会自动执行或改外部工具配置。
- Worker 隔离环境检查 `1/1`；Playnite 当前身份定向 `3 passed / 1 skipped / 0 failed / 4 total`，1 条 legacy 宿主事实跳过。Playnite `net462` / Tests `net472` source-copy `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671` warning，Worker.Tests `0/0`；source/XAML/diff 通过。
- 初次旧产物测试被 `GscBuildCommit` 身份门拦截，固定 `31784686` 后复测；未把身份问题写成行为失败。仅使用合成/fake/隔离目录，真实 Playnite/外部工具/存档/云端/呈现/DPI/UIA/IME/ETW/性能待验，Demo 原目录不可用，main 用户改动未碰、未合并。证据：`design/reviews/ui-finesse-round3-20260915/evidence/R20-03-FIRST-USE-ONBOARDING-20260920.md`。
- 下一项 `R20-04`：核对无结果筛选、条件摘要、清除动作和离线错误边界。

## 第三轮 R20-02 指标统计范围（2026-09-20）

- `DashboardSnapshotDto` 已有 `GeneratedUtc`、全库计数、云端/媒体计数和当前游戏字段，本项复用这些来源，没有新增 DTO/服务或批量写入入口。`OverviewSnapshotDisplay` 提供全库/当前游戏范围、更新时间、未加载 `—` 与合法零 `0` 的格式化；`DashboardViewModel` 在快照/选择/任务/最近访问变化时补发显示属性通知。
- Overview 统计条明确“全库 · Playnite 游戏库”，当前游戏卡片明确“当前游戏 · 与全库快照同步”；未加载时隐藏运行状态胶囊和比例条，避免默认对象被读成真实零/异常，现有比例和零分母逻辑保持。
- 核心定向 `40/40`；概览相关 4 条布局/源码断言 `4 skipped`；更宽筛选 `194 passed / 3 failed / 50 skipped / 247 total`，失败是未修改的设置字段、空态覆盖层、受限下拉模板既有基线。Release 隔离编译 `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671` warning，source/XAML/diff 通过。
- 链接工作树 obj/WPF 临时项目写入继续受 `Access denied` 阻塞，未绕过。只使用合成/fake/隔离目录；真实 Playnite/Worker/外部工具、存档/媒体/云端、presented frame、物理 DPI/跨屏、UIA/IME、ETW、宿主性能待验。Demo 原目录不可用，main 用户改动未碰、未合并。证据：`design/reviews/ui-finesse-round3-20260915/evidence/R20-02-METRIC-SCOPE-20260920.md`。
- 下一项 `R20-03`：先盘点现有环境检查、外部工具设置入口、已完成标记和返回/取消语义，不能自动更改用户配置。

## 第三轮 R20-01 概览下一步（2026-09-20）

- 复用 `OverviewPriorityResolver`、`DashboardViewModel`、`GamePickerViewModel` 和生产 Shell：失败任务进入任务中心“失败”，未匹配/可备份进入现有游戏选框；可备份只设置筛选，不从概览直接发起全库写入。空库继续走既有刷新动作。
- 优先级由快照稳定字段决定，未匹配优先于可备份；新增行为回归覆盖四状态、可备份排除项和无关警告数量变化不跳状态。`OverviewPriorityResolverTests` + `GamePickerViewModelTests` `32/32`，Overview/Shell 交互与源码 `5/5`。
- Release 隔离 source-copy 编译 Playnite `net462` / Tests `net472` 通过，仅有既有 `MediaCenterView.xaml.cs:671` nullable warning；source、XAML `24/24`、diff 通过。链接工作树写 obj/WPF 临时项目受 `Access denied` 阻塞，未绕过权限。
- 仅使用合成 DTO、fake、隔离 testhost/目录；真实 Playnite/Worker/外部工具/存档/云端写入、呈现、物理 DPI/跨屏、UIA/读屏、IME、ETW、宿主性能未验。Demo 原目录不可用，main 用户改动未碰且未合并。证据：`design/reviews/ui-finesse-round3-20260915/evidence/R20-01-OVERVIEW-NEXT-ACTION-20260920.md`。
- 下一项 `R20-02`：盘点概览数字的统计范围、更新时间及未知/未加载表达，确保点击后的列表能解释数字来源。

## 第三轮 R19-08 慢调用可取消（2026-09-20）

- 复核确认已有 `ExternalProcessRunner`/Ludusavi/Rclone timeout+token、外部进程树终止和输出上限；`WorkerIpcClient` 取消/宿主退出/超时边界、同 RequestId 复核及可能已接收语义；`BusyOperationCoordinator` finally 解锁；云端上传只在明确成功时记 `Uploaded`。
- Worker 取消/超时/云队列/ledger `36/36`，Rclone 安全 runner 源码 `1/1`；Playnite 可执行子集 `7/13` 通过、6 条 Named Pipe 跳过。另 2 条旧 net472 源码测试因 `GscBuildCommit` 身份门退出，不计行为失败。本阶段无生产代码变更，source/XAML/diff 通过。
- 真实 Named Pipe、Playnite/Worker 管道断连、Ludusavi/Rclone/远端写入和网络延迟未验；超时/取消不得推断远端写入结果，需只读校验或人工确认，不能以新 RequestId 盲目重试。只用合成/fake/隔离数据，Demo 原目录不可用。证据：`design/reviews/ui-finesse-round3-20260915/evidence/R19-08-CANCELLABLE-SLOW-CALLS-20260920.md`。
- 下一项 `R20-01`：核对概览下一步的四类首屏状态与真实导航入口。

## 第三轮 R19-07 外部文件变化（2026-09-20）

- 复核确认媒体缩略图/录像播放器都有缺失、损坏、不可用占位和迟到结果保护；打开媒体/目录通过统一本地错误回报，目录在目标消失时只回退到真实存在的父目录，刷新/重新加载保留稳定媒体上下文。
- `RestoreReadinessService` 对 ZIP/Manifest/哈希/隔离目录错误分层，`RestoreOrchestrator` 阻止最近失败或损坏版本进入真实恢复；Playnite 重新验证后重载详情，归档路径不静默猜测，重新定位需显式路径检测/设置。
- Worker `14/14`，Playnite 纯行为 `7/7`；组合 WPF `17/21` 的 4 条是旧 net472 `GscBuildCommit` 身份门失败。无生产代码变更，source/XAML/diff 通过。证据：`design/reviews/ui-finesse-round3-20260915/evidence/R19-07-EXTERNAL-FILE-CHANGE-20260920.md`。
- 只用合成 ZIP/Manifest、隔离目录、fake/testhost，未写真实存档/媒体/云端；真实移动/占用/损坏宿主时序、Explorer/播放器和呈现待验。下一项 `R19-08`：核对慢调用取消、超时、UI 解锁与未知写结果。

## 第三轮 R19-06 分页快照变化（2026-09-20）

- 复核确认 Worker 任务/媒体查询使用稳定 `(时间, 稳定 ID)` 游标、严格小于谓词和 limit+1 末页判定；Playnite reset 会推进 generation/清空 cursor，同上下文翻页使用当前 cursor，刷新是快照变化的明确重载策略。
- `MediaPageAccumulator` 按 `MediaId` 去重和更新、窗口最多 `2000`，Task 按 `TaskId` 合并，选择按稳定 ID 恢复。Worker `12/12`、Playnite 分页/索引 `10/10`、选择锚点 `4/4`；合并筛选唯一旧 R06 `GscBuildCommit` 身份失败已拆出。
- 本阶段无生产代码变更；source、XAML `24/24`、diff 通过，沿用 `6d1a401b` clean-tree Release `0 errors/2 existing warnings`。只用合成/fake/隔离 SQLite/testhost，未验真实并发变更和宿主呈现。证据：`design/reviews/ui-finesse-round3-20260915/evidence/R19-06-PAGED-SNAPSHOT-20260920.md`。
- 下一项 `R19-07`：核对外部文件被移动、占用或损坏时的媒体/备份详情回退、诊断上下文和重新定位入口。

## 第三轮 R19-05 Worker 重启恢复（2026-09-20）

- 复核确认 Worker 初始化先 reconcile durable task，再恢复云传输/整库备份；`TaskEventBroadcaster` 每连接 bounded queue 容量 `128`、终态优先，事件 pipe 每连接单独订阅并在断开释放；Playnite 单事件 token + 指数退避，change feed/SQLite 是重连事实来源。
- Worker event/reconcile/cloud `18/18`，Playnite `TaskEventUiBatcher` `3/3`，XAML/source/diff 门禁通过。没有新增第二套恢复服务或修改游戏选框、滚动、命令绑定、取消/错误语义。
- 独立 Worker 硬重启因 Named Pipe 权限跳过；相邻 subscription 测试一条被旧 net472 产物缺 `GscBuildCommit` 阻塞，不宣称真实重启/管道通过。只用合成事件、隔离 SQLite/fake/testhost，Demo 原目录不可用。证据：`evidence/R19-05-WORKER-RESTART-RECOVERY-20260920.md`。
- 下一项 `R19-06`：核对分页 durable cursor、快照变化与末页稳定选择。

## 第三轮 R19-04 取消关闭顺序（2026-09-20）

- 复核确认生产 Dashboard 卸载、`CancelDeferredUiWork`、generation/`LatestRequestCoordinator`、任务事件 batcher、插件 lifetime 和 Worker owned-process 停止已经形成确定收尾链；Worker 重开先按 SQLite 把旧 Queued/Running 标为失败，任务页可恢复真实状态，不伪装成功。
- `LatestRequestCoordinator` + Busy `5/5`，`TaskReconcileService` `1/1`，取消/重启相邻 Worker 组合 `13 passed / 1 skipped / 14 total`。已有行为夹具覆盖取消失效、迟到结果丢弃、busy 恢复、持久化协调幂等和终态保留。
- Worker 进程硬重启因 Named Pipe 权限跳过；WPF shutdown 源测试被旧 net472 产物缺 `GscBuildCommit` 的身份门阻塞；不宣称真实宿主收尾或全量 Playnite 运行通过。只用合成/fake/隔离 SQLite/testhost，Demo 原目录不可用。证据：`evidence/R19-04-CLOSE-CANCEL-ORDER-20260920.md`。
- 下一项 `R19-05`：在可用 Named Pipe/Worker 进程环境复跑重启、进度订阅和重连去重。

## 第三轮 R19-03 重复执行幂等（2026-09-20）

- 复核确认现有 `RequestId`、replay-protected 语义分类、Worker 持久化 ledger 和 `WorkerIpcClient` 同 envelope 复核已经满足单次执行；响应丢失不会以新 ID 重复写入，`REQUEST_IN_PROGRESS`/`REQUEST_INTERRUPTED`/可能已提交均明确提示未知边界。生产 `BusyOperationCoordinator` 以原子门拒绝重复 UI 触发，并在失败/取消后恢复。
- `IpcRequestLedgerTests`/IPC 边界 `16/16`，生产 Busy/按钮 `2/2`；XAML `24/24`、source validation、diff check 通过。未新增重复写服务或修改命令绑定、取消/错误/恢复语义、选框和滚动系统。
- 真实 Named Pipe 客户端测试因环境权限跳过；linked WPF `_wpftmp` 构建 Access denied，外部副本 restore 被 `NU1301` 拒绝，未绕过系统权限；不宣称真实管道或完整 solution 通过。只用合成/fake/隔离数据，Demo 原目录不可用。证据见 `evidence/R19-03-IDEMPOTENCY-20260920.md`。
- 下一项 `R19-04`：核对取消关闭顺序、Worker 断开后的 deterministic cleanup、重开后的任务恢复和 `IsBusy` 清理。

## 第三轮 R19-02 刷新失败保留草稿（2026-09-20）

- 核对确认生产 `DashboardViewModel` 已把只读刷新和编辑字段分开：四个备注/锁定/收藏字段各自有 dirty 标记，稳定 ID 替换对象时 `Sync*Editor(..., preserveDirtyFields)` 只更新干净字段；`FailSaveDetailsLoad`/`FailMediaDetailsLoad` 只更新状态/错误，不清集合或编辑值。
- `7b2d3afa` 只新增直接调用生产 VM 同步/失败边界的合成行为测试，存档和媒体 `2/2` 通过；R11 实际 WPF 备注取消与工作区状态定向合计 `14 passed / 1 skipped / 15 total`。没有引入第二套 Draft 服务。
- clean-tree Release `0 errors/2 existing warnings`，source/XAML/diff 门禁通过。证据：`evidence/R19-02-DRAFT-REFRESH-20260920.md`。
- 只用合成/fake/隔离 testhost，真实 Playnite/Worker 断连时序、DPI/UIA/读屏、presented frame、ETW/宿主性能未验，Demo 原目录不可用。下一项 `R19-03` 重复执行幂等。

## 第三轮 R19-01 旧请求晚返回（2026-09-20）

- `ed107c50` 先核对现有 `LatestRequestCoordinator`、媒体请求代际、`MediaWorkspaceStateCache` 和页面重访门，再修正详情请求的真实跨工作区缺口：`CurrentWorkspace` 变化会推进详情/媒体代际并取消请求，`LoadDetailsAsync` 绑定启动 workspace、generation、game ID，所有成功/取消/失败 UI 回写均需通过同一 `IsCurrentDetailsLoad`。
- Media 详情异常以前只看 media page generation；现在旧游戏/筛选请求的异常也必须通过详情上下文，避免把 A 的失败状态写到 B。筛选/搜索现有 `InvalidateMediaDetailsContext`、选择恢复、取消和 net462 IPC 契约保留。
- `LatestRequestCoordinator` 的合成 A 慢成功/B 新失败负例通过；媒体缓存 A/B 首失败/旧完成负例和工作区状态/分页/锚点/重访相关回归合计 `30 passed / 1 skipped / 31 total`。clean-tree Release `0 errors/2 existing warnings`，source/XAML/diff 门禁通过。
- 证据：`evidence/R19-01-LATE-REQUEST-CONTEXT-20260920.md`。验证只覆盖合成/fake/隔离 testhost；真实 Playnite IPC 延迟、跨线程宿主时序、presented frame、DPI/UIA/读屏、ETW/宿主性能未验，Demo 原目录不可用。下一项 `R19-02` 刷新失败保留草稿。

## 第三轮 R18-08 低性能降级触发（2026-09-20）

- `3bfe3d3c` 复用现有无玻璃/无动画回退：`AdaptiveThemePaletteFactory` 使用真实 null Effect 和不透明表面，`GscMotion` 继续受用户设置、高对比度和 `ClientAreaAnimation` 约束；RenderHarness 只做明确 `glass=false/motion=false` 模拟，不伪造 Render Tier。
- clean-tree lowcostprobe 覆盖六工作区、双主题、1040×700/1600×900 共 24 组合，资源回退全部符合预期：Effects null、PopupTransparency=False、PopupAnimation=None、环境层 opacity=0、visibleEffects=0、可见文本 4–147、非输入框 unexpectedOverflow=0、`lowcostprobe OK`。
- 首轮诊断定位 Media Inspector `MediaClassificationPreviewItems` 的真实水平溢出，原因是 ListBox 横向 Auto 让内部 StackPanel 无限宽测量；修复 preview/history 两个列表为 Horizontal Disabled，保留垂直有限列表、Recycling、路径 TextBox 合法内容滚动。证据：`R18-08-LOW-PERFORMANCE-FALLBACK-20260920.md`；原始目录：`.tmp/r18-08-lowcost-final-clean/`。
- 直接相关测试 `37/37`，Release `0 errors/2 existing MediaCenter nullable warnings`，source/diff 通过；联合 R14 筛选的 2 个旧源码断言失败单独记录，不能写成全量通过。
- 这不是真实低 Tier GPU、RenderCapability.Tier、Playnite host、物理 DPI/跨屏、UIA/读屏、presented frame 或 ETW 证据。Demo 原目录不可用。下一可执行任务：`R19-01` 异步竞态与故障恢复入口。

## 第三轮 R18-07 长时资源曲线（2026-09-20）

- `56d8e1b7` 复用既有 `RunEnduranceProbe`，保留原始时间序列并记录托管堆、私有字节、工作集、线程、句柄、探针可见计时器、反射可见托管事件委托、`HasAnimatedProperties` 动画持有者代理和缩略图缓存诊断；不强制 GC，不把代理指标扩大解释。
- 最终隔离 WPF STA 运行 `1800s` 操作 + `30s` 停止输入静置，`1830.2s/1830`、`177` samples、`2769` cycles、`8537` actions、`0` failures、`enduranceprobe OK`。静置样本周期保持 `2769`、`timers=0`；private `316,137,472→268,775,424`、working set `345,202,688→298,184,704` 后最后两点稳定。
- 静态/行为门禁：相关源测试 `31/31`，Release 隔离构建 `0 errors/2 existing MediaCenter nullable warnings`，source validation/diff 通过。证据：`R18-07-LONG-RUN-RESOURCE-CURVE-20260920.md`；原始序列：`.tmp/r18-07-endurance-final/enduranceprobe-report.txt`。
- 报告元数据 `WorkingTreeClean=False` 是运行时静置 patch 尚未提交的事实；随后由 `c5c33e18` 固化，未篡改报告。`subscriptions=1`、`animated_owners=0`、`thumb_cache=0/96` 是探针限定或本场景未触发，不能冒充全局 WPF 订阅、精确动画时钟或缩略图解码证据。
- 仅使用合成/fake/隔离目录，无真实存档、媒体、云端或诊断写入；未绕过 ETW/系统跟踪权限。Demo 原目录不可用，真实 Playnite/package-host、物理 DPI/跨屏、UIA/读屏、presented frame、ETW 与宿主性能仍未验。下一可执行任务：`R18-08 低性能降级触发`，先盘点无玻璃/无动画回退与渲染 Tier。

## 第三轮 R18-06 页面重访成本（2026-09-20）

- `1b19fd4c` 先复核生产壳层的六页 registry、同页 `PageHost.Content` 复用、Dashboard 页面级读取、generation/cancellation 和 `WorkspaceDataState` presenter；没有重建页面或整库刷新服务。
- `WorkspaceRevisitLoadGate` 以 15 秒为短时热态门，key 按 workspace、稳定游戏 ID 和 Media 筛选/搜索/收件箱模式隔离。成功才记新鲜度；失败/取消、卸载 `CancelDeferredUiWork`、上下文变化和失效后的晚返回均不能发布成功时间。
- 壳层记录 `[PERF] WorkspacePages` binding、`WorkspaceActivation` first/revisit attach/reuse/layout，VM 记录 `[PERF] WorkspaceLoad` read/skip/outcome/load；显式 `LoadDetailsCommand` 不经门。`WorkspaceRevisitLoadGate`/source `7/7`、相关回归 `31/31`、Release solution `0 errors/2 existing warnings`、source/XAML/diff 通过。
- 日志只覆盖生产代码同步委托/布局路径；本轮没有真实 Playnite 首次/重访样本，不将其写成 presented frame、DPI/UIA、ETW 或宿主性能证据。只用合成/fake/隔离 testhost，无真实存档、媒体、云端或诊断写入。下一可执行任务：`R18-07 长时资源曲线`。

## 第三轮 R18-05 后台事件合并（2026-09-20）

- `91947336` 先复核已有 Worker 事件扇出、durable change feed、TaskId 索引、批量 ObservableCollection 和 Dashboard 卸载取消；没有重建媒体/任务服务。UI 进度按 TaskId 合并，pending 上限 `128`、单批 `32`；终态绕过进度队列立即显示，清理同 TaskId 旧进度。
- Worker 订阅维持固定 `128` 容量，满载只淘汰非终态；终态压力样本保留 Failed。断线/极端终态超出瞬时容量时，持久化 TaskChangeFeed 和快照仍是事实来源，事件管道不是永久日志。
- `TaskEventUiBatcherTests 3/3`、Worker 事件 `5/5`、相关 Playnite `19/19`；source、XAML、diff 和 Release `0 errors/2 existing warnings` 通过。证据：`R18-05-BACKGROUND-EVENT-BATCHING-20260920.md`。
- 只用合成 DTO、fake 调度和隔离目录；不把 Dispatcher/集合证据写成 presented frame、ETW、真实 Playnite、DPI/UIA 或宿主性能。下一可执行任务：R18-06 页面重访成本。

## 第三轮 R18-04 表格容器预算（2026-09-20）

- `64843642` 先复用生产 Task/Media DataGrid、MediaPageAccumulator 和共享模板；实际发现 Media Inbox 在外层页级 ScrollViewer 常态 Auto 且内层未先获得有限布局时，Standard/Item/禁列组合会生成完整 2,000 行。修复为正常高度显式有限 `Height/MaxHeight`、外层纵向滚动仅短页/stale fallback 开启，未改 Standard/Item/禁列例外、命令绑定、选择锚点或游戏选框。
- 真实 STA WPF 2k/10k/20k 合成规模的 Task/Media 最大容器分别稳定为 `9/9`，视口分别 `7/9` 行；Media UI 窗口始终 `2,000`，Task/Media 滚动 p95 最大分别为 `52.846/52.846`、`28.365/28.365`、`34.807/34.807ms` 与 `0.132/0.132`、`0.289/0.289`、`0.153/0.153ms`。证据：`R18-04-TABLE-CONTAINER-BUDGET-20260920.md`。
- R18-04 `1/1`，媒体分页/锚点/几何/滚动相关 `23/23`，Release `0 errors/2 existing warnings`，source/XAML/diff 通过。首轮“先 Show 后布局”夹具真实记录了 2,000 全量容器负例；最终夹具首次 measure 前应用响应式布局。c17 的 DynamicResource BasedOn 解析错误也在本阶段改为 StaticResource 并收复两个锚点回归。
- 证据仅是受控 WPF 逻辑 DIP、容器和滚动更新，不是 Playnite 首次 Loaded/真实宿主、物理 DPI/跨屏、UIA/读屏、presented frame、ETW 或宿主性能；Demo 原目录不可用，真实数据/云端未触碰。下一可执行任务：R18-05 后台事件合并。

## 第三轮 R18-03 缩略图滚动预算（2026-09-20）

- `e54d514e`/`18c5073f` 先复核现有 `AsyncThumbnailLoader`/`AsyncThumbnailImage`：3 路解码、96 项 LRU、不可见/卸载取消、generation 成功/失败双侧防迟到均已存在；本轮只补证据夹具。
- 120 个合成 PNG 的 10 个快速窗口实测请求/解码 `120/120`、峰值活动 `3`、缓存封顶 `96/96`、每轮结束活动 `0`；托管堆代理原始最大 `90,072 bytes`；预取消 `1`；替换后旧 Missing 结果没有改写 Ready 新行。证据：`R18-03-THUMBNAIL-BUDGET-20260920.md`。
- R18-03 `1/1`，AsyncThumbnailLoader/Image 回归 `9/9`，Release solution `0 errors/2 existing warnings`，source/XAML/diff 通过。c17 的 `64×64` 尺寸断言按当前 `PreviewWidth=96` 实测校正为 `96×96`，生产代码未改。
- 只用合成图片、隔离目录和 STA testhost；`GetTotalMemory(false)` 是托管堆代理，不能替代 ETW/显存/呈现帧/真实 Playnite。下一可执行任务：R18-04 表格容器预算。

## 第三轮 R18-02 真实 Dispatcher 基准（2026-09-20）

- `59468b37` 增加实际 STA WPF 受控窗口夹具，`5b28b0c3` 校正来源标签；VM 完成点来自内部刷新计数/查询身份，画面反馈点来自 `ListBox.ItemContainerGenerator` 首容器的可见性和实测布局几何，不再把 `FilteredCount` 当成画面延迟。
- 20 次原始样本的 VM p95/最大为 `52.272/63.581ms`，VM→可见容器增量 p95/最大为 `28.343/49.942ms`；可见计数始终 `1`，容器 `476×19.24 DIP`。证据：`R18-02-DISPATCHER-VISIBILITY-20260920.md`。
- 受控窗口显式安装 WPF `DispatcherSynchronizationContext`；未安装的首轮负例暴露了测试宿主与真实 Dispatcher 路径的区别，修正后 R18-02 `1/1`，合并相邻回归 `47/47`。Release solution `0 errors/2 existing warnings`，source/XAML/diff 通过。
- 这不是 presented frame、DWM、物理 DPI、60fps、Playnite 嵌入、UIA/读屏、真实 OS IME 或 ETW 证据；只用合成 DTO/隔离 STA/受控窗口。下一可执行任务：R18-03 缩略图滚动预算。

## 第三轮 R18-01 连续输入基准（2026-09-20）

- `10bc5789` 只在 `GamePickerViewModel` 增加内部性能诊断快照和固定 20ms 防抖常量，并新增 R18 合成基准；既有本地缓存、`SetItems` 批量更新、取消和同步 `RefreshNow` 路径保持。
- 2,000/10,000 项各回放 30 次连续英文查询，p95/最大同步过滤更新为 `4.908/6.121ms` 与 `12.115/13.813ms`；连续过滤评估为 `60,000` 与 `300,000`；粘贴、删除、已提交中文 IME 查询均按可见结果断言，防抖快速输入最终各只刷新一次。
- R18 基准 `1/1`，游戏选框/键盘/IME/防抖相邻回归 `46/46`；Release 隔离 solution `0 errors/2 existing MediaCenter nullable warnings`，source、XAML `24/24`、diff 通过。证据：`R18-01-CONTINUOUS-INPUT-20260920.md`。
- net472 无线程分配 API，测试使用 `GC.GetTotalMemory(false)` 前后非负差值，原始数组完整写入证据；这是托管堆变化代理，不能替代 ETW、分配调用栈、私有字节、呈现帧或真实宿主性能。无 XAML/资源改动，WPF `0/27/162` 沿用最近静态基线。
- 只用合成/fake/隔离目录和 testhost；Demo 原目录不可用，main dirty 用户改动未碰、未合并，阶段 `.tmp/r18-01-solution` 已清理。下一可执行任务：R18-02 真实 Dispatcher 基准。

## 第三轮 R17-08 维护报告可读性（2026-09-20）

- `59d4190b` 复用原维护报告 Worker 服务、DTO、IPC 和 Playnite 导出命令；新增报告请求身份 DTO，输出按“软件身份/摘要/待处理/已验证/未知”固定分组。
- 报告以一次 `generatedUtc` 同时生成 DTO 时间、正文时间和摘要计数；每个分组标题的条目数与摘要一致。`MaintenanceReportRedactor` 统一移除 URL 参数/片段、URL 凭据和 Windows `Users` 用户名，报告不携带真实敏感值。
- Worker R17-08 `2/2`、Playnite `4/4`，合并相关回归 `5/5` 与 `19/19`；隔离 Release solution `0 errors/2 existing warnings`、source/XAML/diff、WPF `0/27/162` 通过。证据：`R17-08-MAINTENANCE-REPORT-20260920.md`。
- 未验真实宿主文件导出/剪贴板、最终呈现、DPI/UIA/IME/读屏、物理跨屏、ETW 和宿主性能；Demo 原目录不可用，继续恢复生产基线，main 用户改动未碰、未合并。
- 下一可执行任务：`R18-01 连续输入基准`，先查 GamePicker 搜索、DebouncedRefresh、IME 和 2,000+ 项合成基准。

## 第三轮 R17-07 检查项一键定位（2026-09-20）

- `e8d581c6` 先复用既有 `FindingNavigationResolver`、精确游戏解析、任务/云队列入口和 `WorkspaceNavigationStack`；没有新增第二套导航栈。健康巡检 Finding 补稳定 `BackupId`，版本页按 `PlayniteId + BackupId` 精确选择。
- SQLite `findings` 对旧表执行 `backup_id` 增量迁移；旧健康标题前缀可兼容读取。缺失游戏或版本不选择邻居，缺少健康版本身份不伪装成失败任务；返回维护中心继续恢复既有筛选、选中诊断与滚动。
- Worker 迁移/健康/Finding `18/18`、Playnite R17 `15/15`、隔离 Release solution `0 errors/2 existing warnings`、source/XAML/diff、WPF `0/27/162` 通过。证据：`R17-07-FINDING-NAVIGATION-20260920.md`。
- 未验真实宿主呈现、主题/DPI/UIA/IME/焦点滚动、Explorer/权限、ETW 和宿主性能；Demo 原目录不可用，继续使用恢复生产基线；main 用户改动未碰、未合并。
- 下一可执行任务：`R17-08 维护报告可读性`，先查现有报告导出、分组和脱敏器，再补时间/计数一致性与 URL/Windows 路径负例。

## 第三轮 R17-06 存储分析导航（2026-09-20）

- `51cae6b9` 先复用既有 `StorageAnalysisService` 的 SQLite 逻辑索引、备份目录实测、TopGames 和 `TaskSourceNavigationResolver`；没有创建第二套存储统计或导航模型。
- `StorageAnalysisDto` 现在区分失联索引路径数量/逻辑体积与目录实测；备份目录不可用写明路径未知，失联路径写明未计入磁盘实测且不代表占用为 0。排行记录最新 `BackupId`，游戏/版本入口只按稳定 ID 精确解析，缺失目标不回退。
- 维护页 Demo 卡片显示“索引体积 / 磁盘实测 / 磁盘剩余”，排行保持现有有限列表；返回维护中心、游戏选框、滚动、命令绑定、取消/错误、恢复保护和 net462 兼容未改。
- Worker `4/4`、Playnite R17-06 `4/4`、Playnite R17 `15/15`、隔离 Release solution `0 errors/2 existing warnings`、source/XAML/diff、WPF `0/27/162` 通过。未验真实 Playnite/package-host、最终 presented frame、DPI/UIA/IME、Explorer/权限、真实磁盘时序、ETW 和宿主性能；Demo 原目录不可用，继续使用恢复生产基线。
- 下一可执行任务：`R17-07 检查项一键定位`，先查现有 Finding/Health/Task 的稳定来源与导航返回状态，再做最小范围定位。

## 第三轮 R17-05 隔离账本入口（2026-09-20）

- `3002a8dc` 复用既有隔离账本 DTO/分页和定向恢复 IPC，在维护行动项中显示原路径、隔离路径与状态；路径只在隔离账本项展开，保留有限列表、滚动、命令绑定和 net462 兼容。
- 操作文案改为“受控恢复”，仍要求逐条确认并发送 `EntryId + Confirmed`；未添加默认删除、批量恢复或路径猜测。Worker 既有隔离 SQLite 夹具覆盖指定项恢复、索引已移除清理、移动中断恢复、分页边界和残留保护。
- Worker `5/5`、Playnite R17 `12/12`、隔离 Release solution `0 errors/2 existing warnings`、source/XAML/diff、WPF `0/27/162` 通过。证据：`R17-05-QUARANTINE-LEDGER-20260920.md`。
- 真实宿主、最终 presented frame、DPI/UIA/IME/焦点滚动、Explorer/权限、重启恢复、ETW 和宿主性能仍待验；Demo 原目录不可用，继续恢复生产基线。
- 下一可执行任务：`R17-06 存储分析导航`，先核对存储统计、来源记录和已有跳转入口。

## 第三轮 R17-04 保留预览对比（2026-09-20）

- 审计确认既有 `RetentionSimulationService`/维护页已经有候选明细、用户锁定/PreRestore/健康保护、预计释放、隔离占用和清理后二次刷新；Apply 会校验预览句柄/十分钟时效并重算 live 候选、策略和归档指纹。
- `3c73b498` 仅补隔离 SQLite 索引删除失败负例：文件先进入隔离，索引删除失败后恢复原路径，`MovedBytes` 和 `FreedBytes` 为 0，恢复账本保留；因此不把隔离移动、索引删除或失败清理计为真实释放。
- Worker `12/12`、Playnite R17 `10/10`、维护源码 `3/3`、Release solution `0 errors/2 existing MediaCenter nullable warnings`，source/XAML/diff、WPF `0/28/162` 通过；布局回归 `20 passed/11 skipped`，skip 为既有条件性夹具。真实宿主/权限/锁/故障/重启时序和最终呈现仍待验。证据：`R17-04-RETENTION-PREVIEW-20260920.md`。
- 下一可执行任务：`R17-05 隔离账本入口`，先查现有隔离分页 DTO、状态文案、原/隔离路径和受控恢复命令，再决定是否需要代码。

## 第三轮 R17-03 检查进度预算（2026-09-20）

- `87473bc3` 先核对既有 `HealthInspectionService` 的游标、单次预算、会话/操作锁和延后持久化，再复用 `LastSummary` 写入本轮索引范围：总版本数、需检查数、延后数、候选数及未读取归档边界；没有新增数据库迁移。
- 健康巡检 DTO 增加最近完成、当前/最近候选、进度边界和下轮计划显示；维护页健康卡和行动项显示最近成功/完成、间隔与单次预算。游戏运行、锁占用、全候选延后分别给出暂停原因；取消、时间预算和异常结束不伪装为整库已检查。
- 最终 Worker 健康巡检 `12/12`、Playnite R17 `10/10`，Release solution `0 errors/2 existing MediaCenter nullable warnings`，source/XAML/diff、WPF `0/28/162` 通过。未验真实宿主、最终呈现、DPI/UIA/IME、ETW、宿主性能和真实进程/锁/超时竞态；只用合成/fake/隔离数据。证据：`R17-03-INSPECTION-PROGRESS-BUDGET-20260920.md`。
- 下一可执行任务：`R17-04 保留预览对比`，先查 `RetentionSimulationService` 的候选、保护项、隔离账本和预览过期再决定是否改代码。

## 第三轮 R17-02 诊断包预览（2026-09-20）

- `2b6e9051` 复用既有有限诊断 ZIP、`DiagnosticRedactor`、2 MiB 包上限和日志尾部上限，新增只读 Preview IPC。预览实际列出类别、每类脱敏范围、可选日志及请求上限。
- 明确 `database.json` 只是 schema/大小/完整性探针摘要；真实 SQLite 文件/表内容、存档/备份归档、媒体、Rclone 配置/凭据和自动上传均排除。Playnite 取消确认不触发生成，确认后结果显示完整位置与大小。
- 最终 Playnite R17 `7/7`、Worker `3/3`、solution `0 errors/2 existing warnings`，source/XAML/diff/WPF 门禁通过。未验真实 host confirmation/Explorer/权限/日志并发/呈现/DPI/UIA/IME/ETW/性能；只用合成/fake/隔离数据。
- 证据：`R17-02-DIAGNOSTICS-PACKAGE-PREVIEW-20260920.md`。下一可执行任务：`R17-03 检查进度预算`，先查 HealthInspectionService 的范围、暂停原因、最近成功和下轮计划。

## 第三轮 R17-01 健康结果分层（2026-09-20）

- `eb033251` 先复用既有开放 finding 查询和健康巡检解决语义，再补 `CreatedUtc`/证据时间；`resolved=1` 的健康 finding 经过隔离 SQLite 验证不会继续出现在待处理队列。
- `FindingTriageResolver` 只在维护展示边界合并同游戏、同稳定代码、同问题标题的重复来源；错误/严重归入需立即处理，Warning 归入建议处理，Info 归入信息项。健康巡检不同备份标题不同，不跨备份合并。
- 维护页保留 `FindingsGrid`、选中详情、复制/导航和原滚动/虚拟化路径，仅增加三档摘要和证据时间。Playnite `5/5`、Worker `2/2`，solution `0 errors/2 existing warnings`，source/XAML/diff/WPF 门禁通过。
- 只用合成 DTO、fake/隔离 SQLite；真实 Playnite/package-host、多来源生产标题、最终呈现/DPI/UIA/IME/ETW/性能仍未验。Demo 原目录不可用，main 用户改动未碰、未合并。
- 证据：`R17-01-HEALTH-RESULT-LAYERS-20260920.md`。下一可执行任务：`R17-02 诊断包预览`，先查 DiagnosticsPackage 既有类别、脱敏和生成后结果能力。

## 第三轮 R16-08 保存冲突处理（2026-09-20）

- `ee6b37c9` 复用 `BeginEdit`/`CancelEdit` 的编辑基线和现有 fingerprint，新增 `SettingsConflictResolver` 三方合并。后台仅改动的字段合入当前草稿；同字段不同值进入冲突，不部分覆盖。
- `EndEdit` 先加载最新持久化 settings；冲突触发字段级 `SettingsConflictDetected` 提示并抛出 `SettingsConflictException`，保存不继续，取消基线保留最新外部值；无冲突才进入原保存/视觉/Worker 链路。真实共享对象后台竞态仍需宿主验证。
- 最终 HEAD 定向 `17/17`，完整 Release/net462 solution `0 errors/2 existing MediaCenter nullable warnings`，source/XAML/diff、WPF `0/28/162` 通过。未写真实配置、存档、媒体、云端；Demo 原目录不可用，main 用户改动未碰、未合并。
- 证据：`R16-08-SETTINGS-CONFLICT-20260920.md`。下一可执行任务：`R17-01 健康结果分层`，先核对健康检查结果、已解决项和跨来源去重时间。

## 第三轮 R16-07 配置导入预览（2026-09-20）

- `451195ad` 在既有设置导入/导出和缺失路径报告上增加非变更预览：解析 detached package，展示架构版本、兼容性、实际变化字段、未知字段和安全说明；未知字段继续忽略，设备身份不被导入。
- 设置页导入改为“读取 → 预览 → Yes/No 确认 → 应用 → 报告”，取消或不兼容不会复制；应用前 `Clone` 快照，复制或后续报告异常时恢复，旧 `ImportPortableJson` API 走同一安全路径。导出仍不包含凭据并清空 `DeviceId`。
- 最终 HEAD 定向回归 `15/15`，完整 Release/net462 solution `0 errors/2 existing MediaCenter nullable warnings`，source/XAML/diff、WPF `0/28/162` 通过。未验真实宿主文件对话框/确认框/保存取消和最终呈现，未写真实配置/存档/媒体/云端；Demo 原目录不可用，main 用户改动未碰、未合并。
- 证据：`R16-07-SETTINGS-IMPORT-PREVIEW-20260920.md`。下一可执行任务：`R16-08 保存冲突处理`，先核对 Playnite 编辑基线与后台设置更新的冲突/拒绝策略。

## 第三轮 R16-06 生效条件说明（2026-09-20）

- `83e7c745` 复核 `GameSaveCenterSettings.EndEdit`、插件 `settings.update` 和 Worker `UpdateSettings`/健康计划链路，在四个设置分类标题旁标注即时预览、保存后即时、下一任务/轮询边界和下一次 Playnite 启动。
- 当前没有普通设置必须重启 Playnite 的消费证据；Worker 可执行文件、随 Playnite 启动 Worker 和下次安全模式分别是下一 Worker/Playnite 生命周期判断，不被写成笼统重启。
- R16-06 链路/负例 + R16-05 路径回归 `8/8`，Release solution `0 errors/2 existing MediaCenter nullable warnings`，source/XAML/diff、WPF `0/28/162` 通过；真实宿主时序仍待验。证据：`R16-06-SETTINGS-EFFECT-CONDITIONS-20260920.md`。
- 下一项：`R16-07 配置导入预览`，先复用 `ImportPortableJson`/报告能力，核对版本、未知字段、凭据和失败回退。

## 第三轮 R16-05 路径编辑一致（2026-09-20）

- `955dc52e` 复用设置页现有 Binding、全量 `SettingsPathValidationService`、路径粘贴标准化和 `ClipboardRetry`，新增六个本地工具/目录字段共用的浏览/校验/打开/复制入口；云端目标继续是远端文本，不当作本地目录打开。
- `SettingsPathEditorService` 先用只读属性探测，再对目录做只读枚举，区分有效、缺失、网络/磁盘不可达、类型错误和无权限；打开严格要求当前字段有效，不回退父目录；浏览取消不修改草稿。
- 定向行为/源码/既有路径回归 `6/6`，外部隔离 Release solution `0 errors/2 existing MediaCenter nullable warnings`，source/XAML/diff、WPF `0/28/162` 通过。未使用真实网络共享、用户 ACL、剪贴板或 Explorer。
- 真实 Playnite/package-host、文件夹对话框归属、最终呈现、DPI/UIA/IME、ETW/性能仍待验；Demo 原目录不可用，main 用户改动未碰、未合并。证据：`R16-05-PATH-EDITOR-20260920.md`。
- 下一项：`R16-06 生效条件说明`，先查设置字段消费点和保存/应用/重启边界。

## 第三轮 R16-04 恢复默认粒度（2026-09-20）

- `2b194461` 确认原设置只有整体保存/取消、首次路径补全和导入校验，没有分级恢复默认；新增 `SettingsResetCatalog` 与设置页单字段/单分类/全部默认入口。
- 可重置字段仅覆盖安全标量和界面偏好，四个分类分别登记默认值；全部默认额外清理筛选预设、列宽、最近访问。Worker/Ludusavi/Rclone、存档/媒体/镜像路径、云端目标和设备身份不在目录中。
- 重置不调用 `EndEdit`、不保存、不启动 Worker，只重绑当前 Playnite 编辑对象。行为 `2/2` 验证连接字段保留和取消恢复原草稿，源码接线 `1/1`；外部 Release `0 errors/2 existing warnings`，source/XAML/diff、WPF `0/28/177` 通过。隔离构建使用 `obj` 和 `--no-restore`，未宣称 fresh restore。
- 真实 Playnite/package-host、最终呈现、DPI/UIA/IME、ETW、宿主性能仍待验；Demo 原目录不可用，main 用户改动未碰、未合并。证据：`R16-04-RESET-GRANULARITY-20260920.md`。
- 下一项：`R16-05 路径编辑一致`，先核对路径编辑已有入口和权限/网络/不存在负例。

## 第三轮 R16-03 模板应用范围（2026-09-20）

- 当前分支 `52fbf5de` 复用已有模板/策略 DTO、归一化和单目标应用服务，新增明确目标的批量模板应用 DTO、IPC、有限预览和逐项结果。请求只接受稳定 Playnite ID，最多 100 个；空选择、超限、筛选隐藏项都不会回退为全库。
- UI 展示目标/排除/变更字段数，按稳定 ID 保留跨筛选选择；Worker 每个目标单独取得游戏操作锁、写策略和审计，单项失败继续，失败项可重试，取消不被吞掉。模板保持一次性复制，未引入实时继承。
- 真实行为证据为 Core 预览 `3/3`；Playnite 源契约 `1/1`；Worker 策略持久化 `2/2`。外部隔离 Release solution `0 errors/2 warnings`，警告为既有 MediaCenter nullable；source/XAML/diff、WPF `0/28/177` 通过。fresh restore 无诊断退出，采用隔离副本现有 `obj` 资产和 `--no-restore` 构建，临时目录已清理。
- 不能把源契约测试当作真实交互/视觉通过。Demo 原目录不可用；真实 Playnite/package-host、最终呈现、DPI/UIA/IME、ETW、宿主性能仍待验。main 用户改动和 `src.zip` 未碰、未合并。证据：`R16-03-POLICY-TEMPLATE-BATCH-20260920.md`。
- 下一项：`R16-04 恢复默认粒度`，先核对设置页/设置服务/Worker 的单字段、单分类、全部默认入口，确认敏感连接字段保护与取消保留草稿。

## 第三轮 R16-02 策略差异预览（2026-09-20）

- `b327d5ef` 在现有策略/模板能力上新增 `BackupPolicyDiff`，比较 13 个策略字段并复用 `BackupPolicyTemplateCatalog.ClonePolicy` 的归一化；`BackupPolicyDto` 增加字段通知，使 Save 页面差异卡随草稿输入更新。模板依旧是一次性复制，未引入实时继承模型。
- Save 页面分开展示已保存基线与显式草稿、模板覆盖值；本地取消通过 `BackupPolicyDiff.CopyTo` 恢复基线，源码门禁确认取消方法体没有 `RequestAsync`。未保存游戏策略草稿时禁用应用模板命令，避免刷新保留草稿与模板应用交叉覆盖。
- 外部隔离 Release solution `0 errors/7 warnings`，警告仅为离线 `NU1900`；核心差异/复制/通知 `5/5`、Playnite 源契约 `1/1`，source/XAML `24/24`/diff、WPF `0/28/177` 通过。真实 Playnite/package-host、最终呈现、DPI/UIA/IME、ETW、宿主性能仍待验；Demo 原目录不可用，linked `obj` 仍 `Access denied`，main 用户改动未碰、未合并。
- 证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R16-02-POLICY-DIFF-20260920.md`。下一项：`R16-03 模板应用范围`。

## 第三轮 R16-01 设置搜索定位（2026-09-20）

- `a4e35578` 复用设置页现有五个分类、控件和 Binding，新增 `SearchTerms` 附加属性、轻量搜索框和结果摘要；逻辑树登记匹配宿主，查询时只切换可见性并定位首个分类，清空恢复首次搜索前分类，不写配置对象。
- 受控 STA 行为：R16 搜索 `1/1`，搜索“恢复巡检间隔”后自动化数值字段可见可编辑、Worker 字段隐藏、配置值不变，清空回原分类且无 pending edit；源契约 `1/1`，验证导航/草稿分别独立 `1/1`。完整 Release solution 外部副本 `0 errors/10 warnings`，警告为离线 `NU1900` 与既有 MediaCenter nullable；source/XAML `24/24`/diff、WPF `0/28/177` 通过。
- 联合 WPF 筛选会因既有夹具在同一 AppDomain 创建多个 `Application` 而失败，不能把它写成产品回归；每项独立 testhost 均通过。未验真实 Playnite/package-host、最终呈现、DPI/UIA/IME、ETW/宿主性能；Demo 原目录不可用，linked `obj` 仍 `Access denied`，只用合成/fake/隔离目录，main 用户改动未碰、未合并。
- 证据：`R16-01-SETTINGS-SEARCH-20260920.md`。下一项：`R16-02 策略差异预览`，先检查现有策略/模板 DTO 与继承/覆盖来源。

## 第三轮 R15-08 清理历史范围（2026-09-20）

- 现行清理能力来自 `88bde5de`、`a841e42c`、`77d5f346`；`a07f0518` 只补当前提交下的行为证据与陈旧测试修正，没有重建 Retention Simulation。全局预览提供预览句柄/生成时间、现有/保留/候选数量、预计释放、锁定/健康恢复点/PreRestore 影响和隔离账本占用；候选带日期、路径和“超出保留窗口或桶位”原因，维护页绑定日期/原因/摘要，列表最多 200 条、有限高 240。
- 应用必须明确确认并匹配 Worker 持有的预览句柄；过期、策略/候选/归档身份变化会拒绝。每个游戏使用与备份、恢复、媒体共用的 `GameOperationKind.Retention` 锁，运行中操作计入忙碌跳过；只处理备份根目录内 ZIP，锁定、健康恢复点、PreRestore 不进入候选，持久化隔离账本先于索引/物理处理，重启后可恢复或人工确认。清理链路不删除任务记录。
- 当前证据：Worker 清理与隔离账本 `16/16`，Playnite `net462` 维护页契约 `3/3`；source、XAML `24/24`、diff 门禁通过；生产 XAML 未改，WPF 静态沿用上一批 `0/28/162`。首次相邻运行暴露的 R15-07 旧 `Clipboard.SetText` 断言已更新为当前 `ClipboardRetry.TrySetTextAsync` 并重跑通过。
- 未验真实 Playnite/package-host、RenderHarness presented frame、DPI/跨屏、UIA/IME、ETW、宿主性能；Demo 原目录不可用，linked `obj` 仍 `Access denied`。只用合成/fake/隔离目录，未写真实存档、媒体、云端、诊断或系统剪贴板。证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R15-08-HISTORY-CLEANUP-SCOPE-20260920.md`；main 用户改动未碰、未合并。
- 下一项：`R16-01 设置搜索定位`，先检查现有设置页搜索/分组/滚动/命令能力，再决定实现或补“已满足”证据。

## 第三轮 R15-07 失败结果复制（2026-09-20）

- `37dd4a03` 先复用已有 `CopyTaskErrorCommand`、任务错误字段、恢复报告脱敏文本和剪贴板入口；新增 Contracts 共享 `ClipboardTextSanitizer`、`FailureSummary`、`SafeDetailMessage`、完整复制格式化器和最多 4 次 COM/`InvalidOperationException` 瞬时失败重试。完整 payload 仍含 `ErrorMessage`、`ErrorCode`、`DetailMessage` 和任务 ID，但 formatter 返回前已脱敏。
- Task Center 失败卡现在显示 240 字符以内的脱敏首行摘要与错误码；技术详情默认收起，使用 `GscWpfUiTextBox` 只读、可选择、`MaxHeight=180` 的详情控件。复制失败不会改动当前任务选择，现有选框、滚动条、命令/Binding、取消/错误/恢复保护和 net462 保留。
- 行为验证：R15TaskFailureCopy `6/6`；R06/R12/R15 相邻回归 `14/14`；完整外部 Release solution `7 warnings/0 errors`（均为离线 NuGet `NU1900`）；Playnite 定向构建保留 `MediaCenterView.xaml.cs:664` 2 条既有 warning；source/XAML/diff、WPF `0/28/162` 通过。R06 旧复制夹具按当前任务表补回“阶段”列后通过。
- 未验真实 Playnite/package-host、RenderHarness、最终呈现、物理 DPI/跨屏、UIA/IME、ETW、宿主性能；linked `obj` 仍 `Access denied`，本批用外部源码副本。只用合成 DTO、fake 剪贴板和隔离 STA WPF，Demo 原目录不可用，未写真实存档/媒体/云端/诊断/系统剪贴板；main 用户改动和 `src.zip` 未碰、未合并。证据：`R15-07-TASK-FAILURE-COPY-20260920.md`。
- 下一项：`R15-08 清理历史范围`，先核对已有清理命令、运行中任务保护、恢复账本和日期/状态预览边界。

## 第三轮 R15-06 耗时与吞吐（2026-09-20）

- `6f65638e` 在现有 `TaskProgress`/`TaskStatusDto`/SQLite 任务链上增加可选工作量采样。只有明确总量的整库游戏数、媒体专属候选文件数和已知下载字节接入；未知总量、远端 rclone 和恢复写入不显示推算速率或 ETA。
- 采样使用单调时钟和最多 5 个推进样本；两个推进样本后才出速率，15 秒无推进重置，10 秒无新推进隐藏速率/ETA。普通阶段报告清空采样，SQLite 旧库默认未知；广播 clone、快照比较器和最近/活动/分页查询保留字段。
- Task Center 新增可靠采样卡片，已有耗时/进度/选框/滚动/命令、取消错误语义和 net462 不变。行为证据为 Worker 定向 `20/20`、Playnite R15 `11/11`，含未知总量、等待确认、停顿和采样变化负例。
- 最终外部隔离 Release solution 到达 Playnite `net462`，`0 errors/2` 条既有 warning，Core `106/106`；Worker 全量 `342/1 skipped/1 failed/344` 的唯一失败是既有 `MediaSyncServiceTests.cs:570`，没有将其改写为通过。source/XAML/diff 和 WPF `0/28/162` 通过。
- 仍未验真实 Playnite/package-host、RenderHarness、最终呈现、DPI/UIA/IME、presented frame、ETW、宿主性能；linked `obj` 仍 `Access denied`，使用外部源码副本。只用合成/fake/隔离 SQLite/目录，Demo 原目录不可用，main 用户改动和 `src.zip` 未碰、未合并。证据：`R15-06-TASK-THROUGHPUT-20260920.md`。
- 下一项：`R15-07 失败结果复制`，先查现有任务摘要、错误码、脱敏和剪贴板失败重试语义；R15-06 的真实宿主和全量 Worker 失败边界保留。

## 第三轮 R15-05 任务来源定位（2026-09-20）

- `0d1ff346` 复用现有任务 DTO、TaskCoordinator、Worker 广播、SQLite 任务查询、云队列入口和媒体历史分页；新增 `TaskSourceReferenceDto`/稳定来源类型及 `source_references_json` 迁移列。稳定 ID 与显示诊断分离，事件 clone 和旧库读取均保持兼容。
- 任务详情来源卡片只对版本、媒体批次、云队列显示附加入口；游戏保留原关联游戏按钮。版本、批次和队列均按稳定身份精确查找；删除对象显示无法定位且不跳同名游戏/邻近版本。当前现有分类操作没有独立 TaskCenter 任务，因此不伪造媒体批次任务来源。
- 验证：当前分支外部源码副本 solution Release `0 errors/2 条既有 warning`；Playnite R15 `10/10`，Worker 来源/任务回归 `18/18`；`validate-source.py`、XAML `24/24`、diff check；WPF 静态 `0/28/162`。测试身份绑定 `GSC_BUILD_COMMIT=0d1ff346`。
- 未验真实 Playnite/package-host、来源对象删除/重命名后的呈现、UIA/IME、物理 DPI/跨屏、presented frame、ETW 和宿主性能；只用合成/fake/隔离 SQLite，Demo 原目录不可用，main 用户改动和 `src.zip` 未碰、未合并。证据：`R15-05-TASK-SOURCE-LOCATION-20260920.md`。
- 下一项：`R15-06 耗时与吞吐`；先核对已有可靠采样字段和未知总量边界。

## 第三轮 R15-04 重复通知归并（2026-09-20）

- `a67d371e` 先复用现有通知门禁、会话摘要、通知级别策略和 Task Center 历史；`TaskNotificationDeduper` 按任务 ID、终态和失败证据去重。进度不通知；同一失败证据不刷屏；不同失败证据和摘要后的新失败/取消保留；历史错误不丢。
- 验证为 source/XAML/diff 门禁通过，Playnite Release `net462` 外部源码副本 0 errors/2 条既有 warning，相关定向夹具 `28/28`，WPF 静态 `0/28/177`。不把 linked WPF `Access denied`、真实宿主 Toast、最终呈现或性能写成通过。
- 仅使用合成/fake/隔离数据和本地构建，Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未碰。证据在 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R15-04-TASK-NOTIFICATION-DEDUPE-20260920.md`。
- 下一项：`R15-05 任务来源定位`，核对稳定对象身份、已删除对象诊断和同名对象误跳负例。

## 2026-09-20 R15-03 任务详情时间线（代码已提交，隔离验证完成；真实宿主待验）

- `fe0c05a9` 先复用现有任务变更 DTO、协调器、Worker 广播和 Task Center；增加 Worker 观察到的 `OccurredUtc`，广播 clone 同步阶段与取消字段。`TaskTimelineBuilder` 只整理已知创建/开始/阶段/取消/结束记录，按 UTC 和序号稳定排序，同时显示本地时间与 UTC。
- 缺少事件或时间时显示“时间未知”，没有事件关联依据不猜测重试。Dashboard 运行期事件窗口最多 64 条/任务、200 个任务；详情卡有限高度 220 DIP，继续使用现有滚动、选框、命令/绑定、取消/错误/恢复保护和 net462 路径。
- `validate-source.py`、XAML `24/24`、diff check、Worker Release 隔离定向 `11/11`、Playnite Release `net462` 定向 `11/11` 已通过；Playnite 构建仅有既有 `MediaCenterView.xaml.cs:664` 两条 nullable warning；WPF 静态检查 `0/28/162`。
- 未验真实 Worker 重启后的持久时间线、完整 solution/RenderHarness、真实 Playnite/最终呈现、DPI/UIA/IME、ETW 或宿主性能；只用合成/fake/隔离数据，Demo 原目录不可用，沿用恢复生产基线；main 用户改动与 `src.zip` 未碰、未合并。
- 证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R15-03-TASK-TIMELINE-20260920.md`。下一项：`R15-04 重复通知归并`，先查现有通知、会话摘要和失败历史入口。

## 2026-09-20 R15-02 取消过程展示（代码已提交，隔离验证完成；真实宿主待验）

- `9c8241fb` 复用现有 `TaskCoordinator`、取消 IPC、任务 DTO、SQLite 查询和 Task Center；共享 `TaskCancellationStates` 区分可取消、正在取消、安全收尾、已取消和无法中断的已结束任务。运行时闸门保证连点取消只发一次令牌取消，成功/取消竞争按已接受取消收敛为 `Cancelled`，取消后真实失败为 `NotInterruptible`，终态不保留取消中状态。
- `tasks.cancellation_state` 通过既有迁移入口接入新增/更新/最近/活动/分页读取；`TaskStatusDto` 提供可取消、取消中和人类可读显示；Task Center 增加取消状态卡；快照比较器和任务复制列同步阶段字段。原有 TaskState、命令绑定、取消入口、滚动、恢复/错误语义和 net462 路径保持。
- 当前提交身份的 Worker 定向测试 `14/14`，Playnite Release `net462` 构建 0 错误且 R06 取消回归、R15-01 阶段、R15-02 夹具 `8/8`；源码校验、XAML `24/24`、diff check 通过；WPF 静态审查 `0/28/162`。Playnite 仍有 `MediaCenterView.xaml.cs:664` 的 2 条既有 nullable warning。
- 完整 solution 脚本在 linked worktree 生成 WPF 临时项目时遇到 `Access denied`，不写成全 solution 通过；Worker/Playnite 项目分别实际构建。只用合成/fake/隔离目录，Demo 原目录不可用，沿用恢复生产基线；未写真实存档、媒体、云端或诊断；main 用户改动和 `src.zip` 未碰、未合并。
- 证据为 `R15-02-TASK-CANCELLATION-20260920.md`；下一项为 `R15-03 任务详情时间线`，先核对任务事件缓存、阶段字段和详情滚动容器。真实 Playnite/RenderHarness、最终呈现、DPI/UIA/IME、presented frame、ETW 和宿主性能仍待验。

## 2026-09-20 R15-01 任务阶段可读（代码已提交，隔离验证完成；真实宿主待验）

- `4e7ac33a` 先复用 `TaskCoordinator`、任务 DTO、SQLite 查询和任务中心视图。`TaskStageResolver` 将已有后端事件映射为真实可读阶段，`StageMessage` 保留最后阶段；终态错误/取消仍在 `Message` 和详情中单独显示，未知进度显示 `—`。
- `tasks.stage_message` 使用现有 SQLite 迁移入口，新增/更新/最近任务/分页查询保持一致；任务中心只增加阶段列和阶段详情，保留现有命令绑定、取消、滚动、有限列表和 net462 路径。
- `validate-source.py`、XAML `24/24`、diff check、Worker 定向 `12/12`、Playnite Release `net462` 定向 `2/2` 已通过；Playnite 构建的 2 条 `MediaCenterView.xaml.cs:664` nullable warning 为既有告警；WPF 静态审查 `0/28/177`。
- 只用合成/fake/隔离数据，未写真实存档、媒体、云端或诊断。Demo 原目录不可用，沿用恢复生产基线；真实 Playnite/RenderHarness、各类宿主阶段事件全覆盖、最终呈现、DPI/UIA/IME、ETW、性能仍待验；main 用户改动和 `src.zip` 未碰、未合并。
- 证据为 `R15-01-TASK-STAGES-20260920.md`；下一项为 `R15-02 取消过程展示`，先核对取消请求和成功/取消竞争的终态。

## 2026-09-20 R14-08 来源规则试运行（代码已提交，隔离验证完成；真实宿主待验）

- `89141528` 先复用 `MediaSourceRuleDto`、Worker 已有媒体扩展名和 `MatchesIncludePattern`，新增来源规则草稿的只读预览 DTO、IPC dispatcher、ViewModel 命令和来源设置样本列表。样本带确定的命中/排除原因、大小和路径，试运行不调用保存、入库、移动或归类。
- Worker 使用 linked cancellation token；时间预算限制为 `100–5000ms`，扫描项为 `1–5000`，样本为 `1–200`。UI 默认 120 样本/2000 扫描项/1500ms；数量或时间耗尽时返回 `Partial`，不把部分目录冒充完整结果。
- 来源页保留当前滚动、命令绑定、主题、选框和 net462 路径，新增 `MaxHeight=240`、Recycling 的有限列表。同步修复重复组卡片 Border 双子级 XAML 编译问题，原有虚拟化和滚动属性未改。
- `validate-source.py`、XAML `24/24`、diff check、Worker Release 隔离构建 `0/0`、Worker 行为 `1/1`、Playnite Release `net462` 契约 `1/1` 已通过；Playnite 构建有 `MediaCenterView.xaml.cs:664` 的既有 nullable warning 2 条；WPF 静态审查 `0/28/177`。render-qa 的 linked `obj` Access denied 仍未形成呈现证据。
- 只用合成/fake/隔离目录和 SQLite；未写真实存档、媒体、云端或诊断。Demo 原目录不可用，沿用恢复生产基线；main 用户改动与 `src.zip` 未碰、未合并。证据为 `R14-08-SOURCE-RULE-PREVIEW-20260920.md`；下一项为 `R15-01 任务阶段可读`，真实宿主/权限拒绝/大目录/呈现边界待验。

## 2026-09-20 R14-07 媒体详情浏览（代码完成，环境待验）

- 先复用现有媒体分页、稳定 `MediaId` 选择和滚动锚点；上一项/下一项只在当前已加载窗口内移动，选中项变化后显式 `ScrollIntoView`/`BringIntoView`，不虚构跨页数据。
- 详情沿用 `MediaItemDto` 的类型/来源/大小/采集时间；`AsyncThumbnailImage` 通过 `BitmapSource.PixelWidth/PixelHeight` 暴露截图尺寸。视频本地路径缺失、不支持格式或 `MediaFailed` 统一进入只读回退，异步截图仍由 generation、取消令牌和卸载取消保护。
- `validate-source.py`、XAML `24/24`、diff check 通过；新增尺寸行为断言和 R14-07 源码契约夹具。Playnite Tests Release build 退出 1，仅输出 0 警告/0 错误、无诊断，不写成构建或运行时通过。
- 只使用合成/fake/隔离路径；未写真实存档、媒体、云端或诊断。Demo 原目录不可用，沿用恢复生产基线；main 用户改动、src.zip 未碰、未合并。
- 证据为 `R14-07-MEDIA-DETAIL-20260920.md`；下一步补跑 R14-04/R14-05/R14-06/R14-07 定向验证，再核对 R14-08 的既有实现与依赖。

## 2026-09-20 R14-06 批量目标防误选（代码完成，环境待验）

- 先查到现有目标均来自 Games，当前全局选框已按 PlayniteId 保留被筛选隐藏的选择，媒体目标也已有 SelectedItem/TargetPlayniteId 绑定；本阶段没有重建选框、过滤或命令链。
- GameDescriptorDto/GameStatusDto 增加只读 IconPath 和 IdentityDisplay。Playnite 适配器通过现有 Database.GetFullFilePath(game.Icon) 解析本地图标，文件不存在或异常则为空；Worker Dashboard 和策略克隆/快照比较同步该字段。
- 全局选框图标缺失回退首字母；媒体批量归类、预览目标覆盖和重新归类目标共用名称/平台/Playnite ID 模板，仍按 SelectedItem 或稳定 TargetPlayniteId 选择，不以首项/索引替代目标。新增夹具覆盖图标/唯一身份展示及目标绑定静态契约。
- cfbb1279 已推送；源码校验、XAML 24/24、diff check 通过。定向 Playnite testhost 无输出，构建/运行时未签收；不把静态契约写成真实呈现或性能证据。
- 只使用合成 DTO、fake/隔离源码；不下载图标，不碰真实存档、媒体、云端或诊断。Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 src.zip 未碰、未合并。
- 证据为 R14-06-TARGET-GUARD-20260920.md；下一步先补跑 R14-04/R14-05/R14-06，再推进 R14-07 媒体详情浏览。

## 2026-09-20 R14-05 重复媒体识别视图（代码完成，环境待验）

- 先核对现有重复能力：扫描入库已按 `MediaHashExistsAsync` 和 `media.sha256` 唯一约束阻止相同哈希再次入库，但没有回看视图。本阶段复用现有媒体 DTO/查询，新增当前游戏范围的有界只读重复检查。
- Worker 以相同非空 SHA-256 生成确定组，以同类型、文件名和大小一致生成疑似组，并排除确定组；扫描 5000、组 100、组内展示 24 的上限保持可控。Media 新 Tab 只支持组选择和重新识别，不新增删除、移动或归类命令。
- `136285d5` 已推送；Worker 疑似组隔离夹具和 Playnite 只读契约已加入。源码校验、XAML `24/24`、diff check 通过；linked worktree 的 `obj` Access denied/SDK-Workload 阻塞 Worker/Contracts 构建及运行时测试，不写成通过。
- 只使用合成/fake/隔离目录，Demo 原目录不可用，沿用恢复生产基线；main 用户改动、`src.zip` 和真实存档/媒体/云端/诊断未碰。证据文件为 `R14-05-DUPLICATE-INSPECTION-20260920.md`。
- 下一步在可用 SDK/Workload 环境补跑 R14-04/R14-05 定向验证，随后推进 `R14-06 批量目标防误选`。

## 2026-09-20 R14-04 撤销边界说明（实现已满足，运行时环境待验）

- 先查到媒体归类撤销能力已由 `1c0c5a37`/`a7c39922` 提供，因此没有重建服务或改变文件移动语义；历史页已有最近批次与所选可回退批次入口，`IsUndoable` 只放行仍有已应用项的可撤销批次。
- Worker 撤销逐项重新读取当前媒体并匹配应用后快照，持久层再次以目标、Assigned、应用后归档路径和 Applied 批次项做条件更新；后来人工修改会变成冲突，保留当前媒体、备注/收藏和归档路径。
- `03521991` 新增 `ClassificationUndoLeavesLaterManualDecisionAndArchiveUntouched` 隔离负例，与既有正常撤销夹具形成正/负行为证据。源码校验、XAML `24/24`、diff check 通过；当前定向 Worker testhost 长时间无输出，未写成运行时通过。
- 只使用合成/fake/隔离目录，Demo 原目录不可用，沿用恢复生产基线；没有碰 main 用户改动、`src.zip` 或真实存档/媒体/云端/诊断。证据文件为 `R14-04-UNDO-BOUNDARY-20260920.md`。
- 下一步推进 `R14-05 重复媒体识别视图`，先核对已有 hash/元数据能力与只读边界；R14-04 Worker/Playnite 运行时复跑待可用 SDK/Workload。

## 2026-09-20 R14-03 部分成功处理（代码已提交，环境待验）

- `aef251b1` 已推送。先复用 Worker 批量归类/忽略/恢复的逐项结果 DTO，不重建操作；ViewModel 记录稳定失败 MediaId、逐项原因、原操作和归类目标。
- 失败列表是有限高度/Recycling，重试命令只提交失败 ID，成功项不会重复执行；失败重新替换为本次仍失败项。未改变归档副本保留、取消或错误语义。
- Worker 行为夹具和 Playnite 源契约夹具已加入；源码校验、XAML `24/24`、diff check 通过，Worker/Playnite 运行时未执行，不把当前环境写成 build/test 通过。
- 本批只用合成/fake/隔离目录，未写真实存档、媒体、云端或诊断；Demo 原目录不可用，main 未碰、未合并，没有新临时产物。
- 下一步补跑 R13-07/R13-08/R14-01/R14-02/R14-03 定向夹具与回归，再做 `R14-04 撤销边界说明`。

## 2026-09-20 R14-02 预览选择编辑（代码已提交，环境待验）

- `69cf2a72` 已推送。先复用现有媒体归类预览批次、稳定 `MediaId`、Worker 重验和撤销链；`IsIncluded` 控制排除，`TargetPlayniteId` 只允许高置信建议在当前游戏目录中做目标覆盖。
- 应用 DTO 只传当前纳入项的稳定媒体 ID 和覆盖目标；Worker 按当前目录、批次和 `Pending` 状态校验，合法覆盖写回目标/原因再应用，非法目标跳过并保持未归类。没有放宽低/中置信门禁。
- `validate-source.py`、XAML `24/24`、diff check、Contracts/Core Release 隔离 `0/0` 通过；当前 Core testhost 未产出可签收结果，Worker/Playnite/RenderHarness 未执行，不写成通过。
- 本批只用合成/fake/隔离目录；媒体预览有限高度/Recycling/滚动、命令绑定、取消/错误/恢复保护、游戏选框和 net462 保持。`.tmp/r14-02-build` 已清理，main 未碰、未合并，Demo 原目录不可用。
- 下一步在可用 SDK/Workload 环境补跑 R13-07/R13-08/R14-01/R14-02 定向夹具与回归，再做 `R14-03 部分成功处理`。

## 2026-09-19 R14-01 归类建议解释（代码已提交，环境待验）

- `7735cd7c` 已推送。先核对确认现有归类预览已读取来源规则、游戏会话、进程映射和文件名证据，再将现有算法结果结构化到共享 `MediaClassificationEvidenceDto`；没有新增服务、IPC、移动、删除或存储表。
- 来源规则显示实际目录/模式，会话显示实际时间窗/进程，进程映射显示可执行文件/目标；多候选不指定目标并展示各候选依据；无依据显示“待判断”，保留既有低置信不可应用门禁。
- `validate-source.py`、XAML `24/24`、diff check 和 Contracts/Core Release `0/0` 已通过。Core 定向测试受项目引用目标框架评估退出 `1` 未进入 testhost；Worker restore、Playnite `net462`、RenderHarness 未执行，不写成通过。
- 本批只用合成/fake/隔离数据，Media Inspector/有限列表/滚动/命令绑定/确认应用撤销保持；R14 隔离构建已清理。`.tmp/r13-verify-source` 曾短暂被占用，阶段末已精确删除，未强杀未知进程；main 用户改动未碰、未合并，Demo 原目录不可用。
- 下一步先在可用 SDK/Workload 环境补跑 R13-07/R13-08/R14-01 定向夹具与相关回归，之后做 `R14-02 预览选择编辑`。

## 2026-09-19 R13-08 失败分类帮助（代码已提交，环境待验）

- `96a4c6a9` 已推送。复用 Rclone 稳定错误码，新增无空间与限流分类；`CloudFailureExplanation` 只为认证、空间、远端不存在、校验差异、限流提供下一步，未知错误保持空解释，原始错误码/详情折叠保留。
- 限流纳入既有有限退避，未改变上传、取消、恢复保护、本地副本保留或通知语义。Core/Worker/Playnite 夹具已加入但未执行；源码校验、XAML `24/24`、diff check 通过。
- 当前只有 SDK `9.0.302`，`global.json` 的 `8.0.100` 向上滚动命中缺失 Workload resolver 目录，Worker restore 退出 `1`；不把 build/test、Release/net462 或真实 rclone 写成通过。Demo 原目录不可用，继续用恢复生产基线。
- 下一步在可用 SDK/Workload 环境同时补跑 R13-07/R13-08 定向测试；`r13-07-source` 已在阶段末精确删除，验证后推进 R14-01。

## 2026-09-19 R13-07 队列筛选与汇总（代码已提交，环境待验）

- `d6c2af90` 已推送。先复用云端队列现有状态/类型筛选、查询一致性 token、分页追加、去重和选中项恢复；新增游戏/Playnite ID、来源设备、时间窗口筛选与全局总数，不另建队列或改变上传/校验/取消/错误/恢复语义。
- Worker 用同一候选集合计算筛选 `TotalCount` 与未筛选 `GlobalTotalCount`；维护页和合成 RenderHarness ViewModel 的摘要、筛选绑定已同步，筛选栏改用可收缩列。新增 Worker SQLite 和 Playnite 源行为夹具覆盖正例与错误设备负例，但测试未执行。
- 源码校验、XAML `24/24`、diff check 已通过。主机只有 .NET SDK `9.0.302`，`global.json` 要求 `8.0.100`，缺少 Workload resolver 目录导致 Worker restore 退出 `1`；不把 Worker/Playnite build/test、Release/net462 或 RenderHarness 写成通过。Demo 原目录不可用，仍沿用恢复生产基线。
- 隔离 `r13-07-build` 已清理；`r13-07-source` 首次清理时短暂被 Contracts 子目录占用，阶段末已精确删除，未强杀未知进程。下一步在可用 SDK/Workload 环境补跑新增定向测试与相关回归，验证后再做 R14-01。

## 2026-09-19 R13-06 离线恢复反馈

- `e4244329` 已推送。先复用 `CloudRetryPolicy` 与 `CloudRetryService` 的退避、轮询、限流和顺序处理；新增共享 DTO 的网络恢复 display-only 文案，不另建队列或改变传输语义。
- `RetryScheduled` 的网络/不完整传输错误显示等待网络恢复、`N/6` 自动重试和本轮最多 10 项；`Transferring` 显示网络已恢复、按批次上传中；认证失败保持空文案。既有策略为 1/5/15/60/240/720 分钟退避、最多 6 次，Worker 30 秒轮询、每轮最多 10 项、无逐条旧失败通知。
- Core `1/1`、Worker `10/10`、Playnite R13 `11/11`、Release `0/0`、XAML `24/24`、source/diff check 通过。证据见 [R13-06 离线恢复反馈](../design/reviews/ui-finesse-round3-20260915/evidence/R13-06-OFFLINE-RECOVERY-20260919.md)。WPF 只做 Demo-first 共享详情容器/绑定质量检查，未运行真实宿主或 RenderHarness。
- 边界：合成/fake/隔离数据，未写真实网络、云端、存档、媒体或诊断；Demo 原目录不可用，沿用恢复生产基线；main 未合并且用户改动未触碰；本批临时目录已清理，旧 `.tmp/r12-07-build-final` 仍因 Access denied 暂留。
- 下一项 `R13-07 队列筛选与汇总`：先查现有状态筛选、全局计数、分页和选中项联动。

## 2026-09-19 R13-05 远端证据详情

- `e280cf1c` 已推送。复用现有 `CloudTransferStatusDto`、`CloudTransferStateService`、队列/远端布局与维护页详情，不另建上传服务；新增 `CloudRemoteDisplay` 和四项 display-only 证据字段。
- 远端对象显示按现有备份/媒体相对路径生成，并对 URI 用户信息、query/key-value secret、Bearer 值统一脱敏；复制诊断仍走既有 `ClipboardValueSanitizer`。未知字段保持未知，来源设备为空为“未知设备”。
- 现有队列不持久化历史成功校验时间，只有当前 `RemoteVerified` 的 `UpdatedUtc` 可作为可证实成功时间；转为 `Uploaded` 后回到未知，不用上传时间伪造历史校验。
- Core `2/2`、Worker `1/1`、Playnite R13 `10/10`、Release `0/0`、XAML `24/24`、source/diff check 通过。证据见 [R13-05 远端证据详情](../design/reviews/ui-finesse-round3-20260915/evidence/R13-05-REMOTE-EVIDENCE-20260919.md)。WPF 只做 Demo-first 共享样式/详情容器质量检查，未运行 RenderHarness 或真实宿主。
- 边界：合成/fake/隔离数据，未写真实云端、存档、媒体、诊断；Demo 原目录不可用，沿用恢复生产基线；main 未合并且用户改动未触碰；旧 `.tmp/r12-07-build-final` 仍因 Access denied 暂留。
- 下一项 `R13-06 离线恢复反馈`：先查现有离线状态与恢复入口及 Q 依赖。

## 2026-09-19 R13-04 暂停与允许时段

- `cca3f052` 已推送。复用 `WorkerOptions` 的暂停/允许时段字段、`CloudRetryService` 与设置页入口；队列摘要新增“队列空闲”分支，优先级为暂停、时段外、空闲、运行中，没有另建队列或替换调度器。
- 现有设置语义已明确：暂停只停止后台自动重试，恢复后继续处理已保存队列；允许时段变化在下一轮 Worker 检查时生效，已开始上传不取消。暂停 sweep 和时段外持久化 defer 为源代码复核事实，不冒充真实云端运行时验证。
- Core `1/1`、Worker `3/3`、Playnite R13 `9/9`、Portable `10/10`；隔离 Debug `0/0`、XAML `24/24`、source/diff check 通过。证据见 [R13-04 暂停与允许时段](../design/reviews/ui-finesse-round3-20260915/evidence/R13-04-PAUSE-WINDOW-20260919.md)。
- 边界保持：只用合成/fake/隔离数据和外部构建，不代表真实 Playnite/package-host、真实远端/进行中上传、最终呈现或宿主性能；Demo 原目录不可用，沿用恢复生产基线；main 用户改动未触碰、未合并。`.tmp/r12-07-build-final` 仍因 Access denied 暂留。
- 下一项 `R13-05 远端证据详情`：先核对远端验证状态、脱敏和已上传/已验证区分。

## 2026-09-19 R13-03 手动重试范围

- `680ea83a` 已推送。复用现有维护页单项、任务中心单项/批量和 IPC replay ledger；新增 DTO 派生的手动范围说明与可重试状态，失败/排队才允许当前选中项重试，传输中/已上传/已校验不重复提交。云端 retry 只复制已有本地备份/媒体归档，不重建本地版本。
- `RelayCommand` 忙态负例实际保持第二次提交数为 `1`；任务中心批量入口既有按当前筛选、稳定任务 ID、任务类型/游戏去重语义保持。`RetryCloudUpload` 与 `RetryMediaCloudUpload` 的 replay protection 仍由 `IpcRequestSemantics`、`WorkerIpcClient` 和 SQLite ledger 提供。
- 证据：Playnite `8/8`、Core `29/29`、Worker 云状态/部分成功 `19/19`、Worker `IpcRequestLedgerTests 6/6`，隔离 Debug `0/0`、XAML `24/24`、source/diff check 通过；named-pipe 客户端行为 `1 passed / 6 skipped / 0 failed`，未将跳过写成通过。证据见 [R13-03 手动重试范围](../design/reviews/ui-finesse-round3-20260915/evidence/R13-03-MANUAL-RETRY-SCOPE-20260919.md)。
- 边界保持：只用合成/fake/隔离数据和外部构建，不代表真实 Playnite/package-host、真实远端、物理 DPI/跨屏、UIA/IME、presented frame、ETW 或宿主性能；Demo 原目录不可用，沿用恢复生产基线；main 用户改动未触碰、未合并。
- 下一项 `R13-04 暂停与允许时段`：先核对 `CloudUploadQueuePaused`、允许时段策略、持久化状态和进行中上传不被意外取消。

## 2026-09-19 R13-02 下次重试时间

- `6fd22892` 复用 `NextAttemptUtc/NextAttemptLocal`，用 `RetryTimingDisplay` 同时表达本地绝对时间和有界相对提示；过期时间钳制为“可立即重试”，不产生负倒计时；空值保持“无自动重试”。详情绑定是被动派生值，没有新增常驻计时器。
- Core `29/29`、Playnite `2/2`、Worker `11/11`，隔离 Debug `0/0`、XAML `24/24`、source/diff check 通过。证据见 [R13-02 下次重试时间](../design/reviews/ui-finesse-round3-20260915/evidence/R13-02-RETRY-TIMING-20260919.md)。
- 仍只覆盖合成/fake/隔离测试，不代表真实 Playnite/package-host、真实远端、最终呈现或宿主性能；Demo 原目录不可用，沿用恢复生产基线。main 尚未合并且用户改动未触碰。
- 下一项 `R13-03 手动重试范围`：先查单项/媒体重试 requestId、幂等和部分成功后的处理范围。

## 2026-09-19 R13-01 队列阶段展示

- `803470b8` 复用 `CloudTransferStatusDto.State`、`CloudTransferSummaryDto.QueueControlDisplay` 和维护页现有队列；新增 `QueuePhaseDisplay`，网络/不完整传输退避与普通重试分开显示，上传中、验证中、等待验证和远端已验证分别可读。没有改变 Worker 状态机、重试调度、远端校验或命令语义。
- 认证失败重试负例保持“等待重试”，不归类成网络等待；详情仍同时显示 `GuaranteeLevelDisplay`，所以已上传不会变成已校验。Core `27/27`、Playnite `7/7`、Worker `11/11`，隔离 Debug `0/0`、XAML `24/24`、source/diff check 通过。
- 证据见 [R13-01 队列阶段展示](../design/reviews/ui-finesse-round3-20260915/evidence/R13-01-CLOUD-QUEUE-STAGES-20260919.md)，代码已推送。只使用合成/fake/隔离测试，没有写真实远端、存档、媒体或诊断；Demo 原目录不可用，沿用恢复生产基线。main 尚未合并且用户改动未触碰。
- 下一项 `R13-02 下次重试时间`：先检查 `NextAttemptUtc/NextAttemptLocal` 的既有投影、系统时钟变化和页面关闭后的计时器生命周期。

## 2026-09-19 R12-08 恢复结果报告

- `fd4756ea` 复用既有恢复编排、任务状态和 PreRestore 能力，新增 `RestoreReportDto`，用稳定 PlayniteId/BackupId/TaskId 记录执行目标、预览文件范围、保护快照、失败阶段和完成/回滚/人工介入/取消/失败结果；没有新建 IPC 或另一套恢复流程。
- Worker 任务状态持久化报告到 SQLite，最近/活动/分页查询都能回读；TaskCenter 结果卡沿用现有详情 ScrollViewer；复制命令输出不含路径、诊断详情或凭据。测试覆盖成功、回滚、人工介入/失败投影和持久化回读，先捕获并修复了分页查询使用错误 JSON 选项导致 `PreRestoreBackupId` 丢失的问题。
- 证据：Playnite R12 `15/15`，Worker 定向 `34/34`，隔离 Debug `0/0`，XAML `24/24`，source/diff check 通过；已推送 `fd4756ea`。证据文件为 [R12-08 恢复结果报告](../design/reviews/ui-finesse-round3-20260915/evidence/R12-08-RESTORE-RESULT-REPORT-20260919.md)。
- 仍只覆盖合成 DTO/fake/隔离 SQLite/目录和外部构建副本，不代表真实 Playnite/package-host、最终呈现、物理 DPI/跨屏、UIA/IME、ETW 或宿主性能；Demo 原目录不可用，沿用恢复生产基线。main 尚未合并且用户改动未触碰；关闭 MSBuild/VB/C# 编译器服务器后，`.tmp/r12-07-build-final` 精确删除仍逐项 Access denied，未强杀未知进程。
- 下一项 `R13-01 队列阶段展示`：先复用 `CloudTransferCoordinator` 和维护页现有阶段/GuaranteeDisplay，再补状态一致性和上传成功不冒充远端校验的负例。

## 2026-09-19 R12-07 预览失效重验

- `00724e62` 复用 `DashboardViewModel` 的确认流程和 Worker 既有映射/readiness/PreRestore 能力，只增加确认返回后的游戏/版本身份守卫；变化时重置流程并拒绝过期确认。
- 既有 Worker 在写入前重解析目标并执行预览、写入后做结果校验；本阶段用真实 fake 调用记录固定 `preview → write → post-validation`，并用同大小不同内容的归档验证 Manifest SHA-256 能识别失效。
- 证据与验证：Playnite R12 `13/13`、Worker `27/27`、Debug `0/0`、XAML `24/24`、source/diff check 通过；已推送 `00724e62`。无 XAML/视觉资源改动，未改变命令、取消/错误、选框、滚动、恢复保护或 net462。
- 仍只覆盖合成/fake/隔离目录；未验真实 Playnite/package-host、全量 WPF、物理 DPI/跨屏、UIA/IME、presented frame、ETW 和宿主性能。Demo 原目录不可用，沿用恢复生产基线；main 尚未合并且用户改动未触碰。下一项 R12-08 恢复结果报告。

## 2026-09-19 R12-06 恢复冲突说明

- 现有恢复链已经提供 `RestoreReadinessDto`、`TaskStatusDto`、稳定错误码和四阶段 `RestoreWorkflowProgress`；本阶段提交 `423856b2` 复用这些能力，用 `RestoreFailureExplanation` 将游戏运行、操作锁、磁盘空间、目标权限与未知原因映射为处理步骤，不新建服务或 DTO。
- 失败阶段仍保留错误码/任务详情和原状态；目标失败不会进入执行，readiness 失败不会写入当前存档。权限只在详情出现权限证据时分类，普通 `RESTORE_WRITE_FAILED` 保持未知，避免错误指导。所有建议都明确不关闭安全机制。
- 通过隔离 Debug 构建 `0/0`、XAML `24/24`、Playnite R12 `11/11`、Worker 恢复编排 `12/12`、源码校验和 diff check。当前未运行全量 WPF，也未宣称真实 Playnite/最终屏幕呈现或宿主性能。
- 下一项 `R12-07 预览失效重验`：优先核对 readiness/preview 缓存失效、目标路径或版本变化后的重验入口，再补过期结果和取消/失败负例；不要把重新读取写成真实恢复演练。

## 2026-09-19 R12-05 远端下载进度

- R12-05 已由 `23e5b9d4` 推送：`RemoteBackupStagingService` 复用 `TaskCoordinator` 和任务事件流，把下载到隔离区、哈希校验、Ludusavi 版本确认、清单写入和等待恢复确认投影为真实任务阶段；不把下载完成显示为恢复完成。
- `TaskProgress` 保留取消终态解释；远端 staging 在取消/失败时清理本次隔离目录，清理失败提升为可见错误，避免残留被当作已清理。Playnite 只订阅 `RemoteStage` 投影，取消按钮只在活动态可见，原远端两步命令和恢复保护不变。
- 证据使用合成 DTO、fake/隔离目录和 net462/Worker 定向测试：Playnite `3/3`、Worker `22/22`、最终 Debug 隔离构建 `0/0`、XAML `24/24`。全量 WPF 测试在既有 R07 resize/focus 失败处按门禁停止，不记录为绿色。
- Demo 原目录不可用，继续以恢复生产基线为视觉依据；真实 Playnite/package-host、rclone/Ludusavi、物理 DPI/跨屏、UIA/IME、presented frame、ETW、宿主性能仍未验。main dirty 用户文件和 `src.zip` 未改，下一任务为 R12-06。

## 2026-09-19 R12-04 恢复保护备份

- `e4e42f40` 复用 RestoreOrchestrator 的 PreRestore、TaskCoordinator 的同游戏串行门和 GameOperationLock；保护备份失败、无法识别、无法锁定或本地索引保存失败统一返回 `RESTORE_PRERESTORE_FAILED`，不会调用目标版本写入。
- 失败投影明确“保护备份阶段失败，危险恢复已中止”；成功任务消息明确执行前保护快照已创建并锁定。fake 测试先让保护备份失败，再重试并将当前状态改为 `A-latest`，确认首次没有危险恢复调用，重试用最新状态建立保护快照并完成目标恢复。
- 证据：Worker `RestoreOrchestratorTests 12/12`、Playnite `R12RestoreWorkflowBehaviorTests 7/7`、资源字典 `137/39/0`；完整隔离 Release `0/0`、Core `85/85`、Worker `326/326`，source `68` 类/WPF `84` 类全部返回 0，XAML `24/24`。
- 边界仍是合成/fake、隔离目录、隔离 SQLite、STA WPF/offscreen logical DIP；未验真实 Playnite/package-host、物理 DPI/跨屏、presented frame、UIA/IME、ETW 或宿主性能。Demo 原目录不可用，沿用恢复生产基线。main 最新安装前源测试失败 `273/18/1` 未被本分支证据覆盖。
- 当前分支已推送，main 尚未合并。下一项 R12-05 远端下载进度。

## 2026-09-19 R12-01 恢复流程分步摘要

- 当前分支 `e1a8da0c` 复用已有恢复 readiness/task/command 能力，新增固定四阶段状态投影和 Save 页面摘要卡；没有重建服务、DTO、Worker IPC、游戏选框或滚动条系统。
- 行为证据：R12 `6/6`，相邻 R11/R06 `17/17`，WPF 资源字典 `137/39/0`；隔离 Release solution `0/0`、XAML `24/24`、source/XAML/diff check 通过。失败夹具检查目标阶段未进入执行、状态/回滚详情保留，不是 Assert.Contains-only。
- `render-qa` 绑定 `e1a8da0c`、WorkingTreeClean=True，但真实退出 `1`，保留既有 Overview/Task/Save/Settings/Shell/Media 离屏基线问题；不宣称 Playnite/package-host、物理呈现、ETW 或宿主性能通过。Demo 原目录不可用，沿用恢复生产基线。
- 证据只使用合成 DTO/fake、隔离 STA WPF 和 `.tmp` 源副本；没有真实存档/媒体/云端/诊断写入。main 的 DEV-INSTALL-008 失败事实仍是 `73/588/57`、安装器退出 `1`，main 用户改动未触碰。下一可执行任务为 R12-02 校验结果解释。

## 2026-09-18 R08-06 数字变化动效

- 复用 Overview 现有六个摘要计数与 `GscMotion`，新增 `NumericChangeFeedback` 附加行为；只在整数值真正变化时对局部 `ScaleTransform` 做 `1.04` pulse，`420ms` 节流，高频进度与技术文本不动。六个计数固定 `96 DIP` 槽位，`99→100` 不推挤邻项；减动效即时更新。
- `acbfe7e0` 为生产实现，`e264acff` 为真实 WPF 动画取样修正。最终隔离 Release `0/0`、Playnite `net462`、XAML `24/24`；R08Numeric `3/3`、R08PageSwitch `2/2`、R08BusinessFeedback `4/4`；源码/XAML/diff check 通过，WPF `0/21/177`。
- RenderHarness 绑定 `e264acff`、`WorkingTreeClean=True`、357 张 PNG；Overview 1040×700 和 1600×900 已抽查。全量报告退出 `1`，只命中既有 Save/Task resize `3/4` 可读问题，不能写成全量通过。证据见 `R08-06-NUMERIC-CHANGE-20260918.md`。
- 边界仍为合成数据、fake/生产资源、隔离 STA/offscreen logical DIP；未验真实 Playnite/Worker、presented frame、物理 DPI/跨屏、UIA/读屏、ETW/宿主性能。Demo 原始目录不可用，沿用恢复生产基线。下一项 R08-07。

## 2026-09-18 R08-05 页面切换轻量化

- 复用既有 `AcrylicProductionShellView` 页面注册表；`Attach` 创建真实页面一次，`NavigateTo` 现在仅在目标引用变化时写入 `PageHost.Content`，同页重复请求不重设视觉树。没有改服务、DTO、命令绑定、游戏选框、滚动条、取消/错误/恢复保护或有限列表策略。
- `ccd71c27` 补真实生产 `TaskCenterView`/`MediaCenterView` STA WPF 夹具和 shell layout pass 计数。任务→媒体 `2/2`、媒体→任务 `1/1`；96 条合成任务的 DataGrid 选中项、DataContext、ItemsSource 和 offset `10→10` 保留；同页重复导航 `0/0`；PageHost 无 Effect 和全页 entrance 入口。
- 隔离身份 `r08-05-source-ccd71c27` / `r08-05-build-ccd71c27` solution `0/0`、Playnite `net462`、XAML `24/24`；R08-05 `2/2`，相邻回归 `28/28`，source/XAML/diff check 通过；WPF 静态检查 `0 error / 21 warning / 177 info`。证据：`R08-05-PAGE-SWITCH-20260918.md`。
- 只证明隔离 STA/offscreen logical DIP 与合成数据，不等价真实 Playnite、物理 DPI/跨屏、presented frame、UIA/读屏、ETW 或宿主性能；Demo 原始目录不可用。本项没有页面级 XAML/响应式变更，未运行 render-qa；下一项 R08-06。

## 2026-09-18 R08-04 业务完成节奏

- 先复用并验证既有业务链：`BusyOperationCoordinator` 的真实 await、失败/取消分流、Dashboard 的终态任务通知和 Task Center 的 `StateDisplay`/错误详情已经满足“慢请求不假完成”；本阶段没有重建服务或 DTO。
- `3bf90a00` 在共享 `Button` 中把即时 `IsBusy` 与视觉 `IsBusyIndicatorVisible` 分离，视觉延迟 `120ms`，快速完成负例不显示 spinner，慢任务实际显示；按钮卸载/结束会停止 DispatcherTimer。`43141399` 将反馈 peer 的说明校正为 net462 实际能力。
- `FeedbackToast` 使用 `AutomationProperties.Name/HelpText` 与 `AutomationElementIdentifiers.NameProperty` 的 property-changed 信号；net462 没有 `LiveSetting/LiveRegionChanged`，因此不宣称 Narrator 或真实读屏已通过。R02/R08-04 定向 `8/8`，solution `0 error`、Playnite `net462`、XAML `24/24`、源校验通过；构建保留 8 条 `NU1900` 漏洞索引网络警告。
- 证据仍限于合成 DTO/fake、隔离 STA WPF/offscreen logical DIP；真实 Playnite/Worker 长请求、真实 UIA/Narrator、物理 DPI/跨屏、presented frame、ETW、宿主性能未验。游戏选框、滚动条、有限列表、命令绑定、取消/错误/恢复保护未改。证据见 `R08-04-BUSINESS-FEEDBACK-20260918.md`；下一项 R08-05 页面切换轻量化。

## 2026-09-18 R08-03 离屏与隐藏停机

- 先核对共享 `ProgressBar` 模板和两个实际不确定进度入口；原有实现只在 `IsIndeterminate` 触发器中无限循环，没有处理 Tab 隐藏或窗口最小化。`4a18fe0c` 增加 `IndeterminateProgressBehavior`，用真实 `Loaded/Unloaded`、`IsVisible` 和宿主窗口状态控制模板 `PauseStoryboard/ResumeStoryboard`，不改业务忙碌状态。
- 当前身份隔离源码根 `r08-03-source-hiddenstop`：solution Release `0/0`、Playnite `net462`、XAML `24/24`；焦点串行 `22/22`（1+1+2+10+8）。真实共享模板行为在可见时移动 `>0.5 DIP`，隐藏 Tab/最小化各暂停 `360ms` 且位置保持到小数后三位，恢复后继续运动。
- 资源字典类相邻运行 `136 passed / 39 skipped / 1 failed`；唯一失败是既有 Save“时间”列源码字符串期望，不归入本项。边界为合成 Tab/ProgressBar、隔离 STA WPF/offscreen logical DIP，不等价真实 Playnite/物理呈现/UIA/ETW/宿主性能；Demo 原始目录不可用，沿用恢复生产基线。下一项 R08-04 业务完成节奏。

## 2026-09-18 R08-02 热关闭动画

- 先核对已有能力：Dashboard 的应用/系统监听和侧栏关闭归一化已经存在，Settings 也已有系统参数与 `Checked/Unchecked` 入口；缺口是通用 render-only 时钟没有按 Dispatcher 集中失效，Settings 入场时钟在自身开关关闭时没有明确收口。`459de0de` 增加弱引用登记、generation guard 和 `NormalizeAll`，不改变业务契约或安全语义。
- 当前身份 `r08-02-build-459de0de` / `r08-02-solution-build-459de0de` 为 `0/0`，Playnite `net462`，XAML `24/24`；串行焦点 `21/21`（1+2+10+8）。Settings 真实窗口关闭时 opacity/Y 时钟释放并落到 `1/0`，重新启用不重放；并行合跑的 3 条 WPF Foundation 时序抖动未计入通过。
- 边界仍是合成设置、隔离 STA/offscreen logical DIP；没有实际修改 Windows 动画偏好，也未验真实 Playnite/物理输入/DPI/呈现/UIA/ETW/宿主性能。WPF 退出 COM 清理诊断已记录且测试退出码为 0；Demo 原始目录不可用，继续恢复生产基线。下一项 R08-03 离屏与隐藏停机。

## 2026-09-18 R08-01 中途反向连续

- 先核对当前代码：侧栏已有 `sidebarTransitionGeneration`；`GscMotion.AnimateTranslate` 的旧完成回调是实际缺口。`a7aaa89e` 用 `ConditionalWeakTable<FrameworkElement, MotionState>` 增加按元素 generation guard，`d62757e9` 只稳定真实 STA WPF 取样，不重建动画体系，也不改变命令/绑定、选框、滚动条、取消/错误/恢复或服务 DTO。
- 当前身份 shadow `r08-01-build-d62757e9`、完整 solution shadow `r08-01-solution-build-d62757e9` 均为 `0/0`，Playnite `net462`，XAML `24/24`；R08 `2/2`，相邻 Chrome `10/10`、Foundation `8/8`。实际 Translate `8.403→8.403→-8`，侧栏 `169.333→169.333→221.333→270`，opacity `1`，transition/opacity clocks released。
- 夹具断言中点、反向当前值、最新目标、最终状态和 clock release。事实边界是合成/fake、隔离 STA WPF/offscreen logical DIP；没有真实 Playnite/物理输入/DPI/呈现/UIA/ETW/宿主性能或 OS reduced-motion 证据。Demo 原始目录不可用，继续恢复生产基线。下一项 R08-02 热关闭动画。

## 2026-09-18 R07-08 resize 压力序列

- 先核对 shell 的 Render 优先级 resize 合并、Task 独立详情滚动和游戏选择器 overlay；没有新增布局体系。`9f478cac` 新增真实生产 shell/page 夹具，执行 `1366×900→960×700→960×560→1440×900→1366×900`，详情、菜单和搜索框焦点均保持。
- 当前身份 shadow 构建 `r07-08-build-9f478cac` 为 XAML `24/24`、solution `0/0`、Playnite `net462`；`R07ResizeStressBehaviorTests 1/1`、R07 过滤回归 `14/14`。表格 ActualHeight `525.333/180/180/525.333/525.333`，Task MaxHeight 全 `∞`，详情 MaxHeight `∞/160/160/∞/∞`，选择器 ActualHeight `122.667` 且 MaxHeight `774.667/541.333/401.333/774.667/774.667`。
- WPF 退出阶段有已知 TextServices COM 清理诊断，但测试退出码为 0；该输出没有被改写成宿主通过。边界仍是合成 DTO、隔离 STA WPF/offscreen logical DIP，无真实 Playnite/拖拽 resize/物理呈现/UIA/ETW/宿主性能。Demo 原始目录不可用。下一项 R08-01 中途反向连续。

## 2026-09-18 R07-07 触控板小增量

- 核对最新实现后确认滚动所有权和 delta 加速已经存在；`98e8b244` 只补共享行为的最终边界收口：没有可移动内层/外层时标记 no-op，避免 WPF 重复安排布局；仍保留可移动外层的边界转发和内层原生滚动。
- 当前身份 shadow 构建 `r07-07-build-98e8b244` 为 XAML `24/24`、solution `0/0`、Playnite `net462`；`R07FineScrollBehaviorTests 2/2`、R07 过滤回归 `13/13`。实际数据：小增量 offset `0,3,...,36`，反向 `33,30,27`，末端 `154/154`，可见行 `6–9`，20 事件布局 `17`，末端 no-op 增量 `0`；加速边界外层 `32→80/432`、内层不跳、布局 `1`。
- 测试用合成行、隔离 STA WPF/offscreen logical DIP；未宣称物理触控板/OS 输入、真实 Playnite/呈现、物理 DPI/跨屏、UIA/读屏、ETW 或宿主性能。Demo 原始目录不可用，继续恢复生产基线。下一项 R07-08 resize 压力序列。

## 2026-09-18 R07-06 状态横幅预算

- 先核对当前生产实现后确认状态语义已存在：Task 无旧数据失败走 `WorkspaceStatePresenter`，保留旧数据的失败/刷新走带重试横幅；Save/Media/Maintenance Stale 横幅和安全模式恢复入口也已绑定。`3551de81` 只新增真实 WPF 行为夹具，没有重建生产状态或改变安全语义。
- `.tmp\\r07-06-build-3551de81` 身份一致构建为 XAML `24/24`、solution `0/0`；R07-06 `4/4`，相邻空态/Task 响应/R06 详情/R07 滚动 `13/13`，合并 `17/17`。实际检查重试/恢复命令、错误 presenter、表格最小高度和非零 viewport。
- 更宽相邻合跑的 Media 源契约失败期待旧字符串，另有一条既有 intentional skip；未改写、未归入本项通过。边界仍是合成/fake、隔离 WPF/offscreen logical DIP、无真实 Playnite/设备输入/物理呈现/ETW/宿主性能/真实数据写入。下一项 R07-07 触控板小增量。

## 2026-09-18 R07-05 详情断点稳定

- `0daca6f0` 在当前分支实现并推送详情断点滞回：980 DIP 基线，`<972` 进入紧凑，`>=988` 回宽；Save/Media/Task/Maintenance 都保留现有详情对象、选择、焦点和 `ScrollViewer`，不改游戏选框、滚动条和业务安全语义。
- `.tmp\\r07-05-build-0daca6f0`（构建身份与提交一致）的 Release XAML `24/24`、solution `0/0`；`ResponsiveLayoutCoordinatorTests 5/5`、`R07DetailsBreakpointBehaviorTests 1/1`；相邻滚动归属 `2/2`、Task 响应 `7/7`、R06 详情预算 `2/2`，合并 `17/17`；源校验/diff check 通过。
- 测试是合成 DTO/fake、隔离 STA WPF、offscreen logical DIP；不等价真实 Playnite 拖拽、物理 DPI/跨屏、设备输入、UIA/读屏、presented frame、ETW/宿主性能。Demo 原始目录不可用，继续以恢复生产基线为准。下一项 R07-06 状态横幅预算。

## 2026-09-18 R01-08 跳过测试说明复验

- 当前 HEAD 72be494d 的隔离输出 .tmp\r01-08-current-72be494d 已产出测试程序集；Core 全量 83/0/0，Playnite 的 WorkerIpcClientBehaviorTests 7/0/0（含 6 条 NamedPipe gated），Worker 的 WorkerProcessRestartTests 1/0/0，源码清单 LegacyProductionUiBaselineFact=57。
- R01-08 现在只以定向 gated 结果和 57 条 legacy 分类签收；当前全量 Playnite 直接复跑观察到非 R01 的 WPF 资源树、动画、布局及 R02/R06/R07 行为失败，未计为 skip，也未写成全量通过。后续必须按独立任务处理，不能用本项摘要掩盖。
- 生产 UI、服务/DTO、命令绑定、游戏选框、滚动条、取消/错误/恢复保护和有限列表性能未改。R02-01～R02-05 已有证据，R02-06 仍为宿主菜单 visual tree 外部阻塞；下一可执行小批量为 R07-05 详情断点稳定。真实 Playnite、物理 DPI/跨屏、OS 输入/IME、presented frame、UIA/读屏、ETW 与宿主性能未验。

## 2026-09-18 R01-06 宿主证据保全复核

- 旧归档因 `RenderHarness/Program.cs` 后续变化被 freshness 正确标为 stale；没有直接沿用 `3929ed7`。复用当前 `c2399d7b` R01-03 审计生成并更新归档，保存完整身份、summary、manifest、route/fidelity/layout、20 行索引和六张精选图。
- 当前归档结果：161 运行时快照、80 INFO、0 Fidelity、0 失败路由、0 HIGH/0 MEDIUM；索引校验 `20/20`。完整 362 张图不进仓库，README 用当前身份的 solution + RenderHarness 独立 restore/build/audit 命令复现。
- metadata 的 OutputRoot/ZipPath 已改为仓库相对或可再生临时路径；未保留绝对工作树、旧 zip 或完整截图。范围仍是 WPF offscreen logical DIP，不等价真实 Playnite/物理呈现/输入/ETW/宿主性能。下一步更新 R01-07 baseline 身份。

## 2026-09-18 R01-05 负例注册表复核

- `810114e2` 当前隔离构建绑定源码根，XAML `24/24`、Release `0/0`；`UiNegativeFixtureRegistryTests` `1/1`。五类 expected-failure 均由实际 detector 捕获：对比度 1 violation、数值列横向失败、无结果 Enter 保持旧选择、层级 overflow gate、Loading 底层不可命中。
- `validate-source.py` 通过；本批没有修改生产入口或安全语义，只确认旧测试侧注册表在当前源码下仍有效。R01-05 可满足，R01-06 仍因 freshness 命中 RenderHarness `Program.cs` 待重跑。
- 继续使用合成/fake、隔离 WPF/offscreen logical DIP，不宣称真实 Playnite/OS 输入、物理 DPI、presented frame、ETW 或宿主性能。下一步先把 R01-05 的 `sourceCommit` 更新到当前证据，再处理 R01-06。

## 2026-09-18 R01-07 freshness 基线复核

- 根据实际新证据更新 baseline：R00-01-02→`89aa27e1`，R00-03/R00-04→`e5a12ffa`，R00-05/R00-06→`e8fe1aab`，R00-07/R00-08→`4f585024`，R01-01/R01-02→`a5219c09`，R01-03→`c2399d7b`。这些是各自已运行的隔离证据身份，不是把当前 HEAD 统一填入所有项目。
- 当前 `check-ui-evidence-freshness.ps1` 扫描为 14 条记录、14 fresh/0 stale；R01-06 已绑定当前审计身份 `c2399d7b`。R01-07 的三类 smoke（文档-only、共享控件、包身份 mismatch）通过。
- `package=not-provided` 仍不等于真实宿主安装；全 fresh 只表示路径基线没有新的非文档命中。下一步进入 R01-08 skip 说明。

## 2026-09-18 R01-03 每项证据直达

- 当前提交 `c2399d7b` 新建隔离构建并运行完整 RenderHarness 审计，输出身份 `c2399d7be9f723e77226619172be16778fe3646f`；solution/RenderHarness `0/0`、XAML `24/24`。
- 审计保留 161 个运行时快照和 80 条预期内部滚动 INFO，Fidelity/失败路由/HIGH/MEDIUM 均为 0。`EVIDENCE_INDEX.md` 实际有 E01～E20，校验器四项均 `20/20`：报告/源码入口、完整身份、样本、未验边界；静态-only 行明确没有运行时几何样本。
- 当前 freshness 扫描发现旧 R01-03 baseline 因 `Program.cs` 后续变更而需重跑，本次已完成新审计并在证据中保留该事实；R01-07 后续负责校正版本化 baseline/扫描记录，不把“需要重跑”改写成产品缺陷。
- 证据继续限于合成/fake、实际 WPF、隔离 STA/offscreen logical DIP；真实 Playnite、物理 DPI/跨屏、OS 输入/IME、presented frame、UIA/读屏、ETW 和宿主性能未验。Demo 原始目录缺失，沿用恢复生产基线。下一项为 R01-07 基线失效规则复核。

## 2026-09-18 R01-01 / R01-02 收口

- `a5219c09` 修正 R01-01 发现的最后一个源码回溯：`R06EmptyStateBehaviorTests.FindRepositoryRoot()` 改用 `TestRepositoryContext.Root`。缺陷来自当前隔离定向身份扫描，先前的旧默认程序集错根拒绝仍保留为有效负例；没有改生产 UI、DTO、服务、命令或空表状态语义。
- 当前提交新建隔离 OutputRoot 后，solution 与 RenderHarness 为 `0 warning/0 error`，XAML `24/24`；身份/代表性源码筛查 `16/16`，R01-02 的实际 STA WPF 数值测试 `2/2`。两项账本均已满足当前可控条件。
- Light/Dark `finesseprobe` 分别保留独立报告和 PNG，均绑定完整 `a5219c09...`、`WorkingTreeClean=True`，四个正例 `4/4` 完整，窄列长负数明确 `HorizontalFit=False` 但 `VerticalFit=True`，不是只看 Assert.Contains 或行高。
- 运行边界继续写实：合成数据/fake、隔离 STA WPF、offscreen logical DIP；没有真实 Playnite、OS 输入/IME、物理 DPI/跨屏、presented frame、UIA/读屏、ETW 或宿主性能证据，也没有真实存档/媒体/云端/诊断写入。Demo 原始目录缺失，继续以恢复生产基线为准。
- 下一步按 `R01-03-EVIDENCE-INDEX-20260916.md` 检查 20 项索引的直达性和负例；只有被引用的当前证据目录保留，旧的未引用隔离输出在验证完成后清理。合适的小阶段完成后再评估把当前分支合并回 main，合并前必须保护 main 的用户 `src.zip`。

## 2026-09-18 R00-07 / R00-08 证据校正

- 当前 HEAD `4f585024` 的隔离构建复核 R00-07/R00-08：toolbar 源契约 `30/30`、选框键盘/焦点/搜索实际 WPF 路由 `31/31`，RenderHarness `0/0`。
- `toolbarprobe` 正常表单、同祖先超宽、同祖先不可达三场景均按用途/几何分类；R00-08 通过无结果旧选择保护、活动 composition 不误确认、IME/方向键保持弹层、可见候选 Enter 关闭并回焦点、Esc/清除焦点回返。
- R00-07/R00-08 已满足当前可控条件；证据为合成 DTO、实际生产 WPF Window、隔离 STA/offscreen logical DIP，不等价真实 Playnite/OS IME/物理呈现/宿主性能。Demo 原始目录仍缺失。
- 下一小批量为 R01-01、R01-02、R01-03：先用当前 HEAD 的隔离身份复跑，校正旧证据与具体索引，不把旧默认 bin 的拒绝绕过。

## 2026-09-18 R00-05 / R00-06 证据校正与夹具修正

- R00-05/R00-06 一次默认 bin 复跑因旧程序集 `447ac07e` 与当前源码身份不一致被 R01-01 正确阻断；改用当前 checkout 的隔离 OutputRoot 后定向 `6/6`，其中 R00-05 `2/2`、R00-06 `4/4`。
- `e8fe1aab` 修正 RenderHarness alternate-density 夹具：生产 DataGrid 各列的 HeaderStyle 也覆盖到 `36 DIP`，与行 `44 DIP` 一起被实际测量；没有降低生产列头 `42 DIP MinHeight`，也没有改滚动条/页面/游戏选框。
- clean `mediageometryprobe` 双主题五场景 OK，包含水平条、短窗回退和 blocked-parent HIGH 负例；clean `shellqa` 三尺寸通过；审计 161 快照、80 INFO、0 HIGH/0 MEDIUM、0 Fidelity/路由失败。R00-05/R00-06 已满足当前可控条件。
- 边界仍为合成/fake、隔离 WPF/offscreen logical DIP；Demo 原始目录缺失，沿用恢复生产基线，真实宿主和物理输入/呈现/性能未验。下一小批量为 R00-07/R00-08。

## 2026-09-18 R00-03 / R00-04 证据校正

- 当前 HEAD `e5a12ff` 复核已有动效生命周期和搜索基准，R00-03 实际 WPF Dispatcher 行为 `3/3`，R00-04 合成 2,000 项基准/不可能结果负例 `2/2`；隔离 Release RenderHarness `0/0`。
- Light/Dark `motionreentryprobe` 从当前渲染宽度接管并在终态 `270/X=0` 清钟；`motionhotprobe` 捕获中间态后热关闭归一 `72/Opacity=1`，禁用重入立即完成。R00-04 原始 artifact 记录 30 个不同查询、30 个可见集合变化、p50/p95/max=`46/47/48ms`。
- R00-03/R00-04 当前可控条件已满足并更新账本；R18-01 仍独立处理连续输入、IME、20ms debounce 分配。边界仍为合成/fake、隔离 WPF/offscreen logical DIP，不宣称真实宿主、ETW、物理呈现或性能。
- Demo 原始目录仍缺失，继续用恢复生产基线；未写真实存档、媒体、云端或诊断数据。下一小批量为 R00-05/R00-06。

## 2026-09-18 R00-01 / R00-02 证据校正

- 当前 HEAD `89aa27e1` 复核既有实现：R00-01/R00-02 只补当前身份与行为证据，不重建按压合成或变换树；定向行为测试 `4/4`，隔离 Release XAML `24/24`、解决方案 `0/0`、RenderHarness `0/0`。
- Light/Dark `finesseprobe` 均 `finesse-fixture OK`，88 个完整状态组合无对比度违规；黑底灰背景和非等距 stop 负例确实捕获；1000 次组合树复用保持节点/深度，独立控件缩放实例不串扰；四个数值样本横纵向可读，窄列负例必失败。
- R00-01/R00-02 当前可控条件已满足并更新账本；证据仍只覆盖合成数据、实际 STA WPF/offscreen logical DIP，不等价真实宿主按压、物理屏幕或呈现帧。可变 Freezable 共享隔离不在本批次，留给 R08-08。
- Demo 原始目录仍缺失，继续用恢复生产基线；未写真实存档、媒体、云端或诊断数据。下一小批量转入 R00-03/R00-04 复核。

## 2026-09-18 R07-04 横向滚动端点

- `8f682fcb` 先复用现有 DataGrid 模板和滚动条系统，只在 RenderHarness 增加实际 WPF `horizontalprobe`；没有改生产 XAML、游戏选框、命令/Binding 或业务服务。
- Light/Dark×Save/Task/Media/Maintenance 共 8 组合通过水平负端点/超最大端点夹断、右端末列与末单元格完整可见、横向条不遮挡第一行、回左无漂移；Task 右端详情列和 Media 右端文件/原因列截图在真正右端 offset 时保存。
- Media 继续保持 `Standard/Item/EnableColumnVirtualization=False`；XAML `24/24`、Release `0/0`、RenderHarness `0/0`。报告 `.tmp/r07-04-horizontal-clean/horizontalprobe-report.txt` 绑定 `WorkingTreeClean=True`、offscreen `DpiScale=1.00`。
- 证据只覆盖合成 DTO/fake、隔离 STA WPF/offscreen logical DIP；Demo 原始目录仍缺失，沿用恢复生产基线。真实宿主、设备输入、Ctrl/Shift/IME、UIA、物理 DPI/跨屏、presented frame、ETW、宿主性能和真实数据写入未验。按用户续跑顺序下一小批量为 R00/R01 证据校正，R07-05 仍是账本下一布局项。

## 2026-09-18 R07-03 短窗底栏可达

- 先复用现有生产布局：shell 固定 footer 与主内容分行，Media/Save/Task/Maintenance 已有提示条、页面/表格/详情滚动和加载/取消/保存命令；没有把 Demo 缺失误判成需要换设计体系。
- `d19e848b` 新增隔离 RenderHarness 短窗探针及合成 Media page-more 状态，`447ac07e` 扩展到 Light/Dark 两主题和 1040×700/560 两高度。实际 STA WPF 元素矩形、祖先关系、可见性和 ScrollViewer offset 验证 16 个组合均 `shortwindowprobe OK`；Save 先发现最大 offset 不能代表中间按钮可达，改为真实按钮 `BringIntoView` 并保留该负例。
- R07-03 相关相邻回归 `20/20`，XAML `24/24`，Release `0/0`，源码校验/diff check 通过。clean 报告 `.tmp/r07-03-short-window/shortwindowprobe-report.txt` 绑定 `447ac07e...`、WorkingTreeClean；证据为合成/fake/隔离 STA WPF/offscreen logical DIP，不等价真实 Playnite、物理 DPI/跨屏、呈现帧或性能。
- 当前分支仍未覆盖真实 Playnite/Worker 时序、设备输入、UIA/读屏、ETW、宿主性能或真实数据写入；Demo 原始目录仍缺失。下一小批量为 R07-04 横向滚动端点。

## 2026-09-18 R07-02 锚点删除回退

- 先核对最新实现再补缺口：Task、Media 主库/Inbox、Findings、进程映射、云端队列、Save 历史/候选各自已有稳定键或组合键，但多个路径在对象删除/筛选后会首项回退或失选。新增 `SelectionAnchorResolver`，统一“稳定 ID 命中优先、旧行位邻近回退”；不触碰集合分页、虚拟化、滚动条和业务命令。
- Task 全量快照/历史分页记录 `TaskId + 旧索引`，任务导航目标在行位回退前优先；Media 用 `MediaId`；Findings 用 `PlayniteId + Code + Title`；进程映射用 `ExecutableName`；云端用 `TransferKey` 并在一致性分页有后续页时继续 pending key；Save 历史用 `BackupId`，候选用既有 `PlayniteId + Path`。
- `R07SelectionAnchorBehaviorTests 4/4` 包含稳定键位置变化、删除首项误选负例、实际 STA WPF DataGrid 删除行后的选中邻项，以及生产 Save 候选恢复方法；最终相邻回归 `20/20`、0 skipped。代码提交 `7acb61a5`，测试补充提交 `ef53a748`。
- clean RenderHarness `.tmp/r07-02-anchor-final/render-qa-report.txt` 绑定 ef53a748、WorkingTreeClean、双主题、多尺寸/滚动/resize 与 50/400/2000/4468 合成数据量通过；当前 checkout 没有 Demo 原始目录，仍沿用恢复生产基线。离屏行为不等价真实 Playnite/Worker、设备输入、UIA/读屏、物理 DPI/跨屏、presented frame、ETW、宿主性能；未写真实用户数据。下一小批量为 R07-03 短窗底栏可达。

## 2026-09-18 R07-01 滚动所有权

- 先盘点已有契约：`GscPageScrollViewer` 管页面垂直主滚动，`GscRedesignWorkspaceDataGrid` 模板的 `DG_ScrollViewer` 管虚拟化表格，`GscInspectorScrollViewer` 管详情；水平端点和当前滚动条系统不改。
- `9c878b9c` 新增 `ScrollBoundaryRoutingBehavior` 并接入两个共享 ScrollViewer 样式与共享 DataGrid 样式。最近内层能沿滚轮方向移动时保持 WPF 原生处理；到边界才寻找最近外层并转发一格，外层也不可动则不伪造移动。DataGrid 行源通过模板内实际 ScrollViewer 解析，避免页面/表格同步跳动。
- `R07ScrollOwnershipBehaviorTests 2/2` 覆盖普通嵌套上/下边界和实际 DataGrid 边界；最终身份正确重建后的相邻回归 `14/14`；XAML `24/24`、Release `0/0`、源校验/diff check 通过。
- clean `.tmp/r07-01-scroll-final/render-qa-report.txt` 绑定完整 SHA，双主题、多尺寸、resize `render-qa OK`，1040×700 各工作页保留至少四行，1366 Task `4/4`。代表图已查看。证据限于合成数据、隔离 STA WPF/offscreen logical DIP。
- 未验真实 Playnite/Worker 和物理滚轮/触控板、UIA/读屏、物理 DPI/跨屏、presented frame、ETW、宿主性能；Demo 原始目录仍缺失，未写真实存档/媒体/云端/诊断数据。下一项 R07-02 锚点删除回退。

## 2026-09-18 R06-08 详情与行高预算

- 先查已有实现：Task/Media/Save/Maintenance 都已有详情对象、独立详情滚动或 Inspector、选中绑定和紧凑布局预算；Task 还有折叠技术详情。没有把长诊断塞进所有列表行，也没有替换游戏选框或滚动条。
- `07376adb` 修复 Save 详情刷新时选中候选的实际缺口：在替换集合前保留旧对象，按大小写不敏感的 `PlayniteId + Path` 恢复刷新后的同一候选，再按 Pending/首项回退；因此 Accepted/Rejected 选中项不会因为 Pending 排在前面而跳变。
- `R06DetailsBudgetBehaviorTests 2/2` 覆盖稳定候选恢复和真实生产 TaskCenter 长详情切换、折叠详情展开、详情滚动与列表行高；相邻 Task 响应式/Media 四行/详情 disclosure `10/10`。XAML `24/24`、Release `0/0`、源校验/diff check 通过。
- clean `.tmp/r06-08-render-final/render-qa-report.txt` 绑定完整 SHA、`WorkingTreeClean=True`、双主题和 1040/1100/1366/2560 DIP/resize `render-qa OK`；Task 紧凑详情 `160 DIP`、宽详情 `360×516`，最窄 Task `4/4`，Media/Maintenance/Save 四行门禁通过。代表图已查看。仅为合成数据、隔离 STA WPF/offscreen logical DIP。
- 真实 Playnite/Worker、UIA/读屏、OS 输入/IME、物理 DPI/跨屏、presented frame、ETW、宿主性能和真实服务失败时序未验；未写真实存档、媒体、云端或诊断数据。Demo 原始目录仍缺失，沿用恢复生产基线。下一项为 R07-01 滚动所有权，先梳理页面/表格/详情/弹层 ScrollViewer 所有权。

## 2026-09-18 R06-07 空表保留结构

- 先核对已有状态 presenter：Task 的 Empty/FilterEmpty/Loading/Error、Media 的 `WorkspaceDataState` 与 stale、Maintenance 的诊断状态均已覆盖本任务；唯一生产缺口是 Save 以 `IsBusy + Count == 0` 判断空态，加载或失败时会误显“暂无存档历史/候选”。
- `5046bf8f` 为 Save 详情增加 `WorkspaceDataState` 生命周期和 presenter 绑定：首次空、候选处理完成/本次扫描无新结果、加载中、无旧数据失败、已有数据失败分别表达；失败时保留旧行并显示降级提示，重试复用 `LoadDetailsCommand`。没有修改服务契约、真实表格滚动模型或候选状态查询语义。
- `R06EmptyStateBehaviorTests 2/2` 实际加载生产 `SaveCenterView` 和合成状态，验证历史 7 列、候选 4 列、Loading/Empty/Ready/Error 状态可见性及重试命令；Release XAML `24/24`、编译 `0/0`，相邻状态/响应式/进度回归通过。
- `.tmp/r06-07-emptytables/emptytables-report.txt` 绑定 `5046bf8f...`、`WorkingTreeClean=True`，双主题 1040×700/1600×900 `emptytables OK`；人工查看 Save 历史 Light 与路径核验 Dark 图。该目录是当前保留的 R06-07 离屏证据。
- Demo 原始目录在当前 checkout 仍不存在，沿用恢复生产基线；保留游戏选框、滚动条、命令/Binding、取消/错误/恢复保护、有限列表和 net462。未验真实宿主 UIA/读屏、OS 输入/IME、物理 DPI/跨屏、presented frame、ETW、宿主性能和真实服务失败时序；未写真实存档、媒体、云端或诊断数据。
- 下一项：R06-08 详情与行高预算，先确认长诊断使用现有详情区而非扩张所有列表行，并验证选中项更新后详情对象及四行门禁。

## 2026-09-18 R06-06 行内进度稳定

- 先核对已有能力：`TaskStatusDto` 已提供进度钳制/未知显示层，`SnapshotComparers.Task` 比较进度和状态，`TaskIndexedCollection.Merge` 按 `TaskId` 更新现有行；任务分页的总数/完成汇总来自 `TaskPageDto.Summary`，不能从当前加载页推断。
- `72fd6d9a` 只补真实生产 TaskCenter 的取消中表达：`TaskCancellationStatusText` 按既有 `IsCancellingTask` 显示“正在取消…”，并设置兼容 net462 的 `AutomationProperties.HelpText`。没有另造 Cancelling 状态、没有改取消请求/Worker/终态协议、没有替换游戏选框或滚动条。
- 真实 STA WPF `R06TaskProgressBehaviorTests` 验证 80 行 DataGrid 的第 42 行进度更新只产生 `Replace` 而无 `Reset`；按 ID 回写后选择索引、任务 ID、逻辑滚动偏移和行数保持。排队/未知/0/超界进度与取消/成功状态边界也通过；`fd9326d7` 再验证实际 TaskCenter 提示的折叠→可见、文本和 HelpText。
- 定向结果：R06-06 `4/4`；TaskIndexed `4/4`、Batch `3/3`、R03 数值 `10/10`、R06 选中焦点 `2/2`、排序 `4/4`；XAML `24/24`、Release 编译 `0/0`、源码校验/diff check 通过。
- clean RenderHarness `.tmp/r06-06-render-final/render-qa-report.txt` 绑定生产代码 `72fd6d9a...`，双主题 357 PNG、任务页 1040/1100/1366/2560 DIP、多数据量、滚动/虚拟化/resize、`WorkingTreeClean=True`、`render-qa OK`；Light/Dark Task 1040×700 已抽查。
- 证据范围仍是合成数据、隔离 STA WPF/offscreen logical DIP；未验真实 Playnite 长任务/取消竞争、UIA/读屏、物理 DPI/跨屏、presented frame、ETW、宿主性能；未写真实存档、媒体、云端或诊断数据。下一项 R06-07 空表保留结构。

## 2026-09-18 R06-05 列头说明

- 先核对既有共享列头模板、排序箭头/拖拽部件和容量格式化：表头已有 `Wrap + TextTrimming=None`、22 DIP sort slot、透明 8 DIP resize thumbs；生产 DTO 已统一输出 `B/KiB/MiB/GiB` 1024 进制。缺口是单位、缩写和同名状态列没有可达解释。
- `7527e638` 新增 `DataGridColumnHeaderHelpBehavior`，说明挂在 `DataGridColumn`，生成 header 在 Loaded 时设置 Tooltip 和 `AutomationProperties.HelpText`；Header 仍是字符串，未加按钮/独立事件，排序、重排和拖拽继续走原生列头。Save/Task/Media/Maintenance Findings 主要列已补说明。
- `R06ColumnHeaderHelpTests 2/2`：实际 WPF 生成列头验证说明、原始 Header、可排序/重排和无嵌套按钮；R06-04 `3/3`、R06-03 `2/2`、R06-02 + R06-01 `9/9`；XAML `24/24`、Release `0/0`、源校验/diff check 通过。
- clean RenderHarness `.tmp/r06-05-render-final/render-qa-report.txt` 绑定完整 SHA，双主题 357 PNG、生产主要表格 header contract `resize=true/sort-arrow=visible`、50/400/2000/4468 数据量、滚动/虚拟化/resize、`WorkingTreeClean=True`、`render-qa OK`；Save/Task/Maintenance 代表图已抽查。
- 边界仍是合成数据、隔离 STA WPF/offscreen logical DIP；未验真实 Playnite 悬停/排序/拖拽、UIA/读屏、物理 DPI/跨屏、presented frame、ETW 或宿主性能；未写真实存档、媒体、云端或诊断数据。下一项 R06-06 先核对任务进度值/显示/刷新路径和选择滚动保持。

## 2026-09-18 R06-04 复制单元格与整行

- 先盘点现有复制能力：复用 `DashboardViewModel` 的路径、任务错误、诊断和维护报告命令，以及 `CopyTextWithRetryAsync`；复用 `TaskStatusDto`、`BackupVersionDto`、`SavePathCandidateDto`、`MediaItemDto`、`ValidationFindingDto`。没有从 main 复制旧实现，原始 Demo 页面目录在当前 checkout 不存在，继续以恢复的生产基线为准。
- `afe4aa55` 新增共享 `DataGridClipboardBehavior` 与五类 DataGrid profile。`Ctrl+C` 复制 Extended 选中行，`Ctrl+Shift+C` 复制当前单元格；按显示顺序输出稳定 TSV/CRLF，稳定 ID 去重；完整技术字段不使用视觉省略值；凭据语法统一 `[已隐藏]`，公共文本写入点也做脱敏。
- `R06ClipboardBehaviorTests 3/3`、R06-03 选中焦点 `2/2`、R06-02 排序 + R06-01 列宽 `9/9`；XAML `24/24`、Release 编译 `0/0`、源校验/diff check 通过。全量 Playnite 合跑观察值 `573/679`、`57` 跳过、`49` 既有 WPF 环境失败，未当作全量通过。
- clean RenderHarness `.tmp/r06-04-render-final/render-qa-report.txt` 绑定完整 SHA，双主题 357 PNG、50/400/2000/4468 数据量、滚动/虚拟化/resize、`WorkingTreeClean=True`、`render-qa OK`；保留当前 R06-03/R06-04 证据，旧 `.tmp` 构建/审计/渲染目录已清理。
- 边界仍是合成数据、隔离 STA WPF、验证 seam 和 offscreen logical DIP；未验真实 Playnite 选择/OS 剪贴板/UIA/读屏/IME、物理 DPI/跨屏、presented frame、ETW 或宿主性能；未写真实存档、媒体、云端或诊断数据。下一项 R06-05 先盘点生产表头说明、单位、Tooltip/Automation 和共享资源。

## 2026-09-18 R06-03 选中焦点区分

- 先盘点共享 `ListBoxItem`/`DataGridRow` 模板和 Media Inbox 本地行样式：现有 active selected、hover 和 DataGridCell 透明内容面可复用，真正缺口是失焦选中、键盘当前边框、错误行以及本地 Media trigger 会覆盖共享状态。没有从 main 复制旧实现，也没有替换游戏选框或滚动条。
- `d83c7378` 在 `WpfUiProduction.xaml` 统一补 active/inactive/keyboard/error 状态，失败态用 `GameSaveCenter.Contracts.TaskState.Failed` 强类型值；`DesignTokens.xaml`/`AdaptiveThemePalette` 增加 `GscSelectionInactiveBrush` 的静态与活动调色板路径；Media Inbox 回到共享行状态。原始 Demo `DesignShellView.xaml`/`Pages` 在当前 checkout 不存在，按事实沿用恢复生产基线。
- `R06SelectionStateBehaviorTests 2/2` 使用合成 `TaskStatusDto`、真实生产资源和隔离 STA WPF，实际验证失败色、键盘焦点 2 DIP 边框、失焦色/边框；资源回归 `137 passed / 39 skipped / 0 failed`，R06-02 排序 `4/4`、R06-01 列宽 `5/5`，XAML `24/24`、编译 `0/0`、源校验/diff check 通过。
- clean RenderHarness 报告 `.tmp/r06-03-render-final/render-qa-report.txt` 绑定 `d83c7378...`，双主题 357 PNG、`WorkingTreeClean=True`、`render-qa OK`；Task/Media/Save 代表图已抽查，数据量 50/400/2000/4468、滚动/虚拟化和 resize 恢复通过。证据为 `R06-03-SELECTION-FOCUS-20260918.md`。
- 边界仍是合成数据、隔离 WPF/offscreen logical DIP；未验真实 Playnite 输入、UIA/读屏、物理 DPI/跨屏、IME、presented frame、ETW、宿主性能；未写真实存档、媒体、云端或诊断数据。下一项 R06-04 先盘点复制命令、DTO/诊断字段和隔离剪贴板能力。

## 2026-09-18 R06-02 排序提示与稳定性

- 先核对现有能力：共享 DataGrid 已提供排序箭头和用户排序开关，生产 Save/Task/Media 表格已有 DTO 绑定但缺少统一原始值/未知值/稳定次键契约；因此只新增控制器和 profile，没有把 main 旧实现带入当前分支，也没有替换游戏选框或滚动条。
- `c577afc5` 接入 Save History/Candidates、Task Queue、Media Inbox。默认时间/置信度降序，数值按原始数值比较，时间按 `DateTime` 比较，枚举按定义值比较，未知值不因降序反转而跑到首位；同值使用 BackupId、Path+PlayniteId、TaskId、MediaId 稳定排序。列头点击和测试 hook 均走同一个升降序切换路径，箭头只标主列。
- `R06SortingBehaviorTests 4/4` 覆盖同值稳定、数字 2/10、未知进度、未知媒体来源、清空后乱序刷新和升降序切换；R06-01 独立回归 `5/5`。XAML `24/24`、编译 `0/0`、源校验/diff check 通过。合并多组 WPF 回归曾因单 AppDomain/Application 和隐藏布局夹具失败，必须按独立进程解释。
- clean RenderHarness 绑定完整 `c577afc5...`，双主题 357 PNG、`WorkingTreeClean=True`、`render-qa OK`；报告确认 Save/Task/Media 头部 `sort-arrow=visible`，多数据量/滚动/resize 通过。证据：`R06-02-SORT-STABILITY-20260918.md`。
- 未验真实 Playnite 多线程刷新/点击、UIA/读屏、OS 输入/IME、物理 DPI/跨屏、presented frame、ETW、宿主性能；未写真实存档、媒体、云端或诊断数据。Maintenance 排序 profile 未扩展。下一项 R06-03 选中焦点区分。

## 2026-09-18 R06-01 列宽用户记忆

- 先核对当前主要入口的真实 DataGrid：Save History/Candidates、Task Queue、Media Inbox 已有用户调整能力、最小宽度和自动横向滚动，但没有持久化；Media 主库是 ListBox，Maintenance 表格不在本项主要入口范围。没有从 main 带入旧实现，也没有改游戏选框或滚动条系统。
- `75a6e6d8` 新增版本化 `DataGridColumnWidths` 和内部 `DataGridColumnLayoutController`。稳定键按视图区隔离；只在用户改变为 Pixel 宽度时捕获，忽略 Star/初始化/响应式赋值，统一按列最小/最大宽度归一化；400ms 防抖、卸载 flush、当前视图区重置并立即保存。旧 `v0` 和未知列键不会污染当前布局。
- 生产接线范围：`save-history` 7 列、`save-candidates` 4 列、`tasks` 6 列、`media-inbox` 5 列；四个页面的“重置列宽”均位于既有操作区。`R06ColumnWidthPersistenceBehaviorTests` 使用隔离设置和合成 DataGrid 实际 `5/5`，包含控制器重建恢复、视图区隔离、版本/未知键负例、最小宽度、重置持久化和窄窗滚动条。
- clean Release XAML `24/24`、编译 `0/0`、源校验/diff check 通过；R05 独立回归全通过。clean RenderHarness 绑定 `75a6e6d8...`，Light/Dark、357 PNG、工作树 clean、`render-qa OK`，报告覆盖多数据量/滚动/resizing，Save/Task/Media `1040×700` 已抽查。证据见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R06-01-COLUMN-WIDTH-PERSISTENCE-20260918.md`。
- “重启恢复”只在隔离设置对象上验证控制器销毁/重建，不能写成真实 Playnite 进程重启或用户拖拽录像；物理 DPI/跨屏、UIA/读屏、OS 输入/IME、presented frame、ETW、宿主性能和实际宿主配置迁移仍未验。Maintenance 表格本轮不记忆化。下一项 R06-02 排序提示与稳定性。

## 2026-09-18 R05-08 弹层资源热切换

- 先复核真实生产 Settings 的 ComboBox `PART_Popup`、Tooltip 样式合并顺序和既有 `GscToolTipBehavior`；缺口不是“没有 Popup”，而是脱离页面资源链的 Tooltip 会从静态 Acrylic 字典拿到旧深色背景。
- `599a8fd9` 在 `AdaptiveThemePalette` 中补旧 Acrylic 兼容键到活动 Demo 主题的别名，并扩展局部 `GscToolTipBehavior`：打开/主题变化同步当前作用域资源，父根卸载时关闭 Popup/Tooltip，解除显式 Tooltip 处理器；没有 Application 级静态订阅或全局资源污染。`bebde2fe` 只稳固了隔离夹具对 `Application.Current` 的复用。
- 实际生产 STA WPF `R05PopupLifecycleBehaviorTests 1/1` 验证 Light→Dark 后 Popup/Tooltip 均保持打开且颜色改变，父窗 `Hide()` 后两层关闭；最终标准产物相邻 R05 定向为 Tooltip `1/1`、Popup `2/2`、焦点 `3/3`、开关 `1/1`、选项虚拟化 `3/3`，共享源契约 `24/24`。
- clean RenderHarness 绑定 `bebde2fe`，双主题 357 PNG、工作树 clean、`render-qa OK`；Settings 1040×700 开面和 Light/Dark Popup/Tooltip 图已检查。完整并行 WPF 记录仍为实现提交上的 Playnite `573/665`、57 skip、35 fail，不能写成全量绿色。
- 不把隐藏夹具当成真实 Playnite 关闭/重建或内存泄漏证明；真实宿主、物理 DPI/跨屏、presented frame、OS 输入/IME、读屏/UIA、泄漏 profiler、ETW、宿主性能仍未验。下一项 R06-01 列宽用户记忆。

## 2026-09-17 R05-07 Tooltip 时序

- 先复核 Q15-03/Q15-07/Q15-08 与当前资源合并顺序；实际生产页面最后覆盖 Tooltip 的是 `AcrylicReferenceControls.xaml`，不能只看 `DesignTokens.xaml` 的 `MaxWidth=420`。本轮统一两套隐式样式的 `Focusable=False`、`Placement=Mouse`、420 DIP 上限和字符串换行/不省略模板。
- `2fc8c0d6` 新增局部 `GscToolTipBehavior`，仅挂在生产 Shell 与独立 Settings 根节点；按 `PlacementTarget` 找当前插件范围内的打开 Tooltip，Esc 关闭并将事件消费，焦点仍留在原输入控件，不进入瞬时 Popup。
- 实际生产 STA 行为 `R05TooltipTimingBehaviorTests 1/1`：Shell `InitialShowDelay=350`、`BetweenShowDelay=100`；合成长路径完整保留，Tooltip/TextBlock 宽度不超过 420 DIP 且实际换行；`Focusable=False`、`Placement=Mouse`、Esc 关闭、原 TextBox 焦点不变。回归源契约 `24/24`、Popup `2/2`、焦点 `3/3`、开关 `1/1`。
- clean RenderHarness 绑定完整提交，双主题 297 PNG、工作树 clean、`render-qa OK`；Settings Popup/Tooltip 开面探针、Settings/Overview 代表图已检查。标准脚本编译与 XAML 通过，Worker 仍有两条既有 `Healthy`/`Skipped` 对 `Warning` 的隔离环境失败，不能误写成全量通过。
- 不把显式打开的生产 Tooltip 样式实例当成真实 Playnite 悬停录像；鼠标快速经过、ShowDuration 消失、屏幕边缘遮挡/翻转、物理 DPI/跨屏、宿主字体、OS 输入/IME、读屏/UIA、presented frame、ETW 和宿主性能仍未验。下一项 R05-08 弹层资源热切换。

## 2026-09-17 R05-06 开关保存语义

- 先复核 R04-08 已有 `ISettings.EndEdit`、Worker live apply、`SettingsSaveFeedbackState` 和稳定 `SettingsSaveHintText`；当前生产字段没有仅重启生效项，因此本轮不另造“即时/重启”模型。
- `cf9a250f5020de056d070c785fc00f92a81cdc2a` 修复设置布尔自动属性在 `CopyFrom`/`CancelEdit`/导入时不通知 WPF 的真实缺口。所有布尔设置使用字段和去重 `SetBoolean`，值变化才发 `PropertyChanged`，让 ToggleSwitch、`IsEnabled` 依赖面板和保存提示跟随实际模型值。
- 真实 `GameSaveCenterSettingsView`/隔离目录/STA WPF 行为 `R05TogglePersistenceBehaviorTests 1/1` 覆盖外观开关的模型/显示/Track/脏提示/`ExportPortableJson` 快照和 `CancelEdit` 回滚，以及自动化页媒体来源与巡检依赖面板的关闭/恢复。回归：`SettingsSaveFeedbackTests 2/2`、`SettingsDraftLifecycleBehaviorTests 1/1`、`UiFinesseRound2ControlSourceTests 24/24`。
- 提交后 clean Release XAML `24/24`、构建 `0/0`、`git diff --check` 通过；RenderHarness 绑定完整 SHA、双主题、297 PNG、工作树 clean、`render-qa OK`，Settings 外观/自动化图已人工抽查。证据见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R05-06-TOGGLE-SAVE-20260917.md`。
- 真实 Playnite 本地写入异常、Worker 应用失败和宿主保存/取消/关闭时序仍未注入；离屏 logical DIP 不等价 presented frame、无闪屏、物理 DPI/跨屏或宿主性能。未写真实存档、媒体或云端。下一项 R05-07 Tooltip 时序。

## 2026-09-17 R05-05 复选框三态边界

- 先查实际消费点：生产媒体批量操作使用 Extended DataGrid、按模式媒体 ID 集合和 `SelectedItems`，不存在“当前页/全部结果”的批量 CheckBox。当前生产 CheckBox 仅是设置项/锁定等标量值，开发夹具的三态 CheckBox 不应升级为产品能力。
- Q10-02 的共享 `GscCheckBox`/`GscDataGridCheckBox` 半选标记已经由既有 Light/Dark 夹具记录为 `mark=visible`；这只覆盖模板视觉。R05-05 因没有真实复选框集合而记“不适用”，不新增全选模型；真实批量选择边界由 R05-04 记录。
- 文档提交后先用标准 Release 构建刷新程序集身份，再跑 `UiFinesseRound2ControlSourceTests 24/24`。不能把源码 HEAD 与旧程序集不一致造成的失败记成产品回归。
- 证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R05-05-CHECKBOX-THREESTATE-20260917.md`。下一项 R05-06 开关保存语义。

## 2026-09-17 R05-04 多选摘要

- `52900815c1fb8a16550446b9ee8d5318b8238a25` 核对后确认真实批量消费点是媒体收件箱 DataGrid，不是当前游戏媒体卡片；原有 `selectedInboxIdsByMode`、加载更多恢复和 `CaptureInboxMediaSelection` 已提供按 ID/去重/无效项能力。
- 生产 `MediaCenterView` 的摘要现在按总选择数和当前窗口作用域表达：空为 `未选择媒体 · Ctrl / Shift 多选`，普通选择为 `已选 N 项`，有保留但不可见 ID 时增加当前可操作数与暂不可见例外。新增“清空选择”只清当前模式选择集合，不改收件箱模式或媒体搜索/类型筛选。
- `R05MultiSelectionSummaryBehaviorTests 3/3`，R05-01/02/03 回归合计 `11/11`；clean Release XAML `24/24`、构建 `0/0`；clean RenderHarness 绑定完整 SHA、双主题 357 PNG、工作树 clean、`render-qa OK`，Media 双主题 `1040×700` 已抽查。
- 边界仍是隔离合成数据/STA WPF/offscreen logical DIP；跨窗口保留 ID 未替代真实后端分页删除竞态，未验真实 Playnite、OS 输入/IME、读屏/UIA、物理 DPI/跨屏、presented frame、ETW 或宿主性能；未写真实存档、媒体或云端。下一项 R05-05 先核对实际复选框消费点与 Q10-02 半选资源。

## 2026-09-17 R05-03 弹层边缘适配

- `cbfacd2064d6bb5400e3e203ec4f5f14493d44ad` 先用实际短窗探针复现固定 460 宽游戏选框越过壳层边界，再只在现有 `PickerPanel` 上增加 `MaxWidth`/`MaxHeight` 到 `PickerOverlay` Actual 尺寸的绑定。游戏选框不是 Popup；共享 ComboBox 原有 Popup、Auto 滚动和 `MaxDropDownHeight` 复用。
- 实际 STA WPF `R05PopupBoundaryBehaviorTests 2/2` 覆盖 720×360 短壳层和当前桌面右下 Popup：面板不越界、列表滚动/选定项可见、Popup 翻转/最大高度/滚动条通过。420×220 只有约 `92×92 DIP`，列表视口为 0，作为最小尺寸待定义边界，不伪报完整交互。
- clean Release XAML `24/24`、构建 `0/0`；clean artifact R05-03 `2/2`、R05-02 回归 `3/3`；RenderHarness 双主题 297 PNG、工作树 clean、`render-qa OK`。证据见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R05-03-POPUP-EDGE-20260917.md`。
- 真实 presented frame 无闪屏、Playnite 嵌入、多屏/物理 DPI、OS 输入/IME、读屏、UIA、ETW、宿主性能未验；未写真实存档、媒体或云端。下一项 R05-04 多选摘要。

## 2026-09-17 R05-02 选项虚拟化焦点

- `7a4ede84667d916ad61d382302b742cca8fd4704` 先复现实际缺口：生产 Shell ListBox 的键盘 Down 会被 `OnPickerSelectionChanged` 误认为选择提交并关闭弹层。修复只在选框预览键路由为方向键、PageUp/PageDown、Home/End 设置短暂保护；鼠标预览点击重置保护并保留旧的点击即提交路径。
- 生产 `PickerList` 原有 Auto 垂直滚动、Recycling 虚拟化、`GamePickerViewModel` 的隐藏选择保留/恢复命令/删除回退均复用，没有改 VM 或滚动系统。实际 STA WPF 2000 项行为证明活动项可见，过滤无结果仍可恢复，删除选中项回退首项。
- clean artifact 定向：R05-02 `3/3`、R05-01 `3/3`、既有 `GamePickerKeyboardBehaviorTests 6/6`；clean Release XAML `24/24`、构建 `0/0`；RenderHarness 双主题 297 PNG、工作树 clean、`render-qa OK`。证据见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R05-02-OPTION-VIRTUALIZATION-20260917.md`。
- 边界仍是隔离 STA WPF/offscreen logical DIP；真实 Playnite/OS 输入、IME、读屏、物理 DPI/跨屏、presented frame、ETW、宿主性能未验，未写真实存档、媒体或云端。下一项 R05-03 弹层边缘适配。

## 2026-09-17 R05-01 弹层焦点范围

- `0004999507d0cf27f483ea1234a7a60b4ad7f9b7` 先复现并修复生产 Shell 游戏选框的 Tab 越界：`PickerOverlay` 现在是独立 focus scope，Tab/Shift+Tab 循环且方向导航包含；打开即聚焦搜索框，关闭复用原有路径回焦上下文按钮。Dashboard `DialogOverlay` 同步声明本地循环并在关闭时恢复保存的打开触发器。
- 共享 ComboBox 只把 `ComboBoxItem` 从表单 Tab 停靠点移除，未替换原生选项导航、弹层滚动或 Escape 语义。`GamePickerViewModel`、命令/Binding、取消/错误、恢复保护、有限列表、Playnite/net462 未改。
- `R05FocusBoundaryBehaviorTests` 实际生产 Shell STA WPF `3/3`；clean Release XAML `24/24`、构建 `0/0`；clean RenderHarness 双主题 297 PNG、工作树 clean、`render-qa OK`。证据见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R05-01-FOCUS-BOUNDARY-20260917.md`。
- 本阶段范围：弹层焦点和共享 ComboBox 合成行为；Dashboard 真实宿主模态时序、OS 键盘/IME、屏幕阅读器、Playnite 嵌入、物理 DPI/跨屏、presented frame、ETW、宿主性能未验，离屏报告中的 `DpiScale=1.00` 仅 logical DIP。未写真实存档、媒体或云端。下一项 R05-02 选项虚拟化焦点。

## 2026-09-17 R04-08 保存反馈闭环

- `c735905aa1092f409bc7ddf9de1272f1d77dc084` 先核对 `ISettings.EndEdit`、`GameSaveCenterPlugin.ApplySettingsAsync` 和现有 `SettingsSaveHintText`，复用真实保存与 Worker live apply 链，没有把 main 旧实现覆盖到当前分支。
- `GameSaveCenterSettings` 以原子保存闸门拒绝重复 `EndEdit`，并区分保存开始、应用开始、应用完成和保存失败；本地写入成功后才清除编辑克隆。插件异步回调失败表示 Playnite 已持久化但 Worker 未应用，设置页显示对应失败态；本地写入失败保留草稿。
- `SettingsSaveFeedbackState` 只承载可控反馈状态；设置页的稳定提示面在保存/应用/失败/编辑/校验状态间切换。当前字段都支持 live apply，没有“需重启”字段，未来新增此类字段必须由真实设置契约驱动。
- 相关选择集 `20/20`，clean Release XAML `24/24`、构建 `0/0`、源码校验/diff check 通过；clean RenderHarness 双主题 297 PNG、工作树 clean、`render-qa OK`，Settings normal/dirty/invalid 已抽查。真实 Playnite 故障注入、宿主保存/取消/关闭时序、物理 DPI/跨屏、IME/剪贴板/读屏、presented frame、ETW、宿主性能未验；未写真实存档、媒体或云端。证据见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R04-08-SAVE-FEEDBACK-20260917.md`。下一项 R05-01 弹层焦点范围。

## 2026-09-17 R04-07 异步校验竞态

- `b1d5b68f` 先核对当前 `GameSaveCenterSettings.VerifySettings`、设置页 Dispatcher 合并和宿主生命周期；没有从 main 覆盖旧实现。完整 `VerifySettings` 保留原路径/数值安全规则，编辑期新增 `VerifySettingsWithoutPathAvailability` 只做轻量范围检查。
- `SettingsPathValidationSnapshot` 固定当前字段版本，`SettingsPathValidationService` 在后台复用 Worker/目录/环境变量/缺失叶目录/不可达卷/权限错误规则；`LatestAsyncValidationCoordinator` 递增版本、取消旧任务并抑制旧结果，DataContext/提交/回滚/Unloaded 都会失效旧结果。单次不可中断文件系统调用只保证结果不回写。
- R04-07 定向 `7/7`，覆盖慢 A 晚于 B 返回、取消后晚回调、真实隔离目录负例和源码接线；clean Release XAML `24/24`、构建 `0/0`、源码校验通过。`df6f8083` 只修复 RenderHarness 对 `RefreshValidationSummary` 的无参数反射兼容。
- clean RenderHarness 绑定完整 `df6f8083`，双主题、297 PNG、`WorkingTreeClean=True`、`render-qa OK`；人工抽查 Settings normal/dirty/invalid。证据范围为合成设置、隔离目录、TaskCompletionSource、STA WPF/offscreen logical DIP，真实 Playnite/IME/剪贴板/读屏/物理 DPI/跨屏/presented frame/ETW/宿主性能仍未验；未写真实存档、媒体或云端。下一项 R04-08 保存反馈闭环。

## 2026-09-17 R04-06 清空与撤销

- `0d3f71af27f00c34cbfc5d012f5f7dde39a4625c` 先核对现有 Shell/Dashboard/Trainer/Task/Media 搜索清空和游戏选框 Escape 路由，没有重建 VM 或危险命令。Dashboard 空状态清除按钮保留 `GamePicker.ClearSearchCommand`，补目标 TextBox Tag/Click，复用现有 `Clear`/`Focus`/`Keyboard.Focus` 并标记事件已处理。
- 普通编辑继续使用 WPF 默认 Undo/Redo 栈；实际 TextBox 测试走 `ApplicationCommands.SelectAll/Undo/Redo` 与标准粘贴事件，Undo 恢复原值和选区，Redo 恢复修改。游戏选框 Esc 只关闭并回焦上下文按钮。
- clean Release XAML `24/24`、构建 `0/0`、源码校验通过；R04-06 行为测试 `3/3`。clean RenderHarness 双主题 `297` PNG、工作树 clean、`render-qa OK`，Task/Shell `1040×700` 已抽查。
- 证据仍限于合成数据、隔离 STA WPF、offscreen logical DIP；真实 Playnite 系统输入/剪贴板/IME/读屏/物理 DPI/跨屏/presented frame/ETW/宿主性能未验，未写真实存档、媒体或云端。下一项 R04-07 异步校验竞态。

## 2026-09-17 R04-05 数字输入边界

- `8d56eb5a050a5c2db2718a42cf928b7d43d6f897` 在最新生产实现上复用 `GscNumericTextBox`、`IntegerRangeValidationRule` 和服务侧安全边界；没有从 main 覆盖旧实现，也没有虚构当前不存在的重试/端口/容量字段。
- `ValidateValue` 统一处理空白、非整数、`Int32` 溢出和上下界错误。Save 存档策略的间隔/保留模板与 Trainer 启动延迟接入同一规则，范围为 `1–1440`、`0–2147483647`、`0–300`。
- `NumericInput` attached behavior 用绑定规则验证滚轮当前值和 ±1 候选；非法当前值或越界候选保持文本/源值不变，不静默 clamp，并把未改值的越界事件留给页面滚动。普通键盘/粘贴仍通过原有 WPF binding validation。
- 相关定向测试 `20/20`，含真实 STA WPF 滚轮行为和溢出/非法输入负例；clean Release XAML `24/24`、构建 `0/0`、源码校验通过。clean RenderHarness 双主题 `297` PNG、工作树 clean、`render-qa OK`，Save `1040×700` 已抽查。
- 边界仍是合成数据/fake 服务/隔离目录/STA WPF/offscreen logical DIP；真实 Playnite 系统输入、IME、读屏、物理 DPI/跨屏、presented frame、ETW、宿主性能未验，也未写真实存档、媒体或云端。下一项 R04-06 清空与撤销。

## 2026-09-17 R04-04 粘贴标准化

- `d7e92f1467648f045ff94fb30da5ac9712cd402b` 先盘点最新生产输入：设置页只有本地路径/云端目标，MediaCenter 有自定义媒体目录/文件模式；当前没有可编辑端口或独立 Exclude 字段，因此不扩展不存在的 DTO/业务模型。
- `PasteNormalization` 是共享 WPF attached behavior，输入类型为 `Path`、`RemoteTarget`、`Port`、`ExcludePattern`。统一规则只处理外层空白、成对单/双引号和单值尾随 shell 换行；剩余换行表示多值粘贴，直接拒绝并保留字段/剪贴板。Port 类型仅保留未来真实字段接入点。
- 标准化以 `TextBox.SelectedText` 写入，让 WPF 维护正常 Undo；本次 raw paste 和结果说明只保存在 TextBox attached state，不落盘、不进 Worker、不改剪贴板。Tooltip/Automation HelpText 告知标准化变化和 Ctrl+Z 恢复语义。
- `PasteNormalizationTests` 6 个、生产 XAML source/既有 settings source 2 个合计 `8/8`；Release XAML `24/24`、构建 `0/0`、`validate-source.py` 通过。clean RenderHarness `d7e92f1` 双主题、297 PNG、`WorkingTreeClean=True`、`render-qa OK`，人工抽查 Settings/Media。
- 证据只代表合成数据、隔离目录、STA WPF 和 offscreen logical DIP；真实 Playnite 剪贴板/输入链、IME、读屏、物理 DPI/跨屏、presented frame、ETW、宿主性能和未来真实端口/Exclude 字段仍未验。未写真实存档、媒体、云端。下一项 R04-05 数字输入边界。

## 2026-09-17 R04-03 未保存离开保护

- `58e1734f0bf84b82d0fd95077327122e9b99cbf3` 复用 `GameSaveCenterSettings` 的 Playnite `ISettings` 编辑克隆和 `CreateSettingsFingerprint`；`HasPendingEdit` 与 `GetEditBaselineFingerprint` 让设置视图重建后仍以 Playnite 捕获的编辑基线判断 dirty。`CancelEdit` 恢复一次并清空克隆，`EndEdit` 成功提交后清空克隆，失败不丢草稿。
- `GameSaveCenterSettingsView` 在 Loaded 挂接宿主 Window Closing，在 Unloaded 解除；有有效脏草稿时复用原生消息框，“是”显式放弃并关闭，“否”取消关闭、保留值并恢复原分类/字段焦点；确认异常时 fail-safe 保留窗口和草稿。临时分离/重挂使用同一编辑对象，并用 `BringIntoView`/`Keyboard.Focus` 恢复。
- `SettingsDraftLifecycleBehaviorTests` 实际 STA WPF `1/1`，与 `PortableSettingsTests`、`SettingsValidationSourceTests` 合计 `12/12`；clean Release XAML `24/24`、构建 `0/0`、源码校验通过。RenderHarness 完整 SHA `58e1734f0bf84b82d0fd95077327122e9b99cbf3`、`WorkingTreeClean=True`、357 PNG、双主题/多尺寸/设置三态/滚动/虚拟化/Shell/resize `render-qa OK`。
- 边界：没有自动点击真实 Playnite 原生关闭确认和 UIA，真实宿主保存/取消/关闭时序、物理 DPI/跨屏、读屏、presented frame、ETW、宿主性能仍未验；测试使用合成设置/fake Worker/隔离目录，未写真实存档、媒体或云端。下一项 R04-04 粘贴标准化。

## 2026-09-17 R04-02 错误摘要导航

- `6524f94` 复用现有设置 VerifySettings、字段校验模板、页头摘要/详情和分类/滚动系统；设置页登记路径、备份、外观和自动化字段目标，把可识别错误变成详情 Hyperlink。点击后先选分类，再对字段 `BringIntoView` 并聚焦；字段 HelpText、链接 Automation Name/HelpText 保留具体原因，命令/Binding 不变。
- `HealthInspectionEnabled=false` 时，恢复巡检间隔和重新验证有效期的无效值不阻止模型保存校验；启用状态下仍执行原范围检查。`SettingsValidationNavigationBehaviorTests` 使用隔离 Worker/目录和真实 STA Window 验证链接跨 Tab、焦点、滚动、自动化原因；设置路径/源校验及禁用字段负例合计 `5/5`。
- clean commit 隔离 Release XAML `24/24`、构建 `0/0`、源码校验通过。RenderHarness 绑定完整 `6524f944f05921f02f0b32b7f5aa3dfa702ac165`，`WorkingTreeClean=True`、双主题、多尺寸和既有滚动/虚拟化/Shell/resize `render-qa OK`、357 PNG；人工抽查 Light/Dark 设置页。探针只作为离屏布局/链接/HelpText 证据，字段真实焦点/滚动以 STA Window 为准。
- 同代码全量 Playnite testhost 两次复跑分别为 `553/628` 通过、18 失败、57 跳过，以及 `548/628` 通过、23 失败、57 跳过；失败为既有 PresentationSource/Visual 上级/缩略图/动效环境性问题，R04-02 定向全通过，未放宽门禁。真实屏幕阅读器、Playnite 嵌入、物理 DPI/跨屏、OS 输入、ETW、宿主性能仍未验；下一项 R04-03 未保存离开保护。

## 2026-09-17 R04-01 组合输入状态

- `40c7f9a` 复核并复用 R00-08 的可见生产 picker 行为：无结果 Enter、`ImeProcessed`、方向键、Escape 焦点回返和有效 `ItemsView` 候选 Enter 已存在；本阶段只在 `AcrylicProductionShellView.GameSearchTextBox` 上补 WPF `TextComposition` start/update/preview+bubble commit 状态，未改 picker/滚动条/设计体系。
- 组合期间阻止选择变化进入 `SelectedGame`/关闭流程，并让 Enter 留给 IME；commit 后恢复候选确认、弹层关闭和焦点回返。卸载清除组合状态。`GamePickerKeyboardBehaviorTests` 实际 `4/4`，覆盖 start/update + Enter 负例、commit + Enter 正例及原有键盘回归；英文即时搜索继续使用本地缓存和已有测试。
- clean Release 全量：XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `569/626`（57 skip/0 fail），源码校验通过。初次未提交工作树出现 2 条一次性失败，复跑与 clean commit 全量均 0 失败，未改写为通过或放宽门禁。
- RenderHarness clean `40c7f9a` 双主题、多尺寸、滚动/虚拟化和 Shell/resize `render-qa OK`，357 PNG，`WorkingTreeClean=True`；人工抽查 Task/Shell。边界：合成 DTO、隔离 WPF/offscreen logical DIP，不等价真实 Windows IME 候选窗口、物理键盘/候选翻页时序、Playnite 嵌入输入、读屏、物理 DPI/跨屏、presented frame、ETW 或宿主性能；R18-01 继续连续输入/IME/debounce 时延与分配验证。下一项 R04-02 错误摘要导航。

## 2026-09-17 R03-08 用户文本缩放

- `981b600` 复用现有 DynamicResource 字体和尺寸 token；共享文本输入、文本按钮、只读路径框不再用硬 `Height`，表头不再用 `ColumnHeaderHeight`，只保留最小节奏和既有模板。表头文字模板设为 `Wrap + Trimming=None`，不通过缩小字号适配。
- `R03TextScaleTests` `2/2` 在真实生产资源链 STA WPF 中将正文/说明字号覆盖为 `24/20 DIP`，实测输入框、按钮、DataGrid 表头自然增长且内容宿主不裁切；源代码门禁覆盖相关样式区段，合法图标按钮固定尺寸不误报。既有搜索/共享样式断言按原责任校正为最小高度、居中和内容可见性。
- 隔离 Release XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `568/625`（57 skip/0 fail），`validate-source.py` 通过。RenderHarness clean `981b600` 双主题 `render-qa OK`、357 PNG，人工抽查 Task/Media/Save。
- 边界：RenderHarness 默认字号，不等价真实用户文本缩放、自选/宿主字体或 Playnite presented frame；真实物理 DPI/跨屏、OS 输入/IME、读屏、ETW、宿主性能仍未验。未改 picker、滚动条、命令/Binding、取消错误、恢复保护、有限列表和 net462；未写真实存档、媒体、云端。下一项 R04-01 组合输入状态。

## 2026-09-17 R03-07 文案标点统一

- `5a07eda` 只收口了已确认的生产可见不一致：Task 状态/类型/范围/时间标签由半角冒号改为全角冒号；失败 `DetailMessage` 和整库失败聚合改为全角字段分隔。没有重建文案服务或设计体系。
- 失败展示格式化只作用于外层分隔符：`ErrorCode`、`ErrorMessage` 属性不变；错误码为空不输出孤立 `错误码：`；含 `C:\Saves\A:1` 的正文原样保留。`CopyPathCommand`、路径预览和 R03-05 的原始复制语义未改。
- 术语表：中文字段标签用 `：`；生成详情字段间用 `；`；中文上下文括号用 `（…）`；产品名/缩写保留 `Worker`、`Playnite`、`FLiNG Trainer`、`任务 ID`、`EXE / CT` 的现有大小写和语义空格；单位保持 `1 KiB`、`1 秒`、`2 项`、`12%`。
- `R03CopySafePunctuationTests` `3/3`，全量隔离 Release XAML `24/24`、Core `83/83`、Worker `311/311`、Playnite `566/623`（57 skip/0 fail）。RenderHarness clean `5a07eda` 双主题 `render-qa OK`，357 PNG；证据限于合成 DTO、STA WPF/offscreen logical DIP，真实宿主 presented frame、字体/物理 DPI、OS 输入/IME、读屏、ETW、性能和剪贴板仍未验。下一项 R03-08 用户文本缩放。

## 2026-09-17 R03-06 双语长度压力

- `52527b6` 复用现有标题省略/Tooltip 和长文本样式，修复共享 `GscWpfUiButtonTextTemplate`：动作文字在窄槽允许换行且不做省略，避免“打开/保存”等核心命令只显示歧义前缀；Tooltip 仍以完整绑定值为内容。
- `BilingualLengthStress` 只属于 RenderHarness 合成数据：中文长游戏名贯穿当前游戏/任务/活动/关注项，165 字符英文句进入任务详情等文本位。实际 STA WPF `R03BilingualLengthTests` 为 `2/2`，154 DIP 槽中英文动作的 `TextWrapping/Trimming/Tooltip/高度` 均按预期。
- clean commit 的双主题 `overviewedges` 在 820/1040/1600 DIP 全部 `OK`：6/6 首页表面、标题可省略且完整 Tooltip、英文长句可见、当前游戏 2 个动作可测、无横向溢出；统一 `render-qa` `WorkingTreeClean=True`、297 PNG、`render-qa OK`。全量 Release XAML `24/24`、Core `83/83`、Playnite `563/620`（57 skip/0 fail），Worker 未改且同分支最近完整基线为 `311/311`。
- 只涉及共享按钮显示模板、合成夹具和测试；未改命令/Binding、取消错误、安全恢复、游戏选框、滚动条、有限列表、net462 业务契约。证据不等价真实 Playnite presented frame、宿主字体/物理 DPI/跨屏、OS 输入/IME、读屏、ETW 或宿主性能；未写真实数据。下一项 R03-07 文案标点统一。

## 2026-09-17 R03-05 长路径分层

- `2b8612f` 复用现有 `GscPathText`/Tooltip 和 `CopyTextWithRetryAsync`，没有重建服务或复制流程。新增 `PathDisplayConverter`，预览中间省略并保留盘符/根路径开头与文件名尾部；只在显示层去除 CR/LF，原始路径绑定值不变。
- `GscWpfUiPathDetailTextBox` 是共享只读详情框：`NoWrap`、隐藏内部横向滚动、原生选择/Ctrl+C、完整值 Tooltip/Automation HelpText。`CopyPathCommand` 直接复制 CommandParameter 原始路径，不把省略号写入剪贴板；空白参数不可执行。
- Save 候选、Media 待归类/已归档媒体、Trainer 选中版本的真实生产详情均提供预览、可选择完整值和“复制完整路径”。`R03LongPathTests` `3/3` 实际验证超过 260 字符、340 DIP 有限宿主、`SelectAll().SelectedText` 精确相等；最终隔离 Release 为 XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `561/618`（57 skip/0 fail），源码校验通过。
- 最终 RenderHarness 绑定 `2b8612f`、`WorkingTreeClean=True`，Light/Dark 多尺寸 `render-qa OK`，297 张 PNG；人工查看 Save/Media/Trainer 1366×768 详情。首轮直接测试缺少 `GSC_BUILD_COMMIT`，按仓库身份协议重跑；全量唯一失败是更新 Trainer 绑定后过期的既有源码断言，已校准后通过。
- 未验真实 Playnite presented frame、OS 真实剪贴板/输入/IME、读屏、宿主字体替换、物理 DPI/跨屏、ETW 或宿主性能；Maintenance 健康/镜像/诊断摘要路径仍是 Tooltip/只读摘要，不能写成独立复制详情。`.tmp/r03-05-*` 文档提交后清理。下一项为 R03-06 双语长度压力。

## 2026-09-17 R03-04 数字列对齐

- `9fa68ef` 先复用已有 Tabular 数字能力：新增 `GscTypographyNumericCell`/`TimeCell`/`PercentCell`，统一右对齐、NoWrap 和数字不裁切；Save、Media、Maintenance、Task 只替换对应单元格样式，未重建设计体系或改命令/Binding。
- `TaskStatusDto` 保留原始 `ProgressPercent`，新增 `ProgressValue`（0～100 显示钳制）和 `ProgressDisplay`（排队默认 0、负数为 `—`，其余为百分比）。这只影响呈现，不把未知/无效值写回数据，也不改变非空计数/容量的零值含义。
- `R03NumericAlignmentTests` 最终 `10/10`，其中实际 STA WPF 生产资源 120 DIP 列对 `9/10/99/100` 的右边界均为 `119.5～120.5 DIP`；8 个 DTO 边界覆盖未知、负数、0、9、10、99、100、120。隔离 Release 为 XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `558/615`（57 skip/0 fail），源码校验通过。
- RenderHarness clean commit 的 Light/Dark 生产 1040×700 页面 `render-qa OK`，人工抽查 Task/Save/Media。第一次全量的两个夹具/既有断言问题已修正后重跑，不改写 skip 或放宽门禁；`.tmp/r03-04-*` 为可再生验证输出，文档提交后清理。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R03-04-NUMERIC-ALIGNMENT-20260917.md`。
- 证据只代表合成数据、受控 WPF 和 offscreen logical DIP；未验真实 Playnite presented frame、宿主字体替换、物理 DPI/跨屏、OS 输入/IME、读屏、ETW 或宿主性能。下一项为 R03-05 长路径分层。

## 2026-09-17 R03-03 双语混排基线

- `466c2f5` 先查明当前生产 Save Center/存档中心标题、日期容量和中文标点都复用现有字体/数字资源；没有生产布局偏移或新设计体系需要引入，因此只补证据，不改生产 XAML。
- `TypographyDiagnostics.CaptureMixedBaseline` 复用 `UiFontChain`，以 WPF `TextFormatter.GetIndexedGlyphRuns()` 读取实际混排 run，记录 line baseline、GlyphRun baseline spread、glyph 数和未配对 surrogate。四组样本的 `runs/glyphs` 为 `3/16、9/24、3/22、6/23`，Light/Dark 均 `spread=0`、`stable=True`；行为测试 `1/1`，Typography 类 `11/11`。
- 最终隔离 Release 为 XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `548/605`（57 skip/0 fail），源码校验通过；RenderHarness Light/Dark `WorkingTreeClean=True` 且 `finesse-fixture OK`，生产 1040×700 双主题 `render-qa OK`，人工抽查 Overview/Media。
- 只使用合成文本、受控 WPF 和 offscreen logical DIP，未写真实存档/媒体/云端，未改 picker/滚动/命令/Binding/取消错误/恢复保护/net462。真实 Playnite presented frame、宿主字体替换、物理 DPI/跨屏、OS 输入/IME、读屏、ETW 和宿主性能仍未验。下一项为 R03-04 数字列对齐。

## 2026-09-17 R03-02 阅读层级校准

- `e889b04` 先盘点实际生产 Views/Settings 的文字入口，复用 `Typography.xaml` 的标题、正文、Caption、Code/Path 层级；删除 5 个 12.5、9 个 10.5、2 个 9.5 字面微变体。壳层品牌、媒体文件名、Overview 任务/活动/问题主标题使用 `GscBodyFontSize=14`；版本、状态、时间、详情和比较质量使用 `GscCaptionFontSize=12`。
- 技术路径仍走 `GscTypographyCode`/`GscPathText`，保留 Tooltip、复制、单行省略；未改游戏选框、滚动条、命令/Binding、取消/错误、安全恢复或 net462 契约。直接扫描生产 Views/Settings 未发现 TextBlock 局部低透明度，现存 0.22/0.9 仅为 ambient 装饰层。
- `ProductionTypographyUsesSharedHierarchyWithoutMicroSizeDrift` 最终 clean 定向 `10/10`；隔离 Release XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `547/604`（57 skip/0 fail），源码校验通过。RenderHarness Light/Dark 1040×700 均 `render-qa OK`，人工查看生产 Overview、Media、Save、Maintenance 窄窗代表图。
- 证据是合成数据、生产 WPF 资源和 offscreen logical DIP；未启动真实 Playnite、未写真实存档/媒体/云端。真实 presented frame、宿主字体替换、物理 DPI/跨屏、OS 输入/IME、读屏、ETW 和宿主性能仍未验。下一项为 R03-03 双语基线。

## 2026-09-17 R03-01 真实落字证据

- `855e727` 先复用既有 `UiFontFamily`/`FindCandidate`，新增 `TypographyDiagnostics.CaptureGlyphRun`：在 STA 中通过 WPF `TextFormatter.GetIndexedGlyphRuns()` 取实际排版运行，记录最终 `GlyphTypeface`，不把候选 `CharacterToGlyphMap` 当作最终命中。
- 证据等级有明确分层：捕获到目标码点且最终 Typeface 映射非零、无 `.notdef` 才是 `GlyphRunCaptured`；无法获取为 `CandidateOnly`/`Unknown`，`.notdef` 或最终码点不可映射为 `GlyphRunNotdefOrUnresolved`。当前 clean Light/Dark 报告中中文、英文、数字、`𠮷`、组合重音均为 `GlyphRunCaptured`；`𠮷` 候选 unresolved 但最终为 `MingLiU-ExtB`。
- 新增实际 STA 行为测试 `GlyphRunProbeSeparatesCandidateCoverageFromFinalLayoutEvidence` `1/1`；`TypographyDiagnosticsTests` 类 `9/9`。最终隔离 Release XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `546/603`（57 skip/0 fail），源码校验通过；RenderHarness Light/Dark `WorkingTreeClean=True`、`finesse-fixture OK`，人工查看 `1120×980` 双主题夹具图。
- 本项没有改生产 XAML、字体链、命令/Binding、picker、滚动、取消/错误、恢复保护或业务数据路径。证据是受控 WPF 排版与 offscreen logical DIP，不等价真实 Playnite 最终 frame、宿主字体替换、物理 DPI/IME/读屏/ETW/宿主性能；下一项为 R03-02 阅读层级校准。

## 2026-09-17 R02-08 动作文案动词化

- `43ea843` 先核对命令 Binding 后只修正文案：LoadDetails 入口统一“重新加载详情”，Validate 入口统一“重新校验”，错误状态保留“重试”；远端恢复明确分成“下载到隔离区并校验”和“快照并恢复”，云端动作明确区分“校验远端内容”和“重试上传”。
- 同步修改了 CloudTransfer DTO、Playnite 提示、Worker 失败消息和源码守卫；生产可见信息不再使用英文 `远端 check`/`只读 check`，协议 `Check*` 状态名和 rclone `check` 参数保留。没有改命令、Binding、取消/错误、安全或数据流程。
- `R02ActionCopyTests` 4/4，相关定向合计 13/13；干净 Release 为 XAML 24/24、构建 0/0、Core 83、Worker 311、Playnite 545/602（57 skip/0 fail）；源码校验通过。RenderHarness Light/Dark `render-qa OK`，人工查看 Save/CloudQueue 双主题 1040×700 代表图。
- 证据是生产源码、合成状态和隔离 WPF offscreen logical DIP；未启动真实 Playnite 或云端/备份操作，真实宿主字体、物理 DPI、OS 输入/IME、读屏、presented frame、ETW 和宿主性能仍未验。下一项为 R03-01 真实落字证据。

## 2026-09-17 R02-07 异步菜单上下文

- 菜单入口仍是 Playnite `GameMenuItem`，不存在可由本项目控制的 WPF `ContextMenu/MenuItem`。`4d4bb93` 新增 `GameMenuActionContext`：生成菜单时固定目标 Guid，异步 Action 执行前按当前数据库重新解析，保留原顺序，不追随可变旧引用或当前选中行。
- 五个目标相关 Action 都在 `EnsureWorkerAsync`/`UpsertGames` 之前做解析；列表刷新正例解析到替换后的 `Game` 实例，删除负例返回缺失 ID、空结果并中止。失效/无法确认时复用既有提示和日志，不写数据。
- `R02MenuActionContextTests` + `R02MenuHostContractTests` 定向 `4/4`；最终干净 Release 为 XAML `24/24`、构建 `0/0`、Core `83`、Worker `311`、Playnite `541/598`（57 skip/0 fail）；源码校验通过。首轮过期字符串断言已单独校准并纳入干净提交。
- 未启动真实 Playnite、未执行菜单 Action、未写真实存档/媒体/云端；宿主实际打开后刷新/删除时序、菜单视觉和 OS 输入/IME/物理 DPI/读屏/presented frame/ETW/性能仍未验。下一项为 R02-08 动作文案动词化。

## 2026-09-17 R02-06 菜单状态完整（外部宿主阻塞）

- 先查当前代码：没有 WPF `ContextMenu/MenuItem`；插件只实现 `GetGameMenuItems`，空选择返回空，有选择时按固定顺序生成六个 `GameSaveCenter` 宿主动作。不能凭空加一套本地菜单来替代 Playnite 的渲染职责。
- `8e48ac8` 新增 `R02MenuHostContractTests` 两条实际插件契约测试，合成多选验证六项描述、分组和 Action 非空，空选择验证零项；完全不执行 Action，所以没有 Worker/真实存档/媒体/云端副作用。
- 完整隔离 Release 为 XAML `24/24`、构建 `0/0`、Core `83`、Worker `311`、Playnite `539/596`（57 skip/0 fail）；源码校验通过。R02-06 的 WPF 菜单状态、键盘导航、快捷键列和宿主定位/关闭属于外部 Playnite，记录为外部阻塞而不是通过。
- 未启动真实 Playnite 或写真实数据；真实宿主菜单、OS 输入、屏幕阅读器、物理 DPI/IME、presented frame、ETW、宿主性能未验。下一项为 R02-07 异步菜单上下文。

## 2026-09-17 R02-05 命中区与间距

- 先核对当前生产实现：`GscIconOnlyButtonBase` 已继承共享按钮模板，实际尺寸 `34×34 DIP`、右侧逻辑间隔 `6 DIP`；`GscIconOnlyToolbarButton` 为 `36×36 DIP`，文字动作复用 `GscButtonHeight=36` 与 `GscCompactButtonHeight=30`。Task/Save 的复制、重试、取消、删除和 Media 批量栏均已有对应入口，因此没有重建服务、DTO 或页面布局。
- `f03b4dd` 新增 `R02HitAreaSpacingTests` 两条实际 STA WPF 行为门禁：复制/删除中心命中各自 Button，6 DIP 间隔不落入任一按钮；窄 `82 DIP` WrapPanel 保持复制/删除同行，第三按钮换行，三个命中矩形无正面积重叠且中心身份正确。
- 完整隔离 Release 为 XAML `24/24`、构建 `0/0`、Core `83`、Worker `311`、Playnite `537/594`（57 skip/0 fail）；RenderHarness 当前 commit 工作树干净，Light/Dark 56 场景 `OK`；源码校验通过。没有修改游戏选框、滚动条、命令/绑定、取消/错误、恢复保护和 net462 契约。
- 证据范围是合成图标、真实生产 WPF 资源、隔离 Window、VisualTreeHelper 命中和 offscreen logical DIP；未验真实 Playnite、物理 DPI/OS 输入/IME/屏幕阅读器/presented frame/ETW/宿主性能，也没有真实数据写入。下一项为 R02-06 菜单状态完整。

## 2026-09-17 R02-04 图文光学居中

- 先核对生产实现：`WpfUiProduction.xaml` 已有共享文字 `DataTemplate`、Center 对齐和 bounded ellipsis；`Redesign.xaml` 已有 Header Visual 的 `ContentTemplate={x:Null}`，所以本项无需生产代码重建。
- `d296ce0` 新增真实 STA WPF 几何测试：同一共享文字样式的中文四字/英文双词基线差 `<0.5 DIP`；16/20 DIP 图标复合内容与数字间距 `8 DIP`、中心差 `≤1.5 DIP`，定向 `3/3`。
- 最终完整隔离 Release 为 XAML `24/24`、构建 `0/0`、Core `83`、Worker `311`、Playnite `535/592`（57 skip/0 fail）；首跑既有 IPC 取消时序用例失败，单项 `1/1` 复跑、随后完整脚本通过。RenderHarness 双主题 56 场景通过，源码校验通过。
- 范围是生产资源、合成 Geometry、offscreen logical DIP；不宣称真实 Playnite/物理 DPI/屏幕阅读器/OS 输入/IME/presented frame/ETW/宿主性能。下一项为 R02-05 命中区与间距。

## 2026-09-16 R02-03 禁用原因可达

- `627f864` 先复用已有命令门禁和 `OpenMaintenanceCommand`，通过 `ActionAvailabilityHints` 把 Restore、Media Inbox、Cloud Transfer、Remote Restore 的第一阻塞条件显示在相邻位置；没有把危险恢复、未校验远端恢复或不可重试云状态放行。
- `GscActionAvailabilityHintText` 是共享生产样式，说明本身可聚焦并设置 Automation Name/HelpText；Redesign 只复用既有诊断气泡几何。Save 无选中状态、Media 批量/检查器、Maintenance 云/远端检查器均有可达说明。
- 真实 STA WPF 定向 `4/4` 覆盖正向、忙态和负例；最终隔离 Release 为 XAML `24/24`、构建 `0/0`、Core `83`、Worker `311`、Playnite `532/589`（57 skip/0 fail）；双主题 RenderHarness 56 场景通过。
- 证据边界仍是合成状态/fake 服务、offscreen logical DIP 和生产资源链；不宣称真实 Playnite/屏幕阅读器/物理 DPI/OS 输入/IME/presented frame/ETW/宿主性能。下一项为 R02-04 图文光学居中。

## 2026-09-16 R02-02 忙碌宽度稳定

- 前序 `4947539` 已有 `Button.IsBusy`、共享 indeterminate `BusyIndicatorHost` 和旧 Dashboard 三个顶栏绑定；当前实际 Acrylic 壳层/工作区按钮未统一接状态。`473cf3a` 在共享 `GscWpfUiButton` 基元增加最近 `UserControl.DataContext.IsBusy` 绑定，派生 Primary/Action/Context/IconOnly/Redesign header 角色自动覆盖，设置/校对夹具无 `IsBusy` 时保持 false。
- `BusyOperationCoordinator` 接入 `DashboardViewModel.RunAsync`，用 `Interlocked` 单槽拒绝并发进入；异常/取消统一走原有错误/取消语义，`finally` 复位并继续原 Trainer 清理和排队刷新。R02 定向 `3/3` 不只是源码字符串：一个测试实际执行协调器的重复/失败/取消负例，另一个在真实生产 WPF 资源链中验证宽度、文本、焦点和不定进度层。
- 最终隔离 Release：XAML `24/24`，构建 `0/0`，Core `83/83`，Worker `311/311`，Playnite `528` 通过/`57` 跳过/`0` 失败；Light/Dark buttonbusyprobe 均 `180→180`、indicator visible/indeterminate/content stable。未启动真实 Playnite 或 Worker 业务，不写真实存档/媒体/云端；真实命令时序、物理 DPI、OS 输入、presented frame、ETW、宿主性能仍未验。下一项 R02-03。

## 2026-09-16 R02-01 动作优先级

- 当前生产按钮系统已有 Primary/Context/IconOnlyDanger 能力；`89c9cc3` 只补共享 `GscWpfUiDangerActionButton`、`GscWpfUiContextDangerButton` 两个角色变体，避免各页重复造危险样式。生产页面的高影响动作应使用 Danger 语义，普通区域维持一个主动作，其余用次要/上下文动作。
- Overview 的 HomeToolbar、CurrentGameCard、MediaInboxBatchActionRow 均由结构测试确认一个 Primary。Media Inbox 的 Apply/Restore 批量主动作按 `MediaInboxMode` 互斥，两个 Restore 入口都默认隐藏、已忽略时显示；不要以静态出现两个按钮升级为多个同时高亮。
- Save Restore 的真实 `RestoreCommand`、Media 删除来源的 `DeleteMediaSourceCommand` 与既有策略删除绑定未改变；游戏选框、滚动条、取消/错误、恢复保护、有限列表性能和 net462 兼容继续是不可回归契约。
- R02-01 定向 `2/2`、完整 Release Core `83`、Worker `311`、Playnite `525/582`（57 skip/0 fail）、XAML `24/24`、RenderHarness `161/0/0` 通过。离屏 logical DIP 证据不代表真实 Playnite、物理 DPI、IME、presented frame、ETW 或宿主性能。下一项 R02-02。

## 2026-09-16 R01-08 跳过测试说明

- 当前隔离 Release 全流程实际结果：Core 83 通过/0 失败/0 跳过，Worker 311/0/0，Playnite 523/0/57（总计 580），合计 917/0/57；XAML 24/24、构建 0 warning/0 error。
- 57 条 Playnite skip 全来自 LegacyProductionUiBaselineFact，分布为 WpfUiResourceDictionary 39、UiLayoutRegression 11、OvernightClosureV6 3、SettingsAndAutoSelect 2、OvernightV4 1、WorkspaceStateSource 1；这是已撤销 UI 架构的历史基线，不应强行改为通过。
- 另有 6 条 NamedPipeFact 与 1 条 WorkerProcessFact 受环境能力 gated，分别覆盖 IPC/调用方取消/宿主关闭/重放恢复/大写入 ambiguous，以及 Worker 硬重启 durable task 恢复。本机 Named Pipe 可用，7 条实际通过；若换机能力不可用，应保留 skip 并记录补测步骤。
- 任务表的 63 项估算对应 Playnite 57 legacy + 6 IPC；Worker gated 单独计入其项目。本阶段不启动真实 Playnite、不写真实数据、不宣称物理 DPI/OS 输入/IME/presented frame/ETW/宿主性能。下一项 R02-01。

## 2026-09-16 R01-07 基线失效规则

- e1324fe 新增 check-ui-evidence-freshness.ps1，从 UI_EVIDENCE_BASELINE.json 读取每条证据的 sourceCommit、sourcePaths、scopes、runtimeKind 和 packageCommit；按 Git diff 命中关联路径后输出 fresh/stale、需要重跑范围、包状态和是否需要重装。配套 test-ui-evidence-freshness.ps1 不是 Assert.Contains 字符串门禁，而是实际运行三种路径/身份场景。
- 当前源码提交 e1324fee0e60b7369aeceeaf11eddf606bd094f8 扫描 14 条记录：7 stale（R00-01-02、R00-04、R00-06、R00-07、R00-08、R01-01、R01-02），7 fresh（R00-03、R00-05、R01-03～R01-07）。这是“旧证据关联源码变了需重跑”，不等于产品缺陷。
- smoke 事实：docs-only 变更 0 rerun/0 reinstall；Redesign.xaml 共享资源变更命中 R00-01-02/R00-05 shared-controls/all-pages 且不命中无关 Media；合成包基线 mismatch 得到 packageStatus=mismatch/reinstallRequired=True，并保持 source/package 身份独立。当前真实 package 身份为 not-provided，没有安装或宿主写入。
- python scripts/validate-source.py 和 freshness smoke 通过；证据见 docs/design/reviews/ui-finesse-round3-20260915/evidence/R01-07-EVIDENCE-FRESHNESS-20260916.md。下一项 R01-08 跳过测试说明。

## 2026-09-16 R01-06 宿主证据保全

- R01-06 已满足：在不触碰 artifacts/ 旧用户产物的前提下，用隔离 .tmp/r01-06-audit 运行当前 RenderHarness 审计，绑定 3929ed73e1056a964d7ceacc54a6046abcc9983e，得到 161 个运行时快照、0 Fidelity、0 失败路由、0 HIGH/0 MEDIUM，以及 73 条已分类 INFO。
- 关键 manifest、summary、metadata、route/fidelity matrix、layout report、20 行具体索引和 6 张精选图已提交到 docs/design/reviews/ui-finesse-round3-20260915/evidence/R01-06-host-evidence-20260916/。metadata 中输出根/ZIP 已便携化为相对路径；完整 353 张图、视觉树 JSON 和大体积机器 manifest 不入 Git，由 README 的固定 checkout 命令重现。
- 标准尺寸壳层、首页和维护诊断图已人工检查；图片只作为受控 WPF 离屏暗色样本。未执行真实业务写入，不宣称 Playnite 嵌入、物理 DPI、OS 输入/IME、presented frame、ETW 或宿主性能。下一项 R01-07 基线失效规则。

## 2026-09-16 R01-05 负例注册表

- `5e6d64a` 新增测试侧 `UiNegativeFixtureRegistryTests`，五项注册表分别调用对比度、数值可读性、生产选框键盘路由、子级布局溢出和 Loading 状态命中检测；N01～N05 全部 `detected=True`，每项都记录明确 expected-failure，不把负例夹具放进生产入口。
- 实际 probe 结果：N01 `violations=1`；N02 长负数窄列 `horizontalFit=False/verticalFit=True/isReadable=False`；N03 `handled=False/overlay=Visible/selectionPreserved=True`；N04 `CHILD_LAYOUT_OVERFLOW.json` 存在；N05 `retry=Collapsed/underlyingHit=False`。N02/N03/N05 是受控真实 WPF Window/视觉树状态采样。
- 绑定提交 `5e6d64aea3a64bc3fcde85627a84ce7cff5759e2` 的 Release 全流程为 XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `523/580`（57 skip/0 fail），注册表 `1/1`，源码校验通过；代码已推送。
- 临时 gate/构建输出在证据同步后清理；未执行真实存档、媒体、云端或诊断外发，未改 picker/滚动/命令绑定/取消错误/恢复保护/有限列表/net462。下一项 R01-06。

## 2026-09-16 R01-04 动效行为替代字符串

## 2026-09-16 R01-04 动效行为替代字符串

- `74abb10` 延续已有 `GscMotion.AnimateEntrance` 生产实现，新增实际 STA WPF 负向敏感行为测试：入场中途采样有效 Y/Opacity，重入后清除最新时钟，必须回到采样基值附近；这覆盖了删除重入分支基值写回时会出现的真实状态回归。
- 原动效源码测试已改为结构用途，只检查方法/活动分支/清钟/动画/终态回调，不再把局部变量名、注释或精确构造字符串当作行为证据。两次隔离突变分别删除 Y/Opacity 写回，测试按预期失败，差值 `8.332255396871652 DIP` 与 `0.74548606462788181`。
- 绑定提交 `74abb10096df77cb77ef59128f1898e621475a51` 的完整 Release 为 XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `522/579`（57 skip/0 fail），动效定向 `4/4`；突变 `.tmp` 已清理，代码已推送。
- 范围是合成 WPF `Window`、Dispatcher 与 offscreen logical DIP；不宣称真实 Playnite、物理 DPI、OS 输入/IME、presented frame、ETW 或宿主帧率。生产命令、绑定、picker、滚动条、取消/错误、恢复保护、有限列表和 net462 未改。下一项 R01-05。

## 2026-09-16 R01-03 每项证据直达

- `3875f88` 为共享 UI 审计报告增加 `EVIDENCE_INDEX.md`，`UiEvidenceIndexBuilder` 从 manifest、layout 和 fidelity 的实际数据生成具体控件/状态索引；不是把一条自动通过句子复制到几十项。`2eb4c46` 修正生产 DataGrid 优先和源码回退，`591deae` 使控件样本按页面分散，避免 20 项被 maintenance 单页吞没。
- `f6a3209` 增加 `validate-ui-evidence-index.ps1`，`9d5146d` 修正 Windows PowerShell BOM、源码身份命令参数和当前共享 `ButtonChrome=0.72` 守卫；校验器逐行检查前 20 个 E01～E20、结果文件 + `source=src\...`、40 位 commit、非空样本和未验边界。最新完整受控审计 `20/20`，其中 13 个生产 DataGrid、7 个跨页面交互控件；`LAYOUT_REPORT` 运行时结果 7 条，其余静态-only 条目明确标注无运行时几何样本。
- 最新审计报告绑定完整 SHA `9d5146d4a4d604b2779f933f3dee00e730c68b71`，校验 `references/identities/samples/boundaries=20/20`，`Fidelity=0`、失败路由 `0`。全流程 Release 构建 `0/0`，Core `83/83`、Worker `311/311`、Playnite `521` 通过/`57` 跳过/`0` 失败；源码契约定向 `6/6`，源码校验通过。
- 读取索引时，静态 manifest 只说明结构和源码，不能升级成运行时可见/可操作；条件、禁用、错误分支仍需各自行为证据。范围不涉及真实 Playnite、物理 DPI、IME、presented frame、ETW 或宿主性能。证据见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R01-03-EVIDENCE-INDEX-20260916.md`。下一项 R01-04。

## 2026-09-16 R01-02 数字单元格裁切

- `acfe1ea` 完成 F07/R01-02 的可读性门禁校正：新增 `NumericCellReadability`，使用真实 realized `DataGridCell` 测量 `TextBlock` 非约束宽度、扣 padding 后的内容宽度和文本高度，分别暴露横向/纵向 fit；不以“行高足够”代替“最后一位未被列宽裁切”。
- `UiFrameworkProbeView` 的数值列从 `110 DIP` 调整到 `160 DIP`，夹具固定包含 `1,024 / 99,999`、`-9,999,999,999`、`512 GiB`、`1.25 TiB`，显式 `NoWrap`/无 trimming。旧基线 Light/Dark 报告能通过四行和对比度，但图像显示第一行数值列末位被边界截断，且没有水平完整性数据；这只确认夹具/门禁缺口，不推断所有生产表格均有同样缺陷。
- 实际 STA WPF 测试 `2/2`：四个生产夹具样本均 `HorizontalFit=True`、`VerticalFit=True`；`56 DIP` 数值列的长负数负例 `VerticalFit=True`、`HorizontalFit=False`、`IsReadable=False`。clean-tree 双主题探针均为 `expected=4 realized=4 horizontalFit=4 verticalFit=4 allReadable=True`，负例通过，contrast violation `0`。全流程 Release 为构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `520/57/0`。
- 只改诊断 helper、Demo-first 校对夹具和测试；生产业务命令、绑定、picker、滚动条、取消/错误、恢复保护、有限列表路径及 net462 兼容未改。证据见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R01-02-NUMERIC-CELL-READABILITY-20260916.md`。
- 边界：WPF Window/离屏 logical DIP 与合成 DTO 不等于 Playnite 嵌入、物理 DPI、用户输入/IME、presented frame、ETW 或真实宿主性能；临时输出须在证据同步后清理。下一项 R01-03“每项证据直达”。

## 2026-09-16 R01-01 测试源码根绑定

- `abb5589` 完成 F10/R01-01：Playnite 测试程序集嵌入 `GscSourceRoot` 与 `GscBuildCommit`，`TestRepositoryContext.Root` 只使用构建时根并校验源码根结构、Git HEAD 和程序集身份；不再从 `AppContext.BaseDirectory` 向上回溯。37 个 `FindRepositoryRoot` 与 6 个直接 reader 已统一。
- 当前 worktree 的 `scripts/build.ps1 -OutputRoot .tmp\r01-01-isolated` 全流程构建 `0/0`，Core `83/83`、Worker `311/311`、Playnite `518` 通过/`57` 跳过/`0` 失败；输出移到 main checkout 的 `.tmp` 后仍相同，证实不会静默读 main 源码。两个临时目录均已清理，main 源码和 `src.zip` 未触碰。
- 首次短 SHA 定向比较产生预期错配诊断，改为安全前缀匹配后身份/代表性测试 `14/14`；未知或不匹配仍阻断。证据见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R01-01-REPOSITORY-IDENTITY-20260916.md`。下一项 R01-02。

## 2026-09-16 R00-08 搜索框 Enter/IME

- `8435d80` 收窄 `AcrylicProductionShellView` 的选框键盘确认：`Key.ImeProcessed`/IME 处理键不关闭；Enter 只接受 `PickerList.SelectedItem` 且通过 `GamePicker.ItemsView.Contains` 的可见候选；无结果保持旧 `SelectedGame` 和弹层，Escape 仍返回 `GameContextButton` 焦点，方向键继续默认路由。
- `GamePickerViewModel` 原有“筛选隐藏仍保留选择”的恢复语义被保留，但不再被 Enter 误当成当前候选。真实生产 Shell 的 STA WPF 行为测试覆盖无结果、IME/方向键、有效 Enter 和 Escape 焦点回返；定向 Release 构建 `0/0`，相关测试 `28/28`。
- 证据见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R00-08-PICKER-ENTER-IME-20260916.md`。合成 DTO 和最小测试 Dashboard 承载不等价 Windows OS IME、Playnite 嵌入、物理键盘/屏幕/ETW；下一项 R01-01 测试源码根绑定。

## 2026-09-16 R00-07 审计排除项收窄

- `42f9dca` 将 `AnalyzeToolbars` 从 Trainer 设置滚动祖先整棵排除改为用途分类：显式 Toolbar/ActionRow 或足够命令按钮为 `action-toolbar`，输入流和非动作内容保留为 `settings-form`/`content-flow` 并写明 `ExclusionReason`；报告新增有效宽度、可见交集、滚动祖先和可达性。
- clean-tree `42f9dca61eb23b92e1cf80a615b764e844d5a1d7`：RenderHarness Release 构建 `0/0`，审计源/既有精修定向 `29/29`；`toolbarprobe` 正常长表单无 `TOOLBAR_*` 告警，同一 `TrainerToolsSettingsScrollViewer` 下超宽和垂直滚动禁用的动作栏分别命中 `TOOLBAR_HORIZONTAL_OVERFLOW` / `TOOLBAR_UNREACHABLE`。完整审计 `161` 快照、0 Fidelity、0 失败路由、0 HIGH/0 MEDIUM。
- 首轮发现检测器把 `Rect.Empty` 的 `-∞` 宽度当成横向溢出，已修正为有效需求宽度比较并以最终全量审计复核；未修改生产视图或业务契约。证据见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R00-07-TOOLBAR-EXCLUSION-20260916.md`。
- 边界仍是合成 WPF/受控生产视图、隔离目录、offscreen logical DIP；真实 Playnite 嵌入、物理 DPI、输入滚轮、ETW、presented frame 和宿主帧率未验。下一执行点为 R00-08。

## 2026-09-16 R00-06 媒体四行门禁

- `7d57575` 在不替换 `main` 旧实现的前提下，新增 `MediaInboxGeometry` 公式并让生产 `MediaCenterView` 按运行时实测表头、行高、水平条和表格框边界设置 floor；`UiLayoutAnalyzer` 改用真实行容器与有效裁剪交集，覆盖短窗页级回退和父级不可达失败。`db5d483` 补齐 `mediageometryprobe` 的场景/提交号/工作树元数据。
- 当前 clean-tree SHA `db5d483d8ac6460ac7c3a07fe64cec5c7fe417d3`：Playnite Release 单节点构建 `0/0`，R00-06 几何、审计源契约和控件源契约定向 `4/4`，RenderHarness Release 构建 `0/0`。双主题各覆盖 normal、horizontal-scroll、alternate-density、short-fallback、blocked-parent，10/10 通过；默认有水平条时主表/表格框要求 `262/288 DIP`，不同密度 `212/238 DIP` 按实际 `36/44 DIP` 计算。
- 当前提交上的全量审计为 `161` 快照、`0` Fidelity、`0` 失败路由、`0 HIGH / 0 MEDIUM`；`shellqa` exit 0，1040/1100/1366 DIP Media 页尾可达。审计保留嵌套滚动与页级回退 INFO，不把短窗信息误报为问题，也不把离屏几何写成宿主呈现验收。
- 证据见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R00-06-MEDIA-FOUR-ROWS-20260916.md`。范围仍为合成数据、隔离目录、STA/offscreen logical DIP；真实 Playnite Dashboard、物理 DPI、输入/滚轮、ETW、presented frame 和大库宿主帧率未验。下一执行点为 R00-07 审计排除项收窄。

## 2026-09-16 R00-04 搜索基准真实性

- `df884b0` 修正 `LargeLibraryPerformanceTests`：每个正式样本使用不同的 `SearchText`，等待函数按真实可见 `PlayniteId` 集合确认过滤已完成；计时器在每次等待内部独立启动，增加不可能期望结果的有限超时负例。
- 当前隔离 worktree 的 Release 构建 `0/0`，R00-04 定向测试 `2/2`；2,000 个合成游戏的 30 个查询全部产生集合变化，p50/p95/max=`45/60/60ms`，原始查询/样本写入证据文档。
- 第一次重跑曾发现只判断 `FilteredCount=1` 会导致后续样本提前返回，实际只变化 1 次；修正为等待可见 ID 集合后 `30/30` 通过。该事实作为门禁校正记录保留。
- 证据见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R00-04-SEARCH-BENCHMARK-20260916.md`。范围仍是 fake/合成/受控 WPF，真实 Playnite、连续输入、IME、物理 DPI、屏幕帧和 ETW 未验；下一执行点为 R00-05。

## 2026-09-16 R00-05 上下文按钮禁用透明度

- `aebcefc` 删除 `GscWpfUiContextButton` 的控件级 `Opacity=0.48`；禁用态统一由共享 `ButtonChrome` 的 `0.72` 一次处理，避免标签/图标/解释文字的 `0.48 × 0.72` 二次淡化，同时保留按钮布局位置。
- 真实 WPF Window/Dispatcher 的 Light/Dark 定向测试 `2/2`；Context、RemoteRestore、MediaBatch 三类实际派生样式均为控件 `1` / chrome `0.72`，启用/禁用高度差小于 `0.01 DIP`，受控合成最低禁用文本对比度 `3.0`。
- 存档、媒体、维护视图引用和命令绑定保持不变。证据见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R00-05-CONTEXT-DISABLED-20260916.md`。测试窗口不等价真实 Playnite/屏幕像素/物理 DPI；下一执行点为 R00-06。

## 2026-09-16 R00-01/R00-02 按压合成与组合缩放复核

- `a95e900` 修正 `AdaptiveThemePaletteContrastGuard.MeasureGradientTextContrast`：整组 chrome opacity 现在先合成文字/表面，再与真实父背景混合；新增带 offset 的 `GradientStop` API，并对 normal、hover、focus、pressed 及组合状态采样。`e216e9b` 增加 `opacity=0.5` 黑父/白 chrome/黑字的灰背景负例，以及非等距 `0/0.9/1` stop 行为断言。
- `GscMotion.GetMutableScaleTransform` 现在递归寻找并复用组合树中的 ScaleTransform；冻结 root/nested group 或 ScaleTransform 只克隆替换一次，既有 Translate/Rotate 值和变换顺序保留。受控测试连续调用 1000 次并检查节点数、深度、独立控件实例。
- 当前 `e216e9bf0d2ed18adced62936d6897b8d84f0d58` clean-tree：定向 WPF 测试 `5/5`；双主题 RenderHarness `finesseprobe` 均 `88` 个状态样本/`0` violation/`finesse-fixture OK`。记录见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R00-01-02-CONTRAST-SCALE-20260916.md`。
- 受控输出的 `DpiScale=1.00` 是 offscreen logical DIP；没有写成真实 Playnite、鼠标/IME、物理 DPI、屏幕呈现帧或 R08-08 可变共享 Freezable 验收。下一执行点是 R00-03 Dispatcher 完成/取消/卸载/重入行为。

## 2026-09-16 R00-03 动效完成态与生命周期行为复核

- 现有 `4414f05` 的完成回调/卸载和代际保护保持不变；`cda168c` 新增真实 WPF Dispatcher 测试，覆盖完成、入口重入、reduced-motion 活动取消、窗口卸载及旧时钟不晚写。R00-03 不再用字符串门禁替代核心行为。
- clean-tree `cda168ca410bee0a4b0416664452010c9246db96`：Release 单节点构建 `0/0`，5 项定向测试 `5/5`；生产壳层 `motionreentryprobe` / `motionhotprobe` 双主题均 exit 0。重入起点差异约 `0.08 DIP`，热取消后保持 `72/Opacity=1/X=0`，禁用重入 `270 DIP`。
- 证据见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R00-03-MOTION-LIFECYCLE-20260916.md`。受控 Window 不等价真实 Playnite/Windows 偏好通知、鼠标键盘快速输入、ETW 或物理呈现帧；下一执行点为 R00-04 搜索基准真实性。

## 2026-09-15 Q18-03 动效当前值接管受控证据

- `bdb99b9` 新增 `RenderHarness.exe motionreentryprobe` 和 `UiFinesseRound2ControlSourceTests` 门禁；clean-tree 报告绑定完整 SHA、`WorkingTreeClean=True`、双主题、900×640 DIP 和受控 `GscMotionNormal=700ms` 审计覆盖。
- 探针在侧栏收起动画中断后立即触发展开，比较新动画起点与当前有效宽度：Light `105.35→105.33 DIP`、Dark `153.17→153.33 DIP`，重入中间态分别 `214.15/233.13 DIP`，终态均为 `270 DIP / X=0 / 无活动动画`；没有回闪到旧的 72 DIP 收起端点。
- 人工查看八张 PNG；RenderHarness Release `0 warning/0 error`，`UiFinesseRound2ControlSourceTests` `24/24`。Q18-03 受控视觉列升级为通过，但真实 Playnite 快速鼠标/键盘切换、宿主时序、物理 DPI 与屏幕帧仍保持外部边界。

## 2026-09-15 Q18-07 动效 Loaded/Unloaded 生命周期受控证据

- `0815871` 新增 `RenderHarness.exe motioncycleprobe` 和 `UiFinesseRound2ControlSourceTests` 门禁；clean-tree 报告绑定完整 SHA、`WorkingTreeClean=True`、Light/Dark、900×640 DIP 和 100 次 Loaded/Unloaded 循环。
- 同一真实生产 `AcrylicProductionShellView` 每次先启动侧栏过渡，再移出 `ContentControl` 触发 `Unloaded`，检查时钟/Opacity/位移归一后重新加载；两主题均为 `cycles=100 loaded=101 unloaded=101 finalLoaded=False transitionRunning=False opacity=1`，每次循环无残留侧栏动画。
- 人工查看六张 PNG；RenderHarness Release `0 warning/0 error`，`UiFinesseRound2ControlSourceTests` `23/23`。Q18-07 受控视觉列升级为通过，但该夹具使用 Fake 数据上下文且未构造真实 Dashboard VM/Worker 订阅，Playnite 宿主、真实窗口关闭、物理 DPI/屏幕帧和 ETW 仍保持外部边界。

## 2026-09-15 Q18-05 系统动画热变更受控证据

- `dc1dd67` 新增 `RenderHarness.exe motionhotprobe` 和 `UiFinesseRound2ControlSourceTests` 契约门禁；clean-tree 报告绑定完整 SHA、`WorkingTreeClean=True`、双主题、900×640 DIP 和受控 `GscMotionNormal=700ms` 审计覆盖。
- 探针直接使用生产 `AcrylicProductionShellView`，在侧栏收起动画的活动中间态把 `MotionEnabledProvider` 从 true 切到 false，再调用生产 `NormalizeMotionIfDisabled()`：Light/Dark 中间宽度约 `100/143 DIP`、Opacity `0.858/0.640` 且活动动画为 true；归一化终态均为 `72/1/X=0`、无活动时钟，禁用重入均同步恢复 `270 DIP`。
- 人工查看六张双主题 PNG；RenderHarness Release `0 warning/0 error`，`UiFinesseRound2ControlSourceTests` `22/22`。Q18-05 受控视觉列升级为通过，但真实 Windows `SystemParameters` 通知链、Playnite 宿主时序、物理 DPI/屏幕帧和 ETW 仍保持外部边界。

## 2026-09-15 Q18-04 生产壳层动效视觉序列

- `af9b1dd0279d58240bc53128e211fb3a7e210ecd` 的 clean-tree RenderHarness `motionprobe` 直接使用生产 `AcrylicProductionShellView`，在 Light/Dark、900×640 DIP 下生成展开、收起中间态、收起终态和快速重入终态 PNG，并在关闭窗口后读取卸载清理状态；报告含 `WorkingTreeClean=True`、DPI `1.00` 和完整数据范围。
- 两主题中间态分别约为 `100.05/0.858` 与 `152.81/0.592`（width/Opacity），终态均为宽度 `72`、Opacity `1`、X `0`、无活动动画；卸载后 `running=False` 且无活动时钟。700ms 是为稳定截取中间帧的审计覆盖值，生产 `GscMotionNormal` 没有改变。
- 人工查看四张代表图后仅升级 Q18-04 受控视觉列；真实 Playnite Loaded/Unloaded 耐久、Rendering/ETW、物理屏幕帧和 Q18-07 宿主/视觉边界仍不可由该夹具签收。

## 2026-09-15 Q17-04 受控状态夹具视觉复核

- `statefixtures-20260915` 在 RenderHarness Release 下产出双主题、`1040×700 / 1100×720 / 1366×768 / 2560×1440 DIP` 四尺寸共 `160` 张截图和 `160` 条 fixture 记录；人工复核浅色 Loading、深色 Stale、深色 Offline、浅色 Stale 下一步运维代表图，进度覆盖层、过期/离线提示、按钮与正文均可读。
- 账本仅将 Q17-04 的受控视觉列升级为通过；真实 Worker 进度节奏、动画完成/停止、悬停/卸载期间的时序和 Playnite 宿主像素仍不可由夹具证明，宿主列继续外部阻塞，结论继续未完成。

## 2026-09-15 当前提交真实宿主嵌入复核

- `37f92f7` 在全新隔离 UserData、扩展目录、Worker 数据目录和唯一 Pipe/EventPipe 下完成 `real-host-audit.ps1 -Configuration Release`；源码、Plugin、Worker、Contracts 构建身份统一为 `0.6.73+37f92f7f107880bb5a33f61c82eee11fe1344874`。
- `artifacts/ui-host-audit-round2-fp-final6-20260915/summary.json` 为 `EmbeddedDashboardCaptured=true`、`EmbeddedSettingsCaptured=true`、两者 `Origin=EmbeddedPlaynite`、`ProductionVisualSourceOfTruthAvailable=true`、`HighGateCount=0`；capture manifest 为 29 个 Dashboard 视口、2 个完整滚动面、1 个 Settings 视口，150% DPI，资源/视觉树/metadata 绑定当前 SHA。
- 本轮 Release 门禁：XAML `24/24`、构建 `0 warning/0 error`、Core `83/83`、Worker `311/311`、Playnite `501/558`（57 skip）、失败 `0`。Settings 的 `GscPrimaryTextBrush=#FFF2F4F8` 与 `GscSecondaryTextBrush=#FFB9C0CC` 可在当前资源快照复核，代表 Overview、维护诊断概览和 Settings PNG 已人工检查。
- 审计结束后以 Playnite 官方 `--shutdown --userdatadir` 关闭隔离宿主；用户 Worker PID `23304` 未触碰。当前仍只有 `DISPLAY1`，因此 Q24-03 物理跨屏、低于 560 DIP 短窗、IME/读屏、真实组合输入和 ETW 性能边界不升级为完成。

## 2026-09-15 非空隔离库宿主边界复核

- 当前文档 HEAD `0fb597e` 使用隔离 3 游戏 `library` 重跑真实宿主；第一次整目录复制旧运行态后改为只复制成功启动过的配置、主题、扩展数据和 library，仍在主窗口前命中 CEF `mojo platform_channel` `Access denied (0x5)`。
- `artifacts/ui-host-audit-round2-fp-data2-20260915/host-startup-blocker.json` 没有 `summary.json`、Embedded 截图，并固定为 `VisualEvidenceCaptured=false` / `CountsAsVisualPass=false`。Q20-01～08 不能用空库或失败启动证据签收；继续保留 `37f92f7` 空库真实嵌入证据与该非空宿主边界。
- 失败实验只写隔离 `.tmp`/`artifacts`，按官方 shutdown/精确进程清理，未修改用户 Playnite 库，用户 Worker `23304` 未触碰。

## 2026-09-15 真实宿主启动阻断结构化

- 在 `9654c05` 之后，`scripts/real-host-audit.ps1` 的隔离 Playnite 启动改为保留 `Start-Process -PassThru` 进程句柄；进程在主窗口前退出且 `playnite.log`/`cef.log` 尾部命中启动标记时，输出根目录新增 `host-startup-blocker.json`。
- 该 JSON 只记录 `RealPlaynite` 启动事实、日志匹配行和进程状态，并固定写入 `VisualEvidenceCaptured=false`、`CountsAsVisualPass=false`；不放入 `gates/`，不得被 `RealHostUiAuditService.CountBlockingGateFiles` 计为视觉通过或阻断门禁。
- 当前隔离诊断以 `--no-sandbox --disable-gpu` 启动仍复现 CEF `mojo platform_channel` `Access denied (0x5)`，说明问题仍在本机宿主/CEF 初始化边界；这些参数不进入生产脚本默认参数。PowerShell AST 与 `DiagnosticsEvidenceSourceTests` 定向测试通过。
- 后续真实宿主重跑应优先检查 `host-startup-blocker.json`，避免等待完整超时或把没有 `summary.json` 的目录误作视觉证据；只有真实 Embedded Dashboard/Settings 捕获后才可更新对应视觉/宿主列。

## 2026-09-15 Q05-05 生产按钮忙态反馈

- `4947539` 新增 `GameSaveCenter.Playnite.Controls.Button.IsBusy` 与 `WpfUiProduction` 的 `BusyIndicatorHost`；忙态只叠加底部 indeterminate 指示，不交换按钮 Content，不改变宽度，也不在模板中延迟命令。Dashboard 顶部刷新、全部备份、媒体同步绑定现有 `DashboardViewModel.IsBusy`，命令可执行性仍由原 RelayCommand 控制。
- `RenderHarness.exe buttonbusyprobe` 使用真实生产按钮模板与资源，在 Light/Dark、STA、96 DPI、DpiScale=1.00 下均报告 `normalWidth=180`、`busyWidth=180`、`indicatorVisible=True`、`indeterminate=True`、`contentStable=True`；四张 PNG 和原始报告已复制到 `evidence/q04-q12/button-busy-20260915/`。
- `UiFinesseRound2ControlSourceTests` 本阶段为 `17/17`，RenderHarness Release 编译 `0/0`；该证据只升级 Q05-05 视觉列，真实业务命令耗时与 Playnite 输入/宿主仍保留外部边界。不要把底部指示层当作已完成 Hover/Pressed/IME/物理 DPI 验收。

## 2026-09-15 Q05-06 危险确认按钮组复核

- `624ece6` 新增 `dangerdialogprobe` 和源契约断言，夹具直接使用生产 `GscRedesignFeedbackDialogCard`、`GscButtonBase` 以及动态主题资源；危险确认的生产路径仍将危险按钮设为错误色，并把取消按钮传给 `OpenDialog` 作为初始焦点。
- Light/Dark 受控 STA WPF 报告均为 `cancelFocused=True`、`dialogWidth=560`、按钮 `gap=8`、`dangerFirstFocus=false`；浅/深截图已复制到 `evidence/q04-q12/danger-dialog-20260915/`，人工复核通过。夹具修正了第一版静态画刷假阳性，标题/说明现在使用 DynamicResource 跟随主题。
- `UiFinesseRound2ControlSourceTests` 升至 `18/18`，RenderHarness Release `0 warning/0 error`；该阶段只升级 Q05-06 受控视觉列，业务确认完成/取消、Playnite 宿主焦点和真实鼠标/键盘输入仍是外部边界。

## 2026-09-15 Q06-02 按钮卸载状态清理

- `7804431` 在自定义生产 `Button` 构造器订阅 `Unloaded` 路由事件；卸载时清掉 `ButtonChrome` Opacity/ScaleX/ScaleY 动画并写回 Scale=1，同时清掉 Hover/Pressed/Focus 三个 overlay 的活动动画和 Opacity。实现只处理模板交互层，不触碰 Command 或业务状态。
- `132e6d5` 修复完整 RenderHarness 暴露的冻结变换边界：卸载清理仅对未冻结 `ScaleTransform` 取消动画并写回 Scale=1；clean-tree 完整 RenderHarness 为 `render-qa OK`。
- `UiFinesseRound2ControlSourceTests` 为 `19/19`，解决方案 Release、RenderHarness Release 均 `0 warning/0 error`，`validate-source.py` 与 XAML `24/24` 通过；该阶段提升 Q06-02 的代码/自动门禁，真实输入顺序和宿主渲染仍不可由静态证据签收。

## 2026-09-15 Q03-07/Q23-07 设置主题打开态运行时夹具

- `a1f3cae` 新增 `RenderHarness` 的 `settingsthemeprobe` 和完整 QA 调用：真实 `GameSaveCenterSettingsView` 放入 STA WPF 隐藏 `Window`，进入外观页，打开生产 ComboBox 的 `PART_Popup` 与设置保存提示 `ToolTip`，在打开态切换 Light→Dark。
- 聚焦探针和 clean-tree 完整 RenderHarness 均通过；报告为 `popupOpen=True`、`tooltipOpen=True`、主文字资源 `#F21B1F27→#FFF2F4F8`，并人工复核 Settings、Popup、ToolTip 共 6 张 PNG。`UiFinesseRound2ControlSourceTests` 定向 `16/16` 通过，RenderHarness Release `0 warning/0 error`。
- 隐藏窗口没有真实鼠标激活，夹具仅临时将已绑定 Popup 设为 `StaysOpen=True` 以保持可测；生产模板的 `StaysOpen=False`、点外部关闭和真实 Playnite 生命周期仍由源门禁/宿主边界负责。该阶段只升级 Q03-07/Q23-07 视觉列，不升级真实宿主列。

## 2026-09-15 真实宿主最终复核结论

- 最终交付基线为 `69e1f84`，clean-tree Release 隔离审计通过：XAML `24/24`、Core `83/83`、Worker `311/311`、Playnite `494/551`（57 skip）、0 fail；包身份和三份审计身份均为 `0.6.73+69e1f844f8b20b1fcf1667d2b8a6af2772eb0ed4`。
- `artifacts/ui-host-audit-round2-final-20260915` 的真实 `EmbeddedPlaynite` Dashboard/Settings 捕获为 29/2/1（视口/完整滚动面/Settings），Settings `1278×762 DIP`、150% DPI，截图稳定可读。Settings capture 现在等待最长入场动画，审计脚本也不会再因 `$LASTEXITCODE` 管道判断丢失 SHA。
- 这只升级当前深色真实宿主像素来源与交付追溯性；不替代 Hover/Focus、保存回滚、主题切换、物理跨屏 Popup、中文 IME、读屏、ETW 呈现帧、Playnite 长时间耐久或低 Tier 实测。Q24-03 仍单屏阻塞。

## 2026-09-15 真实宿主审计必须保留提交身份

- 真实 Settings 图像在 `5b8a87a` 上已恢复稳定亮度，但该次审计的构建身份虽正确，`runner-metadata.json` 和 Settings metadata 的 `CommitSha` 因 PowerShell 管道后的 `$LASTEXITCODE` 判断而为 `unknown`。
- `scripts/real-host-audit.ps1` 现在只以 `git rev-parse HEAD` 的非空输出设置 `GSC_UI_AUDIT_COMMIT`；`DiagnosticsEvidenceSourceTests` 锁定这个契约，后续证据必须同时检查 runner、metadata 和 build identity。
- 本修复不扩展视觉结论；仍需用提交后的 clean tree 重新安装隔离 Playnite，确认新 Settings screenshot 与 SHA 可追溯。Q24-03 继续受单显示器条件阻塞。

## 2026-09-15 真实 Settings 宿主截图等待完整入场动效

- 本轮真实 Playnite 审计把 Settings 截图抓在 `SettingsShell` 入场动画中间态，造成整体低对比度的视觉假阳性；Style fingerprint 中的前景和各层 `Opacity=1` 已证明不是主题资源缺失。
- `RealHostUiAuditService` 的 Settings capture 现在先等待 `WaitForRenderAsync`，再等待 `GscMotion.GetDuration(settingsView, MotionDurationKind.Slow)` 加 90ms，最后再执行 `CaptureSettings` 并重新等待 Render；这样不修改真实用户入场行为，只让开发审计取稳定终态。
- 新增源回归断言锁定 capture 顺序；Release 编译和 `UiAuditBlockerTests` 已通过。提交后的 real-host audit 必须以新 SHA 和新的 `settings/embedded-current/viewport/settings.png` 为准，旧的 `ui-host-audit-round2-current-20260915` 截图只作为问题复现对照，不可继续作为最终 Settings 视觉证据。

## 2026-09-15 Q24-03 物理跨屏前置复核

- 在当前分支生产代码基线 `39e37b1`、文档 HEAD `7609c4a` 上重新检查 Q24-03 前置：`System.Windows.Forms.Screen.AllScreens` 仍只有 `\\.\DISPLAY1`，边界为 2560×1440，工作区为 2560×1368。
- `DiagnosticsEvidenceSourceTests` + `UiFinesseRound2ControlSourceTests` 为 `17/17`，`real-host-audit.ps1` 语法解析通过；没有运行真实窗口跨屏迁移，也没有生成第二屏或打开态 Popup 的截图。
- 该复核只确认 Q24-03 的条件阻塞仍然真实，不改变账本的自动/视觉/宿主结论；第二个物理显示器和可见 Playnite 宿主满足后才能继续。证据页为 `Q24-03-PHYSICAL-CROSS-SCREEN-20260915.md`。

## 2026-09-15 UI 精修 Q18 生产侧栏卸载清理回归

- `39e37b1`（`补充侧栏卸载清理回归`）修正 `AcrylicProductionShellView.OnUnloaded` 的真实缺口：取消 `TranslateTransform.X` 动画后写回 `X = 0`，避免窗口卸载中途留下 `4` DIP 位移残留；同一提交新增 `ProductionShellChromeSourceTests.SidebarTransitionReleasesClocksOnCompletionAndUnloadInAnActualWpfWindow`，用实际 STA WPF `Window` 触发生产按钮、推进完成态，再在长动画中关闭窗口并验证过渡结束、Opacity/X 无活动时钟和位移归零。
- Debug 构建 0 warning/0 error；定向 Playnite `165` 通过、`39` 跳过、0 失败（204 总计）；Release 全量 Core `83/83`、Worker `310/311`（1 skip）、Playnite `487/550`（63 skip），失败 `0`；侧栏回归在 Release 下连续独立回放 `5/5`，`validate-source.py`、XAML `24/24` 和 `git diff --check` 通过。
- 这是受控 WPF Window 的完成/关闭卸载证据，不等价真实 Playnite Loaded/Unloaded 100 次、宿主窗口关闭、Rendering/ETW 或物理屏幕帧；Q18-04/Q18-07 视觉和真实宿主列继续待验。证据页为 `Q18-04-07-MOTION-CLEANUP-20260915.md`，Release 汇总为 `Q25-08-RELEASE-TEST-BASELINE-20260915.md`。

## 2026-09-15 UI 精修 Q18 动效终态运行时回归

- `68b49a1`（`补充动效时钟终态运行时回归`）新增 `UiFinesseFoundationTests.MotionAnimationsReleaseClocksAtTheirFinalValues`：创建真实 STA WPF `Window`/PresentationSource 宿主，使用局部 30ms motion token 推进 `AnimateTranslate` 和 `AnimateEntrance`，完成后断言 Transform/Opacity 为目标值，`DependencyPropertyHelper` 不再报告活动动画。
- Debug 构建 0 warning/0 error；动效运行时测试 `1/1`，Release 独立回放连续 `5/5`；定向 Playnite 门禁 `164` 通过、`39` 跳过、0 失败（203 总计）；Release Core `83/83`、Worker `310/311`（1 skip）、Playnite `486/549`（63 skip），失败 `0`；源码、XAML、`git diff --check` 均通过。
- 该测试是受控 WPF Window，不等价真实 Playnite Loaded/Unloaded 100 次、宿主窗口关闭、Rendering/ETW 采样或物理屏幕帧；Q18-04/Q18-07 的宿主/视觉列继续待验。证据页为 `Q18-04-07-MOTION-CLEANUP-20260915.md`。

## 2026-09-15 UI 精修 Q16-08/Q17-07/Q18-04/Q18-07 动效时钟与卸载清理

- `4414f05` 修复完成态动效生命周期：`GscMotion.AnimateTranslate`/`AnimateEntrance` 捕获当前有效值后以显式 `FillBehavior.HoldEnd` 运行动画，并在完成回调中移除时钟、写回目标 Transform/Opacity；Dashboard 的页面切换、状态胶囊、缩放、对话框和 Toast 入场也采用有界完成清理，生产侧栏保持完成/取消/卸载清理。
- Dashboard 对话框新增 `dialogMotionGeneration` 和 `StopDialogMotion()`，关闭或卸载时会使旧回调失效并清理透明度/Y 位移；`ClearToasts()` 复制当前 Border 后逐一调用 `RemoveToast()`，因此卸载不再绕过 DispatcherTimer 和 Toast 动画清理。
- 验证：Debug 构建 0/0；定向 148 通过、39 跳过、0 失败；Release Core `83/83`、Worker `310/311`（1 skip）、Playnite `485/548`（63 skip），失败 `0`；`validate-source.py` 与 `check-xaml.ps1` 通过。
- 这只签收源码级完成/取消/卸载契约。真实 Playnite 100 次 Loaded/Unloaded、窗口关闭、Rendering/ETW 采样、动画热切换和物理屏幕帧没有运行，Q16-08/Q17-07/Q18-04/Q18-07 的宿主/视觉列必须保持待验。证据见 `Q18-04-07-MOTION-CLEANUP-20260915.md`。

## 2026-09-15 UI 精修 Q17-04/Q18-03 状态夹具与动画重入修复

- 提交 `c76ce62` 修复 `GscMotion.AnimateEntrance` 的重入行为：通过 `DependencyPropertyHelper.GetValueSource` 判断活动动画，捕获当前有效值、停止旧时钟并从该值继续；首次进入仍使用既定 offset/opacity 起点。
- `DesignTokens.xaml` 的不确定进度扫过动画现在由测试明确锁定 `StopStoryboard`，终态不会继续保留循环状态动画；定向门禁共 `18/18` 通过。
- 当前 Release RenderHarness 状态夹具为 `statefixtures OK`，`160` 条 fixture/PNG 记录覆盖 MediaInbox、MediaDetails、MaintenanceAudit、MaintenanceNextSteps 的适用六态、Light/Dark、`1040×700`/`1100×720`/`1366×768`/`2560×1440`。持久证据仅保留报告和 4 张代表截图，不提交整批生成物。
- 状态夹具只能证明状态覆盖层/提示/表面几何；真实 Worker 进度节奏、动画中途取值、宿主卸载和 Playnite 像素仍需宿主复核，Q17-04/Q18-03 的视觉/宿主列不提前签收。
- 当前修复代码的串行 Release 全量测试通过：Core `83/83`、Worker `310/311`（1 skip）、Playnite `484/547`（63 skip），失败 `0`；Playnite 总数的增加来自本阶段两个回归测试，不是跳过项变化。

## 2026-09-15 UI 精修 Q18-01 资源宿主解析收口

- `8dfe7fa` 修复了 Q18-01 的证据假阳性：`GetDuration(host, ...)` 原本虽有单测，生产调用却使用静态时长；现在所有生产动效入口按 `FrameworkElement`/壳层视觉宿主解析局部资源，入口不再绕过 Host override。
- 当前验证为定向 `202/202`（39 项既有宿主布局测试跳过）、Release Core `83/83`、Worker `310/311`（1 skip）、Playnite `485/548`（63 skip），失败 `0`；`validate-source.py` 与 XAML 结构门禁通过。
- 真实系统动画偏好热变更、宿主卸载时序、物理屏幕帧与 ETW 生命周期仍需外部宿主/性能工具，不将本次资源路径门禁写成视觉验收。

## 2026-09-15 UI 精修 Q12-08 双主题业务空表复核

- 提交 `77f4dc5` 为 RenderHarness 增加 `emptytables` 入口和 `FakeDashboardData.ClearTableDataForFixture()`，只清空开发夹具数据，不改生产命令、Binding 或页面行为；夹具状态使用 `WorkspaceFixtureState.Empty`，并暴露 Trainer 工具/目录/版本读取状态。
- clean-tree 上用临时 direct-reference WPF runner 加载 production views/XAML，覆盖 Light/Dark、`1040×700` 与 `1600×900`；报告确认 Save/Task/Trainer/Media/Maintenance 所有目标表/列表为 0 项且有空态文案，结果 `emptytables OK`。
- Q12-08 的离屏视觉列已签收；真实 Worker/Playnite 生命周期、Popup/Tooltip、物理 DPI、键盘/读屏继续不由离屏证据替代。由于本机缺少项目要求的 .NET 8 SDK，常规 ProjectReference 构建仍需在具备正确 SDK 的环境补跑。

## 2026-09-15 UI 精修 Q02-06 生产路径视觉复核

- 提交 `82cf066` 的 clean-tree `RenderHarness audit` 复核了 SaveCenterView“路径与校验”页；1440×900 与 1040×700 DIP 下候选路径列和详情面板均可读到完整 `D:\Games\Baldur's Gate 3\Save\N\SlotN`，路径单元格为 `260 DIP`，详情为 `321.33 DIP`。
- Q02-06 只签收离屏视觉列。`GscPathText`/`SavePathText` 的共享代码字体、CharacterEllipsis 和原值 Tooltip 仍由源码证据覆盖；真实宿主 Tooltip、复制、中文长路径截断、字体/DPI 和 IME 不由该审计替代。证据见 `Q02-PATH-RENDER-20260915.md`。

## 2026-09-15 UI 精修 Q02-07 窄窗正文尺寸视觉复核

- 提交 `3811673` 的 clean-tree `RenderHarness audit` 取得 1040×700 DIP 的 Overview、Save、Media、Maintenance、Trainer、Task 六页截图；人工复核确认重要文本、状态、数字和操作仍可读，汇总为 `HIGH=0 / MEDIUM=0 / Fidelity=0`。
- Q02-07 只签收六页离屏视觉列；Settings 路由未加载有效内容，不把空白页算作证据。源码 10/11 DIP 门禁、真实宿主小窗、物理 DPI、GlyphRun 与 IME 仍分别保持原有边界。

## 2026-09-15 UI 精修 Q00 深色设置前景回归与 Q21/Q23 视觉证据

- 提交 `0648689` 修复设置页 `SettingsValidationDetails` Expander Header 的主题前景漏检：不要因为 `UserControl.Foreground` 已设置，就假定控件模板 Header 会继承正确画刷；共享节点现在明确绑定 `DynamicResource GscPrimaryTextBrush`，源代码回归测试锁定节点级契约。
- 当前 clean-tree RenderHarness 身份为 `0648689a1ac74f43ec918e051f80037a4ff2d20d`，`WorkingTreeClean=True`，Light/Dark 多尺寸及 normal/dirty/invalid Settings 夹具 `render-qa OK`。该证据仍是 96 DPI/1.00 的离屏逻辑 DIP，不能替代实机窗口 DPI、屏幕帧或输入序列。
- 页面截图实证覆盖 Save 差异/备份策略、Trainer 已绑定工具、Settings 分类导航和错误摘要；账本只升级 Q21-03/Q21-07/Q21-08 与 Q23-01/Q23-04 的视觉列，保存中/失败、恢复对话、主题切换 Owner、键盘和宿主边界继续保守记录。

## 2026-09-15 UI 精修 Q01/Q02 当前字体与数字夹具身份复核

- 提交 `8493c1a` 为 `finesseprobe` 报告统一补充 `Commit`、`WorkingTreeClean`、DPI、主题和数据范围元数据；该改动不改变生产 UI，只修复证据身份缺口。
- 当前 clean-tree Dark/Light 夹具均通过：有效文本对比 `12/12`、黑字负例 `1`、行交集 `4/4`，压缩视口负例 `3/4`；报告中的标点、数字/单位、路径和中英混排样本可直接回溯到当前提交。
- 全量 Release 门禁复核为 XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `310/311`（1 skip）、Playnite `481/544`（63 skip、0 fail）；持久报告只引用证据目录内的相对截图名，已清理本轮 `.tmp` 输出。
- 账本视觉列据此升级 Q01-04、Q02-03、Q02-08；Q02-02 数值列实际锚点、Q02-04 未知值、Q02-06 真实路径 Tooltip 与其它宿主行为仍需更针对性的页面/宿主证据。
- 提交 `94dc203` 的 clean-tree RenderHarness audit 记录 SaveHistoryGrid 的 8 行文件数/大小样本共 16 个数值 TextBlock 全部 `HorizontalAlignment=Right`，大小值宽 `55.33–63.33 DIP` 且未裁切；因此 Q02-02 视觉列可签收，真实宿主字体/DPI、列宽调整和排序点击仍保持边界。
- 提交 `a26ef49` 的 clean-tree `edgevalues` 夹具覆盖 `0 B`、`未知大小`、`尚未检查` 和 `文件 0/0 · 大小 0 B/0 B`；Q02-04 视觉可签收，真实业务入口、排序键和宿主状态切换继续保持边界。

## 2026-09-15 UI 精修 Q12-07 表头排序箭头双状态夹具

- 提交 `2f3d17b` 为 RenderHarness 增加 `finesseprobe ... sorted` 开关：仅在开发夹具内给两列设置升/降序状态，读取共享 `SortGlyph` 的可见宽度与降序 `180°` 旋转，不触碰业务排序逻辑。
- Dark/Light clean-tree 报告均记录 `ascending="名称" visible=True width=14`、`descending="数值" visible=True width=14 angle=180`；双主题截图证明箭头未压缩表头文字且对比清晰。
- Q12-07 视觉可签收，真实排序点击、排序结果和宿主输入序列仍是外部边界。

## 2026-09-15 UI 精修 Q15 Tooltip、Popup 与浮层主题自动门禁

- 提交 `86ac336` 将 Dashboard、AcrylicProductionShellView 和独立 Settings 的 Tooltip 宿主边界统一为 `InitialShowDelay=350 ms`、`BetweenShowDelay=100 ms`；共享 Combo Popup 两套模板显式声明 `StaysOpen=False`，避免把点外部关闭留给默认样式推断。
- Popup 仍使用 Bottom 定位、动态 `GscPopupBrush/GscPopupEffect/GscPopupAllowsTransparency/GscPopupAnimation`、有限高度和自动滚动；键盘方向导航为 Contained。该改动没有复制业务控件或改变 Combo Binding/DropDownClosed 语义。
- `FloatingThemeResourcesStayLocalToDashboardAndSettingsOwners` 在 STA 中验证浅色 Dashboard 与深色 Settings 的 Popup 资源差异、Settings 材质只在 Settings 字典、Owner 字典不被反向写入；源码测试同时锁定 Tooltip 自动化名称与共享 Popup 几何。Playnite 全量 `481/544`（63 skip），Release `0/0`，WPF 静态审查 0 error。
- 证据为 `Q15-TOOLTIP-POPUP-THEME-20260915.md`。自动列可签收，但真实 Playnite 悬停时序、Combo 边缘翻转/点外部/子菜单、独立窗口打开态主题切换和物理跨屏继续是宿主边界。

## 2026-09-15 UI 精修 Q25-03 UI 动作热点边界专项

- 提交 `ed97d2c` 为 `RenderHarness enduranceprobe` 增加动作周期耗时、p95、最大值、`>100 ms` 计数和最多 8 条超阈值后的当前线程栈；契约测试 `EnduranceProbeRecordsUiActionHotspotBoundariesWithoutClaimingEtwStacks` 同步加入。
- 干净 120 秒受控运行完成 `120.4s/120s`、`254` 循环、`886` 动作、`13` 样本、0 动作异常；动作 p95/max=`249.94/483.44 ms`，`>100ms=254/254`。原始报告为 `.tmp/q25-03-hotspot-clean-20260915/enduranceprobe-report.txt`，证据为 `Q25-03-UI-HOTSPOT-20260915.md`。
- 栈是在动作完成后的 `finally` 中取到的，不能冒充 ETW/PerfView 停顿期间调用栈；共同路径主要是 `RunActionCycle`/`DispatcherTimer`/WPF Dispatcher。真实 Playnite 宿主热点与优化前后对照仍外部阻塞，不能由该代理数据签收 Q25-03 最终结论。

## 2026-09-15 UI 精修 Q25-02 呈现回调代理专项

- 提交 `a1462ee` 为生产壳层侧栏 Rendering 代理补齐 `frameGapP95`、`maxFrameGap` 和 `slowFrameRatio`；干净 shellqa 的单次切换为 `66.1/193.2/0.147`，快速二次切换为 `15.3/28.5/0.038`，无动画原子终态为 `36.1/36.1/0.250`。
- 这是 `CompositionTarget.Rendering` 回调间隔，不是 DWM/PresentMon 屏幕帧；单次切换的 `193.2ms` 只说明代理有慢间隔，不能转写成显示器丢帧，也没有把它扩展成 Q25-03 调用栈。真实 ETW 仍受 `0x5 / Access denied` 阻塞，证据为 `Q25-02-RENDERING-PROXY-20260915.md`。

## 2026-09-15 UI 精修 Q25-04 30 分钟耐久专项

- 提交 `7d7e2cb` 新增 `RenderHarness enduranceprobe`。当前干净提交以真实 WPF STA Window 承载生产壳和六页工作区，按 250 ms 循环导航、主题、Media 预览分段、列表/DataGrid 选择和详情入口；1800.4 秒完成 `3847` 循环、`13462` 动作、`176` 资源样本，`actionFailures=0`。
- 探针仅用 `GC.GetTotalMemory(false)`、私有字节、工作集、线程和句柄采样，不调用强制 GC。私有字节全窗口趋势约 `348,564.72 bytes/min`，首/末样本为 `153,894,912`/`274,464,768` bytes，预热后线程 `22–25`、句柄 `1170–1177`；报告保留首尾/峰值/均值/趋势，明确这是有界窗口观察，不是数学证明。
- 原始报告为 `.tmp/endurance-clean-20260915/enduranceprobe-report.txt`，证据为 `Q25-04-ENDURANCE-20260915.md`。该结果不替代 Playnite 嵌入宿主、真实输入/媒体/网络任务、ETW 呈现帧或 >100 ms 调用栈；Q25-04 的自动验证已通过，宿主边界保持未完成。

## 2026-09-15 UI 精修 Q25-05 低性能回退专项

- 提交 `97dd0cd` 新增 `RenderHarness lowcostprobe`：六个工作区、浅/深色、`1040×700`/`1600×900` 共 24 个案例在禁玻璃/禁动画资源路径下通过；效果树 0、Popup 透明/动画关闭、环境光与游戏背景透明度 0，可见文字保留。
- `DG_ScrollViewer` 的水平滚动是数据表有界访问方式，不归类为页面溢出；报告为 `WorkingTreeClean=True`。这只证明共享回退和受控离屏像素，真实低 Tier 显卡/Playnite 合成仍不能由此签收。

## 2026-09-15 UI 精修 Q25-01 热态响应专项复核

- 当前基线 `1a58e4b` 在干净工作树上重跑 `LargeLibraryPerformanceTests`：2000 项 GamePicker 预热 5 次、采样 30 次，专项 `4/4` 通过；`search_p50/p95/max=45/46/60 ms`，固化的 `p95≤100 ms` 门禁通过。
- 证据为 `Q25-01-HOT-RESPONSE-20260915.md`，原始结果保留在 `.tmp/q25-01-clean-20260915/ui-qa/benchmarks/large-library.txt`。只签收受控 ViewModel 输入到反馈自动验证，不把它扩展成真实页面导航、屏幕呈现帧、30 分钟耐久或低 Tier 证据。

## 2026-09-15 UI 精修 Q20-08 首页边界状态

- 当前交付基线为 `5a2ed09`。Overview 全局活动空态新增 `OverviewActivityEmptyState` 与 `MinHeight="120"`，修复 `Activities.Count == 0` 时 `Auto` 行把 `WorkspaceStatePresenter` 压成不可读区域的真实布局缺口。
- RenderHarness 新增 `overviewedges`，覆盖空活动、多风险、96 字符超长标题和 Worker 离线四个夹具；浅/深色各覆盖 `1040×700` 与 `1600×900`，16/16 首屏输出通过，空态实测高度 `160 DIP`，并生成页面尾部截图。报告记录 `WorkingTreeClean=True`。
- 标准干净 RenderHarness 为 `render-qa OK` 且无 `PROBLEM`；Core `83/83`、Playnite `474/537`（63 skip、0 fail），定向 Overview/边界静态契约 `8/8`。真实 Playnite 状态切换、Hover/Focus、物理 DPI/多屏、IME/读屏仍待宿主条件；证据见 `Q20-08-OVERVIEW-EDGE-STATES-20260915.md`。

## 2026-09-15 UI 精修游戏选框键盘与自动化名称

- 当前交付基线为 `dea74f7`。`AcrylicProductionShellView` 的生产游戏选框增加了共享 `PreviewKeyDown` 关闭入口：Esc、已有选中游戏时 Enter、点外部和选择游戏都统一关闭，并将焦点返回 `GameContextButton`；打开时搜索框获得键盘焦点。
- 壳层导航、Header 媒体/备份动作、筛选 ComboBox、游戏列表、footer 状态区和 Overview 主要动作新增稳定 `AutomationProperties.Name`；Overview 优先事项标题提供完整标题 Tooltip。
- 定向 WPF 行为/源码测试 `17/17`、RenderHarness 双主题多尺寸 `render-qa OK`、源码验证和 WPF 技能审查 `0 errors` 通过。Q09-06、Q15-07、Q24-04～06 的真实宿主 Popup/Tab/读屏/IME 边界仍未签收，证据见 `Q09-Q24-KEYBOARD-FOCUS-AUTOMATION-20260915.md`。

## 2026-09-15 UI 精修 Q20-07 云端保证级别

- `CloudTransferSummaryDto.GuaranteeDisplay` 固定分开显示 `已上传 N · 已校验 M`；Overview 云端卡以 `Run` 保留 `QueueControlDisplay`，不丢失队列暂停/运行语义，也不把远端校验降格为普通上传。
- `OverviewInteractionTests` 在真实 WPF STA 视觉树中读取绑定后的状态行并继续验证整卡 `OpenCloudQueueCommand` 单次执行；`UiDisplayMappingTests` 和 `UiFinesseRound2ControlSourceTests` 分别锁定 DTO 与 XAML 契约。源码提交为 `f01dfe9`。
- Core 全量 `83/83`、Playnite 全量 `473/536`（63 skip，0 fail）；干净 RenderHarness `f01dfe9` 为 0 warning/0 error、无 `PROBLEM`、`render-qa OK`，Light/Dark 1040×700 实际查看无裁切。Q20-07 真实宿主 Hover/Focus/单次导航仍待验收。

## 2026-09-15 UI 精修 150% 宿主截图渲染修复

- 在提交 `4f1dbb4` 中修正 `UiDiagnosticsExporters.RenderBitmap` 的 DPI 叠加：RenderTargetBitmap 改用 96 DPI 基线，显式 `renderScale` 单独负责高 DPI 像素输出；新增 `HighDpiPngUsesExplicitScaleWithoutApplyingHostDpiTwice` 回归测试，验证 1.5 倍输出不再按 2.25 倍绘制并裁掉右/下边界。
- 真实宿主 `artifacts/ui-host-audit-dpi-fixed-20260915` 在非空隔离库（复制原 Playnite library 的 24 个文件，未修改原数据）中观察到 3 个游戏；150% DPI 下 27 个 Dashboard 视口均为 1948×1350 px 且完整性通过，Overview 与 Maintenance 两个完整滚动面、Settings 视口也均通过捕获验证。当前真实 Dashboard 的右侧当前游戏卡片、六项指标、工具栏和底部活动面板不再出现审计渲染器造成的假裁切。
- 本轮全量门禁为 XAML `24/24`、Release `0/0`、Core `82/82`、Worker `311/311`、Playnite `475/532`（57 skip，0 fail），包身份为 `0.6.73+4f1dbb46e750572862ac954be6d1485b1a6683a9`；审计后隔离 Playnite/Worker 已停止。Q20 交互边界、Q24-03 多屏物理条件、Q25-02～05 ETW/呈现帧/耐久/低 Tier 仍未完成。
- 证据追加至 `docs/design/reviews/ui-finesse-round2-20260913/evidence/q13-q25/REAL_HOST_AUDIT-20260914.md`；截图修复属于证据生成正确性修复，不扩大既有真实宿主覆盖结论。

## 2026-09-15 UI 精修真实宿主原生命令复核

- 在干净 HEAD `2250719b728f6ddee32233f9ff456b8ea1b8fc7d` 上完成第四次隔离 Playnite 真实宿主审计。插件启动后通过 Playnite 自身 `SelectSidebarViewCommand` 选择 `GameSaveCenter`，日志确认 `EmbeddedPlaynite Dashboard capture`，没有使用专用 Dashboard fallback 冒充嵌入来源。
- 本轮完成 XAML `24/24`、Release `0 warning/0 error`、Core `82/82`、Worker `311/311`、Playnite `474/531`（57 skip，0 fail），打包/安装/启动成功；程序集身份为 `0.6.73+2250719b728f6ddee32233f9ff456b8ea1b8fc7d`，隔离配置的 FusionX 主题复制为 true，宿主 DPI 为 150%。
- `summary.json` 为 `EmbeddedDashboardCaptured=true`、`EmbeddedSettingsCaptured=true`、`ControlledDashboardCaptured=false`、`ProductionVisualSourceOfTruthAvailable=true`、`EmbeddedDashboardOrigin=EmbeddedPlaynite`；`capture-manifest.json` 保存 27 个 Dashboard 视口、2 个完整滚动面和 1 个 Settings 视口，当前 Dashboard/Settings 均为真实 Playnite 嵌入来源。该结果首次使当前深色宿主 Dashboard 具备可签收的生产像素来源。
- 运行器的 UIA 侧栏未定位警告仍保留，但不再作为最终判定依据：宿主原生命令调用、插件日志与 Window.GetWindow 真实性元数据共同形成成功闭环。没有取得 Hover/Focus、键盘/IME、读屏、浅色/高对比、物理跨屏、ETW 呈现帧、>100ms 调用栈、30 分钟耐久或低 Tier 实测。
- 隔离 Playnite 与 Worker 已在审计后停止且无残留；原用户数据未修改。Q20 的完整边界状态/交互、Q24-03 的第二物理屏和 Q25-02～05 的性能边界继续保持未完成。证据见 `docs/design/reviews/ui-finesse-round2-20260913/evidence/q13-q25/REAL_HOST_AUDIT-20260914.md`。

## 2026-09-14 Q25 ETW 工具边界确认

- 本机可定位 `xperf`/`wpr`/`wpa`/`wpaexporter`，并能查询 Microsoft-Windows-Dwm-Core 的 `SCHEDULE_RENDER`、`SCHEDULE_PRESENT`、`SCHEDULE_GETPRESENTSTATS` metadata；没有 PresentMon、dotnet-trace 或 PerfView。
- 5 秒 xperf DWM 会话冒烟在 `-start` 阶段返回 `0x5 / Access denied`，没有 ETL、事件样本或残留会话。Q25-02/Q25-03 的呈现帧统计和 `>100ms` 调用栈仍缺证；Worker `[PERF]` 刷新日志不替代 ETW。Q25-04/Q25-05 也不能由此推导通过。
- 证据见 `docs/design/reviews/ui-finesse-round2-20260913/evidence/q13-q25/Q25-ETW-TOOL-BOUNDARY-20260914.md`，后续需要主机允许 ETW 或提供等价呈现采集工具。

## 2026-09-14 UI 精修隔离 Playnite 句柄刷新后的复核

- 在 `a04a824` 上完成干净隔离宿主流程：XAML `24/24`、Release `0 warning/0 error`、Core `82/82`、Worker `311/311`、Playnite `474/531`（57 skip，0 fail），打包/程序集身份/隔离安装成功，身份为 `0.6.73+a04a8249a2411c78f0580d67ff195ef27afaa40d`。
- `scripts/real-host-audit.ps1` 的侧栏探测现在会对进程调用 `Process.Refresh()`；最新 `runner-metadata.json` 仍为 `ConfiguredDesktopThemeCopied=true`。Playnite 日志确认插件加载和 `WindowFactory:Show window`，但刷新后仍未找到 `GameSaveCenter` 侧栏，随后由插件进入专用窗口 fallback。
- 最新输出为 `artifacts/ui-host-audit-refresh-final-20260914`，`summary.json` 为 `EmbeddedDashboardCaptured=false`、`EmbeddedSettingsCaptured=true`、`ControlledDashboardCaptured=true`、`ProductionVisualSourceOfTruthAvailable=false`、`EmbeddedDashboardOrigin=None`。隔离进程已停止，原用户数据未修改；网络更新清单 TLS 失败只记作宿主噪声，不改变插件加载事实。
- 这轮只排除了句柄缓存造成的审计脚本误判，没有取得真实 Dashboard。Q24-03 仍受单屏条件限制；Q25-02～05 的 ETW 呈现帧、>100ms 调用栈、30 分钟真实窗口耐久和低性能 Tier 仍未完成。三次宿主结果见 `evidence/q13-q25/REAL_HOST_AUDIT-20260914.md`。

## 2026-09-14 UI 精修隔离 Playnite 主题修复后的复核

- 在 `ed42868` 上再次运行 `scripts/real-host-audit.ps1 -Configuration Release`。审计脚本已把原 Playnite 配置中的 `FusionX_54244ec8-29ec-418e-bce7-415250c8d67b` 主题复制到隔离用户数据，最新 `runner-metadata.json` 为 `ConfiguredDesktopThemeCopied=true`；Playnite 日志不再出现主题缺失错误，并确认 `GameSaveCenter 0.6.73` 加载及 `WindowFactory:Show window`。
- 最新输出为 `artifacts/ui-host-audit-theme-20260914`，隔离数据为 `.tmp/ui-host-userdata-theme-20260914`。Release 构建 0 warning/0 error，Worker `311/311`，Playnite `474/531`（57 skip，0 fail），程序集身份为 `0.6.73+ed428687f5cdedc64c753e22b85abfa411405be2`；150% DPI 与 `1706.67×912 DIP` Dashboard metadata 保持可追溯。
- 主题修复没有改变捕获边界：`EmbeddedDashboardCaptured=false`、`EmbeddedSettingsCaptured=true`、`ControlledDashboardCaptured=true`、`ProductionVisualSourceOfTruthAvailable=false`、`EmbeddedDashboardOrigin=None`。刷新有效句柄后，UIA 仍只有无后代的 `EmptyWindowAutomationPeer`，没有 `GameSaveCenter` 侧栏项；因此 Dashboard 生产像素、Q24-03 物理跨屏和 Q25-02～05 的 ETW/调用栈/30 分钟耐久/低 Tier 仍未完成。原始结果与两次复核记录在 `evidence/q13-q25/REAL_HOST_AUDIT-20260914.md`。
- 物理前置检查只枚举到主屏 `\\.\\DISPLAY1`（2560×1440，工作区 2560×1368），没有第二物理屏；Q24-03 本轮不执行窗口/Popup 跨屏迁移，继续标为未完成/待宿主条件，不能用单屏事实替代多屏证据。

## 2026-09-14 UI 精修隔离 Playnite 真实宿主审计

- 在干净 HEAD `6bae7c1` 上以 `scripts/real-host-audit.ps1 -Configuration Release` 完成隔离用户数据的 Release 构建、打包、安装和真实 Playnite 启动；Playnite 日志确认 `GameSaveCenter 0.6.73` 已加载。构建 0 warning/0 error，Worker `311/311`，Playnite `474/531`（57 skip，0 fail），程序集身份为 `0.6.73+6bae7c14cb93cfad992c230aedd5a458a10c0f34`。
- 真实宿主 metadata 取得 `EmbeddedSettingsCaptured=true`、`EmbeddedSettingsOrigin=EmbeddedPlaynite`，并记录 `DpiScale=1.5`、`PixelsPerDip=1.5`；审计输出为 `artifacts/ui-host-audit-current-20260914`，持久边界说明见 `evidence/q13-q25/REAL_HOST_AUDIT-20260914.md`。隔离 Playnite 与 Worker 已在审计后停止，用户 Playnite 数据未修改；本轮两个 `artifacts/gsc-b` 中间构建目录已清理，当前宿主输出和最新包保留。
- `EmbeddedDashboardCaptured=false`、`ControlledDashboardCaptured=true`、`ProductionVisualSourceOfTruthAvailable=false`；UI Automation 90 秒内未定位 GameSaveCenter 侧栏，受控窗口截图仍标记 `DedicatedAuditWindow`。因此 Q00-08/Q25-07 的当前包安装/启动事实已补齐，但 Dashboard 宿主视觉、Q24-03 物理跨屏、Q25-02～05 ETW/调用栈/30 分钟耐久/低 Tier 仍未完成。

## 2026-09-14 UI 精修 Q25-08 交付回查

- 当前源码基线为 `eabfc43`；Q25-08 的自动交付门禁已通过：XAML 24/24、Release 构建 0 warning/0 error、Core 82/82、Worker 310/311（1 skip）、Playnite 468/531（63 skip），无失败。
- 证据、208 行账本、项目记忆和工作日志已同步，阶段临时输出已清理；提交已推送并通过 `git ls-remote` 核对远端分支。最终账本审计确认 208/208 唯一任务 ID、0 缺失/异常、0 个缺失相对证据链接，`validate-source.py` 与 `git diff --check` 通过。Q00-08 的 clean package/install/start、Q24-03 物理跨屏、Q25-02～05 的 ETW/耐久/低 Tier 仍保持未完成，不以全量单元测试替代宿主证据。

## 2026-09-14 UI 精修 Q02-08 混排空格与术语

- Q02-08 新增 `UiDisplayMappingTests.MixedLanguageDisplaySurfacesKeepSemanticSpacingAndProductTerms`，覆盖云端摘要、媒体归类建议/批次、恢复指标、最近保护、媒体来源和 Trainer 版本的真实 DTO 显示值；与已有游戏选框 `MetaDisplay`、Trainer 导入候选路径测试合计形成 8 类入口的短术语样本。
- 门禁锁定 ` · ` 两侧单空格、中文量词/单位、`Xbox Game Bar`、`FLiNG Trainer`、`Cyberpunk 2077` 和 `+30 项` 等产品/版本语义，并拒绝重复分隔符；Core 定向测试为 17/17，串行 Release 构建 0 errors。随后完整 Release 门禁为 XAML 24/24、构建 0 warning/0 error、Core 82/82、Worker 310/311（1 skip）、Playnite 468/531（63 skip），无失败。
- 证据见 `docs/design/reviews/ui-finesse-round2-20260913/evidence/q02/Q02-MIXED-LANGUAGE-TERMS-COVERAGE.md`。这仍只是源码映射证据；宿主小窗口折行、真实字形、屏幕阅读器/IME、物理 DPI 和最终视觉继续保持 Q02-08 未完成边界。

## 2026-09-14 UI 精修 Q02-07 正文尺寸下限

- 生产 `Views`/`Settings`（排除 `Views/Development`）中的 109 处显式 `FontSize="10"`/`FontSize="11"` 已在提交 `d22928a` 统一到 `DynamicResource GscCaptionFontSize`；保留 10.5、12.5 等有明确语义的中间层级，技术路径继续使用代码字体入口。
- 新增 `TypographyDiagnosticsTests.ProductionTenAndElevenPointTextUsesSharedCaptionToken`，遍历生产 XAML 并锁定 10/11 精确字号为 0。`validate-source.py`、`check-xaml.ps1`、`git diff --check` 通过；WPF/RenderHarness Release 构建成功，定向 Typography 测试 `8/8`，明暗 `finesseprobe` 均 `finesse-fixture OK`、对比度 0 violations。
- 本阶段构建仍有 Contracts/Core 各 1 个 NU1900 网络漏洞源告警；未将离屏 `DpiScale=1.00`、`FontActualGlyphRun=unknown` 或探针通过扩大为真实 Playnite 小窗口、物理 DPI、IME、宿主字体或最终视觉通过。一键安装/启停链未因安全策略重跑。

## 2026-09-14 UI 精修 Q03-07 主题切换覆盖

- 修正 `UiDiagnosticsExporterTests.ThemeResourceSwitchReplacesStateBrushesWithoutLeavingStaticFallbacks` 的自比较假阳性：现在实际比较 Light/Dark 选中前景、按钮渐变颜色、渐变长度和画刷实例；新增 `UiFinesseRound2ControlSourceTests.TransientSurfacesKeepThemeSensitiveResourcesDynamic`，锁定 Popup/Tooltip/Dialog 动态资源与生产壳层向所有生产工作区广播运行时资源。测试提交为 `46ece40`。
- 相关定向测试 `14/14` 通过；Popup 动画/阴影/透明度、Tooltip 前景/浮层背景和 Dialog 效果都由 `DynamicResource` 读取，主题刷新继续保持在局部视图资源，不污染 Playnite 全局字典。
- 打开态热切换的真实屏幕帧、Popup 跨屏定位、宿主主题跟随和物理 DPI 仍未取得，不把资源测试或离屏夹具写成 Q03-07 最终完成。

## 2026-09-14 精修验收口径修正与第二轮

- 第一轮不能称全部完成：原账本 14 已验收/1 已满足/31 待验收/6 阻塞；“52/52 已更新”只是登记完整。独立复核和下一轮入口分别为 `docs/design/UI_FINESSE_REVIEW_2026-09-13.md`、`UI_FINESSE_ROUND2_208_TASKS_2026-09-13.md`。52 项都有独立审阅结论，208 项按实现/自动/视觉/宿主分开记账。
- 暗色 `finesseprobe` PNG 出现黑色 Numeric、部分按钮/开关/状态文字，但报告仍 OK。需在共享生产入口修正最终 Foreground，并增加实际控件/合成对比及负例；当前 palette 11 项通过不代表所有控件可读。状态色小字应按 4.5:1；`FontHasGlyph` 只证明候选覆盖，不是实际 GlyphRun；Items.Count 不等于完整可读行。
- 时长资源读取、Caption=1、浅色 palette 与新夹具有真实代码成果，应保留；动效重入、100 次生命周期和运行中关闭动画仍需专项实现/证据，不能凭 IsEnabled 或 rapid-toggle 代理认定全局完成。
- 用户明确要求新建 5.6 Luna/max 任务持续实现更大精修计划。新任务默认独立 worktree，中文提交并推送自己的工作分支；主目录用户 `src.zip` 不动，干净 worktree 解决打包源码问题，安装/物理宿主条件分别判断，不无条件重装循环。

## 2026-09-13 UI 精修 P03 / P05～P11 收口记忆

- 当前精修基线为提交 `6c3c238b8ba0c7ce3a010e4234e18cfc34f10ee1`。P03 控件、P05 反馈/过渡、P07 页面、P08 逻辑尺寸、P09 状态/术语/可访问性均继续复用既有共享资源和真实命令绑定；没有引入平行 UI 系统。
- 可重复的受控证据：最终 `render-qa OK` 报告 `.tmp/ui-finesse-qa-final-20260913/`；`statefixtures OK` 覆盖 Ready/Empty/Loading/Error/Stale/Offline；`gridprobe OK` 覆盖 50/400/2000/4468 行及端点/分页；`scaleprobe OK` 覆盖 1000/5000/20000 backend 和 2000 media window；`thumbnailprobe OK` 覆盖 120 项、100 次窗口、peak=3、active=0、cache=96/96；`shellqa OK` 覆盖紧凑壳层和三类 PageHost。
- P06-03/P06-04 与 P08-02/P09-02/P11-02 的受控门禁已足够签收；P03、P05、P06-01/02、P07、P08-01/04、P09-01/03/04、P10-03、P11-01/04 记录为代码完成待验收，因为仍需真实宿主交互。P08-03、P10-01/02/04、P11-03 记录为外部阻塞。
- 所有报告都必须保留证据边界：离屏 `DpiScale=1.00` 是逻辑 DIP；rapid-toggle/maxFrameGap 是代理而非 presented-frame；L32 `ScrollIntoView` 离屏结果 inconclusive；synthetic thumbnail/隐藏 STA 不等同真实 Playnite 视频；专用审计窗口不等同嵌入 Dashboard。
- 真实宿主当前仍为 `MainWindowHandle=0`、无 `summary.json`，PNG 标记 `DedicatedAuditWindow` 且 `DashboardWasAlreadyHostedByPlaynite=false`。根目录 `src.zip` 是用户未跟踪文件，安全打包链不可绕过它；不要删除、移动或提交。
- 最终一键门禁已完成构建与测试：XAML `24/24`、Release `0/0`、Core `76/76`、Worker `311/311`、Playnite `455/512`（57 skip、0 fail）。包装阶段因本轮文档改动和 `src.zip` 使工作树非 clean 而按安全策略停止；以后若用户处理 `src.zip`，可从 `scripts/package.ps1` 重新生成签收包。
- 文档提交后已在提交 `cacdaff` 再次重试：Render QA 仍 `OK`；一键链只剩 `?? src.zip` 仍被打包门禁停止，真实宿主审计也因此只生成 runner metadata、没有 `summary.json`，不改变嵌入 Dashboard 的外部阻塞结论。

## 2026-09-13 UI 精修 P01/P02/P04 共享底座

- `Typography.xaml` 仍是唯一字体入口，实际机器盘点为 `Inter` 未安装、`Segoe UI Variable Text/Display`、`Noto Sans SC`、`Microsoft YaHei UI`、`Cascadia Mono` 和 `Consolas` 可用；固定夹具报告了 CJK→Noto Sans SC、Latin/数字/箭头→Segoe UI Variable Text，`𠮷`（U+20BB7）在显式优选链未命中。没有随包分发字体，因此跨机器一致性继续是待验收边界。
- Caption 共享样式已改为 `Opacity=1`，页面层级由语义色表达；正文/按钮/TextBox/代码/数字语义样式统一 Fixed hinting、`SnapsToDevicePixels` 和 `UseLayoutRounding`。新增 `GscTypographyCode`/`GscCodeText`，校对夹具路径使用 Code，Numeric 样本保留真实单位与变化。
- Adaptive palette 的浅色 Info/Success/Warning/Error 改为深色可读值；ContrastGuard 现在按最终合成测量 Secondary/Muted 4.5、OnAccent 4.5、状态色 3.0，并保留表面/控件的非文字门槛。双主题夹具 11 项均通过，Light 最低 Muted 4.854:1，Dark 最低状态/文字比值高于门槛。
- `MotionTokens.xaml` 是 XAML 权威时长 120/100/220/300ms；`GscMotion` 通过资源读取、宿主局部覆盖和无资源确定回退消费同一语义。定向 `UiFinesseFoundationTests` 8/8，完整 Playnite 代码测试 449 通过/63 跳过/0 失败。
- 当前阶段 RenderHarness Release 构建 0 warning/0 error，双主题完整 `render-qa OK`；rapid-toggle 代理探针 `settled=True`。这仍不等同真实 Playnite 帧时间、DPI 或物理交互。

## 2026-09-13 UI 精修计划与源码差异（P00 前置记录）

## 2026-09-13 UI 精修 P00 基线夹具

- P00-02 已完成：`UiFrameworkProbeView` 现在只在开发入口合并生产 `AcrylicProductionResources.xaml`，固定混排、路径、错误码、状态徽章、输入/选择、共享按钮状态、Toggle、图标按钮和 4 行有限 DataGrid 样本；RenderHarness `finesseprobe <output> <dark|light>` 在 1120×980 DIP 下双主题通过。
- 当前双主题原始证据位于 `.tmp/ui-finesse-probe-20260913-dark/` 和 `.tmp/ui-finesse-probe-20260913-light/`：均为 4 行、22 个按钮、`CaptionOpacity=1`、`finesse-fixture OK`。这是生产资源离屏校对证据，Pressed/Focus 的行为以及真实 Playnite 嵌入仍未验收。
- P00-03 仍是外部阻塞：真实宿主审计的 `MainWindowHandle=0` 且没有 `summary.json`；不循环重装替代真实取证。阶段账本和资源映射在 `docs/design/reviews/ui-finesse-20260913/`。

- 用户近期优先关注 UI 精细度、中英文字体、色彩和动画流畅度，已请求将详细实施提示词交给其他 Agent 读取。入口为 [UI_FINESSE_IMPLEMENTATION_PROMPTS_2026-09-13.md](../design/UI_FINESSE_IMPLEMENTATION_PROMPTS_2026-09-13.md)：12 阶段、52 项任务；本轮仅规划，未启动这些任务或新增安装授权。
- 后续 UI 精修沿用 Demo-first 和既有共享入口；优先完成 P00 基线、字体实际回退/Caption 对比、动效双来源及真实性能采样。所有视觉数值和性能阈值是待验收目标，不能写成已达成。U12 已完成内容作为依赖复用。
- 当前 `MotionTokens.xaml` Normal/Slow=220/300ms，`GscMotion.cs` 静态字段仍为 200/320ms，故历史“时长唯一来源”的描述是目标而非完整实现事实。P04-01 负责统一，本轮未改代码。
- `GscTypographyCaption` 使用 Muted 色并设 0.65 Opacity，存在二次变淡风险；ContrastGuard 的 SecondaryText 仅检查 3.0，不能作为普通小字 4.5:1 的完整证据。实际合成对比、字体命中和目标机呈现效果待 P01/P02 测量。

## 2026-09-13 开发提示词包全量门禁复核

- 已阅读并复核用户提供的完整 WPF/Playnite 开发提示词包及其 Phase 0～12、构建安装和最终报告要求；当前实现以 Demo-first 现有共享资源为视觉真值，不新增第二套 UI 框架或业务层。
- 当前 HEAD `108ae0f` 的 `scripts/render-qa.ps1 -Configuration Release -Output .tmp/ui-prompt-qa-final2-20260913` 返回 `render-qa OK`，覆盖双主题、多尺寸、多页面、状态/空错误夹具、表格虚拟化滚动、Resize 恢复和性能探针；WPF 静态审查无 error。
- 报告的工作树非 clean 是因为现有根目录 `src.zip` 未跟踪文件，不能擅自删除或提交。Playnite 已退出后完成安全安装，安装验证报告为 `artifacts/last-dev-install-current-20260913.txt`，日志确认插件版本 `0.6.73` 已加载。
- 真实宿主审计输出为 `artifacts/ui-host-audit-current-20260913`；由于当前会话仍无可枚举主窗口（`MainWindowHandle=0`），没有 `summary.json`，嵌入 Dashboard、用户主题、物理 DPI、键盘/滚轮和大库帧率继续保持未验收状态。

## 2026-09-13 GPT UI 契约与发布包复核

- 用户补充的 GPT UI 方案已完成与现有资源的契约对齐，不新增第二套玻璃、按钮或字体系统。`DesignTokens.xaml` 现在提供无前缀间距别名、状态字形转换器及 52 DIP 行/42 DIP 表头；Typography、Redesign、ButtonStyles 提供对应语义别名并继续复用生产模板。
- `StatusGlyphConverter` 是纯显示层转换：成功/失败/警告文本分别增加 `✓/×/⚠`，未知状态和已有字形原样返回；任务、存档和维护状态胶囊接入后不改变状态字段、命令或绑定。MotionTokens 的 Normal/Slow 为 220/300ms，维护表格最小高度 260 DIP；任务紧凑表格仍保留 236 DIP 以避免详情抽屉被挤压。
- 验证基线：Release `dotnet build` 0 warning/error；Core `76/76`、Worker `310/311`（1 skip）、Playnite `447/510`（63 skip、0 fail）；`validate-source.py`、`check-xaml.ps1` 与 RenderHarness `render-qa OK` 全部通过。真实 Playnite 的嵌入 Dashboard、宿主 DPI/主题、物理滚轮和键盘仍按真实宿主审计边界处理。
- 历史正式包来自干净 HEAD `b470bf9`（`.pext/.zip` 均为 `43,809,091` 字节，SHA-256 `9E6F4BB11B812DB824D8E3EA4EBFCD45A2490FEA5C907996301ABD9F21A55F8A`）；本轮已在 Playnite 退出后完成当前源码快照的安全安装，安装 DLL 身份为 `0.6.73+a81403478539ec1036aa135198c88e752e7d41d8`，日志确认插件加载成功。

## 2026-09-13 真实宿主审计复跑边界

- 当前 HEAD `d59a6a6` 已通过 `scripts/real-host-audit.ps1` 的 Release 构建、安装和启动流程：构建 `0 warning / 0 error`，Core `76/76`、Worker `311/311`、Playnite `444 passed / 57 skipped / 0 failed`；日志确认 Playnite 加载 `GameSaveCenter 0.6.73`。
- `artifacts/ui-host-audit-live-20260913` 保存 1366×768、1600×1000、最大化、浅/深主题的受控窗口矩阵，metadata 记录 150% DPI 和 `RealFixedLayoutOverflow=[]`。这些证据可用于确认当前程序集及新增 UI 在专用宿主窗口的渲染状态。
- 自动 UIAutomation 仍无法定位 Playnite GameSaveCenter 侧栏（`MainWindowHandle=0`），未生成 `summary.json`；PNG 的 `CaptureOrigin` 全为 `DedicatedAuditWindow` 且 `DashboardWasAlreadyHostedByPlaynite=false`。不得把本轮受控截图写成真实嵌入 Dashboard、物理 DPI、键盘或滚轮验收。

## 2026-09-12 合并 UI 任务队列

- 后续 UI 排期以 `docs/ai/UI_CONSOLIDATED_BACKLOG_2026-09-12.md` 为入口：它将 D12 显示复核和用户提供的 GPT 建议映射为 U12-00～U12-10，避免重新实现已存在的令牌、按钮、图标、字体、玻璃或动效系统。
- GPT 类建议必须先映射到当前共享入口和 Demo-first 基准；不修改 ViewModel、命令、Worker、备份、数据库或业务行为。U12-10 的离屏/宿主验收贯穿其他所有 UI 项。
- U12-01 的直接主题化 1366×768 内容区仅有 528 DIP；`MediaInboxPageScrollViewer` 必须在低于 560 DIP 或收件箱过期时承担页面级滚动，避免有限、虚拟化的 `MediaInboxGrid` 从四行阅读底线跌至三行。紧凑高度可收起重复的 `MediaInboxInfoBand`，但不得隐藏批量命令、Inspector、历史、选择或页尾操作。
- 本轮完整 RenderHarness 已得到 `render-qa OK`（Release 构建 0 warning / 0 error）；它覆盖离屏双主题、多尺寸与表格视口，不构成真实 Playnite/DPI/键盘或宿主主题验收。
- `GscWorkspaceStatePresenter` 的 `FilterEmpty` 必须显示“无结果”而不是复用“空”，并保留绑定的清除筛选下一步；真正空数据、加载、错误、降级和离线状态仍由各页真实状态字段驱动。
- U12-09 审计证据保存在 `docs/ai/UI_ACCESSIBILITY_AUDIT_2026-09-12.md`；自动测试覆盖共享动效 token、减动效终态、焦点视觉、AutomationProperties、Tooltip 与状态 Presenter 键盘命中。真实 Playnite/FusionX、物理 DPI 和系统高对比度仍是人工边界。

## 2026-09-12 Premium Motion System

## 2026-09-12 一键构建测试契约同步

- `scripts/build.cmd` 必须作为完整交付门禁运行；它依次执行 XAML 检查、Release 编译和 Core/Worker/Playnite 三组测试。Worker 测试在禁并行模式下可超过一分半，不应因终端的短输出窗口误判为卡死。
- 高频图标操作的测试应验证共享图标按钮样式、`ToolTip`、`AutomationProperties.Name` 和 `ThemeAwareIcon`，不能把旧 `GscWpfUiCompactButton` 及固定最小宽度当作契约。
- 动效测试应验证 `GscMotion` 与 `MotionTokens.xaml`，而非重新断言页面中的毫秒字面量；共享 token 仍是唯一时长来源。

- 动效时长唯一来源为 `Themes/MotionTokens.xaml`：Fast 120ms、Press 100ms、Normal 200ms、Slow 320ms，统一 Cubic EaseOut。不要在页面或模板重新发明时长、Bounce、Elastic 或大幅位移。
- `Infrastructure/GscMotion` 是 UI 代码中动画 Transform 的唯一实例化入口；它会尊重用户动画设置、`SystemParameters.ClientAreaAnimation` 与高对比度，并为 Style/资源冻结的 Freezable 生成实例级副本。不要恢复共享资源 Transform 的直接动画。
- 允许的反馈限于 opacity、轻微 translate/scale、图标旋转与选择状态；不得把 Width、Height、Margin、GridLength 或 DataGrid/虚拟列表行作为常规高级动效。侧栏伸缩是既有导航例外，仍须使用 `GscMotion.Normal` 和终态规范化。

## 2026-09-11 图标按钮收口

- `Themes/GscIconButtonPack.xaml` 只包含由用户提供图标包改写的 Geometry；颜色必须继续由主题资源决定，不能把 PNG/SVG 颜色或硬编码色值带回生产 XAML。
- `GscIconOnlyButtonBase`、Toolbar、Accent、Danger 位于 `WpfUiProduction.xaml` 中 `GscWpfUiButton` 之后。图标按钮必须保留 `ToolTip`、`AutomationProperties.Name`，且基础样式必须设 `ToolTipService.ShowOnDisabled=True`；不要为压缩空间而移除禁用操作的说明。
- 图标按钮仅用于刷新、重试、删除、复制、取消、折叠等低歧义且高频的上下文操作；不要仅因操作短就隐藏文字。扫描、校验、打开具体媒体或目录、归类、应用策略及其他存在目标/后果差异的动作必须保留可见文字。主 CTA、保存及需确认/复杂语义操作同样保持文字；图标按钮实例继续复用 `ThemeAwareIcon` + `GscLineIcon`。
- Dashboard 的 `SetToolbarLabelsVisible` 只管理仍含文字的顶栏操作；新增图标专用顶栏按钮时，必须同时从其空引用检查、可见性切换和宽度覆写集合中排除，避免生成字段缺失或覆盖 36 DIP 尺寸。

## 2026-09-11 表格/字体迁移测试契约

- `OvernightClosureV6Tests.MaintenanceHeadersUseSharedThemeResources` 必须验证共享表头为透明、无底部分隔线，不能再断言 `GscTableHeaderBrush` 出现在 Maintenance 表头实现中。
- `RestoredAcrylicForkBaselineTests.WorkspaceDiagnosticTextUsesTheSharedCascadiaMonoFallback` 的 `GscCodeFontFamily` 来源是 `Themes/Typography.xaml`，不是 `DesignTokens.xaml`；维护页仍通过 DynamicResource 使用它。

## 2026-09-11 无缝表格与字体系统

- 所有生产 DataGrid 的表头必须与其 DataGrid/表格框共用同一连续阅读表面：`GscDataGridColumnHeaderStyle` 与 `DataGridColumnHeadersPresenter` 保持透明、无底部分隔线；任务、媒体、维护和兼容 Dashboard 不得局部恢复 `GscTableHeaderBrush` 或 `0,0,0,1` 表头描边。行间弱分隔、状态胶囊、排序箭头和列拖拽热区仍可保留。
- `Themes/Typography.xaml` 是唯一字体/字号语义入口，`DesignTokens.xaml` 合并它以覆盖所有视图；当前目标机实际可解析的优选/回退链为 Inter（未安装时跳过）、Segoe UI Variable、Noto Sans SC、Microsoft YaHei UI，代码链保留 Cascadia Mono/Consolas。页面标题 22 SemiBold，Section 16 SemiBold，正文 14 Regular，辅助 12 Regular；数字使用 `GscNumericFontFamily`。不要新增 `FontWeight=Bold`，新的 TextBlock/Button/TextBox 使用隐式基样式或 `GscTypography*`/现有 Gsc 语义样式。
- `Redesign.xaml` 也要直接合并 Typography：其中的页面/Section 样式通过 `StaticResource` 解析，不能只依赖 DesignTokens 的嵌套合并，否则真实 WPF 视图构造会报找不到 `GscTypographySectionTitle`。完整 WPF 资源测试已覆盖该资源链。

## 2026-09-11 任务窄窗与设置主题同步

- 任务中心筛选栏的紧凑断点为 980 DIP，不能退回 760：760–980 DIP 时完整筛选行的 Auto 列会挤压搜索框，视觉上像右缘缺失。紧凑行保留搜索、状态与刷新，类型、历史范围和时间范围必须仍可通过“更多筛选”完成。
- 设置和生产壳共享持久化 `ThemeMode` 与 `VisualSettingsChanged`；设置页应用主题时必须先使用 `AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources` 注入完整公共令牌，再调用 `ApplySettingsMaterialResources` 覆写无选中游戏背景的结构表面。不得恢复手工列举一小部分资源的方式，否则新令牌会与主界面漂移。

## 2026-09-11 按钮顶部横线回归修复

- 真实任务中心截图中的白色长横线由 `GscWpfUiButton` 的 `ButtonHighlight` 和原生 `GscButtonBase` 的 `TopHighlight` 两个独立 1 DIP 高光层造成；它们与设计概念图不符，已从两条模板和 Primary 模板完全移除。不得以独立横向 `Border` 重新实现按钮高光。
- 玻璃按钮的可见层次只保留透明渐变、低对比轮廓、Hover/Pressed 覆层和 Primary 的主题辉光；所有命令、Binding、键盘焦点、Disabled 和模板实例级按压缩放不受影响。定向资源测试 `2/2` 与 Release Playnite 构建通过；真实宿主重启后应复核横线不再出现。

## 2026-09-11 Glass 材质层收敛

- 普通生产按钮的最终视觉由 `AdaptiveThemePaletteFactory.ApplyAccentResources` 的 `GscButtonGlassBrush` 决定，不能只改 `DesignTokens.xaml` 静态 fallback；运行时深色为 `60%→50%`、浅色为 `66%→58%`，高对比度/关闭玻璃仍是完全不透明的安全回退。
- `GscWpfUiButton` 和 `GscButtonBase` 统一使用 14 DIP 圆角及顶部 1 DIP 高光；共享 `ui:Button` 已保留 Hover、Focus、Disabled 与模板实例 `0.97` pressed scale。`GscWpfUiToggleSwitch` 已覆盖生产设置页与工作区，不应新建平行 Toggle 控件。
- Primary 的 `GscPrimaryButtonEffect` 仅在玻璃可用时使用 18 DIP、零偏移、0.40 opacity 的主题强调色阴影；表格/卡片边界应使用 `GscGlassStrokeBrush` 的低对比层，避免恢复为 `GscControlStrokeBrush` 的输入框级硬边。Release 构建和定向 `WpfUiResourceDictionaryTests` 已通过；真实 Playnite/FusionX、DPI 和用户背景材质仍需人工验收。

## 2026-09-10 旧安装包导致按钮视觉未更新

- 用户复核按钮无变化后，必须先比较 `src/.../bin/Release/net462/GameSaveCenter.Playnite.dll` 与 `%APPDATA%\Playnite\Extensions\GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec\GameSaveCenter.Playnite.dll` 的哈希；本次两者不同，且实际目录的 DLL/icon 时间明显更旧，证明宿主加载的是旧安装包。
- 已用 `scripts/dev-install-run.ps1 -Configuration Release -NoStart` 从当前 HEAD `c757ffe` 完成构建、测试与原子安装。安装后包 staging 与实际扩展目录的插件 DLL、Worker DLL、共享程序集、清单和 `icon.png` 逐项 SHA-256 一致，构建身份为 `0.6.73+c757ffe5ff8b678d69cdca1386cf89be10f7a831`。
- 下一次视觉复核前必须完全重启 Playnite；仅重新打开插件页面不能替换已经加载到进程内的旧程序集。不要把离屏 RenderHarness 的截图当作真实宿主已经加载新 DLL 的证据。

## 2026-09-10 共享按钮实际生效与 Playnite 侧栏图标

- 上一轮的 `ButtonStyles.xaml` 别名没有被现有页面引用，不能作为“按钮已改版”的证据；后续按钮视觉修改应优先落在 `WpfUiProduction.xaml` 的 `GscWpfUiButton` 共享基础模板，当前默认表面使用 `GscGlassFillBrush` / `GscGlassStrokeBrush`，Primary/Danger 复用 `GscPrimaryButtonEffect` / `GscSurfaceEffect`，避免逐页替换样式名。
- `GameSaveCenterPlugin.GetSidebarItems()` 的 `SidebarItem.Icon` 可以接收 WPF `Path`。当前使用 ZIP 中 `plugin-main.svg` 的几何线稿，并将 Stroke 设置为 Playnite 的 DynamicResource `GlyphBrush`；这样由宿主主题控制黑/白色，不能改回静态固定色或彩色填充图标。扩展清单的 `icon.png` 已替换为 ZIP 的 `plugin-main-256.png` 透明黑色线稿，静态清单场景本身不承担主题切换，运行时侧栏 Path 才是主题感知实现。
- 代码阶段提交 `83dc1c4` 已通过 Release 构建、全量测试和 WPF/XAML/source 门禁；离屏证据为 [`.tmp/icon-button-qa-clean-20260910/render-qa-report.txt`](../.tmp/icon-button-qa-clean-20260910/render-qa-report.txt)，报告记录 `WorkingTreeClean: True` 与 `render-qa OK`。真实 Playnite 侧栏主题切换、FusionX 具体模板、用户 DPI/主题和物理截图仍需人工验收；本轮已完成扩展目录安装，但使用 `-NoStart`，不能把安装验证写成宿主已经重新加载并显示。

## 2026-09-10 主题感知线性图标包

- 用户提供的 `GameSaveCenter_IconPack_v2_round-flat.zip` 的 SVG 是 24×24、透明底、`currentColor`、低线密度线稿；WPF 生产端通过 `Controls/ThemeAwareIcon.cs` + `Themes/GscIconPack.xaml` 的 Geometry/Path 渲染，不能改回依赖 Segoe MDL2 字形或引入只支持单色/固定底图的 PNG。
- `GscLineIcon` 是共享样式，Path 的 Stroke/可选 Fill 绑定控件 `Foreground`；导航图标从 RadioButton 前景继承，状态图标继续使用 `Gsc*Brush` 语义色。资源由 `WpfUiProduction.xaml` 统一合并，页面不应逐个复制图标 Geometry。
- 当前接入范围包括生产导航与侧栏品牌、设置标题/分组（含常规与目录、设置迁移）、首页活动状态、媒体来源、维护目录、修改器列表、任务搜索、共享游戏图标 fallback、Disclosure chevron 及隐藏兼容 Dashboard 的对应操作；真实命令、Binding、分页/虚拟化、焦点和自动化名称未改。
- 压缩包中的 `plugin-main-256.png` 已覆盖 `src/GameSaveCenter.Playnite/icon.png`；它是透明黑色线稿，运行时侧栏主题感知颜色使用 WPF `Path` + `GlyphBrush`。
- 本轮又补齐 ZIP 中 `section-general-directory` 与 `section-migration` 到设置页对应标题；Release Playnite 构建 `0 warning/0 error`，Playnite `432/495`（432 通过、63 跳过、0 失败），源码/XAML 门禁通过；`.tmp/icon-qa-settings-icons-20260910/render-qa-report.txt` 为双主题、多尺寸 `render-qa OK`。这是离屏 WPF 证据，不能写成真实 Playnite 用户主题/DPI/清单图标已验收。

## 2026-09-10 统一 Glass 按钮组件语义

- 用户粘贴的 Glass UI 方案与现有生产架构的等价实现是 `ui:Button` + `GscWpfUiButton`，因此新增 `Themes/ButtonStyles.xaml` 作为共享语义入口，而不是再造一个会与 Playnite/WPF-UI 冲突的 `GlassButton` 模板。别名覆盖 `GlassButton`、`PrimaryGlassButton`、`DangerGlassButton`、`IconButton`、`SegmentedButton`。
- `GscWpfUiButton` 现在支持 `Secondary`、`Primary`、`Danger`、`Disabled`；按压状态以模板实例级 `ScaleTransform` 缩小到 `0.97`，继续保留 `FocusVisualStyle`、内容模板、命令和现有布局约束。主题颜色全部使用现有动态令牌，未对滚动内容添加 `BlurEffect`。
- 证据：Release 构建 `0 warning/0 error`，Core `76/76`、Worker `310/311`（1 skip）、Playnite `433/496`（63 skip、0 fail）；提交后 `.tmp/button-system-qa-20260910/render-qa-report.txt` 为双主题、多尺寸 `render-qa OK`。这是离屏 WPF 证据，不能写成真实 Playnite 用户主题/DPI/安装验收。

## 2026-09-10 共享表格内容视口固定到顶部

- 用户现场仍能看到待归类表格列头下方的大块空白，因此仅靠 `DataGrid` 的 `VerticalContentAlignment=Top` 和宿主属性绑定不够；Playnite/FusionX 的短窗口测量或滑块端点可能重新安排真实 `ItemsPresenter`。
- `Redesign.xaml` 的共享 DataGrid 模板现将外层 `PART_ScrollContentPresenter` 和外层 ScrollViewer 的 `ItemsPresenter` 显式设为横向拉伸、纵向顶部对齐，保证虚拟化行在有限内容视口的首端布局；不会改变命令、Binding、Selection、Item 滚动或 Media 大数据例外。
- 证据：`MediaWindowAnchorContractTests` `10/10`；`.tmp/qa-table-scroll-20260910-final/render-qa-report.txt` `render-qa OK`。离屏证据使用 1.00 DIP，不能替代真实 Playnite 窗口化/2K 与非 100% DPI 复验。

## 2026-09-10 设置窗口主题优先级与生产导航对齐

- 设置页是 Playnite 的 `UserControl`，实际可能位于独立设置 `Window`。`AdaptiveThemePaletteFactory` 的 FollowPlaynite 资源解析必须先检查当前 Window 及 Owner 链，再检查控件局部视觉资源和 `Application.Current`；不能让 `TryFindResource` 先穿过应用范围拿到旧深色值。
- 这一优先级只影响 FollowPlaynite；`GameSaveCenterThemeMode.Light/Dark` 仍是用户明确选择的覆盖模式。窗口资源不存在时，控件本地资源和应用资源仍提供兼容回退。
- `AcrylicNavItem` 与生产壳层七个导航内容组均显式 `VerticalAlignment=Center`；图标和文字保持同一水平行，收起状态只改变可见性和内容对齐，不改变导航命令。

## 2026-09-10 Media 待归类表格内容视口对齐

- 用户反馈的“滚到底后中下部有数据、上部空白”不是 Media 专属数据分页或 `Standard` 虚拟化本身造成；共享 `GscRedesignDataGridTemplate` 的 `ScrollContentPresenter` 原先未绑定 `HorizontalContentAlignment` / `VerticalContentAlignment`，导致 `VerticalContentAlignment=Top` 没有落到真实内容视口。
- `Redesign.xaml` 已为外层 `DG_ScrollViewer` 和内层 `PART_ScrollContentPresenter` 补上两项 `TemplateBinding`，使 DataGrid 的 `Top` 对齐贯穿真实内容视口。Media 的大数据保护策略仍原样保留：`VirtualizationMode=Standard`、`ScrollUnit=Item`、`EnableColumnVirtualization=False`、`DataGridStarFill.Enabled=False`。
- `MediaWindowAnchorContractTests` 增加源契约断言；定向契约测试 `18/18`，最终全量 Release 回归为 Core `76/76`、Worker `310/311`（1 skip）、Playnite `429/492`（63 skip），0 失败，`render-qa` 报告为 `OK`。这只证明离屏 WPF 端点和模板结构，真实 Playnite/FusionX 的物理拖动仍需人工验收。

## 2026-09-10 页面外围重复环境材质矩形

- 第二张图中的页面外围矩形来自页面级 `AmbientMaterialLayer` 的圆角宽域渐变，不是需要再次铺开的图片层；Shell 已经拥有跨侧栏、页面和 footer 的单一环境坐标系。
- `AmbientMaterialLayer` 新增 `IsShellLayer`。页面实例保持挂载但通过 `GscAmbientPageOpacity=0` 透明；Shell 实例标记为 `True`，内部洗色保持可见并由外层 `GscShellAmbientOpacity` 控制。不要重新让页面层和 Shell 层同时绘制宽域洗色。
- 本轮资源/壳层定向测试通过，Release 全量为 Core `76/76`、Worker `310/311`（1 skip）、Playnite `429/492`（63 skip），构建 0 warning/0 error；干净 RenderHarness 报告 [`.tmp/qa-table-ambient-20260910/render-qa-report.txt`](../../.tmp/qa-table-ambient-20260910/render-qa-report.txt) 为 `fc3c7d5`、`WorkingTreeClean: True`、`render-qa OK`，真实宿主截图和 DPI 仍是验收边界。

## 2026-09-10 生产壳层整页游戏背景与边界缝

- 图二右侧页面/侧栏后方的长方形确实来自选中游戏背景 `ImageBrush`。现有图片层和 `ShellAmbientMaterialLayer` 均为 `Grid.Row=0`、跨两列、跨两行，背景已经是整壳层层级；问题在于外壳 `Margin=4`、侧栏右 `Margin=6`、footer 独立描边/边距形成了视觉缝。
- `AcrylicProductionShellView.xaml` 现在移除这些外层缝隙和独立描边：`DemoShell` 零边距/零边框，`SidebarSurface` 零边距并保留左圆角，`FooterSurface` 跨完整宽度且无独立边框。背景仍被半透明 sidebar/page 表面覆盖，避免原图干扰文字。
- `ProductionShellChromeSourceTests` 增加整壳层背景契约；RenderHarness 双主题及 1040/1100/1366 Shell Media 几何回归通过。离屏没有真实 Playnite 图片资源，不能替代宿主截图。

## 2026-09-10 游戏选择器视觉树运行时回归补强

- 新增 `WpfUiResourceDictionaryTests.GameContextButtonKeepsCompositeGridContentThroughItsRuntimeTemplate`，在 STA WPF 线程中解析生产资源并实际套用 `GscRedesignGameContextButton`，测量/排列后验证 `ContentTemplate={x:Null}`、`Chrome` 模板内的 `ContentPresenter` 仍持有原始 `Grid`，从运行时层面覆盖 `System.Windows.Controls.Grid` 字符串化风险。
- 定向测试 `1/1`；完整 Release 回归为 Core `76/76`、Worker `310/311`（1 skip）、Playnite `428/491`（63 skip），失败 `0`。这是测试覆盖增强，未改变生产程序集，`248d28e` 候选包仍为当前生产候选。

## 2026-09-10 当前生产源候选包与发布边界

- 当前候选包从 HEAD `8657654` 重新生成；六份程序集统一为 `0.6.73+8657654310d99e349c120f3bb0484b65ac9f4dc2`，两个候选包均为 `43,837,912` 字节，SHA-256 为 `E136B5C6465A5A8933C72B0F9707CFF64D91A02D35115B9A007BF6A749B62AB0`。
- 发布链验证为 Core `76/76`、Worker `310/311`（1 skip）、Playnite `428/491`（63 skip），构建无警告/错误；生产视觉修复来自 `248d28e`，`8657654` 只增加游戏选择器运行时模板测试，候选包仍包含首页活动行、云端整卡、顶部复合按钮和活动游戏选择器模板防护。
- staging/隔离构建目录已清理；包仍未安装真实 Playnite，不能把最新包身份校验写成 FusionX、DPI、物理点击或视频验收。
- 包后又执行 `python scripts/validate-source.py`、`scripts/check-xaml.ps1` 和 `git diff --check`，均通过；这些是源码/交付门禁，不替代 L31 真实宿主证据。
- 2026-09-10 在当前 `main` 重新执行 `dotnet test GameSaveCenter.sln -c Release --no-restore -m:1`，Core `76/76`、Worker `310/311`（1 skip）、Playnite `428/491`（63 skip），失败 `0`；新增运行时模板测试确认游戏选择器复合 `Grid` 没有被文本模板转换为类型名；此前 `AsyncThumbnailLoader` 的 `120`/`122` 并发污染本次未再复现。
- 最新 `render-qa-head-clean-20260910/render-qa-report.txt` 的报告提交元数据为 `5c53e59`，`WorkingTreeClean: True`、`render-qa OK`；`5c53e59` 只改文档，生产 XAML 与 `8657654` 候选包一致；覆盖双主题、多尺寸、resize、云端筛选前景、完整壳层背景和 Media 页尾几何，仍属于离屏证据。
- L32 文档链接审计共检查 123 个本地 Markdown 链接，缺失 0；旧临时截图不恢复，工作日志改指向当前证据索引。

## 2026-09-09 首页活动和云端队列整卡交互验证

- `4001d9d` 增加真实 WPF STA 行为测试，不再只依靠 XAML 字符串断言：`OverviewView` 完成测量/排列后，活动按钮内容保持 `Border`，云端卡片内容保持 `StackPanel`，共享按钮的文本模板不会把视觉树显示成类型名。
- 测试通过反射调用 WPF `ButtonBase.OnClick` 的框架点击路径，确认 `OpenCloudQueueCommand` 恰好执行一次；因此“查看明细”被移除后整卡入口仍有行为覆盖。
- 本地验证为 Release 构建 `0 warning / 0 error`、Playnite `427/490`（63 skip、0 fail）。测试夹具不改变生产包；真实 Playnite/FusionX 主题、DPI、鼠标点击和录屏仍不可由该测试代替。

## 2026-09-09 首页活动行可视树与云端队列整卡导航

- 共享 `GscWpfUiButton` 默认通过 `GscWpfUiButtonTextTemplate` 将 `Content` 绑定到 `TextBlock.Text`；当首页全局活动行把 `Border/Grid` 作为按钮内容时，会出现 `System.Windows.Controls.Border`。页面局部的 `OverviewActivityRowButton` 必须设置 `ContentTemplate={x:Null}`，不能修改为关闭全局按钮模板或删除活动命令。
- 云端队列指标采用 `OverviewCloudQueueCardButton` 作为唯一交互面，直接绑定 `OpenCloudQueueCommand`，不再在卡片内部嵌套“查看明细”按钮。保留 `AutomationProperties.Name`、焦点样式、悬停动效和提示。
- 这轮离线证据为 Playnite `426/489`（63 skip、0 fail）、Release `0 warning/0 error`、RenderHarness `render-qa OK`、XAML `19/19`、WPF 静态审计 `0 errors / 22 warnings / 157 info`；真实 Playnite/FusionX、用户主题/DPI 和人工点击仍须宿主验收。

## 2026-09-09 Media 离屏视口门禁按实际行几何收口

- `d777e65` 修正了离屏 RenderHarness 的 Media 误报：DataGrid 不再用固定 `236 DIP` 作为主表通过条件，而是统计行容器相对 DataGrid 边界的完整可见数量；有至少 4 条数据时要求 `4/4` 行完整可读。`230 DIP` 的独立 Media 表格因此得到 `readableRows=4/4`，没有修改生产表格高度或关闭虚拟化。
- `MediaClassificationPreviewItems`/`MediaClassificationHistoryList` 属于媒体 Inspector 内的小型预览/历史列表，分别只有 2/3 条夹具内容，并受 Inspector 自己的滚动容器管理；它们不适用主工作区四行门禁，但报告仍记录尺寸和条目数。
- Resize 探针改为向 Media 传入 `ContentSize` 的 `contentH`，与生产 PageHost 测量一致；原先 `86 DIP` 的错误结果恢复为 `300 DIP`、`6/4` 完整可读行。提交后的报告为 [`.tmp/render-qa-media-gate-clean-20260909/render-qa-report.txt`](../../.tmp/render-qa-media-gate-clean-20260909/render-qa-report.txt)，`WorkingTreeClean: True`、`render-qa OK`。
- 这只完成了离屏质量门禁，不证明 Playnite/FusionX 真实模板链、DPI、用户主题或视频式拖动问题已解决；宿主证据边界保持不变。

## 2026-09-09 当前回归基线与宿主边界同步

- 当前 `main` 交接基线为 `580a70f`。此前缩略图并发测试的 `120/122` 失败已由 `c0197e5` 通过测试集合隔离修复；Playnite 全量为 `425/488`（63 skip、0 fail）。
- `580a70f` 只同步回归证据与离屏审计边界，没有继续修改生产表格模板、滚动单位或虚拟化。真实 Playnite/FusionX 仍没有可绑定窗口，不能把 `render-qa OK` 写成视频问题已解决。

## 2026-09-09 回归失败与离屏审计夹具修正

- `AsyncThumbnailLoader` 的诊断、缓存和并发闸门是进程级静态状态；`AsyncThumbnailLoaderTests` 在 `ResetDiagnostics()` 后若与 `AsyncThumbnailImageTests` 并行，观察到的 `RequestCount=122` 不代表生产代码多发请求。`c0197e5` 将两个测试类放进同一个禁并行集合，保留原有 `120` 精确断言和生产 3 路/96 项边界；定向 `1/1`，Playnite 全量 `425/488`（63 skip、0 fail）。
- `559d64f` 修正 RenderHarness 的三类误报来源：Settings 使用可访问的临时目录、Sidebar 完成探针改为不会被 Render 优先级 tick 饿死、Media 主题响应式探针传入实际 PageHost 高度。干净报告为 [`.tmp/render-qa-harness-clean-20260909/render-qa-report.txt`](../../.tmp/render-qa-harness-clean-20260909/render-qa-report.txt)，构建 `0 warning/0 error`。
- 修正后 Settings normal/dirty/invalid 和 Sidebar rapid-toggle 通过；生产壳层 Media 1040/1100 表格为 `300 DIP`，页尾 footer/history/secondary 可达。render-qa 仍有嵌套归类预览 `126 DIP`、独立 Media 表格 `230 DIP` 和 resize `86 DIP`，这些离屏组合尚未证明生产问题，也不能被直接删除门禁；真实宿主/FusionX 与视频式操作仍待验收。

## 2026-09-09 浅色主题背景与云端队列文字对比度修复

- 生产壳层同时存在全壳层选中游戏 `ImageBrush` 与内容列 `UseSelectedGameBackground=True` 的 `AmbientMaterialLayer`；后者会形成截图中侧栏右边及最右侧的矩形图片边界。修复只保留全壳层图片，Shell ambient wash 跨两列且 `UseSelectedGameBackground=False`，页面局部材质仍可继续提供页面光晕。
- `MaintenanceView.xaml`/`DashboardView.xaml` 的 `GscComboBoxLongText` 显式设置 `Foreground` 和 `TextElement.Foreground` 为 `GscPrimaryTextBrush`。这是对 `BaseTextBlockStyle` 覆盖 ComboBox 模板文字绑定的收口，不改变 ItemsSource、SelectedItem、Popup、选择或截断语义。
- 验证边界：定向契约测试 `2/2`，源校验和 XAML `19/19` 通过，WPF 静态审计 `0 errors`；RenderHarness 构建成功但完整 render-qa 仍报告既有失败。没有真实 Playnite/FusionX 窗口，因此不得把离屏图片或静态契约写成宿主视觉通过。

## 2026-09-09 L42/L41 真实宿主启动边界

- L42 运行基线为 `398a6f0`。L41 受限启动的 `cef.log` 记录 CEF Mojo channel `拒绝访问 (0x5)`，Playnite 随后退出；L42 在提升权限下完成 Core `76/76`、Worker `311/311`、Playnite `431/488`（57 skip），但 Playnite 进程保持 `MainWindowHandle=0`，UI Automation 无法找到侧栏，最终没有 `summary.json` 或表格 replay。
- L42 只证明当前宿主入口没有形成可绑定的真实窗口，不是视频表格异常的复现，也不是修复通过证据。由审计启动的 Playnite 进程已停止，用户 FusionX、全局 Playnite 样式和用户数据目录未修改。
- 当前真实嵌入表格证据仍是 L39：实际 `ScrollViewer`/`DataGridRowsPresenter`、`CanContentScroll=True`、`ScrollUnit=Item`，两表程序化端点回放的尾项完整、异常计数为 0。物理视频操作和问题 B 仍待宿主人工验收；没有诊断日志前不选择某个模板、偏移或锚点原因。
- 详见 [`L42_REAL_HOST_SCROLL_REPLAY_2026-09-09.md`](L42_REAL_HOST_SCROLL_REPLAY_2026-09-09.md)。

## 2026-09-09 L40 真实宿主捕获未形成新的嵌入滚动证据

- L40 运行代码基线为 `c84107b`，构建身份为 `0.6.73+c84107b2f79870133707ed01abd22c521e87e070`。L40 隔离 Playnite 最终 `EmbeddedDashboardCaptured=false`、`ControlledDashboardCaptured=true`、`ProductionVisualSourceOfTruthAvailable=false`，所以不能覆盖或升级 L39 的真实嵌入证据。
- L40 启动前修正了隔离配置残留的旧 Worker 绝对路径；这只修复审计环境身份问题。L40 没有新的真实嵌入 `scroll-replay` JSON，受控窗口截图不能当作生产表格滚动通过。
- b100913 的 RangeValue 滑块等效路径曾在 WPF provider 内抛出 `NullReferenceException`；c84107b 对开发审计调用做异常隔离并继续执行。该路径不是生产滚动逻辑，也不能替代真实鼠标拖拽。
- 最新回归为 Core `76/76`、Worker `310/311`（1 skip）、Playnite `425/488`（63 skip），构建无警告/错误。L39 的真实嵌入端点结果仍有效：媒体 400 条、任务 50 条、各 47 样本、底部尾项完整、异常计数为 0。
- 真实物理滑块、滚轮/PageUp/PageDown/Ctrl+End、主题/DPI 矩阵、加载更多后的锚点和视频录屏仍必须标记为宿主人工待验收；不能因为程序化端点回放或源码契约测试而写成“已解决”。

## 2026-09-09 L39 真实宿主回放未复现视频 B 类异常

- L39 运行代码基线为 `a9b8bce`；L39 包身份为 `0.6.73+a9b8bcec0c05f7d548d8160119b2b29e9698b1ab`。隔离真实 Playnite/FusionX 嵌入采集成功，Dashboard `1365.33×868 DIP`、DPI `1.5`。
- `MediaInboxGrid` 400 条和 `TaskGrid` 50 条各完成 47 个程序化回放样本，包含顶部/底部、20 次上下端点往返、水平端点和尾项选择。两表底部尾项均完整：Media `offset=394/scrollable=394`、Presenter `0,36,604×279.33`、尾行 `399@256..300`；Task `40/40`、Presenter `0,36,637.33×456`、尾行 `49@432..476`。尾项 cell/visual/text 为 `5/5/5`、`6/6/6`。
- 回放 JSON 的空正文、大间隙、末端水平条覆盖、选中内容缺失和尾项不完整均为 0。该证据证明当前模板端点范围和末行视口避让在真实宿主中通过，但不证明物理滑块拖动期间的 B 类视频问题已解决。
- 任务审计必须保留 50 条页面，不调用加载更多；当前任务分页命令会替换页面并保留 `HasMore=true`，重复调用会造成无限分页回放，不能把它误记为滚动异常。
- CUA 原生应用仍为 `apps: []`，真实鼠标滑块、滚轮/键盘矩阵、DPI/主题矩阵和录屏保持宿主人工待验收。后续若人工复现，先按 `Items.Count/CollectionChanged/代际/滚动器/Presenter DIP/首末行/cell visual-text-clip/锚点` 分流，禁止先改参数。

## 2026-09-09 L36 真实 Playnite 嵌入采集已获得，但视频式人工复测仍待宿主

- 当前最新宿主证据为 [`artifacts/ui-host-audit-isolated-l36/summary.json`](../../artifacts/ui-host-audit-isolated-l36/summary.json)、[`metadata.json`](../../artifacts/ui-host-audit-isolated-l36/metadata.json) 和 [`diagnostics-summary.md`](../../artifacts/ui-host-audit-isolated-l36/diagnostics-summary.md)，提交为 `99bc976473d13a92c12d6992dff11dbf807e42b5`。真实 Playnite 生产扩展嵌入 Dashboard/设置页均已采集；没有使用受控专用窗口作为生产宿主证据。
- 隔离只读数据库为 `games=3`、`media=4645`、`tasks=4551`、`game_tools=4`；MediaInbox 页面只加载 `200` 条分页数据。宿主 DPI `1.5`、Dashboard `1365.33×868 DIP`、`FollowPlaynite`。当前最新包 `.pext/.zip` 均为 `43,831,773` 字节，SHA-256 为 `099BDC1B69E1BF44E25B7536A15B03342385C116857AB9CE1DBAE202A5B15C06`。
- 真实日志确认任务表/媒体表行滚动器为 `ScrollViewer`，其 `IScrollInfo` 链包含 `ScrollContentPresenter|DataGridRowsPresenter`，`CanContentScroll=True`、`ScrollUnit=Item`；水平条出现时 Presenter 的实际可见矩形已缩小，当前证据支持现有插件局部模板的“表头 Auto / 行内容 * / 水平条 Auto”方向，不支持继续向页面 Margin 或固定底部补偿扩散。
- 重要诊断边界：真实宿主在初始尺寸/滚动过渡帧会出现 `visual>0,text=0`，约 32ms 后恢复为 `text=visual`；该帧没有 `blank/gap/hOverlap`，没有行漂移记录。本次没有持续 `blank=True` 或 `gap=True`，因此不能把视频根因提前归结为锚点恢复、集合 Reset 或单位错误。
- `capture-manifest.json` 对媒体 Inspector/媒体列表滚动面给出 `CapturedAndValidated`，但这不等价于已人工拖到 `MediaInboxGrid` 已加载末尾并确认最后行完整。由于 CUA 当前无可识别原生窗口，必须保留“真实宿主人工 20 次拖动/键盘矩阵/录屏待验收”。后续若人工复现持续空白，优先对照本摘要的 `items/offset/viewport/extent/presenter/rows/text/clip/anchorGen`，按集合是否 Reset、偏移是否越界、容器几何是否错误、单元格是否仅视觉缺失分流。

## 2026-09-09 L36 之前的真实宿主阻塞记录（历史）

## 2026-09-09 L30 候选安装包与升级/回退

- 候选公共版本固定为 `0.6.73`。`scripts/package.ps1` 必须从当前源码生成插件、Worker、Core、Contracts 的同源构建身份；最新身份为 `0.6.73+1fdd15eedfa64bb34292b85cb0e4d14bbfa9dd81`。Worker 发布必须是 `win-x64` self-contained，并验证 `runtimeconfig` 的 `includedFrameworks`。
- 包内最低必需集合包括 manifest、icon、插件 DLL、Contracts/Core、Worker EXE/DLL、Worker runtimeconfig、hostfxr/hostpolicy/coreclr；`.pext` 与 zip 应保持同字节内容。最新候选两个包均为 `43,837,799` 字节，SHA-256 为 `FD91FB0E0B12ABA2A73F29F76F1E3F90255D6FA4D30798EBA2FEFB53FAD120F9`。
- 数据库迁移采用幂等增量列/表初始化，必要时在事务内重建 `backup_versions` 并保留数据；没有通用 down-migration。升级前必须复制完整隔离配置/状态库，回退先停宿主、保留失败副本、恢复升级前副本再安装旧包，不能让旧版直接打开未知新 schema。
- L30 只验证候选包与隔离迁移，不等价真实 Playnite 安装；包未安装，宿主加载/FusionX/DPI/用户主题/视频继续由 L31 验收。

## 2026-09-09 L31 真实宿主矩阵阻塞

- `scripts/real-host-audit.ps1` 会停止现有 Playnite并通过 `dev-install-run.ps1` 重装/启动开发扩展；本轮没有执行该有副作用流程。
- PowerShell 只读发现 Playnite 位于 `D:\software\Playnite\Playnite.DesktopApp.exe`，但 Windows Computer Use 返回空应用清单，不能绑定窗口，因此没有真实滚动、DPI、键盘、FusionX 或录屏证据。
- 本轮尝试启动既有审计流程时被安全门禁拒绝，因为流程会替换用户扩展目录；未绕过门禁，也未写入 Playnite 用户目录或启动宿主。继续执行需要用户明确授权该替换范围。
- 后续不得把 RenderHarness、源码检查或包内验证当作真实宿主通过；恢复条件和矩阵见 `docs/ai/L31_REAL_HOST_BLOCKER_2026-09-09.md`。

## 2026-09-09 表格滚动诊断补强与离线复现

- `DataGridScrollDiagnostics` 选择实际拥有 `DataGridRowsPresenter` 的内部滚动器，避免宿主模板出现多个 `ScrollViewer` 时把外层页面滚动器误当作表格滚动器；日志仍只记录稳定 ID、数量、尺寸、偏移、代际和状态，不记录文件内容。
- `MediaCenterView` 的锚点记录不改变恢复语义，只增加 `queued/executing/completed/skipped/retry/failed` 状态与原因；`MediaWindowAnchorContractTests` 锁定这些诊断出口。
- 提交 `862742a` 新增隐藏 WPF `Window` 的同数据对照：插件模板从顶部执行 `ScrollIntoView(最后一项)` 后偏移为 `1992/1992`，最后行完整；随后 20 次往返及 `PageDown/PageUp/Ctrl+End` 保持可见和选择。标准 WPF 模板的滑块/Ctrl+End 末行完整，但 deferred `ScrollIntoView` 在该离线夹具仍为 `offscreen-baseline-inconclusive`，不能拿来推断 FusionX。
- 提交 `85b1aeb` 只读加载本机 FusionX `2.1.1` 的 `DefaultControls/DataGrid.xaml` 做同窗体对照；没有写入用户主题。FusionX 直接滑块/Ctrl+End 到 `1987/1987`，末行 `1999@608/44` 完整，Presenter `0,36,1078.67x600`，水平条 `Collapsed/0`；deferred `ScrollIntoView` 仍是不确定基线。该结果不能替代真实 Playnite 内的 FusionX 模板链、DPI 或视频操作。
- 提交 `5198c6c` 增加 FusionX 水平条显示场景：700×640 DIP 视口、1100 DIP 列宽时水平条为 `Visible/17.33`，Presenter 为 `678.67x582.67`，末尾仍到 `1987/1987` 且最后行 `1999@608/44` 完整；20 次往返和语义滚动没有空正文或末行裁剪。该结果仍是隐藏窗口离线夹具，不是宿主录屏。
- 提交 `8775809` 修正 RenderHarness 对空 `git status --porcelain` 的解释；最新 `.tmp/l32-scrollprobe/scaleprobe-report.txt`（canonical 提交 `ea18b11`）记录 `WorkingTreeClean: True`，不再把干净工作树写成 `False`。
- 提交 `f31711c` 让 `CaptureAnchor`/`RestoreAnchor` 使用与诊断器一致的实际表格 `ScrollViewer` 选择规则：优先含 `DataGridRowsPresenter`，再按 DIP 视口高度/宽度排序；提交 `1477a37` 用 STA 隐藏 Window 行为测试锁定多滚动器选择。定向锚点契约为 `10/10`；最新 `1fdd15e` 候选包已包含生产修复、浅色主题修复和首页修复，但仍不能把离线报告写成真实宿主已验证。
- 离线 `scaleprobe` 的 20 次滑块往返在任务表/媒体表 200、2000、10000 条规模均保持可见行和文字；末尾滑块路径的最后行完整。真实 Playnite/FusionX、DPI、视频和加载更多现场锚点仍必须复测。
- 本轮没有真实宿主窗口；后续仍须用 Playnite/FusionX 重复视频动作，不能用该离线报告替代宿主验收。锚点修复和 STA 行为测试后的 Playnite 全量离线回归为 `423/486`（63 skip、0 fail），锚点定向 `10/10`。

## 2026-09-09 L28 持续更新分页与选择恢复

- `CloudTransferStateService.GetStatusAsync` 和 `MediaSyncService.GetClassificationHistoryAsync` 的 revision/一致性令牌是 offset 分页的正确性边界：请求期间或请求前令牌变化必须返回 `PageResetRequired`，不能继续拼接旧页。当前触发器覆盖云端队列/重试队列、游戏/媒体云状态以及归类批次/批次项的增删改。
- `DashboardViewModel.CloudTransfers` 和 `DashboardViewModel.MediaClassification` 在分页重置前捕获稳定选择 ID。重置后使用第一页结果恢复；首屏没有该 ID 且 `HasMore` 时，设置 pending ID 继续下一页，恢复后清理 pending；到达末页仍未找到则按对象删除/不再可见处理并清理选择。
- `allowConsistencyRetry` 只允许一次从第一页自动重试。第二次仍然 `PageResetRequired` 时必须禁止 pending selection 的后续自动翻页；保留 pending ID，设置 `*NeedsManualRefresh`，摘要和 `StatusMessage` 引导用户点击现有刷新命令。手动刷新会重新开启一次有界自动重试。
- L28 Worker 回归覆盖新增记录已有场景，以及云端传输状态更新、归类批次状态更新；Playnite 源契约覆盖选择保留、后页恢复和重复重置停止。不要把离线源契约当成真实宿主行为证据。

## 2026-09-09 L29 全量回归与 skip 账本

- 当前全量计数必须按项目和原因拆开记录：Core `76/76`；Worker `310/311`，1 项 `[WorkerProcessFact]` 跳过；Playnite `420/483`，57 项 `[LegacyProductionUiBaselineFact]` 旧架构断言 + 6 项 `[NamedPipeFact]` IPC 行为测试。跳过不计入通过，详细文件/数量/替代证据在 `docs/ai/SKIP_LEDGER_2026-09-09.md`。
- `LegacyProductionUiBaselineFactAttribute` 不是当前功能失败，而是旧“今日工作台”布局断言与现行 AcrylicFork/Demo-first 生产结构不一致；恢复它们前必须按当前页面契约重写。Named Pipe 和 Worker 进程测试则是环境前置条件缺失，不能用源字符串或普通单元测试宣称行为已通过。
- `scripts/e01-behavior-matrix.ps1 -SkipBuild` 必须使用既有默认 Release 输出；只有实际构建隔离输出时才传 `GscBuildOutputRoot`。否则会出现空日志/`0/0` 且退出码为 0，污染回归证据。修复后 E01 为 `144/151` 通过、`7/151` 跳过，并明确保留真实 Playnite `MANUAL QA REQUIRED`。
- L29 的回归只证明当前代码在可用离线/STA 条件下通过；真实 Playnite、FusionX、DPI、Named Pipe 进程间时序、Worker 重启和视频操作仍是宿主验收项。

## 2026-09-09 L27 配置、路径与外部文件变化

- `GameSaveCenterSettings.VerifySettings` 对存档目录、媒体目录和启用的本地镜像执行只读路径形状/目标类型检查：缺失的叶目录只要所在驱动器或共享可达就保留为可创建状态；指向文件、无效路径、磁盘/共享不可达或 ACL 访问异常会关联到具体目录字段。不要在文本框校验中调用 `Directory.CreateDirectory` 或写探针。
- `MediaSyncService` 的内置系统来源仍允许缺失；用户在媒体来源规则中配置的目录必须先通过一次可枚举性确认，并在扫描期间异常时抛出 `MEDIA_SOURCE_UNAVAILABLE`。单个媒体文件在稳定性/哈希/复制阶段遇到占用或访问异常时抛出 `MEDIA_FILE_UNAVAILABLE`，保持源文件不删除。`MEDIA_SOURCE_UNAVAILABLE`、`MEDIA_FILE_UNAVAILABLE` 的 diagnostic detail 只带路径/异常摘要，不写入媒体内容。
- `SavePathDetectionService` 将 `DetectionRequestDto.AdditionalRoots` 置于默认根之前并标为 required：根目录消失、无法枚举或子目录/文件枚举被拒时抛出 `SAVE_PATH_ROOT_UNAVAILABLE`；用户未明确传入的系统根仍按可选扫描处理，避免普通用户目录权限差导致整个探测失败。
- L27 证据：设置/便携导入定向 `11/11`；媒体与存档路径定向 `16/16`，包含缺失配置来源失败、附加根失败、Unicode/长文件名归档、占用文件失败且源文件仍在。全量 Core `76/76`、Worker `308/309`（1 项隔离 Worker 重启测试在沙箱跳过）、Playnite `419/482`（63 项 UI/宿主条件跳过）；Release `0 warning/0 error`，`validate-source.py`、XAML `19/19`、`git diff --check` 通过。
- 边界：本轮未改用户 FusionX/全局主题，也未关闭任何虚拟化；没有真实 Playnite 保存失败、网络共享 ACL、外置盘断开后的宿主页面、DPI 或视频录屏证据，不能把离线路径夹具写成真实宿主已验收。

## 2026-09-09 L26 Worker 启动、断连与恢复边界

- `WorkerLauncher.EnsureStartedAsync` 的生命周期边界：健康握手先分协议/版本/构建身份；同路径已有进程的 transient probe 最多宽限 45 秒，启动新 Worker 的真实就绪截止 30 秒；不能因一次短 Ping 超时杀掉大库 Worker。`StopOwnedWorker` 只回收本插件保存的 `runningWorker`，Playnite 退出不扫描/停止其他实例。
- Worker SQLite 初始化先 `RecoverIpcRequestLedgerAsync`，把上一个进程遗留的 `InProgress` 写请求变为 `Interrupted`，客户端收到后只能核对状态，不能把旧请求当成可安全重放。任务协调的硬重启恢复会将未完成 durable task 标为 `WORKER_RESTARTED_RETRYABLE`。
- 构建身份规则：已知 actual/expected 两个身份不一致才是 `BuildIdentityIncompatible`；unknown/空身份在协议/版本兼容时可继续工作，但日志必须明确“构建身份未验证”，不能冒充同源。旧 Worker 没有 handshake 时按受限 legacy Ping 兼容。
- L26 证据：独立临时 Worker 硬停止/重启测试 `1/1`；完整 Worker `305/305`、0 skip；Playnite 完整 `423/480`、57 个宿主条件 skip。真实 Playnite UI 的启动失败、运行中断连、恢复刷新和多实例安装仍需宿主矩阵。

## 2026-09-09 L25 IPC 取消、超时与同 ID 复核

- `WorkerIpcClient` 的破坏性请求在超时/断管后只能用原 `IpcEnvelope.RequestId` 复核；不能生成新 ID 直接重发。Worker `ipc_request_ledger` 按请求类型、协议版本和 canonical payload fingerprint 冲突保护，Completed 回放原响应，InProgress 返回“仍在执行”，Interrupted 返回“此前 Worker 已退出，先核对状态”。
- 复核阶段必须继续使用调用方 Token：连接、读取和 `REQUEST_IN_PROGRESS` 间隔等待都可被调用方取消；但因为原写请求可能已送达，取消异常必须保留 `MayHaveBeenAccepted=true`。宿主退出优先报告 `WorkerIpcCancellationReason.HostShutdown`，只读请求响应读取取消保持 `false`。
- 既有 VM 请求作用域/代际保护仍是 UI 提交门，不要在旧 IPC 响应到达时弹过期错误或恢复旧选择。写请求回执丢失是“结果未知”，不应自动显示成功，也不应把普通取消当服务器拒绝。
- L25 证据：本地 Named Pipe 行为测试 `6/6`，Worker 账本测试 `6/6`；测试矩阵包括连接前、只读读响应、宿主关闭、写回执丢失、复核阶段取消和写入中取消。真实 Playnite Worker 启停、页面关闭和录屏仍是宿主验收项。

## 2026-09-09 L24 隔离账本分页与动效采样

- 隔离账本的 UI 查询入口现在是分页契约，不再把所有未删除行一次性跨 IPC 传到维护页：`RetentionQuarantinePageRequestDto` 限制 `Offset/Limit`，Worker 在 SQLite 侧先给 durable `TotalCount`，再按 `updated_utc DESC, entry_id DESC` 返回最多 100 条并给 `HasMore`。现有 Worker 内部恢复/预览仍可使用完整持久化读取，不把 UI 分页误用于安全协调。
- `DashboardViewModel` 首次只装载一页；`LoadMoreRetentionQuarantineCommand` 追加下一页并按 `EntryId` 去重，动作请求仍以稳定 EntryId 为准。每次刷新或恢复后从第一页重建，摘要区分“隔离账本总量”和“已加载/全部”，按钮状态必须进入 `RaiseCommandStatesCore`，否则新页到达后可能保持不可用。
- 维护溢出 `ListBox` 必须保留有限视口（当前 `Tag=FiniteViewport`、`MaxHeight=360`）及 `CanContentScroll=True`、`VirtualizingPanel.IsVirtualizing=True`、`VirtualizationMode=Recycling`；不能用扩大页面、隐藏滚动条或关闭虚拟化掩盖列表规模。`scripts/validate-source.py` 对此有限视口有显式门禁。
- L24 证据：205 条夹具返回 `100/100/5` 三页、durable total `205`、稳定 ID 去重 `205`；shellqa 的单次/快速/关闭动画布局计数为 `46/58/3`，最大帧间隔为 `53.2/31.1/17.1ms`，均 settled。离屏报告不代表 FusionX、真实 DPI 或真实帧率；新增/删除/状态变化仍须在真实宿主通过刷新/恢复操作复测。

## 2026-09-09 L23 搜索、刷新与重复 IPC 合并

- `DashboardViewModel` 的任务历史分页与 Dashboard 快照读取必须遵循最新请求提交规则：开始请求时创建稳定 generation/Token，IPC 返回后和 `ApplyOnUi` 回写前再次确认当前作用域；旧响应只允许结束自己的取消/清理，不得清空新请求状态、覆盖新筛选或写入旧页面。`LatestRequestCoordinator.RequestScope.Token` 在取消后仍可安全读取，不能让释放 CTS 反过来制造 `ObjectDisposedException`。
- 任务搜索/状态/游戏/类型变化先失效当前分页请求，再通过 debounce 合并；历史范围/时间范围和清除筛选是立即查询。debounce 回调必须投递到 UI，若 `IsBusy` 则保留 `taskHistoryQueryQueued`，在 `RunAsync` 释放忙状态后启动最新一次；不可恢复为“直接 `Run`、忙时静默丢失”。
- Dashboard 刷新代际要覆盖同步、`GetDashboard` IPC、快照应用和后续任务/媒体/维护加载；`CancelDeferredUiWork` 同时取消任务页与快照协调器。后台只读刷新不改变写请求的取消/重试语义，也不能将不同筛选条件复用成同一结果。日志只输出请求代际、请求号、数量/尺寸和 hasMore 等诊断字段，不输出搜索文本或文件内容。
- L23 证据：debounce 的连续 `a→ab→abc` 夹具为 `1` 次回调；协调器取消、替换、CTS 释放后 Token 和 `20` 次快速替换测试通过。独立 Playnite 测试 `417` 通过、`62` 跳过、`0` 失败；Core `76/76`、Worker `303/304`（1 跳过），Release 构建无警告/错误。真实 Playnite/FusionX、DPI、视频和真实 IPC 日志仍待宿主验收，不能把离线契约测试写成视频问题已解决。

## 2026-09-09 L22 缩略图加载、取消与缓存边界

- `AsyncThumbnailImage` 的加载入口必须同时满足 `IsLoaded`、`IsVisible` 和非空路径；不可见/卸载先推进 generation、取消并释放旧 `CancellationTokenSource`，同时清空 `Source`。路径或预览宽度变化不能让旧任务回写新卡片。
- `AsyncThumbnailLoader` 的稳定边界是 `BitmapCacheOption.OnLoad`、冻结 `BitmapSource`、最大 3 路解码和最多 96 项 LRU；文件元数据、缓存检查和解码均不在调用方 UI 线程执行。宽度先夹到 `48..480` 后参与缓存键，破损/缺失/权限等预期读取错误返回空值，取消继续向上抛出。
- L22 探针只记录 ID/尺寸/计数：120 项初次窗口 `120` 请求、`120` 成功、峰值并发 `3`、缓存 `96/96`；16 项保留窗口全命中；破损与缺失各返回空；预取消可观测；100 次 12 项窗口往返结束时活动解码为 `0`，实际缓存仍为 `96/96`；旧 800×800 路径替换为新 64×64 路径后最终像素标记为新图且输出宽度 `96`。
- `tests/GameSaveCenter.Playnite.Tests` 的 WPF 控件回归与 `tests/GameSaveCenter.RenderHarness thumbnailprobe` 只证明合成 STA/文件夹夹具；不把它们写成真实 Playnite/FusionX、DPI、用户目录锁定或视频回放验收。临时报告只保留最新 `.tmp/l22-thumbnailprobe-final`，不进 Git。

## 2026-09-09 L21 列表虚拟化与滚动规模实测

- 当前游戏媒体卡片列表必须使用项目已有的 `VirtualizingWrapPanel`；普通 `WrapPanel` 即使配了 `VirtualizingPanel.IsVirtualizing=True` 也会在 200/2000/10000 夹具中生成 200/2000/2000 个卡片，不能作为大库实现。当前 XAML 的 `MediaGrid` 保留 `ItemWidth=164`、`ItemHeight=154`、水平/垂直间距 0，选择和 ListBox 滚动契约不变。
- L21 离屏规模证据：当前媒体窗口上限 2000；200/2000/10000 后端场景顶部、底部、回顶部均约 20 个卡片容器，200 条往返的 extent/viewport/最大偏移为 `6160/345.33/5814.67`。任务 DataGrid 200/2000/10000 保持个位数行容器，收件箱 10000 后端仍保留 2000 条 UI 窗口；选择 ID、末项稳定 ID 和 resize 后容器均受探针检查。
- `DataGridScrollDiagnostics` 必须继续被动记录真实内部 `ScrollViewer`/`ScrollContentPresenter`、单位、offset/viewport/extent、首末行 ID/Y/height、行/单元格内容、选择和水平条，不以 `DataGrid.ActualHeight` 代替内部证据。离屏 `ScrollIntoView` 在插件模板和标准 WPF 模板中同样 deferred，不能写成 FusionX 已定位或已修复。
- 当前阶段没有修改宿主 FusionX、全局样式或关闭 DataGrid 虚拟化；无真实 Playnite/FusionX、DPI 和视频回放，相关验收保持宿主待验收。临时探针证据只保留最新目录，不纳入 Git。

## 2026-09-09 L20 修改器导入与下载结果反馈

- `DashboardViewModel` 的工具导入必须在检测开始时保存目标游戏 ID/名称；多候选 `ImportEntryCandidates` 的确认不能在返回后重新从 `SelectedGame` 推导目标。游戏切换会清理待确认项并要求重新导入；导入请求完成后只有同一游戏仍被选中时才刷新当前工具详情。
- FLiNG 下载请求必须捕获 `PlayniteId`、`CatalogId`、`ReleaseId` 后再进行 IPC。页面反馈只消费 Worker 任务事件和终态 `TaskStatusDto`，按状态与 `FLING_DOWNLOAD_FORBIDDEN`、`FLING_DOWNLOAD_INVALID`、`FLING_RELEASE_PARSE_FAILED` 等稳定错误码给出下一步；不要在 VM 重新实现下载、解压、来源校验或启动可执行文件。
- 取消下载复用现有 `MessageTypes.CancelTask`/`TaskCoordinator.Cancel`。Worker 的取消边界、临时文件清理和不自动运行语义是安全事实；页面只能显示“已发送取消请求/已取消”，不能在取消按钮点击时假定文件已删除或绑定已回滚。
- `TrainerCenterView` 的新增状态卡只承载进度、结果和下一步，不能以固定高度、隐藏滚动条、关闭虚拟化或改全局主题来掩盖问题。离屏 RenderHarness 只证明 XAML/夹具状态可渲染，不证明 Playnite/FusionX 模板链、在线 403/离线和真实视频操作。
- L20 验证：Release 构建 `0 warning/0 error`；Core `76/76`、Worker `303/304`（1 跳过）、Playnite `405/467`（62 跳过）；RenderHarness Release 构建、源校验、XAML `19/19`、WPF 静态审查 `0 errors/21 warnings/172 info`、差异检查通过。完整 render-qa 仍有已知 Media 小视口/预览列表与 Sidebar rapid-toggle 失败。

## 2026-09-09 L19 存档版本识别、比较与恢复信息

- `BackupVersionDto` 的来源、系统和恢复检查时间必须使用显式展示字段；空值显示未知/尚未检查，不能从创建时间、文件数量或锁定状态推导“健康”或“可恢复”。`BackupDiffDto` 的大小变化使用带符号的人类可读值，比较质量继续沿用 Worker 的 Exact/Estimated/InvalidManifest 事实。
- `DashboardViewModel.SelectedBackup` 切换到不同稳定 ID 时清掉旧 `LastBackupDiff`；比较响应提交前重新确认当前游戏、选中版本和上一版本 ID 都未变。比较命令的现有语义是“当前选中版本 vs 上一版本”，UI 必须这样表述，不要暗示任意两版本选择。
- 恢复确认前先复制游戏/版本稳定 ID、创建时间、来源、系统、锁定和就绪摘要；确认后 `RestoreExecute` 只能使用这些快照值。不得因为未知、警告或失败状态在 UI 层猜测健康或替代 Worker 安全阻断；PreRestore 快照与撤销请求保持原协议。
- L19 验证：Release 0 警告/错误；Core `76/76`、Worker `303/304`（1 跳过）、Playnite `404/466`（62 跳过）；源校验、XAML `19/19`、WPF 静态审查 `0/21/172`、差异检查通过。RenderHarness 的 Save 截图与多尺寸/双主题探针未报本轮新增问题；全量 render-qa 的 Media 小视口/预览列表和 Sidebar rapid-toggle 仍是已知失败。真实 Playnite/FusionX、DPI、不同恢复就绪状态和录屏必须继续标记为宿主待验收。

## 2026-09-09 L18 批量动作提交前摘要

- 媒体批量命令必须在命令入口捕获 `MediaId`，先去重并统计原始选择、重复项、无稳定 ID 项，再将捕获的 ID 列表交给确认后的 IPC；不得在确认返回后重新读取 `SelectedItems`。目标游戏 ID/名称、归类预览 `BatchId` 和高置信媒体 ID 同样要在确认前保存。
- 只读媒体归类预览不额外弹确认；批量归类、忽略、恢复以及应用归类建议复用现有确认框。Worker 返回后要区分成功、失败、冲突、跳过和未返回，不能用“成功 N 项”覆盖部分结果；已保留的归档副本和安全移动语义不变。
- 任务批量重试只从当前 `TasksView` 结果计算，先按现有游戏/任务类型去重，再排除没有稳定 `TaskId` 的候选；确认后用任务类型、游戏 ID、错误码和显示信息的不可变快照执行。Worker 协议当前按游戏/任务类型重试，不要为了摘要新增任务重试协议。
- L18 验证：Release 0 警告/错误；Core `72/72`、Worker `303/304`（1 跳过）、Playnite `404/466`（62 跳过）；源校验、XAML `19/19`、WPF 静态审查 `0/21/172`、差异检查通过。本轮未改页面布局，不把未运行的真实 Playnite/FusionX 选择变化、部分失败和录屏写成已验收。

## 2026-09-09 L17 常用筛选与工作区状态记忆

- `GameSaveCenterSettings` 的任务查询状态必须一起处理：状态、动态游戏/类型、搜索、历史范围和时间范围。动态游戏/类型不能在构造函数中直接写入 ComboBox 还未拥有的选项；使用 pending 值，等 `TaskFilterOptionsSync` 完成后再恢复。
- 已删除游戏的旧任务记录不能单独证明筛选仍有效。恢复动态游戏筛选时同时检查当前 Playnite `Games`；仍存在但不在最近任务窗口的游戏可补入动态选项，缺失游戏回退“全部”。历史范围只接受现有选项，旧格式/非法值归一化为“最近任务/全部时间”。
- 如果保存的历史范围或时间范围不是默认值，构造 VM 时必须重新激活 `taskHistoryActive`，让启动后的第一次刷新继续走服务端历史分页；不能只恢复 ComboBox 文本而继续展示最近任务快照。
- 媒体“清除”只重置查询条件：取消媒体搜索和分页防抖、失效当前媒体详情代际、刷新现有 `MediaView` 并重新调度受工作区/游戏保护的分页请求；不得把筛选重置绑定到归类、编辑、刷新全库或批量命令。
- L17 验证：持久化/迁移定向 `10/10`，全量 Core `72/72`、Worker `303/304`（1 跳过）、Playnite `403/465`（62 跳过）；Release 构建 0 警告/错误，源码/XAML/WPF/差异检查通过。完整 render-qa 的既有直接 Media 小视口/预览列表和 Sidebar rapid-toggle 失败继续独立记录，真实 Playnite/FusionX 仍待验收。

## 2026-09-09 L16 媒体收件箱操作可达性

- `MediaCenterView` 的收件箱批量栏只做局部压缩：选择摘要宽度 `112`、模式 `104`、目标游戏 `160`，预览入口与批量动作保持同一操作层级；不得为首屏压缩删除真实归类、预览、批次历史或撤销入口。
- 生产壳层在 1040/1100 窗口下给媒体页的 PageHost 约 577/597 DIP。`MediaCenterView.ApplyResponsiveLayout` 在 `<620` DIP 或 Stale 时打开 `MediaInboxPageScrollViewer`，让页面内容承接工具栏/有限 DataGrid/footer 的总高度；DataGrid 仍是 `Tag=FiniteViewport`、Item 滚动、Recycling 和行列虚拟化。不要改成无限测量、关闭虚拟化或用固定底部像素补偿。
- 离屏生产壳层探针的 1040×700/1100×720 网格为 `300 DIP`、顶部间距 `63 DIP`；将页面滚到 `offset==scrollable` 后，`MediaInboxFooter`、`MediaInboxHistoryButton` 和 `MediaInboxSecondaryActions` 均完整处于 PageHost 视口。真实 FusionX 模板和用户录屏仍未验证，不能把该探针称为宿主通过。
- 完整 render-qa 当前仍会报告直接 Media 场景的预览列表/小视口门禁以及偶发侧栏快速切换；这些与生产壳层底部可达性分开记录，后续不要通过降低门禁或扩大 DataGrid 来掩盖。

## 2026-09-09 L15 任务页查错与范围说明

- 顶部任务摘要保留 `RunningTaskCount`、`RetryableTaskCount` 和今日完成，同时新增 `TaskWaitingSummary`（排队 + 等待确认）与 `TaskRetrySummary`（失败 + 已取消）。快照刷新和历史分页完成时必须一起触发这些派生属性的通知。
- `TaskQueueFilterSummary` 只在 `TaskHasActiveFilters` 时显示在队列标题区，避免宽屏用户必须展开“更多筛选”才能知道当前查询；`TaskLoadedSummary` 继续表达已加载窗口、服务端总数、最近/全部历史和时间范围。不要把已加载页数写成全历史已处理数。
- 批量重试按钮的作用域是当前已加载并通过 `TasksView` 的结果，现有 `GetRetryGroupKey` 去重和逐组安全重试保留；文案必须明确这一点。不要因为增加范围说明而改 Worker 协议、游标分页或 `TaskIndexedCollection` 的 200 行窗口。
- L15 验证：任务定向 `36/42`（6 跳过），全量 Core `72/72`、Worker `303/304`（1 跳过）、Playnite `402/464`（62 跳过）；Release 构建无警告/错误，源码/XAML/差异检查通过。离屏 Task 场景在 1040×700 仍有 6 行首屏，双主题/resize 通过；完整 render-qa 的稳定失败属于既有媒体小视口/媒体壳高度，真实 Playnite/FusionX 仍待验收。

## 2026-09-09 L14 运维总览按处理顺序组织

- `MaintenanceActionItem.Group` 只按现有动作语义分为 `NeedsManualHandling`、`WaitingForRetry` 和 `Routine`：隔离账本、认证/失败云端记录需要人工处理，`RetryScheduled` 进入等待重试，恢复巡检进入例行巡检。不要依据显示文案另造分类，也不要把云端摘要计数当成已加载明细。
- `MaintenanceActionSection` 固定保留完整 `Items`，`PreviewItems` 只取前 3 条，`OverflowItems` 显式取其余记录。概览只展示有记录的分组；每次重建都替换 section 列表，保持集合代际和具体 `TransferKey`/`EntryId` 动作参数不变。
- `MaintenanceView.xaml` 的单条动作模板集中保留状态、详情、时间和真实 `RunMaintenanceActionCommand`；溢出使用基于 `GscDisclosureCard` 的展开器。不要改成批量自动修复、定时刷新或关闭虚拟化。长文件名依靠省略提示和详情文本可达。
- L14 验证：分组边界契约覆盖空组、单条、20 条及长标题；Release 构建 0 警告/错误；Core `72/72`、Worker `303/304`（1 跳过）、Playnite `402/464`（62 跳过）；源码/XAML/差异检查通过。离屏维护页通过；完整 render-qa 的稳定失败属于既有媒体小视口/媒体壳高度，首轮另有一次侧栏 rapid-toggle 未稳定，单独 shellqa 重跑已稳定。真实 Playnite/FusionX 仍待验收。

## 2026-09-09 L13 首页优先级与活动上下文

- `OverviewPriorityResolver` 保持单一 Hero 决策，顺序为 Worker 离线、首次准备、云端待处理、媒体待归类、空库、游戏告警、健康刷新。空库使用 `ManagedGames <= 0` 明确显示“还没有可管理的游戏”，不再把无游戏快照当作健康状态；云端失败/认证/校验/重试仍统一来自 `CloudTransferSummaryDto.AttentionCount`。
- `ActivityEntryDto` 新增稳定 `PlayniteId`，由 `ActivityTimelineMapper` 从审计详情提取；`DashboardViewModel.OpenActivityCommand` 按活动类型路由到存档、媒体、工具、云端或维护工作区，能命中已加载游戏时先恢复同一 `SelectedGame`。Overview 活动行使用透明但真实的按钮模板，保留虚拟化、时间和对象显示，并支持键盘焦点。
- 新增空库/云端失败优先级测试，并扩展活动映射测试。Release 构建无警告/错误；Core `72/72`、Worker `303/304`（1 跳过）、Playnite `401/463`（62 跳过）；源码/XAML/差异检查通过。离屏 render-qa 的 Overview 场景通过，媒体小视口/媒体壳表格高度仍是既有失败；没有真实 Playnite/FusionX 交互证据。

## 2026-09-09 L11 通知、长错误与复制详情

- `UiNotificationEventArgs` 的 `Message` 只用于短摘要，`DetailMessage` 保留完整文本。`GameSaveCenterPlugin.RaiseUiNotification` 按成功/信息与错误/警告使用不同摘要上限；任务终态详情额外包含状态、错误码和任务 ID。
- Dashboard 只在 Toast 位置显示摘要；错误或存在独立详情的长消息通过“查看详情”打开共享结果对话框。对话框消息区使用有限 `ScrollViewer`，复制按钮使用详情快照并在异步重试后确认当前对话框仍对应同一文本，避免旧反馈覆盖新反馈。
- 取消任务从 `ShowInfo` 改为 `ShowWarning`，宿主没有 `NotificationType.Warning` 时仍回退到现有 Info 通知；没有新增弹窗式错误流程、没有改任务集合/重试/取消业务语义。
- 新增 `UiFeedbackTests` 与 `NotificationFeedbackSourceTests`。Release 构建 0 警告/错误；全量 Core `72/72`、Worker `303/304`（1 跳过）、Playnite `398/460`（62 跳过）；源码/XAML/差异检查通过。离屏 `render-qa` 的既有媒体小视口/侧栏快速切换问题继续独立记录，真实宿主通知回退、DPI 和录屏未完成。

## 2026-09-09 L12 详情展开与选中上下文

- 收件箱紧凑 Inspector 的展开状态不再跨对象泄漏：`OnMediaInboxSelectionChanged` 清除 `mediaInboxInspectorOpen`，`OnMediaInboxModeSelectionChanged` 还会清除 `mediaInboxHistoryOpen`；切换模式/换选后需要显式打开当前对象。其他任务、当前媒体、存档历史/候选和维护诊断/进程/设备/云端路径保留各自已有的选择即关闭旧详情语义。
- `MaintenanceView` 的 `CloudTransferGrid` 已移除构造函数的重复 `SelectionChanged` 订阅，XAML handler 作为唯一路由；不能再把一次选择触发两次 `ApplyResponsiveLayout` 当成正常行为。
- `DetailsDisclosureSourceTests` 守护四类 CenterView 的选择清理和云端事件单路由。没有新增通用状态框架、定时器、强制布局或业务命令；真实宿主对象删除、快速换选、窄/宽来回和 FusionX 视觉/键盘轨迹仍待验收。

## 2026-09-08 L10 设置修改、错误定位与取消体验

- `SettingsValidationSummary` 旁新增 `SettingsValidationLocateButton`。`FindValidationCategoryIndex` 根据 `VerifySettings` 的现有错误文本把压缩/保留量送到备份分类，毛玻璃送到外观，进程/刷新/巡检/通知送到自动化，Worker/Ludusavi/Rclone/镜像送到常规；点击后只改变 `SettingsSectionTabs.SelectedIndex` 并聚焦分类导航。
- `GameSaveCenterSettingsView` 在构造时监听 `TextBox.TextChanged`、`ComboBox.SelectionChanged`、`CheckBox.Click` 以及 `ToggleButton.Checked/Unchecked`；`OnVisualSettingChanged` 和 `OnGlassStrengthChanged` 也调用 `QueueValidationSummaryUpdate`，所以切换 ToggleSwitch 或拖动 Slider 会更新指纹/校验状态。所有更新仍经 Dispatcher 合并，不加入定时器或保存命令。
- 导入流程的 `settingsTransferInProgress` 保护和 `settingsBaselineInitialized` 逻辑保持不变：`DataContext` 重绑不会重置旧保存指纹，导入后的可编辑差异继续显示未保存；`CreateSettingsFingerprint` 忽略安装级 `DeviceId`。取消由现有 `CancelEdit` 恢复克隆并触发 `SettingsReverted`。
- `SettingsValidationSourceTests` 守护定位入口和控件事件；RenderHarness 的设置三态/隐藏分类导航探针实测 `selectedCategory=1`。全量测试为 Playnite `395/457`（62 跳过），构建 0 警告/错误。Playnite 真正的 `SavePluginSettings` 失败提示和宿主取消按钮没有在离屏环境中伪造，仍需宿主验收。

## 2026-09-08 L09 设置首屏与保存状态

- `GameSaveCenterSettingsView.xaml` 保留 `SettingsIntroDescription` 作为兼容命名但默认/响应式布局均设为 `Collapsed`；Hero 副标题改成短的“工具路径 · 存档策略 · 外观与自动化”。重复说明不再占用标题与正文之间的首屏高度，完整解释留在对应分类卡片附近。
- `RefreshValidationSummary` 的错误分支必须调用 `RefreshSaveState(errors.Count == 0)`。旧代码传入 `errors.Count != 0`，会在摘要显示校验错误时把保存胶囊误写成“已保存”；这个布尔语义由三态夹具守护。
- RenderHarness 的 `RunSettingsLayoutProbes` 检查重复说明不可见、5 个分类项和正文视口；`RunSettingsStateProbes` 在同一 `1040×700` 画布生成 normal/dirty/invalid 状态，分别核验保存文案、错误摘要可见性和截图。
- L09 全量 Release 构建无警告/错误；Core `72/72`、Worker `303/304`（1 跳过）、Playnite `395/457`（62 跳过）。完整 `render-qa` 的已有媒体小视口/侧栏快速切换门禁仍独立记录，不能归因于设置页；真实 Playnite/FusionX、DPI 和 Playnite 保存/取消仍待验收。

## 2026-09-08 L08 可重复诊断与性能采样入口

- `RenderHarness` 的 `render-qa`、`gridprobe` 和 `shellqa` 统一写入 `Scenario`、`EvidenceSource`、Git 提交/工作树状态、离线逻辑 DIP、主题、数据量和时序字段；`RenderTabs`/`RenderView` 将 `layout_ms` 与 `render_ms` 分开，离线报告把 `request_ms` 标为不适用，避免把离屏测量冒充真实宿主性能。
- `scripts/real-host-audit.ps1` 在真实宿主输出旁写 `runner-metadata.json`，包含场景、提交、配置、窗口 DIP、WPF 实际 DPI、主题、生产数据量和采集清单状态。真实宿主元数据仍由插件审计服务捕获，脚本只补充运行器来源，不修改 FusionX 或 Playnite 全局文件。
- `DiagnosticsPackageService` 的 `system.json` 记录场景、证据来源、窗口 DIP、已加载条目数、数据量和 Worker 查询耗时；没有真实布局采样时 `layoutDurationMs` 保持空值。诊断包不增加媒体/文件内容或敏感路径输出，也没有新增业务写入。
- `gridprobe` 重建后报告为 `gridprobe OK`，包含 50/400/2000/4468 数据量声明和滚动语义探针；这只能证明离线夹具的可重复性。真实 Playnite/FusionX 的同尺寸操作、实际 DPI、请求/布局采样和视频式回归仍是宿主验收项。

## 2026-09-08 L07 键盘、焦点与可访问名称

- `MediaCenterView`、`TaskCenterView`、`SaveCenterView`、`MaintenanceView` 和 `TrainerCenterView` 的紧凑 inspector 统一采用“入口打开 → inspector 获焦 → Esc 关闭 → 原入口恢复焦点”的路径；媒体收件箱批次历史单独回到 `MediaInboxHistoryButton`。
- Inspector ScrollViewer 设置 `Focusable="True"` 与 `KeyboardNavigation.IsTabStop="False"`，PreviewKeyDown 只在对应紧凑状态实际打开时处理 Esc；宽屏常驻详情不会因同一事件处理器而隐藏或把焦点送到折叠按钮。
- 点击处理器不写入非必要的 `RoutedEventArgs.Handled`，兼容现有直接反射调用布局测试；键盘事件仍在真实 RoutedEvent 上标记已处理。没有加入计时器、强制 UpdateLayout、滚动重置或宿主级快捷键劫持。
- `KeyboardFocusSourceTests` 的 STA 用例验证实际 `TaskCenterView` inspector 可获焦但不进入 Tab 顺序；源契约覆盖五页 Esc、Keyboard.Focus、焦点入口和自动化名称。真实 Playnite/FusionX 的人工键盘轨迹仍需验收。

## 2026-09-08 L06 目的导航与返回上下文

- `FindingNavigationTargetResolver` 将诊断目标分为精确游戏、任务名称兜底和不可用三类。存档路径导航只接受当前 `Games` 中存在的稳定 ID；目标消失时不切换当前游戏，直接写入可见状态提示。
- 失败任务导航在 `DashboardViewModel` 中保存短生命周期的诊断目标 ID/名称，不覆盖用户已有的任务搜索、类型、时间和范围筛选。服务端查询使用目标名称，内存过滤优先稳定 ID，加载完成后按目标选择任务；目标没有记录时明确提示。用户修改任一任务筛选后，诊断目标自动清除。
- 目的导航会取消输入防抖和旧任务查询，再启动一次代际保护的读取；没有加入 `Task.Delay`、`ScrollIntoView` 或新的全局导航框架。生产 Shell 的工作区页只创建一次，标签 `SelectedIndex` 继续由 VM 双向保留。
- `FindingNavigationResolverTests` 新增目标缺失、精确 ID、名称兜底和无身份场景；`PurposeNavigationSourceTests` 守护单次显式加载、选中恢复和工作区页/标签上下文。真实宿主仍需验证原用户主题、目标隐藏和快速连续入口。

## 2026-09-08 L05 六态与运维夹具覆盖

- `FakeDashboardData` 的默认构造保持 Ready 兼容；带 `WorkspaceFixtureState` 的构造可生成六态，非 Ready/Stale 会清空对应媒体、诊断和运维动作集合，Stale 保留旧数据并显示过期提示。Fake 不记录媒体文件内容。
- `Program statefixtures` 必须渲染真实 `MediaCenterView`/`MaintenanceView`，不能只渲染裸 `WorkspaceStatePresenter`。它检查关键公共绑定反射存在、状态覆盖层可见性、Stale banner、数据表面 DIP 尺寸和 Ready 运维动作数；缺字段直接进入失败报告。
- 代表性画面已目视复核：Stale 收件箱显示旧行、过期提示和选择框仍在同一行；Error 画面显示失败覆盖层；维护“下一步运维”显示恢复巡检、云端重试和隔离账本三项。错误/离线底层表面仍保留，但状态覆盖层承担不可用语义。
- 夹具暴露的布局根因是 Stale banner 出现后 `MediaInboxGrid` 被压成零高，不是数据集合丢失。`MediaCenterView.ApplyResponsiveLayout` 现在在 Stale banner 可见时启用外层页面滚动，同时保留内部 DataGrid 有限视口、虚拟化和底部操作区。
- 证据目录 `.tmp/l05-statefixtures` 为当前可再生输出；完成交付前只保留当前报告/必要证据，禁止将整批 PNG 或临时构建物提交 Git。真实 Playnite/FusionX 仍需用户环境按视频操作复测。

## 2026-09-08 Q6-04 构建身份闭环（隔离包验收）

- `WorkerLauncher` 仅在实际身份与期望身份都已知且不一致时判定不可复用。旧 Worker 没有 `BuildIdentity`，或任一身份包含 `+unknown`，仍按公共版本/协议继续工作，但健康结果明确标记为“构建身份未验证”，不伪造同源证明。
- `IsBuildIdentityCompatible` 将空/unknown 视为不可验证而非冲突；同版本两个已知提交不同仍返回不兼容。定向 `BuildIdentityTests` `3/3` 通过。
- `scripts/package.ps1` 的隔离正例已读取六个实际 PE 程序集并确认同源；同版本旧插件、`+unknown` 插件、脏工作树、无 Git 夹具均在生成成功提示前失败，失败后 `GSC_BUILD_COMMIT` 恢复为调用方值；最终文档提交后的 HEAD 也已重新打包校验。

## 2026-09-08 Q6-03 运维云端告警归并

- `DashboardViewModel.MaintenanceActions.cs` 的云端来源必须先合并再筛告警：`Snapshot.CloudTransfers.Items` 和分页 `CloudTransferItems` 按 `TransferKey` 聚合，`UpdatedUtc` 较新者胜出，同时间分页明细优先。不能先 `Where(IsAttention)` 再 `GroupBy`，否则旧 Failed 会遮住已 Uploaded/RemoteVerified 的新状态。
- `MaintenanceCloudTransferMergeResult` 同时给出被新明细解决的快照告警数和明细新增告警数，用于修正“未全部加载”的剩余数量与维护摘要；这避免旧摘要计数在已解决记录消失后继续生成伪造占位。
- 时间语义固定：恢复巡检使用 `LastVerifiedDisplay`，云端使用 `LastAttemptDisplay`，隔离账本使用 `LedgerUpdatedDisplay`；上传尝试不再显示为远端验证成功。
- `MaintenanceCloudTransferResolverTests` 5 项通过。真实 Playnite 分页刷新、状态更新、故障注入和维护页录屏仍未完成，代码测试不能替代宿主验收。

## 2026-09-08 Q6-02 媒体状态按上下文隔离

- `DashboardViewModel.WorkspaceStates.cs` 现在用 `MediaWorkspaceStateCache` 保存媒体详情/收件箱状态，成功时间和错误只属于当前上下文；详情上下文由游戏 ID、媒体筛选、搜索词组成，收件箱上下文由待归类/已忽略模式组成。
- 同上下文刷新失败仍保留旧数据并显示 Stale；新游戏、新筛选或新模式没有自己的成功缓存时显示 Error。媒体筛选/搜索/选中游戏改变时会推进 `mediaPageGeneration`、取消旧请求并重置分页状态，避免旧响应在防抖新请求前写入。
- Q6-02 测试覆盖同上下文 Stale、A 成功/B 首失败、旧请求晚回、收件箱模式隔离和取消状态，共 4 项。真实 Playnite 故障注入、状态切换录屏和用户主题仍未完成；后续文档只能写“代码/测试完成，宿主待验收”。

## 2026-09-08 Q6-01 状态面板重试命中修复（代码/离屏已完成）

- 复核确认三处带 `RetryCommand` 的失败状态面板（媒体收件箱、媒体详情、维护审计）不应设置 `IsHitTestVisible="False"`；该父级值会让模板内按钮永远无法鼠标命中。`MaintenanceView` 中没有重试命令的降级提示仍保持非阻塞，不要误删其语义。
- `Themes/Redesign.xaml` 的重试按钮空命令判断改为针对 `RetryCommand` 依赖属性的模板 `Trigger`；Loading 明确隐藏按钮但仍由可见面板阻挡底层操作。按钮保留共享样式、绑定和键盘行为，并补 `AutomationProperties.Name`。
- 新增 `WorkspaceStatePresenterBehaviorTests`：实际加载生产 `GscWorkspaceStatePresenter` 模板，在 STA WPF Window 中验证 Error/Offline 的可见性、绑定命令和视觉树命中，Loading 的底层阻挡，以及 Enter/Space 各执行一次；当前 `5/5` 通过。`WorkspaceStateSourceTests` 额外守护三处使用点不重新加父级禁止命中。
- 新增 RenderHarness `stateprobe`，生成双主题 Error/Offline/Loading 状态图和报告：`docs/design/reviews/2026-09-08-quality/state-*.png`、`stateprobe-report.txt`。这是插件模板的离屏证据，不是真实 Playnite 截图。
- 本阶段 Release 隔离构建与全量测试为 Core `72/72`、Worker `303/304`（1 跳过）、Playnite `376/438`（62 跳过）；源码/XAML/WPF 静态检查无 error。完整 `render-qa` 本次仍报告 25 个媒体小视口/侧栏快速切换问题，不能写成 render-qa 全部通过；真实宿主复测仍待完成。

## 2026-09-08 表格滚动诊断与局部模板修复（当前验收边界）

- 针对用户视频中的任务表/媒体待归类表正文空白、行内容漂移、选中框与文字分离及末行截断，本轮只处理表格滚动正确性，没有扩展功能或继续做页面美化。新增 `DataGridScrollDiagnostics`，只记录稳定 ID、计数、滚动范围、实际 `ScrollViewer`/`IScrollInfo`、`ScrollContentPresenter` 矩形、首末行坐标/高度、单元格内容可见性、滚动条占用和分页/锚点代际，不记录文件内容。
- 先在离屏相同数据探针中复现布局级坏路径：媒体 4468 条、短高度 287 DIP 时，未限制外层无限测量的版本出现 `items=4468`、`ScrollViewer viewport=4468x1963.33`、`ScrollableHeight=0`、`ScrollContentPresenter height=196592`，所有行被一次性测量，滚动动作不再改变可见窗口；这不是正常的中间滚动。去掉短窗口条件后又出现 `gridH=0`/内容视口高度 0，确认不能靠简单关闭页面 fallback 解决。
- 静态检查了用户当前 FusionX 的 DataGrid 模板：未修改 FusionX 或 Playnite 全局样式。插件在 `Themes/Redesign.xaml` 增加局部 `GscRedesignDataGridTemplate`，保留 `PART_ColumnHeadersPresenter`、`PART_ScrollContentPresenter`、ItemsPresenter、双向滚动条、列宽/排序/选择/键盘和虚拟化绑定；内部网格为表头 `Auto`、内容 `*`、水平滚动条 `Auto`，垂直滚动条只占内容行区域。
- `MediaCenterView` 的锚点恢复不再用 `CanContentScroll` 猜单位：捕获/判断可见性使用真实 `ScrollContentPresenter` 的 DIP 矩形；只有实际发现 `VirtualizingStackPanel`、`ScrollUnit=Item` 且双方 `CanContentScroll=true` 时才按逻辑项恢复，否则使用 DIP 像素差；请求/上下文代际保护保留。短窗口最终布局保留有限 `MaxHeight`，避免重新进入无限测量。
- 当前离屏证据保存在 `.tmp/gridprobe-final-20/gridprobe-report.txt`：任务/媒体执行 20 次顶部、底部和中间往返拖动，并执行滚轮、PageUp/PageDown、Ctrl+End、最后项定位；任务/媒体 50、400、2000 和媒体 4468 条、287/311/337/353/419/640/840 高度、600 DIP 窄宽水平滚动均为 `gridprobe OK`，报告诊断异常数为 0。窄宽媒体底部末两行 `y=80..124`，水平条 `y=159.33..171.33`；普通宽度媒体末行完整落在内容视口内。
- Release 构建 0 警告/错误；Playnite 全量测试 `371 通过 / 62 跳过 / 0 失败`，源码校验和 XAML 结构校验通过。真实 Playnite FusionX 窗口未在本会话中重播用户视频，也没有真实宿主前后录屏，因此本轮只能写“离屏验证通过，待宿主验收”，不能宣称用户视频问题已在真实宿主解决。

## 2026-09-08 两项截图问题的当前验收边界

- Worker 身份修复已经过真实打包和安装：包内六个实际程序集、Playnite 扩展目录中的插件/Worker/共享 DLL，以及运行中命名管道握手均为同一个最终打包 HEAD 身份。`scripts/package.ps1` 会在打包前从 PE 元数据读取实际身份，同源不一致或当前工作树不干净时停止，不生成混合包。
- 实际 Playnite 安装目录为 `C:\Users\lopmatu\AppData\Roaming\Playnite\Extensions\GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec`。运行中唯一 Worker 的 `system.handshake` 和 `media.inbox.page` 均成功；后者返回真实数据 `totalCount=4615`。受控停止唯一已核实路径后，管道连接失败，宿主恢复后再次握手成功。
- 媒体页代码已收口为有限 PageHost 与左右独立滚动；离线状态不再把读取失败显示成真实 0。RenderHarness/源码门禁通过，但真实 Playnite 窗口内的导航、尺寸切换、选择/历史滚动和截图尚未完成。
- 32 项扩展计划保持暂停。没有 CUA 宿主操作证据时，后续交接必须写“代码修复已完成，真实视觉宿主验收待完成”，不得写“用户截图问题已解决”。

## 2026-09-08 用户截图反馈修复约束

- Worker 的单实例互斥语义是：重复进程可以以退出码 0 结束，但 Launcher 不能据此直接报告启动失败。启动子进程发现退出码 0 时，必须在有限等待内用期望版本/构建身份探测现有实例；健康则复用并清理本次子进程引用，不健康才保留真实错误。
- 媒体待归类页的 DataGrid 必须在表格卡片顶部正常出现，不能因外层页面 ScrollViewer 的无限测量和父级 `*` 行被排列到卡片底部。表格卡片/内部布局/DataGrid 使用顶部对齐；DataGrid 仍保持固定的可读最小高度、内部滚动和既有虚拟化契约。
- Production Shell 媒体探针必须同时检查 PageHost、页面滚动方向、DataGrid 最小高度以及表卡到 DataGrid 的顶部间距；只检查 DataGrid `ActualHeight` 不足以发现“表格被推到首屏之外”的布局回归。
- 本轮验证：Release 全量 Core `72/72`、Worker `303/304`（1 跳过）、Playnite `369/431`（62 跳过）；XAML `19/19`、源码校验、WPF `0 errors/21 warnings/172 info`、双主题/多尺寸/resize/Production Shell `render-qa OK`。真实 Playnite、DPI/高对比度和完整键盘仍属外部验收边界。

## 2026-09-08 连续开发执行约定

- 用户希望后续无需完成一点就重新要计划，新增 [32 项连续开发队列](CONTINUOUS_DEVELOPMENT_PLAN_2026-09-08.md)。L01～04 对应已确认 Q6，后续为有依赖/验收条件的增强或验证任务，不能都当作既有缺陷。
- 默认完成一项验证、同步文档并提交 push 后继续下一项。仅在必要产品决策、外部条件或授权边界出现时询问；阻塞项不阻断无依赖任务。已满足部分核对证据后跳过。
- 本轮只规划；不代表启动后台开发，不新增真实存档/云端写入或安装授权。后续用任务状态/commit/证据/阻塞表保持可续跑。

## 2026-09-08 Q6 收口计划与证据边界

- 下一轮依据 [97131f0 独立质量复核](QUALITY_REVIEW_2026-09-08.md)，不重复重做 Q4/Q5。新状态面板的重试不能继承 `IsHitTestVisible=false`；成功读取时间和错误必须按游戏/模式隔离；运维状态先按身份与新鲜度归并再筛告警。
- 构建身份底座不等于发布链已验收：SkipBuild 必须核对旧插件与新 Worker 的实际身份，`+unknown` 必须显式处理。报告将脚本推导问题与真实安装验收区分。
- 本轮全量构建/测试、静态和离屏检查通过；Fake 尚未覆盖完整新增状态/运维绑定面。优先补行为测试和六态夹具，再优化设置首屏、运维密度和真实宿主性能。

## 2026-09-08 X2-03 构建身份实现约束

- 公共扩展版本仍由 `extension.yaml`/`VersionPrefix` 控制；构建身份是独立诊断字段，来自 `AssemblyInformationalVersion`，格式为 `版本+提交号`，没有提交号时必须显示 `unknown`，不得用它替代协议版本或擅自升级插件版本。
- `scripts/package.ps1` 必须让插件和 self-contained Worker 使用同一个 Git HEAD 构建身份；`WorkerHandshakeDto`/`WorkerPingDto`、Dashboard 快照和诊断包都要暴露该身份。旧 Worker 返回空身份时保持兼容，两个新构建身份已知且不一致时必须标记为不可复用并重新启动/提示。
- 当前发布窗口尚未执行安装替换；不能把本地打包或离屏检查写成已安装 DLL 验收。实际发布时需核对 `extension.yaml`、程序集版本、包内 Worker、握手身份和 Playnite 扩展目录中的文件来源。
- 当前自动验证：Release 构建 0 警告/0 错误；Core `72/72`、Worker `303/304`（1 跳过）、Playnite `368/430`（62 跳过）；WPF 静态 `0 errors/21 warnings/172 info`，`render-qa OK`。

## 2026-09-07 X2-02 运维总览实现约束

- “下一步运维”必须从真实 `HealthInspectionStateDto`、`CloudTransferStatusDto`/摘要和 `RetentionQuarantineEntryDto` 生成；每个动作都保留真实记录 ID 或明确导航目标，不新增一个没有对象边界的全局自动修复按钮。
- 云端项只在当前已加载的真实记录上显示详情；若摘要中还有未加载的关注项，必须显示“未全部加载”并把动作导向现有云端队列分页。重试、远端 check、认证处理继续使用原有命令与权限语义。
- 隔离账本再次协调只接受 `EntryId`，IPC 请求必须 `Confirmed=true`；Worker 仍检查原/隔离路径安全、文件大小/身份和账本状态，遇到冲突不得覆盖或删除未知文件。启动恢复与用户针对单条记录的协调共用同一状态机。
- 当前验证：Release 构建 0 警告/0 错误；Core `72/72`、Worker `303/304`（1 跳过）、Playnite `366/428`（62 跳过）；源码/XAML 门禁、WPF `0 errors/21 warnings/172 info`、`render-qa OK` 均通过。真实宿主、DPI/高对比度、完整键盘和真实故障注入仍是外部边界。

## 2026-09-07 X2-01 工作区状态实现约束

- 媒体当前列表、收件箱和维护诊断统一使用 `WorkspaceDataState` 的 Loading/Ready/Empty/Stale/Error/Offline 语义，并通过现有 `WorkspaceStatePresenter` 呈现；不要回退为只看集合 Count 或只显示全局 `StatusMessage`。
- Loading/首失败时可以覆盖内容区域；已有成功数据刷新失败必须保留旧集合、选择和编辑草稿，使用降级提示显示上次成功读取时间与错误详情，并把 RetryCommand 接回真实的媒体/诊断刷新命令。Worker 离线优先于普通错误状态。
- 状态变更必须受媒体分页/收件箱代际保护，旧请求不得把新游戏、新模式或新列表覆盖为 Stale/Ready；`RefreshCoreAsync` 的工作区级失败也要结束 Loading 状态。不要用 `Task.Delay` 制造成功或加载效果。
- 当前验证：Release 构建 0 警告/0 错误；Core `72/72`、Worker `302/303`（1 跳过）、Playnite `365/427`（62 跳过）；XAML `19/19`、源码校验、WPF 静态审查 `0 errors/21 warnings/172 info`、`render-qa OK`。离屏证据不能替代真实 Playnite、DPI/高对比度和完整键盘验收。

## 2026-09-07 媒体间距与任务范围实现约束

- 待归类页签的按钮不能贴住工作区边界：`MediaTabControl` 保留 `8,0,8,0` 外边距，页签保留 `16,8` 内边距和 `4` 间距；批量处理卡片保留 `14,12,14,0` 内边距，底部操作区保留 `16,12,16,0` 与 `0,8,0,0` 间距。后续只能在共享布局契约内调整，不要为压缩高度移除这些呼吸空间。
- Dashboard 的最近历史窗口与活动任务集合必须分开查询；活动任务包括 Queued、Running、WaitingForUser，并按稳定创建时间/任务 ID 排序合并。任务摘要 SQL 的云端等待数必须覆盖等待用户状态。
- 批量安全重试必须基于当前 `TasksView` 结果计算，按游戏和任务类型去重，并向用户说明当前结果总数、实际计划、去重数和未纳入数；不能回退为对首页最近任务集合盲目重试。
- 当前验证：Release 构建 0 警告/0 错误；Core `72/72`、Worker `302/303`（1 跳过）、Playnite `364/426`（62 跳过）；XAML `19/19`、源码校验、WPF 静态审查 `0 errors/20 warnings/172 info`、`render-qa OK`。离屏渲染不等于真实 Playnite、DPI/高对比度或完整键盘验收。

## 2026-09-07 任务页紧凑空间实现约束

- `TaskCenterView` 的紧凑详情按钮不能与 `TaskGrid` 共享一个会发生溢出的有限行：详情关闭时使用队列底部按钮，详情打开后必须隐藏该按钮并在 `TaskDetailCard` 内提供“收起详情”，避免按钮覆盖第三行。
- 紧凑详情仍保留 `TaskGrid.MinHeight=180`、36 DIP 数据行和 `TaskDetailScrollViewer.MaxHeight=160` 的可读性底线；Production Shell 探针必须按 `PageHostForAudit` 的实际 DataGrid 视口统计展开后的完整行，1040×700 至少 3 行，1100×720 至少 3 行。
- `TaskHasActiveFilters` 必须覆盖搜索、状态、游戏、类型、历史范围和时间范围，并在每个对应 setter 及清除路径通知；无有效筛选时只隐藏清除按钮，不能隐藏“更多筛选”或断开真实 `ClearTaskFiltersCommand`。
- 当前验证：紧凑详情定向 `4/4`；Core `72/72`、Worker `300/301`（1 跳过）、Playnite `364/426`（62 跳过）；XAML `19/19`、源码校验、WPF 静态审查 `0 errors/20 warnings/172 info`、`render-qa OK`。真实 Playnite、DPI/高对比度、完整键盘和大库连续滚动仍是外部验收边界。

## 2026-09-07 质量计划状态同步

- 当前质量计划 `QUALITY_REVIEW_2026-09-07.md` 已将 Q4-01～Q4-03 的历史待办改为已完成状态：媒体云端重试独立 IPC/结构化结果、目标标签导航、维护页紧凑详情折叠与 Production Shell 离屏探针均已落地并有回归证据。
- 后续阅读计划时，只把真实 Playnite、Rclone/远端、用户数据、DPI/高对比度和完整键盘流程视为外部人工验收边界，不要重新实现上述已完成代码。

## 2026-09-07 动效门控实现约束

- `AcrylicProductionShellView.NormalizeMotionIfDisabled()` 是侧栏动效的统一终态入口：当 Dashboard 的设置或 Windows 动画偏好变为关闭时，必须取消 `ColumnDefinition.Width`、内容层 `Opacity`/`TranslateTransform.X` 的活动时钟，恢复内容不透明、当前侧栏宽度和 `sidebarTransitionRunning=false`。
- Dashboard 的 `ApplyAdaptiveTheme()` 在传播 `MotionEnabled` 后调用生产 Shell 的终态清理，并清理自身已知的入口、游戏筛选、详情页、状态胶囊、对话框和任务详情过渡。动画关闭只跳过视觉过渡，不改变页面可见性、命令、Binding 或业务状态。
- 不要把动画时长改成 `DynamicResource` 直接塞入 Storyboard；WPF 会尝试冻结跨线程时间线并在测试/宿主中抛出冻结异常。当前安全路线是代码级门控、即时终态清理和资源级 PopupAnimation `Fade/None`。
- 行为测试必须在真实 STA `Window` 中验证关闭动画后的最终几何和无活动过渡；离屏渲染不能证明 60fps。当前动效门控定向 `7/7`，全量 Playnite `363/425`（62 跳过），WPF 静态审查 `0 errors/20 warnings/172 info`，RenderHarness `render-qa OK`。最新 `artifacts/GameSaveCenter-0.6.73.pext` 已重新打包并完成包内版本/必需文件校验，未安装到 Playnite；真实 Playnite/DPI/高对比度/键盘和流畅度仍是外部验收边界。

## 2026-09-07 Q4-00 媒体分页锚点行为实现约束

- `MediaCenterView` 的延迟 `RestoreAnchor` 回调必须携带 `anchorRestoreGeneration`；切换游戏、收件箱模式、ViewModel、页面生命周期或产生新的集合/选择上下文时递增代际并统一清除待恢复锚点、选择和计时器。旧回调只能静默退出，不能改动当前列表。
- 收件箱恢复除代际外还要校验 `MediaInboxMode`；`PropertyChanged` 订阅必须随 ViewModel Attach/Detach 成对管理，避免离开页面后旧 ViewModel 继续触发失效逻辑。
- `selectionRestoreQueued` 在整个恢复/重试链路中保持占用，只有恢复成功、无法恢复并显示提示、或上下文失效时释放。不要在单次 `RestoreSelection` 的 `finally` 中提前清零，否则集合 Reset 的后续 `SelectionChanged` 会覆盖待恢复选择。
- 行为测试必须在真实 STA `Window` 中驱动 View、Dispatcher 和私有恢复链路，至少验证旧上下文回调不显示过期提示，以及锚点被裁掉时提示可见且恢复锁释放。源码契约断言只能作为补充，不能替代该行为证据。
- 当前证据：媒体锚点定向 `5/5`；全量 Core `72/72`、Worker `300/301`（1 跳过）、Playnite `362/424`（62 跳过）；构建 0 错误，存在 1 个 `NU1900` 网络审计警告。真实 Playnite、DPI/高对比度、完整键盘和大库连续滚动仍是外部验收边界。

## 2026-09-07 Q5-01 设置页操作反馈实现约束

- 设置页状态必须由当前设置指纹、`VerifySettings` 结果和 Playnite 编辑生命周期共同决定：验证错误优先于脏状态；无错误且指纹与提交基线不同显示未保存；相同显示已保存。不要添加绕过 Playnite 的自定义保存按钮。
- `GameSaveCenterSettings.CreateSettingsFingerprint()` 必须排除 `DeviceId`，因为它是安装身份而非用户可编辑设置；`SettingsCommitted`/`SettingsReverted` 只在 Playnite 的 `EndEdit`/`CancelEdit` 边界更新基线。导入 Portable JSON 仍是当前编辑缓冲区的修改，DataContext 重绑不得清掉未保存状态。
- `SettingsSaveHintText` 在所有高度保持可见，矮窗口只隐藏冗长副标题/说明，不隐藏保存语义；状态文字需有 AutomationProperties.Name 与 Tooltip，并保持双主题资源可用。真实宿主仍需检查 Playnite 保存/取消后的回写、DPI 和键盘焦点。
- 当前验证：RenderHarness 双主题、多尺寸、resize `render-qa OK`；全量 Core `72/72`、Worker `300/301`（1 跳过）、Playnite `360/422`（62 跳过）；XAML 19/19、源码校验和差异检查通过。离屏结果不能当作真实 Playnite 宿主验收。

## 2026-09-07 Q4-04 动态分页一致性实现约束

- `cloud_transfers` 与 `classification_history` 使用 SQLite 持久化修订号，不使用动态查询结果里的 `strftime('now')` 或 `MAX(updated_utc)` 充当快照标识；旧库启动时必须创建 `query_revisions`、种子行和幂等触发器。
- 云端修订由 `cloud_transfer_queue`、`cloud_retry_queue`、游戏名称/云端状态、媒体归属/云端/归类状态变化触发；归类历史修订由批次和批次条目的增删改触发。筛选共用全局修订号，宁可要求刷新也不能静默漏项。
- 两套分页响应必须同时带 `ConsistencyToken`、`PageResetRequired` 和 `PageResetReason`。Worker 在读前拒绝旧 token，在读后发现修订变化也返回 reset；页面不能用空结果宣称“已加载全部”。
- Playnite 继续按稳定 `TransferKey`/`BatchId` 恢复选择。reset 时清空已加载窗口、提示用户列表已变化并自动从第一页重试一次；不要改成拉取全量列表，也不要把可变排序键游标当成一致性快照。
- 当前只验证 Worker/离屏客户端链路；真实 Playnite 宿主、跨进程持续写入、DPI/高对比度和人工键盘仍是外部验收边界。

## 2026-09-07 Q4-03 紧凑维护页详情布局实现约束

- 诊断和进程映射的紧凑断点为 PageHost 宽度 `< 980` DIP。选中行不能自动让 Inspector 进入 Auto 行；默认必须保留列表，详情只能通过命名的紧凑按钮展开，详情打开后才占用有限的第三行空间。
- `FindingsGrid`/`MaintenanceProcessGrid` 的选择变化必须关闭旧详情，`Esc` 关闭当前紧凑 Inspector；按钮提供 AutomationProperties.Name，Tab/Shift+Tab 交给 WPF 键盘导航。宽度恢复到 980 以上时回到并排详情，不能保留紧凑抽屉状态造成空列。
- 进程详情的唯一滚动所有者是 `MaintenanceProcessInspectorScrollViewer`，诊断继续由 `MaintenanceDiagnosticsInspector` 承担；不要再把详情拆成多个竞争滚动条。列表行数回归必须统计可视树中实际可见且有高度的 `DataGridRow`，不能只断言 `ActualHeight`。
- RenderHarness 的 `RunProductionShellMaintenanceProbe` 必须使用 `AcrylicProductionShellView.PageHostForAudit`，覆盖 1040×700、1100×720、1366×768，并同时验证紧凑默认关闭、按钮可见、打开后可见及至少 3 个完整行；离屏结果仍不等同真实 Playnite。

## 2026-09-07 Q4-01/Q4-02 媒体重试与目标标签导航实现约束

- 媒体云端重试必须走 `MessageTypes.RetryMediaCloudUpload` 和 `MediaCloudRetryRequestDto`，Worker 只调用 `MediaSyncService.RetryCloudUploadForUserAsync`；`MessageTypes.RetryCloudUpload` 继续只服务备份，禁止用 `SyncMedia` 冒充“重试上传”。该入口不能扫描来源或归类新文件。
- `MediaCloudRetryResultDto.Outcome` 是 UI 的事实来源：`Submitted` 才能显示提交/完成；`PausedByPolicy` 必须说明游戏策略未允许上传；`CannotSubmit` 必须展示安全模式、全局开关、Rclone、失败或取消原因，不能无条件写成功提示。
- `MediaTabIndex` 初始保留当前游戏媒体页，`SaveTabIndex` 初始保留历史页；首页待归类动作设置媒体索引 0，诊断存档路径动作设置存档索引 1，且必须在切换 `CurrentWorkspace` 前设置，让生产 Shell 的页面绑定不会先落到默认标签。
- 本阶段定向测试覆盖 Worker 策略暂停/云端不可用不创建任务，以及 Playnite IPC/Tab 绑定契约；仍需真实 Playnite 宿主、真实 Rclone、DPI/高对比度和用户数据验收。

## 2026-09-07 质量复核补审更新

- UI3-07 已提交 9e93909；独立补审定向测试 5/5，但新增的 View 契约测试仅查源码字符串。下一步以 [质量报告 Q4-00～04](QUALITY_REVIEW_2026-09-07.md) 为收口计划，先补锚点真实行为与上下文切换验收。
- `docs/design/reviews/2026-09-07-quality/` 属于 b0aa85a 的完整离屏审计，不能归于 UI3-07；后续真实宿主证据另记版本与画布。

## 2026-09-07 UI3-07 缓存窗口与滚动锚点实现约束

- `MediaCenterView` 的“加载更多”不是纯粹追加：点击事件必须先捕获当前列表可见首项、相对位置/逻辑偏移和多选 ID；`MediaPageAccumulator` 发出 Reset 后，由视图按 `VirtualizingWrapPanel` 的像素偏移或 DataGrid 的逻辑 item 偏移恢复。不要用一个像素公式同时处理两种滚动模型。
- 当前游戏媒体、未归类收件箱、已忽略收件箱各自保持既有 2000 项窗口。窗口裁掉锚点时必须显示“返回最新/重新载入较新内容”路径；`ReloadMediaWindowCommand` 和 `ReloadMediaInboxCommand` 只重新请求首批，不自动修复、删除或修改媒体。
- 多选语义是“仅当前保留窗口”：按收件箱模式分开保存 ID，恢复只将仍在 `Items` 中的项目加入控件选择；批量操作继续从当前 `SelectedItems` 取值，不能让被裁掉的 ID 被静默执行。SelectedMedia、SelectedInboxMedia 和未保存备注/收藏草稿仍归 ViewModel 管理。
- 集合 Reset 后的 WPF SelectionChanged 不能覆盖待恢复 ID 集合；`selectionRestoreQueued` 必须在恢复或 15 秒无变化超时后释放。离开页面需解除集合事件，避免旧 ViewModel 接收事件。
- RenderHarness Fake 必须暴露新增恢复命令、分页状态和摘要；RenderHarness 只证明合成壳层的布局/夹具，不等同真实 Playnite。真实大库第 2/11 页滚动、多选、快速切换、编辑中加载、DPI 和高对比度仍属人工验收边界。

## 2026-09-07 UI3 质量复核与后续边界

- [质量报告](QUALITY_REVIEW_2026-09-07.md) 以 `b0aa85a` 为冻结基线；UI3-07 并发改动不属于该次验证。报告只新增文档和原始证据，不改变生产行为。
- 后续优先补媒体重试的独立 IPC/真实结果反馈，以及媒体收件箱、存档路径页的明确 Tab 路由；现有 workspace 跳转不等于到达按钮承诺的位置。
- 紧凑维护 Inspector 挤压列表在独立小画布中复现；须先扩展生产 Shell 几何检查，不能把独立页面场景名当成真实宿主尺寸。动态 offset 分页漏项是静态推导边界，后续先补变更回归再改一致性契约。
- 全量构建/现有测试通过不代表真实宿主、DPI、动画流畅度或 UI3-07 已验收；下一轮遵循报告的分阶段计划，不重复重做 UI3-00～06。

## 2026-09-07 UI3-06 存档与维护详情实现约束

- `SaveCenterView` 的历史时间展示使用 `MM-dd HH:mm`，完整时间只通过 Tooltip 提供；`SaveHistoryTimeColumn` 在窄宽度仍不得低于能读出日期/时间的宽度，类型和状态列优先于备注，备注继续使用星号列。Inspector 中恢复可用性必须先于备注编辑，但安全恢复命令、隔离校验、确认和 PreRestore 保护不能被 UI 重排绕过。
- `MaintenanceView` 的主诊断表只显示等级/游戏/问题三列；`Detail` 与 `SuggestedAction` 只能在选中 Inspector 中完整换行。`ApplyFindingsColumnLayout` 的主表分支按三列处理，审计表仍保持自己的四列契约，不要把两者混用。
- 诊断跳转统一经过 `FindingNavigationResolver`：带云端/Rclone/远端身份进入云队列，任务/Worker/健康或明确任务提示进入失败任务筛选，有游戏身份的其他诊断进入存档工作区并选中同一游戏；未知或无身份返回 `None`。动态按钮只隐藏/显示，不复制多套命令或自动执行修复。
- `CloudTransferStatusDto`、`CloudTransferSummaryDto` 和媒体归类 DTO 只改变显示映射，不改变状态存储/IPC 值；校验失败与上传失败必须可区分，未知未来状态不得把内部英文枚举直接泄漏到界面。设备说明不得写死设备数量。
- UI3-06 当前证据：全量 Core `72/72`、Worker `296/297`（1 跳过）、Playnite `352/414`（62 跳过），XAML `19/19`，WPF 静态审查 0 error，RenderHarness 双主题/多尺寸/resize `render-qa OK`。离屏渲染不等同真实 Playnite，下一阶段继续 UI3-07 缓存窗口与滚动锚点。

## 2026-09-07 UI3-05 首页优先级与下一步动作实现约束

- Hero 的优先级唯一来源是 `OverviewPriorityResolver.Resolve(snapshot, isOnboardingPending)`；不要在 XAML 重新按多个计数拼接互相竞争的主标题或命令。顺序固定为 Worker、Onboarding、Cloud attention、Unassigned media、Warning games、Healthy refresh。
- `DashboardViewModel` 暴露 `OverviewPriorityKind/Title/Description/ActionText/ActionToolTip/ActionCommand`，快照刷新和 onboarding 完成/跳过都必须通知这些派生属性。`OpenMediaWorkspaceCommand` 只切换到真实 `WorkspaceKind.Media` 并调用现有工作区加载路径。
- 首页 Hero 只保留一个动态主操作；全局工具栏处理全部备份/媒体同步，当前游戏卡处理当前游戏备份/详情刷新，关注项保留在风险卡上下文入口。不要恢复被移除的工具栏、当前游戏卡关注按钮或首次环境检查重复横幅。
- 离屏夹具必须把 `CloudTransferViewSummary` 回填到 `Snapshot.CloudTransfers`，否则首页 Hero 会错误地落入媒体/游戏分支。RenderHarness 只是合成壳层证据，真实 Playnite、DPI、高对比度和用户快照仍需人工复核。

## 2026-09-07 UI3-03 媒体归类批次历史与可找回撤销

- `media_classification_batches` 已有的 durable 记录现在通过 `GetMediaClassificationBatchHistoryAsync` 聚合成分页摘要；查询不加载全部项目明细，按批次更新时间和批次号稳定排序，状态筛选不会改变聚合语义。
- `MediaClassificationBatchSummaryDto.IsUndoable` 只对 `Applied`/`AppliedWithConflicts` 且仍有 `Applied` 条目的批次为真。撤销服务仍只接受这两个状态；撤销后 `UndoneWithConflicts` 与冲突计数保留为不可覆盖的历史事实。
- Dashboard 的预览、应用、撤销分别更新“预览对象”“最后应用批次”和历史选中项；不要把预览 ID 当作可撤销 ID，也不要在撤销有冲突时无条件清除批次上下文。
- UI 历史列表使用有限 `ListBox`（最小 236 DIP，最大 260 DIP）并保留内部滚动；筛选变化通过 code-behind 触发刷新，历史请求拥有独立 generation/CTS，离开 Media workspace 必须取消。

## 2026-09-07 UI3-04 任务中心筛选与密度实现约束

- `TaskCenterView` 在 `<760 DIP` 时只把搜索、状态和刷新留在主工具栏；类型、历史范围、时间范围和游戏筛选放入 `TaskMoreFiltersHost`。不要复制 ComboBox 以实现响应式布局：`SetCompactFilterPlacement` 会将同一批真实控件从 `TaskFiltersPanel` 移到 StackPanel，宽屏再移回，Binding/选择状态因此连续。
- `TaskActiveFiltersSummary` 必须随搜索、状态、游戏、类型、范围和时间变化通知；收起的 `TaskMoreFiltersExpander` 仍需显示生效条件并提供 `ClearTaskFiltersCommand`，不能因为隐藏控件而隐藏已生效条件。清除按钮位于 Expander header，点击要阻止误触发折叠切换。
- 任务时间列使用 `TaskTimeCell` 显示 `MM-dd HH:mm`，Tooltip 保留 `yyyy-MM-dd HH:mm:ss`；时间/状态/进度的最小宽度优先于详情，详情可以省略但不能改变真实数据。紧凑模式使用 `TaskCompactDataGridRow`（36 DIP），宽屏必须恢复 `GscStableDataGridRow`，不要修改共享表格行高来迁就单页。
- `FakeDashboardData` 需要覆盖 `TaskTotalCount`、`TaskHistoryScopeOptions`、`TaskHistoryRangeOptions`、`TaskLoadedSummary`、`TaskHistoryHasMore` 和任务详情/批量命令；夹具字段为空不应被误判为生产 Binding 缺陷。最终离屏只能说明合成壳层几何，不能代替真实 Playnite 宿主、DPI 或高对比度验收。

## 2026-09-07 UI3-02 云端队列明细分页与用户操作入口

- 生产维护中心现在有独立的云端队列 Tab。`DashboardViewModel.CloudTransfers` 以 `CloudTransferStatusRequestDto` 调用 Worker 的 `GetCloudTransferStatus`，页大小固定请求 100，服务端聚合总量，客户端按 `TransferKey` 去重追加并在刷新时恢复当前选择。
- `CloudTransferStatusDto` 增加面向 UI 的备份/媒体类型显示；列表和 Inspector 同时显示状态详情、错误码、下次尝试和保证级别，`Uploaded` 不得写成“已验证”。
- `VerifyCloudTransferCommand` 使用 `VerifyCloudTransfer` 只读 check；`RetryCloudUploadCommand` 对 Backup 使用既有云上传重试，对 Media 使用 `SyncMedia` + `UploadAfterSync`，认证需处理状态不被当作自动可重试上传。请求有独立 CTS 和代际，离开维护页会取消云队列请求。
- `MaintenanceView` 宽屏为表格 + Inspector，低于 980 DIP 时表格保持有限高度，详情通过 `CloudTransferCompactDetailsButton` 打开并堆叠；RenderHarness 已加入真实状态夹具和维护页 Tab 索引回归。验证基线为 Core `65/65`、Worker `295/296`（1 跳过）、Playnite `342/404`（62 跳过）、XAML `19/19`、RenderHarness `render-qa OK`。
- 未取得真实 Playnite 宿主、真实 Rclone 凭据/远端、DPI/高对比度证据；下一阶段按 UI3-03 处理可找回的媒体归类批次历史。

## 2026-09-07 UI3-00/01 媒体收件箱可见性与紧凑布局

- 媒体收件箱拆分为“选择/视图、目标/主操作、次级动作”三层：主操作栏在窄屏保持单行，次级操作和加载统计位于表格之后；详情面板在不足以同时容纳列表与 Inspector 时默认折叠，由 `MediaInboxCompactDetailsButton` 打开并堆叠到表格下方。
- `MediaInboxPageScrollViewer` 使用 `GscPageScrollViewer`，`MediaInboxGrid` 仍是 `Tag=FiniteViewport` 的共享 `MediaDataGrid`，代码按实际页面高度赋予有限 Height/MaxHeight。不要把整个 DataGrid 放回无限测量的外层 ScrollViewer，也不要删除页内滚动入口来追求假性首屏填满。
- RenderHarness 现将 measured PageHost/content 高度传给页面协调器，并在真实 `AcrylicProductionShellView` 内验证 1040×700、1100×720、1366×768 的 PageHost、滚动 extent、表格高度；`UiLayoutAnalyzer` 通过祖先裁剪交集检查表格和动作控件是否真的可见。`validate-source.py` 也已认识新的页面滚动契约。
- 当前证据：`render-qa OK`，双主题/多尺寸/resize/壳层几何通过；UI audit 154 个运行时快照，Fidelity 0、失败路由 0、媒体页无 HIGH；Playnite `342/404` 通过、62 跳过。截图见 [`docs/design/reviews/2026-09-07-ui3/`](../design/reviews/2026-09-07-ui3/)。未运行真实 Playnite 宿主，不能把离屏结果写成宿主验收。

## 2026-09-06 FLiNG 后台下载 403 修复

- 复查确认详情页正常、下载文件链接 403 与 Worker 请求特征不完整相符：原实现没有 Cookie 会话和详情页 Referer，且只发送 `GameSaveCenter/0.5` User-Agent。
- 下载源现在显式维护 `CookieContainer`，所有页面/文件请求使用浏览器风格 User-Agent、Accept 和 Accept-Language；下载前从 `release.CatalogId` 找到官方 `/trainer/` 页面并预热，会话 Cookie 与 Referer 一起用于文件请求。
- 仍保留 `EnsureFlingUri` 的 HTTPS/主域及子域约束、重定向后的最终地址校验和 2 GiB 大小限制；403 映射为 `FLING_DOWNLOAD_FORBIDDEN`，便于任务页区分站点拒绝与网络错误。
- 同时修复 `GameToolService` 下载失败时临时 `.download` 文件未进入 finally 的边界；现由下载到安全解压/绑定的完整流程统一清理。
- `FlingTrainerCatalogSourceTests.Download_UsesTrainerPageSessionAndReferer` 覆盖请求顺序、来源头、User-Agent 和文件落盘；定向 `12/12`，Release 全量 Worker `295/296`（1 跳过）。未取得真实 Worker 在线下载或 Cloudflare/验证码挑战证据。

## 2026-09-06 Luna 实现后 UI 复查（仅审阅）

- 基线 `0ac0e39`：抽查 V2 已实现代码与测试，新增 [UI3 可见优化任务包](UI_REVIEW_V3_2026-09-06.md)，不重新执行已完成的 V2 修复。
- 本轮实际离屏审图发现媒体紧凑按钮/表格视口不足；云队列明细后端未接到生产分页 UI；归类“上次批次”内存入口会被新预览替换，需要可找回历史。任务夹具空数值/范围是 FakeDashboardData 缺属性，不直接认定生产缺陷。
- RenderHarness OK 不等于用户可见区域验收，UI3-00 要补祖先裁剪交集与实际生产 Shell 几何；六张原始截图与报告纳入 `docs/design/reviews/2026-09-06-v3/`。
- 本轮仅文档，Release 0 warning/0 error、Core 65/65、Worker 294 通过/1 跳过、Playnite 341 通过/62 跳过、XAML 19/19、源码门禁通过；未安装/操作用户数据，真实宿主仍待验收。
> 本文件面向新的 AI/Codex 会话，目标是在几分钟内恢复项目状态，避免重复实现已完成的工作。

> 当前事实入口：先读 [`CURRENT_STATE.md`](CURRENT_STATE.md)。本文保留按阶段的历史约束和证据；若与当前事实入口或最新代码冲突，以 `CURRENT_STATE.md` 的覆盖说明为准，不要按旧条目恢复已撤销布局或外部 Demo 路径。

## 2026-09-06 V2-07 媒体多页累积成本与有界窗口

- `MediaPageAccumulator` 为媒体主列表、未归类收件箱和已忽略收件箱分别维护 ID 索引；首个游标页替换，后续页只对相同 ID 更新并追加新项，不再按已加载集合执行全量合并/替换。
- 每个缓存默认保留最近 2000 条。裁剪时保留当前选中媒体，`MediaLoadedSummary`/`MediaInboxLoadedSummary` 明确区分当前保留数、服务端总数和窗口上限；游标和总数继续由服务端分页状态驱动。
- `BatchObservableCollection.ApplyBatch` 将一页内的更新、追加和裁剪收敛成一次 Reset 通知，避免逐项触发 UI 重排；索引更新保持与集合位置一致。
- 回归覆盖 250 页 × 200 项的 5 万条输入、2000 条窗口、选中项保留，以及重叠 ID 更新不重复。隔离 Release 全量为 Core `65/65`、Worker `294/295`（1 跳过）、Playnite `341/403`（62 跳过）、XAML `19/19`，构建无警告/错误，源码/XAML/差异门禁通过。
- 未取得真实 Playnite 大库滚动回收、DPI、目标机帧率或长时内存证据；不得将有界集合回归写成真实宿主性能验收。

## 2026-09-06 V2-06 云队列全量摘要与独立分页

- `SqliteStateStore.CloudTransfers` 使用统一 CTE 合并 durable 新队列、legacy 重试表和游戏/媒体基础云状态；按 `transfer_key`（不区分大小写）以新队列优先去重，再由 SQL 聚合全量状态计数和最早重试时间。
- `CloudTransferStateService.GetStatusAsync` 保留无参数调用作为首页兼容入口，同时接受 `CloudTransferStatusRequestDto` 的页码、页大小、状态和类型过滤；摘要总数与分页明细查询相互独立，页大小服务端限制为 100，并返回已加载量和是否还有下一页。
- 旧备份重试表仍按原有错误分类显示认证需处理或下次尝试；基础游戏/媒体状态只在无更高优先级 durable 行时补入，避免新旧来源重复计数。
- 新增 1005 条记录、旧新同键、摘要最早重试、分页末页和失败过滤回归；隔离 Release 全量验证为 Core `65/65`、Worker `294/295`（1 跳过）、Playnite `339/401`（62 跳过）、XAML `19/19`，源码校验与差异检查通过。
- 未取得真实 Playnite 大库 UI 首屏、跨进程并发或真实宿主证据；后续处理 V2-07。

## 2026-09-06 V2-05 云端校验终态与代际保护

- `cloud_transfer_queue` 新增操作类型、操作 ID 和校验前快照字段。上传使用 `Upload` 代际，远端只读校验使用独立 `Verify` 代际；校验在等待全局闸门前就持久化为 `Verifying`，避免 durable 状态与实际操作不一致。
- 校验取消、工具启动/执行异常和失败结果均有明确收尾：可恢复时恢复校验前的 Uploaded/RemoteVerified/RetryScheduled 等快照，失败结果记录 `CheckFailed` 或 `AuthenticationRequired`；恢复失败也不会提升云端保证。投影到游戏行的状态使用尽力路径。
- 校验结果以操作 ID CAS 写回；更晚的上传代际接管后，旧校验不会覆盖队列。Worker 重启只恢复 Upload 型 Pending/Transferring；Verify 型 Verifying 恢复快照或 `CheckFailed`，绝不因校验恢复而排入上传。
- 新增五项云端校验行为回归及旧队列迁移覆盖；隔离 Release 全量构建 0 warning/0 error，Core `65/65`，Worker `293/294`（1 跳过），Playnite `339/401`（62 跳过），XAML `19/19`，源码校验与差异检查通过。
- 真实 Playnite、真实云端凭据/断网、硬杀和跨进程并发证据仍未取得；不要把这些 Worker/SQLite 回归写成真实宿主验收。

## 2026-09-06 V2-01 媒体归类提交与恢复协调

- `media_classification_operations` 记录归类/撤销的操作意图、源/目标路径、哈希、原始文件标记和目标预存状态；媒体行、批次条目和账本状态在 SQLite 同一事务内从 `Moved` 提交为 `Committed`。
- `WorkerInitializationService` 在任务/云队列恢复前调用 `MediaSyncService.RecoverPendingClassificationOperationsAsync`。启动时能确认业务已提交则补齐账本，未提交且状态仍匹配则按账本恢复文件并标记 `Aborted`，无法唯一判断则保留 `RecoveryRequired`。
- 审计是提交后的尽力写入，失败只产生结果告警和 Worker 日志，不回滚已提交业务；取消回滚和文件协调使用独立非取消令牌。原始媒体作为源时只清理本次新建的归档副本。
- 新增 `MediaSyncServiceTests` 的 SQLite 触发器/取消回归：批次条目提交失败后启动恢复可恢复 Inbox 副本，审计失败仍保留 Assigned 副本，取消后文件和数据库回到 Inbox。V2-01 验证为 Release 0 warning/0 error、Core `65/65`、Worker `278/279`（1 跳过）、Playnite `339/401`（62 跳过）、XAML `19/19`、源码门禁与差异检查通过。
- 这仍不是真实 Playnite、用户媒体、断电或跨进程并发证据；后续 V2-02 起按复查包顺序逐项实施。

## 2026-09-06 V2-02 健康巡检调度与计划写入

- `HealthInspectionService` 后台循环在 `_runGate` 争用失败时等待 250ms 后重试，外围 `Get/SyncPlan/RunOne` 异常按 1s 退避隔离，应用停止时不进行无界重试。
- 计划和执行状态采用不同 SQLite 写入契约：`UpdateHealthInspectionPlanAsync` 只更新计划字段，`SaveHealthInspectionExecutionStateAsync` 只更新结果/游标字段；`CompleteAsync` 读取最新计划后设置下一次时间，避免并发设置修改被旧 DTO 整行覆盖。
- 定向回归用可控手动巡检闸门证明 700ms 内后台争锁次数受限，并证明执行写入不会覆盖已更新的启用/间隔/过期/预算；隔离 Release 全量验证为 Core `65/65`、Worker `280/281`（1 跳过）、Playnite `339/401`（62 跳过）、XAML `19/19`。
- 真实宿主长时调度与目标机资源压力仍需人工复核。

## 2026-09-06 V2-03 健康巡检候选公平与恢复游标

- `HealthInspectionService` 先按稳定的 `PlayniteId`、`CreatedUtc`、`BackupId` 顺序建立候选集，再按完整游戏/备份复合游标轮转；启动时若存在持久化 in-flight 游标，优先恢复该确切候选。
- 候选身份在检查会话、游戏锁和归档前通过执行状态契约落盘。状态写失败会停止本轮且不读取归档、不写入 Failed readiness/finding；审计和完成状态仍走尽力记录路径。
- `health_inspection_deferred_candidates` 为被运行中游戏或操作锁推迟的版本记录独立下次尝试时间，允许本轮转向其他游戏；无过期项时返回 `UpToDate`，不重复解包。
- 新增候选提前落盘、状态写失败、公平推迟、新鲜集合和 in-flight 恢复回归；定向健康巡检 `11/11`，隔离 Release 全量结果为 Core `65/65`、Worker `285/286`（1 跳过）、Playnite `339/401`（62 跳过）、XAML `19/19`。
- 本阶段仍未取得真实 Playnite、硬杀/断电、跨进程并发和长时调度证据；后续按复查包处理 V2-04～V2-07。

## 2026-09-06 V2-04 IPC 请求身份与重放指纹

- `ipc_request_ledger` 现在持久化 `protocol_version` 和规范化 JSON 负载 SHA-256 指纹。对象属性按序规范化、数组顺序保持语义；相同请求属性顺序变化可以安全重放。
- `ClaimIpcRequestAsync` 对已有 ID 同时核对 type、协议版本和 payload 指纹；不一致或旧行缺少指纹时返回冲突标记，`NamedPipeServerService` 返回 `REQUEST_ID_CONFLICT`，不会执行或重放旧结果。受保护写请求没有 ID 时返回 `REQUEST_ID_REQUIRED`。
- 旧表通过 `EnsureColumnAsync` 补齐新列；完成行 7 天后按每轮最多 256 行有界清理，中断行保留 30 天以便核对。Named Pipe 服务每小时执行一次只清理终态的维护任务，启动恢复仍单独负责将本进程外的 in-flight 标记为 Interrupted。
- 新增 RequestId 内容冲突、属性顺序、旧账本、迁移和保留策略回归；隔离 Release 全量结果为 Core `65/65`、Worker `288/289`（1 跳过）、Playnite `339/401`（62 跳过）、XAML `19/19`。
- 未取得真实 Playnite、跨版本旧 Worker/插件组合和硬杀端到端重放证据；后续处理 V2-05～V2-07。

## 2026-09-06 完成后复查（仅文档）

- 新增 [FOLLOWUP_REVIEW_2026-09-06.md](FOLLOWUP_REVIEW_2026-09-06.md)，基线 `8018cee`。确认上一轮主体已实现，另列 7 项源码边界和 3 项扩展，未修改生产代码。
- V2-01 媒体归类/撤销数据库提交后异常可能只补偿文件；V2-02/03 巡检忙等、外围异常隔离、游标持久化与公平性；V2-04 RequestId 内容指纹；V2-05/06 云端校验终态与完整摘要；V2-07 媒体多页累积成本。后续应先写行为复现测试，不能将审阅认定为已修复。
- 本轮 Release 0 warning/0 error，Core 65/65、Worker 275 通过/1 跳过、Playnite 339 通过/62 跳过，XAML 19/19、源码门禁通过。没有复跑真实宿主、规模矩阵或新增故障注入；Worker 重启和 5 项 IPC 行为测试本次跳过。

## 2026-09-05 UI-134 任务中心搜索文字垂直裁切修复

- `GscWpfUiTextBoxTemplate` 的 TextBox Padding 由原生 `PART_ContentHost` 消费，外层 `Chrome` 必须保持无 Padding；重复应用 `30,7,38,7` 会把 36 DIP 搜索框的文字 viewport 压到约 5 DIP。
- 本次只移除生产模板外层重复 Padding，任务搜索框尺寸、左右输入区、Foreground、Binding、清除按钮和键盘语义均未变；新增 STA 回归锁定高度 `36 DIP`、viewport 不小于文字 extent 和非透明前景。
- 自动证据：Playnite 定向 Release 测试 `126/165` 通过、`39` 跳过，源码/XAML/WPF 门禁通过；RenderHarness 双主题、多尺寸及 resize `render-qa OK`，修复后 viewport `19/18 DIP`。真实 Playnite 仍需人工复核。

## 2026-09-05 E01 规模性能基线

- `scripts/e01-scale-baseline.ps1` 是显式触发的隔离规模入口：默认不扩大普通测试夹具，`-Profile full` 使用 2,000 游戏/20,000 备份/10,000 任务/5,000 媒体/500 工具，`-Profile stress` 使用 10,000 游戏/20,000 备份/10,000 任务/50,000 媒体/500 工具；两档均写出 `worker-scale.json` 和 `baseline.md`。
- 2026-09-05 两档 Release 基线均通过：full seed/模拟 `105999/427 ms`，游戏首查/热查 `17/14 ms`、任务页首查/热查/下一页 `92/3/3 ms`、媒体页 `3/1/0 ms`；stress seed/模拟 `242832/1529 ms`，游戏首查/热查 `83/84 ms`、任务页 `93/3/2 ms`、媒体页 `4/0/1 ms`。两档还记录了任务/媒体搜索和查询/模拟分配量，托管内存保留增长、句柄/线程增长、订阅者和临时文件残留均受断言约束。
- 这是隔离 Worker/SQLite 数据量、查询首查/热查、搜索/分页、事件/原子写/操作锁模拟和资源增长的基线，不是 Playnite 宿主的冷/热首屏、UI 分配或帧间隔证据；真实宿主大库、DPI 和目标机目录仍按 `MANUAL QA REQUIRED` 处理。

## 2026-09-05 E01 行为证据矩阵与独立 Worker 重启验证

- `scripts/e01-behavior-matrix.ps1` 是自动证据分层入口：输出根默认在 `.tmp/e01-behavior`，业务、IPC、WPF/STA、故障/Soak 每个测试类单独记录；`-IncludeRender` 才运行 RenderHarness，真实 Playnite 始终写入 `MANUAL QA REQUIRED`。故障/Soak 组包含 `WorkerProcessRestartTests`。
- 受控真实 Windows 矩阵结果为业务 `44/44`、IPC `22/22`、WPF/STA `45/45`、故障/Soak `4/4`，整体进程退出码为 0；完整 Release 套件为 Core `65/65`、Worker `276/276`、Playnite `343/400`（57 项跳过）。
- `WorkerProcessRestartTests` 已证明真实 Worker 在随机管道和独立 Mutex 下硬停止后，第二个进程能用同一临时 SQLite 启动，并将未完成 Backup 标记为 `WORKER_RESTARTED_RETRYABLE`。为使该证据成立，Worker 补注册 `ITaskStatusStore`，管道名支持受校验的隔离覆盖而生产默认常量不变；客户端只对破坏性请求报告“可能已提交”。
- 已对当前用户 Worker 执行一次只读 `system.ping` 并收到成功响应，证明真实 Named Pipe 连通；没有停止或写入用户 Worker。E01 仍不等于 Playnite 宿主验收，双选择器、主题/DPI、睡眠唤醒与退出重启等真实宿主项仍需隔离环境复核。

## 2026-09-05 E02 当前事实入口与模块边界文档治理

- `docs/ai/CURRENT_STATE.md` 是新会话的短事实入口：当前版本 `0.6.73`，生产可见路径为 `DashboardView` 承载的 `AcrylicProductionShellView`，工作区和 Worker/Contracts/SQLite 入口均指向仓库实际文件。`GameSaveCenter.AcrylicFork/src/GameSaveCenter.Playnite/Design/` 当前缺失，不能作为编译、测试或逐像素证据输入；生产主题入口是 `Themes/AcrylicProductionResources.xaml` 及其 `DesignTokens`/`WpfUiProduction`/`Redesign` 资源。
- 当前有效的 UI/行为例外和保护边界在短入口集中说明：游戏选择器、既有滚动条、真实数据与安全确认语义保留；筛选使用 OneWay 显示 + `UiFilterSelection.Synchronize` + `DropDownClosed` 写回；历史记忆只作阶段证据，冲突时短入口与最新代码覆盖。
- `AGENTS.md` 已将 CURRENT_STATE 放在启动协议第一项；根 `docs/PROJECT_MEMORY.md` 与 `DEVELOPMENT_HANDOFF.md` 通过链接声明自身为历史归档。源码门禁会检查短入口中的版本、资源、缺失 Demo 和 `MANUAL QA REQUIRED` 标记。
- E02 阶段验证：Release 0 warning/0 error；Core `65/65`、Worker `275/275`、Playnite `338/400`（62 跳过）、XAML `19/19`，源码校验和 `git diff --check` 通过。真实宿主和大库证据仍未被文档治理替代。

## 2026-09-05 F03 媒体归类建议与可撤销批次

- `MediaSyncService` 的归类预览读取 Inbox 媒体、启用来源规则、会话区间和进程映射；规则目录/进程映射可给 High，时间范围或唯一文件名候选给 Medium，多个候选、未知时间或冲突给 Low 且不提供可应用目标。预览默认最多 200 项，批次有效期 30 分钟。
- `media_classification_batches` / `media_classification_batch_items` 保存原始媒体快照（状态、PlayniteId、版本、路径、云端状态、不可变元数据）以及建议/应用后快照。应用默认 `HighConfidenceOnly=true`，只处理仍匹配原快照的条目；手工覆盖或并发修改返回 Conflict，不覆盖用户选择。
- 归类移动的是归档目录中的副本，使用路径安全校验和同盘移动/复制回退；原始媒体和真实存档不删除、不移动。撤销从 SQLite 批次恢复原路径与元数据，只接受当前应用快照和 `Pending` 云端状态，批次可在 Worker 重启或预览过期后撤销；部分失败会保留逐项结果和审计。
- MediaCenter Inspector 的三个命令是预览、应用高置信建议、撤销上次批次；预览为空时列表折叠，不挤占空 Inspector 的首屏高度，非空列表保留 `FiniteViewport`、Recycling 虚拟化和内部滚动。手工 Assign/Ignore/Restore 入口不变。
- F03 阶段验证：Release 0 warning/0 error；Core `65/65`、Worker `275/275`、Playnite `338/400`（62 跳过）、XAML `19/19`、源码校验和 WPF 质量审计通过，RenderHarness 为 `render-qa OK`。未取得真实 Playnite 宿主、真实媒体目录、50,000 媒体压力和多进程并发证据。

## 2026-09-05 F02 云端队列可见与传输策略

- 备份与媒体复制统一写入 SQLite `cloud_transfer_queue`，键为 `Backup/Media + PlayniteId`；旧 `cloud_retry_queue` 继续兼容。Worker 启动恢复未完成的 `Pending/Transferring`，自动扫描同时覆盖旧备份队列和新媒体/备份队列，同一游戏同一类型只保留一条状态。
- `CloudTransferStateService` 负责状态、次数、原因、下次时间及远端保证等级；`Uploaded` 仅表示 copy 命令成功，`RemoteVerified` 才表示 `rclone check` 成功。认证失败为 `AuthenticationRequired` 且不进入自动到期扫描，校验失败只记录状态，不删改本地内容。
- 设置 `CloudUploadQueuePaused`、`CloudUploadAllowedStartMinute/EndMinute` 默认关闭暂停、全天允许；允许时段用本地分钟并支持跨午夜，后台队列以外时段延后，手动入口不受该策略限制。没有实现带宽上限/运行中游戏限速，因为当前没有锁定并验证的 Rclone 参数；安全命令边界仍是 `copy/check/lsf/cat/version`。
- F02 阶段验证：Release 0 warning/0 error；Core `65/65`、Worker `272/272`、Playnite `338/400`（62 跳过）、XAML `19/19`、源码门禁和 RenderHarness `render-qa OK`。目标机真实凭据、断网、Worker 硬重启和云端 check 仍需人工复核。

## 2026-09-05 F01 备份健康巡检与隔离恢复演练

- `HealthInspectionService` 是 Worker HostedService，计划/游标/最近结果位于 SQLite `health_inspection_state` 单例行；每轮只选一个新或超过有效期的备份，按 PlayniteId/创建时间/BackupId 稳定排序，游戏运行或 `GameOperationLock` 忙时只记录 `Deferred`，不读取归档。
- `LastStartedUtc` 在候选读取前落盘，`LastCompletedUtc` 只在终态落盘；Worker 非正常退出后下次启动识别 in-flight 状态并从同一游标重试。成功验证时间只在 `Ready` 更新，`Warning`/`Corrupted`/`Failed`/`Unsupported` 写入稳定的 `HEALTH_INSPECTION_FAILED` 关注项。
- `RestoreOrchestrator` 在真实恢复开始前读取目标版本的最近校验结果；已知 `Corrupted`/`Failed` 直接以 `RESTORE_READINESS_FAILED` 拦截，预览和未建立校验记录的旧/非 ZIP 版本保持兼容，不会因巡检入口自动执行真实恢复。
- 健康巡检复用 `RestoreReadinessService` 的 ZIP/Manifest/哈希/安全路径逻辑，隔离目录位于 `WorkerOptions.RestoreReadinessDirectory`，只在实际检查时创建；空间不足在解包前失败，清理状态明确为 `Pending`、`Cleaned` 或 `Retained`。这不代表真实进度或真实恢复已经演练。
- 设置 DTO/页面新增启用、间隔（15–10080 分钟）和重新验证有效期（1–3650 天）；最大单次预算由 Worker 固定校验范围（30–1800 秒）控制。维护页和报告展示“状态/最近成功/下次计划”，手动入口只执行隔离校验。
- F01 自动证据：Release 0 warning/0 error；Core `65/65`、Worker `264/264`、Playnite `338/400`（62 跳过）、XAML `19/19`、源码门禁和 RenderHarness `render-qa OK`。真实 Playnite、断电/硬杀时序、磁盘耗尽、分卷归档和长时资源压力仍待人工复核。

## 2026-09-05 U02 侧栏动画成本与快速操作终态

- `AcrylicProductionShellView.OnSidebarCollapseClick` 不再拒绝动画期间的后续点击；每次从当前 `SidebarColumn.ActualWidth` 接续到最新 72/270 DIP 目标。`sidebarTransitionGeneration` 使取消或卸载后的旧 `Completed` 回调失效，避免旧动画把新目标覆盖；无动画分支会清除宽度/透明度/位移动画后一次性落终态。
- `tests/GameSaveCenter.RenderHarness/Program.cs` 的侧栏探针统计 Measure/Arrange 次数与耗时、LayoutUpdated、Rendering 帧间隔，并对比 `MotionEnabledProvider=false` 的原子切换。独立 2000×1100 夹具单次 Arrange 约 `9.9ms`、快速往返最终宽度 `270`，原子切换 Arrange 约 `4.0ms`；因此保留当前过渡，暂不做大范围视觉简化。
- 阶段验证：Release 0 warning/0 error；Core `65/65`、Worker `260/260`、Playnite `338/400`（62 跳过）、XAML `19/19`，源码门禁和完整 RenderHarness `render-qa OK` 通过。探针不是真实 Playnite 帧率证据，长列表真机压力、DPI、高对比度和宿主卸载仍需人工复核。

## 2026-09-05 U01 任务页视口与状态试点

- `DashboardViewModel.TaskPageState.cs` 将任务页请求生命周期和展示状态独立出来：`Loading`、`Empty`、`FilterEmpty`、`Error`/`ErrorWithData`、`Ready`；刷新失败或刷新中的已有数据不被清空，并通过 `TaskPageLastUpdatedDisplay` 与 `TaskPageStatusSummary` 暴露旧数据时间和恢复入口。
- TaskCenter 在 1040×700、1366×768 等尺寸下保持主列表+Inspector 和窄屏紧凑详情路径；短高度将摘要 `MinHeight`/Padding 收紧，任务表最小高度保持 `236` DIP，列表内部滚动与 Recycling 虚拟化保留。状态层提供清除筛选、重试和无旧数据错误详情。
- 阶段验证：Release 0 warning/0 error；Core `65/65`、Worker `260/260`、Playnite `337/399`（62 跳过）、XAML `19/19`，源码门禁和 RenderHarness `render-qa OK` 通过。离屏夹具不是真实 Playnite 宿主证据；高对比度、实际 DPI、长历史性能和故障时序仍需人工复核。

## 2026-09-05 U03 游戏目录来源与新鲜度诊断

- `GameDescriptorDto` 新增 `PlayniteIsInstalled` 与 `InstallStateSource`；`PlayniteGameAdapter` 按 Playnite 原始标志、有效安装目录、有效本地 Play action 的顺序记录来源，`GameMatchInput` 不包含安装状态，安装状态变化不会使已有匹配失效。
- `GameCatalogService` 新增 descriptor-only 持久化、按 ID 诊断和单项匹配重试；描述同步不调用 Ludusavi，重试只读取并处理该 Worker 游戏。`SqliteStateStore.games.descriptor_synced_utc` 独立于匹配尝试时间，旧库通过 `EnsureColumnAsync` 升级。
- `GameDiscoveryDiagnosticDto` 只返回存在性、安装信号、匹配/备份摘要、时间和当前筛选条件，不包含完整私人路径；Playnite 插件在 Worker 不可用时仍返回本地来源事实。来源缺失只展示诊断状态，不能触发删除历史或备份。
- `GamePickerViewModel.GetFilterExclusionReasons` 与真实 `FilterItem` 共用判断逻辑；维护页提供按 ID 诊断、清除筛选、单项描述同步、单项匹配重试，普通刷新不改变现有全库同步策略。
- 阶段验证：Release 0 warning/0 error；Core `65/65`、Worker `260/260`、Playnite `335/397`（62 跳过）、XAML `19/19`、源码校验和 `git diff --check` 通过。真实宿主目标游戏、Worker 离线/重启及来源移除后的 UI 仍需人工复核。

## 2026-09-05 R07 IPC 取消与请求结果追踪

- `WorkerIpcClient`/插件请求入口现在接受调用者 `CancellationToken`，并联结插件生命周期；连接、写入和读取均使用可取消的 `Task.WhenAny` 竞态。取消回调只负责发出完成信号并把管道关闭排到线程池，避免在 `CancellationTokenSource.Cancel()` 中同步 Dispose 原生管道。
- `DashboardViewModel` 的选中详情、当前媒体分页、媒体收件箱分页会取消旧请求；generation/选中 ID 仍是响应回写的第二道边界。后台失败汇报忽略 `OperationCanceledException`，用户取消与 Playnite 退出不会弹失败提示。
- 破坏性 IPC 类型由 Contracts 集中分类；客户端超时/断管且请求可能已接收时用同一 RequestId 做有限重放，遇到 `RequestInProgress` 会短暂轮询。Worker `ipc_request_ledger` 保留 7 天，Completed 可重放，InProgress 重启恢复为 Interrupted；任务表新增 RequestId，Backup/Restore 的 TaskStatusDto 与请求关联，`TaskQueryDto.RequestId` 可精确查询。
- net462 继续使用 `NamedPipeClientStream.Connect(int)` 在线程池执行，不依赖现代 Stream 取消重载。真实 Named Pipe 行为套件有能力探测：当前沙箱客户端连接被系统拒绝时 5 项跳过；完整 Windows/Playnite 运行时应执行，不得把跳过当作真实行为验收。
- 阶段验证：Release 构建 0 warning/0 error；Core `65/65`、Worker `258/258`、Playnite `333/395`（62 跳过）、XAML `19/19`、源码校验通过。真实宿主、Worker 重启和长任务断线恢复仍需人工复核。

## 2026-09-05 R06 媒体按需分页

- Contracts 新增 `MediaQueryDto`/`MediaPageDto`；Worker 新增三类分页 IPC，按固定 `classification_state` 查询，支持 `PlayniteId`、`Kind`、`FavoriteOnly`、关键词和稳定 `(captured_utc, media_id)` 游标。`TotalCount` 不受当前游标影响，旧列表消息仍保留给兼容客户端。
- SQLite 新增 `ix_media_game_state_capture` 与 `ix_media_state_capture`，分页数据按 `captured_utc DESC, media_id DESC` 返回；查询计划测试证明当前游戏/收件箱路径命中新索引。搜索匹配路径、备注、媒体 ID 及可识别来源显示名，LIKE 通配符已转义。
- Dashboard 当前游戏首批 200 条，搜索/类型/收藏变化服务端重取首屏；待归类与已忽略集合各自保存游标、总数和 HasMore，模式切换、刷新、批量归类期间的旧页按代际丢弃。`MediaLoadedSummary`/`MediaInboxLoadedSummary` 显示已加载/总数，“加载更多”不改变现有多选、批量操作和 Item/Recycling 虚拟化。
- R06 验证：Release 0 warning/0 error；Core `65/65`、Worker `255/255`、Playnite `333/390`（57 跳过）、XAML `19/19`、源码校验和 `git diff --check` 通过。未运行真实 Playnite、50,000 项性能实验、4 MiB 消息边界和长时 UI 帧率测量。

## 2026-09-05 R05 游戏筛选下拉框 STA 行为验证

- `GamePickerFilterBehaviorTests` 在真实 WPF STA 线程中把筛选 ComboBox 挂到窗口，验证程序化关闭状态选中不会触发提交，以及打开→改选→关闭会触发 `DropDownClosed` 并得到最终选项。
- R05 未改生产行为：两套选择器仍使用 OneWay 显示绑定、`UiFilterSelection.Synchronize` 恢复程序化状态、`DropDownClosed` 作为唯一用户写回入口。脱离可视宿主的 ComboBox 不会自然触发关闭事件，测试夹具不可省略。
- 目前自动证据仅覆盖 WPF 控件基本提交时序；真实宿主 UI Automation Selection、字符搜索、Esc、双实例竞态、主题/DPI 仍待人工验证。

## 2026-09-05 R04 任务统计口径与完整历史查询

- Worker 新增 `TaskQueryDto`/`TaskPageDto`/`TaskSummaryDto` 查询契约和 `tasks.page` IPC；查询支持状态（含组合状态）、游戏、类型、关键词、创建时间半开区间，按 `created_utc DESC, task_id DESC` 加不透明游标分页，稳定覆盖同一创建时间的任务。
- `GetTaskSummaryAsync` 独立聚合全量匹配结果，`DashboardService` 不再用最近 50 条推导云端等待数；今日成功使用本地日边界转换后的 UTC 半开区间。`finished_utc,state` 索引已由查询计划测试证明用于完成数统计。
- TaskCenter 的任务总数、运行中和今日完成不再混用加载窗口；支持最近/全部历史、时间范围筛选和“加载更多”，列表底部明确已加载/总数。选择历史或时间范围后采用服务端分页，重试计数明确为当前已加载结果；原有命令、绑定、虚拟化和任务滚动保留。
- 阶段验证：Release 0 warning/0 error；Core `65/65`、Worker `251/251`、Playnite `331/388`（57 跳过）、XAML `19/19`、源码校验和 `git diff --check` 通过。未运行真实 Playnite 宿主、10,000 条任务性能实验和跨午夜人工验收。

## 2026-09-05 R03 保留清理隔离账本与启动恢复

- 新增 `retention_quarantine_batches` / `retention_quarantine_entries` SQLite 表和 `RetentionQuarantineState` 状态机；每个条目持久化批次、游戏/备份 ID、原路径、隔离路径、文件字节和最后错误，应用流程依次记录 `Planned` → `Moved` → `IndexRemoved` → `Deleted`，不确定时进入 `RecoveryRequired`。
- `WorkerInitializationService` 在常规任务恢复前调用 `RecoverPendingQuarantineAsync`。仅 `IndexRemoved` 且精确 `.pending` 文件仍在当前备份根、大小与账本一致、原路径不存在时自动删除；`Planned`/`Moved` 优先把文件恢复回原路径；原路径冲突、路径越界、大小变化和未知隔离文件均保持不动。
- Retention 预览、应用结果和维护健康报告分别展示 pending 条目数、实际隔离占用、人工恢复数、移入隔离字节和真实释放字节；`FreedBytes` 只在物理删除成功后增加。维护页沿用现有 `RetentionSimulation.Summary` 显示这些状态，因此本阶段未改 XAML。
- 新增 `RetentionQuarantineRecoveryTests` 并补充成功清理的 `Deleted` 账本断言。验证：Worker Debug/Release `247/247`、Core `65/65`、Playnite `331/388`（57 跳过），Release 构建 0 warning/0 error，`validate-source.py` 和 `git diff --check` 通过；未运行真实 Worker 崩溃时序或 Playnite 宿主，阶段提交待完成。

## 2026-09-05 R02 全局保留清理候选身份与互斥

- `RetentionSimulationPreviewDto` 新增 Worker 生成的 `PreviewId`；Worker 仅在服务端保存的预览快照中查找完整候选列表，执行开始即消费句柄，重复提交、重启后旧句柄和过期句柄都会被拒绝。旧的候选数量/体积字段保留为兼容显示字段，不再作为执行授权依据。
- 预览快照指纹包含游戏/备份 ID、规范化归档路径、索引大小、文件数、创建时间、锁定/PreRestore/健康保护状态、策略指纹，以及归档文件长度和最后写入时间；执行前先做全局校验，逐游戏取得共享 `GameOperationLock` 后再次重读并校验，状态变化的游戏只跳过并要求刷新预览，不尝试替代候选。
- 保留清理、备份元数据/锁定状态、游戏策略更新、恢复可恢复性写入和仓库索引重建统一进入现有按游戏互斥通道；归档根路径、ZIP 后缀和 reparse point/junction 边界均在移动前复核。结果新增忙碌/状态变化计数，真实释放字节继续只在隔离文件删除后累计。
- 新增/补强替换候选、同路径同大小文件替换、策略变化、重复提交和共享锁占用回归。验证：Worker `244/244`、Release 0 warning/0 error、`validate-source.py`、`git diff --check` 通过；未运行真实 Playnite/Rclone，后续按 R03 继续。

## 2026-09-05 R01 任务终态写入失败不再锁死同一游戏

- `TaskCoordinator` 将任务状态写入依赖收口为 `ITaskStatusStore`；终态持久化放在独立保护块中，失败时记录 TaskId、GameId、TaskType、原始业务终态和异常，但不阻断 `_taskTokens` 清理或同游戏信号量释放。
- `PersistAndPublishAsync` 仍先成功写入 SQLite 再发布终态事件，因此未成功落盘的 `Succeeded` 不会进入 UI 任务事件流；业务返回值保留原始成功/失败/取消结果，便于区分业务结果与持久化故障。
- 新增 `TaskCoordinatorFailureTests`，对成功、业务失败、取消三条终态路径注入最后一次写入失败，验证后续同游戏任务和其他游戏任务均能在有界时间内完成，取消令牌已清理且没有发布未落盘终态事件。
- 验证：Worker Debug 构建 0 warning/0 error；Worker `238/238` 通过；`git diff --check` 通过。未运行真实 Worker 故障注入或 Playnite 宿主，后续按 R02 继续。

## 2026-09-05 全项目完善方案（仅审阅，尚未实施）

- 用户要求审阅功能、UI、可扩展性、健壮性、稳定性与性能，并提供便于其他 AI 实施的方案；已交付 [IMPROVEMENT_ROADMAP_2026-09-05.md](IMPROVEMENT_ROADMAP_2026-09-05.md)，含 15 个任务的代码依据、步骤、依赖和验收条件，基线 `d5fd494` / 0.6.73。
- 优先候选：TaskCoordinator 终态持久化异常会跳过锁释放；全局保留清理仅比较数量/体积且未参与游戏操作锁；隔离区缺持久化恢复映射；任务“今日完成”没有日期过滤；媒体传输分页仍一次聚合最多 5000 条才显示。它们是本轮源码审阅发现，不是已实施修复，也不是全部已真机复现。
- 游戏筛选 DropDownClosed 的键盘/Automation 行为需要 STA 复现；侧栏 GridLengthAnimation 成本需要实测，不能直接当成已测性能故障。当前工作区缺规则引用的 AcrylicFork Design 目录，未声称比对 Demo 像素。
- 文档交付基线验证：Release 0 warning/0 error，Core 65/65、Worker 235/235、Playnite 331 通过/57 跳过，XAML 19/19、源码验证通过。未运行本轮真实宿主/渲染/故障注入；没有改应用代码或升级版本。

## 2026-09-04 Worker 描述缓存安装状态陈旧

- 用户实测 0.6.72 后仍确认：“全部、已匹配、有备份等都有死亡空间，但已安装没有”；这排除了只修 WPF ComboBox 写回竞态的解释。
- 真正根因在 `GameCatalogService.UpsertAndMatchAsync`：`GameMatchInput.CreateHash` 有意排除 `IsInstalled`，但旧逻辑把“匹配输入未变化”错误当成“描述无需持久化”，导致 SQLite `descriptor_json` 一直保留旧的 `IsInstalled=false`。Dashboard 的“已安装”筛选读取这个持久化描述，因此目标条目被过滤。
- 0.6.73 将 `descriptorsToPersist` 与 `pending` 匹配队列分离；描述全字段变化（安装状态、安装目录、启动动作、标签、进程名、最近游玩等）会更新 SQLite，但只有匹配输入变化或到期重试才进入 Ludusavi。这样保持已有匹配，又让安装筛选使用最新状态。
- 新增 `GameCatalogPersistenceTests.InstallStateChangePersistsWithoutInvalidatingExistingMatch`，锁定“死亡空间已匹配后 false→true”场景；`GameMatchInput` 注释明确记录“匹配输入与描述持久化是两个契约”。
- 验证：Release 0 warning/0 error；Core `65/65`、Worker `235/235`、Playnite `331/388`（57 跳过）；XAML `19/19`、源码验证、WPF 静态检查通过；0.6.73 已打包、安装并由 Playnite 日志确认 `GameSaveCenter 0.6.73.0 loaded`。本机 Playnite 仍只有 3 个样本游戏，不能代替用户目标机器复核。

## 2026-09-03 游戏选择器用户选择写回竞态补强

- 用户进一步确认目标游戏行信息显示“已安装”，但“已安装”筛选搜索不到；这排除了单纯安装状态缺失，问题仍是筛选框显示值与共享 `GamePickerViewModel.StatusFilter` 在 WPF 初始化/集合刷新期间发生抢写。
- 0.6.72 将两套选择器的筛选写回从 `SelectionChanged` 改为 `DropDownClosed`，程序化选中、绑定刷新和 `ItemsSource` 重建不再被当成用户输入；保留 OneWay 显示绑定和 `UiFilterSelection.Synchronize`。
- 新增精确回归：`IsInstalled=true` 的“死亡空间”在搜索词为“死亡空间”、状态为“已安装”时 `FilteredCount=1`，且显示状态为“已安装”。
- 本阶段验证完成：Release 0 warning/0 error；Core `65/65`、Worker `234/234`、Playnite `331/388`（57 跳过）；XAML `19/19`、源码门禁、WPF 静态审查和 Render QA 通过；0.6.72 包已安装到本机 Playnite，`extensions.log` 已记录 `GameSaveCenter 0.6.72.0 loaded`。本机 Playnite 只有 3 条样本数据，不包含用户目标游戏，不能替代目标机器复核。

## 2026-09-03 游戏选择器双向绑定残留与安装判定补强

- GSC-130 的“加载后同步”仍不能阻止 WPF 在两个选择器副本的 ItemsSource 重建期间把旧值写回共享状态；本轮将生产 Shell 与兼容 Dashboard 的状态、平台、排序 ComboBox 改为 `Mode=OneWay`，只在 `SelectionChanged` 收到实际字符串选项时写入 `GamePickerViewModel`，并移除平台的静态 `SelectedIndex="0"`。
- `PlayniteGameAdapter` 现在除 `IsInstalled` 和安装目录外，还把存在的本地 Play action/working directory 作为只读安装信号；Steam URI 等非文件路径不会被误判，安装目录枚举也使用展开后的路径。
- 版本提升到 `0.6.71`，用于强制 Playnite 替换之前复用相同程序集版本号的旧 DLL。交付前必须核对 Playnite 日志为 `0.6.71.0`，不能只看 zip 文件时间。
- 本阶段已完成 Release 编译/回归、XAML/源码/WPF 门禁、Render QA、打包和本机安装；Playnite 日志已记录 `GameSaveCenter 0.6.71.0 loaded`，安装 DLL 与打包暂存 DLL 哈希一致。当前环境的 Playnite 样本只有 3 个游戏，未包含用户截图中的“死亡空间”，因此不能伪称已经在真实目标游戏上完成宿主 UI 复核。

## 2026-09-03 游戏选择器合法过期选中值收口

- 仅移除状态/排序 ComboBox 的静态 `SelectedIndex="0"` 仍不足以覆盖 WPF 初始化顺序：生产 Shell 与隐藏兼容 Dashboard 可能各自保留一个“合法但过期”的选中项，造成界面显示“全部”而 `GamePicker.StatusFilter` 仍为“已安装”。
- `UiFilterSelection.Synchronize` 现在在 Loaded 和平台选项重建后的恢复点，以共享 ViewModel 值为准同步状态、平台和排序三个 ComboBox；用户选择仍先写入共享 ViewModel，因此不会被恢复逻辑改回默认项。`RestoreDefault` 保留给只应修复空选择的其他场景。
- 回归覆盖新增有效过期选中值修复测试，并保留“全部包含未安装但已匹配/有备份”测试。GSC-130 自动证据：Core `65/65`、Worker `234/234`、Playnite `329/386`（57 跳过）、Release 0 warning/0 error、源码/XAML/WPF 门禁和 Render QA 通过。
- 本轮重新安装了当前 Release 到本机 Playnite，安装 DLL 与隔离构建 SHA256 一致；真实宿主审计已启动 Playnite，但 UI Automation 未找到侧栏入口，只产生受控证据，不能替代第二台机器的实际筛选复核。

## 2026-09-03 设置页持久化选择状态收口

- `GameSaveCenterSettingsView.xaml` 的备份格式、压缩方式和主题模式不再设置局部静态 `SelectedIndex="0"`；三项均由持久化 `SelectedValue` 的 `Mode=TwoWay`、`UpdateSourceTrigger=PropertyChanged` 绑定驱动，避免初始化时覆盖 `GameSaveCenterSettings` 已恢复的值。
- 这三项补充了中文 ToolTip 和 `AutomationProperties.Name`；不要为了视觉默认值把静态首项加回设置型 ComboBox。动态集合重建型筛选器可按各自契约保留恢复逻辑。
- GSC-129 自动证据：Release 0 warning/0 error、Core `65/65`、Worker `234/234`、Playnite `328/385`（57 跳过）和源码校验通过；真实 Playnite 重启/主题/设置导入仍需人工复核。

## 2026-09-03 设备冲突状态与媒体筛选绑定收口

- `MaintenanceView` 的设备决策 ComboBox 和 `MediaCenterView` 的媒体类型 ComboBox 移除局部静态 `SelectedIndex="0"`；设备决策显式使用双向 `DeviceDecision` 绑定，媒体筛选继续恢复 `MediaFilterState`，避免初始化阶段把持久化状态抢回第一项。
- 设备冲突 Inspector 显示 `StagedRemoteBackupStatus`，默认提示隔离区保护，完成下载校验后显示游戏、远端设备、备份 ID 和有效期；决策备注、保存决策、下载校验、已校验恢复均补充可访问名称和安全 ToolTip。
- GSC-128 自动证据：Release 0 warning/0 error、Core `65/65`、Worker `234/234`、Playnite `327/384`（57 跳过）、源码/WPF 门禁通过，真实宿主和 Rclone/多设备仍需人工复核。

## 2026-09-03 任务中心批量安全重试

- `DashboardViewModel` 新增 `RetryAllTasksCommand` 与 `RetryAllTasksAsync`；从最近任务中选取 `CanRetryTask` 项，按游戏/任务类型去重并保留最新记录，`BackupAll`/`MediaInbox` 使用全局单例键。
- 批量操作沿用 `RetryTaskCoreAsync` 的 Backup、MediaSync、CloudUpload、MediaInbox 分流；先二次确认，单项异常收集后继续，最终刷新快照并显示汇总，不新增 Worker/IPC 消息。
- GSC-127 自动证据：Release 0 warning/0 error、Core `65/65`、Worker `234/234`、Playnite `326/383`（57 跳过），源码校验和 WPF 静态审计通过；真实 Playnite 长任务和关闭竞态仍需观察。

## 2026-09-03 Playnite 游戏菜单补充媒体同步

- `GameSaveCenterPlugin.GetGameMenuItems` 新增“同步媒体”，对 Playnite 当前选中的一个或多个游戏调用现有 `MessageTypes.SyncMedia`；先写入最新游戏描述，`UploadAfterSync` 跟随 `Settings.EnableCloudUpload`。
- 入口受 `Settings.EnableMediaSync` 保护，媒体关闭时只显示提示；没有新增 Worker/IPC 业务协议，保留既有任务、错误和通知语义。
- GSC-126 自动证据：Playnite `325/382`（57 跳过）和源码校验通过；真实 Playnite 右键菜单、多选和任务通知仍需人工观察。

## 2026-09-03 游戏级云端状态汇总媒体上传

- `SqliteStateStore.GetDashboardGameRecordsAsync` 的现有媒体统计子查询现在同时聚合已归类媒体的云端状态；Dashboard 以失败、等待重试、待上传、已上传的顺序合并存档和媒体状态。
- 这样 `GameStatusDto.CloudState` 在存档已上传但媒体上传失败/排队时仍能显示真实的游戏级风险；`Inbox`/`Ignored` 媒体不计入游戏状态，没有云端内容时保留“未启用”。没有新增 IPC 字段，也没有 N+1 查询。
- GSC-125 自动证据：Release 0 warning/0 error，Core `65/65`、Worker `234/234`、Playnite `326/383`（57 跳过），源码校验和 WPF 静态审计通过；真实 Rclone 和宿主显示仍需人工观察。

## 2026-09-02 游戏选择器状态筛选抢写

- GamePicker 的状态/排序选项是静态列表，不应在 XAML 同时设置 `SelectedIndex="0"` 与共享 ViewModel 的双向 `SelectedItem`。生产 Shell 和隐藏兼容 Dashboard 各有一套选择器，两个初始化序列会互相抢写 `GamePicker.StatusFilter`，表现为用户选“全部”后仍沿用“已安装”过滤。
- 当前两套 XAML 已移除状态、排序的强制索引；动态 `PlatformFilterOptions` 仍保留索引 0 和 Loaded 恢复，因为它会异步重建。不要把状态/排序的静态索引加回来。
- 回归覆盖：`GamePickerViewModelTests.AllFilterIncludesUninstalledGameThatHasMatchOrBackup` 和 `GamePickerShellSourceTests.GamePickerStatusAndSortSelectionsComeFromSharedViewModel`。
- 自动证据：Core `65/65`、Worker `233/233`、Playnite `325/382`（57 跳过）、Release 0 warning/0 error、源码门禁通过，WPF 静态审计 0 error/18 warning/172 info；真实宿主仍需第二台 Playnite 复核。

## 2026-09-02 跨机器 Steam 游戏目录同步与安装状态识别

- 已确认插件不直接读取 Steam 客户端，而是读取 Playnite `Database.Games`，再把目录描述同步到 Worker 的 SQLite；此前 500+ 游戏库在 Dashboard 打开时仍跳过自动目录同步，第二台机器的本地 Worker 缓存为空/过期时，游戏会永久不出现在搜索结果，除非手动刷新。
- `GameSaveCenterPlugin` 现在在 Dashboard 打开后允许大库/超大库同步目录描述；`DashboardViewModel` 仍先绘制缓存，但随后立即在后台触发同步。Worker 先持久化全部描述，再把昂贵的 Ludusavi 匹配放入已有的节流队列，保持首屏不阻塞。
- `PlayniteGameAdapter` 将存在的 `InstallDirectory` 作为 `IsInstalled` 的只读兜底，避免 Steam/Playnite 短暂错误上报未安装时被默认“已安装”筛选隐藏。若游戏根本没有进入 Playnite Steam 库，插件仍无法仅凭 Steam 客户端发现它，必须先让 Playnite 导入该游戏。
- 自动证据：Core `65/65`、Worker `233/233`、Playnite `323/380`（57 跳过），Release 构建 0 warning/0 error，源码校验通过，WPF 静态审计 0 error/18 warning/172 info；真实第二台 Playnite 宿主仍需安装新包后复核。

## 2026-09-02 媒体收件箱旧代际分页取消

- `LoadMediaInboxPagesAsync` 现在接收 `requestGeneration`，并在每页 IPC 前后与 `mediaInboxLoadGeneration` 比较；旧代际直接返回 `null`，调用方不再进入集合或 UI 回写。
- 这收口了页面卸载、模式切换和忙碌期间最新请求排队时的无效工作：当前已发出的单个 IPC 请求仍由客户端既有超时完成，但旧加载不会继续请求最多 5000 条收件箱的剩余页面。
- STAB-021 自动证据：Release 0 warning/0 error，Core `65/65`、Worker `232/232`、Playnite `321/378`（57 跳过），源码校验和 WPF 静态审计通过；真实 Playnite 快速切换/卸载仍需人工观察。

## 2026-09-01 IPC 长连接读取器的取消等待对象累积

- `BoundedIpcLineReader` 不再为每次底层读取创建会一直等待监听器令牌取消的无限 `Task.Delay`；改用一次性 `CancellationToken.Register` 唤醒竞争任务，读取完成后立即释放注册。
- 任务事件长连接的取消、JSON 行边界、4 MiB 上限和超大消息丢弃语义保持不变；这只收口长期运行时的对象/令牌回调积累。
- STAB-020 自动证据：阻塞读取取消与源码契约回归通过；Release 0 warning/0 error，Core `65/65`、Worker `232/232`、Playnite `321/378`（57 跳过），源码门禁通过。真实宿主长时间任务通知仍需人工观察。

## 2026-09-01 初始同步取消的令牌源释放竞态

- `DashboardViewModel.CancelInitialSynchronization` 不再假设字段交换后令牌源一定仍未释放；如果后台同步任务已在并发窗口内完成并释放对象，卸载取消会吞掉该次 `ObjectDisposedException`。
- 令牌源释放权仍归初始同步/缓存重试任务的 `finally`，避免主动 Dispose 与 `Task.Delay` 取消注册竞争；正常取消、代际失效和后台同步行为不变。
- STAB-019 自动证据：定向 Playnite 回归通过；Release 0 warning/0 error，Core `65/65`、Worker `230/230`、Playnite `320/377`（57 跳过），源码门禁通过。真实宿主快速打开/关闭仍需人工观察。

## 2026-09-01 原生确认框 UI 线程边界

- `GameSaveCenterPlugin.ConfirmAsync` 的原生 Playnite 对话兜底不再直接在调用线程执行；结果变量在 `TryInvokeUi` 的 Dispatcher 边界内赋值，后台游戏事件和快捷操作不会从线程池触碰宿主对话 API。
- Dispatcher 已关闭或调用失败时返回 `false`，因此危险操作保持未确认状态；Dashboard 嵌入式确认流程、按钮命令和文案不变。
- STAB-018 自动证据：定向 Playnite 源码回归通过；Release 0 warning/0 error，Core `65/65`、Worker `230/230`、Playnite `319/376`（57 跳过），源码门禁通过。真实宿主后台确认与关闭竞态仍需人工观察。

## 2026-09-01 自动修改器审计异常隔离

- `LaunchAfterDelayAsync` 的自动启动审计统一调用 `TryAppendAutoStartAuditAsync`；该 helper 会观察取消并吞掉审计存储异常，避免 detached task 在 Worker 关闭期间留下未观察异常。
- 原始启动异常在停机 token 已取消时降为 Debug；正常启动失败仍保留 Error。自动启动成功后即使审计失败，也不会影响已启动进程或让后台任务冒泡。
- STAB-017 自动证据：Release 0 warning/0 error，Core `65/65`、Worker `230/230`、Playnite `319/376`（57 跳过），源码门禁通过。真实 Worker 关闭竞态仍需人工观察。

## 2026-09-01 自动修改器审计写入退出期取消

- `GameToolService.LaunchAfterDelayAsync` 的三个审计分支（已有实例跳过、成功启动、启动失败）均使用传入的延迟任务 token，不再以 `CancellationToken.None` 回写 SQLite。
- 延迟任务已经绑定 `GameSessionCoordinator.ApplicationStopping`；Worker 停止时，延迟、启动后的审计和取消异常均不会继续形成停机期存储访问。
- STAB-016 自动证据：Release 0 warning/0 error，Core `65/65`、Worker `230/230`、Playnite `319/376`（57 跳过），源码门禁通过。真实启动/停机时序仍需人工观察。

## 2026-09-01 游戏会话自动化退出期取消

- `GameSessionCoordinator` 注入可选 `IHostApplicationLifetime`，为所有脱离 IPC 请求的会话自动化操作提供统一 `ApplicationStopping` 令牌：自动修改器、退出备份/媒体同步、游玩中定时备份/媒体同步均不再使用 `CancellationToken.None`。
- `RunSafeAsync` 只在 Worker 停止取消时记录 Debug；非取消异常仍记录 Error。这样不会让停机中的任务继续访问已开始释放的 SQLite、归档目录或外部工具。
- STAB-015 自动证据：Release 0 warning/0 error，Core `65/65`、Worker `229/229`、Playnite `319/376`（57 跳过），源码门禁通过。真实 Worker 退出竞态仍需人工观察。

## 2026-09-01 会话存档路径快照退出期取消

- `SavePathDetectionService` 注入可选 `IHostApplicationLifetime`；会话开始时的非阻塞存档路径快照使用 `ApplicationStopping`，不再永久使用 `CancellationToken.None`。
- 这样保留了“不阻塞游戏启动 IPC”的行为，同时保证 Worker 停止时扫描会取消，避免 SQLite 已释放后继续写入快照/审计；完成回调仍观察取消或失败结果。
- STAB-014 自动证据：Release 0 warning/0 error，Core `65/65`、Worker `228/228`、Playnite `319/376`（57 跳过），源码门禁通过。真实 Worker 重启取消时机仍需人工观察。

## 2026-09-01 FLiNG 下载进度写入收口

- FLiNG 下载接口使用可等待的 `Func<long, long?, Task>` 进度回调；下载循环等待回调完成，不再把任务进度写入丢到未观察的后台任务。
- GameToolService 只在百分比变化时报告 5–80 的下载进度，避免 80 KiB 分块级别的 SQLite/任务事件写入和进度倒序；下载、取消、解压和 2 GiB 安全上限未改变。
- STAB-013 自动证据：Release 0 warning/0 error，Core `65/65`、Worker `227/227`、Playnite `319/376`（57 跳过）和源码门禁通过。真实网络下载与取消时机仍需人工回归。

## 2026-09-01 公共 IPC 入口的退出期保护

- `GameSaveCenterPlugin.RequestAsync<T>` 现在检查 `lifetimeCancellation`；退出后返回 `Task.FromCanceled<T>`，统一阻止页面命令、快捷操作和异步续体在多个 await 后绕过生命周期守卫发起新 Worker 请求。
- 该保护只拦截退出后尚未创建的请求，不强行中断已经在管道中的请求；现有调用方的 `OperationCanceledException` 由页面命令的取消路径观察，不显示退出期错误通知。
- STAB-012 自动证据：Release 0 warning/0 error，Core `65/65`、Worker `226/226`、Playnite `319/376`（57 跳过），源码门禁通过。真实宿主关闭中操作仍需人工回归。

## 2026-09-01 Playnite 退出阶段后台生命周期收口

- `GameSaveCenterPlugin.OnApplicationStopped` 现在先取消 `lifetimeCancellation`，停止任务通知计时器后再停止本插件持有的 Worker；计时器已经排队的回调会在轮询入口、IPC 返回后和通知逐项处理前退出。
- `FireAndForget`、`EnsureWorkerAsync`、库回调、游戏启动/停止事件、目录同步和任务通知均拒绝退出后的新工作；同步闸门等待使用 `lifetimeCancellation.Token`，避免 Playnite 关闭期间继续触碰 Worker、Dispatcher 或 UI 通知。
- Release 构建 0 warning/0 error，Core `65/65`、Worker `226/226`、Playnite `319/376`（57 跳过）和源码门禁通过。真实宿主退出竞态、Worker 重启与 Dispatcher 关闭仍属于人工验证边界。

## 2026-09-01 异步操作返回上下文保护

- 保存媒体单项/批量元数据、存档元数据时，先捕获原游戏、条目和编辑值；Worker 返回后只有原游戏仍在媒体/存档工作区、且编辑器仍对应原条目时才更新集合、摘要或清理 dirty 标记，避免切换期间丢失新输入。
- 媒体重新归类、备份完成提示、校验/恢复准备、策略模板应用、备份比较/保留预览和进程映射输入清理也不再读取异步期间变化的当前对象；旧结果只服务于原请求。
- 本阶段未改 XAML；Release 构建 0 warning/0 error，Core `65/65`、Worker `226/226`、Playnite `318/375`（57 跳过）和源码门禁通过。真实 Playnite 快速切换仍需人工验证。

## 2026-09-01 详情编辑草稿与游戏摘要刷新保护

- `SelectedBackup`/`SelectedMedia` 重新绑定同一 ID 时，不再无条件覆盖编辑器字段。`backupCommentDirty`、`backupLockDirty`、`mediaCommentDirty`、`mediaFavoriteDirty` 分别记录用户是否改过对应值，详情刷新只同步未修改字段。
- 切换到新条目或清空选择会完整同步/清空编辑器；存档单项保存、媒体单项保存以及媒体批量修改成功后清理相应 dirty 标记，使后续服务端回读可以校准值。
- 游戏策略编辑以选中游戏的基线副本识别本地未保存 `Policy`，快照刷新时只给展示集合保留该草稿；保存请求捕获原游戏 ID、名称和策略副本，返回期间切换游戏时只更新原游戏的基线，成功提示不会串到新选中的游戏。`GamePickerViewModel.SetItems` 在当前 item 未变但 DTO 更新时补发 `SelectedGame`，Dashboard 再转发通知给壳层绑定。
- 这项保护只涉及 Playnite ViewModel 的 UI 编辑草稿，不改变 DTO、IPC 或 Worker 数据；需在真实 Playnite 中边输入边触发任务刷新/切换页面观察绑定体验。

## 2026-09-01 媒体收件箱与详情刷新一致性

- `MediaInboxMode` 切换现在用 `mediaInboxLoadGeneration` 丢弃过期可见响应；`pendingMediaInboxLoadMode` 保留忙碌期间最后一次模式，`RunAsync` 结束后自动补加载。旧响应仍可更新对应缓存，但不会把旧集合应用到当前视图。
- 媒体收件箱刷新按当前 `MediaId` 和目标游戏 ID 保留用户选择；存档版本和当前游戏媒体详情刷新按 `BackupId`/`MediaId` 保留选择，只有条目不存在时才回退第一项。媒体工作区刷新在“已忽略”模式会同时更新忽略缓存。
- 收件箱补齐共享 Worker 离线态，离线时隐藏“空列表”误导文案；健康状态下的 Demo-first 布局、命令、Binding、滚动和虚拟化不变。新增源契约回归。
- Release 构建 `0 warning/0 error`；Core `65/65`、Worker `226/226`、Playnite `315/372`（57 跳过）；源码/WPF 门禁和双主题、多尺寸、连续 Resize、Shell `render-qa OK`。真实宿主快速切换、Worker 重启和 DPI 仍需人工观察。

## 2026-09-01 修改器目录加载状态反馈

- `DashboardViewModel` 新增 `IsTrainerCatalogLoading` 与 `IsTrainerReleasesLoading`，覆盖 FLiNG 目录同步/搜索及版本查询的真实请求生命周期，并在 `finally` 中清理状态。
- `TrainerCenterView` 的目录结果和版本结果空状态现在要求对应请求已结束；请求期间使用共享 `WorkspaceStatePresenter` 展示加载信息，避免空集合短暂显示为“没有匹配”或“请选择版本”。
- Release 构建 `0 warning/0 error`；Core `65/65`、Worker `226/226`、Playnite `313/370`（57 跳过），源校验和 WPF 静态审计通过；RenderHarness 双主题、多尺寸、连续 Resize 和 Shell 紧凑标题区均 `render-qa OK`。不要把离屏结果写成新增 UI 在真实 Playnite 全矩阵中已验收。

## 2026-09-01 修改器版本读取竞态与逐项操作优化

- `LoadTrainerReleasesCommand` 接收 FLiNG 目录行的 `CommandParameter`；点击某一行的“读取版本”会先将该行设为当前目录项，再读取它的版本，而不是复用此前的选择。
- 版本请求用 `trainerReleaseLoadGeneration` 和当前 `CatalogId` 双重确认响应是否仍然有效；`pendingTrainerReleaseCatalogId` 记录忙碌期间最新选择，`RunAsync` 完成后自动补发一次，旧响应不会覆盖新选择。
- 当前验证：Release 构建 `0 warning/0 error`；Core `65/65`、Worker `226/226`、Playnite `314/371`（57 跳过），源码/WPF 门禁通过，RenderHarness 多尺寸、双主题、连续 Resize 和 Shell QA 均 `render-qa OK`。该请求队列仍需随新包在真实 Playnite 中人工观察一次。

## 2026-09-01 IPC 媒体边界与任务通知缓存优化

- `IpcRequestDispatcher` 对 `ListMedia` 的请求统一夹限到 1–1000 条；数据库仍保留自身 5000 条防线，分发层新增的 1000 条上限用于避免异常 IPC 请求产生过大的单次响应，当前 Dashboard 请求行为不变。
- `GameSaveCenterPlugin` 的任务通知去重从无界 `ConcurrentDictionary` 改为 `BoundedTaskIdSet`，默认保留最近 4096 个任务 ID，操作带锁且大小写不敏感；这只是通知去重缓存，未改变任务执行、状态持久化和通知策略。
- Release 构建 `0 warning/0 error`；Core `65/65`、Worker `226/226`、Playnite `312/369`（57 跳过），`validate-source.py` 通过，WPF 静态审计 `0 error/18 warning/172 info`。真实 Playnite、DPI、历史任务重放和用户环境命名管道响应仍需人工验收。

## 2026-09-01 FLiNG 目录解析收口

- 将在线目录和详情页解析从网络流程中抽成纯解析方法，保留目录最小数量保护；支持绝对、相对和协议相对链接，统一 HTML 解码与 URI 规范化，只接受 FLiNG HTTPS 主域/子域及预期路径。
- 目录页去除追踪查询参数并按规范 URL 去重；下载链接保留查询参数、去除 fragment，非法外站链接不会进入本地缓存或下载列表。新增 HTML 实体、相对链接、重复项和外站链接回归。
- Worker `222/222`、Core `65/65`、Playnite `310/367`（57 跳过）和 `validate-source.py` 通过。真实 FLiNG 页面、实际下载、杀毒软件拦截和 Playnite 宿主仍需人工回归。

## 2026-09-01 媒体云端重试收口

- 修复媒体本地归档成功、云端复制失败后再次同步可能不上传的问题：单游戏同步不再只看本轮 `copied > 0`，还会识别已归类且处于 `Pending`/`Failed`/`RetryScheduled` 的媒体；公共 Inbox 同样会补入这些游戏。
- 新增 `GetMediaGamesNeedingCloudUploadAsync` 及 SQLite 索引；媒体复制前回写 `Pending`，成功为 `Synced`，同时补齐媒体和游戏云端状态的用户文案。Worker 测试 `220/220` 通过。
- 该修复只保证任务重试路径不丢上传，不等同真实 Rclone 远端验证；真实网络失败、远端权限和 Playnite 任务通知仍需人工回归。

## 2026-08-31 UI/可靠性审计收口

- 本轮按用户授权完成所有不依赖真实 Playnite 宿主的修复：`BackupAll` 改为 SQLite 持久化主任务，提交立即返回，逐游戏进度通过既有任务事件/SQLite 状态反馈；Worker 启动时恢复 `Queued/Running` 整库任务，任务中心支持安全重试。真实 Worker 重启后的连续恢复仍需在隔离宿主手工验证。
- 大库匹配保留描述缓存和后台分批执行，未匹配条目按 6 小时重试；后台任务使用 Worker 生命周期取消，并且所有待匹配项都会入队，避免只处理首批后永久遗漏。媒体签名加入 4 KiB 多点采样校验、30 天/10 万条清理，样本只是快速变更探测，完整 SHA-256 仍是去重依据。
- 生产 Shell 紧凑标题区在 `<980 DIP` 使用自动高度 + `WrapPanel`，操作区按可用宽度换行；RenderHarness 新增 `shellqa` 入口，720/960/980/1040×640/700 均通过，720 截图确认“立即备份/全部备份”不再被右侧裁切。剪贴板重试改为异步等待，不阻塞 Playnite Dispatcher。
- 自动证据：Debug 解决方案构建 0 警告/0 错误；Core `59/59`、Worker `219/219`、Playnite `310/367`（57 跳过）；WPF 静态审计 `0 error/18 warning/172 info`；Shell QA `shell-qa OK`。静态审计 warning 主要是既有 StackPanel/ScrollViewer 启发式提示和参考主题 Canvas，不等同于运行时缺陷。
- 当前仍不能宣称真实 Playnite、DPI、宿主主题/高对比度、窗口连续缩放、Worker 实例重启日志和真实大库滚动已验收；环境缺少可证明隔离的 Playnite 安装，必须由用户手工完成这一阶段。

## 2026-08-26 UI-335 标题栏与页面卡片完整圆角

- 用户反馈截图中的标题栏和页面卡片仍有尖角。根因是 `GscRedesignHeaderCorner` 只设置了 `16,16,0,0`，普通 `GscRedesignSectionCard` 也没有统一裁剪内部子元素。
- 标题栏改为 18 DIP 四角圆角、完整 1 DIP 描边和裁剪；共享页面卡片启用 `ClipToBounds=True`，避免内部背景/内容把圆角视觉填回直角。命令、Binding、页面数据、滚动和虚拟化未改。
- Release 0 warning/0 error；Core `59/59`、Worker `210/210`、Playnite `310/367`（57 跳过）；源码/XAML/WPF 门禁通过，`.tmp/ui-qa-rounded-surfaces-v1/render-qa-report.txt` 为 `render-qa OK`。RenderHarness 不包含外层 Shell 标题栏，真实 Playnite 未运行，Phase 4 按用户要求跳过。

## 2026-08-26 UI-334 按钮状态层覆盖范围

- 生产 `GscWpfUiButton` 的 Hover/Pressed 状态层现在覆盖整个圆角按钮；`ButtonChrome` 保持 0 padding，内容间距改由 `ContentPresenter` 的 `TemplateBinding Padding` Margin 保留。
- 新增全按钮 `FocusOverlay`：键盘聚焦时覆盖整个按钮，并继续保留共享焦点环；Primary 按钮使用对应的 on-accent 覆盖色。命令、Binding、按钮尺寸、文字省略、滚动、虚拟化和主题资源契约未改。
- TextBox 的 Padding 回归断言收窄到 TextBox 模板范围，避免与按钮模板的内容 Margin 契约冲突。Release 0 warning/0 error；Core `59/59`、Worker `210/210`、Playnite `310/367`（57 跳过）；源码/XAML/WPF 门禁通过，`.tmp/ui-qa-button-focus-v1/render-qa-report.txt` 为 `render-qa OK`。真实 Playnite 未运行，Phase 4 按用户要求跳过。

## 2026-08-26 UI-333 生产标题栏顶部圆角

- `AcrylicProductionShellView.HeaderSurface` 现在使用共享 `GscRedesignHeaderSurface`，顶部两角使用 `GscRedesignHeaderCorner=16,16,0,0` 并启用 `ClipToBounds`；底部保留直线边界，标题栏与页面内容仍然连续。
- 本轮只改共享 Redesign 资源、生产 Shell 样式引用和源码契约测试；页面导航、标题/副标题绑定、顶部按钮命令、响应式行高和内容滚动保持不变。
- Release 构建 0 warning/0 error；Core `59/59`、Worker `210/210`、Playnite `310/367`（57 跳过）；源码/XAML/WPF 静态门禁通过，`.tmp/ui-qa-header-corner-v1/render-qa-report.txt` 为双主题、多尺寸和 Resize `render-qa OK`。RenderHarness PNG 不包含外层 Shell 标题栏，真实 Playnite 仍未运行，Phase 4 按用户要求跳过。

## 2026-08-26 UI-332 按钮组、提示气泡与环境材质边界

- 按用户当前视觉反馈，维护中心“远端恢复”按钮统一使用共享 `GscWpfUiRemoteRestoreButton`（148×36 DIP），媒体中心当前游戏媒体的两处批量操作统一使用 `GscWpfUiMediaBatchButton`（120×36 DIP）；命令、Binding、间距语义和两处页面实例保持不变。
- 修改器中心“可下载版本”提示改用 `GscDiagnosticHintBubble`/`GscDiagnosticHintText`，取消醒目的蓝色粗体文本，保留低干扰提示气泡语义。
- `AmbientMaterialLayer` 增加共享 `CornerRadius` 依赖属性和 `MaterialChrome` 裁剪容器；页面层默认 16 DIP 圆角，生产 Shell 层显式使用 0 DIP 以覆盖完整内容窗格，消除页面底部环境材质的尖锐矩形边界。
- Release 构建 0 warning/0 error；Core `59/59`、Worker `210/210`、Playnite `309/366`（57 跳过）；源码/XAML/WPF 静态门禁通过，`.tmp/ui-qa-button-material-v1/render-qa-report.txt` 为双主题、多尺寸和 Resize `render-qa OK`。本轮未运行真实 Playnite，Phase 4 仍按用户要求跳过，实机视觉/DPI/宿主日志仍需人工复核。

## 2026-08-26 STAB-007 性能测量事实

- Phase 7 只完成测量，没有性能猜测式改动：默认 Blur/动画/滚动/虚拟化保持不变；Blur 20/78/100 的半径回归为 20/78/100 DIP。
- 全量 Worker 数据规模通过：2,000 游戏、20,000 备份、10,000 任务、30,000 媒体、500 工具，约 3m03s，managed memory 增长 0 MiB、句柄 +0、线程 +0。
- 2,000 游戏合成 UI 集合测试：首次/未变化/单项变化 55/2/15ms，搜索/清空 215/196ms，任务 ReplaceAll 1/0ms；离屏 Render QA 253 样本 20–3032ms，平均 219.01ms，双主题和 Resize 通过。
- 真实 Playnite 首屏、切页、侧栏、主题/背景内存、DPI 和大库滚动仍未验证；Phase 4 是用户明确跳过的宿主阶段，不能把这些数字写成实机结论。

## 2026-08-26 STAB-006 页面树与响应式协调事实

- 生产页面路径现在由 `AcrylicProductionShellView.PageHost` 和 `GetWorkspaceView<T>` 统一承载；Dashboard 中的旧页面树仍存在，但只作为兼容资源/审计面，`GetLegacyCompatibilityWorkspaceViews()` 是显式边界。不要删除旧树，除非后续取得 Playnite 初始化、资源查找和外部引用的真实证明。
- `ResponsiveLayoutCoordinator.Calculate(width, height)` 是宽高状态的唯一数值来源，保留原有断点和尺寸。Dashboard 外壳与生产 Shell 共用状态，Shell 的导航、侧栏切换和延迟 Resize 都通过 `ApplyResponsiveLayout`，回调在 `IsLoaded` 之后才执行。
- 结构测试覆盖双树职责、生产注册表和宽高临界值；Release Core `59/59`、Worker `210/210`、Playnite `309/366`（57 跳过），WPF `0 error/18 warning/172 info`，Render QA 双主题/多尺寸/Resize `render-qa OK`。
- Phase 4 按用户明确指示跳过。离屏渲染、静态审计和测试不等于真实 Playnite；实机视觉、DPI、宿主日志、Worker 重启、性能和 `GscTableViewportHeight` 外部引用仍需人工/环境证据。

## 2026-08-26 STAB-005 媒体 Inbox IPC 分页事实

- 超限接口已确认是 `media.inbox.list` / `ListUnassignedMedia`，不是 Dashboard 快照：旧 Playnite 请求 `Limit=5000` 会把 4615 条 Inbox 媒体拼成超过 4 MiB 的响应，随后 UI 只看到通用“操作失败”。
- `GameQueryDto.Offset` 是兼容性增量字段；Worker 强制将未分配/已忽略媒体单页限制为 500，并使用稳定时间+ID排序和 SQL `OFFSET`。Playnite 连续请求 offset `0..4999`，短页或空页停止，最终最多应用 5000 条，保留原集合、选择、Binding 和虚拟化路径。
- `NamedPipeServerService` 超限日志必须包含请求 ID、消息类型、完整响应字节数和 payload 字节数；4 MiB 限制与 `MESSAGE_TOO_LARGE` 语义不能放宽。该诊断也覆盖未来其他超限接口。
- 自动验证：Core `59/59`、Worker `210/210`、Playnite `302/359`（57 跳过），Release `0 warning/0 error`，RenderHarness `render-qa OK`，WPF `0 error/18 warning/172 info`。现有用户 Worker 的只读 500 条请求为 `745990` 字节；补丁 Worker 未在用户实例中替换运行。
- 本次按用户要求跳过 Phase 4，不得把离屏 RenderHarness或旧 Worker 只读探针写成真实宿主验收；发布/安装新包后应复查日志中 `RequestId=... Type=media.inbox.list ResponseBytes=...`，并确认 Inbox/Ignored 页面无超限错误。

## 2026-08-26 STAB-003A 真实 Named Pipe 烟测事实

- 当前 Release Worker 在隔离 `.tmp/phase3-ipc-runtime-escalated/data` 中真实启动成功；超限请求 `4194778` UTF-8 字节得到 `MESSAGE_TOO_LARGE`，同一连接后续 `system.ping` 成功。
- 受限上下文的 Named Pipe `Access is denied` 已与 Worker 启动问题区分：最小同用户探针同样失败，提升权限后真实 Worker 烟测通过。报告位于 `.tmp/phase3-ipc-runtime-escalated/runtime-smoke-report.txt`。
- 烟测结束已停止临时进程，未触碰 Playnite 用户数据；真实宿主、多客户端、重启和日志矩阵仍属于 Phase 4/人工验收范围。

## 2026-08-26 STAB-004 真实 Playnite 环境阻塞事实

- Phase 4 真实宿主矩阵尚未执行，状态必须保持 `BLOCKED_ENVIRONMENT`：当前机没有 `Playnite.DesktopApp.exe`、运行中的 Playnite 进程、App Paths 或卸载注册表入口；历史候选 `D:\software\Playnite\Playnite\Playnite.DesktopApp.exe` 也不存在。
- 不要运行 `scripts/real-host-audit.ps1` 或 `scripts/dev-install-run.ps1` 作为替代。它们的安装/启动路径可能写入用户扩展目录、关闭当前宿主或复用全局单实例；在没有隔离安装、独立数据根、唯一 PID 和扩展路径证明前，不能触碰用户实例。
- 必须验证的真实条件仍包括四种窗口尺寸、100%/125%/150% DPI、Light/Dark/Follow/High Contrast、七个 Dashboard 页面与 Settings、侧栏/Tab/表格/搜索/键盘、Dashboard 重载、游戏启动事件、Worker 启停/握手/日志和旧 DLL 复用。
- Phase 0–3 自动验证已完成，但不能解除 Phase 4 阻塞，也不能作为 Phase 5 双页面树治理的“前置全部通过”证据。解除条件是用户提供可审计的隔离 Playnite 环境。

## 2026-08-26 STAB-003 IPC 边界事实

- `BoundedIpcLineReader` 是共享 Contracts 类型，必须按连接实例化；它有 4 KiB 内部字符缓冲并保存块内偏移，不能改回静态一次性块读取，否则遇到同块多条消息会丢掉下一条。超限行只保留上限内内容并继续消费到换行。
- 请求、响应和事件都使用 `ProtocolConstants.MaximumMessageBytes`（4 MiB）；Worker 服务端对请求/响应、事件服务端对事件写出、Playnite 客户端对请求/响应/事件读取均有边界。超限稳定码为 `MESSAGE_TOO_LARGE`，事件端收到后忽略并继续重连/监听。
- `NamedPipeServerService` 的业务连接槽位为 32，`TaskEventPipeServerService` 为 8；这是过载保护，不改变单请求、任务事件广播或 SQLite 回退路径。管道名、版本、消息类型和 `PipeOptions.CurrentUserOnly` 不变。
- Playnite 请求响应读取取消后继续抛出原 `TimeoutException("Worker response timed out.")`；事件监听 token 取消仍走外层 `OperationCanceledException` catch；服务端仍分别处理 JSON 错误、IO 断开和停止取消。
- STAB-003 验证：IPC 定向 `3/3`，隔离 Release Core `59/59`、Worker `201/201`、Playnite `302/359`（57 跳过），构建 `0 warning/0 error`，WPF `0 error/18 warning/172 info`，`.tmp/phase3-ipc-boundary-render-final/render-qa-report.txt` 为 `render-qa OK`。
- 本阶段没有 UI 改动；真实宿主的多连接/重启/权限和 Worker 日志仍需人工验证。

## 2026-08-26 STAB-002 外部进程执行事实

- `ProcessExecutionLimits.MaximumOutputBytes` 集中定义为每个 stdout/stderr `4 * 1024 * 1024`；`ExternalProcessRunner` 必须使用有界读取，不能恢复 `ReadToEndAsync` 或把超限内容继续追加到内存。
- `ProcessResult.ErrorCode` 是稳定的低层错误码：超限为 `PROCESS_OUTPUT_LIMIT_EXCEEDED`，超时为 `PROCESS_TIMED_OUT`，可执行文件缺失为 `EXECUTABLE_NOT_FOUND`。正常输出、非零退出码、取消异常和既有标准错误文本都要继续保留。
- 超限读取器达到上限后仍消费管道但丢弃后续内容，以避免子进程阻塞；只记录流是否受限和上限，不记录被截断文本。不要把本地化错误文本当作机器判断条件。
- `RcloneClient.RunSafeAsync` 的签名不再含 `workingDirectory`；它向 Runner 显式传 `null` standardInput，Runner 继续将可执行文件所在目录作为实际 WorkingDirectory。不要为修复命名而改变 Rclone 参数、allowlist 或工作目录。
- STAB-002 验证：定向 Worker `6/6`，隔离 Release Core `59/59`、Worker `201/201`、Playnite `302/359`（57 跳过），构建 `0 warning/0 error`，WPF `0 error/18 warning/172 info`，`.tmp/phase2-process-stability-render/render-qa-report.txt` 为 `render-qa OK`。
- 本阶段未修改 UI；真实 Worker/Rclone/Ludusavi 实机行为和 Playnite 宿主日志仍需人工验证。

## 2026-08-26 STAB-001 Dashboard 生命周期订阅事实

- `DashboardViewModel` 不再在构造函数永久订阅 `PlayniteGameStarted`；`PlayniteGameStartedSubscription` 以幂等 `Start/Stop` 保存唯一 handler，`DashboardView.OnLoaded`/`OnUnloaded` 分别调用启动/停止。
- `OnPlayniteGameStarted` 在进入 `ApplyOnUi` 前后都检查 `IsSubscribed`，因此卸载后已经排队的回调不会继续写入 Dashboard；`pendingAutoSelectPlayniteId`、`TryApplyPendingAutoSelection` 和“游戏尚未到达时保留 pending”语义不变。
- 生命周期回归事实覆盖首次加载、卸载、重载、重复生命周期和 pending selection；源码门禁现在要求上述 View 生命周期契约，不能恢复为构造函数直接订阅。
- STAB-001 验证：定向 `6/6`，隔离 Release Core `59/59`、Worker `201/201`、Playnite `302/359`（57 跳过），构建 `0 warning/0 error`，WPF `0 error/18 warning/172 info`，`.tmp/phase1-dashboard-lifecycle-render/render-qa-report.txt` 为 `render-qa OK`。
- 本阶段没有修改 XAML；真实 Playnite 仍需重启扩展后确认事件订阅、DPI、主题、焦点和重载行为。

## 2026-08-26 STAB-000 当前稳定性修复基线

- 当前基线提交为 `28bccfe4ad7f55a9ea95083d5a686d7d2837e96b`，`main` 与 `origin/main` 同步，工作树干净。
- Phase 0 已完成只读基线：源码校验通过、XAML 19/19、WPF 0 error/18 warning/172 info、Release Core 59/59、Worker 201/201、Playnite 297/354（57 跳过），RenderHarness `render-qa OK`，2000 游戏合成耗时已记录在 WORKLOG。
- 生产壳通过 `AcrylicProductionShellView.PageHost` 动态承载 Overview、Save、Trainer、Media、Task、Maintenance；Settings 是独立 `GameSaveCenterSettingsView`。`DashboardView.xaml` 旧/兼容页面树仍保留，禁止在稳定性阶段删除。
- Phase 1 的首个已确认风险是 Dashboard 生命周期：`DashboardViewModel` 构造函数订阅 `PlayniteGameStarted`，而 View 的 Loaded/Unloaded 没有对应的显式启动/停止 API。后续应增加幂等 Start/Stop，由 `DashboardView.OnLoaded`/`OnUnloaded` 驱动，并锁定卸载后不调度 UI、重新加载可恢复、pending selection 不变。
- Phase 2/3 的首个边界风险也已确认：`ExternalProcessRunner.ReadToEndAsync` 无输出上限；Named Pipe 服务端/客户端读取整行后才检查大小；`RcloneClient.RunSafeAsync` 的 workingDirectory/standardInput 参数语义错位。正常输出、退出码、超时、取消、JSON 错误和协议契约必须保持不变。
- 本轮未重新启动真实 Playnite；RenderHarness 和静态审计均不等于宿主视觉、DPI、键盘焦点或生命周期验收。真实验证继续标记为 `MANUAL QA REQUIRED`。

## 2026-08-25 UI-331 当前事实：共享控件、侧栏动画与输入校验性能

- 历史实现曾通过外层 `Chrome.Padding={TemplateBinding Padding}` 应用输入框内边距；当前有效实现由原生 `PART_ContentHost` 消费 TextBox Padding，外层 Chrome 保持无 Padding。ContentHost 必须继续保持 `Margin=0`、`Padding=0`，不要恢复外层重复 Padding 或把 Padding 绑定到 ContentHost Margin 的旧写法。
- ComboBox 选中值和 `ComboBoxItem` 都显式继承 `FontFamily`、`FontSize`、`FontWeight`；共享 ComboBox 固定 `HorizontalContentAlignment=Left`、像素对齐和布局取整。不要在单个页面给相邻筛选框另设字体或基线补丁。
- `AcrylicProductionShellView` 的侧栏仍由 `GridLengthAnimation` 驱动 270↔72 DIP、210ms、CubicEase EaseOut；内容层额外使用 190ms、4 DIP 的淡入/位移过渡。切换完成、非动画布局恢复和 `OnUnloaded` 都要清理 `BeginAnimation`，避免重载后残留透明/偏移。
- Shell 的游戏背景仍是唯一静态图片层，`CacheMode=BitmapCache` 只用于该层；禁止把缓存或 BlurEffect 扩散到卡片、文字、表格、列表或滚动器，以免内存/虚拟化成本反弹。
- `GameSaveCenterSettingsView.OnSettingsFieldChanged` 通过 `DispatcherPriority.Background` 合并校验通知；`VerifySettings` 含文件存在性检查，不能改回每个字符同步执行，否则长路径输入会卡顿。
- UI-331 验证：WPF 0 error/18 warning/172 info、Playnite 297/354（57 跳过）、Release 0 warning/0 error、`.tmp/ui-qa-polish-v1/render-qa-report.txt` 为 `render-qa OK`。真实 Playnite 帧率、DPI、键盘焦点与动画仍需人工复核。

## 2026-08-25 UI-330 当前事实：毛玻璃强度直接比例映射

- `AdaptiveThemePalette.BlurRadiusForStrength` 将设置滑块 20–100 直接映射为 Blur 半径 20–100 DIP：默认 `GlassEffectStrength=78` 就是 78 DIP，100 才是完整 100 DIP。不要恢复此前 12–34 或 16–34 DIP 的压缩范围。
- 主界面 `GscGameBackgroundEffect` 只挂在 Shell 的静态游戏背景图片层；设置页 `GscSettingsAmbientEffect` 只挂在设置页环境层。卡片、文字、DataGrid、ListBox 和 ScrollViewer 禁止使用 BlurEffect。
- `EnableGlassEffects=false`、`SystemParameters.HighContrast`、无游戏背景/关闭跟随后，仍必须返回真实 `null` 或透明/不透明回退资源；强度调整不能重新触发背景解码。
- UI-330 验证：WPF 资源定向 119/158（39 跳过）、Playnite 全量 297/354（57 跳过）、Release 0 warning/0 error、`.tmp/ui-qa-glass-strength-v1/render-qa-report.txt` 为 `render-qa OK`。真实 Playnite 的 100% 帧率、DPI 和宿主视觉仍需人工复核。

## 2026-08-25 UI-329 当前事实：刷新与背景取色性能

- `DashboardViewModel.RefreshDashboardAsync` 只有在 `SelectedGame.PlayniteId` 变化时才允许刷新当前游戏 Icon/Background；普通自动快照轮询不能重复读取 Playnite 图标或重启同一背景解码。`DashboardView.OnLoaded` 的 `EnsureSelectedGameBackgroundLoaded` 只负责恢复卸载期间被取消且当前仍缺失的加载。
- `PlayniteGameBackgroundProvider.CreateAmbientBrush` 已将整帧 `CopyPixels` 改为五次 1×1 `BitmapSource.CopyPixels`，因为材质只需要五个采样点。不要为了“方便取色”恢复多 MB 的全图临时 `byte[]`，也不要改变已验证的五个采样坐标。
- `AcrylicProductionShellView.OnShellSizeChanged` 使用 `DispatcherPriority.Render` 合并连续窗口尺寸事件；`QueueGamePickerFilterDefaults` 使用单个 `Loaded` 调度和 pending 标记。待处理标记必须在卸载和 Dispatcher 关闭路径可安全复位，避免窗口重开后丢布局或重复恢复筛选。
- 这些优化不改变页面资源、Binding、Command、导航、列表虚拟化和侧栏动画；如果后续需要进一步提速，优先复用快照/材质缓存并保持 UI 线程只做轻量状态更新，不要增加新的轮询器或每帧动画计时器。
- UI-329 验证：定向 26/26、Playnite 297/354（57 跳过）、Release 0 warning/0 error、WPF 0 error/18 warning/172 info、`.tmp/ui-qa-performance-v1/render-qa-report.txt` 为 `render-qa OK`；真实 Playnite 帧率/内存/DPI 仍需人工复核。

## 2026-08-25 UI-328 当前事实：游戏背景跟随开关与材质回退

- `GameSaveCenterSettings.FollowSelectedGameBackground` 默认 `true`，位于设置页“外观与动态效果”中的“跟随当前游戏背景”开关；旧 JSON/便携设置没有该字段时必须继续默认跟随。
- `DashboardViewModel.ApplySelectedGameBackgroundPreference()` 只在偏好状态发生变化时刷新；关闭时取消 `PlayniteGameBackgroundProvider` 当前任务并清空 `SelectedGameBackground`、采样 Brush 和材质标记，开启时按当前选择重新加载。不要把它改成每次调节毛玻璃强度都解码图片。
- `DashboardView.ApplySelectedGameGlassResources` 和 `AdaptiveThemePaletteFactory.ApplyGameBackgroundGlassResources` 必须共同检查开关、采样材质、毛玻璃和高对比度；关闭/无图时 `GscGameBackgroundOpacity=0`、tint 透明、BlurEffect 为 null，并恢复 Demo 中性表面资源。
- `AmbientMaterialLayer` 的 `ThemeAmbientWash` 只能在实际使用游戏材质时隐藏：判断应使用 `UseSelectedGameBackground && hasGameMaterial`，不能只看 `hasGameMaterial`，否则关闭游戏背景后会丢失主题环境光。
- UI-328 验证：源码/XAML 门禁、WPF 0 error、Release 0 warning/0 error、Playnite 296/353（57 跳过）和 `.tmp/ui-qa-game-background-v1/render-qa-report.txt`（`render-qa OK`）通过；真实 Playnite 的开关即时效果、DPI、Follow/高对比度和性能仍需人工复核。

## 2026-08-25 UI-327 当前事实：文字渲染、修改器对齐与诊断气泡

- 生产壳、Dashboard、Settings、共享 `DataGrid` 和开发探针现在统一使用 `TextFormattingMode=Ideal`、`TextRenderingMode=ClearType`、`TextHintingMode=Fixed`，并保留像素对齐；不要为修复锯齿把 BlurEffect 加到文字或滚动内容。
- `TrainerCenterView` 的“确认导入”表单现在用 Grid 将“主程序”标签与 `TrainerImportEntryComboBox` 放在同一行，按钮和辅助说明在下一行；真实 `ImportEntryCandidates`、`SelectedImportEntryCandidate` 和确认/取消命令未改变。
- `Redesign.xaml` 的 `GscDiagnosticHintBubble` / `GscDiagnosticHintText` 是维护中心摘要提示的共享样式：低饱和信息底、细信息描边、常规次级文字。严重程度 pill 仍保留语义色，不能用诊断气泡样式覆盖 Warning/Error/Critical 标识。
- UI-327 已通过 Release 构建、Core 59、Worker 199、Playnite 295/57、源码/XAML/WPF 门禁和 `.tmp/ui-qa-font-bubbles-v1` 多主题多尺寸 render QA；真实 Playnite 宿主字体清晰度、DPI 和 Follow 仍需人工回归。

## 2026-08-25 UI-326 当前事实：设置宿主尺寸与侧栏边界折叠

- `GameSaveCenterSettingsView` 根 `UserControl` 不再设置 `MinWidth/MinHeight`；这些属性会阻止页面缩到紧凑断点。真实宿主第一次加载时由 `EnsureHostWindowSize` 将 Window 设为工作区允许范围内的约 1280×840、`SizeToContent=Manual`、水平/垂直 Stretch；RenderHarness 无 owner Window，不会被改尺寸。
- 设置页后续缩放仍走 `ApplyResponsiveLayout`，所以宿主可在首次打开后缩小，分类栏会堆叠到表单上方，表单内容在 `SettingsScroller` 内滚动。不要把 UserControl 的最小尺寸重新加回去。
- 生产侧栏的 `SidebarCollapseArea` 是 `SidebarLayout` 的底部 Grid 行，控制器使用 `AcrylicSidebarBoundaryButton`，只显示 `‹`/`›`，没有文字或独立书签形状。展开宽度为 270 DIP，折叠宽度为 72 DIP，按钮固定 32×32、16 圆角；静止透明、悬停/按下只叠加低 alpha tint。
- 侧栏宽度必须通过 `Controls/GridLengthAnimation.cs` 的 210ms `GridLengthAnimation` 动画变化，不能回到瞬时 `GridLength` 赋值或 Canvas 绝对定位。动画使用 `CubicEase.EaseOut`，内容列由 Grid 自动同步重排。
- `GameSaveCenterSettings.SidebarCollapsed` 是持久化 UI 偏好；`DashboardView` 通过 `SidebarCollapsedProvider/SidebarCollapsedChanged` 注入生产壳，切换后立即保存，启动时恢复。不要把插件实例直接耦合到 `AcrylicProductionShellView`。
- UI-326 已通过 Debug 隔离构建、Core 59、Worker 199、Playnite 295/57、源码/XAML/WPF 门禁和 `.tmp/ui-qa-sidebar-boundary-v1` render QA；真实 Playnite 宿主仍需人工确认初始 Window 尺寸、实际动画、DPI 和键盘焦点。

## 2026-08-25 UI-319 当前事实：Dune 浅色 FollowPlaynite 判断

- 当前用户 Playnite 配置 `config.json` 的 Desktop 主题是 Dune；Dune 用资源 `ThemeDarkStyle=False` 表示浅色，同时保留 `WindowBackgroundBrush` 和 `DarkWindowBackgroundBrush` 两套颜色。
- `AdaptiveThemePaletteFactory.Create` 在 FollowPlaynite 下必须优先读取 `ThemeDarkStyle`：False 使用浅色窗口资源/浅色回退，True 使用 `DarkWindowBackgroundBrush`/深色回退。不能只读取通用 `WindowBackgourndBrush`，因为 Playnite 默认主题的历史拼写可能作为遗留资源继续可见。
- 没有 `ThemeDarkStyle` 的第三方主题才走背景资源 + `TextBrush`/`TextBrushDark` 的一致性推断；若二者明暗冲突，应优先选择文字资源推断，避免设置独立窗口出现黑底黑字。
- UI-319 已通过定向 2/2、Playnite 290/57/0、Release 0 警告/0 错误、源码校验和 19 个 XAML 结构检查。真实宿主重载后的设置窗口 Follow 浅色像素仍需人工确认。

## 2026-08-25 UI-318 当前事实：Follow Playnite 资源解析

- Playnite Desktop 默认主题的窗口背景资源键是历史拼写 `WindowBackgourndBrush`，由 `MainWindowStyle`/`StandardWindowStyle` 使用；Follow 解析必须保留该键，同时兼容 `WindowBackgroundBrush` 等第三方主题键。
- 设置页可能被 Playnite 放在独立设置窗口中，不能只依赖插件 UserControl 的视觉树背景。`AdaptiveThemePalette` 现在还显式检查 owner Window 与 `Application.Current` 资源；背景仍不可用时用宿主 `TextBrush`/`TextBrushDark` 推断浅深。
- `GameSaveCenterSettingsView.OnThemeModeChanged` 先把 ComboBox 的 enum 写回 `CurrentSettings.ThemeMode`，再排队 `ApplyAdaptiveTheme`，确保从强制深色切换 Follow 不会继续使用旧值。
- UI-318 验证：真实资源键回归测试、Release 全量构建/测试和 v8 多主题多尺寸 render QA 均通过。离屏 RenderHarness 的默认 Follow 没有 Playnite 宿主资源，仍会按中性深色回退；真实宿主需安装后复核。

## 2026-08-25 UI-317 当前事实：壳体圆角玻璃与 Follow Playnite 浅色主题

- 生产 `AcrylicProductionShellView` 的导航由 `SidebarSurface` 真实 Border 承载，使用 `GscRedesignSidebarSurface` 与动态 `GscSidebarMaterialBrush`；不要再让内部 Grid 直接承担整块导航背景，否则右侧上下角不会被圆角裁切。
- 生产页脚由 `FooterSurface` 提供独立四边圆角玻璃面；文字、状态 Binding 和底部布局不变。共享导航样式也已改为同一动态 sidebar 材质。
- 设置页继续不使用游戏图片，但 `SettingsAmbientLayer` 负责固定环境渐变与唯一 BlurEffect，外壳、分类栏、卡片和内容层使用较低 alpha 的主题材质，使环境模糊可见。不要把 BlurEffect 加到文字、表格、列表或滚动面。
- `AdaptiveThemePaletteFactory.Create` 在 `FollowPlaynite` 下必须优先检查宿主发布的 Window/Main/Control/Background 资源，再检查视觉树背景；插件控件自身的深色 `GscBackdropBrush` 只能作为回退，不能遮蔽 Playnite 浅色主题。
- `RenderHarness` 的设置页 Light/Dark 渲染必须调用 `GameSaveCenterSettingsView.ApplyThemeForAudit(mode)`；否则会误测静态深色 DesignTokens，而不是运行时主题链路。
- UI-317 验证：源码/XAML 门禁、Release 全量构建测试、WPF 静态审查和多主题多尺寸 `render-qa OK` 均通过。真实 Playnite 宿主、DPI、Follow、高对比度和关闭玻璃回退仍需安装后人工复核。

## 2026-08-25 UI-314 当前事实：游戏背景增加真实模糊

- UI-313 解决了背景图重复绘制和横向矩形接缝，但原链路只有低透明度图片、主题 tint 和采样渐变，没有任何 `BlurEffect`，因此用户看到的仍接近清晰原图。
- `AcrylicProductionShellView` 的单一游戏背景矩形现在仅在 `HasSelectedGameBackgroundAmbientMaterial=True` 时挂 `GscGameBackgroundEffect`；卡片、文字、页面、列表和滚动内容不挂 BlurEffect。
- `AdaptiveThemePalette.ApplyMaterialResources` 按 `GlassEffectStrength` 生成冻结的 `BlurEffect`；UI-330 已改为直接比例半径 20–100 DIP，默认 78% 为 78 DIP，使用 `RenderingBias.Performance`。关闭毛玻璃、无背景图或高对比度时资源为真正的 null，不保留无效效果视觉。
- UI-314 验证：源码门禁、WPF 静态检查（0 error、18 条既有 warning）、Release 全量构建/测试和多主题多尺寸 `render-qa OK`。真实 Playnite 没有可控窗口，安装后需确认实际模糊观感与性能。

## 2026-08-24 UI-313 当前事实：背景图单层居中与底部接缝修复

- 截图中的横向矩形不是游戏原图被拼接，而是 Shell 和各页面内部的 `AmbientMaterialLayer` 同时绘制同一份游戏采样渐变；不同控件尺寸让相对坐标不同，形成可见接缝。
- `AmbientMaterialLayer.UseSelectedGameBackground` 默认关闭，只有生产 Shell 的 `ShellAmbientMaterialLayer` 开启游戏采样环境色；页面局部层在有游戏背景时仅保持透明，不再重复绘制图片材质或固定绿色洗色。
- `AcrylicProductionShellView` 现在用一个跨越 Shell 两行/两列的 `ImageBrush` 绘制真实背景，`UniformToFill`、`AlignmentX/Y=Center`、`TileMode=None` 明确保持比例、对称裁剪并禁止平铺；主题 tint 也覆盖同一完整区域，页脚不再切换到另一块背景。
- UI-313 验证：源码门禁、WPF 静态检查（0 error、18 条既有 warning）、Release 构建与测试通过；Core 59/59、Worker 199/199、Playnite 289/346（57 跳过）；多主题多尺寸 `render-qa OK`。真实 Playnite 没有可控窗口，仍需安装后用带不同宽高比背景的游戏人工确认裁剪中心。

## 2026-08-24 UI-312 当前事实：卡片表面与游戏背景自适应

- `OverviewTodayHeroCard` 不再嵌套整面背景 Border；它只使用共享 `GscRedesignSectionCard`，因此卡片本身是一层连续的阅读/玻璃表面，不能恢复“卡片里面再放一张全尺寸背景”的结构。
- `PlayniteGameBackgroundProvider.LoadVisualAsync` 返回真实 `ImageSource` 和从同一张位图采样出的冻结 `AmbientBrush`；采样只生成低 alpha 线性渐变，不替换真实图片。缓存仍为最多 6 张、后台最多 1920 宽度解码、远程 5 秒/12 MB 限制、取消和 generation 防串图。
- `AmbientMaterialLayer` 在 `DashboardViewModel.HasSelectedGameBackgroundAmbientMaterial` 为 true 时隐藏固定 `GscAmbientWideWashBrush`，显示 `SelectedGameBackgroundAmbientBrush`；没有背景图时才使用主题 accent/info/success 宽域洗色。这样游戏背景颜色跟随图片，不再固定 success 绿色。
- 当前背景图层透明度为深色/浅色 0.48/0.40，主题 tint alpha 为 0x52/0x66；关闭毛玻璃或高对比度仍让图片层和环境材质透明。不要把图片层改成内容卡片或对整页、表格、列表加 BlurEffect。
- UI-312 验证：源码门禁、WPF 静态检查和 render QA 通过；背景提供器定向测试 5/5；Release 全量测试为 Core 59/59、Worker 199/199、Playnite 289/346（57 跳过、0 失败）。当前 Playnite 无可控窗口，真实宿主背景切换仍需安装后人工切换两款有不同背景图的游戏确认。

## 2026-08-24 UI-311 当前事实：背景切换、Today 圆角与材质强度

- `OverviewTodayHeroCard` 内的宽域洗色必须使用带 `CornerRadius="12"` 的 Border；不要把整面 Rectangle 直接放进圆角卡片并依赖 `ClipToBounds`，WPF 不会按 CornerRadius 裁切子元素。
- `PlayniteGameBackgroundProvider` 优先解析 `Game.BackgroundImage` 的本地路径/数据库文件；如果 Playnite 返回 HTTP/HTTPS 直链，只为当前选中游戏异步下载，5 秒超时、12 MB 上限、取消令牌和后台 1920 宽度解码仍必须保留。快速切换由 generation 和 6 张缓存保护。
- `GscGameBackgroundOpacity` 当前深色/浅色为 0.36/0.28，背景 tint alpha 当前为 0x98/0xA8；`GscAmbientWideWashBrush` 的受限 alpha 也略有提升。若继续调强，优先保持大范围均匀和文字对比度，不要恢复圆形光斑或大面积 BlurEffect。
- UI-311 已完成源码/XAML/WPF 门禁、Release 全量构建测试和多主题多尺寸 `render-qa OK`；Playnite 测试为 288/345（57 跳过），真实 Playnite 背景切换仍需用户安装新包后切换两款有不同背景图的游戏确认。

## 2026-08-24 UI-310 当前游戏背景图环境材质

- `DashboardViewModel.SelectedGameBackground` 由 UI-only `PlayniteGameBackgroundProvider` 从 Playnite `Game.BackgroundImage` 读取；优先解析本地缓存/数据库文件，UI-311 已补齐受限的 HTTP/HTTPS 异步下载。
- 背景图在后台线程按最多 1920 宽度解码，缓存最多 6 张；远程请求仅限当前选中游戏并带 5 秒超时、12 MB 上限，选中游戏切换时使用取消令牌和 generation 丢弃旧结果。不要在 Playnite UI 线程同步解码或无限制下载。
- `AcrylicProductionShellView` 在 Shell 底层绘制低透明度背景图和主题 tint；无图、关闭毛玻璃、高对比度时图片层透明，继续使用 `GscBackdropBrush` 和 `GscAmbientWideWashBrush`。
- 默认背景与主题挂钩：`AdaptiveThemePalette` 依据 Playnite 主题/固定浅深色模式生成默认背景、tint 和宽域材质。背景图只作为环境素材，不覆盖卡片/导航的功能层级。
- UI-310 已完成源码/XAML/WPF 门禁、Release 构建/全量测试和多主题多尺寸 `render-qa OK`；当前用户 Playnite 未被强制重启，真实宿主背景图显示仍需用户在更新后复核。

## 2026-08-24 UI-309 当前事实：全局宽域多色玻璃材质

- `AmbientMaterialLayer.xaml`、Overview Today 卡片、Settings 环境层和兼容 `DashboardView.xaml` 都使用 `GscAmbientWideWashBrush` 的整面 `Rectangle`；生产 UI 不再使用装饰性 `RadialGradientBrush`、椭圆光源或 `GscAmbientBlurEffect`。
- `GscAmbientWideWashBrush` 是按当前主题动态生成的六段对角线性渐变，颜色从 accent/info 过渡到 teal/success 和中性表面，覆盖大范围但保持低饱和、低透明度；`GlassStrength` 只提升受限 alpha。
- `GscAmbientAccentBrush`、`GscAmbientInfoBrush`、`GscAmbientSuccessBrush` 及旧的环境阴影色 token 已删除。状态圆点、图标填充和语义状态色不属于装饰光斑，继续保留。
- 关闭毛玻璃或高对比度时必须返回透明渐变；不要把真实 `BlurEffect` 提升到导航栏、页面根、表格、列表或滚动面。此处是嵌入式 WPF 下的低成本整面渐变模拟，不是宿主桌面像素级 backdrop blur。
- UI-309 验证：源码/XAML/WPF 静态门禁通过，Release 0 warning/0 error，Core 59/59、Worker 199/199、Playnite 283/283（57 跳过），多主题多尺寸 `render-qa OK`；已打包并核对当前用户 Playnite 扩展 `0.6.70.0`。本轮 Computer Use 仅返回 `EmptyWindowAutomationPeer`，未宣称真实宿主页面点击/截图已复核。

## 2026-08-24 UI-308 当前事实：导航栏连续材质与宽域环境洗色

- 生产 Shell 的 `SidebarLayout` 现在统一承载 `GscSidebarMaterialBrush`；标题区和导航内容区使用透明背景，保证整个 236 DIP 导航栏只绘制一层连续对角材质。
- `GscSidebarMaterialBrush` 是动态的低成本半透明线性渐变，不能改回右侧透明渐隐、硬分割线或单独的 `SidebarSeamMaterial`；边界保持正常表面，避免再次出现亮柱。
- `AmbientMaterialLayer` 的第一层是 `GscAmbientWideWashBrush` 线性渐变矩形，负责大范围非圆形洗色；本条记录的旧版本曾保留固定椭圆，当前以 UI-309 为准。
- 宽域洗色运行时按 accent/info/success 和 `GlassStrength` 生成；关闭毛玻璃或高对比度时必须返回透明渐变，不能留下大面积遮罩。
- UI-308 验证：Release 0 warning/0 error；Core 59/59、Worker 199/199、Playnite 283/283（57 跳过）；多主题多尺寸 `render-qa OK`；真实 Playnite `0.6.70.0` 已安装并截图复核导航材质和宽域洗色。

## 2026-08-24 UI-307 当前事实：首页圆角外溢与导航材质均匀化

- `OverviewTodayHeroCard` 的装饰椭圆必须保持在卡片内部；不要恢复 `Margin="-112..."` 等负边距。WPF `ClipToBounds` 是矩形裁切，不会按 `CornerRadius` 裁切，负边距会在圆角外留下直角色块。
- `AmbientMaterialLayer.ShowLeftGlow` 默认是 `true`，页面局部层继续显示左侧环境光；生产 `ShellAmbientMaterialLayer` 必须设置 `ShowLeftGlow="False"`，否则主内容列起点会再次出现竖向亮带。
- 导航栏使用贯穿整栏的中性 `GscSidebarMaterialBrush`，不得恢复透明渐隐到右边缘，也不要重新加入 `SidebarSeamMaterial`、`GscSidebarSeamBrush` 或右侧硬边框。当前材质通过低对比度的整栏渐变体现玻璃感，边界保持均匀。
- Blur 仍只挂在固定尺寸环境光椭圆上；真实导航命令、页面 Binding、滚动/虚拟化、关闭毛玻璃/高对比度降级语义不变。
- UI-307 验证：源码/XAML/WPF 静态门禁通过，Release 0 warning/0 error，Core 59/59、Worker 199/199、Playnite 283/283（57 跳过），多主题多尺寸 `render-qa OK`；真实 Playnite 已安装并截图复核 `0.6.70.0`。

## 2026-08-24 UI-306 历史记录（当前以 UI-307 为准）：Shell 毛玻璃覆盖与导航栏过渡

- 生产 Shell 的 `ShellAmbientMaterialLayer` 当前必须位于 Shell 背景之上、`SidebarLayout`/主内容 Grid 之下，覆盖两列；不要恢复为 `Panel.ZIndex=-1`，否则会被 Shell 背景压住，用户看不到环境光。
- 导航栏右侧硬分割线已移除；`SidebarSeamMaterial` 只负责低成本不可交互的渐隐过渡，不能恢复 `BorderThickness="0,0,1,0"`。导航栏仍使用半透明 `GscSidebarBrush`，导航项、命令、页面切换、列宽和滚动均未改变。
- `GscShellAmbientOpacity` 按 `GlassStrength` 计算，`GscSidebarSeamBrush` 由当前主题和 accent 动态生成；Blur 仍只挂在固定 `AmbientMaterialLayer` 装饰椭圆上，不能对整个 Shell、页面、表格或滚动器加 Blur。
- UI-306 验证：源码门禁、XAML 结构、WPF 静态审查、Release 构建/测试和 `artifacts/ui-qa/shell-glass-final2/render-qa-report.txt` 均通过；真实 Playnite 已安装 `0.6.70.0` 并实际查看首页、存档中心、修改器中心。沙箱直接安装失败是外部 Roaming 目录 ACL 限制，受控当前用户权限安装成功。

## 2026-08-24 UI-305 当前事实：任务表格底部滚动安全区

- `TaskCenterView.xaml` 的 `TaskDataGrid` 使用 `Padding="0,0,0,12"`，为 Playnite 宿主可能覆盖最后一行的水平滚动条保留内部底部安全区；不要通过关闭横向滚动或关闭 `DataGridStarFill` 修复，否则会破坏列可见性和旧的回拉滚动契约。
- 任务表仍保持横向 `Auto`、`ScrollViewer.CanContentScroll=True`、`VirtualizingPanel.ScrollUnit=Item`、Recycling 和真实列宽/Binding/命令。
- RenderHarness 的任务探针使用 500 条数据，明确测试到底→回顶→再到底→中段→回顶，并比较最后一行底部与水平滚动条顶部；任务表无效行、底部覆盖或回拉异常都必须失败。
- UI-305 验证：源码门禁、XAML/WPF 静态检查、Release 0 warning/0 error、Core 59/59、Worker 199/199、Playnite 283/283（57 跳过）及 `artifacts/ui-qa/task-bottom-final/render-qa-report.txt` 均通过。真实 Playnite 逐像素宿主表现仍需用户复核。

## 2026-08-24 UI-304 当前事实：毛玻璃强度联动与固定环境光增强

- `AdaptiveThemePalette` 现在保存规范化 `GlassStrength`；`ApplyAccentResources` 使用它增加固定 accent/info/success 环境光及中心柔光的 alpha，`ApplyMaterialResources` 使用它计算 `GscAmbientPageOpacity`。100% 不再只是让卡片更不透明。
- `AmbientMaterialLayer.xaml` 目前有四个固定尺寸环境光椭圆，新增 `GscAmbientCenterShadowColor`。Blur 仍只附着到这四个装饰椭圆，半径为 24、`RenderingBias.Performance`；严禁把 Blur 提升到页面根、文字、表格、列表或滚动表面。
- 高强度玻璃表面的 alpha 采用受控透光曲线（强度越高越能透出固定环境光），低强度保持稳定阅读底色；`EnableGlassEffects=false`、高对比度和真实 null 效果降级语义不变。
- UI-304 验证：源码门禁、XAML 结构、WPF 静态审查和临时开启 100% 的主题离屏 QA 通过；RenderHarness 已恢复为无毛玻璃布局基线，避免测试夹具污染正式代码。Release 构建 0 警告/0 错误，Core 59/59、Worker 199/199、Playnite 282/282（57 跳过），标准 `render-qa` 为 `render-qa OK`；生产安装已核验 `0.6.70.0`。真实 Playnite 逐像素强度仍需用户本机复核。

## 2026-08-24 UI-303 当前事实：TextBox 内容宿主重复 Padding 修复

- 任务中心输入文字裁切的根因是模板将 `TextBox.Padding` 绑定到 `PART_ContentHost.Margin`，而 WPF TextBox/宿主又会对内容宿主应用自身 Padding；Task 搜索框上下 `7 DIP` 被重复计算，导致 `PART_ContentHost` viewport 只有 `5 DIP`。不要恢复 `Margin="{TemplateBinding Padding}"`，也不要靠增加 TextBox 高度规避。
- `WpfUiProduction.xaml` 与 `DesignTokens.xaml` 的 TextBox `PART_ContentHost` 当前固定 `Margin="0"`、`Padding="0"`、`BorderThickness="0"`，输入起点由 TextBox 的 Padding 单独负责；字体、字号、字重和前景色均从 TextBox 显式传入内容宿主。
- 当前共享 TextBox 样式使用 `GscUiFontFamily`、`GscBodyFontSize` 和 `FontWeight=Normal`。Task/Dashboard 图标搜索框的 `30 DIP` 左侧输入区、右侧清除按钮区、Binding、命令和焦点语义保持。
- UI-303 输入态验证：离屏 RenderHarness 注入“存档”后文字完整显示，`PART_ContentHost` viewport 从 `5` 恢复为 `19 DIP`；空态/输入态 render QA 均通过。Release 0 warning/0 error、Core 59/59、Worker 199/199、Playnite 282/282（57 跳过），生产安装核验 DLL `0.6.70.0`；真实 Playnite 逐像素截图仍需用户复核。

## 2026-08-24 UI-302 当前事实：任务中心搜索框可见性与图标间距

- TaskCenter 的“搜索任务…”和 Dashboard 的游戏库搜索框是带放大镜的特殊输入框，当前 `TextBox.Padding` 与占位提示 `Margin` 均为 `30,7,38,7` / `30,0,38,0`（Task 提示右侧为 `12`）；`30` 是图标与实际文字之间的独立安全间距。普通无图标输入框仍按共享样式的常规左侧内边距处理。
- `WpfUiProduction.xaml` 与 `DesignTokens.xaml` 的 TextBox 模板均要求 `PART_ContentHost` 显式设置 `TextElement.Foreground="{TemplateBinding Foreground}"`，确保输入文字继承 Gsc 前景色；不要仅靠宿主默认 TextBox 主题推断文字颜色。
- 右侧清除按钮、现有 Binding、搜索过滤、命令、键盘焦点和无障碍名称未改变。UI-302 只修复输入可见性与图标间距。
- UI-302 验证：源码验证、XAML 19/19、WPF 静态审查 0 error/20 warnings/165 info、Release 0 warning/0 error、Core 59/59、Worker 199/199、Playnite 282/282（57 跳过）、`artifacts/ui-qa/ui302-task-search-v1/render-qa-report.txt` 为 `render-qa OK`；真实 Playnite 逐像素输入截图仍需用户复核。

## 2026-08-24 UI-301 当前事实：表格右侧安全边距与搜索输入起点

- 共享 `WpfUiProduction.xaml` 的 `GscRoundedDataGridRowTemplate` 和 `DashboardView.xaml` 的本地兼容行模板，选中 `RowChrome` 使用 `4,2,12,2`；`12` 是为 Playnite 宿主垂直滚动轨道保留的右侧安全区。不要恢复到 `4,2,8,2` 或旧的 `4,2`。
- UI-301 曾将 TaskCenter 与 Dashboard 游戏库搜索框的图标字段收紧到 `20` DIP；UI-302 已将当前值修正为 `30` DIP，提示文本同步从 `30` 起始，右侧 `38` DIP 仍为清除按钮预留区。共享普通 TextBox 的左对齐模板、数值输入的专用对齐和清除按钮行为不变。
- UI-301 验证：`validate-source.py`、XAML 19/19、WPF 静态审查 0 error/20 warnings/165 info、Release 0 warning/0 error、Core 59/59、Worker 199/199、Playnite 282/282（57 跳过）、`artifacts/ui-qa/ui301-spacing-v1/render-qa-report.txt` 的 `render-qa OK`；受控一键安装已核验生产扩展 `0.6.70.0`。真实 Playnite 宿主逐像素边距仍需用户复核。

## 2026-08-24 UI-300 / FUNC-004 当前事实：表格、输入框与 FLiNG 归档

- `WpfUiProduction.xaml` 的共享 `GscRoundedDataGridRowTemplate` 选中描边使用 `4,2,12,2` 安全边距，`DashboardView.xaml` 的本地兼容行模板同步；不要把右边距恢复为 `4,2,8,2` 或 `4,2`，否则宿主垂直滚动轨道可能覆盖右侧圆角。共享行的排序、SelectiveScrollingGrid、Item 滚动和 Recycling 不变。
- `GscWpfUiTextBox` 与 `GscTextBox` 的普通文本默认 `HorizontalContentAlignment=Left`、`TextAlignment=Left`，模板把对齐属性传给 `PART_ContentHost`；数值输入专用样式仍可覆盖对齐方式。普通搜索框通过 `GscSearchClearButton` 在非空时显示清除动作并清空后恢复焦点。
- `AcrylicProductionShellView`、`MediaCenterView`、`TaskCenterView`、`TrainerCenterView` 的搜索清除按钮均只影响输入值和焦点，不改绑定/命令；Trainer 的响应式宽度现在作用于 `TrainerSearchBoxHost`，避免按钮被宽度赋值挤出输入框。
- `FlingTrainerCatalogSource` 从 `https://archive.flingtrainer.com/` 有界 BFS 解析 `.zip`、`.rar`、`.7z`、`.exe` 归档直链，并保留同主机 HTTPS、目录/文件上限和可降级在线目录刷新。`GameToolService` 先保留 ZIP/direct EXE 路径，RAR/7z 使用 SharpCompress reader 流式写入临时版本目录，通过 `ArchivePathGuard` 与 1 GiB 单文件/4 GiB 总展开限制后再选择修改器 EXE；不执行下载内容。
- `SharpCompress` 版本由 `Directory.Packages.props` 统一锁定为 `0.50.4`。本阶段验证：源码门禁通过，XAML 19/19，WPF 0 error/20 warnings/165 info，Release 0 warning/0 error，Core 59/59、Worker 199/199、Playnite 282/282（57 跳过），`artifacts/ui-qa/ui300-input-fling-v1/render-qa-report.txt` 为 `render-qa OK`。受控运行 `scripts/dev-install-run.ps1 -Configuration Release -NoStart` 已安装生产扩展并核验 `0.6.70.0`；沙箱内直接运行时的 Access denied 只来自受限环境写不了 Roaming Playnite 扩展目录。真实 FLiNG 归档下载/运行和安全软件拦截仍不由自动验证声称覆盖。

## 2026-08-24 UI-299 当前事实：表格字体与行表面可读性

- `DesignTokens.xaml` 新增 `GscTableRowBrush`；`AdaptiveThemePalette.ApplyRuntimeThemeResources` 和 `ApplyDemoCoreResources` 为浅/深主题提供低透明度行面/交替行面，高对比度保持透明降级。
- 共享 `WpfUiProduction.xaml` 的隐式 `DataGrid` 与 `Redesign.xaml` 的 `GscRedesignWorkspaceDataGrid` 使用 `TextOptions.TextFormattingMode=Display`；`DataGridCell` 显式使用 `GscUiFontFamily`/`GscBodyFontSize`。不要把行底色改成不透明大色块，也不要把 Display 文本格式化扩展到大范围页面滚动器。
- `RowBackground`、隐式/稳定 `DataGridRow` 的正常背景仍来自动态 `GscTableRowBrush`，`AlternatingRowBackground`、Hover、选中态、表头排序箭头和列宽拖拽保持现有共享模板；不要移除 `SelectiveScrollingGrid`、Recycling、Item scrolling 或媒体收件箱的 Standard 虚拟化例外。
- UI-299 当前验证：源码验证、XAML 19/19、WPF 静态审查、Release 0 警告/0 错误、Core 59、Worker 198、Playnite 281 通过/57 跳过；`artifacts/ui-qa/table-readability-v1/render-qa-report.txt` 为 `render-qa OK`。
- 真实 Playnite 日志已确认隔离 Preview 后加载 `GameSaveCenter` 0.6.70；同一用户配置同时放置旧 Preview 时，Playnite 会先加载其 0.6.71 的同名 `GameSaveCenter.Contracts`，导致标准插件的 `MediaInboxBatchResultDto` 类型加载冲突。该冲突属于外部 Preview 安装状态，不要把 Preview 目录禁用动作当成源码修复；本阶段未在 Computer Use 中点击宿主页面，因为返回窗口无真实 HWND，避免把 Codex 截图误当 Playnite。
- 2026-08-24 复核：改由 Windows 正常启动 Playnite 后，启动页短暂出现真实 HWND，但主窗口显示后 `MainWindowHandle` 又回到 0；Computer Use 重新选窗、激活、Raise 后仍返回 Codex 截图和 `EmptyWindowAutomationPeer`。未发送点击或滚动输入；这属于当前宿主/Computer Use 窗口映射限制，不是 UI 代码通过后的失败。Preview 已用 238 个文件恢复原路径，Playnite 与标准 Worker 均已停止。
- 同轮新增真实 WPF 操控证据：临时宿主直接加载生产 `SaveCenterView`、当前主题资源和 160 行 `DataGrid`；Computer Use 实际切换“历史版本”标签、选择第二行、向下滚动再滚回，选中行高亮、右侧详情和表格字体/行面均保持正确。临时宿主及构建输出已清理，正式源码未被测试夹具污染。

## 2026-08-23 UI-298 当前事实：安全毛玻璃高光与环境光 Blur

- `DesignTokens.xaml` 的 `GscAmbientBlurEffect` 默认必须是 `x:Null`；`AdaptiveThemePaletteFactory.ApplyMaterialResources` 仅在 `glassEnabled` 时创建半径 18、`RenderingBias.Performance` 的冻结 `BlurEffect`，关闭毛玻璃或高对比度时返回真实 null。
- `AmbientMaterialLayer.xaml` 只把该效果挂到三个固定尺寸的装饰椭圆；禁止把它提升到根 Grid、页面内容、TextBlock、DataGrid、ListBox 或任何大范围滚动容器，否则会破坏性能和可读性。
- `AcrylicProductionShellView.xaml` 的玻璃高光是 1 DIP、`IsHitTestVisible=False` 的装饰 Border，不参与布局输入；`ApplyRuntimeThemeResources` 在 `glassEnabled=false` 时将 `GscGlassHighlightBrush` 置为透明。
- 现有 `EnableGlassEffects`、高对比度、页面环境光隐藏和不透明主题表面降级语义不变；不要引入 Windows 宿主级 Backdrop、修改 Playnite WindowChrome 或对整个插件做 Blur。
- UI-298 验证：WPF 资源定向测试 2/2、源码验证通过；WPF 静态审查 0 error/21 warnings/164 info；`artifacts/ui-qa/glass-blur-v1/render-qa-report.txt` 为 `render-qa OK`；Release 解决方案 0 warning/0 error。尚未在真实 Playnite 宿主逐像素验证。

## 2026-08-23 FUNC-003 当前事实：FLiNG 历史归档递归解析

- `FlingTrainerCatalogSource.SyncCatalogAsync` 先解析在线目录，再通过 `GetArchiveCatalogAsync` 从 `https://archive.flingtrainer.com/` 以有界 BFS 递归读取归档目录；每个目录只允许继续访问同一归档主机的 HTTPS 子目录。
- 归档文件只接受 `.zip` 和 `.exe`，每次扫描最多 2048 个目录、10000 个文件，结果按 `PageUrl` 去重后写入现有 Trainer catalog；FLiNG 的旧归档文件仍通过现有单文件 release 路径下载。
- `ParseArchiveDirectoryListing` 同时返回文件和子目录，必须保留相对路径解析、外部主机过滤和目录去重；不要把归档递归改成无界网络爬取，也不要放开非 FLiNG 主机。
- 同步日志格式包含总数、在线目录数和归档目录数，例如 `Synchronized ... FLiNG catalog entries (... online, ... archive)`，便于确认 2019 年以前条目是否实际进入本地目录。
- FUNC-003 当前验证：Release 解决方案构建 0 warning/0 error；Core 59/59、Worker 198/198 通过，FLiNG 解析定向测试 3/3 通过。尚未在真实 FLiNG 归档站点和真实 Playnite 宿主中做网络/宿主验收；本阶段没有 UI/XAML 改动。

## 2026-08-23 PERF-001 当前事实：大型库 Worker 预热与 Dashboard 版本探测解耦

- `GameSaveCenterPlugin.OnApplicationStarted` 对 100+ 游戏库会后台调用 `StartWorkerAndScheduleSynchronizationAsync` 预热 Worker；`StartWorkerAndScheduleSynchronizationAsync` 在大型库仍会在 `interactiveSurfaceOpened == false` 时直接返回，不得提交全库 `UpsertGames`/Ludusavi 匹配。
- `WaitForLibraryReadyAndStartWorkerAsync` 在库稳定为大型库后同样预热 Worker；这只提前完成 Worker/SQLite 初始化，不改变 500+ 大库的缓存优先和显式刷新门禁。
- `DashboardService.GetAsync` 不再等待 `ludusavi --version`；首次快照读取内存缓存并通过 `RefreshLudusaviVersionAsync` 后台探测，版本结果在后续快照显示。不要把版本探测重新放回首个 Dashboard IPC 请求。
- `WorkerInitializationService` 记录 storage、stale-task reconciliation、snapshot cleanup 和总耗时；这些日志用于继续定位用户机器上的慢启动阶段。
- PERF-001 验证：Release 解决方案构建 0 warning/0 error；新增定向 Playnite 源码测试通过。真实 Playnite 宿主中的预热时序仍待用户机器实测。

## 2026-08-23 FUNC-002 当前事实：媒体收件箱忽略恢复

- `MediaCenterView.xaml` 的待归类页在既有 `MediaInboxBatchActionRow` 内增加 `MediaInboxMode` 轻量视图切换，选项为“待归类”和“已忽略”。默认仍只加载 `ListUnassignedMedia`；切换到“已忽略”后才按需请求 `ListIgnoredMedia`，避免旧 Worker 在正常启动路径上因未知新消息而受影响。
- `MediaInboxItems` 是 DataGrid 的当前显示集合，`UnassignedMedia` 和 `IgnoredMedia` 分别保留两种缓存。已忽略模式隐藏目标游戏、归类和忽略命令，只显示 `RestoreIgnoredMediaBatchCommand`；切回待归类时恢复原有单条/批量归类操作。不要把已忽略项目重新接回 `AssignInboxMediaCommand` 或 `IgnoreInboxMediaCommand`。
- Worker 新增 `media.inbox.ignored.list` 与 `media.inbox.ignored.restore.batch`。恢复批次最多 500 个去重 ID，逐项复用安全文件移动：优先使用现有归档副本，否则从原始文件重建副本；目标已存在时必须做 SHA-256 相同校验，禁止覆盖不同内容。数据库状态变为 `Inbox`，PlayniteId 清空，CloudState 为 `NotApplicable`，原因固定为“用户撤销忽略，待重新归类”，并追加审计。
- 恢复完成后 Playnite 同时刷新待归类和已忽略列表，避免用户切换视图看到过期缓存。当前视图切换不持久化，页面重新打开默认进入待归类。
- RenderHarness 的 `FakeDashboardData` 通过 `MediaInboxItems => UnassignedMedia` 投影兼容生产绑定，保留 4468 行收件箱虚拟化/回顶探针，不要因为新增显示集合把夹具改回空列表。
- FUNC-002 验证：`validate-source.py`、XAML 19/19、Release 0 warning/0 error；Core 59、Worker 196、Playnite 280 通过/57 跳过；`artifacts/ui-qa/media-inbox-restore-v1/render-qa-report.txt` 为 `render-qa OK`；WPF 审查 0 error、21 warnings、164 info。没有在真实 Playnite 中执行恢复操作，宿主 DPI、高对比度与真实点击仍待人工验收。

## 2026-08-23 FUNC-001 当前事实：媒体收件箱批量处理与侧栏真实状态

- `MediaCenterView.xaml` 的 `MediaInboxGrid` 现在允许 `Extended + FullRow` 多选；表格上方新增 `MediaInboxBatchActionRow`，仅提供目标游戏 ComboBox、`归类所选` 和 `忽略所选`，原有 Inspector 与单条归类/忽略命令保持不变。`MediaInboxGrid` 仍保留既有 `Standard` 行虚拟化、关闭列虚拟化和 `GscDataGridStarFill` 例外，不要为批量操作恢复 star-fill 或改掉滚动模型。
- 新 IPC 类型为 `media.reassign.batch` 与 `media.inbox.ignore.batch`，请求在 Worker 侧最多 500 个去重媒体 ID；Playnite 对更大选择自动分批，每批使用较长请求超时。Worker 逐项复用现有归类/忽略逻辑，保留归档副本和审计记录，失败项通过 `MediaInboxBatchResultDto.Failures` 返回，取消不吞掉。
- 批量归类复用 `InboxTargetGame`，忽略仍需一次安全确认；批量完成后刷新 Dashboard 和 Inbox，部分失败在状态栏和错误提示中显示首条错误。不要把单批上限扩大到无界，也不要改成前端逐项发起数千个 IPC 请求。
- `AcrylicProductionShellView.xaml` 的 Worker/Ludusavi 指示灯和状态文字由 `Snapshot.WorkerHealthy`/`Snapshot.LudusaviAvailable` 的 DataTrigger 驱动，显示“正常/不可用”“可用/不可用”，不再显示原始布尔值。
- FUNC-001 验证：`validate-source.py` 与 XAML 19/19 通过；Release 0 warning/0 error；Core 59、Worker 194、Playnite 280 通过/57 跳过；`artifacts/ui-qa/media-inbox-batch-v1/render-qa-report.txt` 为 `render-qa OK`；WPF 审查 0 error、21 warnings、164 info。未执行真实 Playnite 批量数据变更、宿主 DPI 或高对比度人工验收。

## 2026-08-22 UI-297 当前事实：页面激活时同步运行中游戏

- 旧行为是：Dashboard 首次快照按 `GameSelectionResolver` 选择运行中游戏，页面已打开时由 `PlayniteGameStarted` 事件切换；普通刷新保留用户手动选择。Worker 进程检测首轮是基线扫描，因此 Worker 在游戏已经运行后才启动时，快照可能暂时没有该会话。
- `GameSaveCenterPlugin.TryGetCurrentlyRunningPlayniteGameIds()` 只读 Playnite SDK 的 `Game.IsRunning`，不启动 Worker 会话、不发 `GameSessionStarted`、不扫描进程、不触发备份或其他自动化。
- `DashboardView` 在 `Loaded` 和重新变为可见时调用 `DashboardViewModel.SelectCurrentlyRunningGameOnViewActivation()`；快照异步晚到时也会调用一次。该方法覆盖当前 DTO 的运行状态、刷新 GamePicker 缓存行并按既有 resolver 规则选择运行中游戏；没有运行中游戏时继续保留用户上次选择。
- `GamePickerItem` 实现 `INotifyPropertyChanged` 以支持不替换缓存对象时刷新运行状态；GamePicker 仍然虚拟化/本地筛选，未新增定时器、Worker IPC 轮询或网络请求。
- UI-297 验证：`scripts/validate-source.py` 通过；隔离 Release 构建 XAML 19/19、0 warning/0 error；新增页面激活自动定位源码测试通过；WPF 静态审查 0 error、21 warnings、164 info。完整 Playnite 测试当前分支仍有 19 个既有 Demo/布局断言失败、240 通过、62 跳过，未归因于本改动；真实 Playnite 宿主需启动后验证页面反复打开/切换时的实际选框。

## 2026-08-22 UI-296 当前事实：任务/媒体摘要条实际宿主布局

- `TaskCenterView.xaml` 与 `MediaCenterView.xaml` 的摘要条当前统一使用 `* / Auto / * / Auto / * / Auto / *` 七列；四个统计块位于 `0/2/4/6`，三条竖线位于 `1/3/5`，最后一个统计块右侧不能有 Rectangle。`Auto` 分隔列与首页的 `OverviewStatStrip` 保持同一布局契约，避免恢复旧的四列重叠结构。
- 用户截图中的错位来自实际宿主仍加载旧的四列布局；本轮已通过 `scripts/dev-install-run.ps1 -Configuration Release -NoStart` 将当前 `0.6.70.0` 安装到标准 Playnite 扩展目录。该安装过程没有读取或假定 Demo 文件夹，也不会停止 `GameSaveCenterPreview` 的其他 Worker。
- UI-296 验证：源码门禁通过；Release 为 0 warning/0 error；Core 59、Worker 194、Playnite 277/57/0；`artifacts/ui-qa/summary-divider-layout-fix/render-qa-report.txt` 为 `render-qa OK`，双主题和多尺寸回归通过。真实 Playnite 已安装新 DLL，但本轮未自动捕获嵌入页面像素，需用户启动后复核截图。

## 2026-08-22 UI-295 当前事实：媒体摘要块后的竖线契约（已由 UI-296 统一为 Auto 分隔槽）

- `MediaCenterView.xaml` 的 `MediaSummaryPanel` 使用 7 列 `* / Auto / * / Auto / * / Auto / *`；四个真实统计块在 `0/2/4/6`，三条竖线在 `1/3/5`。不要把 Rectangle 和统计 StackPanel 放进同一列，也不要在最后一个统计块右侧增加竖线。
- `MediaSummary.TotalCount`、截图/录像数量、`TotalSizeDisplay`、`FavoriteCount` 和 `Snapshot.UnassignedMediaCount` 的真实 OneWay Binding 与摘要文案保持不变；这只是几何布局修复。
- UI-295 验证：源码门禁通过；Release 为 0 warning/0 error；Core 59、Worker 194、Playnite 277/57/0；`artifacts/ui-qa/media-summary-divider-fix/render-qa-report.txt` 为 `render-qa OK`，亮/暗主题媒体摘要已抽查。RenderHarness 是离屏证据，不等同真实 Playnite 宿主逐像素验收。

## 2026-08-21 UI-294 当前事实：任务统计分隔线与全工作区环境光（摘要列已由 UI-296 统一为 Auto 分隔槽）

- `TaskCenterView.xaml` 的任务统计条固定为 7 列：四个 `*` 统计列与三个 `Auto` 分隔列交替；分隔线位于分隔列并居中，不能恢复为与统计项共用列。
- `Controls/AmbientMaterialLayer.xaml` 是共享的、无 BlurEffect 的环境光层，已放在 Overview、Save、Trainer、Media、Task、Maintenance 六个页面的真实内容之后；Settings 页使用相同的自适应环境光颜色。它不改变页面尺寸、滚动器、命令、Binding 或虚拟化列表。
- `AdaptiveThemePaletteFactory.ApplyMaterialResources` 提供 `GscAccentShadowColor`、`GscInfoShadowColor`、`GscSuccessShadowColor` 和 `GscAmbientPageOpacity`；透明效果关闭或高对比度时环境光层隐藏。该方案是安全的渐变材质，不应改成对整页或 DataGrid 使用 BlurEffect。
- UI-294 验证：源码门禁通过；Release 为 0 warning/0 error；Core 59、Worker 194、Playnite 276/58/0；`artifacts/ui-qa/task-ambient-final5/render-qa-report.txt` 为 `render-qa OK`，覆盖双主题、多尺寸、Tab、滚动和 resize。RenderHarness 是离屏证据，不等同真实 Playnite 宿主逐像素验收；Demo 文件夹不是运行时或测试依赖。

## 2026-08-21 UI-293 当前事实：首页关注事项与比较质量状态

- `OverviewView.xaml` 的 `AttentionFindings` 行使用 `26` DIP 图标列、`*` 标题列和 `220` DIP 建议列；建议列不能恢复为无约束 `Auto`，否则长 `SuggestedAction` 会把标题/游戏名挤成单字宽。真实 `SuggestedAction` 仍右对齐并用省略号，完整内容通过 ToolTip 可读。
- `SaveCenterView.xaml` 的“版本比较”标题行使用明确的标题/气泡两列布局；标题、`GscRedesignContextPill` 和内部文本都设为垂直居中。`LastBackupDiff.ComparisonQualityDisplay` 同时使用 `TargetNullValue=等待比较` 和 `FallbackValue=等待比较`，比较前不应出现空色块；有实际 DTO 时仍显示 Worker 返回的真实质量。
- `DashboardViewModel.diffSummary` 初始值为“选择两个版本后，比较结果会显示在这里。”，比较前的结果区域不能恢复为空字符串；执行比较后仍由 `diff.Summary` 覆盖。
- UI-293 验证：Release 构建为 XAML 18/18、0 warning/0 error；Core 59、Worker 194、Playnite 276/58/0；`artifacts/ui-qa/attention-pill-fix/render-qa-report.txt` 为 `render-qa OK`，覆盖双主题、多尺寸、Tab、滚动和 resize。RenderHarness 是离屏证据，不等同真实 Playnite 宿主逐像素验收；Demo 文件夹不是运行时或测试依赖。

## 2026-08-21 UI-292 当前事实：设置路径输入框与生产壳顶部

- `WpfUiProduction.xaml` 的 `GscWpfUiTextBoxTemplate` 使用拉伸的 `PART_ContentHost`，正文通过 `VerticalContentAlignment=Center` 垂直居中；`GscWpfUiTextBox` 默认 `HorizontalScrollBarVisibility=Hidden`，避免长路径自动水平滚动条占用输入框底部。
- `DesignTokens.xaml` 的 `GscTextBox` 与 WPF-UI 适配器使用适合 36 DIP 控件的垂直内边距；不要恢复过大的上下 Padding，也不要给设置页路径字段重新加 `HorizontalScrollBarVisibility=Auto`，否则长路径会把文字视口压缩并裁切。
- `GameSaveCenterSettingsView.xaml` 的 Worker、Ludusavi、存档目录、Rclone、云端目标、媒体目录和镜像目录仍绑定真实设置属性，编辑/保存语义不变；横向滚动条只是隐藏，获得焦点后仍可编辑长路径。
- `AcrylicProductionShellView.xaml` 不再显示“主题 / 跟随 Playnite / 浅色 / 深色”顶部 utility surface，Header 直接使用右侧区域第一行；`AcrylicProductionShellView.xaml.cs` 与 `DashboardView.xaml.cs` 不再有壳主题按钮回调。主题选择仍由 Playnite 设置页的 `ThemeMode` 下拉框提供，动态调色板链未删除。
- UI-292 验证：长路径 STA 测量断言通过；Release 为 XAML 18/18、0 warning/0 error、Core 59、Worker 194、Playnite 276/58/0；最终 `artifacts/ui-qa/settings-theme-fix-final/render-qa-report.txt` 为 `render-qa OK`。RenderHarness 仍是离屏证据，不等同真实 Playnite 宿主逐像素验收。

## 2026-08-21 BUILD-003 当前事实：测试根目录不受外部 Demo 工作目录影响

- `WpfUiResourceDictionaryTests`、`RestoredAcrylicForkBaselineTests`、`NumericInputTests` 的仓库根目录探测只从 `AppContext.BaseDirectory` 向上查找 `GameSaveCenter.sln`；找不到时明确失败，不再信任进程当前工作目录。
- 因此即使一键安装器从 `D:\workplace\Github\GameSaveCenter.AcrylicFork` 启动，测试也会读取当前隔离构建输出对应的 `GameSaveCenter` 仓库，不会假定外部 Demo 目录存在。
- BUILD-003 验证：外部 Demo 目录作为当前工作目录时，当前仓库 Playnite 测试 276/58/0；完整 Release 构建为 XAML 18/18、Core 59、Worker 194、Playnite 276/58/0；没有新增 Demo 路径、环境变量或生产运行时依赖。

## 2026-08-21 UI-291 当前事实：媒体来源规则宽窄布局

- `MediaCenterView.xaml` 的来源规则页使用 `MediaSourceLayout`：宽屏为 1.1* 表单、14 DIP 间距、* 规则列表；窄屏由 `ApplyResponsiveLayout` 改为表单在上、规则列表在下，字段从两列收为一列。
- 表单默认可见，真实 `CustomMediaSourcePath`/`CustomMediaPattern`/`CustomMediaShared` 绑定及 `AddMediaSourceCommand` 保留；规则列表继续使用 `MediaSources`、更新/移除命令、内部 Auto 滚动、行虚拟化和空状态。
- 媒体待归类页的 Inspector、目标游戏、归类/忽略/批量操作和大数据表格虚拟化契约未改动；窄屏只给来源表单和规则列表各自有限视口。
- UI-291 证据：当前最终审计 Release 0 warning/0 error；Core 59、Worker 194、Playnite 276/58/0；`artifacts/ui-qa/ui-final-audit/render-qa-report.txt` 为 `render-qa OK`，双主题、多尺寸和 resize 通过。未宣称真实 Playnite 生产宿主逐像素验收，Demo 文件夹不是运行时或测试依赖。

## 2026-08-21 UI-290 当前事实：首页信息层级

- `OverviewPrimaryFlow` 的固定行契约是：`OverviewHeroAndGameRow` 行 0、`OverviewStatStrip` 行 1、`OverviewHomeToolbar` 行 2、`OverviewActivityColumn` 行 3/4；`OverviewSecondaryScrollViewer` 仍由响应式代码在宽屏并列、窄屏后置。
- 首页辅助 toolbar 仍保留 `RefreshCommand`、`BackupAllCommand`、`SyncMediaCommand`、`OpenAttentionCenterCommand` 和环境检查 `OpenMaintenanceCommand`；只是视觉层级后置，不得删除这些真实入口。
- UI-290 验证：源码门禁通过；Release 0 warning/0 error；Core 59/59、Worker 194/194、Playnite 276/58/0；`artifacts/ui-qa/overview-order-ui288/render-qa-report.txt` 为 `render-qa OK`。
- 该 RenderHarness 报告只能作为离屏多尺寸/双主题回归，不能替代 Playnite 宿主像素截图或高 DPI/高对比度人工验收。

## 2026-08-21 UI-289 当前事实：共享控件尺寸

- 普通按钮/输入/ComboBox 的共享基准是 `GscButtonHeight=36`；紧凑按钮使用独立 `GscCompactButtonHeight=30`，不能在页面上重新写回 38 DIP。
- `DesignTokens.xaml` 的 `GscTextBox`、`GscNumericFieldInput`、`GscComboBox`，以及 `WpfUiProduction.xaml` 的 WPF-UI TextBox/ComboBox/按钮适配器都引用普通动态令牌；`GscWpfUiCompactButton` 只引用紧凑令牌。
- 首页保护动作仍保留显式普通高度，因为它是高风险操作例外；普通/紧凑工具栏和存档操作不再通过页面 `MinHeight` 覆盖共享资源。
- UI-289 验证：源码门禁通过；Release 0 warning/0 error；Core 59/59、Worker 194/194、Playnite 275/59/0；WPF validator 0 error、20 warnings、164 info。

## 2026-08-21 UI-288 当前事实：生产壳主题操作与设置入口

- `AcrylicProductionShellView` 的 44 DIP 顶部 utility band 现在是可操作的主题条，不是空白占位；三个 RadioButton 分别对应 `GameSaveCenterThemeMode.FollowPlaynite/Light/Dark`，样式位于 `AcrylicProductionResources.xaml` 的 `AcrylicThemeModeItem`。
- `DashboardView` 给生产壳注入两个真实回调：设置项调用 `plugin.PlayniteApi.MainView.OpenPluginSettings(plugin.Id)`；主题项写入 `plugin.Settings.ThemeMode`、保存设置、触发 `NotifyVisualSettingsChanged()`，并沿用 `AdaptiveThemePaletteFactory` 的动态资源刷新。
- 侧栏 `NavSettings` 不是工作区，不会改写 `DashboardViewModel.CurrentWorkspace`；点击时恢复当前工作区 RadioButton 后打开 Playnite 设置，设置页内部分类 Tab、真实字段和保存语义保持不变。
- UI-288 验证：源码门禁通过；Release 0 warning/0 error；Core 59/59、Worker 194/194、Playnite 274/60/0。该阶段仍未宣称真实 Playnite 嵌入像素验收；Demo 文件夹不是运行时或测试依赖。

## 2026-08-21 BUILD-002 当前事实：跨电脑测试不再依赖 Demo 目录

- `9b19dbd` 之后的跨电脑失败来自五个 Playnite 视觉对照测试仍读取开发者机器的 `D:\workplace\Github\GameSaveCenter.AcrylicFork`；不是生产代码编译或 UI 实现回退。
- 已删除 `tests/GameSaveCenter.Playnite.Tests/AcrylicForkDesignSource.cs`、`AcrylicForkDesignFactAttribute.cs` 及五个测试中的外部 Demo 读取。相关测试现在是普通 `[Fact]`，只验证当前仓库内生产资源契约；Demo 不再是测试运行时输入，也不需要环境变量或兄弟目录。
- 验证：设置 `GSC_ACRYLICFORK_ROOT` 指向不存在目录时，完整 Playnite 为 274 通过、60 跳过、0 失败，共 334 项；隔离 Release 构建成功。没有修改生产 UI、命令、Binding 或业务逻辑。

## 2026-08-21 UI-287 当前事实：共享表格表头排序箭头与列宽调整

- 生产共享 `DataGridColumnHeader` 模板之前只有排序 `Path`，没有 WPF 约定的 `PART_LeftHeaderGripper`/`PART_RightHeaderGripper`，因此虽然 `CanUserResizeColumns=True`，实际表头没有可拖拽列宽的命中区域。
- 排序箭头之前放在固定 14 DIP 列中，但路径本身宽约 13 DIP 还带右侧 inset，窄列排序时会被裁掉；现在预留 22 DIP 独立箭头列，并保留 64 DIP 的共享最小列宽。
- `WpfUiProduction.xaml` 的共享表格和 `DashboardView.xaml` 的兼容表格均保留透明 resize Thumb、完整排序箭头、现有选中态、滚动和虚拟化；不改列绑定、命令或业务排序逻辑。
- RenderHarness 现在对 Save/Task/Media/Maintenance 探测表格检查真实表头模板部件，并强制验证排序状态下箭头有非零布局宽度；第一列左 Thumb 被 WPF 自动折叠是正常边界行为，至少一个边界命中区必须有效。
- UI-287 证据：`artifacts/gsc-b/ui287-table-header-v2` 构建/测试通过，XAML 18/18、0 warning/0 error、Core 59、Worker 194、Playnite 274 通过/60 跳过；`artifacts/ui-qa/ui287-table-header-v2/render-qa-report.txt` 为 `render-qa OK`，覆盖双主题、1040/1100/1366/2560、多 Tab、滚动和 resize transition。真实 Playnite 生产宿主逐页拖拽验证仍未完成。

## 2026-08-21 UI-286 当前事实：修改器导入线程与 FLiNG 历史归档

- `GameToolService.InspectImportAsync` 不得把文件名包含 `Update` 的所有 EXE 排除；显式选中的单文件必须按扩展名保留。目录/ZIP 候选只排除明确的 `unins*`、`uninstall`、`update`、`updater`、`setup` 辅助入口，`Outlast 2 v1.0-Update 2 Plus 4 Trainer.exe` 是有效修改器入口。
- `DashboardViewModel.PrepareGameToolImportAsync`、`ClearPendingGameToolImport` 和导入后选中工具更新涉及绑定集合/属性，必须通过 `ApplyOnUi`；`ImportEntryCandidates` 的 `CollectionView` 不能从 IPC/Worker continuation 修改。
- `FlingTrainerCatalogSource` 现在可选同步 `https://archive.flingtrainer.com/` 的目录链接；仅登记 ZIP/EXE，归档条目的 `PageUrl` 是安全校验后的直接下载地址，`GetReleasesAsync` 返回单一归档版本。下载继续复用现有 HTTPS host 校验、2 GiB 限制、ZIP 路径/大小/文件数校验和入口筛选；RAR/7z 尚未支持。
- UI-286 证据：`artifacts/gsc-b/ui286-trainer-import-v2` 构建/测试通过，Worker 194、Playnite 273/60；`artifacts/ui-qa/ui286-trainer-import-v1/render-qa-report.txt` 为 `render-qa OK`，覆盖双主题、多尺寸、Tab、滚动和 resize。新测试覆盖含 `Update` 的直接 EXE、ZIP 入口和归档目录解析。当前共享环境没有用户的 `D:\Download\Brave` 文件，未进行其真实签名/哈希检查，也未执行它；真实 Playnite 生产宿主逐页验证仍未完成。

## 2026-08-21 UI-285 当前事实：媒体待归类反向滚动禁用共享星号重分配

- `MediaInboxGrid` 处于有限 `Grid` 视口时必须设置 `infra:DataGridStarFill.Enabled=False`。共享 star-fill 只适合无限测量宿主；在媒体 4468 条收件箱中把星号列重算为像素列并 `InvalidateMeasure`，会和 Standard 虚拟行呈现器的底部→顶部回退竞争，产生滚动条仍在但行全部消失的偶发状态。
- 媒体收件箱仍固定 `VirtualizingPanel.ScrollUnit=Item`、`VirtualizationMode=Standard`、行虚拟化开启、列虚拟化关闭；不要为了修复空白而关闭整个虚拟化，也不要把该例外扩散到其他工作区。
- `tests/GameSaveCenter.RenderHarness/Program.cs` 的媒体探针必须覆盖多次底部→顶部→中段回退，并确认 `DataGridStarFill.GetEnabled(MediaInboxGrid)==false`、每一步有实现行且无无效 DataContext/表头 gap。
- UI-285 证据：`artifacts/gsc-b/ui285-media-scroll-v1` 构建/测试通过；`artifacts/ui-qa/ui285-media-scroll-v3/render-qa-report.txt` 为 `render-qa OK`，多尺寸、双主题和反向滚动通过。真实 Playnite 生产宿主逐页截图仍未补齐，Demo-first 总迁移仍未完成。

## 2026-08-21 UI-284 当前事实：共享按钮和主题侧栏已统一

- `Themes/WpfUiProduction.xaml` 的 `GscWpfUiButtonTextTemplate` 必须把宿主 Button 的动态 `Foreground`、字体族、字阶和字重传给内部 `TextBlock`；否则 Primary 按钮文字会回落为黑色，深色主题不可读。页面不得复制按钮模板绕过共享修复。
- `OverviewView.xaml` 的风险操作按钮使用固定 `GscButtonHeight` 的水平布局；`AttentionFindings` 继续是 Demo 风格的真实分隔列表，右侧显示 `SuggestedAction` 文本，空集合仅保留标题、说明和真实维护入口；维护入口必须是有背景/描边的 Secondary 按钮。
- `GameSaveCenterSettingsView.xaml` 的 `SettingsSectionTabs` 使用 `GscSettingsSectionTabs`，不要恢复 `LabSegmented` 灰色整块；选中态、Hover、字体和前景色都从当前主题动态资源读取。
- `SaveCenterView.xaml` 的比较质量气泡固定 28 DIP 高度、内边距和最大宽度，并与“版本比较”标题垂直居中；不要让长文本重新参与标题行高度测量。
- UI-284 证据：`artifacts/gsc-b/ui284-theme-v1` 构建/测试通过；`artifacts/ui-qa/ui284-theme-v1/render-qa-report.txt` 为 `render-qa OK`，双主题、多尺寸和 resize 通过。真实 Playnite 生产宿主逐页截图仍未补齐，Demo-first 总迁移仍未完成。

## 2026-08-21 UI-283 当前事实：媒体待归类使用稳定的大数据虚拟化契约

- `MediaCenterView.xaml` 的 `MediaDataGrid` 继续 `Item` 滚动、行虚拟化、固定共享行高、顶部对齐、排序/列宽调整和圆角选中态；针对真实最多 5000 条的 `UnassignedMedia`，必须局部使用 `VirtualizationMode=Standard` 与 `EnableColumnVirtualization=False`，不能恢复共享的 `Recycling` + 列虚拟化组合。
- 这只是媒体收件箱的性能/呈现例外，不得扩散到 Save/Task/Maintenance 等工作区；其它共享表格仍由 `GscRedesignWorkspaceDataGrid` 使用 `Recycling` 和列虚拟化。当前媒体预览 Inspector、目标游戏 ComboBox、归类/忽略命令、`SelectedInboxMedia` 与安全语义必须保持在 DataGrid 滚动面之外。
- `tests/GameSaveCenter.RenderHarness/Program.cs` 的 `CreateMediaInboxProbeData` 使用 4468 条数据，媒体滚动探针显式期望 Standard/关闭列虚拟化；不要把大数据探针降回 60 条，也不要把所有表格的 Recycling 断言重新写成无例外的全局门禁。
- UI-283 证据：`artifacts/gsc-b/ui283-media-inbox-v1` 构建/测试通过；`artifacts/ui-qa/ui283-media-inbox-v5/render-qa-report.txt` 为 `render-qa OK`，4468 条数据在 0/25/50/75/100% 位置均无正向 gap，双主题、多尺寸和 resize 均通过。真实 Playnite 生产宿主逐页截图仍未补齐，Demo-first 总迁移仍未完成。

## 2026-08-21 UI-282 当前事实：首页小屏与 Demo 风险/比较结构已收口

- 当前 `OverviewView.xaml` 的紧凑堆叠布局中，`OverviewActivityColumn` 使用内层 Flow 第 3、4 行；`OverviewSecondaryScrollViewer` 必须在专用第 5 行，不能回到第 4 行，否则风险卡会与全局活动发生空间叠加。首页根 ScrollViewer 仍是唯一页面级纵向滚动面。
- `OverviewProtectionPreviewItems` 是 Demo 风格的多选 `ListBox`，卡片只显示状态点、游戏、状态 Chip 和原因，不得恢复 Checkbox、逐项“查看”按钮、第二个保护明细卡或安全提示子卡。卡片选中通过 `OnProtectionSelectionChanged` 转发到真实 `OpenProtectionItemCommand`；底部 `OpenProtectionGamesCommand`/`ApplyRecommendedProtectionCommand`、确认和当前快照安全语义不变。
- `AttentionFindings` 必须使用 Demo 的紧凑分隔行，右侧绑定真实 `SuggestedAction` 文字；不要恢复“查看原因”按钮，也不要添加不在 Demo 中的自定义空状态段落。空集合时保留标题和“打开维护中心”真实入口即可。
- `SaveCenterView.xaml` 的“比较与保留”页必须由 `SaveComparePageScrollViewer` 承载 `MinWidth=880` 的横向画布，左右两张 `GscReadingCardStyle` 对等卡片不在窄宽改为上下堆叠；左侧绑定 `LastBackupDiff` 的三类计数/文件清单，右侧绑定 `RetentionSummary`、`LastRetentionPreview.KeepBackupIds` 和 `DeleteCandidateIds`，并保留二次确认安全说明。
- UI-282 证据：`artifacts/gsc-b/ui282-demo-structure-v4` 构建与测试通过；`artifacts/ui-qa/ui282-demo-structure-v2/render-qa-report.txt` 为 `render-qa OK`，首页 1040/1100 小屏的活动与风险坐标不重叠，比较页横向滚动指标正常；`validate-source.py` 0 error，WPF UI 0 error、20 warnings、164 info。真实 Playnite Dashboard 逐页宿主截图仍未补齐，Demo-first 总迁移仍未完成。

## 2026-08-21 UI-281 当前事实：首页风险/关注区与共享按钮几何已修复

- `Themes/WpfUiProduction.xaml` 的 `GscWpfUiButton` 必须让状态层和 `ContentPresenter` 位于带 `Padding` 的同一个按钮外壳内；之前并列的空白 Border 不参与内容测量，是全局中文按钮贴边/溢出的根因。不要在页面里复制按钮模板来绕过它。
- `OverviewView.xaml` 的重复摘要只能隐藏摘要图标、标题和说明，不能折叠包住 `OverviewProtectionDetails`；真实 `RecentProtection.Items`、选择框、状态点/气泡、逐项查看和批量保护命令必须保持可见。首页最近任务/全局活动不使用 `IsMouseOver` 视觉覆盖，选中态和键盘焦点语义保留。
- `AttentionFindings` 仍是 Dashboard 的真实 Finding 集合；需关注事项使用紧凑 Demo 行，有数据时显示真实标题/游戏/动作，无数据时仅显示绑定驱动的空状态，不得添加 Mock 条目。
- `SaveCenterView.xaml` 的比较与保留页使用两个 `*` 对等卡片，动作按钮放在各自标题行；`ApplyResponsiveLayout` 只在窄宽时折叠到单列。备份策略、云端上传设置中的标签/Chip/Toggle/输入框/ComboBox 使用显式 `*` + `Auto` 列，避免“启用备份策略”和“重要游戏 · 严格”错位。
- UI-281 证据：`artifacts/gsc-b/ui281-button-risk-v4` 构建/测试通过；`artifacts/ui-qa/ui281-button-risk-v3/render-qa-report.txt` 为 `render-qa OK`，风险保护列表与关注列表的 ScrollViewer 均恢复非零视口，比较与保留/备份策略截图已抽查。真实 Playnite Dashboard 宿主证据仍未补齐，Demo-first 总迁移仍未完成。

## 2026-08-21 UI-280 当前事实：首页已修正 Demo 密度与选中态

- `OverviewView.xaml` 的 `OverviewStatStrip` 使用 11 列交替布局，统计卡和分隔线不再占用同一列；六个真实 `Snapshot` 指标、比例进度条和统计卡悬停动效保持。
- 首页最近任务、全局活动、风险标题/正文和需关注事项使用本地 Demo 字体别名；状态气泡使用 `GscRedesignTableStatusPill`，共享 ToolTip 使用 UI 字体链、12 DIP 字阶、`Padding=10,7`、`VerticalOffset=4` 和受控最大宽度。
- 首页风险区不再使用额外的 `Expander` 或嵌套生产子卡，`OverviewProtectionDetails` 直接承载真实 `RecentProtection.Items`；复选框的 `IsSelected`、逐项 `OpenProtectionItemCommand`、批量 `OpenProtectionGamesCommand`/`ApplyRecommendedProtectionCommand`、确认和当前快照安全语义必须继续保留。保护项为两行紧凑卡，列表仍在 `OverviewProtectionItemsScrollViewer` 内滚动。
- 需关注事项使用 18 DIP 小图标标记和透明紧凑操作；不要恢复 34 DIP 大图标块。共享 `GscRoundedDataGridRowTemplate` 对隐式 `DataGridRow`、`GscStableDataGridRow` 以及 Media 行提供 Trainer 风格的 Accent 填充、Accent 描边和 14 DIP 圆角选中态；`SelectiveScrollingGrid`、DetailsPresenter、Recycling 和列宽/排序行为保持。
- Dashboard 兼容表格的本地 DataGridRow 模板也补齐相同的选中圆角；当前游戏选框、生产滚动条系统、真实命令/绑定和项目 Tab chrome 未改动。
- UI-280 证据：`artifacts/gsc-b/ui-overview-home-v4` 构建/测试通过（Playnite 272 通过、60 跳过）；`artifacts/ui-qa/ui-overview-home-v2/render-qa-report.txt` 为 `render-qa OK`；WPF 静态校验 0 error、19 warnings、164 info。真实 Playnite 逐页嵌入截图仍未补齐，不能把本阶段离屏证据写成总迁移完成。

## 2026-08-21 UI-279 当前事实：Trainer 窄宽导入工具栏改为可用重排

- `TrainerCenterView.xaml` 的“当前游戏工具”标题区现在把导入工具栏、拖放提示和标题分成可重排的独立行；`ApplyResponsiveLayout` 在 `<980 DIP` 时把四个真实导入按钮移到标题下方，避免约 744 DIP 工作区裁掉最右侧按钮。
- 不得通过隐藏“导入修改器”“导入目录”“导入 CT”“+ 添加启动项”中的任何入口来解决窄宽问题；四个 Command、导入确认、工具列表/Inspector、ScrollViewer 和 Recycling 虚拟化均保持。
- UI-279 证据：`artifacts/gsc-b/ui279-trainer-toolbar-v1` 构建/测试通过；`artifacts/ui-qa/ui279-trainer-toolbar-v1/render-qa-report.txt` 为 `render-qa OK`，浅/深色 Trainer 1040×700 截图按钮均可见；`artifacts/ui-audit-ui279-trainer-toolbar-v1/AUDIT_SUMMARY.md` 为 Fidelity 0、HIGH 0、失败路由 0。真实 Playnite Dashboard 宿主证据仍未补齐。

## 2026-08-21 UI-278 当前事实：RenderHarness 使用主题化宿主画布

- RenderHarness 之前在页面宿主 Grid 中写死 `#181E2B`，导致强制浅色 QA 将 Demo 浅色色板页面错误放在深色画布上；这不是生产 Trainer 页面本身的真实主题关系。
- `tests/GameSaveCenter.RenderHarness/Program.cs` 的页面宿主现在统一通过 `CreateHarnessBackground(view)` 读取当前页面的 `GscBackdropBrush`；没有应用主题的历史探针保留原深色回退，不能删掉回退以改变既有探针语义。
- UI-278 证据：`artifacts/ui-qa/ui278-themed-host-v1/render-qa-report.txt` 为 `render-qa OK`，Light/Dark 七页、多尺寸、滚动和 resize 均通过；浅色 Trainer 1040×700 已确认顶部导入区可读。真实 Playnite Dashboard 宿主证据仍未补齐。

## 2026-08-21 UI-277 当前事实：共享折叠栏 Header 具有主题表面

- `DesignTokens.xaml` 的 `GscDisclosureCardExpander` 现在以 `GscControlFillBrush` 为默认背景，并把 `Background`/`BorderBrush` 传到 Header ToggleButton 的 `HeaderChrome`；Task 窄宽“更多筛选”不再在浅色主题深色画布上隐去标题。
- 不要把折叠栏改回透明 Header 或在 Task 页复制局部模板；整行命中、`TaskMoreFiltersExpander` 的 Visibility/响应式切换、Chevron 150ms 动效和真实 `TaskGameFilter` 绑定由共享资源继续负责。
- UI-277 证据：`artifacts/gsc-b/ui-277-disclosure-surface-v1` Release 0 warning/0 error、Core 59/59、Worker 191/191、Playnite 266 通过/62 跳过/0 失败；`artifacts/ui-qa/ui277-disclosure-surface-v1/render-qa-report.txt` 为 `render-qa OK`，Task 1040×700 双主题截图确认标题可见。真实 Playnite Dashboard 宿主证据仍未补齐。

## 2026-08-21 UI-276 当前事实：媒体当前页窄宽操作区已拆分

- `MediaCenterView.xaml` 的 `MediaCurrentActionRow` 现在是共享的响应式操作容器：宽屏提示和三个批量操作同一行；窄屏提示独占第一行，批量操作与 `MediaCompactDetailsButton` 分列第二行，不能把两组按钮重新放回同一个 Grid 单元格。
- `MediaCenterView.xaml.cs` 只在 `ApplyResponsiveLayout` 中切换操作行的行列位置；媒体 `ListBox` 的 SelectionMode、Recycling、异步缩略图、Inspector 抽屉和 `MediaCompactDetailsButton` 的展开/收起状态保持不变。
- UI-276 证据：`artifacts/gsc-b/ui-276-media-actions-v1` Release 0 warning/0 error、Core 59/59、Worker 191/191、Playnite 266 通过/62 跳过/0 失败；`artifacts/ui-qa/ui276-media-actions-v1/render-qa-report.txt` 为 `render-qa OK`，媒体 1040×700 双主题截图确认操作区不再重叠。真实 Playnite Dashboard 宿主证据仍未补齐。

## 2026-08-21 UI-273 当前事实：共享按钮与开关状态对齐 Demo

- `Themes/WpfUiProduction.xaml` 的共享 `GscWpfUiButton` 现在有 Demo `LabBtn` 对应的 `HoverOverlay`/`PressedOverlay` 状态层和透明度动效；主按钮继续使用生产 Demo 核心色板，覆盖层 `IsHitTestVisible=False`，不得在页面局部复制按钮状态模板。
- 共享 `GscWpfUiToggleSwitch` 使用 40×23 DIP 轨道、17 DIP 滑块、46 DIP 内容起始列和 140ms `RenderTransform.(TranslateTransform.X)` 位移动效；真实 `Content`、设置绑定、键盘焦点、禁用态和项目页面滚动保持不变。
- UI-273 证据：`artifacts/gsc-b/ui-273-shared-button-toggle-v1` 构建通过；`artifacts/ui-qa/ui273-shared-button-toggle-v1/render-qa-report.txt` 为 `render-qa OK`，七页双主题、多尺寸、滚动和 resize 均通过。真实 Playnite Dashboard 宿主证据仍未补齐。

## 2026-08-21 UI-274 当前事实：共享输入框与下拉选项对齐 Demo

- 生产 `GscWpfUiTextBoxTemplate` 的键盘焦点现在同时切换 `GscControlFocusFillBrush` 与 Accent 边框；普通主题由 `ApplyDemoCoreResources` 使用 Demo `FieldFocusFillBrush` 注入，高对比度由 `ApplyWpfUiResources` 提供自适应 fallback，验证错误仍保持错误填充/边框。
- 隐式 `ComboBoxItem` 现在使用 UI 字体链、`GscBodyFontSize` 和 Hand 光标；悬停/选中使用 `GscAccentTintBrush`，选中项 Medium 字重。不要修改 Popup 的真实滚动、键盘导航、最大高度或选择绑定来追求视觉一致。
- UI-274 证据：`artifacts/gsc-b/ui-274-input-combo-v1` 构建通过；`artifacts/ui-qa/ui274-input-combo-v1/render-qa-report.txt` 为 `render-qa OK`，七页双主题、多尺寸、滚动和 resize 均通过。真实 Playnite Dashboard 宿主证据仍未补齐。

## 2026-08-21 UI-275 当前事实：共享滑杆几何对齐 Demo

- 唯一使用点 `Settings/GameSaveCenterSettingsView.xaml` 的 `GlassStrengthSlider` 继续使用共享 `GscSlider`；模板现在为 22 DIP 高、4 DIP 轨道、18 DIP 滑块，生产主题自适应的 Thumb 阴影/悬停/拖动状态和真实 `GlassEffectStrength` 双向绑定不变。
- 不要为 Settings 另写滑杆模板，也不要因为收紧控件几何而移除页面 ScrollViewer、键盘焦点或值变化事件；低高度视口继续通过现有页面滚动到达下方控件。
- UI-275 证据：`artifacts/gsc-b/ui-275-slider-v1` 构建通过；`artifacts/ui-qa/ui275-slider-v1/render-qa-report.txt` 为 `render-qa OK`，七页双主题、多尺寸、滚动和 resize 均通过。真实 Playnite Dashboard 宿主证据仍未补齐。

## 2026-08-20 当前总规则：Demo-first 覆盖旧视觉优先级

- 后续所有页面迁移以 `GameSaveCenter.AcrylicFork/src/GameSaveCenter.Playnite/Design/DesignShellView.xaml`、`Pages/*.xaml`、`DesignTokens.xaml`、`DesignColorsLight.xaml`、`DesignColorsDark.xaml` 和 `DesignControls.xaml` 为唯一主要视觉基准；Demo 与旧生产页面、UiLab、历史计划或通用 Apple-inspired 建议冲突时，以 Demo 的整体结构、层级、空间、字体、颜色和控件为准。
- `wpf-apple-desktop-ui` 不再是视觉与实现路线的优先约束，只作为 WPF 质量检查依据，继续检查真实 Binding/Command、异步错误/取消/安全语义、虚拟化、键盘/UI Automation、可访问性、主题/DPI 和 Playnite 兼容性。
- 当前游戏选框、生产滚动条系统、真实运行时数据和 Demo 未覆盖但目标文件明确要求保留的功能继续保留；Demo Mock 数据、演示色板、窗口按钮和演示行为不得接入生产。
- 本段覆盖早期“当前生产 main > Demo”或“技能优先”的视觉排序；旧条目只用于历史追溯，不得阻止 Demo-first 的新页面迁移。

## 2026-08-21 UI-272 当前事实：修改器中心恢复项目 Tab chrome

- `TrainerCenterView.xaml` 已回滚 `a03accf` 引入的 `TrainerSegmentTabs` + `LabSegmented` 外层分段栏，恢复项目原有 `TrainerTabControl` / `TrainerTabItem`，其样式基于 `GscRedesignWorkspaceTabControl` / `GscRedesignWorkspaceTabItem`。
- 四个真实页面面板重新由 `TabItem` 承载；不要再把修改器中心外层 Tab 改成 Demo 的 `LabSegmented`。Settings 左侧 `SettingsSectionTabs` 仍保留，因为它是目标 Demo 要求的分类栏信息架构而非项目工作区 Tab chrome。
- `ImportTrainerCommand`、工具目录/CT/启动项导入、FLiNG 搜索与版本下载、工具/发行 Inspector、回收虚拟化、ScrollViewer 和 `ApplyResponsiveLayout` 均保持；仅删除 Demo 分段切换的可见性代码。
- 当前阶段证据：`artifacts/gsc-b/ui-272-trainer-tab-rollback-v2` 的 XAML 18/18、Release 0 warning/0 error、Core 59/59、Worker 191/191、Playnite 262 通过/62 跳过/0 失败；source/WPF/diff 门禁通过；`artifacts/ui-qa/ui272-trainer-tab-rollback-v1/render-qa-report.txt` 为 `render-qa OK`，覆盖七页双主题、多尺寸、Tab、滚动和 resize。该离屏结果不替代真实 Playnite 宿主证据。

## 2026-08-21 UI-271 真实 Playnite 宿主审计边界复核

- Release `0.6.70+c6cb235ef446fbe6e0c12566a7920c92e2135af8` 已通过 `scripts/real-host-audit.ps1` 安装并启动 Playnite；`artifacts/ui-host-audit-ui271/summary.json` 明确记录 `EmbeddedSettingsCaptured=true`、`ControlledDashboardCaptured=true`、`EmbeddedDashboardCaptured=false`、`ProductionVisualSourceOfTruthAvailable=false`。
- Settings 的 `settings/embedded-current/viewport/settings.png`、视觉树和资源快照来自真实 `EmbeddedPlaynite` 宿主，可作为 Settings 嵌入证据。Dashboard 自动 UI Automation 仍未定位左侧 GameSaveCenter 入口，`gates/REAL_EMBEDDED_DASHBOARD_NOT_CAPTURED.json` 的 HIGH 门禁有效；受控 Dashboard 图像只能作为布局辅助，不能冒充生产视觉真值。
- Computer Use 观察到 Playnite 主窗口为 `EmptyWindowAutomationPeer`；未绕过该限制，也未停止另一个扩展目录中的旧 Worker。后续必须在用户可见、可交互的 Playnite 窗口中打开 GameSaveCenter 后重跑审计，才可补齐七页 Dashboard 的像素、DPI、键盘焦点、命中区域和真实操作证据。
- 审计内 Release 基线为 XAML 18/18、Release 0 warning/0 error、Core 59/59、Worker 191/191、Playnite 262 通过/62 跳过/0 失败；这不改变总 Demo-first 目标未完成的判断。

## 2026-08-21 UI-271 当前事实：共享表格使用 Demo 正文与表头字阶

- `Themes/DesignTokens.xaml` 当前提供 `GscBodyFontSize=13.5`、`GscCaptionFontSize=12`，分别对应 Demo `SizeBody` 和 `SizeCaption`；生产隐式 `DataGrid` 使用 UI 字体链和正文令牌，`DataGridColumnHeader` 使用 UI 字体链、表头令牌和 Medium 字重。
- 这只统一表格文本密度，不改变 `GscTableRowHeight=44`、`GscTableHeaderHeight=36`、排序箭头、列宽调整、选中态、内部滚动、Recycling 虚拟化或真实表格绑定。
- 当前证据：`artifacts/gsc-b/ui-271-table-typography-v1` Release 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 262 通过/62 跳过；source/WPF/diff 门禁通过；`artifacts/ui-qa/ui271-table-typography-v1/render-qa-report.txt` 双主题、多尺寸、滚动和 resize 均为 `OK`。真实 Playnite 宿主字号、DPI、键盘和列宽拖动验收仍未收口。

## 2026-08-21 UI-270 当前事实：共享折叠栏箭头动效对齐 Demo

- `Themes/DesignTokens.xaml` 的 `GscDisclosureCardExpander` 现在在 `IsChecked` 进入/离开时以 150ms 将 Chevron 在 `-90°` 与 `0°` 间旋转，匹配 Demo `LabDisclosure`；`GscDisclosureCard` 继续作为统一别名。
- 这只改变共享控件的视觉状态过渡，保留整行点击、键盘焦点、内容显隐、真实 Expander 绑定和页面滚动；没有改变业务命令、数据、虚拟化或项目 ScrollBar。
- 当前证据：`artifacts/gsc-b/ui-270-disclosure-animation-v1` Release 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 261 通过/62 跳过；source/WPF/diff 门禁通过；`artifacts/ui-qa/ui270-disclosure-animation-v1/render-qa-report.txt` 双主题、多尺寸、滚动和 resize 均为 `OK`。真实 Playnite 宿主的动效时间、键盘焦点和逐页视觉验收仍未收口。

## 2026-08-21 UI-269 当前事实：Demo 核心主题色不再被宿主中性刷覆盖

- `AdaptiveThemePaletteFactory.ApplyDemoCoreResources` 是生产 Shell 与 Settings 共用的核心色板入口，固定 Demo 的浅色/深色画布渐变、卡片、侧栏、顶栏、输入框、文字层级、表格、分段控件、滚动条、遮罩和语义状态色；宿主 Accent/focus 仍保留给非核心交互。
- 高对比度通过提前返回继续使用系统自适应路径；普通主题不再由 Playnite 背景/正文中性刷重写已迁移页面的核心表面。生产 Tab chrome、当前游戏选框、滚动条行为、虚拟化、命令/Binding 和真实业务数据没有改变。
- 当前证据：`artifacts/gsc-b/ui-269-demo-palette-v2` Release 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 260 通过/62 跳过；source/WPF/diff 门禁通过；`artifacts/ui-qa/ui269-demo-palette-v1/render-qa-report.txt` 双主题、多尺寸、滚动和 resize 均为 `OK`。截图仍不能替代可识别 Playnite 宿主的逐页像素、DPI、键盘、主题和真实操作验收。

## 2026-08-21 UI-268 当前事实：标题字体接入独立 Display 字阶

- `src/GameSaveCenter.Playnite/Themes/DesignTokens.xaml` 当前同时提供 `GscUiFontFamily`（`Segoe UI Variable Text, Segoe UI, Microsoft YaHei UI`）、`GscDisplayFontFamily`（`Segoe UI Variable Display, Segoe UI, Microsoft YaHei UI`）和 `GscCodeFontFamily`（`Cascadia Mono, Consolas, Microsoft YaHei UI`）。
- `GscRedesignHeroTitle`、`GscRedesignFeedbackDialogTitle`、`GscPageTitleStyle`、`AcrylicProductionShellView` 的 `PageTitleText` 和 `DashboardView` 的回退标题使用 Display 字阶；`GscRedesignSectionTitle` 继续使用继承的正文族，保持 Demo `LabTitle`/`LabSection` 的层级关系。
- 标题字体改动没有触及生产 Tab chrome、当前游戏选框、滚动条系统、虚拟化、真实命令/Binding 或业务数据；回归测试保护共享令牌和两个生产标题入口。
- 当前证据：Release 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 259 通过/62 跳过；source/WPF/diff 门禁通过；`artifacts/ui-qa/ui268-display-font-v1/render-qa-report.txt` 双主题、多尺寸、滚动和 resize 均为 `OK`。总目标仍需继续完成 Demo 七页结构/视觉逐项核对及可识别 Playnite 宿主的像素、DPI、键盘和真实操作证据。

## 2026-08-21 UI-267 当前事实：工作区表格测量与几何审计已收口

- `MediaCenterView.xaml` 的当前游戏媒体搜索操作区保持至少 `300 DIP`，搜索输入列保持至少 `160 DIP`；真实 `MediaSearchText`、媒体类型筛选、媒体卡片、预览 Inspector 和批量操作没有改变。
- `SaveCenterView.xaml.cs` 在工作区宽度低于 `1240 DIP` 时启用历史表紧凑列宽；这是为了在标准宿主的 Inspector 并列布局中保持状态列可达，不是删除列或隐藏操作。DataGrid 的 `Auto` 横向滚动、列宽拖动、排序和 Recycling 虚拟化继续由共享生产样式负责。
- 回归断言 `SharedWorkspaceBreakpointsKeepSearchAndHistoryEssentialsReadable` 保护上述两个空间契约；生产 Tab chrome、当前游戏选框、页面滚动、真实命令/绑定和异步安全语义均未改动。
- UI 审计已按主控件直接所属 Grid 行计算纵向填充，允许 Overview 的有限本地虚拟视口，排除列表内部媒体卡片误判工具栏；列宽超过视口但 `DG_ScrollViewer` 有真实 Auto 横向滚动时记为 `EXPECTED_HORIZONTAL_SCROLL`。最新 `artifacts/ui-audit-ui267-fix3` 为 Fidelity 0、HIGH 0、MEDIUM 0、失败路由 0。
- 当前验证证据：`artifacts/gsc-b/ui-audit-layout-fix-v1` Release 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 259 通过/62 跳过；`artifacts/ui-qa/ui267-layout-audit-fix-v1` 为 `render-qa OK`。WPF 静态检查保留 0 error、19 warnings、161 info。真实 Playnite 宿主的逐页像素、DPI、键盘焦点与真实操作仍是总目标的未收口边界。

## 2026-08-20 UI-266 当前事实：存档维护指标统一数值优先阅读

- 存档页“比较与保留”中的新增文件、修改文件、删除文件指标现在统一为“数值 → 标签”；真实 `LastBackupDiff` 绑定、差异文件列表、比较命令和只读保留预览均保持不变。
- 维护页的保留、容量、趋势、保留模拟、保护状态和本地镜像指标统一为“数值 → 标签 → 补充说明”，绑定仍来自真实运行时状态，不得用 Demo 示例数字替换。
- 本阶段没有改变生产 Tab chrome、页面滚动、DataGrid/列表虚拟化、Inspector、命令或安全语义；后续新增指标卡继续优先检查 Demo 的数值优先阅读节奏。
- 当前证据：`artifacts/gsc-b/metrics-rhythm-v1` Release 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 258 通过/62 跳过；XAML/source/WPF/diff 门禁通过；`artifacts/ui-qa/metrics-rhythm-v1` 双主题、多尺寸、滚动和 resize `render-qa OK`。截图仍属于离屏证据，不能替代可识别 Playnite 宿主验收。

## 2026-08-20 UI-265 当前事实：维护诊断概览先显示环境健康

- `MaintenanceView.xaml` 的诊断概览现在按 Demo 顺序先显示 `DiagnosticHealthCard`/`DiagnosticHealthPanel`，再显示 `EnvironmentCheckCard` 与 `MaintenanceDiagnosticsActionCard`；不要把六项健康状态重新藏回“更多维护操作”展开区。
- 健康卡仍来自真实运行时绑定：Worker/Ludusavi/Rclone 状态、数据与媒体目录、待归类媒体数和设备比较数；环境检查、诊断复制/导出、自检、索引重建、任务协调、元数据灾备、路径迁移与安全模式命令没有改变。
- `DiagnosticHealthPanel` 的响应式列数仍由 `ApplyResponsiveLayout` 控制为宽屏 4 列、中等 2 列、窄屏 1 列；生产 Tab chrome 是用户明确例外，不迁移为 Demo 的 segmented UI。
- 当前证据：Release 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 258 通过/62 跳过；XAML/source/WPF/diff 门禁通过；`artifacts/ui-qa/maintenance-health-order-v1` 的双主题、多尺寸、滚动和 resize `render-qa OK`。截图仍属于离屏证据，不能替代可识别 Playnite 宿主的逐页像素验收。

## 2026-08-20 UI-264 当前事实：首页统计条已恢复 Demo 连续结构

- `OverviewView.xaml` 的 `OverviewStatStrip` 当前是一个 `GscRedesignSectionCard` 连续统计条，六个等宽指标使用五条 `GscTableDividerBrush` 分隔；数字在上、标签在下，不要恢复六张带间隙的独立 metric card 或旧的 `UniformGrid.Columns` 响应式换列。
- 六项显示继续绑定真实 `Snapshot.ManagedGames`、`Snapshot.MatchedGames`、`Snapshot.RunningGames`、`Snapshot.WarningGames`、`Snapshot.PendingCloudTasks`、`Snapshot.UnassignedMediaCount`；匹配/风险进度条与 `ManagedGames == 0` 时隐藏的防护仍有效，健康/注意/风险/未知明细也继续来自 Snapshot。
- `OverviewStatStrip` 的命名 XAML 元素已从 `UniformGrid` 调整为 `Border`，`ApplyResponsiveWidth` 不再调整统计列数；今日工作台、当前游戏选框、立即备份/全部备份、活动列表虚拟化、页面滚动和 hover render-only gate 均未迁移。
- 当前证据：Release 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 258 通过/62 跳过；XAML/source/WPF/diff 门禁通过；`artifacts/ui-qa/overview-summary-strip-v1` 的双主题、多尺寸、滚动和 resize `render-qa OK`。截图仍属于离屏证据，不能替代可识别 Playnite 宿主的逐页像素验收。

## 2026-08-20 UI-263 当前事实：任务统计条已恢复 Demo 连续结构

- `TaskCenterView.xaml` 顶部 `TaskSummaryPanel` 当前是一个 `GscRedesignSectionCard` 连续统计条，四个等宽指标之间使用 `GscTableDividerBrush` 分隔；不要恢复旧的可变列数 `UniformGrid` 或独立 metric card。
- 四项真实统计继续绑定 `Tasks.Count`、`RunningTaskCount`、`RetryableTaskCount`、`CompletedTaskCount`；运行中使用 `GscAccentBrush`、需要重试使用 `GscWarningBrush`、今日完成使用 `GscSuccessBrush`，与 Demo 的状态层级一致。
- `TaskSummaryPanelElement` 已从 `UniformGrid` 调整为 `Border`；`ApplyResponsiveLayout` 不再调整摘要列数，但仍负责任务表 236 DIP 最小视口、筛选重排、Inspector 堆叠与详情高度。
- 当前证据：Release 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 258 通过/62 跳过；XAML/source/WPF/diff 门禁通过；`artifacts/ui-qa/task-summary-strip-v1` 的双主题、多尺寸、滚动和 resize `render-qa OK`。截图仍属于离屏证据，不能替代可识别 Playnite 宿主的逐页像素验收。

## 2026-08-20 UI-262 当前事实：媒体统计条已恢复 Demo 连续结构

- `MediaCenterView.xaml` 顶部 `MediaSummaryPanel` 当前是一个 `GscRedesignSectionCard` 连续统计条，四个等宽指标之间使用 `GscTableDividerBrush` 分隔；不要恢复为四张带间隙的 `GscRedesignMetricBorder` 独立卡片。
- 四组显示继续绑定真实值：总媒体/截图/录像来自 `MediaSummary`，占用来自 `MediaSummary.TotalSizeDisplay`，收藏来自 `MediaSummary.FavoriteCount`，待归类来自 `Snapshot.UnassignedMediaCount`；Demo 示例数字没有进入生产。
- `MediaSummaryPanelElement` 已从 `UniformGrid` 调整为 `Border`，`ApplyResponsiveLayout` 不再设置不存在的 `Columns`；媒体来源字段仍独立使用 `UniformGrid` 的响应式列数。
- 当前证据：Release 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 258 通过/62 跳过；XAML/source/WPF/diff 门禁通过；`artifacts/ui-qa/media-summary-strip-v1` 的双主题、多尺寸、滚动和 resize `render-qa OK`。截图仍属于离屏证据，不能替代可识别 Playnite 宿主的逐页像素验收。

## 2026-08-20 UI-253 当前事实：修改器中心已切换为 Demo 分段面板

- `TrainerCenterView.xaml` 当前使用 `TrainerSegmentTabs` + `LabSegmented`，通过 `PanelTools`、`PanelImport`、`PanelCatalog`、`PanelReleases` 四个命名面板承载 Demo 页面结构；不要恢复旧 `TabControl/TabItem` 外壳作为主要导航。
- 分段切换只控制 `Visibility`，真实入口继续存在：`ImportTrainerCommand`、`ImportToolFolderCommand`、`ImportCheatTableCommand`、`ImportCustomLaunchItemCommand`、`ConfirmGameToolImportCommand`、`SearchTrainerCatalogCommand`、`LoadTrainerReleasesCommand` 和 `DownloadTrainerCommand`。
- `TrainerToolsList`、目录结果和发行版本列表继续使用项目现有回收虚拟化和 ScrollViewer 交互；工具设置 Inspector、窄宽详情抽屉和响应式布局仍由 `ApplyResponsiveLayout` 管理。`LabSegmented` 四项导航属于有限标签列表，源码门禁不得要求它承担大列表虚拟化契约。
- XAML 构造期分段事件必须保留面板字段空保护；WPF 页面初始化时 `SelectedIndex` 可能早于后续命名面板生成。
- 当前证据：Release 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 252/252 通过、61 跳过；XAML/source/diff/WPF 静态门禁通过；`artifacts/ui-qa/trainer-segmented-final` 的 RenderHarness 双主题、多尺寸、resize 和四分段探针通过。仍不能把离屏 PNG 作为 Playnite 宿主逐页像素验收。

## 2026-08-20 UI-251 当前事实：存档规则卡与诊断概览按 Demo 第三轮收口

- 存档中心当前规则卡在常见工作区宽度下横向排列“当前存档规则 / 游戏名 / 状态 / 立即扫描 / 重新校验 / 刷新详情”；低于 700 DIP 才堆叠操作，避免正常宿主中规则信息和按钮被拉成多行。
- 维护中心诊断概览以 AcrylicFork 生产页面为结构基线：环境检查卡在前，诊断操作卡在后，健康指标位于“更多维护操作”内；真实检查、修复、导出、取消命令与 Binding 未移除。
- 共享生产表格排序箭头采用 Demo 的 14 DIP 表头保留列和完整路径几何；列宽拖拽、排序、固定行高和虚拟化契约继续有效。
- 设置页与生产壳体/工作区统一继承 `GscUiFontFamily`；`DashboardView` 是安全回退页，已有同一字体入口。
- 当前证据：Playnite 测试 251 通过、61 跳过、0 失败；`validate-source.py`、WPF UI 静态校验（0 error）、`git diff --check` 和 RenderHarness Release `render-qa OK` 均通过。仍无新的可识别 Playnite 生产宿主像素证据，不能把离屏结果写成真实宿主 1:1 验收。
- 已清理两条旧 AcrylicFork 基线门禁：它们曾要求首页没有“今日工作台”、媒体页必须存在已废弃的 `MediaSummaryTabStrip`/`MediaTabStrip`，与当前 Demo 结构和用户要求相反；现改为保护当前工作台、完整 TabControl、媒体 Inspector 和可预览入口。

## 2026-08-20 UI-252 当前事实：存档历史页补回 Demo 操作卡

- 存档中心默认“历史版本”页现在在表格上方显示 `SaveHistorySummaryCard`，包含真实 `Backups.Count` 版本数、当前规则/健康状态摘要以及“立即扫描 / 重新校验 / 刷新详情”三个真实命令入口；不再只有表格和选中后才出现的详情按钮。
- 摘要卡在 700 DIP 以下把操作区移到第二行，正常工作区保持标题、版本数与操作横向排列；历史表仍保持 DataGrid 的列宽拖动、排序、Item scrolling、行/列虚拟化和项目现有滚动条。
- 当前证据：XAML structural validation 18 个文件通过；Release 构建 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 251 通过/61 跳过/0 失败；RenderHarness Release `render-qa OK`，双主题、多尺寸和 resize transition 均通过。仍无新增可识别 Playnite 生产宿主像素证据，不能把离屏结果写成真机 1:1 验收。

## 2026-08-20 UI-250 当前事实：媒体页已恢复 AcrylicFork Tab 结构

- 媒体中心以 AcrylicFork 实际 `MediaCenterView.xaml` 为结构基线：摘要四卡在顶部，工作区 `TabControl` 负责自己的标题栏和内容，不能再恢复成独立 Tab 标题 + `MediaTabContentHost` 的拼接结构。
- 待归类 DataGrid 外部保留 `MediaInboxInspectorScrollViewer`，绑定 `SelectedInboxMedia`，侧栏包含截图/录像预览、来源/原因、目标游戏、`AssignInboxMediaCommand` 与 `IgnoreInboxMediaCommand`。常见 744 DIP 内容宽度下 Inspector 保持侧栏，低于 700 DIP 才堆叠。
- 生产壳体及 Overview/Save/Media/Maintenance/Task/Trainer 根节点显式继承 `GscUiFontFamily`；图标仍使用明确的 Segoe MDL2 Assets，不要用页面根字体覆盖图标。
- UI-248 的“独立 MediaTabContentHost”只保留作历史记录，当前实现以 UI-250 为准。

## 2026-08-20 UI-249 当前事实：AcrylicFork 视觉迁移已重新启动

- 本轮最终视觉参考以 `D:\workplace\github\GameSaveCenter.AcrylicFork` 为准。旧的“不要恢复/不要替换”页面迁移限制已经失效；页面级结构、共享模板、Tab/分段导航和滚动模型可以按参考实现重构，但真实命令、Binding、数据契约、错误/取消/安全语义、虚拟化和 Playnite 兼容性仍必须保留。
- 首页已补回独立“今日工作台”操作区；首页最近任务项固定为 54 DIP、消息单行省略并提供 Tooltip，不能再让多行 DetailMessage 撑高单条记录。
- 存档中心生产壳体现在同时显示“立即备份”（`BackupSelectedCommand`）和“全部备份”（`BackupAllCommand`），前者只作用于当前游戏。
- 共享 `GscRedesignTableFrame`、DataGrid 行/表头样式和 `GscDisclosureCard` 已按 AcrylicFork Demo 的紧凑密度首轮收口；后续页面阶段必须继续检查有限 Grid 行、Inspector 和操作区是否发生隐藏堆叠。
- 当前证据：RenderHarness Release `render-qa OK`、source/WPF 校验和 `git diff --check` 通过；尚未获得可识别的 Playnite 生产宿主像素证据。

## 2026-08-19 UI-247 当前事实：任务搜索区必须是单一输入面

- 任务中心的“搜索任务…”必须作为 `TaskSearchTextBox` 内部提示，搜索图标也在同一输入区内；禁止恢复独立标签列，否则真实宿主会把输入框测量成窄条。
- 当前响应式下限为桌面 `420 DIP`、中等 `300 DIP`、紧凑 `180 DIP`；该逻辑不改变 `TaskSearchText` Binding、状态/类型筛选或刷新命令。
- 如果宿主仍出现“外部搜索任务… + 旁边空框”，优先判断为旧 DLL/旧安装目录/未重启旧 Playnite 进程，不能据此判断当前源码已经回退。

## 2026-08-19 UI-248 当前事实：媒体摘要卡片与 Tab 导航已分区

- `MediaSummaryTabStrip` 现在只承载四张独立等宽统计卡片；统计卡片不再与 Tab 导航共用一个外框。
- `MediaTabStrip` 单独承载“待归类 / 当前游戏媒体 / 来源规则”导航，`MediaTabContentHost` 位于下一行并占据剩余空间。
- 该结构保留真实媒体 Binding、详情 Inspector、项目自身 ScrollBar 和虚拟化，不迁移 Demo 顶部彩色主题按钮；离屏回归通过，仍需可识别的 Playnite 宿主像素截图完成最终验收。

## 2026-08-19 UI-246 历史事实：媒体标签材质基线

- 该阶段曾将媒体标签栏尝试调整为紫色强调材质；后续 UI-248 已按页面信息架构将统计卡片与 Tab 导航拆为两个独立区域，当前实现以 UI-248 为准。

## 2026-08-19 UI-245 当前事实：任务搜索输入区宽度

- 任务中心的搜索提示必须属于 `TaskSearchTextBox` 内部内容，不能再用独立标签占用筛选栏列。
- `TaskSearchBoxHost` 的响应式最小宽度为桌面 `420 DIP`、中等 `300 DIP`、紧凑 `180 DIP`；这是为真实 Playnite 宿主的测量差异设置的布局下限，不改变搜索 Binding 或筛选命令。
- Release 包已重新安装并由 Playnite 日志确认加载生产扩展 DLL；若 Playnite 未出现可识别窗口，后续仍需重新取得宿主截图后再判断视觉结果。

## 当前有效 UI 方向（2026-08-17）

- 用户已明确授权页面级、整页 UI 重构。页面布局、信息架构、导航结构、Tab/Segmented 方案、控件类型、ControlTemplate、共享样式和滚动实现都可以重新设计；不再把上一轮 UiLab/AcrylicFork 迁移中的“不要恢复/不要替换/明确不迁移”当作当前硬性禁令。
- 旧阶段条目继续保留用于追溯当时的迁移决策，但它们不应阻止新的页面方案。新设计默认继续保护真实命令、Binding、数据契约、错误/取消/安全语义、可访问性、列表性能和 Playnite 兼容性；如果设计需要改变这些内容，必须在当前任务中明确并验证，而不是因为历史实现而回避布局或控件重构。
- 本段是当前方向声明，优先于下方 2026-08-17 之前的页面迁移偏好；后续每个独立 UI 阶段都要更新本段对应的当前事实和 `docs/ai/WORKLOG.md`。

## 2026-08-19 UI-244 当前事实：首页风险明细与最近任务有限视口

- 首页最近任务外层和列表不再设置固定最小高度，数据较少时按实际行数收缩；虚拟化、真实任务 Binding 和页面滚动仍保留。
- 首页风险内容只保留一套可见摘要/明细结构；重复预览和重复操作行隐藏，底部操作按钮位于风险视口之外。
- `OverviewRiskViewport` 在紧凑堆叠布局上限为 520 DIP，在侧栏布局上限为 420 DIP；`OverviewProtectionItemsScrollViewer` 在紧凑布局上限为 420 DIP，在侧栏布局上限为 340 DIP。两者都使用项目现有 `GscPageScrollViewer`，超出内容只在对应内部区域滚动。
- 2026-08-19 在可识别的 Playnite 生产宿主中重新展开首页风险明细，确认明细卡片和“查看”操作完整可见，底部保护操作未被内部滚动区域截断。本事实只覆盖首页紧凑布局，不能替代其他页面的宿主对照验收。

## 2026-08-19 UI-240 当前事实：风险栏有限视口与维护 Inspector 按选中状态出现

- 首页“风险与提醒”使用外层 `OverviewRiskViewport` 的 `330 DIP` 有限视口；列表超出后在自身区域使用项目 `GscPageScrollViewer` 滚动，不能让 Dashboard 根页面无限增高。展开的保护明细仍是独立 `190 DIP` 视口。
- 维护中心发现问题/审计列表无选中项时释放右侧 Inspector，表格跨满可用宽度；只有用户点击行后才显示详情 Inspector。`DashboardViewModel` 刷新时仅恢复原先仍存在的选中项，不自动选择第一条。
- 2026-08-19 已在实际 Playnite 生产版重新安装并核对：首页风险栏保持有限高度；维护中心先显示全宽表格，点击问题后显示右侧详情。该次证据来自可识别的 Playnite 宿主窗口，不能与 Preview 入口或 RenderHarness 混淆。
- 同次一键 Release 验证：Core 59/59、Worker 191/191、Playnite 249 通过/61 跳过/0 失败；source/WPF 校验和安装版本核对均通过。

## 2026-08-19 UI-227 当前事实：媒体 Tab 条与生产按钮文本由共享样式控制

- `GscRedesignWorkspaceTabControl` 的外层 Tab 条必须使用 `TemplateBinding Background`，不能把 `GscControlFillBrush` 写死在模板中；媒体中心通过 `MediaTabControl.Background=GscGlassFillBrush` 使用玻璃材质，其他工作区继续沿用各自样式。
- `GscWpfUiButtonTextTemplate` 是生产文本按钮的统一边界：`TextAlignment=Center`、`NoWrap`、`CharacterEllipsis`。新增生产操作按钮优先使用已有 `GscWpfUi*Button` 样式，不要重新创建不受边界约束的局部 Button 模板。
- 这只证明源码与 RenderHarness 的布局契约；必须在 Playnite 生产 `GameSaveCenter` 入口重新安装并人工查看媒体中心，不能把 Preview 入口或离屏截图当成宿主验收。
- 2026-08-19 已完成生产扩展落盘安装验证（目标目录中的 `extension.yaml` 与 DLL 为 `0.6.70`），但本机屏幕控制两次返回了 Codex/其他窗口而非 Playnite 内容，自动化树为 `EmptyWindowAutomationPeer`；后续不得把该次结果写成 Playnite 视觉通过，需改用能确认窗口内容的宿主截图路径。

## 2026-08-19 UI-226 当前事实：维护表格需按工作区宽度压缩固定列

- 维护诊断和异常审计页面在右侧 Inspector 可见时，1040 DIP 工作区的主表格只有约 660 DIP，不能直接沿用宽屏的游戏/标题/详情/动作最小宽度。
- `MaintenanceView.ApplyFindingsColumnLayout` 现在在 1180 DIP 以下压缩非必要固定列，保留详情和建议处理的弹性列；1366 及以上恢复更宽的 Demo 比例。长文本单元格统一字符省略并提供 Tooltip。
- 这只是视口布局策略，不改变真实 Findings、SelectedFinding、Inspector 或命令；维护表格仍使用项目现有 DataGrid、虚拟化和滚动条。
- 当前验证：RenderHarness Release `render-qa OK`，source/WPF 校验和 `git diff --check` 通过。直接 `dotnet build --no-restore` 的 MSB4276 是本机 SDK 9.0.302 缺少 Workload locator 的环境限制，后续优先使用仓库脚本的禁用 Workload resolver 构建路径或补齐 SDK 环境。

## 2026-08-18 当前构建事实：可选目录未配置不应阻断健康检查

- `IntegrityCheckService.CheckDirectory` 对空的可选目录按未启用处理；只有用户配置了路径后，该路径不存在或不可写才报告 Warning。关键目录的空路径仍按原有关键错误语义处理。
- 一键安装遇到 `Healthy` 变 `Warning` 或 `Skipped` 变 `Warning` 时，先检查是否把可选 `GameToolsDirectory`/`DownloadDirectory` 当成缺陷；不要通过修改测试期望值掩盖默认安装语义。
- `scripts/build.ps1 -OutputRoot` 的隔离目录适合编译/Core/Worker 验证，但当前 Playnite 源码测试会从测试运行目录向上定位仓库；Playnite 测试必须在仓库原目录运行，不能把隔离目录失败写成代码回归。

## 2026-08-18 UI-223 当前事实：首页状态语义与真实宿主复核

- 生产首页最近任务的进度条不是所有任务的通用装饰：只有 `执行中`、`等待中`、`等待确认` 显示进度；成功、失败、已取消项只显示状态和时间，避免偏离 Demo 语义。
- 首页最近任务状态徽标、全局活动类型/结果徽标、风险提醒徽标统一使用固定边界内的水平/垂直居中和不换行；风险徽标保持足够最小宽度，不使用省略号截断“风险”等状态文字。
- 生产首页 v0.6.70.0 已重新安装到 Playnite 并实际打开复核；确认侧栏“设置”、最近任务状态布局、全局活动“维护/信息”徽标、风险徽标均来自真实生产宿主。电脑控制已在复核后释放。
- 媒体虚拟化缩略图使用 96px 有界解码宽度；这是性能约束，不要为了视觉放大恢复到 240px 的逐卡解码。
- 当前验证基线：source 校验通过，隔离 Release 构建通过，包生成/安装通过，WPF 校验 0 error；Worker 仍有 2 个集成测试失败，Render QA 有 2 个媒体 resize 恢复用例待修，不能写成全量验证通过。
- 真实宿主本阶段只对首页重点区域做了截图复核；其他页面仍需按页面逐一进入 Playnite 对照 Demo，最终交付必须区分“实际宿主复核”和“RenderHarness/源码验证”。

## 2026-08-18 UI-225 当前事实：首页风险列表必须使用有限视口

- 首页“风险与提醒”中，`AttentionFindings` 和展开的 `RecentProtection.Items` 都不能直接让右栏 `StackPanel` 无限测量；页面使用两个独立的 190 DIP `ScrollViewer`，分别命名为 `OverviewAttentionScrollViewer` 和 `OverviewProtectionItemsScrollViewer`。
- 两个内部滚动面均引用生产 `GscPageScrollViewer`，竖向 `Auto`、横向 `Disabled`；风险卡标题、恢复提示、摘要和“打开维护中心”等操作不在内部列表视口内，始终可见。
- `DashboardViewModel` 仍把首页风险原因限制为前 4 项，`RecentProtectionSummary` 仍把保护明细限制为前 6 项；XAML 的有限视口是防止未来数据契约放宽或展开内容增长时撑高首页的第二道布局保护，不改变业务计数和完整详情入口。
- 本阶段 RenderHarness 构建、双主题多尺寸及 resize 通过；定向 Playnite 测试运行器再次无输出挂起，不能把该次测试写成通过。真实 Playnite 嵌入像素仍需单独人工验收。

## 2026-08-18 UI-224 当前事实：首页徽标模板与 Demo 行密度

- `ChipBase` 模板必须把 `HorizontalContentAlignment`/`VerticalContentAlignment` 传给内部 `ContentPresenter`；仅给外层控件设置对齐属性不够。
- 首页最近任务的进度条只属于执行中、等待中和等待确认项；成功/失败/取消项使用独立状态和时间列。全局活动的类型/结果徽标与风险徽标使用共享 Chip，分别保持 64 DIP 最小宽度与 11 DIP 风险字号，避免“信息/成功”偏移和“风险”裁切。
- 本轮真实 Playnite 只复核首页上述区域；0.6.70 已安装成功且控制已释放。Media resize 两项 RenderHarness 失败、旧 Playnite UI 结构测试失败仍是未清理的验证债务，不得宣称全量 UI 完成。

## 2026-08-17 UI-222 当前事实：生产入口与 Preview 必须区分

- Playnite 当前可能同时加载两个独立扩展：`GameSaveCenter Preview`（0.6.71，Demo/预览）和生产 `GameSaveCenter`（0.6.70，`AcrylicProductionShellView`）。验证生产 UI 时必须从侧栏的 `GameSaveCenter` 进入，不能把 Preview 入口的画面当作生产页面。
- 生产 `DashboardView` 的可见层是 `AcrylicProductionShellView`，其 `PageHost` 承载真实 Overview/Save/Trainer/Media/Task/Maintenance 页面；旧 `DashboardDemoShell` 保留字段但必须保持 `Collapsed`。
- 生产头部标识使用“生产版”，不要恢复 Demo 的“外观预览”文字；Demo 的主题色只进入生产令牌，不迁移 Demo 色板控件。
- Real Host 审计在 150% DPI 下必须把 `TransformToAncestor` 的设备像素坐标规范化到根元素 DIP，并按 `GetRenderScale(window)` 保存受控窗口截图；否则会产生右侧按钮假溢出和截图被裁剪的假象。
- 当前验证基线：source/WPF 校验无 error、Release 无 warning/error、Playnite 303/303、RenderHarness `render-qa OK`；嵌入式 Dashboard 仍需用户真实点击 Playnite 侧栏才能取得像素真值。

## 2026-08-17 UI-221 AcrylicFork 整页视觉迁移收口

- 权威参考仍为 `D:\workplace\github\GameSaveCenter.AcrylicFork` @ `b09cba6`；本轮确认生产 7 页骨架已与其一致，并补齐 Media/Trainer/Save/Maintenance 分段导航右侧说明与真实计数/状态：待归类数、已绑定工具数、当前规则校验状态、安全模式/云端状态。
- 评估过独立 `AcrylicParity.xaml` 别名层，但独立字典的 `BasedOn` 无法解析父级合并字典里的 Gsc 样式，最终未引入；页面继续直接引用生产 Gsc 共享样式，避免解析风险。不要为了“Lab 键”重新创建该字典。
- 继续保留生产滚动条、圆角表头、DataGrid/ListBox 虚拟化和真实绑定；demo 右上角色板、演示数据和样例滚动条未迁移。
- 验证基线：source/XAML 通过；WPF UI 校验 0 errors（16 条既有 warning）；Release 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 304/304；render-qa OK（7 页 × Light/Dark × 1040/1100/1366/2560 + resize）。
- 真机：0.6.70 安装成功并加载，`extensions.log` 无新增 XamlParseException；real-host-audit 捕获 EmbeddedSettings/Controlled，`EmbeddedDashboardCaptured=false`（自动化无法点击侧栏），不得冒充完整宿主像素验收。

## 2026-08-16 UI-220 UiLab 几何对齐与虚拟媒体回滚修复

- 上一轮迁移留下的关键差异已收口：`GscRedesignWorkspaceTabItem` 的页面内容对齐改为 `Stretch`，模板只把分段标题居中；否则 WPF 会把真实表格/卡片内容按标题对齐方式压缩到中间，造成用户截图中的大面积空白和错位。
- 维护中心的诊断二级导航已按 UiLab 改为 `GscRedesignSegmented` + 命名面板宿主；“问题列表”和“诊断概览”都可见，原有诊断表格、详情 Inspector、绑定和命令保留。媒体、存档、维护页的本地 TabItem 样式同步改为 Stretch 内容。
- `VirtualizingWrapPanel` 不再把首次无限高度测量解释为零视口；现在优先使用 ScrollViewer/上一次视口高度，并在生成器刷新越界时排队重新测量，覆盖媒体列表切换、滚动到底后回顶的 WPF 时序。
- 首页 Hero/当前游戏列恢复 UiLab 的 `1.35*:1` 比例，744 DIP 紧凑视口仍自动堆叠并保持三枚操作按钮可用。RenderHarness 新增媒体 WrapPanel 滚动回顶探针，并支持 segmented 页面和维护二级 segmented 导航。
- 当前验证：source 校验通过；WPF UI 校验 0 errors（仅既有布局/主题资源提示）；Playnite Release 构建 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 303/303；`render-qa OK`，双主题、多尺寸、resize、维护二级导航和媒体滚动回顶均通过。离屏截图不能替代 Playnite 宿主像素验收；当前 Computer Use 仍无法稳定激活 Playnite，不能把本轮写成完整真机视觉验收。

## 2026-08-16 UI-219 UiLab 分段页面骨架直迁当前事实

- 当前媒体、存档、修改器、维护四个生产页已按 UiLab 的顶层页面骨架运行：`Grid` 的 Auto 导航行 + `GscRedesignSegmented` + `Grid` 面板宿主；不要再把“共用配色/卡片”当成完成迁移，也不要恢复外层 `TabControl` 作为这四页的主导航。维护诊断和审计内部嵌套页签是 UiLab 本身存在的层级，继续保留。
- 生产面板名称：Media=`MediaInboxScrollSurface`/`MediaCurrentScrollSurface`/`MediaSourcesPanel`；Save=`SaveHistoryPanel`/`SaveCandidatePanel`/`SavePolicyPanel`/`SaveComparePanel`；Trainer=`InstalledToolsLayout`/`TrainerImportPanel`/`TrainerCatalogPanel`/`TrainerReleasesPanel`；Maintenance=`MaintenanceDiagnosticsPanel`/`MaintenanceDevicePanel`/`MaintenanceRetentionPanel`/`MaintenanceAuditPanel`/`MaintenanceProcessPanel`。分段 `SelectionChanged` 在 `InitializeComponent` 未完成时必须容忍空字段。
- `Redesign.xaml` 的 `GscRedesignSegmented` 对齐 UiLab `LabSegmented` 的 `SegmentFillBrush`、选中项填充/描边、RadiusM=10 和 item 内边距；运行时浅色/深色主题在 `AdaptiveThemePalette` 同步这些资源。演示色板、窗口按钮、样例数据、样例滚动条不迁移。
- `VirtualizingWrapPanel` 现在对生成器插入位置做边界裁剪，遇到 WPF 生成器时序越界会清空当前生成子项并重新测量；不要改回无边界 `VisualCollection.Insert`，也不要以普通 `WrapPanel` 替换它。
- 当前自动基线：source/XAML 校验通过，Release 0 warning / 0 error，Core 59/59、Worker 191/191、Playnite 303/303；真实扩展版本为 `0.6.70.0`。启动日志未出现本轮新增崩溃，但 Computer Use 无法激活 Playnite（黑色捕获 + `EmptyWindowAutomationPeer`），所以真实页面像素和逐页切换仍是 `MANUAL QA REQUIRED`，不能写成已验收。

## 2026-08-16 UI-218 UiLab 页面骨架迁入生产

- 当前生产页面已按 `D:\workplace\github\GameSaveCenter.UiLab` 的工作区骨架收敛：Dashboard 只保留一套页面头部游戏上下文；`SelectedGameHeader`、`GameHeaderActions`、`RestoreSafetyBanner` 不得在页面外重新显示成重复的第二层。安全说明应放在备份策略/比较表面内。
- `SaveCenterView.xaml` 的策略页使用三栏 Demo 表面并保留真实 `SelectedGame.Policy`、模板、云端和命令绑定；比较指标使用可换行的固定最小宽度，兼顾用户指定的按钮/指标重叠例外。
- `TrainerCenterView.xaml` 的当前游戏工具栏和拖入导入区是两行；`TaskCenterView.xaml` 的搜索、状态、类型与游戏筛选是两行，`TaskGameFilterHost` 必须整体移动，不能只移动 ComboBox。
- 不迁移 UiLab 的演示色板、窗口控制、演示数据和样例滚动条；生产滚动条、DataGrid 虚拟化、键盘/绑定/命令和 Playnite 兼容性优先。窄宿主视口出现策略卡纵向堆叠属于响应式行为，不等于未迁移。
- 当前验证基线：source/XAML 校验通过，Release 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 303/303；真实 Playnite 人工检查首页、备份中心、修改器中心、任务中心已加载，无重复全局安全横幅。自动审计仍保持 `EmbeddedDashboardCaptured=false`，人工截图不能冒充自动嵌入证据。

## 2026-08-16 UI-217 真实 Playnite 人工视觉复核

- `81fde54` Release 包在真实 Playnite 内加载成功；人工进入 GameSaveCenter 后确认首页的开放式头部、真实游戏上下文、单一六项指标带、工作区卡片和生产页脚均可见，演示色板/窗口按钮/样例滚动条没有进入生产。
- 真实任务中心 1303×673 宿主视口可见四项统计带、筛选区、圆角表头 DataGrid、真实状态胶囊和进度条；未选中任务时 Inspector 按 `SelectedTask` 为空隐藏，属于生产交互状态，不是页面布局缺失。
- 自动 `real-host-audit` 仍因 UIAutomation 找不到 Playnite 侧栏而记录 `EmbeddedDashboardCaptured=false`；人工截图只作为本轮观察，不得改写自动审计摘要或冒充完整宿主矩阵。

## 2026-08-16 UI-215 Task 统计栏与窄窗筛选迁移

- `TaskCenterView.xaml` 已把生产任务中心顶部四个独立指标卡改为 UiLab 同款的单一 `TaskSummaryBand`；`Tasks.Count`、`RunningTaskCount`、`RetryableTaskCount`、`CompletedTaskCount` 四个真实 OneWay 绑定保留，内部使用三条 `GscDividerBrush` 分隔线，不迁移 UiLab 的演示数据或滚动条。
- 指标按 UiLab 的“数值在上、标签在下”阅读节奏收敛到 26 DIP，保留任务队列、详情、筛选、DataGrid 表头/滚动条和虚拟化；任务摘要不再通过 `UniformGrid.Columns` 产生卡片换列。
- 修复窄宽度筛选迁移的残留标签错位：`TaskGameFilterHost` 将“游戏:”与真实下拉框作为一个控件组一起移入/移出“更多筛选”，避免 1040-DIP 工作区出现“游戏:”孤立在“类型:”前的拥挤布局。
- 当前自动验证：`validate-source.py`、XAML 检查、Release 0 warning/0 error、Playnite 303/303、RenderHarness 双主题/1040/1100/1366/1600/2560 及 resize 全部 `render-qa OK`；新证据在 `artifacts/ui-qa/task-summary-band-v2`。真实 Playnite Dashboard 仍未重新捕获，不能把离屏截图写成宿主视觉真值。
- 隔离安装构建曾暴露旧的响应式单元测试仍检查 `TaskGameFilterComboBox` 直接挂在筛选容器；测试已同步到新的 `TaskGameFilterHost` 结构，当前 Playnite 测试恢复 303/303。后续移动筛选控件时必须同时更新父容器契约测试。

## 2026-08-16 UI-214 Overview 统计栏与首页光晕收口

- `OverviewView.xaml` 已把原先六个独立圆角统计卡改为 UiLab 同款的单一 `OverviewStatBand`：六个真实 `Snapshot` 指标保留在同一表面内，使用五条 `GscDividerBrush` 分隔线；两个真实比例条、空库折叠保护和 OneWay 绑定均未改变。
- 删除统计卡专用的悬停位移动画，避免把非交互指标做成堆叠/错位卡片；Dashboard 的 `UiAnimationsEnabled` 合约仍保留给其它工作区。
- `DashboardView.xaml` 的根层环境光从三色椭圆收敛为单一 `GscAmbientAccentBrush` 磨玻璃晕影，保留首页喜欢的背景氛围；生产 ScrollBar、页面滚动、虚拟化和真实命令/绑定未迁移 UiLab 演示实现。
- 当前自动验证：`validate-source.py`、XAML 检查、WPF UI 校验（0 errors）、Release 0 warning/0 error、Playnite 303/303、RenderHarness 双主题/1040/1100/1366/1600/2560 及 resize 全部 `render-qa OK`。新证据在 `artifacts/ui-qa/overview-single-band-v1`。
- 本阶段未把离屏截图写成真实 Playnite 嵌入真值；真实宿主 Dashboard 仍受 UIAutomation 无法定位侧栏入口限制，必须继续保留 `EmbeddedDashboardCaptured=false` 的诚实边界。

## 2026-08-16 UI-213 真实宿主审计当前事实

- 当前提交 `420483f` 的 Release 包已通过 `scripts/real-host-audit.ps1` 安装并启动 Playnite；人工通过 Computer Use 进入真实 Playnite，确认 GameSaveCenter 实例和 Settings 宿主窗口可见，未出现立即的 XAML 解析崩溃。
- `artifacts/ui-host-audit/summary.json` 明确记录：`EmbeddedSettingsCaptured=true`、`EmbeddedDashboardCaptured=false`、`ControlledDashboardCaptured=true`、`ProductionVisualSourceOfTruthAvailable=false`。Settings 的真实宿主截图/滚动证据已生成，但 Controlled Dashboard 证据不是生产嵌入像素。
- 因自动 UIAutomation 仍找不到 Playnite 左侧 GameSaveCenter 入口，本轮不能把 Media 缩略图网格写成“真实宿主已验收”；Media 仍以 `artifacts/ui-qa/media-grid-migration-v3` 的离屏多尺寸/主题结果作为自动证据，真实大媒体库、DPI、键盘和连续缩放继续是 `MANUAL QA REQUIRED`。

## 2026-08-16 UI-212 Media 网格当前事实

- `MediaCenterView.xaml` 的当前媒体 `MediaGrid` 已按 UiLab 的 164×142 DIP 卡片节奏改为 `ui:VirtualizingWrapPanel`，缩略图高度 96 DIP；卡片包含真实 `ArchivePath` 异步缩略图、录像/收藏标识、文件名、拍摄时间和云端状态。
- `VirtualizingWrapPanel` 位于 `src/GameSaveCenter.Playnite/Controls/VirtualizingWrapPanel.cs`，实现 `IScrollInfo`，只生成当前视口附近的容器，兼容 Recycling generator；不要替换成普通 `WrapPanel`，也不要迁移 UiLab 的滚动条模板。
- `MediaGrid` 仍是生产 `ListBox`，保留 `ItemsSource={Binding MediaView}`、Extended selection、`ScrollViewer.CanContentScroll=True`、生产滚动条、Inspector 抽屉和真实批量/编辑命令；窄窗仍使用 `MediaCompactDetailsButton`。
- 首次挂载时生成器可能尚未就绪，面板已对该 WPF 测量时序做空保护；Reset、窄宽切换、Light/Dark 离屏渲染均已通过。
- UI-212 自动验证：`validate-source.py`、XAML 检查、Release 0 warning/0 error、Playnite 303/303、RenderHarness 全量 `render-qa OK`；真实 Playnite 大媒体库/DPI/主题/键盘/连续缩放仍需人工验收。

## 2026-08-16 UI-208 Overview 全局活动当前事实

- `OverviewView.xaml` 的全局活动已按 UiLab 业务列表迁移：类型胶囊 → 对象/事件两行 → 结果胶囊 → 时间，不再额外显示 DataGrid 式表头，也不使用图标列。
- 生产仍绑定真实 `Activities`，保留 `ItemsControl` Recycling、`KindDisplay`/`ResultDisplay`、结果语义色和 `OverviewStackScrollSurface` 页面滚动；demo 的滚动条、演示数据和右上角色板没有迁移。
- 窄窗口只缩小 `ActivityKindColumn`、`ActivityTimeColumn` 并降低摘要最小宽度；不要重新添加内部滚动或把时间/结果挤进摘要列。
- UI-208 自动验证：Playnite 303/303，生产 Release 0 warning/0 error，v6/v6.2 Overview 宽/窄截图通过；真实 Playnite 宿主主题/DPI/键盘/连续缩放仍需人工验收。

## 2026-08-16 UI-209 表头共享模板当前事实

- 生产 `DataGridColumnHeader` 已有独立圆角模板；`DataGridColumnHeadersPresenter` 现在必须保持 `Background=Transparent`，否则连续底色会吞掉列头之间的圆角间隙并恢复成硬矩形表头。
- `GscTableHeaderBrush`、`GscTableDividerBrush`、表头高度/内边距、排序 glyph 和页内 DataGrid 虚拟化仍由生产共享模板控制；不要为对齐 UiLab 而迁移 UiLab 滚动条或关闭 `VirtualizingPanel.ScrollUnit=Item`。
- UI-209 自动验证：Playnite 303/303，生产 Release 0 warning/0 error，v6/v6.2 表格与 Overview 宽/窄离屏截图通过；真实 Playnite 宿主主题/DPI 仍需人工验收。

## 2026-08-16 UI-210 Media 当前事实

- `MediaCenterView.xaml` 顶部统计现在是一个 `GscRedesignSectionCard` 内的四段统计带，`MediaSummaryPanel` 仍是 `UniformGrid`，真实统计绑定未改变；默认 `TabControl.SelectedIndex=1` 展示当前游戏媒体。
- Media 当前媒体仍使用生产 `ListBox + VirtualizingStackPanel`，不是 UiLab 的非虚拟化 `WrapPanel` 缩略图网格；这是为大媒体库和现有滚动性能保留的生产适配边界，不要为了像素复制而关闭虚拟化或迁移 demo 滚动条。
- `ApplyResponsiveLayout` 在 700-720 DIP 常规窗口保持 236 DIP 表格下限；宽屏恢复会把已选择媒体的 Inspector 重新设为 Visible，窄屏继续由 `MediaCompactDetailsButton` 控制 Inspector。
- UI-210 自动验证：Playnite 303/303，生产 Release 0 warning/0 error，Media Light/Dark 多尺寸和 resize transition 通过；全量 render-qa 仅剩 Save 候选表历史窄视口门禁。

## 2026-08-16 UI-211 Save 当前事实

- `SaveCenterView.ApplyResponsiveLayout` 的表格高度公式为 `Math.Max(180d, Math.Min(252d, height - 464d))`；常规 700-720 DIP 窗口的 SaveCandidateGrid/SaveHistoryGrid 不得回到 236 DIP 以下。
- Save 历史/候选 DataGrid 仍使用生产共享表头、Item scrolling、行/列虚拟化和现有 Inspector 抽屉；只修复视口下限，没有替换滚动条或绑定。
- UI-211 自动验证：Playnite 303/303，生产 Release 0 warning/0 error，全量 RenderHarness `render-qa OK`（Light/Dark、7 页面、多尺寸、resize transition）。

## 2026-08-16 UI-205-ACRYLIC-PARITY 当前事实

- 权威视觉来源是 `D:\workplace\github\GameSaveCenter.AcrylicFork`（当前参考提交 `b09cba6`），不是 `GameSaveCenter.UiLab`。生产已迁移其页面层级、颜色/表面层级、圆角尺度、按钮/标题比例、Dashboard 开放式页面头部和 Settings 分类栏；不要回头修复 AcrylicFork 样板自身的布局 bug。
- 明确排除 AcrylicFork 演示数据、右上角颜色/主题按钮和样例滚动条。生产继续使用真实数据/命令/绑定/虚拟化和现有带 Track 绑定的滚动条。
- AcrylicFork 顶部色板作为主题参考：靛蓝 `#7C8CF8`、天蓝 `#4FA3F0`、青碧 `#35B8C9`、薄荷 `#4CC08A`、紫罗兰 `#A07BF5`、琥珀 `#E8973C`、玫瑰 `#E56E8C`。生产默认使用靛蓝基线，同时继续尊重 Playnite/强制主题设置，不增加演示色板按钮。
- 共享几何：Shell 20、Card 16、Control 10；Settings 实际内容宽度 ≥700 DIP 使用左侧分类栏，<700 DIP 顶部横向分类，<620 DIP 收紧窄屏标题和字段；这三个断点与 `GameSaveCenterSettingsView.xaml.cs` 保持一致。
- Overview 的全局命令只在 Dashboard 页面头部显示；`OverviewHomeToolbar` 只有 `IsOnboardingPending=True` 时显示，不能重新改成普通状态下的重复命令卡。
- DataGrid 表头与 Button/Card 已按生产共享模板收敛圆角；不要把 AcrylicFork 的滚动条模板覆盖到生产，也不要给可选 `DataGridColumnHeader.Tag` 增加 `CornerRadius` 绑定，Playnite 生成 filler header 可能得到 `UnsetValue`。
- 安装器修订为 `DEV-INSTALL-008`：只停止当前生产扩展目录下的 Worker；其他扩展的 Worker 留在原处，路径不可读取时仍 fail-closed。一次点击安装已成功，生产 Worker 与 AcrylicFork 外来 Worker 可并存。
- 本阶段验证基线：Release 构建 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 302/302；`validate-source.py`、XAML 检查、render-qa（Light/Dark、7 页面、多个尺寸与 resize transition）全绿。
- 离屏截图不是真实 Playnite 嵌入式像素真值；本阶段已真实安装并启动 Playnite/生产 Worker，但没有把宿主窗口像素冒充为自动视觉证据，主题/DPI/键盘/连续缩放仍需人工确认。

## 2026-08-16 UI-205-REAL-HOST-MIGRATION-FIX 当前事实

- Playnite BAML 不应在生产 XAML 中直接写 `clr-namespace:GameSaveCenter.Contracts;assembly=GameSaveCenter.Contracts`：扩展程序集位于 Playnite 私有目录时，BAML resolver 可能从默认 AppDomain 解析失败，即使安装目录实际包含 Contracts.dll。需要使用生产程序集内的 `GameSaveCenter.Playnite.XamlValues` 包装属性；属性返回真实 Contracts enum object，因此不改变绑定/DataTrigger 语义。
- Dashboard 选中游戏标题栏必须在 Grid 重排后再次按 `TransformToAncestor` 的实际 X 坐标限制宽度；首次 measure 的旧 DesiredSize 会让 `SelectedGameHeaderLayout` 和按钮短暂超过页面右边界。`ApplicationIdle` 二次布局与审计的 ApplicationIdle 等待是配套约束，不能只修截图审计。
- `RealHostUiAuditService.CheckChildLayoutOverflow` 复用输出目录时，干净轮次必须删除旧 `CHILD_LAYOUT_OVERFLOW.json`；最终状态以当前 `overflow-classification.json` 为准，不能用旧门禁文件判断本轮。
- 本轮安装验证：`extension.yaml 0.6.70`、生产 DLL `0.6.70.0`，日志确认 `GameSaveCenter 0.6.70.0 loaded`，没有新的 `XamlParseException`/Contracts 缺失；外来 `GameSaveCenterPreview` Worker 不得结束。
- 本轮自动基线：Core 59/59、Worker 191/191、Playnite 303/303，Release 0 warning/0 error；受控矩阵的最终 `RealFixedLayoutOverflow=[]`。由于 UIAutomation 未定位到 Playnite 侧栏，`EmbeddedDashboardCaptured=false` 仍是诚实门禁，受控窗口不能替代真实嵌入像素。

## 2026-08-16 UI-REAL-HOST-AUDIT-BLOCKERS-FIX 当前事实

- Audit CommitSha 必须可追踪：脚本设置 `GSC_UI_AUDIT_COMMIT`，unknown 触发 `AUDIT_SOURCE_REVISION_MISSING` HIGH。
- Embedded 判定用 `IsGenuinelyEmbeddedDashboard`（IsLoaded + PresentationSource + Window 非 fallback）；headless 无人点击时必须诚实 false + HIGH gate。
- `SafeFileName` 不能再用 `string.Join("-", chars)`；已修复为仅替换非法字符并折叠连续 `-`。
- Overflow gate 必须分类：fixed/scroll/decorative；ScrollViewer 内容与装饰性越界不误报。
- Resize 后必须等待 DataBind/Loaded/Render/Idle 且连续两次几何差 ≤0.5 DIP 才截图（最多 3 pass）。
- Manifest 按 `Scope=Dashboard|Settings` 隔离；内部滚动器（DG_ScrollViewer/PART_ContentHost/TextBox/ComboBox）默认排除。
- 基线：Playnite 302/302、Worker 191/191、Core 59/59；Release 0 warning/0 error。

## 2026-08-16 UI-HOST-AUDIT-TRUTHFULNESS-FIX 当前事实

- Real Host Audit 的 origin 必须显式：`DashboardView.AuditHostKind`（默认 EmbeddedPlaynite，fallback 专用窗口设 ControlledAuditWindow）；不要用 `auditDashboardWindow==null` 推断。
- Sidebar View 不能调用 `Activated`；真实 embedded Dashboard 只能由用户在 Playnite 点击侧栏后经 `Opened` 加载，`OnLoaded` 触发捕获。无人点击时输出必须写 `EmbeddedDashboardCaptured=false`。
- `AuditCaptureSession` 隔离三类 manifest：EmbeddedDashboard / ControlledDashboard / Settings；settings manifest 不允许混入 Dashboard entries。
- DataGrid（`CanContentScroll=true`/`ScrollUnit=Item`）是逻辑 item 单位，禁止复用像素 stitch；`DG_ScrollViewer`/`PART_ContentHost` 默认排除。
- `summary.json` 是硬门禁：`EmbeddedDashboardCaptured` / `EmbeddedSettingsCaptured` / `ControlledDashboardCaptured` / `VisualSourceOfTruthAvailable`。
- 基线：Playnite 294/294、Worker 191/191、Core 59/59；Release 0 warning/0 error。

## 2026-08-16 UI-REAL-HOST-CAPTURE-COMPLETENESS-FIX 当前事实

- Real Host Audit 输出语义已重构为三类，不再用“最大 ScrollViewer”冒充整页：
  - `embedded-current/viewport/`：真实 Playnite 嵌入 Dashboard 的当前视口（production visual truth；本次 headless 会话不可用时会如实标注）。
  - `controlled/<profile>/<theme>/viewport/`：无边框审计窗口，profile 即 client size，Dashboard Stretch。
  - `scroll-surfaces/<route>__<name>.png`：每个 meaningful ScrollViewer 的完整 extent。
- 关键实现：无边框窗口（client == outer）、Dashboard `ClearValue(Width/Height)` + Stretch、`SaveViewport` 校验 `Actual*DpiScale` 输出尺寸、`CAPTURE_VIEWPORT_CLIPPED`/`CAPTURE_PROFILE_SIZE_MISMATCH` gates、`capture-manifest.json`。
- 元数据：`Mode` 只能是 `embedded-current` 或 `controlled-host-window`；`CaptureOrigin`、`DedicatedAuditWindowUsed`、`ProfileSizeApplied`、`ThemeOverrideApplied` 必填；PlayniteDesktopVersion 取宿主 exe 文件版本，失败写 `unknown`。
- 基线：Playnite 287/287、Worker 191/191、Core 59/59；Release 0 warning/0 error。

## 2026-08-15 LUDUSAVI-DIAGNOSTICS-FIX 当前事实

- 备份失败常见根因之一是 Ludusavi 联网更新 manifest 超时（`raw.githubusercontent.com/.../manifest.yaml`）；这不是存档路径/ZIP 写入问题，网络恢复后重试即成功。
- 插件侧已修复三项放大问题：外部进程 stdout/stderr 按 UTF-8 解码；`LudusaviCommandResult.RawOutput` 在失败时保留原始输出；剪贴板复制带重试与失败降级（`CopyTextWithRetry`）。
- 相关代码：`ExternalProcessRunner.cs`、`LudusaviClient.cs`、`DashboardViewModel.CopyTextWithRetry`。
- 基线：Worker 191/191、Playnite 281/281；Release 0 warning/0 error。

## 2026-08-15 UI-REAL-HOST-AUDIT-NESTED-TABS-THEMES 当前事实

- 真机审计现在按 5 档窗口尺寸 × `Light`/`Dark` 双主题捕获；每个尺寸/主题目录含 Dashboard、6 个工作区、全部顶层 Tab 与嵌套 Tab（如“异常与审计”→“审计记录”）。
- 嵌套 Tab 捕获要点：选择父 Tab 后等待 `ApplicationIdle`，分别从 TabItem 视觉树和 `tab.Content` 两个路径找子 TabControl，再递归；单纯视觉树遍历会漏掉部分嵌套页。
- 整页截图统一渲染 Dashboard/Settings 根元素（含侧栏和外壳），高度按内容最大滚动范围撑高（maximized 下工作区 1707×1232、Overview 1707×1717）；不要改成只渲染内部滚动器内容，否则会缺左右外壳。
- 设置页兜底窗口必须注入 `GameSaveCenterSettings` 作为 DataContext，否则主题/玻璃设置是默认值。
- 产物：`artifacts/ui-host-audit/screenshots/<size>/<light|dark>/`，zip `artifacts/GameSaveCenter-ui-host-audit.zip`。
- 基线：Release 0 warning/0 error；`validate-source.py`、`check-xaml.ps1` 通过。真实第三方主题/Playnite 窗口内观感仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-REAL-HOST-AUDIT-MULTI-SIZE 当前事实

- 真机审计按 5 档窗口尺寸捕获：`maximized`（WorkArea 1707×912 DIP）、`1600x1000`、`1366x768`、`1280x720`、`1024x768`；每档包含 Dashboard、6 个工作区、全部内层 Tab、窗口截图与 Settings 5 分类。
- Playnite 进程 DPI-unaware：窗口尺寸必须用 `SystemParameters.WorkArea`，不能用 `GetSystemMetrics`（会返回 39×24 虚拟值导致窗口 640×480）；窗口需 `SizeToContent.Manual` 并显式设置视图宽高。
- 多尺寸扫描不启用全页滚动拼接（避免大表格十几分钟卡死）；内容按逻辑分辨率输出防止 OOM。
- 产物：`artifacts/ui-host-audit/screenshots/<size>/` 与 `metadata-<size>.json`；zip 为 `artifacts/GameSaveCenter-ui-host-audit.zip`。
- 清理规则已写入 AGENTS.md「文件清理规则」与 DEVELOPMENT_HANDOFF：每轮完成后删除旧 `dev-build`、`ui-audit-build`、`phase*`、`audit*`、旧 zip 与 `.tmp` 旧目录，只保留当前安装目录与审计证据。
- 基线：Release 0 warning/0 error；`validate-source.py`、`check-xaml.ps1` 通过。真实第三方主题/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-REAL-HOST-AUDIT-FULL-COVERAGE 当前事实

- 真实宿主审计已覆盖 Dashboard 全部页面/Tab 与 Settings 全部 5 个分类；`artifacts/ui-host-audit/` 是最新证据，zip 为 `artifacts/GameSaveCenter-ui-host-audit.zip`。
- 无交互桌面（Playnite 主窗口不可见）时，审计通过专用窗口兜底：Dashboard 1440×900、Settings 同样 1440×900、左上锚定、ToolWindow 可关闭；不再出现越界不可关窗口。
- 设置页兜底的关键约定：输出根在 Dashboard 完成前缓存并传给 Settings 兜底，且窗口创建必须使用 Dashboard 的 UI Dispatcher（线程池 Dispatcher 不会显示窗口）。
- 设置分类 Header 为复杂 Grid，文件命名须从 Header 视觉树提取中文文本，不能用 `Header.ToString()`（会全部同名）。
- 默认 zip 被其他进程占用时审计会写 `GameSaveCenter-ui-host-audit-<时间戳>.zip`，不会中断 Settings 捕获。
- 基线：Release 0 warning/0 error；Playnite 281/281；`validate-source.py`、`check-xaml.ps1`、WPF UI 校验 0 errors。
- 真实第三方主题、连续缩放、用户实际 Playnite 窗口尺寸仍为 `MANUAL QA REQUIRED`；无交互桌面证据不能冒充真实窗口像素。

## 2026-08-15 UI-REAL-HOST-PARITY-CLOSURE 当前事实

- 来源：`GameSaveCenter_RealHost_UI_Parity_Audit_Prompt.zip`，计划 `docs/ai/REAL_HOST_UI_PARITY_CLOSURE_PLAN.md`，报告 `docs/ai/REAL_HOST_UI_PARITY_CLOSURE_REPORT.md`。
- Audit 定位：Tier A `capture-ui-audit.ps1` 是 Offscreen Regression Audit（几何/滚动/虚拟化/fidelity 门禁，不是视觉真值）；Tier B `real-host-audit.ps1` 才是真实 Playnite 视觉事实来源。
- 插件内 `RealHostUiAuditService`：`GSC_REAL_HOST_AUDIT` 或 `%LOCALAPPDATA%\GameSaveCenter\real-host-audit.request` 触发；从真实 Dashboard/Settings 捕获截图、visual tree、resource snapshot、style fingerprint、真实 DPI/bounds；不触发备份/恢复/删除等业务命令。
- `UiDiagnosticsExporters`：resource snapshot / style fingerprint / visual tree / PNG 导出；`AdaptiveThemePaletteContrastGuard`：palette 对比守卫。
- 本机证据：`artifacts/ui-host-audit/` + `artifacts/GameSaveCenter-ui-host-audit.zip`；DPI 1.5，Dashboard 1264×868，runtime palette（accent #0379FF、Glass alpha 0.78-0.94 等）。
- 离屏更漂亮的根因：离屏用 DesignTokens fallback palette；真实宿主用 AdaptiveThemePaletteFactory runtime palette + 真实 DPI/host bounds/data；当前无证据显示 surface hierarchy 被压平，故未改 palette。
- 协定：AGENTS.md / DEVELOPMENT_HANDOFF 已写明每轮完成后 Agent 自己 commit 并 push。
- 基线：Playnite `281/281`；render-qa 全绿；Offscreen UI Audit 0 HIGH/0 MEDIUM/0 fidelity/0 failed routes。
- 真实 125-200% DPI、第三方主题、连续缩放与 Settings paired evidence 仍需人工/下次脚本运行确认。

## 2026-08-15 UI-AUDIT11-RESIDUAL-CLOSURE 当前事实

- 来源：`GameSaveCenter_Audit11_Residual_UI_Closure_Prompt.zip`，计划 `docs/ai/UI_AUDIT11_RESIDUAL_UI_CLOSURE_PLAN.md`，报告 `docs/ai/UI_AUDIT11_RESIDUAL_UI_CLOSURE_REPORT.md`。
- SaveHistory 大小列使用 `SaveSizeValue`（`TextTrimming=None`，`Tag=SaveHistorySize`），列宽 116 DIP；narrow 状态列保留。
- Maintenance Device Inspector 在 Compact/Narrow 默认收起，独立“查看设备详情 ›”按钮，展开 viewport >= 180 DIP，表格 MinHeight 150（header + 2 行）。
- Audit fidelity 新增 `SHORT_SEMANTIC_VALUE_TRIMMING` 与 `INTERACTIVE_INSPECTOR_USABILITY`，均为 MEDIUM 且触发即失败。
- Settings 分类滚动目标整数取整；`ACTIVE_TAB_VISIBILITY` 保持 0。
- 基线：Playnite `276/276`；render-qa 11 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/0 fidelity/0 failed routes。
- 审计 ZIP：`artifacts/audit11-final/GameSaveCenter-ui-audit.zip`（标准路径被外部进程锁定）。
- 真实 Playnite 宿主主题/DPI 125%/150%/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-FIDELITY-CLOSURE-AUDIT10 当前事实

- 来源：`GameSaveCenter_UI_Fidelity_Closure_Audit10_Prompt.zip`，计划 `docs/ai/UI_FIDELITY_CLOSURE_AUDIT10_PLAN.md`，报告 `docs/ai/UI_FIDELITY_CLOSURE_AUDIT10_REPORT.md`。
- Maintenance 不再有局部 implicit `DataGridColumnHeader` style；真实列统一走 `GscDataGridColumnHeaderStyle`，中间表头全部渲染。
- Media 搜索框为 `Auto/*(MinWidth=160)/Auto/150` Grid；narrow 内容宽约 390 DIP。
- Settings 选中分类在 SelectionChanged/ApplyResponsiveLayout 后同步 scroll-into-view（BringIntoView + 增量 delta 收敛）。
- Save History narrow 收起备注列保留状态列；完整备注在版本详情 Inspector。
- Audit fidelity 门禁：`HEADER_CONTENT_FIDELITY` / `ACTIVE_TAB_VISIBILITY` / `CONTROL_USABILITY_GEOMETRY` / `ESSENTIAL_COLUMN_VISIBILITY` 均为 MEDIUM 且触发即失败。
- 基线：Playnite `273/273`；render-qa 11 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/0 failed routes/0 fidelity。
- 真实 Playnite 宿主主题/DPI 125%/150%/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-POST-TYPOGRAPHY-GEOMETRY-CLOSURE 当前事实

- 来源：`GameSaveCenter_PostTypography_Geometry_Audit_Fix_Prompt.zip`，计划 `docs/ai/UI_POST_TYPOGRAPHY_GEOMETRY_CLOSURE_PLAN.md`，报告 `docs/ai/UI_POST_TYPOGRAPHY_GEOMETRY_CLOSURE_REPORT.md`。
- Maintenance 诊断与异常审计的“等级”列统一使用 `GscSeverityColumnWidth`（DataGridLength 92 DIP），不再有 72 DIP 挤压。
- UI Audit Text-Fit：`UiLayoutAnalyzer` 用 `FormattedText` 无约束宽度对比 `ActualWidth`；`TEXT_FIT`=MEDIUM 且 `UiAuditRunner` 遇任何 TEXT_FIT 返回失败码；wrap/ellipsis 文本不误报。
- visual-tree：exporter 不能用 `IsVisible`（离屏 host 无 PresentationSource 恒 false），改用 `Visibility == Visible`；当前 175 个 JSON 非空。
- 基线：Playnite `268/268`；render-qa 11 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/0 failed routes/0 TEXT-FIT。
- 真实 Playnite 宿主主题/DPI 125%/150%/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-FINAL-TYPOGRAPHY-RESPONSIVE-CLOSURE 当前事实

- 来源：`GameSaveCenter_Final_UI_Typography_Prompt.zip`，计划 `docs/ai/UI_TYPOGRAPHY_RESPONSIVE_CLOSURE_PLAN.md`，报告 `docs/ai/UI_TYPOGRAPHY_RESPONSIVE_CLOSURE_REPORT.md`。
- 字体 token：`GscUiFontFamily = Segoe UI Variable Text, Segoe UI, Microsoft YaHei UI`，`GscCodeFontFamily = Consolas, Microsoft YaHei UI`；普通 UI 无硬编码 UI 字体；图标字体 `Segoe MDL2 Assets` 与代码字体 `Consolas` 保留；通用按钮默认 Medium，Primary 保留 SemiBold。
- Settings Compact/Narrow：长说明/副标题/保存提示按断点隐藏，header 最小高度 56-76 DIP；render-qa 760×560 正文 viewport 300 DIP、880×560 285 DIP。
- Save Compare Narrow：主比较区 MinHeight 240、MaxHeight `max(300, height*0.52)`；1040×700 主比较 viewport 234 DIP，保留策略 246 DIP。
- Compact Inspector：Save/Trainer/Media/Task 五个详情按钮均为表格下方独立 `Grid.Row=1` 操作行，无 overlay。
- Media 待归类底栏与表格内容左边缘统一 12 DIP padding。
- 基线：Playnite `266/266`；render-qa 11 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/0 失败路由。
- 真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-FINAL-POLISH-V7.1 当前事实

- 来源：`GameSaveCenter_UI_Final_Polish_Pack_v7_1.zip`，计划 `docs/ai/UI_FINAL_POLISH_PLAN_V7_1.md`，报告 `docs/ai/UI_FINAL_POLISH_REPORT_V7_1.md`。
- 首页活动行五列：Icon/Scope/Message(*)/MetaChip/Time；chip 独立横向列组，Time 右留白 20 DIP。
- `POSSIBLE_CLIPPING=0`；Audit 消息含元素名/父元素/文本，且按 Margin 修正误报。
- 基线：Playnite `263/263`；render-qa 11 档 + 56 主题 + 7 Resize 全绿；最终 Audit 0 HIGH/0 MEDIUM/0 失败路由。
- 最终 Audit ZIP：`artifacts/GameSaveCenter-ui-audit.zip`（Commit `f6f17a8`）；提交 `702b0d5`、`f6f17a8`。

## 2026-08-15 UI-FINAL-CLOSURE-V7 当前事实

- 来源：`GameSaveCenter_UI_Final_Closure_Pack_v7.zip`，计划 `docs/ai/UI_FINAL_CLOSURE_PLAN_V7.md`，报告 `docs/ai/UI_FINAL_CLOSURE_REPORT_V7.md`。
- Audit 工具已支持嵌套子路由与 expected/actual 主表断言；Settings 标题解析为真实分类名。
- 共享 `DataGridStarFill` 附加行为修复宽屏星号列：2K 六张表 ColumnFillRatio=1.00，MaintenanceProcess 目标游戏 1549 DIP；横向滚动 Disabled，Save/Task <1200 DIP 时 Inspector 收起。
- Task 根改为有限 Grid；Media Inbox/Current 取消 460 上限；Task/Media 主行 VerticalFillRatio=1.00。
- Maintenance 表头白块清零；Progress 模板补 `PART_Track` 并新增专用 track/fill token；单行 TextBox `PART_ContentHost` Stretch + Padding 收口。
- 基线：Playnite `263/263`；render-qa 11 档 + 56 主题 + 7 Resize 全绿；最终 Audit 0 HIGH/0 MEDIUM/0 失败路由。
- 最终 Audit ZIP：`artifacts/GameSaveCenter-ui-audit.zip`（Commit `90738b7`）；Progress probe：`artifacts/ui-qa/v7-progress/`。
- 提交：`5cd0226`、`58191d5`、`494b402`、`87d0553`、`7eaaacd`。真实 Playnite 主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-FEEDBACK-GLOBAL-ACTIVITY-CHIP-CENTER

- 首页“全局活动”的 Kind/Result chip 文字已强制水平/垂直/文本三向居中（`OverviewView.xaml` 宽窄两套共 4 个 TextBlock），并有 `UiLayoutRegressionTests` 回归断言锁定。
- 该修正属于 v6.2 之后的用户反馈补丁，提交 `d962b4d`；Playnite `263/263`、XAML/source 门禁与截图均通过。

## 2026-08-15 UI-TABLE-AND-CHIP-CLOSURE-V6.2 当前事实

- 来源：`GameSaveCenter_UI_Table_and_Chip_Fix_Pack_v6_2.zip`，计划 `docs/ai/UI_TABLE_AND_CHIP_CLOSURE_PLAN_V6_2.md`，报告 `docs/ai/UI_TABLE_AND_CHIP_CLOSURE_REPORT_V6_2.md`。
- Chip 已统一为圆角矩形：`GscRedesignContextPill`（CornerRadius 7、MinHeight 26）、`GscRedesignTableStatusPill`（CornerRadius 7）。
- 共享 `DataGridCell` Padding `12,8,20,8`；Overview 时间列 `Margin=12,0,20,0`，六列 `40|150|*|96|84|112`。
- SaveCandidate 可信度列是 ProgressBar（Height 8、Maximum 1、`Value={Binding Score}`）+ `P0` 文本；Task/Overview 已有真实进度条，Settings 数值不是业务进度。
- Maintenance 四个主表 `MaxHeight=PositiveInfinity`；`MaintenanceDeviceLayout` / `MaintenanceProcessLayout` 为 `VerticalAlignment=Stretch`。2K/4K fill ratio：Diagnostics 0.89/0.93、Device 0.82/0.88、Audit 0.88/0.92、Process 0.90/0.93。
- 当前基线：Release 0 warning/0 error；Playnite `263/263`；render-qa 11 档（含 3840×2160）+ 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/8 EXPECTED INFO/0 失败路由。
- v6.2 截图：`artifacts/ui-qa/v6-2-shots/`，命令 `scripts/capture-v6-2-shots.ps1`。
- 提交：`c58b359`、`6a68a59`。真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-OVERNIGHT-CLOSURE-V6 当前事实

- 页面历史已改为 Playnite 会话级：`GameSaveCenterPlugin.SessionLastWorkspace`；首次打开 Overview、同会话恢复、重启回 Overview；`Settings.LastWorkspace` 保留但不再作为启动依据。
- `GscNumericFieldInput` 根模板已修：`PART_ContentHost` 绑定垂直内容对齐；数字 1/5/30/120/1440 完整居中。
- 全局活动为轻量六列表格（40/150/*/88/76/112）+ header；Overview 主列/次列 disabled ScrollViewer 已改为 Grid；Maintenance Device/Process 与 Media Current 外层 ScrollViewer 已改为有限 Grid。
- Task/Media 筛选带语义前缀；Device/Process 主表最小视口 252 DIP。
- 基线：Release 0 warning/0 error；Playnite `261/261`；render-qa 10 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/8 INFO/0 TRUE_PARENT_CHILD_SCROLL_CONFLICT。
- v6 截图：`artifacts/ui-qa/v6-shots/`；命令 `scripts/capture-v6-shots.ps1`。
- 提交：`baa8f72` 计划及后续实施/文档提交见 `git log`。真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-OVERNIGHT-FIX-V4 当前事实

- 来源：`GameSaveCenter_UI_Overnight_Fix_Pack_v4.zip`，计划在 `docs/ai/UI_OVERNIGHT_FIX_PLAN_V4.md`，报告在 `docs/ai/UI_OVERNIGHT_FIX_REPORT_V4.md`。
- `GscDisclosureCard` 已升级：独立 chevron 图标区、垂直居中、无尾部 `>`；所有页面 Expander 统一引用且折叠体内不再内滚。
- 维护中心诊断页已拆成二级 Tab：默认 `问题列表`（FindingsGrid 独占），次项 `诊断概览`（环境/操作/摘要共用页面滚动）。旧内部 ScrollViewer 已删除。
- 存档备份自动化与策略模板的数值输入全部补齐 label/unit/helper；共享样式 `GscFormFieldLabel`、`GscFormFieldHelper`、`GscNumericFieldInput`。
- 首页全局活动行高 60 DIP、图标居中、列 `40/*/Auto(180)/112`。
- 当前基线：Release 0 warning/0 error；Core `59/59`、Worker `190/190`、Playnite `255/255`；render-qa 10 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/39 INFO/0 失败路由。
- v4 截图：`artifacts/ui-qa/v4-shots/`；命令 `scripts/capture-v4-shots.ps1`。
- 用户后续反馈已修复：折叠 header 文字与图标垂直居中；`GscNumericFieldInput` 数字水平/垂直居中显示，框尺寸不变。
- 提交：`3015182`、`5131e4d`、`0201615`、`5196f4a`、`fc86ecc`。真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-14 UI-VISUAL-REWORK-V3 当前事实

- 来源：`GameSaveCenter_UI_Design_and_Prompt_Pack_v3.zip`，计划在 `docs/ai/UI_VISUAL_REWORK_PLAN_V3.md`。
- Overview：当前游戏卡三按钮同排同几何；最近 30 天动作/统计/折叠三层分离；全局活动为轻量表四列，Time 固定 112 DIP，窄窗 chips 下移。
- Save：当前存档规则状态一行 badge 并按 `SelectedGame.HealthState` 着色，三按钮统一紧凑几何，卡片压高。
- Maintenance：环境卡摘要化，首次环境检查/更多维护操作统一 `GscDisclosureCard`（去尾部 `>`），FindingsGrid 五列最小宽度收敛为 72/120/160/*180/140；两个主 Disclosure 内容使用内部有限滚动，展开不挤压主表。
- Disclosure 统一入口：`GscDisclosureCard`（别名 `GscDisclosureCardExpander` 保留），Chevron 独立图标区、整行可点、Hover/Expanded 主题态；Media/Save/Task 的旧 `GscExpander` 引用已全部替换，页面不再引用旧样式。
- 颜色分层全部使用 DynamicResource/Design Token：正常绿、信息蓝、警告橙、错误红、中性灰蓝；未写死前景/背景。
- 功能保真：REMOVE=0；命令、绑定、DataGrid 5 列、EnvironmentCheckItems、虚拟化和 GamePicker HARD LOCK 均未改。
- Overview Hero/当前游戏列保持 1:1，确保 1536×864 等常用窗口下三个操作按钮同一行；render-qa 会检查 Overview/Save 三按钮的 Y 坐标与高度差。
- 当前自动化基线：Release 0 warning/0 error；Core `59/59`、Worker `190/190`、Playnite `253/253`；render-qa 10 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/32 INFO/0 失败路由。
- 截图证据：`artifacts/ui-qa/v3-shots/` 10 张（当前游戏卡、保护折叠/展开、活动宽/窄、Save 标准/窄、Maintenance 初始/两个展开态），生成命令 `scripts/capture-v3-shots.ps1`。
- 提交：`5c3bdae`（v3 计划）、`9ee3660`（Overview）、`e8b8c31`（Save/Maintenance），最终补强与文档提交见 `git log`。真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-14 UI-REFACTOR-V1（实施包 v1）当前事实

- 本轮是严格受控 WPF UI 重构，不是业务重写。事实来源优先级（当时记录，已由 2026-08-20 Demo-first 总规则覆盖）：当前生产 main > UI Audit（commit `4ab44fe`）> 实施包 v1 锁定/范围 > WPF Demo v6.1 > 旧布局。
- 完整功能保真计划在 `docs/ai/UI_REFACTOR_FIDELITY_PLAN.md`：覆盖 92 条命令、43 个 DataGrid 列、30 个 ScrollViewer、143 个条件 UI；默认禁止 `REMOVE`，只允许 `KEEP/MOVE/RESTYLE/COLLAPSE/RESPONSIVE_MOVE`。
- Dashboard 顶部全局 GamePicker 绝对锁定，必须是 Dashboard 单实例共享控件，在六个工作区永久常驻；首页“今日工作台 / TODAY / 当前游戏”只做布局、间距和响应式修正。
- 已知必须修复的 Audit 症状：SaveCandidateGrid 约 3.7 行、MaintenanceAuditLogGrid 约 1.6～1.9 行、MaintenanceDeviceGrid/ProcessGrid narrow 约 3.7 行、诊断 13 工具 narrow 138 DIP 按钮墙、多处 Page Scroll + DataGrid/List Scroll 嵌套。
- Phase 0 基线：Release 构建 0 警告/0 错误，Core `59/59`、Worker `190/190`、Playnite `238/238`，source/XAML/WPF/render-qa 全绿。后续按 Phase 1～8 分阶段独立提交并 push。
- Phase 1（共享布局基础）已交付：`Redesign.xaml` 新增 `GscInternalTabControl`、`GscInternalTabItem`、`GscToolbarActionRow`、`GscToolbarOverflowButton`，并由 `WpfUiResourceDictionaryTests` 锁定；未改任何 View 页面与业务。
- Phase 2（首页 Overview）已交付：六项 Snapshot 指标改为响应式紧凑 Summary Strip（6/3/2 列），最近 30 天保护明细默认折叠到共享 Expander，全局活动改为稳定四列；`OverviewStatStrip` 响应式列数、保护明细可达性、全局活动四列由新回归测试锁定。GamePicker 与首页锁定结构未改。
- Phase 3（存档中心）已交付：历史/候选窄窗 Inspector 默认收起为“查看详情”按钮，主表高度在 1040×700 分别提升到约 385/254 DIP；候选页头部压成单行；策略模板区默认折叠但全部命令可达。新增窄窗 Inspector 切换回归测试。
- Phase 4a（修改器中心 Trainer）已交付：已绑定工具页窄窗默认收起工具设置 Inspector 为详情按钮，1040×700 工具列表视口 236 DIP；新增 Trainer 窄窗切换回归测试。FLiNG/可下载版本/导入流程未改。
- Phase 4b（媒体中心 Media）已交付：当前媒体窄窗 Inspector 默认收起为详情按钮，来源规则添加表单默认折叠但字段可达；新增 Media 窄窗切换与来源表单折叠回归测试。待归类 DataGrid 与媒体异步缩略图未改。
- Phase 5（任务中心 Task）已交付：游戏筛选在 compact 进入“更多筛选”Expander、wide 回到主行；任务详情 Inspector 窄窗默认收起为详情按钮；操作行保持横向；任务表 1040×700 视口 252 DIP。新增 Task 窄窗切换与更多筛选移动回归测试。
- Phase 6（维护中心 Maintenance）已交付：诊断常用按钮收敛为主行 5 个，低频命令进入共享 Expander；审计日志表视口提升到 280 DIP（约 6 行）；设备/进程/Findings 主表保持 350 DIP。保留策略与全部维护命令未改。
- Phase 7（设置轻量统一）已交付：设置字段标签列宽 token 化为 `GscSettingsFieldColumnWidth`；五个设置分区与保存语义未改。
- Phase 8（最终回归）已交付：Audit HIGH 从 10 清零、MEDIUM 从 4 降到 0，失败路由 0；最终测试基线 Core 59/Worker 190/Playnite 250；真实宿主主题/DPI/连续缩放仍为 MANUAL QA REQUIRED。
- Phase 8 收口：`OverviewView.xaml` 把“当前游戏”卡片 3 个操作按钮底部边距从 8 收到 4 DIP，消除最后一个 Audit MEDIUM（未命名 WrapPanel 92 DIP）；顶部工作台工具栏使用 `Padding="14,10"`，1040×700 下由 91 降到 79 DIP。无 REMOVE，GamePicker 与 Dashboard 锁定区域未改。
- 扩档验证：render-qa 覆盖 10 档逻辑尺寸（1040×700 / 1100×720 / 1280×720 / 1366×768 / 1536×864 / 1600×900 / 1707×960 / 1920×1080 / 2048×1152 / 2560×1440）；UI Audit 新增 2K 与 1100×720 尺寸，快照 161，HIGH/MEDIUM 均 0，运行时警告 39。Audit 工作区高度已改为窗口高度，与生产 Dashboard 和 render-qa 一致。
- 主题 QA：RenderHarness 对 7 个工作区 × 4 尺寸 × Light/Dark 共 56 个离屏场景渲染并校验调色板与视口，全部通过；像素采样确认 Light/Dark 背景确实切换。真实 Playnite 宿主主题仍为 MANUAL QA REQUIRED。
- 页面级横向溢出门禁：render-qa 与主题 QA 要求 `*ScrollSurface` / `SettingsScroller` 的 `hbar=Disabled` 且无横向溢出；DataGrid 内部列滚动允许。10 档尺寸与 56 主题场景均通过。
- Resize 恢复：render-qa 新增 2560×1440 → 1100×720 → 2560×1440 同实例布局恢复探针，7 个工作区全部恢复；修复 Save/Task/Trainer Inspector 宽窗不恢复的缺陷，新增 3 条回归测试。
- 验收审计：`docs/ai/UI_REFACTOR_ACCEPTANCE_AUDIT.md` 已落盘，逐项映射实施包验收清单；真实 Playnite 宿主主题/DPI/连续缩放与大数据滚动仍为 MANUAL QA REQUIRED。
- 真实宿主 reload 已验证：`dev-install-run.ps1 -Configuration Release` 成功安装并启动 Playnite；`playnite.log` 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`，扩展日志记录 `0.6.70.0 loaded`，Worker 从当前扩展目录运行，`18:10` 后无 ERROR/Exception/crash。
- Visual Correction v2 已完成：Overview 单滚动、风险卡去内滚、Disclosure、活动行响应式、Save 卡片、Diagnostics 去父子双滚动、Audit 二级切换；新增 OV/SAVE/MAINT 断言，最终 Audit HIGH 0、MEDIUM 0、运行时警告 33。
- Visual Correction v2 真实宿主 reload 已验证：Playnite 加载 `GameSaveCenter 0.6.70`，扩展日志确认 `0.6.70.0 loaded`，Worker 从当前扩展目录运行，`20:39` 后无 ERROR/Exception/crash。

## 当前事实覆盖（2026-08-14 Layer A 收口、Layer B 13 项与 Layer C 11 项）

- `UI-AUDIT-001` 已交付（提交见 `git log -1`）：开发专用 UI 自动审计工具由 `scripts/capture-ui-audit.ps1` / `GameSaveCenter-UI-Audit.cmd` 启动，复用 RenderHarness 渲染真实生产视图；自动扫描 XAML 生成路由/Manifest/保真矩阵，输出视觉树与布局 JSON；页面级滚动容器直接渲染完整内容，DataGrid/ListBox 逐段滚动拼接 `-scroll-*.png`；覆盖 maximized/2k/wide/standard/compact/narrow-1100/narrow，最终 ZIP 在 `artifacts/GameSaveCenter-ui-audit.zip`。后续新增页面只要放入 Dashboard 或 `Views` 目录并保持无参构造，静态盘点与运行时路由会自动纳入。
- 用户日志中的“编译解决方案”失败根因是旧 `dotnet/testhost` 或 Worker 锁住标准 `bin\Release` 输出，随后测试项目无法覆盖 DLL/PDB/XML；不是 `GameSaveCenter.Contracts` 编译失败。
- 一键开发安装器现在默认不请求管理员权限。`scripts/build.ps1`、`scripts/package.ps1` 和 `scripts/dev-install-run.ps1` 支持按运行生成 `artifacts\dev-build\<Configuration>\<guid>` 隔离的 bin/obj、Worker 发布和安装暂存目录，入口修订号为 `DEV-INSTALL-007`。Playnite 发现增加运行中进程、常见目录、卸载信息、App Paths 和 PATH；未发现 Playnite 且没有运行中的 Playnite 时允许继续构建/安装并提示无法自动启动。Playnite 正常退出超时后，仅当进程属于当前会话、可执行文件路径与本次发现结果完全一致且已经没有主窗口时，才结束该无窗口残留；路径不可确认、跨会话或仍有主窗口时继续停止安装。
- 真实宿主已验证：安装报告为 0.6.70 / DLL 0.6.70.0；Playnite `playnite.log` 记录插件加载，插件日志记录 0.6.70.0，`worker-launch.log` 记录存储初始化、过期任务整理和 `Application started`。不要再用 2026-08-12 的 PID 3896 历史日志判断当前安装器行为。
- 当前自动化基线为 Core `59/59`、Worker `190/190`、Playnite `250/250`，Release 构建 0 warnings / 0 errors；source、XAML、WPF 静态门禁与 10 档 `render-qa` 通过；扩档 UI Audit 161 快照、0 HIGH/0 MEDIUM/0 失败路由。真实开发安装已成功，Playnite 与 Worker 启动日志正常。
- `ATOMIC-IO-001` 已交付：新增共享 `AtomicFileWriter`，Worker 设置持久化与媒体复制统一使用“目标同目录临时文件 + 原子 Move”，失败自动清理 `.tmp/.partial` 后再抛出；`WorkerOptions.Persist()` 与 `MediaSyncService` 私有复制逻辑已委托给共享实现。
- `SOAK-001` 已交付：`SoakStabilityHarness` 加速压测任务协调、事件扇出、单游戏锁、原子写入和 SQLite 探针；`TaskEventBroadcaster.SubscriberCount` 与 `GameOperationLock.TrackedGameCount` 提供只读稳定性计数，`scripts/soak-test.ps1` 支持用 `GSC_SOAK_ITERATIONS` 扩展到最多 5000 轮长跑。
- `FAULT-INJECTION-001` 已交付：`FaultInjectionHarness` 注入原子写、外部进程、任务协调、事件广播、操作锁、损坏 ZIP 与损坏 SQLite 共 15 类边界故障，断言无残留、稳定终态、原始文件不被失败注入删除，且锁/订阅全部回收；`scripts/fault-injection-test.ps1` 可独立运行。
- `A-HARDEN-001` 通知级别主体已收口：`NotificationLevel` 持久化默认 `Summary`，`NotificationLevelPolicy` 控制仅重要事件/退出摘要/详细任务；`SessionNotificationAccumulator` 已抽出并覆盖同 Session 单次 final、期望任务数、重复投递等测试。非任务型重要事件（健康风险/冲突/完整性严重）仍由 Dashboard Findings 承载，未单独 toast。
- `A-HARDEN-002` 已交付：未分类 `CustomExecutable` 在普通游戏下按 AutoStart 正常启动；反作弊游戏下必须持久化 `AllowUnknownToolWithAntiCheat` 授权后才允许，Trainer/CT/GameModification 继续禁止；`game_tools` 新增授权列并纳入旧库升级测试。
- `A-HARDEN-003` 已审计收口：首次使用“测试备份”按钮复用真实 `MessageTypes.BackupGame` 生产链路，无独立假服务；无可用测试游戏时显示“可稍后在存档中心手动执行备份”，并有回归测试锁定命令链路。
- `DIAGNOSTICS-001` 已升级：诊断包包含 `system/worker/dependencies/database/recent-tasks/health/settings` JSON、审计与受限日志；`DiagnosticRedactor` 集中脱敏密码、Token、API Key、Authorization、URL query、UNC 凭据、邮箱和用户路径。
- `SAFE-MODE-001` 已升级：Worker 连续 3 次启动失败后请求安全模式，Playnite 询问确认；设置页支持“下次以安全模式启动”，维护中心安全模式提示条提供“恢复正常模式”。
- `INTEGRITY-001` 已补齐：自检覆盖孤儿归档、Manifest 无效/重复路径、磁盘剩余空间和未配置依赖状态；结果使用 `Healthy/Warning/Error/Skipped`，仍只读不自动修复。
- `DB-MIGRATION-001` 已补齐：两代旧库 Fixture 覆盖策略、模板、会话、设备决策、GameTool 与备份历史，并使用 `ReadScalar` 验证真实数据值而非仅检查表存在。
- `METADATA-BACKUP-001` 已补齐恢复流程：预览校验 manifest/哈希/路径越界，确认后备份当前元数据、原子替换数据库与设置、完整性校验并在失败时回滚；维护中心提供“恢复元数据灾备”入口。
- `REPOSITORY-REBUILD-001` 已补齐：只读扫描预览统计已确认/未归属/部分缺失/损坏归档，执行重建必须用户确认，未确认不写库。
- `PATH-REMAP-001` 已补齐：只读预览按类型列出受影响路径和目标存在状态；目标缺失默认跳过，可显式授权仍应用；执行前自动创建元数据灾备。
- `TASK-RECONCILE-001` 已补齐：任务持久化 `WorkerSessionId`，启动协调只处理旧 Worker 会话遗留任务；Backup/Media/Cloud 标记可重试中断，Integrity 标记普通中断，Restore 标记人工介入且不自动重试。
- `GAME-OP-LOCK-001` 已补齐：`GameOperationKind` 与显式兼容矩阵写入代码，备份/恢复/媒体/云端使用类型化锁；Restore 不与其他操作并发，同游戏双 Backup 禁止。
- `IPC-COMPAT-001` 已补齐：握手返回 `AppVersion` 与能力列表，协议版本独立于应用版本，能力包括 RestoreReadiness/MetadataBackup/RepositoryRebuild/PathRemap/TaskReconcile/GameOperationLock/AtomicIo。
- `ATOMIC-IO-001` 已审计补齐：共享原子写入覆盖设置/媒体/元数据恢复/启动失败计数，取消写入或替换失败时旧文件保持完整且无残留。
- `SOAK-001` 已补齐：DataScale Soak 默认小规模、`GSC_SOAK_DATA_SCALE=1` 全量规模；监控 Managed Memory/句柄/线程/订阅/临时文件并断言有界增长。
- `STORAGE-001` 已交付：维护中心“保留策略”页新增只读备份存储分析卡；显示卷剩余/总容量、目录实测与索引体积、版本数、7/30/90 天增长趋势、Top 5 游戏占用排行，并给出标注“估算”的简单容量耗尽预测；新增 IPC `storage.analysis`、Worker 服务与取消支持。
- `RETENTION-SIM-001` 已交付：维护中心“保留策略”页新增全局保留策略模拟器；按每游戏策略复用 `RetentionPlanner` 计算现有/保留/候选清理/预计释放、用户锁定/健康保护/PreRestore 计数与候选明细；`retention.simulation.apply` 要求二次确认，只删除备份根目录下的 ZIP 候选并同步移除 SQLite 索引，锁定/PreRestore/健康恢复点永不进入候选。
- `LOCAL-MIRROR-001` 已交付：设置页新增“启用第二本地镜像”与镜像目录；维护中心“保留策略”页新增镜像状态与“同步镜像”入口。Worker `LocalMirrorService` 只复制和按大小校验，绝不删除镜像中多余文件；外置硬盘未连接时状态为 `Unavailable` 而不是系统错误；同步完成后写入镜像标记文件。
- `ACTIVITY-001` 已交付：首页新增“全局活动”时间线，由 `ActivityTimelineMapper` 把最近 100 条审计记录映射为备份/恢复/云端/媒体/工具/健康/冲突/完整性/仓库修复等业务事件；只展示时间、游戏、分类、结果与摘要，不暴露原始日志，UI 最多显示 12 条并保持有限视口与虚拟化。
- `PLAYNITE-QUICK-001` 已交付：`GetGameMenuItems` 为游戏右键菜单提供“立即备份 / 查看备份历史 / 验证最新恢复点 / 游戏工具 / 打开设置”五个快捷操作，全部绑定当前所选游戏 ID，并复用 Worker 生产 IPC 链路。
- `DRAGDROP-001` 已交付：修改器中心支持单文件/目录拖拽导入，`.ct` 自动按 CheatTable，`.lnk/.bat/.cmd/.ps1` 按自定义启动项，`.exe` 弹出“修改器/普通启动项”二选一，`.zip`/目录进入既有主程序选择流程；未选择游戏时拒绝导入并提示。
- `UI-STATE-001` 已交付：设置持久化上次 Workspace、任务状态/游戏/类型筛选、任务搜索、媒体筛选与媒体搜索；VM 启动时恢复，变更经 500ms 防抖保存；运行中游戏优先与上次选择恢复继续复用既有 GamePicker 持久化，不保存 Loading/Busy/Error 等瞬态。
- `ACCESSIBILITY-001` 已交付：`Ctrl+F` 按当前 Workspace 聚焦游戏/任务/媒体/FLiNG/进程映射搜索框并全选；任务、媒体、FLiNG 与游戏搜索框补充 `AutomationProperties.Name`；共享 `GscSharedFocusVisual` 与高对比度降级继续生效。
- `UI-STATES-001` 已交付：新增共享 `WorkspaceStatePresenter`，统一 Loading/Empty/Error/Degraded/Offline/Disabled 六种状态的图标、标题、说明与可选重试按钮；Overview 全局活动与 Task 空状态已接入共享控件，其余页面继续复用 `GscEmptyStateText`。
- `SETTINGS-VALIDATION-001` 已交付：设置页在标题区显示即时验证摘要，文本框、下拉框与复选框变化时复用 `VerifySettings` 校验并内联展示最多 4 条错误；验证错误不再只等 Playnite 保存时出现。
- `MAINTENANCE-REPORT-001` 已交付：新增 IPC `maintenance.report.get` 与 Worker `MaintenanceReportService`，从 SQLite 计数、完整性自检、存储分析与本地镜像状态聚合用户可读健康报告；维护中心诊断操作带新增“复制健康报告/导出健康报告”，支持 TXT/Markdown；报告不含日志、原始数据库或凭据，与开发者诊断 ZIP 明确区分。
- 最终代码缺口已闭合：`RepositoryRebuildService` 现在可从空/新 SQLite 按磁盘 ZIP 与 Manifest 重建历史，按 Ludusavi 目录名创建 `recovered-*` 占位游戏，不猜 Parent，二次重建幂等；`MetadataBackupService` 灾备包新增 `settings/plugin-settings.json`，恢复后由 Playnite 侧导入插件设置并回滚；`WorkspaceStatePresenter` 已覆盖存档历史 Loading、修改器工具 Loading/Empty、媒体 Worker Offline、维护云端 Degraded；`LocalMirrorService` 同步改为 SHA256 内容校验，同大小但内容不同会重新复制。
- 崩溃修复：`GscWorkspaceStatePresenter` 模板内重试按钮从普通 `Button` 改为 `ui:Button`，修复真实 Playnite 切换存档页时 `“Button”TargetType 与元素“Button”的类型不匹配` 的 XamlParseException；已增加源码回归断言并在真实宿主复测。
- Metadata 原子回滚：恢复前用 `VACUUM INTO` 生成一致性 DB 快照（不再直接复制可能缺 WAL 的活库）；Worker 新增 `metadata.restore.rollback`，Playnite 侧新增 `MetadataRestoreCoordinator`，Plugin 设置导入/保存/应用任一步失败时先恢复旧插件设置，再调用 Worker 从 PreRestorePath 回滚 DB 与 Worker 设置，失败才进入人工介入。
- 本轮已修复 Layer A 审计缺口：多设备只有 Manifest 内容指纹相同才可判定等价；仅文件数/总大小相同改为保守的未知分歧；Restore Readiness 使用可取消的流式解压与增量 Hash；环境检查分别验证数据、存档和媒体所在磁盘；Manifest 重复路径不会抛异常或产生强指纹。
- `DIAGNOSTICS-001` 已完成：维护中心可导出有上限、只读、脱敏的 ZIP 诊断包；包含环境/任务/审计/Worker 日志摘要，不包含数据库、存档、媒体或凭据；新增 IPC 请求和 Worker 测试覆盖敏感字段与大小边界。
- Layer A 14 项、本轮审计补缺、A-HARDEN-001/002/003、Layer B 13 项（DIAGNOSTICS/SAFE-MODE/INTEGRITY/DB-MIGRATION/METADATA-BACKUP/REPOSITORY-REBUILD/PATH-REMAP/TASK-RECONCILE/GAME-OP-LOCK/IPC-COMPAT/ATOMIC-IO/SOAK/FAULT-INJECTION）与 Layer C 11 项已交付；逐项验收见 `docs/ai/PRODUCT_HARDENING_LAYER_B_AUDIT.md` 与 `docs/ai/PRODUCT_HARDENING_LAYER_C_AUDIT.md`，最终逐项审计见 `docs/ai/PRODUCT_HARDENING_EPIC_FINAL_AUDIT.md`，人工验收清单见 `docs/ai/FINAL_MANUAL_QA_CHECKLIST.md`。由于真实场景人工验收未全部完成，整体 Epic 状态为 `PARTIALLY COMPLETED / MANUAL QA REQUIRED`，不能宣称全部任务完成。
- 通知级别已收口：`ImportantOnly` 只显示失败/取消任务与警告/失败摘要，`Summary` 保持一次退出摘要，`Verbose` 在最终摘要外逐任务显示；设置页新增通知级别选择，旧设置缺省归一为 `Summary`。
- 安全模式已交付：全局开关持久化到插件与 Worker 设置；开启后暂停自动退出/定时备份、自动媒体同步、自动工具启动、会话存档快照与保护提示、云端自动上传与自动重试，手动操作和恢复仍可用。维护中心诊断页与诊断摘要会显示当前状态。
- 完整性自检已交付：维护中心“完整性自检”通过 IPC 检查 SQLite 完整性/外键/表结构、目录可写性、配置程序存在性和索引文件引用；只报告不修复，数据库问题为 Critical，文件缺失为 Warning。
- 数据库迁移 Harness 已交付：`DatabaseMigrationHarness` 在临时目录创建旧版 Fixture 后执行当前 `SqliteStateStore.InitializeAsync`，覆盖旧库升级、全新库创建、重复初始化和失败报告；只操作临时数据库，不触碰用户数据。
- 元数据灾备已交付：维护中心“导出元数据灾备”生成 SQLite `VACUUM INTO` 一致性快照、脱敏 Worker 设置和版本清单 ZIP；不包含存档、媒体或凭据，超过 512 MiB 安全上限时失败并清理。
- 备份索引重建已交付：维护中心“重建备份索引”按 Ludusavi 磁盘列表重建 SQLite 版本索引，单游戏失败不中断，只读归档并保留失败游戏原索引。
- 批量路径迁移已交付：维护中心“批量路径迁移”按旧根/新根前缀批量改写 SQLite 与 Worker 设置中的已索引路径；只改引用不移动文件，服务端强制确认。
- 中断任务协调已交付：维护中心“协调中断任务”把 Worker 重启遗留的排队/运行中任务幂等标记为 `WORKER_RESTARTED`，启动时仍自动执行同一逻辑。
- 单游戏操作锁已交付：同一游戏的备份、云端重试、媒体同步、恢复预览/执行互斥，超时返回 `GAME_OPERATION_BUSY`；不同游戏并行不受影响。
- IPC handshake 已交付：`system.handshake` 返回协议版本、最低支持版本与 Worker 版本；客户端握手不兼容即拒绝，旧 Worker 回退 Ping 探测。
- Restore 在实际写入开始后的失败、异常或后校验失败必须尝试恢复锁定的 PreRestore；回滚本身失败才进入 `ManualInterventionRequired`。灾难演练现覆盖 A/B/Undo、部分写入、写后异常、权限、只读、目录缺失和回滚失败。
- 多设备云目录使用持久化 32 位不透明 `DeviceId`，机器名只用于显示与旧 sidecar 兼容；便携设置导入不得复制设备身份。远端恢复继续要求隔离下载、Rclone check、Ludusavi 版本确认和既有 PreRestore 恢复链。
- 每游戏策略新增 `BackupAnomalyProtectionLevel`（Off/Normal/Strict）；重要游戏模板默认 Strict。Manifest 大量删除参与异常检测，最后健康恢复点与用户 Lock 都不能成为 retention 候选。
- Rclone 每次执行都经过命令白名单 `copy/check/lsf/cat/version`，禁止 `sync/move/delete/purge`；外部进程日志不再记录完整参数。Worker 重启会把未完成任务转为 `WORKER_RESTARTED`，取消会终止子进程。
- 真实 Rclone 断网、真实两台设备、真实游戏 Restore/Undo、真实 EXE/LNK/BAT/PS1、1000+ 游戏库和完整主题/DPI 连续缩放仍为 `MANUAL QA REQUIRED`，不得由自动化结果冒充。

## UI-QA-REAL-006 设置分类 Tab 实际裁切修复（2026-08-13）

- 上一轮仅在 `TabPanel` 外增加底部留白没有解决用户截图中的直线底边。实际根因是 `GscRedesignSettingsTabItem` 让圆角 Border 直接充满 `TabItem` 模板布局槽，并开启 `ClipToBounds=True`；`TabPanel`/宿主布局取整后会把 Chrome 的底部圆角贴槽裁平。
- 当前共享模板使用不裁切的 `TabItemRoot` 包裹独立 Chrome；Chrome `VerticalAlignment=Top`、`Margin=0,0,0,2`，因此始终保留底部安全距离并移除 Chrome 的 `ClipToBounds=True`。
- 分类滚动内容使用真实 `SettingsHeaderBottomSafetyZone` 元素放在 `TabPanel` 后面形成内容 extent；顶部横向模式折叠该元素。RenderHarness 同时检查最后一项 `TabItem`、Chrome 的底部位置和 `chromeSafety >= 1`。
- 当前验证：5 种窗口渲染图通过，设置几何探针和 Playnite `210/210` 通过；真实 Playnite 主机的 DPI/主题/连续缩放依旧只能由人工验收确认。

## 2026-08-13 UI-QA-REAL-005 首页顶端对齐、当前游戏空间与设置圆角回归

- 首页宽屏 `OverviewSecondaryScrollViewer` 与其内容面显式使用 `VerticalAlignment/VerticalContentAlignment=Top`，并在响应式代码中重复设定，避免 Playnite 宿主模板刷新后“今日概览”落到工作区中部。
- 首页 Hero/当前游戏宽屏列由 `1.25* + 0.75*` 调整为 `1.1* + 0.9*`；离屏报告中的当前游戏/Hero 宽度比约 `0.82`，原约 `0.60`，没有改变 Hero/当前游戏的堆叠断点、命令或绑定。
- 设置共享分类栏模板在 `TabPanel` 外增加命名的底部安全 host，并设置顶部内容对齐、像素对齐和布局取整；滚动到末端时最后一个分类的底部仍落在 viewport 内，避免圆角被横向直线裁掉。
- RenderHarness 现在在截图前显式解除设置页入口动画的 `Opacity=0`，并检查 Overview 右栏 top delta、当前游戏宽度比和 Settings 最后一张 Tab 的底部几何，避免“空白 PNG/只测到布局没有测到可见性”。
- 验证：`python scripts/validate-source.py`、WPF 静态门禁、`git diff --check`、五种窗口尺寸 `render-qa` 全绿；Core `42/42`、Worker `117/117`、Playnite `210/210` 通过。真实 Playnite 主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## AI/Codex 启动协议

开始 GameSaveCenter 开发前，请依次阅读：

1. `docs/ai/CURRENT_STATE.md`（当前事实入口）
2. `docs/ai/PROJECT_MEMORY.md`（本文件）
3. `docs/ai/WORKLOG.md`
4. `docs/DEVELOPMENT_HANDOFF.md`
5. `git log` 最近 15～30 个 commit 与 `git status`
6. `docs/design/APPLE_WPF_IMPLEMENTATION_PROMPT.md`（UI 任务）
7. `docs/design/UI_CHANGE_GATE.md`（UI 任务）

然后才开始修改代码。不要仅凭历史对话假设当前项目状态；代码、文档和 Git 历史是唯一事实来源。

## 项目定位

- GameSaveCenter 是 Playnite 的 GenericPlugin，提供存档备份/恢复/校验、媒体同步、任务中心、维护中心、修改器与 CT 管理，以及新增的自定义游戏启动项能力。
- Playnite 是唯一主要 UI（WPF），后台 Worker 是独立 .NET 8 进程，两者通过 Named Pipe IPC 通信。
- `GameSaveCenter.Contracts`：Playnite/Worker 共享的 DTO、枚举、消息类型，netstandard2.0。
- `GameSaveCenter.Core`：Playnite 侧可复用逻辑（目前主要是启动/包装与少量辅助）。
- `GameSaveCenter.Worker`：持久化、Ludusavi、Rclone、媒体索引、任务编排、游戏 Session、GameTool 导入/启动/追踪。
- `GameSaveCenter.Playnite`：WPF Dashboard 外壳 + 六个 Workspace 页面 + 设置页。
- 数据持久化：SQLite（`SqliteStateStore`）+ 文件系统（存档、媒体归档、GameTools 目录）。
- 模块关系：Ludusavi 负责存档底层；Rclone 只允许 copy/check，不使用 sync/delete/purge；媒体为增量同步；GameTool 绑定在游戏级。

## 当前主要架构

### 程序集与入口
- Solution：`GameSaveCenter.sln`，版本 `0.6.70-development-preview`（`Directory.Build.props` 0.6.70）。
- 插件入口：`src/GameSaveCenter.Playnite/GameSaveCenterPlugin.cs`，扩展 ID `66e9f2d7-67bb-43ef-b62a-b8e60734fcec`。
- Worker 入口：`src/GameSaveCenter.Worker`，IPC dispatcher 为 `IpcRequestDispatcher`。
- 测试：Core 42、Worker 117、Playnite 203（2026-08-13 当前基线；优先使用 `scripts/build.ps1 -OutputRoot <目录>`，避免本机旧 Worker/测试宿主锁住标准输出）。
- ONBOARDING-001（2026-08-13）新增 `environment.check`：检查服务驻留 Worker，使用临时 SQLite 表和目录临时文件做可逆探针；Rclone 未配置为 `Skipped`，不把可选云端能力误计为基础失败。当前基线为 Worker 70、Playnite 198；UI 仍复用 Maintenance 诊断页的单一外层滚动与有限表格视口。
- GAME-TOOL-003/004（2026-08-13）新增 `GameToolIfAlreadyRunning` 与 `GameToolRiskCategory` 持久化列。CustomExecutable 的已有实例策略只允许按解析后的 EXE 完整路径匹配；Skip 为默认，Restart 只重启再次确认过的同路径 PID，路径读取不完整时必须保守停止。反作弊游戏仅允许已分类为 `GeneralUtility` 的自定义工具自动启动；Unknown 与 `GameModification` 自动启动必须阻止并写审计，用户需在 TrainerCenter Inspector 明确分类后保存。
- SMART-PROTECT-001/002（2026-08-13）：完整游戏停止请求等待存档识别并以持久化提示状态驱动三选一保护提示；只在识别到候选/匹配存档时提示，未识别时写审计并等待后续识别。`Deferred` 有 7 天冷却，`Enabled`/`Dismissed` 不再弹出；停止 IPC 使用 3 分钟专用超时。Overview 最近游戏列表显示已保护、未匹配、存档未保护和风险，已保护项不可选，其余项可批量启用游戏中/退出后推荐保护并写审计。不要新增主导航页或绕过既有恢复安全边界。
- NOTIFY-001 / MULTI-DEVICE-001 / RCLONE-RELIABILITY-001（2026-08-13）：退出备份与媒体任务使用同一 SessionId，Playnite 依据 Task Center 的终态任务聚合为一条退出摘要；本地备份成功但云端失败时必须同时显示本地成功和云端可重试失败。设备摘要携带 `ParentBackupId`，同一父版本分叉只标记冲突并要求人工决策，禁止自动合并/覆盖/删除；下载远端仍必须进入隔离 staging、校验、归档检查后才能走既有安全恢复链。Rclone 仅允许 copy/check/lsf/cat/version；网络或不完整传输有限重试，凭据/权限/远端不存在明确失败并停止自动重试。

### Dashboard / Workspace
- `DashboardViewModel` 是大型聚合 ViewModel（技术债，暂不拆分），持有所有 Workspace 数据与命令。
- 六个 Workspace：Overview（首页）、Saves（存档中心）、Trainers（修改器中心）、Media（媒体中心）、Tasks（任务中心）、Maintenance（维护中心）；另有 Settings 页面。
- 工作区页面位于 `Views/`：DashboardView + 各 CenterView；共享资源在 `Themes/DesignTokens.xaml`、`Themes/WpfUiProduction.xaml`、`Themes/Redesign.xaml`。
- Dashboard 视图有响应式 code-behind 协调（`DashboardView.xaml.cs`），页面级滚动面 + 主表/主列表有限视口 + 内部虚拟化滚动。

### UI-207 当前约束（2026-08-12）

- Settings 的 `SettingsScroller` 位于共享 `GscRedesignSettingsTabControl` 模板内容区；`SettingsHeaderScroller` 是分类导航区。宽屏分类栏为 232 DIP 左侧有限滚动，紧凑布局为顶部横向 `Auto`，不能把根 UserControl 再包回第二个页面滚动器。
- `GscSelectedGameIconControl` 只用于当前游戏上下文表面（Dashboard、Overview、Save、Trainer、Media），GamePicker 虚拟化列表不得加载真实 Icon。
- GamePicker 选择可被当前筛选隐藏但不能静默丢失；必须保留 `SelectedItem`、显示恢复语义并保持 `GamePickerSelectedGameId` 持久化。默认筛选只对新用户/未知值归一为“已安装”。
- 事件驱动的 `PlayniteGameStarted` 自动定位优先于普通刷新；页面每次 `Loaded/IsVisible=true` 允许一次只读 Playnite `Game.IsRunning` 同步，以补足 Worker 在既有游戏运行后启动时的基线缺口。该同步不得启动 Worker 会话、进程扫描、IPC 轮询或网络请求，也不得改动 DataGrid 滚动/虚拟化契约。
- 当前自动化结果：本阶段 Worker 相关 Release 构建 0 警告/0 错误，Worker 67/67 通过；上一阶段 Core 27/27、Playnite 197/197、render-qa 通过。真实 Playnite 宿主/DPI/主题人工验证仍待环境。

### 数据流
- Playnite → Worker：Named Pipe 请求（`GameSaveCenter.Playnite/Ipc`、`GameSaveCenter.Worker/Ipc`）。
- 任务状态：Worker `TaskCoordinator` 持久化 + `TaskEventBroadcaster` 事件流 + Dashboard 轮询兜底。
- 快照：`MessageTypes.GetDashboard` 返回 `DashboardSnapshotDto`；大库先渲染 SQLite 缓存，后台再同步。

### GameTool 模型
- `GameToolType`：Trainer / CheatTable / CustomExecutable（自定义启动项）。
- `GameToolDto` + `GameToolVersionDto`：DisplayName、Enabled、AutoStart、LaunchTiming、LaunchDelaySeconds、CloseOnGameExit、RequiresAdmin、ActiveVersionId、EntryPath、WorkingDirectory、Arguments、ResolvedTargetPath 等；`game_tool_versions` 已补 `resolved_target_path` 兼容列。
- Worker `GameToolService`：导入（Trainer/CT 复制进 GameTools 目录；自定义启动项默认保留外部路径引用）、更新、删除、启动、随游戏自动启动/延迟/关闭追踪。
- Session 追踪：`GameToolSessionTracker`（SessionId → PID + 实际 StartTime + CloseOnExit），关闭时要求 PID 与实际 StartTime 双向匹配，禁止按进程名杀。

### 任务系统
- `TaskCoordinator` 统一编排；`TaskStatusDto` 有 Progress/Message/ErrorCode/ErrorMessage/State/时间戳。
- Dashboard `TaskIndexedCollection` 按 TaskId 索引增量合并；`knownTaskStates` 去重通知。

### 媒体系统
- `MediaItemDto` 由 Worker 索引；列表与详情预览已改为 `AsyncThumbnailImage` 异步加载（`Task.Run` 强制后台、3 并发、LRU 96、Freeze 后回 UI、Unloaded 取消）；`MediaThumbnailConverter` 保留为兼容实现。
- Media 列表使用 ListBox + Recycling 虚拟化；页面滚动面与列表滚动分工明确。

### 缓存与性能机制
- `BatchObservableCollection<T>`：批量 Replace 只发一次 Reset（默认引用相等比较；PERF-005 起支持内容比较器跳过未变化）。
- GamePicker 有 180ms 搜索防抖、按 PlayniteId 缓存 `GamePickerItem`、平台指纹短路。
- Task 筛选指纹短路（`ComputeTaskFilterFingerprint`）、平台指纹短路（`ComputePlatformFingerprint`）。
- Dashboard 大库 cache-first + 非阻塞后台目录描述同步；昂贵的 Ludusavi 匹配仍由 Worker 节流队列处理；`[PERF]` 日志设施见 `docs/ai/PERFORMANCE_BASELINE.md`。

## UI 设计原则

- 目标是 Apple-inspired 的原生 WPF 桌面工具：清晰层级、克制毛玻璃、圆角、统一设计令牌、自然微动效、深浅色、跟随 Playnite、高对比度、DPI 适配、响应式布局、不使用突兀的原生控件视觉。
- 所有 UI 修改必须先读 `docs/design/APPLE_WPF_IMPLEMENTATION_PROMPT.md` 与 `docs/design/UI_CHANGE_GATE.md`，并遵循 `.codex/skills/wpf-apple-desktop-ui/SKILL.md`。
- 常用窗口下限 1040×700 DIP；1080p/2K/4K 必须按 DPI 换算后的逻辑 DIP 检查全屏、窗口化、最大化；不把 4K 通过当作 1080p 通过。
- 页面级滚动只承载有限测量内容；DataGrid/ListBox 保留 236 DIP 最小视口、内部滚动和虚拟化；堆叠 Inspector 下限 160 DIP。
- 动态下拉框必须显示逻辑默认值（如“全部”）；TaskCenter 游戏/类型筛选通过 `TaskFilterOptionsSync` 增量同步，`全部` 稳定保留在 index 0，不再 Clear/Replace 集合。
- GamePicker 新用户默认筛选为“已安装”，已有明确配置值必须保留；Dashboard 打开时运行中游戏优先，否则恢复上次选择，普通刷新不得抢回用户手动选择。

## 已完成的大型重构 / 优化

- UI-001～UI-205、SKILL-001、QA-001～005：页面 Workspace 化、响应式断点、滚动分工、Inspector 下限、筛选默认值、离屏渲染 QA。
- UI-207（2026-08-12）：设置页 Header 不裁剪与分类栏滚动（920 DIP 断点）、运行中游戏自动定位、上次选择持久化复用、GamePicker 新用户默认“已安装”、当前游戏真实 Playnite Icon（事件驱动，无轮询/无网络，LRU 48）。
- `scripts/render-qa.ps1` + `tests/GameSaveCenter.RenderHarness`：7 页面 × 5 常用窗口离屏渲染回归，含自动失败门禁。
- PERF-001：`BatchObservableCollection` 批量 Reset。
- PERF-002/003：Task 筛选与 GamePicker 平台指纹短路。
- PERF-004（旧编号）：GamePickerItem 缓存复用（新任务编号体系中 PERF-004 是性能基线设施，不要混淆）。
- PERF-004/005/006（新编号）：`[PERF]` 基线日志、Snapshot 无变化 0 Reset、Task/Media 搜索防抖。
- PERF-007：媒体缩略图异步化（`AsyncThumbnailLoader` Task.Run 后台解码 + 3 并发 + LRU + Freeze + `[PERF]` 埋点，`AsyncThumbnailImage` 占位加载并 Unloaded 取消）。
- PERF-009/010：任务事件合并 TaskId 索引 O(1) 更新；命令状态刷新 Dispatcher 合帧。
- GAME-TOOL-001/002：自定义启动项正式支持 EXE/LNK/BAT/CMD/PS1，外部路径引用不复制文件；Session 级 PID 追踪与 CloseOnGameExit 安全关闭。
- UI-204/205：TaskCenter 与 GamePicker 下拉框默认值恢复（含真实 Playnite 异步物化重试）。
- UI-206（含回滚）：DataGrid 滚动几何修复。初版 `Pixel ScrollUnit` 经真实 Playnite A/B 验证会严重恶化空白，已撤回；最终采用 `Item` + `GscStableDataGridRow` 稳定行样式 + geometry probe（60 行 × 非整行高度，gap ≤4 DIP、末行完整、无跳变、Recycling 保持）；诊断摘要取消外层裁剪并由页面滚动负责可达性。

## 当前技术债

- `DashboardViewModel` 仍很大，包含命令、筛选、导入、诊断、设备状态等职责；只有性能实现被严重阻碍或 GAME-TOOL 无法接入时才拆（独立 `ARCH-xxx` 任务）。
- `DashboardView.xaml.cs` 仍承担部分响应式协调。
- 媒体列表/详情缩略图已异步化；真实大量截图滚动下的帧率仍需真机验证。
- 真实 Playnite 宿主、主题切换、DPI 真机、连续缩放流畅性尚未完整人工验收（UI-QA-REAL-001 仅完成冒烟）。

## 当前开发优先级

- P0：性能基础设施与真实热点优化（PERF-004～007、009/010 已完成）。
- P0：自定义游戏启动项（已完成，GAME-TOOL-001/002）。
- P1：媒体性能（PERF-007 异步缩略图，已完成）。
- P1：真实 Playnite / DPI / 大型游戏库 QA（UI-QA-REAL-001 冒烟已完成，完整人工验收待用户）。
- P2：架构进一步拆分（不主动做）。
- PERF-008：已评估收口，维持现状。详情已按激活 Workspace 分支加载，全量快照仅用于全局摘要且后台有 1 分钟 TTL；2000 规模合成 profiling 无 O(n^2)，待真实大库渲染 profiling 证明瓶颈后再评估。

## 2026-08-12 可靠性阶段补充

- `RELIABILITY-RESTORE-001` 已实现：备份历史版本支持非破坏性的恢复可用性检查，结果持久化在 `backup_versions.restore_readiness_json`，检查过程只在应用数据目录隔离提取，不接触真实存档目录。
- `d45f65c` 已补齐恢复校验安全闭环：Manifest 非法、重复/越界路径、Manifest 缺失文件现在不能得到 `Ready`；逐文件路径集合、大小、可用 Hash 和提取结果均纳入判定，验证目录创建失败返回 `Failed`，取消仍由调用方观察。
- `d45f65c` 为恢复编排增加窄接口测试边界，并用临时 SQLite + 内存假 Ludusavi 完成 A→PreRestore→B→失败回滚、成功恢复→Undo、运行中拒绝恢复等灾难演练；未启动 Playnite、未调用真实 Ludusavi、未接触真实存档。
- Ludusavi 备份版本的 `backupPath + backup ID` 已持久化为 `backup_versions.archive_path`；Simple 归档、缺失/损坏 ZIP、路径穿越、超大展开量、不一致统计与不支持压缩方式必须返回明确状态。
- 恢复可用性入口位于现有 Save Center 历史 Inspector，不能新建页面或改变 `SaveHistoryGrid` 的滚动/虚拟化骨架；新增内容必须留在 `SaveHistoryActionsScrollViewer` 内，并继续通过 `render-qa` 验证 1040×700 等窗口。
- 初始实现的历史基线为 Core 13、Worker 58、Playnite 197；当前阶段增量基线为 Worker 67/67，生产 Worker Release 构建 0 警告/0 错误。真实 Playnite 宿主、主题/DPI 人工验收仍待用户环境确认。
- 该恢复可用性阶段的下一项已在后续 `HEALTH-001` 完成；历史记录保留原阶段编号，当前开发顺序见下方 HEALTH-001 补充。

## 2026-08-12 HEALTH-001 阶段补充

- 每游戏健康状态已统一为四态：`Healthy`（健康）、`Attention`（注意）、`Risk`（风险）、`Unknown`（未知）。旧 `Ready / Warning / LudusaviUnavailable` 仅作为 UI/历史缓存兼容输入保留；新 Dashboard 快照输出四态。
- `GameHealthAssessmentService` 是 Core 纯计算服务，证据包括最近游玩、备份版本/时间、最近 30 天失败任务数、最近任务状态、最新 `RestoreReadinessStatus`、未解决 finding 严重度、按游戏策略启用的云端状态；不做磁盘、ZIP、网络或数据库访问。
- Worker 的 `GetDashboardGameRecordsAsync` 一次聚合最新备份可用性、任务失败、finding 和媒体/策略数据；`DashboardService` 只在内存中计算四态和理由，并把 `WarningGames = AttentionGames + RiskGames`，`UnknownGames` 不误计入需处理数。
- UI 改动只复用首页统计卡、Dashboard 游戏列表/选中头部和 Save Center 校验区；四态在有限宽度下不新增列或固定宽度，理由使用已有 Tooltip，旧 `Ready` 夹具继续显示绿色。Snapshot comparer 已比较健康摘要与理由列表。
- 当前测试基线为 Core 19、Worker 59、Playnite 197；源码门禁、XAML 门禁、WPF 静态门禁、隔离 Release 构建和 render-qa 已通过。真实 Playnite 宿主、主题/DPI 人工验收仍待用户环境确认。
- 当前已完成 Restore Readiness、Health、Protection 三项；下一项按附件顺序为 `POLICY-001`，不要重做上述功能，不新增主页面，继续采用小阶段、独立 commit、文档和 push。

## 2026-08-12 PROTECTION-001 阶段补充

- `RecentProtectionAssessmentService` 已在 Core 实现为无副作用纯计算：以 `GameStatusDto.LastPlayedUtc` 过滤最近 7/30/90 天，按未识别存档、从未备份、恢复点不可用、自动保护关闭、云同步异常、游玩后备份过旧和备份健康异常分类；每个游戏只显示一条最高优先级原因。
- `GameStatusDto` 现在带有 `LatestRestoreReadinessStatus`，由 Worker Dashboard 从已有聚合记录投影；Playnite 不增加 IPC、扫描或数据库查询，Overview 只从现有快照计算摘要。
- UI 复用现有 Overview 风险滚动面与 Settings 自动化分类。保护摘要最多展示 6 条；选择条目只改变当前游戏选择并提示用户确认，绝不因筛选/选择自动备份或恢复；没有新增页面，也没有修改 DataGrid、虚拟化或滚动骨架。
- 最近保护窗口设置默认 30 天，接受 7/30/90，便携设置导入会校验非法值，旧 JSON 缺少字段时保持默认值。
- 本阶段验证基线为 Core 27、Worker 59、Playnite 197；Worker/Playnite Release 构建、源码/XAML/WPF 门禁和 render-qa 均通过。真实 Playnite 主题/DPI/键盘/连续缩放验收仍待用户环境。
- 下一项按附件顺序为 `POLICY-001`；不要重做 `HEALTH-001` 或本阶段保护摘要。

## 2026-08-12 POLICY-001 阶段补充

- 策略模板复用 `BackupPolicyDto`，内置模板 ID 固定为 `default`、`important`、`high-frequency`、`exit-only`、`manual-only`；用户模板 ID 必须以 `custom-` 开头。模板应用是一次性复制，不建立继承关系。
- `BackupPolicyTemplateCatalog.ClonePolicy` 是模板的安全边界：周期间隔限制在 1–1440 分钟，保留值不小于 0，所有模板都强制关闭自动恢复。内置模板由 Worker 初始化幂等播种，禁止通过 IPC 修改/删除。
- Save Center 的模板区位于既有策略页滚动内容内，未新增页面、未改变 DataGrid/虚拟化骨架；创建副本时先保存当前选择再清空选择，避免名称丢失。
- Playnite 包必须同时包含 `GameSaveCenter.Core.dll` 与 Worker 的 self-contained Windows runtime；`scripts/package.ps1` 会验证 Core、hostfxr/hostpolicy/coreclr/System.Private.CoreLib 和 `includedFrameworks`，Worker 项目保持 `RuntimeIdentifiers=win-x64`，发布使用单节点/无 node reuse 参数。
- 当前自动验证：Core 29/29、Worker 69/69、Playnite 197/197；Worker/Playnite Release 隔离构建 0 警告/0 错误；source/XAML/WPF 门禁与 render-qa 通过；最终 `.pext` 打包成功。真实 Playnite 日志曾确认插件加载，但旧 Worker PID 3896 仍锁住用户安装目录，完整 Worker/IPC/UI 仍标记为 `MANUAL QA REQUIRED`，不能以隔离首启未进入扩展阶段冒充真实宿主通过。
- 以后每个代码阶段的验收顺序固定为：`dotnet test/build` → 源码/XAML/WPF/render-qa → `scripts/package.ps1` → 安装包内容断言 → 启动 Playnite 并检查 `ExtensionFactory`/扩展日志；若宿主被单实例或权限环境阻断，必须记录为人工验收，不得宣称加载成功。
- 本阶段完成后的下一项为 `ONBOARDING-001`；不要重做 Restore Readiness、Health、Protection 或本阶段策略模板。

## 一键安装器进程停止与权限约束

- `DEV-INSTALL-007` 允许可信 Playnite 候选为空，避免 PowerShell 将空数组绑定到停止函数时直接失败。没有运行中的 Playnite 时，安装器仍使用 `%APPDATA%\Playnite\Extensions`（或显式 `-PlayniteExtensionsPath`）完成安装；以后需要自动启动时应通过 `-PlayniteExecutable` 指定便携版/自定义目录中的 `Playnite.DesktopApp.exe`。
- `scripts/dev-install-run.ps1` 的 `Stop-PlayniteAndOwnedWorkerReliably` 必须先允许 Playnite 正常退出并等待插件回收 Worker，再处理残留；不能把 `Get-Process` 与停止之间的退出竞态误报为失败。
- 安装器不应默认请求管理员权限，也不应按进程名广泛终止 Worker。`DEV-INSTALL-004` 先调用 Playnite 的正常窗口关闭，让插件既有 `OnApplicationStopped`/`WorkerLauncher.StopOwnedWorker()` 回收自己创建的 Worker；只有 Playnite 已退出后仍存在、且路径明确属于当前扩展目录的残留 Worker 才可处理。
- 路径不可读取或残留 Worker 属于其他扩展时必须停止安装并要求用户手动处理，不能为了自动化验证提权或误杀其他用户进程。根目录入口同步检查 `DEV-INSTALL-004`，避免旧副本继续运行已经废弃的提权逻辑。
- `DEV-INSTALL-006` 补齐 Playnite 自身的无窗口残留：先等待 20 秒正常退出；仅对当前会话、精确可信路径且 `MainWindowHandle=0` 的实例执行强制结束，并把 `Refresh` 与停止之间的自然退出视为成功。不得退化为按进程名批量终止。

## 2026-08-12 Worker 生命周期清理补充

- Playnite 插件退出必须调用 `WorkerLauncher.StopOwnedWorker()`；Launcher 只允许停止当前实例记录的 `runningWorker`，不能按名称终止任意 `GameSaveCenter.Worker`。`shutdownRequested` 防止退出竞态重新启动子进程。
- 本阶段 `3f05e16 fix: stop owned worker on Playnite shutdown` 已通过 Playnite Release 全量 198/198、源码校验、Release 编译和 Release self-contained 包验证。
- 隔离 Playnite 的 `--userdatadir` 首次启动会停在 `FirstTimeStartupWindowFactory`，不能据此宣称扩展加载；真实宿主验证仍必须看 `ExtensionFactory:Loaded plugin: GameSaveCenter` 与扩展日志，并记录 `MANUAL QA REQUIRED` 直到用户环境实际通过。

已完成：见 WORKLOG.md 与 Git log；不要重复实现已完成的 UI/性能工作。

## 已知坑

- WPF `ICollectionView.Refresh()` 昂贵；不要在每个按键或每次快照都调用。
- `ObservableCollection` Reset 仍会触发 CollectionView 重建；数据没变时应跳过（PERF-005）。
- 动态 ComboBox Items 重建会清空 SelectedItem；要显式恢复逻辑默认值。
- 大库启动不要同步全量匹配/扫描；先渲染 SQLite 缓存。
- Worker 是独立进程：Playnite 启动早期 IPC 可能超时，要用失败快速降级 + 后台重试。
- 修改器/CT/自定义工具启动一律走 Worker；禁止在 Playnite UI 进程直接 Process.Start 外部程序。
- CloseOnGameExit 只能关闭本 Session 由 GameSaveCenter 启动且能确认 PID/StartTime 的进程；脚本（BAT/CMD/PS1）与系统默认程序打开的文件不可靠，UI 对这类入口禁用开关。
- 自定义启动项支持 EXE/LNK/BAT/CMD/PS1/普通文件：EXE 与导入/重定位时已解析并持久化的 LNK→EXE 目标可跟踪；未解析的 LNK、脚本和系统默认程序启动时 Trackable=false。
- 磁盘 IO、图片解码不要放 UI 线程；图片解码要限制并发并 freeze。
- 表格/列表虚拟化很容易被外层 ScrollViewer 或 DataGrid 嵌套破坏，改 XAML 后必须跑 render-qa。
- DataGrid 不要写死运行时 `Height`，用 `MinHeight/MaxHeight` 保持有限 viewport；`Pixel ScrollUnit` 已在真实 Playnite 验证会回归（轻微滚动即大空白），当前必须保持 `Item` + 稳定行样式，禁止重新改回 Pixel。
- `git push` 前确认没有 bin/obj、用户本地配置、密钥、测试临时文件和大压缩包（如 `GameSaveCenter.7z` 不要提交）。

### ONBOARDING-001 不可丢失约束

- 首次使用状态由 Playnite `GameSaveCenterSettings.OnboardingCompleted` 持久化；未完成时 Dashboard 首次打开定位 Maintenance，用户可“跳过首次检查”，之后仍可手动重新运行环境检查。
- 环境检查只允许读取、创建/删除自身临时探针和只读远端列举；禁止自动备份、上传、同步、删除或覆盖真实存档。测试备份必须由用户明确点击，并且要求当前游戏已匹配且 Ludusavi 可用。
- 真实宿主验收必须看到 `ExtensionFactory:Loaded plugin: GameSaveCenter` 与扩展日志；隔离 Playnite 只能证明进程启动，不可替代真实安装验证。当前安装器不再默认请求 UAC；用户桌面应确认普通双击入口即可完成“关闭 Playnite → 回收 Worker → 构建安装 → 启动 Playnite”链路。

## 文档导航

- `docs/DEVELOPMENT_HANDOFF.md`：跨电脑/跨模型交接入口，包含每轮 UI 基线。
- `docs/PROJECT_MEMORY.md`：长期不可丢失约束与 UI 决策历史（大文件，按章节检索）。
- `docs/DEVELOPMENT_PROGRESS.md`：按 UI 编号的实施历史与下一步线索。
- `docs/design/APPLE_WPF_IMPLEMENTATION_PROMPT.md`、`docs/design/UI_CHANGE_GATE.md`：UI 方向与门禁。
- `.codex/skills/wpf-apple-desktop-ui/SKILL.md`：WPF/Playnite UI 专项技能。
- `docs/ai/WORKLOG.md`：每阶段开发流水记录。
- `docs/ai/PERFORMANCE_BASELINE.md`：性能基线与测量方法。

## 2026-08-13 UI-QA-REAL-002 当前事实

- 首页“今日工作台”状态区已改为标题下方全宽第二行，避免 2K 最大化时原窄列 `WrapPanel` 将状态胶囊挤成不可见/竖向圆点；`OverviewView.xaml.cs` 会根据实际或估算宽度调整英雄卡内边距。
- 维护中心首次使用环境检查复用现有检查项，采用响应式 `UniformGrid`：可用宽度 ≥ 900 DIP 为 3 列，620–899 DIP 为 2 列，更窄为 1 列；检查卡统一拉伸并设置最小高度，避免胶囊乱序和不规则空洞。
- 设置页共享文本框内容宿主已垂直居中，默认高度 42 DIP；设置分类卡片的共享模板增加底部安全间距并使用一致圆角，避免窗口底部裁切。
- 本阶段没有新增主导航页面，没有改动业务命令、绑定、DataGrid 虚拟化或 Worker/恢复体系；渲染夹具仅补充首次使用卡片的演示数据。
- 当前自动化基线为 Core 42/42、Worker 117/117、Playnite 206/206；Release 构建无警告/错误，五种窗口尺寸的 `render-qa` 通过。
- 2026-08-13 真实开发安装完成，Playnite 扩展日志确认 `GameSaveCenter 0.6.70.0` 加载，Worker 正常运行。真实宿主日志/进程证明已记录；用户实际 2K 最大化、主题/DPI、Settings 连续缩放仍标记 `MANUAL QA REQUIRED`。
- 本阶段只完成用户反馈的三类 UI 修复，不引入第 15 个功能；下一步等待人工视觉反馈。

## 2026-08-13 UI-QA-REAL-003 当前事实

- 首页“最近 30 天玩过的游戏”风险卡片的两个操作按钮已经移到摘要下方的独立响应式行，避免 2K 或右侧窄栏中按钮与标题、统计文本挤压。
- 修改器中心启动延迟编辑器现在明确显示“启动延迟”和“秒”，继续绑定 `SelectedGameTool.LaunchDelaySeconds`，输入框高度收敛为 34 DIP。
- 媒体中心 `MediaGrid` 显式使用顶部对齐的虚拟化面板和内容对齐，已修复表头/筛选区下方到首条媒体记录之间的大段空白；未修改列表的虚拟化滚动模型。
- 当前自动化基线为 Core 42/42、Worker 117/117、Playnite 209/209；Release 构建无警告/错误，五种窗口尺寸的 `render-qa` 通过。
- 2026-08-13 一键开发安装完成，Playnite 扩展日志确认 `GameSaveCenter 0.6.70.0` 加载，Worker 进程从当前扩展目录运行。
- `AUTO VERIFIED` 仅覆盖自动化、渲染、安装和真实宿主日志；用户实际 2K 最大化、主题/DPI、连续缩放及真实媒体数据滚动仍为 `MANUAL QA REQUIRED`。
- 本阶段只补充用户反馈的三个布局问题，不新增主导航页面，不改变业务绑定或 Worker/恢复体系；下一步等待人工反馈。

## 2026-08-13 UI-QA-REAL-004 当前事实

- 设置页左侧分类卡的“底部/边缘圆角被削掉”根因已确认是 `SettingsHeaderScroller` 的滚动条占用内容宽度，固定 232 DIP 的 `TabItem` 被 viewport 裁切，不是 CornerRadius 数值失效。
- `SettingsHeaderScroller` 已扩展到 248 DIP，分类 `TabItem` 仍保持 232 DIP 内容宽度；滚动条出现时为卡片边缘预留安全区，分类卡继续使用 14 DIP 圆角并开启自身边界裁剪。
- 设置页在可用高度低于 760 DIP 时使用更紧凑的 60 DIP 分类卡和 8 DIP 间距；左右设置滚动面保留底部安全留白，避免最后一项在宿主 viewport 边缘被直接截断。
- 当前自动化基线为 Core 42/42、Worker 117/117、Playnite 210/210；Release 构建无警告/错误，五种窗口尺寸的 `render-qa` 通过。
- 2026-08-13 一键开发安装完成，真实 Playnite 扩展日志确认 `GameSaveCenter 0.6.70.0` 加载，Worker 进程从当前扩展目录运行。
- `AUTO VERIFIED` 仅覆盖自动化、渲染、安装和真实宿主日志；用户实际 2K/DPI 设置页最终视觉仍为 `MANUAL QA REQUIRED`。
- 本阶段只修复设置页现有分类卡和滚动 viewport 的裁切，不新增页面、不改变设置字段、绑定、保存语义或 Worker/恢复体系；下一步等待人工反馈。

## 2026-08-16 UI-206 Overview 页面级迁移事实

- UiLab 的关键骨架不是“右侧摘要从页面顶部开始”，而是顶部 Hero/当前游戏与六项指标占满整行，最近活动开始后才分成左主区与右侧风险/关注栏；生产 Overview 已按此层级重排。
- 生产右栏使用 330 DIP 固定宽度，宽屏与最近活动卡同一行起始（离屏探针偏移 0 DIP）；窄窗口由现有页面级 ScrollViewer 承载并把右栏下移。不要恢复成 1.2*/0.8* 的整页比例栏，也不要把 UiLab 演示滚动条迁入生产。
- 生产数据和行为保持真实：`Snapshot`、`RecentProtection`、`AttentionFindings`、OpenProtection/OpenAttention 命令、选择状态、虚拟化列表和页面滚动都未替换为 demo 假数据；UiLab 右上角颜色按钮没有迁移。
- 共享表头现在使用低对比度表头填充、8 DIP 圆角和 1/2 DIP 安全边距；普通活动行使用较弱 Divider，避免 DataGrid/活动表头看起来像尖锐矩形。
- 本阶段自动验证：源码/XAML 门禁通过，Playnite 303/303，生产 Release 0 warning/0 error，Overview 多尺寸与 Light/Dark 离屏渲染通过。全量 render harness 仍有 Save/Media 窄尺寸主表 `<236 DIP` 的历史门禁项，不能写成全量 render-qa 通过。
- 本阶段尚未完成真实 Playnite 2K/DPI/Follow/高对比度人工验收；后续优先在真实宿主检查页面级滚动、侧栏下移、键盘焦点和长中文文案，再继续迁移其他页面。

## 2026-08-17 AcrylicFork 全量页面重构事实

- 本轮确认没有任何提示词保护；先前外观不变的根因是生产页面与 AcrylicFork Demo 使用两套不同的页面树，同时旧 Dashboard 外壳重复渲染页面上下文和局部表格样式。
- 生产 Shell 现在只负责侧栏、Header/GameSwitcher、全局操作、PageHost 和 Footer；首页、媒体、任务、存档、修改器、维护页面继续持有真实 ViewModel/Command/Binding，Demo 顶部颜色按钮只作为主题令牌参考，生产滚动条保持项目实现。
- 首页与维护中心已经按 Demo 信息架构迁移；维护中心默认诊断概览显示六项健康卡、环境检查、诊断操作和完整摘要，发现问题表格通过独立问题列表 Tab 保留并验证。
- DataGrid 共享样式采用透明表头、稳定底部分隔线和明确文本对比度；不使用负 Margin、Canvas、透明占位或隐藏溢出来修复布局。首页零分母进度条折叠，关注入口提供可访问说明。
- 2026-08-17 自动事实：源码校验通过；WPF UI 静态校验 0 error；Playnite 测试 303/303；RenderHarness 全量 render-qa OK。真实 Playnite 宿主、DPI、Follow/高对比度、键盘和大库滚动仍需人工复核。
- `scripts/package.ps1` 已修复空 `dotnet` 参数问题；当前无 `BuildOutputRoot` 的标准打包流程可正常生成并校验安装包，且会保留 `GameSaveCenter.Contracts.dll`。
- 一键安装的隔离构建还必须把 `TEMP/TMP` 指向隔离输出盘；否则完整性测试会读取系统临时目录所在磁盘的真实剩余空间，在低于 512 MiB 时把健康夹具判为 `Warning`。
- 一键安装的隔离构建根目录使用短路径 `artifacts/gsc-b/<guid>`；过深的 `artifacts/dev-build/Release/<guid>` 会让 .NET Framework Playnite 测试适配器加载失败。当前完整 `dev-install-run.ps1 -NoStart` 已通过。

## 2026-08-18 UI-214 首页状态徽标事实

- 首页最近任务与全局活动徽标的文本必须显式设置 `HorizontalAlignment=Center` 和 `TextAlignment=Center`，不能依赖旧 Chip/Border 模板的默认测量。
- 风险与提醒徽标使用独立 `Border + TextBlock`，最小宽度 52 DIP、内边距 8/2，并通过 DataTemplate triggers 同时切换背景、边框和文字颜色；这样“风险/需关注/未知/已就绪”不会被裁切或错误显示为同一种颜色。
- 任务图标应引用 `GscAccentBrush` 等生产资源键，不能直接引用 Acrylic Demo 的裸 `AccentBrush`。
- 2026-08-18 Release 构建 0 warning / 0 error；Overview 离屏浅深色多尺寸探针通过。全量 RenderHarness 仍有既有 Media resize recovery 失败，不能标记全量通过。
- 本轮未重新进行 Playnite 宿主截图；屏幕控制此前已由用户物理 Escape 停止，离屏渲染结果不得替代真实宿主人工验收。
- 本轮重复的 Playnite 测试命令长时间无输出并被停止，不能据此宣称新增测试通过；保留上一阶段已记录的回归基线。

## 2026-08-18 UI-225 首页状态徽标实际宿主事实

- 首页纯文本状态徽标使用页面局部 `DataTemplate` 居中，不修改共享 Chip 的复杂内容承载方式；否则 Worker/Ludusavi 的 StackPanel 内容会被错误显示为控件类型字符串。
- 风险与提醒徽标位于独立 Grid `Auto` 列，游戏名称允许省略并提供 Tooltip，风险文本保留 72 DIP 最小宽度；这解决了名称挤压徽标和“风险”中文裁切。
- 最新 v28 生产包已经重新安装并在真实 Playnite 中启动。宿主截图明确显示：最近任务的“成功”徽标文本居中，风险徽标文本完整，侧栏设置项可见。
- 本阶段的 RenderHarness 与宿主验证只覆盖首页徽标修复；全量 RenderHarness 的 Media resize recovery 失败和 Playnite 迁移前结构测试失败仍是公开的后续工作项。
-
## 2026-08-18 UI-224 首页徽标与真实宿主截图核验事实

- 首页活动/任务状态徽标不能依赖 `LabSubCard` 的内边距和默认测量；固定徽标必须使用零内边距、固定宽度、子 `TextBlock` 显式 `HorizontalAlignment=Center` 与 `TextAlignment=Center`。当前生产首页的任务/活动徽标宽度为 58 DIP，风险徽标为 70 DIP。
- 风险徽标必须保留 Tooltip，中文状态不能用省略号替代；背景、边框和文字颜色继续由真实健康状态触发器驱动。
- 真实 Playnite 验收的证据要求提高：必须同时有 Playnite 日志中的 `ExtensionFactory:Loaded plugin: GameSaveCenter`、可识别的 Playnite 页面截图和必要时的交互结果。若 Computer Use 返回 `EmptyWindowAutomationPeer`、`MainWindowHandle=0` 或截图是其他桌面窗口，只能记录为“宿主已加载，视觉验证阻塞”，不得写成视觉通过。
- 2026-08-18 本轮 Release 编译/打包/安装通过，日志确认生产 DLL `0.6.70.0` 加载；但 Computer Use 截图不属于 Playnite 页面，真实宿主视觉验收仍为 `MANUAL QA REQUIRED`。Worker 两项环境状态测试和 Media resize recovery 两项 RenderHarness 失败继续保持公开记录。

## 2026-08-18 UI-226 首页短主题别名事实

- 生产 Acrylic 共享控件与 AcrylicFork Demo 共用一组短资源键：`AccentBrush`、`AccentHoverBrush`、`AccentPressedBrush`、`AccentStrokeBrush`、`AccentTintBrush`、`AccentTintStrongBrush`、`AccentWashBrush` 和 `TextOnAccentBrush`。
- 生产主题适配必须同时写入这些短键和 `Gsc*` 键；只写 `Gsc*` 会让 Playnite 宿主的未解析短键回退为黑色/透明，表现为任务图标、进度条、活动气泡和主按钮与 Demo 色彩不一致。
- 短键只能写入当前页面的局部 `ResourceDictionary`，不能修改 Playnite 全局资源；这样既能复现 Demo 的强调色层级，又不会污染宿主主题。
- 2026-08-18 已确认生产页面的实际资源注入缺口：`DashboardView` 之前只更新旧隐藏页面树，现已同时更新 `AcrylicProductionShellView` 及其 `PageHost` 页面实例；离屏深色 RenderHarness PNG 已确认首页最近任务图标/进度条、`全部` 链接、风险主按钮、全局活动分类和信息气泡恢复紫色或对应语义色。
- Playnite 日志确认新 DLL 已加载，但 Computer Use 返回 `EmptyWindowAutomationPeer`、`MainWindowHandle=0` 且截图不是 Playnite 页面，因此真实宿主视觉验收仍为 `MANUAL QA REQUIRED`，不得把离屏 PNG 写成真实宿主截图通过。全量 RenderHarness 仍有 Media resize recovery 两项失败。

## 2026-08-18 UI-227 媒体中心模式栏事实

- 媒体中心顶部模式栏已从灰色透明条切换为生产深色 `MediaModeStrip`，外层使用 `GscGlassStrongBrush` 与 `GscControlStrokeBrush`；选中 RadioButton 使用 `GscAccentTintStrongBrush`、`GscAccentBrush` 和 `GscSelectionTextBrush`。
- RadioButton 的三种模式、真实数据绑定、命令和项目自身滚动条没有改变；本次只修正页面级 Tab 承载样式，并确保悬停不会覆盖选中态。
- Release 构建与静态检查通过；RenderHarness 编译和主题/尺寸探针完成，但 Media resize recovery 仍有两项失败：回弹后 `MediaGrid` 尺寸不一致、`MediaInspectorScrollViewer` 从可见变为折叠。真实 Playnite 截图仍待可识别宿主窗口后复核。

## 2026-08-18 UI-228 页面基线回退修复事实

- `be5707d` 是一次页面基线回退：它把生产页面覆盖成“今日工作台 / 最近活动”架构，导致两台同步仓库的电脑同时显示数个版本前的页面。
- 当前恢复目标是 `be5707d^` 的 AcrylicFork 生产基线，首页必须包含“最近任务 / 全局活动 / 风险与提醒”；顶部 Demo 彩色按钮和 Demo 滚动条仍不迁移。
- 针对已撤销架构的 61 条 Playnite 源码契约断言必须保持显式跳过，不能用无业务意义的兼容控件让它们假通过；当前基线由 `RestoredAcrylicForkBaselineTests` 覆盖首页、存档、媒体和任务入口。
- 本轮验证：Release 0 warning / 0 error，Core 59/59，Worker 191/191，Playnite 246 通过、61 跳过、0 失败。真实 Playnite 宿主视觉仍需可识别窗口截图确认。

## 2026-08-18 UI-229 媒体模式栏交付验证边界

- 一键 Release 构建、Worker 发布和 Playnite 安装已重新通过，安装目录为 `%APPDATA%\Playnite\Extensions\GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec`，`extension.yaml` 为 `0.6.70`，生产 DLL 为 `0.6.70.0`。
- RenderHarness 不是 Playnite 真机截图；本轮工具上下文没有可用的 Playnite 鼠标/键盘控制，因此没有把离屏结果冒充真实宿主视觉验收。
- 当前提交只交付媒体模式栏颜色修正和对应基线断言，不代表首页、存档、修改器、任务、维护等页面已经完成 1:1 视觉迁移；Media resize recovery 两项失败仍是后续阻塞项。

## 2026-08-18 UI-230 首页风险列表滚动边界事实

- 首页风险卡片的两个可能增长列表必须使用独立的生产 `GscPageScrollViewer`：需关注列表与展开后的最近游戏保护明细均限制为 `MaxHeight=190`，垂直滚动 `Auto`，水平滚动 `Disabled`。
- 页面根滚动与风险列表内部滚动职责分离：普通首页内容由页面滚动承载，风险项超过视口后只在自身区域滚动，不能通过追加列表项把主页面高度无限撑大。
- 当前 Dashboard ViewModel 的需关注数据按真实严重度筛选后完整绑定，首页不再用前 4 条静默截断；保护明细继续使用真实 `RecentProtection.Items`。190 DIP 是 UI 层防护边界，不是业务截断替代品。
- RenderHarness 默认 fixture 没有使需关注列表溢出，因此其 `scrollable=false` 只说明当前 fixture 未超过 190 DIP；静态契约测试已强制检查滚动边界。不得据此宣称已完成真实 Playnite 滚动条交互验收。
- 首页清理了已撤销的“今日工作台”旧工具栏及其代码后置响应式引用；媒体中心模式栏继续使用生产控件底色和紫色选中色，不迁移 Demo 顶部彩色按钮或 Demo 滚动条。

## 2026-08-19 UI-231 首页风险项数量边界事实

- 首页“风险与提醒”及其关联明细必须采用有限视口：最近游戏保护明细、需关注事项列表分别使用生产 `GscPageScrollViewer`，`MaxHeight=190`、垂直滚动 `Auto`、水平滚动 `Disabled`。
- 数量很多时，风险明细在卡片内部滚动，不能把 Dashboard 根内容高度无限撑长；风险卡片本身不再包一层整卡滚动，避免双滚动条。
- 该视口不替代业务数据：真实绑定仍保留，当前展示条数限制只由现有 ViewModel 业务规则和列表视口共同决定。
- 真实 Playnite 截图验收仍未完成：本轮插件已由日志确认加载，但控制接口返回 `EmptyWindowAutomationPeer` 且截图错指 Codex 窗口；后续不得将 RenderHarness 或错误窗口截图称为宿主视觉通过。

## 2026-08-19 UI-232 首页风险区域回归事实

- 首页风险列表的正确边界是列表级有限视口，而不是整张风险卡片或首页根容器无限增长：`OverviewAttentionScrollViewer` 与 `OverviewProtectionItemsScrollViewer` 均使用 `GscPageScrollViewer`、`MaxHeight=190`、垂直 `Auto`、水平 `Disabled`。
- 首页宽布局的右侧风险栏必须与 `OverviewRecentActivityCard` 同行并跨越最近任务/全局活动两行；RenderHarness 现在直接按该卡片比较，避免使用整页滚动面导致错误告警。
- 2026-08-19 RenderHarness 已确认高数量风险探针在 190 DIP 视口内滚动，宽布局右侧栏偏移为 `0 DIP`；这只是离屏渲染验证，不等于 Playnite 真机视觉验收。
- 本轮 Playnite 测试命令因长时间无输出和低 CPU 子进程空转被停止，不能写成测试通过；后续需使用可完成的测试入口重新验证。

## 2026-08-19 UI-233 首页风险列表与 Demo 行结构事实

- 首页风险区域的稳定方案是列表级有限视口：`OverviewAttentionScrollViewer` 和 `OverviewProtectionItemsScrollViewer` 使用项目 `GscPageScrollViewer`，`MaxHeight=190`、垂直 `Auto`、水平 `Disabled`。风险数量很多时只滚动列表内部，不能让风险项把首页根高度无限推长；不要再给整张风险卡片叠加一层滚动。
- RenderHarness 的溢出探针已经确认 190 DIP 视口在 1040×700、1100×720 下保持固定且 `scrollable=True`；普通 fixture 未溢出时的 `scrollable=False` 只能表示当时数据不足，不表示没有滚动配置。
- 首页最近任务和全局活动已按 Demo 行结构重排：任务的类型/游戏名、详情/进度、结果/时间分别分层；活动不再保留额外表头和图标列，分类徽标文本使用紫色主题令牌并显式居中。
- 媒体模式栏的外层表面使用 `GscAccentTintBrush`，内部选中状态使用更强的紫色令牌；顶部 Demo 彩色按钮、Demo 滚动条仍不迁移。
- 仅安装裸 .NET SDK 的机器可能缺少 Workload Resolver 目录；`scripts/build.ps1` 和 `scripts/render-qa.ps1` 通过 `MSBuildEnableWorkloadResolver=false` 兼容本项目的 .NET Framework/WPF 构建，不代表项目依赖任何 SDK Workload。
- 2026-08-19 完成一次可复现验证：Playnite UI 测试 248 通过、61 跳过、0 失败；Playnite 与 RenderHarness Release 编译 0 警告/0 错误；RenderHarness 全量 `render-qa OK`。真实 Playnite 宿主视觉截图仍未完成，离屏证据不能替代宿主验收。

## 2026-08-19 UI-236 风险视口和紫色状态验证事实

- 首页“风险与提醒”必须限制列表视口，而不是限制业务集合：`OverviewAttentionScrollViewer` 与 `OverviewProtectionItemsScrollViewer` 使用 `GscPageScrollViewer`、`MaxHeight=190`、垂直滚动 `Auto`、水平滚动 `Disabled`；数量增加时主页面高度保持稳定，列表内部出现项目现有滚动条。
- 首页最近任务、媒体来源和可下载版本的长文本使用有限 Grid 测量、`CharacterEllipsis` 和 Tooltip，防止标题挤压状态徽标、按钮或 Inspector。
- 媒体中心模式栏使用更明确的 `GscAccentTintStrongBrush` 紫色生产资源；该资源变更已同步源码契约测试，Demo 顶部颜色按钮和 Demo 滚动条仍未迁移。
- 重新编译测试后 Playnite 测试为 248 通过、61 跳过、0 失败；之前 38 项失败来自旧测试二进制，不是当前源码结果。
- RenderHarness `render-current3` 全量 `render-qa OK`，但仍属于离屏证据；如果屏幕控制返回 `EmptyWindowAutomationPeer` 或错误窗口，必须记录为宿主视觉阻塞，不能写成 Playnite 真机验收通过。

## 2026-08-19 UI-237 Release 安装事实和宿主视觉边界

- 当前 `main` 的 `37ab9a6` 已完成 Release 一键安装；安装目录为 `%APPDATA%\Playnite\Extensions\GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec`，`extension.yaml` 为 `0.6.70`，DLL 为 `0.6.70.0`。
- 本轮 Release 验证结果为 Core 59/59、Worker 191/191、Playnite 248 通过/61 跳过/0 失败；安装报告保存在 `artifacts/last-dev-install.txt`。
- 首页“风险与提醒”按列表级有限视口实现：两个风险列表使用项目 `GscPageScrollViewer`、`MaxHeight=190`、垂直 `Auto`、水平 `Disabled`。风险数量增加时只在列表内部滚动，不会无限增加首页高度。
- 真实 Playnite 截图验证仍未通过：Computer Use 唯一返回的窗口标题是 Playnite，但截图内容是其他桌面窗口。此类结果只能记为宿主视觉阻塞，绝不能宣称页面已在 Playnite 中 1:1 验收。

## 2026-08-19 UI-238 首页风险与提醒视口事实

- 首页“风险与提醒”现在有独立的 `OverviewRiskViewport`，使用生产 `GscPageScrollViewer`，最大高度为 `330 DIP`，垂直滚动 `Auto`、水平滚动 `Disabled`。风险数量增加时，首页主内容高度不再被风险条目无限撑大。
- 风险区标题、说明和底部“打开维护中心”按钮位于外层视口之外；展开的最近游戏保护明细仍保留 `OverviewProtectionItemsScrollViewer` 的 `190 DIP` 内部视口。前者限制整个风险提醒栏，后者限制展开明细列表，不是无意义地叠加两个相同滚动条。
- `OverviewRiskScrollViewer` 仍然是兼容 `Panel` 节点，真实 `AttentionFindings` 与 `RecentProtection.Items` 绑定不变；不要把兼容节点直接改成同名 `ScrollViewer`，否则会破坏响应式代码和源码契约测试。
- 2026-08-19 已用单节点测试入口完成 Playnite 249/61、Core 59/59、Worker 191/191；RenderHarness 全量 `render-qa OK`，但这些结果仍不能替代可识别 Playnite 窗口的真实宿主截图。

## 2026-08-19 UI-239 媒体中心结构基线事实

- 媒体中心 Demo 的顶部结构是四张独立指标卡，下面是共享紫色分段 Tab；不能把统计数字、模式 RadioButton 和 Tab 再混合到一个横向条带中。
- 当前生产 `MediaCenterView` 使用 `UniformGrid MediaSummaryPanel` 承载四张 `GscRedesignMetricBorder` 卡片，宽度按 4/2/1 列响应式重排；`MediaTabControl` 基于 `GscRedesignWorkspaceTabControl`，其选中项使用生产紫色强调令牌。
- `MediaModeStrip`、`MediaModeRadio`、`MediaContentTabs` 和 `OnMediaModeChecked` 已从生产媒体页移除；真实 Tab 内容、Binding、Command、虚拟化列表和项目滚动条保持不变。
- 2026-08-19 的 Release RenderHarness 已确认媒体三 Tab 在浅色/深色、多尺寸和缩放过渡下可渲染；离屏 PNG 只能证明结构和布局探针通过，不能替代 Playnite 真机截图。

## 2026-08-19 UI-240 首页风险展开态验证事实

- 首页风险侧栏固定为 410 DIP 宽；`OverviewRiskViewport` 根据窗口高度在 500–720 DIP 之间限制，风险总列表使用生产滚动条，避免风险数量把首页无限撑高。
- 展开“最近游戏保护明细”时，隐藏同一数据源的只读预览列表；明细列表使用独立 300–420 DIP 视口。明细卡片按游戏名、状态、换行说明、查看操作纵向布局，原有选择和命令 Binding 不变。
- 2026-08-19 在新安装的真实 Playnite 窗口 `10621340` 中，展开后重新滚动并获取新截图，确认重复预览已隐藏，完整明细卡片和“查看”操作可见且无重叠。该证据仅覆盖首页风险侧栏，不代表其他页面完成宿主视觉验收。

## 2026-08-19 UI-241 任务中心搜索栏事实

- 任务中心搜索输入区不再使用独立的“搜索任务…”标签列；提示文字与搜索图标在输入框内部，`TaskSearchTextBox` 仍绑定真实 `TaskSearchText`。
- 桌面布局让搜索区占据筛选栏剩余宽度；紧凑布局时搜索区独占第一行，状态、类型、刷新在第二行，避免再次出现输入框被压成窄条或控件重叠。
- 2026-08-19 Release 构建、Core 59/59、Worker 191/191、Playnite 250/61/0 通过；安装已成功。但最终宿主截图验证被用户物理 Escape 中止，不能把本轮写成 Playnite 视觉验收完成。

## 2026-08-19 UI-242 真实宿主搜索栏复核事实

- 已重新启动并绑定真实 Playnite 生产窗口，确认截图目标为生产 `GameSaveCenter`，不是 AcrylicFork Preview。
- 任务中心真实宿主截图确认：搜索提示和搜索图标位于 `TaskSearchTextBox` 内部，输入框占据筛选栏剩余宽度；状态、类型和刷新控件各自保持独立边界。
- `scripts/validate-source.py` 已按当前 `OverviewActivityList` 的真实 Grid/页面滚动宿主结构修正有限视口判断，避免静态门禁把合法布局误报为无限测量。
- 本次真实宿主证据只覆盖首页入口和任务中心搜索区；媒体、存档、修改器、维护和首页风险展开态仍必须逐页截图复核，不能把离屏 RenderHarness 或单页截图写成全量 1:1 完成。

## 2026-08-19 UI-243 当前视觉验收边界

- 任务搜索提示已经和输入框合并；媒体页局部 Tab 已恢复共享紫色分段样式；首页风险侧栏和保护明细使用有限视口，保护明细采用纵向可读卡片。
- 本轮 Release 安装和 Core/Worker/Playnite 测试通过，但 Computer Use 未取得可识别 Playnite 窗口；离屏渲染、安装清单和测试不能替代宿主视觉验收。
- 后续逐页截图必须在同一 Playnite 宿主同时打开生产扩展和 AcrylicFork Preview，分别记录窗口、页面、分辨率、主题和滚动位置；若截图目标不是 Playnite 页面，立即记为阻塞并释放控制。

## 2026-08-20 UI-244 表头前景与媒体摘要卡事实

- 最近任务和任务中心表头不能只设置 `Foreground`：WPF 的 `DataGridColumnHeader` 内容还可能通过 `TextElement.Foreground` 继承宿主默认黑色。共享表头、表头呈现器和任务局部表头现在同时显式绑定生产主题文本令牌，首页任务模板的标题、游戏名、详情和结果也有明确前景色。
- 媒体中心四张摘要卡使用共享 `GscRedesignMetricBorder`，统一采用紧凑内边距、72 DIP 最小高度、14 DIP 圆角和 24 号数字；卡片仍由 `UniformGrid` 等宽承载，不混入 Tab 或来源规则布局。
- 2026-08-20 RenderHarness 最终报告 `artifacts/ui-qa/phase-home-media-cards-final/render-qa-report.txt` 为 `render-qa OK`，覆盖浅色/深色、多窗口尺寸和回弹过渡；Core 59/59，Playnite 251 通过、61 跳过。该证据属于离屏渲染，不能替代可识别 Playnite 宿主的逐页截图。

## 2026-08-20 UI-254 设置页分类栏与任务页 Demo 骨架事实

- 生产设置入口文件是 `src/GameSaveCenter.Playnite/Settings/GameSaveCenterSettingsView.xaml`，不是 `Views/SettingsView.xaml`。当前结构必须保持 `SettingsWorkspace` 的 190 DIP 分类栏、16 DIP 间距和右侧 `SettingsScroller`；分类 ListBox 名称是 `SettingsSectionTabs`，事件是 `OnSettingsTabSelectionChanged`。
- 设置页五个可见面板分别是 `SettingsGeneralPanel`、`SettingsBackupPanel`、`SettingsAppearancePanel`、`SettingsAutomationPanel`、`SettingsMigrationPanel`。切换只改变 `Visibility`，不得把真实字段 Binding、Validation、Playnite 保存按钮语义或导入/导出命令移入 Mock 数据。
- 设置页常见 1040px 逻辑窗口仍使用左侧分类栏；`ApplyResponsiveLayout` 的极窄分支为 `layoutWidth < 560`，窄标题阈值为 `layoutWidth < 520`。常见窗口必须让右侧 `GscPageScrollViewer` 获得有限视口，不能让五项分类栏占满第一屏。
- RenderHarness 的 ListBox 分段入口发现规则同时接受名称以 `SegmentTabs` 结尾的迁移页和生产设置的 `SettingsSectionTabs`；设置布局探针验证五个 `ListBoxItem` 可见可测和右侧内容视口，不再查找旧 `SettingsHeaderScroller`/TabControl。
- 当前 `artifacts/ui-qa/task-settings-final/render-qa-report.txt` 为 `render-qa OK`；这是离屏证据。Playnite 生产宿主 Light/Dark、Follow、DPI、键盘焦点与逐页真实截图仍需单独人工验收。

## 2026-08-20 UI-255 共享工作区表格事实

- `Themes/Redesign.xaml` 的 `GscRedesignWorkspaceDataGrid` 是 Save/Media/Maintenance/Task 四个提取页的显式 LabGrid-like 行为基类，集中保护 `RowHeight`/`ColumnHeaderHeight`、FullRow 单选、列宽调整、排序、`VirtualizingPanel.ScrollUnit=Item`、Recycling、行/列虚拟化和 Auto 内部滚动。
- 页面样式可以继续覆盖 `RowStyle`、`ColumnHeaderStyle`、Background 和媒体专用表头，但不能恢复各页复制一整套表格行为 setter 的分叉模式；新增工作区表格应优先基于该 key，并补充源契约测试。
- `GscCodeFontFamily` 当前为 `Cascadia Mono, Consolas, Microsoft YaHei UI`；维护诊断摘要已使用该 token。业务 Expander 已统一采用 `GscDisclosureCard`，当前没有引入 Demo 滚动条。
- 新自动证据：`artifacts/ui-qa/shared-grid-contract-final/render-qa-report.txt` 为 `render-qa OK`；Release 0 warning/0 error；Core 59/59；Worker 190/190（排除 Soak）；Playnite 251 通过/61 跳过/0 失败；WPF validator 0 error/20 warnings/161 info。
- 本阶段仍不能声称真实 Playnite 宿主逐页验收完成；宿主截图、Follow/高对比度、DPI、键盘/UI Automation、真实长文案和大数据量滚动仍是后续人工边界。

## 2026-08-20 UI-256 共享按钮与反馈资源事实

- Dashboard 的 Toast/Dialog 视觉资源现在位于 `Themes/Redesign.xaml`：`GscRedesignFeedbackToastCard`、`GscRedesignFeedbackDialogCard`、对应遮罩和文字样式；页面代码只负责真实事件、状态、动画、计时器和完成结果。
- Dashboard 不再声明与 `DesignTokens.xaml` 重复的原生 `GscButtonBase`/`GscPrimaryButton` 模板；Toast 关闭/详情按钮和确认 Dialog 按钮复用全局按钮契约。页面级 `ui:Button` 继续使用 `GscWpfUiToolbarButton`、`GscWpfUiActionButton`、`GscWpfUiContextButton` 等共享语义样式，不能新增局部按钮模板解决单页问题。
- `UiNotificationRequested`、`UiConfirmationRequested`、`UiChoiceRequested` 的事件与安全完成逻辑保持不变；设置页导入报告/错误仍使用原生 `MessageBox`，这是为避免 Playnite 共享 Window 中 Window-wide WPF-UI host 冲突的有意边界。
- UI-256 自动证据：XAML 18 文件通过，源码门禁通过，Release 0 warning/0 error，Core 59/59，Worker 190/190（排除 Soak），Playnite 252/61/0，`artifacts/ui-qa/feedback-surfaces-final/render-qa-report.txt` 为 `render-qa OK`，WPF validator 0 error/20 warnings/161 info。
- 仍未完成真实 Playnite 宿主逐页视觉验收；不要把 RenderHarness 的反馈资源加载或 PNG 结果写成 Playnite Light/Dark/Follow、DPI、高对比度和键盘/UI Automation 已验收。

## 2026-08-20 UI-257 首页有限视口事实

- `OverviewStackScrollSurface` 保持现有生产页面滚动条和 `HorizontalScrollBarVisibility=Disabled`；`OverviewLayoutGrid` 必须绑定 `ViewportWidth` 并使用有限宽度，否则 WPF 的无限横向测量会让星号列按内容期望宽度增长，裁切当前游戏卡片和真实按钮。
- 当前首页响应式证据：`artifacts/ui-qa/overview-responsive-ui257/render-qa-report.txt`。1366×768 的 workspace 为 1042 DIP，Hero 为 506 DIP、当前游戏卡片为 x=520..1026，操作按钮高度均为 38 DIP；1600×900 同样无横向溢出。RenderHarness 仅是受控 WPF 证据，不等同 Playnite 嵌入视觉验收。
- UI-257 最终门禁：XAML 18/18；Release 0 警告/0 错误；Core 59/59；Worker 191/191；Playnite 256/318（62 跳过）；`validate-source.py` 通过；WPF 静态审查 0 error、20 warnings、146 info。
- 三次真实宿主审计均确认生产扩展 0.6.70.0 可加载并读取真实数据，最新受控证据位于 `artifacts/ui-host-audit-ui257-final`；但 Playnite 返回 `EmptyWindowAutomationPeer`，未能取得可识别的嵌入页面像素截图。不得把受控窗口截图写成 Playnite 1:1 完成，七页 Demo-first 总目标仍处于进行中。

## 2026-08-20 UI-258 生产宿主七页人工嵌入事实

- 本轮已在真实 Playnite 生产扩展 `GameSaveCenter 0.6.70.0` 中打开七个目标页面；生产壳标题为 `GameSaveCenter 生产版`，当前游戏为 `Bongo Cat`。这是真实嵌入窗口的人工 Computer Use 复核，不是离屏或受控窗口截图。
- 首页、存档、媒体、任务、修改器、维护均从生产壳左侧导航实际进入。首页的当前游戏卡片和操作按钮完整可见；存档的立即备份/全部备份与四个标签可见；媒体显示 30 项、5.76 MiB、待归类 4468 项；任务显示 50 条任务、0 运行中、16 需关注、34 今日完成；修改器显示 Wo Long 与 Yakuza 3 工具及右侧工具设置；维护显示进程映射和诊断页。
- Media Inbox 已实际进入并选中截图，独立 Inspector 滚动后可见预览、归类游戏 ComboBox、“确认归类”和“忽略并保留副本”。本轮不执行这些动作，因此没有改变真实数据。
- 设置通过 Playnite 游戏右键菜单的 `GameSaveCenter → 打开设置` 实际打开，显示 `GameSaveCenter 设置` 的“常规与目录”页面及 Worker、Ludusavi、存档目录字段；关闭时未保存更改。
- 自动审计事实仍不变：Playnite 主窗口的 UIAutomation 树是 `EmptyWindowAutomationPeer`，脚本没有 `summary.json` 的嵌入逐页像素证据；人工截图可证明真实页面能进入和关键控件可达，但不能替代自动门禁，也不能外推到其他 DPI、主题/Follow、高对比度或完整操作回归。

## 2026-08-20 UI-259 媒体收件箱共享虚拟化事实

- `MediaInboxGrid` 现在只保留页面需要的 `ScrollUnit=Item` 与顶部对齐，行/列虚拟化和 `VirtualizationMode=Recycling` 统一从 `GscRedesignWorkspaceDataGrid` 继承；禁止在媒体实例上恢复 `Standard` 或关闭列虚拟化。
- `tests/GameSaveCenter.RenderHarness/Program.cs` 的 `Media-Inbox` 探针使用 60 项真实形状的 `MediaItemDto` 夹具，覆盖 287/311/337/353/419 DIP 视口与 0/25/50/75/100% 滚动位置，检查 Recycling、列虚拟化、首行无 phantom gap 与末行可达。
- UI-259 证据：`artifacts/ui-qa/media-virtualization-fix/render-qa-report.txt` 为 `render-qa OK`；Release 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 256/318（62 跳过）；WPF validator 0 error、19 warnings、146 info。
- 该阶段未改变真实媒体绑定、Inspector 或归类/忽略/保留副本命令；真实宿主七页人工证据沿用 UI-258，不能把本轮离屏探针写成新的 Playnite 视觉截图。

## 2026-08-20 UI-260 存档页标题必须跟随真实当前游戏

- 生产壳 `AcrylicProductionShellView.xaml.cs` 不得保留 Demo 游戏名；存档页副标题必须由 `SelectedGame.Name` 生成，空选择使用“未选择游戏”。
- `UpdatePageHeader` 同时由工作区切换和 `DashboardViewModel.SelectedGame` 属性变更调用，保证当前游戏选择器改变后标题副文案不会滞后。
- UI-260 安装验证：XAML 18/18、Release 0 warning/0 error、Core 59/59、Worker 191/191、Playnite 257/319（62 跳过）；定向契约 14/14。
- 真实 Playnite 修复前复核已捕获 `Bongo Cat` 选择器与 `Elden Ring` 存档副文案不一致；修复后安装已完成，但重启后的 Computer Use 窗口暂时不可捕获，因此不把修复后截图写成宿主像素证据。
- GSC-086 常规宿主滚动复核已完成（4468 条媒体收件箱数据，顶部/中部/底部/快速滚轮/返回顶部无白色空视口）；DPI、窗口缩放、Follow/高对比度、键盘焦点和真实业务操作仍是人工边界。

## 2026-08-20 UI-261 工作区 Tab 栏视觉例外

- Demo-first 视觉基准不覆盖生产页 Tab 栏：用户明确要求继续使用项目当前 Tab chrome，因为它比 Demo 的外层连续分段胶囊更合适；后续迁移不能把该页签视觉重新替换为 Demo 样式。
- `GscRedesignWorkspaceTabControl`/`GscRedesignWorkspaceTabItem` 已在共享 `Themes/Redesign.xaml` 中恢复项目原有的透明 header 带、横向 HeaderScrollViewer、11 DIP 独立圆角页签、选中强调色、焦点视觉和内部 8 DIP 防裁切槽。页面仍保留 Demo 的周边布局以及真实 TabControl/TabItem、内容 Stretch、绑定和命令。
- Save、Media、Maintenance 的顶层页签和维护页内部页签均通过共享契约；不要在单页 XAML 复制一套 TabControl 模板来绕开该例外。
- RenderHarness 的 `SnapshotLayoutMetrics` 对重复模板部件名按出现顺序添加 `#2` 等稳定后缀，解决维护页嵌套 TabControl 的合法同名 `HeaderScrollViewer` 导致 `ToDictionary` 重复键的问题。
- UI-261 证据：源码/XAML/差异检查通过，定向契约 15/15，`artifacts/ui-qa/project-tab-chrome-rollback/render-qa-report.txt` 为 `render-qa OK`；代表离屏截图已确认 Save/Media/Maintenance 的项目 Tab chrome。Tab 回滚后的完整安装也通过：Release 0 warning/0 error、Core 59/59、Worker 191/191、Playnite 258 通过/62 跳过、安装 0.6.70/DLL 0.6.70.0；WPF validator 0 error、19 warnings、161 info。
- 真实宿主重装后的稳定前台截图仍缺失；不要将离屏证据扩写为 Playnite 1:1 验收。DPI、窗口缩放、Follow/高对比度、键盘焦点和真实备份/媒体操作仍是总目标边界。

## 2026-08-25 UI-315 共享自适应毛玻璃材质

- 游戏背景的真实图片、壳体底层模糊和卡片共享材质是三层职责：图片只绘制一次并居中裁剪；BlurEffect 只放在壳体图片层；卡片/表格/浮层通过共享 DynamicResource 使用采样色渐变。
- `AdaptiveThemePaletteFactory.ApplyGameBackgroundGlassResources` 是共享表面入口。当前覆盖 `GscGlassFillBrush`、`GscGlassStrongBrush`、`GscTableHeaderBrush`、`GscPopupBrush` 和 `CardBackgroundFillColorDefaultBrush`，从而覆盖 Redesign 的 SectionCard、TableFrame、Hero、Metric、FloatingPicker 及 WPF-UI Card。
- `DashboardView.ApplySelectedGameGlassResources` 必须在主题刷新和 `SelectedGameBackgroundAmbientBrush`/`HasSelectedGameBackgroundAmbientMaterial` 变化时同步 Dashboard、生产壳及所有 workspace 的本地 ResourceDictionary；切换到无图游戏时必须恢复 `ApplyDemoCoreResources` 的中性资源。
- 不要给每张卡片增加 BlurEffect，也不要把游戏采样色直接作为完全透明背景；前者会模糊文字并增加视觉树成本，后者会让表格在亮色图片上失去可读性。共享表面应保持受控 alpha，真实 Blur 继续留在底层图片。
- 输入框、ComboBox 等交互控件暂不跟随图片大幅变色；如果未来扩大范围，先验证文本对比度、焦点边框、禁用态和高对比度回退。

## 2026-08-25 UI-316 设置页独立毛玻璃材质

- 设置窗口是独立页，不跟随当前游戏图片取色；它通过 `AdaptiveThemePaletteFactory.ApplySettingsMaterialResources` 使用主题 Accent/Info/Success 生成自己的环境渐变和透明材质层。
- `GameSaveCenterSettingsView.xaml` 的外壳、分类栏、右侧 `SettingsScroller`、设置 `Card` 和表单输入继续使用分层 DynamicResource；只有 `SettingsAmbientLayer` 承载整页 BlurEffect，不能把模糊效果挂在卡片或输入控件上。
- `GscSettingsShellBrush`/`PanelBrush`/`CardBrush`/`ContentBrush` 的 alpha 需要保持层次：底层环境渐变可见，表单文字和输入值仍清晰；禁用玻璃和高对比度必须恢复不透明回退。
- RenderHarness 的设置页渲染应显式调用 `ApplyThemeForAudit`，否则只会捕获 `DesignTokens` 初始回退，无法验证设置页运行时玻璃资源是否真正生效。
- UI-316 已通过源码/XAML 校验、Release 全量构建测试和浅色/深色/多尺寸 render-qa；WPF validator 为 0 error、18 warnings、172 info。真实 Playnite 重启后的逐页像素证据仍不具备，不得扩写为宿主验收。

## 2026-08-25 UI-320 游戏选框圆角与筛选默认值事实

- 生产壳游戏选框 `AcrylicProductionShellView.xaml` 的 `PickerList` 必须基于共享隐式 `ListBoxItem` 样式；局部样式只能覆盖间距、对齐和文本前景，不能让 Playnite 默认模板接管选中/预选状态，否则会重新出现矩形高亮。
- 生产壳三个游戏筛选框必须同时保留 `SelectedIndex="0"` 与真实 `SelectedItem` 双向绑定的 `TargetNullValue`/`FallbackValue`。平台选项集合会异步重建，代码需要订阅 `PlatformFilterOptions.CollectionChanged` 并调用 `UiFilterSelection.RestoreDefault`，不要只依赖 XAML 初始 `SelectedIndex`。
- 首页游戏选框自定义 Row 若使用 `CornerRadius`，必须同时使用 `ClipToBounds="True"`；否则圆角背景下的内容/状态层可能露出矩形。
- 真实游戏选择绑定、`SelectedGame` 更新、列表虚拟化和关闭弹层逻辑保持不变。当前 `.tmp/ui-qa-game-picker-rounded-defaults/render-qa-report.txt` 仅是离屏证据，不能写成真实 Playnite 弹层视觉验收。

## 2026-08-25 UI-321 平台筛选默认值时序事实

- 生产 `PickerOverlay` 初始为 `Collapsed`，不能只在 `Attach` 或点击事件的同步代码中调用 `UiFilterSelection.RestoreDefault`；这些时刻可能还没有生成 ComboBox Items。
- 平台筛选默认值恢复必须覆盖弹层打开、平台选项集合变化和 `GamePickerPlatformComboBox.Loaded`，并至少排队到 `DispatcherPriority.DataBind`、`DispatcherPriority.Loaded` 两个阶段。
- `UiFilterSelection.RestoreDefault` 只应修复空选中或不再属于当前 Items 的无效选中；有效的用户平台选择不能被“全部”覆盖。
- UI-321 已通过源码/XAML 门禁、Release 构建和 Playnite 定向测试；真实宿主需重载扩展后确认中间框显示“全部”。

## 2026-08-25 UI-322 底部状态栏与侧栏折叠事实

- 生产壳 `FooterSurface` 现在位于根 Grid 的第 0 列并跨两列，Worker/Ludusavi 状态灯由 `FooterStatusPanel` 承载；侧栏不再放状态卡。新增状态展示必须继续使用 `Snapshot.WorkerHealthy` 与 `Snapshot.LudusaviAvailable`，不能复制静态健康状态。
- 侧栏默认宽度是 236 DIP；`SidebarCollapseButton` 是品牌区内 26×26 的小型共享 `GscWpfUiButton`，点击通过 `ApplySidebarLayout` 切换 78 DIP 图标态，再调用既有页头/页面响应式布局。不要把折叠入口做成导航项，也不要默认启动为折叠态。
- 折叠态只隐藏品牌文字、生产版标签和导航文字，并保留 ToolTip/AutomationProperties.Name；导航 RadioButton 仍是同一组真实工作区入口，绑定、命令、滚动和虚拟化不变。
- UI-322 的源码/XAML/Release/Playnite/RenderHarness 门禁已通过；RenderHarness 只证明页面主题和响应式回归，不等同真实 Playnite 侧栏折叠像素或键盘验收。

## 2026-08-25 UI-323 当前事实：状态栏右对齐、版本气泡与设置尺寸

- 生产壳底部 `FooterStatusPanel` 位于 Footer 的右侧 Auto 列；底部不再显示产品名和“生产版 · 真实数据由 Worker 提供”，状态文字仍必须绑定 `Snapshot.WorkerHealthy`/`Snapshot.LudusaviAvailable`。
- `SidebarProductionVersionText` 在壳体 Loaded 时从 `AcrylicProductionShellView` 程序集读取三段版本号，XAML 的 `v0.6.70` 只是安全初始值；不要将它改回“生产版”静态标签。折叠按钮在 `SidebarUtilityStrip`（标题下方独立工具条），品牌行只负责图标、名称和版本气泡。
- 设置入口 `GameSaveCenterSettingsView` 现在请求 `MinWidth=1180`、`MinHeight=760`；内部 `SettingsShell` 仍受 1360 DIP 上限和原有响应式断点控制，不能为了放大窗口移除滚动或改变保存语义。
- UI-323 的源码/XAML/Release/Playnite/RenderHarness 门禁已通过；RenderHarness 的设置尺寸证据不等同 Playnite 宿主最终窗口尺寸，需重载扩展后人工确认。

## 2026-08-25 UI-324 当前事实：侧栏折叠书签与过渡动画

- `AcrylicProductionShellView.xaml` 的 `SidebarCollapseButton` 不再位于品牌标题下方工具条，而是覆盖在侧栏右侧靠近底部的位置；它使用 `AcrylicSidebarBookmarkButton` 共享 ControlTemplate，呈垂直书签轮廓，默认展开态仍为 236 DIP，折叠态仍为 78 DIP。
- 书签是独立于 `SidebarContentLayer` 的交互层，因此不会挤压 `GameSaveCenter`、版本气泡或真实 `Nav*` 项；导航内容继续由同一组 RadioButton、绑定、滚动和虚拟化承载。
- `OnSidebarCollapseClick` 在动画启用时对 `SidebarContentLayer` 做短暂淡出、4 DIP 横向位移再淡入；`MotionEnabledProvider` 从 `DashboardView` 提供持久化动画设置，系统高对比度/禁用动画时走同步切换。不要把动画改成循环计时器，也不要给页面列表内容加 BlurEffect。
- 书签 ControlTemplate 只使用共享动态材质和状态触发器，包含悬停、按下、键盘焦点、禁用和 Tooltip/AutomationProperties；不要将折叠按钮恢复为普通导航项或重新放回品牌行。
- UI-324 已通过源码/XAML、WPF 0 error、Release、Playnite 295/352 和 RenderHarness `render-qa OK`；真实 Playnite 宿主点击/键盘/DPI 像素仍是人工边界。

## 2026-08-25 UI-325 当前事实：侧栏底部一体式折叠控件

- UI-325 覆盖 UI-324 中“字面垂直书签”的视觉指导。当前不得恢复 `AcrylicSidebarBookmarkButton`、Path 丝带轮廓或贴在侧栏右边的书签形状；“书签”只表示用户提供的底部控制位置概念。
- 当前共享样式是 `AcrylicSidebarCollapseButton`，基于 `GscWpfUiButton` 的普通圆角按钮。展开态在侧栏底部显示图标、“收起侧栏”和右箭头，约 168×34 DIP；折叠态为 40×34 DIP 的小圆角按钮，只保留居中的展开图标。
- 折叠态必须同时设置 `SidebarCollapseButton` 与 `SidebarCollapseButtonContent` 的居中；所有 `Nav*Content` 在折叠时也必须显式 `HorizontalAlignment=Center`。不能只隐藏文字后依赖默认 ContentPresenter 推断位置。
- `OnSidebarCollapseClick` 的动画、MotionEnabledProvider、系统动画/高对比度降级以及 `ApplyHeaderLayout`/`ApplyPageLayout` 重算继续保持；不能为了换控件改变真实导航绑定、滚动、虚拟化或版本气泡。
- UI-325 已完成源码/XAML、Release、定向折叠契约和 RenderHarness `render-qa OK`；完整测试与真实 Playnite 点击/键盘/DPI 像素复核是本阶段提交前/宿主边界。

## 2026-08-25 UI-326 当前事实：折叠图标中心线与首页右侧卡片密度

- 折叠态导航的对齐基准是侧栏内部 26 DIP 图标槽：`SidebarHeaderLayout` 去掉展开态不对称边距，`SidebarBrandContent` 与所有 `Nav*Content` 在折叠态固定宽度并居中；图标 `TextBlock` 必须保持 `TextAlignment="Center"`，不能只依赖 StackPanel 的默认测量。
- `AcrylicProductionShellView.xaml.cs` 的 `ApplySidebarLayout` 仍是展开/折叠唯一布局入口，需保留 `ApplyHeaderLayout`、`ApplyPageLayout` 和导航 RadioButton 的真实绑定/滚动/虚拟化。
- 首页 `OverviewProtectionPreviewCard` 的圆点列为 14 DIP，以便状态点和游戏标题之间保留轻微间距；`OverviewAttentionScrollViewer` 的有限视口为 220 DIP，页面根滚动继续负责更长内容。
- 这些调整只改变共享布局密度，不改变风险状态语义、关注项真实数据、操作命令或主题资源。
- UI-326 已通过 source/XAML 门禁、WPF 0 error、Release 构建、Core 59/59、Worker 199/199、Playnite 295/57 skipped/0 failed，以及双主题多尺寸 `render-qa OK`。
- 真实 Playnite 重启后的折叠像素、Follow/浅色/深色、DPI 和键盘焦点仍未由本轮重新确认；不得把 RenderHarness 截图写成真实宿主逐像素验收。

## 2026-08-26 CORE-327 保留策略清理安全闭环

- `RetentionSimulationApplyRequestDto` 必须包含 `PreviewGeneratedUtc`、`ExpectedCandidateCount` 和 `ExpectedReleaseBytes`；Worker 在加载当前索引后重新计算候选数量/体积，不匹配或超过 10 分钟就抛出 `RETENTION_PREVIEW_STALE`，缺少预览则抛出 `RETENTION_PREVIEW_REQUIRED`。
- `RetentionSimulationService` 处理候选 ZIP 时先移动到备份根下的 `.gsc-retention-quarantine/<batch>/<backupId>.pending`，再删除索引；索引删除失败要尝试原路恢复。不能恢复时必须保留审计明细，不能直接把原文件静默删除。
- 隔离目录中的文件不使用 `.zip` 后缀，以免被后续 Ludusavi 归档扫描误识别；清理失败通过 `PendingQuarantineCount`/`PendingQuarantineBytes` 返回并记录审计。
- 预览 UI 最多展示 200 条候选明细，摘要必须明确“前 N 条/全部候选”，避免产生完整列表的错误认知。
- CORE-327 已完成 Worker 定向测试 6/6；本阶段跳过真实 Playnite 宿主验收，不能把离线测试写成主题、DPI 或宿主行为证据。

## 2026-08-26 CORE-328 诊断环境信息与首次检查性能

- 诊断包展示 DPI 必须来自 `GetDpiForSystem`，不能用 `SystemParameters.PrimaryScreenWidth / WorkArea.Width`；屏幕数量来自 `GetSystemMetrics(80)`，API 失败时回退 1。
- `EnvironmentCheckRequestDto.IncludeBackupProbe` 控制是否调用 Ludusavi 的全库只读列表；首次自动检查传 false，手动“重新检查”传 true。`IncludeRemoteProbe` 同样在首次自动检查关闭，避免启动时网络探测。
- IPC 默认请求仍保持完整探测（两个开关默认 true），只有 Playnite 首次启动路径显式使用快速模式，避免改变其他调用方语义。
- CORE-328 的真实逐窗口 DPI、Playnite 多屏位置和远端探测宿主行为仍属于跳过的人工验收边界。

## 2026-08-26 PERF-329 大库更新回归门槛

- `LargeLibraryPerformanceTests.GamePicker2000_Benchmark_WritesMeasuredTimings` 不仅写 profiling，还必须保持 2000 条首次/单项变化更新和任务首次替换低于 5 秒、未变化替换低于 1 秒；阈值刻意宽松，只拦截数量级退化。
- 详细基准仍写入 `large-library.txt`，不得把这些离线集合耗时扩写为真实 Playnite 渲染帧率。

## 2026-09-12 UI 显示审查后续任务

- 新入口：`docs/ai/UI_DISPLAY_REVIEW_2026-09-12.md`，10 项显示任务，优先待归类短窗、设置错误摘要与任务首屏密度；其余为首页、表格阅读、详情、预览状态和反馈。
- 本轮为文档审查，未修改生产 UI。旧滚动漂移未在真实 Playnite 复验；不能把离屏部分行截图当作末条不可达的证明。
- 完整 render-qa 运行退出 -1 且未产出最终报告；单独 shellqa 首次缺少 theme/light、theme/dark 目录，补建后通过。完整 UI 验收仍未闭环，详见 D12-10。
- 全量构建测试零失败：Core 76/76、Worker 310/311（1 skip）、Playnite 435/498（63 skip）。4 张原始截图与 Shell 报告保存在 `docs/design/reviews/2026-09-12-display/`。

## 2026-09-12 U12-00 共享 UI 审计

- 资源入口、审计结果及页面使用约束记录于 `docs/ai/UI_SHARED_AUDIT_2026-09-12.md`。后续 UI 不新增第二套玻璃、按钮、字体、图标或动效系统；共享入口依次为 DesignTokens/Typography/MotionTokens、WpfUiProduction/Redesign/ButtonStyles 和两套 Gsc 图标包。
- 原始 AcrylicFork Design 目录不在当前工作树；当前 Demo-first 结构的可追溯锚点是提交 `3c12b2c` 和 `RestoredAcrylicForkBaselineTests.cs`。找回原始素材前不要把它作为编译依赖或换用其他视觉体系。

## 2026-09-12 U12-02 设置错误摘要

- `GameSaveCenterSettingsView` 页头错误必须保持紧凑摘要；完整错误由 `SettingsValidationDetails` 可展开呈现。不要恢复把多条路径错误直接拼接到页头的做法。
- 常规与目录错误通过 `SettingsGeneralValidationHint` 靠近字段显示；其他分类仍由 `SettingsValidationLocateButton` 跳转。`VerifySettings`、保存阻断、真实提交/回滚与字段级校验不能改成静态提示。
- 短高度下 `SettingsHeaderEyebrow` 可隐藏，但标题、保存状态、错误摘要和定位入口必须保留；真实 Playnite/DPI/键盘复核仍未完成。

## 2026-09-12 U12-01 待归类媒体短窗

- `MediaInboxInfoBand` 在紧凑高度（低于 800 DIP）折叠，因为页面指标已提供相同的数量上下文；不要在同一短窗重新加回重复标题/数量带。
- `MediaInboxPageScrollViewer` 仅在高度低于 560 DIP 或过期横幅可见时启用整页 fallback。528 DIP 的主题化 1366×768 内容区已证明 520 DIP 不足以保护四行阅读底线；过期状态不能仅为追求首屏而关闭 fallback；DataGrid 的有限视口、Recycling/Item 滚动、选择与批量操作保持不变。

## 2026-09-12 U12-03 任务短窗

- `TaskWaitingSummaryText` 与 `TaskRetrySummaryText` 仅在 720 DIP 以下收起；任务总数、运行中、可重试和今日完成四个统计入口不隐藏。不要为增加行数缩小 TaskGrid 字体或关闭虚拟化。

## 2026-09-12 U12-04 首页短内容区

- `OverviewTodayHeroCard` 在低于 560 DIP 的内容区降到 132 DIP，并隐藏 `OverviewTodayHeroEyebrow` 与说明；优先事项标题和操作必须保留。不要改变当前游戏备份与工作台“全部备份”的真实作用域。

## 2026-09-12 U12-05 表格阅读

- Media 收件箱的全部文本列（含“来源”）必须使用 `MediaLongText` 或等效的省略号和完整 Tooltip 契约。保持稳定时间/类型列、名称/原因弹性列，以及共享 DataGrid 的横向滚动、排序、拖拽和虚拟化。

## 2026-09-12 U12-06 诊断复制边界

- `CopyTaskErrorCommand` 的复制内容必须包括 `ErrorMessage`、`ErrorCode`、`DetailMessage` 和任务 ID；视觉详情去重不能削弱完整诊断复制能力。U12-06 已完成失败优先与可展开技术详情结构。
- 命令可执行条件也必须接受三种字段中的任一项，不能因 `DetailMessage` 缺失而让用户无法复制失败原因或错误码。
- `TaskInspectorErrorCard` 必须位于 `TaskTechnicalDetailsExpander` 之前，Expander 默认收起；失败原因和错误码优先可见，技术详情只在用户展开后出现。

## 2026-09-14 Round2 Q00 当前事实

- Q00 已完成一轮真实共享资源修复：`GscTypographyNumeric`、`GscWpfUiButton`/Primary/Danger、`GscWpfUiToggleSwitch`、`GscRedesignContextPill` 与校对视图不再依赖 WPF 默认黑色前景；Toggle 内容和按钮派生样式均有明确主题绑定。
- `AdaptiveThemePaletteContrastGuard.MeasureTextContrast/ValidateTextContrast` 负责按最终前景 alpha 与实际表面样本测量；正常浅/深色校对各 12 个样本达到 4.5:1，固定黑字暗底负例必须失败。内部 palette 比较也改为未舍入值。
- `UiFrameworkProbeView` 的 DataGrid 由 246 调整为 250 DIP，受控报告以实际 `DataGridRow` 与有效裁剪交集证明 4/4 完整；故意减 4 DIP 得到 3/4。报告只把 `FontHasGlyph` 记为 `FontCandidate`，实际 GlyphRun 明确 unknown。
- 证据索引为 `docs/design/reviews/ui-finesse-round2-20260913/evidence/Q00-INDEX.md`，当前分支 `codex/ui-finesse-round2`。受控截图是 1120×980 DIP/96 DPI 离屏证据，不代表物理 DPI 或真实 Playnite 宿主；当前宿主边界仍是 `MainWindowHandle=0`。
- 提交前一键门禁首次在 Playnite 设置迁移测试阶段失败，独立复现为隔离构建根目录带 32 位 GUID 时 .NET Framework xUnit 适配器加载路径超过 Windows 260 字符；`scripts/dev-install-run.ps1` 已改为 8 位隔离 token，必须在后续完整门禁中确认修复。
- 提交 `87a40c8` 后完整门禁已验证短路径问题消失：XAML 24/24、Release、Core 76/76、Worker 和 Playnite 450/513（63 skip）完成；唯一失败为 `WorkspaceStatePresenterBehaviorTests.RetryButtonKeyboardActivationExecutesOnce` 的一次非确定性 0/1，独立定向重跑 2/2 通过，完整链仍未签收。

## 2026-09-14 Round2 Q01-Q02 当前事实

- Typography 的共享字体入口已补齐 `Segoe UI`、`Noto Sans SC/CJK SC` 和两个 Microsoft YaHei 家族；Q01/Q02 不通过嵌入字体解决缺字，RenderHarness 会分别记录候选覆盖和实际 GlyphRun 未知边界。
- `GscTypographyBody`/`Caption` 使用 20/18 DIP 的共享 BlockLineHeight，`GscTypographyNumeric` 使用 Tabular numeral alignment。报告已覆盖 CJK、Latin、下伸部、全角标点、重音、组合字符、代理对、扩展 CJK、emoji、零、破折号、秒/分钟和时间宽度。
- `TypographyDiagnosticsTests` 当前 4/4 通过；Q01-Q02 受控双主题截图/报告位于 `docs/design/reviews/ui-finesse-round2-20260913/evidence/q01/`。扩展 CJK 与 emoji 缺字保持 unresolved 是真实缺字记录，不得改写成 resolved。
- Q02 的生产路径全面挂接、重要 FontSize 10/11 逐入口清点、八入口术语/单位盘点和真实宿主 Tooltip/复制仍是未完成项；当前进度账本已按“代码/自动/视觉/宿主/最终结论”分别记录，不能用夹具结果替代宿主验收。

## 2026-09-14 Round2 Q03 当前事实

- 主按钮 contrast 不能只测 token accent：玻璃 alpha、渐变 stop、中点、hover/pressed wash 和 pressed chrome opacity 都会改变最终文字对比度。当前 guard 对每主题 33 个状态样本逐点测量；共享 CTA stops 已收敛为 opaque accent，pressed opacity 为 0.96。
- OnAccent 状态层现在按黑/白前景极性选择 wash；Danger 单独使用 `GscOnDangerTextBrush`，不能把错误红底机械复用主 accent 前景。选中、输入正文/placeholder、复杂背景合成也有独立样本和阈值。
- Q03 双主题受控证据位于 `docs/design/reviews/ui-finesse-round2-20260913/evidence/q03/`，索引 `Q03-INDEX.md`；semantic button/layer/complex 三类报告均 0 violation。真实主题切换中的 Popup/Tooltip/Dialog、IME、宿主输入序列和物理 DPI仍未完成。

## 2026-09-14 Round2 Q04-Q12 当前事实

- Q04-Q12 已完成一次共享入口专项复核，范围包括 `DesignTokens.xaml`、`WpfUiProduction.xaml`、`GscIconPack.xaml`、`Redesign.xaml`、`DashboardView.xaml` 和开发校对夹具；账本逐项记录受控证据与真实宿主边界，不把离屏截图计为最终验收。
- 已修复共享 CheckBox 的真实缺口：`GscCheckBox`、`GscDataGridCheckBox` 都有 `IndeterminateMark`，`IsChecked=null` 时显示半选短横线；批量选择不能再用普通勾号冒充半选。
- `UiFrameworkProbeView` 当前额外包含半选 CheckBox 和 Slider；RenderHarness 双主题报告实际测得 1 TextBox、1 ComboBox、4 个按钮、1 Toggle、2 个 CheckBox、1 Slider、1 ListBox，记录输入 padding/caret/selection、Combo Popup 模板、按钮 36 DIP 高度、半选 mark 可见和列表虚拟化。
- Q04-Q12 证据索引为 `docs/design/reviews/ui-finesse-round2-20260913/evidence/Q04-Q12-INDEX.md`，截图与原始报告位于 `evidence/q04-q12/{dark,light}/`。IME、Popup 真定位/移屏、物理 DPI、读屏、真实命令单次执行、六页导航与短窗宿主仍是外部待验边界。
- 阶段提交前一键门禁第二次结果：XAML 24/24、Release 构建 0 警告/0 错误、Core 76/76、Worker 310/311（1 skip）、Playnite 459/522（63 skip）通过；因证据索引仍未提交而按设计停止打包，不能宣称本次已完成安装/启动验证。

## 2026-09-14 Round2 Q13 当前事实

- MediaCenter“待归类”短窗是本阶段发现的真实布局缺陷：compact/narrow 主表实际高度为 190/150/130 DIP，页面滚动又被关闭，无法稳定看到四行。生产修复固定主表/表格壳 212 DIP 最小可读高度，外层 `MediaInboxPageScrollViewer` 保留有限 Auto 通道；不要通过缩小正文、关闭 DataGrid 虚拟化或改写现有命令来规避。
- `MediaInboxInspectorScrollViewer` 是页面内有意保留的详情滚动面，不能与主表页面滚动混为同一职责。布局分析器仅对 `media-center/待归类` 的这个命名边界记录 NESTED_VERTICAL_SCROLL 信息；主表仍单独要求 PRIMARY_SCROLL_ACCESS，避免审计器用“全局禁止嵌套滚动”制造假门禁。
- Q13 证据由媒体锚点/滚动诊断、DataGrid 模板来源、短窗源代码门禁和全量离屏审计组成。审计当前为 Fidelity=0、failed routes=0、HIGH=none；这只覆盖逻辑 DIP/离屏事实，不等同于真实 Playnite 鼠标、触控或像素验收。
- Trainer 审计的 126 DIP Medium 已查明是 `TrainerToolsSettingsScrollViewer` 内的五项设置 WrapPanel，不是页面工具栏；`UiLayoutAnalyzer` 仅按该命名祖先排除此内部设置簇，复跑结果 MEDIUM=none。若未来新增真实页面工具栏，不能依赖这个例外绕过告警。
- Q14–Q25 尚未因本条记忆而关闭。真实宿主视觉、物理 DPI、IME、读屏、ETW 帧、>100ms 调用栈、30 分钟耐久和低性能 Tier 仍是明确阻塞边界；阶段证据索引必须写入真实代码提交身份。

## 2026-09-14 Round2 Q14–Q15 当前事实

- `UiFinesseRound2ControlSourceTests` 现在覆盖共享 ToolTip 主题字体、Caption 尺寸、420 DIP 长提示上限、显示时序和 Trainer 980 DIP 工具栏重排；它验证的是生产资源/代码契约，不代替真实鼠标、Popup、IME 或独立窗口验收。
- Trainer 四个导入命令在 `width < 980` 时移到标题下方，并让拖放提示继续跟随，避免窄宿主右侧自动列裁掉最后一个命中区；普通布局仍保留同一批真实命令和绑定。
- Q14/Q15 的宿主边界继续开放：真实 760/980 DIP 输入序列、Tooltip 边缘定位/关闭时序、菜单 Esc/点外部、设置窗口 Owner 主题隔离、物理 DPI 和字体回退尚未签收。

## 2026-09-14 Round2 Q14–Q25 账本回填事实

- `Q13-Q25-INDEX.md` 已按组列出 Q14–Q23 的生产入口、专项测试和全量离屏审计映射；账本的“已复核”只表示这些受控事实已核对，最终列仍为“未完成”，涉及真实输入、Popup、模态、动画、解码和设置窗口的行继续标记宿主待验。
- Q24 的物理跨屏仍是实施中；键盘次序、焦点语义、UI Automation、高对比与长文案仅有源码/受控证据，不能替代 100/125/150/175/200% DPI、跨屏 Popup、中文 IME 和真实读屏。
- Q25 的受控证据覆盖大库/缩略图并发边界、审计性能字段、构建身份和干净 HEAD 的打包/安装/启动；30 分钟耐久、ETW 呈现帧、超过 100ms 调用栈、低性能 Tier 与收尾回查仍开放。
- 本阶段提交前一键门禁实际完成 XAML 24/24、Release 0 警告/0 错误、Core 76/76、Worker 311/311、Playnite 467/524（57 skip，失败 0）；因三份文档仍未提交，脚本按保护逻辑停止打包，不能把这次运行当作新的安装/启动验证。
- 随后按索引筛选 Q16–Q25 专项测试，焦点/详情、通知、动效、缩略图、首页、存档、任务维护、设置、可访问性、性能和构建身份共 77/77 通过、0 跳过、0 失败；仍不能替代真实宿主、DPI、IME、读屏、ETW 和耐久验收。
- Q25-01 基准曾捕获 2000 项搜索 p95=201ms；修复 `GamePickerViewModel` 的 180ms debounce（改为 20ms）和每项诊断列表分配后，预热 5 次、采样 30 次复测 p50/p95/max=54/57/63ms。它只覆盖 ViewModel 过滤输入到反馈，不冒充页面导航或屏幕帧。
- `LargeLibraryPerformanceTests` 已把 Q25-01 的 `search_p95_ms <= 100` 固化为 30 次采样回归门禁；这仍只约束受控 ViewModel 输入到反馈，不替代页面导航或屏幕帧验收。
- 当前代码提交为 `5f60404`；其一键门禁编译/全量测试通过（XAML 24/24、Release 0 warning/0 error、Core 76/76、Worker 311/311、Playnite 468/525，57 skip，0 fail），但因工作树有当前代码/文档改动按保护规则停止打包。`6450f6e` 的干净安装/启动证据仍只对应此前代码基线。
- 基准变更后的提交前一键门禁完成 XAML 24/24、Release 0 警告/0 错误、Core 76/76、Worker 311/311、Playnite 467/524（57 skip，失败 0）；脚本因本轮代码、证据和文档未提交而停止打包，不能把本次运行当作安装/启动通过。

## 2026-09-14 Round2 Q25 宿主性能边界复核

- 已提交 HEAD `6450f6e` 的干净一键流程真实完成 XAML 24/24、Release 0 warning/0 error、Core 76/76、Worker 311/311、Playnite 468/525（57 skip，0 fail），包体身份、安装验证和 Playnite 启动成功；本轮宿主随后按官方 `--shutdown` 关闭。
- Computer Use 初始化因 kernel assets 路径缺失失败；WPR 的 GPU/DesktopComposition/XAMLActivity 记录又因系统性能分析策略拒绝（`0xc5585011`）无法启动，`wpr -status` 为未录制。因此没有把离屏/Rendering 代理升级成 ETW 呈现帧，也没有伪造 >100ms 调用栈、30 分钟耐久或低 Tier 实机数据。
- 证据文件：`docs/design/reviews/ui-finesse-round2-20260913/evidence/q13-q25/Q25-HOST-PERFORMANCE-BOUNDARY-20260914.txt`；Q24 物理 DPI、跨屏 Popup、中文 IME、读屏和 Q25-02～05 的真实宿主性能仍开放。

## 2026-09-14 Round2 Q00–Q02 增量记忆

- Q00-06 已用测试锁定旧账本恰好 52 个唯一 P 行，并保留每行原状态、结论、依据/下一步和 Q 映射；这只证明追溯结构，不关闭对应外部阻塞。
- 旧 BASELINE 的临时报告链接已改为持久证据索引/宿主边界记录；`docs/design` Markdown 本地链接扫描结果为 0 缺失。
- Q01 标点与字重、Q02 数值/未知值/原始路径 Tooltip 已有双主题报告与 9 项定向测试证据；实际 GlyphRun、完整路径样式迁移、重要 10/11pt 文本清点和真实宿主布局仍开放。
- Q02-06 的存档候选路径列已使用 `SavePathText` 共享代码字体样式；这不等于所有路径入口或宿主复制/Tooltip 已验收。
- Q02-07 的首个重点入口已收紧：Trainer 设置 Inspector 的重要标签、工具路径与风险提示使用共享可读样式，避免显式 10pt；不要据此假设所有生产 10/11pt 文本或宿主小窗口都已完成。
- Q02-02/Q02-03 的存档历史文件数/大小列已使用右锚点 Tabular 数字样式并禁止截断；不要把这一入口的源码契约当成所有表格和宿主布局均已验收。
- Q02-04/Q02-08 的 DTO 门禁已覆盖真实零、未知大小、未检查时间、路径原值和语义分隔符；仍需八入口术语盘点与宿主观感验证。
- Core `UiDisplayMappingTests` 当前为 16/16；若 NuGet 漏洞源不可达，记录 3 条 NU1900 网络警告，不能写成代码警告清零。
- 当前阶段提交前若不能安全运行会停止用户 Playnite 的一键脚本，不得把局部编译/测试或旧提交的 clean install/start 记录改写为当前提交的安装证据。

## 2026-09-14 Round2 Q02-06 技术路径入口覆盖

- 提交 `d97d87b` 新增共享 `GscPathText` 与 `GscWpfUiPathTextBox`：技术路径只读文本统一代码字体/单行省略，编辑框仅切换代码字体，保留原生 TextBox 的选择、光标、校验和编辑语义。
- 设置页 7 个路径类编辑框、Trainer 工作目录、存档候选详情、媒体文件/归档/来源路径、维护诊断路径与进程 EXE 已接入；原始值 Tooltip 和数据绑定没有改写。
- Q02-06 持久证据为 `docs/design/reviews/ui-finesse-round2-20260913/evidence/q02/Q02-PATH-STYLE-COVERAGE.md`；WPF 定向测试 7/7、RenderHarness 深浅主题退出 0。宿主复制/Tooltip、最终截断、物理 DPI 仍保持未完成边界。
# 提交前一键门禁

每次提交前必须运行仓库根目录的 `GameSaveCenter-一键构建安装运行.cmd`，确认隔离 Release 构建、Core/Worker/Playnite 全量测试、打包、安装验证和 Playnite 启动均成功。局部测试或 RenderHarness 通过不能替代该门禁；若输出超时，需后台运行并轮询到最终退出结果后再判断。

## 2026-09-15 Round2 Q24-03 物理跨屏前置采集

- `70935fe` 将真实宿主运行器的显示器拓扑写入 `runner-metadata.json`，`3851228` 修正最终 JSON 深度写入；当前代码包含 `DisplayCount`、每个显示器的 Bounds/WorkArea，以及 `Q24_03PhysicalCrossScreen.Status`（`ready-for-host-replay` 或 `blocked-single-display`）。这只是执行前置，不替代第二物理屏上的窗口迁移与 Popup 截图。
- 游戏选框保持 Dashboard 宿主内 `GameBrowserPanel`/`GameBrowserScrim`，不创建独立 Window；ComboBox 的共享 Popup 模板保留 Bottom 定位、动态主题资源、关闭语义和有限内部滚动。对应源代码契约和证据在 `docs/design/reviews/ui-finesse-round2-20260913/evidence/q13-q25/Q24-03-PHYSICAL-CROSS-SCREEN-20260915.md`。
- 当前 `System.Windows.Forms.Screen.AllScreens` 只有 `DISPLAY1`，边界 `2560×1440`、工作区 `2560×1368`；Q24-03 仍在台账中保持“代码完成/自动待验/视觉待验/外部阻塞/未完成”。
- `python scripts/validate-source.py` 和 PowerShell 语法解析通过；定向 `dotnet test` 在当前 SDK 工程解析阶段无输出，未计为通过。不要运行未经隔离和授权的 `real-host-audit.ps1` 来替代本前置。
- 后续改用 `-m:1` 重试后，Q24-03 定向源测试 `16/16` 通过；并在 `d8fad48` 执行 `dotnet test GameSaveCenter.sln --no-restore -c Release -m:1`，Core `83/83`、Worker `310/311`（1 skip）、Playnite `482/545`（63 skip），失败 0。该 Release 自动基线仍不替代真实宿主跨屏、DPI、IME、读屏和 ETW 证据。

## 2026-09-15 Round2 Q06-05 选中悬停状态优先级

- 生产 `AcrylicNavItem` 的选中/悬停状态必须由共享模板确定性收口：`IsChecked=True` 与 `IsMouseOver=True` 的最后 `MultiTrigger` 恢复 `GscAccentTintStrongBrush`、强边框和 `GscSelectionTextBrush`，不能让普通悬停 tint 稀释当前页选中层级。
- 提交 `513ac5f` 已补实现和源契约；`UiFinesseRound2ControlSourceTests=20/20`、XAML `24/24`、source validation 通过；clean-tree RenderHarness 双主题 `render-qa OK`，报告身份为 `513ac5f2528e0706fde194d977f03fd8bd798e5d`。
- 该事实只关闭共享状态代码缺口。真实 Playnite 中保持悬停、焦点/禁用/按压的输入序列和最终像素仍必须单独验收，账本继续保留 Q06-05 外部待验。

## 2026-09-15 当前提交真实宿主审计边界

- `c5a2997` 的隔离审计使用 Release、隔离 UserData，捕获当前提交 EmbeddedPlaynite Dashboard 29 个视口、2 个滚动面和 Settings 1 个视口；构建 0/0，Core `83/83`、Worker `311/311`、Playnite `499 passed / 57 skipped / 0 failed`，提交 SHA 与 metadata 一致。
- 启动时复用了用户扩展目录内的旧 Worker PID `23304`，精确路径为 `C:\Users\lopmatu\AppData\Roaming\Playnite\Extensions\GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec\Worker\GameSaveCenter.Worker.exe`，身份 `0.6.73+6450f6...`；当前插件为 `0.6.73+c5a2997...`，截图出现构建身份不兼容 Toast，`HighGateCount=1`。
- 运行器不会结束其他扩展目录的用户 Worker；未获用户明确授权前不强制停止该 PID，也不把本轮截图写成当前提交的宿主全链路通过。重跑条件与证据见 `docs/design/reviews/ui-finesse-round2-20260913/evidence/q13-q25/REAL_HOST_AUDIT-CURRENT-20260915.md`。

## 2026-09-15 Round2 WPF 测试调度隔离

- Playnite 测试程序集包含多个真实 STA WPF Window/Dispatcher 回归。完整套件曾在短时动画终态或侧栏卸载时钟断言上偶发失败，而同一 Release 二进制单独重跑通过；这属于测试调度竞争，不能把一次失败写成宿主或产品确定性阻断。
- 新增 `tests/GameSaveCenter.Playnite.Tests/AssemblyInfo.cs` 的 `[assembly: CollectionBehavior(DisableTestParallelization = true)]`，与 Worker 测试策略一致，确保 WPF 测试串行运行；不改变生产命令、绑定、动画或资源。
- 修改后的 Release 全量门禁为 XAML `24/24`、Core `83/83`、Worker `310/311`（1 skip）、Playnite `495/558`（63 skip），失败 `0`。

## 2026-09-15 Round2 Q00 审计门禁假阳性修复复核

- `RealHostUiAuditService.CountBlockingGateFiles` 现在排除 `overflow-classification.json` 诊断分类报告；定向 truthfulness 回归 `1/1` 通过。已有隔离产物只有该 JSON，因此修复规则下阻断门禁为 `0`；旧 `summary.json` 的 `HighGateCount=1` 不被篡改。
- 当前 `cc63523` 的 Release 自动门禁为 XAML `24/24`、Core `83/83`、Worker `310/311`（1 skip）、Playnite `495/558`（63 skip），失败 `0`；打包、隔离安装和构建身份检查也通过。
- 最新宿主启动在 Playnite CEF 初始化时以 `Access denied (0x5)` 退出，未生成新的真实 Dashboard/Settings 证据；历史 `69e1f84` 真实嵌入像素继续作为签收基线，Q00/Q25 账本未把未捕获宿主写成通过。旧 Worker `23304` 保持未触碰。

## 2026-09-18 Round3 R08-07 对话框遮罩同步

- `afa845a6` 在 `codex/ui-finesse-round2` 收口 `DashboardView` 对话框生命周期：Opening/Open/Closing/Closed 状态与一次性完成声明独立管理，关闭期保持 active，遮罩仅在卡片退场完成后折叠；`DialogOverlayMotion` 负责动画清理、减动效归一化与 watchdog 终态保护。
- R08-07 的可重复证据为隔离 Release XAML `24/24`、构建 `0 warning / 0 error`、按类 R08 `16/16`、Core `83/83`、Worker `310/311`（1 skip）、源码校验与 diff check 通过。一个 testhost 合并运行的 `6` 项失败属于 WPF 调度/视觉资源污染，不能写成产品通过或回归。
- C: worktree 的 WPF 临时项目路径权限仍是环境限制；D: 临时隔离源目录只用于同源码构建核验。任何后续证据仍须区分 synthetic/offscreen logical DIP 与真实 Playnite/物理屏幕/呈现帧，不能把离屏、代理性能或 ETW 缺失升级为真实宿主结论。
- 下一步 R08-08 先查 `GscMotion` 的共用可变/冻结 `Freezable`、变换实例归属和已有 R00-02 覆盖，避免重复实现或把表格新增方向误判为缺失。

## 2026-09-19 Round3 R00/R01 当前提交复核

- 当前续作分支身份为 `3354fd82400df6659165a688b8fcb1eb87116ca4`。R00/R01 旧源码断言已按现有 `DialogOverlayMotion`、`IsBusyIndicatorVisible` 和关闭期焦点保护校正；RenderHarness/UiAuditRunner 已优先使用 `GscSourceRoot/GscBuildCommit`，隔离 `.tmp` 输出不会再解析成 main。
- 当前干净 Release/XAML 门禁为 `24/24`、`0 warning / 0 error`；当前提交的受影响行为测试、双主题合成探针、审计和证据索引已复核。新鲜度为 `14 fresh / 0 stale`，freshness 三类负例通过，包身份仍为 `not-provided`。
- 复核证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R00-R01-CURRENT-RECHECK-20260919.md`。R00-07、R01-01、R01-07 原先已 fresh，不为统一时间戳重复构建；不要把每条记录描述成同一批运行。
- Demo 原目录不可用，沿用恢复生产资源基线；离屏 synthetic/STA WPF 不能替代真实 Playnite、物理 DPI/跨屏、IME/UIA、presented frame、ETW、宿主性能或 package-host。main 的用户未提交改动不可覆盖。
- 下一可执行小批量为 R08-08：检查 `GscMotion` 的共享可变/冻结 `Freezable`、变换实例归属和 R00-02 覆盖，再决定最小改动。

## 2026-09-19 Round3 R08-08 变换所有权

- `194a16fe588a2f102047792a94252e25ba5e7714` 已推送。`GscMotion` 不能可靠枚举 WPF 可变 Freezable 的全部依赖属性所有者，因此外部 RenderTransform 首次进入 helper 时统一 `CloneCurrentValue()`，由 `MotionState.OwnedRenderTransform` 记录控件所有权；后续同一控件复用，不再嵌套新组。
- 行为覆盖了两个控件共享未冻结 `TranslateTransform`、两个控件共享带旋转子节点的 `TransformGroup`，以及 R00-02 的冻结组合树 1000 次复用；当前提交隔离构建 XAML `24/24`、0/0，Foundation `9/9`，R08 相关类串行通过。
- 不把该策略扩展解释为动态资源绑定或真实宿主呈现已验；离屏 STA/逻辑 DIP、物理 DPI/跨屏、UIA/读屏、presented frame、ETW、宿主性能和 package-host 仍分开记录。Demo 原目录不可用，保持生产资源基线。
- 下一可执行小批量为 R09-01：核对主题切换的资源更新顺序、Popup/占位图更新和负例，再决定是否需要共享资源修复。

## 2026-09-19 Round3 R09-01 主题转换闪白

- R09-01 没有生产实现缺口需要重建。`DashboardView.ApplyAdaptiveTheme` 与 Settings 的同名路径已将完整 palette 写入插件局部字典，并刷新打开的脱离式 ToolTip；游戏选框为宿主内 `GameBrowserScrim`，不使用 WPF Popup。所有页面/浮层/矢量图标/占位表面使用动态资源键。
- `fe048952` 的 `R09ThemeSwitchBehaviorTests` 在 STA WPF 中先应用 Light、再在 Background 回调切换 Dark，并在下一 Render 采样 Popup/Path/placeholder/text；颜色全部更新，独立 host 哨兵保持，`1/1`。干净隔离构建 XAML `24/24`、0/0。
- 证据仅证明同一 Dispatcher 的局部资源切换顺序和作用域；不把它写成物理屏幕 presented frame 无闪烁或真实 Playnite 系统主题验证。Demo 原目录不可用，继续使用恢复生产基线。
- 下一可执行小批量为 R09-02：盘点现有图标语义映射、尺寸、ThemeAwareIcon/PNG 使用和缺字负例。

## 2026-09-19 Round3 R09-02 图标语义统一

- 先核对现有能力：生产已经使用 `ThemeAwareIcon`、`GscIconPack.xaml` 的矢量 Geometry 和主题前景继承，不存在需要重建的字体图标体系；实际缺口是上传/校验/归类/忽略没有共享动作映射，且若干备份/恢复按钮仍是纯文字。
- `c7c7ae0d` 在原体系内补齐 `GscIconActionBackup/Restore/Upload/Verify/Categorize/Ignore` 和 `GscActionIcon`（统一 `16x16`），并把真实生产动作按钮改为图标加原文案；命令、Binding、CommandParameter、危险样式、恢复保护和现有游戏选框/滚动条没有改变。
- 新增 `R09IconSemanticBehaviorTests`：从当前 checkout 文件流加载图标字典，实测六个 Geometry 非空且有边界；解析生产页面的真实 `IconData` 映射；禁用实际 WPF Button 后 IconData、可见性和 16x16 布局仍保留。精确提交隔离构建 XAML `24/24`、solution `0/0`，新夹具 `2/2`，相邻回归 `3/3`，源码校验/diff check 通过。
- 证据见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R09-02-ICON-SEMANTICS-20260919.md`。Demo 原目录不可用；合成 STA/offscreen logical DIP 不代表真实 Playnite、物理 DPI/跨屏、呈现帧、UIA/读屏、IME、ETW 或宿主性能。用户 DEV-INSTALL-008 main 全量 Playnite.Tests 失败保留为合入后单 checkout 安装器重跑边界，未与本阶段精确产物混写。
- 下一可执行小批量为 R09-03：先检查共享边框/分隔线/选中指示在 100/125/150/175/200% 的可模拟证据与真实宿主边界。

## 2026-09-19 Round3 R09-03 一像素描边

- 先查现有能力后确认主要缺口是共享圆角 Chrome 的像素吸附不一致：`GscRedesignWorkspaceTabItem` 与 Dashboard 同构 Tab 曾显式 `SnapsToDevicePixels=False`，Acrylic/Dashboard 导航 Chrome 没有明确声明。`083b7a22` 只在共享模板收口 `SnapsToDevicePixels=True` 与 `UseLayoutRounding=True`，没有引入新设计体系或改动业务契约。
- 行为夹具实例化真实生产 `TabItem`/`RadioButton` 的选中模板，并以离屏 `RenderTargetBitmap` 的显式 `1.00/1.25/1.50/1.75/2.00` render scale 检查 1 DIP 分隔线和圆角连接。最终 R09-03 `2/2`，相邻 R09-02/共享资源 `6/6`，当前提交重建 `24/24`、0/0。
- “五档 DPI”只能记为明确模拟：96-DPI 基线的显式位图缩放，不能上升为真实物理 DPI/跨屏或 Playnite 宿主呈现；真实 presented frame、UIA/读屏、IME、ETW、宿主性能和 package-host 继续单列。Demo 原始目录不可用，沿用恢复生产基线。
- 证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R09-03-PIXEL-STROKE-20260919.md`；main 的 `DashboardView.xaml.cs`、`src.zip`、R08 基础设施/测试用户改动继续保持未触碰。下一项 R09-04 阴影层次预算。

## 2026-09-19 Round3 R09-04 阴影层次预算

- 先复用既有 `ApplyMaterialResources` 与 `GscSurface/GscElevatedSurface`：现有资源已经把 shadow 限制为 surface/sidebar/popup/dialog/primary/slider 六种角色，不把所有卡片、列表和输入框都加 Effect；`glassEnabled=false` 通过真实 null 回退，避免保留 Opacity=0 的视觉管线。
- `3ad61099` 的真实 WPF 行为夹具验证浅/深主题冻结 `DropShadowEffect` 参数、low-cost null/透明 wash、PopupAnimation 关闭语义，并用三个实际卡片比较有/无 Effect 的 ScrollViewer `ExtentHeight`，确认阴影不扩大滚动范围。R09-04 `3/3`，组合相邻回归 `9/9`。
- 只把高对比回退记为未验：测试没有改 OS High Contrast，R09-06 仍需独立检查系统语义资源；离屏/逻辑布局不能替代真实 Playnite、呈现帧、物理 DPI/跨屏、UIA/读屏、IME、ETW 或宿主性能。Demo 原始目录不可用，沿用恢复生产基线。
- 证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R09-04-SHADOW-BUDGET-20260919.md`。main 用户未提交文件继续不碰。下一项 R09-05 焦点轮廓合成。

## 2026-09-19 Round3 R09-05 焦点轮廓合成

- 先查现有能力：共享焦点资源已经提供 `2 DIP`/圆角 `13` 的非颜色轮廓；按钮 FocusOverlay 覆盖完整圆角 Chrome，选中 Tab 的 Chrome `ClipToBounds=False`，输入框错误触发器在焦点触发器之后明确使用错误色与 `2 DIP` 边框，不需要另建设计体系。
- `3f7d0b30` 只新增行为证据 `R09FocusOutlineBehaviorTests`。实际生产资源与控件验证 Button 焦点/失焦、共享焦点模板实例、selected Tab、TextBox 错误态和有效值恢复，R09-05 `2/2`；xUnit collection 串行本组 WPF STA，并避免已关闭 Dispatcher 的跨测试 `Application.Current` 污染。
- 精确 Release 验证为 XAML `24/24`、solution `0/0`、Playnite `net462`；R09-02/R09-03/R09-04/共享焦点资源相邻合计 `11/11`，源码校验/XAML/diff check 通过。系统输入源缺失时不会自动挂载 Focus Adorner，已作为边界记录，不能把模板实例化冒充真实键盘呈现。
- 证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R09-05-FOCUS-OUTLINE-20260919.md`；真实 Playnite、物理 DPI/跨屏、presented frame、UIA/读屏、IME、OS High Contrast、ETW、宿主性能和 package-host 仍未验。Demo 原目录不可用，main 用户文件未触碰。下一项 R09-06 高对比真实配色。

## 2026-09-19 Round3 R09-06 高对比真实配色

- 复用 `AdaptiveThemePalette` 和既有资源键，没有另建主题体系。高对比 palette 现在显式记录 `IsHighContrast`；默认来自 `SystemParameters.HighContrast`，override 只供隔离测试，不写 Windows 设置。
- `1f2eac4a` 将高对比页面背景/文字/控件/边框/选中/禁用/进度/图标收口到 `SystemColors` 语义资源；玻璃、阴影、Popup transparency/动画、游戏背景模糊和 ambient wash 均关闭或回退到真实 null/透明；普通主题重新应用后材质 stop/ambient 恢复。
- `R09HighContrastBehaviorTests` 用实际 ProgressBar、Path、TextBlock 的 DynamicResource 绑定验证行为/负边界，R09-06 与相邻 R09/源码门禁 `13/13`；正式 Release `0/0`、XAML `24/24`、source/XAML/diff check 通过。此前直接 `dotnet build` 的身份失败已改用 `scripts/build.ps1` 正式复验，不计为产品失败。
- R09-06 证据：`evidence/R09-06-HIGH-CONTRAST-20260919.md`。不把隔离 palette override 说成真实 OS High Contrast、Playnite 呈现、物理 DPI/跨屏、UIA/读屏、IME、ETW 或宿主性能通过；DEV-INSTALL-008 main 安装失败继续保留发布边界。
- Demo 原目录不可用，继续沿用恢复生产资源基线；下一项 R09-07 缩略图占位一致，先盘点现有媒体加载/失败/无图/视频/损坏占位和行高约束。

## 2026-09-19 Round3 R09-07 缩略图占位一致

- 先查现有能力后确认 `AsyncThumbnailLoader` 已提供后台解码、取消、过期请求保护、冻结 `BitmapSource`、缓存和并发上限；本阶段没有重建服务或 DTO。`MediaCenterView` 的卡片已有固定 `164 x 154` 项和 `96/58` 行约束，缺口是列表截图/录像/缺失/损坏状态没有统一有文字的固定槽位。
- `72a1a07b` 新增 `MediaThumbnailPreview`，截图复用 `AsyncThumbnailImage`，状态映射为加载、无图、缺失、损坏、成功；录像/未知类型不启动截图占位。详情的截图状态文案补齐，视频状态交由既有 `MediaElement`，命令/绑定、取消/错误、安全、选框、滚动条和有限列表性能不变。
- 夹具是隔离临时媒体 + 真实 STA WPF Window：固定槽位/操作区与五类语义状态通过；R09-07 `1/1`，相关缩略图/缓存/转换 `9/9`，R09 `12/12`。正式脚本构建 XAML `24/24`、Release `0/0`，source/XAML/diff check 通过。
- UI 证据仍是逻辑 DIP/offscreen 行为，不是 Demo 像素、真实 Playnite presented frame、物理 DPI/跨屏、UIA/读屏、IME、ETW、宿主性能或 package-host 安装通过。用户 DEV-INSTALL-008 的 dirty main 全量 Playnite.Tests `73/588/57` 失败事实不与本阶段隔离通过混写；下一可执行任务：R09-08 主题背景压力。

## 2026-09-19 Round3 R09-08 主题背景压力

- 先复用现有 `AdaptiveThemePaletteContrastGuard`、`AdaptiveThemePaletteFactory` 和运行时资源，不重建主题体系。`7de5c3de` 新增 `R09BackgroundPressureBehaviorTests`，在浅色中性、深色中性、暖色浅背景、蓝色深背景四种合成宿主中读取实际 backdrop/ambient/glass 资源，检查透明 stop 仍存在，并按真实 alpha 层叠计算正文对比度；全部达到 `4.5`，故意失败负例 `1/1` 被拒绝。
- 复验发现 R09-06 后四参数 `AdaptiveThemePaletteFactory.Create` 的反射兼容入口被可选参数改坏，已恢复四参数入口并将隔离高对比 override 收口到 `CreateWithHighContrastOverride`；同步更新过时结构断言。主题/材质 `8/8`、高对比/主题/阴影 `5/5`、R09 `14/14`，正式 Release/XAML `24/24`、solution `0/0`，源码校验、XAML、diff check 通过。
- 证据见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R09-08-BACKGROUND-PRESSURE-20260919.md`。证据只覆盖隔离 STA/WPF、合成 ResourceDictionary 和逻辑 DIP；未验真实 Playnite presented frame、物理 DPI/跨屏、UIA/读屏、IME、ETW、宿主性能、package-host 和真实 Windows High Contrast。Demo 原目录不可用，沿用恢复生产基线；DEV-INSTALL-008 main 全量安装失败仍单列，main 用户文件未触碰。下一可执行任务：R10-01 上下文返回。

## 2026-09-19 Round3 R10-01 上下文返回

- 实现提交 `97770ed4`。优先复用稳定 ID、现有告警解析和任务 DTO；`WorkspaceNavigationSnapshot`/`WorkspaceNavigationStack` 只存当前会话 UI 状态，不写真实存档、媒体、云端或用户配置。
- 真实命令路径覆盖告警→存档/失败任务和任务→关联游戏；返回恢复工作区、页签、筛选、历史范围、任务/诊断选择以及 DataGrid 内部实际滚动偏移。返回时先切换工作区；删除或刷新导致原对象消失时不替换其他目标，并保留解释性状态消息。
- 定向 `13/13`、XAML `24/24`、Release `0 warning / 0 error`、source/XAML/diff check 通过。证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R10-01-CONTEXT-RETURN-20260919.md`；账本 R10-01 已改为“已满足”。
- 当前证据是隔离 fake/合成状态、STA WPF 和逻辑 DIP；不能升级为真实 Playnite、物理 DPI/跨屏、presented frame、UIA/读屏、IME、ETW、宿主性能或 package-host 结论。Demo 原目录不可用，沿用恢复生产基线。
- 用户提供的 main 安装器日志保留为独立边界：编译 `0/0`、Core `83/83`、Worker `311/311`，Playnite `73 failed / 588 passed / 57 skipped`；未在 dirty main 上覆盖或重跑安装器。下一可执行任务：R10-02。

## 2026-09-19 Round3 R10-02 定位当前游戏

- 现有能力已满足本项：任务详情入口按 `SelectedTask.GameId` 定位；媒体页的当前游戏名称和 Shell 选框沿用 `SelectedGame`，媒体请求使用 `SelectedGame.PlayniteId`。不按显示名定位，避免重名游戏误选。
- `3c258873` 的 `R10ContextGameBehaviorTests` 使用真实 `GamePickerViewModel` 验证两个同名 synthetic 游戏中按第二个 `PlayniteId` 选择；生产任务/媒体/Shell 接线同步复核。R10-02 `2/2`，相邻 R10/告警导航共 `15/15`，Release `0 warning / 0 error`、XAML `24/24`。
- 证据已写入 `evidence/R10-02-CURRENT-GAME-20260919.md` 并同步账本；没有新增服务、DTO 或设计体系。Demo 原目录不可用，沿用恢复生产基线。
- 未验真实 Playnite 定位操作、物理 DPI/跨屏、presented frame、UIA/读屏、IME、ETW、宿主性能、package-host；用户 main 安装器的 `73/588/57` 失败事实仍不改写为本项隔离失败。下一可执行任务：R10-03。

## 2026-09-19 Round3 R10-03 搜索快捷键

- 先查已有能力后复用 `DashboardView.FocusWorkspaceSearch`、现有各页 TextBox 和 Shell 的 `PickerOverlay` 状态，没有增加搜索服务、DTO 或新的快捷键体系。`563e6862` 只新增 `SearchShortcutPolicy` 作用域判断，并保持 IME、方向键、Enter、Esc 和 Playnite 全局绑定路径不变。
- 对话框、游戏选框、紧凑浏览器打开时 Ctrl+F 均拒绝搜索路由；Ctrl+Z/C 和无 Ctrl 的 F 负例也拒绝。`R10SearchShortcutBehaviorTests` 与相邻接线合计 `9/9`，Release `0 warning / 0 error`、XAML `24/24`。
- 证据见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R10-03-SEARCH-SHORTCUT-20260919.md`，账本已同步。Demo 原目录不可用，沿用恢复生产基线；未把隔离策略测试写成真实 Playnite 输入、UIA、呈现、DPI/跨屏、ETW、宿主性能或 package-host 结论。
- `.tmp/r10-03-build` 清理尝试遇到 Access denied，未强制终止未知 dotnet/testhost 进程；用户 main 的 DEV-INSTALL-008 `73/588/57` 失败事实仍独立保留。下一可执行任务：R10-04 快捷键帮助。

## 2026-09-19 Round3 R10-04 快捷键帮助

- 先查已有能力后复用 `RelayCommand`、`CanExecute`、`CurrentWorkspace` 和 R10-03 的 `FocusWorkspaceSearch`，没有新增服务或命令体系。`19be9f12` 增加 Shell 页头帮助按钮、Demo-first 浮层 Popup 和 `KeyboardShortcutHelpCatalog`；目录当前只展示已接线的 Ctrl+F，并过滤 `CanExecute=false`。
- 真实 STA WPF 帮助按钮交互：当前媒体页显示“媒体中心 / Ctrl+F”，Popup 可打开和关闭；目录正/负例、当前工作区说明、生产接线及 R10-03/R10-02/R10-01 相邻回归共 `13/13`。Release `0 warning / 0 error`、XAML `24/24`。
- 证据见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R10-04-KEYBOARD-HELP-20260919.md`，账本已同步。Demo 原目录不可用，沿用恢复生产基线；未把隔离 STA Popup 写成真实 Playnite 呈现、UIA、DPI/跨屏、IME、ETW、宿主性能或 package-host 结论。
- C: 盘空间不足的构建尝试改用 D: `.tmp` 成功并清理；旧 VBCSCompiler 锁定目录不强杀。用户 main `73/588/57` 失败事实仍独立保留。下一可执行任务：R10-05 筛选预设。
## 2026-09-19 Round3 R10-05 筛选预设

- 现有能力盘点结论：`PolicyTemplates` 是 Worker/备份策略模板，不能复用为筛选预设；可复用的是真实任务/媒体筛选属性、媒体收件箱模式、Playnite 设置 JSON、`RelayCommand` 和 `plugin.ConfirmAsync`。
- `005dc2c5` 的持久化模型 `FilterPresetDefinition` 必须保持纯标量：`Id/Name/Workspace` 和任务/媒体字符串字段；设置入口统一 `NormalizeMany`，丢弃空名称/未知工作区/重复 ID，限制 32 条，非法状态/范围/媒体值回退。不要把 `TaskStatusDto`、`GameStatusDto`、`MediaItemDto` 或 live ViewModel 写入配置。
- 任务游戏筛选当前协议是展示名字符串（已有查询 `GameName`/选项同步如此），只能记录为标量兼容事实；若后续要稳定 PlayniteId，需另建查询和迁移批次，不能暗中改变本阶段语义。
- 删除、重命名和同名覆盖必须经过确认；任务紧凑布局的预设行不能被响应式重排设为 0。继续保留游戏选框/滚动条/命令绑定/取消错误/恢复保护/有限列表性能。
- 证据门禁：R10-05 `4/4`、R10 相邻 `14/14`、直接相关 `25/25`，Release solution `0/0`、XAML `24/24`。全量 Playnite 当前 testhost `84/610/57` 只能作为混合 WPF/宿主边界；main 的 DEV-INSTALL-008 `73/588/57` 和退出 1 继续独立记录。
- Demo 原目录不可用；不把隔离 STA/offscreen/逻辑 DIP 写成真实 Playnite presented frame、物理 DPI/跨屏、UIA/IME、ETW 或宿主性能。下一可执行任务：R10-06 筛选来源提示。

## 2026-09-19 Round3 R10-06 筛选来源提示

- 继续沿用 Demo-first 和生产基线：来源提示使用现有 `GscDiagnosticHintBubble`，不新建视觉体系；任务页响应式预设行与当前滚动条系统保持不变。
- 导航条件是临时状态，不进设置 JSON。`HasTaskNavigationTarget`/`TaskNavigationSourceSummary` 只说明当前查询叠加的诊断游戏；`ClearTaskNavigationContextCommand` 只清除两个导航字段，必须不触碰用户搜索、状态、游戏、类型和历史范围草稿。全量“清除任务筛选”才清除全部字段。
- 可靠门禁顺序：先跑真实 TaskCenterView 的 STA 绑定正/负行为，再跑 R10 集合，最后跑 Release/net462/XAML/source/diff。源码 `Assert.Contains` 只作为接线补证，不可独立签收交互。
- 当前证据：`R10FilterSourceBehaviorTests 2/2`、R10 `16/16`、Release `0/0`、XAML `24/24`。完整脚本因 C: 磁盘空间耗尽未进入全量测试；真实 Playnite 清除交互、presented frame、DPI/跨屏、UIA/读屏、IME、ETW、宿主性能和 package-host 仍未验。下一可执行任务：R10-07 侧栏信息密度。

## 2026-09-19 Round3 R10-07 侧栏信息密度

- 现有生产实现已满足表格条件：`AcrylicProductionShellView` 的侧栏列按 `270/72 DIP` 收展，主区是相邻星号列且侧栏 ClipToBounds；`ApplySidebarLayout` 在折叠时隐藏标签、保留并居中图标，边界按钮同步 glyph/Tooltip/Automation 名称。品牌版本徽标只在展开态出现，没有导航计数徽标遮挡图标。
- `ProductionShellChromeSourceTests` 新增真实 STA WPF 行为测试：选中任务入口收展前后保持；7 个入口逐项检查图标、Tooltip、Automation 名称、Tab 键入口；长导航名称下版本徽标与品牌图标不相交且主区仍有宽度。生产 Shell `12/12`，R10 组合 `28/28`。
- 本阶段只补行为证据和文档，不新建视觉体系或修改生产 XAML；命令/绑定、游戏选框、滚动条、取消/错误语义、恢复保护、有限列表性能和 Playnite/net462 保持。Release `0/0`、XAML `24/24`、source/XAML/diff check 通过。
- 证据文件：`evidence/R10-07-SIDEBAR-DENSITY-20260919.md`。隔离 STA/合成长名称/逻辑 DIP 不能证明真实 Playnite、物理 DPI/跨屏、presented frame、UIA/读屏、真实键盘/IME、ETW 或宿主性能；Demo 原目录不可用。旧 `.tmp` 清理受 Access denied/锁定句柄影响且未强杀未知进程，main 用户文件未触碰。下一可执行任务：R10-08 最近操作续接。

## 2026-09-19 Round3 R10-08 最近操作续接

- R10-08 先核对现有能力：`OverviewTasks` 明确是任务历史，`Activities`/`OpenActivityCommand` 是全局活动，`GamePickerViewModel` 和 `Games` 已提供稳定 PlayniteId；未重建服务、DTO 或导航体系。
- `RecentAccessRecord` 是纯标量持久化模型：`PlayniteId`、白名单工作区、范围内 TabIndex、UTC 时间，最多 8 条，按稳定 ID 去重排序。`RecentAccessItem` 只从当前 `Games` 快照派生名称和工作区文案；快照替换后清理不存在的 ID，因此不保存名称、路径、DTO 或 ViewModel。
- `DashboardViewModel.RecentAccess.cs` 复用现有 `uiStateSave`、`GamePickerViewModel.SelectGame`、工作区页签属性和 `RequestWorkspaceLoad`。恢复入口有专用 `OpenRecentAccessCommand`；对象缺失时删除记录并说明，不自动替换其他对象。Overview 的独立列表最大 `280 DIP`、Recycling、本地滚动，保留游戏选框和既有滚动系统。
- `R10RecentAccessBehaviorTests` 通过 `2/2`：真实 Overview STA Window 命令点击一次；设置记录正/负行为验证上限、去重、归一化、移除清理和无路径 JSON。R10 `18/18`，定向 Release 编译（Playnite `net462`）成功，XAML `24/24`、source/XAML/diff check 通过。证据：`evidence/R10-08-RECENT-ACCESS-20260919.md`。
- 不把 synthetic/STA/offscreen/逻辑 DIP 写成真实 Playnite package-host、presented frame、物理 DPI/跨屏、UIA/读屏、IME、ETW 或宿主性能通过；Demo 原目录不可用，继续恢复生产基线。main DEV-INSTALL-008 `73/588/57`/安装器退出 1 仍是独立边界，main 用户文件未触碰。
- 下一可执行任务：R11-01 版本信息摘要；保持每阶段小批量、实现后验证再同步文档/提交。

## 2026-09-19 Round3 R11-01 版本信息摘要

- R11-01 复用现有备份 DTO：`BackupVersionDto` 已含 `CreatedUtc`、`TotalBytes`、`FileCount`、`IsLocked`、`SourceDevice`、`RestoreReadiness` 和显示属性；右侧版本详情已经有隔离可恢复性说明，未重建数据层。
- 历史表设备列改为 `SourceDisplay`，空来源回退“未知设备”；`ProtectionAndReadinessDisplay` 只组合锁定和恢复校验摘要。不要把锁定状态当作校验成功：状态模板仅在 `RestoreReadiness.Status == Ready` 使用成功色，未知/未验证必须中性。
- 长摘要仍通过状态 ToolTip/右侧详情承载，固定 DataGrid 列宽与滚动系统不变；恢复命令、取消/错误、保护语义、游戏选框和 net462 保持。
- `R11VersionSummaryBehaviorTests` `2/2`，Save 相邻 `13/13`，定向 Release 编译成功，XAML `24/24`、source/XAML/diff check 通过。证据：`evidence/R11-01-VERSION-SUMMARY-20260919.md`。
- 仅证明 synthetic DTO、真实 SaveCenterView/STA Window 和逻辑 DIP；未证明真实 Playnite/package-host、presented frame、物理 DPI/跨屏、UIA/读屏、IME、ETW、宿主性能。main DEV-INSTALL-008 `73/588/57`/安装器退出 1 仍独立记录，main 用户文件未触碰。下一可执行任务：R11-02 双版本对比选择。

## 2026-09-19 Round3 R11-02 双版本对比选择

- R11-02 先查明现有差异能力已完整存在：`BackupCompareRequestDto` 传递 `LeftBackupId/RightBackupId`，Worker 复用 `FileManifestDiffService.Compare(left,right)`，`BackupDiffDto` 已承载新增/删除/修改/未变化/大小增量；缺口是 UI 选择和方向语义，不是服务缺失。
- 生产 SaveCenter 比较页现在提供 `CompareLeftBackup`（A 基准）与 `CompareRightBackup`（B 对照）两个 ComboBox；选中版本变化时默认保持上一版本→当前版本，用户可选任意不同版本。`SwapCompareBackupCommand` 交换 A/B 后重新发起原有比较请求，摘要始终说明新增属于 B、删除属于 A。版本详情按钮也改为 A/B 文案，避免“上一版本”误导。
- 同一 BackupId（忽略大小写）、空选择或缺少稳定 ID均拒绝比较；同版本界面提示“不发起比较或恢复”，交换按钮只在已有比较结果且选择有效时可用。恢复命令、取消/错误语义、游戏选框、滚动系统、有限列表和 net462 兼容没有变更。
- `R11VersionComparisonBehaviorTests 2/2` 和相邻 `15/15` 通过；前后方向反例使用真实 Core diff service，视图证据使用真实 SaveCenterView/STA Window，不以字符串断言作为唯一交互证据。Release/net462 无 warning/error，XAML `24/24`，source/XAML/diff check 通过。证据：`evidence/R11-02-VERSION-COMPARISON-20260919.md`。
- 仍不可把隔离 STA/合成 manifest/逻辑 DIP扩写成真实 Worker IPC、归档读取、Playnite/package-host、presented frame、物理 DPI/跨屏、UIA/读屏、IME、ETW或宿主性能通过；Demo 原目录不可用。main DEV-INSTALL-008 `73/588/57`/安装器退出 1 仍独立记录，main 用户改动未触碰。下一可执行任务：R11-03 差异列表搜索。

## 2026-09-19 Round3 R11-03 差异列表搜索

- 差异页面已有 `BackupDiffDto` 的新增/修改/删除/未变化和 `CopyPathCommand`，但原三组 ItemsControl 没有筛选和有限窗口。新增 `BackupDiffPathFilter` 作为内存投影：按类型、路径片段匹配后各类最多显示 120 条，计数保留完整匹配数，`LoadMoreDiffPathsCommand` 逐步增加窗口。
- SaveCenter 比较页增加路径搜索、类型 ComboBox、清除筛选、匹配/显示摘要；每条路径用生产只读 TextBox 保持完整相对路径选择，并将原始值传给现有复制命令。`UnchangedCount` 始终作为零变化独立计数；非 Exact 状态显示未知差异提示，不混成安全或零变化。
- 本阶段还修正了 R11-02 行位扩展遗漏：比较标题、A/B、筛选、计数、差异列表现在是独立 Grid 行。`R11SaveWpf` xUnit 集合禁并行，专门保护真实 SaveCenterView STA 夹具不受 WPF 全局资源竞争影响。
- `R11DiffListSearchBehaviorTests 2/2`，R11-01/R11-02/R11-03 串行 `6/6`，R06 存档页相邻 `11/11`；Release/net462 无 warning/error，XAML `24/24`，source/XAML/diff check 通过。证据：`evidence/R11-03-DIFF-LIST-20260919.md`。
- 仍不可把合成 DTO/隔离 STA/逻辑 DIP写成真实 Worker IPC、归档读取、大型清单物理呈现、Playnite/package-host、presented frame、物理 DPI/跨屏、UIA/读屏、IME、ETW或宿主性能通过；Demo 原目录不可用。main DEV-INSTALL-008 `73/588/57`/安装器退出 1 独立保留，main 用户改动未触碰。下一可执行任务：R11-04 版本说明编辑。

## 2026-09-19 Round3 R11-04 版本说明编辑

- 版本备注能力必须继续复用 `BackupMetadataUpdateDto` → `EditBackupAsync` → `RefreshBackupHistoryAsync` → SQLite upsert；编辑字段只允许备注/锁定，不能引入归档路径或文件名编辑。
- `CancelBackupMetadataCommand` 是本地草稿回滚，不得调用 Worker IPC；dirty 状态通过 `HasBackupMetadataChanges` 驱动 CanExecute，回滚复用 `SyncBackupEditor` 并保留稳定 `BackupId`。
- 已验证：真实 SaveCenterView/STA 绑定探针 `R11VersionNoteBehaviorTests 3/3`；同说明重复版本由 `SelectionAnchorResolver` 按 `BackupId` 选择；隔离 SQLite Store 重建 `1/1`；R11 串行 `9/9`；R06 相邻 `11/11`；D 盘 Release solution `0/0`、XAML `24/24`、source validation/diff check 通过。
- 证据边界：SQLite 重建不等价真实 Worker/Ludusavi 进程重启和归档 IPC；未验 Playnite/package-host、presented frame、物理 DPI/跨屏、UIA/读屏、IME、ETW、宿主性能。C 盘空间为 0 时必须使用 D 盘 `GscBuildOutputRoot`，不要覆盖 main 或用户文件；main DEV-INSTALL-008 `73/588/57`/退出 1 继续独立记录。
- 下一可执行任务：R11-05 保护操作解释；先核对现有锁定/保留预览/解除条件文案和真实状态，再决定只补证据还是做最小缺口修复。

## 2026-09-19 Round3 R11-05 保护操作解释

- 保护规则的单一事实源仍是 Core `RetentionPlanner` 和 Worker `RetentionSimulationService`：`IsLocked`、`IsPreRestore`、健康恢复点在保留预览与应用重检中跳过。UI 只能解释该规则，不能另造删除判断。
- `BackupVersionDto.IsHealthProtected` 必须同时满足 Ready、正文件数、正总字节；这样与 Worker 的 `FileCount == 0 || TotalBytes <= 0` 严重异常判定一致。`IsRetentionProtected`、`RetentionProtectionGlyphDisplay`、`RetentionProtectionDisplay`、`RetentionProtectionExplanationDisplay` 复用 DTO 状态给历史行绑定。
- 详情锁定草稿的解释必须明确：锁定并保存后跳过；取消锁定并保存后下一次预览才按策略重新评估。取消仅是本阶段 R11-04 已验证的本地草稿语义，不能把未保存的 CheckBox 改动写成持久化事实。
- 已验证：`R11ProtectionBehaviorTests 2/2`（正例、Ready 空内容负例、SaveCenter 行绑定契约）、Core `RetentionPlannerTests 3/3`（解锁重新成为候选且其他保护仍跳过）、Worker 保护夹具 `2/2`（预览/应用保护行为）；R11 串行 `11/11`，R06 `11/11`，Release `0/0`，XAML `24/24`。
- 证据边界：真实 SaveCenterView STA 夹具只证明绑定契约，不证明 Playnite presented frame、物理 DPI/跨屏、UIA/读屏、IME、ETW 或宿主性能；Worker 夹具是隔离合成目录，不证明真实 Ludusavi IPC。Demo 原目录不可用；WPF 非提升 `wpftmp` Access denied 时使用 D 盘 `GscBuildOutputRoot`，禁止覆盖 main。
- 下一可执行任务：R11-06 备份前变更摘要；先查已有变更摘要、快照/dirty 状态和取消/错误语义，再补最小缺口与证据。

## 2026-09-19 Round3 R11-06 备份前变更摘要

- 备份前预览必须调用 Ludusavi 的 `--preview`，不应把 `SavePathCandidateDto`（路径发现候选）当成此次实际备份范围。预览 DTO 只读，不生成 BackupId，不创建任务/历史/云端状态。
- `BackupOrchestrator.PreviewAsync` 复用现有游戏匹配和 `LudusaviResultParser.ParseOperationSnapshot`；`LudusaviClient` preview 路径跳过备份目录创建。路径列表最多 120 条，`PathCount`/`TotalBytes` 保留完整摘要，避免大清单无限呈现。
- Ready、NoData、Unavailable、Error 必须分开；空数据不能冒充成功，工具错误不能冒充“没有变化”。真实 `BackupSelectedAsync` 仍发 `backup.game`，执行前重新扫描，不使用旧预览作安全保证。
- 已验证：Worker `BackupPreviewBehaviorTests 2/2`；真实 SaveCenterView/STA `R11BackupPreviewBehaviorTests 1/1`；R11 串行 `12/12`；R06 `11/11`；Release `0/0`；XAML `24/24`。
- 证据边界：合成 JSON/隔离 STA/测试宿主不等价真实 Ludusavi 版本输出、真实归档变化、Worker IPC、Playnite presented frame、DPI/跨屏、UIA/读屏、IME、ETW、宿主性能。Demo 原目录不可用；继续禁止触碰 main 和真实存档/媒体/云端/诊断。
- 下一可执行任务：R11-07 备份结果分层；先查现有 `TaskStatusDto`、`CloudTransferStatusDto`、本地成功/云端失败链路和 UI 状态，再补最小缺口。

## 2026-09-19 Round3 R11-07 备份结果分层

- 结果分层必须把本地版本与云端后续复制分开：本地成功后即保留历史版本；云端排队、镜像失败、认证待处理、传输中、已上传待远端校验、远端已校验不能覆盖本地成功。
- 复用 `CloudTransferStatusDto`/状态服务、现有 `RetryCloudUpload` IPC 和重试队列。`BackupResultDto` 是任务结果解释 DTO，不是第二套云端状态源；`TaskCoordinator` 与实时事件克隆必须复制它，否则 UI 事件会丢失补救状态。
- `BackupOrchestrator` 在本地历史持久化后设置本地成功；云端失败时保持任务 Failed 以保留错误语义，同时携带 `HasPartialSuccess`。Playnite 只豁免这种已确认本地成功的云失败，普通失败/取消仍抛出原通知；云端重试不再创建新的本地归档。
- WPF 门禁必须验证行为负例：排队状态结果卡片可见且“单独重试云端上传”可见；`Uploaded` 只显示待远端校验并隐藏上传重试；真实 SaveCenterView/STA 夹具 `3/3`，R11 `14/14`，Worker 分层/事件 `9/9`，云状态相邻 `20/20`。
- main DEV-INSTALL-008 事实独立保留：构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `73/588/57`、退出 `1`，尚未打包/安装。首个 SaveWorkspace 失败是属性插入导致的过期连续字符串断言；当前分支改为 XAML 元素关系验证，不把断言校正写成命令实际可达的全量证明。
- 证据边界：合成 DTO/fake/隔离 STA 和 D 盘可写副本不等于真实 Ludusavi/rclone/Worker IPC、云端/存档、Playnite 宿主/安装呈现、DPI/跨屏、UIA/IME、ETW、宿主性能；Demo 原目录不可用；不得修改 dirty main。
- 提交 `02860571` 已推送 `codex/ui-finesse-round2`；下一可执行任务按用户顺序为 R00/R01 小批量问题修复与证据校正，然后才回到 R11-08。

## 2026-09-19 Round3 R00/R01 合并后门禁纠偏

- 启动时继续把 main 失败日志与 continuation 分支证据分账：main DEV-INSTALL-008 为 Playnite `73 failed / 588 passed / 57 skipped`、安装器退出 `1`；首个 SaveWorkspace 失败是 XAML 属性插入造成的过期连续字符串断言，不能扩大成命令不可达结论。
- 采用 `scripts/run-playnite-tests-isolated.ps1`：先发现测试类，source 类合组，WPF 类每类独立 testhost；`OutputRoot` 提供时必须将 `TEMP/TMP` 指向该输出下的 `test-temp`。任何类返回非零即失败，不用 skip 掩盖失败。`scripts/build.ps1` 的 Playnite 门禁必须调用它。
- 断言修复必须优先解析 XAML 元素/属性关系并保留行为/负例；不要用更宽的字符串包含把交互、焦点、动画或性能签收掉。R08 动效使用有界 Dispatcher 状态等待，固定睡眠不能作为完成证据。
- 本批提交 `c975e16d` 已推送；D 盘 `build3` Release `0/0`、XAML `24/24`，source `65` 类组 + WPF `84` 类进程通过，Core `84/84`，Worker `322/1/0`，资源字典 `137/39/0`。新脚本需要 UTF-8 BOM 以通过 Windows PowerShell 5.1 source validation。
- 真正未验边界仍包括 Playnite/package-host 安装与呈现、物理 DPI/跨屏、UIA/IME、presented frame、ETW、宿主性能；不得读取/写入真实存档、媒体、云端或外发诊断。Demo 原目录不可用；下一任务为 R11-08 历史时间导航。

## 2026-09-19 Round3 R11-08 历史时间导航

- 历史时间导航的事实源是 `BackupVersionDto.CreatedUtc`、`CreatedLocal`、`BackupId`；不要新增时间 DTO、存储或 IPC。范围按本地日历日期而非固定 24 小时窗口，避免夏令时边界漂移。
- `BackupHistoryDateRange` 的活动范围排除未知时间，“全部时间”保留未知时间；同秒版本用 `CreatedUtc` 后接 `BackupId` 稳定排序。清除范围必须恢复完整历史。
- 历史表使用独立 `CollectionViewSource`，不能把 `Backups` 本身改成过滤视图，否则会污染 A/B 比较选择器和其他绑定。最近/更早跳转只在当前可见范围内选择，并复用现有 `SelectedBackup`/状态通知。
- 已验证：R11History `3/3`，R06 `4/4`，R11 版本摘要/保护 `4/4`，资源字典 `137/39/0`，组合 `148 passed / 39 skipped / 0 failed`；Release `0/0`，XAML `24/24`，source/XAML/diff check 通过。提交 `8cc329e4` 已推送。
- 证据边界不变：合成 DTO/fake/隔离 STA/隔离目录不等价真实 Playnite/package-host 安装呈现、物理 DPI/跨屏、UIA/IME、ETW 或宿主性能；Demo 原目录不可用；main DEV-INSTALL-008 `73/588/57`、安装器退出 `1` 独立保留，不能写成 main 已安装。
- 下一可执行任务：R12-01 恢复分步摘要；先复用已有恢复任务/状态 DTO 和取消、错误、保护语义。
