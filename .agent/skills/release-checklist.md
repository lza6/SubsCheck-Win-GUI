# Skill: release-checklist — SubsCheck-Win-GUI 发版检查单

## 适用场景
发布新版本（tag + GitHub Release）前。

## 前置读取
- `workflow_status.md`（本轮验证记录）
- `test_ledger.md`（接缝状态必须全 locked）
- `.agent/memory/verified-scope.md`

## 步骤
1. Release 构建：`MSBuild subs-check.win.gui.sln /p:Configuration=Release` → 0 error
2. 测试：`vstest.console.exe tests\SubsCheck.Tests\bin\Release\SubsCheck.Tests.dll /TestAdapterPath:"packages\MSTest.TestAdapter\build\_common"` → 全过
3. 冒烟：启动 bin\Release\subs-check.win.gui.exe，确认存活 + logs/subscheck-gui.log 生成
4. 检查 config.yaml 兼容：旧版配置能否被 ReadConfig 读取（凭据明文契约）
5. 原子提交（含 README/报告同步）→ push origin master（禁 force）
6. `git tag vX.Y.Z && git push origin vX.Y.Z`
7. `gh release create vX.Y.Z --notes ...`（含变更/验证/回滚三段）

## 验收标准
全部命令真实执行并留输出；release notes 与实际 diff 一致。

## 回滚
- 代码回滚：revert 对应 commit
- 版本回滚：`gh release delete` + `git push origin :refs/tags/X.Y.Z`（慎用，优先发补丁版）

## 常见坑
- 构建前先 Stop-Process 旧 exe（文件锁定，cerebrum Do-Not-Repeat）
- vstest 忘带 /TestAdapterPath 会报"没有可用测试"
- MSTest 包名目录无版本号后缀（-ExcludeVersion 安装）
