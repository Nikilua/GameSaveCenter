# L39 真实 Playnite 表格滚动回放记录

日期：2026-09-09
代码提交：`a9b8bce`
构建身份：`0.6.73+a9b8bcec0c05f7d548d8160119b2b29e9698b1ab`
证据目录：[`artifacts/ui-host-audit-isolated-l39`](../../artifacts/ui-host-audit-isolated-l39)

## 宿主与操作范围

- 使用隔离 Playnite 数据目录启动真实 `D:\software\Playnite\Playnite.DesktopApp.exe`，生产扩展来源为 `EmbeddedPlaynite`；未修改用户 FusionX 文件、Playnite 全局样式或正常用户数据目录。
- Dashboard 为 `1365.33×868 DIP`，DPI 为 `1.5`；媒体表加载 `400` 条，任务表保留真实分页的 `50` 条。
- 每张表均采样 `47` 次：初始、顶部、底部、顶部/底部往返 `20` 次、水平端点（如可用）、尾项选择和恢复原状态。滚动器均为实际负责行滚动的 `ScrollViewer`，`IScrollInfo=ScrollContentPresenter|DataGridRowsPresenter`，`CanContentScroll=True`、`ScrollUnit=Item`。
- 任务表的开发审计不再调用加载更多：该宿主分页命令会替换当前 50 条而继续报告 `HasMore=true`，继续调用会测试分页状态而不是表格滚动。该限制只属于开发审计，不改变生产命令。

## 结果

| 表格 | 底部样本 | 底部偏移/范围 | 实际内容视口 | 尾项位置 | 尾项内容 | 异常样本 |
| --- | ---: | --- | --- | --- | --- | ---: |
| `MediaInboxGrid` | 21 | `394 / 394` | `0,36,604×279.33`；水平条 `0,315.33,604×12` | index `399`，`y=256..300` | `5/5/5`（cells/visual/text） | 0 |
| `TaskGrid` | 21 | `40 / 40` | `0,36,637.33×456`；水平条 `0,492,637.33×12` | index `49`，`y=432..476` | `6/6/6`（cells/visual/text） | 0 |

`presenter.Bottom` 分别为 `315.33` 和 `492`，因此两张表的最后加载行都在实际内容视口内，且不与水平滚动条重叠。尾项选择样本中两张表均保持选中状态，视觉单元格数和文字单元格数仍完整；`clippedCells=1` 是选中状态下 DataGridCell 自身的 ClipToBounds，不是文字消失。

按 JSON 记录重新计算的异常计数（空正文、大块首行间隙、末端水平条覆盖、选中行内容缺失、底部尾项不完整）均为 `0`。顶部和中间位置出现半行时没有计入问题 A，因为该行并非已加载集合末项；末端样本专门用 `lastRowComplete` 与 Presenter DIP 矩形判定。

## 结论与剩余边界

这次真实 FusionX 宿主的程序化回放没有复现视频中的持续空白、行框/文字分离或末行裁剪，因此当前证据不能支持再修改 DataGrid 模板、滚动单位、页面 Margin 或虚拟化参数。生产代码本轮没有加入补偿像素、刷新、定时布局、强制回顶或关闭虚拟化的行为修复；提交内容是诊断尾项完整性字段和开发审计覆盖任务表。

这不是视频式人工验收：当前 Computer Use 原生应用清单为 `apps: []`，本轮没有物理拖动滑块、滚轮/PageUp/PageDown/Ctrl+End、缩放主题矩阵或录屏。因此问题 B 仍标记为“宿主人工待验收”，不能写成视频问题已解决。若人工复现，需把视频时刻与同一诊断行中的 `Items.Count`、`CollectionChanged`、代际、offset/extent、Presenter 矩形、首末行、cell visual/text/clip 和锚点状态对齐，再决定是集合 Reset、偏移范围、容器几何还是单元格回收问题。
