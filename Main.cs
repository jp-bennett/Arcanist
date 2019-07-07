using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.LevelUp;
using Kingmaker.Utility;
using UnityEngine;
using UnityModManagerNet;
using ArcaneTide.Arcanist;
using ArcaneTide.Utils;
using Harmony12;
using UnityEngine.SceneManagement;
using Kingmaker.UnitLogic;
using Kingmaker.Designers;
using Kingmaker.Blueprints.Items.Weapons;
using ArcaneTide.Risia;
using static UnityModManagerNet.UnityModManager.ModEntry;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.View;

namespace ArcaneTide {
    public class Main {
        internal static LibraryScriptableObject library;
        public static bool enabled;
        public static UnityModManager.ModEntry.ModLogger logger;
        public static string ModPath;
        //public static string bundleName = "risia";
        public static AssetBundle Bundle;
        public static Dictionary<string, string> BundleLookup = new Dictionary<string, string>();
        static Harmony12.HarmonyInstance harmonyInstance;
        public static BlueprintCharacterClass arcanist;
        public static bool loaded = false;
        static class UIData {
            static public string posX = "0", posY = "0", posZ = "0";
        }
        [Harmony12.HarmonyPatch(typeof(LibraryScriptableObject), "LoadDictionary", new Type[0])]
        
        static class LibraryScriptableObject_LoadDictionary_Patch {
            
