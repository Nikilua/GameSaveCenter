# R22-04 打开路径失败证据

日期：2026-09-21  
分支：`codex/ui-finesse-round2`  
代码基线：`a43a896843d63762ff6473bc664ba6182cb4f8fc`（复用既有实现，未改生产代码）  
任务：R22-04「打开路径失败」

## 现有能力核对

本批先复用 R16 设置路径编辑器和 Dashboard 本地打开入口，没有新造文件服务、权限修改、复制体系或用户数据写入：

- `SettingsPathEditorService.Probe` 对当前字段做只读路径展开、完整路径解析和存在性/类型/可访问性判断；目录会做只读枚举，不创建、删除或修改目录内容。
- 设置页现有链路为“浏览/校验 → 仅打开有效路径 → 复制当前完整路径”。无效路径在打开前显示具体原因，远端文本字段不进入本地打开目录。
- Dashboard 的 `OpenPath` 只对存在文件/目录调用 Explorer；失效路径抛出明确异常，由统一 `RunLocal`/`ReportDashboardFailure` 捕获为状态和错误通知，不把异常抛到 WPF Dispatcher。

## 实际隔离行为

`R16SettingsPathEditorBehaviorTests` 在每次测试新建临时隔离目录，并使用合成文件/目录：

- 有效可执行文件：`IsValid=true`；
- 有效目录：`IsValid=true`；
- 缺失目录：`IsValid=false`，消息明确包含“不存在”；
- 现有文件作为目录：`IsValid=false`，消息明确说明文件不能作为目录；
- 设置目录选项保持 6 个本地字段，远端 `RcloneDestination` 不进入本地打开动作。

测试实际通过 `SettingsPathEditorService.Probe` 和现有设置路径选项，不是只检查 `Assert.Contains` 源码字符串；源码边界测试另确认无效 Probe 在打开前返回、打开方法不做父目录兜底，也保留完整路径复制入口。

## 验证结果

- 当前提交身份 Release 构建 Playnite `net462` / Tests `net472`：`0 errors`，仅 `MediaCenterView.xaml.cs:671` 的 2 条既有 `CS8602` warning。
- `R16SettingsPathEditorBehaviorTests` + `R16SettingsPathEditorSourceTests`：`3/3`。
- `python scripts/validate-source.py`：通过；`scripts/check-xaml.ps1`：24/24；`git diff --check`：通过。

## 边界与交付判断

行为证据只使用合成路径、临时隔离目录和现有 UI/服务契约；没有启动真实 Explorer、访问真实存档/媒体/云端、修改 ACL/权限或外发诊断。Demo 原目录不可用，继续沿用恢复生产基线。

R22-04 按“已满足，待环境验证”收口：失效路径不会静默进入打开动作，用户仍可看到检查原因并复制原始路径。真实 Playnite/package-host 中 Explorer 启动失败、网络共享/ACL/占用等系统级权限边界、最终呈现、UIA/读屏、OS 输入/IME、DPI/跨屏、ETW 和宿主性能仍未验。
