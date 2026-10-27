# U01-07/08 文案长度与文本缩放

## 去重与结论

- **U01-07 中英文文案长度：已满足。** 生产 `GscWpfUiButtonTextTemplate` 已使用 `TextWrapping=Wrap`、`TextTrimming=None` 和完整文本 Tooltip；`R03BilingualLengthTests.SharedActionTemplateWrapsCompleteLabelsInsideANarrowSlot` 覆盖长英文/中文操作名在窄槽位完整换行。路径预览与可选完整路径、错误详情、表格复制快捷键、只读技术文本选择由 `R03LongPathTests`、`R15TaskFailureCopyTests`、`R06ClipboardBehaviorTests`、`R21SelectableTechnicalTextBehaviorTests` 覆盖，本轮不重复造控件或提示系统。
- **U01-08 文本缩放与选择：代码完成待验收。** 原 `R03TextScaleTests` 只使用了24/20 DIP样例，不能证明相对生产字号达到200%，也没有验证按钮文字字号或完整技术文本选择。本轮把测试夹具提升为真实生产字号的两倍：Body 14→28 DIP、Caption 12→24 DIP、操作按钮生产基准12→24 DIP；并测量可滚动技术详情、表头和输入框，调用 `SelectAll()` 并核对所选全文。测试没有引入Viewbox缩小。
- 生产页面和共享资源没有改动；本批交付是去重结果与更严格的回归夹具。200%逻辑字号的离屏WPF验证不等于真实Playnite宿主、UIA/键盘/IME或物理DPI验收。

## 验证

- SDK：8.0.423；源代码基线提交：`e23f3a384485142de45f51a58377ac711f2f4de1`。本轮唯一源码改动为测试夹具 `tests/GameSaveCenter.Playnite.Tests/R03TextScaleTests.cs`。
- 定向七类行为：21 passed、0 failed、0 skipped。TRX：[`U01-07-08-focused-final.trx`](tests/U01-07-08-focused-final.trx)，SHA-256 `55c565ce892924cd6078c194619a0427bf5040f61ed830cc33c89f0fc0f7a8cb`。
- 200%文本缩放专项：2 passed、0 failed、0 skipped。TRX：[`U01-07-08-text-scale-200-final.trx`](tests/U01-07-08-text-scale-200-final.trx)，SHA-256 `0bd07f5a631e6ac540aa802a124952cb0f83826c61a9977f3800a39dd101fce5`。
- 首两次校准TRX也保留：测试先后暴露按钮实际生产基线为12 DIP，而非猜测的14 DIP；更新到24 DIP后专项通过。失败记录不计入通过数，详见[`tests-manifest.json`](tests/tests-manifest.json)。
- 本批没有生产XAML/资源改动，因此没有另跑RenderHarness、render-qa或WPF静态审查；相关生产呈现代码由现有测试直接加载。整轮UI门禁仍要求最终候选的完整构建链、独立 `scripts/build.cmd`、GitHub适用job及宿主验收。

## 未验边界

真实Playnite窗口中的200%系统文本缩放、物理DPI、多语言字体回退、UIA导航和键盘/IME交互仍待宿主验收；不触碰真实存档恢复、媒体删除或云端写入。Round5 GitHub push-run状态目前受查询权限限制，整轮远端门禁见相应批次记录。
