# S07-08 交付快照与退出条件

## 实现

- 新增 [`DELIVERY_AND_EXIT.md`](../../DELIVERY_AND_EXIT.md)，统一说明每批只做一次实现/证据/下一步快照、64 项计划的完成条件、无可执行项时的记录方式和计划外需求准入边界。
- 与 S07-02 的机器账本、S07-04 的 CI 结果记录规则和现有 autonomous backlog 对照；不新增另一份任务状态源。
- 明确本次用户授权已经覆盖完整 Round4 计划，`PROPOSED` 仅表示依赖/能力对照尚未完成，避免因当前小批阻塞而提前结束总目标。

## 验收

- `python scripts/render-round4-progress.py --check`：64 个 ID、状态摘要、证据路径和生成表格一致。
- `python scripts/validate-source.py .` 与 `git diff --check` 通过。
- 文档校验范围之外的产品构建、宿主或设备验证不适用于本流程文档。

## 边界

Round4 当前仍有大量任务未开始；本项只定义交付与退出方式，不改变任务顺序、依赖、外部权限或 S00-02 UI 门禁状态，也不把总目标标成完成。
