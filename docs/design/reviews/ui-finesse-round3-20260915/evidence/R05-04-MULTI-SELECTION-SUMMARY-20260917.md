# R05-04 多选摘要证据

日期：2026-09-17  
提交：`52900815c1fb8a16550446b9ee8d5318b8238a25`  
任务：`R05-04 | 多选摘要`

## 1. 现有能力核对与本轮改动

- 媒体收件箱原已有 `DataGrid.SelectionMode=Extended`、按媒体 ID 保留当前模式的选择集合、加载更多后的选择恢复、批量命令提交前去重/无效 ID 统计，以及与当前窗口不一致时的例外提示。
- 当前游戏媒体卡片只有单项详情/元数据动作，没有把它误扩展成不存在的批量提交入口；本轮只收口真实存在的媒体收件箱批量栏。
- `MediaInboxBatchSelectionSummary` 现在按总选择数表达 0/1/多选；当保留集合中有当前窗口不可见的 ID 时，明确显示“当前窗口 N 项可操作”和“另 M 项暂不可见”，不把不可见项伪装成会提交的项目。
- 增加独立的“清空选择”按钮。它清除当前收件箱视图/模式的 DataGrid 选择和按 ID 保留集合，不修改收件箱模式、当前游戏媒体搜索或类型筛选；原有批量命令、去重、无效 ID、安全确认和错误反馈链未改。

## 2. 实际行为证据

测试命令：

```text
dotnet vstest .\tests\GameSaveCenter.Playnite.Tests\bin\Release\net472\GameSaveCenter.Playnite.Tests.dll --TestCaseFilter:'FullyQualifiedName~R05' --logger:'console;verbosity=minimal'
```

绑定当前提交的官方测试程序集结果：`失败 0，通过 11，跳过 0，总计 11`。

其中 `R05MultiSelectionSummaryBehaviorTests` 为实际生产 `MediaCenterView`、生产 `MediaInboxGrid`/摘要 TextBlock/清空 Button/模式 ComboBox 和隔离 STA WPF Window：

1. 空选择显示 `未选择媒体 · Ctrl / Shift 多选`，清空按钮隐藏；选择 1 项显示 `已选 1 项 · 可批量处理`；选择多项显示总数，不堆叠媒体文件名。
2. 注入一个已保留但当前窗口不可见的媒体 ID，再通过实际 DataGrid 选择两项，摘要显示 `已选 3 项 · 当前窗口 2 项可操作 · 另 1 项暂不可见`，区分总选择和当前命令可操作范围。
3. 在收件箱模式保持 `已忽略` 时点击实际清空按钮，选择归零、摘要和按钮状态复位，模式仍为 `已忽略`；没有调用媒体搜索/类型筛选清空命令。

已有 R05-01/02/03 回归也在同一轮通过：焦点边界、2000 项选框虚拟化键盘导航、弹层边界共 8 项，加本轮 3 项合计 `11/11`。

## 3. 构建与视觉核对

- `scripts/build.ps1 -Configuration Release -SkipTests`：XAML `24/24`，解决方案构建 `0 warning / 0 error`；Playnite/net462 项目仍按原目标构建。
- `scripts/render-qa.ps1 -Configuration Release -Output .tmp/r05-04-render-clean`：报告绑定完整提交 `52900815...`，`WorkingTreeClean=True`，Light/Dark，357 张 PNG，`render-qa OK`。
- 人工抽查 `.tmp/r05-04-render-clean/theme/light/Media-1040x700.png` 与 `theme/dark/Media-1040x700.png`：批量处理栏的摘要、清空选择、收件箱模式、目标游戏和主动作保持同组，页内列表/滚动条和 Demo-first 层级未被破坏。

## 4. 边界与未验证项

行为测试是隔离合成数据、生产 WPF 视觉树和 STA Window 的路由验证，不等价真实 Playnite 嵌入、真实鼠标/键盘、系统输入法、屏幕阅读器或 UIA Pattern。RenderHarness 是 offscreen logical DIP（`DpiScale=1.00`），不代表物理 DPI/多屏、presented frame、无闪屏、ETW 或宿主帧率/性能。没有写真实存档、媒体、云端或发送诊断；未把被保留但删除的真实后端记录推断成可恢复项，摘要只表达“暂不可见”。

下一可执行任务：`R05-05` 复选框三态；先核对实际消费点和当前 Q10-02 半选资源，再决定是记“已满足”还是补行为/UIA 证据。
