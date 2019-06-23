using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using ArcaneTide.Arcanist;
using ArcaneTide.Utils;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic;
using Harmony12;
using Kingmaker.UI.ActionBar;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UI.UnitSettings;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UI.Constructor;
using UnityModManagerNet;
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace ArcaneTide.Components {
    class FastStudyComponent : AbilityApplyEffect, IAbilityAvailabilityProvider, IAbilityParameterRequirement {
        public bool RequireSpellSlot {
            get {
                return true;
            }
        }

        public bool RequireSpellbook {
            get {
                return true;
            }
        }

        public bool RequireSpellLevel {
            get {
                return true;
            }
        }

        public override void Apply(AbilityExecutionContext context, TargetWrapper target) {
            UnitDescriptor unit = context.MaybeOwner.Descriptor;
            AbilityData spell = context.Ability.ParamSpellSlot.Spell;
            Spellbook spellbook = context.Ability.ParamSpellbook;
            int spellLevel = context.Ability.ParamSpellLevel.Value;
            if (spellLevel < 1 || spellLevel > spellbook.MaxSpellLevel) return;
            foreach(var slot in spellbook.GetMemorizedSpells(spellLevel)) {
                if(slot.Spell == spell) {
                    spellbook.ForgetMemorized(slot);
                }
            }
            unit.AddBuff(flagBuff, unit.Unit);
            FastStudy.RefreshSubAbls(spellbook, spellLevel);
            FastStudy.AddMasterAbls(unit);
        }

        public string GetReason() {
            return string.Empty;
        }

        public bool IsAvailableFor(AbilityData ability) {
            UnityModManager.Logger.Log("Enter.00");
            if (ability == null) {
                UnityModManager.Logger.Log("Fuck!!!!!");
                return false;
            }
            UnityModManager.Logger.Log($"ability name is {ability.Name}");
            /*
            if (ability.Spellbook == null) return false;
            var spellbook = ability.Spellbook;
            var unit = spellbook.Owner;
            return CheckArcanist.isArcanist(spellbook);
            */
            SpellSlot spellSlot = ability.ParamSpellSlot;
            AbilityData spell = (spellSlot != null) ? spellSlot.Spell : null;
            Spellbook spellbook = (spell != null) ? spell.Spellbook : null;
            UnitDescriptor unit = (spellbook != null) ? spellbook.Owner : null;
            return (unit != null) && CheckArcanist.isArcanist(spellbook);
        }
        // shows if a Fast Study process is not finished.
        // buff is removed when the ArcanistClassExploitFastStudy{School}NewSpellAbl abilities are removed.
        // Fast study root ability should be disabled when flagBuff is on.
        public BlueprintBuff flagBuff; 
    }

    class FastStudyMemorizeAction : ContextAction {
        public override string GetCaption() {
            return $"memorize spell {spell.Name}";
        }

        public override void RunAction() {
            spellbook.Memorize(spell);
            foreach(var slot in spellbook.GetMemorizedSpells(spell.SpellLevel)) {
                if(slot.Spell == spell) {
                    slot.Available = true;
                }
            }
            FastStudy.RemoveMasterAbls(base.Target.Unit.Descriptor);
        }

        public AbilityData spell;
        public Spellbook spellbook;
    }
    [HarmonyPatch(typeof(ActionBarGroupSlot), "SetToggleAdditionalSpells", new Type[] { typeof(AbilityData)})]
    class ActionBarGroupSlot_SetToggleAdditionalSpells_Patch {
        static public void Postfix(ActionBarGroupSlot __instance, AbilityData spell, ref List<AbilityData> ___Conversion, ref ButtonPF ___ToggleAdditionalSpells) {
            if (spell == null) UnityModManager.Logger.Log("Rua! spel is null!!!");
            Spellbook spellbook = spell.Spellbook;
            if(spellbook != null) {
                UnityModManager.Logger.Log($"spellbook is {spellbook.Blueprint.Name}");
                MechanicActionBarSlotSpontaneusSpell mechanicActionBarSlotSpontaneusSpell = __instance.MechanicSlot as MechanicActionBarSlotSpontaneusSpell;
                SpellSlot spellSlot2 = (mechanicActionBarSlotSpontaneusSpell != null) ? mechanicActionBarSlotSpontaneusSpell.Spell.ParamSpellSlot : null;
                if (mechanicActionBarSlotSpontaneusSpell == null) UnityModManager.Logger.Log("mechanicABSSS is null");
                if (spellSlot2 == null) UnityModManager.Logger.Log("spellslot2 is null");
                if (spellSlot2 != null) {
                    UnityModManager.Logger.Log("miaomiao1!!!");
                    foreach (Ability ability2 in spell.Caster.Abilities) {
                        if (ability2.Blueprint.GetComponent<FastStudyComponent>()) {
                            UnityModManager.Logger.Log($"Rua, have fast study.");
                            UnityModManager.Logger.Log($"spell is {spell.Name}");
                            AbilityData item2 = new AbilityData(ability2) {
                                ParamSpellSlot = spellSlot2,
                                ParamSpellLevel = spell.SpellLevel,
                                ParamSpellbook = spellbook
                            };
                            ___Conversion.Add(item2);
                        }
                    }
                }
                UnityModManager.Logger.Log("miaomiao2!!!");
                BlueprintAbility spellBlueprint = spell.Blueprint;
                if (___Conversion.Any((AbilityData s) => s.Blueprint != spellBlueprint) || (spellBlueprint.Variants != null && spellBlueprint.Variants.Any<BlueprintAbility>())) {
                    if (___ToggleAdditionalSpells != null) {
                        ___ToggleAdditionalSpells.gameObject.SetActive(true);
                    }
                }
            }
        }
    }
}
