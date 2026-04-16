using JetBrains.Annotations;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "SaveData", menuName = "Scriptable Objects/SaveData")]
public class SaveData : ScriptableObject
{
    public List<DateData> _dateDatas = new();

    public void AddData(DateData data)
    {
        _dateDatas.Add(data);
    }


    public bool TryGetDateData(DateTime date, out DateData data)
    {
        DateData fod = _dateDatas.FirstOrDefault(x => x.date == date);
        if (fod != null)
        {
            data = fod;
            return true;
        }
        else
        {
            data = null;
            return false;
        }
    }
}

[Serializable]

public class DateData
{
    public DateTime date;
    public string note;
    public Color highlightColor;
}