            static void Postfix(LibraryScriptableObject __instance) {
                var self = __instance;
                if (Main.library != null) return;
                Main.library = self;
                logger.Log("Dua 1!?!");
                LoadBundle($"{ModPath}/bundles/risiablue.assetbundle");
                LoadBundle($"{ModPath}/bundles/risia.assetbundle");
                logger.Log("Dua 2!?!");
                
                //use sorcerer to temporarily simulate arcanist
                //use sorcerer to temporarily simulate arcanist
                //use sorcerer to temporarily simulate arcanist
                SafeLoad(Helpers.Load, "Helpers");
                SafeLoad(IconSet.Load, "Icons");
                SafeLoad(MetaFeats.Load, "MetaFeatSet");
                SafeLoad(ArcanistClass.Load, "Arcanist");
                SafeLoad(TestSpawner.TestSpawner.Load, "TestSpawner");
                SafeLoad(TestCopyScene.Load, "");
                Main.arcanist = ArcanistClass.arcanist;
                Main.loaded = true;
            }
        }
        [Harmony12.HarmonyPatch(typeof(Player), "PostLoad", new Type[] {  })]
        static class Player_PostLoad_Patch {
            static void Postfix(Player __instance) {
                var self = __instance;
                
            }
        }
        static bool Load(UnityModManager.ModEntry modEntry) {
            logger = modEntry.Logger;
            ModPath = modEntry.Path;
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            harmonyInstance = Harmony12.HarmonyInstance.Create(modEntry.Info.Id);
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            arcanist = ArcanistClass.arcanist;
            /*
            foreach (var file in Directory.GetFiles(Path.Combine(ModPath, "bundles"))) {

                if (!file.EndsWith("manifest") && Path.GetFileName(file) != bundleName) {
                    BundleLookup[Path.GetFileName(file).Replace("resource_", "")] = file;
                }
            }
            */
            return true;
        }
        static void LoadBundle(string path) {
            var bundle = AssetBundle.LoadFromFile(path);
            if (bundle.isStreamedSceneAssetBundle) {
                /*string[] scenePaths = bundle.GetAllScenePaths();
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePaths[0]);
                SceneManager.LoadScene(sceneName);*/
                return;
            }
            var blueprints = bundle.LoadAllAssets<BlueprintScriptableObject>();
            /*Main.DebugLog("Verifying blueprint ----------------------");
            foreach (var blueprint in blueprints)
            {
                VerifyBundles.VerifyBlueprint(blueprint);
            }
            Main.DebugLog("Finished vefiying blueprint ----------------------");*/
            foreach (var blueprint in blueprints) {
                if (blueprint.name.StartsWith("Existing.")) {
                    continue;
                }
                logger.Log($"Loaded Blueprint {blueprint.name}");
                if (ResourcesLibrary.LibraryObject.BlueprintsByAssetId.ContainsKey(blueprint.AssetGuid)) {
                    logger.Log($"Fuck, Id {blueprint.AssetGuid}, name {blueprint.name} is duplicated!");
                    //throw new Exception($"ResourceLibrary already contains blueprint {blueprint.AssetGuid}");
                    continue;
                }

                ResourcesLibrary.LibraryObject.BlueprintsByAssetId[blueprint.AssetGuid] = blueprint;
                ResourcesLibrary.LibraryObject.GetAllBlueprints().Add(blueprint);
                /*
                if (blueprint is BlueprintRace race) {
                    logger.Log($"Registering Race {blueprint.name}");
                    ref var races = ref Game.Instance.BlueprintRoot.Progression.CharacterRaces;
                    if (races.Contains(race)) return;
                    var length = races.Length;
                    Array.Resize(ref races, length + 1);
                    races[length] = race;
                }
                if (blueprint is BlueprintCharacterClass _class) {
                    logger.Log($"Registering Class {blueprint.name}");
                    ref var classes = ref Game.Instance.BlueprintRoot.Progression.CharacterClasses;
                    if (classes.Contains(_class)) return;
                    var length = classes.Length;
                    Array.Resize(ref classes, length + 1);
                    classes[length] = _class;
                }
                if (blueprint is BlueprintItemWeapon _weapon) {
                    logger.Log($"Registering Weapon {blueprint.name}");
                }
                */
            }
            /*
            foreach (var kv in BundleLookup) {
                ResourcesLibrary.LibraryObject.ResourceNamesByAssetId[kv.Key] = kv.Key;
            }*/
            logger.Log("Do Load Bundle Finish!!!");
            FixBlueprint.Fix(bundle);
        }
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value) {
            enabled = value;
            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry) {
            if (!enabled) return;
            Scene thisScene = SceneManager.GetActiveScene();
            bool flag = thisScene.name == "Start" || thisScene.name == "MainMenu";
            if (flag) {
                GUILayout.Label("Game is not loaded.", Array.Empty<GUILayoutOption>());
                return;
            }
            if (Main.loaded) {
                
                GUILayout.BeginVertical(new GUILayoutOption[] { });
                GUILayout.Label(new GUIContent("All companions:\n"), new GUILayoutOption[] { });
                GUILayout.BeginHorizontal(new GUILayoutOption[] { });
                int followers_cnt = Game.Instance.Player.AllCharacters.Count;
                bool[] buttons = new bool[followers_cnt];
                for (int i = 0; i < followers_cnt; i++) {
                    
                    UnitDescriptor unit = Game.Instance.Player.AllCharacters[i].Descriptor;
                    buttons[i] = GUILayout.Button(new GUIContent($" {unit.CharacterName} "), new GUILayoutOption[] {
                        GUILayout.ExpandWidth(false),
                        GUILayout.MinWidth(30)
                    });
                    if (buttons[i]) {
                        UIData.posX = $"{unit.Unit.Position.x}";
                        UIData.posY = $"{unit.Unit.Position.y}";
                        UIData.posZ = $"{unit.Unit.Position.z}";
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal(new GUILayoutOption[] { });
                UIData.posX = GUILayout.TextField(UIData.posX);
                UIData.posY = GUILayout.TextField(UIData.posY);
                UIData.posZ = GUILayout.TextField(UIData.posZ);
                GUILayout.EndHorizontal();
                bool button2 = GUILayout.Button(new GUIContent("Transport"), new GUILayoutOption[] {
                    GUILayout.ExpandWidth(false)
                });
                if (button2) {
                    UnityEngine.Vector3 position = new UnityEngine.Vector3((float)(Convert.ToDouble(UIData.posX)), (float)(Convert.ToDouble(UIData.posY)), (float)(Convert.ToDouble(UIData.posZ)));
                    GameHelper.GetPlayerCharacter().View.StopMoving();
                    Game.Instance.UI.GetCameraRig().ScrollTo(position);
                    GameHelper.GetPlayerCharacter().Position = position;
                }
                GUILayout.BeginHorizontal(new GUILayoutOption[] { });
                bool button3 = GUILayout.Button(new GUIContent("Export MainChar Prefab"), new GUILayoutOption[] {
                    GUILayout.ExpandWidth(false)
                });
                if (button3) {
                    Bala.ExportMainCharPrefab();
                }
                GUILayout.EndVertical();
                
            }
        }
        static void OnSaveGUI(UnityModManager.ModEntry modEntry) {
        }

        internal static Exception Error(String message) {
            logger?.Log(message);
            return new InvalidOperationException(message);
        }

        internal static void SafeLoad(Action load, String name) {
            try {
                load();
            }
            catch (Exception e) {
                Log.Error(e);
            }
        }
    }
    public class Bala {
        static public LibraryScriptableObject library => Main.library;
        static public ModLogger logger => Main.logger;
        static public void ExportMainCharPrefab() {
            
            UnitEntityData unit = Game.Instance.Player.MainCharacter.Value;
            GameObject obj = unit.View.gameObject;
            UnityEngine.Object newPrefab = UnityEditor.PrefabUtility.CreateEmptyPrefab("Assets/Here.prefab");
            newPrefab = UnityEditor.PrefabUtility.ReplacePrefab(obj, newPrefab);
            /*
            foreach(GameObject obj in SceneManager.GetActiveScene().GetRootGameObjects()) {
                logger.Log(obj.name);
            }*/

        }
    }
}