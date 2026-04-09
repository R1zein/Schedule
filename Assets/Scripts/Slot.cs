using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NUnit.Framework;
using System.Collections.Generic;
using System;

public class Slot : MonoBehaviour
{
    [SerializeField] private TMP_Text dateText;
    private TMP_Dropdown dropdown;
    [SerializeField] private GameObject addNotePanel;


    private void Awake()
    {
        dropdown = GetComponentInChildren <TMP_Dropdown>();
        List<string> options = new List<string>()
        {
            "Add Note",
            "Highlight"
        };
        dropdown.AddOptions(options);
        dropdown.onValueChanged.AddListener(OnValueChangeDropdown);

    }
    public void SetData(DateTime date)
    {
        dateText.text = date.ToString("d MMMM", new System.Globalization.CultureInfo("ru-RU"));
    }

    public void OnValueChangeDropdown(int index)
    {
        addNotePanel.SetActive(true);
    }
        
    
}
