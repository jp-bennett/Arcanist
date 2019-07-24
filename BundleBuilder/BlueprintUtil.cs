using Kingmaker.Blueprints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KingmakerUnity
{
    public class BlueprintUtil
    {
        public static void SetAssetId(BlueprintScriptableObject blueprint, string assetId)
        {
            typeof(BlueprintScriptableObject).GetField("m_AssetGuid", Util.AllBindingFlags).SetValue(blueprint, assetId);
        }

        public static string GetAssetId(BlueprintScriptableObject blueprint)
        {
            return (string)typeof(BlueprintScriptableObject).GetField("m_AssetGuid", Util.AllBindingFlags).GetValue(blueprint);
        }
    }
}
