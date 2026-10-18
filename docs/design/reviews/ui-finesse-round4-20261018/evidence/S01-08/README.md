# S01-08 视图迁移验收

实现提交：`d1005e3350238e1da9f9ad68a76b0b8dee01ac68`（`补齐视图配置迁移回退`）。

插件启动时会隔离处理视图配置加载错误：可解析的配置只移除不兼容的视图布局/密度字段，并保留其他设置；整份 JSON 损坏时使用安全默认值。恢复前先把原 `config.json` 复制到唯一备份文件，备份失败时不会自动覆盖原文件。删除或改名后不再认识的列键，以及没有可识别列键的旧列表，会恢复为当前表格默认显隐。旧密度与未来 schema 按安全默认值和忽略未知条目的规则处理。

## 受控验证

- SDK `8.0.423` Release 解决方案构建通过：0 warning / 0 error。诊断 ID：`20261018T110922448Z-da48bc13`；完整诊断摘要见 [`build-summary.json`](build-summary.json)。
- 五个隔离测试类合计 `21/21`：设置恢复 `2/2`、视图定义 `10/10`、表格配置行为 `3/3`、列显隐 `4/4`、R09 焦点环 `2/2`。TRX 与 SHA256 在 [`tests/`](tests/) 和 [`verification-identity.json`](verification-identity.json)。
- 构建产物 DLL/PDB 的 SHA256、MVID、程序集版本与源码哈希记录在 `verification-identity.json`，构建产物对应提交 `d1005e33`。
- `python scripts/validate-source.py .`、`git diff --check` 通过。WPF 静态审查为 0 errors / 26 warnings / 177 info；没有 XAML 或页面布局改动，因此未运行 Render QA。

## 验收边界

本次未运行真实 Playnite 宿主、安装门禁、真实设置文件保存/重载或双主题宿主呈现。用户提供的旧 R09 诊断 `20261018T012631286Z-e027df9d` 显示 `2,2,2,2` 对 `1,1,1,1`；其 summary、TRX 和测试 DLL/PDB 身份目前不可用。当前提交的隔离输出 R09 `2/2` 通过，不能据此判断旧运行实际加载了哪份源码或程序集。