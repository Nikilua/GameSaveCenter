# R06-02 排序提示与稳定性：当前 main 复核

日期：2026-10-13（Asia/Shanghai）。复核提交：`90454586d702f6a2b413bfcc386ac599424f8bc1`。本轮复用已有稳定排序控制器、生产 profiles、DTO 和行为测试，没有重建排序服务或修改生产/测试源码。`DataGridStableSortController.cs` 与 `R06SortingBehaviorTests.cs` 相对 clean-main 复核提交 `3d83d38f` 无差异。

## 构建与当前行为结果

- 使用隔离本机 SDK `8.0.423`、官方 `scripts/build.ps1 -Configuration Release -SkipTests -OutputRoot ...` 在复核提交 `90454586` 重建；Release solution `0 warnings / 0 errors`，XAML `24/24`，Playnite `net462`、测试 `net472`。
- 首次误用 `afade3cd` 编译的旧输出时，`TestRepositoryContext` 身份保护拒绝了源码测试；该次 `6/7` 不作为结果。随后从 `90454586` 当前 checkout 重建并运行 `R06SortingBehaviorTests`，最终 `7 passed / 0 failed / 0 skipped`。
- 行为覆盖：Save History 原始时间/数值排序、同值 `BackupId` 稳定次键、未知值末尾；排序方向切换后按乱序清空/重加仍保持相同顺序；Task 进度按数值排序并将负值/排队中的 0 置于已知值之后，未知阶段末尾；Media 未知来源在升降序均位于末尾；实际 WPF `DataGridColumnHeader` Click 的升/降序及箭头同步；CollectionView 脱离后列头点击不抛异常、不伪造新活动列/箭头；生产 Save History/Candidates、Task Queue、Media Inbox 的 profile 接线与共享箭头模板契约。
- 已归档脱敏 [定向 TRX](tests/R06-02-current-main.trx)，SHA-256：`D553AC764D23B5DE2F2FFD9962BABDB450F6EC2C78B75BBFAD4F621AAA9927E7`。
- 源码 SHA-256：`R06SortingBehaviorTests.cs` `453CB93ECD6E5611011BD22F5E04A0ECD524D1ACD90484A3DC91486EEA08049E`；`DataGridStableSortController.cs` `7C319EDB5111B3A3CD9E76733B7896E1F44A66ED24DF5BBABD885731F2AD6036`。
- Release 插件 DLL SHA-256 / MVID：`F9800599D02B530B91CEE1ABABC4BA85977120CC5F72726AC77B1B38307A8EC6` / `959cb53c-050a-4cb9-b8fd-058d194448f5`。测试 DLL SHA-256 / MVID：`31C09510C9C364941C22677FB1DDBA671890560E0506C6335E4BEBC01CB5FACE` / `80392b61-b8ea-4a6b-a9f4-59e8bca69ffa`。

## 视觉与行为边界

历史 clean RenderHarness 的 Light/Dark `357` PNG、抽查图及 `render-qa OK` 仍绑定原实现提交 `c577afc5`，见[R06-02 原证据](../R06-02-SORT-STABILITY-20260918.md)。本次只做当前源码身份与定向行为复核，没有把旧图写成 `90454586` 的新截图，也没有声称在真实 Playnite 中显示或点击过表格。

测试使用合成 DTO、隔离 `ListCollectionView` 与 STA WPF Window。真实 Playnite/用户加载包、UIA/Narrator、物理 DPI/跨屏、最终呈现帧、ETW 和宿主性能仍未验；未读取/写入真实存档、媒体、用户云端或诊断数据。

下一项：R06-03 选中焦点区分。对照共享 `DataGridRow`/`ListBoxItem` 资源与现有行为夹具，复核选中、键盘焦点、悬停、失焦选择、错误行及徽章/危险动作的浅深主题边界。
