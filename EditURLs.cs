using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace subs_check.win.gui
{
    public partial class EditURLs: Form
    {
        // 添加一个属性用于存储和传递文本内容
        public string UrlContent { get; set; }
        public System.Windows.Forms.ComboBox.ObjectCollection githubProxys { get; set; }
        public string githubProxy { get; set; }
        string githubProxyURL;
        string SubsCheckURLs;

        // 创建一个类来存储代理测试结果
        private class ProxyResult
        {
            public string Proxy { get; set; }
            public bool IsAvailable { get; set; }
            public int LatencyMs { get; set; }
        }
        public EditURLs()
        {
            InitializeComponent();

            // 设置textBox1的锚点为上、左、右
            textBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

            // 设置button1、button2、button3的锚点为左、下
            button1.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            button2.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            button3.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;

            // 设置button4的锚点为右、下
            button4.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
        }

        // 加载窗体时处理传入的内容
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // 将传入的内容显示在textBox1中
            if (!string.IsNullOrEmpty(UrlContent)) textBox1.Text = UrlContent;

            timer1.Enabled = true;

            if (githubProxys != null)
            {
                comboBox1.Items.Clear();
                foreach (var item in githubProxys)
                {
                    comboBox1.Items.Add(item);
                }
            }
            if (!string.IsNullOrEmpty(githubProxy)) comboBox1.Text = githubProxy;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // 将textBox1的内容保存到UrlContent属性
            UrlContent = textBox1.Text;

            // 设置对话框结果为OK并关闭窗口
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // 确保有内容需要处理
            if (string.IsNullOrWhiteSpace(textBox1.Text))
                return;

            // 按行分割文本
            string[] lines = textBox1.Text.Split(
                new[] { "\r\n", "\r", "\n" },
                StringSplitOptions.RemoveEmptyEntries);

            // 去除每行首尾的空白字符
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].Trim();
            }

            // 使用LINQ的Distinct()方法去重，并过滤掉空行
            string[] uniqueLines = lines
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Distinct(StringComparer.OrdinalIgnoreCase) // 忽略大小写进行去重
                .ToArray();

            // 将去重后的内容设回textBox1
            textBox1.Text = string.Join(Environment.NewLine, uniqueLines) + "\n";

            // 可选：显示去重结果
            int removed = lines.Length - uniqueLines.Length;
            if (removed > 0)
            {
                MessageBox.Show($"已移除 {removed} 个重复行。", "去重完成",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("没有发现重复行。", "去重完成",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;

            if (comboBox1.Text == "自动选择")
            {
                // 创建不包含"自动选择"的代理列表
                List<string> proxyItems = new List<string>();
                for (int j = 0; j < comboBox1.Items.Count; j++)
                {
                    string proxyItem = comboBox1.Items[j].ToString();
                    if (proxyItem != "自动选择")
                        proxyItems.Add(proxyItem);
                }

                // 随机打乱列表顺序
                Random random = new Random();
                proxyItems = proxyItems.OrderBy(x => random.Next()).ToList();

                // 异步检测可用代理
                githubProxyURL = await DetectGitHubProxyAsync(proxyItems);
            }
            else
            {
                githubProxyURL = $"https://{comboBox1.Text}/";
            }

            string SubsCheckURLsURL = $"{githubProxyURL}raw.githubusercontent.com/cmliu/cmliu/main/SubsCheck-URLs";
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5); // 设置5秒超时
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win32; x86) AppleWebKit/537.36 (KHTML, like Gecko) cmliu/SubsCheck-Win-GUI");

                    // 使用异步方式检查URL可访问性
                    HttpResponseMessage response = await client.GetAsync(SubsCheckURLsURL);
                    if (response.IsSuccessStatusCode)
                    {
                        button2.Text = "在线获取";
                        button2.Enabled = true;
                        SubsCheckURLs = await response.Content.ReadAsStringAsync();
                        //MessageBox.Show(SubsCheckURLs);
                    }
                    else
                    {
                        string headers = string.Join("\n", response.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}"));
                        string errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"获取SubsCheckURLs失败: HTTP {(int)response.StatusCode} {response.ReasonPhrase}");
                        Console.WriteLine($"请求URL: {SubsCheckURLsURL}");
                        Console.WriteLine($"响应头:\n{headers}");
                        Console.WriteLine($"响应内容: {errorContent}");
                        button2.Text = "获取失败";
                        button2.Enabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取SubsCheckURLs出错: {ex.GetType().Name} - {ex.Message}");
                Console.WriteLine($"请求URL: {SubsCheckURLsURL}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"内部异常: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}");
                }
                Console.WriteLine($"堆栈跟踪:\n{ex.StackTrace}");
                // 网络错误或其他异常情况
                button2.Text = "获取失败";
                button2.Enabled = false;
            }
        }

        // 创建专用方法用于异步检测GitHub代理（并发测试）
        private async Task<string> DetectGitHubProxyAsync(List<string> proxyItems)
        {
            Console.WriteLine($"检测可用 GitHub 代理（共{proxyItems.Count}个）...");

            List<Task<ProxyResult>> testTasks = new List<Task<ProxyResult>>();

            // 为每个代理创建测试任务
            foreach (string proxyItem in proxyItems)
            {
                var task = Task.Run(async () =>
                {
                    string checkUrl = $"https://{proxyItem}/raw.githubusercontent.com/cmliu/SubsCheck-Win-GUI/master/packages.config";
                    ProxyResult result = new ProxyResult { Proxy = proxyItem, IsAvailable = false, LatencyMs = -1 };

                    try
                    {
                        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

                        using (HttpClient client = new HttpClient())
                        {
                            client.Timeout = TimeSpan.FromSeconds(5); // 设置5秒超时
                            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win32; x86) AppleWebKit/537.36 (KHTML, like Gecko) cmliu/SubsCheck-Win-GUI");

                            // 使用异步方式
                            HttpResponseMessage response = await client.GetAsync(checkUrl);
                            stopwatch.Stop();

                            if (response.IsSuccessStatusCode)
                            {
                                result.IsAvailable = true;
                                result.LatencyMs = (int)stopwatch.ElapsedMilliseconds;
                            }
                        }
                    }
                    catch
                    {
                        // 忽略异常，result.IsAvailable 保持为 false
                    }

                    return result;
                });
                testTasks.Add(task);
            }

            Console.WriteLine($"正在并发测试 {proxyItems.Count} 个 GitHub 代理...");

            // 等待所有测试任务完成
            ProxyResult[] allResults = await Task.WhenAll(testTasks);

            // 统计并显示结果
            List<ProxyResult> availableProxies = new List<ProxyResult>();
            foreach (var result in allResults)
            {
                if (result.IsAvailable)
                {
                    availableProxies.Add(result);
                    Console.WriteLine($"✓ 代理 {result.Proxy} 可用 (延迟: {result.LatencyMs}ms)");
                }
                else
                {
                    Console.WriteLine($"✗ 代理 {result.Proxy} 不可用");
                }
            }

            // 如果没有找到可用的代理
            if (availableProxies.Count == 0)
            {
                Console.WriteLine("未找到可用的 GitHub 代理");
                return "";
            }

            // 按延迟排序，选择最快的代理
            var bestProxy = availableProxies.OrderBy(r => r.LatencyMs).First();
            Console.WriteLine($"选择最优代理: {bestProxy.Proxy} (延迟: {bestProxy.LatencyMs}ms)");

            return $"https://{bestProxy.Proxy}/";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // 检查是否已成功获取在线内容
            if (string.IsNullOrEmpty(SubsCheckURLs))
            {
                MessageBox.Show("未能获取在线内容，请重试。", "获取失败",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 显示带有"覆盖"和"追加"选项的对话框
            DialogResult result = MessageBox.Show(
                "请选择如何处理获取到的内容：\n\n" +
                "- 点击【是】将覆盖当前内容\n" +
                "- 点击【否】将追加到当前内容后面\n" +
                "- 点击【取消】不做任何操作",
                "操作选择",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            // 根据用户选择执行对应操作
            if (result == DialogResult.Yes)
            {
                // 覆盖操作
                // 确保所有换行符格式一致 (将单独的\n或\r替换为Windows风格的\r\n)
                string normalizedText = NormalizeLineEndings(SubsCheckURLs);
                textBox1.Text = normalizedText;
                MessageBox.Show("已用在线内容覆盖原有内容。", "感谢大自然的馈赠",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (result == DialogResult.No)
            {
                // 追加操作
                // 确保原有内容末尾有换行符
                if (!textBox1.Text.EndsWith("\r\n") && !textBox1.Text.EndsWith("\n"))
                {
                    textBox1.Text += Environment.NewLine;
                }

                // 追加新内容，确保换行符格式一致
                textBox1.Text += NormalizeLineEndings(SubsCheckURLs);
                MessageBox.Show("已将在线内容追加到原有内容后。", "感谢大自然的馈赠",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            // 如果用户选择"取消"，不执行任何操作
        }

        // 标准化文本中的换行符为Windows风格 (\r\n)
        private string NormalizeLineEndings(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            // 先将所有类型的换行符替换为\n
            text = text.Replace("\r\n", "\n").Replace("\r", "\n");
            // 然后将\n替换为\r\n
            text = text.Replace("\n", Environment.NewLine);

            return text;
        }

    }
}
