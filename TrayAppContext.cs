using System;
using System.Drawing;
using System.Windows.Forms;

namespace TrayPasswordGenerator
{
    public sealed class TrayAppContext : ApplicationContext
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly AppSettings _settings;

        public TrayAppContext()
        {
            _settings = AppSettings.Load();

            _notifyIcon = new NotifyIcon
            {
                Icon = Utils.LoadIcon(),
                Visible = true,
                Text   = "Генератор паролей"
            };

            var menu = new ContextMenuStrip();
            menu.Items.Add("Настройки", null, (_,__) => ShowSettings());
            menu.Items.Add("Выход",     null, (_,__) => ExitThread());

            _notifyIcon.ContextMenuStrip = menu;
            _notifyIcon.MouseClick += NotifyIcon_MouseClick;
        }

        private void NotifyIcon_MouseClick(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            try
            {
                string pwd = PasswordGenerator.CreatePassword(_settings);
                Clipboard.SetText(pwd);
                _notifyIcon.ShowBalloonTip(1500, "Пароль скопирован", pwd, ToolTipIcon.Info);
            }
            catch (Exception ex)
            {
                _notifyIcon.ShowBalloonTip(3000, "Ошибка", ex.Message, ToolTipIcon.Error);
            }
        }

        private void ShowSettings()
        {
            using var dlg = new SettingsForm(_settings);
            if (dlg.ShowDialog() == DialogResult.OK)
                _settings.Save();
        }

        protected override void ExitThreadCore()
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            base.ExitThreadCore();
        }
    }
}
