# R07-05 当前 main 复核（2026-09-24）

## 结论

R07-05 的受控布局行为在当前 `main` 上仍通过。源码与测试构建身份为 `ec9b0a2107d1fe6d091043d0f99f2e9378e78b7c`；该提交只在前一源码身份 `8e4f3194227afb28640754f12ab0889cb8bb71ce` 之后增加文档与证据。旧实现记录见 [R07-05 初次验收](R07-05-DETAIL-BREAKPOINT-20260918.md)，本文件只记录 main 的复核结果。

本次复核有必要：此前的 `0daca6f0` 之后，详情布局相关的 `ResponsiveLayoutCoordinator`、生产 shell 和 `TaskCenterView` 已继续变化。没有覆盖或回移旧 main 实现。

## 当前构建与行为结果

- `scripts/build.ps1 -Configuration Release -SkipTests -OutputRoot .tmp/build-main-ec9b0a21` 成功；XAML 结构检查 `24/24`，解决方案 `0 error`。保留 `MediaCenterView.xaml.cs:703` 的两条既有 `CS8602` 编译警告。
- 当前身份的首次复用尝试使用了仍标记为 `8e4f3194` 的隔离测试程序集。4 个会读取源码的 `TaskCenterViewResponsiveTests` 用例被 `TestRepositoryContext` 因源码身份不一致拒绝，另外 13 项通过；这不是产品行为失败。按 `ec9b0a21` 重建后重新执行完整过滤器，最终结果为 `17 passed / 0 failed / 0 skipped`，VSTest 退出码 `0`。
- 五个类的精确组成：`ResponsiveLayoutCoordinatorTests 5/5`、`R07DetailsBreakpointBehaviorTests 1/1`、`R07ScrollOwnershipBehaviorTests 2/2`、`TaskCenterViewResponsiveTests 7/7`、`R06DetailsBudgetBehaviorTests 2/2`。TRX：[当前 main 结果](R07-05-CURRENT-MAIN-RECHECK-20260924.trx)。
- 断点测试继续检查 `1000→979→981→971→979→988 DIP` 序列中的稳定选择、详情焦点与垂直滚动位置，以及紧凑态详情仍可见。断点协调器覆盖内容预算带内不抖动、进入紧凑小于 `972 DIP`、回到宽布局达到 `988 DIP`。

## 收尾输出与验收边界

归档 TRX 在 testhost 收尾输出中包含一次 WPF `TextServicesContext` 清理阶段的 `InvalidComObjectException`（`StopTransitoryExtension` / `Uninitialize`）；根因未知。它没有改变 xUnit 的 `17/17` 结果或 VSTest 退出码。

用例使用隔离 STA WPF、合成 DTO/fake 和逻辑 DIP；没有启动正常 Playnite 宿主。断点序列在生产 `TaskCenterView` 行为夹具中驱动，不等价于 Playnite shell 的真实窗口拖动事件链、用户包、物理 DPI/跨屏、触控板/鼠标、UIA/读屏、最终呈现帧、ETW 或宿主性能。没有触碰真实存档、媒体、云端或诊断数据。保留现有游戏选框、滚动条系统、命令与业务状态语义、有限列表和 net462 兼容。

**下一可执行任务：R07-06 状态横幅预算当前 main 复核。** 相关 shell 与 Task 布局源码自旧提交后有变化，先检查现有断言，再复跑本项及相邻空态/详情/滚动回归。
