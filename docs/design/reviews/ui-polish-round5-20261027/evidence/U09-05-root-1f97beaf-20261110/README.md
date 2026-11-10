# U09-05：1f97beaf 中文根入口与独立 build.cmd 验收

## 冻结源码与入口

- Source SHA：`1f97beafda1895ae1b84bc453828607157e1a29b`
- SDK：仓库基线 `8.0.423`
- 实际存在并核对：`GameSaveCenter-一键构建安装运行.cmd`、`GameSaveCenter-Run.cmd`、`scripts/build.cmd`
- 中文包装器链：`GameSaveCenter-一键构建安装运行.cmd → GameSaveCenter-Run.cmd → scripts/dev-install-run.ps1`
- 构建审计摘要记录了既有未跟踪证据目录和 `TestResults`，故 `workingTreeClean=false`；这些是未跟踪工作数据。提交 `1f97beaf` 下没有已跟踪的 src/tests/scripts 改动，未暂存或删除用户/其它任务文件。

## 中文根入口

第一次尝试在沙箱执行完整入口，构建测试结束后复制到 Playnite AppData 被访问策略拒绝，根cmd exit 1；原诊断保留于 `artifacts/round5-root-chain-1f97beaf-20261110/attempt-1-sandbox-denied/`。该次构建诊断 summary 为2,032项、1,964通过、0失败、68跳过；额外10项环境限制skip不是通过。

释放空间后第二次在相同源码上执行最终中文入口并转交英文根入口，未超时，根进程 exit 0。启动前C盘4,607,627,264 bytes、D盘19,923,603,456 bytes，满足4 GiB/3 GiB预检；输入桌面前后均为 `Default`，构建/安装工作站为 `WinSta0`。精确结果：`artifacts/round5-root-chain-1f97beaf-20261110/attempt-2-result.json`；SDK、每一步退出码与TRX汇总：`artifacts/build-diagnostics/20261110T015136805Z-18ad1a94/summary.json`。

完整链Release build/test共2,032项：1,974通过、0失败、58跳过；160份TRX，157个WPF隔离类。58项skip按现有范围分别为57条已撤销“今日工作台”旧架构断言、1条默认关闭的系统指针opt-in用例；都不计通过。恢复真实宿主数据、删除媒体或云端写入不在本轮执行范围。

## 独立 `scripts/build.cmd`

按脚本自身构建与测试职责单独运行，SDK8.0.423/Release、没有 `-SkipTests`。第一次人为隔离输出路径生成265字符备份目标，三条设置/工作清单恢复测试失败；保留原始失败诊断 `artifacts/build-diagnostics/round5-buildcmd-20261110-07c443176a6a47a0a7c63d5860ab5a59/20261110T021830623Z-e2bea4e7/`。失败方法：

- `WorklistCrossSessionPersistenceBehaviorTests.MalformedWorklistSettingsFallBackWithoutChangingOrLosingTheOriginalConfig`
- `PluginSettingsLoadRecoveryTests.MalformedSettingsJsonFallsBackAndKeepsRecoverableBackup`
- `PluginSettingsLoadRecoveryTests.CorruptViewPreferencesAreDiscardedWhileOtherSettingsAndOriginalFileArePreserved`

三项失败均发生在测试夹具复制备份文件时；没有删除断言。将隔离输出根缩短后，215字符路径探针的完整 `scripts/build.cmd` 成功，进程exit 0，无 `SkipTests`。2,032项/1,964通过/0失败/68跳过，160份TRX、157个WPF隔离类。68个skip包含57项已撤销旧架构断言、9项Named Pipe本机客户端和1项Worker进程用例受执行环境限制、1项系统指针opt-in；环境限制项不是通过。精确汇总：`artifacts/build-diagnostics/r5b-buildcmd-20261110/20261110T022534919Z-3b7c675f/summary.json`；入口结果：`artifacts/build-diagnostics/r5b-buildcmd-20261110/standalone-buildcmd-result.json`。

## 发布、安装与启动身份

- 安装产品版本：`0.6.73+1f97beafda1895ae1b84bc453828607157e1a29b`
- 安装身份目录：`%APPDATA%/Playnite/Extensions/GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec`
- zip 与 `.pext` 各45,738,964 bytes，SHA-256相同：`018F813E8C373CCC1D3EC30E44BF7CEFF98519C12D34AC9378BCE099C773DB8C`
- Plugin、Core、Contracts、Worker及Worker Core/Contracts六个程序集版本与SHA-256见 `root-chain-summary.json`。
- 包名、哈希与大小已从磁盘复算；安装记录为 `artifacts/last-dev-install.txt`，根入口完整日志为 `artifacts/one-click-install.log`。
- 根入口启动了 Playnite 和 Worker；复核时两进程仍运行、主窗口句柄均为0。没有强杀进程。

## 窗口与闪屏边界

构建入口及逐类WPF runner运行于WinSta0下独立非输入桌面，输入桌面前后保持Default。根入口的子进程桌面身份探针因Win32错误5被拒绝；因此不能据此声称Playnite实际画面在私有桌面，也不能证明用户桌面完全没有黑窗。用户截图没有提供可关联的HWND/PID，Playnite可见页面、真实UIA/输入/物理DPI和“编译/检查时闪屏”均待宿主验收。当前证据只证明受控入口进程自身退出0及桌面前后快照，**不构成“闪屏已解决”的结论**。

## 失败输出清理

仅删除本阶段超长路径失败的build output：`artifacts/gsc-b/round5-buildcmd-20261110-07c443176a6a47a0a7c63d5860ab5a59`，727文件、250目录、169,290,041 bytes。删除前核对仓库 `artifacts/gsc-b` 边界、路径规范化、重解析点、引用、占用和构建进程；使用PowerShell `Remove-Item -LiteralPath -Recurse -Force`。失败TRX/日志、成功输出、安装包、当前审计及任何用户/其它任务TestResults、存档、配置都保留。C/D盘删除前后观察值以及清单见 [cleanup.json](cleanup.json)。逐个TRX的相对路径、长度及SHA-256见 [trx-sha256-manifest.json](trx-sha256-manifest.json)。本批没有操作任何盘符、分区、卷或映射。