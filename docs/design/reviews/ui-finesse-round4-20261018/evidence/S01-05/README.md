# S01-05 临时覆盖标记

代码提交：`dcfca2353b33a7c991d004ba7282e2df2e13847f`（`增加已保存视图临时改动提示`）。

任务与存档表格的命名视图行现在会标记未保存的列宽、列显隐、排序、密度和筛选差异，并提供“恢复已保存”。恢复会重新应用当前视图，不改业务设置。手动筛选状态无法由选中的筛选预设表示时，保存/另存会提示先保存并应用筛选预设，避免显示已保存但刷新后不能复现的状态。默认视图的既有全局列宽/显隐持久化仍按原行为工作。

## 受控验证

- 使用仓库 SDK `8.0.423` 完成提交后 Release 解决方案构建：`0 warning / 0 error`。诊断 ID：`20261018T080234061Z-158c7f0f`；完整摘要见 [build-summary.json](build-summary.json)。构建时源码提交为 `dcfca235`；仓库已有未跟踪历史 TRX 和 `TestResults/`，均保留未纳入提交。
- 七个独立进程的相关测试合计 `29/29`：视图定义 `9/9`、新视图交互 `1/1`、列显隐 `4/4`、列宽 `6/6`、R09 焦点轮廓 `2/2`、R10 筛选预设 `6/6`、Q14 工具栏 `1/1`。原始 TRX 和 SHA-256 见 [tests](tests/) 与 [verification-identity.json](verification-identity.json)。
- `python scripts/validate-source.py .` 与 `git diff --check` 通过；WPF 静态审查 `0 errors / 26 warnings / 177 info`。Render QA 输出 `render-qa OK`，Task `1040×700` 显示 `4/4` 行；结果摘录和截图见 [render](render/)。这是合成渲染，不代表 Playnite 宿主验收。

## 外机 R09 日志对照

用户提供的 `20261018T012631286Z-e027df9d` 日志显示 R09 期望 `2,2,2,2`、实际 `1,1,1,1`，堆栈指向测试行 151；当前源码第 151 行比较错误边框颜色，第 152 行要求 Chrome `1 DIP`，第 154 行要求独立 `ValidationStroke` `2 DIP`。本次相同提交的 R09 为 `2/2` 通过。用户表示暂时没有该次 `summary.json` 或 TRX，故不能拿外机测试 DLL/PDB 与本证据直接比对。日志中的断言/行号和当前源码不一致，可能是旧 checkout、旧测试程序集或 PDB 源映射；在缺少同次身份材料时无法再区分。日志里的 COM 清理异常出现在测试结束附近，和上述断言失败是不同现象。

## 未验证边界

没有运行完整 WPF/安装门禁或真实 Playnite。完整门禁此前在 R05 `GetCursorPos` Win32 error 5 处受系统指针读取权限阻挡；本批没有重试或绕过该条件。Playnite 设置实际保存与宿主双主题呈现待真实宿主复核。
