using ArcaneTide.Utils;
using ArcaneTide.Components;
using ArcaneTide.Arcanist;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Enums.Damage;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Utility;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Mechanics.Properties;
using Harmony12;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UI.ActionBar;
using Kingmaker.UI.UnitSettings;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.UI.Constructor;
using UnityModManagerNet;
using Kingmaker.EntitySystem.Entities;

namespace ArcaneTide.Patches {
    [HarmonyPatch(typeof(AbilityData), "ActionType", MethodType.Getter)]
    class AbilityData_ActionType_Getter_Patch {

        public static bool Prefix(AbilityData __instance, ref UnitCommand.CommandType __result) {
            BlueprintAbility abl = __instance.Blueprint;
            UnitDescriptor unit = __instance.Caster;
            if(ConsumeSpells.ablList.Contains(abl) || abl == ConsumeItem.abl) {
                if (unit.HasFact(SwiftConsume.exploit)) {
                    __result = UnitCommand.CommandType.Swift;
                    return false;
                }
            }
            return true;
        }
        public static void Postfix(AbilityData __instance, ref UnitCommand.CommandType __result) {
           // Main.logger.Log($"Ability {__instance.Name} has action type of {__result}");
            //Main.logger.Log($"Ability {__instance.Name} has metamagic {(__instance.MetamagicData == null ? 0 : (int)__instance.MetamagicData.MetamagicMask)}");
        }
    }
    [HarmonyPatch(typeof(ActionBarGroupSlot),"SetSpontaneousControls")]
    class ActionBarGroupSlot_SetSpontaneousControls_Patch {
        public static void Postfix(ActionBarGroupSlot __instance, ref List<AbilityData> ___Conversion, ref ButtonPF ___ToggleAdditionalSpells) {
            MechanicActionBarSlotItem mechanicActionBarSlotItem = __instance.MechanicSlot as MechanicActionBarSlotItem;
            if (mechanicActionBarSlotItem != null) {
                //UnityModManager.Logger.Log("Rua 1");
                var itemType = mechanicActionBarSlotItem.Item.Blueprint.Type;
                int spellLevel = mechanicActionBarSlotItem.Item.Blueprint.SpellLevel;
                Ability ability = mechanicActionBarSlotItem.Item.Ability;
                //UnityModManager.Logger.Log("Rua 2");
                if (spellLevel >= 2 && ability != null && (itemType == UsableItemType.Potion || itemType == UsableItemType.Scroll || itemType == UsableItemType.Wand)) {
                    
                    //UnityModManager.Logger.Log("Rua 3");
                    var unit = mechanicActionBarSlotItem.Item.Owner;
                    //UnityModManager.Logger.Log("Rua 4");
                    foreach (Ability _ability in unit.Abilities) {
                        
                        if (_ability.Blueprint.GetComponent<ConsumeItemComponent>() != null && mechanicActionBarSlotItem.Item.Ability != null) {
                            //UnityModManager.Logger.Log("Rua 5");
                            AbilityData abld = new AbilityData(_ability) {
                                ParamSpellSlot = new SpellSlot {
                                    Spell = new AbilityData(ability)
                                }
                                //PotionForOther = true
                            };
                            //UnityModManager.Logger.Log("Rua 6");
                            if (mechanicActionBarSlotItem.Item.Blueprint.Type == UsableItemType.Potion) {
                                // if it's potion, ___Conversion has become  { [give others potion ability]} before this Postfix.
                                ___Conversion.Add(abld);
                            }
                            else {
                                //elsewise, create a new ___Conversion list.
                                ___Conversion = new List<AbilityData>() { abld };
                            }
                            //UnityModManager.Logger.Log($"Rua 7: Conversion has {___Conversion.Count} elements");
                            //UnityModManager.Logger.Log("Rua 8");
                            if (___ToggleAdditionalSpells != null) {
                                ___ToggleAdditionalSpells.gameObject.SetActive(true);
                            }
                            //UnityModManager.Logger.Log("Rua 9");
                        }
                    }
                    


                }
            }
        }
    }
    [HarmonyPatch(typeof(ActionBarGroupSlot),"OnToggleGroupClick")]
    class ActionBarGroupSlot_OnToggleGroupClick_Patch {
        static public void Postfix(ActionBarGroupSlot __instance, ref ActionBarSpellsGroup ___SubGroup, ref UnitEntityData ___Selected, ref List<AbilityData> ___Conversion) {
            MechanicActionBarSlotItem mechanicActionBarSlotItem = __instance.MechanicSlot as MechanicActionBarSlotItem;
            if (mechanicActionBarSlotItem != null && (mechanicActionBarSlotItem.Item.Blueprint.Type == UsableItemType.Scroll || mechanicActionBarSlotItem.Item.Blueprint.Type == UsableItemType.Wand)) {
                Ability ability = mechanicActionBarSlotItem.Item.Ability;
                if (ability != null) {
                    if (___SubGroup != null) {
                        ___SubGroup.Toggle(___Selected, ___Conversion, ability.Data);
                    }
                }
            }
        }
    }
}
