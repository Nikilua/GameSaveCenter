# R21-01 八入口纯键盘（2026-09-21）

## 范围与事实

- 先复核 Q24 键盘/焦点审查、现有 `KeyboardFocusSourceTests`、`GamePickerKeyboardBehaviorTests`、R05 焦点边界测试和八个生产入口；没有重建服务、DTO、命令或导航模型。
- 最新实现已有 Shell 七个导航入口及六个工作区的安全动作。实际缺口是 TrainerCenter 默认“已绑定工具”页的工具栏按钮没有稳定的 Automation 名称；本阶段为四个已有命令补名称，命令和 Binding 不变：`导入修改器`、`导入工具目录`、`导入 Cheat Table`、`添加工具启动项`。提交 `c3459ebd`。
- 新增 `R21KeyboardNavigationTraceTests`：逐一创建 Dashboard Shell、Overview、SaveCenter、TrainerCenter、MediaCenter、TaskCenter、Maintenance、Settings 八个生产 `UserControl`，放入 STA WPF `Window` 和独立焦点范围，记录 `Tab`/`Shift+Tab` 等价的 `TraversalRequest(Next/Previous)` 实际焦点轨迹。每页要求前后向至少两个焦点停靠点、所有停靠点都在入口范围内，并且前向轨迹命中真实 Automation 名称或现有安全命令锚点。
- 轨迹锚点为：Shell 七个导航名；Overview 的刷新/备份/同步；SaveCenter 的扫描/校验/详情重载；TrainerCenter 的导入修改器；MediaCenter 的媒体归类批次历史；TaskCenter 的搜索/刷新；Maintenance 的状态/诊断刷新；Settings 的搜索/分类导航。Media/Trainer 没有把默认不可见的“当前游戏搜索”误写成已达成证据。

## 验证结果

- R21 新增测试：`2 passed / 0 failed / 0 skipped`。
- 相关焦点/键盘/无障碍/生产壳层回归，在隔离 source-copy 中显式设置 `GSC_BUILD_COMMIT=82d00b0f` 后：`31 passed / 0 failed / 0 skipped`。覆盖 R05 弹层焦点边界、ComboBox 方向导航、游戏选框方向键/Enter/Esc、选框关闭后焦点返回、Automation 名称和共享焦点样式源码契约。
- Release 隔离构建：Playnite `net462`、Tests `net472`，`0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671` `CS8602` warning；`validate-source.py`、XAML `24/24`、`git diff --check` 通过。WPF 技能静态审查报告 `0 errors / 27 warnings / 177 info`，警告均为仓库既有布局/资源审查项，本阶段未新增对应警告。
- 首次相关回归未注入 `GSC_BUILD_COMMIT` 时有 `14` 条统一身份门禁失败；补齐仓库要求的构建身份后重跑为上述 `31/31`，未将身份门禁失败记作产品行为失败。

## 证据边界与后续

- 证据使用真实生产 WPF 视图、STA testhost、合成宿主资源和隔离 source-copy；Settings 的独立构造仅补了缺失于 Playnite 宿主的 `BaseTextBlockStyle`，没有修改生产资源。没有调用真实命令，不写真实存档、媒体、用户云端或诊断数据。
- 这次确认的是受控 WPF 焦点行为和现有方向键/Enter/Esc 夹具契约，不宣称真实 Playnite/package-host、Windows 原生 OS 输入、UIA/读屏、IME、物理 DPI/跨屏、最终呈现或宿主性能已验证。八入口的逐页方向键/Enter/Esc 仍应在宿主验证阶段补实机轨迹；当前没有把离屏窗口或 testhost 当作呈现证据。
- Demo 原目录不可用，沿用已恢复生产基线；main 工作树用户改动未碰、未合并。阶段临时 source-copy/build 目录在文档提交前按精确路径清理。
- 下一可执行任务：`R21-02` 控件名称与值，先复用现有 UIA/Automation 名称，盘点图标按钮、复合选择器、开关和进度条的名称/状态/值与负例。
