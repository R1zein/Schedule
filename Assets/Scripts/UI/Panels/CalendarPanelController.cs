using System;
using System.Collections.Generic;
using Schedule.Core.Calendar;
using Schedule.Core.Notes;
using Schedule.UI.Elements;
using UnityEngine.UIElements;

namespace Schedule.UI.Panels
{
    /// <summary>
    /// Сетка месяца: строит недели и ячейки из динамических шаблонов,
    /// держит выделение (одна открытая ячейка) и сообщает наверх о запросе заметки.
    /// </summary>
    public class CalendarPanelController : IDisposable
    {
        public const string ElementName = "calendar-panel";

        private readonly List<VisualElement> _rows = new();
        private readonly List<DaySlotView> _slots = new();

        private UITemplateLibrary _templates;
        private NoteService _notes;
        private DaySlotView.Factory _slotFactory;

        private VisualElement _root;
        private VisualElement _grid;
        private DaySlotView _selected;

        public event Action<DateTime> NoteRequested;

        public void Initialize(
            VisualElement root,
            UITemplateLibrary templates,
            NoteService notes,
            DaySlotView.Factory slotFactory)
        {
            _root = root;
            _templates = templates;
            _notes = notes;
            _slotFactory = slotFactory;

            _grid = _root.Q<VisualElement>("calendar-grid");
        }

        public void SetWeekDayNames(Func<int, string> nameProvider)
        {
            for (int i = 0; i < CalendarService.DaysInWeek; i++)
            {
                var label = _root.Q<Label>($"weekday-{i}");
                if (label != null)
                {
                    label.text = nameProvider(i);
                }
            }
        }

        public void Rebuild(IReadOnlyList<DayCell> days)
        {
            Clear();

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

        public void RefreshDay(DateTime date)
        {
            foreach (DaySlotView slot in _slots)
            {
                if (slot.Date == date.Date)
                {
                    slot.SetHasNote(_notes.HasNote(date.Date));
                }
            }
        }

        public void ClearSelection()
        {
            _selected?.SetSelected(false);
            _selected = null;
        }

        public void Dispose()
        {
            Clear();
            NoteRequested = null;
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

        private void OnSlotNoteRequested(DaySlotView slot) => NoteRequested?.Invoke(slot.Date);
    }
}
