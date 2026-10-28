# U07-04 / U07-05 菜单导航与通知到达焦点

2026-10-29 小批，基于生产源码 `a89b29db`，本批新增两组 WPF 行为测试，没有更改生产 XAML、菜单模板、通知 DTO、视图模型、命令或业务逻辑。

## U07-04 菜单与快捷键列

真实 `DataGridColumnVisibilityController` 与生产菜单资源下验证列菜单稳定次序、必需列禁用状态、禁用提示的 Automation HelpText、菜单项 UIA 名称、可用项焦点能力，以及模板 `ItemsPresenter` 的 `KeyboardNavigationMode.Cycle`。SDK 8.0.423 Release 定向类 5/5、0失败、0跳过：[TRX](tests/U07-04-column-menu.trx)，SHA-256 `04136F0F7AF061F48042FBFCFD9CADBEAC576F751214524BBEC1C29393013004`。

这个列菜单当前只包含勾选列名和“恢复默认列”；它没有图标、快捷键或子菜单数据，本轮没有添加占位列。另有全局键盘帮助弹层的手势/说明双列，沿用 R10 已有可用命令筛选和焦点回归。尝试在测试中直接打开 Popup 并调用 `MenuItem.Focus()` 时，WPF 保持焦点在拥有按钮上，即使菜单项已可见、加载、启用且可聚焦；这类程序化打开无法证明真实输入路径。该失败探针已留存：[TRX](diagnostics/programmatic-menu-focus-probe.trx)，SHA-256 `DADD2CF1F3816EAFE442D1117937A47B3D57218057738A9A7AED5256073980A8`。因此方向键实体遍历、Narrator/Playnite宿主仍待验。

## U07-05 通知卡片与消息到达

使用真实 `NotificationInboxView`、DataGrid 与绑定数据，在搜索框持有键盘焦点和旧通知选中的状态下，于列表首部插入新通知；验证更新不抢焦点、不替换旧选中行。SDK 8.0.423 Release 定向类 4/4、0失败、0跳过：[TRX](tests/U07-05-notification-focus.trx)，SHA-256 `B56173C73866CCD764DB87C177B98673FB981C2022BFBBFBE8DB5D674530D632`。

`NotificationRecordDto` 目前没有来源字段；现有级别、时间、标题、摘要和任务/游戏关联信息保持不变，本轮没有从消息文本或关联对象猜来源。来源契约完善需独立数据设计；数据库/Worker 实际刷新及 Playnite 宿主仍待验。既有 U07-05/06 双主题 Render QA 对应未改动的生产视图，本批仅改测试，未重复渲染。
