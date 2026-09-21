using UnityEngine;
public class BaseManager: MonoBehaviour
{
    protected IslandController island;
    public virtual void Initialize(IslandController controller)
    {
        island = controller;
    }
}