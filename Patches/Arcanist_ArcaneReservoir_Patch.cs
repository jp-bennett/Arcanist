using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcaneTide.Arcanist;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic;

namespace ArcaneTide.Patches {
    [HarmonyPatch(typeof(UnitAbilityResourceCollection), "Restore", new Type[] { typeof(BlueprintScriptableObject)})]
    class Arcanist_ArcaneReservoir_Patch {
        static public void Postfix(UnitAbilityResourceCollection __instance, BlueprintScriptableObject blueprint, ref UnitDescriptor ___m_Owner) {
            if ((blueprint as BlueprintAbilityResource) == ArcaneReservoir.resource) {
                int arcanist_level = ___m_Owner.Progression.GetClassLevel(ArcanistClass.arcanist);
                __instance.Spend(blueprint, arcanist_level / 2 + arcanist_level % 2);
            }
            //resource used as special counters. Should not be restored after rest.
            if ((blueprint as BlueprintAbilityResource) == SpellResistanceDrain.drained_arcane_points_res) {
                __instance.Spend(blueprint, 5);
            }
        }
    }
}
