# R08-01 当前 main 复核（2026-10-13）

## 本轮证据

- 用户提供的 `GameSaveCenter-sdk10-build-diagnostics.zip` SHA-256 为 `A4D13BB58AC343DFDCFFA6FB4DA31257080F1E5128AF151C946B0AEB0E3BC409`，对应 #967 / run `38362229559` 的 SDK10 job `115178885908`，artifact `11795654027`。该 job 使用 SDK `10.0.401`，整体 build/test 均通过。整个 workflow 为失败状态，是因为独立 SDK8 job `115178885652` 在源码结构检查失败；该 SDK10 ZIP 不包含 SDK8 的失败日志。
- 从原 ZIP 的 `tests/wpf.R08MotionReverseBehaviorTests/wpf.R08MotionReverseBehaviorTests.trx` 提取了本项独立 TRX：[R08MotionReverseBehaviorTests-sdk10-#967.trx](R08MotionReverseBehaviorTests-sdk10-#967.trx)，大小 `5,548` bytes，SHA-256 `409D39AB7377E28AE83C16EB1FEBF74645CEDD6187A53A6C8C35DC418FA56F7F`。它列出 `2 passed / 0 failed / 0 skipped`：Translate 中途反向到最新目标、侧栏连续反向并释放旧动画时钟。
- 诊断包记录 test source `tests/GameSaveCenter.Playnite.Tests/R08MotionReverseBehaviorTests.cs` SHA-256 `9C6489FA737D36E20625460BAC0B1A36F9151B8D024290972D864F6463002E7D`；当前 checkout 文件 SHA 相同。源码 commit `983fee5c` 到当前 main `4aa1ec7` 的差异未改本测试或生产动效源码；随后 `d56d23e` 仅同步文档。当前 main 的 #968 SDK8 `8.0.423` 与 SDK10 `10.0.401` 全量门禁也均终态成功。
- 当前证据补强了可控 WPF 行为结论，不能解释用户机器上 `1/2` 失败：未提供那次失败的具体方法、断言、堆栈、原始 TRX 或加载 DLL 身份。该用户报告仍待原始故障材料定位，不能写成已修复。

## 验收边界与下一步

验证对象是生产 WPF 动效及合成输入的隔离测试，不是真实 Playnite 呈现、OS 动效偏好通知、物理 DPI/跨屏、最终呈现帧、ETW 或宿主性能。下一项回到账本中下一个依赖已满足的 Q/R 工作；用户报告原始失败材料仍是独立待验项。
