# Test Ledger — SubsCheck-Win-GUI

## Seam Registry

| 接缝 | 模块A | 模块B | 契约描述 | 最后验证时间 | 状态 | 关联证据 |
|---|---|---|---|---|---|---|
| S1 GUI→config.yaml | Form1.SaveConfig + ConfigService | subs-check.exe 外部进程 | config.yaml 键名与格式（读-改-写保留手写字段 + removeKeys 条件删除 + 原子写） | 2026-10-26 v2.1.0 | locked | evidence/T-100.json + 单测 Config_Save_RemoveKeys 等 6 例 |
| S2 GUI→subs-check.exe | Form1 进程管理 + ProcessSupervisor(Job Object) | subs-check.exe | 启停、输出解析(SubsCheckOutputParser)、孤儿进程防护 | 2026-10-26 v2.1.0 | locked | evidence/T-100.json（解析单测 5 例+冒烟） |
| S3 GUI→WebUI API | Form1 转发 | subs-check WebUI | X-API-Key 头 = config api-key | 2026-10-26 | locked | evidence/T-002.json |
| S4 GUI→GitHub Release | Form1.DownloadSubsCheckEXE | beck-8/subs-check releases | zip 文件名含架构标识 | 2026-10-26 | locked | evidence/T-003.json |
| S5 配置读取 | Form1.ReadConfig + ConfigService.Load | config.yaml | 键名兼容旧版明文凭据 | 2026-10-26 v2.1.0 | locked | evidence/T-100.json |

## Test Status

| 模块/节点 | 测试类型 | 状态 | 最后通过时间 | 关联改动 | 备注 |
|---|---|---|---|---|---|
| 全解决方案构建（VS MSBuild） | build | passed | 2026-10-26 | 基线 | Release 0 error |
| 全解决方案构建（Framework MSBuild） | build | blocked（工具链限制） | 2026-10-26 | csproj LangVersion | C#5 上限，bat 走 VS MSBuild 规避（决策见 decisions） |
| 启动冒烟（进程存活） | smoke | passed | 2026-10-26 | UI 现代化 | UIA 树正常 |

| Services 五模块 | unit | passed | 2026-10-26 v2.1.0 | B1-B6 | vstest 14/14 |
| 未知行告警链路 | unit+smoke | passed | 2026-10-26 v2.1.0 | B2+审查 R3 | 计数复位+WRN 落文件 |
