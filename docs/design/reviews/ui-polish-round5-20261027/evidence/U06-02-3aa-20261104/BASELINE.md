# U06-02 / U08-08：选中框完整包裹与候选 3aaae534

日期：2026-11-04。源码候选：3aaae5345012938f83ca8af6194cc13c56bfe5cd（main，已推送）。

## 基线盘点与去重

U00-01 已有存档、任务、媒体、设置四页各五项基线观察、双主题/双尺寸截图、SHA 清单及旧任务映射，见[已有盘点](../U00-01/BASELINE.md)。本批复核并沿用，不复制基线、不把用户附图外推成其它页面缺陷。AcrylicFork Demo 源当前仍缺失，生产 Render QA 不冒称 Demo 对照。

## 用户反馈与修复

附图显示三个位置的末尾文字落在选中框外：Overview 最近任务的日期时间、存档历史表格的末列、任务队列的状态标签。前一候选 b85f397c/28ff5e0f 加入行内容裁切时曾把 DataGrid 框宽参数从 8 改为 4；框线因此贴到 DataGridRowsPresenter 右边，挤掉应有的四 DIP 滚动轨道空隙。b85f397c 的 run #1449 两个 SDK job 均因 R23 轨道断言失败。失败诊断保留于同目录的 failures 证据和本地原始构建诊断中。

3aaae534 将共享生产 DataGrid 选中框宽度恢复为 8（焦点框 7），保留行内容向内裁切；Dashboard 兼容 DataGrid 同步恢复宽度。这样框线继续与滚动条留出轨道，时间和最后可见单元格内容留在框内。Overview 内容容器也保留 ClipToBounds。Save History 回归夹具固定为 740 DIP 左侧表格宽，重现附图的横向滚动视口；同时保留“必须横向可滚动”和“末列文字在框内”的断言。命令、Binding、UIA、键盘/IME、虚拟化、主题/DPI、业务错误/取消/安全语义、游戏选框及滚动条系统未移除。

代表图：[Overview 长日期选中行](screenshots/Overview-selected-long-time-3aaae534.png)。同批还保留 Overview、Save、Task 双主题/尺寸代表图及 334 PNG Render QA 报告。

## 验证结果

- Source Validation：exit 0。
- WPF 静态审查：88 个 XAML、0 errors、78 warnings、507 info；warnings 保持原值，未放宽门禁。
- 精确 SHA Render QA：334 PNG、0 PROBLEM；offscreen logical DPI 1.00，不能推断物理 DPI 或真实宿主屏幕帧。报告中的 WorkingTreeClean 为 False，因为工作树存在与本批无关的未跟踪历史证据；跟踪源码在最终候选冻结后未改。
- 精确候选根测试：SDK 8.0.423 / MSBuild 17.11.48，8 个项目编译 0 warning / 0 error；160 TRX 共 2028 项，1970 passed、0 failed、58 skipped。Core 157/157、Worker 401/401、Playnite source 557/575、WPF 隔离 855/895；157 个 WPF 类隔离运行。58 条 skip 不计通过：57 条是已撤销“今日工作台”架构断言，1 条是默认不移动系统鼠标的 opt-in 测试。
- 中文实际入口 GameSaveCenter-一键构建安装运行.cmd 转交 GameSaveCenter-Run.cmd，精确候选全链最终 exit 0；完整成功日志、安装转录、摘要和 160 份 TRX 位于本目录。此前一次普通权限安装被用户 Roaming 扩展目录 Access denied；一次提权调用在构建前因空间预算不足退出。恢复系统 TEMP 到 C 盘后，只清理了本阶段已完成测试的隔离输出 4ac8b9e6（1015 files / 281,907,597 bytes），保留失败诊断、当前包/打包目录、最终构建和审计；清单记录空间前后。随后提权完整重跑成功，未跳过测试。
- 独立 scripts/build.cmd 依自身编译/测试职责 exit 0：同一 SHA、SDK，160 TRX 共 2028 项、1960 passed、0 failed、68 skipped。比完整入口多出的 10 项是该次非提权执行环境禁止 Named Pipe 客户端（9）和 Worker 子进程（1）；它们不计通过。独立入口不代替中文完整链。
- 包和安装：ZIP 与 PEXT 均 45,738,426 bytes，SHA-256 同为 62550D4029212FBE7F6F1D34B12380236D880883132E259BBACE7ACDD09A4AD3。安装清单 0.6.73、DLL 0.6.73.0，六个插件/Worker 程序集均为 0.6.73+3aaae5345012938f83ca8af6194cc13c56bfe5cd。Playnite 与 Worker 进程响应；extensions.log 记录插件加载并观察到 3 个 Playnite 游戏。未观察插件实际页面，所以视觉/UIA/实体输入/IME/物理 DPI 仍为“代码完成待验收”。未执行真实存档恢复、媒体删除或云端写入。
- GitHub 精确 push run [#1450](https://github.com/Nikilua/GameSaveCenter/actions/runs/41659230554)（41659230554）对该 SHA completed/success。workflow windows-build.yml 的基线 job 125997757253（15/15 steps）和 SDK 10.0.401 job 125997757096（8/8 steps）全部成功，含编译测试、Render QA、WPF 静态审查、打包与诊断上传。预览包及两份诊断 artifact ID、SHA-256、大小见[CI 身份记录](ci-run-41659230554.json)。required-check/ruleset 读取仍沿用先前 403；不重复请求，U09-06 记录“远端待验”，有权限维护者可只读执行 gh api repos/Nikilua/GameSaveCenter/branches/main/protection/required_status_checks。

## 证据归档路径修正

文档证据提交 967e07e1c2d2c530620fee4300e8566e02b318b4 触发 [GitHub run #1451](https://github.com/Nikilua/GameSaveCenter/actions/runs/41681642551)。两个 job（126076537301、126076536778）都在 checkout 时因证据目录中重复嵌套的 TRX 路径超出 Windows 文件名限制而失败；后续源码检查、构建、测试、Render QA、WPF 审查和打包步骤均被跳过，未产生 artifact。原始 job 日志保留在[失败诊断 JSON](ci-run-41681642551-failure.json)链接的 GitHub job 页面。

本次只缩短证据目录并把完整链、独立 build 与六个 focused TRX 分别展平至 `t/r`、`t/b`、`t/f`；TRX 文件原名、字节和 SHA-256 均校验保持不变，映射见 `diagnostics/local-gates-3aaae534.json`。源码候选仍为 3aaae534；这是文档证据路径兼容性修正，新提交的 CI 将重新核验。

## 文件索引与状态

- [完整本地构建/安装/CI 摘要及 TRX SHA 清单](diagnostics/local-gates-3aaae534.json)
- [中文入口最终成功日志](logs/rootchain-success.log)、[安装身份报告](logs/last-dev-install-sanitized.txt)
- [独立 scripts/build.cmd 日志](logs/scripts-build-success.log)
- [完整链最终 160 TRX](t/r/)；[独立构建 160 TRX](t/b/)；[六项 focused 回归 TRX](t/f/)
- [初次安装权限拒绝诊断](logs/rootchain-install-permission-failure.log)与两次构建前空间预检记录仍保留；未用跳过测试或关闭门禁取得绿色。
- [隔离输出清理预检](cleanup-preflight-rootchain-retry.json)和[清理结果](cleanup-rootchain-retry.json)。

本批处理 U06-02 与 U08-08。状态计数维持 7 项已满足、71 项代码完成待验收、U09-04 进行中、U09-06 远端待验。Playnite 实页和外部 UIA/实体输入/物理 DPI仍待验；按用户边界未做真实存档恢复、媒体删除或云端写入。