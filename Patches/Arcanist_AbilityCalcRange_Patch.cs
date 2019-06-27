using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcaneTide.Components;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;

namespace ArcaneTide.Patches {
    [HarmonyPatch(typeof(AbilityData), "GetApproachDistance")]
    class AbilityData_GetApproachDistance_Patch {
        static public void Postfix(AbilityData __instance, ref float __result) {
            UnitDescriptor unit = __instance.Caster;
            if (unit != null) {
                var comp = __instance.Blueprint.GetComponent<AbilityRangeComponent>();
                if (comp != null) {
                    __result = comp.Calc(unit).Meters;
                    return;
                }
            }
        }
    }
    [HarmonyPatch(typeof(AbilityData), "GetVisualDistance")]
    class AbilityData_GetVisualDistance_Patch {
        static public void Postfix(AbilityData __instance, ref float __result) {
            UnitDescriptor unit = __instance.Caster;
            if (unit != null) {
                float corpulence = __instance.Caster.Unit.View.Corpulence;
                var comp = __instance.Blueprint.GetComponent<AbilityRangeComponent>();
                if (comp != null) {
                    __result = comp.Calc(unit).Meters + corpulence + 0.5f;
                    return;
                }
            }
        }
    }
}
