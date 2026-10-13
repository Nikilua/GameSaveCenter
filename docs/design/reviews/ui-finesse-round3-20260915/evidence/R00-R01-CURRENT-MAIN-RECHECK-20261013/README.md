# R00/R01 当前 main 与 CI 复核

## 当前检查点

- 代码提交：`4aa1ec7ef9bafa95758027a9c9ea4f0c43be8c80`（main）。本次证据更正没有修改 R00/R01 产品代码。
- GitHub Actions #968 / run [`38366158855`](https://github.com/Nikilua/GameSaveCenter/actions/runs/38366158855)：SDK8 job `115191682311` 与 SDK10 job `115191682644` 均已完成成功。SDK8 job 的构建、测试、Render QA、WPF 静态审查、扩展包及诊断上传步骤均成功；SDK10 job 的构建、测试与诊断上传步骤成功。
- SDK 基线分开记录：SDK8 job 实际选择 `8.0.423`（仓库 `global.json` 锁定值）；SDK10 job 实际选择 `10.0.401`。两者是独立 job，不能用一个版本代替另一个。
- 本阶段本机没有重跑 Release build 或 R00/R01 行为套件；本机核查为 `python scripts/validate-source.py`、PowerShell AST 解析和 `git diff --check`。#968 的全套测试步骤成功，但本阶段未从其成功 artifact 导出 R00/R01 按类计数，因此不声称新的按类通过数。

## #967 根因与修正范围

#967 / run `38362229559` 的 SDK8 job `115178885652` 在“源码结构检查”失败。用户提供的 `GameSaveCenter-sdk10-build-diagnostics.zip` 是同一 run 的 SDK10 绿色 job `115178885908`，SHA-256 为 `A4D13BB58AC343DFDCFFA6FB4DA31257080F1E5128AF151C946B0AEB0E3BC409`；它记录 SDK `10.0.401`、117 个 TRX、1566 passed / 0 failed，不是 SDK8 失败日志。SDK8 原始日志匿名接口为 HTTP 403/404，run 没有 SDK8 diagnostics artifact。

在当前 checkout 上执行 CI 相同的 `python scripts/validate-source.py`，重现了 `scripts/verify-sdk-build.ps1` 缺 UTF-8 BOM 触发的源码结构检查失败。只给该脚本补了 BOM，校验随即通过；修正已包含在 `4aa1ec7`。这是一条本机同 guard 复现证据，不伪称读取到了原始 SDK8 job log。

## R00/R01 新鲜度

`freshness-summary.json` 是在代码提交 `4aa1ec7` 上生成的精简复核结果，保留整体源提交/包状态和逐项变更路径计数、匹配路径计数及门禁结果，省略完整审计中的 2,491 条仓库级路径清单以节省空间。14 条记录全部 `needsRerun=false`、`reinstallRequired=false`，每条登记的源路径匹配数均为 0；覆盖 R00-01-02、R00-03、R00-04、R00-05、R00-06、R00-07、R00-08，以及 R01-01 至 R01-07。没有仅改 baseline SHA 来制造新证据。

已存在的行为断言仍分别覆盖 R00-01 的非均匀渐变与多状态颜色合成、R00-02 连续 1000 次缩放后的稳定变换树、R00-03 Dispatcher 完成/重入/清钟行为；对应旧证据仍绑定各自记录的源码身份。R00-08 的真实 OS IME 候选确认和 Playnite 输入链仍未验，不能按 freshness 结果关闭环境边界。

## 边界与下一步

- 当前 package identity 为 `not-provided`。#968 公共页面列出了 Windows preview artifact（86.1 MB，SHA-256 `737d7dbd8f9e17573d29c7fa48cfb23d134e73f9faf89e6a5d167a977ace4dad`），本次没有下载或安装该包，也没有把 artifact 元数据当作用户候选包身份。
- 没有启动真实 Playnite，也没有验证最终 presented frame、物理 DPI/跨屏、Narrator/UIA、IME、ETW 或宿主性能。Demo 原目录不可用；沿用已恢复生产基线。
- 下一项依赖已满足的 Q/R 工作：复核 R05-03 弹层快速翻转是否能在已授权隔离 Playnite 宿主中取得真实帧证据。先执行现有 fail-closed 宿主预检；若预检仍受环境限制，记录具体限制并转到下一项可由隔离 WPF 行为验证的任务。

Artifact metadata also listed two diagnostics archives: SDK8 `11798349157` (1.48 MB, SHA-256 `e5d7cca1058417cfa731e81d3f261b1b9a4a9613c9aef9b0d55ea2623ae2cfaa`) and SDK10 `11797519200` (721 KB, SHA-256 `2bee3acdc8ac2cfc1a80da6663bae546c1956e286a75557d1b01ba191d6e3dab`). Neither archive was downloaded; the run page metadata and job steps are the evidence used here.
