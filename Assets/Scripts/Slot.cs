using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NUnit.Framework;
using System.Collections.Generic;
using System;

public class Slot : MonoBehaviour
{
    [SerializeField] private TMP_Text dateText;
    [SerializeField] private Button addNoteButton;

    private DateNoteManager _dateNoteManager;
    private DateTime _date;


    private void Awake()
    {
        _dateNoteManager = FindAnyObjectByType<DateNoteManager>(FindObjectsInactive.Include);
        addNoteButton.onClick.AddListener(AddNote);
    }
    public void SetData(DateTime date)
    {
        _date = date;
        dateText.text = date.ToString("d MMMM", new System.Globalization.CultureInfo("ru-RU"));
    }

    private void AddNote()
    {
        _dateNoteManager.EditNoteByDate(_date);
    }

    
        
    
}
