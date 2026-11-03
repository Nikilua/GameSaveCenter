# U04-03 / U06-02 / U08-08：选中框可视区与按压几何

## 本批决策

用户截图指出两种共通问题：Overview 最近活动的蓝色选中框与右侧垂直滚动条相贴/重叠；存档和任务表格在窗口变窄并出现水平溢出时，选中框的右侧圆角落在裁切区之外。选中/按压效果可以保留，但不能推动行内容或裁掉图标。本批按共享装饰层处理：Overview 与共享 ListBox 的选中/焦点框向滚动槽内缩16 DIP；共享 DataGrid 与 Dashboard 兼容 DataGrid 的选中/焦点框按 `DataGridRowsPresenter.ActualWidth` 限制宽度，框线仍留在可视区，单元格仍由原有水平滚动模型滚动。共享按钮按压保留半透明覆盖层，移除0.97缩放，避免按钮文字和图标随状态偏移。

未改变命令、Binding、业务状态、行容器、虚拟化、滚动条、主题、UIA或键盘输入语义。选中框仍为圆角；装饰层的留白不参与内容测量。真实 Playnite 页面尚未打开观察。

## 源码基线与验证

- 预提交父提交：`10682b05209d1b0784f52dbed7195c8d31a7d297`；本证据中的构建、测试和 Render QA 对该父提交上的本批脏工作树运行，不能误读为已提交 SHA。源码候选提交与远端 CI 身份随后补入 Round5 进度记录。
- SDK 8.0.423 / Release 定向编译：0 warning、0 error。定向组合为12项通过、0失败、0跳过，原始TRX见 [`tests/passed/`](tests/passed/)。覆盖共享按钮状态、按压反转/释放、高对比度、Overview最近活动和公共ListBox几何、四页DataGrid状态、Dashboard兼容DataGrid与水平溢出时的圆角框边界。
- `LegacyProductionUiBaselineFact` 旧静态断言被隔离为条件跳过，见 [`legacy-static-overview-contract-skipped.trx`](tests/diagnostics/legacy-static-overview-contract-skipped.trx)；未计入通过数，未扩宽 skip。
- 双主题 `scripts/render-qa.ps1` 预提交运行退出码0，报告无 `PROBLEM`，递归图像394张。报告与身份元数据见 [`render-qa/`](render-qa/)。抽查Overview、Save和Task的1040×700图：最近活动选框避开右侧滚动条，Save/Task窄表格右侧圆角仍落在可见区域。
- WPF静态检查：218 XAML / 0 errors（208 warnings、1327 info，含既有保留快照）；XAML检查26项通过；Source Validation退出0。静态原始报告见 [`wpf-static-review.txt`](wpf-static-review.txt)。
- 此处属于UI_CHANGE_GATE源码/行为/双主题Render QA验证，不等价于 Playnite 实际宿主验收。

## 失败与诊断保留

尝试1为测试适配器触发 .NET Framework MAX_PATH；尝试2为旧测试仍期待缩放动画；尝试3为探针错误假定最近任务行含按钮；尝试4真实测出框宽1336 DIP而视口仅672 DIP；尝试5为R10仍断言旧的零留白；尝试6编译器缺少 `System.Windows.Controls.Primitives` 引用。分别修正执行路径、断言/探针、共享行框宽度及using后，最终定向组合12/12通过。原始失败TRX、编译摘要及分类记录见 [`tests/diagnostics/`](tests/diagnostics/)；未删除、跳过或放宽相关断言。

## 尚未验收

截至这份预提交记录，当前可用C盘空间为3,186,733,056 bytes；仓库预算为输出2 GiB、测试临时1 GiB、系统临时1 GiB，完整中文入口再加包1 GiB。完整链空间预检未满足前不得启动。尚未对本源码候选运行中文根入口完整编译/测试/Worker发布/打包/安装身份/Playnite启动链、独立 `scripts/build.cmd` 或精确SHA GitHub jobs。不得将旧提交通过、局部编译、渲染或本地定向测试当作该门禁通过。Playnite真实呈现、UI Automation客户端、实体输入/IME和物理DPI仍待验；未运行真实存档恢复、媒体删除或云写入。
