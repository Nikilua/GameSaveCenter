# 概览空状态说明与主题色证据

日期：2026-09-29  
基于源码提交：`820fb98cf382687231bbc773a5ead1abd1903e18`；本证据目录与对应源码/测试一起提交。  
范围：`OverviewView` 的空状态说明和首页游戏库指标色；`MaintenanceView` 动作分类标签色。

## 变更

- “今日工作台”只在 `IsDashboardSnapshotLoaded == false` 时显示短提示。真实快照加载后折叠该提示；若重新进入未加载状态则恢复。真实刷新、全库备份、媒体同步三条命令及其绑定不变。
- 快照加载时，首页工具栏高度从 `112 DIP` 减至 `92 DIP`。回归直接操作生产 `OverviewView` 和合成、可通知的快照状态。
- “已管理游戏”是统计指标，不再使用信息状态蓝；改用随浅/深主题变化的主强调紫。语义信息色仍用于真实状态/信息反馈。
- Maintenance 动作的分类是次级元数据，不再使用信息状态蓝，改用主题次级文字色。

## 验证

- Release solution build：`0 warnings / 0 errors`；XAML structural validation：`24/24`。
- 精确隔离 Playnite.Tests net472 程序集 VSTest：`8/8` 通过，0 失败、0 跳过。新增 Light/Dark 行为用例检查提示 Visible → Collapsed → Visible、工具栏缩短、三条命令绑定、动态主题令牌，以及分类文字颜色。
- 同一 WPF 夹具按实际解析的 `GscGlassStrongBrush` 采样文本对比度：大号首页指标 Light `4.13:1`、Dark `4.77:1`；普通说明文字 Light `6.43:1`、Dark `7.86:1`。检查门槛分别为 `3:1` 和 `4.5:1`。
- 隔离测试 DLL：SHA-256 `AA2B84466E3245E9D5E3B782C1CEBB044465B7AC41755C1CE263211A2A49F0E6`；MVID `f524cb8c-caab-4475-9a69-e82d4d316c3c`。
- Playnite 插件 DLL：SHA-256 `CA33C083048BB218CD36BCD91733E0E4A7E242384FF0BEF68496A196280A5A3A`；MVID `0c54a0b7-bc94-478d-ac43-09ec068fc29b`。
- `scripts/validate-source.py` 通过。WPF 静态检查 `0 errors / 30 warnings / 177 info`，告警来自既有其他视图、滚动容器和参考资源，本次触及行没有 error。
- `scripts/render-qa.ps1 -Configuration Release` 全矩阵 `render-qa OK`，报告无 `PROBLEM`；涵盖 Light/Dark、多常见尺寸及窄窗。完整报告留在可再生 `.tmp`，只归档本次相关的 1040×700 DIP 截图。

## 依据与限制

Fluent 2 的颜色原则把中性色作为基础、品牌色用于少量强调，语义色用于重要反馈；本次据此把纯统计/分类文字从信息蓝移开，没有整体改写主题色板。WCAG 2.2 SC 1.4.3 对普通文字要求至少 `4.5:1`，大号文字至少 `3:1`。参考：[Fluent 2 Color](https://fluent2.microsoft.design/color)、[WCAG 2.2 Contrast Minimum](https://www.w3.org/WAI/WCAG22/Understanding/contrast-minimum)。

附件是离屏 RenderHarness 图片，布局单位为 DIP；对比度来自 WPF 生产资源夹具。没有启动 Playnite，也没有验证宿主主题、物理 125%/150% DPI、OS 输入或最终呈现帧。静态颜色核对不是所有页面的全量对比度认证。Media Inbox 用户报告的真实滚动空白仍等待安全 Playnite 会话内同进程 `[GSC-GRID-DIAGNOSTIC]`，本批没有触及或推断其根因。

下一项继续盘点生产可见页中带数据后仍显示的静态说明，逐项确认它是否提供必要语义，再补动态状态行为证据；保留数据为空时真正有用的空状态提示。

## 图片与测试记录

- [Light 1040×700 DIP](overview-light-1040x700.png)
- [Dark 1040×700 DIP](overview-dark-1040x700.png)
- [overview-copy-color-contrast-final.trx](overview-copy-color-contrast-final.trx)
