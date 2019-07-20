using ArcaneTide.Components;
using ArcaneTide.Utils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.EventConditionActionSystem.Conditions;
using Kingmaker.Designers.EventConditionActionSystem.Evaluators;
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
        static internal GlobalConstants consts => Main.constsManager;
        static internal BlueprintUnlockableFlag flagIsRisiaSpawned;
        static internal UnitSpawner spawner;
        static public void Load() {
            BlueprintUnit irovetti = library.Get<BlueprintUnit>("8b2cbf4590ed9e84591cd9a1f55bbdb8");
            flagIsRisiaSpawned = Helpers.Create<BlueprintUnlockableFlag>();
            library.AddAsset(flagIsRisiaSpawned, "942339abe7b76dfb00324d433a0a9342");
            flagIsRisiaSpawned.Lock();
            flagIsRisiaSpawned.name = "flagIsRisiaSpawned";
            logger.Log("rua rua rua rua 1");
            
            logger.Log("rua rua rua rua 2");
            /*EntityReference spawnerIrovettiRef = new EntityReference {
                UniqueId = "7d4ab0ca-92d9-4960-9225-c341558a47c8" //irovetti
            };
            UnitSpawner spawnerIrovetti = spawnerIrovettiRef.FindView() as UnitSpawner;
            spawner = UnityEngine.Object.Instantiate<UnitSpawner>(spawnerIrovetti);
            spawner.transform.SetPositionAndRotation(new UnityEngine.Vector3(11.77268f, 1.241176f, 1.633618f), UnityEngine.Quaternion.identity);
            spawner.UniqueId = "c47dab2a-47b4-826e-d162-01142956d607";*/
            EntityReference spawnerRef = new EntityReference {
                UniqueId = consts.GUIDs["RisiaElkTempleSpawner_D"]
            };
            logger.Log("rua rua rua rua 3");
            BlueprintArea ElkTemple = library.Get<BlueprintArea>("340a310b850e1ed469a60388012734f9");
            logger.Log("rua rua rua rua 4");
            var compNeu = Helpers.Create<AreaDidLoadTrigger>();
            compNeu.Conditions = new ConditionsChecker {
                Conditions = new Condition[]{
                    /*Helpers.Create<FlagUnlocked>(a => {
                        a.ConditionFlag = flagIsRisiaSpawned;
                        a.Not = true;
                    })*/
                }
            };
            compNeu.Actions = new ActionList {
                Actions = new GameAction[] {
                    Helpers.Create<ArcaneTide_ReplaceViewAction>(a => {
                        a.dollData_key = "dolldata0";
                        a.unitEV = new UnitFromSpawner {
                            Spawner = spawnerRef
                        };
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
