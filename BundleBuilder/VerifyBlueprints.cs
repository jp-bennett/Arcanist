using Kingmaker.Blueprints;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace KingmakerUnity
{
    class VerifyBlueprints
    {
        public static IEnumerable<SerializedProperty> WalkAssetBundles()
        {
            var seen = new HashSet<UnityEngine.Object>();
            var queue = new Queue<UnityEngine.Object>();
            var names = AssetDatabase.GetAllAssetBundleNames();
            foreach (string name in names)
            {
                foreach (var path in AssetDatabase.GetAssetPathsFromAssetBundle(name))
                {
                    var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                    queue.Enqueue(obj);
                }
            }
            while (queue.Count > 0)
            {
                var obj = queue.Dequeue();
                if (seen.Contains(obj)) continue;
                seen.Add(obj);
                var serializedObject = new SerializedObject(obj);
                var prop = serializedObject.GetIterator();
                prop.Next(true);
                while (prop.NextVisible(true))
                {
                    if (prop.propertyType == SerializedPropertyType.ObjectReference
                    && prop.objectReferenceValue != null
                    && !seen.Contains(prop.objectReferenceValue))
                    {
                        queue.Enqueue(prop.objectReferenceValue);
                    }
                    yield return prop;
                }
            }
        }
        public static Dictionary<string, string> GetExistingBlueprintLookup()
        {
            var existingBlueprints = new Dictionary<string, string>();
            var lines = File.ReadAllLines("Assets/Existing/BlueprintInfo/Blueprints.txt");
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                var parts = line.Split('\t');


            }
            return existingBlueprints;
        }
        public static void VerifyExistingBlueprint(SerializedProperty prop)
        {
            if (prop.propertyType != SerializedPropertyType.ObjectReference) return; ;
            var blueprint = prop.objectReferenceValue as BlueprintScriptableObject;
            if (blueprint == null) return;
            var path = AssetDatabase.GetAssetPath(blueprint);
            var assetId = BlueprintUtil.GetAssetId(blueprint);
            var info = ExistingBlueprintManager.GetInfoByAssetId(assetId);
            if (blueprint.name.StartsWith("Existing."))
            {
                if (info == null)
                {
                    Debug.LogError($"Existing Blueprint has invalid assetId {assetId}, {path}");
                    return;
                }
                if (Path.GetFileName(Path.GetDirectoryName(path)) != "Existing")
                {
                    Debug.LogWarning($"Existing blueprint {path} should be placed in a folder named 'Existing'");
                }
                var blueprintName = blueprint.name.Split('.')[0];
                if (blueprintName != info.Name)
                {
                    Debug.LogWarning($"Existing blueprint {path} asset name {blueprintName} differs from real name {info.Name}");
                }
                var expectedType = Type.GetType($"{info.Type}, Assembly-CSharp");
                if (blueprint.GetType() != expectedType)
                {
                    Debug.LogWarning($"Existing blueprint {path} asset type {blueprint.GetType()} differs from real type {expectedType}");
                }
                var parent = prop.serializedObject.targetObject;
#pragma warning disable CS0252 // Possible unintended reference comparison; left hand side needs cast
                if (PathUtil.GetValueAtPath(parent, prop.propertyPath) != blueprint)
#pragma warning restore CS0252 // Possible unintended reference comparison; left hand side needs cast
                {
                    Debug.LogError($"Could not find existing blueprint by path for {parent.name} at path {prop.propertyPath}");
                }
                var container = PathUtil.GetContainerAtPath(parent, prop.propertyPath);
                if (container == null)
                {
                    Debug.LogError($"Could not find container for existing blueprint by path for {parent.name} at {prop.propertyPath}");
                }
                var fieldPath = PathUtil.SplitPath(prop.propertyPath).Last();
#pragma warning disable CS0252 // Possible unintended reference comparison; left hand side needs cast
                if (PathUtil.GetValueAtPath(container, fieldPath) != blueprint)
#pragma warning restore CS0252 // Possible unintended reference comparison; left hand side needs cast
                {
                    Debug.LogError($"Could not find existing blueprint by path for {parent.name} at path {fieldPath}");
                }
            } else if (info != null)
            {
                Debug.LogError($"New blueprint had duplicate assetId with existing blueprint {assetId} {AssetDatabase.GetAssetPath(blueprint)}");
            }
        }
        public static void DoVerifyBlueprints()
        {
            var blueprints = new Dictionary<string, BlueprintScriptableObject> (); 
            foreach (var prop in WalkAssetBundles())
            {
                if (prop.propertyType != SerializedPropertyType.ObjectReference) continue;
                var blueprint = prop.objectReferenceValue as BlueprintScriptableObject;
                if (blueprint == null) continue;
                if(blueprint.name.StartsWith("Existing.")) continue;
                var assetId = BlueprintUtil.GetAssetId(blueprint);
                if (blueprints.ContainsKey(assetId) && blueprints[assetId] != blueprint)
                {
                    var firstPath = AssetDatabase.GetAssetPath(blueprint);
                    var secondPath = AssetDatabase.GetAssetPath(blueprints[assetId]);
                    Debug.LogError($"Blueprints with duplicate asset ids {assetId}, {firstPath}, {secondPath}");
                }
                blueprints[assetId] = blueprint;
            }

        }
        [MenuItem("BlueprintUtil/Bundles/VerifyBlueprints")]
        public static void DoVerifyExistingBlueprints()
        {
            
            foreach(var prop in WalkAssetBundles())
            {
                VerifyExistingBlueprint(prop);
            }
            DoVerifyBlueprints();
        }
    }
}
