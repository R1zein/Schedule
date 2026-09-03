using System;
using System.Collections.Generic;
using System.Linq;
using Core.Calendar;
using Core.Data;
using Core.Saving;
using Zenject;

namespace Core.Lessons
{
    /// <summary>
    /// Недельный шаблон уроков. Повторяется на все рабочие дни;
    /// в нерабочие (сб, вс, праздники и переносы) уроков нет.
    /// </summary>
    public class ScheduleService : IInitializable, IDisposable, ISaveLoadable
    {
        private static readonly Lesson[] Empty = Array.Empty<Lesson>();

        [Inject] private SaveLoadService _saveLoad;

        private readonly List<DaySchedule> _week = new();

        /// <summary>Бросается, когда меняется количество уроков — правка названия или времени сетку не трогает.</summary>
        public event Action Changed;

        public string Key => "Schedule";

        public void Initialize()
        {
            _saveLoad.Register(this); // если сейв есть, LoadState отработает прямо здесь

            // добиваем недостающие дни, чтобы редактору всегда было что показать
            for (int i = 0; i < CalendarService.DaysInWeek; i++)
            {
                DayOfWeek day = (DayOfWeek)i;
                if (_week.All(d => d.day != day))
                {
                    _week.Add(new DaySchedule { day = day });
                }
            }
        }

        public void Dispose() => _saveLoad.Unregister(this);

        public IReadOnlyList<Lesson> GetLessons(DateTime date)
        {
            return RussianDaysOff.IsDayOff(date) ? Empty : GetTemplate(date.DayOfWeek);
        }

        public List<Lesson> GetTemplate(DayOfWeek day)
        {
            return _week.First(d => d.day == day).lessons;
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
            _saveLoad.Save();
            return lesson;
        }

        public void RemoveLesson(DayOfWeek day, Lesson lesson)
        {
            GetTemplate(day).Remove(lesson);
            Changed?.Invoke();
            _saveLoad.Save();
        }

        public object SaveState() => new Data { week = _week };

        public void LoadState(SaveFile file)
        {
            Data data = file.GetState<Data>(Key);
            if (data?.week == null)
            {
                return;
            }

            _week.Clear();
            _week.AddRange(data.week);
        }

        [Serializable]
        private class Data
        {
            public List<DaySchedule> week = new();
        }
    }
}
