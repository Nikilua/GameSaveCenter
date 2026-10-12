# R06-04 当前 main 快捷键行为复核

日期：2026-10-13（Asia/Shanghai）

结论：复用 `afe4aa55` 已实现的 `DataGridClipboardBehavior`、五类生产表格 profile、DTO 和脱敏策略；当前 main 的原行为测试能证明格式化值和表格接线，但没有实际触发快捷键路由。本轮只增加一个内部 modifiers 测试 seam 和一个受控 WPF `PreviewKeyDown` 行为测试，没有改生产快捷键或剪贴板行为。

## 构建与测试

- 构建源码基线：main `e277a804bd4774cd32ad0bde0dfe2b8f76bc3308`，本轮工作树仅改测试 seam 和测试。Pinned SDK `8.0.423` 位于隔离目录；系统默认 dotnet 只发现 `9.0.302`，没有用它替代项目要求的 SDK。
- 官方 `scripts/build.ps1 -Configuration Release -SkipTests -OutputRoot ... -DiagnosticsRoot ...`：Release `0 warnings / 0 errors`，XAML `24/24`；Playnite `net462`、tests `net472`。隔离输出位于 `D:\GameSaveCenterTools\R06-04-current-main-20261013`，诊断目录 `20261012T200738463Z-ac6c4219`。
- `R06ClipboardBehaviorTests`：`5 passed / 0 failed / 0 skipped`。包括既有格式化/多选显示顺序/ID 去重/完整路径/凭据脱敏/五类 profile 接线，以及本轮实际对生产 `DataGrid` 触发 `PreviewKeyDown` 路由的 Ctrl+C 行复制、Ctrl+Shift+C 单元格复制。负例覆盖 Ctrl+Alt+C、Ctrl+X、解绑后的 Ctrl+C 与没有选中行；未复制时保持 clipboard seam 内容不变，无选择时呈现失败提示且不吞事件。
- `R22CopyFeedbackBehaviorTests`：`2 passed / 0 failed / 0 skipped`，复核剪贴板占用失败提示和重试反馈。
- `python scripts/validate-source.py` 与 `git diff --check` 通过。两份脱敏 TRX 保留在 `tests/`，去掉本机路径、用户名和机器名，并按仓库约定规范为 LF。

## 身份

- `DataGridClipboardBehavior.cs` SHA-256：`A16229D66427BA77F2BF2F44D841D0D026AB0A018D9082E40F4F73D7D0208A5A7`。
- `R06ClipboardBehaviorTests.cs` SHA-256：`99381AD3D4147881DC6C02DCE72F61831A1A073072BA407F755B89D0DD55FA79`。
- Playnite DLL SHA-256 / MVID：`63FA4310CC8295EA58E76B79EDEF2B76CD32046152A91705A529A11CB50265EA` / `d1ab9447-edbe-434a-9d4d-98300a29848b`。
- Playnite tests DLL SHA-256 / MVID：`6DA620E1ED0E749D7CF5ABC4BDE27BBDC7DA519AB0909E2993C613881CA042BE` / `c3e76dff-fedb-47f8-9cfc-4db12727e098`。

| TRX | 结果 | SHA-256 |
| --- | --- | --- |
| [R06-04-clipboard-route.trx](tests/R06-04-clipboard-route.trx) | 5/5 | `BD079EBE93F7BA9837F6509C27242264BC7CE5C46B68888983D3A9B5BB8EFFC9` |
| [R06-04-copy-feedback-regression.trx](tests/R06-04-copy-feedback-regression.trx) | 2/2 | `5F1E8433FA5D3FCB196D7F9459125A2AA57B20ED175A0332F30EE39DEBF26298` |

## 验证边界与下一步

快捷键测试在显示的隔离 STA 窗口中对真实生产 DataGrid 触发 WPF routed event；modifier 通过仅供测试使用的内部 seam 提供，clipboard 写入使用 fake setter。它不代表真实 OS 键盘、系统剪贴板或 Playnite 宿主输入。没有视觉布局改动或当前 RenderHarness 截图；双主题渲染证据仍绑定原实现 `afe4aa55`。真实 Playnite、系统剪贴板占用、物理 DPI/跨屏、IME、UIA/Narrator 和宿主性能仍未验，也未触碰真实存档、媒体、云端或诊断数据。

上一阶段提交 `e277a804` 的 GitHub commit checks 页面截至 2026-10-12 20:11 UTC 仍显示 `Re-running jobs...`，没有可见终态或完整日志；REST API 匿名请求受限流拒绝。该 CI 状态不由本机 SDK `8.0.423` 结果替代，目标 CI SDK `10.0.401` 尚待确认。

下一项：R06-05 列头说明；先核对单位、缩写、排序箭头与现有列头行为，再决定是否只需补当前 main 证据。
