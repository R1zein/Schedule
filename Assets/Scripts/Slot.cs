using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class Slot : MonoBehaviour
{
    [SerializeField] private TMP_Text dateText;
    [SerializeField] private Button addNoteButton;
    [SerializeField] private float popUpTime;
    [SerializeField] private GameObject popUpPanel;
    [SerializeField] private Vector2 startPosition;
    [SerializeField] private Vector2 endPosition;
    [SerializeField] private Button activeButton;
    
    
    private DateNoteManager _dateNoteManager;
    private DateTime _date;


    private void Awake()
    {
        activeButton.onClick.AddListener(() => SetSlotActive(true));
        _dateNoteManager = FindAnyObjectByType<DateNoteManager>(FindObjectsInactive.Include);
        addNoteButton.onClick.AddListener(AddNote);
    }
    public void SetData(DateTime date)
    {
        _date = date;
        dateText.text = date.ToString("dd", new System.Globalization.CultureInfo("ru-RU"));
    }

    private void AddNote()
    {
        _dateNoteManager.EditNoteByDate(_date);
    }
    public async void SetSlotActive(bool active)
    {
        popUpPanel.SetActive(active);
        await PopUpAnimation();
    }

    private async Awaitable PopUpAnimation()
    {
        RectTransform rect = (RectTransform)popUpPanel.transform;
        float elapsed = 0f;

        while (elapsed < popUpTime)
        {
            await Awaitable.NextFrameAsync();
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / popUpTime);
            rect.anchoredPosition = Vector2.Lerp(startPosition, endPosition, t);
        }
        rect.anchoredPosition = endPosition;
        
    }
}
