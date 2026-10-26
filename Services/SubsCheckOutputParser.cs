using System;
using System.Text.RegularExpressions;

namespace subs_check.win.gui
{
    /// <summary>
    /// subs-check.exe stdout 输出解析器。
    /// 将"进度行/下次检查时间/ANSI 转义/GIN 日志"等关键字解析从 Form1 中抽离，
    /// 使其可独立测试，并在内核文案变更时集中告警（未知行事件）。
    /// </summary>
    public class SubsCheckOutputParser
    {
        /// <summary>一次解析的结构化结果。</summary>
        public class ParseResult
        {
            public bool IsProgressLine;     // "进度: [xx%] (12/6167) 可用: 0"
            public double? Percent;         // 进度百分比
            public int? CurrentCount;       // 已检测节点数
            public int? TotalCount;         // 总节点数
            public int? AvailableCount;     // 可用节点数
            public bool HasNextCheckTime;   // 含"下次检查时间:"
            public string NextCheckTimeText;// "下次检查时间: ..." 完整片段
            public bool IsGinLog;           // GIN 框架日志（应过滤）
            public bool IsKnown;            // 是否为已知模式（false 时触发未知行告警）
            public string CleanText;        // 去 ANSI 后的文本
        }

        // 已知的关键字模式（subs-check 内核文案）。新增文案时在此登记。
        private static readonly string[] KnownMarkers =
        {
            "进度: [", "下次检查时间:", "获取订阅链接错误跳过", "[GIN]"
        };

        /// <summary>解析一行内核输出。输入可为原始（含 ANSI）文本。</summary>
        public ParseResult Parse(string raw)
        {
            var r = new ParseResult();
            if (string.IsNullOrEmpty(raw))
            {
                r.IsKnown = true; // 空行不算未知
                return r;
            }

            r.CleanText = RemoveAnsiEscapeCodes(raw);

            if (raw.Contains("进度: ["))
            {
                r.IsProgressLine = true;
                r.IsKnown = true;
                ParseProgress(r, raw);
                return r;
            }

            if (r.CleanText.Contains("下次检查时间:"))
            {
                r.HasNextCheckTime = true;
                r.IsKnown = true;
                int startIndex = r.CleanText.IndexOf("下次检查时间:");
                r.NextCheckTimeText = r.CleanText.Substring(startIndex);
            }

            if (r.CleanText.StartsWith("[GIN]"))
            {
                r.IsGinLog = true;
                r.IsKnown = true;
                return r;
            }

            if (raw.Contains("获取订阅链接错误跳过"))
            {
                r.IsKnown = true;
                return r;
            }

            // 均不匹配 → 未知行（调用方可告警/上报，用于发现内核文案变更）
            r.IsKnown = false;
            return r;
        }

        /// <summary>该行是否包含任何已知关键字（用于告警判定）。</summary>
        public static bool ContainsKnownMarker(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return true;
            foreach (string m in KnownMarkers)
                if (raw.Contains(m)) return true;
            return false;
        }

        private static void ParseProgress(ParseResult r, string raw)
        {
            int percentIndex = raw.IndexOf('%');
            if (percentIndex > 0)
            {
                int startIndex = raw.LastIndexOfAny(new char[] { ' ', '>' }, percentIndex) + 1;
                string percentText = raw.Substring(startIndex, percentIndex - startIndex);
                double percent;
                if (double.TryParse(percentText, out percent)) r.Percent = percent;
            }

            int infoStartIndex = raw.IndexOf('(');
            if (infoStartIndex > 0)
            {
                string fullNodeInfo = raw.Substring(infoStartIndex);
                Match match = Regex.Match(fullNodeInfo, @"\((\d+)/(\d+)\)");
                if (match.Success)
                {
                    int currentCount, totalCount;
                    if (int.TryParse(match.Groups[1].Value, out currentCount) &&
                        int.TryParse(match.Groups[2].Value, out totalCount))
                    {
                        r.CurrentCount = currentCount;
                        r.TotalCount = totalCount;
                    }
                }

                int availableIndex = fullNodeInfo.IndexOf("可用:");
                if (availableIndex > 0)
                {
                    string availableText = fullNodeInfo.Substring(availableIndex + 3);
                    match = Regex.Match(availableText, @"(\d+)");
                    int availableCount;
                    if (match.Success && int.TryParse(match.Groups[1].Value, out availableCount))
                        r.AvailableCount = availableCount;
                }
            }
        }

        /// <summary>过滤 ANSI 转义序列（颜色代码等）。</summary>
        public static string RemoveAnsiEscapeCodes(string input)
        {
            return Regex.Replace(input, @"\x1B\[[0-9;]*[mGK]", string.Empty);
        }
    }
}
