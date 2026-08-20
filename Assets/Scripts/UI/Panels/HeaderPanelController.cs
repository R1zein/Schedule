using System;
using Schedule.Core.Calendar;
using UnityEngine.UIElements;
using Zenject;

namespace Schedule.UI.Panels
{
    /// <summary>
    /// Шапка: заголовок месяца и переключение месяцев.
    /// </summary>
    public class HeaderPanelController : IInitializable, IDisposable
    {
        [Inject] private UIDocument _document;
        [Inject] private CalendarService _calendar;

        private Label _title;
        private Button _previous;
        private Button _next;

        public void Initialize()
        {
            VisualElement root = _document.rootVisualElement.Q<VisualElement>("header-panel");

            _title = root.Q<Label>("header-title");
            _previous = root.Q<Button>("header-previous");
            _next = root.Q<Button>("header-next");

            _previous.clicked += _calendar.PreviousMonth;
            _next.clicked += _calendar.NextMonth;
            _calendar.Changed += UpdateTitle;

            UpdateTitle();
        }

        public void Dispose()
        {
            _previous.clicked -= _calendar.PreviousMonth;
            _next.clicked -= _calendar.NextMonth;
            _calendar.Changed -= UpdateTitle;
        }

        private void UpdateTitle() => _title.text = _calendar.Title;
    }
}
