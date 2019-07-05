using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Designers.EventConditionActionSystem.Conditions;
using Kingmaker.ElementsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcaneTide.Risia {
    static class TestCopyScene {
        static internal LibraryScriptableObject library => Main.library;
        static internal FastGetter dynamicScenesGetter = Helpers.CreateFieldGetter<BlueprintArea>("m_ChangeableDynamics");
        static internal FastSetter dynamicScenesSetter = Helpers.CreateFieldSetter<BlueprintArea>("m_ChangeableDynamics");
        static public void Load() {
            BlueprintArea templeElk = library.Get<BlueprintArea>("340a310b850e1ed469a60388012734f9");
            BlueprintAreaPart.AdditionalDynamicEntry newDynamicSceneEntry = new BlueprintAreaPart.AdditionalDynamicEntry {
                Scene = new SceneReference("RisiaElkScene"),
                Condition = new ConditionsChecker {
                    Conditions = new Condition[] {
                        /*new FlagUnlocked {
                            ConditionFlag = library.Get<BlueprintUnlockableFlag>("eccd6891ce0466c4fbe09a55838b19ab"),
                            SpecifiedValues = (new int[]{1}).ToList<int>()
                        }*/
                    }
                },
                AdditionalDataBank = ""
            };
            BlueprintAreaPart.AdditionalDynamicEntry[] oldDynamicScenes = dynamicScenesGetter(templeElk) as BlueprintAreaPart.AdditionalDynamicEntry[];
            var tmp = oldDynamicScenes.ToList();
            tmp.Add(newDynamicSceneEntry);
            dynamicScenesSetter(templeElk, tmp.ToArray<BlueprintAreaPart.AdditionalDynamicEntry>());

        }
    }
}
