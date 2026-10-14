# R00/R01 共享主题与动画资源当前 main 复核

日期：2026-10-14。源码与测试身份：`ae8f3be9969fe8baa649d7ae39bb88f7c6498484`（`main`）。本包只校正受 Q24-07 共享主题/动效修改命中的 R00-01-02、R00-03、R00-05、R01-05 四条 freshness 记录。它不改写 192 项任务状态，也不表示真实 Playnite 或物理屏幕验收。

## 精确提交构建与测试

- Windows `10.0.22000`，本机隔离 .NET SDK `8.0.423`、MSBuild `17.11.48`、VSTest `17.11.1`。官方 `scripts/build.ps1 -Configuration Release -SkipTests`：XAML 结构 `24/24`，Release solution `0 warning / 0 error`。插件目标 `net462`，测试目标 `net472`，Worker 测试目标 `net8.0-windows10.0.19041.0`。
- 构建诊断身份 `passed`，`sourceCommit` 与本包提交一致；插件与测试程序集 `ProductVersion` 均为 `0.6.73+ae8f3be9969fe8baa649d7ae39bb88f7c6498484`。工作树标记为 dirty，因为已有其他任务留下的未跟踪证据/TRX 和一个 WPF 临时项目文件；它们没有加入本次提交，也未清理或改写。
- 五个类分进程执行，合计 `163 passed / 39 skipped / 0 failed`。39 项 skipped 保持单独统计，不计为通过。测试 DLL SHA-256 `71BD78A446ADC389D8ECEE9FC5707DDE7AF5E5D20A7D754D464B55D654BD35E3`，MVID `1bde9986-4710-49ac-8ea9-5954339f331b`。

| 测试类 | 结果 | 实际覆盖 |
| --- | ---: | --- |
| `UiDiagnosticsExporterTests` | 11/11 | 整组 Chrome opacity、非等距渐变 stop、hover+pressed+focus 组合的有效背景/文字合成；Light/Dark/Follow palette 和语义状态对比正负例。 |
| `UiFinesseFoundationTests` | 9/9 | Dispatcher 动画完成后清钟保留终值；重入保持当前有效值；取消不回跳；组合变换重复 1000 次后树保持稳定。 |
| `R09HighContrastBehaviorTests` | 3/3 | 使用生产资源与控件模板，覆盖 Follow/Light/Dark、宿主明暗、玻璃/动效请求组合下的禁用、选中、焦点、Popup；高对比下清除活动按钮/Disclosure 动画，再恢复普通动效。 |
| `WpfUiResourceDictionaryTests` | 139 passed / 39 skipped | Light/Dark 下实际构造 Context、RemoteRestore、MediaBatch 三种派生按钮；Chrome opacity 为 `0.72`，内容文字与说明色不透明，禁用/启用高度差 `<0.01 DIP`，标签对比度至少 `3.0`。同类其余未跳过用例也通过。 |
| `UiNegativeFixtureRegistryTests` | 1/1 | N01–N05 五个检测器逐一捕获其 expected-failure：黑字暗底、横向数值裁切、无可见候选时 Enter、固定父级超宽子级、Loading 覆盖层底层命中。负例不进入生产入口；层级报告写入一次性隔离目录。 |

测试源对照：`c00f47f2` 工作树的首次完整 `WpfUiResourceDictionaryTests` 运行复现三个断言失败，分别是两个 XAML 属性顺序假设，以及一个要求旧 `GscPopupAnimation` 源码字面式的断言。此次提交将前两项改为按 XAML 名称/样式属性解析，并将动效检查改为调用生产 `ApplyRuntimeThemeResources` 后实测 Fade、显式关闭、高对比强制关闭和恢复。完整资源类已在 `ae8f3be9` 精确提交重新通过。该本机复现不证明旧 CI run 的失败由这三项导致。

## Freshness 校正

复跑前 [freshness-before.json](freshness-before.json) 命中四条 stale：R00-01-02（`DesignTokens.xaml`、`MotionTokens.xaml`、`WpfUiProduction.xaml`）、R00-03（`DashboardView.xaml.cs`）、R00-05（上述共享主题资源及测试文件）、R01-05（`AdaptiveThemePalette.cs`）。四项均已按上述实际行为测试复核，baseline sourceCommit 和 evidence 现指向本提交/本包；未命中的其他 10 条保留各自已有证据。更新后 [freshness-summary.json](freshness-summary.json) 为 `14 fresh / 0 stale`，`package identity=not-provided`。这只表示登记源码路径的新鲜度，不等于宿主验收。

