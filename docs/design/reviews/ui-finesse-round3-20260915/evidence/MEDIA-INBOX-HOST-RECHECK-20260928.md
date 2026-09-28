# Media Inbox 真实 Playnite 滚动问题：宿主条件复核（2026-09-28）

## 结论

本次无法在真实 Playnite 中复现用户报告，也没有修复滚动错位。没有运行中的 Playnite/GameSaveCenter 进程。找到了一份 Playnite Desktop 可执行文件，但隔离 runner 启动前要求的 `Win32_Process` 命令行检查当前仍返回“拒绝访问”；按现有安全流程，宿主启动被拒绝。本次没有尝试绕过、启动普通用户 profile、安装真实扩展或访问用户库。

## 当前检查与历史隔离启动

- 当前用户进程枚举为 0 个 Playnite/GameSaveCenter 进程。可执行文件版本资源显示 FileVersion/ProductVersion 均为 `1.0.0.0`；这只是文件元数据，不是运行中 Playnite 版本或已加载插件身份。
- 通过 `scripts/PlayniteHostIsolation.ps1` 的 `Assert-GscProcessCommandLineInspectionAvailable` 再次执行只读前置检查，结果为 Access Denied。隔离启动依赖该检查确保只启动并核验仓库 `.tmp` profile。
- 已有隔离尝试 `artifacts/real-host-media-inbox-20260928/runner-metadata.json` 绑定较早提交 `e32c246bf3613d68f4182095a6f17458263a7320` 和唯一 `.tmp` profile。其日志记载 bootstrap 进程命令行确实指向该隔离 profile，但配置从隔离备份恢复后，Playnite 未在安全超时内正常关闭；runner 拒绝强制结束并在扩展安装前退出。该次 CEF 尾部报告一个 Chromium extension unsupported manifest version；没有记录到 GSC extension load，也没有滚动诊断。它不是当前插件宿主复现。
- 因而运行中 DLL path/version/MVID、窗口 DIP、WPF DPI、主题，以及顶部/滚动后 `[GSC-GRID-DIAGNOSTIC]` 的 header/presenter/首行/外层 offset 当前均无值。

## 实现与待验边界

当前生产 `DataGridScrollDiagnostics` 已记录同一个 `MediaInboxGrid` 坐标系中的列头底边、`PART_ScrollContentPresenter` 顶边、首个可见行顶边和外层页面滚动偏移；隔离 WPF 回归通过不等于用户 Playnite 宿主复现。当前没有依据选择共享 `Redesign.xaml`/有限高度布局方向或行虚拟化/集合刷新/锚点方向，故本次不改 DataGrid 模板、虚拟化、页高或滚动锚点。

环境可用后需在同一运行中核验加载 DLL 身份、窗口 DIP/DPI/主题，并记录顶部、中段、拖到底和往返日志；再根据 `presenterTop-headerBottom` 或 `firstRowTop-presenterTop` 的变化定位并复核修复。用户也可提供上述脱敏原行以继续本项。物理 125%/150% DPI 和 Playnite 宿主最终复核仍待验。

