# 记忆 — SubsCheck-Win-GUI

## audit-history.md
- 2026-10-26 三轮终局审计：基线（P0/P1 清单）→ 修复（cda4ab1）→ 独立审查发现 DPAPI 跨进程回归（22ed3b3）→ 复验 R2/R5（08365bb）→ 审查闭环。审计范围：Form1.cs 密钥/凭据/状态机链路、bat 构建链、config.yaml 跨进程契约、UI 主题。

## verified-scope.md
- 构建：VS18 MSBuild Release Rebuild 0 error（多轮）
- 冒烟：进程存活 + 窗口正常（PID 20528/30272/35160）
- 随机密钥链路：BM_CLICK → 日志 + 掩码填充（截图）
- CryptoHelper.GenerateApiKey：15 项断言 PASS
- 未验证（待外部条件）：gist/R2/WebDAV 真实上传、内核 x64 zip 下载、8h 长稳

## known-issues.md
- P1-1：16 处 new HttpClient 未复用（backlog，中型重构）
- 既有 Designer 布局：「Clash订阅」按钮与密钥框重叠
- Framework MSBuild C#5 上限：bat 已规避，根治需 LangVersion+Roslyn

## decisions.md
- 撤回 DPAPI 加密：config.yaml 是 GUI↔内核共享契约，凭据必须明文（跨进程兼容 > 本地加密）
- 不做 csproj LangVersion 升级：违反"禁止大幅重构"，bat 规避即可
- 状态机最小化：enum + 属性收口，不引入抽象层

## B1-B6 批次闭环（2026-10-26 晚）
- 6 个 Service 抽离 + MSTest 12 用例全过 + Release 构建 + 冒烟（logs/ 持久化验证）
- 审查修复：未知行告警去噪（聚合计数、仅提示一次）；删除 Form1 死方法 RemoveAnsiEscapeCodes
- 计划文档「参考的结果计划指南.md」已归档删除（manifest 增补）
- 证据：evidence/T-100.json
- 坑：MSTest vstest 必须带 /TestAdapterPath:packages\MSTest.TestAdapter\build\_common
