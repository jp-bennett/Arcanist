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
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
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
    
    public static class SchoolSavant {
        static public BlueprintArchetype archetype;
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
                WizardAbjurationResistanceNeu compNeu = Helpers.Create<WizardAbjurationResistanceNeu>();
                compNeu.Type = compOld.Type;
                compNeu.Pool = compOld.Pool;
                compNeu.Value = compOld.Value;
                compNeu.ValueMultiplier = compOld.ValueMultiplier;
                compNeu.UseValueMultiplier = true;
                compNeu.UsePool = true;
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
           
            //fix necromancy turn undead cl
            var necro1_feat = library.Get<BlueprintFeature>("927707dce06627d4f880c90b5575125f");
            ReplaceCasterLevelOfAbility compOld4 = necro1_feat.GetComponent<ReplaceCasterLevelOfAbility>();
            compOld4.AdditionalClasses = new BlueprintCharacterClass[] { ArcanistClass.arcanist };
            
            //fix transmutation physical enhance
            foreach (string buffId in config.TransmutationPhysicalEnhanceBuffs) {
                var buff = library.Get<BlueprintBuff>(buffId);
                
                AddStatBonusScaled compOld5 = buff.GetComponent<AddStatBonusScaled>();
                AddStatBonusScaledTransmutationSpecialized compNeu5 = Helpers.Create<AddStatBonusScaledTransmutationSpecialized>();
                compNeu5.classes = new BlueprintCharacterClass[] { wizard, ArcanistClass.arcanist };
                compNeu5.Descriptor = compOld5.Descriptor;
                compNeu5.Stat = compOld5.Stat;
                compNeu5.Value = compOld5.Value;
                buff.RemoveComponent(buff.GetComponent<AddStatBonusScaled>());
            }
            
            //fix transmutation polymorphy
            var trans8_feat = library.Get<BlueprintFeature>("aeb56418768235640a3ee858d5ee05e8");
            AddFeatureOnClassLevel compOld6 = trans8_feat.GetComponent<AddFeatureOnClassLevel>();
            compOld6.AdditionalClasses = new BlueprintCharacterClass[] { ArcanistClass.arcanist };
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
                
                prog.Classes = new BlueprintCharacterClass[] { ArcanistClass.arcanist, wizard };
                prog.Archetypes = new BlueprintArchetype[] { archetype };
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
            foreach (string featId in config.SchoolOppositionFeatureIds) {
                BlueprintFeature feat = library.Get<BlueprintFeature>(featId);
                SpellSchool school = feat.GetComponent<AddOppositionSchool>().School;
                feat.AddComponent(Helpers.Create<AddOppositionSchool>(a => { 
                    a.CharacterClass = ArcanistClass.arcanist;
                    a.School = school;
                }));
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
        static private void FixResources() {
            FastGetter Amount_Getter = Helpers.CreateFieldGetter<BlueprintAbilityResource>("m_MaxAmount");
            FastSetter Amount_Setter = Helpers.CreateFieldSetter<BlueprintAbilityResource>("m_MaxAmount");
            foreach(string resId in config.ResourcesIds) {
                var res = library.Get<BlueprintAbilityResource>(resId);
                var amount = Amount_Getter(res);
                Helpers.SetField(amount, "Class", new BlueprintCharacterClass[] { wizard, ArcanistClass.arcanist });
                Amount_Setter(res, amount);
            }
        }
        static public BlueprintArchetype Create() {
            if (archetype != null) return archetype;
            StreamReader fin = new StreamReader(Path.Combine(Main.ModPath, consts.SchoolSavantConfigFile));
            string fileData = fin.ReadToEnd();
            fin.Close();
            config = JsonConvert.DeserializeObject<SchoolSavantConfig>(fileData);
            archetype = Helpers.Create<BlueprintArchetype>();
            library.AddAsset(archetype, "f4e14b7e792cfb93a5b6a07cd6bb342c");//MD5-32[ArcanistClass.Archetype.SchoolSavant]

            FixSpecializationProgressions();
            FixOppositionSchoolFeats();
            FixSpecializationSchoolMainFeats();
            FixSpecialComponents();
            FixResources();

            

            archetype.name = "ArcanistClassArchetypeSchoolSavant";
            archetype.LocalizedName = Helpers.CreateString("ArcanistClass.Archetype.SchoolSavant.Name");
            archetype.LocalizedDescription = Helpers.CreateString("ArcanistClass.Archetype.SchoolSavant.Desc");

            archetype.ChangeCasterType = false;
            archetype.BaseAttackBonus = ArcanistClass.arcanist.BaseAttackBonus;
            archetype.FortitudeSave = ArcanistClass.arcanist.FortitudeSave;
            archetype.ReflexSave = ArcanistClass.arcanist.ReflexSave;
            archetype.WillSave = ArcanistClass.arcanist.WillSave;
            archetype.StartingGold = 411;
            archetype.ReplaceClassSkills = false;
            archetype.ReplaceStartingEquipment = false;
            archetype.ReplaceSpellbook = null;
            archetype.RemoveSpellbook = false;
            archetype.IsArcaneCaster = true;
            archetype.IsDivineCaster = false;

            BlueprintFeatureSelection savantSchoolSelection = library.CopyAndAdd<BlueprintFeatureSelection>(
                "5f838049069f1ac4d804ce0862ab5110",
                "ArcanistClassArchetypeSchoolSavantSchoolSelection",
                "681d02d26bcdb784041d05dd77a79a70"//MD5-32[ArcanistClass.Archetype.SchoolSavant.SchoolSelection]
                );
            //School savant must choose a specialization school, that is, she can't choose universalist.
            List<BlueprintFeature> tmpList = savantSchoolSelection.AllFeatures.ToList();
            tmpList.Remove(library.Get<BlueprintProgression>("0933849149cfc9244ac05d6a5b57fd80"));//universal progression
            savantSchoolSelection.AllFeatures = tmpList.ToArray();
            archetype.AddFeatures = new LevelEntry[] {
                Helpers.LevelEntry(1, savantSchoolSelection)
            };

            var exploit = ArcaneExploits.exploitSelection;
            archetype.RemoveFeatures = new LevelEntry[] {
                Helpers.LevelEntry(1, exploit),
                Helpers.LevelEntry(3, exploit),
                Helpers.LevelEntry(7, exploit)
            };
            return archetype;
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
        [JsonProperty(PropertyName = "FactsWithContextRankConfig")]
        public List<string> FactsWithContextRankConfig { get; set; }
        [JsonProperty(PropertyName = "SchoolSpecializationProgressionsIds")]
        public List<string> SchoolSpecializationProgressionsIds { get; set; }
        [JsonProperty(PropertyName = "SchoolSpecializationMainFeatIds")]
        public List<string> SchoolSpecializationMainFeatIds { get; set; }
        [JsonProperty(PropertyName = "SchoolOppositionFeatureIds")]
        public List<string> SchoolOppositionFeatureIds { get; set; }
        [JsonProperty(PropertyName = "ResourcesIds")]
        public List<string> ResourcesIds { get; set; }
        [JsonProperty(PropertyName = "AbjurationResistanceBuffs")]
        public List<string> AbjurationResistanceBuffs { get; set; }
        [JsonProperty(PropertyName = "TransmutationPhysicalEnhanceBuffs")]
        public List<string> TransmutationPhysicalEnhanceBuffs { get; set; }
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

    public class AddStatBonusScaledTransmutationSpecialized : BuffLogic {
        // Token: 0x060017AE RID: 6062 RVA: 0x0009E1E8 File Offset: 0x0009C3E8
        public override void OnTurnOn() {
            base.OnTurnOn();
            int classLevels = 0;
            foreach(var thisClass in this.classes) {
                classLevels += base.Owner.Progression.GetClassLevel(thisClass);
            }
            int value = 1 + classLevels / 5;
            ModifiableValue stat = base.Owner.Stats.GetStat(this.Stat);
            if (stat != null) {
                this.m_Modifier = stat.AddModifier(value, this, this.Descriptor);
            }
        }

        // Token: 0x060017AF RID: 6063 RVA: 0x000118C7 File Offset: 0x0000FAC7
        public override void OnTurnOff() {
            base.OnTurnOff();
            if (this.m_Modifier != null) {
                this.m_Modifier.Remove();
            }
            this.m_Modifier = null;
        }

        // Token: 0x04001624 RID: 5668
        public ModifierDescriptor Descriptor;

        // Token: 0x04001625 RID: 5669
        public StatType Stat;

        // Token: 0x04001626 RID: 5670
        public int Value;

        // Token: 0x04001627 RID: 5671
        public BlueprintCharacterClass[] classes;
        // Token: 0x04001628 RID: 5672
        private ModifiableValue.Modifier m_Modifier;
    }
}
