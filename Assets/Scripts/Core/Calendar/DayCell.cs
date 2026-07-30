using System;

namespace Schedule.Core.Calendar
{
    /// <summary>
    /// Одна ячейка месячной сетки. Чистые данные: о том, как это рисуется, слой не знает.
    /// </summary>
    public readonly struct DayCell
    {
        public readonly DateTime Date;
        public readonly bool IsCurrentMonth;
        public readonly bool IsToday;

        public DayCell(DateTime date, bool isCurrentMonth, bool isToday)
        {
            Date = date;
            IsCurrentMonth = isCurrentMonth;
            IsToday = isToday;
        }
    }
}
