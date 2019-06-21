using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using UnityModManagerNet;
using UnityEngine;
using UnityEngine.Networking;
using Kingmaker.Blueprints;
using Kingmaker.Visual.CharacterSystem;
using Kingmaker.Blueprints.Classes;
using Kingmaker;
using Kingmaker.Utility;
using System.IO;
using UnityEditor;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Newtonsoft.Json;
using ArcaneTide;
using static UnityModManagerNet.UnityModManager;

namespace ArcaneTide {
    /*
    [Harmony12.HarmonyPatch(typeof(Player),"PostLoad",new Type[] { })]
    class LocalizationPatch {
        static public ModEntry.ModLogger logger => Main.logger;
        static public string ModPath => Main.ModPath;
        static void LoadStrings() {
            Locale locale = LocalizationManager.CurrentLocale;
            string path = Path.Combine(ModPath, "localization", $"{locale.ToString()}.json");
            logger.Log(path);
            if (File.Exists(path)) {
                try {
                    using (StreamReader streamReader = new StreamReader(path)) {
                        var modText = JsonConvert.DeserializeObject<LocalizationPack>(streamReader.ReadToEnd());
                        foreach (var item in modText.Strings) {
                            if (LocalizationManager.CurrentPack.Strings.ContainsKey(item.Key)) {
                                continue;
                            }
                            logger.Log($"RUA {LocalizationManager.CurrentPack.Strings.Count}");
                            LocalizationManager.CurrentPack.Strings.Add(item.Key, item.Value);
                        }
                        logger.Log($"RUAB {LocalizationManager.CurrentPack.Strings.Count}");

                    }
                }
                catch {
                    return;
                }

            }
        }
        static public void Postfix() {
            LoadStrings();
        }
    }*/
    [Harmony12.HarmonyPatch(typeof(LocalizationManager),"LoadPack",new Type[] { typeof(Locale)})]
    class LocalizationManager_LoadPack_Postfix {
        static public ModEntry.ModLogger logger => Main.logger;
        static public string ModPath => Main.ModPath;
        static public void Postfix(LocalizationManager __instance, ref LocalizationPack __result) {
            Locale locale = LocalizationManager.CurrentLocale;
            string path = Path.Combine(ModPath, "localization", $"{locale.ToString()}.json");
            logger.Log(path);
            if (File.Exists(path)) {
                try {
                    using (StreamReader streamReader = new StreamReader(path)) {
                        var modText = JsonConvert.DeserializeObject<LocalizationPack>(streamReader.ReadToEnd());
                        foreach (var item in modText.Strings) {
                            if (__result.Strings.ContainsKey(item.Key)) {
                                continue;
                            }
                            logger.Log($"RUA {__result.Strings.Count}");
                            __result.Strings.Add(item.Key, item.Value);
                        }
                        logger.Log($"RUAB {__result.Strings.Count}");

                    }
                }
                catch {
                    return;
                }
            }
        }
    }
}
