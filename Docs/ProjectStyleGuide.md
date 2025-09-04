# Docs/ProjectStyleGuide.md

## 1) C# Code Style (Unity‑friendly)

**Language level:** C# 10+. **.NET API Compatibility:** Unity LTS default.

### 1.1 Naming

* **Types (classes/structs/enums):** `PascalCase` → `TrickManager`, `BeloteRules`, `CardId`.
* **Interfaces:** `I` prefix + `PascalCase` → `ICardProvider`, `IRoundResolver`.
* **Methods / Events:** `PascalCase` → `ResolveTrick()`, `OnScoreChanged`.
* **Fields (private):** `_camelCase` → `_currentTrick`, `_deck`; mark `[SerializeField]` when needed.
* **Fields (public):** `PascalCase` (properties preferred) → `HandSize`.
* **Constants:** `UPPER_SNAKE_CASE` → `MAX_PLAYERS`.
* **ScriptableObjects:** Suffix with a semantic noun → `RulesConfig`, `AIDifficultyProfile`, `CardArtSet`.
* **MonoBehaviours:** Avoid “Manager” unless it truly orchestrates. Prefer the domain noun → `TrickResolver`, `DealAnimator`, `AudioRouter`. One class per file.
* **Namespaces:** `Company.Product.Feature` → `NajdGames.Belote.Gameplay`, `NajdGames.Belote.UI`.

### 1.2 Structure & Patterns

* **Folder ↔ Namespace:** Mirror `Assets/Scripts/...` → `NajdGames.Belote.<FolderNames>`.
* **Dependency Direction:** `Infrastructure → Services → Managers → GameLogic → UI`. Lower layers must not depend on higher layers.
* **Update Loops:** Centralize in a small number of systems (e.g., `GameLoop`, `InputRouter`). Avoid many per-frame `Update()`s.
* **Data:** Put static/balancing data in ScriptableObjects (immutable at runtime) and runtime state in plain C# models.
* **Events:** Prefer C# events/`UnityEvent` for user-facing, C# events for internal systems. Use **weak coupling**.
* **Async:** Use coroutines for frame-based sequences; `Task` for I/O. Never block main thread.

### 1.3 Formatting

* **Braces:** Allman style.
* **Max line length:** 120 chars.
* **Nullability:** Enable and annotate (`?`).
* **Comments:** Use `///` XML for public APIs. Keep summaries short and precise.

---

## 2) Script Placement in Project

```
Assets/
 ├─ Scripts/
 │   ├─ Interfaces/        // contracts only (no logic)
 │   ├─ Managers/          // orchestration systems (rare)
 │   ├─ GameLogic/         // rules, turn flow, scoring, trick resolution
 │   ├─ Networking/        // if applicable
 │   ├─ AI/                // simple rule-based agents, MCTS, etc.
 │   ├─ UI/                // presenters, view-models, bindings
 │   ├─ Services/          // Audio, Save, Addressables, Telemetry
 │   ├─ Infrastructure/    // wrappers: PlayerPrefs, File I/O, HTTP
 │   └─ Utilities/         // stateless helpers/extensions only
```

* **Interfaces** reference nothing (pure abstractions).
* **Utilities** must be dependency-free and static.
* **GameLogic** has no Unity-specific code except pure models/logic; keep it testable.

---

## 3) Prefab & Scene Conventions

* **Prefab name:** `PascalCase` object noun + optional role → `CardView`, `HandPanel`, `TrickSlot`, `ScorePopup`.
* **Root GameObject name == Prefab file name.**
* **UI prefabs:** Prefix `UI_` for clarity → `UI_HandPanel`, `UI_BidDialog`.
* **Variant prefabs:** Suffix `.Variant` → `CardView.Variant`.
* **Temporary/Prototype:** Suffix `.WIP` (do not commit to `main`).
* **Component order:** Transforms → Layout → Visuals → Scripts. Remove missing scripts.
* **Scene naming:** `00_Boot`, `10_Menu`, `20_Game`, `90_Sandbox`.
* **One Canvas rule:** Prefer a single root `Canvas` with sub-canvases for specialized rendering if required.
* **Do not store live Scene instances** in VCS when playtesting—use test scenes under `Scenes/_Sandboxes/`.

