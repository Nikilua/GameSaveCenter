# 2026-10-22 标准一键构建验收

## 运行身份

- 命令：仓库根目录 `GameSaveCenter-Run.cmd`
- 源码提交：`22cec5976ee066b32bda07df00eab6d0002f440c`
- SDK：`8.0.423`；Release
- 诊断 run ID：`20261021T171031616Z-c05bcf5a`
- 完整命令退出码：`0`

## 结果

- XAML 结构检查：`25/25`
- Release solution build：`0` warning、`0` error
- Core：`157/157`；Worker：`400/400`
- Playnite：`1,339` 项（`1,282` 通过、`57` 跳过、`0` 失败），共运行 `140` 个 WPF 隔离类
- 全流程 TRX 汇总：`1,896` 项，`1,839` 通过、`0` 失败、`57` 跳过、`0` error/timeout/abort
- 57 项 skip 的诊断文本相同：它们针对已撤销的“今日工作台”UI 架构断言，不适用于当前 AcrylicFork 生产页面；按设计标记为 skip，不是失败。
- `R09FocusOutlineBehaviorTests`：`2/2`；`CommandDirectoryKeyboardBehaviorTests` 在全量隔离序列：`5/5`。R09 TRX 来自本次 run，哈希已记录在 [验证身份](verification-identity.json)。
- Worker win-x64 发布、版本身份校验、`.zip`/`.pext` 打包均成功。安装验证报告确认 `extension.yaml` `0.6.73`、DLL `0.6.73.0`；已安装 DLL SHA-256 与本次 Release 构建 DLL 相同。Playnite `D:\software\Playnite\Playnite.DesktopApp.exe` 已成功启动。

## 原始证据

本地保留的完整阶段日志、逐类 TRX、诊断汇总、Console 输出、打包目录和当前安装报告位于：

- `artifacts/build-diagnostics/20261021T171031616Z-c05bcf5a/`
- `artifacts/gsc-b/3e6bac40/`
- `artifacts/GameSaveCenter-66e9f2d7-67bb-43ef-b62a-b8e60734fcec/`
- `artifacts/GameSaveCenter-0.6.73-playnite.zip`
- `artifacts/GameSaveCenter-0.6.73.pext`
- `artifacts/last-dev-install.txt`
- 安装目录：`%APPDATA%/Playnite/Extensions/GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec/`

诊断汇总和原始 Console 输出、包、安装报告、本次 R09 TRX 及程序集哈希均列在 [verification-identity.json](verification-identity.json)。

此验证覆盖了构建、自动化测试、打包、安装身份和 Playnite 启动。物理键盘操作、IME 候选确认、交互过程中的窗口焦点和最终显示器画面仍需真实交互验收；本次启动成功不代表这些已验证。Round4 整体状态不变。
