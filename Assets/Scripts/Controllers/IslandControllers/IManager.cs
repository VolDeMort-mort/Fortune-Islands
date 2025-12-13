using Unity;
using UnityEngine;
public class IManager: MonoBehaviour
{
    protected IslandController island;
    public virtual void Initialize(IslandController controller)
    {
        island = controller;
    }
}