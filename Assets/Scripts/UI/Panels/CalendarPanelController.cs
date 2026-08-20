using System;
using System.Collections.Generic;
using Schedule.Core.Calendar;
using Schedule.Core.Notes;
using Schedule.UI.Elements;
using UnityEngine.UIElements;
using Zenject;

namespace Schedule.UI.Panels
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
        [Inject] private NoteEditorPanelController _noteEditor;
        [Inject] private DaySlotView.Factory _slotFactory;

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

            Rebuild();
        }

        public void Dispose()
        {
            _calendar.Changed -= Rebuild;
            _notes.NoteChanged -= OnNoteChanged;

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

                DaySlotView slot = _slotFactory.Create(_templates.DaySlot.Instantiate());
                slot.Bind(days[i], _notes.HasNote(days[i].Date));
                slot.Selected += OnSlotSelected;
                slot.NoteRequested += OnSlotNoteRequested;

                row.Add(slot.Root);
                _slots.Add(slot);
            }
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
    }
}
