# CLOSE-WRAP-01：错误态输入测量与实际折行间距（2026-10-07）

本阶段修复共享TextBox错误态36→37 DIP测量、任务预设654 DIP单行残留20 DIP行距，以及共享复合按钮固定图标列间距9 DIP。严格容差、命令/Binding与0.6.73版本保留。联合专项64/64、完整离屏render和source/XAML校验通过；后续R05日志证明隐藏ComboBox/Focus失败的夹具前提，补强可见获焦及紧凑负例4/4。当前Core125/Worker357/source470通过；第三轮全量前36类（含Q14、R02、R05焦点）通过，第37类删除回退测试失败，后77类未执行。CI/包及真实宿主仍未签收。下方按阶段保留失败、干预、修复与各自DLL身份。

## 修复前的当前runner证据

[run37570629705](https://github.com/Nikilua/GameSaveCenter/actions/runs/37570629705)源码为clean `c35d9047659a32ff3ae012a5c56c3a08a2a7edfc`，终态failure。实际下载的诊断artifact包含4 JSON、28 TRX、42 TXT，没有EXE或PS1脚手架。

- [诊断自测上下文](ci-c35-baseline/20261007T041648511Z-300b6c86/summary.json)故意保留受控非零输入，摘要failed是负例；Actions自测步骤本身success。上阶段finally后显式exit0的入口修复已在实际runner验证。
- [SDK自测上下文](ci-c35-baseline/20261007T041650438Z-bf6d6671/summary.json)通过。实际SDK列表同时含8/9/10多个版本，仓库精确选择8.0.423；原latestMajor对照选择10.0.401。缺旧SDK负例原生退出码为-2147450725，本机此前是-2147450735，两者分别保留。
- [完整build上下文](ci-c35-baseline/20261007T041655074Z-9c86120b/summary.json)记录SDK8.0.423、Core125/125、Worker357/357、source470 passed/18原有skip；前23个WPF类通过，第24类Q14仍在错误态高度36→37 DIP失败，其后89类未执行。该上下文唯一非零测试步骤为Q14，runner整体没有转绿。
- [修复前Q14 TRX](ci-c35-baseline/20261007T041655074Z-9c86120b/tests/wpf.Q14ToolbarAlignmentBehaviorTests/wpf.Q14ToolbarAlignmentBehaviorTests.trx)保留方法、断言、堆栈、时间和DLL身份。原654 DIP问题由[上一阶段独立几何探针](../close-sdk-20261007/preset-boundary-diagnostic.json)另行复现，不能说这条提前失败的CI运行已执行后半段。

## 根因与实现

共享`GscWpfUiTextBoxTemplate`原错误触发器把承载原生内容视图的Border从1改为2 DIP，内容测量使36 DIP搜索框增高到37。现用固定2 DIP、不参与命中的覆盖描边，通过Opacity显示错误状态；原生PART_ContentHost、Padding和输入边框的测量保持稳定，错误底色和动态语义色仍保留。禁用态在模板根统一淡化，覆盖描边随之变化。

任务预设原先用页面宽度小于657 DIP猜测折行；654 DIP实际panel636 DIP已能容纳七项，仍被加20 DIP下边距。共享`WrapPanelRowGapController`现在读取Arrange之后的layout slot行顶，实际多行才加入请求间距，单行恢复每项作者margin。slot判定避免把居中的短标签误当第二行；Loaded期间监听布局，支持尺寸、文字、可见性和成员变化，Unloaded移除dispatcher-wide订阅，reload只附加一次。没有递归UpdateLayout或写死新边界值。

## 本地受控验证与身份

编译输入为c35d9047加本阶段明确dirty源码，不冒称尚未提交的clean身份。[初始Release](initial-build/summary.json)、[最终Release](final-build/summary.json)均SDK8.0.423、0 warning/0 error、XAML24/24、testsRequested=false；这些是输入/行距阶段的编译快照；随后因全量R02失败新增共享按钮与对应测试，不能把旧快照当成最终联合候选。[输入哈希对照](quality/source-snapshot-comparison.json)确认产品、测试、脚本与项目编译输入未变。

| 定向测试 | 实际结果和边界 |
| --- | --- |
| [Q14](targeted/tests/Q14ToolbarAlignmentBehaviorTests/Q14ToolbarAlignmentBehaviorTests.trx) | 1/1；60组真实STA几何，Light/Dark654 DIP均单行且七项bottom margin为0；620 DIP两行且间距20，660单行，两个主题620→660→620往返均恢复 |
| [共享行距控制器](targeted/tests/WrapPanelRowGapControllerBehaviorTests/WrapPanelRowGapControllerBehaviorTests.trx) | 2/2；作者非零margin、短标签居中、resize、Collapsed、内容增宽、关闭间距、三次卸载/reload、同数量成员替换及未Loaded的手动Arrange |
| [跨页布局](targeted/tests/ReportedWorkspaceLayoutBehaviorTests/ReportedWorkspaceLayoutBehaviorTests.trx) | 54/54；Save/Media/Task/Maintenance/Trainer布局、边界和双主题往返 |

Q14原0.75 DIP容差不变；错误/焦点/禁用/忙碌状态的几何断言通过，新增断言验证错误覆盖描边为2 DIP、主题错误色、Opacity1、与输入同尺寸且不截获命中，清除错误后Opacity0。COM收尾文本仍保留，原生testhost退出0；本阶段没有据此宣称修复COM或关闭用户Q06/R08原故障。

[定向上下文](targeted/summary.json)与最终build中候选Playnite DLL SHA256为`4EFC7A69193921655A84B40441719DD6AD7484EE56FB0ECAD6BF687871C67106`、MVID `85fd4c8c-2cd2-4014-aef4-836226dff7a5`；测试DLL SHA256为`C21057B68D96763CFF8A3ACBD71BD6FCF7092F272141A3685D61E93CA4EBC095`、MVID `cebb626a-e81f-471e-8005-909d668cf489`。完整列表与源码hash以JSON为准。

## 渲染和质量门禁

完整[render-qa报告](render/render-qa-report.txt)终态`render-qa OK`、无PROBLEM，覆盖各工作区、多尺寸、Light/Dark、列表与滚动探针、resize恢复及生产shell几何。RenderHarness编译0 warning/error。报告明确dirty树、offscreen DpiScale1.00；[harness及实际旁置插件身份](render/assembly-identity.json)单独记录SHA/MVID，插件与正常build快照一致。

已实际检查[Light任务页](render/theme/light/Task-1040x700.png)、[Dark任务页](render/theme/dark/Task-1040x700.png)、[紧凑shell任务页](render/Shell-Tasks-1040x700.png)及[普通任务页](render/Task-1040x700.png)。它们只证明合成数据的离屏logical DIP布局。

[质量上下文](quality/summary.json)保存源码验证、diff check、生产XAML扫描的实际退出码；扫描24个生产XAML为0 errors、27 warnings、162 info，既有提示未隐藏。共享模板变更另外命中R00-01-02/R00-05，[逐项candidate影响](quality/candidate-freshness-impact.json)为4 fresh/10 stale；原baseline未更新，也未提供新的package身份。

## 全量门禁与后续

首次全量source：470 passed、18原有skip、0 failed；WPF前28类（含Q14）通过，第29类R02 2 passed/2 failed，其后85类未执行，脚本exit1。[首次全量上下文](first-full-playnite/summary.json)保留全部已执行类console/TRX与原非零退出码。联合候选后续需重新全量验证。UI完成后按10项范围补CLOSE-EVID-01，当前候选CI编译/测试/渲染/包通过前CLOSE-CI-01、CLOSE-SDK-01保持IN_PROGRESS。

没有启动真实Playnite、改OS动画设置、安装到用户Extensions或操作真实存档/媒体/云端。物理DPI/跨屏、宿主UIA/读屏和最终呈现帧、ENV-001、用户原失败及同候选发布矩阵仍分开验收。

## 新发现CLOSE-OPTICAL-01：共享复合按钮对齐

[旧clean c35 DLL独立对照](r02-baseline/summary.json)同样复现R02两条9 DIP间距失败，排除输入/行距改动引入回归。[实际testhost干预](r02-intervention/summary.json)记录DpiScale1.00、content desired32/36却actual134、固定列16/20变17/21、间距9。只在合成夹具临时把presenter改Center，content恢复32/36、固定列16/20、间距8；随后恢复，原断言仍失败，干预不计通过。

共享Button新增只读HasVisualContent状态，并在内容变更时更新；共享presenter对真实UIElement/FrameworkContentElement绑定按钮HorizontalContentAlignment，文本仍Stretch接受有限宽度。[修复后原R02 4/4](r02-fixed/tests/R02OpticalAlignmentTests/R02OpticalAlignmentTests.trx)记录16/20两项实际gap8、centerDifference0.5、content32/36。临时干预已从测试源码移除，严格8±0.5断言保留；新增Left/Center/Right动态切换和文本-复合内容两次往返在下方联合专项中已通过。

以上R02候选与前面的输入/行距DLL分账，完整列表以各摘要SHA/MVID/源码hash为准。首次render报告也属于输入/行距候选，新增共享按钮修复后须重跑render、source与全部114类，不冒称旧渲染覆盖新模板。
## 最终联合候选专项

[联合Release编译](combined-final-build/summary.json)SDK8.0.423、0 warning/error、XAML24/24、testsRequested=false。[联合专项上下文](combined-targeted/summary.json)同一输入实际64/64：R02 7/7（原4项加Left/Center/Right内容转换3项）、Q14 1/1含60组测量、行距控制器2/2、跨页54/54，全部原生进程exit0。保留各TRX和SHA/MVID，前文输入/行距快照及R02旧DLL/干预失败均不替换。

此专项快照后的render、静态/source与全量结果分别见下节，专项64不能代替全量门禁。
## 联合候选render与静态门禁

最终共享按钮/输入模板的[完整render](combined-render/render-qa-report.txt)exit0、render-qa OK、无PROBLEM，RenderHarness编译0/0；[实际harness与旁置插件SHA/MVID](combined-render/assembly-identity.json)同当前输入快照核对。再次实际检查Light/Dark任务页截图；初次render仍作为输入/行距阶段历史保留，不能混用两套身份。

[联合质量摘要](combined-quality/summary.json)实际source/diff/XAML扫描均exit0；24个生产XAML为0 errors/27原有warnings/162 info，[源码快照对照](combined-quality/source-snapshot-comparison.json)无编译输入变更。[candidate freshness](combined-quality/candidate-freshness-impact.json)仍4 fresh/10 stale，baseline未改。此快照采集时新全量仍在运行，后续终态见下节，完整门禁未签收。

侧栏performance probe记录离屏诊断时间/Rendering回调，包括single-toggle duration8591.2ms、maxFrameGap409.2ms、settled=True；这些原始数值保留，不宣称性能改善、物理屏幕presented frame或真实宿主验收。
## 联合全量终态与R05新失败

[联合全量上下文](combined-full-playnite/summary.json)终态failed：source470 passed/18原有skip、WPF前34类（含Q14与R02）全部通过；第35类R05FocusBoundaryBehaviorTests 2 passed/1 failed，其后79类未执行，原生exit1。失败是Popup内MoveFocus(Next)后IsDropDownOpen仍true（147行），不是COM收尾文本；原断言和堆栈完整保留。

[旧clean c35 DLL的独立R05对照](r05-baseline/summary.json)3/3、原生exit0。此对照不足以排除焦点/Dispatcher时序差异，尚不归因共享按钮、输入或行距；同最终联合DLL独立复测终态仍2/1；后续诊断与前置修正见下节。联合完整门禁/CI/包尚未通过，不将前34类代替全部114类。
## R05夹具前提修正与正负例

[候选独立失败](r05-candidate-recheck/summary.json)终态2 passed/1 failed，exit1。随后[原断言加焦点日志](r05-focus-probe/summary.json)仍失败：260×180窗口中overlay为0×3、筛选行Collapsed、ComboBox不可见且Focus()=false，打开Popup后焦点仍在Window；MoveFocus使焦点进入搜索框，但从未离开已获焦的ComboBox。旧DLL偶然3/3不足以证明该夹具前提成立。

[可见筛选行对照](r05-visible/summary.json)900×640、原关闭/方向容器/overlay边界断言完整保留，3/3、exit0。最终增加必须可见、Focus()成功、Keyboard.Focus返回原ComboBox与IsKeyboardFocusWithin前置断言；另补900×320紧凑选择器，隐藏筛选框不能获焦，前后焦点遍历留在overlay且Escape恢复打开按钮。[最终R05](r05-final/summary.json)4/4、exit0，TRX记录visible=true、Focus()=true、overlay572×422；遍历后open=false。没有新增生产关闭事件、直接关闭Popup、skip或放宽断言。该测试验证程序化MoveFocus焦点遍历，不证明OS物理Tab输入。

[最终编译](r05-final-build/summary.json)SDK8.0.423、Release0 warning/0 error、XAML24/24；生产DLL与此前联合候选相同，测试DLL因新增夹具前置断言重新编译。[当前Core/Worker](final-domain-tests/summary.json)125/125与357/357通过，exit0。新114类WPF全量终态见下节；完整门禁仍未通过。

[最终静态门禁](focus-final-quality/summary.json)source/diff/24个生产XAML扫描均exit0，0 errors/27原有warnings/162 info；与最终R05编译输入逐项hash一致，10项旧证据仍stale，未改baseline。

## 第三轮全量终态与CLOSE-PICKER-01

[第三轮全量](focus-fixed-full-playnite/summary.json)exit1、终态failed；[TRX计数汇总](focus-fixed-full-playnite/test-counts.json)source470 passed/18原有skip，WPF前36类通过，第37类R05OptionVirtualizationBehaviorTests 2 passed/1 failed，合计WPF152 passed/1 failed/0 skip，后77类未执行。Q14、R02 7/7与R05焦点4/4在这次全量入口中均通过。第37类期望删除后的回退名称Game 0000、实际Game 0020，原断言保留。

[旧clean c35独立同类](r05-option-baseline/summary.json)与[同candidate独立复测](r05-option-recheck/summary.json)均3/3、exit0。一次独立变绿不能抹除全量失败：该方法不实例化生产视图，本轮未改GamePickerViewModel；默认名称排序/视图首项回退与20ms防抖刷新路径仍待受控归因。无SynchronizationContext时直接在Task延续线程刷新视图存在并发嫌疑，尚未用确定性负例证明。新增CLOSE-PICKER-01，下一独立阶段定位并补正负例；本阶段UI源码及证据单独提交，相关任务保持IN_PROGRESS，不宣称完整门禁通过。

另外[独立RenderHarness的MSBuild属性查询](render-identity-default.json)实测未注入env时GscBuildCommit=unknown、InformationalVersion=0.6.73+unknown，仅查询、未加载UI。本轮render显式传入c35且实际DLL已核对；独立render-qa脚本的身份注入/恢复另在CLOSE-CI-01续办，不把本轮手动env当成脚本已修复。