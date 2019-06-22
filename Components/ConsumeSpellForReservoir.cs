using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Localization;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcaneTide.Components {
    class ConsumeSpellForReservoirAction : ContextAction {
       

        public override void RunAction() {
            
            var unit = base.Context.MaybeCaster.Descriptor;
            if (unit == null) return;
            var spellBook = unit.DemandSpellbook(blueprintSpellbook);
            if (spellBook == null) return;
            if (spellLevel < 0 || spellLevel > spellBook.MaxSpellLevel) return;
            int spontaneousSlots = spellBook.GetSpontaneousSlots(spellLevel);
            if(spontaneousSlots > 0) {
                var enumerate_spells1 = spellBook.GetKnownSpells(spellLevel);
                spellBook.Spend(enumerate_spells1.Last<AbilityData>());
            }
            unit.Resources.Restore(resource, spellLevel);
        }

        public override string GetCaption() {
            return string.Empty;
        }

        public BlueprintAbilityResource resource;
        public int spellLevel;
        public BlueprintSpellbook blueprintSpellbook;
    }

    public class AbilityRequirementClassSpellLevel : BlueprintComponent, IAbilityAvailabilityProvider {
        public string GetReason() {
            LocalizedString lstr = Helpers.CreateString("ArcaneTide.AbilityRequirementClassSpellLevel.Reason");
            return String.Format(lstr.ToString(), RequiredSpellLevel);
        }

        public bool IsAvailableFor(AbilityData ability) {
            UnitDescriptor unit = ability.Caster;
            ClassData classData = unit.Progression.GetClassData(characterClass);
            BlueprintSpellbook x = (classData != null) ? classData.Spellbook : null;
            if (x != null) {
                Spellbook spellbook = unit.DemandSpellbook(classData.CharacterClass);
                int maxSpellLevel = spellbook.MaxSpellLevel;
                if (maxSpellLevel >= this.RequiredSpellLevel) {
                    int SpontaneousSlotCnt = spellbook.GetSpontaneousSlots(this.RequiredSpellLevel);
                    return SpontaneousSlotCnt > 0;
                }
            }
            return false;
        }

        public BlueprintCharacterClass characterClass;
        public int RequiredSpellLevel;
    }
}
