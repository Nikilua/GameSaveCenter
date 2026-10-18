# S07-04 避免 CI 自循环记录

## 现状审查

- `.github/workflows/windows-build.yml` 对 `main` 的所有 push 触发双 SDK 构建、测试、渲染 QA 与打包；当前没有文档路径过滤。本项未更改 workflow、branch protection 或 required checks，路径分流另属 S07-03。
- `scripts/build-diagnostics.ps1` 保留每次诊断的 `sourceCommit`、dirty paths、SDK、step exit code、程序集/PDB 身份；CI 上传 JSON、console 与 TRX artifact。既有 run/job/artifact 可由 GitHub 与证据文档查询。
- `4cde34ce`、`188c8a74`、`5eb0d1fd`、`4d55a7dd` 连续以更新上一次 CI 结果的纯记录为主，后续又对文档提交运行 CI，形成重复记录链。Round4 计划执行规则 #8 已提出“不反复制造纯文档 CI 通过记录”，但需要在仓库启动规则中更明确地执行。

## 已实施规则

根目录 `AGENTS.md` 现要求：

1. CI 结果绑定实际验证的提交、SDK、job 与原始诊断产物。
2. 仅在出现新失败/修复、发布或验收状态变化时新增结论；已可查询的通过结果无需复制到新的纯记录提交。
3. 纯文档记录提交触发的通过结果不再单独产生后续 CI 结果提交，避免自循环。
4. 不调整 workflow 触发器、必需检查或分支保护；此类变化需按 S07-03 独立审查。

## 验证与边界

- `python scripts/validate-source.py .`：通过。
- `git diff --check`：通过。
- 规则与证据链接存在性检查：通过。
- 未运行产品构建或测试；本项是流程规则变更。没有对 GitHub 上的 branch protection/required checks 做未授权推断，也未改 workflow。
