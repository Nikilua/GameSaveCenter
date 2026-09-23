# 设置顶部错位截图复核

## 当前生产视图行为

本批复核 `GameSaveCenterSettingsView` 的真实 WPF 控件树。测试不再通过反射直接调用 `ApplyResponsiveLayout`；它让控件进入 `Window`，由生产 `Loaded` 和 `SizeChanged` 事件更新布局，再在 STA Dispatcher 上完成布局测量。生产设置视图的搜索框左锚点修正在 `3a1dadd8` 已存在，本批没有改生产 XAML 或设置业务代码。

`ReportedWorkspaceLayoutBehaviorTests.SettingsHeaderAndPathActionsStayAnchoredToTheirLabelsAndEachOther` 在 Light/Dark 两主题均通过，合计 `2/2`，0 failed / 0 skipped。Release 构建的 Playnite 目标为 `net462`，测试目标为 `net472`；保留既有 `MediaCenterView.xaml.cs:703 CS8602` nullable warning。实际 TRX 为 [settings-header-event.trx](settings-header-event.trx)。测试程序集构建身份为当时 main HEAD `62b17b0c`；生产 Settings 实现相对前次证据身份没有代码改动。

### 顶栏重置控件几何补测（2026-09-23）

随后把用户截图中的顶部图标和“恢复默认”控件也纳入同一生产视图行为用例。Light/Dark 两主题再次通过，新增实测为：图标与标题左边缘横向间距 12 DIP；重置下拉框及两个按钮均 36 DIP，三者中心差 0 DIP，中心线均为 71.33 DIP。控件宽度为 260、90.67、102.67 DIP。测试还断言两个预期按钮都存在、控件同高且中心对齐。

本次 TRX 为 [settings-header-controls-geometry.trx](settings-header-controls-geometry.trx)，结果 2/2、0 failed / 0 skipped。Release Playnite net462 与测试 net472；构建保留既有 MediaCenterView.xaml.cs:703 CS8602，恢复时 NuGet advisory 源不可达有 NU1900 warning，项目恢复/编译成功。构建元数据 GscBuildCommit=13442aa4，测试包含当时工作树中的新增断言。

有两次不带诊断 console logger 的隔离调用长时间没有返回或生成 TRX；均不计为测试结果，仅中断了本次启动的 dotnet test 会话。随后使用同一隔离 Release 程序集和诊断 console + TRX 双 logger 复跑，5 秒返回 2/2。最终 TRX 未记录 InvalidComObjectException。

## 几何与负例

测试窗口尺寸均为逻辑 DIP，包含 `1280×840` 初始窗口、`1880×1200` 宽窗口、`560` 紧凑窗口和恢复到 `1280×840`。两主题的结果相同：

| 检查 | Light / Dark |
| --- | ---: |
| 标题图标上沿差 | `11.33 DIP`（门槛 `≤12`） |
| 图标到标题的水平间距 | 12 DIP |
| 标题到搜索框左边界 | `0 DIP` |
| 宽窗口 SettingsShell / 搜索左边界差 | `1360 DIP / 0 DIP` |
| 搜索框宽高 | `520×36 DIP` |
| 紧凑窗口搜索宽度 / 溢出 / 左边界差 | `392 DIP / 0 DIP / 0 DIP` |
| 保存提示所在行（紧凑 → 恢复） | `1 → 0` |
| 恢复默认卡左边界 / 与搜索间距 | `0 DIP / 14 DIP` |
| 恢复默认下拉框与两个按钮中心/高度差 | 0 DIP / 0 DIP，三个控件均 36 DIP |
| 路径组合框与四个按钮的高度 | 全部 `36 DIP` |
| 路径控件中心差 / 高度差 | `0 DIP / 0 DIP` |

负例把搜索框临时设置为居中；WPF 实测它会离开标题左边界 `287.33 DIP`。测试随后恢复左对齐并验证实际 bounds 为 `0 DIP`，所以能检测用户截图中的搜索框居中回归，不是只检查 XAML 字符串。

## 结论与宿主边界

仓库当前生产设置视图在 STA WPF 窗口中的 Loaded、缩放、紧凑与恢复布局均通过。用户截图表现与居中负例的方向一致；这只是现象对应关系，用户没有提供安装包/扩展 build identity，不能据此断定其正在运行旧包。该行为测试使用 `DpiScale=1.0` 的隔离窗口，不能代替 Playnite package-host 的最终布局或物理 DPI 观察。

前次隔离 Playnite host 仍因 CEF `platform_channel` `0x5` 未正常进入扩展视图。本项不标记为用户宿主已验。下一步如能获得当前包身份及正常宿主窗口，核对设置页首帧/尺寸变化；若 host 仍受阻，保留该边界并继续依赖已满足的 Q/R 任务。
