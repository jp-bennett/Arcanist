using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Kingmaker.Blueprints;

namespace KingmakerUnity
{
    public class PathUtil
    {
        public static Type GetTypeAtPath(object containingObject, string path)
        {
            if (containingObject == null)
            {
                return null;
            }
            Type containingType = null;
            var index = path.LastIndexOf('.');
            var finalPart = path;
            if (index > 0)
            {
                var containingPath = path.Remove(index);
                finalPart = path.Substring(index + 1);
                if (containingPath.EndsWith(".Array"))
                {
                    index = containingPath.LastIndexOf('.');
                    containingPath = containingPath.Remove(index);
                    finalPart = $"Array.{finalPart}";
                }
                containingObject = GetValueAtPath(containingObject, containingPath);
                containingType = containingObject.GetType();
            }
            else
            {
                containingType = containingObject.GetType();
            }
            if (finalPart.StartsWith("Array."))
            {
                if (containingType.IsArray)
                {
                    var elementType = containingType.GetElementType();
                    return elementType;
                }
                else
                {
                    var genericArgument = containingType.GenericTypeArguments[0];
                    return genericArgument;
                }
            }
            else
            {
                var fieldInfo = GetField(containingType, finalPart);
                return fieldInfo.FieldType;
            }
        }
        public static Type GetElementTypeAtPath(object containingObject, string path)
        {
            var type = GetTypeAtPath(containingObject, path);
            if (type.HasElementType)
            {
                return type.GetElementType();
            }
            else if (type.GenericTypeArguments.Length > 0)
            {
                return type.GenericTypeArguments[0];
            }
            else
            {
                return type;
            }
        }
        public static FieldInfo GetField(Type type, string field)
        {
            var simple = type.GetField(field,
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance);
            if (simple != null) return simple;
            while (type != null)
            {
                var fieldInfo = type.GetField(field, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (fieldInfo != null) return fieldInfo;
                type = type.BaseType;
            }
            return null;
        }

        internal static void SetValueAtPath(object parent, string propPath, object value)
        {
            var parts = SplitPath(propPath);
            var container = GetContainerAtPath(parent, propPath);
            var lastPath = parts[parts.Count - 1];
            if (lastPath.StartsWith("Array"))
            {
                IList list = (IList)container;
                var match = Regex.Match(lastPath, "data\\[(\\d+)\\]");
                var index = int.Parse(match.Groups[1].Value);
                list[index] = value;
            } else
            {
                var field = GetField(container.GetType(), lastPath);
                field.SetValue(container, value);
            }
        }

        public static object GetValueAtPath(object obj, string path)
        {
            var parts = path.Split('.');
            for (int i = 0; i < parts.Length; i++)
            {
                var current = parts[i];
                if (current == "Array")
                {
                    IList list = (IList)obj;
                    var match = Regex.Match(parts[i + 1], "data\\[(\\d+)\\]");
                    var index = int.Parse(match.Groups[1].Value);
                    obj = list[index];
                    i++;
                }
                else
                {
                    var type = obj.GetType();
                    var fieldInfo = GetField(type, current);
                    obj = fieldInfo.GetValue(obj);
                }
            }
            return obj;
        }
        public static object GetContainerAtPath(object obj, string path)
        {
            var newPath = path;
            var index = path.LastIndexOf('.');
            if (index <= 0)
            {
                return obj;
            }
            newPath = newPath.Remove(index);
            if (newPath.EndsWith(".Array"))
            {
                newPath = newPath.Remove(newPath.LastIndexOf('.'));
            }
            return GetValueAtPath(obj, newPath);
        }
        /* Convert a split a unity property part into parts
         * eg:
         * "fieldName.Array.data[1].fieldName" ->
         * ["fieldName", Array.data[1]", "fieldName"]
         */
        public static List<string> SplitPath(string path)
        {
            var parts = path.Split('.');
            List<string> result = new List<string>();
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] == "Array")
                {
                    result.Add($"{parts[i]}.{parts[i + 1]}");
                    i++;
                }
                else
                {
                    result.Add(parts[i]);
                }
            }
            return result;
        }

    }
}
