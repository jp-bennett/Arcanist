using Kingmaker.Blueprints;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ArcaneTide;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Localization;
using Kingmaker.Blueprints.Classes.Spells;

namespace ArcaneTide.Utils {
    static class PresetDurations {

        static public ContextDurationValue threeRounds = new ContextDurationValue {
            Rate = DurationRate.Rounds,
            DiceType = DiceType.Zero,
            DiceCountValue = new ContextValue {
                ValueType = ContextValueType.Simple,
                Value = 0
            },
            BonusValue = new ContextValue {
                ValueType = ContextValueType.Simple,
                Value = 3
            }
        };

        static public ContextDurationValue oneRound = new ContextDurationValue {
            Rate = DurationRate.Rounds,
            DiceType = DiceType.Zero,
            DiceCountValue = new ContextValue {
                ValueType = ContextValueType.Simple,
                Value = 0
            },
            BonusValue = new ContextValue {
                ValueType = ContextValueType.Simple,
                Value = 1
            }
        };

        
    }

    static class IconSet {
        static internal LibraryScriptableObject library => Main.library;
        static public Sprite spell_strike_icon;
        static public Sprite vanish_icon;
        static public Sprite itembond_icon;
        static public Dictionary<SpellSchool, Sprite> school_icons = new Dictionary<SpellSchool, Sprite>();
        static public void Load() {
            spell_strike_icon = library.Get<BlueprintFeature>("be50f4e97fff8a24ba92561f1694a945").Icon;
            vanish_icon = Helpers.GetIcon("f001c73999fb5a543a199f890108d936");
            itembond_icon = library.Get<BlueprintAbility>("e5dcf71e02e08fc448d9745653845df1").Icon;

            school_icons[SpellSchool.Necromancy] = library.Get<BlueprintFeature>("a9bb3dcb2e8d44a49ac36c393c114bd9").Icon;
            school_icons[SpellSchool.Abjuration] = library.Get<BlueprintFeature>("7f8c1b838ff2d2e4f971b42ccdfa0bfd").Icon;
            school_icons[SpellSchool.Conjuration] = library.Get<BlueprintFeature>("ca4a0d68c0408d74bb83ade784ebeb0d").Icon;
            school_icons[SpellSchool.Divination] = library.Get<BlueprintFeature>("09595544116fe5349953f939aeba7611").Icon;
            school_icons[SpellSchool.Enchantment] = library.Get<BlueprintFeature>("875fff6feb84f5240bf4375cb497e395").Icon;
            school_icons[SpellSchool.Evocation] = library.Get<BlueprintFeature>("c3724cfbe98875f4a9f6d1aabd4011a6").Icon;
            school_icons[SpellSchool.Illusion] = library.Get<BlueprintFeature>("6750ead44c0c034428c6509c68110375").Icon;
            school_icons[SpellSchool.Transmutation] = library.Get<BlueprintFeature>("fc519612a3c604446888bb345bca5234").Icon;
            school_icons[SpellSchool.Universalist] = library.Get<BlueprintProgression>("0933849149cfc9244ac05d6a5b57fd80").Icon;
        }
    }

    static class MetaFeats {
        static internal LibraryScriptableObject library => Main.library;
        static public Dictionary<int, string> dict;
        static public void Load() {
            dict = new Dictionary<int, string>();
            foreach(var kv in library.BlueprintsByAssetId) {
                var key = kv.Key;
                var value = kv.Value;
                if((value as BlueprintFeature) != null) {
                    var metamagicComp = value.GetComponent<AddMetamagicFeat>();
                    if(metamagicComp != null) {
                        int metaId = (int)(metamagicComp.Metamagic);
                        dict[metaId] = key;
                    }
                }
            }
            foreach(var kv in dict) {
                UnityModManagerNet.UnityModManager.Logger.Log($"Metamagic Id {kv.Key} refers to feat {kv.Value}");
            }
        }
    }
    static class ResourceManagement {
        static public void RestoreResource(UnitDescriptor unit, BlueprintAbilityResource blueprint, int amount) {
            unit.Resources.Restore(blueprint, amount);
        }
        static public void SpendResource(UnitDescriptor unit, BlueprintAbilityResource blueprint, int amount) {
            unit.Resources.Spend(blueprint, amount);
        }
    }
}
