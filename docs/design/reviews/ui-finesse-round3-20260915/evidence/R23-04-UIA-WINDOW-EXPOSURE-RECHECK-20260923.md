# R23-04 UIA 窗口候选探测复测

日期：2026-09-23（Asia/Shanghai）  
工作区：`D:\workplace\github\GameSaveCenter`  
分支：`codex/ui-finesse-round2`  
代码身份：`f551359c58142e43bfc9dbe4dd3dff3987111244`

## 本批变更

`scripts/real-host-audit.ps1` 原先只使用 Playnite 进程的 `MainWindowHandle` 创建 UIA 根节点。本批复用已有 Win32 `EnumWindows` 结果，逐个探测同一 Playnite PID 的顶层窗口，并保留 `MainWindowHandle` 在枚举时间点缺失时的候选回退。`host-window-exposure.json` 现在还记录每个候选的标题、类名、可见性、UIA 根节点名称/控件类型、是否找到 `GameSaveCenter` 侧栏元素、匹配窗口和实际动作；未找到时仍返回 partial，不猜测句柄、不把顶层窗口升级成 UIA 通过。

本批没有修改生产控件、游戏选框、滚动条、命令/Binding、取消/错误语义、恢复保护或 Playnite `net462` 路线。

## 门禁

- PowerShell AST：`real-host-audit.ps1 parse OK`。
- `scripts/check-xaml.ps1 -ProjectRoot .`：XAML `24/24`。
- `python scripts/validate-source.py`、`git diff --check`：通过。
- 当前 Release `-SkipTests` 构建：`0` errors，保留 `MediaCenterView.xaml.cs:706` 两条既有 `CS8602` warning。
- `DiagnosticsEvidenceSourceTests.RenderAndHostAuditEntriesDeclareEvidenceBoundariesAndTimingFields`：`1/1`；源码契约改为核对当前进程快照/刷新路径，不恢复已删除的旧实现字符串。
- 隔离安装器校验：`extension.yaml 0.6.73`、DLL `0.6.73.0`，仅使用本批合成 profile；runner 明确使用 `-SkipInstallTests`，不把未执行的全量安装测试写成通过。

## 真实宿主结果

使用命令：

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/real-host-audit.ps1 -Configuration Release -Output "D:\workplace\github\GameSaveCenter\artifacts\ui-host-audit-r23-04-uia-candidates-20260923" -UserDataDir "D:\workplace\github\GameSaveCenter\.tmp\r23-04-uia-candidates-user-20260923" -PlayniteExecutable "D:\software\Playnite\Playnite.DesktopApp.exe" -TestTempRoot "D:\workplace\github\GameSaveCenter\.tmp\r23-04-uia-candidates-test-20260923" -SkipInstallTests
```

当前输出 [`artifacts/ui-host-audit-r23-04-uia-candidates-20260923`](../../../../../artifacts/ui-host-audit-r23-04-uia-candidates-20260923) 绑定完整 SHA `f551359c...`。`host-window-exposure.json` 的事实为：

| 项目 | 结果 |
| --- | --- |
| Playnite PID | `3556` |
| `MainWindowHandle` | `0x40B24` |
| 可见顶层窗口 | `Startup Error` / `#32770` |
| 顶层窗口总数 | `5` |
| UIA 探测 | `60 秒`、`30` 次尝试；5 个候选根节点均可建立 |
| `GameSaveCenter` 侧栏 | 未找到；`MatchedWindows=[]`、未执行 Invoke/Select/键盘输入 |
| 结果分类 | `top-level-window-observed-ui-automation-not-confirmed` |
| `summary.json` | 未生成，runner 以 `[PARTIAL]` 结束 |

5 个候选中只有 `Startup Error` 可见，其 UIA 根节点为 `ControlType.Window`；其余为隐藏的 .NET 广播、CicLoader、MSCTF/IME 辅助窗口，均未出现侧栏元素。隔离 `cef.log` 仍记录 `mojo\public\cpp\platform\platform_channel.cc ... 拒绝访问。 (0x5)`；`playnite.log` 记录应用启动，但没有可用于本批 UIA 通过的正常 Playnite 主窗体。

## 结论与边界

本批证明 runner 已实际逐窗探测并保全 UIA 候选事实；没有证明 Playnite 正常主窗体、GameSaveCenter 侧栏、键盘焦点、读屏或 Controlled host 可达。真实 presented frame、物理 DPI/跨屏、ETW/PresentMon 和宿主性能仍未验；本机单显示器使 Q24-03 继续 `blocked-single-display`。Demo 原目录不可用，业务验证只使用合成 profile、fake/隔离 Worker，没有读写真实存档、删除真实媒体、写用户云端或外发诊断。

隔离 Playnite PID `3556` 已停止；待文档提交后清理本批未被证据引用的 profile、测试临时目录和中间构建目录，保留当前审计输出作为本报告引用的证据。

下一可执行任务：取得能够稳定暴露正常 Playnite 主窗体的隔离桌面会话并重跑 UIA/Controlled；若 CEF/窗口暴露条件继续阻塞，则转依赖已满足的 Q/R 小批量。R23-04 状态保持“已满足，待宿主环境验证”。
