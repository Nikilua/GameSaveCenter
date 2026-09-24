# 设置页窗口化顶栏复核（2026-09-24）

## 结论

本机 Playnite 扩展目录中的设置程序集身份早于当前分支的设置、页面窗口和媒体布局修正。用户最新截图仍显示图标、搜索框相对位置错开；这一窗口化布局问题已在当前 main 的 `3a1dadd8` 修正，后续 `c866c027` 构建也通过了相同尺寸的受控行为复核。没有再改已经通过当前布局门禁的生产代码。当前没有正常运行的 Playnite/GameSaveCenter 主界面，因此无法确认用户截图的实际加载包，也不能写成真实窗口已修复。

## 本机扩展身份对照

- 扩展目录中的 manifest 版本 `0.6.73`；`GameSaveCenter.Playnite.dll` FileVersion `0.6.73.0`、ProductVersion `0.6.73+7a4ba2a94da870c832e59f3ee025f9e34325d175`。
- DLL SHA-256：`5E02A462F1EDA26D706B550F8B428612CB13F787078A342CF3CFE4850AD50E22`。
- 该身份是当前 main 的祖先，早于 `3a1dadd8`“修正设置页搜索框错位”和 `da91bd68`“修正窗口页面布局与媒体列表滚动边界”。此前用户报告的 Task 行框修正 `9f3d7ab9`、Media 操作高度修正 `a4bac32f` 也属于更新提交。**推断：**若用户截图来自此扩展目录里的旧 DLL，当前 main 的修正尚未进入截图对应的安装版本；因截图当时未采到进程，不能把该推断写成宿主呈现结论。

## 当前 main 的行为复核

截图文件尺寸为 `1881×1208 px`。其中 36-DIP 按钮视觉高度约 54px，150% 是一种可能的缩放解释，但实际 Windows DPI 未核实。本次以 `1254×800 DIP` 作为近似尺寸假设，直接测量生产 `GameSaveCenterSettingsView` 的 WPF 布局；Light 和 Dark 分开运行，均 `1/1`、VSTest exit `0`：

- 设置图标/标题上边缘差 `11.33 DIP`，水平间距 `12 DIP`。
- 搜索框/标题左边缘差 `0 DIP`；搜索框比图标下移 `72.67 DIP`，处于独立标题行下方。
- 搜索框尺寸 `520×36 DIP`；把搜索框临时居中作为负例会右移 `287.33 DIP`。
- “恢复单字段”下拉框及两个按钮高度差/中心线差均 `0 DIP`；路径组合框和“浏览/校验/打开/复制”按钮全部 `36 DIP`，中心线差 `0 DIP`。

测试期间把两个主题放在同一 VSTest 进程时，xUnit 输出两条用例已通过，但进程未结束、未写 TRX；约两分半后中断该运行，不计作通过。随后 Dark 与 Light 各自独立运行，结果分别 `1/1`、退出码 `0`；保存的两份 TRX 都无 `InvalidComObjectException`。

## 当前 main 安装包候选

- 基于提交 `e83d8ba913080f2700e41f9d0b5f18b98ce04803` 的候选包：[GameSaveCenter-0.6.73.pext](../../../../artifacts/GameSaveCenter-0.6.73.pext)。包大小 `45,537,931` 字节，SHA-256 `75A8E5AD1841C7EE6898CCB63BEEF9C216ABFBB8C8CF57693602B667DEB8B73E`。
- `scripts/package.ps1 -SkipBuild -BuildOutputRoot .tmp/package-e83d8ba9` 完成 Worker 自包含发布、必需内容校验与六个 Plugin/Worker 程序集同源校验；统一身份 `0.6.73+e83d8ba913080f2700e41f9d0b5f18b98ce04803`。包未安装/替换本机 Playnite。
- 同批 Release solution 已构建：XAML `24/24`、0 errors、保留两条既有 `CS8602` warnings；Core `125/125`，Worker `355/356`（1 条既有 skip）。完整 Playnite 隔离套件有 105 类；为复用已完成的干净构建生成包，在第 3 类启动后中断全套，故不记录 Playnite 全量总数。此后按主题独立完成的设置几何 Light/Dark 用例、R08-03/R08-04 行为回归已有各自证据。

该受控几何使用 STA WPF、逻辑 DIP，不模拟 Playnite 父窗口或物理 DPI，不能替代正常宿主截图。当前安装目录 DLL 与最新代码的对应关系是目前能够确认的本机事实；实际用户屏幕/载入进程仍未验。此前隔离 host 的 CEF `platform_channel 0x5` 限制未绕过。

## 原始结果

- [Dark 主题 1254×800 DIP](SETTINGS-HEADER-WINDOWED-1254x800-DARK-20260924.trx)
- [Light 主题 1254×800 DIP](SETTINGS-HEADER-WINDOWED-1254x800-LIGHT-20260924.trx)

## 2026-09-24 当前 main 包复测补充

- 当前 main 候选为 [GameSaveCenter-0.6.73-main-c866c027.pext](../../../../../artifacts/current-main/GameSaveCenter-0.6.73-main-c866c027.pext)，SHA-256 `17B5C51CA502C0C2F119DFCBF98BF720AC43C56F899CB6BC3A6C909923498CAA`；六个插件/Worker 程序集的 informational identity 为 `0.6.73+c866c027a2c7a2232028e9a20f1e2060bcf027cd`。对应 Playnite zip 也保存在同目录。包未安装到用户的实际扩展目录。
- 当前 `c866c027` 的 `ReportedWorkspaceLayoutBehaviorTests` 记录 `8/8` passed、0 failed、0 skipped；Settings 顶栏 Light/Dark 各 `1/1`，包含 `1254×800 DIP` 窗口化尺寸、实际 Loaded/SizeChanged 路由和居中错位负例。原始 TRX：[USER-REPORTED-LAYOUT-CURRENT-MAIN-RECHECK-20260924-C866C027.trx](USER-REPORTED-LAYOUT-CURRENT-MAIN-RECHECK-20260924-C866C027.trx)，SHA-256 `86B03105AE17CD7EE8FC7649FD831234326B3CD8ED549D936D01759A705E9354`；TRX 无 `InvalidComObjectException` 文本。
- 候选包完整 Release 流程：XAML `24/24`、0 errors，保留两条既有 `MediaCenterView.xaml.cs:703 CS8602` warning；Core `125/125`，Worker `355 passed / 0 failed / 1 skipped`。Playnite 隔离测试报告 105 个 WPF 类执行、未提供聚合总数，不据此声称总用例数。
- 同身份隔离宿主安装启动后只出现 `Startup Error`，CEF `platform_channel.cc:108` 报拒绝访问 `0x5`，没有页面/UIA 视觉通过；不绕过该权限限制。隔离安装构建与 `.pext` 候选为分别构建的产物，虽 assembly identity 相同，不声称字节相同。

下一步：有正常 Playnite host 后，用当前 identity 对照用户窗口；条件不变时不重试 CEF 受阻路径。本轮接续其他不依赖宿主/ETW 的 Q/R 小批量。
