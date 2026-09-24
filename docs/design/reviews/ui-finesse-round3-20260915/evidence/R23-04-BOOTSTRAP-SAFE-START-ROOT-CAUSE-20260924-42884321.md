# R23-04 隔离宿主启动夹具根因与修正（2026-09-24）

## 本次运行结果

- 候选提交 `42884321482a81c6cf46597b4b952a7d66e9c79f` 的 Release 包流程完成，六份插件/Worker/共享程序集身份一致；GameSaveCenter 与 seeder 只安装到 `.tmp/r23-04-seeded-host-20260924-d26bfd4a/Extensions`。版本化 `.pext/.zip` 按隔离 runner 要求保留，没有覆盖。
- Playnite 主进程显示 `Startup Error`。UIA 仅观察到顶层“Startup Error”对话框，未发现 GameSaveCenter 侧栏、`summary.json` 或真实 Dashboard。对话框文本提示 Playnite 上次启动异常，并询问是否以禁用第三方主题/扩展的安全模式启动；本次未选择安全模式。
- manifest 未生成：run ID `d0eae852-7672-422f-bdd9-9fbdb7e6bcb3`，`RuntimeStatus=not-observed`、`RuntimeManifestObserved=false`。请求 64 条合成游戏不能记为已导入。`cef.log` 为 0 字节；`playnite.log` 保留的是 profile 初始化阶段 WMI “拒绝访问”与 `Application started`，本次启动后没有 CEF `platform_channel 0x5` 记录。结论是“真实宿主未到 Dashboard，具体启动异常待修复”，不是 CEF 权限阻塞。
- 只有一台显示器，Q24-03 仍为 `blocked-single-display`。

原始审计输出：`artifacts/ui-host-audit-r23-04-seeded-20260924-d26bfd4a/runner-metadata.json`、`host-window-exposure.json`、`dev-install-report.txt`。隔离 profile 原始日志：`.tmp/r23-04-seeded-host-20260924-d26bfd4a/playnite.log`、`cef.log`。

## 审计夹具自身缺陷

复查 `Initialize-IsolatedPlayniteConfig` 发现，Playnite 创建首份隔离配置后 runner 只请求关闭并等待 2 秒，随后会对仍运行的进程执行 `Stop-Process -Force`。隔离 profile 留下 `safestart.flag`；下一次本批启动实际出现了对应的“上次启动异常”安全模式对话框。因此本次 Startup Error 与夹具强制终止路径一致，不能归为外部 CEF 阻塞，也不能用于判断 GameSaveCenter 插件本身加载失败。

另一个可复现夹具问题是主题查找仅将配置主题 ID 拼成用户主题目录名。安装版内置主题位于 `D:\software\Playnite\Themes\Desktop\Default`，其 `theme.yaml` 的 ID 才是 `Playnite_builtin_DefaultDesktop`，旧逻辑因此报“主题未找到”。这会让隔离窗口缺少预期主题表面；它是待修的宿主夹具问题，不是用户主题配置变更。

已在工作树修正：bootstrap 等待主窗口可关闭并等待正常退出；超时即停止 audit，不再强杀或留下 safe-start marker；主题按 `theme.yaml` ID 从用户主题目录与 Playnite 安装目录解析，仅复制到隔离 profile。新增源守卫用例覆盖上述约束。Release solution `0 errors`，定向 `DiagnosticsEvidenceSourceTests 7/7`，`validate-source.py`、PowerShell AST 与 diff check 通过。修复后的真实隔离宿主验证仍待新提交和全新 `.tmp` profile。

## 完成边界

R23-04 仍未满足：没有本次 run ID manifest、非空游戏库、Embedded Dashboard、UIA 侧栏或精选原图。下一次只在 bootstrap 正常退出且安装主题已复制的全新隔离 profile 中运行一次；若 bootstrap 仍不能正常退出，runner 应先停在明确错误，不启动宿主。真实用户 profile 未使用。
