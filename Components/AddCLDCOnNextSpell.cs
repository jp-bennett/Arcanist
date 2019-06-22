using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic.Buffs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcaneTide.Components {
    public class AddCLDCOnNextSpell : BuffLogic, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IRulebookHandler<RuleCalculateAbilityParams>, IInitiatorRulebookSubscriber {
        public override void OnTurnOn() {
        }

        // Token: 0x06001815 RID: 6165 RVA: 0x00068685 File Offset: 0x00066A85
        public override void OnTurnOff() {
        }
        public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt) {
            evt.AddBonusCasterLevel(value);
            evt.AddBonusDC(value);
        }
        public void OnEventDidTrigger(RuleCalculateAbilityParams evt) {

        }

        public int value;
    }
}
