# R08 动效采样稳定性复核（2026-10-25）

修复绑定候选提交 `e1127ac0` 的工作树测试变更；R08 源文件 SHA-256：`
7A4D5F75B4BC7DD349DFA04BD6C81B423A4B003388DB1CE40488488BC2D5D6FA
`。锁定 SDK `8.0.423`，目标程序集来自 `artifacts/gsc-b/d055f5fa`。

## 变更与验证

- 首次完整根入口 run `20261024T165132695Z-aadc0bbd` 在 WPF 第 74/145 类失败，R08 悬停层的旧动画时钟偶发覆盖合成悬停；安装/发布阶段未开始。失败摘要、该类 TRX 与 console 已归档。
- 测试等待模板初始离开动效结束，再开始合成输入；在夹具中克隆生产按钮模板的4个悬停/按压 `BeginStoryboard`，只将8个 `DoubleAnimation` 的时长扩为400ms，保留原目标、Easing、属性和值。断言4个故事板、8个动画均被调整；所有连续性、中间态、方向及最终态断言不变。共享MotionTokens默认120/100ms未改。
- `R08MotionReverseBehaviorTests` 连续5轮各3/3，共15/15；第1轮编译测试项目后运行，后4轮 `--no-build`。所有TRX分别附在本目录。
- 当前测试源下 `R09FocusOutlineBehaviorTests` 2/2；附件中外机 `Thickness(2)`/`Thickness(1)` 仍未绑定同一次源码/DLL/PDB身份，不能用本机结果代替外机验证。

## 重新运行的完整入口

本轮 R08 证据是定向测试，不构成完整验收。下一步须在提交后的干净工作树运行 `GameSaveCenter-Run.cmd`，保留源提交、SDK、构建/安装run、完整测试/跳过/失败数、TRX、ZIP/PEXT/安装程序集身份与启动结果。

## 附件

- `failed-standard-summary.json`、`failed-standard-r08.trx`、`failed-standard-r08-console.txt`：修复前根入口失败身份。
- `r08-iter-1.trx` 至 `r08-iter-5.trx`：修复后五轮。
- `r09-current.trx`：当前代码的 R09 2/2 定向结果。
