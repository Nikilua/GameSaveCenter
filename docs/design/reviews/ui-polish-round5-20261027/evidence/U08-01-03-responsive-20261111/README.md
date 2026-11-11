# U08-01/02/03 响应式文字边界复核（2026-11-11）

## 改动

- U08-01：概览页不再显示重复的“今日工作台”标题或加载提示。刷新、全部备份、媒体同步三个真实命令仍保留；概览状态卡仍负责显示当前状态和下一步。页眉副标题改为“状态与最近任务”。未恢复已撤销的“今日工作台”页面架构。
- U08-02：存档检查器及内部滚动内容允许在并排布局中收缩；恢复可用性长文明确换行，横向滚动继续关闭，纵向滚动仍可访问完整安全说明。恢复、保护和危险操作语义未改。
- U08-03：任务详情检查器的内容树允许收缩，长任务/游戏标题和错误码可换行并留在圆角检查器范围内。重试、取消、失败信息和技术详情命令/绑定保留。

## 源码和验证

- 源码基线：`c2b59dd1a47e996fb493990cb781bbabc4d0ee12`；构建诊断记录工作树有改动，结果对应本批当时的源码内容。Overview 上一处仅注释文字调整在完整构建后发生，不改变 XAML 行为；本次又对最新文件运行了 Source Validation、WPF 静态审查和 `git diff --check`。代码变更及其最终提交身份见 `../../PROGRESS.md` 的本批条目。
- SDK 8.0.423，Release，真实仓库入口 `scripts/build.cmd`；完整独立 build/test exit 0，编译 0 warning、0 error。
- 160 份 TRX 共 2,035 项：1,967 passed、0 failed、0 error/timeout/abort、68 skipped；TRX 汇总计数一致，无解析错误。Core 157/157；Worker 400/401；Playnite source 548/575；WPF 私有桌面隔离 157/157 类执行完成。68个skip逐项原因为57条属于已撤销“今日工作台”架构的旧断言、9条本环境禁止Named Pipe客户端、1条本环境禁止Named Pipe/Worker隔离进程、1条默认关闭的系统鼠标指针测试；均保留原状态且不计通过。逐项原因在 `diagnostics/build-summary.json` 对应的TRX文件中。
- 新增边界用例在 `CompactWorkspaceTableViewportBehaviorTests` 17/17 通过，覆盖浅/深主题下 1040 DIP 并排存档检查器的长中文恢复说明、检查器内边界及纵向可达性，以及长错误码在任务检查器内换行。`ReportedWorkspaceLayoutBehaviorTests` 56/56 通过，其中概览命令、无重复加载提示和加载前后工具栏高度在双主题验证。
- `scripts/validate-source.py` exit 0；WPF UI 静态审查 exit 0，32 个 XAML 文件、0 errors、26 warnings、179 info。两条 Overview 滚动布局警告位于本批未改的第 501/809 行；未关闭门禁或改写警告。`git diff --check` exit 0。
- Render QA exit 0：334 PNG、0 `PROBLEM`，固定输入时间 `2026-11-11T00:00:00Z`；合成窗口覆盖双主题和 1040–3840 DIP 范围。Harness 使用 `WinSta0\GscRenderQA_*`，输入桌面仍为 `Default`。四张代表图保存在 `screenshots/`，完整报告保存在 `diagnostics/render-qa-report.txt`。
- 首次完整 build 在测试程序集编译时发现新夹具误把 `SaveCenterView` 传给主题资源方法，编译器报错；已修正为传 `view.Resources`，未删断言或扩大 skip，并在修正后重跑完整 build/test。首次失败摘要和编译输出保留在 `diagnostics/initial-fixture-compile-failure-*`。

## 冻结提交安装与CI续验

U08源码提交 `18158cf2c159648b56099144fb9a3fb78c08280e` 已通过精确SHA的GitHub run #1535。冻结跟踪文件提交 `266bae3396b9b4d08cdc1978bfc82c7e7cd776c5` 后运行中文→英文正式根入口完整链，第一次因当前沙箱拒绝写入Playnite AppData而exit 1；该尝试的日志和完整TRX已保存于 [拒绝诊断](../r5-root-fail-20261111/root-chain-failure.json)。受控权限重跑后最终exit 0，安装身份、包哈希、SDK、160份TRX和启动进程见[根入口成功复核](../r5-root-ok-20261111/README.md)。CI run #1536对文档提交精确SHA的双SDK作业均成功，见[CI身份记录](ci-run-42967026905.json)。

根入口确认安装并启动Playnite进程，但取样时 `MainWindowHandle=0`，没有检测到该PID的顶层窗口。因此离屏 Render QA 与隔离行为测试不等于真实Playnite宿主像素、用户桌面字体裁切、外部UIA/Narrator、实体键盘/IME或物理DPI验收；U08-01/02/03仍为“代码完成待验收”。没有执行存档恢复、媒体删除或云端写入。

证据文件 SHA-256 清单见 `sha256-manifest.json`。
