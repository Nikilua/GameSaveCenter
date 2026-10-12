# R05-02 当前 main 选项虚拟化焦点复核

日期：2026-10-13

状态：已满足（生产 Shell 列表与 ViewModel 的插件可控行为）；真实 Playnite/物理键盘边界仍待验。

## 当前身份与验证

- 首轮 clean main 复核基于源码/测试 commit `746eaf6a6bb644564e89afaa2c2d08929bc32c45`，只复用已有列表/VM 实现并补证；其后的 Dashboard 行为 fixture 是同一候选工作树的新代码改动，不能把首轮程序集身份当作候选工作树身份。
- 使用隔离 .NET SDK `8.0.423` 构建插件 `net462` 和 Playnite 测试 `net472`；测试宿主为 .NET Framework `4.8.9181.0`。
- 定向运行 `R05OptionVirtualizationBehaviorTests`，结果 `5 passed / 0 failed / 0 skipped`。脱敏 TRX：[R05-02-current-main.trx](tests/R05-02-current-main.trx)，SHA-256 `D00DD5BD7588FE85E5895CBE45F4FCBD2B819EC6272121FB534D17F99F1A7F9C`。
- 同一候选工作树随后合并运行 R05Focus、R05Option 与 R21 focus visual 三个行为类，结果 `11 passed / 0 failed / 0 skipped`；其中 R05Option 为 `5/5`。测试基于 main `746eaf6a6bb644564e89afaa2c2d08929bc32c45` 的未提交候选，`GSC_BUILD_COMMIT` 指向基线。合并预提交脱敏 TRX：[R05-01-02-dashboard-precommit.trx](../R05-01-CURRENT-MAIN-RECHECK-20261013/tests/R05-01-02-dashboard-precommit.trx)，SHA-256 `A8F4E8A68177017A1782B5F60814BC6865073A2190AA4D79762DB9C289333004`；精确提交身份复跑待完成。
- commit `1c39e909856faa736b32224ee196ef0bac6ca460` 的 post-commit 合并运行仍为 `11/11`（R05Option `5/5`），TRX：[R05-01-02-dashboard-postcommit.trx](../R05-01-CURRENT-MAIN-RECHECK-20261013/tests/R05-01-02-dashboard-postcommit.trx)，SHA-256 `DE45C3F9462F44665BDCD2F068E00630377C413D8669B54F4944F3E988312554`。精确程序集 SHA/MVID 见 [R05-01 复核包](../R05-01-CURRENT-MAIN-RECHECK-20261013/README.md)。
- 首轮 clean main 测试程序集 SHA-256 `2ABE260B7635432904C8D063ABADE88508E076FB0052DD29692FFC680D0688B8`，MVID `7a4e4767-6370-4e45-bc69-e74c00284a65`；插件程序集 SHA-256 `FE385B5E23F72BB9AB0803AC20D4D0CFD608ABE7B79B73FE944BEA6B6B8FDA88`，MVID `ed3d4875-5409-4f3e-97ef-9a1107f560d2`。两个程序集的 ProductVersion 为 `0.6.73+746eaf6a6bb644564e89afaa2c2d08929bc32c45`。这些值属于首轮 clean main 运行，不代表新增 Dashboard fixture 的候选程序集；后者待精确提交复跑采集身份。

## 实际行为

- 生产 Shell 的 STA `Window` 中加载 2000 个合成游戏；初次生成的 `ListBoxItem` 只占列表的一部分，证明 `ListBox` 没有一次性创建全量容器。`Down` 将选择移到下一项且选框保持打开；`PageDown`、`End` 后活动项都在实际视口可见，`End` 将滚动位置移离顶部；`Home` 回到首项并回到顶部视口。
- ViewModel 过滤中间选中的游戏时保留同一选择并报告它被隐藏；实际执行已有 `ShowSelectedGameCommand` 可恢复全量结果。刷新数据删除该游戏后，选择回退到首个有效项 `Game 0000`，结果计数为 1999。
- 生产 Shell 的另一个 2000 项用例验证过滤结果仍绑定 `PickerList` 的 `ItemsSource`，过滤列表保持有限视口/容器回收。
- 无 `SynchronizationContext` 的 STA 线程上，防抖刷新实际回到创建它的 Dispatcher 线程；立即 `RefreshNow` 会取消未到期的 debounce，不产生重复刷新。

## 未验证边界

- 键盘使用隔离 STA WPF 窗口中的受控 PreviewKeyDown/KeyDown 路由，不等价真实 Playnite、物理键盘、Windows IME 候选 UI、UI Automation/Narrator、物理 DPI/跨屏、最终呈现帧、ETW 或宿主性能。
- 本次没有运行 RenderHarness 或截图；原始实现证据中的 RenderHarness 只代表当时归档身份，不在这里冒充当前重新渲染。
- 只使用合成游戏 DTO 和隔离窗口；未访问真实存档、媒体或云端。

原始实现证据见 [R05-02-OPTION-VIRTUALIZATION-20260917.md](../R05-02-OPTION-VIRTUALIZATION-20260917.md)。下一项为 R05-03 弹层边缘适配当前 main 复核。
