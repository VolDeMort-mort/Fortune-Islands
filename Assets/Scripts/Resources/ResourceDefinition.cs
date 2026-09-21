using TMPro;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Resources/Resource Definition")]
public class ResourceDefinition : ScriptableObject
{
    public ResourceType type;
    public Sprite icon;
    public ResourceDisplayStyle style;
}