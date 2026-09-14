# UI 精修基线（2026-09-13）

## 基线身份与证据边界

- P00 基线提交：`5b34b35`（`main`，已推送）；当前共享底座改动将在本阶段提交。
- 插件版本：`0.6.73`，版本入口为 `src/GameSaveCenter.Playnite/extension.yaml` 与 `Directory.Build.props`。
- 工作区：仅有用户现有未跟踪文件 `src.zip`，本轮不删除、不提交。
- 静态基线：`scripts/validate-source.py`、`scripts/check-xaml.ps1`（24 个 XAML）和 `git diff --check` 通过。
- WPF 静态基线：0 error、22 warning、175 info。warning 主要是既有有限滚动审查提示；info 主要来自资源字典硬编码颜色和参考控件。
- 离屏基线：`.tmp/ui-prompt-qa-final2-20260913/render-qa-report.txt` 的报告为旧提交 `108ae0f`，不能直接充当本 HEAD 的新证据；P00 另以 `.tmp/ui-finesse-probe-20260913-dark/` 与 `...-light/` 保存双主题校对夹具。
- 真实宿主边界：`artifacts/ui-host-audit-current-20260913/` 有受控窗口证据，但 `MainWindowHandle=0`、未生成 `summary.json`；不得写成真实嵌入 Dashboard、物理 DPI、键盘/滚轮或宿主主题已验收。

## 生产资源生效链

| 类别 | 唯一共享入口 | 真实消费 | 当前覆盖/风险 |
| --- | --- | --- | --- |
| 字体与字阶 | `Themes/Typography.xaml` | `DesignTokens.xaml` 合并后由六页、壳和设置页的 DynamicResource/隐式样式消费 | P01 已实测本机命中：CJK→Noto Sans SC，Latin/数字/箭头→Segoe UI Variable Text；Inter 未安装，𠮷 在显式链未命中 |
| 颜色、间距、材质 | `Themes/DesignTokens.xaml` + `AdaptiveThemePaletteFactory` | `ApplyRuntimeThemeResources` 写入 Dashboard、Shell 和各 workspace 的资源字典 | P02 已按最终合成扩展普通文字、强调色、状态色和表面检查；静态 Demo core 浅色状态色同步收敛 |
| WPF-UI 控件 | `Themes/WpfUiBase.xaml`、`Themes/WpfUiProduction.xaml` | `ui:Button`、Toggle、TextBox、ComboBox、ScrollBar 和共享焦点样式 | 生产按钮/图标按钮已有五态入口；需继续验证按压恢复、禁用 Tooltip 和各主题 |
| 页面/表格 | `Themes/Redesign.xaml` | 六页 DataGrid/ListBox/Tab/Inspector 通过显式样式消费 | 表格保持有限视口、Recycling、行列虚拟化；页面局部字号和紧凑断点仍需同夹具对照 |
| 图标 | `Controls/ThemeAwareIcon.cs`、`Themes/GscIconPack.xaml`、`GscIconButtonPack.xaml` | 壳、状态、页面和图标按钮 | 颜色由主题资源提供；不能退回 emoji/字形图标 |
| 动效 | `Themes/MotionTokens.xaml` + `Infrastructure/GscMotion.cs` | 按钮/Toggle XAML 与 Dashboard/Shell/Overview/设置 C# | P04 已统一为 XAML 权威 120/100/220/300ms；C# 支持宿主资源覆盖与无资源确定回退 |
| 反馈 | `WorkspaceStatePresenter`、内置 Toast/Dialog/Inspector | 六页和设置的真实状态字段与命令 | 状态、失败、取消、复制诊断和页面滚动已有 U12 基线；本轮只补证据和共享缺陷 |

## 代表性资源追踪

