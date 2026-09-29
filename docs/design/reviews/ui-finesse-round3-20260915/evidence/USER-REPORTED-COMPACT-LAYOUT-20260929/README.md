# 用户报告的紧凑窗口布局与信息层级修复

日期：2026-09-29。此为用户截图触发的补充修复批次；它与 Q14 紧凑工具栏工作相邻，但不替代 Q14 或 R03 表格条件，也不改变 Round2 外部验证状态或 Round3 R 台账计数。

## 改动

- 新增 `WrapPanelRowGapController`，只在紧凑宽度给生产动作 WrapPanel 行增加 `8 DIP` 底间隔；返回宽布局时恢复每个子项原有 Margin。用于存档历史动作、媒体待归类批量动作和生产外壳紧凑顶部动作。媒体筛选仍绑定全局选中游戏，没有改选框、滚动或命令系统。
- 外壳窄窗将页面标题与全局游戏选择/动作留在同一标题行，隐藏可见副标题；副标题完整值同步保留在标题 Automation HelpText。宽窗继续显示副标题。
- Task Center 的离页说明缩为“后台任务会继续运行。”，只在有运行任务时显示，完整的不取消/恢复语义保留在 Automation HelpText；动态筛选摘要改用浅/深主题的次级文字令牌。任务队列摘要限制宽度，动作按钮垂直居中且不随摘要高度拉伸。
- 云端传输明细计数 pill 的文本设置垂直居中。
- 更新 RenderHarness 的外壳门禁，使其验证紧凑标题与动作处在同一 Grid 行、第二动作行收起、行内容不裁切，且以二维矩形判定覆盖；旧断言假设 compact 时动作另占一行，已不符合新产品要求。

## 行为与主题证据

- Release Playnite 和测试项目构建：`0 warnings / 0 errors`。`scripts/check-xaml.ps1` 为 `24/24`，`python scripts/validate-source.py` 通过。
- `ReportedWorkspaceLayoutBehaviorTests` 定向方法 `8/8`、0 failed/skipped：浅/深主题都验证存档和媒体窄窗实际两行净距 `8 DIP`、宽窗回一行且恢复作者 Margin；标题/动作同一行且副标题仅在紧凑布局隐藏；任务队列三行摘要高 `58 DIP` 时重试/重置按钮高 `30/36 DIP`，摘要隐藏前后按钮高度不变，中心差 `≤0.33 DIP`；传输计数文字相对 pill 和标题的中心差 `0.33 DIP`。真实 WPF 窗口几何与逐项输出见 [TRX](ReportedWorkspaceLayoutBehaviorTests.trx)。
- `R22LongTaskLeavePageBehaviorTests` `2/2`：有运行任务时短文案可见、完整语义在 HelpText；运行数变为零时说明折叠，后台任务不因视图卸载被取消。见 [TRX](R22LongTaskLeavePageBehaviorTests.trx)。
- 旧的 `WpfUiResourceDictionaryTests.CompactLayoutsKeepSummaryInformationAndUseThePageScroller` 因 `LegacyProductionUiBaselineFact` 标记跳过；这是已撤销页面基线的源测试。交互/几何验收由上述实际 WPF 测量承担。跳过事实见 [TRX](WpfUiResourceDictionaryTests-skipped-legacy-baseline.trx)。
- `validate_wpf_ui.py`：`0 errors / 30 warnings / 177 info`；告警数与该仓库此前扫描一致，本批无新增 error。

## RenderHarness

- 第一次运行通过页面绘制，但旧外壳断言按“紧凑动作必须在第二行”和仅比较纵坐标产生 `PROBLEM`。调整 RenderHarness 的断言后重新执行全套页面/窗口/主题矩阵，最终 `render-qa OK`、`PROBLEM=0`。完整 [报告](render-qa-report.txt)，SHA-256：`D73FFA423255E1F3EB1124ECF2D5A9C023D9E8D8EB9A986496F6147EBEEA82D0`。
- 选取的离屏页面图： [外壳 980×640](Shell-980x640.png)、[存档中心 1040×700](Save-1040x700-tab0.png)、[媒体中心 1040×700](Media-1040x700-tab0.png)、[任务中心 1040×700](Task-1040x700.png)、[维护中心传输页 1040×700](Maintenance-1040x700-tab1.png)。图像来自 `OffscreenRenderHarness`，报告记录逻辑 DPI `1.00`。它们用于源码视觉复核，不代表 Playnite 最终屏幕呈现或物理 DPI。

