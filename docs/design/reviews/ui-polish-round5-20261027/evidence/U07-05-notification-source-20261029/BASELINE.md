## 2026-10-29 U07-05冻结候选门禁

源码提交 `cd06253158c0e35d43de84e037548a764629749a` 已推送main。SDK8.0.423 Release中文根入口完整构建、测试、发布、打包、安装身份核验及启动调用退出0（run `20261028T235825476Z-d43f8012`）；独立 `scripts/build.cmd` 规范输出树全套退出0。两次各2013项、1955通过、0失败、58个既有skip（57个撤销架构断言、1个opt-in系统指针测试），skip不计通过。

GitHub run [40794111982](https://github.com/Nikilua/GameSaveCenter/actions/runs/40794111982)的baseline job `123186245250` 与SDK10.0.401 job `123186245559`均成功；baseline源码、诊断自测、SDK基线负例、构建测试、Render QA、WPF静态、打包、上传均成功。artifact IDs为`12865861325`（baseline诊断）、`12865448641`（preview包）、`12863324535`（SDK10诊断），digest见本批证据 `final-gates/ci.json`。保护required-check名单此前403，本批未复探，U09-06仍远端待验。

首次受限build.cmd多出10项Named Pipe/Worker环境skip；提权临时输出树一次Q10复选框合成空格事件2项失败。保留失败TRX，未修改断言或skip。规范输出树完整复跑通过，但根因尚未证明，不能将路径认定为已证实原因。根入口完成启动调用，后续进程查询未见Playnite，因此持续运行、插件已加载及真实页面视觉仍待验；外部UIA、实体IME/键盘、物理DPI未验，真实存档恢复/媒体删除/云端写入未执行。

U07-05更新为代码完成待验收；U09-01仍需其它视觉批次同数据双主题配对，U09-04仍缺U00逐帧/累计分配基线。当前计数6项已满足、71项代码完成待验收、2项进行中、1项远端待验。临时产物归档已完成，清理盘点和删除尚待执行。

# U07-05 通知来源字段与列表精修

实现候选基于 `9086154134982dfd7c5a4f0fb317d59a4285bfd2`。候选源码增加 `NotificationRecordDto.Source`，限长并规范空白；SQLite通过 `EnsureColumnAsync` 为旧表加 `source TEXT NOT NULL DEFAULT ''`，不从旧摘要或标识推断来源。任务来源使用真实 `TaskTypeDisplay`，退出会话摘要使用显式事件来源“游戏会话”。通知列表在标题下以次级文字显示来源，详情面板也显示来源；旧记录显示“来源未记录”。标题排序和复制仍绑定 `Title`，长标题Tooltip/UIA、焦点、DataGrid共享样式、行列虚拟化及现有滚动系统保留。

## 双主题前后对照

RenderHarness使用固定样本 `2026-10-28T09:00:00Z`、逻辑DPI 1.00、窗口 `1040×700 DIP`，分别输出浅色与深色通知页。除新增的来源数据/展示外，标题、摘要、级别、游戏/任务ID和相对时间沿用同一夹具。完整Render QA递归产出394张PNG、`PROBLEM`计数为0，进程退出码0；报告见 [渲染前报告](render-qa-before-report.txt) 和 [渲染后报告](render-qa-after-report.txt)。报告SHA-256分别为 `A0C5743335619B480856D9687953A67385B2A5458A300CED9BCDCE843255F328`、`FD755E94EE8D023B24D6C59BF1BA53E51C98A495E9B3065E1A3BE33C3E6A9A51`。同样本浅/深主题1040×700前后图和SHA见[对照清单](render-pair.json)。

浅色与深色对照图均显示第二行“来源”，标题和摘要仍可读；行高、列宽、底部滚动条和操作区未改。截图仅代表离屏Harness逻辑DPI，不等同真实Playnite宿主、物理DPI或外部UIA验收。当前完整报告采集于源码冻结前，报告中的HEAD为基准提交且 `WorkingTreeClean=False`；它没有被描述为候选提交的最终根链证据。

## 局部编译与行为检查

SDK `8.0.423`、Release、`--no-restore --disable-build-servers -m:1`：Worker通知持久化13/13，Playnite通知聚合/视图行为7/7，均0失败、0 skip。覆盖来源重启持久化、旧表加列迁移、空来源不覆盖已有来源、旧数据的“来源未记录”、标题排序/复制、Tooltip/UIA及新通知不抢搜索焦点。两份TRX已脱敏归档于 `tests/`，脱敏与原始SHA见[验证清单](validation.json)。Playnite项目输出的 `NU1900` 仅表示本机不可访问NuGet漏洞数据源；后续冻结候选完整构建仍需验证。

`python scripts/validate-source.py`退出0；`scripts/check-xaml.ps1`为26个XAML文件通过；WPF静态审查扫描42个XAML、0 errors、26既有warnings、179 info，未报告 `NotificationInboxView.xaml` 问题，完整结果见[wpf-static.txt](wpf-static.txt)。

本记录当前对应冻结前局部候选；中文根入口完整构建/测试/发布/打包/安装身份/启动、独立 `scripts/build.cmd`、候选精确SHA GitHub双SDK CI、artifact身份及最终清理在冻结提交后补记。真实Playnite刷新、外部UIA/Narrator、实体键盘/IME、物理DPI与真实存档恢复/媒体删除/云端写入不在本批视觉验证范围。
