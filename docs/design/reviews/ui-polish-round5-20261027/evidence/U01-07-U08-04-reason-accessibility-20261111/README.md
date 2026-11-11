# U01-07 / U08-04：媒体长文本 UIA 可访问性

实现提交 `90b084cf084ec8a3eb4c9ec314ae582933da1949`（`main`）给生产 `MediaLongText` 样式补充 `AutomationProperties.HelpText`，绑定当前 `TextBlock.Text`。原因列及复用该样式的类型、来源列均可通过 UI Automation 读取完整显示文本。原有全文 Tooltip、选中项详情、复制、选框、虚拟化、滚动和归类安全门禁保持不变。

## 验证

- SDK 8.0.423 / Release 聚焦测试 `MediaInboxScrollBehaviorTests.InboxLongReasonExposesFullTextToTooltipAndAutomation`：1/1 通过，0失败、0 skip。测试使用生产 `MediaCenterView` 原因列，断言完整中文说明同时出现在 `TextBlock.Text`、Tooltip、AutomationProperties.HelpText 和 AutomationPeer.HelpText。TRX：[结果](tests/attempt-2/U01-07-U08-04-reason-accessibility.trx)，[命令日志](focused-test-attempt-2.log)。首轮还原在两分钟内无进展后中断，未开始测试且没有 TRX；保留[该次日志](focused-test.log)。第二次按仓库构建参数关闭 NuGetAudit、串行 MSBuild 后通过。
- 同 SDK 完整 `MediaInboxScrollBehaviorTests` 类：8/8 通过，0失败、0 skip。覆盖 1.0/1.25/1.5 缩放、2000项生产收件箱滚动/虚拟化、滚动锚点、表格/Inspector滚轮边界与圆角详情滚动。TRX：[结果](tests/media-inbox-regression/MediaInboxScrollBehaviorTests.trx)，[命令日志](media-inbox-regression.log)。
- `python scripts/validate-source.py`：exit 0，日志见[source validation](source-validation.log)。
- `python .codex/skills/wpf-apple-desktop-ui/scripts/validate_wpf_ui.py .`：exit 0，扫描32个 XAML，0 error、26 warning、179 info；现有 warning/info 保留并记录于[静态审查日志](wpf-static-audit.log)。
- `scripts/render-qa.ps1`：exit 0，双主题、7页 × 5种常用窗口，394 PNG、0 `PROBLEM`。离屏报告标记 `DpiScale=1.00`（逻辑 DIP），不推断真实宿主 DPI。完整[Render QA 报告](diagnostics/render-qa-report.txt)、媒体页[浅色](screenshots/Media-1040x700-light.png)和[深色](screenshots/Media-1040x700-dark.png)代表图已归档。原始渲染报告记载 base commit `804c2993`、`WorkingTreeClean=false`；渲染运行时本批 XAML/测试文件已是与 `90b084cf` 相同的内容。
- GitHub Actions 精确 push run #1539 / `43039700405` 对源码 SHA `90b084cf084ec8a3eb4c9ec314ae582933da1949` completed/success。SDK基线 job `130587956023` 15/15 步、SDK 10.0.401 job `130587956706` 8/8 步均成功，包含源码检查、Release编译测试、Render QA、WPF静态审查、扩展包与诊断归档。预览包及两份诊断 artifact 的ID、大小、digest见[精确CI记录](ci-run-43039700405.json)。GitHub combined status无独立status项；required-check/ruleset完整性因U09-06既有HTTP 403仍“远端待验”，未重复请求。

## 源码身份

| 文件 | SHA-256 | Git blob |
|---|---|---|
| `src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml` | `15496AFAE209D02FCDF6868B17D1EF32E963F8242F3188D3131977A1EBD38AA9` | `183de34b67b013497bbcb80179438d4d2025aa0c` |
| `tests/GameSaveCenter.Playnite.Tests/MediaInboxScrollBehaviorTests.cs` | `6C95CF1F8AE9B5E786F3FA22ACD4E2CB74BB525BB6220BD0467142C9D022BD65` | `4d986620404f3d69f3c93183af6b74a8e0df701f` |
| Target TRX | `DBCC9589219151CB2C89D288E5041ABAF9375593B72059F8D01E05C485A5CA49` | — |
| Full Media Inbox regression TRX | `CFEDA24F7FEF19FD7E03C96E64233D172C342BAFB236E68DDFA5BD1C28982EBC` | — |
| Render QA report | `AF921C81F4DEAF8C9E79B2F3C818FEBC940A3F66CD8FE09700B4C29C6AE99B5E` | — |

GitHub run、job与artifact身份清单：[ci-run-43039700405.json](ci-run-43039700405.json)。

## 验收边界与清理

本批没有可见的 Playnite 主窗口，因此真实宿主 UIA/Narrator、键盘/IME 与物理 DPI 仍待验；U01-07、U08-04 保持“代码完成待验收”。没有触发真实媒体删除、存档恢复或云端写入。

渲染临时目录生成395个文件、53,363,710 bytes。复制报告及浅/深主题代表图、核对哈希与文档引用、确认无链接/运行占用后，将原目录移入仓库 `.tmp/` 并删除。D盘可用空间从17,505,112,064 bytes回升到17,559,445,504 bytes；C盘从5,509,967,872 bytes保持5,509,967,872 bytes。仅清理本批自有渲染输出。
