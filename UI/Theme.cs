using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace subs_check.win.gui
{
    /// <summary>
    /// UI 主题：配色系统 + 控件统一着色 + 轻 GDI+ 自绘
    /// 深色/浅色双模式，主色科技蓝 #3B82F6
    /// </summary>
    public static class Theme
    {
        // ===== 语义色结构 =====
        public struct Palette
        {
            public Color WindowBack;    // 窗体背景
            public Color CardBack;      // 卡片/GroupBox 背景
            public Color CardBorder;    // 卡片边框
            public Color InputBack;     // 输入框背景
            public Color InputBorder;   // 输入框边框
            public Color InputFocus;    // 输入框聚焦描边
            public Color TextPrimary;   // 主文字
            public Color TextSecondary; // 次要文字
            public Color Primary;       // 品牌主色
            public Color PrimaryHover;  // 主色悬停
            public Color PrimaryPressed;// 主色按下
            public Color Success;       // 成功
            public Color Warning;       // 警告
            public Color Danger;        // 危险
            public Color LogBack;       // 日志区背景
            public Color Divider;       // 分隔线
        }

        public static bool IsDark { get; private set; } = true;

        public static Palette Current;

        public static readonly Color AccentBlue = ColorTranslator.FromHtml("#3B82F6");

        private static Color FromHex(string hex) => ColorTranslator.FromHtml(hex);

        static Theme()
        {
            ApplyDark();
        }

        public static void ApplyDark()
        {
            IsDark = true;
            Current = new Palette
            {
                WindowBack     = FromHex("#0F1115"),
                CardBack       = FromHex("#171A21"),
                CardBorder     = FromHex("#262B36"),
                InputBack      = FromHex("#1F2430"),
                InputBorder    = FromHex("#2E3542"),
                InputFocus     = AccentBlue,
                TextPrimary    = FromHex("#E5E7EB"),
                TextSecondary  = FromHex("#9CA3AF"),
                Primary        = AccentBlue,
                PrimaryHover   = FromHex("#60A5FA"),
                PrimaryPressed = FromHex("#2563EB"),
                Success        = FromHex("#22C55E"),
                Warning        = FromHex("#F59E0B"),
                Danger         = FromHex("#EF4444"),
                LogBack        = FromHex("#12151C"),
                Divider        = FromHex("#262B36"),
            };
        }

        public static void ApplyLight()
        {
            IsDark = false;
            Current = new Palette
            {
                WindowBack     = FromHex("#F5F6F8"),
                CardBack       = Color.White,
                CardBorder     = FromHex("#E2E5EA"),
                InputBack      = Color.White,
                InputBorder    = FromHex("#D2D6DE"),
                InputFocus     = AccentBlue,
                TextPrimary    = FromHex("#1F2937"),
                TextSecondary  = FromHex("#6B7280"),
                Primary        = AccentBlue,
                PrimaryHover   = FromHex("#2563EB"),
                PrimaryPressed = FromHex("#1D4ED8"),
                Success        = FromHex("#16A34A"),
                Warning        = FromHex("#D97706"),
                Danger         = FromHex("#DC2626"),
                LogBack        = FromHex("#FAFBFC"),
                Divider        = FromHex("#E2E5EA"),
            };
        }

        public static void Toggle()
        {
            if (IsDark) ApplyLight(); else ApplyDark();
        }

        // ===== 字体体系（全局统一微软雅黑） =====
        public static Font FontBody      = new Font("微软雅黑", 9F, FontStyle.Regular);
        public static Font FontBodyBold  = new Font("微软雅黑", 9F, FontStyle.Bold);
        public static Font FontTitle     = new Font("微软雅黑", 10.5F, FontStyle.Bold);
        public static Font FontLog       = new Font("Consolas", 9F, FontStyle.Regular);
        public static Font FontButton    = new Font("微软雅黑", 9F, FontStyle.Regular);

        // ===== 统一应用主题到整个控件树 =====
        public static void Apply(Control root, bool dark = true)
        {
            if (dark) ApplyDark(); else ApplyLight();

            ApplyRecursive(root);
        }

        public static void ApplyRecursive(Control root)
        {
            var p = Current;

            if (root is Form form)
            {
                form.BackColor = p.WindowBack;
                form.Font = FontBody;
                form.ForeColor = p.TextPrimary;
            }
            else if (root is GroupBox gb)
            {
                gb.BackColor = p.WindowBack; // 卡片由绘制器画在窗体底色上
                gb.ForeColor = p.TextPrimary;
                gb.Font = FontTitle;
                gb.Paint += GroupBoxCard_Paint;
            }
            else if (root is Button btn)
            {
                StyleButton(btn);
            }
            else if (root is TextBoxBase tb)
            {
                tb.BackColor = p.InputBack;
                tb.ForeColor = p.TextPrimary;
                tb.BorderStyle = BorderStyle.FixedSingle;
                tb.Font = (root is RichTextBox) ? FontLog : FontBody;
                tb.Enter += (s, e) => { tb.BackColor = p.InputBack; tb.BorderStyle = BorderStyle.FixedSingle; };
            }
            else if (root is ComboBox cb)
            {
                cb.BackColor = p.InputBack;
                cb.ForeColor = p.TextPrimary;
                cb.FlatStyle = FlatStyle.Flat;
            }
            else if (root is CheckBox chk)
            {
                chk.BackColor = p.CardBack;
                chk.ForeColor = p.TextPrimary;
            }
            else if (root is Label lbl)
            {
                lbl.BackColor = p.CardBack;
                lbl.ForeColor = p.TextSecondary;
            }
            else if (root is LinkLabel ll)
            {
                ll.LinkColor = p.Primary;
                ll.ActiveLinkColor = p.PrimaryHover;
            }
            else if (root is NumericUpDown nud)
            {
                nud.BackColor = p.InputBack;
                nud.ForeColor = p.TextPrimary;
                nud.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (root is ProgressBar pb)
            {
                ApplyProgressBar(pb);
            }
            else if (root is TabControl tc)
            {
                StyleTabControl(tc);
            }
            else if (root is TabPage tp)
            {
                tp.BackColor = p.WindowBack;
                tp.ForeColor = p.TextPrimary;
            }

            foreach (Control child in root.Controls)
                ApplyRecursive(child);
        }

        // ===== 按钮样式 =====
        public static void StyleButton(Button btn, string variant = "default")
        {
            var p = Current;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = FontButton;
            btn.Cursor = Cursors.Hand;
            btn.UseVisualStyleBackColor = false;
            btn.Tag = variant;

            switch (variant)
            {
                case "primary":
                    btn.BackColor = p.Primary;
                    btn.ForeColor = Color.White;
                    break;
                case "danger":
                    btn.BackColor = p.Danger;
                    btn.ForeColor = Color.White;
                    break;
                case "success":
                    btn.BackColor = p.Success;
                    btn.ForeColor = Color.White;
                    break;
                default: // 次要：描边灰底
                    btn.BackColor = p.InputBack;
                    btn.ForeColor = p.TextPrimary;
                    btn.FlatAppearance.BorderSize = 1;
                    btn.FlatAppearance.BorderColor = p.InputBorder;
                    break;
            }

            // 重挂 hover/按下事件（避免重复挂）
            btn.MouseEnter -= Btn_Hover;
            btn.MouseLeave -= Btn_Leave;
            btn.MouseDown -= Btn_Down;
            btn.MouseUp -= Btn_Hover;
            btn.MouseEnter += Btn_Hover;
            btn.MouseLeave += Btn_Leave;
            btn.MouseDown += Btn_Down;
            btn.MouseUp += Btn_Hover;
        }

        private static void Btn_Hover(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            var v = (btn.Tag as string) ?? "default";
            var p = Current;
            if (v == "primary") btn.BackColor = p.PrimaryHover;
            else if (v == "danger") btn.BackColor = FromHex("#F87171");
            else if (v == "success") btn.BackColor = FromHex("#4ADE80");
            else btn.BackColor = p.CardBorder;
        }

        private static void Btn_Down(object sender, MouseEventArgs e)
        {
            var btn = (Button)sender;
            var v = (btn.Tag as string) ?? "default";
            var p = Current;
            if (v == "primary") btn.BackColor = p.PrimaryPressed;
            else if (v == "danger") btn.BackColor = FromHex("#DC2626");
            else if (v == "success") btn.BackColor = FromHex("#16A34A");
            else btn.BackColor = p.InputBorder;
        }

        private static void Btn_Leave(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            var v = (btn.Tag as string) ?? "default";
            var p = Current;
            if (v == "primary") btn.BackColor = p.Primary;
            else if (v == "danger") btn.BackColor = p.Danger;
            else if (v == "success") btn.BackColor = p.Success;
            else btn.BackColor = p.InputBack;
        }

        // ===== GroupBox → 圆角卡片绘制 =====
        private static void GroupBoxCard_Paint(object sender, PaintEventArgs e)
        {
            var gb = (GroupBox)sender;
            var p = Current;
            e.Graphics.Clear(p.WindowBack);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (var path = RoundedRect(new Rectangle(0, 0, gb.Width - 1, gb.Height - 1), 8))
            using (var fill = new SolidBrush(p.CardBack))
            using (var pen = new Pen(p.CardBorder, 1))
            {
                e.Graphics.FillPath(fill, path);
                e.Graphics.DrawPath(pen, path);
            }

            // 标题：主色小圆点 + 文本，替代原生分组框线
            TextRenderer.DrawText(e.Graphics, gb.Text, FontTitle,
                new Point(12, 6), p.TextPrimary);

            var dotColor = p.Primary;
            using (var b = new SolidBrush(dotColor))
                e.Graphics.FillEllipse(b, 4, 12, 5, 5);
        }

        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        // ===== 进度条：主色细条 =====
        public static void ApplyProgressBar(ProgressBar pb)
        {
            var p = Current;
            pb.Style = ProgressBarStyle.Continuous;
            // 借 RichTextBox 不必要——直接用 CreateParams 覆写太重，改用自绘 owner 风格
            // 简化：只改 Fore/Back，Windows 10+ 下 VisualStyle 会取系统色，这里关闭视觉样式让它生效
            pb.ForeColor = p.Primary;
            pb.BackColor = p.InputBack;
        }

        // ===== TabControl 深色化 =====
        [System.Runtime.InteropServices.DllImport("uxtheme.dll", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hWnd, string subAppName, string subIdList);

        public static void StyleTabControl(TabControl tc)
        {
            tc.Appearance = TabAppearance.FlatButtons;
            tc.ItemSize = new Size(90, 28);
            tc.SizeMode = TabSizeMode.Fixed;
            tc.DrawMode = TabDrawMode.OwnerDrawFixed;
            tc.Padding = new Point(24, 8);
            tc.BackColor = Current.WindowBack;
            // 去掉系统视觉样式，否则 Tab 头部条强制浅灰、忽略 BackColor
            tc.HandleCreated += (s, e) => SetWindowTheme(tc.Handle, "", "");
            if (tc.IsHandleCreated) SetWindowTheme(tc.Handle, "", "");
            tc.DrawItem += (s, e) =>
            {
                var p = Current;
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                string title = tc.TabPages[e.Index].Text;
                bool selected = e.Index == tc.SelectedIndex;

                using (var bg = new SolidBrush(p.WindowBack))
                    g.FillRectangle(bg, e.Bounds);
                if (selected)
                    using (var selBg = new SolidBrush(p.CardBack))
                        g.FillRectangle(selBg, e.Bounds);
                TextRenderer.DrawText(g, title, selected ? FontBodyBold : FontBody,
                    e.Bounds, selected ? p.Primary : p.TextSecondary,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                if (selected)
                    using (var pen = new Pen(p.Primary, 2))
                        g.DrawLine(pen, e.Bounds.Left + 8, e.Bounds.Bottom - 2, e.Bounds.Right - 8, e.Bounds.Bottom - 2);
            };
            // 头部剩余区域（选中 Tab 之后到控件右端）也刷成深色
            tc.Paint += (s, e) =>
            {
                var p = Current;
                // TabControl 头部高度 = ItemSize.Height + 边框偏移，取保守值 40
                using (var bg = new SolidBrush(p.WindowBack))
                    e.Graphics.FillRectangle(bg, tc.ClientSize.Width - 10, 0, 10, tc.ItemSize.Height + 6);
            };
        }
    }
}
