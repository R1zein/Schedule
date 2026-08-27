using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Data
{
    [CreateAssetMenu(fileName = "SaveData", menuName = "Scriptable Objects/SaveData")]
    public class SaveData : ScriptableObject
    {
        [SerializeField] private List<DateData> _dateDatas = new();
        [SerializeField] private List<DaySchedule> _week = new();

        public IReadOnlyList<DateData> Entries => _dateDatas;

        /// <summary>Шаблон недели: семь дней, порядок неважен — ищется по DayOfWeek.</summary>
        public List<DaySchedule> Week => _week;

        public bool TryGetDateData(DateTime date, out DateData data)
        {
            data = _dateDatas.FirstOrDefault(x => x.date == date.Date);
            return data != null;
        }

        public void SetNote(DateTime date, string note)
        {
            date = date.Date;
            bool empty = string.IsNullOrWhiteSpace(note);

            if (TryGetDateData(date, out DateData existing))
            {
                if (empty && existing.highlightColor.a <= 0f)
                {
                    _dateDatas.Remove(existing);
                }
                else
                {
                    existing.note = note;
                }
            }
            else if (!empty)
            {
                _dateDatas.Add(new DateData { date = date, note = note });
            }
        }
    }

    [Serializable]
    public class DateData
    {
        [SerializeField] private long dateTicks;

        public string note;
        public Color highlightColor;

        public DateTime date
        {
            get => new DateTime(dateTicks);
            set => dateTicks = value.Date.Ticks;
        }
    }
}
