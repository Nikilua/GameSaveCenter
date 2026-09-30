# SDK 10 `field` 标识符编译修复（2026-09-30）

## 问题与修复

用户附带的 `dotnet build` 日志使用 .NET SDK `10.0.401` / C# 14.0。`SettingsConflictResolver.cs:148` 中的 LINQ 参数 `field` 与 C# 14 属性访问器中的 `field` 关键字冲突，编译器随后把表达式按合成属性后备字段解析，产生 `CS9273`、`CS9258` 和 `CS1061`。将参数改名为 `conflictField` 后，`DisplayName` 明确绑定到 `SettingsConflictField`。对应测试将冲突摘要断言改为精确字符串相等，覆盖字段名和草稿未写入语义。

## 验证

- 原始 SDK 10.0.401 日志：`SettingsConflictResolver.cs(148,81)` 的 `CS9273`，同一表达式伴随 `CS9258`，并在 `(148,96)` 报 `CS1061: 'string' does not contain a definition for 'DisplayName'`。
- 本机可用 SDK 为 .NET 9.0.302，未安装 SDK 10；因此没有宣称本机重放 C# 14 编译器。
- `./scripts/build.ps1 -Configuration Release -SkipTests`：Release solution 成功，0 warnings、0 errors；XAML 检查 24/24。`python scripts/validate-source.py` 与 `git diff --check` 通过。
- [定向行为 TRX](relevant-behavior-tests.trx)：7/7 passed、0 failed，其中 `R16SettingsConflictBehaviorTests` 3/3；同一批还覆盖了另外 4 条布局行为。测试源工作树以 `065b9b4b` 为基线并包含尚未提交的布局行距改动，结果仅用于所列行为，不伪称为 SDK 10 构建结果。

## 验证边界

此修改针对日志中明确的 C# 14 名称冲突。SDK 10.0.401 上的最终 GitHub Actions 结果尚未取得；本机 Release 编译使用 SDK 9.0.302。真实 Playnite 安装/运行没有作为该编译修复的验证依据。
