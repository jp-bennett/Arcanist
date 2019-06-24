using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcaneTide.Components {
    class AddClassLevelToRankBonus : RuleInitiatorLogicComponent<RuleCalculateAbilityParams> {
        public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt) {
            if (evt.Spell != Spell) return;
            UnitDescriptor unit = base.Owner;
            int classLevel = unit.Progression.GetClassLevel(Class);
            evt.AddBonusCasterLevel((int)(MultiplierClassLevel * classLevel) + Bonus);
        }

        public override void OnEventDidTrigger(RuleCalculateAbilityParams evt) {
            
        }
        public BlueprintAbility Spell;
        public BlueprintCharacterClass Class;
        public double MultiplierClassLevel=1.0;
        public int Bonus=0;
    }
}
