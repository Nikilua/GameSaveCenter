# Q25-08 当前 Release 测试基线

采集日期：2026-09-15（Asia/Shanghai）  
代码基线：`39e37b1`（`补充侧栏卸载清理回归`）

## `37f92f7` 当前提交 Release 与宿主收尾

在当前 clean tree 执行 `scripts/real-host-audit.ps1 -Configuration Release`，使用全新隔离 UserData、扩展目录和 Worker IPC；构建、测试、打包、安装与真实 Playnite 捕获均退出成功。

| 测试程序集 | 通过 | 跳过 | 总计 |
| --- | ---: | ---: | ---: |
| GameSaveCenter.Core.Tests | 83 | 0 | 83 |
| GameSaveCenter.Worker.Tests | 311 | 0 | 311 |
| GameSaveCenter.Playnite.Tests | 501 | 57 | 558 |

XAML `24/24`，构建 `0 warning/0 error`；程序集身份为 `0.6.73+37f92f7f107880bb5a33f61c82eee11fe1344874`。`artifacts/ui-host-audit-round2-fp-final6-20260915/summary.json` 记录 Dashboard/Settings 均为 `EmbeddedPlaynite`、`HighGateCount=0`；capture manifest 为 29 个 Dashboard 视口、2 个滚动面和 1 个 Settings 视口，150% DPI；审计结束后已用官方 shutdown 清理隔离宿主，用户 Worker 未触碰。

该当前提交证据仍不替代第二物理屏、低于 560 DIP 短窗、IME、读屏、真实鼠标/键盘组合输入、ETW 呈现帧或真实宿主耐久验收。

## 69e1f84 最终真实宿主审计构建基线

`69e1f84`（`补齐宿主审计提交身份`）的 clean-tree `real-host-audit.ps1 -Configuration Release` 完成打包、隔离安装和真实 Playnite 宿主捕获；构建与测试退出码 `0`，失败 `0`。该次运行同时覆盖了 Settings 入场截图时序修复和审计 SHA 元数据修复。

| 测试程序集 | 通过 | 跳过 | 总计 |
| --- | ---: | ---: | ---: |
| GameSaveCenter.Core.Tests | 83 | 0 | 83 |
| GameSaveCenter.Worker.Tests | 311 | 0 | 311 |
| GameSaveCenter.Playnite.Tests | 494 | 57 | 551 |

构建身份为 `0.6.73+69e1f844f8b20b1fcf1667d2b8a6af2772eb0ed4`；XAML `24/24`；真实宿主 summary 的 Dashboard/Settings 均为 `EmbeddedPlaynite`，Q24-03 仅记录单屏阻塞。该 Release 审计仍不等价于物理跨屏、IME、读屏、ETW 呈现帧或真实宿主耐久验收。

## 39e37b1 生产侧栏卸载清理跟进

`39e37b1`（`补充侧栏卸载清理回归`）在同一命令下通过：退出码 `0`，失败 `0`。新增的生产侧栏 STA WPF Window 回归实际触发侧栏完成动画，再在长动画中关闭窗口并推进 Dispatcher，确认完成/卸载路径释放 Opacity/X 时钟、结束过渡并将位移归零；Playnite 测试总数变为 `550`。

| 测试程序集 | 通过 | 跳过 | 总计 |
| --- | ---: | ---: | ---: |
| GameSaveCenter.Core.Tests | 83 | 0 | 83 |
| GameSaveCenter.Worker.Tests | 310 | 1 | 311 |
| GameSaveCenter.Playnite.Tests | 487 | 63 | 550 |

该生产壳层回归在 Release 下独立连续回放 `5/5` 通过。它仍是受控 WPF Window，不等价于真实 Playnite Loaded/Unloaded 100 次、宿主窗口关闭、Rendering/ETW 采样或物理屏幕帧验收。

## 68b49a1 动效终态运行时跟进

`68b49a1`（`补充动效时钟终态运行时回归`）在同一命令下再次通过：退出码 `0`，失败 `0`。新增的 STA WPF Window 测试实际推进 `AnimateTranslate`/`AnimateEntrance`，并在完成后确认终值写回且 `DependencyProperty` 不再处于动画状态；Playnite 测试总数变为 `549`。

