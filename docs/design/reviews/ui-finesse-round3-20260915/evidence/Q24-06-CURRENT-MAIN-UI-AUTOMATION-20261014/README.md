# Q24-06 当前 main UI Automation 复核

日期：2026-10-14。状态：**代码完成待验收**。生产 WPF 控件的自动化名称、帮助文本和原生表格 Pattern 已在受控窗口中验证；任务要求的真实 Narrator/Playnite 读屏抽查尚未执行，因此不标为“已满足”。

## 复核与改动

- 原有 `R21AutomationValueBehaviorTests` 有真实 WPF peer 的 Toggle/RangeValue/Selection 状态测试，但“媒体列表选择”只对合成 `ListBox` 检查了 Selection Pattern，生产 DataGrid 多靠 XAML 文本断言；生产表格也有多个缺少 Name 的 peer。
- 为生产页 11 个 DataGrid 补充语义 Name；保留原有复制快捷键 HelpText，并为原先无帮助文本的维护表补简短用途说明。未改 DataContext、ItemsSource、Command、SelectionMode、虚拟化或滚动资源。
- 新行为测试实例化 Task、Media、Save、Maintenance 四个生产 UserControl，逐个读取实际 DataGrid AutomationPeer 的 Name/HelpText，并取得 `IGridProvider`、`ITableProvider`、`ISelectionProvider`；检查 Table 行主序和直接命名（这些表格没有配置 `LabeledBy`）。媒体收件箱用两个合成行改变实际选中行，并从 Selection Pattern 读回对应行名称。
- 对实际 Task 页图标式关闭详情按钮，peer 返回“收起所选任务详情”，没有把图标内容当名称。进度 RangeValue 行为测试覆盖 42%、未知值和 120% 上限，确认 `AutomationLiveSetting.Off`，状态改变不把键盘焦点从文本输入移走。进度值使用合成 `TaskStatusDto`，生产 XAML 名称/绑定继续由既有契约测试检查。
- 全仓没有 `AutomationProperties.LabeledBy` 用法。现有生产输入/动作采用显式 `AutomationProperties.Name`；本次直接验证表格 peer 的 Name 与 HelpText，不为表格虚构外部标签关系。

## 本机验证

- 源码身份：`af8ed430373c5e52dba77af534c689f6727bb91f`；分支 `main`。构建设置 `GSC_BUILD_COMMIT` 为该精确 SHA。
- 隔离本机 SDK **8.0.423**；Playnite 插件 `net462`，测试 `net472`；VSTest `17.11.1`、测试宿主 .NET Framework `4.8.9181.0`。
- Release 测试项目与依赖构建成功：`0 warnings / 0 errors`。`R21AutomationValueBehaviorTests` 为 `24/24`；R00-06 关联的 `MediaWindowAnchorContractTests` + `MediaInboxGeometryTests` 为 `13/13`。
- `python scripts/validate-source.py` 与 `git diff --check` 通过。WPF 静态检查为 `0 errors / 29 warnings / 177 info`；这是全仓结果，无布局改动。
- 未改视觉布局，未运行 RenderHarness 或产出截图。受控 STA Window 和进程内 WPF AutomationPeer 不等于操作系统 UIA 客户端、Narrator 或 Playnite 宿主。

| 产物 | SHA-256 / 身份 |
| --- | --- |
| `tests/Q24-06-ui-automation.trx`（24/24） | `A8E87F6FC7FC8A5F2A0BC8D016E3707E8C2528F7EED619074248C1A6B859E04E` |
| `tests/Q24-06-media-freshness.trx`（13/13） | `D994FBB594E0B8F05425B2B717F9194B54E8A77F99DB03B0BE3010E6C2036107` |
| 测试源码 `R21AutomationValueBehaviorTests.cs` | `B2CEDE58630CDB9AAAE198388D754B70A80DFB90A02C8A01D8228414328462E5` |
| 插件 DLL | SHA `0D3FFE22F803E8791F547C13C8EB3D8BB5320BC245C32634D44F239E54DDCB30`；MVID `9fa10fe5-f3c0-4129-ad96-989bc5dc279f` |
| 测试 DLL | SHA `64CC6E7CCF65D58F50A4B56A24138FAB67F85DAB444272D22A26FA31B9F41B95`；MVID `d107c1d8-c2d9-48c1-81ec-6c4e11ef690d` |
| 两个 DLL ProductVersion | `0.6.73+af8ed430373c5e52dba77af534c689f6727bb91f` |

## R00/R01 freshness 校正

本次修改命中 `MediaCenterView.xaml`。修改前完整 freshness 检查仅报 R00-06 需要重跑；R00-01/02、03、04、05、07、08 与 R01-01 至 R01-07 均未命中本次路径。对 R00-06 的 `MediaWindowAnchorContractTests` 和 `MediaInboxGeometryTests` 以精确源码身份重跑为 `13/13`，覆盖媒体锚点、行/表格内容区预算和窄窗滚动回退。R00-06 baseline 的 sourceCommit 更新为 `af8ed430...`，其余记录不改；完整重算应为 `14 records / 0 needsRerun`。package identity 未提供。

## 未验边界与下一步

- 真实 Narrator 对名称、帮助文本、选中行变化和进度更新的播报频率仍待人工抽查；也未用 Inspect.exe/Accessibility Insights 连接 Playnite。当前测试只证明 WPF peer 可返回这些值与 Pattern，不保证屏幕阅读器的实际口播节奏。
- 下一次宿主验收可在隔离 Playnite 中用合成任务、媒体和维护结果逐表格读取 Name/HelpText，切换选中行并检查行播报；观察任务进度变化是否过度打断，并确认图标式关闭操作有可理解名称。不得使用真实存档、媒体或云端数据。
- 本机 SDK 8.0.423 不替代 CI SDK 10.0.401。推送后应继续跟踪 SDK8/SDK10 Actions 到明确终态；如果失败，先取得完整日志再定位。#971 的 SDK10 诊断仍缺，不以相邻 ZIP 推断其根因。
- 下一可执行任务为 Q24-07 高对比组合；Q24-06 的真实读屏/Playnite 验收保留为外部待验。
