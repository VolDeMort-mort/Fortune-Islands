# Архітектурне ревʼю TestDiceKindoms

> Ревʼю виконано 21.09.2026 (Unity 6000.2.10f1, URP 17.2, Input System 1.14 + legacy Input).
> Статуси оновлено 26.09.2026 за станом гілки `feature/turn-system`.
> Система ходу (Етап 2) описана окремо: [TurnSystem.md](TurnSystem.md).

## Контекст

Гра за мотивами DiceKingdoms з трьома фазами ходу (**Roll → Build → War**) і власним варіантом фази Війни.
Ревʼю охоплює всі 61 скрипт (~3000 рядків) у `Assets/Scripts`, сцену `SampleScene`, префаб острова і налаштування проєкту.
Критерій: максимальна ефективність і максимально чистий код, оцінка з позиції Senior Unity розробника.

## Легенда статусів

- ✅ зроблено
- 🟡 зроблено частково
- ⬜ не зроблено

## Загальний висновок

Фундамент нормальний: неймспейси розставлені, конфіги винесені в ScriptableObject, генерація мапи розбита на сервіси з `IGenerationPattern`, UI ресурсів побудований на подіях.
Проблема в тому, що більшість «архітектури» була **назвами класів, а не працюючими механізмами**: патерн Command без черги, FSM без циклу, Manager без меж відповідальності.

Проєкт не мав робочого ядра: ні циклу фаз, ні єдиного інпуту, ні цілісної моделі сітки.
Етапи 0-3 варто завершити до написання нових фіч, бо кожна нова фіча зараз лягає на три паралельні обробники кліку.

## Зведення по етапах

| Етап | Зміст | Статус |
|---|---|---|
| 0 | Гігієна проєкту | 🟡 (лишився вибір Input System) |
| 0.5 | Розрив циклів залежностей (підготовка до asmdef) | ✅ |
| 2 | Фази: `TurnController` | ✅ (потрібно підключити HUD у сцені) |
| 3 | Єдиний інпут | ⬜ |
| 1 | Модель даних (сітка, зайнятість) | 🟡 |
| 4 | Юніти й бій | ⬜ |
| 5 | Рендер | ⬜ |
| 6 | Економіка | 🟡 |

Порядок виконання змінено на **0 → 0.5 → 2 → 3 → 1 → 4 → 5 → 6**.
Етап 2 ні від чого не залежить і дає грі робочий цикл. Етап 3 спирається на фази, бо контекст інпуту визначає фаза. Етап 1 потрібен лише перед Етапом 4.

---

## Блок 0. Фатальне (переписувати обовʼязково)

### 1. Інпут прибитий до кожного острова ⬜

`BuildingManager` і `UnitManager` лежать на префабі острова (`Assets/Prefabs/Island/IslandTemplate.prefab`), і кожен має власний `Update()` з глобальним `Input`.
`GameManager.SpawnIsland` створює **два** острови, тож один клік мишею обробляють два `BuildingManager` і два `UnitManager`.
Ворожий острів реагує на кліки гравця: рейкастить, виділяє свої юніти, рахує валідність забудови.
До цього додається `SelectionManager` у сцені: третій незалежний споживач ЛКМ у тому ж кадрі, без пріоритету чи механізму «спожито».
Поле `IslandController.isLocalPlayer` існує, але ніде не читається.

**Рішення:** один `InputRouter` у сцені, який визначає активний контекст (фаза + локальний гравець) і викликає методи менеджера потрібного острова.
Менеджери островів стають чистою логікою без `Update` і без `Input`.

### 2. Машина станів фаз не працює ✅

> Виправлено в Етапі 2: `TurnController` у чистій асемблі `FortuneIslands.Turns`, обробники фаз `Game/Phases`, HUD. Опис: [TurnSystem.md](TurnSystem.md).

- `GameState.Update()` ніколи не викликається: у `GameManager` немає `Update()`.
- `RollState` не запускається ніколи. Єдиний перехід у грі: кнопка назавжди привʼязана до `warState`.
- Немає поняття раунду, ходу, черги гравців, умови завершення фази.
- `[Header("State Machine")]` над приватним полем і публічні `buildState/rollState/warState`, які перезаписуються в `Awake`: декорація, що імітує архітектуру.

