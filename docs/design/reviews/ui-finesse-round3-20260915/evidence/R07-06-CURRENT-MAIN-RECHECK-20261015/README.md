# R07-06 当前 main 状态横幅复核（2026-10-15）

源码身份：`85609395809bfd7f8312362b26c96ba517aefed6`（main）。需要复核的原因是上次 `2e591b9f` 后 `TaskCenterView.xaml` 与响应式测试发生变化，且 `TaskCenter` DataGrid 共享样式本轮接入横向滚轮行为。本次没有修改生产页面、服务、DTO 或测试源码。

## 构建与行为结果

- 本机 SDK：`9.0.302`；VSTest：`17.14.1`；测试宿主显示 .NET Framework `4.8.9181.0`。仓库 `global.json` 固定 SDK `8.0.423`，本机未安装；本结果不替代 SDK8/SDK10 CI。
- 使用隔离输出根目录还原并 Release 构建 Playnite 测试项目及引用项目：`0 warning / 0 error`。Playnite 目标 `net462`，测试目标 `net472`。构建时显式注入当前 `GSC_BUILD_COMMIT` 与 `GSC_SOURCE_ROOT`，保证源码结构断言绑定到本仓库 HEAD。
- `scripts/check-xaml.ps1` 结构检查 `24/24` 通过。
- 横幅与相邻回归合计 `18/18`，`0 failed / 0 skipped`：`R07StatusBannerBudgetBehaviorTests 4/4`、`R06EmptyStateBehaviorTests 2/2`、`TaskCenterViewResponsiveTests 8/8`、`R06DetailsBudgetBehaviorTests 2/2`、`R07ScrollOwnershipBehaviorTests 2/2`。五份脱敏 TRX 位于 [`tests/`](tests/)；测试与插件程序集 SHA-256 分别为 `3D03E617CB10550728ECB490E6C3336266DE3440810D34FAECCBA5DC46278DA8`、`214B0F0C65B90696854CBEDC2AD05B567798B3C3C6D5DBBBDB49FFCF7EC8298B`。
- 脱敏 TRX SHA-256：R07 横幅 `19933E0200CE6C58E30356D3CC5A4F5FB8B5D8A663D2D2447788D853DF5185DA`；R06 空态 `DAE105B65D07E5385AFB7DF6BF76AEF547C5DA6F8AB283539AC79124851E6463`；Task 响应式 `4BC7544F86D23DF176755637410349B8721E82CF648993CDA78E83932ECE2F26`；R06 详情 `294CFDCFCFD50F8A016B17EC01C0ACE001C30AF50A000A63D279BBA1F631F480`；R07 滚动 `9CAEF09FE61C63EC48FDCDDE360404E697739069D3CF1BC3062D57A7991B7F1F`。
- 关键源 SHA-256：`R07StatusBannerBudgetBehaviorTests.cs` `15D748E6D807FD577B9AD5E38AFB9ACA053BA98FCA59C8530F10D8CD71C67B6D`；`R06EmptyStateBehaviorTests.cs` `5BE2033173409EFFAFE9C2E7264A6BC66E486CD23E87CC295C57A0059170EE04`；`TaskCenterViewResponsiveTests.cs` `F1A481EA5264CCF82A91A58A3A3F528D076C70296D4841D37478F1501F1D12CB`；`TaskCenterView.xaml` `00EBAC27AB45A858BBF9107DE60C64C28DA8F95C70CB3FB8425A6ACE5D65C0E2`；`WpfUiProduction.xaml` `FD212D59A0244DF8CABF33A3926FB5CE80CF930E4A0B587692D2DC9D3B15EC6E`；`Redesign.xaml` `3E311AEAE281E20D004BA395A5EED08E1C813FFB44994087645356A91865FE98`。
- 受控行为实际加载生产 `TaskCenterView`、`SaveCenterView` 与 `MaintenanceView`，使用合成 DTO、fake 命令、隔离 STA WPF Window。失败且保留任务时横幅与表格 viewport 均可见；空任务失败走错误 presenter；Save 历史与 Maintenance 审计 stale 表格保留表头和最小高度；横幅只含对应重试动作，安全模式只提供明确恢复动作。测试通过 `IInvokeProvider` 激活真实 WPF Button AutomationPeer，再用 fake 命令计数确认各动作恰好执行一次。详情、空态、状态转换和滚动相邻回归也在当前源码上通过。
- 首次手工构建遗漏 `GSC_BUILD_COMMIT`，触发 `R06EmptyStateBehaviorTests.SaveSourceUsesStateAwareRetryAndDoesNotUseBusyAsAnEmptySignal` 的仓库身份保护断言；按 `scripts/build.ps1` 注入当前提交/源码根后完整重建，最终记录的五类测试均通过。没有据此改测试或生产代码。

## 未验边界

测试覆盖合成数据与隔离 WPF 行为，不代表真实 Playnite UIA 客户端、读屏、宿主焦点、鼠标/键盘输入、物理 DPI/跨屏或最终呈现帧。未做 ETW 或宿主性能验收；没有访问真实存档、媒体、云端或诊断数据。`R07StatusBannerBudgetBehaviorTests` 进程结束时仍输出 WPF `TextServicesHost` 的 `InvalidComObjectException` 清理消息，但 VSTest exit 为 `0`、测试结果为 `4/4`；根因未知，未将其当作测试通过依据或失败原因。

本次不是完整 solution build。SDK8.0.423 / SDK10.0.401 的 GitHub Actions 仍单独追踪；提交 `aa49a569` 的 #1010 已失败，但匿名日志 API 返回 `403`，公开 job 注解只给 `Process completed with exit code 1`，所以未据此推断 R07-04 的 CI 根因。文档提交 `85609395` 的 #1011 在本次记录时仍为 `In progress`。

下一项：R07-07 当前 main 触控板小增量复核；先确认既有滚动能力与源新鲜度，不重建功能。真实触控板/宿主输入仍待验。
