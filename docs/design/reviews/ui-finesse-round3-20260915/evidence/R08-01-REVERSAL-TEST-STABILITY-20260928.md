# R08-01 侧栏反向测试采样稳定性（2026-09-28）

## 用户提供的失败与本地复核

用户的一键构建输出显示 `R08MotionReverseBehaviorTests` 为 `1 passed / 1 failed / 2 total`。失败机器的完整 `artifacts/one-click-install.log` 不在本 checkout；本地同名文件最后修改于 2026-09-24，不能当作本次失败日志，也未取得失败断言文本。

在修复前，当前代码下重复运行 R08 完整类时，曾在 4 轮中复现一次 `SidebarRapidReversalUsesLatestTargetAndReleasesOldClock` 失败；该次用了 quiet logger，没有保留断言详情。随后该单个方法独立 8 次通过，完整类另 8 次全部通过，说明失败为间歇性，不能据未复现认定没有问题。

## 修正范围

失败方法在首次收起后固定等待 `100 ms` 再读取 `ColumnDefinition.ActualWidth`。WPF 渲染/布局尚未提交第一帧时，该读数仍可能是展开终值，使测试把“尚未采到动画中段”误判为动画反向失败。只修正测试采样：使用较长的受控动画时长，并通过有界 dispatcher pump 等待实际宽度进入两端之间且动画时钟仍活动；反向后也等待扩展方向的活动中间样本。保留反向起点连续性、最终宽度、完成回调、透明度以及动画时钟清理断言。未改生产动画时长或视觉实现。

## 修复后验证

- 代码身份：`e467c3b2fe080965c3946f1f3101f707c731472b`。
- Release Playnite 测试项目构建：`0 warning / 0 error`。
- 修改后 R08 完整类在独立 testhost 连续运行 `8/8`，每轮 `2/2`，0 failed/skipped；原始汇总：[8 轮输出](../../../../../artifacts/r08-motion-flake-20260928/r08-fixed-repeat.log)。
- 当前完整 Release build 脚本正在该身份下运行：solution build、Core 与 Worker 已通过，Playnite/WPF 逐类隔离阶段尚未结束；在取得脚本最终退出码前不记全套构建成功。

本复核修复并验证的是自动化采样稳定性，不声明用户 Playnite 内实际侧栏动画已验。用户失败机完整日志仍待提供；若日志显示失败点不是采样窗口，需按该断言继续核对。
