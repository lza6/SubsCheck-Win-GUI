# Verified Scope — SubsCheck-Win-GUI

## 已验证（截至 v2.1.0, 030b900）
- 构建：VS18 MSBuild Release 0 error（多轮）；sln 含测试项目
- 测试：vstest 14/14（Cron 3 / 解析器 5 / ConfigService 6）
- 冒烟：Release exe 启动存活 + logs/subscheck-gui.log INF 级别持久化 + 无孤儿内核
- 审查：两轮独立审查（988s 深审 + 聚焦复验），Verdict: Approve
- 发布：v2.1.0 tag + GitHub Release（含回滚说明）

## 命令
- 构建: `MSBuild subs-check.win.gui.sln /p:Configuration=Release`
- 测试: `vstest.console.exe tests\SubsCheck.Tests\bin\Release\SubsCheck.Tests.dll /TestAdapterPath:"packages\MSTest.TestAdapter\build\_common"`
- 冒烟: Start-Process bin\Release\subs-check.win.gui.exe + logs 检查

## 未验证（外部条件）
- 内核真实测速全流程（需订阅源）
- gist/R2/WebDAV 真实上传（需凭证）
- 8h 长稳
