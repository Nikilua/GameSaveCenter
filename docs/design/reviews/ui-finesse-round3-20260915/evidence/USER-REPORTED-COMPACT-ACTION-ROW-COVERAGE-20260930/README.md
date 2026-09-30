# 跨页窄窗操作行间距补测（2026-09-30）

## 本批处理

按当前生产 WrapPanel 与 WPF 实际换行核查用户提出的跨页行距问题。维护页的首次检查、诊断操作、目录与日志、游戏来源诊断四个多按钮组现在在紧凑布局使用共享 `WrapPanelRowGapController`；切回宽布局时还原各控件原始边距。Trainer 的待确认导入条在很宽的窗口里仍保持两行，因此保持 20 DIP 行距，不仅按窗口宽度决定是否移除间距。

既有存档历史、媒体收件箱/筛选预设、任务筛选预设行也一并复跑，确认新增覆盖没有改变它们的折行与恢复行为。没有修改命令、绑定、选择、分页/滚动、取消/错误或恢复安全语义。测试使用合成数据和假命令；没有写真实业务数据。

## 构建与行为结果

- `scripts/build.ps1 -Configuration Release -SkipTests`：成功，0 warnings、0 errors。
- `scripts/check-xaml.ps1`：24/24 XAML 文件通过。
- `python scripts/validate-source.py`：通过。
- `git diff --check`：通过。
- `ReportedWorkspaceLayoutBehaviorTests` 的 4 个相关行为方法、Light/Dark 共 8/8 通过；完整 TRX 为 [compact-row-gap.trx](compact-row-gap.trx)。
- 维护诊断操作栏：620×720 DIP 下 2 行、净行距 20 DIP，WrapPanel 实测 564×112 DIP；1400 DIP 宽时 1 行且原始 8 DIP 子项边距恢复；回到 620 DIP 再次得到 2 行与 20 DIP 行距。Light/Dark 数值一致。
- Trainer 待确认导入栏：620、1400、2200 DIP 窗口宽度下均为 2 行、20 DIP 行距；2200 DIP 仍折行，所以该区域持续使用 20 DIP。Light/Dark 数值一致。
- 既有行距回归：存档历史与 Inbox 在紧凑宽度 2 行/20 DIP、宽态 1 行/原边距；媒体筛选预设在 520–700 DIP 为 2 行/20 DIP，720 DIP 起单行并恢复；任务预设在 620 DIP 两行/20 DIP、660 DIP 单行并在往返缩放后恢复。

## 运行身份

测试由 Release WPF STA testhost 运行，使用逻辑 DIP 测量真实生产 View 和 WrapPanel。构建的 `AssemblyInformationalVersion` 是 `0.6.73+85ec95cb315f7f7e09e75b0cedafd98eea447c85`；该值反映构建时 Git HEAD。源码来自该 HEAD 上的工作树修改，随后以相同文件内容提交为 `bffc1ae1` 并推送 `main`，以下源码 SHA-256 可用于复核实际编译输入：

| 文件 | SHA-256 |
|---|---|
| `src/GameSaveCenter.Playnite/Views/MaintenanceView.xaml` | `2273999BE114E925F9B6A473159A982FD34BE0D940BBE363445F94FDFA99FD2E` |
| `src/GameSaveCenter.Playnite/Views/MaintenanceView.xaml.cs` | `264CC3F4AAD92F44D4C32BCCCD83BD3D07FBAFB7C6B09CC47AC74042F96AEF9B` |
| `src/GameSaveCenter.Playnite/Views/TrainerCenterView.xaml` | `263ABD589F56D377F9E396C12FF3354401DFC13422C930C9ACF4445ECAB376B4` |
| `src/GameSaveCenter.Playnite/Views/TrainerCenterView.xaml.cs` | `327DAB6CB94D7AB3EE6E485AF0BD2ECFE6CC0C836DDDFF4607C1B49BC928192E` |
| `tests/GameSaveCenter.Playnite.Tests/ReportedWorkspaceLayoutBehaviorTests.cs` | `A6CB76D391D9B59159D9D86B18530E1019A84FCCB4AF1637706C7282F967B21B` |

| 程序集 | SHA-256 | MVID |
|---|---|---|
| `GameSaveCenter.Playnite.dll`（Release/net462） | `4DCE634E75952C8FC1AB3378E197FDAC84DECE4298B0E1064EA15777BF7BC383` | `1778a7e7-b582-43c1-bffb-ff1629ca5b08` |
| `GameSaveCenter.Playnite.Tests.dll`（Release/net472） | `5301BECE92EBB7645108819958F90D9C394ECCCDBEDFD0ADDF0A13EBB73CA578` | `e51c38fb-160e-4d04-adb8-0fd6aa77bf43` |

## 验证边界与后续

几何来自本机隔离 WPF testhost 与合成 ViewModel，不是 Playnite 宿主截图、实机物理 DPI、输入体验或最终呈现验证；本轮没有访问真实媒体/存档。当前仍需继续盘点其他可见操作组，尤其是随数据模板生成的行内按钮，并审查有数据时仍显示的静态辅助文字。Media Inbox 滚动后列头与首行偏移仍待用户授权的安全 Playnite 会话中采集滚动前后同坐标系 `[GSC-GRID-DIAGNOSTIC]`，不能由本证据替代。
