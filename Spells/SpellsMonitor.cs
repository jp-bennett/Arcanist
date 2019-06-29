using Kingmaker.UnitLogic.Abilities.Blueprints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcaneTide.Spells {
    static class SpellsMonitor {
        static List<BlueprintAbility> modSpells;
        static public void Create() {
            modSpells = new List<BlueprintAbility>();
        }
    }
}
