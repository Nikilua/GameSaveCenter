# Q13-02 多选反馈与手势边界（2026-09-29）

## 已有实现与证据

当前生产 Media Inbox、Task Queue、Save History、Maintenance Findings 表格均使用 `DataGrid.SelectionMode=Extended`。Media Inbox 继续把实际 `SelectedItems` 交给批量命令，并以稳定 ID 保留跨有限窗口/视图选择；现有摘要说明总选数、当前窗口可操作数和隐藏项。

已有自动行为证据已覆盖摘要语义，不需要重建批量选择模型：

- `R05MultiSelectionSummaryBehaviorTests 3/3`：真实 WPF Media Inbox DataGrid 分别触发 0/1/2 项 `SelectedItems` 状态，核对摘要与清空按钮；合成隐藏 ID 与两条当前行组合为“已选 3、当前窗口可操作 2、隐藏 1”；点击清空后检查选择清空且“已忽略”视图不变。
- `R22BatchCountBehaviorTests 3/3`：合成策略目标验证筛选隐藏选择计数、空选择不全选与确切文案；隔离 WPF ListBox 选择验证当前媒体批量摘要和隐藏项分流。完整范围见 [R22-05 证据](R22-05-BATCH-COUNT-20260921.md)。
- `GscCheckBox`/`GscDataGridCheckBox` 的半选是控件视觉状态；当前生产 DataGrid 批量选择没有“当前页/全部结果”三态复选框消费者。`R05-05` 已按“不适用”处理，没有增加冗余的全选/半选业务模型。

## 尚缺的行为证据

以上测试通过 WPF 选择集合/API 或 ListBox 选择状态检查，没有向生产 DataGrid 发送 Ctrl/Shift 鼠标键序列；源码中的 `SelectionMode=Extended` 声明也不能证明主机里的修饰键、焦点和选区锚点行为。因此 Q13-02 只能确认批量摘要和选择范围说明已有证据，Ctrl/Shift 手势与跨有限页真实交互仍未验，最终结论继续为“未完成”。没有用伪造的 SelectionChanged 或 `Assert.Contains` 将手势写成通过。

目前不可执行真实 Playnite 手势的环境限制沿用既有隔离宿主记录：进程命令行查询 Access Denied，正常隔离启动遇到 CEF `platform_channel 0x5`。本阶段不使用系统级 `SendInput` 去抢占当前桌面焦点，也不绕过宿主门禁。

## 宿主恢复后的待验步骤

在隔离 Playnite 与合成媒体记录中执行并记录输入修饰键、焦点、当前行、anchor、稳定 ID 与摘要：

1. 单击第 2 行应只选中该行；按住 Ctrl 单击第 4 行应保留第 2 行并增加第 4 行，摘要为 2。
2. 按住 Ctrl 再点已选中的第 4 行应移除它，摘要回到 1；清空选择后摘要为 0，批量命令禁用。
3. 单击第 2 行后按住 Shift 单击第 5 行，应选中含两端的连续范围；再 Shift 到第 3 行，范围按 anchor 更新，不应残留旧范围外行。
4. Shift 到 anchor 上方行，应验证反向范围边界；滚动或追加下一页后，选择只保留可见/有效 ID，隐藏选择计数与可执行集合各自准确。
5. 以普通单击作为负例验证 Extended 手势之外的替换选择语义；切换“待归类/已忽略”作为范围负例，确认不会把另一模式的媒体误传给批量命令。

宿主复核前保留现有跨窗锚点/稳定 ID、命令参数和媒体归类取消/错误语义。无需真实存档、用户媒体目录或云端数据。

下一项继续 Q13-03 行内按钮命中与稳定行标识，优先用受控生产 WPF 视觉树/实际 HitTest 和 routed action 验证；真实主机鼠标命中仍分开记录。
