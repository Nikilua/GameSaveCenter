# S00-03 命令目录键盘交互

日期：2026-10-22  
实现提交：`0968309ca59a87685114be1552ddb8ad09e4bd64`

## 实现

- 搜索结果变更后默认选中第一项；上/下方向键移动选择并保持选中项可见，边界处不越界。
- Enter 只执行当前选中项一次；重新检查可用状态，禁用项不执行也不关闭面板。无结果时 Enter 被面板接收，但不会执行命令。
- Escape 沿用已有 IME composition guard；没有组合输入时关闭面板并把焦点还给触发器。
- `Ctrl+Shift+P` 通过 `AcrylicProductionShellView.InputBindings` 注册在插件控件树上，不注册 Windows 或 Playnite 全局热键。面板/游戏选择器已打开、文本控件持有焦点或 IME 组合期间不会接管该快捷键。
- 搜索框与触发按钮的 Automation 帮助文字说明方向键、Enter、Escape 和插件范围快捷键；结果项使用共享强调色显示选择状态。

## 受控验证

- SDK `8.0.423`、Release；精确源码提交 `0968309c`。Playnite 测试项目及引用项目构建成功，退出码 `0`。
- `CommandDirectory` 筛选：`12/12` 通过，`0` 失败，`0` 跳过。覆盖方向键边界、单次 Enter、空/禁用结果、编辑键透传、IME composition、焦点返回，以及插件 `InputBindings` 的快捷键范围和可执行状态。
- `scripts/validate-source.py .` 和 `git diff --check` 通过。
- 原始 TRX：[CommandDirectory-0968309c.trx](tests/CommandDirectory-0968309c.trx)。源文件、程序集和 TRX SHA-256：[verification-identity.json](verification-identity.json)。

## 尚待宿主验证

受控 WPF composition 事件不等于物理键盘或 Windows 输入法候选行为。本项仍待真实 Playnite 中的控件焦点、快捷键冲突、IME 候选确认和最终呈现验证；不注册全局热键，也不把合成输入称为真实 IME 证据。

用户提供的运行 `20261021T135554848Z-c2927c39` 报告 R09 断言 `Expected 2,2,2,2 / Actual 1,1,1,1`，堆栈指向测试源第 151 行。该次 `artifacts/build-diagnostics` 目录目前不在工作树；当前提交中 R09 第 151 行是颜色断言、描边宽度断言在第 154 行，因此无法确认旧运行实际使用的源码、DLL 或 PDB。此记录保留为无法归因的历史运行；当前提交的完整 `.cmd` 结果将在最终一键验证证据中单独记录。
