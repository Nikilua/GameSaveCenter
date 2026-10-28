# U05-04 键盘说明弹层补齐

2026-10-28，基线 `498dd3a4`。这批修正上一批盘点后的生产缺口：KeyboardShortcutHelpPopup 的透明度与 Fade 硬编码，改用共享 GscPopupAllowsTransparency / GscPopupAnimation。打开后焦点进入带 UIA 名称的说明卡，Escape 关闭并回 HeaderKeyboardHelpButton。鼠标点外部仍沿用 StaysOpen=False；不新增关闭动画回调或业务确认。游戏选框及滚动条不变。

前后：原来说明弹层始终 Fade/允许透明，缺少 Escape 接线；现在跟随共享低透明度/Reduced Motion资源，实际打开后键盘可达，Escape 回到打开按钮。终态页面布局没有变化。

SDK8.0.423 Release：首次帮助专项7/7，最终组合16/16、0失败、0跳过、退出0。新增两种资源配置各连续开闭三次，验证实际 Popup 属性、焦点进入、Escape及回焦；资源配置是实例内受控输入，不宣称切换了Windows系统设置。组合还运行 CommandDirectoryKeyboardBehaviorTests 的打开/关闭/只执行一次/IME保护及任务详情的反向开合、Reduced Motion与队列锚点测试。共享对话框重复确认/退出/回焦沿用[同批上一提交9/9](../U05-03-04/BASELINE.md)。上述覆盖当前说明/命令层/对话框/任务详情的可控路径；真实宿主仍待验。

Source validation通过；XAML26/26；WPF静态37 XAML、0错误、26既有warnings/179 info。RenderHarness Release 0 warning/0 error，scripts/render-qa.ps1实际退出0，双主题394 PNG、0 PROBLEM、render-qa OK。报告见render-qa-report.txt，逐图身份见render-hashes.json。渲染编译发生在498dd3a4加本批工作树改动；其后只补说明卡UIA名称（无视觉差异），最终组合测试重新编译了此属性。未把候选渲染说成精确新提交身份。

本批当前审计完整截图保留在 `.tmp/round5-u05-help/render`，原始TRX和命令日志保留在同阶段目录。持久证据保存报告、逐图hash和脱敏TRX，不复制全量截图。disk-before/after记录空间；本批输出仍在使用，无已确认无用的自有旧包或目录，删除清单为空；未清理用户及其他阶段的TRX/TestResults。后续只按路径边界/引用/占用检查安全清理。

U05-04状态为代码完成待验收。提交后查询该SHA双SDK所有适用CI job，尚不能称远端通过。整轮最终中文根入口完整构建/测试/发布/打包/安装身份/启动、独立scripts/build.cmd、真实Playnite/UIA客户端/物理IME及DPI仍未验；真实恢复、媒体删除与云端写入未执行。
