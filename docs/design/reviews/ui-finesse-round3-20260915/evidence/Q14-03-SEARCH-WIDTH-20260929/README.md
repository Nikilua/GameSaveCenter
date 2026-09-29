# Q14-03 Task Center 搜索视口宽度

日期：2026-09-29。源码提交：`5559b0fbc9eba2c14207660628860a6f44b30571`。修复 Task Center 紧凑窗口下搜索框与状态筛选争用同一行宽度的问题。

## 修复与覆盖

- `TaskCenterView.ApplyResponsiveLayout` 将紧凑筛选布局断点由 `980 DIP` 调整为 `1216 DIP`。在 `760–1215 DIP` 内保持搜索、状态和刷新控件在主行；其余筛选继续通过现有“更多筛选”折叠区访问。既有搜索框、清除按钮、绑定与命令不变。
- 真实生产 `TaskCenterView` 的 STA WPF 行为在 Light/Dark × `760、979、980、1040、1215、1216、1280 DIP` 共 14 个搜索布局场景中实测：搜索 cell 和输入 viewport 均在父级内；viewport 至少 `200 DIP`；搜索/状态、状态/刷新各留 `10 DIP`；刷新按钮未越过筛选视口；搜索清除按钮留在搜索 cell 内。
- `1040 DIP` 的负例在同一窗口强制恢复旧 full-row 放置，检测到搜索框溢出；搜索 cell 与状态组出现 `-164.67 DIP` 重叠间距。由此确认回归能识别旧布局问题，而不只是检查元素存在。
- 同一生产视图行为还覆盖原工具栏的 10 个 Light/Dark × 尺寸场景，核对 `36 DIP` 控件高度、中心对齐，以及校验错误、键盘焦点、禁用刷新和忙碌状态下几何稳定。筛选标签回归另覆盖 Light/Dark × `620、760、979、980、1040、1215、1216、1280 DIP` 的 16 个场景，核对组绑定、选项往返和窄窗分组换行。现有恢复基线测试 `16/16`。

## 验证结果

- 三份 post-commit Release TRX：`Q14ToolbarAlignmentBehaviorTests 1/1`、`Q14FilterLabelBehaviorTests 1/1`、`RestoredAcrylicForkBaselineTests 16/16`，共 `18/18`，0 失败、0 跳过。前两条各自单个 xUnit Fact 内运行多组 Light/Dark 与尺寸场景；几何详情见 [`geometry-report.txt`](geometry-report.txt)。
- 精确源码身份的 RenderHarness 完整矩阵：`render-qa OK`，clean tree，Light/Dark、11 个窗口尺寸与合成列表数据量 `50/400/2000/4468`；报告摘要中的离屏 `DpiScale=1.00`。本批代表性 `1040×700` 截图：[浅色](Task-1040x700-light.png)、[深色](Task-1040x700-dark.png)。
- 归档完整 RenderHarness 报告：[render-qa-report.txt](render-qa-report.txt)，SHA-256 `9313B3D6366D4191E895FB6D6A630C2FAFEA3128135F6AE7919F6512A0051983`。TRX 保留测试结果和实际 WPF 几何输出。

## 验收边界与后续

- 受控 WPF 和 RenderHarness 支持搜索视口本地行为/布局通过；没有启动真实 Playnite，没有在物理 `125%/150%` DPI、OS 输入或最终屏幕呈现帧下验证。因此 Round2 Q14-03 记为本地代码与自动验证通过、宿主外部阻塞、最终未完成。
- R00/R01 freshness 对截至本提交的 14 条记录为 `14 FRESH / 0 STALE`；没有命中本次 TaskCenter 源码或测试文件，不需重跑这些既有证据。脚本完整输出归档于 [`ui-evidence-freshness.json`](ui-evidence-freshness.json)，本次 source 检查未提供独立 package commit。
- 下一可执行项：继续审计跨页面可换行控件组的窄窗垂直间距；只按真实可触发换行的小批补 WPF 几何行为，不全局机械改写 WrapPanel。Media Inbox 用户报告的真实 Playnite 滚动错位仍待安全宿主同进程 `[GSC-GRID-DIAGNOSTIC]`。
