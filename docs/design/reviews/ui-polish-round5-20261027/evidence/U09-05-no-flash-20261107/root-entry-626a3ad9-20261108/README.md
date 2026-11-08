# Round5 U09-05：中文根入口复核与闪窗反馈

## 本机完整入口

- 冻结源码：`626a3ad90cd5b0cf72d1fbe9d8d6f566e0744ddf`；`src/`、`tests/`、`scripts/` 与此前完整构建的 `92bd9de13bd40c13806fb0cba8900ada2e379fc2` 相同。工作树还存在其他任务的未跟踪 TRX/证据，未纳入提交或清理。
- 实际中文入口：`GameSaveCenter-一键构建安装运行.cmd` → `GameSaveCenter-Run.cmd` → `scripts/dev-install-run.ps1`。运行时间 2026-11-08 11:58:26Z–12:27:40Z；SDK 8.0.423、Release；诊断目录 `artifacts/build-diagnostics/20261108T115825877Z-f3fa5041/`，结论 `passed`。
- 160 份 TRX：2030 项、1972 通过、0 失败、0 error/timeout/abort、58 skip，157 个 WPF 隔离类；计数无误差。58 个 skip 为 57 条已撤销“今日工作台”架构断言与 1 条默认不移动系统指针的交互用例，均不计为通过。此前第一次根链有 68 skip，原因是当前执行环境限制 Named Pipe；该失败尝试保留在对应诊断目录，不当作通过。
- 插件 ZIP 与 PEXT 均为 45,739,069 bytes，SHA-256 `14C025D4921F1B6FA31272E8058CFC530DD953F1FE7C0B28EABBDAC7467A5383`。安装清单/DLL 版本 `0.6.73` / `0.6.73.0`，构建身份 `0.6.73+626a3ad90cd5b0cf72d1fbe9d8d6f566e0744ddf`。六个已安装程序集逐一具备同一构建身份，SHA-256 见本机 `artifacts/last-dev-install.txt`。
- 根入口 stdout 到达 `[OK] Build, verification, installation and Playnite startup completed.`；对应 `GameSaveCenter-Run.cmd` 成功分支随后执行 `exit /b 0`，中文包装器转交 `%ERRORLEVEL%`。但是外部验收脚本自身因共享重定向文件锁报错，记录 `RunnerExit=125`，没有成功捕获被启动根命令的最终进程退出码。因此“根入口源码成功分支返回 0”有日志和源码依据，**外层观察到的最终进程码不是 0；严格 exit-code 门禁仍待可靠捕获**。

## 闪窗边界与用户反馈

- `WinSta0\Default` 监测从 `2026-11-08T19:58:19.833+08:00` 持续到 `20:27:54.435+08:00`，起止 input desktop 均为 `Default`，被筛选目标窗口事件为 0；但初始枚举省略了 270 个其它窗口，过滤列表不是完整进程集，不能据此宣称全桌面无闪。
- 根入口在安装前关闭 Playnite/其自有 Worker，安装后普通启动 Playnite。该行为会重启宿主，是明确的可见界面变化候选；仍没有证据将用户截图上的黑窗与具体 HWND/PID/class 联系起来。Playnite 在本次验收使用非输入桌面；Playnite 页面未作视觉验收。
- 私有桌面关闭复核曾向 Playnite PID `35464` 的可见窗口发送 `WM_CLOSE`，30 秒后进程仍在；第二次检查窗口数为 0，仍未强杀。当前 `Playnite.DesktopApp` PID `35464` 与 Worker PID `36964` 在本机进程快照仍存活、没有主窗口句柄。不得对它们做额外强杀。
- 用户再次指出截图中仍有黑窗。随后只读 CUA 应用清单未列出可见 Playnite 窗口；这是当前时点快照，不覆盖瞬时帧。U09-05 继续“进行中”，本记录不声称闪窗已修复。
- 后续小批构建只用独立 `scripts/build.cmd`/定向隔离验证，不重复启动会关闭并重开宿主的完整入口。完整入口不得再作为每批编译命令使用。

## 可追溯文件

- 根入口输出日志（本机临时审计副本）：SHA-256 `334ADB2220AEE493A4B91BF224EB4F38B7A532C758C9671416E4E53D00299F23`。
- 外层验收结果：SHA-256 `C27E1A494B7828E16F464C0FE86EB5A9256958BABB8B5F0DC84F5F2BF05D532C`；原始异常为向同一 runner 输出重定向写入时文件被占用。
- 过滤窗口监测日志：SHA-256 `283815A197E767E05F31E6D4CF9BA641779CC07E1A09B320CA552715F850FFC2`。
- 安装报告 SHA-256 `9F1D49A6BEE75179E6A74F055F7334BDFC3F5E1AE80892768D2209D035F9477A`；安装日志 SHA-256 `49DC7DE5CA9ADEEA5C99454BE03779D6C658BC09EC1B0A925F7926A3084B0A11`。

## GitHub CI

精确提交 `626a3ad90cd5b0cf72d1fbe9d8d6f566e0744ddf` 的 workflow run [#1491](https://github.com/Nikilua/GameSaveCenter/actions/runs/42467986641) 于 2026-11-08 completed/success。基线 job `128691845992` 15/15 步、SDK10.0.401 job `128691846130` 8/8 步均成功，含源码检查、编译/测试、Render QA、WPF 静态审查、打包与诊断上传。artifact ID、大小与 digest 见 [`ci-run-1491.json`](ci-run-1491.json)。required-checks/ruleset 仍因既有 403 无法核对，U09-06 保持“远端待验”。

没有操作盘符、卷、映射、存档、媒体或云端数据。
