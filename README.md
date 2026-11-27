# WheelSpin Demo

Vertigo Games case project, a wheel-of-fortune version of Critical Strike's Card Game.

- **Unity:** 6000.3.23f1
- **Dependencies:** DOTween (free), TextMeshPro
- **Run:** open `Assets/_Project/Scenes/WheelGame`, or install the APK from [Releases](../../releases)
- **Tests:** Window → General → Test Runner → EditMode → Run All

## Screenshots

| 20:9 | 16:9 | 4:3 |
|---|---|---|
| ![20:9](docs/screenshot_20x9.png) | ![16:9](docs/screenshot_16x9.png) | ![4:3](docs/screenshot_4x3.png) |

## Project overview

The code is split into layers, each in its own folder under `Assets/_Project/Scripts/Runtime`:

| Layer | Classes | Responsibility |
|---|---|---|
| **Data** | `RewardDefinition`, `WheelDefinition`, `ZoneTypeDefinition`, `ZoneSettings`, `EconomySettings`, `ZoneTheme` | ScriptableObjects that hold all content and tuning, edited in the Inspector. They only store data and validate it in the editor. They do not reference any other layer. |
| **Gameplay** | `GameSession`, `ZoneRules`, `EconomyRules`, `RewardInventory`, `IWallet`, `IRandomProvider`, models (`RewardModel`, `WheelModel`, `SliceModel`, `ZoneTypeModel`, `WheelTierModel`) | Plain C# with no MonoBehaviours and no ScriptableObjects. Owns every game rule: zone types, which wheel a zone uses, reward growth, leaving, bombs, revive cost and payment, depositing currencies. |
| **Configuration** | `GameConfiguration` | The thin layer between Data and Gameplay. Turns the ScriptableObjects into Gameplay models once at startup, and maps models back to their assets when the UI needs an icon or a wheel sprite. |
| **Core** | `IEventBus`, `EventBus`, `IGameEvent` | A typed publish/subscribe event bus shared by the other layers. |
| **UI** | `WheelViewMono`, `ZoneBarViewMono`, `ZoneInfoViewMono`, `RewardPanelViewMono`, `RewardFlyAnimatorMono`, popups | Passive views. They display what they are given, play DOTween animations and publish the player's requests. They contain no game rules. |
| **Presentation** | `WheelGameBootstrapMono`, `WheelGamePresenter` | `WheelGameBootstrapMono` is the composition root: it builds the configuration, event bus, wallet, session and presenter. `WheelGamePresenter` is a plain C# class that connects session events to view updates and view requests to the session. |

Dependencies point one way: Gameplay depends only on Core and on its own abstractions (`IWallet`, `IRandomProvider`, `IEventBus`). Data depends on nothing. Configuration depends on Data and Gameplay. UI and Presentation sit on top.

### How the systems communicate

1. A view turns a button press into a request event on the bus, e.g. `WheelViewMono` publishes `SpinRequestedEvent`. Views never call the game logic directly.
2. `WheelGamePresenter` receives the request and asks `GameSession` to act, e.g. `TryBeginSpin`.
3. `GameSession` decides the outcome first (the slice is picked before any animation) and publishes state events such as `ZoneChangedEvent`, `SessionStateChangedEvent`, `BombHitEvent` and `RewardsCollectedEvent`. `RewardInventory` publishes `RewardAmountChangedEvent`.
4. The presenter receives those events, uses `GameConfiguration` to find the matching assets, and updates the views.
5. Animations report completion through a callback passed to the view, not the bus. When the wheel stops, `WheelViewMono` calls back; the presenter then plays the reward fly animation or the bomb shake, and calls `GameSession.CompleteSpin()` when that finishes. A completion callback answers one specific call, so a broadcast event would add nothing.
6. If a view is disabled in the middle of a spin animation, it publishes `SpinAnimationInterruptedEvent` and the presenter completes the spin right away. Since the result was already chosen, the session never gets stuck in the spinning state. `CompleteSpin` ignores repeated calls, so a late callback cannot apply the result twice.

Because the result is chosen before the wheel animates, the outcome is testable and does not depend on frame rate.

### Scene flow

