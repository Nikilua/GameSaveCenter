# U09-05 控制台隐藏启动复核（2026-11-10）

## 结果

- 源码基线为 `e400e42db0bc2d5adeac873052244ecd2367d350`；构建时只有 `scripts/hide-console.ps1` 与 `scripts/tests/Test-LauncherExitCodes.ps1` 带有本批未提交改动。改动快照以base64和SHA保存在 `source-diff.json`，两个文件的长度与 SHA-256 在 `source-files.json`。
- 隐藏隔离启动的 launcher fixture 通过：`GameSaveCenter-Run.cmd`、`GameSaveCenter-一键构建安装运行.cmd`、`scripts/build.cmd` 各测试退出码 0、7、23，共 9/9；新增父进程/共享控制台策略 5/5。完整 stdout/stderr 与逐入口日志在 `launcher-fixture/`。
- 独立真实入口 `scripts/build.cmd` 在 SDK 8.0.423 / Release 完整运行，根进程退出码 0。2,032 项中 1,964 通过、0 失败、68 个既有 skip；157 个 WPF 类全部在独立私有桌面逐类运行。Summary 中 157/157 桌面身份与输入桌面检查均匹配，Default 输入桌面前后不变。逐类 TRX、完整 summary 和构建/测试控制台记录位于 `success-short-path/`。
- 首次长隔离路径运行退出码 1：三个配置/工作清单恢复断言失败，1,102 通过、3 失败、28 skip；WPF 类尚未开始。缩短输出和 TEMP 路径后全套测试通过，没有跳过或修改断言。失败原始 TRX、summary 与日志保留在 `failure-long-path/`；长路径敏感性是本次观察到的差异，真实根因仍以失败和成功原始诊断为准。
- 这次启动的 cmd、PowerShell 与 dotnet 进程查询到的 `MainWindowHandle` 均为 0；构建由 `Start-Process -WindowStyle Hidden` 启动并重定向日志。`scripts/validate-source.py` 与 `git diff --check` 均退出 0。

## 验收边界

上述进程级结果说明本轮编译/检查进程没有可见主窗口，不足以证明整个用户交互桌面绝无短暂窗口，也不能确认附件黑窗的 HWND/PID。本批没有运行安装启动链或真实 Playnite 页面；因此没有把用户反馈标记为宿主实测解决。U09-05仍“代码完成待验收”，真实 Playnite/用户桌面观察待验。清理前C盘实测4,173,307,904 bytes、D盘19,061,547,008 bytes；首次清理后C/D为4,172,320,768 / 19,414,482,944 bytes。压缩归档暂存区清理后的最新复测为C/D 4,145,061,888 / 19,418,931,200 bytes；C距中文根入口4 GiB门槛仍差149,905,408 bytes。各快照和自有输出清单见 `cleanup.json`；完整链继续等待空间预检满足。

本批只改构建控制台辅助脚本与策略测试，没有 WPF/XAML/资源改动，因此不运行 Render QA。阶段临时输出清单与磁盘前后值见 `cleanup.json`。
