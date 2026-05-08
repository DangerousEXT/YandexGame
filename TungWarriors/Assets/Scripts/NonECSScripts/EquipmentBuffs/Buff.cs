using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Scripting;
using static UnityEngine.Rendering.DebugUI;

[Preserve]
[Serializable]
public abstract class Buff
{
    [SerializeField] private float _value;

    public float Value
    {
        get => (float)Math.Round(_value, 2);
        set => _value = value;
    }
    public abstract EquipmentType[] Type { get; }
    public abstract float MinValue { get; }
    public abstract float MaxValue { get; }
    public abstract string Description { get;}

    public abstract void Apply(Entity playerEntity);

    public virtual BuffToSaveData Serialize()
    {
        return new()
        {
            typeName = this.GetType().AssemblyQualifiedName,
            value = _value
        };
    }

    public static Buff Deserialize(BuffToSaveData save)
    {
        var buffType = System.Type.GetType(save.typeName);

        if (buffType == null) return null;

        var buff = (Buff)Activator.CreateInstance(buffType);
        buff.Value = save.value;
        return buff;
    }
}
