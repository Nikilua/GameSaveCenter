# 用户截图布局问题复核（2026-09-23）

## 复核身份与修正

当前 `main` 源码身份：`3a1dadd80bae6152dce9c3f684d5d2c745f02bc8`。四类用户截图问题分别收口为独立提交：

- `da91bd68`：窗口化存档历史区动作行紧凑换行；媒体待归类内部滚动条限制在表格框内。
- `a4bac32f`：媒体批量动作统一为紧凑高度，并补实际行框/表格边界行为检查。
- `9f3d7ab9`：任务失败状态留在状态单元格内，不把整行套错误边框。
- `3a1dadd8`：设置页搜索框移到标题列并按可用宽度收缩；标题图标、搜索、恢复默认卡片和路径编辑按钮之间的锚点有双主题行为断言。

`ReportedWorkspaceLayoutBehaviorTests` 的四个用例各跑 Light/Dark 理论参数，合计 `8/8`。结果来自合成数据、生产 WPF 视图和隔离 STA 窗口，尺寸均为逻辑 DIP：

| 用户截图 | 当前观测 | 行为结果 |
| --- | --- | --- |
| 媒体中心 / 待归类 | 清空、重置列宽、归类所选按钮均 `36 DIP`；模式选择框 `36 DIP`；中心偏差最大 `0.33 DIP`；表格框 `360 DIP` | 表格与 footer 无重叠；内部滚动条被表格框包含，`74/74 DIP` 内部滚动结束后页级偏移为 `107.33/107.33 DIP`，未滚到 footer 按钮区域 |
| 任务中心 | 2,400 条合成行，虚拟化开启；初始/回收容器 `13/14`；跨滚动检查 7 个失败状态行 | 失败态仍只显示在状态胶囊中；错误呈现不匹配 `0`，行框位置误差 `0 DIP`，行高差 `0 DIP` |
| 存档中心 / 历史版本 | `1040×700` 窗口化窗口；摘要 `1010×442 DIP`；操作行 `987.33×36 DIP`；宽窗口行号 `0`、恢复窗口化后行号 `1` | 内容到动作间距 `10 DIP`、尾部空白 `9.33 DIP`；动作仅占两行紧凑工具栏，不再撑起大块空区；选择和右侧检查器保持可见 |
| 设置页 | Light/Dark 标题图标与标题上沿差 `11.33 DIP`；搜索宽屏 `520×36 DIP`、窄窗口宽 `408 DIP` | 搜索框与标题、恢复默认卡片左边差均 `0 DIP`；恢复卡片与搜索框间距 `14 DIP`；窄布局无溢出；路径编辑按钮与组合框均 `36 DIP`、中心/高度离散均 `0 DIP` |

设置页宽屏搜索框的 `520 DIP` 是上限，布局代码按宿主可用宽度缩小；测试另测 408 DIP 窄布局没有右侧溢出。原有命令、Binding、Playnite 设置保存/取消草稿语义未改。

## 构建、测试与视觉检查

- 隔离 Release 构建：Playnite `net462`，XAML `24/24`，`0 errors`；保留两条现有 nullable `CS8602` warning，均在 `MediaCenterView.xaml.cs:703`。
- 当前身份的 `ReportedWorkspaceLayoutBehaviorTests`：`8/8`，Light/Dark 各覆盖四种页面行为。R18-04 专测 `1/1`，四个关联分页/锚点/几何/选择行为类 `23/23`；另外 Foundation `9/9`、Diagnostics `11/11`、审计源 `6/6`、布局回归 `20 passed / 11 skipped`、设置搜索/验证/草稿相关测试均按独立 testhost 通过。本批列明的全部隔离类共 `87 passed / 13 skipped / 0 failed`；跳过项只计入 skipped。
- `scripts/validate-source.py`、`scripts/check-xaml.ps1` 通过；WPF 静态审查 `0 errors / 30 warnings / 177 info`。渲染总门禁仍有 `40 PROBLEM`：Overview 最近访问列表 `20`、Save History `8`、Task Grid `4`、其他 SettingsLayout 尺寸断点 `8`。这四个全局问题组不能被本次报告问题对应的定向几何用例替代或写成全局门禁通过。
- RenderHarness 当前审计为 `168` snapshots、`118` warnings、`0` Fidelity、`0` failed routes；实际保留 `7 HIGH` 父子滚动冲突和 `4 MEDIUM` 工具栏纵向扩展。证据索引可追溯性为 `20/20`，并非风险清零。
- 四张代表截图保存在本目录：`Settings-2048x1152-tab0.png`、`Media-2048x1152-tab0.png`、`Task-2048x1152.png`、`Save-1040x700-tab0.png`。

## 证据边界与后续

截图由隔离 WPF 离屏渲染器生成，包含合成数据；`DpiScale=1.0` 和测试中的 DIP 不是物理屏幕 DPI。未验证当前 Playnite package-host 的最终呈现、Windows UIA/读屏、OS IME、真实鼠标滚轮、DWM presented frame、物理跨屏、ETW 或宿主性能。R18-04 的 WPF testhost 结束时另记录 `TextServicesHost.OnUnregisterTextStore InvalidComObjectException` 清理输出 6 次；TRX 为 `1/1` 且退出码 `0`，根因未知，按清理噪声记录。

本次没有读取或改写真实存档、媒体、用户云端或诊断数据。Demo 原目录不可用，布局复核沿用恢复的生产基线。R02-06 主机菜单和 R23-05 系统级呈现仍保留实际环境边界。

## 2026-09-23 追加：设置窗口截图差异

