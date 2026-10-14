# Q24-07 当前 main 高对比组合复核

## 结论

代码提交 `0243c108eaa7f3d63430475dd87d292cfcfeb950` 已完成受控 WPF 行为验证。高对比启用时使用系统语义色、关闭透明弹层/玻璃和动效，禁用态维持不透明可辨；键盘焦点、按压、选中项和弹层状态有实际模板/属性断言。退出高对比后再验证 Light、Dark、Follow Playnite 主题以及玻璃/动效请求恢复。

这只证明隔离 STA WPF 窗口和生产资源字典中的行为，不代表 Playnite 宿主呈现或真实系统输入。Q24-07 的本机行为栏通过；推送后的 SDK8/SDK10 CI 结果尚待记录，最终验收仍未完成。

## 构建身份

- 分支/源码：`main` / `0243c108eaa7f3d63430475dd87d292cfcfeb950`
- 本机 SDK：`8.0.423`（隔离安装）；Windows `10.0.22000`；MSBuild `17.11.48`
- 命令：`scripts/build.ps1 -Configuration Release -SkipTests -OutputRoot .tmp/q24-07-official-postcommit-0243c108`
- XAML 结构门禁：`24/24`；Release solution build：`0 warning / 0 error`
- Playnite 插件目标 `net462`，测试目标 `net472`；VSTest `17.11.1`
- 插件 `ProductVersion=0.6.73+0243c108eaa7f3d63430475dd87d292cfcfeb950`，SHA-256 `68CE14E34F913DBC4BF801150F72334A8AAE696CC7A3D4ACA6AF883A66B093E6`
- 测试程序集同一 `ProductVersion`，SHA-256 `585415A96279B3024E9A9AC023A3B30037558CB71EBCC4A23D0B091C21D16454`
- `python scripts/validate-source.py`、`git diff --check` 通过。

## 定向行为结果

七个 Playnite/WPF 测试类按仓库隔离流程逐类执行，每类独立 testhost，避免 WPF 进程级 `Application.Current` 跨 STA 所有权冲突。TRX 均来自上面的精确源码身份。

| 测试类 | 结果 | TRX |
| --- | ---: | --- |
| `Q06ButtonStateSequenceBehaviorTests` | 1/1 | [结果](tests/Q06ButtonStateSequenceBehaviorTests.trx) |
| `R09HighContrastBehaviorTests` | 3/3 | [结果](tests/R09HighContrastBehaviorTests.trx) |
| `R09FocusOutlineBehaviorTests` | 2/2 | [结果](tests/R09FocusOutlineBehaviorTests.trx) |
| `R09ShadowBudgetBehaviorTests` | 3/3 | [结果](tests/R09ShadowBudgetBehaviorTests.trx) |
| `PopupBoundaryBehaviorTests` | 2/2 | [结果](tests/PopupBoundaryBehaviorTests.trx) |
| `PopupLifecycleBehaviorTests` | 1/1 | [结果](tests/PopupLifecycleBehaviorTests.trx) |
| `Q13ScrollBarThumbBehaviorTests` | 1/1 | [结果](tests/Q13ScrollBarThumbBehaviorTests.trx) |
| **合计** | **13/13** | **0 failed / 0 skipped** |

高对比组合覆盖 Follow Playnite、Light、Dark，宿主深浅色，玻璃开/关与动效请求开/关；正例检查系统语义色、禁用按钮和选项、2 DIP 焦点框、选中行、弹层不透明/无阴影、动画时钟清除，以及恢复后普通主题行为。另有测试先确认普通模式按钮/Disclosure 动画时钟活动，再切换高对比验证稳定终态，最后恢复普通模式验证动效重新启用。

WPF 直接构造的 `KeyUpEvent` 不会更新 `KeyboardDevice`。因此按压正例通过真实 routed `KeyDown` 触发；释放测试态由测试子类暴露的受控 `IsPressed` setter 清理，并在测试内明确标注。这不是物理键盘 KeyUp 的验证。

## 未通过的相邻完整门禁边界

同一提交身份单独执行 `R05TooltipTimingBehaviorTests` 得到 `2/3`。唯一失败在建立合成窗口之前，Windows `GetCursorPos` 无法保存测试开始前的系统指针位置；未执行 Tooltip hover/dwell 断言。完整 TRX：[R05 光标前提诊断](tests/R05TooltipTimingBehaviorTests-host-cursor-prerequisite.trx)。没有绕过系统跟踪/输入限制，也没有把它算作 Q24-07 通过项。

## 未验证范围

- 未启动真实 Playnite，未验证实际宿主/最终呈现帧，也未生成截图；受控模板和资源断言不作为视觉签收。
- 未切换 Windows 真正的高对比设置、真实鼠标/键盘输入或 Playnite 原生 UIA/Narrator。
- 未验证物理 DPI/跨屏、ETW 或宿主性能。
- Demo 原目录不可用，按用户要求沿用恢复后的生产基线；没有换用新的设计体系。
- 仅使用合成 WPF 控件和资源；未读取或更改用户设置、存档、媒体或云端。

## 后续

在 `0243c108` 上 freshness 初检为 `14 records / 4 needsRerun`：R00-01-02、R00-03、R00-05、R01-05；其他 10 条未命中。下一小批按原测试范围逐项复核并补真实行为/负例，再更新对应证据基线。包身份仍为 `not-provided`。
