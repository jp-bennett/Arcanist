using ArcaneTide.Utils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcaneTide.Arcanist.Archetypes {
    
    class SchoolSavant {
        static private SchoolSavantConfig config;
        static internal GlobalConstants consts => Main.constsManager;
        static internal LibraryScriptableObject library => Main.library;
        static internal BlueprintCharacterClass wizard = library.Get<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");
        static private void FixExistingBlueprintFacts() {
            FastGetter classes_Getter = Helpers.CreateFieldGetter<ContextRankConfig>("m_Class");
            FastSetter classes_Setter = Helpers.CreateFieldSetter<ContextRankConfig>("m_Class");
            foreach (string factId in config.FactsWithContextRankConfig) {
                BlueprintUnitFact fact = library.Get<BlueprintUnitFact>(factId);
                foreach(ContextRankConfig comp in fact.GetComponents<ContextRankConfig>()) {
                    if (comp.IsBasedOnClassLevel) {
                        BlueprintCharacterClass[] classes = classes_Getter(comp) as BlueprintCharacterClass[];
                        if(classes == null) {
                            throw new Exception($"Error! get null classes for IsBasedOnClassLevel ContextRankConfig named {fact.name}!");
                        }
                        List<BlueprintCharacterClass> classesList = classes.ToList<BlueprintCharacterClass>();
                        if (classesList.Contains(wizard)) {
                            classesList.Add(ArcanistClass.arcanist);
                        }
                        classes_Setter(comp, classesList.ToArray());
                    }
                }

            }
        }
        static private void FixSpecialComponents() {
            //fix abjuration resistance buffs
            foreach(string buffId in config.AbjurationResistanceBuffs) {
                var buff = library.Get<BlueprintBuff>(buffId);
                WizardAbjurationResistance compOld = buff.GetComponent<WizardAbjurationResistance>();
                WizardAbjurationResistanceNeu compNeu = UnityEngine.Object.Instantiate<WizardAbjurationResistance>(compOld) as WizardAbjurationResistanceNeu;
                compNeu.Wizards = new BlueprintCharacterClass[] { wizard, ArcanistClass.arcanist };
                buff.RemoveComponent(compOld);
                buff.AddComponent(compNeu);
            }
            //fix conjuration enhance summoning
            var conj1_feat = library.Get<BlueprintFeature>("cee0f7edbd874a042952ee150f878b84");
            conj1_feat.RemoveComponent(conj1_feat.GetComponent<AddClassLevelToSummonDuration>());
            AddClassLevelToSummonDurationNeu compNeu2 = Helpers.Create<AddClassLevelToSummonDurationNeu>();
            compNeu2.CharacterClasses = new BlueprintCharacterClass[] { wizard, ArcanistClass.arcanist };
            compNeu2.Half = true;
            conj1_feat.AddComponent(compNeu2);
            //fix evocation intense spell
            var evo1_feat = library.Get<BlueprintFeature>("c46512b796216b64899f26301241e4e6");
            evo1_feat.RemoveComponent(evo1_feat.GetComponent<IntenseSpells>());
            IntenseSpellsNeu compNeu3 = Helpers.Create<IntenseSpellsNeu>();
            compNeu3.Wizards = new BlueprintCharacterClass[] { wizard, ArcanistClass.arcanist };
            evo1_feat.AddComponent(compNeu3);
        }
        static private void FixSpecializationProgressions() {
            //take such situation into consideration: SchoolSavant Arcanist + Specialization Wizard. Thus 
            //the character would have 2 specialization school, and that's not permitted.
            //This method will add prerequisites so that a character can't choose two different 
            //specialization school BlueprintProgression s.
            Dictionary<string, BlueprintProgression> progs = new Dictionary<string, BlueprintProgression>();
            Dictionary<string, PrerequisiteNoFeature> preqNoThisProgression = new Dictionary<string, PrerequisiteNoFeature>();
            //get progressions
            foreach (string progressionId in config.SchoolSpecializationProgressionsIds) {
                var prog = library.Get<BlueprintProgression>(progressionId);
                progs[progressionId] = prog;
            }
            //create prerequisites
            foreach (string progressionId in config.SchoolSpecializationProgressionsIds) {
                preqNoThisProgression[progressionId] = Helpers.PrerequisiteNoFeature(progs[progressionId]);
            }
            //fix progressions
            foreach (string progressionId in config.SchoolSpecializationProgressionsIds) {
                var thisProgression = progs[progressionId];
                foreach(var kv in preqNoThisProgression) {
                    if(kv.Key != progressionId) {
                        thisProgression.AddComponent(kv.Value);
                    }
                }
            }
        }
        static private void FixOppositionSchoolFeats() {
            //fix opposition school feats so that they can function on arcanist class
            foreach(string featId in config.SchoolOppositionFeatureIds) {
                BlueprintFeature feat = library.Get<BlueprintFeature>(featId);
                feat.AddComponent(Helpers.Create<AddOppositionSchool>(a => a.CharacterClass = ArcanistClass.arcanist));
            }
        }
        static private void FixSpecializationSchoolMainFeats() {
            //add AddSpecialSpellList for Arcanist
            foreach(string featId in config.SchoolSpecializationMainFeatIds) {
                var mainFeat = library.Get<BlueprintFeature>(featId);
                AddSpecialSpellList compWiz = mainFeat.GetComponent<AddSpecialSpellList>();
                if(compWiz == null) {
                    throw new Exception($"Wrong at SchoolSavant.FixSpecializationSchoolMainFeats! compWiz of {mainFeat.name} get null.");
                }
                AddSpecialSpellList compArc = UnityEngine.Object.Instantiate(compWiz);
                compArc.CharacterClass = ArcanistClass.arcanist;
                mainFeat.AddComponent(compArc);
            }
        }
        static public void Load() {
            StreamReader fin = new StreamReader(Path.Combine(Main.ModPath, consts.SchoolSavantConfigFile));
            string fileData = fin.ReadToEnd();
            fin.Close();
            config = JsonConvert.DeserializeObject<SchoolSavantConfig>(fileData);

            FixSpecializationProgressions();
            FixOppositionSchoolFeats();
            FixSpecializationSchoolMainFeats();
        }
        static public void Test() {
            BlueprintFeature abjurationFeat = library.Get<BlueprintFeature>("30f20e6f850519b48aa59e8c0ff66ae9");
            BlueprintSpellList abjurationSpellList = library.Get<BlueprintSpellList>("c7a55e475659a944f9229d89c4dc3a8e");
            abjurationFeat.AddComponent(Helpers.Create<AddSpecialSpellList>(a => {
                a.CharacterClass = ArcanistClass.arcanist;
                a.SpellList = abjurationSpellList;
            }));
        }
    }
    public class SchoolSavantConfig {
        public List<string> FactsWithContextRankConfig { get; set; }
        public List<string> SchoolSpecializationProgressionsIds { get; set; }
        public List<string> SchoolOppositionFeatureIds { get; set; }
        public List<string> SchoolSpecializationMainFeatIds { get; set; }

        public List<string> AbjurationResistanceBuffs { get; set; }
    }

    public class WizardAbjurationResistanceNeu : WizardAbjurationResistance {
        // Token: 0x060018F8 RID: 6392 RVA: 0x000A0F78 File Offset: 0x0009F178
        public new void OnEventAboutToTrigger(RuleCalculateDamage evt) {

            int classLevel = 0;
            foreach (var thisClass in this.Wizards) {
                classLevel += base.Owner.Progression.GetClassLevel(thisClass);
            }
            if (classLevel == 20) {
                foreach (BaseDamage baseDamage in evt.DamageBundle) {
                    EnergyDamage energyDamage = baseDamage as EnergyDamage;
                    if (energyDamage != null && energyDamage.EnergyType == this.Type) {
                        baseDamage.Immune = true;
                    }
                }
            }
        }

        // Token: 0x060018F9 RID: 6393 RVA: 0x00004102 File Offset: 0x00002302
        public new void OnEventDidTrigger(RuleCalculateDamage evt) {
        }

        // Token: 0x04001724 RID: 5924
        public BlueprintCharacterClass[] Wizards;
    }

    public class AddClassLevelToSummonDurationNeu : RuleInitiatorLogicComponent<RuleSummonUnit> {
        // Token: 0x0600197C RID: 6524 RVA: 0x000A1E30 File Offset: 0x000A0030
        public override void OnEventAboutToTrigger(RuleSummonUnit evt) {
            AbilityData ability = evt.Reason.Ability;
            if (((ability != null) ? ability.Spellbook : null) != null && ability.Blueprint.School == SpellSchool.Conjuration) {
                int num = 0;
                foreach (var thisClass in this.CharacterClasses) {
                    base.Owner.Progression.GetClassLevel(thisClass);
                }
                num = ((!this.Half) ? num : Math.Max(num / 2, 1));
                evt.BonusDuration += num.Rounds();
            }
        }

        // Token: 0x0600197D RID: 6525 RVA: 0x00004102 File Offset: 0x00002302
        public override void OnEventDidTrigger(RuleSummonUnit evt) {
        }

        // Token: 0x040017AC RID: 6060
        public BlueprintCharacterClass[] CharacterClasses;

        // Token: 0x040017AD RID: 6061
        public bool Half;
    }

    public class IntenseSpellsNeu : RuleInitiatorLogicComponent<RuleCalculateDamage> {
        // Token: 0x0600187F RID: 6271 RVA: 0x000A0388 File Offset: 0x0009E588
        public override void OnEventAboutToTrigger(RuleCalculateDamage evt) {
            BaseDamage baseDamage = evt.DamageBundle.FirstOrDefault<BaseDamage>();
            AbilityData ability = evt.Reason.Ability;
            if (ability == null || ability.Blueprint.School != SpellSchool.Evocation || baseDamage == null) {
                return;
            }
            int classLevels = 0;
            foreach(var thisClass in this.Wizards) {
                classLevels += base.Owner.Progression.GetClassLevel(thisClass);
            }
            if (evt.ParentRule.Projectile == null || evt.ParentRule.Projectile.IsFirstProjectile) {
                int bonusDamage = Math.Max(classLevels / 2, 1);
                baseDamage.AddBonus(bonusDamage);
            }
        }

        // Token: 0x06001880 RID: 6272 RVA: 0x00004102 File Offset: 0x00002302
        public override void OnEventDidTrigger(RuleCalculateDamage evt) {
        }

        // Token: 0x040016EE RID: 5870
        public BlueprintCharacterClass[] Wizards;
    }
}
