using Kingmaker.Blueprints;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcaneTide.Utils {
    static class PresetDurations {

        static public ContextDurationValue threeRounds = new ContextDurationValue {
            Rate = DurationRate.Rounds,
            DiceType = DiceType.Zero,
            DiceCountValue = new ContextValue {
                ValueType = ContextValueType.Simple,
                Value = 0
            },
            BonusValue = new ContextValue {
                ValueType = ContextValueType.Simple,
                Value = 3
            }
        };
    }

    static class ResourceManagement {
        static public void RestoreResource(UnitDescriptor unit, BlueprintAbilityResource blueprint, int amount) {
            unit.Resources.Restore(blueprint, amount);
        }
        static public void SpendResource(UnitDescriptor unit, BlueprintAbilityResource blueprint, int amount) {
            unit.Resources.Spend(blueprint, amount);
        }
    }
}
