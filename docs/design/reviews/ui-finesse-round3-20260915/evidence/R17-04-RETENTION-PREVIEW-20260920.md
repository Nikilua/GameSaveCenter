# R17-04 保留预览对比证据

日期：2026-09-20  
状态：已满足，受控验证完成；真实宿主验证待进行  
证据提交：`3c73b498`（`补充保留清理失败证据`）

## 现有能力核对

- 既有 `RetentionSimulationService.PreviewAsync` 已返回候选数、明细、用户锁定/PreRestore/健康恢复点保护数、索引体积预计释放和隔离账本占用；预览明确只读。
- 既有 `ApplyAsync` 已校验二次确认、预览句柄、生成时间、十分钟时效、候选/策略/归档指纹，并在持有共享游戏操作锁后重读 live 状态；过期或状态变化不会继续清理。
- Apply 已将 `MovedBytes`、`FreedBytes` 和隔离账本状态分开累计；只有隔离文件删除成功才进入 `FreedBytes`，索引删除失败会恢复原路径并保留恢复账本，未把隔离或索引删除误报为实际释放。
- 因此本阶段没有重建服务或替换 UI，只增加一个真实隔离 SQLite 失败负例，补齐现有能力的可审阅证据。

## 验证

- Worker `RetentionSimulationServiceTests`：`12/12`。新增索引删除失败测试实际创建 SQLite trigger：归档进入隔离后删除索引失败，归档恢复到原路径，`MovedBytes=0`、`FreedBytes=0`、失败计数增加，账本保留恢复状态；同组还覆盖候选/保护、成功释放、锁忙碌、过期预览、状态/策略/归档变化和取消。
- Playnite R17：`10/10`；维护保留预览源码门禁：`3/3`；布局回归：`20 passed / 11 skipped`，跳过项是既有条件性布局夹具，未写成全绿。
- 完整 `GameSaveCenter.sln` Release 构建：`0 errors / 2 warnings`；两条均为既有 `src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml.cs:664` nullable warning。
- `validate-source.py`：通过；XAML 结构：`24/24`；`git diff --check`：通过；WPF 静态质量：`0 errors / 28 warnings / 162 info`，无新增错误。
- 行为验证只使用合成策略、fake/隔离 SQLite trigger、隔离存档目录和临时文件；没有删除真实媒体、存档、数据库或写用户云端。

## 未验证边界

- 未启动真实 Playnite/package-host，未把最终浅深主题、DPI/UIA/IME、焦点/滚动、presented frame、ETW 或宿主性能写成通过；WPF warning/info 为既有基线。
- 未在真实用户目录验证 Explorer/权限、真实进程锁、真实文件系统故障和重启恢复时序；隔离账本分支由合成 SQLite/目录故障覆盖。
- Demo 原目录不可用，沿用恢复生产基线；main 上用户未提交的 `DashboardView.xaml.cs`、`src.zip`、对话框文件/测试未触碰，当前未合并 main。

下一可执行任务：`R17-05 隔离账本入口`，先核对已有分页隔离列表、原路径/隔离路径/状态展示和受控恢复入口，不默认删除残留。