| 测试程序集 | 通过 | 跳过 | 总计 |
| --- | ---: | ---: | ---: |
| GameSaveCenter.Core.Tests | 83 | 0 | 83 |
| GameSaveCenter.Worker.Tests | 310 | 1 | 311 |
| GameSaveCenter.Playnite.Tests | 486 | 63 | 549 |

该运行时门禁仍是受控 WPF Window；专门的 Release 回放连续 `5/5` 通过，但不等价于真实 Playnite Loaded/Unloaded 循环、宿主窗口关闭、Rendering/ETW 采样或物理屏幕帧验收。

## 最新修复后跟进

`c76ce62`（`修复重入动画与不确定进度停机`）在同一命令下再次通过：退出码 `0`，失败 `0`。新增的两个回归门禁使 Playnite 测试总数变为 `547`，不是测试减少或跳过增加。

| 测试程序集 | 通过 | 跳过 | 总计 |
| --- | ---: | ---: | ---: |
| GameSaveCenter.Core.Tests | 83 | 0 | 83 |
| GameSaveCenter.Worker.Tests | 310 | 1 | 311 |
| GameSaveCenter.Playnite.Tests | 484 | 63 | 547 |

这次跟进仍只证明 Release 构建与自动测试通过；真实 Playnite 宿主屏幕像素、物理 DPI、IME、读屏、跨屏 Popup、进度节奏和 ETW 性能边界继续保持未验。

## Q18-01 资源宿主修复后跟进

`8dfe7fa`（`补齐动画资源宿主解析路径`）在同一命令下再次通过：退出码 `0`，失败 `0`。新增的 Q18-01 调用路径回归门禁使 Playnite 测试总数变为 `548`。

| 测试程序集 | 通过 | 跳过 | 总计 |
| --- | ---: | ---: | ---: |
| GameSaveCenter.Core.Tests | 83 | 0 | 83 |
| GameSaveCenter.Worker.Tests | 310 | 1 | 311 |
| GameSaveCenter.Playnite.Tests | 485 | 63 | 548 |

真实动画热切换、系统动画偏好变化、宿主屏幕像素和 ETW 性能边界仍未由自动测试替代。

## Q18-04/Q18-07 动效清理跟进

`4414f05`（`修复动效完成与卸载清理`）在同一命令下再次通过：退出码 `0`，失败 `0`。本次将完成态 `FillBehavior`、完成回调、对话框取消代际、Toast 逐卡清理和 Dashboard/壳层卸载清理纳入代码门禁；测试数量与上一跟进保持一致。

| 测试程序集 | 通过 | 跳过 | 总计 |
| --- | ---: | ---: | ---: |
| GameSaveCenter.Core.Tests | 83 | 0 | 83 |
| GameSaveCenter.Worker.Tests | 310 | 1 | 311 |
| GameSaveCenter.Playnite.Tests | 485 | 63 | 548 |

真实 Playnite Loaded/Unloaded 循环、窗口关闭、Rendering/ETW 采样和物理屏幕帧仍是外部边界；本跟进不把源码门禁写成宿主验收。

## 命令与结果

在干净工作树上使用单 MSBuild 节点执行：

```text
dotnet test GameSaveCenter.sln --no-restore -c Release -m:1 --logger "console;verbosity=minimal"
```

结果：退出码 `0`，失败 `0`。

| 测试程序集 | 通过 | 跳过 | 总计 |
| --- | ---: | ---: | ---: |
| GameSaveCenter.Core.Tests | 83 | 0 | 83 |
| GameSaveCenter.Worker.Tests | 310 | 1 | 311 |
| GameSaveCenter.Playnite.Tests | 482 | 63 | 545 |

跳过项仍是项目既有的宿主/隔离边界测试，不被改写为通过；其余 Release 测试全部通过。该命令证明当前代码的构建与自动测试基线，不证明 Playnite 真实宿主的屏幕像素、物理 DPI、IME、读屏、跨屏 Popup 或 ETW 性能证据。

## 与 Q25-08 的关系

本页同时保留 `d8fad48` 历史基线、`c76ce62` 重入动画跟进、`8dfe7fa` 资源宿主修复跟进、`4414f05` 动效清理跟进和 `68b49a1` 动效终态运行时跟进，支持 Q25-08 的代码/自动化收尾；真实宿主边界仍按 `ROUND2_PROGRESS.md` 和 `Q13-Q25-INDEX.md` 保留，不把自动测试升级成宿主验收。
