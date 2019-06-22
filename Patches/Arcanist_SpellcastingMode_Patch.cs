using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony12;
using UnityModManagerNet;
using Kingmaker.UI.ActionBar;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.Items.Slots;
using Kingmaker.UnitLogic;
using Kingmaker.UI.ServiceWindow;
using Kingmaker;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Stats;
using UnityEngine;
using Kingmaker.Blueprints.Root;
using TMPro;
using UnityEngine.UI;
using Kingmaker.Utility;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;

namespace ArcaneTide.Patches {
    
    static class ArcanistPatch_Helpers {
        static public int getArcanistMemorizeSlotCnt(int spellLevel) {
            Spellbook spellBook = Game.Instance.UI.SpellBookController.CurrentSpellbook;
            BlueprintSpellbook spellBookBlueprint = spellBook.Blueprint;
            ModifiableValueAttributeStat castingStat = spellBook.Owner.Stats.GetStat<ModifiableValueAttributeStat>(spellBookBlueprint.CastingAttribute);
            if (castingStat == null) return 0;
            if (castingStat < 10 + spellLevel) return 0;
            if (spellLevel < 0 || spellLevel > spellBook.MaxSpellLevel) return 0;
            return spellBookBlueprint.SpellsKnown.GetCount(spellBook.CasterLevel+20, spellLevel).Value;
        }
    }
    /*
    [HarmonyPatch(typeof(Spellbook), "SpendInternal")]
    class Spellbook_SpendInternal_Patch {

    }
    [HarmonyPatch(typeof(Spellbook), "GetAvailableForCastSpellCount", new Type[] { typeof(AbilityData)})]
    class Spellbook_GetAvailableForCastSpellCount_Patch {
        static public bool Prefix(Spellbook __instance, AbilityData spell, int __result) {
            if(__instance.Blueprint.CharacterClass != Main.arcanist) {
                return true;
            }
            int spellLevel = __instance.GetSpellLevel(spell);
            if (spellLevel < 0) {
                __result = 0;
                return false;
            }
            __result = __instance.GetSpontaneousSlots(spellLevel);
            return false;
        }
    }
    */
    
    //Fix metamagiced-spells' casting time.
    [HarmonyPatch(typeof(AbilityData), "RequireFullRoundAction", MethodType.Getter)]
    class AbilityData_RequireFullRoundAction_Getter_Patch {
        static public bool Prefix(AbilityData __instance, bool __result) {
            bool result;
            if (__instance.ActionType == UnitCommand.CommandType.Standard) {
                Spellbook spellbook = __instance.Spellbook;
                bool? flag = (spellbook != null) ? new bool?(spellbook.Blueprint.Spontaneous && spellbook.Blueprint.CharacterClass != Main.arcanist) : null;
                if (flag != null && flag.Value) {
                    MetamagicData metamagicData = __instance.MetamagicData;
                    bool? flag2 = (metamagicData != null) ? new bool?(metamagicData.NotEmpty) : null;
                    if (flag2 != null && flag2.Value) {
                        __result = true;
                        return false;
                    }
                }
                __result = __instance.Blueprint.IsFullRoundAction;
                return false;
            }
            else {
                __result = false;
                return false;
            }
        }
    }
    
    [HarmonyPatch(typeof(Spellbook), "Rest", new Type[] { })]
    class Spellbook_Rest_Patch {
        static public bool Prefix(Spellbook __instance, ref int[] ___m_SpontaneousSlots, ref List<SpellSlot>[] ___m_MemorizedSpells) {
            if (__instance.Blueprint.CharacterClass != Main.arcanist) {
                return true;
            }
            __instance.UpdateAllSlotsSize(true);
            for (int i = 0; i <= 9; i++) {
                ___m_SpontaneousSlots[i] = __instance.GetSpellsPerDay(i);
            }
            ___m_MemorizedSpells.SelectMany((List<SpellSlot> list) => list).ForEach(delegate (SpellSlot m) {
                m.Available = true;
            });
            return false;
        }
    }
    
