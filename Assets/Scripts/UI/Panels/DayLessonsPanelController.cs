using System;
using System.Collections.Generic;
using Core;
using Core.Data;
using Core.Lessons;
using UnityEngine.UIElements;
using Zenject;

namespace UI.Panels
{
    /// <summary>
    /// Подробное расписание одного дня. Только просмотр: шаблон правится в редакторе недели.
    /// </summary>
    public class DayLessonsPanelController : IInitializable, IDisposable
    {
        private const string HiddenClass = "panel--hidden";

        [Inject] private UIDocument _document;
        [Inject] private UITemplateLibrary _templates;
        [Inject] private ScheduleService _schedule;
        [Inject] private LocalizationConfig _localization;

        private VisualElement _root;
        private VisualElement _scrim;
        private VisualElement _list;
        private Label _title;
        private Label _empty;
        private Button _close;

        public void Initialize()
        {
            _root = _document.rootVisualElement.Q<VisualElement>("day-lessons-panel");

            _scrim = _root.Q<VisualElement>("day-lessons-scrim");
            _list = _root.Q<VisualElement>("day-lessons-list");
            _title = _root.Q<Label>("day-lessons-title");
            _empty = _root.Q<Label>("day-lessons-empty");
            _close = _root.Q<Button>("day-lessons-close");

            _close.clicked += Close;
            _scrim.RegisterCallback<ClickEvent>(OnScrimClicked);

            _root.AddToClassList(HiddenClass);
        }

        public void Open(DateTime date)
        {
            _title.text = date.ToString("d MMMM yyyy", _localization.Culture);

            _list.Clear();
            IReadOnlyList<Lesson> lessons = _schedule.GetLessons(date);
            foreach (Lesson lesson in lessons)
            {
                TemplateContainer line = _templates.LessonLine.Instantiate();
                line.Q<Label>("lesson-line-time").text =
                    $"{Lesson.FormatTime(lesson.startMinutes)} — {Lesson.FormatTime(lesson.endMinutes)}";
                line.Q<Label>("lesson-line-name").text = lesson.name;
                _list.Add(line);
            }

            _empty.EnableInClassList(HiddenClass, lessons.Count > 0);
            _root.RemoveFromClassList(HiddenClass);
        }

        public void Close() => _root.AddToClassList(HiddenClass);

        public void Dispose()
        {
            _close.clicked -= Close;
            _scrim.UnregisterCallback<ClickEvent>(OnScrimClicked);
        }

        private void OnScrimClicked(ClickEvent evt)
        {
            if (evt.target == _scrim)
            {
                Close();
            }
        }
    }
}
