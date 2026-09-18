# R09-08 主题背景压力证据

日期：2026-09-19  
实现提交：`7de5c3de` 补齐主题背景压力验证

## 结论

R09-08 已满足。最新生产实现已经具备按真实 alpha 顺序合成背景、环境 wash、玻璃 surface 和正文的 `AdaptiveThemePaletteContrastGuard`；本阶段没有重建主题或材质系统，只补直接读取运行时 `ResourceDictionary` 的背景压力夹具，并修复了 R09-06 引入的主题工厂反射兼容性回归。

`AdaptiveThemePaletteFactory.Create` 保留原四参数入口，隔离高对比验证改用独立的 `CreateWithHighContrastOverride`，因此现有反射型源码测试和 Playnite/net462 调用契约不再因可选第五参数而失效。高对比仍关闭透明材质并使用系统语义色，正常主题仍保留透明玻璃和环境 wash。

## 可复核验证

- `R09BackgroundPressureBehaviorTests`：`2/2`。使用浅色中性、深色中性、暖色浅背景、蓝色深背景四种合成宿主背景，从运行时资源读取 `GscBackdropBrush`、`GscAmbientWideWashBrush`、`GscGlassFillBrush`、`GscGlassStrongBrush` 的实际 Solid/Gradient 颜色；确认环境与 surface 存在非零且小于不透明的 alpha，再按 backdrop→ambient→surface 的顺序测量正文对比度，全部达到 `4.5`。故意黑字/深色材质负例被同一检测器拒绝 `1/1`。
- 修复后主题/材质相邻回归 `8/8`；高对比、主题切换、阴影预算回归 `5/5`；R09 定向回归 `14/14`。
- `scripts/build.ps1 -SkipTests`：XAML `24/24`，Release solution `0 warning / 0 error`，包含 Playnite `net462` 与测试程序集。
- `python scripts/validate-source.py`、`scripts/check-xaml.ps1`、`git diff --check`：通过。

本阶段只使用合成宿主资源、隔离 STA WPF 和逻辑 DIP 颜色合成，不读取或修改真实存档、真实媒体、用户云端或系统主题设置。

## 边界与未验项

夹具证明了运行时资源的 alpha 合成和语义文字门槛，不等价于真实 Playnite presented frame、物理 DPI/跨屏、真实桌面色彩管理、UIA/读屏、IME、ETW、宿主性能或 package-host 安装验证。没有把代理性能、离屏结果或逻辑 DPI 写成真实呈现通过。

Demo 原目录不可用，本阶段继续沿用恢复生产资源基线。用户提供的 DEV-INSTALL-008 main 日志仍单列：编译 `0 warning / 0 error`、Core `83/83`、Worker `311/311`，但 dirty main 的 Playnite.Tests 全量为 `73 failed / 588 passed / 57 skipped`；本阶段没有覆盖 main 用户改动，也没有在 dirty main 上重跑安装器。

下一可执行任务：R10-01 上下文返回，先核对现有告警→任务→游戏详情返回契约和稳定 ID，再决定补行为证据或最小修复。
