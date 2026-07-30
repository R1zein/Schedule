using System;
using Schedule.Core.Calendar;
using UnityEngine.UIElements;
using Zenject;

namespace Schedule.UI.Elements
{
    /// <summary>
    /// Обёртка над динамическим шаблоном DaySlot.uxml — одна ячейка дня.
    /// Создаётся фабрикой в рантайме, сам шаблон приходит из <see cref="UITemplateLibrary"/>.
    /// </summary>
    public class DaySlotView : IDisposable
    {
        private const string OutsideClass = "slot--outside";
        private const string TodayClass = "slot--today";
        private const string SelectedClass = "slot--selected";
        private const string HasNoteClass = "slot--has-note";

        private readonly VisualElement _root;
        private readonly VisualElement _slot;
        private readonly Button _surface;
        private readonly Button _addNote;
        private readonly Label _dayLabel;

        public event Action<DaySlotView> Selected;
        public event Action<DaySlotView> NoteRequested;

        public DaySlotView(TemplateContainer root)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
            _root.AddToClassList("slot-cell");

            _slot = _root.Q<VisualElement>("day-slot");
            _surface = _root.Q<Button>("slot-surface");
            _addNote = _root.Q<Button>("slot-add-note");
            _dayLabel = _root.Q<Label>("slot-day");

            _surface.clicked += OnSurfaceClicked;
            _addNote.clicked += OnAddNoteClicked;
        }

        public VisualElement Root => _root;

        public DateTime Date { get; private set; }

        public void Bind(DayCell cell, bool hasNote)
        {
            Date = cell.Date;
            _dayLabel.text = cell.Date.Day.ToString("00");

            _slot.EnableInClassList(OutsideClass, !cell.IsCurrentMonth);
            _slot.EnableInClassList(TodayClass, cell.IsToday);
            SetSelected(false);
            SetHasNote(hasNote);
        }

        public void SetSelected(bool selected)
        {
            _slot.EnableInClassList(SelectedClass, selected);
        }

        public void SetHasNote(bool hasNote)
        {
            _slot.EnableInClassList(HasNoteClass, hasNote);
        }

        public void Dispose()
        {
            _surface.clicked -= OnSurfaceClicked;
            _addNote.clicked -= OnAddNoteClicked;
            Selected = null;
            NoteRequested = null;
            _root.RemoveFromHierarchy();
        }

        private void OnSurfaceClicked() => Selected?.Invoke(this);

        private void OnAddNoteClicked() => NoteRequested?.Invoke(this);

        /// <summary>Фабрика динамических ячеек, биндится в инсталлере.</summary>
        public class Factory : PlaceholderFactory<TemplateContainer, DaySlotView>
        {
        }
    }
}
