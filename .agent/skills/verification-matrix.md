# Skill: verification-matrix — SubsCheck-Win-GUI 验证矩阵

## 适用场景
任何代码改动后的验证；判断"改到什么程度要跑什么"。

## 前置读取
- `test_ledger.md`（接缝状态）
- `.agent/memory/verified-scope.md`（避免重复验证）

## 分层规则

| 改动 | 必跑 | 说明 |
|---|---|---|
| Services/*.cs 纯逻辑 | vstest 全量 + Release 构建 | 单测即接缝测试（服务层有直接单测） |
| Form1.cs UI 事件/布局 | 构建 + 冒烟 + 截图（PrintWindow） | cerebrum 规则：UI 必须截图验证 |
| Form1.cs OutputDataReceived/进程链路 | 构建 + 冒烟 + 未知行告警日志检查 | 触达 S2 |
| ConfigService / SaveConfig / ReadConfig | vstest（Config_* 6 例）+ 构建 | 触达 S1/S5；必须验证 removeKeys 语义 |
| LogService | 构建 + 启动日志文件检查 | 触达 logs/ 持久化 |
| ProcessSupervisor | 构建 + 冒烟（进程存活） | 强杀链路无法注入实测，签名审查代替 |
| CheckUpdates / 下载链路 | 构建（真实下载需网络条件，标边界） | S4 |

## 验收标准
- 每项声明附真实命令输出（evidence/*.json）
- 无法实测项写入 limitations，禁止标 passed

## 回滚
验证失败 → 修复前先 `git stash` 或记录 diff，保证可回退。

## 常见坑
- 单测通过 ≠ 接缝安全：Form1 接线处（如 removeKeys 传参）必须人工核对调用点
- bin\Release 下冒烟会读用户真实 config——如改过配置格式，先备份
