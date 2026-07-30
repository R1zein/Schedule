using System;
using Schedule.Core.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DateNoteManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button addNoteButton;
    [SerializeField] private SaveData saveData;

    private DateTime _date;
    private void Awake()
    {
        addNoteButton.onClick.AddListener(ClosePanel);
    }

    public void EditNoteByDate(DateTime date)
    {
        _date = date;
        inputField.text = saveData.TryGetDateData(date, out DateData data) ? data.note : "";
        gameObject.SetActive(true);
    }

    private void ClosePanel()
    {
        saveData.SetNote(_date, inputField.text);
        gameObject.SetActive(false);
    }

}
