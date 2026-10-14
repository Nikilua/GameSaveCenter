# Q24-04 失败任务业务状态键盘路径补证

日期：2026-10-15。该证据补充默认窗口键盘轨迹，验证 Task Center 在选中失败任务且展开技术详情时的当前控件顺序。结果属于隔离 WPF STA 行为，不代表真实 Playnite 宿主或物理键盘走查。

## 本机身份与结果

- 基线提交：`5838bb51384fe1643fc5d154f1cdf080c9bc04f4`。新增测试源 `tests/GameSaveCenter.Playnite.Tests/R21KeyboardNavigationTraceTests.cs` SHA-256：`13EA3A03BA4CCF4D26049916F78ECCCB0625D4C79E82FFDF4EB8D4E62AD9885A`。该源在上述提交之后有本阶段未提交改动；程序集 ProductVersion 保留了构建时基线身份，不能将本轮称为干净提交构建。
- Windows 本机使用隔离 .NET SDK `9.0.302`、VSTest `17.14.1`、WPF testhost `.NET Framework 4.8.9181.0`；插件目标 `net462`，测试目标 `net472`。Release 测试项目及引用项目定向构建成功，不是完整 solution build。SDK9 本机结果不替代仓库 SDK8 或 CI SDK `10.0.401`。
- `R21KeyboardNavigationTraceTests` `3/3`、`R06DetailsBudgetBehaviorTests` `2/2`、`R23ProductionResourceStateBehaviorTests` `9/9`，共 `14/14`，0 失败、0 跳过。TRX 已去除本机用户名及绝对路径：
  - [`R21KeyboardNavigationTraceTests`](tests/Q24-04-R21Keyboard-5838bb51.trx)，SHA-256 `03A3885B1E5FE7AE2CF5AC60CB58DAB9884CB1AAAC4CCBE764F3E582DA3914EF`
  - [`R06DetailsBudgetBehaviorTests`](tests/Q24-04-R06Details-5838bb51.trx)，SHA-256 `4E52DDE6530BEF280BE3737F83047DA7E0A7C34E791E81169E63FC61F1EAD9A1`
  - [`R23ProductionResourceStateBehaviorTests`](tests/Q24-04-R23ProductionState-5838bb51.trx)，SHA-256 `87884FEF0FA5338F05ABCAB5B9EF65A2DCD0BCCF5C209F72DEFC4375ED43530B`
- 测试程序集 `GameSaveCenter.Playnite.Tests.dll` ProductVersion `0.6.73+5838bb51384fe1643fc5d154f1cdf080c9bc04f4`、SHA-256 `EC74B584065D41225EBD7D9739184D4F1B0972D214E3C0912B502823C4E56815`；PDB SHA-256 `65F16B7584976F1F55477DF804CE1477AD07A502EC5DCC125178C6811F6CFBC5`。生产插件程序集 SHA-256 `0DC7BEE84580ED8D9C4A13B67F7DFF0742C1FB41F5F064B7028E7B084320748E`，ProductVersion 与测试程序集相同。

## 行为范围

- 使用生产 `TaskCenterView`、真实筛选器/表格/详情/页尾控件及合成的成功、运行中、失败 `TaskStatusDto`；失败任务被选中，表格显示 3 行并展开技术详情。
- 实际检查筛选、表格行/单元格焦点、错误详情及页尾操作的 Tab 与 Shift+Tab 次序；失败任务显示错误详情，Retry 可用且 Cancel 不可用。
- 保留重试的真实 XAML 命令绑定，但测试 fixture 的命令是 no-op，未执行重试、取消、复制或其他业务操作。没有接触用户任务、真实存档、剪贴板或 Playnite 服务。
- 新断言补充了此前只覆盖默认状态的焦点轨迹；只检查控件存在或命令绑定不足以证明业务状态下的可达性和反向次序。

## 未验证边界与后续

- 本轮只改 WPF 行为测试；未改生产 XAML、ViewModel、服务或 DTO，因此没有截图、RenderHarness、真实视觉结果或 Playnite 呈现声明。
- 未验证真实 Playnite、操作系统级按键注入、UI Automation/Narrator、IME、物理 DPI/跨屏、最终呈现帧、ETW 或宿主性能；完整页面阅读顺序仍需人工走查。
- `R00-08` 登记了该键盘测试源路径，代码提交后必须按实际提交号更新 baseline 并运行 freshness 校验。当前证据包先保留这一待办，不把旧 baseline 误报为 fresh。
- 下一依赖项按现有账本为 Q24-06 UI Automation；Q24-05 已有当前 main 焦点语义行为证据，须复用并核实，不重建同一功能。
