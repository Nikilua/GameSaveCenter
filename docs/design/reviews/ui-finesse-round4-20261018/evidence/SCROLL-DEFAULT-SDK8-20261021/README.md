# 2026-10-21 滚动回归与 SDK8 默认一键构建

本证据包覆盖用户指出的存档中心表格滚动条进入详情区、任务队列滚动范围和路径候选内容被遮挡问题。它验证当前本地工作树的 Release 默认构建及受控 WPF 布局行为；不是 Round4 全部任务验收，也不代表真实 Playnite 窗口、设备或物理 DPI 验收。

## 默认一键构建

命令：

```powershell
.\scripts\build.ps1 -Configuration Release `
  -OutputRoot .tmp\round4-scroll-final-build-output `
  -TestTempRoot .tmp\round4-scroll-final-build-temp `
  -DiagnosticsRoot .tmp\round4-scroll-final-build-diagnostics
```

本地仓库锁定的 SDK `8.0.423` 执行默认全量测试。结果：restore 成功；XAML `25/25`；Release solution build `0 warning / 0 error`；汇总 `1,891` 条结果，`1,823 passed / 0 failed / 68 skipped`。测试范围包含 Core `157/157`、Worker `399/400`（1 skip）、Playnite source `533/560`（27 skip）、WPF 隔离类 `734/774`（40 skip；139 个类均完成）。142 个 TRX 的计数与汇总一致，解析错误、计数错配、失败、错误、超时和中止均为 0。各阶段退出码为 0。

68 个 skip 的原因按 TRX 分类：57 个是已撤销的“今日工作台 UI”旧架构断言；10 个受当前会话禁止创建 Named Pipe 影响；1 个 Tooltip 用例需要系统鼠标指针，而当前 Windows 会话拒绝访问。没有绕过系统权限，也没有把这些 skip 记成通过。该命令证明本机 SDK8 默认 Release 构建和已执行的本地测试通过；它不证明安装门禁、完整交互桌面或 Playnite 宿主可用。

诊断 run ID：`20261021T104126057Z-ed60009c`。摘要中的源提交为 `0ddfd8d9bb2fdea7e7589ea8ef1716346769e2da`，`workingTreeClean=false`；执行时含本轮八个已审阅的源码/测试改动，以及工作区原有未跟踪 Round3 TRX 和 `tests/GameSaveCenter.Playnite.Tests/TestResults/`。摘要完整保留了当时的 `dirtyPaths`、源文件哈希、程序集信息、每阶段状态和逐 TRX 计数。相关改动随后连同本证据一起提交；旧未跟踪产物不属于本提交。

原始汇总：[summary.json](summary.json)，原始控制台输出：本目录下的 `*-console.txt`，142 份原始 TRX：[tests/default-build-trx.zip](tests/default-build-trx.zip)。

关键证据 SHA-256：

- `summary.json`: `BE16135005F37286EFF24FBED21A0FAC64D90460C7696947ABC2B374A0A6663C`
- `tests/default-build-trx.zip`: `EB2162925C8D01FF481C7A74F1D2E62860BF0A2DB47D0938198F1172F34070D1`
- `render/render-qa-report.txt`: `9A15CB077B13017ED22B36B63D17F9A7D4B34E5023B1355A236491A1151A2B07`

## 滚动布局复核

RenderHarness 对 Task Center、Maintenance 和其他共享页面按多个窗口尺寸生成离屏图像与几何报告。Task Center 在生产 PageHost 尺寸 `1040x700`、`1100x720` 和 `1366x768` 下均保留 4 行队列，表格视口为 `274 DIP`；页面可继续承载检查器，窗口变窄或高度恢复时重新计算预算。报告包含缩小/放大过渡检查，`Task resize transition recovered 5 metrics`，最终 `render-qa OK`、0 个 `PROBLEM`。Maintenance 的保留策略游戏/版本 DataGrid 使用有限视口，并为无数据/未选中状态显示说明，不用空白表遮住下方内容。

完整报告：[render/render-qa-report.txt](render/render-qa-report.txt)，抽样图像：[任务队列](render/screenshots/Task-1040x700.png)、[PageHost 任务页](render/screenshots/Shell-Tasks-1040x700.png)、[Maintenance 六个标签页](render/screenshots/)。RenderHarness 的截图由合成数据和离屏 WPF 窗口生成，不能当作用户桌面的真实截图或 Playnite 宿主呈现证据。

修正一项测试夹具问题：先前 RenderHarness 的 resize probe 将 Task Center 留在未 Loaded 的 Grid 中，因此没有生产 PageScrollViewer 的 `ScrollChanged` 更新，产生了错误的缩小后恢复失败。现在转场探针将页面装入隐藏、无边框 WPF Window 并使用生产 PageHost 内容高度；真实产品窗口不会因此被创建或操作。该修正消除了探针生命周期缺失造成的误报。

## 复核与范围

- `scripts/validate-source.py .`：通过。
- `scripts/check-xaml.ps1 -ProjectRoot .`：`25/25` 通过。
- WPF UI 静态审查：`0 error / 27 warning / 179 info`；告警不是本次断言。
- `git diff --check`：通过。
- 当前受控截图/STA/WPF 不覆盖真实 Playnite、显示器缩放、物理滚动输入和宿主样式；需要隔离 Playnite 真机环境的门禁仍按计划阻塞。
- Round4 的 `S00-01` 继续是“已满足”，证据仍指向既有命令目录；本批没有新置 READY 的后继计划项。S00-02、S00-03、S07-03 等未完成项仍保留各自状态，Round4 没有整体完成。
