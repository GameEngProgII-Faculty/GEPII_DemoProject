using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Item")]
public class Item : ScriptableObject
{
    [Header("Only gameplay")]
    public ItemType type;
    public ActionType actionType;
    // public Vector2Int range = new Vector2Int(5, 4);

    [Header("Only UI")]
    public bool stackable = true;
    [TextArea] public string description;

    [Header("Both")]
    public Sprite icon;
    public GameObject mesh;
}

public enum ItemType
{
    Tool, Resource, Consumable
}

public enum ActionType
{
    Attack,
    Mine,
    Heal
}