    [HarmonyPatch(typeof(Spellbook), "ForgetMemorized", new Type[] { typeof(SpellSlot)})]
    class Spellbook_ForgetMemorized_Patch {
        static public bool Prefix(Spellbook __instance, SpellSlot spell) {
            if (__instance.Blueprint.CharacterClass == Main.arcanist) {
                __instance.Blueprint.Spontaneous = false;
            }
            return true;
        }
        static public void Postfix(Spellbook __instance) {
            if (__instance.Blueprint.CharacterClass == Main.arcanist) {
                __instance.Blueprint.Spontaneous = true;
            }
        }
    }
    
    [HarmonyPatch(typeof(Spellbook), "Memorize", new Type[] {typeof(AbilityData), typeof(SpellSlot) })]
    class Spellbook_Memorize_Patch {
        static public bool Prefix(Spellbook __instance) {
            if(__instance.Blueprint.CharacterClass == Main.arcanist) {
                __instance.Blueprint.Spontaneous = false;
            }
            return true;
        }
        static public void Postfix(Spellbook __instance) {
            if(__instance.Blueprint.CharacterClass == Main.arcanist) {
                __instance.Blueprint.Spontaneous = true;
            }
        }
    }
    
    [HarmonyPatch(typeof(SpellBookMemorizingPanel), "SetUp", new Type[] { })]
    class SpellBookMemorizingPanel_SetUp_Patch {
        static public bool Prefix(SpellBookMemorizingPanel __instance, ref bool ___m_IsInit, ref List<SpellSlotItem> ___m_CommonSpellSlots, ref SpellSlotItem[] ___m_SpecialSpellSlots, ref Transform ___m_SpecialSlotsContainer, ref TextMeshProUGUI ___m_LabelSpecialSlots, ref Image ___m_SpecialSlotsBackground) {
            // *******************
            // This check: check if this spellbook belongs to arcanist.
            // *******************
            if(Game.Instance.UI.SpellBookController.CurrentSpellbook.Blueprint.CharacterClass != Main.arcanist) {
                return true;
            }
            
            var spellBookController = Game.Instance.UI.SpellBookController;
            var spellBook = spellBookController.CurrentSpellbook;
            var currentLevel = spellBookController.CurrentBookLevel;
            var t = Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.SpellBookTexts;
            FastInvoke SubstituteText_Invoker = Helpers.CreateInvoker<SpellBookMemorizingPanel>("SubstituteText");

            spellBook.Blueprint.Spontaneous = false;

            if (!___m_IsInit) {
                return false;
            }
            bool returnValue = (bool)SubstituteText_Invoker(__instance);
            UnityModManager.Logger.Log($"SubstituteText invoker returns {returnValue}");
            if (returnValue) {
                return false;
            }
            if (currentLevel < 0 || currentLevel > spellBookController.MaxCasterLevel) {
                return false;
            }
            if (spellBookController.CurrentSpellbook == null) {
                return false;
            }
            foreach (SpellSlotItem spellSlotItem in ___m_CommonSpellSlots) {
                spellSlotItem.DeactivateSlotItem();
                spellSlotItem.SetActive(true);
            }
            foreach (SpellSlotItem spellSlotItem2 in ___m_SpecialSpellSlots) {
                spellSlotItem2.DeactivateSlotItem();
                spellSlotItem2.SetActive(false);
            }
            //
            // *********************
            // Patch Start
            // [num] controls how many normal slot of tier [currentLevel] spell has.
            // For arcanists, we have to switch this to her known spell slot number.
            // *********************
            int num;
            num = ArcanistPatch_Helpers.getArcanistMemorizeSlotCnt(currentLevel);
            //num = spellBookController.CurrentSpellbook.CalcSlotsLimit(currentLevel, SpellSlotType.Common);
            // *********************
            // Patch End
            // *********************
            int num2 = spellBookController.CurrentSpellbook.CalcSlotsLimit(currentLevel, SpellSlotType.Favorite);
            foreach (SpellSlotItem spellSlotItem3 in ___m_CommonSpellSlots) {
                spellSlotItem3.DeactivateSlotItem();
            }
            int[] array = new int[]
            {
                0,
                0,
                0
            };
            int num3 = (num <= 8) ? 2 : 3;
            int num4 = (int)Mathf.Floor((float)(num / num3));
            int num5 = num % num3;
            if (num > 8) {
                array[1] = num4 + num5;
                array[0] = num4;
                array[2] = num4;
            }
            else if (num > 1) {
                array[0] = num4 + num5;
                array[1] = num4;
            }
            else if (num > 0) {
                array[0] = 1;
            }
            int num6 = 0;
            foreach (int num7 in array) {
                for (int k = 0; k < num7; k++) {
                    int index = num6 * ___m_CommonSpellSlots.Count / 3 + k;
                    ___m_CommonSpellSlots[index].ActivateSlotItem(SpellSlotType.Common);
                }
                num6++;
            }
            if (___m_SpecialSpellSlots.Length > 0 && num2 > 0) {
                ___m_SpecialSlotsContainer.gameObject.SetActive(true);
                if (spellBookController.CurrentSpellbook.Blueprint.CantripsType == CantripsType.Orisions) {
                    ___m_LabelSpecialSlots.text = t.DomainSlots;
                    ___m_SpecialSlotsBackground.color = BlueprintRoot.Instance.UIRoot.SpellBookColors.DomainSlotColor;
                }
                else {
                    ___m_LabelSpecialSlots.text = t.FavoriteSchoolSlots;
                    ___m_SpecialSlotsBackground.color = BlueprintRoot.Instance.UIRoot.SpellBookColors.FavoriteSlotColor;
                }
                foreach (SpellSlotItem spellSlotItem4 in ___m_SpecialSpellSlots) {
                    if (num2 <= 0) {
                        break;
                    }
                    SpellSlotType type = (spellBookController.CurrentSpellbook.Blueprint.CantripsType != CantripsType.Orisions) ? SpellSlotType.Favorite : SpellSlotType.Domain;
                    spellSlotItem4.ActivateSlotItem(type);
                    num2--;
                }
            }
            else {
                ___m_SpecialSlotsContainer.gameObject.SetActive(false);
            }
            IEnumerable<SpellSlot> memorizedSpellSlots = spellBookController.CurrentSpellbook.GetMemorizedSpellSlots(currentLevel);
            foreach (SpellSlot spellMechanicSlot in memorizedSpellSlots) {
                __instance.AddSlot(spellMechanicSlot);
            }

            spellBook.Blueprint.Spontaneous = true;
            return false;
        }

        
    }
    [HarmonyPatch(typeof(ActionBarGroupElement), "FillSlots", new Type[] { })]
    class ActionBarGroupElement_FillSlots_Patch {
        static public bool Prefix(ActionBarGroupElement __instance, ref UnitEntityData ___m_Selected, ref ActionBarSubGroupLevels ___m_Levels) {
            if (!__instance.PreInit) {
                __instance.Dispose();
            }
            int index = 0;
            ActionBarSlotType slotType = __instance.SlotType;
            if (slotType != ActionBarSlotType.Spell) {
                /*
                if (slotType != ActionBarSlotType.Item) {
                    if (slotType == ActionBarSlotType.ActivatableAbility) {
                        foreach (Ability ability in ___m_Selected.Abilities) {
                            if (!ability.Hidden && !ability.Blueprint.IsCantrip) {
                                ActionBarGroupSlot slot = __instance.GetSlot(index++);
                                slot.Set(___m_Selected, new MechanicActionBarSlotAbility {
                                    Ability = ability.Data,
                                    Unit = ___m_Selected
                                });
                            }
                        }
                        foreach (ActivatableAbility activatableAbility in ___m_Selected.ActivatableAbilities) {
                            ActionBarGroupSlot slot2 = __instance.GetSlot(index++);
                            slot2.Set(___m_Selected, new MechanicActionBarSlotActivableAbility {
                                ActivatableAbility = activatableAbility,
                                Unit = ___m_Selected
                            });
                        }
                    }
                }
                else {
                    foreach (UsableSlot usableSlot in ___m_Selected.Body.QuickSlots) {
                        ActionBarGroupSlot slot3 = __instance.GetSlot(index++);
                        if (usableSlot.HasItem) {
                            slot3.Set(___m_Selected, new MechanicActionBarSlotItem {
                                Item = usableSlot.Item,
                                Unit = ___m_Selected
                            });
                        }
                        else {
                            slot3.Set(___m_Selected, new MechanicActionBarSlotEmpty());
                        }
                    }
                }
                __instance.AddEmptySlots(index);
                return;
                */
                return true;
            }
            if (___m_Levels != null) {
                ___m_Levels.Clear();
            }
            List<AbilityData> list = new List<AbilityData>();
            foreach (Ability ability2 in ___m_Selected.Abilities) {
                if (!ability2.Hidden && ability2.Blueprint.IsCantrip) {
                    AbilityData data = ability2.Data;
                    if (!list.Contains(data)) {
                        list.Add(data);
                        if (___m_Levels != null) {
                            ___m_Levels.AddSlot(0, new MechanicActionBarSlotAbility {
                                Ability = data,
                                Unit = ___m_Selected
                            });
                        }
                    }
                }
            }
            foreach (Spellbook spellbook in ___m_Selected.Descriptor.Spellbooks) {
                if (spellbook.Blueprint.Spontaneous) {
                    
                    if (spellbook.Blueprint.CharacterClass == Main.arcanist) {
                        
                        for (int i = 1; i <= spellbook.MaxSpellLevel; i++) {
                            IEnumerable<SpellSlot> memorizedSpells = spellbook.GetMemorizedSpells(i);
                            foreach(SpellSlot spellSlot in memorizedSpells) {
                                if (!spellSlot.Available) continue;
                                AbilityData abilityData = spellSlot.Spell;
                                if (!list.Contains(abilityData)) {
                                    list.Add(abilityData);
                                    if (___m_Levels != null) {
                                        ___m_Levels.AddSlot(i, new MechanicActionBarSlotSpontaneusSpell {
                                            Spell = abilityData,
                                            Unit = ___m_Selected
                                        });
                                    }
                                }
                            }
                        }
                    }
                    
                    else {
                        for (int i = 1; i <= spellbook.MaxSpellLevel; i++) {
                            List<AbilityData> list2 = spellbook.GetSpecialSpells(i).Concat(spellbook.GetKnownSpells(i)).Distinct<AbilityData>().ToList<AbilityData>();
                            List<AbilityData> collection = spellbook.GetCustomSpells(i).ToList<AbilityData>();
                            List<AbilityData> list3 = list2;
                            list3.AddRange(collection);
                            foreach (AbilityData abilityData in list3) {
                                if (!list.Contains(abilityData)) {
                                    list.Add(abilityData);
                                    if (___m_Levels != null) {
                                        ___m_Levels.AddSlot(i, new MechanicActionBarSlotSpontaneusSpell {
                                            Spell = abilityData,
                                            Unit = ___m_Selected
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
                else {
                    //Added codes start. Arcanist use their memorized spells as known spells.
                    if (1==0/*spellbook.Blueprint.CharacterClass == Main.arcanist*/) {
                        UnityModManager.Logger.Log("Rua spellbook is arcanist!");
                        for (int i = 1; i <= spellbook.MaxSpellLevel; i++) {
                            IEnumerable<SpellSlot> memorizedSpells = spellbook.GetMemorizedSpells(i);
                            foreach (SpellSlot spellSlot in memorizedSpells) {
                                if (!spellSlot.Available) continue;
                                AbilityData abilityData = spellSlot.Spell;
                                if (!list.Contains(abilityData)) {
                                    list.Add(abilityData);
                                    if (___m_Levels != null) {
                                        ___m_Levels.AddSlot(i, new MechanicActionBarSlotSpontaneusSpell {
                                            Spell = abilityData,
                                            Unit = ___m_Selected
                                        });
                                    }
                                }
                            }
                        }
                    }
                    //Added codes end.
                    else {
                        for (int j = 0; j <= spellbook.MaxSpellLevel; j++) {
                            IEnumerable<SpellSlot> memorizedSpells = spellbook.GetMemorizedSpells(j);
                            foreach (SpellSlot spellSlot in memorizedSpells) {
                                if (!list.Contains(spellSlot.Spell)) {
                                    list.Add(spellSlot.Spell);
                                    if (___m_Levels != null) {
                                        ___m_Levels.AddSlot(j, new MechanicActionBarSlotMemorizedSpell {
                                            SpellSlot = spellSlot,
                                            Unit = ___m_Selected
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (___m_Levels != null) {
                ___m_Levels.SetType(__instance.SlotType);
            }
            return false;
        }
    }
}
