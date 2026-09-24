# R23-04 合成非空库夹具准备（2026-09-24）

## 实现范围

- 新增仅供测试的 Playnite GenericPlugin，默认不安装、不运行，也不进入 GameSaveCenter 正式包。只有 `scripts/real-host-audit.ps1 -SeedSyntheticLibrary` 会把它部署到明确指定的隔离扩展目录。
- 合成目录默认 64 条，硬上限 512。游戏名称以 `GSC Audit Synthetic` 开头，稳定 ID 以 `gsc-ui-audit-r23-04-` 开头；全部标记未安装，不设置安装目录或真实游戏路径。重复执行按 ID/名称补缺，不删除或改写其他条目。
- 启用前要求 `UserDataDir` 位于仓库 `.tmp/`，并拒绝路径祖先中的 reparse point。导入通过 Playnite SDK `IGameDatabase.ImportGame`，只作用于隔离 Playnite 数据库。插件按本次 run ID 写入合成库 manifest；运行器只接受相同 run ID 的结果，未观察到则明确记录 `not-observed`。
- 隔离审计把构建输出放在 profile 的 `audit-fixtures/build`，并启用 `SkipPackageArchives`，因此包归档历史哈希不会因运行隔离审计而被覆盖。安装日志与报告写入本次 audit output，不覆盖通用安装日志。正常产品插件与业务服务没有改动。

## 验证

- Release solution 构建成功，`0` errors；保留两条既有 `MediaCenterView.xaml.cs:703 CS8602` warning。Seeder 编译目标为 Playnite `net462`。
- `scripts/validate-source.py`、XAML `24/24` 与三个 PowerShell AST parse 检查通过。
- 合成目录稳定性/边界/无安装目标 `4/4`；宿主 evidence source tests `6/6`；用户报告四页 Light/Dark 几何行为 `8/8`，0 failed/skipped。
- 无效 `UserDataDir` 负例在输出目录改动前被拒绝，保留标记文件并清理自建测试目录。

## 当前边界与下一步

本记录只说明夹具代码、编译、测试和路径门禁已准备；本阶段未启动 Playnite，也没有声称产生了非空库或通过真实宿主验收。下一步在代码提交后，按现有隔离流程仅启动一次：使用仓库 `.tmp/` 下的新 profile，`-SeedSyntheticLibrary -SyntheticLibraryCount 64 -SkipInstallTests`。检查本次 manifest 的 64 条记录、组装身份和隔离数据库计数；再读取 CEF/Playnite 启动结果。若 CEF `platform_channel 0x5` 仍阻止正常启动，记录实际 seed 是否在失败前完成，不重试相同阻塞状态，不把受控窗口或专用窗口写作 Embedded 视觉通过。

未触碰真实存档、媒体、用户云端或诊断数据。R23-04 仍待真实 Embedded Playnite/UIA 环境通过。
