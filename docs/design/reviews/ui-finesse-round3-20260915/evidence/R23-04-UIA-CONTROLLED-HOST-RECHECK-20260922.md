# R23-04 UIA / Controlled host 复测

日期：2026-09-22
工作区：`D:\workplace\github\GameSaveCenter`
分支：`codex/ui-finesse-round2`
复测身份：`f457a7a0b68f35b53f2934040a4141265628854c`

## 结论

本批没有生产代码变更。真实的 `D:\software\Playnite\Playnite.DesktopApp.exe` 已在隔离用户数据目录中加载当前 GameSaveCenter 包，并由插件生成了绑定当前提交的嵌入式 Dashboard 截图、视觉树、资源快照、样式指纹和滚动复测输出。这个结果证明当前包能进入 Playnite 的 Dashboard 承载路径，但不等价于 UIA、键盘或专用 Controlled host 验收。

UI Automation 没有定位到 GameSaveCenter 侧栏项；runner 等待约 90 秒后没有生成 `summary.json`，脚本以 partial 结果退出。因此 R23-04 仍保持“已满足，待宿主环境验证”，不把本次嵌入式图像或结构化快照签成 UIA/键盘/专用窗口通过。

## 执行范围

使用的命令为：

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/real-host-audit.ps1 -Configuration Release -Output "D:\workplace\github\GameSaveCenter\artifacts\ui-host-audit-r23-04-uia-20260922" -UserDataDir "D:\workplace\github\GameSaveCenter\.tmp\r23-04-playnite-isolated-20260922" -PlayniteExecutable "D:\software\Playnite\Playnite.DesktopApp.exe" -TestTempRoot "D:\workplace\github\GameSaveCenter\.tmp\r23-04-playnite-audit-test-20260922"
```

- `UserDataDir`、Worker 数据目录、数据库路径和 IPC 管道均为本次合成审计专用隔离范围；只创建了空的 `library/database.json`（`Version=4`），没有导入真实游戏、存档、媒体或云端数据。
- runner 绑定的 commit 为 `f457a7a0...`；Playnite 日志确认加载 `GameSaveCenter` `0.6.73`，Dashboard 已由 Playnite 托管。
- 宿主 metadata：`CaptureOrigin=EmbeddedPlaynite`、`Mode=embeddedcurrent`、`DashboardWasAlreadyHostedByPlaynite=true`、`DedicatedAuditWindowUsed=false`、`ProfileSizeApplied=false`、`ThemeOverrideApplied=false`；DPI `1.5`，PixelsPerDip `1.5`，Dashboard `1313.33 × 898 DIP`，主题为 `FollowPlaynite`，glass 开启、强度 `78`、动画开启。
- 本机拓扑只有 `\\.\DISPLAY10` 一个显示器，Q24-03 物理跨屏状态仍为 `blocked-single-display`。

## 门禁结果

- XAML 结构门禁：`24/24`。
- Core Release 测试：`125/125`。
- Worker Release 测试：`355/355`。
- Playnite 测试：源码类 `111` 个，隔离 WPF 类 `101` 个，均通过。
- Release solution：`0 error`，仅保留既有 `MediaCenterView.xaml.cs:699` 的 `2` 条 `CS8602` warning。
- 共享按钮复合内容、TaskGrid 七列和 stage 排序相关行为沿用已提交实现；本批不通过增加字符串断言替代交互证据。

## 证据与未验边界

当前证据目录为 [`artifacts/ui-host-audit-r23-04-uia-20260922`](../../../../../artifacts/ui-host-audit-r23-04-uia-20260922)。其中的 `metadata.json`、`runner-metadata.json`、`visual-tree-dashboard.json`、`resource-snapshot.json`、`style-fingerprints.json`、overflow 分类和 `embedded-current` 截图均绑定本次 `f457a7a0`。截图可用于核对当前嵌入式生产基线，不可用于声称最终 presented frame、物理跨屏或 Playnite Desktop 版本已验证。

以下事实继续保留：

- UIA 侧栏定位失败，未取得 UIA 元素树、键盘焦点移动、读屏输出或可复现的专用 Controlled dashboard 窗口；没有 `summary.json`，不能补写成功状态。
- `PlayniteDesktopVersion=unknown`；没有复制 Fusion 到隔离 profile，也没有把当前单屏条件改写为双屏通过。
- 没有使用 ETW/WPR/PresentMon，也没有绕过系统跟踪权限；宿主性能和真实 DWM/presented frame 仍未验。
- Demo 原目录不可用，继续参考已恢复生产基线；仓库当前没有 `scripts/validate_wpf_ui.py`，不把缺失脚本写成静态审查通过。
- 隔离 Playnite 已通过 `--shutdown --userdatadir` 关闭；本批产生的 profile、测试临时目录和未被证据引用的构建缓存已清理。当前审计输出因被本报告引用而保留。
- Playnite 隔离日志中的更新检查 TLS 报错属于宿主更新噪声，不作为本次 UI 结论；未向外发送诊断。

## 下一步

下一可执行任务仍是 R23-04 runner 的 UIA/Controlled host 收口：需要一个能稳定暴露 GameSaveCenter 侧栏项并生成 `summary.json` 的隔离宿主交互路径。若该外部条件继续不可用，则转做 R23-05 的独立几何小批量；只能报告可复现的 WPF 几何/代理数据，不能把离屏或代理数据写成真实呈现帧性能。
