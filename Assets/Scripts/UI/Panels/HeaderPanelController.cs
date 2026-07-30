using System;
using UnityEngine.UIElements;

namespace Schedule.UI.Panels
{
    /// <summary>
    /// Шапка: заголовок месяца и переключение месяцев.
    /// Всё, что нужно, приходит в <see cref="Initialize"/> от корня UI —
    /// панель ничего не ищет сама и ни от кого не зависит через конструктор.
    /// </summary>
    public class HeaderPanelController : IDisposable
    {
        public const string ElementName = "header-panel";

        private Label _title;
        private Button _previous;
        private Button _next;

        public event Action PreviousRequested;
        public event Action NextRequested;

        public void Initialize(VisualElement root)
        {
            _title = root.Q<Label>("header-title");
            _previous = root.Q<Button>("header-previous");
            _next = root.Q<Button>("header-next");

            _previous.clicked += OnPreviousClicked;
            _next.clicked += OnNextClicked;
        }

        public void SetTitle(string title) => _title.text = title;

        public void Dispose()
        {
            if (_previous != null)
            {
                _previous.clicked -= OnPreviousClicked;
            }

            if (_next != null)
            {
                _next.clicked -= OnNextClicked;
            }

            PreviousRequested = null;
            NextRequested = null;
        }

        private void OnPreviousClicked() => PreviousRequested?.Invoke();

        private void OnNextClicked() => NextRequested?.Invoke();
    }
}
