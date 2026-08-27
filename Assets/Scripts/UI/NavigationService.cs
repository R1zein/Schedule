using System;

namespace UI
{
    /// <summary>Вкладка называется Tab, а не Screen: Screen занят UnityEngine.</summary>
    public enum Tab
    {
        Calendar,
        ScheduleEditor,
    }

    /// <summary>
    /// Текущая вкладка. Панель вкладок пишет сюда, экраны подписываются
    /// и прячут себя сами — так вкладки не знают, какие панели существуют.
    /// </summary>
    public class NavigationService
    {
        public Tab Current { get; private set; } = Tab.Calendar;

        public event Action Changed;

        public void Show(Tab tab)
        {
            if (Current == tab)
            {
                return;
            }

            Current = tab;
            Changed?.Invoke();
        }
    }
}
