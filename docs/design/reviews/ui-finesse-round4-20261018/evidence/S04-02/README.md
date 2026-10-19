# S04-02 双图工作区

实现提交：`825e58e9867ae3303c100eef31693f768f1772cf`（`实现截图并排比较工作区`）。

## 实现

- 比较集合达到两张截图后，在媒体中心显示两个固定尺寸预览框。卡片展示名称、原始像素尺寸、文件大小、来源和 Playnite 游戏 ID；图片在相同外框内按比例显示，不拉伸。
- 复用 `AsyncThumbnailImage` 与缩略图缓存；在后台读取被选中图片的源帧尺寸，显示原图尺寸而不是被 480 像素预览上限缩小后的尺寸。取消、清空和失败时同步清理尺寸状态。
- 比较工作区宽度低于 560 DIP 时上下排列。宿主高度低于 680 DIP 时自动收起预览以保留列表空间，用户可通过“展开对比”手动打开。当前媒体列表的最小可读高度为 154 DIP，至少容纳完整一行媒体卡片。
- RenderHarness 使用空归档路径的合成 DTO，不探测外部媒体文件。实际缩略图和源尺寸行为另用测试生成的 960×540 PNG 验证。

## 验证

- SDK `8.0.423`，Release 全解决方案构建：`0 warnings / 0 errors`；XAML 结构检查 `25/25`。构建诊断 ID：`20261019T073254207Z-67c0a921`。该构建源提交为 `330d9e4794e4f45f05082565a3192652cf8a2c01`，工作树包含本项改动；源文件哈希列于下表。
- `MediaComparisonSelectionTests`：`5/5` 通过，覆盖生产按钮命令、自动展开/短宿主收起、手动开关、宽窄排布及筛选切换；`R18ThumbnailBudgetTests`：`2/2` 通过，覆盖原图尺寸 `960 × 540 px` 与预览 `480 × 270 px`。两个 WPF 测试类分别启动进程运行。TRX：[比较工作区](tests/S04-02-media-comparison.trx)、[原图尺寸](tests/S04-02-thumbnail-dimensions.trx)。
- RenderHarness Release 构建 `0 warnings / 0 errors`；light/dark 两主题、11 种窗口尺寸，`render-qa OK`、`0 PROBLEM`。摘要：[render-summary.txt](render-summary.txt)；[2560×1440 合成布局截图](render/Media-2560x1440-tab1.png)。完整报告 SHA-256：`B8544C4339BA70CC6B8B39E9CA0202E585C9C6DA3E6957FE1BD8308C1F0F4E63`。
- `python scripts/validate-source.py .` 与 `git diff --check` 通过。

## 输入身份

下表记录本次实现源码与受控输出的 SHA-256。插件 DLL、测试 DLL/PDB 来自隔离目录 `.tmp/round4-s04-02-validation/build`。

| 文件 | SHA-256 |
| --- | --- |
| `AsyncThumbnailLoader.cs` | `52E65F9B04BB099127A4484880E167F4719A64E1E34F505267AAF5D3444FCAC0` |
| `AsyncThumbnailImage.cs` | `BA46FFE418FD43708C0B9EE93EF3C4D95E2640D708BE12C86ED62D217D998564` |
| `DashboardViewModel.Media.cs` | `A5401F9EFD5164878E2551D33A6AF5AE64F473EDA079C7493E116D2AB05CE6B8` |
| `DashboardViewModel.cs` | `0D66482D2D2F1CDA992894C160C77B855A996001BF20C0E34A548183257EEE07` |
| `MediaCenterView.xaml` | `49374C56C6F08B4F7FE49334C964D7AD75698C40C040688FB6A0921C82380577` |
| `MediaCenterView.xaml.cs` | `FA020831621010C9F420153321068C41B2D62C80179C8576A0917FD259973F62` |
| `MediaComparisonSelectionTests.cs` | `C52FEDC6AC5C6D8E12016171F96657C012E30D1061364F1206D57C5D6E91D6CB` |
| `R18ThumbnailBudgetTests.cs` | `DAE17C4984979567DD12B3E267AEAC33C275565714DA562A51042C4CEAA1394D` |
| `FakeDashboardData.cs` | `0833546C1BE357562AB714A2F5CE2F16E1A3E47E0CB583ECA66A84AD23CD809A` |
| `Program.cs` (RenderHarness) | `7BB0E7CA012532B8ABFE820AED000B5A39CBB823F288E35BB88A9C316459A7A4` |
| Playnite plugin DLL | `753B5584E8E543CBCD5FE278ECEA08DE339A5943C64055069274BA0E28D9F464` |
| Playnite plugin PDB | `073AA320A4EA7E4DD8E8A9A564BDAA1ED99349DBD5A176799617ECD96D58C1E9` |
| Playnite test DLL | `427FC42D0745F94FC44118ABB8BC499E964AA3917737766628DFCCB8B6D5F5BE` |
| Playnite test PDB | `04AF6B4D28EEEADCD80CB5E522A8568F1E8C105E10577AC0A8CCDF049687AD7E` |
| RenderHarness EXE | `8A64C917E52023694864390D29C982D087304D0F262E61929877C0EE42C8E03F` |
| 比较工作区 TRX | `9FE712D8F4E8F3471879E58130A63139CB7A06587EBE625D3CB799E9B0622972` |
| 原图尺寸 TRX | `962FB483842B1F87FCFB27D9B713FEE25E3BB84928CE0518318A10D0C6F31933` |
| 离屏布局截图 | `648ACD52CE648E552CE0F96A89EC3CB355DEE713E61A47BF2BC9F00C0A78549D` |

## 验收边界

未运行完整 WPF 逐类隔离循环或真实 Playnite。合成渲染截图只用于布局检查，不代表宿主实机画面；未读取用户的真实媒体文件。同步缩放、拖动和候选切换仍属于后续 S04 任务。
