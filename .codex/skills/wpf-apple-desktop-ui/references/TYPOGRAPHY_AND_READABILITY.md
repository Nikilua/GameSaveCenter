# 字体与可读性

## 字体

优先系统 UI 字体：Segoe UI Variable / Segoe UI、Microsoft YaHei UI、Yu Gothic UI 和系统语言回退。不要捆绑 Apple 字体。

## 建议层级

- Page Title：26–30 Semibold
- Section Title：18–22 Semibold
- Body：14–16
- Caption/Metadata：12–13
- Button：14–15 Semibold
- DataGrid：13–15

中文主要信息通常不应低于 12。

## 对齐

- 标题、说明、路径、长文本左对齐
- 状态、数值、短枚举可在表格中居中
- 按钮图标和文字作为整体居中，并保持视觉基线
- 数值列可右对齐，但全项目一致

## 截断

```xml
TextWrapping="NoWrap"
TextTrimming="CharacterEllipsis"
ToolTip="{Binding FullText}"
```

重要错误不能只留省略号，必须可查看/复制完整内容。技术路径、ID、日志可使用等宽样式。

## 文案

优先用户语言：

- `MediaInbox` → `媒体归类`
- `LUDUSAVI_GAME_UNMATCHED` → `尚未匹配到 Ludusavi`
- C# 类型全名放技术详情，不作主要选项名

按钮使用明确动词：`立即备份`、`重新扫描`、`打开目录`、`复制详情`。

## 状态页面

必须设计：首次加载、加载中、无数据、筛选无结果、权限不足、服务不可用、失败、部分完成。空状态说明“发生了什么、下一步做什么”。
