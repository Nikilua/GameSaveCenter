# 失败尝试保留说明

- 初次全量本地Release阶段的3个Playnite settings/worklist恢复用例因系统TEMP路径深度导致备份路径写入断言失败。短TEMP根目录后这3项通过；原TRX保存在 failed-full-run-playnite-source-path-limit.trx。没有改备份或恢复断言。
- 最初的1040×700媒体壳层版本仍出现两行批量操作，新增的双主题布局断言准确失败，原TRX在 failed-first-layout-assertion.trx。修正真实操作布局后，同一原断言Light/Dark均通过。
- 为适配紧凑高度曾尝试把模式选择移入会折叠的简介区；该路径不满足紧凑视口，因此已撤回，模式选择继续保留在批量工具栏中。
- 这些诊断仅说明本批修正过程；局部定向测试不替代最终冻结候选的中文根入口完整链。
