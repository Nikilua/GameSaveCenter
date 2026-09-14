# Q24-03 物理跨屏前置与宿主边界证据

采集日期：2026-09-15（Asia/Shanghai）  
代码基线：`3851228`（承接 `70935fe` 的 `补充物理跨屏审计前置采集`，并修正最终 JSON 深度写入）

## 结论

- 生产实现的可控部分已完成：游戏选框是 `DashboardView` 内的 `GameBrowserPanel` + `GameBrowserScrim`，不创建独立窗口，也不使用 WPF `Popup`；打开状态随宿主页面布局更新。
- ComboBox 的两个共享模板仍使用 `Placement="Bottom"`、`StaysOpen="False"`、动态 Popup 主题资源、有限 `MaxHeight` 和内部滚动；没有新增固定屏幕坐标或独立窗口定位逻辑。
- `scripts/real-host-audit.ps1` 现在在启动前自动写入 `runner-metadata.json` 的 `DisplayCount`、`DisplayTopology` 和 `Q24_03PhysicalCrossScreen.Status`。双屏时状态为 `ready-for-host-replay`，单屏时明确为 `blocked-single-display`。
- 当前机器的 PowerShell 前置检查结果为：`screen-count=1`，仅 `\\.\DISPLAY1`，边界 `0,0,2560×1440`，工作区 `0,0,2560×1368`。因此没有执行“迁移宿主窗口并保持打开态 Popup”的动作，也没有生成跨屏通过结论。

## 可复核检查

| 检查 | 结果 | 说明 |
| --- | --- | --- |
| `python scripts/validate-source.py` | 通过 | JSON/XML/YAML、XAML 语义/资源、C# 分隔符和交付门禁通过 |
| `real-host-audit.ps1` PowerShell 语法 | 通过 | `System.Management.Automation.Language.Parser` 未报告错误 |
| 显示器拓扑前置枚举 | 前置通过/条件阻塞 | `System.Windows.Forms.Screen.AllScreens` 可执行，但当前仅 1 个显示器 |
| Q24-03 源码契约 | 已加入 | `DiagnosticsEvidenceSourceTests`、`UiFinesseRound2ControlSourceTests` 锁定运行器拓扑字段和宿主内浮层边界 |
| 定向 `dotnet test` | 未产生结果 | 当前 .NET SDK/工程解析阶段长时间无输出；未将未执行命令计作测试通过 |
| 真实双屏窗口迁移与打开态 Popup | 待宿主 | 需要第二个物理显示器，并在 Playnite 可见宿主中完成迁移、回迁和截图核对 |

## 证据边界

本文件只证明 Q24-03 的实现约束和真实宿主执行前置状态，不证明 Popup 在第二屏上的实际像素位置、DPI 切换清晰度、窗口回迁后的残留或 Playnite 全局 DPI/Chrome 未变更。获得第二个物理显示器后，应使用隔离 Playnite 数据目录运行真实宿主审计，先确认 `DisplayCount >= 2`，再在 Popup 打开态迁移宿主窗口并保留前后窗口/Popup 截图与 `runner-metadata.json`。
