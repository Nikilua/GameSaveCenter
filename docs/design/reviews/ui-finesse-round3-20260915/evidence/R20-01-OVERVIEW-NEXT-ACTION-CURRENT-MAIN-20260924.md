# R20-01 概览下一步：当前 main 复核（2026-09-24）

## 结论

R20-01 在当前 main 已满足，真实 Playnite/package-host 仍待环境验证。复用现有 `OverviewPriorityResolver`、`DashboardViewModel` 命令映射、`GamePickerViewModel` 和 shell 请求事件，没有新增业务服务、DTO 或自动写入路径。本批新增一个真实 WPF 主按钮回归，覆盖“未匹配”和“可备份”两种入口。

## 现有行为与负例

- Resolver 将后台故障、首次环境准备、云队列关注、失败任务、空游戏库、未匹配游戏、可备份游戏、待归类媒体、其他提醒和健康状态按稳定顺序投影成唯一主动作；失败/空库/未匹配/可备份都有专门状态测试。
- 失败任务入口映射到既有 `OpenFailedTasksCommand`，设置“失败”筛选、切换任务工作区并加载任务页；空库动作复用刷新命令。它们没有在本轮主动启动真实 Worker 请求。
- 未匹配/可备份映射到现有 GamePicker 命令。新行为用例加载生产 `OverviewView`，通过 `ButtonBase.OnClick` 的 WPF 命令分发，进入生产 `OverviewPriorityActionCommand` 和 `OpenOverviewGamePicker`；断言筛选分别为“未匹配”/“可备份”、工作区回到 Overview、shell 请求事件触发一次，并确认 `BackupAllCommand` 调用数为零。
- 既有优先级负例覆盖未匹配优先于可备份、无关警告数变化不使主动作跳动；“可备份”筛选本地行为覆盖符合条件项和排除项。主按钮实际点击不只依赖 XAML 字符串断言。

新测试用未初始化的 `DashboardViewModel` 仅装配当前动作所需字段，避免触发 Playnite、Worker、文件和云端初始化；被执行的是生产状态解析、命令属性和路由方法。它不代替真实 Shell 覆盖层或 Playnite 宿主。

## 当前 main 验证

代码和测试身份为 `1a43f474`。完整 Release solution build 成功：XAML `24/24`、0 errors；保留两条既有 `MediaCenterView.xaml.cs:703 CS8602` 警告。`scripts/validate-source.py` 与 `git diff --check` 通过。

| 当前测试类 | 结果 | TRX |
| --- | ---: | --- |
| `OverviewPriorityResolverTests` | 15/15 | [resolver](R20-01-PRIORITY-RESOLVER-MAIN-1A43F474.trx) |
| `GamePickerViewModelTests` | 22/22 | [筛选行为](R20-01-GAME-PICKER-FILTERS-MAIN-1A43F474.trx) |
| `OverviewInteractionTests` | 4/4 | [概览按钮与路由](R20-01-HERO-ROUTE-MAIN-1A43F474.trx) |
| `GamePickerShellSourceTests` | 4/4 | [shell 选框入口](R20-01-GAME-PICKER-SHELL-MAIN-1A43F474.trx) |
| `ReportedWorkspaceLayoutBehaviorTests` | 8/8 | [四页布局](USER-REPORTED-LAYOUT-CURRENT-MAIN-RECHECK-20260924-1A43F474.trx) |

合计 `53 passed / 0 failed / 0 skipped`。五份 TRX 均无 WPF `InvalidComObjectException` 清理噪声。

## 设置窗口截图及其他用户报告

同一 Release 身份下的四页 STA WPF 窗口序列再次覆盖 Media Inbox、Task Center、Save Center 和 Settings，Light/Dark 共 `8/8`。Settings 在 `1880×1200 DIP` 宽态和 `1254×800 DIP` 窗口化假设下，图标/标题顶部差 `11.33 DIP`、水平间距 `12 DIP`，搜索框/标题左边差 `0 DIP`、搜索框与图标顶端间隔 `72.67 DIP`；搜索居中负例会右移 `287.33 DIP`。重置控件和路径控件均为 `36 DIP` 并保持中心线对齐。测试能证明当前源码在受控 WPF 布局下的关系，不代表截图实际加载了该 DLL。

先前读取的本机扩展目录 DLL 为 `0.6.73+7a4ba2a9`，早于 `3a1dadd8` 搜索锚点修正；没有进程模块记录把本次截图关联到该文件。可审阅候选包 [GameSaveCenter-0.6.73-main-c866c027.pext](../../../../../artifacts/current-main/GameSaveCenter-0.6.73-main-c866c027.pext) SHA-256 `17B5C51CA502C0C2F119DFCBF98BF720AC43C56F899CB6BC3A6C909923498CAA`，其生产源码与本次 main 相同，未安装到用户 profile。隔离 Playnite 主窗体仍受 CEF `platform_channel 0x5` 阻断，不重试、不绕过权限。

所有几何是逻辑 DIP 的隔离 STA WPF 结果。正常 Playnite 设置父容器、实际加载程序集、Windows 物理 DPI/跨屏、UIA/IME、最终呈现帧和宿主性能仍未验；因此用户截图中的实际窗口问题保持开放。未读写真实存档、媒体、云端或诊断数据；Demo 原目录不可用，继续沿用恢复生产基线。

下一可执行任务：`R20-02 指标统计范围`，先核对全库/当前游戏各数字的数据源、更新时间和未加载语义。
