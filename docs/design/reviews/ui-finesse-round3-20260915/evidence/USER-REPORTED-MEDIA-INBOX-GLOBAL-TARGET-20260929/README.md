# Media Inbox 全局目标与批量归类控件核对（已满足）

日期：2026-09-29  
当前 `main` 最近源码构建身份：`f88bcb4dccbac0dc32001f0667de278d7028171a`。相关生产实现来自 `4f778e9b`（“修复媒体收件箱全局目标归类”）；`f88bcb4d` 之后到当前 HEAD `32997511` 只有文档/证据变化，`src/` 与 `tests/` 无差异。

## 当前代码行为

- 待归类批量操作区已经没有第二个游戏选择 `ComboBox`。`MediaInboxGlobalTargetSummary` 是只读的 `220×36 DIP` 当前目标摘要，绑定全局 `SelectedGame.Name`；完整 Playnite 身份留在 ToolTip 与 UI Automation HelpText。媒体 Inspector 也显示同一个只读上下文。
- “归类所选”命令接收所选媒体 ID；目标游戏直接来自全局 `SelectedGame`，为空时命令不可执行；确认对话框显示目标名，实际请求使用该游戏的 `PlayniteId`。
- Light/Dark WPF 行为测试实测全局目标由第一项切换到合成第二项、清除为“未选择游戏”、再恢复时两个只读摘要同步更新；三按钮与目标摘要均 `36 DIP` 且中心偏差为 `0 DIP`。选择器没有成为第二份可编辑状态。

## 验证身份与记录

- Release solution/XAML：`0 warnings / 0 errors`、`24/24`。行为测试 `CompactInboxKeepsBatchButtonsCompactAndTheGridInsideItsFrameRow`：浅/深色 `2/2` 通过。
- 测试 DLL（`f88bcb4d`）：SHA-256 `FB545E6CF844657FC3E71983000B16D7830F820471C08AEB49AF55A97F4B7471`；MVID `ee871846-c041-4721-ad46-5d4edd326444`。
- 插件 DLL（`f88bcb4d`）：SHA-256 `F4FEC53393C0ECE03E0E7AD1D8317CD0FA2184C1BF9D8E0F463A047FE3E4FCA9`；MVID `3c257c3d-8479-4037-b638-7f4e922b029c`。
- [`media-inbox-global-target-layout.trx`](media-inbox-global-target-layout.trx) 输出完整包含两个主题下目标切换、清空/恢复、按钮/目标高度、滚动条边界和页尾可达几何读数。

## 边界

当前代码和离屏行为已证明没有冗余的第二个选择器；本批没有安装/启动 Playnite，因此用户截图所示运行实例 DLL 身份仍未知。未验证真实宿主呈现或物理 DPI。Media Inbox 滚动后行偏移问题仍是另一条未完成的真实宿主诊断，不因本证据升级。