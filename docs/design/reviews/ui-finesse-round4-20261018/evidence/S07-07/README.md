# S07-07 环境能力准入缓存

## 实现

- 新增 [`ENVIRONMENT_CAPABILITY_CACHE.json`](../../../../../ai/ENVIRONMENT_CAPABILITY_CACHE.json)，集中登记 S00-02 的精确 SDK 缺失、尚未运行的真实 Playnite 门禁、旧 Q/R 的 CEF/ETW 权限阻塞，以及 S07-03 的 GitHub branch protection 读取权限阻塞。
- 每项分别保留状态、已知检查日期/方式/结果、证据路径和允许重试条件。历史检查日期未知时留空；未检查项不伪造检查结果。
- 新增 `scripts/validate-environment-capability-cache.py`，校验唯一 ID、状态、重试条件、日期与仓库内证据路径；source validation 自动调用。
- 没有重启因 SDK 版本失败的宿主流程，也没有重试 CEF 或 ETW 权限探测。用户明确要求重试时仍须满足对应门禁与授权条件。

## 验收

- `python -m unittest discover -s scripts/tests -p test_environment_capability_cache.py`：7 项通过，覆盖有效状态、重复 ID、未知状态、缺失检查状态/证据、错误标记未检查与路径越界。
- `python scripts/validate-environment-capability-cache.py`：5 项能力记录与证据路径有效（包含后续 S07-03 API 权限审查）。
- `python scripts/validate-source.py .`、`python scripts/render-round4-progress.py --check`、`git diff --check` 通过。

## 边界

此缓存仅管理环境准入和重试条件，不代表 SDK、Playnite、CEF 或 ETW 现在可用，不会自动探测或更改权限。S00-02 的真实 Playnite UI 门禁仍未通过；旧 Q/R 环境阻塞仍独立保留。
