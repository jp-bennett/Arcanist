using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace KingmakerUnity
{
    class VerifyMonoscripts
    {
        /* Verify that imported interface script are serializeable
         * Note: Scripts in a plugin namespace must inherit from UnityEngine.Object to be serialized
         * Fields cannot be new classes that do not inherit from UnityEngine.Object
         * TODO: Check if plugin System.Object classes can be used in generic classes
         * TODO: Check if new enums are allowed
         */ 
        [MenuItem("BlueprintUtil/Bundles/Verify Monoscripts")]
        static void DoVerifyMonoscripts()
        {
            var scripts = AssetDatabase.FindAssets("t:script");
            foreach(var guid in scripts)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.StartsWith("Assets/InterfaceScripts/"))
                {
                    var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                    var _class = script.GetClass();
                    if (!typeof(UnityEngine.Object).IsAssignableFrom(_class) && !_class.IsEnum)
                    {
                        //EditorUtility.DisplayDialog("Invalid Script", $"Class {_class.Namespace}.{_class.Name} at {path} doesn't inherit from UnityEngine.Object", "OK");
                        //return;
                        Debug.LogError($"Class {_class.Namespace}.{_class.Name} at {path} doesn't inherit from UnityEngine.Object");
                    }
                    
                }

            }
        }
    }
}
