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

本次 R00/R01 freshness 复核截至 main `8846d712`：14 条记录的 sourcePaths 均无变更，但没有 package identity。此前隔离 Playnite host 未能正常加载扩展（CEF `platform_channel` `0x5`），所以当前可见用户截图与现有受控图的差异还没有在正常宿主中解释。下一项先以截图尺寸和逻辑 DPI 核验当前生产设置视图及 `SizeChanged` 响应；如果当前 checkout 在该条件下错位，则修共享布局并增加行为/负例验证；否则保留安装包/宿主身份待核边界，不宣称已解决。
