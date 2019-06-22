using Kingmaker.RuleSystem;
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
}
