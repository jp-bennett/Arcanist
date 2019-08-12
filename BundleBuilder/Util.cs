using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

namespace KingmakerUnity
{
    public class Util
    {
        public static BindingFlags AllBindingFlags = BindingFlags.Instance
            | BindingFlags.Static
            | BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.GetField
            | BindingFlags.SetField
            | BindingFlags.GetProperty
            | BindingFlags.SetProperty;
        public static void PrepareExportDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                DeleteDirectory(path);
            }
            Thread.Sleep(1);
            Directory.CreateDirectory(path);
        }
        public static void DeleteDirectory(string path)
        {
            foreach (string directory in Directory.GetDirectories(path))
            {
                Thread.Sleep(1);
                DeleteDir(directory);
            }
            DeleteDir(path);
        }

        private static void DeleteDir(string dir)
        {
            try
            {
                Thread.Sleep(1);
                Directory.Delete(dir, true);
            }
            catch (IOException)
            {
                DeleteDir(dir);
            }
            catch (UnauthorizedAccessException)
            {
                DeleteDir(dir);
            }
        }
        public static void EditorSafeDestroy(UnityEngine.Object obj)
        {
#if UNITY_EDITOR
            UnityEngine.Object.DestroyImmediate(obj);
#else
            UnityEngine.Object.Destroy(obj);
#endif
        }
        public static string GetRelativePath(string filespec, string folder)
        {
            Uri pathUri = new Uri(filespec);
            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }
            Uri folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
