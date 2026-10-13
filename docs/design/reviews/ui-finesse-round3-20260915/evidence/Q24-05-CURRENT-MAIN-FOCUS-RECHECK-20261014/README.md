# Q24-05 当前 main 焦点语义复核

日期：2026-10-14。按 Q24-05 条件复核 Focus、Selected、Current 三类状态是否分开，以及关闭游戏选择层后的焦点是否回到可见有效控件。

## 结论与已有实现

Q24-05 在当前 main 的受控 WPF 行为范围内已满足。本轮复用已有生产模板、GamePicker 和测试，没有改生产代码或重建焦点状态服务。相关行为实现与回归测试最初由提交 b20f1a4b 补充 R21-07，导航资源状态由 a18cb43e、表格状态由 e277a804 等提交建立；本轮在后续 main 精确身份上复核。

- 生产导航的 IsChecked 当前页状态在键盘焦点移到外部输入框后仍保留；焦点轮廓随键盘焦点移动，在 Light/Dark 两主题分别断言颜色、边框粗细与状态。
- Settings 页签和生产 Task、Media Inbox、Save、Maintenance 表格以真实 WPF 控件验证选中状态与键盘焦点分离；表格失焦选中行不增加焦点边框，单元格获焦后出现焦点轮廓，禁用目标不能再获焦。状态测试覆盖 Light/Dark。
- 生产游戏选择层以 2,000 条合成条目验证搜索框焦点在滚动到末项及浅/深主题切换后仍可定位；通过生产触发按钮关闭选择层后，焦点回到仍可见、启用的按钮。
- 负例覆盖导航失焦、导航/页签/表格禁用、错误文本框修复后状态恢复及焦点环不替换选中/错误 chrome。

## 本机复核

- 分支 main，源代码身份 cb31947b1148195e2b8750dbfc75ae0269b1978a；隔离 .NET SDK 8.0.423。Playnite 插件目标 net462，测试目标 net472；VSTest 17.11.1，测试宿主 .NET Framework 4.8.9181.0。
- Release 测试项目及依赖构建成功：0 warnings / 0 errors。随后使用精确身份程序集分组运行 7 项 WPF 行为：1 + 2 + 4 = 7 passed / 0 failed / 0 skipped。
- 测试程序集 SHA-256 0CB62473D6657551EEE987111CF759CE981D84C51AD200DFABCD6988C8DAD1B1，MVID 1063a07b-2f81-4634-a884-a19cefb85c17，ProductVersion 0.6.73+cb31947b1148195e2b8750dbfc75ae0269b1978a。
- 插件程序集 SHA-256 BBA3DB5F45C00006C8DF03A238472171E2FFED4731488972E3AAD3BD9A2F2FB5，MVID 5f52e495-7f2e-4799-8ae6-29026fee86c6，ProductVersion 同为 0.6.73+cb31947b1148195e2b8750dbfc75ae0269b1978a。

| 测试组 | 结果 | TRX SHA-256 |
| --- | ---: | --- |
| R21FocusVisualRegressionBehaviorTests | 1/1 | F07855D0D800C2AFB4B03D4C4BB863927BCC809A51392BD68E868BE27265BE98 |
| R09FocusOutlineBehaviorTests | 2/2 | 5F0A1A9D83038C38370CA5BA5A8D0E76AF43DC157B54A2D71E2285F569B1F8DC |
| R23 导航、设置页签与四类生产表格状态 | 4/4 | 0F554BEF3161DFDA63C46B582E3C36C23B9D4F30EAACE92767E62F6A31DA5B65 |

TRX：[选框回焦与滚动](tests/Q24-05-picker-focus.trx)、[焦点环与错误恢复](tests/Q24-05-focus-outline.trx)、[当前/选中/禁用状态及负例](tests/Q24-05-current-selected-negative.trx)。测试源 SHA-256：R21FocusVisualRegressionBehaviorTests.cs 25BC1B18C44A6EE51B3F7FE493640C3F36C6641F4BA5D7F668B4EE66104FC316；R09FocusOutlineBehaviorTests.cs A5AAB9E90C340F93301D8424FF1E30D98B160D39C1265172AEE7A6B01B2CBC98；R23ProductionResourceStateBehaviorTests.cs 663FF65E6EDF569D05D51D002C45C74D4D125AA2F281617F02BD8081AA84B3FC。

共享资源源 SHA-256：AcrylicProductionResources.xaml 943BD53C17042E10382F35731784C0942CCE3896E6E35593E08E6B04F6550870；WpfUiProduction.xaml 6D6EE5777D0B50D0DEC21EDF4DB5EE7F1632532992D24B8E2B8F108037181211。

第一组测试通过后，WPF 进程关闭时控制台出现 TextServicesContext/TextStore 的 InvalidComObjectException 清理噪声；dotnet test 退出码为 0，TRX 为通过，原因未确认。其余组均通过。该异常没有被解释成产品焦点故障。

## 共同门禁与未验边界

此项没有生产样式或布局更改，因此没有新增截图或 RenderHarness 声明；实际 WPF 控件状态、主题资源和几何断言承担本轮适用的行为验证。未运行真实 Playnite，也未注入 OS 级键盘输入或真实 Windows UI Automation/读屏；未验证焦点环最终像素、物理 DPI/跨屏、最终呈现帧或宿主性能。这些外部验证仍待完成，不改变当前 main 受控行为已满足的结论。

推送后的 CI 与本机 SDK 分开记录：旧 #973/run 38429051152 页面仍显示 In progress；#974/run 38436999880 的 SDK8 与 SDK10 job 页面仍处于 Re-running jobs，日志要求登录，实际运行 SDK 及终态没有取证。提交新证据后仍须继续跟踪 Actions 到明确终态。

用户 2026-10-14 提供的 #970/#971 三个 ZIP 均与已审包 SHA 相同：#970 SDK10、#970 SDK8、#971 SDK8；没有新的 #971 SDK10 日志。缺少这份日志不阻塞独立任务。

下一可执行项 Q24-06：在当前 main 复核现有 UIA Name、HelpText、LabeledBy 与原生 Pattern 行为；真实 Windows Narrator/宿主验收单列待验。Q24-03 物理跨屏仍需第二台物理显示器。
