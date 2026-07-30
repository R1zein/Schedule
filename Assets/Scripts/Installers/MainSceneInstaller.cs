using System;
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
    /// из чего собирается приложение. Зависимости идут сверху вниз —
    /// UI знает о сервисах, сервисы о UI не знают.
    /// </summary>
    public class MainSceneInstaller : MonoInstaller
    {
        [Header("Scene")]
        [SerializeField] private UIDocument _document;

        [Header("Assets")]
        [SerializeField] private SaveData _saveData;
        [SerializeField] private UITemplateLibrary _templates;

        [Header("Localization")]
        [SerializeField] private string _cultureName = "ru-RU";

        public override void InstallBindings()
        {
            ValidateSetup();

            InstallData();
            InstallServices();
            InstallUI();
        }

        private void ValidateSetup()
        {
            if (_document == null)
            {
                throw new InvalidOperationException($"{nameof(MainSceneInstaller)}: не назначено поле Document (объект с UIDocument).");
            }

            if (_saveData == null)
            {
                throw new InvalidOperationException($"{nameof(MainSceneInstaller)}: не назначено поле Save Data.");
            }

            if (_templates == null)
            {
                throw new InvalidOperationException($"{nameof(MainSceneInstaller)}: не назначено поле Templates (UITemplateLibrary).");
            }
        }

        private void InstallData()
        {
            Container.BindInstance(_document).AsSingle();
            Container.BindInstance(_saveData).AsSingle();
            Container.BindInstance(_templates).AsSingle();
            Container.BindInstance(new LocalizationConfig(_cultureName)).AsSingle();
        }

        private void InstallServices()
        {
            Container.Bind<CalendarService>().AsSingle();
            Container.Bind<NoteService>().AsSingle();
        }

        private void InstallUI()
        {
            // динамические ячейки создаются в рантайме через фабрику,
            // сам uxml-шаблон берётся из UITemplateLibrary
            Container.BindFactory<TemplateContainer, DaySlotView, DaySlotView.Factory>();

            // панели — обычные объекты: их Initialize вызывает корень UI,
            // поэтому порядок задаётся кодом, а не настройками контейнера
            Container.Bind<HeaderPanelController>().AsSingle();
            Container.Bind<CalendarPanelController>().AsSingle();
            Container.Bind<NoteEditorPanelController>().AsSingle();

            Container.BindInterfacesAndSelfTo<UIRootController>().AsSingle().NonLazy();
        }
    }
}
