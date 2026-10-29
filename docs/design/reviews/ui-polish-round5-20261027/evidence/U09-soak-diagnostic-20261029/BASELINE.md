# U09-05 Worker soak访问拒绝诊断

2c4ad8c31dd4329fb26f75fb6ed0f4a05692ddab在SDK8.0.423冻结中文根链中退出1。Core157通过，Worker400通过/1失败；SoakDataScaleTests.DataScaleSoakRemainsBounded的File.Move抛UnauthorizedAccessException。临时目录为本次构建artifacts/gsc-b/241164e5/test-temp，夹具Guid目录已被原Dispose清理，异常不含具体文件名。未进入Playnite测试、发布打包安装或独立build.cmd，不能计完整通过。

使用同一已编译DLL、同一隔离临时目录做一次定向复现：1/1通过、0 skip，原失败未复现，未查明唯一原因。仅在测试夹具补异常Data：实际路径、cycle、第几次写入、失败时文件属性；测试输出HRESULT及Data后原样throw。属性读取异常只作为诊断文本，不覆盖原异常。未重试写入、未改权限、生产原子写入、断言、超时、SDK或skip。

补诊断后Release重编译及完整Worker运行退出0：400 pass/1 skip/0 fail。既有动态WorkerProcessFact因当前受限环境不允许创建Named Pipe而跳过真实进程重启测试，不能计通过；冻结完整入口仍须在获授权环境执行该测试。本轮并发运行未复现soak拒绝，定向与局部通过不代表根因修复，也不替代新候选完整链及双SDK CI。

保留原失败构建、诊断及全部TRX，无新包/截图，未删除未知输出或反复探测旧权限拒绝。真实Playnite页面、物理输入/DPI与required-check既有403边界保留。
