# R14-05 重复媒体识别视图证据

日期：2026-09-20  
任务：R14-05 重复媒体识别视图  
代码提交：136285d5（补充媒体重复识别视图）  
分支：codex/ui-finesse-round2

## 实现事实

- 先核对现有能力：MediaSyncService.ArchiveCandidateAsync 已通过 MediaHashExistsAsync 按 SHA-256 阻止重复媒体再次入库，media.sha256 也有唯一约束；此前没有面向用户的重复组回看视图。
- 新增只读 MediaDuplicateQueryDto/MediaDuplicateInspectionDto/MediaDuplicateGroupDto 和 media.duplicates.list IPC。Worker 只查询当前选中游戏的 Assigned 媒体，最多扫描 5000 项、最多返回 100 组，每组最多展示 24 个文件。
- 确定组按非空 SHA-256 完全一致分组；由于当前入库路径默认阻止相同哈希，确定组主要兼容历史/不一致数据，不把“没有组”解释成全库绝对没有重复。
- 疑似组按同类型、文件名和大小一致分组，并排除已经有 SHA-256 证据的项目；界面分别显示“确定重复”和“疑似重复”及具体依据。
- Media 中心新增“重复识别”Tab，组列表和组内媒体列表均有限高度、FiniteViewport、Recycling；可以选择组查看文件名、时间、大小和归档路径，没有删除、移动或批量变更命令。
- 重复查询与当前媒体详情同一选中游戏边界，带请求 generation、取消令牌和重载命令；查询失败只清空该视图并保留其他媒体详情可用。

## 夹具与门禁

- Worker 隔离 SQLite 夹具 DuplicateInspectionSeparatesMetadataSuspectsFromHashEvidence 使用两个不同哈希但相同元数据的合成媒体，断言返回疑似组、组内数量和“确定组=0”；没有写真实媒体。
- Playnite 源契约夹具确认当前 Tab 可选择查看、使用 Recycling，并且片段内不存在删除/重新归类命令；同时确认 IPC、Worker 两类依据和稳定刷新命令均接通。该夹具不是唯一的交互证据，Worker 夹具覆盖了重复分组行为。
- python scripts/validate-source.py：通过。
- scripts/check-xaml.ps1 -ProjectRoot ...：24/24 通过。
- git diff --check：通过。
- Worker dotnet build --no-restore 和 Contracts 直接构建仍受 linked worktree 的 obj Access denied/当前 SDK-Workload 目标框架环境阻塞；隔离输出尝试也未得到可签收构建。Worker/Playnite 运行时测试、Release/net462、RenderHarness 和真实宿主未执行，不写成通过。

## 边界与清理

- 仅使用合成 DTO、fake/隔离 SQLite 和本地源码；未读取或写入真实存档、媒体、用户云端或对外诊断，未增加删除/移动入口，也没有绕过 ETW/系统跟踪权限。
- Demo 原目录不可用，视觉基准沿用恢复生产基线。静态/XAML 门禁和源码夹具不等价真实 Playnite 呈现、物理 DPI/跨屏、UIA/读屏/IME、presented frame、ETW 或宿主性能。
- 本阶段没有留下新的 artifacts/.tmp 产物；既有临时目录未扩大清理范围。main 用户改动、src.zip 未碰、未合并。

下一可执行任务：在可用 SDK/Workload 环境补跑 R14-04/R14-05 Worker/Playnite 定向夹具与媒体回归；随后推进 R14-06 批量目标防误选，先核对现有游戏选框、封面/平台/唯一 ID 展示和过滤后选中目标保护。
