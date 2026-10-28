# U08-05 / U08-06 当前工作树复核

日期：2026-10-29。Git 基线为已推送提交 `d6d940e70c24d87a742882cbd052c45cd9926c8f`，本次验证在其上的未提交工作树执行；不能把该提交的 CI 结果称为本次源码变更的远端验证。

## 页面行为

- U08-05：工具卡继续显示来源、文件可用状态和自动启动/手动启动说明。此前点“启动”后缺少专属进行中状态；现在 `IsLaunchingGameTool` 在启动请求期间为真，并在成功、失败或取消退出时由 `finally` 清除。启动按钮使用已有忙碌指示器，页面显示“正在启动所选工具…”，并提供 UIA 名称/说明。退出时关闭工具的开关仍由 `CanTrackProcess` 决定，禁用原因继续同时提供禁用态工具提示和 UIA HelpText；没有改启动命令、风险分类或关闭策略。
- U08-06：维护诊断摘要仍在诊断概览页显示；选择的问题详情保留独立渐进展开区域，低频诊断命令保留折叠分组。新增 1160×520 DIP 真实 WPF 窗口回归：诊断页可纵向滚至摘要底部，长摘要自身保留纵/横滚动；回到操作卡后，复制按钮可见且仍执行绑定命令。

## 验证

- SDK 8.0.423 / Release 定向行为组合 5/5 通过，0失败、0 skip：[TRX](tests/U08-05-06-focused.trx)，SHA-256 `F9E724F0D1AC9CC9D30741FB86532D72942D9407572066E0B3A3096ADC285856`。其中包含工具状态开关、长窗维护页摘要/复制路径、原有维护折叠 Inspector 和工具卡语义状态。
- 第一轮 WPF 夹具诊断保留：[失败 TRX](tests/U08-05-06-state-fixture-diagnostic.trx)，SHA-256 `D2A2D5C926BC5224B829BAB1AEEDF13024D36EC6C4ABF9DF64E8A5E3F28FF4B3`。失败是测试未选中工具、未绑定维护主标签，不是生产行为；修正夹具后最终 TRX 全部通过。目标 Playnite 框架不支持 `AutomationProperties.LiveSetting`，实现改用受支持的 Automation Name/HelpText。
- `python scripts/validate-source.py` 退出0；WPF 静态检查42个XAML、0错误、26项warning、179项info（与已有全库基线一致）。测试构建出现 NuGet vulnerability feed `NU1900` 连接 warning；编译与测试仍完成并通过。
- `scripts/render-qa.ps1` Release，固定样本时间 `2026-10-28T00:00:00Z`：双主题394 PNG、0 `PROBLEM`，RenderHarness 0警告/0错误。报告为工作树证据，标记 Commit `d6d940e7`、`WorkingTreeClean: False`、逻辑DPI 1.00：[完整报告](render-qa-report.txt)，SHA-256 `CBB1499A3DDFBE5C8FE8B2FB2C0BA71B8FF32500E9936A43AC154DC4DBE88508`。图像位于本机 `.tmp/round5-u08-05-06-render-d6d940e7/`，不将工作树截图冒充提交后截图。

清理测试中间产物：归档最终通过TRX和一份夹具失败诊断后，删除本批自有 `.tmp/round5-u08-05-06-launch-state/`，共7个临时TRX、55,487字节。删除前确认规范化路径位于仓库`.tmp/`、无文档引用、无运行中的测试/渲染进程。C盘空闲空间 2,612,453,376→2,612,453,376 字节；D盘 11,333,373,952→11,333,443,584 字节。保留当前Render QA图片目录53,276,915字节，作为本基线引用的394张审计图。

## 未验边界与 CI

以上窗口来自本机合成 WPF 夹具，不代表真实 Playnite 宿主、Narrator/外部 UIA、物理键鼠或跨屏 DPI 验收；真实启动工具未执行。提交本批源码前，上一已推送 SHA `d6d940e7` 的 run `40731004737` 中 SDK10 job `122966980155` 已成功、artifact `12835609657`；基线 job `122966980646` 仍在生成扩展包。此结果不覆盖本工作树变更；本批 commit 后按新 SHA 查询基线 SDK、SDK10 job 和 artifact。
