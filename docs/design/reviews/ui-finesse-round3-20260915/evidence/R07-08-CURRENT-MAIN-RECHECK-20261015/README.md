# R07-08 当前 main resize 压力复核（2026-10-15）

当前源码身份：`d190f7eac4d85e81193080a2ff4f73105cec9b45`（main）。先复核了此前 `552b8168` 测试基线到当前 main 的源差异：TaskCenter 页与 R07Resize 行为测试源未变，但共享 `WpfUiProduction.xaml`、`Redesign.xaml` 和 DataGrid 输入接线有更新，因此用当前 main 重新构建并执行，而不是只复用旧测试程序集。没有改生产或测试源码。

## 构建与行为

- 本机锁定 SDK `8.0.423`、VSTest `17.11.1`、WPF testhost `.NET Framework 4.8.9181.0`；Playnite `net462`、测试 `net472`。隔离 restore 和 Playnite 测试项目/引用的 Release build 为 `0 warning / 0 error`；这不是完整 solution build。
- `scripts/check-xaml.ps1` `24/24`；`R07ResizeStressBehaviorTests 1/1` 与 Task 响应式相邻类 `TaskCenterViewResponsiveTests 8/8`，合计 `9/9`，0 failed / 0 skipped。脱敏 TRX 与哈希在 [`tests/`](tests/) 中。
- 生产 shell、TaskCenter、DataGrid、GamePicker overlay 在隔离 STA WPF 窗口中载入；测试把合成失败任务选入实际 DataGrid，通过紧凑详情按钮的 routed Click 打开详情，再运行 `wide → narrow → short → wide-again → restored` 五阶段。
- 五阶段的 Window 尺寸分别为 `1366×900`、`960×700`、`960×560`、`1440×900`、`1366×900 DIP`。PageHost 实测分别为 `1038.667×759.333`、`632.667×559.333`、`632.667×419.333`、`1112.667×759.333`、`1038.667×759.333`。恢复阶段与 wide 基线一致，PageHost 宽高偏差不超过 `1 DIP`。
- 五阶段任务详情与 GamePicker 始终可见且尺寸非零，搜索框焦点始终保留，任务表始终有高度，未留下有限 TaskGrid MaxHeight。窄/短阶段详情高度为 `136 DIP`、MaxHeight=`136`；宽/恢复阶段详情为 `360×476 DIP`、MaxHeight=`∞`。Picker 高度预算随窗口变窄或变短而收紧，shell 实收 `11` 次 SizeChanged。
- 测试每次 `ApplySize` 后经过 WPF Dispatcher `ApplicationIdle` 并重新 `UpdateLayout`，再采样几何；它验证稳定布局后的状态和恢复尺寸，不测真实鼠标拖动帧时间。测试进程退出时出现多条 WPF TextServices `InvalidComObjectException` 清理消息，但 VSTest exit `0`、所有用例通过；根因未知，不将 teardown 噪声当作测试结果。

## 程序集与源身份

- 测试 DLL SHA-256：`59F353700EB8E906920725709300F124601E8A8B7746889E7C4CDACE89414E60`；PDB：`67C1471D17A3B889D920AA8743984E2B7E7389A0F0248E3AE252C6C60FEEC19B`。
- Playnite DLL SHA-256：`BFCF165BF6C924E998FA442A783FBAF2D00A11A4CE2429CB7621FB54D3BED691`；PDB：`786752A90BD3C80952E4BB113976B230C5000895E845355A8A5C088844220C9A`。
- `R07ResizeStressBehaviorTests.cs` SHA-256 `DA8B64BB92F2834F942995733E4F5AF28880F09C6799C81FCD99DFBE1DA1C7D4`；`TaskCenterViewResponsiveTests.cs` `F1A481EA5264CCF82A91A58A3A3F528D076C70296D4841D37478F1501F1D12CB`；`TaskCenterView.xaml` `00EBAC27AB45A858BBF9107DE60C64C28DA8F95C70CB3FB8425A6ACE5D65C0E2`；`TaskCenterView.xaml.cs` `05B078985AEDC56D984CAD38D80EF7708A653A28888C251C7F2B9ED3FC79F66D`；`AcrylicProductionShellView.xaml.cs` `736C1554D06C8B67E753F68FBFB0E2F63935F007414BA9F45BB1E29D35AE39C7`；共享 `WpfUiProduction.xaml` `FD212D59A0244DF8CABF33A3926FB5CE80CF930E4A0B587692D2DC9D3B15EC6E`。

## CI 与边界

R07-08 的受控 resize 条件在当前 main 本机 SDK8 上复核通过。真实 Playnite 窗口、实际鼠标拖动节奏、设备 DPI/跨屏、UIA/读屏、最终呈现帧和宿主性能仍未验证。d190f7ea 已推送；当前会话无法读取 GitHub Actions：REST 请求被本机代理拒绝、公开页面 cache miss、浏览器标签读取超时，因此该提交对应的 run ID 和终态未确认。此前 #1012/run `38640644812` 对父提交 `2b423737` 的最后观察为 `In progress`，不能据此推断新提交 CI 结果；SDK8 本机通过也不替代 CI SDK `10.0.401`。

**R07-08：当前受控验收已复核；CI 终态及真实宿主/拖动性能仍待验。下一项：R10-02 当前 main 复核，外机 R09 的 DLL/PDB 身份仍待用户可回复时补充。**
