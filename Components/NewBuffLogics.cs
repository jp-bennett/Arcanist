using ArcaneTide.Arcanist;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Units;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityModManagerNet;

namespace ArcaneTide.Components {
    class BuffAddFacts : BuffLogic {
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

    class BuffChangeSingleSelectedFeature : BuffLogic{
        public override void OnTurnOn() {
            base.OnTurnOn();

            var unit = base.Owner;
            
            foreach (var subBuff in GreaterMetamagicKnowledge.subBuffs) {
                if (subBuff == this.Buff.Blueprint) continue;
                if (unit.Buffs.HasFact(subBuff)) {
                    this.Buff.Remove();
                    return;
                }
            }

            unit.Resources.Spend(ArcaneReservoir.resource, 1);
            //UnityModManager.Logger.Log($"Unit = {unit.CharacterName}");
            FeatureSelectionData selectionData = unit.Progression.GetSelectionData(selectionBlue);
            if (selectionData.SelectionsByLevel.Count != 1) return;
            level = selectionData.SelectionsByLevel.ElementAt(0).Key;

            this.replacedOriginalFeats = new List<BlueprintFeature>(selectionData.GetSelections(level));
            //UnityModManager.Logger.Log($"Replaced Original Feats has {this.replacedOriginalFeats.Count} feats.");
            
            foreach (var feat in this.replacedOriginalFeats) {
                //UnityModManager.Logger.Log($"Remove Feat {feat.Name}");
                unit.Progression.Features.RemoveFact(feat);
                //selectionData.RemoveSelection(level, feat);
                //UnityModManager.Logger.Log($"Remove Feat {feat.Name} Finish.");
            }
            //UnityModManager.Logger.Log($"Start to add Feat.");
            //selectionData.AddSelection(level, newFeatBlue);
            unit.AddFact(newFeatBlue);
            //UnityModManager.Logger.Log($"Added feat {newFeatBlue.Name}");
        }
        public override void OnTurnOff() {
            if (this.replacedOriginalFeats != null && level != -1) {
                var unit = base.Owner;
                FeatureSelectionData selectionData = unit.Progression.GetSelectionData(selectionBlue);
                unit.Progression.Features.RemoveFact(newFeatBlue);
                foreach (var feat in this.replacedOriginalFeats) {
                    unit.AddFact(feat);
                }
            }
            
            base.OnTurnOff();
        }
        public BlueprintFeatureSelection selectionBlue;
        public BlueprintFeature newFeatBlue;
        private List<BlueprintFeature> replacedOriginalFeats = null;
        private int level = -1;
    }

    class BuffAcidBurst : BuffLogic, ITickEachRound {
        public override void OnTurnOn() {
            base.OnTurnOn();
            hasDealtFirstDamage = true;
            diceCnt = CL.Calculate(this.Context);
        }
        public override void OnTurnOff() {
            base.OnTurnOff();
        }

        public void OnNewRound() {
            if (!hasDealtFirstDamage) {
                return;
            }
            diceCnt /= 2;
            DiceFormula damageDice = new DiceFormula(diceCnt, DiceType.D6);
            base.Context.TriggerRule<RuleDealDamage>(new RuleDealDamage(this.Buff.Context.MaybeCaster, this.Buff.Owner.Unit,
                new EnergyDamage(damageDice, DamageEnergyType.Acid)));
            if (diceCnt == 1) this.Buff.Remove();
        }
        bool hasDealtFirstDamage = false;
        int diceCnt;
        public ContextValue CL;
    }
}
