using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

public class Slot : MonoBehaviour
{
    [SerializeField] private TMP_Text dateText;
    [SerializeField] private TMP_Dropdown dropdown;

    public void SetData(DateTime date)
    {
        dateText.text = date.ToString("d MMMM", new System.Globalization.CultureInfo("ru-RU"));
    }

       
    
}
