# R18-01 连续输入基准定向复核

日期：2026-09-23  
当前复核提交：`0173364e`（`校正R17-08维护报告证据`）  
实现提交：`10bc5789`（R18-01 连续输入基准）  
分支：`codex/ui-finesse-round2`

## 结论

R18-01 在当前可控范围内继续记为“已满足，待环境验证”。本次没有新增生产代码，复用现有 `GamePickerViewModel`、本地同步过滤、20ms `DebouncedRefresh` 和真实 WPF 输入路由；旧证据中的相邻筛选数字按当前筛选命令重新核对，不把宽筛选中的其他 R18 类别写成 R18-01 单项完成。

- `R18ContinuousInputBenchmarkTests`：`1/1`。
- 当前 `GamePicker|DebouncedRefreshTests` 宽筛选：`50/50`。该口径包含已存在的 R18-02 受控窗口夹具、键盘焦点/闭环、大库和 GamePicker 源回归；R18-02 不因被包含而提前签收。
- 当前基准仍断言 2,000 与 10,000 项各 30 次连续英文查询、粘贴、删除和已提交中文查询，最终连续快速输入只产生 1 次防抖刷新。

## 本次原始样本

| 数据集 | 连续样本 | p95 / 最大同步过滤更新 | 过滤评估总数 | 最大托管堆增量代理 | 粘贴 / 删除 / IME 提交 | 防抖等待观测 |
| ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| 2,000 | 30 | 2.350 / 3.825 ms | 60,000 | 303,024 bytes | 1.270 / 4.566 / 1.166 ms | 1 次 / 63.923 ms |
| 10,000 | 30 | 7.297 / 9.657 ms | 300,000 | 1,448,112 bytes | 6.254 / 29.440 / 5.822 ms | 1 次 / 46.203 ms |

本次详细 testhost 输出保留了每档 30 个 `search_ms`、`filter_evaluations` 和 `managed_delta_bytes` 样本；每次过滤评估数分别稳定为 2,000 或 10,000，粘贴查询只留下 1 项，删除恢复完整列表，中文已提交查询只留下 1 项。数据只来自合成 `GameStatusDto` 内存列表。

## 门禁与实现边界

- 隔离 Release solution：`0 errors / 2 warnings`；警告均为既有 `src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml.cs:706` 的 `CS8602`，目标仍为 Playnite `net462`。
- `validate-source.py`：通过；XAML 结构校验：`24/24`；`git diff --check`：通过。
- `validate_wpf_ui.py src/GameSaveCenter.Playnite`：`0 errors / 28 warnings / 162 info`；本批无 XAML/主题资源变更，不把静态扫描当作最终视觉呈现验证。
- 相邻现有 GamePicker/键盘/防抖/大库测试继续保持通过，保留游戏选框、当前滚动条系统、命令绑定、取消/错误语义、恢复保护、虚拟化和有限列表性能；没有把 `FilteredCount`、`GC.GetTotalMemory(false)` 或离屏窗口结果写成 presented frame、ETW 或物理跨屏性能。

## 未验边界

Demo 原目录不可用，本阶段沿用已恢复的生产基线。未运行真实 Playnite/package-host、Windows IME 候选 UI、真实物理键盘/触控板、UIA/读屏、最终 presented frame、物理 DPI/跨屏、ETW/系统跟踪或宿主性能；没有绕过被拒绝的跟踪权限，也没有写真实存档、媒体、云端或诊断目录。

## 下一步

下一可执行任务为 `R18-02 真实 Dispatcher 基准`：复用现有生产 Dispatcher/受控窗口夹具，分别记录 ViewModel 数据完成与窗口可见反馈的原始时间戳，区分容器可见性与 VM 完成，不用 `FilteredCount` 代替画面延迟。
