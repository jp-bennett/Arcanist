using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints;
namespace ArcaneTide {
    static class HelpersNeu {
        static LibraryScriptableObject library => Main.library;
        static public void SetSpellTableLevel(this BlueprintSpellsTable spellPerDay, int level, int[] Count) {
            try {
                spellPerDay.Levels[level] = new SpellsLevelEntry();
                spellPerDay.Levels[level].Count = Count;
            }
            catch(Exception e) {
                spellPerDay.Levels = new SpellsLevelEntry[21];
            }
        }

    }
}
