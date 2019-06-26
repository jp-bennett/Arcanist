using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcaneTide.Arcanist;
using Kingmaker.Blueprints.Classes.Spells;

namespace ArcaneTide.Components {
    /*
    class AddClassLevelToRankBonus : RuleInitiatorLogicComponent<RuleCalculateAbilityParams> {
        
        public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt) {
            if (!(Spells.Contains<BlueprintAbility>(evt.Spell)))return;
            UnitDescriptor unit = base.Owner;
            int classLevel = 0;
            foreach (var cls in ClassesAdd) {
                classLevel += unit.Progression.GetClassLevel(cls);
            }
            foreach(var cls in ClassesRemove) {
                classLevel -= unit.Progression.GetClassLevel(cls);
            }
            evt.AddBonusCasterLevel((int)(MultiplierClassLevel * classLevel) + Bonus);
        }

        public override void OnEventDidTrigger(RuleCalculateAbilityParams evt) {
            
        }
        public BlueprintAbility[] Spells;
        public BlueprintCharacterClass[] ClassesAdd, ClassesRemove;
        public double MultiplierClassLevel=1.0;
        public int Bonus=0;
    }

    class AddOtherLv1Abilities : BuffLogic {
        public override void OnTurnOn() {
            base.OnTurnOn();
            TempFeats = new List<BlueprintFeature>();
            UnitDescriptor unit = base.Owner;
            foreach (var abl in SchoolUnderstanding.school1_abl_list) {
                if (unit.HasFact(abl)) {
                    sc = SchoolUnderstanding.school1_abl_dict[abl];
                    break;
                }
            }
            foreach (var kv in SchoolUnderstanding.school1_feat_neu_dict) {
                if (kv.Value == sc && !unit.HasFact(kv.Key)) {
                    TempFeats.Add(kv.Key);
                    unit.AddFact(kv.Key);
                }
            }
        }
        public override void OnTurnOff() {
            UnitDescriptor unit = base.Owner;
            foreach(var ft in TempFeats) {
                unit.RemoveFact(ft);
            }
            TempFeats.Clear();
            base.OnTurnOff();
        }
        List<BlueprintFeature> TempFeats;
        SpellSchool sc = SpellSchool.None;
    }
    */
}
