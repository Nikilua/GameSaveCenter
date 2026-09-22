# R16-06 生效条件说明定向复核

日期：2026-09-23

复核提交：`9cef273b`（`codex/ui-finesse-round2`，D 盘工作区）

## 结论

R16-06 的受控实现条件已满足，账本状态校正为“已满足，待环境验证”。本批没有新增生产代码，复用 `83e7c745` 的设置提示和真实消费链标注；源契约测试的范围是文案/链路/负例，不把它写成真实宿主点击或时序通过。

## 实际复测

- `R16SettingsEffectSourceTests`：`2/2` 通过。第一项确认四个分类提示分别对应外观即时重建、下一次任务、下一轮轮询/队列/健康计划和下一次 Playnite 启动，并沿 `EndEdit → NotifyVisualSettingsChanged → ApplySettingsAsync → settings.update → WorkerOptions.Apply/SyncPlan` 检查服务消费点；第二项确认没有“所有修改都需要重启”或“保存后必须重启”的错误文案。
- R16-05 路径编辑与既有设置路径回归：`6/6` 通过，作为本设置页相关回归，保持当前字段 Binding、保存/取消、路径错误边界和剪贴板入口没有被生效说明改坏。
- 当前提交在 `.tmp/r16-06-recheck-20260923` 完成隔离 Release solution 构建：`0 errors / 2 existing warnings`，两条均为 `MediaCenterView.xaml.cs:706` 的既有 `CS8602`；源码程序集身份与 `9cef273b` 一致。
- `python scripts/validate-source.py`、XAML 结构检查 `24/24`、`git diff --check` 通过；WPF 技能静态检查为 `0 errors / 28 warnings / 162 info`，未见本批新增错误。

## 保留能力与边界

- 继续保留游戏选框、滚动条、命令/Binding、保存/取消、错误/取消/恢复保护、有限列表性能和 Playnite `net462`；没有改变 Worker 更新、健康计划、云端时段或安全模式的既有语义，也没有把普通设置笼统改成重启要求。
- 证据使用源码消费链、合成设置回归和隔离构建目录，没有写真实存档、媒体、云端、用户配置或诊断数据。Demo 原目录不可用，沿用已恢复生产基线。
- 未验真实 Playnite/package-host 保存后页面、Worker 重启/轮询时序、最终浅深主题、UIA/读屏/IME、DPI/跨屏、RenderHarness presented frame、ETW 或宿主性能；源码契约测试不替代这些验证。

下一可执行任务：`R16-07 配置导入预览`。先核对现有导入报告、架构/未知字段、凭据边界和失败回退，继续使用 detached/fake/隔离目录。
