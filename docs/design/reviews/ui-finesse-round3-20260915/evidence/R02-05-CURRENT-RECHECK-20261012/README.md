# R02-05 当前 main 命中区与间距复验

日期：2026-10-12。复核对象为 R02-05“命中区与间距”。当前 `main` 为 `90099a09104677708a5829cf7e6a5d22b6b414ff`。共享密度实现和 R02-05 行为测试早已存在；本轮未修改生产控件、页面、命令或业务逻辑，只按当前提交重建、运行既有行为门禁并补双主题证据。账本状态保持“已满足”。

## 实际命中与窄宽行为

- 当前 main Release solution build 使用本机 SDK `8.0.423`，通过：XAML `24/24`、`0 warning / 0 error`。构建脚本跳过测试；之后用确切当前提交程序集单独运行 `R02HitAreaSpacingTests`，结果 `2/2`，TRX 见 [R02-05-current.trx](tests/R02-05-current.trx)。
- 两项测试加载生产 `DesignTokens.xaml`、`WpfUiProduction.xaml`、`Redesign.xaml`，在 STA WPF `Window` 中 Measure/Arrange，并用 `VisualTreeHelper.HitTest` 命中真实 `Button` 实例，不是源码字符串断言或手工加大 hit-test 代理。
- `AdjacentCopyAndDeleteButtonsHaveIndependentHitTargets` 验证复制与危险删除各约 `34×34 DIP`（容差按当前 WPF DPI 下半个物理像素换算）；共享按钮 6 DIP margin 形成 `5.5–6.5 DIP` 净距；中心分别命中自己的实例，中心间隙不命中按钮，两个命中矩形没有正面积相交。
- `NarrowWrapKeepsCopyAndDeleteInOneActionRow` 使用 `82 DIP` 窄 `WrapPanel`：复制和删除仍同排，第三动作换到下一排；三个按钮的中心分别命中对应实例；相邻间距保持，任意矩形不正面积重叠。此项同时覆盖所需的负例（间隙不命中、矩形不重叠）。

## 当前双主题页面复核

- 当前提交身份的 RenderHarness Release `0 warning / 0 error`；Light/Dark 全量 `render-qa OK`，生成 `372` PNG，报告 `WorkingTreeClean=True`、offscreen logical DPI `1.00`。
- 已人工对照合成数据下 Media `1040×700` Light/Dark 截图：批量动作组保持在同一操作区，页面滚动和表格在可见范围内。[Light](screenshots/Media-1040x700-light.png) / [Dark](screenshots/Media-1040x700-dark.png)。截图只证明离屏页面排列和主题对照，不代表鼠标/OS 命中、真实 Playnite、物理 DPI 或最终呈现帧。
- `scripts/validate-source.py` 通过；WPF 静态扫描 `36 XAML / 0 errors / 29 warnings / 177 info`。本轮没有 XAML/resource 改动，现有提示不归因于本阶段。
- 当前程序集 SHA/MVID、测试文件与 TRX SHA、RenderHarness 身份见 [build-identity.json](build-identity.json)。没有生成包，package identity `not-provided`。

## 推送后的 Actions

- `90099a09` 对应 run `38247748056` 已终态 failure，CI SDK `10.0.401`，失败步骤“编译与测试”，步骤注释仅为 `Process completed with exit code 1`。SDK安装、源码结构、诊断自测和SDK基线步骤成功；离屏渲染、WPF静态审查和打包被跳过；诊断上传成功。
- 未认证 Jobs/log API 被 rate limit HTTP `403`；页面列出完整步骤状态但没有日志正文。诊断 artifact ID `11747310690`，名称 `GameSaveCenter-build-diagnostics`，479 KB，SHA-256 `8586bcace066ca779c72dbf9fbea1843c19a49133d619b9a6000e6e8dc6e79c5`；下载路由最终返回 `404`。失败命令、断言和堆栈未取得，根因未知；不按exit code猜修复。

## 边界与下一步

验证的是生产 WPF 样式、隔离 STA Window、逻辑 DIP 几何和 `VisualTreeHelper.HitTest`。未模拟真实鼠标、触屏/笔、OS UI Automation/读屏或 Playnite 嵌入；未运行真实云端、存档、媒体删除或诊断。屏幕截图中的游戏及媒体记录为 RenderHarness 合成数据。

下一项先检查 R02-06 菜单由插件还是 Playnite 宿主拥有、已有菜单契约能覆盖哪些状态。宿主侧 UI 若仍不可审计，则记录具体环境限制并继续独立的 R02-07 异步上下文，不自建第二套菜单体系。
