# R02-04 当前 main 图文光学对齐复验

日期：2026-10-12。当前 `main` 为 `a21440834865551cdb6894830548af51ad1d8e78`。生产共享按钮实现已在较早提交中存在；本次没有修改生产 XAML、控件、命令或数据契约，只按当前提交重建并复验。R02-04 账本状态保持“已满足”。

## 行为与几何证据

- 当前 main 的 Release solution 构建通过：本机 SDK `8.0.423`，XAML `24/24`，`0 warning / 0 error`。完整构建刻意跳过测试，随后对 `R02OpticalAlignmentTests` 独立执行并保存 TRX。
- `R02OpticalAlignmentTests` 使用生产 `DesignTokens.xaml`、`WpfUiProduction.xaml`、`Redesign.xaml` 的 STA WPF 资源和实际视觉树，共 `7/7` 通过。包含中文/英文文字基线断言（差值不超过 `0.5 DIP`）、16/20 DIP 复合图标数字间距与中心断言，以及 Left/Center/Right 对齐切换和文字/视觉内容往返。
- TRX 的实际布局输出：16 DIP 图标 gap `8 DIP`、中心差 `0 DIP`；20 DIP 图标 gap `8 DIP`、中心差 `0 DIP`。该 STA 测试进程的 `VisualTreeHelper.GetDpi` 报告 `1.5×`；读数是逻辑 WPF 几何，不作为宿主物理 DPI 验收。
- `[R02-04-current.trx](tests/R02-04-current.trx)` 保留七条测试结果和几何输出。测试源码与 Playnite、Playnite.Tests 程序集身份见 [build-identity.json](build-identity.json)。

## 双主题视觉复核

- 当前提交身份的 RenderHarness Release 构建 `0 warning / 0 error`；完整 Light/Dark `render-qa` 通过，共生成 `372` 张 PNG，报告标记 `WorkingTreeClean=True`、logical DPI `1.00`。
- 已人工对照当前生产 Save 页面 `1040×700` Light/Dark 两张截图：图标、中文标签和同高按钮排列正常，未见明显挤字或上下偏移。只归档这两张与本项相关的截图：[Light](screenshots/Save-1040x700-light.png)、[Dark](screenshots/Save-1040x700-dark.png)。这些是合成数据下的离屏布局对照，未验证真实 Playnite、物理 DPI 或屏幕最终呈现帧。
- `scripts/validate-source.py` 通过。WPF 静态扫描 `36 XAML / 0 errors / 29 warnings / 177 info`；本阶段没有 XAML 变更，既有提示不归因为本阶段。

## Actions 与边界

- 推送 `a2144083` 的 Actions run `38244963620` 已终态 failure，失败步骤为“编译与测试”，步骤注释仅显示 `Process completed with exit code 1`。CI SDK `10.0.401` 与本机 SDK `8.0.423` 分开记录；失败命令、断言和堆栈仍未知，不据退出码盲改。
- 当前未认证 GitHub REST 请求因 API rate limit 返回 HTTP `403`；公开 job 页面要求登录后查看完整日志。保留诊断 artifact 元数据：ID `11745496487`、`GameSaveCenter-build-diagnostics`、478 KB、SHA-256 `11de81935473ce3da60739d481c768eba2af589d2c76cc819cf3977fd4daea1e`。artifact 下载路由返回 `404`，尚未取得其内容或完整日志。
- 未生成安装包，package identity 为 `not-provided`；未运行真实 Playnite、真实云端/存档操作、物理 DPI/跨屏、OS UIA/Narrator、IME、ETW 或宿主性能验收。下一可执行项为 R02-05 命中区与间距；CLOSE-OPTICAL-01 的完整共享回归门禁仍受 CI 日志与整轮验证状态约束。
