using UnityEngine;
public class ResourceCluster
{
    public Vector2 center;
    public float radius;
    public int resourceType; // 0 = tree, 1 = mountain
    public float density;

    public ResourceCluster(Vector2 center, float radius, int type, float density)
    {
        this.center = center;
        this.radius = radius;
        this.resourceType = type;
        this.density = density;
    }

    public float GetInfluence(float x, float z)
    {
        float dx = x - center.x;
        float dz = z - center.y;
        float dist = Mathf.Sqrt(dx * dx + dz * dz);

        if (dist > radius) return 0f;

        float normalizedDist = dist / radius;
        return (1f - normalizedDist) * density;
    }
}