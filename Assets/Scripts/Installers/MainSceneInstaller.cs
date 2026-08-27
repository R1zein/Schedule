using Core;
using Core.Calendar;
using Core.Data;
using Core.Lessons;
using Core.Notes;
using UI;
using UI.Elements;
using UI.Panels;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace Installers
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

            Container.BindInterfacesAndSelfTo<CalendarService>().AsSingle();
            Container.BindInterfacesAndSelfTo<NoteService>().AsSingle();
            Container.BindInterfacesAndSelfTo<ScheduleService>().AsSingle();
            Container.Bind<NavigationService>().AsSingle();

            Container.BindFactory<TemplateContainer, DaySlotView, DaySlotView.Factory>();
            Container.BindFactory<TemplateContainer, LessonRowView, LessonRowView.Factory>();

            Container.BindInterfacesAndSelfTo<TabsPanelController>().AsSingle();
            Container.BindInterfacesAndSelfTo<HeaderPanelController>().AsSingle();
            Container.BindInterfacesAndSelfTo<CalendarPanelController>().AsSingle();
            Container.BindInterfacesAndSelfTo<NoteEditorPanelController>().AsSingle();
            Container.BindInterfacesAndSelfTo<ScheduleEditorPanelController>().AsSingle();
            Container.BindInterfacesAndSelfTo<DayLessonsPanelController>().AsSingle();
        }
    }
}