**Рішення:** `TurnController` з явним циклом `Roll → Build → War → Roll`, подією `OnPhaseChanged`, лічильником раундів і завершенням фази через контролер.
Стани отримують контекст (`ITurnContext`), а не `GameManager` цілком.

### 3. `CellData.OccupyingObject` зберігає то префаб, то інстанс ⬜

Під час генерації туди кладеться `WorldEntity` **з префаба** (`ClusterPattern`, `RandomPattern`), а потім `RenderService` мовчки підміняє його на живий інстанс.
Одне поле має два несумісні значення залежно від моменту.

- `MapResourceGenService.ClearResources` викликає `Object.Destroy(cell.OccupyingObject)`: знищується **компонент**, а не GameObject, тож при регенерації лишаються сироти. Якщо там префаб, Unity видасть помилку про знищення ассета.
- Модель сітки (чисті дані) тримає посилання на `MonoBehaviour`: її неможливо серіалізувати, протестувати чи синхронізувати по мережі.

**Рішення:** розділити дані й представлення. `CellData` містить тільки дані (`MapResourceType?`, id зайнятості, `CellType`), візуал будується окремим шаром.

### 4. Звільнення клітинок юнітами 🟡

> **Уточнення від 21.09.** Початкове формулювання («мертві юніти назавжди блокують клітинки») було неточним. `OccupyingUnit` мав тип `UnitController` (`UnityEngine.Object`), і перевірка `== null` для знищеного обʼєкта повертала `true` завдяки перевантаженому оператору Unity, тобто клітинка звільнялась випадково.
> Після переходу на `IGridOccupant` цього механізму немає, тому звільнення зроблено явним.

- ✅ `UnitController.OnDestroy` звільняє свою клітинку (з перевіркою `ReferenceEquals`, щоб не стерти чужого).
- ⬜ Володіння клітинкою досі розмазане між `UnitController`, `UnitManager` і `Pathfinding`. Потрібен єдиний `GridOccupancyService`.
- ⬜ `StopCoroutine` посеред кроку може лишити сітку в неузгодженому стані (див. пункт 12).

---

## Блок 1. Продуктивність

### 5. A* (`Pathfinding.cs`) ⬜

| Проблема | Вартість |
|---|---|
| `new PathNode()` для кожної клітинки на кожен запит | мапа 100×100 = 10 000 алокацій на один шлях |
| `OrderBy().ThenBy().First()` усередині головного циклу | O(n log n) + алокація ітераторів на кожній ітерації замість O(log n) |
| `openList.Contains()`: лінійний пошук по `List`, двічі на сусіда | O(n) замість O(1) |
| `gCost` за замовчуванням `0`, тому умова `newCost < neighbor.gCost` для нових вузлів завжди `false` | працює випадково, завдяки `!openList.Contains` |

Групове виділення з 20 юнітів дає ~200 000 алокацій синхронно в одному кадрі: гарантований фриз.

**Рішення:** пласкі масиви замість класів-вузлів, бінарна купа (або bucket-queue), bitset для closed-множини, перевикористання буферів.
Для групового наказу: **flow field** замість N незалежних A*. Дані grid-based, тож Burst + Jobs підійдуть добре.

### 6. Алокації та рейкасти щокадру в режимі будівництва ⬜

У `BuildingManager.Update` щокадру, поки тримається привид будівлі:

- `GetComponent<Structure>()` на префабі;
- `GetRotatedFootprint()` викликається тричі (валідатор, привид, хайлайти), і кожен виклик створює новий `List<FootprintTile>`;
- `ClearHighlights()` + новий `Physics.Raycast` на кожен тайл футпринту (`PlacementRenderer.UpdateHighlights`).

**Рішення:** кешувати `Structure`, рахувати футпринт один раз на зміну повороту, оновлювати хайлайти лише коли змінилась клітинка під курсором, брати `Surface` з мапи за координатами замість рейкасту.

### 7. `Surface.ToggleHighlight` ⬜

- `_renderer.materials` в `Awake` інстанціює **копії матеріалів для кожного тайла мапи**: 10 000 копій, кінець батчингу.
- У `ToggleHighlight` створюється `newMats`, цикл заповнення закоментований, і масив з `null` присвоюється рендереру: підсвітка робить тайл невидимим/рожевим.

