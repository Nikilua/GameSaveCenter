# 2026-10-22 S00-04 标准一键构建验收

## 运行身份

- 命令：仓库根目录 `GameSaveCenter-Run.cmd`
- 源码提交：`f8b3144e36d190663f760a98aa0602bec6bda2f8`
- SDK：`8.0.423`；Release
- 诊断 run ID：`20261021T195442896Z-4faecb49`
- 诊断阶段：2026-10-21 19:54:43–20:54:31 UTC
- 完整命令退出码：`0`
- 诊断 JSON：149 步，所有步骤退出码 `0`；143 份 TRX 结果计数与文件内 counters 一致，无解析或计数差异。

## 结果

- XAML 结构检查：`25/25`；Release solution build：`0` warning、`0` error。
- Core：`157/157`；Worker：`400/400`。
- Playnite 源测试：`567` 项，`549` 通过、`18` 跳过；WPF 隔离测试：`780` 项，`741` 通过、`39` 跳过，`140/140` 个隔离类均完成。
- 汇总：`1,904` 项，`1,847` 通过、`57` 跳过、`0` 失败、`0` error/timeout/abort。57 项的原始跳过原因一致：针对已撤销的“今日工作台”UI 架构断言，不适用于当前生产页面。
- `R09FocusOutlineBehaviorTests`：`2/2`，本次 TRX SHA-256：`1DD6A9D8F2DB5BE4007610513F4A1451D7E13FEBD4A4EE4AB112EE0BD192093E`。用户附件记录的描边宽度期望/实际为 `2`/`1`；当前提交标准运行未复现，旧运行使用的源码/程序集身份和根因未知。
- Worker `win-x64` 发布、插件/Worker 共享构建身份校验、ZIP 与 PEXT 打包成功。安装目录 `extension.yaml` 为 `0.6.73`，插件 DLL 为 `0.6.73.0`。Release 构建 DLL SHA-256 `53CD4E0E560CE40252001CA282D4CADA22739140356FA9C1AAACF5F37737C4C2`，与已安装 DLL 完全一致。
- ZIP 与 PEXT SHA-256 均为 `312EF488146E39D21D0F07506FB5A04EDAEDC09F852D53CE03438F51F3F654DB`。Playnite `D:\software\Playnite\Playnite.DesktopApp.exe` 启动成功；检查到进程 PID `44860`。
- 首次未提升的安装尝试被 Windows 沙箱拒绝写入 Playnite 扩展目录；随后在提升权限下从头执行完整标准链并成功。该失败是安装路径权限拒绝，未出现编译或测试失败。

## 原始证据位置

- `artifacts/build-diagnostics/20261021T195442896Z-4faecb49/`：summary、149 阶段记录、Console 输出、143 份 TRX 与程序集身份。
- `artifacts/gsc-b/1808c959/`：本次隔离 Release 构建输出。
- `artifacts/GameSaveCenter-66e9f2d7-67bb-43ef-b62a-b8e60734fcec/`：安装打包目录。
- `artifacts/GameSaveCenter-0.6.73-playnite.zip`
- `artifacts/GameSaveCenter-0.6.73.pext`
- `artifacts/last-dev-install.txt` 与 `artifacts/one-click-install.log`
- R09 TRX：`artifacts/build-diagnostics/20261021T195442896Z-4faecb49/tests/wpf.R09FocusOutlineBehaviorTests/wpf.R09FocusOutlineBehaviorTests.trx`
- 安装目录：`%APPDATA%/Playnite/Extensions/GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec/`

完整启动成功不代表真实页面交互或视觉验收。物理键盘、IME 候选、焦点与宿主快捷键协作、主题/DPI 下的用户屏幕呈现仍待独立验收；原有 Round3 未跟踪 TRX 与 Playnite `TestResults/` 保持未修改、未提交。
