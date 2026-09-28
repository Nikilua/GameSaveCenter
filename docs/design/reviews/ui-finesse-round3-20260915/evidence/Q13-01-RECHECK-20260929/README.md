# Q13-01 失焦选中当前 main 复核（2026-09-29）

## 结果

Q13-01 的共享选中/焦点视觉与可控行为已经由现有实现覆盖；本阶段复核当前 main，不重做产品样式，也不新增生产代码。根据任务表，Q13-01 最终状态仍为“未完成”：真实 Playnite 宿主中的鼠标/键盘失焦操作及最终呈现没有在当前环境验证。

当前身份 `d46d6be5d63b49dd327b641ac661d26c9fd86104` 的 Release solution 构建成功，XAML `24/24`，`0 warning / 0 error`。Playnite test assembly SHA-256：`72F71A98D00DC21A29CDCA1B7E244B66DDE4DCE894CF9EF13256C07490316519`。

## 行为覆盖

- `R06SelectionStateBehaviorTests`：`2/2` passed、0 failed/skipped。实际 STA WPF DataGrid 先让成功行获得键盘焦点，再将焦点移到外部 TextBox；选中状态保留，并从活动 accent selection 切到 `GscSelectionInactiveBrush`/muted outline。失败行仍留在其状态单元格，失焦选中表面不覆盖错误色；共享 CellChrome 保持透明。
- `R23ProductionResourceStateBehaviorTests.EachProductionPageGridKeepsSelectedFocusAndDisabledRowStatesAcrossThemes`：`1/1` passed、0 failed/skipped。实际 Task、Media Inbox、Save、Maintenance 生产 DataGrid 在 Light/Dark 中验证未选、失焦选中、键盘焦点选中与禁用状态。测量每个 cell/TextBlock 相对表格的 x/y/宽/高，状态切换误差门限 `<=0.25 DIP`。
- 两份最终 TRX 与 `d46d6be5` test assembly 对应，见本目录。

此前 `R06-03` 已对照 Demo-first 生产基线完成 Light/Dark RenderHarness 357 PNG `render-qa OK`，并实际抽查错误状态徽章/危险动作；`R06-DATAGRID-SELECTION-GEOMETRY-CURRENT-MAIN-20260924.md` 另覆盖修复后的生产行状态几何。Media Inbox 主表 `212 DIP` 门禁和真实滚动边界见 `MEDIA-INBOX-HOST-SCROLL-DIAGNOSTIC-PREP-20260928.md`。这些证据支持自动行为与视觉条件，不代表本次重新跑了截图或真实 Playnite 输入。

## 边界

本次测试使用合成 DTO、隔离 STA WPF Window 与逻辑 DIP；未启动真实 Playnite、未使用 OS 级失焦键鼠输入、UIA/读屏、物理 DPI 或 presented frame。未操作用户存档、媒体、云端或诊断外发。Q13-01 可控代码/行为/视觉条件已有证据，宿主栏仍为外部待验，最终结论不提升为已完成。

下一项为 Q13-02 多选反馈：先盘点现有 `DataGrid` `SelectionMode=Extended`、选择计数与半选状态，再以合成数据验证真实 WPF Ctrl/Shift 路由和拒绝/边界序列，不能只用 `Assert.Contains` 签收手势。
