using System.Linq;

namespace MediaShareApp
{
    /// <summary>
    /// Политика надёжности паролей.
    /// Класс разработан методом TDD (Red → Green → Refactor).
    ///
    /// Требование: пароль надёжный, если:
    ///   1) длина не менее 8 символов
    ///   2) содержит хотя бы одну цифру.
    /// </summary>
    public class PasswordPolicy
    {
        /// <summary>
        /// Проверяет, является ли пароль надёжным.
        /// </summary>
        /// <param name="password">Проверяемый пароль.</param>
        /// <returns>true — пароль надёжный, false — нет.</returns>
        public bool IsPasswordStrong(string? password)
        {
            // Фаза Refactor: итоговая версия после всех циклов TDD
            // Проверка null/empty добавлена при рефакторинге
            if (string.IsNullOrEmpty(password))
            {
                return false;
            }

            // LINQ: Any(char.IsDigit) — хотя бы одна цифра
            return password.Length >= 8 && password.Any(char.IsDigit);
        }
    }
}
