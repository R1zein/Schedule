using UnityEngine.UIElements;
using Zenject;

namespace UI.Elements
{
    /// <summary>
    /// Создаёт View динамических элементов. Обычный сервис вместо PlaceholderFactory:
    /// по имени метода видно, какой шаблон во что превращается, а переход по коду ведёт
    /// в конструктор View, а не в сгенерированную фабрику.
    /// Создаём через контейнер, а не через new, чтобы View получал свои зависимости.
    /// </summary>
    public class ViewFactory
    {
        [Inject] private DiContainer _container;
        [Inject] private UITemplateLibrary _templates;

        public DaySlotView CreateDaySlot() => Create<DaySlotView>(_templates.DaySlot);

        public LessonRowView CreateLessonRow() => Create<LessonRowView>(_templates.LessonRow);

        private T Create<T>(VisualTreeAsset template)
        {
            return _container.Instantiate<T>(new object[] { template.Instantiate() });
        }
    }
}
