# U06-02 / U08-08 Dashboard兼容 DataGrid 行选中框不挤内容

日期：2026-11-03。继续落实“蓝色圆角框可以保留，但选中不要让整行和图标右下偏移”的要求，并复核相邻共享选中样式。

## 变更与边界

`OverviewView` 首页最近活动的生产修正已在 `f9d09cb2` 移除 `TaskItemChrome` 的 `Margin=4,2`，蓝色圆角框保留。该生产路径本批未改；本批在 `DashboardView.xaml` 的兼容性隐式 `DataGridRow` 样式发现同类测量风险：选中时，带 inset Margin 的边框曾包住 `SelectiveScrollingGrid` 内容。现在行内容、选中色背景和框线是同级元素；选中态的 `Margin=4,2,12,2` 只作用于背景/框线 sibling overlay，框线不命中鼠标，WPF 选择/虚拟滚动所需的 `SelectiveScrollingGrid` 与 `PART_CellsPresenter` 保留。浅/深主题的实际 DataGrid 行回归检查单元格相对行边界不变、蓝色描边仍存在。

共享生产 `GscStableDataGridRow`、Overview 与公共 `ListBoxItem` 已分别有既有 overlay 几何回归。本批抽查确认它们无需为统一视觉再改写：最近活动与公共 ListBox 的独立回归各 1/1 通过；任务、媒体、存档、维护四个生产表格的共享行回归 1/1 通过。没有改命令、Binding、游戏选择、UIA 名称、滚动条或业务语义。

## 本地检查

- SDK 8.0.423 / Release：新增 `DashboardCompatibilityRowKeepsCellContentInPlaceWhenSelectedInBothThemes` 1/1 通过、0 失败、0 skip；共享 DataGrid 回归 1/1 通过；最近活动和共享 ListBox 回归 1/1 通过。三个通过 TRX 及 SHA-256 在 [`test-runs.json`](test-runs.json) 与 [`tests/`](tests/) 中。
- 另有隔离/组合运行不通过：R06 隔离为2通过/1失败，失败为既有生产行夹具未生成；R06+R10组合为3通过/3失败，报文包括 DataGrid 行未生成、最近访问选择项实例断言及容器未生成。另保存了 8 次新增夹具完善过程中的失败 TRX。失败均未计通过、未扩大 skip；完整结果和哈希见[`test-runs.json`](test-runs.json)。
- GitHub run `41525415460`（#1438）验证精确提交`cf5173e75758782ad2bcd372cc9492ad393b908b`，`.github/workflows/windows-build.yml` 两个适用 job 均失败：基线job `125551752423` 在“编译与测试”失败，SDK10.0.401 job `125551752823` 在Release编译与测试失败；两者失败测试均为`R06SelectionStateBehaviorTests.ProductionRowsKeepFailureInItsStatusCellWithoutReplacingSelectionChrome`，异常`DataGrid row was not generated.`。Render QA、WPF静态审查与扩展包步骤因门禁失败而跳过；两份构建诊断artifact成功上传，无预览包。job日志及artifact ID/digest见[`ci-run-41525415460.json`](ci-run-41525415460.json)。
- 本机定位为测试夹具STA生命周期问题：独立测试先创建的`Application.Current`仍关联已结束的Dispatcher，随后生产行测试的Window未加载。没有改产品选择行为或弱化断言；将真实Dashboard兼容行Light/Dark几何断言放入同一活动STA窗口中，在生产行测试关闭主窗口前完成，另保留独立静态XML sibling契约检查。SDK8.0.423 Release以修正后的工作树复测R06整类3/3、0失败/0 skip；提交`d5387b39dc1a3bc1e10ba0a61363e48876faca38`后精确R06整类仍为3/3、0失败/0 skip，TRX SHA与提交身份见[`r06-fixture-lifecycle-runs.json`](r06-fixture-lifecycle-runs.json)。迭代失败/通过TRX及哈希见该JSON与[`tests/`](tests/)。
- `python scripts/validate-source.py` 退出码0；`git diff --check` 通过。WPF静态审查扫描218份XAML，0 errors、208 warnings、1327 info；既有警告/信息包含仓库保留的临时快照，原始输出见[`wpf-static-review.txt`](wpf-static-review.txt)。
- 中文提交 `cf5173e75758782ad2bcd372cc9492ad393b908b`（`修正Dashboard行选中框内容偏移`）已推送至 `origin/main`，父提交为 `e49c52bead9108a2e8943f79ff206e1ddca256f5`。精确提交的 `scripts/render-qa.ps1 -Configuration Release` 退出码0；RenderHarness Release 0 warning/0 error，Light/Dark 共394 PNG、0 `PROBLEM`。报告记录 `WorkingTreeClean: True`、逻辑DPI 1.00、固定样本UTC `2026-11-03T00:00:00Z`；SHA-256 `5B549CF7626D973F3FAF1411552066E95FB590A6CC1D93615C8C30EF96C2E3CB`，原始报告见[`render-qa-report.txt`](render-qa-report.txt)，394张本机审阅图保留于 `artifacts/ui-qa/U06-02-datagrid-row-overlay-20261103/`。该全页渲染QA不等同Playnite真实宿主验收。

