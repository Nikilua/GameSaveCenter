# 表格滚动与详情区域修复（2026-10-22）

用户五张截图涉及任务表格居中留白、筛选项纵向堆叠、滚动时文字与选中框分离、详情卡片过窄/过短，以及窄列标题竖排。

## 已确认的原因和修复

- 共享行模板及 Dashboard 兼容模板把 `DataGridCellsPresenter` 的 `SelectiveScrollingOrientation` 设成 `Horizontal`，会反向补偿纵向滚动。回归在中段测得文字相对行顶从 `16.67` 漂到 `974.27` DIP；改为 `Both` 后，文字与行框一起滚动。保留原有焦点样式、选择状态、排序、缩放列与虚拟化。
- 任务表格的固定子高度被放进更高的星号行，Stretch 会把固定尺寸居中。回归测得框架高 `703.33`、表格高 `513.33`、表格顶偏移 `94.67` DIP。现在直接测量实际工具区，有限页面 Grid 分配剩余星号行，表格填满框架；短窗口仍保留四行底线与页面溢出通道。
- 紧凑任务筛选使用水平 WrapPanel，整组标签/选择器一起按实际宽度换行。宽窄往返恢复各行的 Grid 行号和跨列，保留绑定与选值。
- 表头单行省略，完整标题/说明保留 Tooltip 和 UI Automation HelpText。列拖动、排序目标与缩放柄保留。
- 宽屏下任务、存档和媒体待归类工作区将表格与详情按 `1*:1*` 等分，避免右栏固定宽度造成空间浪费；收件箱在宽度不足以容纳 800 DIP 表格、520 DIP 详情和 14 DIP 间隔时切换为紧凑详情。当前游戏媒体用 `1*:0.75*`，保留 1280 DIP 下两张比较图并排的空间，详情仍比原固定 480 DIP 更宽。通用 Inspector 仍为 `360 DIP`。存档摘要操作根据表格列宽在 880 DIP 处切换并排/独立操作行。详情边框在内容 ScrollViewer 外保持固定圆角，滚动不会裁掉卡片上下边缘。任务/存档详情利用页面剩余高度，媒体待归类左右滚动互相独立。

## 受控验证

环境：锁定 SDK `8.0.423`、Release、Windows WPF。最初受控结果来自实施工作树；最终独立离屏报告和标准默认完整链已在精确提交 `4f40e016` 通过，见[完整验收](STANDARD_BUILD.md)。

- [初始几何失败](tests/focused-2.trx) 保留真实表格居中与累计文字漂移的失败证据；同批 Q14 两类共进程出现的 Application 跨 Dispatcher 问题通过按类独立进程运行核验，未修改断言或增加 skip。
- [任务、紧凑视口与媒体几何](tests/focused-4.trx)：20/20；媒体宽详情结构调整后的[最终媒体滚动](tests/media-final.trx)：6/6，包括 100%/125%/150% 输出变换、反复首尾滚动、缩放窗口、分页锚点、滚动条角落、紧凑边界传递，以及宽详情边框在左右两路滚动时保持原位。
- [任务筛选](tests/Q14FilterLabelBehaviorTests.trx) 1/1（16 个双主题尺寸场景，760 DIP 及以上四组筛选同一行）；[工具栏](tests/Q14ToolbarAlignmentBehaviorTests.trx) 1/1；[表头](tests/R06ColumnHeaderHelpTests.trx) 2/2；[文字缩放](tests/R03TextScaleTests.trx) 2/2。
- 新增宽屏三页等宽断言，覆盖任务队列、存档历史/候选、媒体待归类/当前游戏媒体；筛选组仍按整组换行，760 DIP 及以上可放在同一行。
- [资源字典完整类](tests/resources-final.trx)：140 pass / 39 原有历史 skip / 0 fail；未新增 skip。
- [完整 RenderHarness 报告](render-qa-report.txt)：`render-qa OK`、0 PROBLEM；包含 Light/Dark、多尺寸、生产 PageHost 和抽屉打开后的行数/边界检查。折叠抽屉中尚未实例化的关闭按钮改用 XAML namescope 查找，打开后的可见性和测量断言保留。
- XAML 25/25；source validation 通过；WPF 静态检查 0 error / 26 warning（已有项目提示）；`git diff --check` 通过。源码验证器现检查任务有限页面根与四行边框预算，并识别显式 MaxHeight=260 的媒体历史列表；未取消有限视口门禁。

