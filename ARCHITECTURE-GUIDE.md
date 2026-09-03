# Архитектура Unity-проекта: Zenject + UI Toolkit

Инструкция для агента, который переносит эту архитектуру на другой проект.
Правила обязательны: если код им не соответствует — переписывается код, а не правило.
Проверено на Unity 6000.3, Zenject (Extenject), UI Toolkit (UIDocument, runtime).

---

## 1. Слои и сборки

Три asmdef, зависимости только вниз:

```
Schedule.Installers  ->  Schedule.UI  ->  Schedule.Core  ->  Zenject
        (Core, UI, Zenject)   (Core, Zenject)     (Zenject)
```

| Сборка | rootNamespace | Что внутри | Что запрещено |
|---|---|---|---|
| `<Project>.Core` | `Core` | модель, сервисы, данные, сохранение | `using UnityEngine.UIElements`, любые ссылки на UI, любое знание о панелях |
| `<Project>.UI` | `UI` | контроллеры панелей, View динамических элементов, навигация, библиотека шаблонов | ссылки на Installers; создание сервисов вручную (`new`) |
| `<Project>.Installers` | `Installers` | только инсталлеры | любая логика, кроме биндингов |

Правила:
- `Core` не знает ни о UI, ни о том, что его вообще кто-то отображает. Наружу — только события и публичные методы.
- `UI` знает `Core`, но не наоборот. Никогда не добавляй ссылку `Core -> UI`, чтобы «дёрнуть панель».
- `Installers` — единственная сборка, которая знает обо всех остальных.
- Zenject подключается ссылкой во все три asmdef.

Структура папок повторяет сборки:

```
Assets/Scripts/
  Core/           <Project>.Core.asmdef
    Calendar/  Data/  Lessons/  Notes/  Saving/   (папка = смысловая область)
  UI/             <Project>.UI.asmdef
    Panels/       контроллеры статических панелей
    Elements/     View динамических элементов
  Installers/     <Project>.Installers.asmdef
Assets/UI/
  main.uxml       корневой контейнер панелей
  Panels/         по одному uxml на панель
  Dynamic/        шаблоны, которые инстанцируются в рантайме
  Styles/         uss-листы
```

---

## 2. MonoBehaviour запрещены

**Единственный MonoBehaviour в проекте — инсталлер.** Всё остальное — обычные C#-классы, которые
создаёт и держит контейнер.

- Нет `MonoBehaviour`-контроллеров, нет `Start()`, `Update()`, `FindObjectOfType`, синглтонов,
  статических «менеджеров», `Resources.Load`.
- Жизненный цикл берётся из Zenject: `IInitializable` вместо `Start()`, `IDisposable` вместо
  `OnDestroy()`, `ITickable` — только если реально нужен покадровый апдейт.
- Всё, что должно приходить из сцены или из ассетов (UIDocument, ScriptableObject, префабы),
  попадает в контейнер через `[SerializeField]` инсталлера.

---

## 3. Композиционный корень

Один `MonoInstaller` на сцену, повешенный на `SceneContext`. Это единственное место, где
известно, из чего собрано приложение.

```csharp
public class MainSceneInstaller : MonoInstaller
{
    [SerializeField] private UIDocument _document;
    [SerializeField] private UITemplateLibrary _templates;
    [SerializeField] private string _cultureName = "ru-RU";

    public override void InstallBindings()
    {
        // 1. внешние ресурсы — инстансами
        Container.BindInstance(_document);
        Container.BindInstance(_templates);
        Container.BindInstance(new LocalizationConfig(_cultureName));

        // 2. сервисы Core
        Container.BindInterfacesAndSelfTo<SaveLoadService>().AsSingle();
        Container.BindInterfacesAndSelfTo<CalendarService>().AsSingle();
        Container.BindInterfacesAndSelfTo<ScheduleService>().AsSingle();
        Container.Bind<NavigationService>().AsSingle();      // без IInitializable — просто Bind

        // 3. фабрика динамических View — обычный сервис, а не BindFactory
        Container.Bind<ViewFactory>().AsSingle();

        // 4. контроллеры панелей
        Container.BindInterfacesAndSelfTo<CalendarPanelController>().AsSingle();
        Container.BindInterfacesAndSelfTo<NoteEditorPanelController>().AsSingle();
    }
}
```