**Рішення:** `MaterialPropertyBlock` + `sharedMaterial`. `.materials` не має зустрічатись у проєкті.

### 8. GameObject на кожну клітинку мапи ⬜

`RenderService` інстанціює окремий префаб на кожну клітинку, включно з водою, без пулу, чанків і батчингу. Два острови × N² клітинок = десятки тисяч GameObject.

**Рішення:** `Graphics.RenderMeshInstanced` або комбінування мешів по чанках. Вода: один квад із шейдером.

### 9. Решта гарячих точок 🟡

- ⬜ `OnGUI` для рамки виділення в `UnitManager`: IMGUI викликається кілька разів за кадр і алокує. Замінити на Canvas-елемент.
- ⬜ `Camera.main` у циклі по юнітах (`SelectUnitsInBox`) та інших місцях: кешувати.
- ⬜ `GetValidDestinations`: при кожному збільшенні `radius` перескановується весь квадрат замість кільця, плюс лінійний `results.Contains(p)`.
- ✅ `DiceManager.PerformRoll` більше не викликає `GetComponentsInChildren`: провайдери реєструються самі через `IDiceSource`.
- ✅ `Debug.Log` у гарячих шляхах замінено на `Log.Info` з `[Conditional]`. `Log.Warning` і `Log.Error` пишуть у консоль з правильним рівнем і лишаються в релізі.

---

## Блок 2. Дизайн коду

### 10. `Unit` vs `UnitController` ⬜

Дві голови на одному GameObject: `Unit : WorldEntity` (здоровʼя, смерть) і `UnitController : MonoBehaviour` (рух, команди).

- `UnitController` не реалізує `ISelectable`, а робить `GetComponent<WorldEntity>()` при кожному виділенні.
- `SpawnUnit` за потреби викликає `AddComponent<UnitController>()` у рантаймі: контракт префаба невизначений.
- `UnitStats.maxHealth` існує, але `Unit.currentHealth/maxHealth` це окремі серіалізовані поля, які ніколи не читаються зі stats. SO-конфіг частково ігнорується.

### 11. Command-патерн без сенсу ⬜

`IUnitCommand` це делегат, загорнутий у клас: немає черги, скасування, undo, серіалізації, групових наказів.
Гірше: `ExecuteCommand` зупиняє `_actionRoutine`, але `_moveRoutine` це окремий хендл, тому `StartAttacking` не зупиняє попередній рух. Юніт продовжує йти в старе місце, одночасно атакуючи.

**Рішення:** або нормальна черга наказів (політика переривання + одна стейт-машина юніта), або прибрати шар Command.

### 12. Корутини як ігрова логіка ⬜

Рух це `IEnumerator` з ручним `Vector3.Lerp`. Це недетерміновано, не переживає паузу, не серіалізується (сейви, мультиплеєр), а `StopCoroutine` лишає сітку в порваному стані.
Для покрокової гри з фазами потрібна tick-based симуляція з явним станом, а візуал окремо, як інтерполяція між тіками.

### 13. Ініціалізація на порядку рядків 🟡

- ✅ `BaseManager` видалено. Кожен менеджер отримує свої залежності явним параметром `Initialize(...)`. `IslandController` став єдиним складальником (composition root), порядок ініціалізації залежностний і видимий.
- ✅ Прихована залежність `mapManager.worldContainer = worldContainer` зникла.
- ⬜ Логіка досі в `MonoBehaviour`. Мета: звичайні C# класи з конструкторами (VContainer або ручний composition root), `MonoBehaviour` як тонкі адаптери. Це передумова для юніт-тестів.

### 14. Синглтон і підписки без відписок 🟡

- ✅ `UIController.Initialize` викликає `RemoveAllListeners()` перед додаванням слухачів (перезапуск гри більше не дублює їх).
- ✅ `GameManager.Instance` відхиляє другий екземпляр і обнуляється в `OnDestroy`.
- ✅ `ClearOldGame` зупиняє цикл ходу: поточна фаза встигає вимкнути свої системи до знищення островів.
- ✅ `GameManager` відписується від `TurnController` в `OnDestroy`; `TurnHud` відписується сам.
- ⬜ `MainMenuController` не відписується від кнопок.
- ⬜ Файл `MenuController.cs` містить клас `MainMenuController`.

