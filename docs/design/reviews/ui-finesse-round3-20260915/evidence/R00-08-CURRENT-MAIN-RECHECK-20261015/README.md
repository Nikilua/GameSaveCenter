# R00-08 键盘搜索与焦点行为：当前 main 复核

日期：2026-10-15

测试代码身份：`55ed3a17f14087859319b350297132c7fcb3f79c`（当前 main；本轮未修改生产源码）

## 复核原因和范围

freshness 扫描发现 `R10KeyboardShortcutHelpBehaviorTests.cs` 命中 R00-08 登记的 `*Keyboard*.cs` 路径，所以本轮重跑搜索框 Enter/IME/焦点回返行为、生产键盘导航轨迹，以及该新增的生产帮助弹层键盘行为测试。只更新 R00-08 baseline 记录；没有修改生产 XAML、ViewModel、命令或资源。

## 行为结果

- `GamePickerKeyboardBehaviorTests 6/6`：无可见结果时 Enter 保留旧游戏并保持 picker 打开；未提交文本组合期间 picker 保持打开；ImeProcessed 和方向键仍走 picker 输入路由；有效可见候选 Enter 确认；Escape 关闭并把焦点还给上下文按钮；清空搜索按钮清文本并返回搜索框焦点。
- `R21KeyboardNavigationTraceTests 2/2`：Shell 与 Settings 入口 Name/TabStop 状态有效；8 个真实生产入口都能以前向 Tab 和反向 Shift+Tab 到达当前可见交互目标，反向顺序与正向轨迹相反且不越出入口。
- `R10KeyboardShortcutHelpBehaviorTests 5/5`：当前页帮助弹层可开关、只列当前可执行命令、不可执行搜索命令时保持空列表；包含 Workspace 变更和生产帮助入口绑定行为。该类对应触发 freshness 的新增键盘测试路径。

所有类分别串行运行。输入由隔离 STA WPF Window 和合成 routed key/text-composition events 提供；这不能证明 Windows OS IME、实体键盘、Playnite 实际嵌入输入链或最终屏幕呈现。

## 构建和测试环境

本机隔离 SDK `8.0.423`（`D:\GameSaveCenterTools\dotnet-sdk-8.0.423\dotnet.exe`），VSTest `17.11.1`；Playnite 目标 `net462`，测试目标 `net472`，testhost 为 .NET Framework `4.8.9181.0`。首个过滤构建在隔离 `.tmp/r00-08-55ed3a17-20261015` 下编译测试项目与引用，随后两个类使用同一精确输出 `--no-build` 顺序执行。不是完整 solution build，也没有运行真实 Playnite/package-host。

清理检查（2026-10-15）：3 个 TRX 已归档在本证据目录；`.tmp/r00-08-55ed3a17-20261015` 的 bin/obj 暂未删除，检查时存在来源不明的活动 `dotnet` 进程，无法确认缓存未被占用。DLL/PDB 不作为持久产物保留。

XAML 结构检查 `24/24`、`python scripts/validate-source.py`、`git diff --check` 通过。CI SDK8 / SDK10.0.401 是独立门禁；本机 SDK8 不替代推送后 Actions 终态。package identity 没有提供。

## 文件身份

| 文件 | SHA-256 | MVID / 说明 |
| --- | --- | --- |
| `R10KeyboardShortcutHelpBehaviorTests.cs` | `E4E0A118D72D0075B9AFFD9EEE91F335695124EA095297EE846E777F0AC4CFF2` | freshness 命中源 |
| `GamePickerKeyboardBehaviorTests.cs` | `D930DE267A6903DDA3280C13C2247391418587EC6DBFDD83B0E3A497AAC0319D` | 搜索框行为源 |
| `R21KeyboardNavigationTraceTests.cs` | `E788AD7C559955E83B5ABAC1131009A3231F892EF98BAB3837365CC7B406280E` | 焦点轨迹源 |
| Playnite.Tests.dll | `37241F7564A54BD44096285BD75A4EC01A6644E98BDF50B113EF3607C498B28F` | MVID `c29141cd-850d-48a5-8fd0-865144d9180d` |
| Playnite.Tests.pdb | `00D244E089BAF9FDB8986D3D64603618F66A9CC413BB064DFAB9CEB5F52340C8` | 测试符号 |
| Playnite.dll | `1EE257AA527D2D0B33A4326CC68DE694BB698C916B29C98DA4CBB365C8128BB5` | MVID `52ada672-a2a4-4d82-ba9c-c965323a8fe7` |