Правила:
- `BindInterfacesAndSelfTo<T>().AsSingle()` для всего, что реализует `IInitializable`/`IDisposable`
  и при этом инжектится по конкретному типу. `Bind<T>().AsSingle()` — если интерфейсов нет.
- Порядок в методе: ресурсы → сервисы → фабрики → контроллеры. Порядок вызова `Initialize()`
  Zenject определяет сам, поэтому **контроллер не имеет права рассчитывать, что чужой
  `Initialize()` уже отработал**. Всё, что нужно другому объекту на старте, он берёт свойством,
  а не «полученным на инициализации» состоянием.
- Никаких зашитых констант в коде: культура, пути, размеры сетки — параметры инсталлера или
  конфиг-объект (`LocalizationConfig`), созданный в нём.
- Захламлённых `ProjectContext` нет; если появится глобальное состояние между сценами — отдельный
  `ProjectInstaller`, но сцена по-прежнему собирается своим `MonoInstaller`.

---

## 4. Инъекция

В проекте используется **field injection**:

```csharp
[Inject] private UIDocument _document;
[Inject] private CalendarService _calendar;
```

- Поля `private`, с подчёркиванием, каждое со своим `[Inject]`.
- `readonly`-поля — только для собственных коллекций (`private readonly List<DaySlotView> _slots = new();`).
- Конструктор используется только у View, которые создаёт фабрика (`DaySlotView(TemplateContainer root)`).
- Не смешивай стили в одном классе. Не пиши `[Inject] public` и method-injection.

---

## 5. Сервисы (Core)

Шаблон сервиса:

```csharp
public class CalendarService : IInitializable
{
    [Inject] private LocalizationConfig _localization;

    public event Action Changed;

    public void Initialize() { /* стартовое состояние */ }

    public IReadOnlyList<DayCell> Days => _days;   // наружу — только для чтения

    public void NextMonth() => SetMonth(_month.AddMonths(1));

    private void SetMonth(DateTime month)
    {
        _month = month;
        Rebuild();
        Changed?.Invoke();            // событие — единственный способ сообщить UI
    }
}
```

Правила:
- Сервис не знает, кто на него подписан, и не хранит ссылок на UI.
- Наружу отдаются `IReadOnlyList<T>` / value-типы. Изменяемые списки — только там, где
  редактор действительно правит модель на месте (`GetTemplate(day)` возвращает `List<Lesson>`),
  и это отмечено комментарием.
- Событие бросается тогда, когда меняется **состав** данных, а не на каждое поле. Правка текста
  урока не должна дёргать пересборку календаря — она уходит в модель напрямую.
- Данные-снимки для UI — маленькие `readonly struct` (`DayCell`), без методов отрисовки.
- Сервис владеет своим состоянием сам и сохраняет его через `ISaveLoadable` (§14). Общего
  «мешка данных», в который лезут все сервисы, нет.
- Помни ограничения сериализации Unity и `JsonUtility`: нет `TimeSpan` (храни `int` минут),
  нет `Dictionary` (храни `List` + поиск), нет `DateTime` (храни `long Ticks` в приватном поле
  с публичным свойством-обёрткой).

---

## 6. UI Toolkit: структура разметки

**`main.uxml` — только контейнер.** Он говорит, какие панели есть и где они лежат; внутреннее
устройство панели — в её собственном файле.

