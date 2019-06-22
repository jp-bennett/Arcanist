using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Validation;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility;
using Newtonsoft.Json;
using ArcaneTide;
using UnityEngine;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using ArcaneTide.Components;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics;
using ArcaneTide.Utils;
using Kingmaker.ElementsSystem;

namespace ArcaneTide.Arcanist {
    static class ArcanistClass {
        static LibraryScriptableObject library => Main.library;
        internal static BlueprintCharacterClass wizard, sorcerer;
        public static BlueprintCharacterClass arcanist;
        static public void Load() {
            wizard = library.Get<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");
            sorcerer = library.Get<BlueprintCharacterClass>("b3a505fb61437dc4097f43c3f8f9a4cf");
            arcanist = Helpers.Create<BlueprintCharacterClass>();
            library.AddAsset(arcanist, "c94a10ada8f7b4ac49c5b39a9f909b1a");//MD5-32[ArcanistClass]

            var HPD6 = wizard.HitDie;
            var BABLow = wizard.BaseAttackBonus;
            var SaveHigh = wizard.WillSave;
            var SaveLow = wizard.FortitudeSave;

            arcanist.name = "ArcanistClass";
            arcanist.LocalizedName = Helpers.CreateString("ArcanistClass.ClassName");
            arcanist.LocalizedDescription = Helpers.CreateString("ArcanistClass.ClassDesc");
            arcanist.m_Icon = sorcerer.m_Icon;
            arcanist.SkillPoints = wizard.SkillPoints;
            arcanist.HitDie = HPD6;
            arcanist.BaseAttackBonus = BABLow;
            arcanist.FortitudeSave = SaveLow;
            arcanist.ReflexSave = SaveLow;
            arcanist.WillSave = SaveHigh;
            arcanist.ClassSkills = new StatType[] {
                StatType.SkillKnowledgeArcana,
                StatType.SkillKnowledgeWorld,
                StatType.SkillLoreNature,
                StatType.SkillLoreReligion,
                StatType.SkillUseMagicDevice,
                StatType.SkillPersuasion
            };

            var spellbook = Helpers.Create<BlueprintSpellbook>();
            library.AddAsset(spellbook, "c05d9a49349f602d114a69dac8143408");//MD5-32[ArcanistClass.Spellbook]
            spellbook.name = "ArcanistClassSpellbook";
            spellbook.Name = arcanist.LocalizedName;
            spellbook.SpellsPerDay = CreateArcanistPerDay();
            spellbook.SpellsKnown = CreateArcanistMemorize();
            spellbook.SpellList = wizard.Spellbook.SpellList;
            spellbook.Spontaneous = true;
            spellbook.IsArcane = true;
            spellbook.AllSpellsKnown = false;
            spellbook.CanCopyScrolls = true;
            spellbook.CastingAttribute = StatType.Intelligence;
            spellbook.CharacterClass = arcanist;
            spellbook.CantripsType = CantripsType.Cantrips;
            spellbook.SpellsPerLevel = 2;
            arcanist.Spellbook = spellbook;

            arcanist.IsDivineCaster = false;
            arcanist.IsArcaneCaster = true;

            arcanist.StartingGold = 411;
            arcanist.StartingItems = wizard.StartingItems;
            arcanist.PrimaryColor = wizard.PrimaryColor;
            arcanist.SecondaryColor = wizard.SecondaryColor;
            arcanist.EquipmentEntities = sorcerer.EquipmentEntities;
            arcanist.MaleEquipmentEntities = sorcerer.MaleEquipmentEntities;
            arcanist.FemaleEquipmentEntities = sorcerer.FemaleEquipmentEntities;

            arcanist.RecommendedAttributes = new StatType[] { StatType.Intelligence, StatType.Charisma };
            arcanist.NotRecommendedAttributes = new StatType[] { StatType.Strength };

            arcanist.ComponentsArray = sorcerer.ComponentsArray;

            var progression = Helpers.CreateProgression("ArcanistClassProgression", "奥能师", "奥能师-描述",
                "9559b14457eddc15fffe6aae4f63783e",//MD5-32[ArcanistClass.Progression]
                arcanist.Icon, FeatureGroup.None);
            progression.SetName(Helpers.CreateString("ArcanistClass.ProgressionName"));
            progression.SetDescription(Helpers.CreateString("ArcanistClass.ProgressionDesc"));
            progression.Classes = new BlueprintCharacterClass[] { arcanist };
            var arcanistCantrip = library.CopyAndAdd<BlueprintFeature>(
                "44d19b62d00179e4bad7afae7684f2e2",// wizard cantrip
                "ArcanistClassCantrip",
                "c39de0ed2f661b0bf95300a1fcc23f02");//MD5-32[ArcanistClass.Cantrip]
            arcanistCantrip.SetName(Helpers.CreateString("ArcanistClass.Cantrip.Name"));
            arcanistCantrip.SetDescription(Helpers.CreateString("ArcanistClass.Cantrip.Desc"));
            arcanistCantrip.SetComponents(arcanistCantrip.ComponentsArray.Select(c => {
                var bind = c as BindAbilitiesToClass;
                if (bind == null) {
                    var bind2 = c as LearnSpells;
                    if (bind2 == null) return c;
                    bind2 = UnityEngine.Object.Instantiate(bind2);
                    bind2.CharacterClass = arcanist;
                    return bind2;
                }
                bind = UnityEngine.Object.Instantiate(bind);
                bind.CharacterClass = arcanist;
                bind.Stat = StatType.Intelligence;
                return bind;
            }));
            UnityModManagerNet.UnityModManager.Logger.Log($"{((arcanistCantrip.ComponentsArray[2] as BindAbilitiesToClass).CharacterClass == arcanist ? "Rua yes" : "Rua no")}");
            var arcanistProfiency = library.CopyAndAdd<BlueprintFeature>(
                "25c97697236ccf2479d0c6a4185eae7f",//sorcerer profiency
                "ArcanistClassProfiencies",
                "ee849748c015853463ea315725f998d3");//MD5-32[ArcanistClass.Profiencies]
            arcanistProfiency.SetName(Helpers.CreateString("ArcanistClass.Profiencies.Name"));
            arcanistProfiency.SetDescription(Helpers.CreateString("ArcanistClass.Profiencies.Desc"));

            var rayCalcFeat = library.Get<BlueprintFeature>("d3e6275cfa6e7a04b9213b7b292a011c");
            var touchCalcFeat = library.Get<BlueprintFeature>("62ef1cdb90f1d654d996556669caf7fa");
            var Caster9 = library.Get<BlueprintFeature>("9fc9813f569e2e5448ddc435abf774b3");
            var detectMagic = library.Get<BlueprintFeature>("ee0b69e90bac14446a4cf9a050f87f2e");

            var entries = new List<LevelEntry>();
            entries.Add(Helpers.LevelEntry(1,
                arcanistCantrip, 
                arcanistProfiency, rayCalcFeat, touchCalcFeat, Caster9, detectMagic));
            entries.Add(Helpers.LevelEntry(1, ArcaneReservoir.CreateReservoir()));
            entries.Add(Helpers.LevelEntry(1, ArcaneReservoir.CreateAddDCCLFeature()));
            progression.LevelEntries = entries.ToArray<LevelEntry>();


            progression.UIDeterminatorsGroup = new BlueprintFeatureBase[] {
                arcanistCantrip,
                arcanistProfiency, rayCalcFeat, touchCalcFeat, Caster9, detectMagic
            };
            arcanist.Progression = progression;
            arcanist.RegisterClass();
        }
        static internal BlueprintSpellsTable CreateArcanistMemorize() {
            if (library.BlueprintsByAssetId.ContainsKey("70a540f04461bcd29da08e9a25b5c566")) {
                return library.Get<BlueprintSpellsTable>("70a540f04461bcd29da08e9a25b5c566");
            }
            BlueprintSpellsTable memorize = Helpers.Create<BlueprintSpellsTable>();
            const int MAX_KNOWN = 255;
            memorize.Levels = new SpellsLevelEntry[41];
            memorize.Levels[0] = new SpellsLevelEntry {
                Count = new int[] { 0, 0 }
            };
            for (int i = 1; i <= 20; i++) {
                int maxTier = (i < 4) ? 1 : Math.Min(9, i / 2);
                memorize.Levels[i] = new SpellsLevelEntry {
                    Count = new int[maxTier + 1]
                };
                memorize.Levels[i].Count[0] = 0;
                for (int j = 1; j <= maxTier; j++) {
                    //Arcanist is a spontaneous caster but also get two new spells every time she levels up, just like a wizard.
                    //On choosing new spells of each level up(say level i -> level i+1), the game will check whether \
                    //number of chosen spells of each spell tier j is greater than known(i+1,j)-known(i,j)
                    //if j<known(i+1,j)-known(i,j) then the spell choosing part is not finished.
                    //So an easy way to fix this is to set an arcanist's known spell of level 1-20 to 1.
                    //(On level 1 you can only choose tier 1 spell so set it to 1 won't cause a bug on character creation(level 0->1).
                    //And data of level 21-40 of known spell table contains number of arcanist's every day memorized spells.
                    memorize.Levels[i].Count[j] = 1;
                }

                memorize.Levels[i + 20] = sorcerer.Spellbook.SpellsKnown.Levels[i];
            }
            library.AddAsset(memorize, "70a540f04461bcd29da08e9a25b5c566");//MD5-32[ArcanistClass.SpellMemorizeTable]
            return memorize;
        }
        static internal BlueprintSpellsTable CreateArcanistPerDay() {
            if (library.BlueprintsByAssetId.ContainsKey("7de5c1dbbbc57d9dea0f7280a229d6db")) {
                return library.Get<BlueprintSpellsTable>("7de5c1dbbbc57d9dea0f7280a229d6db");
            }
            //UnityModManagerNet.UnityModManager.Logger.Log("RUA RUA RUA 000");
            BlueprintSpellsTable perday = Helpers.Create<BlueprintSpellsTable>();
            perday.Levels = new SpellsLevelEntry[21];
            //UnityModManagerNet.UnityModManager.Logger.Log("RUA RUA RUA 001");
            perday.Levels[0] = new SpellsLevelEntry {
                Count = new int[] { 0, 0 }
            };
            //UnityModManagerNet.UnityModManager.Logger.Log("RUA RUA RUA 002");
            for (int i = 1; i <= 20; i++) {
                int maxTier = (i < 4) ? 1 : Math.Min(9, i / 2); // max spell tier on level i
                perday.Levels[i] = new SpellsLevelEntry {
                    Count = new int[maxTier + 1]
                };
                perday.Levels[i].Count[0] = 0;
                for(int j = 1; j <= maxTier; j++) {
                    UnityModManagerNet.UnityModManager.Logger.Log($"{i}, {j}");
                    perday.Levels[i].Count[j] = (j == 1) ? Math.Min(4, i + 1) : Math.Min(4, i - j * 2 + 2);
                }
            }
            library.AddAsset(perday, "7de5c1dbbbc57d9dea0f7280a229d6db");//MD5-32[ArcanistClass.SpellsPerDayTable]
            return perday;
        }
        
    }

