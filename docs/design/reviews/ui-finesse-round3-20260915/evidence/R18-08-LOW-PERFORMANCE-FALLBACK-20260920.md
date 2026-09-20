# R18-08 低性能降级触发证据

日期：2026-09-20  
任务：`R18-08` 低性能降级触发  
代码：`3bfe3d3c`  
原始报告：[`lowcostprobe-report.txt`](../../../../../../.tmp/r18-08-lowcost-final-clean/lowcostprobe-report.txt)  
代表图：[`Media Light 1600×900`](../../../../../../.tmp/r18-08-lowcost-final-clean/media-light-1600x900.png)、[`Trainer Light 1600×900`](../../../../../../.tmp/r18-08-lowcost-final-clean/trainer-light-1600x900.png)

## 结论

R18-08 在“明确模拟的低成本回退”范围内已满足：现有无玻璃/无动画路径在六个工作区、浅色/深色和两种窗口尺寸下保持资源、文本和滚动边界可读，且没有非预期页面横向溢出。此前夹具发现 Media Inspector 的归类建议列表因横向 Auto 测量产生真实可见溢出，已在生产 XAML 中将归类建议列表和同构历史列表的水平滚动明确关闭；垂直有限列表、Recycling 和既有操作保留。

这不是硬件 `RenderCapability.Tier`、真实低 Tier GPU、Playnite package-host 或最终屏幕呈现证据。本次只证明显式 `glass=false`、`motion=false` 模拟条件下的功能/文本回退；系统高对比度、真实 Playnite 主题和物理 DPI 仍需环境验证。

## 受控运行

- clean-tree Release RenderHarness，commit `3bfe3d3c1edcb9fb370898cafc30bfd11e3eec86`，`WorkingTreeClean=True`。
- `lowcostprobe` 显式使用 `glassEnabled: false, motionEnabled: false`；六个工作区 `Overview/Save/Trainer/Media/Maintenance/Task`，Light/Dark，`1040×700` 与 `1600×900`，共 `24` 个组合。
- 资源门禁：每个组合 `GscSurfaceEffect`、各按钮/侧栏/弹层/对话框/滑块 Effect 和游戏背景 Effect 均为 `null`；`GscPopupAllowsTransparency=False`、`GscPopupAnimation=None`、`GscShellAmbientOpacity=0`、`GscGameBackgroundOpacity=0`。
- 结果：`visibleEffects=0`；每个组合 `unexpectedOverflow=0`；可见非空 TextBlock 数为 `4–147`；最终 `lowcostprobe OK`。
- DataGrid 的 `DG_ScrollViewer` 水平滚动仍保留为合法表格容器行为；技术路径 TextBox 内部的横向内容滚动记为 `textInputOverflow`，不计为页面布局溢出。

## 实际缺口与修复

首轮 clean-ish 模拟运行在 Trainer/Media 1600×900 暴露 4 个溢出报告。可视树诊断确认 Trainer 和 Media Inspector 的部分项是路径 TextBox 的 `PART_ContentHost`，但 Media 仍有一个真正可见的 unnamed ScrollViewer，其归属为 `ListBox#MediaClassificationPreviewItems`。该列表的文本应换行，却因横向 Auto 让 StackPanel 以无限宽测量。

`3bfe3d3c` 做了最小修复：

- `MediaClassificationPreviewItems` 增加 `ScrollViewer.HorizontalScrollBarVisibility="Disabled"`；
- 同构 `MediaClassificationHistoryList` 同步明确水平滚动关闭；
- 保留垂直 `Auto`、`CanContentScroll`、有限 `MaxHeight=260` 和 Recycling；
- RenderHarness 将 TextBox 内部承载与可见页面结构溢出分开，并记录可视树归属，避免只用字符串断言收口。

修复后 clean-tree 运行的 Media Light 代表图保持右侧 Inspector、路径复制和底部操作可见；Trainer Light 代表图保持工具列表、设置表单和滚动条可用。

## 质量门禁与边界

- 直接相关回归 `37/37`：低成本源契约、R18 表格容器、Media Inbox 几何和媒体窗口锚点；Release 构建 `0 errors`，仅有既存 `MediaCenterView.xaml.cs:671` 的 2 条 nullable warning。
- 更宽的联合筛选为 `40 passed / 2 failed / 42 total`；两个失败是已有 R14 源断言与本批无关：旧断言查找不存在的 `Kind = "SourceRule"` 文本，以及把 Media 页面 TabControl 的 `SelectedIndex` 误判为目标选择。未将联合结果写成全绿。
- `validate-source.py`、`git diff --check` 通过；代表图来自隔离 WPF RenderHarness/offscreen logical DIP，不是 presented frame。
- `GscMotion.IsEnabled` 已尊重设置、系统高对比度和 `SystemParameters.ClientAreaAnimation`；`NormalizeAll` 清除现有时钟并恢复基值。生产材质回退使用真实 null Effect、关闭 PopupAnimation、隐藏环境层并使用不透明表面。
- 未修改游戏选框、滚动条系统、命令绑定、取消/错误语义、恢复保护、有限列表性能或 Playnite/net462 契约；没有真实存档、媒体、云端或诊断写入。
- Demo 原目录不可用，沿用恢复生产基线。未运行真实低 Render Tier、Playnite package-host、硬件 GPU、物理 DPI/跨屏、UIA/读屏、ETW 或真实用户输入；不把 `DpiScale=1.00` 和离屏 PNG 写成这些证据。

下一可执行任务：`R19-01` 异步竞态与故障恢复入口，先核对现有请求 generation、取消、晚返回和 Worker/页面状态投影，再选一个边界明确的小批量。
