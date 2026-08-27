using System;
using System.Collections.Generic;
using Core.Calendar;
using Core.Data;
using Core.Lessons;
using UI.Elements;
using UnityEngine.UIElements;
using Zenject;

namespace UI.Panels
{
    /// <summary>
    /// Редактор недельного шаблона: семь колонок, в каждой список уроков.
    /// Пересобирается целиком на добавление и удаление — правка полей идёт мимо, в модель.
    /// </summary>
    public class ScheduleEditorPanelController : IInitializable, IDisposable
    {
        private readonly List<LessonRowView> _rows = new();
        private readonly List<VisualElement> _columns = new();

        [Inject] private UIDocument _document;
        [Inject] private UITemplateLibrary _templates;
        [Inject] private ScheduleService _schedule;
        [Inject] private CalendarService _calendar;
        [Inject] private NavigationService _navigation;
        [Inject] private LessonRowView.Factory _rowFactory;

        private VisualElement _root;
        private VisualElement _days;

        public void Initialize()
        {
            _root = _document.rootVisualElement.Q<VisualElement>("schedule-editor-panel");
            _days = _root.Q<VisualElement>("schedule-days");

            _navigation.Changed += UpdateVisibility;
            _schedule.Changed += OnScheduleChanged;

            UpdateVisibility();
            Rebuild();
        }

        public void Dispose()
        {
            _navigation.Changed -= UpdateVisibility;
            _schedule.Changed -= OnScheduleChanged;

            Clear();
        }

        private void UpdateVisibility()
        {
            _root.EnableInClassList("panel--hidden", _navigation.Current != Tab.ScheduleEditor);
        }

        // добавление и удаление приходят из клика по кнопке, которую Rebuild тут же уничтожит,
        // поэтому пересборку откладываем на следующий кадр — иначе рвём дерево прямо в обработчике
        private void OnScheduleChanged() => _root.schedule.Execute(Rebuild);

        private void Rebuild()
        {
            Clear();

            for (int i = 0; i < CalendarService.DaysInWeek; i++)
            {
                DayOfWeek day = (DayOfWeek)((i + 1) % CalendarService.DaysInWeek);

                TemplateContainer column = _templates.ScheduleDay.Instantiate();
                column.AddToClassList("schedule-day-container");
                _days.Add(column);
                _columns.Add(column);

                column.Q<Label>("schedule-day-title").text = _calendar.GetWeekDayFullName(i);

                Button add = column.Q<Button>("schedule-day-add");
                add.clicked += () => _schedule.AddLesson(day);

                VisualElement lessons = column.Q<VisualElement>("schedule-day-lessons");
                foreach (Lesson lesson in _schedule.GetTemplate(day))
                {
                    LessonRowView row = _rowFactory.Create(_templates.LessonRow.Instantiate());
                    row.Bind(day, lesson);
                    row.RemoveRequested += OnRemoveRequested;

                    lessons.Add(row.Root);
                    _rows.Add(row);
                }
            }
        }

        private void OnRemoveRequested(LessonRowView row)
        {
            _schedule.RemoveLesson(row.Day, row.Model);
        }

        private void Clear()
        {
            for (int i = _rows.Count - 1; i >= 0; i--)
            {
                _rows[i].RemoveRequested -= OnRemoveRequested;
                _rows[i].Dispose();
            }

            _rows.Clear();

            for (int i = _columns.Count - 1; i >= 0; i--)
            {
                _columns[i].RemoveFromHierarchy();
            }

            _columns.Clear();
        }
    }
}
