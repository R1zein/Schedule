using System;
using Core.Data;
using UnityEngine.UIElements;
using Zenject;

namespace UI.Elements
{
    /// <summary>
    /// Строка урока в редакторе: название и время правятся прямо в полях.
    /// Правки уходят в модель на месте — пересобирать сетку календаря из-за них не нужно.
    /// </summary>
    public class LessonRowView : IDisposable
    {
        private readonly VisualElement _root;
        private readonly TextField _name;
        private readonly TextField _start;
        private readonly TextField _end;
        private readonly Button _remove;

        private Lesson _lesson;

        public event Action<LessonRowView> RemoveRequested;

        public LessonRowView(TemplateContainer root)
        {
            _root = root;

            _name = _root.Q<TextField>("lesson-name");
            _start = _root.Q<TextField>("lesson-start");
            _end = _root.Q<TextField>("lesson-end");
            _remove = _root.Q<Button>("lesson-remove");

            _name.RegisterValueChangedCallback(OnNameChanged);
            _start.RegisterValueChangedCallback(OnStartChanged);
            _end.RegisterValueChangedCallback(OnEndChanged);
            _remove.clicked += OnRemoveClicked;
        }

        public VisualElement Root => _root;

        public DayOfWeek Day { get; private set; }

        public Lesson Model => _lesson;

        public void Bind(DayOfWeek day, Lesson lesson)
        {
            Day = day;
            _lesson = lesson;

            _name.SetValueWithoutNotify(lesson.name);
            _start.SetValueWithoutNotify(Lesson.FormatTime(lesson.startMinutes));
            _end.SetValueWithoutNotify(Lesson.FormatTime(lesson.endMinutes));
        }

        public void Dispose()
        {
            _name.UnregisterValueChangedCallback(OnNameChanged);
            _start.UnregisterValueChangedCallback(OnStartChanged);
            _end.UnregisterValueChangedCallback(OnEndChanged);
            _remove.clicked -= OnRemoveClicked;

            RemoveRequested = null;
            _root.RemoveFromHierarchy();
        }

        private void OnNameChanged(ChangeEvent<string> evt) => _lesson.name = evt.newValue;

        // непонятное время не принимаем и возвращаем в поле прежнее значение
        private void OnStartChanged(ChangeEvent<string> evt)
        {
            if (Lesson.TryParseTime(evt.newValue, out int minutes))
            {
                _lesson.startMinutes = minutes;
            }

            _start.SetValueWithoutNotify(Lesson.FormatTime(_lesson.startMinutes));
        }

        private void OnEndChanged(ChangeEvent<string> evt)
        {
            if (Lesson.TryParseTime(evt.newValue, out int minutes))
            {
                _lesson.endMinutes = minutes;
            }

            _end.SetValueWithoutNotify(Lesson.FormatTime(_lesson.endMinutes));
        }

        private void OnRemoveClicked() => RemoveRequested?.Invoke(this);

        public class Factory : PlaceholderFactory<TemplateContainer, LessonRowView>
        {
        }
    }
}
