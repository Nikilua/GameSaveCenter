# S06-01 工作清单契约

日期：2026-10-21

## 实现范围

新增设置层契约 `WorklistDefinition`、`WorklistItemRecord` 和 `WorklistObjectKind`。清单有 GUID、名称和稳定对象引用集合；每项只存对象种类、稳定对象 ID、人工备注和 `IsManuallyComplete`。对象类型先覆盖备份版本、媒体项、后台任务和校验发现。父级 ID 未包含在各源对象 ID 中时，调用方须将其编入 `StableObjectId`。

归一化约束：最多 20 个清单；名称最多 80 字符；每清单最多 200 项；稳定对象 ID 最多 256 字符；备注最多 500 字符。枚举、空或过长 ID 的条目丢弃，引用按“对象种类 + 稳定 ID”（忽略大小写）去重；备注和名称先去首尾空白，备注超长截断，稳定 ID 超长则整项丢弃以免改变身份。列表及条目都支持独立克隆。

该契约不含运行时 DTO、文件路径/内容、认证参数、命令参数、Worker 预览句柄或操作授权。本项不把清单接入设置保存、界面入口、导航或业务命令；由 S06-02 起按计划接入。

## 验证

- SDK 9.0.302、Playnite.Tests Release 项目构建：成功，0 warning、0 error；编译 Contracts、Core、Playnite `net462`、HostAuditSeeder 和 Playnite.Tests `net472`。不是全解决方案构建。
- `WorklistDefinitionTests`：4/4 通过。覆盖按类型去重与完成状态保留、20/200/80/256/500 数量和长度边界、无效 ID/类型拒绝、重复清单 ID 规范化及深克隆。
- 源码结构/性能护栏校验通过；本项没有 XAML/UI 改动，也没有真实宿主验收需求。

构建日志：[Playnite.Tests Release build](build/playnite-release-build.txt)。测试原始记录：[控制台](tests/worklist-tests-console.txt)、[TRX](tests/s06-01-worklist.trx)。

## 身份与边界

本机 `global.json` 固定 SDK 8.0.423 未安装；使用 SDK 9.0.302，从系统临时目录执行以遵守 `rollForward:disable`，未修改 `global.json`。源码/测试由受控设置模型验证；设置持久化兼容性、UIA、真实 Playnite 及跨会话保存留给后续 S06 项。

SHA-256：

| 文件/产物 | SHA-256 |
| --- | --- |
| `build/playnite-release-build.txt` | `A7115968A3FD3D0FB35A55A7B5657BAD7B0001B02F5857B729761B5F33FE1090` |
| `tests/worklist-tests-console.txt` | `2AE66B1CBB204E30392129D3E313DDE7F3C229B1C777574EF332CFEEAEE5C369` |
| `tests/s06-01-worklist.trx` | `23797BCA76D9E3A6720D4BE240134AE8DC307B2F44D4E629DAF3855333101584` |
| `GameSaveCenter.Playnite.dll` | `35D1FE7C09F8D44ACE45DB8448B4DD4101D83489F4C1B10C8C65EBC53F15E3F3` |
| `GameSaveCenter.Playnite.Tests.dll` | `5C60282D0F66FDA61C044FBC052F62EA5430E1B15F7BAC13D6664CF9F7A13555` |
| `GameSaveCenter.Playnite.pdb` | `DD7E55F5BC9AFD40F61E222965E950A7B1A4609C21DDE713F6E18488245D4424` |
| `GameSaveCenter.Playnite.Tests.pdb` | `DC3135976B3A0DE9633962133E95E30243B5D78816E727D21B3661DF77389FF2` |
