# Q06 按钮悬停视觉探针隔离

此前根入口在源码提交 `2caafef153f368034b6232f4ab6eb3db1e9d0687` 上运行 `20261025T081807812Z-6979bbcf`，在 Q06 的 Light/hover 采样得到 `mouseOver=False`、`hoverOverlay=0`，总计 1299 项 1280 通过、18 跳过、1 失败；未进入打包或安装。

根因是夹具调用进程级 `MouseDevice.ChangeMouseOver`，而 WPF 的桌面指针同步会在窗口消息处理中重置合成状态。测试现在用 WPF `UIElement.IsMouseOverPropertyKey` 在不可命中测试的夹具上直接设置只读依赖属性。生产按钮共享模板仍由真实 `IsMouseOver` 属性触发，HoverOverlay 不透明度仍须大于 0.9；浅色、深色各自都验证原五态矩阵。夹具不再修改全局 MouseDevice。此证明仅覆盖按钮状态的视觉触发，不等同于物理鼠标输入或真实 Playnite 宿主输入。

候选源码在 SDK 8.0.423 / Release 下完成解决方案构建，0 警告、0 错误。Q06 完整十状态测试以隔离进程运行 5 次，5/5 通过；每次 TRX 均为 1/1。第 2 轮保留了双主题十张状态图，Light 与 Dark hover 图中覆盖层均达到 1.0。

测试源码 SHA-256：`0EC744C45E50C1AF25638699170C13CE4A4DB5C8D86A04D39D3C081F87906D5F`。候选测试程序集 SHA-256：`C85228427EEC29F123C8652152C76756F48FF48A0CEE8073FFF7D29C46E0EEB5`。Build diagnostics summary 记录了实际源文件与程序集摘要；此次目标构建是在 `2caafef` 基础上包含未提交测试夹具改动，程序集版本元数据仍指向基础提交。

单类重复结果与完整状态报告见 [verification.json](verification.json)、[状态报告](button-state-probe-report.txt)、[首轮 TRX](tests/q06-dpkey-2.trx) 和 [SHA256SUMS.txt](SHA256SUMS.txt)。完整根入口构建、打包、安装和启动仍待此修复提交后复验，不能用本证据替代。

| Theme | Hover capture |
| --- | --- |
| Light | [light-hover.png](captures/light-hover.png) |
| Dark | [dark-hover.png](captures/dark-hover.png) |
