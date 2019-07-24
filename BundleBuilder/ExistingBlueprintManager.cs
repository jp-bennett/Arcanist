using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KingmakerUnity
{
    public class ExistingBlueprintManager
    {
        static readonly string BlueprintInfoPath = "Assets/Existing/BlueprintInfo/Blueprints.txt";
        static List<ExistingBlueprint> ExistingBlueprints;
        static Dictionary<string, ExistingBlueprint> AssetIdLookup;
        static void LoadBlueprints()
        {
            if (!File.Exists(BlueprintInfoPath))
            {
                throw new Exception($"Couldn't find file {BlueprintInfoPath}");
            }
            ExistingBlueprints = new List<ExistingBlueprint>();
            AssetIdLookup = new Dictionary<string, ExistingBlueprint>();
            var lines = File.ReadAllLines(BlueprintInfoPath);
            for (var i = 1; i < lines.Length; i++)
            {
                var parts = lines[i].Split('\t');
                var existing = new ExistingBlueprint()
                {
                    Name = parts[0],
                    AssetId = parts[1],
                    Type = parts[2]
                };
                if (AssetIdLookup.ContainsKey(existing.AssetId))
                {
                    //TODO: FIx blueprint info to not nave duplicates
                    //throw new Exception($"Existing Blueprint Info contains duplicate AssetId {existing.AssetId}");
                }
                AssetIdLookup[existing.AssetId] = existing;
                ExistingBlueprints.Add(existing);
            }
        }
        public static ExistingBlueprint GetInfoByAssetId(string assetId)
        {
            if (AssetIdLookup == null) LoadBlueprints();
            ExistingBlueprint result = null;
            AssetIdLookup.TryGetValue(assetId, out result);
            return result;
        }
        public class ExistingBlueprint
        {
            public string Name;
            public string AssetId;
            public string Type;
        }
    }
}
