# R21-02 Maintenance 云端队列筛选器名称与值证据

日期：2026-09-21  
证据提交：`f3eecad0`（`补充云端筛选器行为证据`）  
分支：`codex/ui-finesse-round2`

## 本批范围

本批不修改生产 XAML，只为 Maintenance 已有云端队列筛选器补受控行为证据：

- 云端队列状态筛选：`云端队列状态筛选`。
- 云端队列类型筛选：`云端队列类型筛选`。
- 云端队列时间筛选：`云端队列时间筛选`。

三个 ComboBox 继续使用原有 `CloudTransferStateOptions`、`CloudTransferStateFilter`、`CloudTransferKindOptions`、`CloudTransferKindFilter`、`CloudTransferTimeFilterOptions` 和 `CloudTransferTimeFilter` Binding；没有新增服务、DTO、命令或云端写入语义。

## 验证结果

- `R21AutomationValueBehaviorTests`：`11/11` 通过；新增行为测试实际创建三个 WPF ComboBox AutomationPeer，检查语义名称并验证选值从“全部/全部时间”切换到“待处理/媒体/最近一天”。
- 相关定向筛选：`59 passed / 0 failed / 0 skipped / 59 total`。
- 生产源码契约确认三个名称仍附着于对应 Maintenance 选择器；本批没有只用字符串断言签收交互。
- 提交身份 `GscBuildCommit=f3eecad0` 的 D 盘源码副本 Release 构建目标为 Playnite `net462`、Tests `net472`：`0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning。链接工作树直接触发 WPF `_wpftmp.csproj` 仍遇 `Access denied`，改用项目已有的 D 盘源码副本流程完成同一身份验证，没有绕过权限。
- `python scripts/validate-source.py`：通过；`scripts/check-xaml.ps1`：`24/24` 通过；`git diff --check`：通过。
- WPF 静态检查：`0 errors / 27 warnings / 177 info`，警告和信息均为既有基线，未见本批新增诊断。
- 本批 D 盘源码副本和隔离构建目录已清理。

## 证据边界

证据来自已有生产 Maintenance XAML、合成 WPF AutomationPeer、fake/隔离 testhost 和隔离源码副本。没有运行真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、最终 presented frame 或宿主性能验证；没有以离屏截图或代理性能替代这些事实。Demo 原目录不可用，沿用已恢复的生产资源基线。main 工作区仍有用户改动，本批未碰、未合并。

下一可执行任务：继续盘点剩余复合选择器和逐控件状态/值负例；R21-02 公共门禁完成后再进入 R21-03 错误播报。
