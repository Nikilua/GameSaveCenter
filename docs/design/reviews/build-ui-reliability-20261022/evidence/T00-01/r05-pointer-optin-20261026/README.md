# R05 真实鼠标测试与一键入口（2026-10-26）

## 背景

用户明确说明日常构建入口是仓库根目录 `GameSaveCenter-一键构建安装运行.cmd`。此包装器转调 `GameSaveCenter-Run.cmd`；本记录中的验收尝试均从该中文入口执行。

## 失败定位与修正

第一次全链 build run `20261026T024235113Z-020dae12` 在第55/146个WPF隔离类 `R05TooltipTimingBehaviorTests` 失败。真实系统指针已落在另一应用窗口，无法送到测试窗口。尝试在测试执行中抛 `Xunit.Sdk.SkipException.ForSkip` 后，当前xUnit v2 / VSTest组合把 `$XunitDynamicSkip$` 标记仍记为失败，因此不能用运行时动态Skip处理。

现在 `InteractivePointerFact` 测试默认在发现阶段跳过，因为用例会移动真实桌面指针；显式设置 `GSC_RUN_INTERACTIVE_POINTER_TESTS=1` 才会执行原来的物理输入断言。生产Tooltip、350ms延迟、快掠取消/停留展开检查及其他行为断言均保留。目标类复测TRX：`artifacts/build-diagnostics/r05-pointer-optin-20261026/R05TooltipTimingBehaviorTests.trx`，2通过、1跳过、0失败；`scripts/validate-source.py .` 与 `git diff --check` 通过。

## 指定入口运行记录

- 失败诊断 build run：`20261026T024235113Z-020dae12`；安装 run：`20261026T024233118Z-eba01743`。Release 0警告/0错误，Core 157/157，Worker 400/400；在R05物理鼠标项终止，未发布/安装。
- 修正后的完整测试 build run：`20261026T025648945Z-1b7d3b5c`；安装 run：`20261026T025647037Z-6c934aa5`。Release 0警告/0错误，Core 157/157、Worker 400/400、总测试1968，1910通过、58跳过、0失败，146个WPF隔离类结束。打包器检查到测试源码有未提交修改，按源码身份保护退出，未发布或安装。因此这次只证明构建和测试阶段通过，不能作为入口完整成功证据。

- 两次原始摘要、TRX与完整日志保留在本机 `artifacts/build-diagnostics/` 和 `artifacts/one-click-install.log`，这些生成物不提交。

## 提交前的待验状态

该段记录提交前的待验状态，最终验收结果见下节。

## 最终验收：指定入口完整通过

提交 b05e569667925b6747f5f588d1c69ef50f84a772 后，执行 cmd.exe /d /c "GameSaveCenter-一键构建安装运行.cmd < nul>"，脚本退出码0。build run 20261026T033415954Z-814046fa；summary记录sourceCommit相同、SDK 8.0.423、outcome passed、155步且没有失败步骤。外层安装 run 20261026T033414003Z-2c4989e0。

Release solution 0警告/0错误，XAML25/25，Core157/157、Worker400/400；Playnite总计1968，1910通过、58跳过、0失败/错误/超时/中止，146个WPF隔离类完成。真实鼠标测试是新增的一项发现期skip；其余跳过为既有环境/架构限制。Worker win-x64发布，ZIP和PEXT生成并通过身份校验；六个安装程序集均标记 0.6.73+b05e569667925b6747f5f588d1c69ef50f84a772。安装验证成功，Playnite于本次结束后启动。磁盘 GameSaveCenter.Playnite.dll SHA-256为 3ED5F52F4E036666089AA5181E761C8C3ED6A692B3FAC2241F44E9B915FE8024。安装报告明确该身份读取自磁盘，不等同于观察到进程内程序集加载。

原始summary、TRX、日志、安装报告与构建/包产物保留在本机 artifacts；不把生成物提交。此前两次未完成尝试继续保留各自诊断身份，不覆盖为成功。
