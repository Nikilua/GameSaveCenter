# Q13-05 共享滚动条 Thumb 行为（2026-09-29）

## 复核与行为证据

生产共享模板已在 `Themes/DesignTokens.xaml` 使用纵/横两种方向模板：Thumb 由单个圆角 `Rectangle` 绘制，内部边距对两端对称；纵向轨道上下各留 4 DIP、横向左右各留 4 DIP；有效 Thumb 最小长度为 36 DIP。共享 `GscScrollThumb` 状态使用动态主题资源，未更换 ScrollViewer、`ScrollUnit` 或现有滚动系统。

提交 `ae35be70` 新增生产 WPF 控件级 STA 行为回归。`Q13ScrollBarThumbBehaviorTests 1/1` 在 Light/Dark 下分别检查纵/横向 ScrollBar：最小可拖长度、滚动值两端移动到轨道相反端、轨道剩余边隙闭合、单一 Thumb 绘制形状与端帽/边距对称，以及 WPF Hover 进入/离开时动态主题刷切换和恢复。TRX：[scrollbar-thumb.trx](scrollbar-thumb.trx)。

该改动只增加测试，没有改共享 XAML。精确提交 Release solution 构建为 XAML `24/24`、`0` warning、`0` error。插件 DLL SHA-256：`257DBBD778E1CDF560FAE3557278DD7C5EB500E067346D16639E891DC283BC68`；测试 DLL SHA-256：`40AC373D89A8ADB85A4D46E699B70941720A019DD9A105DDAC6EF4C485C24EDA`。命令：`scripts/build.ps1 -Configuration Release -SkipTests -OutputRoot .tmp/q13-05-ae35be70/build`；行为类以 `--no-build --no-restore` 在隔离 WPF 测试进程运行。

同一提交身份的既有 `MediaInboxScrollBehaviorTests 3/3` 通过；合成数据的顶部/中段/Thumb 到底/多轮往返、窗口缩放、首行锚定、末行完整和页尾命令可达继续通过，保留 `EnableRowVirtualization=true`、`ScrollUnit.Item` 和 `VirtualizationMode.Standard`。该夹具 `outputScale=1/1.25/1.5` 是 RenderTransform 尺度，不是 125%/150% 的物理显示器 DPI。测试退出时出现 WPF `TextServicesHost.InvalidComObjectException` 清理噪声，但 TRX 明确为 `3/3` 成功、VSTest exit `0`；根因未知。结果：[media inbox scroll WPF TRX](../MEDIA-INBOX-HOST-RECHECK-20260929/media-inbox-scroll-wpf.trx)。

## 未验证边界

Hover 由 STA 测试调用 WPF `MouseDevice.ChangeMouseOver` 驱动状态，没有发送 OS 物理鼠标；本次没有捕获显示器截图或真实屏幕像素。因此 Q13-05 自动行为证据已通过，物理 Hover/宿主栏仍未完成。

用户报告的 Media Inbox 滚动空白尚无宿主根因结论。当前隔离 Playnite 前置检查因读取进程命令行返回 Access Denied 而拒绝启动；没有运行中插件 DLL 身份、窗口 DIP、DPI/主题或真实滚动前后日志。最新复核和具体待验步骤：[Media Inbox 宿主边界](../MEDIA-INBOX-HOST-RECHECK-20260929/README.md)。
