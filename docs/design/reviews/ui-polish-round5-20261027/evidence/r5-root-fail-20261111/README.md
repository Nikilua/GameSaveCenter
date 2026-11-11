# 沙箱拒绝安装的完整根入口尝试（2026-11-11）

冻结提交 `266bae3396b9b4d08cdc1978bfc82c7e7cd776c5` 的第一次中文根入口完整运行使用 SDK 8.0.423。编译、2,035项测试、Worker 发布和扩展包均成功；最终在复制扩展到 `%APPDATA%/Playnite/Extensions` 时因当前沙箱拒绝写入而 exit 1，未到 Playnite 启动阶段。详细原因、测试数及包信息见 [`root-chain-failure.json`](root-chain-failure.json)，原始中文入口日志是 [`one-click-install.log`](one-click-install.log)。

原始build console、SDK信息、安装前探测与160份TRX均完整保存在 [`diagnostics.zip`](diagnostics.zip)。`diagnostics-sha256.json` 提供压缩包内641个文件的逐文件路径、长度和SHA-256；已逐项与压缩包条目比对。展开副本核验后删除，以免在仓库保留重复的几千个文本文件。成功的受控权限完整重跑见 [`../r5-root-ok-20261111/README.md`](../r5-root-ok-20261111/README.md)。
