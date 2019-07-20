using ArcaneTide.Arcanist;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcaneTide.Risia {
    static public class RisiaAddLevels {
        static internal BlueprintCharacterClass arcanist => ArcanistClass.arcanist;
        static internal LibraryScriptableObject library => Main.library;
        static internal AddClassLevels compNeutral, compBoss;
        static internal BlueprintAbility[] RisiaSpellKnown, RisiaBossSpellKnown;
        static internal BlueprintAbility[] RisiaSpellMemory, RisiaBossSpellMemory;
        static internal BlueprintFeature[] RisiaArcaneExploits, RisiaFeats;
        static private BlueprintAbility getSpell(string Id) => library.Get<BlueprintAbility>(Id);
        static private BlueprintFeature getFeat(string Id) => library.Get<BlueprintFeature>(Id);
        static private BlueprintFeatureSelection getSelection(string Id) => library.Get<BlueprintFeatureSelection>(Id);
        static void Prepare() {
            // from Tier 1 to Tier 3
            string[] RisiaSpellKnownIds = new string[] {
 				"4783c3709a74a794dbe7c8e7e0b1b038",
				"bd81a3931aa285a4f9844585b5d97e51",
				"91da41b9793a4624797921f221db653c",
				"8e7cfa5f213a90549aadd18f8f6f4664",
				"c60969e7f264e6d4b84a1499fdcf9039",
				"95851f6e85fe87d4190675db0419d112",
				"9e1ad5d6f87d19e4d8883d63a6e35568",
				"ef768022b0785eb43a18969903c537c4",
				"4ac47ddb9fa1eaf43a1b6809980cfbd2",
				"433b1faf4d02cc34abb0ade5ceda47c4",
				"4e0e9aba6447d514f88eff1464cc4763",
				"bb7ecad2d3d2c8247a38f44855c99061",
				"2c38da66e5a599347ac95b3294acbe00",
				"f001c73999fb5a543a199f890108d936",
				"88367310478c10b47903463c5d0152b0",
				"46fd02ad56c35224c9c91c88cd457791",
				"14ec7a4e52e90fa47a4c8d63c69fd5c1",
				"b7731c2b4fa1c9844a092329177be4c3",
				"29ccc62632178d344ad0be0865fd3113",
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
				"92681f181b507b34ea87018e8f7a528a",
				"903092f6488f9ce45a80943923576ab3",
				"2d81362af43aeac4387a3d4fced489c3",
				"c7104f7526c4c524f91474614054547e",
				"96c9d98b6a9a7c249b6c4572e4977157",
				"1a045f845778dc54db1c2be33a8c3c0a",
				"f492622e473d34747806bdb39356eb89",
                "68a9e6d7256f1354289a39003a46d826"
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

            List<BlueprintAbility> tmpList = new List<BlueprintAbility>();
            foreach(var id in RisiaSpellKnownIds) {
                tmpList.Add(getSpell(id));
            }
            RisiaSpellKnown = tmpList.ToArray();

            tmpList = new List<BlueprintAbility>();
            foreach(var id in RisiaSpellMemoryIds) {
                tmpList.Add(getSpell(id));
            }
            RisiaSpellMemory = tmpList.ToArray();

            RisiaFeats = new BlueprintFeature[] {
                getFeat("16fa59cc9a72a6043b566b49184f53fe"),// spell focus illusion               
                getFeat("797f25d709f559546b29e7bcb181cc74"),// improved initiative
                getFeat("16fa59cc9a72a6043b566b49184f53fe"),// spell focus enchantment
                getFeat("06964d468fde1dc4aa71a92ea04d930d"),// combat casting
            };
            RisiaArcaneExploits = new BlueprintFeature[] {
                ArcaneExploits.fastStudy,
                ArcaneExploits.potentMagic,
                ArcaneExploits.metaMixing,
                ArcaneExploits.dimensionalSlide
            };
        }
        static public void LoadNeutral() {
            
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
            compNeutral.SelectSpells = RisiaSpellKnown;
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
        }
        static public void Load() {
            if (ArcanistClass.arcanist == null) {
                throw new Exception("Wrong: Risia should be loaded after Arcanist is loaded");
            }
            Prepare();
            LoadNeutral();

            BlueprintUnit risia_companion = library.Get<BlueprintUnit>("c2dc52c5fec84bc2a74e2cb34fdb566b");
            BlueprintUnit risia_neutral = library.Get<BlueprintUnit>("d87f8e86724f46e798821d60f9d31eaf");
            risia_neutral.AddComponent(compNeutral);
            risia_companion.AddComponent(compNeutral);

            BlueprintUnit octavia = library.Get<BlueprintUnit>("f9161aa0b3f519c47acbce01f53ee217");
            Main.logger.Log($"{(octavia.Faction == risia_companion.Faction ? "Yes, faction are the same" : "Nope!")}");
        }
    }
}
