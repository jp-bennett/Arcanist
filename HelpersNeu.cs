using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints;
using Kingmaker.View.Spawners;
using UnityEngine;
using static UnityModManagerNet.UnityModManager.ModEntry;

namespace ArcaneTide {
    static class HelpersNeu {
        static LibraryScriptableObject library => Main.library;
        static ModLogger logger => Main.logger;
        static GameObject exampleGameObject;
        
        static public void SetSpellTableLevel(this BlueprintSpellsTable spellPerDay, int level, int[] Count) {
            try {
                spellPerDay.Levels[level] = new SpellsLevelEntry();
                spellPerDay.Levels[level].Count = Count;
            }
            catch(Exception e) {
                spellPerDay.Levels = new SpellsLevelEntry[21];
            }
        }
        static FastSetter UnitSpawner_Blueprint_Setter = Helpers.CreateFieldSetter<UnitSpawner>("m_Blueprint");
        static FastSetter UnitSpawner_IsCompanionSetter = Helpers.CreateFieldSetter<UnitSpawner>("m_IsCompanion");
        static FastSetter UnitSpawner_RestoreCompanionOnReload_Setter = Helpers.CreateFieldSetter<UnitSpawner>("m_RestoreCompanionOnReload");
        static FastSetter UnitSpawner_SummonPools_Setter = Helpers.CreateFieldSetter<UnitSpawner>("m_SummonPools");
        
        static public UnitSpawner CreateSpawner(BlueprintUnit unitBlue, string guid, Vector3 Position, Quaternion rotation, bool isCompanion, bool isRestoreCompanion, List<BlueprintSummonPool> summonPools) {

            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            UnitSpawner ans = obj.AddComponent<UnitSpawner>();
            obj.SetActive(true);
            ans.UniqueId = new Guid(guid).ToString();

            logger.Log("rua rua 1");
            ans.transform.SetPositionAndRotation(Position, rotation);
            logger.Log("rua rua 2");
            UnitSpawner_Blueprint_Setter(ans, unitBlue);
            logger.Log("rua rua 3");
            UnitSpawner_IsCompanionSetter(ans, isCompanion);
            logger.Log("rua rua 4");
            UnitSpawner_RestoreCompanionOnReload_Setter(ans, isRestoreCompanion);
            logger.Log("rua rua 5");
            UnitSpawner_SummonPools_Setter(ans, summonPools);
            logger.Log("rua rua 6");
            ans.OnSpawned = new UnitSpawner.UnitSpawnedEvent();
            logger.Log("rua rua 7");
            return ans;
        }

    }
}
