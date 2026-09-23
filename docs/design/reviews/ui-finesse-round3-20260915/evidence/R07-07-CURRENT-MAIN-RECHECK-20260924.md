# R07-07 当前 main 复核（2026-09-24）

## 结论

R07-07 的细滚动、反向、末端和可视容器预算在当前 `main` 上仍通过受控验收。复核源码身份：`d46fc76e8da76814a6f4870880f98f153c2581cd`。`ScrollBoundaryRoutingBehavior` 自初次验收提交 `98e8b244` 后未改动，但共享 `WpfUiProduction`/`Redesign` 资源及 DataGrid/Task 布局后来发生变化，因此本次重新测量当前 WPF 控件。

## 构建、用例与实测

- 当前 main Release solution 构建成功，XAML `24/24`、`0 errors`；保留 `MediaCenterView.xaml.cs:703` 两条既有 `CS8602` warning。Playnite 为 `net462`、测试 `net472`。
- 当前滚动和相邻视口组 `19 passed / 0 failed / 0 skipped`，VSTest exit `0`。组成：`R07FineScrollBehaviorTests 2/2`、`R07ScrollOwnershipBehaviorTests 2/2`、`MediaInboxGeometryTests 3/3`、`ResponsiveLayoutCoordinatorTests 5/5`、`TaskCenterViewResponsiveTests 7/7`。TRX：[当前 main 结果](R07-07-CURRENT-MAIN-RECHECK-20260924.trx)。
- 真实生产 DataGrid 内部 ScrollViewer、160 行合成表格、隔离 STA Window：12 个 `-30` 路由滚轮增量将 offset 从 `0` 单调推到 `36 DIP`，每次推进 `3 DIP`；3 个 `+120` 反向样本为 `33→30→27`。末端 `154/154 DIP` 稳定；20 个滚轮事件共 `17` 次 `LayoutUpdated`，末端连续 5 个无位移事件新增布局 `0`；实测可见 DataGridRow `6–9`。
- 嵌套边界 `-360` 单事件将外层 offset `32→80/432 DIP`，内层保持 `628 DIP`，未超出外层最大值；样本记录 `1` 次布局更新、`2` 个可见内容容器。指标仍来自受控 WPF，而非宿主帧率或物理设备性能。

## 边界

事件是合成 routed wheel，而非物理触控板轨迹。未启动 Playnite，不代表 OS 输入栈、真实滚轮设备、物理 DPI/跨屏、UIA/读屏、presented frame、ETW 或宿主性能通过。测试使用合成行数据与逻辑 DIP，没有写真实存档、媒体、云端或诊断。保留现有滚动条系统、DataGrid 虚拟化/`CanContentScroll`、游戏选框和 net462 兼容。

**R07-07：已满足受控验收，真实触控板/宿主输入仍待验。下一可执行任务：R07-08 resize 压力序列当前 main 复核。**
