# R09 外机厚度失败当前 main 复核

日期：2026-10-15

## 当前 main 复跑

- 源码提交 `91bc88ad2f865c760469c8ad68fc6bf061c4a97b`，生产代码基线 `88b8eb57`；本机 SDK `9.0.302` / VSTest `17.14.1`，Playnite `net462`、测试 `net472`，testhost `.NET Framework 4.8.9181.0`。本机缺仓库固定的 SDK8.0.423；此次从当前源码隔离构建测试项目及引用项目，非完整 solution build。
- `R09FocusOutlineBehaviorTests` 在该输出中 `2 passed / 0 failed / 0 skipped`。脱敏 TRX：[R09-focus-outline-sdk9302.trx](tests/R09-focus-outline-sdk9302.trx)，SHA-256 `347CE37C55441002F87BC1052E8780C55DFD6FB19E23485D385697CA7EE8227C`。
- 测试源码 SHA-256 `A5AAB9E90C340F93301D8424FF1E30D98B160D39C1265172AEE7A6B01B2CBC98`；测试程序集 SHA-256 `EF1E9495271366BE76F5E018B98378A836EE80439972B8198A639C65CADD48E3`，PDB SHA-256 `D5CFDAEC660E278BDF2AAD811309F69DFEEB9574D8CC4A2F0262A14F1770F0E`；生产插件程序集 SHA-256 `F0EE376B6B593CE8F73F248A9AEB14677A4CC49C5F20BC31AFE1A18CC2EC3F7B`。

## 外机日志对应关系

- 当前测试源码第 151 行检查错误边框颜色，第 152 行检查 Chrome 为 `1 DIP`，第 154 行检查外置 `ValidationStroke` 为 `2 DIP`。直接读取历史提交 `3f7d0b30` 后确认，其第 151 行是旧的 Chrome `2 DIP` 厚度断言。用户日志的“第 151 行、期望 2、实际 1”与该旧断言相符。
- 这证明外机失败记录所映射的断言与当前源码行不同；但外机实际 HEAD、工作树、DLL、PDB 和构建输出身份未知。旧源码/二进制映射是线索，不是已确认的根因。当前 main 的 SDK9 结果和之前 SDK8 结果也不能替代外机同一次失败的身份信息或 SDK10 CI。
- `libpng iCCP` 警告不在断言调用栈；日志里的 WPF `InvalidComObjectException` 在断言失败后的 TextServices teardown，不能解释厚度比较。没有更改生产模板或弱化断言。

## 下一步

用户方便时，对失败机同一次输出采集 `git rev-parse HEAD`、`git status --short`、测试源文件 SHA-256、测试 DLL 与 PDB SHA-256，以及实际构建/测试命令。对照后再判断是不同源码、复用的旧程序集/PDB，还是其他构建路径问题。
