using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class EquipmentToSaveData
{
    public string name;
    public string iconId;
    public EquipmentType type;
    public List<BuffToSaveData> buffsData = new();
}
