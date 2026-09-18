# R08-01 动画中途反向连续

## 结论

R08-01 在当前分支已满足当前可控验收条件。生产修改提交为 `a7aaa89e`，取样夹具稳定提交为 `d62757e9`。先核对已有能力：侧栏开合已经有 `sidebarTransitionGeneration`；本项只为通用 `GscMotion.AnimateTranslate` 补按元素 generation guard，避免旧完成回调覆盖最新目标，没有重建动画体系或改变命令/Binding、安全语义、游戏选框和滚动条系统。

## 行为与数据证据

当前身份隔离 Release 输出为 `r08-01-build-d62757e9`；完整 solution Release 构建输出为 `r08-01-solution-build-d62757e9`，均为 `0 warning / 0 error`，Playnite 目标为 `net462`。XAML 结构检查 `24/24`；`R08MotionReverseBehaviorTests 2/2`；相邻 `ProductionShellChromeSourceTests 10/10`、`UiFinesseFoundationTests 8/8`；`python scripts/validate-source.py` 与 `git diff --check` 通过。

实际 STA WPF 行为取样：

- Translate：普通动画目标为 `12`，中途 80 ms 取样为 `8.403`，随后改为 `-8`；反向开始值仍为 `8.403`，最终为 `-8`，动画时钟已释放。
- 侧栏：真实 `AcrylicProductionShellView` 在合成 `Window` 中先收起再快速展开；收起取样 `169.333`，反向起点仍为 `169.333`，反向 200 ms 取样 `221.333`，最终宽度 `270`，opacity `1`，transition 与 opacity clock 均已清理。

断言覆盖了内部中点、当前有效值连续性、最新目标、最终状态和时钟释放，不是只检查源码字符串。

## 验收边界

证据使用合成/fake 数据、真实生产 WPF shell、隔离 STA Window 和 offscreen logical DIP；没有真实存档、媒体、云端或诊断写入。未验真实 Playnite 宿主、物理 DPI/跨屏、物理鼠标/触控板/键盘、UIA/读屏、呈现帧、ETW、宿主性能或用户 OS reduced-motion 设置。Demo 原始目录不可用，沿用恢复生产基线。完整 solution 构建日志未报告警告或错误。

下一可执行任务为 R08-02 热关闭动画；R08-01 不证明宿主呈现帧或系统级性能。
