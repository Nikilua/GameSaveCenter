# 控件设计目录

每个公共控件覆盖：Normal、PointerOver、Pressed、Focused、Disabled、Selected/Checked、Validation error、Light/Dark/High Contrast。

## Button

类型：Primary、Secondary、Ghost、Destructive、IconButton、More。

- 高度 40–42
- 图标 16–20，图文间距 8
- 水平和垂直内容居中
- 同一工具栏按钮同高
- 图标、文字、数字组合整体居中
- 卡片型入口除左侧图标外，其余内容在剩余空间对齐/居中
- 删除无意义的 `>`，仅明确导航下一层时保留

## CheckBox / RadioButton / Toggle

CheckBox 建议 16–18，圆角 4–5，选中 Accent 背景和矢量勾。表格单元格内水平垂直居中。开关用于持续设置，CheckBox 用于多选或明确布尔选项。

## TextBox

高度 42–46；统一内边距；聚焦显示 Accent 边框/Focus Ring；错误有可见文案；不以真实 Text 模拟 Placeholder。

## SearchBox

- 左侧搜索图标
- 真 Watermark：`Text 为空 AND 未聚焦`
- 聚焦后 Watermark 消失
- 有文本时显示清除按钮
- 清除后保持焦点
- 清除按钮垂直居中
- 支持无结果状态

## ComboBox

完整模板需要覆盖 ToggleButton、Selected content、Chevron、Popup、ScrollViewer、Item、Focus、Disabled。

- 默认值可读，如“全部”
- Popup 至少与主体同宽
- 长选项省略 + Tooltip
- 展开时箭头可轻微旋转
- 深色主题不泄漏白色宿主模板
- 保留 ExpandCollapse Automation

## Pill Tabs / Segmented Control

- 四角完整圆角
- Selected 使用 AccentSoft + Accent 文本
- 不只用下划线
- 只在 Header 模板内部居中
- TabItem 内容保持 Stretch，避免整页居中
- 页签间距放模板内部，避免 TabPanel 裁切
- 过多时横向滚动或溢出菜单

## ScrollBar

- Thumb 是单一圆角几何形状
- 两端对称
- 大数据时保持可拖动最小长度
- 不出现重叠端帽、尾线、尖点
- Track 可透明但保留翻页行为
- 横向/纵向分别定义模板
- 不盲用 999 圆角
- Track 内留安全边距
- 测试顶部、中部、底部和数千项列表

## ListBox / ListView

启用虚拟化和 Recycling；Selected 前景显式；Hover 与 Selected 不冲突；大列表不为每行添加 Effect/Storyboard；滚动时行高稳定。

## DataGrid

详见 `DATA_DENSE_UI.md`。

## ProgressBar

进度条与百分比同一水平线；百分比保留最小宽度；条使用剩余宽度；0%、100% 完整显示；失败/完成不只靠颜色。

## Slider

Thumb 足够大；轨道低对比；显示值或 Tooltip；支持键盘；不使用极细难点击轨道。

## Card

卡片用于分组，不是所有元素默认包装。点击卡片有 Hover/Focus，非点击卡片不要伪装成按钮，避免完整边框卡片嵌套。

## Navigation

一级导航稳定；当前项有背景/指示条/文字变化；紧凑模式图标有 Tooltip；页面 Tab 不重复一级导航。

## ToolBar

常用动作前置，次要动作进入更多；同组同高；避开窗口控制区；窄窗口 Wrap/Overflow，不重叠。

## Dialog

标题、说明、风险和按钮层级明确；主按钮右侧；危险按钮不默认聚焦；Esc 取消；支持复制技术详情。

## Toast

不抢焦点；成功/信息 2–4 秒；错误更久；最多 3–4 条；悬停暂停；只动画 Opacity + TranslateTransform。

## Tooltip / Popup / ContextMenu

统一浮层表面、圆角和阴影；Tooltip 不替代必要标签；路径可换行；Popup 不越过安全区；保留键盘导航。
