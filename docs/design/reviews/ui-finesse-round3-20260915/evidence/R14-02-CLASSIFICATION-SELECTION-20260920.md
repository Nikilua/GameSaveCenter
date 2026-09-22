# R14-02 预览选择编辑证据

日期：2026-09-20  
代码提交：`69cf2a72`（`支持媒体归类预览选择编辑`）  
分支：`codex/ui-finesse-round2`

## 实现事实

- 预览项复用现有批次快照和稳定 `MediaId`，增加 `IsIncluded`；应用请求从当前纳入且可安全应用的项生成 `MediaIds`，排除项不会进入 Worker。
- 高置信且已有建议目标的项才开放目标 ComboBox。用户选择使用稳定 `PlayniteId`，并通过 `MediaClassificationTargetOverrideDto` 随批次提交；低/中置信项仍不能被手工改成可应用项。
- Worker 在应用前按当前游戏目录校验目标、按批次和 `Pending` 状态校验媒体项；合法覆盖目标和“用户在预览中调整目标”原因写回批次记录后重新读取，继续走原有冲突、取消、恢复保护和撤销路径。非法目标记为跳过并保持未归类。
- 汇总显示纳入、可应用高置信和排除数量；没有可应用项时 `ApplyMediaClassificationCommand` 的 `CanExecute` 为 false。Media 预览仍保留有限高度、Recycling、滚动和既有 Inspector。

## 夹具与验证

- Core 增加选择/目标覆盖/稳定 ID 行为夹具；Worker 增加“只应用所选稳定 ID、合法目标覆盖生效、未选项保持 Inbox”的隔离 SQLite/fake 夹具；Playnite 增加 XAML、ViewModel、DTO、Worker 接线源契约夹具。
- `python scripts/validate-source.py`：通过。
- `scripts/check-xaml.ps1 -ProjectRoot ...`：24/24 通过。
- `git diff --check`：通过；Core 测试文件仅有 CRLF 将被规范化的提示。
- Contracts/Core Release 隔离构建：0 warning / 0 error。Core 测试项目在当前 SDK 下的 VSTest/项目引用目标框架评估未产出可运行结果，不能签收定向测试；Worker restore/测试、Playnite `net462`、RenderHarness 和真实宿主未执行。

## 边界与清理

- 验证仅使用合成数据、fake 服务和隔离目录，未读取或写入真实存档、媒体、用户云端或对外诊断；没有绕过被拒绝的系统跟踪权限。
- Demo 原目录不可用，视觉基准沿用恢复生产基线。真实 Playnite/package-host 呈现、物理 DPI/跨屏、UIA/读屏/IME、presented frame、ETW、宿主性能仍未验证；代理性能或离屏结构不等价真实呈现。
- `.tmp/r14-02-build` 已精确删除，未强杀未知进程；main 的用户改动、`src.zip` 未修改、未合并。

下一可执行任务：在可用 SDK/Workload 环境补跑 R13-07/R13-08/R14-01/R14-02 定向测试与相关回归；实际未验边界是当前 Core testhost、Worker/Playnite 编译测试以及目标覆盖/排除项的真实宿主呈现。
