# Q10-08 设置密集选项分组行为证据（2026-09-28）

## 结果

Q10-08 所需的生产设置页密集选项分组在当前实现中已满足本次可自动验证的行为条件。本阶段只补了测试，没有改生产视图或共享样式。真实 Playnite/package-host 的显示仍待核验，所以 Round2 任务保持未完成。

## 验证范围

在源码/测试身份 `53008d0b74670e599670e9c661228cc62d82f68b` 的 Release Playnite 测试程序集上，`ReportedWorkspaceLayoutBehaviorTests` 为 `16/16`，0 failed、0 skipped，VSTest 正常退出。构建该测试程序集为 `0 warning / 0 error`。TRX 留在本机 `artifacts/q10-08-dense-options-20260928/q10-08-reported-53008d0b.trx`；未提交原始日志，以免带入测试宿主路径信息。

新用例 `SettingsDenseOptionsKeepEachHelpTextWithItsOwnToggleAtNarrowSizes` 在 Light、Dark、FollowPlaynite 三种资源模式分别检查 1280×840、920×700、560×640 DIP：

| 窗口逻辑尺寸 | 媒体来源子项行数 | 安全模式行尺寸 |
| --- | ---: | ---: |
| 1280×840 | 1 | 774×34.67 DIP |
| 920×700 | 2 | 554.67×34.67 DIP |
| 560×640 | 2 | 419.33×50.67 DIP |

实际断言覆盖八个生产开关：标题仍在自己的轨道右侧，帮助文本紧随标题并留在同一开关行，行内不裁切，标题字重强于说明且使用不同前景色；五个媒体来源子项缩窄换行时仍处于组内、不互相重叠；页尾安全模式选项可完整滚入页面视口；关闭媒体同步后所有子项经 `INotifyPropertyChanged` 绑定禁用，再启用后恢复。测试未仅凭文案存在断言布局。

## 限制

`FollowPlaynite` 在此为测试夹具中的受控资源模式，不等于真实 Playnite 主题宿主；DIP 尺寸是 WPF 窗口逻辑单位。该用例没有记录物理显示器 DPI、OS 鼠标/键盘输入或真实宿主呈现，因此不以本次通过签收 Playnite/package-host 视觉或物理 DPI。没有读取/修改用户设置、存档、媒体或云端。
