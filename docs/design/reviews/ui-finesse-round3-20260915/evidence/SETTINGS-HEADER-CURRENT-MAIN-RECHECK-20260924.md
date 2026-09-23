# 设置页顶栏最新截图复核（2026-09-24）

## 结论

用户最新截图仍表现为设置图标落到搜索行附近、搜索框明显右移、顶部控件视觉错位。当前 main 的生产 `GameSaveCenterSettingsView` 在受控 WPF 几何行为中没有复现：Light/Dark 都保持图标在标题左侧、搜索框从标题左边界开始、恢复默认控件和路径操作控件同高并对齐。没有新增生产代码，因为当前允许的隔离 WPF 窗口不复现这张图里的偏移，盲目改变宿主布局会引入未经验证的回归。

## 当前 main 的受控结果

- Release solution 当前 main `4b7f0a34` 构建成功：XAML `24/24`、0 errors；保留两条既有 `MediaCenterView.xaml.cs:703 CS8602` warning。生产设置布局自 `3a1dadd8` 后未变。
- `ReportedWorkspaceLayoutBehaviorTests.SettingsHeaderAndPathActionsStayAnchoredToTheirLabelsAndEachOther` 在 Light/Dark `2/2`，包含实际 Loaded/SizeChanged 窗口路由、窄/宽/恢复尺寸、控件几何及居中负例。结果见[当前 main TRX](SETTINGS-HEADER-CURRENT-MAIN-RECHECK-20260924.trx)。
- 两主题标题/搜索框左边缘差 `0 DIP`；图标/标题横向间距 `12 DIP`、图标上沿差 `11.33 DIP`；搜索框 `520×36 DIP`，窄态 `392 DIP` 且无右溢出。把搜索框临时改成居中会右移 `287.33 DIP`，能检测截图里的偏移型回归。恢复默认 ComboBox 与两个按钮都是 `36 DIP`，中心线差 `0 DIP`；路径 ComboBox 与“浏览/校验/打开/复制”均为 `36 DIP`、中心差 `0 DIP`。

## 未解决的实机差异

用户当前扩展包/插件程序集 build identity 未提供；桌面实际 DPI、Playnite 设置父容器约束和正常 package-host 首帧也未得到验证。上述测试在隔离 STA WPF、逻辑 DIP `1.0` 下运行，不等价真实 Playnite 或物理屏幕。此前隔离 Playnite host 因 CEF `platform_channel` 拒绝访问 `0x5` 未能进入正常扩展页面；没有绕过该阻挡。

已有可供身份比对的本地包仍为 [GameSaveCenter-0.6.73-main-8e4f3194.pext](../../../../artifacts/GameSaveCenter-0.6.73-main-8e4f3194.pext)，程序集身份 `0.6.73+8e4f3194227afb28640754f12ab0889cb8bb71ce`，尚未安装，也不是当前 main `4b7f0a34` 身份。本证据不表示用户已使用该包。

**当前受控布局已满足；最新用户截图的真实宿主差异仍待 package identity 与正常 Playnite host 对照。**

## 2026-09-24 窗口截图补充

本机安装目录中的 DLL 已只读识别为 `0.6.73+7a4ba2a9`，其祖先关系早于当前 main 设置搜索修正 `3a1dadd8` 和窗口/媒体布局修正 `da91bd68`；没有运行中的 Playnite 进程可与该 DLL 建立截图时间关联。当前 main 在近似窗口逻辑尺寸 `1254×800 DIP` 的 Light/Dark 实际 WPF 测量均为通过：icon/title top 差 `11.33 DIP`、search/title left 差 `0`、search/icon top 间隔 `72.67 DIP`、恢复默认组控件中心/高度差 `0`、路径组全 `36 DIP` 同中心。该 150% 尺寸换算只是由截图控件像素高度推测，实际 DPI 未核实。

结果、DLL SHA-256、VSTest 中断后按主题单独重跑的事实与边界见[窗口化截图当前 main 复核](SETTINGS-HEADER-WINDOWED-1254x800-CURRENT-MAIN-20260924.md)和两份 TRX。当前截图视觉位置与最新源码结构不一致；旧安装身份是可能解释，仍未证明当时屏幕用的是该扩展 DLL。未宣称 Playnite 实际窗口已经修复。
