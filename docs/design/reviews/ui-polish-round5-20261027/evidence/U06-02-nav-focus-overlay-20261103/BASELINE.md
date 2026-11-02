# U06-02 / U08-08 共享导航焦点框无偏移

日期：2026-11-03。此批跟进首页最近活动选中后整行向右下偏移的反馈，并扩展同一原则到生产共享导航焦点框。

## 变更

- 最近活动的选中框修复保留在生产代码中：Overview 的蓝色圆角框仍可见，`TaskItemChrome` 不再通过 `Margin=4,2` 推动内容。公共隐式 `ListBoxItem` 的选中/焦点描边继续位于内容层后，未改变其游戏选择、Binding、虚拟化或滚动行为。
- 本批复核 `AcrylicNavItem`：此前键盘焦点会把装有 `ContentPresenter` 的内部边框从 1 DIP 改成 2 DIP，并把原选中色框也改成蓝色，出现内容内缩及双层蓝框。模板现把焦点框放在非交互 sibling overlay；焦点变化不再改动内容边框宽度/颜色。选中填色、焦点蓝色圆角框、RadioButton、焦点视觉样式、鼠标命中、导航 Tag、Checked 处理器和 UI Automation 名称保持。
- 已盘点其他主要共享选中/焦点样式：`DataGridRow` 的行框与背景在内容之外，表格选中/焦点几何已有 Light/Dark 回归；媒体选择框有独立 overlay；Overview 保护卡与 ComboBox 状态只改色。没有把无偏移控件为统一外观而改写。

## 本地验证

- SDK 8.0.423 Release 精确构建 `R23ProductionResourceStateBehaviorTests.AcrylicNavigation` 的两条 WPF 用例，2/2 通过、0 失败、0 skip。两种主题分别覆盖未选中、选中、选中且键盘聚焦、焦点移出；断言 ContentPresenter 的相对导航边界和边框宽度稳定，焦点 overlay 始终可见且不命中鼠标。
- 第一次过宽筛选额外命中 `UiFinesseRound2ControlSourceTests` 的源码身份守卫；直接调用没有注入 `GSC_BUILD_COMMIT`，该守卫失败，不是导航断言失败。收窄筛选后两条 R23 WPF 用例均通过。两个原始 TRX 的 SHA/计数见本地审计路径 `artifacts/ui-qa/U06-02-selection-frame-align-20261102/focused-tests/`；测试编译临时目录已按 `cleanup.json` 清除。
- `python scripts/validate-source.py` 退出码 0。WPF 静态检查覆盖 218 份 XAML，0 errors、208 warnings、1327 info；warning/info 包含仓库保留的历史 `.tmp` 快照。
- UI_CHANGE_GATE 的 `scripts/render-qa.ps1` 退出码 0，Light/Dark 共 394 PNG、0 `PROBLEM`。第一次输出在源码工作树未提交时生成（报告 SHA `CFECEFC15B66AB7F66E3442933CF847F1A2701B10A4601326AB04F709CEDF073`，报告标记 `WorkingTreeClean=False`），所以它只证明该工作树渲染成功。冻结候选提交后会在原审计目录覆盖运行，另记绑定正式候选 SHA 的报告身份。

## 交付边界

本阶段源码提交后的中文根入口、独立 `scripts/build.cmd`、安装身份、启动和 GitHub 双 SDK jobs 尚未运行；上一冻结候选的通过记录不用于替代。Playnite 实际页面、外部 UIA、实体键盘/IME、物理 DPI 与宿主焦点行为未验。没有执行真实存档恢复、媒体删除或云端写入。

磁盘起点、历史构建保留理由与本阶段临时输出见 [`stage-start.json`](stage-start.json) 和 [`cleanup.json`](cleanup.json)。
