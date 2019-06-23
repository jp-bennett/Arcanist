using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Localization;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcaneTide.Components {
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
                return maxSpellLevel >= RequiredSpellLevel;
            }
            return false;
        }

        public BlueprintCharacterClass characterClass;
        public int RequiredSpellLevel;
    }

    public class AbilityRequirementFeature : BlueprintComponent, IAbilityAvailabilityProvider {
        public string GetReason() {
            return string.Empty;
        }

        public bool IsAvailableFor(AbilityData ability) {
            var unit = ability.Caster;
            bool hasFeat = unit.HasFact(Feat);
            return Not ? (!hasFeat) : hasFeat;
        }

        public bool Not;
        public BlueprintFeature Feat;
    }

    public class AbilityRequirementBuff : BlueprintComponent, IAbilityAvailabilityProvider {
        public string GetReason() {
            return string.Empty;
        }

        public bool IsAvailableFor(AbilityData ability) {
            var unit = ability.Caster;
            bool hasFeat = unit.Buffs.HasFact(buff);
            return Not ? (!hasFeat) : hasFeat;
        }

        public bool Not;
        public BlueprintBuff buff;
    }
}
