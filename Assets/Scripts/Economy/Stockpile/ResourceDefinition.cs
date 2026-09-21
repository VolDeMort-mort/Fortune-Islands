using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace FortuneIslands.Economy.Stockpile
{

    [CreateAssetMenu(menuName = "Resources/Resource Definition")]
    public class ResourceDefinition : ScriptableObject
    {
        public ResourceType type;
        public Sprite icon;
        public ResourceDisplayStyle style;
    }
}