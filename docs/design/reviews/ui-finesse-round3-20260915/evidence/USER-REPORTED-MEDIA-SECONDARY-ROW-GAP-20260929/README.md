# 媒体次级动作窄窗行距与 R00-06 当前复核

日期：2026-09-29。源码提交：`ce12c68da11069da21cf014a2f8a6f1840fa4603`（`补齐媒体动作行距`）。

## 修复与实测断点

- `MediaInboxSecondaryActions` 在 Light/Dark 两主题下，`520–576 DIP` 为两行，原净距 `4 DIP`；`577 DIP` 起单行。复用 `WrapPanelRowGapController`，仅 `<577 DIP` 时净距 `8 DIP`，单行后还原作者 `4 DIP` 边距。
- 任务中心筛选预设 `<657 DIP`、媒体筛选预设 `<720 DIP` 的 `8 DIP` 动态行距仍通过回归；宽态 margin 恢复不变。
- 不改媒体命令、模式、游戏目标、筛选/选择行为、虚拟化或滚动模型。

## 验证

- 当前提交隔离 Release solution 构建成功：XAML `24/24`，`0 warning / 0 error`。
- 三个 WPF 类在独立 VSTest host 运行：用户报告紧凑行距 `2/2`、`MediaInboxGeometryTests 3/3`、`MediaWindowAnchorContractTests 10/10`，合计 `15/15`，失败 `0`、跳过 `0`。测试 DLL SHA-256：`F57D233919490641CDC721E85816181479CE4AA46D8ED736B3ED55A8225C0F5F`。
- 组合布局用例在浅/深主题扫描次级动作 `520/540/560/570/575/576/577/578/579/580/620/700/720/800 DIP`；小于 `577 DIP` 的真实生产 `WrapPanel` 量得 `8 DIP`，`577 DIP` 及以上保留单行与原 `4 DIP` child margin。包含 `576→577→576 DIP` 往返断言。
- R00-06 的短窗测试为 `700×600 DIP` 四行下限/页级 fallback，以及 `520×600 DIP` footer 换行时 `VerticalScrollBarVisibility=Auto`、页级 `ScrollableHeight>0`；几何、footer 可达与分页锚点契约均通过。
- 最初的快编译测试曾引用默认 `bin` 下的旧插件 DLL；该结果未计入。改用隔离完整 Release 输出重新编译插件和测试后逐类重跑，归档的 TRX 均对应本节源码提交。

## R00/R01 新鲜度与验收边界

- 本次改动触及 `MediaCenterView.xaml.cs`，故更新 R00-06 的 freshness source identity 到 `ce12c68da11069da21cf014a2f8a6f1840fa4603`；R00/R01 报告为 `14 FRESH / 0 STALE`，freshness 自测通过。package commit 未提供。
- 复核为合成数据、生产 WPF 控件、隔离 STA 窗口的逻辑 DIP。没有启动 Playnite、切换物理 DPI、产生 OS 输入或检查最终屏幕呈现；完整 RenderHarness 矩阵未重跑。
- 用户报告的真实 Media Inbox 滚动空白依旧待安全 Playnite 会话：先核对当前 DLL 身份/主题/窗口 DIP/DPI，再保存同一 `MediaInboxGrid` 的滚动前后 `[GSC-GRID-DIAGNOSTIC]`；没有据本次行距测试宣称其已修复。

## TRX

- `reported-compact-rows-postcommit.trx`
- `media-inbox-compact-geometry-postcommit.trx`
- `media-anchor-postcommit.trx`
