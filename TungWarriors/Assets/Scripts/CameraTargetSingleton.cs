using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class CameraTargetSingleton : MonoBehaviour
{
    public static CameraTargetSingleton Instance;

    public void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("Multiple instances of CameraTargetSingleton detected. Destroying duplicate.", Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
}
