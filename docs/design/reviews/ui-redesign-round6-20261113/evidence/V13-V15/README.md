# V13–V15 · 微交互、页面切换与存档详情开合

## 本批变化

- `MotionTokens.xaml` 增加语义化时长：hover 120ms、press 90ms、selection 160ms、workspace transition 180ms。共享按钮动效启用时由同一组可反向接续的 hover storyboard 改变透明度；关闭动效时保留立即到位。
- `GscMotion.AnimateEntrance` 支持 X/Y 双轴及透明度一起按有效当前值重新定向。动画generation令被新操作取代的完成回调失效。
- PageHost 更换缓存页面时不先清除旧 clock；新内容在4 DIP短位移和轻淡入中接续当前值。Dashboard里散落的秒数并入共享令牌，降低动效时走统一Normalize路径。
- 紧凑存档详情的开合统一以Opacity和Translate绘制，不逐帧改变宽高或外边距。打开/关闭的业务状态、关闭时的命中测试、焦点目标和最终折叠即时更新；快速反向不排队。

保留真实命令、Binding、当前选中版本、虚拟化、生产滚动条和恢复安全语义；未触碰游戏选框系统。

## 本地行为与静态验证

| 检查 | 结果 |
| --- | --- |
| `R08MotionReverseBehaviorTests` + `R08PageSwitchBehaviorTests` | 11/11通过，0失败/0跳过 |
| 追加详情渲染帧采样 | 1/1通过；打开中间透明度/位移可见；close→open在同一UI操作链内从当前值接续；最终分别收敛到打开和折叠终态 |
| `python scripts/validate-source.py` | 通过 |
| `scripts/check-xaml.ps1` | 27/27通过 |
| `validate_wpf_ui.py src/GameSaveCenter.Playnite` | 0 errors / 26 warnings / 164 info；警告为本轮前已存在的全局静态审查项 |
| `git diff --check` | 通过 |

预提交候选 Render QA 输出335张PNG，`render-qa OK`、0 `PROBLEM`，`WinSta0`私有桌面运行且输入桌面前后为`Default`。但该次自动报告记录的是父提交`6703857a4cc72a5d7847ef93da0d7e8eaeb90414`并标记`WorkingTreeClean:false`；它只作为本地预检，不能代表V13–V15的精确提交验证。提交后需以提交SHA重新跑Render QA及查询GitHub适用job，再补充下表。

| 精确候选门禁 | 身份 / 结果 |
| --- | --- |
| V13–V15源码提交 | 待提交 |
| Render QA | 精确提交待复验 |
| GitHub Windows构建与测试 | 精确提交待查询 |

详情动画行为夹具使用具备`Backups`与`SelectedBackup`绑定属性的内存上下文，避免无DataContext测试表格在首帧后清空选择。测试宿主`ShowActivated=false`，因而没有把这项非激活窗口的焦点设置当成真实键盘焦点验证。焦点目标仍沿用生产视图的详情滚动区/详情按钮代码路径；实际Playnite输入、Narrator/UIA、物理DPI与真实前台动画仍待最终用户环境验证。

本地预提交Render QA输出位于：`C:\Users\lopmatu\.codex\visualizations\2026\11\13\01a1d537-a7d8-7e01-881e-471e263417c2\round6-v13-v15-render-qa`。此目录不在仓库`artifacts/`或`.tmp/`内，也不纳入Git。
