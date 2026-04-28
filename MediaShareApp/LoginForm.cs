using System;
using System.Windows.Forms;

namespace MediaShareApp.WinForms
{
    /// <summary>
    /// Форма входа в систему.
    /// AccessibleName на каждом элементе — «крючок» для FlaUI,
    /// чтобы тесты могли находить элементы без знания разметки.
    /// </summary>
    public class LoginForm : Form
    {
        private TextBox txtUsername = null!;
        private TextBox txtPassword = null!;
        private Button btnLogin = null!;
        private Button btnRegister = null!;
        private Label lblError = null!;

        public LoginForm() => this.BuildUI();

        private void BuildUI()
        {
            // ── Свойства окна ──────────────────────────────────────────────
            this.Text = "Вход — MediaShare";
            this.Name = "LoginForm";
            this.Size = new System.Drawing.Size(360, 280);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            // ── Заголовок ──────────────────────────────────────────────────
            var lblTitle = new Label
            {
                Text = "Вход в систему",
                Font = new System.Drawing.Font("Segoe UI", 13, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(70, 20),
                Size = new System.Drawing.Size(230, 30),
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
            };

            // ── Поле логина ────────────────────────────────────────────────
            var lblUser = new Label
            {
                Text = "Логин:",
                Location = new System.Drawing.Point(30, 72),
                Size = new System.Drawing.Size(80, 20),
            };

            this.txtUsername = new TextBox
            {
                Name = "txtUsername",
                Location = new System.Drawing.Point(120, 69),
                Size = new System.Drawing.Size(200, 23),
                AccessibleName = "UsernameField", // <-- FlaUI ищет по этому имени
            };

            // ── Поле пароля ────────────────────────────────────────────────
            var lblPass = new Label
            {
                Text = "Пароль:",
                Location = new System.Drawing.Point(30, 108),
                Size = new System.Drawing.Size(80, 20),
            };

            this.txtPassword = new TextBox
            {
                Name = "txtPassword",
                Location = new System.Drawing.Point(120, 105),
                Size = new System.Drawing.Size(200, 23),
                PasswordChar = '*',
                AccessibleName = "PasswordField",
            };

            // ── Метка ошибки ───────────────────────────────────────────────
            this.lblError = new Label
            {
                Name = "lblError",
                Text = string.Empty,
                ForeColor = System.Drawing.Color.Crimson,
                Location = new System.Drawing.Point(30, 138),
                Size = new System.Drawing.Size(300, 20),
            };

            // ── Кнопка «Войти» ────────────────────────────────────────────
            this.btnLogin = new Button
            {
                Name = "btnLogin",
                Text = "Войти",
                Location = new System.Drawing.Point(30, 170),
                Size = new System.Drawing.Size(130, 36),
                AccessibleName = "LoginButton",
            };
            this.btnLogin.Click += this.OnLogin;

            // ── Кнопка «Регистрация» ──────────────────────────────────────
            this.btnRegister = new Button
            {
                Name = "btnRegister",
                Text = "Регистрация",
                Location = new System.Drawing.Point(180, 170),
                Size = new System.Drawing.Size(140, 36),
                AccessibleName = "RegisterButton",
            };
            this.btnRegister.Click += this.OnGoRegister;

            this.Controls.AddRange(new Control[]
                {
                    lblTitle, lblUser, this.txtUsername, lblPass, this.txtPassword,
                    this.lblError, this.btnLogin, this.btnRegister,
                });
        }

        private void OnLogin(object? sender, EventArgs e)
        {
            this.lblError.Text = string.Empty;
            try
            {
                var user = Program.Service.Login(this.txtUsername.Text.Trim(), this.txtPassword.Text);
                var main = new MainForm(user.Username);
                main.Show();
                this.Hide();
                main.FormClosed += (_, _) => this.Close();
            }
            catch (Exception ex)
            {
                this.lblError.Text = ex.Message;
            }
        }

        private void OnGoRegister(object? sender, EventArgs e)
        {
            var reg = new RegisterForm();
            reg.Show();
            this.Hide();
            reg.FormClosed += (_, _) => this.Show();
        }
    }
}
