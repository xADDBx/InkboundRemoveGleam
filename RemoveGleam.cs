using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using InkboundModEnabler.Util;
using ShinyShoe;
using ShinyShoe.Ares;
using ShinyShoe.SharedDataLoader;
using System.Reflection;

namespace RemoveGleam {
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [CosmeticPlugin]
    public class RemoveGleam : BaseUnityPlugin {
        public const string PLUGIN_GUID = "ADDB.RemoveGleam";
        public const string PLUGIN_NAME = "Remove Gleam";
        public const string PLUGIN_VERSION = "1.0.1";
        public ConfigEntry<bool> GleamIfDaily;
        public ConfigEntry<bool> DisableQuestGleam;
        public static ManualLogSource log;
        public static RemoveGleam instance;
        public static Harmony HarmonyInstance => new Harmony(PLUGIN_GUID);
        private void Awake() {
            instance = this;
            log = Logger;
            log.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");
            DisableQuestGleam = Config.Bind("", "DisableQuestGleam", false, new ConfigDescription("Completely disable Gleam effects for the quest section and button."));
            GleamIfDaily = Config.Bind("", "GleamIfDaily", true, new ConfigDescription("Gleam if daily tasks are available"));
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }
        [HarmonyPatch(typeof(QuestSystem))]
        public static class QuestSystem_Patch {
            [HarmonyPatch("AreAnyUnfinishedQuestLogQuestsAvailable")]
            [HarmonyPrefix]
            public static bool AreAnyUnfinishedQuestLogQuestsAvailable(QuestSystem.State state, EntityHandle playerHandle, WorldEngine.IReadonly worldEngineRo, WorldState.IReadonly worldStateRo, AssetLibrary assetLibrary, ref bool __result) {
                if (!instance.DisableQuestGleam.Value) {
                    foreach (ShinyShoe.Ares.SharedSOs.QuestData questData in state.availableQuests) {
                        if (questData.AssetAddressIcon != "05732be845f197142b9c377cd66deb59[UI_Atlas_QuestReward_128px_6]"
                            && !questData.RunOnly
                            && (instance.GleamIfDaily.Value || questData.QuestType != QuestType.Daily)
                            && (QuestHelper.CanAcceptQuest(playerHandle, questData, worldEngineRo, worldStateRo, assetLibrary) || worldStateRo.GetQuestDBRo().IsQuestActive(playerHandle, questData.Guid))) {
                            __result = true;
                            return false;
                        }
                    }
                }
                __result = false;
                return false;
            }
        }
    }
}
