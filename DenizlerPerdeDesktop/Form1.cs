using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace DenizlerPerdeDesktop;

public partial class Form1 : Form
{
    private const string SiteUrl = "https://denizlerperde-hpkf.onrender.com";

    private static readonly Color Brand = Color.FromArgb(111, 83, 67);
    private static readonly Color BrandDeep = Color.FromArgb(45, 34, 30);
    private static readonly Color Cream = Color.FromArgb(250, 247, 243);
    private static readonly Color Soft = Color.FromArgb(242, 233, 224);
    private static readonly Color Accent = Color.FromArgb(244, 221, 196);
    private static readonly Color Line = Color.FromArgb(226, 214, 204);

    private readonly WebView2 _webView = new();
    private readonly Label _statusLabel = new();
    private readonly Label _pageLabel = new();
    private readonly Panel _loadingOverlay = new();
    private readonly List<NavItem> _navItems = new();
    private NavItem? _logoutNavItem;
    private readonly TableLayoutPanel _root = new();
    private int _renderRetryCount;
    private bool _sidebarCollapsed;

    public Form1()
    {
        InitializeComponent();
        BuildInterface();
        Shown += async (_, _) => await InitializeBrowserAsync();
    }

    private void BuildInterface()
    {
        Text = "Denizler Perde & Tasarım";
        SetAppIcon();
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1180, 780);
        WindowState = FormWindowState.Maximized;
        BackColor = Cream;
        Font = new Font("Segoe UI", 9);

