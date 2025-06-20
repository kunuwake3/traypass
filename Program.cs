using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Reflection;
using System.Drawing;

namespace TrayPasswordGenerator;

/// <summary>
/// Точка входа – создаём контекст с иконкой-треем.
/// </summary>
static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new TrayAppContext());
    }
}

/// <summary>
/// Контекст приложения, «живущий» в системном трее.
/// </summary>
public sealed class TrayAppContext : ApplicationContext
{
    private readonly NotifyIcon _tray;
    private PasswordOptions _opts;
    private readonly string _cfgPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                     "TrayPasswordGenerator", "settings.json");

    public TrayAppContext()
    {
        _opts = PasswordOptions.Load(_cfgPath);

        _tray = new NotifyIcon
        {
            Icon    = LoadEmbeddedIcon(),
            Visible = true,
            Text    = "Tray Password Generator"
        };
        _tray.MouseClick += OnClick;

        var menu = new ContextMenuStrip();
        menu.Items.Add("Настройки…", null, (_, __) => ShowSettings());
        menu.Items.Add("Выход",      null, (_, __) => ExitThread());
        _tray.ContextMenuStrip = menu;
    }

    private static Icon LoadEmbeddedIcon()
    {
        var asm = Assembly.GetExecutingAssembly();
        var resName = asm.GetName().Name + ".tray_favicon.ico";   // «TrayPasswordGenerator.tray_favicon.ico»
        using var s = asm.GetManifestResourceStream(resName)
                   ?? throw new InvalidOperationException($"Не найден ресурс {resName}");
        return new Icon(s);
    }

    private void OnClick(object? s, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;   // Левый клик = сгенерировать
        string pwd = PasswordGenerator.Generate(_opts);
        Clipboard.SetText(pwd);
        _tray.ShowBalloonTip(
            1500,
            "Пароль скопирован",
            pwd.Length > 32 ? $"{pwd[..32]}…" : pwd,
            ToolTipIcon.Info);
    }

    private void ShowSettings()
    {
        using var dlg = new SettingsForm(_opts);
        if (dlg.ShowDialog() == DialogResult.OK)
        {
            _opts = dlg.Options;
            _opts.Save(_cfgPath);
        }
    }

    protected override void ExitThreadCore()   // корректное завершение
    {
        _tray.Visible = false;
        _tray.Dispose();
        base.ExitThreadCore();
    }
}

/// <summary>
/// Настройки генерации (галочки + длина + префикс).
/// </summary>
public sealed record PasswordOptions
{
    public bool UseDigits         { get; init; } = true;
    public bool UseUpper          { get; init; } = true;
    public bool UseSpecial        { get; init; } = false;
    public bool SafeSpecialOnly   { get; init; } = true;
    public int  Length            { get; init; } = 16;
    public string Prefix          { get; init; } = "";

    // путь к файлу берётся снаружи — удобнее тестировать
    public static PasswordOptions Load(string path)
    {
        try
        {
            return File.Exists(path)
                 ? JsonSerializer.Deserialize<PasswordOptions>(File.ReadAllText(path)) ?? new()
                 : new();
        }
        catch { return new(); }
    }
    public void Save(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path,
            JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }
}

/// <summary>
/// Генератор пароля.
/// </summary>
public static class PasswordGenerator
{
    private const string Digits  = "0123456789";
    private const string Upper   = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string Lower   = "abcdefghijklmnopqrstuvwxyz";
    private const string SpecialAll  = "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";
    private const string SpecialSafe = "!@#$%^&*()-_=+[]{}.";

    public static string Generate(PasswordOptions o)
    {
        // формируем «алфавит»
        var pool = new StringBuilder(Lower);
        if (o.UseDigits)  pool.Append(Digits);
        if (o.UseUpper)   pool.Append(Upper);
        if (o.UseSpecial) pool.Append(o.SafeSpecialOnly ? SpecialSafe : SpecialAll);

        if (pool.Length == 0) pool.Append(Lower);  // хотя бы что-то

        Span<byte> rand = stackalloc byte[o.Length];
        RandomNumberGenerator.Fill(rand);

        var pwd = new StringBuilder(o.Prefix, o.Prefix.Length + o.Length);
        for (int i = 0; i < o.Length; i++)
            pwd.Append(pool[rand[i] % pool.Length]);

        return pwd.ToString();
    }
}

/// <summary>
/// Окно настроек (правый клик по иконке).
/// </summary>
public sealed class SettingsForm : Form
{
    public PasswordOptions Options { get; private set; }

    private readonly CheckBox _digits, _upper, _special, _safe;
    private readonly NumericUpDown _len;
    private readonly TextBox _prefix;

    public SettingsForm(PasswordOptions current)
    {
        Options = current;
        Text = "Настройки генератора паролей";
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;

        // Layout 2×N
        var grid = new TableLayoutPanel
        {
            ColumnCount = 2,
            RowCount = 0,
            AutoSize = true,
            Padding = new Padding(10),
            Dock = DockStyle.Fill
        };
        Controls.Add(grid);

        _digits  = AddCheck("Цифры (0-9)"          , current.UseDigits);
        _upper   = AddCheck("Буквы верхнего регистра", current.UseUpper);
        _special = AddCheck("Спец-символы"         , current.UseSpecial, (s, _) =>
        {
            _safe.Enabled = ((CheckBox)s!).Checked;
        });
        _safe    = AddCheck("Только безопасные", current.SafeSpecialOnly);
        _safe.Enabled = current.UseSpecial;

        grid.Controls.Add(new Label { Text = "Длина:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, grid.RowCount);
        _len = new NumericUpDown { Minimum = 4, Maximum = 128, Value = current.Length };
        grid.Controls.Add(_len, 1, grid.RowCount++);

        grid.Controls.Add(new Label { Text = "Префикс:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, grid.RowCount);
        _prefix = new TextBox { Text = current.Prefix ?? "" };
        grid.Controls.Add(_prefix, 1, grid.RowCount++);

        // Кнопки OK/Cancel
        var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Bottom, AutoSize = true };
        var ok  = new Button { Text = "OK", DialogResult = DialogResult.OK  };
        var esc = new Button { Text = "Отмена", DialogResult = DialogResult.Cancel };
        buttons.Controls.Add(ok);
        buttons.Controls.Add(esc);
        Controls.Add(buttons);

        AcceptButton = ok; CancelButton = esc;
        ok.Click += (_, __) => Save();
    }

    private CheckBox AddCheck(string text, bool state, EventHandler? changed = null)
    {
        var cb = new CheckBox { Text = text, Checked = state, AutoSize = true };
        if (changed != null) cb.CheckedChanged += changed;
        var row = ((TableLayoutPanel)Controls[0]).RowCount++;
        ((TableLayoutPanel)Controls[0]).Controls.Add(cb, 0, row);
        ((TableLayoutPanel)Controls[0]).SetColumnSpan(cb, 2);
        return cb;
    }

    private void Save()
    {
        Options = Options with
        {
            UseDigits       = _digits.Checked,
            UseUpper        = _upper.Checked,
            UseSpecial      = _special.Checked,
            SafeSpecialOnly = _safe.Checked,
            Length          = (int)_len.Value,
            Prefix          = _prefix.Text
        };
    }
}
