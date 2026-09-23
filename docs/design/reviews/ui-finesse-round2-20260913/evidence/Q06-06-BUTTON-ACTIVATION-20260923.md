# Q06-06 按钮激活行为复核

日期：2026-09-23（Asia/Shanghai）

代码基线：`main`，取证时 HEAD `34c9d817`；本项行为测试修改在同一工作树中。

## 复用的生产入口与检查

- 复用 `WorkspaceStatePresenter` 生产模板中的 `RetryButton` 和 `GscWpfUiActionButton`，绑定 fake `ICommand` 计数器；没有新建命令、服务或 DTO，也未访问用户数据。
- 扩展 `tests/GameSaveCenter.Playnite.Tests/WorkspaceStatePresenterBehaviorTests.cs`。Release 测试命令：

  ```powershell
  dotnet test tests/GameSaveCenter.Playnite.Tests/GameSaveCenter.Playnite.Tests.csproj -c Release --no-restore -m:1 -p:BuildInParallel=false --filter FullyQualifiedName~WorkspaceStatePresenterBehaviorTests --logger "console;verbosity=normal"
  ```

- 结果：`8/8` 通过，0 失败。Playnite Release `net462`、测试目标 `net472`；构建无错误。首次完整编译显示已有 `MediaCenterView.xaml.cs:703 CS8602` 警告。
- `Key.Enter`、`Key.Space` 分别经过按钮的 WPF `KeyDown`/`KeyUp` 路由，fake 命令各执行一次；Space 的 `KeyDown` 后同步观察到 `Button.IsPressed=true`。不可执行命令两种键盘序列均为 0 次执行，且按钮禁用。
- 鼠标点击只用反射调用保护的 `ButtonBase.OnClick`，复用 WPF 指针释放后的命令派发终点，确认命令执行一次；不可执行负例同样为 0 次。直接 `RaiseEvent(Button.ClickEvent)` 不会跑该命令路径，因此没有用它冒充点击验证。

## 证据边界

- 键盘是隔离 STA WPF Window 中人工构造并路由的事件，不是操作系统输入。合成 `KeyUp` 不会更新实际 `KeyboardDevice` 状态，因此本夹具只断言 Space 按下期间的 `IsPressed`，不把松开状态或动画时序记为通过。
- `ButtonBase.OnClick` 是点击命令派发路径探针，不是物理鼠标按下/抬起、命中测试或 Playnite 窗口输入。没有截图证明像素反馈。
- 所以本项的自动行为列有受控证据；真实鼠标、按下/松开视觉、动画、UIA 及 Playnite 宿主仍待环境验证，最终结论保持“未完成”。

下一可执行任务：Q06-07 高频连续操作；继续复用真实命令可执行逻辑，并保留真实宿主输入边界。
