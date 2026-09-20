# R17-01 健康结果分层证据

日期：2026-09-20  
状态：已实现，待真实宿主验证  
实现提交：`eb033251`（`按影响分层维护诊断结果`）

## 现状核对与实现

- 既有 SQLite `findings.resolved` 和 `GetOpenFindingsAsync` 的 `resolved=0` 语义已复用；健康巡检原有稳定 finding id 和解决入口未替换。
- `ValidationFindingDto` 现在保留 `CreatedUtc`，Worker 查询同时返回 `created_utc`；维护详情显示本地化证据时间，历史未知时间显示“证据时间未知”。
- `FindingTriageResolver` 在 Playnite 展示边界按“游戏 + 稳定代码 + 问题标题”合并重复来源，错误/严重优先，时间较新的同级证据优先；健康巡检不同备份标题不同，因此仍分别保留。
- 维护页新增“需立即处理 / 建议处理 / 信息项”三档摘要卡，原 `FindingsGrid`、选择、详情、复制、导航和现有滚动系统继续使用；没有把三档摘要写成虚假的修复入口。

## 验证

- Playnite R17 定向测试：`5/5`，最终源码身份绑定 `eb033251`。
- Worker R17 定向测试：`2/2`；实际隔离 SQLite 验证证据时间返回，以及解决健康 finding 后开放队列为空。
- 完整 `GameSaveCenter.sln` Release 构建：`0 errors / 2 warnings`；两条均为既有 `src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml.cs:664` nullable warning。
- `validate-source.py`：通过；XAML 结构：`24/24`；`git diff --check`：通过；WPF 静态质量检查：`0 errors / 28 warnings / 162 info`，未新增错误。
- 测试使用合成 DTO、fake/隔离 Worker 和临时 SQLite 目录；没有读取或修改真实存档、媒体、用户配置、云端或诊断上传目标。

## 未验证边界

- 未启动真实 Playnite/package-host，未宣称最终浅深主题呈现、DPI/UIA/IME、真实焦点/鼠标滚动、presented frame、ETW、物理跨屏或宿主性能。
- 未验证真实多来源生产数据的标题规范是否始终稳定；当前去重键刻意保留问题标题，以避免把同游戏不同备份的健康问题合并。R17-07 将继续覆盖问题定位和返回后的筛选/滚动。
- Demo 原目录不可用，沿用已恢复生产基线；主分支用户改动、`src.zip` 和未跟踪对话框文件未触碰、未合并。

下一可执行任务：`R17-02 诊断包预览`，先核对现有 `DiagnosticsPackageService`、维护页生成入口和脱敏清单，再补生成前类别/脱敏范围预览与生成后大小/位置结果。
