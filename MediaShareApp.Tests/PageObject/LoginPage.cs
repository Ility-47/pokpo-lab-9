using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;

namespace MediaShareApp.Tests.PageObjects
{
    /// <summary>
    /// Page Object для LoginForm.
    ///
    /// Принцип POM: класс знает всё о разметке формы (локаторы),
    /// а тесты работают только с методами (LoginAs, GetErrorMessage).
    /// Если разметка изменится — правим только здесь, не трогая тесты.
    /// </summary>
    public class LoginPage
    {
        private readonly Window window;
        private readonly ConditionFactory cf;

        public LoginPage(Window window, ConditionFactory cf)
        {
            this.window = window;
            this.cf = cf;
        }

        // ── Локаторы (приватные — снаружи не нужны) ──────────────────────
        // Ищем по AutomationId (это WinForms Name), чтобы локаторы были стабильными.
        private TextBox UsernameField
            => this.RequireTextBox("txtUsername");

        private TextBox PasswordField
            => this.RequireTextBox("txtPassword");

        private Button LoginButton
            => this.RequireButton("btnLogin");

        private Button GoRegisterButton
            => this.RequireButton("btnRegister");

        private Label? ErrorLabel
            => this.window.FindFirstDescendant(this.cf.ByAutomationId("lblError"))?.AsLabel();

        // ── Публичные методы (то, что видят тесты) ───────────────────────

        /// <summary>Вводит логин и пароль, нажимает «Войти».</summary>
        public void LoginAs(string username, string password)
        {
            this.UsernameField.Enter(username);
            this.PasswordField.Enter(password);
            this.LoginButton.Click();
        }

        /// <summary>Возвращает текст сообщения об ошибке (пустая строка = нет ошибки).</summary>
        /// <returns></returns>
        public string GetErrorMessage() => this.ErrorLabel?.Text ?? string.Empty;

        /// <summary>Нажимает кнопку перехода на форму регистрации.</summary>
        public void GoToRegister() => this.GoRegisterButton.Click();

        private Button RequireButton(string automationId)
            => this.RequireElement(automationId).AsButton();

        private TextBox RequireTextBox(string automationId)
            => this.RequireElement(automationId).AsTextBox();

        private AutomationElement RequireElement(string automationId)
        {
            return this.window.FindFirstDescendant(this.cf.ByAutomationId(automationId))
                ?? throw new InvalidOperationException($"Элемент '{automationId}' не найден на LoginForm.");
        }
    }
}