### 15. Мультиплеєр не закладений ⬜

У манифесті `com.unity.multiplayer.center`, у сцені кнопка Multiplayer, в `IslandController` поле `isLocalPlayer`. Проте логіка синхронно мутує стан з `Input` в `Update`, генерація бере випадковий сід усередині, рух на корутинах.
Якщо мультиплеєр у планах, закладати треба зараз: детермінована симуляція, сід ззовні, команди як дані.
**Потрібне рішення власника проєкту.**

---

## Блок 3. Гігієна проєкту

### 16. Папки з назвою `Resources` ✅

Назва зарезервована Unity: вміст безумовно пакується в білд. Перейменовано; ресурсний код тепер у `Scripts/Economy/Stockpile`.

### 17. Два бекенди інпуту одночасно ⬜

`activeInputHandler: 2` (Both). `InputSystem_Actions.inputactions` є, але весь код на legacy `Input` (`BuildingManager`, `UnitManager`, `SelectionManager`, `CameraController`).
**Рекомендація:** перейти на новий Input System разом з `InputRouter` (Етап 3). До того лишати `Both`.
Обовʼязково замінити `StandaloneInputModule` на `InputSystemUIInputModule` на `EventSystem`, інакше UI перестане реагувати.

### 18. Немає `.asmdef` 🟡

Весь проєкт був в `Assembly-CSharp`: повна рекомпіляція на кожну правку, немає меж між модулями, тестів нуль (пакет `com.unity.test-framework` підключений).

- ✅ Створено `.asmdef` на кожен модуль (див. додаток А). Компіляцію підтверджено в Unity 26.09.
- ✅ Тестова асемблі `FortuneIslands.Turns.Tests` (EditMode), 32 тести на цикл ходу.
- ⬜ Тести для `WorldMap`, `Pathfinding`, `TileService`, `ResourceManager`.

### 19. Сміття і структура папок 🟡

- ✅ `Assets/_Recovery/` прибрано і додано в `.gitignore`.
- ✅ `Assets/Map.unity` видалено.
- ✅ `IslandTemplate.prefab` переїхав в `Assets/Prefabs/Island/`.
- ✅ `Assets/TextMesh Pro/Examples & Extras/` видалено (285 файлів, ззовні на них ніхто не посилався).
- ✅ `Сottage.prefab` з кириличною «С» перейменовано на `Cottage.prefab` (разом з `.meta`, GUID не змінився).
- ✅ Додано `.gitattributes`: сцени, префаби, ассети і `.meta` завжди з LF.
- ⬜ Папка `RecourcesPrefabs` (помилка в слові).
- ⬜ `StructurePrefabs` містить лише `Snow`, `UnitPrefabs` лише `Winter`: ієрархія «тип → біом» переплутана.

### 20. Мертвий і напівмертвий код ⬜

- `ResourceGenEffect`: вся логіка закоментована.
- `TowerAttackEffect`: `FindClosestEnemy()` завжди `return null`, але `Update()` крутиться на кожній вежі щокадру.
- `NeighBoostEffect`: «Just holds data», `targetBuildingID` це `string` при наявному `enum BuildingTypes`.
- `WorldEntities/Resource.cs`: порожній клас.
- `ResourceItemUI`: `int max = 15;` хардкод.
- `WorldMap.debugGrid()`: дебаг-метод у продакшн-класі.

### 21. Дубльовані й розʼїхані правила сітки ⬜

- `WorldMap.isPlacable` ігнорує тип клітинки (перевірка `Type` закоментована), а `PlacementValidator` окремо звіряє `requiredSurface`: одне правило, дві половини.
- Умова прохідності існує в трьох копіях: `CellData.IsWalkable`, `WorldMap.IsWalkable`, `Pathfinding`.
- Іменування: `isPlacable` (lowerCase + помилка в слові, має бути `IsPlaceable`), `mapSize`, `debugGrid()`.

### 22. Трава робить клітинку непрохідною ⬜

`RandomPattern` кладе префаб трави в `OccupyingObject`, а і `IsWalkable`, і `isPlacable` перевіряють саме це поле. Декоративна трава блокує і забудову, і рух юнітів.
Декор і перешкоди мають бути різними полями.

