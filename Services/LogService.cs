using System;
using System.IO;
using System.Text;

namespace subs_check.win.gui
{
    /// <summary>
    /// 日志服务：级别化 + 文件持久化 + 按大小轮转。
    /// 零依赖实现（不引入日志库），保持便携形态。
    /// 线程安全：所有写入经 lock 串行化。
    /// </summary>
    public static class LogService
    {
        public enum LogLevel { Debug = 0, Info = 1, Warn = 2, Error = 3 }

        private static readonly object lockObj = new object();
        private static string logDirPath;
        private const long MaxFileSizeBytes = 2 * 1024 * 1024; // 单文件 2MB
        private const int MaxBackupFiles = 3;                   // 保留 3 个历史

        /// <summary>日志目录初始化失败时触发（供 UI 提示——日志静默消失会导致排障链路断裂）。参数为失败原因。</summary>
        public static event Action<string> InitFailed;

        /// <summary>初始化日志目录（exe 同级 logs/）。失败时降级为仅 UI 输出并触发 InitFailed。</summary>
        public static void Init()
        {
            try
            {
                string executablePath = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
                logDirPath = Path.Combine(executablePath, "logs");
                if (!Directory.Exists(logDirPath)) Directory.CreateDirectory(logDirPath);
            }
            catch (Exception ex)
            {
                logDirPath = null; // 磁盘不可写等情况：放弃文件持久化
                var handler = InitFailed;
                if (handler != null)
                {
                    try { handler($"日志文件持久化不可用（{ex.Message}），仅保留界面日志显示。"); } catch { }
                }
            }
        }

        public static void Write(string message, bool isError = false)
        {
            Write(isError ? LogLevel.Error : LogLevel.Info, message);
        }

        public static void Write(LogLevel level, string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string logType = LevelTag(level);
            string[] lines = message.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var output = new StringBuilder();
            foreach (string line in lines)
            {
                if (!string.IsNullOrEmpty(line))
                    output.AppendLine($"{timestamp} {logType} {line}");
            }
            AppendToFile(output.ToString());
        }

        private static string LevelTag(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug: return "DBG";
                case LogLevel.Warn: return "WRN";
                case LogLevel.Error: return "ERR";
                default: return "INF";
            }
        }

        private static void AppendToFile(string text)
        {
            if (logDirPath == null) return;
            try
            {
                lock (lockObj)
                {
                    string filePath = Path.Combine(logDirPath, "subscheck-gui.log");
                    RotateIfNeeded(filePath);
                    File.AppendAllText(filePath, text, Encoding.UTF8);
                }
            }
            catch
            {
                // 日志写入失败不影响主功能（静默降级）
            }
        }

        /// <summary>超过 MaxFileSizeBytes 时轮转：log → log.1 → log.2 → log.3（丢弃最旧）。</summary>
        private static void RotateIfNeeded(string filePath)
        {
            try
            {
                if (!File.Exists(filePath) || new FileInfo(filePath).Length < MaxFileSizeBytes) return;

                for (int i = MaxBackupFiles - 1; i >= 1; i--)
                {
                    string src = filePath + "." + i;
                    string dst = filePath + "." + (i + 1);
                    if (File.Exists(src)) File.Copy(src, dst, true);
                }
                File.Copy(filePath, filePath + ".1", true);
                File.Delete(filePath);
            }
            catch
            {
                // 轮转失败继续追加（下次再试）
            }
        }
    }
}
