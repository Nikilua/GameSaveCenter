# GameSaveCenter UI Typography + Responsive Closure Plan

来源：`GameSaveCenter_Final_UI_Typography_Prompt.zip`（2026-08-15）
基线：`9eb12e1`（`main` 与 `origin/main` 已同步）
范围：最终 Typography + Responsive Closure。不重构页面整体结构，不改变业务逻辑、命令、绑定、DataGrid 虚拟化或共享滚动骨架。

## 优先级与实施项

1. 统一中文字体链与字重层级
   - 新增 `GscUiFontFamily`（`Segoe UI Variable Text, Segoe UI, Microsoft YaHei UI`）与 `GscCodeFontFamily`。
   - 替换普通 UI 中硬编码的 `Segoe UI Variable Text, Segoe UI`。
   - 保留 `Segoe MDL2 Assets` 图标字体与 `Consolas` 代码/日志字体。
   - 通用按钮默认字重从 `SemiBold` 降为 `Medium`；Primary 按钮单独保留 `SemiBold`。

2. Settings Compact/Narrow Header
   - Compact 隐藏长说明；Narrow/矮窗隐藏副标题与“由 Playnite 保存按钮提交”提示。
   - 缩小 header icon、Padding、MinHeight 与底部 Margin。
   - 目标：Compact/Narrow 下设置正文 viewport 不低于 180-220 DIP，不再只剩几十 DIP。

3. Save Compare Narrow
   - 保持“版本比较 → 保留策略预览”纵向堆叠。
   - 主比较区 `MinHeight=240`、`MaxHeight` 提升到 `height * 0.52` 下限 300；保留策略预览保持独立 Auto 行。

4. Compact Inspector 按钮
   - Save 历史、Save 候选、Trainer 已绑定工具、Media 当前游戏媒体、Task 队列的“查看详情”按钮从覆盖浮层移到表格下方独立操作行，不再遮挡状态文字。

5. Media 待归类底栏
   - “归类到”操作栏与表格内容左边缘统一 12 DIP 水平 padding，窄窗继续自然换行。

## 验收方式

- `python scripts/validate-source.py`
- `powershell scripts/check-xaml.ps1`
- `python .codex/skills/wpf-apple-desktop-ui/scripts/validate_wpf_ui.py .`
- Release 构建与 Core/Worker/Playnite 全量测试
- `scripts/render-qa.ps1`（含 Settings 正文 viewport 探针）
- `scripts/capture-ui-audit.ps1`（0 HIGH / 0 MEDIUM / 0 失败路由）
- 更新 `docs/ai/WORKLOG.md`、`docs/ai/PROJECT_MEMORY.md`、`docs/DEVELOPMENT_HANDOFF.md`
- commit + push `main`
