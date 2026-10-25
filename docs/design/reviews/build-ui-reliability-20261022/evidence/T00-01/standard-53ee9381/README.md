# 53ee9381 标准根入口完整链

源码身份 53ee9381d23772a60ebaf7ff16aeb81d6584d28a 的默认命令 cmd.exe /d /c "GameSaveCenter-Run.cmd < nul" 退出码为 0。构建 run 20261025T091123674Z-d1fdc06a，安装 run 20261025T091121578Z-c93b09da。完整链期间跟踪文件保持不变；运行前已有的用户未跟踪 Round3 TRX 与 tests/GameSaveCenter.Playnite.Tests/TestResults/ 保留。

SDK 8.0.423、Release solution build 0 警告/0 错误，155 个构建步骤无失败。Core 157/157、Worker 400/400；Playnite source 562 项（544 通过、18 skip）；WPF 隔离测试 849 项（810 通过、39 skip），146/146 个隔离类全部结束。总计 1968 项、1911 通过、57 skip、0 失败/错误/超时/中止；149 份 TRX 与汇总计数一致、无解析或计数差异。57 个 skip 的共同理由是已撤销的“今日工作台 UI 架构”断言。

Worker win-x64 发布成功。ZIP 与 PEXT SHA-256 均为 1C777A613BE72D87B232D8216329AAC753BF3F5DF41516DD90EBB0A682033C48。安装清单版本 0.6.73、DLL 版本 0.6.73.0；六个已安装程序集版本均为 0.6.73+53ee9381d23772a60ebaf7ff16aeb81d6584d28a，SHA 见 [install-identities.txt](install-identities.txt)。Playnite 在安装后启动，PID 50020。本记录不声称已核验该进程实际加载的插件程序集路径或完整真实页面/物理输入矩阵。

完整诊断和149份原始TRX保留在本机 artifacts/build-diagnostics/20261025T091123674Z-d1fdc06a/；当前隔离构建输出在 artifacts/gsc-b/44f15e66/，安装报告在 artifacts/last-dev-install.txt。包保留在 artifacts/GameSaveCenter-0.6.73-playnite.zip 和 artifacts/GameSaveCenter-0.6.73.pext。

本机 Q06 双主题按钮状态及 R08 动效反向类均通过。较早外机附件 run 20261025T022508878Z-409c39f7 在 Q06 Dark hover 因 hoverOverlay=0 失败，且未进入打包；当时终端记录对应的 checkout 更新到 d6a401af。此历史结果不代表当前 53ee9381。用户外机仍须保留其 stash 并在同步到最新主线后独立运行完整入口。

完整链之后删除本任务五个旧 .tmp 夹具目录，1456 个文件、324453441 逻辑字节；删除前后空间观察与保留项见 [cleanup.json](cleanup.json)。证据文件自身 SHA-256 见 [SHA256SUMS.txt](SHA256SUMS.txt)。
