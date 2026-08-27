using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Data
{
    /// <summary>
    /// Один урок в шаблоне недели. Время — минуты от полуночи:
    /// TimeSpan Unity не сериализует, а из int формат "09:30" собирается в одну строку.
    /// </summary>
    [Serializable]
    public class Lesson
    {
        public string name = "Урок";
        public int startMinutes = 9 * 60;
        public int endMinutes = 9 * 60 + 45;

        public static string FormatTime(int minutes)
        {
            return $"{minutes / 60:00}:{minutes % 60:00}";
        }

        /// <summary>Разбирает "9:30" или "0930"; при мусоре на входе возвращает false и время не меняется.</summary>
        public static bool TryParseTime(string text, out int minutes)
        {
            minutes = 0;
            string[] parts = text.Trim().Split(':');
            if (parts.Length != 2
                || !int.TryParse(parts[0], out int hours)
                || !int.TryParse(parts[1], out int mins)
                || hours < 0 || hours > 23
                || mins < 0 || mins > 59)
            {
                return false;
            }

            minutes = hours * 60 + mins;
            return true;
        }
    }

    /// <summary>Уроки одного дня недели. Список, а не словарь: словари Unity не сериализует.</summary>
    [Serializable]
    public class DaySchedule
    {
        public DayOfWeek day;
        public List<Lesson> lessons = new();
    }
}
