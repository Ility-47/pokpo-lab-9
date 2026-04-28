using System;
using System.IO;
using System.Threading;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.UIA3;
using MediaShareApp.Tests.PageObjects;
using NUnit.Framework;

namespace MediaShareApp.Tests.UITests
{
    /// <summary>
    /// Автоматизированные UI-тесты (Задание 1).
    /// Используют Page Object Model — тесты не содержат прямых обращений к элементам.
    ///
    /// Как работает:
    ///   1. [SetUp]    — запускаем .exe приложения через FlaUI
    ///   2. Тест       — управляем окнами через Page Object'ы
    ///   3. [TearDown] — убиваем процесс.
    /// </summary>
    [TestFixture]
    [Category("Integration")]
    [Apartment(System.Threading.ApartmentState.STA)] // WinForms требует STA-поток
    public class LoginUITests
    {
        private Application app = null!;
        private UIA3Automation automation = null!;
        private Window window = null!;
        private ConditionFactory cf = null!;

        // Путь до скомпилированного WinForms .exe
        // Относительный путь: из bin/Debug тестов поднимаемся на 4 уровня к корню solution,
        // затем спускаемся к WinForms exe.
        private static string AppPath => Path.GetFullPath(
            Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                @"..\..\..\..\MediaShareApp\bin\Debug\net8.0-windows\MediaShareApp.exe"));

        [SetUp]
        public void Setup()
        {
            // Запускаем приложение
            this.app = Application.Launch(AppPath);
            this.automation = new UIA3Automation();
            this.cf = new ConditionFactory(new UIA3PropertyLibrary());

            // Ждём появления главного окна (LoginForm) — до 10 секунд
            this.window = RequireWindow(this.app.GetMainWindow(this.automation, TimeSpan.FromSeconds(10)), "LoginForm");

            // Небольшая пауза для полной отрисовки всех контролов
            Thread.Sleep(400);
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                this.app?.Close();
            }
            catch
            { /* игнорируем, если уже закрыто */
            }
            this.automation?.Dispose();
        }

        // ══════════════════════════════════════════════════════════════════
        // Сценарий 1 (Позитивный): успешный вход после регистрации
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Тест: зарегистрированный пользователь может войти в систему.
        /// Ожидаем: открывается MainForm с приветствием «Добро пожаловать, testuser!».
        /// </summary>
        [Test]
        public void Login_WithValidCredentials_OpensMainFormWithWelcome()
        {
            // Arrange: сначала регистрируем пользователя через UI
            this.RegisterUserViaUI("testuser", "password1");

            // Act: возвращаемся к LoginForm и входим
            this.window = RequireWindow(this.app.GetMainWindow(this.automation, TimeSpan.FromSeconds(5)), "LoginForm");
            var loginPage = new LoginPage(this.window, this.cf);
            loginPage.LoginAs("testuser", "password1");
            Thread.Sleep(800);

            // Assert: MainForm открылась, приветствие содержит имя пользователя
            var mainWindow = RequireWindow(this.app.GetMainWindow(this.automation, TimeSpan.FromSeconds(5)), "MainForm");
            var mainPage = new MainPage(mainWindow, this.cf);
            string welcome = mainPage.GetWelcomeText();

            Assert.That(welcome, Does.Contain("testuser"),
                "После входа должно отображаться имя пользователя в приветствии");
        }

        // ══════════════════════════════════════════════════════════════════
        // Сценарий 2 (Негативный): вход с неверным паролем
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Тест: попытка входа с неверным паролем показывает сообщение об ошибке.
        /// Ожидаем: LoginForm остаётся открытой, lblError содержит текст ошибки.
        /// </summary>
        [Test]
        public void Login_WithWrongPassword_ShowsErrorMessage()
        {
            // Act: пробуем войти с несуществующим пользователем
            var loginPage = new LoginPage(this.window, this.cf);
            loginPage.LoginAs("nonexistent_user", "badpass1");
            Thread.Sleep(500);

            // Assert: сообщение об ошибке появилось
            string error = loginPage.GetErrorMessage();
            Assert.That(error, Is.Not.Empty,
                "При неверных данных должно отображаться сообщение об ошибке");
        }

        // ══════════════════════════════════════════════════════════════════
        // Сценарий 3 (E2E): регистрация → вход
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Сквозной тест: регистрируем нового пользователя и сразу входим.
        /// Проверяем оба шага: сначала факт успешной регистрации, затем вход.
        /// </summary>
        [Test]
        public void Register_NewUser_ThenLogin_SuccessfullyOpensMainForm()
        {
            // Уникальный логин — чтобы тест не зависел от порядка запуска
            string username = $"user{Environment.TickCount64 % 100000}";
            string password = "secure99";

            // ── Шаг 1: переходим на форму регистрации ─────────────────────
            var loginPage = new LoginPage(this.window, this.cf);
            loginPage.GoToRegister();
            Thread.Sleep(500);

            // ── Шаг 2: регистрируемся ─────────────────────────────────────
            var regWindow = RequireWindow(this.app.GetMainWindow(this.automation, TimeSpan.FromSeconds(5)), "RegisterForm");
            var registerPage = new RegisterPage(regWindow, this.cf);
            registerPage.Register(username, password);
            Thread.Sleep(500);

            // Проверяем успешный статус регистрации
            string status = registerPage.GetStatusText();
            Assert.That(status, Does.Contain("Успешно"),
                "После регистрации должно появиться сообщение об успехе");

            // ── Шаг 3: закрываем RegisterForm, возвращаемся к LoginForm ──
            regWindow.Close();
            Thread.Sleep(500);

            // ── Шаг 4: входим ─────────────────────────────────────────────
            this.window = RequireWindow(this.app.GetMainWindow(this.automation, TimeSpan.FromSeconds(5)), "LoginForm");
            loginPage = new LoginPage(this.window, this.cf);
            loginPage.LoginAs(username, password);
            Thread.Sleep(800);

            // ── Assert: MainForm открылась ─────────────────────────────────
            var mainWindow = RequireWindow(this.app.GetMainWindow(this.automation, TimeSpan.FromSeconds(5)), "MainForm");
            var mainPage = new MainPage(mainWindow, this.cf);

            Assert.That(mainPage.GetWelcomeText(), Does.Contain(username),
                "После входа MainForm должна показывать имя зарегистрированного пользователя");
        }

        // ── Вспомогательный метод ─────────────────────────────────────────

        /// <summary>
        /// Регистрирует пользователя через UI и возвращает управление LoginForm.
        /// Используется в тестах как Arrange-шаг.
        /// </summary>
        private void RegisterUserViaUI(string username, string password)
        {
            var loginPage = new LoginPage(this.window, this.cf);
            loginPage.GoToRegister();
            Thread.Sleep(500);

            var regWindow = RequireWindow(this.app.GetMainWindow(this.automation, TimeSpan.FromSeconds(5)), "RegisterForm");
            var registerPage = new RegisterPage(regWindow, this.cf);
            registerPage.Register(username, password);
            Thread.Sleep(400);

            regWindow.Close();
            Thread.Sleep(500);
        }

        private static Window RequireWindow(Window? window, string expectedWindow)
        {
            return window ?? throw new InvalidOperationException($"Не удалось найти окно '{expectedWindow}'.");
        }
    }
}
