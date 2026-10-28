# U09-05 — 冻结候选完整构建、安装与启动

- 源码提交：`5f0eb6209ccfb33c67efac36aee68aee204de614`；SDK `8.0.423`；Release；两次运行启动时跟踪文件干净。
- 用户提到的 `run.cmd` / “一键构建运行” 映射到实际入口：`GameSaveCenter-一键构建安装运行.cmd` → `GameSaveCenter-Run.cmd` → `scripts/dev-install-run.ps1 -Configuration Release`。该根链退出码为 `0`，发布、打包、安装身份核对后启动 Playnite。
- 根链诊断 run `20261028T210324425Z-f531a673`：157 份 TRX，2012 项、1954 通过、0 失败、58 跳过；所有 TRX 计数一致，无错误、超时或中止；WPF 154 个隔离类完成。分组汇总见[本地运行与安装身份](local-runs-and-install-identity.json)。
- 另行独立执行 `scripts/build.cmd`（Release，隔离输出、测试临时目录与诊断根均位于本批 `.tmp`）：退出码 `0`，同为 2012 项、1954 通过、0 失败、58 跳过，154 个 WPF 隔离类完成。该入口按职责完成构建与测试，不替代上面的发布安装启动根链。
- 两次运行的 58 个 skip 均有显式原因：57 个属于已撤销的“今日工作台”测试断言，1 个是默认 opt-in 的系统指针 Tooltip 用例。没有将 skip 计为通过；本次没有命名管道沙箱 skip。见[skip 分类](skip-breakdown.json)。
- 根链输出包 `GameSaveCenter-0.6.73-playnite.zip` 与 `GameSaveCenter-0.6.73.pext` 均为 45,736,111 字节，SHA-256 `F80780FE18D890610C10B881D1B21D7D9967CE7EFFEBF265B3EDF3DB4D514DBD`。最新构建目录、诊断目录、包及安装报告保留在仓库 `artifacts/`。
- 已安装扩展身份为 `0.6.73+5f0eb6209ccfb33c67efac36aee68aee204de614`。主插件 DLL SHA-256 `220BE2409571A14902F1350BBD48655950EF974C94E857D1C7CE5E1EB0859F02`；包内/安装目录的共享程序集身份见上方 JSON 与脱敏安装报告。Playnite 主进程 PID `42296` 启动于 `2026-10-29 05:27:32 +08:00`。
- 根链与 `build.cmd` 的314份原始 TRX 已汇入 [TRX ZIP](tests-root-and-build-cmd.trx.zip)，逐文件 SHA-256 在 [TRX manifest](trx-manifest.json)；命令输出已脱敏存档：[根链日志](root-chain-console-redacted.log)、[build.cmd 日志](build-cmd-console-redacted.log)。冻结前输入记录与真实退出码文件也保留在本目录。
- 通过本机 Playnite 进程启动验证不等于视觉、外部 UIA、实体键鼠、IME 或物理 DPI 验收；这些边界依照各条进度记录保留。真实存档恢复、媒体删除及云端写入没有执行。

本地归档校验：ZIP 含314项，manifest 含314项，压缩包内每个条目的 SHA-256 与 manifest 一致；未发现仓库绝对路径或用户目录。

## GitHub 精确 SHA 门禁

`windows-build.yml` push run [40768381673](https://github.com/Nikilua/GameSaveCenter/actions/runs/40768381673)，run #1374，head SHA 与上述源码提交一致，最终结论 `success`。该 workflow 的两个适用 job 全部成功：baseline `编译、测试与打包` job `123098598311`（SDK 8.0.423，编译/测试、Render QA、WPF 静态审查、扩展包、上传及诊断归档均通过）；SDK 10.0.401 job `123098597735` 及诊断归档通过。没有失败、取消、排队或缺失 job。

精确 run 的三份 artifact：Windows preview `12855272229`（90,786,760 bytes，SHA-256 `a6e2799a1dd287aac891fb5498e40a03c35aa5f83dc365bfbd5ef6f883d3f279`）；baseline diagnostics `12854783673`（2,019,551 bytes，SHA-256 `de772a593f11f7e513954382f39cc45ec08ee9fb3ef257923ebcd9ffae4aaede`）；SDK10 diagnostics `12852732983`（964,854 bytes，SHA-256 `4947940bb304982686397c66eb243a5a3f49c21383998edc064eb2a3c984a28a`）。完整 job、artifact、expiry 与下一步记录见[GitHub CI 身份](github-ci.json)。

required-check 名称/分支保护仍标“远端待验”：先前对应读取权限返回403，依用户指示没有重试；两个 workflow job 的绿色结果不替代 required-check 列表核对。下一步由有权限的仓库维护者或授权 GitHub 连接读取 `main` 的 required status checks，并与上述两个 workflow job 对照。
