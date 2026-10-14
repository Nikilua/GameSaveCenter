# R08-02 当前 main 热关闭动画复核（2026-10-14）

当前代码基线：`d3cb0b2d`；测试和生产源文件 SHA-256 见 [`tests/hashes.txt`](tests/hashes.txt)。隔离 Release 输出在本机 SDK `9.0.302` 下构建；Playnite `net462`、测试 `net472`，MSBuild `17.14.13`、VSTest `17.14.1`、.NET Framework `4.8.9181.0`。本机只安装 SDK 9，本次选择 SDK 9 构建；它不替代仓库锁定 SDK 8.0.423 或 CI SDK 10.0.401。

## 行为复核

- 应用设置实际 ToggleSwitch：关闭期间中的 SettingsShell 入场时钟立即清理，Opacity/Y 回到 `1/0`；重新开启并等待原时长不重播。`R08MotionHotChangeBehaviorTests` `1/1`。
- 系统动效回调受控模拟：保留应用内动效设置为开启；对活动中的 SettingsShell 入场模拟系统策略关闭，调用生产 `OnSystemParametersChanged` 回调并让真实页面排队刷新主题/动效状态。时钟清理、Opacity/Y=`1/0`；模拟系统策略重新开启并等待后不重播。`R08SystemMotionHotChangeBehaviorTests` `1/1`。
- 生产侧栏活动过渡在 reduced-motion 关闭后到达收起稳定值，动画时钟停止且无迟到写入；最终态归一化另一实例通过。`ProductionShellChromeSourceTests` 的两个 reduced-motion 行为 `2/2`。
- Dashboard/Settings 延迟回调的订阅、卸载退订、Dispatcher 保护源门禁 `1/1`。它只校验事件接线和卸载约束；实际系统事件由上面的受控回调行为覆盖。
- Release 构建 `0 warning / 0 error`；`validate-source.py` 通过，XAML 结构 `24/24`，`git diff --check` 通过。四份脱敏 TRX 与插件/测试 DLL 的 SHA-256、MVID、ProductVersion 在 `tests/`。

WPF 类按隔离进程串行执行，以避免 STA/Application 全局状态在测试类间污染。测试成功后 VSTest 进程仍输出 WPF TextServices COM `InvalidComObjectException` 清理噪声；xUnit 计数和进程退出码成功，异常根因未知。

## 系统与宿主边界

测试通过实例级 `MotionEnabledOverrideForBehaviorTest` 模拟系统有效策略，没有修改 Windows 动画设置、注册表或全局偏好；测试调用真实页面的系统参数处理方法，但没有人为触发 `SystemParameters.StaticPropertyChanged` 静态事件。因此当前受控代码路径已覆盖，真实 Windows 设置通知仍需在安全 Playnite 宿主中确认。该内部注入属性在生产默认未设置时继续使用真实 `GscMotion.IsEnabled` 策略。

本项没有视觉改动，不生成截图。未运行真实 Playnite、实际系统偏好通知、物理 DPI/跨屏、OS UIA/Narrator、呈现帧、ETW 或宿主性能。没有触及真实存档、媒体或用户配置。Demo 原目录仍不可用，沿用已恢复生产基线。
