# 当前提交真实宿主审计边界（2026-09-15）

本记录对应提交 `c5a2997523642ce8608cd376bde0952cc676f44b`，用于区分“当前代码已经被宿主加载并捕获”与“Worker 功能链路已经干净通过”。它不覆盖或改写此前 `69e1f84` 的最终宿主审计记录。

## 已捕获事实

- 运行器使用 `Release`、隔离 UserData：`.tmp/ui-host-userdata-library-final-20260915`，Playnite 可执行文件为 `D:\software\Playnite\Playnite.DesktopApp.exe`。
- 构建结果为 0 warning / 0 error；Core `83/83`、Worker `311/311`、Playnite `499 passed / 57 skipped / 0 failed`。
- `summary.json` 记录 `EmbeddedDashboardCaptured=true`、`EmbeddedSettingsCaptured=true`，两者来源均为 `EmbeddedPlaynite`，提交 SHA 与当前审计提交一致。
- 当前审计目录包含 35 张 PNG：Dashboard 29 个视口、2 个滚动面和 1 个 Settings 视口；`metadata.json` 记录深色主题、150% DPI、`PixelsPerDip=1.5`。这些像素可以作为当前提交的宿主捕获事实，但不能单独证明 Worker 相关行为通过。
- `runner-metadata.json` 记录只有 `DISPLAY1`，因此 Q24-03 仍为 `blocked-single-display`。

## 阻断事实

本轮宿主启动时复用了隔离目录之外仍在运行的旧用户 Worker：

```text
PID: 23304
Path: C:\Users\lopmatu\AppData\Roaming\Playnite\Extensions\GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec\Worker\GameSaveCenter.Worker.exe
Worker identity: 0.6.73+6450f6...
Plugin identity: 0.6.73+c5a2997...
```

因此 Dashboard 截图顶部出现了“Worker 启动进程已退出 / 构建身份不兼容”的真实失败 Toast，运行器汇总 `HighGateCount=1`。运行器明确不会结束其他扩展目录的用户 Worker；针对该精确 PID/路径的强制停止也没有在未获用户明确授权时执行。

## 结论与重跑条件

- 本轮不能标记为“当前提交真实宿主全链路通过”，也不能用这组带错误 Worker 的截图关闭 Q06-05、Q20、Q21、Q22、Q23、Q24、Q25 中依赖 Worker 或真实输入的待验项。
- 共享导航选中悬停状态的代码、源测试和 clean-tree RenderHarness 证据仍以 `nav-priority-20260915.md` 为准；本记录只增加宿主边界。
- 在用户明确允许停止上面已核实路径的旧 Worker 后，应重新运行相同的隔离审计，要求 `HighGateCount=0`、插件/Worker 身份一致，并重新检查 Dashboard/Settings 截图与交互日志。

证据目录：`artifacts/ui-host-audit-round2-nav-20260915`。
