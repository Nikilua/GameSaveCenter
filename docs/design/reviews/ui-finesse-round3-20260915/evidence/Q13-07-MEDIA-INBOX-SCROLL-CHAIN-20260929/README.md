# Q13-07 媒体 Inbox 内外滚动链行为复核

日期：2026-09-29。测试代码提交：`8e4c32a7ffc65538f28ac7411ef88b286c4ef94e`。

## 复核结果

先检查生产入口与已有行为，不新增滚动服务或改滚动策略。生产 `MediaCenterView` 已给页面和 Inspector 配置 `ScrollBoundaryRoutingBehavior`，Inspector 是有界的 `ScrollViewer`；既有 `R07ScrollOwnershipBehaviorTests` 与 `R07FineScrollBehaviorTests` 覆盖通用滚动面及 DataGrid 边界。缺少的是 Media Inbox 实际页面、实际 compact details 操作打开 Inspector 后的整合断言。

本提交只在 `MediaInboxScrollBehaviorTests` 新增 `ProductionInboxInspectorTransfersWheelOnlyAtItsScrollBoundary`。夹具装载生产 `MediaCenterView` 和 `MediaInboxGrid`，使用 2,000 条合成媒体；在 860×620 DIP 窗口中选择一项并点击生产 compact details 按钮，打开窄窗堆叠布局的详情滚动面。只在合成 Inspector 内容内追加 24 个 42 DIP 测试元素，以确保有内部滚动范围。

测试通过真实生产路由行为接收合成的 WPF `PreviewMouseWheel` routed event，具体检查：

| 状态 | 观察值 | 断言 |
| --- | --- | --- |
| 窗口与 Inspector | Window `860×620 DIP`；page range `549.33 DIP`；Inspector 可见且 `ActualHeight=MaxHeight=416.67 DIP`；内部 range `1502 DIP` | 两层都有 Auto 垂直滚动范围，Inspector 有限且可见 |
| Inspector 中段向下 | page `274.67/549.33`；Inspector `751/1502`；event 未处理 | 页面 offset 不变；内部面未到边界时不转移 |
| Inspector 底部向下 | page `274.67→290.67`；Inspector 保持 `1502/1502`；event 已处理 | 页面接收一个 16 DIP 步进，Inspector 不越过末端 |
| Inspector 顶部向上 | page `274.67→258.67`；Inspector 保持 `0/1502`；event 已处理 | 页面接收一个 16 DIP 反向步进，Inspector 不越过起点 |
| 两层都在顶部 | page 与 Inspector offset 都为 `0`；event 已处理 | 消耗无可滚动目标的事件，两层位置不变 |

## 精确身份与测试

- 插件 DLL：`0.6.73+8e4c32a7ffc65538f28ac7411ef88b286c4ef94e`，MVID `554421ff-cea9-4b9b-ba64-d7970f0964bb`，SHA-256 `444150691B412C6B6F0CD671DEC7D887EA33342760AB3667F555678EA3FB4DDC`。
- Playnite 测试程序集 SHA-256：`8DDAEE378AEF7A46ED0F68DC8C5A61E2F2625DA7218FDA534FA0E4B2BF8555C6`。
- Release solution build：`0 warning / 0 error`；XAML 检查 `24/24`；`scripts/validate-source.py` 通过。
- 三个相互隔离的 VSTest 进程均以 exit `0` 结束：`MediaInboxScrollBehaviorTests 5/5`、`R07ScrollOwnershipBehaviorTests 2/2`、`R07FineScrollBehaviorTests 2/2`。原始结果：[Media Inbox](MediaInboxScrollBehaviorTests.trx)、[滚动所有权](R07ScrollOwnershipBehaviorTests.trx)、[细步长边界](R07FineScrollBehaviorTests.trx)。

## 验收边界

此结果证明生产控件树中的路由行为、offset 和终端负例；事件由 WPF routed-event fixture 合成，不是物理鼠标滚轮。没有启动真实 Playnite，也没有触发 OS 输入、做真实主题/有效 per-monitor DPI 检查或验证触控板惯性。因此 Q13-07 的自动行为证据补齐，但真实宿主及触控链仍为外部待验，账本继续标记“外部阻塞 / 未完成”。Media Inbox 用户报告的真实宿主滚动错位仍需安全隔离条件可用后采集同进程前后日志，不因本次 Inspector 用例而推断其根因。
