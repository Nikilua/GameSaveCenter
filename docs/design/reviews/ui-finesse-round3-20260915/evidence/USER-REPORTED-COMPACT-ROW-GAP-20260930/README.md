# 窄窗换行操作组间距复核

日期：2026-09-30。生产代码提交：`af915a2d637dc4901e1c81e2e2cdd097c958a002`（`加大窄窗折行操作间距`）。

## 修改与范围

- 将共享 `ResponsiveLayoutCoordinator.CompactActionRowGap` 从 `16` 调为 `20 DIP`。该协调值供存档历史动作、媒体待归类批量/次级动作、任务筛选预设及紧凑 shell 动作行使用；只在原有折行状态生效，宽态恢复原 margin。
- 本次继续保留游戏选框、现有滚动系统、命令和数据绑定。没有扩大到表单字段、开关或休眠的折叠 Demo 面板。

## 验证

- `ReportedWorkspaceLayoutBehaviorTests`：`46 passed / 0 failed / 0 skipped`。浅/深主题下存档历史、媒体批量动作和任务预设的两行间净距为 `20 DIP`；媒体次级动作在 `520–576 DIP` 折行时净距为 `20 DIP`，从 `577 DIP` 起恢复单行原 `4 DIP` margin；Shell 保持紧凑单列标题/动作布局。
- `Q14ToolbarAlignmentBehaviorTests`：`1/1`。任务预设在 `520–620 DIP` 实际测得 `20 DIP` 行间距，`620→660→620 DIP` 往返后恢复紧凑/宽态 margin。
- R00-06 关联复核：`MediaInboxGeometryTests 3/3`、`MediaWindowAnchorContractTests 10/10`，合计 `13/13`。四行/短窗页尾可达与锚点契约均继续通过。
- Q14 TRX 在测试成功后有 13 条、MediaWindowAnchor TRX 有 2 条 WPF `InvalidComObjectException` 清理输出；对应 xUnit 结果分别为 `1/1` 与 `10/10`、VSTest exit `0`。异常原因未知；另两份 TRX 没有该输出。
- Release solution build：XAML structural checks `24/24`，`0 warning / 0 error`；`python scripts/validate-source.py` 通过。提交后 RenderHarness clean-tree `render-qa OK`，报告的 `DpiScale=1.00` 明确为离屏逻辑 DIP。

## 程序集身份

所有程序集 `ProductVersion` 均为 `0.6.73+af915a2d637dc4901e1c81e2e2cdd097c958a002`。

| 程序集 | 目标 | SHA-256 | MVID |
| --- | --- | --- | --- |
| GameSaveCenter.Playnite.dll（Release） | net462 | `44979AD6B1F5929BC2F27C10BBCE81837CF816402E052B1BE5CBD30BAA00CC0F` | `d41038c0-f33a-4c06-9598-f6df5998d378` |
| GameSaveCenter.Playnite.Tests.dll（隔离 Release） | net472 | `902107B992FC404AEC7E928A345EA44F00F4D5D24039A7EFD1EC7AD84E25A945` | `d9e40ed9-26c6-44e8-9be0-fcca3bf8d371` |
| GameSaveCenter.RenderHarness.exe | net472 | `B9FEBD014C4EA2EA4BAB038756810945D3121AE1F529554B43DA7F49E71FCC8A` | `a015a9f2-4caa-418c-837b-3ca03669c929` |
| RenderHarness 加载的 GameSaveCenter.Playnite.dll | net472 | `B8311291D40F6BB0D5655A77608C5DB7181F245BBEB137B33077F200D8BAFDD1` | `1a14c126-4a47-41a9-a304-95f9b42c3716` |

## 归档文件

- [RenderHarness 完整报告](render-qa-report.txt)：含生产视图尺寸、滚动探针、布局时间及 `[GSC-GRID-DIAGNOSTIC]`，仓库绝对路径已替换为 `REPO`。
- [窄窗存档历史](screenshots/Save-1040x700-tab0.png)、[窄窗媒体待归类](screenshots/Media-1040x700-tab0.png)、[紧凑 shell](screenshots/Shell-1040x700.png)、[任务中心](screenshots/Task-1040x700.png)。
- [布局和文案行为 TRX](trx/ReportedWorkspaceLayoutBehaviorTests-af915a2d.trx)、[任务预设工具栏 TRX](trx/Q14ToolbarAlignmentBehaviorTests-af915a2d.trx)、[R00-06 几何 TRX](trx/MediaInboxGeometryTests-af915a2d.trx)、[R00-06 锚点 TRX](trx/MediaWindowAnchorContractTests-af915a2d.trx)。TRX 中机器名、账户名和仓库绝对路径已脱敏，测试结果与读数保留。

验证来自隔离 STA WPF、合成数据和 OffscreenRenderHarness。TRX 中 `1.5×1.5` 是测试进程的合成 WPF 布局尺度，截图报告的 `1.00` 是离屏逻辑 DIP；两者都不代表物理 DPI。没有安装或启动真实 Playnite，也没有检查用户安装 DLL 或最终屏幕帧。Media Inbox 用户报告的真实宿主滚动后表头/数据行偏移仍需同一安全宿主进程的前后 `[GSC-GRID-DIAGNOSTIC]` 日志。
