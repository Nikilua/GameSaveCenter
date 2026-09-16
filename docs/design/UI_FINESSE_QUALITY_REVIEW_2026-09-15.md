# UI 精修第二轮独立质量复查

日期：2026-09-15。结论：**有明显、可验证的进展，但“已有实现/自动通过”距离专业应用的完整验收仍有差距。应先修门禁与交互盲区，再继续增加功能。** 本轮不合并或覆盖 Luna 的开发分支，不修改生产功能。

审查主基线为 `35b82761c60b8ec8a3fedc283545e670bdf05127`（`codex/ui-finesse-round2`）。先审阅并构建了 `6bae7c1`，发现分支继续推进后更新独立副本到 `35b8276`，重新完整构建/测试并运行双主题夹具。主工作区 main 为 `f501476`；下述源码位置属于被审分支，不能把 main 的旧代码视为当前实现。

审查期间又出现 `4414f05` / `3ca74c4`，其动画清理改动另列后续状态；不反复追逐移动 HEAD，也不把这些未经本轮全量构建的后续提交冒充 `35b8276` 的独立测试结果。

## 1. 哪些改进值得保留

- 深色夹具的正文、数字、按钮和状态标签比第一轮清楚；共享按钮显式前景、危险动作独立文字颜色、Caption 透明度处理和语义字号替换都有实质代码，不只是改账本。
- 对比度检查加入实际文字样本和故意失败的黑字负例；字体诊断明确区分候选覆盖和未知的最终 GlyphRun，比先前“字体存在就算通过”严谨。独立探针用不存在的字体查 A 返回 False，没有支持“任意缺失字体都会被误报可用”的猜测。
- 搜索减少过滤热路径分配，有实测样本；列表批量替换、稳定对象身份等已有底座继续发挥作用。仍需补连续输入与真实 UI 延迟，不能只看一次 VM 更新。
- `c76ce62` 已补入场动画中途接管，`8dfe7fa` 已把多处时长解析接到真实资源宿主。相关源码字符串测试只能证明这些结构存在，运行时终态仍需补验。
- 已有真实 Playnite 嵌入取证进展。本轮只读查看了 `4f1dbb4` 的原始 summary 和概览 PNG：`EmbeddedDashboardCaptured=true`、`EmbeddedSettingsCaptured=true`、`ControlledDashboardCaptured=false`，深色非空隔离库、150% DPI 的记录有据可查。不能再笼统说“从未捕获真实 Dashboard”。这份历史图不证明最新共享资源、浅色、IME、读屏或真实呈现帧已通过。
- 后端已有恢复保护、校验、远端 staging、保留预览、云重试、诊断包、媒体归类等能力；新增任务应完善这些能力的可理解性和边界，避免重建同名服务。

## 2. 独立验证与证据

| 项目 | 本轮结果 | 边界 |
| --- | --- | --- |
| Release 构建，固定 35b8276 | 0 warning / 0 error | 独立 detached worktree，输出位于该副本内 |
| Core | 83/83，通过 | 自动测试 |
| Worker | 310 通过 / 1 跳过 / 311 总计 | 跳过不计通过 |
| Playnite | 485 通过 / 63 跳过 / 548 总计 | 不是完整真机交互矩阵 |
| RenderHarness 构建 | 0 warning / 0 error | 独立构建 |
| finesseprobe 浅/深 | 两份 `finesse-fixture OK` | 仍有下述未覆盖的问题；不是“视觉无缺陷” |
| 独立 WPF 反射探针 | 重现缩放嵌套、共用可变变换串扰、清钟基值和合成误差 | 直接调用构建程序集；不是物理屏幕性能 |
| 历史宿主原图复查 | 4f1dbb4 概览与 summary 一致 | 只读查看 Luna 已采集结果，本轮未重新安装/操作用户 Playnite |

可复核证据：

