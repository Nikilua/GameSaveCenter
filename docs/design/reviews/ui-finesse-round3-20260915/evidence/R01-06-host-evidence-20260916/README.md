# R01-06 宿主证据保全归档

## 归档身份

- 审计提交：3929ed73e1056a964d7ceacc54a6046abcc9983e
- 生成时间：2026-09-16T11:19:12.9218368Z
- 插件版本：0.6.73.0
- Playnite SDK：6.16.0.0
- 受控窗口：WPF 离屏窗口，逻辑 DPI 1.0
- 结果：运行时快照 161、Fidelity 警告 0、失败路由 0、HIGH 0、MEDIUM 0

这是在 R01-06 文档提交前对干净代码状态生成的审计；后续提交只增加本归档和 AI/账本文档，不改变被审计的生产代码。

## 归档内容

- [AUDIT_SUMMARY.md](AUDIT_SUMMARY.md)：聚合计数、告警分类和失败路由。
- [audit-metadata.json](audit-metadata.json)：可移植的生成身份、版本、尺寸和输出相对路径。
- [UI_MANIFEST.md](UI_MANIFEST.md)：静态页面、Tab、控件、滚动容器和条件 UI。
- [UI_ROUTE_MAP.md](UI_ROUTE_MAP.md)：页面路由树。
- [UI_FIDELITY_MATRIX.md](UI_FIDELITY_MATRIX.md)：交互入口与快照可见性的逐项矩阵。
- [LAYOUT_REPORT.md](LAYOUT_REPORT.md)：运行时 DataGrid/ScrollViewer 几何结果。
- [EVIDENCE_INDEX.md](EVIDENCE_INDEX.md)：20 个具体控件/状态样本的结果入口、代码身份、样本和边界。
- [screenshots/](screenshots/)：6 张精选代表图；完整 353 张图不提交，按下述命令重现。

## 复核和重现

换机器只需 clone 仓库即可阅读本目录。若要重现同一份身份，先在独立 clone/worktree 中 checkout 3929ed73e1056a964d7ceacc54a6046abcc9983e，再执行：

~~~powershell
$buildRoot = '.tmp\r01-06-reproduce-build'
$auditRoot = '.tmp\r01-06-reproduce-audit'
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\build.ps1 -Configuration Release -OutputRoot $buildRoot
& "$buildRoot\bin\GameSaveCenter.RenderHarness\Release\net472\GameSaveCenter.RenderHarness.exe" audit $auditRoot
~~~

审计仅切换页面/Tab、展开受控 UI 状态并滚动截图；不执行备份、恢复、删除、迁移、下载、设置保存、真实媒体写入、云端写入或诊断外发。脚本生成的全量截图和 JSON 属于可再生临时输出，不应复制回 artifacts/ 或本证据目录。

## 边界

本归档证明的是当前代码身份下的受控 WPF 离屏结果，不等价真实 Playnite 嵌入、用户主题、物理 DPI、OS 键盘/IME、物理鼠标滚轮、presented frame、ETW 或宿主性能。当前游戏选框、滚动条系统、命令/绑定、取消/错误语义、恢复保护和有限列表契约未因本项改变。
