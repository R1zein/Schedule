using UnityEngine;
using UnityEngine.UIElements;

namespace Schedule.UI
{
    /// <summary>
    /// Ссылки на uxml-шаблоны, которые создаются и удаляются в рантайме.
    /// Статические панели лежат в main.uxml и сюда не попадают.
    /// </summary>
    [CreateAssetMenu(fileName = "UITemplateLibrary", menuName = "Scriptable Objects/UI/Template Library")]
    public class UITemplateLibrary : ScriptableObject
    {
        [SerializeField] private VisualTreeAsset _weekRow;
        [SerializeField] private VisualTreeAsset _daySlot;

        public VisualTreeAsset WeekRow => _weekRow;

        public VisualTreeAsset DaySlot => _daySlot;
    }
}
