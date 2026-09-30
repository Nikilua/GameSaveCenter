# Trainer 与媒体操作行折行间距（2026-09-30）

## 本批改动

- Trainer 已安装工具检查器固定宽度，五个操作按钮实际会折成两行；现在通过共享 `WrapPanelRowGapController` 保持 `20 DIP` 净行距。切到紧凑抽屉后按 `520 DIP` 以下启用行距；宽度足以单行时还原每个按钮的原始下边距。
- 媒体中心“当前游戏媒体”批量动作在 620–680 DIP 实测为两行，增加 `20 DIP` 行距；700 DIP 起实测单行，恢复 XAML 原始边距。命令、游戏选择、媒体集合和滚动逻辑均未改。

## 验证结果

- Release solution `0 warnings / 0 errors`、XAML 检查 `24/24`；`python scripts/validate-source.py` 通过。
- [WPF 行为 TRX](../SDK10-COMPILE-IDENTIFIER-FIX-20260930/relevant-behavior-tests.trx) 总计 `7/7 passed`。两项几何测试分别在 Light/Dark 下执行：
  - Trainer：桌面检查器 `2` 行、净间距 `20 DIP`；620 DIP 紧凑抽屉为 `1` 行，原始可见子项下边距恢复为 `[8,8,8,0,8]`。
  - Media：宿主宽度 620/640/660/680 DIP 时，动作区分别为 560/580/600/620 DIP、`2` 行、间距 `20 DIP`；700 DIP（动作区 640 DIP）与更宽采样为 `1` 行并恢复边距。
- RenderHarness Light/Dark 多尺寸矩阵通过，报告为 `render-qa OK`。该运行使用离屏逻辑 DPI `1.00`；它不代表 Playnite 实际宿主、物理 125%/150% DPI 或最终屏幕帧。

## 源码输入

行为测试运行时 Git 基线为 `065b9b4b0febb2ba460eb4555db9bf6e42888e28`，以下改动源文件 SHA-256 对应实际被构建的工作树输入：

| 文件 | SHA-256 |
|---|---|
| `src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml.cs` | `704E7301967C209A649D7286F2B662FE6AEDA1210AFC15282F5D14284A223666` |
| `src/GameSaveCenter.Playnite/Views/TrainerCenterView.xaml` | `9AD6B82E97B6DFF86E3E5634B971D1B7CBB06614AFAAE1441227663EF3516440` |
| `src/GameSaveCenter.Playnite/Views/TrainerCenterView.xaml.cs` | `C9615A81E53A18ECEB0328AF6AEBC4E5747FACB5AF6D845ABDF77DAA84FAA98E` |
| `tests/GameSaveCenter.Playnite.Tests/ReportedWorkspaceLayoutBehaviorTests.cs` | `3553390ED8B95A39000077CF1204E59298D93EF894FD9BE4C4DD4EED5518456C` |

测试以合成 DTO、隔离 STA WPF 窗口和逻辑 DIP 测量生产 View；没有启动 Playnite，也没有访问真实媒体或存档。当前 Media Inbox 在真实 Playnite 中滚动后的列头/首行偏移仍需同一安全宿主进程采集 `[GSC-GRID-DIAGNOSTIC]`，本批证据不替代该诊断。
