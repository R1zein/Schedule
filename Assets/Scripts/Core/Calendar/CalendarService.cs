using System;
using System.Collections.Generic;
using System.Globalization;
using Zenject;

namespace Core.Calendar
{
    /// <summary>
    /// Модель календаря: текущий месяц и его сетка. Ни о каком UI не знает,
    /// сообщает о смене месяца только через событие <see cref="Changed"/>.
    /// </summary>
    public class CalendarService : IInitializable
    {
        public const int DaysInWeek = 7;

        private CultureInfo _culture;
        private List<DayCell> _days = new();

        private DateTime _month;

        public event Action Changed;

        [Inject] private LocalizationConfig _localization;

        public void Initialize()
        {
            _culture = _localization.Culture;

            DateTime today = DateTime.Today;
            SetMonth(new DateTime(today.Year, today.Month, 1));
        }

        public IReadOnlyList<DayCell> Days => _days;

        public int WeekCount => _days.Count / DaysInWeek;

        public DateTime Month => _month;

        public string Title => Capitalize(_month.ToString("MMMM yyyy", _culture));

        // индекс 0 — понедельник: и сетка календаря, и редактор расписания начинаются с него
        public string GetWeekDayName(int index) => Capitalize(_culture.DateTimeFormat.AbbreviatedDayNames[(index + 1) % DaysInWeek]);

        public string GetWeekDayFullName(int index) => Capitalize(_culture.DateTimeFormat.DayNames[(index + 1) % DaysInWeek]);

        private string Capitalize(string text) => char.ToUpper(text[0], _culture) + text.Substring(1);

        public void NextMonth() => SetMonth(_month.AddMonths(1));

        public void PreviousMonth() => SetMonth(_month.AddMonths(-1));

        public void GoToMonth(DateTime date) => SetMonth(new DateTime(date.Year, date.Month, 1));

        private void SetMonth(DateTime month)
        {
            _month = month;
            Rebuild();
            Changed?.Invoke();
        }

        private void Rebuild()
        {
            _days.Clear();

            int leading = ((int)_month.DayOfWeek + 6) % DaysInWeek; // понедельник — первый день недели
            DateTime first = _month.AddDays(-leading);

            int used = leading + DateTime.DaysInMonth(_month.Year, _month.Month);
            int total = (used + DaysInWeek - 1) / DaysInWeek * DaysInWeek; // добиваем до целых недель

            DateTime today = DateTime.Today;
            for (int i = 0; i < total; i++)
            {
                DateTime date = first.AddDays(i);
                bool current = date.Year == _month.Year && date.Month == _month.Month;
                _days.Add(new DayCell(date, current, date == today, RussianDaysOff.IsDayOff(date)));
            }
        }
    }
}
