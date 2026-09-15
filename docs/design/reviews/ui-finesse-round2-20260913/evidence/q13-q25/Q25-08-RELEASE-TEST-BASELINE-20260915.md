# Q25-08 当前 Release 测试基线

采集日期：2026-09-15（Asia/Shanghai）  
代码基线：`d8fad48`（`更新跨屏证据最终代码基线`）

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

本页同时保留 `d8fad48` 历史基线、`c76ce62` 重入动画跟进和 `8dfe7fa` 资源宿主修复跟进，支持 Q25-08 的代码/自动化收尾；真实宿主边界仍按 `ROUND2_PROGRESS.md` 和 `Q13-Q25-INDEX.md` 保留，不把自动测试升级成宿主验收。
