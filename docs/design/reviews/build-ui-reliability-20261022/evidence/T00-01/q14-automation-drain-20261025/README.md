# Q14 窗口自动化事件收尾复核（2026-10-25）

候选基线提交 `100429d09101da0b7c362b4ef2bc926c67015aae`，SDK8.0.423，build run `20261024T173618477Z-d107dea1`，install run `20261024T173616587Z-be700d2d`。根失败时的Q14测试源码为该提交版本，SHA-256 `CB6731A9CA0C928E1E9E270F94796C479CB85D7843224148D1E07441249623BC`；修正后的工作树测试源码SHA-256 `D6F96A793BDA24A354E08F095F9F4FC6A2EA6A1E5BA1404EA4211F7F284CFDF4`。本页先记录该候选根链失败和随后定向修正；定向结果不替代完整根入口验收。

## 根入口失败

Release solution 0警告/0错误，Core157/157，Worker400/400；WPF第35/145类在 `Q14FilterLabelBehaviorTests` 失败，累计1308项1289通过/18跳过/1失败/0超时或中止。异常为 `WindowAutomationPeer.GetNameCore` 调用 `GetWindowTextLength` 时Win32参数错误，发生在 `FlushLayout` 的dispatcher布局处理中。后续打包/发布/安装/启动未运行。完整摘要、Q14 TRX/console已归档。

对同一提交、同一隔离输出的原测试单独复跑为1/1通过，见 `q14-targeted-100429d0.trx`。这说明根入口失败具有时序性，但单次复跑不足以确认根因。

## 测试夹具修正

每个合成窗口关闭后，等待该窗口Dispatcher到 `ApplicationIdle`，让关闭窗口关联的延迟布局/AutomationPeer回调排空，再创建下一场景。没有屏蔽自动化、跳过测试或更改生产UI。

修正后，SDK8.0.423、`GscBuildCommit=100429d0` 下重新编译测试项目，并以独立测试进程运行Q14六次：首次编译运行1/1，随后五次 `--no-build` 均1/1，合计6/6。每次均退出0、无失败/跳过。逐次TRX及五次console见 [patched-repeats](patched-repeats/)，SHA-256见目录内 `SHA256SUMS.txt`。归档文件已将用户名、主机名和本机绝对构建/结果路径替换为中性值；原始文件保留在本机忽略的 `artifacts/` 目录供本阶段复核。部分进程在测试成功后打印WPF TextServices COM清理异常；VSTest均报告成功并退出0。

这些重复运行本身只覆盖Q14定向类。随后提交 `d0e1192935e7e161a1592d86eccbba3995419146` 的默认 `GameSaveCenter-Run.cmd` 完整链退出0；1967项测试0失败、145个WPF隔离类结束，Worker发布、ZIP/PEXT、安装和Playnite启动均成功。完整结果见[提交级根入口证据](../standard-d0e11929/README.md)。另一台电脑的stash冲突恢复及快进后的完整链仍待用户验证。
