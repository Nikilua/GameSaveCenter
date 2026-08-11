# 数据密集界面与 DataGrid

## 何时使用表格

多列比较、排序、列宽调整、大量数据时使用 DataGrid；结构差异大、以浏览和操作为主时使用列表卡片。不要把所有信息都做成后台管理表格。

## 基础设置

```xml
HeadersVisibility="Column"
GridLinesVisibility="Horizontal"
VerticalGridLinesBrush="Transparent"
CanUserResizeColumns="True"
CanUserSortColumns="True"
EnableRowVirtualization="True"
EnableColumnVirtualization="True"
ScrollViewer.HorizontalScrollBarVisibility="Auto"
ScrollViewer.VerticalScrollBarVisibility="Auto"
```

## 对齐

居中：时间（可按项目左对齐）、数量、大小、类型、状态、进度、CheckBox、短枚举。

左对齐：文件名、路径、备注、错误详情、长说明。

## 列宽

不要所有列都用 `*`。

- 时间 140–170
- 数量 70–100
- 大小 90–120
- 类型 100–140
- 状态 100–120
- 进度 150–190
- 路径/详情 `*` + MinWidth
- 操作 Auto 或固定最小宽度

允许调整并可按表格 Key 持久化。

## 表头圆角

外层 Border 不自动裁切内部表头。采用真实 Clip Geometry、首末列表头圆角模板，或把表头放在独立圆角容器。检查背景不越过外框。

## 列宽拖动 Thumb

保留热区和 `SizeWE`，可见分隔线低对比；浅色主题不能显示突兀白色拉块。

## 行视觉

行高 44–48；轻水平分隔线；无粗竖线；Hover 很轻；Selected 使用 AccentSoft，前景显式为 PrimaryText；键盘焦点用圆角 Ring。

## 状态单元格

圆点和文字放同一容器，同一基线，整体居中。状态不能只靠圆点颜色。

## 进度单元格

```xml
<Grid>
  <Grid.ColumnDefinitions>
    <ColumnDefinition Width="*"/>
    <ColumnDefinition Width="44"/>
  </Grid.ColumnDefinitions>
  <ProgressBar Grid.Column="0" MinWidth="64" Height="8"/>
  <TextBlock Grid.Column="1" HorizontalAlignment="Right"/>
</Grid>
```

百分比保留 40–48 DIP；空间不足时横向滚动，不压掉数字。

## 长文本

TextTrimming + Tooltip；选中行详情面板显示完整信息；支持复制；路径可中间省略；原始错误代码放技术详情。

## 横向滚动

共享 DataGrid 使用 Auto，但还要确保关键列有 MinWidth、总宽度允许超过视口、不要强制所有列压缩。

## 性能

虚拟化和 Recycling；避免每行 Effect/Storyboard；Converter 不做 I/O；缩略图异步缓存；复杂详情按需加载；筛选避免频繁 Reset 全集合。
