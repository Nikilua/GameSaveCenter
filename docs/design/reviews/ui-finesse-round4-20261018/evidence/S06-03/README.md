# S06-03 清单浏览整理验收

日期：2026-10-21
实现提交：`0fc43773c56b75ba79bb6e295b5a9a31dec1fa4f`

## 交付范围

任务中心新增人工待办浏览器：可选择清单、按对象类型或引用状态分组、搜索类型/稳定编号/备注/状态，并在未分组、未搜索时用按钮或 `Ctrl+↑/↓` 重排。单项和多项移除都会先确认；确认内容限定为当前清单里的精确条数，保存失败会恢复原清单。业务对象不会因重排或移除而改变。

状态刷新通过 Worker 按稳定键查询当前 SQLite 索引。查到记录显示“索引存在”，校验问题已解决时显示“已解决”，只有精确查询未找到时显示“失效”；未查询和 Worker 不可用分别保持“待刷新”和“无法检查”。状态文案明确不代表源文件存在、可读或可恢复。备份版本使用长度前缀的 PlayniteId+BackupId 组合键，避免按显示名或路径猜测。

## 验证结果

- SDK `9.0.302` Release 完整解决方案构建通过：0 警告、0 错误。构建从系统临时目录调用，以避开仓库 `global.json` 锁定但本机未安装的 SDK `8.0.423`；设置 `MSBuildEnableWorkloadResolver=false` 以避开本机 SDK 缺少 Workload resolver locator 的环境问题。未改动 SDK 锁定或产品构建配置。
- Worker 定向行为测试：2/2，通过精确 ID 命中、未找到、无效类型、已解决校验问题和 200 项查询上限。
- Playnite 定向行为测试：8/8，包含清单有界归一化、稳定引用搜索、分组状态变化和人工状态投影。
- `scripts/validate-source.py .` 通过；WPF 静态审查 0 错误、27 条既有警告、179 条提示；Round4 状态结构校验通过；`git diff --check` 通过。

完整输出与原始 TRX：

- [Release 构建日志](build/release-solution-sdk9302.txt)
- [Worker 测试 TRX](tests/worker-worklist-reference-resolution.trx)
- [Playnite 测试 TRX](tests/playnite-worklist-behavior.trx)
- [源码校验](build/source-validation.txt)
- [WPF 静态审查](build/wpf-static-audit.txt)
- [RenderHarness 构建诊断](build/renderharness-build-blocked.txt)
- [程序集与 TRX SHA-256](verification-identity.json)

## 验收边界

真实 Playnite 未运行。仓库要求的精确 SDK `8.0.423` 当前不可用，因此没有启动 `GameSaveCenter-Run.cmd` 的安装/宿主门禁；没有重试或绕过已有 CEF/ETW 权限拒绝。

RenderHarness 试构建因现有 `tests/GameSaveCenter.RenderHarness/obj/project.assets.json` 缺少 `net472/win-x86` 目标而失败，详见诊断文件；没有为此进行额外还原。离屏截图和模拟上下文不作为真实屏幕证据。本批的受控验收仅证明编译、引用解析与 ViewModel 投影行为。

原有未跟踪 Round3 TRX 与 `tests/GameSaveCenter.Playnite.Tests/TestResults/` 均未暂存或修改。构建临时文件和本批 TRX 在证据归档后清理。
