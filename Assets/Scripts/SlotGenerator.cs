using UnityEngine;
using System;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using System.Data;

public class SlotGenerator : MonoBehaviour
{
    [SerializeField] private Transform[] rows;
    [SerializeField] private Slot _slot;
    
    private List<DateTime> dates = new();
    private int currMonth = 1;
    private int currYear = 2026;
    [SerializeField] private List<Slot> slots = new();
    public event Action <string> OnDateChange ;


    private void Awake()
    {
        GenerateSlots();
        OnDateChange?.Invoke(new DateTime(currYear, currMonth, 1).ToString("MMMM yyyy", new System.Globalization.CultureInfo("ru-RU")));
    }

    private void GenerateSlots()
    {
        dates.Clear();
        DateTime first = new DateTime(currYear, currMonth, 1);
        int dow = (int)first.DayOfWeek;
        int daysBack = dow == 0 ? 6 : dow - 1;
        DateTime day = first.AddDays(-daysBack);
        DateTime last = new DateTime(currYear, currMonth, 1).AddMonths(1).AddDays(-1);
        int dow2 = (int)last.DayOfWeek;
        int daysLeft = 0;
        if (dow2 == 0)
        {
            daysLeft = 0;
        }
        else
        {
            daysLeft = 7 - dow2;
        }

        var daysInMonth = DateTime.DaysInMonth(currYear, currMonth) + daysBack;
        for (int i = 0; i < daysInMonth; i++)
        {
            dates.Add(day);
            day = day.AddDays(1);
        }
        int counter = 0;
        int weekIndex = 0; 
        foreach (DateTime date in dates)
        {
            
            if (counter >6)
            {
                counter = 0;
                weekIndex++;
            }
            counter++;
            var slot = Instantiate(_slot, rows[weekIndex]);
            slot.SetData(date);
            slots.Add(slot);
        }
        for (int i = 0; i < daysLeft; i++)
        {
            GameObject obj = new GameObject("Filler", typeof(RectTransform));
            obj.transform.SetParent(rows[weekIndex]);
            obj.tag = "Filler";
        }
    }

    


    private void DeleteSlots()
    {
        for (int i = slots.Count-1;i >=0;i--)
        {
            var slot  = slots[i];
            slots.Remove(slot);
            DestroyImmediate(slot.gameObject);
        }
        var filters = GameObject.FindGameObjectsWithTag("Filler");
        for (int i = filters.Length - 1; i>=0; i--)
        {
            DestroyImmediate(filters[i]);
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