1. `OverviewView` 的 `TextBlock` 默认字体来自 `DesignTokens` → `Typography` 的隐式样式；页面标题显式使用 `GscTypographyPageTitle`/Demo 别名，运行时颜色来自 `GscPrimaryTextBrush`。
2. `AcrylicProductionShellView` 的顶栏刷新按钮使用 `GscIconOnlyToolbarButton`，其基础模板是 `WpfUiProduction.xaml` 中的 `GscIconOnlyButtonBase` → `GscWpfUiButton`，Tooltip/AutomationProperties 保留。
3. `TaskCenterView` 的 `TaskGrid` 使用 `GscRedesignWorkspaceDataGrid`，行高来自 `GscTableRowHeight`（52 DIP），表头来自 `GscTableHeaderHeight`（42 DIP），滚动为有限视口 + Item scrolling。
4. 设置页先调用 `AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources`，再调用 `ApplySettingsMaterialResources`；这是独立设置窗口中主题资源不漂移的实际入口。
5. C# 动效调用集中在 `GscMotion` 和 Dashboard/Shell/Overview/设置的有限根元素；未发现列表行逐项动画或大面积 BlurEffect。

## 初始问题台账

| 分级 | 任务 | 页面/共享入口 | 事实 | 验收条件 |
| --- | --- | --- | --- | --- |
| 可读性 | P01-01/P01-04 | Typography | 字体命中已在夹具报告记录；Caption 已移除 0.65 二次透明 | 目标机/多机字体分发和罕见字 fallback 仍需真实验收；普通小字按最终合成测量 |
| 可读性 | P02-02 | AdaptiveThemePaletteContrastGuard | 已检查 Secondary/Muted 4.5、OnAccent 4.5、四类状态色 3.0，并区分装饰性表面线 | 普通辅助文字、选中、按钮、placeholder、错误、焦点的最终配对值需随宿主复杂背景补测 |
| 交互一致性 | P03-01 | WpfUiProduction | 生产共享模板已覆盖状态，但需要五态及卸载/失焦恢复证据 | 不改布局，按压 0.97 后回 1，禁用 Tooltip 和命令次数正确 |
| 体感 | P04-01 | MotionTokens/GscMotion | Normal/Slow 双来源 | C# 与 XAML 由同一可验证策略取值，缺字典和关闭动画安全 |
| 性能 | P00-03/P06/P10 | RenderHarness/真实宿主 | 现有帧探针是代理；真实宿主窗口不可枚举 | 记录环境、原始样本、p50/p95/max；不能把代理数据写成屏幕帧率达标 |

## 固定校对样本

审计夹具使用任务包第 4.1 节的中英、数字、路径、诊断和状态样本，数据为合成内容，不注入真实业务数据：

`存档中心  Save Center`、`媒体归类  Media Inbox`、`备份完成，云端校验失败。可以稍后重试。`、`赛博朋克 2077：往日之影 / Cyberpunk 2077: Phantom Liberty`、`AgjQy  Il1 O0  0123456789  1,024 / 99,999  9% → 100%`、`D:\Games\中文目录\Cyberpunk 2077\一个非常长的备份文件名.zip`、`任务部分完成  FLING_DOWNLOAD_FORBIDDEN  ✓ × ⚠`、`罕见字：𠮷；外文标题：NieR Replicant / Pokémon / 龍が如く`。

## P00 夹具证据

- RenderHarness 命令：`dotnet run --project tests/GameSaveCenter.RenderHarness/GameSaveCenter.RenderHarness.csproj -c Release --no-build -- finesseprobe <output> <dark|light>`。
- Dark 与 Light 均在 1120×980 DIP 完成：`finesse-fixture OK`、4 行 `DataGrid`、22 个按钮、`CaptionOpacity: 1`；截图和报告分别位于 `.tmp/ui-finesse-probe-20260913-dark/`、`.tmp/ui-finesse-probe-20260913-light/`。
- 夹具只挂在 `UiFrameworkProbeView.UserControl.Resources`，不写入 Playnite 全局资源；截图中的混排/路径/错误码/状态数据均为合成校对数据。正常、Hover、Pressed、Disabled、Keyboard Focus 由共享模板覆盖，但 Pressed/Focus 的物理行为仍需行为探针或真实宿主复核。

