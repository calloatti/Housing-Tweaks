using System;
using UnityEngine;
using Timberborn.BlueprintSystem;

namespace Calloatti.HousingTweaks
{
  public static class OriginalCapacityFetcher
  {
    // --- 1. SAFE JSON RETRIEVAL ---
    public static string GetRawJson(BlueprintSourceService sourceService, Blueprint blueprint)
    {
      try
      {
        var bundle = sourceService.Get(blueprint);
        if (bundle == null) return null;

        if (bundle.Jsons.Length > 0)
        {
          return bundle.Jsons[0];
        }
      }
      catch (Exception ex)
      {
        Debug.LogWarning($"[HousingTweaks] Failed to extract JSON for {blueprint.Name}: {ex.Message}");
      }

      return null;
    }

    // --- 2. ORIGINAL CAPACITY RETRIEVAL ---
    public static int GetOriginalCapacity(Blueprint blueprint, string originalJson)
    {
      if (string.IsNullOrEmpty(originalJson)) return -1;

      try
      {
        // Target MaxBeavers instead of MaxCapacity
        string searchKey = "\"MaxBeavers\":";
        int index = originalJson.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase);

        if (index != -1)
        {
          int startIndex = index + searchKey.Length;
          int endIndex = originalJson.IndexOfAny(new char[] { ',', '}' }, startIndex);

          if (endIndex != -1)
          {
            string valueStr = originalJson.Substring(startIndex, endIndex - startIndex).Trim();
            if (int.TryParse(valueStr, out int capacity))
            {
              return capacity;
            }
          }
        }
      }
      catch (Exception ex)
      {
        Debug.LogWarning($"[HousingTweaks] Failed to parse JSON capacity for {blueprint.Name}: {ex.Message}");
      }

      return -1;
    }
  }
}