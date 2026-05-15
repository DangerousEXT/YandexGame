using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YG;

public class EquipmentUIManager : MonoBehaviour
{
    private Equipment equipment;
    [SerializeField] private TextMeshProUGUI name;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private Image equipmentImage;

    public Equipment Equipment => equipment;

    public void OnEnable()
    {
        YG2.onSwitchLang += ChangeLanguage;
    }

    public void OnDisable()
    {
        YG2.onSwitchLang -= ChangeLanguage;
    }

    public void NewEquipment(Equipment equipment)
    {
        this.equipment = equipment;
        name.text = equipment.Name;
        description.text = string.Join("\n", equipment.Buffs.Select(buff => buff.Description));
        equipmentImage.sprite = equipment.Icon;
        equipmentImage.preserveAspect = true;
    }

    private void ChangeLanguage(string language)
    {
        NewEquipment(equipment);
    }

}