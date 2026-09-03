using System;
using UnityEngine;

namespace Core.Data
{
    /// <summary>
    /// Что привязано к конкретной дате: заметка и подсветка.
    /// Дата лежит тиками — ни Unity, ни JsonUtility не сериализуют DateTime.
    /// </summary>
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