## TRX 与身份

所有 TRX 使用仓库的 `ConvertTo-GscDiagnosticText` 规则去除主机、用户名与仓库绝对路径；文件保留完整测试名和结果。

| 文件 | 结果 | SHA-256 |
| --- | ---: | --- |
| [UiDiagnosticsExporterTests.trx](tests/UiDiagnosticsExporterTests.trx) | 11/11 | `FB289758981CC242CE4979A3DC954BFAC1EFFF073DBDD223CA3AF20FF09FE8A5` |
| [UiFinesseFoundationTests.trx](tests/UiFinesseFoundationTests.trx) | 9/9 | `99381278A8AD67B7E9BAC4E242482D3CF67A71403173B266418D0432A69E929C` |
| [R09HighContrastBehaviorTests.trx](tests/R09HighContrastBehaviorTests.trx) | 3/3 | `9C4C500B01990F039BB2A02603FFE7B6F378D769557404D3BDA9D46469651E1B` |
| [WpfUiResourceDictionaryTests.trx](tests/WpfUiResourceDictionaryTests.trx) | 139 passed / 39 skipped | `3DA8F54EE59DF687651942C5BC6CFC128B2AA9C1C7BB587FF312A7D227076BE6` |
| [UiNegativeFixtureRegistryTests.trx](tests/UiNegativeFixtureRegistryTests.trx) | 1/1 | `58D537401DAB58098AF18196F19637ED5759A2473EB433F7C9C18FE2897C5E6C` |

插件 DLL SHA-256 `3BBFDCD2044DADE8954338741539123D74F1AB9CB25CAF1A2696E00D58937130`，MVID `3a59afc3-7ca0-4ef4-8bb1-0b85044a1526`。未生成或安装 Playnite 扩展包，package identity 仍为 `not-provided`。

## GitHub Actions 与外部边界

- #977 / run `38507898138`（source `c00f47f21e61c1f90e425bdb588a1b55533d0acb`）的 SDK10.0.401 命名 job 与 SDK8 job 均在 Release 编译/测试步骤失败；此前置 run 的失败原因仍未知。完整 job logs 匿名下载 HTTP 403。diagnostics artifact `11868792390`（518,334 bytes，SHA-256 `70eaeacd45103ce5c6481ed093da62e61f8dfdaa06d14b8da1a9232f2eaeb574`）和 `11869725370`（593,030 bytes，SHA-256 `2d5231df80bdf656389c9458ce3cff1f1962d84e774d63e713eb2b984b21430a`）下载均 HTTP 401。不能从通用 exit code 反推 CI 根因。
- 本次代码提交 `ae8f3be9969fe8baa649d7ae39bb88f7c6498484` 对应 [#978 / run `38509928015`](https://github.com/Nikilua/GameSaveCenter/actions/runs/38509928015)，现已终态失败，总耗时 `15m08s`。SDK8 命名 job `115675846585` 的“编译与测试”步骤失败；SDK10.0.401 命名 job `115675846886` 的“隔离工作树并运行 Release 编译与测试”步骤失败，耗时 `11m13s`。job 名分别指向 SDK8 与 SDK10.0.401 lane，但没有完整日志，无法确认 runner 实际 SDK 版本、失败命令或测试断言。
- #978 页面显示两个诊断 artifact：`GameSaveCenter-build-diagnostics`，580 KB，digest `sha256:11108fca6bcd9cefcd13a26734ef5450e0d91628e9aaac391bbb07313b637e64`；`GameSaveCenter-sdk10-build-diagnostics`，506 KB，digest `sha256:9a72271153208947622f8c20dea0c4a9b3f03191470eea61a944f219b7eb5d18`。匿名日志页提示需登录查看，当前环境无法读取完整日志/包内容。页面另有 Node.js 20 弃用告警，不能据此认定为失败原因。CI 根因未知，不凭 exit code 盲改；本机 SDK `8.0.423` 结果不能替代 SDK10 lane。
- 2026-10-14 重新附加的 #970/#971 ZIP 与已审文件 SHA 完全一致：#970 SDK10 与 SDK8、#971 SDK8；没有新增 #971 SDK10 diagnostics。映射和根因见[诊断记录](../CI-RUN-970-971-TEST-RECHECK-20261013/README.md)。
- 合成 STA WPF、调色板算法与离屏资源验证；未运行真实 Playnite、未生成安装包；未切换真实 Windows 高对比主题或验证 OS UIA/Narrator/IME、物理 DPI/跨屏、最终 presented frame、ETW 或宿主性能。没有绕过系统跟踪权限。