    static class ArcaneReservoir {
        static internal BlueprintCharacterClass arcanist => ArcanistClass.arcanist;
        static internal LibraryScriptableObject library => Main.library;

        static public BlueprintBuff AR_AddCLBuff, AR_AddDCBuff;
        static public BlueprintAbility AR_AddCLAbl, AR_AddDCAbl;
        static public BlueprintAbilityResource resource;

        static internal BlueprintFeature CreateReservoir() {
            if (library.BlueprintsByAssetId.ContainsKey("46c10437728d31d5b7611eb34f6cb011")) {
                // has created arcane reservoir already.
                return library.Get<BlueprintFeature>("46c10437728d31d5b7611eb34f6cb011");
            }
            Sprite icon_AR = library.Get<BlueprintFeature>("55edf82380a1c8540af6c6037d34f322").Icon;//use icon of Elven Magic(elf race feature)
            LocalizedString reservoir_name = Helpers.CreateString("ArcanistClass.Reservoir.Name");
            LocalizedString reservoir_desc = Helpers.CreateString("ArcanistClass.Reservoir.Desc");

            BlueprintAbilityResource reservoir_resource = Helpers.CreateAbilityResource("ArcanistArcaneReservoirResource",
                "", "",
                "b0fb97baca84839a03906195bbc06a1b",//MD5-32[ArcanistClass.ArcaneReservoir.Resource]
                icon_AR);
            reservoir_resource.LocalizedName = reservoir_name;
            reservoir_resource.LocalizedDescription = reservoir_desc;
            resource = reservoir_resource;

            BlueprintFeature reservoir = Helpers.CreateFeature("ArcanistClassArcaneReservoir",
                "", "", "46c10437728d31d5b7611eb34f6cb011",//MD5-32[ArcanistClass.ArcaneReservoir]
                icon_AR,
                FeatureGroup.None,
                Helpers.Create<AddAbilityResources>(a => a.Resource = reservoir_resource));
            reservoir.SetName(reservoir_name);
            reservoir.SetDescription(reservoir_desc);
            return reservoir;
        }

