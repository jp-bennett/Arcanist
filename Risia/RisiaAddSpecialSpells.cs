using ArcaneTide.Arcanist;
using ArcaneTide.Utils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcaneTide.Risia {
    static class RisiaAddSpecialSpells {
        static internal LibraryScriptableObject library => Main.library;
        static public List<BlueprintAbility> specialSpells = new List<BlueprintAbility>();
        static public List<int> specialSpellLevels = new List<int>();

        static private BlueprintFeature addSpecialSpellFeat;
        static private bool prepared = false;
        static private void FormatAndStoreSpecialSpell(BlueprintAbility spell, int level) {
            var comps = spell.GetComponents<SpellListComponent>();
            if (comps != null) {
                foreach (var comp in comps) {
                    spell.RemoveComponent(comp);
                }
            }
            if(spell.GetComponent<UniqueSpellComponent>() == null) {
                spell.AddComponent(Helpers.Create<UniqueSpellComponent>());
            }
            specialSpells.Add(spell);
            specialSpellLevels.Add(level);
        }
        static public void LoadSpecialSpells() {
            var shieldSwiftOriginal = library.Get<BlueprintAbility>("3c1b92a0a3ce0754a889fb0d7b2c23a4");
            FormatAndStoreSpecialSpell(shieldSwiftOriginal, 1);

            var mirror = library.Get<BlueprintAbility>("3e4ab69ada402d145a5e0ad3ad4b8564");
            var mirrorSwift = library.CopyAndAdd<BlueprintAbility>(mirror, "RisiaMirrorSwift", OtherUtils.GetMd5("Risia.MirrorImageSwift"));
            mirrorSwift.ActionType = UnitCommand.CommandType.Swift;
            FormatAndStoreSpecialSpell(mirrorSwift, 2);

            var stoneSkinFree = library.CopyAndAdd<BlueprintAbility>(
                "c66e86905f7606c4eaa5c774f0357b2b",
                "RisiaStoneskinFree",
                OtherUtils.GetMd5("Risia.StoneskinFree")
                );
            var stoneSkinSwift = library.CopyAndAdd<BlueprintAbility>(
                "c66e86905f7606c4eaa5c774f0357b2b",
                "RisiaStoneskinSwift",
                OtherUtils.GetMd5("Risia.StoneskinSwift")
                );
            stoneSkinFree.ActionType = UnitCommand.CommandType.Free;
            stoneSkinSwift.ActionType = UnitCommand.CommandType.Swift;
            FormatAndStoreSpecialSpell(stoneSkinFree, 4);
            FormatAndStoreSpecialSpell(stoneSkinSwift, 4);

            var greaterInvisibilitySwift = library.CopyAndAdd<BlueprintAbility>(
                "ecaa0def35b38f949bd1976a6c9539e0",
                "RisiaGreaterInvisibilitySwift",
                OtherUtils.GetMd5("Risia.GreaterInvisibility.Swift")
                );
            greaterInvisibilitySwift.ActionType = UnitCommand.CommandType.Swift;
            FormatAndStoreSpecialSpell(greaterInvisibilitySwift, 4);
            prepared = true;
        }
        static public void AddSpellsToRisia(BlueprintUnit risia) {
            if (!prepared) LoadSpecialSpells();
            if (addSpecialSpellFeat != null) {
                if (!risia.AddFacts.Contains(addSpecialSpellFeat)) {
                    var tmplist = risia.AddFacts.ToList();
                    tmplist.Add(addSpecialSpellFeat);
                    risia.AddFacts = tmplist.ToArray();
                }
                return;
            }
            addSpecialSpellFeat = Helpers.CreateFeature("RisiaAddSpecialSpellFeat", "", "",
                OtherUtils.GetMd5("Risia.AddSpecialSpell.Feat"),
                IconSet.elvenmagic,
                FeatureGroup.None);
            addSpecialSpellFeat.SetName(Helpers.CreateString("Risia.AddSpecialSpell.Feat.Name"));
            addSpecialSpellFeat.SetDescription(Helpers.CreateString("Risia.AddSpecialSpell.Feat.Desc"));
            int len = specialSpellLevels.Count;
            for(int i = 0; i < len; i++) {
                addSpecialSpellFeat.AddComponent(Helpers.CreateAddKnownSpell(specialSpells[i], ArcanistClass.arcanist, specialSpellLevels[i]));
            }
            var tmplist1 = risia.AddFacts.ToList();
            tmplist1.Add(addSpecialSpellFeat);
            risia.AddFacts = tmplist1.ToArray();
        }
    }
}
