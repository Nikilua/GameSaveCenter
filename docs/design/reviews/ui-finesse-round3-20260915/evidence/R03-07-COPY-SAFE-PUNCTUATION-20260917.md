# R03-07 文案标点统一证据（2026-09-17）

## 结论

R03-07 的当前生产可见范围已满足受控验收条件。代码提交为 `5a07edaa2fedaebf98c681530d13182fe103f085`（`统一任务文案标点并保护错误详情`），已推送到 `codex/ui-finesse-round2`。

- 盘点当前生产 Views、Contracts 和 Worker 后，确认任务筛选区的半角冒号是可直接修复的可见不一致；状态、类型、范围、时间四个标签统一为全角冒号，不改变筛选控件、绑定或响应式布局。
- 失败任务的 `DetailMessage` 改为生成“错误码：…；…”的展示层文本；`ErrorCode` 属性保持原值，错误正文保持原值。错误码为空时直接显示错误正文，不生成“错误码：；”孤立标签。
- 整库备份的失败/取消聚合详情同步使用“游戏：…；…”；这是可见错误详情的分隔调整，不改变任务状态、错误码、路径或恢复流程。
- 既有显示单位规则继续保留：Latin 单位使用 `1 KiB` 形式，中文单位使用 `1 秒`、`2 项` 形式，百分比保持 `12%` 形式；`Worker`、`Playnite`、`Ludusavi`、`FLiNG`、`ID`、`EXE` 等英文缩写/产品名不改大小写或强行插入空格。
- `R03CopySafePunctuationTests` 定向 `3/3`：实际 STA WPF 生产 Task 视图读取四个标签；失败详情样本包含 `C:\Saves\A:1`，验证生成分隔符变化不会替换正文中的半角冒号；空错误码和非失败原消息均不添加错误码标签。
- 隔离 Release 全量：XAML `24/24`；构建 `0` 警告、`0` 错误；Core `83/83`、Worker `311/311`、Playnite `566/623` 通过、`57` 跳过、`0` 失败；源码校验和 `git diff --check` 通过。
- clean commit 上的双主题 RenderHarness 绑定 `5a07eda`，`WorkingTreeClean=True`，`render-qa OK`，共 `357` 张 PNG；Task `1040×700`、`1100×720`、`1366×768` 均通过页面、表格可读行和滚动探针，人工抽查 Light/Dark Task 图。

## 本轮术语与标点表

| 语义 | 展示规则 | 本轮证据/边界 |
| --- | --- | --- |
| 中文字段标签 | 使用全角冒号 `：`，不再使用 `状态:` 这类半角冒号 | Task 状态、类型、范围、时间四个生产 TextBlock 实例为 `状态：`、`类型：`、`范围：`、`时间：` |
| 同一条生成详情 | 字段间使用全角分号 `；`；语义标签使用全角冒号 | 失败详情为 `错误码：RCLONE_COPY_FAILED；...`，整库聚合为 `游戏：...；...` |
| 中文上下文括号 | 使用全角 `（…）` | 现有任务/诊断可见文案继续保持；技术标识、路径和用户原文不做括号替换 |
| 英文产品名/缩写 | 保持原大小写和项目约定空格，如 `Worker`、`Playnite`、`FLiNG Trainer`、`任务 ID`、`EXE / CT` | 本轮没有对用户游戏名、错误码、路径或协议字段做机械空格化 |
| 数字与单位 | Latin 单位前保留一个 ASCII 空格；中文单位与数字之间保留一个空格；百分比不插空格 | 沿用现有 `1 KiB`、`1 秒`、`2 项`、`12%` 显示属性；没有改排序键或原始数字 |
| 技术错误码与路径 | 只格式化外层展示分隔符；错误码和路径内容原样保留 | `TaskStatusDto.ErrorCode/ErrorMessage` 属性不变；含 `C:\Saves\A:1` 的正文逐字保留；R03-05 已验证复制路径直传原始值 |

## 变更范围与已有能力核对

- 复用现有 `TaskCenterView` 的标签和 `TaskStatusDto.DetailMessage`，没有新增文案服务或替换设计体系。
- `DetailMessage` 仍只作为展示/搜索/通知详情来源；复制任务诊断仍由既有 `CopySelectedTaskErrorAsync` 组装完整的游戏、错误原因、错误码、技术详情和任务 ID，未放宽命令可执行条件。
- `CopyPathCommand`、`CopyPathAsync`、`PathDisplayConverter`、只读完整路径控件和路径绑定未修改；R03-05 的证据继续负责“复制内容无省略号”的具体行为，本轮只增加错误正文含路径字符的保留负例。
- 游戏选框、滚动条系统、命令/Binding、取消/错误语义、恢复保护、有限列表和 net462 兼容未改变。

## 可复现验证

```powershell
$env:GSC_BUILD_COMMIT=(git rev-parse HEAD).Trim()
$env:GSC_SOURCE_ROOT=(Get-Location).Path
dotnet test tests/GameSaveCenter.Playnite.Tests/GameSaveCenter.Playnite.Tests.csproj --no-restore --filter FullyQualifiedName~R03CopySafePunctuationTests --verbosity minimal
powershell.exe -ExecutionPolicy Bypass -File scripts/build.ps1 -Configuration Release -OutputRoot .tmp/r03-07-build
powershell.exe -ExecutionPolicy Bypass -File scripts/render-qa.ps1 -Configuration Release -Output .tmp/r03-07-render-final
```

最终报告来自隔离输出、合成 DTO、实际生产 WPF 视图、受控 STA 和 offscreen logical DIP；`DpiScale=1.00` 不能代表真实宿主物理 DPI。没有启动真实 Playnite，也没有写真实存档、媒体、云端或 OS 剪贴板。真实 Playnite presented frame、宿主字体替换、物理 DPI/跨屏、OS 输入/IME、屏幕阅读器、ETW 和宿主性能仍未验。

下一可执行任务：R03-08 用户文本缩放；先盘点固定高度控件、系统文本放大/自选字体入口和现有宿主边界，再做小批量兼容验证。
