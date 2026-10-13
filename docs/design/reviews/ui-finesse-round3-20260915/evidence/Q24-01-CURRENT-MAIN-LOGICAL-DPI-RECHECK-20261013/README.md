# Q24-01 当前 main 分数 DPI 受控复核

日期：2026-10-14。代码身份：`3ca41ec9a5a3f3c6888db72ba3a6254b046dcd40`（main）。本包只记录受控 WPF 离屏目标 DPI 检查；物理显示器、真实 Playnite 与最终呈现帧仍待验。

## 检查范围

- 新增 `Q24FractionalDpiTypographyBehaviorTests`，使用生产 `DesignTokens.xaml`、`WpfUiProduction.xaml`、`Redesign.xaml`、`GscIconPack.xaml`、`GscBodyStyle`、`ThemeAwareIcon` 和校验图标 Geometry；没有修改生产视觉或业务代码。
- 在 `320×68 DIP` 的真实 WPF Visual 中放置中英文字段、`16×16 DIP` 图标和 `1 DIP` 边框。`RenderTargetBitmap` 分别以 96/120/144/168/192 DPI 输出，尺寸为 320×68、400×85、480×102、560×119、640×136 像素。此路径按目标 DPI 原生栅格化，没有先生成一张位图再缩放。
- 断言文字无 Wrap/Trimming、所需宽度落在实际布局宽度内，文字/图标都处在视口内；五档均有文字、图标和顶边描边像素。另以裁切文字及 1.25 倍 RenderTransform 负例确认门禁会拒绝这两种状态。
- 本机人工查看五张 PNG，五档均能看到中文/英文、生产图标和边框，没有明显裁字或图标消失。

## 本机验证身份

- Windows 11 家庭中文版 `10.0.22000`；隔离安装 SDK `8.0.423`；VSTest `17.11.1`，测试适配器 `3.1.4`，测试宿主 `.NET Framework 4.8.9181.0`。项目目标仍是 Playnite `net462`、测试 `net472`。
- 系统 DPI API 返回 `144`（150%），当前枚举到的物理显示器为 `\\.\DISPLAY1`、2560×1440。受控 Visual 也报告继承源 DPI `1.5`；这里不将其伪称为五台物理 DPI 显示器。
- 精确代码提交 `3ca41ec9a5a3f3c6888db72ba3a6254b046dcd40` 下，Release 定向测试 `ProductionTypographyIconAndOneDipBorderRenderAtFiveExplicitTargetDpis`：`1 passed / 0 failed / 0 skipped`。TRX：[`Q24-01-fractional-dpi-3ca41ec9.trx`](tests/Q24-01-fractional-dpi-3ca41ec9.trx)，SHA-256 `6AE269D7DD8768DAB58920C0CC82E8636C868FD760AD02D4909639372BACF207`。
- 生产插件 DLL SHA-256 `288C0E39C712BAEEB6C7C7FC48DA398B21CECBA8E9D0C8EB6175E5907E1A8A4C`；测试 DLL SHA-256 `3544E257B8A98D7E54BAF0B27E005228C37D29DFFED3507BE2509956AA62E61A`。

| 显式目标比例 | RenderTargetBitmap DPI | 像素 | 文字像素 | 图标像素 | 1 DIP 边线像素 |
| --- | ---: | ---: | ---: | ---: | ---: |
| 100% | 96 | 320×68 | 684 | 41 | 280 |
| 125% | 120 | 400×85 | 1008 | 65 | 700 |
| 150% | 144 | 480×102 | 1463 | 104 | 840 |
| 175% | 168 | 560×119 | 2120 | 128 | 980 |
| 200% | 192 | 640×136 | 2890 | 179 | 1120 |

## 图像与边界

- [100%](renders/q24-01-target-100pct.png) — 320×68，SHA-256 `08375C5EE89D4DF4A3907F2A3C7FB704C0A5936B85418CFEBFF0FD829D9B3AEF`
- [125%](renders/q24-01-target-125pct.png) — 400×85，SHA-256 `F9FE0705625D6802A0D1B4061E2E6D39ABBF45660573B17A9365F5033DDCE9BE`
- [150%](renders/q24-01-target-150pct.png) — 480×102，SHA-256 `2BE22FA8E1AF2DA14E7D047B89F07BB8E03C09F114691D2318F65C40A32C0CAE`
- [175%](renders/q24-01-target-175pct.png) — 560×119，SHA-256 `658E08A36849ED214FE12AD9B06D847B4863DBA47288FC76069C858990EC4019`
- [200%](renders/q24-01-target-200pct.png) — 640×136，SHA-256 `85E84C518EFBF3A45C00080DC6B4741DC4B9A44390E8612B31F60E8A472070EE`

该实验验证五个显式离屏目标 DPI 的 WPF 栅格化和生产资源行为。它没有移动真实窗口、改变操作系统显示设置或启动真实 Playnite，不覆盖桌面物理 DPI 切换、Popup/跨屏、操作系统文字放大、真实呈现帧、Playnite 主题/Chrome、UIA/IME/读屏或用户显卡路径。Q24-01 的受控视觉列可记“受控通过”，Host 仍“外部阻塞”，最终结论仍“未完成”；这些图片不能替代物理屏检查。

推送门禁：main 的 #972（[run 38421480496](https://github.com/Nikilua/GameSaveCenter/actions/runs/38421480496)，提交 `8546acab`）现已终态成功。SDK8 job `115379639786` 使用仓库 `global.json` 的 `8.0.423`，SDK10 job `115379639572` 安装/验证 `10.0.401`；两 job 均成功。#972 早于 Q24 代码提交 `3ca41ec9`，不能代替该代码的新 CI；本证据包推送后仍需分别跟踪 SDK8 与 SDK10 jobs 到终态。
