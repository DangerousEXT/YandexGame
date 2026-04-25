using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentUIManager : MonoBehaviour
{
    private Equipment equipment;
    [SerializeField] private TextMeshProUGUI name;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private Image equipmentImage;

    public Equipment Equipment => equipment;

    public Equipment NewEquipment(Equipment equipment)
    {
        this.equipment = equipment;
        name.text = equipment.Name;
        description.text = string.Join("\n", equipment.Buffs.Select(buff => buff.Description));
        equipmentImage.sprite = equipment.Icon;
        equipmentImage.preserveAspect = true;
        return equipment;
    }
}