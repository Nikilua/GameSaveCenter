# Q24-02 当前 main PageHost 短窗与恢复复核

日期：2026-10-14。范围是受控 WPF 行为和几何复核；不代表真实 Playnite、物理 DPI 或最终显示器呈现。

## 实现与复核

- `5fcd68562ee79ef819af054fb14a189c2610d890` 扩展已有 `R07ResizeStressBehaviorTests`，直接记录生产 `AcrylicProductionShellView.PageHost.ActualWidth/ActualHeight`，并检查短窗高度缩小、宽窗恢复尺寸一致。没有用 Window 请求尺寸替代 PageHost 几何。
- 在 Q24 合跑时复现了生产 `GscToolTipBehavior` 的跨 Dispatcher 树扫描异常：收件箱用例关闭其 STA 窗口后，另一个 WPF 窗口 `Loaded` 回调遍历 `PresentationSource.CurrentSources` 中不属于当前线程的 Visual，触发 `Dispatcher.VerifyAccess()`。单独运行原两组测试通过，合跑在 5 个收件箱用例后中止；[修复前 TRX](tests/Combined-before-tooltip-fix-5fcd6856.trx)，SHA-256 `F4494F0884DEFA7C0059B92F9041009099AF9516B227B3C299D24545D8D39565`。
- `74815b2bccd3fe89e123538f038c552ae47a068b` 让 Tooltip/Popup 扫描只枚举与 scope 同 Dispatcher 的 PresentationSource roots；新增 `R05TooltipTimingBehaviorTests.RefreshIgnoresPresentationRootsOwnedByAnotherDispatcher`，以合成独立 STA `HwndSource` 锁定回归。现有同 Dispatcher Tooltip/Popup 路径仍由原行为用例覆盖。

## 本机证据

- Windows 11 `10.0.22000`；隔离 SDK `8.0.423`；VSTest `17.11.1`；Playnite 插件 `net462`、测试 `net472`。精确代码身份为 `74815b2bccd3fe89e123538f038c552ae47a068b`。插件 DLL SHA-256 `8928BAD992186C05F97EE0DC760F118709B3E66812E884D3E2FC816FF1A0366A`；测试 DLL SHA-256 `8D92E1D8F67D53D2FA1A93C9A2376E73FB7320B24F6F94F523CB787295ED086C`。
- 精确身份定向组 `MediaInboxScrollBehaviorTests`、`R07ResizeStressBehaviorTests`、`R05TooltipTimingBehaviorTests`：`9 passed / 0 failed / 0 skipped`。TRX：[Q24-02-PageHost-scroll-tooltip-74815b2b.trx](tests/Q24-02-PageHost-scroll-tooltip-74815b2b.trx)，SHA-256 `9A92F7493AE93398669643897AF023FFF1CFE8343D2C08D3984136E67E8D22B0`。
- PageHost 实测（DIP）：

| 阶段 | Window 实际尺寸 | Shell 实际尺寸 | PageHost 实际尺寸 |
| --- | ---: | ---: | ---: |
| wide | `1366×900` | `1352.667×886.667` | `1038.667×759.333` |
| narrow | `960×700` | `946.667×686.667` | `632.667×559.333` |
| short | `960×560` | `946.667×546.667` | `632.667×419.333` |
| wide-again | `1440×900` | `1426.667×886.667` | `1112.667×759.333` |
| restored | `1366×900` | `1352.667×886.667` | `1038.667×759.333` |

  恢复后的 PageHost 宽高与初始宽窗一致（误差不超过 `1 DIP`）；短窗 PageHost 高度确实下降。详情面板、选框与焦点在五个阶段保持可用，生产 shell 收到 `11` 次 `SizeChanged`。
- 收件箱使用 `2,000` 条合成媒体 DTO。Grid 保持行虚拟化、有限视口，初始视口约 `5` 行、实际化 `7` 行；滚动三轮覆盖顶部、中段、四分之三、滚动条 thumb 末端、追加页后锚点恢复、窄窗末行和页脚动作可达、宽窗恢复。所有检查中实际化行数保持 `<40`，末行完整可见。
- 测试参数 `outputScale=1.0/1.25/1.5` 是夹具 RenderTransform；同一台机器报告的真实 WPF DPI 为 `1.5×1.5`。这不能称为物理 DPI 切换、跨屏、真实呈现帧或宿主性能证据。
- 补充运行了 Popup、焦点和选框虚拟化类：总计 `8 passed / 8 failed / 0 skipped`，不作为通过门禁。失败现场记录了 Popup 未创建、选框未显示、`Focus()=False`/键盘焦点为空，以及选框虚拟化定位值为 `0`；[精确身份 TRX](tests/R05-transient-surface-regression-74815b2b-exact.trx)，SHA-256 `9DF8C89E846873F83321071E5CAFBF57B237772F4BCF7F70D01D3647745A47DF`。本次没有查明这些交互夹具前提失效的原因；不归因为 Tooltip 修复，也不把它们算作通过。其余直接相关的 Tooltip 行为 `3/3`、Popup lifecycle `1/1` 通过。
- `scripts/validate-source.py` 与 `git diff --check` 通过；UI evidence freshness 检查在 source `74815b2b` 下为 `0 stale`。无 XAML/生产视觉改动，因此没有新增截图或 RenderHarness 声明。

## CI 与未验边界

- #973/run `38429051152` 对上一份推送文档提交 `bee29749` 最近仍显示 `In progress`，不包含 `5fcd6856`/`74815b2b`。四项代码与证据已随 `cb31947b` 推送；新 run #974/run `38436999880` 的 SDK8 job `115433489365`、SDK10 job `115433489872` 页面仍显示 Re-running jobs，日志要求登录，实际 SDK 版本/终态尚未取证。#973 与 #974 均需继续跟到终态。
- #970/#971 用户补充的三个 ZIP 经 SHA-256 对照均为已审包的相同副本：#970 SDK10 `155612D3…43EAE`、#970 SDK8 `598C2CA6…4A8B6`、#971 SDK8 `6AC19F26…ADA4C`。#970 的 SDK8/SDK10 失败与通过内容均已定位；#971 SDK10 lane 仍无原始诊断，根因未知，不阻塞其他独立任务。
- 没有启动真实 Playnite、调整 OS 显示设置或跨越物理显示器；没有验证真实宿主主题/Chrome、UIA/IME/读屏、最终 presented frame、ETW 或宿主性能。业务数据全是合成 DTO，不读写用户配置、存档、媒体、云端或真实目录。
- Q24-02 受控 WPF 行为/几何栏通过；真实宿主仍外部待验，最终状态不签收。下一项可执行任务是 Q24-04 键盘次序；Q24-03 需要第二台物理显示器，本机当前只枚举到 `DISPLAY1`，留作外部阻塞。
