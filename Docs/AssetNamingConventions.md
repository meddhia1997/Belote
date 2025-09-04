# Docs/AssetNamingConventions.md

> **Scope:** Filenames, labels, and folder placement for all assets (Sprites, Audio, Prefabs, ScriptableObjects, Localization). The goals: searchability, sortability, and zero collision across locales and platforms.

---

## 1) General Rules

* **Filenames:** `lower_snake_case` with semantic segments.
* **No spaces, no accents, no emojis.** ASCII only.
* **Avoid plurals** unless the asset is a set.
* **Include category prefix** for discoverability.
* **Use leading zeros** for numeric series (e.g., `07`).
* **File extension lower-case**.

**Template:**

```
<category>_<subcat>_<descriptor>[_v##][_pX]
```

* `v##` = authored version (optional)
* `pX` = platform/size suffix (e.g., `p2k` for 2048px)

---

## 2) Sprites (Addressable)

**Location:** `Assets/AddressableAssets/Sprites/...`

### 2.1 Cards (Fronts/Backs)

```
// Fronts
card_front_<suit>_<rank>.png      // card_front_clubs_07.png
card_front_<suit>_<rank>@2x.png   // high DPI variant if needed

// Backs (themes/skins)
card_back_<theme>[_v##].png       // card_back_classic.png, card_back_dark_v02.png
```

* **Suits:** `clubs`, `spades`, `hearts`, `diamonds`.
* **Ranks:** `07, 08, 09, 10, jack, queen, king, ace` (Belote deck).
* **Pivot:** centered; set tight mesh off for crisp UI.
* **Addressables labels:** `cards:<suit>`, `cards:rank:<rank>`, `cards:back:<theme>`.

### 2.2 Icons & UI

```
icon_ui_<name>.png                // icon_ui_trump.png, icon_ui_score.png
bg_ui_<context>.png               // bg_ui_table.png, bg_ui_menu.png
```

* **Labels:** `ui:icons`, `ui:backgrounds`.

---

## 3) Audio

**Location:**

* `Assets/AddressableAssets/Audio/SFX/`
* `Assets/AddressableAssets/Audio/Music/`

**Filnames:**

```
sfx_<action>[_<detail>].wav       // sfx_deal_cards.wav, sfx_button_click.wav
music_<mood>[_bpm###].wav         // music_calm_90.wav, music_tense_120.wav
```

**Import & Mixing (defaults):**

* **SFX:** mono preferred; peak around **-3 dBFS**; short tails trimmed; `Load In Background` off for critical SFX; `Preload Audio Data` on.
* **Music:** stereo; loopable with sample-accurate loop points; integrated loudness around **-16 LUFS**.
* **Addressables labels:** `audio:sfx:<action>`, `audio:music:<mood>`.

---

## 4) Prefabs

**Location:** `Assets/Prefabs/...`

**Filenames:**

```
ui_<widget>.prefab                // ui_bid_dialog.prefab, ui_score_panel.prefab
game_<entity>.prefab              // game_card_view.prefab, game_trick_slot.prefab
```

**Rules:**

* Prefab root name == file name in `PascalCase` for the GameObject (e.g., `UI_BidDialog`).
* Variants: `*_variant_##.prefab` → `game_card_view_variant_01.prefab`.
* Temporary: suffix `_wip` (never on `main`).

---

## 5) ScriptableObjects

**Location:** `Assets/ScriptableObjects/...`

**Filenames:**

```
rules_config.asset                // game ruleset knobs
aiprofile_<name>.asset            // aiprofile_easy.asset
card_art_set_<theme>.asset        // card_art_set_classic.asset
```

**Labels:** `config:rules`, `config:ai:<name>`, `cards:artset:<theme>`.

---

## 6) Localization

**Location:** `Assets/Localization/StringTables/`

**Table filenames:** `ui.table`, `rules.table`, `errors.table`.

**Key format:** `context_key` (lower\_snake) → `menu_play`, `bid_take`, `error_illegal_card`.

**Locale folders:** `Assets/Localization/Locales/<locale_code>/` → `fr-FR`, `ar-TN`, `en-US`.

**Audio/VO (if any):** `vo_<locale>_<line_id>.wav`.

---

## 7) Textures & Art Placeholders

**Location:** `Assets/Art/Placeholders/`

**Filenames:** `ph_<category>_<descriptor>[_v##].png` → `ph_card_front.png`.

**Replace policy:** Track via ticket; delete when real art lands.

---

## 8) Config Files & JSON

**Location:** `Assets/Config/`

**Filenames:** `cfg_<area>_<purpose>.json` → `cfg_rules_default.json`, `cfg_ai_baseline.json`.

---

## 9) Addressables Labels & Groups

* **Groups:** mirror folders → `Sprites/Cards`, `Sprites/Icons`, `Audio/SFX`, `Audio/Music`, `Configs`.
* **Labels:** semantic, hierarchical `domain:sub:detail`.
* **Example:** `cards:clubs`, `cards:rank:jack`, `ui:icons`, `audio:sfx:deal`.

---

## 10) Example Mapping (Belote)

* `Assets/AddressableAssets/Sprites/Cards/card_front_clubs_jack.png` → labels: `cards:clubs`, `cards:rank:jack`.
* `Assets/Prefabs/Game/game_card_view.prefab` (root `GameObject`: `Game_CardView`).
* `Assets/ScriptableObjects/Rules/rules_config.asset` → label `config:rules`.
* `Assets/AddressableAssets/Audio/SFX/sfx_deal_cards.wav` → label `audio:sfx:deal`.

---

## 11) Do/Don’t Checklist

**Do**

* Use lower\_snake\_case for files; PascalCase for GameObjects and types.
* Add Addressables labels on import.
* Keep suits and ranks fixed to Belote set (07–Ace).

**Don’t**

* Put assets under `Resources/`.
* Use spaces or accents in filenames.
* Commit `.WIP` or `_wip` to `main`.

---
