using ArcaneTide.Arcanist;
using ArcaneTide.Utils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static UnityModManagerNet.UnityModManager.ModEntry;

namespace ArcaneTide.Risia {
    public class ContextActionDispelCertainMagic : ContextAction {
        public override string GetCaption() {
            return string.Empty;
        }

        public override void RunAction() {
            List<Buff> list = base.Target.Unit.Buffs.Enumerable.ToList<Buff>();
            Buff buff = list.Find((Buff a) => a.Blueprint == buffBlue);

            UnitEntityData unit = base.Target.Unit;

            RuleDispelMagic ruleDispelMagic = new RuleDispelMagic(base.Context.MaybeCaster, unit, buff, RuleDispelMagic.CheckType.CasterLevel, StatType.Charisma);
            bool success = base.Context.TriggerRule<RuleDispelMagic>(ruleDispelMagic).Success;
        }
        public BlueprintBuff buffBlue;
    }
    static class RisiaAddDispelAura {
        static internal LibraryScriptableObject library => Main.library;
        static internal ModLogger logger => Main.logger;
        static private BlueprintAbility dispelHoly, dispelUnholy;
        static private void CreateDispelAura() {
            if(dispelHoly != null || dispelUnholy != null) {
                return;
            }
            BlueprintAbility greaterDMArea = library.Get<BlueprintAbility>("b9be852b03568064b8d2275a6cf9e2de");
            BlueprintBuff holyAuraBuff = library.Get<BlueprintBuff>("a33bf327207a5904d9e38d6a80eb09e2");
            BlueprintBuff unholyAuraBuff = library.Get<BlueprintBuff>("9eda82a1f78558747a03c17e0e9a1a68");

            dispelHoly = library.CopyAndAdd<BlueprintAbility>(
                greaterDMArea,
                "RisiaDispelAreaHolyAura",
                OtherUtils.GetMd5("Risia.DispelMagicArea.HolyAura")
                );
            dispelUnholy = library.CopyAndAdd<BlueprintAbility>(
                greaterDMArea,
                "RisiaDispelAreaUnholyAura",
                OtherUtils.GetMd5("Risia.DispelMagicArea.UnholyAura")
                );
            dispelHoly.Parent = null;
            dispelUnholy.Parent = null;
            var compRunActionDMArea = greaterDMArea.GetComponent<AbilityEffectRunAction>();
            dispelHoly.RemoveComponent(compRunActionDMArea);
            dispelHoly.RemoveComponent(compRunActionDMArea);

            var compRunActionDMHoly = UnityEngine.Object.Instantiate<AbilityEffectRunAction>(compRunActionDMArea);
            var compRunActionDMUnholy = UnityEngine.Object.Instantiate<AbilityEffectRunAction>(compRunActionDMArea);
            compRunActionDMHoly.Actions = new ActionList {
                Actions = new GameAction[] {
                    Helpers.Create<ContextActionDispelCertainMagic>(a => a.buffBlue = holyAuraBuff)
                }
            };
            compRunActionDMUnholy.Actions = new ActionList {
                Actions = new GameAction[] {
                    Helpers.Create<ContextActionDispelCertainMagic>(a => a.buffBlue = unholyAuraBuff)
                }
            };

        }
        static public void AddToRisia(ref BlueprintUnit risia) {

            CreateDispelAura();
            HelpersNeu.Add<BlueprintUnitFact>(ref risia.AddFacts, dispelHoly);
            HelpersNeu.Add<BlueprintUnitFact>(ref risia.AddFacts, dispelUnholy);
        }
    }
    static class RisiaAddSpecialSpells {
        static internal LibraryScriptableObject library => Main.library;
        static public List<BlueprintAbility> specialSpells = new List<BlueprintAbility>();
        static public List<int> specialSpellLevels = new List<int>();

        static public BlueprintFeature addSpecialSpellListFeat;
        static public BlueprintProgression addSpecialSpellFeat;
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

