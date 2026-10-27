# U03-08 / U04-01 / U04-02 去重与验证

日期：2026-10-28。状态：代码完成待验收；当前远端双 SDK workflow 尚未结束。

## 范围与前后变化

- **U03-08 数值及路径编辑**：盘点后没有发现生产实现缺口，不重复造控件或验证层。既有数值编辑器已验证范围、溢出和滚轮边界；路径设置已有预览、目录/文件类型检查、缺失子目录说明和本地打开保护。非法值不会通过控件刷新静默写入有效配置。本批无生产资源改动。
- **U04-01 动效语义**：生产 `AcrylicReferenceControls.xaml` 显式合并 `MotionTokens.xaml`；短 hover 转场归到 `GscMotionFast`（120 ms），press 归到 `GscMotionPress`（100 ms），DisclosureCard 展开/收起的 150 ms 转场也改用 fast token。ProgressBar 的 2 s 不确定进度扫动保留为长节奏，并在资源旁说明它表达持续进度，不属于短交互转场。`WpfUiProduction.xaml` 原先已使用相同令牌。
- **U04-02 快速掠过**：生产按钮的合成 `IsMouseOver` 状态以每个主题 3 轮、8 个兄弟按钮快速切换；清除 hover 后持续泵送 dispatcher，确认无活动探针和残留 overlay，再验证 hover 能重新进入。测试合并在同一个 STA 用例中运行，避免多事实各自创建 WPF `Application` 的跨线程冲突。

实现提交：`631b218e48cf2fb3d2be9be02ebf680551398db1`；STA 场景整合：`95b5dbf942e44a23da46a2f2227f3c22b8a7f7b7`；Light/Dark 行为覆盖：`0c7c42687bd3be6f1f97192c50b5ca580e6f4545`。三笔均已推送到 `main`。后两笔只调整测试覆盖，不改变生产 XAML。

## 本地检查

- SDK 8.0.423，最新 SHA `0c7c42687bd3be6f1f97192c50b5ca580e6f4545`：数值/路径定向测试 21/21，动效 token + Light/Dark 合成行为 2/2，0 skip。TRX 与 SHA-256 在 [`tests-manifest.json`](tests-manifest.json)。
- `python scripts/validate-source.py` 通过；`scripts/check-xaml.ps1` 通过 26/26。WPF 静态审查扫描 37 个 XAML：0 error、26 warning、179 info（既有基线）。这些检查针对实现提交；随后两笔仅改变测试文件。
- Release RenderHarness 构建 0 warning / 0 error。提交 `95b5dbf9` 的完整 render-qa 覆盖浅/深主题与页面/尺寸探针，394 PNG、0 `PROBLEM`，报告末尾为 `render-qa OK`。该提交与最终 `0c7c4268` 的生产资源完全相同；只在后者增加了深色合成行为断言。报告见 [`render-qa/render-qa-report.txt`](render-qa/render-qa-report.txt)，SHA-256 `567a6fe8fd2f98d8b996527e54b270cdc25597c40b077cab24ecabfacdc10971`。
- 合成 `IsMouseOver` 不等于物理指针输入。未声称真实 Playnite 宿主、Narrator/UIA 客户端、物理鼠标、IME 或实体 DPI 已验证；游戏选框、滚动条、命令/Binding 和业务/恢复安全语义未改。

## GitHub Actions

每次 push 都检查 `.github/workflows/windows-build.yml` 的两个适用 job：SDK 基线 `编译、测试与打包` 和 SDK 10.0.401 复核。run/job/artifact 状态见 [`ci-runs.json`](ci-runs.json)。提交 `631b218e` 的baseline与SDK10 job `122424019673`/`122424019068`均失败于同一 `R08MotionReverseBehaviorTests.ProductionButtonHoverAndPressReverseFromTheirRenderedValues` STA夹具：另一线程拥有的WPF `Application` 导致`Application.set_ShutdownMode`跨线程异常；两job均为1425 pass、1 fail、19 skip。原始日志已读取，baseline诊断artifact `12767366434`（738991 bytes，SHA-256 `d3fe80b3267c55515af4d5057ce0cf978fc1557c7e10f0ed447b24e3fc7d2913`），SDK10诊断artifact `12765473491`（645803 bytes，SHA-256 `477d13c3bd5a3bf5f13ce0e5d9ed61d10d22258221de3016b2f0ddf42e603fa0`）。提交 `95b5dbf9` 将场景并入同一 STA 用例，`0c7c4268` 再加入深色覆盖；两者的远端 job 仍在运行，不能据此结项。

读取 `main` 的 required status checks 接口返回 HTTP 403 `Resource not accessible by integration`，因此不对分支保护配置作推断。失败历史保留，不修改 workflow 或保护规则。

## 输出与清理

RenderHarness 阶段输出为 `.tmp/round5-u03-08-u04-01-02-render-95b5dbf9`：395 文件、53183890 bytes，其中 394 PNG 共 52042066 bytes。完整报告已复制并核 SHA 到本目录；图像只留一份临时集，没有复制截图副本。渲染开始前 D 盘可用 6429085696 bytes、C 盘 1582723072 bytes；结束后分别 5419372544 / 1569579008 bytes，空间变化未归因，未清理其他任务/用户输出。当前阶段文件及路径边界见 [`cleanup.json`](cleanup.json)。
