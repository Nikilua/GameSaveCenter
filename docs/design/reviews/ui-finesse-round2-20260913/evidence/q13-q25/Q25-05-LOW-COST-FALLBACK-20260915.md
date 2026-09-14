# Q25-05 低性能回退专项证据（2026-09-15）

## 结论

生产资源回退在受控 STA RenderHarness 中完成双主题、六个工作区和两种窗口尺寸的视觉复核：禁用玻璃与动效后仍保留可读文字、操作内容和数据表滚动能力，没有残留效果视觉或非表格水平溢出。真实低 Tier 显卡、宿主合成器和物理窗口性能仍不签收。

## 运行身份与覆盖

- 代码提交：`97dd0cd0bf9e95881060300b4935550517b3eb53`（`补充低性能回退探针`）。报告记录 `WorkingTreeClean=True`。
- 专用命令：`tests/GameSaveCenter.RenderHarness/bin/Release/net472/GameSaveCenter.RenderHarness.exe lowcostprobe .tmp/lowcostprobe-20260915`。
- 原始报告：[`.tmp/lowcostprobe-20260915/lowcostprobe-report.txt`](../../../../../../.tmp/lowcostprobe-20260915/lowcostprobe-report.txt)。
- 覆盖 `Overview`、`Save`、`Trainer`、`Media`、`Maintenance`、`Task` 六个工作区；浅/深色各覆盖 `1040×700` 与 `1600×900`，共 24 个 PNG。
- 报告明确记录 `DpiScale=1.00`，这是离屏逻辑 DIP，不推导真实宿主 DPI 或合成器性能。

## 回退门禁

- 每个案例的 `GscSurfaceEffect`、主按钮/侧栏/弹层/对话框/滑块效果和 `GscGameBackgroundEffect` 均为 `null`；24/24 案例 `visibleEffects=0`。
- `GscPopupAllowsTransparency=False`、`GscPopupAnimation=None`；`GscShellAmbientOpacity=0`、`GscGameBackgroundOpacity=0`；环境光渐变所有 stop alpha 为 0。
- 每个案例均保留可见文字；最少案例仍有 4 个可见 `TextBlock`，Overview 在窄/宽视口分别有 143/145 个可见文字块。
- 报告中的水平滚动只来自命名 `DG_ScrollViewer` 的有界 DataGrid（任务/维护数据列需要保留访问方式），`unexpectedOverflow=0`；没有页面级或非表格水平溢出。
- 深色 Overview、深色 Task 窄窗和浅色 Maintenance 宽窗 PNG 已实际查看：文字、状态、操作按钮和表格内容仍清晰，关闭材质没有留下黑底/空白效果层。

## 回归与边界

- RenderHarness Release 构建：`0 warning / 0 error`。
- `UiFinesseRound2ControlSourceTests`：`8/8`；既有透明/动效回退与实际 WPF 窗口 ReducedMotion 测试：`2/2`。
- 该证据只签收共享资源回退和受控离屏像素；不替代真实低 Tier 显卡测量、真实 Playnite 合成、Q25-02 呈现帧、Q25-03 UI 线程热点或 Q25-04 30 分钟耐久。