- [构建与全量测试](reviews/ui-finesse-round3-20260915/evidence/release-build-tests.txt)、[Harness 构建](reviews/ui-finesse-round3-20260915/evidence/harness-build.txt)、[源码校验](reviews/ui-finesse-round3-20260915/evidence/source-validation.txt)。
- [深色原图](reviews/ui-finesse-round3-20260915/evidence/dark-ui-finesse-fixture.png)、[深色报告](reviews/ui-finesse-round3-20260915/evidence/dark-ui-finesse-fixture-report.txt)；[浅色原图](reviews/ui-finesse-round3-20260915/evidence/light-ui-finesse-fixture.png)、[浅色报告](reviews/ui-finesse-round3-20260915/evidence/light-ui-finesse-fixture-report.txt)。
- [独立探针结果](reviews/ui-finesse-round3-20260915/evidence/independent-wpf-probes.txt)与[可复跑脚本](reviews/ui-finesse-round3-20260915/evidence/run-wpf-probes.ps1)。命令为 `powershell -NoProfile -STA -File <脚本> -PluginAssemblyPath <被审构建的 GameSaveCenter.Playnite.dll>`；从同一输出目录加载依赖。先核对构建身份；脚本用于测量，不代表所有输出都应为 True。
- [历史宿主概览](reviews/ui-finesse-round3-20260915/evidence/historical-host-4f1dbb4-overview.png)、[对应原始 summary](reviews/ui-finesse-round3-20260915/evidence/historical-host-4f1dbb4-summary.json)。原结果保留 `HighGateCount=1`，不能删除这个边界再签收。

本轮还暴露了测试运行根目录问题：第一次将 6bae7c1 的隔离输出放在主仓库 `.tmp/r3b`，部分测试从 `AppContext.BaseDirectory` 向上找到 main 的源码，因此 Playnite 出现 10 失败 / 458 通过。改为副本内输出后，同一 6bae7c1 为 468 通过、0 失败、63 跳过；更新到 35b8276 后为上表结果。**这 10 项是测试错根造成的无效比较，不能报告成产品回归，也不能忽略这种运行器缺陷。** 已加入 R01-01。

## 3. 第二轮账本实际含义

固定 `35b8276` 的 208 行：实现列为 124“代码完成”+84“已复核”；自动列 207“通过”+1“待验”；视觉列 146“通过”+59“待验”+3“不适用”；宿主列 196“外部阻塞”+8“待验”+3“通过”+1“不适用”；最终列 **4“已验收”、204“未完成”**。

4 项为 Q00-06、Q00-08、Q25-07、Q25-08。这是账本原标记，不是本审查重新为这四项作无条件签收。Q25-07 的宿主基线仍是 4f1dbb4，后续共享资源修改应有失效/复验规则。不能将最终 4/208 理解为仅写了 4 项代码，也不能用自动 207/208 声称接近全部专业级验收。

完整原状态另存为 [208 项状态快照](reviews/ui-finesse-round3-20260915/evidence/ROUND2_STATUS_SNAPSHOT.md)。逐组数字是账本自报，独立检查是下节有依据的抽查，不声称逐个执行了 208 个交互场景。

| 组 | 自动通过/8 | 视觉通过/8 | 最终已验收/8 |
| --- | --- | --- | --- |
| Q00 | 8 | 6 | 2 |
| Q01 | 8 | 8 | 0 |
| Q02 | 8 | 8 | 0 |
| Q03 | 8 | 7 | 0 |
| Q04 | 8 | 8 | 0 |
| Q05 | 8 | 6 | 0 |
| Q06 | 8 | 3 | 0 |
| Q07 | 8 | 7 | 0 |
| Q08 | 8 | 3 | 0 |
| Q09 | 8 | 3 | 0 |
| Q10 | 8 | 4 | 0 |
| Q11 | 8 | 1 | 0 |
| Q12 | 8 | 8 | 0 |
| Q13 | 8 | 8 | 0 |
| Q14 | 8 | 8 | 0 |
| Q15 | 8 | 5 | 0 |
| Q16 | 8 | 6 | 0 |
| Q17 | 8 | 6 | 0 |
| Q18 | 8 | 4 | 0 |
| Q19 | 8 | 5 | 0 |
| Q20 | 8 | 8 | 0 |
| Q21 | 8 | 7 | 0 |
| Q22 | 8 | 8 | 0 |
| Q23 | 8 | 4 | 0 |
| Q24 | 7 | 1 | 0 |
| Q25 | 8 | 4 | 2 |

## 4. 已确认的问题与优先处理项

### F01：按压态对比度没有模拟真实整组透明度（P1，门禁缺陷）

位置：`Infrastructure/AdaptiveThemePaletteContrastGuard.cs:159–162,226–242`。`AddButtonStateSample` 只把 opacity 乘到 Foreground，Background 保持原色；XAML 实际把 `ButtonChrome.Opacity=0.96` 施加到包含文字的整棵按钮视觉。两种合成不是同一件事。

独立负例：黑父背景、白 chrome、黑字、opacity=0.5，当前函数报告白背景、灰字，ratio≈4.004；正确的整组混合应是灰背景、黑字。这里用 0.5 放大差异来验证算法，**不表示生产按钮使用 0.5，也不表示所有现有主按钮一定不达标**。此外按压采样没有同时覆盖 hover+pressed 的组合，渐变 API 只接颜色而丢失 stop offset。

