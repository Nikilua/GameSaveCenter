# Q06-02 按下中断后的状态恢复

日期：2026-10-17（Asia/Shanghai）
代码提交：`547efd4badaa462379532c2c0c96eea5c5a5c524`（`补测按钮按下中断后的状态恢复`）

## 行为证据

- 扩展现有 `Q06ButtonStateSequenceBehaviorTests`，复用生产 `GscWpfUiPrimaryButton` 模板，在同一 STA Dispatcher 中以 Light/Dark 各执行一次按下中断序列。
- 用合成 Space `KeyDown` 建立真实 WPF `IsPressed` 状态；先确认缩放小于 `1`、`PressedOverlay` 大于 `0.9`，然后分别通过程序化移焦、禁用按钮、从可视树移除（触发 `Unloaded`）打断按压。
- 每个中断后检查 `IsPressed` 已清除、`PressedOverlay` 回到 `0–0.01`、ScaleX/ScaleY 回到 `0.99–1.01`。fake `ICommand` 在按下及三个中断后执行次数始终为 `0`，因此未释放的按压不会派发命令。
- 原五态 Light/Dark 检查仍在同一测试中通过；没有改生产按钮、XAML、命令或业务取消逻辑，也没有新增屏幕截图。

## 本机运行身份

- Windows `10.0.22000.0`，仓库随附 SDK `8.0.423`，VSTest `17.11.1`，测试宿主 `.NET Framework 4.8.9181.0`。
- Playnite Release `net462`、测试 Release `net472`；定向类 `1/1` 通过，相关项目构建成功，未报告编译警告或错误。`scripts/validate-source.py` 与 `git diff --check` 通过。
- 测试程序集 SHA-256：`0956CF35D0457FA18C40F1EBE6A5C8B509E755DB0EF376CBA84C87030959733F`（1,943,552 bytes）；PDB：`BE425D68416C2F3341D84F41A4585DEFCBBE5CF9708FF26801FA5CADC12AFA9E`（446,640 bytes）。
- 插件程序集 SHA-256：`EE27DADBEB16F68A34EE6A6B17C1FE4EB0B512CE87702D92448996A850702832`（1,830,912 bytes）；PDB：`31967494B152A022C2004D9AC81FA1DD637F9DA908F92CA1BC4A7A0811EBB7FA`（486,456 bytes）。
- 测试源码 `Q06ButtonStateSequenceBehaviorTests.cs` SHA-256：`3A3207CFE4DE6BDB8D4A6914076BE0E81E093806E4C71F35EC233A38E64F55B0`。
- [脱敏 TRX](Q06-02-PRESSED-RESTORE-20261017.trx) SHA-256：`5357AFC6CFCD2BE16629411C0CA93EA2059B4BA8AB9D34592759783EF17E4074`（3,133 bytes，按仓库 LF 规则计算）。原始本地 TRX SHA-256：`E4C2564F258FF7C4ED996A159B38C38B6FB85B75251F79C1742F593AFAE26C96`；归档副本已移除本机名和隔离目录绝对路径。

## 验收边界

- 此结果是合成 Space 键事件与隔离 STA WPF Window 的行为证据，不是物理鼠标按下后移出按钮；真实鼠标、Playnite 宿主与桌面呈现仍待验证。
- 测试证明按压尚未释放时命令不执行；未覆盖真实异步命令启动后的取消回调时序。Q06-02 视觉、宿主和最终验收状态保持未完成，不据此声称整项通过。
- 断言以可见终态值为准；按压释放样式可能保留完成态动画时钟，时钟清理属于 R08 生命周期门禁，不由此项推断。
