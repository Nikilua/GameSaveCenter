# R23-04 隔离宿主启动夹具根因与修正（2026-09-24）

## 首次运行结果（历史快照）

- 候选提交 `42884321482a81c6cf46597b4b952a7d66e9c79f` 的 Release 包流程完成，六份插件/Worker/共享程序集身份一致；GameSaveCenter 与 seeder 只安装到 `.tmp/r23-04-seeded-host-20260924-d26bfd4a/Extensions`。版本化 `.pext/.zip` 按隔离 runner 要求保留，没有覆盖。
- Playnite 主进程显示 `Startup Error`。UIA 仅观察到顶层“Startup Error”对话框，未发现 GameSaveCenter 侧栏、`summary.json` 或真实 Dashboard。对话框文本提示 Playnite 上次启动异常，并询问是否以禁用第三方主题/扩展的安全模式启动；本次未选择安全模式。
- manifest 未生成：run ID `d0eae852-7672-422f-bdd9-9fbdb7e6bcb3`，`RuntimeStatus=not-observed`、`RuntimeManifestObserved=false`。请求 64 条合成游戏不能记为已导入。`cef.log` 为 0 字节；`playnite.log` 保留的是 profile 初始化阶段 WMI “拒绝访问”与 `Application started`，本次启动后没有 CEF `platform_channel 0x5` 记录。结论是“真实宿主未到 Dashboard，具体启动异常待修复”，不是 CEF 权限阻塞。
- 只有一台显示器，Q24-03 仍为 `blocked-single-display`。

原始审计输出：`artifacts/ui-host-audit-r23-04-seeded-20260924-d26bfd4a/runner-metadata.json`、`host-window-exposure.json`、`dev-install-report.txt`。隔离 profile 原始日志：`.tmp/r23-04-seeded-host-20260924-d26bfd4a/playnite.log`、`cef.log`。

## 审计夹具自身缺陷与因果边界

复查 `Initialize-IsolatedPlayniteConfig` 发现，Playnite 创建首份隔离配置后 runner 只请求关闭并等待 2 秒，随后会对仍运行的进程执行 `Stop-Process -Force`。旧 profile 留下 `safestart.flag`，随后一次运行确实出现通用安全模式对话框。这证明夹具有强杀/残留标记缺陷，但不能单凭时间顺序断定 `42884321` Startup Error 的唯一根因；该次 `cef.log` 为空。

另一个可复现夹具问题是主题查找仅将配置主题 ID 拼成用户主题目录名。安装版内置主题位于 `D:\software\Playnite\Themes\Desktop\Default`，其 `theme.yaml` 的 ID 才是 `Playnite_builtin_DefaultDesktop`，旧逻辑因此报“主题未找到”。这会让隔离窗口缺少预期主题表面；它是待修的宿主夹具问题，不是用户主题配置变更。

已在 `08a10da9` 修正：bootstrap 等待主窗口可关闭并等待正常退出；超时即停止 audit，不强杀；主题按 `theme.yaml` ID 从用户主题目录与 Playnite 安装目录解析，仅复制到隔离 profile。该修正独立于其后的 CEF 权限问题。

## 最新全新 profile 尝试（2026-09-24）

- 使用 `.tmp/r23-04-seeded-host-20260924-08a10da9`，没有复用旧 profile。Playnite 启动后没有正常退出，`safestart.flag` 仍在；runner 在安装扩展/seeder 前停止。没有第二次宿主启动，也没有 GSC 加载、当前 run ID manifest、`summary.json`、Dashboard、UIA 侧栏或截图。本次不能算成 UI 失败或成功。
- 此次 `.tmp/.../cef.log` 有直接行：`[27032:11988:0924/103715.922:FATAL:mojo\public\cpp\platform_channel.cc:108] Check failed: . : 拒绝访问。 (0x5)`。错误发生在隔离 profile 初始 bootstrap、扩展安装之前；这是当前已确认的 CEF/系统访问拒绝阻塞，与先前强杀夹具缺陷分开记录。当前机器同一环境不重复启动；不删除 safe-start 标记、不绕过权限。
- 当时 runner 在 `Initialize-IsolatedPlayniteConfig` 异常后、初始 `runner-metadata.json` 写盘前退出，所以 `artifacts/ui-host-audit-r23-04-seeded-20260924-08a10da9` 没有运行元数据。当前工作树为失败分支补充了 catch：按 CEF 直接拒绝标记分类，先写 `runner-metadata.json` 和 `host-startup-blocker.json`，再重抛异常。

原始 profile 日志：`.tmp/r23-04-seeded-host-20260924-08a10da9/cef.log`、`playnite.log`、`safestart.flag`。本地隔离目录保留作证据，不在同状态下复跑。

## 失败证据路径验证

- 基于 HEAD `08a10da9` 的当前工作树，Release solution build：0 errors，保留两条既有 `MediaCenterView.xaml.cs:703 CS8602` warnings；XAML structural check `24/24`。
- `DiagnosticsEvidenceSourceTests 7/7` 通过，覆盖 bootstrap 关闭/主题查找和 runner 将失败证据写盘置于安装脚本之前；离线解析并执行 `New-IsolatedBootstrapFailureEvidence` 的合成正例/负例也通过：带 `platform_channel`+拒绝访问/`0x5` 分类为 CEF blocker，不含拒绝标记则分类为一般 bootstrap 失败，两者 `CountsAsVisualPass=false`。没有启动 Playnite。
- 用户四页布局当前行为 Light/Dark `8/8` 通过，TRX 无 `InvalidComObjectException`。Source validator、PowerShell AST 和 diff check 通过；WPF 静态审查 `0 errors`、30 warnings/177 info，warning/info 为全仓审查输出，未用它们宣称宿主验证。
- 此次构建在 `08a10da9` HEAD 上包含尚未提交的脚本/测试修改；提交后会按最终源码 identity 再构建和复跑定向用例。原始 TRX：[`R23-04 bootstrap evidence 7/7`](R23-04-BOOTSTRAP-FAILURE-EVIDENCE-20260924.trx)、[`用户四页行为 8/8`](USER-REPORTED-LAYOUT-CURRENT-MAIN-RECHECK-20260924-LATEST.trx)。

## 完成边界

R23-04 仍未满足：没有本次 run ID manifest、非空游戏库、Embedded Dashboard、UIA 侧栏或精选原图。下一步需先有环境变化能让隔离 Playnite bootstrap 正常启动并退出；当前已观察到 CEF `platform_channel 0x5`，相同权限/系统状态下不重试。失败 catch 的元数据路径可用离线合成日志夹具验证。真实用户 profile 未使用。
