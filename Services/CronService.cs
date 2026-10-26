using System;
using System.Linq;

namespace subs_check.win.gui
{
    /// <summary>
    /// cron 表达式纯逻辑（从 Form1 抽离）：校验 + 友好描述生成。
    /// 5 段格式：分 时 日 月 周。无外部依赖，可独立测试。
    /// </summary>
    public static class CronService
    {
        private static readonly string[] Weekdays =
            { "星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六", "星期日" };

        /// <summary>校验 5 段 cron 表达式是否合法。</summary>
        public static bool IsValid(string cronExpression)
        {
            string expr = (cronExpression ?? "").Trim();
            if (string.IsNullOrWhiteSpace(expr)) return false;

            string[] parts = expr.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 5) return false;

            try
            {
                return IsValidField(parts[0], 0, 59)   // 分钟
                    && IsValidField(parts[1], 0, 23)   // 小时
                    && IsValidField(parts[2], 1, 31)   // 日
                    && IsValidField(parts[3], 1, 12)   // 月
                    && IsValidField(parts[4], 0, 7);   // 星期（0/7 均为周日）
            }
            catch
            {
                return false;
            }
        }

        /// <summary>生成 cron 表达式的中文友好描述（与旧版 GetCronExpressionDescription 输出一致）。</summary>
        public static string Describe(string cronExpression)
        {
            try
            {
                string expr = (cronExpression ?? "").Trim();
                string[] parts = expr.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 5) return "无效的cron表达式";

                string minuteDesc = ParsePart(parts[0], "分钟", false);
                string hourDesc = ParsePart(parts[1], "小时", false);
                string dayDesc = ParsePart(parts[2], "日", false);
                string monthDesc = ParsePart(parts[3], "月", false);
                string weekDesc = ParsePart(parts[4], "星期", true);

                string description = "执行时间: ";

                if (monthDesc != "每月") description += monthDesc + "的";

                if (parts[2] == "*" && parts[4] != "*") description += weekDesc + "的";
                else if (parts[2] != "*" && parts[4] == "*") description += dayDesc;
                else if (parts[2] != "*" && parts[4] != "*") description += $"{dayDesc}或{weekDesc}";
                else description += "每天";

                description += $"{hourDesc}{minuteDesc}";
                return description;
            }
            catch
            {
                return "无法解析cron表达式";
            }
        }

        private static bool IsValidField(string field, int min, int max)
        {
            if (field == "*") return true;

            if (field.StartsWith("*/"))
            {
                int step;
                if (int.TryParse(field.Substring(2), out step))
                    return step > 0 && step <= max;
                return false;
            }

            if (field.Contains("-"))
            {
                string[] range = field.Split('-');
                if (range.Length != 2) return false;
                int start, end;
                if (int.TryParse(range[0], out start) && int.TryParse(range[1], out end))
                    return start >= min && end <= max && start <= end;
                return false;
            }

            if (field.Contains(","))
            {
                foreach (string item in field.Split(','))
                {
                    int v;
                    if (!int.TryParse(item, out v) || v < min || v > max) return false;
                }
                return true;
            }

            int fieldValue;
            if (int.TryParse(field, out fieldValue)) return fieldValue >= min && fieldValue <= max;

            return false;
        }

        private static string ParsePart(string part, string unit, bool isWeekday)
        {
            if (part == "*") return $"每{unit}";

            if (part.StartsWith("*/"))
            {
                int step = int.Parse(part.Substring(2));
                return $"每{step}{unit}";
            }

            if (part.Contains("-"))
            {
                string[] range = part.Split('-');
                int start = int.Parse(range[0]);
                int end = int.Parse(range[1]);
                if (isWeekday) return $"从{Weekdays[start]}到{Weekdays[end]}";
                return $"从{start}{unit}到{end}{unit}";
            }

            if (part.Contains(","))
            {
                string[] values = part.Split(',');
                if (isWeekday)
                    return string.Join("、", values.Select(v => Weekdays[int.Parse(v)]));
                return $"{string.Join("、", values)}{unit}";
            }

            int value = int.Parse(part);
            if (isWeekday) return Weekdays[value];
            return $"{value}{unit}";
        }
    }
}
