using Calloatti.Config;
using HarmonyLib;
using Timberborn.ModManagerScene;
using UnityEngine;

namespace Calloatti.HousingTweaks
{
  public class ModStarter : IModStarter
  {
    public static SimpleConfig Config { get; private set; }

    public void StartMod(IModEnvironment modEnvironment)
    {
      Config = new SimpleConfig(modEnvironment.ModPath);

      new Harmony("calloatti.housingtweaks").PatchAll();

      Debug.Log("[HousingTweaks] Harmony Patches Applied.");
    }
  }
}