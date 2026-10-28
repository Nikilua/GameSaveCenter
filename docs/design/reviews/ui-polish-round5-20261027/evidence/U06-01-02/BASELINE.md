# U06-01/02 表格节奏与叠加状态

2026-10-28，基线5ee7be00。本批仅修改行为测试，生产模板、密度设置和资源没有差异。

U06-01去重：共享GscRedesignWorkspaceDataGrid和GscStableDataGridRow已有统一行高/表头高、文字对齐与S01密度预设，表头排序Chevron拥有22 DIP预留槽。本批新增生产表头从未排序到升/降序时文字起点、宽度和表头高度不变、箭头不越界检查，保留原排序命令、完整Tooltip/UIA说明和缩放/密度令牌验证。没有另造密度设置。

U06-02增补：四个实际生产页Task/Media Inbox/Save/Maintenance × Light/Dark，检查悬停+选中+键盘焦点同时出现时选择底色、2 DIP焦点边框和几何不变；焦点移到外部后仍保持失焦选中底色。然后添加300个合成对象（共302），滚动至第220项，对Recycling页面断言相同DataGridRow实例的Item引用已经改变，再检查远处实现行无旧悬停、旧焦点或错误选中色。Media Inbox沿用Standard模式历史兼容例外，不为造复用证据更改其生产模式。

SDK8.0.423 Release初始表头/密度/状态组合16/16、0 skip；补齐真实滚动复用后的专项1/1、0 skip。TRX与hash见tests/。第一次只快照原始2行未观察到复用；随后仅比较容器存在曾通过，但增加Item引用变化断言后定位Save/Light视口616×15758、全部302行实现：独立页面夹具缺少真实Shell有限视口调用。现使用生产ApplyResponsiveLayout(1280,900)，保留实例与Item变化断言，专项通过。三份失败/探索结果均保留，不删除复用断言或替换Standard例外。

Source validation通过，XAML26/26；WPF静态37 XAML、0 errors、26既有warnings/179 info。无生产视觉差异，复用完全相同生产代码的[U05-07/08 Render QA](../U05-07-08/BASELINE.md)：双主题394 PNG、0 PROBLEM。本批未生成新截图或运行新的完整安装链，不把继承证据写作当前测试提交的Render报告。

悬停由受控MouseDevice状态驱动，键盘焦点为实际STA窗口控件焦点；未验证物理指针、系统键盘/IME、UIA客户端、真实Playnite或实体DPI。两项保持代码完成待验收；提交后查询精确SHA的所有适用双SDK CI。最终中文根入口完整构建/测试/发布/打包/安装身份/启动与独立scripts/build.cmd仍待整轮最终候选。

disk-before/after记录空间；本批仅本阶段TRX和脱敏证据，无新包/截图/脚手架。保留当前成功和用于定位夹具问题的失败诊断，无已确认可删的本批旧目录，删除清单为空；用户及其他阶段TRX/TestResults、存档和配置未动。
