using System;
using System.Collections.Generic;
using System.Linq;
using Core.Data;
using Core.Saving;
using Zenject;

namespace Core.Notes
{
    /// <summary>
    /// Заметки по датам. Владеет своим списком и сохраняет его сама;
    /// о UI не знает, об изменениях сообщает событием <see cref="NoteChanged"/>.
    /// </summary>
    public class NoteService : IInitializable, IDisposable, ISaveLoadable
    {
        [Inject] private SaveLoadService _saveLoad;

        private readonly List<DateData> _dates = new();

        public event Action<DateTime> NoteChanged;

        public string Key => "Notes";

        public void Initialize() => _saveLoad.Register(this);

        public void Dispose() => _saveLoad.Unregister(this);

        public string GetNote(DateTime date)
        {
            return TryGet(date.Date, out DateData data) ? data.note : string.Empty;
        }

        public bool HasNote(DateTime date)
        {
            return !string.IsNullOrWhiteSpace(GetNote(date));
        }

        public void SetNote(DateTime date, string note)
        {
            date = date.Date;
            bool empty = string.IsNullOrWhiteSpace(note);

            if (TryGet(date, out DateData existing))
            {
                // пустая заметка без подсветки — просто мусор в файле
                if (empty && existing.highlightColor.a <= 0f)
                {
                    _dates.Remove(existing);
                }
                else
                {
                    existing.note = note;
                }
            }
            else if (!empty)
            {
                _dates.Add(new DateData { date = date, note = note });
            }

            NoteChanged?.Invoke(date);
            _saveLoad.Save();
        }

        

        private bool TryGet(DateTime date, out DateData data)
        {
            data = _dates.FirstOrDefault(x => x.date == date);
            return data != null;
        }

        public object SaveState() => new Data { dates = _dates };

        public void LoadState(SaveFile file)
        {
            Data data = file.GetState<Data>(Key);
            if (data?.dates == null)
            {
                return;
            }

            _dates.Clear();
            _dates.AddRange(data.dates);
        }
        [Serializable]
        private class Data
        {
            public List<DateData> dates = new();
        }
    }
}
