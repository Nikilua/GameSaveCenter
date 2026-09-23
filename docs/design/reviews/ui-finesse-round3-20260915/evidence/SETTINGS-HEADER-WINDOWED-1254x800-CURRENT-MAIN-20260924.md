# 设置页窗口化顶栏复核（2026-09-24）

## 结论

本机 Playnite 扩展目录中的设置程序集身份早于当前分支的设置、页面窗口和媒体布局修正。用户最新截图的图标与搜索框相对位置不符合当前 main 的生产 XAML。没有再改生产布局；先保留当前实现的尺寸行为证据，并准备将当前 main 打包供用户更新核对。当前没有运行中的 Playnite/GameSaveCenter 进程，因此无法证明该截图由本机这个 DLL 实际呈现。

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

该受控几何使用 STA WPF、逻辑 DIP，不模拟 Playnite 父窗口或物理 DPI，不能替代正常宿主截图。当前安装目录 DLL 与最新代码的对应关系是目前能够确认的本机事实；实际用户屏幕/载入进程仍未验。此前隔离 host 的 CEF `platform_channel 0x5` 限制未绕过。

## 原始结果

- [Dark 主题 1254×800 DIP](SETTINGS-HEADER-WINDOWED-1254x800-DARK-20260924.trx)
- [Light 主题 1254×800 DIP](SETTINGS-HEADER-WINDOWED-1254x800-LIGHT-20260924.trx)

下一步：基于提交后的当前 main 准备可审阅 `.pext`，不安装到 Playnite；之后继续 R08-05。
