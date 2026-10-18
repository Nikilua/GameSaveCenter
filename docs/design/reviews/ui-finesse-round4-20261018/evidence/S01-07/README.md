# S01-07 断点适配记忆

实现提交：`87520a921f6d48ea6864d3f923c762fa2a218e38`（`增加表格列宽断点适配`）。

共享 `DataGridColumnLayoutController` 现在区分用户偏好宽度与窗口窄时的临时有效宽度。网格加载、尺寸变化和列显隐变化时，会按可用宽度从末尾显示列开始压缩像素列至其最小宽度；列宽恢复后回到偏好值。临时适配不会写入设置或命名视图。存档历史原来的单页窄屏列宽分支已删除；候选表状态、置信度列加入可读最小宽度。

## 受控验证

- 提交后使用仓库隔离 SDK `8.0.423` 构建 Release 全解决方案：`0 warning / 0 error`。诊断 ID `20261018T101957814Z-ecc1f9cf`；摘要与插件/测试程序集身份见 [build-summary.json](build-summary.json) 和 [verification-identity.json](verification-identity.json)。
- 六个独立 WPF 测试进程合计 `20 passed / 0 failed / 0 skipped`：R06 列宽 `7/7`、真实窗口 `Loaded/SizeChanged` 列宽恢复 `1/1`、R07 详情断点 `1/1`、R07 缩放压力 `1/1`、TaskCenter 响应布局 `8/8`、R09 焦点轮廓 `2/2`。TRX 见 [tests](tests/)。
- `python scripts/validate-source.py .` 与 `git diff --check` 通过。
- 提交版离屏 Render QA 为 `render-qa OK`；Task 与候选路径表在 `1040×700` 均显示 `4/4` 可读行。报告及代表截图见 [render](render/)。此渲染使用 `1.00` 逻辑 DIP；RenderHarness 不触发列控制器的窗口 `Loaded` 生命周期，实际窗口 `Loaded/SizeChanged` 行为由独立 R06 WPF 测试覆盖。它不代表 Playnite 宿主验收。

## R09 外机失败复核

用户暂时没有外机诊断 ID `20261018T012631286Z-e027df9d` 对应的 `summary.json`、TRX 或 DLL/PDB。Git 历史可确认 `f13b9e05`（2026-10-12）把 R09 错误状态主边框 `Chrome` 的厚度断言由 `2 DIP` 改为 `1 DIP`，并新增独立 `ValidationStroke` 的 `2 DIP` 描边；用户日志的 `2,2,2,2 → 1,1,1,1` 与该旧断言形态一致。当前提交使用全新隔离输出编译，R09 为 `2/2`。由于缺少外机同次程序集和源码身份，无法确认外机实际加载了哪份测试输入，也不将其表述为已复现或已解决。

## 未验证边界

完整 WPF 隔离循环仍会遇到既有 R05 `GetCursorPos` Win32 error 5 系统权限边界；本批没有绕过该限制。未运行安装/版本门禁、真实 Playnite 设置保存或宿主双主题呈现。
