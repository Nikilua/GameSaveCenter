# R08 动效测试 CI 失败诊断与夹具修正（2026-10-15）

## CI 失败证据

GitHub Actions run [#1024 / `38677622085`](https://github.com/Nikilua/GameSaveCenter/actions/runs/38677622085) 对应 `main` 基线 `c30b89e2053e509c24ebd4135b3641b8d152867d`，终态为失败。通过 GitHub 插件取得的两个完整 job log 均指向同一测试：

- SDK `8.0.423` job `116234717488`，步骤“编译与测试”失败。
- SDK `10.0.401` job `116234717135`，步骤“隔离工作树并运行 Release 编译与测试”失败。
- `R08MotionHotChangeBehaviorTests.SettingsAnimationToggleEndsEntranceAndReenableDoesNotReplayIt` 在测试源第 49 行抛出 `app-motion test did not start opacity animation; opacity=1; loaded=True`。失败后的 WPF TextServices `InvalidComObjectException` 是清理输出，不是该断言的根因。

测试先在窗口显示前写入 1 秒动效资源；Settings 初次应用自适应主题时又按真实 Windows 动效策略重写这些资源。托管 runner 的系统策略可以把时长设为零，于是行为测试尝试观察的入场动画没有启动。应用设置测试已有实例级开关覆盖，但这不能替代主题资源的系统策略计算。

## 修正与本机验证

只修改两个 R08 行为测试夹具，没有改生产代码或弱化断言。两类测试在窗口显示并完成首轮主题应用后，再写入专用时长资源；应用设置用例继续独立控制应用开关，系统设置用例继续通过实例级 provider 和生产 `OnSystemParametersChanged` 回调模拟系统偏好变化。

- 本机 SDK `8.0.423`、VSTest `17.11.1`；Release 测试项目 build 成功，`0 warning / 0 error`。Playnite 目标 `net462`，测试目标 `net472`。
- `R08MotionHotChangeBehaviorTests`：`1/1`；`R08SystemMotionHotChangeBehaviorTests`：`1/1`；两次均 exit `0`。
- `python scripts/validate-source.py` 与 `git diff --check` 均通过。
- 不涉及 XAML 或生产视觉变化，不适用截图验收。没有运行真实 Playnite，也没有修改 Windows 系统动效偏好。

## 可复核文件身份

以下哈希对应本机 SDK8 Release 产物和当前测试源码；源码改动基于 `c30b89e2`，实际提交由本阶段 Git HEAD 记录。

| 文件 | SHA-256 |
|---|---|
| `R08MotionHotChangeBehaviorTests.cs` | `C41C2E8536B35D734E6B8FB72EAC6CBEBE14B722349694567A0A185DF65CC98B` |
| `R08SystemMotionHotChangeBehaviorTests.cs` | `1D66E35B524D219DE20A65ED3506145D1268C52410A823E273AD20B3DE98D3B3` |
| `GameSaveCenter.Playnite.Tests.dll` | `E3B473255E25C2772D6235660F523E2E9BC20B543E2E5F014F1BFE27EAD21273` |
| `GameSaveCenter.Playnite.dll` | `B452E35E1599BE269F9E19D9E54A3894D19F4C9156E53B23E6AB4468A4FC5D03` |
| `R08MotionHotChangeBehaviorTests-sdk8423.trx` | `BC25921E20A052B54D558E684A55E99E5C3099D6E61D9F7815DDC64DB0C3FD42` |
| `R08SystemMotionHotChangeBehaviorTests-sdk8423.trx` | `BBE4470611CD08C09240E61099831AD9B4923BBA244E3D8B8442F0C267DAB778` |

TRX 位于本目录 `tests/`。本机 SDK8 结果不替代代码提交后的 SDK8/SDK10.0.401 Actions。物理系统偏好通知、真实 Playnite 宿主、DPI/跨屏、最终呈现帧、UIA/读屏和 ETW/宿主性能仍未验证。
