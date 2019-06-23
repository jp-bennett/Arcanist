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

   
}
