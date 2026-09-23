# Q06-07 高频连续操作复核

日期：2026-09-23（Asia/Shanghai）

代码基线：`main`，`caa6099455f6a1f9e2b2cdbbe2780ccd040b8e7f`。本项先检查并复用现有忙态协调器、RelayCommand 与动效逆转逻辑，没有改生产控件、服务或 DTO。

## 已有能力与行为检查

- `R02BusyStateTests.BusyCoordinatorRejectsDuplicateAndRestoresAfterFailureAndCancellation` 已覆盖运行中重复提交被拒绝、失败/取消后复位，状态序列为 `true,false,true,false`。
- `R13CloudTransferStageBehaviorTests.ManualRetryCommandGateBlocksSecondClickWhileSubmissionIsBusy` 已覆盖云端手动重试的 `CanExecute` 忙态门禁和重复提交负例。
- `R08MotionReverseBehaviorTests.TranslateReversalStartsAtRenderedValueAndFinishesAtLatestTarget` 检查反向动画从已渲染值接续并到达最新目标，且完成后移除动画时钟。
- `R08MotionReverseBehaviorTests.SidebarRapidReversalUsesLatestTargetAndReleasesOldClock` 在隔离 WPF Window 中快速折叠/反向展开侧栏，检查中间值、最新宽度 `270 DIP`、最终 opacity `1`、transition 停止及动画时钟释放。
- 新增 `tests/GameSaveCenter.Playnite.Tests/Q06ContinuousButtonBehaviorTests.cs`：在生产 `WorkspaceStatePresenter`/`GscWpfUiActionButton` 上用 fake `RelayCommand` 做快速点击派发序列。第一次派发进入 busy、按钮禁用；第二次派发不重复提交；同一页面的兄弟安全操作仍启用；busy 清除后按钮恢复，下一次派发可执行。

## 验证结果

- Release Playnite `net462`、测试 `net472`；构建无错误，编译保留既有 `MediaCenterView.xaml.cs:703 CS8602` warning。
- 隔离 VSTest：`Q06ContinuousButtonBehaviorTests 1/1`、`R02BusyStateTests 4/4`、`R08MotionReverseBehaviorTests 2/2`、云端重试门禁方法 `1/1`，共 `8/8` 通过。
- `R08MotionReverseBehaviorTests` 结束时另打印 WPF `TextServicesHost.OnUnregisterTextStore` `InvalidComObjectException` 清理堆栈；TRX `2/2` 且进程 exit `0`，根因未知。
- 源码测试需带当前构建身份：`-p:GscBuildCommit=caa6099455f6a1f9e2b2cdbbe2780ccd040b8e7f`。一次 `--no-build` 重跑因旧测试程序集没有 `GscBuildCommit` 元数据而失败；按当前身份重新构建后 `R02BusyStateTests` 全部 `4/4` 通过。

## 证据边界

- 按钮快速提交通过 `ButtonBase.OnClick` 框架命令派发阶段调用，不是物理鼠标连点或 Playnite 中真实输入。
- 侧栏逆转是合成 `Click` 路由事件驱动的 STA WPF Window/Dispatcher 行为，证明最新目标与时钟收尾；不是屏幕录制、真实刷新率或 DWM 呈现帧。
- 因此本项的受控命令门禁、动画逆转与终态行为有证据；物理鼠标高频输入、视觉反馈像素及真实 Playnite 宿主仍待环境验证，最终结论保持“未完成”。

下一可执行任务：Q06-08 状态序列录证；先复用当前共享模板和状态夹具，核查 normal/hover/pressed/focus/disabled 的真实输入覆盖边界。
