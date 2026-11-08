# U09-05 中文根入口退出码与闪屏复核

冻结提交：e42da2a5bd33e8988acb0b31218bdd66b3d96048。该提交相对生产源码候选 626a3ad90cd5b0cf72d1fbe9d8d6f566e0744ddf 只有文档变化；本机记录的源码/脚本未变。

## 完整链结果

中文 GameSaveCenter-一键构建安装运行.cmd 经 GameSaveCenter-Run.cmd 执行 Release 安装运行链，SDK 8.0.423。第一次外层进程实际退出 1：构建、全测和打包已过，工作区沙箱阻止了 Playnite 扩展 AppData 安装写入。第二次在同一源码 SHA 上重跑完整构建、测试、Worker 发布、打包、安装身份核验和启动，根 cmd.exe 的直接退出码为 **0**；stdout/stderr 未重定向。第一次失败日志与摘要仍保留在本机 .tmp/round5-u09-05-root-exit-20261108/failure-1/，对应的根诊断也保留于 artifacts/build-diagnostics/。

160 份 TRX 共 2,030 项：1,972 通过、0 失败、0 error/timeout/abort、58 个既有 skip；157 个 WPF 隔离类执行完成。58 个 skip 保留原因：57 条针对已撤销的“今日工作台”架构断言，1 条系统指针测试默认 opt-in；skip 不算通过。计数、逐文件原始/脱敏 SHA 及脱敏 TRX 包见测试清单与 tests-redacted.zip。原始诊断目录为本机 artifacts/build-diagnostics/20261108T151452539Z-a56b3d8a/；诊断摘要哈希记录在清单中。已脱敏的入口日志见 one-click-redacted.log。

## 安装与启动

ZIP 和 PEXT 各 45,739,040 bytes，SHA-256 9A99CCC6293D88D1D7F24384FFFA4B7E88BA00DCB98033F0A32DE1C82641ADDD。扩展清单版本 0.6.73、DLL 版本 0.6.73.0；实装六个 Plugin/Worker/Shared 程序集均核对为 0.6.73+e42da2a5bd33e8988acb0b31218bdd66b3d96048，逐项 SHA-256 见安装身份。

## 闪屏边界

根进程运行在 WinSta0 的非输入私有桌面 GscRootExitRetry_6fae9270ee654214bacc2368bd247805；开始和结束时 input desktop 都是 Default。该轮 Playnite 窗口被枚举到私有桌面，默认桌面在对应快照中没有本次 Playnite/Worker 窗口。用户这次附图的本地文件元数据时间为 2026-11-06 13:59:27 +08:00，且图像没有 HWND、PID 或所属桌面；它不能定位 11 月 8 日根链产生的窗口。当前闪屏审计是一次性桌面快照，不是整个构建时段的事件监听，也不能证明间歇闪屏不存在。

脚本核对显示，独立 scripts/build.cmd 只编译/测试，不启动 Playnite；中文完整入口会在安装后关闭/启动 Playnite。后续小批编译继续用 scripts/build.cmd，完整宿主验收仍须使用中文根入口，但只能在非输入私有桌面运行以避免打扰用户桌面。**黑窗的真实来源和“用户桌面不闪屏”仍未验收；U09-05 保持进行中。** 未做存档恢复、媒体删除、云端写入；未操作卷、盘符或映射。

精确SHA的 GitHub `windows-build.yml` run #1492 两个适用 SDK job 均成功；run/job/artifact 身份及 digest 见 [CI 记录](ci-run-1492.json)。required-checks/ruleset读取此前403，U09-06仍远端待验。

本批只删除一个未成功启动的一次性关闭脚本；根链结果和首轮安装失败诊断保留，前后空间与精确删除清单见 [清理记录](cleanup.json)。


根链启动的 Playnite/Worker 在私有桌面；随后对精确 Playnite 主窗口发出一次正常 WM_CLOSE。Playnite 在20秒、Worker在10秒等待后仍存活，因此未强制结束。当前桌面边界和正常关闭结果见 [关闭请求记录](shutdown-attempt.json)；这不改变默认桌面窗口快照与闪屏未归因结论。
