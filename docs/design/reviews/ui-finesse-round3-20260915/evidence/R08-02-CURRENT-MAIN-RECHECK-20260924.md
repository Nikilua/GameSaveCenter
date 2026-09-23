# R08-02 当前 main 热关闭动画复核（2026-09-24）

## 结论

当前 main 上设置动画开关的热关闭行为通过。生产和测试程序集均在 `9002668cafa6fe1a39272cf2b998ce95d752418e` 身份重建；R08-02 使用现有 `GscMotion` generation guard、Dispatcher 隔离弱引用登记、Settings 动画开关与 shell 生命周期，不新建一套动效服务。

## 构建与串行回归

- Release solution 构建成功：XAML `24/24`、0 errors，保留两条既有 `MediaCenterView.xaml.cs:703 CS8602` warnings；Playnite `net462`、测试 `net472`。
- 按测试类分别执行 VSTest，避免 STA/动画时序互扰：`R08MotionHotChangeBehaviorTests 1/1`、`R08MotionReverseBehaviorTests 2/2`、`ProductionShellChromeSourceTests 12/12`、`UiFinesseFoundationTests 9/9`，总计 `24/24`，各运行 0 failed/skipped 且 exit `0`。对应四份当前身份 TRX 已并列归档。
- 一次使用前一提交 `4b7f0a34` 的测试程序集时，生产源码读取测试按身份保护拒绝读取 `9002668c` checkout（7 个门禁失败）；这不是产品断言失败。随后在当前 HEAD 完整 Release 重建并按类串行复跑，上述 `24/24` 全部通过。

## 行为结果

- 设置页：真实 `GameSaveCenterSettingsView` 入场时确认 SettingsShell opacity 与 translate 时钟运行；关闭动画选项后两时钟立刻释放，opacity=`1`、Y=`0`。重新开启并等待原动画长度后，不会重新触发旧入场动画，仍为中性状态且无活动时钟。
- Translate 反向：实际中点/接管起点均 `9.566`，最后到达 `-8`；旧 clock 清理。生产 shell 侧栏收起中点/反向起点 `185.333/185.333`，反向中点 `209.333`，最终宽度 `270`、opacity `1`，transition 与 opacity clock 清理。
- reduced-motion 和卸载的现有 sidebar 行为、基础动画 clock 与资源 fallback 均在相邻回归通过。

HotChange TRX shutdown 有 8 段、Reverse 有 1 段、ShellChrome 有 1 段 `TextServicesHost.OnUnregisterTextStore InvalidComObjectException`；MotionFoundation 未记录。根因未知，所有当前身份测试运行仍明确 exit `0`。

## 验收边界

系统动画偏好没有被修改；只改变合成设置对象的动画选项并由真实 WPF 控件响应。测试是隔离 STA Window/逻辑 DIP，不表示真实 Playnite、系统设置即时切换、物理 DPI/跨屏、输入设备、UIA/读屏、DWM presented frame、ETW 或宿主性能。没有访问真实存档、媒体、云端或诊断。

**R08-02：当前 main 上已满足可控热关闭动画行为。下一项 R08-03 离屏与隐藏停机。**
