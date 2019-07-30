using ArcaneTide.Utils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ArcaneTide.Arcanist.Archetypes {
    [Harmony12.HarmonyPatch(typeof(ContextRankConfig), "GetBaseValue", new Type[] { typeof(MechanicsContext) })]
    public class ContextRankConfig_GetBaseValue_Patch {
        public static void Postfix(ContextRankConfig __instance, ref int __result, MechanicsContext context, ref ContextRankBaseValueType ___m_BaseValueType) {
            if (!BloodArcanist.wantToModify_ContextRankConfig.Contains(__instance)) {
                return;
            }
            if (___m_BaseValueType == ContextRankBaseValueType.SummClassLevelWithArchetype) {
                ClassData classdata = context.MaybeCaster.Descriptor.Progression.GetClassData(ArcanistClass.arcanist);
                if (classdata != null && classdata.Archetypes.Contains(BloodArcanist.archetype)) {
                    __result += classdata.Level;
                    return;
                }
            }
            if (___m_BaseValueType == ContextRankBaseValueType.OwnerSummClassLevelWithArchetype) {
                var maybeOwner = context.MaybeOwner;
                if (maybeOwner == null) return;
                ClassData classdata = maybeOwner.Descriptor.Progression.GetClassData(ArcanistClass.arcanist);
                if (classdata != null && classdata.Archetypes.Contains(BloodArcanist.archetype)) {
                    __result += classdata.Level;
                    return;
                }
            }
        }
    }
    public static class BloodArcanist {
        static public BlueprintArchetype archetype;
        static private BloodArcanistConfig config;
        static internal GlobalConstants consts => Main.constsManager;
        static internal LibraryScriptableObject library => Main.library;
        static private List<BlueprintFeature> draconicBreathFeatures = new List<BlueprintFeature>();
        static private List<BlueprintFeature> draconicAddFeatureOnLevels = new List<BlueprintFeature>();
        static private List<BlueprintAbility> draconicBreathAbilities = new List<BlueprintAbility>();
        static private List<BlueprintFeature> elementalAddFeatureOnLevels = new List<BlueprintFeature>();
        
        static private List<BlueprintAbility> elementalRayAbilities = new List<BlueprintAbility>();
        static private List<BlueprintFeature> elementalBlastFeatures = new List<BlueprintFeature>();
        static private List<BlueprintAbility> elementalBlastAbilities = new List<BlueprintAbility>();

        static public List<ContextRankConfig> wantToModify_ContextRankConfig = new List<ContextRankConfig>();
        static private void PrepareFix() {
            Regex draconicAddFeatureOnLevels_regex = new Regex(@"\ABloodlineDraconic.*AddLevel[1-9]\Z");
            Regex draconicBreathFeature_regex = new Regex(@"\ABloodlineDraconic[a-zA-Z]*BreathWeaponFeature\Z");
            Regex draconicBreathAbility_regex = new Regex(@"\ABloodlineDraconic[a-zA-Z]*BreathWeaponAbility\Z");

            Regex elementalRayAbility_regex = new Regex(@"\ABloodlineElemental[a-zA-Z]*ElementalRayAbility\Z");
            Regex elementalResistanceAddFeatures_regex = new Regex(@"\ABloodlineElemental[a-zA-Z]*ResistanceAbilityAdd\Z");
            Regex elementalBlastFeature_regex = new Regex(@"\ABloodlineElemental[a-zA-Z]*ElementalBlastFeature\Z");
            Regex elementalBlastAbility_regex = new Regex(@"\ABloodlineElemental[a-zA-Z]*ElementalBlastAbility\Z");
            foreach (var kv in library.BlueprintsByAssetId) {
                if(kv.Value is BlueprintFeature) {
                    BlueprintFeature feat = kv.Value as BlueprintFeature;
                    if (draconicBreathFeature_regex.IsMatch(feat.name)) {
                        draconicBreathFeatures.Add(feat);
                    }
                    if (draconicAddFeatureOnLevels_regex.IsMatch(feat.name)) {
                        draconicAddFeatureOnLevels.Add(feat);
                    }
                    if (elementalResistanceAddFeatures_regex.IsMatch(feat.name)) {
                        elementalAddFeatureOnLevels.Add(feat);
                    }
                    if (elementalBlastFeature_regex.IsMatch(feat.name)) {
                        elementalBlastFeatures.Add(feat);
                    }
                }
                if(kv.Value is BlueprintAbility) {
                    BlueprintAbility abl = kv.Value as BlueprintAbility;
                    if (draconicBreathAbility_regex.IsMatch(abl.name)) {
                        draconicBreathAbilities.Add(abl);
                    }
                    if (elementalRayAbility_regex.IsMatch(abl.name)) {
                        elementalRayAbilities.Add(abl);
                    }
                    if (elementalBlastAbility_regex.IsMatch(abl.name)) {
                        elementalBlastAbilities.Add(abl);
                    }
                }
            }
        }
        static private void FixProgressions() {
            foreach(string id in config.BloodlineProgressions) {
                BlueprintProgression prog = library.Get<BlueprintProgression>(id);
                HelpersNeu.Add<BlueprintCharacterClass>(ref prog.Classes, ArcanistClass.arcanist);
                HelpersNeu.Add<BlueprintArchetype>(ref prog.Archetypes, archetype);
            }
        }
        
        static private void FixSpecialComps() {
            //arcane 9, new arcane selection, wants a variant for blood arcanist
            BlueprintFeatureSelection arcane9_select = library.Get<BlueprintFeatureSelection>("20a2435574bdd7f4e947f405df2b25ce");
            BlueprintFeature arcane9_scion = library.Get<BlueprintFeature>("c66e61dea38f3d8479a54eabec20ac99");
            BlueprintFeature arcane9_ba = library.CopyAndAdd<BlueprintFeature>(
                arcane9_scion,
                "BloodlineArcaneNewArcanaFeatureArcanist",
                "b0a5863f046005d408d53f05894a6c3a"//MD5-32[ArcanistClass.Archetype.BloodArcanist.Arcane9.Feat]
                );
            arcane9_ba.RemoveComponent(arcane9_ba.GetComponent<LearnSpellParametrized>());
            arcane9_ba.RemoveComponent(arcane9_ba.GetComponent<PrerequisiteArchetypeLevel>());
            arcane9_ba.AddComponent(Helpers.Create<LearnSpellParametrized>(a => {
                a.SpellcasterClass = ArcanistClass.arcanist;
                a.SpellList = library.Get<BlueprintSpellList>("ba0401fdeb4062f40a7aa95b6f07fe89");//wizard spelllist
            }));
            arcane9_ba.AddComponent(Helpers.Create<PrerequisiteArchetypeLevel>(a => {
                a.Archetype = archetype;
                a.CharacterClass = ArcanistClass.arcanist;
                a.Level = 1;
            }));
            HelpersNeu.Add(ref arcane9_select.AllFeatures, arcane9_ba);
            HelpersNeu.Add(ref arcane9_select.Features, arcane9_ba);
        }
        static private void FixResources() {
            List<BlueprintAbilityResource> resources = new List<BlueprintAbilityResource>();
            foreach(string id in config.ResourcesIds) {
                resources.Add(library.Get<BlueprintAbilityResource>(id));
            }

        }
        static private void FixFactsWithContextRankConfig() {
            FastGetter contextRankConfig_Archetype_getter = Helpers.CreateFieldGetter<ContextRankConfig>("Archetype");

            List<BlueprintUnitFact> facts = new List<BlueprintUnitFact>();
            foreach(string id in config.FactWithContextRankConfig) {
                facts.Add(library.Get<BlueprintUnitFact>(id));
            }
            facts.AddRange(draconicBreathAbilities);
            facts.AddRange(elementalRayAbilities);
            facts.AddRange(elementalBlastAbilities);
            foreach(BlueprintUnitFact fact in facts) {
                var comps = fact.GetComponents<ContextRankConfig>();
                if(comps == null) {
                    throw new Exception($"In BloodArcanist FixFactsWithContextRankConfig: Fact {fact.name} has null ContextRankConfig arrays.");
                }
                //ContextRankConfig has only one archetype slot, and that slot is occupied
                //by Eldritch Scion already. This is a very troublesome issue.
                //Curse you three thousands, Owlcat!!!
                //So here we want to use harmony patches to "Add" Blood Arcanist archetype
                //to the return value of ContextRankConfig.GetBaseValue(), when we are dealing with
                //those ContextRankConfigs that have something to do with sorcerer/eldritch scion levels.

                //The patch is class ContextRankConfig_GetBaseValue_Patch 
                wantToModify_ContextRankConfig.AddRange(comps);
            }
        }
        static private void FixFactsWithAddFeatureOnLevel() {
            List<BlueprintUnitFact> facts = new List<BlueprintUnitFact>();
            foreach(string id in config.FactWithAddFeatureOnLevel) {
                facts.Add(library.Get<BlueprintUnitFact>(id));
            }
            facts.AddRange(draconicAddFeatureOnLevels);
            foreach(BlueprintUnitFact fact in facts) {
                var comps = fact.GetComponents<AddFeatureOnClassLevel>();
                if (comps == null) {
                    throw new Exception($"Error on BloodArcanist.FixFactsWithAddFeatureOnLevel: Fact {fact.name} has null AddFeatureOnLevel comp!");
                }
                foreach(AddFeatureOnClassLevel comp in comps) {
                    HelpersNeu.Add<BlueprintCharacterClass>(ref comp.AdditionalClasses, ArcanistClass.arcanist);
                    HelpersNeu.Add<BlueprintArchetype>(ref comp.Archetypes, archetype);
                }
            }
        }
        static private void FixFactsWithReplaceCL() {
            List<BlueprintUnitFact> facts = new List<BlueprintUnitFact>();
            foreach (string id in config.FactWithReplaceCL) {
               facts.Add(library.Get<BlueprintUnitFact>(id));
            }
            facts.AddRange(draconicBreathFeatures);
            facts.AddRange(elementalBlastFeatures);
            foreach(var fact in facts) {
                ReplaceCasterLevelOfAbility comp = fact.GetComponent<ReplaceCasterLevelOfAbility>();
                HelpersNeu.Add<BlueprintCharacterClass>(ref comp.AdditionalClasses, ArcanistClass.arcanist);
                HelpersNeu.Add<BlueprintArchetype>(ref comp.Archetypes, archetype);
            }
        }
        static private void FixFactsWithBindClass() {
            List<BlueprintUnitFact> facts = new List<BlueprintUnitFact>();
            Main.logger.Log("RUA 0");
            foreach (string id in config.FactWithBindClass) {
                facts.Add(library.Get<BlueprintUnitFact>(id));
            }
            Main.logger.Log("RUA 1");
            facts.AddRange(draconicBreathFeatures);
            facts.AddRange(elementalBlastFeatures);
            Main.logger.Log("RUA 2");
            foreach (var fact in facts) {
                Main.logger.Log($"Fixing Fact {fact.name}, ID {fact.AssetGuid}");
                BindAbilitiesToClass comp = fact.GetComponent<BindAbilitiesToClass>();
                if(comp == null) {
                    Main.logger.Log("Bind Abl to class comp is NULL !");
                }
                if(comp.AdditionalClasses == null) {
                    Main.logger.Log("Comp.additionalClasses is NULL !");
                }
                if(comp.Archetypes == null) {
                    Main.logger.Log("Comp.Archetypes is NULL !");
                }
                HelpersNeu.Add<BlueprintCharacterClass>(ref comp.AdditionalClasses, ArcanistClass.arcanist);
                HelpersNeu.Add<BlueprintArchetype>(ref comp.Archetypes, archetype);
            }
        }
        static public BlueprintArchetype Create() {
            if (archetype != null) return archetype;

            StreamReader fin = new StreamReader(Path.Combine(Main.ModPath, consts.BloodArcanistConfigFile));
            string fileData = fin.ReadToEnd();
            fin.Close();
            config = JsonConvert.DeserializeObject<BloodArcanistConfig>(fileData);

            archetype = Helpers.Create<BlueprintArchetype>();
            library.AddAsset(archetype, "3c012939eb29ab9e2a3553f0bc50d1e2");//MD5-32[ArcanistClass.Archetype.BloodArcanist]

            wantToModify_ContextRankConfig = new List<ContextRankConfig>();

            PrepareFix();
            FixProgressions();
            FixResources();
            FixFactsWithAddFeatureOnLevel();
            FixFactsWithContextRankConfig();
            FixFactsWithBindClass();
            FixFactsWithReplaceCL();
            FixSpecialComps();

            archetype.name = "ArcanistClassArchetypeBloodArcanist";
            archetype.LocalizedName = Helpers.CreateString("ArcanistClass.Archetype.BloodArcanist.Name");
            archetype.LocalizedDescription = Helpers.CreateString("ArcanistClass.Archetype.BloodArcanist.Desc");

            archetype.ReplaceStartingEquipment = false;
            archetype.RemoveSpellbook = false;
            archetype.RecommendedAttributes = new StatType[] { StatType.Intelligence, StatType.Charisma };
            archetype.ChangeCasterType = false;

            archetype.AddFeatures = new LevelEntry[] {
                Helpers.LevelEntry(1, library.Get<BlueprintFeatureSelection>("24bef8d1bee12274686f6da6ccbc8914")) //bloodline seelction
            };
            archetype.RemoveFeatures = new LevelEntry[] {
                Helpers.LevelEntry(1, ArcaneExploits.exploitSelection),
                Helpers.LevelEntry(3, ArcaneExploits.exploitSelection),
                Helpers.LevelEntry(9, ArcaneExploits.exploitSelection),
                Helpers.LevelEntry(15, ArcaneExploits.exploitSelection),
                Helpers.LevelEntry(20, Supremancy.feat)
            };
            //HelpersNeu.Add<BlueprintArchetype>(ref ArcanistClass.arcanist.Archetypes, archetype);
            return archetype;
        }
    }
    public class BloodArcanistConfig {
        [JsonProperty(PropertyName ="BloodlineProgressions")]
        public List<string> BloodlineProgressions { get; set; }

        [JsonProperty(PropertyName = "ResourcesIds")]
        public List<string> ResourcesIds { get; set; }
        
        [JsonProperty(PropertyName = "FactWithAddFeatureOnLevel")]
        public List<string> FactWithAddFeatureOnLevel { get; set; }

        [JsonProperty(PropertyName = "FactWithContextRankConfig")]
        public List<string> FactWithContextRankConfig { get; set; }

        [JsonProperty(PropertyName = "FactWithReplaceCL")]
        public List<string> FactWithReplaceCL { get; set; }

        [JsonProperty(PropertyName = "FactWithBindClass")]
        public List<string> FactWithBindClass { get; set; }
    }
}
