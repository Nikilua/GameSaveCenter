# U00-07 / U06-04 媒体收件箱工具栏小批

实现提交：64f26f58153efdb4a634bf8a0aa7c832898a581d（已推送 main）

## 前后变化

- 1040×700 Playnite 壳层的媒体批量操作从两行收紧为一行；模式选择、当前游戏目标、批量归类和详情入口保持可见，清空选择和重置列宽移至原有可滚动页脚。
- Before/after Render QA 均为 SDK 8.0.423、Release、Light/Dark、固定 UTC 2026-10-28T09:00:00Z，各生成394张PNG。1040×700中页面表格起点从 Y=376 DIP 移到 Y=300 DIP（上移76 DIP）；原 PageHost/滚动视口和 DataGrid 尺寸保持 726×570 / 710×423 / 663×267 DIP。after行为夹具确认批量工具栏一行、两种主题各四个完整数据行可见，次要操作在滚动后可达。
- 视觉复核的是壳层Before/after和Light/Dark媒体页样张。列、命令、Binding、Automation名称、ToolTip及已有滚动条行为保留；没有执行媒体删除。

## 验证

- SDK 8.0.423 Release定向编译：exit 0，0 warning / 0 error；其身份变量为父提交40fd0bc（编译发生在提交前），不把它写成64f26f5提交后本地构建。
- 相关行为类：218 passed，0 failed，39 skipped。39项是撤销的“今日工作台”架构既有skip，不计入通过。TRX、日志及类别计数见 tests-manifest.json。
- post-commit Render QA：commit 64f26f58153efdb4a634bf8a0aa7c832898a581d，WorkingTreeClean=True，render-qa OK，双主题各394 PNG；离屏逻辑DPI=1.00。
- source validation与生产Views WPF静态审查的原始输出见相邻文本文件。当前子树静态10 XAML/0 errors；原有23 warnings和4 info留存。
- GitHub run 41041454086（#1420）针对精确SHA 64f26f5：baseline SDK job 124014997522与SDK 10.0.401 job 124014997301全部步骤成功，三个artifact的head SHA相同，ID/digest见 ci.json。
- 条件分支保护required-check名单仍因此前403权限拒绝记“远端待验”；不推断两job是否为required。

## 失败诊断与边界

首次全量本地测试的3个设置恢复测试因系统TEMP路径过深失败；短TEMP复跑解决了这3项。中间布局版本两主题的1040壳层行数断言失败，随后保留原谓词并修正实际动作行；相关TRX在 diagnostics/。没有放宽断言或skip。构建/行为检查来自提交前最终工作树；精确SHA验证由提交后的Render QA及双SDK GitHub run提供。真实Playnite页面、外部UIA客户端、实体键盘/IME和物理DPI未验；真实存档恢复、媒体删除、云端写入未运行。页面仍为“代码完成待验收”。

## 证据归档与阶段清理

最终证据目录含40个文件、5,254,737字节；其中先归档的39个文件为5,253,096字节，清理清单在归档后新增，占1,641字节。`cleanup.json`记录准确删除的本阶段自有`.tmp/round5-media-first-view-20261030`、1,853个文件/290,968,319字节、边界与占用检查，以及C盘空闲4,338,810,880→4,633,923,584字节、D盘未变。其它构建、包、SDK及用户/其他任务测试产物未清理。
