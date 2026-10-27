# U00-02 / U00-03 — 存档页资源归属与信息层级

日期：2026-10-27。界面方向仍以Demo-first为准；当前checkout没有Demo源文件，本批在真实生产WPF资源和RenderHarness上实现与核验。

## 本批实现

### U00-02 控件资源归属

- `SaveCenterView.xaml` 页面资源原先同时合并 `DesignTokens.xaml`、`WpfUiProduction.xaml`、`Redesign.xaml`。生产 `Redesign.xaml` 已在自身字典作用域合并 `Typography.xaml`、`MotionTokens.xaml`、`WpfUiProduction.xaml`、`ButtonStyles.xaml`；`WpfUiProduction.xaml` 再合并 `WpfUiBase.xaml`、图标包及动效令牌。页面重复合并 `WpfUiProduction.xaml` 已移除，保留先载入的 `DesignTokens.xaml` 与 `Redesign.xaml` 顺序。未触及Playnite应用级资源。
- 移除 `SaveTabControl` 页面别名样式：它仅以 `GscRedesignWorkspaceTabControl` 为BasedOn，重复设置四个已在TabControl实例上明确声明的Stretch对齐属性。该实例现直接引用共享 `GscRedesignWorkspaceTabControl`；页签项目仍使用既有 `SaveTabItem`。未复制或改写共享模板。

### U00-03 信息主次示范

- 将页签内容标题从重复的“历史版本”改为“存档清单”，页签名称仍是“历史版本”；标题旁的版本数上下文保留。
- 操作顺序调整为主要数据操作优先：“立即扫描”作为Primary，其后依次为“预览备份”“重新校验”“重新加载详情”；历史日期范围/跳转/加载和列、视图配置操作排在后面。
- 只调整标签与布局顺序。真实命令、Binding、ToolTip、AutomationProperties、日期范围、列与视图配置、数据表、恢复/详情审阅面板均保留；未减少信息或改变危险操作、安全语义。游戏选框和现有滚动条系统未改。

## 前后证据

- 当前候选截图与SHA-256见 [screenshots.json](screenshots.json)：Light/Dark主题、1040×700与1366×768 DIP。RenderHarness继续覆盖Light/Dark及1040、1100、1280、1366、1536、1600、1707、1920、2048、2560、3840等受控逻辑宽度；全部无 `PROBLEM`。此为离屏样本，不表示Playnite宿主/物理DPI/真实输入已验证。
- 1040 DIP首屏保留原表格与详情内容；主操作更早出现，历史筛选和视图配置仍可访问。宽布局继续显示详情检查器。没有改变DataGrid列、虚拟化或滚动模型。
- 全量WPF静态审查扫描31个XAML：0 error、26 warning、179 info，与U00-01基线一致。warning/info主要位于未改动页或共享/示例资源；本批目标页没有新增审查问题。

## 构建与行为验证

- `python scripts/validate-source.py`：通过（包含XAML语义/资源、启动器和版本等源码门禁）。`git diff --check`：通过。
- WPF RenderHarness Release，SDK 8.0.423：build 0 warning/0 error，`render-qa OK`、exit 0、0 `PROBLEM`。原始RenderHarness输出保留在 `.tmp/ui-polish-round5-u00-01`，报告SHA-256 `D14B1D188E172F3917B26668230833A6B58E15F766FFB984C4A8A1397125858A`；四张本批截图从同一输出归档。
- `WpfUiResourceDictionaryTests.SaveCenter*`：SDK 8.0.423，3 passed、0 failed、4 `NotExecuted`（既有条件性skip，不计通过），见 [TRX](tests/SaveCenterLayoutTests-sdk8423-head-ac18.trx)。同组最初一次调用缺少构建身份环境变量，产生2个环境设置失败；补设 `GSC_BUILD_COMMIT` 后重跑通过，初次TRX仅作诊断保留，见 [初次诊断](tests/SaveCenterLayoutTests-sdk8423.trx)。
- 标题层级回归 `RestoredAcrylicForkBaselineTests.SaveCenterUsesTheRestoredBackupPolicySections`：更新断言以同时确认页签仍为“历史版本”、内容标题为“存档清单”；SDK8.0.423本机1 passed，见[TRX](tests/SaveCenterTitleHierarchy-sdk8423-58872.trx)。旧断言期待内容标题仍重复为“历史版本”，已在远端SDK10复核中被发现并按新页面合同修正，没有删除断言。
- 媒体比较既有行为回归 `ProductionButtonsKeepSelectedPairAcrossFilterChangesAndClearItOnRequest`：测试夹具显式将被测生产Grid设为640/500 DIP，完整保留宽/窄两种生产分支断言，以避免把runner顶层窗口的请求尺寸误当作实际子视口；SDK8.0.423、`GSC_BUILD_COMMIT=3b97af01`定向复核1 passed，见[控制尺寸TRX](tests/MediaComparisonSelectionTests-local-controlled-width-sdk8423-3b97.trx)。先前原夹具的本机复核TRX仍保留作历史对照，见[原TRX](tests/MediaComparisonSelectionTests-local-repro-sdk8423.trx)。本修正仅调整测试输入，没有改变媒体页面生产布局。
- 定向测试构建所记录的 `GscBuildCommit` 为父提交 `ac18a918`，并非之后的源码候选提交身份；工作区当时包含本批XAML变更。源码候选完整根入口和精确提交CI仍须按交付协议执行。

## 临时输出与空间

本批复用已有 `.tmp/ui-polish-round5-u00-01`，没有新建临时目录、安装包或审计副本。继承的入口快照为395文件/53,094,868字节，D盘最近记录余量7,375,695,872字节；本批开始前没有独立复测磁盘余量，故不将该数写成精确阶段起点。Render QA复核后该目录为395文件/53,058,496字节，D盘余量12,544,065,536字节。当前四张截图归档副本共363,948字节并有SHA清单。完整测量和保留理由见 [cleanup.json](cleanup.json)。U00-01同一路径的精确PowerShell清理命令已被执行策略拒绝，本批没有重复尝试；该目录保留最新RenderHarness原始报告供当前审计，其他任务TRX/TestResults未触碰。

## 未验证边界

真实Playnite呈现、跟随宿主主题、系统物理DPI/100–200%缩放、UIA树与键盘/IME实操、实际呈现帧及完整启动安装链未由离屏检查覆盖。恢复存档、媒体删除和云端写入均未触发。首次环境变量缺失的TRX和四项既有skip都不视为通过。
