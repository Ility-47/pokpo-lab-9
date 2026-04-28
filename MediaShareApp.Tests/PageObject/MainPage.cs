using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;

namespace MediaShareApp.Tests.PageObjects
{
    /// <summary>
    /// Page Object для MainForm (главное окно после входа).
    /// </summary>
    public class MainPage
    {
        private readonly Window window;
        private readonly ConditionFactory cf;

        public MainPage(Window window, ConditionFactory cf)
        {
            this.window = window;
            this.cf = cf;
        }

        private Label? WelcomeLabel
            => this.window.FindFirstDescendant(this.cf.ByAutomationId("lblWelcome"))?.AsLabel();

        private Button LogoutButton
            => this.RequireElement("btnLogout").AsButton();

        /// <summary>Возвращает текст приветствия.</summary>
        /// <returns></returns>
        public string GetWelcomeText() => this.WelcomeLabel?.Text ?? string.Empty;

        /// <summary>Нажимает кнопку выхода.</summary>
        public void Logout() => this.LogoutButton.Click();

        private AutomationElement RequireElement(string automationId)
        {
            return this.window.FindFirstDescendant(this.cf.ByAutomationId(automationId))
                ?? throw new InvalidOperationException($"Элемент '{automationId}' не найден на MainForm.");
        }
    }
}
