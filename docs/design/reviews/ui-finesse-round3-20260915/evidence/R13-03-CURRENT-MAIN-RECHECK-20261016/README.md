# R13-03 当前 main 手动重试范围复核

- 日期：2026-10-16
- 源码提交：`ad2e67167072ca2af2a55e32de16077ab5a6603e`（`补强云端重试范围行为验证`）
- 分支：`main`；本轮只补测试，不改生产命令、DTO、Worker、XAML 或存档逻辑。

## 已有能力与本轮验证

任务原实现提交 `680ea83a` 已覆盖手动重试边界。本轮先核对现有维护页入口、`BusyOperationCoordinator`、`RelayCommand`、Worker 幂等 RequestId ledger 和本地备份复用路径，没有重建服务或 DTO。新增/强化的行为验证包括：

- 使用真实 `BusyOperationCoordinator` 与 `RelayCommand` 执行第一项失败传输；操作尚未结束时再次 Execute 被门控拒绝，最终 DTO 只完成一次状态迁移。
- `R13CloudTransferStageBehaviorTests` 在本轮完整隔离 Release 输出上 `14/14`。失败/等待重试项可重试；传输中、已上传、远端已验证为负例；同时验证队列筛选/分页选择、生产维护视图阶段与保证级别、详情边界和忙态第二次提交。
- `ManualCloudRetryUsesExistingBackupTests.RetryUploadsExistingLocalBackupWithoutRequiringAnotherLudusaviBackup` 在 Worker 全量测试中通过。合成目录、隔离 SQLite 和故意失败的 fake cloud executable 验证已有本地副本仍在、失败错误可见，且没有启动 Ludusavi 重新备份。
- 当前源码的 R00/R01 freshness 为 `14/14 fresh`、`0 stale`、`0` 个登记源码路径命中，比较 `2,866` 条路径；package identity 未提供。精简结果见 [freshness 报告](R00-R01-freshness-ad2e6716.json)。

## 本机 Release 门禁

使用仓库锁定 SDK `8.0.423`、MSBuild `17.11.48`、VSTest `17.11.1`、`.NET Framework 4.8.9181.0` testhost，并将构建/测试输出隔离在 `.tmp/r09-full-isolated-ad2e6716-20261016/`：

- XAML `24/24`，Release solution build `0 warning / 0 error`。
- Core `125/125`；Worker `360 passed / 1 skipped / 0 failed`。唯一跳过是本机环境不能创建本地 Named Pipe；手动重试复用本地备份用例为 Passed。
- 完整 Playnite 隔离扫描在第 `45/124` 个 WPF 类停止：`R05TooltipTimingBehaviorTests` 要求测试前读取系统指针位置，`GetCursorPos` 返回 Win32 `ERROR_ACCESS_DENIED (5)`。本机只做只读探测，没有移动指针；因此不把中断后的扫描记作全量通过，也不把此系统输入限制归到产品 UI。
- 在同一份刚生成的隔离程序集上，R09 焦点轮廓类 `2/2`，R13-03 类 `14/14`。两份 TRX 在 `tests/` 下。R09 的 test DLL SHA-256 为 `467C6560507BF68F0BA8804A0CAA32855A20AC2122DB1D92AA7509132FE416B5`、MVID `56fa26da-7f2a-4c5a-a722-0cf604a121b2`、PDB SHA-256 `2F1D9038B6667EEEA21F5B4E8140C54C86E5D9D329818C7272E4595EFDF68C09`；ProductVersion 为 `0.6.73+ad2e67167072ca2af2a55e32de16077ab5a6603e`。生产插件 DLL SHA-256 `9EF09C7966C12836A11AE281190776B4310838C82F35BD2C4B91ACBBAF701326`，MVID `ba816aae-163d-4a88-9ef2-cfe70f92dbb2`。
- R09 测试源 SHA-256 `A5AAB9E90C340F93301D8424FF1E30D98B160D39C1265172AEE7A6B01B2CBC98`；R13 Playnite 测试源 SHA-256 `444BA64816F4AB21CCED6C6033699AD2756CFA6F419E008340ACE164E69667EA`；Worker fake 重试测试源 SHA-256 `DDC5C168364B57E68AEEF7BAE645257F873B0EF9A721E9D668BE8C604522E182`；生产共享输入模板 `WpfUiProduction.xaml` SHA-256 `FD212D59A0244DF8CABF33A3926FB5CE80CF930E4A0B587692D2DC9D3B15EC6E`。

SDK 8 完整脚本因受限桌面的 `GetCursorPos` 前置失败而未跑完整 Playnite 集合；这与 R09/R13 定向类结果分开记录。旧 Actions #1037/run `38859916493` 的 SDK8、SDK10.0.401 jobs 均已成功，但它们构建的是 `09173448`，不能代替 `ad2e6716` 的待查 Actions。

## 外机 R09 日志的身份边界

用户贴出的外机日志同样报告 R09 `Expected 2 / Actual 1`，堆栈指向测试源第 151 行，并显示 WPF 类隔离总数 `116`。当前 `main`（以及截图中的祖先提交 `983fee5`）第 151 行检查错误边框颜色，第 154 行才检查独立 `ValidationStroke` 的 `2 DIP`；当前全量隔离脚本发现 `124` 个 WPF 类。日志未带实际 HEAD、源快照、DLL/PDB SHA 或诊断清单，故只能确认测试行映射/测试集身份与当前复核不同，不能断言外机根因，也没有更改生产样式或放宽断言。完整复核见 [R09 外机失败记录](../CI-R09-OTHER-MACHINE-FAILURE-RECHECK-20261014/README.md)。

当前 main 的 R09 全类 TRX SHA-256：`9C73CD9B5C9B966C269AF569644A354650055CA62FB9C57168688CC32D0C08AA`。R13-03 全类 TRX SHA-256：`87046588ACBDBC56191DC3167E9B3391E5333F28F4AACD347C854E46F75EBEB2`。

## 边界与下一步

仅使用合成 DTO、fake cloud executable、隔离 SQLite 和临时目录；未访问真实存档、真实媒体、用户云端，也未外发诊断。当前受控 UI 行为通过，R13-03 维持“已满足”；真实 Playnite/package-host、真实远端、物理 DPI/跨屏、OS UIA/IME、最终呈现帧、ETW 与宿主性能仍待外部环境验证。完成文档与提交后，下一项为 `R13-04 暂停与允许时段`；推送后先跟踪本提交的 SDK8 与 SDK10.0.401 Actions 至终态。
