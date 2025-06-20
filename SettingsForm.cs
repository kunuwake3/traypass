using System;
using System.Drawing;
using System.Windows.Forms;

namespace TrayPasswordGenerator
{
    public sealed class SettingsForm : Form
    {
        private readonly AppSettings _s;

        private CheckBox    _cbNumbers   = null!;
        private CheckBox    _cbUpper     = null!;
        private RadioButton _rbNone      = null!;
        private RadioButton _rbSafe      = null!;
        private RadioButton _rbAll       = null!;
        private NumericUpDown _numLen    = null!;
        private TextBox       _txtPrefix = null!;

        public SettingsForm(AppSettings s)
        {
            _s = s;
            BuildUi();
            LoadFromSettings();
        }

        private void BuildUi()
        {
            Text = "Настройки генератора";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition   = FormStartPosition.CenterScreen;
            MaximizeBox     = false;
            MinimizeBox     = false;
            ClientSize      = new Size(420, 260);

            int y = 15;

            _cbNumbers = AddCheck("Цифры (0-9)",            y); y += 25;
            _cbUpper   = AddCheck("Буквы верхнего регистра", y); y += 25;

            _rbNone = AddRadio("Без спецсимволов",          y); y += 25;
            _rbSafe = AddRadio("Безопасные (_-+@#)",        y); y += 25;
            _rbAll  = AddRadio("Все спецсимволы",           y); y += 30;

            Controls.Add(new Label { Text = "Длина пароля:", Left = 15, Top = y + 3, AutoSize = true });
            _numLen = new NumericUpDown
            {
                Left = 150, Top = y, Width = 60,
                Minimum = 4, Maximum = 128
            };
            Controls.Add(_numLen);
            y += 30;

            Controls.Add(new Label { Text = "Статический префикс:", Left = 15, Top = y + 3, AutoSize = true });
            _txtPrefix = new TextBox { Left = 150, Top = y, Width = 240 };
            Controls.Add(_txtPrefix);
            y += 40;

            var btnOK     = new Button { Text = "OK",     DialogResult = DialogResult.OK,     Left = Width - 180, Top = y, Width = 75 };
            var btnCancel = new Button { Text = "Отмена", DialogResult = DialogResult.Cancel, Left = Width - 95,  Top = y, Width = 75 };

            AcceptButton = btnOK;
            CancelButton = btnCancel;
            btnOK.Click += (_,__) => SaveToSettings();

            Controls.AddRange(new Control[] { btnOK, btnCancel });
        }

        private CheckBox AddCheck(string text, int top)
        {
            var cb = new CheckBox { Text = text, Left = 15, Top = top, AutoSize = true };
            Controls.Add(cb);
            return cb;
        }
        private RadioButton AddRadio(string text, int top)
        {
            var rb = new RadioButton { Text = text, Left = 35, Top = top, AutoSize = true };
            Controls.Add(rb);
            return rb;
        }

        private void LoadFromSettings()
        {
            _cbNumbers.Checked = _s.UseNumbers;
            _cbUpper.Checked   = _s.UseUppercase;

            _rbSafe.Checked = _s.SpecialCharactersMode == AppSettings.SpecialMode.Safe;
            _rbAll .Checked = _s.SpecialCharactersMode == AppSettings.SpecialMode.All;
            _rbNone.Checked = _s.SpecialCharactersMode == AppSettings.SpecialMode.None;

            _numLen.Value  = _s.PasswordLength;
            _txtPrefix.Text= _s.StaticPrefix;
        }

        private void SaveToSettings()
        {
            _s.UseNumbers  = _cbNumbers.Checked;
            _s.UseUppercase= _cbUpper.Checked;

            _s.SpecialCharactersMode =
                _rbAll.Checked  ? AppSettings.SpecialMode.All  :
                _rbSafe.Checked ? AppSettings.SpecialMode.Safe :
                                  AppSettings.SpecialMode.None;

            _s.PasswordLength = (int)_numLen.Value;
            _s.StaticPrefix   = _txtPrefix.Text;
        }
    }
}
