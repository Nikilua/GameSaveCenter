# R05-03 当前 main Popup 边界复核

日期：2026-10-15

状态：部分满足。当前生产 Picker/ComboBox 的边界、滚动和所选项可见性行为通过；“翻转无闪屏”仍缺真实呈现帧证据。

## 源码与本机验证

- 分支 `main`，源码提交 `91bc88ad2f865c760469c8ad68fc6bf061c4a97b`，生产代码基线 `88b8eb57`。本阶段未改生产或测试源码。
- 仓库 `global.json` 固定 .NET SDK `8.0.423`，本机没有该版本；通过仓库外工作目录显式选择安装的 SDK `9.0.302`。Playnite 测试项目 Release `net472` 定向构建，生成生产插件 `net462` 与隔离测试程序集；VSTest `17.14.1`，测试宿主 .NET Framework `4.8.9181.0`。这不是 SDK8、SDK10 CI 或完整 solution 的结果。
- `R05PopupBoundaryBehaviorTests` 与 `R05OptionVirtualizationBehaviorTests` 合计 `7 passed / 0 failed / 0 skipped`。脱敏 TRX：[R05-boundary-virtualization-sdk9302.trx](tests/R05-boundary-virtualization-sdk9302.trx)，SHA-256 `A86C24C4819B05BA6820E139925CBEB00532E65CA04F33EDA039F3F51480389F`。
- 证据覆盖 720×360 DIP 生产 Picker 短窗、200 项列表选择末项和滚动可见性，以及生产 ComboBox 在桌面工作区右下边缘打开后的 Popup 边界、120 DIP 最大高度、Auto 滚动和第 100 项可见性。

## 未验证边界

- 测试读取 Popup 打开后的几何，不捕获屏幕呈现序列；不能证明动画翻转无闪屏。真实 Playnite 嵌入、OS UIA/IME/Narrator、物理 DPI/跨屏、ETW 和宿主性能也未验证。
- 先前 bootstrap 阻塞 profile 的 PID/进程树在 2026-10-15 复核时均已不存在。确认该目录位于仓库 `.tmp`、审计 marker 匹配且无重解析点后，清理了精确目录 `.tmp/r05-03-popup-profile-20261013`（递归文件总计 `134,054,425` bytes）。本轮未重跑 `real-host-audit.ps1`，因此宿主呈现边界仍在。
- 下一步继续 Round3 中依赖已满足的独立 Q/R 项；R05-03 保留“部分满足”，待可审阅的真实呈现帧证据后再验收。推送本复核文档后，SDK8 与 SDK10.0.401 Actions 分别追踪到终态。