```xml
<ui:UXML ...>
    <Style src="project://database/Assets/UI/Styles/Main.uss" />

    <ui:Template name="HeaderPanel"   src=".../Panels/HeaderPanel.uxml" />
    <ui:Template name="CalendarPanel" src=".../Panels/CalendarPanel.uxml" />

    <ui:VisualElement name="app-root" class="bg-app" style="flex-grow: 1; padding: 16px;">
        <ui:Instance template="HeaderPanel"   name="header-panel"   style="flex-shrink: 0;" />
        <ui:Instance template="CalendarPanel" name="calendar-panel" style="flex-grow: 1;" />
        <!-- оверлеи — последними, position: absolute на весь экран -->
        <ui:Instance template="NoteEditorPanel" name="note-editor-panel"
                     style="position: absolute; left: 0; top: 0; right: 0; bottom: 0;" />
    </ui:VisualElement>
</ui:UXML>
```

Правила:
- Новая панель = новый файл в `Panels/` + `Template` + `Instance` в `main.uxml`. Больше нигде.
- Имя инстанса — `<что-то>-panel`, по нему контроллер находит свой корень.
- Динамические шаблоны (`Dynamic/`) в `main.uxml` **не упоминаются** — они создаются кодом.
- `name` в разметке — kebab-case, уникальные в пределах панели. Именно по `name` идёт `Q<>()`.
- Геометрию одиночного элемента можно задать инлайном (`style="flex-grow: 1;"`). Всё, что
  повторяется или переключается состоянием, — классом в uss.

---

## 7. Динамические шаблоны и `UITemplateLibrary`

Всё, что создаётся и уничтожается в рантайме, лежит в `Assets/UI/Dynamic/` и попадает в
ScriptableObject-библиотеку:

```csharp
[CreateAssetMenu(fileName = "UITemplateLibrary", menuName = "Scriptable Objects/UI/Template Library")]
public class UITemplateLibrary : ScriptableObject
{
    [SerializeField] private VisualTreeAsset _daySlot;
    public VisualTreeAsset DaySlot => _daySlot;
}
```

- Библиотека биндится инстансом в инсталлере, контроллеры получают её через `[Inject]`.
- Статические панели в библиотеку **не попадают** — они уже в `main.uxml`.
- `Resources.Load` и прямые ссылки на `VisualTreeAsset` в контроллерах запрещены.

---

## 8. Контроллер панели

Обычный класс, `IInitializable + IDisposable`. Скелет обязателен к соблюдению:

```csharp
public class HeaderPanelController : IInitializable, IDisposable
{
    [Inject] private UIDocument _document;
    [Inject] private CalendarService _calendar;
    [Inject] private NavigationService _navigation;

    private VisualElement _root;
    private Label _title;
    private Button _previous;

    public void Initialize()
    {
        _root = _document.rootVisualElement.Q<VisualElement>("header-panel");   // свой корень
        _title = _root.Q<Label>("header-title");                                // всё дальше — от него
        _previous = _root.Q<Button>("header-previous");

        _previous.clicked += _calendar.PreviousMonth;
        _calendar.Changed += UpdateTitle;
        _navigation.Changed += UpdateVisibility;

        UpdateTitle();          // первичная отрисовка руками — событий ещё не было
        UpdateVisibility();
    }

    public void Dispose()
    {
        _previous.clicked -= _calendar.PreviousMonth;    // ровно те же строки с минусом
        _calendar.Changed -= UpdateTitle;
        _navigation.Changed -= UpdateVisibility;
    }
}
```

Жёсткие правила:
- Контроллер ищет **только свой корень** по имени панели в `rootVisualElement`; все остальные
  `Q<>()` — от `_root`. Лазить в чужую панель нельзя.
- Ссылки на элементы кешируются полями в `Initialize()`, а не ищутся заново в обработчиках.
- Каждой подписке в `Initialize()` соответствует отписка в `Dispose()`. Симметрия проверяется
  глазами: одинаковый порядок строк, `+=` / `-=`.
- Если контроллер строит элементы — в `Dispose()` вызывается `Clear()`, который диспозит View и
  снимает контейнеры через `RemoveFromHierarchy()`.
- Контроллер не содержит бизнес-логики: он читает модель и переводит клики в вызовы сервиса.
  Любое вычисление «что показать» — в `Core`.

---

## 9. View динамического элемента

Обёртка над `TemplateContainer`. Создаётся фабрикой-сервисом (см. §9.1):