        _root.Dock = DockStyle.Fill;
        _root.ColumnCount = 2;
        _root.BackColor = Cream;
        _root.Padding = new Padding(8);
        _root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 0));
        _sidebarCollapsed = true;
        _root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _root.Controls.Add(BuildSidebar(), 0, 0);
        _root.Controls.Add(BuildWorkspace(), 1, 0);

        Controls.Add(_root);
    }

    private Control BuildSidebar()
    {
        var sidebar = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            Radius = 28,
            BackColor = BrandDeep,
            Padding = new Padding(24, 26, 24, 20),
            Margin = new Padding(0, 0, 10, 0)
        };

        var brandBox = new Panel { Dock = DockStyle.Top, Height = 132, BackColor = BrandDeep };
        brandBox.Controls.Add(new RoundLabel
        {
            Text = "DP",
            Location = new Point(0, 4),
            Size = new Size(58, 58),
            Radius = 18,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = BrandDeep,
            BackColor = Accent,
            Font = new Font("Segoe UI", 16, FontStyle.Bold)
        });
        brandBox.Controls.Add(new Label
        {
            Text = "Denizler Perde",
            AutoSize = true,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            Location = new Point(0, 72)
        });
        brandBox.Controls.Add(new Label
        {
            Text = "Satış ve randevu sistemi",
            AutoSize = true,
            ForeColor = Color.FromArgb(221, 205, 194),
            Font = new Font("Segoe UI", 9),
            Location = new Point(2, 102)
        });

        var nav = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 560,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = BrandDeep,
            Padding = new Padding(0, 18, 0, 0)
        };

        nav.Controls.Add(CreateNavItem("Ana Sayfa", "Genel görünüm", "/", true));
        nav.Controls.Add(CreateNavItem("Satış", "Ürünler ve siparişler", "/Satis"));
        nav.Controls.Add(CreateNavItem("Ürünler", "Detaylı katalog", "/Products"));
        nav.Controls.Add(CreateNavItem("Sepet", "Alışveriş listesi", "/Sepet"));
        nav.Controls.Add(CreateNavItem("Randevu", "Ölçü ve montaj", "/Randevu"));
        nav.Controls.Add(CreateNavItem("Giriş", "Admin / çalışan / müşteri", "/Auth/Login"));
        _logoutNavItem = CreateNavItem("Çıkış Yap", "Oturumu kapat", "/Auth/Logout", false, true);
        _logoutNavItem.Visible = false;
        nav.Controls.Add(_logoutNavItem);

        sidebar.Controls.Add(nav);
        sidebar.Controls.Add(brandBox);
        return sidebar;
    }

    private Control BuildWorkspace()
    {
        var workspace = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            BackColor = Cream,
            Padding = new Padding(24)
        };
        workspace.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
        workspace.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        workspace.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));

        var topbar = new Panel { Dock = DockStyle.Fill, BackColor = Cream };

        var menuButton = CreateIconButton(NavIcon.Menu, "Menüyü aç/kapat", (_, _) => ToggleSidebar());
        menuButton.Location = new Point(0, 16);
        topbar.Controls.Add(menuButton);

        _pageLabel.Text = "Ana Sayfa";
        _pageLabel.AutoSize = true;
        _pageLabel.ForeColor = BrandDeep;
        _pageLabel.Font = new Font("Segoe UI", 21, FontStyle.Bold);
        _pageLabel.Location = new Point(58, 8);
        topbar.Controls.Add(_pageLabel);

        topbar.Controls.Add(new Label
        {
            Text = "Site dışarı açılmadan bu pencerenin içinde çalışır.",
            AutoSize = true,
            ForeColor = Color.FromArgb(105, 96, 89),
            Font = new Font("Segoe UI", 10),
            Location = new Point(60, 50)
        });

        var quick = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            Width = 160,
            FlowDirection = FlowDirection.RightToLeft,
            BackColor = Cream,
            Padding = new Padding(0, 14, 0, 0)
        };
        quick.Controls.Add(CreateIconButton(NavIcon.Refresh, "Yenile", (_, _) => Reload()));
        quick.Controls.Add(CreateIconButton(NavIcon.Forward, "İleri", (_, _) => { if (_webView.CanGoForward) _webView.GoForward(); }));
        quick.Controls.Add(CreateIconButton(NavIcon.Back, "Geri", (_, _) => { if (_webView.CanGoBack) _webView.GoBack(); }));
        topbar.Controls.Add(quick);

        var frame = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            Radius = 18,
            BackColor = Color.White,
            Padding = new Padding(1)
        };

        _webView.Dock = DockStyle.Fill;
        _webView.DefaultBackgroundColor = Color.White;
        _webView.NavigationStarting += (_, _) => ShowLoading("Canlı site hazırlanıyor...");
        _webView.NavigationCompleted += async (_, args) => await HandleNavigationCompletedAsync(args);

        _loadingOverlay.Dock = DockStyle.Fill;
        _loadingOverlay.BackColor = Color.White;
        _loadingOverlay.Visible = true;
        BuildLoadingOverlay();

        frame.Controls.Add(_webView);
        frame.Controls.Add(_loadingOverlay);
        _loadingOverlay.BringToFront();

        _statusLabel.Dock = DockStyle.Fill;
        _statusLabel.ForeColor = Brand;
        _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        _statusLabel.Text = "Uygulama hazırlanıyor...";

        workspace.Controls.Add(topbar, 0, 0);
        workspace.Controls.Add(frame, 0, 1);
        workspace.Controls.Add(_statusLabel, 0, 2);
        return workspace;
    }

    private void BuildLoadingOverlay()
    {
        _loadingOverlay.Controls.Clear();

        var box = new RoundedPanel
        {
            Size = new Size(430, 178),
            Radius = 24,
            BackColor = Cream,
            Anchor = AnchorStyles.None
        };
        box.Location = new Point((_loadingOverlay.Width - box.Width) / 2, (_loadingOverlay.Height - box.Height) / 2);
        _loadingOverlay.Resize += (_, _) =>
        {
            box.Location = new Point((_loadingOverlay.Width - box.Width) / 2, (_loadingOverlay.Height - box.Height) / 2);
        };

        box.Controls.Add(new RoundLabel
        {
            Text = "DP",
            Size = new Size(54, 54),
            Location = new Point(32, 30),
            Radius = 18,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.White,
            BackColor = Brand,
            Font = new Font("Segoe UI", 15, FontStyle.Bold)
        });
        box.Controls.Add(new Label
        {
            Text = "Denizler Perde yükleniyor",
            AutoSize = true,
            Location = new Point(104, 34),
            ForeColor = BrandDeep,
            Font = new Font("Segoe UI", 16, FontStyle.Bold)
        });
        box.Controls.Add(new Label
        {
            Text = "Render ücretsiz sunucu olduğu için ilk açılışta kısa süre uyanmasını bekleyebilir.",
            Size = new Size(292, 48),
            Location = new Point(106, 76),
            ForeColor = Color.FromArgb(106, 96, 88),
            Font = new Font("Segoe UI", 9)
        });
        var retryButton = CreateTopPill("Tekrar dene", (_, _) => Reload());
        retryButton.Location = new Point(106, 126);
        box.Controls.Add(retryButton);

        _loadingOverlay.Controls.Add(box);
    }

    private NavItem CreateNavItem(string title, string description, string path, bool active = false, bool danger = false)
    {
        var item = new NavItem(title, description, danger)
        {
            Width = 250,
            Height = 62,
            Margin = new Padding(0, 0, 0, 10),
            Cursor = Cursors.Hand,
            Tag = path
        };

        item.Click += (_, _) => Activate(item, title, path);
        foreach (Control child in item.Controls)
        {
            child.Click += (_, _) => Activate(item, title, path);
            child.Cursor = Cursors.Hand;
        }

        _navItems.Add(item);
        item.SetActive(active);
        return item;
    }

    private void Activate(NavItem item, string title, string path)
    {
        foreach (var navItem in _navItems)
        {
            navItem.SetActive(navItem == item);
        }

        _pageLabel.Text = title;
        Navigate(path);
        CloseSidebar();
    }

    private void ToggleSidebar()
    {
        if (_sidebarCollapsed)
        {
            OpenSidebar();
        }
        else
        {
            CloseSidebar();
        }
    }

    private void OpenSidebar()
    {
        _sidebarCollapsed = false;
        _root.ColumnStyles[0].Width = 306;
    }

    private void CloseSidebar()
    {
        _sidebarCollapsed = true;
        _root.ColumnStyles[0].Width = 0;
    }

    private void UpdateLogoutVisibility()
    {
        var currentPath = _webView.Source?.AbsolutePath ?? "";
        var loggedInArea = currentPath.Contains("/Admin", StringComparison.OrdinalIgnoreCase)
            || currentPath.Contains("/CalisanPanel", StringComparison.OrdinalIgnoreCase)
            || currentPath.Contains("/KullaniciSayfasi", StringComparison.OrdinalIgnoreCase)
            || currentPath.Contains("/Favorilerim", StringComparison.OrdinalIgnoreCase)
            || currentPath.Contains("/HesapAyarlari", StringComparison.OrdinalIgnoreCase);

        if (_logoutNavItem != null)
        {
            _logoutNavItem.Visible = loggedInArea;
        }
    }

    private void CollapseSidebarAfterLogin()
    {
        var currentPath = _webView.Source?.AbsolutePath ?? "";
        var shouldCollapse = currentPath.Contains("/Admin", StringComparison.OrdinalIgnoreCase)
            || currentPath.Contains("/CalisanPanel", StringComparison.OrdinalIgnoreCase)
            || currentPath.Contains("/KullaniciSayfasi", StringComparison.OrdinalIgnoreCase);

        if (shouldCollapse && !_sidebarCollapsed)
        {
            CloseSidebar();
        }
    }

    private static Button CreateTopPill(string text, EventHandler onClick)
    {
        var button = new Button
        {
            Text = text,
            Width = 92,
            Height = 34,
            Margin = new Padding(8, 0, 0, 0),
            FlatStyle = FlatStyle.Flat,
            BackColor = Soft,
            ForeColor = Brand,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Cursor = Cursors.Hand
        };

        button.FlatAppearance.BorderSize = 0;
        button.Click += onClick;
        return button;
    }

    private static Control CreateIconButton(NavIcon icon, string tooltip, EventHandler onClick)
    {
        var button = new IconButton(icon)
        {
            Width = 40,
            Height = 40,
            Margin = new Padding(8, 0, 0, 0),
            Cursor = Cursors.Hand
        };

        button.Click += onClick;
        var tip = new ToolTip { InitialDelay = 250, ReshowDelay = 100 };
        tip.SetToolTip(button, tooltip);
        return button;
    }

    private async Task InitializeBrowserAsync()
    {
        try
        {
            ShowLoading("Canlı site hazırlanıyor...");
            await _webView.EnsureCoreWebView2Async();
            _webView.CoreWebView2.Settings.AreDevToolsEnabled = false;
            _webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
            Navigate("/");
        }
        catch (WebView2RuntimeNotFoundException)
        {
            MessageBox.Show("Bu bilgisayarda Microsoft Edge WebView2 Runtime eksik.", "Denizler Perde", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            OpenInBrowser();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Masaüstü uygulaması açılırken hata oluştu:\n\n" + ex.Message, "Denizler Perde", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task HandleNavigationCompletedAsync(CoreWebView2NavigationCompletedEventArgs args)
    {
        if (!args.IsSuccess)
        {
            ShowLoading("Sayfa yüklenemedi. İnternet bağlantısını kontrol edin.");
            return;
        }

        var bodyText = await _webView.ExecuteScriptAsync("document.body.innerText");
        if (bodyText.Contains("Render", StringComparison.OrdinalIgnoreCase) && _renderRetryCount < 4)
        {
            _renderRetryCount++;
            ShowLoading("Render sunucusu uyanıyor, birazdan tekrar denenecek...");
            await Task.Delay(3500);
            Reload();
            return;
        }

        _renderRetryCount = 0;
        _loadingOverlay.Visible = false;
        _statusLabel.Text = "Hazır";
        UpdateLogoutVisibility();
        CollapseSidebarAfterLogin();
    }

    private void ShowLoading(string message)
    {
        _statusLabel.Text = message;
        _loadingOverlay.Visible = true;
        _loadingOverlay.BringToFront();
    }

    private void Reload()
    {
        _renderRetryCount = 0;
        ShowLoading("Sayfa yenileniyor...");
        _webView.Reload();
    }

    private void Navigate(string path)
    {
        if (_webView.CoreWebView2 == null)
        {
            return;
        }

        ShowLoading("Canlı site hazırlanıyor...");
        _webView.CoreWebView2.Navigate(SiteUrl + path);
    }

    private static void OpenInBrowser()
    {
        Process.Start(new ProcessStartInfo { FileName = SiteUrl, UseShellExecute = true });
    }

    private void SetAppIcon()
    {
        var iconPath = Path.Combine(AppContext.BaseDirectory, "app.ico");
        if (File.Exists(iconPath))
        {
            Icon = new Icon(iconPath);
        }
    }

    private enum NavIcon
    {
        Menu,
        Back,
        Forward,
        Refresh
    }

    private sealed class IconButton : Control
    {
        private readonly NavIcon _icon;
        private bool _hovered;
        private bool _pressed;

        public IconButton(NavIcon icon)
        {
            _icon = icon;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            BackColor = Cream;
            ForeColor = Brand;
            TabStop = false;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _hovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hovered = false;
            _pressed = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            _pressed = true;
            Invalidate();
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _pressed = false;
            Invalidate();
            base.OnMouseUp(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var fillColor = _pressed ? Soft : _hovered ? Color.FromArgb(253, 250, 247) : Color.White;

            using var fill = new SolidBrush(fillColor);
            using var border = new Pen(Line, 1.2f);
            using var path = RoundedRectangle(new Rectangle(0, 0, Width - 1, Height - 1), 8);
            e.Graphics.FillPath(fill, path);
            e.Graphics.DrawPath(border, path);

            using var pen = new Pen(Brand, 2.4f)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };

            var centerY = Height / 2f;
            switch (_icon)
            {
                case NavIcon.Menu:
                    e.Graphics.DrawLine(pen, 12, 14, 28, 14);
                    e.Graphics.DrawLine(pen, 12, 20, 28, 20);
                    e.Graphics.DrawLine(pen, 12, 26, 28, 26);
                    break;
                case NavIcon.Back:
                    e.Graphics.DrawLine(pen, 26, centerY, 14, centerY);
                    e.Graphics.DrawLine(pen, 14, centerY, 20, centerY - 6);
                    e.Graphics.DrawLine(pen, 14, centerY, 20, centerY + 6);
                    break;
                case NavIcon.Forward:
                    e.Graphics.DrawLine(pen, 14, centerY, 26, centerY);
                    e.Graphics.DrawLine(pen, 26, centerY, 20, centerY - 6);
                    e.Graphics.DrawLine(pen, 26, centerY, 20, centerY + 6);
                    break;
                case NavIcon.Refresh:
                    e.Graphics.DrawArc(pen, 12, 12, 16, 16, 35, 285);
                    e.Graphics.DrawLine(pen, 27, 12, 27, 18);
                    e.Graphics.DrawLine(pen, 27, 12, 21, 12);
                    break;
            }
        }
    }

    private class RoundedPanel : Panel
    {
        public int Radius { get; set; } = 16;

        protected override void OnPaint(PaintEventArgs e)
        {
            using var path = RoundedRectangle(ClientRectangle, Radius);
            Region = new Region(path);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var brush = new SolidBrush(BackColor);
            e.Graphics.FillPath(brush, path);
            base.OnPaint(e);
        }
    }

    private sealed class RoundLabel : Label
    {
        public int Radius { get; set; } = 16;

        protected override void OnPaint(PaintEventArgs e)
        {
            using var path = RoundedRectangle(ClientRectangle, Radius);
            Region = new Region(path);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var brush = new SolidBrush(BackColor);
            e.Graphics.FillPath(brush, path);
            base.OnPaint(e);
        }
    }

    private sealed class NavItem : RoundedPanel
    {
        private readonly Label _title;
        private readonly Label _description;
        private readonly bool _danger;

        public NavItem(string title, string description, bool danger = false)
        {
            Radius = 18;
            _danger = danger;
            BackColor = danger ? Color.FromArgb(92, 35, 35) : BrandDeep;

            _title = new Label
            {
                Text = title,
                AutoSize = true,
                Location = new Point(18, 9),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            _description = new Label
            {
                Text = description,
                AutoSize = true,
                Location = new Point(18, 31),
                Font = new Font("Segoe UI", 8)
            };

            Controls.Add(_title);
            Controls.Add(_description);
        }

        public void SetActive(bool active)
        {
            BackColor = active ? Accent : _danger ? Color.FromArgb(92, 35, 35) : BrandDeep;
            _title.ForeColor = active ? BrandDeep : Color.White;
            _description.ForeColor = active ? Brand : _danger ? Color.FromArgb(255, 210, 210) : Color.FromArgb(214, 198, 187);
            Invalidate();
        }
    }

    private static GraphicsPath RoundedRectangle(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        var diameter = radius * 2;
        var arc = new Rectangle(bounds.Location, new Size(diameter, diameter));

        path.AddArc(arc, 180, 90);
        arc.X = bounds.Right - diameter;
        path.AddArc(arc, 270, 90);
        arc.Y = bounds.Bottom - diameter;
        path.AddArc(arc, 0, 90);
        arc.X = bounds.Left;
        path.AddArc(arc, 90, 90);
        path.CloseFigure();
        return path;
    }
}






