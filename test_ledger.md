# Test Ledger — SubsCheck-Win-GUI

## Seam Registry

| 接缝 | 模块A | 模块B | 契约描述 | 最后验证时间 | 状态 | 关联证据 |
|---|---|---|---|---|---|---|
| S1 GUI→config.yaml | Form1.SaveConfig | subs-check.exe 外部进程 | config.yaml 键名与格式（listen-port / api-key / sub-store-port） | 2026-10-26 | locked | evidence/T-001.json |
| S2 GUI→subs-check.exe | Form1 进程管理 | subs-check.exe | 启停、命令行参数、output/all.yaml 产出 | 2026-10-26 | locked | evidence/T-004.json |
| S3 GUI→WebUI API | Form1 转发 | subs-check WebUI | X-API-Key 头 = config api-key | 2026-10-26 | locked | evidence/T-002.json |
| S4 GUI→GitHub Release | Form1.DownloadSubsCheckEXE | beck-8/subs-check releases | zip 文件名含架构标识 | 2026-10-26 | locked | evidence/T-003.json |
| S5 配置读取 | Form1.ReadConfig | config.yaml | 键名兼容旧版明文凭据 | 2026-10-26 | locked | evidence/T-002.json |

## Test Status

| 模块/节点 | 测试类型 | 状态 | 最后通过时间 | 关联改动 | 备注 |
|---|---|---|---|---|---|
| 全解决方案构建（VS MSBuild） | build | passed | 2026-10-26 | 基线 | Release 0 error |
| 全解决方案构建（Framework MSBuild） | build | failed→修复中 | 2026-10-26 | csproj LangVersion | C#5 默认，见 bug-004 |
| 启动冒烟（进程存活） | smoke | passed | 2026-10-26 | UI 现代化 | UIA 树正常 |
