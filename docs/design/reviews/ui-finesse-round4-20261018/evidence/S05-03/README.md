# S05-03 估算不确定性

## 等价能力审计

`StorageAnalysisService` 已提供近 7/30/90 天新增逻辑体积和本地卷可用空间，但旧 `BuildPrediction` 只按近 30 天新增量给出单一“几个月后达到磁盘 90%”结论；它不检查样本覆盖，也把索引逻辑增长直接映射成物理磁盘写入。存储 DTO 没有远端容量/配额数据。S05-01/02 已有增长假设输入和回显，但比较结果明确表示该假设不参与规划；输入草稿本身没有供容量视图即时读取的显示值。因此现有统计与假设不足以满足 S05-03。

## 实现行为

- Worker 只从近 90 天的版本记录计算样本范围：至少覆盖 30 天并包含 3 个有新增的 UTC 日期，才展示观察范围；只使用样本覆盖到的 7/30/90 天滚动窗口，将各窗口逻辑新增量折算到每 30 天，取最小至最大作为历史观察包络。
- 区间明确标为观察范围、不是置信区间。样本不足时区间图隐藏并显示未知及覆盖日期数；不再给出确定的磁盘耗尽月份/日期。
- 说明历史逻辑新增量不能直接换算磁盘实占，因为压缩、保留和其他进程写入没有建模。本地卷总量/剩余空间仍由 `DriveInfo` 单独提供；远端可用容量明确显示未知，因为当前没有读取服务端配额。
- 维护页单独显示历史统计、当前会话的用户增长假设、本地磁盘剩余与远端未知状态。增长输入变化时假设显示即时更新；未新增设置持久化、清理或远端写入路径。

## 验证

- SDK `8.0.423`，Release solution build `0 warning / 0 error`，XAML 结构检查 `25/25`。诊断 `20261020T132742784Z-b6610f70`；构建日志见 [build-console.txt](build/build-console.txt)，SHA-256 `3CCE5DC44493A69A2FE5C64B58ED7EBE6C618BD1ED65671747BD07D30E5A1A9D`；机器摘要见 [summary.json](build/summary.json)，SHA-256 `BBCE2B926FBB6377162C81B679864496E6903A21DC8521B8A37DA478A3C44E85`。构建源提交为 `8c76e28e`，构建对应其后的 S05-03 工作树修改。
- Worker `StorageAnalysisServiceTests` `5/5`，覆盖基本统计、目录不可用、样本不足时的未知提示、样本充足时的滚动历史范围及不输出耗尽日期：[TRX](tests/s05-03-storage-analysis.trx)，SHA-256 `0A1C71CDF3FB1BA8C6623D2A2406D450A12B4C82EE7791BF0ACE8FC753EE0E5B`；Worker 测试程序集 SHA-256 `DADB58CFC02792AE594CA717109565FC49C35F9539D66573A3225801ED185D18`。
- 生产 `MaintenanceView` Light/Dark 行为 `2/2`：验证用户假设实时绑定、历史范围图端点、本地容量、远端未知和容量判断文本：[TRX](tests/s05-03-maintenance-storage-ui.trx)，SHA-256 `565AE9A3A43A7B9CEA7DEAFF7A134F3498EDC9442C22B828DCA188D7C9D959E1`；Playnite 测试程序集 SHA-256 `D4B3AD7B4999DACD7FCA30A68FBB4FAB2198AA780F0B971EA3CB2132030717ED`。
- `validate-source.py`、XAML 结构检查和 `git diff --check` 通过；源码校验输出见 [source-validation.txt](validation/source-validation.txt)。没有因本项重复记录或触发 CI。

没有安装或运行真实 Playnite 窗口、完整 WPF 独立循环或物理 DPI 验收；远端容量保持未知。Light/Dark 结果是生产 WPF 控件的受控行为测试，不是屏幕截图证据。历史增长区间也不声称统计置信度或磁盘耗尽预测。
