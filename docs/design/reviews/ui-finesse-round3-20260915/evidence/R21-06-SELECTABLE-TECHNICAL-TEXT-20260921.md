# R21-06 可选择技术文本证据

日期：2026-09-21
分支：`codex/ui-finesse-round2`
任务：R21-06「可选择技术文本」
提交身份：`GscBuildCommit=866ceecde2425c1b5ec159f5012c42023bfae841`

## 现有能力核对

本批先复用已有路径/技术详情能力，再只补一个共享样式和两个版本入口；没有新增服务、DTO、命令或复制通道：

- `GscWpfUiPathDetailTextBox` 是只读原生 `TextBox`，保留完整绑定值、键盘选择和 Ctrl+C；长路径不依赖省略号文本，另有 Tooltip 承载完整值。
- SaveCenter、MediaCenter、TrainerCenter 的路径详情都绑定该共享样式，并把未转换的路径值传给已有 `CopyPathCommand`。
- TaskCenter 的技术详情使用只读 `TextBox` 绑定 `SafeDetailMessage`；已有 `CopyTaskErrorCommand`/`TaskFailureClipboardFormatter` 将任务摘要、错误码、脱敏详情和任务 ID 组成复制内容。
- 新增共享 `GscWpfUiTechnicalTextBox`，将 Dashboard 和 AcrylicProductionShell 的插件版本从普通 `TextBlock` 改为无边框只读 `TextBox`；原有程序集版本赋值代码、布局和导航不变。样式设置 `SelectionBrush`，复制动作仍由原生 TextBox/既有命令通道承载，不触发页面导航。

## 行为证据

复用并重跑既有实际行为测试：

- `R03LongPathTests`：长路径详情在有限宽度内保持完整 `TextBox.Text`，`SelectAll()` 后 `SelectedText` 等于原值；生产路径入口保持预览与完整复制分离。
- `R11DiffListSearchBehaviorTests`：实际 `SaveCenterView` 中的路径详情可全选，复制按钮的命令参数为完整路径，命令执行后复制值保持完整。
- `R15TaskFailureCopyTests`：任务技术详情实际使用只读可选择控件；复制内容保留错误码/技术详情/任务 ID，同时隐藏密码样本；完整详情可通过 `SelectAll()` 读回。
- `R21SelectableTechnicalTextBehaviorTests`：实际加载生产资源字典和 STA WPF `Window`，版本文本可聚焦、可 `SelectAll()` 回读，HelpText 明确提示 Ctrl+C；两个生产侧栏入口均绑定共享样式并具有语义名称。

四组行为合计：13 通过、0 失败、0 跳过。该结果不是新增 `Assert.Contains` 结构断言，包含实际 WPF `TextBox`、`SaveCenterView`、版本选择区和复制格式化行为；源码断言只作为两个生产入口的接线补充。

## 门禁与边界

- 当前提交身份 Release 测试项目编译：0 错误；保留项目已有 `MediaCenterView.xaml.cs:671` 两条 `CS8602` 警告。
- `scripts/validate-source.py`：通过；`scripts/check-xaml.ps1`：24 个 XAML 文件通过；`git diff --check`：通过。
- 选择背景的实际像素对比、真实键盘/剪贴板、Narrator/系统 UIA、Playnite/package-host、DPI/跨屏和最终呈现帧没有在本批宣称已验证；Demo 原目录不可用，视觉依据仍为已恢复生产基线。
- 未访问真实存档、媒体目录、云端、用户云同步或外发诊断。诊断摘要和错误详情原本已是可选择只读文本；剩余未验边界是宿主/系统呈现，而不是继续重建复制能力。
