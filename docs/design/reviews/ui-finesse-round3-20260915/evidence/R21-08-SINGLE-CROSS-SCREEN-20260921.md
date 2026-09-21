# R21-08 单屏与跨屏分账证据

日期：2026-09-21  
分支：`codex/ui-finesse-round2`  
代码基线：`8a31421aea3e4ca3d61884a9ff6c035300faf723`（文档批次，未改生产代码）  
任务：R21-08「单屏与跨屏分账」

## 现有能力核对

本批先复核已有 Q24-03 前置和共享控件行为，没有新增跨屏定位、独立窗口、Popup 或 DPI 体系：

- `scripts/real-host-audit.ps1` 在宿主启动前使用 `System.Windows.Forms.Screen.AllScreens` 记录 `DisplayCount`、每个显示器的 `Bounds`/`WorkArea`，并把 `Q24_03PhysicalCrossScreen.Status` 明确为 `ready-for-host-replay` 或 `blocked-single-display`。
- 生产游戏选框仍是宿主内 `GameBrowserPanel`/`GameBrowserScrim`；共享 ComboBox Popup 继续使用模板定位、有限高度、内部滚动和动态主题资源。没有把游戏选框改成跨窗口 Popup，也没有引入固定屏幕坐标。
- 既有 R05 Popup 边界、R09 像素/DPI 模拟和主题 Popup 回归已经覆盖可在单屏/隔离 WPF 中复核的部分；本批只补当前机器真实拓扑和待双屏执行清单。

## 当前单屏事实

在当前 Windows 主机直接枚举 `System.Windows.Forms.Screen.AllScreens`，结果为：

| 设备 | 主屏 | Bounds | WorkArea |
| --- | --- | --- | --- |
| `\\.\DISPLAY1` | 是 | `0,0 1707×960` | `0,0 1707×912` |

因此本批不启动真实跨屏迁移，不生成第二屏 Popup、字体、焦点或资源释放的通过结论。`real-host-audit.ps1` 的 PowerShell 语法解析通过；单屏时运行器会保留 `blocked-single-display` 前置状态。

## 可复核证据

- Release 身份构建 Playnite `net462` / Tests `net472`：`0 errors`，仅 `MediaCenterView.xaml.cs:671` 的 2 条既有 `CS8602` warning。
- `R05PopupBoundaryBehaviorTests`：`2/2`。实际 STA WPF Window 中，生产 ComboBox Popup 在当前桌面工作区内，有限高度和内部滚动保持有效，边缘位置的选中项仍可见；生产游戏选框短宿主布局也通过。
- `R09PixelStrokeBehaviorTests`：`2/2`。生产资源在 `1.00/1.25/1.50/1.75/2.00` 逻辑尺度的离屏像素探针通过；这只是 DPI/像素模拟，不是物理跨屏证据。
- `R09ThemeSwitchBehaviorTests`：`1/1`。打开 Popup 的动态主题资源在浅/深主题切换后更新。
- `DiagnosticsEvidenceSourceTests` + `UiFinesseRound2ControlSourceTests`：`26/26`。锁定拓扑元数据字段、宿主内浮层边界、动态 Popup 资源和无独立窗口定位路径。
- `real-host-audit.ps1` 语法解析：通过；`Screen.AllScreens` 运行时枚举：单屏阻塞。

## 双屏可执行场景清单（获得第二个物理屏后）

仅在 `runner-metadata.json` 中 `DisplayCount >= 2` 时执行，并使用隔离 Playnite 数据目录：

1. 在显示器 A 启动隔离宿主，记录 A 的 `Bounds`、`WorkArea`、WPF `VisualTreeHelper.GetDpi` 和窗口位置；打开生产 Settings 或游戏选框筛选 ComboBox 的 Popup。
2. Popup 保持打开，将宿主窗口从 A 移到显示器 B，再等待布局稳定；同时保留迁移前/后窗口与 Popup 截图、Popup 物理矩形、Popup 所属 `PresentationSource` DPI、焦点控件 Automation 名称和字体/文本度量。
3. 验证 Popup 完整位于 B 的 `WorkArea`，边缘翻转与内部滚动仍可用；检查字体大小、字形清晰度、换行/裁剪和选中项可见性，不以逻辑 DIP 数值替代像素呈现。
4. 验证焦点仍在有效 ComboBox/选项或合理回退控件，键盘方向键/Enter/Esc 不落到 A 的残留窗口；切换浅/深主题后 Popup 资源、背景、阴影/低成本资源和文本仍属于当前宿主。
5. 关闭 Popup、回迁 A、再次打开/关闭并等待 Dispatcher 清理；记录 A/B 两侧无残留 Popup、宿主可继续交互，必要时保留 `PresentationSource.CurrentSources`/打开 Popup 枚举和资源快照。

若 `DisplayCount < 2`、无法取得真实 Playnite 可见宿主或无法保留前后呈现帧，应记录为前置阻塞，不以离屏截图、代理性能或 DPI 模拟替代。

## 边界与交付判断

本批只用生产源码、现有 STA WPF 夹具和显示器拓扑只读枚举；没有访问真实存档、媒体目录、云端或外发诊断，也没有绕过 ETW/系统跟踪权限。Demo 原目录不可用，继续沿用恢复生产基线。

R21-08 当前按“已满足，待环境验证”收口：单屏能力、DPI 模拟与双屏执行前置已有可复核事实；物理跨屏 Popup 位置、字体、焦点和资源释放仍待第二个物理显示器上的真实 Playnite/package-host 回放。真实 UIA/读屏、OS 输入/IME、跨屏呈现帧和宿主性能也不由本批替代验证。
