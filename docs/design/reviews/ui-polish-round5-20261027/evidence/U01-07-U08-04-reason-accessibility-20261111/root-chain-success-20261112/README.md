# 冻结提交根入口验收（2026-11-12）

中文根入口 `GameSaveCenter-一键构建安装运行.cmd` 转交 `GameSaveCenter-Run.cmd`，再调用 `scripts/dev-install-run.ps1 -Configuration Release`。冻结源码为 `d1cc9dc5fc0d8fef32f0e89e91e3bc87924ad2ef`，SDK 8.0.423；根入口的 `cmd.exe` 实际退出码为 0。构建、测试、发布、打包、安装身份核验及启动调用均完成。160 份 TRX 合计 2,036 项：1,978 通过、58 skip、0失败/错误/超时/中止，计数一致；157 个 WPF 隔离类均完成。58个skip原样保留且不计通过：57条是已撤销“今日工作台”架构断言，1条是默认关闭的系统指针测试。

zip/pext均为45,738,863 bytes，SHA-256均为 `4431C10F0918D63F29D655B3DC079178EE9E6F6B127B4D467B95BBD4866697F2`。安装清单版本0.6.73、DLL版本0.6.73.0、程序集身份 `0.6.73+d1cc9dc5fc0d8fef32f0e89e91e3bc87924ad2ef`；逐DLL哈希和安装报告原文件哈希见[`install-identity-sanitized.txt`](install-identity-sanitized.txt)。原入口日志的原文件SHA也在[`root-entry-log-sanitized.txt`](root-entry-log-sanitized.txt)。用户目录路径已从跟踪证据中脱敏。

完整构建诊断与TRX见[`diagnostics.zip`](diagnostics.zip)，逐项大小/SHA及压缩包SHA见[`diagnostics-sha256.json`](diagnostics-sha256.json)。入口摘要见[`../root-gate-20261112.json`](../root-gate-20261112.json)。启动后Playnite和Worker进程存在，但 `MainWindowHandle=0`，所以真实宿主渲染、UIA/Narrator、实体键盘/IME和物理DPI仍待验收；不宣称页面视觉验证完成。没有执行存档恢复、媒体删除或云端写入。
