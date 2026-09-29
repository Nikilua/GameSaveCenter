# R08-01 反向动画当前 main 复测（2026-09-30）

本机在生产源码 `dc7f97cfa49724778c4987224c9f736c744b3631` 对应的 Release 二进制上复跑用户提及的 `R08MotionReverseBehaviorTests`。HEAD 当时为 `20c2393984a1fd30306fd2fba10f06bdd9207ab3`，生产文件自源码身份后只有文档变化。

## 结果

- VSTest/xUnit `2/2` 通过，失败 `0`、跳过 `0`、exit code `0`；TRX：[R08MotionReverseBehaviorTests.trx](R08MotionReverseBehaviorTests.trx)。本次未复现用户先前报告的 `1/2` 失败。
- TRX 在成功结果后有一条 `TextServicesHost.OnUnregisterTextStore` `InvalidComObjectException` 退出清理记录。根因未知；它未改变测试汇总或退出代码，不归因为产品动画。
- 用户给出的失败摘要缺少失败方法、断言、堆栈及对应 DLL 身份。故此复测只表明当前本机身份未复现；失败机器原始 log/TRX 仍是定位该报告的必要证据。

## 二进制身份

- 测试 DLL：`ProductVersion=0.6.73+dc7f97cfa49724778c4987224c9f736c744b3631`，SHA-256 `1E5A4D29C33327BDBEB3A340986CC04551D602DAC7B05937A8A3AAF07AB36745`，MVID `1ef3680f-fcc8-4eff-81e9-7ae091df7e9f`。
- 插件 DLL：`ProductVersion=0.6.73+unknown`，SHA-256 `854E1549BA35626BBBFD19C82F9596CC5B6AC4320A72D28941CEDBD919BC9EC5`，MVID `3ad2d781-4904-4ff1-bfc2-5cf394ffdafb`。
- 命令：`dotnet test tests/GameSaveCenter.Playnite.Tests/GameSaveCenter.Playnite.Tests.csproj -c Release -f net472 --no-build --no-restore -m:1 /nodeReuse:false --filter FullyQualifiedName~R08MotionReverseBehaviorTests`。

## 验收边界

这是当前 checkout 上隔离 WPF 测试的复测，不是用户失败机器或真实 Playnite 宿主的复现。没有生产源码修改，没有 OS 输入、物理 DPI 或呈现帧验证。保持 R08-01 用户侧失败未定位状态。