## 修复提交的 GitHub CI 终态

精确提交`d5387b39dc1a3bc1e10ba0a61363e48876faca38`对应run [41529821227（#1439）](https://github.com/Nikilua/GameSaveCenter/actions/runs/41529821227)，事件为push、head SHA与该提交一致，run completed/success。核对工作流定义和实际job列表共2个适用job：基线job125566804922“编译、测试与打包”15/15步骤success；SDK10.0.401 job125566804561“编译与测试复核”8/8步骤success。

基线SDK8.0.423/Release完整编译与测试2026项、1968通过、0失败、58既有skip、155个WPF隔离类。之后`package.ps1`再次执行其内嵌build/test，同样2026项、1968通过、0失败、58 skip、155个隔离类；SDK10完整测试亦为该结果。skip按已有原因说明且不计通过。较早的11项诊断采集自测包含一条故意构造的失败样本，但诊断自测本身通过，不混入三次完整测试汇总。Render QA步骤成功；WPF UI静态审查扫描32份XAML、0错误、26 warning、179 info。Worker及Playnite包生成成功，包内六个相关程序集的AssemblyInformationalVersion均为`0.6.73+d5387b39dc1a3bc1e10ba0a61363e48876faca38`，脚本完成包内容、版本和自包含Worker校验。

三个上传artifact均绑定head SHA `d5387b39dc1a3bc1e10ba0a61363e48876faca38`：预览包artifact 13185515505，90,790,822 bytes，digest `sha256:eb87bc0bd2618bd18a51dc8bddd78e1bab5b525e2fdb84d824ee035b5bfaae26`；基线诊断artifact 13185097388，2,032,144 bytes，digest `sha256:219d7e221db5eaf7ced48afb98d7a2351454adeea4e34df16e48c6e169ee58e8`；SDK10诊断artifact 13182029784，972,520 bytes，digest `sha256:d592fb94da12eed93e3eebf03055faad3618bca8d528fa9c69a05fc25b76e29d`。Run/job全步骤、SDK、源码身份、artifact期限与digest快照见[`ci-run-41529821227.json`](ci-run-41529821227.json)及[run #1439](https://github.com/Nikilua/GameSaveCenter/actions/runs/41529821227)。上一个cf5173提交的#1438失败仍作为历史记录，不覆盖为通过。

## 交付边界

`cf5173e7`的历史CI失败已定位为测试夹具生命周期并修复；`d5387b39`精确本机R06与#1439双SDK CI均通过。required-check/规则集API此前403，不重复请求，完整性仍“远端待验”。此批没有重试先前因PowerShell `ConstrainedLanguage`在启动阶段拒绝dot-source的本地完整入口；U09-05硬门禁仍未满足，需在组织授权FullLanguage环境对当前候选执行真实入口。Playnite实际宿主、外部UIA、实体键盘/IME和物理DPI待验；没有执行真实存档恢复、媒体删除或云端写入。

阶段起点和首次源码QA输出清单见[`stage-start.json`](stage-start.json)与[`cleanup.json`](cleanup.json)。后续R06夹具修复测试阶段的起点SDK/空间见[`r06-fixture-stage-preflight.json`](r06-fixture-stage-preflight.json)，提交SHA `d5387b39dc1a3bc1e10ba0a61363e48876faca38`的本机R06整类3/3、0 skip TRX已归档。`.tmp/round5-u09-06-sdk10-r06-fix-20261103/`移除前核对路径确属仓库`.tmp`、0重解析点、0个dotnet/MSBuild/VSTest/Playnite进程、0个Markdown链接指向该临时路径；清理337个文件/29,378,923 bytes，C盘可用5,000,470,528→5,030,694,912 bytes，D盘8,366,694,400 bytes未变。删除清单及磁盘前后见[`r06-fixture-stage-cleanup.json`](r06-fixture-stage-cleanup.json)。SDK8.0.423、394张本机Render QA PNG及TRX/报告保留；首轮Render QA阶段`.tmp/round5-u08-08-datagrid-selection-overlay-20261103/`的清理明细见`cleanup.json`。
