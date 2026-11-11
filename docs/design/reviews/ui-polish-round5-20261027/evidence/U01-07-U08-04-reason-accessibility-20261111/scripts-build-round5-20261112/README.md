# 独立 scripts/build.cmd 复核（2026-11-12）

冻结跟踪源码 `d1cc9dc5fc0d8fef32f0e89e91e3bc87924ad2ef` 上，以SDK 8.0.423 / Release 独立执行 `scripts/build.cmd`（自定义隔离的 `artifacts/gsc-b/scripts-build-20261112` 输出、测试临时目录与诊断目录），实际 `cmd.exe` 退出码 0。解决方案编译 0 warning / 0 error；166 个诊断步骤全部成功，160份TRX共2,036项：1,968通过、68 skip、0失败/错误/超时/中止，计数一致，157个WPF隔离类全部运行。skip均保留原因为且不计通过：57项已撤销“今日工作台”断言、9项Named Pipe客户端环境限制、1项独立Worker进程环境限制、1项默认关闭的系统鼠标指针测试。

逐步构建命令与结果见[`scripts-build-result.json`](scripts-build-result.json)；完整诊断及TRX见[`diagnostics.zip`](diagnostics.zip)，逐文件哈希和ZIP哈希见[`diagnostics-sha256.json`](diagnostics-sha256.json)。入口日志原始字节SHA保留在结果JSON中，归档文本已规范化为UTF-8并移除用户目录路径，见[`scripts-build-console-sanitized.txt`](scripts-build-console-sanitized.txt)。

此脚本按职责完成编译与测试，不做扩展打包、安装或启动；它与中文根入口 `GameSaveCenter-一键构建安装运行.cmd` 的完整build/test/publish/package/install/start链分别核验。真实宿主视觉与UIA仍待验，未由本脚本宣称。