            var stinkCloudHeighten4 = library.CopyAndAdd<BlueprintAbility>(
                "68a9e6d7256f1354289a39003a46d826",
                "StinkingCloud4",
                OtherUtils.GetMd5("Risia.StinkingCloud.Heighten4")
                );
            var cloakOfDream7 = library.CopyAndAdd<BlueprintAbility>(
                "7f71a70d822af94458dc1a235507e972",
                "CloakOfDream7",
                OtherUtils.GetMd5("Risia.CloakOfDream.Heighten7")
                );
            var tarPool7 = library.CopyAndAdd<BlueprintAbility>(
                "7d700cdf260d36e48bb7af3a8ca5031f",
                "TarPool7",
                OtherUtils.GetMd5("Risia.TarPool.Heighten7")
                );
            FormatAndStoreSpecialSpell(stinkCloudHeighten4, 4);
            FormatAndStoreSpecialSpell(cloakOfDream7, 7);
            FormatAndStoreSpecialSpell(tarPool7, 7);
            var mindFogQuicken = library.CopyAndAdd<BlueprintAbility>(
                "eabf94e4edc6e714cabd96aa69f8b207",
                "MindFogSwift",
                OtherUtils.GetMd5("Risia.MindFog.Swift")
                );
            FormatAndStoreSpecialSpell(mindFogQuicken, 5);
            prepared = true;
        }
        static public void CreateFeats() {
            if (!prepared) LoadSpecialSpells();
            if (addSpecialSpellFeat != null) {
                return;
            }
            BlueprintSpellList specialList = Helpers.Create<BlueprintSpellList>();
            specialList.name = "RisiaBossSpecialSpells";
            specialList.SpellsByLevel = new SpellLevelList[10];
            for (int i = 0; i < 10; i++) specialList.SpellsByLevel[i] = new SpellLevelList(i);
            int len = specialSpellLevels.Count;
            for(int i = 0; i < len; i++) {
                specialList.SpellsByLevel[specialSpellLevels[i]].Spells.Add(specialSpells[i]);
            }
            library.AddAsset(specialList, OtherUtils.GetMd5("Risia.Boss.AddSpecialSpellList.List"));
            addSpecialSpellListFeat = Helpers.CreateFeature("RisiaAddSpecialSpellListFeat", "", "",
                OtherUtils.GetMd5("Risia.Boss.AddSpecialSpellList.Feat"),
                IconSet.elvenmagic,
                FeatureGroup.None,
                Helpers.Create<AddSpecialSpellList>(a => {
                    a.CharacterClass = ArcanistClass.arcanist;
                    a.SpellList = specialList;
                }));

            /*addSpecialSpellFeat = Helpers.CreateFeature("RisiaAddSpecialSpellFeat", "", "",
                OtherUtils.GetMd5("Risia.AddSpecialSpell.Feat"),
                IconSet.elvenmagic,
                FeatureGroup.None);*/
            addSpecialSpellFeat = Helpers.CreateProgression("RisiaAddSpecialSpellProgression", "", "",
                OtherUtils.GetMd5("Risia.AddSpecialSpell.Progression.Feat"),
                null,
                FeatureGroup.None);
            addSpecialSpellFeat.IsClassFeature = false;
            addSpecialSpellFeat.Classes = new BlueprintCharacterClass[] { ArcanistClass.arcanist };
            addSpecialSpellFeat.LevelEntries = new LevelEntry[20];
            for (int i = 0; i < 20; i++) {
                addSpecialSpellFeat.LevelEntries[i] = new LevelEntry {
                    Level = i+1
                };
            }
            BlueprintFeature[] subFeats = new BlueprintFeature[10];
            for (int i = 0; i < 10; i++) {
                subFeats[i] = Helpers.CreateFeature($"Risia.AddSpecialSpellFeat.Level{i}", "", "",
                    OtherUtils.GetMd5($"Risia.AddSpecialSpellFeat.Level{i}"),
                    null,
                    FeatureGroup.None);
            }
            for(int i = 0; i < len; i++) {
                subFeats[specialSpellLevels[i]].AddComponent(specialSpells[i].CreateAddKnownSpell(ArcanistClass.arcanist, specialSpellLevels[i]));
            }
            for(int i = 1; i <= 9; i++) {
                int firstArcanistLevel = (i == 1 ? 1 : i * 2)+1;
                addSpecialSpellFeat.LevelEntries[firstArcanistLevel - 1] = Helpers.LevelEntry(firstArcanistLevel, subFeats[i]);
            }       
            addSpecialSpellFeat.SetName(Helpers.CreateString("Risia.AddSpecialSpell.Feat.Name"));
            addSpecialSpellFeat.SetDescription(Helpers.CreateString("Risia.AddSpecialSpell.Feat.Desc"));
            
        }
    }
    //[Harmony12.HarmonyPatch(typeof(AddKnownSpell), "OnFactActivate")]
    class AddKnownSpellDebugPatch1 {
        static public void Postfix() {
            Main.logger.Log("Finish Activate!");
        }
    }
    //[Harmony12.HarmonyPatch(typeof(Spellbook), "AddKnown")]
    class SpellbookDeubgPatvh1 {
        static public void Prefix(int spellLevel, BlueprintAbility spell) {
            Main.logger.Log($"On add known name {spell}, spelllevel {spellLevel}");
        }
        static public void Postfix(int spellLevel, BlueprintAbility spell) {
            Main.logger.Log($"Finish add known name {spell}, spelllevel {spellLevel}");
        }
    }
}
