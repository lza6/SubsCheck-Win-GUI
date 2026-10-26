using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using subs_check.win.gui;

namespace SubsCheck.Tests
{
    /// <summary>B4 测试基线：CronService / SubsCheckOutputParser / ConfigService 纯逻辑回归。</summary>
    [TestClass]
    public class CoreLogicTests
    {
        // ---------- CronService ----------

        [TestMethod]
        public void Cron_Valid_ShouldPass()
        {
            Assert.IsTrue(CronService.IsValid("*/30 * * * *"));
            Assert.IsTrue(CronService.IsValid("0 */2 * * *"));
            Assert.IsTrue(CronService.IsValid("0 0 */2 * *"));
            Assert.IsTrue(CronService.IsValid("0 0 1 * *"));
            Assert.IsTrue(CronService.IsValid("30 8 * * 1-5"));
            Assert.IsTrue(CronService.IsValid("0 0 1,15 * *"));
        }

        [TestMethod]
        public void Cron_Invalid_ShouldFail()
        {
            Assert.IsFalse(CronService.IsValid(""));
            Assert.IsFalse(CronService.IsValid(null));
            Assert.IsFalse(CronService.IsValid("* * * *"));      // 只有4段
            Assert.IsFalse(CronService.IsValid("* * * * * *"));  // 6段
            Assert.IsFalse(CronService.IsValid("60 * * * *"));   // 分钟越界
            Assert.IsFalse(CronService.IsValid("* 24 * * *"));   // 小时越界
            Assert.IsFalse(CronService.IsValid("* * 32 * *"));   // 日越界
            Assert.IsFalse(CronService.IsValid("* * * 13 *"));   // 月越界
            Assert.IsFalse(CronService.IsValid("* * * * 8"));    // 星期越界
            Assert.IsFalse(CronService.IsValid("*/0 * * * *"));  // 步长为0
        }

        [TestMethod]
        public void Cron_Describe_KnownOutputs()
        {
            // 与旧版 GetCronExpressionDescription 输出保持一致（回归基线；旧版对 * 会拼接"每天每小时"）
            Assert.AreEqual("执行时间: 每天每小时每30分钟", CronService.Describe("*/30 * * * *"));
            StringAssert.Contains(CronService.Describe("0 8 * * 1"), "星期一");
            StringAssert.Contains(CronService.Describe("0 8 * * 1"), "8小时");
        }

        // ---------- SubsCheckOutputParser ----------

        [TestMethod]
        public void Parser_ProgressLine_ShouldExtractAllFields()
        {
            var parser = new SubsCheckOutputParser();
            var r = parser.Parse("检测中 进度: [ 45%] (12/6167) 可用: 3");
            Assert.IsTrue(r.IsProgressLine);
            Assert.AreEqual(45.0, r.Percent.Value, 0.01);
            Assert.AreEqual(12, r.CurrentCount.Value);
            Assert.AreEqual(6167, r.TotalCount.Value);
            Assert.AreEqual(3, r.AvailableCount.Value);
            Assert.IsTrue(r.IsKnown);
        }

        [TestMethod]
        public void Parser_NextCheckTime_ShouldExtract()
        {
            var parser = new SubsCheckOutputParser();
            var r = parser.Parse("\x1B[92m下次检查时间: 2026-01-01 08:00:00\x1B[0m");
            Assert.IsFalse(r.IsProgressLine);
            Assert.IsTrue(r.HasNextCheckTime);
            StringAssert.StartsWith(r.NextCheckTimeText, "下次检查时间:");
        }

        [TestMethod]
        public void Parser_GinLog_ShouldBeFiltered()
        {
            var parser = new SubsCheckOutputParser();
            var r = parser.Parse("[GIN] 2026/01/01 - 200 | 1ms | GET /all.yaml");
            Assert.IsTrue(r.IsGinLog);
            Assert.IsTrue(r.IsKnown);
        }

        [TestMethod]
        public void Parser_UnknownLine_ShouldBeFlagged()
        {
            var parser = new SubsCheckOutputParser();
            var r = parser.Parse(" completely new kernel message format v9 ");
            Assert.IsFalse(r.IsKnown); // 未知行告警信号
        }

        [TestMethod]
        public void Parser_AnsiRemoval()
        {
            Assert.AreEqual("hello",
                SubsCheckOutputParser.RemoveAnsiEscapeCodes("\x1B[92mhel\x1B[0mlo"));
        }

        // ---------- ConfigService ----------

        [TestMethod]
        public void Config_ReadModifyWrite_PreservesUnknownFields()
        {
            string dir = Path.Combine(Path.GetTempPath(), "scwg_test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                string path = Path.Combine(dir, "config.yaml");
                // 含 UI 未管理的"手写字段" custom-field
                File.WriteAllText(path, "concurrent: 4\ncustom-field: keepme\n", Encoding.UTF8);

                var updates = new Dictionary<string, object> { { "concurrent", 8 } };
                ConfigService.Save(path, updates, null);

                var loaded = ConfigService.Load(path);
                Assert.AreEqual("8", ConfigService.GetString(loaded, "concurrent"));
                Assert.AreEqual("keepme", ConfigService.GetString(loaded, "custom-field")); // 未被覆盖丢失
            }
            finally { Directory.Delete(dir, true); }
        }

        [TestMethod]
        public void Config_MoreYaml_Appended()
        {
            string dir = Path.Combine(Path.GetTempPath(), "scwg_test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                string path = Path.Combine(dir, "config.yaml");
                ConfigService.Save(path,
                    new Dictionary<string, object> { { "timeout", 2 } },
                    "# more params\nextra: true");

                string content = File.ReadAllText(path);
                StringAssert.Contains(content, "extra: true");
                StringAssert.Contains(content, "timeout: 2");
            }
            finally { Directory.Delete(dir, true); }
        }

        [TestMethod]
        public void Config_Getters()
        {
            var config = new Dictionary<string, object> {
                { "n", 5 }, { "s", "abc" },
                { "list", new List<object> { "a", "b" } }
            };
            Assert.AreEqual(5, ConfigService.GetInt(config, "n").Value);
            Assert.IsNull(ConfigService.GetInt(config, "missing"));
            Assert.AreEqual("abc", ConfigService.GetString(config, "s"));
            CollectionAssert.AreEqual(new List<string> { "a", "b" }, ConfigService.GetList(config, "list"));
        }

        [TestMethod]
        public void Config_AtomicWrite_NoTmpLeftover()
        {
            string dir = Path.Combine(Path.GetTempPath(), "scwg_test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                string path = Path.Combine(dir, "x.yaml");
                ConfigService.AtomicWrite(path, "v1");
                ConfigService.AtomicWrite(path, "v2");
                Assert.AreEqual("v2", File.ReadAllText(path));
                Assert.IsFalse(File.Exists(path + ".tmp"));
            }
            finally { Directory.Delete(dir, true); }
        }
    }
}
