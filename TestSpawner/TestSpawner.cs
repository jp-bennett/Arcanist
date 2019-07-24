using ArcaneTide.Components;
using ArcaneTide.Risia;
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
        static internal BlueprintUnlockableFlag flagIsRisiaSpawned => RisiaMainLoad.flagIsRisiaSpawned;
        static internal BlueprintUnlockableFlag flagIsRisiaBossSpawned => RisiaMainLoad.flagIsRisiaBossSpawned;
        static internal UnitSpawner spawner;
        static public void Load() {
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
            EntityReference spawnerBossRef = new EntityReference {
                UniqueId = consts.GUIDs["RisiaBossElkTempleSpawner_D"]
            };
            UnitFromSpawner risiaEval = Helpers.Create<UnitFromSpawner>(a => a.Spawner = spawnerRef);
            UnitFromSpawner risiaBossEval = Helpers.Create<UnitFromSpawner>(a => a.Spawner = spawnerBossRef);
            BlueprintArea ElkTemple = library.Get<BlueprintArea>("340a310b850e1ed469a60388012734f9");
            var compNeu = Helpers.Create<AreaDidLoadTrigger>();
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
                    Helpers.Create<ArcaneTide_ReplaceViewAction>(a => {
                        a.dollData_key = "dolldata0";
                        a.unitEV = risiaEval;
                    }),
                    Helpers.Create<UnlockFlag>(a => a.flag = flagIsRisiaSpawned)
                }
            };
            var compNeuBoss = Helpers.Create<AreaDidLoadTrigger>();
            compNeuBoss.Conditions = new ConditionsChecker {
                Conditions = new Condition[]{
                    Helpers.Create<FlagUnlocked>(a => {
                        a.ConditionFlag = flagIsRisiaBossSpawned;
                        a.Not = true;
                    })
                }
            };
            compNeuBoss.Actions = new ActionList {
                Actions = new GameAction[] {
                    Helpers.Create<ArcaneTide_ReplaceViewAction>(a => {
                        a.dollData_key = "dolldata1";
                        a.unitEV = risiaBossEval;
                    }),
                    Helpers.Create<UnlockFlag>(a => a.flag = flagIsRisiaBossSpawned),
                    Helpers.Create<SwitchToNeutral>(a => {
                        a.Target = risiaBossEval;
                        a.Faction = library.Get<BlueprintFaction>("72f240260881111468db610b6c37c099"); //Player Faction
                    })
                }
            };
            ElkTemple.AddComponent(compNeu);
            ElkTemple.AddComponent(compNeuBoss);
        }
    }
}
