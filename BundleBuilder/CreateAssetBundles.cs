using UnityEditor;
using System.IO;
using System.Linq;
using System;
using UnityEngine;
using System.Collections.Generic;
using Kingmaker.Blueprints;


namespace KingmakerUnity
{
    public class CreateAssetBundles
    {
        [MenuItem("BlueprintUtil/Bundles/Build AssetBundles")]
        static void BuildAllAssetBundles()
        {
            try
            {
                EditorUtility.DisplayProgressBar("Building Asset Bundles", "Building Asset Bundles", 0);
                BuildAssetBundles();
                Debug.Log($"Built {AssetDatabase.GetAllAssetBundleNames().Length} Asset Bundles");
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
            }
            EditorUtility.ClearProgressBar();
        }
        [MenuItem("BlueprintUtil/Bundles/Install AssetBundles")]
        static void InstallAllAssetBundles()
        {
            try
            {
                EditorUtility.DisplayProgressBar("Building Asset Bundles", "Building Asset Bundles", 0);
                string assetBundleDirectory = "AssetBundles";
                var gameDir = @"G:\SteamLibrary\steamapps\common\Pathfinder Kingmaker";
                var modDir = Path.Combine(gameDir, "Mods/ArcaneTide");
                var bundleDir = $"{modDir}/bundles";

                Util.PrepareExportDirectory(bundleDir);
                Util.PrepareExportDirectory(assetBundleDirectory);
                BuildAssetBundles();
                EditorUtility.DisplayProgressBar("Installing Asset Bundles", "Installing Asset Bundles", 0.5f);
                var bundles = Directory.GetFiles(assetBundleDirectory, "*", SearchOption.AllDirectories)
                    .Where(file => Path.GetFileName(file) != "common_assets" && Path.GetFileName(file) != "shaders");
                foreach (var file in bundles)
                {
                    var filename = Path.GetFileName(file);
                    File.Copy(file, Path.Combine(bundleDir, filename), true);
                }
                UnityEngine.Debug.Log($"Installed {bundles.Count()} asset bundles");
                Directory.CreateDirectory(Path.Combine(modDir, "Localization"));
                File.Copy("Assets/Localization/enGB.json", $"{modDir}/Localization/enGB.json", true);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            EditorUtility.ClearProgressBar();
        }
        static void BuildAssetBundles()
        {
            string assetBundleDirectory = "AssetBundles";
            Directory.CreateDirectory(assetBundleDirectory);
            BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
        }
            static List<Tuple<UnityEngine.Object, string>> GetChildren(SerializedProperty prop, string path = null)
        {
            var result = new List<Tuple<UnityEngine.Object, string>>();
            if (prop.propertyType == SerializedPropertyType.Generic && prop.isArray)
            {
            } else if(prop.propertyType == SerializedPropertyType.ObjectReference && prop.objectReferenceValue != null)
            {
                result.Add(new Tuple<UnityEngine.Object, string>(prop.objectReferenceValue, prop.name));
            } else if(prop.propertyType == SerializedPropertyType.Generic)
            {
                var obj = prop.objectReferenceValue;
            }
            return result;
        }
        static List<Tuple<UnityEngine.Object, string>> GetChildren(UnityEngine.Object obj)
        {
            var result = new List<Tuple<UnityEngine.Object, string>>();
            var serializedObject = new SerializedObject(obj);
            var prop = serializedObject.GetIterator();
            prop.Next(true);
            while (prop.NextVisible(true)) //Skip Object Hide Flags
            {
                if (prop.name == "m_Script") continue;
                if(prop.propertyType == SerializedPropertyType.ObjectReference
                    && !prop.isArray
                    && prop.objectReferenceValue != null)
                {
                    result.Add(new Tuple<UnityEngine.Object, string>(prop.objectReferenceValue, prop.propertyPath));
                }
            }
            return result;
        }
        [MenuItem("BlueprintUtil/Bundles/Update Blueprint Manifest")]
        static void UpdateBlueprintManifest()
        {
            var names = AssetDatabase.GetAllAssetBundleNames();
            foreach (string name in names)
            {
                var blueprintManifest = AssetDatabase.GetAssetPathsFromAssetBundle(name)
                    .Select(path => AssetDatabase.LoadAssetAtPath<BlueprintManifest>(path))
                    .FirstOrDefault(r => r != null);
                if (blueprintManifest == null) continue;
                var scriptableObjects = AssetDatabase.GetAssetPathsFromAssetBundle(name)
                    .Select(path => AssetDatabase.LoadAssetAtPath<ScriptableObject>(path))
                    .Where(so => so != null && so.GetType() != typeof(BlueprintManifest));
                var blueprints = new HashSet<BlueprintScriptableObject>(
                    scriptableObjects
                    .Where(s => s is BlueprintScriptableObject)
                    .Cast<BlueprintScriptableObject>());
                var seen = new HashSet<UnityEngine.Object>();
                var queue = new Queue<UnityEngine.Object>(scriptableObjects);
                var result = new List<Tuple<UnityEngine.Object, string>>();
                while (queue.Count > 0)
                {
                    var obj = queue.Dequeue();
                    seen.Add(obj);
                    var children = GetChildren(obj);
                    foreach(var t in children)
                    {
                        if (!typeof(BlueprintScriptableObject).IsAssignableFrom(t.Item1.GetType())) continue;
                        var blueprint = (BlueprintScriptableObject)t.Item1;
                        if (blueprint.name.StartsWith("Existing."))
                        {
                            result.Add(new Tuple<UnityEngine.Object, string>(obj, t.Item2));
                        }
                    }
                    foreach (var child in children)
                    {
                        if (typeof(BlueprintScriptableObject).IsAssignableFrom(child.Item1.GetType()) &&
                            !blueprints.Contains(child.Item1)){
                            var blueprint = (BlueprintScriptableObject)child.Item1;
                            throw new Exception($"On Processing {(obj as ScriptableObject).name}: Blueprint {blueprint.name} missing from asset bundle {name}");
                        }
                        if (!seen.Contains(child.Item1))
                        {
                            queue.Enqueue(child.Item1);
                        }
                    }
                }
                blueprintManifest.SetExistingBlueprints(result);
                EditorUtility.SetDirty(blueprintManifest);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}