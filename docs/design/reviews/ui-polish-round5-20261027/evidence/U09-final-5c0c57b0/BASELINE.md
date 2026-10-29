# 5c0c57b0 冻结门禁

SDK8.0.423冻结源码，两入口均退出0：中文包装器转交GameSaveCenter-Run.cmd完整构建/测试/发布/打包/安装/启动，独立scripts/build.cmd按自身职责构建/测试。各158 TRX、2020项，1962通过、0失败、58跳过；57项为撤销架构旧断言，1项为opt-in物理指针测试，跳过不计通过。命令身份、SDK、日志、源诊断和逐份TRX摘要见local-verification.json与files.json。

PEXT为45,737,311 bytes，SHA-256 `bce032ef9d6c95aa145d0c99a69ee3be16ef7d8dda59e8964db10bc2eb076ee6`；六个Playnite/Worker程序集与安装目录逐字节一致。Playnite PID30324于2026-10-29 23:46启动且响应；未证明插件运行时模块SHA或可视页面。

soak曾报一次File.Move访问拒绝，完整诊断补路径/轮次后重新完整根链与Worker均通过；同DLL定向复现亦通过，但未查明原拒绝根因，见[诊断](../U09-soak-diagnostic-20261029/BASELINE.md)。没有更改重试、权限、断言或skip。

GitHub push run [40900374673](https://github.com/Nikilua/GameSaveCenter/actions/runs/40900374673)，head SHA一致、run 1398终态success。SDK8 job123540700192和SDK10.0.401 job123540699717全部步骤success；三个artifact 12914531169、12914426395、12908709759的head SHA、digest及有效期见github-ci.json。已成功的Checks和artifact身份不替代required-check名单读取；branch-protection API既有403未重试。具权限维护者可执行`gh api repos/Nikilua/GameSaveCenter/branches/main/protection/required_status_checks`。

真实Playnite界面仍待验。本轮node_repl/@oai/sky初始化报kernel assets路径不存在(os error 3)，按仓库Computer Use流程未改用其他自动化或重复试探。真实宿主菜单/UIA/物理输入DPI未核；真实恢复、媒体删除和云端写入未执行。