        static internal BlueprintAbility CreateAddCLAbl() {
            AddCLDCOnNextSpell component = Helpers.Create<AddCLDCOnNextSpell>();
            component.valueCL = 1;
            component.valueDC = 0;
            Sprite icon = Helpers.GetIcon("f001c73999fb5a543a199f890108d936");//vanish
            BlueprintBuff buff = Helpers.CreateBuff("ArcanistClassReservoirAddCLBuff", "", "",
                "798949f3385934097022de98a28a4e3e",//MD5-32[ArcanistClass.Reservoir.AddCLBuff]
                icon,
                null,
                component);
            buff.SetName(Helpers.CreateString("ArcanistClass.Reservoir.AddCLBuff.Name"));
            buff.SetDescription(Helpers.CreateString("ArcanistClass.Reservoir.AddCLBuff.Desc"));

            var ablResourceLogicComp = Helpers.Create<AbilityResourceLogic>();
            ablResourceLogicComp.RequiredResource = resource;
            ablResourceLogicComp.IsSpendResource = true;
            ablResourceLogicComp.CostIsCustom = false;
            ablResourceLogicComp.Amount = 1;

            var ablEffectComp = Helpers.Create<AbilityEffectRunAction>();
            var ablEffectCompAction = Helpers.Create<ContextActionApplyBuff>();
            ablEffectCompAction.Buff = buff;
            ablEffectCompAction.Permanent = false;
            ablEffectCompAction.DurationValue = PresetDurations.threeRounds;
            ablEffectCompAction.IsFromSpell = false;
            ablEffectCompAction.IsNotDispelable = false;
            ablEffectCompAction.ToCaster = false;
            ablEffectCompAction.AsChild = true;
            ablEffectComp.Actions = new ActionList {
                Actions = new GameAction[] { null, ablEffectCompAction }
            };

            BlueprintAbility abl = Helpers.CreateAbility("ArcanistClassReservoirAddCLAbl", "", "",
                "d7947d5dd590717d19b9537f70dd7dd7",//MD5-32[ArcanistClass.Reservoir.AddCLAbl]
                icon,
                AbilityType.Supernatural,
                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free,
                AbilityRange.Personal,
                "",
                "",
                ablResourceLogicComp,
                ablEffectComp);
            abl.SetName(Helpers.CreateString("ArcanistClass.Reservoir.AddCLAbl.Name"));
            abl.SetDescription(Helpers.CreateString("ArcanistClass.Reservoir.AddCLAbl.Desc"));
            abl.LocalizedDuration = Helpers.CreateString("ArcaneTide.ThreeRounds");
            abl.LocalizedSavingThrow = Helpers.CreateString("ArcaneTide.WillSave.CanAbandon");
            return abl;
        }

