# T02-01 滚动累积漂移回归

测试直接加载生产 `DesignTokens.xaml`、`WpfUiProduction.xaml` 和 `Redesign.xaml`，创建含 240 行及两列长表格的 WPF DataGrid。在同一 STA 线程中分别验证 Light、Dark 主题；每个主题连续 8 轮执行顶端→底端→顶端，并交替滚动到水平两端，共记录 48 个检查点。

每个检查点记录实际垂直/水平偏移、行高、行头可见性、选中框位置尺寸和已实现单元格几何。单元格保持在行内，单元格纵向位置与选中框相对行的位置在循环间不漂移；滚动范围均为正。生产模板把 `DataGridRowHeader` 折叠，所以此处验证其状态，不报告不存在的行头几何。

SDK 8.0.423 Release 定向编译退出码 0（0 警告、0 错误）；定向测试退出码 0，1/1 通过。提交 `9fd1496c2e82cf57d3ad5e5bda4d28a953800189` 上的根目录 `GameSaveCenter-Run.cmd` 也完整退出 0：XAML 25/25、Release 编译 0 警告/0 错误、1955 项中 1898 通过、57 跳过、0 失败，148 份 TRX 计数一致，145 个 WPF 隔离类完成。57 项是已退役 Today Workspace 架构断言（Playnite source 18、WPF 资源测试 39），仍明确计作 skip。发布 ZIP/PEXT 哈希一致，安装的 6 个插件/Worker 程序集与打包暂存逐个哈希相同，Playnite 启动成功。完整身份与结果见 [standard-verification.json](standard-verification.json)，提交级 T02 用例观测见 [full-chain-targeted.trx](full-chain-targeted.trx)。

定向运行指标和初次定向测试/程序集/资源哈希见 [results.json](results.json)，48 条脱敏几何观测见 [targeted.trx](targeted.trx)。测试代码 SHA-256 为 `CEC630C0DA0DDCC63FD26BAD59497E098E454E51E8709CE3208F2B5A1E74E194`。

这是生产资源模板的 STA/WPF 行为回归；标准链证明安装版本和 Playnite 进程启动，未验证真实页面交互、物理滚轮输入或 DPI 变化。

归档两份最终 TRX 后，清理本批 990 个中间文件（86,156,284 逻辑字节），D 盘可用空间从 15,668,543,488 增至 15,757,197,312 字节。清理前逐一规范化路径、确认无链接/活动测试进程并独占打开文件。最新构建目录、包、标准诊断、安装记录与根入口日志保留；旧 Round3 TRX 和 `tests/.../TestResults/` 未触碰。清理清单与空间数据见标准验收 JSON。
