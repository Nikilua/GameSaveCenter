# U07-05 通知来源字段与列表精修

实现候选基于 `9086154134982dfd7c5a4f0fb317d59a4285bfd2`。候选源码增加 `NotificationRecordDto.Source`，限长并规范空白；SQLite通过 `EnsureColumnAsync` 为旧表加 `source TEXT NOT NULL DEFAULT ''`，不从旧摘要或标识推断来源。任务来源使用真实 `TaskTypeDisplay`，退出会话摘要使用显式事件来源“游戏会话”。通知列表在标题下以次级文字显示来源，详情面板也显示来源；旧记录显示“来源未记录”。标题排序和复制仍绑定 `Title`，长标题Tooltip/UIA、焦点、DataGrid共享样式、行列虚拟化及现有滚动系统保留。

## 双主题前后对照

RenderHarness使用固定样本 `2026-10-28T09:00:00Z`、逻辑DPI 1.00、窗口 `1040×700 DIP`，分别输出浅色与深色通知页。除新增的来源数据/展示外，标题、摘要、级别、游戏/任务ID和相对时间沿用同一夹具。完整Render QA递归产出394张PNG、`PROBLEM`计数为0，进程退出码0；报告见 [渲染前报告](render-qa-before-report.txt) 和 [渲染后报告](render-qa-after-report.txt)。报告SHA-256分别为 `A0C5743335619B480856D9687953A67385B2A5458A300CED9BCDCE843255F328`、`FD755E94EE8D023B24D6C59BF1BA53E51C98A495E9B3065E1A3BE33C3E6A9A51`。同样本浅/深主题1040×700前后图和SHA见[对照清单](render-pair.json)。

浅色与深色对照图均显示第二行“来源”，标题和摘要仍可读；行高、列宽、底部滚动条和操作区未改。截图仅代表离屏Harness逻辑DPI，不等同真实Playnite宿主、物理DPI或外部UIA验收。当前完整报告采集于源码冻结前，报告中的HEAD为基准提交且 `WorkingTreeClean=False`；它没有被描述为候选提交的最终根链证据。

## 局部编译与行为检查

SDK `8.0.423`、Release、`--no-restore --disable-build-servers -m:1`：Worker通知持久化13/13，Playnite通知聚合/视图行为7/7，均0失败、0 skip。覆盖来源重启持久化、旧表加列迁移、空来源不覆盖已有来源、旧数据的“来源未记录”、标题排序/复制、Tooltip/UIA及新通知不抢搜索焦点。两份TRX已脱敏归档于 `tests/`，脱敏与原始SHA见[验证清单](validation.json)。Playnite项目输出的 `NU1900` 仅表示本机不可访问NuGet漏洞数据源；后续冻结候选完整构建仍需验证。

`python scripts/validate-source.py`退出0；`scripts/check-xaml.ps1`为26个XAML文件通过；WPF静态审查扫描42个XAML、0 errors、26既有warnings、179 info，未报告 `NotificationInboxView.xaml` 问题，完整结果见[wpf-static.txt](wpf-static.txt)。

本记录当前对应冻结前局部候选；中文根入口完整构建/测试/发布/打包/安装身份/启动、独立 `scripts/build.cmd`、候选精确SHA GitHub双SDK CI、artifact身份及最终清理在冻结提交后补记。真实Playnite刷新、外部UIA/Narrator、实体键盘/IME、物理DPI与真实存档恢复/媒体删除/云端写入不在本批视觉验证范围。
