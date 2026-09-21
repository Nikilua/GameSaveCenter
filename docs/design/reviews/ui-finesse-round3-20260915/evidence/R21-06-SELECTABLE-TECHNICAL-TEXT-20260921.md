# R21-06 可选择技术文本证据

日期：2026-09-21
分支：`codex/ui-finesse-round2`
任务：R21-06「可选择技术文本」
提交身份：`GscBuildCommit=e647b5bd`

## 现有能力核对

本批没有新增控件、服务、DTO、命令或复制通道。复用当前生产实现：

- `GscWpfUiPathDetailTextBox` 是只读原生 `TextBox`，保留完整绑定值、键盘选择和 Ctrl+C；长路径不依赖省略号文本，另有 Tooltip 承载完整值。
- SaveCenter、MediaCenter、TrainerCenter 的路径详情都绑定该共享样式，并把未转换的路径值传给已有 `CopyPathCommand`。
- TaskCenter 的技术详情使用只读 `TextBox` 绑定 `SafeDetailMessage`；已有 `CopyTaskErrorCommand`/`TaskFailureClipboardFormatter` 将任务摘要、错误码、脱敏详情和任务 ID 组成复制内容。
- 共享文本框样式设置 `SelectionBrush`，不会用普通 `TextBlock` 冒充可选择文本；复制动作保持命令通道，不触发页面导航。

## 行为证据

复用并重跑既有实际行为测试：

- `R03LongPathTests`：长路径详情在有限宽度内保持完整 `TextBox.Text`，`SelectAll()` 后 `SelectedText` 等于原值；生产路径入口保持预览与完整复制分离。
- `R11DiffListSearchBehaviorTests`：实际 `SaveCenterView` 中的路径详情可全选，复制按钮的命令参数为完整路径，命令执行后复制值保持完整。
- `R15TaskFailureCopyTests`：任务技术详情实际使用只读可选择控件；复制内容保留错误码/技术详情/任务 ID，同时隐藏密码样本；完整详情可通过 `SelectAll()` 读回。

三组既有行为合计：11 通过、0 失败、0 跳过。该结果不是新增 `Assert.Contains` 结构断言，包含实际 WPF `TextBox`、`SaveCenterView`、选择区和复制格式化行为。

## 门禁与边界

- 当前提交身份 Release 测试项目编译：0 错误；保留项目已有 `MediaCenterView.xaml.cs:671` 两条 `CS8602` 警告。
- `scripts/validate-source.py`：通过；`scripts/check-xaml.ps1`：24 个 XAML 文件通过；`git diff --check`：通过。
- 选择背景的实际像素对比、真实键盘/剪贴板、Narrator/系统 UIA、Playnite/package-host、DPI/跨屏和最终呈现帧没有在本批宣称已验证；Demo 原目录不可用，视觉依据仍为已恢复生产基线。
- 未访问真实存档、媒体目录、云端、用户云同步或外发诊断。下一小批应核对版本/诊断文本中仍为普通 `TextBlock` 的入口，再决定是否需要共享可选择样式或仅保留现有选择器/复制按钮。
