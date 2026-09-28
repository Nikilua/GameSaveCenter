# Media Inbox「待归类」滚动问题：2026-09-29 宿主前置复核

## 本次结果

- 当前隔离流程枚举到 `0` 个 Playnite/GameSaveCenter 冲突进程。
- 只读调用 `scripts/PlayniteHostIsolation.ps1` 的 `Assert-GscProcessCommandLineInspectionAvailable` 仍因 Windows 返回 `拒绝访问` 而失败。`scripts/real-host-audit.ps1` 在启动前要求该检查通过，以确认进程命令行确属唯一仓库 `.tmp` profile；因此本次 runner 拒绝启动。
- 未尝试普通用户 Playnite profile、未安装到真实扩展目录、未终止任何进程或绕过 WMI/ETW/CEF 限制。没有运行中插件 DLL 身份、MVID、窗口逻辑尺寸、WPF DPI、主题值，也没有真实 Playnite 的滚动前/中/底日志或截图。当前 `ae35be70` Release DLL 是待测候选构建身份，不是已加载宿主身份。
- 同提交隔离 WPF 的 `MediaInboxScrollBehaviorTests 3/3` 通过并留存于 [media-inbox-scroll-wpf.trx](media-inbox-scroll-wpf.trx)；用例覆盖合成数据滚动与缩放，不代表物理显示器 DPI 或 Playnite 最终呈现。退出噪声为 TextServicesHost `InvalidComObjectException`，xUnit/TRX 结果通过、exit `0`，根因未知。

## 现有诊断与待收集数据

生产 `DataGridScrollDiagnostics` 已为 `MediaInboxGrid` 记录插件信息版本/MVID/路径、主题、窗口与表格 DIP、WPF DPI、列头底边、`PART_ScrollContentPresenter` 顶边、首个视口相交行顶边，以及页面 ScrollViewer offset/viewport；列头、presenter、首行都以 DataGrid 为坐标原点。相关生产实现和隔离行为准备见[前序诊断准备](../MEDIA-INBOX-HOST-SCROLL-DIAGNOSTIC-PREP-20260928.md)。

启动条件恢复后，必须在同一隔离 Playnite 实例中先记录实际加载 DLL identity、window DIP、DPI、主题，再保存顶部、中段、拖动到底、重复往返和窗口缩放的原始 `[GSC-GRID-DIAGNOSTIC]` 行。比较 `presenterTop-headerBottom`：视口整体下移才检查共享 DataGrid 模板/页面有限高度；比较 `firstRowTop-presenterTop`：只有数据行下移才检查虚拟化、集合刷新和锚点恢复。验证首行贴住有效视口、末行完整、页尾操作可达，并在真实物理 125%/150% DPI 下复核。没有这些数据前不猜原因或改虚拟化/负边距。

本机旧隔离尝试背景见[2026-09-28 宿主复核](../MEDIA-INBOX-HOST-RECHECK-20260928.md)。用户也可以提供脱敏后的同进程原始日志继续定位。