截图：[任务](screenshots/Task-1600x900.png)、[存档](screenshots/Save-1600x900-tab0.png)、[媒体待归类](screenshots/Media-1600x900-tab0.png)。这些是合成离屏 WPF，不能当作真实 Playnite 交互或物理跨屏 DPI 验收。

## 构建与产物

最终源码 `4f40e016` 的默认中文入口已完整退出0：编译0/0、1908项（1851 pass、57原有skip、0 fail/error/timeout/abort）、140个WPF类、143份TRX计数一致，发布、打包、安装身份与Playnite启动均成功。[完整证据与SHA](STANDARD_BUILD.md)。未跳测试、未扩大skip、未放宽源码身份；旧结构字符串和独立宿主接线遗漏已同步到实际结构/入口，同时修复详情加宽后的媒体工具栏错排，并保留原对齐断言。

[初次失败](FIRST_STANDARD_FAILURE.md)、[第二次失败](SECOND_STANDARD_FAILURE.md)、[后续失败与修正](FOLLOWUP_STANDARD_DIAGNOSTICS.md)全部保留；不以通过覆盖失败。最终离屏报告0 PROBLEM，三张截图已更新。固定检出已归档，归档前已逐文件保留诊断、最新构建和包；清理本阶段39个旧目录，详情见完整证据。用户旧Round3 TRX/TestResults未添加、覆盖或删除。

真实Playnite页面交互与物理输入未验证；Round4状态不变，继续独立实现S00-06～08。

## 2026-10-23 详情列宽与紧凑断点复核

在工作区 1:1 扩宽后，补充当前游戏媒体 `1*:0.75*` 列权重，保持两图比较在 1280 DIP 并排；将收件箱紧凑断点按实际 800 DIP 表格/520 DIP 详情预算计算，并将存档摘要操作的并排断点调至 880 DIP。图片比较摘要和操作按钮分行，避免窄列文件名逐字换行。此前的累计漂移、任务表格留白、筛选整组换行、表头单行省略与滚动固定圆角结构继续由共享模板和既有回归覆盖。

SDK 8.0.423 Release 编译 0 警告/0 错误；全量测试 1954 项，1886 通过、68 个既有跳过、0 失败，144 个 WPF 隔离类。`ReportedWorkspaceLayoutBehaviorTests` 双主题 54/54；`CompactWorkspaceTableViewportBehaviorTests` 与媒体比较定向用例通过。完整诊断位于本机 `artifacts/build-diagnostics/20261023T033037645Z-3b5ae286`。这一轮打包在未提交源码身份门禁处被正确拒绝，提交后必须重新跑干净提交的标准链，不能将这次测试称为最终安装验收。用户旧 Round3 TRX/TestResults 未暂存或删除。

提交 `f3ac4c743ce50705871e03d074127f89824e11dd` 后，标准链完成 SDK8.0.423 Release solution 0/0、1954 项1886 pass/68 skip/0 fail和144个WPF隔离类，诊断 `artifacts/build-diagnostics/20261023T052656925Z-a06c8358`。发布/身份/打包通过；安装写入被沙箱阻止后，同包续装239文件SHA一致，记录 `artifacts/layout-install-resume-20261023.json`。RenderHarness同提交输出 `artifacts/ui-width-regression-20261023/render-clean-commit` 返回 render-qa OK。真实页面交互尚不据此声明通过。

## 2026-10-23 新截图：存档操作区异常高度

880 DIP断点仍不能代表操作组所需的真实宽度。扩展回归复现摘要宽度0 DIP、卡片高5754 DIP、操作条1604 DIP宽。改为摘要与操作条上下两个Auto行，各自拿到有限卡片宽度；历史导航按钮不再包含在一个不可换行大组内。双主题报告布局54/54已通过，860～2100 DIP往返检查摘要宽度、内容外留白、按钮横向边界，并保留修复前失败与后续TRX：`artifacts/save-summary-reflow-20261023`。当前源码的官方身份构建、资源/缩放/紧凑视口回归和最终安装待本阶段完成后记录，不继承f3ac4c74全量结果。
