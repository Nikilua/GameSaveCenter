# R11-08 当前 main 日期范围、DST 与跳转行为复核

日期：2026-10-16。测试源码基线提交 `964f325d3bed8e273de98e6d8532b1c5e5065fc7`，测试源码在构建时含本轮未提交变更；本轮只调整现有日期范围 helper 并补测试，没有改 XAML、DTO、备份服务或真实存档。

## 复核范围

- 复用 `BackupVersionDto`、`BackupHistoryDateRange`、`DashboardViewModel.BackupHistoryView` 与生产 `SaveCenterView`。生产入口继续使用系统本地时区；helper 增加显式 `TimeZoneInfo` 参数，以便在固定时区里确定性验证 DST。
- `DaylightSavingTransitionsUseTargetLocalDateAndUtcNavigationOrder` 覆盖美东 2026 春季跳时（本地 01:59 到 03:01）、DST 转换日午夜前后日期归属，以及秋季两条不同 UTC 时间都显示本地 01:30 时仍按 UTC 先后导航。
- `ProductionSaveHistoryRangeAndNavigationControlsFilterRestoreAndSelectVersions` 在隔离 STA 窗口加载真实 `SaveCenterView`，以合成的今天/较早/未知时间 DTO 设置生产 DataGrid 和真实 ViewModel：切到近 7 天只显示今天；最近按钮选择今天；空的昨天范围清空所选行并禁用两个跳转命令和按钮；清空范围恢复全部三行；最近/较早按钮随后分别选择今天和较早版本。旧的源契约断言保留，但交互验收以该运行时行为测试为证。
- 同类现有测试另覆盖今天/昨天/未知值、同秒 BackupId 稳定次序和跳转时间文案。本次定向类共 `6/6`，0 失败、0 跳过。

## 本机编译和结果身份

- 本机 SDK `9.0.302`、MSBuild `17.14.13`、VSTest `17.14.1`；测试 host 为 .NET Framework `4.8.9181.0`。插件 `net462` 与 Playnite 测试 `net472` 的 Release 项目及引用构建通过，0 warning / 0 error。构建从仓库外层目录选择可用 SDK；仓库 `global.json` 固定 `8.0.423`，本机结果不替代 SDK8 或 CI SDK `10.0.401`。
- 构建身份参数为 `GscBuildCommit=964f325d3bed8e273de98e6d8532b1c5e5065fc7`，测试源当时有修改；源哈希、程序集哈希与脱敏 TRX 一并列出，避免把未提交工作树误记为该提交自身。
- `scripts/validate-source.py` 与 `git diff --check` 通过。未修改视觉资源，无需新的截图或离屏视觉声明。

| 文件 | SHA-256 |
| --- | --- |
| `src/GameSaveCenter.Playnite/Infrastructure/BackupHistoryDateRange.cs` | `3FF0BB81A9B4C0A0086AF3B39174860C3C23657896F19CC30C363E02B698BEF3` |
| `src/GameSaveCenter.Playnite/ViewModels/DashboardViewModel.BackupHistory.cs` | `18356553A008DD88E59690D01CB456517BB7519487F8E7F735709C8E268B64DE` |
| `src/GameSaveCenter.Playnite/Views/SaveCenterView.xaml` | `4A86836BB5FB4CDA36DD2E4085F43A345EB9F2ABDBFDD1C7C96CE5CA8DC718F9` |
| `src/GameSaveCenter.Playnite/Views/SaveCenterView.xaml.cs` | `4EEB811FCC51EF268BC8C7295CECAE5E39AC0C9ABC444AF81D20C01053EAF7DE` |
| `tests/GameSaveCenter.Playnite.Tests/R11HistoryTimeNavigationBehaviorTests.cs` | `A5E4842EE1D4F9B1896B559A0CACC40EF055F36DA1198B35DFB43EC82182FEF9` |
| `GameSaveCenter.Playnite.dll` | `E288B4C944DE7882E283C42BD9BC8793CB6C2B2D05831A10A5E46E43075D4617` |
| `GameSaveCenter.Playnite.pdb` | `CB97EDAA071A1CBD0594DEB5361663B8BAC0F3D662AA84589A27558E1DC3EEE8` |
| `GameSaveCenter.Playnite.Tests.dll` | `FAF08DD23E540BCB61A29D171B44B848EC54790B6698DF5C1154C79140A5C789` |
| `GameSaveCenter.Playnite.Tests.pdb` | `859E812E8B07B6729B179C76B81C82AD2EE80F393B3569384D755ACDBAF0C941` |
| `tests/R11HistoryTimeNavigationBehaviorTests-sdk9302-release.trx` | `146BBD15C785AD528C66B50A714E853D2AA37DCAAAE9203B97BEB2BE9673D3AB` |

## 外部边界和下一步

测试只用合成 DTO 与隔离 STA 窗口，不启动真实 Playnite/package host，不读取或写入用户存档、媒体、设置或云端。没有验证最终呈现帧、物理 DPI/跨屏、外部 UIA/读屏、真实 OS 输入、ETW 或宿主性能。另一台机器 R09 的 `2 DIP` 断言仍与当前测试源不一致；这组 R11 结果与 R09 故障无关，不能覆盖其外机失败或推断其根因。

本轮源改动提交并推送后，追踪对应 Actions 的 SDK8 和 SDK10.0.401 job 到终态；当前提交前的 #1040 不包含本轮源改动。CI 结束后再推进账本中下一个依赖已满足的 Q/R 项。
