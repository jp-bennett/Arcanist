using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcaneTide.Components {
    class BuffAddFacts : OwnedGameLogicComponent<UnitDescriptor> {
        public override void OnTurnOn() {
            base.OnTurnOn();
            factsToRemoveList = new List<BlueprintUnitFact>();
            var unit = base.Owner;
            if(Facts != null) {
                foreach(var fact in Facts) {
                    if (unit.HasFact(fact)) continue;
                    unit.AddFact(fact);
                    factsToRemoveList.Add(fact);
                }
            }
            
        }
        public override void OnTurnOff() {
            base.OnTurnOff();
            foreach(var fact in factsToRemoveList) {
                if (base.Owner.HasFact(fact)) {
                    base.Owner.RemoveFact(fact);
                }
            }
        }
        public BlueprintUnitFact[] Facts;
        List<BlueprintUnitFact> factsToRemoveList;
    }
}
