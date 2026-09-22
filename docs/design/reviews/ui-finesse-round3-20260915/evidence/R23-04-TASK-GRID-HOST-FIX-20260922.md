# R23-04 任务表宿主初始化修复与当前嵌入证据

日期：2026-09-22
分支：`codex/ui-finesse-round2`
当前代码身份：`9026f4a2812c5c534a46abb1bf79aed7afd7988d`（`补齐任务表排序列映射`）
任务：R23-04「非空隔离宿主」

## 结论

本批收口了 R23-04 runner 暴露的两个真实宿主初始化错误，并重新绑定当前代码运行真实 Playnite 嵌入捕获。账本状态提升为“已满足，待宿主环境验证”，不提升为完整宿主验收。

- `TaskGrid` 实际有 7 列：`local-time`、`task`、`stage`、`game`、`state`、`progress`、`detail`。此前列宽持久化契约只有 6 个 key；补齐 `stage` 并新增实际 XAML 与生产代码顺序/数量回归测试。
- 第一次修复后的当前宿主 runner 继续暴露排序契约仍只有 6 项；补齐 `ProductionDataGridSortProfiles.AttachTasks` 的 stage 排序，并补未知阶段置后的真实 resolver 文本负例。代码提交为 `9026f4a2`。
- 当前身份的完整 Release 门禁通过：XAML `24/24`、Core `125/125`、Worker `355/355`、Playnite source `111` 类、WPF `101` 类；Release `0 errors`，仅保留既有 `MediaCenterView.xaml.cs:699` 两条 `CS8602` warning。
- 当前 `artifacts/GameSaveCenter-0.6.73-playnite.zip` 与 `.pext` 均为 `44,061,248` bytes，SHA-256 均为 `D30E65D9B4B675FC6D20BC50BDAAC8E460FB67B64218061B25E90B7AA321D17`；包身份与宿主 summary 均绑定 `9026f4a2`。
- 重新运行真实 `D:\software\Playnite\Playnite.DesktopApp.exe` 后，09:13 启动日志加载 `GameSaveCenter` `0.6.73`，没有新的 `Column key count` 或 `Sort contract count` 未处理异常。当前 runner 于 09:16:15 生成了绑定 `9026f4a2` 的 `summary.json` 和 `capture-manifest.json`；summary 的 `EmbeddedDashboardCaptured=true`、`EmbeddedSettingsCaptured=true`、`ControlledDashboardCaptured=false`、`HighGateCount=0`。

## 修复边界

### 任务表列契约

真实宿主先后记录了两种异常：

1. `DataGridColumnLayoutController` 报 `Column key count must match the DataGrid column count`；
2. 列宽修复后，`DataGridStableSortController` 报 `Sort contract count must match the DataGrid column count`。

修复均保持现有 7 列顺序，不删除 stage，也没有替换 DataGrid、选框或滚动系统。排序使用已有 `TaskStatusDto.StageDisplay`/`HasKnownStage`；未知阶段被置后。新增 `R06SortingBehaviorTests.TaskStageSortKeepsUnknownStageLast` 覆盖“已识别阶段排序 + 未知阶段负例”，列宽回归测试覆盖实际 XAML 顺序与生产 key 顺序。

### 当前宿主捕获

当前输出目录：`artifacts/ui-host-audit-r23-04-sort-fix-20260922`。

| 事实 | 当前值 |
| --- | --- |
| `runner-metadata.json` commit | `9026f4a2812c5c534a46abb1bf79aed7afd7988d` |
| `metadata.json` CaptureOrigin | `EmbeddedPlaynite` |
| `DashboardWasAlreadyHostedByPlaynite` | `true` |
| `DedicatedAuditWindowUsed` | `false` |
| Dashboard / Settings 捕获 | `33 / 1`，均为 `EmbeddedPlaynite` |
| 当前 Dashboard viewport | `1970×1347 px`，`1313.33×898.0 DIP`，DPI `1.5` |
| 合成库 | 隔离 `games.db`，`Game` 集合 `1` 条 |
| 显示器 | `DISPLAY1` 单屏，Q24-03 `blocked-single-display` |

精选当前原图包括 `embedded-current/dashboard/viewport/overview.png`、`saves.png`、`trainers.png`、`media.png`、`tasks.png` 和 `maintenance.png`；完整路由与尺寸以当前 `capture-manifest.json` 为准，不能引用上一轮旧 commit 图片代替。

## 仍未验边界

- runner 的 UI Automation 仍未找到 `GameSaveCenter` 侧栏项；`ControlledDashboardCaptured=false`，没有专用审计窗口证据，也没有把外层自动点击/键盘路径写成通过。当前 `summary.json`/manifest 是插件在真实 Playnite 嵌入宿主生成的当前身份证据，不替代 UIA 可达性证据。
- 配置的 Fusion 主题未复制到隔离 profile，Playnite Desktop 版本字段为 `unknown`；不据此猜测主题或版本。
- 单屏、真实 presented frame、物理跨屏、ETW/PresentMon 和宿主性能仍未验；不以当前嵌入原图、WPF Rendering 或 Stopwatch 代理替代这些结论。
- 业务验证只用合成库、fake/隔离 Worker 和隔离用户数据，没有读写真实存档、删除真实媒体、写用户云端或外发诊断。Demo 原目录不可用，继续沿用恢复生产基线与 Demo-first 资源链。

下一可执行任务：把 R23-04 UIA/Controlled host 路径保留为外部待验边界，转做 R23-05 shell/media 五项几何失败的小批量修复；真实 presented frame 继续等待合规采集权限。
