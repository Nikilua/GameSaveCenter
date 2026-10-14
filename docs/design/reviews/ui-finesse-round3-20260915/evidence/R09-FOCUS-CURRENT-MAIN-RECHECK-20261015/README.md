# R09 焦点轮廓当前 main 对照

更新：2026-10-15；源码基线 `6cf279238d55fe3d139fa2cb0eb7b09229a7478a`；本机 SDK `9.0.302` / VSTest `17.14.1`。

用隔离 Release solution build 生成的同一输出重新运行 `R09FocusOutlineBehaviorTests`：`2/2` 通过。脱敏 TRX：[R09FocusOutlineBehaviorTests-sdk9302-6cf27923.trx](tests/R09FocusOutlineBehaviorTests-sdk9302-6cf27923.trx)，SHA-256 `9E53FDA1DDE8A0627D6BA51E89D289CCB17C36443C8ABEC0BA8433C7EC283D3C`。测试 DLL SHA-256 `A23CD935417BC2C99AF658A3D1350434AD4630D289CDC56ABC7A415DDA968CF4`、PDB SHA-256 `A355D305EA8EBE54BDB3172BEFA78BED030ED953DED81936A55D6F041AD4902B`；生产插件 DLL SHA-256 `DADDC7807254337358D27FD77ADBA99E83DD8C9C7092C97AEDE165DC618E2B3D`、PDB SHA-256 `2618959D11B6D7CEE6528B7B4F8FE9488398B9F1C1895DD0EE3E3908D2A174D6`。

当前源码的第 151 行检查错误边框颜色，第 152 行检查 Chrome `1 DIP`，第 154 行检查独立 `ValidationStroke` `2 DIP`。本次同一隔离输出中测试与生产资源一致，类通过。环境未安装仓库固定的 SDK `8.0.423`，所以这是 SDK9 本机对照，不是 SDK8 基线或 SDK10.0.401 CI 结果。

它仍不能确定用户另一台机器实际运行的 HEAD、工作树、测试源、DLL、PDB 或嵌入资源。用户日志中的旧厚度断言/行号错配仍是可能推断，根因未确认；没有改模板或放宽断言。若之后仍复现，最小诊断材料仍是同一次运行的 `summary.json`、`tests/wpf.R09FocusOutlineBehaviorTests/` console/TRX、`git rev-parse HEAD`、`git status --short` 和实际测试 DLL/PDB、插件 DLL 哈希。
