# Q06 深色悬停动画采样时序复核（2026-10-25）

## 外机标准入口失败

用户提供的另一台电脑日志对应源码 `d6a401af6151760dda32c772e43fed02d6f68929`、SDK 8.0.423、PowerShell 7.6.7、Windows 10.0.26200，根入口 run `20261025T022508878Z-409c39f7`。Release 编译已成功，但测试在第 29/145 个 WPF 隔离类 `Q06ButtonStateSequenceBehaviorTests` 失败：累计 1299 项，1280 通过、18 跳过、1 失败。失败状态为 Dark hover、`IsMouseOver=true`、`hoverOverlay=0`；根入口未进入打包、安装或启动阶段。

外机此前的 stash 冲突已由用户通过 `git stash push --include-untracked` 保全，工作树清洁后快进到 `d6a401af`。本次失败发生在冲突标记清除后的主线代码上，是另一个测试时序问题。

## 修正与验证

测试夹具在布局后再次合成悬停事件，随后立即读取由 120 ms WPF 故事板驱动的 `HoverOverlay.Opacity`。在不同机器的 Dispatcher 调度下，读取可能发生在动画更新前。测试现于重新应用悬停后将 Dispatcher 推进 180 ms，再检查同一动画属性。生产资源、故事板和原有断言均未修改。

基于提交 `d6a401af6151760dda32c772e43fed02d6f68929` 的补丁源码 SHA-256 为 `FF40E6EAF1064B15F01EA7A1DC047003FEF2AB561D59BF7849F18ED2E9CB25E7`。SDK 8.0.423 下，Q06 定向测试运行 3 次，3/3 通过；每次覆盖 Light/Dark 的十种状态。WPF 状态探针报告中两种主题的 hover overlay 均为 1。

测试修复及本页证据提交：`ccfe4cbf546c0688f171ad5a24d35cc2d4308921`（`修正悬停状态测试时序`）。

随后以隔离输出根运行完整 Release 构建和测试，run `20261025T030714205Z-ab08ddf5`，退出 0：Release 编译 0 警告、0 错误；1967 项测试中 1899 通过、68 跳过、0 失败，148 份 TRX 计数一致，145 个 WPF 隔离类全部完成。逐类 Q06 TRX 与三次定向 TRX 已归档；有限状态探针截图显示 Light/Dark 悬停均已绘制。

## 验收边界

这次完整验证使用 `scripts/build.ps1` 隔离构建/测试链，不包含打包、Playnite 安装或宿主启动。本机已有 Playnite Desktop 和 Worker 进程，因此没有运行会替换安装并启动宿主的默认根入口。T00-01 保持“代码完成待验收”，须由外机在本修复提交上运行 `cmd.exe /d /c "GameSaveCenter-Run.cmd < nul"` 并回传完整结果后，才能恢复标准全链验收状态。原外机 stash 保留，不执行 `stash pop`。

验证数据与归档文件哈希见 [verification.json](verification.json) 和 [SHA256SUMS.txt](SHA256SUMS.txt)。测试 TRX 和控制台输出已将用户名、主机名及仓库绝对路径替换为中性值。
