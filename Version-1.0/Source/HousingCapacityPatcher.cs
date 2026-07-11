using HarmonyLib;
using System;
using Timberborn.BlueprintSystem;
using Timberborn.DwellingSystem;
using Timberborn.EntitySystem;
using UnityEngine;

namespace Calloatti.HousingTweaks
{
  [HarmonyPatch(typeof(SpecService), "Load")]
  public static class HousingCapacityPatcher
  {
    // Replaced Reflection FieldInfo with Harmony's FieldRef for direct, zero-allocation memory access.
    // This is required because MaxBeavers is an { get; init; } property on a record.
    public static AccessTools.FieldRef<DwellingSpec, int> MaxBeaversRef = AccessTools.FieldRefAccess<DwellingSpec, int>("<MaxBeavers>k__BackingField");

    [HarmonyPostfix]
    public static void Postfix(SpecService __instance)
    {
      Debug.Log("[HousingTweaks] SpecService.Load Postfix started. Applying capacities...");
      ProcessConfig(__instance);
      Debug.Log("[HousingTweaks] Finished applying housing capacities.");
    }

    private static void ProcessConfig(SpecService specService)
    {
      try
      {
        // Directly access publicized fields
        var sourceService = specService._blueprintSourceService;
        var deserializer = specService._blueprintDeserializer;
        var specDict = specService._cachedBlueprintsBySpecs;

        if (specDict == null || deserializer == null || sourceService == null) return;

        // Target DwellingSpec instead of StockpileSpec
        if (!specDict.TryGetValue(typeof(DwellingSpec), out var lazyList)) return;

        foreach (var lazyObj in lazyList)
        {
          var blueprint = lazyObj.Value;
          if (blueprint == null) continue;

          // 1. Get Default Capacity
          string rawJson = OriginalCapacityFetcher.GetRawJson(sourceService, blueprint);
          int defaultCap = OriginalCapacityFetcher.GetOriginalCapacity(blueprint, rawJson);

          if (defaultCap <= 0) defaultCap = blueprint.GetSpec<DwellingSpec>().MaxBeavers;

          // 2. Modded Capacity using SimpleConfig
          int moddedCap = defaultCap;


          if (ModStarter.Config.HasKey(blueprint.Name))
          {
            moddedCap = ModStarter.Config.GetInt(blueprint.Name);
            if (moddedCap <= 0) moddedCap = defaultCap;
          }
          else
          {
            ModStarter.Config.Set(blueprint.Name, defaultCap);
          }

          // Always apply SetInlineComment to force legacy file comments into the modern layout structure
          ModStarter.Config.SetInlineComment(
            key: blueprint.Name,
            type: "int",
            defaultValue: defaultCap,
            label: blueprint.Name,
            tooltip: "Sets the maximum number of beavers that can live in this building.",
            controlType: "slider",
            minValue: 1,
            maxValue: 100,
            step: 1,
            requiresReload: true
          );
          
          Debug.Log($"[HousingTweaks] {blueprint.Name} | Default: {defaultCap} | Modded: {moddedCap}");

          // 3. Apply modded capacity if it differs from the default
          if (moddedCap != defaultCap)
          {
            MaxBeaversRef(blueprint.GetSpec<DwellingSpec>()) = moddedCap;
          }
        }

         Debug.Log("[HousingTweaks] Saving dynamic keys via SimpleConfig...");
         ModStarter.Config.Save();

      }
      catch (Exception ex)
      {
        Debug.LogError($"[HousingTweaks] Error in ProcessConfig: {ex}");
      }
    }
  }
}