```csharp
public class DaySlotView : IDisposable
{
    private const string SelectedClass = "slot--selected";   // классы — константами

    private readonly VisualElement _root;
    private readonly Button _surface;

    public event Action<DaySlotView> Selected;    // наружу отдаём себя, не данные

    public DaySlotView(TemplateContainer root)
    {
        _root = root;
        _root.AddToClassList("slot-cell");        // класс на сам TemplateContainer вешает код
        _surface = _root.Q<Button>("slot-surface");
        _surface.clicked += OnSurfaceClicked;
    }

    public VisualElement Root => _root;

    public void Bind(DayCell cell, bool hasNote) { /* данные -> элементы и классы */ }

    public void SetSelected(bool selected) => _slot.EnableInClassList(SelectedClass, selected);

    public void Dispose()
    {
        _surface.clicked -= OnSurfaceClicked;
        Selected = null;                 // обнуляем события
        _root.RemoveFromHierarchy();     // и снимаем себя с дерева
    }

    private void OnSurfaceClicked() => Selected?.Invoke(this);
}
```

Правила:
- Конструктор принимает готовый `TemplateContainer` — сам View шаблоны не ищет и не инстанцирует.
- Вложенных `Factory` внутри View нет. Никакого `PlaceholderFactory`.
- View **ничего не решает**: он публикует события (`Action<TView>`), решение принимает контроллер.
- View не инжектит сервисы. Если ему нужны данные — их передают в `Bind()`.
- `Dispose()` обязателен: отписки → `event = null` → `RemoveFromHierarchy()`.
- `TemplateContainer` — лишний узел в иерархии с `flex-direction: column`. Если элемент должен
  тянуться или стоять в строке, вешай класс на сам контейнер (`.slot-cell { flex-grow: 1; flex-basis: 0; }`),
  иначе раскладка рассыплется. Класс вешает код, а не uxml.

---

## 9.1. Фабрика View — обычный сервис, а не PlaceholderFactory

`PlaceholderFactory` и `BindFactory` **не используются**: фабрика существует только как
сгенерированный тип, по коду не читается (переход по `Create` ведёт в сам Zenject), а связка
«какой шаблон → какой View» размазана между инсталлером и вызовом в контроллере.

Вместо этого — один обычный сервис, который инжектит `DiContainer` и библиотеку шаблонов:

```csharp
/// <summary>
/// Создаёт View динамических элементов. По имени метода видно, какой шаблон во что
/// превращается, а переход по коду ведёт в конструктор View.
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
```

Правила:
- Биндится как обычный сервис: `Container.Bind<ViewFactory>().AsSingle();`.
- Один метод `CreateX()` на каждый тип View. Имя метода — единственное, что видит контроллер:
  `DaySlotView slot = _views.CreateDaySlot();`.
- `_container.Instantiate<T>(args)`, а не `new`. Это и есть смысл фабрики: конструктор получает
  `TemplateContainer` параметром, а `[Inject]`-поля View подставит контейнер — добавить зависимость
  во View можно, не трогая контроллеры.
- Инстанцирование шаблона (`template.Instantiate()`) живёт внутри фабрики. Контроллер о
  `UITemplateLibrary` для этих элементов не знает и `TemplateContainer` руками не создаёт.
- Фабрика не хранит созданные View и не владеет их жизнью — за `Dispose()` отвечает тот
  контроллер, который их создал.
- Если типов View станет много, режь на несколько сервисов по смысловым областям
  (`CalendarViewFactory`, `ScheduleViewFactory`), а не превращай один в свалку.

---

## 10. Связи между панелями

- **Модель → UI**: только события сервиса. Панель подписывается сама.
- **UI → модель**: прямой вызов метода сервиса.
- **Панель → оверлей**: панель-владелец инжектит контроллер оверлея и вызывает `Open(...)`.
  Обратно оверлей ничего не зовёт — он пишет в сервис, а владелец узнаёт из события сервиса.
- **Между вкладками**: через `NavigationService`. Панель вкладок пишет `Show(Tab)`, каждая панель
  подписана на `Changed` и **прячет себя сама**:

