using System;
using Schedule.Core;
using Schedule.Core.Calendar;
using Schedule.Core.Notes;
using Schedule.UI.Elements;
using Schedule.UI.Panels;
using UnityEngine.UIElements;
using Zenject;

namespace Schedule.UI
{
    /// <summary>
    /// Вершина иерархии UI и единственный IInitializable в слое.
    /// Сам разбирает main.uxml на панели, раздаёт им зависимости и связывает между собой,
    /// поэтому порядок инициализации — это просто порядок строк здесь.
    /// Знает обо всех — о нём не знает никто.
    /// </summary>
    public class UIRootController : IInitializable, IDisposable
    {
        private readonly UIDocument _document;
        private readonly CalendarService _calendar;
        private readonly NoteService _notes;
        private readonly UITemplateLibrary _templates;
        private readonly LocalizationConfig _localization;
        private readonly DaySlotView.Factory _slotFactory;

        private readonly HeaderPanelController _header;
        private readonly CalendarPanelController _calendarPanel;
        private readonly NoteEditorPanelController _noteEditor;

        public UIRootController(
            UIDocument document,
            CalendarService calendar,
            NoteService notes,
            UITemplateLibrary templates,
            LocalizationConfig localization,
            DaySlotView.Factory slotFactory,
            HeaderPanelController header,
            CalendarPanelController calendarPanel,
            NoteEditorPanelController noteEditor)
        {
            _document = document;
            _calendar = calendar;
            _notes = notes;
            _templates = templates;
            _localization = localization;
            _slotFactory = slotFactory;
            _header = header;
            _calendarPanel = calendarPanel;
            _noteEditor = noteEditor;
        }

        public void Initialize()
        {
            VisualElement root = _document.rootVisualElement;
            if (root == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(UIDocument)} на объекте '{_document.name}' без Source Asset — назначь main.uxml.");
            }

            _header.Initialize(Find(root, HeaderPanelController.ElementName));
            _calendarPanel.Initialize(
                Find(root, CalendarPanelController.ElementName),
                _templates,
                _notes,
                _slotFactory);
            _noteEditor.Initialize(
                Find(root, NoteEditorPanelController.ElementName),
                _localization);

            _header.PreviousRequested += _calendar.PreviousMonth;
            _header.NextRequested += _calendar.NextMonth;
            _calendarPanel.NoteRequested += OnNoteRequested;
            _noteEditor.Saved += OnNoteSaved;
            _calendar.Changed += OnCalendarChanged;

            _calendarPanel.SetWeekDayNames(_calendar.GetWeekDayName);
            OnCalendarChanged();
        }

        public void Dispose()
        {
            _header.PreviousRequested -= _calendar.PreviousMonth;
            _header.NextRequested -= _calendar.NextMonth;
            _calendarPanel.NoteRequested -= OnNoteRequested;
            _noteEditor.Saved -= OnNoteSaved;
            _calendar.Changed -= OnCalendarChanged;

            _header.Dispose();
            _calendarPanel.Dispose();
            _noteEditor.Dispose();
        }

        private static VisualElement Find(VisualElement root, string name)
        {
            VisualElement element = root.Q<VisualElement>(name);
            if (element == null)
            {
                throw new InvalidOperationException($"В main.uxml нет элемента с именем '{name}'.");
            }

            return element;
        }

        private void OnCalendarChanged()
        {
            _header.SetTitle(_calendar.Title);
            _calendarPanel.Rebuild(_calendar.Days);
        }

        private void OnNoteRequested(DateTime date)
        {
            _noteEditor.Open(date, _notes.GetNote(date));
        }

        private void OnNoteSaved(DateTime date, string note)
        {
            _notes.SetNote(date, note);
            _calendarPanel.RefreshDay(date);
            _calendarPanel.ClearSelection();
        }
    }
}
