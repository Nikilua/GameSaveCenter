# Q18-03 动效当前值接管受控证据

日期：2026-09-15  
生产探针提交：`bdb99b930be12abc967df365fb82194733125a16`（clean tree）  
探针：`RenderHarness.exe motionreentryprobe`  
证据源：真实生产 `AcrylicProductionShellView`，受控 STA WPF `Window`

## 运行身份与范围

```text
Scenario: motionreentryprobe
EvidenceSource: ControlledWpfWindow
Commit: bdb99b930be12abc967df365fb82194733125a16
WorkingTreeClean: True
DpiScale: 1.00 (offscreen logical DIP; real host DPI is not inferred)
Themes: light,dark
DataVolumes: production shell; audit-only GscMotionNormal=700ms override; interrupted transition takeover; 900x640 DIP
```

探针先启动侧栏收起动画，在约 210ms 后读取当前渲染宽度；随即触发相反的展开意图，并在新动画开始后立即再次读取宽度。若实现把动画硬重置到旧起点，第二次读取会发生明显跳变；本探针要求即时宽度与中断宽度差异不超过 1.5 DIP，并继续检查重入中间态和最终目标。

## 结果

| 主题 | 中断宽度 | 第二意图即时宽度 | 重入中间宽度 | 终态宽度 | 终态 X | 活动动画 |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| Light | 105.35 DIP | 105.33 DIP | 214.15 DIP | 270 DIP | 0 | False |
| Dark | 153.17 DIP | 153.33 DIP | 233.13 DIP | 270 DIP | 0 | False |

两主题的第二意图都从当前有效宽度接管（即时差异分别为 `0.02` 和 `0.16 DIP`），没有闪回到 72 DIP 收起端点；重入中间态继续向 270 DIP 展开，完成后 X 位移和动画时钟均归一。

## 人工查看的截图

- [浅色中断态](motion-reentry-probe-20260915/motion-reentry-light-interrupted.png)
- [浅色第二意图即时态](motion-reentry-probe-20260915/motion-reentry-light-takeover.png)
- [浅色重入中间态](motion-reentry-probe-20260915/motion-reentry-light-mid.png)
- [浅色重入终态](motion-reentry-probe-20260915/motion-reentry-light-final.png)
- [深色中断态](motion-reentry-probe-20260915/motion-reentry-dark-interrupted.png)
- [深色第二意图即时态](motion-reentry-probe-20260915/motion-reentry-dark-takeover.png)
- [深色重入中间态](motion-reentry-probe-20260915/motion-reentry-dark-mid.png)
- [深色重入终态](motion-reentry-probe-20260915/motion-reentry-dark-final.png)

人工查看两主题的中断、接管、中间和终态截图，确认壳层文字、按钮、状态点与侧栏表面保持可读，且没有明显的旧终点闪回。

## 门禁与边界

- RenderHarness Release：0 warning / 0 error。
- `UiFinesseRound2ControlSourceTests`：24/24 通过。
- 该证据将 Q18-03 的受控视觉列升级为通过；真实 Playnite 鼠标/键盘快速切换、物理 DPI、屏幕帧和宿主窗口时序仍不由受控窗口替代，宿主列与最终结论继续保持未完成。

原始报告：[motion-reentry-probe-report.txt](motion-reentry-probe-20260915/motion-reentry-probe-report.txt)
