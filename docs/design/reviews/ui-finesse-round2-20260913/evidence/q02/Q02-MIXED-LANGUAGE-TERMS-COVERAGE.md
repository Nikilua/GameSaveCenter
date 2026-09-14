# Q02-08 混排空格与术语覆盖证据

采集日期：2026-09-14（Asia/Shanghai）。代码基线：`8a135eb`（补齐混排术语显示门禁）。本证据证明显示映射、分隔符和产品术语的源码契约，不替代真实 Playnite 宿主中的折行、字形、物理 DPI 或最终视觉验收。

## 短术语样本表

以下入口沿用真实 DTO/ViewModel 属性和生产 XAML 的绑定，不改写用户路径或排序键。跨字段组合统一使用带单空格的 ` · `；产品名和版本名保留原始大小写/空格语义。

| 入口 | 真实显示契约 | 受控样本 |
| --- | --- | --- |
| 游戏选框元信息 | `GamePickerItem.MetaDisplay` 按平台、安装、匹配状态用 ` · ` 连接 | `Steam · 未安装 · 未匹配` 的三个字段分别由现有 Playnite 测试锁定 |
| Trainer 导入候选 | `GameToolEntryCandidateDto.Display` 保留相对路径原值，再以 ` · ` 追加大小 | `工具/FLiNG Trainer v1.2/启动器.exe · 1 KiB` |
| 云端传输摘要 | 状态、项目数和可选下次尝试时间用 ` · `/中文标点分开 | `上传失败 · 2 项` |
| 媒体归类建议与批次 | 置信度、游戏名、原因，以及批次状态中的冲突语义保持分段 | `高置信 · Cyberpunk 2077 · 来源含 FLiNG Trainer`；`已应用 · 有冲突` |
| 恢复可用性指标 | 文件计数和大小指标分别保留单位，并用 ` · ` 分组 | `文件 2/3 · 大小 1 KiB/2 KiB` |
| 最近保护摘要 | 总数、已保护、需处理三段保持中文量词 | `共 3 个 · 已保护 1 个 · 需处理 2 个` |
| 媒体来源 | 平台/产品专名不翻译成不稳定的中文别名 | `Xbox Game Bar` |
| Trainer 版本信息 | 版本正则和功能数量显示保持版本原值与 `+N 项` 单位 | `FLiNG Trainer v1.2 Plus 30` → `v1.2`、`+30 项` |

## 自动门禁

- `UiDisplayMappingTests.MixedLanguageDisplaySurfacesKeepSemanticSpacingAndProductTerms` 构造云端、媒体归类、恢复指标、最近保护、媒体来源和 Trainer 版本的真实 DTO，锁定上表中的中英混排、单位、产品名和 ` · ` 分隔符；同时拒绝 `··`、分隔符两侧重复空格。
- 同一测试类已有路径样本 `MixedPathAndProductTermsKeepOriginalPathAndSemanticSeparator`，确认 `FLiNG Trainer` 路径原值和 `1 KiB` 单位不被格式化破坏。
- `GamePickerViewModelTests.PickerItemsExposeDemoStylePresentationFieldsLocally` 继续分别确认游戏选框的平台、安装和匹配字段都进入 `MetaDisplay`；它不改变真实游戏名称。
- 本次 Core 定向测试：`UiDisplayMappingTests` 共 17/17 通过；Core Release 串行构建 0 errors，保留 3 个 NU1900 网络漏洞源告警。

## 证据边界

这批门禁只覆盖字符串映射和源码绑定语义，不能证明八个入口在真实宿主小窗口中都不折行、字形均来自预期字体或在 125/150/175/200% 物理 DPI 下仍保持同样观感。宿主术语观感、屏幕阅读器朗读、IME/输入和跨页布局仍保持 Q02-08 的视觉/宿主待验边界。