```csharp
private void UpdateVisibility()
    => _root.EnableInClassList("panel--hidden", _navigation.Current != Tab.Calendar);
```

  Так переключатель вкладок не знает, какие панели существуют, и добавление новой вкладки не
  трогает существующий код.
- Прямых ссылок «панель дёргает элемент чужой панели» не бывает никогда.
- Enum вкладок называется `Tab`, а не `Screen` — `Screen` занят `UnityEngine`.

---

## 11. Стили (USS)

Разделение листов:

| Лист | Что внутри | Куда подключён |
|---|---|---|
| `Main.uss` | токены `:root`, шрифт, `.panel--hidden`, цветовые классы из токенов, общие кнопки | во все uxml |
| `<Область>.uss` (`Slot.uss`, `Schedule.uss`) | элемент и его состояния | в uxml панели **и** в uxml её динамических шаблонов |

Правила, нарушение которых ломает рантайм:

1. **`var()` не работает в инлайн-стиле UXML.** Резолвера переменных там нет — будет
   `NullReferenceException`, и рухнет сборка всего дерева. Цвет из токена — только классом
   (`.bg-app { background-color: var(--bg); }`), инлайном — только геометрия.
2. **Инлайн-стиль нельзя перебить классом.** Всё, что переключается состоянием
   (`--selected`, `--today`, `--hidden`), задаётся классом в uss, иначе модификатор не дотянется
   до свойства.
3. Состояния переключаются из C# через `EnableInClassList(Class, bool)`, а имена классов лежат
   в `private const string` рядом.
4. Динамический шаблон подключает свои `<Style>` сам — чтобы открываться в редакторе отдельно.
   Тот же лист подключается и в панели-хосте, чтобы накрыть обёртки `TemplateContainer`,
   которые лежат выше корня шаблона.
5. Шрифт задаётся в `:root` как `-unity-font-definition: url(".../Font.ttf")`. Именно `.ttf`,
   не TMP-ассет: `TMP_FontAsset` наследуется от `TMP_Asset`, а свойство принимает `Font` или
   TextCore-шрифт. Из ttf UI Toolkit строит динамический атлас — отсюда резкость.

Именование: блок `.slot`, элемент `.slot__surface`, модификатор `.slot--today`, общий
модификатор состояния `.panel--hidden`.

---

## 12. Грабли рантайма

- **Не перестраивай дерево внутри обработчика события элемента, который сам будет уничтожен.**
  Откладывай на следующий кадр:
  ```csharp
  private void OnScheduleChanged() => _root.schedule.Execute(Rebuild);
  ```
- При биндинге значений в поля — `SetValueWithoutNotify()`, иначе получишь цикл
  «биндинг → ChangeEvent → запись в модель».
- Невалидный ввод не принимается молча: пишем в модель только при успешном разборе и возвращаем
  в поле прежнее значение через `SetValueWithoutNotify()`.
- `Button.clicked` ловит только левую кнопку. Правая — отдельно:
  `RegisterCallback<PointerDownEvent>(e => { if (e.button == 1) ... })`.
- Фокус после открытия оверлея — через `element.schedule.Execute(() => element.Focus())`.
- Клик по затемнению закрывает оверлей только если `evt.target == _scrim`, иначе закроется от
  клика по содержимому.
- Перебор списков на удаление — с конца: `for (int i = list.Count - 1; i >= 0; i--)`.

---

## 13. Комментарии и стиль кода

- У каждого класса — `/// <summary>`, в одну-две строки, **на русском**, объясняет роль класса в
  системе и границу ответственности («о UI не знает», «только просмотр, правится в редакторе»).
- Комментарии внутри метода объясняют **почему**, а не что. Пишутся там, где решение неочевидно:
  обход ограничения Unity, порядок кадров, причина выбора типа. Пересказ кода не пишется.
- Приватные поля — `_camelCase`, публичные свойства — `PascalCase`, сериализованные поля данных
  внутри `[Serializable]`-классов — `camelCase` без подчёркивания (их видит инспектор).
