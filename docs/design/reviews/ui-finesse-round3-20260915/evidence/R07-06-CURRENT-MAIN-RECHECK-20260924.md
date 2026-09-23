# R07-06 当前 main 复核（2026-09-24）

## 结论

R07-06 的状态与表格预算行为在当前 `main` 上通过受控验收。生产源码 commit `2e591b9f6217c1dcf7ac5d0494a55a7e6e230c82`；本批没有改生产页面、服务或 DTO，只加强了行为测试。当前测试源码 SHA-256：`15D748E6D807FD577B9AD5E38AFB9ACA053BA98FCA59C8530F10D8CD71C67B6D`。

需要重验是因为 `ResponsiveLayoutCoordinator`、production shell 与 `TaskCenterView` 自 R07-06 初次验收后已有布局改动；不能把 `3551de81` 的旧结果直接当作 main 当前结果。

## 构建与行为结果

- 当前 main Release 隔离 solution 构建成功：XAML `24/24`、`0 errors`；保留 `MediaCenterView.xaml.cs:703` 两条既有 `CS8602` 警告。修改后的 Playnite 测试程序集重建无警告、无错误；Playnite 目标 `net462`、测试 `net472`。
- 五类测试共 `17 passed / 0 failed / 0 skipped`，VSTest exit `0`：`R07StatusBannerBudgetBehaviorTests 4/4`、`R06EmptyStateBehaviorTests 2/2`、`TaskCenterViewResponsiveTests 7/7`、`R06DetailsBudgetBehaviorTests 2/2`、`R07ScrollOwnershipBehaviorTests 2/2`。TRX：[当前 main 结果](R07-06-CURRENT-MAIN-RECHECK-20260924.trx)。
- 保留旧任务行时，失败横幅与刷新中横幅均维持非零表格 viewport；模拟读取恢复后，横幅按绑定状态收起。无旧行的读取失败不显示 Stale 横幅，错误 `WorkspaceStatePresenter` 仍可见，重试入口仍可用。
- Save 历史与 Maintenance 审计 Stale 表格各保留最小高度/非零 viewport。三个 Stale 横幅都只有一个按钮且含义为“重试”，没有泛化关闭按钮；安全模式告警只提供明确的“恢复正常模式”动作。
- 旧断言只核对重试命令引用，没有触发命令。本批改用隔离 WPF `IInvokeProvider.Invoke` 激活实际实例化的按钮，并用计数型 fake 命令确认 Task 失败/空失败、Save Stale、Maintenance Stale 与安全模式恢复各自只执行一次；没有调用真实 Worker 或改变真实安全模式。Task 夹具补齐加载/失败属性的 `PropertyChanged` 通知，确保状态转换断言确实经过绑定更新。

## 边界

使用合成 DTO/fake、实际生产页面控件、隔离 STA WPF Window 和逻辑 DIP。`IInvokeProvider` 是受控 WPF AutomationPeer 行为，不等同真实 Playnite UIA 客户端、读屏、宿主焦点或物理输入。没有执行 Playnite 宿主窗口尺寸拖动，也没有做浅/深主题截图矩阵、物理 DPI/跨屏、presented frame、ETW 或宿主性能验收。没有接触真实存档、媒体、云端或诊断数据；游戏选框、滚动条系统、命令/取消/错误/安全语义、有限列表与 net462 兼容均未改。

TRX 收尾记录 9 段 WPF `TextServicesHost.OnUnregisterTextStore` / `TextStore.OnDetach` `InvalidComObjectException` 清理输出；根因未知。xUnit/VSTest 明确为 `17/17` 且 exit `0`，没有将清理日志计为测试失败，也没有隐藏该现象。

**R07-06：已满足受控验收，真实宿主环境待验。下一可执行任务：R07-07 触控板小增量当前 main 复核。**
