# T02-02 详情交叉滚动覆盖审查

既有可复用运行时测试：

- `CompactWorkspaceTableViewportBehaviorTests.SaveHistoryAndPathTablesKeepFiniteInnerScrollbarsInsideScrollablePages`：生产 SaveCenterView，打开历史/候选 Inspector，900×600 DIP，表格内滚动条、详情有限高度、页面滚动及相对布局。
- 同类 `PageSizingTracksCurrentHostHeightAndKeepsDetailsReachableAfterResize`：主表与页面末端可达，宿主高度变化后重新测量。
- `R07ScrollOwnershipBehaviorTests`：合成嵌套滚动面、DataGrid 在边界移交滚轮；不是生产长详情整页交叉操作。
- `MediaInboxScrollBehaviorTests`：生产媒体列表反复滚动和缩放，但不等同于同时打开长详情。

仍缺直接证据：同一生产页面在长详情打开时，按列表→页面→详情顺序交叉滚动，分别验证偏移所有权、表格滚动条与详情边界、末行和底部真实动作可达；Light/Dark需在同一测试身份下记录。下一步扩展已有生产页面夹具，而非重复合成滚动面测试。

本次仅审查代码，不新增运行通过结论。既有标准全链 run `20261023T150002369Z-f20509ae` 可查询这些类的原始TRX，但不证明上述缺失的复合场景。真实物理滚轮和宿主DPI仍需独立验证。

2026-10-24新增生产SaveCenter长详情夹具通过：Light/Dark两种主题2/2，所在类9/9。32行、900×600 DIP，三滚动面偏移独立及底部可见动作可达，生产布局未改。见 t02-02-themes.trx 与 t02-02-class.trx。尚未覆盖物理滚轮与宿主DPI，最终完整链待跑。
