# WPF Apple-inspired Desktop UI Skill

这是一个供 Codex / Agent Skills 兼容工具使用的 WPF UI 专项 Skill，覆盖：

- 信息架构、层级、布局与可读性
- 自适应窗口、DPI、深浅主题与高对比度
- Button、SearchBox、ComboBox、CheckBox、TabControl、ScrollBar、DataGrid 等公共控件
- Dialog、Toast、加载、空状态、错误与进度反馈
- WPF ResourceDictionary、ControlTemplate、VisualState、虚拟化与性能
- Playnite 插件和 GameSaveCenter 的宿主约束
- 静态 UI 反模式检查脚本

## 安装到本机 Codex

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\install_skill.ps1
```

默认安装到：

```text
%USERPROFILE%\.agents\skills\wpf-apple-desktop-ui
```

也可指定目录：

```powershell
.\scripts\install_skill.ps1 -Destination "D:\CodexSkills"
```

安装后重启 Codex。

## 项目接入

将 `AGENTS_SNIPPET.md` 合并到项目根目录 `AGENTS.md`。GameSaveCenter 继续保留并优先读取：

- `docs/design/APPLE_WPF_IMPLEMENTATION_PROMPT.md`
- `docs/design/UI_CHANGE_GATE.md`

## 使用示例

```text
使用 $wpf-apple-desktop-ui 审查并优化整个 WPF 界面。
不要只修截图中的一个控件；检查所有共享样式。
先读取 AGENTS.md 和 docs/design 下的两份 UI 规范。
保持业务逻辑、Playnite 兼容性和列表虚拟化。
```

## 验证脚本

```powershell
python .\scripts\validate_wpf_ui.py D:\path\to\repo
```

严格模式：

```powershell
python .\scripts\validate_wpf_ui.py D:\path\to\repo --strict
```

该脚本是启发式审查工具，不替代 WPF 编译、Windows 渲染和 Playnite 真机回归。
