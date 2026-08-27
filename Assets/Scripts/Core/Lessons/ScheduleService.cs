using System;
using System.Collections.Generic;
using System.Linq;
using Core.Calendar;
using Core.Data;
using Zenject;

namespace Core.Lessons
{
    /// <summary>
    /// Недельный шаблон уроков. Повторяется на все рабочие дни;
    /// в нерабочие (сб, вс, праздники и переносы) уроков нет.
    /// </summary>
    public class ScheduleService : IInitializable
    {
        private static readonly Lesson[] Empty = Array.Empty<Lesson>();

        [Inject] private SaveData _saveData;

        /// <summary>Бросается, когда меняется количество уроков — правка названия или времени сетку не трогает.</summary>
        public event Action Changed;

        public void Initialize()
        {
            // добиваем недостающие дни, чтобы редактору всегда было что показать
            for (int i = 0; i < 7; i++)
            {
                DayOfWeek day = (DayOfWeek)i;
                if (_saveData.Week.All(d => d.day != day))
                {
                    _saveData.Week.Add(new DaySchedule { day = day });
                }
            }
        }

        public IReadOnlyList<Lesson> GetLessons(DateTime date)
        {
            return RussianDaysOff.IsDayOff(date) ? Empty : GetTemplate(date.DayOfWeek);
        }

        public List<Lesson> GetTemplate(DayOfWeek day)
        {
            return _saveData.Week.First(d => d.day == day).lessons;
        }

        public Lesson AddLesson(DayOfWeek day)
        {
            List<Lesson> lessons = GetTemplate(day);
            Lesson previous = lessons.LastOrDefault();
            Lesson lesson = previous == null
                ? new Lesson()
                : new Lesson
                {
                    startMinutes = previous.endMinutes + 10,
                    endMinutes = previous.endMinutes + 55,
                };

            lessons.Add(lesson);
            Changed?.Invoke();
            return lesson;
        }

        public void RemoveLesson(DayOfWeek day, Lesson lesson)
        {
            GetTemplate(day).Remove(lesson);
            Changed?.Invoke();
        }
    }
}
