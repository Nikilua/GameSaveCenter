# S02-07 重复归并摘要

实现提交：`9ed6b4e5`、`c4fb56a0`。用户从通知或任务记录重试时，原任务的问题身份会随备份、媒体同步及云端上传请求进入 Worker 并持久化到新任务。相同问题链的失败任务归并到一条本地通知；不同游戏、任务类型或根问题 ID 保持分离。

通知记录展示累计次数、首次/最近发生时间和最后错误，并保留首次失败与最近任务的稳定 ID 跳转。事件 ID 使重复投递幂等；异步乱序写入时仍按发生时间更新首末任务、时间与错误。重试成功将问题标记为已恢复但保留历史错误与任务入口；后续重试再次失败会重开告警。Worker 启动时将旧版单任务失败通知迁入新字段；任务表仍是任务状态的权威来源。

## 验证

- 使用 SDK `9.0.302` 对最终提交 `c4fb56a0` 执行 Release solution restore/build：`0` warnings / `0` errors。restore/build 输出见 [restore.log](build/restore.log) 和 [build.log](build/build.log)；[验证身份](verification-identity.json)记录 SDK、命令、源码与加载程序集的 SHA-256。本机没有 `global.json` 锁定的 SDK `8.0.423`。
- Core 全量测试 `147/147`；Worker 全量测试 `374 passed / 1 skipped / 0 failed`；通知持久化 `12/12`；Playnite 通知归并、重试来源与 R09 定向测试 `19/19`，其中 R09 `2/2`。TRX 和 console logs 在 [tests](tests/) 下。
- `python scripts/validate-source.py` 与 `git diff --check` 通过，记录在 [validation](validation/)；WPF 静态审查 `0 errors / 26 warnings / 177 info`。未运行全量 WPF 独立进程循环或 `GameSaveCenter-Run.cmd` 安装门禁。

## 边界

此次构建和测试使用 SDK `9.0.302`，不能替代 SDK `8.0.423` 基线。未运行真实 Playnite 宿主或浅/深主题渲染，因此不声称宿主呈现已经验证。

用户提供的旧诊断 `20261018T012631286Z-e027df9d` 是测试开始后发生的 xUnit 断言失败（期望 `2,2,2,2`、实际 `1,1,1,1`），不是编译错误；其 summary、TRX 和 DLL/PDB 身份均未保留，无法确认旧运行读到的测试源码和程序集。当前提交匹配构建的 R09 `2/2` 通过，但不能反推旧失败输入。此阶段没有改焦点描边模板。

## 2026-10-21 SDK8 当前源码复核

SDK `8.0.423` Release solution build 0 warning / 0 error、XAML 25/25；通知归并/筛选/已读/重试来源与 R09 定向类共 19/19，Worker 通知持久化 12/12。新 TRX：[SDK8 复核](../SDK8-S02-RECHECK-20261021/README.md)。复核不解释旧 R09 诊断的缺失输入身份，也不替代真实 Playnite 宿主验收。
