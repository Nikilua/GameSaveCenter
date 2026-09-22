# R21-02 MediaCenter 收藏开关名称与状态证据

日期：2026-09-21  
提交：`42f5744d`（`补充媒体收藏控件语义证据`）

## 本批范围

- 先复核现有 MediaCenter 能力：当前媒体详情已经有 `MediaFavorite` Binding、`ToggleSwitch` 和原有样式；只发现该开关缺稳定 `AutomationProperties.Name`。
- 生产 XAML 只补 `AutomationProperties.Name="收藏当前媒体"`，保留 `IsChecked="{Binding MediaFavorite}"`、`Content="收藏"`、开/关内容、样式和布局；没有新增服务、DTO、命令、存档/媒体写入或第二套控件。
- 新增 `MediaCenterFavoriteToggleExposesSemanticState`：用实际 WPF `ToggleSwitch` AutomationPeer 检查名称，验证 `Off → On → Off` 往返，覆盖回切负例。

## 证据结果

- `R21AutomationValueBehaviorTests`：`13/13`。
- R21-02 相关进度/焦点/键盘/无障碍/生产壳层套件：`61/61`。
- D 盘源码副本、显式 `GscBuildCommit=42f5744d` 的 Release 构建：Playnite `net462`、Tests `net472`，`0 errors`；仅有既有 `MediaCenterView.xaml.cs:671 CS8602` 两条 warning。
- `scripts/validate-source.py`：通过；XAML 结构检查：`24/24`；`git diff --check`：通过。
- `validate_wpf_ui.py`：`0 errors / 27 warnings / 177 info`，未见本批新增诊断。
- 首次未提交 source-copy 测试被测试夹具按设计拒绝：`GscBuildCommit=working-tree` 与源码根 HEAD `8c003d52` 不一致；提交后以同一身份重建重跑，最终 `61/61` 通过，不把该身份门误记为业务失败。

## 边界与清理

- 只验证合成 WPF Peer、fake/隔离 testhost 和源码副本；未宣称真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、最终呈现或宿主性能已验证。
- 链接工作树 WPF `_wpftmp.csproj` 仍因 `Access denied` 不可直接写入，未绕过权限；本批 D 盘 source-copy/build 已清理。
- Demo 原目录不可用，沿用已恢复的生产基线；main 工作树用户改动未碰、未合并。
- R21-02 仍不整项签收。下一可执行小批量：继续盘点 MediaCenter 备注/元数据动作和其他逐控件状态/值负例；完成公共门禁后再进入 `R21-03` 错误播报。
