# R00-03 / R00-04 当前 main 证据复核

日期：2026-10-13。测试源码身份：`5762c874ef9c1ee2e92692a61c71d17cc9c0f371`（main）。本包只更新 R00-03、R00-04 两条 freshness 记录；R01 及其余 R00 记录没有命中本轮代码变化，保持原证据，不声称本轮重跑。

## 构建与行为结果

- Windows `10.0.22000`，本机 .NET SDK `8.0.423`、MSBuild `17.11.48`、VSTest `17.11.1`。用官方 `scripts/build.ps1 -Configuration Release -SkipTests` 构建：XAML 结构 `24/24`，Release `0 warning / 0 error`；Playnite 目标 `net462`，测试目标 `net472`。
- 定向行为组分开进程执行，共 `25 passed / 0 failed / 0 skipped`：

| 测试类 | 结果 | 覆盖范围 |
| --- | ---: | --- |
| `UiFinesseFoundationTests` | 9/9 | 动画终值、重入/取消后的当前 Dispatcher 值、时钟释放及相关源契约 |
| `R08MotionReverseBehaviorTests` | 2/2 | 真实 WPF 动画反向接管采样与时钟清理 |
| `R08MotionHotChangeBehaviorTests` | 1/1 | 动效设置关闭时结束活动入场；重新开启不重播 |
| `R08DialogOverlayBehaviorTests` | 3/3 | overlay 的显示/关闭与焦点生命周期 |
| `R05FocusBoundaryBehaviorTests` | 5/5 | Dashboard 确认弹层焦点、键盘边界及关闭行为 |
| `LargeLibraryPerformanceTests` | 5/5 | 集合替换、2000 项搜索 30 个不同查询，以及不可能结果数的有限超时负例 |

R00-04 原始报告在 [large-library.txt](benchmarks/large-library.txt)：预热 5 次、测量 30 次，30/30 次都改变过滤结果集合；查询序列及每个样本保留。此次本机合成 ViewModel 测量 p50 `46 ms`、p95 `63 ms`、max `75 ms`；超时负例同组通过。它只描述本机合成列表路径，不外推真实 Playnite 大型库表现。

## 两条 freshness 记录为何需要更新

- R00-03 关联的 `DashboardView.xaml.cs` 变化加入了面向行为测试的可选构造与 motion override。默认生产构造仍传入 `null / false`，沿用 `GscMotion.IsEnabled(plugin.Settings.EnableUiAnimations)`；本轮实际跑了相关 Dispatcher 动画组和 Dashboard 焦点行为组。`GscMotion.cs` 本身未在旧证据提交后变化。
- R00-04 关联的 `AcrylicProductionShellView.xaml` 变化仅把 `ToolTipService.BetweenShowDelay` 从 `100` 改为 `0`，与搜索实现无关；其余 GamePicker 搜索 ViewModel 与性能测试代码未变。由于 baseline 将该 XAML 文件作为关联源，仍按任务要求在当前身份重跑完整 `LargeLibraryPerformanceTests`。
- baseline 中 R00-03/R00-04 的 `sourceCommit` 已改为本次测试身份，并指向本 README。生成的 [freshness-summary.json](freshness-summary.json) 为 `14 fresh / 0 stale`；这是源码证据新鲜度，不等于整项 Round3 或 Playnite 宿主验收。

## 证据文件

下列 TRX 已去除本机用户名、计算机名和绝对临时路径；测试结果与全部测试名仍保留。`freshness-summary.json` 的身份为本次运行源提交；`large-library.txt` 是合成搜索的原始样本。

| 文件 | SHA-256 |
| --- | --- |
| `tests/UiFinesseFoundationTests.trx` | `F60CE00B16572809E095A2D360960AC225E2757632BD6DA078291BDBF9E910E0` |
| `tests/R08MotionReverseBehaviorTests.trx` | `9B12B81B4CA4B5027C10C3855DCD7B02242417E059B44C2CC1D4CBA9466DDF0E` |
| `tests/R08MotionHotChangeBehaviorTests.trx` | `56A8E6801D0C3217B6F11ACFFA2EFE430D503C39B13366C7E8691862947A47D9` |
| `tests/R08DialogOverlayBehaviorTests.trx` | `9A18AA9E07F8450DE1ED988408B4A819A0DE9606CEC33D54813ED9CA8DA18C20` |
| `tests/R05FocusBoundaryBehaviorTests.trx` | `AFCAE1A754FD2FF0DEBAA7034D1DE20633A634822C12BB00FAF1473C39F7285B` |
| `tests/LargeLibraryPerformanceTests.trx` | `ABE2200F0AC32D300AF05EBCF3A2D670FEBED3EDCD10E23E86648F458572DDEE` |
| `benchmarks/large-library.txt` | `EE3293427E4CE8BF8D4072B5AFB1FECF8DAF119AE6F6BF3DCD0711D24A3EB45C` |

## 未验证边界

没有构建或安装 Playnite 包（package identity `not-provided`），没有启动真实 Playnite。WPF 行为窗口与 2000 个游戏项均为本机合成夹具；没有验证 OS UIA/Narrator、真实输入/IME、物理 DPI/跨屏、最终 presented frame、ETW 或宿主性能。CI SDK 也不能由本机 SDK 推断；前一代码 run #952 失败的完整日志限制见当前 handoff。
