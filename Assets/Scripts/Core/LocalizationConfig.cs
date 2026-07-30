using System.Globalization;

namespace Schedule.Core
{
    /// <summary>
    /// Настройки локали. Обычный класс: создаётся инсталлером и раздаётся сервисам,
    /// чтобы культура не была зашита в код.
    /// </summary>
    public class LocalizationConfig
    {
        public LocalizationConfig(string cultureName)
        {
            Culture = string.IsNullOrWhiteSpace(cultureName)
                ? CultureInfo.CurrentCulture
                : new CultureInfo(cultureName);
        }

        public CultureInfo Culture { get; }
    }
}
