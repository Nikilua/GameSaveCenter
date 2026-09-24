# 用户报告四页布局复核（main 42884321，2026-09-24）

## 结果

- Release `main` 构建 0 errors；全量隔离审计构建观察到两条既有 `MediaCenterView.xaml.cs:703 CS8602` warning。Playnite 目标 `net462`，WPF 行为测试目标 `net472`。
- `ReportedWorkspaceLayoutBehaviorTests` Light/Dark 各覆盖 Media Inbox、Task Center、存档历史、Settings，合计 `8/8`、0 failed/skipped。当前原始 TRX：`USER-REPORTED-LAYOUT-CURRENT-MAIN-RECHECK-20260924-42884321.trx`；TRX 无 `InvalidComObjectException` 清理噪声。
- Media Inbox 批量动作保持 `36 DIP`，滚动条留在有限 Grid 行；Task 失败状态留在自己的单元格，容器回收下位置误差为 0，未出现整行红框；存档操作行保持 `36 DIP`，摘要卡未被按钮撑高；Settings 图标/标题顶差 `11.33 DIP`、水平间距 `12 DIP`，搜索框与标题左差 `0 DIP`。
- Settings 窗口化序列 `1254×800 DIP` 中搜索框为 `520×36 DIP`、位于标题列，恢复默认组合框和两个按钮同为 `36 DIP` 且中心/高度差 `0`；路径组合框与浏览/校验/打开/复制四按钮均为 `36 DIP`。紧凑宽度搜索框 `392 DIP` 且横向溢出 `0`。居中错位负例实测左差 `287.33 DIP`。

## 对新 Settings 截图的判断

用户新截图仍显示搜索框明显右于标题，图标也落在与标题不同的垂直位置；这组关系没有在当前生产 XAML 的隔离 STA WPF 窗口中复现。已有修正来自 `3a1dadd8`，`c866c027` 审阅包 SHA-256 `17B5C51CA502C0C2F119DFCBF98BF720AC43C56F899CB6BC3A6C909923498CAA` 包含该生产源码；该包未安装。当前本机旧扩展 DLL 身份曾读为 `0.6.73+7a4ba2a9`，但没有运行进程可把本次截图关联到具体模块，所以“截图来自旧包”仍是推测，不是结论。

本测试使用真实 WPF `Window`/Loaded/SizeChanged 生命周期、合成状态与逻辑 DIP。它证明当前源码布局与负例行为，不证明 Playnite 设置宿主、用户屏幕 DPI 或用户安装包呈现已修复。R23-04 当前宿主 run 被隔离 runner 的 safe-start 清理缺陷挡住，细节见 [R23-04 bootstrap 根因](R23-04-BOOTSTRAP-SAFE-START-ROOT-CAUSE-20260924-42884321.md)。不把隔离几何改写成真实宿主通过。
