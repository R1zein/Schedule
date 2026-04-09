using UnityEngine;
using System;
using System.Collections.Generic;
using Mono.Cecil.Cil;

public class SlotGenerator : MonoBehaviour
{
    [SerializeField] private Slot _slot;
    
    private List<DateTime> dates = new();
    private int currMonth = 3;
    private int currYear = 2026;
    [SerializeField] private List<Slot> slots = new();
    public event Action <string> OnDateChange ;


    private void Awake()
    {
        GenerateSlots();
        OnDateChange(new DateTime(currYear, currMonth, 1).ToString("MMMM yyyy", new System.Globalization.CultureInfo("ru-RU")));
    }

    private void GenerateSlots()
    {
        dates.Clear();
        DateTime first = new DateTime(currYear, currMonth, 1);
        int dow = (int)first.DayOfWeek;
        int daysBack = dow == 0 ? 6 : dow - 1;
        DateTime day = first.AddDays(-daysBack);

        var daysInMonth = DateTime.DaysInMonth(currYear, currMonth) + daysBack;
        for (int i = 0; i < daysInMonth; i++)
        {
            dates.Add(day);
            day = day.AddDays(1);
        }
        foreach (DateTime date in dates)
        {
            var slot = Instantiate(_slot, transform);
            slot.SetData(date);
            slots.Add(slot);
        }
    }

    private void DeleteSlots()
    {
        for (int i = slots.Count-1;i >=0;i--)
        {
            var slot  = slots[i];
            slots.Remove(slot);
            Destroy(slot.gameObject);
        }
    }

    public void NextMonth()
    {
        currMonth++;
        if (currMonth > 12)
        {
            currYear += 1;
            currMonth = 1;
        }
        DeleteSlots();
        GenerateSlots();
        OnDateChange(new DateTime(currYear, currMonth, 1).ToString("MMMM yyyy", new System.Globalization.CultureInfo("ru-RU")));

    }

    public void PreviousMonth()
    { 
        currMonth--;
        if (currMonth < 1)
        {
            currYear -= 1;
            currMonth = 12;
        }
        DeleteSlots();
        GenerateSlots();
        OnDateChange(new DateTime(currYear, currMonth, 1).ToString("MMMM yyyy", new System.Globalization.CultureInfo("ru-RU")));

    }

}
