# R08-02 当前 main 热关闭动画复核（2026-09-30）

生产源码身份：`dc7f97cfa49724778c4987224c9f736c744b3631`；测试/RenderHarness 执行时 HEAD 为 `63b0c19816a28e1b9138e4d2029772242094bd3c`（之后仅有文档变更）。运行目录由本批隔离构建复制的受控 WPF 输出构成，不安装到 Playnite 用户扩展目录。

## 当前行为证据

- `R08MotionHotChangeBehaviorTests.SettingsAnimationToggleEndsEntranceAndReenableDoesNotReplayIt`：TRX `1/1` 通过、失败/跳过为 `0`、VSTest exit `0`。真实生产 `GameSaveCenterSettingsView` 的 SettingsShell 入场中途关闭应用动画设置后，Opacity/Translate 时钟立即清除且恢复 opacity=`1`、Y=`0`；重新开启并等待原动画周期，不重播旧动画。
- RenderHarness `motionhotprobe` 在生产 `AcrylicProductionShellView` 上以 Light/Dark 两主题执行真实按钮事件。动画进行中 `duringAnimated=True`，关闭后两主题都立即达到收起终点 `72 DIP`、opacity=`1`、X=`0`、无活动时钟；关闭状态重新开合立即抵达 `270 DIP`，仍无残留时钟。逐次几何和退出状态在 [`motion-hot-probe-report.txt`](render-harness/motion-hot-probe-report.txt)，代表图覆盖启用中、关闭终点和禁用态再次交互。
- 当前代码在 Dashboard 与 Settings 页面订阅/卸载 `SystemParameters.StaticPropertyChanged`，并在主题刷新中按 `GscMotion.IsEnabled` 归一化活动状态。**本轮没有更改 Windows“显示动画”设置，也没有人为触发真实系统设置通知**；系统通知到达当前 Playnite 进程仍待安全宿主验证。

## 身份与日志

- 插件 DLL：`ProductVersion=0.6.73+unknown`，SHA-256 `854E1549BA35626BBBFD19C82F9596CC5B6AC4320A72D28941CEDBD919BC9EC5`，MVID `3ad2d781-4904-4ff1-bfc2-5cf394ffdafb`。
- 测试 DLL：`ProductVersion=0.6.73+dc7f97cfa49724778c4987224c9f736c744b3631`，SHA-256 `1E5A4D29C33327BDBEB3A340986CC04551D602DAC7B05937A8A3AAF07AB36745`，MVID `1ef3680f-fcc8-4eff-81e9-7ae091df7e9f`。
- 测试 TRX 在 xUnit `1/1` 成功后记录 `8` 条 WPF `TextServicesHost.OnUnregisterTextStore InvalidComObjectException` 清理输出；根因未知，不改变 xUnit/VSTest 通过结果，也不归因于产品动画。

## 验收边界

截图与量测来自合成数据、STA WPF `Window`、离屏逻辑 DPI `1.00`；没有启动真实 Playnite、切换真实 Windows 动画偏好、验证物理 DPI/跨屏/呈现帧/UIA/ETW/宿主帧率。不会以此声称系统偏好通知已真机验收。应用内动效关闭/重新开启的受控行为已有当前身份复核；保留真实系统通知边界，Round3 状态不标记为无边界验收。
