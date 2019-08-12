using ArcaneTide.Utils;
using ArcaneTide.Components;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Enums.Damage;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Utility;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Mechanics.Properties;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.EventConditionActionSystem.Conditions;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules;

namespace ArcaneTide.Arcanist {
    public class WizardSchoolUtils {
        /*
        Abjuration = 1,
        Conjuration = 2,
        Divination = 3,
        Enchantment = 4,
        Evocation = 5,
        Illusion = 6,
        Necromancy = 7,
        Transmutation = 8,
        Universalist = 9
        */
        static internal LibraryScriptableObject library => Main.library;
        static internal BlueprintCharacterClass wizard = library.Get<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");
        static internal string oppositionSchoolSelectionId = "6c29030e9fea36949877c43a6f94ff31";

        static public string[] wizardSchoolProgressionIds = new string[] {
            "",
            "c451fde0aec46454091b70384ea91989",
            "567801abe990faf4080df566fadcd038",
            "d7d18ce5c24bd324d96173fdc3309646",
            "252363458703f144788af49ef04d0803",
            "f8019b7724d72a241a97157bc37f1c3b",
            "24d5402c0c1de48468b563f6174c6256",
            "e9450978cc9feeb468fb8ee3a90607e3",
            "b6a604dab356ac34788abf4ad79449ec",
            "0933849149cfc9244ac05d6a5b57fd80"
        };//Sorted by id of SpellSchool enum 1-9.
        static public string[] wizardSchoolOppositionFeatureIds = new string[] {
            "",
            "7f8c1b838ff2d2e4f971b42ccdfa0bfd",
            "ca4a0d68c0408d74bb83ade784ebeb0d",
            "09595544116fe5349953f939aeba7611",
            "875fff6feb84f5240bf4375cb497e395",
            "c3724cfbe98875f4a9f6d1aabd4011a6",
            "6750ead44c0c034428c6509c68110375",
            "a9bb3dcb2e8d44a49ac36c393c114bd9",
            "fc519612a3c604446888bb345bca5234"
        };//Sorted by id of SpellSchool enum 1-8.
        static public string[] wizardSchoolSpellListIds = new string[] {
            "",
            "c7a55e475659a944f9229d89c4dc3a8e",
            "69a6eba12bc77ea4191f573d63c9df12",
            "d234e68b3d34d124a9a2550fdc3de9eb",
            "c72836bb669f0c04680c01d88d49bb0c",
            "79e731172a2dc1f4d92ba229c6216502",
            "d74e55204daa9b14993b2e51ae861501",
            "5fe3acb6f439db9438db7d396f02c75c",
            "becbcfeca9624b6469319209c2a6b7f1"
        };//Sorted by id of SpellSchool enum 1-8.
        static List<BlueprintUnitFact>[] wizardSchoolFacts = new List<BlueprintUnitFact>[10];
        static List<BlueprintAbilityResource>[] wizardSchoolRes = new List<BlueprintAbilityResource>[10];
        static Dictionary<string, string> oldId_to_newId = new Dictionary<string, string>();
        
        internal struct ReplacePointerData {
            BlueprintUnitFact fact;
            BlueprintComponent comp_old;
            BlueprintComponent comp_new;
        }
        static List<ReplacePointerData> replacers = new List<ReplacePointerData>();
        static private void Init() {
            for(int i = 0; i < 10; i++) {
                wizardSchoolFacts[i] = new List<BlueprintUnitFact>();
                wizardSchoolRes[i] = new List<BlueprintAbilityResource>();
            }
        }
        static private void ScanFacts() {
            //Scan Features from Progression.
            var scanned = new List<string>();
            int allFactsCnt = 0;
            for(int i = 1; i <= 9; i++) {
                var factList = wizardSchoolFacts[i];
                BlueprintProgression prog = library.Get<BlueprintProgression>(wizardSchoolProgressionIds[i]);
                foreach(LevelEntry entry in prog.LevelEntries) {
                    foreach(BlueprintFeature feat in entry.Features) {
                        if(feat.AssetGuid != oppositionSchoolSelectionId) {
                            // is not OppositionSchoolSelection
                            factList.Add(feat);
                        }
                    }
                }
            }
            //Scan Abilities from Features
            for(int i = 1; i <= 9; i++) {
                var factList = wizardSchoolFacts[i];
                foreach(BlueprintUnitFact fact in factList) {
                    //now every fact is a BlueprintFeature
                    if (!scanned.Contains(fact.AssetGuid)) {
                        scanned.Add(fact.AssetGuid);
                    }
                    else continue;
                    foreach(BlueprintComponent comp in fact.ComponentsArray) {
                        if(comp is AddAbilityResources) {
                            var res = (comp as AddAbilityResources).Resource;
                            wizardSchoolRes[i].Add(res);
                        }
                        if(comp is AddFacts) {
                            var facts = (comp as AddFacts).Facts;
                            factList.AddRange(facts);
                            allFactsCnt += facts.Length;
                        }
                    }
                }
            }
            while (scanned.Count != allFactsCnt) {
                //Scan Abilities and Buffs from Abilities.
                for (int i = 1; i <= 9; i++) {
                    var factList = wizardSchoolFacts[i];
                    foreach (BlueprintUnitFact fact in factList) {
                        //now every fact is a BlueprintFeature
                        if (!scanned.Contains(fact.AssetGuid)) {
                            scanned.Add(fact.AssetGuid);
                        }
                        else continue;
                        if (fact is BlueprintActivatableAbility) {
                            var buff = (fact as BlueprintActivatableAbility).Buff;
                            factList.Add(buff);
                            allFactsCnt++;
                        }
                        foreach (BlueprintComponent comp in fact.ComponentsArray) {
                            if (comp is AddAbilityResources) {
                                var res = (comp as AddAbilityResources).Resource;
                                wizardSchoolRes[i].Add(res);
                            }
                            if (comp is AddFacts) {
                                var facts = (comp as AddFacts).Facts;
                                factList.AddRange(facts);
                                allFactsCnt += facts.Length;
                            }
                            if(comp is AbilityEffectRunAction) {
                                var actnList = (comp as AbilityEffectRunAction).Actions.Actions;
                                foreach(var actn in actnList) {
                                    if(actn is ContextActionApplyBuff) {
                                        var buff = (actn as ContextActionApplyBuff).Buff;
                                        factList.Add(buff);
                                        allFactsCnt++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            //manually add facts not added by the procedure above.
            //Abjuration Has None.
            //Conjuration Has None.

            //NecromancyGreaterBuff
            wizardSchoolFacts[(int)SpellSchool.Necromancy].Add(library.Get<BlueprintBuff>("750e4a94522ea4d43a86eb9c648f39ff"));

        }
        static private void DoCopy() {

        }
        static private void DoFix() {
            //Fix school selections.
            //Fix progressions.
            //Fix OppositionSelections.
            //Fix Special Spelllists.
            //Fix resources.

            //Feature SpecialistConjurationFeature.cee0f7edbd874a042952ee150f878b84 has 
            // a special Component Kingmaker.Designers.Mechanics.Facts.AddClassLevelToSummonDuration.

        }
        static public void Load() {
            Init();
            ScanFacts();
            DoCopy();
            DoFix();
        }
    }
}
