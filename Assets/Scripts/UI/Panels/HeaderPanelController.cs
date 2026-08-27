using System;
using Core.Calendar;
using UnityEngine.UIElements;
using Zenject;

namespace UI.Panels
{
    /// <summary>
    /// Шапка: заголовок месяца и переключение месяцев. Живёт только на вкладке календаря.
    /// </summary>
    public class HeaderPanelController : IInitializable, IDisposable
    {
        [Inject] private UIDocument _document;
        [Inject] private CalendarService _calendar;
        [Inject] private NavigationService _navigation;

        private VisualElement _root;
        private Label _title;
        private Button _previous;
        private Button _next;

        public void Initialize()
        {
            _root = _document.rootVisualElement.Q<VisualElement>("header-panel");

            _title = _root.Q<Label>("header-title");
            _previous = _root.Q<Button>("header-previous");
            _next = _root.Q<Button>("header-next");

            _previous.clicked += _calendar.PreviousMonth;
            _next.clicked += _calendar.NextMonth;
            _calendar.Changed += UpdateTitle;
            _navigation.Changed += UpdateVisibility;

            UpdateTitle();
            UpdateVisibility();
        }

        public void Dispose()
        {
            _previous.clicked -= _calendar.PreviousMonth;
            _next.clicked -= _calendar.NextMonth;
            _calendar.Changed -= UpdateTitle;
            _navigation.Changed -= UpdateVisibility;
        }

        private void UpdateTitle() => _title.text = _calendar.Title;

        private void UpdateVisibility()
        {
            _root.EnableInClassList("panel--hidden", _navigation.Current != Tab.Calendar);
        }
    }
}
