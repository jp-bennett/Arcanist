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
            spellbook.SpellsPerDay = wizard.Spellbook.SpellsPerDay;
            spellbook.SpellsKnown = sorcerer.Spellbook.SpellsKnown;
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
            entries.Add(Helpers.LevelEntry(1, CreateReservoir()));
            progression.LevelEntries = entries.ToArray<LevelEntry>();


            progression.UIDeterminatorsGroup = new BlueprintFeatureBase[] {
                arcanistCantrip,
                arcanistProfiency, rayCalcFeat, touchCalcFeat, Caster9, detectMagic
            };
            arcanist.Progression = progression;
            arcanist.RegisterClass();
        }

        static internal BlueprintSpellsTable CreateArcanistPerDay() {
            if (library.BlueprintsByAssetId.ContainsKey("7de5c1dbbbc57d9dea0f7280a229d6db")) {
                return library.Get<BlueprintSpellsTable>("7de5c1dbbbc57d9dea0f7280a229d6db");
            }
            BlueprintSpellsTable perday = Helpers.Create<BlueprintSpellsTable>();
            perday.Levels = new SpellsLevelEntry[21];
            perday.Levels[0].Count = new int[] { 0, 0 };
            for(int i = 1; i <= 20; i++) {
                int maxTier = (i < 4) ? 1 : Math.Min(9, i / 2); // max spell tier on level i
                perday.Levels[i].Count = new int[maxTier + 1];
                perday.Levels[i].Count[0] = 0;
                for(int j = 1; j <= 9; j++) {
                    perday.Levels[i].Count[j] = (j == 1) ? Math.Min(4, i + 1) : Math.Min(4, i - j * 2 + 2);
                }
            }
            library.AddAsset(perday, "7de5c1dbbbc57d9dea0f7280a229d6db");//MD5-32[ArcanistClass.SpellsPerDayTable]
            return perday;
        }
        static internal BlueprintFeature CreateReservoir() {
            if (library.BlueprintsByAssetId.ContainsKey("46c10437728d31d5b7611eb34f6cb011")) {
                // has created arcane reservoir already.
                return library.Get<BlueprintFeature>("46c10437728d31d5b7611eb34f6cb011");
            }
            Sprite icon_AR = library.Get<BlueprintFeature>("55edf82380a1c8540af6c6037d34f322").Icon;//use icon of Elven Magic(elf race feature)
            LocalizedString reservoir_name = Helpers.CreateString("ArcanistClass.ArcaneReservoir.Name");
            LocalizedString reservoir_desc = Helpers.CreateString("ArcanistClass.ArcaneReservoir.Desc");

            BlueprintAbilityResource reservoir_resource = Helpers.CreateAbilityResource("ArcanistArcaneReservoirResource",
                "", "",
                "b0fb97baca84839a03906195bbc06a1b",//MD5-32[ArcanistClass.ArcaneReservoir.Resource]
                icon_AR);
            reservoir_resource.LocalizedName = reservoir_name;
            reservoir_resource.LocalizedDescription = reservoir_desc;

            BlueprintFeature reservoir = Helpers.CreateFeature("ArcanistClassArcaneReservoir",
                "", "", "46c10437728d31d5b7611eb34f6cb011",//MD5-32[ArcanistClass.ArcaneReservoir]
                icon_AR,
                FeatureGroup.None,
                Helpers.Create<AddAbilityResources>(a => a.Resource = reservoir_resource));
            reservoir.SetName(reservoir_name);
            reservoir.SetDescription(reservoir_desc);
            return reservoir;
        }
    }
}
