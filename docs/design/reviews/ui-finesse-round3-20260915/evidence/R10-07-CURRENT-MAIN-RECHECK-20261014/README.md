# R10-07 侧栏密度当前 main 复核

日期：2026-10-14；分支 `main`；构建源码身份 `e0a72c93774610110100d7ead0ab59108689d1eb`。本次复核未修改 Shell 侧栏实现。

## 当前行为

- `ProductionShellChromeSourceTests` 在当前 Release 测试程序集上 `22/22` 通过，0 失败、0 跳过。
- 真实 STA WPF Window 覆盖折叠/展开后选中工作区保持、7 个导航入口的图标/Tooltip/Automation 名称/Tab 键入口、72 DIP 折叠宽度、270 DIP 展开宽度、长标签与主区可用空间，以及 reduced-motion 和动画时钟清理。
- 侧栏生产 XAML、代码和行为测试源 SHA-256 分别见下表。当前 UI 阶段只改动 Overview，不影响本组侧栏源。

## 本机验证身份

- SDK `9.0.302`、VSTest `17.14.1`；Release Playnite `net462`、测试 `net472` 与项目引用完成重建。仓库 `global.json` 的 SDK `8.0.423` 本机未安装，SDK9 结果不能替代 SDK8 或 CI SDK `10.0.401`。
- 首次直接测试因缺少 `GSC_BUILD_COMMIT` 元数据，有 7 个源码测试拒绝读取身份不明的程序集；设置 `GSC_BUILD_COMMIT`/`GSC_SOURCE_ROOT` 并按仓库约定重建后，下面归档的 `22/22` 是有效结果。首次试验未计入通过数。
- 脱敏 TRX：[ProductionShellChromeSourceTests 当前 main](tests/ProductionShellChromeSourceTests-current-main.trx)，SHA-256 `7D12DEA30FF10807274781598CC4C57A894B0912C0166188023AD75537EFC8D9`。

| 文件 | SHA-256 | MVID / 版本 |
| --- | --- | --- |
| `AcrylicProductionShellView.xaml` | `B9E7A777B84F4AC3DD08FEF69643D7F64D7632466F3DC0271239F8792B756089` | — |
| `AcrylicProductionShellView.xaml.cs` | `736C1554D06C8B67E753F68FBFB0E2F63935F007414BA9F45BB1E29D35AE39C7` | — |
| `ProductionShellChromeSourceTests.cs` | `33879158CDE26DDB40451D5751DC55DCF93064CDD86BE1C062B47D1721B7CB06` | — |
| `GameSaveCenter.Playnite.dll` | `D42A07426F001B5FE20F6168B00E6C51DCD1AF5838AC158CDB91B92E15FE61A4` | `40cdb240-f202-47c8-a438-60820e0e78c3`; ProductVersion `0.6.73+e0a72c93774610110100d7ead0ab59108689d1eb` |
| `GameSaveCenter.Playnite.Tests.dll` | `39DD3668B86A3782AA6C76DF01305D45108F39FDAE5D6AEAC685123034DFAF4B` | `eeee7482-78a5-4357-93b5-2ad1f85b1057`; same ProductVersion |

## 边界与后续

验证使用隔离 STA WPF 和合成状态；未启动 Playnite/package-host，未验证物理 DPI/跨屏、最终呈现帧、外部 UIA/读屏、真实键盘/IME、ETW 或宿主性能。Demo 原目录在本机不可用，继续以恢复后的生产基线为依据。

下一项：R10-08 最近访问可达入口当前 main 修复与验证。