| `R10KeyboardShortcutHelpBehaviorTests` TRX | `846589AFFE9A029BF4CB4E8EB047C5F596A802A856C04B003127F4005743BD96` | [5/5](tests/R10KeyboardShortcutHelpBehaviorTests-sdk8423-55ed3a17.trx) |
| `GamePickerKeyboardBehaviorTests` TRX | `171A2A1004FDEA93B61D5B071F47D4243359ECF8D13B1A49924C4BA64028EBAA` | [6/6](tests/GamePickerKeyboardBehaviorTests-sdk8423-55ed3a17.trx) |
| `R21KeyboardNavigationTraceTests` TRX | `F8A7D44831305AB19EFACF27345C4F476DF56BDF38F932064386714EF574BFA4` | [2/2](tests/R21KeyboardNavigationTraceTests-sdk8423-55ed3a17.trx) |

测试程序集 informational version 为 `0.6.73+55ed3a17`。

## Freshness 校正

[复核前 freshness JSON](freshness-before-summary.json) 精确绑定 `40af39c80d904bd6306a4d5433e1356e367925ca`，显示唯一 stale 为 R00-08，匹配路径 `tests/GameSaveCenter.Playnite.Tests/R10KeyboardShortcutHelpBehaviorTests.cs`。本轮三个相关类全部通过后，R00-08 baseline 的 `sourceCommit` 更新到被测试的 `55ed3a17f14087859319b350297132c7fcb3f79c`，`evidence` 更新到本文件；其余 R00/R01 记录未改。更新后的 [freshness summary](freshness-summary.json) 记录 14 项、14 项 fresh、0 项需重跑。

未验边界：真实 Playnite/OS 输入设备、中文 IME 候选 UI、读屏、物理 DPI/跨屏、presented frame、ETW 和宿主性能。没有读取或修改用户存档、媒体或云端。

## 2026-10-15 Q24-04 键盘源 freshness 补充

提交 `e221feb35a4f7ca796dc348c2c5cde247a5b58ed` 在 R00-08 登记路径中只改了 R21 键盘测试源。最终源文件 SHA-256 为 `13EA3A03BA4CCF4D26049916F78ECCCB0625D4C79E82FFDF4EB8D4E62AD9885A`；新增失败任务行为和原有 R21 轨迹 `3/3` 通过。TRX、测试/插件程序集身份见[Q24-04 业务状态键盘证据](../Q24-04-BUSINESS-STATE-KEYBOARD-RECHECK-20261015/README.md)。测试在代码提交前从 `5838bb51` 基础的工作树构建；程序集 ProductVersion 标记 `5838bb51`，所以不声称二进制是在 `e221feb3` 之后构建。提交中的源码内容与受测 SHA 一致。

上文未改变的 `GamePickerKeyboardBehaviorTests 6/6` 与 `R10KeyboardShortcutHelpBehaviorTests 5/5` 仍适用；其登记源码未改。合并 R00-08 行为证据为 `14/14`，由上述历史两类和更新后的 R21 组成。baseline `sourceCommit` 已更新为 `e221feb3`；最新 freshness 结果见 [freshness-summary.json](freshness-summary.json)：14 records、14 fresh、0 needsRerun。package identity 仍为 `not-provided`。
