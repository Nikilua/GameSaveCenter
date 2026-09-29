# R00 / R01 当前 main 证据复核（2026-09-29）

源码身份：`9c906cc0772aad06143bdf3237255effd417e2de`。本轮先按该 clean main 身份构建，再运行定向行为测试、生产资源探针和完整离屏 RenderHarness/UI Audit。此前 freshness 命中的 8 条证据均以本轮可重复材料复核；旧章节中的历史结果保留作时间线，不再作为当前源码结论。

## 构建与程序集身份

- Release solution：XAML 结构检查 `24/24`，构建 `0 warning / 0 error`。
- RenderHarness 用同一隔离输出单独 Release 构建：`0 warning / 0 error`。
- RenderHarness 及探针运行记录 `WorkingTreeClean=True`、源码 Commit 为完整 `9c906cc...`；`DpiScale=1.00` 仅表示离屏逻辑 DIP。
- 被测 `GameSaveCenter.Playnite.dll`：版本 `0.6.73.0`，MVID `13e1c144-cc4b-4945-8bcb-611976a72928`，SHA-256 `4E3751984AA6F6ACEFE6ACDDEEB86FD6BC3313943F75ACD6F772C97EAD6E8DA0`。
- 被测 `GameSaveCenter.Playnite.Tests.dll`：版本 `0.6.73.0`，MVID `5a73c75d-169c-4473-9f63-f6667fecaf37`，SHA-256 `0726FDD8B7B318904AE7A719344A8760C8F882CF5E386EF4CB5BDBD967D18697`。

## 当前源码定向结果

所有 TRX 均为 xUnit 明确结果；共 `39/39` 通过、`0` 失败、`0` 跳过。各组覆盖边界如下。

| 任务 | 结果 | 实际覆盖 |
| --- | ---: | --- |
| R00-01/02 | 4/4 | 渐变 stop 与整组 opacity 合成、Light/Dark 语义状态色、复合变换内复用 ScaleTransform 后树稳定 |
| R00-04 | 2/2 | 2,000 个合成游戏的 30 个不同查询、每次结果 ID 集合均变化；不可能结果的运行中计时超时负例 |
| R00-05 | 2/2 | Light/Dark 下 Context、RemoteRestore、MediaBatch 三种生产派生样式；控件 opacity `1`、chrome `0.72`，启用/禁用高度差 `<0.01 DIP` |
| R00-06 | 8/8 | 真实生产视图几何预算、完整行/有效裁剪交集、末行和最右列、水平条交角、滚轮边界、拖动到头、往返和 resize |
| R00-08 | 11/11 | 当前选择不因无结果 Enter 改变；可见候选 Enter、IME 处理键、活动 composition、方向键、Esc 与焦点回返的 WPF 路由事件 |
| R01-03 | 11/11 | 当前源码根与程序集身份契约、审计入口及 20 项证据索引契约 |
| R01-05 | 1/1 | N01 对比度、N02 数值裁切、N03 picker 焦点、N04 子级溢出、N05 Loading 命中五个 expected-failure 检测器 |

逐项方法、计数和 TRX 位于 [`trx/`](trx/)。R00-04 原始查询序列与 30 个时延样本在 [`large-library.txt`](large-library.txt)：p50 `46 ms`、p95 `47 ms`、max `60 ms`、可见 ID 集变化 `30/30`。有限超时负例由独立运行中的 Stopwatch 断言，不使用停止计时器；本轮不为它补写未经记录的精确耗时。

## 双主题、裁切和四行证据

[`finesse/`](finesse/) 保存当前 production resource probe 的 Light/Dark 报告与图。两主题均记录 `88` 个按钮/渐变状态对比样本、语义层 `4` 个样本、违规 `0`；两主题的数值可读样本均 `4/4`，故意缩窄数值列的负例被检出。共享模板触发器覆盖 hover/pressed/focus 的样式状态检查不等于 OS 鼠标输入。

[`media-geometry-report.txt`](media-geometry-report.txt) 同时列出 Light/Dark × 五个场景：

- normal `42 DIP` 表头、`52 DIP` 行，完整行 `4/4`；
- 水平条 `12 DIP` 时仍测到 `4/4` 行，并把水平条、边框和 padding 纳入预算；
- alternate density 使用实测 `36/44 DIP` 表头/行，完整行 `4/4`；
- `760×340 DIP` 短窗口在网格显示 `0/4` 行时，明确记录 page-scroll fallback；
- 故意阻断父级滚动时，产生不可达/HIGH 负例，未把它报告成成功。

ScrollBehavior TRX 的受控 WPF testhost 自报 `dpi=1.5×1.5`；该日志来自当前桌面上的测试窗口，不是一次有控制变量的 125% 与 150% 物理显示器切换或 Playnite 嵌入验证。额外三种输出尺度 `1.0/1.25/1.5` 是测试窗口内的 WPF RenderTransform，也不是物理 DPI。所有几何量均为同一受控视觉树中的视口/裁剪交集。完整 multi-page 双主题、多尺寸结果见 [`render-qa-report.txt`](render-qa-report.txt)，末尾为 `render-qa OK`。

## R01 审计归档指针

当前同身份 UI Audit 和五张精选页图位于 [`../R01-06-controlled-audit-20260929/README.md`](../R01-06-controlled-audit-20260929/README.md)。该归档的 20 项索引校验为 `20/20`；Audit 本身仍包含 `7 HIGH / 4 MEDIUM`，本轮没有把其余风险误标为已解决。

## 未验边界

- 探针、图片与 UI Audit 使用合成 DTO、隔离 WPF 窗口和 logical DIP；不代表已加载的 Playnite 插件呈现、OS 键盘/鼠标/IME、物理 DPI/跨屏或 DWM presented frame。
- R00-08 的 TextComposition/KeyEvent 路由由受控 WPF 测试构造，不是系统输入法候选窗的人工确认。
- 没有访问真实游戏库、存档、媒体、用户云端或配置，也没有执行会改动用户数据的命令。
- 用户报告的 Media Inbox 主机滚动现象仍需正常隔离 Playnite 会话中的当前 DLL/MVID、主题、窗口 DIP、有效 DPI 和同坐标系滚动前后诊断；离屏结果不替代该项。
