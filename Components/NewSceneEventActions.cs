using Kingmaker;
using Kingmaker.Designers.EventConditionActionSystem.Evaluators;
using Kingmaker.ElementsSystem;
using Kingmaker.Items.Slots;
using Kingmaker.UnitLogic;
using Kingmaker.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcaneTide.Components {
    class ArcaneTide_ReplaceViewAction : GameAction {
        public override string GetCaption() {
            return string.Empty;
        }

        public override void RunAction() {
            ModStorage.dolls["dolldata0"] = Game.Instance.Player.MainCharacter.Value.Descriptor.Doll;
            if (ModStorage.dolls.ContainsKey(dollData_key)) {
                DollData doll = ModStorage.dolls[dollData_key];
                UnitDescriptor unit = unitEV.GetValue().Descriptor;
                UnitEntityView newViw = doll.CreateUnitView();
                UnitEntityView oldVIw = unit.Unit.View;
                newViw.UniqueId = unit.Unit.UniqueId;
                newViw.transform.SetParent(unit.Unit.View.transform);
                newViw.transform.SetPositionAndRotation(unit.Unit.View.transform.position, unit.Unit.View.transform.rotation);
                newViw.Blueprint = unit.Unit.Blueprint;
                unit.Doll = doll;
                unit.Unit.AttachToViewOnLoad(newViw);

                foreach (ItemSlot itemSlot in unit.Unit.Body.AllSlots) {
                    if (itemSlot.HasItem && itemSlot.CanRemoveItem()) {
                        itemSlot.RemoveItem();
                    }
                }
                //UnityEngine.Object.Destroy(oldVIw.gameObject);

            }
        }
        public UnitFromSpawner unitEV;
        public string dollData_key;
    }
}
