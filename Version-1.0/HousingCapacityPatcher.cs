using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
using Timberborn.DwellingSystem;
using Timberborn.BlueprintSystem;

namespace Calloatti.HousingTweaks
{
  [HarmonyPatch(typeof(SpecService), "Load")]
  public static class HousingCapacityPatcher
  {
    // Cache the backing field once. MaxBeavers is an { get; init; } property on DwellingSpec.
    public static readonly FieldInfo MaxBeaversField = typeof(DwellingSpec).GetField("<MaxBeavers>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

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
        bool fileModified = false;

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
            ModStarter.Config.SetComment(blueprint.Name, $"Default value: {defaultCap}");
            fileModified = true;
          }

          Debug.Log($"[HousingTweaks] {blueprint.Name} | Default: {defaultCap} | Modded: {moddedCap}");

          // 3. Apply modded capacity if it differs from the default
          if (moddedCap != defaultCap)
          {
            MaxBeaversField?.SetValue(blueprint.GetSpec<DwellingSpec>(), moddedCap);
          }
        }

        if (fileModified)
        {
          Debug.Log("[HousingTweaks] Saving dynamic keys via SimpleConfig...");
          ModStarter.Config.Save();
        }
      }
      catch (Exception ex)
      {
        Debug.LogError($"[HousingTweaks] Error in ProcessConfig: {ex}");
      }
    }
  }
}