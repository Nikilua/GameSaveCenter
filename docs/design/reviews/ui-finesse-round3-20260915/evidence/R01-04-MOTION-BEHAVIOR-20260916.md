# R01-04 动效行为替代字符串证据

## 结论

R01-04 已满足受控 WPF 行为门禁。实现提交为 `74abb10096df77cb77ef59128f1898e621475a51`，生产动效实现沿用已有 `GscMotion.AnimateEntrance`；本阶段把重入基值语义交给实际 STA WPF 状态采样，并将源码断言收窄为结构约束。

## 实现与行为

- `UiFinesseFoundationTests.EntranceMotionReentryKeepsTheRenderedBaseWhenLatestClockIsCancelled` 创建真实 WPF `Window`/`Border`，用 `GscMotion.AnimateEntrance` 启动 `12 DIP` 入场，等待 `120 ms` 采样有效 Y/Opacity，立即以另一个意图重入，再清除最新动画时钟；Y 与 Opacity 必须分别保持在采样值 `0.8 DIP` / `0.08` 误差内。
- 该测试不读取生产源码文本，也不依赖局部变量名或注释。既有 `EntranceMotionReentryKeepsTheCurrentDispatcherValue` 继续检查重入瞬间连续性和最终 `Y=0/Opacity=1`，`MotionAnimationsReleaseClocksAtTheirFinalValues` 检查完成后时钟清除与终态写回。
- 原 `EntranceMotionTakesOverFromTheCurrentEffectiveValue` 只保留 `AnimateEntrance`、活动动画分支、清钟、动画对象、HoldEnd 和 Completed 等结构约束；移除 `currentY/currentOpacity` 的精确源码字符串断言。结构门禁仍用于发现接线消失，不能单独签收行为。

## 突变负例

突变只在仓库 `.tmp` 隔离输出中验证，源码已恢复且目录已清理，未进入提交：

| 突变 | 隔离结果 | 失败证据 |
| --- | --- | --- |
| 删除重入分支 `translate.Y = currentY` | Release 隔离构建 `0/0`；行为测试按预期失败 | 清除最新时钟后 Y 偏差 `8.332255396871652 DIP`，超过 `0.8` 门限 |
| 删除重入分支 `element.Opacity = currentOpacity` | Release 隔离构建 `0/0`；行为测试按预期失败 | 清除最新时钟后 Opacity 偏差 `0.74548606462788181`，超过 `0.08` 门限 |

这证明删除关键基值处理会使行为测试失败；只改变注释或局部变量名不会改变该测试输入、事件顺序和状态断言。

## 可复核验证

- `scripts/build.ps1 -Configuration Release -OutputRoot .tmp\r01-04-final-build`：XAML `24/24`；构建 `0 warning / 0 error`；Core `83/83`；Worker `311/311`；Playnite `522` 通过、`57` 跳过、`0` 失败，总计 `579`。
- 绑定当前提交的动效定向过滤 `FullyQualifiedName~EntranceMotion`：`4/4` 通过，包含结构门禁、真实重入清钟行为和现有 Settings 动效关联测试。
- 代码提交 `74abb10` 已推送到 `origin/codex/ui-finesse-round2`；提交后工作树保持干净。

## 边界

- 证据使用合成控件、隔离 `Window`、Dispatcher 和 offscreen logical DIP；不等价真实 Playnite 嵌入、真实鼠标/键盘输入、物理 DPI、presented frame、ETW 或宿主帧率。
- 未启动 Playnite/Worker，未写真实存档、媒体、云端或诊断外发；游戏选框、滚动条、命令/绑定、取消/错误、恢复保护、有限列表和 `net462` 契约未改。

## 下一步

R01-04 已满足；下一可执行小批量为 R01-05“负例注册表”，先盘点现有对比、裁切、焦点、层级和状态负例，避免重复创建生产入口。
