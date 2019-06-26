using ArcaneTide.Arcanist;
using ArcaneTide.Utils;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Items;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityModManagerNet;
using static Kingmaker.UI.Common.ItemsFilter;

namespace ArcaneTide.Components {
    class ConsumeItemComponent : AbilityApplyEffect, IAbilityAvailabilityProvider, IAbilityParameterRequirement {
        public bool RequireSpellSlot {
            get {
                return true;
            }
        }

        public bool RequireSpellbook {
            get {
                return false;
            }
        }

        public bool RequireSpellLevel {
            get {
                return false;
            }
        }

        public override void Apply(AbilityExecutionContext context, TargetWrapper target) {
            UnitDescriptor unit = context.MaybeOwner.Descriptor;
            AbilityData spell = context.Ability.ParamSpellSlot.Spell;
            ItemEntity item = spell.SourceItem;
            BlueprintItemEquipmentUsable blueprintItm = spell.SourceItemUsableBlueprint;
            int spellLevel = blueprintItm.SpellLevel;
            unit.Resources.Restore(ArcaneReservoir.resource, spellLevel / 2);
            if(blueprintItm.Type == UsableItemType.Wand) {
                for (int i = 1; i <= 5; i++) {
                    item.SpendCharges(unit);
                }
            }
            else {
                item.SpendCharges(unit);
            }
        }

        public string GetReason() {
            return string.Empty;
        }

        public bool IsAvailableFor(AbilityData ability) {
            //UnityModManager.Logger.Log("Enter.00");
            UnityModManager.Logger.Log("Rua Rua 1");
            var unit = ability.Caster;
            UnityModManager.Logger.Log("Rua Rua 1.5");
            if (ability == null || !unit.HasFact(ArcaneReservoir.reservoir)) {
                return false;
            }
            UnityModManager.Logger.Log("Rua Rua 2");
            SpellSlot spellSlot = ability.ParamSpellSlot;
            UnityModManager.Logger.Log("Rua Rua 2.5");
            AbilityData spell = (spellSlot != null) ? spellSlot.Spell : null;
            UnityModManager.Logger.Log("Rua Rua 3");
            ItemEntity item = (spell != null) ? spell.SourceItem : null;
            UnityModManager.Logger.Log("Rua Rua 3.5");
            BlueprintItemEquipmentUsable blueprintItm = (item != null) ? (item.Blueprint as BlueprintItemEquipmentUsable) : null;
            UnityModManager.Logger.Log("Rua Rua 4");
            if (blueprintItm != null) {
                UnityModManager.Logger.Log("Rua Rua 5");
                if (blueprintItm.Type == UsableItemType.Scroll) {
                    int spellLevel = blueprintItm.SpellLevel;
                    return spellLevel >= 2;
                }
                if(blueprintItm.Type == UsableItemType.Wand) {
                    int spellLevel = blueprintItm.SpellLevel;
                    int charges = item.Charges;
                    return spellLevel >= 2 && charges >= 5;
                }
                if(blueprintItm.Type == UsableItemType.Potion) {
                    int spellLevel = blueprintItm.SpellLevel;
                    return spellLevel >= 2;
                }
                return false;
            }
            else return false;
        }
    }
}
