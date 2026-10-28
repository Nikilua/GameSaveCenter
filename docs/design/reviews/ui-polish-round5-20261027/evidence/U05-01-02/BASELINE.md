# U05-01/02 页面与任务详情轻量过渡

日期：2026-10-28

实现提交：`237e993f9e03541f5e8056c30cab8fdb029c0a77`（`补齐页面与任务详情轻量过渡`，已推送到 `main`）

## 去重与变化

- **U05-01 页面切换方向：** 生产 `PageHost` 仅在 `Content` 真正变化后使用共享 `Fast` 时长做淡入和 6 DIP 上移；同一缓存页复用时不重播。动效在响应式布局稳定后开始；Reduced Motion 直接归一化。快速切页用真实导航 `RadioButton` 路由验证最终页、标题、单选状态及键盘焦点。没有增加全页模糊。
- **U05-02 详情打开关闭：** 紧凑任务检查器进入/退出只使用透明度和 RenderTransform，不改变 `Width` 或 `Height`。退出期间保持布局尺寸，关闭命中并将焦点交回“查看任务详情”；反向打开、选择对象变化、转宽布局及降低动态效果都使旧动画时钟失效并收敛到合法状态。行为验证保持任务选中锚点和队列滚动位置。
- `GscMotion.Normalize(FrameworkElement)` 现在能撤销自己在 `TransformGroup` 中拥有的动画变换；`AnimateExit` 使用代次保护，避免过期完成回调折叠刚重新打开的面板。Dashboard 将应用/系统动效策略传给生产任务页。
- 对历史 baseline 去重后，这批只改变 PageHost 与紧凑详情的过渡；命令、数据绑定、任务执行/取消、安全语义、虚拟化、滚动条、游戏选框、错误状态和 Playnite 集成契约未改。没有验证存档恢复、媒体删除或云写入。

## 前后对照

静态布局前后分别来自提交 `48f69413e137bd0b609c5db618914aaeb4690687` 与本批 `237e993f9e03541f5e8056c30cab8fdb029c0a77`，同为离屏 1040×700 DIP。两侧均保留未选详情与选中详情画面；文件哈希见 [screenshots.json](screenshots.json)。这些终态截图用于检查布局/主题没有漂移，不代表中间动画帧或 Playnite 实际呈现。

- Before：[`Task-1040x700.png`](render/before/Task-1040x700.png)、[`Shell-Tasks-1040x700.png`](render/before/Shell-Tasks-1040x700.png)、[`Shell-Tasks-1040x700-closed.png`](render/before/Shell-Tasks-1040x700-closed.png)、[`Task-1040x700-dark.png`](render/before/Task-1040x700-dark.png)。
- After：[`Task-1040x700.png`](render/after/Task-1040x700.png)、[`Shell-Tasks-1040x700.png`](render/after/Shell-Tasks-1040x700.png)、[`Shell-Tasks-1040x700-closed.png`](render/after/Shell-Tasks-1040x700-closed.png)、[`Task-1040x700-dark.png`](render/after/Task-1040x700-dark.png)。

## 验证

- SDK **8.0.423 / Release**：5 项适用行为测试通过，0 失败、0 跳过。包括真实PageHost首次进入/快速导航焦点、源码守卫、任务详情开合反向且不移动队列锚点、Reduced Motion 即时开合，以及系统动效设置热切换清理/不重播。TRX 和命令输出见 [`tests/u05-focused-sdk8.trx`](tests/u05-focused-sdk8.trx) 与 [`tests/u05-focused-sdk8.log`](tests/u05-focused-sdk8.log)。系统动效回归另经三个独立无构建复跑均通过；首次本地合并运行暴露了 WPF 默认 `OnLastWindowClose` 污染后续 STA 测试的夹具问题，已设为 `OnExplicitShutdown` 并在上述最终5项批次通过。该失败不是通过跳过或删断言处理。
- `scripts/validate-source.py` 通过；XAML 结构检查 **26/26**；WPF 静态审查 **37 个 XAML、0 error、26 warnings、179 info**。基线文本见本目录 `source-validation.txt`、`xaml-validation.txt`、`wpf-static.txt`。
- RenderHarness Release **0 warning / 0 error**；提交 SHA 精确匹配的 Render QA **394 PNG、0 `PROBLEM`、`render-qa OK`**。完整文本报告见 [`render/render-qa-report.txt`](render/render-qa-report.txt)，SHA-256 为 `C765430BC402FE81A6C51E6CE312BF19F37535A82EBFBA2EF65D525A858D9824`。离屏报告标记 `WorkingTreeClean=False`，当时唯一已知脏跟踪文件为上一批 CI 证据 `U03-08-U04-01-02/evidence/ci-runs.json`；本批源码已经提交，报告的 source commit 仍准确为 `237e993f…`。
- GitHub Actions push run [1339 / 40623449222](https://github.com/Nikilua/GameSaveCenter/actions/runs/40623449222) 对应精确源码 SHA `237e993f9e03541f5e8056c30cab8fdb029c0a77`，workflow `.github/workflows/windows-build.yml` 的两个适用 job 均成功：基线 job `122602679724` 使用 SDK **8.0.423**，SDK复核 job `122602679492` 使用 **10.0.401**；各自全量测试 **1982 项、1924 通过、0 失败、58 个既有 skip、148 个WPF隔离类**。基线 Release 构建两轮均为0 warning/0 error；源码验证、XAML结构26/26、Render QA 394 PNG/`render-qa OK`、WPF静态审查32 XAML/0错误/26 warning/179 info及扩展包生成全部通过。第二轮打包验证再次完成相同测试集。诊断采集自测中的单个失败TRX结果是专门用于检验失败捕获的预期负例；其诊断预览和诊断测试通过，不能与正式测试失败混淆。
- CI artifacts：Windows预览与扩展包 `12790790406`（90,781,948 bytes，SHA-256 `9bb87173576b5f418fcb43ecc5623afdcd4dab44096eaaaad07259cc31d355fa`）；基线诊断 `12790705921`（1,970,995 bytes，SHA-256 `f34908b12d5e202ecca15930b7836e742055a97d64e991d43ac7444eca7a70c3`）；SDK10诊断 `12788344957`（943,107 bytes，SHA-256 `fda31bce879de5a1f8e9efe260bf16b6ee04d8620699ab50795fd483708e0a8e`）。完整run/job/artifact清单见 [`ci-run-40623449222.json`](ci-run-40623449222.json) 及共享 [`ci-runs.json`](../U03-08-U04-01-02/ci-runs.json)。required-checks接口先前返回HTTP403，保护状态仍标“远端待验”；不从这两个job成功推断分支保护。

## 未验边界

状态保持 **代码完成待验收**：没有在真实 Playnite 宿主观察实际动效；物理键盘、真实 UIA 客户端、IME、跨屏物理 DPI 未测。此批不覆盖 U05-03～08，不据离屏截图宣称 Playnite 渲染效果。最终中文根入口完整构建/测试/发布/打包/安装身份/启动以及独立 `scripts/build.cmd` 仍是整轮硬门禁。branch-protection required checks endpoint此前403，需用可读取权限核对，不能由run绿色推断保护状态。