## P01 / P02 / P04 共享底座证据

- 字体报告同时记录了 `FontChain` 和五个 Unicode 样本的实际解析：CJK、Latin、数字、箭头命中，`U+20BB7` 罕见字在显式链未命中；这不是跨机器分发成功证明。
- `finesseprobe` 的 ContrastGuard 输出每主题 11 项配对。Light：Primary 19.433、Secondary 9.626、Muted 4.854、OnAccent 5.086、Info 4.754、Success 4.078、Warning 4.371、Error 4.513；Dark：Primary 18.201、Secondary 10.234、Muted 6.327、OnAccent 6.964、Info 7.335、Success 10.254、Warning 9.693、Error 6.296；装饰表面/控件检查也通过。
- P04 的 `UiFinesseFoundationTests` 验证 120/100/220/300ms 语义和宿主资源覆盖；RenderHarness 全量报告 `render-qa OK`，rapid-toggle 代理探针为 `settled=True`。该代理测量不替代物理屏幕帧时间。

## P03 / P05～P11 当前收口证据（2026-09-13）

- 当前代码与报告基线为 `6c3c238b8ba0c7ce3a010e4234e18cfc34f10ee1`。`tests/GameSaveCenter.RenderHarness/bin/Release/net472/GameSaveCenter.RenderHarness.exe` 已在当前提交重新执行 `statefixtures`、`gridprobe`、`thumbnailprobe`；三项均返回 `OK`，报告中的 `WorkingTreeClean=False` 仅来自根目录用户文件 `src.zip`。
- 历史 `.tmp` 报告在阶段清理时已删除；其持久化替代证据集中在当前 [Q13–Q25 证据索引](../ui-finesse-round2-20260913/evidence/Q13-Q25-INDEX.md) 与 [Q04–Q12 受控证据](../ui-finesse-round2-20260913/evidence/Q04-Q12-INDEX.md)。这些索引保留各类离屏几何、状态、网格、缩略图和壳层结果的范围与边界，不把它们升级为真实宿主帧率或物理 DPI。

### 真实宿主与最终门禁边界

- P03、P05、P07～P09 的“代码完成待验收”代表共享实现和受控证据已闭环，但真实 Pressed/Focus、IME、Popup、键盘/读屏、用户主题、物理 DPI、真实备份/归类/忽略操作仍需要可见 Playnite 宿主。
- P08-03、P10-01、P10-02、P10-04、P11-03 仍是外部阻塞。最近自动宿主审计的 `MainWindowHandle=0`、无 `summary.json`，PNG 为 `DedicatedAuditWindow` 且 `DashboardWasAlreadyHostedByPlaynite=false`；不以专用窗口替代嵌入 Dashboard。
- P11-03 的安全打包链必须看到 clean working tree；当前只剩用户未跟踪 `src.zip`，本轮不删除、不移动、不提交它。代码/测试/离屏证据完成不等同新包可签收。
- 最终一键链（`GameSaveCenter-一键构建安装运行.cmd`）已在文档改动和 `src.zip` 存在时执行：XAML `24/24`、Release `0 warning/0 error`、Core `76/76`、Worker `311/311`、Playnite `455/512`（57 skip、0 fail）；随后按 `scripts/package.ps1` 的 clean-tree 安全门禁停止，详见 `artifacts/one-click-install.log`。没有伪造新包、安装或运行成功。
- 文档提交后已按用户要求重试一次：一键链仍为 XAML `24/24`、Release `0 warning/0 error`、Core `76/76`、Worker `311/311`、Playnite `455/512`（57 skip、0 fail），并且只剩 `?? src.zip`，随后仍在打包前停止。当前宿主性能边界见 [Q25 宿主性能边界记录](../ui-finesse-round2-20260913/evidence/q13-q25/Q25-HOST-PERFORMANCE-BOUNDARY-20260914.txt)，没有新的 `summary.json` 或嵌入截图。
