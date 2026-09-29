# 紧凑窗口行距、标题和动作对齐

日期：2026-09-30。生产代码提交：`dc7f97cfa49724778c4987224c9f736c744b3631`（`优化窄窗按钮间距和标题排布`），已推送到 `origin/main`。

## 修复

- 紧凑换行的存档历史动作、媒体预设、媒体待归类批量/次级动作、任务筛选预设和外壳动作间距由 `8 DIP` 提至 `12 DIP`；各组只在实际紧凑/换行区间启用，宽态恢复原 margin。Task Center 的筛选组仍作为完整标签+控件组换行。
- 外壳在紧凑断点 `<1280 DIP` 改为一列：页面标题占第 0 行，全局游戏选择与操作占第 1 行，两者横跨可用宽度；标题/操作间距 `8 DIP`，副标题隐藏，完整页面摘要继续在 UI Automation HelpText 中提供。`1280 DIP` 及以上恢复原标题与操作同一行的宽态布局。
- 顶部两个主备份按钮的图标改用与文字相同的 `GscOnAccentTextBrush`。实际模板正常/悬停/焦点/按下渐变状态的最小对比度为 Light `5.09:1`、Dark `6.96:1`；两条真实命令绑定、执行和 Automation 名称均在测试中保留。
- Task 队列已有短提示、辅助说明状态与固定按钮高度在本批复核中继续通过：摘要 `58 DIP`，重试/重置按钮 `30/36 DIP`，中心差 `0/0.33 DIP`，隐藏摘要不改变按钮高度；文字使用主题次级色。传输明细标题/计数胶囊与胶囊文字中心差均为 `0.33 DIP`。

## 当前提交验证

| 验证 | 结果 |
| --- | --- |
| Release solution / XAML | `0 warning / 0 error`；XAML `24/24` |
| 源码验证 | `python scripts/validate-source.py` 通过 |
| WPF 静态检查 | `0 errors / 30 warnings / 177 info`；没有静态错误 |
| 紧凑布局/任务按钮/计数胶囊/顶部图标行为 | `ReportedWorkspaceLayoutBehaviorTests` `12/12`、Q14 Task 筛选 `1/1` |
| R00-04 搜索基准 | `2/2`；2000 项合成数据、30 次查询、结果集变化 `30/30`，p50/p95/max `46/48/48 ms`。原始样本见 [`large-library.txt`](large-library.txt) |
| R00-06 Media Inbox 几何/锚点 | `13/13` |
| R00-08 选框键盘/焦点 | `33/33` |
| R01-03 证据审计源码契约 | `6/6`；E01–E20 validator `20/20` |
| R01-05 负例注册表 | `1/1`；N01–N05 检测器均捕获预期负例 |
| 完整 RenderHarness | `render-qa OK`；`WorkingTreeClean=True`，源码身份与上述提交相同，Light/Dark，全矩阵；报告为离屏逻辑 DIP `1.00` |

上表各组为独立测试进程，不是一个合并测试总数。定向用例 TRX 在 [`trx/`](trx/)；当前 RenderHarness 报告为 [`render-qa-report.txt`](render-qa-report.txt)。紧凑标题区在 `720/960/980/1040/1200/1279 DIP` 使用两行标题结构，`1280/1366 DIP` 恢复宽态单行结构；实际图见 [720 DIP](../R01-06-controlled-audit-20260930/screenshots/Shell-720x640.png)、[1040 DIP](../R01-06-controlled-audit-20260930/screenshots/Shell-1040x700.png)、[存档历史](../R01-06-controlled-audit-20260930/screenshots/Save-1040x700-tab0.png)、[媒体待归类](../R01-06-controlled-audit-20260930/screenshots/Media-1040x700-tab0.png)、[任务中心](../R01-06-controlled-audit-20260930/screenshots/Task-1040x700.png)。R00/R01 的完整 freshness 输出为 [`ui-evidence-freshness.json`](ui-evidence-freshness.json)。

## 构建身份和证据边界

- 插件 DLL：`src/GameSaveCenter.Playnite/bin/Release/net462/GameSaveCenter.Playnite.dll`，SHA-256 `854E1549BA35626BBBFD19C82F9596CC5B6AC4320A72D28941CEDBD919BC9EC5`，MVID `3ad2d781-4904-4ff1-bfc2-5cf394ffdafb`。该 DLL 的 `ProductVersion` 是 `0.6.73+unknown`，所以以 DLL SHA/MVID 和 RenderHarness 完整源码提交身份识别，不把插件版本字符串当作提交证明。
- 测试 DLL：`tests/GameSaveCenter.Playnite.Tests/bin/Release/net472/GameSaveCenter.Playnite.Tests.dll`，`ProductVersion=0.6.73+dc7f97cfa49724778c4987224c9f736c744b3631`，SHA-256 `1E5A4D29C33327BDBEB3A340986CC04551D602DAC7B05937A8A3AAF07AB36745`，MVID `1ef3680f-fcc8-4eff-81e9-7ae091df7e9f`。
- 颜色采用“主题 Accent 表达交互/重点，语义色只表达真实状态，颜色不单独传递状态”的原则；Microsoft Windows 设计资料也建议克制使用强调色并匹配主题，Windows 辅助功能指南要求避免仅靠颜色区分、关注对比度和 DPI。[Windows Color](https://learn.microsoft.com/en-us/windows/apps/design/signature-experiences/color)、[Designing inclusive software](https://learn.microsoft.com/en-us/windows/apps/design/accessibility/designing-inclusive-software)。本批实际对比度是上述两个主按钮图标/标签，不代表全产品所有颜色均完成审计。
- 本轮没有执行 Playnite 安装或启动，没有测试用户安装 DLL、真实宿主主题、物理显示器 125%/150% DPI、OS IME、presented frame、ETW 或物理跨屏。WPF testhost/RenderHarness 的窗口和 DPI 设定不等价于物理显示器。
- `MediaInboxGrid` 滚动后列头/数据行偏移仍未解决；本批的几何/锚点回归不代替用户所要求的同一安全 Playnite 进程滚动前后 `[GSC-GRID-DIAGNOSTIC]`。R01-06 的当前受控审计与完整未验边界见 [审计归档](../R01-06-controlled-audit-20260930/README.md)。
- `r00-06-media-geometry-postcommit.trx` 在 xUnit `13/13` 完成后还记录 `2` 条 WPF `TextServicesHost.OnUnregisterTextStore` `InvalidComObjectException`；`q14-filter-spacing-postcommit.trx` 在 xUnit `1/1` 完成后记录 `18` 条同类清理输出。两项测试均 exit `0`、失败/跳过为 `0`；清理异常根因未知，本证据不将其写成产品问题已定位或已修复。
- 当前 Demo 原目录 `GameSaveCenter.AcrylicFork/src/GameSaveCenter.Playnite/Design/DesignShellView.xaml` 不在本 checkout；本批以恢复的生产页面基线和用户截图校准，没有引入新的视觉体系。
- 192 项任务总数与状态未变。下一项继续检查其他真实可见的紧凑 WrapPanel 与说明密度；Media Inbox 滚动的宿主诊断继续等待安全同进程条件。
