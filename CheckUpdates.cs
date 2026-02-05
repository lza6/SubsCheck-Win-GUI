using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace subs_check.win.gui
{
    public partial class CheckUpdates : Form
    {
        // 添加一个属性用于存储和传递文本内容
        public string UrlContent { get; set; }
        public System.Windows.Forms.ComboBox.ObjectCollection githubProxys { get; set; }
        public string githubProxy { get; set; }
        string githubProxyURL;
        public string 当前subsCheck版本号 { get; set; }
        public string 当前GUI版本号 { get; set; }
        public string 最新GUI版本号 { get; set; }
        public Action<string, bool> LogCallback { get; set; } // 添加日志回调
        public bool DisableGitHubProxy { get; set; } // 禁用GitHub代理状态

        public CheckUpdates()
        {
            InitializeComponent();
        }

        // 判断代理类型：直接访问型（1）或代理加速型（2）
        private int GetProxyType(string proxyItem)
        {
            // 直接访问型镜像站（直接替换 github.com）
            var directAccessProxies = new[]
            {
                "bgithub.xyz",
                "kkgithub.com",
                "gitclone.com",
                "github.ur1.fun"
            };

            // 检查是否是直接访问型
            foreach (var proxy in directAccessProxies)
            {
                if (proxyItem.StartsWith(proxy))
                {
                    return 1; // 直接访问型
                }
            }

            return 2; // 默认为代理加速型
        }

        // 日志输出方法
        private void Log(string message, bool isError = false)
        {
            // 如果有回调，调用主窗口的 Log 方法（主窗口会添加时间戳）
            if (LogCallback != null)
            {
                try
                {
                    LogCallback(message, isError);
                }
                catch { }
            }
            // 如果没有回调（例如在测试时），可以在这里输出到控制台
            else
            {
                Console.WriteLine(message);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

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

        private async void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            label3.Text = 最新GUI版本号;
            label4.Text = 当前GUI版本号;

            label5.Text = 当前subsCheck版本号;

            // 检查是否禁用GitHub代理
            if (checkBox1.Checked)
            {
                Log("已禁用GitHub代理，将使用本地代理模式");
                githubProxyURL = "";
                DisableGitHubProxy = true;
            }
            else
            {
                DisableGitHubProxy = false;
                
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

                    // 并发检测所有代理，选择延迟最低的
                    githubProxyURL = await DetectFastestGitHubProxyAsync(proxyItems);
                }
                else
                {
                    githubProxyURL = $"https://{comboBox1.Text}/";
                }
            }

            if (最新GUI版本号 != 当前GUI版本号)
            {
                // 检查当前目录下是否存在 Upgrade.exe
                string upgradeExePath = System.IO.Path.Combine(Application.StartupPath, "Upgrade.exe");
                if (System.IO.File.Exists(upgradeExePath))
                {
                    button1.Text = "立即更新";
                    button1.Enabled = true;
                }
                else
                {
                    button1.Text = "缺少更新程序";
                    button1.Enabled = false;
                }
            }
            else
            {
                button1.Text = "已是最新版本";
                button1.Enabled = false;
            }

            // 获取最新版本号
            string latestVersion = await GetLatestVersionFromRedirect();
            if (!string.IsNullOrEmpty(latestVersion))
            {
                label6.Text = latestVersion;
                if (当前subsCheck版本号 != latestVersion)
                {
                    button2.Text = "立即更新";
                    button2.Enabled = true;
                }
                else
                {
                    button2.Text = "已是最新版本";
                    button2.Enabled = false;
                }
            }
        }

        // 从重定向URL中提取版本号
        private async Task<string> GetLatestVersionFromRedirect()
        {
            string url;
            string version = "";

            // 如果禁用GitHub代理或检测到了可用的代理，使用代理地址访问
            if (!checkBox1.Checked && !string.IsNullOrEmpty(githubProxyURL))
            {
                // 获取代理类型
                string proxyName = githubProxyURL.Replace("https://", "").Replace("/", "");
                int proxyType = GetProxyType(proxyName);

                if (proxyType == 1)
                {
                    // 直接访问型：https://bgithub.xyz/beck-8/subs-check/releases/latest
                    url = $"{githubProxyURL}beck-8/subs-check/releases/latest";
                }
                else
                {
                    // 代理加速型：https://ghp.ci/github.com/beck-8/subs-check/releases/latest
                    url = $"{githubProxyURL}github.com/beck-8/subs-check/releases/latest";
                }
                Log($"使用GitHub代理: {url}");
            }
            else
            {
                url = "https://github.com/beck-8/subs-check/releases/latest";
                if (checkBox1.Checked)
                {
                    Log("禁用GitHub代理，使用原生GitHub链接（依赖本地代理）");
                }
                else
                {
                    Log($"未检测到可用的GitHub代理，使用原生GitHub链接");
                }
            }

            Log($"访问URL: {url}");

            try
            {
                using (HttpClientHandler handler = new HttpClientHandler())
                {
                    // 如果禁用GitHub代理，尝试使用本地代理
                    if (checkBox1.Checked)
                    {
                        // 检测本地代理
                        int[] commonPorts = { 10808, 10809, 1080, 7890, 7891, 1087, 1086, 1085 };
                        foreach (int port in commonPorts)
                        {
                            if (await TestLocalProxyPort(port))
                            {
                                var proxy = new System.Net.WebProxy($"http://127.0.0.1:{port}");
                                handler.Proxy = proxy;
                                Log($"配置使用本地代理: 127.0.0.1:{port}");
                                break;
                            }
                        }
                    }
                    
                    using (HttpClient client = new HttpClient(handler))
                    {
                        // 允许自动重定向
                        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win32; x86) AppleWebKit/537.36 (KHTML, like Gecko) cmliu/SubsCheck-Win-GUI");
                        client.Timeout = TimeSpan.FromSeconds(30);

                        // 发送请求
                        HttpResponseMessage response = await client.GetAsync(url);

                        Log($"响应状态: HTTP {(int)response.StatusCode} {response.StatusCode}");

                        // 打印重要的响应头
                        if (response.Headers.Location != null)
                        {
                            Log($"重定向位置: {response.Headers.Location}");
                        }
                        if (response.Content.Headers.ContentType != null)
                        {
                            Log($"内容类型: {response.Content.Headers.ContentType}");
                        }

                        // 获取重定向后的URL
                        if (response.StatusCode == System.Net.HttpStatusCode.Found || response.StatusCode == System.Net.HttpStatusCode.MovedPermanently)
                        {
                            var redirectUrl = response.Headers.Location?.ToString();
                            Log($"检测到重定向，重定向到: {redirectUrl}");

                            if (!string.IsNullOrEmpty(redirectUrl))
                            {
                                // 从URL中提取版本号，例如: https://github.com/beck-8/subs-check/releases/tag/v1.4.6
                                var lastSlashIndex = redirectUrl.LastIndexOf('/');
                                if (lastSlashIndex > 0)
                                {
                                    version = redirectUrl.Substring(lastSlashIndex + 1);
                                    Log($"从重定向URL提取到版本号: {version}");
                                }
                            }
                        }
                        else if (response.IsSuccessStatusCode)
                        {
                            // 如果没有重定向，尝试从响应内容中解析
                            string content = await response.Content.ReadAsStringAsync();
                            Log($"响应内容长度: {content.Length} 字符");

                            // 打印响应内容的前500字符（如果内容不太长）
                            if (content.Length > 0)
                            {
                                string preview = content.Length > 500 ? content.Substring(0, 500) + "..." : content;
                                Log($"响应内容预览:\n{preview}");
                            }

                            // 尝试从HTML中查找版本号
                            var regex = new System.Text.RegularExpressions.Regex(@"tag/v([\d.]+)");
                            var match = regex.Match(content);
                            if (match.Success)
                            {
                                version = "v" + match.Groups[1].Value;
                                Log($"从HTML内容提取到版本号: {version}");
                            }
                            else
                            {
                                Log("未能在HTML内容中找到版本号", true);
                            }
                        }
                        else
                        {
                            Log($"请求失败 HTTP {(int)response.StatusCode}", true);
                            // 尝试读取错误响应内容
                            try
                            {
                                string errorContent = await response.Content.ReadAsStringAsync();
                                if (!string.IsNullOrEmpty(errorContent))
                                {
                                    Log($"错误响应内容: {errorContent.Substring(0, Math.Min(500, errorContent.Length))}", true);
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"获取版本信息出错: {ex.Message}", true);
                Log($"异常类型: {ex.GetType().Name}", true);
            }

            return version;
        }

        // 测试本地代理端口是否可用
        private async Task<bool> TestLocalProxyPort(int port)
        {
            try
            {
                using (HttpClientHandler handler = new HttpClientHandler())
                {
                    // 设置代理
                    var proxy = new System.Net.WebProxy($"http://127.0.0.1:{port}");
                    handler.Proxy = proxy;
                    
                    using (var client = new HttpClient(handler))
                    {
                        client.Timeout = TimeSpan.FromSeconds(3);

                        // 使用代理访问一个简单的API来测试连接
                        var response = await client.GetAsync("http://www.google.com/generate_204");
                        return response.IsSuccessStatusCode;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        // 创建专用方法用于异步检测GitHub代理
        private async Task<string> DetectGitHubProxyAsync(List<string> proxyItems)
        {
            string detectedProxyURL = "";

            // 遍历随机排序后的代理列表
            foreach (string proxyItem in proxyItems)
            {
                string checkUrl = $"https://{proxyItem}/https://raw.githubusercontent.com/cmliu/SubsCheck-Win-GUI/master/packages.config";

                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromSeconds(5); // 设置5秒超时
                                                                  // 添加User-Agent头，避免被拒绝访问
                        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win32; x86) AppleWebKit/537.36 (KHTML, like Gecko) cmliu/SubsCheck-Win-GUI");

                        // 使用异步方式
                        HttpResponseMessage response = await client.GetAsync(checkUrl);
                        if (response.IsSuccessStatusCode)
                        {
                            // 找到可用代理
                            detectedProxyURL = $"https://{proxyItem}/";
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 记录错误但继续尝试下一个
                    Log($"代理 {proxyItem} 测试失败: {ex.Message}", true);
                }
            }
            return detectedProxyURL;
        }

        // 代理检测结果
        private class ProxyTestResult
        {
            public string ProxyName { get; set; }
            public string ProxyURL { get; set; }
            public long Latency { get; set; }
            public bool IsAvailable { get; set; }
        }

        private async Task<string> DetectFastestGitHubProxyAsync(List<string> proxyItems)
        {
            Log($"开始检测 {proxyItems.Count} 个 GitHub 代理...");

            // 创建检测任务列表
            var tasks = new List<Task<ProxyTestResult>>();

            foreach (string proxyItem in proxyItems)
            {
                tasks.Add(TestProxyLatency(proxyItem));
            }

            // 并发执行所有检测任务
            var results = await Task.WhenAll(tasks);

            // 批量收集日志消息，避免频繁刷新UI
            var logMessages = new System.Text.StringBuilder();
            logMessages.AppendLine("代理检测结果:");
            int availableCount = 0;
            int failedCount = 0;
            
            foreach (var proxy in results)
            {
                if (proxy.IsAvailable)
                {
                    logMessages.AppendLine($"  ✓ {proxy.ProxyName}: {proxy.Latency}ms");
                    availableCount++;
                }
                else
                {
                    failedCount++;
                }
            }
            
            logMessages.AppendLine($"检测完成：共检测 {results.Length} 个代理，可用 {availableCount} 个，失败 {failedCount} 个");
            
            // 一次性输出所有日志
            Log(logMessages.ToString());

            // 过滤出可用的代理
            var availableProxies = results.Where(r => r.IsAvailable).ToList();

            if (availableProxies.Count == 0)
            {
                Log("未找到可用的 GitHub 代理", true);
                return "";
            }

            // 找到延迟最低的代理
            var fastestProxy = availableProxies.OrderBy(p => p.Latency).First();

            // 打印可用代理的延迟排名
            Log($"可用代理延迟排名 ({availableProxies.Count}个):");
            foreach (var proxy in availableProxies.OrderBy(p => p.Latency))
            {
                Log($"  - {proxy.ProxyName}: {proxy.Latency}ms");
            }
            Log($"选择延迟最低的代理: {fastestProxy.ProxyName} ({fastestProxy.Latency}ms)");

            return fastestProxy.ProxyURL;
        }

        private async Task<ProxyTestResult> TestProxyLatency(string proxyItem)
        {
            var result = new ProxyTestResult
            {
                ProxyName = proxyItem,
                ProxyURL = $"https://{proxyItem}/",
                IsAvailable = false
            };

            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                string checkUrl;
                int proxyType = GetProxyType(proxyItem);

                if (proxyType == 1)
                {
                    // 直接访问型：https://bgithub.xyz/cmliu/SubsCheck-Win-GUI/master/packages.config
                    checkUrl = $"https://{proxyItem}/cmliu/SubsCheck-Win-GUI/master/packages.config";
                }
                else
                {
                    // 代理加速型：https://ghp.ci/https://raw.githubusercontent.com/cmliu/SubsCheck-Win-GUI/master/packages.config
                    checkUrl = $"https://{proxyItem}/https://raw.githubusercontent.com/cmliu/SubsCheck-Win-GUI/master/packages.config";
                }

                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win32; x86) AppleWebKit/537.36 (KHTML, like Gecko) cmliu/SubsCheck-Win-GUI");

                    HttpResponseMessage response = await client.GetAsync(checkUrl);
                    stopwatch.Stop();

                    if (response.IsSuccessStatusCode)
                    {
                        result.IsAvailable = true;
                        result.Latency = stopwatch.ElapsedMilliseconds;
                    }
                }
            }
            catch
            {
                result.IsAvailable = false;
            }

            return result;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // 设置对话框结果为OK，表示用户点击了"立即更新"按钮
            this.DialogResult = DialogResult.OK;

            // 关闭窗口
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //下载链接
            string downloadURL = $"{githubProxyURL}https://github.com/cmliu/SubsCheck-Win-GUI/releases/download/{最新GUI版本号}/SubsCheck_Win_GUI.zip";
            //目标文件
            string downloadEXE = "subs-check.win.gui.exe";

            try
            {
                // 获取应用程序目录
                string executablePath = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
                // 创建 Upgrade.ini 文件路径
                string iniFilePath = System.IO.Path.Combine(executablePath, "Upgrade.ini");

                // 准备 INI 文件内容
                string iniContent =
                    "[Upgrade]\r\n" +
                    $"DownloadURL={downloadURL}\r\n" +
                    $"TargetFile={downloadEXE}\r\n";

                // 写入文件（如果文件已存在会被覆盖）
                System.IO.File.WriteAllText(iniFilePath, iniContent);

                DialogResult result = MessageBox.Show(
                    $"发现新版本: {最新GUI版本号}\n\n" +
                    "· 点击【确定】将下载并安装更新\n" +
                    "· 更新过程中程序会自动关闭并重启\n" +
                    "· 更新完成后所有设置将保持不变\n\n" +
                    "是否立即更新到最新版本？",
                    "发现新版本",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Information);

                if (result == DialogResult.OK)
                {
                    // 检查目标文件是否存在
                    string targetFilePath = System.IO.Path.Combine(Application.StartupPath, "Upgrade.exe");
                    if (System.IO.File.Exists(targetFilePath))
                    {
                        // 使用Process.Start异步启动应用程序
                        System.Diagnostics.Process.Start(targetFilePath);
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("更新程序 Upgrade.exe 不存在！",
                            "错误",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"写入更新信息时出错: {ex.Message}",
                    "错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                Log("已勾选【禁用GitHub代理】，将使用本地代理模式");
                DisableGitHubProxy = true;
            }
            else
            {
                Log("已取消勾选【禁用GitHub代理】，将使用GitHub代理");
                DisableGitHubProxy = false;
            }
        }
    }
}
