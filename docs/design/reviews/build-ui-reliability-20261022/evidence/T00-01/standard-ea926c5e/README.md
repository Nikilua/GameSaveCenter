# ea926c5e 标准完整构建验收

- 源码提交：`ea926c5e36305dc4d67659d98d42e207b71e7439`
- 标准入口：仓库默认 `GameSaveCenter-Run.cmd`，退出码 `0`
- 诊断 run：`20261024T124042418Z-170b15d4`
- SDK：`.NET SDK 8.0.423`；PowerShell 7.6.2；Windows x64
- Release solution build：0 warnings / 0 errors
- 测试：1967 total，1910 passed、57 skipped、0 failed；148 TRX 与计数一致，145 个 WPF 隔离类结束
- 跳过：18 个 Playnite source + 39 个 WPF case，原因均为已撤销的“今日工作台 UI 架构”断言。
- 发布与交付：Worker `win-x64` publish、ZIP/PEXT、安装版本/程序集身份和SHA复核均成功；Playnite 启动成功（PID 6268）。
- 本次证明默认完整入口在此源码身份下成功。没有据此声称真实页面交互或进程内插件加载路径通过。

## 原始诊断

完整、未裁剪诊断保存在本机：`artifacts/build-diagnostics/20261024T124042418Z-170b15d4`；隔离 Release 输出：`artifacts/gsc-b/103f4893`。根诊断摘要记录源码身份、SDK、阶段退出码、148份TRX计数与程序集身份。安装报告位于本机 `artifacts/last-dev-install.txt`。这些可再生的大型输出不提交到 Git。

`verification.json` 是此 run 的小型摘要。`install-identities.txt` 记录六个安装程序集SHA和完整构建身份；磁盘身份核对不等于确认运行进程实际加载了这些路径。

## 外机失败对照

另一台个人电脑的最新日志在 `GamePickerViewModel.cs` 多处出现 `CS8300`，编译器提示遇到 merge conflict marker。当前提交的同名文件无冲突标记，且上面的标准根入口构建成功。外机日志没有记录其 HEAD、dirty paths 或冲突文件 diff，故不能确认冲突由未完成的 merge/rebase、携带的本地改动还是其他输入引入；先备份/检查外机工作区，再处理冲突。
