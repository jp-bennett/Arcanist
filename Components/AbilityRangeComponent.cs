using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcaneTide.Components {
    class AbilityRangeComponent : BlueprintComponent {
        public Feet Calc(UnitDescriptor unit) {
            float ans = 0;
            foreach(var cls in classes) {
                ans += unit.Progression.GetClassLevel(cls) * multiplier;
            }
            return new Feet(ans);
        }
        public BlueprintCharacterClass[] classes;
        public float multiplier;
    }
}
