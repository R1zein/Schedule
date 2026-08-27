using System;
using Core.Data;
using Zenject;

namespace Core.Notes
{
    /// <summary>
    /// Доступ к заметкам поверх <see cref="SaveData"/>. О UI не знает,
    /// об изменениях сообщает событием <see cref="NoteChanged"/>.
    /// </summary>
    public class NoteService : IInitializable
    {
        [Inject] private SaveData _saveData;

        public event Action<DateTime> NoteChanged;

        public void Initialize()
        {
            // сюда встанет загрузка заметок, когда они переедут из ScriptableObject на диск
        }

        public string GetNote(DateTime date)
        {
            return _saveData.TryGetDateData(date.Date, out DateData data) ? data.note : string.Empty;
        }

        public bool HasNote(DateTime date)
        {
            return !string.IsNullOrWhiteSpace(GetNote(date));
        }

        public void SetNote(DateTime date, string note)
        {
            _saveData.SetNote(date.Date, note);
            NoteChanged?.Invoke(date.Date);
        }
    }
}
