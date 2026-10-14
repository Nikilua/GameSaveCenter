# R08-01 当前 main 按钮反向行为复核

日期：2026-10-14  
源码基线：`066b9a6c01057c044ddc0d7c63ff70ce3dd7a6b3`  
本机 SDK：`9.0.302`；Playnite `net462`，测试 `net472`  
测试源 SHA-256：`278CB3D2ED588084BF16EDBA881A5D3CD729C5EF66D4449DC90D84954E07DAFA`

## 核对范围

先核对现有覆盖：R08 类已有 `GscMotion.AnimateTranslate` 与生产侧栏开合的中途反向行为；它们检查当前值连续、朝最新目标推进、最终值与动画时钟释放。此前没有覆盖生产共享按钮模板的 hover/press 入场与退出动画反向。Q06 已单独覆盖生产按钮双主题状态、Command/键盘触发和禁用状态。

本次只增加测试夹具，没有改生产 XAML、动画实现、命令、绑定或业务代码。新用例通过实际生产 `GscWpfUiPrimaryButton` 模板观察 `HoverOverlay`、`PressedOverlay` 和 `ButtonChrome` 的 `ScaleTransform`，覆盖 hover 进入后离开并重新进入，以及 press 后释放并快速再次按压。每个切换点检查当前渲染值的连续性；随后检查值朝新的目标推进，并在等待后检查最后输入对应的终态。断言同时会拒绝跳到目标、继续向旧目标运行或未完成终态的行为。

hover/press 状态由 WPF 私有状态入口在隔离 STA Window 中合成驱动，属于生产控件状态和动画时钟的行为测试，不是物理鼠标/键盘输入。Q06 命令/键盘语义仍由原测试覆盖。

## 本机验证

- 隔离 Release build 成功：SDK `9.0.302`、MSBuild `17.14.13`、VSTest `17.14.1`、.NET Framework `4.8.9181.0`；插件 `net462`、测试 `net472`，`0 warning / 0 error`。build commit metadata 使用父提交 `066b9a6`；测试夹具的实际工作树源码由上面的 SHA-256 标识。
- `R08MotionReverseBehaviorTests`：`3/3` 通过；现含 Translate、侧栏和生产按钮 hover/press 三条行为测试。
- `Q06ButtonStateSequenceBehaviorTests.ProductionButtonCapturesAndChecksFiveStatesInLightAndDark`：`1/1` 通过。
- `python scripts/validate-source.py`、XAML structural validation `24/24`、`git diff --check` 通过。
- 按钮探针样本：hover `0.439` 时离开仍为 `0.439`，随后退出推进至 `0.304`；重入切换瞬间仍为 `0.304`，随后推进至 `0.418`，最后为 `0.999`。press scale `0.989` 时释放仍为 `0.989`，朝中性值回到 `0.994` 后再按仍为 `0.994`，最终压下 `0.970`、释放 `1.000`。pressed overlay `0.375` 时释放连续保持 `0.375`，退出推进至 `0.201`；再按切换瞬间仍为 `0.201`，推进至 `0.371`，最终压下 `1.000`、释放 `0.000`。连续阈值为 scale `0.01`、opacity `0.08`；每条轨迹均实际采到中间值。
- 测试进程在 VSTest 成功退出后打印 WPF TextServices `InvalidComObjectException` shutdown 清理异常。根因未查；TRX 计数与退出码均为成功，不将清理噪声解释成测试通过原因。

## 证据与未验边界

脱敏 TRX、插件/测试程序集 SHA 与 MVID、源和证据文件 SHA 见同目录 `tests/`。测试程序集 ProductVersion 的 commit 字段是构建时传入的父提交 `066b9a6`，不可据此误认为未提交测试修改已包含在该 HEAD。

本机自动行为通过没有定位用户报告的 `1/2` 失败。用户失败方法、断言、堆栈和对应 DLL 身份仍缺；#967 的 SDK10 单类 TRX `2/2` 及本次复核都没有复现该故障。当前 SDK9 结果不能代替仓库锁定 SDK `8.0.423` 或 CI SDK `10.0.401`。#971/run `38393317198` 最新公开页面仍显示 `In progress`，用户所附该 run 包仅有 SDK8 诊断；#978/run `38509928015` 已失败，公开 annotations 只有通用 exit code，完整日志需要 GitHub 登录，实际 runner SDK和根因仍未知。

本轮无视觉改动，不生成离屏截图；未启动真实 Playnite、未使用物理输入或验证呈现帧、物理 DPI/跨屏、ETW 或宿主性能。下一可执行项：提交并推送本批，追踪 SDK8/SDK10 Actions 到明确终态；等待期间继续 R08-02 热关闭动效的独立当前 main 复核。用户原始失败若之后可取得，再按具体失败路径定位。
