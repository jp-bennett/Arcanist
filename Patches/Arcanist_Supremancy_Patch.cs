using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony12;
using Kingmaker.UnitLogic.Abilities;
using ArcaneTide;
using ArcaneTide.Arcanist;
namespace ArcaneTide.Patches {
    [HarmonyPatch(typeof(AbilityData), "Spend", new Type[] { })]
    class AbilityData_Spend_Patch {
        static public bool Prefix(AbilityData __instance) {
            if (__instance.Spellbook == null) return true;
            if(__instance.Spellbook.Blueprint.CharacterClass == Main.arcanist) {
                if(Supremancy.buff == null) {
                    UnityModManagerNet.UnityModManager.Logger.Log("fuck! buff is null!");
                    return true;
                }
                UnityModManagerNet.UnityModManager.Logger.Log($"buff1 name is {Supremancy.buff.Name}");
                UnityModManagerNet.UnityModManager.Logger.Log($"buff2 name is {Supremancy.buff2.Name}");
                if (__instance.Caster.Buffs.HasFact(Supremancy.buff) && __instance.Caster.Buffs.HasFact(Supremancy.buff2)) {
                    __instance.SpendMaterialComponent();
                    __instance.Caster.Buffs.RemoveFact(Supremancy.buff);
                    __instance.Caster.Buffs.RemoveFact(Supremancy.buff2);
                    return false;
                }
                
            }
            return true;
        }
    }
}
