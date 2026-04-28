using System;
using System.Windows.Forms;

namespace MediaShareApp.WinForms
{
    /// <summary>
    /// Форма регистрации нового пользователя.
    /// </summary>
    public class RegisterForm : Form
    {
        private TextBox txtUsername = null!;
        private TextBox txtPassword = null!;
        private Button btnRegister = null!;
        private Label lblStatus = null!;

        public RegisterForm() => this.BuildUI();

        private void BuildUI()
        {
            this.Text = "Регистрация — MediaShare";
            this.Name = "RegisterForm";
            this.Size = new System.Drawing.Size(360, 280);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            var lblTitle = new Label
            {
                Text = "Регистрация",
                Font = new System.Drawing.Font("Segoe UI", 13, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(90, 20),
                Size = new System.Drawing.Size(190, 30),
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
            };

            var lblUser = new Label
            {
                Text = "Логин:",
                Location = new System.Drawing.Point(30, 72),
                Size = new System.Drawing.Size(80, 20),
            };

            this.txtUsername = new TextBox
            {
                Name = "txtRegUsername",
                Location = new System.Drawing.Point(120, 69),
                Size = new System.Drawing.Size(200, 23),
                AccessibleName = "RegUsernameField",
            };

            var lblPass = new Label
            {
                Text = "Пароль:",
                Location = new System.Drawing.Point(30, 108),
                Size = new System.Drawing.Size(80, 20),
            };

            this.txtPassword = new TextBox
            {
                Name = "txtRegPassword",
                Location = new System.Drawing.Point(120, 105),
                Size = new System.Drawing.Size(200, 23),
                PasswordChar = '*',
                AccessibleName = "RegPasswordField",
            };

            // Подсказка о минимальной длине пароля
            var lblHint = new Label
            {
                Text = "Минимум 6 символов",
                ForeColor = System.Drawing.Color.Gray,
                Font = new System.Drawing.Font("Segoe UI", 8),
                Location = new System.Drawing.Point(120, 131),
                Size = new System.Drawing.Size(200, 16),
            };

            this.lblStatus = new Label
            {
                Name = "lblStatus",
                Text = string.Empty,
                Location = new System.Drawing.Point(30, 155),
                Size = new System.Drawing.Size(300, 20),
            };

            this.btnRegister = new Button
            {
                Name = "btnRegisterSubmit",
                Text = "Зарегистрироваться",
                Location = new System.Drawing.Point(80, 188),
                Size = new System.Drawing.Size(200, 36),
                AccessibleName = "RegisterSubmitButton",
            };
            this.btnRegister.Click += this.OnRegister;

            this.Controls.AddRange(new Control[]
                {
                    lblTitle, lblUser, this.txtUsername, lblPass, this.txtPassword,
                    lblHint, this.lblStatus, this.btnRegister,
                });
        }

        private void OnRegister(object? sender, EventArgs e)
        {
            this.lblStatus.Text = string.Empty;
            try
            {
                Program.Service.Register(this.txtUsername.Text.Trim(), this.txtPassword.Text);
                this.lblStatus.ForeColor = System.Drawing.Color.Green;
                this.lblStatus.Text = "Успешно зарегистрирован!";
            }
            catch (Exception ex)
            {
                this.lblStatus.ForeColor = System.Drawing.Color.Crimson;
                this.lblStatus.Text = ex.Message;
            }
        }
    }
}