## 当前门禁与限制

- 本批的 STA WPF 窗口/RenderHarness 使用合成数据和现有真实视图/共享资源，没有调用真实服务、写存档、删媒体、访问云端或发送诊断。
- 用户真实 Playnite 窗口、插件加载 DLL、OS 输入、125%/150% 物理 DPI、跨屏和最终呈现帧未验证；不可将离屏结果写成真实宿主修复确认。Media Inbox 滚动缺陷仍按其同进程 `[GSC-GRID-DIAGNOSTIC]` 取证要求保持打开。
- 颜色调整只把说明/筛选摘要从蓝色信息色改为主题语义次级文字；警告/成功/失败等状态仍保留语义令牌和文字/图标。WCAG 2.2 要求信息不能只依赖颜色，非文字控件/图形需满足对比度条件，见 [Use of Color](https://www.w3.org/WAI/WCAG22/Understanding/use-of-color) 与 [Non-text Contrast](https://www.w3.org/WAI/WCAG22/understanding/non-text-contrast.html)。本批测试确认浅/深主题令牌映射，没有声称测完全应用对比度。

## 完整 Release 流程结果

- `scripts/dev-install-run.ps1` 使用独立 `.tmp/q14-compact-install/` 构建/测试目录与隔离扩展目标，XAML `24/24`、Release solution build `0 warning / 0 error`、Core `125/125`、Worker `356 passed / 1 existing skip / 0 failed`、Playnite source `111` 类与 WPF 隔离 `113/113` 类全部通过；本批 `ReportedWorkspaceLayoutBehaviorTests` 与 `R22LongTaskLeavePageBehaviorTests` 也在全套隔离中通过。日志末尾确认所有 Playnite 测试通过。
- 首次 package/install 阶段因本批尚未提交的源码和文档而命中 dirty-tree 安全门禁；该门禁防止用旧 HEAD 冒充新源，不是构建或测试失败。
- 先前新增的 `tests/GameSaveCenter.Playnite.Tests/TestResults/q14-*.trx` 已将所需 TRX 归档到本目录；源码工作树的临时副本完成本阶段记录后清理。

### clean commit package/install 复核

- 提交 `fc58264e6776b7e021c65e6a8fc11e2eb1032746` 后，从 clean `main` 重跑 `scripts/package.ps1 -Configuration Release -BuildOutputRoot .tmp/q14-compact-install/build -SkipPackageArchives`。XAML `24/24`、solution `0 warning / 0 error`、Core `125/125`、Worker `356/357`（1 个现有硬进程重启 skip）、Playnite source `111` 类及 WPF 隔离 `113/113` 类通过。package 身份校验确认插件、Worker、Core、Contracts 六个程序集的 InformationalVersion 均为 `0.6.73+fc58264e6776b7e021c65e6a8fc11e2eb1032746`。
- `scripts/install-dev.ps1` 仅将版本化暂存包复制至仓库 `.tmp/q14-compact-install/Extensions/GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec`。安装清单 `0.6.73`，DLL FileVersion `0.6.73.0`；插件 DLL SHA-256 `64BBA6B451C287544AA65A7E17BC380E0BFBFEA72F17B1F1EE0FFF76D71F5D57`、MVID `6f7b0c1e-610d-4590-993b-0d9e28c360d8`；Worker DLL SHA-256 `3CBEBD86A353060AC3BA57EED21DE43F69A8178D43A9E5C3D11B16295C006398`、MVID `22e37b66-7cc1-4796-84a9-461cbe991a53`。按隔离验证选项跳过 zip/pext 归档，版本化 artifacts 暂存目录为当前包。记录身份后清理了本批 `.tmp/q14-*` 构建/安装/离屏目录和本批本地安装日志；仅保留当前版本化 artifacts 暂存包。
- 没有启动 Playnite。此处仅验证了文件复制到仓库 `.tmp` 隔离扩展路径；真实宿主载入该 DLL、用户窗口、OS 输入和物理 DPI 仍未验证。

## 后续

当前 main 的紧凑窗口补充修复已提交并推送。下一阶段先按当前 `main` 重跑 R00/R01 freshness，仅对命中本批修改路径的证据重测或重签“已满足”；再继续依赖已满足的 Q/R 项（包括 Q14-03 搜索宽度）。Q14-01/02/本批并不自动签收其 Round2 项目。