### 23. Хардкод геймплею 🟡

- 🟡 Спавн воїна після кожної будівлі переїхав з `BuildingManager.CommitBuild` в `IslandController.HandleStructureBuilt` (подія `OnStructureBuilt`). Правило залишилось хардкодом, але живе в `Game`, а не в шляху будівництва.
- ⬜ Магічні числа висот: `new Vector3(x, 1f, z)` у білді, `0.15f` у `RenderService`.

---

## План переробки

### Етап 0. Гігієна проєкту 🟡

- [x] Клас `Log` з `[Conditional]`, заміна викликів `Debug.Log`
- [x] Перейменувати папки `Resources`
- [x] Прибрати `_Recovery` і `Map.unity`
- [x] Перенести `IslandTemplate.prefab` в `Assets/Prefabs/Island/`
- [x] Видалити `Assets/TextMesh Pro/Examples & Extras/`
- [x] Створити `.asmdef` на модуль (очікує перевірки компіляції в Unity)
- [ ] Визначитись з Input System (рекомендація: новий, разом з Етапом 3)
- [x] Додати `.gitattributes` (`*.prefab`, `*.unity`, `*.asset`, `*.meta` з `text eol=lf`)
- [x] Перейменувати `Сottage.prefab` (латинська «C»)
- [x] Виправити рівні `Log.Warning` / `Log.Error`

### Етап 0.5. Розрив циклів залежностей ✅

Виконано перед створенням asmdef, бо `.asmdef` забороняє цикли.

- [x] **A.** Видалити `BaseManager`, залежності передавати явно в `Initialize`
- [x] **B.** `BuildingContext` замість `IslandController` в `IBuildingFeature`
- [x] **C.** `IGridOccupant` в `Core` замість `UnitController` в `CellData`
- [x] **D.** `ResourceCost` перенесено в `Economy/Stockpile`
- [x] **E.** `IDiceSource` в `Economy`, `DiceProvider` реєструється сам
- [x] **F.** Подія `OnStructureBuilt` замість прямого спавну юніта в `BuildingManager`
- [x] **G.** `UIController.Initialize(ResourceManager, BuildingManager)`
- [x] **H.** Зайві `using`

Корекція: спершу було запропоновано перенести `DiceProvider` в `Economy`. Це створило б цикл через `IBuildingFeature`, тому правильне рішення: `DiceProvider` лишається в `Building` і реалізує інтерфейс `IDiceSource` з `Economy`.

### Етап 1. Модель даних 🟡

- [x] Маркерний інтерфейс `IGridOccupant` в `Core`
- [ ] `CellData` без `GameObject`
- [ ] `GridOccupancyService` як єдиний власник зайнятості клітинок
- [ ] Розділити декор і перешкоди
- [ ] Сід генерації ззовні (детермінізм і тестованість)
- [ ] Звести три копії `IsWalkable` в одну

### Етап 2. Фази ✅

