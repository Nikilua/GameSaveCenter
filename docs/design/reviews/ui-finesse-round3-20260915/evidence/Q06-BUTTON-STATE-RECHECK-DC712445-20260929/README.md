# Q06 按钮状态测试失败复核

日期：2026-09-29。用户提供另一工作区的一键构建日志，报告 `Q06ButtonStateSequenceBehaviorTests` 单项失败。

## 失败构建身份

- 日志来自干净的 `main` checkout `dc712445ec4baefd9b1cb3173d176c80b697d050`，与当前 checkout 同提交。Release 构建通过；Core `125/125`、Worker `357/357`。WPF 隔离 runner 在第 `17/111` 类处停止。
- 失败测试程序集与插件的 `ProductVersion` 均为 `0.6.73+dc712445ec4baefd9b1cb3173d176c80b697d050`。测试程序集 SHA-256：`9193BC1C08FB20CDE249183246517F23A2334AAD1BF4BA141A5B556F887BC18D`；插件 SHA-256：`F78B4BDC9F3046FDE5F6DF4A8AAA9B8F2851C20709079869B1D8CD505AB7F774`。
- 原一键日志使用 `console;verbosity=quiet`，只留下测试名和 `1 failed / 0 passed` 摘要，没有失败状态、断言消息、堆栈或 TRX。因此该次具体失败状态无法从原日志判断。

## 同一测试程序集复跑

- 直接从用户失败日志中的隔离输出加载**同一测试 DLL**，三个独立 VSTest 进程均为 `1/1 passed`、退出码 `0`；随后使用与隔离脚本相同的 `dotnet test --no-build` 项目入口、相同 DLL 和隔离 `TEMP/TMP` 再运行一轮，也为 `1/1 passed`、退出码 `0`。总计 `4/4`，各轮记录见 [`01`](Q06-replay-01.trx)、[`02`](Q06-replay-02.trx)、[`03`](Q06-replay-03.trx)、[`04`](Q06-replay-04.trx)。前三份记录直接 VSTest，第四份记录项目级入口。
- 首轮报告记录 Light/Dark × normal/hover/pressed/focus/disabled 共 10 个状态全部 `passed=True`。hover/键盘状态是受控 WPF 输入探针；按钮 overlay、opacity 和 scale 的逐状态读数见 [状态报告](button-state-probe-report.txt)。
- 抽查了 Light pressed 与 Dark focus 两张 WPF `RenderTargetBitmap` 图像：[Light pressed](images/light-pressed.png)、[Dark focus](images/dark-focus.png)。它们是 `96 DPI` 合成逻辑窗口图像，不是 Playnite 屏幕呈现或物理输入验收。全部十态截图保存在 `images/`。
- 此复核使用了用户失败工作区的 Release 测试程序集；没有更改按钮生产样式或 Q06 行为测试。因此，四次通过说明此次**未复现**，不能证明先前的失败已修复。

## Runner 诊断调整与边界

- `scripts/run-playnite-tests-isolated.ps1` 将缓冲控制台 logger 调整为 `verbosity=normal`。runner 只会在该类退出非零时回放缓冲输出，所以成功构建仍不输出逐项日志；失败时会保留详细断言与堆栈。PowerShell 语法解析通过。
- 下一步若再次失败，保留新鲜的 `one-click-install.log` 与该 class 的 TRX，按其失败状态和断言继续定位。当前没有足够信息把问题归因到某个按钮状态，也没有宣称 Q06 间歇失败已解决。
- 本次未启动或安装 Playnite；未触发真实 OS 鼠标/键盘输入。系统 DPI 下的实际宿主呈现仍未验证。
