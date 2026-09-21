using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceItemUI : MonoBehaviour
{


    [Header("UI References")]
    public Image iconImage;
    public TMP_Text amountText;

    private ResourceManager _manager;
    private ResourceType _myType;
    private ResourceDisplayStyle _currentStyle; // Store the style for this specific item

    // Update Setup to accept the style
    public void Setup(ResourceManager manager, ResourceDefinition definition, ResourceDisplayStyle style)
    {
        _manager = manager;
        _myType = definition.type;
        _currentStyle = style; // Save it

        if (iconImage != null) iconImage.sprite = definition.icon;
        
        UpdateText();
        _manager.OnResourceChanged += HandleResourceChange;
    }

    private void OnDestroy()
    {
        if (_manager != null) _manager.OnResourceChanged -= HandleResourceChange;
    }

    private void HandleResourceChange(ResourceType changedType)
    {
        if (changedType == _myType) UpdateText();
    }

    private void UpdateText()
    {
        if (amountText == null) return;

        int currentAmount = _manager.GetAmount(_myType);

        // Check which style we are using
        if (_currentStyle == ResourceDisplayStyle.NoCapacity)
        {
            // Format: "10"
            amountText.text = currentAmount.ToString();
        }
        else if (_currentStyle == ResourceDisplayStyle.WithCapacity)
        {
            // Format: "10/15"
            // You can get max storage from manager or hardcode it
            int max = 15; 
            amountText.text = $"{currentAmount}/{max}";
        }
    }
}