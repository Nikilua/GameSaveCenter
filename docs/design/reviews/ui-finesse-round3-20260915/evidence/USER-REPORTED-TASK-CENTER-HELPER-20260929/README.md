# Task Center 辅助文案与队列按钮复核

日期：2026-09-29  
源码/测试身份：`f88bcb4dccbac0dc32001f0667de278d7028171a`。当前源码已满足用户截图中提到的三点，本批没有重复改写生产控件。

## 已有行为

- `RunningTaskCount > 0` 时，仅显示一行“后台任务会继续运行。”；`RunningTaskCount == 0` 时整条提示折叠。完整的离页不取消后台任务、返回后恢复阶段/进度的说明保留在 UI Automation HelpText 中。
- 队列摘要布局为 `58 DIP`；浅色、深色下“重试可恢复 / 重置列宽”按钮高度分别为 `30 / 36 DIP`，垂直中心偏差分别为 `0 / 0.33 DIP`。折叠筛选摘要和最近更新文本后按钮高度不变。
- 离页提示与队列筛选摘要取 `GscSecondaryTextBrush`：Light `#F24E5666`、Dark `#FFB9C0CC`；与语义信息蓝不同（Light `#FF256FBD`、Dark `#FF5CAAF0`）。
- 因此将 Task Center 的“按钮被三行摘要撑高、说明常显且偏蓝”记为当前源码已满足；用户截图运行实例的 DLL/提交身份未知，本次未推断其截图对应哪个版本。

## 验证

- 当前 checkout 提交后 Release 隔离构建：XAML `24/24`、解决方案 `0 warnings / 0 errors`。
- 精确 `net472` 行为测试 `4/4`：离页提示显示/折叠、后台任务不被页面卸载取消、Light/Dark 队列按钮高度/中心/颜色状态。
- 测试 DLL ProductVersion `0.6.73+f88bcb4dccbac0dc32001f0667de278d7028171a`；SHA-256 `FB545E6CF844657FC3E71983000B16D7830F820471C08AEB49AF55A97F4B7471`；MVID `ee871846-c041-4721-ad46-5d4edd326444`。
- 插件 DLL ProductVersion 同为 `0.6.73+f88bcb4dccbac0dc32001f0667de278d7028171a`；SHA-256 `F4FEC53393C0ECE03E0E7AD1D8317CD0FA2184C1BF9D8E0F463A047FE3E4FCA9`；MVID `3c257c3d-8479-4037-b638-7f4e922b029c`。
- [`task-center-helper-layout.trx`](task-center-helper-layout.trx) 包含两种主题的实际 DIP/颜色读数。早先用源码提交 `98800f1a` DLL 运行其中源码读取用例时被身份门禁拦截；该轮作废。随后从当前 `f88bcb4d` checkout 全量重建并运行，本记录只采纳新程序集的 `4/4` 结果。

## 未验证边界

DLL 只完成隔离构建，没有安装或加载到真实 Playnite。用户截图运行时的 DLL/MVID、窗口尺寸、主题和 DPI 未提供；本批未验证物理 DPI、系统输入或宿主最终呈现帧。

下一项继续核对 Media Center 的全局游戏选择与待归类批量操作选择是否重复；先检查当前绑定和截图运行实例身份，再决定是否需要改生产 UI。