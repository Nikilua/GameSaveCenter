# U09-05：选框与长详情像素复核

本批从 `3dc59152fee9e2e7e18872c13cadaee2e1c28dc8` 开始。生产 XAML、主题资源和业务代码没有改动；扩展三类已有选框回归为长列表/窄视口夹具，并加入由环境变量启用的 `RenderTargetBitmap` 捕获，补充任务详情窄栏长文的截图复核。

## 本地构建与行为

- 入口：`scripts/build.cmd -SkipTests -OutputRoot .tmp/round5-frame-boundaries-20261109/build-output -TestTempRoot .tmp/round5-frame-boundaries-20261109/test-temp -DiagnosticsRoot .tmp/round5-frame-boundaries-20261109/diagnostics-final`
- 配置 / SDK：Release / .NET SDK 8.0.423。
- 完整解决方案编译：exit 0，0 warning、0 error；XAML结构检查26/26通过。原始摘要与日志在 `build/`。
- 最新 Playnite 测试程序集 SHA-256：`AD1DAC1059896F973B95D4A4E45F1E3BAD8DBE2148DFC12A7C0EBDE8EEDF8502`。
- 四个隔离用例合计8/8通过、0失败、0跳过：Overview 2/2；Task Grid 1/1；Save History 1/1；Task details 4/4。TRX、控制台输出和桌面身份记录在 `tests/`。
- 四个测试进程各用唯一 `WinSta0\GscR5Capture_*` 非输入桌面；启动与结束时 input desktop 均为 `Default`。
- `scripts/validate-source.py .` 通过。WPF静态审查限定 `src/GameSaveCenter.Playnite`：26 XAML、0 errors、26 warnings、164 info；没有改动需要处理这些既有项。

首次本地编译捕获到测试源码中的插值语法错误，原始失败诊断保存在 `diagnostics/initial-build-failure/`。修正后完整解决方案编译通过；没有删断言或调整 skip。一次像素捕获启动曾落到全局 SDK 9.0.302，未进入测试发现；之后复用仓库 SDK 初始化函数，8项最终测试均按 8.0.423 执行。

## 像素复核

`captures/` 中七张图来自当前 WPF 控件树的离屏 `RenderTargetBitmap`，进程 DPI 为 150%。检查结果：最近任务时间和状态在 Overview 选框内；任务队列状态与进度位于选中行框内；存档历史设备文字位于行框内；任务详情的长失败文案在窄栏内换行。每个界面仍保留对应的真实滚动范围。原图按 PNG 提供，哈希和文件大小见 `file-manifest.json`。

这些图片不是屏幕截图，也不是 Playnite 宿主呈现。捕获的是测试构造的控件/详情卡，无法替代当前安装版本的真实 Playnite 页面、宿主 DPI、主题或实体输入验收。生产视图背景可能通过宿主透明材质提供，因此不能用这些离屏图评判最终页面底色。

## 闪屏反馈状态

U09-05 仍为“进行中”。用户本次附图没有 HWND、PID 或 desktop 身份，且与11月6日图像相同；本批编译和 WPF 窗口均按隔离路径运行，没有启动、关闭或重启 Playnite。该结果证明本批受控测试没有在 input desktop 建窗，不能据此称用户仍看到的闪屏已消失，也未定位 Explorer 启动竞态或 Playnite 窗口为来源。待用户报告发生时取得窗口身份/事件时序，或后续真实宿主隔离验收确认。

完整 `scripts/build.cmd`、中文完整安装启动链、精确提交 GitHub 双 SDK jobs 和真实宿主页仍按 Round5 最终候选门禁执行；本地 `-SkipTests` 与定向用例不替代它们。
## 2026-11-09 代理命令黑窗反馈续记

用户再次明确表示执行期间仍有闪屏。当前附图没有窗口身份；本次命令进程所在窗口站不能访问交互桌面（EnumWindows 返回 false、前台 HWND 为0），不将快照空值写成“没有窗口”。源码中 hide-console.ps1 为避免关闭现有交互终端，仅在祖先进程精确为 Explorer -> cmd 时隐藏调用者控制台；从 Codex/PowerShell 运行会保留调用终端。后续代理构建必须在启动 .cmd 前就通过隐藏进程运行、输出重定向到阶段日志，不能先显示后隐藏。该路径解释本代理构建可能带来的闪窗风险，但尚未证明其就是用户所见黑窗；Playnite/Worker运行窗口与用户桌面当前事件仍待拿到新鲜 HWND/PID/desktop 证据。

## 本批清理

像素图、四组 TRX/控制台与桌面身份、成功构建摘要及初次编译失败诊断已归档后，移除本阶段拥有的 .tmp/round5-frame-boundaries-20261109：802 个文件、164,938,833 bytes。删除前确认该路径在仓库 .tmp 内、无重解析点、无运行构建/测试进程引用、没有 Markdown 证据链接指向临时副本；其他任务的 TestResults/存档/配置未触碰。D 盘观测可用空间增加 167,067,648 bytes；C 盘测量间系统占用略增。详细前后数值见 [清理记录](cleanup.json)。

## 当前源码 SHA 的 GitHub CI

提交 `c30e2b7371b370a11cd255438c095545e29b927c` 的 [Windows 构建验证 #1496](https://github.com/Nikilua/GameSaveCenter/actions/runs/42535222553) 已 completed/success。基线 job `128897926440` 的 15/15 步成功，SDK 8.0.423 Release 测试为 2030 项、1972 通过、0 失败、58 个既有 skip（skip 不计通过），157 个 WPF 隔离类；离屏 Render QA、WPF 静态审查、Playnite 扩展打包与上传通过。SDK 10.0.401 job `128897926646` 的 8/8 步成功。原始日志及三个 artifact 的 run/job/SHA/大小/digest 见 [ci-run-1496.json](ci-run-1496.json)。
