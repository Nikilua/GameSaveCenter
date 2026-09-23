# R08-04 当前 main 复核（2026-09-24）

## 结论

R08-04 原生产实现仍满足可控验收条件，当前 main 未改动其生产或测试源。本次以 `09ec3132` 重建并复核，补齐当前主分支的构建与行为证据，账本维持“已满足”。受控测试不代表正常 Playnite 中真实 Worker 长任务、读屏或最终呈现已验证。

## 当前身份与构建

- 完整 Release solution 构建成功：XAML `24/24`、0 errors；仅保留 `MediaCenterView.xaml.cs:703` 两条既有 `CS8602` warnings。Playnite 目标 `net462`、测试目标 `net472`。
- R08-04 的生产文件和行为测试自实现/兼容性提交 `43141399` 后未再修改。当前 checkout/程序集 identity 为 `09ec3132`。
- 当前身份下 `R02BusyStateTests 4/4` 与 `R08BusinessFeedbackBehaviorTests 4/4` 按类串行，共 `8/8`、0 failed/skipped，两次 VSTest exit `0`。两份 TRX 未见 `InvalidComObjectException` 文本。

## 行为证据

- 慢任务保持真实忙碌状态，只有 action 结束后解除；忙态按钮保持宽度、内容和焦点。快速完成负例等待 `150ms` 后，spinner 仍为 `Collapsed`。
- `BusyOperationCoordinator` 的重复请求门禁以及失败、取消恢复行为通过；生产壳层动作继续使用共享忙碌控件。
- 成功、失败、取消 DTO 的状态、显示文字和详情保持一致。实际 `FeedbackToast` AutomationPeer 暴露最终状态文本及 Custom 控件语义。
- 本批只复核现有实现，未改变命令、绑定、取消/错误处理、恢复保护或业务状态。

测试使用合成 DTO、fake/隔离 STA WPF Window、Dispatcher 和逻辑 DIP；未启动真实 Playnite host 或 Worker 长请求。没有验证 Narrator/真实 UIA、物理 DPI/跨屏、呈现帧、ETW 或宿主性能，也没有访问真实存档、媒体、云端或诊断。Demo 原始目录不可用，沿用恢复生产基线。

## 原始结果

- [R02 busy state 4/4](R08-04-R02-BUSY-CURRENT-MAIN-20260924.trx)
- [Business feedback 4/4](R08-04-BUSINESS-FEEDBACK-CURRENT-MAIN-20260924.trx)

下一可执行任务：R08-05 页面切换轻量化。
