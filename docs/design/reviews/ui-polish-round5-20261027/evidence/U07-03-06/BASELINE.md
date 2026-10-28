# U07-03 / U07-06 Tooltip 边缘回退与 Toast 堆叠

2026-10-29 小批，基于 `842557f6` 的生产代码。仅增加 WPF 行为回归，没有修改生产 XAML、样式、命令、Binding 或业务逻辑。

- **U07-03**：使用真实 WPF `ToolTip` 与生产 Tooltip 样式，在主显示器工作区底边打开 TextBox 帮助提示。验证编辑器默认下锚点、可用空间不足时 Popup 实际翻到控件上方，且提示仍完整打开。此前显式位置优先、统一延迟、长路径换行与 Escape 保持焦点已有定向覆盖。
- **U07-06**：在生产 `DashboardView` Toast host 中连续创建 5 条通知，验证最多保留 4 条、最新在前、溢出时移除最旧项；关闭中间项后，在退出动画完成前剩余卡片顺序稳定，动画结束后正确回收该卡片和计时器。

SDK 8.0.423，隔离 WPF 定向组总计 7 项：6 通过、0 失败、1 个既有 opt-in 跳过（系统指针移动测试默认跳过）。结果：[TRX](tests/U07-03-06-focused.trx)，SHA-256 `4135DBB964AD17548789445A0533E02E7EAFD404EC2FB2F70D35FE790D2E1DFF`。本轮行为命令使用 `-m:1 -nodeReuse:false` 和 `RunConfiguration.DisableAppDomain=true`；编译与测试均退出码 0。

没有改动生产视觉资源，因此没有为该测试专门重跑 Render QA；上一轮 U07-03/04 与 U07-05/06 双主题 Render QA 证据继续对应未改动的生产视图。此新增测试不是实体键盘、IME、跨屏混合 DPI 或 Playnite 宿主验收。待本批提交后查询对应 GitHub baseline/SDK10 job；本轮总体完整构建、安装身份与启动链仍按 U09-05 最终候选门禁处理。
