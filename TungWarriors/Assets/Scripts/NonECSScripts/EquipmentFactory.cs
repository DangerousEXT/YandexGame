using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class EquipmentFactory 
{
    private float distributionPower = 2f;
    private int minCountAmplifications;
    private int maxCountAmplifications;
    private List<ItemData> AvailableItems;
    private List<Buff> AvailableBuffs;

    public EquipmentFactory(List<ItemData> availableItems, int minBuffs = 1, int maxBuffs = 4)
    {
        AvailableItems = availableItems;
        minCountAmplifications = minBuffs;
        maxCountAmplifications = maxBuffs;

        if (AvailableBuffs == null)
        {
            AvailableBuffs = new List<Buff>();
            var buffTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(Buff)));

            foreach (var type in buffTypes)
            {
                var buff = (Buff)Activator.CreateInstance(type);
                AvailableBuffs.Add(buff);
            }
        }
    }

    public Equipment CreateRandomEquipment()
    {
        var equipmentsList = Enum.GetNames(typeof(EquipmentType));
        var randomType = (EquipmentType)UnityEngine.Random.Range(0, equipmentsList.Count());
        return CreateCertainEquipment(randomType);
    }

    public Equipment CreateCertainEquipment(EquipmentType type)
    {
        var item = GetRandomItem(type);
        var buffsCount = Mathf.FloorToInt(GetRandomCount(minCountAmplifications, maxCountAmplifications));
        var buffs = GetRandomBuffs(type, buffsCount);
        return new Equipment
        {
            Name = item.name,
            Icon = item.img,
            Type = type,
            Buffs = buffs
        };
}

    private float GetRandomCount(float minValue, float maxValue)
    {
        var range = maxValue - minValue;

        var roll = UnityEngine.Random.value;

        var normalized = Mathf.Pow(roll, distributionPower);

        return minValue + normalized * range;
    }

    private ItemData GetRandomItem(EquipmentType type)
    {
        var items = AvailableItems.Where(item => item.type == type).ToArray();
        return items[UnityEngine.Random.Range(0, items.Count())];
    }

    private List<Buff> GetRandomBuffs(EquipmentType type, int count)
    {
        var bufs = AvailableBuffs.Where(buff => buff.Type.Any(Type => Type == type)).ToArray();
        var result = new List<Buff>();

        if(bufs.Count() == 0)
            return result;

        for(var i = 0; i < count; i++)
        {
            var buff = bufs[UnityEngine.Random.Range(0, bufs.Count())];
            var buffCopy = (Buff)Activator.CreateInstance(buff.GetType());
            buffCopy.Value = GetRandomCount(buffCopy.MinValue, buffCopy.MaxValue);
            result.Add(buffCopy);
        }
        return result;
    }
}

