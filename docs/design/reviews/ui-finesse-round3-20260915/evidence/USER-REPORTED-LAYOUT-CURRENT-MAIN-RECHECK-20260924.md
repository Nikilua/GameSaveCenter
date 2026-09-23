# 用户报告的四处窗口布局问题：当前 main 行为复核（2026-09-24）

## 当前构建与测试

- Playnite `net462` / tests `net472` 的当前 main Release 输出 identity 为 `abd7927b`；XAML `24/24`，solution `0 errors`，保留两条既有 `MediaCenterView.xaml.cs:703 CS8602` warnings。
- `ReportedWorkspaceLayoutBehaviorTests` Light/Dark 四组各两例，共 `8/8`，0 failed/skipped，VSTest exit `0`。TRX 没有 `InvalidComObjectException` 清理文本。
- 本轮没有新增生产代码或减弱断言；读取的是生产控件，并在隔离 STA WPF Window 中测量布局、滚动条与回收后的 DataGrid 行状态。

## 四项当前受控结果

| 用户报告 | Light/Dark 当前实测 | 行为和负例覆盖 |
| --- | --- | --- |
| Media Inbox 按钮高度与列表滚动进入“忽略所选” footer | 批量清空/重置/归类、视图模式按钮都是 `36 DIP`；中心差最多 `0.33 DIP`；DataGrid 固定 `360 DIP`，grid/footer overlap `0`；内部 scrollbar 全程在 grid 的 `98.67–458.67 DIP` 区域内，footer 从 `468.67 DIP` 开始 | 实际滚动内部 `ScrollViewer`；滚动条 thumb 保持在内层轨道，页 footer 可达且不与表格重叠；Light/Dark 各一例 |
| Task Center 失败行外框错位 | 每主题生成 2,400 条合成任务并滚动复用行容器；回收前/后容器 `13/14`，抽查 7 个失败行，presentation mismatch `0`，行 chrome position error `0 DIP`，行高差 `0 DIP` | 验证行回收后的失败状态与 cell-local chrome；明确断言没有整行错误外框 |
| Save Center 窗口化按钮周围垂直空白 | summary 卡 `1010×442 DIP`，内容 `987.33×377.33 DIP`，操作行 `987.33×36 DIP`；行距 `10 DIP`、尾部留白 `9.33 DIP`；选择和 inspector 状态保留 | 由真实 WPF Window 尺寸序列进入布局事件，验证窗口化→宽态→再窗口化的行位置；不是单看 XAML 中有没有 Width/Height 属性 |
| Settings 顶部图标、搜索、操作行错位 | icon/title 顶差 `11.33 DIP`、水平间距 `12 DIP`；search/title 左差 `0 DIP`，固定搜索框 `520×36 DIP`；恢复默认操作和路径按钮均 `36 DIP` 且中心线差 `0`；居中搜索负例右移 `287.33 DIP` | Light/Dark 实测 Loaded/SizeChanged 生命周期；另测紧凑搜索宽度 `392 DIP`、横向溢出 `0`、提示行折叠和路径按钮关系 |

Media Inbox 的内部 scrollbar containment、页 footer 回退和窄窗滚动通道另由同一当前 test assembly 的 `MediaInboxGeometryTests 3/3` 覆盖。R18-04 专测和 23 项关联测试组成见 [R18-04 当前 main 复核](R18-04-CURRENT-MAIN-RECHECK-20260924.md)。

## 截图与候选包边界

- 受控几何结果与用户 Settings 截图中的布局关系不同：当前视图结果是搜索框左边界与标题对齐、搜索框位于图标下方；截图显示搜索框靠右且接近图标行。当前本机 Extensions 目录中读取到的旧 DLL identity 为 `0.6.73+7a4ba2a9`，它早于 Settings header/search 修正 `3a1dadd8`、窗口/Media 滚动修正 `da91bd68`、Task error frame 修正 `9f3d7ab9` 和 Inbox 动作高度修正 `a4bac32f`。这使旧 package 成为截图来源的可能解释；没有运行进程可把图片关联到具体 DLL，不能写成已确认因果。
- 包含上述修正的可审阅候选 `[GameSaveCenter-0.6.73.pext](../../../../../artifacts/GameSaveCenter-0.6.73.pext)` 为 identity `0.6.73+e83d8ba9`、SHA-256 `75A8E5AD1841C7EE6898CCB63BEEF9C216ABFBB8C8CF57693602B667DEB8B73E`；没有安装到用户真实 Extensions。
- 这组证据来自合成数据、生产 WPF 页面、隔离 STA Window 和逻辑 DIP。未验证正常 Playnite/package-host、截图的真实加载版本、Windows 物理 DPI/跨屏、UIA/读屏、IME、DWM presented frame、ETW 或宿主性能。不能据此声明用户屏幕上的问题已修复。
- 没有读写真实存档、媒体、云端或诊断数据；Demo 原始目录不可用，沿用恢复生产基线。CEF `platform_channel 0x5` 的隔离 host 限制未绕过。

原始测试结果：[ReportedWorkspaceLayoutBehaviorTests 当前 main TRX](USER-REPORTED-LAYOUT-CURRENT-MAIN-20260924.trx)。
