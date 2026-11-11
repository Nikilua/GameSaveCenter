# 冻结提交根入口安装启动复核（2026-11-11）

`GameSaveCenter-一键构建安装运行.cmd` 转交 `GameSaveCenter-Run.cmd`，对提交 `266bae3396b9b4d08cdc1978bfc82c7e7cd776c5` 执行完整 Release 构建、测试、Worker 发布、扩展打包、安装身份校验和启动，最终根入口退出码 0。SDK 为 8.0.423；2,035 项测试中 1,977 通过、58 个 skip、0 失败/错误/超时/中止；157 个 WPF 类隔离执行完成。58 项保持skip且不计通过：57项是已撤销“今日工作台”架构断言，1项是默认关闭的系统指针测试。TRX、诊断、原始入口日志、安装报告在本目录。独立 `scripts/build.cmd` 已在 U08 源码批次运行通过。

安装到 `%APPDATA%/Playnite/Extensions/GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec`，清单版本 0.6.73，程序集身份为 `0.6.73+266bae3396b9b4d08cdc1978bfc82c7e7cd776c5`；zip 与 pext 完全同 SHA-256。安装文件哈希和包哈希见 `root-chain-success.json`。

Playnite Desktop PID 2092 与 Worker PID 40784 存在且 responding；脚本完成启动调用。不过采样时 `MainWindowHandle=0`，按PID枚举也没有发现Playnite顶层窗口，所以没有据此宣称可见窗口、真实宿主渲染或截图中的裁切视觉已验收。UIA/Narrator、实体键盘/IME、物理DPI也仍需实际交互确认。未执行真实存档恢复、媒体删除或云端写入。

完整原始构建/测试诊断与160份TRX保存在 [`diagnostics.zip`](diagnostics.zip)；`diagnostics-sha256.json` 对应压缩包内每个条目，并已逐条比对长度与SHA-256。入口日志和安装身份报告为本目录单独文件。顶层清单见 `sha256-manifest.json`。

GitHub `windows-build.yml` run #1536 对同一冻结提交 completed/success，基线和SDK10作业及artifact身份见 [`../U08-01-03-responsive-20261111/ci-run-42967026905.json`](../U08-01-03-responsive-20261111/ci-run-42967026905.json)。

此前同一冻结提交的一次沙箱安装拒绝保存在 `../root-chain-denied-20261111T054504865Z/`，该次编译、测试、发布、打包均成功，唯安装目录写入被拒绝；受控提升权限后本目录所记完整入口 exit 0。SHA-256 清单见本目录。
