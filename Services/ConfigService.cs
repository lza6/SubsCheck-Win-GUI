using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;

namespace subs_check.win.gui
{
    /// <summary>
    /// 配置读写服务：解析 config.yaml 为字典、按字段更新、原子写回。
    /// 核心改进：读-改-写保留 UI 未管理的手写字段（旧实现整体重写会覆盖丢失）。
    /// 原子写：先写临时文件再 File.Replace/Move，避免写一半被进程读到残缺配置。
    /// </summary>
    public static class ConfigService
    {
        /// <summary>反序列化 config.yaml 为字典。文件不存在返回空字典。</summary>
        public static Dictionary<string, object> Load(string configFilePath)
        {
            if (!File.Exists(configFilePath)) return new Dictionary<string, object>();
            string yamlContent = File.ReadAllText(configFilePath);
            var deserializer = new Deserializer();
            return deserializer.Deserialize<Dictionary<string, object>>(yamlContent)
                   ?? new Dictionary<string, object>();
        }

        private static readonly object saveLock = new object();

        /// <summary>
        /// 读-改-写：在现有配置基础上更新 updates 中的字段，未提及的字段原样保留。
        /// removeKeys 中的键将被删除（用于 UI 条件字段：如关闭 cron 模式后 cron-expression 必须移除，
        /// 否则外部内核会按残留的过期表达式继续调度——旧行为是整体重写时键自然消失）。
        /// more.yaml 追加逻辑与旧版保持一致（由调用方传入其内容，可为 null）。
        /// 线程安全：全程持 saveLock + 唯一临时文件名，防并发 Save 交错。
        /// </summary>
        public static void Save(string configFilePath, Dictionary<string, object> updates, string moreYamlContent, IEnumerable<string> removeKeys = null)
        {
            lock (saveLock)
            {
                // 1. 读现有配置（保留手写字段）
                var config = Load(configFilePath);

                // 2. 删除本次不应存在的条件字段（C1 修复）
                if (removeKeys != null)
                    foreach (string key in removeKeys)
                        config.Remove(key);

                // 3. 覆盖 UI 管理的字段
                foreach (var kv in updates)
                    config[kv.Key] = kv.Value;

                // 4. 序列化
                var serializer = new SerializerBuilder()
                    .WithIndentedSequences()
                    .Build();
                string yamlContent = serializer.Serialize(config);

                // 5. 追加 more.yaml
                if (!string.IsNullOrEmpty(moreYamlContent))
                {
                    if (!moreYamlContent.StartsWith("\n") && !moreYamlContent.StartsWith("\r\n"))
                        yamlContent += "\n";
                    yamlContent += moreYamlContent;
                }

                // 6. 原子写
                AtomicWrite(configFilePath, yamlContent);
            }
        }

        /// <summary>原子写：写临时文件 → Replace/Move 覆盖目标，避免残缺配置。</summary>
        public static void AtomicWrite(string targetPath, string content)
        {
            string dir = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            // R2 修复：唯一临时文件名，防并发写冲突
            string tmpPath = targetPath + ".tmp." + Guid.NewGuid().ToString("N");
            File.WriteAllText(tmpPath, content);

            try
            {
                if (File.Exists(targetPath))
                    File.Replace(tmpPath, targetPath, null);
                else
                    File.Move(tmpPath, targetPath);
            }
            catch (Exception ex)
            {
                // R1 修复：tmp 未落盘时保留原始异常，不误导为"找不到 tmp"
                if (!File.Exists(tmpPath)) throw;

                try
                {
                    // File.Replace 在个别文件系统（FAT）不支持，降级为直接覆盖
                    File.Copy(tmpPath, targetPath, true);
                    File.Delete(tmpPath);
                }
                catch (Exception ex2)
                {
                    throw new IOException($"原子写降级覆盖失败（原始原因: {ex.Message}）", ex2);
                }
            }
        }

        // ---- 字段读取辅助（与 Form1 内旧方法语义一致，供测试与复用）----

        public static int? GetInt(Dictionary<string, object> config, string field)
        {
            object v;
            if (config.TryGetValue(field, out v) && v != null)
            {
                int value;
                if (int.TryParse(v.ToString(), out value)) return value;
            }
            return null;
        }

        public static string GetString(Dictionary<string, object> config, string field)
        {
            object v;
            if (config.TryGetValue(field, out v) && v != null) return v.ToString();
            return null;
        }

        public static List<string> GetList(Dictionary<string, object> config, string field)
        {
            object v;
            if (config.TryGetValue(field, out v) && v != null)
            {
                if (v is List<object> listItems)
                    return listItems.Select(item => item?.ToString()).Where(s => !string.IsNullOrEmpty(s)).ToList();
            }
            return null;
        }
    }
}
