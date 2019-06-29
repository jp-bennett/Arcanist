using Kingmaker.UnitLogic.Abilities.Blueprints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcaneTide.Spells {
    static class SpellsNonPFMonitor {
        static List<BlueprintAbility> modSpells_NonPF;
        static public void Create() {
            modSpells_NonPF = new List<BlueprintAbility>();
        }
    }

    static class Spells1 {
        static public BlueprintAbility MissileStorm;
        static bool loaded = false;
        static public void Create() {
        }
    }
}