要求：重开 Q03 对按压合成的签收依据；R00-01、R01-05 用可独立计算的正负例与真实控件状态校验。不得靠降低门槛让报告变绿。

### F02：上下文禁用按钮仍有双重淡化（P2，样式问题）

位置：`Themes/WpfUiProduction.xaml:866,927–945`。共享 template 在禁用时设置 chrome 0.72，`GscWpfUiContextButton` 还设置控件整体 0.48，内容有效透明度约 0.3456。普通禁用按钮样本不能覆盖这个派生链。它与“共享 template 已保证可读”的意图不一致。

要求：R00-05 校准真实 Context/RemoteRestore 等派生样式，明确禁用标签的产品可读目标并提供原因入口。此处不把启用文本的强制对比标准直接套给所有禁用控件。

### F03：组合变换下重复缩放会持续嵌套（P2，运行时辅助方法缺陷）

位置：`Infrastructure/GscMotion.cs:128–149`。只复用根节点为 ScaleTransform 的情况；已有 Translate/Rotate/TransformGroup 时，每次调用都新建组与缩放。

独立复现：给 Border 一个 TranslateTransform，调用三次 `GetMutableScaleTransform`，第一次与第二次引用不同，根、child0、child0.child0 都是 TransformGroup。说明 helper 对组合变换不幂等；不是声称所有默认控件都已经泄漏。Q18-06“实例 Transform 复用”的自动/视觉通过不足以覆盖这个条件。R00-02 要测 1000 次节点稳定性。

### F04：未冻结但共享的变换仍会串扰（P2，潜在复用风险）

位置：`Infrastructure/GscMotion.cs:86–124`。只针对 IsFrozen 克隆，没有处理两个实例共用一个可变 TranslateTransform 的所有权。独立探针把同一变换赋给两个 Border，经 helper 修改第一个的 X=42，第二个也变为 42。现有测试仅验证冻结对象克隆。

要求：R08-08 补共享可变、冻结组、已有其他变换的实例隔离；没有找到生产中这样的共享实例之前，不把它写成已发生的用户页面错位。

### F05：35b8276 入场动画依赖时钟持有终态（P2；已有后续修复）

位置：`Infrastructure/GscMotion.cs:163–191`。中途接管已有修复，但初始 base 值仍为 opacity=0/Y=12，清掉时钟会回到透明偏移态。独立探针在开始后清钟得到同样结果。完成时钟继续 HoldEnd 不等同于持久终态正确。

**后续变更**：`4414f05` 已为完成事件写入 opacity=1/Y=0 并增加卸载清理，`3ca74c4` 同步证据。本轮读过该 diff，不再要求重复实现“增加 Completed”这件事。R00-03 应在后续提交上验证完成、取消、卸载、重入与热关闭动画的真实 Dispatcher 行为；旧时钟回调不能覆盖新动画。当前独立 probe 结果明确只属于 35b8276。

### F06：212 DIP 不能证明表头加四行完整可读（P1，布局门禁缺陷）

位置：`Views/MediaCenterView.xaml.cs:394–396`、`Themes/DesignTokens.xaml:186,203`、`UiAudit/UiLayoutAnalyzer.cs` 的 `PRIMARY_VIEWPORT_TOO_SHORT` 检查。

当前行高 52、表头 42，仅表头+四行就需要 250 DIP，还没加边框/水平滚动条。生产表格设置 MinHeight=212，分析器也以 `<212` 判断“不足表头加约四行”，这两个值互相配合并不能证明四行。212 DIP 在该度量下最多容纳约三行完整内容。

要求：R00-06 用行的实际可见交集判断，同时验短窗回退和主操作可达性。不应只把常量改成 250，再用同一常量测试；不同密度、水平条和父级裁剪都需覆盖。

### F07：夹具数值列横向截字但报告 OK（P2，视觉与检测缺口）

位置：本轮双主题 `ui-finesse-fixture.png` 第一行数值 `1,024 / 99,999` 的末位被列边界截断。报告验证了四行纵向完整和独立 Numeric 样本对比，但没有验证这个单元格的横向文字完整。

要求：R01-02 加列内容可读检测及负例。这是**校对夹具**的确定问题，不能仅凭此断言全部生产表格都截字；但它说明 `finesse-fixture OK` 不能替代看图。

### F08：搜索性能样本含无变化输入，预热超时计时器不运行（P1，测量缺陷）

