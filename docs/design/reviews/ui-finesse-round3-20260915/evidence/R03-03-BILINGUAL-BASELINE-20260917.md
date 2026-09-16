# R03-03 双语混排基线

日期：2026-09-17  
状态：已满足（受控 WPF 混排基线与双主题离屏验证；真实 Playnite 宿主最终呈现仍未验）  
代码提交：`466c2f53c2110044a42d9dd6af385583abb461a9`（`codex/ui-finesse-round2`，验证时工作树干净）

## 任务边界

R03-03 要求校对 Save Center/存档中心、日期容量和中文标点混排，确认同一行没有明显上下漂移；只有确有必要时才使用明确的文本片段样式，不能依靠整行任意偏移。

本阶段先核对当前生产基线：壳层页面标题已由现有导航与 `UpdatePageHeader` 提供“存档中心”，Save Center 的日期、容量和数字列已经复用现有 Typography/数字样式，页面没有为中英文片段设置任意垂直偏移。没有把 main 的旧实现带入当前分支，也没有引入新的字体或设计体系。由于已有页面使用同一资源链，本阶段只补可复核的实际排版证据，不改生产 XAML。

## 实现与证据

`TypographyDiagnostics.MixedBaselineEvidence` 和 `CaptureMixedBaseline` 复用现有 `UiFontChain`，在 STA WPF `TextFormatter` 中取得 `TextLine.GetIndexedGlyphRuns()`，记录整行 baseline、各实际 `GlyphRun.BaselineOrigin.Y` 的最小/最大值、run/glyph 数和未配对 surrogate。`IsStable` 只有在有实际 glyph run、glyph 数非零、无未配对 surrogate 且 baseline spread `≤0.5 DIP` 时才成立；因此测试不是 `Assert.Contains` 式源码存在性检查。

RenderHarness 的 `finesseprobe` 对 Light/Dark 都输出相同的四组基线样本：

| 样本 | Glyph runs | Glyphs | Line baseline | Min/Max glyph baseline | Spread | 结果 |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| `存档中心 Save Center` | 3 | 16 | 11.357 | 0 / 0 | 0 | stable=True |
| `日期：2026-09-17 · 时间 03:02` | 9 | 24 | 11.357 | 0 / 0 | 0 | stable=True |
| `容量：1.71 GiB · 24.6 MiB` | 3 | 22 | 11.357 | 0 / 0 | 0 | stable=True |
| `中文标点：全角引号“存档”、书名号《中心》……` | 6 | 23 | 11.357 | 0 / 0 | 0 | stable=True |

同时保留 R03-01 的候选字体与最终 GlyphRun 证据，中文标点、全角符号、数字和 Latin 片段没有被单独推断为缺字或通过行级位移“修正”。

## 验证结果

1. 受影响 Playnite 项目编译：`0 warning / 0 error`；新增 `MixedChineseLatinDateAndCapacityRunsShareOneBaseline` 行为测试独立 `1/1`。
2. `scripts/build.ps1 -Configuration Release -OutputRoot .tmp\\r03-03-build-final` 绑定 `466c2f5` 通过：XAML `24/24`；解决方案构建 `0 warning / 0 error`；Core `83/83`；Worker `311/311`；Playnite `548` 通过、`57` 跳过、`0` 失败，总计 `605`。
3. 从隔离构建程序集运行 `TypographyDiagnosticsTests`：`11/11` 通过，`0` 失败、`0` 跳过。
4. RenderHarness Release 构建 `0 warning / 0 error`；Light/Dark `finesseprobe` 均报告 `Commit=466c2f5`、`WorkingTreeClean=True`、四项 `spread=0`、`finesse-fixture OK`。报告明确为 `1120×980` offscreen logical DIP，未推断真实宿主 DPI。
5. `scripts/render-qa.ps1 -Configuration Release -Output .tmp\\r03-03-render-final` 绑定当前 clean commit，双主题生产页面与 1040×700 窄窗探针均 `render-qa OK`；Overview、Save、Trainer、Media、Maintenance、Task、Settings 的页面滚动/列表可达性报告无失败。人工抽查 Light/Dark Overview 与 Media 1040×700 图，未见 R03-03 相关的中英文、日期容量或标点挤压/裁切。
6. `python scripts/validate-source.py`：通过。

本阶段只增加排版证据、行为门禁和 RenderHarness 报告输出；没有改游戏选框、滚动条系统、命令/Binding、取消/错误语义、恢复保护、有限列表性能或 Playnite/net462 契约。验证使用合成混排文本与隔离输出，没有执行真实备份、恢复、媒体删除、云端写入或外部诊断发送。

## 未验边界与下一步

`GlyphRun` 和截图证据来自受控 STA WPF/offscreen logical DIP，不等价真实 Playnite 最终 presented frame、用户安装字体差异、物理 DPI/跨屏、OS 输入/IME、屏幕阅读器、ETW 或宿主帧率/性能。生产页面实际是否由宿主外层替换字体、用户主题如何呈现，仍需宿主验收；57 条既有 UI 基线跳过仍按项目规则保留。下一可执行任务为 R03-04 数字列对齐。
