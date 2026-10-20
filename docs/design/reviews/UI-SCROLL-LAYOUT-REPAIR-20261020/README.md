# 页面滚动区域越界修复

## 问题与根因

用户提供的 Playnite 截图显示存档历史表格下方详情、任务队列和路径候选列表被遮挡，内层滚动条延伸到所属区域之外。检查发现存档与任务页把根内容高度固定为基于 `ActualHeight` / `ViewportHeight` 推算的旧布局结果；窗口、Tab 或详情切换后的下一次测量仍沿用旧值，导致页面后续内容被裁切。DataGrid 与详情滚动器本身仍可滚动，因而表现为滚动条仍有效但显示区域被遮住。

## 修复

- `SaveCenterView` 和 `TaskCenterView` 不再给整页根布局写入缓存高度；PageHost 当前高度是布局预算，页面外层 ScrollViewer 自然测量整页内容并承担剩余纵向滚动。
- 只限制虚拟化 DataGrid 及详情滚动器的内部视口高度，保留内部滚动；历史与候选路径表格高度有限，详情在并排与堆叠布局均有上限。
- 重排/调整大小时使用当前 PageHost 高度，避免旧 Page ScrollViewer `ViewportHeight` 反馈到新的表格尺寸。
- 不改共享滚动条主题或 DataGrid 模板；当前媒体列表已有独立的有限视口行为，本次以回归测试确认其未受影响。

## 验证

- SDK `8.0.423`，`scripts/build.ps1 -Configuration Release -SkipTests`：Release 全解决方案构建成功，`0 warning / 0 error`；XAML 结构检查 `25/25`。构建控制台：[build-console.txt](build/build-console.txt)，诊断目录 `20261020T120122348Z-dcd21c2d`。控制台 SHA-256：`37B7910921B5F2431932A8E1E649203FA1C6B45F9F806811905783241606FE8E`。
- STA/WPF 页面与表格几何回归：`5/5` 通过，覆盖存档历史、候选路径、任务队列、宿主高度变化后的表格及详情可达性、当前媒体列表；[TRX](tests/scroll-layout-final.trx)，SHA-256：`B36D46D1CC9D507BAA205B4C6F39BE71D7D6612A28D15284BC04084712D69794`。
- WPF 源码/资源契约检查：`4` 通过、`0` 失败、`1` 跳过。跳过项是已撤销 UI 架构对应的历史断言；[TRX](tests/scroll-layout-contracts-final.trx)，SHA-256：`86A204FF0F030661B2086F7794204C629DDE214036491C11A6B01A9F66DE94CB`。测试程序集 SHA-256：`A865B3D6AECE5268E6702058F7A1E0513DFA8DCCA42E2E69EA6835EEE41555A5`。
- 仓库源码/XAML 校验器经本次有限视口规则更新后通过；`git diff --check` 通过。
- 构建验证对应基线提交 `3ef50cc27d80b4ddf3ad342c602837db8e143787` 的工作树修改；提交前的构建摘要记录 `workingTreeClean: false`，改动范围为本修复的四个源码/测试文件及保留的用户 TRX/TestResults。此处没有记录 CI 通过，也没有为此触发 CI。

STA 几何测试检查 WPF 实际测量边界、内外 ScrollViewer 位置及缩小宿主后到达详情末端的能力；它是离屏控件证据，不代表真实 Playnite 窗口或物理 DPI 已复验。此次没有使用模拟截图作为真实屏幕证据。用户提供的截图仅作为缺陷复现线索。

## 2026-10-21 用户截图复核：视口预算与内容裁切的后续修正

用户随后提供存档历史、任务队列和路径候选的宿主截图，确认 10 月 20 日修复仍有可见问题。复查发现遗漏两点：三个整页根 Grid 仍设置 `ClipToBounds`，且表格仍按页面视口的 `68%` 分配，没有从高度中扣除筛选/摘要/表头/详情区；视口测量值也可能滞后一轮，缩小后再放大会把表格留在短窗高度。

- 移除 `SaveHistoryLayout`、`SaveCandidatePageSurface`、`TaskPageScrollSurface` 的根裁切；保留 `SaveTableFrame` / `TaskTableFrame` 的局部裁切，使内层表格滚动条留在卡片内。
- 历史表、路径候选和任务队列都从当前宿主与已测 Page ScrollViewer 视口取得预算，减去当前页面工具区、表格框架与堆叠详情区后再给虚拟化表格分配高度；详情和其余页面内容由外层页面滚动承载。
- 宿主变高时先接受新的宿主高度，避免较小旧视口阻止恢复；缩小时保守使用宿主/已测视口较小值，布局稳定后让实测视口修正。保留四行可读下限及现有表内滚动。

验证：SDK `9.0.302` Release 全解决方案 `0 error`（9 条 `NU1900` 均为 NuGet 漏洞索引因网络不可达；依赖从缓存还原）；布局几何与端点行为 `6/6`；25 个 XAML 文件结构检查通过；`scripts/validate-source.py` 与 `git diff --check` 通过。RenderHarness 双主题、多尺寸、`2560x1440 → 1100x720 → 2560x1440` 窗口恢复矩阵为 `render-qa OK`，无 `PROBLEM`。构建、TRX、离屏报告和 SHA-256 见本目录的 `build/follow-up-release-sdk9-console.txt`、`tests/scroll-layout-follow-up.trx`、`render/render-qa-follow-up.txt`、`render/render-build-sdk9.txt`。

- 构建日志 SHA-256：`2978B6F7FB25859382019071E04496410B2DDBB3D5EA6891125D7A9793CD854E`。
- 几何 TRX SHA-256：`7A77368B35287589E68944E8E077889296D852ECFCF896C983431557100F5046`。
- RenderHarness 报告 SHA-256：`18EEE0EDC4B4DA7077A56FDEAB5253DB998FE3FE1E856DF2A87C104E576C51BE`；RenderHarness 构建日志 SHA-256：`8FF262CE9455C5A5DBA93C36623306A782FB3964C297C14BE3AC40D297EFE9ED`。

该机器未安装仓库 `global.json` 锁定的 SDK `8.0.423`；此次用 SDK `9.0.302` 从仓库目录外构建，未改 SDK 锁定。RenderHarness 与 WPF 行为测试使用合成数据、逻辑 DPI `1.00`，只证明受控几何/内容可达，不是用户真实屏幕；没有重新启动 Playnite。真实 Playnite 窗口和物理 DPI 仍待宿主复验。
