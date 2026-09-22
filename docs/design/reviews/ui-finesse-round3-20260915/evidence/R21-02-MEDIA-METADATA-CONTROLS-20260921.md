# R21-02 MediaCenter 备注与元数据动作证据

日期：2026-09-21  
提交：`b20dac99`（`补充媒体元数据控件语义证据`）

## 本批范围

- 复用 MediaCenter 详情区现有 `MediaComment` Binding、`UpdateMediaMetadataCommand` 和 `ReassignMediaCommand`；没有新增服务、DTO、命令、写入语义或业务状态。
- 生产 XAML 只补三个稳定语义名：备注 TextBox 为“当前媒体备注”，保存动作按钮为“保存当前媒体元数据”，移动归类动作按钮为“移动并归类当前媒体”；保留原样式、Binding、命令和布局。
- 新增 `MediaCenterMetadataControlsExposeSemanticValueAndActions`：实际 WPF Peer 验证三个名称；通过 `IValueProvider` 将备注从“原备注”更新为“更新备注”；通过两个 `IInvokeProvider` 触发隔离按钮处理器，验证 Save/Reassign 动作通道可调用。

## 证据结果

- `R21AutomationValueBehaviorTests`：`14/14`。
- R21-02 相关进度/焦点/键盘/无障碍/生产壳层套件：`62/62`。
- D 盘源码副本、显式 `GscBuildCommit=b20dac99` 的 Release 构建：Playnite `net462`、Tests `net472`，`0 errors`；仅有既有 `MediaCenterView.xaml.cs:671 CS8602` 两条 warning。
- `scripts/validate-source.py`：通过；XAML 结构检查：`24/24`；`git diff --check`：通过。
- `validate_wpf_ui.py`：`0 errors / 27 warnings / 177 info`，未见本批新增诊断。

## 边界与清理

- Invoke 行为验证的是隔离 WPF Button 的 UIA 调用通道，不宣称真实 `ICommand`、Playnite host 或媒体写入已执行；业务命令与 Binding 仅由生产源码契约保留并检查。
- 只验证合成 WPF Peer、fake/隔离 testhost 和源码副本；未宣称真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、最终呈现或宿主性能已验证。
- 链接工作树 WPF `_wpftmp.csproj` 仍因 `Access denied` 不可直接写入，未绕过权限；本批 D 盘 source-copy/build 已清理。
- Demo 原目录不可用，沿用已恢复的生产基线；main 工作树用户改动未碰、未合并。
- R21-02 仍不整项签收。下一可执行小批量：继续盘点批量媒体动作与其他逐控件状态/值负例；完成公共门禁后再进入 `R21-03` 错误播报。
