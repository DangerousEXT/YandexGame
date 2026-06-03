using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class CellPrefabData : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Button button;
    [SerializeField] private Equipment equipment;
    [SerializeField] private Sprite defaultIcon;
    public Image Image
    {
        get => image;
        set => image = value;
    }

    public Button Button
    {
        get => button;
    }

    public Equipment Equipment
    {
        get => equipment;
        set => equipment = value;
    }

    public Sprite DefaultIcon
    {
        get => defaultIcon;
        set => defaultIcon = value;
    }
}
