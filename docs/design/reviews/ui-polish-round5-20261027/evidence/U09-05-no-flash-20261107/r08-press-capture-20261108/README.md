# R08 按压动画测试采样修正

## 改动范围

只改了 `R08MotionReverseBehaviorTests` 的测试夹具。测试克隆生产按钮模板中的 Storyboard，在隔离桌面上把 Hover 采样时窗保持为 800 ms、PressedOverlay 采样时窗设为 1600 ms，并将按压探针的有界超时调整到覆盖该时窗。开始探针前等待最长夹具时窗结束。生产 XAML、MotionTokens、命令、业务代码和断言均未改；既有中点、连续反向、最终值、变换身份及浅/深主题兄弟项断言仍在。

## 触发原因与验证

精确提交 `b573fea7210d810eef221721395b007da96d7458` 的本机独立完整 `scripts/build.cmd` 已编译成功，但在第 77/157 个 WPF 类 `R08MotionReverseBehaviorTests` 中出现一次 `ProductionButtonHoverAndPressReverseFromTheirRenderedValues` 按压入口未采到中间值的失败。失败 TRX 保存在 `tests/full-build-b573-pre-fix-failure.trx`；当时完整链已运行 1469 项，1439 通过、1 失败、29 跳过，未计为通过。随后同一旧二进制的单用例 1/1、完整 R08 类 4/4 通过，表明该探针在长批次中存在渲染采样时序敏感性，不能据此把全量失败抹掉。

修正后的当前源码 blob `c9432aab21e1b5559da1582e0add0d02c7fc4baf` 在 SDK 8.0.423 / Release 下通过 `scripts/build.cmd -SkipTests`：XAML 26/26、解决方案 0 warning / 0 error；Source Validation 与 `git diff --check` 通过。目标按压行为在三个独立 `WinSta0` 私有非输入桌面各 1/1 通过，合计 3/3、0 失败、0 跳过。每次输入桌面均为 `Default`，前后未改变。编译测试程序集 SHA-256：`BF037240E27CF8631C74A0C621EFFA061EAD679B578F783EC244D888EBD2C4B0`。六份脱敏 TRX 已随本目录保存。

UI 生产资源没有变化，因此本批不运行 Render QA 或 WPF 静态资源审查；行为测试直接加载真实 `GscWpfUiPrimaryButton` 生产模板。完整独立构建、中文入口安装启动链须在修正提交冻结后重跑。

## 闪屏边界

上一精确提交 `b573fea7210d810eef221721395b007da96d7458` 的测试运行期间，对 `WinSta0\Default` 做了连续 600 秒 `EVENT_OBJECT_SHOW/HIDE` 顶层窗口监听，事件数为 0。覆盖了最后一批私有桌面 WPF 类，但不覆盖此前全程，也不验证手动 Explorer 启动或真实 Playnite 页面。用户截图文件时间为 2026-11-06 13:59:27，没有 HWND/PID/桌面身份；本次结果不据此结项。U09-05 仍为“进行中”，需继续验证实际启动入口及新反馈窗口身份。

## CI 身份

`b573fea` 的 Windows workflow run [#1489 / 42349851252](https://github.com/Nikilua/GameSaveCenter/actions/runs/42349851252) 已完成成功：SDK 基线 job `128335451147`、SDK 10.0.401 job `128335451280`。产物：预览包 `13566987938` / 90,793,200 bytes / SHA-256 `45416e0b3c70196dea228bf554c6d368b85d43108c50f77df9c8a98ddea91608`；SDK 基线诊断 `13566533664` / 2,523,023 bytes / SHA-256 `d4a5347b42b7d4f1d06dd1b2e014fffb13b480dbfe5ca79e187e5c2ff5f218d7`；SDK10诊断 `13565694229` / 1,278,950 bytes / SHA-256 `014b1952b42ede7868442189a2260d6fdeb16ab76b21855b4807543706907753`。该 CI 验证的是 `b573fea`，不代表当前修正提交已通过。分支 required-checks/ruleset 读取仍受此前 403 限制，继续记“远端待验”。

本阶段已归档 TRX 后，只清理两个本阶段自有临时复测目录；路径边界、嵌套链接、占用进程及前后空间见 [清理清单](cleanup.json)。保留修正源码的 Release 输出和原始完整链失败诊断。