- [x] `TurnController` у чистій асемблі `FortuneIslands.Turns` (без Unity): раунди, фази, таймери, готовність гравців
- [x] Цикл `Build → Roll`, у кожному 3-му раунді `Build → WarPlanning → WarBattle → Roll`
- [x] Обробники фаз `Game/Phases` з вузьким `PhaseContext` замість `GameManager`
- [x] `TurnHud`: раунд, фаза, таймер, кнопка Ready
- [x] До 10 гравців (`GameManager.playerCount`), острови сіткою
- [x] Будівництво можна почати лише у фазі Build
- [x] 32 EditMode-тести
- [ ] Підключити `TurnHud` у сцені `SampleScene` (кроки в [TurnSystem.md](TurnSystem.md#налаштування-сцени))

Правила й архітектура: [TurnSystem.md](TurnSystem.md).

### Етап 3. Інпут ⬜

- [ ] Єдина точка входу (`InputRouter`), роутинг до локального гравця, контексти на фазу
- [ ] Прибрати `Update`/`Input`/`OnGUI` з усіх менеджерів островів
- [ ] Викинути або підпорядкувати `SelectionManager`
- [ ] Перейти на новий Input System (`InputSystemUIInputModule` на `EventSystem`)

### Етап 4. Юніти й бій (ключова відмінність від DiceKingdoms) ⬜

- [ ] Один компонент замість `Unit` + `UnitController`, `UnitStats` як джерело істини
- [ ] Tick-based рух замість корутин
- [ ] Переписаний A* (купа, пул, bitset), flow field для груп
- [ ] Планування атак у `WarPlanning`: позначення цілей на чужих островах, доступне лише після будівництва порту
- [ ] Бій у `WarBattlePhase`: човни пливуть до цілей, фаза завершується через `CompletePhase()`, коли бій розіграно

### Етап 5. Рендер ⬜

- [ ] `MaterialPropertyBlock` замість `.materials`
- [ ] Інстансинг або чанки замість GameObject на клітинку
- [ ] Кешування футпринтів і хайлайтів у режимі будівництва

### Етап 6. Економіка 🟡

- [x] Реєстр `IDiceSource` замість `GetComponentsInChildren`
- [ ] Доробити або видалити `ResourceGenEffect`, `NeighBoostEffect`, `TowerAttackEffect`
- [ ] Прибрати хардкод `max = 15` і спавн воїна як правило

---

## Додаток А. Модулі та asmdef

Граф залежностей (порахований за реально використовуваними `using`), циклів немає:

```
Core           -> -
Turns          -> -              (без Unity: noEngineReferences)
WorldEntities  -> Core
Economy        -> Core
Map            -> Core, WorldEntities
Units          -> Core, Map
Building       -> Core, Economy, Map, WorldEntities
UI             -> Core, Economy, Building, Turns
Game           -> Building, CameraControl, Core, Economy, Map, Turns, UI, Units
Selection      -> Core, Units
CameraControl  -> -
```

Правило: **ніхто, крім `Game`, не імпортує `Game`**.

Асембли (створено; файли `FortuneIslands.<Модуль>.asmdef` лежать у теці кожного модуля):

| Assembly | Посилання на асембли проєкту | Зовнішні |
|---|---|---|
| `FortuneIslands.Core` | нічого | нічого |
| `FortuneIslands.Turns` | нічого | нічого, навіть UnityEngine (`noEngineReferences`) |
| `FortuneIslands.WorldEntities` | Core | нічого |
| `FortuneIslands.Economy` | Core | нічого |
| `FortuneIslands.Map` | Core, WorldEntities | `Unity.Mathematics` |
| `FortuneIslands.Units` | Core, Map | нічого |
| `FortuneIslands.Building` | Core, Economy, Map, WorldEntities | `UnityEngine.UI` (там живе `EventSystem`) |
| `FortuneIslands.UI` | Core, Economy, Building, Turns | `Unity.TextMeshPro`, `UnityEngine.UI` |
| `FortuneIslands.CameraControl` | нічого | нічого |
| `FortuneIslands.Game` | усі вищі | `UnityEngine.UI` |
| `FortuneIslands.Turns.Tests` | Turns | `UnityEngine.TestRunner`, `UnityEditor.TestRunner`, `nunit.framework.dll` (лише Editor) |

`Selection` можна лишити без asmdef: скрипти поза asmdef потрапляють в `Assembly-CSharp`, який бачить усі asmdef, а його не бачить ніхто. За планом (Етап 3) цей клас все одно видаляється.
Після переходу на новий інпут додати `Unity.InputSystem` там, де читається інпут.

Тести лежать в `Assets/Tests/EditMode` (Window → General → Test Runner → EditMode). Наступні кандидати: `WorldMap`, `Pathfinding`, `TileService`, `ResourceManager`. Для них потрібна тестова асемблі з посиланнями на `Core`, `Map`, `Economy`.

## Що перевірити при наступному відкритті Unity

Етап 2 скомпільовано поза редактором (`dotnet build` з тими ж межами асембл і налаштуваннями компілятора, що в Unity), а 32 тести пройшли під NUnit. У самому Unity це ще треба підтвердити.

1. Console без помилок після імпорту.
2. Test Runner → EditMode → Run All: 32 тести зелені.
3. Підключити `TurnHud` у сцені ([кроки](TurnSystem.md#налаштування-сцени)).
4. Димова перевірка: Build → Ready → Roll (ресурси додались, через 5 с новий раунд) → на 3-му раунді WarPlanning з таймером 60 с → Roll. Кнопки будівель не працюють поза Build.
