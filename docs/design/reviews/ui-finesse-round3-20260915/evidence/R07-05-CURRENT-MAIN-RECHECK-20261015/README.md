# R07-05 当前 main 复核（2026-10-15）

源码身份：`aa49a5698492faf847cd4385e1647f6eb1b01683`（main）。本次复核必要：此前 `ec9b0a21` 之后，详情断点协调器、TaskCenter 视图及其行为测试有变化；本轮 R07-04 还在共享 DataGrid 样式接入滚轮行为。本次没有改 R07-05 生产布局或测试源码。

## 构建与行为

- 本机 SDK：`9.0.302`；VSTest：`17.14.1`。仓库 `global.json` 固定 SDK `8.0.423`，本机未安装。SDK9 结果仅为本机证据，不替代 SDK8/SDK10 CI。
- 从隔离输出根目录构建 Playnite 测试项目及引用项目 Release：`0 warning / 0 error`；Playnite 仍目标 `net462`，测试目标 `net472`。构建程序集 ProductVersion 为 `0.6.73+aa49a5698492faf847cd4385e1647f6eb1b01683`；只保留源码证据、脱敏 TRX 与程序集哈希，不保留临时 bin/obj。
- 本项及相邻回归 `18/18`：`ResponsiveLayoutCoordinatorTests 5/5`、`R07DetailsBreakpointBehaviorTests 1/1`、`R07ScrollOwnershipBehaviorTests 2/2`、`TaskCenterViewResponsiveTests 8/8`、`R06DetailsBudgetBehaviorTests 2/2`。完整结果见[脱敏 TRX](tests/R07-05-current-main-sdk9302.trx)，SHA-256 `1D3F92E2EC18C4F16BD2006CB554FE7B071BC6533B08C5527EAA1B7D7DC6DEF5`。
- 详情断点行为在生产 `TaskCenterView`、合成失败 DTO 和隔离 STA WPF Window 中按 `1000→979→981→971→979→988 DIP` 驱动，验证选择对象、键盘焦点和垂直 offset 保留，紧凑态详情可见；重复落在内容预算带内不切换，低于 `972 DIP` 进入紧凑态，到 `988 DIP` 返回宽态。Coordinator 单测另核对重复宽度的幂等行为及两个状态转换。
- 测试 DLL SHA-256：`C687F086D3F95041734C5DDE40169E2E3B888FD91F490A0EA93C4157B2915A48`；插件 DLL SHA-256：`864373C808588527491C4C2B699D66DA4195993DFD7154EF836013A910123C9C`。

## 验收边界

本次 WPF testhost 收尾时打印一次 `TextServicesContext.InvalidComObjectException`，但 xUnit `18/18` 通过、VSTest exit `0`；根因未知。测试只证明隔离 STA WPF 中的逻辑 DIP 行为，不代表真实 Playnite shell 窗口拖动事件链、物理 DPI/跨屏、设备输入、UIA/读屏、最终呈现帧、ETW 或宿主性能。未接触真实存档、媒体、云端或诊断数据；命令、游戏选框、滚动条系统、有限列表与 net462 契约均未更改。

下一项：R07-06 状态横幅预算当前 main 复核；先检查当前断言和共享控件影响，再跑本项及相邻空态/详情/滚动回归。
