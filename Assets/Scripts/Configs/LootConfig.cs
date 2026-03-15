using UnityEngine;

[CreateAssetMenu(fileName = "LootConfig", menuName = "Exotic Harvest/LootConfig")]
public class LootConfig : ScriptableObject
{
    public string DisplayName;
    public Sprite Icon;
    public Color UiColor = Color.white;
    
    public EResourceType ResourceType;
    public int Quantity = 1;
}