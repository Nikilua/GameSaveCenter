# R11-01 版本摘要当前 main 复核

日期：2026-10-14；分支 main；源码基线提交 58090d99ae2d7918aeb3557817d472021d8305a2。该任务已有实现，本轮只复核并补当前 main 证据，没有改生产或测试源码。

## 现有能力与验收

复用 BackupVersionDto、RestoreReadinessDto、SaveCenterView 和既有 DataGrid 列宽/滚动系统。历史行显示时间、类型、文件数、大小、来源、备注和状态；空来源显示“未知设备”；未知或未检查的恢复状态显示中性文案，锁定本身不表示校验安全；只有 Ready 才显示“可恢复”。长校验摘要留在 ToolTip/详情，不推宽固定列。

行为覆盖：

- R11VersionSummaryBehaviorTests：2/2。DTO 正/负状态验证空来源、未提供 readiness 与锁定组合、Ready 摘要；真实 SaveCenterView 在 STA Window 中加载合成历史行和 7 列 DataGrid，并检查来源/状态绑定与未知状态不触发成功色。
- R06EmptyStateBehaviorTests：2/2。Save 页面重试/忙态与空结果分离；表格保留 loading、empty、completed、failure 状态。
- R06SortingBehaviorTests：7/7。Save 历史时间/数值稳定排序、未知项排序顺序、生产列头排序双向与 CollectionView 脱离负例。
- R06ColumnWidthPersistenceBehaviorTests：6/6。生产列键稳定性、未知键/非法版本负例、窄窗口最小宽可经现有横向滚动访问、按页面恢复像素宽度、重置仅影响当前视图。
- 总计 17/17，0 失败/跳过。四个 WPF 类分进程串行运行。

## 本机构建与测试身份

- 本机 SDK 9.0.302；VSTest 17.14.1；测试 host 为 .NET Framework 4.8.9181.0。
- Release 测试项目及引用项目构建成功，0 warning / 0 error；Playnite 插件目标 net462，测试目标 net472。仓库 global.json 要求 SDK 8.0.423，本机 SDK9 结果不替代 SDK8 或 CI SDK10.0.401。
- source validator 通过，XAML 结构检查 24/24，git diff --check 通过。没有视觉源码改动，因此未生成新的截图，也不把旧图当成本轮证据。
- DLL ProductVersion 为 0.6.73+58090d99ae2d7918aeb3557817d472021d8305a2。

| 文件 | SHA-256 | MVID / 版本 |
| --- | --- | --- |
| DashboardDtos.cs | EC89626A82BFB5915DB3C838F250CD3BDE9D9FE41DA7EA47093C3FEA0B893D49 | — |
| SaveCenterView.xaml | 4A86836BB5FB4CDA36DD2E4085F43A345EB9F2ABDBFDD1C7C96CE5CA8DC718F9 | — |
| R11VersionSummaryBehaviorTests.cs | 8636B5533CCE1B527D74041C5ABE8B07EB0938648C6B2448A28573F6574DE831 | — |
| R06EmptyStateBehaviorTests.cs | 5BE2033173409EFFAFE9C2E7264A6BC66E486CD23E87CC295C57A0059170EE04 | — |
| R06SortingBehaviorTests.cs | 453CB93ECD6E5611011BD22F5E04A0ECD524D1ACD90484A3DC91486EEA08049 | — |
| R06ColumnWidthPersistenceBehaviorTests.cs | CB41E6A518AE63F4D35D8BC345CCCF2D2A2C02F9860DE6B7E547D1C90E64BC18 | — |
| GameSaveCenter.Playnite.dll | EBFEC5A5E55D7134D81420EA052B268599A03F05413D0B2AE6F6698806FB1D73 | 3df7d39e-a1b2-4fc7-97ed-d7172ddb758b |
| GameSaveCenter.Playnite.pdb | 1FB8532854C978E030AC86916FA3EE4C352A95EE2F4384284C07DD1A66620BBC | — |
| GameSaveCenter.Playnite.Tests.dll | BCAFAFC4C3291AF68941401F947B44A99008FE29457621F4E2EAF83A3F9F78B0 | 75e298e7-4088-4e0a-b1ad-5ebb44e68234 |
| GameSaveCenter.Playnite.Tests.pdb | 6753B54D4B58E4986EC23FF5E84D00C7F91D1DA0FF63D28453E707925E209692 | — |

脱敏 TRX：

| 结果 | SHA-256 |
| --- | --- |
| tests/R11VersionSummaryBehaviorTests-current-main.trx | E2DA1D76287444ADA81867803FC6EB6E2141F2A53E76D22FEE5228B66018B92C |
| tests/R06EmptyStateBehaviorTests-current-main.trx | A3F3AD60E180278EC3E3F5E5EB84C7EB3013E9C55B0ACE410FD14E917B68ECC2 |
| tests/R06SortingBehaviorTests-current-main.trx | 2C6D7B55319857C526901B7098B8CDA65CBD24894C70ADD6B270F3848AF4081D |
| tests/R06ColumnWidthPersistenceBehaviorTests-current-main.trx | AD8459259AF290C3346B69A8E5DBA292960865755AF4A456D72BC6B27F0127CE |

## 外部边界与后续

用合成 DTO、真实 SaveCenterView 和隔离 STA Window/逻辑 DIP；没有启动真实 Playnite/package-host，没有读取或修改真实存档、媒体、用户云端或设置。未验证真实呈现帧、物理 DPI/跨屏、外部 UIA/读屏、真实键盘/IME、ETW 或宿主性能。Demo 原目录在本机不可用，继续采用恢复的生产基线。

已推送代码提交 58090d99。匿名 GitHub Actions API 返回 HTTP 403 rate limit；最新公开页面是旧缓存快照（972 results，最新可见 #972/head 8546aca In progress；较早快照曾显示 #979），两者均不含 58090d99，故该提交的 run 是否创建/终态未知。不得把 SDK9 本机通过当作 CI 通过；继续间隔查询。

下一可执行任务：R11-02 当前 main 复核。先核对现有 A/B 选择、交换方向与相同版本禁用行为，复用 CompareBackups；不执行真实恢复。