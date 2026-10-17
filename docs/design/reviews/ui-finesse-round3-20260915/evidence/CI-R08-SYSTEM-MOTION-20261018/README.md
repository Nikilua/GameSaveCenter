# R08 system-motion CI 时长夹具复核（2026-10-18）

## 基线 run #1078

提交 `5241add2fd93ca04ca65b95242a783239eb7e65e` 对应 [Actions #1078/run `39054894478`](https://github.com/Nikilua/GameSaveCenter/actions/runs/39054894478)。两条 job 均已终态，但 run 整体为失败：

| Lane | Job | 结果 |
| --- | --- | --- |
| SDK 8.0.423 | `117460748837` | Release build `0 warning / 0 error`；Core `125/125`；Worker `362/362`；WPF 隔离在 `[66/125]` 的 R08 system-motion 测试失败 |
| SDK 10.0.401 | `117460748676` | Release build 与完整测试成功；Core `125/125`、Worker `362/362`、WPF 隔离 `125` 类 |

SDK8 完整 job log 指向 `R08SystemMotionHotChangeBehaviorTests.SystemAnimationPreferenceChangeEndsEntranceAndReenableDoesNotReplayIt` 第58行。启动入场时钟的正向断言失败，日志值 `opacity=1; loaded=True`，`DependencyPropertyHelper` 未观察到动画时钟。TRX 位于 CI 诊断目录 `20261017T174255023Z-77415c26/tests/wpf.R08SystemMotionHotChangeBehaviorTests/`。测试退出时出现的 WPF TextServices `InvalidComObjectException` 在该断言和 `Test Run Failed` 后，日志没有证据表明它是失败原因。

SDK8 诊断 artifact：ID `12109790764`，609,140 bytes，SHA-256 `edb89c8b722baeaff502cc113df97f7f252bc4a4f44ed67ba66b862caf18373f`。SDK10 artifact：ID `12109567791`，776,313 bytes，SHA-256 `26d32ae0062ff13e94801656e0694c8b7e61b35a7f6914cc298720c52abc33de`。完整 job logs 已通过 GitHub connector 读取。SDK10 的成功不覆盖 SDK8 失败；本次 CI 也不代表用户外机 R09 失败已解决。

## 夹具修正

系统动效热切换测试需要先确立一个正在运行的 entrance clock，再由真实 `GameSaveCenterSettingsView.OnSystemParametersChanged` 回调触发关闭和恢复检查。原夹具在窗口加载期间改写资源字典时长，但 SDK8 runner 未能在动画启动断言观察到活动时钟。现有日志没有提供启动瞬间的实际资源时长，故不把具体系统参数或 Dispatcher 覆写路径记为已证实根因。

共享 `GscMotion.AnimateEntrance(element, offset)` 生产入口仍按元素资源读取 normal/slow 时长，再委托给新增的显式时长重载。该行为测试直接传入一秒时长，消除 fixture 对宿主资源刷新时序的依赖，但仍通过同一个 `Track`、动画建立和完成回调。测试保留动画实际启动正向断言；关掉系统策略后仍检查两个时钟释放、Opacity/Y=`1/0`；重新打开并经过原时长后仍检查不重播。没有弱化交互或负例，也没有更改生产默认策略。

## 本机复核与下一步

- 本机 Windows：SDK `9.0.302`、VSTest `17.14.1`。修改后的 Playnite `net462` / tests `net472` Release 相关项目编译，目标行为测试 `1/1` 通过。
- `python scripts/validate-source.py .` 通过，`git diff --check` 通过。无视觉变化，不适用截图验收；未启动真实 Playnite，未改 Windows 动效设置。
- 本机 SDK9 结果不替代 SDK8.0.423 或 SDK10.0.401。阶段提交后的新 Actions 结果见下节。

未验证边界：真实 Playnite 系统偏好通知、系统高对比度环境中的用户界面行为、OS UIA/读屏、物理 DPI/跨屏、最终呈现帧和宿主性能。外机 R09 同次 checkout/test DLL/PDB/plugin DLL 身份继续缺失，与本次 R08 CI 结果无关。

## #1079 双 SDK 收口与 R00/R01 freshness

代码提交 `de8bdbb176bb7ae85c4ca3f488f0aeb37958bfec` 对应 [Actions #1079/run `39057273073`](https://github.com/Nikilua/GameSaveCenter/actions/runs/39057273073)，两条 job 均终态成功。完整日志确认 SDK8.0.423 job `117467706465` 和 SDK10.0.401 job `117467706337` 都运行了完整 Playnite WPF 隔离清单 `125/125`；其中 `UiFinesseFoundationTests` 是第 `121/125` 类，随后 R08 system-motion 类也运行，最终均报告全部 Playnite 测试成功。两 lane Release build 均 `0 warning / 0 error`，Core `125/125`、Worker `362/362`。

SDK8 另完成 `render-qa OK`、WPF 静态审查（30 XAML、0 errors、27 warnings、177 info）、扩展包生成和上传。Artifacts：SDK10 diagnostics `12110214194`（775,251 bytes，SHA-256 `fa95154a4b93daedd4d57f26547a0b02959bb53c40c222a2e0f3b803d992dc2e`）；SDK8 diagnostics `12110638801`（1,628,891 bytes，SHA-256 `2fb041e67a8e5f5b39caedcea4adabf9cddd65e7d5aba1e3057fa07b7839141b`）；SDK8 package `12111535634`（90,314,202 bytes，SHA-256 `dd45ec28d148d19202a80acdb1b917f59ade012c24dca0a7ec621b34eec59381`）。

R00/R01 freshness 初扫的 14 条记录中，只有 R00-03 与 R01-04 命中 `GscMotion.cs`。#1079 全量隔离测试在两种 SDK lane 中覆盖这两个记录共用的 `UiFinesseFoundationTests`，因此将这两条登记证据更新到精确代码提交 `de8bdbb`；复核脚本在更新后的基线返回 `14 fresh / 0 stale / 0 matched source paths`，package identity `not-provided`。这只刷新已登记源码范围，不代表包安装或真实 Playnite 呈现。完整 job log 已读取；CI 通过只说明相同提交在对应 GitHub runner 输入下通过。

R00-03 与 R01-04 继续标记“已满足（受控 WPF 行为）”。真实 Playnite 生命周期/系统偏好通知、OS UIA/读屏、物理 DPI/跨屏、最终呈现帧和宿主性能未验。用户外机 R09 的 `1/2` 失败仍是独立真实问题，当前同次 checkout、TRX、test/plugin DLL/PDB 身份仍缺；本次 CI 不覆盖该失败。
