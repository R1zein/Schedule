using System;
using UnityEngine.UIElements;
using Zenject;

namespace UI.Panels
{
    /// <summary>Переключатель экранов над шапкой.</summary>
    public class TabsPanelController : IInitializable, IDisposable
    {
        [Inject] private UIDocument _document;
        [Inject] private NavigationService _navigation;

        private Button _calendar;
        private Button _schedule;

        public void Initialize()
        {
            VisualElement root = _document.rootVisualElement.Q<VisualElement>("tabs-panel");

            _calendar = root.Q<Button>("tab-calendar");
            _schedule = root.Q<Button>("tab-schedule");

            _calendar.clicked += ShowCalendar;
            _schedule.clicked += ShowSchedule;
            _navigation.Changed += UpdateActive;

            UpdateActive();
        }

        public void Dispose()
        {
            _calendar.clicked -= ShowCalendar;
            _schedule.clicked -= ShowSchedule;
            _navigation.Changed -= UpdateActive;
        }

        private void ShowCalendar() => _navigation.Show(Tab.Calendar);

        private void ShowSchedule() => _navigation.Show(Tab.ScheduleEditor);

        private void UpdateActive()
        {
            _calendar.EnableInClassList("tab--active", _navigation.Current == Tab.Calendar);
            _schedule.EnableInClassList("tab--active", _navigation.Current == Tab.ScheduleEditor);
        }
    }
}
