using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class Managers : MonoBehaviour
{
    public static Managers Instance { get; private set; }

    private readonly List<IManager> managerList = new();

    [Header("Debug (Read Only)")]
    [SerializeField] private List<MonoBehaviour> registeredManagersDebug = new();

    public IReadOnlyList<IManager> GetRegisteredManagers() => managerList;

    public void RegisterManager(IManager manager)
    {
        managerList.Add(manager);
        Debug.Log($"Managers: Registered {manager.Name}");
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
