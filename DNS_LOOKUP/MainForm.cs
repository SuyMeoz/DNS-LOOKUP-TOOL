using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DnsClient;
using JsonFormatting = Newtonsoft.Json.Formatting;
using Newtonsoft.Json;
using System.Net.NetworkInformation;
using System.Diagnostics;


namespace DnsLookupTool
{
    public partial class MainForm : Form
    {
        // Configuration
        private IPAddress? CustomDnsServer = null;
        private List<HistoryEntry> QueryHistory = new List<HistoryEntry>();
        private int RetryCount = 3;
        private TimeSpan Timeout = TimeSpan.FromSeconds(5);
        private bool ForceTcpOnly = false;
        private bool EnableDnsSec = false;
        private string SecuritySettingsFile = "security_settings.json";

        // UI Controls
        private TabControl tabControl = null!;
        private Panel headerPanel = null!;
        private Label lblDnsServer = null!;
        private Button btnConfigDns = null!;

        // Tab 1: Domain Lookup (A/AAAA)
        private TextBox txtDomain = null!;
        private Button btnLookupDomain = null!;
        private ListBox lstResults = null!;
        private Label lblStatus = null!;

        // Tab 2: Reverse Lookup (PTR)
        private TextBox txtIpReverse = null!;
        private Button btnReverseIp = null!;
        private RichTextBox rtbReverseResults = null!;

        // Tab 3: Multiple Records
        private TextBox txtQueryMulti = null!;
        private ComboBox cmbRecordType = null!;
        private Button btnQueryMulti = null!;
        private RichTextBox rtbMultiResults = null!;

        // Tab 4: Batch Process
        private TextBox txtBatchFile = null!;
        private Button btnBrowseBatch = null!;
        private Button btnProcessBatch = null!;
        private RichTextBox rtbBatchResults = null!;
        private ProgressBar progressBatch = null!;

        // Tab 5: History
        private DataGridView dgvHistory = null!;

        // Tab 6: Settings
        private CheckBox chkForceTcp = null!;
        private CheckBox chkDnsSec = null!;
        private Button btnResetSecurity = null!;
        private Button btnExportResults = null!;

        public MainForm()
        {
            InitializeComponent();
            LoadHistory();
            LoadSecuritySettings();
            ApplyTheming(); 
        }

