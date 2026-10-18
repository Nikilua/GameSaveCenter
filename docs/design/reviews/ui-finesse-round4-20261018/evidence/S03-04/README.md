# S03-04 恢复方案草稿受控验证

## 结果

实现提交 `1c7083e2`；本次最终验证输入为 `b5bb5158960e05a4c948124d9e29540f4a05812a`（包含 Q06 测试夹具清理修正）。SDK `8.0.423` Release 完整解决方案构建为 `0 warning / 0 error`。恢复映射预览只返回脱敏汇总；草稿最多 50 条，只保存恢复目标/基线引用、映射计数、预览引用、readiness 检查引用及固定保护要求，不保存路径、凭据或执行授权。UI 提供预览、保存、删除，并检查预览 30 分钟新鲜度和当前选择匹配。

## 测试与安装

- Core：`156/156` 通过。
- Worker：`380` 通过、`1` 跳过、`0` 失败。
- Playnite 非 WPF：`475` 通过、`27` 跳过、`0` 失败。
- 定向用例：R09 `2/2`、恢复草稿 `4/4`、侧栏来源 `22/22`、Q06 按下态 `1/1`、恢复映射预览 `2/2`，共 `31/31`。
- 当前 main 的完整一键复跑源提交为 `d4068e6562b7d78a9893211a522059d5d1ce3c9f`。Release 构建成功、0 警告/错误，Core `156/156`、Worker `380` 通过/`1` 跳过、Playnite 非 WPF `475` 通过/`27` 跳过。WPF 跑到 `48/133` 类时停止，已跑用例 `178` 通过、`1` 失败；失败项是 `R05TooltipTimingBehaviorTests.QuickMousePassesCancelTooltipAndAStationaryDwellOpensIt`，无法在启动测试前读取系统指针位置。其余 `85` 个 WPF 类未运行。该失败属于现有系统指针读取边界，不是 R09 焦点描边断言。
- 同一当前 main 构建产物定向 R09 `2/2` 通过。完整一键流程因 R05 失败而在安装阶段之前退出；此前 b5 源码上的独立隔离安装流程已通过。
- 隔离安装流程单独通过：扩展清单 `0.6.73`，插件 DLL `0.6.73.0`。流程未关闭或启动用户的 Playnite。
- RenderHarness 双主题完整矩阵最终 `render-qa OK`。首次运行只有侧栏快速切换时序探针未稳定；不重建的同一 Harness 立即复跑通过。报告和恢复方案草稿截图见本目录。

构建、测试、安装、渲染及静态检查原件分别保存在 `validation/`、`tests/` 和 `screenshots/`。`one-click-partial-summary.json` 是较早手动停止的记录；`one-click-full-run-summary.json` 是当前 main 完整一键复跑，结果为 `outcome=failed`，失败发生在第 48 个 WPF 类。完整复跑已生成的 Core、Worker、Playnite source 和 WPF TRX 在 `tests/full-run-partial/`。构建身份与程序集 SHA-256 在 `validation/build-identity.json`；所有证据文件另有 `validation/SHA256SUMS.txt`。

## 检查

`validate-source.py` 通过；XAML 结构检查 `25/25`；WPF 静态审查扫描 31 个 XAML 文件，`0 errors / 26 warnings / 177 info`。Release 编译与隔离安装分别成功。

## 外机 R09 日志边界

用户粘贴的诊断 `20261018T012631286Z-e027df9d` 显示 R09 在测试阶段失败（预期 `2,2,2,2`，实际 `1,1,1,1`），不是编译失败。该厚度断言与历史版本里把错误态 `Chrome.BorderThickness` 期望为 2 DIP 的写法吻合；提交 `f13b9e05` 已将其改为 Chrome 1 DIP，并另行断言独立 `ValidationStroke` 为 2 DIP。当前源码第 152 行断言 Chrome 为 1，第 154 行断言 ValidationStroke 为 2，最终源码/程序集下 R09 `2/2` 通过。用户暂时没有旧诊断的 `summary.json`、TRX 或 DLL/PDB，因此无法证明该次运行实际用了历史断言版本或确定其具体输入；当前复测也不能反推旧运行身份。

真实 Playnite 宿主启动与呈现未运行；完整 133 类 WPF 隔离循环被 R05 系统指针读取失败中断。
