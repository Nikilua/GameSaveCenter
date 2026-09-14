# Q02-07 生产正文尺寸覆盖证据

采集日期：2026-09-14（Asia/Shanghai）。代码基线：`d22928a`（统一生产小字号语义资源）。本证据证明生产 XAML 的源码门禁与共享资源解析，不替代真实 Playnite 小窗口、125/150/175/200% 物理 DPI 或最终 GlyphRun 验收。

## 源码处理范围

- `src/GameSaveCenter.Playnite/Views` 与 `Settings` 下生产 XAML 共 9 个文件完成迁移，排除了 `Views/Development` 和 `Themes/AcrylicReference` 参考资产。
- 本阶段替换 109 处显式 `FontSize="10"`/`FontSize="11"`，统一使用 `{DynamicResource GscCaptionFontSize}`；该共享令牌当前由 `Themes/Typography.xaml` 定义为 12 DIP。
- 10.5、12.5、18 等明确的中间层级/标题层级未被机械抹平；它们需要结合各自语义继续审查。路径输入另由 `GscWpfUiPathTextBox` 提供代码字体，不借小字号解决技术文本可读性。

## 自动门禁

- `TypographyDiagnosticsTests.ProductionTenAndElevenPointTextUsesSharedCaptionToken` 遍历生产 Views/Settings（排除 Development），断言没有残留 `FontSize="10"` 或 `FontSize="11"`，并确认共享 Caption 令牌实际被使用。
- `rg -n 'FontSize="(10|11)"' src/GameSaveCenter.Playnite/Views src/GameSaveCenter.Playnite/Settings -g '*.xaml'` 当前结果为 0 条。
- WPF Release 构建成功（0 errors；Contracts/Core 各有 1 个既有 NU1900 网络漏洞源告警），`TypographyDiagnosticsTests` 定向测试 8/8 通过；RenderHarness 明/暗主题 `finesseprobe` 均退出 0、对比度 0 violations、`finesse-fixture OK`。

## 证据边界

离屏夹具的 `DpiScale=1.00` 是逻辑 DIP，且报告的 `FontActualGlyphRun` 仍为 `unknown`。因此生产小窗口的折行、裁切、真实宿主字体安装差异、物理 DPI、IME/输入和 Playnite 嵌入视觉仍保持 Q02-07 未完成边界。
