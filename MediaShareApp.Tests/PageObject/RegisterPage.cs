using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;

namespace MediaShareApp.Tests.PageObjects
{
    /// <summary>
    /// Page Object для RegisterForm.
    /// </summary>
    public class RegisterPage
    {
        private readonly Window window;
        private readonly ConditionFactory cf;

        public RegisterPage(Window window, ConditionFactory cf)
        {
            this.window = window;
            this.cf = cf;
        }

        private TextBox UsernameField
            => this.RequireTextBox("txtRegUsername");

        private TextBox PasswordField
            => this.RequireTextBox("txtRegPassword");

        private Button SubmitButton
            => this.RequireButton("btnRegisterSubmit");

        private Label? StatusLabel
            => this.window.FindFirstDescendant(this.cf.ByAutomationId("lblStatus"))?.AsLabel();

        /// <summary>Заполняет форму регистрации и отправляет её.</summary>
        public void Register(string username, string password)
        {
            this.UsernameField.Enter(username);
            this.PasswordField.Enter(password);
            this.SubmitButton.Click();
        }

        /// <summary>Возвращает текст статусной метки после попытки регистрации.</summary>
        /// <returns></returns>
        public string GetStatusText() => this.StatusLabel?.Text ?? string.Empty;

        private Button RequireButton(string automationId)
            => this.RequireElement(automationId).AsButton();

        private TextBox RequireTextBox(string automationId)
            => this.RequireElement(automationId).AsTextBox();

        private AutomationElement RequireElement(string automationId)
        {
            return this.window.FindFirstDescendant(this.cf.ByAutomationId(automationId))
                ?? throw new InvalidOperationException($"Элемент '{automationId}' не найден на RegisterForm.");
        }
    }
}
