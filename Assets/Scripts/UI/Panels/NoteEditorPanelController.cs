using System;
using System.Globalization;
using Schedule.Core;
using UnityEngine.UIElements;

namespace Schedule.UI.Panels
{
    /// <summary>
    /// Оверлей редактирования заметки на дату.
    /// </summary>
    public class NoteEditorPanelController : IDisposable
    {
        public const string ElementName = "note-editor-panel";

        private const string HiddenClass = "panel--hidden";

        private CultureInfo _culture;
        private VisualElement _root;
        private VisualElement _scrim;
        private Label _title;
        private TextField _input;
        private Button _save;
        private Button _close;

        private DateTime _date;

        public event Action<DateTime, string> Saved;
        public event Action Closed;

        public bool IsOpen => _root != null && !_root.ClassListContains(HiddenClass);

        public void Initialize(VisualElement root, LocalizationConfig localization)
        {
            _root = root;
            _culture = localization.Culture;

            _scrim = _root.Q<VisualElement>("note-scrim");
            _title = _root.Q<Label>("note-title");
            _input = _root.Q<TextField>("note-input");
            _save = _root.Q<Button>("note-save");
            _close = _root.Q<Button>("note-close");

            _save.clicked += OnSaveClicked;
            _close.clicked += OnCloseClicked;
            _scrim.RegisterCallback<ClickEvent>(OnScrimClicked);

            Hide();
        }

        public void Open(DateTime date, string note)
        {
            _date = date.Date;
            _title.text = _date.ToString("d MMMM yyyy", _culture);
            _input.SetValueWithoutNotify(note ?? string.Empty);

            _root.RemoveFromClassList(HiddenClass);
            _input.schedule.Execute(() => _input.Focus());
        }

        public void Close()
        {
            if (!IsOpen)
            {
                return;
            }

            Hide();
            Closed?.Invoke();
        }

        public void Dispose()
        {
            if (_save != null)
            {
                _save.clicked -= OnSaveClicked;
            }

            if (_close != null)
            {
                _close.clicked -= OnCloseClicked;
            }

            _scrim?.UnregisterCallback<ClickEvent>(OnScrimClicked);

            Saved = null;
            Closed = null;
        }

        private void Hide() => _root.AddToClassList(HiddenClass);

        private void OnSaveClicked()
        {
            DateTime date = _date;
            string note = _input.value;

            Hide();
            Saved?.Invoke(date, note);
            Closed?.Invoke();
        }

        private void OnCloseClicked() => Close();

        private void OnScrimClicked(ClickEvent evt)
        {
            if (evt.target == _scrim)
            {
                Close();
            }
        }
    }
}