- Явные типы вместо `var`.
- Однострочные тела — через `=>`, если это действительно одно действие.
- Все скобки на месте, даже у однострочного `if` внутри метода.

---

## 14. Сохранение данных: ISaveLoadable

**ScriptableObject — не файл сохранения.** Запись в SO в рантайме меняет объект в памяти:
в редакторе она живёт до перезагрузки домена, в билде SO лежит в read-only данных и дописать
его нечем. `AssetDatabase` существует только в редакторе. Поэтому пользовательское состояние
всегда идёт в JSON в `Application.persistentDataPath`, а SO остаётся тем, чем является, —
данными дизайна (дефолты, конфиги, ссылки на ассеты).

Схема: сущность сама объявляет, что у неё есть сохраняемое состояние, и сама регистрируется.

```csharp
public interface ISaveLoadable
{
    /// Ключ раздела в файле. Строка, а не тип: переименование класса не должно ломать сейвы.
    string Key { get; }

    /// Снимок состояния — обычный [Serializable] класс. null — «мне нечего сохранять».
    object SaveState();

    /// Вызывается в момент регистрации. Своего раздела может не быть — тогда остаёмся на дефолтах.
    void LoadState(SaveFile file);
}
```

Сервис-сборщик:

```csharp
public class SaveLoadService : IInitializable, IDisposable
{
    private readonly List<ISaveLoadable> _savables = new();
    private SaveFile _file;
    private bool _loaded;

    public void Register(ISaveLoadable savable)
    {
        if (_savables.Contains(savable)) { /* warning, return */ }

        EnsureLoaded();                       // файл читается лениво, при первой регистрации
        _savables.Add(savable);

        if (_file != null) savable.LoadState(_file);   // состояние приходит сразу же
    }

    public void Save()
    {
        _file ??= new SaveFile();
        foreach (ISaveLoadable savable in _savables)
            if (savable.SaveState() is { } state)
                _file.SetState(state, savable.Key);
        _file.Write();
    }
}
```

Сохраняемая сущность:

```csharp
public class NoteService : IInitializable, IDisposable, ISaveLoadable
{
    [Inject] private SaveLoadService _saveLoad;

    private readonly List<DateData> _dates = new();   // состоянием владеет сама

    public string Key => "Notes";

    public void Initialize() => _saveLoad.Register(this);
    public void Dispose()    => _saveLoad.Unregister(this);

    public object SaveState() => new Data { dates = _dates };

    public void LoadState(SaveFile file)
    {
        Data data = file.GetState<Data>(Key);
        if (data?.dates == null) return;      // раздела нет — остаёмся на дефолтах
        _dates.Clear();
        _dates.AddRange(data.dates);
    }

    [Serializable]
    private class Data { public List<DateData> dates = new(); }
}
```

Правила:

- **Загрузка происходит в момент регистрации, а не по расписанию.** Это ключевое решение:
  Zenject не гарантирует порядок `Initialize()`, а так сущность получает данные ровно тогда,
  когда она к ним готова. Никаких `BindExecutionOrder` и «сначала загрузчик, потом все остальные».
- Файл читается лениво, при первой регистрации — по той же причине.
- **DTO — вложенный `private [Serializable] class Data`.** Наружу он не торчит: формат сохранения
  сущности не должен становиться частью её публичного API.
- `LoadState` обязан переживать отсутствие своего раздела (`null` → выходим, остаёмся на дефолтах).
  Так добавление новой сохраняемой сущности не ломает старые сейвы.
- Состоянием владеет сущность. Общего «мешка данных», в который лезут все, нет.
- Снапшот **накопительный**: `Save()` перезаписывает только ключи зарегистрированных сущностей,
  разделы отсутствующих остаются в файле нетронутыми.
- `Save()` дёргается на значимых изменениях (сущность вызывает его сама) плюс страховкой на
  `Application.quitting` и `Application.focusChanged(false)` — правки, которые не бросают событий,
  иначе не доедут до диска. Это же снимает нужду в MonoBehaviour: оба события статические.
