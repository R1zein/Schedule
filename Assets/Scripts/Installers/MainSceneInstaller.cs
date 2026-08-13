using Schedule.Core;
using Schedule.Core.Calendar;
using Schedule.Core.Data;
using Schedule.Core.Notes;
using Schedule.UI;
using Schedule.UI.Elements;
using Schedule.UI.Panels;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace Schedule.Installers
{
    /// <summary>
    /// Композиционный корень сцены: единственное место, где известно,
    /// из чего собирается приложение.
    /// </summary>
    public class MainSceneInstaller : MonoInstaller
    {
        [SerializeField] private UIDocument _document;
        [SerializeField] private SaveData _saveData;
        [SerializeField] private UITemplateLibrary _templates;
        [SerializeField] private string _cultureName = "ru-RU";

        public override void InstallBindings()
        {
            Container.BindInstance(_document);
            Container.BindInstance(_saveData);
            Container.BindInstance(_templates);
            Container.BindInstance(new LocalizationConfig(_cultureName));

            Container.Bind<CalendarService>().AsSingle();
            Container.Bind<NoteService>().AsSingle();

            Container.BindFactory<TemplateContainer, DaySlotView, DaySlotView.Factory>();

            Container.Bind<HeaderPanelController>().AsSingle();
            Container.Bind<CalendarPanelController>().AsSingle();
            Container.Bind<NoteEditorPanelController>().AsSingle();

            Container.BindInterfacesAndSelfTo<UIRootController>().AsSingle().NonLazy();
        }
    }
}
