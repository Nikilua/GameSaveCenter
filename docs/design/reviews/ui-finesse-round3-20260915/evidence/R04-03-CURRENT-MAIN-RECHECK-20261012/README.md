# R04-03 当前 main 设置草稿生命周期复核

日期：2026-10-12
状态：已满足（插件可控的草稿、标签切换与窗口关闭分支）；Playnite 原生确认框和宿主关闭时序仍待验证。

## 账本条件

R04-03 要求有效草稿不静默丢失；选择继续编辑时回到原焦点，确认放弃后才撤销。质量审查公共门禁要求使用实际事件或受控行为探针、至少覆盖反向/失败路径，并按受控 WPF 与真实宿主分别记录。本次没有视觉布局或共享样式改动。

## 当前复核身份

- 分支为 `main`，测试时 HEAD 为 `e952c414a3941491543ecf558f6f6ab2a68bbf6e`。该次测试工作树仅有本阶段设置关闭接缝和行为测试源码改动；证据文档在测试完成后更新。程序集信息版本以测试时源码基线记录为 `0.6.73+e952c414a3941491543ecf558f6f6ab2a68bbf6e`，不冒充干净提交产物。
- 使用隔离 .NET SDK `8.0.423`，构建插件 `net462` 和 Playnite 测试 `net472`；测试宿主报告 .NET Framework `4.8.9181.0`。本次仅运行定向测试，没有重跑完整 Release、RenderHarness 或真实 Playnite。
- 命令：`dotnet test tests/GameSaveCenter.Playnite.Tests/GameSaveCenter.Playnite.Tests.csproj -c Release --filter "FullyQualifiedName~SettingsDraftLifecycleBehaviorTests|FullyQualifiedName~PortableSettingsTests|FullyQualifiedName~SettingsValidationSourceTests"`。
- 结果 `12 passed / 0 failed / 0 skipped`。脱敏 TRX：[SettingsDraftLifecycle-current-main.trx](tests/SettingsDraftLifecycle-current-main.trx)，SHA-256 `05D22564EEE2234E15AAB22EF92638EAB011BF6CBDAB523566FCFB482AC3C148`。
- 测试程序集 SHA-256 `BFF4AB77E0284CB26575495AA9C8A8B26F011DED49ED6301B096D1E5E305383D`，MVID `0c8d985b-d8ca-482e-b556-0de1b66f0ff5`；插件程序集 SHA-256 `BC002A92C6B99A3E36FAC97A1625B1798E4A58036E771A2EEDB3FBB22468B00D`，MVID `684902f8-2b2c-4afb-84f6-95dd8a063d18`。两者 assembly version 均为 `0.6.73.0`。
- `python scripts/validate-source.py` 通过；WPF 静态审查扫描 36 个 XAML 文件，`0 errors / 29 warnings / 177 info`。本次没有修改 XAML；静态审查中的既有警告不归因于本阶段。R00/R01 freshness 检查了 14 条记录，设置关闭代码路径没有命中登记源码路径，`0` 条需要重跑。

## 实际行为

`SettingsDraftLifecycleBehaviorTests.DraftSurvivesDetachAndWindowCloseChoicesPreserveOrDiscardIt` 在隔离 STA WPF `Window` 中使用合成 `GameSaveCenterSettings`、fake Worker 文件和临时目录：

- 编辑备份间隔后在设置分类标签间切换，草稿值与 `HasPendingEdit` 保持；将设置视图从窗口分离再挂回后，同一草稿仍在，原输入框恢复键盘焦点。
- 显式 `CancelEdit` 将值从 `60` 恢复到基线 `30` 并清空编辑状态，复用既有取消语义。
- 对实际 `Window.Close()` 事件，本阶段添加的内部 test seam 只替换确认结果提供器；生产默认仍调用同一 owner、message、title、`YesNo` 与 `Warning` 参数的 WPF `MessageBox.Show`。返回“否”时实际窗口保持打开，草稿仍为 `60`，标签为 3，原输入框是 `Keyboard.FocusedElement`；返回“是”时窗口关闭，既有 `CancelEdit` 将值恢复到 `30` 并清空编辑状态。
- `SettingsValidationSourceTests` 的源码断言只检查接线和生产默认路径，交互通过以上 STA 窗口事件与状态断言验证，不把 `Assert.Contains` 当成交互证据。

## 未验证边界

- 该行为探针未弹出或自动操作真实原生 MessageBox；真实 Playnite 设置宿主的确认时序、`ISettings` 保存调用和 Worker 应用仍待验证。UI Automation、Narrator、物理 DPI/跨屏、最终 presented frame、ETW 与宿主性能也未验证。
- 只使用合成设置、fake Worker 和隔离临时目录；未修改真实用户配置/存档、删除真实媒体、写入云端或发送诊断。
- 前一提交 `e952c414` 的 Actions run [#939](https://github.com/Nikilua/GameSaveCenter/actions/runs/38270923668) 已终态失败，失败步骤为“编译与测试”，7m11s 后 exit 1。run/job logs 与 artifact API 均返回 HTTP 403；GitHub job 页面要求登录，完整日志及失败根因无法取得。CI 实际 SDK 版本未从本次 run 证实；用户任务补充指定的 CI `10.0.401` 与本机 `8.0.423` 分开跟踪，不以本机结果代替 CI。

原始实现证据见 [R04-03-DRAFT-LIFECYCLE-20260917.md](../R04-03-DRAFT-LIFECYCLE-20260917.md)。R04-04 至 R04-08 已在账本标记满足；后续从依赖已满足的 R05-01 开始当前 main 证据复核。