- Не инжектить `List<ISaveLoadable>` в сервис: явная регистрация переживает и динамические
  сущности, и разные контейнеры (project/scene).
- Файл пишется **атомарно**: во временный, затем `File.Replace`. Обрыв записи не должен оставлять
  битый сейв, который положит следующий запуск.
- В корне файла — `Version`. Схема поменяется, а старые файлы надо будет мигрировать, а не терять.
- `JsonUtility` не умеет словари, `DateTime`, свойства и полиморфизм. Хранить: `long Ticks`,
  `List` вместо `Dictionary`, публичные поля. Нужно больше — Newtonsoft.

Чек-лист: сделать сущность сохраняемой

1. Добавить `ISaveLoadable` в список интерфейсов класса.
2. `Key` — короткая стабильная строка.
3. Вложенный `private [Serializable] class Data` с публичными полями.
4. `SaveState()` — собрать `Data`; `LoadState()` — прочитать с null-проверкой.
5. `Register(this)` в `Initialize()`, `Unregister(this)` в `Dispose()`.
6. `_saveLoad.Save()` там, где состояние меняется осмысленно.

Инсталлер при этом не трогается — сущность уже забинжена через `BindInterfacesAndSelfTo`.

---

## 15. Чек-лист: добавить новую панель

1. `Assets/UI/Panels/XPanel.uxml` — разметка, `<Style>` на `Main.uss` + свой лист, `name` в kebab-case.
2. В `main.uxml`: `<ui:Template name="XPanel" .../>` и `<ui:Instance template="XPanel" name="x-panel" .../>`.
3. `Assets/Scripts/UI/Panels/XPanelController.cs` — `IInitializable, IDisposable`, `[Inject]` поля,
   `Q<>` от `"x-panel"`, подписки/отписки, `UpdateVisibility()` если панель живёт на вкладке.
4. В инсталлер: `Container.BindInterfacesAndSelfTo<XPanelController>().AsSingle();`.
5. Логика — в сервис `Core`, панель только читает и вызывает.

## Чек-лист: добавить динамический элемент

1. `Assets/UI/Dynamic/X.uxml` + свои `<Style>`.
2. Поле `VisualTreeAsset` + свойство в `UITemplateLibrary`, ссылка проставлена в ассете.
3. `Assets/Scripts/UI/Elements/XView.cs` — конструктор от `TemplateContainer`, `Bind()`,
   события `Action<XView>`, `Dispose()`. Никакой вложенной `Factory`.
4. В `ViewFactory` — метод `CreateX()` (сам сервис уже забинжен, инсталлер не трогаем).
5. В контроллере: `XView view = _views.CreateX();`, подписка на события, снятие в `Clear()`/`Dispose()`.

---

## 16. Красные флаги при переносе

Если в целевом проекте встретится — переписывай:

- `MonoBehaviour`-контроллеры UI, `Start`/`Update` вместо `IInitializable`/`ITickable`;
- статические синглтоны, `Instance`, `FindObjectOfType`, `GameObject.Find`;
- `Resources.Load` и прямые ссылки на `VisualTreeAsset`/`StyleSheet` в коде логики;
- обращение к `rootVisualElement.Q<>` за пределами своей панели;
- подписка без отписки;
- `PlaceholderFactory`, `BindFactory`, вложенные `class Factory` — заменяются сервисом-фабрикой
  с `DiContainer` (§9.1);
- `new XView(...)` в обход контейнера — зависимости View не подставятся;
- бизнес-логика (расчёты дат, валидация, сортировка) в контроллере панели;
- `var(--token)` в инлайн-стиле uxml;
- переключение состояний через `style.display` / `style.backgroundColor` вместо классов;
- один гигантский uxml со всеми экранами и один uss на весь проект;
- UGUI-остатки (`Canvas`, `Image`, `TextMeshProUGUI`) в новых экранах — UI только на UI Toolkit.

> В этом проекте лежат `Assets/Scripts/Test.cs` и `Test1.cs` — старый мусор вне архитектуры,
> образцом не являются и на новый проект не переносятся.
