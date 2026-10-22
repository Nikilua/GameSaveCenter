# 表格滚动与详情区域修复（2026-10-22）

用户五张截图涉及任务表格居中留白、筛选项纵向堆叠、滚动时文字与选中框分离、详情卡片过窄/过短，以及窄列标题竖排。

## 已确认的原因和修复

- 共享行模板及 Dashboard 兼容模板把 `DataGridCellsPresenter` 的 `SelectiveScrollingOrientation` 设成 `Horizontal`，会反向补偿纵向滚动。回归在中段测得文字相对行顶从 `16.67` 漂到 `974.27` DIP；改为 `Both` 后，文字与行框一起滚动。保留原有焦点样式、选择状态、排序、缩放列与虚拟化。
- 任务表格的固定子高度被放进更高的星号行，Stretch 会把固定尺寸居中。回归测得框架高 `703.33`、表格高 `513.33`、表格顶偏移 `94.67` DIP。现在直接测量实际工具区，有限页面 Grid 分配剩余星号行，表格填满框架；短窗口仍保留四行底线与页面溢出通道。
- 紧凑任务筛选使用水平 WrapPanel，整组标签/选择器一起按实际宽度换行。宽窄往返恢复各行的 Grid 行号和跨列，保留绑定与选值。
- 表头单行省略，完整标题/说明保留 Tooltip 和 UI Automation HelpText。列拖动、排序目标与缩放柄保留。
- 三个工作区使用独立 `480 DIP` 详情宽度令牌；通用 Inspector 仍为 `360 DIP`。详情边框移到内容 ScrollViewer 外，固定圆角、留出内边距。任务/存档详情利用整个左侧区域的高度；媒体待归类宽窗口让左侧页脚滚动与右侧详情滚动独立，窄窗口将同一详情移入页面抽屉。

## 受控验证

环境：锁定 SDK `8.0.423`、Release、Windows WPF。结果来自 `87eaab38` 后的实施工作树；精确提交上的标准命令全链尚待下一阶段运行，不能用本段代替安装/启动证明。

- [初始几何失败](tests/focused-2.trx) 保留真实表格居中与累计文字漂移的失败证据；同批 Q14 两类共进程出现的 Application 跨 Dispatcher 问题通过按类独立进程运行核验，未修改断言或增加 skip。
- [任务、紧凑视口与媒体几何](tests/focused-4.trx)：20/20；媒体宽详情结构调整后的[最终媒体滚动](tests/media-final.trx)：6/6，包括 100%/125%/150% 输出变换、反复首尾滚动、缩放窗口、分页锚点、滚动条角落、紧凑边界传递，以及宽详情边框在左右两路滚动时保持原位。
- [任务筛选](tests/Q14FilterLabelBehaviorTests.trx) 1/1（16 个双主题尺寸场景，760 DIP 及以上四组筛选同一行）；[工具栏](tests/Q14ToolbarAlignmentBehaviorTests.trx) 1/1；[表头](tests/R06ColumnHeaderHelpTests.trx) 2/2；[文字缩放](tests/R03TextScaleTests.trx) 2/2。
- [资源字典完整类](tests/resources-final.trx)：140 pass / 39 原有历史 skip / 0 fail；未新增 skip。
- [完整 RenderHarness 报告](render-qa-report.txt)：`render-qa OK`、0 PROBLEM；包含 Light/Dark、多尺寸、生产 PageHost 和抽屉打开后的行数/边界检查。折叠抽屉中尚未实例化的关闭按钮改用 XAML namescope 查找，打开后的可见性和测量断言保留。
- XAML 25/25；source validation 通过；WPF 静态检查 0 error / 26 warning（已有项目提示）；`git diff --check` 通过。源码验证器现检查任务有限页面根与四行边框预算，并识别显式 MaxHeight=260 的媒体历史列表；未取消有限视口门禁。

截图：[任务](screenshots/Task-1600x900.png)、[存档](screenshots/Save-1600x900-tab0.png)、[媒体待归类](screenshots/Media-1600x900-tab0.png)。这些是合成离屏 WPF，不能当作真实 Playnite 交互或物理跨屏 DPI 验收。

## 构建与产物

根 `GameSaveCenter-Run.cmd` 调用 `dev-install-run.ps1 -Configuration Release` 并保存/传播退出码；中文入口调用该根命令并传播退出码；`scripts/build.cmd` 只转交构建。首次完整默认链在 `52f74b1e` 上因旧媒体滚动源码契约退出 1；新分栏契约定向 6/6，完整链仍待新提交复跑。见[首次标准失败与修正](FIRST_STANDARD_FAILURE.md)，当前不宣称安装启动通过。

本阶段临时输出位于 `artifacts/ui-layout-fix/` 与 `artifacts/ui-layout-*.log`。起始复核 D 卷可用 `9,035,120,640` 字节，后续阶段记录归档与安全清理量。用户原有 Round3 未跟踪 TRX 和 Tests/TestResults 未添加、覆盖或删除。Round4 总状态仍为 40 已验收、2 已满足、18 代码完成待验收、3 待开始、1 外部阻塞。
