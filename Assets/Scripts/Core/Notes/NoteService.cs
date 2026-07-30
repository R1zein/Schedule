using System;
using Schedule.Core.Data;

namespace Schedule.Core.Notes
{
    /// <summary>
    /// Доступ к заметкам поверх <see cref="SaveData"/>. О UI не знает,
    /// об изменениях сообщает событием <see cref="NoteChanged"/>.
    /// </summary>
    public class NoteService
    {
        private readonly SaveData _saveData;

        public event Action<DateTime> NoteChanged;

        public NoteService(SaveData saveData)
        {
            _saveData = saveData != null ? saveData : throw new ArgumentNullException(nameof(saveData));
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
