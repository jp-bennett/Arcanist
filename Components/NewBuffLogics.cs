using ArcaneTide.Arcanist;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Units;
using Kingmaker.Enums.Damage;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.ServiceWindow;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
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
        private void RemoveMetamagicFeatInSpellSlots(BlueprintFeature metafeatOld) {
            Metamagic metaOld = metafeatOld.GetComponent<AddMetamagicFeat>().Metamagic;
            List<AbilityData> spellsToRemove = new List<AbilityData>();
            List<SpellSlot> slotsToForget = new List<SpellSlot>();
            var unit = base.Owner;
            foreach(Spellbook spellbook in unit.Spellbooks) {
                int maxSplLevel = spellbook.MaxSpellLevel;
                for (int i = 1; i <= maxSplLevel; i++) {
                    foreach (AbilityData spell in spellbook.GetCustomSpells(i)) {
                        if (spell.HasMetamagic(metaOld)) {
                            spellsToRemove.Add(spell);
                        }
                    }
                    foreach (SpellSlot slot in spellbook.GetMemorizedSpellSlots(i)) {
                        if(slot.Spell != null && !slot.IsOpposition) {
                            if (slot.Spell.HasMetamagic(metaOld)) {
                                slotsToForget.Add(slot);
                            }
                        }
                    }
                }
                foreach (SpellSlot slot in slotsToForget) {
                    spellbook.ForgetMemorized(slot);
                }
                foreach (AbilityData spell in spellsToRemove) {
                    spellbook.RemoveCustomSpell(spell);
                }
                
            }
        }
        public override void OnTurnOn() {
            base.OnTurnOn();

            var unit = base.Owner;
            
            foreach (var subBuff in GreaterMetamagicKnowledge.subBuffs) {
                if (subBuff == this.Buff.Blueprint) continue;
                if (unit.Buffs.HasFact(subBuff)) {
                    unit.Buffs.RemoveFact(subBuff);
                }
            }
            if(spellMixer == null) {
                spellMixer = spellMixerGetter(Game.Instance.UI.SpellBookController) as SpellBookMetamagicMixer;
                if(spellMixer == null) {
                    base.Buff.Remove();
                    throw new Exception("BuffChangeSingleSelectedFeature.OnTurnOn(): Get Null SpellBookMetamagicMixer!");
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
                RemoveMetamagicFeatInSpellSlots(feat); 
                selectionData.RemoveSelection(level, feat);
                //UnityModManager.Logger.Log($"Remove Feat {feat.Name} Finish.");
            }
            //UnityModManager.Logger.Log($"Start to add Feat.");
            selectionData.AddSelection(level, newFeatBlue);
            unit.AddFact(newFeatBlue);
            builder_setter(spellMixer, null);
            //UnityModManager.Logger.Log($"Added feat {newFeatBlue.Name}");
        }
        public override void OnTurnOff() {
            if (this.replacedOriginalFeats != null && spellMixer != null && level != -1) {
                var unit = base.Owner;
                FeatureSelectionData selectionData = unit.Progression.GetSelectionData(selectionBlue);
                unit.Progression.Features.RemoveFact(newFeatBlue);
                RemoveMetamagicFeatInSpellSlots(newFeatBlue);
                selectionData.RemoveSelection(level, newFeatBlue);
                foreach (var feat in this.replacedOriginalFeats) {
                    unit.AddFact(feat);
                    selectionData.AddSelection(level, feat);
                }
                builder_setter(spellMixer, null);
            }
            
            base.OnTurnOff();
        }
        private FastSetter builder_setter = Helpers.CreateFieldSetter<SpellBookMetamagicMixer>("m_MetamagicBuilder");
        public BlueprintFeatureSelection selectionBlue;
        public BlueprintFeature newFeatBlue;
        private List<BlueprintFeature> replacedOriginalFeats = null;
        private int level = -1;
        private FastGetter spellMixerGetter = Helpers.CreateFieldGetter<SpellBookController>("m_SpellBookMetamagicMixer");
        private SpellBookMetamagicMixer spellMixer = null;
    }
    class BuffSpecialGreaterMetamagic : BuffLogic, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IRulebookHandler<RuleCalculateAbilityParams>, IInitiatorRulebookSubscriber {

        public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt) {
            UnitDescriptor unit = evt.AbilityData != null ? evt.AbilityData.Caster : null;
            if (unit != null && evt.Spell != null && evt.Spellbook != null && evt.Spell.Type == AbilityType.Spell) {
                if (metamagic == Metamagic.Heighten && HeightenTarget > evt.SpellLevel) {
                    int metamagicCost = MetamagicHelper.DefaultCost(metamagic);
                    if (metamagic == Metamagic.Heighten) {
                        metamagicCost += (HeightenTarget - evt.SpellLevel);
                    }
                    if(unit.Resources.GetResourceAmount(ArcaneReservoir.resource) < metamagicCost) {
                        this.AboutToRemove = true;
                    }
                    unit.Resources.Spend(ArcaneReservoir.resource, metamagicCost);

                    evt.AddMetamagic(this.metamagic);
                    MetamagicData md = metamagicData_Getter(evt) as MetamagicData;
                    md.HeightenLevel = (HeightenTarget - evt.SpellLevel >= md.HeightenLevel ? HeightenTarget - evt.SpellLevel : md.HeightenLevel);
                    this.AboutToRemove = true;
                }
                else if(metamagic != Metamagic.Heighten) {
                    int metamagicCost = MetamagicHelper.DefaultCost(metamagic);
                    if (unit.Resources.GetResourceAmount(ArcaneReservoir.resource) < metamagicCost) {
                        if (this.ShallRemove) {
                            base.Buff.Remove();
                        }
                    }
                    unit.Resources.Spend(ArcaneReservoir.resource, metamagicCost);

                    evt.AddMetamagic(this.metamagic);
                    this.AboutToRemove = true;
                }
            }
        }

        public void OnEventDidTrigger(RuleCalculateAbilityParams evt) {
            if(this.ShallRemove && this.AboutToRemove) {
                base.Buff.Remove();
            }
        }
        public Metamagic metamagic;
        public int HeightenTarget = 9;
        public bool ShallRemove = true;
        private bool AboutToRemove = false;
        private FastGetter metamagicData_Getter = Helpers.CreateFieldGetter<RuleCalculateAbilityParams>("m_MetamagicData");
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

    class BuffIcyTombGotFireDamageRemoval : BuffLogic, ITargetRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ITargetRulebookSubscriber {
        public void OnEventAboutToTrigger(RuleDealDamage evt) {
            
        }

        public void OnEventDidTrigger(RuleDealDamage evt) {
            foreach(DamageValue val in evt.ResultDamage) {
                if(val.Source is EnergyDamage) {
                    EnergyDamage d = val.Source as EnergyDamage;
                    if(d.EnergyType == damageType && val.FinalValue >= 10) {
                        base.Buff.Remove();
                    }
                }
            }
        }
        public DamageEnergyType damageType;
    }
}
