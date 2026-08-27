using System;

namespace Core.Calendar
{
    /// <summary>
    /// Одна ячейка месячной сетки. Чистые данные: о том, как это рисуется, слой не знает.
    /// </summary>
    public readonly struct DayCell
    {
        public readonly DateTime Date;
        public readonly bool IsCurrentMonth;
        public readonly bool IsToday;
        public readonly bool IsDayOff;

        public DayCell(DateTime date, bool isCurrentMonth, bool isToday, bool isDayOff)
        {
            Date = date;
            IsCurrentMonth = isCurrentMonth;
            IsToday = isToday;
            IsDayOff = isDayOff;
        }
    }
}
