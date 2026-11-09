# U09-05 e570f7b7 完整独立构建与 CI

## 结果

- 精确源码提交 `e570f7b7e0d5019b7f57c411237de3abb55a6e89` 使用 SDK 8.0.423 执行 `scripts/build.cmd -Configuration Release`，退出码 0。完整日志、160 份 TRX 和汇总位于本机 `artifacts/build-diagnostics/round5-u09-05-detached-e570f7b7/20261109T085813954Z-7831612a/`；编译输出位于 `artifacts/gsc-b/e570f7b7-detached-full-20261109/`。机器可读摘要见 [full-build.json](full-build.json)。
- 测试汇总为 2,032 项、1,964 通过、0 失败，68 项既有 skip；157 个 WPF 隔离类全部执行，TRX 计数没有不一致。skip 原因分别是已撤销“今日工作台”断言 57 项、执行环境不允许的 Named Pipe 客户端 9 项及服务端 1 项、默认不运行的系统指针交互测试 1 项。skip 均未计为通过。
- 同一时段 WinEvent 监听 `WinSta0\Default`（也是 input desktop）记录 show=0、hide=0。该证据只覆盖此编译/检查时段，不能归因用户此前看到的黑窗，也不能覆盖最终 Playnite 启动路径。
- 精确提交的 GitHub run [#1500](https://github.com/Nikilua/GameSaveCenter/actions/runs/42579208247) 已完成：SDK 10.0.401 job `129033708730` 与基线 job `129033708852` 均成功。包、诊断 artifact ID、大小和 digest 见 [ci-run-1500.json](ci-run-1500.json)。required-check 配置此前返回 403，故完整性保持“远端待验”。

## 尚待最终入口

根入口安装启动预检要求 C 盘至少空闲 4 GiB。本次结束后测得 C 盘空闲 `3238178816` bytes，仍差 `1056788480` bytes；D 盘空闲 `19902840832` bytes。经项目专属临时目录检查，`%TEMP%\GameSaveCenter.Tests` 当前为空，未发现可安全清理且足以补足差额的本项目输出。当前 e570 构建输出和诊断仍用于交付审计，因此保留。旧 Playnite PID `24248` 和 Worker PID `42704` 均已不存在，当前进程查询也未发现这两个宿主。

空间门禁未满足，所以本阶段没有执行中文包装器转交英文 `GameSaveCenter-Run.cmd` 的安装、身份核验和启动。没有降低门槛、修改 SDK、动盘符/卷/映射或清理来源不明的数据。
