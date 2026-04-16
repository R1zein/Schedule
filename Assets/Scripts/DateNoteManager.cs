using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DateNoteManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button addNoteButton;
    [SerializeField] private SaveData saveData;

    private DateTime _date;
    private DateData _data;
    private void Awake()
    {
        addNoteButton.onClick.AddListener(ClosePanel);
    }

    public void EditNoteByDate(DateTime date)
    {
        _date = date;
        inputField.text = "";
        if (saveData.TryGetDateData(date, out DateData data))
        {
            _data = data;
            inputField.text = data.note;
        }
        else
        {
            _data = new DateData();
        }
        gameObject.SetActive(true);
    }

    private void ClosePanel()
    {
        _data.date = _date;
        _data.note = inputField.text;
        saveData.AddData(_data);
        gameObject.SetActive(false);
    }

}
