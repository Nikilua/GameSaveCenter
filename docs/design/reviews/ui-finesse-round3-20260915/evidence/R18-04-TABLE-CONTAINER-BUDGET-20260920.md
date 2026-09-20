# R18-04 表格容器预算（2026-09-20）

## 结论

R18-04 在当前分支 `codex/ui-finesse-round2` 的实现提交 `64843642` 已满足受控完成条件。复用生产 `TaskCenterView`、`MediaCenterView`、`MediaPageAccumulator` 和共享 DataGrid 模板，补充实际 STA WPF 容器预算测试，并修正 Media Inbox 的有限视口边界：正常高度下内层 DataGrid 使用显式有限高度，外层页级纵向滚动只在短页或 stale fallback 开启。现有 Media `Standard` / `Item` / `EnableColumnVirtualization=False` 例外保持不变。

同一批还校正了 c17 引入的媒体详情模板资源错误：`Style.BasedOn` 不使用 WPF 不支持的 `DynamicResource`，改为同一资源字典中的 `StaticResource`。此前两个真实媒体锚点 STA 测试会在 `MediaCenterView` 构造时抛 `XamlParseException`，校正后相关回归恢复通过；没有改媒体预览命令、绑定或错误/取消语义。

## 受控 WPF 结果

测试 `R18TableContainerBudgetTests` 使用合成 DTO、隔离 testhost 和实际生产 `TaskCenterView` / `MediaCenterView`。每种规模执行 8 次 `ScrollToVerticalOffset`，记录实际 `DataGridRow` 容器、可视行和滚动更新耗时。响应式布局在 `Window.Show()` 前应用，确保第一次 measure 就拥有生产的有限视口。

| 表格 | 后台规模 | UI 窗口项数 | 模式/单位/列虚拟化 | 视口行数 | 最大已实现容器 | 最大可视行 | 滚动 p95/最大 |
| --- | ---: | ---: | --- | ---: | ---: | ---: | ---: |
| Task | 2,000 | 2,000 | Recycling / Item / True | 7 | 9 | 7 | 52.846 / 52.846 ms |
| Media Inbox | 2,000 | 2,000 | Standard / Item / False | 9 | 9 | 9 | 0.132 / 0.132 ms |
| Task | 10,000 | 10,000 | Recycling / Item / True | 7 | 9 | 7 | 28.365 / 28.365 ms |
| Media Inbox | 10,000 | 2,000 | Standard / Item / False | 9 | 9 | 9 | 0.289 / 0.289 ms |
| Task | 20,000 | 20,000 | Recycling / Item / True | 7 | 9 | 7 | 34.807 / 34.807 ms |
| Media Inbox | 20,000 | 2,000 | Standard / Item / False | 9 | 9 | 9 | 0.153 / 0.153 ms |

原始滚动样本（毫秒）：

```text
Task-2000=3.272,52.846,21.988,30.919,14.207,18.003,16.978,20.865
Media-2000=0.079,0.132,0.060,0.032,0.060,0.034,0.027,0.026
Task-10000=0.811,17.190,16.070,28.365,13.433,18.355,20.365,25.060
Media-10000=0.034,0.029,0.035,0.028,0.026,0.035,0.044,0.289
Task-20000=0.820,34.807,15.762,15.360,16.897,21.642,16.568,24.595
Media-20000=0.044,0.075,0.041,0.092,0.075,0.079,0.076,0.153
```

所有样本均报告 `rowsPanelVirtualizing=True`，Task 子项数为 `9`，Media 子项数为 `9`；Media 的 10k/20k 后台数据由生产 `MediaPageAccumulator.DefaultCapacity=2000` 保留为 2,000 项 UI 窗口。测试阈值为已实现容器不超过视口行数加 32，且不等于完整 UI 源；三档均远低于该上限。

## 真实边界与曾捕获的负例

- 初版受控夹具先 `Window.Show()` 再调用 `ApplyResponsiveLayout`，真实捕获到 Media Inbox 在 9 行视口下先实例化完整 2,000 个 `DataGridRow` 的负例；这不是通过放宽断言掩盖。原因是首轮嵌套页级 ScrollViewer 的无限测量在有限布局生效前已经生成 Standard 行。
- 最终夹具在首次 measure 前应用生产响应式布局；生产代码同时把正常高度的内层 DataGrid 设为有限 `Height/MaxHeight`，并关闭外层页级纵向滚动，只在短页或 stale fallback 继续使用页面溢出。这保留了 Media Standard/Item/禁列虚拟化组合和短窗口页级回退。
- 第一次相关回归还捕获了 c17 的 `BasedOn="{DynamicResource GscCaptionStyle}"` WPF 解析失败；改为 `StaticResource` 后 `MediaWindowAnchorContractTests` 两个此前失败项通过。

## 门禁与边界

- R18-04 基准：`1/1`；相关媒体分页、锚点、四行几何、细滚动和滚动归属回归：`23/23`。
- `scripts/validate-source.py`：通过；`scripts/check-xaml.ps1`：`24/24`；`git diff --check`：通过。
- Release 隔离 solution `.tmp/r18-04-solution`：`0 errors / 2 existing MediaCenterView.xaml.cs nullable warnings`（当前行 671）；目标仍包含 Playnite `net462`。构建目录已在关闭 build server 后清理。
- 这是受控 WPF `Window` 的逻辑 DIP、容器和滚动更新证据，不是物理屏幕帧、DWM presented frame、显存、60fps、ETW、真实 Playnite/package-host 或跨屏 DPI 证据；没有绕过被拒绝的系统跟踪权限。
- Demo 原目录不可用，继续使用已恢复生产基线。只使用合成 DTO、fake 分页和隔离 testhost，未读取或修改真实存档、媒体、用户云端或诊断目录。没有把 TRX、`.tmp` 构建目录或一次性脚手架提交进 Git。
- 最新 WPF 静态质量扫描基线仍为 `0 errors / 27 warnings / 162 info`，本批未把它冒充为新的宿主呈现通过；真实 Playnite 首次 Loaded/布局时序、浅深主题最终呈现、DPI/UIA/读屏、物理跨屏和宿主性能仍待验。

下一可执行小批量：`R18-05 后台事件合并`，先盘点现有任务/媒体事件投递、批处理和页面卸载订阅，再用合成/fake 高频事件验证有界队列、完成/失败不丢失及关闭后停止刷新。
