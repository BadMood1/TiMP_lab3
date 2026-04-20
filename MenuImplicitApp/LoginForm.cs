using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using AuthorizationLibrary;

namespace MenuImplicitApp;

public class LoginForm : Form
{
    private const int WmInputLangChange = 0x0051;

    private readonly AuthorizationService _authorizationService;
    private readonly TextBox _txtUser = new();
    private readonly TextBox _txtPassword = new();
    private readonly ToolStripStatusLabel _statusLang = new();
    private readonly ToolStripStatusLabel _statusCaps = new();
    private readonly StatusStrip _statusStrip = new();
    private readonly Image _keysImage;

    public Dictionary<string, int>? ResultStatuses { get; private set; }

    public LoginForm()
    {
        var usersPath = Path.Combine(AppContext.BaseDirectory, "USERS.txt");
        _authorizationService = new AuthorizationService(usersPath);
        _keysImage = LoadKeysImage();
        BuildUi();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _keysImage.Dispose();
        }

        base.Dispose(disposing);
    }

    private void BuildUi()
    {
        Text = "Вход";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Microsoft Sans Serif", 8.25f);
        BackColor = Color.FromArgb(185, 209, 234);
        ClientSize = new Size(420, 200);
        KeyPreview = true;

        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 65,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(3),
            Margin = new Padding(3)
        };

        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        header.RowStyles.Add(new RowStyle(SizeType.Percent, 30f));
        header.RowStyles.Add(new RowStyle(SizeType.Percent, 5f));
        header.RowStyles.Add(new RowStyle(SizeType.Percent, 30f));
        header.RowStyles.Add(new RowStyle(SizeType.Percent, 5f));
        header.RowStyles.Add(new RowStyle(SizeType.Percent, 30f));

        var keysPanel = new Panel
        {
            Dock = DockStyle.None,
            Size = new Size(55, 45),
            Location = new Point(7, 0),
            BackColor = Color.Transparent,
            Visible = true
        };
        keysPanel.Paint += OnKeysPanelPaint;

        var cream = Color.FromArgb(255, 250, 205);
        var yellow = Color.FromArgb(255, 215, 0);
        var white = Color.FromArgb(255, 255, 255);
        var blue = Color.FromArgb(185, 209, 234);

        var panelBlueTop = new Panel { Dock = DockStyle.Fill, BackColor = blue, Margin = new Padding(0) };
        var panelBlueBottom = new Panel { Dock = DockStyle.Fill, BackColor = blue, Margin = new Padding(0) };

        var panelCream = new Panel { Dock = DockStyle.Fill, BackColor = cream, Margin = new Padding(0) };
        var lblApp = new Label
        {
            Text = "АИС Отдел кадров",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            BackColor = cream,
            Padding = new Padding(218, 0, 0, 0)
        };
        panelCream.Controls.Add(lblApp);

        var version = typeof(LoginForm).Assembly.GetName().Version;
        var panelYellow = new Panel { Dock = DockStyle.Fill, BackColor = yellow, Margin = new Padding(0) };
        var lblVersion = new Label
        {
            Text = version is null
                ? "Версия 1.0.0.0"
                : $"Версия {version.Major}.{version.Minor}.{version.Build}.{version.Revision}",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            BackColor = yellow,
            Padding = new Padding(8, 2, 0, 0)
        };
        panelYellow.Controls.Add(lblVersion);

        var panelWhite = new Panel { Dock = DockStyle.Fill, BackColor = white, Margin = new Padding(0) };
        var lblPrompt = new Label
        {
            Text = "Введите имя пользователя и пароль",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            BackColor = white,
            Padding = new Padding(218, 0, 0, 0)
        };
        panelWhite.Controls.Add(lblPrompt);

        header.Controls.Add(panelCream, 1, 0);
        header.Controls.Add(panelBlueTop, 1, 1);
        header.Controls.Add(panelYellow, 1, 2);
        header.Controls.Add(panelBlueBottom, 1, 3);
        header.Controls.Add(panelWhite, 1, 4);

        var body = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = blue,
            Padding = new Padding(16, 8, 16, 12)
        };

        var lblUser = new Label
        {
            Text = "Имя пользователя",
            AutoSize = true,
            Location = new Point(10, 16),
            BackColor = blue
        };

        _txtUser.Location = new Point(160, 12);
        _txtUser.Width = 250;
        _txtUser.BackColor = Color.White;

        var lblPass = new Label
        {
            Text = "Пароль",
            AutoSize = true,
            Location = new Point(10, 52),
            BackColor = blue
        };

        _txtPassword.Location = new Point(160, 48);
        _txtPassword.Width = 250;
        _txtPassword.BackColor = Color.White;
        _txtPassword.PasswordChar = '*';

        var btnOk = new Button
        {
            Text = "Вход",
            Location = new Point(20, 83),
            Size = new Size(90, 24),
            BackColor = Color.FromArgb(240, 240, 240)
        };
        btnOk.Click += OnLoginClick;

        var btnCancel = new Button
        {
            Text = "Отмена",
            Location = new Point(320, 83),
            Size = new Size(90, 24),
            BackColor = Color.FromArgb(240, 240, 240)
        };
        btnCancel.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };

        body.Controls.Add(lblUser);
        body.Controls.Add(_txtUser);
        body.Controls.Add(lblPass);
        body.Controls.Add(_txtPassword);
        body.Controls.Add(btnOk);
        body.Controls.Add(btnCancel);

        _statusStrip.Dock = DockStyle.Bottom;
        _statusStrip.SizingGrip = true;
        _statusStrip.RenderMode = ToolStripRenderMode.System;

        _statusLang.Spring = true;
        _statusLang.TextAlign = ContentAlignment.MiddleLeft;
        _statusCaps.TextAlign = ContentAlignment.MiddleRight;
        _statusCaps.AutoSize = true;

        _statusStrip.Items.Add(_statusLang);
        _statusStrip.Items.Add(_statusCaps);

        Controls.Add(body);
        Controls.Add(_statusStrip);
        Controls.Add(header);
        Controls.Add(keysPanel);
        keysPanel.BringToFront();

        KeyUp += (_, _) => UpdateCapsStatus();
        Shown += (_, _) =>
        {
            UpdateInputLanguageLabel();
            UpdateCapsStatus();
        };

        UpdateInputLanguageLabel();
        UpdateCapsStatus();
    }

    private void OnKeysPanelPaint(object? sender, PaintEventArgs e)
    {
        if (sender is Panel panel)
        {
            e.Graphics.DrawImage(_keysImage, panel.ClientRectangle);
        }
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WmInputLangChange)
        {
            BeginInvoke(new Action(UpdateInputLanguageLabel));
        }

        base.WndProc(ref m);
    }

    private void UpdateInputLanguageLabel()
    {
        try
        {
            var lang = InputLanguage.CurrentInputLanguage;
            _statusLang.Text = "Язык ввода " + FormatInputLanguageName(lang);
        }
        catch
        {
            _statusLang.Text = "Язык ввода";
        }
    }

    private static string FormatInputLanguageName(InputLanguage lang)
    {
        var culture = lang.Culture;
        if (culture.TwoLetterISOLanguageName.Equals("en", StringComparison.OrdinalIgnoreCase))
        {
            return "Английский";
        }

        if (culture.TwoLetterISOLanguageName.Equals("ru", StringComparison.OrdinalIgnoreCase))
        {
            return "Русский";
        }

        try
        {
            return culture.NativeName;
        }
        catch (CultureNotFoundException)
        {
            return culture.DisplayName;
        }
    }

    private void UpdateCapsStatus()
    {
        _statusCaps.Text = Control.IsKeyLocked(Keys.CapsLock) ? "Клавиша CapsLock нажата" : string.Empty;
        _statusCaps.ForeColor = SystemColors.ControlText;
    }

    private void OnLoginClick(object? sender, EventArgs e)
    {
        var username = _txtUser.Text.Trim();
        var password = _txtPassword.Text;

        try
        {
            var statuses = _authorizationService.Authenticate(username, password);
            if (statuses == null)
            {
                MessageBox.Show(
                    "Неверное имя пользователя или пароль.",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            ResultStatuses = statuses;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static Image LoadKeysImage()
    {
        var imagePath = Path.Combine(AppContext.BaseDirectory, "keys.png");
        return File.Exists(imagePath) ? Image.FromFile(imagePath) : new Bitmap(55, 45);
    }
}
