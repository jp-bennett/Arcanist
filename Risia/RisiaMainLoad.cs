using ArcaneTide.Utils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic.FactLogic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArcaneTide.Risia {
    public class RisiaMainLoad {
        static public BlueprintUnlockableFlag flagIsRisiaSpawned, flagIsRisiaBossSpawned;
        static internal LibraryScriptableObject library => Main.library;
        static internal GlobalConstants consts => Main.constsManager;
        static private bool loaded = false;
        static public PortraitData CreatePortraitData(string actorName) {
            string[] portraitPathsRela = consts.GetImgFilepaths(actorName);
            if (portraitPathsRela == null) return null;
            string[] portraitPathsAbsolute = new string[3];
            int[] w = new int[] { 692, 330, 185 };
            int[] h = new int[] { 1024, 432, 242 };
            Sprite[] portraits = new Sprite[3];
            for(int i = 0; i < 3; i++) {
                portraitPathsAbsolute[i] = Path.Combine(Main.ModPath, portraitPathsRela[i]);
                byte[] data = File.ReadAllBytes(portraitPathsAbsolute[i]);
                if(data == null) {
                    throw new Exception($"On reading the {i + 1}th portrait (1L2M3S), read null");
                }
                Texture2D tex = new Texture2D(w[i], h[i]);
                tex.LoadImage(data);
                portraits[i] = Sprite.Create(tex, new Rect(0f, 0f, w[i] * 1.0f, h[i] * 1.0f), new Vector2(0f, 0f));
            }
            return new PortraitData(actorName, portraits[2], portraits[1], portraits[0]);
        }

        static public void LoadOnLibraryLoaded() {
            if (loaded) return;
            flagIsRisiaSpawned = Helpers.Create<BlueprintUnlockableFlag>();
            flagIsRisiaBossSpawned = Helpers.Create<BlueprintUnlockableFlag>();
            library.AddAsset(flagIsRisiaSpawned, "942339abe7b76dfb00324d433a0a9342");
            library.AddAsset(flagIsRisiaBossSpawned, "942339abe7b76dfb00324d433a0a9343");
            flagIsRisiaSpawned.Lock();
            flagIsRisiaSpawned.name = "flagIsRisiaSpawned";
            flagIsRisiaBossSpawned.Lock();
            flagIsRisiaBossSpawned.name = "flagIsRisiaBossSpawned";

            FastGetter portrait_Getter = Helpers.CreateFieldGetter<BlueprintUnit>("m_Portrait");
            PortraitData risia_portraitData = CreatePortraitData("Risia");
            BlueprintUnit risia_companion = library.Get<BlueprintUnit>("c2dc52c5fec84bc2a74e2cb34fdb566b");
            BlueprintUnit risia_neutral = library.Get<BlueprintUnit>("d87f8e86724f46e798821d60f9d31eaf");
            BlueprintUnit risia_boss = library.Get<BlueprintUnit>("95fb27a5b8ae40099bd727ea93de5b9b");
            foreach(var unit in new BlueprintUnit[] { risia_companion, risia_neutral, risia_boss }) {
                BlueprintPortrait portraitBlue = portrait_Getter(unit) as BlueprintPortrait;
                portraitBlue.Data = risia_portraitData;
            }
            RisiaAddSpecialSpells.LoadSpecialSpells();
            RisiaAddLevels.Load();
            RisiaAddSpecialSpells.CreateFeats();
            RisiaAddBrain.Load();

            List<BlueprintFeature> risiaNeutralAddFacts = new List<BlueprintFeature>();
            risiaNeutralAddFacts.AddRange(RisiaAddLevels.RisiaAddFacts);
            risiaNeutralAddFacts.Add(RisiaAddSpecialSpells.addSpecialSpellListFeat);
            risiaNeutralAddFacts.Add(RisiaAddSpecialSpells.addSpecialSpellFeat);
            List<BlueprintFeature> risiaBossAddFacts = new List<BlueprintFeature>();
            risiaBossAddFacts.AddRange(RisiaAddLevels.RisiaBossAddFacts);
            risiaBossAddFacts.Add(RisiaAddSpecialSpells.addSpecialSpellListFeat);
            risiaBossAddFacts.Add(RisiaAddSpecialSpells.addSpecialSpellFeat);

            var risiaFeatureList = Helpers.CreateFeature("RisiaFeatureList", "", "",
                OtherUtils.GetMd5("Risia.FeatureList"),
                IconSet.elvenmagic,
                FeatureGroup.None,
                //RisiaAddLevels.compNeutral,
                Helpers.Create<AddFacts>(a => a.Facts = risiaNeutralAddFacts.ToArray()));
            var risiaBossFeatureList = Helpers.CreateFeature("RisiaBossFeatureList", "", "",
                OtherUtils.GetMd5("Risia.Boss.FeatureList"),
                IconSet.elvenmagic,
                FeatureGroup.None,
                //RisiaAddLevels.compBoss,
                Helpers.Create<AddFacts>(a => a.Facts = risiaBossAddFacts.ToArray()));

            risia_neutral.AddComponent(RisiaAddLevels.compNeutral);
            risia_companion.AddComponent(RisiaAddLevels.compNeutral);
            risia_boss.AddComponent(RisiaAddLevels.compBoss);
            var tmpList = risia_neutral.AddFacts.ToList();
            tmpList.Add(risiaFeatureList);
            risia_neutral.AddFacts = tmpList.ToArray();
            var tmpList2 = risia_companion.AddFacts.ToList();
            tmpList2.Add(risiaFeatureList);
            risia_companion.AddFacts = tmpList2.ToArray();
            var tmpList3 = risia_boss.AddFacts.ToList();
            tmpList3.Add(risiaBossFeatureList);
            risia_boss.AddFacts = tmpList3.ToArray();
            loaded = true;
        }
    }
}
