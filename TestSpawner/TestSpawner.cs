using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.EventConditionActionSystem.Conditions;
using Kingmaker.Designers.EventConditionActionSystem.Events;
using Kingmaker.ElementsSystem;
using Kingmaker.View.Spawners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityModManagerNet.UnityModManager.ModEntry;

namespace ArcaneTide.TestSpawner {
    static class TestSpawner {
        static internal LibraryScriptableObject library => Main.library;
        static internal ModLogger logger => Main.logger;
        static internal BlueprintUnlockableFlag flagIsRisiaSpawned;
        static internal UnitSpawner spawner;
        static public void Load() {
            BlueprintUnit irovetti = library.Get<BlueprintUnit>("8b2cbf4590ed9e84591cd9a1f55bbdb8");
            flagIsRisiaSpawned = Helpers.Create<BlueprintUnlockableFlag>();
            library.AddAsset(flagIsRisiaSpawned, "942339abe7b76dfb00324d433a0a9342");
            flagIsRisiaSpawned.Lock();
            flagIsRisiaSpawned.name = "flagIsRisiaSpawned";
            logger.Log("rua rua rua rua 1");
            spawner = HelpersNeu.CreateSpawner(irovetti, "c47dab2a47b4826ed16201142956d607",
                new UnityEngine.Vector3(11.77268f, 1.241176f, 1.633618f), UnityEngine.Quaternion.identity,
                false, false, new List<BlueprintSummonPool>());
            logger.Log("rua rua rua rua 2");
            /*EntityReference spawnerIrovettiRef = new EntityReference {
                UniqueId = "7d4ab0ca-92d9-4960-9225-c341558a47c8" //irovetti
            };
            UnitSpawner spawnerIrovetti = spawnerIrovettiRef.FindView() as UnitSpawner;
            spawner = UnityEngine.Object.Instantiate<UnitSpawner>(spawnerIrovetti);
            spawner.transform.SetPositionAndRotation(new UnityEngine.Vector3(11.77268f, 1.241176f, 1.633618f), UnityEngine.Quaternion.identity);
            spawner.UniqueId = "c47dab2a-47b4-826e-d162-01142956d607";*/
            EntityReference spawnerRef = new EntityReference {
                UniqueId = "9197b23a-4e69-dba5-be76-fef09c815d93"
            };
            logger.Log("rua rua rua rua 3");
            BlueprintArea ElkTemple = library.Get<BlueprintArea>("340a310b850e1ed469a60388012734f9");
            logger.Log("rua rua rua rua 4");
            var compNeu = Helpers.Create<ActivateTrigger>();
            compNeu.Conditions = new ConditionsChecker {
                Conditions = new Condition[]{
                    Helpers.Create<FlagUnlocked>(a => {
                        a.ConditionFlag = flagIsRisiaSpawned;
                        a.Not = true;
                    })
                }
            };
            compNeu.Actions = new ActionList {
                Actions = new GameAction[] {
                    Helpers.Create<Spawn>(a => {
                        a.Spawners = new EntityReference[]{spawnerRef};
                        a.ActionsOnSpawn = new ActionList();
                    }),
                    Helpers.Create<UnlockFlag>(a => a.flag = flagIsRisiaSpawned)
                }
            };
            logger.Log("rua rua rua rua 5");
            ElkTemple.AddComponent(compNeu);
            logger.Log("rua rua rua rua 6");
        }
    }
}
