# R00/R01 当前 main freshness 复核

日期：2026-10-15
源码提交：`88b8eb57f13750b4f0a43c1f94a7dc1d59ae6acc`
本机 SDK：`8.0.423`，MSBuild `17.11.48`，VSTest `17.11.1`；插件目标 `net462`，Playnite 测试目标 `net472`

## 复核范围

HEAD freshness 扫描先发现 R00/R01 的六条基线记录仍引用旧源码身份：`R00-01-02`、`R00-03`、`R00-05`、`R00-08`、`R01-03`、`R01-06`。本轮针对每条记录登记的源码路径，重建当前 Release 输出，分别运行相关行为类，并重新执行 RenderHarness audit 与 E01–E20 索引校验。基线中的 `sourceCommit` 只更新到本次确实验证的 `88b8eb57`；package identity 仍为 `not-provided`。

没有生产代码、XAML、命令、绑定或测试源改动。这是证据与 freshness 校正，不是重新实现这些功能。

## 构建、测试与索引

- 完整解决方案 Release 构建成功：`0 warnings / 0 errors`。脚本的 XAML structural validation 为 `24/24`；`python scripts/validate-source.py` 通过。
- 受影响测试类分进程串行执行：`UiDiagnosticsExporterTests 11/11`、`UiFinesseFoundationTests 9/9`、`R09HighContrastBehaviorTests 3/3`、`WpfUiResourceDictionaryTests 139 passed / 39 skipped / 0 failed`、`GamePickerKeyboardBehaviorTests 7/7`、`UiAuditSourceTests 6/6`。合计 `175 passed / 39 skipped / 0 failed`；39 个跳过没有计作通过。
- RenderHarness 独立 Release 构建成功：`0 warnings / 0 errors`。当前 audit 的 E01–E20 索引校验为 `20/20` 引用、身份、样本和边界有效。
- 同一完整隔离输出另行重跑异机报告涉及的 `R09FocusOutlineBehaviorTests`：`2/2` 通过。首个启动尝试使用过深的临时路径，testhost 尚未执行测试；缩短 `TEMP/TMP` 后，同一构建输出成功运行。此启动前路径限制不计为测试结果。

### 精简审计结果

RenderHarness 当前源码身份为 `88b8eb57`：`168` runtime snapshots、`116` runtime warnings、`0 HIGH`、`13 MEDIUM`、`0 Fidelity`、`0 failed routes`。13 条 MEDIUM 仍记录工具栏垂直扩展/自然换行风险，没有清零或折算为通过。20 项索引与受控审计报告、完整 manifest/route JSON、布局与 fidelity 报告均在 [`audit/`](audit/)；只保留五张 1440×900 DIP 的合成代表图，未归档数百张临时图或 88 MB 审计 ZIP。

RenderHarness metadata 的 `WindowsVersion` 字段报告 `6.2.9200.0`，而同次 SDK 信息报告 `10.0.22000`。原值保留在归档中；本记录不把该 metadata 字段当作宿主或硬件环境验证。

保存截图前已检查图面。媒体图中的 `D:\Media\Inbox` 是合成夹具路径；没有真实存档、媒体、设置、云端或诊断数据参与审计。图片是离屏逻辑 DIP WPF 输出，不代表 Playnite 最终呈现、物理 DPI、真实跨屏或屏幕帧。

## 外机 R09 焦点描边失败对照

用户所贴日志的断言值是 `2,2,2,2` 对 `1,1,1,1`，而堆栈报告测试源码第 151 行。当前提交的同一测试第 151 行检查错误边框颜色，第 152 行检查 `Chrome` 的 `1 DIP`，第 154 行才检查独立 `ValidationStroke` 的 `2 DIP`。因此该日志行号/值形态与当前源码映射不一致，旧测试源、测试 DLL 或 PDB 是线索，仍不能据此确认外机根因。

当前 main 的 `GameSaveCenter.Playnite.dll` 与 `GameSaveCenter.Playnite.Tests.dll` 在本次隔离 Release 输出中重建；对这同一输出运行 R09 焦点类为 `2/2`。程序集与源码哈希、MVID 及测试结果见 [`build/`](build/) 和 [`tests/R09FocusOutlineBehaviorTests.trx`](tests/R09FocusOutlineBehaviorTests.trx)。`libpng iCCP` 警告不在断言堆栈中；`InvalidComObjectException` 出现在外机断言失败后的 WPF 清理阶段，当前证据不足以把它当作厚度断言的原因。

因此没有调整模板、放宽描边断言或改写视觉。确认外机具体载入内容仍需要那次失败运行的 `HEAD`、测试程序集和 PDB 身份；此信息缺失不阻塞其他任务。

## 文件身份和复现边界

本次 Release 输出的插件 DLL SHA-256 为 `CB6D7DA5613A36065BD8541AABF2BD68318A4B9D312666A0B73DF049273F8D97`，MVID `24135592-de2e-4198-8ac6-930f120226ea`；Playnite 测试 DLL SHA-256 为 `BAF74B8A177F498BED63F3F3F47E1BDFB7E85DDFD48D0811590CCF8E7ACB17CB`，MVID `54831945-8a8b-42f8-b602-07abd8258a9c`。文件级 SHA-256 另见 [`build/file-hashes.txt`](build/file-hashes.txt)。

本机 build diagnostics 记录工作树不是 clean：当时存在其他未跟踪测试结果/临时文件，本轮没有改动或清理它们。源码与程序集的 ProductVersion 标记本次提交；没有生成或验证 Playnite 安装包，不能声称 package identity 或真实宿主已验证。

重建受控审计：

```powershell
$env:GSC_SOURCE_ROOT = (Get-Location).Path
$env:GSC_UI_AUDIT_COMMIT = (git rev-parse HEAD).Trim()
dotnet run --project tests/GameSaveCenter.RenderHarness/GameSaveCenter.RenderHarness.csproj -c Release --no-build -- audit .tmp/r01-06-audit
./scripts/validate-ui-evidence-index.ps1 -AuditRoot .tmp/r01-06-audit -MinimumRows 20
```

SDK 8.0.423 的本机结果不替代 GitHub Actions SDK 10.0.401 lane。推送后仍须记录对应 Actions 的明确终态；真实 Playnite、OS UIA/读屏/IME、物理 DPI/跨屏、presented frame、ETW 和宿主性能仍未验证。
