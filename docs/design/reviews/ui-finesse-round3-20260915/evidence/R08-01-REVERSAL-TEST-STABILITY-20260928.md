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
- 用户稍后的一次完整隔离构建在 R08 类序号 `[54/108]` 通过；同轮最终 `WpfUiResourceDictionaryTests` 有另一项滑杆契约测试失败，与 R08 无关。它在 `f8322666` 仍静态要求 `GscSlider` 高度 `22 DIP`，而稍早的 `e007a339` 已经根据真实 WPF 命中行为扩大控件/Thumb 输入区域至 `32 DIP`，保留 `4 DIP` 可见轨道和 `18 DIP` 可见圆头。该旧断言造成构建末尾 `138 passed / 39 skipped / 1 failed`，不是滑杆交互故障。原始结果：[完整构建日志](../../../../../artifacts/r08-motion-flake-20260928/full-release-build-f8322666.log)、[旧结构断言 TRX](../../../../../artifacts/r08-motion-flake-20260928/full-release-slider/shared-slider-demo-geometry-f8322666.trx)。
- 已更新 `WpfUiResourceDictionaryTests`：解析共享样式并核对控件/Thumb 命中高度 `32`、两段 `4 DIP` 轨道、`18 DIP` 可视 Thumb，以及设置页使用共享样式。当前修订源码编译到隔离 Release 程序集，构建 `0 warning / 0 error`；新结构用例 `1/1`，现有设置滑杆 Light/Dark/FollowPlaynite 命中、值回写和键盘行为 `3/3`，R08 完整类 `2/2`。
- 上述 R08 复跑时 xUnit 两项均通过且 VSTest exit `0`，退出日志额外出现 `TextServicesHost.OnUnregisterTextStore InvalidComObjectException` 清理噪声；根因未知。清理噪声不改变测试结果，但按用户要求保留说明。
- 最终提交身份 `4d0ff11c40a9ba0848689bad21e927d4205b1b55` 运行完整 `scripts/build.ps1 -Configuration Release -OutputRoot .tmp/build-full-4d0ff11c` 并成功结束：XAML `24/24`，Release solution `0 warning / 0 error`，Core `125/125`，Worker `356 passed / 1 existing skip / 0 failed`；Playnite 源测试 `111` 类组和隔离 WPF `108` 类组全部通过。运行器明确输出 `All Playnite tests passed with WPF classes isolated by process`、`构建与测试全部成功`。完整过程日志保存在 [Release build log](../../../../../artifacts/r08-motion-flake-20260928/full-release-build-4d0ff11c.log)；其中 Playnite 隔离脚本的 `Write-Host` 进度不进入 Tee 文件，但本轮完整脚本的最终成功输出已由执行结果确认。
- R08 本次完整隔离构建再次通过。其独立定向复跑为 `2/2`、VSTest exit `0`，额外输出一条 `TextServicesHost.OnUnregisterTextStore InvalidComObjectException` 退出清理日志。Worker 的一项跳过仍为既有 `HardRestartReconcilesDurableIncompleteTask`。用户机器原失败堆栈仍不可见，因此不宣称本地采样问题必然是该机器失败的唯一根因。

本复核修复并验证的是自动化采样稳定性，不声明用户 Playnite 内实际侧栏动画已验。用户失败机完整日志仍待提供；若日志显示失败点不是采样窗口，需按该断言继续核对。
