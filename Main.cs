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

namespace ArcaneTide {
    public class Main {
        internal static LibraryScriptableObject library;
        public static bool enabled;
        public static UnityModManager.ModEntry.ModLogger logger;
        public static string ModPath;
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
                //use sorcerer to temporarily simulate arcanist
                //use sorcerer to temporarily simulate arcanist
                //use sorcerer to temporarily simulate arcanist
                SafeLoad(Helpers.Load, "Helpers");
                SafeLoad(IconSet.Load, "Icons");
                SafeLoad(MetaFeats.Load, "MetaFeatSet");
                SafeLoad(ArcanistClass.Load, "Arcanist");
                //SafeLoad(TestSpawner.TestSpawner.Load, "TestSpawner");
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
            return true;
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
}