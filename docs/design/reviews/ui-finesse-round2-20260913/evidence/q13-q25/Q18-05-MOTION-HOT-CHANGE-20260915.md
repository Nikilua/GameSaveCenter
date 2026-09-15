# Q18-05 系统动画热变更受控证据

日期：2026-09-15  
生产探针提交：`dc1dd6708cef9d86c1d6576d440402acbf0c5df0`（clean tree）  
探针：`RenderHarness.exe motionhotprobe`  
证据源：真实生产 `AcrylicProductionShellView`，受控 STA WPF `Window`，不是伪造的 Dashboard 或宿主替身

## 运行身份与范围

报告记录：

```text
Scenario: motionhotprobe
EvidenceSource: ControlledWpfWindow
Commit: dc1dd6708cef9d86c1d6576d440402acbf0c5df0
WorkingTreeClean: True
DpiScale: 1.00 (offscreen logical DIP; real host DPI is not inferred)
Themes: light,dark
DataVolumes: production shell; audit-only GscMotionNormal=700ms override; runtime motion preference toggle; 900x640 DIP
```

700ms 仅用于让受控桌面负载下的中间帧可观测，生产资源文件没有改动。

## 行为结果

| 主题 | 切换禁用前中间宽度 | 中间 Opacity | 活动动画 | 禁用后终态 | 禁用重入 | 重入活动动画 |
| --- | ---: | ---: | --- | ---: | ---: | --- |
| Light | 100.09 DIP | 0.858 | True | 72 DIP / 1 / X=0 | 270 DIP | False |
| Dark | 143.26 DIP | 0.640 | True | 72 DIP / 1 / X=0 | 270 DIP | False |

探针在侧栏收起动画运行约 210ms 时将 `MotionEnabledProvider` 从 `true` 切为 `false`，直接调用生产 `NormalizeMotionIfDisabled()`，确认活动宽度动画、Opacity 动画和 X 位移时钟均被清除并写回终态；随后再次展开，禁用路径同步完成，不再启动动画。

## 人工查看的截图

- [浅色活动中间态](motion-hot-probe-20260915/motion-hot-light-enabled-mid.png)
- [浅色禁用终态](motion-hot-probe-20260915/motion-hot-light-disabled-final.png)
- [浅色禁用重入](motion-hot-probe-20260915/motion-hot-light-disabled-reentry.png)
- [深色活动中间态](motion-hot-probe-20260915/motion-hot-dark-enabled-mid.png)
- [深色禁用终态](motion-hot-probe-20260915/motion-hot-dark-disabled-final.png)
- [深色禁用重入](motion-hot-probe-20260915/motion-hot-dark-disabled-reentry.png)

人工检查确认两主题壳层文字、按钮、状态点和侧栏在终态均保持可读；截图只证明受控 WPF 的视觉终态，不推导真实物理 DPI 或屏幕帧。

## 门禁与边界

- RenderHarness Release：0 warning / 0 error。
- `UiFinesseRound2ControlSourceTests`：22/22 通过。
- `GscMotion.IsEnabled` 继续动态读取 `SystemParameters.HighContrast` 与 `SystemParameters.ClientAreaAnimation`；探针用 provider 切换复现同一生产归一化路径。
- 该证据将 Q18-05 的受控视觉列升级为通过；真实 Windows 通知到 Playnite 页面、宿主输入、物理屏幕帧和宿主 DPI 仍保留在宿主/性能边界，不升级宿主列或最终结论。

原始报告：[motion-hot-probe-report.txt](motion-hot-probe-20260915/motion-hot-probe-report.txt)
