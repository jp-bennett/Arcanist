using ArcaneTide.Arcanist;
using ArcaneTide.Utils;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Controllers.Brain;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.Controllers.Brain.Blueprints.Considerations;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcaneTide.Risia {
    static public class RisiaAddLevels {
        static internal BlueprintCharacterClass arcanist => ArcanistClass.arcanist;
        static internal LibraryScriptableObject library => Main.library;
        
        static internal BlueprintAbility[] RisiaSpellKnownLevelupSelect, RisiaSpellKnownAfterwards, RisiaBossSpellKnownLevelupSelect, RisiaBossSpellKnownAfterwards;
        static internal BlueprintAbility[] RisiaSpellMemory, RisiaBossSpellMemory;
        static internal BlueprintFeature[] RisiaArcaneExploits, RisiaFeats;
        static internal List<BlueprintFeature> RisiaAddFacts, RisiaBossAddFacts;

        static public AddClassLevels compNeutral, compBoss;
        static public LearnSpells learnNeutral, learnBoss;
        static private BlueprintAbility getSpell(string Id) => library.Get<BlueprintAbility>(Id);
        static private BlueprintFeature getFeat(string Id) => library.Get<BlueprintFeature>(Id);
        static private BlueprintFeatureSelection getSelection(string Id) => library.Get<BlueprintFeatureSelection>(Id);
        static void Prepare() {
            // from Tier 1 to Tier 3
            string[] RisiaSpellKnownLevelUpIds = new string[] {
 				"4783c3709a74a794dbe7c8e7e0b1b038",
				
				"91da41b9793a4624797921f221db653c",
				"8e7cfa5f213a90549aadd18f8f6f4664",
				"c60969e7f264e6d4b84a1499fdcf9039",
				"95851f6e85fe87d4190675db0419d112",
				
				"ef768022b0785eb43a18969903c537c4",
				"4ac47ddb9fa1eaf43a1b6809980cfbd2",
				"433b1faf4d02cc34abb0ade5ceda47c4",
				"4e0e9aba6447d514f88eff1464cc4763",
				"bb7ecad2d3d2c8247a38f44855c99061",

				"f001c73999fb5a543a199f890108d936",
				
				"46fd02ad56c35224c9c91c88cd457791",// level 2 spell start : blindness
				"14ec7a4e52e90fa47a4c8d63c69fd5c1",
				"b7731c2b4fa1c9844a092329177be4c3",
				"29ccc62632178d344ad0be0865fd3113",
				
				"92681f181b507b34ea87018e8f7a528a",//level 3 spell start
				"903092f6488f9ce45a80943923576ab3",
				"2d81362af43aeac4387a3d4fced489c3",
				"c7104f7526c4c524f91474614054547e"
				
            };
            string[] RisiaSpellMemoryIds = new string[] {
                "95851f6e85fe87d4190675db0419d112",
                "ef768022b0785eb43a18969903c537c4",
                "2c38da66e5a599347ac95b3294acbe00",
                "91da41b9793a4624797921f221db653c",
                "4ac47ddb9fa1eaf43a1b6809980cfbd2",
                "3e4ab69ada402d145a5e0ad3ad4b8564",
                "fd4d9fd7f87575d47aafe2a64a6e2d8d",
                "ce7dad2b25acf85429b6c9550787b2d9",
                "f492622e473d34747806bdb39356eb89",
                "c7104f7526c4c524f91474614054547e"
            };
            string[] RisiaSpellKnownAfterwardsIds = new string[] {
                "bd81a3931aa285a4f9844585b5d97e51",//level 1 spell start
                "2c38da66e5a599347ac95b3294acbe00",
                "88367310478c10b47903463c5d0152b0",
                "9e1ad5d6f87d19e4d8883d63a6e35568",

                "7a5b5bf845779a941a67251539545762",
                "ae4d3ad6a8fda1542acf2e9bbc13d113",
                "ce7dad2b25acf85429b6c9550787b2d9",
                "fd4d9fd7f87575d47aafe2a64a6e2d8d",
                "89940cde01689fb46946b2f8cd7b66b7",
                "3e4ab69ada402d145a5e0ad3ad4b8564",
                "cdb106d53c65bbc4086183d54c3b97c7",
                "5181c2ed0190fc34b8a1162783af5bf4",
                "1724061e89c667045a6891179ee2e8e7",
                "134cb6d492269aa4f8662700ef57449f",

                "96c9d98b6a9a7c249b6c4572e4977157",
                "1a045f845778dc54db1c2be33a8c3c0a",
                "f492622e473d34747806bdb39356eb89",
                "68a9e6d7256f1354289a39003a46d826"

            };

            // Arcanist Memory : 9 5 5 4 4 4 3 3 3 3 
            string[] RisiaBossSpellMemoryIds = new string[] {
                // tier 1
                "95851f6e85fe87d4190675db0419d112",
                "3c1b92a0a3ce0754a889fb0d7b2c23a4", //shield swift
                "2c38da66e5a599347ac95b3294acbe00",
                "91da41b9793a4624797921f221db653c",
                "4ac47ddb9fa1eaf43a1b6809980cfbd2",
                // tier 2
                OtherUtils.GetMd5("Risia.MirrorImageSwift"), //mirror swift
                "fd4d9fd7f87575d47aafe2a64a6e2d8d", //hideous laughter
                "ce7dad2b25acf85429b6c9550787b2d9", //glitter dust
                "cdb106d53c65bbc4086183d54c3b97c7", //scorching ray
                "134cb6d492269aa4f8662700ef57449f", //web
                // tier 3
                "f492622e473d34747806bdb39356eb89", //slow
                "c7104f7526c4c524f91474614054547e", //hold person
                "903092f6488f9ce45a80943923576ab3", //displacement
                "92681f181b507b34ea87018e8f7a528a", //dispel magic
                // tier 4
                "cf6c901fb7acc904e85c63b342e9c949", //confusion
                OtherUtils.GetMd5("Risia.GreaterInvisibility.Swift"), //invisibility greater swift
                OtherUtils.GetMd5("Risia.StinkingCloud.Heighten4"), //stink cloud heighten 4
                // tier 5
                "237427308e48c3341b3d532b9d3a001f", //shadow evocation
                "eabf94e4edc6e714cabd96aa69f8b207", //mind fog
                "a34921035f2a6714e9be5ca76c5e34b5", //vampiric shadow shield
                "3105d6e9febdc3f41a08d2b7dda1fe74", //baleful polymorph
                // tier 6
                "dbf99b00cd35d0a4491c6cc9e771b487", //acid fog
                "f0f761b808dc4b149b08eaf44b99f633", //dispel magic greater
                "4aa7942c3e62a164387a73184bca3fc1", //disintegrate
                // tier 7
                OtherUtils.GetMd5("Risia.CloakOfDream.Heighten7"), //claok of dream 7
                OtherUtils.GetMd5("Risia.TarPool.Heighten7"), // tar pool 7
                "ab167fd8203c1314bac6568932f1752f", //summon monster 7
                // tier 8
                "0e67fa8f011662c43934d486acc50253", //prediction failure
                "f958ef62eea5050418fb92dfa944c631", //power word stun
                "d3ac756a229830243a72e84f3ab050d0", //summon monster 8
                // tier 9
                "41cf93453b027b94886901dbfc680cb9", //overwhelming presence
                "08ccad78cac525040919d51963f9ac39", //fiery body
                "870af83be6572594d84d276d7fc583e0", //weird
            };
            List<BlueprintAbility> tmpList = new List<BlueprintAbility>();
            foreach(var id in RisiaSpellKnownLevelUpIds) {
                tmpList.Add(getSpell(id));
            }
            RisiaSpellKnownLevelupSelect = tmpList.ToArray();

            tmpList = new List<BlueprintAbility>();
            foreach(var id in RisiaSpellMemoryIds) {
                tmpList.Add(getSpell(id));
            }
            RisiaSpellMemory = tmpList.ToArray();

            tmpList = new List<BlueprintAbility>();
            foreach(var id in RisiaSpellKnownAfterwardsIds) {
                tmpList.Add(getSpell(id));
            }
            RisiaSpellKnownAfterwards = tmpList.ToArray();

            // Risia Boss
            tmpList = new List<BlueprintAbility>();
            for(int i = 1; i <= 9; i++) {
                // get 7 tier 1 spell at level 1, 2 spells every level afterwards
                int levelSpellCnt = (i == 1) ? 11 : ((i == 9) ? 6 : 4);
                SpellLevelList lst = ArcanistClass.arcanist.Spellbook.SpellList.SpellsByLevel[i];
                for(int j = 0; j < levelSpellCnt; j++) {
                    tmpList.Add(lst.Spells[j]);
                }
            }
            RisiaBossSpellKnownLevelupSelect = tmpList.ToArray();

            tmpList = new List<BlueprintAbility>();
            foreach(var id in RisiaBossSpellMemoryIds) {
                tmpList.Add(getSpell(id));
            }
            RisiaBossSpellMemory = tmpList.ToArray();

            tmpList = new List<BlueprintAbility>();
            for (int i = 1; i <= 9; i++) {
                SpellLevelList lst = ArcanistClass.arcanist.Spellbook.SpellList.SpellsByLevel[i];
                foreach (var spell in lst.Spells) {
                    if (!RisiaBossSpellKnownLevelupSelect.Contains(spell)) {
                        tmpList.Add(spell);
                    }
                }     
            }
            RisiaBossSpellKnownAfterwards = tmpList.ToArray<BlueprintAbility>();

            RisiaFeats = new BlueprintFeature[] {
                getFeat("16fa59cc9a72a6043b566b49184f53fe"),// spell focus illusion               
                getFeat("797f25d709f559546b29e7bcb181cc74"),// improved initiative
                getFeat("16fa59cc9a72a6043b566b49184f53fe"),// spell focus enchantment
                getFeat("06964d468fde1dc4aa71a92ea04d930d"),// combat casting
                getFeat("5b04b45b228461c43bad768eb0f7c7bf"),// spell focus greater illusion
                getFeat("90e54424d682d104ab36436bd527af09"),// weapon finesse
                getFeat("ee7dc126939e4d9438357fbd5980d459"),// spell penetration
                getFeat("5b04b45b228461c43bad768eb0f7c7bf"),// spell focus greater enchantment
                getFeat("d09b20029e9abfe4480b356c92095623"),// toughness
                getFeat("79042cb55f030614ea29956177977c52") // great fortitude
            };
            RisiaArcaneExploits = new BlueprintFeature[] {
                ArcaneExploits.fastStudy,
                ArcaneExploits.potentMagic,
                ArcaneExploits.metaMixing,
                ArcaneExploits.dimensionalSlide,
                ArcaneExploits.metaKnowledge,
                ArcaneExploits.greaterMetaKnowledgeTmp,
                ArcaneExploits.familiar,
                ArcaneExploits.swiftConsume,
                ArcaneExploits.energyShield,
                ArcaneExploits.SR
            };
        }
        static public void LoadNeutral() {
            RisiaAddFacts = new List<BlueprintFeature>();

            compNeutral = Helpers.Create<AddClassLevels>();
            compNeutral.Archetypes = new BlueprintArchetype[] { };
            compNeutral.CharacterClass = arcanist;
            compNeutral.Levels = 7;
            compNeutral.LevelsStat = StatType.Intelligence;
            compNeutral.Skills = new StatType[] {
                StatType.SkillKnowledgeArcana,
                StatType.SkillKnowledgeWorld,
                StatType.SkillPersuasion,
                StatType.SkillStealth,
                StatType.SkillPerception,
                StatType.SkillUseMagicDevice,
                StatType.SkillMobility
            };
            compNeutral.SelectSpells = RisiaSpellKnownLevelupSelect;
            compNeutral.MemorizeSpells = RisiaSpellMemory;

            BlueprintFeatureSelection basicFeatSelection = getSelection("247a4068296e8be42890143f451b4b45");
            BlueprintParametrizedFeature spellFocus = getFeat("16fa59cc9a72a6043b566b49184f53fe") as BlueprintParametrizedFeature;

            List<SelectionEntry> selectionEntryList = new List<SelectionEntry>();
            selectionEntryList.Add(new SelectionEntry {
                Selection = basicFeatSelection,//basic feat selection
                Features = RisiaFeats
            });
            selectionEntryList.Add(new SelectionEntry {
                Selection = getSelection("247a4068296e8be42890143f451b4b45"),
                IsParametrizedFeature = true,
                ParametrizedFeature = spellFocus,
                ParamSpellSchool = SpellSchool.Illusion
            });
            selectionEntryList.Add(new SelectionEntry {
                Selection = getSelection("247a4068296e8be42890143f451b4b45"),
                IsParametrizedFeature = true,
                ParametrizedFeature = spellFocus,
                ParamSpellSchool = SpellSchool.Enchantment
            });
            selectionEntryList.Add(new SelectionEntry {
                Selection = ArcaneExploits.exploitSelection,
                Features = RisiaArcaneExploits
            });
            compNeutral.Selections = selectionEntryList.ToArray();
            learnNeutral = Helpers.Create<LearnSpells>(a => {
                a.Spells = RisiaSpellKnownAfterwards;
                a.CharacterClass = ArcanistClass.arcanist;
            });
            var RisiaLearnSpellFeat = Helpers.CreateFeature("RisiaLearnSpellFeat", "", "",
                OtherUtils.GetMd5("Risia.LearnSpellFeat"),
                null,
                FeatureGroup.None,
                learnNeutral);
            RisiaAddFacts.Add(RisiaLearnSpellFeat);

        }
        static public void LoadBoss() {
            RisiaBossAddFacts = new List<BlueprintFeature>();

            compBoss = Helpers.Create<AddClassLevels>();
            compBoss.Archetypes = new BlueprintArchetype[] { };
            compBoss.CharacterClass = arcanist;
            compBoss.Levels = 20;
            compBoss.LevelsStat = StatType.Intelligence;
            compBoss.Skills = new StatType[] {
                StatType.SkillKnowledgeArcana,
                StatType.SkillKnowledgeWorld,
                StatType.SkillPersuasion,
                StatType.SkillStealth,
                StatType.SkillPerception,
                StatType.SkillUseMagicDevice,
                StatType.SkillMobility
            };
            compBoss.SelectSpells = RisiaBossSpellKnownLevelupSelect;
            compBoss.MemorizeSpells = RisiaBossSpellMemory;

            BlueprintFeatureSelection basicFeatSelection = getSelection("247a4068296e8be42890143f451b4b45");
            BlueprintParametrizedFeature spellFocus = getFeat("16fa59cc9a72a6043b566b49184f53fe") as BlueprintParametrizedFeature;
            BlueprintParametrizedFeature spellFocusGreater = getFeat("5b04b45b228461c43bad768eb0f7c7bf") as BlueprintParametrizedFeature;
            List<SelectionEntry> selectionEntryList = new List<SelectionEntry>();
            selectionEntryList.Add(new SelectionEntry {
                Selection = basicFeatSelection,//basic feat selection
                Features = RisiaFeats
            });
            selectionEntryList.Add(new SelectionEntry {
                Selection = basicFeatSelection,
                IsParametrizedFeature = true,
                ParametrizedFeature = spellFocus,
                ParamSpellSchool = SpellSchool.Illusion
            });
            selectionEntryList.Add(new SelectionEntry {
                Selection = basicFeatSelection,
                IsParametrizedFeature = true,
                ParametrizedFeature = spellFocus,
                ParamSpellSchool = SpellSchool.Enchantment
            });
            selectionEntryList.Add(new SelectionEntry {
                Selection = basicFeatSelection,
                IsParametrizedFeature = true,
                ParametrizedFeature = spellFocusGreater,
                ParamSpellSchool = SpellSchool.Illusion
            });
            selectionEntryList.Add(new SelectionEntry {
                Selection = basicFeatSelection,
                IsParametrizedFeature = true,
                ParametrizedFeature = spellFocusGreater,
                ParamSpellSchool = SpellSchool.Enchantment
            });

            //Exploit Selections
            selectionEntryList.Add(new SelectionEntry {
                Selection = ArcaneExploits.exploitSelection,
                Features = RisiaArcaneExploits
            });
            selectionEntryList.Add(new SelectionEntry {
                Selection = MetamagicKnownledge.exploit,
                Features = new BlueprintFeature[] {getFeat("ef7ece7bb5bb66a41b256976b27f424e") }//quicken spell
            });
            string GreaterMetaKnowledgeHeightenId = OtherUtils.GetMd5($"ArcanistClassExploitTempGreaterMetamagicKnowledge{Metamagic.Heighten}SubFeat");
            selectionEntryList.Add(new SelectionEntry {
                Selection = GreaterMetamagicKnowledge.exploitTemp,
                Features = new BlueprintFeature[] {getFeat(GreaterMetaKnowledgeHeightenId) }//heighten spell
            });
            selectionEntryList.Add(new SelectionEntry {
                Selection = Familiar.exploit,
                Features = new BlueprintFeature[] { getFeat("1cb0b559ca2e31e4d9dc65de012fa82f") } //cat familiar
            });
            compBoss.Selections = selectionEntryList.ToArray();
            learnBoss = Helpers.Create<LearnSpells>(a => {
                a.Spells = RisiaBossSpellKnownAfterwards;
                a.CharacterClass = ArcanistClass.arcanist;
            });
            var RisiaBossLearnSpellFeat = Helpers.CreateFeature("RisiaBossLearnSpellFeat", "", "",
                OtherUtils.GetMd5("Risia.Boss.LearnSpellFeat"),
                null,
                FeatureGroup.None,
                learnBoss);
            var RisiaBossApplyBuffOnBattle = Helpers.CreateFeature("RisiaBossApplyBuffOnBattle", "", "",
                OtherUtils.GetMd5("Risia.Boss.ApplyBuffOnBattle.Feat"),
                null,
                FeatureGroup.None,
                Helpers.Create<AddBuffOnCombatStart>(a => a.Feature = library.Get<BlueprintBuff>("8ab51b96c1310c34199238549d601160")),//stoneskin cl15
                Helpers.Create<AddBuffOnCombatStart>(a => a.Feature = library.Get<BlueprintBuff>("51ebd62ee464b1446bb01fa1e214942f")));//delay poison;
            RisiaBossAddFacts.Add(RisiaBossLearnSpellFeat);
            RisiaBossAddFacts.Add(RisiaBossApplyBuffOnBattle);

        }
        static public void Load() {
            if (ArcanistClass.arcanist == null) {
                throw new Exception("Wrong: Risia should be loaded after Arcanist is loaded");
            }
            Prepare();
            LoadNeutral();
            LoadBoss();
            BlueprintUnit risia_companion = library.Get<BlueprintUnit>("c2dc52c5fec84bc2a74e2cb34fdb566b");
            BlueprintUnit risia_neutral = library.Get<BlueprintUnit>("d87f8e86724f46e798821d60f9d31eaf");
            BlueprintUnit risia_boss = library.Get<BlueprintUnit>("95fb27a5b8ae40099bd727ea93de5b9b");
            
            

            BlueprintUnit octavia = library.Get<BlueprintUnit>("f9161aa0b3f519c47acbce01f53ee217");
            Main.logger.Log($"{(octavia.Faction == risia_companion.Faction ? "Yes, faction are the same" : "Nope!")}");
        }
    }

    
}
