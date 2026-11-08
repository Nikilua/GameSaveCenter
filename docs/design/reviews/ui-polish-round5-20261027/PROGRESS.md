## 2026-11-09 U09-05：截图匹配的选框与长详情像素复核

从 `3dc59152fee9e2e7e18872c13cadaee2e1c28dc8` 扩大 Overview、Task Grid 与 Save History 的既有选框回归：测试夹具分别包含11/50/20行，覆盖150%进程DPI、约493–500 DIP窄视口、横向滚动、双主题和纵向滚动范围；另外为 Task detail 1120/1400 DIP 双主题长文用例加入可选 `RenderTargetBitmap` 捕获。未改生产 XAML、共享资源或业务代码。

本机 SDK8.0.423 / Release `scripts/build.cmd -SkipTests` exit0、0 warning/0 error；4个隔离WPF用例合计8/8通过、0失败、0跳过。Source validation通过；WPF静态审查限定26个生产XAML，0 errors/26 warnings/164 info。七张离屏图中，当前控件树的 Overview 时间、任务状态/进度、存档设备文字均在选框内；任务详情长失败文案在窄栏换行。该复核未重现越界，不能替代真实Playnite页面验收。完整编译摘要、TRX、桌面身份、PNG及SHA见[本批证据](evidence/U09-05-frame-pixel-check-20261109/README.md)。

用户本次仍报告闪屏，U09-05保持进行中。本批测试与控件截图均在WinSta0私有非输入桌面，没有执行会关闭/重启Playnite的安装启动入口；所附黑窗没有HWND/PID/desktop；本地命令进程所在窗口站的EnumWindows返回false、前台HWND为0，无法观察用户交互桌面，不能把空结果记成“没有窗口”。隐藏助手在非Explorer到cmd链路保留交互终端，代理构建会改为从启动前隐藏并重定向日志；尚未证实该路径就是用户黑窗。因此本次结果仅覆盖构建路径和选框像素，不能称用户闪屏已解决。精确本批源码SHA双SDK CI待推送后核对；整轮中文根入口完整链仍留给最终冻结候选。

## 2026-11-09 U09-04/U09-05：任务详情动效探针与再次闪屏反馈

用户再次表示仍会闪屏，按未解决问题继续跟踪。附件文件大小150,954 bytes、SHA-256 `67C3EBB175A78E5969A80137CFECB8C3F982C92507D14CEC868A202243159195`，原始时间2026-11-06 13:59:27 +08:00，与此前图像完全相同且没有HWND/PID/desktop。Windows Computer Use读取当前窗口时初始化失败（`failed to write kernel assets: 系统找不到指定的路径`），没有取得新窗口身份。稍后一次只读 `EnumWindows` 快照未发现当前可见的 Playnite/GameSaveCenter/testhost/build-console/WPF wrapper 匹配窗口，但这不是瞬态事件监听，不能据此宣称桌面无闪；详见本批 `current-window-snapshot.json`。本批没有运行安装/启动根入口，也没有重启Playnite。

补充U09-04任务详情面板同条件探针：使用U00 `5d0a2448d6315af61571d0d3fbecfd902e5344fc` 与生产源码基线 `0258ab3513c7d1bc95b8b622eead42a0fab30a76`，两套测试Harness共享同一 `Program.cs` 哈希并记录被替换的测试夹具身份。SDK8.0.423 Release两次编译均0 warning/0 error；两主题各12次打开/关闭，选中行与收起终态稳定。当前受控窗口开合动作到终态p95为348.09–415.58ms（打开）、250.16ms左右（关闭）；U00无对应动画且更快。当前Rendering回调间隔p95为124.78–149ms，是私有桌面的Dispatcher代理，不是DWM呈现帧；探针没有测累计分配和物理输入延迟，不据此得出性能通过。U09-04保持进行中。报告、build和桌面身份见[本批证据](evidence/U09-04-task-inspector-motion-20261109/README.md)。

