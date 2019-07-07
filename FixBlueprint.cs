using Kingmaker.Blueprints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityModManagerNet.UnityModManager.ModEntry;

namespace ArcaneTide {
    static class FixBlueprint {
        static public LibraryScriptableObject library => Main.library;
        static internal ModLogger logger => Main.logger;
        static public void Fix<T> (ref T x) where T : BlueprintScriptableObject{
            if(x as T) {
                FastGetter assetIdGetter = Helpers.CreateFieldGetter<T>("m_AssetGuid");
                var guid = assetIdGetter(x) as string;
                if (guid != null) {
                    x = library.Get<T>(guid);
                }
                else throw new Exception("x is not a BlueprintScriptableObject");
            }
        }
        static public void Fix(AssetBundle bundle) {
            var blueprintManifestList = bundle.LoadAllAssets<BlueprintManifest>();
            if (blueprintManifestList.Length == 0) {
                logger.Log("No race info found");
            }
            else {
                logger.Log($"Found {blueprintManifestList.Length} raceinfo");
            }
            foreach (var blueprintManifest in blueprintManifestList) {
                if (blueprintManifest == null) {
                    throw new Exception("RaceInfo is null");
                }
                if (blueprintManifest.Parents == null || blueprintManifest.Fields == null) {
                    if (blueprintManifest.Parents == null && blueprintManifest.Fields == null) {
                        throw new Exception("BlueprintManifest.Parents and BlueprintManifest.Fields is null");
                    }
                    else if (blueprintManifest.Parents == null) {
                        throw new Exception("BlueprintManifest.Parents is null");
                    }
                    else {
                        throw new Exception("BlueprintManifest.Fields is null");
                    }
                }
                logger.Log($"Iterating race info");
                for (int i = 0; i < blueprintManifest.Parents.Length; i++) {
                    var parent = blueprintManifest.Parents[i];
                    var path = blueprintManifest.Fields[i];
                    var fakeBlueprint = (BlueprintScriptableObject)PathUtil.GetValueAtPath(parent, path);
                    var realBlueprint = ResourcesLibrary.TryGetBlueprint<BlueprintScriptableObject>(fakeBlueprint.AssetGuid);
                    logger.Log($"Fixing Blueprint {parent.name} - {blueprintManifest.Fields[i]} - {fakeBlueprint.name} - {realBlueprint.name}");
                    PathUtil.SetValueAtPath(parent, path, realBlueprint);
                }
            }
        }
    }
}