The game has a single scene, `Assets/_Project/Scenes/WheelGame`, which is the only scene in the build.

1. `WheelGameBootstrapMono.Awake` builds `GameConfiguration` from `zone_settings` and `economy_settings`. If a wheel has more slices than `WheelViewMono` can show, it stops with an error instead of starting.
2. It then creates the event bus, the `PlayerPrefsWallet`, the `GameSession` and the `WheelGamePresenter`. The presenter subscribes in `OnEnable` and unsubscribes in `OnDisable`.
3. `Start` calls `GameSession.TryStartRun()`, which moves the session from `NotStarted` to `Idle` at zone 1.
4. During a run the session moves between `Idle` and `Spinning`. A bomb moves it to `Lost` and opens the bomb popup, where the player can give up (a new run) or pay to revive (back to `Idle` on the next zone). Leaving in a safe or super zone moves it to `Collected` and opens the collected popup, whose button starts a new run.

`TryStartRun` only works before the first run and after collecting, so a run cannot be restarted in the middle.

### Content without code changes

- **Zone types** are `ZoneTypeDefinition` assets with an interval (every Nth zone; 0 marks the default type), an *Is Safe* flag (no bomb, the player may leave) and a list of wheel tiers. When several types match a zone, the largest interval wins, so zone 30 is a super zone rather than a safe zone. Adding a new zone type means creating an asset and adding it to `zone_settings`.
- **Wheel tiers** unlock better wheels at later zones. The normal wheel changes at zones 1, 6, 16 and 31, right after safe zones. Reward amounts also grow every zone.
- **Rewards and currencies** are `RewardDefinition` assets. A reward with *Is Currency* ticked is added to the wallet under its id when the player leaves, so a new currency is just a new asset. The revive currency and cost are set in `economy_settings`.
- **Zone styles** in `ZoneTheme` are a list of zone type and style pairs, with a default style for types that have no entry.
- **Zone info boxes** in the top right are `ZoneInfoEntryViewMono` components that each point to a zone type. Adding a box means duplicating it in the prefab.
- **Animations**: every duration, ease and target value is a serialized field on the view that plays it.

`ShippedConfigurationTests` load the real assets through `GameConfiguration`, the same path the game uses, and check the shipped rules: leaving only at safe and super zones, no bomb on safe wheels, a bomb on every risky wheel, normal wheels that change as the run goes on, every wheel fitting the wheel view, and a revive that costs a currency.

### Code conventions

Constants are `SCREAMING_SNAKE_CASE`, booleans start with `Is` or `Has`, MonoBehaviours end with `Mono`, members use block bodies, classes are grouped with regions, Unity objects are null-checked with `if (obj)`, and tween callbacks are named methods linked to their GameObject with `SetLink`.

## Design patterns

All five patterns below are covered in the "Must watch and learn" resources: Unity's *Level up your code with design patterns and SOLID* and the refactoring.guru catalog.

### 1. Observer: event bus

- **Where:** `IEventBus` and `EventBus` in `Core/Events`; events in `SessionEvents.cs` and `UiRequestEvents.cs`. `GameSession`, `RewardInventory` and the views publish; `WheelGamePresenter` subscribes.
- **Why:** publishers don't know who listens. `GameSession` reports that the zone changed without referencing any UI, and a view reports that Spin was pressed without referencing the session. Events are structs, so publishing does not allocate.
- **Without it:** each view would expose its own C# `event Action`, which the first version did, and the presenter would subscribe to each one by hand. Every new button meant a new event field on the view plus new wiring, and views and listeners stayed coupled by type. The alternative of the session calling views directly would put UI references inside the game logic, so the rules could not be tested without a scene.

### 2. Model-View-Presenter

- **Where:** model `GameSession`; views `WheelViewMono`, `ZoneBarViewMono`, `RewardPanelViewMono`, `BombPopupViewMono`, `CollectedPopupViewMono` and others; presenter `WheelGamePresenter`, created by `WheelGameBootstrapMono`.
- **Why:** game rules and display change for different reasons. Keeping rules in a plain C# model means they run in EditMode tests without Unity scenes, and views can be restyled without touching rules.
- **Without it:** each MonoBehaviour would hold part of the state and part of the rules. In the first version, for example, the presenter checked the gold balance and charged for the revive, outside the tested game logic. Rules spread across views are hard to find, easy to duplicate inconsistently, and cannot be unit tested.

