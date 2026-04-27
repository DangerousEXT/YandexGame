using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;

[Serializable]
public class Equipment
{
    [SerializeField] private string name;
    [SerializeField] private string iconId;
    [SerializeField] private EquipmentType type;
    [SerializeField] private List<Buff> buffs = new();
    public string Name 
    { 
        get { return name; } 
        set { name = value; } 
    }
    public Sprite Icon 
    {
        get { return SpritesBase.GetSprite(iconId); }
        set { iconId = SpritesBase.GetId(value); }
    }
    public EquipmentType Type 
    { 
        get { return type; }
        set { type = value; }
    }
    public List<Buff> Buffs 
    { 
        get { return buffs; } 
        set { buffs = value; }
    }
    public int Cost => Buffs.Count * 15;
    public void ApplyToPlayer(Entity playerEntity)
    {
        foreach (var buff in Buffs)
        {
            buff.Apply(playerEntity);
        }
    }
}