using ArcaneTide.Arcanist;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Localization;
using Kingmaker.UnitLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ArcaneTide.Utils {
    static class OtherUtils {
        public static string GetMd5(string str) {
            var md5 = MD5.Create();
            var bytValue = Encoding.UTF8.GetBytes(str);
            var bytHash = md5.ComputeHash(bytValue);
            var sb = new StringBuilder();
            foreach (var b in bytHash) {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
        public static Pair<U,V> make_pair<U,V>(U u, V v) {
            return new Pair<U, V>(u, v);
        }

        public static string GetSchoolStr(SpellSchool sc) {
            switch (sc) {
                case SpellSchool.Abjuration:
                    return "Abjuration";
                case SpellSchool.Conjuration:
                    return "Conjuration";
                case SpellSchool.Divination:
                    return "Divination";
                case SpellSchool.Enchantment:
                    return "Enchantment";
                case SpellSchool.Evocation:
                    return "Evocation";
                case SpellSchool.Illusion:
                    return "Illusion";
                case SpellSchool.Necromancy:
                    return "Necromancy";
                case SpellSchool.Transmutation:
                    return "Transmutation";
                case SpellSchool.Universalist:
                    return "Universal";
                default:
                    return "None";
            }
        }

    }

    static class CheckArcanist {
        public static bool isArcanist(Spellbook spellbook) {
            if (spellbook == null) return false;
            return spellbook.Blueprint.CharacterClass == ArcanistClass.arcanist;
        }
    }

    static class PresetLocStrings {
        static public LocalizedString loc_3Round = Helpers.CreateString("ArcaneTide.ThreeRounds");
        static public LocalizedString loc_1Round = Helpers.CreateString("ArcaneTide.OneRound");
        static public LocalizedString loc_instant = Helpers.CreateString("ArcaneTide.Instant");
        static public LocalizedString loc_minute = Helpers.CreateString("ArcaneTide.MinutesPerLevel");
        static public LocalizedString save_will_noharm = Helpers.CreateString("ArcaneTide.WillSave.NoHarm");
        static public LocalizedString save_none = Helpers.CreateString("ArcaneTide.NoSave");
    }
    class Pair<U, V> {
        public U first { get; set; }
        public V second { get; set; }
        public Pair(U u, V v){
            first = u;
            second = v;
        }
    }
}
