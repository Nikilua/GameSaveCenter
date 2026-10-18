# S07-02 结构化任务状态

## 改动

- 新增 `ROUND4_STATUS.json`，为 64 个 Round4 ID 分别登记总体状态、实现状态/commit、受控验证状态、宿主验证状态、验证说明、结构化证据链接与边界。该索引只覆盖 Round4，不写回或清除历史 Q/R 状态及外部阻塞。
- 新增 `scripts/render-round4-progress.py`。`--write` 从 JSON 生成 Round4 账本的状态摘要与 64 行表；未标记为生成内容的手写背景与解释仍保留。`--check` 校验 ID/标题与实施计划一致且唯一、状态字段有效、实现完成项有 commit、通过状态有证据、结构化证据索引和表格链接一致，所有证据路径位于仓库内且文件存在。
- 将 `scripts/render-round4-progress.py --check` 接入 `scripts/validate-source.py`；后续手动或 CI 源码检查会发现状态 JSON 与账本摘要/表格不一致。

## 验证

- `python scripts/render-round4-progress.py --write`：生成 64 个任务条目。
- `python scripts/render-round4-progress.py --check`：64 个唯一 ID、计划标题、所有证据路径和生成块通过。
- `python -m unittest discover -s scripts/tests -p test_round4_progress.py`：5 项通过，覆盖有效索引、重复 ID、缺失 evidence、链接索引错配、已完成实现缺少 commit。
- `python scripts/validate-source.py .`：通过；其中包含 Round4 状态索引检查。
- 原有手写说明仍保留；Q/R 账本与 `AUTONOMOUS_BACKLOG.md` 中的既有外部阻塞未被自动迁移或清除。

## 使用

更新任务状态和证据后执行：

```powershell
python scripts/render-round4-progress.py --write
python scripts/render-round4-progress.py --check
```

状态 JSON 是 Round4 64 项的机器数据源；`ROUND4_PROGRESS.md` 的标记区域为生成摘要/表格，区域外说明由维护者编辑。
