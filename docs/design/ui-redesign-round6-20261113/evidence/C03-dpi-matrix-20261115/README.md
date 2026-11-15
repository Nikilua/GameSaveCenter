# C03 受控 WPF DPI 矩阵（2026-11-15）

## 状态与范围

C03 的受控矩阵已完成。生产候选仍是 `9880c81be09365d02b9335a633be95506095c8d9`；DPI Harness、截图和几何修正的最终源码提交为 `15f82b650fde8cbe370a48f8fbd6ba8978bf4dc3`，本轮对生产 `src/` 无改动。C03 只证明合成数据下的 WPF 布局与控件采样，不代表真实 Playnite 页面、实体键鼠、UIA 客户端、跨屏切换或物理输入验收。

受控运行使用 SDK 8.0.423 / Release、固定样本时间 `2026-11-13T00:00:00Z`、私有 `WinSta0` 桌面。进程及 UI 线程启用 PMv2。唯一物理屏幕为 `DISPLAY10`，3840×2160，窗口实际 DPI 为 144×144（150%）；该组 150% 页面、控件与 Popup 图像由真正运行在当前 DPI 下的 WPF 窗口捕获。125% 和 200% 为标明的 RenderTargetBitmap 栅格比例模拟，不是对应的 Windows/WPF DPI 环境，也不用于证明跨屏行为。运行前后输入桌面均为 `Default`，Harness 仅以代码设置合成数据，没有注入物理输入。

每个主题覆盖任务详情、概览优先操作 CTA、存档策略游戏选框和任务状态 ComboBox/Popup；共38张最终 PNG。选框保持 `IsSelected` Binding，CTA 保持 `OverviewPriorityActionCommand`，状态筛选保持 `TaskStatusFilter`。页面继续使用现有生产模板；捕获修正只改变 Harness 的离屏页面稳定和裁切。原始首轮捕获、修正版及最终核验输出均保留：本目录有30张首轮图；`../C03-dpi-matrix-20261115-final/` 有38张图；本轮权威验收目录 `../C03-dpi-matrix-20261115-verified/` 有38张图、几何表、报告、运行身份和 SHA-256 manifest。首次裁切问题作为诊断保留，验收以 `verified` 为准。

## 直接查看最终图像

当前显示器实际 WPF DPI（150%）的代表图：

| 样本 | 图像 |
| --- | --- |
| 任务详情，浅色 | [查看](../C03-dpi-matrix-20261115-verified/task-details-light-150pct-current-monitor-WPF-unnamed.png) |
| 概览 CTA，深色 | [查看](../C03-dpi-matrix-20261115-verified/overview-cta-dark-150pct-current-monitor-WPF-查看云端队列.png) |
| 存档策略游戏选框，浅色 | [查看](../C03-dpi-matrix-20261115-verified/save-policy-checkbox-light-150pct-current-monitor-WPF-unnamed.png) |
| 任务状态 Popup，深色 | [查看](../C03-dpi-matrix-20261115-verified/task-status-popup-dark-150pct-current-monitor-WPF-popup.png) |

同一任务详情的125%/200%图是栅格模拟：

| 比例 | 图像 |
| --- | --- |
| 125% 栅格模拟 | [查看](../C03-dpi-matrix-20261115-verified/task-details-light-125pct-logical-raster-simulation-unnamed.png) |
| 200% 栅格模拟 | [查看](../C03-dpi-matrix-20261115-verified/task-details-light-200pct-logical-raster-simulation-unnamed.png) |

## 可复核记录

[`dpi-matrix-report.txt`](../C03-dpi-matrix-20261115-verified/dpi-matrix-report.txt) 标记 `DpiEvidence OK`，SHA-256 为 `823443DF7572A1EABD7660714089825DDE59FA7D7112A33F4C9D9DEB75104341`；[`dpi-geometry.tsv`](../C03-dpi-matrix-20261115-verified/dpi-geometry.tsv) SHA-256 为 `D29EE602F792FF78029F984969FEB263465DB35614115DBB16A2CCE3C6E4476E`。运行身份见[`runner-metadata.json`](../C03-dpi-matrix-20261115-verified/runner-metadata.json)；全部图像和数据的清单见[`sha256-manifest.txt`](../C03-dpi-matrix-20261115-verified/sha256-manifest.txt)（SHA-256 `469981D0EEF89C9B8DBD34C7A6AF5EDAB7554A0318C3AA95B8A8C32FB3D1E5AB`）。

C03 三个源码提交及精确 SHA 上的两类 GitHub Actions job 和 artifact 身份见[最终候选门禁与 CI 证据](../final-candidate-gates-20261115/README.md)。现场 Playnite 视觉、真实 profile、真实存档/云端及物理输入均未操作或验收。
