# R16-04 恢复默认粒度定向复核

日期：2026-09-23

复核提交：`666c60bb`（`codex/ui-finesse-round2`，D 盘工作区）

## 结论

R16-04 的受控实现条件已满足，账本状态校正为“已满足，待环境验证”。本批没有新增生产代码，复用 `2b194461` 的设置恢复目录、当前 Playnite 草稿重绑和取消保护；先发现仓库内旧隔离程序集导致身份保护拒绝读取，再用当前 checkout 的隔离 Release 输出重建并复测通过。

## 实际复测

- Playnite `R16SettingsDefaultsBehaviorTests`：`2/2` 通过，覆盖全部默认只处理安全字段与本地 UI 偏好、保留 Worker/Ludusavi/Rclone/路径/云端目标和设备身份，以及取消恢复重置前的草稿。
- Playnite `R16SettingsDefaultsSourceTests`：`1/1` 通过，确认单字段、四个分类和全部默认入口均接线，重置刷新没有调用 `EndEdit`。
- 使用当前提交在 `.tmp/r16-04-recheck-20260923` 的隔离 Release solution 构建：`0 errors / 2 existing warnings`，两条均为 `MediaCenterView.xaml.cs:706` 的既有 `CS8602`；源码程序集身份与 `666c60bb` 一致。
- `python scripts/validate-source.py`、XAML 结构检查 `24/24`、`git diff --check` 通过。第一次直接复用旧输出得到的 `1f968819`/`666c60bb` 身份不一致已记录为测试夹具边界，未将失败写成实现失败。

## 保留能力与边界

- 继续保留游戏选框、滚动条、命令/Binding、保存/取消、错误/取消语义、恢复保护、有限列表性能和 Playnite `net462`；恢复默认只改当前草稿，不保存、不启动 Worker。
- 测试使用合成 settings、隔离构建目录和源码契约，没有修改真实存档、媒体、云端、用户配置或诊断数据。Demo 原目录不可用，沿用恢复生产基线；没有把源码契约或离屏边界写成真实呈现结论。
- 未验真实 Playnite/package-host 点击与视觉呈现、浅深主题、UIA/读屏/IME、DPI/跨屏、RenderHarness presented frame、ETW 或宿主性能。

下一可执行任务：`R16-05 路径编辑一致`。先核对路径浏览、校验、打开、复制和权限/网络/不存在负例；当前临时复测目录待本阶段提交后清理。