        private void InitializeComponent()
        {
            this.Text = "DNS Lookup Tool";
            this.Size = new Size(900, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Icon = SystemIcons.Application;

            headerPanel = new Panel
            {
                Height = 60,
                Dock = DockStyle.Bottom,
                BackColor = Color.FromArgb(45, 45, 48)
            };

            var lblTitle = new Label
            {
                Text = "🌐 DNS Lookup Tool",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(15, 0)
            };
            headerPanel.Controls.Add(lblTitle);

            lblDnsServer = new Label
            {
                Text = $"DNS Server: {(CustomDnsServer?.ToString() ?? "Default")}",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.LightGray,
                AutoSize = true,
                Location = new Point(15, 35)
            };
            headerPanel.Controls.Add(lblDnsServer);

            btnConfigDns = new Button
            {
                Text = "⚙️ Configure DNS",
                Location = new Point(780, 15),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnConfigDns.Click += BtnConfigDns_Click;
            headerPanel.Controls.Add(btnConfigDns);

            this.Controls.Add(headerPanel);

            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                TabIndex = 0
            };

            CreateDomainLookupTab();
            CreateReverseLookupTab();
            CreateMultipleRecordsTab();
            CreateBatchProcessTab();
            CreateHistoryTab();
            CreateSettingsTab();
            CreateNetworkToolsTab();
            CreateWhoisTab();
            CreateInfoTab();

            this.Controls.Add(tabControl);
        }

        // Tab 7
        private void CreateNetworkToolsTab()
        {
            var tabPage = new TabPage { Text = "Network Tools", AutoScroll = true };
            tabPage.BackColor = Color.White;

            // Ping section
            var lblPing = new Label
            {
                Text = "Ping Test:",
                Location = new Point(20, 20),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true
            };
            tabPage.Controls.Add(lblPing);

            var txtPingHost = new TextBox
            {
                Location = new Point(20, 50),
                Size = new Size(200, 30),
                Font = new Font("Segoe UI", 11),
                Text = "google.com"
            };
            tabPage.Controls.Add(txtPingHost);

            var btnPing = new Button
            {
                Text = "🚀 Ping",
                Location = new Point(230, 50),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnPing.Click += async (s, e) =>
            {
                var host = txtPingHost.Text.Trim();
                if (string.IsNullOrWhiteSpace(host))
                {
                    MessageBox.Show("Please enter host to ping", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var pingResult = new RichTextBox
                {
                    Location = new Point(20, 90),
                    Size = new Size(500, 150),
                    Font = new Font("Consolas", 9),
                    ReadOnly = true
                };

                // Remove previous ping result if exists
                var existing = tabPage.Controls.OfType<RichTextBox>().FirstOrDefault(r => r.Name == "pingResult");
                if (existing != null) tabPage.Controls.Remove(existing);

                pingResult.Name = "pingResult";
                tabPage.Controls.Add(pingResult);

                try
                {
                    pingResult.AppendText($"Pinging {host}...\r\n");

                    var ping = new Ping();
                    var reply = await ping.SendPingAsync(host, 1000);

                    pingResult.AppendText($"\r\nReply from {reply.Address}: ");
                    pingResult.AppendText($"bytes={reply.Buffer.Length} ");
                    pingResult.AppendText($"time={reply.RoundtripTime}ms ");
                    pingResult.AppendText($"TTL={reply.Options?.Ttl ?? 0}\r\n");

                    pingResult.AppendText(reply.Status == IPStatus.Success
                        ? "✅ Ping successful\r\n"
                        : $"❌ Ping failed: {reply.Status}\r\n");
                }
                catch (Exception ex)
                {
                    pingResult.AppendText($"❌ Error: {ex.Message}\r\n");
                }
            };
            tabPage.Controls.Add(btnPing);

            // Traceroute section
            var lblTrace = new Label
            {
                Text = "Traceroute:",
                Location = new Point(20, 260),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true
            };
            tabPage.Controls.Add(lblTrace);

            var txtTraceHost = new TextBox
            {
                Location = new Point(20, 290),
                Size = new Size(200, 30),
                Font = new Font("Segoe UI", 11),
                Text = "google.com"
            };
            tabPage.Controls.Add(txtTraceHost);

            var btnTrace = new Button
            {
                Text = "🔄 Trace",
                Location = new Point(230, 290),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(156, 39, 176),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnTrace.Click += async (s, e) =>
            {
                var host = txtTraceHost.Text.Trim();
                if (string.IsNullOrWhiteSpace(host))
                {
                    MessageBox.Show("Please enter host to trace", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var traceResult = new RichTextBox
                {
                    Location = new Point(20, 330),
                    Size = new Size(500, 200),
                    Font = new Font("Consolas", 9),
                    ReadOnly = true
                };

                // Remove previous trace result if exists
                var existing = tabPage.Controls.OfType<RichTextBox>().FirstOrDefault(r => r.Name == "traceResult");
                if (existing != null) tabPage.Controls.Remove(existing);

                traceResult.Name = "traceResult";
                tabPage.Controls.Add(traceResult);

                traceResult.AppendText($"Tracing route to {host}...\r\n");

                await Task.Run(() =>
                {
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "tracert",
                            Arguments = host,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true
                        }
                    };

                    process.Start();

                    while (!process.StandardOutput.EndOfStream)
                    {
                        var line = process.StandardOutput.ReadLine();
                        traceResult.Invoke(new Action(() => traceResult.AppendText(line + "\r\n")));
                    }

                    process.WaitForExit();
                });
            };
            tabPage.Controls.Add(btnTrace);

            tabControl.TabPages.Add(tabPage);
        }

        // Tab 1
        private void CreateDomainLookupTab()
        {
            var tabPage = new TabPage { Text = "A/AAAA Lookup", AutoScroll = true };
            tabPage.BackColor = Color.White;

            var lblInput = new Label
            {
                Text = "Domain Name:",
                Location = new Point(20, 20),
                AutoSize = true
            };
            tabPage.Controls.Add(lblInput);

            txtDomain = new TextBox
            {
                Location = new Point(20, 45),
                Size = new Size(400, 30),
                Font = new Font("Segoe UI", 11)
            };
            tabPage.Controls.Add(txtDomain);

            btnLookupDomain = new Button
            {
                Text = "🔍 Lookup",
                Location = new Point(430, 45),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10)
            };
            btnLookupDomain.Click += BtnLookupDomain_Click;
            tabPage.Controls.Add(btnLookupDomain);

            lblStatus = new Label
            {
                Text = "Ready",
                Location = new Point(20, 85),
                Size = new Size(500, 25),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray
            };
            tabPage.Controls.Add(lblStatus);

            lstResults = new ListBox
            {
                Location = new Point(20, 120),
                Size = new Size(550, 350),
                Font = new Font("Segoe UI", 11),
                ItemHeight = 25
            };
            tabPage.Controls.Add(lstResults);

            tabControl.TabPages.Add(tabPage);
        }

        // Tab 2
        private void CreateReverseLookupTab()
        {
            var tabPage = new TabPage { Text = "PTR Lookup", AutoScroll = true };
            tabPage.BackColor = Color.White;

            var lblInput = new Label
            {
                Text = "IP Address:",
                Location = new Point(20, 20),
                AutoSize = true
            };
            tabPage.Controls.Add(lblInput);

            txtIpReverse = new TextBox
            {
                Location = new Point(20, 45),
                Size = new Size(400, 30),
                Font = new Font("Segoe UI", 11)
            };
            tabPage.Controls.Add(txtIpReverse);

            btnReverseIp = new Button
            {
                Text = "↩️ Reverse",
                Location = new Point(430, 45),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10)
            };
            btnReverseIp.Click += BtnReverseIp_Click;
            tabPage.Controls.Add(btnReverseIp);

            rtbReverseResults = new RichTextBox
            {
                Location = new Point(20, 90),
                Size = new Size(550, 380),
                Font = new Font("Segoe UI", 10),
                ReadOnly = true
            };
            tabPage.Controls.Add(rtbReverseResults);

            tabControl.TabPages.Add(tabPage);
        }

        // Tab 3
        private void CreateMultipleRecordsTab()
        {
            var tabPage = new TabPage { Text = "DNS Records", AutoScroll = true };
            tabPage.BackColor = Color.White;

            var lblInput = new Label
            {
                Text = "Domain or IP:",
                Location = new Point(20, 20),
                AutoSize = true
            };
            tabPage.Controls.Add(lblInput);

            txtQueryMulti = new TextBox
            {
                Location = new Point(20, 45),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 11)
            };
            tabPage.Controls.Add(txtQueryMulti);

            var lblType = new Label
            {
                Text = "Record Type:",
                Location = new Point(330, 20),
                AutoSize = true
            };
            tabPage.Controls.Add(lblType);

            cmbRecordType = new ComboBox
            {
                Location = new Point(330, 45),
                Size = new Size(100, 30),
                Font = new Font("Segoe UI", 11),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbRecordType.Items.AddRange(new[] { "A", "AAAA", "PTR", "MX", "CNAME", "TXT", "NS", "SOA" });
            cmbRecordType.SelectedIndex = 0;
            tabPage.Controls.Add(cmbRecordType);

            btnQueryMulti = new Button
            {
                Text = "📋 Query",
                Location = new Point(440, 45),
                Size = new Size(90, 30),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10)
            };
            btnQueryMulti.Click += BtnQueryMulti_Click;
            tabPage.Controls.Add(btnQueryMulti);

            rtbMultiResults = new RichTextBox
            {
                Location = new Point(20, 90),
                Size = new Size(510, 380),
                Font = new Font("Segoe UI", 10),
                ReadOnly = true
            };
            tabPage.Controls.Add(rtbMultiResults);

            tabControl.TabPages.Add(tabPage);
        }

        // Tab 4
        private void CreateBatchProcessTab()
        {
            var tabPage = new TabPage { Text = "Batch Process", AutoScroll = true };
            tabPage.BackColor = Color.White;

            var lblFile = new Label
            {
                Text = "Batch File (one query per line):",
                Location = new Point(20, 20),
                AutoSize = true
            };
            tabPage.Controls.Add(lblFile);

            txtBatchFile = new TextBox
            {
                Location = new Point(20, 45),
                Size = new Size(350, 30),
                Font = new Font("Segoe UI", 11),
                ReadOnly = true
            };
            tabPage.Controls.Add(txtBatchFile);

            btnBrowseBatch = new Button
            {
                Text = "📁 Browse",
                Location = new Point(380, 45),
                Size = new Size(90, 30),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnBrowseBatch.Click += BtnBrowseBatch_Click;
            tabPage.Controls.Add(btnBrowseBatch);

            btnProcessBatch = new Button
            {
                Text = "▶ Process",
                Location = new Point(480, 45),
                Size = new Size(90, 30),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnProcessBatch.Click += BtnProcessBatch_Click;
            tabPage.Controls.Add(btnProcessBatch);

            progressBatch = new ProgressBar
            {
                Location = new Point(20, 85),
                Size = new Size(550, 25),
                Style = ProgressBarStyle.Continuous
            };
            tabPage.Controls.Add(progressBatch);

            rtbBatchResults = new RichTextBox
            {
                Location = new Point(20, 120),
                Size = new Size(550, 350),
                Font = new Font("Consolas", 9),
                ReadOnly = true
            };
            tabPage.Controls.Add(rtbBatchResults);

            tabControl.TabPages.Add(tabPage);
        }

        // Tab 5
        private void CreateHistoryTab()
        {
            var tabPage = new TabPage { Text = "History", AutoScroll = true };
            tabPage.BackColor = Color.White;

            dgvHistory = new DataGridView
            {
                Location = new Point(20, 20),
                Size = new Size(550, 400),
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            dgvHistory.Columns.Add("Type", "Type");
            dgvHistory.Columns.Add("Query", "Query");
            dgvHistory.Columns.Add("Timestamp", "Timestamp");
            dgvHistory.Columns.Add("ElapsedMs", "Time (ms)");

            tabPage.Controls.Add(dgvHistory);

            var btnClearHistory = new Button
            {
                Text = "🗑️ Clear History",
                Location = new Point(20, 430),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnClearHistory.Click += (s, e) =>
            {
                QueryHistory.Clear();
                dgvHistory.Rows.Clear();
                SaveHistory();
            };
            tabPage.Controls.Add(btnClearHistory);

            tabControl.TabPages.Add(tabPage);
        }

        // Tab 6
        private void CreateSettingsTab()
        {
            var tabPage = new TabPage { Text = "Settings & Export", AutoScroll = true };
            tabPage.BackColor = Color.White;

            var lblSecurity = new Label
            {
                Text = "Security Settings:",
                Location = new Point(20, 20),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true
            };
            tabPage.Controls.Add(lblSecurity);

            chkForceTcp = new CheckBox
            {
                Text = "Force TCP Only",
                Location = new Point(20, 50),
                AutoSize = true,
                Checked = ForceTcpOnly
            };
            chkForceTcp.CheckedChanged += (s, e) => { ForceTcpOnly = chkForceTcp.Checked; SaveSecuritySettings(); };
            tabPage.Controls.Add(chkForceTcp);

            chkDnsSec = new CheckBox
            {
                Text = "Enable DNSSEC Validation (Stub)",
                Location = new Point(20, 80),
                AutoSize = true,
                Checked = EnableDnsSec
            };
            chkDnsSec.CheckedChanged += (s, e) => { EnableDnsSec = chkDnsSec.Checked; SaveSecuritySettings(); };
            tabPage.Controls.Add(chkDnsSec);

            btnResetSecurity = new Button
            {
                Text = "Reset to Defaults",
                Location = new Point(20, 120),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(255, 152, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnResetSecurity.Click += (s, e) =>
            {
                ForceTcpOnly = false;
                EnableDnsSec = false;
                chkForceTcp.Checked = false;
                chkDnsSec.Checked = false;
                SaveSecuritySettings();
            };
            tabPage.Controls.Add(btnResetSecurity);

            var lblExport = new Label
            {
                Text = "Export & Report:",
                Location = new Point(20, 180),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true
            };
            tabPage.Controls.Add(lblExport);

            btnExportResults = new Button
            {
                Text = "💾 Export History",
                Location = new Point(20, 210),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnExportResults.Click += BtnExportResults_Click;
            tabPage.Controls.Add(btnExportResults);

            var btnGenerateReport = new Button
            {
                Text = "📊 Generate Report",
                Location = new Point(180, 210),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(156, 39, 176),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnGenerateReport.Click += BtnGenerateReport_Click;
            tabPage.Controls.Add(btnGenerateReport);

            tabControl.TabPages.Add(tabPage);
        }

        // Tab 9
        private void CreateInfoTab()
        {
            var tabPage = new TabPage { Text = "About", AutoScroll = true };
            tabPage.BackColor = Color.White;

            var panel = new Panel
            {
                Location = new Point(50, 50),
                Size = new Size(500, 400),
                BorderStyle = BorderStyle.None
            };

            var lblTitle = new Label
            {
                Text = "🌐 DNS Lookup Tool",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 120, 215),
                AutoSize = true,
                Location = new Point(0, 0)
            };

            var lblAuthor = new Label
            {
                Text = "Created by: Hoang Hao Hung [ MUOP THE LO ]",
                Font = new Font("Segoe UI", 14, FontStyle.Regular),
                ForeColor = Color.Black,
                AutoSize = true,
                Location = new Point(0, 50)
            };

            var lblVersion = new Label
            {
                Text = "Version: 2.0",
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(0, 90)
            };

            var lblFeatures = new Label
            {
                Text = "Features:\n• DNS Lookup (A, AAAA, MX, TXT, etc.)" +
                "\n• Reverse DNS (PTR)\n• Batch Processing\n• Network Tools\n• Export Results\n• Query History\n• Who Is Tool",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.Black,
                AutoSize = false,
                Size = new Size(450, 200),
                Location = new Point(0, 130)
            };

            panel.Controls.Add(lblTitle);
            panel.Controls.Add(lblAuthor);
            panel.Controls.Add(lblVersion);
            panel.Controls.Add(lblFeatures);

            tabPage.Controls.Add(panel);
            tabControl.TabPages.Add(tabPage);
        }

        private void ApplyTheming()
        {
            // Modern dark theme for better UX
            this.BackColor = Color.FromArgb(30, 30, 30);

            // Style all buttons consistently
            var buttons = this.Controls.OfType<Button>()
                .Concat(tabControl.TabPages.Cast<TabPage>()
                    .SelectMany(tp => tp.Controls.OfType<Button>()));

            foreach (var btn in buttons)
            {
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.Cursor = Cursors.Hand;
                btn.Font = new Font("Segoe UI", 10);
            }

            // Style textboxes
            var textboxes = this.Controls.OfType<TextBox>()
                .Concat(tabControl.TabPages.Cast<TabPage>()
                    .SelectMany(tp => tp.Controls.OfType<TextBox>()));

            foreach (var txt in textboxes)
            {
                txt.BackColor = Color.FromArgb(45, 45, 48);
                txt.ForeColor = Color.White;
                txt.BorderStyle = BorderStyle.FixedSingle;
            }
        }

        // Event Handlers
        private async void BtnLookupDomain_Click(object? sender, EventArgs e)
        {
            var domain = txtDomain.Text.Trim();
            if (string.IsNullOrWhiteSpace(domain))
            {
                MessageBox.Show("Please enter a domain name", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            lblStatus.Text = "Looking up...";
            lstResults.Items.Clear();

            try
            {
                var client = CreateLookupClient();
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var result = await client.QueryAsync(domain, QueryType.A);
                sw.Stop();

                if (result.Answers.Count == 0)
                {
                    lstResults.Items.Add("No records found");
                    lblStatus.Text = "No results";
                    return;
                }

                var addresses = result.Answers.OfType<DnsClient.Protocol.ARecord>().Select(r => r.Address.ToString()).ToArray();
                foreach (var ip in addresses)
                {
                    lstResults.Items.Add($"📌 {ip}");
                }

                lblStatus.Text = $"Found {addresses.Length} result(s) in {sw.ElapsedMilliseconds}ms";
                SaveToHistory("A/AAAA", domain, sw.ElapsedMilliseconds, string.Join(", ", addresses));
                RefreshHistory();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Lookup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Error";
            }
        }

        private async void BtnReverseIp_Click(object? sender, EventArgs e)
        {
            var ipText = txtIpReverse.Text.Trim();
            if (!IPAddress.TryParse(ipText, out var ip))
            {
                MessageBox.Show("Invalid IP address", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            rtbReverseResults.Clear();
            rtbReverseResults.AppendText($"🔍 Reverse DNS Lookup\r\n");
            rtbReverseResults.AppendText($"Target: {ip}\r\n");
            rtbReverseResults.AppendText($"Time: {DateTime.Now:HH:mm:ss}\r\n");
            rtbReverseResults.AppendText("─".PadRight(50, '─') + "\r\n\r\n");

            try
            {
                var client = CreateLookupClient();
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var result = await client.QueryReverseAsync(ip);
                sw.Stop();

                var ptrRecords = result.Answers.OfType<DnsClient.Protocol.PtrRecord>().ToList();

                if (ptrRecords.Count == 0)
                {
                    rtbReverseResults.AppendText("❌ No reverse DNS records found\r\n\r\n");
                    rtbReverseResults.AppendText($"Technical details:\r\n");
                    rtbReverseResults.AppendText($"• Query: {GetReverseQueryFormat(ip)}\r\n");
                    rtbReverseResults.AppendText($"• DNS Server: {(CustomDnsServer?.ToString() ?? "System Default")}\r\n");
                    rtbReverseResults.AppendText($"• Response time: {sw.ElapsedMilliseconds}ms\r\n");
                }
                else
                {
                    rtbReverseResults.AppendText($"✅ Found {ptrRecords.Count} PTR record(s):\r\n\r\n");

                    foreach (var ptr in ptrRecords)
                    {
                        var hostname = ptr.PtrDomainName?.ToString() ?? "Unknown";
                        rtbReverseResults.AppendText($"📌 Hostname: {hostname}\r\n");
                        rtbReverseResults.AppendText($"   • Domain: {ptr.DomainName}\r\n");
                        rtbReverseResults.AppendText($"   • TTL: {ptr.TimeToLive} seconds\r\n");
                        rtbReverseResults.AppendText("\r\n");
                    }

                    rtbReverseResults.AppendText($"📊 Summary:\r\n");
                    rtbReverseResults.AppendText($"• Query time: {sw.ElapsedMilliseconds}ms\r\n");
                    rtbReverseResults.AppendText($"• DNS Server: {(CustomDnsServer?.ToString() ?? "System Default")}\r\n");
                }

                rtbReverseResults.AppendText("─".PadRight(50, '─') + "\r\n");
                rtbReverseResults.AppendText($"Completed at {DateTime.Now:HH:mm:ss}\r\n");

                SaveToHistory("PTR", ip.ToString(), sw.ElapsedMilliseconds,
                    ptrRecords.Count > 0 ?
                    string.Join(", ", ptrRecords.Select(p => p.PtrDomainName?.ToString())) :
                    "No PTR records");
                RefreshHistory();
            }
            catch (Exception ex)
            {
                rtbReverseResults.AppendText($"\r\n❌ Error: {ex.Message}\r\n");
            }
        }

        // Hàm hỗ trợ BtnReverseIp_Click
        private string GetReverseQueryFormat(IPAddress ip)
        {
            var bytes = ip.GetAddressBytes();
            Array.Reverse(bytes); 
            return $"{string.Join(".", bytes)}.in-addr.arpa";
        }

        private async void BtnQueryMulti_Click(object? sender, EventArgs e)
        {
            var query = txtQueryMulti.Text.Trim();
            if (string.IsNullOrWhiteSpace(query))
            {
                MessageBox.Show("Please enter a query", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var recordTypeStr = cmbRecordType.SelectedItem?.ToString() ?? "A";
            var recordType = GetQueryType(recordTypeStr);

            rtbMultiResults.Clear();
            rtbMultiResults.AppendText("Querying...\r\n");

            try
            {
                var client = CreateLookupClient();
                var sw = System.Diagnostics.Stopwatch.StartNew();

                // Nếu là IP và loại record là PTR, thực hiện reverse lookup
                if (IPAddress.TryParse(query, out var ipAddress))
                {
                    if (recordType == QueryType.PTR)
                    {
                        var result = await client.QueryReverseAsync(ipAddress);
                        sw.Stop();

                        rtbMultiResults.Clear();
                        if (result.Answers.Count == 0)
                        {
                            rtbMultiResults.AppendText("No PTR records found\r\n");
                            return;
                        }

                        foreach (var answer in result.Answers.OfType<DnsClient.Protocol.PtrRecord>())
                        {
                            rtbMultiResults.AppendText($"• PTR: {answer.PtrDomainName}\r\n");
                        }
                    }
                    else
                    {
                        rtbMultiResults.Clear();
                        rtbMultiResults.AppendText($"❌ Error: IP addresses can only be used for PTR lookups\r\n");
                        rtbMultiResults.AppendText($"   Please select 'PTR' record type for IP addresses\r\n");
                        return;
                    }
                }
                else
                {
                    // Nếu là domain, thực hiện lookup bình thường
                    var result = await client.QueryAsync(query, recordType);
                    sw.Stop();

                    rtbMultiResults.Clear();
                    if (result.Answers.Count == 0)
                    {
                        rtbMultiResults.AppendText("No records found\r\n");
                        return;
                    }

                    foreach (var answer in result.Answers)
                    {
                        string recordStr = FormatRecord(recordType, answer);
                        rtbMultiResults.AppendText($"• {recordStr}\r\n");
                    }
                }

                rtbMultiResults.AppendText($"\r\nTime: {sw.ElapsedMilliseconds}ms\r\n");
                SaveToHistory(recordTypeStr, query, sw.ElapsedMilliseconds, "");
                RefreshHistory();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Query Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnProcessBatch_Click(object? sender, EventArgs e)
        {
            var filePath = txtBatchFile.Text.Trim();
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                MessageBox.Show("Please select a valid file", "File Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            rtbBatchResults.Clear();
            rtbBatchResults.AppendText("Starting batch process...\r\n\r\n");

            try
            {
                var lines = (await File.ReadAllLinesAsync(filePath))
                    .Select(l => l.Trim())
                    .Where(l => !string.IsNullOrWhiteSpace(l))
                    .ToArray();

                progressBatch.Maximum = lines.Length;
                progressBatch.Value = 0;

                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    try
                    {
                        var client = CreateLookupClient();
                        var sw = System.Diagnostics.Stopwatch.StartNew();
                        var res = await client.QueryAsync(line, QueryType.A);
                        sw.Stop();

                        var addrs = res.Answers.OfType<DnsClient.Protocol.ARecord>()
                            .Select(r => r.Address.ToString())
                            .ToArray();

                        string result = addrs.Length > 0 
                            ? string.Join(", ", addrs)
                            : "No results";

                        rtbBatchResults.AppendText($"✓ {line} => {result} ({sw.ElapsedMilliseconds}ms)\r\n");
                        SaveToHistory("A", line, sw.ElapsedMilliseconds, result);
                    }
                    catch (Exception ex)
                    {
                        rtbBatchResults.AppendText($"✗ {line} => Error: {ex.Message}\r\n");
                    }

                    progressBatch.Value = i + 1;
                    Application.DoEvents();
                }

                rtbBatchResults.AppendText("\r\nBatch process completed!");
                RefreshHistory();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Batch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnConfigDns_Click(object? sender, EventArgs e)
        {
            var form = new DnsConfigForm { DnsServer = CustomDnsServer };
            if (form.ShowDialog() == DialogResult.OK)
            {
                CustomDnsServer = form.DnsServer;
                lblDnsServer.Text = $"DNS Server: {(CustomDnsServer?.ToString() ?? "Default")}";
            }
        }

        private void BtnBrowseBatch_Click(object? sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog { Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtBatchFile.Text = ofd.FileName;
                }
            }
        }

        private void BtnExportResults_Click(object? sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt|CSV Files (*.csv)|*.csv|JSON Files (*.json)|*.json|HTML Report (*.html)|*.html",
                Title = "Export Results"
            })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var ext = Path.GetExtension(sfd.FileName).ToLower();

                        switch (ext)
                        {
                            case ".csv":
                                var csvLines = QueryHistory.Select(h =>
                                    $"\"{h.Type}\",\"{h.Query}\",\"{h.Timestamp:yyyy-MM-dd HH:mm:ss}\",{h.ElapsedMs},\"{h.Details}\"");
                                File.WriteAllLines(sfd.FileName, new[] { "Type,Query,Timestamp,ElapsedMs,Details" }.Concat(csvLines));
                                break;

                            case ".json":
                                var json = JsonConvert.SerializeObject(QueryHistory, Formatting.Indented);
                                File.WriteAllText(sfd.FileName, json);
                                break;

                            case ".html":
                                GenerateHtmlReport(sfd.FileName);
                                break;

                            default: // .txt
                                var lines = QueryHistory.Select(h =>
                                    $"{h.Type}\t{h.Query}\t{h.Timestamp:yyyy-MM-dd HH:mm:ss}\t{h.ElapsedMs}ms\t{h.Details}");
                                File.WriteAllLines(sfd.FileName, lines);
                                break;
                        }

                        MessageBox.Show($"Export successful to {Path.GetFileName(sfd.FileName)}",
                            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // Xuất báo cáo dưới dạng Html
        private void GenerateHtmlReport(string filePath)
        {
            var htmlTemplate = @"<!DOCTYPE html>
                <html>
                <head>
                    <title>DNS Lookup Report</title>
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 40px; }}
                        table {{ border-collapse: collapse; width: 100%; }}
                        th, td {{ border: 1px solid #ddd; padding: 12px; text-align: left; }}
                        th {{ background-color: #4CAF50; color: white; }}
                        tr:nth-child(even) {{ background-color: #f2f2f2; }}
                        .summary {{ background-color: #e7f3fe; padding: 20px; margin-bottom: 20px; }}
                        .timestamp {{ color: #666; font-size: 12px; }}
                    </style>
                </head>
                <body>
                    <h1>DNS Lookup Tool Report</h1>
                    <div class='summary'>
                        <strong>Generated:</strong> {0:yyyy-MM-dd HH:mm:ss}<br>
                        <strong>Total Queries:</strong> {1}<br>
                        <strong>Period:</strong> {2:yyyy-MM-dd} to {3:yyyy-MM-dd}
                    </div>
                    <table>
                        <tr>
                            <th>Type</th><th>Query</th><th>Timestamp</th><th>Time (ms)</th><th>Details</th>
                        </tr>
                        {4}
                    </table>
                </body>
                </html>";

            var rows = string.Join("", QueryHistory.Select(h => $@"
                <tr>
                    <td>{h.Type}</td>
                    <td>{h.Query}</td>
                    <td>{h.Timestamp:yyyy-MM-dd HH:mm:ss}</td>
                    <td>{h.ElapsedMs}</td>
                    <td>{h.Details}</td>
                </tr>"));

            var html = string.Format(htmlTemplate,
                DateTime.Now,
                QueryHistory.Count,
                QueryHistory.Min(h => h.Timestamp),
                QueryHistory.Max(h => h.Timestamp),
                rows);

            File.WriteAllText(filePath, html);
        }

        private void BtnGenerateReport_Click(object? sender, EventArgs e)
        {
            if (QueryHistory.Count == 0)
            {
                MessageBox.Show("No history to generate report", "Empty History", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("DNS Lookup Tool - Report");
                sb.AppendLine($"Generated: {DateTime.Now}");
                sb.AppendLine($"Total Queries: {QueryHistory.Count}\r\n");

                var byType = QueryHistory.GroupBy(h => h.Type)
                    .OrderByDescending(g => g.Count());

                sb.AppendLine("Statistics by Type:");
                foreach (var g in byType)
                {
                    var avg = g.Where(x => x.ElapsedMs >= 0).DefaultIfEmpty().Average(x => x.ElapsedMs);
                    sb.AppendLine($"  {g.Key}: {g.Count()} queries, avg {avg:F1}ms");
                }

                sb.AppendLine("\nTop 10 Queries:");
                var topQueries = QueryHistory.GroupBy(h => h.Query)
                    .OrderByDescending(g => g.Count())
                    .Take(10);
                foreach (var g in topQueries)
                {
                    sb.AppendLine($"  {g.Key}: {g.Count()} times");
                }

                var reportPath = "report.txt";
                File.WriteAllText(reportPath, sb.ToString());
                MessageBox.Show($"Report generated: {Path.GetFullPath(reportPath)}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Report generation failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Helper Methods
        private QueryType GetQueryType(string typeStr)
        {
            return typeStr switch
            {
                "AAAA" => QueryType.AAAA,
                "PTR" => QueryType.PTR,
                "MX" => QueryType.MX,
                "CNAME" => QueryType.CNAME,
                "TXT" => QueryType.TXT,
                "NS" => QueryType.NS,
                "SOA" => QueryType.SOA,
                _ => QueryType.A
            };
        }

        private string FormatRecord(QueryType type, DnsClient.Protocol.DnsResourceRecord answer)
        {
            return type switch
            {
                QueryType.A => $"A: {((DnsClient.Protocol.ARecord)answer).Address}",
                QueryType.AAAA => $"AAAA: {((DnsClient.Protocol.AaaaRecord)answer).Address}",
                QueryType.PTR => $"PTR: {((DnsClient.Protocol.PtrRecord)answer).PtrDomainName}",
                QueryType.MX => $"MX: {((DnsClient.Protocol.MxRecord)answer).Exchange} (Preference: {((DnsClient.Protocol.MxRecord)answer).Preference})",
                QueryType.CNAME => $"CNAME: {((DnsClient.Protocol.CNameRecord)answer).CanonicalName}",
                QueryType.TXT => $"TXT: {string.Join(" ", ((DnsClient.Protocol.TxtRecord)answer).Text)}",
                QueryType.NS => $"NS: {((DnsClient.Protocol.NsRecord)answer).NSDName}",
                QueryType.SOA => $"SOA: {((DnsClient.Protocol.SoaRecord)answer).MName}",
                _ => "Unknown record type"
            };
        }

        private LookupClient CreateLookupClient()
        {
            try
            {
                var options = CustomDnsServer != null
                    ? new LookupClientOptions(CustomDnsServer)
                    {
                        Timeout = Timeout,
                        Retries = RetryCount,
                        UseTcpOnly = ForceTcpOnly,
                        EnableAuditTrail = true
                    }
                    : new LookupClientOptions
                    {
                        Timeout = Timeout,
                        Retries = RetryCount,
                        UseTcpOnly = ForceTcpOnly,
                        EnableAuditTrail = true
                    };

                return new LookupClient(options);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create DNS client: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        private void SaveToHistory(string type, string query, long elapsedMs, string details)
        {
            QueryHistory.Add(new HistoryEntry
            {
                Type = type,
                Query = query,
                Timestamp = DateTime.Now,
                ElapsedMs = elapsedMs,
                Details = details
            });
        }

        private void RefreshHistory()
        {
            dgvHistory.Rows.Clear();
            foreach (var entry in QueryHistory.OrderByDescending(h => h.Timestamp).Take(100))
            {
                dgvHistory.Rows.Add(entry.Type, entry.Query, entry.Timestamp.ToString("g"), entry.ElapsedMs);
            }
        }

        private void SaveHistory()
        {
            try
            {
                var json = JsonConvert.SerializeObject(QueryHistory);
                File.WriteAllText("history.json", json);
            }
            catch { }
        }

        private void LoadHistory()
        {
            try
            {
                if (File.Exists("history.json"))
                {
                    var json = File.ReadAllText("history.json");
                    QueryHistory = JsonConvert.DeserializeObject<List<HistoryEntry>>(json) ?? new List<HistoryEntry>();
                }
            }
            catch
            {
                QueryHistory = new List<HistoryEntry>();
            }
        }

        private void SaveSecuritySettings()
        {
            try
            {
                var obj = new { ForceTcpOnly, EnableDnsSec };
                var json = JsonConvert.SerializeObject(obj, JsonFormatting.Indented);
                File.WriteAllText(SecuritySettingsFile, json);
            }
            catch { }
        }

        private void LoadSecuritySettings()
        {
            try
            {
                if (File.Exists(SecuritySettingsFile))
                {
                    var json = File.ReadAllText(SecuritySettingsFile);
                    var node = JsonConvert.DeserializeObject<dynamic>(json);
                    if (node != null)
                    {
                        ForceTcpOnly = node.ForceTcpOnly == true;
                        EnableDnsSec = node.EnableDnsSec == true;
                    }
                }
            }
            catch
            {
                ForceTcpOnly = false;
                EnableDnsSec = false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            SaveHistory();
            SaveSecuritySettings();
            base.Dispose(disposing);
        }

        private async Task<string> QueryWhois(string domain)
        {
            try
            {
                using var client = new System.Net.Sockets.TcpClient();
                await client.ConnectAsync("whois.iana.org", 43);

                using var stream = client.GetStream();
                using var writer = new StreamWriter(stream);
                using var reader = new StreamReader(stream);

                await writer.WriteLineAsync(domain);
                await writer.FlushAsync();

                var result = await reader.ReadToEndAsync();
                return result;
            }
            catch
            {
                return "Whois lookup failed";
            }
        }

        // Tab 8
        private void CreateWhoisTab()
        {
            var tabPage = new TabPage { Text = "Whois Lookup", AutoScroll = true };
            tabPage.BackColor = Color.White;

            var lblInput = new Label
            {
                Text = "Domain or IP for Whois:",
                Location = new Point(20, 20),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true
            };
            tabPage.Controls.Add(lblInput);

            var txtWhoisQuery = new TextBox
            {
                Location = new Point(20, 50),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 11),
                Text = "example.com"
            };
            tabPage.Controls.Add(txtWhoisQuery);

            var btnWhois = new Button
            {
                Text = "🔍 Whois",
                Location = new Point(330, 50),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnWhois.Click += async (s, e) =>
            {
                var query = txtWhoisQuery.Text.Trim();
                if (string.IsNullOrWhiteSpace(query))
                {
                    MessageBox.Show("Please enter a domain or IP", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var whoisResult = new RichTextBox
                {
                    Location = new Point(20, 90),
                    Size = new Size(500, 350),
                    Font = new Font("Consolas", 9),
                    ReadOnly = true
                };

                // Xoa ban ghi truoc neu da ton tai
                var existing = tabPage.Controls.OfType<RichTextBox>().FirstOrDefault(r => r.Name == "whoisResult");
                if (existing != null) tabPage.Controls.Remove(existing);

                whoisResult.Name = "whoisResult";
                tabPage.Controls.Add(whoisResult);

                whoisResult.AppendText($"Querying Whois for: {query}\r\n");
                whoisResult.AppendText("=".PadRight(60, '=') + "\r\n");

                try
                {
                    var result = await QueryWhois(query);
                    whoisResult.AppendText(result);

                    // Lưu vào lịch sử
                    SaveToHistory("WHOIS", query, 0, $"Whois lookup for {query}");
                    RefreshHistory();
                }
                catch (Exception ex)
                {
                    whoisResult.AppendText($"❌ Error: {ex.Message}\r\n");
                }
            };
            tabPage.Controls.Add(btnWhois);

            tabControl.TabPages.Add(tabPage);
        }
    }

    public class HistoryEntry
    {
        public string Type { get; set; } = "";
        public string Query { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public long ElapsedMs { get; set; }
        public string Details { get; set; } = "";
    }
}