---

## 4) ScriptableObject Conventions

* **File name:** `<Thing><Suffix>` → `RulesConfig`, `AIDifficultyProfile_Easy`.
* **Addressable label:** `config` + a domain tag → `config:rules`, `config:ai:easy`.
* **Immutable at runtime:** Do not `Instantiate` to mutate; copy into runtime models.

---

## 5) Belote Domain Types (Examples)

* **`CardId`**: `{ Suit: Clubs/Spades/Hearts/Diamonds, Rank: Seven/Eight/Nine/Ten/Jack/Queen/King/Ace }`.
* **Sprites**: front/back per suit/rank; name via `Assets/Docs/AssetNamingConventions.md`.
* **Rules**: ScriptableObject `RulesConfig` controls *trump selection, belote/rebelote, scoring table, capot*, etc.

---

## 6) Testing

* **Runtime Tests:** `Assets/Tests/PlayMode`.
* **EditMode Tests:** `Assets/Tests/EditMode`.
* Test pure logic in `GameLogic` without Unity dependencies.

---

## 7) Addressables & Resources

* **Addressables only.** No `Resources/`.
* **Groups by domain:** `Sprites/Cards`, `Sprites/Icons`, `Audio/Music`, `Audio/SFX`, `Configs`.
* **Labels:** hierarchical (`cards:clubs`, `ui:icons`, `audio:sfx:ui`).
* **Loading:** Centralize in `Services/AssetService` wrapper. Always handle release/unload.

---

## 8) Localization

* **Tables:** `UI`, `Rules`, `Errors`.
* **Key format:** `context_key` lower\_snake → `menu_play`, `bid_choose_trump`, `error_network_timeout`.
* **No hardcoded strings** in scripts. Use `LocalizedString` or binding layer.

---

## 9) Git Branch Strategy (Trunk‑Based + Release)

* **Mainline:** `main` (always releasable; protected).
* **Short‑lived branches:** `feature/<ticket>-<slug>`, `fix/<ticket>-<slug>`, `refactor/<area>`, `chore/<area>`.
* **Release branches:** `release/vX.Y.Z` for stabilization; only fixes and localization updates.
* **Hotfix:** `hotfix/vX.Y.(Z+1)` from `main`, cherry-pick into `release/*` if needed.
* **Pull Requests:** Required reviews (≥1), CI green, no direct commits to `main`.
* **Versioning:** Semantic Versioning. Tag `vX.Y.Z` on release branch merge.

**Examples:**

* `feature/123-bidding-dialog`
* `fix/201-card-depth-sorting`
* `release/v0.6.0`

---

## 10) Commit Message Style (Conventional Commits)

Use **type(scope): message** + optional body + footer.

**Types:** `feat`, `fix`, `refactor`, `perf`, `chore`, `ci`, `docs`, `test`, `build`.

**Scopes (suggested):** `scripts`, `prefabs`, `addressables`, `ui`, `audio`, `localization`, `ai`, `rules`, `net`, `infrastructure`.

**Examples:**

* `feat(ui): add bidding dialog with timed choices`
* `fix(prefabs): correct CardView pivot to avoid jitter`
* `refactor(gameLogic): extract trick resolution into service`
* `docs: add ProjectStyleGuide and AssetNamingConventions`

**Bodies:** What/why, not how. Reference tickets.

**Breaking change:**

```
feat(rules): rename TricksWon to Tricks

BREAKING CHANGE: public API of ScoreCalculator changed; update callers.
```

---

## 11) Code Review Definition of Done

* ✅ Builds locally & CI passes
* ✅ No null ref risks (defensive checks) and no allocations in hot paths
* ✅ Names follow this guide
* ✅ Tests updated/added for logic changes
* ✅ Addressables labels set; assets referenced via service (no hard refs)
* ✅ Localization keys added; no hardcoded strings
* ✅ Perf sanity (no per-frame GC, no `Find()` calls)

---

## 12) Unity Project Settings (defaults)

* **Sprite Mode:** Single; Pixels Per Unit chosen per UI scale (e.g., 100).
* **Texture Compression:** Automatic; cards use `RGBA32` if crisp UI required.
* **Audio:** Force to mono for SFX where applicable; preload critical UI SFX.

---