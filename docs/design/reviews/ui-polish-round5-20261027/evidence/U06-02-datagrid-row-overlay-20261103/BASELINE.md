# U06-02 / U08-08 Dashboard兼容 DataGrid 行选中框不挤内容

日期：2026-11-03。继续落实“蓝色圆角框可以保留，但选中不要让整行和图标右下偏移”的要求，并复核相邻共享选中样式。

## 变更与边界

`OverviewView` 首页最近活动的生产修正已在 `f9d09cb2` 移除 `TaskItemChrome` 的 `Margin=4,2`，蓝色圆角框保留。该生产路径本批未改；本批在 `DashboardView.xaml` 的兼容性隐式 `DataGridRow` 样式发现同类测量风险：选中时，带 inset Margin 的边框曾包住 `SelectiveScrollingGrid` 内容。现在行内容、选中色背景和框线是同级元素；选中态的 `Margin=4,2,12,2` 只作用于背景/框线 sibling overlay，框线不命中鼠标，WPF 选择/虚拟滚动所需的 `SelectiveScrollingGrid` 与 `PART_CellsPresenter` 保留。浅/深主题的实际 DataGrid 行回归检查单元格相对行边界不变、蓝色描边仍存在。

共享生产 `GscStableDataGridRow`、Overview 与公共 `ListBoxItem` 已分别有既有 overlay 几何回归。本批抽查确认它们无需为统一视觉再改写：最近活动与公共 ListBox 的独立回归各 1/1 通过；任务、媒体、存档、维护四个生产表格的共享行回归 1/1 通过。没有改命令、Binding、游戏选择、UIA 名称、滚动条或业务语义。

## 本地检查

- SDK 8.0.423 / Release：新增 `DashboardCompatibilityRowKeepsCellContentInPlaceWhenSelectedInBothThemes` 1/1 通过、0 失败、0 skip；共享 DataGrid 回归 1/1 通过；最近活动和共享 ListBox 回归 1/1 通过。三个通过 TRX 及 SHA-256 在 [`test-runs.json`](test-runs.json) 与 [`tests/`](tests/) 中。
- 另有隔离/组合运行不通过：R06 隔离为2通过/1失败，失败为既有生产行夹具未生成；R06+R10组合为3通过/3失败，报文包括 DataGrid 行未生成、最近访问选择项实例断言及容器未生成。另保存了 8 次新增夹具完善过程中的失败 TRX。失败均未计通过、未扩大 skip；完整结果和哈希见[`test-runs.json`](test-runs.json)。
- `python scripts/validate-source.py` 退出码0；`git diff --check` 通过。WPF静态审查扫描218份XAML，0 errors、208 warnings、1327 info；既有警告/信息包含仓库保留的临时快照，原始输出见[`wpf-static-review.txt`](wpf-static-review.txt)。
- 本批已先运行 `scripts/render-qa.ps1`，生成394张 Light/Dark 截图且报告为0 `PROBLEM`；但当时工作树未提交，报告绑定父提交 `e49c52be` 且 `WorkingTreeClean: False`，故只作为预检查，不用作冻结候选证据。精确源码提交后会重跑并在此补录。

## 交付边界

当前变更从父提交 `e49c52bead9108a2e8943f79ff206e1ddca256f5` 派生。源文件与测试的提交身份、精确提交 Render QA、远端双 SDK CI、清理清单在提交后补录。此批没有重试先前因 PowerShell `ConstrainedLanguage` 在启动阶段拒绝 dot-source 的本地完整入口；这项硬门禁仍未满足，需在组织授权的 FullLanguage 环境执行真实入口。Playnite 实际宿主、外部 UIA、实体键盘/IME 和物理 DPI 待验；没有执行真实存档恢复、媒体删除或云端写入。
