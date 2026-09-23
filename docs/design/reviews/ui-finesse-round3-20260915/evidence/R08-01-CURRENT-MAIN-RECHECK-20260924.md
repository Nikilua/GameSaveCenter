# R08-01 当前 main 复核（2026-09-24）

## 结论

当前 main 的动画中途反向验收通过。生产源码在 `0c0869a2a95a9e949af5dc257bf7c7b4da2f80b6`；当前测试/文档提交 `4b7f0a34` 没有改动该生产布局或动画实现。复用已有 `GscMotion` 每元素 generation guard 和侧栏 transition generation，不重建动画服务。

## 当前构建与测试

- 当前 main Release 全解决方案构建成功：XAML `24/24`、`0 error`；保留两条既有 `MediaCenterView.xaml.cs:703 CS8602` warning。Playnite 目标 `net462`，测试 `net472`。隔离输出在本地 `.tmp/build-main-4b7f0a34`。
- R08 动画专测 `2/2`、0 failed/skipped，VSTest exit `0`：[R08-01 TRX](R08-01-CURRENT-MAIN-RECHECK-20260924.trx)。相邻生产壳层与基础动画、设置几何行为 `23/23`、0 failed/skipped、exit `0`：[相邻与设置几何 TRX](SETTINGS-HEADER-CURRENT-MAIN-RECHECK-20260924.trx)。相邻组成：`ProductionShellChromeSourceTests 12`、`UiFinesseFoundationTests 9`、`ReportedWorkspaceLayoutBehaviorTests.SettingsHeaderAndPathActionsStayAnchoredToTheirLabelsAndEachOther 2`。
- Translate 当前值 `9.287` 在改目标瞬间保持连续，随后到达 `-8`；动画时钟清理。生产侧栏收起中点/反向起点均 `184`，反向采样到 `213.333`，最终宽度 `270`、opacity `1`，transition 和 opacity 时钟清理。采样值会随 Dispatcher 帧时序略变；验收检查连续性、最新目标和终态。

两组 TRX 在 WPF shutdown 都记录了 `TextServicesHost.OnUnregisterTextStore` 的 `InvalidComObjectException` 清理输出；根因未知。两个测试运行均以明确通过结果和 exit `0` 完成，清理输出没有隐瞒或按产品失败计数。

## 范围边界

证据由真实生产 WPF 动画实现/壳层、合成状态和隔离 STA Window 构成。未验证正常 Playnite host、物理鼠标/键盘/触控板、物理 DPI/跨屏、UIA/读屏、DWM presented frame、ETW、宿主性能或 OS reduced-motion 设置。没有操作真实存档、媒体、云端或诊断。

**R08-01：当前 main 上已满足可控 WPF 动画反向条件。下一项 R08-02 热关闭动画。**
