# R07-08 当前 main 复核（2026-09-24）

## 结论

R07-08 的窗口化压力序列在当前 `main` 上通过受控验收。生产源码身份 `0c0869a2a95a9e949af5dc257bf7c7b4da2f80b6`；测试夹具 SHA-256：`32662ADFE6CAD0BBB66F4AA49D09848FF3E295A783123D6CA70B01CB6E2FAE16`。生产布局代码自前次验收后有较大更新，本次也修正了旧测试直接调用 `ApplyResponsiveLayout` 的缺口：现在只调整外层 `Window.Width/Height`，等待 Dispatcher 排空，并依赖生产 shell 的真实 WPF `SizeChanged` 路由响应。

## 构建、回归与序列结果

- 当前 main Release solution 构建成功：XAML `24/24`、`0 errors`；保留 `MediaCenterView.xaml.cs:703` 两条既有 `CS8602` 警告。最终 Playnite 测试项目重建 `0 warnings / 0 errors`；Playnite `net462`、测试 `net472`。
- resize 与相邻行为回归 `21 passed / 0 failed / 0 skipped`，VSTest exit `0`：`R07ResizeStressBehaviorTests 1/1`、`ResponsiveLayoutCoordinatorTests 5/5`、`TaskCenterViewResponsiveTests 7/7`、`R07DetailsBreakpointBehaviorTests 1/1`、`R07ScrollOwnershipBehaviorTests 2/2`、`MediaInboxGeometryTests 3/3`、`R07FineScrollBehaviorTests 2/2`。TRX：[当前 main 结果](R07-08-CURRENT-MAIN-RECHECK-20260924.trx)。
- 实际生产 `AcrylicProductionShellView` + `TaskCenterView` 在隔离 STA Window 中，保持已选任务详情、打开的游戏选择器 overlay 和搜索焦点，按 `1366×900 → 960×700 → 960×560 → 1440×900 → 1366×900` 调整外层窗口。

| 窗口尺寸 | Shell 实际尺寸 | 详情 | Task 表格 ActualHeight / MaxHeight | 详情 MaxHeight | 选择器 ActualHeight / MaxHeight | 搜索焦点 |
| --- | --- | --- | --- | --- | --- | --- |
| 1366×900 | 1352.667×886.667 | 可见 | 455.333 / `∞` | `∞` | 122.667 / 774.667 | 是 |
| 960×700 | 946.667×686.667 | 可见 | 180 / `∞` | 160 | 122.667 / 541.333 | 是 |
| 960×560 | 946.667×546.667 | 可见 | 180 / `∞` | 160 | 122.667 / 401.333 | 是 |
| 1440×900 | 1426.667×886.667 | 可见 | 455.333 / `∞` | `∞` | 122.667 / 774.667 | 是 |
| 1366×900 恢复 | 1352.667×886.667 | 可见 | 455.333 / `∞` | `∞` | 122.667 / 774.667 | 是 |

外层窗口到 shell 内容区的宽高差稳定为 `13.333 DIP`。夹具观察到 `shell SizeChanged` 共 `7` 次；每个窄/短样本详情限高切换为 `160 DIP`，宽态恢复正无穷；选择器可用高度随窗口缩小而下降，没有空菜单、空表格或丢焦点。

## 收尾噪声与边界

TRX 收尾记录 6 段 WPF 文本服务 `InvalidComObjectException`：4 段来自 `TextServicesHost.OnUnregisterTextStore`，2 段来自 `TextServicesContext.StopTransitoryExtension`；根因未知。测试结果仍明确为 `21/21`，VSTest exit `0`，没有把清理日志记作通过证据或隐藏它。

这是实际生产 WPF shell/page、合成任务/fake context、隔离 STA Window 和逻辑 DIP 的尺寸事件行为，不是正常 Playnite 宿主、用户安装包、物理鼠标拖动、跨屏 DPI、OS 输入、真实 UIA/读屏、最终呈现帧、ETW 或宿主性能证据。没有触碰真实存档、媒体、云端或诊断数据；没有改生产命令、取消/错误/安全语义、游戏选框、滚动条或虚拟化。

**R07-08：已满足当前 WPF `SizeChanged` 行为验收，正常 Playnite 宿主与物理屏幕仍待验。下一项：R08-01 中途反向连续。**
