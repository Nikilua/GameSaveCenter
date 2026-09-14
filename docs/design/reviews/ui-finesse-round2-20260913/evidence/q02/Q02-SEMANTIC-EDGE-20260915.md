# Q02-04 零值、未知值与未检查状态证据

采集日期：2026-09-15（Asia/Shanghai）。证据来自提交 `a26ef497ddbe3ca87b6b7b171cf0e6f411c5e2a7` 的 clean-tree `RenderHarness finesseprobe ... edgevalues`；报告身份为 `WorkingTreeClean=True`、`DpiScale=1.00`，窗口为 1120×980 DIP。

## 双主题截图与报告

- [深色 edgevalues 截图](edge-20260915/ui-finesse-edge-dark.png) / [报告](edge-20260915/ui-finesse-edge-dark-report.txt)
- [浅色 edgevalues 截图](edge-20260915/ui-finesse-edge-light.png) / [报告](edge-20260915/ui-finesse-edge-light-report.txt)

两张截图的第四行样本均明确显示：

- `0 B` 是真实零值，不与未知值混用；
- `未知大小` 是未知尺寸文案，不被格式化成零；
- `尚未检查` 表示未发生检查，不伪造时间；
- `文件 0/0 · 大小 0 B/0 B` 保留真实零的数量与大小语义，分隔符两侧可读。

夹具报告同时记录 `SemanticEdgeValues`、双主题对比 0 violation、行完整性 4/4 与压缩视口负例 3/4。该证据只签收受控表面上的语义可见性；业务入口盘点、真实字体/DPI、排序键和宿主状态切换仍按 Q02-04 边界待验。
