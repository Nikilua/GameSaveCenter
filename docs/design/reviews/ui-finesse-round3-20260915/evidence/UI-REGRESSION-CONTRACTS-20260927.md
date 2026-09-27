# RenderHarness 当前身份复核与共享样式回归断言校准

日期：2026-09-27；产品基线：`d5f0e8c2aa134d76255408c822f63348439e56d9`

## 结果

- 使用已有 restore assets 执行 Release RenderHarness 构建，`0 warnings / 0 errors`；完整双主题、多尺寸运行生成 372 张 PNG，报告标记 `WorkingTreeClean=True`，最终 `render-qa OK`、0 PROBLEM。
- Settings first-viewport normal、dirty、invalid 状态依次显示“已保存”“有未保存更改”“存在校验错误”；summary 可见性分别为 `false/false/true`，全部符合夹具预期。GSC-058 首轮记录的 4 条 SettingsState PROBLEM 本次未复现，根因未知；没有更改 Settings 生产代码或夹具。
- 校准两个 `UiLayoutRegressionTests` 源契约：Save 当前规则刷新按钮使用 `GscIconOnlyToolbarButton`，并核对 `WpfUiProduction.xaml` 中该样式基于 `GscIconOnlyButtonBase`；Settings 恢复默认 Expander 明确使用 `GscExpander`，并核对该共享主题资源的 `TargetType=Expander`。其他六个页面对 `GscExpander` 的既有限制保持不变。
- 当前 HEAD 身份的 Playnite.Tests Release build `0 warnings / 0 errors`；目标用例 `2/2`；`UiLayoutRegressionTests` 整类 `20 passed / 11 skipped / 0 failed`。未运行完整 Playnite.Tests 套件。

## 范围与边界

本阶段只改测试断言和项目记忆/evidence，不改产品 UI、XAML、绑定或命令。首次没有注入 `GscBuildCommit` 的测试运行被仓库源码身份门禁拒绝；随后显式注入当前 HEAD 身份并成功重跑。普通 Restore 因用户级 NuGet.Config ACL 无法读取而失败，验证改用已有资产 `--no-restore`；没有更改 ACL。RenderHarness 是离屏 WPF logical DIP，不证明真实 Playnite 宿主、物理 DPI、UIA 或最终呈现；本阶段未启动 Playnite。

报告/PNG 临时写入 `.tmp/gsc058-settings-state-recheck-20260927/`，记录后清理，不纳入 Git。R 台账仍为 192 项，状态计数 `106/83/1/1/1` 不变。