本批再次运行 Render QA：SDK8.0.423 Release编译0/0，394 PNG、0问题行；RenderHarness在 `WinSta0\GscRenderQA_e9c34f8c3c3b4bba95237089c55f09d5`，输入桌面前后 `Default`。没有生产XAML/资源改动。GitHub基线 `0258ab35` 的 run [#1494](https://github.com/Nikilua/GameSaveCenter/actions/runs/42513349534) 已 completed/success：baseline job `128830946846` 与SDK10.0.401 job `128830946943` 均成功，三个artifact身份见[CI记录](evidence/U09-05-renderqa-private-20261109/ci-run-1494.json)。U09-06 required-checks/ruleset此前403，保持“远端待验”。计数仍5项已满足、71项代码完成待验收、3项进行中、1项远端待验；未操作盘符、卷、存档、媒体或云端数据。

## 2026-11-09 U09-05：隔离 Render QA 窗口；最终入口仍待验

用户再次报告闪屏。附件 SHA-256 `67C3EBB175A78E5969A80137CFECB8C3F982C92507D14CEC868A202243159195` 与 11 月 6 日附件相同，文件最后写入时间为 2026-11-06 13:59:27；无 HWND/PID/desktop，不能据此确认 11 月 9 日当前窗口身份。

补齐交互式 `scripts/render-qa.ps1`：先选取项目固定 SDK 8.0.423，再把 RenderHarness 放到 `WinSta0\GscRenderQA_<guid>` 私有非输入桌面，核验 station/desktop 与父子输入桌面前后身份；CI/service 的非交互路径保持原执行。`scripts/validate-source.py` 增加隔离守卫。本批未改生产 XAML。

精确源码提交 `0258ab3513c7d1bc95b8b622eead42a0fab30a76` 的 `scripts/build.cmd` Release exit 0：160份TRX、2030项、1962通过、0失败、68跳过、157个WPF类隔离；skip保持原原因且不计为通过。Render QA exit 0，报告有394张PNG、0条 `PROBLEM/FAILED/ERROR`，SHA-256 `3625C3992BF17CB84D7BA3F3776BF0808C84C8C9623D7252B217CDC176419751`。Default桌面WinEvent覆盖Render QA全程及WPF隔离测试中后段（监听器未覆盖最早几个类），各自SHOW/HIDE均为0，输入桌面保持Default。详细源码/SDK/报告/TRX摘要、监听范围与清理清单见[本批证据](evidence/U09-05-renderqa-private-20261109/README.md)。

后续对同一冻结构建的全部157份 WPF 逐类 JSON 记录复核，station 全为 WinSta0、desktop 均为各自 `GscWpfTest_<guid>`、Default 输入桌面前后保持不变、退出码均为0；清单见[逐类桌面身份](evidence/U09-05-renderqa-private-20261109/wpf-desktop-identities.json)。这补足窗口监听器未覆盖最早测试类的时序缺口，但只证明测试 runner 身份，不能识别用户黑窗。静态检查仍发现 Explorer 直接启动 `.cmd` 时存在“控制台先创建、随后才隐藏”的竞态可能；尚未复现，也不作为用户黑窗归因。

提交已推送；精确 `0258ab35` 的 `windows-build.yml` run [#1494](https://github.com/Nikilua/GameSaveCenter/actions/runs/42513349534) 已完成，baseline job `128830946846` 和SDK10.0.401 job `128830946943` 均success，job/artifact身份见[CI记录](evidence/U09-05-renderqa-private-20261109/ci-run-1494.json)。此前required-checks/ruleset接口403，不重复探测，U09-06仍远端待验。中文→英文根入口完整安装/启动链尚未对本轮最终冻结候选复验，留在80项最终候选执行；它会关闭并重启Playnite，须在隔离桌面。U09-05保持进行中，不声称用户黑窗已完全解决。计数维持5项已满足、71项代码完成待验收、3项进行中、1项远端待验。未操作盘符、存档、媒体或云端数据。
## 2026-11-08 U09-05 用户继续报告闪屏：根入口复核

用户再次指出编译期间仍有黑窗。截图没有 HWND/PID/桌面身份；本机附件文件元数据显示时间为 11 月 6 日，无法归因 11 月 8 日运行。冻结提交 `e42da2a5bd33e8988acb0b31218bdd66b3d96048` 的中文完整入口已在 WinSta0 私有非输入桌面实跑，根 cmd 直接退出码0；SDK8.0.423/Release，160份TRX、2030项、1972通过、0失败、58个保留原因的skip，157个WPF类隔离执行，发布/打包/安装身份均核对。Playnite 主窗只在该私有桌面枚举到，根链前后输入桌面均为Default；这不是对所有时段/其它启动方式的事件监测，用户黑窗仍未归因。详细退出码、安装与闪屏边界见[本次根入口复核](evidence/U09-05-root-e42da2a5-20261108/README.md)。

安装入口会先关闭 Playnite/Worker，再安装并启动 Playnite；反复调用会重启宿主。后续每批编译/行为检查只用独立 `scripts/build.cmd` 和隔离检查，不再反复运行完整入口。完整链退出码门禁已满足，但用户黑窗的真实来源与整个时段的零闪证据仍未验，U09-05保持进行中。当前文档提交 `e42da2a5` 的 CI #1492 双 SDK jobs 均成功；required-checks/ruleset接口此前403，U09-06仍远端待验。任务计数5项已满足、71项代码完成待验收、3项进行中、1项远端待验。

## 2026-11-07 U09-05 隔离 WPF 测试窗口修复：0feff185

- 交互式本机的逐类WPF测试改到WinSta0私有非输入桌面，并核验每个子进程的station/desktop与输入Default。精确提交 `0feff18501a5ec971e3d7fb2ba212e8d0d28e0ac` 的独立SDK8完整 `scripts/build.cmd` 与CI双SDK通过；该修改没有改业务UI/断言/skip。
- 随后的用户截图文件时间为11/6，早于此提交，且无HWND/PID；不能据此识别来源。隔离测试证明受控WPF用例不再在输入桌面创建窗口，不等于实际Explorer启动或Playnite页面已验收；U09-05保持进行中。

## 2026-11-08 Round5 U09-05 R08 按压采样复核

精确提交 `b573fea7210d810eef221721395b007da96d7458` 的本机独立完整 `scripts/build.cmd` 在第77/157个WPF类失败于 `ProductionButtonHoverAndPressReverseFromTheirRenderedValues` 首次按压中间值断言。当时已执行1469项：1439通过、1失败、29跳过。原TRX保留在[失败证据](evidence/U09-05-no-flash-20261107/r08-press-capture-20261108/tests/full-build-b573-pre-fix-failure.trx)。同一旧测试程序集随后单例1/1、R08类4/4通过，因此未把测试或全量失败标记为已通过。

只调整测试夹具：PressedOverlay本地采样时窗1600ms，按压状态超时有界覆盖，并在开始探针前等待最长夹具时窗。生产模板、MotionTokens、业务、动画断言和skip未改。当前源码blob `c9432aab21e1b5559da1582e0add0d02c7fc4baf` 的SDK8.0.423/Release `scripts/build.cmd -SkipTests` 编译0 warning/0 error、XAML26/26；Source Validation与`git diff --check`通过。精确测试程序集SHA `BF037240E27CF8631C74A0C621EFFA061EAD679B578F783EC244D888EBD2C4B0` 下，目标用例在三个独立WinSta0私有非输入桌面各1/1通过，合计3/3、0失败、0skip，输入Default前后保持不变。TRX与源码、程序集、运行身份见[本批证据](evidence/U09-05-no-flash-20261107/r08-press-capture-20261108/README.md)。完整R08类和完整独立构建须在修正提交上重验。

精确旧提交 `b573fea` 的GitHub run [#1489 / 42349851252](https://github.com/Nikilua/GameSaveCenter/actions/runs/42349851252) 已成功：SDK基线job `128335451147`、SDK10.0.401 job `128335451280`；产物身份见同一证据README。该CI不覆盖当前修改。`WinSta0\Default`连续600秒顶层show/hide监听0次事件，但仅覆盖该次WPF全测后段；不等于用户看到的黑窗已定位或实际启动路径已消失。当前仍5项已满足、71项代码完成待验收、3项进行中、1项远端待验。U09-05黑窗、根入口完整安装启动与新SHA CI待验；U09-06 required-checks/ruleset读取此前403，保持远端待验。未操作盘符、存档、媒体删除或云端写入。

用户再次报告黑窗后，确认此前 WPF 测试虽设为 1% 透明，仍在 `WinSta0\Default` 创建窗口。提交 `0feff18501a5ec971e3d7fb2ba212e8d0d28e0ac` 将交互式本机 WPF 测试类逐类放到 WinSta0 非输入桌面；子进程检查 station、desktop 和 input desktop。测试断言与 skip 未改。非交互 CI 保留完整测试路径。

精确源码 SDK 8.0.423 / Release 的独立 `scripts/build.cmd` 完整链 exit 0：160 TRX、2030 项、1962 通过、0 失败、68 个既有 skip；157 个 WPF 类均隔离运行。Default 桌面监测全程 0 次窗口显示事件。Q10 动效定向回归在 `WinSta0` 私有桌面 1/1、0 skip，输入桌面前后均为 Default。PowerShell 语法、Source Validation、`git diff --check` 和 WPF 静态审查 exit 0（26 XAML、0 errors、218 warnings、1440 info）；本提交不改生产 XAML/资源。证据见 [U09-05 隐藏 WPF 测试复核](evidence/U09-05-hidden-wpf-20261107/README.md)。

中文根入口完整编译、测试、Worker 发布、打包、安装身份核验及 Playnite 启动均完成。摘要为 2030 项、1972 通过、0 失败、58 skip，157 个 WPF 类隔离；ZIP/PEXT 各 45,739,058 bytes、SHA-256 `CA3374AE521C6E4D4047F8B73D52E7469DADB0CACFDA5144CBFE487BB0617A94`，安装六个程序集含精确源码身份。入口日志到达 `[OK]` 成功分支，其源码随后 `exit /b 0`，中文包装器原样转交 errorlevel；但外层 PowerShell 日志捕获因 Playnite/Worker 继承重定向句柄，没有生成自己的最终退出记录，故不把该包装器观察记作已完成。官方 `--shutdown` 等待超时，未强制结束进程。Default 桌面监测覆盖构建、测试及启动收尾，0 次窗口显示事件。

精确 SHA GitHub `windows-build.yml` run [#1486](https://github.com/Nikilua/GameSaveCenter/actions/runs/42326547974) completed/success：SDK 8.0.423 job `128263060304` 与 SDK 10.0.401 job `128263060528` 均通过；artifact ID、digest 见上方证据 JSON。required-check/ruleset 读权限此前返回 403，不重复试探，U09-06 仍“远端待验”。用户截图无 HWND/PID，本批受控运行的零事件不能反向识别截图黑窗，也不能证明所有手动启动路径都绝无闪现；因此 U09-05 继续进行中。U09-04、U09-08 继续推进。状态为 5 项已满足、71 项代码完成待验收、3 项进行中、1 项远端待验。未做真实存档恢复、媒体删除或云端写入。

## 2026-11-07 U09-05 用户再次报告黑窗：重新进行中

用户再次提供了构建期间出现黑色窗口的截图。截图中能看到黑色窗口覆盖 Playnite/GameSaveCenter 界面，但没有 HWND、PID 或所属桌面信息，不能确认窗口由 Playnite、WPF 测试宿主或控制台创建。此前 25 ms 轮询只覆盖了特定进程和受控运行；它不能排除短于采样间隔的窗口，也没有识别用户本次截图中的窗口。因此撤回“U09-05 已满足”的当前结论；旧运行数据仍作为历史证据保留，当前状态重新设为“进行中”。

本轮先将 WPF 测试放进 Codex 非活动桌面，R08 和 Q10 动画用例分别因动画时钟未前进失败；该运行环境不适用于动效验收，失败保留且未改断言/skip。进一步确认真正输入桌面为 `WinSta0\Default`，在该桌面使用 `CREATE_NO_WINDOW` 重跑 Q10 后 1/1 通过。WinEvent 实测仅看到 8 个无标题 testhost `HwndWrapper`，没有 Console 或 Playnite 窗口；测试夹具本身为 `Opacity=0.01`、`ShowActivated=false`、`ShowInTaskbar=false`。静态截图仍无 HWND/PID，故不据此声称识别了截图中的历史窗口。

定位到真实根入口 `GameSaveCenter-Run.cmd` 的成功及失败分支都会 `pause`，会使命令窗口留在桌面；已移除等待并给隐藏 PowerShell 子进程加 `-WindowStyle Hidden`。独立 `scripts/build.cmd` 子进程也隐藏；参数仍以 `%*` 原样转交。SDK8.0.423 隐藏启动参数 smoke、source validation 和 `git diff --check` 通过。精确提交 `62a363d3` 的 GitHub run #1483 两个适用 job 均成功；artifact 身份见 [CI #1483](evidence/U09-05-no-flash-20261107/ci-run-1483.json)。本次脚本修改候选的独立 build.cmd、中文安装启动完整链及瞬态窗口事件复核仍待执行；因此 U09-05 继续进行中。详情见[本次桌面窗口观测与原始诊断](evidence/U09-05-no-flash-20261107/followup-window-20261107/README.md)和[用户反馈与复核边界](evidence/U09-05-no-flash-20261107/followup-user-feedback.md)。

U09-05 与 U09-04、U09-08 当前进行中；总计 5 项已满足、71 项代码完成待验收、3 项进行中、1 项远端待验。真实存档恢复、媒体删除和云端写入仍不在本轮范围。

## 2026-11-07 U09-05 验收期间黑窗闪现复核：76e5670f

用户反馈验收期间仍出现黑窗。单次前台快照不足，因此在 `Default` 桌面以 25 ms 间隔观察完整运行窗口；安装脚本启动 Playnite 的路径改为 `ProcessStartInfo` 且 `UseShellExecute = false`，入口修订号为 `DEV-INSTALL-010`，退出码测试增加对应源码守卫。

精确源码 `76e5670f02a3bf85432c312a27d1ae51fc379678` 的中文根入口完整链 exit 0，安装包和六个已安装程序集均核对通过；独立 `scripts/build.cmd` exit 0。SDK 8.0.423 / Release 下，根链 2030 项、1972 通过、0 失败、58 跳过；独立构建 2030 项、1962 通过、0 失败、68 跳过；两次各运行 157 个隔离 WPF 测试类。跳过项均按原 TRX 记录并不计为通过。根链先有一次普通权限写用户 Playnite 扩展目录失败，保留该失败事实；随后授权提升的完整重跑通过。本机仅跟踪源码文件干净；既有未跟踪证据和 TestResults 保留。

覆盖两个根链运行和独立构建的 `Default` 桌面探针从 2026-11-06T18:05:55.628Z 持续到 19:52:36.302Z，25 ms 间隔，仅记录目标窗口出现/消失；目标进程集在该期间没有 `SEEN` 或 `GONE` 事件。成功根链的 Playnite 主窗口确实可见，但所属桌面是非活动的 `GscR5Elev_e8bce0044f4a4413bbcb73faa69d1c31`，不是 `Default`；独立构建也运行在非活动桌面。采样不能排除短于 25 ms 的窗口，也不能回溯识别先前截图中的 HWND，因此结论限定为本次完整验收没有观察到目标窗口进入日常桌面。详情见[最终结果与边界](evidence/U09-05-no-flash-20261107/BASELINE.md)和[桌面采样](evidence/U09-05-no-flash-20261107/desktop-launch-audit.json)。

源码 run [#1481](https://github.com/Nikilua/GameSaveCenter/actions/runs/42265616989) 对应精确 SHA，baseline SDK 8.0.423 与 SDK 10.0.401 两个适用 job 均 completed/success，artifact ID 与 digest 见[CI 证据](evidence/U09-05-no-flash-20261107/ci-run-1481.json)。branch required-check / ruleset 接口沿用此前 403，不重复试探，U09-06 仍“远端待验”。U09-05 标记已满足；U09-04、U09-08 继续进行中。当前计数为 6 项已满足、71 项代码完成待验收、2 项进行中、1 项远端待验。此结论不代表真实插件页、外部 UIA、实体键盘/IME、物理 DPI 或历史黑窗 HWND 已验明；真实存档恢复、媒体删除和云端写入未执行。

## 2026-11-07 Round5 U09-05 / U09-06 冻结候选门禁：2bb24aab

冻结源码 `2bb24aabede7d98ecbed47c967df24418bcd8312` 的独立 `scripts/build.cmd` 与真实中文入口 `GameSaveCenter-一键构建安装运行.cmd` → `GameSaveCenter-Run.cmd` → `scripts/dev-install-run.ps1` 均以 SDK 8.0.423、Release 完成，入口/根链退出码均为 0；两次工作树跟踪文件均干净。每次 160 份 TRX，2030 项、1972 通过、0 失败、58 跳过；157 个 WPF 隔离组全部完成。58 个跳过原因未更改：57 条属于已撤销的“今日工作台”架构断言，1 条是默认 opt-in 的物理鼠标指针测试。两次诊断摘要、入口日志、原始 TRX 与哈希见 [本机门禁证据](evidence/U09-05-2bb24aab/BASELINE.md)。

中文根链生成的 PEXT 为 45,739,381 bytes，SHA-256 `2B1BBE74ADDAE42B7A86830C2CA5E12C0F28C32859724725D038442D4198445F`；安装清单 0.6.73、DLL 0.6.73.0，六个已安装程序集逐一与包内哈希相同。根入口日志确认安装及 Playnite 启动完成。构建/启动在非活动桌面，输入桌面仍为 Default；启动后的前台窗口清单只含 ChatGPT 与 FlClash，没有构建控制台或 Playnite。该清单是检查时点快照，不能证明前后所有瞬时帧都没有闪现；按你指出的黑窗问题继续保持所有构建和宿主启动远离日常前台。关闭本次宿主的官方 `--shutdown` 调用等待超时，未强制结束后台进程；关闭状态不计作通过。

精确 SHA 的 GitHub [run #1479](https://github.com/Nikilua/GameSaveCenter/actions/runs/42244931836) 总体 completed/success：基线 job `127984550172` 与 SDK 10.0.401 job `127984549862` 均通过，诊断/预览 artifact 与 SDK10 诊断 artifact 的身份及 digest 见 [CI 记录](evidence/U09-05-2bb24aab/ci-run-1479.json)。required-check / ruleset 端点先前 403，本轮未重复探测，故 U09-06 仍是“远端待验”。先前 04b6457d 的真实宿主审计捕获嵌入首页和设置页、0 条高等级记录；本提交没有重新做实体 UIA、键盘/IME、物理多屏 DPI 或呈现帧验证。未执行真实存档恢复、媒体删除或云端写入。U09-04、U09-08 继续进行中；状态计数仍为 6 项已满足、71 项代码完成待验收、2 项进行中、1 项远端待验。

[独立 build.cmd 与中文根入口证据](evidence/U09-05-2bb24aab/BASELINE.md) · [320份 TRX 清单及 SHA-256](evidence/U09-05-2bb24aab/trx-manifest.json)

## 2026-11-06 Round5 U04 动效重入采样与宿主审计复核

04b6457d 的独立构建退出 0，Windows CI #1478 两个 SDK 通过，三个产物匹配源码。中文根入口退出 1，动效重入用例在固定等待后读到已完成的位移 0，安装未执行。改为在 WPF 渲染回调内采到符合原范围的中间帧时立即重入并取消，原范围和连续性断言保留。SDK 8.0.423 定向类在非活动桌面 9/9 通过，0 跳过。新候选完整链及远端待验。

04b6457d 的真实宿主审计退出 0，嵌入首页与设置均匹配源码。相同宿主视口及 64 个合成游戏条件下，高等级记录由 2 条降为 0 条。外部 UIA、实体输入和帧呈现仍待验。

[原失败 TRX](evidence/U09-motion-reentry/before.trx) · [定向修复 TRX](evidence/U09-motion-reentry/after.trx) · [宿主摘要](evidence/U09-motion-reentry/host-audit-04b6.json)

## 2026-11-06 Round5 宿主审计输出隔离

上次审计在真实宿主启动前写出了 CommitSha 为 unknown 的摘要和两条高等级记录。脚本此前在安装测试前就设置 GSC_REAL_HOST_AUDIT。现在安装和测试不继承这个输出路径，只有正式启动的 Playnite 进程继承它。子进程启动后恢复调用方环境值，启动失败也恢复。PowerShell 语法检查与模拟安装、退出码传播、宿主启动和异常恢复检查已通过。没有修改业务代码、测试断言或跳过项。冻结提交的完整链、双 SDK CI 和隔离宿主复核待完成。

## 2026-11-06 Round5 真实 Playnite 嵌入页面抽查

冻结源码 f0d2b8f1 使用独立资料、独立 Worker 管道和 64 个合成游戏。首次配置初始化未正常退出，随后用 Playnite 官方退出命令关闭并确认资料根。第二次在过深的测试临时目录中有三条设置恢复测试失败。第三次通过 TestTempRoot 指定较短目录，三条用例均通过，审计最终退出 0。

宿主报告实际 DashboardView 已嵌入 Playnite，未使用专用审计窗口，源身份匹配，视口 1313.33×898 DIP，DPI 150%，使用宿主当前主题。核对了任务空态截图。首页、存档和任务视口已归档。任务队列没有记录，这组截图无法核验选中行边框。审计输出还保留两条在安装测试期间生成的高等级记录。其摘要曾为 unknown 且未捕获 Dashboard，之后真实宿主才写出匹配源码的元数据和图像，不能把整个审计目录当作无问题通过。外部 UIA 未定位侧栏入口，单显示器环境未验证跨屏，实体输入、IME 和呈现帧仍待验。

[真实宿主元数据](evidence/U09-host-f0d2/metadata.json) · [审计摘要](evidence/U09-host-f0d2/summary.json) · [任务页截图](evidence/U09-host-f0d2/tasks.png) · [外部 UIA 边界](evidence/U09-host-f0d2/host-window-exposure.json)

## 2026-11-06 Round5 U09-02 焦点边界诊断

R05 失败信息增加 Tab 步数、前后控件名称、模板宿主和窗口激活状态，原焦点断言与范围保持。首次定向执行缺少 GscBuildCommit，源码身份门禁拒绝该次结果。补齐真实身份后在非活动桌面编译及复核 5/5 通过，0 跳过。偶发焦点逸出未复现，完整构建仍待验。 [定向 TRX](evidence/U05-02-reduced-motion-focus/focus-boundary-diagnostics.trx)

## 2026-11-06 Round5 非活动桌面构建与焦点诊断

Win32 隔离探针证实父、子进程和实际 WPF HWND 位于非活动桌面，输入桌面保持 Default。冻结候选 c44abbac 的独立 scripts/build.cmd 在该环境运行，SDK 8.0.423，诊断 20261106T075942754Z-00b1925a。第 50 个 WPF 隔离组 R05FocusBoundaryBehaviorTests 失败，测试 1380 项，1361 通过、1 失败、18 跳过。正常尺寸游戏选框在 MoveFocus 遍历时焦点落到选框外。独立入口退出 1，中文根入口未运行。同程序集在另一个非活动桌面定向复核 5/5 通过，仍未定位完整运行中的偶发条件。保留失败，定向通过不计作完整链通过。 [隔离探针](evidence/U05-02-reduced-motion-focus/desktop-probe.txt) · [失败 TRX](evidence/U05-02-reduced-motion-focus/desktop-focus-fail.trx) · [定向复核 TRX](evidence/U05-02-reduced-motion-focus/desktop-focus-recheck.trx)

## 2026-11-06 Round5 Windows CI #1466 Worker 夹具修复

候选 4719a59c 的 [SDK 8 作业 127733032844](https://github.com/Nikilua/GameSaveCenter/actions/runs/42169279866/job/127733032844) 在打包步骤的 Worker 集成测试失败。原始日志显示 400 通过、1 失败。Rclone 不完整传输分类用例在 Dispose 删除临时 gamesavecenter.db 时遇到文件占用，分类断言未报告失败。诊断产物 13481573092。SDK 10 作业 127733033114 的 8 步通过，诊断产物 13480957381。

将分类与退避的纯逻辑用例移到无数据库夹具的 CloudRetryPolicyTests。持久化用例仍使用原数据库夹具。SDK 8.0.423 Release 定向测试 12/12 通过，0 跳过。后续候选双 SDK 及桌面隔离环境的本机完整链仍待验。 [Worker 定向 TRX](evidence/U05-02-reduced-motion-focus/worker-policy.trx)

修复提交 `c44abbac647749522184913bf7ae3affa26c91d8` 的 [Windows CI #1469](https://github.com/Nikilua/GameSaveCenter/actions/runs/42174313887) 已通过双 SDK。SDK 8 作业 127749131247 通过 15/15 步，包括打包和上传。SDK 10 作业 127749131015 通过 8/8 步。预览包 13485421973、基线诊断 13485277562、SDK 10 诊断 13483612639 均对应此 SHA。当前候选的本机完整链、真实 Playnite 输入和分支必需检查名单仍待验。

## 2026-11-06 Round5 本机构建中断与桌面隔离要求

候选 4719a59c 首次独立构建退出 1。构建期间共享 checkout 的 HEAD 前进到 d7f82e44，R06 源码身份门禁拒绝程序集与当前源码混用。此前焦点相关测试类 14/14 通过。随后从 4719a59c 的独立固定检出重新执行，但 WPF 测试窗口仍出现在用户桌面。已终止该次构建进程树，中文安装启动入口未运行。

隐藏构建控制台不足以隐藏 WPF 子窗口。当前桌面只执行不会创建应用窗口的操作。先通过 GitHub runner 执行编译、测试与渲染。本机完整链需在与日常桌面隔离且能满足原测试及安装核验的环境中运行，确认隔离前不启动 WPF 用例、渲染夹具或 Playnite。两次未完成执行均不计通过。 [身份失败 TRX](evidence/U05-02-reduced-motion-focus/identity-failure.trx) · [定向门禁与入口身份](evidence/U05-02-reduced-motion-focus/ui-gates.json)

## 2026-11-06 Round5 U05-02 / U09-02 任务详情收起焦点

关闭动效时任务详情立即收起，原分支跳过焦点归还。补齐后，收起按钮和 Escape 共用的关闭路径会把焦点还给“查看任务详情”。SDK 8.0.423 Release 定向行为测试先失败（焦点仍在详情 ScrollViewer），修复后 1/1 通过。源码检查通过，WPF 静态审查 0 错误。离屏渲染、冻结候选完整链与双 SDK CI 待核，实际 Playnite 键盘及外部 UIA 仍待宿主验收。 [修复前 TRX](evidence/U05-02-reduced-motion-focus/before.trx) · [修复后 TRX](evidence/U05-02-reduced-motion-focus/after.trx)

## 2026-11-06 Round5 U09-04 收起详情后的滚动器归属

收起详情并同步布局后，双版本全部样本都选中了表格滚动器。9 个对齐周期中，候选的组合场景更慢，具体测法、边界与原始报告见[滚动器归属证据](evidence/U09-04-scroll-owner-20261106/README.md)。U09-04 仍进行中。

## 2026-11-06 Round5 U09-04 同夹具正反序复测：U00 / a13a7dbb

在 SDK 8.0.423 下，用 U00 `5d0a2448d6315af61571d0d3fbecfd902e5344fc` 和候选 `a13a7dbbe27f4cb90d37057163e1481aa8abf355` 做了两组顺序相反的 60 秒活动 + 30 秒稳定观察，统一 1040×700 DIP、双主题、七个假工作区和固定样本UTC。窗口隐藏且位于屏幕外，复用相同Harness源码。两个候选运行均 actionFailures=0。

活动段托管累计分配下降12.64%–13.36%；全局周期P95两组方向相反，不能判断稳定回退。最大滚动器 `scroll_offset` P95 两组均上升（16.61–17.65 → 29.42–31.60 ms），详情收起P95从10.67–10.85升到18.99–19.68 ms。后续逐页采样将滚动差异定位到Task页，但未记录滚动器身份；详情收起后按 ScrollableHeight 选最大值可能混用 DIP 与逻辑项，不能归因外层或排除表格；已有TaskGrid专项Light 16.526→16.409 ms、Dark 16.498→16.499 ms。逐页样本仅约5–7个且出现分散的Dispatcher长尾，尚未定位到单一生产控件/源码因果，不据此延长动画或盲改布局。

候选动效回调探针一次深色中点失败、精确重试通过；重试有一个681.6 ms `CompositionTarget.Rendering`回调间隔，另一轮正反切换约34.5/36.7 ms。该事件不是合成器真实呈现帧，物理输入响应、Playnite前台ETW/DWM帧仍待宿主验收，因此U09-04继续“进行中”。完整指标、原始报告/失败证据、固定Harness源码哈希见[本批复测证据](evidence/U09-04-aligned-20261106/BASELINE.md)和[`metrics.json`](evidence/U09-04-aligned-20261106/metrics.json)。无生产UI源码变更，未执行存档恢复、媒体删除或云写入。

本机中文根入口完整链及独立`scripts/build.cmd`由冻结候选`a13a7dbb`的既有验收覆盖；精确源码CI #1461 两SDK已成功。文档提交`5cb4a68c0eb843c3c6657fb6cdc84cfac9aade93`对应CI #1462（run 42127707158）现已completed/success：SDK8 job `127596045500` 15/15步、SDK10 job `127596045343` 8/8步成功，三个 artifact ID/digest 见[`ci-run-42127707158.json`](evidence/U09-04-aligned-20261106/ci-run-42127707158.json)。唯一阶段目录`.tmp/round5-u09-04-aligned-20261106`已归档核验后清理，20,405文件/1,010,046,290 bytes，净释放D盘1,058,009,088 bytes；精确路径、边界和C/D前后空间见[`cleanup.json`](evidence/U09-04-aligned-20261106/cleanup.json)。required-check名单仍因既有403“远端待验”，不重复请求。当前80项状态仍7项已满足、71项代码完成待验收、U09-04进行中、U09-06远端待验。

## 2026-11-06 Round5 U09-05/U09-08 冻结候选最终门禁：a13a7dbb

冻结源码候选 `a13a7dbbe27f4cb90d37057163e1481aa8abf355`。独立 `scripts/build.cmd` 以 SDK 8.0.423 / Release 退出0：160份TRX、2030项、1962通过、0失败、68跳过；真实中文入口 `GameSaveCenter-一键构建安装运行.cmd` → `GameSaveCenter-Run.cmd` 完成编译、测试、win-x64 Worker发布、ZIP/PEXT打包、安装身份核验和Playnite启动，入口及外层退出码均为0：160份TRX、2030项、1972通过、0失败、58跳过，157/157个WPF隔离组完成。跳过的原因和数量均保留在TRX，且不计通过。320份TRX及逐文件SHA见[evidence/U09-05-final-a13a7dbb](evidence/U09-05-final-a13a7dbb/FINAL-ACCEPTANCE.md)。

Source Validation、XAML结构检查、WPF静态审查通过（26 XAML、0错误、26 warning、164 info）；精确SHA Render QA为394张、0 `PROBLEM`。ZIP/PEXT各45,739,077 bytes，SHA-256相同：`DD9156E61EC1132B65A9FC906A9D3E58B731C50232B47625DFEE2AF19F450045`。安装清单0.6.73、DLL 0.6.73.0，六个程序集身份带候选SHA；入口、日志、TRX、包、安装和 UI_CHANGE_GATE 细节见[`local-gates.json`](evidence/U09-05-final-a13a7dbb/local-gates.json)。

根入口启动的Playnite加载 `GameSaveCenter 0.6.73.0` 并观察到3个游戏。构建控制台与Playnite窗口句柄均为0；没有把窗口带到用户桌面。实际插件页面没有显示或捕获，所以页面视觉、外部UIA、实体键盘/IME、物理DPI/跨屏仍待宿主验收。仅结束本次入口启动的无窗口Playnite实例及其经过路径/父进程核对的当前扩展Worker。此次未操作 `subst`、磁盘管理或盘符；未执行真实存档恢复、媒体删除或云端写入。

精确源码GitHub [run #1461](https://github.com/Nikilua/GameSaveCenter/actions/runs/42110142797) completed/success：SDK 8.0.423 job `127535228072` 的15/15步及SDK 10.0.401 job `127535227511` 的8/8步全部成功；三个artifact身份见[`windows-ci.json`](evidence/U09-05-final-a13a7dbb/windows-ci.json)。required-check/ruleset读取现有权限返回403，不重复请求；U09-06仍“远端待验”。

U09-05本机最终验收通过；U09-08完成80项状态审查，没有把未验项目标为结项。整轮状态仍为7项已满足、71项代码完成待验收、U09-04进行中、U09-06远端待验。U09-04将继续查找/重建可比U00实际帧时和累计分配证据；不以离屏逻辑测量代替Playnite帧，也不重复试探required-check权限拒绝。

## 2026-11-04 Round5 U09-05/U09-08 本机最终验收：292f3bc6

冻结候选 `292f3bc63a8ab08c57505d50ac5ba68b6c0d431a` 的独立 `scripts/build.cmd` 与真实中文入口均按 SDK 8.0.423 / Release 完成。独立入口 exit 0，run `20261104T030507692Z-10c2f177`；中文入口 `GameSaveCenter-一键构建安装运行.cmd` → `GameSaveCenter-Run.cmd` 根进程 exit 0，安装 run `20261104T060044161Z-375720ee`，诊断 run `20261104T060046800Z-ba7cae6d`。

两入口各 160 份 TRX、2030 项、0 失败/错误/超时/中止。独立 build 为1962通过/68跳过；中文完整链为1972通过/58跳过，157/157个WPF隔离组完成。完整链58个跳过为57条已撤销“今日工作台”断言和1条鼠标opt-in；独立入口另有10条Named Pipe环境限制，完整授权入口中这10项已通过。跳过不计通过。包 ZIP/PEXT 各45,739,194 bytes、SHA-256 `91F4C4FCAB4CF88DC9C3D5E2921AF9A4055A550F36E9B450639B7EDE6D29BD1E`。

安装清单0.6.73、DLL文件版本0.6.73.0；Plugin DLL SHA-256 `D066E97586A4FA55B4851127ACB70A3E5C001CD011653188D6AFF47BE03D80E3`，Worker DLL SHA-256 `C8A5C7B86D0CAA619BC1361A72DA9D1B93C39BFFD26225A5F154C699B517CA1B`，身份均含候选SHA。Playnite PID 50956保持响应，扩展日志确认 `0.6.73.0` 已加载并观察到3个游戏。窗口标题“Playnite”在非活动隐藏桌面枚举到；没有切换桌面或捕获实机图。收尾辅助程序未能通过WM_CLOSE关闭该实例，未强杀、未启动重复实例；这个辅助程序退出1，中文根入口本身仍为exit 0。

精确源码 CI run [#1454](https://github.com/Nikilua/GameSaveCenter/actions/runs/41742480073) 已成功：SDK8.0.423 job126287170658、SDK10.0.401 job126287170119。required-check/ruleset读取沿用此前403，不重试，U09-06标记远端待验。

U09-05与U09-08完成。80项状态更新为7项已满足、71项代码完成待验收、U09-04进行中、U09-06远端待验。U09-04缺U00同条件真实帧时与分配数据；真实插件页视觉检查、外部UIA、实体键盘/IME、物理DPI/跨屏仍待宿主验收。U06-02/U08-08状态仍为代码完成待验收。未运行真实存档恢复、媒体删除或云端写入。详见[本机最终门禁](evidence/U09-05-final-292f3bc6/FINAL-ACCEPTANCE.md)及[结构化身份](evidence/U09-05-final-292f3bc6/local-gates.json)。

阶段清理仅删除已由诊断摘要确认归属本阶段且被当前构建替代的三个旧 `artifacts/gsc-b/` 编译输出（2485文件/626,324,030 bytes）和本批一次性隐藏桌面启动脚本；当前构建87dcfccd、ZIP/PEXT、TRX、诊断及失败日志保留。删除清单和前后空间见[cleanup.json](evidence/U09-05-final-292f3bc6/cleanup.json)。按用户要求解除R:的SUBST映射，底层C盘目录保留，C/D盘符未改。
## 2026-11-04 U06-02/U08-08 用户框外文字跟进：292f3bc6

针对三张新截图修复最近任务时间、存档行最右侧文字和任务队列状态/详情落在选框外的问题。提交 `4ded0f52` 修正共享行视口裁切与选框范围，`3e98148e` 补充缺少祖先滚动绑定时的配置内边距回退，`292f3bc6` 保持 Overview 框线与列表行内容对齐；三次均为中文提交且已推送 main。选框没有把内容向左挤，也未改变命令、Binding、UIA、键盘输入、虚拟化、主题或现有滚动条。

精确候选 Source Validation / XAML structural / WPF static review 均 exit 0；WPF 扫描 26 XAML、0 errors、26 warnings、164 info。R06 selection state、R10 recent access 定向用例均 3/3；SDK 8.0.423 `scripts/build.cmd` exit 0：160 TRX、2030项/1962通过/0失败/68 skip（57撤销架构断言、1鼠标opt-in、10受限环境Named Pipe；skip不计通过）。完整Render QA双主题多尺寸共394 PNG、0 PROBLEM；六张关键页图固定1040×700、Light/Dark，与全部TRX见[evidence/U06R5-2911](evidence/U06R5-2911/BASELINE.md)。

中文入口 `GameSaveCenter-一键构建安装运行.cmd` 已确认转交 `GameSaveCenter-Run.cmd`。精确候选整链此前在构建前C盘空间预检退出，exit 1（当时可用3,473,227,776 / 4,294,967,296 bytes）；归档核验后只清理隔离checkout本阶段 `artifacts/`、`.tmp/` 内5个输出（1,855文件、281,163,222 bytes），释放285,892,608 bytes。2026-11-04 00:58 UTC复测C盘可用3,964,444,672 bytes，仍差330,522,624 bytes；用户已确认能释放至少463 MB，待空间实际达到脚本原门槛后重跑，没有降低预检。完整安装启动及安装身份仍待验。

精确源码 push run [#1454](https://github.com/Nikilua/GameSaveCenter/actions/runs/41742480073) 已 completed/success，验证 SHA `292f3bc63a8ab08c57505d50ac5ba68b6c0d431a`。基线 job `126287170658`（SDK 8.0.423）15/15 步、SDK 10.0.401 job `126287170119` 8/8 步均成功，含编译测试、Render QA、WPF 静态审查、扩展包及诊断上传。预览包 artifact `13285597578`（90,794,124 bytes，sha256 `1d47945c1059545e59231a5728a18b697ce95aa73698ffa0656a086195e9c65c`）；基线诊断 `13285722410`（2,043,876 bytes，sha256 `a3c9f3596578f1cd539d8616219848b00d0aa14e5aba8b9a9fc842e58704e453`）；SDK10诊断 `13283196951`（976,992 bytes，sha256 `c4237ec546e26158cd9aa609ef9a442763216ed0938aac423b3c90f9f21f2b0e`）。artifact 均绑定候选 SHA，身份见 [CI 记录](evidence/U06R5-2911/ci-run-41742480073.json)。required-check/ruleset只读此前403，不重复请求，required-check 名单仍远端待验。真实Playnite页面/UIA/实体输入IME/物理DPI仍待验，未运行真实恢复、媒体删除或云端写入；状态计数5已满足、71代码完成待验收、U09-04/U09-05/U09-08进行中、U09-06远端待验。

## 2026-11-04 Round5 证据归档修复与 Windows 双 SDK CI 验收（f203f15f）

文档证据提交 967e07e1c2d2c530620fee4300e8566e02b318b4 的 [run #1451](https://github.com/Nikilua/GameSaveCenter/actions/runs/41681642551) 两个 job 都在 Windows checkout 因重复嵌套 TRX 路径 `Filename too long` 失败；源检查、测试、渲染和打包未运行且无 artifact，原始 job 日志链接见[失败诊断](evidence/U06-02-3aa-20261104/ci-run-41681642551-failure.json)。

修复提交 f203f15f18bf8ce5f84c38ae7e3f03a407ab3a3e 缩短证据目录并展平 320 份完整链/独立 build 与 6 份 focused TRX；字节、SHA-256 和 manifest 均复核一致。精确 [run #1452](https://github.com/Nikilua/GameSaveCenter/actions/runs/41684052918) completed/success：主 job 126084964467 为 15/15 步，SDK 10.0.401 job 126084964958 为 8/8 步，包含编译测试、Render QA、WPF 静态审查、扩展包和诊断上传；三个 artifact ID/大小/digest 见[CI 成功身份](evidence/U06-02-3aa-20261104/ci-run-41684052918.json)。源码候选仍冻结在 3aaae534。U09-06 的 branch required-check/ruleset 读取仍受既有 403 限制，继续标记“远端待验”，不重复试探。
## 2026-11-04 Round5 U06-02/U08-08 选框完整包裹与 U09 门禁（3aaae534）

用户附图确认 Overview 日期时间、Save History 末列和 Task 队列状态文字均未被选中框完整包住。此前收窄 DataGrid 框宽到 4 造成框边贴住行视口，破坏 R23 的 4 DIP 滚动轨道间隙。提交 3aaae534 将共享生产 DataGrid 与 Dashboard 宽度参数恢复为 8（焦点框 7），保留行内容向内裁切；R06 Save History 测试固定 740 DIP 表格夹具以重现横向滚动截图。蓝色选中反馈、内容位置、命令/绑定、UIA、虚拟化和滚动条均保留。用户指定的 U00-01 基线与去重在既有 evidence/U00-01/BASELINE.md 中已完成，本批复核后沿用。

3aaae534 已推送 main。Source Validation exit 0；WPF 静态审查 88 XAML / 0 errors / 78 warnings / 507 info；精确 SHA Render QA 334 PNG / 0 PROBLEM。R06 SaveHistory、R06 SelectionState、R06 TaskGrid、R10 RecentAccess、R23 资源状态回归均在完整链中通过。SDK 8.0.423、MSBuild 17.11.48；中文入口 GameSaveCenter-一键构建安装运行.cmd 转交 GameSaveCenter-Run.cmd，完整编译、测试、Worker 发布、打包、安装身份核验及 Playnite 启动最终 exit 0。160 TRX 共 2028 项、1970 通过、0 失败、58 既有 skip（57 条撤销架构断言、1 条鼠标 opt-in，skip 不计通过）。独立 scripts/build.cmd 按真实编译/测试职责 exit 0，2028 项、1960 通过、0 失败、68 skip；额外 10 条为受限环境 Named Pipe 测试，不计通过。包 ZIP/PEXT 各 45,738,426 bytes，SHA-256 62550D4029212FBE7F6F1D34B12380236D880883132E259BBACE7ACDD09A4AD3；六个安装程序集身份含精确源码 SHA，Playnite/Worker 响应且扩展日志记录已加载。完整 TRX 与身份见同目录 evidence/diagnostics/local-gates-3aaae534.json。

精确 push run [#1450](https://github.com/Nikilua/GameSaveCenter/actions/runs/41659230554) completed/success：基线 job 125997757253 15/15 步、SDK10.0.401 job 125997757096 8/8 步通过；预览包和两份诊断 artifact ID/digest 均绑定候选 SHA。required-check/ruleset 读取沿用先前403，不重复试探，U09-06仍远端待验，由有读取权限的维护者只读核对。安装第一次因普通权限无法写入用户 Roaming 扩展目录失败，保留诊断；系统TEMP恢复到C盘后，清理本阶段测试完成的隔离输出 4ac8b9e6（1015文件/281,907,597 bytes）并记录空间前后，再按授权提权完整重跑成功。包、当前审计、诊断与TRX均保留。

Playnite 进程和插件加载日志不等于实际页面验收；当前仍未观察插件页面。外部 UIA、实体键盘/IME、物理 DPI 仍待验，状态为代码完成待验收。未执行真实存档恢复、媒体删除或云端写入。整轮计数 7 项已满足、71 项代码完成待验收、U09-04 进行中、U09-06 远端待验。
## 2026-11-03 U06-02 / U08-08 右缘完整包裹与精确候选 6cd0f9ec

用户附图指出最近任务的日期时间以及存档/任务列表末列文字落在选中框外。共享选框右侧inset与DataGrid行宽换算叠加过大；修复将公共ListBox与生产DataGrid选框inset收至约4 DIP，并同步背景overlay/宽度换算，保持选择/焦点/悬停反馈而不挤压文本、图标或列表滚动。修复后Overview、Save、Task及Shell嵌套任务表的右侧文字都在框内，截图、TRX和完整门禁见[本批证据](evidence/U06-02-frame-wrap-6cd0f9ec-20261103/BASELINE.md)。

当前冻结源码为 6cd0f9ec55ada37f03a88d24bc46690ed1837a06。SDK8.0.423下Source Validation和WPF静态审查通过；Render QA 334张、0条PROBLEM；独立scripts/build.cmd exit0（2026项/1958通过/0失败/68 skip）；中文完整安装启动链exit0（8项目、0警告/错误，2026项/1968通过/0失败/58 skip）。跳过不计通过；独立入口额外10项因执行环境禁止Named Pipe而跳过，逐项保留TRX。

精确push run [#1446](https://github.com/Nikilua/GameSaveCenter/actions/runs/41613860203) completed/success：基线job 125842266009的15步和SDK10.0.401 job 125842265806的8步全部通过，三份artifact均绑定候选SHA。根入口安装身份、ZIP/PEXT SHA、日志与测试摘要见证据目录。首轮根入口仅因空间预检不足而退出；按清单清理本阶段已确认过期的输出后重跑成功，保留旧失败诊断。

本批U06-02/U08-08代码完成待验收，U09-05/U09-08已满足。80项状态仍为7项已满足、71项代码完成待验收、U09-04进行中、U09-06远端待验。Playnite进程已启动并核验安装DLL，但没有观察插件实际页面；UIA、实体键盘/IME、物理DPI和U09-04原始帧/累计分配数据仍待验。required-check接口既有403不重试，下一步为有权限维护者只读核对。真实恢复、媒体删除、云端写入未执行。

## 2026-11-03 U09-05 / U09-08 冻结候选 de6f7f82 最终验收

冻结候选 `de6f7f82038c59c5fbdbb9b366c3e280354ffc5f` 已提交并推送。实际中文入口 `GameSaveCenter-一键构建安装运行.cmd` 转交 `GameSaveCenter-Run.cmd`，对精确候选完成 SDK 8.0.423 Release 编译、完整测试、Worker发布、打包、安装身份核验及Playnite启动链，最终退出码0；8项目、0 warning/0 error，158份TRX共2026项、1968通过、0失败、58条既有skip不计通过。独立 `scripts/build.cmd` 在 `eebef009` 上完成编译/测试、exit0；其 `src/`、`tests/`、`scripts/` 树与冻结候选一致，不替代完整安装启动链。安装包ZIP/PEXT均45,737,961 bytes、SHA-256 `4C03AF7D78FCA82BD23D27D37934B44BEFF1598EAC57EF5CC57A942C19A7E243`；六个安装DLL身份与de6 SHA一致，Playnite日志确认插件0.6.73已加载。

精确源码 push run [41547554497（#1441）](https://github.com/Nikilua/GameSaveCenter/actions/runs/41547554497) 已 completed/success；SDK基线 job `125626741394` 的15步和SDK10.0.401 job `125626741153` 的8步全部成功，含编译/测试、Render QA、WPF静态审查、打包与上传。三个artifact的ID、大小、digest和源码身份见[CI证据](evidence/U09-05-final-root-de6f7f82-20261103/ci-run-41547554497.json)，本机入口、TRX、安装身份和边界见[最终门禁报告](evidence/U09-05-final-root-de6f7f82-20261103/BASELINE.md)。

U09-05与U09-08已满足。80项现状为7项已满足、71项代码完成待验收、U09-04进行中、U09-06远端待验。真实Playnite页面没有实际观察：计算机控制运行时缺路径并连续初始化失败两次；插件加载日志不代表页面验收。外部UIA、实体输入/IME及物理DPI仍待宿主验收；U09-04仍缺U00原始逐帧/累计分配和真实Playnite呈现数据。required-check/规则集接口既有403不重复请求，U09-06下一步为有权限维护者只读核对required-check名单。真实存档恢复、媒体删除及云端写入未运行。

## 2026-11-03 U06-02 / U08-08 DataGrid兼容行选中框无偏移

生产提交`cf5173e75758782ad2bcd372cc9492ad393b908b`（`修正Dashboard行选中框内容偏移`）及R06测试夹具修正`d5387b39dc1a3bc1e10ba0a61363e48876faca38`已推送`origin/main`。首页最近活动的蓝色圆角框保留，行内容和图标不因选框向右下偏移；Dashboard兼容DataGrid也把内容、选中色背景和框线拆成同级，inset只作用于非交互overlay。共享生产DataGrid、Overview和公共ListBox复核均保持内容几何。

SDK8.0.423 Release对cf5173的Dashboard双主题DataGrid行回归1/1、共享四页DataGrid 1/1、最近活动/共享ListBox 1/1通过，均0 skip；修正R06夹具后，d5387精确提交整类3/3、0失败/skip。Source Validation、WPF静态审查与双主题Render QA通过。旧run #1438的两job确因R06夹具未生成行容器而失败，渲染/静态/打包被门禁跳过，历史和诊断保留。修复提交对应run #1439（41529821227）现已completed/success：基线job125566804922的15/15步骤和SDK10.0.401 job125566804561的8/8步骤均通过。完整测试、包身份、artifact digest见[#1439证据](evidence/U06-02-datagrid-row-overlay-20261103/ci-run-41529821227.json)；#1438失败证据见[run #1438](evidence/U06-02-datagrid-row-overlay-20261103/ci-run-41525415460.json)，本地R06 TRX见[夹具生命周期复测](evidence/U06-02-datagrid-row-overlay-20261103/r06-fixture-lifecycle-runs.json)。

80项状态计数不变：5项已满足、71项代码完成待验收，U09-04/05/08进行中、U09-06远端待验。已找到当前FullLanguage执行环境；独立`scripts/build.cmd`已在`eebef009`源码树通过Release编译和全测试，中文根入口完整安装启动链仍待在冻结候选上执行。Playnite实页、外部UIA、实体输入/IME、物理DPI仍待验。

## 2026-11-03 U09-05 独立build.cmd同源码树复核（eebef009）

独立入口`scripts/build.cmd`按真实职责调用`build.ps1`，SDK 8.0.423 Release 编译0警告/0错误；158份TRX共2026项，1968通过、0失败、58跳过，155个WPF隔离类。58项为既有57条撤销“今日工作台”架构断言和1条默认不移动系统鼠标测试；均不计通过，未扩大skip。诊断见[当前build.cmd记录](evidence/U09-05-final-eebef009-20261103/independent-buildcmd.json)。

首次相同源码运行的Worker压力测试在第17周期第二次原子写入收到`UnauthorizedAccessException`/`0x80070005`；复核未找到ACL拒绝，原因未确认。保留完整失败TRX与诊断；调整到短测试临时根后在同一源码、SDK和断言下复跑通过，没有改源码、删断言或增加skip。磁盘预检及随后只删除已确认归属本阶段且被成功输出替代的`artifacts/gsc-b/`构建二进制，见本证据目录的`stage-start.json`和`cleanup.json`；两个候选的诊断/TRX均保留。

当时冻结候选仍待中文根入口完整链；后续de6f7f82已完成该链及精确双SDK CI，见本文件顶部的最终验收记录。真实插件页面、UIA、实体输入/IME、物理DPI仍未验；存档恢复、媒体删除和云端写入未运行。

## 2026-11-03 上一源码候选 77482a9a 焦点框与入口门禁

按用户指出的首页最近活动选中偏移问题，保留此前 `f9d09cb2` 对 `TaskItemChrome.Margin=4,2` 的修复：蓝色圆角选框仍在，整行与图标不随选中框右下移动。共享资源巡查又发现 `AcrylicNavItem` 键盘焦点会把内部内容边框从1 DIP加到2 DIP并叠加蓝线；本批改为独立、不命中的 sibling overlay，维持选中色、焦点环、RadioButton导航、Tag/Checked与UIA语义。源码提交 `77482a9a56f8d1507de3cf819c94559388dddc4f` 已推送 `origin/main`。

SDK8.0.423定向R23两条AcrylicNavigation WPF用例2/2通过；提交后Source Validation退出0，WPF静态扫描218 XAML/0错误，精确候选Render QA为Light/Dark共394 PNG、0 PROBLEM、WorkingTreeClean=True。报告与SHA见[U06-02共享焦点框证据](evidence/U06-02-nav-focus-overlay-20261103/BASELINE.md)。

本机实际中文包装器链与独立 `scripts/build.cmd` 均在构建前遇到PowerShell `ConstrainedLanguage` dot-source拒绝并以exit1结束；没有本候选全量测试/TRX、包、安装身份或Playnite启动，原始日志已保留。上一候选`34d8a99f`的成功只作历史；未在同一受限环境重试。GitHub精确候选run [41498607041（#1436）](https://github.com/Nikilua/GameSaveCenter/actions/runs/41498607041)两job、全部步骤成功，预览包和两份诊断artifact均绑定本SHA。Required-check/保护规则接口此前403不重试，U09-06仍为“远端待验”；前一纯文档run #1435成功不代替源码CI。

当前80项状态：5项已满足、71项代码完成待验收、U09-04/05/08三项进行中、U09-06一项远端待验。U06-02/U08-08仍待Playnite实页、外部UIA、实体输入/IME和物理DPI验收；真实存档恢复、媒体删除和云端写入未执行。完整本机门禁、run/job/artifact身份和清理记录见[当前候选门禁证据](evidence/U09-05-final-gates-77482a9a-20261103/BASELINE.md)。
## 2026-11-03 上一冻结候选 34d8a99f 本地入口与远端门禁记录

按用户补充复核“最近活动”选中框：生产修正已在`f9d09cb2`移除`TaskItemChrome.Margin=4,2`，蓝色圆角框保留且行内容不再右下偏移。继续盘点公共选择效果：Overview专用和共享`ListBoxItem`三态几何由R10覆盖；任务/媒体/存档/维护四页`DataGridRow`的背景与框线是独立overlay，R23跨Light/Dark比较单元格和文字布局。R10类3/3、R23几何回归方法通过；没有删除选中描边或新增位移。

候选`34d8a99faf7ec827b2fea8303baf8c18a8edbefd`的实际中文入口`GameSaveCenter-一键构建安装运行.cmd`经`GameSaveCenter-Run.cmd`完成Release编译、全测试、Worker发布、打包、Playnite安装身份核验和启动，最终exit0。SDK8.0.423/MSBuild17.11.48，8项目0 warning/0 error。独立`scripts/build.cmd`亦按其真实build/test职责完成、exit0。入口完整汇总及安装哈希见[本批证据](evidence/U09-05-final-gates-20261103/BASELINE.md)。

完整中文链TRX 158份、2025项：1967通过、0失败/错误/超时/中止、58 skip；独立build.cmd是1957通过、0失败/错误/超时/中止、68 skip。58项为57条已撤销“今日工作台”架构断言与1条默认不移动系统鼠标测试；build.cmd因受限执行进程额外跳过9条Named Pipe客户端行为与1条Worker进程重启行为。它们均不计通过、skip未扩宽；中文完整链同SHA未跳过这10条Named Pipe行为。真实插件页面未观测。

Source Validation exit0；本地WPF静态扫描218 XAML、0错误（208 warning、1327 info，包含保留`.tmp`快照）；精确SHA的CI Render QA与静态审查步骤成功。GitHub run [41477919033（#1434）](https://github.com/Nikilua/GameSaveCenter/actions/runs/41477919033)下SDK基线job125388281628及SDK10.0.401 job125388281904、全部步骤均completed/success；preview及两份diagnostics artifact都绑定SHA`34d8a99f`，详见[CI身份](evidence/U09-05-final-gates-20261103/ci-run-41477919033.json)。required-check API此前403不重试，U09-06仍记“远端待验”。

本阶段在C盘空间预检不足后，仅清理本阶段确认归属的旧`artifacts/gsc-b/`编译二进制4104个/1,447,319,716 bytes；保留所有test-temp、未知文件、最新build/包、完整TRX和诊断。具体删除路径及磁盘前后见证据目录`stage-start.json`与`cleanup.json`。状态更新为6项已满足、71项代码完成待验收、U09-04/U09-08进行中、U09-06远端待验；U09-05已满足。U06-02/U08-08仍待真实Playnite实页、外部UIA、实体输入/IME及物理DPI。无真实恢复、媒体删除或云端写入。

## 2026-11-02 Round5 U09-04 TaskGrid滚动归属复测

旧耐久探针为Task页挑选最大可滚高度的`ScrollViewer`，不能确定控件归属；U00原始耐久探针没有独立TaskGrid滚动动作。本批在U00源码`5d0a2448`与当前生产源码`f9d09cb2`隔离快照注入完全相同的临时探针，只操作`TaskGrid`模板子树内唯一的`DG_ScrollViewer`。两源码各自SDK8.0.423 Release编译0警告/错误，Light/Dark每轮60秒+30秒收尾、1040×700 DIP、60条固定假数据，四轮均exit0、0动作失败。

Light p95为U00/current 16.526/16.409 ms（428/441样本），Dark为16.498/16.499 ms（430/446样本）；中位数约13.8 ms。60项中内部视口6行、实现7行、ScrollViewer可滚范围54；每轮均有一半样本移动离顶。窄化后的逻辑动作未显示TaskGrid滚动p95回退。旧36.33/33.81/31.45ms与52.02/42.96ms属于页面或最大滚动器动作，不再归到TaskGrid。构建/探针、SHA、完整报告和restore失败诊断均已归档；临时快照仅清理本阶段自有`.tmp`目录。

这不是Playnite实际宿主、物理滚轮/拖动、呈现帧、IME/UIA或物理DPI测量；每个源码/主题仅一轮，U00原始逐帧及累计分配仍缺。U09-04维持进行中；整轮计数5项已满足、71项代码完成待验收、3项进行中、1项远端待验。证据：[TaskGrid归属复测](evidence/U09-04-taskgrid-scroll-20261102/BASELINE.md)。

## 2026-11-02 Round5 U06-02/U08-08 选框对齐与偏移修正

用户补充指出首页最近活动选中时，蓝色圆角框可以保留，但不能让整行向右下偏移而裁切图标。此前Overview专用模板把选中态`TaskItemChrome.Margin`设为`4,2`；生产提交`f9d09cb2b7665355a74926e2cd4a801567c60baa`移除该偏移，只保留选中色、蓝色描边和14 DIP圆角。盘点确认公共隐式`ListBoxItem`选中/焦点框已与内容分层并保持Margin=0；DataGridRow与媒体卡已有独立覆盖层，Overview保护项与ComboBox只改变颜色，未为统一样式重写无偏移问题的控件。

新增几何断言覆盖页面专用与公共列表三态，图标及容器相对列表槽位在未选中、选中、键盘焦点时不变。精确SHA的R10类3/3通过；source validation、WPF静态审查26 XAML/0 errors和双主题Render QA 394 PNG/0 PROBLEM通过。完整Release底层脚本直接调用exit0（2025总项、1967通过、0失败、58既有skip不计通过），当前包/安装程序集身份匹配源码SHA，Playnite进程已启动；真实插件页面仍未人工观察。

当前候选硬门禁未全部通过：实际中文根`.cmd`与独立`scripts/build.cmd`上次在未变化的包装器/脚本版本`0067525f`均因ConstrainedLanguage dot-source拒绝而exit1；按授权不重复试探相同拒绝，`f9d09cb2`没有精确入口成功记录，底层脚本成功不替代入口通过。GitHub精确run `41449095659`已completed/success：baseline与SDK10 jobs所有步骤均成功，扩展包已生成并上传，run/job/artifact身份见[本批证据](evidence/U06-02-selection-frame-align-20261102/BASELINE.md)。因此U09-05/U09-08继续进行中；U06-02/U08-08仍“代码完成待验收”，总计5项已满足、71项代码完成待验收、3项进行中、1项远端待验。没有运行真实存档恢复、媒体删除或云端写入。

## 2026-11-02 Round5 U06-02/U08-08 整行选中槽位回归

按用户补充的首页最近活动问题，保护选中蓝色圆角框不再推动整行或裁切图标。本批只扩展`R10RecentAccessBehaviorTests.cs`：在Overview专用模板和共享隐式`ListBoxItem`模板上，比较未选中、选中、键盘焦点三态的图标tile及容器相对ListBox的bounds，并断言容器`Margin`、`RenderTransform`、`LayoutTransform`不变。框线依然可见，既有Overview 14 DIP、公共列表8 DIP圆角及图标坐标≤0.01 DIP断言保留。生产XAML/资源/命令/Binding未变，前后图和共享样式审查沿用[cb7b78fd选框证据](evidence/U06-02-selection-overlay-20261102/BASELINE.md)。

SDK 8.0.423 Release编译后按仓库WPF隔离设置运行完整`R10RecentAccessBehaviorTests`类：3/3通过、0失败、0 skip，退出0，干净日志无COM收尾异常。首次还原停滞与默认并行目标运行已取消、未启动测试；单方法1/1运行虽退出0但TestHost收尾有`InvalidComObjectException`，均不冒充干净结果。测试源、程序集身份、TRX/日志SHA和各尝试见[槽位回归证据](evidence/U06-02-selection-slot-guard-20261102/BASELINE.md)。Source Validation与`git diff --check`通过；本批无生产XAML差异，因此重用cb7精确源码的静态审查与Render QA，不声称本批重新渲染。

仅清理本阶段自有`.tmp/round5-u06-02-slot-regression-20261102`输出，5个日志/TRX共13,862 bytes均已先归档并逐项复核长度/SHA，清理后路径不存在且Markdown无残留引用；具体目标见[清理清单](evidence/U06-02-selection-slot-guard-20261102/cleanup.json)。容量前值是阶段开始快照，后值在清理后复核；期间发生SDK编译，净变化不归因于删除，紧邻清理瞬间的控制台容量快照因工具输出截断未留存。

U06-02/U08-08仍为“代码完成待验收”；真实Playnite页面、外部UIA、实体输入/IME及物理DPI未验。测试源码产生了新的最终候选，故U09-05与U09-08重新进行中，需对该候选重新冻结并执行完整链；U09-04仍进行中，U09-06仍远端待验，当前计数5项已满足、71项代码完成待验收、3项进行中、1项远端待验。没有执行真实存档恢复、媒体删除或云端写入。

## 2026-11-02 Round5 U09-04 U00同条件时长代理复测

在当前生产源码`cb7b78fd`上将U00-06与U09两轮控制在同一60秒活动期、30秒收尾、1040×700 DIP、双主题与七页假数据。页面切换布局p95由U00 803.59ms降至426.38/411.29ms，详情打开125.58降至35.69/34.73ms，滚动36.33降至33.81/31.45ms；程序化搜索更新p95当前14样本下高于U00但中位数更低，样本尾部太少不判定回退。所有值均为受控WPF逻辑动作代理，不是呈现帧/物理输入；托管堆首尾差也不是累计分配。Source Validation exit0；本批进度及证据说明267个相对链接0缺失，全仓自写扫描另报157条历史缺失链接（主要为指向旧`.tmp`/`artifacts`的记录），本批未依赖这些路径且未扩大清理范围。完整原始报告、源码/二进制身份和清理清单见[复测证据](evidence/U09-04-u00-aligned-20261102/BASELINE.md)。U09-04仍进行中，U00原始帧/累计分配及真实Playnite测量仍缺。

## 2026-11-02 Round5 U09状态表校准与文档候选CI

复核发现U09-05和U09-08证据段已结案但80项表格行仍显示“进行中”，本次按最终中文入口/独立build链和整轮交付审查证据校准；状态仍为7项已满足、71项代码完成待验收、U09-04进行中、U09-06远端待验。U09-04的U00-06确有逻辑动作性能代理值，但原始逐帧/累计分配数据缺失；与后续不同采样时长/动作脚本的样本不作直接性能归因。文档提交`21ba511c`精确run [41413816012](https://github.com/Nikilua/GameSaveCenter/actions/runs/41413816012)（#1428）中baseline job `125172732996`（15/15步）与SDK10.0.401 job `125172732641`（8/8步）均completed/success；artifact身份见[`ci-run-41413816012.json`](evidence/U09-final-cb7b78fd-20261102/ci-run-41413816012.json)。required-check保护API仍沿用先前403状态，不重复请求。

## 2026-11-02 Round5 U09-05/U09-08 当前冻结候选完整门禁

生产UI源码提交`cb7b78fd514e7d6eaa2a34e341340d5d06610508`已推送；最终文档冻结候选`0b61ee51ff0d78bf2f0f7c428cf358721753e558`仅追加文档，源码、测试、脚本和资源子树与生产提交一致。冻结候选先提交再执行了实际存在的中文入口`GameSaveCenter-一键构建安装运行.cmd`完整编译、测试、发布、打包、安装核验及Playnite启动，退出码0；随后独立运行`scripts/build.cmd -OutputRoot artifacts\gsc-b\1c0856c2`，退出码0。未创建或改名`run.cmd`等入口。SDK 8.0.423、Release；两入口各158份TRX、2025项，1967通过、0失败、58条既有skip（不计通过），Core 157/157、Worker 401/401、155个WPF隔离类均完成。日志、诊断ID与逐份TRX SHA见[本轮最终证据](evidence/U09-final-cb7b78fd-20261102/BASELINE.md)及[`local-gates.json`](evidence/U09-final-cb7b78fd-20261102/local-gates.json)。

最终ZIP与PEXT均为45,737,895 bytes、SHA-256 `CC3F08165F76AE32CFB48E8EC4A8A4930B2F59B10D1D69021420A5614055D82C`；安装清单0.6.73，插件与Worker程序集身份均为`0.6.73+0b61ee51ff0d78bf2f0f7c428cf358721753e558`，Playnite进程启动成功。GitHub `windows-build.yml` run [41403537450](https://github.com/Nikilua/GameSaveCenter/actions/runs/41403537450)对精确冻结SHA的SDK基线job `125140372775`（15步）和SDK 10.0.401 job `125140372862`（8步）均completed/success；三个artifact ID/digest见[`ci-run-41403537450.json`](evidence/U09-final-cb7b78fd-20261102/ci-run-41403537450.json)。required-check清单继续沿用U09-06既有403限制，保持“远端待验”，不重复请求。只清理本批自有D盘临时目录，清单和空间前后见[`cleanup.json`](evidence/U09-final-cb7b78fd-20261102/cleanup.json)；保留最新包、构建和审计输出以及历史`.tmp/`。U09-05、U09-08已满足；U09-04仍进行中，U09-06远端待验；总计7项已满足、71项代码完成待验收、1项进行中、1项远端待验。真实Playnite页面、外部UIA、实体输入/IME及物理DPI仍未验；没有执行真实存档恢复、媒体删除或云端写入。

## 2026-11-02 Round5 U06-02/U08-08 选中框与共享列表内容稳定

针对首页最近任务选中后图标随蓝色圆角框右下偏移的问题，生产提交`cb7b78fd514e7d6eaa2a34e341340d5d06610508`已推送`origin/main`。Overview 专用模板把选中框从内容测量层拆出，同时保留原4×2 DIP内缩、蓝色边框和14 DIP圆角；公共`ListBoxItem`模板也把选中/2 DIP焦点框与内容分层。`DataGridRow`、媒体卡片等已是独立覆盖层或仅改颜色，本批未重复改动。

1040×700 DIP、固定`2026-11-02T00:00:00Z`样本的Light/Dark选中态前后图见[本批证据](evidence/U06-02-selection-overlay-20261102/BASELINE.md)：旧图图标贴近右下边界，新图恢复到行内原位，选框仍可见。新WPF回归测真实图标tile相对行容器在未选中/选中/焦点三态的坐标和尺寸，断言误差≤0.01 DIP；共享和局部模板均通过。

SDK8.0.423 Release解决方案编译0 warning/0 error；`R10RecentAccessBehaviorTests` 3/3通过、0失败、0 skip；Source Validation退出0；WPF静态审查218 XAML/0 errors（208 warnings、1327 info，Overview两条滚动容器提示与本次模板无关）；精确提交Render QA双主题394 PNG、0 PROBLEM、WorkingTreeClean=True。完整日志/TRX、394项PNG哈希和清理清单在证据目录。GitHub run `41392841712` (#1425)对精确SHA的SDK8基线job `125107766820`及SDK 10.0.401 job `125107767536`均completed/success、所有步骤完成；preview artifact `13120801010`、基线诊断 `13119839919`、SDK10诊断 `13118807409`均绑定本提交，digest见[生产提交CI记录](evidence/U09-final-cb7b78fd-20261102/ci-run-41392841712.json)。活动期间job日志接口曾返回BlobNotFound，不影响最终job状态。U06-02与U08-08仍“代码完成待验收”，Playnite真实页面、外部UIA、实体输入/IME与物理DPI仍待验；未触碰恢复、媒体删除或云写入。U09-05和U09-08已满足，总计7/71/1/1。

另核实U06-03悬停夹具修正候选提交`3ecacdc0dc5376e45a6460b2af2668a1df57f328`：run `40647037308` 的SDK8.0.423基线与SDK10.0.401 jobs均completed/success，所有适用步骤完成；preview及两份诊断artifact均绑定该SHA。旧的39c6ad80/e4285c05失败保留为历史，不再把已修正候选标作CI待验。U06-03仍因真实宿主/物理输入未验而保持代码完成待验收。

## 2026-11-02 Round5 U05-01/U06-07/U09-04 活动工作区重排复测

生产提交 `25bcbf7934575e6aa933373e1f508a1234b28b97` 已推送。响应式布局只重排 PageHost 当前托管的缓存页；脱离页面在再次激活时使用最新尺寸。R08 页面切换行为 5/5、0 失败、0 skip；SDK 8.0.423 Release 解决方案构建 0 warning / 0 error，Source Validation 和 WPF 静态审查退出 0。首版 Render QA 报告 8 个 Maintenance 外部夹具问题，控制基线后加 PageHost 缓存实例归属判断；最终精确提交 Render QA 双主题 394 PNG、0 PROBLEM。

同条件一次 120 秒活动加 30 秒稳定观察，203 个 workspace_switch_layout 样本 p95 408.80 ms、最大 544.66 ms；此前 f1a614a8 两轮为 463.20 / 439.21 ms。计时包含主题变化和整页同步布局，保留观察结果，不归因单一控件或声称修复呈现帧卡顿。U05-01、U06-07 状态仍代码完成待验收；U09-04 保持进行中，真实宿主和 U00 原始帧/分配基线仍缺。

GitHub run [41371608155](https://github.com/Nikilua/GameSaveCenter/actions/runs/41371608155) 对 f1a614a8 的 baseline 与 SDK10 jobs 均 success；文档提交 de9fe37d 的 run [41376045182](https://github.com/Nikilua/GameSaveCenter/actions/runs/41376045182) 两 jobs 也已 success。25bcbf79 的 run [41379419270](https://github.com/Nikilua/GameSaveCenter/actions/runs/41379419270) 最近查询时 SDK10 已 success、baseline 仍在编译与测试，不计通过，继续跟踪。required-checks API 的 403 限制不重复探测，U09-06 仍远端待验。

详细证据、失败对照、源码/截图清单、CI身份与本阶段清理见[本批证据](evidence/U05-01-U06-07-U09-04-active-layout-20261102/BASELINE.md)。中文根入口完整链、独立 scripts/build.cmd、最终候选安装身份和启动仍未执行；不把离屏/受控窗口结果表述成真实Playnite验收。

## 2026-10-30 U00-07/U06-04 媒体收件箱工具栏层级

生产提交 64f26f58153efdb4a634bf8a0aa7c832898a581d 已推送。媒体收件箱批量动作由两行改为一行，模式、当前游戏目标、批量归类和详情入口仍在可见工具栏；低频清空选择/重置列宽移到原有滚动页脚。命令与安全语义保持不变。1040×700壳层 DataGrid 起点由Y=376上移至Y=300 DIP（76 DIP），保持四个完整数据行；before/after两主题Render QA各394 PNG、0 PROBLEM。定向行为合计218通过、0失败；另有39条撤销“今日工作台”架构的既有skip，不计通过。证据、TRX、截图SHA、source validation/WPF静态输出、CI身份与阶段清理见[evidence](evidence/U00-07-U06-04-media-inbox-20261030/BASELINE.md)。

GitHub run 41041454086 对精确SHA的baseline job 124014997522与SDK 10.0.401 job 124014997301均completed/success，所有步骤完成；三个artifact的head SHA与digest见同目录ci.json。required-checks/branch protection沿用此前403权限限制，不重复探测。U00-07/U06-04状态仍为代码完成待验收：真实Playnite、外部UIA、实体输入/IME和物理DPI未验。整轮计数7/71/1/1；继续推进独立任务，不等待旧S/T权限项。

## 2026-10-30 U00-07/U06-04 任务页工具栏层级

基于 U00-01 任务页截图，P1 候选是“查看任务详情”落到第二行且重置按钮文案被截断。生产提交 `d7c4bd5e3fc2ca83d0393ea51fc7f911cb80100f` 将详情入口并入队列主操作行，将列设置和布局重置移入已有筛选/视图配置展开区；保留完整文案、命令、Binding、Tooltip、UIA 名称和重置范围预览。提交 `7bc6ab38fa661c11a34086548797f78e02deff43` 修正随布局迁移而过期的回归断言，未改变生产 XAML。相同 UTC 样本、1040×700 DIP、Light/Dark 前后图和差异说明见[任务页批次证据](evidence/U00-07-U06-04-task-toolbar-20261030/BASELINE.md)。

精确源 `7bc6ab38` 的五个隔离测试进程合计 101 通过、0 失败、11 条既有“今日工作台”断言 skip；skip 不计通过。Source Validation 通过；Render QA 在生产 UI 源 `d7c4bd5e` 上 394 图、0 PROBLEM；WPF 静态审查 10 个 XAML、0 error，23 条既有 warning 均在其它视图。单进程合跑的失败已留档，隔离重跑五类均通过。GitHub run `41017807712` 对 `d7c4bd5e` 因旧祖先断言失败；修正提交 `7bc6ab38` 的 run `41022318628` 两项适用 job 均 success，preview/诊断 artifact 身份见本批 `ci.json`。required-checks/branch protection 沿用先前 403“远端待验”状态，不重复查询。Demo 源缺失；Playnite 宿主、外部 UIA、实体键盘/IME、物理 DPI 未验，U00-07、U06-04 仍“代码完成待验收”。未运行真实恢复、媒体删除或云写入。

## 2026-10-30 U00-03/04与U06-04/07存档页工具栏整理

保留主扫描、预览、校验、重载、筛选和列设置操作；把低频视图名称/保存/复制/改名/删除/重置收进默认折叠的既有`GscDisclosureCard`。存档历史状态列默认紧跟时间和类型，1040×700 DIP首屏可读恢复可用性；没有触碰游戏选框、滚动条或业务安全语义。浅/深主题同数据前后图、源码差异和日志见[本批证据](evidence/U00-03-04-U06-04-07-20261030/BASELINE.md)。

SDK8.0.423 Release的`CompactWorkspaceTableViewportBehaviorTests`为14/14、0 skip；`scripts/build.cmd`编译8项目、0警告/0错误；源码检查通过。10个生产View XAML静态检查0错误，23条既有提示均在其他视图。固定UTC双主题Render QA为334图/0 PROBLEM；隔离状态夹具生成216图，归档两张代表图。首次仅测量宿主导致1条可见性断言失败，改为Window宿主后单测与整类均通过，原始TRX和原因保留。

实现提交`3139876b347ae380e663857ed2995605edc95872`已推送`main`。GitHub `windows-build.yml` run `41010673702`（1415）对精确SHA终态success；baseline job `123915583587`与SDK10.0.401复核 job `123915583967`均success，所有步骤完成。三份artifact均绑定该SHA，ID/digest见[本批证据](evidence/U00-03-04-U06-04-07-20261030/BASELINE.md)。当前workflow job总数2并均已通过；required-checks/branch-protection读取权限沿用U09-06既有403状态，仍记“远端待验”，不重复探测。U00-03、U00-04、U06-04、U06-07仍为“代码完成待验收”；Demo源码、真实Playnite、外部UIA、实体输入/IME及物理DPI仍未验。只清理本批两个自有`.tmp`目录共1,766个文件/292,224,197 bytes；C盘空闲3,908,599,808→4,199,956,480 bytes，D盘约16.99 GB，清理细节见[清单](evidence/U00-03-04-U06-04-07-20261030/cleanup.json)。80项计数保持7项已满足、71项代码完成待验收、1项进行中（U09-04）、1项远端待验（U09-06）。本批未执行恢复、媒体删除或云端写入。

## 2026-10-30 U09-04 重建U00基线与任务页滚动复测

从U00源码提交`90bf040567f013f3a8d9ed8667028a4edb2ec524`隔离重建固定周期RenderHarness，与冻结候选`5c0c57b06aed27dc46c41ef3d655ec20dafa5ec7`使用SDK8.0.423、固定UTC、70周期/297动作、30秒settle及同尺寸双主题。U00补丁只把45处FakeDashboardData `DateTime.UtcNow`替换为固定假时钟，并加入相同诊断仪表；没有修改生产源码。两侧对应RenderHarness编译均exit0、0 warning/0 error，所有固定周期与动作运行exit0。

合并210个主题应用样本的p95从U00 448.77 ms降至候选325.97 ms，>300 ms从133降为21；内容布局p95为87.52→97.49 ms，超过300 ms均为0。活动期AppDomain托管分配均值11.118 GB→8.878 GB（候选低20.15%）。这些不是纯产品UI分配；Rendering回调也不是物理帧。任务页DataGrid内部滚动两组单轮动作对照均向慢方向：p95 29.36→38.23 ms及29.73→37.74 ms，分配p95约3.14/3.19 MB→5.00 MB；每组每侧仅10个样本，测量包含显式`UpdateLayout()`，不能据此归因。TaskDataGrid样式、实例和共享DataGrid模板相同；共享行样式选中边框改为几何令牌，任务页代码后续也改过紧凑详情动效与自适应布局，仍需更窄的重复探针定位。

U09-04继续进行中：主题和总分配信号改善，内容布局旧长尾未复现，但任务表格滚动信号未解释。原始U00帧/累计分配产物、真实Playnite呈现、实体输入/IME和物理DPI仍未验；本批没有修改生产/测试源码，也没有运行安装启动全链或独立`scripts/build.cmd`。

[U00重建协议、测量与代码差异](evidence/U09-04-correlation-20261030/u00-reconstructed-20261030/BASELINE.md)；[构建/运行身份](evidence/U09-04-correlation-20261030/u00-reconstructed-20261030/build-run.json)；[可复算比较](evidence/U09-04-correlation-20261030/u00-reconstructed-20261030/comparison.json)。状态计数保持7项已满足、71项代码完成待验收、1项进行中（U09-04）、1项远端待验（U09-06）。

仅清理本阶段两个已确认自有`.tmp`目标，共13,293文件/860,449,585 bytes；C盘空闲4,450,738,176→5,339,082,752 bytes，D盘18,815,148,032→18,815,213,568 bytes。归档SHA和清理边界、进程查询限制见[清理记录](evidence/U09-04-correlation-20261030/u00-reconstructed-20261030/cleanup.json)。

## 2026-10-30 U09-01 全批次证据清查

从U00-01源基线`90bf0405`到冻结候选`5c0c57b0`审阅37个UI源码变更提交；U01–U08的可视差异和动态/生命周期任务均映射到现有双主题配对图、实际Rendering采样或有序行为序列。当前HEAD`f6836930`与冻结候选的`src/`、`tests/`树一致。未发现未映射的生产UI变更批次，U09-01按“对比证据最小集”范围标记已满足，不代表真实宿主验收。完整提交清单与引用见[覆盖审计](evidence/U09-01-coverage-20261030/BASELINE.md)。Demo源文件在当前及相邻main工作树都不存在，本批不声称Demo对齐。

最新状态：7项已满足、71项代码完成待验收、1项进行中（U09-04）、1项远端待验（U09-06）。8d9724be精确SHA的run查询与combined status均为空，记录为远端待验；查询证据见[CI状态](evidence/U09-01-coverage-20261030/ci-query-8d9724be.json)。

## 2026-10-30 U09-04 布局尾部统计与诊断边界

以当前冻结候选已有三轮原始CSV统计每轮70个布局样本：p50为46.88–47.76 ms，p95为472.10–649.76 ms，17/210次超过300 ms，分布在七页和双主题。每轮时长与分配的Pearson r为0.153–0.318，属描述性弱相关，未能把尾部归因到某页、动画、GC或外部调度。Rendering p95仍为338–360 ms，超过100 ms间隔132–139个；该回调不是物理呈现帧。合成搜索/滚动p95与e6接近，活动期托管分配亦接近。e6/current样本固定标记和逐页样本数不同，保留条件差异。

初次隔离探针加入布局起止时间与GC计数，但构建时未传仓库标准的MSBuildEnableWorkloadResolver=false参数，触发MSB4276；复制的旧NuGet资产路径另引发NETSDK1064。原始诊断日志、binlog、资产路径和该次未编译补丁已归档。随后按仓库既有参数、同版本SDK8.0.423构建成功并追加三轮固定时钟测量，见[修正参数后的GC/时间戳复测](evidence/U09-04-correlation-20261030/fixed-gc-rerun-20261030/BASELINE.md)。没有修改生产源码、SDK或门禁。U09-04仍进行中；新的内容布局长尾未复现，U00原始帧/累计分配和真实宿主验收仍缺，不能把Rendering回调当物理帧，也不能把GC重叠当因果。

分析与限制：[U09-04初次诊断与三轮长尾分析](evidence/U09-04-correlation-20261030/BASELINE.md)；[固定时钟GC/时间戳补充复测](evidence/U09-04-correlation-20261030/fixed-gc-rerun-20261030/BASELINE.md)。

## 2026-10-30 Round5提交4e0fb814 GitHub CI远端待验

提交4e0fb814f7ec2040383a9f34e197790468d6f599已推送main。push适用.github/workflows/windows-build.yml中的build和sdk10-build。现有run查询只过滤PR触发，返回空；combined status也无status，当前接口拿不到push run/job/artifact身份，因此不能推断通过。下一步用有权限的Actions读取接口按精确SHA查两job终态、所有步骤和artifact head SHA/digest；若当前连接器仍不支持，由有权限维护者执行只读查询。记录：[CI查询结果](evidence/U09-01-U07-01-04-pair-20261030/ci-4e0fb814.json)。

## 2026-10-30 U09-01补U07-01/02与U07-03/04历史配对

按相同假数据与固定 UTC `2026-10-28T09:00:00Z` 重建三份历史生产快照：`ac0afe72`（U07-01/02前）、`2aba614f`（U07-01/02后且U07-03/04前）、`7c4d0775`（U07-03/04后）。SDK8.0.423 Release下每份Worker和最终RenderHarness构建均exit0、0 warning/0 error；26次定向捕获均exit0，双主题对话框覆盖1040×700与1366×768 DIP。保留18张SHA去重唯一PNG。U07-01/02前版单行动作溢出，后版WrapPanel将长确认移到第二行且保留正文滚动；U07-03搜索提示从Mouse移至控件下方；U07-04必要列始终禁用，新增禁用态可见Tooltip并与UIA HelpText一致。测量、源码/程序集/图片哈希、日志和范围限制见[配对证据](evidence/U09-01-U07-01-04-pair-20261030/BASELINE.md)。

本批仅更新证据、未改生产源码；RenderHarness提供隔离用中性`BaseTextBlockStyle`，不代表Playnite宿主。未执行真实恢复、媒体删除、云端写入或菜单动作；未验Demo对照、Playnite、外部UIA、实体键盘/IME及物理DPI。U07-01～04保持“代码完成待验收”，U09-01继续盘点其它批次。状态仍为6项已满足、71项代码完成待验收、2项进行中、1项远端待验。仅清理本批自有`.tmp`目标，C盘空闲实测从4,172,505,088增至5,007,486,976 bytes（+834,981,888），D盘不变；删除后执行器未保存递归文件数/字节数，记录为未知。路径清单和空间见[cleanup.json](evidence/U09-01-U07-01-04-pair-20261030/cleanup.json)。

## 2026-10-30 U09-01 补U08-07/08历史配对

按固定样本 UTC `2026-10-28T09:00:00Z`、SDK8.0.423 Release完成三个历史快照；Worker、RenderHarness及render-qa均exit0，每份334 PNG、0 PROBLEM。U08-07 `9181700f`→`d4c4357c`的通知无结果状态补齐浅/深主题、1040×700/1366×768前后图；U08-08 `fd2d1316`→`9181700f`补齐Trainer浅/深主题、双尺寸前后图，4组像素哈希均相同。固定样本注入仅存在于隔离压缩快照。完整记录、脱敏日志、报告、源码包/程序集/夹具哈希及目标截图见[本批证据](evidence/U09-01-U08-07-08-pair-20261030/BASELINE.md)。

本批仅更新证据文档，未改生产源码。清理本批自有`.tmp/u08-07-08-pair-20261030`共1225个文件/164,212,806字节；C盘空闲从5,213,196,288增至5,379,444,736字节，D盘保持18,824,085,504字节；逐文件清单与边界、占用检查见[cleanup.json](evidence/U09-01-U08-07-08-pair-20261030/cleanup.json)。U08-07/08仍为“代码完成待验收”；离屏截图不替代Playnite、外部UIA、实体输入、物理DPI或真实跳转验证。U09-01继续盘点，其余待配对批次未结项；当前状态仍为6项已满足、71项代码完成待验收、2项进行中、1项远端待验。

## 2026-10-29/30 5040ac5 精确SHA CI通过

windows-build.yml run [40958239784](https://github.com/Nikilua/GameSaveCenter/actions/runs/40958239784) 的公开run记录确认状态 completed/success，head SHA 为 5040ac555c3d202b144c4ca9730a87978d139252。baseline job 123743921559 与 SDK10.0.401 job 123743921221 均 success，所有 workflow 步骤完成。三个 artifact 均绑定相同 head：Windows preview 12937747846 / sha256:3b67364eb23b3651011fc372f9f222ee7f9b8f303672f14217c9ccd8d1abe13e；build diagnostics 12937583089 / sha256:210c25bd803e9490874ce902f92c20c37a3c46390a8956727f3da073c0d49dde；SDK10 diagnostics 12936770001 / sha256:bbf7236c8ff949397e2ede03a953c1402264f3950060b85e32d583c3a803ac8f。required-check/branch-protection名单仍因先前403权限限制为远端待验，不重复探测。

## 2026-10-30 U09-01补U08-05/06双主题配对

U08-05历史快照`d6d940e7`→`d19570fa`及U08-06快照`b9bdd2d3`→`e81c17ce`均已按固定样本UTC补齐浅/深主题、1040×700与1366×768配对。U08-05改后1366窗口可见“正在启动所选工具…”；两侧完整Render QA均exit0、0 PROBLEM，Worker与RenderHarness构建exit0。U08-06维护动作保持相同三条假数据，改后按“需要人工处理/等待自动重试/例行巡检”分组并保留更多操作展开；完整历史Render QA全局分别有26/25个既有Media视口与侧栏切换问题，但目标维护/工具页8个主题×尺寸行均OK，未把全局失败当通过。完整报告、目标图、源码包SHA、临时测试注入哈希见[evidence](evidence/U09-01-U08-05-06-pair-20261030/BASELINE.md)。本批自有`.tmp/u08-05-06-pair-20261030`清理18,652个文件/1,412,199,397字节；C盘空闲从3,973,689,344增至5,437,734,912字节，D盘保持18,827,005,952字节，SDK8.0.423保留。路径、SHA复制核验和空间见[evidence cleanup](evidence/U09-01-U08-05-06-pair-20261030/cleanup.json)。

冻结主线`5040ac555c3d202b144c4ca9730a87978d139252` SDK8.0.423 Release相关行为5/5、0失败、0 skip；源码校验exit0；WPF静态审查26 XAML/0 error/26 warning/164 info。压缩快照上一次测试因缺少`GSC_BUILD_COMMIT`触发身份守卫失败，原始log/TRX保留、不计通过。未改生产源码，未运行真实工具/恢复/删除/云端写入；Demo树不在本工作树，Playnite、外部UIA、实体输入和物理DPI仍待验收。U08-05/06保持“代码完成待验收”，U09-01继续盘点；当前状态计数仍为6已满足、71代码完成待验收、2进行中、1远端待验。
## 2026-10-30 U09-01提交CI远端待验

证据提交`f8feee813c9aaebecec5ff3f644c8038a599685c`已推送到`main`。该push适用`.github/workflows/windows-build.yml`的`build`（编译、测试与打包）和`sdk10-build`（SDK10.0.401编译与测试复核）。本轮GitHub连接器返回HTTP 429，公开Actions API也触发未认证速率限制；本机无`gh`命令，Computer Use初始化因kernel资源路径不存在失败。没有run、job或artifact身份可读，因此两项均记“远端待验”，不推断通过。限流恢复后，以精确head SHA查询Actions runs，再核对两job终态、步骤与产物digest；必要时由有权限维护者执行同一只读API查询。

## 2026-10-30 U09-01补U08-01/02同数据历史配对

对照历史生产快照`18481e6d8d3415ffc31362764980614e76445848`→`fd2d1316be9ff2d65b0851ac57c45e40064e50ac`，生产差异仅为概览眉题“待处理”改为“当前状态”，以及存档历史摘要显示所选游戏名并提供身份Tooltip/UIA HelpText；命令、列表、恢复前置与滚动行为未改。两侧以SDK8.0.423 Release分别构建真实Worker和RenderHarness，完整Render QA均exit0、394 PNG、0 PROBLEM；固定UTC及运行任务时长后，38图变化、356图像素相同，任务页无变化，双主题目标图已复核。after行为测试1/1通过、0失败、0 skip。当前生产子树WPF静态审查26 XAML/0 error/26 warning/164 info；Overview两条是容器结构启发式误报，渲染无问题。证据、日志、TRX与哈希见[同数据历史配对](evidence/U09-01-U08-pair-20261030/BASELINE.md)。

本批未改生产源码。技能规则指定的`GameSaveCenter.AcrylicFork/.../Design` Demo资源不在此工作树，直接Demo视觉对照未完成；U08-01/02继续“代码完成待验收”。离屏结果不代表Playnite、实体UIA/输入或物理DPI验收，且未执行真实恢复/媒体删除/云端写入。U09-01仍进行中，其它历史配对继续盘点。仅清理本阶段`.tmp/round5-u09-01-pairs-20261030`，本工作树SDK8.0.423副本另行保留；清单和空间前后见[清理记录](evidence/U09-01-U08-pair-20261030/cleanup.json)。当前6项已满足、71项代码完成待验收、2项进行中、1项远端待验。

## 2026-10-30 U09-01补U08-03/04历史配对

生产快照`c5b03e576edddfe7a5f06172e081407eb9a5ee62`→`18481e6d8d3415ffc31362764980614e76445848`。同一固定UTC/任务时长/缺失媒体样本在SDK8.0.423 Release下完成双侧 Worker、RenderHarness 与 Render QA：各394 PNG、0 PROBLEM、exit0；逐图390张相同、4张 Media tab1 大窗图变化。专用探针在浅/深色媒体缩略卡都确认缺图与视频说明文字从空白变为“媒体文件不存在”/“录像预览”；UIA HelpText 为非视觉变化，任务页双主题前后截图哈希相同。首轮 before 报告中的 SettingsState 断言失败已保存；分别构建真实快照 Worker 后最终双侧通过。既有当前主线行为测试15/15、0失败、0 skip，目标源码至该测试基线无变化。证据与产物SHA见[evidence](evidence/U09-01-U08-03-04-pair-20261030/BASELINE.md)。Demo树缺失、Playnite/外部UIA/实体输入IME/物理DPI未验；真实恢复、媒体删除和云端写入未运行。U08-03/04仍“代码完成待验收”，U09-01继续盘点；状态计数不变。

## 2026-10-30 U09-04当前源码固定轮次性能补测

SDK8.0.423 Release当前探针构建exit0，三轮均70周期/297操作/30秒静置/0失败。当前活动期托管分配8.8775–8.8779GB，与e6两轮8.8767–8.8769GB基本持平；合成搜索与滚动p95接近。当前Rendering回调p95为338.073–360.335ms、>33.3ms占18.6–20.5%，工作区逻辑布局p95为709.49–854.35ms，均高于e6对应的286.317–305.010ms、14.4–16.6%和388.30–413.79ms；中位数接近，逐页布局中位数也接近，但当前逐页最大样本较高。此为离屏尾部信号，未归因到页签模板或动画。旧e6的时间字段相同但pinned=False，e6同探针重建又因便携SDK缺workload locator目录exit1；两项差异均已记录，不能宣称完全同环境复测。U00原始逐帧与累计分配缺失，真实Playnite、实体输入/IME及物理呈现帧未验，U09-04保持进行中。[固定轮次与逐页对照](evidence/U09-04-currentperf-20261030/BASELINE.md)

本阶段只清理自有`.tmp/u09-04-currentperf-20261030`临时工作目录；预检、归档SHA和空间前后见[cleanup.json](evidence/U09-04-currentperf-20261030/cleanup.json)。当前6项已满足、71项代码完成待验收、2项进行中、1项远端待验。

## 2026-10-30 U09-01补U07-07/08历史配对

同主题、同视口锚点的历史生产快照配对：设置页 `a76de89f`→`c5b03e57`，任务页 `17d883ab`→`0aff7f32`。同一离屏探针、SDK8.0.423 Release编译和四次双主题捕获均exit0；设置和任务各保留浅/深色前后图共8张，任务样本时间固定，UIA HelpText与重试指引正文一致。首轮1040×700设置图裁切且入场动画未收敛，已排除；最终设置图使用1600×900 DIP、静止动效及同控件视口锚点。构建4条既有NU1900警告、0 error；日志、检查输出、探针与逐文件SHA已归档。[历史配对证据](evidence/U07-07-08/historical-render-pair-20261030/BASELINE.md)。

归档后仅清理本阶段`.tmp/u07-visual-pairs-20261030`（2,708文件/125,652,193字节），完整性、路径与占用预检通过；空间及删除清单见[cleanup.json](evidence/U07-07-08/historical-render-pair-20261030/cleanup.json)。

本批未改生产源码，U07-07/08仍为“代码完成待验收”；真实Playnite、外部UIA、实体键盘/IME与物理DPI未验，真实恢复/媒体删除/云端写入未运行。U09-01继续进行，剩余其它批次配对仍需盘点。当前6项已满足、71项代码完成待验收、2项进行中、1项远端待验。

## 2026-10-30 U09-05冻结门禁通过

5c0c57b06aed432 (完整SHA见证据) SDK8.0.423；中文包装器完整链exit0、独立scripts/build.cmd exit0。两入口各158 TRX/2020项：1962通过、0失败、58既有skip，skip不计通过；PEXT 45,737,311 bytes，SHA256 `bce032ef9d6c95aa145d0c99a69ee3be16ef7d8dda59e8964db10bc2eb076ee6`，六个包内/安装DLL逐字节相同。Playnite启动PID30324并响应，实际页面未观察。精确SHA的GitHub run40900374673，两项适用job均success，三artifact的head SHA/ID/digest已核。required-check API既有403未重探，仍待有权限维护者查询。soak一次File.Move访问拒绝完整诊断已归档；新门禁通过但旧失败未查明唯一原因。证据：[冻结本地与CI身份](evidence/U09-final-5c0c57b0/BASELINE.md)。

Computer Use依仓库Skill使用node_repl/@oai/sky，本轮初始化返回kernel assets路径不存在(os error 3)，此前也出现同错；停止自动化。插件可视页面/外部UIA/物理输入DPI未验，相关项继续标代码完成待验收；真实恢复、媒体删除、云写入未执行。required-check接口下一步为有权限维护者执行`gh api repos/Nikilua/GameSaveCenter/branches/main/protection/required_status_checks`。

## 2026-10-29 U09-01补U04-06/08中断与复用对照

两侧历史生产src逐字节核对未改，SDK8.0.423单节点编译退出0（NU1900网络审计警告保留）。真实披露卡双主题：旧版中途禁用仍保留动画时钟，新版立即回到合法展开角度且无时钟；重新启用不重播。主题重应用前后都替换Transform，因此100次开合按主题完成后的新基准测量，两侧Header/Path/Transform身份均稳定，不虚构复用改善。最新20图及三轮CSV、源码与探针身份已归档；真实取消/窗口失活、Playnite及物理输入DPI仍待验，当前生产源码未改，U09-01保持进行中。

[双主题历史图与采样证据](evidence/U09-01-U04-interrupt-20261029/BASELINE.md)

## 2026-10-29 U09 Worker soak失败待定因

2c4ad8c3冻结中文根链在Worker原子移动时访问拒绝，退出1：Core157 pass、Worker400 pass/1 fail，未进入Playnite测试、独立build.cmd及新安装。原夹具已清理且异常缺少路径；同DLL同临时目录定向1/1通过，不能宣称根因已修复。只补路径/轮次/属性/HRESULT诊断，原异常继续抛出，未重试或放宽门禁。补诊断后完整Worker400 pass/1动态Named Pipe受限skip/0 fail，该skip不计通过。生产源码未改；新候选仍需冻结完整链及双SDK CI。当前5已满足、71代码完成待验收、3进行中、1远端待验。

[原始失败与补诊断记录](evidence/U09-soak-diagnostic-20261029/BASELINE.md)

## 2026-10-29 U09渲染帧取样修正候选

392854b3完整根链通过对话框类后，R08首次hover未采到中间值，退出1；局部1445 pass/1 fail/19 skip，独立build.cmd未启动。改用Rendering回调观察实际计算帧，保留原谓词、500/250ms超时及已有fixture时长；finally回收事件/timer，宿主补UIA名称。SDK8类4/4、0 skip，进入/退出/重入2/2/3帧、重置0。本轮未复现旧失败，不宣称已证明唯一根因；生产源码与392/388相同。新候选仍须独立冻结全链及双SDK CI，旧失败不擦除。当前5已满足、71代码完成待验收、3进行中、1远端待验。

[R08原始失败与帧采样证据](evidence/U09-render-sampling-20261029/BASELINE.md)

## 2026-10-29 U09对话框计时断言修正候选

388c46ea中文根链退出1，失败于R08对话框进入动画的UTC耗时下限；局部1440 pass/1 fail/19 skip（73/155 WPF类），独立build.cmd未启动。原失败缺少实际耗时，未确定唯一根因。只把测试经过时间/1500ms期限改用Stopwatch，保留100ms下限与所有断言/skip，增加1秒资源校验和双时钟诊断；为宿主补UIA名称。最终类3/3、0 skip。生产源码与388相同，不重做已有394图Render QA；新测试候选仍须提交后冻结全链及双SDK CI，不以旧通过或定向通过冒充。当前5已满足、71代码完成待验收、3进行中、1远端待验。

[本地失败与计时修正](evidence/U09-dialog-timing-20261029/BASELINE.md)

## 2026-10-29 U04页签提示条避让与状态保持候选

U09-01补齐U04-01/04/07原始生产快照的同数据双主题图与实际时间采样，发现提示条覆盖标题；当前e6基线的共同坐标几何断言也准确失败。共享Redesign模板将提示条移入既有左留白，保留旧7×16 DIP测量下限，内部页签也保留至少3 DIP读区间距；禁用0.42透明度改在共同视觉根层生效，保持旧整体变暗而不令提示条全亮。新增状态回归先捕获初版透明度偏差，再修正。最终聚焦2/2、完整R23类12/12、0 skip；源码校验通过，生产26 XAML静态0 error，最新Render QA394图/0 PROBLEM/退出0。前后样图已复核。本候选仍未提交冻结及跑自身完整门禁，不能以e6通过替代。

此前e6e7f3b9冻结中文根链和独立build.cmd均退出0，各1961 pass/58既有skip；六个安装DLL与PEXT字节一致，Playnite新进程及加载日志已核。run40851428895的SDK8 job123371589585及SDK10 job123371590197均success，三artifact同SHA与digest已归档。两轮重建补测分配约8.88GB，比未优化后快照低29.1%，比历史前快照低20.2%；不代表实体输入/呈现帧，也不替代缺失的U00原始基线。旧Q06间歇失败仍不作唯一根因断言。

新候选提交后必须重新冻结跑中文根链、独立build.cmd与双SDK CI；真实宿主/物理输入DPI及required-check既有403边界保留，不重复权限拒绝。当前5已满足、71代码完成待验收、3进行中、1远端待验；U09-01其它批次配对、U09-04实际宿主性能和U09-05新源门禁仍未结项。已删除470张本阶段旧PNG，保留当前394图、有效配对、全部TRX/日志及未知文件，哈希/字节/空间清单已归档。

[e6完整门禁](evidence/U09-final-e6e7f3b9/BASELINE.md)；[U04历史配对](evidence/U09-01-U04-pairs-20261029/BASELINE.md)；[新页签修正](evidence/U04-indicator-gap-20261029/BASELINE.md)

## 2026-10-29 U09主题资源发布优化候选

性能拆分确认主题重应用是主成本（前10.572GB、后11.754GB）；新增DynamicResource回归旧代码4/4失败，证实每次2～3次重复通知。生产管线改为无宿主字典归并后逐键发布最终值；修复首版漏空值导致的高对比阴影回归，七个可空效果键显式保留。最终五类150 pass/0 fail，39既有撤销架构skip；源码与WPF静态通过（0 error），补建真实Worker后render-qa退出0、394图、0 PROBLEM。缺构建身份/缺Worker和首版高对比失败均归档，没有改断言或门禁。此为候选，尚未性能复测或冻结全链/双SDK CI；U09-04/05保持进行中，Q06间歇失败不宣称已修复。当前5已满足、71代码完成待验收、3进行中、1远端待验。

[发布优化候选及失败/修复证据](evidence/U09-palette-publication-20261029/BASELINE.md)

## 2026-10-29 U09性能重建补测发现分配回退

固定Fake时钟的历史0447237a→acebe07d生产快照，双主题七页1040×700 DIP、70周期297操作、30秒settle，前/后/后/前四轮及一对逐页诊断均退出0。后快照活动期托管分配约12.52GB，前约11.12GB，稳定增加约12.58%；七页都增加，任务页增量最大。U09-04仍进行中，下一步拆分主题资源重应用与布局成本；不能把Rendering回调间隔当物理呈现帧、把重建数据充当原始U00或把退出0充当性能通过。当前生产源码未改。Q06三次定向诊断通过但未复现旧失败，U09-05仍进行中；不追绿重跑整链。当前5已满足、71代码完成待验收、3进行中、1远端待验。

[性能补测与回退证据](evidence/U09-04-fixedcycles-20261029/BASELINE.md)

## 2026-10-29 Q06按压恢复待诊断

2d248548中文根链退出1：已通过媒体滚动类，但第30类Q06按钮五态的按压恢复断言失败；部分1321项为1302 pass/1 fail/18 skip，独立build.cmd未启动。失败TRX和根诊断已归档。仅给原断言补状态、缩放、覆盖层、焦点、窗口激活信息，所有谓词不变；一次有明确目的的定向复现通过，但未复现原失败，不能视为修复或完整链通过。下一步区分合成Space、原生捕获与焦点干扰，不无依据重跑整链。U09-05仍进行中，源码/完整链/CI门禁均不以旧通过替代。

[Q06待诊断](evidence/U09-05-q06-20261029/BASELINE.md)

## 2026-10-29 U09-02/03媒体STA清理候选

f1eb8f8f在C盘真实中文根链退出1：MediaInboxScrollBehaviorTests的WPF TextStore窗口移动回调跨STA访问导致宿主崩溃。该类没有发布用例结果，Sequence标记outputScale1.25未完成；已完成部分1227 pass/18 skip不能代表完整通过，独立build.cmd未启动。诊断及Sequence已用短路径归档。

新增保留IME启用的真实TextBox生命周期回归，旧夹具因Dispatcher未关闭准确失败。仅在共享RunSta finally中于所有者线程InvokeShutdown，清理异常亦返回失败，未关闭IME或改滚动/锚点/虚拟化断言、三种输出缩放或skip。按正式隔离配置连续三轮7/7，共21/21、0 skip。缺失清理已被直接证明，旧崩溃唯一因果不作过度归因；新候选仍须中文根链、独立build.cmd及双SDK CI，U09-05继续进行中。

[STA清理证据](evidence/U09-02-03-sta-20261029/BASELINE.md)

## 2026-10-29 U09归档路径兼容修正

隔离工作树首次检出因两批归档路径过长失败：长WPF类名在目录及TRX文件名重复。已改为每个run内trx/短编号.trx，清单保留原路径、原始SHA、旧副本SHA及新副本SHA；逐份校验测试名称/结果相同，文本仅归一换行及行尾空白。未修改Git全局配置、源码、workflow或SDK。首次检出失败不记构建通过；下一候选仍需完整链与双SDK CI。旧清理权限拒绝未重试，旧构建发现测试数据库后保留未删。

## 2026-10-29 U04-02/03采样截止修正候选

acebe07d中文根链SDK8.0.423退出0，157份TRX为1955 pass/58既有skip，六个安装DLL与PEXT逐字节相同；新宿主进程68964及14:07:12日志确认加载GameSaveCenter0.6.73，但未验真实页面或运行时模块SHA。独立build.cmd在第75类R08 hover重新进入未采到中间帧失败（局部1463项：1443 pass/1 fail/19 skip），完整交付门禁未通过；原始诊断归档，未追绿重跑旧链。

新增有限Render队列复现：旧1ms采样截止等到40次Render全部结束才返回，准确失败。仅将测试采样截止计时器改为Send优先级，保留生产时长、所有原断言、超时和skip。三轮直接运行12/12断言通过，但额外Test AppDomain有既有COM卸载异常，日志保留、不记为无异常门禁；按仓库正式DisableAppDomain=true复核4/4、0 skip且无该异常。此复现证明采样器可被饿死，旧hover失败未记录迟到耗时，不宣称回溯了唯一根因。

生产资源未改。新候选提交后须重新完整构建安装链、独立build.cmd和双SDK CI；D盘空间不足时以同SHA的C盘隔离工作树执行，不降低空间预算或替换SDK，不重试旧权限拒绝。当前5项已满足、71项代码完成待验收、3项进行中、1项远端待验；U09-05重新进行中，U09-01其它配对及U09-04重建补测继续。required-check名单仍按既有403远端待验，不复探。

[采样截止修正候选](evidence/U04-02-03-sampling-deadline-20261029/BASELINE.md)

## 2026-10-29 U03-04禁用原生文字修正候选

清理说明：当前保留SDK位于 .tmp/round5-u09-01-03/dotnet-sdk8；已删除逐文件SHA相同的旧SDK副本、四份已由cd062531构建1a593ccc替代的Round5输出及四份过期渲染副本。旧文字中的当前输出仅代表当时状态，现以本条和清理清单为准。原始失败诊断、最新包/构建与当前审计继续保留；D盘由约2.32GB恢复至4.33GB，未降低门禁。

历史同状态对照发现共享TextBox只改变PART_ContentHost附加Foreground，原生编辑文字仍读取控件Foreground；新增实际属性断言在修改前准确失败（预期#FF9AA4B5、实际#FFF2F4F8）。共享模板改为设置TextBox本身Foreground，沿原TemplateBinding传给内容宿主；补重新启用恢复主文字色断言，错误描边/HelpText和尺寸语义保留。

SDK8.0.423 Release定向8/8、0 skip；首次扩展运行因未传构建身份导致6个源码测试被门禁拦截，补真实HEAD元数据后通过，失败TRX保留。双主题同状态图片已检查；源码校验、XAML26/26、WPF静态42文件/0 errors（既有26 warning/179 info）及完整Render QA394 PNG/0 PROBLEM/exit0通过。当前为源码候选；提交冻结后还须本候选中文根入口完整链、独立build.cmd及双SDK CI，不能沿用cd062531通过结果。真实Playnite、实体输入/IME/UIA及物理DPI继续待验。

[原生禁用文字修正候选](evidence/U03-disabled-native-text-20261029/BASELINE.md)

## 2026-10-29 U09-01补U03-04/05输入状态对照

重建f59fe4c8→d1fc1308（实际父88e1e8f3与f59生产src树相同），Fake固定UTC且源码相同，src逐文件与ZIP核对未改。SDK8.0.423 Release Worker/Harness及探针重编译均0警告0错误，两侧原RunThemeQa补充矩阵0 problems/exit0；双主题六种输入组合尺寸均480×42，After错误HelpText可读，输入探针两版均exit0。v1说明文字资源键错误导致标签不可见，v2已修正探针并保留首轮诊断；两侧双主题状态图及任务/媒体搜索页图已保存。

视觉复核发现禁用态原生文字仍接近启用态主文字色；现有“PART_ContentHost附加Foreground”断言不能证明原生编辑文字实际前景色。本批只补历史证据，不将这一点记为验收通过，下一批核查当前共享TextBox。未改当前生产源码；真实Playnite、实体输入/IME/UIA及物理DPI仍待验，U09-01保持进行中。

[输入状态历史对照](evidence/U09-01-U03-input-pair-20261029/BASELINE.md)

## 2026-10-29 U09-01补U02-05/06圆角与线宽对照

以已归档504562a7为前图（与实际父提交3f710530的src树相同），重建417bed51圆角/裁切与f59fe4c8线宽令牌两侧，分别保存两个变化边界。Fake源码无差异，时钟固定为2026-10-28T09:00:00Z；ZIP注释/哈希及src逐文件核对证明生产源码未改。两侧SDK8.0.423 Release Worker/Harness编译均0警告0错误，原RunThemeQa补充矩阵和四次双主题finesseprobe均exit0，矩阵0 problems。共享控件双主题图已视觉检查；线宽令牌收敛允许终态外观相同，不虚构视觉差异。

本批只补历史静态证据，不改当前生产资源，不冒充完整Render QA、安装链或物理DPI验收。U02-07/08原批只有专项复核、无生产资源变化，继续引用其原行为证据。U09-01仍进行中，真实Playnite、实体输入/IME/UIA、U00性能原始基线及required-check权限边界保留。

[圆角与线宽补充对照](evidence/U09-01-U02-geometry-pairs-20261029/BASELINE.md)

## 2026-10-29 U09-01补U02-01～04主题/状态对照

原始813adb09→504562a7源码快照，Fake时间固定2026-10-28T09:00:00Z；实际父提交4448b718与813的src树完全相同。ZIP注释/哈希及src逐文件核对证明生产源码未改。两侧SDK8.0.423 Release编译0警告0错误，调用原RunThemeQa均exit0/0 problems，After双主题finesseprobe均exit0。此为补充主题矩阵，不冒充新完整394图Render QA或安装链。

生产ListBox模板/容器样式的受控投影完整展示六状态，两主题共12行均实现；After图标前景色与对应语义资源一致。真实“最近访问”按钮验证卡片Visible、任务卡片Collapsed（实际是卡片切换，不是浮层）；两侧同数据卡片图展示普通图标由强调色改为次级文字色，命令为无业务副作用的夹具。初轮整页夹具未填充无关摘要，因此最终只捕获实际卡片，初轮状态CSV作为方法诊断保留。六状态和最近访问双主题均已视觉检查。

历史“今日工作台”只存在原始源码对照，当前生产架构未回退。当前生产源码未改，物理DPI、高对比实机、Playnite、实体输入/IME/UIA及真实恢复/媒体删除/云端写入未验；不据此宣称性能验收。U09-01其它视觉批次仍缺配对，保持进行中。

[U02四项补充配对](evidence/U09-01-U02-pair-reconstruction-20261029/BASELINE.md)

## 2026-10-29 U09-01补U01四项历史对照

重建0447237a→7e61bba3→813adb09三个源码快照，分别补U01-01/02标题/混排及U01-05/06禁用文字/图标批次。Fake时间同为2026-10-28T09:00:00Z，生产wall-clock未改；src逐文件与原始ZIP核对未改。三侧SDK8.0.423 Release Worker/Harness编译均0警告0错误，各394 PNG；标题和禁用文字两侧完整Render QA exit0。最早Before因Sidebar rapid-toggle未收敛exit1，失败报告保留，未重跑追绿；它只用于静态前图，不记整体验证通过。

六次原有finesseprobe双主题exit0；Shell标题和控件禁用态样图已视觉检查。42个页面/控件主题引用以内容哈希去重保存，包含存档、任务、媒体、工具、通知及Shell；两组变更边界分开记录。仓库Git报告会读取父仓库，历史身份以ZIP注释/哈希及源码核对证明。本批不改当前生产源码，不新增真实恢复/媒体删除/云写入；实际Playnite、物理DPI、缺失可选字体、实体输入/IME/UIA仍待验。U09-01其它视觉批次对照仍缺，保持进行中。

[本次U01配对证据](evidence/U09-01-U01-pair-reconstruction-20261029/BASELINE.md)

## 2026-10-29 U09-01补U06-07/08候选表配对

原始3ecacdc0→ac0afe72隔离快照，固定Fake数据UTC为2026-10-28T09:00:00Z；ZIP注释/哈希及src逐文件核对证明生产源码未改。SDK8.0.423 Release两侧编译0警告0错误，补充截图探针exit0。复用历史RenderTabs/SavePng路径，保存1040×700及3840×2160逻辑窗口双主题候选页前后配对；实际内容744×460与3544×1920。两主题窄窗表高310 DIP不变，高窗由560扩展至1664.67 DIP，8条同样本候选。主题、表格、详情和滚动区域已视觉检查。

首两轮原生Window探针虽然尺寸断言通过，但图片有未呈现区域，因此不作为验收图；诊断CSV/示例图保留。复用原始独立Grid捕获后完整呈现，具体原生窗口/DPI根因未证明。本批只补静态候选页对照，不冒充新的完整394图Render QA、大列表性能或真实Playnite/物理4K验证。U06-08行为证据仍按原批次范围引用；U09-01其它批次配对仍缺，继续进行中。

[本次补配对证据](evidence/U09-01-U06-pair-reconstruction-20261029/BASELINE.md)

## 2026-10-29 U05-01/02动态时间采样补齐

在原始前后源码隔离快照中加入同一诊断探针，通过生产NavigateTo和详情按钮采样；逐文件核对src/与原始git archive完全一致，未修改生产动画时长。两侧SDK8.0.423 Release编译0警告0错误、探针双主题exit0。CSV分开记录同步action_ms和返回后实际时间；调度目标不等于实际时间，包含布局/截图开销，不用于p95、长帧或累计分配验收。v1/v2方法诊断和首次页面进入数据保留。

After双主题缓存页及详情进/退均采到中间透明度/位移，1600ms时归一并释放时钟；详情退出可见期间关闭命中，随后折叠。首入浅色页面因布局开销未充分采到中间帧，不隐去。已检查缓存页和详情退出指定时点截图。U09-01中的U05-01/02已具有同数据双主题静态配对及动态时间样本，整项仍因其它批次配对缺口进行中；真实Playnite、实体输入、物理DPI仍待验。

[动态采样证据](evidence/U09-01-U05-pair-reconstruction-20261029/motion/BASELINE.md)

## 2026-10-29 U09-01补U05-01/02历史配对

从原始提交48f69413与237e993f重建隔离快照，ZIP注释SHA与文件哈希证明源码身份；仅Fake夹具时钟固定为2026-10-28T09:00:00Z，生产资源未改。两侧SDK8.0.423 Release Worker/Harness编译均0警告0错误，完整Render各394 PNG、0 PROBLEM、exit0。1040×700双主题任务页前后文件SHA相同，已视觉检查；每主题仅存一份图并由两侧引用。历史报告Commit字段沿父目录读取当前仓库，不能作为历史源码身份，详见新证据pair.json。

首次快照渲染因未先编译Worker.exe导致4项SettingsState失败；补齐真实历史Worker编译依赖后通过，未改断言或校验。保留失败报告。该证据只补静态终态配对，不证明页面/详情动画中间状态；U09-01仍进行中，下一步补U05时间采样。历史快照仍在使用，后续归档核对后清理冗余渲染目录；未执行真实恢复/媒体删除/云端写入。

[本次配对证据](evidence/U09-01-U05-pair-reconstruction-20261029/BASELINE.md)

## 2026-10-29 通知来源批次产物清理

核对321份归档SHA全部一致后，仅删除本批`.tmp/round5-u07-05-source-20261029/`下10个明确目录，共323,762,007 bytes；路径边界、重解析链接及独占打开检查均通过，D盘前后实测增加330,719,232 bytes。保留标准根构建、根诊断、最新包、SDK及其它任务TRX/TestResults；清单见Round5通知来源证据`final-gates/cleanup.json`。U07-05代码完成待验收，U09-01配对/U09-04基线/U09-06保护名单边界继续保留。

## 2026-10-29 U07-05冻结候选门禁

源码提交 `cd06253158c0e35d43de84e037548a764629749a` 已推送main。SDK8.0.423 Release中文根入口完整构建、测试、发布、打包、安装身份核验及启动调用退出0（run `20261028T235825476Z-d43f8012`）；独立 `scripts/build.cmd` 规范输出树全套退出0。两次各2013项、1955通过、0失败、58个既有skip（57个撤销架构断言、1个opt-in系统指针测试），skip不计通过。

GitHub run [40794111982](https://github.com/Nikilua/GameSaveCenter/actions/runs/40794111982)的baseline job `123186245250` 与SDK10.0.401 job `123186245559`均成功；baseline源码、诊断自测、SDK基线负例、构建测试、Render QA、WPF静态、打包、上传均成功。artifact IDs为`12865861325`（baseline诊断）、`12865448641`（preview包）、`12863324535`（SDK10诊断），digest见本批证据 `final-gates/ci.json`。保护required-check名单此前403，本批未复探，U09-06仍远端待验。

首次受限build.cmd多出10项Named Pipe/Worker环境skip；提权临时输出树一次Q10复选框合成空格事件2项失败。保留失败TRX，未修改断言或skip。规范输出树完整复跑通过，但根因尚未证明，不能将路径认定为已证实原因。根入口完成启动调用，后续进程查询未见Playnite，因此持续运行、插件已加载及真实页面视觉仍待验；外部UIA、实体IME/键盘、物理DPI未验，真实存档恢复/媒体删除/云端写入未执行。

U07-05更新为代码完成待验收；U09-01仍需其它视觉批次同数据双主题配对，U09-04仍缺U00逐帧/累计分配基线。当前计数6项已满足、71项代码完成待验收、2项进行中、1项远端待验。本批临时产物已归档核验并清理，见最新清理记录。

# 第五轮 UI 与动画精修进度

## 2026-10-29 U07-05 / U09-01 通知来源与配对证据

为通知记录增加显式来源字段和SQLite加列迁移；新任务通知来源取稳定任务类型显示值，会话摘要来源取创建事件，旧记录保留空值并显示“来源未记录”。通知表标题下显示来源，详情面板也提供来源；保留标题排序/复制、Tooltip/UIA、虚拟化、滚动条和新通知不抢焦点行为。相同固定 UTC 样本、双主题、1040×700 DIP完成前后对照；SDK8.0.423 Release Worker通知定向13/13、Playnite通知/UIA定向7/7，无skip。source validation、XAML结构检查、WPF静态审查及完整Render QA均通过。证据见[evidence/U07-05-notification-source-20261029](evidence/U07-05-notification-source-20261029/BASELINE.md)。当前仅局部候选验证完成；冻结提交、中文根入口完整链、独立`scripts/build.cmd`及该提交GitHub CI待执行。来源数据以显式事件为准，不从摘要猜测；真实Playnite宿主、外部UIA/IME和物理DPI待验。

## 2026-10-29 U07-07 / U07-08 恢复说明补齐

U07-07复核设置页已有的巡检依赖说明，并新增源码守护，确认说明紧邻开关且关闭巡检时保留间隔值。U07-08在任务详情增加终态可见的“重试前检查”说明：权限不足、网络不可达、传输未完成分别引导核权限/连接/远端对象；未知云端错误要求先核实远端是否收到提交。重试命令资格、任务对象、Worker、错误/取消及云端写入流程未改。提交`0aff7f32aab4fb31940fd791c1486c67d36ebdbd`；SDK8.0.423 Release精确提交定向10/10、0失败、0 skip。Source validation退出0；WPF静态42 XAML / 0 errors / 26既有warnings / 179 info。固定UTC Render QA候选394 PNG / 0 PROBLEM / exit 0；报告保留其采集时工作树身份`17d883ab`，精确SHA还须由当前CI渲染复核。首个View测试辅助器定位不到可见TextBlock，失败TRX保留；改用命名节点后通过。证据、SHA及清理记录见[evidence/U07-07-08-RECOVERY](evidence/U07-07-08-RECOVERY/BASELINE.md)。run `40749256118`的baseline job `123031214603`在编译测试，SDK10 job `123031215214`在SDK安装/复核启动；无artifact，不能记远端通过。Playnite宿主、外部UIA/Narrator、实体键盘/IME、物理DPI未验；真实存档恢复、媒体删除和云端写入未执行。两项更新为“代码完成待验收”。

## 2026-10-29 Round5 U09-05/06/07/08 冻结候选最终门禁与整轮审查

- 精确源码候选 `5f0eb6209ccfb33c67efac36aee68aee204de614`。源代码修复只为隔离的键盘/UIA列菜单测试停用无关的WPF stylus/touch栈，规避宿主 `StylusWisp` Popup捕获崩溃；不改生产程序集、XAML、资源、Playnite触控或业务语义。失败原因和聚焦修复证据见[evidence/U09-05-wpf-stylus-fix](evidence/U09-05-wpf-stylus-fix/BASELINE.md)。
- 用户说的 `run.cmd` /“一键构建运行”映射到真实入口 `GameSaveCenter-一键构建安装运行.cmd` → `GameSaveCenter-Run.cmd` → `scripts/dev-install-run.ps1 -Configuration Release`。在冻结跟踪文件、SDK8.0.423、Release下完整构建/测试/发布/打包/安装身份核验/启动均完成，实际根退出码0；独立 `scripts/build.cmd` 也以隔离输出/诊断完成其构建与测试职责，退出码0。每次2012项、1954通过、0失败、58跳过；157份TRX计数完全匹配，无错误/超时/中止，154个WPF隔离类完成。58个skip仅为57个已撤销“今日工作台”断言和1个默认opt-in的系统鼠标指针测试，未算成通过，见[本地门禁、安装身份与TRX归档](evidence/U09-05-final-5f0eb620/BASELINE.md)。
- 最终本地包 `GameSaveCenter-0.6.73-playnite.zip` / `.pext` 均45,736,111 bytes、SHA-256 `F80780FE18D890610C10B881D1B21D7D9967CE7EFFEBF265B3EDF3DB4D514DBD`。安装版本 `0.6.73+5f0eb620...`，主插件DLL SHA-256 `220BE2409571A14902F1350BBD48655950EF974C94E857D1C7CE5E1EB0859F02`；Playnite PID42296启动成功。启动不等于真实页面视觉、外部UIA、实体键鼠/IME/物理DPI验收；本轮未执行真实存档恢复、媒体删除、云端写入。
- 精确SHA的 `windows-build.yml` run [40768381673](https://github.com/Nikilua/GameSaveCenter/actions/runs/40768381673) / #1374 总结success。baseline job `123098598311`（SDK8.0.423：测试、Render QA、WPF静态审查、扩展包、上传、诊断全部成功）；SDK10.0.401 job `123098597735`成功。三份artifact身份和digest见[GitHub CI记录](evidence/U09-05-final-5f0eb620/github-ci.json)。
- required-check名称/branch protection读取仍受先前403权限限制；按用户要求不重复相同探测，不把两个workflow job通过推断成保护规则已齐全。U09-06只对此项标“远端待验”，下一步由有权限维护者/连接执行 `gh api repos/Nikilua/GameSaveCenter/branches/main/protection/required_status_checks`，对照两个workflow job。
- U09-07只清理本阶段拥有、路径已规范化在仓库`.tmp`且无重解析点/占用/Markdown引用的输出。两次U09-05临时目录合计1386个文件、187,568,148 bytes，D盘实测空闲增加191,361,024 bytes；此前U09-01-03目录的869个旧构建/测试/离屏图/日志输出合计113,271,898 bytes，D盘实测空闲增加115,417,088 bytes。其9份失败TRX（10个失败结果）先脱敏归档；现有失败CI证据与用户/其他任务`TestResults`未动。唯一保留的SDK8.0.423本地工具及bootstrap脚本是因为系统全局仅有SDK9.0.302且`global.json`要求8.0.423；最新构建目录、诊断、包、安装报告和当前文档证据均保留。清单、释放前后空间及留存理由见[清理记录](evidence/U09-05-final-5f0eb620/cleanup.json)。
- U09-08已完成80项逐项盘点与去重审查；本表80项逐行状态未以无证据方式结项。当前局部门禁完成后的状态计数：6项已满足、70项代码完成待验收、3项进行中（U07-05源码候选完整根入口/CI待验；U09-01跨批同数据双主题配对不全；U09-04缺U00逐帧/分配基线）、1项远端待验（U09-06 required-check名单）。旧S/T宿主或权限状态不阻止已完成独立优化；现有真实宿主、权限与未验边界保持明确。

## 2026-10-29 U08-08 / U09-01 与 CI 回归修正

删除 Trainer 工具列表中永久折叠、与共享空态 Presenter 重复的旧文字节点；新增来源回归，守住共享 Presenter 与加载时不显示空态。修复 `d19570fa` 双 SDK CI 报出的窄窗测试夹具时序：窗口加载后再捕获可见动作的原始边距，并核实忙碌状态提示空闲时折叠。SDK 8.0.423 定向组合 16/17（16通过、0失败、1既有skip）；双主题间距用例2/2。Render QA 双主题394图、0 PROBLEM；源码校验和 WPF 静态审查退出0。细节、脱敏 TRX/Render QA 报告哈希、CI job 与 artifact 身份见[evidence/U08-08-09-01](evidence/U08-08-09-01/BASELINE.md)。修正提交的远端双SDK CI待查；真实 Playnite/UIA/IME/物理DPI未验。U08-08代码完成待验收，U09-01继续进行中，跨页同条件配对仍有缺口。

## 2026-10-29 U08-05 / U08-06 工具状态与短窗维护页

U08-05补上启动请求独立的忙碌动画/“正在启动所选工具…”UIA状态，并确保无论成功、失败或取消都会清除；来源、文件可用/缺失、自动启动和不可跟踪原因原表达及安全门控保留。U08-06用1160×520 DIP真实WPF窗口验证诊断页滚动到底可读完整长摘要、摘要内部滚动，并能返回操作卡触发原绑定复制命令。SDK8.0.423 Release定向5/5、0 skip；source validation退出0；WPF静态42 XAML/0错误/26 warning/179 info；固定样本双主题394 PNG、0 PROBLEM。工作树基线、TRX/Render报告SHA和边界见[evidence/U08-05-06-current-main-d6d940e7](evidence/U08-05-06-current-main-d6d940e7/BASELINE.md)。前置提交`d6d940e7`的run `40731004737`中baseline job `122966980646`和SDK10 job `122966980155`均通过；本批基线`d19570fa`的run `40737167888`双job失败于同一浅/深主题夹具断言，根因与修正见[evidence/U08-08-09-01](evidence/U08-08-09-01/BASELINE.md)，修正提交CI待查。真实Playnite、外部UIA/Narrator、物理输入/DPI仍待验。

## 2026-10-29 U08-03 / U08-04 当前主线行为复核

以生产源码提交 `8ac477b9fad108d714e94a7da202b5be22ef6b30` 为基线，新增任务行高状态切换回归后，SDK 8.0.423 Release 定向组合 15/15 通过、0失败、0 skip。U08-03 覆盖真实 `TaskCenterView` 中运行/失败/取消/完成替换、阶段与进度更新、长文本 UIA HelpText；U08-04 覆盖各类媒体占位恢复、2000条虚拟化列表滚动/追加/替换/移除选中项，以及行内选中与焦点边界。新增测试没有发现行高跳变；本批只改行为测试和证据，没有生产XAML/资源改动。

证据与TRX SHA、源码校验、WPF静态审查和未验边界见[evidence/U08-03-04-current-main-8ac477b9](evidence/U08-03-04-current-main-8ac477b9/BASELINE.md)。旧394图属于旧基线，本批未重跑Render QA；本机合成WPF窗口不代表Playnite宿主、实体UIA/输入或物理跨屏DPI。`8ac477b9` 的run `40728479122` 查询时 baseline job `122958046275` 和SDK10 job `122958047269`仍运行且无artifact；本批提交后的双SDK结果须按新SHA重新查询。

## 2026-10-29 U07-03 / U07-06 独立补齐

Tooltip 主显示器工作区底边实际 Popup 翻转与 Toast 五条堆叠/退出期间顺序，本批补入隔离 WPF 行为回归：SDK 8.0.423，6 通过、0 失败、1 个既有系统指针测试跳过。证据和 TRX SHA 见[evidence/U07-03-06](evidence/U07-03-06/BASELINE.md)。本批只改测试与进度证据，生产视图/XAML 未变；实体 IME、跨屏混合 DPI、Playnite 宿主和该批 GitHub CI 尚待验。

## 2026-10-29 U07-04 / U07-05 独立补齐

列菜单检查稳定顺序、必需列禁用说明、UIA名称及ItemsPresenter循环键盘导航，定向5/5；通知收件箱验证新通知插入不抢搜索焦点、不替换当前选中项，定向4/4。失败的程序化Popup焦点探针保留并说明其不能代表实体按键。通知来源字段不存在于当前 DTO，未伪造，U07-05继续进行中。证据见[evidence/U07-04-05](evidence/U07-04-05/BASELINE.md)；真实键盘、数据层刷新及Playnite宿主待验。

## 2026-10-29 U07-05 / U08-07 焦点路径补齐

通知页已有“清除筛选”生产处理器会聚焦搜索框，但旧回归只断言筛选值。本批把测试夹具置于实际无结果空态，在已显示窗口中先聚焦清除按钮，再触发清除并核实键盘焦点回到搜索框；同时保留U07-05的新通知前插后搜索焦点和旧选择。SDK 8.0.423 Release 通知视图类4/4、0 skip；首次失败是夹具隐藏了空态按钮，诊断与修正通过的TRX分别见[evidence/U07-05-08](evidence/U07-05-08/BASELINE.md)。源码校验通过，WPF静态审查42 XAML/0错误/26既有警告/179 info。仅改行为测试，无生产XAML或资源改动；引用此前U08-07-08双主题394图/0 PROBLEM，不宣称本批新渲染或真实宿主结果。`7304a58e`的run `40726534942`（baseline job `122951221975`、SDK10 job `122951222525`）查询时仍运行且无artifact；本批提交的新run待查询。实际Playnite跳转、DB/Worker通知刷新、实体键盘/UIA仍待验。

## 2026-10-29 U09-05 完整链诊断续跑

第一次完整中文入口在沙箱中已完成编译、测试和打包，但安装阶段无法写入用户 Playnite 扩展目录；授权提升后的重跑通过 SDK/Release 编译及核心测试，随后在第10/154个WPF隔离类遇到系统 `StylusWisp` Popup捕获崩溃，未进入打包安装。为避免键盘/UIA测试被不相关的主机 stylus/touch 捕获中断，在仅测试类的 STA 辅助器中设置 WPF AppContext 开关；生产程序集、WPF资源和触控设置未变。正式 `scripts/build.ps1 -SkipTests`：SDK8.0.423、Release、0 warnings/0 errors、XAML26/26；该隔离类5/5、0失败、0 skip。首次直接 `dotnet test` 的源码身份元数据缺失属于不完整调用，按正式构建脚本补齐身份后重跑通过。C盘系统临时目录不足时未清理外部数据，改将进程 `TEMP/TMP` 定向到本阶段 D盘临时目录，空间预检通过。证据含原崩溃/TRX、修复后TRX、构建日志和空间快照，见[evidence/U09-05-wpf-stylus-fix](evidence/U09-05-wpf-stylus-fix/BASELINE.md)。本地完整中文入口仍未成功，独立 `scripts/build.cmd` 与当前提交双SDK CI仍待验；本项继续进行中。

## 2026-10-28 U09 当前批次

已提交并推送 RenderHarness 固定样本时钟 `8deb17c75ac91d716ec38b05f6f0a54530df2217`。U09-01 至 U09-04 首批验证已记录：同一 UTC 假数据时间的两次完整双主题 Render QA 样图逐像素一致；精确提交键盘/UIA 定向16/16、0 skip；双主题×11尺寸394 PNG、0 PROBLEM；同条件操作 P95 较 U00-06 下降，但 U00 没有逐帧或分配字节数据，单次侧栏收起的离屏帧间隔代理约90ms，需真实宿主复核。对应报告、TRX、像素哈希和限制见下方 U09-01～04 行。U09-05 标准入口完整链、U09-07 清理、U09-08 总审尚未完成。U09-06 中，2b6c4022 两个 CI job 已通过；8deb17c7 的 SDK10 job 已通过、baseline job仍运行；required-checks 接口403，状态远端待验。

2026-10-28 U04-06/08本地代码与行为批已完成：共享DisclosureCard在动画中途禁用后清除旧时钟并维持业务展开态，恢复不回放旧动画；双主题回归和100次开合生命周期通过。源码提交c5c86558b6232995438657c00810aeb327ed9ba5已推送，SDK8.0.423精确提交测试13/13、0 skip；source validation、XAML26/26、WPF静态37 XAML/0错误及Render QA 394 PNG/0 PROBLEM通过。细节、哈希和边界见evidence/U04-06-08/BASELINE.md。当前run 40583173450 baseline job 122467678548与SDK10 job 122467678982在编译测试，尚无终态/artifact；CanExecute取消、真实窗口失活及Playnite宿主未由本地用例直接驱动。清理命令被执行策略拒绝，没有改用其它删除命令，记录见evidence/U04-06-08/cleanup.json。下一批推进U05页面/详情过渡去重，不等待旧S/T宿主/权限状态。

任务源：[80项计划与实施提示词](../../UI_POLISH_ROUND5_80_TASKS_2026-10-27.md)。U00-01 已完成本地离屏基线盘点并推送；`ac18a918` 与SDK修正提交 `3b97af01` 的对应run均失败。SDK10已成功安装8.0.423，第二个run两job都在媒体比较控件宽度断言失败；两次run的job/artifact身份和诊断见[40453690050](evidence/U00-01/ci-run-40453690050.json)、[40456945529](evidence/U00-01/ci-run-40456945529.json)。现以固定640/500 DIP验证该生产断点，本机SDK8精确测试1/1通过；候选远端CI待验。阶段自有 `.tmp` 清理因执行策略拒绝而待执行。其余任务仍按依赖准入，排期不等于完成。

截至2026-10-28，U01-07/08测试提交`11c1411e`和U02-01～04实现提交`504562a7f0dad88454dde57b290b7862f91c93e6`均已推送。U02源码批SDK8定向测试15/15、0 skip，源码/XAML/WPF静态检查及双主题Render QA通过；但run `40541712897`基线job `122321530415`与SDK10.0.401 job `122321530884`均失败于旧禁用态断言，纯文档run `40543778584`的job `122328805842`和`122328804650`重复该失败。随后修复测试提交`bf7540e29a1852552784f4d2ad62c5dcd707cc60`的run `40545849867`两个job也失败于3个过时测试假设；原始诊断及artifact身份分别见[run 40541712897](evidence/U02-01-04/ci-run-40541712897.json)、[run 40543778584](evidence/U02-01-04/ci-run-40543778584.json)。当前源码候选已在`417bed510806f965122554beb4419082443b905e`与扩展修正`f59fe4c86c78cb2fc11414a431aceb49821910ee`提交并推送；SDK8.0.423精确SHA聚焦测试165通过、39个既有skip、0失败，source/XAML/WPF静态审查与双主题Render QA通过。对应run `40554157661`（job `122365585853`/`122365586753`）和最终候选run `40555566776`（job `122370590922`/`122370591760`）目前仍在编译测试，结果与artifact待取。required-checks接口403，仍标“远端待验”。当前Render QA审计与最终TRX保留；SDK8.0.423安装在`.tmp/round5-u02-sdk8.0.423`供后续根链使用；较早U01清理受执行策略拒绝，未换shell。

2026-10-28 U02-07/08 去重后确认生产资源与 R09 行为已覆盖阴影层级、低透明降级、动态主题切换、高对比模式及宿主跟随；本批未改产品代码。SDK8.0.423专项9/9、0 skip、0失败；受控90秒探针82轮/351动作/0失败，总动作P95 714.46 ms，对照U00代理基线933.16 ms，但不据此声称Playnite/呈现帧性能无回退。详情与TRX、报告SHA见[U02-07/08证据](evidence/U02-07-08/BASELINE.md)。两项保持“代码完成待验收”，等待真实宿主与本轮远端门禁。CI最近快照与artifact身份见[U02-05/06 CI清单](evidence/U02-05-06/ci-runs.json)：run `40554157661` 的SDK10 job `122365586753`已成功、artifact `12758905811`，baseline已过Render QA/WPF静态审查并在生成扩展包；`40555566776` baseline job `122370590922`在Render QA、SDK10 job `122370591760`在编译/测试；文档run `40557567716`两job也在运行。接下来进入 U03 按钮共享样式小批，不等待上述已有宿主待验项。

2026-10-28 U03-01～03 去重确认共享按钮模板已覆盖主/次/危险/图标按钮五态、稳定忙碌覆盖、34/36 DIP图标目标及窄空间换行。本批无生产资源改动；新增生产XAML图标动作 UIA 名称/命令和共享键盘焦点的源码回归检查。精确提交`8c927a93cff92b923a8efbedbdf28aadbc10d00b` SDK8.0.423 Release 定向13/13、0 skip；五态双主题10图、忙碌与命中区探针和两份失败诊断归档见[evidence/U03-01-03](evidence/U03-01-03/BASELINE.md)。截至19:21 UTC，run `40561836233` SDK10.0.401 job `122392618705`成功/artifact `12762447495`，baseline job `122392618285`仍生成扩展包；更完整快照见[CI清单](evidence/U02-05-06/ci-runs.json)。U03-01～03保留“代码完成待验收”，真实 Playnite/UIA、Narrator、物理鼠标与实体DPI未验；进入U03-04/05，不等待这些边界。

2026-10-28 U03-04/05 提交`d1fc1308317fb805d61d67a7dda77d71e0afff18`。共享TextBox禁用态改为语义文字/填色/描边，验证错误边框优先并把错误文本放入Automation HelpText；搜索的清除入口与计数布局补源码回归约束，未改变IME、Esc、命令或业务语义。SDK8.0.423精确提交隔离定向32/32、0 skip，source validation、XAML26/26、WPF静态37 XAML/0错误、RenderHarness编译0警告/0错误及双主题394图Render QA exit0/0 PROBLEM均通过；混合进程失败及后续隔离通过的完整TRX保留于[evidence/U03-04-05](evidence/U03-04-05/BASELINE.md)。run `40566121765` baseline/SDK10 job `122407721274`/`122407720730`仍在编译测试，当前不能记CI通过；真实Playnite/UIA客户端/实体DPI未验。

2026-10-28 U03-06/07 复用现有普通ComboBox与GscCheckBox模板和R05/Q10行为覆盖，仅在提交`5300a2b5c70f7736f1f46617667de73749b8d883`增加长选中项省略/完整提示、选中内容与箭头列分开的源码守护断言。SDK8.0.423精确提交13/13、0 skip；ComboBox边缘翻转/滚动2/2，CheckBox双主题长标签命中、Space和禁用4/4。未修改当前游戏选框、DropdownClosed提交语义、滚动条或生产XAML。run `40567683673`的baseline job `122413211080`与SDK10 job `122413210438`仍在编译测试；任务保持“代码完成待验收”。证据见[evidence/U03-06-07](evidence/U03-06-07/BASELINE.md)。

2026-10-28 U03-08/U04-01/02 去重与实现。U03-08确认数值范围/溢出/滚轮边界、路径预览/类型验证/缺失目录说明和本地打开保护已有覆盖，无生产改动。U04-01将生产共享交互的短hover、按压与DisclosureCard转场映射到共享动效令牌，2秒不确定进度扫动保留并说明原因；U04-02在Light/Dark合成IsMouseOver下覆盖8个按钮×3轮快速扫过，清除原生/合成探针后验证零残留与可重新进入。源码提交`631b218e`；同STA整合提交`95b5dbf9`；双主题测试补齐提交`0c7c4268`。SDK8.0.423精确SHA聚焦21/21与2/2、0 skip；source validation、XAML26/26、WPF静态37个XAML/0 errors、RenderHarness 0 warning/0 error、双主题Render QA 394 PNG/0 PROBLEM通过，报告及TRX哈希见[evidence/U03-08-U04-01-02](evidence/U03-08-U04-01-02/BASELINE.md)。run `40570767550`失败于跨STA Application访问；其后run `40571484630`与`40572542627`两SDK均失败于DisclosureChevron仍硬编码150ms的过时断言，原始job/artifact诊断已复核并列入`ci-runs.json`。该断言已按共享`GscMotionFast`更新，测试修复与新选择指示一并在`4225eb17`推送。run `40575315327`（文档SHA `06d4e854`）与精确代码run `40578426793`（SHA `4225eb17`）的baseline/SDK10双job正在运行；required-checks API曾返回403，当前保护状态不推断。真实Playnite、UIA客户端、物理鼠标、IME与实体DPI未验；保留游戏选框、滚动条、命令/Binding和业务/恢复安全语义。Render QA临时输出与清理记录见U04-03/04/05/07证据。

2026-10-28 U04-03/04/05/07依赖批。U04-03/05复用R08、Q06、R09及GamePicker键盘回归，不重复生产逻辑；修正R08/Q14测试夹具跨STA全局Application依赖。U04-04为共享工作区页签增加3×16 DIP选中提示条，渐入退场跟随`GscMotionNormal`且反向切换互相移除故事板；焦点环/选中底色/内容布局保持分离。U04-07关闭动效时直接设终态并移除时钟。DisclosureChevron旧150ms断言同步更新到共享120ms fast令牌。源码/测试提交`4225eb17139a844ede87219f02a9ad97e88a12de`已推送；SDK8.0.423组合行为15/15、0 skip，令牌断言1/1，source validation、XAML26/26、WPF静态37/0 errors、RenderHarness 0/0、双主题Render QA 394 PNG/0 PROBLEM通过。详细TRX、报告SHA、STA诊断、未验边界与磁盘空间见[U04批次证据](evidence/U04-03-05/BASELINE.md)及`evidence/U04-03-05/cleanup.json`。run `40578426793`仍待两SDK结论；真实Playnite、实际UIA、IME/物理输入、实体DPI待验。下一批推进U04-06与U04-08，不等待真实宿主，不触碰真实存档/媒体删除/云写入。

状态：待开始、实施中、代码完成待验收、已验收、已满足、外部阻塞、不适用。先注明旧能力/本轮增量；视觉行为、本地构建、远端CI、清理分别留证，真实宿主与受控验证分开。

| ID | 任务 | 状态 | 旧能力/本轮增量 | 实现提交与视觉/行为证据 | 构建/CI/清理及下一步 |
| --- | --- | --- | --- | --- | --- |
| U00-01 | 视觉基线盘点 | 代码完成待验收 | 继承现有生产主题、RenderHarness 与历史证据；本项新增存档/任务/媒体/设置 Light/Dark、1040/1366 DIP 同条件基线、逐页候选差异及旧任务去重 | [基线与逐页观察](evidence/U00-01/BASELINE.md)，16张截图SHA-256在同目录`screenshots.json`；提交`ac18a918`，产品代码身份`90bf0405` | SDK8.0.423；RenderHarness Release 0/0，render-qa exit 0 / 0 PROBLEM；WPF静态审查31 XAML / 0 error / 26 warning / 179 info。run `40453690050`与SDK修正提交run `40456945529`仍为失败历史；后者确认SDK8.0.423已装，两job重复失败于媒体比较测试的控件宽度假设。候选用固定640/500 DIP修正夹具并本机1/1通过，等新提交CI；原始job/artifact身份见两个CI JSON。清理命令遭执行策略拒绝，395个本阶段输出/53,094,868字节仍在`.tmp`；见`cleanup.json`。当前示范批U00-02/U00-03同步进行；真实宿主/有效默认设置截图仍待验。 |
| U00-02 | 控件资源归属 | 代码完成待验收 | SaveCenterView原先重复合并WpfUiProduction（Redesign已合并）；页面TabControl别名样式重复实例已声明的Stretch属性。移除重复项，直接应用共享模板，不动宿主资源 | [资源来源、变更与验证](evidence/U00-02-03/BASELINE.md)；双主题/双尺寸截图及SHA见[evidence](evidence/U00-02-03/screenshots.json)；源码提交58872f2c | source validation通过；SDK8.0.423 RenderHarness Release 0/0、exit0、0 PROBLEM；WPF静态审查31 XAML / 0 error / 26 warning / 179 info。20be8073 run 40462741496两job败于媒体收件箱测试夹具（runner实际1014 DIP而断言按请求1360）；修正为夹具布局1360 DIP后SDK8定向类6/6。a83c008a run 40480521410两SDK发现R23隔离测试缺少Playnite宿主BaseTextBlockStyle，已拆出独立Application生命周期类，本机SDK8新类1/1、原R23类8/8。5d0a2448 run 40486014630的SDK8 job 122127864273与SDK10 job 122127864513又败于保存页宽屏测试夹具；已固定页面逻辑宽并保留单行/间距断言，本机SDK8整类54/54、0 skip，待新提交双SDK复核。原job/artifact和修复TRX见[CI记录](evidence/U00-02-03/ci-run-40486014630.json)。真实宿主/UIA/物理DPI仍待验。
| U00-03 | 信息主次示范 | 代码完成待验收 | 重复标题改为“存档清单”；主要扫描/预览/校验/详情操作前置，保留筛选、配置、表格、详情、命令与安全语义 | [信息层级和前后证据](evidence/U00-02-03/BASELINE.md)；1040/1366 DIP Light/Dark截图见[evidence](evidence/U00-02-03/screenshots.json)；源码提交`58872f2c` | SDK8.0.423定向测试3 passed / 4既有skip；媒体比较受控宽度1/1 passed；标题层级回归修正后测试1/1 passed。run `40461606378`的标题断言旧失败已修，`20be8073` run `40462741496`两job通过Core 157/157、Worker 400/400，但各在Playnite 1233项中1项失败、18既有skip；失败为无关的媒体收件箱宽屏夹具，现受控1360 DIP并本机SDK8类6/6通过，修复提交待CI。真实宿主、UIA/IME、物理DPI和完整根链待验。
| U00-04 | 间距节奏示范 | 代码完成待验收 | 存档摘要横向节奏保留；CI 暴露纵向留白使表格少14 DIP 后，恢复紧凑值（Padding 12,4；卡片底距2；动作顶距10），命令、内容和顺序不变 | [前后观察与验证](evidence/U00-04-05/BASELINE.md)；双主题/尺寸截图及SHA见[evidence](evidence/U00-04-05/screenshots.json)；回归修正见[evidence/U01-01-02/BASELINE.md)；生产XAML提交`7e61bba3`，紧凑断言修复`bb984a07` | SDK8 RenderHarness Release 0 warning/0 error；宽屏夹具整类54/54；R07紧凑布局4/4、0 skip；render-qa exit0/0 PROBLEM。run `40505231943`旧间距断言失败后，`bb984a07`把断言更新为当前值并本机1/1通过；新提交run `40510427122` SDK8/SDK10双job、render-qa、静态审查、打包均通过。最终本地根链和真实宿主仍待验。
| U00-05 | 状态样张集 | 代码完成待验收 | SaveHistory加入内存隔离的Ready/Empty/Loading/Error/Stale/Offline及长文本多选；无数据库写入或真实命令 | [状态夹具说明](evidence/U00-04-05/BASELINE.md)；双主题×4尺寸×7样张共56截图，SHA及代表图见[evidence](evidence/U00-04-05/screenshots.json)；本批源码待CI | statefixtures OK；保留旧数据的Stale与首载空表状态断言通过；相关行为3/3、0 skip。真实UIA/键盘/IME、Playnite宿主及最终根链待验。
| U00-06 | 改动前性能基线 | 代码完成待验收 | 既有耐久探针已有切页/详情综合耗时；本项新增可重复的切页、详情开合、滚动与合成搜索输入分类统计 | [方法、样本和限制](evidence/U00-06-07/BASELINE.md)；原始报告及SHA见[报告](evidence/U00-06-07/enduranceprobe-report.txt) | SDK8 Release编译0/0；60秒测量+30秒稳定观察，68周期/290步/10资源样本/0失败。切页、详情、滚动、输入均有中位数/P95/最大值；仅逻辑WPF代理，非呈现帧/物理键盘延迟。临时副本清理被执行策略拒绝，见[记录](evidence/U00-06-07/cleanup.json)；新提交CI与最终全链待验。
| U00-07 | 视觉差异分级 | 代码完成待验收 | 继承U00-01四页截图与几何候选；任务详情入口已并入任务主操作行；媒体收件箱批量动作从两行收为一行，低频清空选择/重置列宽移入已有可滚动页脚，保留模式选择、当前游戏、批量归类和详情入口 | [差异级别和后续目标](evidence/U00-06-07/BASELINE.md)；[任务页 P1 前后证据](evidence/U00-07-U06-04-task-toolbar-20261030/BASELINE.md)；[媒体收件箱 P1 前后证据](evidence/U00-07-U06-04-media-inbox-20261030/BASELINE.md)；来源及不确定性见[U00-01观察](evidence/U00-01/BASELINE.md) | 媒体壳层1040×700工具栏由两行减为一行，DataGrid上移76 DIP并保持四个完整数据行；次要动作滚动后可达。设置有效默认态仍待证；真实宿主待验。横向滚动本身不判作不可用。|
| U00-08 | 示范页整体复核 | 代码完成待验收 | 将存档页字体层级、动态主题色、卡片间距、生产控件及六类状态组合复核；未迁移共享模板 | [双主题、窗口及未验边界](evidence/U00-08/DEMO_REVIEW.md)；样张索引见[evidence](evidence/U00-04-05/screenshots.json)；本批实现提交待后续账本登记 | Light/Dark、1040/1366与额外逻辑尺寸有对照；Demo源缺失，未声称与Demo一致；真实Playnite/UIA/IME/物理DPI、设置有效默认态、中文根链及独立build.cmd仍待验。 |
| U01-01 | 混排基线校准 | 代码完成待验收 | 生产按钮与DataGrid样本实测中英/数字GlyphRun；按钮12 DIP SemiBold、表格14 DIP Normal，baseline spread均为0；无固定偏移、未捆绑字体 | [实现、数据和边界](evidence/U01-01-02/BASELINE.md)；浅深探针截图及SHA见[evidence](evidence/U01-01-02/screenshots.json)；实现提交`7e61bba3`，相关断言修复`bb984a07` | SDK8.0.423；Typography/生产标题/R07定向测试37 passed、0 failed、0 skipped；finesse双主题、source/XAML/WPF及render-qa通过。`40505231943`的旧间距字面量失败由`bb984a07`修复；[run `40510427122`](evidence/U01-03-04/ci-run-40510427122.json) SDK8/SDK10及render-qa/静态审查/打包全通过。可选字体未安装、宿主/物理DPI待验；本阶段`.tmp`清理命令被策略拒绝且未重试，见[cleanup](evidence/U01-01-02/cleanup.json)。
| U01-02 | 标题层级收敛 | 代码完成待验收 | Shell页标题改用GscPageTitleStyle；媒体、通知、任务、工具页的同级分组标题统一引用GscSectionTitleStyle，保留紧凑表格和页面语义 | [实现、前后观察和回归修复](evidence/U01-01-02/BASELINE.md)；页面双主题截图和SHA见[截图索引](evidence/U01-01-02/screenshots.json)；生产样式提交`7e61bba3`，测试修复`bb984a07` | SDK8.0.423 RenderHarness Release 0 warning/0 error；生产标题及相关行为37/37定向通过，0 skip；render-qa双主题多尺寸通过。新提交`bb984a07`的run `40510427122`双SDK、渲染、WPF静态、打包均通过。真实Playnite、UIA/IME、物理DPI与最终本地根链待验。
| U01-03 | 数字与单位节奏 | 已满足 | 复用既有Tabular/右对齐数字、固定文件数/容量/时间列宽、1024进制ByteSizeFormatter；无需再造格式化或列宽体系 | [去重结论和样式证据](evidence/U01-03-04/BASELINE.md)；当前页面截图及SHA见[截图索引](evidence/U01-01-02/screenshots.json)；两份定向TRX和SHA见[evidence/U01-03-04/tests-manifest.json]；无生产源码改动 | SDK8.0.423容量13/13、时间/字体/路径/剪贴板51/51，0 skip；SHA `bb984a07` 的GitHub run `40510427122`双SDK通过。局部样式、容量边界和固定列锚点有源码/行为证据；剩余真实宿主/物理DPI属于整轮边界。
| U01-04 | 长路径呈现 | 已满足 | 复用72字符保盘符/文件名的路径预览；原始路径绑定、可选详情、明确复制按钮和Ctrl+C/Ctrl+Shift+C精确值，不只靠Tooltip | [去重结论、预览和键盘复制证据](evidence/U01-03-04/BASELINE.md)；双主题混排探针图见[截图索引](evidence/U01-01-02/screenshots.json)；相关TRX和SHA见[evidence/U01-03-04/tests-manifest.json]；无生产源码改动 | SDK8.0.423长路径/完整文本选择/剪贴板相关用例包含于51/51通过、0 skip；SHA `bb984a07` 的GitHub run `40510427122`双SDK通过。Tooltip为补充，键盘和只读可选完整路径可直接到达。
| U01-05 | 辅助文字可读性 | 代码完成待验收 | Caption原有12 DIP/不降整体Opacity和动态Muted Brush；共享WPF-UI按钮普通禁用态仍将Chrome设为0.72，且只在高对比态使用Disabled Brush。本批统一普通/高对比语义前景、控制底色和描边，DisabledText按最小4.5:1算法更新；未改命令、绑定和禁用语义 | [去重、改动与边界](evidence/U01-05-06/BASELINE.md)；[26项SDK8行为TRX](evidence/U01-05-06/tests/U01-05-06-focused-final.trx)；[render-qa报告](evidence/U01-05-06/render-qa-report.txt)；实现提交[`813adb09`](evidence/U01-05-06/BASELINE.md) | SDK8.0.423：26/26、0 skip；source validation通过；WPF静态31 XAML/0 error/26 warning/179 info；RenderHarness Release 0/0、0 PROBLEM。提交已推送；GitHub push-run/job无法由当前权限读取，状态“远端待验”，所用工具限制与下一步见[verification.json](evidence/U01-05-06/verification.json)。真实Playnite/UIA与物理DPI待验；根入口完整链和独立`scripts/build.cmd`留待整轮最终候选。394张PNG临时图/398个文件系统条目清理遭执行策略拒绝，见[cleanup.json](evidence/U01-05-06/cleanup.json) |
| U01-06 | 图标光学对齐 | 已满足 | 复用GscActionIcon 16 DIP、GscLineIcon 24 DIP与既有光学间距/语义实现；125/150/175/200%边缘和不挤压文字已有探针，本批不另造图标/占位系统 | [去重与证据说明](evidence/U01-05-06/BASELINE.md)；R02光学对齐、图标语义及Q24 DPI用例包含在[同批TRX](evidence/U01-05-06/tests/U01-05-06-focused-final.trx)；无产品源码改动 | SDK8.0.423组合回归26/26、0 skip；WPF静态与双主题render-qa通过。真实宿主/物理DPI仍属整轮待验边界；U01-05实现提交CI状态一并跟踪 |
| U01-07 | 中英文文案长度 | 代码完成待验收 | 复用生产按钮完整换行/无省略模板；长中英文标签、长路径、错误详情、快捷键复制及只读技术文本选择已有行为覆盖，本轮确认无须新增产品控件 | [去重与测试证据](evidence/U01-07-08/BASELINE.md)；21项组合TRX和SHA见[manifest](evidence/U01-07-08/tests/tests-manifest.json)；测试提交`11c1411e` | SDK8.0.423定向7类21/21、0 skip；本地证据通过，但对应提交GitHub Actions远端待验，不能提前结项。 |
| U01-08 | 文本缩放与选择 | 代码完成待验收 | 加强现有字号夹具至生产Body 14→28、Caption/按钮12→24 DIP；覆盖输入、按钮、表头、可滚动技术详情及SelectAll全文，无Viewbox整体缩小；生产资源未改 | [去重、夹具改动与边界](evidence/U01-07-08/BASELINE.md)；专项2项及组合21项TRX见[manifest](evidence/U01-07-08/tests/tests-manifest.json)；测试提交`11c1411e` | SDK8.0.423专项2/2及组合21/21、0 skip；字号校准失败完整保留。真实Playnite、UIA/键盘/IME和物理DPI待验；此提交GitHub Actions远端待验；整轮中文根入口和独立`scripts/build.cmd`留待最终候选。 |
| U02-01 | 浅色层次校准 | 代码完成待验收 | U00-01生产主题/视图及`AdaptiveThemePaletteContrastGuard`已有页面-卡片、卡片-控件分层和边界阈值；本批Light模式复核达标，无证据要求重调表面色 | [去重、检查和边界](evidence/U02-01-04/BASELINE.md)；语义字色守卫扩至浅色主题；提交`504562a7` | SDK8定向15/15、0 skip；Render QA exit 0、0 PROBLEM。run `40541712897`、文档run `40543778584`及测试修复run `40545849867`均因3个过时Q06/调色板断言失败；下一修正候选`f59fe4c8`的run `40555566776`双job运行中。真实宿主/物理DPI待验；required-checks API 403。 |
| U02-02 | 深色层次校准 | 代码完成待验收 | 复用U00-01深色基线与自适应表面量测；本批Dark模式复核现有分层和状态文本，无证据要求重调表面色 | [去重、检查和边界](evidence/U02-01-04/BASELINE.md)；新增状态文字/浅色语义底对比计算；提交`504562a7` | SDK8定向15/15、0 skip；Render QA 1040×700至3840×2160逻辑窗口、Dark主题及侧栏转场通过。`40545849867`双job失败的测试断言已在当前共享XAML验证批修正；最终候选run `40555566776`双SDK待完成。真实Playnite和物理DPI待验。 |
| U02-03 | 强调色使用配额 | 代码完成待验收 | 概览最近访问普通行图标与取消状态图标原用强调色底；本批改用中性控制底/次级色，仅选中框继续用强调色 | [实现与渲染说明](evidence/U02-01-04/BASELINE.md)；代表截图与SHA见[evidence](evidence/U02-01-04/verification.json)；提交`504562a7` | SDK8定向15/15、0 skip；R10键盘可达行为覆盖；Render QA exit 0。精确测试修复run `40545849867`两job失败于旧断言；最终候选run `40555566776`双SDK待验；真实宿主/UIA待验。 |
| U02-04 | 语义状态色 | 代码完成待验收 | 原概览状态不全：成功沿用历史图标、失败/运行/取消使用强调底；本批完成成功/警告/错误/进行中/排队/取消六态图标、前景、浅底与标签语义一致，并将普通状态文字对比守卫提高到4.5:1；高对比用系统色资源 | [状态表、对比和边界](evidence/U02-01-04/BASELINE.md)；[15项SDK8 TRX、报告和SHA](evidence/U02-01-04/verification.json)；源码提交`504562a7` | SDK8 Release 15/15、0 skip；双主题WPF行为检查、Source Validation、XAML 25/25、静态审查31 XAML/0 error、Render QA exit 0 / 0 PROBLEM。测试修复run `40545849867`失败于3个旧测试假设；当前资源/测试候选`f59fe4c8`的run `40555566776`双job待完成。真实高对比宿主、UIA/键盘/IME、物理DPI待验；完整根入口与独立`scripts/build.cmd`留待整轮最终门禁。 |
| U02-05 | 边框与分隔线 | 代码完成待验收 | 将生产资源与页面散落的1 DIP边框统一成共享 hairline；分隔线统一复用样式；全项目 XAML 静态断言防止新字面量回流 | [去重、实现与验证边界](evidence/U02-05-06/BASELINE.md)；[334张截图SHA索引](evidence/U02-05-06/screenshots.json)；提交`417bed51`、扩展提交`f59fe4c8` | SDK8.0.423精确SHA聚焦组165通过/39既有skip/0失败；Source Validation通过、XAML 26/26、静态审查37 XAML/0 error、Render QA exit0/0 PROBLEM。`417bed51` run `40554157661`和`f59fe4c8` run `40555566776`的baseline/SDK10 job均成功；preview/诊断artifact身份见[CI清单](evidence/U02-05-06/ci-runs.json)。实际Playnite宿主、UIA、实体DPI待验；整轮根入口/build.cmd留到最终门禁。 |
| U02-06 | 圆角与裁切 | 代码完成待验收 | 新增按实际尺寸、CornerRadius 与消费者原Clip裁切的共享行为；按层级复用圆角，覆盖卡片、表格、壳层、按钮、状态/空态面板及现有游戏选框样式，不引入负Margin | [去重、实现与验证边界](evidence/U02-05-06/BASELINE.md)；[334张截图SHA索引](evidence/U02-05-06/screenshots.json)；提交`417bed51`、扩展提交`f59fe4c8` | 圆角像素行为在100/125/150/175/200%缩放探针通过；SDK8精确SHA聚焦组165通过/39既有skip/0失败；双主题 Render QA 334 PNG、0 PROBLEM。两个对应源码run均双SDK成功。离屏验证不代表真实Playnite、UIA或物理DPI；当前保持代码完成待验收。 |
| U02-07 | 阴影与透明预算 | 代码完成待验收 | 共享资源已有有限阴影层级、关闭透明降级和无列表行Blur；本轮去重并复核，没有发现需改生产样式的缺口 | [去重、行为与性能证据](evidence/U02-07-08/BASELINE.md)；[专项TRX](evidence/U02-07-08/tests/U02-07-08-r09-focused.trx)；[受控探针](evidence/U02-07-08/performance/enduranceprobe-report.txt)；代码基线`f59fe4c8` | SDK8.0.423定向9/9、0 skip；90秒受控WPF探针82轮/351动作/0失败，总动作P95 714.46 ms（U00代理基线933.16 ms）。run `40560233057`的SDK10 job `122386948131`成功/artifact `12762575915`，baseline `122386947995`仍在生成扩展包。趋势仅作代理观察，不代表呈现帧或真实宿主；required-checks权限此前403，真实Playnite/UIA/实体DPI待验。 |
| U02-08 | 主题切换连续性 | 代码完成待验收 | 既有DynamicResource切换在下一次Render前更新弹层Brush/图标/占位符；高对比/宿主跟随/效果降级与恢复已有R09覆盖，本轮复核未发现需改生产资源的缺口 | [去重、行为与性能证据](evidence/U02-07-08/BASELINE.md)；[专项TRX](evidence/U02-07-08/tests/U02-07-08-r09-focused.trx)；代码基线`f59fe4c8` | SDK8.0.423主题/高对比/背景组9/9、0 skip；继承双主题 Render QA。run `40560233057` SDK10成功/artifact `12762575915`，baseline仍在生成包；远端门禁未完整。真实Playnite主题切换、UIA及实体DPI待验。 |
| U03-01 | 按钮五态一致 | 代码完成待验收 | 生产主/次/危险/图标按钮共享Normal/Hover/Pressed/Disabled/Focus态且不改尺寸；本轮复核并留存双主题五态图，无生产样式增量 | [去重与证据](evidence/U03-01-03/BASELINE.md)；10图及SHA见[screenshots.json](evidence/U03-01-03/screenshots.json)；测试提交`8c927a93` | SDK8.0.423精确SHA定向13/13、0 skip；run `40561836233` SDK10 job `122392618705`成功/artifact `12762447495`，baseline job `122392618285`仍在生成扩展包。真实Playnite/鼠标/UIA待验。 |
| U03-02 | 忙碌按钮稳定 | 代码完成待验收 | 沿用共享延迟忙碌层，保留内容/宽度/焦点并阻止重复执行；失败/取消后恢复CanExecute已有行为覆盖，本轮精确复验 | [去重与证据](evidence/U03-01-03/BASELINE.md)；TRX与SHA见[screenshots.json](evidence/U03-01-03/screenshots.json)；测试提交`8c927a93` | SDK8.0.423精确SHA定向13/13、0 skip；五态探针与R02忙碌行为通过。run `40561836233` SDK10成功、baseline仍打包；真实宿主仍待验。 |
| U03-03 | 图标按钮命中区 | 代码完成待验收 | 主样式34×34、工具栏36×36 DIP并保留6 DIP间距，窄宽换行；新增生产XAML图标操作UIA名称+Command/Click+共享键盘焦点源码断言 | [去重、回归与边界](evidence/U03-01-03/BASELINE.md)；按钮矩阵及SHA见[screenshots.json](evidence/U03-01-03/screenshots.json)；测试提交`8c927a93` | SDK8.0.423精确SHA定向13/13、0 skip，图标边界与VisualTree命中检查通过。物理鼠标/实际UIA客户端未测；run `40561836233` SDK10成功，baseline仍打包。 |
| U03-04 | 输入框复合状态 | 代码完成待验收 | 共享禁用态改用语义文字/填色/描边；验证错误边框最高优先级，错误内容加入Automation HelpText；当前无TextBox忙碌业务命令，未新造状态 | [状态、测试、Render QA及未验边界](evidence/U03-04-05/BASELINE.md)；双主题截图及SHA见`evidence/U03-04-05/render/`；提交`d1fc1308` | SDK8精确SHA 32/32、0 skip；Source Validation通过、XAML26/26、静态37 XAML/0 error、Render QA 394 PNG / 0 PROBLEM。run `40566121765` baseline `122407721274`与SDK10 `122407720730`仍编译测试。真实Playnite/实际UIA/物理DPI待验。 |
| U03-05 | 搜索与清除入口 | 代码完成待验收 | 搜索输入、清除按钮、结果数量保留独立布局；现有IME确认与页面Escape契约未改，新增生产布局源码检查 | [状态、测试、Render QA及未验边界](evidence/U03-04-05/BASELINE.md)；提交`d1fc1308` | SDK8精确SHA相关隔离32/32、0 skip；任务/媒体搜索摘要独立、clear区域未进入编辑文本布局；命令目录/游戏选择器IME与任务搜索视口测试通过。run `40566121765`两个job仍运行；实体键盘/Playnite待验。 |
| U03-06 | 下拉与选项排版 | 代码完成待验收 | 复用共享ComboBox箭头/选中项20 DIP列、长文省略/提示、Popup限高和方向键导航；本批仅加源码守护测试，不改生产资源 | [去重与验证边界](evidence/U03-06-07/BASELINE.md)；TRX/SHA见`evidence/U03-06-07/tests-manifest.json`；提交`5300a2b5` | SDK8精确SHA 13/13、0 skip；通用下拉边界翻转与选中项滚动2/2；source/XAML/WPF静态检查通过。run `40567683673` baseline `122413211080`与SDK10 `122413210438`仍编译测试。当前游戏选框和DropDownClosed契约保持不变。 |
| U03-07 | 勾选与开关状态 | 代码完成待验收 | 复用勾选/不确定/禁用标记、长标签换行和完整命中区；Space、UIA及Light/Dark已有行为覆盖，本批不重复造样式 | [去重与验证边界](evidence/U03-06-07/BASELINE.md)；TRX/SHA见`evidence/U03-06-07/tests-manifest.json`；提交`5300a2b5` | SDK8精确SHA Q10命中/Space/禁用组4/4、0 skip；静态来源与同批WPF行为通过。真实宿主、Narrator、物理鼠标及实体DPI未验；CI run `40567683673`仍运行。 |
| U03-08 | 数值及路径编辑 | 代码完成待验收 | 复用既有范围/溢出/滚轮与路径验证、缺失目录说明和安全打开守卫，本批去重未改生产控件 | [去重与验证证据](evidence/U03-08-U04-01-02/BASELINE.md)；数值/路径精确SHA 21/21、0 skip见`tests-manifest.json` | source/XAML/WPF静态、双主题Render QA均通过；run `40572542627` baseline `122430277192`、SDK10 `122430276522`仍运行。真实宿主及实体输入待验。 |
| U04-01 | 动效语义收敛 | 代码完成待验收 | 共用120ms fast与100ms press时长令牌；DisclosureCard使用fast；不确定进度2s扫动作为持续进度例外保留 | 改动与视觉/行为证据见evidence/U03-08-U04-01-02/BASELINE.md；实现提交631b218e，令牌修正4225eb17 | 精确冻结源5c0c57b0中文根链与独立scripts/build.cmd均exit0：各158 TRX、1962通过/0失败/58既有skip；GitHub run40900374673基线与SDK10 job均success。真机输入/宿主呈现待验。 |
| U04-02 | 快速掠过收尾 | 代码完成待验收 | 8个兄弟按钮×3轮合成hover，清除原生/合成状态后确认动画回零并可重新进入；物理鼠标分开待验 | 双主题行为证据见evidence/U03-08-U04-01-02/BASELINE.md；测试提交95b5dbf9、0c7c4268 | Light/Dark各24次访问，残留opacity<0.001；Render QA 394 PNG无PROBLEM。5c0c57b0中文根链/build.cmd及双SDK CI已通过；Playnite和物理指针待验。 |
| U04-03 | 按压释放接续 | 代码完成待验收 | R08覆盖按压、释放、移出/重入连续性；Q06覆盖禁用/CanExecute与命令安全；本批移除跨STA Application初始化依赖 | 批次证据见evidence/U04-03-05/BASELINE.md；提交4225eb17 | SDK8定向组合15/15、0 skip。Q14旧夹具失败由c5c86558修复；冻结源5c0c57b0中文根链/build.cmd及run40900374673双SDK CI均成功。实体输入和Playnite宿主未验。 |
| U04-04 | 选择指示过渡 | 代码完成待验收 | 共享3×16 DIP提示条保留120ms Opacity过渡；本次移入既有左留白并保留最小测量尺寸，普通/内部页签均避开标题且禁用整体透明度保持 | [新修正与红绿/双主题对照](evidence/U04-indicator-gap-20261029/BASELINE.md)；[原批次历史配对](evidence/U09-01-U04-pairs-20261029/BASELINE.md) | R23类12/12、0 skip；静态0 error、Render QA394 PNG/0 PROBLEM。修正提交388c46ea包含于冻结源5c0c57b0，中文根链/build.cmd与run40900374673双SDK CI均成功；Playnite宿主待验。 |
| U04-05 | 输入反馈时序 | 代码完成待验收 | 焦点、错误校验和搜索清除已有即时状态/焦点行为；复用R09、GamePicker键盘与Q14测试，不增加输入等待 | 去重证据见evidence/U04-03-05/BASELINE.md；无生产输入控件改动；夹具修复提交c5c86558 | SDK8.0.423定向组合15/15、0 skip；Q14夹具修复包含于冻结源5c0c57b0，中文根链/build.cmd及run40900374673双SDK CI均成功。真实IME/Playnite UIA客户端待验。 |
| U04-06 | 中途禁用与取消 | 代码完成待验收 | DisclosureCard在打开过程中禁用时移除旧Storyboard时钟，保留IsExpanded并以绑定恢复合法角度；重新启用不回放旧动画。取消命令与真实窗口失活未由本批直接驱动 | 实现与双主题行为见evidence/U04-06-08/BASELINE.md；源码提交c5c86558；新增历史配对见[evidence](evidence/U09-01-U04-interrupt-20261029/BASELINE.md) | SDK8.0.423精确SHA 13/13、0 skip；source/XAML/WPF静态与Render QA通过。冻结源5c0c57b0中文根链/build.cmd及run40900374673双SDK CI均成功；真实CanExecute取消/窗口失活及Playnite宿主待验。 |
| U04-07 | 降低动态效果 | 代码完成待验收 | MotionEnabledActive关闭时页签选择直接写入终态并移除两方向Storyboards；无缩放、位移或输入等待 | 状态及行为见evidence/U04-03-05/BASELINE.md；提交4225eb17 | 双主题SDK8用例确认终态、无活动时钟；修复已进入冻结源5c0c57b0，中文根链/build.cmd与run40900374673双SDK CI均成功。真实系统动画设置/Playnite宿主待验。 |
| U04-08 | 动画对象生命周期 | 代码完成待验收 | 同一生产DisclosureCard完成100次开合；Header、Chevron Path和RotateTransform身份稳定；Reduced Motion清除最终Storyboard时钟 | 实现与双主题行为见evidence/U04-06-08/BASELINE.md；源码提交c5c86558；新增历史配对见[evidence](evidence/U09-01-U04-interrupt-20261029/BASELINE.md) | SDK8.0.423精确SHA 13/13、0 skip；Render QA 394 PNG/0 PROBLEM。冻结源5c0c57b0中文根链/build.cmd及run40900374673双SDK CI均成功；真实Playnite和长期宿主内存增长待验。 |
| U05-01 | 页面切换方向 | 代码完成待验收 | 真实PageHost只在Content变化后使用Fast时长的轻淡入/上移；快速导航验证最终页、标题、选中项和焦点，不做全页模糊。新增激活缓存页时才重排该页，沿用当前窗口尺寸 | [U05-01/02证据](evidence/U05-01-02/BASELINE.md)；[活动页面重排复测](evidence/U05-01-U06-07-U09-04-active-layout-20261102/BASELINE.md)；实现提交`237e993f9e03541f5e8056c30cab8fdb029c0a77` | SDK8.0.423行为批5/5与本批R08 5/5、均0 skip；source/XAML/WPF静态通过；精确SHA Render QA 394图/0 PROBLEM。新提交CI与真实Playnite仍待验；本批不改变轻淡入动画。 |
| U05-02 | 详情打开关闭 | 代码完成待验收 | 紧凑任务详情开合使用Opacity/Translate，不动画Width；关闭中禁命中、焦点回到打开按钮，可反向/在选择、布局或降低动态效果时安全收尾，保留队列选中锚点和滚动 | [U05-01/02证据](evidence/U05-01-02/BASELINE.md)；实现提交`237e993f9e03541f5e8056c30cab8fdb029c0a77` | SDK8.0.423 open-close-reverse及Reduced Motion通过，0 skip；基线与SDK10精确SHA job均成功；Render QA 394图/0 PROBLEM；run 1339 artifacts已记录。真实宿主待验。 |
| U05-03 | 详情对象切换 | 代码完成待验收 | 复用生产选择变化归一化；补充同一UI线程标题/选择/实际按钮命令目标一致性 | [本批证据](evidence/U05-03-04/BASELINE.md) | 提交18d73836已push；SDK8.0.423精确身份9/9、0 skip；Light/Dark×两宽度；run40638353451双SDK运行中，真实宿主待验 |
| U05-04 | 弹层进入退出 | 代码完成待验收 | 键盘说明弹层接入共享动画/透明度资源，打开进入说明卡，Escape关闭回焦；复用现有对话框Normal与详情Fast状态机 | [补齐证据](evidence/U05-04-help/BASELINE.md)及[退出负例](evidence/U05-03-04/BASELINE.md) | SDK8.0.423组合16/16、0 skip；XAML26/26、静态0错误、双主题Render QA394图/0 PROBLEM；本批CI及真实宿主待验 |
| U05-05 | 加载到内容 | 代码完成待验收 | 共享Loading提示180ms后显示，Hidden保留尺寸，短响应不闪卡；错误立即显示、退出/隐藏/卸载清计时器 | [实现与证据](evidence/U05-05-06/BASELINE.md)；[历史双主题配对](evidence/U05-05-06/historical-render-pair-20261030/run.json) | SDK8.0.423组合13/13、0 skip；source/XAML26/WPF静态0 error；Render QA双主题394图/0 PROBLEM；本批CI和真实宿主待验 |
| U05-06 | 进度与阶段变化 | 代码完成待验收 | 去重保留真实进度/未知/取消/成功语义；新增生产进度单元格跨五态的宽度与右对齐边界稳定检查 | [回归与边界](evidence/U05-05-06/BASELINE.md)；[历史双主题配对](evidence/U05-05-06/historical-render-pair-20261030/run.json) | SDK8组合13/13、0 skip；无生产进度逻辑差异，不伪补100；本批CI及真实宿主待验 |
| U05-07 | 页面离开收尾 | 代码完成待验收 | 换页/卸载关闭说明Popup与命令层、归一化PageHost，任务页卸载撤销详情退出代次与时钟；隐藏进度保留busy语义 | [本批证据](evidence/U05-07-08/BASELINE.md) | SDK8.0.423合计7/7、0 skip；source/XAML26/WPF静态0 error；双主题Render QA394图/0 PROBLEM；新提交CI及真实宿主待验 |
| U05-08 | 连续操作压力 | 代码完成待验收 | 31次实际切页×主题×说明Popup×详情退出中断，最后卸载Shell；检查最终页/焦点、时钟、命中锁及缓存/选中/滚动 | [序列与失败诊断](evidence/U05-07-08/BASELINE.md) | 本地受控7/7、0 skip；Render QA通过；未将合成输入或逻辑DIP当真实Playnite、物理输入/帧率证明；CI待验 |
| U06-01 | 表头与行视觉节奏 | 代码完成待验收 | 复用共享行高/表头/对齐与S01密度，补排序三态文字几何和箭头留位检查 | [证据](evidence/U06-01-02/BASELINE.md) | SDK8.0.423表头/密度/状态初轮16/16；无生产视觉差异，Render QA继承同生产代码U05-07/08；新提交CI及真实宿主待验 |
| U06-02 | 悬停选中焦点叠加 | 代码完成待验收 | 292f3bc6 共享裁切/选框几何修复：Overview时间、Save末列和Task状态/详情均完整落在框内；保留内容锚点、蓝色选择/悬停/焦点与滚动条 | [本批修复与验证边界](evidence/U06R5-2911/BASELINE.md)；[门禁摘要和TRX](evidence/U06R5-2911/gates.json)；[Light/Dark截图](evidence/U06R5-2911/screens/overview-light.png) | R06/R10定向行为用例、精确候选 scripts/build.cmd、双主题Render QA均通过；真实Playnite页/UIA/实体键盘IME/物理DPI仍待验收，根入口当前未完成
| U06-03 | 行内操作显隐 | 代码完成待验收 | 复用常显文字动作与稳定对象参数；双主题焦点/受控悬停、列宽与UIA Invoke目标检查 | [证据](evidence/U06-03-04/BASELINE.md)；[修正候选CI身份](evidence/U06-03-04/ci-run-40647037308.json) | 原39c6ad80/e4285c05失败保留历史；修正提交3ecacdc0的run40647037308中SDK8基线job122678147577与SDK10 job122678147262全步骤success，3个artifact均绑定同SHA；真实宿主/物理输入待验 |
| U06-04 | 工具栏拥挤分级 | 代码完成待验收 | 已核对存档/媒体/维护/概览工具栏既有换行和保留数量；任务详情入口并回主操作行、任务列设置/重置收进既有视图配置展开区；媒体模式、当前游戏目标、批量归类和详情入口保持可见，低频清空/重置收进页脚 | [既有跨页行为](evidence/U06-03-04/BASELINE.md)；[任务页前后证据](evidence/U00-07-U06-04-task-toolbar-20261030/BASELINE.md)；[媒体页前后证据](evidence/U00-07-U06-04-media-inbox-20261030/BASELINE.md) | SDK8定向相关类218通过、0失败、39既有架构skip（不计通过）；post-commit Render QA双主题394图、0 PROBLEM；GitHub run 41041454086 两job均成功，ID与artifact digest见媒体批次 ci.json。真实宿主/UIA/输入/物理DPI仍待验|
| U06-05 | 多选条布局稳定 | 代码完成待验收 | 存档历史非零滚动起点进入/清除40项多选保持锚点，Escape保留选择；双主题断点往返保留焦点和选择 | [证据](evidence/U06-05-06/BASELINE.md) | SDK8.0.423组合17/17、0 skip；生产视觉未改；本批CI和真实宿主待验 |
| U06-06 | 空表与无结果 | 代码完成待验收 | 真实任务页空数据/筛选无结果/错误分别显示；合成清筛选不触发刷新，错误重试调用刷新 | [证据](evidence/U06-05-06/BASELINE.md) | SDK8.0.423组合17/17、0 skip；继承同生产源码Render QA；本批CI及真实宿主待验 |
| U06-07 | 高窗与窄窗比例 | 代码完成待验收 | 候选表移除560 DIP上限并沿用有限剩余高度预算；3840×2160逻辑DIP增长至1665，900×600回缩；缓存页只在进入PageHost时按当前尺寸重排 | [前后与验证](evidence/U06-07-08/BASELINE.md)；[活动页面重排复测](evidence/U05-01-U06-07-U09-04-active-layout-20261102/BASELINE.md) | SDK8.0.423组合4/4及本批R08 5/5、均0 skip；Render QA双主题394图/0 PROBLEM；真实4K与Playnite宿主待验 |
| U06-08 | 大列表视觉成本 | 代码完成待验收 | 任务与媒体2千/1万/2万夹具复核有限实现行、Recycling/Standard例外和行/变换时钟；共享行模板无逐行Storyboard。R18受控STA Window下20k Task实现10行/视口8，Media后端20k受业务上限显示2k项并实现7行/视口7；20点detached scaleprobe媒体容器峰值44，改窗后6，两种夹具不可等同 | [本轮夹具与差异](evidence/U06-08-U09-04-scroll-followup-20261102/BASELINE.md)；[既有测量](evidence/U06-07-08/BASELINE.md) | R18 1/1通过、0 skip，scaleprobe退出0；保留Media Inbox Standard行模式与关闭列虚拟化安全例外。真实Playnite容器/帧率仍待验 |
| U07-01 | 对话框信息层级 | 代码完成待验收 | 保留标题、真实对象/影响文本及独立正文滚动；动作区采用可换行布局 | [实现与行为证据](evidence/U07-01-02/BASELINE.md)；[双主题双尺寸历史配对](evidence/U09-01-U07-01-04-pair-20261030/BASELINE.md) | SDK8.0.423对话框10/10、0 skip；历史图确认长确认按钮换行且正文滚动保持；长正文真实宿主待验 |
| U07-02 | 确认按钮位置 | 代码完成待验收 | 共用动作区统一8 DIP间距、长文案可换行；危险确认不设默认Enter，Escape取消并回焦 | [实现与行为证据](evidence/U07-01-02/BASELINE.md)；[双主题双尺寸历史配对](evidence/U09-01-U07-01-04-pair-20261030/BASELINE.md) | SDK8合成WPF10/10、0 skip；历史前后图覆盖1040×700和1366×768 DIP；新提交CI、真实Playnite/物理输入待验 |
| U07-03 | 提示框节制 | 代码完成待验收 | 编辑器默认提示锚定控件下方并尊重显式位置；真实 WPF Tooltip 在主显示器工作区底边空间不足时翻到控件上方 | [既有去重及双主题证据](evidence/U07-03-04/BASELINE.md)；[边缘几何回归](evidence/U07-03-06/BASELINE.md)；[搜索提示历史配对与屏幕坐标](evidence/U09-01-U07-01-04-pair-20261030/BASELINE.md) | SDK8隔离 WPF 7项中6通过、1既有系统指针跳过；离屏报告显示默认放置改为控件下方；实体编辑/IME、跨屏混合DPI、Playnite宿主待验 |
| U07-04 | 菜单与快捷键列 | 代码完成待验收 | 必要列禁用原因有鼠标提示和UIA说明；菜单顺序、可用项、UIA名称及生产ItemsPresenter循环导航配置回归通过；真实菜单无图标/快捷键/子菜单数据 | [既有菜单实现](evidence/U07-03-04/BASELINE.md)；[导航结构回归与焦点探针](evidence/U07-04-05/BASELINE.md)；[必要列提示双主题历史配对](evidence/U09-01-U07-01-04-pair-20261030/BASELINE.md) | SDK8定向5/5；历史图确认必要列仍禁用且补有鼠标提示；方向键实体遍历、Narrator与Playnite宿主待验，不把程序化Popup焦点探针当实际输入结论 |
| U07-05 | 通知卡片精修 | 代码完成待验收 | 通知记录新增受界限的真实来源字段和向后兼容加列迁移；任务来源来自 `TaskTypeDisplay`、会话摘要来自会话事件，旧记录明确显示来源未记录。列表标题下增加来源行，详情显示来源；标题Tooltip/UIA、排序/复制、虚拟化、焦点与现有滚动系统保留 | [本批实现、同样本双主题对照与局部门禁](evidence/U07-05-notification-source-20261029/BASELINE.md)；[既有收件箱实现](evidence/U07-05-06/BASELINE.md)；[到达焦点回归](evidence/U07-04-05/BASELINE.md)；[到达/清空筛选焦点](evidence/U07-05-08/BASELINE.md)；[长标题精修](evidence/U07-05-06/BASELINE.md) | SDK8.0.423 Release Worker通知定向13/13、Playnite通知/UIA定向7/7，0失败/0 skip；source/XAML/WPF静态/双主题Render QA exit0。cd062531完整根入口、独立build.cmd均exit0，CI baseline与SDK10均success；真实Playnite来源刷新、外部UIA/IME/物理DPI未验 |
| U07-06 | Toast堆叠与退出 | 代码完成待验收 | Toast标题单行省略且完整标题可由Tooltip、UIA及详情对话框读取；键盘焦点停留暂停计时；保留四条上限/最新优先，关闭中间卡片时退出动画不重排剩余项且正确回收计时器 | [既有详情与计时证据](evidence/U07-05-06/BASELINE.md)；[多卡堆叠回归](evidence/U07-03-06/BASELINE.md)；[长标题精修](evidence/U07-05-06/BASELINE.md) | SDK8.0.423 Release精确提交7/7、0 skip；Render QA双主题394图/0 PROBLEM；真实Playnite中Toast与关键操作区域的遮挡关系及本批CI待验 |
| U07-07 | 设置分组与帮助 | 代码完成待验收 | 巡检依赖条件就近常驻说明，保留全局错误定位 | [既有局部证据](evidence/U07-07-08/BASELINE.md)；[依赖邻接守护](evidence/U07-07-08-RECOVERY/BASELINE.md)；[浅深主题历史配对](evidence/U07-07-08/historical-render-pair-20261030/BASELINE.md) | SDK8.0.423精确提交10/10；历史双主题对照确认依赖说明可见、字段仍禁用；其它设置依赖及真实宿主待验 |
| U07-08 | 错误反馈恢复入口 | 代码完成待验收 | 任务重试说明提交范围和核对上次结果；增加权限/网络/传输/未知云端分类；命令未改 | [既有局部证据](evidence/U07-07-08/BASELINE.md)；[分类与WPF视图证据](evidence/U07-07-08-RECOVERY/BASELINE.md)；[浅深主题历史配对](evidence/U07-07-08/historical-render-pair-20261030/BASELINE.md) | SDK8.0.423精确提交10/10、0 skip；历史双主题卡片/UIA文本检查通过；Playnite宿主及外部UIA待验 |
| U08-01 | 概览首屏重排 | 代码完成待验收 | 眉题为中性“当前状态”；1040×700窗口中状态、下一步文字/命令/UIA可见，保留页面滚动通道 | [基线与可见窗口复验](evidence/U08-01-02/BASELINE.md)；[双主题同数据前后配对](evidence/U09-01-U08-pair-20261030/BASELINE.md) | SDK8.0.423 Release精确提交2/2、0 skip；历史双主题Render QA 394图/0 PROBLEM；宿主、实体UIA/输入、物理DPI及本批CI待验 |
| U08-02 | 存档页阅读路径 | 代码完成待验收 | 历史摘要显示所选游戏名称、身份Tooltip/UIA及未选择状态；1040×700下摘要/表格滚动边界可见，保留原命令与恢复前置 | [基线与可见窗口复验](evidence/U08-01-02/BASELINE.md)；[双主题同数据前后配对](evidence/U09-01-U08-pair-20261030/BASELINE.md) | SDK8.0.423 Release精确提交2/2、0 skip；历史双主题Render QA 394图/0 PROBLEM；未执行恢复；真实宿主、实体UIA/输入、物理DPI及本批CI待验 |
| U08-03 | 任务页扫描效率 | 代码完成待验收 | 截断任务文字完整 UIA HelpText；状态替换期间任务行保持高度稳定，阶段/进度更新可见 | [当前主线证据](evidence/U08-03-04-current-main-8ac477b9/BASELINE.md) | SDK8定向15/15；真实Playnite宿主、实体UIA/键鼠及该提交CI待验 |
| U08-04 | 媒体页图片与文字 | 代码完成待验收 | 占位文字覆盖无图/视频/缺失/损坏并可恢复；长虚拟列表滚动、追加、替换、移除选中项保持锚点与边界 | [当前主线证据](evidence/U08-03-04-current-main-8ac477b9/BASELINE.md) | SDK8定向15/15；本批无新Render QA；真实Playnite与物理DPI待验 |
| U08-05 | 工具页状态表达 | 代码完成待验收 | 来源/文件状态/自动启动说明清楚；启动请求有局部忙碌动画与UIA状态；不可跟踪时禁用原因可读 | [当前工作树证据](evidence/U08-05-06-current-main-d6d940e7/BASELINE.md)；[同数据双主题配对](evidence/U09-01-U08-05-06-pair-20261030/BASELINE.md) | SDK8本批5/5；当前主线5040ac5的run40958239784双SDK CI通过；真实工具启动、Playnite宿主、外部UIA/Narrator待验 |
| U08-06 | 维护页密度整理 | 代码完成待验收 | 诊断摘要先呈现；详情渐进展开；完整摘要复制命令保留；短窗可滚达摘要与操作入口 | [当前工作树证据](evidence/U08-05-06-current-main-d6d940e7/BASELINE.md)；[同数据双主题配对](evidence/U09-01-U08-05-06-pair-20261030/BASELINE.md) | SDK8本批5/5；本机WPF 1160×520 DIP覆盖；当前主线5040ac5的run40958239784双SDK CI通过；真实宿主待验 |
| U08-07 | 通知页整体一致 | 代码完成待验收 | 无结果可清除筛选并回焦搜索，空态文字复用共享样式；实际WPF窗口回归确认按钮焦点转移 | [局部证据](evidence/U08-07-08/BASELINE.md)；[筛选焦点回归](evidence/U07-05-08/BASELINE.md)；[固定样本双主题历史配对](evidence/U09-01-U08-07-08-pair-20261030/BASELINE.md) | SDK8既有15/15并新增通知视图4/4、0 skip；真实游戏/任务跳转和Playnite宿主待验 |
| U08-08 | 跨页一致性巡检 | 代码完成待验收 | 复核Overview时间、Save History最右列和Task队列状态/详情边界；共享行内容裁切与选框不遮挡文字，滚动条系统保留 | [本批跨页截图与修复](evidence/U06R5-2911/BASELINE.md)；[双主题目标截图](evidence/U06R5-2911/screens/task-dark.png)；[门禁/TRX](evidence/U06R5-2911/gates.json) | 精确292候选 394 PNG/0 PROBLEM；R06/R10回归和独立build通过；未观察实际Playnite页面，真实宿主交互待验收
| U09-01 | 对比证据最小集 | 已满足 | 复核U00源基线至冻结候选的37个UI源码提交；U01–U08视觉差异均映射到双主题历史配对，动效/生命周期有实际时间样本或事件序列。当前HEAD与`5c0c57b0`的`src/`、`tests/`相同，无额外未映射批次 | [完整覆盖审计](evidence/U09-01-coverage-20261030/BASELINE.md)；[37个源码提交清单](evidence/U09-01-coverage-20261030/ui-source-commit-audit.json)；[U01](evidence/U09-01-U01-pair-reconstruction-20261029/BASELINE.md)；[U02主题/几何](evidence/U09-01-U02-pair-reconstruction-20261029/BASELINE.md)、[U02几何补充](evidence/U09-01-U02-geometry-pairs-20261029/BASELINE.md)；[U03输入与禁用态](evidence/U09-01-U03-input-pair-20261029/BASELINE.md)、[禁用原生文字修正](evidence/U03-disabled-native-text-20261029/BASELINE.md)；[U04交互/中断](evidence/U09-01-U04-pairs-20261029/BASELINE.md)、[U04时间样本](evidence/U09-01-U04-interrupt-20261029/BASELINE.md)；[U05页面/详情双主题与动态样本](evidence/U09-01-U05-pair-reconstruction-20261029/BASELINE.md)；[U06候选表](evidence/U09-01-U06-pair-reconstruction-20261029/BASELINE.md)；[U07-01～04](evidence/U09-01-U07-01-04-pair-20261030/BASELINE.md)；[U08-01/02](evidence/U09-01-U08-pair-20261030/BASELINE.md)、[U08-03/04](evidence/U09-01-U08-03-04-pair-20261030/BASELINE.md)、[U08-05/06](evidence/U09-01-U08-05-06-pair-20261030/BASELINE.md)、[U08-07/08](evidence/U09-01-U08-07-08-pair-20261030/BASELINE.md) | Demo 文件当前不可用，未声称Demo视觉对齐；配对图和动态证据为受控离屏WPF数据，不代替真实Playnite、物理DPI、实体输入/外部UIA验收。
| U09-02 | 键盘与UIA回归 | 代码完成待验收 | SDK8.0.423 Release 精确提交`8deb17c7`组合16/16、0失败、0 skip；TRX原始与脱敏SHA已归档。修正R07夹具默认`SaveTabIndex`，增加可见/排列/UIA Invoke断言 | [U09-01/02/03证据](evidence/U09-01-03/BASELINE.md) | WPF MoveFocus/UIA Peer不等于实体键盘、IME、外部UIA或Playnite宿主验收 |
| U09-03 | 尺寸主题交叉回归 | 代码完成待验收 | 精确提交`8deb17c7`：双主题×11窗口尺寸，394 PNG、0 PROBLEM、逻辑DPI1.00；source validation通过、XAML26/26、WPF静态42文件/0 errors。Render QA原始及脱敏报告SHA已记录 | [U09-01/02/03证据](evidence/U09-01-03/BASELINE.md) | 物理DPI/多屏/Playnite窗口未验；Render报告含未跟踪外部输出，跟踪树在运行时干净 |
| U09-04 | 动画性能复测 | 进行中 | U00 与 a13a7dbb 的同夹具活动段托管累计分配下降 12.64%–13.36%；固定布局独立滚表基本持平。详情收起后立即滚动的对象为TaskGrid，9个对齐样本滚动代理中位数38.84→52.61ms、收起13.06→21.19ms。新增任务详情面板双主题探针：U00无动画，当前版有动画，选中项与终态稳定；私有桌面回调p95不作为实际呈现帧，未测累计分配，不能据此结案 | [同夹具复测](evidence/U09-04-aligned-20261106/BASELINE.md) · [滚动器归属](evidence/U09-04-scroll-owner-20261106/README.md) · [独立表格滚动](evidence/U09-04-taskgrid-scroll-20261102/BASELINE.md) · [任务详情动效探针](evidence/U09-04-task-inspector-motion-20261109/README.md) | 组合场景布局耗时增加，尚未定位到具体改动。真实Playnite呈现帧、物理输入延迟及该面板累计分配仍待验 |
| U09-05 | 标准cmd最终验收 | 进行中 | 独立 `scripts/build.cmd` 精确 `92bd9de` SDK8.0.423全测 exit0：2030项/1962通过/0失败/68 skip，157个WPF隔离类；精确冻结 `e42da2a5` 中文完整入口exit0：2030项/1972通过/0失败/58既有skip，Worker发布、ZIP/PEXT、安装身份及启动成功。`0258ab35` 的Render QA隔离补齐，run #1494 baseline与SDK10 jobs均success。用户持续反馈仍闪；重传附件实际与11/6同文件，无HWND/PID；时点快照无目标可见窗口，仍未获得当前瞬态身份 | [根链、TRX与安装身份](evidence/U09-05-root-e42da2a5-20261108/README.md) · [build.cmd完整构建/监测](evidence/U09-05-no-flash-20261107/full-build-no-flash-20261108/README.md) · [Render QA与CI #1494](evidence/U09-05-renderqa-private-20261109/README.md) · [闪屏反馈时点快照](evidence/U09-04-task-inspector-motion-20261109/current-window-snapshot.json) · [按压采样修正](evidence/U09-05-no-flash-20261107/r08-press-capture-20261108/README.md) · [本批选框像素复核](evidence/U09-05-frame-pixel-check-20261109/README.md) | 用户黑窗来源、全运行时段瞬态事件、最终冻结候选中文→英文根入口及安装身份、真实Playnite视觉页、外部UIA/实体输入/IME/DPI仍待验；小批只用build.cmd，不重复重启宿主；不强杀私有桌面残留进程 |
| U09-06 | GitHub双SDK门禁 | 远端待验 | 精确SHA `e42da2a5bd33e8988acb0b31218bdd66b3d96048` 的 workflow #1492 / run `42485016157` completed/success；baseline job `128744888778` 15/15步、SDK10.0.401 job `128744889050` 8/8步；三份artifact的SHA digest已核 | [CI #1492 job/artifact身份](evidence/U09-05-root-e42da2a5-20261108/ci-run-1492.json) | workflow两job通过；required-checks/ruleset接口此前403，不重复试探。需维护者提供只读读取权限或required-checks列表后核对分支保护完整性 |
| U09-07 | 产物清理与可追溯 | 已满足 | 当前源码/构建/CI证据已脱敏归档；旧9份失败TRX脱敏保存。本轮清理自有`.tmp`阶段中的一次性关闭脚本，仅4,096 bytes净增D盘空闲、C盘无变化；根链、失败诊断和最新包保留 | [前序清理记录](evidence/U09-05-final-5f0eb620/cleanup.json) · [完整构建清理](evidence/U09-05-no-flash-20261107/full-build-no-flash-20261108/cleanup.json) · [根链脚本清理](evidence/U09-05-root-e42da2a5-20261108/cleanup.json) · [历史失败TRX](evidence/U09-01-03/historical-failed-local-trx-manifest.json) | 保留原始完整build诊断与160份TRX于`artifacts/build-diagnostics/20261108T074929337Z-a6ae1e37/`；未碰用户或其他任务TestResults/存档/配置，未改卷或映射 |
| U09-08 | 整轮交付审查 | 进行中 | 80个任务ID逐项核对；计数仍为5项已满足、71项代码完成待验收、3项进行中、1项远端待验。U09-05根入口直接退出0且安装身份/CI #1492通过，用户闪屏来源仍无身份/时序证据；U09-06 required-checks完整性待只读权限 | [U09-05根链与闪屏边界](evidence/U09-05-root-e42da2a5-20261108/README.md) · [CI #1492](evidence/U09-05-root-e42da2a5-20261108/ci-run-1492.json) · [焦点回归TRX](evidence/U05-02-reduced-motion-focus/after.trx) · [最新选框与长文复核](evidence/U09-05-frame-pixel-check-20261109/README.md) | U09-04动画性能结论、U09-05黑窗来源/连续事件监测、真实插件页/实体输入/外部UIA/物理DPI、required-checks完整性仍待验 |

## 2026-11-07 Round5 U09-05 / CI #1488 动画等待稳定性修正

上一候选 `2699e3ec6e3020ba31129cc0f816287d27988bf8` 的 Windows CI #1488 SDK8 job 在 `CompactTaskInspectorReversesItsTransitionWithoutMovingTheQueueAnchor` 因固定等待后动画尚未到达终态而失败，SDK10 job通过。当前只改测试等待方式：使用有界状态轮询，并把反向关闭窗口设为测试本地1秒，保留选中项、焦点、行列布局及折叠状态断言；无生产XAML/资源/业务改动、无skip。SDK8.0.423 Release编译0警告/错误，目标行为在3个独立 WinSta0 私有桌面各1/1通过，合计3/3、0失败、0 skip。源码检查、WPF静态审查和diff-check通过；TRX、哈希与命令记录见[本批证据](evidence/U09-05-no-flash-20261107/sdk8-animation-wait-fix-20261107/README.md)。

此测试修正不定位或解决用户黑窗。新截图文件时间早于本轮测试隔离修订；后续冻结候选完整构建/测试、中文安装启动、全程默认桌面事件和精确SHA双SDK CI仍待验。U09-05保持进行中，整轮计数仍为5项已满足、71项代码完成待验收、3项进行中、1项远端待验；未操作盘符或映射，未执行真实恢复、媒体删除或云端写入。
## 2026-11-07 Round5 U09-05 用户再次报告闪屏与 R23 SDK10 用例修正

用户再次表示黑窗仍然出现，因此 U09-05 继续“进行中”。当前截图文件记录时间为 2026-11-06 13:59:27，没有 HWND、PID 或所属桌面；它早于 11/7 的 WPF 测试隔离提交，不能用来判断该提交后的窗口来源，也不能据它宣称问题已解决。

精确 pre-fix 提交 `e24a37faec8c2c971cb0d68132f7a2605a4048f4` 的 Windows CI run [#1487](https://github.com/Nikilua/GameSaveCenter/actions/runs/42335119569) 中，SDK 8 job `128290196828` 成功，SDK 10.0.401 job `128290197024` 失败于 `WorkspaceTabSelectionIndicatorReversesAndHonorsReducedMotionInBothThemes`：隔离桌面在 250ms 窗口内未同时给出进入/退出的中间帧。已读取原始 SDK10 日志；失败诊断 artifact `13559861369`，1,058,146 bytes，SHA-256 `466aeb86191511a48202a6bc1864996e811f650c3781df06fc1438ff0ce982d6`。其它 artifact 与 job 身份见 [pre-fix CI 记录](evidence/U09-05-no-flash-20261107/sdk8-r23-timeline-20261107/ci-before-fix.json)。

测试现在从生产 `GscRedesignWorkspaceTabItem` 读取 Storyboard，检查目标、Opacity 属性、时长、缓动中点，以及浅/深主题切换最终态和减弱动效状态；不再依赖隔离桌面合成器在固定 250ms 内提供帧样本。未改生产 XAML、动效时长、业务代码、断言 skip。SDK 8.0.423 / Release 当前精确程序集 `118AEDA80B5D403000ACF957C90BC78A3027272A2A4B64EBDB5A779B6A731E58` 下，定向用例在 3 个独立 WinSta0 私有非输入桌面各 1/1 通过、0 失败、0 skip；输入桌面前后均为 `Default`。编译、Source Validation 与 `git diff --check` 通过。脱敏 TRX、SHA 与运行身份见 [R23 定向证据](evidence/U09-05-no-flash-20261107/sdk8-r23-timeline-20261107/README.md)。

这些结果只证明该测试不再依赖不稳定的合成帧采样，不证明用户所见黑窗已定位或消失；本批也没有真实 Playnite 页面视觉或手动 Explorer 启动验收。当前工作树的 `scripts/build.cmd` 完整构建测试和中文入口完整安装启动/身份核验仍待冻结提交后运行；上一轮 Playnite/Worker 仍有进程且官方关闭等待超时，未强杀或重复启动。提交后的双 SDK workflow、run/job/artifact 身份待查。U09-05 继续进行中，整轮计数仍为 5 项已满足、71 项代码完成待验收、3 项进行中、1 项远端待验。未操作盘符或映射，也未执行真实存档恢复、媒体删除或云端写入。

## 2026-11-08 Round5 U09-05：完整 `scripts/build.cmd` 与交互桌面监测

冻结源码提交 `92bd9de13bd40c13806fb0cba8900ada2e379fc2` 的独立 `scripts/build.cmd -OutputRoot .tmp/r5-r08-final-full-20261108/output` 使用 SDK8.0.423 完整 Release build/test exit0。160份TRX共2030项：1962通过、0失败、0 error/timeout/abort、68个按明确原因跳过（57条旧“今日工作台”架构断言，9条Named Pipe客户端、1条Named Pipe服务端受环境限制，1条默认跳过的系统指针测试）；157个WPF隔离类全部完成，TRX计数无不一致。R08按压/悬停反转类完整TRX亦通过。skip不计通过。原始结果与哈希见[完整构建及桌面监测证据](evidence/U09-05-no-flash-20261107/full-build-no-flash-20261108/README.md)。

WinEvent监听线程绑定 `WinSta0\Default`，与input desktop `Default`一致，覆盖构建/测试整个37分钟区间，顶层show/hide事件0。此次明确证明独立`build.cmd`没有在用户交互桌面创建可见窗口，但没有调用最终会启动Playnite的中文`GameSaveCenter-Run.cmd`。旧Playnite PID 23488/Worker PID 30516无主窗口句柄，均未关闭/重启；不把这次零事件扩大解释为用户历史黑窗已解决。未操作盘符/映射，未执行存档恢复、媒体删除或云写入。

当前SHA的 CI #1490 / run `42459902671` 已 completed/success：SDK10.0.401 job `128666747381` 8/8步与基线 job `128666747608` 15/15步均成功，包含 Release 编译/测试、Render QA、WPF静态审查、扩展打包和上传。预览artifact `13610836964`（90,793,668 bytes，digest `162f7b7955c8b0a2ef7c87425329afa13241c4f821247ab8881ae2e51de2e4bf`），基线诊断 `13610448945`（2,524,028 bytes，digest `89ccc21dd9929d63fdf8da5a10e6a757f41c6ad2e08d0c9e368f53bb3d82131e`），SDK10诊断 `13610475659`（1,278,945 bytes，digest `4876f23877623874671e8f2a16221d1f8302350b3d84a592832305dce2d1d196`）；清单见[evidence](evidence/U09-05-no-flash-20261107/full-build-no-flash-20261108/ci-run-1490.json)。故 U09-06 的 workflow jobs 已通过；required-checks读取此前403，仍远端待验，U09-05也因黑窗和中文安装启动未验继续进行中。

本阶段证据副本SHA核对后，只清理自有 `.tmp/r5-r08-final-full-20261108/`：728文件/169,300,382 bytes，0重解析点、0活动build进程。D盘空闲实测11,950,292,992→12,121,559,040 bytes；C盘前后4,907,458,560 bytes。清理记录与删除路径见[cleanup.json](evidence/U09-05-no-flash-20261107/full-build-no-flash-20261108/cleanup.json)；原始artifacts/TRX、其他任务TestResults保留，未改卷/盘符/映射。

## 2026-11-08 U09-05 用户再次报告闪屏：确认启动脚本行为

对照三个实际入口：`scripts/build.cmd` 只隐藏启动 PowerShell 并调用 `build.ps1`，不启动 Playnite；`GameSaveCenter-一键构建安装运行.cmd` 转交 `GameSaveCenter-Run.cmd`，后者执行 `dev-install-run.ps1`。该脚本先完成 build/test/package，再关闭已发现的 Playnite（有主窗则请求正常退出；超过20秒且确认是当前会话的无窗 Playnite 时才强停），替换插件，然后用普通 `Process.Start` 启动 Playnite。因此反复调用完整入口会重复触发宿主窗口切换；这是已确认的潜在闪窗路径，但截图没有窗口身份，不能确认它就是截图来源。

后续小批编译和行为检查使用 `scripts/build.cmd` 与定向无界面验证，不为每批反复运行会关闭并重开的中文完整链。冻结候选仍需满足中文完整链门禁；宿主启动验证与零闪屏验证分别记录。当前 Playnite PID 23488、Worker PID 30516 未触碰。
