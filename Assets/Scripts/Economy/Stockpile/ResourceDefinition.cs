using UnityEngine;


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