# Copilot Instructions — Grid Frontline

## Build & Test

```powershell
# Build
dotnet build

# Run the game (headless, no window)
& "path/to/Godot_v4.3-stable_mono_win64_console.exe" --headless --path .

# Run automated tests (27 tests, exits automatically)
& "path/to/Godot_v4.3-stable_mono_win64_console.exe" --headless --path . res://scenes/main/Test.tscn

# Run the game with GUI
& "path/to/Godot_v4.3-stable_mono_win64.exe" --path .
```

Tests live in `scripts/Core/AutoTest.cs` and run via `scenes/main/Test.tscn`. Add new test methods to `AutoTest._Ready()` using the `Assert(bool, string)` helper.

## Architecture

**GameManager** (`scripts/Core/GameManager.cs`) is the central orchestrator. It is the root node of `Main.tscn` and creates all subsystems from code in `_Ready()`:

```
GameManager (root)
├── EconomyManager   — gold tracking, AddGold/SpendGold/CanAfford
├── Camera2D
├── GameBoard        — 6×8 GridCell array + corridor/enemy zone visuals
├── RallyZone        — soldier queuing between player zone and corridor
└── UILayer (CanvasLayer)
    ├── HUD          — gold display, debug buttons
    └── BuildPanel   — building selection buttons
```

**Signal flow** — components communicate via Godot signals, wired in `GameManager.WireSignals()`:

```
BuildPanel.BuildingSelected → GameManager stores selection
GridCell.CellClicked → GameBoard re-emits → GameManager places building
MilitaryBuilding.UnitProduced → GameManager → RallyZone.AddUnit()
RallyZone deploy button → Unit.Deploy() + unblocks MilitaryBuilding
EconomyManager.GoldChanged → HUD updates display
```

**Code-first scene creation**: Only `Main.tscn` and `Test.tscn` exist as scene files. All nodes (grid cells, buildings, units, UI) are created programmatically via `new` + `AddChild()`. This means new node types do NOT need `.tscn` files.

## Key Conventions

**Namespace**: All classes use `namespace GridFrontline;` (file-scoped).

**Node classes**: Must be `public partial class` (Godot 4 C# requirement).

**Naming**:
- Files/classes: `PascalCase` — `MilitaryBuilding.cs`
- Private fields: `_camelCase` — `_productionBlocked`
- Signals: `[Signal] public delegate void {Name}EventHandler(...)` — the `EventHandler` suffix is required by Godot
- Signal handlers: `On{SignalName}` — `OnCellClicked`, `OnUnitProduced`

**Building system**: Buildings are defined as data + behavior pairs:
- `BuildingData` (Resource subclass) holds stats (cost, HP, interval, color)
- `BuildingDatabase` (static class) provides predefined instances — add new buildings here
- `Building` → `EconomyBuilding` / `MilitaryBuilding` implement behavior
- The `IsUnitProducer` flag on `BuildingData` determines which subclass `GameManager.PlaceBuilding()` instantiates

**Rally/production blocking**: When a `MilitaryBuilding` produces a unit, the unit goes to `RallyZone` and the building's `ProductionBlocked = true`. Production resumes only after the player deploys. This is a core game mechanic — do not bypass it.

**UI mouse filter**: UI controls (`HUD`, `BuildPanel`) must set `MouseFilter = MouseFilterEnum.Ignore` so clicks pass through to the game world's `Area2D` grid cells.

**Placeholder visuals**: Phase 1 uses `ColorRect` + `Label` for all visuals. Buildings show a single character (`F` for farm, `B` for barracks). Units are small blue squares.

## Game Design Reference

See `docs/high-level-plan.md` for the full game design (map structure, systems, strategy layers). See `docs/phase1-detail-plan.md` for Phase 1 implementation details. The game is a strategy + real-time combat + card-building hybrid where soldiers have auto-chess-style skills and move freely in 2D (not lane-locked).
