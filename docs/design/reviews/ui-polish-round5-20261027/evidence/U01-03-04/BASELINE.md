# U01-03 / U01-04 既有数字节奏与长路径入口核对

日期：2026-10-27。核对源码提交 `bb984a070c92b61d06e1045cf64dfa07811e64a8`。本批没有生产代码增量：现有共享格式和交互已覆盖任务要求，避免重复建设。

## U01-03：数字、单位与时间

- `GscTypographyNumericCell` 使用 Tabular 数字、右对齐、不换行且不裁切；存档文件数列固定 82 DIP，大小列固定 116 DIP（最小78 DIP）。值从9变为100或由 KiB 变为 MiB/GiB 时，文本长度会按实际内容变化，但DataGrid列边界不会随数据变动。
- `SaveHistorySizeColumn` 绑定既有 `SizeDisplay`，由 `ByteSizeFormatter` 按1024进制输出B、KiB、MiB、GiB。容量测试覆盖0/1/1023 B、1/1.5 KiB、1 MiB、1 GiB，仍保持旧格式化语义。
- 时间列固定110 DIP，使用共享 Tabular 数字时间样式；本地相对时间可截断时仍有完整本地时间和原始UTC值的Tooltip/HelpText，复制行保留完整本地时间。百分比样式沿用同一Tabular数字基础；存档候选评分使用130 DIP固定列和40 DIP百分比槽位。
- 现有混排报告记录数字样本 1、8、99、100、时长 `00:09` / `12:59`、`59 秒` / `1 分钟`，同时显示统一字形尺寸和稳定列锚点。见 [浅色报告](../U01-01-02/reports/finesse-light-report.txt) 与 [深色报告](../U01-01-02/reports/finesse-dark-report.txt)。

## U01-04：完整路径

- `PathDisplayConverter` 只生成72字符的单行预览，保留路径开头/盘符和最终文件名尾部；不会改写来源路径。`GscPathText` 负责单行省略，Tooltip只作补充。
- 存档候选、媒体、工具表格使用既有 `DataGridClipboardBehavior`：Ctrl+C复制所选完整行，Ctrl+Shift+C复制当前单元格原始值。详情中的只读路径文本框保留完整值并可选中，复制入口走既有 `CopyPathCommand`；完整路径可通过键盘操作获取，不依赖悬停。
- [U01截图索引](../U01-01-02/screenshots.json)及两张混排探针截图展示受限宽度长路径省略；行为TRX覆盖超260字符路径盘符/文件名保留、详情完整选择及剪贴板精确值。

## 验证

- [容量单位 TRX](tests/U01-03-capacity-units.trx)：13 passed，0 failed，0 skipped。
- [时间、混排、长路径和剪贴板 TRX](tests/U01-03-04-time-typography-path-clipboard.trx)：51 passed，0 failed，0 skipped。
- 当前源码SHA `bb984a07…` 的 [GitHub双SDK run](ci-run-40510427122.json) 两个适用job、render-qa、WPF静态审查、打包及artifact上传全部通过。

未验证物理跨屏DPI、真实Playnite宿主和未安装可选字体环境；路径恢复、媒体删除和云端写入均未执行。
