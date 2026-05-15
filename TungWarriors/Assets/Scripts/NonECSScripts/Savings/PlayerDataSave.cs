using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace YG
{
    public partial class SavesYG
    {
        public int gold;
        public int gems;
        public int rubies;

        public List<EquipmentToSaveData> inventory;
        public List<EquipmentOnPlayerToSaveData> equipmentOnPlayer;

        public List<EquipmentToSaveData> SerializeInventory(List<Equipment> inv)
        {
            return inv.Select(e => e.Serialize()).ToList();
        }

        public List<Equipment> DeserializeInventory()
        {
            return inventory.Select(e => Equipment.Deserialize(e)).ToList();
        }

        public List<EquipmentOnPlayerToSaveData> SerializeEquipmentOnPlayer(Dictionary<EquipmentOnPlayerType, Equipment> inv)
        {
            var res = inv.Select(t => new EquipmentOnPlayerToSaveData
            {
                type = t.Key,
                equipmentData = t.Value.Serialize()
            }).ToList();
            return res;
        }

        public Dictionary<EquipmentOnPlayerType, Equipment> DeserializeEquipmentOnPlayer()
        {
            var result = new Dictionary<EquipmentOnPlayerType, Equipment>();

            foreach (var data in equipmentOnPlayer)
            {
                var equipment = Equipment.Deserialize(data.equipmentData);
                if (equipment != null)
                {
                    result[data.type] = equipment;
                }
            }

            return result;
        }
    }
}