用户随后提供的设置窗口截图仍显示顶部图标落到搜索行附近、搜索输入框从标题左边界向右偏移。该附件属于用户当前宿主观察，不是仓库生成的离屏图，也没有复制进仓库 artifacts。当前生产 XAML 已在 `3a1dadd8` 将搜索框明确放到标题列并左对齐；现有 `ReportedWorkspaceLayoutBehaviorTests` 以生产设置视图验证 Light/Dark `8/8`，包含图标/标题顶边、搜索/标题左边界、恢复默认卡片和路径编辑组合框/按钮几何。这些 STA WPF 几何结果不能说明用户当前安装包身份，也不证明实际 Playnite host 的 DPI/尺寸行为。

本次 R00/R01 freshness 复核在 main source HEAD 62b17b0c：14 条记录的 sourcePaths 均无变更，package identity 仍未提供。随后在当前生产设置视图补 Loaded/SizeChanged 几何行为复核，两主题 2/2；不手动调用布局方法，搜索左差 0 DIP，居中负例为 287.33 DIP，窄窗口保存状态行回到正常位置，路径组合框和四按钮均为 36 DIP。详细数据见[设置截图行为复核](settings-header-responsive-20260923/README.md)。

### 2026-09-23 设置顶栏与恢复默认控件补测

Light/Dark 再次通过 2/2。图标到标题横向间距为 12 DIP，顶部恢复默认 ComboBox 与两个按钮全部为 36 DIP，中心差 0 DIP；完整原始几何和单独 TRX 见 [settings-header-controls-geometry.trx](settings-header-responsive-20260923/settings-header-controls-geometry.trx)。这些结果来自当前源代码的隔离 STA WPF 窗口，不是用户当前 Playnite host/package 的呈现结果。

用户截图仍没有包身份，之前隔离 Playnite host 未能正常加载扩展（CEF `platform_channel` `0x5`），所以截图与当前受控行为的差异尚未在正常宿主解释。下一项核对当前 package identity/正常 host；如果宿主边界继续阻塞，保留待验状态并推进独立 Q/R 小批量，不把测试通过写成用户当前窗口已解决。

### 2026-09-23 main Release 包身份与几何复验

当前 main 8e4f3194 的完整隔离 Release 解决方案构建成功：XAML 24/24、0 errors，保留两条既有 MediaCenterView.xaml.cs:703 CS8602。相同构建产物上的设置页几何测试 Light/Dark 2/2；图标/标题 12 DIP，搜索/标题左差 0 DIP，顶部恢复默认控件全为 36 DIP、中心差 0。当前构建的原始 TRX 和测量见 [设置页行为证据](settings-header-responsive-20260923/settings-header-controls-main-8e4f3194.trx)；离屏代表图为 [Settings-2048x1152-tab0.png](user-reported-layout-20260923/Settings-2048x1152-tab0.png)。

为了给用户提供可核对的当前构建，新包已生成在 [GameSaveCenter-0.6.73-main-8e4f3194.pext](../../../../artifacts/GameSaveCenter-0.6.73-main-8e4f3194.pext)，程序集身份 0.6.73+8e4f3194227afb28640754f12ab0889cb8bb71ce，SHA-256 B6602DB38D98CDE9B11B8B0B414F43337B00AA021A11C001BBCB542912D9B3B0。它仅已打包，未安装或在真实 Playnite host 打开；原有同版本包保留。

当前受控 source/render 与用户截图不一致，所以还需要把截图对应的当前插件 identity 与新包对照。用户当前包、正常 Playnite 父容器、物理 DPI、真实屏幕仍未验，不能宣称已经在用户环境修复。隔离 Playnite host 的 CEF platform_channel 0x5 仍阻止正常宿主复核。
### 2026-09-23 R00/R01 freshness 复核更新

本次 freshness 采样 main HEAD 13442aa4，14 条记录均无需重跑且源码路径命中为 0；package identity 未提供，不能据此关闭真实宿主验证。

### 2026-09-24 当前 main 窗口化设置截图复核

用户补充的窗口化截图再次指出顶部图标、搜索输入和按钮偏位。当前 main 已有 `3a1dadd8` 的共享设置头部锚点修正；本批没有重复修改生产布局。预提交身份 `f11e27dd` 的 `ReportedWorkspaceLayoutBehaviorTests` 全类在完整 Release 构建上 `8/8` 通过，涵盖媒体、任务、存档和设置四类反馈的 Light/Dark 行为。设置测量为图标/标题顶边差 `11.33 DIP`、横向间距 `12 DIP`、搜索框/标题左差 `0 DIP`、搜索框/图标顶边差 `72.67 DIP`、居中错位负例 `287.33 DIP`；恢复默认控件和路径编辑 ComboBox/按钮均 `36 DIP` 且中心线差 `0`。紧凑窗口搜索宽 `392 DIP`、无右溢出。

本机扩展目录中记录的 DLL `ProductVersion=0.6.73+7a4ba2a94da870c832e59f3ee025f9e34325d175`，SHA-256 `5E02A462F1EDA26D706B550F8B428612CB13F787078A342CF3CFE4850AD50E22`，身份早于 `3a1dadd8`；它可能解释截图差异，但不能证明截图捕获时 Playnite 进程载入了该文件。测试是隔离 STA WPF、逻辑 DIP，不等于真实 host。隔离 Playnite 仍受 CEF `platform_channel 0x5` 阻挡，因此问题保持“源码行为已验证、当前用户宿主未验”，不写成实际窗口已修复。原始当前 main TRX：[USER-REPORTED-LAYOUT-MAIN-F11E27DD.trx](USER-REPORTED-LAYOUT-MAIN-F11E27DD.trx)。
