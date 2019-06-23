using Harmony12;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcaneTide.Arcanist;
using Kingmaker.UnitLogic;
using UnityModManagerNet;
using ArcaneTide.Utils;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.AreaLogic.Cutscenes;

namespace ArcaneTide.Patches {
    [HarmonyPatch(typeof(UnitUseAbility), MethodType.Constructor,new Type[] { typeof(UnitCommand.CommandType),typeof(AbilityData),typeof(TargetWrapper)})]
    class Arcanist_SponMetamagic_Patch {
        static public bool Prefix(UnitUseAbility __instance, UnitCommand.CommandType commandType, ref AbilityData spell, TargetWrapper target) {
            //base ctor
            /*this.Type = commandType;
            this.Target = target;
            this.Cutscene = (ElementsContext.GetData<CutsceneParametersContext.ContextData>() != null);*/

            //modify spell
            if (spell == null) return true;
            if (spell.Spellbook == null) return true;
            if (spell.Spellbook.Blueprint.CharacterClass != ArcanistClass.arcanist) return true;
            UnitDescriptor unit = spell.Caster;
            bool hadMetamagic = spell.MetamagicData != null;
            bool hadMetamixing = unit.Buffs.HasFact(Metamixing.buff);

            //An arcanist can't combine prepared metamagic and spontaneous metamagic, if she doesn't have 
            //the Metamixing exploit.
            if (hadMetamagic && !hadMetamixing) return true;
            //UnityModManager.Logger.Log($"spell {spell.Name} has metamagic {(spell.MetamagicData!=null?spell.MetamagicData.MetamagicMask:0)}");
            MetamagicBuilder builder = new MetamagicBuilder(spell.Spellbook, spell);
            Dictionary<Metamagic, Feature> meta_feat = new Dictionary<Metamagic, Feature>();
            foreach(var ft in builder.SpellMetamagicFeatures) {
                AddMetamagicFeat comp = ft.Get<AddMetamagicFeat>();
                if (comp == null) continue;
                Metamagic metaId = comp.Metamagic;
                meta_feat[metaId] = ft;
            }
            bool flag = false;
            foreach(var kv in SponMetamagic.buffDict) {
                Metamagic metaId = (Metamagic)(kv.Key.first);
                int HeightenLevel = kv.Key.second;
                if (!meta_feat.ContainsKey(metaId)) continue;
                BlueprintBuff buff = kv.Value;
                if (!unit.HasFact(buff)) continue;

                //If the arcanist has the Metamixing exploit, she can add ONE 
                //metamagic feat to a prepared spell, without using extra time to cast.
                //(regardless if the spell had metamagic feats when prepared)
                if(metaId == Metamagic.Heighten) {
                    int originalLevel = spell.Spellbook.GetSpellLevel(spell.Blueprint);
                    if (HeightenLevel <= originalLevel) continue;
                    builder.AddHeightenMetamagic(meta_feat[metaId], HeightenLevel-originalLevel);
                    unit.RemoveFact(buff);
                    flag = true;
                    if (hadMetamixing) {
                        unit.Resources.Spend(ArcaneReservoir.resource, 1);
                        break;
                    }
                }
                else {
                    builder.AddMetamagic(meta_feat[metaId]);
                    unit.RemoveFact(buff);
                    flag = true;
                    if (hadMetamixing) {
                        unit.Resources.Spend(ArcaneReservoir.resource, 1);
                        break;
                    }
                }
            }
            if (flag && !hadMetamixing) unit.AddBuff(SponMetamagic.flagBuff,unit.Unit);
            spell = builder.ResultAbilityData;
            //UnityModManager.Logger.Log($"new spell {spell.Name} has metamagic {(spell.MetamagicData != null ? spell.MetamagicData.MetamagicMask : 0)}");
            return true;

        }
    }
}
