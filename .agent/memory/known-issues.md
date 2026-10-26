# Known Issues — SubsCheck-Win-GUI

| ID | 级别 | 问题 | 状态 | 备注 |
|---|---|---|---|---|
| KI-1 | P2 | 16 处 new HttpClient 未复用（socket 耗尽风险） | backlog | 中型重构，各 handler 参数各异；v2.0.2 轮评估后搁置 |
| KI-2 | P2 | 「Clash订阅」按钮与密钥输入框视觉重叠 | backlog | Designer 布局，需 VS 设计器处理 |
| KI-3 | P3 | Framework MSBuild C#5 上限 | 规避中 | bat 走 VS MSBuild；根治需 LangVersion+Roslyn |
| KI-4 | P3 | ANSI 正则未覆盖光标定位/OSC 序列 | backlog | 审查 S1 建议，当前内核输出无此序列 |
| KI-5 | P3 | LogService.Init 前的日志静默丢弃 | backlog | 审查 S2 建议，可 lazy-init |
| KI-6 | 边界 | 覆盖率未测（.NET Fx 4.7.2 无 coverlet） | 已声明 | 需 Core 类库迁移后可测 |
| KI-7 | 边界 | Job Object 强杀链路未注入式实测 | 已声明 | P/Invoke 签名经独立审查核对 |
| KI-8 | P2 | UI 保存 config.yaml 时 YAML 注释丢失（字典往返固有限制） | 已文档化 | README 已注；根治需注释保留型读写（大改，未授权） |
| KI-9 | P3 | ANSI 正则不覆盖 ?/2J 等带参数序列（残留乱码仅显示） | backlog | 匹配走原始行不受影响 |
| KI-10 | P3 | 双实例日志轮转跨进程失效（文件占用时 2MB 上限失效） | 边界 | 单实例为主场景 |
