# U09-05 选框右缘修正

## 问题与改动

用户给出的三个界面截图显示：概览最近任务的时间、存档历史行和任务队列状态徽标未被选中框完整包住。

- `OverviewView` 不再给 `TaskItemContent` 单独绑定比行项更窄的滚动视口宽度；选中框随虚拟化列表项拉伸。
- 共享 `GscRoundedDataGridRowTemplate` 的选中、非活动选中和键盘焦点边框向右扩展。选中框右侧从视口内缩 4 DIP 调整到 1 DIP；键盘焦点边框保留 2 DIP 描边与 2 DIP 视口余量。
- `RowContent` 原有 8 DIP 可视裁切、水平滚动锚定、列虚拟化、状态徽标、命令、Binding、UIA 和滚动条系统未改变。

没有改变失败、运行、取消等业务状态表达，也没有执行真实存档恢复、媒体删除或云端写入。

## 验证

所有命令均由进程启动时隐藏的 PowerShell/cmd 执行并重定向输出；WinEvent 监听覆盖各命令全程，监听线程在 `WinSta0\Default`，该桌面也是 input desktop。编译、WPF 行为、共享资源测试、源码审查和 Render QA 的目标窗口 SHOW/HIDE 均为 0。这个结论只适用于本记录中的命令时段，不能识别或归因用户此前看到的黑窗。

- `scripts/build.cmd -SkipTests -Configuration Release -OutputRoot .tmp/round5-u09-05-final-20261109/build-output -TestTempRoot .tmp/round5-u09-05-final-20261109/test-temp`：SDK 8.0.423，exit 0，0 warning、0 error。启动、SDK、诊断和源码 blob 身份见 [`build/`](build/) 与 [`tested-source-files.json`](tested-source-files.json)。
- 定向 WPF 行为测试：Overview 6/6、Save History 1/1、Task Grid 1/1、选框状态 3/3，合计 11/11，0 失败、0 skip。覆盖浅/深主题、992/1040 DIP 概览、493 DIP 存档表多横向偏移、500 DIP 任务表多横向偏移；每类都在唯一 WinSta0 私有非输入桌面运行，input desktop 前后为 Default。TRX、控制台及桌面身份见 [`tests/`](tests/) 和 [`desktop-identities/`](desktop-identities/)。
- `WpfUiResourceDictionaryTests`：140 项通过、39 项既有 skip、0 失败。39 项由 `LegacyProductionUiBaselineFact` 明确跳过，原因是断言属于已撤销的“今日工作台”架构，未计为通过。TRX 的 XML `ResultSummary` 将 `notExecuted` 记为 0，但结果集合中有 39 条 `NotExecuted`，控制台也逐项显示 `[SKIP]`；以结果集合和控制台为准。包含本次共享 DataGrid 选框几何断言的 `SharedDataGridSelectionUsesTrainerRoundedCardChrome` 已通过。
- `python scripts/validate-source.py` exit 0。WPF 技能静态审查 exit 0，扫描 398 个仓库 XAML 文件，0 errors、218 warnings、1440 info；扫描包含已有 `.tmp` 历史副本。当前 `WpfUiProduction.xaml` 没有告警；`OverviewView.xaml` 的两条 Large ScrollViewer/StackPanel 提示在修改前后均存在，和选框宽度变更无关。完整输出见 [`source-checks/`](source-checks/)。
- 文档归档完成后，再运行一次 `python scripts/validate-source.py` 和五个本轮 Markdown 文件的本地相对链接检查，均 exit 0；该次只有文档变更，因此未重跑完整构建、静态 XAML 审查或 Render QA。末次检查摘要见 [`source-checks/final-doc-link-check.json`](source-checks/final-doc-link-check.json)。
- `scripts/render-qa.ps1 -Configuration Release -Output .tmp/round5-u09-05-final-20261109/selection-frame-fix-20261109/render-qa`：exit 0，394 PNG、0 条 `PROBLEM`。报告见 [`render-qa/render-qa-report.txt`](render-qa/render-qa-report.txt)。
- 离屏截图确认时间与徽标完整落在选中框内；窄视口和最大横向偏移图见 [`captures/`](captures/)。截图来自合成 WPF 页面，不是实际 Playnite 宿主验收。

另有一条补加的选框状态行为测试起初因硬编码旧右侧 Margin 而失败。没有删断言或加 skip；更新为本次设计的边距后重新编译，四个行为类 11/11 通过。旧期望失败 TRX 与诊断保存在 [`diagnostics/stale-margin-expectation/`](diagnostics/stale-margin-expectation/)。

## 未验边界

最终中文安装启动入口仍未运行，真实 Playnite 页面、外部 UIA、实体键盘/IME 与物理 DPI 仍待验收。只读核对时 Playnite PID 24248 与 Worker PID 42704 仍运行且 `MainWindowHandle=0`；本批没有关闭、重启或强杀它们。用户此前询问的正常退出命令尚未在这些进程上得到确认；最终安装启动前需再次确认进程已正常退出。未更改驱动器、盘符或映射。

本地编译和静态审查基于 `17dc5fd0dbaffd8fe2f2c5398aa0a37b5f77f498` 的工作树候选；[`tested-source-files.json`](tested-source-files.json) 固定了参与本批的七个已跟踪源文件 Git blob。精确候选提交的双 SDK CI、最终完整独立构建及中文根入口链仍待执行。

归档文件 SHA-256 与长度见 [`manifest.json`](manifest.json)。
