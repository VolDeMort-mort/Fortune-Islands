using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct DiceFace
{
    public ResourceType type; // e.g. Gold, Food
    public int amount;        // e.g. 1, 2, 4
    // public Sprite faceIcon;   // Visual for UI
}

[CreateAssetMenu(fileName = "NewDice", menuName = "Game/Dice Definition")]
public class Dice : ScriptableObject
{
    public string diceName; // e.g. "Economy Die"
    public Color diceColor = Color.white;


    // A standard die has 6 faces
    [Tooltip("Define exactly 6 faces for a standard die")]
    public List<DiceFace> faces = new List<DiceFace>();

    public DiceFace Roll()
    {
        // Pick a random face
        return faces[Random.Range(0, faces.Count)];
    }
}