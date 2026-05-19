using UnityEngine;

public class SkillTreePanelManager : MonoBehaviour
{
    private void Awake()
    {
        if (GetComponent<MetaProgressionPanelManager>() == null)
        {
            gameObject.AddComponent<MetaProgressionPanelManager>();
        }
    }
}
