# 130cd307 默认标准入口完整通过

源码 `130cd3073a19a7c262e34d4ac1f1c9a5da19b3e4`，默认 GameSaveCenter-Run.cmd，终端退出0；run `20261024T071312317Z-8b9a34fd`，SDK8.0.423，隔离输出 artifacts/gsc-b/1bb3e482。[核验记录](verification.json)、[原入口控制台脱敏副本](root-console.txt)、[安装报告](last-dev-install.txt)。

XAML25/25、Release0警告/0错误。1967项：1910通过、57既有跳过，0失败/错误/超时/中止；148TRX计数一致，145WPF隔离类全部结束。57个跳过名称与上一成功190bbaea完全一致。前次崩溃的R12RemoteStageProgressBehaviorTests本轮5/5。原摘要、TRX和逐类日志保留在原诊断目录，verification保存其路径和摘要SHA，未因复制证据删除失败历史。

发布、ZIP/PEXT、安装、启动均完成；ZIP/PEXT实际SHA相同，六个关键程序集安装/暂存SHA与完整ProductVersion一致。15:39新Playnite与Worker进程路径/启动时间见核验记录；[插件加载原日志脱敏摘录](plugin-load-log.txt)确认实际加载路径。启动及磁盘身份不替代页面视觉审计；没有新增双主题、物理键盘/IME或DPI结论。

结合[真实中文空格长路径构建与入口修复](../actual-path/README.md)，T00-04验收：原工作区未移动，未复制真实用户数据，混合模块环境故障保留且修复后同条件成功；最终CRLF入口双宿主三入口各9/9，且当前提交完整链成功。

[克隆清理](path-clone-cleanup.json)：前次17个分析器占用本次复查已为0；确认规范路径在.tmp、无reparse、6211文件均可独占打开且克隆构建均已结束后，删除约546MB隔离克隆。原失败/成功日志和摘要已归档，当前标准输出/包/诊断、旧唯一失败DLL及用户历史TRX/TestResults均保留。克隆临时路径仅用于历史身份说明，不作为仍存在的交付链接。
## 另一台电脑的 R09 日志对照

用户后来附的失败日志与旧测试源码完全吻合：`3f7d0b30` 在 SelectedTabAndInvalidTextBox... 中要求 `errorChrome.BorderThickness = 2 DIP`，并在该机读到1。主线 `f13b9e05` 起改成 Chrome 1 DIP、独立 `ValidationStroke` 2 DIP，不随验证状态改变内容测量；当前测试分别检查这两个角色。`130cd307` 的完整链包含同一类2/2通过结果。因此不能把旧断言或另一台机器的行号当作 `130cd307` 源码失败，也不能据此归咎于.NET Framework补丁版本。历史详细身份边界：[外机 R09 复核](../../../../../../design/reviews/ui-finesse-round3-20260915/evidence/CI-R09-OTHER-MACHINE-FAILURE-RECHECK-20261014/README.md)。

粘贴日志未提供外机 HEAD、dirty source、测试 DLL/PDB SHA；可确认测试输入过旧，具体是 checkout 未更新还是实际测试程序集/符号来自旧输出，仍无法仅由日志区分。先在外机仓库运行 `git status --short --branch` 与 `git rev-parse HEAD`；不要覆盖本地改动。目标提交是 `130cd3073a19a7c262e34d4ac1f1c9a5da19b3e4`。若有本地改动先备份/提交后再快进更新；最终核对测试第152行 Chrome=1、第154行 ValidationStroke=2。其后再从新诊断目录确认源码提交与测试DLL/ProductVersion匹配。

逐项可复查的版本对照：[历史旧断言失败 TRX](../../../../../../ai/evidence/close-picker-20261012/targeted/r09-focus-repro.trx) 正是 `Expected 2 / Actual 1`；该文件源于焦点模板旧实现/旧测试契约。旁边的 [130cd307 当前完整链 R09 TRX](R09FocusOutlineBehaviorTests.trx) 两项均通过。此次用户粘贴还出现的 PNG iCCP 警告与 COM 收尾错误，不在失败的断言成因中。