### 3. Object Pool

- **Where:** `RewardPanelViewMono` keeps a `Stack<RewardRowViewMono>` of rows; `RewardFlyAnimatorMono` reuses its flying icons; `ZoneBarViewMono` recycles a fixed set of 15 `ZoneCellViewMono`s and renumbers them after each shift.
- **Why:** these objects appear and disappear constantly, on every reward and every zone change. Reusing them avoids `Instantiate`/`Destroy` and garbage collection spikes, which matters on mobile.
- **Without it:** the panel would instantiate a row per reward and destroy all rows on restart, the fly animation would create and destroy five images per spin, and the zone bar would destroy and create cells every zone. That means garbage collection, possible hitches mid-animation, and more UI rebuild work.

### 4. Strategy, through interfaces

- **Where:** `IRandomProvider` (`SystemRandomProvider` in the game, `FixedRandomProvider` in tests) and `IWallet` (`PlayerPrefsWallet` in the game, `FakeWallet` in tests), injected into `GameSession`.
- **Why:** the session needs randomness and a currency store, but should not depend on how they work. Tests can force a bomb or a specific slice and control each currency's balance.
- **Without it:** `GameSession` would call `UnityEngine.Random` and `PlayerPrefs` directly. Tests could not force an outcome, would read and write real saved data, and swapping the save system later would mean editing the game logic.

### 5. Flyweight, with ScriptableObjects

- **Where:** `RewardDefinition`, `WheelDefinition` and `ZoneTypeDefinition` assets, and the models `GameConfiguration` builds from them once. One `reward_gold` asset and one `RewardModel` are shared by every wheel slice, reward row and flying icon that shows gold.
- **Why:** the shared, unchanging data (icon, slice layout, zone rules) is stored once. Only the per-use data lives in each object: the amount on a slice, the total in a row, the position of an icon. Editing one asset updates every place that uses it.
- **Without it:** each slice and row would carry its own copy of the icon and settings, for example as serialized fields set by hand on each prefab instance. That duplicates data in memory and in every scene or prefab file, and changing a reward's icon would mean finding and editing every copy, with a real risk of missing one.

## AI usage

- **Tool:** Claude (Anthropic), used as a chat assistant throughout the project.
- **What it was used for:**
  - planning the architecture and folder structure from the case brief
  - writing most of the C# code, the tests and an editor tool that generated the first version of the prefabs and scene, which was removed before submission
  - diagnosing Unity errors: DOTween assembly references, the Input System module, package versions when moving to Unity 6000.3.23f1
  - reviewing the code for dead code, magic numbers and inconsistent naming
  - the two revisions after Vertigo's feedback
- **How I reviewed and verified the output:**
  - read through every line of code to understand it and see that no systematic problems were present
  - ran every version in the Unity editor and checked the layout at 20:9, 16:9 and 4:3
  - played the build on an Android phone
  - ran the EditMode tests in the Test Runner after each change
  - when the behaviour didn't match what I saw in Critical Strike, described the problem and had it reworked. For example, the indicator first moved on its own instead of being pushed by the wheel, and the zone bar changed colour only after it stopped moving.
  - checked that the Unity UI rules in the brief were followed: naming, raycast targets, sliced sprites, anchors, and buttons connected in code

## Prompts

Prompts I wrote while working on this project:

1. > I got a demo project for vertigo games' critical strike for a job application. I'll give you the game's screenshots and the provided ui as needed. We'll make the demo, explain every line of code as we write them

2. > The first three photos are to check the different resolutions. When I press spin it truns and slows down with time, however the ticking of the indicator doesn't feel realistic, add a rigidbody to it and make the wheel literally hit it?
   > When the level is changing, the square on top of the current level and the current levels numbers change color after the correct placement is achived wihch looks bad. On the forth photo I can't press escape even though I'm on the 7th level

3. > it still looks bad towards the end when the wheel spin gets slower and I can see that it's not the edges of the wheel that's pushing the indicator but rather the indicator doing its own thing
