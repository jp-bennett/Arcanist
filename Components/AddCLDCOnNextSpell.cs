using Kingmaker.Blueprints.Classes;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic.Buffs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.UnitLogic;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Blueprints.Classes.Spells;
using ArcaneTide.Arcanist;

namespace ArcaneTide.Components {
    public class AddCLDCOnNextSpell_AR : BuffLogic, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IRulebookHandler<RuleCalculateAbilityParams>, IInitiatorRulebookSubscriber {
        public override void OnTurnOn() {
        }

        // Token: 0x06001815 RID: 6165 RVA: 0x00068685 File Offset: 0x00066A85
        public override void OnTurnOff() {
        }
        public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt) {
            int scale = (base.Owner.HasFact(PotentMagic)) ? 2 : 1;
            evt.AddBonusCasterLevel(valueCL*scale);
            evt.AddBonusDC(valueDC*scale);
            
        }
        public void OnEventDidTrigger(RuleCalculateAbilityParams evt) {
            base.Buff.Remove();
        }

        public BlueprintFeature PotentMagic;
        public int valueCL, valueDC;
    }

    public class MagicSupremancyBuffComp : BuffLogic, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IRulebookHandler<RuleCalculateAbilityParams>, IInitiatorRulebookSubscriber {
        public override void OnTurnOn() {
        }

        // Token: 0x06001815 RID: 6165 RVA: 0x00068685 File Offset: 0x00066A85
        public override void OnTurnOff() {
        }
        public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt) {
            int spellLevel = evt.SpellLevel;
            int resource_cnt = base.Owner.Resources.GetResourceAmount(resource);
            if (resource_cnt >= spellLevel) {
                evt.AddBonusCasterLevel(2);
                evt.AddBonusDC(2);
                base.Owner.Resources.Spend(resource, spellLevel);
                base.Owner.AddBuff(Supremancy.buff2, base.Owner.Unit);
            }

        }
        public void OnEventDidTrigger(RuleCalculateAbilityParams evt) {

        }
        bool isToRemove = false;
        public BlueprintAbilityResource resource;
    }
    public class AbilityRequirementNoSupremancy : BlueprintComponent, IAbilityAvailabilityProvider {
        public string GetReason() {
            return string.Empty;
        }

        public bool IsAvailableFor(AbilityData ability) {
            UnitDescriptor unit = ability.Caster;
            return !unit.Buffs.HasFact(Supremancy.buff);
        }

    }
}
