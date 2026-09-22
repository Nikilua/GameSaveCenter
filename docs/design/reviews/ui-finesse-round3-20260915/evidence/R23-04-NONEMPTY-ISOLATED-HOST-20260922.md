# R23-04 非空隔离宿主证据

日期：2026-09-22  
分支：`codex/ui-finesse-round2`  
当前代码身份：`5b5d6305521e59d0644e21db845b6f4aa7ff1d1c`（`修正下拉模板证据校验`）  
任务：R23-04「非空隔离宿主」

## 结论

本批取得了当前提交在真实 Playnite 嵌入宿主中的非空隔离库证据，但外层 runner 没有完成最终收口，因此不把本项写成完整宿主验收。

- 已满足：真实 `Playnite.DesktopApp.exe` 使用隔离用户数据目录启动；合成 `games.db` 读取到 `Game` 集合 1 条记录；当前提交的包身份已安装到该隔离 profile；插件在 Playnite 已托管的嵌入界面生成当前截图和布局/资源快照。
- 待收口：UI Automation 未找到 GameSaveCenter 侧栏项，runner 随后等待 `summary.json` 时宿主已退出并被中断；输出没有 `summary.json`，也没有 `embedded-current/dashboard/capture-manifest.json`。因此没有把外层 manifest 完整性、侧栏 UIA 可达性或专用审计窗口路径写成通过。
- 环境边界：本机只有 1 个物理显示器，Q24-03 记录为 `blocked-single-display`；配置的 Fusion 桌面主题未复制到隔离 profile，记录为 warning；Playnite Desktop 版本字段为 `unknown`，不作猜测。

## 固定身份与隔离数据

构建/测试门禁在同一提交上完成：

- XAML 结构：`24/24`。
- Core：`125/125`。
- Worker：`355/355`。
- Playnite source 测试类：`111`；Playnite WPF 测试类：`101`，均用隔离 testhost 运行并通过。
- Release 构建：`0 errors`；仅保留既有 `MediaCenterView.xaml.cs:699` 的两条 `CS8602` warning。
- `validate-source.py`、XAML 检查和 `git diff --check` 通过；WPF 静态基线为 `0 errors / 27 warnings / 177 info`，警告为既有质量提示。

隔离运行器元数据（`artifacts/ui-host-audit-r23-04-rerun-20260922/runner-metadata.json`）记录：

- 真实宿主：`D:\software\Playnite\Playnite.DesktopApp.exe`。
- 隔离 profile：`C:\Users\lopmatu\.codex\worktrees\1aa8\GameSaveCenter\.tmp\r23-04-synthetic-profile-20260922`。
- 合成库：上述 profile 下的 `library\games.db`；通过 LiteDB 读取 `Game` 集合计数为 `1`，没有读取或修改用户真实库。
- `EvidenceSource=RealPlaynite`，`UserDataMode=isolated-user-data`，包/插件和 runner 的 commit 均绑定 `5b5d6305521e59d0644e21db845b6f4aa7ff1d1c`。
- Worker 数据目录、IPC pipe 和 event pipe 均带 `isolated-audit-process` 范围，不使用用户生产 Worker 状态。

## 嵌入与专用窗口区分

当前嵌入元数据位于 `artifacts/ui-host-audit-r23-04-rerun-20260922/metadata.json`：

| 字段 | 实际值 |
| --- | --- |
| `Mode` | `embeddedcurrent` |
| `CaptureOrigin` | `EmbeddedPlaynite` |
| `DashboardWasAlreadyHostedByPlaynite` | `true` |
| `DedicatedAuditWindowUsed` | `false` |
| `ProfileSizeApplied` | `false` |
| `ThemeOverrideApplied` | `false` |
| `CommitSha` | `5b5d6305521e59d0644e21db845b6f4aa7ff1d1c` |
| `PluginVersion` | `0.6.73.0` |
| `PlayniteSdkVersion` | `6.16.0.0` |
| `DpiScaleX/Y` | `1.5 / 1.5` |
| `DashboardWidth/Height` | `1313.33 / 898.0` |
| `ThemeMode` | `FollowPlaynite` |

这证明当前截图来自 Playnite 已托管的嵌入界面，而不是把专用窗口或旧包截图当作当前实现。当前批没有产生专用审计窗口证据；该路径保持未验。

## 当前嵌入原图与结构证据

以下文件均来自当前提交的真实嵌入捕获，保留为精选原图；哈希用于防止后续把旧图替换进报告：

| 场景 | 文件 | SHA-256 |
| --- | --- | --- |
| 概览 | `artifacts/ui-host-audit-r23-04-rerun-20260922/embedded-current/dashboard/viewport/overview.png` | `725A0DB9738F7FD0B115B02ECE91E8B79A57AB7A56CF2844E9CD41337EA6FA58` |
| 媒体 | `artifacts/ui-host-audit-r23-04-rerun-20260922/embedded-current/dashboard/viewport/media.png` | `5C47DD4028C33E9666D977F336C09BCDC3D216BC74D2DB45BDAF512E023F3BD4` |
| 存档 | `artifacts/ui-host-audit-r23-04-rerun-20260922/embedded-current/dashboard/viewport/saves.png` | `D2D799B9F209655369E61B38A0E4EB4971B4916C071AF5273838BC25D3E22ECD` |
| 工具 | `artifacts/ui-host-audit-r23-04-rerun-20260922/embedded-current/dashboard/viewport/trainers.png` | `B63813DD910FB35AFC895790243B4F0C52C756C1E61297DD4457CEF4046CCED4` |
| 概览滚动面 | `artifacts/ui-host-audit-r23-04-rerun-20260922/embedded-current/dashboard/scroll-surfaces/workspace-overview__overviewstackscrollsurface.png` | `A5C9701987B374995F89506809CE8457C8E5123FEC13356FC146A41F6F3443A1` |

同一输出还保留了 `layout/embedded-current/workspace-*.json`、`resource-snapshot.json`、`style-fingerprints.json`、`visual-tree-dashboard.json` 和 `gates/overflow-classification.json`。当前溢出分类四项均为空：没有识别到固定布局溢出、意外滚动溢出、装饰溢出或审计误报。

## 未验边界与下一步

- 由于侧栏 UIA 定位失败，尚未证明真实宿主里的导航 UIA/键盘可达性，也没有把 runner 未完成的 summary/manifest 生成补写成事实。
- 没有物理跨屏、最终 presented-frame、ETW 或宿主帧性能结论；不以离屏截图、Rendering/Stopwatch 代理替代这些证据。
- 没有读写真实存档、删除真实媒体、写用户云端或外发诊断；所有业务库均为合成数据和隔离目录。
- Demo 原目录不可用，本批继续以恢复的生产基线和 Demo-first 资源链为参考。

下一可执行任务：先把 R23-04 的 runner UIA/summary 收口列为独立待验步骤；在此阻塞期间推进 R23-05 帧性能证据盘点，明确 Rendering 回调、Stopwatch 代理和真实呈现帧三类证据的分账。
