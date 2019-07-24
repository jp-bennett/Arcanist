using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace ArcaneTide.Utils {
    public class GlobalConstants {
        public List<string> assetBundleFiles { get; set; }
        public string MainAssetBundleFilename { get; set; }
        public Dictionary<string, string> GUIDs { get; set; }
        public List<string> LocalizationFiles { get; set; }
        public Dictionary<string, string> ImgFiles { get; set; }
        public string[] GetImgFilepaths(string actorName) {
            List<string> tmp = new List<string>();
            foreach (string sizeId in new string[] { "L", "M", "S" }) {
                if (this.ImgFiles.ContainsKey(actorName + sizeId)) {
                    tmp.Add(this.ImgFiles[actorName + sizeId]);
                }
                else return null;
            }
            return tmp.ToArray<string>();
        }
    }
}
