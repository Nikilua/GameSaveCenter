# R08-06 当前 main 系统动效门控复核

日期：2026-10-13

任务：`R08-06 | 数字变化动效`

状态：R08 数字反馈在 #965 的 CI 诊断包中 `5/5` 通过；整次 run 仍因无关布局测试失败。#962/#963 的旧失败已由系统动效输入修正覆盖；本轮补齐 #965 的布局测试宿主校正后，须以推送后的新 Actions 终态收口门禁。

## CI 失败证据

用户提供的诊断压缩包与各 run 的源码提交相符：

| 压缩包 | run / 源码提交 | CI SDK / VSTest | 失败 |
|---|---|---|---|
| `GameSaveCenter-build-diagnostics.zip` | #963 / `38338090417` / `dc8f26ee972c08976ca8cfbe7531e5c68c5a0559` | SDK `8.0.423` / VSTest `17.11.1` | `R08NumericChangeBehaviorTests.CounterChangeUsesRealRenderPulseAndSuppressesHighFrequencyBounce` 的反馈计数为 `0`，期望 `1`；该类另外 2 项通过 |
| `GameSaveCenter-build-diagnostics (1).zip` | #962 / `38335989087` / `87d27592086bbef355b9426a00614b5bb9856e8c` | SDK `8.0.423` / VSTest `17.11.1` | 同一方法、同一 `expected 1 / actual 0`；该类另外 2 项通过 |
| `GameSaveCenter-build-diagnostics (2).zip` | #965 / `38342488530` / `d0dbaea00dc620545524236c55444748d43d6540` | SDK `8.0.423` / VSTest `17.11.1` | Release build 成功、`R08NumericChangeBehaviorTests` `5/5`；布局类 `50/54`，另两项测试在双主题下失败：收件箱中心偏差 `42 DIP`、Settings 壳宽 `952 DIP`，详见 [#965 布局宿主校正](../CI-965-LAYOUT-HOST-CURRENT-MAIN-20261013/README.md) |

两份 summary 都记录 Release 构建成功，随后只在 `R08NumericChangeBehaviorTests` 失败。该失败不是 ScaleX 归零：TRX 的源行 `69` 映射到源码第一条内部反馈计数断言（源码第 68 行）；本机临时制造该断言失败后复核了 PDB 行映射。压缩包 SHA-256：#963 `ACCD264B84772BAD13FE9B09EFD973420CE7125506CD0213B1E6C67DBF5C0B8B`；#962 `1DA9613A6B35CDBCD7461A5C374260150C4D9A132F848AF6EF4A00C3E7E5E157`。

源码中数字反馈先验证数字文本、页面动效开关，再调用 `GscMotion.IsEnabled(true)`；后者遵循 High Contrast 与 Windows `ClientAreaAnimation`。先前 #959 同一 runner 的完整日志记录 `HighContrast=False; ClientAreaAnimation=False`，因此旧测试隐含要求 runner 开启系统动效，与产品无动画偏好规则冲突。#962/#963 压缩包没有记录这两个系统值，故将其归因为测试环境输入未固定；不宣称两个包直接证明了具体系统值。

随后推送的 #964 / `38339686245` / `83fbee9712195afe454f6f36f07e130ecc236398` 已终态 Failure，公开 job `115107636301` 显示“编译与测试”失败。匿名日志读取不可用，`artifacts_partial` 返回 404；#964 未取得完整诊断包。该提交尚未包含本次修正。

## 修正与行为覆盖

- 为 `NumericChangeFeedback` 的每个 `TextBlock` 增加内部审计覆盖值；新挂接时清空覆盖。默认值仍为 `null`，生产路径继续读取真实 `GscMotion.IsEnabled(true)`；无新增公开 API 或设置。
- 正例显式固定审计输入为开启，实际 STA WPF `Window` 采样 `99→100` 的 `ScaleTransform` 动画时钟，接着 `101→102` 高频刷新仍只有一个反馈，`NormalizeAll()` 后缩放回到 `1`，固定 `96 DIP` 数字槽位不推动相邻控件。
- 负例固定系统动效关闭，确认数字立即更新、反馈数为 `0`、无动画。现有应用级 `MotionEnabled=false` 负例继续覆盖即时更新；新增默认路径测试则将结果与当前 `GscMotion.IsEnabled(true)` 比较。
- 保留 Overview 六个启用计数、排除任务进度与技术文本的源码范围检查；它只证明接线范围，动画行为由上述 WPF 测试证明。

## 本机结果

- 源码基线 HEAD `83fbee9712195afe454f6f36f07e130ecc236398`；构建时工作树非 clean，唯一 tracked 差异为 `NumericChangeFeedback.cs` 与 `R08NumericChangeBehaviorTests.cs`。本 README 与 TRX 是本阶段新证据；R04/R05/R07 未跟踪证据保留且未纳入本阶段。
- 明确调用隔离安装的 `.NET SDK 8.0.423`（仓库 `global.json` 同版本），VSTest `17.11.1`。Release solution build `0 warning / 0 error`；`python scripts/validate-source.py` 与 `git diff --check` 通过。
- `R08NumericChangeBehaviorTests` `5 passed / 0 failed / 0 skipped`：真实动效正例、应用级 reduced-motion 负例、系统动效关闭负例、默认系统偏好路径、Overview 范围检查。
- 本机 .NET Framework `4.8.9181.0` 与 CI 日志的 `4.8.9345.0` 不同。本机结果不能代替推送后 Actions；需确认新 run 完成。
- 测试源 SHA-256：`NumericChangeFeedback.cs` `2BF28D05780501340EECB1088614389D591E1AD2FD37DE5BC88D873A980985AF`；`R08NumericChangeBehaviorTests.cs` `FC15DEAB7F6210A8706F9C64678621665D030FAA35E3F3BEDAA9ADDD11C6D3F4`。
- 程序集（本地 build metadata 仍标记基线 `83fbee97`）：Playnite SHA-256 `AD27A35BDF87CD27350D74D5F480837D395018125D2CBFEC2A92F9F6FCAA113F` / MVID `c600771b-c2a1-4347-afab-71ffb7d5ce64`；Tests SHA-256 `BBD15C5E020FACB2C1A031EECF8C55394E8EF77449381405B8CC96BF074B9140` / MVID `434425c6-def5-49a7-95fb-802a34e8aeda`。
- 脱敏 TRX：`tests/R08NumericChangeBehaviorTests-sdk8423-current-main.trx`，SHA-256 `D181AAD26F87CDEAD97A1C5D9FC69620A97ADA4C8A5B060DA39FF69FBE96517F`。

## 未验边界与下一步

当前行为证据是合成输入和隔离 STA WPF；没有启动真实 Playnite/Worker，也没有验物理 DPI/跨屏、UIA/读屏、真实 OS 输入、最终呈现帧、ETW 或宿主性能。Demo 原始 `DesignShellView.xaml`/`Pages` 目录在当前 checkout 缺失，沿用恢复生产基线。

R08 测试本身已在 #965 通过；整次 run 的具体布局失败和测试宿主改动见 [#965 布局宿主校正](../CI-965-LAYOUT-HOST-CURRENT-MAIN-20261013/README.md)。修正提交并推送 main 后，仍要检查新 Actions 直到终态。只有 CI 成功才收口当前门禁；随后先复核 R00/R01 问题与证据，再推进依赖已满足的 Q/R。
