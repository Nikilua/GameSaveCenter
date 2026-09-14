# Q02-06 当前生产路径视觉证据

采集日期：2026-09-15（Asia/Shanghai）。提交：`82cf066`；`RenderHarness audit` 在干净工作树上运行，`DpiScale=1.00`，截图为逻辑 DIP，不替代真实宿主物理 DPI、Tooltip 触发或复制操作。

## 证据

- [1440×900 标准窗口](path-20260915/save-center-path-standard-1440x900.png)：生产 `SaveCenterView` 的“路径与校验”页，候选表 8 行均可读到 `D:\Games\Baldur's Gate 3\Save\N\SlotN`，右侧判断依据同时保留完整路径。
- [1040×700 窄窗口](path-20260915/save-center-path-narrow-1040x700.png)：窄窗口仍保留路径列和候选详情入口；首行路径未被表格边界裁掉，表格内部滚动仍可用。
- 运行时视觉树的标准/窄窗口路径 TextBlock 均为 `ActualWidth=260 DIP`、`DesiredWidth=260 DIP`；标准窗口详情面板路径 TextBlock 为 `ActualWidth=321.33 DIP`，完整路径文本保留为 `D:\Games\Baldur's Gate 3\Save\1\Slot1`。

## 结论与边界

- 共享 `GscPathText`/`SavePathText` 已在生产路径表格和详情入口生效；源码中的 `CharacterEllipsis` 与原值 `Path` Tooltip 契约见 [`Q02-PATH-STYLE-COVERAGE.md`](Q02-PATH-STYLE-COVERAGE.md)。当前截图签收 Q02-06 的离屏视觉列。
- 真实 Playnite 宿主中的 Tooltip 展开、复制原值、中文/更长路径截断、字体安装差异和物理 DPI 仍未验证，因此 Q02-06 的自动与宿主列不改写为通过。

