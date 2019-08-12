using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ArcaneTide {
    public class ResourceList : ScriptableObject {

        public Dictionary<string, string> nameDict = new Dictionary<string, string>();
        public string GetResourceName(string assetId) {
            if (!nameDict.ContainsKey("resource_" + assetId)) {
                return null;
            }
            return nameDict["resource_" + assetId];
        }
        public void SetResourceName(string assetId, string name) {
            if (!nameDict.ContainsKey("resource_" + assetId)) {
                nameDict["resource_" + assetId] = name;
            }
        }
        public void Init() {
            nameDict = new Dictionary<string, string>();
        }
    }
    /*
    public class CreateResourceList : MonoBehaviour {
        [MenuItem("Assets/SnowyJune/Refresh Resource Dict")]
        public static void Refresh() {
            var obj = Selection.activeObject;
            if (obj is ResourceList) {
                ResourceList objDict = obj as ResourceList;
                string[] assetBundleNames = AssetDatabase.GetAllAssetBundleNames();
                objDict.Init();
                foreach (var bundle in assetBundleNames) {
                    if (bundle.StartsWith("resource_") && bundle.Length == 41) {
                        string resourceId = bundle.Substring(9, 32);
                        if (!System.Text.RegularExpressions.Regex.IsMatch(resourceId, @"([0-9a-f]){32}")) {
                            continue;
                        }
                        var paths = AssetDatabase.GetAssetPathsFromAssetBundle(bundle);
                        if (paths.Length != 1) {
                            throw new Exception("Resource bundles must contain 1 and only 1 asset");
                        }
                        string assetName = Path.GetFileNameWithoutExtension(paths[0]);
                        objDict.SetResourceName(resourceId, assetName);
                        Debug.Log($"Resource {resourceId} inserted into dict");
                    }
                }
            }
        }
        [MenuItem("Assets/Create/Create Blueprint/ResourceList Dictionary")]
        public static void CreateAsset() {
            ResourceList obj = ScriptableObject.CreateInstance<ResourceList>();
            var ProjectPath = EditorUtility.SaveFilePanelInProject("Choose Save Path", "ResourceListDict", "asset", "");
            AssetDatabase.CreateAsset(obj, ProjectPath);
            AssetDatabase.Refresh();
        }
    }
    */
}