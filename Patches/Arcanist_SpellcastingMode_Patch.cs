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
using ArcaneTide.Arcanist;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic.Commands;

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
        static public int getArcanistMemorizeSlotCnt(int spellLevel, Spellbook spellBook) {
            
            BlueprintSpellbook spellBookBlueprint = spellBook.Blueprint;
            ModifiableValueAttributeStat castingStat = spellBook.Owner.Stats.GetStat<ModifiableValueAttributeStat>(spellBookBlueprint.CastingAttribute);
            if (castingStat == null) return 0;
            if (castingStat < 10 + spellLevel) return 0;
            if (spellLevel < 0 || spellLevel > spellBook.MaxSpellLevel) return 0;
            return spellBookBlueprint.SpellsKnown.GetCount(spellBook.CasterLevel + 20, spellLevel).Value;
        }
    }
    //[HarmonyPatch(typeof(UnitUseAbility), "OnAction")]
    class Debug_UnitUseAbility {
        static public void Prefix(UnitUseAbility __instance) {
            Main.logger.Log($"DUA, use ability {__instance.Spell.Name}");
        }
    }
    //[HarmonyPatch(typeof(RuleCastSpell), "OnTrigger")]
    class Debug_RuleCastSpell {
        static public void Prefix(RuleCastSpell __instance) {
            Main.logger.Log($"rule cast spell");
        }
        static public void Postfix(RuleCastSpell __instance) {
            Main.logger.Log($"rule cast return success is {__instance.Success}");
        }
    }


    
    [HarmonyPatch(typeof(Spellbook), "SpendInternal")]
    static class Spellbook_SpendInternal_Patch {
        static public FastInvoke sureMemorizedSpells_Invoker = Helpers.CreateInvoker<Spellbook>("SureMemorizedSpells");
        static public List<SpellSlot> SureMemorizedSpellsBelowAndK(this Spellbook spellbook, int K) {
            List<SpellSlot> ans = new List<SpellSlot>();
            for(int i = 1; i <= K; i++) {
                List<SpellSlot> tmp = sureMemorizedSpells_Invoker(spellbook, i) as List<SpellSlot>;
                if (tmp == null) return null;
                ans.AddRange(tmp);
            }
            return ans;
        }
        static public bool Prefix(Spellbook __instance, ref bool __result, BlueprintAbility blueprint,  AbilityData spell, bool doSpend, bool excludeSpecial, ref int[] ___m_SpontaneousSlots, ref int? ___m_MaxSpellLevel) {
            
            if(__instance.Blueprint.CharacterClass != ArcanistClass.arcanist) {
                return true;
            }
            UnitDescriptor unit = __instance.Owner;

            if (doSpend && spell == null) {
                UberDebug.LogError("Trying to spend ability without specfying instance", Array.Empty<object>());
                __result = false;
                return false;
            }
            int num = (!(spell != null)) ? __instance.GetSpellLevel(blueprint) : __instance.GetSpellLevel(spell);
            //Main.logger.Log($"spell={spell.Name},num={num}");
            if (num < 0) {
                //Main.logger.Log("Return FALSE");
                __result = false;
                return false;
            }
            int? maxSpellLevel = ___m_MaxSpellLevel;
            if (maxSpellLevel != null && num >= ___m_MaxSpellLevel) {
                __result = false;
                return false;
            }
            int num2 = __instance.Owner.Stats.GetStat(__instance.Blueprint.CastingAttribute);
            if (num2 < 10 + num) {
                __result = false;
                return false;
            }
            
            

            List<SpellSlot> list = SureMemorizedSpellsBelowAndK(__instance, num);
            
            for (int i = list.Count - 1; i >= 0; i--) {
                SpellSlot spellSlot = list[i];
                
                int num3 = ___m_SpontaneousSlots[num];
                
                if (spellSlot.Available && (num3 > 0 ||(num3 == 0 && !doSpend))) {
           
                    if (spellSlot.Type != SpellSlotType.Favorite || !excludeSpecial) {
 
                        if (spell != null && spellSlot.Spell != null) {

                            int spellMeta = spell.MetamagicData == null ? 0 : (int)spell.MetamagicData.MetamagicMask;
                            int slotSpellMeta = spellSlot.Spell.MetamagicData == null ? 0 : (int)spell.MetamagicData.MetamagicMask;
                           
                            bool flag = ((spellMeta | slotSpellMeta) == spellMeta);//metamagic of spellSlot.Spell is a subset of metamagic of spell.
                            if (spell.Blueprint.Equals(spellSlot.Spell.Blueprint) && flag) {

                                if (doSpend) {
                                    ___m_SpontaneousSlots[num] = num3 - 1;
                                }
                                //Main.logger.Log($"RUA");
                                __result = true;
                                return false;
                            }
                        }
                        else if(spell == null){
                            AbilityData spell2 = spellSlot.Spell;
                            //Main.logger.Log($"RUA1");
                            if (blueprint == ((spell2 != null) ? spell2.Blueprint : null)) {
                                //Main.logger.Log($"RUA1.2");
                                __result = true;
                                return false;
                            }
                        }
                    }
                }
            }
            //Main.logger.Log("Dua???");
            __result = false;
            return false;
        }
    }
    /*
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
        static public bool Prefix(AbilityData __instance, ref bool __result) {
            bool result;
            if (__instance.ActionType == UnitCommand.CommandType.Standard) {
                Spellbook spellbook = __instance.Spellbook;
                bool? isSpontaneous = (spellbook != null) ? new bool?(spellbook.Blueprint.Spontaneous) : null;
                bool? isArcanist = (spellbook != null) ? new bool?(spellbook.Blueprint.CharacterClass == ArcanistClass.arcanist) : null;
                bool? isUsingSponMetamagic = (spellbook != null) ? new bool?(spellbook.Owner.HasFact(SponMetamagic.flagBuff)) : null;
                //UnityModManager.Logger.Log($"spontaneous  = {(isSpontaneous!=null ? (isSpontaneous.Value?"T":"F") : "null")}");
                //UnityModManager.Logger.Log($"arcanist  = {(isArcanist != null ? (isArcanist.Value ? "T" : "F") : "null")}");
                //UnityModManager.Logger.Log($"sponmeta  = {(isUsingSponMetamagic != null ? (isUsingSponMetamagic.Value ? "T" : "F") : "null")}");
                if (isSpontaneous != null && isSpontaneous.Value) {
                    MetamagicData metamagicData = __instance.MetamagicData;
                    bool? flag2 = (metamagicData != null) ? new bool?(metamagicData.NotEmpty) : null;
                    if (flag2 != null && flag2.Value) {
                        __result = !(isArcanist.Value && !(isUsingSponMetamagic.Value));
                        if (isUsingSponMetamagic.Value) spellbook.Owner.RemoveFact(SponMetamagic.flagBuff);
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
                Main.logger.Log($"Arcanist Try to forget spellslot {spell.Index}, spellslot is now available {spell.Available}");
                if (spell.Available) {
                    Main.logger.Log($"Spellslot contains {spell.Spell}");
                }
            }
            return true;
        }
        static public void Postfix(Spellbook __instance) {
            if (__instance.Blueprint.CharacterClass == Main.arcanist) {
                __instance.Blueprint.Spontaneous = true;
                Main.logger.Log("Forget success");
            }
        }
    }
    
    [HarmonyPatch(typeof(Spellbook), "Memorize", new Type[] {typeof(AbilityData), typeof(SpellSlot) })]
    class Spellbook_Memorize_Patch {
        static public bool Prefix(Spellbook __instance, AbilityData data, SpellSlot slot) {
            if(__instance.Blueprint.CharacterClass == Main.arcanist) {
                __instance.Blueprint.Spontaneous = false;
            }
            Main.logger.Log($"On memory spell {data.Name}, slot index {((slot == null) ? -666 : slot.Index)}");
            return true;
        }
        static public void Postfix(Spellbook __instance) {
            if(__instance.Blueprint.CharacterClass == Main.arcanist) {
                for(int i = 1; i <= __instance.MaxSpellLevel; i++) {
                    var memorizedSlots = __instance.GetMemorizedSpellSlots(i);
                    int j = 0;
                    foreach(SpellSlot slot in memorizedSlots) {
                        int ind = slot.Index;
                        if(slot.Type == SpellSlotType.Common && ind >= ArcanistPatch_Helpers.getArcanistMemorizeSlotCnt(i, __instance)) {
                            slot.Available = false;
                            slot.Spell = null;
                            slot.LinkedSlots = null;
                            slot.IsOpposition = false;
                        }
                        j++;
                    }
                }
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
                                //the following 2 lines shouldn't be removed.
                                //these are for dealing with the bug appearing at new game:
                                //on new game, the system would automatically memorize (spellperday[1]) spells.
                                //however, arcanist's spell slot cnt on level 1 is fewer than her spell per day.
                                //so the spells memorized in an invalid spellslot would never be forgotten with normal means.
                                int ind = spellSlot.Index;
                                if (ind >= ArcanistPatch_Helpers.getArcanistMemorizeSlotCnt(spellSlot.SpellLevel, spellbook)) continue;

                                AbilityData abilityData = spellSlot.Spell;
                                abilityData.ParamSpellSlot = spellSlot;
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

    [HarmonyPatch(typeof(ApplySpellbook), "Apply", new Type[] { typeof(LevelUpState), typeof(UnitDescriptor)})]
    class ApplySpellbook_Apply_Patch {
        static private FastInvoke ApplySpellbook_TryApplyCustomSpells = Helpers.CreateInvoker<ApplySpellbook>("TryApplyCustomSpells");
        static public bool Prefix(ApplySpellbook __instance, LevelUpState state, UnitDescriptor unit) {
            if(state.SelectedClass != ArcanistClass.arcanist) {
                return true;
            }

            if (state.SelectedClass == null) {
                return false;
            }
            SkipLevelsForSpellProgression component = state.SelectedClass.GetComponent<SkipLevelsForSpellProgression>();
            if (component != null && component.Levels.Contains(state.NextClassLevel)) {
                return false;
            }
            ClassData classData = unit.Progression.GetClassData(state.SelectedClass);
            if (classData == null) {
                return false;
            }
            if (classData.Spellbook != null) {
                Spellbook spellbook = unit.DemandSpellbook(classData.Spellbook);
                if (state.SelectedClass.Spellbook && state.SelectedClass.Spellbook != classData.Spellbook) {
                    Spellbook spellbook2 = unit.Spellbooks.FirstOrDefault((Spellbook s) => s.Blueprint == state.SelectedClass.Spellbook);
                    if (spellbook2 != null) {
                        foreach (AbilityData abilityData in spellbook2.GetAllKnownSpells()) {
                            spellbook.AddKnown(abilityData.SpellLevel, abilityData.Blueprint, false);
                        }
                        unit.DeleteSpellbook(state.SelectedClass.Spellbook);
                    }
                }
                int casterLevel = spellbook.CasterLevel;
                spellbook.AddCasterLevel();
                int casterLevel2 = spellbook.CasterLevel;
                SpellSelectionData spellSelectionData = state.DemandSpellSelection(spellbook.Blueprint, spellbook.Blueprint.SpellList);
                if (spellbook.Blueprint.SpellsKnown != null && 1 == 0) {
                    // SelectedClass must be arcanist, arcanist requires wizard-like spell selection, 
                    // while its SpellsKnown is not null
                    for (int i = 0; i <= 9; i++) {
                        int? count = spellbook.Blueprint.SpellsKnown.GetCount(casterLevel, i);
                        int num = (count == null) ? 0 : count.Value;
                        int? count2 = spellbook.Blueprint.SpellsKnown.GetCount(casterLevel2, i);
                        int num2 = (count2 == null) ? 0 : count2.Value;
                        spellSelectionData.SetLevelSpells(i, num2 - num);
                    }
                }
                int maxSpellLevel = spellbook.MaxSpellLevel;
                if (spellbook.Blueprint.SpellsPerLevel > 0) {
                    if (casterLevel == 0) {
                        spellSelectionData.SetExtraSpells(0, maxSpellLevel);
                        spellSelectionData.ExtraByStat = true;
                        spellSelectionData.UpdateMaxLevelSpells(unit);
                    }
                    else {
                        spellSelectionData.SetExtraSpells(spellbook.Blueprint.SpellsPerLevel, maxSpellLevel);
                    }
                }
                foreach (AddCustomSpells customSpells in spellbook.Blueprint.GetComponents<AddCustomSpells>()) {
                    ApplySpellbook_TryApplyCustomSpells(__instance, spellbook, customSpells, state, unit);
                }
                
            }
            return false;
        }
    }
}
