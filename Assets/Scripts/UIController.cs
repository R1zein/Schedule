using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] private TMP_Text dateText;
    [SerializeField] private SlotGenerator slotGenerator;

    private void OnEnable()
    {
        slotGenerator.OnDateChange += UpdateHeader;
    }

    private void OnDisable()
    {
        slotGenerator.OnDateChange -= UpdateHeader;
    }

    public void UpdateHeader(string text)
    {
        dateText.text = text;
    }

}
