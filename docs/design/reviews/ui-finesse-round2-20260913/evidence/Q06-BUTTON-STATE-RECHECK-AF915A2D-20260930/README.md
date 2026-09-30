# Q06-08 当前 main 双主题按钮状态复测

日期：2026-09-30。测试程序集构建身份：`0.6.73+af915a2d637dc4901e1c81e2e2cdd097c958a002`。

## 用户报告与当前复测

- 用户提供的 one-click 输出报告 `Q06ButtonStateSequenceBehaviorTests.ProductionButtonCapturesAndChecksFiveStatesInLightAndDark` 为 `1 failed / 0 passed`，但消息中没有失败断言、堆栈或加载程序集身份。当前 checkout 中不存在报告所指的 `artifacts/one-click-install.log`，因此不能据该摘要定位原因。
- 当前 `main` 精确 Release DLL 的同一测试通过两种入口复测：直接 VSTest `1/1`，以及 one-click 隔离命令使用的 `dotnet test --no-build --no-restore` 参数组 `1/1`；两者均 exit `0`、无 skip。当前复测没有重现用户报告，不表示其失败已修复。
- 生产 `GscWpfUiPrimaryButton` 的 Light/Dark × normal/hover/pressed/focus/disabled 共 10 状态均通过状态与反例检查；禁用假命令在框架 Click 后执行计数仍为 `0`。截图由隔离 STA WPF Window、合成 hover/Space/程序化焦点生成；这不是物理输入或真实 Playnite 宿主。

## 当前程序集身份

| 程序集 | 目标 | SHA-256 | MVID |
| --- | --- | --- | --- |
| GameSaveCenter.Playnite.dll | net462 | `44979AD6B1F5929BC2F27C10BBCE81837CF816402E052B1BE5CBD30BAA00CC0F` | `d41038c0-f33a-4c06-9598-f6df5998d378` |
| GameSaveCenter.Playnite.Tests.dll | net472 | `902107B992FC404AEC7E928A345EA44F00F4D5D24039A7EFD1EC7AD84E25A945` | `d9e40ed9-26c6-44e8-9be0-fcca3bf8d371` |

二进制 SHA/MVID 与 TRX 都属于本地 Release `af915a2d` 构建。归档的 TRX、状态报告及截图：[TRX](trx/Q06ButtonStateSequenceBehaviorTests-runner-af915a2d.trx)、[状态读数](button-state-probe-report.txt)、[浅色普通](screenshots/light-normal.png)、[浅色悬停](screenshots/light-hover.png)、[浅色按压](screenshots/light-pressed.png)、[浅色焦点](screenshots/light-focus.png)、[浅色禁用](screenshots/light-disabled.png)、[深色普通](screenshots/dark-normal.png)、[深色悬停](screenshots/dark-hover.png)、[深色按压](screenshots/dark-pressed.png)、[深色焦点](screenshots/dark-focus.png)、[深色禁用](screenshots/dark-disabled.png)。账户名、机器名和本机绝对路径已脱敏。

Q06-08 的物理鼠标/键盘、真实宿主呈现和辅助技术边界仍未验收。若用户侧失败再次出现，需要对应失败 TRX/详细日志中的断言与堆栈、以及加载 DLL 的 SHA/MVID；本机成功不能替代这些信息。
