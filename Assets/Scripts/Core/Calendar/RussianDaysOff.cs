using System;
using System.Collections.Generic;

namespace Schedule.Core.Calendar
{
    /// <summary>
    /// Нерабочие дни в РФ: суббота с воскресеньем, праздники по ст. 112 ТК
    /// и ежегодные переносы выходных, которые правительство утверждает постановлением.
    /// Таблица переносов заполнена по производственному календарю с 2018 по 2027 год;
    /// за её пределами остаются только выходные и фиксированные праздники.
    /// </summary>
    public static class RussianDaysOff
    {
        /// <summary>Выходные, перенесённые на будни: даты, которые не суббота, не воскресенье и не праздник.</summary>
        private static readonly HashSet<DateTime> Transferred = new()
        {
            // 2018: 6 и 7 января → 9 марта и 2 мая; субботы 28.04, 09.06, 29.12 → понедельники; 4 ноября — воскресенье
            new DateTime(2018, 3, 9), new DateTime(2018, 4, 30), new DateTime(2018, 5, 2),
            new DateTime(2018, 6, 11), new DateTime(2018, 11, 5), new DateTime(2018, 12, 31),

            // 2019: 5 и 6 января, 23 февраля → 2, 3 и 10 мая
            new DateTime(2019, 5, 2), new DateTime(2019, 5, 3), new DateTime(2019, 5, 10),

            // 2020: 4 и 5 января → 4 и 5 мая; 23.02, 08.03 и 09.05 выпали на выходные
            new DateTime(2020, 2, 24), new DateTime(2020, 3, 9), new DateTime(2020, 5, 4),
            new DateTime(2020, 5, 5), new DateTime(2020, 5, 11),

            // 2021: 2 и 3 января → 5 ноября и 31 декабря; суббота 20.02 → 22 февраля
            new DateTime(2021, 2, 22), new DateTime(2021, 5, 3), new DateTime(2021, 5, 10),
            new DateTime(2021, 6, 14), new DateTime(2021, 11, 5), new DateTime(2021, 12, 31),

            // 2022: 1 и 2 января → 3 и 10 мая; суббота 05.03 → 7 марта
            new DateTime(2022, 3, 7), new DateTime(2022, 5, 2), new DateTime(2022, 5, 3),
            new DateTime(2022, 5, 10), new DateTime(2022, 6, 13),

            // 2023: 1 и 8 января → 24 февраля и 8 мая; 4 ноября — суббота
            new DateTime(2023, 2, 24), new DateTime(2023, 5, 8), new DateTime(2023, 11, 6),

            // 2024: 6 и 7 января → 10 мая и 31 декабря; субботы 27.04, 02.11, 28.12 → 29.04, 30.04, 30.12
            new DateTime(2024, 4, 29), new DateTime(2024, 4, 30), new DateTime(2024, 5, 10),
            new DateTime(2024, 12, 30), new DateTime(2024, 12, 31),

            // 2025: 4 и 5 января → 2 мая и 31 декабря; 23.02 и 08.03 → 8 мая и 13 июня; суббота 01.11 → 3 ноября
            new DateTime(2025, 5, 2), new DateTime(2025, 5, 8), new DateTime(2025, 6, 13),
            new DateTime(2025, 11, 3), new DateTime(2025, 12, 31),

            // 2026: 3 и 4 января → 9 января и 31 декабря; 08.03 и 09.05 выпали на выходные
            new DateTime(2026, 1, 9), new DateTime(2026, 3, 9), new DateTime(2026, 5, 11),
            new DateTime(2026, 12, 31),

            // 2027: 2 и 3 января → 5 ноября и 31 декабря; суббота 20.02 → 22 февраля
            new DateTime(2027, 2, 22), new DateTime(2027, 5, 3), new DateTime(2027, 5, 10),
            new DateTime(2027, 6, 14), new DateTime(2027, 11, 5), new DateTime(2027, 12, 31),
        };

        /// <summary>Субботы, ставшие рабочими: выходной с них уехал на будний день.</summary>
        private static readonly HashSet<DateTime> Working = new()
        {
            new DateTime(2018, 4, 28), new DateTime(2018, 6, 9), new DateTime(2018, 12, 29),
            new DateTime(2021, 2, 20),
            new DateTime(2022, 3, 5),
            new DateTime(2024, 4, 27), new DateTime(2024, 11, 2), new DateTime(2024, 12, 28),
            new DateTime(2025, 11, 1),
            new DateTime(2027, 2, 20),
        };

        public static bool IsDayOff(DateTime date)
        {
            DateTime day = date.Date;

            if (Working.Contains(day))
            {
                return false;
            }

            return day.DayOfWeek == DayOfWeek.Saturday
                || day.DayOfWeek == DayOfWeek.Sunday
                || IsHoliday(day)
                || Transferred.Contains(day);
        }

        /// <summary>Нерабочие праздничные дни по ст. 112 ТК РФ.</summary>
        public static bool IsHoliday(DateTime date)
        {
            return (date.Month, date.Day) switch
            {
                (1, >= 1 and <= 8) => true, // новогодние каникулы и Рождество
                (2, 23) => true,            // День защитника Отечества
                (3, 8) => true,             // Международный женский день
                (5, 1) => true,             // Праздник Весны и Труда
                (5, 9) => true,             // День Победы
                (6, 12) => true,            // День России
                (11, 4) => true,            // День народного единства
                _ => false,
            };
        }
    }
}
