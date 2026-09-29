# Q14-02 Task Center 筛选标签与紧凑换行

日期：2026-09-29。复用生产 `TaskCenterView`、现有筛选 DTO/选项集合和共享主题资源；未改变筛选业务或命令。

## 改动

- 状态、类型、范围、时间和游戏标签统一用全角冒号；标签与对应 ComboBox 放在同一水平组内，间隔 `4 DIP`、垂直中心对齐，UI Automation 名称和 TwoWay 绑定保留。
- 窄布局把类型/范围/时间整组移入 `WrapPanel`；换行时整组不拆开。组的底部 `8 DIP` 间隔只在紧凑区启用，移回主筛选行时复位为 `0 DIP`，避免把主行按钮和输入框撑高。
- WPF 在视图重排期间可能短暂丢失继承的 DataContext，并把 TwoWay 选择写回首项。重排前保存四个筛选选择、重排后恢复；测试实际更改非默认选择，再往返两个布局断点验证选择仍在。

## 构建与行为验证

- 测试构建用隔离输出 `.tmp/q14-02-labels-038640e3`，Release solution build `0 warning / 0 error`，目标含 Playnite `net462` 与测试 `net472`。构建时 `GscBuildCommit=038640e39a35c566593c4b7d7424d77aa027f85c`、`GscSourceRoot` 为本仓库；工作树包含本阶段源/测试改动，尚未提交。
- `scripts/check-xaml.ps1`：`24/24`；`python scripts/validate-source.py`：通过；`validate_wpf_ui.py`：`0 error / 30 warning / 177 info`，告警为仓库扫描发现，未新增由本批次引起的 error。
- 隔离 VSTest 按类分进程运行：`Q14FilterLabelBehaviorTests 1/1`（一条 Fact 覆盖 Light/Dark × `1280×720`、`980×700`、`979×700`、`760×640`、`620×640 DIP` 共 10 个场景）；`Q14ToolbarAlignmentBehaviorTests 1/1`；`TaskCenterViewResponsiveTests 8/8`；`R03CopySafePunctuationTests 3/3`。合计 `13/13`，全部 exit `0`。
- 新用例测量同组标签/控件 `4 DIP` 间隔、垂直中心、父容器、绑定和非默认选择；`620 DIP` 窄窗实际验证筛选组跨行，行间净距至少 `7.25 DIP`。Toolbar 回归再次确认主筛选行各控件保持 `36 DIP`，没有被紧凑组间距改变高度。
- WPF testhost 实际 `DpiScale=1.5×1.5`。xUnit/TRX 均成功，但退出时出现 WPF TextServices `InvalidComObjectException` 清理输出，根因未知。

## 离屏页面门禁

- `scripts/render-qa.ps1 -Configuration Release` 完成，`render-qa OK`，报告中 `PROBLEM` 为 0。报告是 `OffscreenRenderHarness`、逻辑 DPI `1.00`；记录窗口矩阵、主题、任务表视口等几何，不代表 Playnite 实际宿主或物理 DPI。完整报告见 [`render-qa-report.txt`](render-qa-report.txt)，SHA-256：`A8B266D5092DB5ECFD86A95F3F2881F37E425527F44C34C0304D98B992ABC9BE`。
- 保留四份原始 TRX：[`Q14FilterLabelBehaviorTests.trx`](Q14FilterLabelBehaviorTests.trx)、[`Q14ToolbarAlignmentBehaviorTests.trx`](Q14ToolbarAlignmentBehaviorTests.trx)、[`TaskCenterViewResponsiveTests.trx`](TaskCenterViewResponsiveTests.trx)、[`R03CopySafePunctuationTests.trx`](R03CopySafePunctuationTests.trx)。测试 DLL SHA-256：`6B3DCCA26CE1B15FD2D09A20D4B8D03381D1EF16B4F30C14E4ECC2CD296CB317`；Playnite 插件 DLL SHA-256：`0F26EA8284C5794E624855C9B5AC84AE02D39EA96591948494D5AAABE533B419`。

## 验收边界与下一步

- 本批只完成 Q14-02 的可控行为和离屏门禁。没有启动真实 Playnite，没有物理 125%/150% DPI、OS 输入或最终呈现帧证据；Round2 账本仍保留宿主外部阻塞/未完成。
- 用户报告的跨页面紧凑布局补充问题已在[独立补充批次](../USER-REPORTED-COMPACT-LAYOUT-20260929/README.md)完成受控修复和行为/离屏证据；完整隔离 Release 测试通过，package/install 因未提交改动命中 dirty guard 而待提交后完成。不因此签收 Round2 Q14-02。下一阶段按最新目标先核对 R00/R01 freshness，再继续依赖满足的 Q/R；Media Inbox 用户滚动仍等待安全宿主同进程 `[GSC-GRID-DIAGNOSTIC]`。