位置：`tests/GameSaveCenter.Playnite.Tests/LargeLibraryPerformanceTests.cs` 的 `GamePicker2000_Benchmark_WritesMeasuredTimings` 和 `WaitForFilteredCountAsync`。

5 次预热以窄查询结束，正式第 0 次还是同一窄查询，因此它不是输入→结果变化样本。预热传入的 timer 是之前已 Stop 的 timer；若过滤计数始终不符合预期且停止值小于 5000，while 中的超时不会增长，测试可能无限等。现有成功运行不消除这个失败路径缺陷。

要求：R00-04 使用每次新计时器/超时令牌，确保全部样本发生变化并输出原始序列；R18-01 再测连续打字、IME 与 20ms debounce 的过滤次数/分配，不能只以减少等待延迟作为流畅证明。

### F09：工具栏分析整棵按名称排除（P2，检测盲区）

位置：`tests/GameSaveCenter.RenderHarness/UiAudit/UiLayoutAnalyzer.cs` 的 `AnalyzeToolbars`。新增 `!IsInsideNamedAncestor(...,"TrainerToolsSettingsScrollViewer")`，让该设置滚动区中的所有候选面板不参与工具栏膨胀检测。

长表单不应被误认为工具栏，这是合理意图；但目前按祖先名字的整棵排除，也会掩盖未来放进去的真正异常工具条。本轮没有据此声称当前设置页一定不可达。R00-07 要用语义/几何分类，保留同祖先内异常工具条的失败负例。

### F10：隔离构建可以让源码测试读错 checkout（P1，测试基础设施缺陷）

位置：多个 `FindRepositoryRoot()` 从 `AppContext.BaseDirectory` 向上查找 `GameSaveCenter.sln`，包括 `UiFinesseFoundationTests`；`scripts/build.ps1 -OutputRoot` 接受仓库外/其他 checkout 的路径。第 2 节记录了实际错根结果。

要求：R01-01 将明确测试源码根与构建身份关联，至少在错根时失败并说明；不能让新分支程序集配 main 源码测试后得到貌似正常的“通过/失败”结论。

## 5. 尚未证明为缺陷的风险与体验空间

1. 游戏选框 `OnPickerPreviewKeyDown` 在 Enter 且已有 SelectedGame 时关闭弹层，需要核查无搜索结果、输入框焦点和 IME 候选确认是否误确认旧游戏。R00-08 先复现再修；源码存在该分支不是物理输入行为已经重现。
2. 历史宿主概览图同时出现“先完成环境准备”、Ludusavi 不可用、顶栏“一切运行正常”，且若干摘要存在“0 项需处理无需处理”等重复组合。图属于 4f1dbb4，35b8276 已有 Overview 后续改动；R20-01/R20-02/R20-08 核对最新行为，不把历史文案无条件重派。
3. ContextMenu/ComboBox 开关、焦点返回、动画重入等新增测试仍有大量源码字符串断言。自动通过可以证明接线代码存在，但不能证明真实事件顺序、IME 和屏幕状态。按 R01-04/R21 补关键行为；不要求把所有结构测试删除。
4. 已有宿主原始证据主要在可清理的 artifacts；本轮只归档了一份 summary 和精选概览。R01-06 应保全关键身份、manifest 和代表性状态，避免其他机器 clone 后只能看到一句“曾经通过”。
5. 高对比、浅色真实宿主、物理跨屏、读屏和真实呈现帧仍不能由离屏/Rendering 回调/逻辑 DPI 代替。继续使用现有隔离审计能力，不绕过系统跟踪的拒绝。
6. 新功能计划中的版本备注、筛选预设、设置搜索等是候选增量；实施前检查现有入口与契约，已满足则给证据并跳过重建。没有把“需要完善的功能体验”写成安全漏洞。

## 6. 交付与下一步

新增 [192 项实施提示词](UI_FINESSE_ROUND3_192_TASKS_2026-09-15.md) 和 [192 行第三轮账本](reviews/ui-finesse-round3-20260915/ROUND3_PROGRESS.md)。每项有具体入口、旧 Q 任务衔接、实施要求、完成条件；初始状态为待开始。

优先 R00/R01：修正计算/测量/生命周期和输入风险后，再推进控件与功能体验。后续实现继续使用已有 Luna 任务与开发分支；本轮不另建实现线程，不自动合并尚未验收的整轮改动。保留当前主仓库用户文件 `src.zip`。

本报告是对固定源码、全量自动测试、精选生产/夹具视觉与关键负例的质量审查，不是全面安全渗透测试，也不是 192 项新功能已经实现的交付声明。

