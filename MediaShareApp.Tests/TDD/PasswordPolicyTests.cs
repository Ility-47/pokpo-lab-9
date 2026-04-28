using MediaShareApp;
using NUnit.Framework;

namespace MediaShareApp.Tests.TDD
{
    /// <summary>
    /// TDD тесты для IsPasswordStrong.
    /// Порядок написания отражает цикл Red → Green → Refactor.
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    public class PasswordPolicyTests
    {
        private PasswordPolicy policy;

        [SetUp]
        public void Setup() => this.policy = new PasswordPolicy();

        // ── RED 1: метода нет → тест не компилируется ──────────────────────
        // ── GREEN 1: добавили "return false;" → проходит ───────────────────
        [Test]
        public void IsPasswordStrong_ShortPassword_ReturnsFalse()
        {
            bool result = this.policy.IsPasswordStrong("short"); // 5 символов
            Assert.IsFalse(result, "Пароль короче 8 символов — ненадёжный");
        }

        // ── RED 2: тест падает т.к. метод всегда false ─────────────────────
        // ── GREEN 2: добавили проверку длины ───────────────────────────────
        [Test]
        public void IsPasswordStrong_8CharsNoDigit_ReturnsFalse()
        {
            bool result = this.policy.IsPasswordStrong("abcdefgh"); // 8 символов, нет цифр
            Assert.IsFalse(result, "Пароль без цифры — ненадёжный");
        }

        // ── RED 3: нужна проверка на цифру ─────────────────────────────────
        // ── GREEN 3: добавили Any(char.IsDigit) ────────────────────────────
        [Test]
        public void IsPasswordStrong_ValidPassword_ReturnsTrue()
        {
            bool result = this.policy.IsPasswordStrong("StrongPass1");
            Assert.IsTrue(result, "Пароль 8+ символов с цифрой — надёжный");
        }

        // ── После Refactor: граничные случаи ───────────────────────────────
        [Test]
        public void IsPasswordStrong_Null_ReturnsFalse()
            => Assert.IsFalse(this.policy.IsPasswordStrong(null));

        [Test]
        public void IsPasswordStrong_Empty_ReturnsFalse()
            => Assert.IsFalse(this.policy.IsPasswordStrong(string.Empty));

        [Test]
        public void IsPasswordStrong_Exactly8CharsWithDigit_ReturnsTrue()
            => Assert.IsTrue(this.policy.IsPasswordStrong("abcdef1g")); // ровно 8, есть цифра

        [Test]
        public void IsPasswordStrong_7CharsWithDigit_ReturnsFalse()
            => Assert.IsFalse(this.policy.IsPasswordStrong("abcde1f")); // 7 символов

        [Test]
        public void IsPasswordStrong_OnlyDigits8Chars_ReturnsTrue()
            => Assert.IsTrue(this.policy.IsPasswordStrong("12345678")); // только цифры, 8 штук
    }
}