        static internal BlueprintAbility CreateAddDCAbl() {
            AddCLDCOnNextSpell component = Helpers.Create<AddCLDCOnNextSpell>();
            component.valueCL = 0;
            component.valueDC = 1;
            Sprite icon = Helpers.GetIcon("f001c73999fb5a543a199f890108d936");//vanish
            BlueprintBuff buff = Helpers.CreateBuff("ArcanistClassReservoirAddDCBuff", "", "",
                "85143f03db31082e776c095f3518a505",//MD5-32[ArcanistClass.Reservoir.AddDCBuff]
                icon,
                null,
                component);
            buff.SetName(Helpers.CreateString("ArcanistClass.Reservoir.AddDCBuff.Name"));
            buff.SetDescription(Helpers.CreateString("ArcanistClass.Reservoir.AddDCBuff.Desc"));

            var ablResourceLogicComp = Helpers.Create<AbilityResourceLogic>();
            ablResourceLogicComp.RequiredResource = resource;
            ablResourceLogicComp.IsSpendResource = true;
            ablResourceLogicComp.CostIsCustom = false;
            ablResourceLogicComp.Amount = 1;

            var ablEffectComp = Helpers.Create<AbilityEffectRunAction>();
            var ablEffectCompAction = Helpers.Create<ContextActionApplyBuff>();
            ablEffectCompAction.Buff = buff;
            ablEffectCompAction.Permanent = false;
            ablEffectCompAction.DurationValue = PresetDurations.threeRounds;
            ablEffectCompAction.IsFromSpell = false;
            ablEffectCompAction.IsNotDispelable = false;
            ablEffectCompAction.ToCaster = false;
            ablEffectCompAction.AsChild = true;
            ablEffectComp.Actions = new ActionList {
                Actions = new GameAction[] { null, ablEffectCompAction }
            };

            BlueprintAbility abl = Helpers.CreateAbility("ArcanistClassReservoirAddDCAbl", "", "",
                "a572d2412acba81b25cb7ca77c129010",//MD5-32[ArcanistClass.Reservoir.AddDCAbl]
                icon,
                AbilityType.Supernatural,
                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free,
                AbilityRange.Personal,
                "",
                "",
                ablResourceLogicComp,
                ablEffectComp);
            abl.SetName(Helpers.CreateString("ArcanistClass.Reservoir.AddDCAbl.Name"));
            abl.SetDescription(Helpers.CreateString("ArcanistClass.Reservoir.AddDCAbl.Desc"));
            abl.LocalizedDuration = Helpers.CreateString("ArcaneTide.ThreeRounds");
            abl.LocalizedSavingThrow = Helpers.CreateString("ArcaneTide.WillSave.CanAbandon");
            return abl;
        }

        static public BlueprintFeature CreateAddDCCLFeature() {
            if (library.BlueprintsByAssetId.ContainsKey("930d62a76ccde4a698d396b4bb932d6a")) {
                return library.Get<BlueprintFeature>("930d62a76ccde4a698d396b4bb932d6a");
            }
            var icon = Helpers.GetIcon("f001c73999fb5a543a199f890108d936");//vanish
            BlueprintFeature feat = Helpers.CreateFeature(
                "ArcanistClassReservoirAddDCCLFeat",
                "",
                "",
                "930d62a76ccde4a698d396b4bb932d6a",//MD5-32[ArcanistClass.Reservoir.AddDCCLFeat]
                icon,
                FeatureGroup.None,
                Helpers.Create<AddFacts>(a => a.Facts = new BlueprintUnitFact[] { CreateAddCLAbl(), CreateAddDCAbl() })
                );
            feat.SetName(Helpers.CreateString("ArcanistClass.Reservoir.AddDCCLFeat.Name"));
            feat.SetDescription(Helpers.CreateString("ArcanistClass.Reservoir.AddDCCLFeat.Desc"));
            return feat;
        }
    }
}
