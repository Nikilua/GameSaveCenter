# S05-04 预算分解

实现提交：`cf50643c05df3fd6e69df7d6252842fa88a1ffde`（`增加存储预算分项盘点`）。

## 等价能力审计

已有备份存储分析能递归统计 Ludusavi 目录文件长度，并对照 SQLite 版本索引的逻辑大小；但维护页没有将本地存档、第二镜像、远端恢复 staging 和媒体归档放在同一预算视图。镜像状态的 `TotalBytes` 可能直接来自同步标记，媒体 SQL 汇总只按单个游戏并只统计已归类记录，staging DTO 仅记录暂存身份/校验/有效期。以上都不能提供四类目录的当前文件长度及完整统计范围。

## 实现行为

- “存储预算分项”位于维护中心当前可见的保留策略页存储卡。每类各自显示扫描状态、目录文件长度和文件数；本地存档与媒体另显示相应 SQLite 逻辑体积/记录数。媒体逻辑统计覆盖所有登记状态，目录文件长度也会包含未登记文件。
- Worker 只读遍历配置的存档、镜像、远端恢复 staging、媒体目录。未启用镜像显示“未启用”；目录已存在但为空显示 `0 B / 目录为空`；目录缺失、不可访问或遍历未完成时不把部分结果写成 0，而显示未知状态。staging 目录尚未创建时保持未知，不推断为 0。
- 分项不产生合计。镜像行标明其副本关系；检测到两个配置目录相同或互相包含时，两行均提示可能重复并要求勿相加。文件长度合计不是 NTFS/卷簇实占，卷剩余容量继续由现有 `DriveInfo` 指标单独显示。
- 未新增清理、同步、暂存、远端访问、设置写入或媒体文件操作。既有单游戏媒体汇总、镜像同步及恢复保护流程保持原样。

## 验证

- SDK `8.0.423` Release solution build `0 warning / 0 error`，提交 `cf50643c05df3fd6e69df7d6252842fa88a1ffde`，诊断 `20261020T150304229Z-4f5cc725`。原始构建记录与摘要见 [build-console.txt](build/build-console.txt) 和 [summary.json](build/summary.json)，SHA-256 分别为 `B7F24D3F577950E5F998E57685B7F41A54D5E069E0031F20D29799952CEE27DF`、`F41AB6821AE5FF103AE9CF9E1912571DCC2FA1C2D764F95BB6817B0EC665EAEC`。
- Worker `StorageAnalysisServiceTests` `8/8`：检查四类目录的独立物理文件长度/逻辑索引、媒体全部状态汇总、空目录与未知的区别、镜像未启用和重叠路径警告。[TRX](tests/worker-storage-analysis.trx)，SHA-256 `B95821F9ADFE366E432DEC5A1A373C40EE55378930BA2EC0066C4D1AC21DB29B`。
- 生产 WPF Light/Dark 实际模板与 `R17StorageAnalysisNavigationTests` 共 `4/4`；测试在两主题下加载可见维护页存储卡、滚动到分项并核验四类数据和未知状态绑定。[TRX](tests/playnite-storage-budget-ui.trx)，SHA-256 `B277355C48793C0B665C33B7F5960F9DBD5E02F569BCA27744D2F067CCB9D8ED`。Worker 与 Playnite 测试程序集 SHA-256 分别为 `7B0511737C2BA2C8452C0ADAB358D5C461F85EF3ED61167ABDD861364CBEEECB`、`428FE7D372016F33B09C5D806E6C872C9340BE4476075BF679F67C80EE9083CF`。
- `scripts/validate-source.py` 与 `git diff --check` 通过，校验日志见 [source-validation.txt](validation/source-validation.txt)。构建摘要只列出启动时保留的 Round3 未跟踪 TRX 和 `TestResults/` 为 dirty paths；没有修改或纳入提交。

没有运行完整 WPF 隔离循环、真实 Playnite 页面或物理 DPI 验收。目录文件长度不代表磁盘簇分配/压缩/稀疏文件实占；镜像和嵌套路径重复项不能相加。没有因本项触发 CI。
