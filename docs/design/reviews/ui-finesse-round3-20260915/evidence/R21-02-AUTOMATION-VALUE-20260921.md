# R21-02 控件名称与值（2026-09-21，部分收口）

## 范围与事实

- 先盘点现有 UIA/Automation 名称：Shell、媒体归类历史、任务预设和多数图标按钮已有稳定名称；本阶段没有重建控件、服务、DTO、命令或 Binding。
- 本批补的是语义明确且不改变业务值的生产名称：Dashboard 后台操作进度、Overview 的已匹配/需注意/任务完成进度、Maintenance 备份存储占用比例、Trainer 修改器下载进度，以及 TaskCenter 的任务状态/类型/游戏三个筛选器。提交 `1ca2d01d`。
- 新增 `R21AutomationValueBehaviorTests`：使用真实 WPF `AutomationPeer` 验证 glyph 按钮的名称来自动作名而不是字形，ComboBox 名称可读，`ToggleSwitch` 的 `IToggleProvider` 能从 Off 变为 On，`ProgressBar` 的 `IRangeValueProvider` 能读出当前值、最小值和最大值；另对本批生产 XAML 做对应契约核对。

## 验证结果

- R21-02 新增测试：`2 passed / 0 failed / 0 skipped`。
- 与 R21-01 相关的键盘、焦点、无障碍、生产壳层回归加本批测试：`33 passed / 0 failed / 0 skipped`。
- Release 隔离 source-copy 构建：Playnite `net462`、Tests `net472`，`0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671` `CS8602` warning；`validate-source.py`、XAML `24/24`、`git diff --check` 通过。WPF 技能静态扫描 `0 errors / 27 warnings / 177 info`，未新增对应警告。

## 未收口边界

- R21-02 仍为“实现中，待继续”：SaveCenter 外置标签 ToggleSwitch、更多页面的复合选择器/外置标签关联、DataGrid 内重复进度条和逐控件 UIA 状态/值还未逐项签收。本批不以 ToolTip、邻近 TextBlock 或 `Assert.Contains` 替代这些行为证据。
- 当前确认的是受控 WPF AutomationPeer 和生产 XAML 接线，不宣称真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、最终呈现或宿主性能已验证；未调用真实命令，也未写存档、媒体、云端或诊断数据。Demo 原目录不可用，main 用户改动未碰、未合并。
- 阶段隔离 source-copy/build 目录已在文档提交前按精确路径清理。
- 下一可执行小批量仍为 `R21-02`：先处理 SaveCenter 外置标签开关与更多复合选择器，再补状态/值的负例；完成后再进入 `R21-03` 验证错误播报。
