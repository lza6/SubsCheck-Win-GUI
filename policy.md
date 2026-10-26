# Policy — SubsCheck-Win-GUI

## 覆盖率门禁
- 本项目为 C#/WinForms（.NET Framework 4.7.2），当前无测试工程。本次不新建测试工程（用户授权小修逻辑，禁止大幅重构）。
- 核心纯逻辑（密钥生成、架构选择、状态机）以定向构建 + 运行时冒烟验证。
- 后续 v1.5 计划建 Core 类库 + xUnit 覆盖纯逻辑 ≥80%。

## 网络安全和成本开关
- paid_api_real_call = false（本项目无付费 API）
- 不发起任何真实付费请求

## 任务分级
- trivial：改动文件 ≤2，不动外部接口/配置 schema
- standard：一般逻辑修复，需定向构建 + 冒烟
- complex：跨模块/核心链路（进程启停、配置读写），需影响分析
- high-risk：配置兼容（老用户 config）、凭据加密；默认先给方案+回滚

## 审查门禁
- 每个修改节点完成后独立自审（Critic 视角，从怀疑起点出发）
- 未运行验证命令不得标记 Done

## 证据门禁
- 所有"构建通过""修复完成"必须附实际执行命令与真实输出摘要
- 禁止把静态阅读说成运行验证

## 版本控制规则
- 默认不 push、不创建 tag/release（除非用户明确要求）
- 保护工作区已有未提交改动（UI 现代化成果 Form1.cs +126 行 / UI/Theme.cs）
- 不 force push、不 reset --hard
- 每完成一个逻辑修复一个原子 commit（用户已授权 commit）
