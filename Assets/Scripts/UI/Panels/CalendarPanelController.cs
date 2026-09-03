using System;
using System.Collections.Generic;
using Core.Calendar;
using Core.Lessons;
using Core.Notes;
using UI.Elements;
using UnityEngine.UIElements;
using Zenject;

namespace UI.Panels
{
    /// <summary>
    /// Сетка месяца: строит недели и ячейки из динамических шаблонов
    /// и держит выделение — одну открытую ячейку.
    /// </summary>
    public class CalendarPanelController : IInitializable, IDisposable
    {
        private readonly List<VisualElement> _rows = new();
        private readonly List<DaySlotView> _slots = new();

        [Inject] private UIDocument _document;
        [Inject] private UITemplateLibrary _templates;
        [Inject] private CalendarService _calendar;
        [Inject] private NoteService _notes;
        [Inject] private ScheduleService _schedule;
        [Inject] private NoteEditorPanelController _noteEditor;
        [Inject] private NavigationService _navigation;
        [Inject] private DayLessonsPanelController _dayLessons;
        [Inject] private ViewFactory _views;

        private VisualElement _root;
        private VisualElement _grid;
        private DaySlotView _selected;

        public void Initialize()
        {
            _root = _document.rootVisualElement.Q<VisualElement>("calendar-panel");
            _grid = _root.Q<VisualElement>("calendar-grid");

            for (int i = 0; i < CalendarService.DaysInWeek; i++)
            {
                _root.Q<Label>($"weekday-{i}").text = _calendar.GetWeekDayName(i);
            }

            _calendar.Changed += Rebuild;
            _notes.NoteChanged += OnNoteChanged;
            _schedule.Changed += Rebuild;
            _navigation.Changed += UpdateVisibility;

            UpdateVisibility();
            Rebuild();
        }

        public void Dispose()
        {
            _calendar.Changed -= Rebuild;
            _notes.NoteChanged -= OnNoteChanged;
            _schedule.Changed -= Rebuild;
            _navigation.Changed -= UpdateVisibility;

            Clear();
        }

        private void Rebuild()
        {
            Clear();

            IReadOnlyList<DayCell> days = _calendar.Days;
            VisualElement row = null;

            for (int i = 0; i < days.Count; i++)
            {
                if (i % CalendarService.DaysInWeek == 0)
                {
                    TemplateContainer rowContainer = _templates.WeekRow.Instantiate();
                    rowContainer.AddToClassList("week-row-container");
                    _grid.Add(rowContainer);
                    _rows.Add(rowContainer);

                    // ячейки кладём в сам week-row, а не в обёртку шаблона:
                    // у TemplateContainer направление column, и строка бы рассыпалась в столбец
                    row = rowContainer.Q<VisualElement>("week-row");
                }

                DaySlotView slot = _views.CreateDaySlot();
                slot.Bind(days[i], _notes.HasNote(days[i].Date));
                slot.SetLessonCount(_schedule.GetLessons(days[i].Date).Count);
                slot.Selected += OnSlotSelected;
                slot.NoteRequested += OnSlotNoteRequested;
                slot.DetailsRequested += OnSlotDetailsRequested;

                row.Add(slot.Root);
                _slots.Add(slot);
            }
        }

        private void UpdateVisibility()
        {
            _root.EnableInClassList("panel--hidden", _navigation.Current != Tab.Calendar);
        }

        private void OnNoteChanged(DateTime date)
        {
            foreach (DaySlotView slot in _slots)
            {
                if (slot.Date == date.Date)
                {
                    slot.SetHasNote(_notes.HasNote(date.Date));
                }
            }

            ClearSelection();
        }

        private void ClearSelection()
        {
            _selected?.SetSelected(false);
            _selected = null;
        }

        private void Clear()
        {
            ClearSelection();

            for (int i = _slots.Count - 1; i >= 0; i--)
            {
                _slots[i].Selected -= OnSlotSelected;
                _slots[i].NoteRequested -= OnSlotNoteRequested;
                _slots[i].DetailsRequested -= OnSlotDetailsRequested;
                _slots[i].Dispose();
            }

            _slots.Clear();

            for (int i = _rows.Count - 1; i >= 0; i--)
            {
                _rows[i].RemoveFromHierarchy();
            }

            _rows.Clear();
        }

        private void OnSlotSelected(DaySlotView slot)
        {
            if (_selected == slot)
            {
                ClearSelection();
                return;
            }

            _selected?.SetSelected(false);
            _selected = slot;
            _selected.SetSelected(true);
        }

        private void OnSlotNoteRequested(DaySlotView slot) => _noteEditor.Open(slot.Date);

        private void OnSlotDetailsRequested(DaySlotView slot) => _dayLessons.Open(slot.Date);
    }
}
