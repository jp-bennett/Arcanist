using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.FactLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcaneTide.Components {
    class RealMetamagicOnNextSpell : BuffLogic, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IRulebookHandler<RuleCalculateAbilityParams> {
        public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt) {
            /*
            if (evt.Spellbook == null) return;
            MetamagicBuilder builder = new MetamagicBuilder (evt.Spellbook,evt.AbilityData);
            List<Feature> metaFeatureSet = builder.SpellMetamagicFeatures;
            Feature this_feature = null;
            foreach(Feature ft in metaFeatureSet) {
                var comp = ft.Get<AddMetamagicFeat>();
                if (comp == null) continue;
                var _metamagic = comp.Metamagic;
                if (_metamagic == null) continue;
                if(_metamagic == this.metamagic) {
                    this_feature = ft;
                    break;
                }
            }
            if (this_feature == null) return;
            builder.AddMetamagic(this_feature);
            UnityModManagerNet.UnityModManager.Logger.Log($"result spell level is {builder.ResultSpellLevel}");
            evt. = builder.ResultSpellLevel;
            evt.AddMetamagic(this.metamagic);
            */
        }

        public void OnEventDidTrigger(RuleCalculateAbilityParams evt) {
            
        }
        public Metamagic metamagic;
    }
}
