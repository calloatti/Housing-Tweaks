Include ..\AGENTS.md

# Housing Tweaks — Mod-Specific Agent Instructions

## Identity
- **Assembly:** `housingtweaks`
- **Namespace:** `Calloatti.HousingTweaks`
- **Framework:** Harmony
- **ModId:** `Calloatti.HousingTweaks`
- **Min Game Version:** 1.0.0.0 — uses `timberborn-decompiled-1.0.*`

## What This Mod Does
Tweaks housing capacity values. Uses Harmony patches to override building capacity and fetches original capacity values for reference.

## Source Architecture (`Version-1.0/Source/`)

| File | Role |
|---|---|
| `ModStarter.cs` | Entry point — `IModStarter` |
| `HousingCapacityPatcher.cs` | Harmony patches for housing capacity |
| `OriginalCapacityFetcher.cs` | Utility to read original capacity values |
