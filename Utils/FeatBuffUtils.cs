using Kingmaker.Blueprints;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ArcaneTide;
using Kingmaker.Blueprints.Classes;

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

    static class IconSet {
        static internal LibraryScriptableObject library => Main.library;
        static public Sprite spell_strike_icon;
        static public Sprite vanish_icon;
        static public void Load() {
            spell_strike_icon = library.Get<BlueprintFeature>("be50f4e97fff8a24ba92561f1694a945").Icon;
            vanish_icon = Helpers.GetIcon("f001c73999fb5a543a199f890108d936");
        }
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
