# R07-08 resize 压力序列

## 结论

R07-08 在当前分支已满足可控验收条件。最新测试提交为 `9f478cac`；生产 shell 已有 Render 优先级的 resize 合并、页面独立详情滚动和游戏选择器 overlay，本项没有换布局体系或改变业务命令，只补真实生产 shell 的行为夹具。

## 行为与数据证据

当前身份隔离 Release shadow 输出为 `r07-08-build-9f478cac`；XAML 结构检查 `24/24`，solution 编译 `0 warning / 0 error`，Playnite 目标 `net462`。当前 worktree 的 WPF 标记编译临时项目受环境写入限制，bin/obj 放在已授权外部隔离目录，源码快照来自 `9f478cac`，未写 main 或仓库临时目录。

`R07ResizeStressBehaviorTests 1/1` 使用真实 `AcrylicProductionShellView`、真实 `TaskCenterView`、合成失败任务和隔离 STA Window：任务详情保持打开，游戏选择器 overlay 保持打开，搜索框焦点在每个样本均为 `True`；Task 表格实际高度非零，Task `MaxHeight` 没有残留有限值，详情和选择器 `MaxHeight` 均为合法有限值或正无穷。

实际序列和观测值如下：

| 样本 | 详情 | 菜单 | 搜索焦点 | 表格 ActualHeight / MaxHeight | 详情 MaxHeight | 选择器 ActualHeight / MaxHeight |
| --- | --- | --- | --- | --- | --- | --- |
| 1366×900 wide | Visible | Visible | True | `525.333 / ∞` | `∞` | `122.667 / 774.667` |
| 960×700 narrow | Visible | Visible | True | `180 / ∞` | `160` | `122.667 / 541.333` |
| 960×560 short | Visible | Visible | True | `180 / ∞` | `160` | `122.667 / 401.333` |
| 1440×900 wide-again | Visible | Visible | True | `525.333 / ∞` | `∞` | `122.667 / 774.667` |
| 1366×900 restored | Visible | Visible | True | `525.333 / ∞` | `∞` | `122.667 / 774.667` |

当前身份按 `FullyQualifiedName~R07` 的回归为 `14/14`，其中 R07-07 精细滚动 `2/2`、既有滚动所有权 `2/2`；`python scripts/validate-source.py`、`git diff --check` 通过。测试进程退出阶段仍打印一次 WPF `TextServicesContext` 的 `InvalidComObjectException` 清理诊断，但 vstest 退出码为 `0`，未将该输出隐藏为宿主通过。

## 验收边界

测试使用合成 DTO/fake context、真实生产 WPF shell/page、隔离 STA Window 和 offscreen logical DIP；没有写真实存档、媒体、云端或诊断数据。未启动真实 Playnite，不宣称物理 DPI/跨屏、OS 触控板/鼠标、UIA/读屏、presented frame、ETW、宿主帧率或性能，也未把外部 shadow 输出当成仓库产物。Demo 原始目录仍不可用，继续沿用恢复生产基线。

下一项可执行任务：R08-01 中途反向连续。R07-08 尚未验证真实宿主拖拽 resize、物理屏幕呈现和系统级性能计数器。
