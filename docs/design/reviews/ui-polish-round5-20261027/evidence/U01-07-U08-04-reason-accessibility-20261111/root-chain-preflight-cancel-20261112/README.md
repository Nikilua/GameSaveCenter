# 根入口预检中止记录（2026-11-12）

冻结源码 d1cc9dc5fc0d8fef32f0e89e91e3bc87924ad2ef，SDK 8.0.423。首次非提升权限运行因本机沙箱限制，若继续会在向 Playnite %APPDATA% 安装目录写入时被拒绝；为避免重试同一权限路径，人工在 WPF 隔离测试阶段停止了这次预检。诊断摘要记录87个已完成WPF类，但整轮没有完成，因此它不计为通过。随后同一冻结源码经受控权限完成中文根入口全链，最终cmd退出码0，见[../root-chain-success-20261112/README.md](../root-chain-success-20261112/README.md)。

本次未完成诊断目录共362个文件、3,637,075 bytes；原始诊断与已完成测试/TRX保存在[diagnostics.zip](diagnostics.zip)，逐文件核验、计数与ZIP SHA见[diagnostics-sha256.json](diagnostics-sha256.json)。运行器和TRX中的用户目录路径已在归档前检查脱敏。
