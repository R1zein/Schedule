using System;
using Core;
using Core.Notes;
using UnityEngine.UIElements;
using Zenject;

namespace UI.Panels
{
    /// <summary>
    /// Оверлей редактирования заметки на дату.
    /// </summary>
    public class NoteEditorPanelController : IInitializable, IDisposable
    {
        private const string HiddenClass = "panel--hidden";

        [Inject] private UIDocument _document;
        [Inject] private LocalizationConfig _localization;
        [Inject] private NoteService _notes;

        private VisualElement _root;
        private VisualElement _scrim;
        private Label _title;
        private TextField _input;
        private Button _save;
        private Button _close;

        private DateTime _date;

        public bool IsOpen => !_root.ClassListContains(HiddenClass);

        public void Initialize()
        {
            _root = _document.rootVisualElement.Q<VisualElement>("note-editor-panel");

            _scrim = _root.Q<VisualElement>("note-scrim");
            _title = _root.Q<Label>("note-title");
            _input = _root.Q<TextField>("note-input");
            _save = _root.Q<Button>("note-save");
            _close = _root.Q<Button>("note-close");

            _save.clicked += OnSaveClicked;
            _close.clicked += Close;
            _scrim.RegisterCallback<ClickEvent>(OnScrimClicked);

            Hide();
        }

        public void Open(DateTime date)
        {
            _date = date.Date;
            _title.text = _date.ToString("d MMMM yyyy", _localization.Culture);
            _input.SetValueWithoutNotify(_notes.GetNote(_date));

            _root.RemoveFromClassList(HiddenClass);
            _input.schedule.Execute(() => _input.Focus());
        }

        public void Close()
        {
            if (IsOpen)
            {
                Hide();
            }
        }

        public void Dispose()
        {
            _save.clicked -= OnSaveClicked;
            _close.clicked -= Close;
            _scrim.UnregisterCallback<ClickEvent>(OnScrimClicked);
        }

        private void Hide() => _root.AddToClassList(HiddenClass);

        private void OnSaveClicked()
        {
            Hide();
            _notes.SetNote(_date, _input.value);
        }

        private void OnScrimClicked(ClickEvent evt)
        {
            if (evt.target == _scrim)
            {
                Close();
            }
        }
    }
}
