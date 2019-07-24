using Kingmaker.Blueprints;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace KingmakerUnity
{
    class LogObjectHierarchy
    {
        [MenuItem("BlueprintUtil/Bundles/Log Object Heiarchy")]
        static void DoLogObject()
        {
            var asset = AssetDatabase.LoadAssetAtPath<BlueprintScriptableObject>("Assets/Races/Dhampir/Abilities/BlurDhampir.asset");
            using (var sw = new StreamWriter("objTree.txt"))
            {
                LogObject(asset, sw);
            }
        }
        public static void LogObject(UnityEngine.Object asset, StreamWriter sw, HashSet<object> seen = null, int depth = 0)
        {
            if (seen == null) seen = new HashSet<object>();
            var serializedObject = new SerializedObject(asset);
            var prop = serializedObject.GetIterator();
            prop.Next(true);
            string prefix = new string(' ', depth * 2);
            sw.WriteLine($"{prefix}{{");
            while (prop.NextVisible(false)) //Skip Object Hide Flags
            {
                var member = serializedObject.FindProperty(prop.name);
                if (member.propertyType == SerializedPropertyType.String)
                {
                    var obj = member.objectReferenceValue;
                    sw.WriteLine($"{prefix}  {prop.name} - {member.propertyType} - {GetTypeString(obj)}");
                }
                else if (member.isArray)
                {
                    sw.WriteLine($"{prefix}  [");
                    for (int i = 0; i < member.arraySize; i++)
                    {
                        var _prop = member.arraySize > 0 ? member.GetArrayElementAtIndex(0) : null;
                        var obj = _prop.objectReferenceValue;
                        sw.WriteLine($"{prefix}    {_prop.name} - {_prop.propertyType} - {GetTypeString(obj)}");
                    }
                    sw.WriteLine($"{prefix}  ]");
                }
                else if (member.propertyType == SerializedPropertyType.Generic && member.objectReferenceValue != null)
                {
                    var obj = member.objectReferenceValue;
                    sw.WriteLine($"{prefix}  {prop.name} - {member.propertyType} - {GetTypeString(obj)}");
                }
                else
                {
                    var obj = member.objectReferenceValue;
                    sw.WriteLine($"{prefix}  {prop.name} - {member.propertyType} - {GetTypeString(obj)}");
                }
            }
            sw.WriteLine($"{prefix}}}");
        }
        public static string GetTypeString(object obj)
        {
            return obj == null ? "Null" : obj.GetType().ToString();
        }

    }
}
