# T02-01 滚动累积漂移回归

测试直接加载生产 `DesignTokens.xaml`、`WpfUiProduction.xaml` 和 `Redesign.xaml`，创建含 240 行及两列长表格的 WPF DataGrid。在同一 STA 线程中分别验证 Light、Dark 主题；每个主题连续 8 轮执行顶端→底端→顶端，并交替滚动到水平两端，共记录 48 个检查点。

每个检查点记录实际垂直/水平偏移、行高、行头可见性、选中框位置尺寸和已实现单元格几何。单元格保持在行内，单元格纵向位置与选中框相对行的位置在循环间不漂移；滚动范围均为正。生产模板把 `DataGridRowHeader` 折叠，所以此处验证其状态，不报告不存在的行头几何。

SDK 8.0.423 Release 定向编译退出码 0（0 警告、0 错误）；定向测试退出码 0，1/1 通过。运行指标和测试/程序集/资源哈希见 [results.json](results.json)，48 条脱敏几何观测见 [targeted.trx](targeted.trx)。测试代码 SHA-256 为 `CEC630C0DA0DDCC63FD26BAD59497E098E454E51E8709CE3208F2B5A1E74E194`。

这是生产资源模板的 STA/WPF 行为回归；未验证真实 Playnite 页面、物理滚轮输入或 DPI 变化。提交后的标准完整链仍待执行，完成后会在证据账本中绑定确切源码提交。
